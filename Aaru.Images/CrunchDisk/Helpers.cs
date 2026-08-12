// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helper methods for CrunchDisk disk images.
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

namespace Aaru.Images;

public sealed partial class CrunchDisk
{
    /// <summary>Decompresses PowerPacker compressed data</summary>
    /// <param name="source">Compressed data</param>
    /// <param name="packedLength">Length of compressed data</param>
    /// <param name="unpackedLength">Expected length of decompressed data</param>
    /// <param name="offsetSizes">Bit widths for offset encoding per index (4 entries)</param>
    /// <returns>Decompressed data</returns>
    static byte[] PowerPackerDecompress(byte[] source, int packedLength, int unpackedLength, byte[] offsetSizes)
    {
        var output = new byte[unpackedLength];

        if(packedLength < 5 || packedLength > source.Length) return output;

        int  srcPos   = packedLength - 4;
        int  dstPos   = unpackedLength;
        uint shiftReg = 1u << 8;

        // Skip the number of bits indicated by the last byte of compressed data
        SkipBits(source, ref srcPos, ref shiftReg, source[packedLength - 1]);

        while(dstPos > 0)
        {
            // Determine whether we need to copy literal bytes
            uint flag = ReadBits(source, ref srcPos, ref shiftReg, 1);

            if(flag == 0)
            {
                // Read run of literal bytes: at least 1, extended in groups of 3
                uint count = 1;
                uint extra;

                do
                {
                    extra =  ReadBits(source, ref srcPos, ref shiftReg, 2);
                    count += extra;
                } while(extra == 3);

                // Copy literal bytes from the bitstream
                while(count-- > 0)
                {
                    if(--dstPos < 0) return output;

                    output[dstPos] = (byte)ReadBits(source, ref srcPos, ref shiftReg, 8);
                }
            }

            // Decode a back-reference from already-decompressed data
            uint idx    = ReadBits(source, ref srcPos, ref shiftReg, 2);
            int  nBits  = offsetSizes[idx];
            uint length = idx + 2;
            uint offset;

            if(idx == 3)
            {
                // Extended match: variable offset width and extended length
                uint useFullBits = ReadBits(source, ref srcPos, ref shiftReg, 1);
                offset = ReadBits(source, ref srcPos, ref shiftReg, useFullBits != 0 ? nBits : 7);

                // Read additional length in groups of 7
                uint lengthExtra;

                do
                {
                    lengthExtra =  ReadBits(source, ref srcPos, ref shiftReg, 3);
                    length      += lengthExtra;
                } while(lengthExtra == 7);
            }
            else
                offset = ReadBits(source, ref srcPos, ref shiftReg, nBits);

            offset++;

            // Copy bytes from previous output
            while(length-- > 0)
            {
                if(--dstPos < 0) return output;

                if(dstPos + offset >= output.Length) return output;

                output[dstPos] = output[dstPos + offset];
            }
        }

        return output;
    }

    /// <summary>Reads a number of bits from the PowerPacker bitstream (backward, LSB-first)</summary>
    static uint ReadBits(byte[] source, ref int srcPos, ref uint shiftReg, int count)
    {
        uint result = 0;

        for(var i = 0; i < count; i++)
        {
            if((shiftReg & 1u << 8) != 0)
            {
                if(srcPos <= 0) return result;

                shiftReg = 1u << 16 | source[--srcPos];
            }

            result   =   result << 1 | shiftReg & 1;
            shiftReg >>= 1;
        }

        return result;
    }

    /// <summary>Skips a number of bits in the PowerPacker bitstream</summary>
    static void SkipBits(byte[] source, ref int srcPos, ref uint shiftReg, int count)
    {
        for(var i = 0; i < count; i++)
        {
            if((shiftReg & 1u << 8) != 0)
            {
                if(srcPos <= 0) return;

                shiftReg = 1u << 16 | source[--srcPos];
            }

            shiftReg >>= 1;
        }
    }

    /// <summary>
    ///     De-interleaves cylinder data from CrunchDisk's 4-pass byte ordering back to sequential sector data.
    ///     In CrunchDisk format, each sector's bytes are stored as all first bytes, then all second bytes,
    ///     then third bytes, then fourth bytes (interleaved in 4-byte strides).
    /// </summary>
    /// <param name="source">Interleaved source data</param>
    /// <param name="destination">Output buffer for de-interleaved data</param>
    /// <param name="blockSize">Size of each sector in bytes</param>
    /// <param name="numBlocks">Number of sectors in the cylinder</param>
    /// <param name="sourceSize">Actual size of source data available</param>
    static void ResortCylinderData(byte[] source, byte[] destination, uint blockSize, uint numBlocks, uint sourceSize)
    {
        var srcIdx  = 0;
        var dstBase = 0;

        for(uint block = 0; block < numBlocks; block++)
        {
            int blockEnd = dstBase + (int)blockSize;

            if(sourceSize >= blockSize)
            {
                for(var pass = 0; pass < 4; pass++)
                {
                    for(int p = dstBase + pass; p < blockEnd; p += 4) destination[p] = source[srcIdx++];
                }

                sourceSize -= blockSize;
            }
            else
                Array.Clear(destination, dstBase, (int)blockSize);

            dstBase = blockEnd;
        }
    }

    /// <summary>Computes PowerPacker offset bit widths from the efficiency parameter</summary>
    /// <param name="efficiency">Efficiency level 1-4</param>
    /// <returns>Array of 4 bit widths for offset encoding</returns>
    static byte[] ComputeOffsetSizes(ushort efficiency)
    {
        byte[] sizes = [9, 9, 9, 9];

        // Each efficiency level progressively adds bits to the offset sizes
        // Higher levels use wider offsets for better compression of larger data
        switch(efficiency)
        {
            case >= 4:
                sizes[3]++;

                goto case 3;
            case 3:
                sizes[3]++;
                sizes[2]++;

                goto case 2;
            case 2:
                sizes[3]++;
                sizes[2]++;

                goto case 1;
            case 1:
                sizes[3]++;
                sizes[2]++;
                sizes[1]++;

                break;
        }

        return sizes;
    }
}