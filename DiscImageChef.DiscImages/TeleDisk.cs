/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : TeleDisk.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Disk image plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Manages Sydex TeleDisk disk images.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.IO;
using System.Collections.Generic;

// Created following notes from Dave Dunfield
// http://www.classiccmp.org/dunfield/img54306/td0notes.txt
namespace DiscImageChef.ImagePlugins
{
    class TeleDisk : ImagePlugin
    {
        #region Internal Structures

        struct TD0Header
        {
            // "TD" or "td" depending on compression
            public UInt16 signature;
            // Sequence, but TeleDisk seems to complaing if != 0
            public byte sequence;
            // Random, same byte for all disks in the same set
            public byte diskSet;
            // TeleDisk version, major in high nibble, minor in low nibble
            public byte version;
            // Data rate
            public byte dataRate;
            // BIOS drive type
            public byte driveType;
            // Stepping used
            public byte stepping;
            // If set means image only allocates sectors marked in-use by FAT12
            public byte dosAllocation;
            // Sides of disk
            public byte sides;
            // CRC of all the previous
            public UInt16 crc;
        }

        struct TDCommentBlockHeader
        {
            // CRC of comment block after crc field
            public UInt16 crc;
            // Length of comment
            public UInt16 length;
            public byte year;
            public byte month;
            public byte day;
            public byte hour;
            public byte minute;
            public byte second;
        }

        struct TDTrackHeader
        {
            // Sectors in the track, 0xFF if end of disk image (there is no spoon)
            public byte sectors;
            // Cylinder the head was on
            public byte cylinder;
            // Head/side used
            public byte head;
            // Lower byte of CRC of previous fields
            public byte crc;
        }

        struct TDSectorHeader
        {
            // Cylinder as stored on sector address mark
            public byte cylinder;
            // Head as stored on sector address mark
            public byte head;
            // Sector number as stored on sector address mark
            public byte sectorNumber;
            // Sector size
            public byte sectorSize;
            // Sector flags
            public byte flags;
            // Lower byte of TeleDisk CRC of sector header, data header and data block
            public byte crc;
        }

        struct TDDataHeader
        {
            // Size of all data (encoded) + next field (1)
            public UInt16 dataSize;
            // Encoding used for data block
            public byte dataEncoding;
        }

        #endregion

        #region Internal Constants

        // "TD" as little endian uint.
        const UInt16 tdMagic = 0x4454;
        // "td" as little endian uint. Means whole file is compressed (aka Advanced Compression)
        const UInt16 tdAdvCompMagic = 0x6474;

        // DataRates
        const byte DataRate250kbps = 0x00;
        const byte DataRate300kbps = 0x01;
        const byte DataRate500kbps = 0x02;

        // TeleDisk drive types
        const byte DriveType525HD_DDDisk = 0x00;
        const byte DriveType525HD = 0x01;
        const byte DriveType525DD = 0x02;
        const byte DriveType35DD = 0x03;
        const byte DriveType35HD = 0x04;
        const byte DriveType8inch = 0x05;
        const byte DriveType35ED = 0x06;

        // Stepping
        const byte SteppingSingle = 0x00;
        const byte SteppingDouble = 0x01;
        const byte SteppingEvenOnly = 0x02;
        // If this bit is set, there is a comment block
        const byte CommentBlockPresent = 0x80;

        // CRC polynomial
        const UInt16 TeleDiskCRCPoly = 0xA097;

        // Sector sizes table
        const byte SectorSize128 = 0x00;
        const byte SectorSize256 = 0x01;
        const byte SectorSize512 = 0x02;
        const byte SectorSize1K = 0x03;
        const byte SectorSize2K = 0x04;
        const byte SectorSize4K = 0x05;
        const byte SectorSize8K = 0x06;

        // Flags
        // Address mark repeats inside same track
        const byte FlagsSectorDuplicate = 0x01;
        // Sector gave CRC error on reading
        const byte FlagsSectorCRCError = 0x02;
        // Address mark indicates deleted sector
        const byte FlagsSectorDeleted = 0x04;
        // Sector skipped as FAT said it's unused
        const byte FlagsSectorSkipped = 0x10;
        // There was an address mark, but no data following
        const byte FlagsSectorDataless = 0x20;
        // There was data without address mark
        const byte FlagsSectorNoID = 0x40;

        // Data block encodings
        // Data is copied as is
        const byte dataBlockCopy = 0x00;
        // Data is encoded as a pair of len.value uint16s
        const byte dataBlockPattern = 0x01;
        // Data is encoded as RLE
        const byte dataBlockRLE = 0x02;

        #endregion

        #region Internal variables

