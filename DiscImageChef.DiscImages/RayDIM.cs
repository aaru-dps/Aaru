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
    public class RayDim : IMediaImage
    {
        const string REGEX_SIGNATURE =
                @"Disk IMage VER (?<major>\d).(?<minor>\d) Copyright \(C\) (?<year>\d{4}) Ray Arachelian, All Rights Reserved\."
            ;

        MemoryStream disk;
        ImageInfo imageInfo;

        public RayDim()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                HasPartitions = false,
                HasSessions = false,
                Application = "Ray Arachelian's Disk IMage",
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

        public virtual string Name => "Ray Arachelian's Disk IMage";
        public virtual Guid Id => new Guid("F541F4E7-C1E3-4A2D-B07F-D863E87AB961");
        public virtual ImageInfo Info => imageInfo;

        public virtual string ImageFormat => "Ray Arachelian's Disk IMage";

        public virtual List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public virtual List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public virtual List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public virtual bool IdentifyImage(IFilter imageFilter)
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

        public virtual bool OpenImage(IFilter imageFilter)
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

            imageInfo.ApplicationVersion = $"{sm.Groups["major"].Value}.{sm.Groups["minor"].Value}";

            imageInfo.Cylinders = (uint)(header.cylinders + 1);
            imageInfo.Heads = (uint)(header.heads + 1);
            imageInfo.SectorsPerTrack = header.sectorsPerTrack;
            imageInfo.Sectors = imageInfo.Cylinders * imageInfo.Heads * imageInfo.SectorsPerTrack;
            imageInfo.SectorSize = 512;

            byte[] sectors = new byte[imageInfo.SectorsPerTrack * imageInfo.SectorSize];
            disk = new MemoryStream();

            for(int i = 0; i < imageInfo.SectorsPerTrack * imageInfo.SectorSize; i++)
            {
                stream.Read(sectors, 0, sectors.Length);
                stream.Seek(imageInfo.SectorsPerTrack, SeekOrigin.Current);
                disk.Write(sectors, 0, sectors.Length);
            }

            switch(header.diskType)
            {
                case RayDiskTypes.Md2hd:
                    if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 15)
                        imageInfo.MediaType = MediaType.DOS_525_HD;
                    else goto case RayDiskTypes.Md2dd;
                    break;
                case RayDiskTypes.Md2dd:
                    if(imageInfo.Heads == 1 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 8)
                        imageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
                    else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 9)
                        imageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
                    else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 8)
                        imageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
                    else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 40 && imageInfo.SectorsPerTrack == 9)
                        imageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                    else imageInfo.MediaType = MediaType.Unknown;
                    break;
                case RayDiskTypes.Mf2ed:
                    if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 36)
                        imageInfo.MediaType = MediaType.DOS_35_ED;
                    else goto case RayDiskTypes.Mf2hd;
                    break;
                case RayDiskTypes.Mf2hd:
                    if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 18)
                        imageInfo.MediaType = MediaType.DOS_35_HD;
                    else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 21)
                        imageInfo.MediaType = MediaType.DMF;
                    else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 82 && imageInfo.SectorsPerTrack == 21)
                        imageInfo.MediaType = MediaType.DMF_82;
                    else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 15)
                        imageInfo.MediaType = MediaType.NEC_35_HD_15;
                    else goto case RayDiskTypes.Mf2dd;
                    break;
                case RayDiskTypes.Mf2dd:
                    if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 9)
                        imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                    else if(imageInfo.Heads == 2 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 8)
                        imageInfo.MediaType = MediaType.DOS_35_DS_DD_8;
                    else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 9)
                        imageInfo.MediaType = MediaType.DOS_35_SS_DD_9;
                    else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 80 && imageInfo.SectorsPerTrack == 8)
                        imageInfo.MediaType = MediaType.DOS_35_SS_DD_8;
                    else if(imageInfo.Heads == 1 && imageInfo.Cylinders == 70 && imageInfo.SectorsPerTrack == 9)
                        imageInfo.MediaType = MediaType.Apricot_35;
                    else imageInfo.MediaType = MediaType.Unknown;
                    break;
                default:
                    imageInfo.MediaType = MediaType.Unknown;
                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            return true;
        }

        public virtual bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public virtual bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public virtual bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public virtual bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public virtual bool? VerifyMediaImage()
        {
            return null;
        }

        public virtual byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public virtual byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            disk.Seek((long)(sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
            disk.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }

        public virtual byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public virtual byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
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