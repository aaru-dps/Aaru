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
//     Writes Connectix and Microsoft Virtual PC disk images.
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
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using Version = System.Version;

namespace Aaru.Images;

public sealed partial class Vhd
{
#region IWritableImage Members

    /// <inheritdoc />
    /// TODO: Resume writing
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize)
    {
        if(options != null)
        {
            if(options.TryGetValue("block_size", out string tmpValue))
            {
                if(!uint.TryParse(tmpValue, out _blockSize))
                {
                    ErrorMessage = Localization.Invalid_block_size;

                    return false;
                }
            }
            else
                _blockSize = 2097152;

            if(options.TryGetValue("dynamic", out tmpValue))
            {
                if(!bool.TryParse(tmpValue, out _dynamic))
                {
                    ErrorMessage = Localization.Invalid_value_for_dynamic_option;

                    return false;
                }
            }
            else
                _dynamic = true;
        }
        else
        {
            _blockSize = 2097152;
            _dynamic   = true;
        }

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

        _imageInfo = new ImageInfo
        {
            MediaType  = mediaType,
            SectorSize = sectorSize,
            Sectors    = sectors
        };

        try
        {
            _writingStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        }
        catch(IOException ex)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, ex.Message);
            AaruConsole.WriteException(ex);

            return false;
        }

        IsWriting    = true;
        ErrorMessage = null;

        Version thisVersion = GetType().Assembly.GetName().Version ?? new Version();

        _thisFooter = new HardDiskFooter
        {
            Cookie = IMAGE_COOKIE,
            Features = FEATURES_RESERVED,
            Version = VERSION1,
            Timestamp = (uint)(DateTime.Now - new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
            CreatorApplication = CREATOR_AARU,
            CreatorVersion =
                (uint)(((thisVersion.Major & 0xFF) << 24) +
                       ((thisVersion.Minor & 0xFF) << 16) +
                       ((thisVersion.Build & 0xFF) << 8)  +
                       (thisVersion.Revision & 0xFF)),
            CreatorHostOs = DetectOS.GetRealPlatformID() == PlatformID.MacOSX ? CREATOR_MACINTOSH : CREATOR_WINDOWS,
            DiskType      = _dynamic ? TYPE_DYNAMIC : TYPE_FIXED,
            UniqueId      = Guid.NewGuid(),
            OriginalSize  = _imageInfo.Sectors * 512,
            CurrentSize   = _imageInfo.Sectors * 512
        };

        SetChsInFooter();

        if(!_dynamic) return true;

        ulong numberOfBlocks = _thisFooter.CurrentSize / _blockSize;

        if(_thisFooter.CurrentSize % _blockSize != 0) numberOfBlocks++;

        if(numberOfBlocks > uint.MaxValue)
        {
            ErrorMessage = Localization.Block_size_too_small_for_number_of_sectors;

            return false;
        }

        _thisDynamic = new DynamicDiskHeader
        {
            Cookie          = DYNAMIC_COOKIE,
            DataOffset      = 0xFFFFFFFF,
            TableOffset     = 512 + 1024,
            HeaderVersion   = VERSION1,
            MaxTableEntries = (uint)numberOfBlocks,
            BlockSize       = _blockSize
        };

        _blockAllocationTable = new uint[numberOfBlocks];
        for(uint i = 0; i < numberOfBlocks; i++) _blockAllocationTable[i] = 0xFFFFFFFF;

        _bitmapSize = (uint)Math.Ceiling((double)_thisDynamic.BlockSize /
                                         512

                                         // 1 bit per sector on the bitmap
                                        /
                                         8

                                         // and aligned to 512 byte boundary
                                        /
                                         512);

        _currentFooterPosition = (long)(512 + 1024 + sizeof(uint) * numberOfBlocks);
        if(_currentFooterPosition % 512 != 0) _currentFooterPosition += 512 - _currentFooterPosition % 512;

        // Write empty image
        Flush();

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

        if(data.Length != 512)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress >= _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        if(!_dynamic)
        {
            _writingStream.Seek((long)(0 + sectorAddress * 512), SeekOrigin.Begin);
            _writingStream.Write(data, 0, data.Length);

            ErrorMessage = string.Empty;

            return true;
        }

        // Block number for BAT searching
        var blockNumber = (uint)Math.Floor(sectorAddress / (_thisDynamic.BlockSize / 512.0));

        // If there's a cached block and it's the one we're looking for, flush cached data (clears cache)
        if(_blockInCache && _cachedBlockNumber != blockNumber) Flush();

        // If there is no block in cache, load or create it
        if(!_blockInCache)
        {
            _cachedBlock = new byte[_thisDynamic.BlockSize + _bitmapSize * 512];

            // If the block is not allocated, allocate it
            if(_blockAllocationTable[blockNumber] == 0xFFFFFFFF)
            {
                // If there's no data in sector, bail out happily writing nothing
                if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

                _blockAllocationTable[blockNumber] =  Swapping.Swap((uint)_currentFooterPosition / 512);
                _cachedBlockPosition               =  _currentFooterPosition;
                _currentFooterPosition             += _thisDynamic.BlockSize + _bitmapSize * 512;
                _writingStream.Position            =  _cachedBlockPosition;
            }
            else
            {
                _cachedBlockPosition    = Swapping.Swap(_blockAllocationTable[blockNumber]) * 512;
                _writingStream.Position = _cachedBlockPosition;
                _writingStream.EnsureRead(_cachedBlock, 0, _cachedBlock.Length);
            }

            _blockInCache      = true;
            _cachedBlockNumber = blockNumber;
        }

        // Sector number inside of block
        var  sectorInBlock = (uint)(sectorAddress % (_thisDynamic.BlockSize / 512));
        var  bitmapByte    = (int)Math.Floor((double)sectorInBlock          / 8);
        var  bitmapBit     = (int)(sectorInBlock % 8);
        var  mask          = (byte)(1 << 7 - bitmapBit);
        bool dirty         = (_cachedBlock[bitmapByte] & mask) == mask;

        // If there's no data in sector...
        if(ArrayHelpers.ArrayIsNullOrEmpty(data))
        {
            // ...but there's in the image
            if(!dirty) return true;

            // Clear bitmap
            _cachedBlock[bitmapByte] &= (byte)~mask;

            // A for loop allows the compiler to optimize to SIMD automatically
            for(long j = _bitmapSize * 512 + sectorInBlock * 512;
                j < _bitmapSize * 512 + sectorInBlock * 512 + 512;
                j++)
                _cachedBlock[j] = 0;

            Array.Copy(data, 0, _cachedBlock, (int)(_bitmapSize * 512 + sectorInBlock * 512), 512);
        }

        // Set bitmap bit and data
        else
        {
            _cachedBlock[bitmapByte] |= mask;
            Array.Copy(data, 0, _cachedBlock, (int)(_bitmapSize * 512 + sectorInBlock * 512), 512);
        }

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        if(_dynamic)
        {
            if(ArrayHelpers.ArrayIsNullOrEmpty(data))
            {
                for(var i = 0; i < length; i++)
                {
                    // Block number for BAT searching
                    var blockNumber = (uint)Math.Floor(sectorAddress / (_thisDynamic.BlockSize / 512.0));

                    // Block not allocated, bail out
                    if(_blockAllocationTable[blockNumber] == 0xFFFFFFFF) continue;

                    if(_blockInCache && _cachedBlockNumber != blockNumber)
                    {
                        Flush();

                        _cachedBlockPosition    = Swapping.Swap(_blockAllocationTable[blockNumber]) * 512;
                        _writingStream.Position = _cachedBlockPosition;
                        _writingStream.EnsureRead(_cachedBlock, 0, _cachedBlock.Length);
                        _blockInCache      = true;
                        _cachedBlockNumber = blockNumber;
                    }

                    // Sector number inside of block
                    var  sectorInBlock = (uint)(sectorAddress % (_thisDynamic.BlockSize / 512));
                    var  bitmapByte    = (int)Math.Floor((double)sectorInBlock          / 8);
                    var  bitmapBit     = (int)(sectorInBlock % 8);
                    var  mask          = (byte)(1 << 7 - bitmapBit);
                    bool dirty         = (_cachedBlock[bitmapByte] & mask) == mask;

                    if(!dirty) continue;

                    //Clear bitmap
                    _cachedBlock[bitmapByte] &= (byte)~mask;

                    // A for loop allows the compiler to optimize to SIMD automatically
                    for(long j = _bitmapSize * 512 + sectorInBlock * 512;
                        j < _bitmapSize * 512 + sectorInBlock * 512 + 512;
                        j++)
                        _cachedBlock[j] = 0;
                }

                return true;
            }

            for(var i = 0; i < length; i++)
            {
                if(!WriteSector(data[(i * 512)..(i * 512 + 512)], sectorAddress + (ulong)i)) return false;
            }

            return true;
        }

        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(data.Length % 512 != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress + length > _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        _writingStream.Seek((long)(0 + sectorAddress * 512), SeekOrigin.Begin);
        _writingStream.Write(data, 0, data.Length);

        ErrorMessage = string.Empty;

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

        Flush();

        _writingStream.Flush();
        _writingStream.Close();

        IsWriting    = false;
        ErrorMessage = string.Empty;

        return true;
    }

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo) => true;

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
    {
        if(cylinders > 0xFFFF)
        {
            ErrorMessage = Localization.Too_many_cylinders;

            return false;
        }

        if(heads > 0xFF)
        {
            ErrorMessage = Localization.Too_many_heads;

            return false;
        }

        if(sectorsPerTrack > 0xFF)
        {
            ErrorMessage = Localization.Too_many_sectors_per_track;

            return false;
        }

        _imageInfo.SectorsPerTrack = sectorsPerTrack;
        _imageInfo.Heads           = heads;
        _imageInfo.Cylinders       = cylinders;

        SetChsInFooter();

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata) => false;

