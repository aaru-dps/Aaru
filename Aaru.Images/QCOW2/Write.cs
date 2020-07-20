// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes QEMU Copy-On-Write v2 disk images.
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
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages
{
    public partial class Qcow2
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";

                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";

                return false;
            }

            // TODO: Correct this calculation
            if((sectors * sectorSize) / 65536 > uint.MaxValue)
            {
                ErrorMessage = "Too many sectors for selected cluster size";

                return false;
            }

            imageInfo = new ImageInfo
            {
                MediaType  = mediaType,
                SectorSize = sectorSize,
                Sectors    = sectors
            };

            try
            {
                writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            string extension = Path.GetExtension(path);
            bool   version3  = extension == ".qcow3" || extension == ".qc3";

            qHdr = new QCow2Header
            {
                magic         = QCOW_MAGIC,
                version       = version3 ? QCOW_VERSION3 : QCOW_VERSION2,
                size          = sectors * sectorSize,
                cluster_bits  = 16,
                header_length = (uint)Marshal.SizeOf<QCow2Header>()
            };

            clusterSize    = 1 << (int)qHdr.cluster_bits;
            clusterSectors = 1 << ((int)qHdr.cluster_bits - 9);
            l2Bits         = (int)(qHdr.cluster_bits      - 3);
            l2Size         = 1 << l2Bits;

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

            qHdr.l1_size = (uint)(qHdr.size >> l1Shift);

            if(qHdr.l1_size == 0)
                qHdr.l1_size = 1;

            l1Table = new ulong[qHdr.l1_size];

            ulong clusters       = qHdr.size      / (ulong)clusterSize;
            ulong refCountBlocks = (clusters * 2) / (ulong)clusterSize;

            if(refCountBlocks == 0)
                refCountBlocks = 1;

            qHdr.refcount_table_offset   = (ulong)clusterSize;
            qHdr.refcount_table_clusters = (uint)((refCountBlocks * 8) / (ulong)clusterSize);

            if(qHdr.refcount_table_clusters == 0)
                qHdr.refcount_table_clusters = 1;

            refCountTable        = new ulong[refCountBlocks];
            qHdr.l1_table_offset = qHdr.refcount_table_offset + (ulong)(qHdr.refcount_table_clusters * clusterSize);
            ulong l1TableClusters = (qHdr.l1_size * 8) / (ulong)clusterSize;

            if(l1TableClusters == 0)
                l1TableClusters = 1;

            byte[] empty = new byte[qHdr.l1_table_offset + (l1TableClusters * (ulong)clusterSize)];
            writingStream.Write(empty, 0, empty.Length);

            IsWriting    = true;
            ErrorMessage = null;

            return true;
        }

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Writing media tags is not supported.";

            return false;
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length != imageInfo.SectorSize)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data))
                return true;

            ulong byteAddress = sectorAddress * 512;

            ulong l1Off = (byteAddress & l1Mask) >> l1Shift;

            if((long)l1Off >= l1Table.LongLength)
                throw new ArgumentOutOfRangeException(nameof(l1Off),
                                                      $"Trying to write past L1 table, position {l1Off} of a max {l1Table.LongLength}");

            if(l1Table[l1Off] == 0)
            {
                writingStream.Seek(0, SeekOrigin.End);
                l1Table[l1Off] = (ulong)writingStream.Position;
                byte[] l2TableB = new byte[l2Size * 8];
                writingStream.Seek(0, SeekOrigin.End);
                writingStream.Write(l2TableB, 0, l2TableB.Length);
            }

            writingStream.Position = (long)l1Table[l1Off];

            ulong l2Off = (byteAddress & l2Mask) >> (int)qHdr.cluster_bits;

            writingStream.Seek((long)(l1Table[l1Off] + (l2Off * 8)), SeekOrigin.Begin);

            byte[] entry = new byte[8];
            writingStream.Read(entry, 0, 8);
            ulong offset = BigEndianBitConverter.ToUInt64(entry, 0);

            if(offset == 0)
            {
                offset = (ulong)writingStream.Length;
                byte[] cluster = new byte[clusterSize];
                entry = BigEndianBitConverter.GetBytes(offset);
                writingStream.Seek((long)(l1Table[l1Off] + (l2Off * 8)), SeekOrigin.Begin);
                writingStream.Write(entry, 0, 8);
                writingStream.Seek(0, SeekOrigin.End);
                writingStream.Write(cluster, 0, cluster.Length);
            }

            writingStream.Seek((long)(offset + (byteAddress & sectorMask)), SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            int   refCountBlockEntries = (clusterSize * 8)                  / 16;
            ulong refCountBlockIndex   = (offset      / (ulong)clusterSize) % (ulong)refCountBlockEntries;
            ulong refCountTableIndex   = offset / (ulong)clusterSize        / (ulong)refCountBlockEntries;

            ulong refBlockOffset = refCountTable[refCountTableIndex];

            if(refBlockOffset == 0)
            {
                refBlockOffset                    = (ulong)writingStream.Length;
                refCountTable[refCountTableIndex] = refBlockOffset;
                byte[] cluster = new byte[clusterSize];
                writingStream.Seek(0, SeekOrigin.End);
                writingStream.Write(cluster, 0, cluster.Length);
            }

            writingStream.Seek((long)(refBlockOffset + refCountBlockIndex), SeekOrigin.Begin);

            writingStream.Write(new byte[]
            {
                0, 1
            }, 0, 2);

            ErrorMessage = "";

            return true;
        }

        // TODO: This can be optimized
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length % imageInfo.SectorSize != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data))
                return true;

            for(uint i = 0; i < length; i++)
            {
                byte[] tmp = new byte[imageInfo.SectorSize];
                Array.Copy(data, i * imageInfo.SectorSize, tmp, 0, imageInfo.SectorSize);

                if(!WriteSector(tmp, sectorAddress + i))
                    return false;
            }

            ErrorMessage = "";

            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";

                return false;
            }

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.magic), 0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.version), 0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.backing_file_offset), 0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.backing_file_size), 0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.cluster_bits), 0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.size), 0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.crypt_method), 0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.l1_size), 0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.l1_table_offset), 0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.refcount_table_offset), 0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.refcount_table_clusters), 0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.nb_snapshots), 0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.snapshots_offset), 0, 8);

            if(qHdr.version == QCOW_VERSION3)
            {
                writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.features), 0, 8);
                writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.compat_features), 0, 8);
                writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.autoclear_features), 0, 8);
                writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.refcount_order), 0, 4);
                writingStream.Write(BigEndianBitConverter.GetBytes(qHdr.header_length), 0, 4);
            }

            writingStream.Seek((long)qHdr.refcount_table_offset, SeekOrigin.Begin);

            for(long i = 0; i < refCountTable.LongLength; i++)
                writingStream.Write(BigEndianBitConverter.GetBytes(refCountTable[i]), 0, 8);

            writingStream.Seek((long)qHdr.l1_table_offset, SeekOrigin.Begin);

            for(long i = 0; i < l1Table.LongLength; i++)
                l1Table[i] = Swapping.Swap(l1Table[i]);

            byte[] l1TableB = MemoryMarshal.Cast<ulong, byte>(l1Table).ToArray();
            writingStream.Write(l1TableB, 0, l1TableB.Length);

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        public bool SetMetadata(ImageInfo metadata) => true;

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        public bool SetCicmMetadata(CICMMetadataType metadata) => false;
    }
}