// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DiscImageChef.Helpers
{
    /// <summary>Provides methods to marshal binary data into C# structs</summary>
    public static class Marshal
    {
        /// <summary>
        ///     Returns the size of an unmanaged type in bytes.
        /// </summary>
        /// <typeparam name="T">The type whose size is to be returned.</typeparam>
        /// <returns>The size, in bytes, of the type that is specified by the <see cref="T" /> generic type parameter.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf<T>();
        }

        /// <summary>
        ///     Marshal little-endian binary data to a structure
        /// </summary>
        /// <param name="bytes">Byte array containing the binary data</param>
        /// <typeparam name="T">Type of the structure to marshal</typeparam>
        /// <returns>The binary data marshalled in a structure with the specified type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ByteArrayToStructureLittleEndian<T>(byte[] bytes) where T : struct
        {
            var ptr = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var str =
                (T) System.Runtime.InteropServices.Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(T));
            ptr.Free();
            return str;
        }

        /// <summary>
        ///     Marshal little-endian binary data to a structure
        /// </summary>
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

        /// <summary>
        ///     Marshal big-endian binary data to a structure
        /// </summary>
        /// <param name="bytes">Byte array containing the binary data</param>
        /// <typeparam name="T">Type of the structure to marshal</typeparam>
        /// <returns>The binary data marshalled in a structure with the specified type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ByteArrayToStructureBigEndian<T>(byte[] bytes) where T : struct
        {
            var ptr = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            object str =
                (T) System.Runtime.InteropServices.Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(T));
            ptr.Free();
            return (T) SwapStructureMembersEndian(str);
        }

        /// <summary>
        ///     Marshal big-endian binary data to a structure
        /// </summary>
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

        /// <summary>
        ///     Marshal PDP-11 binary data to a structure
        /// </summary>
        /// <param name="bytes">Byte array containing the binary data</param>
        /// <typeparam name="T">Type of the structure to marshal</typeparam>
        /// <returns>The binary data marshalled in a structure with the specified type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ByteArrayToStructurePdpEndian<T>(byte[] bytes) where T : struct
        {
            {
                var ptr = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                object str =
                    (T) System.Runtime.InteropServices.Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(T));
                ptr.Free();
                return (T) SwapStructureMembersEndianPdp(str);
            }
        }

        /// <summary>
        ///     Marshal PDP-11 binary data to a structure
        /// </summary>
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
        ///     Marshal little-endian binary data to a structure. If the structure type contains any non value type, this method
        ///     will crash.
        /// </summary>
        /// <param name="bytes">Byte array containing the binary data</param>
        /// <typeparam name="T">Type of the structure to marshal</typeparam>
        /// <returns>The binary data marshalled in a structure with the specified type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SpanToStructureLittleEndian<T>(ReadOnlySpan<byte> bytes) where T : struct
        {
            return MemoryMarshal.Read<T>(bytes);
        }

        /// <summary>
        ///     Marshal little-endian binary data to a structure. If the structure type contains any non value type, this method
        ///     will crash.
        /// </summary>
        /// <param name="bytes">Byte span containing the binary data</param>
        /// <param name="start">Start on the span where the structure begins</param>
        /// <param name="length">Length of the structure in bytes</param>
        /// <typeparam name="T">Type of the structure to marshal</typeparam>
        /// <returns>The binary data marshalled in a structure with the specified type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SpanToStructureLittleEndian<T>(ReadOnlySpan<byte> bytes, int start, int length)
            where T : struct
        {
            return MemoryMarshal.Read<T>(bytes.Slice(start, length));
        }

        /// <summary>
        ///     Marshal big-endian binary data to a structure. If the structure type contains any non value type, this method will
        ///     crash.
        /// </summary>
        /// <param name="bytes">Byte array containing the binary data</param>
        /// <typeparam name="T">Type of the structure to marshal</typeparam>
        /// <returns>The binary data marshalled in a structure with the specified type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SpanToStructureBigEndian<T>(ReadOnlySpan<byte> bytes) where T : struct
        {
            var str = SpanToStructureLittleEndian<T>(bytes);
            return (T) SwapStructureMembersEndian(str);
        }

        /// <summary>
        ///     Marshal big-endian binary data to a structure. If the structure type contains any non value type, this method will
        ///     crash.
        /// </summary>
        /// <param name="bytes">Byte span containing the binary data</param>
        /// <param name="start">Start on the span where the structure begins</param>
        /// <param name="length">Length of the structure in bytes</param>
        /// <typeparam name="T">Type of the structure to marshal</typeparam>
        /// <returns>The binary data marshalled in a structure with the specified type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SpanToStructureBigEndian<T>(ReadOnlySpan<byte> bytes, int start, int length) where T : struct
        {
            var str = SpanToStructureLittleEndian<T>(bytes.Slice(start, length));
            return (T) SwapStructureMembersEndian(str);
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
            return (T) SwapStructureMembersEndianPdp(str);
        }

        /// <summary>
        ///     Marshal PDP-11 binary data to a structure. If the structure type contains any non value type, this method will
        ///     crash.
        /// </summary>
        /// <param name="bytes">Byte array containing the binary data</param>
        /// <typeparam name="T">Type of the structure to marshal</typeparam>
        /// <returns>The binary data marshalled in a structure with the specified type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SpanToStructurePdpEndian<T>(ReadOnlySpan<byte> bytes, int start, int length) where T : struct
        {
            object str = SpanToStructureLittleEndian<T>(bytes.Slice(start, length));
            return (T) SwapStructureMembersEndianPdp(str);
        }

        /// <summary>
        ///     Marshal a structure depending on the decoration of <see cref="MarshallingPropertiesAttribute" />. If the decoration
        ///     is not present it will marshal as a reference type containing little endian structure.
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
            if (!(typeof(T).GetCustomAttribute(typeof(MarshallingPropertiesAttribute)) is MarshallingPropertiesAttribute
                properties)) return ByteArrayToStructureLittleEndian<T>(bytes);

            switch (properties.Endian)
            {
                case BitEndian.Little:
                    return properties.HasReferences
                        ? ByteArrayToStructureLittleEndian<T>(bytes)
                        : SpanToStructureLittleEndian<T>(bytes);

                    break;
                case BitEndian.Big:
                    return properties.HasReferences
                        ? ByteArrayToStructureBigEndian<T>(bytes)
                        : SpanToStructureBigEndian<T>(bytes);

                    break;

                case BitEndian.Pdp:
                    return properties.HasReferences
                        ? ByteArrayToStructurePdpEndian<T>(bytes)
                        : SpanToStructurePdpEndian<T>(bytes);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Swaps all members of a structure
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object SwapStructureMembersEndian(object str)
        {
            var t = str.GetType();
            var fieldInfo = t.GetFields();
            foreach (var fi in fieldInfo)
                if (fi.FieldType == typeof(short))
                {
                    var x = (short) fi.GetValue(str);
                    fi.SetValue(str, (short) ((x << 8) | ((x >> 8) & 0xFF)));
                }
                else if (fi.FieldType == typeof(int))
                {
                    var x = (int) fi.GetValue(str);
                    x = (int) (((x << 8) & 0xFF00FF00) | (((uint) x >> 8) & 0xFF00FF));
                    fi.SetValue(str, (int) (((uint) x << 16) | (((uint) x >> 16) & 0xFFFF)));
                }
                else if (fi.FieldType == typeof(long))
                {
                    var x = (long) fi.GetValue(str);
                    x = ((x & 0x00000000FFFFFFFF) << 32) | (long) (((ulong) x & 0xFFFFFFFF00000000) >> 32);
                    x = ((x & 0x0000FFFF0000FFFF) << 16) | (long) (((ulong) x & 0xFFFF0000FFFF0000) >> 16);
                    x = ((x & 0x00FF00FF00FF00FF) << 8) | (long) (((ulong) x & 0xFF00FF00FF00FF00) >> 8);

                    fi.SetValue(str, x);
                }
                else if (fi.FieldType == typeof(ushort))
                {
                    var x = (ushort) fi.GetValue(str);
                    fi.SetValue(str, (ushort) ((x << 8) | (x >> 8)));
                }
                else if (fi.FieldType == typeof(uint))
                {
                    var x = (uint) fi.GetValue(str);
                    x = ((x << 8) & 0xFF00FF00) | ((x >> 8) & 0xFF00FF);
                    fi.SetValue(str, (x << 16) | (x >> 16));
                }
                else if (fi.FieldType == typeof(ulong))
                {
                    var x = (ulong) fi.GetValue(str);
                    x = ((x & 0x00000000FFFFFFFF) << 32) | ((x & 0xFFFFFFFF00000000) >> 32);
                    x = ((x & 0x0000FFFF0000FFFF) << 16) | ((x & 0xFFFF0000FFFF0000) >> 16);
                    x = ((x & 0x00FF00FF00FF00FF) << 8) | ((x & 0xFF00FF00FF00FF00) >> 8);
                    fi.SetValue(str, x);
                }
                else if (fi.FieldType == typeof(float))
                {
                    var flt = (float) fi.GetValue(str);
                    var flt_b = BitConverter.GetBytes(flt);
                    fi.SetValue(str, BitConverter.ToSingle(new[] {flt_b[3], flt_b[2], flt_b[1], flt_b[0]}, 0));
                }
                else if (fi.FieldType == typeof(double))
                {
                    var dbl = (double) fi.GetValue(str);
                    var dbl_b = BitConverter.GetBytes(dbl);
                    fi.SetValue(str,
                        BitConverter
                            .ToDouble(
                                new[] {dbl_b[7], dbl_b[6], dbl_b[5], dbl_b[4], dbl_b[3], dbl_b[2], dbl_b[1], dbl_b[0]},
                                0));
                }
                else if (fi.FieldType == typeof(byte) || fi.FieldType == typeof(sbyte))
                {
                    // Do nothing, can't byteswap them!
                }
                else if (fi.FieldType == typeof(Guid))
                {
                    // TODO: Swap GUID
                }
                // TODO: Swap arrays and enums
                else if (fi.FieldType.IsValueType && !fi.FieldType.IsEnum && !fi.FieldType.IsArray)
                {
                    var obj = fi.GetValue(str);
                    var strc = SwapStructureMembersEndian(obj);
                    fi.SetValue(str, strc);
                }

            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object SwapStructureMembersEndianPdp(object str)
        {
            var t = str.GetType();
            var fieldInfo = t.GetFields();
            foreach (var fi in fieldInfo)
                if (fi.FieldType == typeof(short) || fi.FieldType == typeof(long) || fi.FieldType == typeof(ushort) ||
                    fi.FieldType == typeof(ulong) || fi.FieldType == typeof(float) || fi.FieldType == typeof(double) ||
                    fi.FieldType == typeof(byte) || fi.FieldType == typeof(sbyte) || fi.FieldType == typeof(Guid))
                {
                    // Do nothing
                }
                else if (fi.FieldType == typeof(int))
                {
                    var x = (int) fi.GetValue(str);
                    fi.SetValue(str, ((x & 0xffff) << 16) | ((x & 0xffff0000) >> 16));
                }
                else if (fi.FieldType == typeof(uint))
                {
                    var x = (uint) fi.GetValue(str);
                    fi.SetValue(str, ((x & 0xffff) << 16) | ((x & 0xffff0000) >> 16));
                }
                // TODO: Swap arrays and enums
                else if (fi.FieldType.IsValueType && !fi.FieldType.IsEnum && !fi.FieldType.IsArray)
                {
                    var obj = fi.GetValue(str);
                    var strc = SwapStructureMembersEndianPdp(obj);
                    fi.SetValue(str, strc);
                }

            return str;
        }

        /// <summary>
        ///     Marshal a structure to little-endian binary data
        /// </summary>
        /// <param name="bytes">Byte array containing the binary data</param>
        /// <typeparam name="T">Type of the structure to marshal</typeparam>
        /// <returns>The binary data marshalled in a structure with the specified type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] StructureToByteArrayLittleEndian<T>(T str) where T : struct
        {
            var buf = new byte[SizeOf<T>()];
            var ptr = GCHandle.Alloc(buf, GCHandleType.Pinned);
            System.Runtime.InteropServices.Marshal.StructureToPtr(str, ptr.AddrOfPinnedObject(), false);
            ptr.Free();
            return buf;
        }
    }
}