// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads QEMU Copy-On-Write v2 disk images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages
{
    public partial class Qcow2
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] qHdrB = new byte[Marshal.SizeOf<QCow2Header>()];
            stream.Read(qHdrB, 0, Marshal.SizeOf<QCow2Header>());
            qHdr = Marshal.SpanToStructureBigEndian<QCow2Header>(qHdrB);

            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.magic = 0x{0:X8}", qHdr.magic);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.version = {0}", qHdr.version);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_offset = {0}", qHdr.backing_file_offset);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_size = {0}", qHdr.backing_file_size);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.cluster_bits = {0}", qHdr.cluster_bits);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.size = {0}", qHdr.size);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.crypt_method = {0}", qHdr.crypt_method);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l1_size = {0}", qHdr.l1_size);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l1_table_offset = {0}", qHdr.l1_table_offset);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.refcount_table_offset = {0}", qHdr.refcount_table_offset);

            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.refcount_table_clusters = {0}",
                                       qHdr.refcount_table_clusters);

            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.nb_snapshots = {0}", qHdr.nb_snapshots);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.snapshots_offset = {0}", qHdr.snapshots_offset);

            if(qHdr.version >= QCOW_VERSION3)
            {
                AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.features = {0:X}", qHdr.features);
                AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.compat_features = {0:X}", qHdr.compat_features);
                AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.autoclear_features = {0:X}", qHdr.autoclear_features);
                AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.refcount_order = {0}", qHdr.refcount_order);
                AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.header_length = {0}", qHdr.header_length);

                if((qHdr.features & QCOW_FEATURE_MASK) != 0)
                    throw new
                        ImageNotSupportedException($"Unknown incompatible features {qHdr.features & QCOW_FEATURE_MASK:X} enabled, not proceeding.");
            }

            if(qHdr.size <= 1)
                throw new ArgumentOutOfRangeException(nameof(qHdr.size), "Image size is too small");

            if(qHdr.cluster_bits < 9 ||
               qHdr.cluster_bits > 16)
                throw new ArgumentOutOfRangeException(nameof(qHdr.cluster_bits),
                                                      "Cluster size must be between 512 bytes and 64 Kbytes");

            if(qHdr.crypt_method > QCOW_ENCRYPTION_AES)
                throw new ArgumentOutOfRangeException(nameof(qHdr.crypt_method), "Invalid encryption method");

            if(qHdr.crypt_method > QCOW_ENCRYPTION_NONE)
                throw new NotImplementedException("AES encrypted images not yet supported");

            if(qHdr.backing_file_offset != 0)
                throw new NotImplementedException("Differencing images not yet supported");

            clusterSize    = 1 << (int)qHdr.cluster_bits;
            clusterSectors = 1 << ((int)qHdr.cluster_bits - 9);
            l2Bits         = (int)(qHdr.cluster_bits      - 3);
            l2Size         = 1 << l2Bits;

            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSize = {0}", clusterSize);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSectors = {0}", clusterSectors);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.qHdr.l1_size = {0}", qHdr.l1_size);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Size = {0}", l2Size);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.sectors = {0}", imageInfo.Sectors);

            byte[] l1TableB = new byte[qHdr.l1_size * 8];
            stream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);
            stream.Read(l1TableB, 0, (int)qHdr.l1_size * 8);
            l1Table = MemoryMarshal.Cast<byte, ulong>(l1TableB).ToArray();
            AaruConsole.DebugWriteLine("QCOW plugin", "Reading L1 table");

            for(long i = 0; i < l1Table.LongLength; i++)
                l1Table[i] = Swapping.Swap(l1Table[i]);

            l1Mask = 0;
            int c = 0;
            l1Shift = (int)(l2Bits + qHdr.cluster_bits);

            for(int i = 0; i < 64; i++)
            {
                l1Mask <<= 1;

                if(c >= 64 - l1Shift)
                    continue;

                l1Mask += 1;
                c++;
            }

            l2Mask = 0;

            for(int i = 0; i < l2Bits; i++)
                l2Mask = (l2Mask << 1) + 1;

            l2Mask <<= (int)qHdr.cluster_bits;

            sectorMask = 0;

            for(int i = 0; i < qHdr.cluster_bits; i++)
                sectorMask = (sectorMask << 1) + 1;

            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Mask = {0:X}", l1Mask);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Shift = {0}", l1Shift);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Mask = {0:X}", l2Mask);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.sectorMask = {0:X}", sectorMask);

            maxL2TableCache = MAX_CACHE_SIZE / (l2Size * 8);
            maxClusterCache = MAX_CACHE_SIZE / clusterSize;

            imageStream = stream;

            sectorCache  = new Dictionary<ulong, byte[]>();
            l2TableCache = new Dictionary<ulong, ulong[]>();
            clusterCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = qHdr.size / 512;
            imageInfo.SectorSize           = 512;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.ImageSize            = qHdr.size;
            imageInfo.Version              = $"{qHdr.version}";

            imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
            imageInfo.Heads           = 16;
            imageInfo.SectorsPerTrack = 63;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            // Check cache
            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector))
                return sector;

            ulong byteAddress = sectorAddress * 512;

            ulong l1Off = (byteAddress & l1Mask) >> l1Shift;

            if((long)l1Off >= l1Table.LongLength)
                throw new ArgumentOutOfRangeException(nameof(l1Off),
                                                      $"Trying to read past L1 table, position {l1Off} of a max {l1Table.LongLength}");

            // TODO: Implement differential images
            if(l1Table[l1Off] == 0)
                return new byte[512];

            if(!l2TableCache.TryGetValue(l1Off, out ulong[] l2Table))
            {
                imageStream.Seek((long)(l1Table[l1Off] & QCOW_FLAGS_MASK), SeekOrigin.Begin);
                byte[] l2TableB = new byte[l2Size * 8];
                imageStream.Read(l2TableB, 0, l2Size * 8);
                AaruConsole.DebugWriteLine("QCOW plugin", "Reading L2 table #{0}", l1Off);
                l2Table = MemoryMarshal.Cast<byte, ulong>(l2TableB).ToArray();

                for(long i = 0; i < l2Table.LongLength; i++)
                    l2Table[i] = Swapping.Swap(l2Table[i]);

                if(l2TableCache.Count >= maxL2TableCache)
                    l2TableCache.Clear();

                l2TableCache.Add(l1Off, l2Table);
            }

            ulong l2Off = (byteAddress & l2Mask) >> (int)qHdr.cluster_bits;

            ulong offset = l2Table[l2Off];

            sector = new byte[512];

            if((offset & QCOW_FLAGS_MASK) != 0)
            {
                if(!clusterCache.TryGetValue(offset, out byte[] cluster))
                {
                    if((offset & QCOW_COMPRESSED) == QCOW_COMPRESSED)
                    {
                        ulong compSizeMask = (ulong)(1 << (int)(qHdr.cluster_bits - 8)) - 1;
                        byte  countbits    = (byte)(qHdr.cluster_bits - 8);
                        compSizeMask <<= 62 - countbits;
                        ulong offMask = ~compSizeMask & QCOW_FLAGS_MASK;

                        ulong realOff  = offset & offMask;
                        ulong compSize = (((offset & compSizeMask) >> (62 - countbits)) + 1) * 512;

                        byte[] zCluster = new byte[compSize];
                        imageStream.Seek((long)realOff, SeekOrigin.Begin);
                        imageStream.Read(zCluster, 0, (int)compSize);

                        var zStream = new DeflateStream(new MemoryStream(zCluster), CompressionMode.Decompress);
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

                    if(clusterCache.Count >= maxClusterCache)
                        clusterCache.Clear();

                    clusterCache.Add(offset, cluster);
                }

                Array.Copy(cluster, (int)(byteAddress & sectorMask), sector, 0, 512);
            }

            if(sectorCache.Count >= MAX_CACHED_SECTORS)
                sectorCache.Clear();

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

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}