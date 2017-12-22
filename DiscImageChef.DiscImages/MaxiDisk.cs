// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MaxiDisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages MaxiDisk images.
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
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class MaxiDisk : ImagePlugin
    {
        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HdkHeader
        {
            public byte unknown;
            public byte diskType;
            public byte heads;
            public byte cylinders;
            public byte bytesPerSector;
            public byte sectorsPerTrack;
            public byte unknown2;
            public byte unknown3;
        }

        enum HdkDiskTypes : byte
        {
            Dos360 = 0,
            Maxi420 = 1,
            Dos720 = 2,
            Maxi800 = 3,
            Dos1200 = 4,
            Maxi1400 = 5,
            Dos1440 = 6,
            Mac1440 = 7,
            Maxi1600 = 8,
            Dmf = 9,
            Dos2880 = 10,
            Maxi3200 = 11
        }
        #endregion

        #region Internal variables
        /// <summary>Disk image file</summary>
        Filter hdkImageFilter;
        #endregion

        public MaxiDisk()
        {
            Name = "MAXI Disk image";
            PluginUuid = new Guid("D27D924A-7034-466E-ADE1-B81EF37E469E");
            ImageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                ImageHasPartitions = false,
                ImageHasSessions = false,
                ImageApplication = "MAXI Disk",
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

            if(stream.Length < 8) return false;

            byte[] buffer = new byte[8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            HdkHeader tmpHeader = (HdkHeader)Marshal.PtrToStructure(ftrPtr, typeof(HdkHeader));
            Marshal.FreeHGlobal(ftrPtr);

            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown = {0}", tmpHeader.unknown);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.diskType = {0}", tmpHeader.diskType);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.heads = {0}", tmpHeader.heads);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.cylinders = {0}", tmpHeader.cylinders);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.bytesPerSector = {0}", tmpHeader.bytesPerSector);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.sectorsPerTrack = {0}",
                                      tmpHeader.sectorsPerTrack);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown2 = {0}", tmpHeader.unknown2);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown3 = {0}", tmpHeader.unknown3);

            // This is hardcoded
            // But its possible values are unknown...
            //if(tmp_header.diskType > 11)
            //    return false;

            // Only floppies supported
            if(tmpHeader.heads == 0 || tmpHeader.heads > 2) return false;

            // No floppies with more than this?
            if(tmpHeader.cylinders > 90) return false;

            // Maximum supported bps is 16384
            if(tmpHeader.bytesPerSector > 7) return false;

            int expectedFileSize = tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack *
                                   (128 << tmpHeader.bytesPerSector) + 8;

            return expectedFileSize == stream.Length;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 8) return false;

            byte[] buffer = new byte[8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            HdkHeader tmpHeader = (HdkHeader)Marshal.PtrToStructure(ftrPtr, typeof(HdkHeader));
            Marshal.FreeHGlobal(ftrPtr);

            // This is hardcoded
            // But its possible values are unknown...
            //if(tmp_header.diskType > 11)
            //    return false;

            // Only floppies supported
            if(tmpHeader.heads == 0 || tmpHeader.heads > 2) return false;

            // No floppies with more than this?
            if(tmpHeader.cylinders > 90) return false;

            // Maximum supported bps is 16384
            if(tmpHeader.bytesPerSector > 7) return false;

            int expectedFileSize = tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack *
                                   (128 << tmpHeader.bytesPerSector) + 8;

            if(expectedFileSize != stream.Length) return false;

            ImageInfo.Cylinders = tmpHeader.cylinders;
            ImageInfo.Heads = tmpHeader.heads;
            ImageInfo.SectorsPerTrack = tmpHeader.sectorsPerTrack;
            ImageInfo.Sectors = (ulong)(tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack);
            ImageInfo.SectorSize = (uint)(128 << tmpHeader.bytesPerSector);

            hdkImageFilter = imageFilter;

            ImageInfo.ImageSize = (ulong)(stream.Length - 8);
            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();

            if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 15 &&
               ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_HD;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 16 &&
                    ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_DS_DD;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 16 &&
                    ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_SS_DD_80;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 10 &&
                    ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ACORN_525_SS_SD_80;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 77 && ImageInfo.SectorsPerTrack == 8 &&
                    ImageInfo.SectorSize == 1024) ImageInfo.MediaType = MediaType.NEC_525_HD;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 8 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 9 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 8 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 9 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 18 &&
                    ImageInfo.SectorSize == 128) ImageInfo.MediaType = MediaType.ATARI_525_SD;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 26 &&
                    ImageInfo.SectorSize == 128) ImageInfo.MediaType = MediaType.ATARI_525_ED;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 18 &&
                    ImageInfo.SectorSize == 256) ImageInfo.MediaType = MediaType.ATARI_525_DD;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 36 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_ED;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 18 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_HD;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 21 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DMF;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 82 && ImageInfo.SectorsPerTrack == 21 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DMF_82;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 77 && ImageInfo.SectorsPerTrack == 8 &&
                    ImageInfo.SectorSize == 1024) ImageInfo.MediaType = MediaType.NEC_35_HD_8;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 15 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.NEC_35_HD_15;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 9 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 8 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_DS_DD_8;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 9 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_SS_DD_9;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 8 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.DOS_35_SS_DD_8;
            else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 5 &&
                    ImageInfo.SectorSize == 1024) ImageInfo.MediaType = MediaType.ACORN_35_DS_DD;
            else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 70 && ImageInfo.SectorsPerTrack == 9 &&
                    ImageInfo.SectorSize == 512) ImageInfo.MediaType = MediaType.Apricot_35;
            else ImageInfo.MediaType = MediaType.Unknown;

            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;

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
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.SectorSize];

            Stream stream = hdkImageFilter.GetDataForkStream();
            stream.Seek((long)(8 + sectorAddress * ImageInfo.SectorSize), SeekOrigin.Begin);
            stream.Read(buffer, 0, (int)(length * ImageInfo.SectorSize));

            return buffer;
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

        public override string GetImageFormat()
        {
            return "MAXI Disk";
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

        #region Unsupported features
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
        #endregion Unsupported features
    }
}