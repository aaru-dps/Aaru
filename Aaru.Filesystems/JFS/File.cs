// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of IBM's Journaled File System</summary>
public sealed partial class JFS
{
    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "OpenFile: path='{0}'", path);

        // Resolve the path to its inode
        ErrorNumber errno = GetInodeForPath(path, out Inode inode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: GetInodeForPath failed with {0}", errno);

            return errno;
        }

        // Check if it's a directory
        if((inode.di_mode & 0xF000) == 0x4000)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: path is a directory");

            return ErrorNumber.IsDirectory;
        }

        // Must be a regular file
        if((inode.di_mode & 0xF000) != 0x8000)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: path is not a regular file (mode=0x{0:X4})", inode.di_mode);

            return ErrorNumber.InvalidArgument;
        }

        node = new JfsFileNode
        {
            Path   = path,
            Length = (long)inode.di_size,
            Offset = 0,
            Inode  = inode
        };

        AaruLogging.Debug(MODULE_NAME, "OpenFile: success, inode={0}, size={1}", inode.di_number, inode.di_size);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        return node is not JfsFileNode ? ErrorNumber.InvalidArgument : ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not JfsFileNode fileNode) return ErrorNumber.InvalidArgument;

        if(buffer is null) return ErrorNumber.InvalidArgument;

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
            // Calculate which filesystem block contains the current offset
            long logicalBlock  = currentOffset / _superblock.s_bsize;
            var  offsetInBlock = (int)(currentOffset % _superblock.s_bsize);

            // Map logical block to physical block using the xtree
            ErrorNumber errno = XTreeLookup(fileNode.Inode.di_u, false, logicalBlock, out long physicalBlock);

            if(errno != ErrorNumber.NoError)
            {
                // Sparse/hole — fill with zeros
                if(errno == ErrorNumber.InvalidArgument)
                {
                    long bytesToZero = Math.Min(_superblock.s_bsize - offsetInBlock, toRead - bytesRead);
                    Array.Clear(buffer, (int)bytesRead, (int)bytesToZero);
                    bytesRead     += bytesToZero;
                    currentOffset += bytesToZero;

                    continue;
                }

                AaruLogging.Debug(MODULE_NAME, "ReadFile: XTreeLookup failed for block {0}: {1}", logicalBlock, errno);

                break;
            }

            // physicalBlock == 0 means XAD_NOTRECORDED (sparse hole) — fill with zeros
            if(physicalBlock == 0)
            {
                long bytesToZero = Math.Min(_superblock.s_bsize - offsetInBlock, toRead - bytesRead);
                Array.Clear(buffer, (int)bytesRead, (int)bytesToZero);
                bytesRead     += bytesToZero;
                currentOffset += bytesToZero;

                continue;
            }

            // Read the physical block
            errno = ReadFsBlock(physicalBlock, out byte[] blockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadFile: ReadFsBlock failed for block {0}: {1}", physicalBlock, errno);

                break;
            }

            // Copy data from block to buffer
            long bytesToCopy = Math.Min(_superblock.s_bsize - offsetInBlock, toRead - bytesRead);
            Array.Copy(blockData, offsetInBlock, buffer, bytesRead, bytesToCopy);

            bytesRead     += bytesToCopy;
            currentOffset += bytesToCopy;
        }

        read            =  bytesRead;
        fileNode.Offset += bytesRead;

        AaruLogging.Debug(MODULE_NAME, "ReadFile: read {0} bytes, new offset={1}", read, fileNode.Offset);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "ReadLink: path='{0}'", path);

        // Resolve the path to its inode
        ErrorNumber errno = GetInodeForPath(path, out Inode inode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadLink: GetInodeForPath failed with {0}", errno);

            return errno;
        }

        // Verify it's a symbolic link (S_IFLNK = 0xA000)
        if((inode.di_mode & 0xF000) != 0xA000)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadLink: path is not a symbolic link (mode=0x{0:X4})", inode.di_mode);

            return ErrorNumber.InvalidArgument;
        }

        var size = (int)inode.di_size;

        if(size == 0)
        {
            dest = "";

            return ErrorNumber.NoError;
        }

        // Fast symlink: target stored inline in inode extension area
        if(size < IDATASIZE)
        {
            if(inode.di_u is null || inode.di_u.Length < FASTSYMLINK_OFFSET + size)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadLink: inode extension area too small for fast symlink");

                return ErrorNumber.InvalidArgument;
            }

            dest = _encoding.GetString(inode.di_u, FASTSYMLINK_OFFSET, size).TrimEnd('\0');

            AaruLogging.Debug(MODULE_NAME, "ReadLink: fast symlink target='{0}'", dest);

            return ErrorNumber.NoError;
        }

        // Regular symlink: target stored as file data, read via xtree
        var linkData      = new byte[size];
        var bytesRead     = 0;
        var currentOffset = 0;

        while(bytesRead < size)
        {
            long logicalBlock  = currentOffset / _superblock.s_bsize;
            var  offsetInBlock = (int)(currentOffset % _superblock.s_bsize);

            errno = XTreeLookup(inode.di_u, false, logicalBlock, out long physicalBlock);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadLink: XTreeLookup failed for block {0}: {1}", logicalBlock, errno);

                return errno;
            }

            errno = ReadFsBlock(physicalBlock, out byte[] blockData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadLink: ReadFsBlock failed for block {0}: {1}", physicalBlock, errno);

                return errno;
            }

            var bytesToCopy = (int)Math.Min(_superblock.s_bsize - offsetInBlock, size - bytesRead);
            Array.Copy(blockData, offsetInBlock, linkData, bytesRead, bytesToCopy);

            bytesRead     += bytesToCopy;
            currentOffset += bytesToCopy;
        }

        dest = _encoding.GetString(linkData, 0, size).TrimEnd('\0');

        AaruLogging.Debug(MODULE_NAME, "ReadLink: symlink target='{0}'", dest);

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

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory handling
        if(normalizedPath == "/")
        {
            ErrorNumber rootErrno = GetFilesetInode(ROOT_I, out Inode rootInode);

            if(rootErrno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Stat: error reading root inode: {0}", rootErrno);

                return rootErrno;
            }

            stat = InodeToFileEntryInfo(rootInode, ROOT_I);

            return ErrorNumber.NoError;
        }

        // Remove leading slash
        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        ErrorNumber resolveErrno = ResolvePathToInode(pathWithoutLeadingSlash, out uint targetInodeNumber);

        if(resolveErrno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Stat: error resolving path: {0}", resolveErrno);

            return resolveErrno;
        }

        // Read the target inode
        ErrorNumber readErrno = GetFilesetInode(targetInodeNumber, out Inode targetInode);

        if(readErrno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Stat: error reading target inode {0}: {1}", targetInodeNumber, readErrno);

            return readErrno;
        }

        stat = InodeToFileEntryInfo(targetInode, targetInodeNumber);

        AaruLogging.Debug(MODULE_NAME,
                          "Stat: successful for '{0}': inode={1}, size={2}, mode=0x{3:X}",
                          path,
                          stat.Inode,
                          stat.Length,
                          stat.Mode);

        return ErrorNumber.NoError;
    }

    /// <summary>Converts a JFS on-disk inode to a FileEntryInfo structure</summary>
    /// <param name="inode">The JFS inode</param>
    /// <param name="inodeNumber">The inode number</param>
    /// <returns>The FileEntryInfo structure</returns>
    FileEntryInfo InodeToFileEntryInfo(in Inode inode, uint inodeNumber)
    {
        var info = new FileEntryInfo
        {
            Attributes          = FileAttributes.None,
            BlockSize           = _superblock.s_bsize,
            Inode               = inodeNumber,
            Length              = (long)inode.di_size,
            Links               = inode.di_nlink,
            UID                 = inode.di_uid,
            GID                 = inode.di_gid,
            Mode                = inode.di_mode & 0x0FFFu,
            Blocks              = (long)inode.di_nblocks,
            AccessTimeUtc       = DateHandlers.UnixUnsignedToDateTime(inode.di_atime.tv_sec, inode.di_atime.tv_nsec),
            StatusChangeTimeUtc = DateHandlers.UnixUnsignedToDateTime(inode.di_ctime.tv_sec, inode.di_ctime.tv_nsec),
            LastWriteTimeUtc    = DateHandlers.UnixUnsignedToDateTime(inode.di_mtime.tv_sec, inode.di_mtime.tv_nsec),
            CreationTimeUtc     = DateHandlers.UnixUnsignedToDateTime(inode.di_otime.tv_sec, inode.di_otime.tv_nsec)
        };

        uint mode = inode.di_mode;

        // Determine file type from di_mode (S_IFMT mask = 0xF000)
        info.Attributes = (mode & 0xF000) switch
                          {
                              0x4000 => FileAttributes.Directory,   // S_IFDIR
                              0x8000 => FileAttributes.File,        // S_IFREG
                              0xA000 => FileAttributes.Symlink,     // S_IFLNK
                              0x2000 => FileAttributes.CharDevice,  // S_IFCHR
                              0x6000 => FileAttributes.BlockDevice, // S_IFBLK
                              0x1000 => FileAttributes.FIFO,        // S_IFIFO
                              0xC000 => FileAttributes.Socket,      // S_IFSOCK
                              _      => FileAttributes.File
                          };

        // For char/block device inodes, extract di_rdev from extension area
        if(((mode & 0xF000) == 0x2000 || (mode & 0xF000) == 0x6000) && inode.di_u is { Length: >= RDEV_OFFSET + 4 })
        {
            uint rdev = BitConverter.ToUInt32(inode.di_u, RDEV_OFFSET);
            info.DeviceNo = rdev;
        }

        // Map OS/2 extended attributes from di_mode upper bits
        if((mode & IREADONLY) != 0) info.Attributes |= FileAttributes.ReadOnly;
        if((mode & IHIDDEN)   != 0) info.Attributes |= FileAttributes.Hidden;
        if((mode & ISYSTEM)   != 0) info.Attributes |= FileAttributes.System;
        if((mode & IARCHIVE)  != 0) info.Attributes |= FileAttributes.Archive;

        // Map Linux extended flags from di_mode
        if((mode & JFS_IMMUTABLE_FL) != 0) info.Attributes |= FileAttributes.Immutable;
        if((mode & JFS_APPEND_FL)    != 0) info.Attributes |= FileAttributes.AppendOnly;

        return info;
    }
}