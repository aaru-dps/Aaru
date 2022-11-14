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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

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

public sealed partial class Qcow2
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
        if(sectors * sectorSize / 65536 > uint.MaxValue)
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

        string extension = Path.GetExtension(path);
        bool   version3  = extension is ".qcow3" or ".qc3";

        _qHdr = new Header
        {
            magic         = QCOW_MAGIC,
            version       = version3 ? QCOW_VERSION3 : QCOW_VERSION2,
            size          = sectors * sectorSize,
            cluster_bits  = 16,
            header_length = (uint)Marshal.SizeOf<Header>()
        };

        _clusterSize    = 1 << (int)_qHdr.cluster_bits;
        _clusterSectors = 1 << ((int)_qHdr.cluster_bits - 9);
        _l2Bits         = (int)(_qHdr.cluster_bits      - 3);
        _l2Size         = 1 << _l2Bits;

        _l1Mask = 0;
        var c = 0;
        _l1Shift = (int)(_l2Bits + _qHdr.cluster_bits);

        for(var i = 0; i < 64; i++)
        {
            _l1Mask <<= 1;

            if(c >= 64 - _l1Shift)
                continue;

            _l1Mask += 1;
            c++;
        }

        _l2Mask = 0;

        for(var i = 0; i < _l2Bits; i++)
            _l2Mask = (_l2Mask << 1) + 1;

        _l2Mask <<= (int)_qHdr.cluster_bits;

        _sectorMask = 0;

        for(var i = 0; i < _qHdr.cluster_bits; i++)
            _sectorMask = (_sectorMask << 1) + 1;

        _qHdr.l1_size = (uint)(((long)_qHdr.size + (1 << _l1Shift) - 1) >> _l1Shift);

        if(_qHdr.l1_size == 0)
            _qHdr.l1_size = 1;

        _l1Table = new ulong[_qHdr.l1_size];

        ulong clusters       = _qHdr.size   / (ulong)_clusterSize;
        ulong refCountBlocks = clusters * 2 / (ulong)_clusterSize;

        if(clusters * 2 % (ulong)_clusterSize > 0)
            refCountBlocks++;

        if(refCountBlocks == 0)
            refCountBlocks = 1;

        _qHdr.refcount_table_offset   = (ulong)_clusterSize;
        _qHdr.refcount_table_clusters = (uint)(refCountBlocks * 8 / (ulong)_clusterSize);

        if(_qHdr.refcount_table_clusters == 0)
            _qHdr.refcount_table_clusters = 1;

        _refCountTable        = new ulong[refCountBlocks];
        _qHdr.l1_table_offset = _qHdr.refcount_table_offset + (ulong)(_qHdr.refcount_table_clusters * _clusterSize);
        ulong l1TableClusters = _qHdr.l1_size * 8 / (ulong)_clusterSize;

        if(l1TableClusters == 0)
            l1TableClusters = 1;

        var empty = new byte[_qHdr.l1_table_offset + l1TableClusters * (ulong)_clusterSize];
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
        {
            ErrorMessage = $"Trying to write past L1 table, position {l1Off} of a max {_l1Table.LongLength}";

            return false;
        }

        if(_l1Table[l1Off] == 0)
        {
            _writingStream.Seek(0, SeekOrigin.End);
            _l1Table[l1Off] = (ulong)_writingStream.Position;
            var l2TableB = new byte[_l2Size * 8];
            _writingStream.Seek(0, SeekOrigin.End);
            _writingStream.Write(l2TableB, 0, l2TableB.Length);
        }

        _writingStream.Position = (long)_l1Table[l1Off];

        ulong l2Off = (byteAddress & _l2Mask) >> (int)_qHdr.cluster_bits;

        _writingStream.Seek((long)(_l1Table[l1Off] + l2Off * 8), SeekOrigin.Begin);

        var entry = new byte[8];
        _writingStream.EnsureRead(entry, 0, 8);
        var offset = BigEndianBitConverter.ToUInt64(entry, 0);

        if(offset == 0)
        {
            offset = (ulong)_writingStream.Length;
            var cluster = new byte[_clusterSize];
            entry = BigEndianBitConverter.GetBytes(offset);
            _writingStream.Seek((long)(_l1Table[l1Off] + l2Off * 8), SeekOrigin.Begin);
            _writingStream.Write(entry, 0, 8);
            _writingStream.Seek(0, SeekOrigin.End);
            _writingStream.Write(cluster, 0, cluster.Length);
        }

        _writingStream.Seek((long)(offset + (byteAddress & _sectorMask)), SeekOrigin.Begin);
        _writingStream.Write(data, 0, data.Length);

        int   refCountBlockEntries = _clusterSize * 8 / 16;
        ulong refCountBlockIndex   = offset       / (ulong)_clusterSize % (ulong)refCountBlockEntries;
        ulong refCountTableIndex   = offset / (ulong)_clusterSize / (ulong)refCountBlockEntries;

        ulong refBlockOffset = _refCountTable[refCountTableIndex];

        if(refBlockOffset == 0)
        {
            refBlockOffset                     = (ulong)_writingStream.Length;
            _refCountTable[refCountTableIndex] = refBlockOffset;
            var cluster = new byte[_clusterSize];
            _writingStream.Seek(0, SeekOrigin.End);
            _writingStream.Write(cluster, 0, cluster.Length);
        }

        _writingStream.Seek((long)(refBlockOffset + refCountBlockIndex), SeekOrigin.Begin);

        _writingStream.Write(new byte[]
        {
            0, 1
        }, 0, 2);

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
            var tmp = new byte[_imageInfo.SectorSize];
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

        _writingStream.Seek(0, SeekOrigin.Begin);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.magic), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.version), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.backing_file_offset), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.backing_file_size), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.cluster_bits), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.size), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.crypt_method), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.l1_size), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.l1_table_offset), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.refcount_table_offset), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.refcount_table_clusters), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.nb_snapshots), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.snapshots_offset), 0, 8);

        if(_qHdr.version == QCOW_VERSION3)
        {
            _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.features), 0, 8);
            _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.compat_features), 0, 8);
            _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.autoclear_features), 0, 8);
            _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.refcount_order), 0, 4);
            _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.header_length), 0, 4);
        }

        _writingStream.Seek((long)_qHdr.refcount_table_offset, SeekOrigin.Begin);

        for(long i = 0; i < _refCountTable.LongLength; i++)
            _writingStream.Write(BigEndianBitConverter.GetBytes(_refCountTable[i]), 0, 8);

        _writingStream.Seek((long)_qHdr.l1_table_offset, SeekOrigin.Begin);

        for(long i = 0; i < _l1Table.LongLength; i++)
            _l1Table[i] = Swapping.Swap(_l1Table[i]);

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