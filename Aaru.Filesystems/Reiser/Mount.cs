// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser filesystem plugin
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

/// <inheritdoc />
/// <summary>Implements the Reiser v3 filesystem</summary>
public sealed partial class Reiser
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting ReiserFS volume");

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
        AaruLogging.Debug(MODULE_NAME, "Block size: {0}",    _blockSize);
        AaruLogging.Debug(MODULE_NAME, "Block count: {0}",   _superblock.block_count);
        AaruLogging.Debug(MODULE_NAME, "Root block: {0}",    _superblock.root_block);
        AaruLogging.Debug(MODULE_NAME, "Tree height: {0}",   _superblock.tree_height);
        AaruLogging.Debug(MODULE_NAME, "Key version: {0}",   _keyVersion == KEY_FORMAT_3_6 ? "3.6" : "3.5");
        AaruLogging.Debug(MODULE_NAME, "Hash function: {0}", _superblock.hash_function_code);

        // Initialize block cache
        _blockCache = [];

        _directoryCache =
            new Dictionary<(uint dirId, uint objectId), Dictionary<string, (uint dirId, uint objectId)>>();

        _modeCache = new Dictionary<(uint dirId, uint objectId), ushort>();
        _statCache = new Dictionary<(uint dirId, uint objectId), Aaru.CommonTypes.Structs.FileEntryInfo>();
        _pathCache = new Dictionary<string, (uint dirId, uint objectId)>();

        // Load root directory
        errno = LoadRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading root directory: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Root directory loaded with {0} entries", _rootDirectoryCache.Count);

        // Try to locate the xattr root: /.reiserfs_priv/xattrs
        // Only v3.6 format supports xattrs
        _xattrRootEntries = null;

        if(_keyVersion == KEY_FORMAT_3_6) LoadXattrRoot();

        // Build metadata
        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            ClusterSize  = _superblock.blocksize,
            Clusters     = _superblock.block_count,
            FreeClusters = _superblock.free_blocks,
            Dirty        = _superblock.umount_state == REISERFS_ERROR_FS
        };

        if(_superblock.version >= REISERFS_VERSION_2)
        {
            Metadata.VolumeName   = StringHandlers.CToString(_superblock.label, _encoding);
            Metadata.VolumeSerial = _superblock.uuid.ToString();
        }

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "Volume mounted successfully");

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Unmounting volume");

        _rootDirectoryCache?.Clear();
        _blockCache?.Clear();
        _directoryCache?.Clear();
        _modeCache?.Clear();
        _statCache?.Clear();
        _pathCache?.Clear();
        _xattrRootEntries?.Clear();
        _rootDirectoryCache = null;
        _blockCache         = null;
        _directoryCache     = null;
        _modeCache          = null;
        _statCache          = null;
        _pathCache          = null;
        _xattrRootEntries   = null;
        _mounted            = false;
        _imagePlugin        = null;
        _partition          = default(Partition);
        _encoding           = null;
        _blockSize          = 0;
        Metadata            = null;

        AaruLogging.Debug(MODULE_NAME, "Volume unmounted");

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and validates the ReiserFS superblock</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadSuperblock()
    {
        uint sbAddr = REISER_SUPER_OFFSET / _imagePlugin.Info.SectorSize;

        if(sbAddr == 0) sbAddr = 1;

        var sbSize = (uint)(Marshal.SizeOf<Superblock>() / _imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<Superblock>() % _imagePlugin.Info.SectorSize != 0) sbSize++;

        if(_partition.Start + sbAddr + sbSize >= _partition.End)
        {
            AaruLogging.Debug(MODULE_NAME, "Partition too small for superblock");

            return ErrorNumber.InvalidArgument;
        }

        ErrorNumber errno =
            _imagePlugin.ReadSectors(_partition.Start + sbAddr, false, sbSize, out byte[] sector, out _);

        if(errno != ErrorNumber.NoError) return errno;

        if(sector.Length < Marshal.SizeOf<Superblock>()) return ErrorNumber.InvalidArgument;

        _superblock = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        if(!_magic35.SequenceEqual(_superblock.magic) &&
           !_magic36.SequenceEqual(_superblock.magic) &&
           !_magicJr.SequenceEqual(_superblock.magic))
        {
            AaruLogging.Debug(MODULE_NAME, "No valid ReiserFS magic found");

            return ErrorNumber.InvalidArgument;
        }

        _blockSize = _superblock.blocksize;

        if(_blockSize is < 512 or > 65536)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid block size: {0}", _blockSize);

            return ErrorNumber.InvalidArgument;
        }

        // Determine key version from magic
        _keyVersion = _magic35.SequenceEqual(_superblock.magic) ? KEY_FORMAT_3_5 : KEY_FORMAT_3_6;

        return ErrorNumber.NoError;
    }

    /// <summary>Loads the root directory and caches its contents</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory...");

        _rootDirectoryCache = new Dictionary<string, (uint dirId, uint objectId)>();

        // The root directory's stat data has key (1, 2, 0, TYPE_STAT_DATA)
        // First, verify the root directory exists by reading its stat data
        ErrorNumber errno = SearchByKey(REISERFS_ROOT_PARENT_OBJECTID,
                                        REISERFS_ROOT_OBJECTID,
                                        0,
                                        TYPE_STAT_DATA,
                                        out byte[] statLeaf,
                                        out int statIndex);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Root directory stat data not found: {0}", errno);

            return errno;
        }

        if(statIndex < 0) return ErrorNumber.NoSuchFile;

        // Read stat data to verify it's a directory
        ItemHead statIh  = ReadItemHead(statLeaf, BLKH_SIZE + statIndex * IH_SIZE);
        int      statVer = GetItemKeyVersion(statIh.ih_version);

        if(statIh.ih_item_location + statIh.ih_item_len > statLeaf.Length) return ErrorNumber.InvalidArgument;

        ushort mode;

        if(statVer == KEY_FORMAT_3_6 && statIh.ih_item_len >= Marshal.SizeOf<StatDataV2>())
        {
            StatDataV2 sd =
                Marshal.ByteArrayToStructureLittleEndian<StatDataV2>(statLeaf,
                                                                     statIh.ih_item_location,
                                                                     Marshal.SizeOf<StatDataV2>());

            mode = sd.sd_mode;
        }
        else
        {
            StatDataV1 sd =
                Marshal.ByteArrayToStructureLittleEndian<StatDataV1>(statLeaf,
                                                                     statIh.ih_item_location,
                                                                     Marshal.SizeOf<StatDataV1>());

            mode = sd.sd_mode;
        }

        if((mode & S_IFMT) != S_IFDIR)
        {
            AaruLogging.Debug(MODULE_NAME, "Root inode is not a directory (mode=0x{0:X4})", mode);

            return ErrorNumber.InvalidArgument;
        }

        // Now read all directory entry items for the root directory.
        // Directory entries for object (1, 2) have key (1, 2, offset, TYPE_DIRENTRY).
        // The first directory item starts at offset DOT_OFFSET (1).
        // There may be multiple directory items with increasing offsets.
        errno = ReadDirectoryEntries(REISERFS_ROOT_PARENT_OBJECTID,
                                     REISERFS_ROOT_OBJECTID,
                                     out Dictionary<string, (uint dirId, uint objectId)> entries);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root directory entries: {0}", errno);

            return errno;
        }

        // Cache entries (skip . and .. and the hidden .reiserfs_priv private root)
        foreach(KeyValuePair<string, (uint dirId, uint objectId)> entry in entries)
        {
            if(entry.Key is "." or ".." or PRIVROOT_NAME) continue;

            _rootDirectoryCache[entry.Key] = entry.Value;
        }

        AaruLogging.Debug(MODULE_NAME, "Cached {0} root directory entries", _rootDirectoryCache.Count);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Attempts to locate the xattr root directory at <c>/.reiserfs_priv/xattrs</c>
    ///     and caches its entries for later use. Failure is not fatal — the filesystem
    ///     simply won't report any extended attributes.
    /// </summary>
    void LoadXattrRoot()
    {
        // Look for .reiserfs_priv in the root directory (including hidden entries)
        ErrorNumber errno = ReadDirectoryEntries(REISERFS_ROOT_PARENT_OBJECTID,
                                                 REISERFS_ROOT_OBJECTID,
                                                 out Dictionary<string, (uint dirId, uint objectId)> rootAll);

        if(errno != ErrorNumber.NoError) return;

        if(!rootAll.TryGetValue(PRIVROOT_NAME, out (uint dirId, uint objectId) privRoot))
        {
            AaruLogging.Debug(MODULE_NAME, "No .reiserfs_priv directory found — xattrs not available");

            return;
        }

        // Read the privroot directory to find the xattrs subdirectory
        errno = ReadDirectoryEntries(privRoot.dirId,
                                     privRoot.objectId,
                                     out Dictionary<string, (uint dirId, uint objectId)> privEntries);

        if(errno != ErrorNumber.NoError) return;

        if(!privEntries.TryGetValue(XAROOT_NAME, out (uint dirId, uint objectId) xaRoot))
        {
            AaruLogging.Debug(MODULE_NAME, "No xattrs directory inside .reiserfs_priv — xattrs not available");

            return;
        }

        // Read the xattr root directory — each entry is named <OBJECTID_HEX>.<GENERATION_HEX>
        errno = ReadDirectoryEntries(xaRoot.dirId,
                                     xaRoot.objectId,
                                     out Dictionary<string, (uint dirId, uint objectId)> xaEntries);

        if(errno != ErrorNumber.NoError) return;

        _xattrRootEntries = new Dictionary<string, (uint dirId, uint objectId)>();

        foreach(KeyValuePair<string, (uint dirId, uint objectId)> entry in xaEntries)
        {
            if(entry.Key is "." or "..") continue;

            _xattrRootEntries[entry.Key] = entry.Value;
        }

        AaruLogging.Debug(MODULE_NAME, "Xattr root loaded with {0} per-object directories", _xattrRootEntries.Count);
    }
}