// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BTree.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : OS/2 High Performance File System plugin.
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
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class HPFS
{
    /// <summary>Reads an fnode from disk and optionally caches it.</summary>
    /// <param name="sector">Sector number of the fnode.</param>
    /// <param name="fnode">The fnode structure read.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadFNode(uint sector, out FNode fnode)
    {
        fnode = default(FNode);

        if(_fnodeCache.TryGetValue(sector, out fnode)) return ErrorNumber.NoError;

        ErrorNumber errno = _image.ReadSector(_partition.Start + sector, false, out byte[] fnodeSector, out _);

        if(errno != ErrorNumber.NoError) return errno;

        fnode = Marshal.ByteArrayToStructureLittleEndian<FNode>(fnodeSector);

        if(fnode.magic != FNODE_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid fnode magic at sector {0}: 0x{1:X8} (expected 0x{2:X8})",
                              sector,
                              fnode.magic,
                              FNODE_MAGIC);

            return ErrorNumber.InvalidArgument;
        }

        _fnodeCache[sector] = fnode;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a dnode from disk (4 consecutive sectors) and optionally caches it.</summary>
    /// <param name="sector">Starting sector number of the dnode.</param>
    /// <param name="dnode">The dnode structure read.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadDNode(uint sector, out DNode dnode)
    {
        dnode = default(DNode);

        if(_dnodeCache.TryGetValue(sector, out dnode)) return ErrorNumber.NoError;

        // DNodes are 4 sectors (2048 bytes) long
        ErrorNumber errno = _image.ReadSectors(_partition.Start + sector, false, 4, out byte[] dnodeSectors, out _);

        if(errno != ErrorNumber.NoError) return errno;

        dnode = Marshal.ByteArrayToStructureLittleEndian<DNode>(dnodeSectors);

        if(dnode.magic != DNODE_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid dnode magic at sector {0}: 0x{1:X8} (expected 0x{2:X8})",
                              sector,
                              dnode.magic,
                              DNODE_MAGIC);

            return ErrorNumber.InvalidArgument;
        }

        // Validate self-pointer
        if(dnode.self != sector)
        {
            AaruLogging.Debug(MODULE_NAME, "DNode self-pointer mismatch at sector {0}: self={1}", sector, dnode.self);

            return ErrorNumber.InvalidArgument;
        }

        _dnodeCache[sector] = dnode;

        return ErrorNumber.NoError;
    }

    /// <summary>Extracts B+ tree leaf nodes from an fnode's btree structure.</summary>
    /// <param name="header">B+ tree header.</param>
    /// <param name="data">B+ tree data bytes.</param>
    /// <returns>Array of leaf nodes.</returns>
    static BPlusLeafNode[] GetBPlusLeafNodes(BPlusHeader header, byte[] data)
    {
        if(header.IsInternal || data == null || header.n_used_nodes == 0) return [];

        var nodes = new BPlusLeafNode[header.n_used_nodes];

        for(var i = 0; i < header.n_used_nodes; i++)
        {
            int offset = i * 12; // Each leaf node is 12 bytes

            if(offset + 12 > data.Length) break;

            nodes[i] = Marshal.ByteArrayToStructureLittleEndian<BPlusLeafNode>(data, offset, 12);
        }

        return nodes;
    }

    /// <summary>Recursively caches directory entries from a dnode and its children.</summary>
    /// <param name="dnode">The dnode to process.</param>
    /// <param name="cache">Dictionary to cache entries into (filename to fnode mapping).</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber CacheDNodeEntries(DNode dnode, Dictionary<string, uint> cache)
    {
        // Parse directory entries from the dnode
        var offset    = 0;
        int endOffset = (int)dnode.first_free - 20; // first_free is offset from start of dnode

        while(offset < endOffset && offset + 31 < dnode.dirent.Length)
        {
            // Read directory entry header
            DirectoryEntry entry = Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(dnode.dirent, offset, 31);

            // Check for end of entries
            if(entry.length is < 32 or > 2048) break;

            // Skip special entries (first entry "." and last entry with 0xFF name)
            if(!entry.flags.HasFlag(DirectoryEntryFlags.First) && !entry.flags.HasFlag(DirectoryEntryFlags.Last))
            {
                // Extract filename
                if(entry.namelen > 0 && offset + 31 + entry.namelen <= dnode.dirent.Length)
                {
                    var nameBytes = new byte[entry.namelen];
                    Array.Copy(dnode.dirent, offset + 31, nameBytes, 0, entry.namelen);
                    string filename = _encoding.GetString(nameBytes);

                    // Cache the entry (case-insensitive key)
                    cache[filename.ToUpperInvariant()] = entry.fnode;

                    AaruLogging.Debug(MODULE_NAME, "Cached entry: {0} -> fnode {1}", filename, entry.fnode);
                }
            }

            // Check if there's a down pointer (B-tree child)
            if(entry.flags.HasFlag(DirectoryEntryFlags.Down))
            {
                // Down pointer is at the end of the entry, 4 bytes before next entry
                int downPtrOffset = offset + entry.length - 4;

                if(downPtrOffset + 4 <= dnode.dirent.Length)
                {
                    var downDnode = BitConverter.ToUInt32(dnode.dirent, downPtrOffset);

                    // Recursively process child dnode
                    ErrorNumber errno = ReadDNode(downDnode, out DNode childDnode);

                    if(errno == ErrorNumber.NoError) CacheDNodeEntries(childDnode, cache);
                }
            }

            // Move to next entry
            offset += entry.length;

            // Handle last entry
            if(entry.flags.HasFlag(DirectoryEntryFlags.Last)) break;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Gets directory entries from a directory fnode, caching them as the filesystem is read-only.</summary>
    /// <param name="fnode">Fnode sector number of the directory.</param>
    /// <param name="entries">Dictionary of filename to fnode sector.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber GetDirectoryEntries(uint fnode, out Dictionary<string, uint> entries)
    {
        if(_directoryCache.TryGetValue(fnode, out entries)) return ErrorNumber.NoError;

        ErrorNumber errno = ReadDirectoryEntries(fnode, out entries);

        if(errno == ErrorNumber.NoError) _directoryCache[fnode] = entries;

        return errno;
    }

    /// <summary>Reads directory entries from a directory fnode.</summary>
    /// <param name="fnode">Fnode sector number of the directory.</param>
    /// <param name="entries">Dictionary of filename to fnode sector.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadDirectoryEntries(uint fnode, out Dictionary<string, uint> entries)
    {
        entries = new Dictionary<string, uint>();

        // Read the fnode
        ErrorNumber errno = ReadFNode(fnode, out FNode fnodeStruct);

        if(errno != ErrorNumber.NoError) return errno;

        // Validate it's a directory
        if(!fnodeStruct.IsDirectory)
        {
            AaruLogging.Debug(MODULE_NAME, "Fnode {0} is not a directory", fnode);

            return ErrorNumber.NotDirectory;
        }

        // For directories, the first extent in the btree_data directly points to the root dnode.
        // The Linux kernel accesses this as fnode->u.external[0].disk_secno without checking
        // the btree flags, because directory fnodes always use this format.
        if(fnodeStruct.btree_data == null || fnodeStruct.btree_data.Length < 12)
        {
            AaruLogging.Debug(MODULE_NAME, "Directory fnode {0} has no btree data", fnode);

            return ErrorNumber.InvalidArgument;
        }

        // Read the first leaf node directly (offset 8 is disk_secno in the leaf node structure)
        var dnodeSector = BitConverter.ToUInt32(fnodeStruct.btree_data, 8);

        if(dnodeSector == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Directory fnode {0} has null dnode pointer", fnode);

            return ErrorNumber.InvalidArgument;
        }

        // Read the root dnode
        errno = ReadDNode(dnodeSector, out DNode rootDnode);

        if(errno != ErrorNumber.NoError) return errno;

        // Cache all entries from the directory tree
        return CacheDNodeEntries(rootDnode, entries);
    }

    /// <summary>Searches a dnode tree for an entry by name.</summary>
    /// <param name="dnodeSector">Starting dnode sector.</param>
    /// <param name="name">Name to search for (case-insensitive).</param>
    /// <param name="entry">The found directory entry.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber FindEntryInDnode(uint dnodeSector, string name, out DirectoryEntry entry)
    {
        entry = default(DirectoryEntry);

        ErrorNumber errno = ReadDNode(dnodeSector, out DNode dnode);

        if(errno != ErrorNumber.NoError) return errno;

        string nameUpper = name.ToUpperInvariant();

        // Parse directory entries
        var offset    = 0;
        int endOffset = (int)dnode.first_free - 20;

        while(offset < endOffset && offset + 31 < dnode.dirent.Length)
        {
            DirectoryEntry currentEntry =
                Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(dnode.dirent, offset, 31);

            if(currentEntry.length is < 32 or > 2048) break;

            // Skip special entries
            if(!currentEntry.flags.HasFlag(DirectoryEntryFlags.First) &&
               !currentEntry.flags.HasFlag(DirectoryEntryFlags.Last))
            {
                if(currentEntry.namelen > 0 && offset + 31 + currentEntry.namelen <= dnode.dirent.Length)
                {
                    var nameBytes = new byte[currentEntry.namelen];
                    Array.Copy(dnode.dirent, offset + 31, nameBytes, 0, currentEntry.namelen);
                    string entryName = _encoding.GetString(nameBytes);

                    // Compare names (case-insensitive)
                    int cmp = string.Compare(nameUpper, entryName.ToUpperInvariant(), StringComparison.Ordinal);

                    if(cmp == 0)
                    {
                        entry = currentEntry;

                        return ErrorNumber.NoError;
                    }

                    // B-tree ordering: if our name is less than this entry's name,
                    // and there's a down pointer, search the child
                    if(cmp < 0 && currentEntry.flags.HasFlag(DirectoryEntryFlags.Down))
                    {
                        int downPtrOffset = offset + currentEntry.length - 4;

                        if(downPtrOffset + 4 <= dnode.dirent.Length)
                        {
                            var downDnode = BitConverter.ToUInt32(dnode.dirent, downPtrOffset);

                            return FindEntryInDnode(downDnode, name, out entry);
                        }
                    }
                }
            }

            // Check down pointer for last entry or if we need to go down
            if(currentEntry.flags.HasFlag(DirectoryEntryFlags.Down))
            {
                int downPtrOffset = offset + currentEntry.length - 4;

                if(downPtrOffset + 4 <= dnode.dirent.Length)
                {
                    var downDnode = BitConverter.ToUInt32(dnode.dirent, downPtrOffset);

                    // If this is the last entry, search its subtree
                    if(currentEntry.flags.HasFlag(DirectoryEntryFlags.Last))
                        return FindEntryInDnode(downDnode, name, out entry);
                }
            }

            offset += currentEntry.length;

            if(currentEntry.flags.HasFlag(DirectoryEntryFlags.Last)) break;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>
    ///     Looks up a file sector in the B+ tree and returns the corresponding disk sector.
    ///     Based on hpfs_bplus_lookup from the Linux kernel.
    /// </summary>
    /// <param name="btreeHeader">B+ tree header from fnode or anode.</param>
    /// <param name="btreeData">B+ tree data (nodes) from fnode or anode.</param>
    /// <param name="fileSector">File sector number to look up.</param>
    /// <param name="diskSector">Output: corresponding disk sector number.</param>
    /// <param name="runLength">Output: number of consecutive sectors in this extent.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber BPlusLookup(BPlusHeader btreeHeader, byte[] btreeData, uint fileSector, out uint diskSector,
                            out uint    runLength)
    {
        diskSector = 0;
        runLength  = 0;

        // If this is an internal node, find the correct subtree and recurse
        if(btreeHeader.IsInternal)
        {
            // Parse internal nodes (8 bytes each: file_secno + down pointer)
            for(var i = 0; i < btreeHeader.n_used_nodes; i++)
            {
                int offset = i * 8;

                if(offset + 8 > btreeData.Length) break;

                BPlusInternalNode internalNode =
                    Marshal.ByteArrayToStructureLittleEndian<BPlusInternalNode>(btreeData, offset, 8);

                // Internal nodes store the maximum file sector for each subtree
                // If our sector is less than this node's file_secno, descend into this subtree
                if(fileSector < internalNode.file_secno)
                {
                    // Read the anode
                    ErrorNumber errno = _image.ReadSector(_partition.Start + internalNode.down,
                                                          false,
                                                          out byte[] anodeSector,
                                                          out _);

                    if(errno != ErrorNumber.NoError) return errno;

                    ANode anode = Marshal.ByteArrayToStructureLittleEndian<ANode>(anodeSector);

                    if(anode.magic != ANODE_MAGIC) return ErrorNumber.InvalidArgument;

                    // Recurse into the anode's btree
                    return BPlusLookup(anode.btree, anode.btree_data, fileSector, out diskSector, out runLength);
                }
            }

            // If we get here, the sector wasn't found in any subtree
            return ErrorNumber.InvalidArgument;
        }

        // This is a leaf node - search for the extent containing our sector
        // Parse leaf nodes (12 bytes each: file_secno + length + disk_secno)
        for(var i = 0; i < btreeHeader.n_used_nodes; i++)
        {
            int offset = i * 12;

            if(offset + 12 > btreeData.Length) break;

            BPlusLeafNode leafNode = Marshal.ByteArrayToStructureLittleEndian<BPlusLeafNode>(btreeData, offset, 12);

            // Check if this extent contains our file sector
            // file_secno is the starting file sector, length is the number of sectors
            if(fileSector >= leafNode.file_secno && fileSector < leafNode.file_secno + leafNode.length)
            {
                // Calculate the disk sector: base disk sector + offset within extent
                diskSector = leafNode.disk_secno + (fileSector - leafNode.file_secno);
                runLength  = leafNode.length     - (fileSector - leafNode.file_secno);

                return ErrorNumber.NoError;
            }
        }

        // Sector not found
        return ErrorNumber.InvalidArgument;
    }
}