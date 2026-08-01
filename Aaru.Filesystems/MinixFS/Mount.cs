// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MINIX filesystem plugin.
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
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class MinixFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting Minix volume");

        _imagePlugin = imagePlugin;
        _partition   = partition;
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");

        if(imagePlugin.Info.SectorSize < 512)
        {
            AaruLogging.Debug(MODULE_NAME, "Sector size too small: {0}", imagePlugin.Info.SectorSize);

            return ErrorNumber.InvalidArgument;
        }

        // Read and validate the superblock
        ErrorNumber errno = ReadSuperblock();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Superblock read successfully");
        AaruLogging.Debug(MODULE_NAME, "Filesystem version: {0}", _version);
        AaruLogging.Debug(MODULE_NAME, "Block size: {0}",         _blockSize);
        AaruLogging.Debug(MODULE_NAME, "Filename length: {0}",    _filenameSize);
        AaruLogging.Debug(MODULE_NAME, "Little endian: {0}",      _littleEndian);

        // Calculate inodes per block
        int inodeSize = _version == FilesystemVersion.V1 ? V1_INODE_SIZE : V2_INODE_SIZE;
        _inodesPerBlock = _blockSize / inodeSize;

        AaruLogging.Debug(MODULE_NAME, "Inode size: {0}",       inodeSize);
        AaruLogging.Debug(MODULE_NAME, "Inodes per block: {0}", _inodesPerBlock);

        // Calculate first inode block
        // Disk layout: boot block (0), superblock (1), inode map, zone map, inodes, data
        _firstInodeBlock = START_BLOCK + _imapBlocks + _zmapBlocks;

        AaruLogging.Debug(MODULE_NAME, "Inode map blocks: {0}",  _imapBlocks);
        AaruLogging.Debug(MODULE_NAME, "Zone map blocks: {0}",   _zmapBlocks);
        AaruLogging.Debug(MODULE_NAME, "First inode block: {0}", _firstInodeBlock);

        // Load root directory
        errno = LoadRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading root directory: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Root directory loaded with {0} entries", _rootDirectoryCache.Count);

        // Build metadata
        Metadata = new FileSystem
        {
            Type = _version == FilesystemVersion.V3
                       ? FS_TYPE_V3
                       : _version == FilesystemVersion.V2
                           ? FS_TYPE_V2
                           : FS_TYPE_V1,
            ClusterSize = (uint)_blockSize,
            Clusters    = _zones,
            Dirty       = !_isClean
        };

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "Volume mounted successfully");

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Unmounting volume");

        _rootDirectoryCache.Clear();
        _inodeCache.Clear();
        _directoryCache.Clear();
        _indirectBlockCache.Clear();
        _mounted        = false;
        _imagePlugin    = null;
        _partition      = default(Partition);
        _encoding       = null;
        _blockSize      = 0;
        _inodesPerBlock = 0;
        Metadata        = null;

        AaruLogging.Debug(MODULE_NAME, "Volume unmounted");

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and validates the Minix superblock</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadSuperblock()
    {
        AaruLogging.Debug(MODULE_NAME, "Reading superblock...");

        uint sectorSize = _imagePlugin.Info.SectorSize;

        // Superblock is at byte offset 1024 (block 1)
        // Calculate sector and offset based on actual sector size
        uint sector = SUPER_BLOCK_BYTES / sectorSize;
        uint offset = SUPER_BLOCK_BYTES % sectorSize;

        if(sector + _partition.Start >= _partition.End)
        {
            AaruLogging.Debug(MODULE_NAME, "Partition too small");

            return ErrorNumber.InvalidArgument;
        }

        ErrorNumber errno = _imagePlugin.ReadSector(sector + _partition.Start, false, out byte[] sbSector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock sector: {0}", errno);

            return errno;
        }

        // Handle optical disc offset
        if(offset > 0)
        {
            var tmp = new byte[0x200];
            Array.Copy(sbSector, offset, tmp, 0, 0x200);
            sbSector = tmp;
        }

        // Try to detect filesystem version by checking magic at different offsets
        // V1/V2: magic at offset 0x10
        // V3: magic at offset 0x18
        var magicV1V2 = BitConverter.ToUInt16(sbSector, 0x10);
        var magicV3   = BitConverter.ToUInt16(sbSector, 0x18);

        AaruLogging.Debug(MODULE_NAME, "Magic at 0x10: 0x{0:X4}", magicV1V2);
        AaruLogging.Debug(MODULE_NAME, "Magic at 0x18: 0x{0:X4}", magicV3);

        // Check for V3 first (magic at 0x18)
        if(magicV3 is MINIX3_MAGIC or MINIX3_CIGAM or MINIX2_MAGIC or MINIX2_CIGAM or MINIX_MAGIC or MINIX_CIGAM)
        {
            _littleEndian = magicV3 is not (MINIX3_CIGAM or MINIX2_CIGAM or MINIX_CIGAM);

            SuperBlock3 sb3 = _littleEndian
                                  ? Marshal.ByteArrayToStructureLittleEndian<SuperBlock3>(sbSector)
                                  : Marshal.ByteArrayToStructureBigEndian<SuperBlock3>(sbSector);

            _version = magicV3 switch
                       {
                           MINIX3_MAGIC or MINIX3_CIGAM => FilesystemVersion.V3,
                           MINIX2_MAGIC or MINIX2_CIGAM => FilesystemVersion.V2,
                           _                            => FilesystemVersion.V1
                       };

            _filenameSize = V3_NAME_MAX;
            _blockSize    = magicV3 is MINIX3_MAGIC or MINIX3_CIGAM ? sb3.s_blocksize : V1_V2_BLOCK_SIZE;
            _ninodes      = sb3.s_ninodes;
            _imapBlocks   = sb3.s_imap_blocks;
            _zmapBlocks   = sb3.s_zmap_blocks;
            _logZoneSize  = sb3.s_log_zone_size;
            _maxSize      = sb3.s_max_size;
            _zones        = sb3.s_zones > 0 ? sb3.s_zones : sb3.s_nzones;
            _isClean      = (sb3.s_flags & (ushort)FilesystemStateFlags.Clean) != 0;

            // For V3 filesystems, s_firstdatazone may be 0 if the value was too large to fit
            // in the 16-bit field. In that case, compute it on the fly like Minix MFS does.
            if(sb3.s_firstdatazone == 0)
            {
                // Calculate inodes per block
                int inodesPerBlock = _blockSize / V2_INODE_SIZE;

                // offset = START_BLOCK + imap_blocks + zmap_blocks + inode_blocks
                int start_Block = START_BLOCK + _imapBlocks + _zmapBlocks;
                start_Block += (int)((_ninodes + inodesPerBlock - 1) / inodesPerBlock);

                // firstdatazone = (offset + (1 << log_zone_size) - 1) >> log_zone_size
                _firstDataZone = start_Block + (1 << _logZoneSize) - 1 >> _logZoneSize;
            }
            else
                _firstDataZone = sb3.s_firstdatazone;

            // Check for mandatory flags that we don't understand
            if((sb3.s_flags & (ushort)MandatoryFlags.Mask) != 0)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "Filesystem has mandatory flags (0x{0:X4}) that are not supported",
                                  sb3.s_flags & (ushort)MandatoryFlags.Mask);

                return ErrorNumber.NotSupported;
            }

            AaruLogging.Debug(MODULE_NAME, "Detected Minix V3 superblock format");
        }
        else if(magicV1V2 is MINIX_MAGIC
                          or MINIX_MAGIC2
                          or MINIX2_MAGIC
                          or MINIX2_MAGIC2
                          or MINIX_CIGAM
                          or MINIX_CIGAM2
                          or MINIX2_CIGAM
                          or MINIX2_CIGAM2)
        {
            _littleEndian = magicV1V2 is not (MINIX_CIGAM or MINIX_CIGAM2 or MINIX2_CIGAM or MINIX2_CIGAM2);

            SuperBlock sb = _littleEndian
                                ? Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sbSector)
                                : Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sbSector);

            _version = magicV1V2 switch
                       {
                           MINIX2_MAGIC or MINIX2_MAGIC2 or MINIX2_CIGAM or MINIX2_CIGAM2 => FilesystemVersion.V2,
                           _                                                              => FilesystemVersion.V1
                       };

            _filenameSize = magicV1V2 is MINIX_MAGIC2 or MINIX2_MAGIC2 or MINIX_CIGAM2 or MINIX2_CIGAM2
                                ? V1_NAME_MAX_LONG
                                : V1_NAME_MAX;

            _blockSize     = V1_V2_BLOCK_SIZE;
            _ninodes       = sb.s_ninodes;
            _imapBlocks    = sb.s_imap_blocks;
            _zmapBlocks    = sb.s_zmap_blocks;
            _firstDataZone = sb.s_firstdatazone;
            _logZoneSize   = sb.s_log_zone_size;
            _maxSize       = sb.s_max_size;

            // V1 filesystems only have s_nzones (16-bit), s_zones doesn't exist in the on-disk structure
            // V2 filesystems have both, with s_zones (32-bit) replacing s_nzones for larger volume support
            _zones = _version == FilesystemVersion.V1
                         ? sb.s_nzones
                         : sb.s_zones > 0
                             ? sb.s_zones
                             : sb.s_nzones;

            _isClean = (sb.s_state & (ushort)FilesystemStateFlags.Clean) != 0;

            AaruLogging.Debug(MODULE_NAME, "Detected Minix V1/V2 superblock format");
        }
        else
        {
            AaruLogging.Debug(MODULE_NAME, "No valid Minix magic found");

            return ErrorNumber.InvalidArgument;
        }


        return ErrorNumber.NoError;
    }

    /// <summary>Loads the root directory and caches its contents</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory...");

        _rootDirectoryCache.Clear();
        _inodeCache.Clear();
        _directoryCache.Clear();
        _indirectBlockCache.Clear();

        // Read the root inode (inode 1)
        ErrorNumber errno = ReadInode(ROOT_INODE, out object rootInodeObj);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root inode: {0}", errno);

            return errno;
        }

        // Validate that root inode is a directory
        ushort mode;
        uint   size;

        if(_version == FilesystemVersion.V1)
        {
            var rootInode = (V1DiskInode)rootInodeObj;
            mode                      = rootInode.d1_mode;
            size                      = rootInode.d1_size;
            _inodeCacheV1[ROOT_INODE] = rootInode;
        }
        else
        {
            var rootInode = (V2DiskInode)rootInodeObj;
            mode                    = rootInode.d2_mode;
            size                    = rootInode.d2_size;
            _inodeCache[ROOT_INODE] = rootInode;
        }

        if((mode & (ushort)InodeMode.TypeMask) != (ushort)InodeMode.Directory)
        {
            AaruLogging.Debug(MODULE_NAME, "Root inode is not a directory (mode=0x{0:X4})", mode);

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "Root inode size: {0} bytes", size);

        // Read directory contents
        errno = ReadDirectoryContents(ROOT_INODE, out Dictionary<string, uint> entries);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading directory contents: {0}", errno);

            return errno;
        }

        // Cache entries (skip . and ..)
        foreach(KeyValuePair<string, uint> entry in entries)
        {
            if(entry.Key is "." or "..") continue;

            _rootDirectoryCache[entry.Key] = entry.Value;

            AaruLogging.Debug(MODULE_NAME, "Cached entry: {0} -> inode {1}", entry.Key, entry.Value);
        }

        AaruLogging.Debug(MODULE_NAME, "Cached {0} root directory entries", _rootDirectoryCache.Count);


        return ErrorNumber.NoError;
    }
}