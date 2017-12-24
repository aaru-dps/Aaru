// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Anex86.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Anex86 disk images.
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
    public class Anex86 : ImagePlugin
    {
        Filter anexImageFilter;
        Anex86Header fdihdr;

        public Anex86()
        {
            Name = "Anex86 Disk Image";
            PluginUuid = new Guid("0410003E-6E7B-40E6-9328-BA5651ADF6B7");
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

            fdihdr = new Anex86Header();

            if(stream.Length < Marshal.SizeOf(fdihdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(fdihdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            fdihdr = (Anex86Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Anex86Header));
            handle.Free();

            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.unknown = {0}", fdihdr.unknown);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.hddtype = {0}", fdihdr.hddtype);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.hdrSize = {0}", fdihdr.hdrSize);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.dskSize = {0}", fdihdr.dskSize);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.bps = {0}", fdihdr.bps);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.spt = {0}", fdihdr.spt);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.heads = {0}", fdihdr.heads);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.cylinders = {0}", fdihdr.cylinders);

            return stream.Length == fdihdr.hdrSize + fdihdr.dskSize &&
                   fdihdr.dskSize == fdihdr.bps * fdihdr.spt * fdihdr.heads * fdihdr.cylinders;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            fdihdr = new Anex86Header();

            if(stream.Length < Marshal.SizeOf(fdihdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(fdihdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            fdihdr = (Anex86Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Anex86Header));
            handle.Free();

            ImageInfo.MediaType = MediaType.GENERIC_HDD;

            switch(fdihdr.cylinders)
            {
                case 40:
                    switch(fdihdr.bps)
                    {
                        case 512:
                            switch(fdihdr.spt)
                            {
                                case 8:
                                    switch(fdihdr.heads)
                                    {
                                        case 1:
                                            ImageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
                                            break;
                                        case 2:
                                            ImageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
                                            break;
                                    }

                                    break;
                                case 9:
                                    switch(fdihdr.heads)
                                    {
                                        case 1:
                                            ImageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
                                            break;
                                        case 2:
                                            ImageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                                            break;
                                    }

                                    break;
                            }

                            break;
                    }

                    break;
                case 70:
                    switch(fdihdr.bps)
                    {
                        case 512:
                            switch(fdihdr.spt)
                            {
                                case 9:
                                    if(fdihdr.heads == 1) ImageInfo.MediaType = MediaType.Apricot_35;
                                    break;
                            }

                            break;
                    }

                    break;
                case 77:
                    switch(fdihdr.bps)
                    {
                        case 128:
                            switch(fdihdr.spt)
                            {
                                case 26:
                                    if(fdihdr.heads == 2) ImageInfo.MediaType = MediaType.NEC_8_SD;
                                    break;
                            }

                            break;
                        case 256:
                            switch(fdihdr.spt)
                            {
                                case 26:
                                    if(fdihdr.heads == 2) ImageInfo.MediaType = MediaType.NEC_8_DD;
                                    break;
                            }

                            break;
                        case 512:
                            switch(fdihdr.spt)
                            {
                                case 8:
                                    if(fdihdr.heads == 1) ImageInfo.MediaType = MediaType.Apricot_35;
                                    break;
                            }

                            break;
                        case 1024:
                            switch(fdihdr.spt)
                            {
                                case 8:
                                    if(fdihdr.heads == 2) ImageInfo.MediaType = MediaType.NEC_525_HD;
                                    break;
                            }

                            break;
                    }

                    break;
                case 80:
                    switch(fdihdr.bps)
                    {
                        case 256:
                            switch(fdihdr.spt)
                            {
                                case 16:
                                    switch(fdihdr.heads)
                                    {
                                        case 1:
                                            ImageInfo.MediaType = MediaType.NEC_525_SS;
                                            break;
                                        case 2:
                                            ImageInfo.MediaType = MediaType.NEC_525_DS;
                                            break;
                                    }

                                    break;
                            }

                            break;
                        case 512:
                            switch(fdihdr.spt)
                            {
                                case 8:
                                    switch(fdihdr.heads)
                                    {
                                        case 1:
                                            ImageInfo.MediaType = MediaType.DOS_35_SS_DD_8;
                                            break;
                                        case 2:
                                            ImageInfo.MediaType = MediaType.DOS_35_DS_DD_8;
                                            break;
                                    }

                                    break;
                                case 9:
                                    switch(fdihdr.heads)
                                    {
                                        case 1:
                                            ImageInfo.MediaType = MediaType.DOS_35_SS_DD_9;
                                            break;
                                        case 2:
                                            ImageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                                            break;
                                    }

                                    break;
                                case 15:
                                    if(fdihdr.heads == 2) ImageInfo.MediaType = MediaType.NEC_35_HD_15;
                                    break;
                                case 18:
                                    if(fdihdr.heads == 2) ImageInfo.MediaType = MediaType.DOS_35_HD;
                                    break;
                                case 36:
                                    if(fdihdr.heads == 2) ImageInfo.MediaType = MediaType.DOS_35_ED;
                                    break;
                            }

                            break;
                    }

                    break;
                case 240:
                    switch(fdihdr.bps)
                    {
                        case 512:
                            switch(fdihdr.spt)
                            {
                                case 38:
                                    if(fdihdr.heads == 2) ImageInfo.MediaType = MediaType.NEC_35_TD;
                                    break;
                            }

                            break;
                    }

                    break;
            }

            DicConsole.DebugWriteLine("Anex86 plugin", "MediaType: {0}", ImageInfo.MediaType);

            ImageInfo.ImageSize = (ulong)fdihdr.dskSize;
            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.Sectors = (ulong)(fdihdr.cylinders * fdihdr.heads * fdihdr.spt);
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.SectorSize = (uint)fdihdr.bps;
            ImageInfo.Cylinders = (uint)fdihdr.cylinders;
            ImageInfo.Heads = (uint)fdihdr.heads;
            ImageInfo.SectorsPerTrack = (uint)fdihdr.spt;

            anexImageFilter = imageFilter;

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
            return "Anex86 disk image";
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

            Stream stream = anexImageFilter.GetDataForkStream();

            stream.Seek((long)((ulong)fdihdr.hdrSize + sectorAddress * ImageInfo.SectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * ImageInfo.SectorSize));

            return buffer;
        }

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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Anex86Header
        {
            public int unknown;
            public int hddtype;
            public int hdrSize;
            public int dskSize;
            public int bps;
            public int spt;
            public int heads;
            public int cylinders;
        }
    }
}