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
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Images;

public sealed partial class Qed
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512) return ErrorNumber.InvalidArgument;

        var qHdrB = new byte[68];
        stream.EnsureRead(qHdrB, 0, 68);
        _qHdr = Marshal.SpanToStructureLittleEndian<QedHeader>(qHdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.magic = 0x{0:X8}",          _qHdr.magic);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.cluster_size = {0}",        _qHdr.cluster_size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.table_size = {0}",          _qHdr.table_size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.header_size = {0}",         _qHdr.header_size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.features = {0}",            _qHdr.features);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.compat_features = {0}",     _qHdr.compat_features);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.autoclear_features = {0}",  _qHdr.autoclear_features);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l1_table_offset = {0}",     _qHdr.l1_table_offset);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.image_size = {0}",          _qHdr.image_size);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.backing_file_offset = {0}", _qHdr.backing_file_offset);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.backing_file_size = {0}",   _qHdr.backing_file_size);

        if(_qHdr.image_size <= 1)
        {
            AaruConsole.ErrorWriteLine(Localization.Image_size_is_too_small);

            return ErrorNumber.InvalidArgument;
        }

        if(!IsPowerOfTwo(_qHdr.cluster_size))
        {
            AaruConsole.ErrorWriteLine(Localization.Cluster_size_must_be_a_power_of_2);

            return ErrorNumber.InvalidArgument;
        }

        if(_qHdr.cluster_size is < 4096 or > 67108864)
        {
            AaruConsole.ErrorWriteLine(Localization.Cluster_size_must_be_between_4_Kbytes_and_64_Mbytes);

            return ErrorNumber.InvalidArgument;
        }

        if(!IsPowerOfTwo(_qHdr.table_size))
        {
            AaruConsole.ErrorWriteLine(Localization.Table_size_must_be_a_power_of_2);

            return ErrorNumber.InvalidArgument;
        }

        if(_qHdr.table_size is < 1 or > 16)
        {
            AaruConsole.ErrorWriteLine(Localization.Table_size_must_be_between_1_and_16_clusters);

            return ErrorNumber.InvalidArgument;
        }

        if((_qHdr.features & QED_FEATURE_MASK) > 0)
        {
            AaruConsole.ErrorWriteLine(string.Format(Localization.Image_uses_unknown_incompatible_features_0,
                                                     _qHdr.features & QED_FEATURE_MASK));

            return ErrorNumber.InvalidArgument;
        }

        if((_qHdr.features & QED_FEATURE_BACKING_FILE) == QED_FEATURE_BACKING_FILE)
        {
            AaruConsole.ErrorWriteLine(Localization.Differencing_images_not_yet_supported);

            return ErrorNumber.NotImplemented;
        }

        _clusterSectors = _qHdr.cluster_size                    / 512;
        _tableSize      = _qHdr.cluster_size * _qHdr.table_size / 8;

        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.clusterSectors = {0}", _clusterSectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.tableSize = {0}",      _tableSize);

        var l1TableB = new byte[_tableSize * 8];
        stream.Seek((long)_qHdr.l1_table_offset, SeekOrigin.Begin);
        stream.EnsureRead(l1TableB, 0, (int)_tableSize * 8);
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_L1_table);
        _l1Table = MemoryMarshal.Cast<byte, ulong>(l1TableB).ToArray();

        _l1Mask = 0;
        var c = 0;
        _clusterBits = Ctz32(_qHdr.cluster_size);
        _l2Mask      = _tableSize - 1 << _clusterBits;
        _l1Shift     = _clusterBits + Ctz32(_tableSize);

        for(var i = 0; i < 64; i++)
        {
            _l1Mask <<= 1;

            if(c >= 64 - _l1Shift) continue;

            _l1Mask += 1;
            c++;
        }

        _sectorMask = 0;

        for(var i = 0; i < _clusterBits; i++) _sectorMask = (_sectorMask << 1) + 1;

        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.clusterBits = {0}",  _clusterBits);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l1Mask = {0:X}",     _l1Mask);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l1Shift = {0}",      _l1Shift);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.l2Mask = {0:X}",     _l2Mask);
        AaruConsole.DebugWriteLine(MODULE_NAME, "qHdr.sectorMask = {0:X}", _sectorMask);

        _maxL2TableCache = MAX_CACHE_SIZE / _tableSize;
        _maxClusterCache = MAX_CACHE_SIZE / _qHdr.cluster_size;

        _imageStream = stream;

        _sectorCache  = new Dictionary<ulong, byte[]>();
        _l2TableCache = new Dictionary<ulong, ulong[]>();
        _clusterCache = new Dictionary<ulong, byte[]>();

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = _qHdr.image_size / 512;
        _imageInfo.SectorSize           = 512;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.GENERIC_HDD;
        _imageInfo.ImageSize            = _qHdr.image_size;

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
            var l2TableB = new byte[_tableSize * 8];
            _imageStream.EnsureRead(l2TableB, 0, (int)_tableSize * 8);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_L2_table_0, l1Off);
            l2Table = MemoryMarshal.Cast<byte, ulong>(l2TableB).ToArray();

            if(_l2TableCache.Count >= _maxL2TableCache) _l2TableCache.Clear();

            _l2TableCache.Add(l1Off, l2Table);
        }

        ulong l2Off = (byteAddress & _l2Mask) >> _clusterBits;

        ulong offset = l2Table[l2Off];

        buffer = new byte[512];

        if(offset != 0 && offset != 1)
        {
            if(!_clusterCache.TryGetValue(offset, out byte[] cluster))
            {
                cluster = new byte[_qHdr.cluster_size];
                _imageStream.Seek((long)offset, SeekOrigin.Begin);
                _imageStream.EnsureRead(cluster, 0, (int)_qHdr.cluster_size);

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