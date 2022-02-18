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
//     Writes QEMU Enhanced Disk images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages
{
    public sealed partial class Qed
    {
        /// <inheritdoc />
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
                ErrorMessage = $"Unsupported media format {mediaType}";

                return false;
            }

            // TODO: Correct this calculation
            if(sectors * sectorSize / DEFAULT_CLUSTER_SIZE > uint.MaxValue)
            {
                ErrorMessage = "Too many sectors for selected cluster size";

                return false;
            }

            _imageInfo = new ImageInfo
            {
                MediaType  = mediaType,
                SectorSize = sectorSize,
                Sectors    = sectors
            };

            try
            {
                _writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            _qHdr = new QedHeader
            {
                magic           = QED_MAGIC,
                cluster_size    = DEFAULT_CLUSTER_SIZE,
                table_size      = DEFAULT_TABLE_SIZE,
                header_size     = 1,
                l1_table_offset = DEFAULT_CLUSTER_SIZE,
                image_size      = sectors * sectorSize
            };

            _clusterSectors = _qHdr.cluster_size                    / 512;
            _tableSize      = _qHdr.cluster_size * _qHdr.table_size / 8;

            _l1Table = new ulong[_tableSize];
            _l1Mask  = 0;
            int c = 0;
            _clusterBits = Ctz32(_qHdr.cluster_size);
            _l2Mask      = (_tableSize - 1) << _clusterBits;
            _l1Shift     = _clusterBits + Ctz32(_tableSize);

            for(int i = 0; i < 64; i++)
            {
                _l1Mask <<= 1;

                if(c >= 64 - _l1Shift)
                    continue;

                _l1Mask += 1;
                c++;
            }

            _sectorMask = 0;

            for(int i = 0; i < _clusterBits; i++)
                _sectorMask = (_sectorMask << 1) + 1;

            byte[] empty = new byte[_qHdr.l1_table_offset + (_tableSize * 8)];
            _writingStream.Write(empty, 0, empty.Length);

            IsWriting    = true;
            ErrorMessage = null;

            return true;
        }

        /// <inheritdoc />
        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Writing media tags is not supported.";

            return false;
        }

        /// <inheritdoc />
        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length != _imageInfo.SectorSize)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress >= _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data))
                return true;

            ulong byteAddress = sectorAddress * 512;

            ulong l1Off = (byteAddress & _l1Mask) >> _l1Shift;

            if((long)l1Off >= _l1Table.LongLength)
                throw new ArgumentOutOfRangeException(nameof(l1Off),
                                                      $"Trying to write past L1 table, position {l1Off} of a max {_l1Table.LongLength}");

            if(_l1Table[l1Off] == 0)
            {
                _writingStream.Seek(0, SeekOrigin.End);
                _l1Table[l1Off] = (ulong)_writingStream.Position;
                byte[] l2TableB = new byte[_tableSize * 8];
                _writingStream.Seek(0, SeekOrigin.End);
                _writingStream.Write(l2TableB, 0, l2TableB.Length);
            }

            _writingStream.Position = (long)_l1Table[l1Off];

            ulong l2Off = (byteAddress & _l2Mask) >> _clusterBits;

            _writingStream.Seek((long)(_l1Table[l1Off] + (l2Off * 8)), SeekOrigin.Begin);

            byte[] entry = new byte[8];
            _writingStream.Read(entry, 0, 8);
            ulong offset = BitConverter.ToUInt64(entry, 0);

            if(offset == 0)
            {
                offset = (ulong)_writingStream.Length;
                byte[] cluster = new byte[_qHdr.cluster_size];
                entry = BitConverter.GetBytes(offset);
                _writingStream.Seek((long)(_l1Table[l1Off] + (l2Off * 8)), SeekOrigin.Begin);
                _writingStream.Write(entry, 0, 8);
                _writingStream.Seek(0, SeekOrigin.End);
                _writingStream.Write(cluster, 0, cluster.Length);
            }

            _writingStream.Seek((long)(offset + (byteAddress & _sectorMask)), SeekOrigin.Begin);
            _writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";

            return true;
        }

        // TODO: This can be optimized
        /// <inheritdoc />
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length % _imageInfo.SectorSize != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress + length > _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data))
                return true;

            for(uint i = 0; i < length; i++)
            {
                byte[] tmp = new byte[_imageInfo.SectorSize];
                Array.Copy(data, i * _imageInfo.SectorSize, tmp, 0, _imageInfo.SectorSize);

                if(!WriteSector(tmp, sectorAddress + i))
                    return false;
            }

            ErrorMessage = "";

            return true;
        }

        /// <inheritdoc />
        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        /// <inheritdoc />
        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        /// <inheritdoc />
        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";

                return false;
            }

            byte[] hdr = new byte[Marshal.SizeOf<QedHeader>()];
            MemoryMarshal.Write(hdr, ref _qHdr);

            _writingStream.Seek(0, SeekOrigin.Begin);
            _writingStream.Write(hdr, 0, hdr.Length);

            _writingStream.Seek((long)_qHdr.l1_table_offset, SeekOrigin.Begin);
            byte[] l1TableB = MemoryMarshal.Cast<ulong, byte>(_l1Table).ToArray();
            _writingStream.Write(l1TableB, 0, l1TableB.Length);

            _writingStream.Flush();
            _writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        /// <inheritdoc />
        public bool SetMetadata(ImageInfo metadata) => true;

        /// <inheritdoc />
        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

        /// <inheritdoc />
        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        /// <inheritdoc />
        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        /// <inheritdoc />
        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        /// <inheritdoc />
        public bool SetCicmMetadata(CICMMetadataType metadata) => false;
    }
}