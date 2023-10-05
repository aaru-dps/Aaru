// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads d2f disk images.
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
// Copyright © 2018-2023 Michael Drüing
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class WCDiskImage
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        string comments = string.Empty;
        Stream stream   = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        var header = new byte[32];
        stream.EnsureRead(header, 0, 32);

        FileHeader fheader = Marshal.ByteArrayToStructureLittleEndian<FileHeader>(header);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   Localization.Detected_WC_DISK_IMAGE_with_0_heads_1_tracks_and_2_sectors_per_track,
                                   fheader.heads, fheader.cylinders, fheader.sectorsPerTrack);

        _imageInfo.Cylinders       = fheader.cylinders;
        _imageInfo.SectorsPerTrack = fheader.sectorsPerTrack;
        _imageInfo.SectorSize      = 512; // only 512 bytes per sector supported
        _imageInfo.Heads           = fheader.heads;
        _imageInfo.Sectors         = _imageInfo.Heads   * _imageInfo.Cylinders * _imageInfo.SectorsPerTrack;
        _imageInfo.ImageSize       = _imageInfo.Sectors * _imageInfo.SectorSize;

        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);

        _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                      (ushort)_imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                      false));

        /* buffer the entire disk in memory */
        for(var cyl = 0; cyl < _imageInfo.Cylinders; cyl++)
        {
            for(var head = 0; head < _imageInfo.Heads; head++)
            {
                ErrorNumber errno = ReadTrack(stream, cyl, head);

                if(errno != ErrorNumber.NoError)
                    return errno;
            }
        }

        /* if there are extra tracks, read them as well */
        if(fheader.extraTracks[0] == 1)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Extra_track_1_head_0_present_reading);
            ReadTrack(stream, (int)_imageInfo.Cylinders, 0);
        }

        if(fheader.extraTracks[1] == 1)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Extra_track_1_head_1_present_reading);
            ReadTrack(stream, (int)_imageInfo.Cylinders, 1);
        }

        if(fheader.extraTracks[2] == 1)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Extra_track_2_head_0_present_reading);
            ReadTrack(stream, (int)_imageInfo.Cylinders + 1, 0);
        }

        if(fheader.extraTracks[3] == 1)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Extra_track_2_head_1_present_reading);
            ReadTrack(stream, (int)_imageInfo.Cylinders + 1, 1);
        }

        /* adjust number of cylinders */
        if(fheader.extraTracks[0] == 1 || fheader.extraTracks[1] == 1)
            _imageInfo.Cylinders++;

        if(fheader.extraTracks[2] == 1 || fheader.extraTracks[3] == 1)
            _imageInfo.Cylinders++;

        /* read the comment and directory data if present */
        if(fheader.extraFlags.HasFlag(ExtraFlag.Comment))
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Comment_present_reading);
            var sheaderBuffer = new byte[6];
            stream.EnsureRead(sheaderBuffer, 0, 6);

            SectorHeader sheader = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(sheaderBuffer);

            if(sheader.flag != SectorFlag.Comment)
            {
                AaruConsole.ErrorWriteLine(string.Format(Localization.Invalid_sector_type_0_encountered,
                                                         sheader.flag.ToString()));

                return ErrorNumber.InvalidArgument;
            }

            var comm = new byte[sheader.crc];
            stream.EnsureRead(comm, 0, sheader.crc);
            comments += Encoding.ASCII.GetString(comm) + Environment.NewLine;
        }

        if(fheader.extraFlags.HasFlag(ExtraFlag.Directory))
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Directory_listing_present_reading);
            var sheaderBuffer = new byte[6];
            stream.EnsureRead(sheaderBuffer, 0, 6);

            SectorHeader sheader = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(sheaderBuffer);

            if(sheader.flag != SectorFlag.Directory)
            {
                AaruConsole.ErrorWriteLine(string.Format(Localization.Invalid_sector_type_0_encountered,
                                                         sheader.flag.ToString()));

                return ErrorNumber.InvalidArgument;
            }

            var dir = new byte[sheader.crc];
            stream.EnsureRead(dir, 0, sheader.crc);
            comments += Encoding.ASCII.GetString(dir);
        }

        if(comments.Length > 0)
            _imageInfo.Comments = comments;

        // save some variables for later use
        _fileHeader    = fheader;
        _wcImageFilter = imageFilter;

        return ErrorNumber.InvalidArgument;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        int sectorNumber   = (int)(sectorAddress % _imageInfo.SectorsPerTrack) + 1;
        var trackNumber    = (int)(sectorAddress / _imageInfo.SectorsPerTrack);
        int headNumber     = _imageInfo.Heads > 1 ? trackNumber % 2 : 0;
        int cylinderNumber = _imageInfo.Heads > 1 ? trackNumber / 2 : trackNumber;

        if(_badSectors[(cylinderNumber, headNumber, sectorNumber)])
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.reading_bad_sector_0_1_2_3, sectorAddress,
                                       cylinderNumber, headNumber, sectorNumber);

            /* if we have sector data, return that */
            if(_sectorCache.ContainsKey((cylinderNumber, headNumber, sectorNumber)))
            {
                buffer = _sectorCache[(cylinderNumber, headNumber, sectorNumber)];

                return ErrorNumber.NoError;
            }

            /* otherwise, return an empty sector */
            buffer = new byte[512];

            return ErrorNumber.NoError;
        }

        buffer = _sectorCache[(cylinderNumber, headNumber, sectorNumber)];

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion

    /// <summary>Read a whole track and cache it</summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="cyl">The cylinder number of the track being read.</param>
    /// <param name="head">The head number of the track being read.</param>
    ErrorNumber ReadTrack(Stream stream, int cyl, int head)
    {
        for(var sect = 1; sect < _imageInfo.SectorsPerTrack + 1; sect++)
        {
            /* read the sector header */
            var sheaderBuffer = new byte[6];
            stream.EnsureRead(sheaderBuffer, 0, 6);

            SectorHeader sheader = Marshal.ByteArrayToStructureLittleEndian<SectorHeader>(sheaderBuffer);

            /* validate the sector header */
            if(sheader.cylinder != cyl || sheader.head != head || sheader.sector != sect)
            {
                AaruConsole.
                    ErrorWriteLine(string.Format(Localization.Unexpected_sector_encountered_Found_CHS_0_1_2_but_expected_3_4_5,
                                                 sheader.cylinder, sheader.head, sheader.sector, cyl, head, sect));

                return ErrorNumber.InvalidArgument;
            }

            var sectorData = new byte[512];

            /* read the sector data */
            switch(sheader.flag)
            {
                case SectorFlag.Normal: /* read a normal sector and store it in cache */
                    stream.EnsureRead(sectorData, 0, 512);
                    _sectorCache[(cyl, head, sect)] = sectorData;
                    CRC16IbmContext.Data(sectorData, 512, out byte[] crc);
                    var calculatedCRC = (short)(256 * crc[0] | crc[1]);
                    /*
                    AaruConsole.DebugWriteLine(MODULE_NAME, "CHS {0},{1},{2}: Regular sector, stored CRC=0x{3:x4}, calculated CRC=0x{4:x4}",
                        cyl, head, sect, sheader.crc, 256 * crc[0] + crc[1]);
                     */
                    _badSectors[(cyl, head, sect)] = sheader.crc != calculatedCRC;

                    if(calculatedCRC != sheader.crc)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.
                                                       CHS_0_1_2_CRC_mismatch_stored_CRC_3_X4_calculated_CRC_4_X4, cyl,
                                                   head, sect, sheader.crc, calculatedCRC);
                    }

                    break;
                case SectorFlag.BadSector:
                    /*
                    AaruConsole.DebugWriteLine(MODULE_NAME, "CHS {0},{1},{2}: Bad sector",
                        cyl, head, sect);
                     */
                    _badSectors[(cyl, head, sect)] = true;

                    break;
                case SectorFlag.RepeatByte:
                    /*
                    AaruConsole.DebugWriteLine(MODULE_NAME, "CHS {0},{1},{2}: RepeatByte sector, fill byte 0x{0:x2}",
                        cyl, head, sect, sheader.crc & 0xff);
                     */
                    for(var i = 0; i < 512; i++)
                        sectorData[i] = (byte)(sheader.crc & 0xff);

                    _sectorCache[(cyl, head, sect)] = sectorData;
                    _badSectors[(cyl, head, sect)]  = false;

                    break;
                default:
                    AaruConsole.ErrorWriteLine(string.Format(Localization.Invalid_sector_type_0_encountered,
                                                             sheader.flag.ToString()));

                    return ErrorNumber.InvalidArgument;
            }
        }

        return ErrorNumber.NoError;
    }
}