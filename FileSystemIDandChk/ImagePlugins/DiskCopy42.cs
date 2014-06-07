/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : DiskCopy42.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Disc image plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Manages Apple DiskCopy 4.2 disc images, including unofficial modifications.
 
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

namespace FileSystemIDandChk.ImagePlugins
{
    // Checked using several images and strings inside Apple's DiskImages.framework
    class DiskCopy42 : ImagePlugin
    {
        #region Internal Structures

        // DiskCopy 4.2 header, big-endian, data-fork, start of file, 84 bytes
        struct DC42Header
        {
            // 0x00, 64 bytes, pascal string, disk name or "-not a Macintosh disk-", filled with garbage
            public string diskName;
            // 0x40, size of data in bytes (usually sectors*512)
            public UInt32 dataSize;
            // 0x44, size of tags in bytes (usually sectors*12)
            public UInt32 tagSize;
            // 0x48, checksum of data bytes
            public UInt32 dataChecksum;
            // 0x4C, checksum of tag bytes
            public UInt32 tagChecksum;
            // 0x50, format of disk, see constants
            public byte format;
            // 0x51, format of sectors, see constants
            public byte fmtByte;
            // 0x52, is disk image valid? always 0x01
            public byte valid;
            // 0x53, reserved, always 0x00
            public byte reserved;
        }

        #endregion

        #region Internal Constants

        // format byte
        // 3.5", single side, double density, GCR
        const byte kSonyFormat400K = 0x00;
        // 3.5", double side, double density, GCR
        const byte kSonyFormat800K = 0x01;
        // 3.5", double side, double density, MFM
        const byte kSonyFormat720K = 0x02;
        // 3.5", double side, high density, MFM
        const byte kSonyFormat1440K = 0x03;
        // 3.5", double side, high density, MFM, 21 sectors/track (aka, Microsoft DMF)
        // Unchecked value
        const byte kSonyFormat1680K = 0x04;
        // Defined by Sigma Seven's BLU
        const byte kSigmaFormatTwiggy = 0x54;
        // There should be a value for Apple HD20 hard disks, unknown...
        // fmyByte byte
        // Based on GCR nibble
        // Always 0x02 for MFM disks
        // Unknown for Apple HD20
        // Defined by Sigma Seven's BLU
        const byte kSignaFmtByteTwiggy = 0x01;
        // 3.5" single side double density GCR and MFM all use same code
        const byte kSonyFmtByte400K = 0x02;
        const byte kSonyFmtByte720K = kSonyFmtByte400K;
        const byte kSonyFmtByte1440K = kSonyFmtByte400K;
        const byte kSonyFmtByte1680K = kSonyFmtByte400K;
        // 3.5" double side double density GCR, 512 bytes/sector, interleave 2:1
        const byte kSonyFmtByte800K = 0x22;
        // 3.5" double side double density GCR, 512 bytes/sector, interleave 2:1, incorrect value (but appears on official documentation)
        const byte kSonyFmtByte800KIncorrect = 0x12;
        // 3.5" double side double density GCR, ProDOS format, interleave 4:1
        const byte kSonyFmtByteProDos = 0x24;
        // Unformatted sectors
        const byte kInvalidFmtByte = 0x96;

        #endregion

        #region Internal variables

        // Start of data sectors in disk image, should be 0x58
        UInt32 dataOffset;
        // Start of tags in disk image, after data sectors
        UInt32 tagOffset;
        // Sectors
        UInt32 sectors;
        // Bytes per sector, should be 512
        UInt32 bps;
        // Bytes per tag, should be 12
        UInt32 bptag;
        // Header of opened image
        DC42Header header;
        // Disk image file
        string dc42ImagePath;

        #endregion

        public DiskCopy42(PluginBase Core)
        {
            Name = "Apple DiskCopy 4.2";
            PluginUUID = new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
        }

        public override bool IdentifyImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[0x58];
            byte[] pString = new byte[64];
            stream.Read(buffer, 0, 0x58);

            // Incorrect pascal string length, not DC42
            if (buffer[0] > 63)
                return false;

            DC42Header tmp_header = new DC42Header();

            Array.Copy(buffer, 0, pString, 0, 64);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            tmp_header.diskName = StringHandlers.PascalToString(pString);
            tmp_header.dataSize = BigEndianBitConverter.ToUInt32(buffer, 0x40);
            tmp_header.tagSize = BigEndianBitConverter.ToUInt32(buffer, 0x44);
            tmp_header.dataChecksum = BigEndianBitConverter.ToUInt32(buffer, 0x48);
            tmp_header.tagChecksum = BigEndianBitConverter.ToUInt32(buffer, 0x4C);
            tmp_header.format = buffer[0x50];
            tmp_header.fmtByte = buffer[0x51];
            tmp_header.valid = buffer[0x52];
            tmp_header.reserved = buffer[0x53];

