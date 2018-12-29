// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Compression;
using DiscImageChef.Console;

namespace DiscImageChef.DiscImages
{
    public partial class TeleDisk
    {
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

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(tag != MediaTagType.Floppy_LeadOut)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            if(leadOut != null) return leadOut;

            throw new FeatureNotPresentImageException("Lead-out not present in disk image");
        }
    }
}