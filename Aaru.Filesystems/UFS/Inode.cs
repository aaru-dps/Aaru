// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Inode.cs
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
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class UFSPlugin
{
    /// <summary>Computes the fragment address of the start of a cylinder group</summary>
    long CgStart(int cg)
    {
        long cgBase = (long)_superBlock.fs_fpg * cg;

        if(_superBlock.fs_isUfs2) return cgBase;

        return cgBase + _superBlock.fs_cgoffset * (cg & ~_superBlock.fs_cgmask);
    }

    /// <summary>Computes the fragment address of the inode blocks in a cylinder group</summary>
    long CgImin(int cg) => CgStart(cg) + _superBlock.fs_iblkno;

    /// <summary>Reads a fragment (or multiple contiguous fragments) from the filesystem</summary>
    ErrorNumber ReadFragments(long fragNo, int count, out byte[] buffer)
    {
        buffer = null;

        long byteOffset = fragNo * _superBlock.fs_fsize;

        uint sectorSize = _imagePlugin.Info.SectorSize;

        if(sectorSize is 2336 or 2352 or 2448) sectorSize = 2048;

        long byteLen       = (long)count * _superBlock.fs_fsize;
        long sectorOffset  = byteOffset  / sectorSize;
        var  sectorsToRead = (uint)((byteLen + sectorSize - 1) / sectorSize);

        ErrorNumber errno = _imagePlugin.ReadSectors(_partition.Start + (ulong)sectorOffset,
                                                     false,
                                                     sectorsToRead,
                                                     out byte[] raw,
                                                     out _);

        if(errno != ErrorNumber.NoError) return errno;

        var offsetInSector = (int)(byteOffset % sectorSize);

        if(offsetInSector == 0 && raw.Length == byteLen)
        {
            buffer = raw;

            return ErrorNumber.NoError;
        }

        buffer = new byte[byteLen];

        var toCopy = (int)Math.Min(byteLen, raw.Length - offsetInSector);

        if(toCopy > 0) Array.Copy(raw, offsetInSector, buffer, 0, toCopy);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the fragment block containing an inode through a bounded cache</summary>
    /// <param name="fragAddr">Fragment address of the inode table block</param>
    /// <param name="data">The read block data</param>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadInodeBlock(long fragAddr, out byte[] data)
    {
        if(_inodeBlockCache.TryGetValue(fragAddr, out data)) return ErrorNumber.NoError;

        ErrorNumber errno = ReadFragments(fragAddr, _superBlock.fs_frag, out data);

        if(errno != ErrorNumber.NoError) return errno;

        // Bound memory use; blocks are cheap to re-read after a wholesale reset
        if(_inodeBlockCache.Count >= 1024) _inodeBlockCache.Clear();

        _inodeBlockCache[fragAddr] = data;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a UFS1 inode from disk given its inode number</summary>
    ErrorNumber ReadInode(uint inodeNumber, out Inode inode)
    {
        inode = default(Inode);

        var cg = (int)(inodeNumber / (uint)_superBlock.fs_ipg);

        long fragAddr = CgImin(cg) +
                        (inodeNumber % (uint)_superBlock.fs_ipg / _superBlock.fs_inopb << _superBlock.fs_fragshift);

        // Read the fragment block containing the inode
        ErrorNumber errno = ReadInodeBlock(fragAddr, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        // UFS1 inode is 128 bytes
        var inodeSize = 128;
        int offset    = (int)(inodeNumber % _superBlock.fs_inopb) * inodeSize;

        if(offset + inodeSize > data.Length) return ErrorNumber.InvalidArgument;

        var inodeData = new byte[inodeSize];
        Array.Copy(data, offset, inodeData, 0, inodeSize);

        inode = _bigEndian
                    ? Marshal.ByteArrayToStructureBigEndian<Inode>(inodeData)
                    : Marshal.ByteArrayToStructureLittleEndian<Inode>(inodeData);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a SunOS/Solaris inode from disk given its inode number</summary>
    ErrorNumber ReadInodeSun(uint inodeNumber, out InodeSun inode)
    {
        inode = default(InodeSun);

        var cg = (int)(inodeNumber / (uint)_superBlock.fs_ipg);

        long fragAddr = CgImin(cg) +
                        (inodeNumber % (uint)_superBlock.fs_ipg / _superBlock.fs_inopb << _superBlock.fs_fragshift);

        ErrorNumber errno = ReadInodeBlock(fragAddr, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        // Solaris inode is 128 bytes (same size as UFS1)
        var inodeSize = 128;
        int offset    = (int)(inodeNumber % _superBlock.fs_inopb) * inodeSize;

        if(offset + inodeSize > data.Length) return ErrorNumber.InvalidArgument;

        var inodeData = new byte[inodeSize];
        Array.Copy(data, offset, inodeData, 0, inodeSize);

        inode = _bigEndian
                    ? Marshal.ByteArrayToStructureBigEndian<InodeSun>(inodeData)
                    : Marshal.ByteArrayToStructureLittleEndian<InodeSun>(inodeData);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads a UFS2 inode from disk given its inode number</summary>
    ErrorNumber ReadInode2(uint inodeNumber, out Inode2 inode)
    {
        inode = default(Inode2);

        var cg = (int)(inodeNumber / (uint)_superBlock.fs_ipg);

        long fragAddr = CgImin(cg) +
                        (inodeNumber % (uint)_superBlock.fs_ipg / _superBlock.fs_inopb << _superBlock.fs_fragshift);

        ErrorNumber errno = ReadInodeBlock(fragAddr, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        // UFS2 inode is 256 bytes
        var inodeSize = 256;
        int offset    = (int)(inodeNumber % _superBlock.fs_inopb) * inodeSize;

        if(offset + inodeSize > data.Length) return ErrorNumber.InvalidArgument;

        var inodeData = new byte[inodeSize];
        Array.Copy(data, offset, inodeData, 0, inodeSize);

        inode = _bigEndian
                    ? Marshal.ByteArrayToStructureBigEndian<Inode2>(inodeData)
                    : Marshal.ByteArrayToStructureLittleEndian<Inode2>(inodeData);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Gets the list of physical fragment addresses for all logical blocks of a file.
    ///     For UFS1, block pointers are 32-bit; for UFS2, they are 64-bit.
    ///     Returns fragment addresses (not disk block addresses).
    /// </summary>
    ErrorNumber GetBlockList(long[] directBlocks, long[] indirectBlocks, ulong fileSize, out List<long> blockList)
    {
        blockList = [];

        int blockSize = _superBlock.fs_bsize;

        if(blockSize <= 0) return ErrorNumber.InvalidArgument;

        var totalBlocks = (long)((fileSize + (ulong)blockSize - 1) / (ulong)blockSize);

        // Direct blocks
        for(var i = 0; i < NDADDR && blockList.Count < totalBlocks; i++) blockList.Add(directBlocks[i]);

        if(blockList.Count >= totalBlocks) return ErrorNumber.NoError;

        int pointersPerBlock = _superBlock.fs_nindir;

        // Single indirect
        if(indirectBlocks[0] != 0)
        {
            ErrorNumber errno = ReadIndirectBlock(indirectBlocks[0], 1, totalBlocks, blockList);

            if(errno != ErrorNumber.NoError) return errno;
        }
        else
        {
            // Sparse — fill with zeros up to the single indirect limit
            long limit = Math.Min(totalBlocks, NDADDR + pointersPerBlock);

            while(blockList.Count < limit) blockList.Add(0);
        }

        if(blockList.Count >= totalBlocks) return ErrorNumber.NoError;

        // Double indirect
        if(indirectBlocks[1] != 0)
        {
            ErrorNumber errno = ReadIndirectBlock(indirectBlocks[1], 2, totalBlocks, blockList);

            if(errno != ErrorNumber.NoError) return errno;
        }
        else
        {
            long limit = Math.Min(totalBlocks, NDADDR + pointersPerBlock + (long)pointersPerBlock * pointersPerBlock);

            while(blockList.Count < limit) blockList.Add(0);
        }

        if(blockList.Count >= totalBlocks) return ErrorNumber.NoError;

        // Triple indirect
        if(indirectBlocks[2] != 0)
        {
            ErrorNumber errno = ReadIndirectBlock(indirectBlocks[2], 3, totalBlocks, blockList);

            if(errno != ErrorNumber.NoError) return errno;
        }
        else
        {
            while(blockList.Count < totalBlocks) blockList.Add(0);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Recursively reads indirect block pointers and appends to the block list</summary>
    ErrorNumber ReadIndirectBlock(long blockPointer, int level, long maxBlocks, List<long> blockList)
    {
        if(blockPointer == 0 || blockList.Count >= maxBlocks) return ErrorNumber.NoError;

        // Read the indirect block (it's a full filesystem block = fs_frag fragments)
        long fragAddr = blockPointer;

        ErrorNumber errno = ReadFragments(fragAddr, _superBlock.fs_frag, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        int pointersPerBlock = _superBlock.fs_nindir;

        if(level == 1)
        {
            // Leaf level: extract block pointers directly
            for(var i = 0; i < pointersPerBlock && blockList.Count < maxBlocks; i++)
            {
                long ptr;

                if(_superBlock.fs_isUfs2)
                {
                    ptr = _bigEndian
                              ? Swapping.Swap(BitConverter.ToInt64(data, i * 8))
                              : BitConverter.ToInt64(data, i * 8);
                }
                else
                {
                    ptr = _bigEndian
                              ? Swapping.Swap(BitConverter.ToInt32(data, i * 4))
                              : BitConverter.ToInt32(data, i * 4);
                }

                blockList.Add(ptr);
            }
        }
        else
        {
            // Intermediate level: each pointer points to next-level indirect block
            for(var i = 0; i < pointersPerBlock && blockList.Count < maxBlocks; i++)
            {
                long ptr;

                if(_superBlock.fs_isUfs2)
                {
                    ptr = _bigEndian
                              ? Swapping.Swap(BitConverter.ToInt64(data, i * 8))
                              : BitConverter.ToInt64(data, i * 8);
                }
                else
                {
                    ptr = _bigEndian
                              ? Swapping.Swap(BitConverter.ToInt32(data, i * 4))
                              : BitConverter.ToInt32(data, i * 4);
                }

                if(ptr == 0)
                {
                    // Sparse: compute how many leaf blocks this sub-tree covers
                    long leafCount = 1;

                    for(var l = 1; l < level; l++) leafCount *= pointersPerBlock;

                    long limit = Math.Min(maxBlocks, blockList.Count + leafCount);

                    while(blockList.Count < limit) blockList.Add(0);

                    continue;
                }

                errno = ReadIndirectBlock(ptr, level - 1, maxBlocks, blockList);

                if(errno != ErrorNumber.NoError) return errno;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads data from a file given its inode number, at the specified byte offset and length</summary>
    ErrorNumber ReadInodeData(uint inodeNumber, long offset, long length, out byte[] buffer)
    {
        buffer = null;

        // Read the inode and get its block list
        ulong  fileSize;
        long[] directBlocks;
        long[] indirectBlocks;

        if(_superBlock.fs_isUfs2)
        {
            ErrorNumber errno = ReadInode2(inodeNumber, out Inode2 inode2);

            if(errno != ErrorNumber.NoError) return errno;

            fileSize       = inode2.di_size;
            directBlocks   = inode2.di_db;
            indirectBlocks = inode2.di_ib;
        }
        else
        {
            ErrorNumber errno = ReadInode(inodeNumber, out Inode inode);

            if(errno != ErrorNumber.NoError) return errno;

            fileSize = inode.di_size;

            // Convert 32-bit block pointers to 64-bit
            directBlocks = new long[NDADDR];

            for(var i = 0; i < NDADDR; i++) directBlocks[i] = inode.di_db[i];

            indirectBlocks = new long[NIADDR];

            for(var i = 0; i < NIADDR; i++) indirectBlocks[i] = inode.di_ib[i];
        }

        if(offset >= (long)fileSize)
        {
            buffer = [];

            return ErrorNumber.NoError;
        }

        // Clamp length to file size
        if(offset + length > (long)fileSize) length = (long)fileSize - offset;

        if(length <= 0)
        {
            buffer = [];

            return ErrorNumber.NoError;
        }

        ErrorNumber err = GetBlockList(directBlocks, indirectBlocks, fileSize, out List<long> blockList);

        if(err != ErrorNumber.NoError) return err;

        int blockSize = _superBlock.fs_bsize;

        buffer = new byte[length];

        long bytesRead     = 0;
        long currentOffset = offset;

        while(bytesRead < length)
        {
            var  logicalBlock  = (int)(currentOffset / blockSize);
            var  offsetInBlock = (int)(currentOffset % blockSize);
            long remaining     = length - bytesRead;
            var  toRead        = (int)Math.Min(remaining, blockSize - offsetInBlock);

            if(logicalBlock >= blockList.Count) break;

            long fragAddr = blockList[logicalBlock];

            if(fragAddr == 0)
            {
                // Sparse block — return zeros (buffer is already zeroed)
                bytesRead     += toRead;
                currentOffset += toRead;

                continue;
            }

            // Read the full filesystem block (fs_frag fragments)
            ErrorNumber errno = ReadFragments(fragAddr, _superBlock.fs_frag, out byte[] blockData);

            if(errno != ErrorNumber.NoError) return errno;

            int copyLen = Math.Min(toRead, blockData.Length - offsetInBlock);

            if(copyLen > 0) Array.Copy(blockData, offsetInBlock, buffer, bytesRead, copyLen);

            bytesRead     += toRead;
            currentOffset += toRead;
        }

        return ErrorNumber.NoError;
    }
}