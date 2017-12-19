// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleDOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages interleaved Apple ][ disk images.
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
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    // Checked using several images and strings inside Apple's DiskImages.framework
    public class AppleDOS : ImagePlugin
    {
        #region Internal variables
        byte[] deinterleaved;
        string extension;
        #endregion

        public AppleDOS()
        {
            Name = "Apple ][ Interleaved Disk Image";
            PluginUUID = new Guid("A5828AC0-62C9-4304-81D4-EFD4AAE47360");
            ImageInfo = new ImageInfo
            {
                readableSectorTags = new List<SectorTagType>(),
                readableMediaTags = new List<MediaTagType>(),
                imageHasPartitions = false,
                imageHasSessions = false,
                imageVersion = null,
                imageApplication = null,
                imageApplicationVersion = null,
                imageCreator = null,
                imageComments = null,
                mediaManufacturer = null,
                mediaModel = null,
                mediaSerialNumber = null,
                mediaBarcode = null,
                mediaPartNumber = null,
                mediaSequence = 0,
                lastMediaSequence = 0,
                driveManufacturer = null,
                driveModel = null,
                driveSerialNumber = null,
                driveFirmwareRevision = null
            };
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            extension = Path.GetExtension(imageFilter.GetFilename()).ToLower();

            if(imageFilter.GetDataForkLength() == 143360 && (extension == ".po" || extension == ".do")) return true;

            return false;
        }

        readonly int[] dosOffsets = {0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15};
        readonly int[] prodosOffsets = {0, 8, 1, 9, 2, 10, 3, 11, 4, 12, 5, 13, 6, 14, 7, 15};

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] tmp = new byte[imageFilter.GetDataForkLength()];
            stream.Read(tmp, 0, tmp.Length);

            deinterleaved = new byte[tmp.Length];

            extension = Path.GetExtension(imageFilter.GetFilename()).ToLower();

            int[] offsets;

            if(extension == ".do") offsets = dosOffsets;
            else offsets = prodosOffsets;

            for(int t = 0; t < 35; t++)
            {
                for(int s = 0; s < 16; s++)
                    Array.Copy(tmp, (t * 16 * 256) + (s * 256), deinterleaved, (t * 16 * 256) + (offsets[s] * 256),
                               256);
            }

            ImageInfo.sectorSize = 256;
            ImageInfo.imageSize = (ulong)imageFilter.GetDataForkLength();
            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.sectors = 560;
            ImageInfo.mediaType = MediaType.Apple33SS;
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.cylinders = 35;
            ImageInfo.heads = 2;
            ImageInfo.sectorsPerTrack = 16;

            return true;
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

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.sectorSize];

            Array.Copy(deinterleaved, (int)(sectorAddress * ImageInfo.sectorSize), buffer, 0, buffer.Length);

            return buffer;
        }

        public override string GetImageFormat()
        {
            if(extension == ".po") return "Apple ][ Interleaved Disk Image (ProDOS order)";

            return "Apple ][ Interleaved Disk Image (DOS order)";
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

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
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

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
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

        #region Unsupported features
        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
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

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        #endregion Unsupported features
    }
}