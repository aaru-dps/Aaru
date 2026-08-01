// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : IBM JFS filesystem plugin
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

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of IBM's Journaled File System</summary>
public sealed partial class JFS
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize the path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory - return cached entries
        if(normalizedPath == "/")
        {
            if(_rootDirectoryCache.Count == 0) return ErrorNumber.NoSuchFile;

            node = new JfsDirNode
            {
                Path     = "/",
                Position = 0,
                Entries  = _rootDirectoryCache.Keys.ToArray()
            };

            return ErrorNumber.NoError;
        }

        // Subdirectory traversal
        // Remove leading slash and split path
        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        AaruLogging.Debug(MODULE_NAME, "OpenDir: traversing path with {0} components", pathComponents.Length);

        // Start from root directory cache
        Dictionary<string, uint> currentEntries = _rootDirectoryCache;

        // Traverse each path component
        foreach(string component in pathComponents)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenDir: navigating to component '{0}'", component);

            // Find the component in current directory
            if(!currentEntries.TryGetValue(component, out uint childInodeNumber))
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: component '{0}' not found in directory", component);

                return ErrorNumber.NoSuchFile;
            }

            // Read the child inode
            ErrorNumber errno = ReadFilesetInode(childInodeNumber, out Inode childInode);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: error reading inode {0}: {1}", childInodeNumber, errno);

                return errno;
            }

            // Check if it's a directory
            if((childInode.di_mode & 0xF000) != 0x4000)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "OpenDir: component '{0}' is not a directory (mode=0x{1:X})",
                                  component,
                                  childInode.di_mode);

                return ErrorNumber.NotDirectory;
            }

            // Parse the child directory's dtree
            errno = GetDirectoryEntries(childInodeNumber, childInode.di_u, out Dictionary<string, uint> childEntries);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: error parsing directory dtree: {0}", errno);

                return errno;
            }

            currentEntries = childEntries;
        }

        // Filter out . and ..
        string[] entries = currentEntries.Keys.Where(static k => k is not ("." or "..")).ToArray();

        node = new JfsDirNode
        {
            Path     = normalizedPath,
            Position = 0,
            Entries  = entries
        };

        AaruLogging.Debug(MODULE_NAME,
                          "OpenDir: successfully opened directory '{0}' with {1} entries",
                          normalizedPath,
                          entries.Length);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not JfsDirNode jfsDirNode) return ErrorNumber.InvalidArgument;

        jfsDirNode.Position = -1;
        jfsDirNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not JfsDirNode jfsDirNode) return ErrorNumber.InvalidArgument;

        if(jfsDirNode.Position < 0) return ErrorNumber.InvalidArgument;

        if(jfsDirNode.Position >= jfsDirNode.Entries.Length) return ErrorNumber.NoError;

        filename = jfsDirNode.Entries[jfsDirNode.Position++];

        return ErrorNumber.NoError;
    }
}