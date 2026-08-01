// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser filesystem plugin
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
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class Reiser
{
    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Stat: path='{0}'", path);

        // Normalize path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory
        if(normalizedPath is "/") return ReadStatData(REISERFS_ROOT_PARENT_OBJECTID, REISERFS_ROOT_OBJECTID, out stat);

        // Split path into components
        string stripped = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                              ? normalizedPath[1..]
                              : normalizedPath;

        string[] components = stripped.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(components.Length == 0) return ErrorNumber.InvalidArgument;

        // Start from root directory cache
        Dictionary<string, (uint dirId, uint objectId)> currentEntries = _rootDirectoryCache;

        for(var i = 0; i < components.Length; i++)
        {
            string component = components[i];

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out (uint dirId, uint objectId) target))
            {
                AaruLogging.Debug(MODULE_NAME, "Stat: '{0}' not found", component);

                return ErrorNumber.NoSuchFile;
            }

            // Last component — stat this object
            if(i == components.Length - 1) return ReadStatData(target.dirId, target.objectId, out stat);

            // Intermediate component — must be a directory, descend into it
            ErrorNumber errno = ReadObjectMode(target.dirId, target.objectId, out ushort mode);

            if(errno != ErrorNumber.NoError) return errno;

            if((mode & S_IFMT) != S_IFDIR)
            {
                AaruLogging.Debug(MODULE_NAME, "Stat: '{0}' is not a directory", component);

                return ErrorNumber.NotDirectory;
            }

            errno = GetDirectoryEntries(target.dirId,
                                        target.objectId,
                                        out Dictionary<string, (uint dirId, uint objectId)> subEntries);

            if(errno != ErrorNumber.NoError) return errno;

            // No need to filter "." and ".." here: they are already skipped above before the
            // lookup, so leaving them in the cached dictionary is harmless and avoids an O(n)
            // copy of every directory's entries on every single intermediate path component.
            currentEntries = subEntries;
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

        // Resolve object
        ErrorNumber errno = ResolvePath(path, out uint dirId, out uint objectId);

        if(errno != ErrorNumber.NoError) return errno;

        // Read stat data to get size and verify it's not a directory
        errno = ReadStatData(dirId, objectId, out FileEntryInfo stat);

        if(errno != ErrorNumber.NoError) return errno;

        if(stat.Attributes.HasFlag(FileAttributes.Directory)) return ErrorNumber.IsDirectory;

        node = new ReiserFileNode
        {
            Path     = path,
            Length   = stat.Length,
            Offset   = 0,
            DirId    = dirId,
            ObjectId = objectId
        };

        AaruLogging.Debug(MODULE_NAME, "OpenFile: success, size={0}", stat.Length);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not ReiserFileNode) return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "ReadLink: path='{0}'", path);

        // Resolve the path
        ErrorNumber errno = ResolvePath(path, out uint dirId, out uint objectId);

        if(errno != ErrorNumber.NoError) return errno;

        // Verify it's a symlink
        errno = ReadObjectMode(dirId, objectId, out ushort mode);

        if(errno != ErrorNumber.NoError) return errno;

        if((mode & S_IFMT) != S_IFLNK) return ErrorNumber.InvalidArgument;

        // Symlinks store their target as regular file data — always small
        errno = ReadFileData(dirId, objectId, out byte[] linkData);

        if(errno != ErrorNumber.NoError) return errno;

        dest = _encoding.GetString(linkData).TrimEnd('\0');

        AaruLogging.Debug(MODULE_NAME, "ReadLink: target='{0}'", dest);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not ReiserFileNode fileNode) return ErrorNumber.InvalidArgument;

        if(buffer == null) return ErrorNumber.InvalidArgument;

        if(fileNode.Offset < 0 || fileNode.Offset >= fileNode.Length) return ErrorNumber.InvalidArgument;

        // Clamp to remaining file size and buffer capacity
        long toRead = length;

        if(fileNode.Offset + toRead > fileNode.Length) toRead = fileNode.Length - fileNode.Offset;

        if(toRead <= 0) return ErrorNumber.NoError;

        if(toRead > buffer.Length) toRead = buffer.Length;

        AaruLogging.Debug(MODULE_NAME, "ReadFile: offset={0}, length={1}, toRead={2}", fileNode.Offset, length, toRead);

        long bytesRead     = 0;
        long currentOffset = fileNode.Offset;

        while(bytesRead < toRead)
        {
            // ReiserFS file data offsets are 1-based in the key
            ulong keyOffset = (ulong)currentOffset + 1;

            // First try to find a direct item at this offset
            ErrorNumber errno = SearchByKey(fileNode.DirId,
                                            fileNode.ObjectId,
                                            keyOffset,
                                            TYPE_DIRECT,
                                            out byte[] leaf,
                                            out int index);

            if(errno == ErrorNumber.NoError && index >= 0)
            {
                ItemHead ih        = ReadItemHead(leaf, BLKH_SIZE + index * IH_SIZE);
                int      ihVersion = GetItemKeyVersion(ih.ih_version);
                int      ihType    = GetKeyType(ih.ih_key, ihVersion);

                if(ih.ih_key.k_dir_id                   == fileNode.DirId    &&
                   ih.ih_key.k_objectid                 == fileNode.ObjectId &&
                   ihType                               == TYPE_DIRECT       &&
                   ih.ih_item_location + ih.ih_item_len <= leaf.Length)
                {
                    ulong itemOffset = GetKeyOffset(ih.ih_key, ihVersion);
                    var   filePos    = (long)(itemOffset - 1);

                    // Calculate where we are within this item
                    var skipInItem = (int)(currentOffset - filePos);

                    if(skipInItem >= 0 && skipInItem < ih.ih_item_len)
                    {
                        int available = ih.ih_item_len - skipInItem;
                        var toCopy    = (int)Math.Min(available, toRead - bytesRead);
                        Array.Copy(leaf, ih.ih_item_location + skipInItem, buffer, bytesRead, toCopy);
                        bytesRead     += toCopy;
                        currentOffset += toCopy;

                        continue;
                    }
                }
            }

            // Try indirect item (array of block pointers)
            errno = SearchByKey(fileNode.DirId, fileNode.ObjectId, keyOffset, TYPE_INDIRECT, out leaf, out index);

            if(errno == ErrorNumber.NoError && index >= 0)
            {
                ItemHead ih        = ReadItemHead(leaf, BLKH_SIZE + index * IH_SIZE);
                int      ihVersion = GetItemKeyVersion(ih.ih_version);
                int      ihType    = GetKeyType(ih.ih_key, ihVersion);

                if(ih.ih_key.k_dir_id                   == fileNode.DirId    &&
                   ih.ih_key.k_objectid                 == fileNode.ObjectId &&
                   ihType                               == TYPE_INDIRECT     &&
                   ih.ih_item_location + ih.ih_item_len <= leaf.Length)
                {
                    ulong itemOffset = GetKeyOffset(ih.ih_key, ihVersion);
                    var   filePos    = (long)(itemOffset - 1);
                    int   ptrCount   = ih.ih_item_len / 4;

                    // Which block pointer within this item covers currentOffset?
                    long offsetInItem = currentOffset - filePos;

                    if(offsetInItem >= 0)
                    {
                        var ptrIndex = (int)(offsetInItem / _blockSize);

                        while(ptrIndex < ptrCount && bytesRead < toRead)
                        {
                            var blockPtr = BitConverter.ToUInt32(leaf, ih.ih_item_location + ptrIndex * 4);

                            byte[] blockData;

                            if(blockPtr == 0)
                            {
                                // Sparse hole — return zeros
                                blockData = new byte[_blockSize];
                            }
                            else
                            {
                                ErrorNumber blkErr = ReadBlock(blockPtr, out blockData);

                                if(blkErr != ErrorNumber.NoError)
                                {
                                    if(bytesRead > 0) break;

                                    return blkErr;
                                }
                            }

                            var offsetInBlock = (int)(currentOffset - (filePos + ptrIndex * (long)_blockSize));
                            int available     = _blockSize - offsetInBlock;
                            var toCopy        = (int)Math.Min(available, toRead - bytesRead);
                            Array.Copy(blockData, offsetInBlock, buffer, bytesRead, toCopy);
                            bytesRead     += toCopy;
                            currentOffset += toCopy;
                            ptrIndex++;
                        }

                        continue;
                    }
                }
            }

            // No data item found — end of readable data
            break;
        }

        read            =  bytesRead;
        fileNode.Offset += bytesRead;

        AaruLogging.Debug(MODULE_NAME, "ReadFile: read {0} bytes, new offset={1}", read, fileNode.Offset);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the stat data for an object and returns a populated FileEntryInfo</summary>
    /// <param name="dirId">Directory (packing locality) id</param>
    /// <param name="objectId">Object id</param>
    /// <param name="stat">The populated file entry information</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadStatData(uint dirId, uint objectId, out FileEntryInfo stat)
    {
        (uint dirId, uint objectId) key = (dirId, objectId);

        if(_statCache.TryGetValue(key, out stat)) return ErrorNumber.NoError;

        ErrorNumber cacheErrno = ReadStatDataUncached(dirId, objectId, out stat);

        if(cacheErrno == ErrorNumber.NoError) _statCache[key] = stat;

        return cacheErrno;
    }

    /// <summary>Reads the stat data for an object and returns a populated FileEntryInfo</summary>
    /// <param name="dirId">Directory (packing locality) id</param>
    /// <param name="objectId">Object id</param>
    /// <param name="stat">The populated file entry information</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadStatDataUncached(uint dirId, uint objectId, out FileEntryInfo stat)
    {
        stat = null;

        ErrorNumber errno = SearchByKey(dirId, objectId, 0, TYPE_STAT_DATA, out byte[] leaf, out int index);

        if(errno != ErrorNumber.NoError) return errno;

        if(index < 0) return ErrorNumber.NoSuchFile;

        ItemHead ih  = ReadItemHead(leaf, BLKH_SIZE + index * IH_SIZE);
        int      ver = GetItemKeyVersion(ih.ih_version);

        if(ih.ih_item_location + ih.ih_item_len > leaf.Length) return ErrorNumber.InvalidArgument;

        stat = new FileEntryInfo
        {
            Attributes = FileAttributes.None,
            BlockSize  = _blockSize,
            Inode      = objectId
        };

        if(ver == KEY_FORMAT_3_6 && ih.ih_item_len >= Marshal.SizeOf<StatDataV2>())
        {
            StatDataV2 sd =
                Marshal.ByteArrayToStructureLittleEndian<StatDataV2>(leaf,
                                                                     ih.ih_item_location,
                                                                     Marshal.SizeOf<StatDataV2>());

            stat.Mode   = (uint)(sd.sd_mode & 0x0FFF);
            stat.Length = (long)sd.sd_size;
            stat.Links  = sd.sd_nlink;
            stat.UID    = sd.sd_uid;
            stat.GID    = sd.sd_gid;
            stat.Blocks = sd.sd_blocks;

            stat.AccessTimeUtc       = DateHandlers.UnixUnsignedToDateTime(sd.sd_atime);
            stat.LastWriteTimeUtc    = DateHandlers.UnixUnsignedToDateTime(sd.sd_mtime);
            stat.StatusChangeTimeUtc = DateHandlers.UnixUnsignedToDateTime(sd.sd_ctime);

            stat.Attributes = ModeToAttributes(sd.sd_mode);

            // Map persistent inode flags from sd_attrs
            if((sd.sd_attrs & REISERFS_IMMUTABLE_FL) != 0) stat.Attributes |= FileAttributes.Immutable;
            if((sd.sd_attrs & REISERFS_APPEND_FL)    != 0) stat.Attributes |= FileAttributes.AppendOnly;
            if((sd.sd_attrs & REISERFS_SYNC_FL)      != 0) stat.Attributes |= FileAttributes.Sync;
            if((sd.sd_attrs & REISERFS_NOATIME_FL)   != 0) stat.Attributes |= FileAttributes.NoAccessTime;
            if((sd.sd_attrs & REISERFS_NODUMP_FL)    != 0) stat.Attributes |= FileAttributes.NoDump;

            // Device files
            var fileType = (ushort)(sd.sd_mode & S_IFMT);

            if(fileType is S_IFCHR or S_IFBLK) stat.DeviceNo = sd.sd_rdev_or_generation;
        }
        else
        {
            StatDataV1 sd =
                Marshal.ByteArrayToStructureLittleEndian<StatDataV1>(leaf,
                                                                     ih.ih_item_location,
                                                                     Marshal.SizeOf<StatDataV1>());

            stat.Mode   = (uint)(sd.sd_mode & 0x0FFF);
            stat.Length = sd.sd_size;
            stat.Links  = sd.sd_nlink;
            stat.UID    = sd.sd_uid;
            stat.GID    = sd.sd_gid;

            stat.AccessTimeUtc       = DateHandlers.UnixUnsignedToDateTime(sd.sd_atime);
            stat.LastWriteTimeUtc    = DateHandlers.UnixUnsignedToDateTime(sd.sd_mtime);
            stat.StatusChangeTimeUtc = DateHandlers.UnixUnsignedToDateTime(sd.sd_ctime);

            stat.Attributes = ModeToAttributes(sd.sd_mode);

            // Device files
            var fileType = (ushort)(sd.sd_mode & S_IFMT);

            if(fileType is S_IFCHR or S_IFBLK) stat.DeviceNo = sd.sd_rdev_or_blocks;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Converts a POSIX mode to Aaru FileAttributes</summary>
    static FileAttributes ModeToAttributes(ushort mode)
    {
        var fileType = (ushort)(mode & S_IFMT);

        return fileType switch
               {
                   S_IFDIR  => FileAttributes.Directory,
                   S_IFREG  => FileAttributes.File,
                   S_IFLNK  => FileAttributes.Symlink,
                   S_IFCHR  => FileAttributes.CharDevice,
                   S_IFBLK  => FileAttributes.BlockDevice,
                   S_IFIFO  => FileAttributes.FIFO,
                   S_IFSOCK => FileAttributes.Socket,
                   _        => FileAttributes.None
               };
    }

    /// <summary>Looks up an object's (dirId, objectId) by traversing a path from a given directory</summary>
    /// <param name="pathComponents">Path components to traverse</param>
    /// <param name="startEntries">Directory entries to start from</param>
    /// <param name="dirId">Resolved directory (packing locality) id</param>
    /// <param name="objectId">Resolved object id</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber LookupObject(string[] pathComponents, Dictionary<string, (uint dirId, uint objectId)> startEntries,
                             out uint dirId,          out uint                                        objectId)
    {
        dirId    = 0;
        objectId = 0;

        Dictionary<string, (uint dirId, uint objectId)> currentEntries = startEntries;

        for(var i = 0; i < pathComponents.Length; i++)
        {
            string component = pathComponents[i];

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out (uint dirId, uint objectId) target))
                return ErrorNumber.NoSuchFile;

            // Last component — found it
            if(i == pathComponents.Length - 1)
            {
                dirId    = target.dirId;
                objectId = target.objectId;

                return ErrorNumber.NoError;
            }

            // Intermediate — must be a directory
            ErrorNumber errno = ReadObjectMode(target.dirId, target.objectId, out ushort mode);

            if(errno != ErrorNumber.NoError) return errno;

            if((mode & S_IFMT) != S_IFDIR) return ErrorNumber.NotDirectory;

            errno = GetDirectoryEntries(target.dirId,
                                        target.objectId,
                                        out Dictionary<string, (uint dirId, uint objectId)> subEntries);

            if(errno != ErrorNumber.NoError) return errno;

            currentEntries = subEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>Resolves a filesystem path to its (dirId, objectId) pair</summary>
    /// <param name="path">The path to resolve</param>
    /// <param name="dirId">Resolved directory (packing locality) id</param>
    /// <param name="objectId">Resolved object id</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ResolvePath(string path, out uint dirId, out uint objectId)
    {
        dirId    = 0;
        objectId = 0;

        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory
        if(normalizedPath is "/")
        {
            dirId    = REISERFS_ROOT_PARENT_OBJECTID;
            objectId = REISERFS_ROOT_OBJECTID;

            return ErrorNumber.NoError;
        }

        if(_pathCache.TryGetValue(normalizedPath, out (uint dirId, uint objectId) cached))
        {
            dirId    = cached.dirId;
            objectId = cached.objectId;

            return ErrorNumber.NoError;
        }

        string stripped = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                              ? normalizedPath[1..]
                              : normalizedPath;

        string[] components = stripped.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(components.Length == 0) return ErrorNumber.InvalidArgument;

        ErrorNumber errno = LookupObject(components, _rootDirectoryCache, out dirId, out objectId);

        if(errno == ErrorNumber.NoError) _pathCache[normalizedPath] = (dirId, objectId);

        return errno;
    }

    /// <summary>Reads the generation number from a v2 object's stat data</summary>
    /// <param name="dirId">Directory (packing locality) id</param>
    /// <param name="objectId">Object id</param>
    /// <param name="generation">The generation number, or 0 if unavailable</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadObjectGeneration(uint dirId, uint objectId, out uint generation)
    {
        generation = 0;

        ErrorNumber errno = SearchByKey(dirId, objectId, 0, TYPE_STAT_DATA, out byte[] leaf, out int index);

        if(errno != ErrorNumber.NoError) return errno;

        if(index < 0) return ErrorNumber.NoSuchFile;

        ItemHead ih  = ReadItemHead(leaf, BLKH_SIZE + index * IH_SIZE);
        int      ver = GetItemKeyVersion(ih.ih_version);

        if(ih.ih_item_location + ih.ih_item_len > leaf.Length) return ErrorNumber.InvalidArgument;

        if(ver == KEY_FORMAT_3_6 && ih.ih_item_len >= Marshal.SizeOf<StatDataV2>())
        {
            StatDataV2 sd =
                Marshal.ByteArrayToStructureLittleEndian<StatDataV2>(leaf,
                                                                     ih.ih_item_location,
                                                                     Marshal.SizeOf<StatDataV2>());

            var fileType = (ushort)(sd.sd_mode & S_IFMT);

            // For device files the kernel uses k_dir_id as generation
            generation = fileType is S_IFCHR or S_IFBLK ? dirId : sd.sd_rdev_or_generation;
        }
        else
        {
            // v1 objects use k_dir_id as generation
            generation = dirId;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the entire contents of a file given its (dirId, objectId)</summary>
    /// <param name="dirId">Directory (packing locality) id</param>
    /// <param name="objectId">Object id</param>
    /// <param name="data">The file data</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadFileData(uint dirId, uint objectId, out byte[] data)
    {
        data = null;

        // Get file size from stat data
        ErrorNumber errno = ReadStatData(dirId, objectId, out FileEntryInfo stat);

        if(errno != ErrorNumber.NoError) return errno;

        if(stat.Length == 0)
        {
            data = [];

            return ErrorNumber.NoError;
        }

        data = new byte[stat.Length];
        long bytesRead = 0;

        // Read all data items for this object, starting at offset 1
        // (ReiserFS offsets are 1-based for file data)
        ulong currentOffset = 1;

        while(bytesRead < stat.Length)
        {
            // Try direct items first (inline data in leaf nodes)
            errno = SearchByKey(dirId, objectId, currentOffset, TYPE_DIRECT, out byte[] leaf, out int index);

            if(errno == ErrorNumber.NoError && index >= 0)
            {
                ItemHead ih        = ReadItemHead(leaf, BLKH_SIZE + index * IH_SIZE);
                int      ihVersion = GetItemKeyVersion(ih.ih_version);
                int      ihType    = GetKeyType(ih.ih_key, ihVersion);

                if(ih.ih_key.k_dir_id == dirId && ih.ih_key.k_objectid == objectId && ihType == TYPE_DIRECT)
                {
                    if(ih.ih_item_location + ih.ih_item_len <= leaf.Length)
                    {
                        ulong itemOffset = GetKeyOffset(ih.ih_key, ihVersion);

                        // ReiserFS file offsets are 1-based
                        var filePos = (long)(itemOffset - 1);

                        if(filePos >= 0 && filePos < stat.Length)
                        {
                            var toCopy = (int)Math.Min(ih.ih_item_len, stat.Length - filePos);
                            Array.Copy(leaf, ih.ih_item_location, data, filePos, toCopy);
                            bytesRead = Math.Max(bytesRead, filePos + toCopy);
                        }
                    }

                    currentOffset = GetKeyOffset(ih.ih_key, ihVersion) + ih.ih_item_len;

                    continue;
                }
            }

            // Try indirect items (array of block pointers)
            errno = SearchByKey(dirId, objectId, currentOffset, TYPE_INDIRECT, out leaf, out index);

            if(errno == ErrorNumber.NoError && index >= 0)
            {
                ItemHead ih        = ReadItemHead(leaf, BLKH_SIZE + index * IH_SIZE);
                int      ihVersion = GetItemKeyVersion(ih.ih_version);
                int      ihType    = GetKeyType(ih.ih_key, ihVersion);

                if(ih.ih_key.k_dir_id == dirId && ih.ih_key.k_objectid == objectId && ihType == TYPE_INDIRECT)
                {
                    if(ih.ih_item_location + ih.ih_item_len <= leaf.Length)
                    {
                        ulong itemOffset = GetKeyOffset(ih.ih_key, ihVersion);
                        var   filePos    = (long)(itemOffset - 1);
                        int   ptrCount   = ih.ih_item_len / 4;

                        for(var p = 0; p < ptrCount && filePos + p * _blockSize < stat.Length; p++)
                        {
                            var blockPtr = BitConverter.ToUInt32(leaf, ih.ih_item_location + p * 4);

                            if(blockPtr == 0) continue;

                            ErrorNumber blkErr = ReadBlock(blockPtr, out byte[] blockData);

                            if(blkErr != ErrorNumber.NoError) continue;

                            long destPos = filePos + p * _blockSize;
                            var  toCopy  = (int)Math.Min(_blockSize, stat.Length - destPos);

                            if(destPos < 0 || destPos >= stat.Length) continue;

                            Array.Copy(blockData, 0, data, destPos, toCopy);
                            bytesRead = Math.Max(bytesRead, destPos + toCopy);
                        }
                    }

                    currentOffset = GetKeyOffset(ih.ih_key, ihVersion) + (ulong)(ih.ih_item_len / 4 * _blockSize);

                    continue;
                }
            }

            // No more data items found
            break;
        }

        return ErrorNumber.NoError;
    }
}