// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : B-tree file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Directory reading and enumeration methods.
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
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class BTRFS
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory
        if(normalizedPath is "/")
        {
            if(_rootDirectoryCache.Count == 0) return ErrorNumber.NoSuchFile;

            node = new BtrfsDirNode
            {
                Path     = "/",
                Position = 0,
                Entries  = _rootDirectoryCache.Keys.ToArray()
            };

            return ErrorNumber.NoError;
        }

        // Subdirectory traversal
        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        Dictionary<string, DirEntry> currentEntries  = _rootDirectoryCache;
        ulong                        currentTreeRoot = _fsTreeRoot;

        for(var p = 0; p < pathComponents.Length; p++)
        {
            string component = pathComponents[p];

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out DirEntry entry)) return ErrorNumber.NoSuchFile;

            if(entry.Type != BTRFS_FT_DIR) return ErrorNumber.NotDirectory;

            // Handle subvolume crossing
            ulong treeRoot = entry.SubvolTreeRoot != 0 ? entry.SubvolTreeRoot : currentTreeRoot;

            ErrorNumber errno =
                GetDirectoryContents(entry.ObjectId, treeRoot, out Dictionary<string, DirEntry> dirEntries);

            if(errno != ErrorNumber.NoError) return errno;

            if(p == pathComponents.Length - 1)
            {
                node = new BtrfsDirNode
                {
                    Path     = normalizedPath,
                    Position = 0,
                    Entries  = dirEntries.Keys.ToArray()
                };

                return ErrorNumber.NoError;
            }

            currentEntries  = dirEntries;
            currentTreeRoot = treeRoot;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not BtrfsDirNode btrfsNode) return ErrorNumber.InvalidArgument;

        btrfsNode.Position = -1;
        btrfsNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not BtrfsDirNode btrfsNode) return ErrorNumber.InvalidArgument;

        if(btrfsNode.Position < 0) return ErrorNumber.InvalidArgument;

        // End of directory
        if(btrfsNode.Position >= btrfsNode.Entries.Length) return ErrorNumber.NoError;

        // Get current filename and advance position
        filename = btrfsNode.Entries[btrfsNode.Position++];

        return ErrorNumber.NoError;
    }

    /// <summary>Loads the root directory of the default subvolume from the FS tree and caches its entries</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory from FS tree...");

        _rootDirectoryCache = new Dictionary<string, DirEntry>();

        ErrorNumber errno = ReadTreeBlock(_fsTreeRoot, out byte[] fsTreeData);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading FS tree root: {0}", errno);

            return errno;
        }

        Header fsTreeHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(fsTreeData);

        AaruLogging.Debug(MODULE_NAME, "FS tree root level: {0}, items: {1}", fsTreeHeader.level, fsTreeHeader.nritems);

        // The root directory inode is BTRFS_FIRST_FREE_OBJECTID (256) in the default subvolume
        // We search for DIR_INDEX items for objectid 256 to get directory entries sorted by index
        errno = WalkTreeForDirEntries(fsTreeData, fsTreeHeader, BTRFS_FIRST_FREE_OBJECTID, _rootDirectoryCache);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error walking FS tree for directory entries: {0}", errno);

            return errno;
        }

        // Resolve subvolume entries: when a directory entry points to a ROOT_ITEM,
        // look up the subvolume's tree root and update the entry accordingly
        List<string> subvolKeys = [];

        foreach(KeyValuePair<string, DirEntry> kvp in _rootDirectoryCache)
            if(kvp.Value.LocationType == BTRFS_ROOT_ITEM_KEY)
                subvolKeys.Add(kvp.Key);

        foreach(string key in subvolKeys)
        {
            DirEntry entry = _rootDirectoryCache[key];

            ErrorNumber subvolErrno = ResolveSubvolumeRoot(entry.ObjectId, out ulong subvolRoot);

            if(subvolErrno != ErrorNumber.NoError) continue;

            entry.SubvolTreeRoot     = subvolRoot;
            entry.ObjectId           = BTRFS_FIRST_FREE_OBJECTID;
            _rootDirectoryCache[key] = entry;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Walks a tree node recursively to find all DIR_INDEX items for the specified directory objectid. DIR_INDEX items
    ///     are preferred over DIR_ITEM because they provide entries sorted by insertion order (index).
    /// </summary>
    /// <param name="nodeData">Raw tree node data</param>
    /// <param name="header">Parsed node header</param>
    /// <param name="dirObjectId">The objectid of the directory (inode) to enumerate</param>
    /// <param name="entries">Dictionary to collect directory entries into</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber WalkTreeForDirEntries(byte[]                       nodeData, in Header header, ulong dirObjectId,
                                      Dictionary<string, DirEntry> entries)
    {
        int headerSize = Marshal.SizeOf<Header>();

        if(header.level == 0) return ExtractDirEntriesFromLeaf(nodeData, header, dirObjectId, entries);

        // Internal node - use binary search to find children whose subtrees may contain our objectid
        int keyPtrSize = Marshal.SizeOf<KeyPtr>();

        BinarySearchNodeRange(nodeData, header, dirObjectId, out int startIdx, out int endIdx);

        for(int i = startIdx; i < endIdx; i++)
        {
            int keyPtrOffset = headerSize + i * keyPtrSize;

            if(keyPtrOffset + keyPtrSize > nodeData.Length) break;

            KeyPtr keyPtr = Marshal.ByteArrayToStructureLittleEndian<KeyPtr>(nodeData, keyPtrOffset, keyPtrSize);

            ErrorNumber errno = ReadTreeBlock(keyPtr.blockptr, out byte[] childData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading FS tree child at {0}: {1}", keyPtr.blockptr, errno);

                continue;
            }

            Header childHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(childData);

            errno = WalkTreeForDirEntries(childData, childHeader, dirObjectId, entries);

            if(errno != ErrorNumber.NoError) return errno;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Extracts directory entries from a leaf node for the specified directory objectid. Looks for DIR_INDEX items and
    ///     caches the directory entries.
    /// </summary>
    /// <param name="leafData">Raw leaf node data</param>
    /// <param name="header">Parsed leaf header</param>
    /// <param name="dirObjectId">The objectid of the directory to enumerate</param>
    /// <param name="entries">Dictionary to collect directory entries into</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ExtractDirEntriesFromLeaf(byte[]                       leafData, in Header header, ulong dirObjectId,
                                          Dictionary<string, DirEntry> entries)
    {
        int itemSize    = Marshal.SizeOf<Item>();
        int headerSize  = Marshal.SizeOf<Header>();
        int dirItemSize = Marshal.SizeOf<DirItem>();

        uint startItem = BinarySearchLeaf(leafData, header, dirObjectId);

        for(uint i = startItem; i < header.nritems; i++)
        {
            int itemOffset = headerSize + (int)i * itemSize;

            if(itemOffset + itemSize > leafData.Length) break;

            Item item = Marshal.ByteArrayToStructureLittleEndian<Item>(leafData, itemOffset, itemSize);

            if(item.key.objectid > dirObjectId) break;

            // We want DIR_INDEX items for our directory objectid
            if(item.key.objectid != dirObjectId || item.key.type != BTRFS_DIR_INDEX_KEY) continue;

            int dataOffset = headerSize + (int)item.offset;

            if(dataOffset + dirItemSize > leafData.Length) continue;

            DirItem dirItem = Marshal.ByteArrayToStructureLittleEndian<DirItem>(leafData, dataOffset, dirItemSize);

            int nameOffset = dataOffset + dirItemSize;

            if(nameOffset + dirItem.name_len > leafData.Length) continue;

            string name = _encoding.GetString(leafData, nameOffset, dirItem.name_len);

            // Skip . and .. entries
            if(name is "." or "..") continue;

            entries[name] = new DirEntry
            {
                ObjectId     = dirItem.location.objectid,
                Type         = dirItem.type,
                Index        = item.key.offset,
                LocationType = dirItem.location.type
            };

            AaruLogging.Debug(MODULE_NAME,
                              "Root directory entry: \"{0}\" -> objectid {1}, type {2}",
                              name,
                              dirItem.location.objectid,
                              dirItem.type);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Gets the contents of a directory given its inode objectid, caching them as the filesystem is
    ///     read-only.
    /// </summary>
    /// <param name="dirObjectId">The objectid of the directory inode</param>
    /// <param name="treeRoot">The logical byte address of the tree to read from</param>
    /// <param name="entries">The directory entries found</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber GetDirectoryContents(ulong dirObjectId, ulong treeRoot, out Dictionary<string, DirEntry> entries)
    {
        (ulong dirObjectId, ulong treeRoot) key = (dirObjectId, treeRoot);

        if(_directoryCache.TryGetValue(key, out entries)) return ErrorNumber.NoError;

        ErrorNumber errno = ReadDirectoryContents(dirObjectId, treeRoot, out entries);

        if(errno == ErrorNumber.NoError) _directoryCache[key] = entries;

        return errno;
    }

    /// <summary>Reads the contents of a directory given its inode objectid by walking the specified tree</summary>
    /// <param name="dirObjectId">The objectid of the directory inode</param>
    /// <param name="treeRoot">The logical byte address of the tree to read from</param>
    /// <param name="entries">The directory entries found</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadDirectoryContents(ulong dirObjectId, ulong treeRoot, out Dictionary<string, DirEntry> entries)
    {
        entries = new Dictionary<string, DirEntry>();

        ErrorNumber errno = ReadTreeBlock(treeRoot, out byte[] fsTreeData);

        if(errno != ErrorNumber.NoError) return errno;

        Header fsTreeHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(fsTreeData);

        errno = WalkTreeForDirEntries(fsTreeData, fsTreeHeader, dirObjectId, entries);

        if(errno != ErrorNumber.NoError) return errno;

        // Resolve subvolume entries: when a directory entry points to a ROOT_ITEM,
        // look up the subvolume's tree root and update the entry accordingly
        List<string> subvolKeys = [];

        foreach(KeyValuePair<string, DirEntry> kvp in entries)
            if(kvp.Value.LocationType == BTRFS_ROOT_ITEM_KEY)
                subvolKeys.Add(kvp.Key);

        foreach(string key in subvolKeys)
        {
            DirEntry entry = entries[key];

            ErrorNumber subvolErrno = ResolveSubvolumeRoot(entry.ObjectId, out ulong subvolRoot);

            if(subvolErrno != ErrorNumber.NoError) continue;

            entry.SubvolTreeRoot = subvolRoot;
            entry.ObjectId       = BTRFS_FIRST_FREE_OBJECTID;
            entries[key]         = entry;
        }

        return ErrorNumber.NoError;
    }
}