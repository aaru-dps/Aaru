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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Images;

public sealed partial class SuperCardPro
{
#region IWritableFluxImage Members

    /// <inheritdoc />
    /// <summary>Opens and parses SuperCard Pro flux image file</summary>
    public ErrorNumber Open(IFilter imageFilter)
    {
        Header     = new ScpHeader();
        _scpStream = imageFilter.GetDataForkStream();
        _scpStream.Seek(0, SeekOrigin.Begin);

        _scpFilter = imageFilter;

        // Per SCP spec: Minimum file size is header size (bytes 0x00-0x0F + TDH offset table)
        if(_scpStream.Length < Marshal.SizeOf<ScpHeader>()) return ErrorNumber.InvalidArgument;

        // Per SCP spec: Read header starting at byte 0x00
        // Bytes 0x00-0x02: "SCP" signature
        // Byte 0x03: Version/revision (Version<<4|Revision)
        var hdr = new byte[Marshal.SizeOf<ScpHeader>()];
        _scpStream.EnsureRead(hdr, 0, Marshal.SizeOf<ScpHeader>());

        Header = Marshal.ByteArrayToStructureLittleEndian<ScpHeader>(hdr);

        AaruLogging.Debug(MODULE_NAME, "header.signature = \"{0}\"", StringHandlers.CToString(Header.signature));

        AaruLogging.Debug(MODULE_NAME, "header.version = {0}.{1}", (Header.version & 0xF0) >> 4, Header.version & 0xF);

        AaruLogging.Debug(MODULE_NAME, "header.type = {0}",            Header.type);
        AaruLogging.Debug(MODULE_NAME, "header.revolutions = {0}",     Header.revolutions);
        AaruLogging.Debug(MODULE_NAME, "header.start = {0}",           Header.start);
        AaruLogging.Debug(MODULE_NAME, "header.end = {0}",             Header.end);
        AaruLogging.Debug(MODULE_NAME, "header.bitCellEncoding = {0}", Header.bitCellEncoding);
        AaruLogging.Debug(MODULE_NAME, "header.heads = {0}",           Header.heads);
        AaruLogging.Debug(MODULE_NAME, "header.resolution = {0}",      Header.resolution);
        AaruLogging.Debug(MODULE_NAME, "header.checksum = 0x{0:X8}",   Header.checksum);

        // Per SCP spec: FLAGS byte (0x08) bit definitions
        // Bit 0 (INDEX): cleared = no index reference, set = flux starts at index
        // Bit 1 (TPI): cleared = 48TPI, set = 96TPI (5.25" drives only)
        // Bit 2 (RPM): cleared = 300 RPM, set = 360 RPM
        // Bit 3 (TYPE): cleared = original flux, set = normalized flux
        // Bit 4 (MODE): cleared = read-only, set = read/write capable
        // Bit 5 (FOOTER): cleared = no footer, set = footer present
        // Bit 6 (EXTENDED MODE): cleared = floppy only, set = extended mode (tapes/hard drives)
        // Bit 7 (FLUX CREATOR): cleared = SuperCard Pro, set = other device
        AaruLogging.Debug(MODULE_NAME,
                          "header.flags.StartsAtIndex = {0}",
                          Header.flags.HasFlag(ScpFlags.StartsAtIndex));

        AaruLogging.Debug(MODULE_NAME,
                          "header.flags.Tpi = {0}",
                          Header.flags.HasFlag(ScpFlags.Tpi) ? "96tpi" : "48tpi");

        AaruLogging.Debug(MODULE_NAME,
                          "header.flags.Rpm = {0}",
                          Header.flags.HasFlag(ScpFlags.Rpm) ? "360rpm" : "300rpm");

        AaruLogging.Debug(MODULE_NAME, "header.flags.Normalized = {0}", Header.flags.HasFlag(ScpFlags.Normalized));

        AaruLogging.Debug(MODULE_NAME, "header.flags.Writable = {0}", Header.flags.HasFlag(ScpFlags.Writable));

        AaruLogging.Debug(MODULE_NAME, "header.flags.HasFooter = {0}", Header.flags.HasFlag(ScpFlags.HasFooter));

        AaruLogging.Debug(MODULE_NAME, "header.flags.NotFloppy = {0}", Header.flags.HasFlag(ScpFlags.NotFloppy));

        AaruLogging.Debug(MODULE_NAME,
                          "header.flags.CreatedByOtherDevice = {0}",
                          Header.flags.HasFlag(ScpFlags.CreatedByOtherDevice));

        // Per SCP spec: First 3 bytes must be "SCP" signature
        if(!_scpSignature.SequenceEqual(Header.signature)) return ErrorNumber.InvalidArgument;

        // Extended mode (FLAGS bit 6) indicates non-floppy media (tapes/hard drives)
        // These remain unimplemented for now
        if(Header.flags.HasFlag(ScpFlags.NotFloppy)) return ErrorNumber.NotImplemented;

        // Since we only support floppy disks, always set MetadataMediaType to BlockMedia
        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

        ScpTracks = new Dictionary<byte, TrackHeader>();

        // Per SCP spec: For single-sided disks, skip TDH entries appropriately
        // heads = 0: both heads (read all tracks)
        // heads = 1: side 0 only (read even entries: 0,2,4,6...)
        // heads = 2: side 1 only (read odd entries: 1,3,5,7...)
        for(byte t = Header.start; t <= Header.end; t++)
        {
            if(t >= Header.offsets.Length) break;

            // Skip entries based on single-sided disk configuration
            if(Header.heads == 1 && (t % 2) != 0) continue; // Side 0 only - skip odd entries
            if(Header.heads == 2 && (t % 2) == 0) continue; // Side 1 only - skip even entries

            if(Header.offsets[t] == 0) continue; // Per SCP spec: 0x00000000 means no flux data for this track

            _scpStream.Position = Header.offsets[t];

            var trk = new TrackHeader
            {
                Signature = new byte[3],
                Entries   = new TrackEntry[Header.revolutions]
            };

            // Per SCP spec: Track Data Header (TDH) starts with "TRK" signature (3 bytes) + track number (1 byte)
            _scpStream.EnsureRead(trk.Signature, 0, trk.Signature.Length);
            trk.TrackNumber = (byte)_scpStream.ReadByte();

            // Per SCP spec: Validate TDH signature for recovery from corrupt files
            if(!trk.Signature.SequenceEqual(_trkSignature))
            {
                AaruLogging.Debug(MODULE_NAME,
                                  Localization.Track_header_at_0_contains_incorrect_signature,
                                  Header.offsets[t]);

                continue;
            }

            if(trk.TrackNumber != t)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  Localization.Track_number_at_0_should_be_1_but_is_2,
                                  Header.offsets[t],
                                  t,
                                  trk.TrackNumber);

                continue;
            }

