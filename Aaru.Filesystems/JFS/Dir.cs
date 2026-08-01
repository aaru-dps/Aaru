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
        // Remove leading slash
        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        ErrorNumber resolveErrno = ResolvePathToInode(pathWithoutLeadingSlash, out uint dirInodeNumber);

        if(resolveErrno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenDir: error resolving path: {0}", resolveErrno);

            return resolveErrno;
        }

        ErrorNumber inodeErrno = GetFilesetInode(dirInodeNumber, out Inode dirInode);

        if(inodeErrno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenDir: error reading inode {0}: {1}", dirInodeNumber, inodeErrno);

            return inodeErrno;
        }

        if((dirInode.di_mode & 0xF000) != 0x4000)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "OpenDir: '{0}' is not a directory (mode=0x{1:X})",
                              normalizedPath,
                              dirInode.di_mode);

            return ErrorNumber.NotDirectory;
        }

        ErrorNumber dirErrno =
            GetDirectoryEntries(dirInodeNumber, dirInode.di_u, out Dictionary<string, uint> currentEntries);

        if(dirErrno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenDir: error parsing directory dtree: {0}", dirErrno);

            return dirErrno;
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