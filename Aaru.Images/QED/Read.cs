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
//     Reads QEMU Enhanced Disk images.
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
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages
{
    public partial class Qed
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] qHdrB = new byte[68];
            stream.Read(qHdrB, 0, 68);
            qHdr = Marshal.SpanToStructureLittleEndian<QedHeader>(qHdrB);

            AaruConsole.DebugWriteLine("QED plugin", "qHdr.magic = 0x{0:X8}", qHdr.magic);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.cluster_size = {0}", qHdr.cluster_size);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.table_size = {0}", qHdr.table_size);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.header_size = {0}", qHdr.header_size);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.features = {0}", qHdr.features);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.compat_features = {0}", qHdr.compat_features);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.autoclear_features = {0}", qHdr.autoclear_features);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.l1_table_offset = {0}", qHdr.l1_table_offset);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.image_size = {0}", qHdr.image_size);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.backing_file_offset = {0}", qHdr.backing_file_offset);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.backing_file_size = {0}", qHdr.backing_file_size);

            if(qHdr.image_size <= 1)
                throw new ArgumentOutOfRangeException(nameof(qHdr.image_size), "Image size is too small");

            if(!IsPowerOfTwo(qHdr.cluster_size))
                throw new ArgumentOutOfRangeException(nameof(qHdr.cluster_size), "Cluster size must be a power of 2");

            if(qHdr.cluster_size < 4096 ||
               qHdr.cluster_size > 67108864)
                throw new ArgumentOutOfRangeException(nameof(qHdr.cluster_size),
                                                      "Cluster size must be between 4 Kbytes and 64 Mbytes");

            if(!IsPowerOfTwo(qHdr.table_size))
                throw new ArgumentOutOfRangeException(nameof(qHdr.table_size), "Table size must be a power of 2");

            if(qHdr.table_size < 1 ||
               qHdr.table_size > 16)
                throw new ArgumentOutOfRangeException(nameof(qHdr.table_size),
                                                      "Table size must be between 1 and 16 clusters");

            if((qHdr.features & QED_FEATURE_MASK) > 0)
                throw new ArgumentOutOfRangeException(nameof(qHdr.features),
                                                      $"Image uses unknown incompatible features {qHdr.features & QED_FEATURE_MASK:X}");

            if((qHdr.features & QED_FEATURE_BACKING_FILE) == QED_FEATURE_BACKING_FILE)
                throw new NotImplementedException("Differencing images not yet supported");

            clusterSectors = qHdr.cluster_size                     / 512;
            tableSize      = (qHdr.cluster_size * qHdr.table_size) / 8;

            AaruConsole.DebugWriteLine("QED plugin", "qHdr.clusterSectors = {0}", clusterSectors);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.tableSize = {0}", tableSize);

            byte[] l1TableB = new byte[tableSize * 8];
            stream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);
            stream.Read(l1TableB, 0, (int)tableSize * 8);
            AaruConsole.DebugWriteLine("QED plugin", "Reading L1 table");
            l1Table = MemoryMarshal.Cast<byte, ulong>(l1TableB).ToArray();

            l1Mask = 0;
            int c = 0;
            clusterBits = Ctz32(qHdr.cluster_size);
            l2Mask      = (tableSize - 1) << clusterBits;
            l1Shift     = clusterBits + Ctz32(tableSize);

            for(int i = 0; i < 64; i++)
            {
                l1Mask <<= 1;

                if(c >= 64 - l1Shift)
                    continue;

                l1Mask += 1;
                c++;
            }

            sectorMask = 0;

            for(int i = 0; i < clusterBits; i++)
                sectorMask = (sectorMask << 1) + 1;

            AaruConsole.DebugWriteLine("QED plugin", "qHdr.clusterBits = {0}", clusterBits);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.l1Mask = {0:X}", l1Mask);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.l1Shift = {0}", l1Shift);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.l2Mask = {0:X}", l2Mask);
            AaruConsole.DebugWriteLine("QED plugin", "qHdr.sectorMask = {0:X}", sectorMask);

            maxL2TableCache = MAX_CACHE_SIZE / tableSize;
            maxClusterCache = MAX_CACHE_SIZE / qHdr.cluster_size;

            imageStream = stream;

            sectorCache  = new Dictionary<ulong, byte[]>();
            l2TableCache = new Dictionary<ulong, ulong[]>();
            clusterCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = qHdr.image_size / 512;
            imageInfo.SectorSize           = 512;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.ImageSize            = qHdr.image_size;

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
                imageStream.Seek((long)l1Table[l1Off], SeekOrigin.Begin);
                byte[] l2TableB = new byte[tableSize * 8];
                imageStream.Read(l2TableB, 0, (int)tableSize * 8);
                AaruConsole.DebugWriteLine("QED plugin", "Reading L2 table #{0}", l1Off);
                l2Table = MemoryMarshal.Cast<byte, ulong>(l2TableB).ToArray();

                if(l2TableCache.Count >= maxL2TableCache)
                    l2TableCache.Clear();

                l2TableCache.Add(l1Off, l2Table);
            }

            ulong l2Off = (byteAddress & l2Mask) >> clusterBits;

            ulong offset = l2Table[l2Off];

            sector = new byte[512];

            if(offset != 0 &&
               offset != 1)
            {
                if(!clusterCache.TryGetValue(offset, out byte[] cluster))
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