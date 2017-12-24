// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RayDIM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Ray Arachelian's disk images.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class RayDim : ImagePlugin
    {
        const string REGEX_SIGNATURE =
                "Disk IMage VER (?<major>\\d).(?<minor>\\d) Copyright \\(C\\) (?<year>\\d{4}) Ray Arachelian, All Rights Reserved\\."
            ;

        MemoryStream disk;

        public RayDim()
        {
            Name = "Ray Arachelian's Disk IMage";
            PluginUuid = new Guid("F541F4E7-C1E3-4A2D-B07F-D863E87AB961");
            ImageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                ImageHasPartitions = false,
                ImageHasSessions = false,
                ImageApplication = "Ray Arachelian's Disk IMage",
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

            if(stream.Length < Marshal.SizeOf(typeof(RayHdr))) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(RayHdr))];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            RayHdr header = (RayHdr)Marshal.PtrToStructure(ftrPtr, typeof(RayHdr));
            Marshal.FreeHGlobal(ftrPtr);

            string signature = StringHandlers.CToString(header.signature);

            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.signature = {0}", signature);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.diskType = {0}", header.diskType);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.heads = {0}", header.heads);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.cylinders = {0}", header.cylinders);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.sectorsPerTrack = {0}",
                                      header.sectorsPerTrack);

            Regex sx = new Regex(REGEX_SIGNATURE);
            Match sm = sx.Match(signature);

            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.signature matches? = {0}",
                                      sm.Success);

            return sm.Success;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < Marshal.SizeOf(typeof(RayHdr))) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(RayHdr))];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            RayHdr header = (RayHdr)Marshal.PtrToStructure(ftrPtr, typeof(RayHdr));
            Marshal.FreeHGlobal(ftrPtr);

            string signature = StringHandlers.CToString(header.signature);

            Regex sx = new Regex(REGEX_SIGNATURE);
            Match sm = sx.Match(signature);

            if(!sm.Success) return false;

            ImageInfo.ImageApplicationVersion = $"{sm.Groups["major"].Value}.{sm.Groups["minor"].Value}";

            ImageInfo.Cylinders = (uint)(header.cylinders + 1);
            ImageInfo.Heads = (uint)(header.heads + 1);
            ImageInfo.SectorsPerTrack = header.sectorsPerTrack;
            ImageInfo.Sectors = ImageInfo.Cylinders * ImageInfo.Heads * ImageInfo.SectorsPerTrack;
            ImageInfo.SectorSize = 512;

            byte[] sectors = new byte[ImageInfo.SectorsPerTrack * ImageInfo.SectorSize];
            disk = new MemoryStream();

            for(int i = 0; i < ImageInfo.SectorsPerTrack * ImageInfo.SectorSize; i++)
            {
                stream.Read(sectors, 0, sectors.Length);
                stream.Seek(ImageInfo.SectorsPerTrack, SeekOrigin.Current);
                disk.Write(sectors, 0, sectors.Length);
            }

            switch(header.diskType)
            {
                case RayDiskTypes.Md2hd:
                    if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 15)
                        ImageInfo.MediaType = MediaType.DOS_525_HD;
                    else goto case RayDiskTypes.Md2dd;
                    break;
                case RayDiskTypes.Md2dd:
                    if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 8)
                        ImageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 9)
                        ImageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 8)
                        ImageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 40 && ImageInfo.SectorsPerTrack == 9)
                        ImageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                    else ImageInfo.MediaType = MediaType.Unknown;
                    break;
                case RayDiskTypes.Mf2ed:
                    if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 36)
                        ImageInfo.MediaType = MediaType.DOS_35_ED;
                    else goto case RayDiskTypes.Mf2hd;
                    break;
                case RayDiskTypes.Mf2hd:
                    if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 18)
                        ImageInfo.MediaType = MediaType.DOS_35_HD;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 21)
                        ImageInfo.MediaType = MediaType.DMF;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 82 && ImageInfo.SectorsPerTrack == 21)
                        ImageInfo.MediaType = MediaType.DMF_82;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 15)
                        ImageInfo.MediaType = MediaType.NEC_35_HD_15;
                    else goto case RayDiskTypes.Mf2dd;
                    break;
                case RayDiskTypes.Mf2dd:
                    if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 9)
                        ImageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                    else if(ImageInfo.Heads == 2 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 8)
                        ImageInfo.MediaType = MediaType.DOS_35_DS_DD_8;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 9)
                        ImageInfo.MediaType = MediaType.DOS_35_SS_DD_9;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 80 && ImageInfo.SectorsPerTrack == 8)
                        ImageInfo.MediaType = MediaType.DOS_35_SS_DD_8;
                    else if(ImageInfo.Heads == 1 && ImageInfo.Cylinders == 70 && ImageInfo.SectorsPerTrack == 9)
                        ImageInfo.MediaType = MediaType.Apricot_35;
                    else ImageInfo.MediaType = MediaType.Unknown;
                    break;
                default:
                    ImageInfo.MediaType = MediaType.Unknown;
                    break;
            }

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

            disk.Seek((long)(sectorAddress * ImageInfo.SectorSize), SeekOrigin.Begin);
            disk.Read(buffer, 0, (int)(length * ImageInfo.SectorSize));

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
            return "Ray Arachelian's Disk IMage";
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RayHdr
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)] public byte[] signature;
            public RayDiskTypes diskType;
            public byte cylinders;
            public byte sectorsPerTrack;
            public byte heads;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum RayDiskTypes : byte
        {
            Md2dd = 1,
            Md2hd = 2,
            Mf2dd = 3,
            Mf2hd = 4,
            Mf2ed = 5
        }
    }
}