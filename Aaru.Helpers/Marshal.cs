// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Marshal.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides marshalling for binary data.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aaru.Helpers;

/// <summary>Provides methods to marshal binary data into C# structs</summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
[SuppressMessage("ReSharper", "UnusedMember.Global",       Justification = "Public API")]
public static class Marshal
{
    /// <summary>Returns the size of an unmanaged type in bytes.</summary>
    /// <typeparam name="T">The type whose size is to be returned.</typeparam>
    /// <returns>The size, in bytes, of the type that is specified by the <see cref="T" /> generic type parameter.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SizeOf<T>() => System.Runtime.InteropServices.Marshal.SizeOf<T>();

    /// <summary>Marshal little-endian binary data to a structure</summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructureLittleEndian<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                    DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        T>(byte[] bytes) where T : struct
    {
        var ptr = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try
        {
            return System.Runtime.InteropServices.Marshal.PtrToStructure<T>(ptr.AddrOfPinnedObject());
        }
        finally
        {
            ptr.Free();
        }
    }

    /// <summary>Marshal little-endian binary data to a structure</summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <param name="start">Start on the array where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructureLittleEndian<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                    DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        T>(byte[] bytes, int start, int length) where T : struct
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)start + (uint)length, (uint)bytes.Length);

        var ptr = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try
        {
            return System.Runtime.InteropServices.Marshal.PtrToStructure<T>(ptr.AddrOfPinnedObject() + start);
        }
        finally
        {
            ptr.Free();
        }
    }

    /// <summary>
    ///     Marshal big-endian binary data to a structure using compile-time generated swap method.
    /// </summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal (must be marked with [SwapEndian] attribute)</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructureBigEndian<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                    DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        T>(byte[] bytes) where T : struct, ISwapEndian<T>
    {
        T str = ByteArrayToStructureLittleEndian<T>(bytes);

        return str.SwapEndian();
    }

    /// <summary>
    ///     Marshal big-endian binary data to a structure using compile-time generated swap method.
    /// </summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <param name="start">Start on the array where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal (must be marked with [SwapEndian] attribute)</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructureBigEndian<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                    DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        T>(byte[] bytes, int start, int length) where T : struct, ISwapEndian<T> =>
        ByteArrayToStructureLittleEndian<T>(bytes, start, length).SwapEndian();

    /// <summary>Marshal PDP-11 binary data to a structure</summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructurePdpEndian<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                    DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        T>(byte[] bytes) where T : struct, ISwapPdpEndian<T>
    {
        T str = ByteArrayToStructureLittleEndian<T>(bytes);

        return str.SwapPdpEndian();
    }


    /// <summary>Marshal PDP-11 binary data to a structure</summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <param name="start">Start on the array where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructurePdpEndian<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                    DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        T>(byte[] bytes, int start, int length) where T : struct, ISwapPdpEndian<T> =>
        ByteArrayToStructureLittleEndian<T>(bytes, start, length).SwapPdpEndian();

    /// <summary>
    ///     Marshal little-endian binary data to a structure. If the structure type contains any non value type, this
    ///     method will crash.
    /// </summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpanToStructureLittleEndian<T>(ReadOnlySpan<byte> bytes) where T : struct =>
        MemoryMarshal.Read<T>(bytes);

    /// <summary>
    ///     Marshal little-endian binary data to a structure. If the structure type contains any non value type, this
    ///     method will crash.
    /// </summary>
    /// <param name="bytes">Byte span containing the binary data</param>
    /// <param name="start">Start on the span where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpanToStructureLittleEndian<T>(ReadOnlySpan<byte> bytes, int start, int length) where T : struct =>
        MemoryMarshal.Read<T>(bytes.Slice(start, length));

    /// <summary>
    ///     Marshal big-endian binary data to a structure using compile-time generated swap method.
    ///     If the structure type contains any non value type, this method will crash.
    /// </summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal (must be marked with [SwapEndian] attribute)</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpanToStructureBigEndian<T>(ReadOnlySpan<byte> bytes) where T : struct, ISwapEndian<T>
    {
        T str = SpanToStructureLittleEndian<T>(bytes);

        return str.SwapEndian();
    }

    /// <summary>
    ///     Marshal big-endian binary data to a structure using compile-time generated swap method.
    ///     If the structure type contains any non value type, this method will crash.
    /// </summary>
    /// <param name="bytes">Byte span containing the binary data</param>
    /// <param name="start">Start on the span where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal (must be marked with [SwapEndian] attribute)</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpanToStructureBigEndian<T>(ReadOnlySpan<byte> bytes, int start, int length)
        where T : struct, ISwapEndian<T>
    {
        T str = SpanToStructureLittleEndian<T>(bytes.Slice(start, length));

        return str.SwapEndian();
    }

    /// <summary>
    ///     Marshal PDP-11 binary data to a structure. If the structure type contains any non value type, this method will
    ///     crash.
    /// </summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpanToStructurePdpEndian<T>(ReadOnlySpan<byte> bytes) where T : struct, ISwapPdpEndian<T>
    {
        T str = SpanToStructureLittleEndian<T>(bytes);

        return str.SwapPdpEndian();
    }

    /// <summary>
    ///     Marshal PDP-11 binary data to a structure. If the structure type contains any non value type, this method will
    ///     crash.
    /// </summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <param name="start">Start on the span where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpanToStructurePdpEndian<T>(ReadOnlySpan<byte> bytes, int start, int length)
        where T : struct, ISwapPdpEndian<T>
    {
        T str = SpanToStructureLittleEndian<T>(bytes.Slice(start, length));

        return str.SwapPdpEndian();
    }

    /// <summary>Marshal a structure to little-endian binary data</summary>
    /// <param name="str">The structure you want to marshal to binary</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The byte array representing the given structure</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] StructureToByteArrayLittleEndian<T>(T str) where T : struct
    {
        var buf = new byte[SizeOf<T>()];
        var ptr = GCHandle.Alloc(buf, GCHandleType.Pinned);
        System.Runtime.InteropServices.Marshal.StructureToPtr(str, ptr.AddrOfPinnedObject(), false);
        ptr.Free();

        return buf;
    }

    /// <summary>Marshal a structure to little-endian binary data</summary>
    /// <param name="str">The structure you want to marshal to binary</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The byte array representing the given structure</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] StructureToByteArrayBigEndian<T>(T str) where T : struct, ISwapEndian<T> =>
        StructureToByteArrayLittleEndian(str.SwapEndian());

    /// <summary>Converts a hexadecimal string into a byte array</summary>
    /// <param name="hex">Hexadecimal string</param>
    /// <param name="outBuf">Resulting byte array</param>
    /// <returns>Number of output bytes processed</returns>
    public static int ConvertFromHexAscii(string hex, out byte[] outBuf)
    {
        outBuf = null;

        if(hex is null or "") return -1;

        var off = 0;

        if(hex.Length >= 2 && hex[0] == '0' && (hex[1] == 'x' || hex[1] == 'X')) off = 2;

        outBuf = new byte[(hex.Length - off) / 2];
        var count = 0;

        for(int i = off; i + 1 < hex.Length; i += 2)
        {
            char c = hex[i];

            if(c is < '0' or > '9' and < 'A' or > 'F' and < 'a' or > 'f') break;

            c -= c switch
                 {
                     >= 'a' and <= 'f' => '\u0057',
                     >= 'A' and <= 'F' => '\u0037',
                     _                 => '\u0030'
                 };

            outBuf[(i - off) / 2] = (byte)(c << 4);

            c = hex[i + 1];

            if(c is < '0' or > '9' and < 'A' or > 'F' and < 'a' or > 'f') break;

            c -= c switch
                 {
                     >= 'a' and <= 'f' => '\u0057',
                     >= 'A' and <= 'F' => '\u0037',
                     _                 => '\u0030'
                 };

            outBuf[(i - off) / 2] += (byte)c;

            count++;
        }

        return count;
    }
}