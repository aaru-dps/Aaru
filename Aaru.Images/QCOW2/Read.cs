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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages;

public sealed partial class Qcow2
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512)
            return ErrorNumber.InvalidArgument;

        byte[] qHdrB = new byte[Marshal.SizeOf<Header>()];
        stream.Read(qHdrB, 0, Marshal.SizeOf<Header>());
        _qHdr = Marshal.SpanToStructureBigEndian<Header>(qHdrB);

        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.magic = 0x{0:X8}", _qHdr.magic);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.version = {0}", _qHdr.version);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_offset = {0}", _qHdr.backing_file_offset);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.backing_file_size = {0}", _qHdr.backing_file_size);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.cluster_bits = {0}", _qHdr.cluster_bits);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.size = {0}", _qHdr.size);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.crypt_method = {0}", _qHdr.crypt_method);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l1_size = {0}", _qHdr.l1_size);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l1_table_offset = {0}", _qHdr.l1_table_offset);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.refcount_table_offset = {0}", _qHdr.refcount_table_offset);

        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.refcount_table_clusters = {0}",
                                   _qHdr.refcount_table_clusters);

        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.nb_snapshots = {0}", _qHdr.nb_snapshots);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.snapshots_offset = {0}", _qHdr.snapshots_offset);

        if(_qHdr.version >= QCOW_VERSION3)
        {
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.features = {0:X}", _qHdr.features);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.compat_features = {0:X}", _qHdr.compat_features);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.autoclear_features = {0:X}", _qHdr.autoclear_features);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.refcount_order = {0}", _qHdr.refcount_order);
            AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.header_length = {0}", _qHdr.header_length);

            if((_qHdr.features & QCOW_FEATURE_MASK) != 0)
            {
                AaruConsole.
                    ErrorWriteLine($"Unknown incompatible features {_qHdr.features & QCOW_FEATURE_MASK:X} enabled, not proceeding.");

                return ErrorNumber.InvalidArgument;
            }
        }

        if(_qHdr.size <= 1)
        {
            AaruConsole.ErrorWriteLine("Image size is too small");

            return ErrorNumber.InvalidArgument;
        }

        if(_qHdr.cluster_bits < 9 ||
           _qHdr.cluster_bits > 16)
        {
            AaruConsole.ErrorWriteLine("Cluster size must be between 512 bytes and 64 Kbytes");

            return ErrorNumber.InvalidArgument;
        }

        if(_qHdr.crypt_method > QCOW_ENCRYPTION_AES)
        {
            AaruConsole.ErrorWriteLine("Invalid encryption method");

            return ErrorNumber.InvalidArgument;
        }

        if(_qHdr.crypt_method > QCOW_ENCRYPTION_NONE)
        {
            AaruConsole.ErrorWriteLine("AES encrypted images not yet supported");

            return ErrorNumber.NotImplemented;
        }

        if(_qHdr.backing_file_offset != 0)
        {
            AaruConsole.ErrorWriteLine("Differencing images not yet supported");

            return ErrorNumber.NotImplemented;
        }

        _clusterSize    = 1 << (int)_qHdr.cluster_bits;
        _clusterSectors = 1 << ((int)_qHdr.cluster_bits - 9);
        _l2Bits         = (int)(_qHdr.cluster_bits      - 3);
        _l2Size         = 1 << _l2Bits;

        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSize = {0}", _clusterSize);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.clusterSectors = {0}", _clusterSectors);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.qHdr.l1_size = {0}", _qHdr.l1_size);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Size = {0}", _l2Size);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.sectors = {0}", _imageInfo.Sectors);

        byte[] l1TableB = new byte[_qHdr.l1_size * 8];
        stream.Seek((long)_qHdr.l1_table_offset, SeekOrigin.Begin);
        stream.Read(l1TableB, 0, (int)_qHdr.l1_size * 8);
        _l1Table = MemoryMarshal.Cast<byte, ulong>(l1TableB).ToArray();
        AaruConsole.DebugWriteLine("QCOW plugin", "Reading L1 table");

        for(long i = 0; i < _l1Table.LongLength; i++)
            _l1Table[i] = Swapping.Swap(_l1Table[i]);

        _l1Mask = 0;
        int c = 0;
        _l1Shift = (int)(_l2Bits + _qHdr.cluster_bits);

        for(int i = 0; i < 64; i++)
        {
            _l1Mask <<= 1;

            if(c >= 64 - _l1Shift)
                continue;

            _l1Mask += 1;
            c++;
        }

        _l2Mask = 0;

        for(int i = 0; i < _l2Bits; i++)
            _l2Mask = (_l2Mask << 1) + 1;

        _l2Mask <<= (int)_qHdr.cluster_bits;

        _sectorMask = 0;

        for(int i = 0; i < _qHdr.cluster_bits; i++)
            _sectorMask = (_sectorMask << 1) + 1;

        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Mask = {0:X}", _l1Mask);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l1Shift = {0}", _l1Shift);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.l2Mask = {0:X}", _l2Mask);
        AaruConsole.DebugWriteLine("QCOW plugin", "qHdr.sectorMask = {0:X}", _sectorMask);

        _maxL2TableCache = MAX_CACHE_SIZE / (_l2Size * 8);
        _maxClusterCache = MAX_CACHE_SIZE / _clusterSize;

        _imageStream = stream;

        _sectorCache  = new Dictionary<ulong, byte[]>();
        _l2TableCache = new Dictionary<ulong, ulong[]>();
        _clusterCache = new Dictionary<ulong, byte[]>();

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = _qHdr.size / 512;
        _imageInfo.SectorSize           = 512;
        _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.GENERIC_HDD;
        _imageInfo.ImageSize            = _qHdr.size;
        _imageInfo.Version              = $"{_qHdr.version}";

        _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
        _imageInfo.Heads           = 16;
        _imageInfo.SectorsPerTrack = 63;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        // Check cache
        if(_sectorCache.TryGetValue(sectorAddress, out buffer))
            return ErrorNumber.NoError;

        ulong byteAddress = sectorAddress * 512;

        ulong l1Off = (byteAddress & _l1Mask) >> _l1Shift;

        if((long)l1Off >= _l1Table.LongLength)
        {
            AaruConsole.DebugWriteLine("QCOW2 plugin",
                                       $"Trying to read past L1 table, position {l1Off} of a max {_l1Table.LongLength}");

            return ErrorNumber.InvalidArgument;
        }

        // TODO: Implement differential images
        if(_l1Table[l1Off] == 0)
        {
            buffer = new byte[512];

            return ErrorNumber.NoError;
        }

        if(!_l2TableCache.TryGetValue(l1Off, out ulong[] l2Table))
        {
            _imageStream.Seek((long)(_l1Table[l1Off] & QCOW_FLAGS_MASK), SeekOrigin.Begin);
            byte[] l2TableB = new byte[_l2Size * 8];
            _imageStream.Read(l2TableB, 0, _l2Size * 8);
            AaruConsole.DebugWriteLine("QCOW plugin", "Reading L2 table #{0}", l1Off);
            l2Table = MemoryMarshal.Cast<byte, ulong>(l2TableB).ToArray();

            for(long i = 0; i < l2Table.LongLength; i++)
                l2Table[i] = Swapping.Swap(l2Table[i]);

            if(_l2TableCache.Count >= _maxL2TableCache)
                _l2TableCache.Clear();

            _l2TableCache.Add(l1Off, l2Table);
        }

        ulong l2Off = (byteAddress & _l2Mask) >> (int)_qHdr.cluster_bits;

        ulong offset = l2Table[l2Off];

        buffer = new byte[512];

        if((offset & QCOW_FLAGS_MASK) != 0)
        {
            if(!_clusterCache.TryGetValue(offset, out byte[] cluster))
            {
                if((offset & QCOW_COMPRESSED) == QCOW_COMPRESSED)
                {
                    ulong compSizeMask = (ulong)(1 << (int)(_qHdr.cluster_bits - 8)) - 1;
                    byte  countbits    = (byte)(_qHdr.cluster_bits - 8);
                    compSizeMask <<= 62 - countbits;
                    ulong offMask = ~compSizeMask & QCOW_FLAGS_MASK;

                    ulong realOff  = offset & offMask;
                    ulong compSize = (((offset & compSizeMask) >> (62 - countbits)) + 1) * 512;

                    byte[] zCluster = new byte[compSize];
                    _imageStream.Seek((long)realOff, SeekOrigin.Begin);
                    _imageStream.Read(zCluster, 0, (int)compSize);

                    var zStream = new DeflateStream(new MemoryStream(zCluster), CompressionMode.Decompress);
                    cluster = new byte[_clusterSize];
                    int read = zStream.Read(cluster, 0, _clusterSize);

                    if(read != _clusterSize)
                        return ErrorNumber.InOutError;
                }
                else
                {
                    cluster = new byte[_clusterSize];
                    _imageStream.Seek((long)(offset & QCOW_FLAGS_MASK), SeekOrigin.Begin);
                    _imageStream.Read(cluster, 0, _clusterSize);
                }

                if(_clusterCache.Count >= _maxClusterCache)
                    _clusterCache.Clear();

                _clusterCache.Add(offset, cluster);
            }

            Array.Copy(cluster, (int)(byteAddress & _sectorMask), buffer, 0, 512);
        }

        if(_sectorCache.Count >= MAX_CACHED_SECTORS)
            _sectorCache.Clear();

        _sectorCache.Add(sectorAddress, buffer);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }
}