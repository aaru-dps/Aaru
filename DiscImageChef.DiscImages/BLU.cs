// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BLU.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Basic Lisa Utility disk images.
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
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    public class BLU : ImagePlugin
    {
        #region Internal Structures
        struct BLUHeader
        {
            public byte[] deviceName;
            public uint deviceType;
            public uint deviceBlocks;
            public ushort bytesPerBlock;
        }
        #endregion Internal Structures

        #region Internal Constants
        const string profileName   = "PROFILE      ";
        const string profile10Name = "PROFILE 10   ";
        const string widgetName    = "WIDGET-10    ";
        const string priamName     = "PRIAMDTATOWER";
        #endregion Internal Constants

        #region Internal variables

        BLUHeader ImageHeader;
        Filter bluImageFilter;
        int bptag;

        #endregion Internal variables

        #region Public methods
        public BLU()
        {
            Name = "Basic Lisa Utility";
            PluginUUID = new Guid("A153E2F8-4235-432D-9A7F-20807B0BCD74");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplication = null;
            ImageInfo.imageApplicationVersion = null;
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

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 0x200)
                return false;

            byte[] header = new byte[0x17];
            stream.Read(header, 0, 0x17);

            BLUHeader tmpHdr = new BLUHeader();
            tmpHdr.deviceName = new byte[0x0D];
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            Array.Copy(header, 0, tmpHdr.deviceName, 0, 0x0D);
            tmpHdr.deviceType = BigEndianBitConverter.ToUInt32(header, 0x0C) & 0x00FFFFFF;
            tmpHdr.deviceBlocks = BigEndianBitConverter.ToUInt32(header, 0x11) & 0x00FFFFFF;
            tmpHdr.bytesPerBlock = BigEndianBitConverter.ToUInt16(header, 0x15);

            for(int i = 0; i < 0xD; i++)
            {
                if(tmpHdr.deviceName[i] < 0x20)
                    return false;
            }

            if((tmpHdr.bytesPerBlock & 0xFE00) != 0x200)
                return false;

            return true;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            ImageHeader = new BLUHeader();
            ImageHeader.deviceName = new byte[0x0D];
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] header = new byte[0x17];
            stream.Read(header, 0, 0x17);
            Array.Copy(header, 0, ImageHeader.deviceName, 0, 0x0D);
            ImageHeader.deviceType = BigEndianBitConverter.ToUInt32(header, 0x0C) & 0x00FFFFFF;
            ImageHeader.deviceBlocks = BigEndianBitConverter.ToUInt32(header, 0x11) & 0x00FFFFFF;
            ImageHeader.bytesPerBlock = BigEndianBitConverter.ToUInt16(header, 0x15);

            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceName = \"{0}\"", StringHandlers.CToString(ImageHeader.deviceName));
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceType = {0}", ImageHeader.deviceType);
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceBlock = {0}", ImageHeader.deviceBlocks);
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.bytesPerBlock = {0}", ImageHeader.bytesPerBlock);

            for(int i = 0; i < 0xD; i++)
            {
                if(ImageHeader.deviceName[i] < 0x20)
                    return false;
            }

            if((ImageHeader.bytesPerBlock & 0xFE00) != 0x200)
                return false;

            stream.Seek(0, SeekOrigin.Begin);
            header = new byte[ImageHeader.bytesPerBlock];
            stream.Read(header, 0, ImageHeader.bytesPerBlock);


            ImageInfo.sectorSize = 0x200;

            ImageInfo.sectors = ImageHeader.deviceBlocks;
            ImageInfo.imageSize = ImageHeader.deviceBlocks * ImageHeader.bytesPerBlock;
            bptag = ImageHeader.bytesPerBlock - 0x200;
            byte[] hdrTag = new byte[bptag];
            Array.Copy(header, 0x200, hdrTag, 0, bptag);

            switch(StringHandlers.CToString(ImageHeader.deviceName))
            {
                case profileName:
                    if(ImageInfo.sectors == 0x2600)
                        ImageInfo.mediaType = MediaType.AppleProfile;
                    else
                        ImageInfo.mediaType = MediaType.GENERIC_HDD;
					ImageInfo.cylinders = 152;
					ImageInfo.heads = 4;
					ImageInfo.sectorsPerTrack = 16;
					break;
                case profile10Name:
                    if(ImageInfo.sectors == 0x4C00)
                        ImageInfo.mediaType = MediaType.AppleProfile;
                    else
                        ImageInfo.mediaType = MediaType.GENERIC_HDD;
					ImageInfo.cylinders = 304;
					ImageInfo.heads = 4;
					ImageInfo.sectorsPerTrack = 16;
					break;
                case widgetName:
                    if(ImageInfo.sectors == 0x4C00)
                        ImageInfo.mediaType = MediaType.AppleWidget;
                    else
                        ImageInfo.mediaType = MediaType.GENERIC_HDD;
					ImageInfo.cylinders = 304;
					ImageInfo.heads = 4;
					ImageInfo.sectorsPerTrack = 16;
					break;
                case priamName:
                    if(ImageInfo.sectors == 0x022C7C)
                        ImageInfo.mediaType = MediaType.PriamDataTower;
                    else
                        ImageInfo.mediaType = MediaType.GENERIC_HDD;
					// This values are invented...
					ImageInfo.cylinders = 419;
					ImageInfo.heads = 4;
					ImageInfo.sectorsPerTrack = 85;
					break;
                default:
                    ImageInfo.mediaType = MediaType.GENERIC_HDD;
					ImageInfo.cylinders = (uint)((ImageInfo.sectors / 16) / 63);
					ImageInfo.heads = 16;
					ImageInfo.sectorsPerTrack = 63;
                    break;
            }

            ImageInfo.imageApplication = StringHandlers.CToString(hdrTag);

            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());

            bluImageFilter = imageFilter;

            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

            if(bptag > 0)
                ImageInfo.readableSectorTags.Add(SectorTagType.AppleSectorTag);

            DicConsole.VerboseWriteLine("BLU image contains a disk of type {0}", ImageInfo.mediaType);

            return true;
        }

        #region Verification, should add tag checksum checks
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
            return null;
        }
        #endregion Verification, should add tag checksum checks

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

            MemoryStream buffer = new MemoryStream();
            int seek = 0;
            int read = 0x200;
            int skip = bptag;

            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * ImageHeader.bytesPerBlock), SeekOrigin.Begin);

            for(int i = 0; i < length; i++)
            {
                stream.Seek(seek, SeekOrigin.Current);
                byte[] sector = new byte[read];
                stream.Read(sector, 0, read);
                buffer.Write(sector, 0, read);
                stream.Seek(skip, SeekOrigin.Current);
            }

            return buffer.ToArray();
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException(string.Format("Tag {0} not supported by image format", tag));

            if(bptag == 0)
                throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            int seek = 0x200;
            int read = bptag;
            int skip = 0;

            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * ImageHeader.bytesPerBlock), SeekOrigin.Begin);

            for(int i = 0; i < length; i++)
            {
                stream.Seek(seek, SeekOrigin.Current);
                byte[] sector = new byte[read];
                stream.Read(sector, 0, read);
                buffer.Write(sector, 0, read);
                stream.Seek(skip, SeekOrigin.Current);
            }

            return buffer.ToArray();
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

            byte[] buffer = new byte[length * ImageHeader.bytesPerBlock];
            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * ImageHeader.bytesPerBlock), SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Basic Lisa Utility";
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
        #endregion Public methods

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

    }
}
