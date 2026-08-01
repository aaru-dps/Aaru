// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
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
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class Reiser
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory
        if(normalizedPath is "/")
        {
            if(_rootDirectoryCache.Count == 0) return ErrorNumber.NoSuchFile;

            node = new ReiserDirNode
            {
                Path     = "/",
                Position = 0,
                Entries  = _rootDirectoryCache.Keys.ToArray()
            };

            return ErrorNumber.NoError;
        }

        // Subdirectory traversal
        string stripped = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                              ? normalizedPath[1..]
                              : normalizedPath;

        string[] components = stripped.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(components.Length == 0) return ErrorNumber.InvalidArgument;

        AaruLogging.Debug(MODULE_NAME, "OpenDir: traversing {0} components", components.Length);

        // Start from root directory cache
        Dictionary<string, (uint dirId, uint objectId)> currentEntries = _rootDirectoryCache;

        for(var i = 0; i < components.Length; i++)
        {
            string component = components[i];

            AaruLogging.Debug(MODULE_NAME, "OpenDir: navigating to '{0}'", component);

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out (uint dirId, uint objectId) target))
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: '{0}' not found", component);

                return ErrorNumber.NoSuchFile;
            }

            // Verify it's a directory
            ErrorNumber errno = ReadObjectMode(target.dirId, target.objectId, out ushort mode);

            if(errno != ErrorNumber.NoError) return errno;

            if((mode & S_IFMT) != S_IFDIR)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: '{0}' is not a directory (mode=0x{1:X4})", component, mode);

                return ErrorNumber.NotDirectory;
            }

            // Read this subdirectory's entries
            errno = GetDirectoryEntries(target.dirId,
                                        target.objectId,
                                        out Dictionary<string, (uint dirId, uint objectId)> subEntries);

            if(errno != ErrorNumber.NoError) return errno;

            // Filter . and ..
            var filtered = new Dictionary<string, (uint dirId, uint objectId)>();

            foreach(KeyValuePair<string, (uint dirId, uint objectId)> entry in subEntries)
            {
                if(entry.Key is "." or "..") continue;

                filtered[entry.Key] = entry.Value;
            }

            // Last component — this is the directory being opened
            if(i == components.Length - 1)
            {
                node = new ReiserDirNode
                {
                    Path     = normalizedPath,
                    Position = 0,
                    Entries  = filtered.Keys.ToArray()
                };

                AaruLogging.Debug(MODULE_NAME,
                                  "OpenDir: opened '{0}' with {1} entries",
                                  normalizedPath,
                                  filtered.Count);

                return ErrorNumber.NoError;
            }

            // Intermediate component — descend
            currentEntries = filtered;
        }

        return ErrorNumber.NoSuchFile;
    }


    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node) => ErrorNumber.NoError;

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(node is not ReiserDirNode dirNode) return ErrorNumber.InvalidArgument;

        if(dirNode.Position >= dirNode.Entries.Length) return ErrorNumber.NoError; // No more entries, filename is null

        filename = dirNode.Entries[dirNode.Position];
        dirNode.Position++;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the mode field from an object's stat data, caching by (dirId, objectId)</summary>
    ErrorNumber ReadObjectMode(uint dirId, uint objectId, out ushort mode)
    {
        (uint dirId, uint objectId) key = (dirId, objectId);

        if(_modeCache.TryGetValue(key, out mode)) return ErrorNumber.NoError;

        ErrorNumber cacheErrno = ReadObjectModeUncached(dirId, objectId, out mode);

        if(cacheErrno == ErrorNumber.NoError) _modeCache[key] = mode;

        return cacheErrno;
    }

    /// <summary>Reads the mode field from an object's stat data</summary>
    ErrorNumber ReadObjectModeUncached(uint dirId, uint objectId, out ushort mode)
    {
        mode = 0;

        // Search for the stat data item for this object
        ErrorNumber errno = SearchByKey(dirId, objectId, 0, TYPE_STAT_DATA, out byte[] leafBlock, out int itemIndex);

        if(errno != ErrorNumber.NoError) return errno;

        if(itemIndex < 0) return ErrorNumber.NoSuchFile;

        BlockHead blkHead = ReadBlockHead(leafBlock);

        if(itemIndex >= blkHead.blk_nr_item) return ErrorNumber.NoSuchFile;

        ItemHead ih = ReadItemHead(leafBlock, BLKH_SIZE + itemIndex * IH_SIZE);

        // Verify this is the stat data item
        if(ih.ih_key.k_dir_id != dirId || ih.ih_key.k_objectid != objectId) return ErrorNumber.NoSuchFile;

        int ihType = GetKeyType(ih.ih_key, GetItemKeyVersion(ih.ih_version));

        if(ihType != TYPE_STAT_DATA) return ErrorNumber.NoSuchFile;

        if(ih.ih_item_location + 2 > leafBlock.Length) return ErrorNumber.NoSuchFile;

        // Mode is at offset 0 in the stat data (first 2 bytes)
        mode = BitConverter.ToUInt16(leafBlock, ih.ih_item_location);

        return ErrorNumber.NoError;
    }

    /// <summary>Gets directory entries for the given directory object, caching them as the filesystem is read-only.</summary>
    /// <param name="dirId">Directory (packing locality) id of the directory</param>
    /// <param name="objectId">Object id of the directory</param>
    /// <param name="entries">Parsed directory entries (filename -> (dirId, objectId) of target)</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber GetDirectoryEntries(uint                                                dirId, uint objectId,
                                    out Dictionary<string, (uint dirId, uint objectId)> entries)
    {
        (uint dirId, uint objectId) key = (dirId, objectId);

        if(_directoryCache.TryGetValue(key, out entries)) return ErrorNumber.NoError;

        ErrorNumber errno = ReadDirectoryEntries(dirId, objectId, out entries);

        if(errno == ErrorNumber.NoError) _directoryCache[key] = entries;

        return errno;
    }

    /// <summary>
    ///     Reads all directory entries for the given directory object.
    ///     Mirrors the kernel's <c>reiserfs_readdir_inode</c> exactly:
    ///     one item per iteration, tree path maintained, right delimiting key
    ///     used for leaf-boundary continuation.
    /// </summary>
    /// <param name="dirId">Directory (packing locality) id of the directory</param>
    /// <param name="objectId">Object id of the directory</param>
    /// <param name="entries">Parsed directory entries (filename -> (dirId, objectId) of target)</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadDirectoryEntries(uint                                                dirId, uint objectId,
                                     out Dictionary<string, (uint dirId, uint objectId)> entries)
    {
        entries = new Dictionary<string, (uint dirId, uint objectId)>();

        ulong nextPos  = DOT_OFFSET;
        var   foundAny = false;

        TreePath path = null;

        while(true)
        {
            ErrorNumber errno = SearchByKey(dirId,
                                            objectId,
                                            nextPos,
                                            TYPE_DIRENTRY,
                                            out byte[] leafBlock,
                                            out int itemIndex,
                                            ref path);

            if(errno != ErrorNumber.NoError) break;

            if(itemIndex < 0) break;

            BlockHead blkHead = ReadBlockHead(leafBlock);

            if(itemIndex >= blkHead.blk_nr_item) break;

            ItemHead ih = ReadItemHead(leafBlock, BLKH_SIZE + itemIndex * IH_SIZE);

            if(ih.ih_key.k_dir_id != dirId || ih.ih_key.k_objectid != objectId) break;

            int ihVersion = GetItemKeyVersion(ih.ih_version);
            int ihType    = GetKeyType(ih.ih_key, ihVersion);

            if(ihType != TYPE_DIRENTRY) break;

            if(ih.ih_item_location + ih.ih_item_len > leafBlock.Length) break;

            int entryNum =
                BinarySearchEntries(leafBlock, ih.ih_item_location, ih.ih_free_space_or_entry_count, nextPos);

            if(entryNum < ih.ih_free_space_or_entry_count)
            {
                for(int e = entryNum; e < ih.ih_free_space_or_entry_count; e++)
                {
                    int dehOff = ih.ih_item_location + e * DEH_SIZE;

                    if(dehOff + DEH_SIZE > leafBlock.Length) break;

                    DirectoryEntryHead deh = ReadDirEntryHead(leafBlock, dehOff);

                    if((deh.deh_state & 1 << DEH_VISIBLE) == 0) continue;

                    int nameStart = ih.ih_item_location + deh.deh_location;
                    int nameLen;

                    if(e == 0)
                        nameLen = ih.ih_item_len - deh.deh_location;
                    else
                    {
                        DirectoryEntryHead prevDeh =
                            ReadDirEntryHead(leafBlock, ih.ih_item_location + (e - 1) * DEH_SIZE);

                        nameLen = prevDeh.deh_location - deh.deh_location;
                    }

                    if(nameLen <= 0 || nameStart < 0 || nameStart >= leafBlock.Length) continue;

                    if(nameLen > REISERFS_MAX_NAME) continue;

                    if(nameStart + nameLen > leafBlock.Length) nameLen = leafBlock.Length - nameStart;

                    if(nameLen <= 0) continue;

                    var nameBytes = new byte[nameLen];
                    Array.Copy(leafBlock, nameStart, nameBytes, 0, nameLen);

                    string name = StringHandlers.CToString(nameBytes, _encoding);

                    if(string.IsNullOrEmpty(name)) continue;

                    entries[name] = (deh.deh_dir_id, deh.deh_objectid);
                    foundAny      = true;

                    nextPos = deh.deh_offset + 1;
                }
            }

            // End of directory reached if not last item in leaf
            if(itemIndex != blkHead.blk_nr_item - 1) break;

            // Get right delimiting key from tree path
            Key? rkey = GetRKey(path);

            // rkey == MIN_KEY (all zeros) — continue with next_pos
            if(rkey.HasValue && rkey.Value.k_dir_id == 0 && rkey.Value.k_objectid == 0 && rkey.Value.k_offset_v2 == 0)
                continue;

            // rkey == MAX_KEY (null) — rightmost edge of tree, continue with next_pos
            if(!rkey.HasValue) continue;

            // rkey belongs to a different object — directory end
            if(CompareShortKeys(rkey.Value.k_dir_id, rkey.Value.k_objectid, dirId, objectId) != 0) break;

            // Directory continues in the right neighboring block
            nextPos = GetKeyOffset(rkey.Value, KEY_FORMAT_3_5);
        }

        if(!foundAny) return ErrorNumber.NoSuchFile;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a DirectoryEntryHead struct from raw block data at the given offset</summary>
    static DirectoryEntryHead ReadDirEntryHead(byte[] data, int offset) =>
        Marshal.ByteArrayToStructureLittleEndian<DirectoryEntryHead>(data, offset, DEH_SIZE);

    /// <summary>Binary search to find the entry index to start from within a directory item</summary>
    /// <param name="blockData">Block containing the item</param>
    /// <param name="itemLocation">Location of item body in block</param>
    /// <param name="entryCount">Number of entries in the item</param>
    /// <param name="searchOffset">The offset to search for</param>
    /// <returns>Entry index to start processing from</returns>
    static int BinarySearchEntries(byte[] blockData, int itemLocation, int entryCount, ulong searchOffset)
    {
        var lo     = 0;
        int hi     = entryCount - 1;
        int result = entryCount; // Default: past the end

        while(lo <= hi)
        {
            int                mid = (lo + hi) / 2;
            DirectoryEntryHead deh = ReadDirEntryHead(blockData, itemLocation + mid * DEH_SIZE);

            if(deh.deh_offset >= searchOffset)
            {
                result = mid;
                hi     = mid - 1;
            }
            else
                lo = mid + 1;
        }

        return result;
    }
}