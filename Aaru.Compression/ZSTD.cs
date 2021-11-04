// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ZSTD.cs
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Compression;

public class ZSTD
{
    [DllImport("libAaru.Compression.Native", SetLastError = true)]
    static extern nuint AARU_zstd_decode_buffer(byte[] dst_buffer, nuint dst_size, byte[] src_buffer, nuint src_size);

    [DllImport("libAaru.Compression.Native", SetLastError = true)]
    static extern nuint AARU_zstd_encode_buffer(byte[] dst_buffer, nuint dst_size, byte[] src_buffer, nuint src_size,
                                                int compressionLevel);

    /// <summary>Decodes a buffer compressed with ZSTD</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination) =>
        (int)(Native.IsSupported
                  ? AARU_zstd_decode_buffer(destination, (nuint)destination.Length, source, (nuint)source.Length) : 0);

    /// <summary>Compresses a buffer using ZSTD</summary>
    /// <param name="source">Data to compress</param>
    /// <param name="destination">Buffer to store the compressed data</param>
    /// <returns></returns>
    public static int EncodeBuffer(byte[] source, byte[] destination, int compressionLevel) =>
        (int)(Native.IsSupported
                  ? AARU_zstd_encode_buffer(destination, (nuint)destination.Length, source, (nuint)source.Length,
                                            compressionLevel) : 0);
}