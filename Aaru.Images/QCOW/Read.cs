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
// Copyright © 2011-2023 Natalia Portillo
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

namespace Aaru.Images;

public sealed partial class Qcow
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512) return ErrorNumber.InvalidArgument;

        var qHdrB = new byte[48];
        stream.EnsureRead(qHdrB, 0, 48);
        _qHdr = Marshal.SpanToStructureBigEndian<Header>(qHdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.magic = 0x{0:X8}",          _qHdr.magic);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.version = {0}",             _qHdr.version);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.backing_file_offset = {0}", _qHdr.backing_file_offset);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.backing_file_size = {0}",   _qHdr.backing_file_size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.mtime = {0}",               _qHdr.mtime);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.size = {0}",                _qHdr.size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.cluster_bits = {0}",        _qHdr.cluster_bits);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l2_bits = {0}",             _qHdr.l2_bits);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.padding = {0}",             _qHdr.padding);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.crypt_method = {0}",        _qHdr.crypt_method);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l1_table_offset = {0}",     _qHdr.l1_table_offset);

        if(_qHdr.size <= 1)
        {
            AaruConsole.ErrorWriteLine(Localization.Image_size_is_too_small);

            return ErrorNumber.InvalidArgument;
        }

        if(_qHdr.cluster_bits is < 9 or > 16)
        {
            AaruConsole.ErrorWriteLine(Localization.Cluster_size_must_be_between_512_bytes_and_64_Kbytes);

            return ErrorNumber.InvalidArgument;
        }

        if(_qHdr.l2_bits is < 9 - 3 or > 16 - 3)
        {
            AaruConsole.ErrorWriteLine(Localization.L2_size_must_be_between_512_bytes_and_64_Kbytes);

            return ErrorNumber.InvalidArgument;
        }

        switch(_qHdr.crypt_method)
        {
            case > QCOW_ENCRYPTION_AES:
                AaruConsole.ErrorWriteLine(Localization.Invalid_encryption_method);

                return ErrorNumber.InvalidArgument;
            case > QCOW_ENCRYPTION_NONE:
                AaruConsole.ErrorWriteLine(Localization.AES_encrypted_images_not_yet_supported);

                return ErrorNumber.NotImplemented;
        }

        if(_qHdr.backing_file_offset != 0)
        {
            AaruConsole.ErrorWriteLine(Localization.Differencing_images_not_yet_supported);

            return ErrorNumber.NotImplemented;
        }

        int shift = _qHdr.cluster_bits + _qHdr.l2_bits;

        if(_qHdr.size > ulong.MaxValue - (ulong)(1 << shift))
        {
            AaruConsole.ErrorWriteLine(Localization.Image_is_too_large);

            return ErrorNumber.InvalidArgument;
        }

        _clusterSize    = 1 << _qHdr.cluster_bits;
        _clusterSectors = 1 << _qHdr.cluster_bits - 9;
        _l1Size         = (uint)(_qHdr.size + (ulong)(1 << shift) - 1 >> shift);
        _l2Size         = 1 << _qHdr.l2_bits;

        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.clusterSize = {0}",    _clusterSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.clusterSectors = {0}", _clusterSectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l1Size = {0}",         _l1Size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l2Size = {0}",         _l2Size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.sectors = {0}",        _imageInfo.Sectors);

        var l1TableB = new byte[_l1Size * 8];
        stream.Seek((long)_qHdr.l1_table_offset, SeekOrigin.Begin);
        stream.EnsureRead(l1TableB, 0, (int)_l1Size * 8);
        _l1Table = MemoryMarshal.Cast<byte, ulong>(l1TableB).ToArray();
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_L1_table);

        for(long i = 0; i < _l1Table.LongLength; i++) _l1Table[i] = Swapping.Swap(_l1Table[i]);

        _l1Mask = 0;
        var c = 0;
        _l1Shift = _qHdr.l2_bits + _qHdr.cluster_bits;

        for(var i = 0; i < 64; i++)
        {
            _l1Mask <<= 1;

            if(c >= 64 - _l1Shift) continue;

            _l1Mask += 1;
            c++;
        }

        _l2Mask = 0;

        for(var i = 0; i < _qHdr.l2_bits; i++) _l2Mask = (_l2Mask << 1) + 1;

        _l2Mask <<= _qHdr.cluster_bits;

        _sectorMask = 0;

        for(var i = 0; i < _qHdr.cluster_bits; i++) _sectorMask = (_sectorMask << 1) + 1;

        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l1Mask = {0:X}",     _l1Mask);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l1Shift = {0}",      _l1Shift);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l2Mask = {0:X}",     _l2Mask);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.sectorMask = {0:X}", _sectorMask);

        _maxL2TableCache = MAX_CACHE_SIZE / (_l2Size * 8);
        _maxClusterCache = MAX_CACHE_SIZE / _clusterSize;

        _imageStream = stream;

        _sectorCache  = new Dictionary<ulong, byte[]>();
        _l2TableCache = new Dictionary<ulong, ulong[]>();
        _clusterCache = new Dictionary<ulong, byte[]>();

        _imageInfo.CreationTime = imageFilter.CreationTime;

        _imageInfo.LastModificationTime = _qHdr.mtime > 0
                                              ? DateHandlers.UnixUnsignedToDateTime(_qHdr.mtime)
                                              : imageFilter.LastWriteTime;

        _imageInfo.MediaTitle        = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors           = _qHdr.size / 512;
        _imageInfo.SectorSize        = 512;
        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType         = MediaType.GENERIC_HDD;
        _imageInfo.ImageSize         = _qHdr.size;

        _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
        _imageInfo.Heads           = 16;
        _imageInfo.SectorsPerTrack = 63;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        // Check cache
        if(_sectorCache.TryGetValue(sectorAddress, out buffer)) return ErrorNumber.NoError;

        ulong byteAddress = sectorAddress * 512;

        ulong l1Off = (byteAddress & _l1Mask) >> _l1Shift;

        if((long)l1Off >= _l1Table.LongLength)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       string.Format(Localization.Trying_to_read_past_L1_table_position_0_of_a_max_1,
                                                     l1Off,
                                                     _l1Table.LongLength));

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
            _imageStream.Seek((long)_l1Table[l1Off], SeekOrigin.Begin);
            var l2TableB = new byte[_l2Size * 8];
            _imageStream.EnsureRead(l2TableB, 0, _l2Size * 8);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_L2_table_0, l1Off);
            l2Table = MemoryMarshal.Cast<byte, ulong>(l2TableB).ToArray();

            for(long i = 0; i < l2Table.LongLength; i++) l2Table[i] = Swapping.Swap(l2Table[i]);

            if(_l2TableCache.Count >= _maxL2TableCache) _l2TableCache.Clear();

            _l2TableCache.Add(l1Off, l2Table);
        }

        ulong l2Off = (byteAddress & _l2Mask) >> _qHdr.cluster_bits;

        ulong offset = l2Table[l2Off];

        buffer = new byte[512];

        if(offset != 0)
        {
            if(!_clusterCache.TryGetValue(offset, out byte[] cluster))
            {
                if((offset & QCOW_COMPRESSED) == QCOW_COMPRESSED)
                {
                    ulong compSizeMask = (ulong)(1 << _qHdr.cluster_bits) - 1;
                    compSizeMask <<= 63 - _qHdr.cluster_bits;
                    ulong offMask = ~compSizeMask ^ QCOW_COMPRESSED;

                    ulong realOff  = offset & offMask;
                    ulong compSize = (offset & compSizeMask) >> 63 - _qHdr.cluster_bits;

                    var zCluster = new byte[compSize];
                    _imageStream.Seek((long)realOff, SeekOrigin.Begin);
                    _imageStream.EnsureRead(zCluster, 0, (int)compSize);

                    var zStream = new DeflateStream(new MemoryStream(zCluster), CompressionMode.Decompress);
                    cluster = new byte[_clusterSize];
                    int read = zStream.EnsureRead(cluster, 0, _clusterSize);

                    if(read != _clusterSize) return ErrorNumber.InOutError;
                }
                else
                {
                    cluster = new byte[_clusterSize];
                    _imageStream.Seek((long)offset, SeekOrigin.Begin);
                    _imageStream.EnsureRead(cluster, 0, _clusterSize);
                }

                if(_clusterCache.Count >= _maxClusterCache) _clusterCache.Clear();

                _clusterCache.Add(offset, cluster);
            }

            Array.Copy(cluster, (int)(byteAddress & _sectorMask), buffer, 0, 512);
        }

        if(_sectorCache.Count >= MAX_CACHED_SECTORS) _sectorCache.Clear();

        _sectorCache.Add(sectorAddress, buffer);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}