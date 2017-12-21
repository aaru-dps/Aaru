// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : QCOW2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages QEMU Copy-On-Write v2 disk images.
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
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace DiscImageChef.DiscImages
{
    public class Qcow2 : ImagePlugin
    {
        #region Internal constants
        /// <summary>
        /// Magic number: 'Q', 'F', 'I', 0xFB
        /// </summary>
        const uint QCOW_MAGIC = 0x514649FB;
        const uint QCOW_VERSION2 = 2;
        const uint QCOW_VERSION3 = 3;
        const uint QCOW_ENCRYPTION_NONE = 0;
        const uint QCOW_ENCRYPTION_AES = 1;

        const ulong QCOW_FEATURE_DIRTY = 0x01;
        const ulong QCOW_FEATURE_CORRUPT = 0x02;
        const ulong QCOW_FEATURE_MASK = 0xFFFFFFFFFFFFFFFC;

        const ulong QCOW_COMPAT_FEATURE_LAZY_REFCOUNTS = 0x01;
        const ulong QCOW_AUTO_CLEAR_FEATURE_BITMAP = 0x01;

        const ulong QCOW_FLAGS_MASK = 0x3FFFFFFFFFFFFFFF;
        const ulong QCOW_COPIED = 0x8000000000000000;
        const ulong QCOW_COMPRESSED = 0x4000000000000000;

        const ulong QCOW_HEADER_EXTENSION_BACKING_FILE = 0xE2792ACA;
        const ulong QCOW_HEADER_EXTENSION_FEATURE_TABLE = 0x6803F857;
        const ulong QCOW_HEADER_EXTENSION_BITMAPS = 0x23852875;

        const int MAX_CACHE_SIZE = 16777216;
        #endregion

        #region Internal Structures
        /// <summary>
        /// QCOW header, big-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QCow2Header
        {
            /// <summary>
            /// <see cref="Qcow2.QCOW_MAGIC"/>
            /// </summary>
            public uint magic;
            /// <summary>
            /// Must be 1
            /// </summary>
            public uint version;
            /// <summary>
            /// Offset inside file to string containing backing file
            /// </summary>
            public ulong backing_file_offset;
            /// <summary>
            /// Size of <see cref="backing_file_offset"/>
            /// </summary>
            public uint backing_file_size;
            /// <summary>
            /// Cluster bits
            /// </summary>
            public uint cluster_bits;
            /// <summary>
            /// Size in bytes
            /// </summary>
            public ulong size;
            /// <summary>
            /// Encryption method
            /// </summary>
            public uint crypt_method;
            /// <summary>
            /// Size of L1 table
            /// </summary>
            public uint l1_size;
            /// <summary>
            /// Offset to L1 table
            /// </summary>
            public ulong l1_table_offset;
            /// <summary>
            /// Offset to reference count table
            /// </summary>
            public ulong refcount_table_offset;
            /// <summary>
            /// How many clusters does the refcount table span
            /// </summary>
            public uint refcount_table_clusters;
            /// <summary>
            /// Number of snapshots
            /// </summary>
            public uint nb_snapshots;
            /// <summary>
            /// Offset to QCowSnapshotHeader
            /// </summary>
            public ulong snapshots_offset;

            // Added in version 3
            public ulong features;
            public ulong compat_features;
            public ulong autoclear_features;
            public uint refcount_order;
            public uint header_length;
        }
        #endregion

        QCow2Header qHdr;
        int clusterSize;
        int clusterSectors;
        int l2Bits;
        int l2Size;
        ulong[] l1Table;

        ulong l1Mask;
        int l1Shift;
        ulong l2Mask;
        ulong sectorMask;

        Dictionary<ulong, byte[]> sectorCache;
        Dictionary<ulong, byte[]> clusterCache;
        Dictionary<ulong, ulong[]> l2TableCache;

        int maxCachedSectors = MAX_CACHE_SIZE / 512;
        int maxL2TableCache;
        int maxClusterCache;

        Stream imageStream;

        public Qcow2()
        {
            Name = "QEMU Copy-On-Write disk image v2";
            PluginUuid = new Guid("F20107CB-95B3-4398-894B-975261F1E8C5");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageVersion = null;
            ImageInfo.ImageApplication = "QEMU";
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

            byte[] qHdrB = new byte[Marshal.SizeOf(qHdr)];
            stream.Read(qHdrB, 0, Marshal.SizeOf(qHdr));
            qHdr = BigEndianMarshal.ByteArrayToStructureBigEndian<QCow2Header>(qHdrB);

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.magic = 0x{0:X8}", qHdr.magic);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.version = {0}", qHdr.version);

            return qHdr.magic == QCOW_MAGIC && (qHdr.version == QCOW_VERSION2 || qHdr.version == QCOW_VERSION3);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] qHdrB = new byte[Marshal.SizeOf(qHdr)];
            stream.Read(qHdrB, 0, Marshal.SizeOf(qHdr));
            qHdr = BigEndianMarshal.ByteArrayToStructureBigEndian<QCow2Header>(qHdrB);

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.magic = 0x{0:X8}", qHdr.magic);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.version = {0}", qHdr.version);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_offset = {0}", qHdr.backing_file_offset);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_size = {0}", qHdr.backing_file_size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.cluster_bits = {0}", qHdr.cluster_bits);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.size = {0}", qHdr.size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.crypt_method = {0}", qHdr.crypt_method);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1_size = {0}", qHdr.l1_size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1_table_offset = {0}", qHdr.l1_table_offset);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.refcount_table_offset = {0}", qHdr.refcount_table_offset);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.refcount_table_clusters = {0}",
                                      qHdr.refcount_table_clusters);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.nb_snapshots = {0}", qHdr.nb_snapshots);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.snapshots_offset = {0}", qHdr.snapshots_offset);

            if(qHdr.version >= QCOW_VERSION3)
            {
                DicConsole.DebugWriteLine("QCOW plugin", "qHdr.features = {0:X}", qHdr.features);
                DicConsole.DebugWriteLine("QCOW plugin", "qHdr.compat_features = {0:X}", qHdr.compat_features);
                DicConsole.DebugWriteLine("QCOW plugin", "qHdr.autoclear_features = {0:X}", qHdr.autoclear_features);
                DicConsole.DebugWriteLine("QCOW plugin", "qHdr.refcount_order = {0}", qHdr.refcount_order);
                DicConsole.DebugWriteLine("QCOW plugin", "qHdr.header_length = {0}", qHdr.header_length);

                if((qHdr.features & QCOW_FEATURE_MASK) != 0)
                    throw new
                        ImageNotSupportedException($"Unknown incompatible features {qHdr.features & QCOW_FEATURE_MASK:X} enabled, not proceeding.");
            }

            if(qHdr.size <= 1) throw new ArgumentOutOfRangeException(nameof(qHdr.size), "Image size is too small");

            if(qHdr.cluster_bits < 9 || qHdr.cluster_bits > 16)
                throw new ArgumentOutOfRangeException(nameof(qHdr.cluster_bits),
                                                      "Cluster size must be between 512 bytes and 64 Kbytes");

            if(qHdr.crypt_method > QCOW_ENCRYPTION_AES)
                throw new ArgumentOutOfRangeException(nameof(qHdr.crypt_method), "Invalid encryption method");

            if(qHdr.crypt_method > QCOW_ENCRYPTION_NONE)
                throw new NotImplementedException("AES encrypted images not yet supported");

            if(qHdr.backing_file_offset != 0)
                throw new NotImplementedException("Differencing images not yet supported");

            int shift = (int)(qHdr.cluster_bits + l2Bits);

            if(qHdr.size > ulong.MaxValue - (ulong)(1 << shift))
                throw new ArgumentOutOfRangeException(nameof(qHdr.size), "Image is too large");

            clusterSize = 1 << (int)qHdr.cluster_bits;
            clusterSectors = 1 << ((int)qHdr.cluster_bits - 9);
            l2Bits = (int)(qHdr.cluster_bits - 3);
            l2Size = 1 << l2Bits;

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSize = {0}", clusterSize);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSectors = {0}", clusterSectors);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.qHdr.l1_size = {0}", qHdr.l1_size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Size = {0}", l2Size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.sectors = {0}", ImageInfo.Sectors);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] l1TableB = new byte[qHdr.l1_size * 8];
            stream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);
            stream.Read(l1TableB, 0, (int)qHdr.l1_size * 8);
            l1Table = new ulong[qHdr.l1_size];
            // TODO: Optimize this
            DicConsole.DebugWriteLine("QCOW plugin", "Reading L1 table");
            for(long i = 0; i < l1Table.LongLength; i++)
                l1Table[i] = BigEndianBitConverter.ToUInt64(l1TableB, (int)(i * 8));

            l1Mask = 0;
            int c = 0;
            l1Shift = (int)(l2Bits + qHdr.cluster_bits);

            for(int i = 0; i < 64; i++)
            {
                l1Mask <<= 1;

                if(c >= 64 - l1Shift) continue;

                l1Mask += 1;
                c++;
            }

            l2Mask = 0;
            for(int i = 0; i < l2Bits; i++) l2Mask = (l2Mask << 1) + 1;

            l2Mask <<= (int)qHdr.cluster_bits;

            sectorMask = 0;
            for(int i = 0; i < qHdr.cluster_bits; i++) sectorMask = (sectorMask << 1) + 1;

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Mask = {0:X}", l1Mask);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Shift = {0}", l1Shift);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Mask = {0:X}", l2Mask);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.sectorMask = {0:X}", sectorMask);

            maxL2TableCache = MAX_CACHE_SIZE / (l2Size * 8);
            maxClusterCache = MAX_CACHE_SIZE / clusterSize;

            imageStream = stream;

            sectorCache = new Dictionary<ulong, byte[]>();
            l2TableCache = new Dictionary<ulong, ulong[]>();
            clusterCache = new Dictionary<ulong, byte[]>();

            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.Sectors = qHdr.size / 512;
            ImageInfo.SectorSize = 512;
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.MediaType = MediaType.GENERIC_HDD;
            ImageInfo.ImageSize = qHdr.size;
            ImageInfo.ImageVersion = $"{qHdr.version}";

            ImageInfo.Cylinders = (uint)(ImageInfo.Sectors / 16 / 63);
            ImageInfo.Heads = 16;
            ImageInfo.SectorsPerTrack = 63;

            return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            byte[] sector;

            // Check cache
            if(sectorCache.TryGetValue(sectorAddress, out sector)) return sector;

            ulong byteAddress = sectorAddress * 512;

            ulong l1Off = (byteAddress & l1Mask) >> l1Shift;

            if((long)l1Off >= l1Table.LongLength)
                throw new ArgumentOutOfRangeException(nameof(l1Off),
                                                      $"Trying to read past L1 table, position {l1Off} of a max {l1Table.LongLength}");

            // TODO: Implement differential images
            if(l1Table[l1Off] == 0) return new byte[512];

            ulong[] l2Table;

            if(!l2TableCache.TryGetValue(l1Off, out l2Table))
            {
                l2Table = new ulong[l2Size];
                imageStream.Seek((long)(l1Table[l1Off] & QCOW_FLAGS_MASK), SeekOrigin.Begin);
                byte[] l2TableB = new byte[l2Size * 8];
                imageStream.Read(l2TableB, 0, l2Size * 8);
                DicConsole.DebugWriteLine("QCOW plugin", "Reading L2 table #{0}", l1Off);
                for(long i = 0; i < l2Table.LongLength; i++)
                    l2Table[i] = BigEndianBitConverter.ToUInt64(l2TableB, (int)(i * 8));

                if(l2TableCache.Count >= maxL2TableCache) l2TableCache.Clear();

                l2TableCache.Add(l1Off, l2Table);
            }

            ulong l2Off = (byteAddress & l2Mask) >> (int)qHdr.cluster_bits;

            ulong offset = l2Table[l2Off];

            sector = new byte[512];

            if((offset & QCOW_FLAGS_MASK) != 0)
            {
                byte[] cluster;
                if(!clusterCache.TryGetValue(offset, out cluster))
                {
                    if((offset & QCOW_COMPRESSED) == QCOW_COMPRESSED)
                    {
                        ulong compSizeMask;
                        ulong offMask;

                        compSizeMask = (ulong)(1 << (int)(qHdr.cluster_bits - 8)) - 1;
                        byte countbits = (byte)(qHdr.cluster_bits - 8);
                        compSizeMask <<= 62 - countbits;
                        offMask = ~compSizeMask & QCOW_FLAGS_MASK;

                        ulong realOff = offset & offMask;
                        ulong compSize = (((offset & compSizeMask) >> (62 - countbits)) + 1) * 512;

                        byte[] zCluster = new byte[compSize];
                        imageStream.Seek((long)realOff, SeekOrigin.Begin);
                        imageStream.Read(zCluster, 0, (int)compSize);

                        DeflateStream zStream =
                            new DeflateStream(new MemoryStream(zCluster), CompressionMode.Decompress);
                        cluster = new byte[clusterSize];
                        int read = zStream.Read(cluster, 0, clusterSize);

                        if(read != clusterSize)
                            throw new
                                IOException($"Unable to decompress cluster, expected {clusterSize} bytes got {read}");
                    }
                    else
                    {
                        cluster = new byte[clusterSize];
                        imageStream.Seek((long)(offset & QCOW_FLAGS_MASK), SeekOrigin.Begin);
                        imageStream.Read(cluster, 0, clusterSize);
                    }

                    if(clusterCache.Count >= maxClusterCache) clusterCache.Clear();

                    clusterCache.Add(offset, cluster);
                }

                Array.Copy(cluster, (int)(byteAddress & sectorMask), sector, 0, 512);
            }

            if(sectorCache.Count >= maxCachedSectors) sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > ImageInfo.Sectors)
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
            return "QEMU Copy-On-Write";
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