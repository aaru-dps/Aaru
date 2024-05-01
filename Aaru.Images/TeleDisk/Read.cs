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
//     Reads Sydex TeleDisk disk images.
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
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Compression;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class TeleDisk
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        _header = new Header();
        var headerBytes = new byte[12];
        _inStream = imageFilter.GetDataForkStream();
        var stream = new MemoryStream();
        _inStream.Seek(0, SeekOrigin.Begin);

        _inStream.EnsureRead(headerBytes, 0, 12);
        stream.Write(headerBytes, 0, 12);

        _header.Signature = BitConverter.ToUInt16(headerBytes, 0);

        if(_header.Signature != TD_MAGIC && _header.Signature != TD_ADV_COMP_MAGIC) return ErrorNumber.InvalidArgument;

        _header.Sequence      = headerBytes[2];
        _header.DiskSet       = headerBytes[3];
        _header.Version       = headerBytes[4];
        _header.DataRate      = headerBytes[5];
        _header.DriveType     = headerBytes[6];
        _header.Stepping      = headerBytes[7];
        _header.DosAllocation = headerBytes[8];
        _header.Sides         = headerBytes[9];
        _header.Crc           = BitConverter.ToUInt16(headerBytes, 10);

        _imageInfo.MediaTitle  = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Version     = $"{(_header.Version & 0xF0) >> 4}.{_header.Version & 0x0F}";
        _imageInfo.Application = _imageInfo.Version;

        var headerBytesForCrc = new byte[10];
        Array.Copy(headerBytes, headerBytesForCrc, 10);
        ushort calculatedHeaderCrc = TeleDiskCrc(0x0000, headerBytesForCrc);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.signature = 0x{0:X4}",     _header.Signature);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.sequence = 0x{0:X2}",      _header.Sequence);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.diskSet = 0x{0:X2}",       _header.DiskSet);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.version = 0x{0:X2}",       _header.Version);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dataRate = 0x{0:X2}",      _header.DataRate);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.driveType = 0x{0:X2}",     _header.DriveType);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.stepping = 0x{0:X2}",      _header.Stepping);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dosAllocation = 0x{0:X2}", _header.DosAllocation);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.sides = 0x{0:X2}",         _header.Sides);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.crc = 0x{0:X4}",           _header.Crc);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.calculated_header_crc_equals_0_X4, calculatedHeaderCrc);

        // We need more checks as the magic is too simply.
        // This may deny legal images

        // That would be much of a coincidence
        if(_header.Crc != calculatedHeaderCrc)
        {
            _aDiskCrcHasFailed = true;

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Calculated_CRC_does_not_coincide_with_stored_one);
        }

        if(_header.Sequence != 0x00) return ErrorNumber.InvalidArgument;

        if(_header.DataRate != DATA_RATE_250_KBPS &&
           _header.DataRate != DATA_RATE_300_KBPS &&
           _header.DataRate != DATA_RATE_500_KBPS)
            return ErrorNumber.NotSupported;

        if(_header.DriveType != DRIVE_TYPE_35_DD          &&
           _header.DriveType != DRIVE_TYPE_35_ED          &&
           _header.DriveType != DRIVE_TYPE_35_HD          &&
           _header.DriveType != DRIVE_TYPE_525_DD         &&
           _header.DriveType != DRIVE_TYPE_525_HD         &&
           _header.DriveType != DRIVE_TYPE_525_HD_DD_DISK &&
           _header.DriveType != DRIVE_TYPE_8_INCH)
            return ErrorNumber.NotSupported;

        if(_header.Signature == TD_ADV_COMP_MAGIC)
        {
            int rd;
            _inStream.Seek(12, SeekOrigin.Begin);
            stream.Seek(12, SeekOrigin.Begin);
            var lzh = new TeleDiskLzh(_inStream);

            do
            {
                if((rd = lzh.Decode(out byte[] obuf, BUFSZ)) > 0) stream.Write(obuf, 0, rd);
            } while(rd == BUFSZ);
        }
        else
        {
            // Not using Stream.CopyTo() because it's failing with LZIP
            var copybuf = new byte[_inStream.Length];
            _inStream.Seek(0, SeekOrigin.Begin);
            _inStream.EnsureRead(copybuf, 0, copybuf.Length);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(copybuf, 0, copybuf.Length);
        }

        stream.Seek(12, SeekOrigin.Begin);

        _imageInfo.CreationTime = DateTime.MinValue;

        if((_header.Stepping & COMMENT_BLOCK_PRESENT) == COMMENT_BLOCK_PRESENT)
        {
            _commentHeader = new CommentBlockHeader();

            var commentHeaderBytes = new byte[10];

            stream.EnsureRead(commentHeaderBytes, 0, 10);
            _commentHeader.Crc    = BitConverter.ToUInt16(commentHeaderBytes, 0);
            _commentHeader.Length = BitConverter.ToUInt16(commentHeaderBytes, 2);
            _commentHeader.Year   = commentHeaderBytes[4];
            _commentHeader.Month  = commentHeaderBytes[5];
            _commentHeader.Day    = commentHeaderBytes[6];
            _commentHeader.Hour   = commentHeaderBytes[7];
            _commentHeader.Minute = commentHeaderBytes[8];
            _commentHeader.Second = commentHeaderBytes[9];

            _commentBlock = new byte[_commentHeader.Length];
            stream.EnsureRead(_commentBlock, 0, _commentHeader.Length);

            var commentBlockForCrc = new byte[_commentHeader.Length + 8];
            Array.Copy(commentHeaderBytes, 2, commentBlockForCrc, 0, 8);
            Array.Copy(_commentBlock,      0, commentBlockForCrc, 8, _commentHeader.Length);

            ushort cmtcrc = TeleDiskCrc(0, commentBlockForCrc);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Comment_header);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\tcommentheader.crc = 0x{0:X4}",               _commentHeader.Crc);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Calculated_CRC_equals_0_X4, cmtcrc);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\tcommentheader.length = {0} bytes", _commentHeader.Length);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\tcommentheader.year = {0}",   _commentHeader.Year);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\tcommentheader.month = {0}",  _commentHeader.Month);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\tcommentheader.day = {0}",    _commentHeader.Day);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\tcommentheader.hour = {0}",   _commentHeader.Hour);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\tcommentheader.minute = {0}", _commentHeader.Minute);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\tcommentheader.second = {0}", _commentHeader.Second);

            _aDiskCrcHasFailed |= cmtcrc != _commentHeader.Crc;

            for(var i = 0; i < _commentBlock.Length; i++)

                // Replace NULLs, used by TeleDisk as newline markers, with UNIX newline marker
            {
                if(_commentBlock[i] == 0x00) _commentBlock[i] = 0x0A;
            }

            _imageInfo.Comments = Encoding.ASCII.GetString(_commentBlock);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Comment);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", _imageInfo.Comments);

            _imageInfo.CreationTime = new DateTime(_commentHeader.Year  + 1900,
                                                   _commentHeader.Month + 1,
                                                   _commentHeader.Day,
                                                   _commentHeader.Hour,
                                                   _commentHeader.Minute,
                                                   _commentHeader.Second,
                                                   DateTimeKind.Unspecified);
        }

        if(_imageInfo.CreationTime == DateTime.MinValue) _imageInfo.CreationTime = imageFilter.CreationTime;

        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Image_created_on_0, _imageInfo.CreationTime);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Image_modified_on_0, _imageInfo.LastModificationTime);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Parsing_image);

        _totalDiskSize       = 0;
        _imageInfo.ImageSize = 0;

        int  totalCylinders = -1;
        int  totalHeads     = -1;
        int  maxSector      = -1;
        var  totalSectors   = 0;
        long currentPos     = stream.Position;
        _imageInfo.SectorSize      = uint.MaxValue;
        _imageInfo.SectorsPerTrack = uint.MaxValue;

        // Count cylinders
        while(true)
        {
            var teleDiskTrack = new TrackHeader
            {
                Sectors  = (byte)stream.ReadByte(),
                Cylinder = (byte)stream.ReadByte(),
                Head     = (byte)stream.ReadByte(),
                Crc      = (byte)stream.ReadByte()
            };

            if(teleDiskTrack.Cylinder > totalCylinders) totalCylinders = teleDiskTrack.Cylinder;

            if(teleDiskTrack.Head > totalHeads) totalHeads = teleDiskTrack.Head;

            if(teleDiskTrack.Sectors == 0xFF) // End of disk image
                break;

            for(byte processedSectors = 0; processedSectors < teleDiskTrack.Sectors; processedSectors++)
            {
                var teleDiskSector = new SectorHeader();
                var teleDiskData   = new DataHeader();
                var dataSizeBytes  = new byte[2];

                teleDiskSector.Cylinder     = (byte)stream.ReadByte();
                teleDiskSector.Head         = (byte)stream.ReadByte();
                teleDiskSector.SectorNumber = (byte)stream.ReadByte();
                teleDiskSector.SectorSize   = (byte)stream.ReadByte();
                teleDiskSector.Flags        = (byte)stream.ReadByte();
                teleDiskSector.Crc          = (byte)stream.ReadByte();

                if(teleDiskSector.SectorNumber > maxSector) maxSector = teleDiskSector.SectorNumber;

                if((teleDiskSector.Flags & FLAGS_SECTOR_DATALESS) != FLAGS_SECTOR_DATALESS &&
                   (teleDiskSector.Flags & FLAGS_SECTOR_SKIPPED)  != FLAGS_SECTOR_SKIPPED)
                {
                    stream.EnsureRead(dataSizeBytes, 0, 2);
                    teleDiskData.DataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                    teleDiskData.DataSize--; // Sydex decided to including dataEncoding byte as part of it
                    teleDiskData.DataEncoding = (byte)stream.ReadByte();
                    var data = new byte[teleDiskData.DataSize];
                    stream.EnsureRead(data, 0, teleDiskData.DataSize);
                }

                if(128 << teleDiskSector.SectorSize < _imageInfo.SectorSize)
                    _imageInfo.SectorSize = (uint)(128 << teleDiskSector.SectorSize);

                totalSectors++;
            }
        }

        totalCylinders++;
        totalHeads++;

        if(totalCylinders <= 0 || totalHeads <= 0)
        {
            AaruConsole.ErrorWriteLine(Localization.No_cylinders_or_heads_found);

            return ErrorNumber.InvalidArgument;
        }

        var hasLeadOutOnHead0 = false;
        var hasLeadOutOnHead1 = false;
        _imageInfo.Cylinders = (ushort)totalCylinders;
        _imageInfo.Heads     = (byte)totalHeads;

        // Count sectors per track
        stream.Seek(currentPos, SeekOrigin.Begin);

        while(true)
        {
            var teleDiskTrack = new TrackHeader
            {
                Sectors  = (byte)stream.ReadByte(),
                Cylinder = (byte)stream.ReadByte(),
                Head     = (byte)stream.ReadByte(),
                Crc      = (byte)stream.ReadByte()
            };

            if(teleDiskTrack.Sectors == 0xFF) // End of disk image
                break;

            if(teleDiskTrack.Sectors < _imageInfo.SectorsPerTrack)
            {
                if(teleDiskTrack.Cylinder + 1 == totalCylinders)
                {
                    hasLeadOutOnHead0 |= teleDiskTrack.Head == 0;
                    hasLeadOutOnHead1 |= teleDiskTrack.Head == 1;

                    if(_imageInfo.Cylinders == totalCylinders) _imageInfo.Cylinders--;
                }
                else
                    _imageInfo.SectorsPerTrack = teleDiskTrack.Sectors;
            }

            for(byte processedSectors = 0; processedSectors < teleDiskTrack.Sectors; processedSectors++)
            {
                var teleDiskSector = new SectorHeader();
                var teleDiskData   = new DataHeader();
                var dataSizeBytes  = new byte[2];

                teleDiskSector.Cylinder     = (byte)stream.ReadByte();
                teleDiskSector.Head         = (byte)stream.ReadByte();
                teleDiskSector.SectorNumber = (byte)stream.ReadByte();
                teleDiskSector.SectorSize   = (byte)stream.ReadByte();
                teleDiskSector.Flags        = (byte)stream.ReadByte();
                teleDiskSector.Crc          = (byte)stream.ReadByte();

                if((teleDiskSector.Flags & FLAGS_SECTOR_DATALESS) == FLAGS_SECTOR_DATALESS ||
                   (teleDiskSector.Flags & FLAGS_SECTOR_SKIPPED)  == FLAGS_SECTOR_SKIPPED)
                    continue;

                stream.EnsureRead(dataSizeBytes, 0, 2);
                teleDiskData.DataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                teleDiskData.DataSize--; // Sydex decided to including dataEncoding byte as part of it
                teleDiskData.DataEncoding = (byte)stream.ReadByte();
                var data = new byte[teleDiskData.DataSize];
                stream.EnsureRead(data, 0, teleDiskData.DataSize);
            }
        }

        _sectorsData = new byte[totalCylinders][][][];

        // Total sectors per track
        var spts = new uint[totalCylinders][];

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   Localization.Found_0_cylinders_and_1_heads_with_a_maximum_sector_number_of_2,
                                   totalCylinders,
                                   totalHeads,
                                   maxSector);

        // Create heads
        for(var i = 0; i < totalCylinders; i++)
        {
            _sectorsData[i] = new byte[totalHeads][][];
            spts[i]         = new uint[totalHeads];

            for(var j = 0; j < totalHeads; j++) _sectorsData[i][j] = new byte[maxSector + 1][];
        }

        // Decode the image
        stream.Seek(currentPos, SeekOrigin.Begin);

        while(true)
        {
            var teleDiskTrack = new TrackHeader();
            var tdTrackForCrc = new byte[3];

            teleDiskTrack.Sectors  = (byte)stream.ReadByte();
            teleDiskTrack.Cylinder = (byte)stream.ReadByte();
            teleDiskTrack.Head     = (byte)stream.ReadByte();
            teleDiskTrack.Crc      = (byte)stream.ReadByte();

            tdTrackForCrc[0] = teleDiskTrack.Sectors;
            tdTrackForCrc[1] = teleDiskTrack.Cylinder;
            tdTrackForCrc[2] = teleDiskTrack.Head;

            var tdTrackCalculatedCrc = (byte)(TeleDiskCrc(0, tdTrackForCrc) & 0xFF);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Track_follows);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Track_cylinder_0, teleDiskTrack.Cylinder);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Track_head_0,     teleDiskTrack.Head);

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Sectors_in_track_0, teleDiskTrack.Sectors);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "\t" + Localization.Track_header_CRC_0_X2_calculated_1_X2,
                                       teleDiskTrack.Crc,
                                       tdTrackCalculatedCrc);

            _aDiskCrcHasFailed |= tdTrackCalculatedCrc != teleDiskTrack.Crc;

            if(teleDiskTrack.Sectors == 0xFF) // End of disk image
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.End_of_disk_image_arrived);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Total_of_0_data_sectors_for_1_bytes,
                                           totalSectors,
                                           _totalDiskSize);

                break;
            }

            for(byte processedSectors = 0; processedSectors < teleDiskTrack.Sectors; processedSectors++)
            {
                var    teleDiskSector = new SectorHeader();
                var    teleDiskData   = new DataHeader();
                var    dataSizeBytes  = new byte[2];
                byte[] decodedData;

                teleDiskSector.Cylinder     = (byte)stream.ReadByte();
                teleDiskSector.Head         = (byte)stream.ReadByte();
                teleDiskSector.SectorNumber = (byte)stream.ReadByte();
                teleDiskSector.SectorSize   = (byte)stream.ReadByte();
                teleDiskSector.Flags        = (byte)stream.ReadByte();
                teleDiskSector.Crc          = (byte)stream.ReadByte();

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Sector_follows);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.AddressMark_cylinder_0,
                                           teleDiskSector.Cylinder);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.AddressMark_head_0, teleDiskSector.Head);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.AddressMark_sector_number_0,
                                           teleDiskSector.SectorNumber);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Sector_size_0, teleDiskSector.SectorSize);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Sector_flags_0_X2, teleDiskSector.Flags);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.Sector_CRC_plus_headers_0_X2,
                                           teleDiskSector.Crc);

                var lba = (uint)(teleDiskSector.Cylinder * _header.Sides * _imageInfo.SectorsPerTrack +
                                 teleDiskSector.Head     * _imageInfo.SectorsPerTrack                 +
                                 (teleDiskSector.SectorNumber - 1));

                if((teleDiskSector.Flags & FLAGS_SECTOR_DATALESS) != FLAGS_SECTOR_DATALESS &&
                   (teleDiskSector.Flags & FLAGS_SECTOR_SKIPPED)  != FLAGS_SECTOR_SKIPPED)
                {
                    stream.EnsureRead(dataSizeBytes, 0, 2);
                    teleDiskData.DataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                    teleDiskData.DataSize--; // Sydex decided to including dataEncoding byte as part of it
                    _imageInfo.ImageSize      += teleDiskData.DataSize;
                    teleDiskData.DataEncoding =  (byte)stream.ReadByte();
                    var data = new byte[teleDiskData.DataSize];
                    stream.EnsureRead(data, 0, teleDiskData.DataSize);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "\t\t" + Localization.Data_size_in_image_0,
                                               teleDiskData.DataSize);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "\t\t" + Localization.Data_encoding_0_X2,
                                               teleDiskData.DataEncoding);

                    ErrorNumber errno = DecodeTeleDiskData(teleDiskSector.SectorSize,
                                                           teleDiskData.DataEncoding,
                                                           data,
                                                           out decodedData);

                    if(errno != ErrorNumber.NoError) return errno;

                    var tdSectorCalculatedCrc = (byte)(TeleDiskCrc(0, decodedData) & 0xFF);

                    if(tdSectorCalculatedCrc != teleDiskSector.Crc)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization
                                                      .Sector_0_3_4_calculated_CRC_1_X2_differs_from_stored_CRC_2_X2,
                                                   teleDiskTrack.Cylinder,
                                                   tdSectorCalculatedCrc,
                                                   teleDiskSector.Crc,
                                                   teleDiskTrack.Cylinder,
                                                   teleDiskSector.SectorNumber);

                        if((teleDiskSector.Flags & FLAGS_SECTOR_NO_ID) != FLAGS_SECTOR_NO_ID)
                            _sectorsWhereCrcHasFailed.Add(lba);
                    }
                }
                else
                    decodedData = new byte[128 << teleDiskSector.SectorSize];

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.LBA_0, lba);

                if((teleDiskSector.Flags & FLAGS_SECTOR_NO_ID) == FLAGS_SECTOR_NO_ID) continue;

                if(_sectorsData[teleDiskTrack.Cylinder][teleDiskTrack.Head][teleDiskSector.SectorNumber] != null)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               (teleDiskSector.Flags & FLAGS_SECTOR_DUPLICATE) == FLAGS_SECTOR_DUPLICATE
                                                   ? "\t\t" +
                                                     Localization
                                                        .Sector_0_on_cylinder_1_head_2_is_duplicate_and_marked_so
                                                   : "\t\t" +
                                                     Localization
                                                        .Sector_0_on_cylinder_1_head_2_is_duplicate_but_is_not_marked_so,
                                               teleDiskSector.SectorNumber,
                                               teleDiskSector.Cylinder,
                                               teleDiskSector.Head);
                }
                else
                {
                    _sectorsData[teleDiskTrack.Cylinder][teleDiskTrack.Head][teleDiskSector.SectorNumber] = decodedData;

                    _totalDiskSize += (uint)decodedData.Length;
                }
            }
        }

        var leadOutMs = new MemoryStream();

        if(hasLeadOutOnHead0)
        {
            for(var i = 0; i < _sectorsData[totalCylinders - 1][0].Length; i++)
            {
                if(_sectorsData[totalCylinders - 1][0][i] != null)
                {
                    leadOutMs.Write(_sectorsData[totalCylinders - 1][0][i],
                                    0,
                                    _sectorsData[totalCylinders - 1][0][i].Length);
                }
            }
        }

        if(hasLeadOutOnHead1)
        {
            for(var i = 0; i < _sectorsData[totalCylinders - 1][1].Length; i++)
            {
                if(_sectorsData[totalCylinders - 1][1][i] != null)
                {
                    leadOutMs.Write(_sectorsData[totalCylinders - 1][1][i],
                                    0,
                                    _sectorsData[totalCylinders - 1][1][i].Length);
                }
            }
        }

        if(leadOutMs.Length != 0)
        {
            _leadOut = leadOutMs.ToArray();
            _imageInfo.ReadableMediaTags.Add(MediaTagType.Floppy_LeadOut);
        }

        _imageInfo.Sectors   = _imageInfo.Cylinders * _imageInfo.Heads * _imageInfo.SectorsPerTrack;
        _imageInfo.MediaType = DecodeTeleDiskDiskType();

        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

        AaruConsole.VerboseWriteLine(Localization.TeleDisk_image_contains_a_disk_of_type_0, _imageInfo.MediaType);

        if(!string.IsNullOrEmpty(_imageInfo.Comments))
            AaruConsole.VerboseWriteLine(Localization.TeleDisk_comments_0, _imageInfo.Comments);

        _inStream.Dispose();
        stream.Dispose();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer                                    = null;
        (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

        if(cylinder >= _sectorsData.Length) return ErrorNumber.SectorNotFound;

        if(head >= _sectorsData[cylinder].Length) return ErrorNumber.SectorNotFound;

        if(sector > _sectorsData[cylinder][head].Length) return ErrorNumber.SectorNotFound;

        buffer = _sectorsData[cylinder][head][sector];

        return ErrorNumber.NoError;
    }

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

            // Sector not in image, TODO for 6.0 return NotDumped status
            if(sector is null) continue;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) =>
        ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer) =>
        ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(tag != MediaTagType.Floppy_LeadOut) return ErrorNumber.NotSupported;

        buffer = _leadOut?.Clone() as byte[];

        return buffer != null ? ErrorNumber.NoError : ErrorNumber.NoData;
    }

#endregion
}