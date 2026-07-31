// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux extended filesystem 2, 3 and 4 plugin.
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

// ReSharper disable once InconsistentNaming
public sealed partial class ext2FS
{
    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory — use cached entries
        if(normalizedPath is "/")
        {
            if(_rootDirectoryCache.Count == 0) return ErrorNumber.NoSuchFile;

            node = new Ext2DirNode
            {
                Path     = "/",
                Position = 0,
                Entries  = _rootDirectoryCache.Keys.ToArray()
            };

            return ErrorNumber.NoError;
        }

        // Subdirectory traversal — split path into components
        string stripped = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                              ? normalizedPath[1..]
                              : normalizedPath;

        string[] components = stripped.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(components.Length == 0) return ErrorNumber.InvalidArgument;

        Dictionary<string, uint> currentEntries = _rootDirectoryCache;

        for(var i = 0; i < components.Length; i++)
        {
            string component = components[i];

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out uint inodeNumber)) return ErrorNumber.NoSuchFile;

            // Read inode and verify it's a directory
            ErrorNumber errno = ReadInode(inodeNumber, out Inode inode);

            if(errno != ErrorNumber.NoError) return errno;

            if((inode.mode & S_IFMT) != S_IFDIR) return ErrorNumber.NotDirectory;

            // Read subdirectory entries from the per-mount cache (or from disk on first access)
            errno = GetCachedDirectoryEntries(inode, inodeNumber, out Dictionary<string, uint> filtered);

            if(errno != ErrorNumber.NoError) return errno;

            // Last component — create the node
            if(i == components.Length - 1)
            {
                node = new Ext2DirNode
                {
                    Path     = normalizedPath,
                    Position = 0,
                    Entries  = filtered.Keys.ToArray()
                };

                return ErrorNumber.NoError;
            }

            // Intermediate component — descend
            currentEntries = filtered;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not Ext2DirNode dirNode) return ErrorNumber.InvalidArgument;

        dirNode.Position = -1;
        dirNode.Entries  = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(node is not Ext2DirNode dirNode) return ErrorNumber.InvalidArgument;

        if(dirNode.Position < 0) return ErrorNumber.InvalidArgument;

        if(dirNode.Position >= dirNode.Entries.Length) return ErrorNumber.NoError;

        filename = dirNode.Entries[dirNode.Position++];

        return ErrorNumber.NoError;
    }


    /// <summary>
    ///     Returns the directory entries for an inode, filtered of "." and "..", using a per-mount cache to avoid
    ///     re-reading and re-parsing directory blocks
    /// </summary>
    /// <param name="inode">The directory inode</param>
    /// <param name="inodeNumber">The inode number</param>
    /// <param name="entries">Dictionary of filename to inode number, without "." and ".."</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber GetCachedDirectoryEntries(Inode inode, uint inodeNumber, out Dictionary<string, uint> entries)
    {
        if(_directoryCache.TryGetValue(inodeNumber, out entries)) return ErrorNumber.NoError;

        ulong dirSize = (ulong)inode.size_high << 32 | inode.size_lo;

        ErrorNumber errno = ReadDirectoryEntries(inode, inodeNumber, dirSize, out Dictionary<string, uint> subEntries);

        if(errno != ErrorNumber.NoError) return errno;

        // Filter . and ..
        entries = new Dictionary<string, uint>(subEntries.Count, StringComparer.Ordinal);

        foreach(KeyValuePair<string, uint> e in subEntries)
        {
            if(e.Key is not ("." or "..")) entries[e.Key] = e.Value;
        }

        _directoryCache[inodeNumber] = entries;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads directory entries from an inode's data blocks</summary>
    /// <param name="inode">The directory inode</param>
    /// <param name="inodeNumber">The inode number</param>
    /// <param name="size">The directory size in bytes</param>
    /// <param name="entries">Dictionary of filename to inode number</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadDirectoryEntries(Inode inode, uint inodeNumber, ulong size, out Dictionary<string, uint> entries)
    {
        entries = new Dictionary<string, uint>(StringComparer.Ordinal);

        // Inline data directories: data stored in inode body + ibody xattr
        if((inode.i_flags & EXT4_INLINE_DATA_FL) != 0)
        {
            ErrorNumber inlineErr = GetInlineData(inodeNumber, inode, size, out byte[] inlineData);

            if(inlineErr != ErrorNumber.NoError) return inlineErr;

            if(inlineData == null || inlineData.Length < 4) return ErrorNumber.NoError;

            // Inline directory layout (per kernel inline.c):
            // Bytes 0-3:  Parent inode number (compact ".." entry, EXT4_INLINE_DOTDOT_SIZE)
            // Bytes 4+:   Regular ext4_dir_entry_2 entries
            // Skip the first 4 bytes (compact .. entry)
            var dataStart = 4;
            int dataLen   = inlineData.Length - dataStart;

            if(dataLen > 0)
            {
                var dirData = new byte[dataLen];
                Array.Copy(inlineData, dataStart, dirData, 0, dataLen);
                ParseDirectoryBlock(dirData, (uint)dataLen, entries);
            }

            return ErrorNumber.NoError;
        }

        // Standard block-based directory reading
        // Get all data blocks for the inode
        ErrorNumber errno =
            GetInodeDataBlocks(inode, out List<(ulong physicalBlock, uint length, bool unwritten)> blockList);

        if(errno != ErrorNumber.NoError) return errno;

        ulong bytesRead = 0;

        foreach((ulong physicalBlock, uint length, bool unwritten) in blockList)
        {
            if(bytesRead >= size) break;

            // Read the data blocks
            var bytesToRead = (uint)Math.Min(length * _blockSize, size - bytesRead);

            if(unwritten)
            {
                // Unwritten extents are zeroes, skip — no valid directory entries
                bytesRead += length * _blockSize;

                continue;
            }

            errno = ReadBytes(physicalBlock * _blockSize, bytesToRead, out byte[] blockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading directory block at {0}: {1}", physicalBlock, errno);

                bytesRead += length * _blockSize;

                continue;
            }

            // Parse directory entries within this data
            ParseDirectoryBlock(blockData, bytesToRead, entries);

            bytesRead += length * _blockSize;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Parses directory entries from a block's data</summary>
    /// <param name="blockData">The block data</param>
    /// <param name="validBytes">Number of valid bytes in the block</param>
    /// <param name="entries">Dictionary to add entries to</param>
    void ParseDirectoryBlock(byte[] blockData, uint validBytes, Dictionary<string, uint> entries)
    {
        uint offset = 0;

        while(offset + 8 <= validBytes && offset + 8 <= blockData.Length)
        {
            // Read the fixed header fields manually since entries are variable-length
            var inodeNum = BitConverter.ToUInt32(blockData, (int)offset);
            var recLen   = BitConverter.ToUInt16(blockData, (int)(offset + 4));

            // Validate record length
            // rec_len must be at least 12 (8 bytes header + minimum 4 bytes for alignment)
            // and be a multiple of 4
            if(recLen < 12 || recLen % 4 != 0)
            {
                AaruLogging.Debug(MODULE_NAME, "Invalid record length {0} at offset {1}", recLen, offset);

                break;
            }

            if(offset + recLen > validBytes || offset + recLen > blockData.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "Record extends beyond valid data at offset {0}", offset);

                break;
            }

            // Handle ext4_rec_len_from_disk encoding for large blocks:
            // If recLen == 0xFFFF or recLen == 0, it means "rest of block"
            uint actualRecLen = recLen;

            if(recLen == 0xFFFF || recLen == 0)
                actualRecLen = _blockSize - offset % _blockSize;
            else if(_blockSize > 65536)
            {
                // Low 2 bits encode high 2 bits of length
                actualRecLen = (uint)(recLen & 0xFFFC | (recLen & 3) << 16);
            }

            byte nameLen;
            byte fileType = 0;

            if(_hasFileType)
            {
                // DirectoryEntry2: name_len is 1 byte at offset 6, file_type is 1 byte at offset 7
                nameLen  = blockData[offset + 6];
                fileType = blockData[offset + 7];

                // Filter out checksum tail entries (fake dir entries with file_type = 0xDE)
                if(fileType == EXT4_FT_DIR_CSUM)
                {
                    offset += actualRecLen;

                    continue;
                }
            }
            else
            {
                // DirectoryEntry: name_len is 2 bytes at offset 6
                nameLen = (byte)Math.Min(BitConverter.ToUInt16(blockData, (int)(offset + 6)), (ushort)255);
            }

            // Validate name length
            if(nameLen > 0 && nameLen + 8 <= actualRecLen && inodeNum != 0)
            {
                if(offset + 8 + nameLen <= blockData.Length)
                {
                    var nameBytes = new byte[nameLen];
                    Array.Copy(blockData, (int)(offset + 8), nameBytes, 0, nameLen);
                    string filename = StringHandlers.CToString(nameBytes, _encoding);

                    if(!string.IsNullOrWhiteSpace(filename)) entries[filename] = inodeNum;
                }
            }

            offset += actualRecLen;
        }
    }
}