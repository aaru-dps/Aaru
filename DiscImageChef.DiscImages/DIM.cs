// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DIM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages DIM disk images.
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
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class Dim : ImagePlugin
    {
        #region Internal enumerations
        enum DiskType : byte
        {
            Hd2 = 0,
            Hs2 = 1,
            Hc2 = 2,
            Hde2 = 3,
            Hq2 = 9,
            N88 = 17
        }
        #endregion

        #region Internal constants
        readonly byte[] headerId = {0x44, 0x49, 0x46, 0x43, 0x20, 0x48, 0x45, 0x41, 0x44, 0x45, 0x52, 0x20, 0x20};
        #endregion

        #region Internal variables
        /// <summary>Start of data sectors in disk image, should be 0x100</summary>
        const uint DATA_OFFSET = 0x100;
        /// <summary>Disk image file</summary>
        Filter dimImageFilter;
        byte[] comment;
        byte[] hdrId;
        DiskType dskType;
        #endregion

        public Dim()
        {
            Name = "DIM Disk Image";
            PluginUuid = new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageVersion = null;
            ImageInfo.ImageApplication = null;
            ImageInfo.ImageApplicationVersion = null;
            ImageInfo.ImageCreator = null;
            ImageInfo.ImageComments = null;
            ImageInfo.MediaManufacturer = null;
            ImageInfo.MediaModel = null;
            ImageInfo.MediaSerialNumber = null;
            ImageInfo.MediaBarcode = null;
            ImageInfo.MediaPartNumber = null;
            ImageInfo.MediaSequence = 0;
            ImageInfo.LastMediaSequence = 0;
            ImageInfo.DriveManufacturer = null;
            ImageInfo.DriveModel = null;
            ImageInfo.DriveSerialNumber = null;
            ImageInfo.DriveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < DATA_OFFSET) return false;

            comment = new byte[60];
            hdrId = new byte[13];
            stream.Seek(0, SeekOrigin.Begin);
            dskType = (DiskType)stream.ReadByte();
            stream.Seek(0xAB, SeekOrigin.Begin);
            stream.Read(hdrId, 0, 13);
            stream.Seek(0xC2, SeekOrigin.Begin);
            stream.Read(comment, 0, 60);

            return headerId.SequenceEqual(hdrId);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < DATA_OFFSET) return false;

            long diskSize = stream.Length - DATA_OFFSET;

            comment = new byte[60];
            hdrId = new byte[13];
            stream.Seek(0, SeekOrigin.Begin);
            dskType = (DiskType)stream.ReadByte();
            stream.Seek(0xAB, SeekOrigin.Begin);
            stream.Read(hdrId, 0, 13);
            stream.Seek(0xC2, SeekOrigin.Begin);
            stream.Read(comment, 0, 60);

            if(!headerId.SequenceEqual(hdrId)) return false;

            ImageInfo.MediaType = MediaType.Unknown;

            switch(dskType)
            {
                // 8 spt, 1024 bps
                case DiskType.Hd2:
                    if(diskSize % (2 * 8 * 1024) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 8 * 1024));
                        return false;
                    }

                    if(diskSize / (2 * 8 * 1024) == 77) ImageInfo.MediaType = MediaType.SHARP_525;
                    ImageInfo.SectorSize = 1024;
                    break;
                // 9 spt, 1024 bps
                case DiskType.Hs2:
                    if(diskSize % (2 * 9 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 9 * 512));
                        return false;
                    }

                    if(diskSize / (2 * 9 * 512) == 80) ImageInfo.MediaType = MediaType.SHARP_525_9;
                    ImageInfo.SectorSize = 512;
                    break;
                // 15 spt, 512 bps
                case DiskType.Hc2:
                    if(diskSize % (2 * 15 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 15 * 512));
                        return false;
                    }

                    if(diskSize / (2 * 15 * 512) == 80) ImageInfo.MediaType = MediaType.DOS_525_HD;
                    ImageInfo.SectorSize = 512;
                    break;
                // 9 spt, 1024 bps
                case DiskType.Hde2:
                    if(diskSize % (2 * 9 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 9 * 512));
                        return false;
                    }

                    if(diskSize / (2 * 9 * 512) == 80) ImageInfo.MediaType = MediaType.SHARP_35_9;
                    ImageInfo.SectorSize = 512;
                    break;
                // 18 spt, 512 bps
                case DiskType.Hq2:
                    if(diskSize % (2 * 18 * 512) != 0)
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 18 * 512));
                        return false;
                    }

                    if(diskSize / (2 * 18 * 512) == 80) ImageInfo.MediaType = MediaType.DOS_35_HD;
                    ImageInfo.SectorSize = 512;
                    break;
                // 26 spt, 256 bps
                case DiskType.N88:
                    if(diskSize % (2 * 26 * 256) == 0)
                    {
                        if(diskSize % (2 * 26 * 256) == 77) ImageInfo.MediaType = MediaType.NEC_8_DD;
                        ImageInfo.SectorSize = 256;
                    }
                    else if(diskSize % (2 * 26 * 128) == 0)
                    {
                        if(diskSize % (2 * 26 * 128) == 77) ImageInfo.MediaType = MediaType.NEC_8_SD;
                        ImageInfo.SectorSize = 256;
                    }
                    else
                    {
                        DicConsole.ErrorWriteLine("DIM shows unknown image with {0} tracks", diskSize / (2 * 26 * 256));
                        return false;
                    }

                    break;
                default: return false;
            }

            DicConsole.VerboseWriteLine("DIM image contains a disk of type {0}", ImageInfo.MediaType);
            if(!string.IsNullOrEmpty(ImageInfo.ImageComments))
                DicConsole.VerboseWriteLine("DIM comments: {0}", ImageInfo.ImageComments);

            dimImageFilter = imageFilter;

            ImageInfo.ImageSize = (ulong)diskSize;
            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.Sectors = ImageInfo.ImageSize / ImageInfo.SectorSize;
            ImageInfo.ImageComments = StringHandlers.CToString(comment, Encoding.GetEncoding(932));
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            switch(ImageInfo.MediaType)
            {
                case MediaType.SHARP_525:
                    ImageInfo.Cylinders = 77;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.SHARP_525_9:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_525_HD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.SHARP_35_9:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_35_HD:
                    ImageInfo.Cylinders = 80;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 18;
                    break;
                case MediaType.NEC_8_DD:
                case MediaType.NEC_8_SD:
                    ImageInfo.Cylinders = 77;
                    ImageInfo.Heads = 2;
                    ImageInfo.SectorsPerTrack = 26;
                    break;
            }

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return false;
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

        public override string GetImageFormat()
        {
            return "DIM disk image";
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

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
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

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.SectorSize];

            Stream stream = dimImageFilter.GetDataForkStream();

            stream.Seek((long)(DATA_OFFSET + sectorAddress * ImageInfo.SectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * ImageInfo.SectorSize));

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

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
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