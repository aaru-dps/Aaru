// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Map.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     File mapping operations for the Files-11 On-Disk Structure.
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

namespace Aaru.Filesystems;

public sealed partial class ODS
{
    /// <summary>Gets the mapping data from a file header.</summary>
    /// <param name="header">File header.</param>
    /// <returns>Mapping data bytes.</returns>
    static byte[] GetMapData(FileHeader header)
    {
        // Map area starts at mpoffset words from start of header
        // Map area size in use is map_inuse words
        int mapOffset = header.mpoffset  * 2;
        int mapSize   = header.map_inuse * 2;

        if(mapSize <= 0) return null;

        // The map data is within the reserved area of the file header
        // We need to extract it from the header structure
        // The reserved area starts at offset 0x50 and is 430 bytes
        // But the map is at mpoffset*2 from the start of the header

        // Since we can't easily access raw header bytes from the struct,
        // we need to re-read or use a different approach
        // For now, return the reserved area portion that contains the map
        if(header.reserved == null || header.reserved.Length < mapSize) return null;

        // Map offset relative to reserved area start (0x50)
        const int reservedStart      = 0x50;
        int       mapOffsetInReserve = mapOffset - reservedStart;

        if(mapOffsetInReserve < 0 || mapOffsetInReserve + mapSize > header.reserved.Length) return null;

        var mapData = new byte[mapSize];
        Array.Copy(header.reserved, mapOffsetInReserve, mapData, 0, mapSize);

        return mapData;
    }

    /// <summary>Maps a Virtual Block Number (VBN) to a Logical Block Number (LBN).</summary>
    /// <param name="mapData">Mapping data from file header.</param>
    /// <param name="mapInUse">Number of words in use in the map.</param>
    /// <param name="vbn">Virtual block number (1-based).</param>
    /// <param name="lbn">Output logical block number (0-based).</param>
    /// <param name="extent">Output extent size.</param>
    /// <returns>Error number indicating success or failure.</returns>
    static ErrorNumber MapVbnToLbn(byte[] mapData, byte mapInUse, uint vbn, out uint lbn, out uint extent) =>
        MapVbnToLbnWithSum(mapData, mapInUse, vbn, 0, out lbn, out extent, out _);

    /// <summary>Maps a Virtual Block Number (VBN) to a Logical Block Number (LBN) with a starting VBN sum.</summary>
    /// <param name="mapData">Mapping data from file header.</param>
    /// <param name="mapInUse">Number of words in use in the map.</param>
    /// <param name="vbn">Virtual block number (1-based).</param>
    /// <param name="startSum">Starting VBN sum (for extension headers).</param>
    /// <param name="lbn">Output logical block number (0-based).</param>
    /// <param name="extent">Output extent size.</param>
    /// <param name="endSum">Output ending VBN sum after processing this map.</param>
    /// <returns>Error number indicating success or failure.</returns>
    internal static ErrorNumber MapVbnToLbnWithSum(byte[] mapData, byte mapInUse, uint vbn, uint startSum, out uint lbn,
                                          out uint extent, out uint endSum)
    {
        lbn    = 0;
        extent = 0;
        endSum = startSum;

        if(mapData == null || mapData.Length == 0) return ErrorNumber.InvalidArgument;

        uint sum = startSum;
        var  i   = 0;

        while(i < mapInUse * 2 && i < mapData.Length)
        {
            // Read format from high 2 bits of first word
            var word0  = BitConverter.ToUInt16(mapData, i);
            var format = (byte)(word0 >> 14 & 0x03);

            uint count;
            uint diskLbn;

            switch(format)
            {
                case 0:
                    // Placement control - skip
                    i += 2;

                    continue;

                case 1:
                    // 8-bit count, 22-bit LBN (4 bytes total)
                    if(i + 4 > mapData.Length)
                    {
                        endSum = sum;

                        return ErrorNumber.InvalidArgument;
                    }

                    count   =  (uint)(mapData[i] + 1);
                    diskLbn =  (uint)((mapData[i + 1] & 0x3F) << 16 | BitConverter.ToUInt16(mapData, i + 2));
                    i       += 4;

                    break;

                case 2:
                    // 14-bit count, 32-bit LBN (6 bytes total)
                    if(i + 6 > mapData.Length)
                    {
                        endSum = sum;

                        return ErrorNumber.InvalidArgument;
                    }

                    count   =  (uint)((word0 & 0x3FFF) + 1);
                    diskLbn =  BitConverter.ToUInt32(mapData, i + 2);
                    i       += 6;

                    break;

                case 3:
                    // 30-bit count, 32-bit LBN (8 bytes total)
                    if(i + 8 > mapData.Length)
                    {
                        endSum = sum;

                        return ErrorNumber.InvalidArgument;
                    }

                    var lowCount = BitConverter.ToUInt16(mapData, i + 2);
                    count   =  (uint)(((word0 & 0x3FFF) << 16 | lowCount) + 1);
                    diskLbn =  BitConverter.ToUInt32(mapData, i + 4);
                    i       += 8;

                    break;

                default:
                    endSum = sum;

                    return ErrorNumber.InvalidArgument;
            }

            // Check if this extent contains our VBN. The lower bound matters when startSum is non-zero
            // (an extension header map): a VBN below this map's base must not match its first extent.
            if(vbn > sum && vbn <= sum + count)
            {
                lbn    = diskLbn + (vbn - sum) - 1;
                extent = count   - (lbn - diskLbn);
                endSum = sum     + count;

                return ErrorNumber.NoError;
            }

            sum += count;
        }

        endSum = sum;

        // VBN not found in this map - may need extension header
        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Calculates the total VBN count covered by a map.</summary>
    /// <param name="mapData">Mapping data from file header.</param>
    /// <param name="mapInUse">Number of words in use in the map.</param>
    /// <returns>Total VBN count.</returns>
    static uint GetMapVbnCount(byte[] mapData, byte mapInUse)
    {
        if(mapData == null || mapData.Length == 0) return 0;

        uint sum = 0;
        var  i   = 0;

        while(i < mapInUse * 2 && i < mapData.Length)
        {
            var word0  = BitConverter.ToUInt16(mapData, i);
            var format = (byte)(word0 >> 14 & 0x03);

            uint count;

            switch(format)
            {
                case 0:
                    i += 2;

                    continue;

                case 1:
                    if(i + 4 > mapData.Length) return sum;

                    count =  (uint)(mapData[i] + 1);
                    i     += 4;

                    break;

                case 2:
                    if(i + 6 > mapData.Length) return sum;

                    count =  (uint)((word0 & 0x3FFF) + 1);
                    i     += 6;

                    break;

                case 3:
                    if(i + 8 > mapData.Length) return sum;

                    var lowCount = BitConverter.ToUInt16(mapData, i + 2);
                    count =  (uint)(((word0 & 0x3FFF) << 16 | lowCount) + 1);
                    i     += 8;

                    break;

                default:
                    return sum;
            }

            sum += count;
        }

        return sum;
    }
}