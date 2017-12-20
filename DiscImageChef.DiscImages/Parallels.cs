// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Parallels.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using DiscImageChef.DiscImages;

namespace DiscImageChef.DiscImages
{
    public class Parallels : ImagePlugin
    {
        #region Internal constants
        readonly byte[] parallelsMagic =
            {0x57, 0x69, 0x74, 0x68, 0x6F, 0x75, 0x74, 0x46, 0x72, 0x65, 0x65, 0x53, 0x70, 0x61, 0x63, 0x65};
        readonly byte[] parallelsExtMagic =
            {0x57, 0x69, 0x74, 0x68, 0x6F, 0x75, 0x46, 0x72, 0x65, 0x53, 0x70, 0x61, 0x63, 0x45, 0x78, 0x74};

        const uint PARALLELS_VERSION = 2;

        const uint PARALLELS_INUSE = 0x746F6E59;
        const uint PARALLELS_CLOSED = 0x312E3276;

        const uint PARALLELS_EMPTY = 0x00000001;
        #endregion

        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        /// <summary>
        /// Parallels disk image header, little-endian
        /// </summary>
        struct ParallelsHeader
        {
            /// <summary>
            /// Magic, <see cref="Parallels.parallelsMagic"/> or <see cref="Parallels.parallelsExtMagic"/> 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] magic;
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
            /// Set to <see cref="Parallels.PARALLELS_INUSE"/> if image is opened by any software, <see cref="Parallels.PARALLELS_CLOSED"/> if not, and 0 if old version
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
        uint[] bat;
        long dataOffset;
        uint clusterBytes;
        bool empty;
        Stream imageStream;

        Dictionary<ulong, byte[]> sectorCache;

        const uint MAX_CACHE_SIZE = 16777216;
        uint maxCachedSectors = MAX_CACHE_SIZE / 512;

        public Parallels()
        {
            Name = "Parallels disk image";
            PluginUuid = new Guid("E314DE35-C103-48A3-AD36-990F68523C46");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageVersion = "2";
            ImageInfo.ImageApplication = "Parallels";
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

            if(stream.Length < 512) return false;

            byte[] pHdrB = new byte[Marshal.SizeOf(pHdr)];
            stream.Read(pHdrB, 0, Marshal.SizeOf(pHdr));
            pHdr = new ParallelsHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pHdr));
            Marshal.Copy(pHdrB, 0, headerPtr, Marshal.SizeOf(pHdr));
            pHdr = (ParallelsHeader)Marshal.PtrToStructure(headerPtr, typeof(ParallelsHeader));
            Marshal.FreeHGlobal(headerPtr);

            return parallelsMagic.SequenceEqual(pHdr.magic) || parallelsExtMagic.SequenceEqual(pHdr.magic);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] pHdrB = new byte[Marshal.SizeOf(pHdr)];
            stream.Read(pHdrB, 0, Marshal.SizeOf(pHdr));
            pHdr = new ParallelsHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pHdr));
            Marshal.Copy(pHdrB, 0, headerPtr, Marshal.SizeOf(pHdr));
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

            extended = parallelsExtMagic.SequenceEqual(pHdr.magic);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.extended = {0}", extended);

            DicConsole.DebugWriteLine("Parallels plugin", "Reading BAT");
            bat = new uint[pHdr.bat_entries];
            byte[] batB = new byte[pHdr.bat_entries * 4];
            stream.Read(batB, 0, batB.Length);
            for(int i = 0; i < bat.Length; i++) bat[i] = BitConverter.ToUInt32(batB, i * 4);

            clusterBytes = pHdr.cluster_size * 512;
            if(pHdr.data_off > 0) dataOffset = pHdr.data_off * 512;
            else dataOffset = ((stream.Position / clusterBytes) + (stream.Position % clusterBytes)) * clusterBytes;

            sectorCache = new Dictionary<ulong, byte[]>();

            empty = (pHdr.flags & PARALLELS_EMPTY) == PARALLELS_EMPTY;

            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.Sectors = pHdr.sectors;
            ImageInfo.SectorSize = 512;
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.MediaType = MediaType.GENERIC_HDD;
            ImageInfo.ImageSize = pHdr.sectors * 512;
            ImageInfo.Cylinders = pHdr.cylinders;
            ImageInfo.Heads = pHdr.heads;
            ImageInfo.SectorsPerTrack = (uint)((ImageInfo.Sectors / ImageInfo.Cylinders) / ImageInfo.Heads);
            imageStream = stream;

            return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if(empty) return new byte[512];

            byte[] sector;

            if(sectorCache.TryGetValue(sectorAddress, out sector)) return sector;

            ulong index = sectorAddress / pHdr.cluster_size;
            ulong secOff = sectorAddress % pHdr.cluster_size;

            uint batOff = bat[index];
            ulong imageOff;

            if(batOff == 0) return new byte[512];

            if(extended) imageOff = batOff * clusterBytes;
            else imageOff = batOff * 512;

            byte[] cluster = new byte[clusterBytes];
            imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            imageStream.Read(cluster, 0, (int)clusterBytes);
            sector = new byte[512];
            Array.Copy(cluster, (int)(secOff * 512), sector, 0, 512);

            if(sectorCache.Count > maxCachedSectors) sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            if(empty) return new byte[512 * length];

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
            return "Parallels";
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