// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads QEMU Copy-On-Write disk images.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using Marshal = DiscImageChef.Helpers.Marshal;

namespace DiscImageChef.DiscImages
{
    public partial class Qcow
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] qHdrB = new byte[48];
            stream.Read(qHdrB, 0, 48);
            qHdr = Marshal.SpanToStructureBigEndian<QCowHeader>(qHdrB);

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.magic = 0x{0:X8}",          qHdr.magic);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.version = {0}",             qHdr.version);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_offset = {0}", qHdr.backing_file_offset);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_size = {0}",   qHdr.backing_file_size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.mtime = {0}",               qHdr.mtime);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.size = {0}",                qHdr.size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.cluster_bits = {0}",        qHdr.cluster_bits);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l2_bits = {0}",             qHdr.l2_bits);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.padding = {0}",             qHdr.padding);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.crypt_method = {0}",        qHdr.crypt_method);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1_table_offset = {0}",     qHdr.l1_table_offset);

            if(qHdr.size <= 1) throw new ArgumentOutOfRangeException(nameof(qHdr.size), "Image size is too small");

            if(qHdr.cluster_bits < 9 || qHdr.cluster_bits > 16)
                throw new ArgumentOutOfRangeException(nameof(qHdr.cluster_bits),
                                                      "Cluster size must be between 512 bytes and 64 Kbytes");

            if(qHdr.l2_bits < 9 - 3 || qHdr.l2_bits > 16 - 3)
                throw new ArgumentOutOfRangeException(nameof(qHdr.l2_bits),
                                                      "L2 size must be between 512 bytes and 64 Kbytes");

            if(qHdr.crypt_method > QCOW_ENCRYPTION_AES)
                throw new ArgumentOutOfRangeException(nameof(qHdr.crypt_method), "Invalid encryption method");

            if(qHdr.crypt_method > QCOW_ENCRYPTION_NONE)
                throw new NotImplementedException("AES encrypted images not yet supported");

            if(qHdr.backing_file_offset != 0)
                throw new NotImplementedException("Differencing images not yet supported");

            int shift = qHdr.cluster_bits + qHdr.l2_bits;

            if(qHdr.size > ulong.MaxValue - (ulong)(1 << shift))
                throw new ArgumentOutOfRangeException(nameof(qHdr.size), "Image is too large");

            clusterSize    = 1 << qHdr.cluster_bits;
            clusterSectors = 1 << (qHdr.cluster_bits - 9);
            l1Size         = (uint)((qHdr.size + (ulong)(1 << shift) - 1) >> shift);
            l2Size         = 1 << qHdr.l2_bits;

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSize = {0}",    clusterSize);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSectors = {0}", clusterSectors);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Size = {0}",         l1Size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Size = {0}",         l2Size);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.sectors = {0}",        imageInfo.Sectors);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] l1TableB = new byte[l1Size * 8];
            stream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);
            stream.Read(l1TableB, 0, (int)l1Size * 8);
            l1Table = MemoryMarshal.Cast<byte, ulong>(l1TableB).ToArray();
            DicConsole.DebugWriteLine("QCOW plugin", "Reading L1 table");
            for(long i = 0; i < l1Table.LongLength; i++) l1Table[i] = Swapping.Swap(l1Table[i]);

            l1Mask = 0;
            int c = 0;
            l1Shift = qHdr.l2_bits + qHdr.cluster_bits;

            for(int i = 0; i < 64; i++)
            {
                l1Mask <<= 1;

                if(c >= 64 - l1Shift) continue;

                l1Mask += 1;
                c++;
            }

            l2Mask = 0;
            for(int i = 0; i < qHdr.l2_bits; i++) l2Mask = (l2Mask << 1) + 1;

            l2Mask <<= qHdr.cluster_bits;

            sectorMask = 0;
            for(int i = 0; i < qHdr.cluster_bits; i++) sectorMask = (sectorMask << 1) + 1;

            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Mask = {0:X}",     l1Mask);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Shift = {0}",      l1Shift);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Mask = {0:X}",     l2Mask);
            DicConsole.DebugWriteLine("QCOW plugin", "qHdr.sectorMask = {0:X}", sectorMask);

            maxL2TableCache = MAX_CACHE_SIZE / (l2Size * 8);
            maxClusterCache = MAX_CACHE_SIZE / clusterSize;

            imageStream = stream;

            sectorCache  = new Dictionary<ulong, byte[]>();
            l2TableCache = new Dictionary<ulong, ulong[]>();
            clusterCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = qHdr.mtime > 0
                                                 ? DateHandlers.UnixUnsignedToDateTime(qHdr.mtime)
                                                 : imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle   = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors      = qHdr.size / 512;
            imageInfo.SectorSize   = 512;
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            imageInfo.MediaType    = MediaType.GENERIC_HDD;
            imageInfo.ImageSize    = qHdr.size;

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
                imageStream.Seek((long)l1Table[l1Off], SeekOrigin.Begin);
                byte[] l2TableB = new byte[l2Size * 8];
                imageStream.Read(l2TableB, 0, l2Size * 8);
                DicConsole.DebugWriteLine("QCOW plugin", "Reading L2 table #{0}", l1Off);
                l2Table = MemoryMarshal.Cast<byte, ulong>(l2TableB).ToArray();
                for(long i = 0; i < l2Table.LongLength; i++) l2Table[i] = Swapping.Swap(l2Table[i]);

                if(l2TableCache.Count >= maxL2TableCache) l2TableCache.Clear();

                l2TableCache.Add(l1Off, l2Table);
            }

            ulong l2Off = (byteAddress & l2Mask) >> qHdr.cluster_bits;

            ulong offset = l2Table[l2Off];

            sector = new byte[512];

            if(offset != 0)
            {
                if(!clusterCache.TryGetValue(offset, out byte[] cluster))
                {
                    if((offset & QCOW_COMPRESSED) == QCOW_COMPRESSED)
                    {
                        ulong compSizeMask = (ulong)(1 << qHdr.cluster_bits) - 1;
                        compSizeMask <<= 63 - qHdr.cluster_bits;
                        ulong offMask = ~compSizeMask ^ QCOW_COMPRESSED;

                        ulong realOff  = offset & offMask;
                        ulong compSize = (offset & compSizeMask) >> (63 - qHdr.cluster_bits);

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
                        imageStream.Seek((long)offset, SeekOrigin.Begin);
                        imageStream.Read(cluster, 0, clusterSize);
                    }

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
    }
}