#endregion

    void SetChsInFooter()
    {
        if(_imageInfo.Cylinders == 0)
        {
            ulong cylinderTimesHeads;

            if(_imageInfo.Sectors > 65535 * 16 * 255)
            {
                _imageInfo.Cylinders       = 65535;
                _imageInfo.Heads           = 16;
                _imageInfo.SectorsPerTrack = 255;
            }

            if(_imageInfo.Sectors >= 65535 * 16 * 63)
            {
                _imageInfo.Heads           = 16;
                _imageInfo.SectorsPerTrack = 255;
                cylinderTimesHeads         = _imageInfo.Sectors / _imageInfo.SectorsPerTrack;
            }
            else
            {
                _imageInfo.SectorsPerTrack = 17;
                cylinderTimesHeads         = _imageInfo.Sectors / _imageInfo.SectorsPerTrack;

                _imageInfo.Heads = (uint)((cylinderTimesHeads + 1023) / 1024);

                if(_imageInfo.Heads < 4) _imageInfo.Heads = 4;

                if(cylinderTimesHeads >= _imageInfo.Heads * 1024 || _imageInfo.Heads > 16)
                {
                    _imageInfo.SectorsPerTrack = 31;
                    _imageInfo.Heads           = 16;
                    cylinderTimesHeads         = _imageInfo.Sectors / _imageInfo.SectorsPerTrack;
                }

                if(cylinderTimesHeads >= _imageInfo.Heads * 1024)
                {
                    _imageInfo.SectorsPerTrack = 63;
                    _imageInfo.Heads           = 16;
                    cylinderTimesHeads         = _imageInfo.Sectors / _imageInfo.SectorsPerTrack;
                }
            }

            _imageInfo.Cylinders = (uint)(cylinderTimesHeads / _imageInfo.Heads);
        }

        _thisFooter.DiskGeometry = ((_imageInfo.Cylinders & 0xFFFF) << 16) +
                                   ((_imageInfo.Heads     & 0xFF)   << 8)  +
                                   (_imageInfo.SectorsPerTrack & 0xFF);
    }

    void Flush()
    {
        _thisFooter.Offset = _thisFooter.DiskType == TYPE_FIXED ? ulong.MaxValue : 512;

        var footerBytes = new byte[512];
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.Cookie),             0, footerBytes, 0x00, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.Features),           0, footerBytes, 0x08, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.Version),            0, footerBytes, 0x0C, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.Offset),             0, footerBytes, 0x10, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.Timestamp),          0, footerBytes, 0x18, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.CreatorApplication), 0, footerBytes, 0x1C, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.CreatorVersion),     0, footerBytes, 0x20, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.CreatorHostOs),      0, footerBytes, 0x24, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.OriginalSize),       0, footerBytes, 0x28, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.CurrentSize),        0, footerBytes, 0x30, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.DiskGeometry),       0, footerBytes, 0x38, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.DiskType),           0, footerBytes, 0x3C, 4);
        Array.Copy(_thisFooter.UniqueId.ToByteArray(),                             0, footerBytes, 0x44, 4);

        _thisFooter.Checksum = VhdChecksum(footerBytes);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisFooter.Checksum), 0, footerBytes, 0x40, 4);

        if(!_dynamic)
        {
            _writingStream.Seek((long)_thisFooter.OriginalSize, SeekOrigin.Begin);
            _writingStream.Write(footerBytes, 0, 512);

            _writingStream.Flush();

            return;
        }

        if(_blockInCache)
        {
            _writingStream.Position = _cachedBlockPosition;
            _writingStream.Write(_cachedBlock, 0, _cachedBlock.Length);
            _cachedBlock  = null;
            _blockInCache = false;
        }

        _writingStream.Position = (long)_thisDynamic.TableOffset;
        ReadOnlySpan<uint> span = _blockAllocationTable;

        byte[] bat = MemoryMarshal.Cast<uint, byte>(span)[..(int)(_thisDynamic.MaxTableEntries * sizeof(uint))]
                                  .ToArray();

        _writingStream.Write(bat, 0, bat.Length);

        var dynamicBytes = new byte[1024];
        Array.Copy(BigEndianBitConverter.GetBytes(_thisDynamic.Cookie),          0, dynamicBytes, 0x00, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisDynamic.DataOffset),      0, dynamicBytes, 0x08, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisDynamic.TableOffset),     0, dynamicBytes, 0x10, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisDynamic.HeaderVersion),   0, dynamicBytes, 0x18, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisDynamic.MaxTableEntries), 0, dynamicBytes, 0x1C, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisDynamic.BlockSize),       0, dynamicBytes, 0x20, 4);

        _thisDynamic.Checksum = VhdChecksum(dynamicBytes);
        Array.Copy(BigEndianBitConverter.GetBytes(_thisDynamic.Checksum), 0, dynamicBytes, 0x24, 4);

        _writingStream.Position = 0;
        _writingStream.Write(footerBytes, 0, 512);
        _writingStream.Position = 512;
        _writingStream.Write(dynamicBytes, 0, 1024);
        _writingStream.Position = _currentFooterPosition;
        _writingStream.Write(footerBytes, 0, 512);

        _writingStream.Flush();
    }
}