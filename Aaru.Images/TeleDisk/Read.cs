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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Compression;
using Aaru.Console;

namespace Aaru.DiscImages
{
    public sealed partial class TeleDisk
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            _header = new Header();
            byte[] headerBytes = new byte[12];
            _inStream = imageFilter.GetDataForkStream();
            var stream = new MemoryStream();
            _inStream.Seek(0, SeekOrigin.Begin);

            _inStream.Read(headerBytes, 0, 12);
            stream.Write(headerBytes, 0, 12);

            _header.Signature = BitConverter.ToUInt16(headerBytes, 0);

            if(_header.Signature != TD_MAGIC &&
               _header.Signature != TD_ADV_COMP_MAGIC)
                return false;

            _header.Sequence      = headerBytes[2];
            _header.DiskSet       = headerBytes[3];
            _header.Version       = headerBytes[4];
            _header.DataRate      = headerBytes[5];
            _header.DriveType     = headerBytes[6];
            _header.Stepping      = headerBytes[7];
            _header.DosAllocation = headerBytes[8];
            _header.Sides         = headerBytes[9];
            _header.Crc           = BitConverter.ToUInt16(headerBytes, 10);

            _imageInfo.MediaTitle  = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            _imageInfo.Version     = $"{(_header.Version & 0xF0) >> 4}.{_header.Version & 0x0F}";
            _imageInfo.Application = _imageInfo.Version;

