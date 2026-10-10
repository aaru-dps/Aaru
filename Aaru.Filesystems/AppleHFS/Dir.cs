// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
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

// ReSharper disable UnusedType.Local

// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBePrivate.Local

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
public sealed partial class AppleHFS
{
    /// <summary>
    ///     Opens a directory for enumeration.
    ///     Supports full path traversal including subdirectories.
    /// </summary>
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        // Root directory case
        if(string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
        {
            if(_rootDirectoryCache == null) return ErrorNumber.InvalidArgument;

            var contents = _rootDirectoryCache.Keys.ToList();
            contents.Sort();

            node = new HfsDirNode
            {
                Path          = "/",
                Position      = 0,
                Contents      = contents.ToArray(),
                DirectoryCNID = kRootCnid
            };

            return ErrorNumber.NoError;
        }

        // Parse path components
        string cutPath = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                             ? normalizedPath[1..]
                             : normalizedPath;

        string[] pieces = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, CatalogEntry> currentDirectory = _rootDirectoryCache;
        uint                             currentDirCNID   = kRootCnid;
        var                              currentPath      = "";

        // Traverse through path components
        for(var p = 0; p < pieces.Length; p++)
        {
            string component = pieces[p];

            // Convert colons to slashes in component (readdir returns display names with colons)
            component = component.Replace(":", "/");

            // Look for the component in current directory
            KeyValuePair<string, CatalogEntry> foundEntry = default;
            var                                found      = false;

            if(currentDirectory != null)
            {
                foreach(KeyValuePair<string, CatalogEntry> entry in currentDirectory)
                {
                    if(string.Equals(entry.Key, component, StringComparison.OrdinalIgnoreCase))
                    {
                        foundEntry = entry;
                        found      = true;

                        break;
                    }
                }
            }

            if(!found) return ErrorNumber.NoSuchFile;

            CatalogEntry catalogEntry = foundEntry.Value;

            // Check if it's a directory
            if(catalogEntry.Type != kCatalogRecordTypeDirectory) return ErrorNumber.NotDirectory;

            // Update current directory info
            currentDirCNID = catalogEntry.CNID;
            currentPath    = p == 0 ? pieces[0] : $"{currentPath}/{pieces[p]}";

            // If this is the last component, we're opening this directory
            if(p == pieces.Length - 1)
            {
                // Try to get cached entries for this directory
                ErrorNumber cacheErr = CacheDirectoryIfNeeded(currentDirCNID);

                if(cacheErr != ErrorNumber.NoError) return cacheErr;

                // Get entries for this directory from cache
                Dictionary<string, CatalogEntry> dirEntries = GetDirectoryEntries(currentDirCNID);

                if(dirEntries == null) return ErrorNumber.InvalidArgument;

                var contents = dirEntries.Keys.ToList();
                contents.Sort();

                node = new HfsDirNode
                {
                    Path          = normalizedPath,
                    Position      = 0,
                    Contents      = contents.ToArray(),
                    DirectoryCNID = currentDirCNID
                };

                return ErrorNumber.NoError;
            }

            // Not the last component - need to load next level
            ErrorNumber cacheNextErr = CacheDirectoryIfNeeded(currentDirCNID);

            if(cacheNextErr != ErrorNumber.NoError) return cacheNextErr;

            currentDirectory = GetDirectoryEntries(currentDirCNID);

            if(currentDirectory == null) return ErrorNumber.NoSuchFile;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>
    ///     Closes a directory node and releases resources.
    /// </summary>
    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not HfsDirNode mynode) return ErrorNumber.InvalidArgument;

        mynode.Position = -1;
        mynode.Contents = null;

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Reads the next filename from an open directory.
    ///     Returns NoError on success with filename set, or NoError with null filename at end of directory.
    /// </summary>
    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not HfsDirNode mynode) return ErrorNumber.InvalidArgument;

        if(mynode.Position < 0) return ErrorNumber.InvalidArgument;

        // End of directory
        if(mynode.Position >= mynode.Contents.Length) return ErrorNumber.NoError;

        // Get current filename and advance position
        filename = mynode.Contents[mynode.Position++];

        // In HFS, the colon (:) is the path separator, not the forward slash (/)
        // Convert forward slashes in filenames to colons for proper Mac OS representation
        filename = filename.Replace("/", ":");

        return ErrorNumber.NoError;
    }
}