            AaruLogging.Debug(MODULE_NAME, Localization.Found_track_0_at_1, t, Header.offsets[t]);

            // Per SCP spec: Each revolution has 3 longwords: indexTime, trackLength, dataOffset
            // dataOffset is relative to start of TDH (not file start)
            for(byte r = 0; r < Header.revolutions; r++)
            {
                var rev = new byte[Marshal.SizeOf<TrackEntry>()];
                _scpStream.EnsureRead(rev, 0, Marshal.SizeOf<TrackEntry>());

                trk.Entries[r] = Marshal.ByteArrayToStructureLittleEndian<TrackEntry>(rev);

                // Per SCP spec: dataOffset is relative to TDH start, convert to absolute file offset
                trk.Entries[r].dataOffset += Header.offsets[t];

                // Flux data is one 16-bit cell per length unit and must fit inside the file
                if(trk.Entries[r].dataOffset + (ulong)trk.Entries[r].trackLength * 2 > (ulong)_scpStream.Length)
                    return ErrorNumber.InvalidArgument;
            }

            ScpTracks.Add(t, trk);
        }

        switch(Header.type)
        {
            case ScpDiskType.Commodore64:
                _imageInfo.MediaType = MediaType.CBM_1540;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 40);
                _imageInfo.Heads     = 1;

                break;
            case ScpDiskType.CommodoreAmiga:
                _imageInfo.MediaType = MediaType.CBM_AMIGA_35_DD;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.CommodoreAmigaHD:
                _imageInfo.MediaType = MediaType.CBM_AMIGA_35_HD;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.AtariFMSS:
                _imageInfo.MediaType = MediaType.ATARI_525_SD;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 40);
                _imageInfo.Heads     = 1;

                break;
            case ScpDiskType.AtariFMDS:
                _imageInfo.MediaType = MediaType.Unknown;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 40);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.AtariFMEx:
                return ErrorNumber.NotImplemented;
            case ScpDiskType.AtariSTSS:
                _imageInfo.MediaType = MediaType.ATARI_35_SS_DD;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads     = 1;

                break;
            case ScpDiskType.AtariSTDS:
                _imageInfo.MediaType = MediaType.ATARI_35_DS_DD;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.AtariSTSSHD: // Per SCP spec v2.5: Atari ST SS HD
            case ScpDiskType.AtariSTDSHD: // Per SCP spec v2.5: Atari ST DS HD
                return ErrorNumber.NotImplemented;
            case ScpDiskType.AppleII:
                _imageInfo.MediaType = MediaType.Apple32DS;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 40);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.AppleIIPro:
                return ErrorNumber.NotImplemented;
            case ScpDiskType.Apple400K:
                _imageInfo.MediaType       = MediaType.AppleSonySS;
                _imageInfo.Cylinders       = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 10;

                break;
            case ScpDiskType.Apple800K:
                _imageInfo.MediaType       = MediaType.AppleSonyDS;
                _imageInfo.Cylinders       = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 10;

                break;
            case ScpDiskType.Apple144:
                _imageInfo.MediaType       = MediaType.DOS_525_HD;
                _imageInfo.Cylinders       = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 18;

                break;
            case ScpDiskType.PC360K:
                _imageInfo.MediaType       = MediaType.DOS_525_DS_DD_9;
                _imageInfo.Cylinders       = (uint)int.Max((Header.end + 1) / 2, 40);
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 9;

                break;
            case ScpDiskType.PC720K:
                _imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.PC12M:
                _imageInfo.MediaType = MediaType.DOS_525_HD;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.PC144M:
                _imageInfo.MediaType       = MediaType.DOS_35_HD;
                _imageInfo.Cylinders       = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 18;

                break;
            case ScpDiskType.TandySSDD:
            case ScpDiskType.TandyDSSD:
            case ScpDiskType.TandyDSDD:
            case ScpDiskType.Ti994A:
            case ScpDiskType.RolandD20:
            case ScpDiskType.AmstradCPC:
                return ErrorNumber.NotImplemented;
            case ScpDiskType.Generic360K:
                _imageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 40);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.Generic12M:
                _imageInfo.MediaType = MediaType.DOS_525_HD;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads     = 2;

                break;

            case ScpDiskType.Generic720K:
                _imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.Generic144M:
                _imageInfo.MediaType = MediaType.DOS_35_HD;
                _imageInfo.Cylinders = (uint)int.Max((Header.end + 1) / 2, 80);
                _imageInfo.Heads     = 2;

                break;
            case ScpDiskType.TapeGCR1:
            case ScpDiskType.TapeGCR2:
            case ScpDiskType.TapeMFM:
                // Tapes remain unimplemented for now
                return ErrorNumber.NotImplemented;
            case ScpDiskType.HddMFM:
            case ScpDiskType.HddRLL:
                // Hard drives remain unimplemented for now
                return ErrorNumber.NotImplemented;
            default:
                _imageInfo.MediaType = MediaType.Unknown;

                _imageInfo.Cylinders =
                    (uint)int.Max((Header.end + 1) / 2, Header.flags.HasFlag(ScpFlags.Tpi) ? 80 : 40);

                _imageInfo.Heads = Header.heads == 0 ? 2 : (uint)1;

                break;
        }

        // Per SCP spec: Timestamp (if present) appears after track data, before footer
        // Read timestamp if present
        long   lastTrackDataPosition = _scpStream.Position;
        string timestamp             = ReadTimestamp(_scpStream, lastTrackDataPosition);

        if(timestamp != null)
        {
            AaruLogging.Debug(MODULE_NAME, "Found timestamp: \"{0}\"", timestamp);

            // Timestamp is informative only - we use footer timestamps if footer is present
        }

        // Per SCP spec: Footer detection - look for "FPCS" signature
        if(Header.flags.HasFlag(ScpFlags.HasFooter))
        {
            long position = timestamp != null ? _scpStream.Position : lastTrackDataPosition;
            _scpStream.Seek(-4, SeekOrigin.End);

            while(_scpStream.Position >= position)
            {
                var footerSig = new byte[4];
                _scpStream.EnsureRead(footerSig, 0, 4);
                var footerMagic = BitConverter.ToUInt32(footerSig, 0);

                if(footerMagic == FOOTER_SIGNATURE)
                {
                    _scpStream.Seek(-Marshal.SizeOf<Footer>(), SeekOrigin.Current);

                    AaruLogging.Debug(MODULE_NAME, Localization.Found_footer_at_0, _scpStream.Position);

                    var ftr = new byte[Marshal.SizeOf<Footer>()];
                    _scpStream.EnsureRead(ftr, 0, Marshal.SizeOf<Footer>());

                    Footer footer = Marshal.ByteArrayToStructureLittleEndian<Footer>(ftr);

                    AaruLogging.Debug(MODULE_NAME, "footer.manufacturerOffset = 0x{0:X8}", footer.manufacturerOffset);

                    AaruLogging.Debug(MODULE_NAME, "footer.modelOffset = 0x{0:X8}", footer.modelOffset);

                    AaruLogging.Debug(MODULE_NAME, "footer.serialOffset = 0x{0:X8}", footer.serialOffset);

                    AaruLogging.Debug(MODULE_NAME, "footer.creatorOffset = 0x{0:X8}", footer.creatorOffset);

                    AaruLogging.Debug(MODULE_NAME, "footer.applicationOffset = 0x{0:X8}", footer.applicationOffset);

                    AaruLogging.Debug(MODULE_NAME, "footer.commentsOffset = 0x{0:X8}", footer.commentsOffset);

                    AaruLogging.Debug(MODULE_NAME, "footer.creationTime = {0}", footer.creationTime);

                    AaruLogging.Debug(MODULE_NAME, "footer.modificationTime = {0}", footer.modificationTime);

                    AaruLogging.Debug(MODULE_NAME,
                                      "footer.applicationVersion = {0}.{1}",
                                      (footer.applicationVersion & 0xF0) >> 4,
                                      footer.applicationVersion & 0xF);

                    AaruLogging.Debug(MODULE_NAME,
                                      "footer.hardwareVersion = {0}.{1}",
                                      (footer.hardwareVersion & 0xF0) >> 4,
                                      footer.hardwareVersion & 0xF);

                    AaruLogging.Debug(MODULE_NAME,
                                      "footer.firmwareVersion = {0}.{1}",
                                      (footer.firmwareVersion & 0xF0) >> 4,
                                      footer.firmwareVersion & 0xF);

                    AaruLogging.Debug(MODULE_NAME,
                                      "footer.imageVersion = {0}.{1}",
                                      (footer.imageVersion & 0xF0) >> 4,
                                      footer.imageVersion & 0xF);

                    AaruLogging.Debug(MODULE_NAME,
                                      "footer.signature = \"{0}\"",
                                      StringHandlers.CToString(BitConverter.GetBytes(footer.signature)));

                    _imageInfo.DriveManufacturer = ReadPStringUtf8(_scpStream, footer.manufacturerOffset);
                    _imageInfo.DriveModel        = ReadPStringUtf8(_scpStream, footer.modelOffset);
                    _imageInfo.DriveSerialNumber = ReadPStringUtf8(_scpStream, footer.serialOffset);
                    _imageInfo.Creator           = ReadPStringUtf8(_scpStream, footer.creatorOffset);
                    _imageInfo.Application       = ReadPStringUtf8(_scpStream, footer.applicationOffset);
                    _imageInfo.Comments          = ReadPStringUtf8(_scpStream, footer.commentsOffset);

                    AaruLogging.Debug(MODULE_NAME,
                                      "ImageInfo.driveManufacturer = \"{0}\"",
                                      _imageInfo.DriveManufacturer);

                    AaruLogging.Debug(MODULE_NAME, "ImageInfo.driveModel = \"{0}\"", _imageInfo.DriveModel);

                    AaruLogging.Debug(MODULE_NAME,
                                      "ImageInfo.driveSerialNumber = \"{0}\"",
                                      _imageInfo.DriveSerialNumber);

                    AaruLogging.Debug(MODULE_NAME, "ImageInfo.imageCreator = \"{0}\"", _imageInfo.Creator);

                    AaruLogging.Debug(MODULE_NAME, "ImageInfo.imageApplication = \"{0}\"", _imageInfo.Application);

                    AaruLogging.Debug(MODULE_NAME, "ImageInfo.imageComments = \"{0}\"", _imageInfo.Comments);

                    _imageInfo.CreationTime = footer.creationTime != 0
                                                  ? DateHandlers.UnixToDateTime(footer.creationTime)
                                                  : imageFilter.CreationTime;

                    _imageInfo.LastModificationTime = footer.modificationTime != 0
                                                          ? DateHandlers.UnixToDateTime(footer.modificationTime)
                                                          : imageFilter.LastWriteTime;

                    AaruLogging.Debug(MODULE_NAME, "ImageInfo.imageCreationTime = {0}", _imageInfo.CreationTime);

                    AaruLogging.Debug(MODULE_NAME,
                                      "ImageInfo.imageLastModificationTime = {0}",
                                      _imageInfo.LastModificationTime);

                    _imageInfo.ApplicationVersion =
                        $"{(footer.applicationVersion & 0xF0) >> 4}.{footer.applicationVersion & 0xF}";

                    _imageInfo.DriveFirmwareRevision =
                        $"{(footer.firmwareVersion & 0xF0) >> 4}.{footer.firmwareVersion & 0xF}";

                    // Per SCP spec: When FOOTER bit is set and version byte is 0, use footer version
                    if(Header.version == 0)
                    {
                        _imageInfo.Version = $"{(footer.imageVersion & 0xF0) >> 4}.{footer.imageVersion & 0xF}";

                        AaruLogging.Debug(MODULE_NAME,
                                          "Using footer version (header version was 0): {0}",
                                          _imageInfo.Version);
                    }
                    else
                    {
                        _imageInfo.Version = $"{(footer.imageVersion & 0xF0) >> 4}.{footer.imageVersion & 0xF}";
                    }

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
        }

        return ErrorNumber.NoError;
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

    public ErrorNumber ReadFluxIndexResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                               out ulong resolution)
    {
        resolution = (ulong)((Header.resolution + 1) * DEFAULT_RESOLUTION);

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadFluxDataResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                              out ulong resolution)
    {
        resolution = (ulong)((Header.resolution + 1) * DEFAULT_RESOLUTION);

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadFluxResolution(uint      head,            ushort    track, byte subTrack, uint captureIndex,
                                          out ulong indexResolution, out ulong dataResolution)
    {
        indexResolution = dataResolution = 0;

        ErrorNumber indexError = ReadFluxIndexResolution(head, track, subTrack, captureIndex, out indexResolution);

        if(indexError != ErrorNumber.NoError) return indexError;

        ErrorNumber dataError = ReadFluxDataResolution(head, track, subTrack, captureIndex, out dataResolution);

        return dataError;
    }

    public ErrorNumber ReadFluxIndexCapture(uint       head, ushort track, byte subTrack, uint captureIndex,
                                            out byte[] buffer)
    {
        buffer = null;

        // Non-floppy media (tapes/hard drives) not supported
        if(Header.flags.HasFlag(ScpFlags.NotFloppy)) return ErrorNumber.NotImplemented;

        if(captureIndex > 0) return ErrorNumber.OutOfRange;

        List<byte> tmpBuffer = [];

        // Per SCP spec: If flux starts at index, add initial 0 marker
        if(Header.flags.HasFlag(ScpFlags.StartsAtIndex)) tmpBuffer.Add(0);

        byte scpTrackNum = (byte)HeadTrackSubToScpTrack(head, track, subTrack, Header.heads);

        if(!ScpTracks.TryGetValue(scpTrackNum, out TrackHeader scpTrack)) return ErrorNumber.OutOfRange;

        // Per SCP spec: indexTime is duration in nanoseconds/25ns for one revolution
        for(var i = 0; i < Header.revolutions; i++)
            tmpBuffer.AddRange(UInt32ToFluxRepresentation(scpTrack.Entries[i].indexTime));

        buffer = tmpBuffer.ToArray();

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadFluxDataCapture(uint head, ushort track, byte subTrack, uint captureIndex, out byte[] buffer)
    {
        buffer = null;

        // Non-floppy media (tapes/hard drives) not supported
        if(Header.flags.HasFlag(ScpFlags.NotFloppy)) return ErrorNumber.NotImplemented;

        byte scpTrackNum = (byte)HeadTrackSubToScpTrack(head, track, subTrack, Header.heads);

        if(scpTrackNum > Header.end) return ErrorNumber.OutOfRange;

        if(captureIndex > 0) return ErrorNumber.OutOfRange;

        // Per SCP spec: bitCellEncoding (byte 0x09) = 0 means 16 bits, other values are for future expansion
        if(Header.bitCellEncoding != 0 && Header.bitCellEncoding != 16) return ErrorNumber.NotImplemented;

        if(!ScpTracks.TryGetValue(scpTrackNum, out TrackHeader scpTrack)) return ErrorNumber.OutOfRange;

        Stream stream = _scpFilter.GetDataForkStream();
        var    br     = new BinaryReader(stream);

        List<byte> tmpBuffer = [];

        for(var i = 0; i < Header.revolutions; i++)
        {
            br.BaseStream.Seek(scpTrack.Entries[i].dataOffset, SeekOrigin.Begin);

            // Per SCP spec: Handle 0x0000 overflow entries in flux data (strongbits protection)
            ReadFluxDataWithOverflow(br, scpTrack.Entries[i].trackLength, tmpBuffer);
        }

        buffer = tmpBuffer.ToArray();

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadFluxCapture(uint       head,            ushort    track, byte subTrack, uint captureIndex,
                                       out ulong  indexResolution, out ulong dataResolution, out byte[] indexBuffer,
                                       out byte[] dataBuffer)
    {
        dataBuffer = indexBuffer = null;

        ErrorNumber error =
            ReadFluxResolution(head, track, subTrack, captureIndex, out indexResolution, out dataResolution);

        if(error != ErrorNumber.NoError) return error;

        error = ReadFluxDataCapture(head, track, subTrack, captureIndex, out dataBuffer);

        if(error != ErrorNumber.NoError) return error;

        ErrorNumber indexCapture = ReadFluxIndexCapture(head, track, subTrack, captureIndex, out indexBuffer);

        return indexCapture;
    }

    /// <inheritdoc />
    public ErrorNumber GetAllFluxCaptures(out List<FluxCapture> captures)
    {
        captures = [];

        if(ScpTracks is { Count: > 0 })
        {
            ulong resolution = (ulong)((Header.resolution + 1) * DEFAULT_RESOLUTION);

            captures =
            [
                .. ScpTracks.Select(kvp =>
                {
                    byte scpTrack = kvp.Key;

                    // Reverse HeadTrackSubToScpTrack based on heads configuration
                    // Per SCP spec: Single-sided disks use specific entry patterns
                    uint       head;
                    ushort     track;
                    const byte subTrack = 0; // SuperCardPro always has subTrack = 0

                    if(Header.heads == 1) // Side 0 only - even entries
                    {
                        track = (ushort)(scpTrack / 2);
                        head  = 0;
                    }
                    else if(Header.heads == 2) // Side 1 only - odd entries
                    {
                        track = (ushort)(scpTrack / 2);
                        head  = 1;
                    }
                    else // Double-sided - standard mapping
                    {
                        head  = (uint)(scpTrack   % 2);
                        track = (ushort)(scpTrack / 2);
                    }

                    return new FluxCapture
                    {
                        Head            = head,
                        Track           = track,
                        SubTrack        = subTrack,
                        CaptureIndex    = 0, // SuperCardPro always has one capture per track
                        IndexResolution = resolution,
                        DataResolution  = resolution
                    };
                })
            ];
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
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.NotDumped;

        return ReadSectors(sectorAddress, negative, 1, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, negative, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.NotDumped;

        return ReadSectorsLong(sectorAddress, negative, 1, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        return ErrorNumber.NotSupported;
    }

#endregion
}