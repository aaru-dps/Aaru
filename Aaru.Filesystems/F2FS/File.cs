// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : F2FS filesystem plugin.
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
using Aaru.Compression;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

public sealed partial class F2FS
{
    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(string.IsNullOrEmpty(path) || path == "/") return ErrorNumber.IsDirectory;

        AaruLogging.Debug(MODULE_NAME, "OpenFile: path='{0}'", path);

        // Resolve path to inode number
        ErrorNumber errno = LookupFile(path, out uint nid);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: LookupFile failed with {0}", errno);

            return errno;
        }

        // Read the raw node block so we can both parse the inode and keep inline data if needed
        errno = LookupNat(nid, out uint blockAddr);

        if(errno != ErrorNumber.NoError) return errno;

        if(blockAddr == 0) return ErrorNumber.InvalidArgument;

        errno = ReadBlock(blockAddr, out byte[] nodeBlock);

        if(errno != ErrorNumber.NoError) return errno;

        Inode inode = Marshal.ByteArrayToStructureLittleEndian<Inode>(nodeBlock);

        // Reject directories
        if((inode.i_mode & 0xF000) == 0x4000)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: path is a directory");

            return ErrorNumber.IsDirectory;
        }

        // Reject encrypted files — data is ciphertext without the key
        if((inode.i_advise & FADVISE_ENCRYPT_BIT) != 0)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: file is encrypted, cannot read data");

            return ErrorNumber.NotSupported;
        }

        // Reject compressed files whose blocks have been released — data no longer exists on disk
        if((inode.i_inline & F2FS_COMPRESS_RELEASED) != 0)
        {
            AaruLogging.Debug(MODULE_NAME, "OpenFile: compressed file has released blocks, data unavailable");

            return ErrorNumber.NotSupported;
        }

        // Compute addrsPerInode using centralized helpers matching kernel logic
        int extraIsize    = GetExtraIsize(inode);
        int xattrAddrs    = GetInlineXattrAddrs(inode);
        int addrsPerInode = DEF_ADDRS_PER_INODE - extraIsize - xattrAddrs;

        // Check for inline data
        bool   hasInlineData  = (inode.i_inline & F2FS_INLINE_DATA) != 0;
        var    inlineDataOff  = 0;
        var    inlineDataSize = 0;
        byte[] retainedBlock  = null;

        if(hasInlineData)
        {
            // Inline data starts at i_addr area after extra_isize + DEF_INLINE_RESERVED_SIZE (1 __le32)
            int inodeFixedSize = Marshal.SizeOf<Inode>() - DEF_ADDRS_PER_INODE * 4 - DEF_NIDS_PER_INODE * 4;

            inlineDataOff = inodeFixedSize + (extraIsize + DEF_INLINE_RESERVED_SIZE) * 4;

            int totalAddrs  = DEF_ADDRS_PER_INODE - extraIsize;
            int usableAddrs = totalAddrs          - xattrAddrs - DEF_INLINE_RESERVED_SIZE;
            inlineDataSize = usableAddrs * 4;

            // Keep a reference to the raw node block bytes for reads
            retainedBlock = nodeBlock;

            AaruLogging.Debug(MODULE_NAME,
                              "OpenFile: inline data, offset={0}, maxSize={1}",
                              inlineDataOff,
                              inlineDataSize);
        }

        // Detect compression (F2FS_COMPR_FL in i_flags, with extra attributes)
        var  isCompressed = false;
        byte compressAlg  = 0;
        byte logClusterSz = 0;
        var  clusterSize  = 1;

        if((inode.i_flags  & F2FS_COMPR_FL)   != 0 &&
           (inode.i_inline & F2FS_EXTRA_ATTR) != 0 &&
           inode.i_addr is { Length: > EXTRA_OFFSET_COMPRESS_ALG })
        {
            isCompressed = true;

            // i_addr[8]: low byte = i_compress_algorithm, next byte = i_log_cluster_size
            compressAlg  = (byte)(inode.i_addr[EXTRA_OFFSET_COMPRESS_ALG]      & 0xFF);
            logClusterSz = (byte)(inode.i_addr[EXTRA_OFFSET_COMPRESS_ALG] >> 8 & 0xFF);
            clusterSize  = 1 << logClusterSz;

            AaruLogging.Debug(MODULE_NAME,
                              "OpenFile: compressed file, algorithm={0}, log_cluster_size={1}, cluster_size={2}",
                              compressAlg,
                              logClusterSz,
                              clusterSize);
        }

        node = new F2fsFileNode
        {
            Path               = path,
            Length             = (long)inode.i_size,
            Offset             = 0,
            InodeNumber        = nid,
            Inode              = inode,
            AddrsPerInode      = addrsPerInode,
            HasInlineData      = hasInlineData,
            InlineDataOffset   = inlineDataOff,
            InlineDataSize     = inlineDataSize,
            NodeBlock          = retainedBlock,
            CachedBlock        = null,
            CachedBlockIndex   = -1,
            IsCompressed       = isCompressed,
            CompressAlgorithm  = compressAlg,
            LogClusterSize     = logClusterSz,
            ClusterSize        = clusterSize,
            CachedCluster      = null,
            CachedClusterIndex = -1
        };

        AaruLogging.Debug(MODULE_NAME, "OpenFile: success, nid={0}, size={1}", nid, inode.i_size);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not F2fsFileNode fileNode) return ErrorNumber.InvalidArgument;

        fileNode.CachedBlock        = null;
        fileNode.CachedBlockIndex   = -1;
        fileNode.NodeBlock          = null;
        fileNode.CachedCluster      = null;
        fileNode.CachedClusterIndex = -1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not F2fsFileNode fileNode) return ErrorNumber.InvalidArgument;

        if(buffer == null) return ErrorNumber.InvalidArgument;

        if(fileNode.Offset < 0 || fileNode.Offset >= fileNode.Length) return ErrorNumber.InvalidArgument;

        // Clamp to remaining file size and buffer capacity
        long toRead = length;

        if(fileNode.Offset + toRead > fileNode.Length) toRead = fileNode.Length - fileNode.Offset;

        if(toRead <= 0) return ErrorNumber.NoError;

        if(toRead > buffer.Length) toRead = buffer.Length;

        // Inline data path — data lives inside the inode node block
        if(fileNode.HasInlineData) return ReadInlineData(fileNode, toRead, buffer, out read);

        // Compressed file path
        if(fileNode.IsCompressed) return ReadCompressedData(fileNode, toRead, buffer, out read);

        // Regular (block-mapped) data path
        long bytesRead     = 0;
        long currentOffset = fileNode.Offset;

        while(bytesRead < toRead)
        {
            long blockIndex    = currentOffset / _blockSize;
            var  offsetInBlock = (int)(currentOffset % _blockSize);

            byte[] blockData;

            // Use single-block cache if it matches
            if(blockIndex == fileNode.CachedBlockIndex && fileNode.CachedBlock != null)
                blockData = fileNode.CachedBlock;
            else
            {
                ErrorNumber errno = ResolveDataBlock(fileNode.Inode,
                                                     (uint)blockIndex,
                                                     fileNode.AddrsPerInode,
                                                     out uint dataBlockAddr);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "ReadFile: ResolveDataBlock failed for block {0}: {1}",
                                      blockIndex,
                                      errno);

                    if(bytesRead > 0) break;

                    return errno;
                }

                if(dataBlockAddr == 0)
                {
                    // Sparse hole — fill with zeros
                    long bytesToZero = Math.Min(_blockSize - offsetInBlock, toRead - bytesRead);
                    Array.Clear(buffer, (int)bytesRead, (int)bytesToZero);
                    bytesRead     += bytesToZero;
                    currentOffset += bytesToZero;

                    continue;
                }

                errno = ReadBlock(dataBlockAddr, out blockData);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME, "ReadFile: ReadBlock failed at {0}: {1}", dataBlockAddr, errno);

                    if(bytesRead > 0) break;

                    return errno;
                }

                // Cache this single block
                fileNode.CachedBlock      = blockData;
                fileNode.CachedBlockIndex = blockIndex;
            }

            if(blockData == null || blockData.Length == 0)
            {
                // Treat as sparse
                long bytesToZero = Math.Min(_blockSize - offsetInBlock, toRead - bytesRead);
                Array.Clear(buffer, (int)bytesRead, (int)bytesToZero);
                bytesRead     += bytesToZero;
                currentOffset += bytesToZero;

                continue;
            }

            long bytesToCopy = Math.Min(blockData.Length - offsetInBlock, toRead - bytesRead);
            Array.Copy(blockData, offsetInBlock, buffer, bytesRead, bytesToCopy);

            bytesRead     += bytesToCopy;
            currentOffset += bytesToCopy;
        }

        read            =  bytesRead;
        fileNode.Offset += bytesRead;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        // Root directory
        if(normalizedPath == "/")
        {
            ErrorNumber rootErrno = ReadInode(_superblock.root_ino, out Inode rootInode);

            if(rootErrno != ErrorNumber.NoError) return rootErrno;

            stat = InodeToFileEntryInfo(rootInode, _superblock.root_ino);

            return ErrorNumber.NoError;
        }

        // Parse path components
        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        // Traverse directories to find the target
        Dictionary<string, uint> currentEntries = _rootDirectoryCache;
        string                   targetName     = pathComponents[^1];

        // Traverse all but the last component (intermediate directories)
        for(var i = 0; i < pathComponents.Length - 1; i++)
        {
            string component = pathComponents[i];

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out uint dirInodeNumber))
            {
                AaruLogging.Debug(MODULE_NAME, "Stat: Component '{0}' not found", component);

                return ErrorNumber.NoSuchFile;
            }

            // Read directory entries for this component
            ErrorNumber errno = GetDirectoryEntries(dirInodeNumber, out Dictionary<string, uint> childEntries);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Stat: Error reading directory for '{0}': {1}", component, errno);

                return errno;
            }

            currentEntries = childEntries;
        }

        // Find the target in the current directory
        if(!currentEntries.TryGetValue(targetName, out uint targetNid))
        {
            AaruLogging.Debug(MODULE_NAME, "Stat: Target '{0}' not found", targetName);

            return ErrorNumber.NoSuchFile;
        }

        // Read the target inode
        ErrorNumber readErrno = ReadInode(targetNid, out Inode targetInode);

        if(readErrno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Stat: Error reading inode {0}: {1}", targetNid, readErrno);

            return readErrno;
        }

        stat = InodeToFileEntryInfo(targetInode, targetNid);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Verify it's a symlink
        ErrorNumber errno = Stat(path, out FileEntryInfo stat);

        if(errno != ErrorNumber.NoError) return errno;

        if(!stat.Attributes.HasFlag(FileAttributes.Symlink)) return ErrorNumber.InvalidArgument;

        // Resolve path to inode
        errno = LookupFile(path, out uint nid);

        if(errno != ErrorNumber.NoError) return errno;

        // Read the raw node block (needed for both inline and block-mapped cases)
        errno = LookupNat(nid, out uint blockAddr);

        if(errno != ErrorNumber.NoError) return errno;

        if(blockAddr == 0) return ErrorNumber.InvalidArgument;

        errno = ReadBlock(blockAddr, out byte[] nodeBlock);

        if(errno != ErrorNumber.NoError) return errno;

        Inode inode = Marshal.ByteArrayToStructureLittleEndian<Inode>(nodeBlock);

        // Reject encrypted symlinks — target is ciphertext without the key
        if((inode.i_advise & FADVISE_ENCRYPT_BIT) != 0)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadLink: symlink is encrypted, cannot read target");

            return ErrorNumber.NotSupported;
        }

        if(inode.i_size == 0)
        {
            dest = string.Empty;

            return ErrorNumber.NoError;
        }

        byte[] linkData;

        // F2FS uses page_symlink(): target is stored as inline data or in data block 0
        if((inode.i_inline & F2FS_INLINE_DATA) != 0)
        {
            // Inline data: read from the inode's i_addr area
            int extraIsize     = GetExtraIsize(inode);
            int xattrAddrs     = GetInlineXattrAddrs(inode);
            int inodeFixedSize = Marshal.SizeOf<Inode>() - DEF_ADDRS_PER_INODE * 4 - DEF_NIDS_PER_INODE * 4;

            int inlineDataOff  = inodeFixedSize      + (extraIsize + DEF_INLINE_RESERVED_SIZE) * 4;
            int totalAddrs     = DEF_ADDRS_PER_INODE - extraIsize;
            int usableAddrs    = totalAddrs          - xattrAddrs - DEF_INLINE_RESERVED_SIZE;
            int inlineDataSize = usableAddrs * 4;

            var maxLen = (int)Math.Min((long)inode.i_size, inlineDataSize);

            if(inlineDataOff + maxLen > nodeBlock.Length) return ErrorNumber.InvalidArgument;

            linkData = new byte[maxLen];
            Array.Copy(nodeBlock, inlineDataOff, linkData, 0, maxLen);
        }
        else
        {
            // Block-mapped: symlinks always fit in one block, read data block 0
            int addrsPerInode = GetAddrsPerInode(inode);

            errno = ResolveDataBlock(inode, 0, addrsPerInode, out uint dataBlockAddr);

            if(errno != ErrorNumber.NoError) return errno;

            if(dataBlockAddr == 0) return ErrorNumber.InvalidArgument;

            errno = ReadBlock(dataBlockAddr, out byte[] dataBlock);

            if(errno != ErrorNumber.NoError) return errno;

            var maxLen = (int)Math.Min((long)inode.i_size, _blockSize);
            linkData = new byte[maxLen];
            Array.Copy(dataBlock, 0, linkData, 0, maxLen);
        }

        // The target is a null-terminated string
        dest = StringHandlers.CToString(linkData, _encoding);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads data from an inline-data file node</summary>
    static ErrorNumber ReadInlineData(F2fsFileNode fileNode, long toRead, byte[] buffer, out long read)
    {
        read = 0;

        if(fileNode.NodeBlock == null) return ErrorNumber.InvalidArgument;

        int available = fileNode.InlineDataSize - (int)fileNode.Offset;

        if(available <= 0) return ErrorNumber.NoError;

        long bytesToCopy = Math.Min(toRead, available);
        int  srcOffset   = fileNode.InlineDataOffset + (int)fileNode.Offset;

        if(srcOffset + bytesToCopy > fileNode.NodeBlock.Length) bytesToCopy = fileNode.NodeBlock.Length - srcOffset;

        if(bytesToCopy <= 0) return ErrorNumber.NoError;

        Array.Copy(fileNode.NodeBlock, srcOffset, buffer, 0, bytesToCopy);

        read            =  bytesToCopy;
        fileNode.Offset += bytesToCopy;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads data from a compressed file node, decompressing clusters on the fly</summary>
    ErrorNumber ReadCompressedData(F2fsFileNode fileNode, long toRead, byte[] buffer, out long read)
    {
        read = 0;

        long clusterBytes  = fileNode.ClusterSize * _blockSize;
        long bytesRead     = 0;
        long currentOffset = fileNode.Offset;

        while(bytesRead < toRead)
        {
            long clusterIndex    = currentOffset / clusterBytes;
            var  offsetInCluster = (int)(currentOffset % clusterBytes);

            byte[] clusterData;

            // Use cluster cache if it matches
            if(clusterIndex == fileNode.CachedClusterIndex && fileNode.CachedCluster != null)
                clusterData = fileNode.CachedCluster;
            else
            {
                ErrorNumber errno = ReadCluster(fileNode, (uint)clusterIndex, out clusterData);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME,
                                      "ReadCompressedData: ReadCluster failed for cluster {0}: {1}",
                                      clusterIndex,
                                      errno);

                    if(bytesRead > 0) break;

                    return errno;
                }

                fileNode.CachedCluster      = clusterData;
                fileNode.CachedClusterIndex = clusterIndex;
            }

            long bytesToCopy = Math.Min(clusterData.Length - offsetInCluster, toRead - bytesRead);

            if(bytesToCopy <= 0) break;

            Array.Copy(clusterData, offsetInCluster, buffer, bytesRead, bytesToCopy);

            bytesRead     += bytesToCopy;
            currentOffset += bytesToCopy;
        }

        read            =  bytesRead;
        fileNode.Offset += bytesRead;

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Reads a cluster (compressed or uncompressed) for a compressed file.
    ///     If the first block of the cluster is COMPRESS_ADDR, reads and decompresses
    ///     the compressed data blocks. Otherwise, reads the raw blocks directly.
    /// </summary>
    ErrorNumber ReadCluster(F2fsFileNode fileNode, uint clusterIndex, out byte[] clusterData)
    {
        clusterData = null;

        int  clusterSize  = fileNode.ClusterSize;
        long clusterBytes = clusterSize  * _blockSize;
        uint startPage    = clusterIndex * (uint)clusterSize;

        // Resolve the first block of the cluster
        ErrorNumber errno =
            ResolveDataBlock(fileNode.Inode, startPage, fileNode.AddrsPerInode, out uint firstBlockAddr);

        if(errno != ErrorNumber.NoError) return errno;

        // Compressed cluster: first block addr == COMPRESS_ADDR
        if(firstBlockAddr == COMPRESS_ADDR)
        {
            // Count how many compressed data blocks follow (blocks 1..clusterSize-1 that are non-zero/non-null)
            var compressedBlocks = new List<byte[]>();

            for(var i = 1; i < clusterSize; i++)
            {
                errno = ResolveDataBlock(fileNode.Inode, startPage + (uint)i, fileNode.AddrsPerInode, out uint blkAddr);

                if(errno != ErrorNumber.NoError) return errno;

                if(blkAddr == NULL_ADDR || blkAddr == NEW_ADDR || blkAddr == COMPRESS_ADDR) break;

                errno = ReadBlock(blkAddr, out byte[] blkData);

                if(errno != ErrorNumber.NoError) return errno;

                compressedBlocks.Add(blkData);
            }

            if(compressedBlocks.Count == 0)
            {
                // No compressed data blocks — treat as sparse
                clusterData = new byte[clusterBytes];

                return ErrorNumber.NoError;
            }

            // Assemble all compressed blocks into one contiguous buffer
            var compBuf = new byte[compressedBlocks.Count * _blockSize];

            for(var i = 0; i < compressedBlocks.Count; i++)
                Array.Copy(compressedBlocks[i], 0, compBuf, i * (int)_blockSize, (int)_blockSize);

            // Parse the CompressData header
            if(compBuf.Length < COMPRESS_HEADER_SIZE)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadCluster: compressed buffer too small for header");

                return ErrorNumber.InvalidArgument;
            }

            var clen = BitConverter.ToUInt32(compBuf, 0);

            if(clen == 0 || clen > compBuf.Length - COMPRESS_HEADER_SIZE)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "ReadCluster: invalid clen={0}, bufSize={1}",
                                  clen,
                                  compBuf.Length - COMPRESS_HEADER_SIZE);

                return ErrorNumber.InvalidArgument;
            }

            // Extract compressed payload (after header)
            var compressedPayload = new byte[clen];
            Array.Copy(compBuf, COMPRESS_HEADER_SIZE, compressedPayload, 0, (int)clen);

            // Decompress
            clusterData = new byte[clusterBytes];

            errno = DecompressCluster(fileNode.CompressAlgorithm, compressedPayload, clusterData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "ReadCluster: decompression failed for cluster {0}: {1}",
                                  clusterIndex,
                                  errno);

                return errno;
            }

            return ErrorNumber.NoError;
        }

        // Non-compressed cluster: read individual blocks
        clusterData = new byte[clusterBytes];

        for(var i = 0; i < clusterSize; i++)
        {
            uint blkAddr;

            if(i == 0)
                blkAddr = firstBlockAddr;
            else
            {
                errno = ResolveDataBlock(fileNode.Inode, startPage + (uint)i, fileNode.AddrsPerInode, out blkAddr);

                if(errno != ErrorNumber.NoError) return errno;
            }

            if(blkAddr == NULL_ADDR || blkAddr == NEW_ADDR || blkAddr == COMPRESS_ADDR) continue; // sparse

            errno = ReadBlock(blkAddr, out byte[] blkData);

            if(errno != ErrorNumber.NoError) return errno;

            Array.Copy(blkData, 0, clusterData, i * (int)_blockSize, (int)_blockSize);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Decompresses a compressed cluster payload using the specified F2FS compression algorithm</summary>
    static ErrorNumber DecompressCluster(byte algorithm, byte[] source, byte[] destination)
    {
        int result;

        switch(algorithm)
        {
            case COMPRESS_LZO:
            case COMPRESS_LZORLE:
                // F2FS uses LZO1X for both LZO and LZO-RLE
                result = LZO.DecodeBuffer(source, destination, LZO.Algorithm.LZO1X);

                if(result < 0)
                {
                    AaruLogging.Debug("F2FS plugin", "LZO decompression failed");

                    return ErrorNumber.InvalidArgument;
                }

                break;

            case COMPRESS_LZ4:
                result = LZ4.DecodeBuffer(source, destination);

                if(result < 0)
                {
                    AaruLogging.Debug("F2FS plugin", "LZ4 decompression failed");

                    return ErrorNumber.InvalidArgument;
                }

                break;

            case COMPRESS_ZSTD:
                result = ZSTD.DecodeBuffer(source, destination);

                if(result <= 0)
                {
                    AaruLogging.Debug("F2FS plugin", "ZSTD decompression failed");

                    return ErrorNumber.InvalidArgument;
                }

                break;

            default:
                AaruLogging.Debug("F2FS plugin", "Unknown compression algorithm: {0}", algorithm);

                return ErrorNumber.NotSupported;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Resolves a path to an F2FS inode node ID by traversing directories from the root</summary>
    /// <param name="path">File path</param>
    /// <param name="nid">Output node ID</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber LookupFile(string path, out uint nid)
    {
        nid = 0;

        string normalizedPath = path ?? "/";

        if(normalizedPath is "" or ".") normalizedPath = "/";

        if(normalizedPath is "/")
        {
            nid = _superblock.root_ino;

            return ErrorNumber.NoError;
        }

        string pathWithoutLeadingSlash = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                                             ? normalizedPath[1..]
                                             : normalizedPath;

        string[] pathComponents = pathWithoutLeadingSlash.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if(pathComponents.Length == 0) return ErrorNumber.InvalidArgument;

        Dictionary<string, uint> currentEntries = _rootDirectoryCache;
        string                   targetName     = pathComponents[^1];

        // Traverse parent directories
        for(var i = 0; i < pathComponents.Length - 1; i++)
        {
            string component = pathComponents[i];

            if(component is "." or "..") continue;

            if(!currentEntries.TryGetValue(component, out uint dirNid)) return ErrorNumber.NoSuchFile;

            ErrorNumber errno = GetDirectoryEntries(dirNid, out Dictionary<string, uint> childEntries);

            if(errno != ErrorNumber.NoError) return errno;

            currentEntries = childEntries;
        }

        if(!currentEntries.TryGetValue(targetName, out nid)) return ErrorNumber.NoSuchFile;

        return ErrorNumber.NoError;
    }

    /// <summary>Converts an F2FS inode to a FileEntryInfo structure</summary>
    /// <param name="inode">The F2FS inode</param>
    /// <param name="inodeNumber">The inode/node number</param>
    /// <returns>FileEntryInfo populated from the inode</returns>
    FileEntryInfo InodeToFileEntryInfo(in Inode inode, ulong inodeNumber)
    {
        // Determine file type from mode field (Unix-style S_IFMT mask = 0xF000)
        FileAttributes attributes = (inode.i_mode & 0xF000) switch
                                    {
                                        0x4000 => FileAttributes.Directory,
                                        0xA000 => FileAttributes.Symlink,
                                        0x2000 => FileAttributes.CharDevice,
                                        0x6000 => FileAttributes.BlockDevice,
                                        0x1000 => FileAttributes.FIFO,
                                        0xC000 => FileAttributes.Socket,
                                        _      => FileAttributes.File
                                    };

        // Map F2FS inode flags (i_flags) to file attributes
        if((inode.i_flags & F2FS_IMMUTABLE_FL) != 0) attributes |= FileAttributes.Immutable;

        if((inode.i_flags & F2FS_APPEND_FL) != 0) attributes |= FileAttributes.AppendOnly;

        if((inode.i_flags & F2FS_NODUMP_FL) != 0) attributes |= FileAttributes.NoDump;

        if((inode.i_flags & F2FS_NOATIME_FL) != 0) attributes |= FileAttributes.NoAccessTime;

        if((inode.i_flags & F2FS_SYNC_FL) != 0) attributes |= FileAttributes.Sync;

        if((inode.i_flags & F2FS_DIRSYNC_FL) != 0) attributes |= FileAttributes.Sync;

        if((inode.i_flags & F2FS_COMPR_FL) != 0) attributes |= FileAttributes.Compressed;

        if((inode.i_flags & F2FS_INDEX_FL) != 0) attributes |= FileAttributes.IndexedDirectory;

        // Map inline flags
        if((inode.i_inline & F2FS_INLINE_DATA) != 0) attributes |= FileAttributes.Inline;

        var info = new FileEntryInfo
        {
            Attributes          = attributes,
            Inode               = inodeNumber,
            Links               = inode.i_links,
            Length              = (long)inode.i_size,
            Blocks              = (long)inode.i_blocks,
            BlockSize           = _blockSize,
            UID                 = inode.i_uid,
            GID                 = inode.i_gid,
            Mode                = (uint)(inode.i_mode & 0x0FFF),
            AccessTimeUtc       = DateHandlers.UnixUnsignedToDateTime((uint)inode.i_atime, inode.i_atime_nsec),
            LastWriteTimeUtc    = DateHandlers.UnixUnsignedToDateTime((uint)inode.i_mtime, inode.i_mtime_nsec),
            StatusChangeTimeUtc = DateHandlers.UnixUnsignedToDateTime((uint)inode.i_ctime, inode.i_ctime_nsec)
        };

        // Extract creation time from extra attributes if present
        if((inode.i_inline & F2FS_EXTRA_ATTR) != 0 && inode.i_addr is { Length: > EXTRA_OFFSET_CRTIME_NSEC })
        {
            // i_crtime is a u64 stored in i_addr[3..4], i_crtime_nsec is u32 in i_addr[5]
            ulong crtime     = inode.i_addr[EXTRA_OFFSET_CRTIME] | (ulong)inode.i_addr[EXTRA_OFFSET_CRTIME + 1] << 32;
            uint  crtimeNsec = inode.i_addr[EXTRA_OFFSET_CRTIME_NSEC];

            if(crtime > 0) info.CreationTimeUtc = DateHandlers.UnixUnsignedToDateTime((uint)crtime, crtimeNsec);
        }

        // For character and block device inodes, decode the device number from the data address area
        // The kernel uses get_dnode_addr() which skips extra attrs, then checks addr[0] and addr[1]
        if((inode.i_mode & 0xF000) is 0x2000 or 0x6000 && inode.i_addr is { Length: > 0 })
        {
            int extraSize = GetExtraIsize(inode);

            // addr[0] after extra area = old-style dev, addr[1] = new-style dev
            if(extraSize + 1 < inode.i_addr.Length)
            {
                uint oldDev = inode.i_addr[extraSize];
                uint newDev = inode.i_addr[extraSize + 1];

                // If old_dev is non-zero, use old encoding; otherwise use new encoding
                // old_decode_dev: major = dev >> 8, minor = dev & 0xFF
                // new_decode_dev: major = (dev & 0xfff00) >> 8, minor = (dev & 0xff) | ((dev >> 12) & 0xfff00)
                if(oldDev != 0)
                    info.DeviceNo = (ulong)(oldDev >> 8) << 32 | oldDev & 0xFF;
                else if(newDev != 0)
                {
                    uint major = (newDev & 0xFFF00) >> 8;
                    uint minor = newDev & 0xFF | newDev >> 12 & 0xFFF00;
                    info.DeviceNo = (ulong)major << 32 | minor;
                }
            }
        }

        return info;
    }
}