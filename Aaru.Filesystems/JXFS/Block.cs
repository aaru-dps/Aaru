// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Block.cs
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
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class JXFS
{
    /// <summary>Reads a block of a volume</summary>
    /// <param name="imagePlugin">Image to read from</param>
    /// <param name="partition">Partition that holds the volume</param>
    /// <param name="blockSize">Block size of the volume</param>
    /// <param name="block">Block to read, relative to the partition</param>
    /// <param name="data">The block contents</param>
    /// <returns>Error number</returns>
    static ErrorNumber ReadBlock(IMediaImage imagePlugin, Partition partition, uint blockSize, ulong block,
                                 out byte[] data)
    {
        data = null;

        uint  sectorSize = imagePlugin.Info.SectorSize;
        ulong byteOffset = block * blockSize;
        ulong sector     = byteOffset / sectorSize;
        int   inSector   = (int)(byteOffset % sectorSize);
        uint  count      = (uint)((inSector + blockSize + sectorSize - 1) / sectorSize);

        if(partition.Start + sector + count - 1 > partition.End) return ErrorNumber.InvalidArgument;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + sector, false, count, out byte[] buffer, out _);

        if(errno != ErrorNumber.NoError) return errno;

        if(buffer.Length < inSector + blockSize) return ErrorNumber.InvalidArgument;

        if(inSector == 0 && buffer.Length == blockSize)
        {
            data = buffer;

            return ErrorNumber.NoError;
        }

        data = new byte[blockSize];
        Array.Copy(buffer, inSector, data, 0, blockSize);

        return ErrorNumber.NoError;
    }

    /// <summary>Checks a block checksum: the seed plus the sum of all big-endian words must be 0</summary>
    /// <param name="data">Block contents</param>
    /// <returns><c>true</c> if the checksum is valid</returns>
    static bool ValidChecksum(byte[] data)
    {
        uint sum = CHECKSUM_SEED;

        for(int i = 0; i + 4 <= data.Length; i += 4) sum += BigEndianBitConverter.ToUInt32(data, i);

        return sum == 0;
    }

    /// <summary>Reads a block and checks its tag, checksum and own block number</summary>
    /// <param name="block">Block to read, relative to the partition</param>
    /// <param name="tag">Expected tag</param>
    /// <param name="data">The block contents</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadTaggedBlock(ulong block, uint tag, out byte[] data)
    {
        ErrorNumber errno = ReadBlock(_imagePlugin, _partition, _blockSize, block, out data);

        if(errno != ErrorNumber.NoError) return errno;

        uint blockTag = BigEndianBitConverter.ToUInt32(data, 0);
        uint ownBlock = BigEndianBitConverter.ToUInt32(data, 8);

        if(blockTag != tag)
        {
            AaruLogging.Debug(MODULE_NAME, "Block {0} has tag 0x{1:X8}, expected 0x{2:X8}", block, blockTag, tag);

            return ErrorNumber.InvalidArgument;
        }

        if(!ValidChecksum(data))
        {
            AaruLogging.Debug(MODULE_NAME, "Block {0} has an invalid checksum", block);

            return ErrorNumber.InvalidArgument;
        }

        if(ownBlock == block) return ErrorNumber.NoError;

        AaruLogging.Debug(MODULE_NAME, "Block {0} says it is block {1}", block, ownBlock);

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Finds which volume block holds a block of a file stored in extents</summary>
    /// <param name="extents">Extents of the file</param>
    /// <param name="relative">Block number inside the file</param>
    /// <param name="block">Block number in the volume</param>
    /// <returns>Error number</returns>
    static ErrorNumber MapBlock(List<Extent> extents, ulong relative, out ulong block)
    {
        block = 0;

        foreach(Extent extent in extents)
        {
            if(relative < extent.Blocks)
            {
                block = extent.Start + relative;

                return ErrorNumber.NoError;
            }

            relative -= extent.Blocks;
        }

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Reads a block of a system file and checks its tag, checksum and block numbers</summary>
    /// <param name="extents">Extents of the system file</param>
    /// <param name="relative">Block number inside the system file</param>
    /// <param name="tag">Expected tag</param>
    /// <param name="data">The block contents</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadSystemFileBlock(List<Extent> extents, uint relative, uint tag, out byte[] data)
    {
        data = null;

        ErrorNumber errno = MapBlock(extents, relative, out ulong block);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Block {0} is beyond the end of its system file", relative);

            return errno;
        }

        errno = ReadTaggedBlock(block, tag, out data);

        if(errno != ErrorNumber.NoError) return errno;

        uint stored = BigEndianBitConverter.ToUInt32(data, RELATIVE_BLOCK_OFFSET);

        if(stored == relative) return ErrorNumber.NoError;

        AaruLogging.Debug(MODULE_NAME, "Block {0} says it is system file block {1}, expected {2}", block, stored,
                          relative);

        return ErrorNumber.InvalidArgument;
    }
}