            if (MainClass.isDebug)
            {
                Console.WriteLine("DEBUG (DC42 plugin): tmp_header.diskName = \"{0}\"", tmp_header.diskName);
                Console.WriteLine("DEBUG (DC42 plugin): tmp_header.dataSize = {0} bytes", tmp_header.dataSize);
                Console.WriteLine("DEBUG (DC42 plugin): tmp_header.tagSize = {0} bytes", tmp_header.tagSize);
                Console.WriteLine("DEBUG (DC42 plugin): tmp_header.dataChecksum = 0x{0:X8}", tmp_header.dataChecksum);
                Console.WriteLine("DEBUG (DC42 plugin): tmp_header.tagChecksum = 0x{0:X8}", tmp_header.tagChecksum);
                Console.WriteLine("DEBUG (DC42 plugin): tmp_header.format = 0x{0:X2}", tmp_header.format);
                Console.WriteLine("DEBUG (DC42 plugin): tmp_header.fmtByte = 0x{0:X2}", tmp_header.fmtByte);
                Console.WriteLine("DEBUG (DC42 plugin): tmp_header.valid = {0}", tmp_header.valid);
                Console.WriteLine("DEBUG (DC42 plugin): tmp_header.reserved = {0}", tmp_header.reserved);
            }

            if (tmp_header.valid != 1 || tmp_header.reserved != 0)
                return false;

            FileInfo fi = new FileInfo(imagePath);

            if (tmp_header.dataSize + tmp_header.tagSize + 0x54 != fi.Length && tmp_header.format != kSigmaFormatTwiggy)
                return false;

            if (tmp_header.format != kSonyFormat400K && tmp_header.format != kSonyFormat800K && tmp_header.format != kSonyFormat720K &&
                tmp_header.format != kSonyFormat1440K && tmp_header.format != kSonyFormat1680K && tmp_header.format != kSigmaFormatTwiggy)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("Unknown tmp_header.format = 0x{0:X2} value", tmp_header.format);

                return false;
            }

            if (tmp_header.fmtByte != kSonyFmtByte400K && tmp_header.fmtByte != kSonyFmtByte800K && tmp_header.fmtByte != kSonyFmtByte800KIncorrect &&
                tmp_header.fmtByte != kSonyFmtByteProDos && tmp_header.fmtByte != kInvalidFmtByte && tmp_header.fmtByte != kSignaFmtByteTwiggy)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("Unknown tmp_header.fmtByte = 0x{0:X2} value", tmp_header.fmtByte);

