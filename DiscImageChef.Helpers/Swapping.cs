// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Swapping.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Byte-swapping methods.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.Linq;

namespace DiscImageChef
{
    public static class Swapping
    {
        public static byte[] SwapTenBytes(byte[] source)
        {
            byte[] destination = new byte[8];

            destination[0] = source[9];
            destination[1] = source[8];
            destination[2] = source[7];
            destination[3] = source[6];
            destination[4] = source[5];
            destination[5] = source[4];
            destination[6] = source[3];
            destination[7] = source[2];
            destination[8] = source[1];
            destination[9] = source[0];

            return destination;
        }

        public static byte[] SwapEightBytes(byte[] source)
        {
            byte[] destination = new byte[8];

            destination[0] = source[7];
            destination[1] = source[6];
            destination[2] = source[5];
            destination[3] = source[4];
            destination[4] = source[3];
            destination[5] = source[2];
            destination[6] = source[1];
            destination[7] = source[0];

            return destination;
        }

        public static byte[] SwapFourBytes(byte[] source)
        {
            byte[] destination = new byte[4];

            destination[0] = source[3];
            destination[1] = source[2];
            destination[2] = source[1];
            destination[3] = source[0];

            return destination;
        }

        public static byte[] SwapTwoBytes(byte[] source)
        {
            byte[] destination = new byte[2];

            destination[0] = source[1];
            destination[1] = source[0];

            return destination;
        }

        public static uint PDPFromLittleEndian(uint x)
        {
            return ((x & 0xffff) << 16) | ((x & 0xffff0000) >> 16);
        }

        public static uint PDPFromBigEndian(uint x)
        {
            return ((x & 0xff00ff) << 8) | ((x & 0xff00ff00) >> 8);
        }

        public static ulong Swap(ulong x)
        {
            x = (x & 0x00000000FFFFFFFF) << 32 | (x & 0xFFFFFFFF00000000) >> 32;
            x = (x & 0x0000FFFF0000FFFF) << 16 | (x & 0xFFFF0000FFFF0000) >> 16;
            x = (x & 0x00FF00FF00FF00FF) << 8 | (x & 0xFF00FF00FF00FF00) >> 8;
            return x;
        }

        public static long Swap(long x)
        {
            return BitConverter.ToInt64(BitConverter.GetBytes(x).Reverse().ToArray(), 0);
        }

        public static ushort Swap(ushort x)
        {
            return (ushort)((x << 8) | (x >> 8));
        }

        public static short Swap(short x)
        {
            return (short)((x << 8) | ((x >> 8) & 0xFF));
        }

        public static uint Swap(uint x)
        {
            x = ((x << 8) & 0xFF00FF00) | ((x >> 8) & 0xFF00FF);
            return (x << 16) | (x >> 16);
        }

        public static int Swap(int x)
        {
            x = (int)(((x << 8) & 0xFF00FF00) | ((x >> 8) & 0xFF00FF));
            return (x << 16) | ((x >> 16) & 0xFFFF);
        }
    }
}
