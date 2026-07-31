// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux extended filesystem 2, 3 and 4 plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;
using Partition = Aaru.CommonTypes.Partition;


namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />

// ReSharper disable once InconsistentNaming
public sealed partial class ext2FS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting volume");

        _imagePlugin = imagePlugin;
        _partition   = partition;
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");

        // Read and validate the superblock
        ErrorNumber errno = ReadSuperblock();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Superblock read successfully");
        AaruLogging.Debug(MODULE_NAME, "Block size: {0}",       _blockSize);
        AaruLogging.Debug(MODULE_NAME, "Inodes: {0}",           _superblock.inodes);
        AaruLogging.Debug(MODULE_NAME, "Blocks per group: {0}", _superblock.blocks_per_grp);
        AaruLogging.Debug(MODULE_NAME, "Inodes per group: {0}", _superblock.inodes_per_grp);
        AaruLogging.Debug(MODULE_NAME, "64-bit: {0}",           _is64Bit);
        AaruLogging.Debug(MODULE_NAME, "Inode size: {0}",       _inodeSize);
        AaruLogging.Debug(MODULE_NAME, "Descriptor size: {0}",  _descSize);

        // Check for unsupported incompatible features
        uint unsupportedIncompat = _superblock.ftr_incompat & ~EXT2_SUPPORTED_INCOMPAT;

        if(unsupportedIncompat != 0)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Filesystem has unsupported incompatible features: 0x{0:X8}",
                              unsupportedIncompat);

            return ErrorNumber.NotSupported;
        }

        // Read block group descriptors
        errno = ReadBlockGroupDescriptors();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading block group descriptors: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Read {0} block group descriptors", _blockGroupCount);

        // Load root directory
        errno = LoadRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading root directory: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Root directory loaded with {0} entries", _rootDirectoryCache.Count);

        // Determine filesystem type
        bool ext3 = (_superblock.ftr_compat   & EXT3_FEATURE_COMPAT_HAS_JOURNAL)   != 0 ||
                    (_superblock.ftr_incompat & EXT3_FEATURE_INCOMPAT_RECOVER)     != 0 ||
                    (_superblock.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) != 0;

        bool ext4 = (_superblock.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_HUGE_FILE)   != 0 ||
                    (_superblock.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_GDT_CSUM)    != 0 ||
                    (_superblock.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_DIR_NLINK)   != 0 ||
                    (_superblock.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE) != 0 ||
                    (_superblock.ftr_incompat  & EXT4_FEATURE_INCOMPAT_64BIT)        != 0 ||
                    (_superblock.ftr_incompat  & EXT4_FEATURE_INCOMPAT_MMP)          != 0 ||
                    (_superblock.ftr_incompat  & EXT4_FEATURE_INCOMPAT_FLEX_BG)      != 0 ||
                    (_superblock.ftr_incompat  & EXT4_FEATURE_INCOMPAT_EA_INODE)     != 0 ||
                    (_superblock.ftr_incompat  & EXT4_FEATURE_INCOMPAT_DIRDATA)      != 0;

        if(ext4) ext3 = false;

        string fsType = ext4
                            ? FS_TYPE_EXT4
                            : ext3
                                ? FS_TYPE_EXT3
                                : FS_TYPE_EXT2;

        ulong totalBlocks = _is64Bit ? (ulong)_superblock.blocks_hi << 32 | _superblock.blocks : _superblock.blocks;

        ulong freeBlocks = _is64Bit
                               ? (ulong)_superblock.free_blocks_hi << 32 | _superblock.free_blocks
                               : _superblock.free_blocks;

        Metadata = new FileSystem
        {
            Type         = fsType,
            ClusterSize  = _blockSize,
            Clusters     = totalBlocks,
            FreeClusters = freeBlocks,
            Dirty        = (_superblock.state & EXT2_ERROR_FS) == EXT2_ERROR_FS,
            VolumeName   = StringHandlers.CToString(_superblock.volume_name, _encoding),
            VolumeSerial = _superblock.uuid.ToString()
        };

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "Volume mounted successfully as {0}", fsType);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Unmounting volume");

        _rootDirectoryCache.Clear();
        _directoryCache.Clear();
        _blockGroupDescriptors = null;
        _mounted               = false;
        _imagePlugin           = null;
        _partition             = default(Partition);
        _superblock            = default(SuperBlock);
        _encoding              = null;
        _blockSize             = 0;
        _blockGroupCount       = 0;
        _inodeSize             = 0;
        _descSize              = 0;
        _is64Bit               = false;
        _hasFileType           = false;
        _hasCompression        = false;
        _isHurd                = false;
        _isMasix               = false;
        _isVisopsys            = false;
        Metadata               = null;

        AaruLogging.Debug(MODULE_NAME, "Volume unmounted successfully");

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and validates the ext2/3/4 superblock</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadSuperblock()
    {
        AaruLogging.Debug(MODULE_NAME, "Reading superblock...");

        int sbSizeInBytes   = Marshal.SizeOf<SuperBlock>();
        var sbSizeInSectors = (uint)(sbSizeInBytes / _imagePlugin.Info.SectorSize);

        if(sbSizeInBytes % _imagePlugin.Info.SectorSize > 0) sbSizeInSectors++;

        ulong sbSectorOff = SB_POS / _imagePlugin.Info.SectorSize;
        uint  sbOff       = SB_POS % _imagePlugin.Info.SectorSize;

        if(sbSectorOff + _partition.Start >= _partition.End)
        {
            AaruLogging.Debug(MODULE_NAME, "Partition too small for superblock");

            return ErrorNumber.InvalidArgument;
        }

        ErrorNumber errno = _imagePlugin.ReadSectors(sbSectorOff + _partition.Start,
                                                     false,
                                                     sbSizeInSectors,
                                                     out byte[] sbSector,
                                                     out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock sectors: {0}", errno);

            return errno;
        }

        if(sbOff + sbSizeInBytes > sbSector.Length)
        {
            AaruLogging.Debug(MODULE_NAME, "Superblock sector data too small");

            return ErrorNumber.InvalidArgument;
        }

        var sblock = new byte[sbSizeInBytes];
        Array.Copy(sbSector, sbOff, sblock, 0, sbSizeInBytes);
        _superblock = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sblock);

        AaruLogging.Debug(MODULE_NAME, "Validating superblock...");

        // Validate magic number
        if(_superblock.magic != EXT2_MAGIC && _superblock.magic != EXT2_MAGIC_OLD)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid magic: 0x{0:X4}, expected 0x{1:X4}", _superblock.magic, EXT2_MAGIC);

            return ErrorNumber.InvalidArgument;
        }

        // Validate and compute block size: actual = 1024 << s_log_block_size
        // s_log_block_size must be <= 6 (for max 64K blocks)
        if(_superblock.block_size > 6)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid log block size: {0}", _superblock.block_size);

            return ErrorNumber.InvalidArgument;
        }

        _blockSize = EXT4_MIN_BLOCK_SIZE << (int)_superblock.block_size;

        if(_blockSize < EXT4_MIN_BLOCK_SIZE || _blockSize > EXT4_MAX_BLOCK_SIZE)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid block size: {0}", _blockSize);

            return ErrorNumber.InvalidArgument;
        }

        // Validate blocks per group and inodes per group are non-zero
        if(_superblock.blocks_per_grp == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Blocks per group is zero");

            return ErrorNumber.InvalidArgument;
        }

        if(_superblock.inodes_per_grp == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Inodes per group is zero");

            return ErrorNumber.InvalidArgument;
        }

        // Determine inode size
        if(_superblock.revision >= EXT2_DYNAMIC_REV)
        {
            _inodeSize = _superblock.inode_size;

            if(_inodeSize < EXT2_GOOD_OLD_INODE_SIZE || _inodeSize > _blockSize || (_inodeSize & _inodeSize - 1) != 0)
            {
                AaruLogging.Debug(MODULE_NAME, "Invalid inode size: {0}", _inodeSize);

                return ErrorNumber.InvalidArgument;
            }
        }
        else
            _inodeSize = EXT2_GOOD_OLD_INODE_SIZE;

        // Check for 64-bit feature
        _is64Bit = (_superblock.ftr_incompat & EXT4_FEATURE_INCOMPAT_64BIT) != 0;

        // Determine descriptor size
        if(_is64Bit)
        {
            _descSize = _superblock.desc_grp_size;

            if(_descSize < 64) _descSize = 64;
        }
        else
            _descSize = EXT4_MIN_DESC_SIZE;

        // Check for filetype in directory entries
        _hasFileType    = (_superblock.ftr_incompat & EXT2_FEATURE_INCOMPAT_FILETYPE)    != 0;
        _hasCompression = (_superblock.ftr_incompat & EXT2_FEATURE_INCOMPAT_COMPRESSION) != 0;
        _isHurd         = _superblock.creator_os                                         == EXT2_OS_HURD;
        _isMasix        = _superblock.creator_os                                         == EXT2_OS_MASIX;
        _isVisopsys     = _superblock.creator_os                                         == EXT2_OS_VISOPSYS;

        // Compute block group count
        ulong totalBlocks = _is64Bit ? (ulong)_superblock.blocks_hi << 32 | _superblock.blocks : _superblock.blocks;

        _blockGroupCount = (uint)((totalBlocks - _superblock.first_block + _superblock.blocks_per_grp - 1) /
                                  _superblock.blocks_per_grp);

        if(_blockGroupCount == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Block group count is zero");

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "Superblock validation successful");

        return ErrorNumber.NoError;
    }

    /// <summary>Reads all block group descriptors from disk</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadBlockGroupDescriptors()
    {
        AaruLogging.Debug(MODULE_NAME, "Reading block group descriptors...");

        _blockGroupDescriptors = new BlockGroupDescriptor[_blockGroupCount];

        // Block group descriptors start at the block immediately after the superblock block.
        // For 1K blocks, superblock is at block 1, so descriptors start at block 2.
        // For larger blocks, superblock is within block 0, so descriptors start at block 1.
        ulong bgdBlock = _superblock.first_block + 1;

        // Total bytes needed for all descriptors
        uint totalDescBytes = _blockGroupCount * _descSize;

        // Read descriptor data
        ErrorNumber errno = ReadBytes(bgdBlock * _blockSize, totalDescBytes, out byte[] descData);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading block group descriptors: {0}", errno);

            return errno;
        }

        // Parse each descriptor
        int bgdStructSize = Marshal.SizeOf<BlockGroupDescriptor>();

        for(uint i = 0; i < _blockGroupCount; i++)
        {
            uint offset = i * _descSize;

            if(offset + _descSize > descData.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Block group descriptor {0} extends beyond data", i);

                return ErrorNumber.InvalidArgument;
            }

            // On non-64-bit filesystems _descSize (32) is smaller than the full struct,
            // so only marshal _descSize bytes — the hi fields will be zero-filled.
            int bytesToMarshal = Math.Min(bgdStructSize, _descSize);

            _blockGroupDescriptors[i] =
                Marshal.ByteArrayToStructureLittleEndian<BlockGroupDescriptor>(descData, (int)offset, bytesToMarshal);
        }

        AaruLogging.Debug(MODULE_NAME, "Block group descriptors read successfully");

        return ErrorNumber.NoError;
    }

    /// <summary>Loads the root directory and caches its contents</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory...");

        _rootDirectoryCache.Clear();
        _directoryCache.Clear();

        // Read root inode (inode 2)
        ErrorNumber errno = ReadInode(EXT2_ROOT_INO, out Inode rootInode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root inode: {0}", errno);

            return errno;
        }

        // Validate root inode is a directory (S_ISDIR check)
        if((rootInode.mode & S_IFMT) != S_IFDIR)
        {
            AaruLogging.Debug(MODULE_NAME, "Root inode is not a directory (mode=0x{0:X4})", rootInode.mode);

            return ErrorNumber.InvalidArgument;
        }

        // Validate root inode has data
        ulong rootSize = (ulong)rootInode.size_high << 32 | rootInode.size_lo;

        if(rootSize == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Root inode has zero size");

            return ErrorNumber.InvalidArgument;
        }

        // Inline data directories store data in inode body, so blocks can be zero
        bool rootInline = (rootInode.i_flags & EXT4_INLINE_DATA_FL) != 0;

        if(!rootInline)
        {
            ulong rootBlocks = _isHurd || _isMasix || _isVisopsys
                                   ? rootInode.blocks_lo
                                   : (ulong)rootInode.blocks_high << 32 | rootInode.blocks_lo;

            if(rootBlocks == 0)
            {
                AaruLogging.Debug(MODULE_NAME, "Root inode has zero blocks");

                return ErrorNumber.InvalidArgument;
            }
        }

        AaruLogging.Debug(MODULE_NAME, "Root inode size: {0} bytes", rootSize);

        // Read directory entries from the root inode's data blocks
        errno = ReadDirectoryEntries(rootInode, EXT2_ROOT_INO, rootSize, out Dictionary<string, uint> entries);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root directory entries: {0}", errno);

            return errno;
        }

        // Cache entries (skip . and ..)
        foreach(KeyValuePair<string, uint> entry in entries)
        {
            if(entry.Key is "." or "..") continue;

            _rootDirectoryCache[entry.Key] = entry.Value;
        }

        AaruLogging.Debug(MODULE_NAME, "Cached {0} root directory entries", _rootDirectoryCache.Count);

        return ErrorNumber.NoError;
    }
}