                return false;
            }

            if (tmp_header.fmtByte == kInvalidFmtByte)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("Image says it's unformatted");

                return false;
            }

            return true;
        }

        public override bool OpenImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[0x58];
            byte[] pString = new byte[64];
            stream.Read(buffer, 0, 0x58);

            // Incorrect pascal string length, not DC42
            if (buffer[0] > 63)
                return false;

            header = new DC42Header();
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            Array.Copy(buffer, 0, pString, 0, 64);
            header.diskName = StringHandlers.PascalToString(pString);
            header.dataSize = BigEndianBitConverter.ToUInt32(buffer, 0x40);
            header.tagSize = BigEndianBitConverter.ToUInt32(buffer, 0x44);
            header.dataChecksum = BigEndianBitConverter.ToUInt32(buffer, 0x48);
            header.tagChecksum = BigEndianBitConverter.ToUInt32(buffer, 0x4C);
            header.format = buffer[0x50];
            header.fmtByte = buffer[0x51];
            header.valid = buffer[0x52];
            header.reserved = buffer[0x53];

            if (MainClass.isDebug)
            {
                Console.WriteLine("DEBUG (DC42 plugin): header.diskName = \"{0}\"", header.diskName);
                Console.WriteLine("DEBUG (DC42 plugin): header.dataSize = {0} bytes", header.dataSize);
                Console.WriteLine("DEBUG (DC42 plugin): header.tagSize = {0} bytes", header.tagSize);
                Console.WriteLine("DEBUG (DC42 plugin): header.dataChecksum = 0x{0:X8}", header.dataChecksum);
                Console.WriteLine("DEBUG (DC42 plugin): header.tagChecksum = 0x{0:X8}", header.tagChecksum);
                Console.WriteLine("DEBUG (DC42 plugin): header.format = 0x{0:X2}", header.format);
                Console.WriteLine("DEBUG (DC42 plugin): header.fmtByte = 0x{0:X2}", header.fmtByte);
                Console.WriteLine("DEBUG (DC42 plugin): header.valid = {0}", header.valid);
                Console.WriteLine("DEBUG (DC42 plugin): header.reserved = {0}", header.reserved);
            }

            if (header.valid != 1 || header.reserved != 0)
                return false;

            FileInfo fi = new FileInfo(imagePath);

            if (header.dataSize + header.tagSize + 0x54 != fi.Length && header.format != kSigmaFormatTwiggy)
                return false;

            if (header.format != kSonyFormat400K && header.format != kSonyFormat800K && header.format != kSonyFormat720K &&
                header.format != kSonyFormat1440K && header.format != kSonyFormat1680K && header.format != kSigmaFormatTwiggy)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (DC42 plugin): Unknown header.format = 0x{0:X2} value", header.format);

                return false;
            }

            if (header.fmtByte != kSonyFmtByte400K && header.fmtByte != kSonyFmtByte800K && header.fmtByte != kSonyFmtByte800KIncorrect &&
                header.fmtByte != kSonyFmtByteProDos && header.fmtByte != kInvalidFmtByte && header.fmtByte != kSignaFmtByteTwiggy)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (DC42 plugin): Unknown tmp_header.fmtByte = 0x{0:X2} value", header.fmtByte);

                return false;
            }

            if (header.fmtByte == kInvalidFmtByte)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (DC42 plugin): Image says it's unformatted");

                return false;
            }

            dataOffset = 0x54;
            tagOffset = header.tagSize != 0 ? 0x54 + header.dataSize : 0;
            bps = 512;
            bptag = (uint)(header.tagSize != 0 ? 12 : 0);
            dc42ImagePath = imagePath;

            sectors = header.dataSize / 512;

            if (header.tagSize != 0)
            {
                if (header.tagSize / 12 != sectors)
                {
                    if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (DC42 plugin): header.tagSize / 12 != sectors");

                    return false;
                }
            }

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return false;
        }

        public override UInt64 GetImageSize()
        {
            return sectors * bps + sectors * bptag;
        }

        public override UInt64 GetSectors()
        {
            return sectors;
        }

        public override UInt32 GetSectorSize()
        {
            return bps;
        }

        public override byte[] ReadSector(UInt64 sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(UInt64 sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length)
        {
            if (sectorAddress > sectors - 1)
                throw new ArgumentOutOfRangeException("sectorAddress", "Sector address not found");

            if (sectorAddress + length > sectors)
                throw new ArgumentOutOfRangeException("length", "Requested more sectors than available");

            byte[] buffer = new byte[length * bps];

            FileStream stream = new FileStream(dc42ImagePath, FileMode.Open, FileAccess.Read);

            stream.Seek((long)(dataOffset + sectorAddress * bps), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * bps));

            return buffer;
        }

        public override byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, SectorTagType tag)
        {
            if (tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException(String.Format("Tag {0} not supported by image format", tag));

            if (header.tagSize == 0)
                throw new FeatureNotPresentImageException("Disk image does not have tags");

            if (sectorAddress > sectors - 1)
                throw new ArgumentOutOfRangeException("sectorAddress", "Sector address not found");

            if (sectorAddress + length > sectors)
                throw new ArgumentOutOfRangeException("length", "Requested more sectors than available");

            byte[] buffer = new byte[length * bptag];

            FileStream stream = new FileStream(dc42ImagePath, FileMode.Open, FileAccess.Read);

            stream.Seek((long)(tagOffset + sectorAddress * bptag), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * bptag));

            return buffer;
        }

        public override byte[] ReadSectorLong(UInt64 sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public override byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length)
        {
            if (sectorAddress > sectors - 1)
                throw new ArgumentOutOfRangeException("sectorAddress", "Sector address not found");

            if (sectorAddress + length > sectors)
                throw new ArgumentOutOfRangeException("length", "Requested more sectors than available");

            byte[] data = ReadSectors(sectorAddress, length);
            byte[] tags = ReadSectorsTag(sectorAddress, length, SectorTagType.AppleSectorTag);
            byte[] buffer = new byte[data.Length + tags.Length];

            for (uint i = 0; i < length; i++)
            {
                Array.Copy(data, i * (bps), buffer, i * (bps + bptag), bps);
                Array.Copy(tags, i * (bptag), buffer, i * (bps + bptag) + bps, bptag);
            }

            return buffer;
        }

        public override string   GetImageFormat()
        { 
            return "Apple DiskCopy 4.2";
        }

        public override string   GetImageVersion()
        {
            return "4.2";
        }

        public override string   GetImageApplication()
        {
            return "Apple DiskCopy";
        }

        public override string   GetImageApplicationVersion()
        {
            return "4.2";
        }

        public override DateTime GetImageCreationTime()
        {
            FileInfo fi = new FileInfo(dc42ImagePath);

            return fi.CreationTimeUtc;
        }

        public override DateTime GetImageLastModificationTime()
        {
            FileInfo fi = new FileInfo(dc42ImagePath);

            return fi.LastWriteTimeUtc;
        }

        public override string   GetImageName()
        {
            return header.diskName;
        }

        public override DiskType GetDiskType()
        {
            switch (header.format)
            {
                case kSonyFormat400K:
                    return DiskType.AppleSonySS;
                case kSonyFormat800K:
                    return DiskType.AppleSonyDS;
                case kSonyFormat720K:
                    return DiskType.DOS_35_DS_DD_9;
                case kSonyFormat1440K:
                    return DiskType.DOS_35_HD;
                case kSonyFormat1680K:
                    return DiskType.DMF;
                case kSigmaFormatTwiggy:
                    return DiskType.AppleFileWare;
                default:
                    return DiskType.Unknown;
            }
        }

        #region Unsupported features

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

