// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LZFSE.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Compression algorithms.
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

using System.Runtime.InteropServices;

namespace Aaru.Compression;

// ReSharper disable once InconsistentNaming
/// <summary>Implements the LZFSE compression algorithm</summary>
public partial class LZFSE
{
    /// <summary>Set to <c>true</c> if this algorithm is supported, <c>false</c> otherwise.</summary>
    public static bool IsSupported => Native.IsSupported;

    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_lzfse_decode_buffer(byte[]    srcBuffer, nuint  srcSize, byte[] dstBuffer,
                                                        ref nuint dstSize,   byte[] scratchBuffer);

    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_lzfse_encode_buffer(byte[]    srcBuffer, nuint  srcSize, byte[] dstBuffer,
                                                        ref nuint dstSize,   byte[] scratchBuffer);

    /// <summary>Decodes a buffer compressed with LZFSE</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes, or -1 on error</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination)
    {
        if(!Native.IsSupported) return 0;

        nuint dstSize = (nuint)destination.Length;

        return AARU_lzfse_decode_buffer(source, (nuint)source.Length, destination, ref dstSize, null) == 0
                   ? (int)dstSize
                   : -1;
    }

    /// <summary>Compresses a buffer using LZFSE</summary>
    /// <param name="source">Data to compress</param>
    /// <param name="destination">Buffer to store the compressed data</param>
    /// <returns>The size of the compressed data, or -1 on error</returns>
    public static int EncodeBuffer(byte[] source, byte[] destination)
    {
        if(!Native.IsSupported) return 0;

        nuint dstSize = (nuint)destination.Length;

        return AARU_lzfse_encode_buffer(source, (nuint)source.Length, destination, ref dstSize, null) == 0
                   ? (int)dstSize
                   : -1;
    }
}