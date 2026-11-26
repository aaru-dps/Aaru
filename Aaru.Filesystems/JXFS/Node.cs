// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Node.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class JXFS
{
    /// <summary>Decodes a node</summary>
    /// <param name="data">Node container block</param>
    /// <param name="offset">Offset of the node in the block</param>
    /// <returns>The node</returns>
    static Node DecodeNode(byte[] data, int offset)
    {
        return new Node
        {
            Number         = BigEndianBitConverter.ToUInt32(data, offset),
            Protection     = BigEndianBitConverter.ToUInt32(data, offset + 0x04),
            AttributeBlock = BigEndianBitConverter.ToUInt32(data, offset + 0x08),
            Size = (ulong)BigEndianBitConverter.ToUInt16(data, offset + 0x0C) << 32 |
                   BigEndianBitConverter.ToUInt32(data, offset + 0x0E),
            Pointer     = BigEndianBitConverter.ToUInt32(data, offset + 0x0C),
            Extent1     = Read48(data, offset + 0x12),
            Extent2     = Read48(data, offset + 0x18),
            ExtentBlock = BigEndianBitConverter.ToUInt32(data, offset + 0x1E),
            Flags       = (NodeFlags)BigEndianBitConverter.ToUInt16(data, offset + 0x22),
            Date        = (ulong)data[offset + 0x24] << 32 | BigEndianBitConverter.ToUInt32(data, offset + 0x25)
        };
    }

    /// <summary>Reads a 48-bit big-endian number</summary>
    /// <param name="data">Buffer</param>
    /// <param name="offset">Offset of the number</param>
    /// <returns>The number</returns>
    static ulong Read48(byte[] data, int offset)
    {
        return (ulong)BigEndianBitConverter.ToUInt32(data, offset) << 16 |
               BigEndianBitConverter.ToUInt16(data, offset + 4);
    }

    /// <summary>Gets the first block of an extent, the top 28 bits of it, in units of 128 blocks</summary>
    /// <param name="extent">48-bit extent</param>
    /// <returns>Extent start, in units</returns>
    static uint ExtentStart(ulong extent)
    {
        return (uint)(extent >> 20) & 0x1FFFFFF;
    }

    /// <summary>Gets the length of an extent, the low 20 bits of it, in units of 128 blocks</summary>
    /// <param name="extent">48-bit extent</param>
    /// <returns>Extent length, in units</returns>
    static uint ExtentLength(ulong extent)
    {
        return (uint)(extent & 0xFFFFF);
    }

    /// <summary>Checks if a file keeps its data in a chain of small data blocks instead of extents</summary>
    /// <param name="node">File node</param>
    /// <returns><c>true</c> for small files</returns>
    static bool IsSmallFile(Node node)
    {
        return ExtentLength(node.Extent1) == 0;
    }

    /// <summary>Gets the first data block of a small file, relative to <c>:misc:</c></summary>
    /// <param name="node">File node</param>
    /// <returns>First data block, 0 for an empty file</returns>
    static uint FirstDataBlock(Node node)
    {
        return (uint)(node.Extent1 >> 20) & 0xFFFFFFF;
    }

    /// <summary>Converts a node date to a <see cref="DateTime" /></summary>
    /// <param name="ticks">Ticks of 1/50 seconds since 1978-01-01</param>
    /// <returns>The date</returns>
    static DateTime TicksToDateTime(ulong ticks)
    {
        return _amigaEpoch.AddTicks((long)ticks * (TimeSpan.TicksPerSecond / 50));
    }

    /// <summary>Reads a node</summary>
    /// <param name="number">Node number</param>
    /// <param name="node">The node</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadNode(uint number, out Node node)
    {
        node = default(Node);

        if(number == 0) return ErrorNumber.InvalidArgument;

        uint        index = number - 1;
        ErrorNumber errno;
        byte[]      data;

        // The filesystem reads the first nodes straight from the first node container, as they describe :node:
        if(number <= LAST_BOOTSTRAP_NODE || _nodeFile is null)
        {
            if(index >= _nodesPerBlock) return ErrorNumber.InvalidArgument;

            errno = ReadTaggedBlock(_nodeFileStart + 1, NODECONTAINER_ID, out data);
        }
        else
            errno = ReadSystemFileBlock(_nodeFile, 1 + index / _nodesPerBlock, NODECONTAINER_ID, out data);

        if(errno != ErrorNumber.NoError) return errno;

        node = DecodeNode(data, NODECONTAINER_HEADER_SIZE + (int)(index % _nodesPerBlock) * NODE_SIZE);

        if(node.Number == number) return ErrorNumber.NoError;

        AaruLogging.Debug(MODULE_NAME, "Node {0} slot holds node {1}", number, node.Number);

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Converts a 48-bit extent to a run of blocks</summary>
    /// <param name="extent">48-bit extent</param>
    /// <returns>The run of blocks</returns>
    static Extent DecodeExtent(ulong extent)
    {
        return new Extent
        {
            Start  = (ulong)ExtentStart(extent)  << EXTENT_UNIT_SHIFT,
            Blocks = (ulong)ExtentLength(extent) << EXTENT_UNIT_SHIFT
        };
    }

    /// <summary>Gets the extents of a file that keeps its data in extents</summary>
    /// <param name="node">File node</param>
    /// <param name="extents">The extents, in file order</param>
    /// <returns>Error number</returns>
    ErrorNumber GetExtents(Node node, out List<Extent> extents)
    {
        extents = [];

        if(ExtentLength(node.Extent1) == 0) return ErrorNumber.NoError;

        extents.Add(DecodeExtent(node.Extent1));

        if(ExtentLength(node.Extent2) == 0) return ErrorNumber.NoError;

        extents.Add(DecodeExtent(node.Extent2));

        // Extent container blocks are in :misc:. While :misc: itself is being loaded its inline extents are used.
        List<Extent> misc = _miscFile ?? extents;

        uint          containerBlock = node.ExtentBlock;
        HashSet<uint> visited = new HashSet<uint>();

        while(containerBlock != 0)
        {
            if(!visited.Add(containerBlock) || visited.Count > MAX_CHAIN_LENGTH) return ErrorNumber.InvalidArgument;

            ErrorNumber errno = ReadSystemFileBlock(misc, containerBlock, EXTENTCONTAINER_ID, out byte[] data);

            if(errno != ErrorNumber.NoError) return errno;

            for(int offset = EXTENTCONTAINER_HEADER_SIZE; offset + EXTENT_SIZE <= data.Length; offset += EXTENT_SIZE)
            {
                ulong extent = Read48(data, offset);

                if(ExtentStart(extent) == 0) break;

                extents.Add(DecodeExtent(extent));
            }

            containerBlock = BigEndianBitConverter.ToUInt32(data, EXTENTCONTAINER_NEXT_OFFSET);
        }

        return ErrorNumber.NoError;
    }
}