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
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class Blu : ImagePlugin
    {
        const string PROFILE_NAME = "PROFILE      ";
        const string PROFILE10_NAME = "PROFILE 10   ";
        const string WIDGET_NAME = "WIDGET-10    ";
        const string PRIAM_NAME = "PRIAMDTATOWER";
        Filter bluImageFilter;
        int bptag;

        BluHeader imageHeader;

        public Blu()
        {
            Name = "Basic Lisa Utility";
            PluginUuid = new Guid("A153E2F8-4235-432D-9A7F-20807B0BCD74");
            ImageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                ImageHasPartitions = false,
                ImageHasSessions = false,
                ImageVersion = null,
                ImageApplication = null,
                ImageApplicationVersion = null,
                ImageCreator = null,
                ImageComments = null,
                MediaManufacturer = null,
                MediaModel = null,
                MediaSerialNumber = null,
                MediaBarcode = null,
                MediaPartNumber = null,
                MediaSequence = 0,
                LastMediaSequence = 0,
                DriveManufacturer = null,
                DriveModel = null,
                DriveSerialNumber = null,
                DriveFirmwareRevision = null
            };
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 0x200) return false;

            byte[] header = new byte[0x17];
            stream.Read(header, 0, 0x17);

            BluHeader tmpHdr = new BluHeader {DeviceName = new byte[0x0D]};
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            Array.Copy(header, 0, tmpHdr.DeviceName, 0, 0x0D);
            tmpHdr.DeviceType = BigEndianBitConverter.ToUInt32(header, 0x0C) & 0x00FFFFFF;
            tmpHdr.DeviceBlocks = BigEndianBitConverter.ToUInt32(header, 0x11) & 0x00FFFFFF;
            tmpHdr.BytesPerBlock = BigEndianBitConverter.ToUInt16(header, 0x15);

            for(int i = 0; i < 0xD; i++) if(tmpHdr.DeviceName[i] < 0x20) return false;

            return (tmpHdr.BytesPerBlock & 0xFE00) == 0x200;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            imageHeader = new BluHeader {DeviceName = new byte[0x0D]};
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] header = new byte[0x17];
            stream.Read(header, 0, 0x17);
            Array.Copy(header, 0, imageHeader.DeviceName, 0, 0x0D);
            imageHeader.DeviceType = BigEndianBitConverter.ToUInt32(header, 0x0C) & 0x00FFFFFF;
            imageHeader.DeviceBlocks = BigEndianBitConverter.ToUInt32(header, 0x11) & 0x00FFFFFF;
            imageHeader.BytesPerBlock = BigEndianBitConverter.ToUInt16(header, 0x15);

            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceName = \"{0}\"",
                                      StringHandlers.CToString(imageHeader.DeviceName));
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceType = {0}", imageHeader.DeviceType);
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceBlock = {0}", imageHeader.DeviceBlocks);
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.bytesPerBlock = {0}", imageHeader.BytesPerBlock);

            for(int i = 0; i < 0xD; i++) if(imageHeader.DeviceName[i] < 0x20) return false;

            if((imageHeader.BytesPerBlock & 0xFE00) != 0x200) return false;

            stream.Seek(0, SeekOrigin.Begin);
            header = new byte[imageHeader.BytesPerBlock];
            stream.Read(header, 0, imageHeader.BytesPerBlock);

            ImageInfo.SectorSize = 0x200;

            ImageInfo.Sectors = imageHeader.DeviceBlocks;
            ImageInfo.ImageSize = imageHeader.DeviceBlocks * imageHeader.BytesPerBlock;
            bptag = imageHeader.BytesPerBlock - 0x200;
            byte[] hdrTag = new byte[bptag];
            Array.Copy(header, 0x200, hdrTag, 0, bptag);

            switch(StringHandlers.CToString(imageHeader.DeviceName))
            {
                case PROFILE_NAME:
                    ImageInfo.MediaType = ImageInfo.Sectors == 0x2600 ? MediaType.AppleProfile : MediaType.GENERIC_HDD;
                    ImageInfo.Cylinders = 152;
                    ImageInfo.Heads = 4;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case PROFILE10_NAME:
                    ImageInfo.MediaType = ImageInfo.Sectors == 0x4C00 ? MediaType.AppleProfile : MediaType.GENERIC_HDD;
                    ImageInfo.Cylinders = 304;
                    ImageInfo.Heads = 4;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case WIDGET_NAME:
                    ImageInfo.MediaType = ImageInfo.Sectors == 0x4C00 ? MediaType.AppleWidget : MediaType.GENERIC_HDD;
                    ImageInfo.Cylinders = 304;
                    ImageInfo.Heads = 4;
                    ImageInfo.SectorsPerTrack = 16;
                    break;
                case PRIAM_NAME:
                    ImageInfo.MediaType =
                        ImageInfo.Sectors == 0x022C7C ? MediaType.PriamDataTower : MediaType.GENERIC_HDD;
                    // This values are invented...
                    ImageInfo.Cylinders = 419;
                    ImageInfo.Heads = 4;
                    ImageInfo.SectorsPerTrack = 85;
                    break;
                default:
                    ImageInfo.MediaType = MediaType.GENERIC_HDD;
                    ImageInfo.Cylinders = (uint)(ImageInfo.Sectors / 16 / 63);
                    ImageInfo.Heads = 16;
                    ImageInfo.SectorsPerTrack = 63;
                    break;
            }

            ImageInfo.ImageApplication = StringHandlers.CToString(hdrTag);

            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());

            bluImageFilter = imageFilter;

            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            if(bptag > 0) ImageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);

            DicConsole.VerboseWriteLine("BLU image contains a disk of type {0}", ImageInfo.MediaType);

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.ImageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
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
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            int seek = 0;
            int read = 0x200;
            int skip = bptag;

            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * imageHeader.BytesPerBlock), SeekOrigin.Begin);

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
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            if(bptag == 0) throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            int seek = 0x200;
            int read = bptag;
            int skip = 0;

            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * imageHeader.BytesPerBlock), SeekOrigin.Begin);

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
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageHeader.BytesPerBlock];
            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * imageHeader.BytesPerBlock), SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Basic Lisa Utility";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.MediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.MediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.MediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.MediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.MediaPartNumber;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.MediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.LastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.DriveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.DriveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.DriveSerialNumber;
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

        struct BluHeader
        {
            public byte[] DeviceName;
            public uint DeviceType;
            public uint DeviceBlocks;
            public ushort BytesPerBlock;
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

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }
        #endregion Verification, should add tag checksum checks
    }
}