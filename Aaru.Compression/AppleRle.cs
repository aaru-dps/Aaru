// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleRle.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Compression algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decompress Apple variant of RLE.
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
// Copyright © 2011-2023 Natalia Portillo
// Copyright © 2018-2019 David Ryskalczyk
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Compression;

/// <summary>Implements the Apple version of RLE</summary>
public static class AppleRle
{
    const uint DART_CHUNK = 20960;

    /// <summary>Set to <c>true</c> if this algorithm is supported, <c>false</c> otherwise.</summary>
    public static bool IsSupported => true;

    [DllImport("libAaru.Compression.Native", SetLastError = true)]
    static extern int AARU_apple_rle_decode_buffer(byte[] dstBuffer, int dstSize, byte[] srcBuffer, int srcSize);

    /// <summary>Decodes a buffer compressed with Apple RLE</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination)
    {
        if(Native.IsSupported)
            return AARU_apple_rle_decode_buffer(destination, destination.Length, source, source.Length);

        var  count         = 0;
        var  nextA         = true; // true if A, false if B
        byte repeatedByteA = 0, repeatedByteB = 0;
        var  repeatMode    = false; // true if we're repeating, false if we're just copying
        int  inPosition    = 0, outPosition = 0;

        while(inPosition  <= source.Length &&
              outPosition <= destination.Length)
        {
            switch(repeatMode)
            {
                case true when count > 0:
                {
                    count--;

                    if(nextA)
                    {
                        nextA = false;

                        destination[outPosition++] = repeatedByteA;

                        continue;
                    }

                    nextA = true;

                    destination[outPosition++] = repeatedByteB;

                    continue;
                }
                case false when count > 0:
                    count--;

                    destination[outPosition++] = source[inPosition++];

                    continue;
            }

            if(inPosition == source.Length)
                break;

            while(true)
            {
                byte b1 = source[inPosition++];
                byte b2 = source[inPosition++];
                var  s  = (short)(b1 << 8 | b2);

                if(s == 0          ||
                   s >= DART_CHUNK ||
                   s <= -DART_CHUNK)
                    continue;

                if(s < 0)
                {
                    repeatMode    = true;
                    repeatedByteA = source[inPosition++];
                    repeatedByteB = source[inPosition++];
                    count         = -s * 2 - 1;
                    nextA         = false;

                    destination[outPosition++] = repeatedByteA;

                    break;
                }

                repeatMode = false;
                count      = s * 2 - 1;

                destination[outPosition++] = source[inPosition++];

                break;
            }
        }

        return outPosition;
    }
}