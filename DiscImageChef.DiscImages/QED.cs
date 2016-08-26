// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : QED.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages QEMU Enhanced Disk images.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;

namespace DiscImageChef.ImagePlugins
{
    class QED : ImagePlugin
    {
        #region Internal constants
        /// <summary>
        /// Magic number: 'Q', 'E', 'D', 0x00
        /// </summary>
        const uint QedMagic = 0x00444551;

        /// <summary>
        /// Mask of unsupported incompatible features
        /// </summary>
        const ulong QedFeatureMask = 0xFFFFFFFFFFFFFFF8;

        /// <summary>
        /// File is differential (has a backing file)
        /// </summary>
        const ulong QedFeature_BackingFile = 0x01;
        /// <summary>
        /// Image needs a consistency check before writing
        /// </summary>
        const ulong QedFeature_NeedsCheck = 0x02;
        /// <summary>s
        /// Backing file is a raw disk image
        /// </summary>
        const ulong QedFeature_RawBacking = 0x04;

        const int MaxCacheSize = 16777216;
        #endregion

        #region Internal Structures
        /// <summary>
        /// QED header, big-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QedHeader
        {
            /// <summary>
            /// <see cref="QedMagic"/> 
            /// </summary>
            public uint magic;
            /// <summary>
            /// Cluster size in bytes
            /// </summary>
            public uint cluster_size;
            /// <summary>
            /// L1 and L2 table size in cluster
            /// </summary>
            public uint table_size;
            /// <summary>
            /// Header size in clusters
            /// </summary>
            public uint header_size;
            /// <summary>
            /// Incompatible features
            /// </summary>
            public ulong features;
            /// <summary>
            /// Compatible features
            /// </summary>
            public ulong compat_features;
            /// <summary>
            /// Self-resetting features
            /// </summary>
            public ulong autoclear_features;
            /// <summary>
            /// Offset to L1 table
            /// </summary>
            public ulong l1_table_offset;
            /// <summary>
            /// Image size
            /// </summary>
            public ulong image_size;
            /// <summary>
            /// Offset inside file to string containing backing file
            /// </summary>
            public ulong backing_file_offset;
            /// <summary>
            /// Size of <see cref="backing_file_offset"/> 
            /// </summary>
            public uint backing_file_size;
        }
        #endregion

        QedHeader qHdr;
        uint clusterSectors;
        uint tableSize;
        ulong[] l1Table;

        ulong l1Mask;
        int l1Shift;
        ulong l2Mask;
        ulong sectorMask;
        int clusterBits;

        Dictionary<ulong, byte[]> sectorCache;
        Dictionary<ulong, byte[]> clusterCache;
        Dictionary<ulong, ulong[]> l2TableCache;

        uint maxCachedSectors = MaxCacheSize / 512;
        uint maxL2TableCache;
        uint maxClusterCache;

        FileStream imageStream;

        public QED()
        {
            Name = "QEMU Enhanced Disk image";
            PluginUUID = new Guid("B9DBB155-A69A-4C10-BF91-96BF431B9BB6");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = "1";
            ImageInfo.imageApplication = "QEMU";
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

        public override bool IdentifyImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;
            
            byte[] qHdr_b = new byte[64];
            stream.Read(qHdr_b, 0, 64);
            qHdr = new QedHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(64);
            Marshal.Copy(qHdr_b, 0, headerPtr, 64);
            qHdr = (QedHeader)Marshal.PtrToStructure(headerPtr, typeof(QedHeader));
            Marshal.FreeHGlobal(headerPtr);

            return qHdr.magic == QedMagic;
        }