        TD0Header header;
        TDCommentBlockHeader commentHeader;
        byte[] commentBlock;
        Dictionary<UInt32, byte[]> sectorsData;
        // LBA, data
        UInt32 totalDiskSize;
        bool ADiskCRCHasFailed;
        List<UInt64> SectorsWhereCRCHasFailed;

        #endregion

        public TeleDisk()
        {
            Name = "Sydex TeleDisk";
            PluginUUID = new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableDiskTags = new List<DiskTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageApplication = "Sydex TeleDisk";
            ImageInfo.imageComments = null;
            ImageInfo.imageCreator = null;
            ImageInfo.diskManufacturer = null;
            ImageInfo.diskModel = null;
            ImageInfo.diskSerialNumber = null;
            ImageInfo.diskBarcode = null;
            ImageInfo.diskPartNumber = null;
            ImageInfo.diskSequence = 0;
            ImageInfo.lastDiskSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
            ADiskCRCHasFailed = false;
            SectorsWhereCRCHasFailed = new List<UInt64>();
        }

        public override bool IdentifyImage(string imagePath)
        {
            header = new TD0Header();
            byte[] headerBytes = new byte[12];
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

            stream.Read(headerBytes, 0, 12);
            stream.Close();

            header.signature = BitConverter.ToUInt16(headerBytes, 0);

            if (header.signature != tdMagic && header.signature != tdAdvCompMagic)
                return false;

            header.sequence = headerBytes[2];
            header.diskSet = headerBytes[3];
            header.version = headerBytes[4];
            header.dataRate = headerBytes[5];
            header.driveType = headerBytes[6];
            header.stepping = headerBytes[7];
            header.dosAllocation = headerBytes[8];
            header.sides = headerBytes[9];
            header.crc = BitConverter.ToUInt16(headerBytes, 10);

            byte[] headerBytesForCRC = new byte[10];
            Array.Copy(headerBytes, headerBytesForCRC, 10);
            UInt16 calculatedHeaderCRC = TeleDiskCRC(0x0000, headerBytesForCRC);

            //if (MainClass.isDebug)
            {
                Console.WriteLine("DEBUG (TeleDisk plugin): header.signature = 0x{0:X4}", header.signature);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.sequence = 0x{0:X2}", header.sequence);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.diskSet = 0x{0:X2}", header.diskSet);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.version = 0x{0:X2}", header.version);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.dataRate = 0x{0:X2}", header.dataRate);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.driveType = 0x{0:X2}", header.driveType);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.stepping = 0x{0:X2}", header.stepping);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.dosAllocation = 0x{0:X2}", header.dosAllocation);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.sides = 0x{0:X2}", header.sides);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.crc = 0x{0:X4}", header.crc);
                Console.WriteLine("DEBUG (TeleDisk plugin): calculated header crc = 0x{0:X4}", calculatedHeaderCRC);
            }

            // We need more checks as the magic is too simply.
            // This may deny legal images

            // That would be much of a coincidence
            if (header.crc == calculatedHeaderCRC)
                return true;

            if (header.sequence != 0x00)
                return false;

            if (header.dataRate != DataRate250kbps && header.dataRate != DataRate300kbps && header.dataRate != DataRate500kbps)
                return false;

            if (header.driveType != DriveType35DD && header.driveType != DriveType35ED && header.driveType != DriveType35HD && header.driveType != DriveType525DD &&
                header.driveType != DriveType525HD && header.driveType != DriveType525HD_DDDisk && header.driveType != DriveType8inch)
                return false;

            return true;
        }

        public override bool OpenImage(string imagePath)
        {
            header = new TD0Header();
            byte[] headerBytes = new byte[12];
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            
            stream.Read(headerBytes, 0, 12);
            
            header.signature = BitConverter.ToUInt16(headerBytes, 0);
            
            if (header.signature != tdMagic && header.signature != tdAdvCompMagic)
                return false;
            
            header.sequence = headerBytes[2];
            header.diskSet = headerBytes[3];
            header.version = headerBytes[4];
            header.dataRate = headerBytes[5];
            header.driveType = headerBytes[6];
            header.stepping = headerBytes[7];
            header.dosAllocation = headerBytes[8];
            header.sides = headerBytes[9];
            header.crc = BitConverter.ToUInt16(headerBytes, 10);

            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imagePath);
            ImageInfo.imageVersion = String.Format("{0}.{1}", (header.version & 0xF0) >> 4, header.version & 0x0F);
            ImageInfo.imageApplication = ImageInfo.imageVersion;

            byte[] headerBytesForCRC = new byte[10];
            Array.Copy(headerBytes, headerBytesForCRC, 10);
            UInt16 calculatedHeaderCRC = TeleDiskCRC(0x0000, headerBytesForCRC);
            
            //if (MainClass.isDebug)
            {
                Console.WriteLine("DEBUG (TeleDisk plugin): header.signature = 0x{0:X4}", header.signature);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.sequence = 0x{0:X2}", header.sequence);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.diskSet = 0x{0:X2}", header.diskSet);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.version = 0x{0:X2}", header.version);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.dataRate = 0x{0:X2}", header.dataRate);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.driveType = 0x{0:X2}", header.driveType);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.stepping = 0x{0:X2}", header.stepping);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.dosAllocation = 0x{0:X2}", header.dosAllocation);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.sides = 0x{0:X2}", header.sides);
                Console.WriteLine("DEBUG (TeleDisk plugin): header.crc = 0x{0:X4}", header.crc);
                Console.WriteLine("DEBUG (TeleDisk plugin): calculated header crc = 0x{0:X4}", calculatedHeaderCRC);
            }
            
            // We need more checks as the magic is too simply.
            // This may deny legal images
            
            // That would be much of a coincidence
            if (header.crc != calculatedHeaderCRC)
            {
                ADiskCRCHasFailed = true;
                //if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (TeleDisk plugin): Calculated CRC does not coincide with stored one.");
            }

            if (header.sequence != 0x00)
                return false;
            
            if (header.dataRate != DataRate250kbps && header.dataRate != DataRate300kbps && header.dataRate != DataRate500kbps)
                return false;
            
            if (header.driveType != DriveType35DD && header.driveType != DriveType35ED && header.driveType != DriveType35HD && header.driveType != DriveType525DD &&
                header.driveType != DriveType525HD && header.driveType != DriveType525HD_DDDisk && header.driveType != DriveType8inch)
                return false;

            if (header.signature == tdAdvCompMagic)
                throw new NotImplementedException("TeleDisk Advanced Compression support not yet implemented");

            ImageInfo.imageCreationTime = DateTime.MinValue;

            if ((header.stepping & CommentBlockPresent) == CommentBlockPresent)
            {
                commentHeader = new TDCommentBlockHeader();

                byte[] commentHeaderBytes = new byte[10];
                byte[] commentBlockForCRC;

                stream.Read(commentHeaderBytes, 0, 10);
                commentHeader.crc = BitConverter.ToUInt16(commentHeaderBytes, 0);
                commentHeader.length = BitConverter.ToUInt16(commentHeaderBytes, 2);
                commentHeader.year = commentHeaderBytes[4];
                commentHeader.month = commentHeaderBytes[5];
                commentHeader.day = commentHeaderBytes[6];
                commentHeader.hour = commentHeaderBytes[7];
                commentHeader.minute = commentHeaderBytes[8];
                commentHeader.second = commentHeaderBytes[9];

                commentBlock = new byte[commentHeader.length];
                stream.Read(commentBlock, 0, commentHeader.length);

                commentBlockForCRC = new byte[commentHeader.length + 8];
                Array.Copy(commentHeaderBytes, 2, commentBlockForCRC, 0, 8);
                Array.Copy(commentBlock, 0, commentBlockForCRC, 8, commentHeader.length);

                UInt16 cmtcrc = TeleDiskCRC(0, commentBlockForCRC);

                //if (MainClass.isDebug)
                {
                    Console.WriteLine("DEBUG (TeleDisk plugin): Comment header");
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tcommentheader.crc = 0x{0:X4}", commentHeader.crc);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tCalculated CRC = 0x{0:X4}", cmtcrc);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tcommentheader.length = {0} bytes", commentHeader.length);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tcommentheader.year = {0}", commentHeader.year);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tcommentheader.month = {0}", commentHeader.month);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tcommentheader.day = {0}", commentHeader.day);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tcommentheader.hour = {0}", commentHeader.hour);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tcommentheader.minute = {0}", commentHeader.minute);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tcommentheader.second = {0}", commentHeader.second);
                }

                ADiskCRCHasFailed |= cmtcrc != commentHeader.crc;

                for (int i = 0; i < commentBlock.Length; i++)
                {
                    // Replace NULLs, used by TeleDisk as newline markers, with UNIX newline marker
                    if (commentBlock[i] == 0x00)
                        commentBlock[i] = 0x0A;
                }

                ImageInfo.imageComments = System.Text.Encoding.ASCII.GetString(commentBlock);

                //if (MainClass.isDebug)
                {
                    Console.WriteLine("DEBUG (TeleDisk plugin): Comment");
                    Console.WriteLine("DEBUG (TeleDisk plugin): {0}", ImageInfo.imageComments);
                }

                ImageInfo.imageCreationTime = new DateTime(commentHeader.year + 1900, commentHeader.month + 1, commentHeader.day,
                    commentHeader.hour, commentHeader.minute, commentHeader.second, DateTimeKind.Unspecified);
            }

            FileInfo fi = new FileInfo(imagePath);
            if (ImageInfo.imageCreationTime == DateTime.MinValue)
                ImageInfo.imageCreationTime = fi.CreationTimeUtc;
            ImageInfo.imageLastModificationTime = fi.LastWriteTimeUtc;

            //if (MainClass.isDebug)
            {
                Console.WriteLine("DEBUG (TeleDisk plugin): Image created on {0}", ImageInfo.imageCreationTime);
                Console.WriteLine("DEBUG (TeleDisk plugin): Image modified on {0}", ImageInfo.imageLastModificationTime);
            }

            //if (MainClass.isDebug)
                Console.WriteLine("DEBUG (TeleDisk plugin): Parsing image");

            totalDiskSize = 0;
            byte spt = 0;
            ImageInfo.imageSize = 0;
            sectorsData = new Dictionary<uint, byte[]>();
            ImageInfo.sectorSize = 0;
            while (true)
            {
                TDTrackHeader TDTrack = new TDTrackHeader();
                byte[] TDTrackForCRC = new byte[3];
                byte TDTrackCalculatedCRC;

                TDTrack.sectors = (byte)stream.ReadByte();
                TDTrack.cylinder = (byte)stream.ReadByte();
                TDTrack.head = (byte)stream.ReadByte();
                TDTrack.crc = (byte)stream.ReadByte();

                TDTrackForCRC[0] = TDTrack.sectors;
                TDTrackForCRC[1] = TDTrack.cylinder;
                TDTrackForCRC[2] = TDTrack.head;

                TDTrackCalculatedCRC = (byte)(TeleDiskCRC(0, TDTrackForCRC) & 0xFF);

                //if (MainClass.isDebug)
                {
                    Console.WriteLine("DEBUG (TeleDisk plugin): Track follows");
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tTrack cylinder: {0}\t", TDTrack.cylinder);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tTrack head: {0}\t", TDTrack.head);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tSectors in track: {0}\t", TDTrack.sectors);
                    Console.WriteLine("DEBUG (TeleDisk plugin): \tTrack header CRC: 0x{0:X2} (calculated 0x{1:X2})\t", TDTrack.crc, TDTrackCalculatedCRC);
                }

                ADiskCRCHasFailed |= TDTrackCalculatedCRC != TDTrack.crc;

                if (TDTrack.sectors == 0xFF) // End of disk image
                {
                    //if (MainClass.isDebug)
                    {
                        Console.WriteLine("DEBUG (TeleDisk plugin): End of disk image arrived");
                        Console.WriteLine("DEBUG (TeleDisk plugin): Total of {0} data sectors, for {1} bytes", sectorsData.Count, totalDiskSize);
                    }

                    break;
                }

                if (spt != TDTrack.sectors && TDTrack.sectors > 0)
                {
                    if (spt != 0)
                        throw new FeatureUnsupportedImageException("Variable number of sectors per track. This kind of image is not yet supported");
                    spt = TDTrack.sectors;
                }

                for (byte processedSectors = 0; processedSectors < TDTrack.sectors; processedSectors++)
                {
                    TDSectorHeader TDSector = new TDSectorHeader();
                    TDDataHeader TDData = new TDDataHeader();
                    byte[] dataSizeBytes = new byte[2];
                    byte[] data;
                    byte[] decodedData;

                    TDSector.cylinder = (byte)stream.ReadByte();
                    TDSector.head = (byte)stream.ReadByte();
                    TDSector.sectorNumber = (byte)stream.ReadByte();
                    TDSector.sectorSize = (byte)stream.ReadByte();
                    TDSector.flags = (byte)stream.ReadByte();
                    TDSector.crc = (byte)stream.ReadByte();

                    //if (MainClass.isDebug)
                    {
                        Console.WriteLine("DEBUG (TeleDisk plugin): \tSector follows");
                        Console.WriteLine("DEBUG (TeleDisk plugin): \t\tAddressMark cylinder: {0}", TDSector.cylinder);
                        Console.WriteLine("DEBUG (TeleDisk plugin): \t\tAddressMark head: {0}", TDSector.head);
                        Console.WriteLine("DEBUG (TeleDisk plugin): \t\tAddressMark sector number: {0}", TDSector.sectorNumber);
                        Console.WriteLine("DEBUG (TeleDisk plugin): \t\tSector size: {0}", TDSector.sectorSize);
                        Console.WriteLine("DEBUG (TeleDisk plugin): \t\tSector flags: 0x{0:X2}", TDSector.flags);
                        Console.WriteLine("DEBUG (TeleDisk plugin): \t\tSector CRC (plus headers): 0x{0:X2}", TDSector.crc);
                    }

                    UInt32 LBA = (uint)((TDSector.cylinder * header.sides * spt) + (TDSector.head * spt) + (TDSector.sectorNumber - 1));
                    if ((TDSector.flags & FlagsSectorDataless) != FlagsSectorDataless && (TDSector.flags & FlagsSectorSkipped) != FlagsSectorSkipped)
                    {
                        stream.Read(dataSizeBytes, 0, 2);
                        TDData.dataSize = BitConverter.ToUInt16(dataSizeBytes, 0);
                        TDData.dataSize--; // Sydex decided to including dataEncoding byte as part of it
                        ImageInfo.imageSize += TDData.dataSize;
                        TDData.dataEncoding = (byte)stream.ReadByte();
                        data = new byte[TDData.dataSize];
                        stream.Read(data, 0, TDData.dataSize);
                        //if (MainClass.isDebug)
                        {
                            Console.WriteLine("DEBUG (TeleDisk plugin): \t\tData size (in-image): {0}", TDData.dataSize);
                            Console.WriteLine("DEBUG (TeleDisk plugin): \t\tData encoding: 0x{0:X2}", TDData.dataEncoding);
                        }

                        decodedData = DecodeTeleDiskData(TDSector.sectorSize, TDData.dataEncoding, data);

                        byte TDSectorCalculatedCRC = (byte)(TeleDiskCRC(0, decodedData) & 0xFF);

                        if (TDSectorCalculatedCRC != TDSector.crc)
                        {
                            //if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (TeleDisk plugin): Sector LBA {0} calculated CRC 0x{1:X2} differs from stored CRC 0x{2:X2}", LBA, TDSectorCalculatedCRC, TDSector.crc);
                            if ((TDSector.flags & FlagsSectorNoID) != FlagsSectorNoID)
                            if (!sectorsData.ContainsKey(LBA) && (TDSector.flags & FlagsSectorDuplicate) != FlagsSectorDuplicate)
                                SectorsWhereCRCHasFailed.Add((UInt64)LBA);
                        }
                    }
                    else
                    {
                        switch (TDSector.sectorSize)
                        {
                            case SectorSize128:
                                decodedData = new byte[128];
                                break;
                            case SectorSize256:
                                decodedData = new byte[256];
                                break;
                            case SectorSize512:
                                decodedData = new byte[512];
                                break;
                            case SectorSize1K:
                                decodedData = new byte[1024];
                                break;
                            case SectorSize2K:
                                decodedData = new byte[2048];
                                break;
                            case SectorSize4K:
                                decodedData = new byte[4096];
                                break;
                            case SectorSize8K:
                                decodedData = new byte[8192];
                                break;
                            default:
                                throw new ImageNotSupportedException(String.Format("Sector size {0} for cylinder {1} head {2} sector {3} is incorrect.",
                                    TDSector.sectorSize, TDSector.cylinder, TDSector.head, TDSector.sectorNumber));
                        }
                        ArrayHelpers.ArrayFill(decodedData, (byte)0);
                    }
                    //if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (TeleDisk plugin): \t\tLBA: {0}", LBA);

                    if ((TDSector.flags & FlagsSectorNoID) != FlagsSectorNoID)
                    {
                        if (sectorsData.ContainsKey(LBA))
                        {
                            if ((TDSector.flags & FlagsSectorDuplicate) == FlagsSectorDuplicate)
                            {
                                //if (MainClass.isDebug)
                                    Console.WriteLine("DEBUG (TeleDisk plugin): \t\tSector {0} on cylinder {1} head {2} is duplicate, and marked so",
                                        TDSector.sectorNumber, TDSector.cylinder, TDSector.head);
                            }
                            else
                            {
                                //if (MainClass.isDebug)
                                    Console.WriteLine("DEBUG (TeleDisk plugin): \t\tSector {0} on cylinder {1} head {2} is duplicate, but is not marked so",
                                        TDSector.sectorNumber, TDSector.cylinder, TDSector.head);
                            }
                        }
                        else
                        {
                            sectorsData.Add(LBA, decodedData);
                            totalDiskSize += (uint)decodedData.Length;
                        }
                    }
                    if (decodedData.Length > ImageInfo.sectorSize)
                        ImageInfo.sectorSize = (uint)decodedData.Length;
                }
            }

            ImageInfo.sectors = (ulong)sectorsData.Count;
            ImageInfo.diskType = DecodeTeleDiskDiskType();

            stream.Close();
            return true;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.imageHasPartitions;
        }

        public override UInt64 GetImageSize()
        {
            return ImageInfo.imageSize;
        }

        public override UInt64 GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override UInt32 GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override byte[] ReadSector(UInt64 sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length)
        {
            if (sectorAddress > (ulong)sectorsData.Count - 1)
                throw new ArgumentOutOfRangeException("sectorAddress", "Sector address not found");

            if (sectorAddress + length > (ulong)sectorsData.Count)
                throw new ArgumentOutOfRangeException("length", "Requested more sectors than available");

            byte[] data = new byte[1]; // To make compiler happy
            bool first = true;
            int dataPosition = 0;

            for (ulong i = sectorAddress; i < (sectorAddress + length); i++)
            {
                if (!sectorsData.ContainsKey((uint)i))
                    throw new ImageNotSupportedException(String.Format("Requested sector {0} not found", i));

                byte[] sector;

                if (!sectorsData.TryGetValue((uint)i, out sector))
                    throw new ImageNotSupportedException(String.Format("Error reading sector {0}", i));

                if (first)
                {
                    data = new byte[sector.Length];
                    Array.Copy(sector, data, sector.Length);
                    first = false;
                }
                else
                {
                    Array.Resize(ref data, dataPosition + sector.Length);
                    Array.Copy(sector, 0, data, dataPosition, sector.Length);
                }
                dataPosition += sector.Length;
            }

            return data;
        }

        public override byte[] ReadSectorLong(UInt64 sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length)
        {
            return ReadSectors(sectorAddress, length);
        }

        public override string   GetImageFormat()
        { 
            return "Sydex TeleDisk";
        }

        public override string   GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string   GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override string   GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string   GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override DiskType GetDiskType()
        {
            return ImageInfo.diskType;
        }

        public override bool? VerifySector(UInt64 sectorAddress)
        {
            return !SectorsWhereCRCHasFailed.Contains(sectorAddress);
        }

        public override bool? VerifySector(UInt64 sectorAddress, UInt32 track)
        {
            return null;
        }

        public override bool? VerifySectors(UInt64 sectorAddress, UInt32 length, out List<UInt64> FailingLBAs, out List<UInt64> UnknownLBAs)
        {
            FailingLBAs = new List<UInt64>();
            UnknownLBAs = new List<UInt64>();

            for (UInt64 i = sectorAddress; i < sectorAddress + length; i++)
                if (SectorsWhereCRCHasFailed.Contains(sectorAddress))
                    FailingLBAs.Add(sectorAddress);

            return FailingLBAs.Count <= 0;
        }

        public override bool? VerifySectors(UInt64 sectorAddress, UInt32 length, UInt32 track, out List<UInt64> FailingLBAs, out List<UInt64> UnknownLBAs)
        {
            FailingLBAs = new List<UInt64>();
            UnknownLBAs = new List<UInt64>();

            for (UInt64 i = sectorAddress; i < sectorAddress + length; i++)
                UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifyDiskImage()
        {
            return ADiskCRCHasFailed;
        }

        #region Private methods

        static UInt16 TeleDiskCRC(UInt16 crc, byte[] buffer)
        {
            int counter = 0;

            while (counter < buffer.Length)
            {
                crc ^= (UInt16)((buffer[counter] & 0xFF) << 8);

                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) > 0)
                        crc = (UInt16)((crc << 1) ^ TeleDiskCRCPoly);
                    else
                        crc = (UInt16)(crc << 1);
                }

                counter++;
            }

            return crc;
        }

        static byte[] DecodeTeleDiskData(byte sectorSize, byte encodingType, byte[] encodedData)
        {
            byte[] decodedData;
            switch (sectorSize)
            {
                case SectorSize128:
                    decodedData = new byte[128];
                    break;
                case SectorSize256:
                    decodedData = new byte[256];
                    break;
                case SectorSize512:
                    decodedData = new byte[512];
                    break;
                case SectorSize1K:
                    decodedData = new byte[1024];
                    break;
                case SectorSize2K:
                    decodedData = new byte[2048];
                    break;
                case SectorSize4K:
                    decodedData = new byte[4096];
                    break;
                case SectorSize8K:
                    decodedData = new byte[8192];
                    break;
                default:
                    throw new ImageNotSupportedException(String.Format("Sector size {0} is incorrect.", sectorSize));
            }

            switch (encodingType)
            {
                case dataBlockCopy:
                    Array.Copy(encodedData, decodedData, decodedData.Length);
                    break;
                case dataBlockPattern:
                    {
                        int ins = 0;
                        int outs = 0;
                        while (ins < encodedData.Length)
                        {
                            UInt16 repeatNumber;
                            byte[] repeatValue = new byte[2];

                            repeatNumber = BitConverter.ToUInt16(encodedData, ins);
                            Array.Copy(encodedData, ins + 2, repeatValue, 0, 2);
                            byte[] decodedPiece = new byte[repeatNumber * 2];
                            ArrayHelpers.ArrayFill(decodedPiece, repeatValue);
                            Array.Copy(decodedPiece, 0, decodedData, outs, decodedPiece.Length);
                            ins += 4;
                            outs += decodedPiece.Length;
                        }
                        //if (MainClass.isDebug)
                        {
                            Console.WriteLine("DEBUG (TeleDisk plugin): (Block pattern decoder): Input data size: {0} bytes", encodedData.Length);
                            Console.WriteLine("DEBUG (TeleDisk plugin): (Block pattern decoder): Processed input: {0} bytes", ins);
                            Console.WriteLine("DEBUG (TeleDisk plugin): (Block pattern decoder): Output data size: {0} bytes", decodedData.Length);
                            Console.WriteLine("DEBUG (TeleDisk plugin): (Block pattern decoder): Processed Output: {0} bytes", outs);
                        }
                        break;
                    }
                case dataBlockRLE:
                    {
                        int ins = 0;
                        int outs = 0;
                        while (ins < encodedData.Length)
                        {
                            byte Run;
                            byte Length;
                            byte Encoding;
                            byte[] Piece;

                            Encoding = encodedData[ins];
                            if (Encoding == 0x00)
                            {
                                Length = encodedData[ins + 1];
                                Array.Copy(encodedData, ins + 2, decodedData, outs, Length);
                                ins += (2 + Length);
                                outs += Length;
                            }
                            else
                            {
                                Length = (byte)(Encoding * 2);
                                Run = encodedData[ins + 1];
                                byte[] Part = new byte[Length];
                                Array.Copy(encodedData, ins + 2, Part, 0, Length);
                                Piece = new byte[Length * Run];
                                ArrayHelpers.ArrayFill(Piece, Part);
                                Array.Copy(Piece, 0, decodedData, outs, Piece.Length);
                                ins += (2 + Length);
                                outs += Piece.Length;
                            }
                        }
                        //if (MainClass.isDebug)
                        {
                            Console.WriteLine("DEBUG (TeleDisk plugin): (RLE decoder): Input data size: {0} bytes", encodedData.Length);
                            Console.WriteLine("DEBUG (TeleDisk plugin): (RLE decoder): Processed input: {0} bytes", ins);
                            Console.WriteLine("DEBUG (TeleDisk plugin): (RLE decoder): Output data size: {0} bytes", decodedData.Length);
                            Console.WriteLine("DEBUG (TeleDisk plugin): (RLE decoder): Processed Output: {0} bytes", outs);
                        }
                        break;
                    }
                default:
                    throw new ImageNotSupportedException(String.Format("Data encoding {0} is incorrect.", encodingType));
            }

            return decodedData;
        }

        DiskType DecodeTeleDiskDiskType()
        {
            switch (header.driveType)
            {
                case DriveType525DD:
                case DriveType525HD_DDDisk:
                case DriveType525HD:
                    {
                        switch (totalDiskSize)
                        {
                            case 163840:
                                {
                                    // Acorn disk uses 256 bytes/sector
                                    if (ImageInfo.sectorSize == 256)
                                        return DiskType.ACORN_525_SS_DD_40;
                                    else // DOS disks use 512 bytes/sector
                                        return DiskType.DOS_525_SS_DD_8;
                                }
                            case 184320:
                                {
                                    // Atari disk uses 256 bytes/sector
                                    if (ImageInfo.sectorSize == 256)
                                        return DiskType.ATARI_525_DD;
                                    else // DOS disks use 512 bytes/sector
                                        return DiskType.DOS_525_SS_DD_9;
                                }
                            case 327680:
                                {
                                    // Acorn disk uses 256 bytes/sector
                                    if (ImageInfo.sectorSize == 256)
                                        return DiskType.ACORN_525_SS_DD_80;
                                    else // DOS disks use 512 bytes/sector
                                        return DiskType.DOS_525_DS_DD_8;
                                }
                            case 368640:
                                return DiskType.DOS_525_DS_DD_9;
                            case 1228800:
                                return DiskType.DOS_525_HD;
                            case 102400:
                                return DiskType.ACORN_525_SS_SD_40;
                            case 204800:
                                return DiskType.ACORN_525_SS_SD_80;
                            case 655360:
                                return DiskType.ACORN_525_DS_DD;
                            case 92160:
                                return DiskType.ATARI_525_SD;
                            case 133120:
                                return DiskType.ATARI_525_ED;
                            case 1310720:
                                return DiskType.NEC_525_HD;
                            case 1261568:
                                return DiskType.SHARP_525;
                            case 839680:
                                return DiskType.FDFORMAT_525_DD;
                            case 1304320:
                                return DiskType.ECMA_99_8;
                            case 1223424:
                                return DiskType.ECMA_99_15;
                            case 1061632:
                                return DiskType.ECMA_99_26;
                            case 80384:
                                return DiskType.ECMA_66;
                            case 325632:
                                return DiskType.ECMA_70;
                            case 653312:
                                return DiskType.ECMA_78;
                            case 737280:
                                return DiskType.ECMA_78_2;
                            default:
                                {
                                    //if (MainClass.isDebug)
                                        Console.WriteLine("DEBUG (TeleDisk plugin): Unknown 5,25\" disk with {0} bytes", totalDiskSize);
                                    return DiskType.Unknown;
                                }
                        }
                    }
                case DriveType35DD:
                case DriveType35ED:
                case DriveType35HD:
                    {
                        switch (totalDiskSize)
                        {
                            case 327680:
                                return DiskType.DOS_35_SS_DD_8;
                            case 368640:
                                return DiskType.DOS_35_SS_DD_9;
                            case 655360:
                                return DiskType.DOS_35_DS_DD_8;
                            case 737280:
                                return DiskType.DOS_35_DS_DD_9;
                            case 1474560:
                                return DiskType.DOS_35_HD;
                            case 2949120:
                                return DiskType.DOS_35_ED;
                            case 1720320:
                                return DiskType.DMF;
                            case 1763328:
                                return DiskType.DMF_82;
                            case 1884160: // Irreal size, seen as BIOS with TSR, 23 sectors/track
                            case 1860608: // Real data size, sum of all sectors
                                return DiskType.XDF_35;
                            case 819200:
                                return DiskType.CBM_35_DD;
                            case 901120:
                                return DiskType.CBM_AMIGA_35_DD;
                            case 1802240:
                                return DiskType.CBM_AMIGA_35_HD;
                            case 1310720:
                                return DiskType.NEC_35_HD_8;
                            case 1228800:
                                return DiskType.NEC_35_HD_15;
                            case 1261568:
                                return DiskType.SHARP_35;
                            default:
                                {
                                    //if (MainClass.isDebug)
                                        Console.WriteLine("DEBUG (TeleDisk plugin): Unknown 3,5\" disk with {0} bytes", totalDiskSize);
                                    return DiskType.Unknown;
                                }
                        }
                    }
                case DriveType8inch:
                    {
                        switch (totalDiskSize)
                        {
                            case 81664:
                                return DiskType.IBM23FD;
                            case 242944:
                                return DiskType.IBM33FD_128;
                            case 287488:
                                return DiskType.IBM33FD_256;
                            case 306432:
                                return DiskType.IBM33FD_512;
                            case 499200:
                                return DiskType.IBM43FD_128;
                            case 574976:
                                return DiskType.IBM43FD_256;
                            case 995072:
                                return DiskType.IBM53FD_256;
                            case 1146624:
                                return DiskType.IBM53FD_512;
                            case 1222400:
                                return DiskType.IBM53FD_1024;
                            case 256256:
                                // Same size, with same disk geometry, for DEC RX01, NEC and ECMA, return ECMA
                                return DiskType.ECMA_54;
                            case 512512:
                                {
                                    // DEC disk uses 256 bytes/sector
                                    if (ImageInfo.sectorSize == 256)
                                        return DiskType.RX02;
                                    else // ECMA disks use 128 bytes/sector
                                        return DiskType.ECMA_59;
                                }
                            case 1261568:
                                return DiskType.NEC_8_DD;
                            case 1255168:
                                return DiskType.ECMA_69_8;
                            case 1177344:
                                return DiskType.ECMA_69_15;
                            case 1021696:
                                return DiskType.ECMA_69_26;
                            default:
                                {
                                    //if (MainClass.isDebug)
                                        Console.WriteLine("DEBUG (TeleDisk plugin): Unknown 8\" disk with {0} bytes", totalDiskSize);
                                    return DiskType.Unknown;
                                }
                        }
                    }
                default:
                    {
                        //if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (TeleDisk plugin): Unknown drive type {1} with {0} bytes", totalDiskSize, header.driveType);
                        return DiskType.Unknown;
                    }

            }
        }

        #endregion

        #region Unsupported features

        public override byte[] ReadSectorTag(UInt64 sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadDiskTag(DiskTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        public override string   GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override string   GetDiskManufacturer()
        {
            return ImageInfo.diskManufacturer;
        }

        public override string   GetDiskModel()
        {
            return ImageInfo.diskModel;
        }

        public override string   GetDiskSerialNumber()
        {
            return ImageInfo.diskSerialNumber;
        }

        public override string   GetDiskBarcode()
        {
            return ImageInfo.diskBarcode;
        }

        public override string   GetDiskPartNumber()
        {
            return ImageInfo.diskPartNumber;
        }

        public override int      GetDiskSequence()
        {
            return ImageInfo.diskSequence;
        }

        public override int      GetLastDiskSequence()
        {
            return ImageInfo.lastDiskSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.driveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.driveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.driveSerialNumber;
        }

        public override List<CommonTypes.Partition> GetPartitions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetTracks()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(UInt16 session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(UInt64 sectorAddress, UInt32 track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(UInt64 sectorAddress, UInt32 track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length, UInt32 track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, UInt32 track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(UInt64 sectorAddress, UInt32 track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length, UInt32 track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        #endregion Unsupported features
    }
}

