// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads CPCEMU disk images.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Decoders.Floppy;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class Cpcdsk
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512) return ErrorNumber.InvalidArgument;

        var headerB = new byte[256];
        stream.EnsureRead(headerB, 0, 256);

        int pos;

        for(pos = 0; pos < 254; pos++)
        {
            if(headerB[pos] == 0x0D && headerB[pos + 1] == 0x0A) break;
        }

        if(pos >= 254) return ErrorNumber.InvalidArgument;

        string magic = Encoding.ASCII.GetString(headerB, 0, pos);

        stream.Position = pos + 2;
        stream.EnsureRead(headerB, 0, 256);

        DiskInfo header = Marshal.ByteArrayToStructureLittleEndian<DiskInfo>(headerB);

        if(string.Compare(CPCDSK_ID, magic, StringComparison.InvariantCultureIgnoreCase) != 0 &&
           string.Compare(EDSK_ID,   magic, StringComparison.InvariantCultureIgnoreCase) != 0 &&
           string.Compare(DU54_ID,   magic, StringComparison.InvariantCultureIgnoreCase) != 0)
            return ErrorNumber.InvalidArgument;

        _extended = string.Compare(EDSK_ID, magic, StringComparison.InvariantCultureIgnoreCase) == 0;
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Extended_equals_0, _extended);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.magic_equals_0_quoted, magic);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.magic = \"{0}\"", StringHandlers.CToString(header.magic));

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.creator = \"{0}\"", StringHandlers.CToString(header.creator));

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.tracks = {0}", header.tracks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.sides = {0}",  header.sides);

        if(!_extended)
            AaruConsole.DebugWriteLine(MODULE_NAME, "header.tracksize = {0}", header.tracksize);
        else
        {
            for(var i = 0; i < header.tracks; i++)
            {
                for(var j = 0; j < header.sides; j++)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Track_0_Side_1_size_equals_2,
                                               i,
                                               j,
                                               header.tracksizeTable[i * header.sides + j] * 256);
                }
            }
        }

        ulong currentSector = 0;
        _sectors      = new Dictionary<ulong, byte[]>();
        _addressMarks = new Dictionary<ulong, byte[]>();
        ulong readtracks        = 0;
        var   allTracksSameSize = true;
        ulong sectorsPerTrack   = 0;

        // Seek to first track descriptor
        stream.Seek(256, SeekOrigin.Begin);

        for(var i = 0; i < header.tracks; i++)
        {
            for(var j = 0; j < header.sides; j++)
            {
                // Track not stored in image
                if(_extended && header.tracksizeTable[i * header.sides + j] == 0) continue;

                long trackPos = stream.Position;

                var trackB = new byte[256];
                stream.EnsureRead(trackB, 0, 256);
                TrackInfo trackInfo = Marshal.ByteArrayToStructureLittleEndian<TrackInfo>(trackB);

                if(string.Compare(TRACK_ID,
                                  Encoding.ASCII.GetString(trackInfo.magic),
                                  StringComparison.InvariantCultureIgnoreCase) !=
                   0)
                {
                    AaruConsole.ErrorWriteLine(Localization.Not_the_expected_track_info);

                    return ErrorNumber.InvalidArgument;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "trackInfo[{1}:{2}].magic = \"{0}\"",
                                           StringHandlers.CToString(trackInfo.magic),
                                           i,
                                           j);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "trackInfo[{1}:{2}].bps = {0}",
                                           SizeCodeToBytes(trackInfo.bps),
                                           i,
                                           j);

                AaruConsole.DebugWriteLine(MODULE_NAME, "trackInfo[{1}:{2}].dataRate = {0}", trackInfo.dataRate, i, j);

                AaruConsole.DebugWriteLine(MODULE_NAME, "trackInfo[{1}:{2}].filler = 0x{0:X2}", trackInfo.filler, i, j);

                AaruConsole.DebugWriteLine(MODULE_NAME, "trackInfo[{1}:{2}].gap3 = 0x{0:X2}", trackInfo.gap3, i, j);

                AaruConsole.DebugWriteLine(MODULE_NAME, "trackInfo[{1}:{2}].padding = {0}", trackInfo.padding, i, j);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "trackInfo[{1}:{2}].recordingMode = {0}",
                                           trackInfo.recordingMode,
                                           i,
                                           j);

                AaruConsole.DebugWriteLine(MODULE_NAME, "trackInfo[{1}:{2}].sectors = {0}", trackInfo.sectors, i, j);

                AaruConsole.DebugWriteLine(MODULE_NAME, "trackInfo[{1}:{2}].side = {0}", trackInfo.side, i, j);

                AaruConsole.DebugWriteLine(MODULE_NAME, "trackInfo[{1}:{2}].track = {0}", trackInfo.track, i, j);

                if(trackInfo.sectors != sectorsPerTrack)
                {
                    if(sectorsPerTrack == 0)
                        sectorsPerTrack = trackInfo.sectors;
                    else
                        allTracksSameSize = false;
                }

                Dictionary<int, byte[]> thisTrackSectors      = new();
                Dictionary<int, byte[]> thisTrackAddressMarks = new();

                for(var k = 1; k <= trackInfo.sectors; k++)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "trackInfo[{1}:{2}].sector[{3}].id = 0x{0:X2}",
                                               trackInfo.sectorsInfo[k - 1].id,
                                               i,
                                               j,
                                               k);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "trackInfo[{1}:{2}].sector[{3}].len = {0}",
                                               trackInfo.sectorsInfo[k - 1].len,
                                               i,
                                               j,
                                               k);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "trackInfo[{1}:{2}].sector[{3}].side = {0}",
                                               trackInfo.sectorsInfo[k - 1].side,
                                               i,
                                               j,
                                               k);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "trackInfo[{1}:{2}].sector[{3}].size = {0}",
                                               SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size),
                                               i,
                                               j,
                                               k);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "trackInfo[{1}:{2}].sector[{3}].st1 = 0x{0:X2}",
                                               trackInfo.sectorsInfo[k - 1].st1,
                                               i,
                                               j,
                                               k);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "trackInfo[{1}:{2}].sector[{3}].st2 = 0x{0:X2}",
                                               trackInfo.sectorsInfo[k - 1].st2,
                                               i,
                                               j,
                                               k);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "trackInfo[{1}:{2}].sector[{3}].track = {0}",
                                               trackInfo.sectorsInfo[k - 1].track,
                                               i,
                                               j,
                                               k);

                    int sectLen = _extended
                                      ? trackInfo.sectorsInfo[k - 1].len
                                      : SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size);

                    var sector = new byte[sectLen];
                    stream.EnsureRead(sector, 0, sectLen);

                    if(sectLen < SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size))
                    {
                        var temp = new byte[SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size)];
                        Array.Copy(sector, 0, temp, 0, sector.Length);
                        sector = temp;
                    }
                    else if(sectLen > SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size))
                    {
                        var temp = new byte[SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size)];
                        Array.Copy(sector, 0, temp, 0, temp.Length);
                        sector = temp;
                    }

                    thisTrackSectors[(trackInfo.sectorsInfo[k - 1].id & 0x3F) - 1] = sector;

                    var amForCrc = new byte[8];
                    amForCrc[0] = 0xA1;
                    amForCrc[1] = 0xA1;
                    amForCrc[2] = 0xA1;
                    amForCrc[3] = (byte)IBMIdType.AddressMark;
                    amForCrc[4] = trackInfo.sectorsInfo[k - 1].track;
                    amForCrc[5] = trackInfo.sectorsInfo[k - 1].side;
                    amForCrc[6] = trackInfo.sectorsInfo[k - 1].id;
                    amForCrc[7] = (byte)trackInfo.sectorsInfo[k - 1].size;

                    CRC16IbmContext.Data(amForCrc, 8, out byte[] amCrc);

                    var addressMark = new byte[22];
                    Array.Copy(amForCrc, 0, addressMark, 12, 8);
                    Array.Copy(amCrc,    0, addressMark, 20, 2);

                    thisTrackAddressMarks[(trackInfo.sectorsInfo[k - 1].id & 0x3F) - 1] = addressMark;
                }

                foreach(KeyValuePair<int, byte[]> s in thisTrackSectors.OrderBy(k => k.Key))
                {
                    _sectors.Add(currentSector, s.Value);
                    _addressMarks.Add(currentSector, s.Value);
                    currentSector++;

                    if(s.Value.Length > _imageInfo.SectorSize) _imageInfo.SectorSize = (uint)s.Value.Length;
                }

                stream.Seek(trackPos, SeekOrigin.Begin);

                if(_extended)
                {
                    stream.Seek(header.tracksizeTable[i * header.sides + j] * 256, SeekOrigin.Current);
                    _imageInfo.ImageSize += (ulong)(header.tracksizeTable[i * header.sides + j] * 256) - 256;
                }
                else
                {
                    stream.Seek(header.tracksize, SeekOrigin.Current);
                    _imageInfo.ImageSize += (ulong)header.tracksize - 256;
                }

                readtracks++;
            }
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Read_0_sectors,             _sectors.Count);
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Read_0_tracks,              readtracks);
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.All_tracks_are_same_size_0, allTracksSameSize);

        _imageInfo.Application          = StringHandlers.CToString(header.creator);
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = (ulong)_sectors.Count;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.CompactFloppy;
        _imageInfo.ReadableSectorTags.Add(SectorTagType.FloppyAddressMark);

        _imageInfo.Cylinders       = header.tracks;
        _imageInfo.Heads           = header.sides;
        _imageInfo.SectorsPerTrack = (uint)(_imageInfo.Sectors / (_imageInfo.Cylinders * _imageInfo.Heads));

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) =>
        _sectors.TryGetValue(sectorAddress, out buffer) ? ErrorNumber.NoError : ErrorNumber.SectorNotFound;

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(tag != SectorTagType.FloppyAddressMark) return ErrorNumber.NotSupported;

        return _addressMarks.TryGetValue(sectorAddress, out buffer) ? ErrorNumber.NoError : ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(tag != SectorTagType.FloppyAddressMark) return ErrorNumber.NotSupported;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] addressMark);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(addressMark, 0, addressMark.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}