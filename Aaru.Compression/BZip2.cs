// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BZip2.cs
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

using System.IO;
using System.Runtime.InteropServices;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;

namespace Aaru.Compression;

/// <summary>Implements the BZIP2 compression algorithm</summary>
public partial class BZip2
{
    /// <summary>Set to <c>true</c> if this algorithm is supported, <c>false</c> otherwise.</summary>
    public static bool IsSupported => true;

    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_bzip2_decode_buffer(byte[]    srcBuffer, nuint srcSize, byte[] dstBuffer,
                                                        ref nuint dstSize);

    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_bzip2_encode_buffer(byte[]    srcBuffer, nuint srcSize, byte[] dstBuffer,
                                                        ref nuint dstSize,   int   blockSize100K);

    /// <summary>Decodes a buffer compressed with BZIP2</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes, or -1 on error</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination)
    {
        var destinationSize = (nuint)destination.Length;

        if(Native.IsSupported)
        {
            int res = AARU_bzip2_decode_buffer(source, (nuint)source.Length, destination, ref destinationSize);

            return res == 0 ? (int)destinationSize : -1;
        }

        using var cmpMs     = new MemoryStream(source);
        using var decStream = BZip2Stream.Create(cmpMs, CompressionMode.Decompress, false);

        return decStream.Read(destination, 0, destination.Length);
    }

    /// <summary>Compresses a buffer using BZIP2</summary>
    /// <param name="source">Data to compress</param>
    /// <param name="destination">Buffer to store the compressed data</param>
    /// <param name="blockSize100K">Block size in 100KiB units</param>
    /// <returns>The size of the compressed data, or -1 on error</returns>
    public static int EncodeBuffer(byte[] source, byte[] destination, int blockSize100K)
    {
        var destinationSize = (nuint)destination.Length;

        if(Native.IsSupported)
        {
            int res = AARU_bzip2_encode_buffer(source,
                                               (nuint)source.Length,
                                               destination,
                                               ref destinationSize,
                                               blockSize100K);

            return res == 0 ? (int)destinationSize : -1;
        }

        using var cmpMs     = new MemoryStream(source);
        using var encStream = BZip2Stream.Create(new MemoryStream(destination), CompressionMode.Compress, false);
        encStream.Write(source, 0, source.Length);

        return source.Length;
    }
}