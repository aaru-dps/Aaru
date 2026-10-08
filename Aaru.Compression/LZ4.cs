// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LZ4.cs
// Author(s)      : Rebecca Wallander <sakcheen+github@gmail.com>
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
// Copyright © 2011-2026 Rebecca Wallander
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Compression;

// ReSharper disable once InconsistentNaming
/// <summary>Implements the LZ4 compression algorithm</summary>
public partial class LZ4
{
    /// <summary>Set to <c>true</c> if this algorithm is supported, <c>false</c> otherwise.</summary>
    public static bool IsSupported => Native.IsSupported;

    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_lz4_decode_buffer(byte[]    srcBuffer, nuint srcSize, byte[] dstBuffer,
                                                      ref nuint dstSize);

    /// <summary>Decodes a buffer compressed with LZ4</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination)
    {
        if(!Native.IsSupported) return 0;

        nuint dstSize = (nuint)destination.Length;

        return AARU_lz4_decode_buffer(source, (nuint)source.Length, destination, ref dstSize) == 0 ? (int)dstSize : -1;
    }
}