// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : B-tree file system plugin.
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
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class BTRFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting btrfs volume");

        _imagePlugin    = imagePlugin;
        _partition      = partition;
        _encoding       = encoding ?? Encoding.GetEncoding("iso-8859-15");
        _treeBlockCache = new Dictionary<ulong, byte[]>();
        _directoryCache = new Dictionary<(ulong dirObjectId, ulong treeRoot), Dictionary<string, DirEntry>>();

        // Step 1: Read and validate the superblock
        ErrorNumber errno = ReadSuperblock();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Superblock read successfully");
        AaruLogging.Debug(MODULE_NAME, "UUID: {0}",        _superblock.uuid);
        AaruLogging.Debug(MODULE_NAME, "Generation: {0}",  _superblock.generation);
        AaruLogging.Debug(MODULE_NAME, "Root tree: {0}",   _superblock.root_lba);
        AaruLogging.Debug(MODULE_NAME, "Chunk tree: {0}",  _superblock.chunk_lba);
        AaruLogging.Debug(MODULE_NAME, "Node size: {0}",   _superblock.nodesize);
        AaruLogging.Debug(MODULE_NAME, "Sector size: {0}", _superblock.sectorsize);
        AaruLogging.Debug(MODULE_NAME, "Label: {0}",       _superblock.label);

        // Step 2: Parse the system chunk array from the superblock to bootstrap logical→physical translation
        errno = ParseSystemChunkArray();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error parsing system chunk array: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Parsed {0} chunk mappings from system chunk array", _chunkMap.Count);

        // Step 3: Read the chunk tree to complete the chunk map
        errno = ReadChunkTree();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading chunk tree: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Chunk tree read, total {0} chunk mappings", _chunkMap.Count);

        // Step 4: Find the FS tree root by searching the root tree for the FS_TREE root item
        errno = FindFsTreeRoot();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error finding FS tree root: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "FS tree root at logical address {0}", _fsTreeRoot);

        // Step 5: Load the root directory from the FS tree
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
            ClusterSize  = _superblock.sectorsize,
            Clusters     = _superblock.total_bytes / _superblock.sectorsize,
            VolumeName   = _superblock.label,
            VolumeSerial = _superblock.uuid.ToString()
        };

        Metadata.FreeClusters = Metadata.Clusters - _superblock.bytes_used / _superblock.sectorsize;

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "btrfs volume mounted successfully");

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Unmounting btrfs volume");

        _rootDirectoryCache?.Clear();
        _directoryCache?.Clear();
        _chunkMap?.Clear();
        _treeBlockCache?.Clear();
        _rootDirectoryCache = null;
        _chunkMap           = null;
        _treeBlockCache     = null;
        _mounted            = false;
        _imagePlugin        = null;
        _partition          = default(Partition);
        _encoding           = null;
        _superblock         = default(SuperBlock);
        _fsTreeRoot         = 0;
        _fsTreeLevel        = 0;
        Metadata            = null;

        AaruLogging.Debug(MODULE_NAME, "btrfs volume unmounted");

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and validates the btrfs superblock from the image</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadSuperblock()
    {
        AaruLogging.Debug(MODULE_NAME, "Reading superblock...");

        ulong sbSectorOff  = 0x10000 / _imagePlugin.Info.SectorSize;
        uint  sbSectorSize = 0x1000  / _imagePlugin.Info.SectorSize;

        if(sbSectorOff + _partition.Start >= _partition.End)
        {
            AaruLogging.Debug(MODULE_NAME, "Partition too small for superblock");

            return ErrorNumber.InvalidArgument;
        }

        ErrorNumber errno =
            _imagePlugin.ReadSectors(sbSectorOff + _partition.Start, false, sbSectorSize, out byte[] sector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock sectors: {0}", errno);

            return errno;
        }

        _superblock = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

        if(_superblock.magic != BTRFS_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid magic: 0x{0:X16}, expected 0x{1:X16}",
                              _superblock.magic,
                              BTRFS_MAGIC);

            return ErrorNumber.InvalidArgument;
        }

        if(_superblock.nodesize is < 4096 or > 65536)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid node size: {0}", _superblock.nodesize);

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "Superblock validated successfully");

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Searches the root tree for the default subvolume by looking up the "default" DIR_ITEM under
    ///     ROOT_TREE_DIR_OBJECTID (6). If found, uses its location to find the corresponding ROOT_ITEM.
    ///     Falls back to FS_TREE_OBJECTID (5) if the default dir item is not present.
    /// </summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber FindFsTreeRoot()
    {
        AaruLogging.Debug(MODULE_NAME, "Searching root tree for default subvolume...");

        ErrorNumber errno = ReadTreeBlock(_superblock.root_lba, out byte[] rootTreeData);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root tree: {0}", errno);

            return errno;
        }

        Header rootHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(rootTreeData);

        AaruLogging.Debug(MODULE_NAME, "Root tree level: {0}, items: {1}", rootHeader.level, rootHeader.nritems);

        // Try to find the default subvolume by looking for a DIR_ITEM named "default" under objectid 6
        ulong defaultSubvolId = BTRFS_FS_TREE_OBJECTID;

        ErrorNumber dirErrno = FindDefaultSubvolume(rootTreeData, rootHeader, out ulong resolvedSubvolId);

        if(dirErrno == ErrorNumber.NoError && resolvedSubvolId != 0)
        {
            defaultSubvolId = resolvedSubvolId;

            AaruLogging.Debug(MODULE_NAME, "Default subvolume objectid: {0}", defaultSubvolId);
        }
        else
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Default subvolume DIR_ITEM not found, falling back to FS_TREE (objectid 5)");
        }

        // Now find the ROOT_ITEM for the resolved subvolume
        errno = SearchTreeForRootItem(rootTreeData, rootHeader, defaultSubvolId);

        if(errno != ErrorNumber.NoError)
        {
            // If the resolved default subvol wasn't found and it wasn't 5, try subvol 5 as last resort
            if(defaultSubvolId != BTRFS_FS_TREE_OBJECTID)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "ROOT_ITEM for default subvolume {0} not found, trying FS_TREE (5)",
                                  defaultSubvolId);

                errno = SearchTreeForRootItem(rootTreeData, rootHeader, BTRFS_FS_TREE_OBJECTID);

                if(errno == ErrorNumber.NoError) return ErrorNumber.NoError;
            }

            AaruLogging.Debug(MODULE_NAME, "FS tree root item not found in root tree");

            return ErrorNumber.NoSuchFile;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Searches the root tree for a DIR_ITEM named "default" under ROOT_TREE_DIR_OBJECTID (6)
    ///     and returns the objectid from its location key, which is the default subvolume id.
    /// </summary>
    /// <param name="nodeData">Raw root tree node data</param>
    /// <param name="header">Parsed root tree header</param>
    /// <param name="subvolId">The default subvolume objectid if found</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber FindDefaultSubvolume(byte[] nodeData, in Header header, out ulong subvolId)
    {
        subvolId = 0;
        int headerSize = Marshal.SizeOf<Header>();

        if(header.level == 0) return ExtractDefaultSubvolFromLeaf(nodeData, header, out subvolId);

        // Internal node — use binary search to find the child that may contain objectid 6
        int keyPtrSize = Marshal.SizeOf<KeyPtr>();

        var keyBytes = new byte[17];
        BitConverter.TryWriteBytes(keyBytes.AsSpan(0), BTRFS_ROOT_TREE_DIR_OBJECTID);
        keyBytes[8] = BTRFS_DIR_ITEM_KEY;

        // offset = 0 already from array init
        DiskKey targetKey = Marshal.ByteArrayToStructureLittleEndian<DiskKey>(keyBytes);

        int childIdx = BinarySearchNode(nodeData, header, targetKey);

        if(childIdx < 0) return ErrorNumber.NoSuchFile;

        int keyPtrOffset = headerSize + childIdx * keyPtrSize;

        if(keyPtrOffset + keyPtrSize > nodeData.Length) return ErrorNumber.NoSuchFile;

        KeyPtr keyPtr = Marshal.ByteArrayToStructureLittleEndian<KeyPtr>(nodeData, keyPtrOffset, keyPtrSize);

        ErrorNumber errno = ReadTreeBlock(keyPtr.blockptr, out byte[] childData);

        if(errno != ErrorNumber.NoError) return errno;

        Header childHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(childData);

        return FindDefaultSubvolume(childData, childHeader, out subvolId);
    }

    /// <summary>
    ///     Scans a leaf node for a DIR_ITEM with objectid=ROOT_TREE_DIR_OBJECTID (6) and name="default",
    ///     returning the location.objectid which identifies the default subvolume.
    /// </summary>
    /// <param name="leafData">Raw leaf node data</param>
    /// <param name="header">Parsed leaf header</param>
    /// <param name="subvolId">The default subvolume objectid if found</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ExtractDefaultSubvolFromLeaf(byte[] leafData, in Header header, out ulong subvolId)
    {
        subvolId = 0;
        int itemSize    = Marshal.SizeOf<Item>();
        int headerSize  = Marshal.SizeOf<Header>();
        int dirItemSize = Marshal.SizeOf<DirItem>();

        uint startItem = BinarySearchLeaf(leafData, header, BTRFS_ROOT_TREE_DIR_OBJECTID);

        for(uint i = startItem; i < header.nritems; i++)
        {
            int itemOffset = headerSize + (int)i * itemSize;

            if(itemOffset + itemSize > leafData.Length) break;

            Item item = Marshal.ByteArrayToStructureLittleEndian<Item>(leafData, itemOffset, itemSize);

            if(item.key.objectid > BTRFS_ROOT_TREE_DIR_OBJECTID) break;

            if(item.key.objectid != BTRFS_ROOT_TREE_DIR_OBJECTID || item.key.type != BTRFS_DIR_ITEM_KEY) continue;

            int dataOffset = headerSize + (int)item.offset;

            if(dataOffset + dirItemSize > leafData.Length) continue;

            DirItem dirItem = Marshal.ByteArrayToStructureLittleEndian<DirItem>(leafData, dataOffset, dirItemSize);

            int nameOffset = dataOffset + dirItemSize;

            if(nameOffset + dirItem.name_len > leafData.Length) continue;

            string name = _encoding.GetString(leafData, nameOffset, dirItem.name_len);

            if(name != "default") continue;

            subvolId = dirItem.location.objectid;

            AaruLogging.Debug(MODULE_NAME, "Found default subvolume DIR_ITEM: location objectid={0}", subvolId);

            return ErrorNumber.NoError;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>
    ///     Recursively searches a tree (starting from a node) for a ROOT_ITEM with the specified objectid. When found,
    ///     stores the FS tree root address and level in instance fields.
    /// </summary>
    /// <param name="nodeData">Raw tree node data</param>
    /// <param name="header">Parsed node header</param>
    /// <param name="targetObjectId">The objectid to search for (typically BTRFS_FS_TREE_OBJECTID = 5)</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber SearchTreeForRootItem(byte[] nodeData, in Header header, ulong targetObjectId)
    {
        ErrorNumber errno = ResolveSubvolumeRoot(nodeData, header, targetObjectId, out ulong bytenr, out byte level);

        if(errno != ErrorNumber.NoError) return errno;

        _fsTreeRoot  = bytenr;
        _fsTreeLevel = level;

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Resolves a subvolume's tree root by searching the root tree for its ROOT_ITEM.
    ///     Returns the bytenr and level of the subvolume's tree root node.
    /// </summary>
    /// <param name="subvolObjectId">The objectid of the subvolume (ROOT_ITEM key)</param>
    /// <param name="treeRoot">The logical byte address of the subvolume's tree root</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ResolveSubvolumeRoot(ulong subvolObjectId, out ulong treeRoot)
    {
        treeRoot = 0;

        ErrorNumber errno = ReadTreeBlock(_superblock.root_lba, out byte[] rootTreeData);

        if(errno != ErrorNumber.NoError) return errno;

        Header rootHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(rootTreeData);

        return ResolveSubvolumeRoot(rootTreeData, rootHeader, subvolObjectId, out treeRoot, out _);
    }

    /// <summary>
    ///     Recursively searches a tree node for a ROOT_ITEM with the specified objectid and returns
    ///     its bytenr and level without modifying instance fields.
    /// </summary>
    /// <param name="nodeData">Raw tree node data</param>
    /// <param name="header">Parsed node header</param>
    /// <param name="targetObjectId">The objectid to search for</param>
    /// <param name="bytenr">The logical byte address of the found tree root</param>
    /// <param name="level">The level of the found tree root node</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ResolveSubvolumeRoot(byte[]   nodeData, in Header header, ulong targetObjectId, out ulong bytenr,
                                     out byte level)
    {
        bytenr = 0;
        level  = 0;

        int headerSize = Marshal.SizeOf<Header>();

        if(header.level == 0)
        {
            // Leaf node - search for the root item
            int itemSize = Marshal.SizeOf<Item>();

            uint startItem = BinarySearchLeaf(nodeData, header, targetObjectId);

            for(uint i = startItem; i < header.nritems; i++)
            {
                int itemOffset = headerSize + (int)i * itemSize;

                if(itemOffset + itemSize > nodeData.Length) break;

                var  objectid = BitConverter.ToUInt64(nodeData, itemOffset);
                byte keyType  = nodeData[itemOffset                        + 8];
                var  dataOff  = BitConverter.ToUInt32(nodeData, itemOffset + 17);

                if(objectid > targetObjectId) break;

                if(objectid != targetObjectId || keyType != BTRFS_ROOT_ITEM_KEY) continue;

                int dataOffset   = headerSize + (int)dataOff;
                int rootItemSize = Marshal.SizeOf<RootItem>();

                if(dataOffset + rootItemSize > nodeData.Length)
                {
                    AaruLogging.Debug(MODULE_NAME, "Root item data truncated");

                    return ErrorNumber.InvalidArgument;
                }

                RootItem rootItem =
                    Marshal.ByteArrayToStructureLittleEndian<RootItem>(nodeData, dataOffset, rootItemSize);

                bytenr = rootItem.bytenr;
                level  = rootItem.level;

                AaruLogging.Debug(MODULE_NAME,
                                  "Found tree root for objectid {0}: bytenr={1}, level={2}",
                                  targetObjectId,
                                  rootItem.bytenr,
                                  rootItem.level);

                return ErrorNumber.NoError;
            }

            return ErrorNumber.NoSuchFile;
        }

        // Internal node - use binary search to find the child containing our target
        int keyPtrSize = Marshal.SizeOf<KeyPtr>();

        var keyBytes = new byte[17];
        BitConverter.TryWriteBytes(keyBytes.AsSpan(0), targetObjectId);
        keyBytes[8] = BTRFS_ROOT_ITEM_KEY;

        // offset = 0 already from array init
        DiskKey searchKey = Marshal.ByteArrayToStructureLittleEndian<DiskKey>(keyBytes);

        int idx = BinarySearchNode(nodeData, header, searchKey);

        if(idx < 0) return ErrorNumber.NoSuchFile;

        int keyPtrOffset = headerSize + idx * keyPtrSize;

        if(keyPtrOffset + keyPtrSize > nodeData.Length) return ErrorNumber.NoSuchFile;

        KeyPtr keyPtr = Marshal.ByteArrayToStructureLittleEndian<KeyPtr>(nodeData, keyPtrOffset, keyPtrSize);

        ErrorNumber errno = ReadTreeBlock(keyPtr.blockptr, out byte[] childData);

        if(errno != ErrorNumber.NoError) return errno;

        Header childHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(childData);

        return ResolveSubvolumeRoot(childData, childHeader, targetObjectId, out bytenr, out level);
    }
}