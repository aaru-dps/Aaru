// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MINIX filesystem plugin.
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
public sealed partial class MinixFS
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

            node = new MinixDirNode
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
        for(var p = 0; p < pathComponents.Length; p++)
        {
            string component = pathComponents[p];

            AaruLogging.Debug(MODULE_NAME, "OpenDir: Navigating to component '{0}'", component);

            // Skip "." and ".." during traversal
            if(component is "." or "..") continue;

            // Find the component in current directory
            if(!currentEntries.TryGetValue(component, out uint inodeNumber))
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Component '{0}' not found in directory", component);

                return ErrorNumber.NoSuchFile;
            }

            AaruLogging.Debug(MODULE_NAME, "OpenDir: Component '{0}' found with inode {1}", component, inodeNumber);

            // Read the inode
            ErrorNumber errno = ReadInode(inodeNumber, out object inodeObj);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Error reading inode {0}: {1}", inodeNumber, errno);

                return errno;
            }

            // Check if it's a directory
            ushort mode = _version == FilesystemVersion.V1
                              ? ((V1DiskInode)inodeObj).d1_mode
                              : ((V2DiskInode)inodeObj).d2_mode;

            if((mode & (ushort)InodeMode.TypeMask) != (ushort)InodeMode.Directory)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: '{0}' is not a directory (mode=0x{1:X4})", component, mode);

                return ErrorNumber.NotDirectory;
            }

            // Read directory contents
            errno = GetDirectoryContents(inodeNumber, out Dictionary<string, uint> dirEntries);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "OpenDir: Error reading directory contents: {0}", errno);

                return errno;
            }

            // Filter out "." and ".." entries
            var filteredEntries = new Dictionary<string, uint>();

            foreach(KeyValuePair<string, uint> entry in dirEntries)
                if(entry.Key is not ("." or ".."))
                    filteredEntries[entry.Key] = entry.Value;

            // If this is the last component, we're opening this directory
            if(p == pathComponents.Length - 1)
            {
                node = new MinixDirNode
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
        if(node is not MinixDirNode minixNode) return ErrorNumber.InvalidArgument;

        minixNode.Position = -1;
        minixNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not MinixDirNode minixNode) return ErrorNumber.InvalidArgument;

        if(minixNode.Position < 0) return ErrorNumber.InvalidArgument;

        // End of directory
        if(minixNode.Position >= minixNode.Entries.Length) return ErrorNumber.NoError;

        // Get current filename and advance position
        filename = minixNode.Entries[minixNode.Position++];

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the contents of a directory, caching them as the filesystem is read-only</summary>
    /// <param name="inodeNumber">Inode number of the directory</param>
    /// <param name="entries">Dictionary of filename -> inode number</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber GetDirectoryContents(uint inodeNumber, out Dictionary<string, uint> entries)
    {
        if(_directoryCache.TryGetValue(inodeNumber, out entries)) return ErrorNumber.NoError;

        ErrorNumber errno = ReadDirectoryContents(inodeNumber, out entries);

        if(errno == ErrorNumber.NoError) _directoryCache[inodeNumber] = entries;

        return errno;
    }

    /// <summary>Reads the contents of a directory</summary>
    /// <param name="inodeNumber">Inode number of the directory</param>
    /// <param name="entries">Dictionary of filename -> inode number</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadDirectoryContents(uint inodeNumber, out Dictionary<string, uint> entries)
    {
        entries = new Dictionary<string, uint>();

        // Get inode
        ErrorNumber errno = ReadInode(inodeNumber, out object inodeObj);

        if(errno != ErrorNumber.NoError) return errno;

        uint   size;
        uint[] zones;
        int    nrDzones;

        if(_version == FilesystemVersion.V1)
        {
            var inode = (V1DiskInode)inodeObj;
            size = inode.d1_size;

            // Convert ushort[] to uint[]
            zones = new uint[inode.d1_zone.Length];

            for(var i = 0; i < inode.d1_zone.Length; i++) zones[i] = inode.d1_zone[i];

            nrDzones = V1_NR_DZONES;
        }
        else
        {
            var inode = (V2DiskInode)inodeObj;
            size     = inode.d2_size;
            zones    = inode.d2_zone;
            nrDzones = V2_NR_DZONES;
        }

        if(size == 0) return ErrorNumber.NoError;

        // Read directory data block by block
        var dirData   = new byte[size];
        var bytesRead = 0;

        // Read direct zones
        for(var i = 0; i < nrDzones && bytesRead < size; i++)
        {
            if(zones[i] == 0)
            {
                // Sparse - fill with zeros
                int toFill = Math.Min(_blockSize, (int)(size - bytesRead));
                bytesRead += toFill;

                continue;
            }

            errno = ReadBlock((int)zones[i], out byte[] blockData);

            if(errno != ErrorNumber.NoError) return errno;

            int toCopy = Math.Min(blockData.Length, (int)(size - bytesRead));
            Array.Copy(blockData, 0, dirData, bytesRead, toCopy);
            bytesRead += toCopy;
        }

        // Read single indirect zone if needed
        if(bytesRead < size && nrDzones < zones.Length && zones[nrDzones] != 0)
        {
            errno = ReadIndirectZone(zones[nrDzones], ref dirData, ref bytesRead, (int)size, 1);

            if(errno != ErrorNumber.NoError) return errno;
        }

        // Read double indirect zone if needed
        if(bytesRead < size && nrDzones + 1 < zones.Length && zones[nrDzones + 1] != 0)
        {
            errno = ReadIndirectZone(zones[nrDzones + 1], ref dirData, ref bytesRead, (int)size, 2);

            if(errno != ErrorNumber.NoError) return errno;
        }

        // Read triple indirect zone if needed (V2 only)
        if(bytesRead < size && nrDzones + 2 < zones.Length && zones[nrDzones + 2] != 0)
        {
            errno = ReadIndirectZone(zones[nrDzones + 2], ref dirData, ref bytesRead, (int)size, 3);

            if(errno != ErrorNumber.NoError) return errno;
        }

        // Parse directory entries
        int entrySize  = _filenameSize + (_version == FilesystemVersion.V3 ? 4 : 2);
        int numEntries = (int)size / entrySize;

        for(var i = 0; i < numEntries; i++)
        {
            int offset = i * entrySize;

            uint ino;

            if(_version == FilesystemVersion.V3)
            {
                ino = _littleEndian
                          ? BitConverter.ToUInt32(dirData, offset)
                          : (uint)(dirData[offset]     << 24 |
                                   dirData[offset + 1] << 16 |
                                   dirData[offset + 2] << 8  |
                                   dirData[offset + 3]);

                offset += 4;
            }
            else
            {
                ino = _littleEndian
                          ? BitConverter.ToUInt16(dirData, offset)
                          : (ushort)(dirData[offset] << 8 | dirData[offset + 1]);

                offset += 2;
            }

            // Skip empty entries
            if(ino == 0) continue;

            // Extract filename
            var nameBytes = new byte[_filenameSize];
            Array.Copy(dirData, offset, nameBytes, 0, _filenameSize);

            string name = StringHandlers.CToString(nameBytes, _encoding);

            if(string.IsNullOrEmpty(name)) continue;

            entries[name] = ino;
        }

        return ErrorNumber.NoError;
    }
}