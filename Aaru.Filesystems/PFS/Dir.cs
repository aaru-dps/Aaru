// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Professional File System plugin.
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

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class PFS
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize the path
        string normalizedPath = path ?? "/";

        if(normalizedPath == "" || normalizedPath == ".") normalizedPath = "/";

        // Root directory - return cached entries
        if(normalizedPath == "/" || string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
        {
            if(_rootDirectoryCache.Count == 0) return ErrorNumber.NoSuchFile;

            node = new PFSDirNode
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

        AaruLogging.Debug(MODULE_NAME, "OpenDir: Traversing path with {0} components", pathComponents.Length);

        // Start from root directory cache
        Dictionary<string, DirEntryCacheItem> currentEntries = _rootDirectoryCache;

        // Traverse each path component
        foreach(string component in pathComponents)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenDir: Navigating to component '{0}'", component);

            // Find the component in current directory (case-insensitive via dictionary comparer)
            if(!currentEntries.TryGetValue(component, out DirEntryCacheItem entry))
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Component '{0}' not found in directory", component);

                return ErrorNumber.NoSuchFile;
            }

            AaruLogging.Debug(MODULE_NAME,
                              "OpenDir: Component '{0}' found with anode {1}, type {2}",
                              component,
                              entry.Anode,
                              entry.Type);

            // Check if it's a directory
            if(entry.Type != EntryType.Directory && entry.Type != EntryType.HardLinkDir)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: '{0}' is not a directory (type={1})", component, entry.Type);

                return ErrorNumber.NotDirectory;
            }

            // Read directory contents
            ErrorNumber errno = GetDirectoryContents(entry.Anode, out currentEntries);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Error reading directory blocks: {0}", errno);

                return errno;
            }
        }

        // Create directory node with the entries we found
        node = new PFSDirNode
        {
            Path     = normalizedPath,
            Position = 0,
            Entries  = currentEntries.Keys.ToArray()
        };

        AaruLogging.Debug(MODULE_NAME,
                          "OpenDir: Successfully opened directory '{0}' with {1} entries",
                          normalizedPath,
                          currentEntries.Count);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not PFSDirNode pfsNode) return ErrorNumber.InvalidArgument;

        pfsNode.Position = -1;
        pfsNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not PFSDirNode pfsNode) return ErrorNumber.InvalidArgument;

        if(pfsNode.Position < 0) return ErrorNumber.InvalidArgument;

        if(pfsNode.Position >= pfsNode.Entries.Length) return ErrorNumber.NoError;

        filename = pfsNode.Entries[pfsNode.Position++];

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the contents of a directory, caching them as the filesystem is read-only</summary>
    /// <param name="anodeNumber">Anode number of the directory</param>
    /// <param name="entries">Dictionary of filename -> entry</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber GetDirectoryContents(uint anodeNumber, out Dictionary<string, DirEntryCacheItem> entries)
    {
        if(_directoryCache.TryGetValue(anodeNumber, out entries)) return ErrorNumber.NoError;

        ErrorNumber errno = GetAnode(anodeNumber, out Anode dirAnode);

        if(errno != ErrorNumber.NoError) return errno;

        entries = new Dictionary<string, DirEntryCacheItem>(StringComparer.OrdinalIgnoreCase);
        errno   = ReadDirectoryBlocks(dirAnode, entries);

        if(errno == ErrorNumber.NoError) _directoryCache[anodeNumber] = entries;

        return errno;
    }

    /// <summary>Reads directory blocks following an anode chain and caches entries</summary>
    /// <param name="startAnode">The starting anode</param>
    /// <param name="cache">The cache dictionary to populate</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber ReadDirectoryBlocks(Anode startAnode, Dictionary<string, DirEntryCacheItem> cache)
    {
        Anode currentAnode = startAnode;

        while(true)
        {
            // Read blocks in this anode's cluster
            for(uint i = 0; i < currentAnode.clustersize; i++)
            {
                uint blockNr = currentAnode.blocknr + i;

                ErrorNumber errno = ReadBlock(blockNr, out byte[] blockData);

                if(errno != ErrorNumber.NoError) return errno;

                // Parse directory block
                var blockId = BigEndianBitConverter.ToUInt16(blockData, 0);

                if(blockId != DBLKID)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "Invalid directory block ID: 0x{0:X4} at block {1}",
                                      blockId,
                                      blockNr);

                    continue;
                }

                // Parse directory entries
                ParseDirectoryBlock(blockData, cache);
            }

            // Move to next anode in chain
            if(currentAnode.next == ANODE_EOF) break;

            ErrorNumber err = GetAnode(currentAnode.next, out currentAnode);

            if(err != ErrorNumber.NoError) return err;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Parses a directory block and adds entries to the cache</summary>
    /// <param name="blockData">The directory block data</param>
    /// <param name="cache">The cache dictionary to populate</param>
    void ParseDirectoryBlock(byte[] blockData, Dictionary<string, DirEntryCacheItem> cache)
    {
        // Directory block header: id(2) + notused(2) + datestamp(4) + notused2(4) + anodenr(4) + parent(4) = 20 bytes
        var offset = 20;

        while(offset < blockData.Length)
        {
            // Check if we've reached the end of entries
            if(blockData[offset] == 0) break;

            byte entrySize = blockData[offset];

            if(entrySize < 22 || offset + entrySize > blockData.Length) break;

            // Parse entry
            var  type           = (EntryType)(sbyte)blockData[offset + 1];
            var  anode          = BigEndianBitConverter.ToUInt32(blockData, offset + 2);
            var  fsize          = BigEndianBitConverter.ToUInt32(blockData, offset + 6);
            var  creationday    = BigEndianBitConverter.ToUInt16(blockData, offset + 10);
            var  creationminute = BigEndianBitConverter.ToUInt16(blockData, offset + 12);
            var  creationtick   = BigEndianBitConverter.ToUInt16(blockData, offset + 14);
            var  protection     = (ProtectionBits)blockData[offset + 16];
            byte nlength        = blockData[offset + 17];

            if(nlength == 0 || offset + 18 + nlength > blockData.Length)
            {
                offset += entrySize;

                continue;
            }

            string filename = _encoding.GetString(blockData, offset + 18, nlength);

            // Create cache item
            var item = new DirEntryCacheItem
            {
                Anode          = anode,
                Type           = type,
                Size           = fsize,
                Protection     = protection,
                CreationDay    = creationday,
                CreationMinute = creationminute,
                CreationTick   = creationtick
            };

            // Check for extra fields (comment follows filename)
            int commentOffset = offset + 18 + nlength;

            if(commentOffset < offset + entrySize && blockData[commentOffset] > 0)
            {
                int commentLength = blockData[commentOffset];

                if(commentOffset + 1 + commentLength <= offset + entrySize)
                    item.Comment = _encoding.GetString(blockData, commentOffset + 1, commentLength);
            }

            // Check for extended file size in dir extension mode
            if(_modeFlags.HasFlag(ModeFlags.LargeFile) && _modeFlags.HasFlag(ModeFlags.DirExtension))
            {
                // ExtraFields may follow the comment - we'd need to parse them for fsizex
                // For now, just use the 32-bit size
            }

            if(!string.IsNullOrEmpty(filename) && !cache.ContainsKey(filename)) cache[filename] = item;

            offset += entrySize;
        }
    }
}