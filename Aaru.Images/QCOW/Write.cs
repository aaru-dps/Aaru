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
//     Writes QEMU Copy-On-Write disk images.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Images;

public sealed partial class Qcow
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize)
    {
        if(sectorSize != 512)
        {
            ErrorMessage = Localization.Unsupported_sector_size;

            return false;
        }

        if(!SupportedMediaTypes.Contains(mediaType))
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

            return false;
        }

        // TODO: Correct this calculation
        if(sectors * sectorSize / 65536 > uint.MaxValue)
        {
            ErrorMessage = Localization.Too_many_sectors_for_selected_cluster_size;

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
        catch(IOException ex)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, ex.Message);
            AaruConsole.WriteException(ex);

            return false;
        }

        _qHdr = new Header
        {
            magic           = QCOW_MAGIC,
            version         = QCOW_VERSION,
            size            = sectors * sectorSize,
            cluster_bits    = 12,
            l2_bits         = 9,
            l1_table_offset = (ulong)(Marshal.SizeOf<Header>() + 7 & ~7)
        };

        int shift = _qHdr.cluster_bits + _qHdr.l2_bits;
        _clusterSize    = 1 << _qHdr.cluster_bits;
        _clusterSectors = 1 << _qHdr.cluster_bits - 9;
        _l1Size         = (uint)(_qHdr.size + (ulong)(1 << shift) - 1 >> shift);
        _l2Size         = 1 << _qHdr.l2_bits;

        _l1Table = new ulong[_l1Size];

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

        var empty = new byte[_qHdr.l1_table_offset + _l1Size * 8];
        _writingStream.Write(empty, 0, empty.Length);

        IsWriting    = true;
        ErrorMessage = null;

        return true;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        ErrorMessage = Localization.Writing_media_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(data.Length != _imageInfo.SectorSize)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress >= _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        // Ignore empty sectors
        if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

        ulong byteAddress = sectorAddress * 512;

        ulong l1Off = (byteAddress & _l1Mask) >> _l1Shift;

        if((long)l1Off >= _l1Table.LongLength)
        {
            ErrorMessage = string.Format(Localization.Trying_to_write_past_L1_table_position_0_of_a_max_1,
                                         l1Off,
                                         _l1Table.LongLength);

            return false;
        }

        if(_l1Table[l1Off] == 0)
        {
            _writingStream.Seek(0, SeekOrigin.End);
            _l1Table[l1Off] = (ulong)((_writingStream.Length + _clusterSize - 1) / _clusterSize * _clusterSize);
            var l2TableB = new byte[_l2Size                                      * 8];
            _writingStream.Position = (long)_l1Table[l1Off];
            _writingStream.Write(l2TableB, 0, l2TableB.Length);
        }

        _writingStream.Position = (long)_l1Table[l1Off];

        ulong l2Off = (byteAddress & _l2Mask) >> _qHdr.cluster_bits;

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

        ErrorMessage = "";

        return true;
    }

    // TODO: This can be optimized
    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(data.Length % _imageInfo.SectorSize != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress + length > _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        // Ignore empty sectors
        if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

        for(uint i = 0; i < length; i++)
        {
            var tmp = new byte[_imageInfo.SectorSize];
            Array.Copy(data, i * _imageInfo.SectorSize, tmp, 0, _imageInfo.SectorSize);

            if(!WriteSector(tmp, sectorAddress + i)) return false;
        }

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        _qHdr.mtime = (uint)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

        _writingStream.Seek(0, SeekOrigin.Begin);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.magic),               0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.version),             0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.backing_file_offset), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.backing_file_size),   0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.mtime),               0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.size),                0, 8);
        _writingStream.WriteByte(_qHdr.cluster_bits);
        _writingStream.WriteByte(_qHdr.l2_bits);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.padding),         0, 2);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.crypt_method),    0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_qHdr.l1_table_offset), 0, 8);

        _writingStream.Seek((long)_qHdr.l1_table_offset, SeekOrigin.Begin);

        for(long i = 0; i < _l1Table.LongLength; i++) _l1Table[i] = Swapping.Swap(_l1Table[i]);

        byte[] l1TableB = MemoryMarshal.Cast<ulong, byte>(_l1Table).ToArray();
        _writingStream.Write(l1TableB, 0, l1TableB.Length);

        _writingStream.Flush();
        _writingStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo) => true;

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata) => false;

#endregion
}