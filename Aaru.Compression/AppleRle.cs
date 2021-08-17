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
// Copyright © 2011-2021 Natalia Portillo
// Copyright © 2018-2019 David Ryskalczyk
// ****************************************************************************/

using System.IO;

namespace Aaru.Compression
{
    /// <summary>
    /// Implements the Apple version of RLE
    /// </summary>
    public class AppleRle
    {
        const uint DART_CHUNK = 20960;

        readonly Stream _inStream;
        int             _count;
        bool            _nextA; // true if A, false if B
        byte            _repeatedByteA, _repeatedByteB;
        bool            _repeatMode; // true if we're repeating, false if we're just copying

        /// <summary>
        /// Initializes a decompressor for the specified stream
        /// </summary>
        /// <param name="stream">Stream containing the compressed data</param>
        public AppleRle(Stream stream)
        {
            _inStream = stream;
            Reset();
        }

        void Reset()
        {
            _repeatedByteA = _repeatedByteB = 0;
            _count         = 0;
            _nextA         = true;
            _repeatMode    = false;
        }

        /// <summary>
        /// Decompresses a byte
        /// </summary>
        /// <returns>Decompressed byte</returns>
        public int ProduceByte()
        {
            if(_repeatMode && _count > 0)
            {
                _count--;

                if(_nextA)
                {
                    _nextA = false;

                    return _repeatedByteA;
                }

                _nextA = true;

                return _repeatedByteB;
            }

            if(!_repeatMode &&
               _count > 0)
            {
                _count--;

                return _inStream.ReadByte();
            }

            if(_inStream.Position == _inStream.Length)
                return -1;

            while(true)
            {
                byte  b1 = (byte)_inStream.ReadByte();
                byte  b2 = (byte)_inStream.ReadByte();
                short s  = (short)((b1 << 8) | b2);

                if(s == 0          ||
                   s >= DART_CHUNK ||
                   s <= -DART_CHUNK)
                    continue;

                if(s < 0)
                {
                    _repeatMode    = true;
                    _repeatedByteA = (byte)_inStream.ReadByte();
                    _repeatedByteB = (byte)_inStream.ReadByte();
                    _count         = (-s * 2) - 1;
                    _nextA         = false;

                    return _repeatedByteA;
                }

                if(s <= 0)
                    continue;

                _repeatMode = false;
                _count      = (s * 2) - 1;

                return _inStream.ReadByte();
            }
        }
    }
}