            byte[] headerBytesForCrc = new byte[10];
            Array.Copy(headerBytes, headerBytesForCrc, 10);
            ushort calculatedHeaderCrc = TeleDiskCrc(0x0000, headerBytesForCrc);

            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.signature = 0x{0:X4}", _header.Signature);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.sequence = 0x{0:X2}", _header.Sequence);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.diskSet = 0x{0:X2}", _header.DiskSet);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.version = 0x{0:X2}", _header.Version);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.dataRate = 0x{0:X2}", _header.DataRate);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.driveType = 0x{0:X2}", _header.DriveType);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.stepping = 0x{0:X2}", _header.Stepping);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.dosAllocation = 0x{0:X2}", _header.DosAllocation);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.sides = 0x{0:X2}", _header.Sides);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "header.crc = 0x{0:X4}", _header.Crc);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "calculated header crc = 0x{0:X4}", calculatedHeaderCrc);

            // We need more checks as the magic is too simply.
            // This may deny legal images

            // That would be much of a coincidence
            if(_header.Crc != calculatedHeaderCrc)
            {
                _aDiskCrcHasFailed = true;
                AaruConsole.DebugWriteLine("TeleDisk plugin", "Calculated CRC does not coincide with stored one.");
            }

            if(_header.Sequence != 0x00)
                return false;

            if(_header.DataRate != DATA_RATE_250_KBPS &&
               _header.DataRate != DATA_RATE_300_KBPS &&
               _header.DataRate != DATA_RATE_500_KBPS)
                return false;

            if(_header.DriveType != DRIVE_TYPE_35_DD          &&
               _header.DriveType != DRIVE_TYPE_35_ED          &&
               _header.DriveType != DRIVE_TYPE_35_HD          &&
               _header.DriveType != DRIVE_TYPE_525_DD         &&
               _header.DriveType != DRIVE_TYPE_525_HD         &&
               _header.DriveType != DRIVE_TYPE_525_HD_DD_DISK &&
               _header.DriveType != DRIVE_TYPE_8_INCH)
                return false;

            if(_header.Signature == TD_ADV_COMP_MAGIC)
            {
                int rd;
                _inStream.Seek(12, SeekOrigin.Begin);
                stream.Seek(12, SeekOrigin.Begin);
                var lzh = new TeleDiskLzh(_inStream);

                do
                    if((rd = lzh.Decode(out byte[] obuf, BUFSZ)) > 0)
                        stream.Write(obuf, 0, rd);
                while(rd == BUFSZ);
            }
            else
            {
                // Not using Stream.CopyTo() because it's failing with LZIP
                byte[] copybuf = new byte[_inStream.Length];
                _inStream.Seek(0, SeekOrigin.Begin);
                _inStream.Read(copybuf, 0, copybuf.Length);
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(copybuf, 0, copybuf.Length);
            }

            stream.Seek(12, SeekOrigin.Begin);

            _imageInfo.CreationTime = DateTime.MinValue;

            if((_header.Stepping & COMMENT_BLOCK_PRESENT) == COMMENT_BLOCK_PRESENT)
            {
                _commentHeader = new CommentBlockHeader();

                byte[] commentHeaderBytes = new byte[10];

                stream.Read(commentHeaderBytes, 0, 10);
                _commentHeader.Crc    = BitConverter.ToUInt16(commentHeaderBytes, 0);
                _commentHeader.Length = BitConverter.ToUInt16(commentHeaderBytes, 2);
                _commentHeader.Year   = commentHeaderBytes[4];
                _commentHeader.Month  = commentHeaderBytes[5];
                _commentHeader.Day    = commentHeaderBytes[6];
                _commentHeader.Hour   = commentHeaderBytes[7];
                _commentHeader.Minute = commentHeaderBytes[8];
                _commentHeader.Second = commentHeaderBytes[9];

                _commentBlock = new byte[_commentHeader.Length];
                stream.Read(_commentBlock, 0, _commentHeader.Length);

                byte[] commentBlockForCrc = new byte[_commentHeader.Length + 8];
                Array.Copy(commentHeaderBytes, 2, commentBlockForCrc, 0, 8);
                Array.Copy(_commentBlock, 0, commentBlockForCrc, 8, _commentHeader.Length);

                ushort cmtcrc = TeleDiskCrc(0, commentBlockForCrc);

                AaruConsole.DebugWriteLine("TeleDisk plugin", "Comment header");
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.crc = 0x{0:X4}", _commentHeader.Crc);
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tCalculated CRC = 0x{0:X4}", cmtcrc);

                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.length = {0} bytes",
                                           _commentHeader.Length);

                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.year = {0}", _commentHeader.Year);
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.month = {0}", _commentHeader.Month);
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.day = {0}", _commentHeader.Day);
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.hour = {0}", _commentHeader.Hour);
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.minute = {0}", _commentHeader.Minute);
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.second = {0}", _commentHeader.Second);

                _aDiskCrcHasFailed |= cmtcrc != _commentHeader.Crc;

                for(int i = 0; i < _commentBlock.Length; i++)

                    // Replace NULLs, used by TeleDisk as newline markers, with UNIX newline marker
                    if(_commentBlock[i] == 0x00)
                        _commentBlock[i] = 0x0A;

                _imageInfo.Comments = Encoding.ASCII.GetString(_commentBlock);

                AaruConsole.DebugWriteLine("TeleDisk plugin", "Comment");
                AaruConsole.DebugWriteLine("TeleDisk plugin", "{0}", _imageInfo.Comments);

                _imageInfo.CreationTime = new DateTime(_commentHeader.Year + 1900, _commentHeader.Month + 1,
                                                       _commentHeader.Day, _commentHeader.Hour, _commentHeader.Minute,
                                                       _commentHeader.Second, DateTimeKind.Unspecified);
            }

            if(_imageInfo.CreationTime == DateTime.MinValue)
                _imageInfo.CreationTime = imageFilter.GetCreationTime();

            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

            AaruConsole.DebugWriteLine("TeleDisk plugin", "Image created on {0}", _imageInfo.CreationTime);
            AaruConsole.DebugWriteLine("TeleDisk plugin", "Image modified on {0}", _imageInfo.LastModificationTime);

            AaruConsole.DebugWriteLine("TeleDisk plugin", "Parsing image");

            _totalDiskSize       = 0;
            _imageInfo.ImageSize = 0;

            int  totalCylinders = -1;
            int  totalHeads     = -1;
            int  maxSector      = -1;
            int  totalSectors   = 0;
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

                if(teleDiskTrack.Cylinder > totalCylinders)
                    totalCylinders = teleDiskTrack.Cylinder;

                if(teleDiskTrack.Head > totalHeads)
                    totalHeads = teleDiskTrack.Head;

                if(teleDiskTrack.Sectors == 0xFF) // End of disk image
                    break;

                for(byte processedSectors = 0; processedSectors < teleDiskTrack.Sectors; processedSectors++)
                {
                    var    teleDiskSector = new SectorHeader();
                    var    teleDiskData   = new DataHeader();
                    byte[] dataSizeBytes  = new byte[2];

                    teleDiskSector.Cylinder     = (byte)stream.ReadByte();
                    teleDiskSector.Head         = (byte)stream.ReadByte();
                    teleDiskSector.SectorNumber = (byte)stream.ReadByte();
                    teleDiskSector.SectorSize   = (byte)stream.ReadByte();
                    teleDiskSector.Flags        = (byte)stream.ReadByte();
                    teleDiskSector.Crc          = (byte)stream.ReadByte();

                    if(teleDiskSector.SectorNumber > maxSector)
                        maxSector = teleDiskSector.SectorNumber;

                    if((teleDiskSector.Flags & FLAGS_SECTOR_DATALESS) != FLAGS_SECTOR_DATALESS &&
                       (teleDiskSector.Flags & FLAGS_SECTOR_SKIPPED)  != FLAGS_SECTOR_SKIPPED)
                    {
                        stream.Read(dataSizeBytes, 0, 2);
                        teleDiskData.DataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                        teleDiskData.DataSize--; // Sydex decided to including dataEncoding byte as part of it
                        teleDiskData.DataEncoding = (byte)stream.ReadByte();
                        byte[] data = new byte[teleDiskData.DataSize];
                        stream.Read(data, 0, teleDiskData.DataSize);
                    }

                    if(128 << teleDiskSector.SectorSize < _imageInfo.SectorSize)
                        _imageInfo.SectorSize = (uint)(128 << teleDiskSector.SectorSize);

                    totalSectors++;
                }
            }

            totalCylinders++;
            totalHeads++;

            if(totalCylinders <= 0 ||
               totalHeads     <= 0)
                throw new ImageNotSupportedException("No cylinders or heads found");

            bool hasLeadOutOnHead0 = false;
            bool hasLeadOutOnHead1 = false;
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
                    if(teleDiskTrack.Cylinder + 1 == totalCylinders)
                    {
                        hasLeadOutOnHead0 |= teleDiskTrack.Head == 0;
                        hasLeadOutOnHead1 |= teleDiskTrack.Head == 1;

                        if(_imageInfo.Cylinders == totalCylinders)
                            _imageInfo.Cylinders--;
                    }
                    else
                        _imageInfo.SectorsPerTrack = teleDiskTrack.Sectors;

                for(byte processedSectors = 0; processedSectors < teleDiskTrack.Sectors; processedSectors++)
                {
                    var    teleDiskSector = new SectorHeader();
                    var    teleDiskData   = new DataHeader();
                    byte[] dataSizeBytes  = new byte[2];

                    teleDiskSector.Cylinder     = (byte)stream.ReadByte();
                    teleDiskSector.Head         = (byte)stream.ReadByte();
                    teleDiskSector.SectorNumber = (byte)stream.ReadByte();
                    teleDiskSector.SectorSize   = (byte)stream.ReadByte();
                    teleDiskSector.Flags        = (byte)stream.ReadByte();
                    teleDiskSector.Crc          = (byte)stream.ReadByte();

                    if((teleDiskSector.Flags & FLAGS_SECTOR_DATALESS) == FLAGS_SECTOR_DATALESS ||
                       (teleDiskSector.Flags & FLAGS_SECTOR_SKIPPED)  == FLAGS_SECTOR_SKIPPED)
                        continue;

                    stream.Read(dataSizeBytes, 0, 2);
                    teleDiskData.DataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                    teleDiskData.DataSize--; // Sydex decided to including dataEncoding byte as part of it
                    teleDiskData.DataEncoding = (byte)stream.ReadByte();
                    byte[] data = new byte[teleDiskData.DataSize];
                    stream.Read(data, 0, teleDiskData.DataSize);
                }
            }

            _sectorsData = new byte[totalCylinders][][][];

            // Total sectors per track
            uint[][] spts = new uint[totalCylinders][];

            AaruConsole.DebugWriteLine("TeleDisk plugin",
                                       "Found {0} cylinders and {1} heads with a maximum sector number of {2}",
                                       totalCylinders, totalHeads, maxSector);

            // Create heads
            for(int i = 0; i < totalCylinders; i++)
            {
                _sectorsData[i] = new byte[totalHeads][][];
                spts[i]         = new uint[totalHeads];

                for(int j = 0; j < totalHeads; j++)
                    _sectorsData[i][j] = new byte[maxSector + 1][];
            }

            // Decode the image
            stream.Seek(currentPos, SeekOrigin.Begin);

            while(true)
            {
                var    teleDiskTrack = new TrackHeader();
                byte[] tdTrackForCrc = new byte[3];

                teleDiskTrack.Sectors  = (byte)stream.ReadByte();
                teleDiskTrack.Cylinder = (byte)stream.ReadByte();
                teleDiskTrack.Head     = (byte)stream.ReadByte();
                teleDiskTrack.Crc      = (byte)stream.ReadByte();

                tdTrackForCrc[0] = teleDiskTrack.Sectors;
                tdTrackForCrc[1] = teleDiskTrack.Cylinder;
                tdTrackForCrc[2] = teleDiskTrack.Head;

                byte tdTrackCalculatedCrc = (byte)(TeleDiskCrc(0, tdTrackForCrc) & 0xFF);

                AaruConsole.DebugWriteLine("TeleDisk plugin", "Track follows");
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tTrack cylinder: {0}\t", teleDiskTrack.Cylinder);
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tTrack head: {0}\t", teleDiskTrack.Head);
                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tSectors in track: {0}\t", teleDiskTrack.Sectors);

                AaruConsole.DebugWriteLine("TeleDisk plugin", "\tTrack header CRC: 0x{0:X2} (calculated 0x{1:X2})\t",
                                           teleDiskTrack.Crc, tdTrackCalculatedCrc);

                _aDiskCrcHasFailed |= tdTrackCalculatedCrc != teleDiskTrack.Crc;

                if(teleDiskTrack.Sectors == 0xFF) // End of disk image
                {
                    AaruConsole.DebugWriteLine("TeleDisk plugin", "End of disk image arrived");

                    AaruConsole.DebugWriteLine("TeleDisk plugin", "Total of {0} data sectors, for {1} bytes",
                                               totalSectors, _totalDiskSize);

                    break;
                }

                for(byte processedSectors = 0; processedSectors < teleDiskTrack.Sectors; processedSectors++)
                {
                    var    teleDiskSector = new SectorHeader();
                    var    teleDiskData   = new DataHeader();
                    byte[] dataSizeBytes  = new byte[2];
                    byte[] decodedData;

                    teleDiskSector.Cylinder     = (byte)stream.ReadByte();
                    teleDiskSector.Head         = (byte)stream.ReadByte();
                    teleDiskSector.SectorNumber = (byte)stream.ReadByte();
                    teleDiskSector.SectorSize   = (byte)stream.ReadByte();
                    teleDiskSector.Flags        = (byte)stream.ReadByte();
                    teleDiskSector.Crc          = (byte)stream.ReadByte();

                    AaruConsole.DebugWriteLine("TeleDisk plugin", "\tSector follows");

                    AaruConsole.DebugWriteLine("TeleDisk plugin", "\t\tAddressMark cylinder: {0}",
                                               teleDiskSector.Cylinder);

                    AaruConsole.DebugWriteLine("TeleDisk plugin", "\t\tAddressMark head: {0}", teleDiskSector.Head);

                    AaruConsole.DebugWriteLine("TeleDisk plugin", "\t\tAddressMark sector number: {0}",
                                               teleDiskSector.SectorNumber);

                    AaruConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector size: {0}", teleDiskSector.SectorSize);
                    AaruConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector flags: 0x{0:X2}", teleDiskSector.Flags);

                    AaruConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector CRC (plus headers): 0x{0:X2}",
                                               teleDiskSector.Crc);

                    uint lba = (uint)((teleDiskSector.Cylinder * _header.Sides * _imageInfo.SectorsPerTrack) +
                                      (teleDiskSector.Head     * _imageInfo.SectorsPerTrack)                 +
                                      (teleDiskSector.SectorNumber - 1));

                    if((teleDiskSector.Flags & FLAGS_SECTOR_DATALESS) != FLAGS_SECTOR_DATALESS &&
                       (teleDiskSector.Flags & FLAGS_SECTOR_SKIPPED)  != FLAGS_SECTOR_SKIPPED)
                    {
                        stream.Read(dataSizeBytes, 0, 2);
                        teleDiskData.DataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                        teleDiskData.DataSize--; // Sydex decided to including dataEncoding byte as part of it
                        _imageInfo.ImageSize      += teleDiskData.DataSize;
                        teleDiskData.DataEncoding =  (byte)stream.ReadByte();
                        byte[] data = new byte[teleDiskData.DataSize];
                        stream.Read(data, 0, teleDiskData.DataSize);

                        AaruConsole.DebugWriteLine("TeleDisk plugin", "\t\tData size (in-image): {0}",
                                                   teleDiskData.DataSize);

                        AaruConsole.DebugWriteLine("TeleDisk plugin", "\t\tData encoding: 0x{0:X2}",
                                                   teleDiskData.DataEncoding);

                        decodedData = DecodeTeleDiskData(teleDiskSector.SectorSize, teleDiskData.DataEncoding, data);

                        byte tdSectorCalculatedCrc = (byte)(TeleDiskCrc(0, decodedData) & 0xFF);

                        if(tdSectorCalculatedCrc != teleDiskSector.Crc)
                        {
                            AaruConsole.DebugWriteLine("TeleDisk plugin",
                                                       "Sector {0}:{3}:{4} calculated CRC 0x{1:X2} differs from stored CRC 0x{2:X2}",
                                                       teleDiskTrack.Cylinder, tdSectorCalculatedCrc,
                                                       teleDiskSector.Crc, teleDiskTrack.Cylinder,
                                                       teleDiskSector.SectorNumber);

                            if((teleDiskSector.Flags & FLAGS_SECTOR_NO_ID) != FLAGS_SECTOR_NO_ID)
                                _sectorsWhereCrcHasFailed.Add(lba);
                        }
                    }
                    else
                        decodedData = new byte[128 << teleDiskSector.SectorSize];

                    AaruConsole.DebugWriteLine("TeleDisk plugin", "\t\tLBA: {0}", lba);

                    if((teleDiskSector.Flags & FLAGS_SECTOR_NO_ID) == FLAGS_SECTOR_NO_ID)
                        continue;

                    if(_sectorsData[teleDiskTrack.Cylinder][teleDiskTrack.Head][teleDiskSector.SectorNumber] != null)
                        AaruConsole.DebugWriteLine("TeleDisk plugin",
                                                   (teleDiskSector.Flags & FLAGS_SECTOR_DUPLICATE) ==
                                                   FLAGS_SECTOR_DUPLICATE
                                                       ? "\t\tSector {0} on cylinder {1} head {2} is duplicate, and marked so"
                                                       : "\t\tSector {0} on cylinder {1} head {2} is duplicate, but is not marked so",
                                                   teleDiskSector.SectorNumber, teleDiskSector.Cylinder,
                                                   teleDiskSector.Head);
                    else
                    {
                        _sectorsData[teleDiskTrack.Cylinder][teleDiskTrack.Head][teleDiskSector.SectorNumber] =
                            decodedData;

                        _totalDiskSize += (uint)decodedData.Length;
                    }
                }
            }

            var leadOutMs = new MemoryStream();

            if(hasLeadOutOnHead0)
                for(int i = 0; i < _sectorsData[totalCylinders - 1][0].Length; i++)
                    if(_sectorsData[totalCylinders - 1][0][i] != null)
                        leadOutMs.Write(_sectorsData[totalCylinders - 1][0][i], 0,
                                        _sectorsData[totalCylinders - 1][0][i].Length);

            if(hasLeadOutOnHead1)
                for(int i = 0; i < _sectorsData[totalCylinders - 1][1].Length; i++)
                    if(_sectorsData[totalCylinders - 1][1][i] != null)
                        leadOutMs.Write(_sectorsData[totalCylinders - 1][1][i], 0,
                                        _sectorsData[totalCylinders - 1][1][i].Length);

            if(leadOutMs.Length != 0)
            {
                _leadOut = leadOutMs.ToArray();
                _imageInfo.ReadableMediaTags.Add(MediaTagType.Floppy_LeadOut);
            }

            _imageInfo.Sectors   = _imageInfo.Cylinders * _imageInfo.Heads * _imageInfo.SectorsPerTrack;
            _imageInfo.MediaType = DecodeTeleDiskDiskType();

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            AaruConsole.VerboseWriteLine("TeleDisk image contains a disk of type {0}", _imageInfo.MediaType);

            if(!string.IsNullOrEmpty(_imageInfo.Comments))
                AaruConsole.VerboseWriteLine("TeleDisk comments: {0}", _imageInfo.Comments);

            _inStream.Dispose();
            stream.Dispose();

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress)
        {
            (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

            if(cylinder >= _sectorsData.Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(head >= _sectorsData[cylinder].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sector > _sectorsData[cylinder][head].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            return _sectorsData[cylinder][head][sector];
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var buffer = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i) ?? new byte[_imageInfo.SectorSize];
                buffer.Write(sector, 0, sector.Length);
            }

            return buffer.ToArray();
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length) => ReadSectors(sectorAddress, length);

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(tag != MediaTagType.Floppy_LeadOut)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(_leadOut != null)
                return _leadOut;

            throw new FeatureNotPresentImageException("Lead-out not present in disk image");
        }
    }
}