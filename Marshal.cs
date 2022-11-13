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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Helpers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>Provides methods to marshal binary data into C# structs</summary>
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
    public static T ByteArrayToStructureLittleEndian<T>(byte[] bytes) where T : struct
    {
        var ptr = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        var str = (T)(System.Runtime.InteropServices.Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(T)) ??
                      default(T));

        ptr.Free();

        return str;
    }

    /// <summary>Marshal little-endian binary data to a structure</summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <param name="start">Start on the array where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructureLittleEndian<T>(byte[] bytes, int start, int length) where T : struct
    {
        Span<byte> span = bytes;

        return ByteArrayToStructureLittleEndian<T>(span.Slice(start, length).ToArray());
    }

    /// <summary>Marshal big-endian binary data to a structure</summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructureBigEndian<T>(byte[] bytes) where T : struct
    {
        var ptr = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        object str = (T)(System.Runtime.InteropServices.Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(T)) ??
                         default(T));

        ptr.Free();

        return (T)SwapStructureMembersEndian(str);
    }

    /// <summary>Marshal big-endian binary data to a structure</summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <param name="start">Start on the array where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructureBigEndian<T>(byte[] bytes, int start, int length) where T : struct
    {
        Span<byte> span = bytes;

        return ByteArrayToStructureBigEndian<T>(span.Slice(start, length).ToArray());
    }

    /// <summary>Marshal PDP-11 binary data to a structure</summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructurePdpEndian<T>(byte[] bytes) where T : struct
    {
        {
            var ptr = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            object str =
                (T)(System.Runtime.InteropServices.Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(T)) ??
                    default(T));

            ptr.Free();

            return (T)SwapStructureMembersEndianPdp(str);
        }
    }

    /// <summary>Marshal PDP-11 binary data to a structure</summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <param name="start">Start on the array where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructurePdpEndian<T>(byte[] bytes, int start, int length) where T : struct
    {
        Span<byte> span = bytes;

        return ByteArrayToStructurePdpEndian<T>(span.Slice(start, length).ToArray());
    }

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
    ///     Marshal big-endian binary data to a structure. If the structure type contains any non value type, this method
    ///     will crash.
    /// </summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpanToStructureBigEndian<T>(ReadOnlySpan<byte> bytes) where T : struct
    {
        T str = SpanToStructureLittleEndian<T>(bytes);

        return (T)SwapStructureMembersEndian(str);
    }

    /// <summary>
    ///     Marshal big-endian binary data to a structure. If the structure type contains any non value type, this method
    ///     will crash.
    /// </summary>
    /// <param name="bytes">Byte span containing the binary data</param>
    /// <param name="start">Start on the span where the structure begins</param>
    /// <param name="length">Length of the structure in bytes</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpanToStructureBigEndian<T>(ReadOnlySpan<byte> bytes, int start, int length) where T : struct
    {
        T str = SpanToStructureLittleEndian<T>(bytes.Slice(start, length));

        return (T)SwapStructureMembersEndian(str);
    }

    /// <summary>
    ///     Marshal PDP-11 binary data to a structure. If the structure type contains any non value type, this method will
    ///     crash.
    /// </summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SpanToStructurePdpEndian<T>(ReadOnlySpan<byte> bytes) where T : struct
    {
        object str = SpanToStructureLittleEndian<T>(bytes);

        return (T)SwapStructureMembersEndianPdp(str);
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
    public static T SpanToStructurePdpEndian<T>(ReadOnlySpan<byte> bytes, int start, int length) where T : struct
    {
        object str = SpanToStructureLittleEndian<T>(bytes.Slice(start, length));

        return (T)SwapStructureMembersEndianPdp(str);
    }

    /// <summary>
    ///     Marshal a structure depending on the decoration of <see cref="MarshallingPropertiesAttribute" />. If the
    ///     decoration is not present it will marshal as a reference type containing little endian structure.
    /// </summary>
    /// <param name="bytes">Byte array containing the binary data</param>
    /// <typeparam name="T">Type of the structure to marshal</typeparam>
    /// <returns>The binary data marshalled in a structure with the specified type</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The <see cref="MarshallingPropertiesAttribute" /> contains an unsupported
    ///     endian
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MarshalStructure<T>(byte[] bytes) where T : struct
    {
        if(!(typeof(T).GetCustomAttribute(typeof(MarshallingPropertiesAttribute)) is MarshallingPropertiesAttribute
                 properties))
            return ByteArrayToStructureLittleEndian<T>(bytes);

        return properties.Endian switch
               {
                   BitEndian.Little => properties.HasReferences ? ByteArrayToStructureLittleEndian<T>(bytes)
                                           : SpanToStructureLittleEndian<T>(bytes),
                   BitEndian.Big => properties.HasReferences ? ByteArrayToStructureBigEndian<T>(bytes)
                                        : SpanToStructureBigEndian<T>(bytes),
                   BitEndian.Pdp => properties.HasReferences ? ByteArrayToStructurePdpEndian<T>(bytes)
                                        : SpanToStructurePdpEndian<T>(bytes),
                   _ => throw new ArgumentOutOfRangeException()
               };
    }

    /// <summary>Swaps all members of a structure</summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), SuppressMessage("ReSharper", "InconsistentNaming")]
    public static object SwapStructureMembersEndian(object str)
    {
        Type        t         = str.GetType();
        FieldInfo[] fieldInfo = t.GetFields();

        foreach(FieldInfo fi in fieldInfo)
            if(fi.FieldType == typeof(short) ||
               fi.FieldType.IsEnum && fi.FieldType.GetEnumUnderlyingType() == typeof(short))
            {
                var x = (short)(fi.GetValue(str) ?? default(short));
                fi.SetValue(str, (short)((x << 8) | ((x >> 8) & 0xFF)));
            }
            else if(fi.FieldType == typeof(int) ||
                    fi.FieldType.IsEnum && fi.FieldType.GetEnumUnderlyingType() == typeof(int))
            {
                var x = (int)(fi.GetValue(str) ?? default(int));
                x = (int)(((x                   << 8) & 0xFF00FF00) | (((uint)x >> 8) & 0xFF00FF));
                fi.SetValue(str, (int)(((uint)x << 16) | (((uint)x >> 16) & 0xFFFF)));
            }
            else if(fi.FieldType == typeof(long) ||
                    fi.FieldType.IsEnum && fi.FieldType.GetEnumUnderlyingType() == typeof(long))
            {
                var x = (long)(fi.GetValue(str) ?? default(long));
                x = ((x & 0x00000000FFFFFFFF) << 32) | (long)(((ulong)x & 0xFFFFFFFF00000000) >> 32);
                x = ((x & 0x0000FFFF0000FFFF) << 16) | (long)(((ulong)x & 0xFFFF0000FFFF0000) >> 16);
                x = ((x & 0x00FF00FF00FF00FF) << 8)  | (long)(((ulong)x & 0xFF00FF00FF00FF00) >> 8);

                fi.SetValue(str, x);
            }
            else if(fi.FieldType == typeof(ushort) ||
                    fi.FieldType.IsEnum && fi.FieldType.GetEnumUnderlyingType() == typeof(ushort))
            {
                var x = (ushort)(fi.GetValue(str) ?? default(ushort));
                fi.SetValue(str, (ushort)((x << 8) | (x >> 8)));
            }
            else if(fi.FieldType == typeof(uint) ||
                    fi.FieldType.IsEnum && fi.FieldType.GetEnumUnderlyingType() == typeof(uint))
            {
                var x = (uint)(fi.GetValue(str) ?? default(uint));
                x = ((x             << 8) & 0xFF00FF00) | ((x >> 8) & 0xFF00FF);
                fi.SetValue(str, (x << 16) | (x               >> 16));
            }
            else if(fi.FieldType == typeof(ulong) ||
                    fi.FieldType.IsEnum && fi.FieldType.GetEnumUnderlyingType() == typeof(ulong))
            {
                var x = (ulong)(fi.GetValue(str) ?? default(ulong));
                x = ((x & 0x00000000FFFFFFFF) << 32) | ((x & 0xFFFFFFFF00000000) >> 32);
                x = ((x & 0x0000FFFF0000FFFF) << 16) | ((x & 0xFFFF0000FFFF0000) >> 16);
                x = ((x & 0x00FF00FF00FF00FF) << 8)  | ((x & 0xFF00FF00FF00FF00) >> 8);
                fi.SetValue(str, x);
            }
            else if(fi.FieldType == typeof(float))
            {
                var    flt   = (float)(fi.GetValue(str) ?? default(float));
                byte[] flt_b = BitConverter.GetBytes(flt);

                fi.SetValue(str, BitConverter.ToSingle(new[]
                {
                    flt_b[3], flt_b[2], flt_b[1], flt_b[0]
                }, 0));
            }
            else if(fi.FieldType == typeof(double))
            {
                var    dbl   = (double)(fi.GetValue(str) ?? default(double));
                byte[] dbl_b = BitConverter.GetBytes(dbl);

                fi.SetValue(str, BitConverter.ToDouble(new[]
                {
                    dbl_b[7], dbl_b[6], dbl_b[5], dbl_b[4], dbl_b[3], dbl_b[2], dbl_b[1], dbl_b[0]
                }, 0));
            }
            else if(fi.FieldType == typeof(byte) ||
                    fi.FieldType == typeof(sbyte))
            {
                // Do nothing, can't byteswap them!
            }
            else if(fi.FieldType == typeof(Guid))
            {
                // TODO: Swap GUID
            }

            // TODO: Swap arrays
            else if(fi.FieldType.IsValueType &&
                    !fi.FieldType.IsEnum     &&
                    !fi.FieldType.IsArray)
            {
                object obj  = fi.GetValue(str);
                object strc = SwapStructureMembersEndian(obj);
                fi.SetValue(str, strc);
            }

        return str;
    }

    /// <summary>Swaps all fields in an structure considering them to follow PDP endian conventions</summary>
    /// <param name="str">Source structure</param>
    /// <returns>Resulting structure</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object SwapStructureMembersEndianPdp(object str)
    {
        Type        t         = str.GetType();
        FieldInfo[] fieldInfo = t.GetFields();

        foreach(FieldInfo fi in fieldInfo)
            if(fi.FieldType == typeof(short)  ||
               fi.FieldType == typeof(long)   ||
               fi.FieldType == typeof(ushort) ||
               fi.FieldType == typeof(ulong)  ||
               fi.FieldType == typeof(float)  ||
               fi.FieldType == typeof(double) ||
               fi.FieldType == typeof(byte)   ||
               fi.FieldType == typeof(sbyte)  ||
               fi.FieldType == typeof(Guid))
            {
                // Do nothing
            }
            else if(fi.FieldType == typeof(int) ||
                    fi.FieldType.IsEnum && fi.FieldType.GetEnumUnderlyingType() == typeof(int))
            {
                var x = (int)(fi.GetValue(str) ?? default(int));
                fi.SetValue(str, ((x & 0xffffu) << 16) | ((x & 0xffff0000u) >> 16));
            }
            else if(fi.FieldType == typeof(uint) ||
                    fi.FieldType.IsEnum && fi.FieldType.GetEnumUnderlyingType() == typeof(uint))
            {
                var x = (uint)(fi.GetValue(str) ?? default(uint));
                fi.SetValue(str, ((x & 0xffffu) << 16) | ((x & 0xffff0000u) >> 16));
            }

            // TODO: Swap arrays
            else if(fi.FieldType.IsValueType &&
                    !fi.FieldType.IsEnum     &&
                    !fi.FieldType.IsArray)
            {
                object obj  = fi.GetValue(str);
                object strc = SwapStructureMembersEndianPdp(obj);
                fi.SetValue(str, strc);
            }

        return str;
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
    public static byte[] StructureToByteArrayBigEndian<T>(T str) where T : struct =>
        StructureToByteArrayLittleEndian((T)SwapStructureMembersEndian(str));

    /// <summary>Converts a hexadecimal string into a byte array</summary>
    /// <param name="hex">Hexadecimal string</param>
    /// <param name="outBuf">Resulting byte array</param>
    /// <returns>Number of output bytes processed</returns>
    public static int ConvertFromHexAscii(string hex, out byte[] outBuf)
    {
        outBuf = null;

        if(hex is null or "")
            return -1;

        var off = 0;

        if(hex[0] == '0' &&
           (hex[1] == 'x' || hex[1] == 'X'))
            off = 2;

        outBuf = new byte[(hex.Length - off) / 2];
        var count = 0;

        for(int i = off; i < hex.Length; i += 2)
        {
            char c = hex[i];

            if(c < '0'            ||
               c > '9' && c < 'A' ||
               c > 'F' && c < 'a' ||
               c > 'f')
                break;

            c -= c >= 'a' && c <= 'f'
                     ? '\u0057'
                     : c >= 'A' && c <= 'F'
                         ? '\u0037'
                         : '\u0030';

            outBuf[(i - off) / 2] = (byte)(c << 4);

            c = hex[i + 1];

            if(c < '0'            ||
               c > '9' && c < 'A' ||
               c > 'F' && c < 'a' ||
               c > 'f')
                break;

            c -= c >= 'a' && c <= 'f'
                     ? '\u0057'
                     : c >= 'A' && c <= 'F'
                         ? '\u0037'
                         : '\u0030';

            outBuf[(i - off) / 2] += (byte)c;

            count++;
        }

        return count;
    }
}