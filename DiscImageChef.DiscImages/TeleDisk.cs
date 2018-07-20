// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : TeleDisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Sydex TeleDisk disk images.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Compression;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.DiscImages
{
    // Created following notes from Dave Dunfield
    // http://www.classiccmp.org/dunfield/img54306/td0notes.txt
    public class TeleDisk : IMediaImage
    {
        // "TD" as little endian uint.
        const ushort TD_MAGIC = 0x4454;
        // "td" as little endian uint. Means whole file is compressed (aka Advanced Compression)
        const ushort TD_ADV_COMP_MAGIC = 0x6474;

        // DataRates
        const byte DATA_RATE_250KBPS = 0x00;
        const byte DATA_RATE_300KBPS = 0x01;
        const byte DATA_RATE_500KBPS = 0x02;

        // TeleDisk drive types
        const byte DRIVE_TYPE_525_HD_DD_DISK = 0x00;
        const byte DRIVE_TYPE_525_HD         = 0x01;
        const byte DRIVE_TYPE_525_DD         = 0x02;
        const byte DRIVE_TYPE_35_DD          = 0x03;
        const byte DRIVE_TYPE_35_HD          = 0x04;
        const byte DRIVE_TYPE_8_INCH         = 0x05;
        const byte DRIVE_TYPE_35_ED          = 0x06;

        // Stepping
        const byte STEPPING_SINGLE    = 0x00;
        const byte STEPPING_DOUBLE    = 0x01;
        const byte STEPPING_EVEN_ONLY = 0x02;
        // If this bit is set, there is a comment block
        const byte COMMENT_BLOCK_PRESENT = 0x80;

        // CRC polynomial
        const ushort TELE_DISK_CRC_POLY = 0xA097;

        // Sector sizes table
        const byte SECTOR_SIZE_128 = 0x00;
        const byte SECTOR_SIZE_256 = 0x01;
        const byte SECTOR_SIZE_512 = 0x02;
        const byte SECTOR_SIZE_1K  = 0x03;
        const byte SECTOR_SIZE_2K  = 0x04;
        const byte SECTOR_SIZE_4K  = 0x05;
        const byte SECTOR_SIZE_8K  = 0x06;

        // Flags
        // Address mark repeats inside same track
        const byte FLAGS_SECTOR_DUPLICATE = 0x01;
        // Sector gave CRC error on reading
        const byte FLAGS_SECTOR_CRC_ERROR = 0x02;
        // Address mark indicates deleted sector
        const byte FLAGS_SECTOR_DELETED = 0x04;
        // Sector skipped as FAT said it's unused
        const byte FLAGS_SECTOR_SKIPPED = 0x10;
        // There was an address mark, but no data following
        const byte FLAGS_SECTOR_DATALESS = 0x20;
        // There was data without address mark
        const byte FLAGS_SECTOR_NO_ID = 0x40;

        // Data block encodings
        // Data is copied as is
        const byte DATA_BLOCK_COPY = 0x00;
        // Data is encoded as a pair of len.value uint16s
        const byte DATA_BLOCK_PATTERN = 0x01;
        // Data is encoded as RLE
        const byte DATA_BLOCK_RLE = 0x02;

        const int                  BUFSZ = 512;
        bool                       aDiskCrcHasFailed;
        byte[]                     commentBlock;
        TeleDiskCommentBlockHeader commentHeader;

        TeleDiskHeader header;
        ImageInfo      imageInfo;
        Stream         inStream;
        byte[]         leadOut;
        // Cylinder by head, sector data matrix
        byte[][][][] sectorsData;
        List<ulong>  sectorsWhereCrcHasFailed;
        // LBA, data
        uint totalDiskSize;

        public TeleDisk()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Application           = "Sydex TeleDisk",
                Comments              = null,
                Creator               = null,
                MediaManufacturer     = null,
                MediaModel            = null,
                MediaSerialNumber     = null,
                MediaBarcode          = null,
                MediaPartNumber       = null,
                MediaSequence         = 0,
                LastMediaSequence     = 0,
                DriveManufacturer     = null,
                DriveModel            = null,
                DriveSerialNumber     = null,
                DriveFirmwareRevision = null
            };
            aDiskCrcHasFailed        = false;
            sectorsWhereCrcHasFailed = new List<ulong>();
        }

        public ImageInfo Info => imageInfo;

        public string Name => "Sydex TeleDisk";
        public Guid   Id   => new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");

        public string Format => "Sydex TeleDisk";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            header = new TeleDiskHeader();
            byte[] headerBytes = new byte[12];
            Stream stream      = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            stream.Read(headerBytes, 0, 12);

            header.Signature = BitConverter.ToUInt16(headerBytes, 0);

            if(header.Signature != TD_MAGIC && header.Signature != TD_ADV_COMP_MAGIC) return false;

            header.Sequence      = headerBytes[2];
            header.DiskSet       = headerBytes[3];
            header.Version       = headerBytes[4];
            header.DataRate      = headerBytes[5];
            header.DriveType     = headerBytes[6];
            header.Stepping      = headerBytes[7];
            header.DosAllocation = headerBytes[8];
            header.Sides         = headerBytes[9];
            header.Crc           = BitConverter.ToUInt16(headerBytes, 10);

            byte[] headerBytesForCrc = new byte[10];
            Array.Copy(headerBytes, headerBytesForCrc, 10);
            ushort calculatedHeaderCrc = TeleDiskCrc(0x0000, headerBytesForCrc);

            DicConsole.DebugWriteLine("TeleDisk plugin", "header.signature = 0x{0:X4}",      header.Signature);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sequence = 0x{0:X2}",       header.Sequence);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.diskSet = 0x{0:X2}",        header.DiskSet);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.version = 0x{0:X2}",        header.Version);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dataRate = 0x{0:X2}",       header.DataRate);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.driveType = 0x{0:X2}",      header.DriveType);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.stepping = 0x{0:X2}",       header.Stepping);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dosAllocation = 0x{0:X2}",  header.DosAllocation);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sides = 0x{0:X2}",          header.Sides);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.crc = 0x{0:X4}",            header.Crc);
            DicConsole.DebugWriteLine("TeleDisk plugin", "calculated header crc = 0x{0:X4}", calculatedHeaderCrc);

            // We need more checks as the magic is too simply.
            // This may deny legal images

            // That would be much of a coincidence
            if(header.Crc == calculatedHeaderCrc) return true;

            if(header.Sequence != 0x00) return false;

            if(header.DataRate != DATA_RATE_250KBPS && header.DataRate != DATA_RATE_300KBPS &&
               header.DataRate != DATA_RATE_500KBPS) return false;

            return header.DriveType == DRIVE_TYPE_35_DD  || header.DriveType == DRIVE_TYPE_35_ED          ||
                   header.DriveType == DRIVE_TYPE_35_HD  || header.DriveType == DRIVE_TYPE_525_DD         ||
                   header.DriveType == DRIVE_TYPE_525_HD || header.DriveType == DRIVE_TYPE_525_HD_DD_DISK ||
                   header.DriveType == DRIVE_TYPE_8_INCH;
        }

        public bool Open(IFilter imageFilter)
        {
            header = new TeleDiskHeader();
            byte[] headerBytes = new byte[12];
            inStream = imageFilter.GetDataForkStream();
            MemoryStream stream = new MemoryStream();
            inStream.Seek(0, SeekOrigin.Begin);

            inStream.Read(headerBytes, 0, 12);
            stream.Write(headerBytes, 0, 12);

            header.Signature = BitConverter.ToUInt16(headerBytes, 0);

            if(header.Signature != TD_MAGIC && header.Signature != TD_ADV_COMP_MAGIC) return false;

            header.Sequence      = headerBytes[2];
            header.DiskSet       = headerBytes[3];
            header.Version       = headerBytes[4];
            header.DataRate      = headerBytes[5];
            header.DriveType     = headerBytes[6];
            header.Stepping      = headerBytes[7];
            header.DosAllocation = headerBytes[8];
            header.Sides         = headerBytes[9];
            header.Crc           = BitConverter.ToUInt16(headerBytes, 10);

            imageInfo.MediaTitle  = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Version     = $"{(header.Version & 0xF0) >> 4}.{header.Version & 0x0F}";
            imageInfo.Application = imageInfo.Version;

            byte[] headerBytesForCrc = new byte[10];
            Array.Copy(headerBytes, headerBytesForCrc, 10);
            ushort calculatedHeaderCrc = TeleDiskCrc(0x0000, headerBytesForCrc);

            DicConsole.DebugWriteLine("TeleDisk plugin", "header.signature = 0x{0:X4}",      header.Signature);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sequence = 0x{0:X2}",       header.Sequence);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.diskSet = 0x{0:X2}",        header.DiskSet);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.version = 0x{0:X2}",        header.Version);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dataRate = 0x{0:X2}",       header.DataRate);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.driveType = 0x{0:X2}",      header.DriveType);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.stepping = 0x{0:X2}",       header.Stepping);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dosAllocation = 0x{0:X2}",  header.DosAllocation);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sides = 0x{0:X2}",          header.Sides);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.crc = 0x{0:X4}",            header.Crc);
            DicConsole.DebugWriteLine("TeleDisk plugin", "calculated header crc = 0x{0:X4}", calculatedHeaderCrc);

            // We need more checks as the magic is too simply.
            // This may deny legal images

            // That would be much of a coincidence
            if(header.Crc != calculatedHeaderCrc)
            {
                aDiskCrcHasFailed = true;
                DicConsole.DebugWriteLine("TeleDisk plugin", "Calculated CRC does not coincide with stored one.");
            }

            if(header.Sequence != 0x00) return false;

            if(header.DataRate != DATA_RATE_250KBPS && header.DataRate != DATA_RATE_300KBPS &&
               header.DataRate != DATA_RATE_500KBPS) return false;

            if(header.DriveType != DRIVE_TYPE_35_DD  && header.DriveType != DRIVE_TYPE_35_ED          &&
               header.DriveType != DRIVE_TYPE_35_HD  && header.DriveType != DRIVE_TYPE_525_DD         &&
               header.DriveType != DRIVE_TYPE_525_HD && header.DriveType != DRIVE_TYPE_525_HD_DD_DISK &&
               header.DriveType != DRIVE_TYPE_8_INCH) return false;

            if(header.Signature == TD_ADV_COMP_MAGIC)
            {
                int rd;
                inStream.Seek(12, SeekOrigin.Begin);
                stream.Seek(12, SeekOrigin.Begin);
                TeleDiskLzh lzh = new TeleDiskLzh(inStream);
                do
                    if((rd = lzh.Decode(out byte[] obuf, BUFSZ)) > 0)
                        stream.Write(obuf, 0, rd);
                while(rd == BUFSZ);
            }
            else
            {
                // Not using Stream.CopyTo() because it's failing with LZIP
                byte[] copybuf = new byte[inStream.Length];
                inStream.Seek(0, SeekOrigin.Begin);
                inStream.Read(copybuf, 0, copybuf.Length);
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(copybuf, 0, copybuf.Length);
            }

            stream.Seek(12, SeekOrigin.Begin);

            imageInfo.CreationTime = DateTime.MinValue;

            if((header.Stepping & COMMENT_BLOCK_PRESENT) == COMMENT_BLOCK_PRESENT)
            {
                commentHeader = new TeleDiskCommentBlockHeader();

                byte[] commentHeaderBytes = new byte[10];

                stream.Read(commentHeaderBytes, 0, 10);
                commentHeader.Crc    = BitConverter.ToUInt16(commentHeaderBytes, 0);
                commentHeader.Length = BitConverter.ToUInt16(commentHeaderBytes, 2);
                commentHeader.Year   = commentHeaderBytes[4];
                commentHeader.Month  = commentHeaderBytes[5];
                commentHeader.Day    = commentHeaderBytes[6];
                commentHeader.Hour   = commentHeaderBytes[7];
                commentHeader.Minute = commentHeaderBytes[8];
                commentHeader.Second = commentHeaderBytes[9];

                commentBlock = new byte[commentHeader.Length];
                stream.Read(commentBlock, 0, commentHeader.Length);

                byte[] commentBlockForCrc = new byte[commentHeader.Length + 8];
                Array.Copy(commentHeaderBytes, 2, commentBlockForCrc, 0, 8);
                Array.Copy(commentBlock,       0, commentBlockForCrc, 8, commentHeader.Length);

                ushort cmtcrc = TeleDiskCrc(0, commentBlockForCrc);

                DicConsole.DebugWriteLine("TeleDisk plugin", "Comment header");
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.crc = 0x{0:X4}", commentHeader.Crc);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tCalculated CRC = 0x{0:X4}",    cmtcrc);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.length = {0} bytes",
                                          commentHeader.Length);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.year = {0}",   commentHeader.Year);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.month = {0}",  commentHeader.Month);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.day = {0}",    commentHeader.Day);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.hour = {0}",   commentHeader.Hour);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.minute = {0}", commentHeader.Minute);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tcommentheader.second = {0}", commentHeader.Second);

                aDiskCrcHasFailed |= cmtcrc != commentHeader.Crc;

                for(int i = 0; i < commentBlock.Length; i++)
                    // Replace NULLs, used by TeleDisk as newline markers, with UNIX newline marker
                    if(commentBlock[i] == 0x00)
                        commentBlock[i] = 0x0A;

                imageInfo.Comments = Encoding.ASCII.GetString(commentBlock);

                DicConsole.DebugWriteLine("TeleDisk plugin", "Comment");
                DicConsole.DebugWriteLine("TeleDisk plugin", "{0}", imageInfo.Comments);

                imageInfo.CreationTime = new DateTime(commentHeader.Year + 1900, commentHeader.Month + 1,
                                                      commentHeader.Day, commentHeader.Hour, commentHeader.Minute,
                                                      commentHeader.Second, DateTimeKind.Unspecified);
            }

            if(imageInfo.CreationTime == DateTime.MinValue) imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

            DicConsole.DebugWriteLine("TeleDisk plugin", "Image created on {0}",  imageInfo.CreationTime);
            DicConsole.DebugWriteLine("TeleDisk plugin", "Image modified on {0}", imageInfo.LastModificationTime);

            DicConsole.DebugWriteLine("TeleDisk plugin", "Parsing image");

            totalDiskSize       = 0;
            imageInfo.ImageSize = 0;

            int  totalCylinders = -1;
            int  totalHeads     = -1;
            int  maxSector      = -1;
            int  totalSectors   = 0;
            long currentPos     = stream.Position;
            imageInfo.SectorSize      = uint.MaxValue;
            imageInfo.SectorsPerTrack = uint.MaxValue;

            // Count cylinders
            while(true)
            {
                TeleDiskTrackHeader teleDiskTrack = new TeleDiskTrackHeader
                {
                    Sectors  = (byte)stream.ReadByte(),
                    Cylinder = (byte)stream.ReadByte(),
                    Head     = (byte)stream.ReadByte(),
                    Crc      = (byte)stream.ReadByte()
                };

                if(teleDiskTrack.Cylinder > totalCylinders) totalCylinders = teleDiskTrack.Cylinder;
                if(teleDiskTrack.Head     > totalHeads) totalHeads         = teleDiskTrack.Head;

                if(teleDiskTrack.Sectors == 0xFF) // End of disk image
                    break;

                for(byte processedSectors = 0; processedSectors < teleDiskTrack.Sectors; processedSectors++)
                {
                    TeleDiskSectorHeader teleDiskSector = new TeleDiskSectorHeader();
                    TeleDiskDataHeader   teleDiskData   = new TeleDiskDataHeader();
                    byte[]               dataSizeBytes  = new byte[2];

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
                        stream.Read(dataSizeBytes, 0, 2);
                        teleDiskData.DataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                        teleDiskData.DataSize--; // Sydex decided to including dataEncoding byte as part of it
                        teleDiskData.DataEncoding = (byte)stream.ReadByte();
                        byte[] data = new byte[teleDiskData.DataSize];
                        stream.Read(data, 0, teleDiskData.DataSize);
                    }

                    if(128 << teleDiskSector.SectorSize < imageInfo.SectorSize)
                        imageInfo.SectorSize = (uint)(128 << teleDiskSector.SectorSize);

                    totalSectors++;
                }
            }

            totalCylinders++;
            totalHeads++;

            if(totalCylinders <= 0 || totalHeads <= 0)
                throw new ImageNotSupportedException("No cylinders or heads found");

            bool hasLeadOutOnHead0 = false;
            bool hasLeadOutOnHead1 = false;
            imageInfo.Cylinders = (ushort)totalCylinders;
            imageInfo.Heads     = (byte)totalHeads;

            // Count sectors per track
            stream.Seek(currentPos, SeekOrigin.Begin);
            while(true)
            {
                TeleDiskTrackHeader teleDiskTrack = new TeleDiskTrackHeader
                {
                    Sectors  = (byte)stream.ReadByte(),
                    Cylinder = (byte)stream.ReadByte(),
                    Head     = (byte)stream.ReadByte(),
                    Crc      = (byte)stream.ReadByte()
                };

                if(teleDiskTrack.Sectors == 0xFF) // End of disk image
                    break;

                if(teleDiskTrack.Sectors < imageInfo.SectorsPerTrack)
                    if(teleDiskTrack.Cylinder + 1 == totalCylinders)
                    {
                        hasLeadOutOnHead0 |= teleDiskTrack.Head == 0;
                        hasLeadOutOnHead1 |= teleDiskTrack.Head == 1;
                        if(imageInfo.Cylinders == totalCylinders) imageInfo.Cylinders--;
                    }
                    else
                        imageInfo.SectorsPerTrack = teleDiskTrack.Sectors;

                for(byte processedSectors = 0; processedSectors < teleDiskTrack.Sectors; processedSectors++)
                {
                    TeleDiskSectorHeader teleDiskSector = new TeleDiskSectorHeader();
                    TeleDiskDataHeader   teleDiskData   = new TeleDiskDataHeader();
                    byte[]               dataSizeBytes  = new byte[2];

                    teleDiskSector.Cylinder     = (byte)stream.ReadByte();
                    teleDiskSector.Head         = (byte)stream.ReadByte();
                    teleDiskSector.SectorNumber = (byte)stream.ReadByte();
                    teleDiskSector.SectorSize   = (byte)stream.ReadByte();
                    teleDiskSector.Flags        = (byte)stream.ReadByte();
                    teleDiskSector.Crc          = (byte)stream.ReadByte();

                    if((teleDiskSector.Flags & FLAGS_SECTOR_DATALESS) == FLAGS_SECTOR_DATALESS ||
                       (teleDiskSector.Flags & FLAGS_SECTOR_SKIPPED)  == FLAGS_SECTOR_SKIPPED) continue;

                    stream.Read(dataSizeBytes, 0, 2);
                    teleDiskData.DataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                    teleDiskData.DataSize--; // Sydex decided to including dataEncoding byte as part of it
                    teleDiskData.DataEncoding = (byte)stream.ReadByte();
                    byte[] data = new byte[teleDiskData.DataSize];
                    stream.Read(data, 0, teleDiskData.DataSize);
                }
            }

            sectorsData = new byte[totalCylinders][][][];
            // Total sectors per track
            uint[][] spts = new uint[totalCylinders][];

            DicConsole.DebugWriteLine("TeleDisk plugin",
                                      "Found {0} cylinders and {1} heads with a maximum sector number of {2}",
                                      totalCylinders, totalHeads, maxSector);

            // Create heads
            for(int i = 0; i < totalCylinders; i++)
            {
                sectorsData[i] = new byte[totalHeads][][];
                spts[i]        = new uint[totalHeads];

                for(int j = 0; j < totalHeads; j++) sectorsData[i][j] = new byte[maxSector + 1][];
            }

            // Decode the image
            stream.Seek(currentPos, SeekOrigin.Begin);
            while(true)
            {
                TeleDiskTrackHeader teleDiskTrack = new TeleDiskTrackHeader();
                byte[]              tdTrackForCrc = new byte[3];

                teleDiskTrack.Sectors  = (byte)stream.ReadByte();
                teleDiskTrack.Cylinder = (byte)stream.ReadByte();
                teleDiskTrack.Head     = (byte)stream.ReadByte();
                teleDiskTrack.Crc      = (byte)stream.ReadByte();

                tdTrackForCrc[0] = teleDiskTrack.Sectors;
                tdTrackForCrc[1] = teleDiskTrack.Cylinder;
                tdTrackForCrc[2] = teleDiskTrack.Head;

                byte tdTrackCalculatedCrc = (byte)(TeleDiskCrc(0, tdTrackForCrc) & 0xFF);

                DicConsole.DebugWriteLine("TeleDisk plugin", "Track follows");
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tTrack cylinder: {0}\t",   teleDiskTrack.Cylinder);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tTrack head: {0}\t",       teleDiskTrack.Head);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tSectors in track: {0}\t", teleDiskTrack.Sectors);
                DicConsole.DebugWriteLine("TeleDisk plugin", "\tTrack header CRC: 0x{0:X2} (calculated 0x{1:X2})\t",
                                          teleDiskTrack.Crc, tdTrackCalculatedCrc);

                aDiskCrcHasFailed |= tdTrackCalculatedCrc != teleDiskTrack.Crc;

                if(teleDiskTrack.Sectors == 0xFF) // End of disk image
                {
                    DicConsole.DebugWriteLine("TeleDisk plugin", "End of disk image arrived");
                    DicConsole.DebugWriteLine("TeleDisk plugin", "Total of {0} data sectors, for {1} bytes",
                                              totalSectors, totalDiskSize);

                    break;
                }

                for(byte processedSectors = 0; processedSectors < teleDiskTrack.Sectors; processedSectors++)
                {
                    TeleDiskSectorHeader teleDiskSector = new TeleDiskSectorHeader();
                    TeleDiskDataHeader   teleDiskData   = new TeleDiskDataHeader();
                    byte[]               dataSizeBytes  = new byte[2];
                    byte[]               decodedData;

                    teleDiskSector.Cylinder     = (byte)stream.ReadByte();
                    teleDiskSector.Head         = (byte)stream.ReadByte();
                    teleDiskSector.SectorNumber = (byte)stream.ReadByte();
                    teleDiskSector.SectorSize   = (byte)stream.ReadByte();
                    teleDiskSector.Flags        = (byte)stream.ReadByte();
                    teleDiskSector.Crc          = (byte)stream.ReadByte();

                    DicConsole.DebugWriteLine("TeleDisk plugin", "\tSector follows");
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tAddressMark cylinder: {0}",
                                              teleDiskSector.Cylinder);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tAddressMark head: {0}", teleDiskSector.Head);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tAddressMark sector number: {0}",
                                              teleDiskSector.SectorNumber);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector size: {0}",
                                              teleDiskSector.SectorSize);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector flags: 0x{0:X2}", teleDiskSector.Flags);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tSector CRC (plus headers): 0x{0:X2}",
                                              teleDiskSector.Crc);

                    uint lba = (uint)(teleDiskSector.Cylinder * header.Sides * imageInfo.SectorsPerTrack +
                                      teleDiskSector.Head                    * imageInfo.SectorsPerTrack +
                                      (teleDiskSector.SectorNumber - 1));
                    if((teleDiskSector.Flags & FLAGS_SECTOR_DATALESS) != FLAGS_SECTOR_DATALESS &&
                       (teleDiskSector.Flags & FLAGS_SECTOR_SKIPPED)  != FLAGS_SECTOR_SKIPPED)
                    {
                        stream.Read(dataSizeBytes, 0, 2);
                        teleDiskData.DataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                        teleDiskData.DataSize--; // Sydex decided to including dataEncoding byte as part of it
                        imageInfo.ImageSize       += teleDiskData.DataSize;
                        teleDiskData.DataEncoding =  (byte)stream.ReadByte();
                        byte[] data = new byte[teleDiskData.DataSize];
                        stream.Read(data, 0, teleDiskData.DataSize);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tData size (in-image): {0}",
                                                  teleDiskData.DataSize);
                        DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tData encoding: 0x{0:X2}",
                                                  teleDiskData.DataEncoding);

                        decodedData = DecodeTeleDiskData(teleDiskSector.SectorSize, teleDiskData.DataEncoding, data);

                        byte tdSectorCalculatedCrc = (byte)(TeleDiskCrc(0, decodedData) & 0xFF);

                        if(tdSectorCalculatedCrc != teleDiskSector.Crc)
                        {
                            DicConsole.DebugWriteLine("TeleDisk plugin",
                                                      "Sector {0}:{3}:{4} calculated CRC 0x{1:X2} differs from stored CRC 0x{2:X2}",
                                                      teleDiskTrack.Cylinder, tdSectorCalculatedCrc, teleDiskSector.Crc,
                                                      teleDiskTrack.Cylinder, teleDiskSector.SectorNumber);
                            if((teleDiskSector.Flags & FLAGS_SECTOR_NO_ID) != FLAGS_SECTOR_NO_ID)
                                sectorsWhereCrcHasFailed.Add(lba);
                        }
                    }
                    else decodedData = new byte[128 << teleDiskSector.SectorSize];

                    DicConsole.DebugWriteLine("TeleDisk plugin", "\t\tLBA: {0}", lba);

                    if((teleDiskSector.Flags & FLAGS_SECTOR_NO_ID) == FLAGS_SECTOR_NO_ID) continue;

                    if(sectorsData[teleDiskTrack.Cylinder][teleDiskTrack.Head][teleDiskSector.SectorNumber] != null)
                        DicConsole.DebugWriteLine("TeleDisk plugin",
                                                  (teleDiskSector.Flags & FLAGS_SECTOR_DUPLICATE) ==
                                                  FLAGS_SECTOR_DUPLICATE
                                                      ? "\t\tSector {0} on cylinder {1} head {2} is duplicate, and marked so"
                                                      : "\t\tSector {0} on cylinder {1} head {2} is duplicate, but is not marked so",
                                                  teleDiskSector.SectorNumber, teleDiskSector.Cylinder,
                                                  teleDiskSector.Head);
                    else
                    {
                        sectorsData[teleDiskTrack.Cylinder][teleDiskTrack.Head][teleDiskSector.SectorNumber] =
                            decodedData;
                        totalDiskSize += (uint)decodedData.Length;
                    }
                }
            }

            MemoryStream leadOutMs = new MemoryStream();
            if(hasLeadOutOnHead0)
                for(int i = 0; i < sectorsData[totalCylinders - 1][0].Length; i++)
                    if(sectorsData[totalCylinders - 1][0][i] != null)
                        leadOutMs.Write(sectorsData[totalCylinders - 1][0][i], 0,
                                        sectorsData[totalCylinders - 1][0][i].Length);
            if(hasLeadOutOnHead1)
                for(int i = 0; i < sectorsData[totalCylinders - 1][1].Length; i++)
                    if(sectorsData[totalCylinders - 1][1][i] != null)
                        leadOutMs.Write(sectorsData[totalCylinders - 1][1][i], 0,
                                        sectorsData[totalCylinders - 1][1][i].Length);

            if(leadOutMs.Length != 0)
            {
                leadOut = leadOutMs.ToArray();
                imageInfo.ReadableMediaTags.Add(MediaTagType.Floppy_LeadOut);
            }

            imageInfo.Sectors   = imageInfo.Cylinders * imageInfo.Heads * imageInfo.SectorsPerTrack;
            imageInfo.MediaType = DecodeTeleDiskDiskType();

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            DicConsole.VerboseWriteLine("TeleDisk image contains a disk of type {0}", imageInfo.MediaType);
            if(!string.IsNullOrEmpty(imageInfo.Comments))
                DicConsole.VerboseWriteLine("TeleDisk comments: {0}", imageInfo.Comments);

            inStream.Dispose();
            stream.Dispose();

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

            if(cylinder >= sectorsData.Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(head >= sectorsData[cylinder].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sector > sectorsData[cylinder][head].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            return sectorsData[cylinder][head][sector];
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i) ?? new byte[imageInfo.SectorSize];
                buffer.Write(sector, 0, sector.Length);
            }

            return buffer.ToArray();
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            return ReadSectors(sectorAddress, length);
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            return !sectorsWhereCrcHasFailed.Contains(sectorAddress);
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++)
                if(sectorsWhereCrcHasFailed.Contains(sectorAddress))
                    failingLbas.Add(sectorAddress);

            return failingLbas.Count <= 0;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifyMediaImage()
        {
            return aDiskCrcHasFailed;
        }

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(tag != MediaTagType.Floppy_LeadOut)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(leadOut != null) return leadOut;

            throw new FeatureNotPresentImageException("Lead-out not present in disk image");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        (ushort cylinder, byte head, byte sector) LbaToChs(ulong lba)
        {
            ushort cylinder = (ushort)(lba                           / (imageInfo.Heads * imageInfo.SectorsPerTrack));
            byte   head     = (byte)(lba / imageInfo.SectorsPerTrack % imageInfo.Heads);
            byte   sector   = (byte)(lba % imageInfo.SectorsPerTrack + 1);

            return (cylinder, head, sector);
        }

        static ushort TeleDiskCrc(ushort crc, byte[] buffer)
        {
            int counter = 0;

            while(counter < buffer.Length)
            {
                crc ^= (ushort)((buffer[counter] & 0xFF) << 8);

                for(int i = 0; i < 8; i++)
                    if((crc & 0x8000) > 0)
                        crc = (ushort)((crc << 1) ^ TELE_DISK_CRC_POLY);
                    else
                        crc = (ushort)(crc << 1);

                counter++;
            }

            return crc;
        }

        static byte[] DecodeTeleDiskData(byte sectorSize, byte encodingType, byte[] encodedData)
        {
            byte[] decodedData;
            switch(sectorSize)
            {
                case SECTOR_SIZE_128:
                    decodedData = new byte[128];
                    break;
                case SECTOR_SIZE_256:
                    decodedData = new byte[256];
                    break;
                case SECTOR_SIZE_512:
                    decodedData = new byte[512];
                    break;
                case SECTOR_SIZE_1K:
                    decodedData = new byte[1024];
                    break;
                case SECTOR_SIZE_2K:
                    decodedData = new byte[2048];
                    break;
                case SECTOR_SIZE_4K:
                    decodedData = new byte[4096];
                    break;
                case SECTOR_SIZE_8K:
                    decodedData = new byte[8192];
                    break;
                default: throw new ImageNotSupportedException($"Sector size {sectorSize} is incorrect.");
            }

            switch(encodingType)
            {
                case DATA_BLOCK_COPY:
                    Array.Copy(encodedData, decodedData, decodedData.Length);
                    break;
                case DATA_BLOCK_PATTERN:
                {
                    int ins  = 0;
                    int outs = 0;
                    while(ins < encodedData.Length)
                    {
                        byte[] repeatValue = new byte[2];

                        ushort repeatNumber = BitConverter.ToUInt16(encodedData, ins);
                        Array.Copy(encodedData, ins + 2, repeatValue, 0, 2);
                        byte[] decodedPiece = new byte[repeatNumber * 2];
                        ArrayHelpers.ArrayFill(decodedPiece, repeatValue);
                        Array.Copy(decodedPiece, 0, decodedData, outs, decodedPiece.Length);
                        ins  += 4;
                        outs += decodedPiece.Length;
                    }

                    DicConsole.DebugWriteLine("TeleDisk plugin", "(Block pattern decoder): Input data size: {0} bytes",
                                              encodedData.Length);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "(Block pattern decoder): Processed input: {0} bytes",
                                              ins);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "(Block pattern decoder): Output data size: {0} bytes",
                                              decodedData.Length);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "(Block pattern decoder): Processed Output: {0} bytes",
                                              outs);
                    break;
                }
                case DATA_BLOCK_RLE:
                {
                    int ins  = 0;
                    int outs = 0;
                    while(ins < encodedData.Length)
                    {
                        byte length;

                        byte encoding = encodedData[ins];
                        if(encoding == 0x00)
                        {
                            length = encodedData[ins + 1];
                            Array.Copy(encodedData, ins + 2, decodedData, outs, length);
                            ins  += 2 + length;
                            outs += length;
                        }
                        else
                        {
                            length = (byte)(encoding * 2);
                            byte   run  = encodedData[ins + 1];
                            byte[] part = new byte[length];
                            Array.Copy(encodedData, ins + 2, part, 0, length);
                            byte[] piece = new byte[length * run];
                            ArrayHelpers.ArrayFill(piece, part);
                            Array.Copy(piece, 0, decodedData, outs, piece.Length);
                            ins  += 2 + length;
                            outs += piece.Length;
                        }
                    }

                    DicConsole.DebugWriteLine("TeleDisk plugin", "(RLE decoder): Input data size: {0} bytes",
                                              encodedData.Length);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "(RLE decoder): Processed input: {0} bytes", ins);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "(RLE decoder): Output data size: {0} bytes",
                                              decodedData.Length);
                    DicConsole.DebugWriteLine("TeleDisk plugin", "(RLE decoder): Processed Output: {0} bytes", outs);

                    break;
                }
                default: throw new ImageNotSupportedException($"Data encoding {encodingType} is incorrect.");
            }

            return decodedData;
        }

        MediaType DecodeTeleDiskDiskType()
        {
            switch(header.DriveType)
            {
                case DRIVE_TYPE_525_DD:
                case DRIVE_TYPE_525_HD_DD_DISK:
                case DRIVE_TYPE_525_HD:
                {
                    switch(totalDiskSize)
                    {
                        case 163840:
                        {
                            // Acorn disk uses 256 bytes/sector
                            return imageInfo.SectorSize == 256
                                       ? MediaType.ACORN_525_SS_DD_40
                                       : MediaType.DOS_525_SS_DD_8;

                            // DOS disks use 512 bytes/sector
                        }
                        case 184320:
                        {
                            // Atari disk uses 256 bytes/sector
                            return imageInfo.SectorSize == 256 ? MediaType.ATARI_525_DD : MediaType.DOS_525_SS_DD_9;

                            // DOS disks use 512 bytes/sector
                        }
                        case 327680:
                        {
                            // Acorn disk uses 256 bytes/sector
                            return imageInfo.SectorSize == 256
                                       ? MediaType.ACORN_525_SS_DD_80
                                       : MediaType.DOS_525_DS_DD_8;

                            // DOS disks use 512 bytes/sector
                        }
                        case 368640:  return MediaType.DOS_525_DS_DD_9;
                        case 1228800: return MediaType.DOS_525_HD;
                        case 102400:  return MediaType.ACORN_525_SS_SD_40;
                        case 204800:  return MediaType.ACORN_525_SS_SD_80;
                        case 655360:  return MediaType.ACORN_525_DS_DD;
                        case 92160:   return MediaType.ATARI_525_SD;
                        case 133120:  return MediaType.ATARI_525_ED;
                        case 1310720: return MediaType.NEC_525_HD;
                        case 1261568: return MediaType.SHARP_525;
                        case 839680:  return MediaType.FDFORMAT_525_DD;
                        case 1304320: return MediaType.ECMA_99_8;
                        case 1223424: return MediaType.ECMA_99_15;
                        case 1061632: return MediaType.ECMA_99_26;
                        case 80384:   return MediaType.ECMA_66;
                        case 325632:  return MediaType.ECMA_70;
                        case 653312:  return MediaType.ECMA_78;
                        case 737280:  return MediaType.ECMA_78_2;
                        default:
                        {
                            DicConsole.DebugWriteLine("TeleDisk plugin", "Unknown 5,25\" disk with {0} bytes",
                                                      totalDiskSize);
                            return MediaType.Unknown;
                        }
                    }
                }
                case DRIVE_TYPE_35_DD:
                case DRIVE_TYPE_35_ED:
                case DRIVE_TYPE_35_HD:
                {
                    switch(totalDiskSize)
                    {
                        case 322560:  return MediaType.Apricot_35;
                        case 327680:  return MediaType.DOS_35_SS_DD_8;
                        case 368640:  return MediaType.DOS_35_SS_DD_9;
                        case 655360:  return MediaType.DOS_35_DS_DD_8;
                        case 737280:  return MediaType.DOS_35_DS_DD_9;
                        case 1474560: return MediaType.DOS_35_HD;
                        case 2949120: return MediaType.DOS_35_ED;
                        case 1720320: return MediaType.DMF;
                        case 1763328: return MediaType.DMF_82;
                        case 1884160: // Irreal size, seen as BIOS with TSR, 23 sectors/track
                        case 1860608: // Real data size, sum of all sectors
                            return MediaType.XDF_35;
                        case 819200:  return MediaType.CBM_35_DD;
                        case 901120:  return MediaType.CBM_AMIGA_35_DD;
                        case 1802240: return MediaType.CBM_AMIGA_35_HD;
                        case 1310720: return MediaType.NEC_35_HD_8;
                        case 1228800: return MediaType.NEC_35_HD_15;
                        case 1261568: return MediaType.SHARP_35;
                        default:
                        {
                            DicConsole.DebugWriteLine("TeleDisk plugin", "Unknown 3,5\" disk with {0} bytes",
                                                      totalDiskSize);
                            return MediaType.Unknown;
                        }
                    }
                }
                case DRIVE_TYPE_8_INCH:
                {
                    switch(totalDiskSize)
                    {
                        case 81664:   return MediaType.IBM23FD;
                        case 242944:  return MediaType.IBM33FD_128;
                        case 287488:  return MediaType.IBM33FD_256;
                        case 306432:  return MediaType.IBM33FD_512;
                        case 499200:  return MediaType.IBM43FD_128;
                        case 574976:  return MediaType.IBM43FD_256;
                        case 995072:  return MediaType.IBM53FD_256;
                        case 1146624: return MediaType.IBM53FD_512;
                        case 1222400: return MediaType.IBM53FD_1024;
                        case 256256:
                            // Same size, with same disk geometry, for DEC RX01, NEC and ECMA, return ECMA
                            return MediaType.ECMA_54;
                        case 512512:
                        {
                            // DEC disk uses 256 bytes/sector
                            return imageInfo.SectorSize == 256 ? MediaType.RX02 : MediaType.ECMA_59;

                            // ECMA disks use 128 bytes/sector
                        }
                        case 1261568: return MediaType.NEC_8_DD;
                        case 1255168: return MediaType.ECMA_69_8;
                        case 1177344: return MediaType.ECMA_69_15;
                        case 1021696: return MediaType.ECMA_69_26;
                        default:
                        {
                            DicConsole.DebugWriteLine("TeleDisk plugin", "Unknown 8\" disk with {0} bytes",
                                                      totalDiskSize);
                            return MediaType.Unknown;
                        }
                    }
                }
                default:
                {
                    DicConsole.DebugWriteLine("TeleDisk plugin", "Unknown drive type {1} with {0} bytes", totalDiskSize,
                                              header.DriveType);
                    return MediaType.Unknown;
                }
            }
        }

        struct TeleDiskHeader
        {
            /// <summary>"TD" or "td" depending on compression</summary>
            public ushort Signature;
            /// <summary>Sequence, but TeleDisk seems to complaing if != 0</summary>
            public byte Sequence;
            /// <summary>Random, same byte for all disks in the same set</summary>
            public byte DiskSet;
            /// <summary>TeleDisk version, major in high nibble, minor in low nibble</summary>
            public byte Version;
            /// <summary>Data rate</summary>
            public byte DataRate;
            /// <summary>BIOS drive type</summary>
            public byte DriveType;
            /// <summary>Stepping used</summary>
            public byte Stepping;
            /// <summary>If set means image only allocates sectors marked in-use by FAT12</summary>
            public byte DosAllocation;
            /// <summary>Sides of disk</summary>
            public byte Sides;
            /// <summary>CRC of all the previous</summary>
            public ushort Crc;
        }

        struct TeleDiskCommentBlockHeader
        {
            /// <summary>CRC of comment block after crc field</summary>
            public ushort Crc;
            /// <summary>Length of comment</summary>
            public ushort Length;
            public byte Year;
            public byte Month;
            public byte Day;
            public byte Hour;
            public byte Minute;
            public byte Second;
        }

        struct TeleDiskTrackHeader
        {
            /// <summary>Sectors in the track, 0xFF if end of disk image (there is no spoon)</summary>
            public byte Sectors;
            /// <summary>Cylinder the head was on</summary>
            public byte Cylinder;
            /// <summary>Head/side used</summary>
            public byte Head;
            /// <summary>Lower byte of CRC of previous fields</summary>
            public byte Crc;
        }

        struct TeleDiskSectorHeader
        {
            /// <summary>Cylinder as stored on sector address mark</summary>
            public byte Cylinder;
            /// <summary>Head as stored on sector address mark</summary>
            public byte Head;
            /// <summary>Sector number as stored on sector address mark</summary>
            public byte SectorNumber;
            /// <summary>Sector size</summary>
            public byte SectorSize;
            /// <summary>Sector flags</summary>
            public byte Flags;
            /// <summary>Lower byte of TeleDisk CRC of sector header, data header and data block</summary>
            public byte Crc;
        }

        struct TeleDiskDataHeader
        {
            /// <summary>Size of all data (encoded) + next field (1)</summary>
            public ushort DataSize;
            /// <summary>Encoding used for data block</summary>
            public byte DataEncoding;
        }
    }
}