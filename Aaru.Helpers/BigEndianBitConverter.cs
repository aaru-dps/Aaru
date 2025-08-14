// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BigEndianBitConverter.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Override of System.BitConverter that knows how to handle big-endian.
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
// ****************************************************************************/

using System;
using System.Linq;

namespace Aaru.Helpers
{
    /// <summary>
    ///     Converts base data types to an array of bytes, and an array of bytes to base data types. All info taken from
    ///     the meta data of System.BitConverter. This implementation allows for Endianness consideration.
    /// </summary>
    public static class BigEndianBitConverter
    {
        /// <summary>Converts the specified double-precision floating point number to a 64-bit signed integer.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 64-bit signed integer whose value is equivalent to value.</returns>
        /// <exception cref="NotImplementedException">It is not currently implemented</exception>
        public static long DoubleToInt64Bits(double value) => throw new NotImplementedException();

        /// <summary>Returns the specified Boolean value as an array of bytes.</summary>
        /// <param name="value">A Boolean value.</param>
        /// <returns>An array of bytes with length 1.</returns>
        public static byte[] GetBytes(bool value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Returns the specified Unicode character value as an array of bytes.</summary>
        /// <param name="value">A character to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public static byte[] GetBytes(char value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Returns the specified double-precision floating point value as an array of bytes.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        public static byte[] GetBytes(double value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Returns the specified single-precision floating point value as an array of bytes.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        public static byte[] GetBytes(float value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Returns the specified 32-bit signed integer value as an array of bytes.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        public static byte[] GetBytes(int value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Returns the specified 64-bit signed integer value as an array of bytes.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        public static byte[] GetBytes(long value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Returns the specified 16-bit signed integer value as an array of bytes.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public static byte[] GetBytes(short value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Returns the specified 32-bit unsigned integer value as an array of bytes.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        public static byte[] GetBytes(uint value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Returns the specified 64-bit unsigned integer value as an array of bytes.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        public static byte[] GetBytes(ulong value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Returns the specified 16-bit unsigned integer value as an array of bytes.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public static byte[] GetBytes(ushort value) => BitConverter.GetBytes(value).Reverse().ToArray();

        /// <summary>Converts the specified 64-bit signed integer to a double-precision floating point number.</summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A double-precision floating point number whose value is equivalent to value.</returns>
        public static double Int64BitsToDouble(long value) => throw new NotImplementedException();

        /// <summary>Returns a Boolean value converted from one byte at a specified position in a byte array.</summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>true if the byte at <see cref="startIndex" /> in value is nonzero; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <see cref="startIndex" /> is less than zero or greater than the
        ///     length of value minus 1.
        /// </exception>
        public static bool ToBoolean(byte[] value, int startIndex) => throw new NotImplementedException();

        /// <summary>Returns a Unicode character converted from two bytes at a specified position in a byte array.</summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A character formed by two bytes beginning at <see cref="startIndex" />.</returns>
        /// <exception cref="System.ArgumentException"><see cref="startIndex" /> equals the length of value minus 1.</exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <see cref="startIndex" /> is less than zero or greater than the
        ///     length of value minus 1.
        /// </exception>
        public static char ToChar(byte[] value, int startIndex) => throw new NotImplementedException();

        /// <summary>
        ///     Returns a double-precision floating point number converted from eight bytes at a specified position in a byte
        ///     array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A double precision floating point number formed by eight bytes beginning at <see cref="startIndex" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     <see cref="startIndex" /> is greater than or equal to the length of value
        ///     minus 7, and is less than or equal to the length of value minus 1.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <see cref="startIndex" /> is less than zero or greater than the
        ///     length of value minus 1.
        /// </exception>
        public static double ToDouble(byte[] value, int startIndex) => throw new NotImplementedException();

        /// <summary>Returns a 16-bit signed integer converted from two bytes at a specified position in a byte array.</summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 16-bit signed integer formed by two bytes beginning at <see cref="startIndex" />.</returns>
        /// <exception cref="System.ArgumentException"><see cref="startIndex" /> equals the length of value minus 1.</exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     startIndex is less than zero or greater than the length of value
        ///     minus 1.
        /// </exception>
        public static short ToInt16(byte[] value, int startIndex) =>
            BitConverter.ToInt16(value.Reverse().ToArray(), value.Length - sizeof(short) - startIndex);

        /// <summary>Returns a 32-bit signed integer converted from four bytes at a specified position in a byte array.</summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 32-bit signed integer formed by four bytes beginning at <see cref="startIndex" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     <see cref="startIndex" /> is greater than or equal to the length of value
        ///     minus 3, and is less than or equal to the length of value minus 1.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     startIndex is less than zero or greater than the length of value
        ///     minus 1.
        /// </exception>
        public static int ToInt32(byte[] value, int startIndex) =>
            BitConverter.ToInt32(value.Reverse().ToArray(), value.Length - sizeof(int) - startIndex);

        /// <summary>Returns a 64-bit signed integer converted from eight bytes at a specified position in a byte array.</summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 64-bit signed integer formed by eight bytes beginning at <see cref="startIndex" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     <see cref="startIndex" /> is greater than or equal to the length of value
        ///     minus 7, and is less than or equal to the length of value minus 1.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <see cref="startIndex" /> is less than zero or greater than the
        ///     length of value minus 1.
        /// </exception>
        public static long ToInt64(byte[] value, int startIndex) =>
            BitConverter.ToInt64(value.Reverse().ToArray(), value.Length - sizeof(long) - startIndex);

        /// <summary>
        ///     Returns a single-precision floating point number converted from four bytes  at a specified position in a byte
        ///     array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A single-precision floating point number formed by four bytes beginning at <see cref="startIndex" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     <see cref="startIndex" /> is greater than or equal to the length of value
        ///     minus 3, and is less than or equal to the length of value minus 1.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <see cref="startIndex" /> is less than zero or greater than the
        ///     length of value minus 1.
        /// </exception>
        public static float ToSingle(byte[] value, int startIndex) =>
            BitConverter.ToSingle(value.Reverse().ToArray(), value.Length - sizeof(float) - startIndex);

        /// <summary>
        ///     Converts the numeric value of each element of a specified array of bytes to its equivalent hexadecimal string
        ///     representation.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <returns>
        ///     A System.String of hexadecimal pairs separated by hyphens, where each pair represents the corresponding
        ///     element in value; for example, "7F-2C-4A".
        /// </returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        public static string ToString(byte[] value) => BitConverter.ToString(value.Reverse().ToArray());

        /// <summary>
        ///     Converts the numeric value of each element of a specified subarray of bytes to its equivalent hexadecimal
        ///     string representation.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>
        ///     A System.String of hexadecimal pairs separated by hyphens, where each pair represents the corresponding
        ///     element in a subarray of value; for example, "7F-2C-4A".
        /// </returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     startIndex is less than zero or greater than the length of value
        ///     minus 1.
        /// </exception>
        public static string ToString(byte[] value, int startIndex) =>
            BitConverter.ToString(value.Reverse().ToArray(), startIndex);

        /// <summary>
        ///     Converts the numeric value of each element of a specified subarray of bytes to its equivalent hexadecimal
        ///     string representation.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <param name="length">The number of array elements in value to convert.</param>
        /// <returns>
        ///     A System.String of hexadecimal pairs separated by hyphens, where each pair represents the corresponding
        ///     element in a subarray of value; for example, "7F-2C-4A".
        /// </returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     startIndex or length is less than zero. -or- startIndex is greater
        ///     than zero and is greater than or equal to the length of value.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     The combination of startIndex and length does not specify a position within
        ///     value; that is, the startIndex parameter is greater than the length of value minus the length parameter.
        /// </exception>
        public static string ToString(byte[] value, int startIndex, int length) =>
            BitConverter.ToString(value.Reverse().ToArray(), startIndex, length);

        /// <summary>Returns a 16-bit unsigned integer converted from two bytes at a specified position in a byte array.</summary>
        /// <param name="value">The array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 16-bit unsigned integer formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="System.ArgumentException">startIndex equals the length of value minus 1.</exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     startIndex is less than zero or greater than the length of value
        ///     minus 1.
        /// </exception>
        public static ushort ToUInt16(byte[] value, int startIndex) =>
            BitConverter.ToUInt16(value.Reverse().ToArray(), value.Length - sizeof(ushort) - startIndex);

        /// <summary>Returns a 32-bit unsigned integer converted from four bytes at a specified position in a byte array.</summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 32-bit unsigned integer formed by four bytes beginning at startIndex.</returns>
        /// <exception cref="System.ArgumentException">
        ///     startIndex is greater than or equal to the length of value minus 3, and is
        ///     less than or equal to the length of value minus 1.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     startIndex is less than zero or greater than the length of value
        ///     minus 1.
        /// </exception>
        public static uint ToUInt32(byte[] value, int startIndex) =>
            BitConverter.ToUInt32(value.Reverse().ToArray(), value.Length - sizeof(uint) - startIndex);

        /// <summary>Returns a 64-bit unsigned integer converted from eight bytes at a specified position in a byte array.</summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 64-bit unsigned integer formed by the eight bytes beginning at startIndex.</returns>
        /// <exception cref="System.ArgumentException">
        ///     startIndex is greater than or equal to the length of value minus 7, and is
        ///     less than or equal to the length of value minus 1.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     startIndex is less than zero or greater than the length of value
        ///     minus 1.
        /// </exception>
        public static ulong ToUInt64(byte[] value, int startIndex) =>
            BitConverter.ToUInt64(value.Reverse().ToArray(), value.Length - sizeof(ulong) - startIndex);

        /// <summary>Converts a big endian byte array representation of a GUID into the .NET Guid structure</summary>
        /// <param name="value">Byte array containing a GUID in big endian</param>
        /// <param name="startIndex">Start of the byte array to process</param>
        /// <returns>Processed Guid</returns>
        public static Guid ToGuid(byte[] value, int startIndex) => new Guid(ToUInt32(value, 0 + startIndex),
                                                                            ToUInt16(value, 4 + startIndex),
                                                                            ToUInt16(value, 6 + startIndex),
                                                                            value[8           + startIndex + 0],
                                                                            value[8           + startIndex + 1],
                                                                            value[8           + startIndex + 2],
                                                                            value[8           + startIndex + 3],
                                                                            value[8           + startIndex + 5],
                                                                            value[8           + startIndex + 5],
                                                                            value[8           + startIndex + 6],
                                                                            value[8           + startIndex + 7]);
    }
}