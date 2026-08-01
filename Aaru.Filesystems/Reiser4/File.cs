// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser4 filesystem plugin
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

public sealed partial class Reiser4
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
        if(normalizedPath is "/")
        {
            LargeKey rootKey = BuildRootStatDataKey();

            return ReadStatData(rootKey, out stat);
        }

        // Resolve path to a stat-data key
        ErrorNumber errno = ResolvePath(normalizedPath, out LargeKey targetKey);

        if(errno != ErrorNumber.NoError) return errno;

        return ReadStatData(targetKey, out stat);
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(string.IsNullOrEmpty(path) || path == "/") return ErrorNumber.IsDirectory;

        AaruLogging.Debug(MODULE_NAME, "OpenFile: path='{0}'", path);

        // Resolve path to stat-data key
        ErrorNumber errno = ResolvePath(path, out LargeKey statDataKey);

        if(errno != ErrorNumber.NoError) return errno;

        // Read stat data to get size and verify it's not a directory
        errno = ReadStatData(statDataKey, out FileEntryInfo stat);

        if(errno != ErrorNumber.NoError) return errno;

        if(stat.Attributes.HasFlag(FileAttributes.Directory)) return ErrorNumber.IsDirectory;

        node = new Reiser4FileNode
        {
            Path        = path,
            Length      = stat.Length,
            Offset      = 0,
            StatDataKey = statDataKey
        };

        AaruLogging.Debug(MODULE_NAME, "OpenFile: success, size={0}", stat.Length);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not Reiser4FileNode) return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "ReadLink: path='{0}'", path);

        // Resolve the path
        ErrorNumber errno = ResolvePath(path, out LargeKey statDataKey);

        if(errno != ErrorNumber.NoError) return errno;

        // Verify it's a symlink
        errno = ReadObjectMode(statDataKey, out ushort mode);

        if(errno != ErrorNumber.NoError) return errno;

        if((mode & S_IFMT) != S_IFLNK) return ErrorNumber.InvalidArgument;

        // Check if symlink target is in the stat-data SYMLINK extension
        errno = ReadSymlinkFromStatData(statDataKey, out dest);

        if(errno == ErrorNumber.NoError && dest != null) return ErrorNumber.NoError;

        // Otherwise read file body
        errno = ReadAllFileData(statDataKey, out byte[] linkData);

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

        if(node is not Reiser4FileNode fileNode) return ErrorNumber.InvalidArgument;

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
            // Build a file body key for the current offset
            LargeKey bodyKey = BuildFileBodyKey(fileNode.StatDataKey, (ulong)currentOffset);

            // First try to find a tail (formatting) item at leaf level
            ErrorNumber errno = SearchByKey(bodyKey, out byte[] leafData, out int itemPos);

            if(errno == ErrorNumber.NoError && itemPos >= 0)
            {
                Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(leafData);

                if(itemPos < nh.nr_items)
                {
                    ReadItemHeader(leafData,
                                   itemPos,
                                   nh.nr_items,
                                   out LargeKey itemKey,
                                   out ushort bodyOff,
                                   out _,
                                   out ushort pluginId);

                    if(pluginId == FORMATTING_ID && KeyMatchesFileBody(itemKey, fileNode.StatDataKey))
                    {
                        int itemLen = GetItemLength(leafData, itemPos, nh.nr_items, nh.free_space_start);

                        ulong itemOffset = GetKeyOffset(itemKey);

                        // Calculate position within this tail item
                        var skipInItem = (int)((ulong)currentOffset - itemOffset);

                        if(skipInItem >= 0 && skipInItem < itemLen)
                        {
                            int available = itemLen - skipInItem;
                            var toCopy    = (int)Math.Min(available, toRead - bytesRead);

                            Array.Copy(leafData, bodyOff + skipInItem, buffer, bytesRead, toCopy);

                            bytesRead     += toCopy;
                            currentOffset += toCopy;

                            continue;
                        }
                    }
                }
            }

            // Try extent item at twig level
            errno = SearchByKey(bodyKey, out byte[] twigData, out itemPos, TWIG_LEVEL);

            if(errno == ErrorNumber.NoError && itemPos >= 0)
            {
                Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(twigData);

                if(itemPos < nh.nr_items)
                {
                    ReadItemHeader(twigData,
                                   itemPos,
                                   nh.nr_items,
                                   out LargeKey itemKey,
                                   out ushort bodyOff,
                                   out _,
                                   out ushort pluginId);

                    if(pluginId == EXTENT_POINTER_ID && KeyMatchesFileBody(itemKey, fileNode.StatDataKey))
                    {
                        int itemLen = GetItemLength(twigData, itemPos, nh.nr_items, nh.free_space_start);

                        long copied = ReadFromExtentItem(twigData,
                                                         bodyOff,
                                                         itemLen,
                                                         itemKey,
                                                         currentOffset,
                                                         buffer,
                                                         bytesRead,
                                                         toRead - bytesRead);

                        if(copied > 0)
                        {
                            bytesRead     += copied;
                            currentOffset += copied;

                            continue;
                        }
                    }
                }
            }

            // No data found at this offset — end of readable data
            break;
        }

        read            =  bytesRead;
        fileNode.Offset += bytesRead;

        AaruLogging.Debug(MODULE_NAME, "ReadFile: read {0} bytes, new offset={1}", read, fileNode.Offset);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Builds a file body key from a stat-data key and a byte offset.
    ///     The body key uses the same locality/ordering/objectid but with KEY_BODY_MINOR type
    ///     and the offset in the last element.
    /// </summary>
    LargeKey BuildFileBodyKey(LargeKey statDataKey, ulong offset)
    {
        // Replace type: clear old type, set KEY_BODY_MINOR
        ulong el0 = statDataKey.el0 & KEY_LOCALITY_MASK | KEY_BODY_MINOR;

        if(_largeKeys)
        {
            return new LargeKey
            {
                el0 = el0,
                el1 = statDataKey.el1, // ordering
                el2 = statDataKey.el2, // objectid
                el3 = offset
            };
        }

        return new LargeKey
        {
            el0 = el0,
            el1 = statDataKey.el1, // objectid
            el2 = offset,
            el3 = 0
        };
    }

    /// <summary>
    ///     Checks whether an item key belongs to the same file as the given stat-data key
    ///     by comparing locality, ordering (for large keys), and objectid.
    /// </summary>
    bool KeyMatchesFileBody(LargeKey itemKey, LargeKey statDataKey)
    {
        // Locality must match (upper 60 bits of el0)
        if((itemKey.el0 & KEY_LOCALITY_MASK) != (statDataKey.el0 & KEY_LOCALITY_MASK)) return false;

        if(_largeKeys)
        {
            // Ordering and objectid must match
            return itemKey.el1                       == statDataKey.el1 &&
                   (itemKey.el2 & KEY_OBJECTID_MASK) == (statDataKey.el2 & KEY_OBJECTID_MASK);
        }

        // Short keys: objectid must match
        return (itemKey.el1 & KEY_OBJECTID_MASK) == (statDataKey.el1 & KEY_OBJECTID_MASK);
    }

    /// <summary>Extracts the offset field from a file body key</summary>
    ulong GetKeyOffset(LargeKey key) => _largeKeys ? key.el3 : key.el2;

    /// <summary>
    ///     Reads data from an extent item (array of extent descriptors).
    ///     Each extent has a start block and width (number of blocks).
    ///     Returns the number of bytes actually copied.
    /// </summary>
    long ReadFromExtentItem(byte[] nodeData, int bodyOff, int itemLen, LargeKey itemKey, long fileOffset, byte[] buffer,
                            long   bufferOffset, long maxBytes)
    {
        ulong itemFileOffset = GetKeyOffset(itemKey);
        int   extentCount    = itemLen / EXTENT_SIZE;

        if(extentCount <= 0) return 0;

        long bytesRead            = 0;
        var  currentExtentFileOff = (long)itemFileOffset;

        for(var i = 0; i < extentCount && bytesRead < maxBytes; i++)
        {
            int off = bodyOff + i * EXTENT_SIZE;

            if(off + EXTENT_SIZE > nodeData.Length) break;

            var start = BitConverter.ToUInt64(nodeData, off);
            var width = BitConverter.ToUInt64(nodeData, off + 8);

            var extentBytes = (long)(width * _blockSize);

            // Check if the requested offset falls within this extent
            if(fileOffset < currentExtentFileOff + extentBytes && fileOffset >= currentExtentFileOff)
            {
                long offsetInExtent = fileOffset - currentExtentFileOff;

                // start == 0 means a hole (sparse file)
                if(start == 0)
                {
                    long available = extentBytes - offsetInExtent;
                    var  toCopy    = (int)Math.Min(available, maxBytes - bytesRead);

                    Array.Clear(buffer, (int)(bufferOffset + bytesRead), toCopy);

                    bytesRead  += toCopy;
                    fileOffset += toCopy;
                }
                else
                {
                    // Read blocks from the extent
                    var blockInExtent = offsetInExtent / _blockSize;
                    var offsetInBlock = (int)(offsetInExtent % _blockSize);

                    while(blockInExtent < (long)width && bytesRead < maxBytes)
                    {
                        ulong blockNum = start + (ulong)blockInExtent;

                        ErrorNumber blkErr = ReadBlock(blockNum, out byte[] blockData);

                        if(blkErr != ErrorNumber.NoError) return bytesRead;

                        int available = (int)_blockSize - offsetInBlock;
                        var toCopy    = (int)Math.Min(available, maxBytes - bytesRead);

                        Array.Copy(blockData, offsetInBlock, buffer, bufferOffset + bytesRead, toCopy);

                        bytesRead     += toCopy;
                        fileOffset    += toCopy;
                        offsetInBlock =  0;
                        blockInExtent++;
                    }
                }
            }

            currentExtentFileOff += extentBytes;
        }

        return bytesRead;
    }

    /// <summary>
    ///     Reads the symlink target from the SYMLINK_STAT extension in the stat-data.
    ///     Reiser4 symlinks store their target as a null-terminated string in the stat-data itself.
    /// </summary>
    ErrorNumber ReadSymlinkFromStatData(LargeKey statDataKey, out string target)
    {
        target = null;

        ErrorNumber errno = SearchByKey(statDataKey, out byte[] leafData, out int itemPos);

        if(errno != ErrorNumber.NoError) return errno;

        if(itemPos < 0) return ErrorNumber.NoSuchFile;

        Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(leafData);

        if(itemPos >= nh.nr_items) return ErrorNumber.NoSuchFile;

        ReadItemHeader(leafData, itemPos, nh.nr_items, out _, out ushort bodyOff, out _, out ushort pluginId);

        if(pluginId != STATIC_STAT_DATA_ID) return ErrorNumber.NoSuchFile;

        int sdLen = GetItemLength(leafData, itemPos, nh.nr_items, nh.free_space_start);

        if(sdLen < Marshal.SizeOf<StatDataBase>()) return ErrorNumber.InvalidArgument;

        StatDataBase sdBase =
            Marshal.ByteArrayToStructureLittleEndian<StatDataBase>(leafData, bodyOff, Marshal.SizeOf<StatDataBase>());

        if((sdBase.extmask & SD_SYMLINK) == 0) return ErrorNumber.NotSupported;

        // Walk through extensions to find the symlink extension
        int sdOff = bodyOff + Marshal.SizeOf<StatDataBase>();

        if((sdBase.extmask & SD_LIGHT_WEIGHT) != 0) sdOff += Marshal.SizeOf<LightWeightStat>();

        if((sdBase.extmask & SD_UNIX) != 0) sdOff += Marshal.SizeOf<UnixStat>();

        if((sdBase.extmask & SD_LARGE_TIMES) != 0) sdOff += Marshal.SizeOf<LargeTimesStat>();

        // Now we should be at the symlink extension — it's a null-terminated string
        if(sdOff >= bodyOff + sdLen) return ErrorNumber.InvalidArgument;

        int maxLen = bodyOff + sdLen - sdOff;
        var len    = 0;

        for(int j = sdOff; j < sdOff + maxLen && j < leafData.Length; j++)
        {
            if(leafData[j] == 0) break;

            len++;
        }

        if(len > 0) target = _encoding.GetString(leafData, sdOff, len);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Reads all file body data for an object. Used for symlinks and small files.
    ///     Tries tail items first, then extents.
    /// </summary>
    ErrorNumber ReadAllFileData(LargeKey statDataKey, out byte[] data)
    {
        data = null;

        // Get file size from stat data
        ErrorNumber errno = ReadStatData(statDataKey, out FileEntryInfo stat);

        if(errno != ErrorNumber.NoError) return errno;

        if(stat.Length <= 0)
        {
            data = [];

            return ErrorNumber.NoError;
        }

        data = new byte[stat.Length];

        // Use the same logic as ReadFile
        long bytesRead     = 0;
        long currentOffset = 0;

        while(bytesRead < stat.Length)
        {
            LargeKey bodyKey = BuildFileBodyKey(statDataKey, (ulong)currentOffset);

            // Try tail item
            errno = SearchByKey(bodyKey, out byte[] leafData, out int itemPos);

            if(errno == ErrorNumber.NoError && itemPos >= 0)
            {
                Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(leafData);

                if(itemPos < nh.nr_items)
                {
                    ReadItemHeader(leafData,
                                   itemPos,
                                   nh.nr_items,
                                   out LargeKey itemKey,
                                   out ushort bodyOff,
                                   out _,
                                   out ushort pluginId);

                    if(pluginId == FORMATTING_ID && KeyMatchesFileBody(itemKey, statDataKey))
                    {
                        int itemLen = GetItemLength(leafData, itemPos, nh.nr_items, nh.free_space_start);

                        ulong itemOffset = GetKeyOffset(itemKey);
                        var   skip       = (int)((ulong)currentOffset - itemOffset);

                        if(skip >= 0 && skip < itemLen)
                        {
                            int available = itemLen - skip;
                            var toCopy    = (int)Math.Min(available, stat.Length - bytesRead);

                            Array.Copy(leafData, bodyOff + skip, data, bytesRead, toCopy);

                            bytesRead     += toCopy;
                            currentOffset += toCopy;

                            continue;
                        }
                    }
                }
            }

            // Try extent item
            errno = SearchByKey(bodyKey, out byte[] twigData, out itemPos, TWIG_LEVEL);

            if(errno == ErrorNumber.NoError && itemPos >= 0)
            {
                Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(twigData);

                if(itemPos < nh.nr_items)
                {
                    ReadItemHeader(twigData,
                                   itemPos,
                                   nh.nr_items,
                                   out LargeKey itemKey,
                                   out ushort bodyOff,
                                   out _,
                                   out ushort pluginId);

                    if(pluginId == EXTENT_POINTER_ID && KeyMatchesFileBody(itemKey, statDataKey))
                    {
                        int itemLen = GetItemLength(twigData, itemPos, nh.nr_items, nh.free_space_start);

                        long copied = ReadFromExtentItem(twigData,
                                                         bodyOff,
                                                         itemLen,
                                                         itemKey,
                                                         currentOffset,
                                                         data,
                                                         bytesRead,
                                                         stat.Length - bytesRead);

                        if(copied > 0)
                        {
                            bytesRead     += copied;
                            currentOffset += copied;

                            continue;
                        }
                    }
                }
            }

            break;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Resolves a filesystem path to the stat-data key of the target object</summary>
    /// <param name="path">The path to resolve</param>
    /// <param name="statDataKey">Resolved stat-data key</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ResolvePath(string path, out LargeKey statDataKey)
    {
        statDataKey = default(LargeKey);

        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        if(normalizedPath is "/")
        {
            statDataKey = BuildRootStatDataKey();

            return ErrorNumber.NoError;
        }

        if(_pathCache.TryGetValue(normalizedPath, out statDataKey)) return ErrorNumber.NoError;

        string stripped = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                              ? normalizedPath[1..]
                              : normalizedPath;

        string[] components = stripped.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(components.Length == 0) return ErrorNumber.InvalidArgument;

        // Start from root directory cache
        Dictionary<string, LargeKey> currentEntries = _rootDirectoryCache;

        for(var i = 0; i < components.Length; i++)
        {
            string component = components[i];

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out LargeKey target))
            {
                AaruLogging.Debug(MODULE_NAME, "ResolvePath: '{0}' not found", component);

                return ErrorNumber.NoSuchFile;
            }

            // Last component — found it
            if(i == components.Length - 1)
            {
                statDataKey                = target;
                _pathCache[normalizedPath] = statDataKey;

                return ErrorNumber.NoError;
            }

            // Intermediate component — must be a directory, descend into it
            ErrorNumber errno = ReadObjectMode(target, out ushort mode);

            if(errno != ErrorNumber.NoError) return errno;

            if((mode & S_IFMT) != S_IFDIR)
            {
                AaruLogging.Debug(MODULE_NAME, "ResolvePath: '{0}' is not a directory", component);

                return ErrorNumber.NotDirectory;
            }

            ulong objectId = GetStatDataObjectId(target);

            errno = ReadDirectoryEntries(objectId, out Dictionary<string, LargeKey> subEntries);

            if(errno != ErrorNumber.NoError) return errno;

            // "." and ".." are already skipped above before the lookup, so leaving them in the
            // cached dictionary is harmless and avoids an O(n) copy of every directory's entries
            // on every single intermediate path component.
            currentEntries = subEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>
    ///     Reads the full stat-data for an object given its stat-data key
    ///     and populates a <see cref="FileEntryInfo" />.
    /// </summary>
    ErrorNumber ReadStatData(LargeKey statDataKey, out FileEntryInfo stat)
    {
        ulong objectId = GetStatDataObjectId(statDataKey);

        if(_statCache.TryGetValue(objectId, out stat)) return ErrorNumber.NoError;

        ErrorNumber cacheErrno = ReadStatDataUncached(statDataKey, out stat);

        if(cacheErrno == ErrorNumber.NoError) _statCache[objectId] = stat;

        return cacheErrno;
    }

    /// <summary>
    ///     Reads the full stat-data for an object given its stat-data key
    ///     and populates a <see cref="FileEntryInfo" />.
    /// </summary>
    ErrorNumber ReadStatDataUncached(LargeKey statDataKey, out FileEntryInfo stat)
    {
        stat = null;

        ErrorNumber errno = SearchByKey(statDataKey, out byte[] leafData, out int itemPos);

        if(errno != ErrorNumber.NoError) return errno;

        if(itemPos < 0) return ErrorNumber.NoSuchFile;

        Node40Header nh = Marshal.ByteArrayToStructureLittleEndian<Node40Header>(leafData);

        if(itemPos >= nh.nr_items) return ErrorNumber.NoSuchFile;

        ReadItemHeader(leafData, itemPos, nh.nr_items, out _, out ushort bodyOff, out _, out ushort pluginId);

        if(pluginId != STATIC_STAT_DATA_ID) return ErrorNumber.NoSuchFile;

        int sdLen = GetItemLength(leafData, itemPos, nh.nr_items, nh.free_space_start);

        if(sdLen < Marshal.SizeOf<StatDataBase>()) return ErrorNumber.InvalidArgument;

        StatDataBase sdBase =
            Marshal.ByteArrayToStructureLittleEndian<StatDataBase>(leafData, bodyOff, Marshal.SizeOf<StatDataBase>());

        stat = new FileEntryInfo
        {
            Attributes = FileAttributes.None,
            BlockSize  = _blockSize,
            Inode      = GetStatDataObjectId(statDataKey)
        };

        int sdOff = bodyOff + Marshal.SizeOf<StatDataBase>();

        // Parse light-weight stat extension (mode, nlink, size)
        if((sdBase.extmask & SD_LIGHT_WEIGHT) != 0)
        {
            if(sdOff + Marshal.SizeOf<LightWeightStat>() > leafData.Length) return ErrorNumber.InvalidArgument;

            LightWeightStat lws =
                Marshal.ByteArrayToStructureLittleEndian<LightWeightStat>(leafData,
                                                                          sdOff,
                                                                          Marshal.SizeOf<LightWeightStat>());

            stat.Mode       = (uint)(lws.mode & 0x0FFF);
            stat.Links      = lws.nlink;
            stat.Length     = (long)lws.size;
            stat.Attributes = ModeToAttributes(lws.mode);

            sdOff += Marshal.SizeOf<LightWeightStat>();
        }

        // Parse unix stat extension (uid, gid, timestamps, rdev/bytes)
        if((sdBase.extmask & SD_UNIX) != 0)
        {
            if(sdOff + Marshal.SizeOf<UnixStat>() > leafData.Length) return ErrorNumber.InvalidArgument;

            UnixStat us =
                Marshal.ByteArrayToStructureLittleEndian<UnixStat>(leafData, sdOff, Marshal.SizeOf<UnixStat>());

            stat.UID                 = us.uid;
            stat.GID                 = us.gid;
            stat.AccessTimeUtc       = DateHandlers.UnixUnsignedToDateTime(us.atime);
            stat.LastWriteTimeUtc    = DateHandlers.UnixUnsignedToDateTime(us.mtime);
            stat.StatusChangeTimeUtc = DateHandlers.UnixUnsignedToDateTime(us.ctime);

            // For regular files, rdev_or_bytes holds number of bytes used on disk
            if(stat.Attributes.HasFlag(FileAttributes.File)) stat.Blocks = (long)(us.rdev_or_bytes / _blockSize);

            // For device files, rdev_or_bytes holds the device number
            if(stat.Attributes.HasFlag(FileAttributes.BlockDevice) ||
               stat.Attributes.HasFlag(FileAttributes.CharDevice))
                stat.DeviceNo = us.rdev_or_bytes;

            sdOff += Marshal.SizeOf<UnixStat>();
        }

        // Parse large times extension (sub-second timestamps — we just skip past it)
        if((sdBase.extmask & SD_LARGE_TIMES) != 0) sdOff += Marshal.SizeOf<LargeTimesStat>();

        // Skip symlink extension (bit 3) — variable length, not needed for stat
        // Skip plugin extension (bit 4) — variable length, not needed for stat

        // Parse flags extension
        if((sdBase.extmask & SD_FLAGS) != 0)
        {
            // Must skip symlink (bit 3) and plugin (bit 4) extensions first if present.
            // Since those are variable length and hard to parse without more context,
            // we only attempt flags if neither is present.
            if((sdBase.extmask & (SD_SYMLINK | SD_PLUGIN)) == 0 &&
               sdOff + Marshal.SizeOf<FlagsStat>()         <= leafData.Length)
            {
                FlagsStat fs =
                    Marshal.ByteArrayToStructureLittleEndian<FlagsStat>(leafData, sdOff, Marshal.SizeOf<FlagsStat>());

                if((fs.flags & 0x00000010) != 0) stat.Attributes |= FileAttributes.Immutable;
                if((fs.flags & 0x00000020) != 0) stat.Attributes |= FileAttributes.AppendOnly;
                if((fs.flags & 0x00000040) != 0) stat.Attributes |= FileAttributes.NoDump;
                if((fs.flags & 0x00000080) != 0) stat.Attributes |= FileAttributes.NoAccessTime;
                if((fs.flags & 0x00000008) != 0) stat.Attributes |= FileAttributes.Sync;
            }
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
}