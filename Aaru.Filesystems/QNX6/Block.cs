// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Block.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : QNX6 filesystem plugin.
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
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class QNX6
{
    /// <summary>Reads a block from the filesystem</summary>
    /// <param name="blockNumber">The physical block number</param>
    /// <param name="blockData">The block data</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber ReadBlock(uint blockNumber, out byte[] blockData)
    {
        blockData = null;

        // Convert block number to sector address
        ulong sectorAddress = (ulong)blockNumber * _blockSize / _imagePlugin.Info.SectorSize;
        uint  sectorsToRead = _blockSize                      / _imagePlugin.Info.SectorSize;

        if(sectorsToRead == 0) sectorsToRead = 1;

        if(_partition.Start + sectorAddress + sectorsToRead > _partition.End + 1)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadBlock: Block {0} is past partition end", blockNumber);

            return ErrorNumber.InvalidArgument;
        }

        ErrorNumber errno = _imagePlugin.ReadSectors(_partition.Start + sectorAddress,
                                                     false,
                                                     sectorsToRead,
                                                     out blockData,
                                                     out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadBlock: Error reading block {0}: {1}", blockNumber, errno);

            return errno;
        }

        // Ensure we have a full block
        if(blockData.Length < _blockSize)
        {
            var fullBlock = new byte[_blockSize];
            Array.Copy(blockData, 0, fullBlock, 0, blockData.Length);
            blockData = fullBlock;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Maps a logical block to a physical block using the tree structure</summary>
    /// <remarks>
    ///     QNX6 uses a B+ tree structure for block mapping with up to 5 levels.
    ///     The root node contains 16 direct pointers. Each level adds indirection.
    /// </remarks>
    /// <param name="rootNode">The root node of the tree (from superblock)</param>
    /// <param name="logicalBlock">The logical block number within the tree</param>
    /// <param name="physicalBlock">The physical block number on disk</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber MapBlock(qnx6_root_node rootNode, uint logicalBlock, out uint physicalBlock)
    {
        physicalBlock = 0;

        int depth    = rootNode.levels;
        var ptrBits  = (int)Math.Log2(_blockSize / 4); // Number of bits per pointer level
        int bitDelta = ptrBits * depth;

        // Calculate the index into the direct pointers
        var levelPtr = (int)(logicalBlock >> bitDelta);

        if(levelPtr > QNX6_NO_DIRECT_POINTERS - 1)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "MapBlock: Requested block {0} too big (levelPtr={1})",
                              logicalBlock,
                              levelPtr);

            return ErrorNumber.InvalidArgument;
        }

        // Get the block from the direct pointer, add block offset
        physicalBlock = rootNode.ptr[levelPtr] + _blockOffset;

        // If no indirection levels, we're done
        if(depth == 0) return ErrorNumber.NoError;

        var mask = (uint)((1 << ptrBits) - 1);

        // Traverse the indirect levels
        for(var i = 0; i < depth; i++)
        {
            // Read the indirect block
            ErrorNumber errno = ReadBlock(physicalBlock, out byte[] indirectBlock);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "MapBlock: Error reading indirect block {0}", physicalBlock);

                return errno;
            }

            bitDelta -= ptrBits;
            levelPtr =  (int)(logicalBlock >> bitDelta & mask);

            // Get the pointer from the indirect block (array of uint32)
            uint ptr = _littleEndian
                           ? BitConverter.ToUInt32(indirectBlock, levelPtr          * 4)
                           : BigEndianBitConverter.ToUInt32(indirectBlock, levelPtr * 4);

            // Check for unused block pointer (0xFFFFFFFF)
            if(ptr == 0xFFFFFFFF)
            {
                AaruLogging.Debug(MODULE_NAME, "MapBlock: Hit unused block pointer at level {0}", i);

                return ErrorNumber.InvalidArgument;
            }

            physicalBlock = ptr + _blockOffset;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Maps a logical block to a physical block using an inode's block pointers</summary>
    /// <param name="inode">The inode entry</param>
    /// <param name="logicalBlock">The logical block number within the file</param>
    /// <param name="physicalBlock">The physical block number on disk</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber MapBlock(qnx6_inode_entry inode, uint logicalBlock, out uint physicalBlock)
    {
        physicalBlock = 0;

        int depth    = inode.di_filelevels;
        var ptrBits  = (int)Math.Log2(_blockSize / 4);
        int bitDelta = ptrBits * depth;

        var levelPtr = (int)(logicalBlock >> bitDelta);

        AaruLogging.Debug(MODULE_NAME,
                          "MapBlock(inode): logicalBlock={0}, depth={1}, ptrBits={2}, bitDelta={3}, levelPtr={4}",
                          logicalBlock,
                          depth,
                          ptrBits,
                          bitDelta,
                          levelPtr);

        if(levelPtr > QNX6_NO_DIRECT_POINTERS - 1)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "MapBlock: Requested block {0} too big (levelPtr={1})",
                              logicalBlock,
                              levelPtr);

            return ErrorNumber.InvalidArgument;
        }

        if(inode.di_block_ptr == null || levelPtr >= inode.di_block_ptr.Length)
        {
            AaruLogging.Debug(MODULE_NAME, "MapBlock: di_block_ptr is null or levelPtr out of range");

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "MapBlock(inode): di_block_ptr[{0}]={1}, _blockOffset={2}, result={3}",
                          levelPtr,
                          inode.di_block_ptr[levelPtr],
                          _blockOffset,
                          inode.di_block_ptr[levelPtr] + _blockOffset);

        physicalBlock = inode.di_block_ptr[levelPtr] + _blockOffset;

        AaruLogging.Debug(MODULE_NAME, "MapBlock(inode): physicalBlock={0}", physicalBlock);

        if(depth == 0) return ErrorNumber.NoError;

        var mask = (uint)((1 << ptrBits) - 1);

        for(var i = 0; i < depth; i++)
        {
            ErrorNumber errno = ReadBlock(physicalBlock, out byte[] indirectBlock);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "MapBlock: Error reading indirect block {0}", physicalBlock);

                return errno;
            }

            bitDelta -= ptrBits;
            levelPtr =  (int)(logicalBlock >> bitDelta & mask);

            uint ptr = _littleEndian
                           ? BitConverter.ToUInt32(indirectBlock, levelPtr          * 4)
                           : BigEndianBitConverter.ToUInt32(indirectBlock, levelPtr * 4);

            if(ptr == 0xFFFFFFFF)
            {
                AaruLogging.Debug(MODULE_NAME, "MapBlock: Hit unused block pointer at level {0}", i);

                return ErrorNumber.InvalidArgument;
            }

            physicalBlock = ptr + _blockOffset;
        }

        return ErrorNumber.NoError;
    }
}