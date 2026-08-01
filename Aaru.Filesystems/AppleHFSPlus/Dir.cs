// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System Plus plugin.
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
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Aaru.Filesystems;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
/// <summary>Implements detection of Apple Hierarchical File System Plus (HFS+)</summary>
public sealed partial class AppleHFSPlus
{
    /// <inheritdoc />
    /// <summary>
    ///     Opens a directory for enumeration.
    ///     Supports full path traversal including subdirectories.
    /// </summary>
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
            contents.Remove(HFS_PLUS_PRIVATE_DATA_DIR);
            contents.Sort();

            // Convert internal representation (slashes) to Mac OS display format (colons)
            var displayContents                                        = new string[contents.Count];
            for(var i = 0; i < contents.Count; i++) displayContents[i] = contents[i].Replace("/", ":");

            node = new HfsPlusDirNode
            {
                Path          = "/",
                Position      = 0,
                Contents      = displayContents,
                DirectoryCNID = kHFSRootFolderID
            };

            return ErrorNumber.NoError;
        }

        // Parse path components using forward slash (/) as the ONLY path separator
        // In HFS+, colons (:) are NOT path separators - they are literal characters in filenames
        // Mac OS X displays forward slashes in filenames as colons in the UI, but internally
        // the path separator is always the forward slash
        string cutPath = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                             ? normalizedPath[1..]
                             : normalizedPath;

        string[] pieces = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, CatalogEntry> currentDirectory = _rootDirectoryCache;
        uint                             currentDirCNID   = kHFSRootFolderID;

        // Traverse through path components
        for(var p = 0; p < pieces.Length; p++)
        {
            string component = pieces[p];

            // Convert colons back to slashes for internal catalog lookup
            // Users provide paths with colons (Mac OS display format: "file:with:slashes")
            // But the catalog stores them with slashes (internal format: "file/with/slashes")
            component = component.Replace(":", "/");

            // Look for the component in current directory using the volume's comparison rules
            CatalogEntry foundEntry = null;

            currentDirectory?.TryGetValue(component, out foundEntry);

            if(foundEntry == null) return ErrorNumber.NoSuchFile;

            // Check if it's a directory
            if(foundEntry.Type != (int)BTreeRecordType.kHFSPlusFolderRecord) return ErrorNumber.NotDirectory;

            // Update current directory info
            currentDirCNID = foundEntry.CNID;

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

                // Convert internal representation (slashes) to Mac OS display format (colons)
                var displayContents                                        = new string[contents.Count];
                for(var i = 0; i < contents.Count; i++) displayContents[i] = contents[i].Replace("/", ":");

                node = new HfsPlusDirNode
                {
                    Path          = normalizedPath,
                    Position      = 0,
                    Contents      = displayContents,
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

    /// <inheritdoc />
    /// <summary>
    ///     Closes a directory node and releases resources.
    /// </summary>
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not HfsPlusDirNode mynode) return ErrorNumber.InvalidArgument;

        mynode.Position = -1;
        mynode.Contents = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Reads the next filename from an open directory.
    ///     Returns NoError on success with filename set, or NoError with null filename at end of directory.
    /// </summary>
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not HfsPlusDirNode mynode) return ErrorNumber.InvalidArgument;

        if(mynode.Position < 0) return ErrorNumber.InvalidArgument;

        // End of directory
        if(mynode.Position >= mynode.Contents.Length) return ErrorNumber.NoError;

        // Get current filename and advance position
        // Filenames are already in Mac OS display format (colons instead of slashes)
        filename = mynode.Contents[mynode.Position++];

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Caches directory entries for a given CNID if not already cached.
    ///     Reuses existing root directory cache if CNID is kHFSRootFolderID.
    /// </summary>
    /// <param name="cnid">Catalog Node ID of the directory to cache</param>
    /// <returns>Error status</returns>
    ErrorNumber CacheDirectoryIfNeeded(uint cnid)
    {
        // Initialize directory cache dictionary if needed
        _directoryCaches ??= new Dictionary<uint, Dictionary<string, CatalogEntry>>();

        // Root directory is already cached in _rootDirectoryCache
        if(cnid == kHFSRootFolderID) return ErrorNumber.NoError;

        // Check if already cached
        if(_directoryCaches.ContainsKey(cnid)) return ErrorNumber.NoError;

        // Cache this directory
        return CacheDirectory(cnid);
    }

    /// <summary>
    ///     Caches all entries for a directory by its CNID.
    ///     Searches the catalog B-Tree for all records with the given parentID.
    /// </summary>
    /// <param name="cnid">Catalog Node ID (used as parentID for records) of the directory to cache</param>
    /// <returns>Error status</returns>
    ErrorNumber CacheDirectory(uint cnid)
    {
        _directoryCaches ??= new Dictionary<uint, Dictionary<string, CatalogEntry>>();

        var directoryEntries = new Dictionary<string, CatalogEntry>(_nameComparer);

        // Keyed range scan: descend the catalog B-tree to the first record of this directory and
        // walk forward until the parent CNID changes
        ErrorNumber errno = ScanDirectoryRecords(cnid, directoryEntries);

        if(errno != ErrorNumber.NoError) return errno;

        _directoryCaches[cnid] = directoryEntries;

        AaruLogging.Debug(MODULE_NAME, $"Cached directory CNID={cnid} with {directoryEntries.Count} entries");

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Gets cached directory entries for a given CNID.
    ///     Returns the root directory cache if CNID is kHFSRootFolderID.
    /// </summary>
    /// <param name="cnid">Catalog Node ID of the directory</param>
    /// <returns>Dictionary of entries keyed by filename, or null if not cached</returns>
    Dictionary<string, CatalogEntry> GetDirectoryEntries(uint cnid)
    {
        if(cnid == kHFSRootFolderID) return _rootDirectoryCache;

        if(_directoryCaches == null) return null;

        _directoryCaches.TryGetValue(cnid, out Dictionary<string, CatalogEntry> entries);

        return entries;
    }
}