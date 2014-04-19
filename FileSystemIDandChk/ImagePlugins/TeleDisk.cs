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
        

        #endregion
        
        public TeleDisk(PluginBase Core)
        {
            Name = "Sydex TeleDisk";
            PluginUUID = new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
        }
        
        public override bool IdentifyImage(string imagePath)
        {
            TD0Header header = new TD0Header();
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
            }

            return true;
        }
        
        public override bool OpenImage(string imagePath)
        {
            throw new NotImplementedException("Not yet implemented.");
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

