// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Aaru.Helpers;

/// <summary>Helper operations to work with swapping endians</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Swapping
{
    /// <summary>Gets the PDP endian equivalent of the given little endian unsigned integer</summary>
    /// <param name="x">Little endian unsigned integer</param>
    /// <returns>PDP unsigned integer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PDPFromLittleEndian(uint x) => (x & 0xffff) << 16 | (x & 0xffff0000) >> 16;

    /// <summary>Gets the PDP endian equivalent of the given big endian unsigned integer</summary>
    /// <param name="x">Big endian unsigned integer</param>
    /// <returns>PDP unsigned integer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PDPFromBigEndian(uint x) => (x & 0xff00ff) << 8 | (x & 0xff00ff00) >> 8;

    /// <summary>Swaps the endian of the specified unsigned short integer</summary>
    /// <param name="x">Unsigned short integer</param>
    /// <returns>Swapped unsigned short integer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort Swap(ushort x) => (ushort)(x << 8 | x >> 8);

    /// <summary>Swaps the endian of the specified signed short integer</summary>
    /// <param name="x">Signed short integer</param>
    /// <returns>Swapped signed short integer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Swap(short x) => (short)(x << 8 | x >> 8 & 0xFF);

    /// <summary>Swaps the endian of the specified unsigned integer</summary>
    /// <param name="x">Unsigned integer</param>
    /// <returns>Swapped unsigned integer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Swap(uint x)
    {
        x = x << 8 & 0xFF00FF00 | x >> 8 & 0xFF00FF;

        return x << 16 | x >> 16;
    }

    /// <summary>Swaps the endian of the specified signed integer</summary>
    /// <param name="x">Signed integer</param>
    /// <returns>Swapped signed integer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Swap(int x)
    {
        x = (int)(x << 8 & 0xFF00FF00 | (uint)x >> 8 & 0xFF00FF);

        return (int)((uint)x << 16 | (uint)x >> 16 & 0xFFFF);
    }

    /// <summary>Swaps the endian of the specified unsigned long integer</summary>
    /// <param name="x">Unsigned long integer</param>
    /// <returns>Swapped unsigned long integer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Swap(ulong x)
    {
        x = (x & 0x00000000FFFFFFFF) << 32 | (x & 0xFFFFFFFF00000000) >> 32;
        x = (x & 0x0000FFFF0000FFFF) << 16 | (x & 0xFFFF0000FFFF0000) >> 16;
        x = (x & 0x00FF00FF00FF00FF) << 8  | (x & 0xFF00FF00FF00FF00) >> 8;

        return x;
    }

    /// <summary>Swaps the endian of the specified signed long integer</summary>
    /// <param name="x">Signed long integer</param>
    /// <returns>Swapped signed long integer</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Swap(long x)
    {
        x = (x & 0x00000000FFFFFFFF) << 32 | (long)(((ulong)x & 0xFFFFFFFF00000000) >> 32);
        x = (x & 0x0000FFFF0000FFFF) << 16 | (long)(((ulong)x & 0xFFFF0000FFFF0000) >> 16);
        x = (x & 0x00FF00FF00FF00FF) << 8  | (long)(((ulong)x & 0xFF00FF00FF00FF00) >> 8);

        return x;
    }
}