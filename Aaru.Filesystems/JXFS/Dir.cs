// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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
public sealed partial class JXFS
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out DirectoryEntry entry);

        if(errno != ErrorNumber.NoError) return errno;

        if((entry.Node.Flags & NodeFlags.Directory) == 0) return ErrorNumber.NotDirectory;

        errno = LoadDirectory(entry.Node, out Dictionary<string, DirectoryEntry> entries);

        if(errno != ErrorNumber.NoError) return errno;

        node = new JxfsDirNode
        {
            Path     = path,
            Position = 0,
            Entries  = entries.Keys.ToArray()
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not JxfsDirNode dirNode || dirNode.Position < 0) return ErrorNumber.InvalidArgument;

        if(dirNode.Position >= dirNode.Entries.Length) return ErrorNumber.NoError;

        filename = dirNode.Entries[dirNode.Position++];

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not JxfsDirNode dirNode) return ErrorNumber.InvalidArgument;

        dirNode.Position = -1;
        dirNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a NUL-terminated name from a block</summary>
    /// <param name="data">Block contents</param>
    /// <param name="offset">Offset of the name</param>
    /// <param name="end">Offset of the terminating NUL, or the block size if there is none</param>
    /// <returns>The name</returns>
    string ReadName(byte[] data, int offset, out int end)
    {
        end = Array.IndexOf(data, (byte)0, offset);

        if(end < 0) end = data.Length;

        return _encoding.GetString(data, offset, end - offset);
    }

    /// <summary>Loads the entries of a directory, without the hidden system entries</summary>
    /// <param name="directory">Directory node</param>
    /// <param name="entries">Entries by name</param>
    /// <returns>Error number</returns>
    ErrorNumber LoadDirectory(Node directory, out Dictionary<string, DirectoryEntry> entries)
    {
        if(_directoryCache.TryGetValue(directory.Number, out entries)) return ErrorNumber.NoError;

        entries = new Dictionary<string, DirectoryEntry>(_caseSensitive
                                                             ? StringComparer.Ordinal
                                                             : StringComparer.OrdinalIgnoreCase);

        // The directory node points to its first container, and each container to the next one
        uint          container = directory.Pointer;
        HashSet<uint> visited = new HashSet<uint>();

        while(container != 0)
        {
            if(!visited.Add(container) || visited.Count > MAX_CHAIN_LENGTH) return ErrorNumber.InvalidArgument;

            ErrorNumber errno = ReadSystemFileBlock(_dirFile, container, OBJECTCONTAINER_ID, out byte[] data);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading directory container {0}: {1}", container, errno);

                return errno;
            }

            // Each record is the key, the node and the name, aligned to 2 bytes
            int offset = OBJECTCONTAINER_HEADER_SIZE;

            while(offset < data.Length - 9 && data[offset + 8] != 0)
            {
                uint   key        = BigEndianBitConverter.ToUInt32(data, offset);
                uint   nodeNumber = BigEndianBitConverter.ToUInt32(data, offset + 4);
                string name       = ReadName(data, offset + 8, out int end);

                offset =  end + 1;
                offset += offset & 1;

                errno = ReadNode(nodeNumber, out Node node);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME, "Error reading node {0} of entry \"{1}\": {2}", nodeNumber, name,
                                      errno);

                    continue;
                }

                if((node.Flags & NodeFlags.Hidden) != 0) continue;

                entries[name] = new DirectoryEntry
                {
                    Key  = key,
                    Name = name,
                    Node = node
                };
            }

            container = BigEndianBitConverter.ToUInt32(data, OBJECTCONTAINER_NEXT_OFFSET);
        }

        _directoryCache[directory.Number] = entries;

        return ErrorNumber.NoError;
    }

    /// <summary>Finds the directory entry of a path</summary>
    /// <param name="path">Path, with '/' separators</param>
    /// <param name="entry">The entry</param>
    /// <returns>Error number</returns>
    ErrorNumber GetEntry(string path, out DirectoryEntry entry)
    {
        entry = _rootEntry;

        string[] components = (path ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach(string component in components)
        {
            if(component == ".") continue;

            if((entry.Node.Flags & NodeFlags.Directory) == 0) return ErrorNumber.NotDirectory;

            ErrorNumber errno = LoadDirectory(entry.Node, out Dictionary<string, DirectoryEntry> entries);

            if(errno != ErrorNumber.NoError) return errno;

            if(!entries.TryGetValue(component, out entry)) return ErrorNumber.NoSuchFile;
        }

        return ErrorNumber.NoError;
    }
}