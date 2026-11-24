// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Amiga Fast File System plugin.
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
public sealed partial class AmigaDOSPlugin
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

            node = new AmigaDOSDirNode
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
        Dictionary<string, uint> currentEntries = _rootDirectoryCache;

        // Traverse each path component
        foreach(string component in pathComponents)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenDir: Navigating to component '{0}'", component);

            // Find the component in current directory (case-insensitive for international mode)
            string foundKey = null;

            foreach(string key in currentEntries.Keys)
            {
                if(string.Equals(key,
                                 component,
                                 _isIntl ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    foundKey = key;

                    break;
                }
            }

            if(foundKey == null)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Component '{0}' not found in directory", component);

                return ErrorNumber.NoSuchFile;
            }

            uint childBlock = currentEntries[foundKey];

            AaruLogging.Debug(MODULE_NAME, "OpenDir: Component '{0}' found at block {1}", component, childBlock);

            // Read the child block
            ErrorNumber errno = ReadBlock(childBlock, out byte[] blockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Error reading block {0}: {1}", childBlock, errno);

                return errno;
            }

            // Validate block type
            var type = BigEndianBitConverter.ToUInt32(blockData, 0x00);

            if(type != TYPE_HEADER)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Invalid block type at {0}: {1}", childBlock, type);

                return ErrorNumber.InvalidArgument;
            }

            // Get secondary type (at end of block)
            int secTypeOffset = blockData.Length - 4;
            var secType       = BigEndianBitConverter.ToUInt32(blockData, secTypeOffset);

            // Check if it's a directory (ST_DIR = 2, ST_ROOT = 1)
            if(secType != SUBTYPE_DIR && secType != SUBTYPE_ROOT)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "OpenDir: Block {0} is not a directory (secType={1})",
                                  childBlock,
                                  secType);

                return ErrorNumber.NotDirectory;
            }

            // Parse directory entries from hash table
            errno = ReadDirectoryEntries(blockData, out currentEntries);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Error reading directory entries: {0}", errno);

                return errno;
            }
        }

        // Create directory node with the entries we found
        node = new AmigaDOSDirNode
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
        if(node is not AmigaDOSDirNode amigaNode) return ErrorNumber.InvalidArgument;

        amigaNode.Position = -1;
        amigaNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not AmigaDOSDirNode amigaNode) return ErrorNumber.InvalidArgument;

        if(amigaNode.Position < 0) return ErrorNumber.InvalidArgument;

        if(amigaNode.Position >= amigaNode.Entries.Length) return ErrorNumber.NoError;

        filename = amigaNode.Entries[amigaNode.Position++];

        return ErrorNumber.NoError;
    }

    /// <summary>Reads directory entries from a directory block's hash table</summary>
    /// <param name="blockData">The directory block data</param>
    /// <param name="entries">Output dictionary of entries (name -> block number)</param>
    /// <returns>Error code</returns>
    ErrorNumber ReadDirectoryEntries(byte[] blockData, out Dictionary<string, uint> entries)
    {
        entries = new Dictionary<string, uint>();

        // Hash table size is at offset 0x0C (in longs)
        var hashTableSize = BigEndianBitConverter.ToUInt32(blockData, 0x0C);

        // For directories, hash table size may be 0, use block size calculation
        if(hashTableSize == 0) hashTableSize = (uint)(blockData.Length / 4 - 56);

        // Hash table starts at offset 0x18 (6 longs from start)
        int hashTableOffset = 6 * 4;

        for(var i = 0; i < hashTableSize; i++)
        {
            var blockPtr = BigEndianBitConverter.ToUInt32(blockData, hashTableOffset + i * 4);

            if(blockPtr == 0) continue;

            // Follow the hash chain
            while(blockPtr != 0)
            {
                ErrorNumber errno = ReadBlock(blockPtr, out byte[] entryBlockData);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "ReadDirectoryEntries: Error reading block {0}: {1}",
                                      blockPtr,
                                      errno);

                    return errno;
                }

                // Validate block type
                var type = BigEndianBitConverter.ToUInt32(entryBlockData, 0x00);

                if(type != TYPE_HEADER)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "ReadDirectoryEntries: Invalid block type at {0}: {1}",
                                      blockPtr,
                                      type);

                    break;
                }

                string name = GetEntryName(entryBlockData);

                if(!string.IsNullOrEmpty(name) && !entries.ContainsKey(name)) entries[name] = blockPtr;

                // Get next in hash chain
                int nextHashOffset = entryBlockData.Length - 4 * 4;
                blockPtr = BigEndianBitConverter.ToUInt32(entryBlockData, nextHashOffset);
            }
        }

        return ErrorNumber.NoError;
    }
}