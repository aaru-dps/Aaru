// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : QED.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
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
    public class Qed : IMediaImage
    {
        /// <summary>
        ///     Magic number: 'Q', 'E', 'D', 0x00
        /// </summary>
        const uint QED_MAGIC = 0x00444551;

        /// <summary>
        ///     Mask of unsupported incompatible features
        /// </summary>
        const ulong QED_FEATURE_MASK = 0xFFFFFFFFFFFFFFF8;

        /// <summary>
        ///     File is differential (has a backing file)
        /// </summary>
        const ulong QED_FEATURE_BACKING_FILE = 0x01;
        /// <summary>
        ///     Image needs a consistency check before writing
        /// </summary>
        const ulong QED_FEATURE_NEEDS_CHECK = 0x02;
        /// <summary>
        ///     s
        ///     Backing file is a raw disk image
        /// </summary>
        const ulong QED_FEATURE_RAW_BACKING = 0x04;

        const int MAX_CACHE_SIZE = 16777216;

        const uint MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
        int clusterBits;
        Dictionary<ulong, byte[]> clusterCache;
        uint clusterSectors;
        ImageInfo imageInfo;

        Stream imageStream;

        ulong l1Mask;
        int l1Shift;
        ulong[] l1Table;
        ulong l2Mask;
        Dictionary<ulong, ulong[]> l2TableCache;
        uint maxClusterCache;
        uint maxL2TableCache;

        QedHeader qHdr;

        Dictionary<ulong, byte[]> sectorCache;
        ulong sectorMask;
        uint tableSize;

        public Qed()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                HasPartitions = false,
                HasSessions = false,
                Version = "1",
                Application = "QEMU",
                ApplicationVersion = null,
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

        public string Name => "QEMU Enhanced Disk image";
        public Guid Id => new Guid("B9DBB155-A69A-4C10-BF91-96BF431B9BB6");

        public string Format => "QEMU Enhanced Disk";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] qHdrB = new byte[64];
            stream.Read(qHdrB, 0, 64);
            qHdr = new QedHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(64);
            Marshal.Copy(qHdrB, 0, headerPtr, 64);
            qHdr = (QedHeader)Marshal.PtrToStructure(headerPtr, typeof(QedHeader));
            Marshal.FreeHGlobal(headerPtr);

            return qHdr.magic == QED_MAGIC;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] qHdrB = new byte[64];
            stream.Read(qHdrB, 0, 64);
            qHdr = new QedHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(64);
            Marshal.Copy(qHdrB, 0, headerPtr, 64);
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
                throw new ArgumentOutOfRangeException(nameof(qHdr.cluster_size),
                                                      "Cluster size must be between 4 Kbytes and 64 Mbytes");

            if(!IsPowerOfTwo(qHdr.table_size))
                throw new ArgumentOutOfRangeException(nameof(qHdr.table_size), "Table size must be a power of 2");

            if(qHdr.table_size < 1 || qHdr.table_size > 16)
                throw new ArgumentOutOfRangeException(nameof(qHdr.table_size),
                                                      "Table size must be between 1 and 16 clusters");

            if((qHdr.features & QED_FEATURE_MASK) > 0)
                throw new ArgumentOutOfRangeException(nameof(qHdr.features),
                                                      $"Image uses unknown incompatible features {qHdr.features & QED_FEATURE_MASK:X}");

            if((qHdr.features & QED_FEATURE_BACKING_FILE) == QED_FEATURE_BACKING_FILE)
                throw new NotImplementedException("Differencing images not yet supported");

            clusterSectors = qHdr.cluster_size / 512;
            tableSize = qHdr.cluster_size * qHdr.table_size / 8;

            DicConsole.DebugWriteLine("QED plugin", "qHdr.clusterSectors = {0}", clusterSectors);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.tableSize = {0}", tableSize);

            byte[] l1TableB = new byte[tableSize * 8];
            stream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);
            stream.Read(l1TableB, 0, (int)tableSize * 8);
            l1Table = new ulong[tableSize];
            DicConsole.DebugWriteLine("QED plugin", "Reading L1 table");
            for(long i = 0; i < l1Table.LongLength; i++) l1Table[i] = BitConverter.ToUInt64(l1TableB, (int)(i * 8));

            l1Mask = 0;
            int c = 0;
            clusterBits = Ctz32(qHdr.cluster_size);
            l2Mask = (tableSize - 1) << clusterBits;
            l1Shift = clusterBits + Ctz32(tableSize);

            for(int i = 0; i < 64; i++)
            {
                l1Mask <<= 1;

                if(c >= 64 - l1Shift) continue;

                l1Mask += 1;
                c++;
            }

            sectorMask = 0;
            for(int i = 0; i < clusterBits; i++) sectorMask = (sectorMask << 1) + 1;

            DicConsole.DebugWriteLine("QED plugin", "qHdr.clusterBits = {0}", clusterBits);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.l1Mask = {0:X}", l1Mask);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.l1Shift = {0}", l1Shift);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.l2Mask = {0:X}", l2Mask);
            DicConsole.DebugWriteLine("QED plugin", "qHdr.sectorMask = {0:X}", sectorMask);

            maxL2TableCache = MAX_CACHE_SIZE / tableSize;
            maxClusterCache = MAX_CACHE_SIZE / qHdr.cluster_size;

            imageStream = stream;

            sectorCache = new Dictionary<ulong, byte[]>();
            l2TableCache = new Dictionary<ulong, ulong[]>();
            clusterCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors = qHdr.image_size / 512;
            imageInfo.SectorSize = 512;
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            imageInfo.MediaType = MediaType.GENERIC_HDD;
            imageInfo.ImageSize = qHdr.image_size;

            imageInfo.Cylinders = (uint)(imageInfo.Sectors / 16 / 63);
            imageInfo.Heads = 16;
            imageInfo.SectorsPerTrack = 63;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            // Check cache
            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            ulong byteAddress = sectorAddress * 512;

            ulong l1Off = (byteAddress & l1Mask) >> l1Shift;

            if((long)l1Off >= l1Table.LongLength)
                throw new ArgumentOutOfRangeException(nameof(l1Off),
                                                      $"Trying to read past L1 table, position {l1Off} of a max {l1Table.LongLength}");

            // TODO: Implement differential images
            if(l1Table[l1Off] == 0) return new byte[512];

            if(!l2TableCache.TryGetValue(l1Off, out ulong[] l2Table))
            {
                l2Table = new ulong[tableSize];
                imageStream.Seek((long)l1Table[l1Off], SeekOrigin.Begin);
                byte[] l2TableB = new byte[tableSize * 8];
                imageStream.Read(l2TableB, 0, (int)tableSize * 8);
                DicConsole.DebugWriteLine("QED plugin", "Reading L2 table #{0}", l1Off);
                for(long i = 0; i < l2Table.LongLength; i++) l2Table[i] = BitConverter.ToUInt64(l2TableB, (int)(i * 8));

                if(l2TableCache.Count >= maxL2TableCache) l2TableCache.Clear();

                l2TableCache.Add(l1Off, l2Table);
            }

            ulong l2Off = (byteAddress & l2Mask) >> clusterBits;

            ulong offset = l2Table[l2Off];

            sector = new byte[512];

            if(offset != 0 && offset != 1)
            {
                if(!clusterCache.TryGetValue(offset, out byte[] cluster))
                {
                    cluster = new byte[qHdr.cluster_size];
                    imageStream.Seek((long)offset, SeekOrigin.Begin);
                    imageStream.Read(cluster, 0, (int)qHdr.cluster_size);

                    if(clusterCache.Count >= maxClusterCache) clusterCache.Clear();

                    clusterCache.Add(offset, cluster);
                }

                Array.Copy(cluster, (int)(byteAddress & sectorMask), sector, 0, 512);
            }

            if(sectorCache.Count >= MAX_CACHED_SECTORS) sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        static bool IsPowerOfTwo(uint x)
        {
            while((x & 1) == 0 && x > 1) x >>= 1;

            return x == 1;
        }

        static int Ctz32(uint val)
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
            if((val & 0x1) == 0) cnt++;

            return cnt;
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadDiskTag(MediaTagType tag)
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

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
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

        public bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        /// <summary>
        ///     QED header, big-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QedHeader
        {
            /// <summary>
            ///     <see cref="Qed.QED_MAGIC" />
            /// </summary>
            public uint magic;
            /// <summary>
            ///     Cluster size in bytes
            /// </summary>
            public uint cluster_size;
            /// <summary>
            ///     L1 and L2 table size in cluster
            /// </summary>
            public uint table_size;
            /// <summary>
            ///     Header size in clusters
            /// </summary>
            public uint header_size;
            /// <summary>
            ///     Incompatible features
            /// </summary>
            public ulong features;
            /// <summary>
            ///     Compatible features
            /// </summary>
            public ulong compat_features;
            /// <summary>
            ///     Self-resetting features
            /// </summary>
            public ulong autoclear_features;
            /// <summary>
            ///     Offset to L1 table
            /// </summary>
            public ulong l1_table_offset;
            /// <summary>
            ///     Image size
            /// </summary>
            public ulong image_size;
            /// <summary>
            ///     Offset inside file to string containing backing file
            /// </summary>
            public ulong backing_file_offset;
            /// <summary>
            ///     Size of <see cref="backing_file_offset" />
            /// </summary>
            public uint backing_file_size;
        }
    }
}