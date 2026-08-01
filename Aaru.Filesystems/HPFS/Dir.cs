// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
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
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class HPFS
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if(!normalizedPath.StartsWith("/", StringComparison.Ordinal)) normalizedPath = "/" + normalizedPath;

        // Root directory case
        if(normalizedPath == "/")
        {
            if(_rootDirectoryCache == null) return ErrorNumber.InvalidArgument;

            // Get sorted list of filenames with their fnodes

            node = new HpfsDirNode
            {
                Path     = "/",
                Position = 0,
                Entries = _rootDirectoryCache.OrderBy(static k => k.Key)
                                             .Select(static kvp => (kvp.Key, kvp.Value))
                                             .ToArray()
            };

            return ErrorNumber.NoError;
        }

        // Parse path components
        string cutPath = normalizedPath[1..]; // Remove leading '/'

        string[] pieces = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, uint> currentDirectory = _rootDirectoryCache;

        // Traverse through path components
        for(var p = 0; p < pieces.Length; p++)
        {
            string component = pieces[p].ToUpperInvariant();

            // Look for the component in current directory
            if(!currentDirectory.TryGetValue(component, out uint fnode)) return ErrorNumber.NoSuchFile;

            // Read fnode to check if it's a directory
            ErrorNumber errno = ReadFNode(fnode, out FNode fnodeStruct);

            if(errno != ErrorNumber.NoError) return errno;

            if(!fnodeStruct.IsDirectory) return ErrorNumber.NotDirectory;

            // Read directory entries from the fnode
            errno = GetDirectoryEntries(fnode, out Dictionary<string, uint> dirEntries);

            if(errno != ErrorNumber.NoError) return errno;

            // If this is the last component, we're opening this directory
            if(p == pieces.Length - 1)
            {
                // Get sorted list of filenames with their fnodes

                node = new HpfsDirNode
                {
                    Path = normalizedPath,
                    Position = 0,
                    Entries = dirEntries.OrderBy(static k => k.Key).Select(static kvp => (kvp.Key, kvp.Value)).ToArray()
                };

                return ErrorNumber.NoError;
            }

            // Not the last component - move to next level
            currentDirectory = dirEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not HpfsDirNode mynode) return ErrorNumber.InvalidArgument;

        mynode.Position = -1;
        mynode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not HpfsDirNode mynode) return ErrorNumber.InvalidArgument;

        if(mynode.Position < 0) return ErrorNumber.InvalidArgument;

        // End of directory
        if(mynode.Position >= mynode.Entries.Length) return ErrorNumber.NoError;

        // Get current filename and advance position
        filename = mynode.Entries[mynode.Position++].Filename;

        return ErrorNumber.NoError;
    }

    /// <summary>Gets a directory entry by path.</summary>
    /// <param name="path">Normalized path to the file/directory.</param>
    /// <param name="entry">The directory entry found.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber GetDirectoryEntry(string path, out DirectoryEntry entry)
    {
        entry = default(DirectoryEntry);

        // Remove leading slash and split path
        string   cutPath = path[1..];
        string[] pieces  = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root dnode
        uint currentDnode = _rootDnode;

        for(var i = 0; i < pieces.Length; i++)
        {
            string component = pieces[i];

            // Search for the component in current dnode
            ErrorNumber errno = FindEntryInDnode(currentDnode, component, out DirectoryEntry foundEntry);

            if(errno != ErrorNumber.NoError) return errno;

            // If this is the last component, return it
            if(i == pieces.Length - 1)
            {
                entry = foundEntry;

                return ErrorNumber.NoError;
            }

            // Not the last component - must be a directory
            if(!foundEntry.attributes.HasFlag(DosAttributes.Directory)) return ErrorNumber.NotDirectory;

            // Get the dnode for the subdirectory
            errno = ReadFNode(foundEntry.fnode, out FNode subFnode);

            if(errno != ErrorNumber.NoError) return errno;

            if(!subFnode.IsDirectory) return ErrorNumber.NotDirectory;

            // For directories, read dnode pointer directly from btree_data
            // (disk_secno is at offset 8 in the first leaf node)
            if(subFnode.btree_data == null || subFnode.btree_data.Length < 12) return ErrorNumber.InvalidArgument;

            currentDnode = BitConverter.ToUInt32(subFnode.btree_data, 8);

            if(currentDnode == 0) return ErrorNumber.InvalidArgument;
        }

        return ErrorNumber.NoSuchFile;
    }
}