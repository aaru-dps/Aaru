// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DIM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;
using System.Linq;
using System.Text;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    // Checked using several images and strings inside Apple's DiskImages.framework
    public class DIM : ImagePlugin
    {
        #region Internal enumerations
        enum DiskType : byte
        {
            HD2 = 0,
            HS2 = 1,
            HC2 = 2,
            HDE2 = 3,
            HQ2 = 9,
            N88 = 17
        }
        #endregion

        #region Internal constants
        readonly byte[] HeaderID = { 0x44, 0x49, 0x46, 0x43, 0x20, 0x48, 0x45, 0x41, 0x44, 0x45, 0x52, 0x20, 0x20 };
        #endregion


        #region Internal variables

        /// <summary>Start of data sectors in disk image, should be 0x100</summary>
        const uint dataOffset = 0x100;
        /// <summary>Disk image file</summary>
        Filter dimImageFilter;
        byte[] comment;
        byte[] hdrId;
        DiskType dskType;
        #endregion

        public DIM()
        {
            Name = "DIM Disk Image";
            PluginUUID = new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
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

            if(stream.Length < dataOffset)
                return false;

            comment = new byte[60];
            hdrId = new byte[13];
            stream.Seek(0, SeekOrigin.Begin);
            dskType = (DiskType)stream.ReadByte();
            stream.Seek(0xAB, SeekOrigin.Begin);
            stream.Read(hdrId, 0, 13);
            stream.Seek(0xC2, SeekOrigin.Begin);
            stream.Read(comment, 0, 60);

            return HeaderID.SequenceEqual(hdrId);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < dataOffset)
                return false;

            long diskSize = stream.Length - dataOffset;

            comment = new byte[60];
            hdrId = new byte[13];
            stream.Seek(0, SeekOrigin.Begin);
            dskType = (DiskType)stream.ReadByte();
            stream.Seek(0xAB, SeekOrigin.Begin);
            stream.Read(hdrId, 0, 13);
            stream.Seek(0xC2, SeekOrigin.Begin);
            stream.Read(comment, 0, 60);

            if(!HeaderID.SequenceEqual(hdrId))
                return false;

            ImageInfo.mediaType = MediaType.Unknown;

            switch(dskType)
            {
                // 8 spt, 1024 bps
                case DiskType.HD2:
                    if(diskSize % (2 * 8 * 1024) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 8 * 1024));
                        return false;
                    }
                    if(diskSize / (2 * 8 * 1024) == 77)
                        ImageInfo.mediaType = MediaType.SHARP_525;
                    ImageInfo.sectorSize = 1024;
                    break;
                // 9 spt, 1024 bps
                case DiskType.HS2:
                    if(diskSize % (2 * 9 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 9 * 512));
                        return false;
                    }
                    if(diskSize / (2 * 9 * 512) == 80)
                        ImageInfo.mediaType = MediaType.SHARP_525_9;
                    ImageInfo.sectorSize = 512;
                    break;
                // 15 spt, 512 bps
                case DiskType.HC2:
                    if(diskSize % (2 * 15 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 15 * 512));
                        return false;
                    }
                    if(diskSize / (2 * 15 * 512) == 80)
                        ImageInfo.mediaType = MediaType.DOS_525_HD;
                    ImageInfo.sectorSize = 512;
                    break;
                // 9 spt, 1024 bps
                case DiskType.HDE2:
                    if(diskSize % (2 * 9 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 9 * 512));
                        return false;
                    }
                    if(diskSize / (2 * 9 * 512) == 80)
                        ImageInfo.mediaType = MediaType.SHARP_35_9;
                    ImageInfo.sectorSize = 512;
                    break;
                // 18 spt, 512 bps
                case DiskType.HQ2:
                    if(diskSize % (2 * 18 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 18 * 512));
                        return false;
                    }
                    if(diskSize / (2 * 18 * 512) == 80)
                        ImageInfo.mediaType = MediaType.DOS_35_HD;
                    ImageInfo.sectorSize = 512;
                    break;
                // 26 spt, 256 bps
                case DiskType.N88:
                    if(diskSize % (2 * 26 * 256) == 0)
                    {
                        if(diskSize % (2 * 26 * 256) == 77)
                            ImageInfo.mediaType = MediaType.NEC_525_DD;
                        ImageInfo.sectorSize = 256;
                    }
                    else if(diskSize % (2 * 26 * 128) == 0)
                    {
                        if(diskSize % (2 * 26 * 128) == 77)
                            ImageInfo.mediaType = MediaType.NEC_525_SD;
                        ImageInfo.sectorSize = 256;
                    }
                    else
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 26 * 256));
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            DicConsole.VerboseWriteLine("DIM image contains a disk of type {0}", ImageInfo.mediaType);
            if(!string.IsNullOrEmpty(ImageInfo.imageComments))
                DicConsole.VerboseWriteLine("DIM comments: {0}", ImageInfo.imageComments);
            
            dimImageFilter = imageFilter;

            ImageInfo.imageSize = (ulong)diskSize;
            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.sectors = ImageInfo.imageSize / ImageInfo.sectorSize;
            ImageInfo.imageComments = StringHandlers.CToString(comment, Encoding.GetEncoding(932));
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return false;
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

        public override string GetImageFormat()
        {
            return "DIM disk image";
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

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
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

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.sectorSize];

            Stream stream = dimImageFilter.GetDataForkStream();

            stream.Seek((long)(dataOffset + sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * ImageInfo.sectorSize));

            return buffer;
        }

        #region Unsupported features

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
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

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
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

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetMediaManufacturer()
        {
            return null;
        }

        public override string GetMediaModel()
        {
            return null;
        }

        public override string GetMediaSerialNumber()
        {
            return null;
        }

        public override string GetMediaBarcode()
        {
            return null;
        }

        public override string GetMediaPartNumber()
        {
            return null;
        }

        public override int GetMediaSequence()
        {
            return 0;
        }

        public override int GetLastDiskSequence()
        {
            return 0;
        }

        public override string GetDriveManufacturer()
        {
            return null;
        }

        public override string GetDriveModel()
        {
            return null;
        }

        public override string GetDriveSerialNumber()
        {
            return null;
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

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.sectors; i++)
                UnknownLBAs.Add(i);
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }

        #endregion
    }
}