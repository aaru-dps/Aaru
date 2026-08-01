// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : IBM JFS filesystem plugin
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
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of IBM's Journaled File System</summary>
public sealed partial class JFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting JFS volume");

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
        AaruLogging.Debug(MODULE_NAME, "Block size: {0} bytes",      _superblock.s_bsize);
        AaruLogging.Debug(MODULE_NAME, "Aggregate size: {0} blocks", _superblock.s_size);

        // Load root directory
        errno = LoadRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading root directory: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Root directory loaded with {0} entries", _rootDirectoryCache.Count);

        // Build metadata
        string volumeName = _superblock.s_version == 1
                                ? StringHandlers.CToString(_superblock.s_fpack, _encoding)
                                : StringHandlers.CToString(_superblock.s_label, _encoding);

        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            VolumeName   = volumeName,
            ClusterSize  = _superblock.s_bsize,
            Clusters     = _superblock.s_size,
            Dirty        = _superblock.s_state != 0,
            VolumeSerial = $"{_superblock.s_uuid}",
            ModificationDate = DateHandlers.UnixUnsignedToDateTime(_superblock.s_time.tv_sec,
                                                                   _superblock.s_time.tv_nsec)
        };

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "Volume mounted successfully");

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Unmounting JFS filesystem...");

        _rootDirectoryCache.Clear();
        _directoryCache.Clear();
        _inodeCache.Clear();
        _pathCache.Clear();
        _superblock  = default(SuperBlock);
        _fsInode     = default(Inode);
        _l2nbperpage = 0;
        _imagePlugin = null;
        _encoding    = null;
        _mounted     = false;
        Metadata     = null;

        AaruLogging.Debug(MODULE_NAME, "JFS filesystem unmounted successfully");

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and validates the JFS superblock</summary>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber ReadSuperblock()
    {
        AaruLogging.Debug(MODULE_NAME, "Reading superblock...");

        uint sectorSize = _imagePlugin.Info.SectorSize;

        // Superblock is at byte offset 0x8000 (JFS_BOOT_BLOCKS_SIZE)
        var superblockByteOffset = (long)JFS_BOOT_BLOCKS_SIZE;
        var sectorOffset         = (ulong)(superblockByteOffset / sectorSize);
        var offsetInSector       = (int)(superblockByteOffset   % sectorSize);

        int sbStructSize  = Marshal.SizeOf<SuperBlock>();
        var sectorsNeeded = (uint)((offsetInSector + sbStructSize + sectorSize - 1) / sectorSize);

        ErrorNumber errno = _imagePlugin.ReadSectors(_partition.Start + sectorOffset,
                                                     false,
                                                     sectorsNeeded,
                                                     out byte[] sbSector,
                                                     out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock sector: {0}", errno);

            return errno;
        }

        if(offsetInSector + sbStructSize > sbSector.Length)
        {
            AaruLogging.Debug(MODULE_NAME, "Sector too small for superblock");

            return ErrorNumber.InvalidArgument;
        }

        var sbData = new byte[sbStructSize];
        Array.Copy(sbSector, offsetInSector, sbData, 0, sbStructSize);

        _superblock = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sbData);

        AaruLogging.Debug(MODULE_NAME, "Magic: 0x{0:X8}", _superblock.s_magic);

        if(_superblock.s_magic != JFS_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid magic: 0x{0:X8}, expected 0x{1:X8}",
                              _superblock.s_magic,
                              JFS_MAGIC);

            return ErrorNumber.InvalidArgument;
        }

        if(_superblock.s_version is not (1 or 2))
        {
            AaruLogging.Debug(MODULE_NAME, "Unknown JFS version: {0}", _superblock.s_version);

            return ErrorNumber.InvalidArgument;
        }

        if(_superblock.s_bsize is 0 or > 4096)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid block size: {0}", _superblock.s_bsize);

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "JFS version: {0}", _superblock.s_version);
        AaruLogging.Debug(MODULE_NAME, "Block size: {0}",  _superblock.s_bsize);
        AaruLogging.Debug(MODULE_NAME, "AG size: {0}",     _superblock.s_agsize);
        AaruLogging.Debug(MODULE_NAME, "Flags: {0}",       _superblock.s_flags);
        AaruLogging.Debug(MODULE_NAME, "State: {0}",       _superblock.s_state);

        return ErrorNumber.NoError;
    }

    /// <summary>Loads the root directory and caches its contents</summary>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory...");

        _rootDirectoryCache.Clear();
        _directoryCache.Clear();
        _inodeCache.Clear();
        _pathCache.Clear();

        // Step 1: Read the FILESYSTEM_I inode (inode 16) from the fixed aggregate inode table
        ErrorNumber errno = ReadAggregateInode(FILESYSTEM_I, out _fsInode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading FILESYSTEM_I inode: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "FILESYSTEM_I inode read, di_number={0}, di_nlink={1}",
                          _fsInode.di_number,
                          _fsInode.di_nlink);

        _l2nbperpage = L2PSIZE - _superblock.s_l2bsize;

        AaruLogging.Debug(MODULE_NAME, "l2nbperpage={0}", _l2nbperpage);

        // Step 2: Read the root directory inode (inode 2 in the fileset)
        errno = ReadFilesetInode(ROOT_I, out Inode rootInode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root inode: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "Root inode: di_number={0}, di_mode=0x{1:X8}, di_size={2}, di_nlink={3}",
                          rootInode.di_number,
                          rootInode.di_mode,
                          rootInode.di_size,
                          rootInode.di_nlink);

        // Validate root inode is a directory (S_IFDIR = 0x4000 in POSIX mode bits)
        if((rootInode.di_mode & 0xF000) != 0x4000)
        {
            AaruLogging.Debug(MODULE_NAME, "Root inode is not a directory (mode=0x{0:X8})", rootInode.di_mode);

            return ErrorNumber.InvalidArgument;
        }

        // Step 3: Parse the dtree root in the root inode's extension area
        errno = ParseDtreeRoot(rootInode.di_u, out Dictionary<string, uint> entries);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error parsing root directory dtree: {0}", errno);

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