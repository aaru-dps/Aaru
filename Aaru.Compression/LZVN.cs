// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LZVN.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Compression algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the LZVN compression algorithm based on Apple's reference
//     implementation from lzvn_decode_base.c
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
// Based on Apple's LZVN reference implementation
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Aaru.Compression;

// ReSharper disable once InconsistentNaming
/// <summary>Implements the LZVN compression algorithm (Apple's LZVN)</summary>
public static partial class LZVN
{
    /// <summary>Set to <c>true</c> if this algorithm is supported, <c>false</c> otherwise.</summary>
    public static bool IsSupported => Native.IsSupported;

    /// <summary>
    /// AARU_EXPORT int32_t AARU_CALL AARU_lzvn_decode_buffer(const uint8_t *src_buffer, size_t src_size, uint8_t *dst_buffer, size_t *dst_size)
    /// </summary>
    /// <param name="src_buffer">Buffer that contains the compressed data</param>
    /// <param name="src_size">Size of the source buffer</param>
    /// <param name="dst_buffer">Buffer to write the decompressed data to</param>
    /// <param name="dst_size">On entry, size of the destination buffer. On return, number of bytes written</param>
    /// <returns>0 on success, non-zero on error</returns>
    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_lzvn_decode_buffer(byte[]    src_buffer, nuint src_size, byte[] dst_buffer,
                                                       ref nuint dst_size);

    /// <summary>
    /// AARU_EXPORT int32_t AARU_CALL AARU_lzvn_encode_buffer(const uint8_t *src_buffer, size_t src_size, uint8_t *dst_buffer, size_t *dst_size, void *scratch_buffer)
    /// </summary>
    /// <param name="src_buffer">Buffer that contains the data to compress</param>
    /// <param name="src_size">Size of the source buffer</param>
    /// <param name="dst_buffer">Buffer to write the compressed data to</param>
    /// <param name="dst_size">On entry, size of the destination buffer. On return, number of bytes written</param>
    /// <param name="scratch_buffer">Scratch buffer</param>
    /// <returns>0 on success, non-zero on error</returns>
    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_lzvn_encode_buffer(byte[]    src_buffer, nuint  src_size, byte[] dst_buffer,
                                                       ref nuint dst_size,   byte[] scratch_buffer);

    /// <summary>Decodes a buffer compressed with LZVN</summary>
    /// <param name="source">Compressed buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes, or -1 on error</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination)
    {
        if(!Native.IsSupported) return DecodeBufferManaged(source, destination);

        if(source == null || destination == null) return -1;

        nuint dstSize = (nuint)destination.Length;

        return AARU_lzvn_decode_buffer(source, (nuint)source.Length, destination, ref dstSize) == 0
                   ? (int)dstSize
                   : -1;
    }

    /// <summary>
    /// Compressed a buffer using the LZVN algorithm.
    /// </summary>
    /// <param name="source">Buffer with the data to compress.</param>
    /// <param name="destination">Buffer where to write the compressed data.</param>
    /// <returns>The size of the compressed data, or -1 on error.</returns>
    public static long EncodeBuffer(byte[] source, byte[] destination)
    {
        if(!Native.IsSupported) return -1;

        if(source == null || destination == null) return -1;

        byte[] scratch = new byte[1048576]; // LZVN requires a 512KB scratch buffer for encoding, let's give it twice that

        nuint dstSize = (nuint)destination.LongLength;

        return AARU_lzvn_encode_buffer(source, (nuint)source.LongLength, destination, ref dstSize, scratch) == 0
                   ? (long)dstSize
                   : -1;
    }

