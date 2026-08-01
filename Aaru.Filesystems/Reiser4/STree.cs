// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : STree.cs
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
using Aaru.CommonTypes.Enums;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class Reiser4
{
    /// <summary>
    ///     Builds the root directory stat-data key for format40.
    ///     el[0] = (FORMAT40_ROOT_LOCALITY &lt;&lt; 4) | KEY_SD_MINOR
    ///     el[1] = 0 (ordering, large keys) or FORMAT40_ROOT_OBJECTID (short keys)
    ///     el[2] = FORMAT40_ROOT_OBJECTID (large keys) or 0 (short keys)
    ///     el[3] = 0 (large keys only)
    /// </summary>
    LargeKey BuildRootStatDataKey()
    {
        if(_largeKeys)
        {
            return new LargeKey
            {
                el0 = FORMAT40_ROOT_LOCALITY << KEY_LOCALITY_SHIFT | KEY_SD_MINOR,
                el1 = 0,
                el2 = FORMAT40_ROOT_OBJECTID,
                el3 = 0
            };
        }

        // For non-large keys we still store LargeKey with el3=0 so comparison works uniformly
        return new LargeKey
        {
            el0 = FORMAT40_ROOT_LOCALITY << KEY_LOCALITY_SHIFT | KEY_SD_MINOR,
            el1 = FORMAT40_ROOT_OBJECTID,
            el2 = 0,
            el3 = 0
        };
    }

    /// <summary>
    ///     Builds a directory entry search key for the given directory objectid.
    ///     Used to find the first directory entry item for a directory.
    /// </summary>
    LargeKey BuildDirEntrySearchKey(ulong dirObjectId)
    {
        if(_largeKeys)
        {
            return new LargeKey
            {
                el0 = dirObjectId << KEY_LOCALITY_SHIFT | KEY_FILE_NAME_MINOR,
                el1 = 0,
                el2 = 0,
                el3 = 0
            };
        }

        return new LargeKey
        {
            el0 = dirObjectId << KEY_LOCALITY_SHIFT | KEY_FILE_NAME_MINOR,
            el1 = 0,
            el2 = 0,
            el3 = 0
        };
    }

    /// <summary>
    ///     Compares two keys element-by-element, as reiser4 does.
    ///     Returns negative if a &lt; b, 0 if equal, positive if a &gt; b.
    /// </summary>
    int CompareKeys(LargeKey a, LargeKey b)
    {
        if(a.el0 != b.el0) return a.el0 < b.el0 ? -1 : 1;
        if(a.el1 != b.el1) return a.el1 < b.el1 ? -1 : 1;
        if(a.el2 != b.el2) return a.el2 < b.el2 ? -1 : 1;

        if(!_largeKeys) return 0;

        if(a.el3 != b.el3) return a.el3 < b.el3 ? -1 : 1;

        return 0;
    }

    /// <summary>
    ///     Checks whether the locality+type of the key matches the given values.
    /// </summary>
    static bool KeyLocalityTypeMatch(LargeKey key, ulong locality, ulong type)
    {
        ulong keyLocality = (key.el0 & KEY_LOCALITY_MASK) >> KEY_LOCALITY_SHIFT;
        ulong keyType     = key.el0 & KEY_TYPE_MASK;

        return keyLocality == locality && keyType == type;
    }

    /// <summary>Extracts the locality field from a key</summary>
    static ulong GetKeyLocality(LargeKey key) => (key.el0 & KEY_LOCALITY_MASK) >> KEY_LOCALITY_SHIFT;

    /// <summary>Extracts the type/minor-locality field from a key</summary>
    static ulong GetKeyType(LargeKey key) => key.el0 & KEY_TYPE_MASK;

    /// <summary>Extracts the objectid field from a large key (lower 60 bits of el[2])</summary>
    static ulong GetKeyObjectId(LargeKey key) => key.el2 & KEY_OBJECTID_MASK;

    /// <summary>
    ///     Reads an item header from raw node data at the given position index.
    ///     Item headers are stored at the end of the node, growing inward (right to left).
    ///     Returns the key, body offset, flags, and plugin_id.
    /// </summary>
    void ReadItemHeader(byte[]     nodeData, int        itemPos, int nrItems, out LargeKey key, out ushort bodyOffset,
                        out ushort flags,    out ushort pluginId)
    {
        // Item header for pos i is at: nodeEnd - (i+1) * _itemHeaderSize
        int headerOff = nodeData.Length - (itemPos + 1) * _itemHeaderSize;

        if(_largeKeys)
        {
            key = new LargeKey
            {
                el0 = BitConverter.ToUInt64(nodeData, headerOff),
                el1 = BitConverter.ToUInt64(nodeData, headerOff + 8),
                el2 = BitConverter.ToUInt64(nodeData, headerOff + 16),
                el3 = BitConverter.ToUInt64(nodeData, headerOff + 24)
            };

            bodyOffset = BitConverter.ToUInt16(nodeData, headerOff + 32);
            flags      = BitConverter.ToUInt16(nodeData, headerOff + 34);
            pluginId   = BitConverter.ToUInt16(nodeData, headerOff + 36);
        }
        else
        {
            var e0 = BitConverter.ToUInt64(nodeData, headerOff);
            var e1 = BitConverter.ToUInt64(nodeData, headerOff + 8);
            var e2 = BitConverter.ToUInt64(nodeData, headerOff + 16);

            // Normalize short key into LargeKey for uniform comparison
            key = new LargeKey
            {
                el0 = e0,
                el1 = e1,
                el2 = e2,
                el3 = 0
            };

            bodyOffset = BitConverter.ToUInt16(nodeData, headerOff + 24);
            flags      = BitConverter.ToUInt16(nodeData, headerOff + 26);
            pluginId   = BitConverter.ToUInt16(nodeData, headerOff + 28);
        }
    }

    /// <summary>
    ///     Computes the length (in bytes) of item at position <paramref name="itemPos" /> within a node.
    /// </summary>
    int GetItemLength(byte[] nodeData, int itemPos, int nrItems, ushort freeSpaceStart)
    {
        // Get this item's body offset
        int headerOff = nodeData.Length - (itemPos + 1) * _itemHeaderSize;
        int keyBytes  = _largeKeys ? 32 : 24;

        var thisOffset = BitConverter.ToUInt16(nodeData, headerOff + keyBytes);

        if(itemPos == nrItems - 1)
        {
            // Last item: length = free_space_start - offset
            return freeSpaceStart - thisOffset;
        }

        // Next item's offset (item at itemPos+1, header to the LEFT in memory)
        int nextHeaderOff = nodeData.Length - (itemPos                    + 2) * _itemHeaderSize;
        var nextOffset    = BitConverter.ToUInt16(nodeData, nextHeaderOff + keyBytes);

        return nextOffset - thisOffset;
    }

    /// <summary>
    ///     Searches the reiser4 S+tree for items matching the given key.
    ///     Finds the node at the specified stop level containing the item with the greatest key &lt;= search key.
    /// </summary>
    /// <param name="searchKey">Key to search for</param>
    /// <param name="nodeData">Output: raw data of the node found</param>
    /// <param name="itemPos">Output: position of the best-match item within the node</param>
    /// <param name="stopLevel">Tree level to stop at (LEAF_LEVEL=1 for tails/stat-data, TWIG_LEVEL=2 for extents)</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber SearchByKey(LargeKey searchKey, out byte[] nodeData, out int itemPos, byte stopLevel = LEAF_LEVEL)
    {
        nodeData = null;
        itemPos  = -1;

        ulong currentBlock = _format40Sb.root_block;

        for(var level = 0; level < REISER4_MAX_TREE_HEIGHT; level++)
        {
            ErrorNumber errno = ReadBlock(currentBlock, out byte[] blockData);

            if(errno != ErrorNumber.NoError) return errno;

            if(blockData.Length < Marshal.SizeOf<Node40Header>()) return ErrorNumber.InvalidArgument;

            // Parse node header
            Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(blockData);

            if(nh.nr_items == 0) return ErrorNumber.NoSuchFile;

            if(nh.level <= stopLevel)
            {
                // Target level: binary search through item headers
                nodeData = blockData;
                var lo   = 0;
                int hi   = nh.nr_items - 1;
                int best = -1;

                while(lo <= hi)
                {
                    int mid = (lo + hi) / 2;

                    ReadItemHeader(blockData, mid, nh.nr_items, out LargeKey midKey, out _, out _, out _);

                    int cmp = CompareKeys(searchKey, midKey);

                    switch(cmp)
                    {
                        case 0:
                            // Exact match — find leftmost with same key
                            best = mid;

                            while(best > 0)
                            {
                                ReadItemHeader(blockData,
                                               best - 1,
                                               nh.nr_items,
                                               out LargeKey prevKey,
                                               out _,
                                               out _,
                                               out _);

                                if(CompareKeys(prevKey, midKey) != 0) break;

                                best--;
                            }

                            itemPos = best;

                            return ErrorNumber.NoError;
                        case > 0:
                            best = mid;
                            lo   = mid + 1;

                            break;
                        default:
                            hi = mid - 1;

                            break;
                    }
                }

                itemPos = best;

                return best >= 0 ? ErrorNumber.NoError : ErrorNumber.NoSuchFile;
            }

            // Internal node: binary search through item keys to find which child to follow
            var lo2  = 0;
            int hi2  = nh.nr_items - 1;
            var pos2 = 0;

            while(lo2 <= hi2)
            {
                int mid2 = (lo2 + hi2) / 2;

                ReadItemHeader(blockData, mid2, nh.nr_items, out LargeKey midKey, out _, out _, out _);

                int cmp2 = CompareKeys(searchKey, midKey);

                if(cmp2 >= 0)
                {
                    pos2 = mid2;
                    lo2  = mid2 + 1;
                }
                else
                    hi2 = mid2 - 1;
            }

            // Read the internal item body at pos2 to get the child block pointer
            ReadItemHeader(blockData, pos2, nh.nr_items, out _, out ushort bodyOff, out _, out _);

            if(bodyOff + 8 > blockData.Length) return ErrorNumber.InvalidArgument;

            currentBlock = BitConverter.ToUInt64(blockData, bodyOff);

            if(currentBlock == 0) return ErrorNumber.NoSuchFile;
        }

        AaruLogging.Debug(MODULE_NAME, "Tree traversal exceeded maximum height");

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>
    ///     Reads all directory entries from the tree for the directory with the given objectid.
    ///     Handles both compound (CDE) and simple (SDE) directory entry items.
    /// </summary>
    ErrorNumber ReadDirectoryEntries(ulong dirObjectId, out Dictionary<string, LargeKey> entries)
    {
        if(_directoryCache.TryGetValue(dirObjectId, out entries)) return ErrorNumber.NoError;

        ErrorNumber cacheErrno = ReadDirectoryEntriesUncached(dirObjectId, out entries);

        if(cacheErrno == ErrorNumber.NoError) _directoryCache[dirObjectId] = entries;

        return cacheErrno;
    }

    /// <summary>
    ///     Reads all directory entries from the tree for the directory with the given objectid.
    ///     Handles both compound (CDE) and simple (SDE) directory entry items.
    /// </summary>
    ErrorNumber ReadDirectoryEntriesUncached(ulong dirObjectId, out Dictionary<string, LargeKey> entries)
    {
        entries = new Dictionary<string, LargeKey>(StringComparer.Ordinal);

        LargeKey searchKey = BuildDirEntrySearchKey(dirObjectId);

        ErrorNumber errno = SearchByKey(searchKey, out byte[] leafData, out int itemPos);

        if(errno != ErrorNumber.NoError) return errno;

        if(itemPos < 0) return ErrorNumber.NoError; // Empty directory

        Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(leafData);

        // Iterate through items in this leaf starting from itemPos
        for(int i = itemPos; i < nh.nr_items; i++)
        {
            ReadItemHeader(leafData,
                           i,
                           nh.nr_items,
                           out LargeKey itemKey,
                           out ushort bodyOff,
                           out _,
                           out ushort pluginId);

            // Check if this item still belongs to the same directory
            if(!KeyLocalityTypeMatch(itemKey, dirObjectId, KEY_FILE_NAME_MINOR)) break;

            int itemLen = GetItemLength(leafData, i, nh.nr_items, nh.free_space_start);

            if(itemLen <= 0 || bodyOff + itemLen > leafData.Length) continue;

            if(pluginId == COMPOUND_DIR_ENTRY_ID)
                ParseCdeItem(leafData, bodyOff, itemLen, itemKey, entries);
            else if(pluginId == SIMPLE_DIR_ENTRY_ID) ParseSdeItem(leafData, bodyOff, itemLen, itemKey, entries);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Parses a compound directory entry item (CDE) and adds entries to the dictionary.
    /// </summary>
    void ParseCdeItem(byte[] data, int itemOffset, int itemLen, LargeKey itemKey, Dictionary<string, LargeKey> entries)
    {
        if(itemLen < 2) return;

        var numEntries = BitConverter.ToUInt16(data, itemOffset);

        if(numEntries == 0) return;

        int objKeyIdSize   = _largeKeys ? 24 : 16; // ObjKeyIdLarge : ObjKeyId
        int unitHeaderSize = _largeKeys ? 26 : 18; // DeIdLarge(24)+u16 : DeId(16)+u16

        // Unit headers start right after the num_of_entries field
        int unitHeadersStart = itemOffset + 2;

        for(var u = 0; u < numEntries; u++)
        {
            int uhOff = unitHeadersStart + u * unitHeaderSize;

            if(uhOff + unitHeaderSize > itemOffset + itemLen) break;

            // Read the offset to the entry body (last 2 bytes of unit header)
            var entryBodyOff = BitConverter.ToUInt16(data, uhOff + unitHeaderSize - 2);

            int absEntryOff = itemOffset + entryBodyOff;

            if(absEntryOff + objKeyIdSize > data.Length) continue;

            // Read the obj_key_id from the entry body
            LargeKey sdKey = ReadObjKeyIdAsKey(data, absEntryOff);

            // Read the name that follows the obj_key_id
            int nameStart = absEntryOff + objKeyIdSize;

            // Determine the end of this entry's body
            int entryBodyEnd;

            if(u + 1 < numEntries)
            {
                int nextUhOff = unitHeadersStart + (u + 1) * unitHeaderSize;

                if(nextUhOff + unitHeaderSize <= itemOffset + itemLen)
                    entryBodyEnd = itemOffset + BitConverter.ToUInt16(data, nextUhOff + unitHeaderSize - 2);
                else
                    entryBodyEnd = itemOffset + itemLen;
            }
            else
                entryBodyEnd = itemOffset + itemLen;

            if(nameStart >= entryBodyEnd || nameStart >= data.Length) continue;

            // Read the entry key from the unit header (de_id portion)
            // The unit header contains de_id which holds part of the entry key.
            // We need the entry key to determine whether the name is "long" or "short".
            LargeKey entryKey = ReadEntryKeyFromUnitHeader(data, uhOff, itemKey);

            string name = ExtractEntryName(data, nameStart, entryBodyEnd, entryKey);

            if(!string.IsNullOrEmpty(name) && !entries.ContainsKey(name)) entries[name] = sdKey;
        }
    }

    /// <summary>
    ///     Parses a simple directory entry item (SDE) and adds it to the dictionary.
    ///     A simple directory entry item contains exactly one entry.
    /// </summary>
    void ParseSdeItem(byte[] data, int itemOffset, int itemLen, LargeKey itemKey, Dictionary<string, LargeKey> entries)
    {
        int objKeyIdSize = _largeKeys ? 24 : 16;

        if(itemLen < objKeyIdSize) return;

        LargeKey sdKey    = ReadObjKeyIdAsKey(data, itemOffset);
        int      nameOff  = itemOffset + objKeyIdSize;
        int      entryEnd = itemOffset + itemLen;

        string name = ExtractEntryName(data, nameOff, entryEnd, itemKey);

        if(!string.IsNullOrEmpty(name) && !entries.ContainsKey(name)) entries[name] = sdKey;
    }

    /// <summary>
    ///     Reads an obj_key_id from raw data and converts it to a LargeKey representing the stat-data key.
    ///     extract_key_from_id() in the kernel simply memcpys the id into the key.
    /// </summary>
    LargeKey ReadObjKeyIdAsKey(byte[] data, int offset)
    {
        if(_largeKeys)
        {
            // ObjKeyIdLarge: locality(8) + ordering(8) + objectid(8) = 24 bytes
            // These map directly to key el[0], el[1], el[2]; el[3] = 0
            return new LargeKey
            {
                el0 = BitConverter.ToUInt64(data, offset),
                el1 = BitConverter.ToUInt64(data, offset + 8),
                el2 = BitConverter.ToUInt64(data, offset + 16),
                el3 = 0
            };
        }

        // ObjKeyId: locality(8) + objectid(8) = 16 bytes
        // These map to key el[0], el[1]; el[2] = 0
        return new LargeKey
        {
            el0 = BitConverter.ToUInt64(data, offset),
            el1 = BitConverter.ToUInt64(data, offset + 8),
            el2 = 0,
            el3 = 0
        };
    }

    /// <summary>
    ///     Reconstructs the full directory entry key from the de_id stored in a CDE unit header
    ///     and the item key (which provides the locality). Mirrors extract_key_from_de_id().
    /// </summary>
    LargeKey ReadEntryKeyFromUnitHeader(byte[] data, int unitHeaderOffset, LargeKey itemKey)
    {
        // The de_id stores the portion of the key starting from el[1] (skipping el[0] = locality+type)
        // For large keys: de_id = ordering(8) + objectid(8) + offset(8) = 24 bytes → el[1], el[2], el[3]
        // For short keys: de_id = objectid(8) + offset(8) = 16 bytes → el[1], el[2]

        if(_largeKeys)
        {
            return new LargeKey
            {
                el0 = itemKey.el0, // locality + type from the item key
                el1 = BitConverter.ToUInt64(data, unitHeaderOffset),
                el2 = BitConverter.ToUInt64(data, unitHeaderOffset + 8),
                el3 = BitConverter.ToUInt64(data, unitHeaderOffset + 16)
            };
        }

        return new LargeKey
        {
            el0 = itemKey.el0,
            el1 = BitConverter.ToUInt64(data, unitHeaderOffset),
            el2 = BitConverter.ToUInt64(data, unitHeaderOffset + 8),
            el3 = 0
        };
    }

    /// <summary>
    ///     Extracts the file name from a directory entry.
    ///     Short names (not longer than 23 chars for large keys, 15 for short keys) are fully
    ///     encoded in the key, while long names are stored as null-terminated strings after
    ///     the obj_key_id in the entry body.
    /// </summary>
    string ExtractEntryName(byte[] data, int nameStart, int nameEnd, LargeKey entryKey)
    {
        bool isLongName = IsLongNameKey(entryKey);

        if(!isLongName)
        {
            // Short name: decode from the key
            return ExtractNameFromKey(entryKey);
        }

        // Long name: read null-terminated string from body
        if(nameStart >= nameEnd || nameStart >= data.Length) return null;

        int end = Math.Min(nameEnd, data.Length);
        var len = 0;

        for(int j = nameStart; j < end; j++)
        {
            if(data[j] == 0) break;

            len++;
        }

        return len > 0 ? _encoding.GetString(data, nameStart, len) : null;
    }

    /// <summary>
    ///     Returns true if the directory entry key indicates a long name (H bit set).
    /// </summary>
    bool IsLongNameKey(LargeKey key) =>

        // H bit is in el[1] (ordering for large keys, objectid for short keys)
        (key.el1 & LONGNAME_MARK) != 0;

    /// <summary>
    ///     Extracts a short file name encoded in the key.
    ///     Mirrors extract_name_from_key() in kassign.c.
    /// </summary>
    string ExtractNameFromKey(LargeKey key)
    {
        var buf = new char[32];
        var pos = 0;

        if(_largeKeys)
        {
            // ordering field (el[1]): strip fibration mask, then unpack 7 chars from bits [55:0]
            ulong ordering = key.el1 & ~FIBRATION_MASK;

            pos = UnpackString(ordering, buf, pos);

            // objectid field (el[2] full): unpack 8 chars
            pos = UnpackString(key.el2, buf, pos);

            // offset field (el[3]): unpack 8 chars
            pos = UnpackString(key.el3, buf, pos);
        }
        else
        {
            // objectid field (el[1]): strip fibration mask, unpack 7 chars from bits [55:0]
            ulong objectid = key.el1 & ~FIBRATION_MASK;

            pos = UnpackString(objectid, buf, pos);

            // offset field (el[2]): unpack 8 chars
            pos = UnpackString(key.el2, buf, pos);
        }

        return pos > 0 ? new string(buf, 0, pos) : ".";
    }

    /// <summary>
    ///     Unpacks characters from a 64-bit value, mirroring reiser4_unpack_string().
    ///     Characters are stored from the highest byte to the lowest, MSB-first.
    /// </summary>
    static int UnpackString(ulong value, char[] buf, int startPos)
    {
        int pos = startPos;

        while(value != 0)
        {
            var ch = (char)(value >> 56);

            if(ch != 0) buf[pos++] = ch;

            value <<= 8;
        }

        return pos;
    }
}