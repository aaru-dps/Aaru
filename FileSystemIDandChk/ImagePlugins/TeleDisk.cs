/***************************************************************************
FileSystem identifier and checker
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
namespace FileSystemIDandChk.ImagePlugins
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
        string comment;
        DateTime creationDate;
        DateTime modificationDate;

        #endregion
        
        public TeleDisk(PluginBase Core)
        {
            Name = "Sydex TeleDisk";
            PluginUUID = new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
        }

        public override bool IdentifyImage(string imagePath)
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

            byte[] headerBytesForCRC = new byte[10];
            Array.Copy(headerBytes, headerBytesForCRC, 10);
            UInt16 calculatedHeaderCRC = TeleDiskCRC(0x0000, headerBytesForCRC);

            if (MainClass.isDebug)
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
            
            byte[] headerBytesForCRC = new byte[10];
            Array.Copy(headerBytes, headerBytesForCRC, 10);
            UInt16 calculatedHeaderCRC = TeleDiskCRC(0x0000, headerBytesForCRC);
            
            if (MainClass.isDebug)
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
            if (header.crc != calculatedHeaderCRC && MainClass.isDebug)
                Console.WriteLine("DEBUG (TeleDisk plugin): Calculated CRC does not coincide with stored one.");

            if (header.sequence != 0x00)
                return false;
            
            if (header.dataRate != DataRate250kbps && header.dataRate != DataRate300kbps && header.dataRate != DataRate500kbps)
                return false;
            
            if (header.driveType != DriveType35DD && header.driveType != DriveType35ED && header.driveType != DriveType35HD && header.driveType != DriveType525DD &&
                header.driveType != DriveType525HD && header.driveType != DriveType525HD_DDDisk && header.driveType != DriveType8inch)
                return false;

            if (header.signature == tdAdvCompMagic)
                throw new NotImplementedException("TeleDisk Advanced Compression support not yet implemented");

            creationDate = DateTime.MinValue;

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

                if(MainClass.isDebug)
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

                for(int i=0;i<commentBlock.Length;i++)
                {
                    // Replace NULLs, used by TeleDisk as newline markers, with UNIX newline marker
                    if(commentBlock[i]==0x00)
                        commentBlock[i]=0x0A;
                }

                comment = System.Text.Encoding.ASCII.GetString(commentBlock);

                if(MainClass.isDebug)
                {
                    Console.WriteLine("DEBUG (TeleDisk plugin): Comment");
                    Console.WriteLine("DEBUG (TeleDisk plugin): {0}", comment);
                }

                creationDate = new DateTime(commentHeader.year+1900, commentHeader.month+1, commentHeader.day,
                                            commentHeader.hour, commentHeader.minute, commentHeader.second, DateTimeKind.Unspecified);
            }

            FileInfo fi = new FileInfo(imagePath);
            if (creationDate == DateTime.MinValue)
                creationDate = fi.CreationTimeUtc;
            modificationDate = fi.LastWriteTimeUtc;

            if (MainClass.isDebug)
            {
                Console.WriteLine("DEBUG (TeleDisk plugin): Image created on {0}", creationDate);
                Console.WriteLine("DEBUG (TeleDisk plugin): Image modified on {0}", modificationDate);
            }

            return false;
        }
        
        public override bool ImageHasPartitions()
        {
            return false;
        }
        
        public override UInt64 GetImageSize()
        {
            throw new NotImplementedException("Not yet implemented.");
        }
        
        public override UInt64 GetSectors()
        {
            throw new NotImplementedException("Not yet implemented.");
        }
        
        public override UInt32 GetSectorSize()
        {
            throw new NotImplementedException("Not yet implemented.");
        }
        
        public override byte[] ReadSector(UInt64 sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }
        
        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length)
        {
            throw new NotImplementedException("Not yet implemented.");
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
            throw new NotImplementedException("Not yet implemented.");
        }
        
        public override string   GetImageApplication()
        {
            return "Sydex TeleDisk";
        }
        
        public override string   GetImageApplicationVersion()
        {
            throw new NotImplementedException("Not yet implemented.");
        }
        
        public override DateTime GetImageCreationTime()
        {
            throw new NotImplementedException("Not yet implemented.");
        }
        
        public override DateTime GetImageLastModificationTime()
        {
            throw new NotImplementedException("Not yet implemented.");
        }
        
        public override string   GetImageName()
        {
            throw new NotImplementedException("Not yet implemented.");
        }
        
        public override DiskType GetDiskType()
        {
            throw new NotImplementedException("Not yet implemented.");
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
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override string   GetImageComments()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override string   GetDiskManufacturer()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override string   GetDiskModel()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override string   GetDiskSerialNumber()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override string   GetDiskBarcode()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override string   GetDiskPartNumber()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override int      GetDiskSequence()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override int      GetLastDiskSequence()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override string GetDriveManufacturer()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override string GetDriveModel()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override string GetDriveSerialNumber()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        
        public override List<PartPlugins.Partition> GetPartitions()
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

