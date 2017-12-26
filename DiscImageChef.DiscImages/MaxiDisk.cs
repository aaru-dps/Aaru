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
    public class MaxiDisk : IMediaImage
    {
        /// <summary>Disk image file</summary>
        IFilter hdkImageFilter;
        ImageInfo imageInfo;

        public MaxiDisk()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                HasPartitions = false,
                HasSessions = false,
                Application = "MAXI Disk",
                Creator = null,
                Comments = null,
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

        public ImageInfo Info => imageInfo;

        public string Name => "MAXI Disk image";
        public Guid Id => new Guid("D27D924A-7034-466E-ADE1-B81EF37E469E");

        public string ImageFormat => "MAXI Disk";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool IdentifyImage(IFilter imageFilter)
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

        public bool OpenImage(IFilter imageFilter)
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

            imageInfo.Cylinders = tmpHeader.cylinders;
            imageInfo.Heads = tmpHeader.heads;
            imageInfo.SectorsPerTrack = tmpHeader.sectorsPerTrack;
            imageInfo.Sectors = (ulong)(tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack);
            imageInfo.SectorSize = (uint)(128 << tmpHeader.bytesPerSector);

            hdkImageFilter = imageFilter;

            imageInfo.ImageSize = (ulong)(stream.Length - 8);
            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

            if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 15 &&
               imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_525_HD;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 16 &&
                    imageInfo.SectorSize == 256) imageInfo.MediaType = MediaType.ACORN_525_DS_DD;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 16 &&
                    imageInfo.SectorSize == 256) imageInfo.MediaType = MediaType.ACORN_525_SS_DD_80;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 10 &&
                    imageInfo.SectorSize == 256) imageInfo.MediaType = MediaType.ACORN_525_SS_SD_80;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 77 && imageInfo.SectorsPerTrack == 8 &&
                    imageInfo.SectorSize == 1024) imageInfo.MediaType = MediaType.NEC_525_HD;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 8 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 9 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 8 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 9 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 18 &&
                    imageInfo.SectorSize == 128) imageInfo.MediaType = MediaType.ATARI_525_SD;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 26 &&
                    imageInfo.SectorSize == 128) imageInfo.MediaType = MediaType.ATARI_525_ED;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 18 &&
                    imageInfo.SectorSize == 256) imageInfo.MediaType = MediaType.ATARI_525_DD;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 36 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_35_ED;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 18 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_35_HD;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 21 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DMF;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 82 && imageInfo.SectorsPerTrack == 21 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DMF_82;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 77 && imageInfo.SectorsPerTrack == 8 &&
                    imageInfo.SectorSize == 1024) imageInfo.MediaType = MediaType.NEC_35_HD_8;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 15 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.NEC_35_HD_15;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 9 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 8 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_35_DS_DD_8;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 9 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_35_SS_DD_9;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 8 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.DOS_35_SS_DD_8;
            else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 5 &&
                    imageInfo.SectorSize == 1024) imageInfo.MediaType = MediaType.ACORN_35_DS_DD;
            else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 70 && imageInfo.SectorsPerTrack == 9 &&
                    imageInfo.SectorSize == 512) imageInfo.MediaType = MediaType.Apricot_35;
            else imageInfo.MediaType = MediaType.Unknown;

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            return true;
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Stream stream = hdkImageFilter.GetDataForkStream();
            stream.Seek((long)(8 + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

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
    }
}