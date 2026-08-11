// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Locus filesystem plugin
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
public sealed partial class Locus
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory case
        if(normalizedPath == "/" || string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
        {
            if(_rootDirectoryCache.Count == 0) return ErrorNumber.NoSuchFile;

            node = new LocusDirNode
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
        Dictionary<string, int> currentEntries = _rootDirectoryCache;

        // Traverse each path component
        for(var p = 0; p < pathComponents.Length; p++)
        {
            string component = pathComponents[p];

            AaruLogging.Debug(MODULE_NAME, "OpenDir: Navigating to component '{0}'", component);

            // Skip "." and ".." during traversal
            if(component is "." or "..") continue;

            // Find the component in current directory
            if(!currentEntries.TryGetValue(component, out int inodeNumber))
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Component '{0}' not found in directory", component);

                return ErrorNumber.NoSuchFile;
            }

            AaruLogging.Debug(MODULE_NAME, "OpenDir: Component '{0}' found with inode {1}", component, inodeNumber);

            // Read the inode
            ErrorNumber errno = ReadInode(inodeNumber, out Dinode inode);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Error reading inode {0}: {1}", inodeNumber, errno);

                return errno;
            }

            // Check if it's a directory
            var fileType = (FileMode)(inode.di_mode & (ushort)FileMode.IFMT);

            if(fileType != FileMode.IFDIR)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "OpenDir: '{0}' is not a directory (type=0x{1:X4})",
                                  component,
                                  (ushort)fileType);

                return ErrorNumber.NotDirectory;
            }

            // Read directory contents
            errno = ReadDirectoryContents(inodeNumber, inode, out Dictionary<string, int> dirEntries);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Error reading directory contents: {0}", errno);

                return errno;
            }

            // Filter out "." and ".." entries
            var filteredEntries = new Dictionary<string, int>();

            foreach(KeyValuePair<string, int> entry in dirEntries)
                if(entry.Key is not ("." or ".."))
                    filteredEntries[entry.Key] = entry.Value;

            // If this is the last component, we're opening this directory
            if(p == pathComponents.Length - 1)
            {
                node = new LocusDirNode
                {
                    Path     = normalizedPath,
                    Position = 0,
                    Entries  = filteredEntries.Keys.ToArray()
                };

                AaruLogging.Debug(MODULE_NAME,
                                  "OpenDir: Successfully opened directory '{0}' with {1} entries",
                                  normalizedPath,
                                  filteredEntries.Count);

                return ErrorNumber.NoError;
            }

            // Not the last component - move to next level
            currentEntries = filteredEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not LocusDirNode locusNode) return ErrorNumber.InvalidArgument;

        locusNode.Position = -1;
        locusNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not LocusDirNode locusNode) return ErrorNumber.InvalidArgument;

        if(locusNode.Position < 0) return ErrorNumber.InvalidArgument;

        // End of directory
        if(locusNode.Position >= locusNode.Entries.Length) return ErrorNumber.NoError;

        // Get current filename and advance position
        filename = locusNode.Entries[locusNode.Position++];

        return ErrorNumber.NoError;
    }

    /// <summary>Reads directory contents from an inode</summary>
    /// <param name="inodeNumber">Inode number</param>
    /// <param name="inode">Directory inode</param>
    /// <param name="entries">Dictionary of filename to inode number</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadDirectoryContents(int inodeNumber, Dinode inode, out Dictionary<string, int> entries)
    {
        entries = new Dictionary<string, int>();

        AaruLogging.Debug(MODULE_NAME,
                          "ReadDirectoryContents: size={0}, blocks={1}, dflag=0x{2:X4}",
                          inode.di_size,
                          inode.di_blocks,
                          inode.di_dflag);

        if(inode.di_size <= 0)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadDirectoryContents: Directory has zero size");

            return ErrorNumber.NoError;
        }

        // Debug: Log first few block addresses
        if(inode.di_addr != null)
        {
            for(var i = 0; i < Math.Min(5, inode.di_addr.Length); i++)
                AaruLogging.Debug(MODULE_NAME, "ReadDirectoryContents: di_addr[{0}] = {1}", i, inode.di_addr[i]);
        }
        else
            AaruLogging.Debug(MODULE_NAME, "ReadDirectoryContents: di_addr is NULL!");

        // Check if this is a long directory (BSD 4.3 format) or old format
        bool longDir = (inode.di_dflag & (short)DiskFlags.DILONGDIR) != 0;

        AaruLogging.Debug(MODULE_NAME, "Directory format: {0}", longDir ? "long (BSD 4.3)" : "old (System V)");

        // Read all directory data
        ErrorNumber errno = ReadFileData(inodeNumber, inode, out byte[] dirData);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading directory data: {0}", errno);

            return errno;
        }

        if(longDir)
            ParseLongDirectory(dirData, entries);
        else
            ParseOldDirectory(dirData, entries);

        return ErrorNumber.NoError;
    }

    /// <summary>Parses old System V format directory</summary>
    /// <param name="data">Raw directory data</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    void ParseOldDirectory(byte[] data, Dictionary<string, int> entries)
    {
        var offset = 0;

        while(offset + DIRSIZ + 2 <= data.Length) // 2 bytes for inode + 14 bytes for name
        {
            ushort ino = _bigEndian
                             ? (ushort)(data[offset] << 8 | data[offset + 1])
                             : (ushort)(data[offset]      | data[offset + 1] << 8);

            offset += 2;

            if(ino == 0)
            {
                offset += DIRSIZ;

                continue;
            }

            // Extract name (14 bytes, null-padded)
            var nameLen = 0;

            for(var i = 0; i < DIRSIZ && data[offset + i] != 0; i++) nameLen++;

            string name = _encoding.GetString(data, offset, nameLen);
            offset += DIRSIZ;

            if(!string.IsNullOrEmpty(name) && !entries.ContainsKey(name))
            {
                entries[name] = ino;

                AaruLogging.Debug(MODULE_NAME, "Old dir entry: {0} -> inode {1}", name, ino);
            }
        }
    }

    /// <summary>Parses long BSD 4.3 format directory</summary>
    /// <param name="data">Raw directory data</param>
    /// <param name="entries">Dictionary to populate with entries</param>
    void ParseLongDirectory(byte[] data, Dictionary<string, int> entries)
    {
        var offset = 0;

        while(offset < data.Length)
        {
            if(offset + 8 > data.Length) break;

            int ino = _bigEndian
                          ? data[offset] << 24 | data[offset + 1] << 16 | data[offset + 2] << 8 | data[offset + 3]
                          : data[offset] | data[offset + 1] << 8 | data[offset + 2] << 16 | data[offset + 3] << 24;

            ushort reclen = _bigEndian
                                ? (ushort)(data[offset + 4] << 8 | data[offset + 5])
                                : (ushort)(data[offset                         + 4] | data[offset + 5] << 8);

            ushort namlen = _bigEndian
                                ? (ushort)(data[offset + 6] << 8 | data[offset + 7])
                                : (ushort)(data[offset                         + 6] | data[offset + 7] << 8);

            if(reclen == 0) break;

            if(ino != 0 && namlen > 0 && offset + 8 + namlen <= data.Length)
            {
                string name = _encoding.GetString(data, offset + 8, namlen);

                if(!string.IsNullOrEmpty(name) && !entries.ContainsKey(name))
                {
                    entries[name] = ino;

                    AaruLogging.Debug(MODULE_NAME, "Long dir entry: {0} -> inode {1}", name, ino);
                }
            }

            offset += reclen;
        }
    }
}