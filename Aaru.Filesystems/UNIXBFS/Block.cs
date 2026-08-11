// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Block.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UnixWare boot filesystem plugin.
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
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class BFS
{
    /// <summary>Reads a block from disk</summary>
    /// <param name="blockNumber">The block number</param>
    /// <param name="blockData">The block data</param>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber ReadBlock(uint blockNumber, out byte[] blockData)
    {
        blockData = null;

        // Convert block number to sector address
        ulong sectorAddress = (ulong)blockNumber * BFS_BSIZE / _imagePlugin.Info.SectorSize;
        uint  sectorsToRead = BFS_BSIZE                      / _imagePlugin.Info.SectorSize;

        if(sectorsToRead == 0) sectorsToRead = 1;

        if(_partition.Start + sectorAddress + sectorsToRead > _partition.End + 1)
        {
            AaruLogging.Debug(MODULE_NAME, "Block {0} is past partition end", blockNumber);

            return ErrorNumber.InvalidArgument;
        }

        ErrorNumber errno = _imagePlugin.ReadSectors(_partition.Start + sectorAddress,
                                                     false,
                                                     sectorsToRead,
                                                     out blockData,
                                                     out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading block {0}: {1}", blockNumber, errno);

            return errno;
        }

        // Ensure we have a full block
        if(blockData.Length < BFS_BSIZE)
        {
            var fullBlock = new byte[BFS_BSIZE];
            Array.Copy(blockData, 0, fullBlock, 0, blockData.Length);
            blockData = fullBlock;
        }

        return ErrorNumber.NoError;
    }
}