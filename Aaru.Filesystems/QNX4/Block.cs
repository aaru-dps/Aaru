// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Block.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : QNX4 filesystem plugin.
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
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class QNX4
{
    /// <summary>Reads a block from the filesystem</summary>
    /// <param name="blockNumber">The block number to read (1-based in QNX4)</param>
    /// <param name="blockData">The block data</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber ReadBlock(uint blockNumber, out byte[] blockData)
    {
        blockData = null;

        if(blockNumber == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadBlock: Invalid block number 0");

            return ErrorNumber.InvalidArgument;
        }

        // QNX4 block numbers are 1-based, convert to 0-based sector
        ulong sectorAddress = blockNumber - 1;

        if(_partition.Start + sectorAddress > _partition.End)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadBlock: Block {0} is past partition end", blockNumber);

            return ErrorNumber.InvalidArgument;
        }

        ErrorNumber errno = _imagePlugin.ReadSector(_partition.Start + sectorAddress, false, out blockData, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadBlock: Error reading block {0}: {1}", blockNumber, errno);

            return errno;
        }

        // Ensure we have a full block
        if(blockData.Length < QNX4_BLOCK_SIZE)
        {
            var fullBlock = new byte[QNX4_BLOCK_SIZE];
            Array.Copy(blockData, 0, fullBlock, 0, blockData.Length);
            blockData = fullBlock;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Maps a logical block offset to a physical block using extent information</summary>
    /// <remarks>
    ///     Implements the qnx4_block_map logic from Linux kernel.
    ///     QNX4 uses extents to map logical blocks to physical blocks.
    ///     The first extent is in the inode, additional extents are in extent blocks (xblk).
    /// </remarks>
    /// <param name="inode">The file inode entry</param>
    /// <param name="logicalBlock">The logical block offset within the file</param>
    /// <param name="physicalBlock">The physical block number (1-based)</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber MapBlock(in qnx4_inode_entry inode, uint logicalBlock, out uint physicalBlock)
    {
        physicalBlock = 0;

        uint offset = logicalBlock;

        // Try the first extent in the inode
        uint extentSize = inode.di_first_xtnt.xtnt_size;

        if(offset < extentSize)
        {
            // Block is within first extent
            physicalBlock = inode.di_first_xtnt.xtnt_blk + offset;

            return ErrorNumber.NoError;
        }

        // Block is beyond first extent, need to follow extent chain
        offset -= extentSize;

        ushort numExtents = inode.di_num_xtnts;

        if(numExtents <= 1)
        {
            // No more extents
            AaruLogging.Debug(MODULE_NAME, "MapBlock: Block {0} beyond file extents", logicalBlock);

            return ErrorNumber.InvalidArgument;
        }

        uint xblkNum = inode.di_xblk;
        numExtents--; // Already processed first extent

        while(numExtents > 0)
        {
            if(xblkNum == 0)
            {
                AaruLogging.Debug(MODULE_NAME, "MapBlock: Extent chain ended prematurely");

                return ErrorNumber.InvalidArgument;
            }

            // Read extent block
            ErrorNumber errno = ReadBlock(xblkNum, out byte[] xblkData);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "MapBlock: Error reading extent block {0}", xblkNum);

                return errno;
            }

            // Parse extent block
            qnx4_xblk xblk = Marshal.ByteArrayToStructureLittleEndian<qnx4_xblk>(xblkData);

            // Validate signature
            string signature = Encoding.ASCII.GetString(xblk.xblk_signature, 0, 7);

            if(signature != "IamXblk")
            {
                AaruLogging.Debug(MODULE_NAME, "MapBlock: Invalid extent block signature at block {0}", xblkNum);

                return ErrorNumber.InvalidArgument;
            }

            // Try each extent in this block
            for(var i = 0; i < xblk.xblk_num_xtnts && numExtents > 0; i++)
            {
                qnx4_xtnt_t extent = xblk.xblk_xtnts[i];
                extentSize = extent.xtnt_size;

                if(offset < extentSize)
                {
                    // Found it!
                    physicalBlock = extent.xtnt_blk + offset;

                    return ErrorNumber.NoError;
                }

                offset -= extentSize;
                numExtents--;
            }

            // Move to next extent block
            xblkNum = xblk.xblk_next_xblk;
        }

        AaruLogging.Debug(MODULE_NAME, "MapBlock: Block {0} not found in extents", logicalBlock);

        return ErrorNumber.InvalidArgument;
    }
}