// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser4 filesystem plugin
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
/// <summary>Implements detection of the Reiser v4 filesystem</summary>
public sealed partial class Reiser4
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting Reiser4 volume");

        _imagePlugin = imagePlugin;
        _partition   = partition;
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");

        if(imagePlugin.Info.SectorSize < 512)
        {
            AaruLogging.Debug(MODULE_NAME, "Sector size too small: {0}", imagePlugin.Info.SectorSize);

            return ErrorNumber.InvalidArgument;
        }

        // Read and validate the master super block
        ErrorNumber errno = ReadSuperblock();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Master superblock read successfully");
        AaruLogging.Debug(MODULE_NAME, "Block size: {0}", _blockSize);

        // Read and validate the format40 super block
        errno = ReadFormat40Superblock();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading format40 superblock: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Format40 superblock read successfully");
        AaruLogging.Debug(MODULE_NAME, "Block count: {0}",    _format40Sb.block_count);
        AaruLogging.Debug(MODULE_NAME, "Free blocks: {0}",    _format40Sb.free_blocks);
        AaruLogging.Debug(MODULE_NAME, "Root block: {0}",     _format40Sb.root_block);
        AaruLogging.Debug(MODULE_NAME, "File count: {0}",     _format40Sb.file_count);
        AaruLogging.Debug(MODULE_NAME, "Tree height: {0}",    _format40Sb.tree_height);
        AaruLogging.Debug(MODULE_NAME, "Large keys: {0}",     _largeKeys);
        AaruLogging.Debug(MODULE_NAME, "Format version: {0}", _format40Sb.version);

        // Initialize block cache
        _blockCache     = [];
        _directoryCache = new Dictionary<ulong, Dictionary<string, LargeKey>>();
        _modeCache      = new Dictionary<ulong, ushort>();
        _statCache      = new Dictionary<ulong, Aaru.CommonTypes.Structs.FileEntryInfo>();
        _pathCache      = new Dictionary<string, LargeKey>();

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
            Type         = FS_TYPE,
            ClusterSize  = _blockSize,
            Clusters     = _format40Sb.block_count,
            FreeClusters = _format40Sb.free_blocks,
            Files        = _format40Sb.file_count,
            VolumeName   = StringHandlers.CToString(_masterSb.label, _encoding),
            VolumeSerial = _masterSb.uuid.ToString()
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

        _rootDirectoryCache?.Clear();
        _blockCache?.Clear();
        _directoryCache?.Clear();
        _modeCache?.Clear();
        _statCache?.Clear();
        _pathCache?.Clear();
        _rootDirectoryCache = null;
        _blockCache         = null;
        _directoryCache     = null;
        _modeCache          = null;
        _statCache          = null;
        _pathCache          = null;
        _mounted            = false;
        _imagePlugin        = null;
        _partition          = default(Partition);
        _encoding           = null;
        _blockSize          = 0;
        Metadata            = null;

        AaruLogging.Debug(MODULE_NAME, "Volume unmounted");

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and validates the reiser4 master super block</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadSuperblock()
    {
        uint sbAddr = REISER4_SUPER_OFFSET / _imagePlugin.Info.SectorSize;

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

        _masterSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        if(!_magic.SequenceEqual(_masterSb.magic))
        {
            AaruLogging.Debug(MODULE_NAME, "No valid Reiser4 magic found");

            return ErrorNumber.InvalidArgument;
        }

        _blockSize = _masterSb.blocksize;

        if(_blockSize is < 512 or > 65536)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid block size: {0}", _blockSize);

            return ErrorNumber.InvalidArgument;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and validates the format40 disk super block</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadFormat40Superblock()
    {
        // Format40 super block is at the next block after the master super block.
        // FORMAT40_OFFSET = REISER4_MASTER_OFFSET + PAGE_SIZE
        // The super block number = FORMAT40_OFFSET / blocksize
        ulong format40Block = (REISER4_SUPER_OFFSET + _blockSize) / _blockSize;

        ErrorNumber errno = ReadBlock(format40Block, out byte[] blockData);

        if(errno != ErrorNumber.NoError) return errno;

        if(blockData.Length < Marshal.SizeOf<Format40DiskSuperblock>()) return ErrorNumber.InvalidArgument;

        _format40Sb = Marshal.ByteArrayToStructureLittleEndian<Format40DiskSuperblock>(blockData);

        // Validate the format40 magic
        string f40Magic = StringHandlers.CToString(_format40Sb.magic, Encoding.ASCII);

        if(!f40Magic.StartsWith(FORMAT40_MAGIC, StringComparison.Ordinal))
        {
            AaruLogging.Debug(MODULE_NAME, "No valid format40 magic found: '{0}'", f40Magic);

            return ErrorNumber.InvalidArgument;
        }

        // Determine key size from format flags
        _largeKeys = (_format40Sb.flags & FORMAT40_LARGE_KEYS) != 0;

        // Set key and item header sizes
        _keySize        = _largeKeys ? 32 : 24;
        _itemHeaderSize = _keySize + 6; // key + offset(2) + flags(2) + plugin_id(2)

        return ErrorNumber.NoError;
    }

    /// <summary>Loads the root directory and caches its contents</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory...");

        _rootDirectoryCache = new Dictionary<string, LargeKey>(StringComparer.Ordinal);

        // First verify the root directory exists by searching for its stat-data
        LargeKey rootSdKey = BuildRootStatDataKey();

        ErrorNumber errno = SearchByKey(rootSdKey, out byte[] leafData, out int itemPos);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Root directory stat data not found: {0}", errno);

            return errno;
        }

        if(itemPos < 0) return ErrorNumber.NoSuchFile;

        // Read the item header to verify it's a stat-data item
        Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(leafData);

        ReadItemHeader(leafData, itemPos, nh.nr_items, out _, out ushort bodyOff, out _, out ushort pluginId);

        if(pluginId != STATIC_STAT_DATA_ID)
        {
            AaruLogging.Debug(MODULE_NAME, "Root item is not stat-data (plugin={0})", pluginId);

            return ErrorNumber.InvalidArgument;
        }

        // Verify it's a directory by reading the stat-data extension mask and mode
        int sdLen = GetItemLength(leafData, itemPos, nh.nr_items, nh.free_space_start);

        if(sdLen < Marshal.SizeOf<StatDataBase>()) return ErrorNumber.InvalidArgument;

        StatDataBase sdBase =
            Marshal.ByteArrayToStructureLittleEndian<StatDataBase>(leafData, bodyOff, Marshal.SizeOf<StatDataBase>());

        // Parse stat-data extensions to find the mode
        int    sdOff = bodyOff + Marshal.SizeOf<StatDataBase>();
        ushort mode  = 0;

        if((sdBase.extmask & SD_LIGHT_WEIGHT) != 0)
        {
            if(sdOff + Marshal.SizeOf<LightWeightStat>() <= leafData.Length)
            {
                LightWeightStat lws =
                    Marshal.ByteArrayToStructureLittleEndian<LightWeightStat>(leafData,
                                                                              sdOff,
                                                                              Marshal.SizeOf<LightWeightStat>());

                mode = lws.mode;
            }
        }

        if(mode != 0 && (mode & S_IFMT) != S_IFDIR)
        {
            AaruLogging.Debug(MODULE_NAME, "Root inode is not a directory (mode=0x{0:X4})", mode);

            return ErrorNumber.InvalidArgument;
        }

        // Now read directory entries for the root directory.
        // Root directory entries have locality = FORMAT40_ROOT_OBJECTID
        errno = ReadDirectoryEntries(FORMAT40_ROOT_OBJECTID, out Dictionary<string, LargeKey> dirEntries);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root directory entries: {0}", errno);

            return errno;
        }

        // Cache entries (skip . and ..)
        foreach(KeyValuePair<string, LargeKey> entry in dirEntries)
        {
            if(entry.Key is "." or "..") continue;

            _rootDirectoryCache[entry.Key] = entry.Value;
        }

        AaruLogging.Debug(MODULE_NAME, "Cached {0} root directory entries", _rootDirectoryCache.Count);

        return ErrorNumber.NoError;
    }
}