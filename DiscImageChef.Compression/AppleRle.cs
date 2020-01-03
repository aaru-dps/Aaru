// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// Copyright © 2018-2019 David Ryskalczyk
// ****************************************************************************/

using System.IO;

namespace DiscImageChef.Compression
{
    public class AppleRle
    {
        const uint DART_CHUNK = 20960;

        readonly Stream inStream;
        int             count;
        bool            nextA; // true if A, false if B

        byte repeatedbyteA, repeatedbyteB;
        bool repeatMode; // true if we're repeating, false if we're just copying

        public AppleRle(Stream stream)
        {
            inStream = stream;
            Reset();
        }

        void Reset()
        {
            repeatedbyteA = repeatedbyteB = 0;
            count         = 0;
            nextA         = true;
            repeatMode    = false;
        }

        public int ProduceByte()
        {
            if(repeatMode && count > 0)
            {
                count--;
                if(nextA)
                {
                    nextA = false;
                    return repeatedbyteA;
                }

                nextA = true;
                return repeatedbyteB;
            }

            if(!repeatMode && count > 0)
            {
                count--;
                return inStream.ReadByte();
            }

            if(inStream.Position == inStream.Length) return -1;

            while(true)
            {
                byte  b1 = (byte)inStream.ReadByte();
                byte  b2 = (byte)inStream.ReadByte();
                short s  = (short)((b1 << 8) | b2);

                if(s == 0 || s >= DART_CHUNK || s <= -DART_CHUNK) continue;

                if(s < 0)
                {
                    repeatMode    = true;
                    repeatedbyteA = (byte)inStream.ReadByte();
                    repeatedbyteB = (byte)inStream.ReadByte();
                    count         = -s * 2 - 1;
                    nextA         = false;
                    return repeatedbyteA;
                }

                if(s <= 0) continue;

                repeatMode = false;
                count      = s * 2 - 1;
                return inStream.ReadByte();
            }
        }
    }
}