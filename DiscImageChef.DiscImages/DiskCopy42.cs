// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DiskCopy42.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Apple DiskCopy 4.2 disc images, including unofficial modifications.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.ImagePlugins
{
    // Checked using several images and strings inside Apple's DiskImages.framework
    class DiskCopy42 : ImagePlugin
    {
        #region Internal Structures

        // DiskCopy 4.2 header, big-endian, data-fork, start of file, 84 bytes
        struct DC42Header
        {
            /// <summary>0x00, 64 bytes, pascal string, disk name or "-not a Macintosh disk-", filled with garbage</summary>
            public string diskName;
            /// <summary>0x40, size of data in bytes (usually sectors*512)</summary>
            public uint dataSize;
            /// <summary>0x44, size of tags in bytes (usually sectors*12)</summary>
            public uint tagSize;
            /// <summary>0x48, checksum of data bytes</summary>
            public uint dataChecksum;
            /// <summary>0x4C, checksum of tag bytes</summary>
            public uint tagChecksum;
            /// <summary>0x50, format of disk, see constants</summary>
            public byte format;
            /// <summary>0x51, format of sectors, see constants</summary>
            public byte fmtByte;
            /// <summary>0x52, is disk image valid? always 0x01</summary>
            public byte valid;
            /// <summary>0x53, reserved, always 0x00</summary>
            public byte reserved;
        }

        #endregion

        #region Internal Constants

        // format byte
        /// <summary>3.5", single side, double density, GCR</summary>
        const byte kSonyFormat400K = 0x00;
        /// <summary>3.5", double side, double density, GCR</summary>
        const byte kSonyFormat800K = 0x01;
        /// <summary>3.5", double side, double density, MFM</summary>
        const byte kSonyFormat720K = 0x02;
        /// <summary>3.5", double side, high density, MFM</summary>
        const byte kSonyFormat1440K = 0x03;
        /// <summary>3.5", double side, high density, MFM, 21 sectors/track (aka, Microsoft DMF)
        // Unchecked value</summary>
        const byte kSonyFormat1680K = 0x04;
        /// <summary>Defined by Sigma Seven's BLU</summary>
        const byte kSigmaFormatTwiggy = 0x54;
        /// <summary>Defined by LisaEm</summary>
        const byte kNotStandardFormat = 0x5D;
        // There should be a value for Apple HD20 hard disks, unknown...
        // fmyByte byte
        // Based on GCR nibble
        // Always 0x02 for MFM disks
        // Unknown for Apple HD20
        /// <summary>Defined by Sigma Seven's BLU</summary>
        const byte kSigmaFmtByteTwiggy = 0x01;
        /// <summary>3.5" single side double density GCR and MFM all use same code</summary>
        const byte kSonyFmtByte400K = 0x02;
        const byte kSonyFmtByte720K = kSonyFmtByte400K;
        const byte kSonyFmtByte1440K = kSonyFmtByte400K;
        const byte kSonyFmtByte1680K = kSonyFmtByte400K;
        /// <summary>3.5" double side double density GCR, 512 bytes/sector, interleave 2:1</summary>
        const byte kSonyFmtByte800K = 0x22;
        /// <summary>3.5" double side double density GCR, 512 bytes/sector, interleave 2:1, incorrect value (but appears on official documentation)</summary>
        const byte kSonyFmtByte800KIncorrect = 0x12;
        /// <summary>3.5" double side double density GCR, ProDOS format, interleave 4:1</summary>
        const byte kSonyFmtByteProDos = 0x24;
        /// <summary>Unformatted sectors</summary>
        const byte kInvalidFmtByte = 0x96;
        /// <summary>Defined by LisaEm</summary>
        const byte kFmtNotStandard = 0x93;

        #endregion

        #region Internal variables

        /// <summary>Start of data sectors in disk image, should be 0x58</summary>
        uint dataOffset;
        /// <summary>Start of tags in disk image, after data sectors</summary>
        uint tagOffset;
        /// <summary>Bytes per tag, should be 12</summary>
        uint bptag;
        /// <summary>Header of opened image</summary>
        DC42Header header;
        /// <summary>Disk image file</summary>
        string dc42ImagePath;

        byte[] twiggyCache;
        byte[] twiggyCacheTags;
        bool twiggy;

        #endregion

        public DiskCopy42()
        {
            Name = "Apple DiskCopy 4.2";
            PluginUUID = new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = "4.2";
            ImageInfo.imageApplication = "Apple DiskCopy";
            ImageInfo.imageApplicationVersion = "4.2";
            ImageInfo.imageCreator = null;
            ImageInfo.imageComments = null;
            ImageInfo.mediaManufacturer = null;
            ImageInfo.mediaModel = null;
            ImageInfo.mediaSerialNumber = null;
            ImageInfo.mediaBarcode = null;
            ImageInfo.mediaPartNumber = null;
            ImageInfo.mediaSequence = 0;
            ImageInfo.lastMediaSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
            ImageInfo.driveFirmwareRevision = null;
        }

        public override bool IdentifyImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[0x58];
            byte[] pString = new byte[64];
            stream.Read(buffer, 0, 0x58);
            stream.Close();

            // Incorrect pascal string length, not DC42
            if(buffer[0] > 63)
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

            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.diskName = \"{0}\"", tmp_header.diskName);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.dataSize = {0} bytes", tmp_header.dataSize);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.tagSize = {0} bytes", tmp_header.tagSize);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.dataChecksum = 0x{0:X8}", tmp_header.dataChecksum);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.tagChecksum = 0x{0:X8}", tmp_header.tagChecksum);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.format = 0x{0:X2}", tmp_header.format);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.fmtByte = 0x{0:X2}", tmp_header.fmtByte);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.valid = {0}", tmp_header.valid);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.reserved = {0}", tmp_header.reserved);

            if(tmp_header.valid != 1 || tmp_header.reserved != 0)
                return false;

            FileInfo fi = new FileInfo(imagePath);

            if(tmp_header.dataSize + tmp_header.tagSize + 0x54 != fi.Length && tmp_header.format != kSigmaFormatTwiggy)
                return false;

            if(tmp_header.format != kSonyFormat400K && tmp_header.format != kSonyFormat800K && tmp_header.format != kSonyFormat720K &&
                tmp_header.format != kSonyFormat1440K && tmp_header.format != kSonyFormat1680K && tmp_header.format != kSigmaFormatTwiggy &&
               tmp_header.format != kNotStandardFormat)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Unknown tmp_header.format = 0x{0:X2} value", tmp_header.format);

                return false;
            }

            if(tmp_header.fmtByte != kSonyFmtByte400K && tmp_header.fmtByte != kSonyFmtByte800K && tmp_header.fmtByte != kSonyFmtByte800KIncorrect &&
               tmp_header.fmtByte != kSonyFmtByteProDos && tmp_header.fmtByte != kInvalidFmtByte && tmp_header.fmtByte != kSigmaFmtByteTwiggy &&
               tmp_header.fmtByte != kFmtNotStandard)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Unknown tmp_header.fmtByte = 0x{0:X2} value", tmp_header.fmtByte);

                return false;
            }

            if(tmp_header.fmtByte == kInvalidFmtByte)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Image says it's unformatted");

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
            stream.Close();

            // Incorrect pascal string length, not DC42
            if(buffer[0] > 63)
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

            DicConsole.DebugWriteLine("DC42 plugin", "header.diskName = \"{0}\"", header.diskName);
            DicConsole.DebugWriteLine("DC42 plugin", "header.dataSize = {0} bytes", header.dataSize);
            DicConsole.DebugWriteLine("DC42 plugin", "header.tagSize = {0} bytes", header.tagSize);
            DicConsole.DebugWriteLine("DC42 plugin", "header.dataChecksum = 0x{0:X8}", header.dataChecksum);
            DicConsole.DebugWriteLine("DC42 plugin", "header.tagChecksum = 0x{0:X8}", header.tagChecksum);
            DicConsole.DebugWriteLine("DC42 plugin", "header.format = 0x{0:X2}", header.format);
            DicConsole.DebugWriteLine("DC42 plugin", "header.fmtByte = 0x{0:X2}", header.fmtByte);
            DicConsole.DebugWriteLine("DC42 plugin", "header.valid = {0}", header.valid);
            DicConsole.DebugWriteLine("DC42 plugin", "header.reserved = {0}", header.reserved);

            if(header.valid != 1 || header.reserved != 0)
                return false;

            FileInfo fi = new FileInfo(imagePath);

            if(header.dataSize + header.tagSize + 0x54 != fi.Length && header.format != kSigmaFormatTwiggy)
                return false;

            if(header.format != kSonyFormat400K && header.format != kSonyFormat800K && header.format != kSonyFormat720K &&
                header.format != kSonyFormat1440K && header.format != kSonyFormat1680K && header.format != kSigmaFormatTwiggy && header.format != kNotStandardFormat)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Unknown header.format = 0x{0:X2} value", header.format);

                return false;
            }

            if(header.fmtByte != kSonyFmtByte400K && header.fmtByte != kSonyFmtByte800K && header.fmtByte != kSonyFmtByte800KIncorrect &&
                header.fmtByte != kSonyFmtByteProDos && header.fmtByte != kInvalidFmtByte && header.fmtByte != kSigmaFmtByteTwiggy && header.fmtByte != kFmtNotStandard)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Unknown tmp_header.fmtByte = 0x{0:X2} value", header.fmtByte);

                return false;
            }

            if(header.fmtByte == kInvalidFmtByte)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Image says it's unformatted");

                return false;
            }

            dataOffset = 0x54;
            tagOffset = header.tagSize != 0 ? 0x54 + header.dataSize : 0;
            ImageInfo.sectorSize = 512;
            bptag = (uint)(header.tagSize != 0 ? 12 : 0);
            dc42ImagePath = imagePath;

            ImageInfo.sectors = header.dataSize / 512;

            if(header.tagSize != 0)
            {
                bptag = (uint)(header.tagSize / ImageInfo.sectors);
                DicConsole.DebugWriteLine("DC42 plugin", "bptag = {0} bytes", bptag);

                if(bptag != 12 && bptag != 20 && bptag != 24)
                {
                    DicConsole.DebugWriteLine("DC42 plugin", "Unknown tag size");
                    return false;
                }

                ImageInfo.readableSectorTags.Add(SectorTagType.AppleSectorTag);
            }

            ImageInfo.imageSize = ImageInfo.sectors * ImageInfo.sectorSize + ImageInfo.sectors * bptag;
            ImageInfo.imageCreationTime = fi.CreationTimeUtc;
            ImageInfo.imageLastModificationTime = fi.LastWriteTimeUtc;
            ImageInfo.imageName = header.diskName;

            switch(header.format)
            {
                case kSonyFormat400K:
                    ImageInfo.mediaType = MediaType.AppleSonySS;
                    break;
                case kSonyFormat800K:
                    ImageInfo.mediaType = MediaType.AppleSonyDS;
                    break;
                case kSonyFormat720K:
                    ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
                    break;
                case kSonyFormat1440K:
                    ImageInfo.mediaType = MediaType.DOS_35_HD;
                    break;
                case kSonyFormat1680K:
                    ImageInfo.mediaType = MediaType.DMF;
                    break;
                case kSigmaFormatTwiggy:
                    ImageInfo.mediaType = MediaType.AppleFileWare;
                    break;
                case kNotStandardFormat:
                    switch(ImageInfo.sectors)
                    {
                        case 9728:
                            ImageInfo.mediaType = MediaType.AppleProfile;
                            break;
                        case 19456:
                            ImageInfo.mediaType = MediaType.AppleProfile;
                            break;
                        case 38912:
                            ImageInfo.mediaType = MediaType.AppleWidget;
                            break;
                        case 39040:
                            ImageInfo.mediaType = MediaType.AppleHD20;
                            break;
                        default:
                            ImageInfo.mediaType = MediaType.Unknown;
                            break;
                    }
                    break;
                default:
                    ImageInfo.mediaType = MediaType.Unknown;
                    break;
            }

            if(ImageInfo.mediaType == MediaType.AppleFileWare)
            {
                byte[] data = new byte[header.dataSize];
                byte[] tags = new byte[header.tagSize];

                twiggyCache = new byte[header.dataSize];
                twiggyCacheTags = new byte[header.tagSize];
                twiggy = true;

                FileStream datastream = new FileStream(dc42ImagePath, FileMode.Open, FileAccess.Read);
                datastream.Seek((dataOffset), SeekOrigin.Begin);
                datastream.Read(data, 0, (int)header.dataSize);
                datastream.Close();

                FileStream tagstream = new FileStream(dc42ImagePath, FileMode.Open, FileAccess.Read);
                tagstream.Seek((tagOffset), SeekOrigin.Begin);
                tagstream.Read(tags, 0, (int)header.tagSize);
                tagstream.Close();

                ushort MFS_Magic = BigEndianBitConverter.ToUInt16(data, (int)((data.Length / 2) + 0x400));
                ushort MFS_AllBlocks = BigEndianBitConverter.ToUInt16(data, (int)((data.Length / 2) + 0x412));

                // Detect a Macintosh Twiggy
                if(MFS_Magic == 0xD2D7 && MFS_AllBlocks == 422)
                {
                    DicConsole.DebugWriteLine("DC42 plugin", "Macintosh Twiggy detected, reversing disk sides");
                    Array.Copy(data, (header.dataSize / 2), twiggyCache, 0, header.dataSize / 2);
                    Array.Copy(tags, (header.tagSize / 2), twiggyCacheTags, 0, header.tagSize / 2);
                    Array.Copy(data, 0, twiggyCache, header.dataSize / 2, header.dataSize / 2);
                    Array.Copy(tags, 0, twiggyCacheTags, header.tagSize / 2, header.tagSize / 2);
                }
                else
                {
                    DicConsole.DebugWriteLine("DC42 plugin", "Lisa Twiggy detected, reversing second half of disk");
                    Array.Copy(data, 0, twiggyCache, 0, header.dataSize / 2);
                    Array.Copy(tags, 0, twiggyCacheTags, 0, header.tagSize / 2);

                    int copiedSectors = 0;
                    int sectorsToCopy = 0;

                    for(int i = 0; i < 46; i++)
                    {
                        if(i >= 0 && i <= 3)
                            sectorsToCopy = 22;
                        if(i >= 4 && i <= 10)
                            sectorsToCopy = 21;
                        if(i >= 11 && i <= 16)
                            sectorsToCopy = 20;
                        if(i >= 17 && i <= 22)
                            sectorsToCopy = 19;
                        if(i >= 23 && i <= 28)
                            sectorsToCopy = 18;
                        if(i >= 29 && i <= 34)
                            sectorsToCopy = 17;
                        if(i >= 35 && i <= 41)
                            sectorsToCopy = 16;
                        if(i >= 42 && i <= 45)
                            sectorsToCopy = 15;

                        Array.Copy(data, header.dataSize / 2 + copiedSectors * 512, twiggyCache, twiggyCache.Length - copiedSectors * 512 - sectorsToCopy * 512, sectorsToCopy * 512);
                        Array.Copy(tags, header.tagSize / 2 + copiedSectors * bptag, twiggyCacheTags, twiggyCacheTags.Length - copiedSectors * bptag - sectorsToCopy * bptag, sectorsToCopy * bptag);

                        copiedSectors += sectorsToCopy;
                    }
                }
            }

            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

            return true;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++)
                UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++)
                UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifyMediaImage()
        {
            byte[] data = new byte[header.dataSize];
            byte[] tags = new byte[header.tagSize];
            uint dataChk;
            uint tagsChk = 0;

            DicConsole.DebugWriteLine("DC42 plugin", "Reading data");
            FileStream datastream = new FileStream(dc42ImagePath, FileMode.Open, FileAccess.Read);
            datastream.Seek((dataOffset), SeekOrigin.Begin);
            datastream.Read(data, 0, (int)header.dataSize);
            datastream.Close();

            DicConsole.DebugWriteLine("DC42 plugin", "Calculating data checksum");
            dataChk = DC42CheckSum(data);
            DicConsole.DebugWriteLine("DC42 plugin", "Calculated data checksum = 0x{0:X8}", dataChk);
            DicConsole.DebugWriteLine("DC42 plugin", "Stored data checksum = 0x{0:X8}", header.dataChecksum);

            if(header.tagSize > 0)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Reading tags");
                FileStream tagstream = new FileStream(dc42ImagePath, FileMode.Open, FileAccess.Read);
                tagstream.Seek((tagOffset), SeekOrigin.Begin);
                tagstream.Read(tags, 0, (int)header.tagSize);
                tagstream.Close();

                DicConsole.DebugWriteLine("DC42 plugin", "Calculating tag checksum");
                tagsChk = DC42CheckSum(tags);
                DicConsole.DebugWriteLine("DC42 plugin", "Calculated tag checksum = 0x{0:X8}", tagsChk);
                DicConsole.DebugWriteLine("DC42 plugin", "Stored tag checksum = 0x{0:X8}", header.tagChecksum);
            }

            return dataChk == header.dataChecksum && tagsChk == header.tagChecksum;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.imageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.sectorSize];

            if(twiggy)
            {
                Array.Copy(twiggyCache, (int)sectorAddress * ImageInfo.sectorSize, buffer, 0, length * ImageInfo.sectorSize);
            }
            else
            {
                FileStream stream = new FileStream(dc42ImagePath, FileMode.Open, FileAccess.Read);
                stream.Seek((long)(dataOffset + sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);
                stream.Read(buffer, 0, (int)(length * ImageInfo.sectorSize));
                stream.Close();
            }

            return buffer;
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException(string.Format("Tag {0} not supported by image format", tag));

            if(header.tagSize == 0)
                throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * bptag];

            if(twiggy)
            {
                Array.Copy(twiggyCacheTags, (int)sectorAddress * bptag, buffer, 0, length * bptag);
            }
            else
            {
                FileStream stream = new FileStream(dc42ImagePath, FileMode.Open, FileAccess.Read);
                stream.Seek((long)(tagOffset + sectorAddress * bptag), SeekOrigin.Begin);
                stream.Read(buffer, 0, (int)(length * bptag));
                stream.Close();
            }

            return buffer;
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] data = ReadSectors(sectorAddress, length);
            byte[] tags = ReadSectorsTag(sectorAddress, length, SectorTagType.AppleSectorTag);
            byte[] buffer = new byte[data.Length + tags.Length];

            for(uint i = 0; i < length; i++)
            {
                Array.Copy(data, i * (ImageInfo.sectorSize), buffer, i * (ImageInfo.sectorSize + bptag), ImageInfo.sectorSize);
                Array.Copy(tags, i * (bptag), buffer, i * (ImageInfo.sectorSize + bptag) + ImageInfo.sectorSize, bptag);
            }

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Apple DiskCopy 4.2";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override string GetImageApplicationVersion()
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

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        #region Unsupported features

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.mediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.mediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.mediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.mediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.mediaPartNumber;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.mediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.lastMediaSequence;
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

        public override List<Partition> GetPartitions()
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

        public override List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        #endregion Unsupported features

        #region Private methods

        private static uint DC42CheckSum(byte[] buffer)
        {
            uint dc42chk = 0;
            if((buffer.Length & 0x01) == 0x01)
                return 0xFFFFFFFF;

            for(uint i = 0; i < buffer.Length; i += 2)
            {
                dc42chk += (uint)(buffer[i] << 8);
                dc42chk += buffer[i + 1];
                dc42chk = (dc42chk >> 1) | (dc42chk << 31);
            }
            return dc42chk;
        }

        #endregion
    }
}