        public override bool OpenImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] qHdr_b = new byte[64];
            stream.Read(qHdr_b, 0, 64);
            qHdr = new QedHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(64);
            Marshal.Copy(qHdr_b, 0, headerPtr, 64);
            qHdr = (QedHeader)Marshal.PtrToStructure(headerPtr, typeof(QedHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("QED plugin", "qHdr.magic = 0x{0:X8}", qHdr.magic);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.cluster_size = {0}", qHdr.cluster_size);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.table_size = {0}", qHdr.table_size);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.header_size = {0}", qHdr.header_size);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.features = {0}", qHdr.features);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.compat_features = {0}", qHdr.compat_features);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.autoclear_features = {0}", qHdr.autoclear_features);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.l1_table_offset = {0}", qHdr.l1_table_offset);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.image_size = {0}", qHdr.image_size);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.backing_file_offset = {0}", qHdr.backing_file_offset);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.backing_file_size = {0}", qHdr.backing_file_size);

            if(qHdr.image_size <= 1)
                throw new ArgumentOutOfRangeException(nameof(qHdr.image_size), "Image size is too small");

            if(!IsPowerOfTwo(qHdr.cluster_size))
                throw new ArgumentOutOfRangeException(nameof(qHdr.cluster_size), "Cluster size must be a power of 2");

            if(qHdr.cluster_size < 4096 || qHdr.cluster_size > 67108864)
                throw new ArgumentOutOfRangeException(nameof(qHdr.cluster_size), "Cluster size must be between 4 Kbytes and 64 Mbytes");
            
            if(!IsPowerOfTwo(qHdr.table_size))
                throw new ArgumentOutOfRangeException(nameof(qHdr.table_size), "Table size must be a power of 2");

            if(qHdr.table_size < 1 || qHdr.table_size > 16)
                throw new ArgumentOutOfRangeException(nameof(qHdr.table_size), "Table size must be between 1 and 16 clusters");

            if((qHdr.features & QedFeatureMask) > 0)
                throw new ArgumentOutOfRangeException(nameof(qHdr.features), string.Format("Image uses unknown incompatible features {0:X}", qHdr.features & QedFeatureMask));

            if((qHdr.features & QedFeature_BackingFile) == QedFeature_BackingFile)
                throw new NotImplementedException("Differencing images not yet supported");

            clusterSectors = qHdr.cluster_size / 512;
            tableSize = (qHdr.cluster_size * qHdr.table_size) / 8;

            DicConsole.DebugWriteLine("QED plugin", "qHdr.clusterSectors = {0}", clusterSectors);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.tableSize = {0}", tableSize);

            byte[] l1Table_b = new byte[tableSize];
            stream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);
            stream.Read(l1Table_b, 0, (int)tableSize);
            l1Table = new ulong[tableSize / 8];
            DicConsole.DebugWriteLine("QED plugin", "Reading L1 table");
            for(long i = 0; i < l1Table.LongLength; i++)
                l1Table[i] = BitConverter.ToUInt64(l1Table_b, (int)(i * 8));

            l1Mask = 0;
            int c = 0;
            clusterBits = ctz32(qHdr.cluster_size);
            l2Mask = tableSize - 1;
            l1Shift = clusterBits + ctz32(tableSize);

            for(int i = 0; i < 64; i++)
            {
                l1Mask <<= 1;

                if(c < 64 - l1Shift)
                {
                    l1Mask += 1;
                    c++;
                }
            }


            sectorMask = 0;
            for(int i = 0; i < clusterBits; i++)
                sectorMask = (sectorMask << 1) + 1;

            DicConsole.DebugWriteLine("QED plugin", "qHdr.clusterBits = {0}", clusterBits);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.l1Mask = {0:X}", l1Mask);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.l1Shift = {0}", l1Shift);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.l2Mask = {0:X}", l2Mask);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.sectorMask = {0:X}", sectorMask);

            maxL2TableCache = MaxCacheSize / (tableSize / 8);
            maxClusterCache = MaxCacheSize / qHdr.cluster_size;

            imageStream = stream;

            sectorCache = new Dictionary<ulong, byte[]>();
            l2TableCache = new Dictionary<ulong, ulong[]>();
            clusterCache = new Dictionary<ulong, byte[]>();

            FileInfo fi = new FileInfo(imagePath);
            ImageInfo.imageCreationTime = fi.CreationTimeUtc;
            ImageInfo.imageLastModificationTime = fi.LastWriteTimeUtc;
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imagePath);
            ImageInfo.sectors = qHdr.image_size / 512;
            ImageInfo.sectorSize = 512;
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.mediaType = MediaType.GENERIC_HDD;
            ImageInfo.imageSize = qHdr.image_size;

            return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;

            // Check cache
            if(sectorCache.TryGetValue(sectorAddress, out sector))
                return sector;

            ulong byteAddress = sectorAddress * 512;

            ulong l1Off = (byteAddress & l1Mask) >> l1Shift;

            if((long)l1Off >= l1Table.LongLength)
                throw new ArgumentOutOfRangeException(nameof(l1Off), string.Format("Trying to read past L1 table, position {0} of a max {1}", l1Off, l1Table.LongLength));

            // TODO: Implement differential images
            if(l1Table[l1Off] == 0)
                return new byte[512];

            ulong[] l2Table;

            if(!l2TableCache.TryGetValue(l1Off, out l2Table))
            {
                l2Table = new ulong[tableSize / 8];
                imageStream.Seek((long)l1Table[l1Off], SeekOrigin.Begin);
                byte[] l2Table_b = new byte[tableSize];
                imageStream.Read(l2Table_b, 0, (int)tableSize);
                DicConsole.DebugWriteLine("QED plugin", "Reading L2 table #{0}", l1Off);
                for(long i = 0; i < l2Table.LongLength; i++)
                    l2Table[i] = BitConverter.ToUInt64(l2Table_b, (int)(i * 8));

                if(l2TableCache.Count >= maxL2TableCache)
                    l2TableCache.Clear();

                l2TableCache.Add(l1Off, l2Table);
            }

            ulong l2Off = (byteAddress & l2Mask) >> clusterBits;

            ulong offset = l2Table[l2Off];

            sector = new byte[512];

            if(offset != 0 && offset != 1)
            {
                byte[] cluster;
                if(!clusterCache.TryGetValue(offset, out cluster))
                {
                    cluster = new byte[qHdr.cluster_size];
                    imageStream.Seek((long)offset, SeekOrigin.Begin);
                    imageStream.Read(cluster, 0, (int)qHdr.cluster_size);

                    if(clusterCache.Count >= maxClusterCache)
                        clusterCache.Clear();

                    clusterCache.Add(offset, cluster);
                }

                Array.Copy(cluster, (int)(byteAddress & sectorMask), sector, 0, 512);
            }

            if(sectorCache.Count >= maxCachedSectors)
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
            return "QEMU Enhanced Disk";
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

        bool IsPowerOfTwo(uint x)
        {
            while(((x & 1) == 0) && x > 1)
                x >>= 1;
            return (x == 1);
        }

        static int ctz32(uint val)
        {
            int cnt = 0;
            if((val & 0xFFFF) == 0)
            {
                cnt += 16;
                val >>= 16;
            }
            if((val & 0xFF) == 0)
            {
                cnt += 8;
                val >>= 8;
            }
            if((val & 0xF) == 0)
            {
                cnt += 4;
                val >>= 4;
            }
            if((val & 0x3) == 0)
            {
                cnt += 2;
                val >>= 2;
            }
            if((val & 0x1) == 0)
            {
                cnt++;
                val >>= 1;
            }
            if((val & 0x1) == 0)
            {
                cnt++;
            }

            return cnt;
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

