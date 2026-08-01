// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Professional File System plugin.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class PFS
{
    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "ReadLink: path='{0}'", path);

        // Find the file entry
        ErrorNumber errno = GetEntryForPath(path, out DirEntryCacheItem entry);

        if(errno != ErrorNumber.NoError) return errno;

        // Check if it's a symbolic link
        if(entry.Type != EntryType.SoftLink)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadLink: '{0}' is not a symbolic link (type={1})", path, entry.Type);

            return ErrorNumber.InvalidArgument;
        }

        // Get the link's anode to find the data block
        errno = GetAnode(entry.Anode, out Anode linkAnode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadLink: Error getting anode {0}: {1}", entry.Anode, errno);

            return errno;
        }

        // Read the block containing the symbolic link target
        errno = ReadBlock(linkAnode.blocknr, out byte[] blockData);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadLink: Error reading block {0}: {1}", linkAnode.blocknr, errno);

            return errno;
        }

        // The fsize field contains the length of the symbolic link path
        // The path is stored at the beginning of the data block
        var linkLength = (int)entry.Size;

        if(linkLength <= 0 || linkLength > blockData.Length)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadLink: Invalid link length {0}", linkLength);

            return ErrorNumber.InvalidArgument;
        }

        // Extract the symbolic link target
        dest = _encoding.GetString(blockData, 0, linkLength);

        AaruLogging.Debug(MODULE_NAME, "ReadLink: '{0}' -> '{1}'", path, dest);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "OpenFile: path='{0}'", path);

        // Find the file entry
        ErrorNumber errno = GetEntryForPath(path, out DirEntryCacheItem entry);

        if(errno != ErrorNumber.NoError) return errno;

        // Check if it's a file
        if(entry.Type == EntryType.Directory || entry.Type == EntryType.HardLinkDir)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: '{0}' is a directory", path);

            return ErrorNumber.IsDirectory;
        }

        // Get the file's starting anode
        errno = GetAnode(entry.Anode, out Anode fileAnode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: Error getting anode {0}: {1}", entry.Anode, errno);

            return errno;
        }

        // Create file node
        node = new PFSFileNode
        {
            Path         = path,
            Length       = entry.Size,
            Offset       = 0,
            StartAnode   = entry.Anode,
            CurrentAnode = fileAnode,
            AnodeOffset  = 0,
            BlockOffset  = 0,
            FileSize     = entry.Size
        };

        AaruLogging.Debug(MODULE_NAME,
                          "OpenFile: Opened file '{0}', size={1}, anode={2}",
                          path,
                          entry.Size,
                          entry.Anode);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not PFSFileNode) return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(buffer is null || buffer.Length < length) return ErrorNumber.InvalidArgument;

        if(node is not PFSFileNode pfsNode) return ErrorNumber.InvalidArgument;

        // Can't read past end of file
        if(pfsNode.Offset >= pfsNode.Length) return ErrorNumber.NoError;

        // Limit read to remaining file size
        if(length > pfsNode.Length - pfsNode.Offset) length = pfsNode.Length - pfsNode.Offset;

        var  bufferOffset   = 0;
        long bytesRemaining = length;

        while(bytesRemaining > 0)
        {
            // Calculate current block number within the filesystem
            uint currentBlock = pfsNode.CurrentAnode.blocknr + pfsNode.AnodeOffset;

            // Read the current data block
            ErrorNumber errno = ReadBlock(currentBlock, out byte[] blockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadFile: Error reading block {0}: {1}", currentBlock, errno);

                return errno;
            }

            // Calculate how much data to read from this block
            var dataInBlock = (int)(_blockSize - pfsNode.BlockOffset);

            if(dataInBlock > bytesRemaining) dataInBlock = (int)bytesRemaining;

            // Copy data to buffer
            Array.Copy(blockData, (int)pfsNode.BlockOffset, buffer, bufferOffset, dataInBlock);

            bufferOffset   += dataInBlock;
            bytesRemaining -= dataInBlock;
            pfsNode.Offset += dataInBlock;

            // Update position within block
            pfsNode.BlockOffset += (uint)dataInBlock;

            // If we've read past the end of this block, move to the next
            if(pfsNode.BlockOffset >= _blockSize)
            {
                pfsNode.BlockOffset = 0;
                pfsNode.AnodeOffset++;

                // Check if we need to move to the next anode in the chain
                if(pfsNode.AnodeOffset >= pfsNode.CurrentAnode.clustersize)
                {
                    // Move to next anode
                    if(pfsNode.CurrentAnode.next == ANODE_EOF)
                    {
                        // End of file
                        break;
                    }

                    errno = GetAnode(pfsNode.CurrentAnode.next, out Anode nextAnode);

                    if(errno != ErrorNumber.NoError)
                    {
                        AaruLogging.Debug(MODULE_NAME,
                                          "ReadFile: Error getting next anode {0}: {1}",
                                          pfsNode.CurrentAnode.next,
                                          errno);

                        break;
                    }

                    pfsNode.CurrentAnode = nextAnode;
                    pfsNode.AnodeOffset  = 0;
                }
            }
        }

        read = bufferOffset;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Stat: path='{0}'", path);

        // Normalize the path
        string normalizedPath = path ?? "/";

        if(normalizedPath == "" || normalizedPath == ".") normalizedPath = "/";

        // Root directory
        if(normalizedPath == "/" || string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
        {
            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.Directory,
                Inode      = ANODE_ROOTDIR,
                Links      = 1,
                BlockSize  = _blockSize,
                CreationTimeUtc =
                    DateHandlers.AmigaToDateTime(_rootBlock.creationday,
                                                 _rootBlock.creationminute,
                                                 _rootBlock.creationtick),
                LastWriteTimeUtc = DateHandlers.AmigaToDateTime(_rootBlock.creationday,
                                                                _rootBlock.creationminute,
                                                                _rootBlock.creationtick)
            };

            return ErrorNumber.NoError;
        }

        // Find the entry
        ErrorNumber errno = GetEntryForPath(normalizedPath, out DirEntryCacheItem entry);

        if(errno != ErrorNumber.NoError) return errno;

        // Build stat from entry
        stat = BuildStatFromEntry(entry);

        return ErrorNumber.NoError;
    }

    /// <summary>Gets the directory entry for a given path</summary>
    /// <param name="path">The path to find</param>
    /// <param name="entry">The directory entry</param>
    /// <returns>Error code</returns>
    ErrorNumber GetEntryForPath(string path, out DirEntryCacheItem entry)
    {
        entry = null;

        // Remove leading slash
        string pathWithoutLeadingSlash = path.StartsWith("/", StringComparison.Ordinal) ? path[1..] : path;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        // Start from root directory cache
        Dictionary<string, DirEntryCacheItem> currentEntries = _rootDirectoryCache;
        DirEntryCacheItem                     currentEntry   = null;

        // Traverse each path component
        for(var i = 0; i < pathComponents.Length; i++)
        {
            string component = pathComponents[i];

            // Find the component in current directory (case-insensitive via dictionary comparer)
            if(!currentEntries.TryGetValue(component, out currentEntry))
            {
                AaruLogging.Debug(MODULE_NAME, "GetEntryForPath: Component '{0}' not found", component);

                return ErrorNumber.NoSuchFile;
            }

            // If not the last component, it must be a directory
            if(i < pathComponents.Length - 1)
            {
                if(currentEntry.Type != EntryType.Directory && currentEntry.Type != EntryType.HardLinkDir)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "GetEntryForPath: '{0}' is not a directory (type={1})",
                                      component,
                                      currentEntry.Type);

                    return ErrorNumber.NotDirectory;
                }

                // Read directory contents
                ErrorNumber errno = GetDirectoryContents(currentEntry.Anode, out currentEntries);

                if(errno != ErrorNumber.NoError) return errno;
            }
        }

        entry = currentEntry;

        return ErrorNumber.NoError;
    }

    /// <summary>Builds a FileEntryInfo from a cached directory entry</summary>
    /// <param name="entry">The directory entry</param>
    /// <returns>FileEntryInfo structure</returns>
    FileEntryInfo BuildStatFromEntry(DirEntryCacheItem entry)
    {
        var stat = new FileEntryInfo
        {
            Inode           = entry.Anode,
            Links           = 1,
            BlockSize       = _blockSize,
            CreationTimeUtc = DateHandlers.AmigaToDateTime(entry.CreationDay, entry.CreationMinute, entry.CreationTick),
            LastWriteTimeUtc =
                DateHandlers.AmigaToDateTime(entry.CreationDay, entry.CreationMinute, entry.CreationTick),
            Mode = ProtectionToUnixMode(entry.Protection)
        };

        // Determine attributes from entry type
        switch(entry.Type)
        {
            case EntryType.Directory:
            case EntryType.HardLinkDir:
                stat.Attributes = FileAttributes.Directory;

                break;

            case EntryType.File:
            case EntryType.HardLinkFile:
            case EntryType.RolloverFile:
                stat.Attributes = FileAttributes.File;
                stat.Length     = entry.Size;

                break;

            case EntryType.SoftLink:
                stat.Attributes = FileAttributes.Symlink;

                break;

            default:
                stat.Attributes = FileAttributes.File;
                stat.Length     = entry.Size;

                break;
        }

        // Apply Amiga protection flags to attributes
        // Archive bit (active high)
        if(entry.Protection.HasFlag(ProtectionBits.Archive)) stat.Attributes |= FileAttributes.Archive;

        // Pure bit - maps to System (resident)
        if(entry.Protection.HasFlag(ProtectionBits.Pure)) stat.Attributes |= FileAttributes.System;

        // Calculate blocks used (for files)
        if(stat.Length > 0) stat.Blocks = (stat.Length + _blockSize - 1) / _blockSize;

        return stat;
    }

    /// <summary>Converts Amiga protection bits to Unix-style mode</summary>
    /// <param name="protect">Amiga protection bits</param>
    /// <returns>Unix-style mode</returns>
    static uint ProtectionToUnixMode(ProtectionBits protect)
    {
        // Amiga owner bits are active-low (0 = allowed), Unix are active-high
        uint mode = 0;

        // Owner permissions (active-low, so check if NOT set means allowed)
        if(!protect.HasFlag(ProtectionBits.Read)) // Read allowed
            mode |= 0x100;                        // S_IRUSR

        if(!protect.HasFlag(ProtectionBits.Write)) // Write allowed
            mode |= 0x080;                         // S_IWUSR

        if(!protect.HasFlag(ProtectionBits.Execute)) // Execute allowed
            mode |= 0x040;                           // S_IXUSR

        // Note: Group and Other permissions would require ExtendedProtectionBits
        // which are stored separately in ExtraFields. For basic PFS, we just
        // mirror owner permissions to group and other.
        if(!protect.HasFlag(ProtectionBits.Read))
        {
            mode |= 0x020; // S_IRGRP
            mode |= 0x004; // S_IROTH
        }

        if(!protect.HasFlag(ProtectionBits.Write))
        {
            mode |= 0x010; // S_IWGRP
            mode |= 0x002; // S_IWOTH
        }

        if(!protect.HasFlag(ProtectionBits.Execute))
        {
            mode |= 0x008; // S_IXGRP
            mode |= 0x001; // S_IXOTH
        }

        return mode;
    }
}