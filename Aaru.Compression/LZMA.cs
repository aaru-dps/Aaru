// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LZMA.cs
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Runtime.InteropServices;
using Aaru.Helpers;
using SharpCompress.Compressors.LZMA;

namespace Aaru.Compression;

/// <summary>Implements the LZMA compression algorithm</summary>
public class LZMA
{
    /// <summary>Set to <c>true</c> if this algorithm is supported, <c>false</c> otherwise.</summary>
    public static bool IsSupported => true;

    [DllImport("libAaru.Compression.Native", SetLastError = true)]
    static extern int AARU_lzma_decode_buffer(byte[] dstBuffer, ref nuint dstSize, byte[] srcBuffer, ref nuint srcSize,
                                              byte[] props, nuint propsSize);

    [DllImport("libAaru.Compression.Native", SetLastError = true)]
    static extern int AARU_lzma_encode_buffer(byte[] dstBuffer, ref nuint dstSize, byte[] srcBuffer, nuint srcSize,
                                              byte[] outProps, ref nuint outPropsSize, int level, uint dictSize, int lc,
                                              int lp, int pb, int fb, int numThreads);

    /// <summary>Decodes a buffer compressed with LZMA</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <param name="properties">LZMA stream properties</param>
    /// <returns>The number of decoded bytes</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination, byte[] properties)
    {
        if(Native.IsSupported)
        {
            nuint srcSize = (nuint)source.Length;
            nuint dstSize = (nuint)destination.Length;

            AARU_lzma_decode_buffer(destination, ref dstSize, source, ref srcSize, properties,
                                    (nuint)properties.Length);

            return (int)dstSize;
        }

        using var cmpMs     = new MemoryStream(source);
        using var lzmaBlock = new LzmaStream(properties, cmpMs);
        lzmaBlock.EnsureRead(destination, 0, destination.Length);

        return destination.Length;
    }

    /// <summary>Compresses a buffer using BZIP2</summary>
    /// <param name="source">Data to compress</param>
    /// <param name="destination">Buffer to store the compressed data</param>
    /// <param name="properties">LZMA stream properties</param>
    /// <param name="level">Compression level</param>
    /// <param name="dictSize">Dictionary size</param>
    /// <param name="lc">Literal context bits</param>
    /// <param name="lp">Literal position bits</param>
    /// <param name="pb">Position bits</param>
    /// <param name="fb">Forward bits</param>
    /// <returns>How many bytes have been written to the destination buffer</returns>
    public static int EncodeBuffer(byte[] source, byte[] destination, out byte[] properties, int level, uint dictSize,
                                   int lc, int lp, int pb, int fb)
    {
        if(Native.IsSupported)
        {
            properties = new byte[5];
            nuint dstSize   = (nuint)destination.Length;
            nuint propsSize = (nuint)properties.Length;
            nuint srcSize   = (nuint)source.Length;

            AARU_lzma_encode_buffer(destination, ref dstSize, source, srcSize, properties, ref propsSize, level,
                                    dictSize, lc, lp, pb, fb, 0);

            return (int)dstSize;
        }

        var lzmaEncoderProperties = new LzmaEncoderProperties(true, (int)dictSize, fb);

        using var lzmaStream = new LzmaStream(lzmaEncoderProperties, false, new MemoryStream(destination));

        lzmaStream.Write(source, 0, source.Length);
        properties = new byte[lzmaStream.Properties.Length];
        lzmaStream.Properties.CopyTo(properties, 0);

        return destination.Length;
    }
}