    /// <summary>Decodes a buffer compressed with LZVN</summary>
    /// <param name="source">Compressed buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes, or -1 on error</returns>
    public static int DecodeBufferManaged(byte[] source, byte[] destination)
    {
        if(source == null || destination == null) return -1;

        var srcPos    = 0;
        int srcEnd    = source.Length;
        var dstPos    = 0;
        int dstEnd    = destination.Length;
        var dstBegin  = 0;
        var matchDist = 0; // Previous match distance (d_prev)

        while(srcPos < srcEnd && dstPos < dstEnd)
        {
            byte opc = source[srcPos];

            int literalLen;
            int matchLen;
            int opcLen;

            // Decode based on opcode ranges from Apple's implementation
            if(opc >= 224)
            {
                // Literal only opcodes: 224-239 (sml_l and lrg_l)
                if(opc == 224)
                {
                    // lrg_l: Large literal
                    opcLen = 2;

                    if(srcPos + opcLen > srcEnd) return -1;

                    literalLen = source[srcPos + 1] + 16;

                    if(srcPos + opcLen + literalLen > srcEnd || dstPos + literalLen > dstEnd) return -1;

                    Array.Copy(source, srcPos + opcLen, destination, dstPos, literalLen);
                    srcPos += opcLen + literalLen;
                    dstPos += literalLen;
                }
                else if(opc >= 225 && opc <= 239)
                {
                    // sml_l: Small literal (1110LLLL)
                    opcLen     = 1;
                    literalLen = opc & 0x0F;

                    if(srcPos + opcLen + literalLen > srcEnd || dstPos + literalLen > dstEnd) return -1;

                    Array.Copy(source, srcPos + opcLen, destination, dstPos, literalLen);
                    srcPos += opcLen + literalLen;
                    dstPos += literalLen;
                }
                else if(opc == 240)
                {
                    // lrg_m: Large match (11110000 MMMMMMMM)
                    opcLen = 2;

                    if(srcPos + opcLen > srcEnd) return -1;

                    matchLen = source[srcPos + 1] + 16;

                    if(dstPos < matchDist || matchDist == 0 || dstPos + matchLen > dstEnd) return -1;

                    CopyMatch(destination, dstPos, matchDist, matchLen);
                    srcPos += opcLen;
                    dstPos += matchLen;
                }
                else // opc >= 241 && opc <= 255
                {
                    // sml_m: Small match (1111MMMM)
                    opcLen   = 1;
                    matchLen = opc & 0x0F;

                    if(srcPos + opcLen > srcEnd) return -1;

                    if(dstPos < matchDist || matchDist == 0 || dstPos + matchLen > dstEnd) return -1;

                    CopyMatch(destination, dstPos, matchDist, matchLen);
                    srcPos += opcLen;
                    dstPos += matchLen;
                }
            }
            else if(opc >= 160 && opc <= 191)
            {
                // med_d: Medium distance (101LLMMM DDDDDDMM DDDDDDDD)
                opcLen = 3;

                if(srcPos + opcLen > srcEnd) return -1;

                literalLen = opc >> 3 & 0x03;
                matchLen   = ((opc & 0x07) << 2 | source[srcPos + 1] & 0x03) + 3;
                matchDist  = source[srcPos + 1] >> 2 | source[srcPos + 2] << 6;

                if(srcPos + opcLen + literalLen > srcEnd) return -1;

                // Copy literal
                if(literalLen > 0)
                {
                    if(dstPos + literalLen > dstEnd) return -1;

                    Array.Copy(source, srcPos + opcLen, destination, dstPos, literalLen);
                    dstPos += literalLen;
                }

                srcPos += opcLen + literalLen;

                // Copy match
                if(dstPos < matchDist || matchDist == 0 || dstPos + matchLen > dstEnd) return -1;

                CopyMatch(destination, dstPos, matchDist, matchLen);
                dstPos += matchLen;
            }
            else if(opc == 6)
            {
                // eos: End of stream
                return dstPos;
            }
            else if((opc & 0x07) == 0x06)
            {
                // pre_d: Previous distance (LLMMM110)
                opcLen     = 1;
                literalLen = opc >> 6 & 0x03;
                matchLen   = (opc >> 3 & 0x07) + 3;

                if(srcPos + opcLen + literalLen > srcEnd) return -1;

                // Copy literal
                if(literalLen > 0)
                {
                    if(dstPos + literalLen > dstEnd) return -1;

                    Array.Copy(source, srcPos + opcLen, destination, dstPos, literalLen);
                    dstPos += literalLen;
                }

                srcPos += opcLen + literalLen;

                // Copy match using previous distance
                if(dstPos < matchDist || matchDist == 0 || dstPos + matchLen > dstEnd) return -1;

                CopyMatch(destination, dstPos, matchDist, matchLen);
                dstPos += matchLen;
            }
            else if((opc & 0x07) == 0x07)
            {
                // lrg_d: Large distance (LLMMM111 DDDDDDDD DDDDDDDD)
                opcLen     = 3;
                literalLen = opc >> 6 & 0x03;
                matchLen   = (opc >> 3 & 0x07) + 3;

                if(srcPos + opcLen + literalLen > srcEnd) return -1;

                matchDist = source[srcPos + 1] | source[srcPos + 2] << 8;

                // Copy literal
                if(literalLen > 0)
                {
                    if(dstPos + literalLen > dstEnd) return -1;

                    Array.Copy(source, srcPos + opcLen, destination, dstPos, literalLen);
                    dstPos += literalLen;
                }

                srcPos += opcLen + literalLen;

                // Copy match
                if(dstPos < matchDist || matchDist == 0 || dstPos + matchLen > dstEnd) return -1;

                CopyMatch(destination, dstPos, matchDist, matchLen);
                dstPos += matchLen;
            }
            else
            {
                // sml_d: Small distance (LLMMMDDD DDDDDDDD)
                opcLen     = 2;
                literalLen = opc >> 6 & 0x03;
                matchLen   = (opc >> 3 & 0x07) + 3;

                if(srcPos + opcLen + literalLen > srcEnd) return -1;

                matchDist = (opc & 0x07) << 8 | source[srcPos + 1];

                // Copy literal
                if(literalLen > 0)
                {
                    if(dstPos + literalLen > dstEnd) return -1;

                    Array.Copy(source, srcPos + opcLen, destination, dstPos, literalLen);
                    dstPos += literalLen;
                }

                srcPos += opcLen + literalLen;

                // Copy match
                if(dstPos < matchDist || matchDist == 0 || dstPos + matchLen > dstEnd) return -1;

                CopyMatch(destination, dstPos, matchDist, matchLen);
                dstPos += matchLen;
            }
        }

        return dstPos;
    }

    /// <summary>Copies a match (LZ77 backreference)</summary>
    /// <param name="buffer">Destination buffer</param>
    /// <param name="dstPos">Current position in destination</param>
    /// <param name="distance">Distance to copy from (0-based offset backwards)</param>
    /// <param name="length">Number of bytes to copy</param>
    private static void CopyMatch(byte[] buffer, int dstPos, int distance, int length)
    {
        int srcPos = dstPos - distance;

        // Handle overlapping copies (distance < length) byte-by-byte
        // This is important for patterns like RLE where distance = 1
        for(var i = 0; i < length; i++) buffer[dstPos + i] = buffer[srcPos + i];
    }
}