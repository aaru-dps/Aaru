// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : B-tree file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     File stat and inode reading methods.
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
using Aaru.Logging;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class BTRFS
{
    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory
        if(normalizedPath is "/")
        {
            ErrorNumber errno = ReadInode(BTRFS_FIRST_FREE_OBJECTID, _fsTreeRoot, out InodeItem rootInode);

            if(errno != ErrorNumber.NoError) return errno;

            stat            = InodeItemToFileEntryInfo(rootInode, BTRFS_FIRST_FREE_OBJECTID);
            stat.Attributes = FileAttributes.Directory;

            return ErrorNumber.NoError;
        }

        // Resolve path to an objectid
        ErrorNumber pathErrno = ResolvePath(normalizedPath, out ulong objectId, out ulong treeRoot);

        if(pathErrno != ErrorNumber.NoError) return pathErrno;

        ErrorNumber inodeErrno = ReadInode(objectId, treeRoot, out InodeItem inode);

        if(inodeErrno != ErrorNumber.NoError) return inodeErrno;

        stat = InodeItemToFileEntryInfo(inode, objectId);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber statErrno = Stat(path, out FileEntryInfo stat);

        if(statErrno != ErrorNumber.NoError) return statErrno;

        if(!stat.Attributes.HasFlag(FileAttributes.Symlink)) return ErrorNumber.InvalidArgument;

        ErrorNumber pathErrno = ResolvePath(path, out ulong objectId, out ulong treeRoot);

        if(pathErrno != ErrorNumber.NoError) return pathErrno;

        // Read the FS tree to find extent data for this symlink
        ErrorNumber errno = ReadTreeBlock(treeRoot, out byte[] fsTreeData);

        if(errno != ErrorNumber.NoError) return errno;

        Header fsTreeHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(fsTreeData);

        List<ExtentEntry> extents = [];
        errno = WalkTreeForExtents(fsTreeData, fsTreeHeader, objectId, extents);

        if(errno != ErrorNumber.NoError) return errno;

        // BTRFS symlinks are always stored as a single inline extent at offset 0
        foreach(ExtentEntry extent in extents)
        {
            if(extent.Type != BTRFS_FILE_EXTENT_INLINE || extent.FileOffset != 0) continue;

            if(extent.InlineData is null || extent.InlineData.Length == 0) return ErrorNumber.InvalidArgument;

            dest = _encoding.GetString(extent.InlineData).TrimEnd('\0');

            return ErrorNumber.NoError;
        }

        return ErrorNumber.InvalidArgument;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(string.IsNullOrEmpty(path) || path is "/" or "." or "..") return ErrorNumber.IsDirectory;

        ErrorNumber pathErrno = ResolvePath(path, out ulong objectId, out ulong treeRoot);

        if(pathErrno != ErrorNumber.NoError) return pathErrno;

        ErrorNumber statErrno = Stat(path, out FileEntryInfo stat);

        if(statErrno != ErrorNumber.NoError) return statErrno;

        if(stat.Attributes.HasFlag(FileAttributes.Directory)) return ErrorNumber.IsDirectory;

        node = new BtrfsFileNode
        {
            Path     = path,
            Length   = stat.Length,
            Offset   = 0,
            ObjectId = objectId,
            TreeRoot = treeRoot
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not BtrfsFileNode) return ErrorNumber.InvalidArgument;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(node is not BtrfsFileNode fileNode) return ErrorNumber.InvalidArgument;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(length <= 0) return ErrorNumber.NoError;

        if(fileNode.Offset >= fileNode.Length) return ErrorNumber.NoError;

        // Clamp read to remaining file size and buffer capacity
        long toRead = length;

        if(fileNode.Offset + toRead > fileNode.Length) toRead = fileNode.Length - fileNode.Offset;

        if(toRead > buffer.Length) toRead = buffer.Length;

        // Walk the FS tree to find extent data items for this file and read the requested range
        ErrorNumber errno = ReadTreeBlock(fileNode.TreeRoot, out byte[] fsTreeData);

        if(errno != ErrorNumber.NoError) return errno;

        Header fsTreeHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(fsTreeData);

        // Collect all extent entries for this file's object
        List<ExtentEntry> extents = [];
        errno = WalkTreeForExtents(fsTreeData, fsTreeHeader, fileNode.ObjectId, extents);

        if(errno != ErrorNumber.NoError) return errno;

        // Sort extents by file offset
        extents.Sort((a, b) => a.FileOffset.CompareTo(b.FileOffset));

        long currentOffset = fileNode.Offset;
        long bytesRead     = 0;

        while(bytesRead < toRead)
        {
            // Find the extent covering the current offset
            ExtentEntry? coveringExtent = null;

            foreach(ExtentEntry extent in extents)
            {
                if(currentOffset >= (long)extent.FileOffset &&
                   currentOffset < (long)extent.FileOffset + (long)extent.Length)
                {
                    coveringExtent = extent;

                    break;
                }
            }

            if(coveringExtent is null)
            {
                // Gap in extents — file has a hole, fill with zeros
                // Find next extent to determine hole size
                long holeEnd = fileNode.Length;

                foreach(ExtentEntry extent in extents)
                {
                    if((long)extent.FileOffset > currentOffset)
                    {
                        holeEnd = (long)extent.FileOffset;

                        break;
                    }
                }

                long holeBytes = Math.Min(holeEnd - currentOffset, toRead - bytesRead);
                Array.Clear(buffer, (int)bytesRead, (int)holeBytes);
                bytesRead     += holeBytes;
                currentOffset += holeBytes;

                continue;
            }

            ExtentEntry ext = coveringExtent.Value;

            long offsetInExtent  = currentOffset    - (long)ext.FileOffset;
            long extentRemaining = (long)ext.Length - offsetInExtent;
            long bytesToCopy     = Math.Min(extentRemaining, toRead - bytesRead);

            switch(ext.Type)
            {
                case BTRFS_FILE_EXTENT_PREALLOC:
                    // Preallocated — reads as zeros
                    Array.Clear(buffer, (int)bytesRead, (int)bytesToCopy);
                    bytesRead     += bytesToCopy;
                    currentOffset += bytesToCopy;

                    break;

                case BTRFS_FILE_EXTENT_INLINE:
                {
                    byte[] inlineSource;

                    if(ext.Compression != BTRFS_COMPRESS_NONE)
                    {
                        if(fileNode.CachedExtentData != null && fileNode.CachedExtentOffset == ext.FileOffset)
                            inlineSource = fileNode.CachedExtentData;
                        else
                        {
                            inlineSource = DecompressExtent(ext.InlineData, (uint)ext.RamBytes, ext.Compression);

                            if(inlineSource is null)
                            {
                                AaruLogging.Debug(MODULE_NAME,
                                                  "Unsupported compression type {0} for inline extent",
                                                  ext.Compression);

                                return ErrorNumber.NotSupported;
                            }

                            fileNode.CachedExtentOffset = ext.FileOffset;
                            fileNode.CachedExtentData   = inlineSource;
                        }
                    }
                    else
                        inlineSource = ext.InlineData;

                    long inlineCopy = Math.Min(bytesToCopy, inlineSource.Length - offsetInExtent);

                    if(inlineCopy > 0)
                        Array.Copy(inlineSource, (int)offsetInExtent, buffer, (int)bytesRead, (int)inlineCopy);

                    bytesRead     += inlineCopy;
                    currentOffset += inlineCopy;

                    break;
                }

                case BTRFS_FILE_EXTENT_REG:
                    if(ext.DiskBytenr == 0)
                    {
                        // Sparse extent — reads as zeros
                        Array.Clear(buffer, (int)bytesRead, (int)bytesToCopy);
                        bytesRead     += bytesToCopy;
                        currentOffset += bytesToCopy;

                        break;
                    }

                    if(ext.Compression != BTRFS_COMPRESS_NONE)
                    {
                        byte[] decompressed;

                        if(fileNode.CachedExtentData != null && fileNode.CachedExtentOffset == ext.FileOffset)
                            decompressed = fileNode.CachedExtentData;
                        else
                        {
                            // Read all compressed data from disk
                            ErrorNumber compReadErrno =
                                ReadLogicalBytes(ext.DiskBytenr, (uint)ext.DiskBytes, out byte[] compressedData);

                            if(compReadErrno != ErrorNumber.NoError) return compReadErrno;

                            decompressed = DecompressExtent(compressedData, (uint)ext.RamBytes, ext.Compression);

                            if(decompressed is null)
                            {
                                AaruLogging.Debug(MODULE_NAME,
                                                  "Unsupported compression type {0} for regular extent",
                                                  ext.Compression);

                                return ErrorNumber.NotSupported;
                            }

                            fileNode.CachedExtentOffset = ext.FileOffset;
                            fileNode.CachedExtentData   = decompressed;
                        }

                        // ExtentOffset is offset into decompressed data for this file extent
                        long decompOffset = (long)ext.ExtentOffset + offsetInExtent;
                        long decompCopy   = Math.Min(bytesToCopy, decompressed.Length - decompOffset);

                        if(decompCopy > 0)
                            Array.Copy(decompressed, (int)decompOffset, buffer, (int)bytesRead, (int)decompCopy);

                        bytesRead     += decompCopy;
                        currentOffset += decompCopy;
                    }
                    else
                    {
                        // Read from disk in reasonable chunks to avoid caching whole file
                        while(bytesToCopy > 0)
                        {
                            // Read up to one nodesize at a time
                            var chunkSize = (uint)Math.Min(bytesToCopy, _superblock.nodesize);

                            // Logical address = disk_bytenr + extent offset + position within extent
                            ulong logicalRead = ext.DiskBytenr + ext.ExtentOffset + (ulong)offsetInExtent;

                            ErrorNumber readErrno = ReadLogicalBytes(logicalRead, chunkSize, out byte[] chunkData);

                            if(readErrno != ErrorNumber.NoError) return readErrno;

                            Array.Copy(chunkData, 0, buffer, (int)bytesRead, (int)chunkSize);

                            bytesRead      += chunkSize;
                            currentOffset  += chunkSize;
                            offsetInExtent += chunkSize;
                            bytesToCopy    -= chunkSize;
                        }
                    }

                    break;

                default:
                    AaruLogging.Debug(MODULE_NAME, "Unknown extent type {0}", ext.Type);

                    return ErrorNumber.InvalidArgument;
            }
        }

        read            =  bytesRead;
        fileNode.Offset += bytesRead;

        return ErrorNumber.NoError;
    }

    /// <summary>Resolves a filesystem path to its target objectid and tree root by walking the directory tree</summary>
    /// <param name="path">Absolute path to resolve (must start with /)</param>
    /// <param name="objectId">The objectid of the target file or directory</param>
    /// <param name="treeRoot">The tree root bytenr for the subvolume containing the target</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ResolvePath(string path, out ulong objectId, out ulong treeRoot)
    {
        objectId = 0;
        treeRoot = _fsTreeRoot;

        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        if(normalizedPath is "/")
        {
            objectId = BTRFS_FIRST_FREE_OBJECTID;

            return ErrorNumber.NoError;
        }

        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        Dictionary<string, DirEntry> currentEntries  = _rootDirectoryCache;
        ulong                        currentTreeRoot = _fsTreeRoot;

        for(var p = 0; p < pathComponents.Length; p++)
        {
            string component = pathComponents[p];

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out DirEntry entry)) return ErrorNumber.NoSuchFile;

            // Handle subvolume crossing
            if(entry.SubvolTreeRoot != 0) currentTreeRoot = entry.SubvolTreeRoot;

            // Last component — this is the target
            if(p == pathComponents.Length - 1)
            {
                objectId = entry.ObjectId;
                treeRoot = currentTreeRoot;

                return ErrorNumber.NoError;
            }

            // Intermediate — must be a directory
            if(entry.Type != BTRFS_FT_DIR) return ErrorNumber.NotDirectory;

            ErrorNumber errno =
                GetDirectoryContents(entry.ObjectId, currentTreeRoot, out Dictionary<string, DirEntry> dirEntries);

            if(errno != ErrorNumber.NoError) return errno;

            currentEntries = dirEntries;
        }

        return ErrorNumber.NoSuchFile;
    }
}