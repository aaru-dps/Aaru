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
//     Reads SuperCardPro flux images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class SuperCardPro
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Header     = new ScpHeader();
        _scpStream = imageFilter.GetDataForkStream();
        _scpStream.Seek(0, SeekOrigin.Begin);

        _scpFilter = imageFilter;

        if(_scpStream.Length < Marshal.SizeOf<ScpHeader>())
            return ErrorNumber.InvalidArgument;

        byte[] hdr = new byte[Marshal.SizeOf<ScpHeader>()];
        _scpStream.EnsureRead(hdr, 0, Marshal.SizeOf<ScpHeader>());

        Header = Marshal.ByteArrayToStructureLittleEndian<ScpHeader>(hdr);

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.signature = \"{0}\"",
                                   StringHandlers.CToString(Header.signature));

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.version = {0}.{1}", (Header.version & 0xF0) >> 4,
                                   Header.version & 0xF);

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.type = {0}", Header.type);
        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.revolutions = {0}", Header.revolutions);
        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.start = {0}", Header.start);
        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.end = {0}", Header.end);
        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.bitCellEncoding = {0}", Header.bitCellEncoding);
        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.heads = {0}", Header.heads);
        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.resolution = {0}", Header.resolution);
        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.checksum = 0x{0:X8}", Header.checksum);

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.flags.StartsAtIndex = {0}",
                                   Header.flags == ScpFlags.StartsAtIndex);

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.flags.Tpi = {0}",
                                   Header.flags == ScpFlags.Tpi ? "96tpi" : "48tpi");

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.flags.Rpm = {0}",
                                   Header.flags == ScpFlags.Rpm ? "360rpm" : "300rpm");

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.flags.Normalized = {0}",
                                   Header.flags == ScpFlags.Normalized);

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.flags.Writable = {0}",
                                   Header.flags == ScpFlags.Writable);

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.flags.HasFooter = {0}",
                                   Header.flags == ScpFlags.HasFooter);

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.flags.NotFloppy = {0}",
                                   Header.flags == ScpFlags.NotFloppy);

        AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.flags.CreatedByOtherDevice = {0}",
                                   Header.flags == ScpFlags.CreatedByOtherDevice);

        if(!_scpSignature.SequenceEqual(Header.signature))
            return ErrorNumber.InvalidArgument;

        ScpTracks = new Dictionary<byte, TrackHeader>();

        for(byte t = Header.start; t <= Header.end; t++)
        {
            if(t >= Header.offsets.Length)
                break;

            _scpStream.Position = Header.offsets[t];

            var trk = new TrackHeader
            {
                Signature = new byte[3],
                Entries   = new TrackEntry[Header.revolutions]
            };

            _scpStream.EnsureRead(trk.Signature, 0, trk.Signature.Length);
            trk.TrackNumber = (byte)_scpStream.ReadByte();

            if(!trk.Signature.SequenceEqual(_trkSignature))
            {
                AaruConsole.DebugWriteLine("SuperCardPro plugin",
                                           Localization.Track_header_at_0_contains_incorrect_signature,
                                           Header.offsets[t]);

                continue;
            }

            if(trk.TrackNumber != t)
            {
                AaruConsole.DebugWriteLine("SuperCardPro plugin", Localization.Track_number_at_0_should_be_1_but_is_2,
                                           Header.offsets[t], t, trk.TrackNumber);

                continue;
            }

            AaruConsole.DebugWriteLine("SuperCardPro plugin", Localization.Found_track_0_at_1, t, Header.offsets[t]);

            for(byte r = 0; r < Header.revolutions; r++)
            {
                byte[] rev = new byte[Marshal.SizeOf<TrackEntry>()];
                _scpStream.EnsureRead(rev, 0, Marshal.SizeOf<TrackEntry>());

                trk.Entries[r] = Marshal.ByteArrayToStructureLittleEndian<TrackEntry>(rev);

                // De-relative offsets
                trk.Entries[r].dataOffset += Header.offsets[t];
            }

            ScpTracks.Add(t, trk);
        }

        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

        switch(Header.type)
        {
            case ScpDiskType.Commodore64:
                _imageInfo.MediaType = MediaType.CBM_1540;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 40);
                _imageInfo.Heads     = 1;

                break;
            case ScpDiskType.CommodoreAmiga:
                _imageInfo.MediaType = MediaType.CBM_AMIGA_35_DD;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.CommodoreAmigaHD:
                _imageInfo.MediaType = MediaType.CBM_AMIGA_35_HD;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.AtariFMSS:
                _imageInfo.MediaType = MediaType.ATARI_525_SD;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 40);
                _imageInfo.Heads     = 1;

                break;
            case ScpDiskType.AtariFMDS:
                _imageInfo.MediaType = MediaType.Unknown;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 40);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.AtariFMEx: break;
            case ScpDiskType.AtariSTSS:
                _imageInfo.MediaType = MediaType.ATARI_35_SS_DD;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads     = 1;

                break;
            case ScpDiskType.AtariSTDS:
                _imageInfo.MediaType = MediaType.ATARI_35_DS_DD;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.AppleII:
                _imageInfo.MediaType = MediaType.Apple32DS;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 40);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.AppleIIPro: break;
            case ScpDiskType.Apple400K:
                _imageInfo.MediaType       = MediaType.AppleSonySS;
                _imageInfo.Cylinders       = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 10;

                break;
            case ScpDiskType.Apple800K:
                _imageInfo.MediaType       = MediaType.AppleSonyDS;
                _imageInfo.Cylinders       = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 10;

                break;
            case ScpDiskType.Apple144:
                _imageInfo.MediaType       = MediaType.DOS_525_HD;
                _imageInfo.Cylinders       = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 18;

                break;
            case ScpDiskType.PC360K:
                _imageInfo.MediaType       = MediaType.DOS_525_DS_DD_9;
                _imageInfo.Cylinders       = (uint)int.Max(((Header.end + 1) / 2), 40);
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 9;

                break;
            case ScpDiskType.PC720K:
                _imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.PC12M:
                _imageInfo.MediaType = MediaType.DOS_525_HD;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.PC144M:
                _imageInfo.MediaType       = MediaType.DOS_35_HD;
                _imageInfo.Cylinders       = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 18;

                break;
            case ScpDiskType.TandySSSD:  break;
            case ScpDiskType.TandySSDD:  break;
            case ScpDiskType.TandyDSSD:  break;
            case ScpDiskType.TandyDSDD:  break;
            case ScpDiskType.Ti994A:     break;
            case ScpDiskType.RolandD20:  break;
            case ScpDiskType.AmstradCPC: break;
            case ScpDiskType.Generic360K:
                _imageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 40);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.Generic12M:
                _imageInfo.MediaType = MediaType.DOS_525_HD;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads     = 2;

                break;

            case ScpDiskType.Generic720K:
                _imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.Generic144M:
                _imageInfo.MediaType = MediaType.DOS_35_HD;
                _imageInfo.Cylinders = (uint)int.Max(((Header.end + 1) / 2), 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.TapeGCR1:
            case ScpDiskType.TapeGCR2:
            case ScpDiskType.TapeMFM:
                _imageInfo.MediaType = MediaType.UnknownTape;

                break;
            case ScpDiskType.HddMFM:
            case ScpDiskType.HddRLL:
                _imageInfo.MediaType = MediaType.GENERIC_HDD;

                break;
            default:
                _imageInfo.MediaType = MediaType.Unknown;

                _imageInfo.Cylinders =
                    (uint)int.Max((Header.end + 1) / 2, Header.flags.HasFlag(ScpFlags.Tpi) ? 80 : 40);

                _imageInfo.Heads = Header.heads == 0 ? 2 : (uint)1;

                break;
        }

        if(Header.flags.HasFlag(ScpFlags.HasFooter))
        {
            long position = _scpStream.Position;
            _scpStream.Seek(-4, SeekOrigin.End);

            while(_scpStream.Position >= position)
            {
                byte[] footerSig = new byte[4];
                _scpStream.EnsureRead(footerSig, 0, 4);
                uint footerMagic = BitConverter.ToUInt32(footerSig, 0);

                if(footerMagic == FOOTER_SIGNATURE)
                {
                    _scpStream.Seek(-Marshal.SizeOf<Footer>(), SeekOrigin.Current);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", Localization.Found_footer_at_0,
                                               _scpStream.Position);

                    byte[] ftr = new byte[Marshal.SizeOf<Footer>()];
                    _scpStream.EnsureRead(ftr, 0, Marshal.SizeOf<Footer>());

                    Footer footer = Marshal.ByteArrayToStructureLittleEndian<Footer>(ftr);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.manufacturerOffset = 0x{0:X8}",
                                               footer.manufacturerOffset);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.modelOffset = 0x{0:X8}",
                                               footer.modelOffset);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.serialOffset = 0x{0:X8}",
                                               footer.serialOffset);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.creatorOffset = 0x{0:X8}",
                                               footer.creatorOffset);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.applicationOffset = 0x{0:X8}",
                                               footer.applicationOffset);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.commentsOffset = 0x{0:X8}",
                                               footer.commentsOffset);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.creationTime = {0}", footer.creationTime);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.modificationTime = {0}",
                                               footer.modificationTime);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.applicationVersion = {0}.{1}",
                                               (footer.applicationVersion & 0xF0) >> 4,
                                               footer.applicationVersion & 0xF);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.hardwareVersion = {0}.{1}",
                                               (footer.hardwareVersion & 0xF0) >> 4, footer.hardwareVersion & 0xF);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.firmwareVersion = {0}.{1}",
                                               (footer.firmwareVersion & 0xF0) >> 4, footer.firmwareVersion & 0xF);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.imageVersion = {0}.{1}",
                                               (footer.imageVersion & 0xF0) >> 4, footer.imageVersion & 0xF);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.signature = \"{0}\"",
                                               StringHandlers.CToString(BitConverter.GetBytes(footer.signature)));

                    _imageInfo.DriveManufacturer = ReadPStringUtf8(_scpStream, footer.manufacturerOffset);
                    _imageInfo.DriveModel        = ReadPStringUtf8(_scpStream, footer.modelOffset);
                    _imageInfo.DriveSerialNumber = ReadPStringUtf8(_scpStream, footer.serialOffset);
                    _imageInfo.Creator           = ReadPStringUtf8(_scpStream, footer.creatorOffset);
                    _imageInfo.Application       = ReadPStringUtf8(_scpStream, footer.applicationOffset);
                    _imageInfo.Comments          = ReadPStringUtf8(_scpStream, footer.commentsOffset);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveManufacturer = \"{0}\"",
                                               _imageInfo.DriveManufacturer);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveModel = \"{0}\"",
                                               _imageInfo.DriveModel);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveSerialNumber = \"{0}\"",
                                               _imageInfo.DriveSerialNumber);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageCreator = \"{0}\"",
                                               _imageInfo.Creator);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageApplication = \"{0}\"",
                                               _imageInfo.Application);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageComments = \"{0}\"",
                                               _imageInfo.Comments);

                    _imageInfo.CreationTime = footer.creationTime != 0
                                                  ? DateHandlers.UnixToDateTime(footer.creationTime)
                                                  : imageFilter.CreationTime;

                    _imageInfo.LastModificationTime = footer.modificationTime != 0
                                                          ? DateHandlers.UnixToDateTime(footer.modificationTime)
                                                          : imageFilter.LastWriteTime;

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageCreationTime = {0}",
                                               _imageInfo.CreationTime);

                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageLastModificationTime = {0}",
                                               _imageInfo.LastModificationTime);

                    _imageInfo.ApplicationVersion =
                        $"{(footer.applicationVersion & 0xF0) >> 4}.{footer.applicationVersion & 0xF}";

                    _imageInfo.DriveFirmwareRevision =
                        $"{(footer.firmwareVersion & 0xF0) >> 4}.{footer.firmwareVersion & 0xF}";

                    _imageInfo.Version = $"{(footer.imageVersion & 0xF0) >> 4}.{footer.imageVersion & 0xF}";

                    break;
                }

                _scpStream.Seek(-8, SeekOrigin.Current);
            }
        }
        else
        {
            _imageInfo.Application = (Header.flags & ScpFlags.CreatedByOtherDevice) == 0 ? "SuperCardPro" : null;
            _imageInfo.ApplicationVersion = $"{(Header.version & 0xF0) >> 4}.{Header.version & 0xF}";
            _imageInfo.CreationTime = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
            _imageInfo.Version = "2.4";
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotSupported;
    }

    public ErrorNumber SubTrackLength(uint head, ushort track, out byte length)
    {
        length = 1;

        return ErrorNumber.NoError;
    }

    public ErrorNumber CapturesLength(uint head, ushort track, byte subTrack, out uint length)
    {
        length = 1;

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadFluxIndexResolution(uint head, ushort track, byte subTrack, uint captureIndex,
                                               out ulong resolution)
    {
        resolution = (ulong)((Header.resolution + 1) * DEFAULT_RESOLUTION);

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadFluxDataResolution(uint head, ushort track, byte subTrack, uint captureIndex,
                                              out ulong resolution)
    {
        resolution = (ulong)((Header.resolution + 1) * DEFAULT_RESOLUTION);

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadFluxResolution(uint head, ushort track, byte subTrack, uint captureIndex,
                                          out ulong indexResolution, out ulong dataResolution)
    {
        indexResolution = dataResolution = 0;

        ErrorNumber indexError = ReadFluxIndexResolution(head, track, subTrack, captureIndex, out indexResolution);

        if(indexError != ErrorNumber.NoError)
            return indexError;

        ErrorNumber dataError = ReadFluxDataResolution(head, track, subTrack, captureIndex, out dataResolution);

        return dataError;
    }

    public ErrorNumber ReadFluxIndexCapture(uint head, ushort track, byte subTrack, uint captureIndex,
                                            out byte[] buffer)
    {
        buffer = null;

        if(Header.flags.HasFlag(ScpFlags.NotFloppy))
            return ErrorNumber.NotImplemented;

        if(captureIndex > 0)
            return ErrorNumber.OutOfRange;

        List<byte> tmpBuffer = new();

        if(Header.flags.HasFlag(ScpFlags.StartsAtIndex))
            tmpBuffer.Add(0);

        TrackHeader scpTrack = ScpTracks[(byte)HeadTrackSubToScpTrack(head, track, subTrack)];

        for(int i = 0; i < Header.revolutions; i++)
            tmpBuffer.AddRange(UInt32ToFluxRepresentation(scpTrack.Entries[i].indexTime));

        buffer = tmpBuffer.ToArray();

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadFluxDataCapture(uint head, ushort track, byte subTrack, uint captureIndex, out byte[] buffer)
    {
        buffer = null;

        if(Header.flags.HasFlag(ScpFlags.NotFloppy))
            return ErrorNumber.NotImplemented;

        if(HeadTrackSubToScpTrack(head, track, subTrack) > Header.end)
            return ErrorNumber.OutOfRange;

        if(captureIndex > 0)
            return ErrorNumber.OutOfRange;

        if(Header.bitCellEncoding != 0 &&
           Header.bitCellEncoding != 16)
            return ErrorNumber.NotImplemented;

        TrackHeader scpTrack = ScpTracks[(byte)HeadTrackSubToScpTrack(head, track, subTrack)];

        Stream stream = _scpFilter.GetDataForkStream();
        var    br     = new BinaryReader(stream);

        List<byte> tmpBuffer = new();

        for(int i = 0; i < Header.revolutions; i++)
        {
            br.BaseStream.Seek(scpTrack.Entries[i].dataOffset, SeekOrigin.Begin);

            // TODO: Check for 0x0000
            for(ulong j = 0; j < scpTrack.Entries[i].trackLength; j++)
                tmpBuffer.AddRange(UInt16ToFluxRepresentation(BigEndianBitConverter.ToUInt16(br.ReadBytes(2), 0)));
        }

        buffer = tmpBuffer.ToArray();

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadFluxCapture(uint head, ushort track, byte subTrack, uint captureIndex,
                                       out ulong indexResolution, out ulong dataResolution, out byte[] indexBuffer,
                                       out byte[] dataBuffer)
    {
        dataBuffer = indexBuffer = null;

        ErrorNumber error =
            ReadFluxResolution(head, track, subTrack, captureIndex, out indexResolution, out dataResolution);

        if(error != ErrorNumber.NoError)
            return error;

        error = ReadFluxDataCapture(head, track, subTrack, captureIndex, out dataBuffer);

        if(error != ErrorNumber.NoError)
            return error;

        ErrorNumber indexCapture = ReadFluxIndexCapture(head, track, subTrack, captureIndex, out indexBuffer);

        return indexCapture;
    }
}