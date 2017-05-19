// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Parallels.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Parallels disk images.
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.ImagePlugins;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class Parallels : ImagePlugin
    {
        #region Internal constants
        readonly byte[] ParallelsMagic = { 0x57, 0x69, 0x74, 0x68, 0x6F, 0x75, 0x74, 0x46, 0x72, 0x65, 0x65, 0x53, 0x70, 0x61, 0x63, 0x65 };
        readonly byte[] ParallelsExtMagic = { 0x57, 0x69, 0x74, 0x68, 0x6F, 0x75, 0x46, 0x72, 0x65, 0x53, 0x70, 0x61, 0x63, 0x45, 0x78, 0x74 };

        const uint ParallelsVersion = 2;

        const uint ParallelsInUse = 0x746F6E59;
        const uint ParallelsClosed = 0x312E3276;

        const uint ParallelsEmpty = 0x00000001;
        #endregion

        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        /// <summary>
        /// Parallels disk image header, little-endian
        /// </summary>
        struct ParallelsHeader
        {
            /// <summary>
            /// Magic, <see cref="ParallelsMagic"/> or <see cref="ParallelsExtMagic"/> 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] magic;
            /// <summary>
            /// Version
            /// </summary>
            public uint version;
            /// <summary>
            /// Disk geometry parameter
            /// </summary>
            public uint heads;
            /// <summary>
            /// Disk geometry parameter
            /// </summary>
            public uint cylinders;
            /// <summary>
            /// Cluser size in sectors
            /// </summary>
            public uint cluster_size;
            /// <summary>
            /// Entries in BAT (clusters in image)
            /// </summary>
            public uint bat_entries;
            /// <summary>
            /// Disk size in sectors
            /// </summary>
            public ulong sectors;
            /// <summary>
            /// Set to <see cref="ParallelsInUse"/> if image is opened by any software, <see cref="ParallelsClosed"/> if not, and 0 if old version
            /// </summary>
            public uint in_use;
            /// <summary>
            /// Offset in sectors to start of data
            /// </summary>
            public uint data_off;
            /// <summary>
            /// Flags
            /// </summary>
            public uint flags;
            /// <summary>
            /// Offset in sectors to format extension
            /// </summary>
            public ulong ext_off;
        }
        #endregion

        bool extended;
        ParallelsHeader pHdr;
        uint[] BAT;
        long dataOffset;
        uint clusterBytes;
        bool empty;
        Stream imageStream;

        Dictionary<ulong, byte[]> sectorCache;

        const uint MaxCacheSize = 16777216;
        uint maxCachedSectors = MaxCacheSize / 512;

        public Parallels()
        {
            Name = "Parallels disk image";
            PluginUUID = new Guid("E314DE35-C103-48A3-AD36-990F68523C46");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = "2";
            ImageInfo.imageApplication = "Parallels";
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

            if(stream.Length < 512)
                return false;

            byte[] pHdr_b = new byte[Marshal.SizeOf(pHdr)];
            stream.Read(pHdr_b, 0, Marshal.SizeOf(pHdr));
            pHdr = new ParallelsHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pHdr));
            Marshal.Copy(pHdr_b, 0, headerPtr, Marshal.SizeOf(pHdr));
            pHdr = (ParallelsHeader)Marshal.PtrToStructure(headerPtr, typeof(ParallelsHeader));
            Marshal.FreeHGlobal(headerPtr);

            return ParallelsMagic.SequenceEqual(pHdr.magic) || ParallelsExtMagic.SequenceEqual(pHdr.magic);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] pHdr_b = new byte[Marshal.SizeOf(pHdr)];
            stream.Read(pHdr_b, 0, Marshal.SizeOf(pHdr));
            pHdr = new ParallelsHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pHdr));
            Marshal.Copy(pHdr_b, 0, headerPtr, Marshal.SizeOf(pHdr));
            pHdr = (ParallelsHeader)Marshal.PtrToStructure(headerPtr, typeof(ParallelsHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.magic = {0}", StringHandlers.CToString(pHdr.magic));
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.version = {0}", pHdr.version);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.heads = {0}", pHdr.heads);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.cylinders = {0}", pHdr.cylinders);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.cluster_size = {0}", pHdr.cluster_size);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.bat_entries = {0}", pHdr.bat_entries);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.sectors = {0}", pHdr.sectors);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.in_use = 0x{0:X8}", pHdr.in_use);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.data_off = {0}", pHdr.data_off);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.flags = {0}", pHdr.flags);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.ext_off = {0}", pHdr.ext_off);

            extended = ParallelsExtMagic.SequenceEqual(pHdr.magic);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.extended = {0}", extended);

            DicConsole.DebugWriteLine("Parallels plugin", "Reading BAT");
            BAT = new uint[pHdr.bat_entries];
            byte[] BAT_b = new byte[pHdr.bat_entries * 4];
            stream.Read(BAT_b, 0, BAT_b.Length);
            for(int i = 0; i < BAT.Length; i++)
                BAT[i] = BitConverter.ToUInt32(BAT_b, i * 4);

            clusterBytes = pHdr.cluster_size * 512;
            if(pHdr.data_off > 0)
                dataOffset = pHdr.data_off * 512;
            else
                dataOffset = ((stream.Position / clusterBytes) + (stream.Position % clusterBytes)) * clusterBytes;

            sectorCache = new Dictionary<ulong, byte[]>();

            empty = (pHdr.flags & ParallelsEmpty) == ParallelsEmpty;

            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.sectors = pHdr.sectors;
            ImageInfo.sectorSize = 512;
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.mediaType = MediaType.GENERIC_HDD;
            ImageInfo.imageSize = pHdr.sectors * 512;
            imageStream = stream;

            return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            if(empty)
                return new byte[512];

            byte[] sector;

            if(sectorCache.TryGetValue(sectorAddress, out sector))
               return sector;

            ulong index = sectorAddress / pHdr.cluster_size;
            ulong secOff = sectorAddress % pHdr.cluster_size;

            uint batOff = BAT[index];
            ulong imageOff;

            if(batOff == 0)
                return new byte[512];

            if(extended)
                imageOff = batOff * clusterBytes;
            else
                imageOff = batOff * 512;

            byte[] cluster = new byte[clusterBytes];
            imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            imageStream.Read(cluster, 0, (int)clusterBytes);
            sector = new byte[512];
            Array.Copy(cluster, (int)(secOff * 512), sector, 0, 512);

            if(sectorCache.Count > maxCachedSectors)
                sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            if(empty)
                return new byte[512 * length];

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
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
            return "Parallels";
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

        #region Unsupported features

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
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

