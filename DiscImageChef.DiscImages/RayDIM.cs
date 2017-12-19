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
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    public class RayDIM : ImagePlugin
    {
        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RayHdr
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)] public byte[] signature;
            public RayDiskTypes diskType;
            public byte cylinders;
            public byte sectorsPerTrack;
            public byte heads;
        }

        enum RayDiskTypes : byte
        {
            Md2dd = 1,
            Md2hd = 2,
            Mf2dd = 3,
            Mf2hd = 4,
            Mf2ed = 5
        }
        #endregion

        readonly string DimSignatureRegEx =
                "Disk IMage VER (?<major>\\d).(?<minor>\\d) Copyright \\(C\\) (?<year>\\d{4}) Ray Arachelian, All Rights Reserved\\."
            ;

        #region Internal variables
        MemoryStream disk;
        #endregion

        public RayDIM()
        {
            Name = "Ray Arachelian's Disk IMage";
            PluginUUID = new Guid("F541F4E7-C1E3-4A2D-B07F-D863E87AB961");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageApplication = "Ray Arachelian's Disk IMage";
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

            if(stream.Length < Marshal.SizeOf(typeof(RayHdr))) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(RayHdr))];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            RayHdr header = new RayHdr();
            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            header = (RayHdr)Marshal.PtrToStructure(ftrPtr, typeof(RayHdr));
            Marshal.FreeHGlobal(ftrPtr);

            string signature = StringHandlers.CToString(header.signature);

            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.signature = {0}", signature);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.diskType = {0}", header.diskType);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.heads = {0}", header.heads);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.cylinders = {0}", header.cylinders);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.sectorsPerTrack = {0}",
                                      header.sectorsPerTrack);

            Regex sx = new Regex(DimSignatureRegEx);
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

            RayHdr header = new RayHdr();
            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            header = (RayHdr)Marshal.PtrToStructure(ftrPtr, typeof(RayHdr));
            Marshal.FreeHGlobal(ftrPtr);

            string signature = StringHandlers.CToString(header.signature);

            Regex sx = new Regex(DimSignatureRegEx);
            Match sm = sx.Match(signature);

            if(!sm.Success) return false;

            ImageInfo.imageApplicationVersion =
                string.Format("{0}.{1}", sm.Groups["major"].Value, sm.Groups["minor"].Value);

            ImageInfo.cylinders = (uint)(header.cylinders + 1);
            ImageInfo.heads = (uint)(header.heads + 1);
            ImageInfo.sectorsPerTrack = header.sectorsPerTrack;
            ImageInfo.sectors = ImageInfo.cylinders * ImageInfo.heads * ImageInfo.sectorsPerTrack;
            ImageInfo.sectorSize = 512;

            byte[] sectors = new byte[ImageInfo.sectorsPerTrack * ImageInfo.sectorSize];
            disk = new MemoryStream();

            for(int i = 0; i < ImageInfo.sectorsPerTrack * ImageInfo.sectorSize; i++)
            {
                stream.Read(sectors, 0, sectors.Length);
                stream.Seek(ImageInfo.sectorsPerTrack, SeekOrigin.Current);
                disk.Write(sectors, 0, sectors.Length);
            }

            switch(header.diskType)
            {
                case RayDiskTypes.Md2hd:
                    if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 15)
                        ImageInfo.mediaType = MediaType.DOS_525_HD;
                    else goto case RayDiskTypes.Md2dd;
                    break;
                case RayDiskTypes.Md2dd:
                    if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 8)
                        ImageInfo.mediaType = MediaType.DOS_525_SS_DD_8;
                    else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 9)
                        ImageInfo.mediaType = MediaType.DOS_525_SS_DD_9;
                    else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 8)
                        ImageInfo.mediaType = MediaType.DOS_525_DS_DD_8;
                    else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 9)
                        ImageInfo.mediaType = MediaType.DOS_525_DS_DD_9;
                    else ImageInfo.mediaType = MediaType.Unknown;
                    break;
                case RayDiskTypes.Mf2ed:
                    if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 36)
                        ImageInfo.mediaType = MediaType.DOS_35_ED;
                    else goto case RayDiskTypes.Mf2hd;
                    break;
                case RayDiskTypes.Mf2hd:
                    if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 18)
                        ImageInfo.mediaType = MediaType.DOS_35_HD;
                    else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 21)
                        ImageInfo.mediaType = MediaType.DMF;
                    else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 82 && ImageInfo.sectorsPerTrack == 21)
                        ImageInfo.mediaType = MediaType.DMF_82;
                    else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 15)
                        ImageInfo.mediaType = MediaType.NEC_35_HD_15;
                    else goto case RayDiskTypes.Mf2dd;
                    break;
                case RayDiskTypes.Mf2dd:
                    if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 9)
                        ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
                    else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 8)
                        ImageInfo.mediaType = MediaType.DOS_35_DS_DD_8;
                    else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 9)
                        ImageInfo.mediaType = MediaType.DOS_35_SS_DD_9;
                    else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 8)
                        ImageInfo.mediaType = MediaType.DOS_35_SS_DD_8;
                    else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 70 && ImageInfo.sectorsPerTrack == 9)
                        ImageInfo.mediaType = MediaType.Apricot_35;
                    else ImageInfo.mediaType = MediaType.Unknown;
                    break;
                default:
                    ImageInfo.mediaType = MediaType.Unknown;
                    break;
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
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.sectorSize];

            disk.Seek((long)(sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);
            disk.Read(buffer, 0, (int)(length * ImageInfo.sectorSize));

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
    }
}