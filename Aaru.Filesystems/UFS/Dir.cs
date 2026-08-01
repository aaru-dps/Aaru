// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNIX FIle System plugin.
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
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class UFSPlugin
{
    /// <summary>Parses directory entries from a directory inode's data</summary>
    ErrorNumber ParseDirectory(uint inodeNumber, out List<DirectoryEntryInfo> entries)
    {
        entries = [];

        // Get directory size
        ulong dirSize;

        if(_superBlock.fs_isUfs2)
        {
            ErrorNumber errno = ReadInode2(inodeNumber, out Inode2 inode2);

            if(errno != ErrorNumber.NoError) return errno;

            dirSize = inode2.di_size;
        }
        else
        {
            ErrorNumber errno = ReadInode(inodeNumber, out Inode inode);

            if(errno != ErrorNumber.NoError) return errno;

            dirSize = inode.di_size;
        }

        if(dirSize == 0) return ErrorNumber.NoError;

        // Read entire directory data
        ErrorNumber err = ReadInodeData(inodeNumber, 0, (long)dirSize, out byte[] dirData);

        if(err != ErrorNumber.NoError) return err;

        // Determine if we use old-format directory entries (no d_type field)
        bool oldFormat = _superBlock.fs_inodefmt != 2;

        // Walk through directory entries
        var pos = 0;

        while(pos + 8 <= dirData.Length) // Minimum entry is 8 bytes (ino + reclen + namlen)
        {
            uint   dIno;
            ushort dReclen;
            int    dNamlen;

            if(_bigEndian)
            {
                dIno    = Swapping.Swap(BitConverter.ToUInt32(dirData, pos));
                dReclen = Swapping.Swap(BitConverter.ToUInt16(dirData, pos + 4));
            }
            else
            {
                dIno    = BitConverter.ToUInt32(dirData, pos);
                dReclen = BitConverter.ToUInt16(dirData, pos + 4);
            }

            if(dReclen == 0) break;

            if(oldFormat)
            {
                // Pre-4.4BSD: d_namlen is a 16-bit value at offset 6
                dNamlen = _bigEndian
                              ? Swapping.Swap(BitConverter.ToUInt16(dirData, pos + 6))
                              : BitConverter.ToUInt16(dirData, pos + 6);
            }
            else
            {
                // 4.4BSD+: d_type at offset 6, d_namlen at offset 7
                dNamlen = dirData[pos + 7];
            }

            if(dIno != 0 && dNamlen > 0 && pos + 8 + dNamlen <= dirData.Length)
            {
                string name = _encoding.GetString(dirData, pos + 8, dNamlen);

                entries.Add(new DirectoryEntryInfo
                {
                    Inode = dIno,
                    Name  = name
                });
            }

            pos += dReclen;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Parsed directory entries plus a name index for O(1) lookups</summary>
    sealed class CachedDirectory
    {
        public Dictionary<string, uint> ByName;
        public List<DirectoryEntryInfo> Entries;
    }

    /// <summary>Gets the contents of a directory, caching them as the filesystem is read-only</summary>
    /// <param name="inodeNumber">Inode number of the directory</param>
    /// <param name="dir">Parsed directory</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber GetDirectory(uint inodeNumber, out CachedDirectory dir)
    {
        if(_directoryCache.TryGetValue(inodeNumber, out dir)) return ErrorNumber.NoError;

        ErrorNumber errno = ParseDirectory(inodeNumber, out List<DirectoryEntryInfo> entries);

        if(errno != ErrorNumber.NoError) return errno;

        var byName = new Dictionary<string, uint>(StringComparer.Ordinal);

        foreach(DirectoryEntryInfo entry in entries) byName.TryAdd(entry.Name, entry.Inode);

        dir = new CachedDirectory
        {
            Entries = entries,
            ByName  = byName
        };

        _directoryCache[inodeNumber] = dir;

        return ErrorNumber.NoError;
    }

    /// <summary>Resolves a path string to an inode number</summary>
    ErrorNumber ResolvePath(string path, out uint inodeNumber)
    {
        inodeNumber = UFS_ROOTINO;

        if(string.IsNullOrEmpty(path) || path == "/") return ErrorNumber.NoError;

        string stripped = path.StartsWith('/') ? path[1..] : path;

        if(stripped.EndsWith('/')) stripped = stripped[..^1];

        if(string.IsNullOrEmpty(stripped)) return ErrorNumber.NoError;

        if(_pathCache.TryGetValue(stripped, out inodeNumber)) return ErrorNumber.NoError;

        // Resolve against the already-cached parent so a path a thousand directories deep does not
        // re-walk the whole ancestor chain on every call
        int lastSlash = stripped.LastIndexOf('/');

        if(lastSlash > 0 && _pathCache.TryGetValue(stripped[..lastSlash], out uint parentInode))
        {
            ErrorNumber parentErrno = GetDirectory(parentInode, out CachedDirectory parentDir);

            if(parentErrno != ErrorNumber.NoError) return parentErrno;

            if(!parentDir.ByName.TryGetValue(stripped[(lastSlash + 1)..], out inodeNumber))
                return ErrorNumber.NoSuchFile;

            CachePath(stripped, inodeNumber);

            return ErrorNumber.NoError;
        }

        string[] components = stripped.Split('/', StringSplitOptions.RemoveEmptyEntries);

        uint currentInode = UFS_ROOTINO;

        foreach(string component in components)
        {
            ErrorNumber errno = GetDirectory(currentInode, out CachedDirectory dir);

            if(errno != ErrorNumber.NoError) return errno;

            // Find matching entry
            if(!dir.ByName.TryGetValue(component, out currentInode)) return ErrorNumber.NoSuchFile;
        }

        inodeNumber = currentInode;
        CachePath(stripped, inodeNumber);

        return ErrorNumber.NoError;
    }

    /// <summary>Caches a resolved path, bounding memory use</summary>
    void CachePath(string strippedPath, uint inodeNumber)
    {
        // Bound memory use; resolutions are cheap to redo after a wholesale reset
        if(_pathCache.Count >= 262144) _pathCache.Clear();

        _pathCache[strippedPath] = inodeNumber;
    }

    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = ResolvePath(path, out uint inodeNumber);

        if(errno != ErrorNumber.NoError) return errno;

        // Verify it's a directory
        if(_superBlock.fs_isUfs2)
        {
            errno = ReadInode2(inodeNumber, out Inode2 inode2);

            if(errno != ErrorNumber.NoError) return errno;

            if((inode2.di_mode & 0xF000) != 0x4000) return ErrorNumber.NotDirectory;
        }
        else
        {
            errno = ReadInode(inodeNumber, out Inode inode);

            if(errno != ErrorNumber.NoError) return errno;

            if((inode.di_mode & 0xF000) != 0x4000) return ErrorNumber.NotDirectory;
        }

        // Parse directory entries
        errno = GetDirectory(inodeNumber, out CachedDirectory dir);

        if(errno != ErrorNumber.NoError) return errno;

        // Collect entry names, skipping . and ..
        List<string> nameList = [];

        foreach(DirectoryEntryInfo entry in dir.Entries)
        {
            if(entry.Name is "." or "..") continue;

            nameList.Add(entry.Name);
        }

        node = new UfsDirNode
        {
            Path     = path,
            Entries  = nameList.ToArray(),
            Position = 0
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not UfsDirNode dirNode) return ErrorNumber.InvalidArgument;

        if(dirNode.Position >= dirNode.Entries.Length) return ErrorNumber.NoError;

        filename = dirNode.Entries[dirNode.Position++];

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not UfsDirNode) return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <summary>Holds a parsed directory entry</summary>
    struct DirectoryEntryInfo
    {
        public uint   Inode;
        public string Name;
    }
}