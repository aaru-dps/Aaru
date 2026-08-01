// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class MinixFS
{
    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "ReadLink: path='{0}'", path);

        // Get file stat to verify it's a symlink
        ErrorNumber errno = Stat(path, out FileEntryInfo stat);

        if(errno != ErrorNumber.NoError) return errno;

        if(!stat.Attributes.HasFlag(FileAttributes.Symlink)) return ErrorNumber.InvalidArgument;

        // Look up the file's inode
        errno = LookupFile(path, out uint inodeNumber);

        if(errno != ErrorNumber.NoError) return errno;

        // Read the inode
        errno = ReadInode(inodeNumber, out object inodeObj);

        if(errno != ErrorNumber.NoError) return errno;

        // Get file size and zones
        uint   size;
        uint[] zones;
        int    directZones;

        if(_version == FilesystemVersion.V1)
        {
            var inode = (V1DiskInode)inodeObj;
            size = inode.d1_size;

            zones = new uint[inode.d1_zone.Length];

            for(var i = 0; i < inode.d1_zone.Length; i++) zones[i] = inode.d1_zone[i];

            directZones = V1_NR_DZONES;
        }
        else
        {
            var inode = (V2DiskInode)inodeObj;
            size        = inode.d2_size;
            zones       = inode.d2_zone;
            directZones = V2_NR_DZONES;
        }

        if(size == 0)
        {
            dest = "";

            return ErrorNumber.NoError;
        }

        // Read symlink data - symlinks are typically small, so read all data
        var linkData  = new byte[size];
        var bytesRead = 0;

        // Read direct zones
        for(var i = 0; i < directZones && bytesRead < size; i++)
        {
            if(zones[i] == 0)
            {
                // Sparse - shouldn't happen for symlinks but handle it
                int toFill = Math.Min(_blockSize, (int)(size - bytesRead));
                bytesRead += toFill;

                continue;
            }

            errno = ReadBlock((int)zones[i], out byte[] blockData);

            if(errno != ErrorNumber.NoError) return errno;

            int toCopy = Math.Min(blockData.Length, (int)(size - bytesRead));
            Array.Copy(blockData, 0, linkData, bytesRead, toCopy);
            bytesRead += toCopy;
        }

        // For longer symlinks, read indirect zones (rare but possible)
        if(bytesRead < size && directZones < zones.Length && zones[directZones] != 0)
        {
            errno = ReadIndirectZone(zones[directZones], ref linkData, ref bytesRead, (int)size, 1);

            if(errno != ErrorNumber.NoError) return errno;
        }

        dest = _encoding.GetString(linkData, 0, (int)size).TrimEnd('\0');

        AaruLogging.Debug(MODULE_NAME, "ReadLink: target='{0}'", dest);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Stat: path='{0}'", path);

        // Normalize path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory handling
        if(normalizedPath == "/" || string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
        {
            // Read root inode
            ErrorNumber errno = ReadInode(ROOT_INODE, out object rootInodeObj);

            if(errno != ErrorNumber.NoError) return errno;

            stat = InodeToFileEntryInfo(rootInodeObj, ROOT_INODE);

            return ErrorNumber.NoError;
        }

        // Remove leading slash
        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        // Start from root directory cache
        Dictionary<string, uint> currentEntries = _rootDirectoryCache;

        // Traverse path to find the file
        for(var p = 0; p < pathComponents.Length; p++)
        {
            string component = pathComponents[p];

            // Skip "." and ".."
            if(component is "." or "..") continue;

            // Find the component in current directory
            if(!currentEntries.TryGetValue(component, out uint inodeNumber))
            {
                AaruLogging.Debug(MODULE_NAME, "Stat: Component '{0}' not found", component);

                return ErrorNumber.NoSuchFile;
            }

            // Read the inode
            ErrorNumber errno = ReadInode(inodeNumber, out object inodeObj);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Stat: Error reading inode {0}: {1}", inodeNumber, errno);

                return errno;
            }

            // If this is the last component, return its stat
            if(p == pathComponents.Length - 1)
            {
                stat = InodeToFileEntryInfo(inodeObj, inodeNumber);

                return ErrorNumber.NoError;
            }

            // Not the last component - must be a directory
            ushort mode = _version == FilesystemVersion.V1
                              ? ((V1DiskInode)inodeObj).d1_mode
                              : ((V2DiskInode)inodeObj).d2_mode;

            if((mode & (ushort)InodeMode.TypeMask) != (ushort)InodeMode.Directory)
            {
                AaruLogging.Debug(MODULE_NAME, "Stat: '{0}' is not a directory", component);

                return ErrorNumber.NotDirectory;
            }

            // Read directory contents for next iteration
            errno = GetDirectoryContents(inodeNumber, out Dictionary<string, uint> dirEntries);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Stat: Error reading directory contents: {0}", errno);

                return errno;
            }

            currentEntries = dirEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(string.IsNullOrEmpty(path) || path == "/") return ErrorNumber.IsDirectory;

        AaruLogging.Debug(MODULE_NAME, "OpenFile: path='{0}'", path);

        // Get file stat to verify it exists and determine its type
        ErrorNumber errno = Stat(path, out FileEntryInfo stat);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: Stat failed with {0}", errno);

            return errno;
        }

        // Check it's not a directory
        if(stat.Attributes.HasFlag(FileAttributes.Directory))
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: path is a directory");

            return ErrorNumber.IsDirectory;
        }

        // Look up the file's inode
        errno = LookupFile(path, out uint inodeNumber);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: LookupFile failed with {0}", errno);

            return errno;
        }

        // Read the inode
        errno = ReadInode(inodeNumber, out object inodeObj);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: ReadInode failed with {0}", errno);

            return errno;
        }

        // Extract zone pointers and file size
        uint[] zones;
        uint   size;
        int    directZones;

        if(_version == FilesystemVersion.V1)
        {
            var inode = (V1DiskInode)inodeObj;
            size = inode.d1_size;

            // Convert ushort[] to uint[]
            zones = new uint[inode.d1_zone.Length];

            for(var i = 0; i < inode.d1_zone.Length; i++) zones[i] = inode.d1_zone[i];

            directZones = V1_NR_DZONES;
        }
        else
        {
            var inode = (V2DiskInode)inodeObj;
            size        = inode.d2_size;
            zones       = inode.d2_zone;
            directZones = V2_NR_DZONES;
        }

        node = new MinixFileNode
        {
            Path        = path,
            Length      = size,
            Offset      = 0,
            InodeNumber = inodeNumber,
            Zones       = zones,
            DirectZones = directZones
        };

        AaruLogging.Debug(MODULE_NAME, "OpenFile: success, size={0}", size);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not MinixFileNode) return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not MinixFileNode fileNode) return ErrorNumber.InvalidArgument;

        if(buffer == null) return ErrorNumber.InvalidArgument;

        if(fileNode.Offset < 0 || fileNode.Offset >= fileNode.Length) return ErrorNumber.InvalidArgument;

        // Clamp read length to remaining file size and buffer size
        long toRead = length;

        if(fileNode.Offset + toRead > fileNode.Length) toRead = fileNode.Length - fileNode.Offset;

        if(toRead <= 0) return ErrorNumber.NoError;

        if(toRead > buffer.Length) toRead = buffer.Length;

        AaruLogging.Debug(MODULE_NAME, "ReadFile: offset={0}, length={1}, toRead={2}", fileNode.Offset, length, toRead);

        long bytesRead     = 0;
        long currentOffset = fileNode.Offset;

        while(bytesRead < toRead)
        {
            // Calculate logical block number within the file
            var logicalBlock = (int)(currentOffset / _blockSize);

            // Map logical block to physical block (zone)
            ErrorNumber errno = ReadMap(fileNode.Zones, fileNode.DirectZones, logicalBlock, out int physicalBlock);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadFile: ReadMap failed for block {0}: {1}", logicalBlock, errno);

                // If we've read some data, return what we have
                if(bytesRead > 0) break;

                return errno;
            }

            // Calculate offset within the block
            var offsetInBlock = (int)(currentOffset % _blockSize);

            byte[] blockData;

            if(physicalBlock == 0)
            {
                // Sparse file hole - return zeros
                blockData = new byte[_blockSize];
            }
            else
            {
                // Read the block
                errno = ReadBlock(physicalBlock, out blockData);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "ReadFile: ReadBlock failed for block {0}: {1}",
                                      physicalBlock,
                                      errno);

                    // If we've read some data, return what we have
                    if(bytesRead > 0) break;

                    return errno;
                }
            }

            // Calculate how much data to copy from this block
            long bytesToCopy = Math.Min(_blockSize - offsetInBlock, toRead - bytesRead);
            Array.Copy(blockData, offsetInBlock, buffer, bytesRead, bytesToCopy);

            bytesRead     += bytesToCopy;
            currentOffset += bytesToCopy;
        }

        read            =  bytesRead;
        fileNode.Offset += bytesRead;

        AaruLogging.Debug(MODULE_NAME, "ReadFile: read {0} bytes, new offset={1}", read, fileNode.Offset);

        return ErrorNumber.NoError;
    }

    /// <summary>Converts an inode to a FileEntryInfo structure</summary>
    /// <param name="inodeObj">The inode object (V1DiskInode or V2DiskInode)</param>
    /// <param name="inodeNumber">The inode number</param>
    /// <returns>The FileEntryInfo structure</returns>
    FileEntryInfo InodeToFileEntryInfo(object inodeObj, uint inodeNumber)
    {
        var info = new FileEntryInfo
        {
            Attributes = FileAttributes.None,
            BlockSize  = _blockSize,
            Inode      = inodeNumber
        };

        ushort mode;
        uint   size;
        ushort uid;
        ushort gid;
        byte   nlinks;
        uint   mtime;
        uint   atime;
        uint   ctime;

        if(_version == FilesystemVersion.V1)
        {
            var inode = (V1DiskInode)inodeObj;
            mode   = inode.d1_mode;
            size   = inode.d1_size;
            uid    = inode.d1_uid;
            gid    = inode.d1_gid;
            nlinks = inode.d1_nlinks;
            mtime  = inode.d1_mtime;

            // V1 only has mtime
            atime = mtime;
            ctime = mtime;
        }
        else
        {
            var inode = (V2DiskInode)inodeObj;
            mode   = inode.d2_mode;
            size   = inode.d2_size;
            uid    = (ushort)inode.d2_uid;
            gid    = inode.d2_gid;
            nlinks = (byte)inode.d2_nlinks;
            mtime  = inode.d2_mtime;
            atime  = inode.d2_atime;
            ctime  = inode.d2_ctime;
        }

        info.Length = size;
        info.Links  = nlinks;
        info.UID    = uid;
        info.GID    = gid;
        info.Mode   = (uint)(mode & (ushort)InodeMode.PermissionMask);

        // Set timestamps
        info.LastWriteTimeUtc    = DateHandlers.UnixToDateTime(mtime);
        info.AccessTimeUtc       = DateHandlers.UnixToDateTime(atime);
        info.StatusChangeTimeUtc = DateHandlers.UnixToDateTime(ctime);
        info.CreationTimeUtc     = DateHandlers.UnixToDateTime(ctime);

        // Determine file type from mode
        var fileType = (InodeMode)(mode & (ushort)InodeMode.TypeMask);

        switch(fileType)
        {
            case InodeMode.Directory:
                info.Attributes = FileAttributes.Directory;

                break;
            case InodeMode.Regular:
                info.Attributes = FileAttributes.File;

                break;
            case InodeMode.SymbolicLink:
                info.Attributes = FileAttributes.Symlink;

                break;
            case InodeMode.CharDevice:
                info.Attributes = FileAttributes.CharDevice;

                // Extract device numbers from first zone
                // V1/V2: Use old_decode_dev() style - only low 16 bits (major=bits 8-15, minor=bits 0-7)
                // V3: Use raw 32-bit value like Minix MFS does (major=bits 8-15, minor=bits 0-7 of full value)
                if(_version == FilesystemVersion.V1)
                {
                    var inode = (V1DiskInode)inodeObj;

                    if(inode.d1_zone is { Length: > 0 })
                    {
                        uint dev   = inode.d1_zone[0];
                        uint major = dev >> 8 & 0xFF;
                        uint minor = dev      & 0xFF;
                        info.DeviceNo = (ulong)major << 32 | minor;
                    }
                }
                else
                {
                    var inode = (V2DiskInode)inodeObj;

                    if(inode.d2_zone is { Length: > 0 })
                    {
                        uint dev = inode.d2_zone[0];

                        // V2: old_decode_dev() only uses low 16 bits
                        // V3: Minix MFS uses raw 32-bit value directly as dev_t
                        if(_version == FilesystemVersion.V2) dev &= 0xFFFF;

                        uint major = dev >> 8 & 0xFF;
                        uint minor = dev      & 0xFF;
                        info.DeviceNo = (ulong)major << 32 | minor;
                    }
                }

                break;
            case InodeMode.BlockDevice:
                info.Attributes = FileAttributes.BlockDevice;

                // Extract device numbers from first zone
                // V1/V2: Use old_decode_dev() style - only low 16 bits (major=bits 8-15, minor=bits 0-7)
                // V3: Use raw 32-bit value like Minix MFS does (major=bits 8-15, minor=bits 0-7 of full value)
                if(_version == FilesystemVersion.V1)
                {
                    var inode = (V1DiskInode)inodeObj;

                    if(inode.d1_zone is { Length: > 0 })
                    {
                        uint dev   = inode.d1_zone[0];
                        uint major = dev >> 8 & 0xFF;
                        uint minor = dev      & 0xFF;
                        info.DeviceNo = (ulong)major << 32 | minor;
                    }
                }
                else
                {
                    var inode = (V2DiskInode)inodeObj;

                    if(inode.d2_zone is { Length: > 0 })
                    {
                        uint dev = inode.d2_zone[0];

                        // V2: old_decode_dev() only uses low 16 bits
                        // V3: Minix MFS uses raw 32-bit value directly as dev_t
                        if(_version == FilesystemVersion.V2) dev &= 0xFFFF;

                        uint major = dev >> 8 & 0xFF;
                        uint minor = dev      & 0xFF;
                        info.DeviceNo = (ulong)major << 32 | minor;
                    }
                }

                break;
            case InodeMode.Fifo:
                info.Attributes = FileAttributes.Pipe;

                break;
            case InodeMode.Socket:
                info.Attributes = FileAttributes.Socket;

                break;
            default:
                info.Attributes = FileAttributes.File;

                break;
        }

        // Calculate blocks (size / block size, rounded up)
        info.Blocks = (size + _blockSize - 1) / _blockSize;

        return info;
    }

    /// <summary>Looks up a file by path and returns its inode number</summary>
    /// <param name="path">Path to the file</param>
    /// <param name="inodeNumber">The inode number of the file</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber LookupFile(string path, out uint inodeNumber)
    {
        inodeNumber = 0;

        // Normalize path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        if(normalizedPath == "/")
        {
            inodeNumber = ROOT_INODE;

            return ErrorNumber.NoError;
        }

        // Remove leading slash
        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        // Start from root directory cache
        Dictionary<string, uint> currentEntries = _rootDirectoryCache;

        // Traverse path to find the file
        for(var p = 0; p < pathComponents.Length; p++)
        {
            string component = pathComponents[p];

            // Skip "." and ".."
            if(component is "." or "..") continue;

            // Find the component in current directory
            if(!currentEntries.TryGetValue(component, out uint foundInodeNumber)) return ErrorNumber.NoSuchFile;

            // If this is the last component, we found it
            if(p == pathComponents.Length - 1)
            {
                inodeNumber = foundInodeNumber;

                return ErrorNumber.NoError;
            }

            // Not the last component - read directory contents for next iteration
            ErrorNumber errno = ReadInode(foundInodeNumber, out object inodeObj);

            if(errno != ErrorNumber.NoError) return errno;

            ushort mode = _version == FilesystemVersion.V1
                              ? ((V1DiskInode)inodeObj).d1_mode
                              : ((V2DiskInode)inodeObj).d2_mode;

            if((mode & (ushort)InodeMode.TypeMask) != (ushort)InodeMode.Directory) return ErrorNumber.NotDirectory;

            errno = GetDirectoryContents(foundInodeNumber, out Dictionary<string, uint> dirEntries);

            if(errno != ErrorNumber.NoError) return errno;

            currentEntries = dirEntries;
        }

        return ErrorNumber.NoSuchFile;
    }
}