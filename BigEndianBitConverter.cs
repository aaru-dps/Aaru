// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;

namespace DiscImageChef
{
    /// <summary>
    /// Converts base data types to an array of bytes, and an array of bytes to base
    /// data types.
    /// All info taken from the meta data of System.BitConverter. This implementation
    /// allows for Endianness consideration.
    ///</summary>
    public static class BigEndianBitConverter
    {
        /// <summary>
        /// Indicates the byte order ("endianess") in which data is stored in this computer
        /// architecture.
        ///</summary>
        public static bool IsLittleEndian { get; set; }
        // should default to false, which is what we want for Empire
        /// <summary>
        /// Converts the specified double-precision floating point number to a 64-bit
        /// signed integer.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// A 64-bit signed integer whose value is equivalent to value.
        ///</summary>
        public static long DoubleToInt64Bits(double value)
        {
            throw new NotImplementedException();
        }

        ///
        /// <summary>
        /// Returns the specified Boolean value as an array of bytes.
        ///
        /// Parameters:
        /// value:
        /// A Boolean value.
        ///
        /// Returns:
        /// An array of bytes with length 1.
        ///</summary>
        public static byte[] GetBytes(bool value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Returns the specified Unicode character value as an array of bytes.
        ///
        /// Parameters:
        /// value:
        /// A character to convert.
        ///
        /// Returns:
        /// An array of bytes with length 2.
        ///</summary>
        public static byte[] GetBytes(char value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Returns the specified double-precision floating point value as an array of
        /// bytes.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// An array of bytes with length 8.
        ///</summary>
        public static byte[] GetBytes(double value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Returns the specified single-precision floating point value as an array of
        /// bytes.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// An array of bytes with length 4.
        ///</summary>
        public static byte[] GetBytes(float value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Returns the specified 32-bit signed integer value as an array of bytes.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// An array of bytes with length 4.
        ///</summary>
        public static byte[] GetBytes(int value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Returns the specified 64-bit signed integer value as an array of bytes.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// An array of bytes with length 8.
        ///</summary>
        public static byte[] GetBytes(long value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Returns the specified 16-bit signed integer value as an array of bytes.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// An array of bytes with length 2.
        ///</summary>
        public static byte[] GetBytes(short value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Returns the specified 32-bit unsigned integer value as an array of bytes.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// An array of bytes with length 4.
        ///</summary>
        //[CLSCompliant(false)]
        public static byte[] GetBytes(uint value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Returns the specified 64-bit unsigned integer value as an array of bytes.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// An array of bytes with length 8.
        ///</summary>
        //[CLSCompliant(false)]
        public static byte[] GetBytes(ulong value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Returns the specified 16-bit unsigned integer value as an array of bytes.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// An array of bytes with length 2.
        ///</summary>
        public static byte[] GetBytes(ushort value)
        {
            return !IsLittleEndian ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray();
        }

        ///
        /// <summary>
        /// Converts the specified 64-bit signed integer to a double-precision floating
        /// point number.
        ///
        /// Parameters:
        /// value:
        /// The number to convert.
        ///
        /// Returns:
        /// A double-precision floating point number whose value is equivalent to value.
        ///</summary>
        public static double Int64BitsToDouble(long value)
        {
            throw new NotImplementedException();
        }

        ///
        /// <summary>
        /// Returns a Boolean value converted from one byte at a specified position in
        /// a byte array.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// true if the byte at startIndex in value is nonzero; otherwise, false.
        ///
        /// Exceptions:
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static bool ToBoolean(byte[] value, int startIndex)
        {
            throw new NotImplementedException();
        }

        ///
        /// <summary>
        /// Returns a Unicode character converted from two bytes at a specified position
        /// in a byte array.
        ///
        /// Parameters:
        /// value:
        /// An array.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A character formed by two bytes beginning at startIndex.
        ///
        /// Exceptions:
        /// System.ArgumentException:
        /// startIndex equals the length of value minus 1.
        ///
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static char ToChar(byte[] value, int startIndex)
        {
            throw new NotImplementedException();
        }

        ///
        /// <summary>
        /// Returns a double-precision floating point number converted from eight bytes
        /// at a specified position in a byte array.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A double precision floating point number formed by eight bytes beginning
        /// at startIndex.
        ///
        /// Exceptions:
        /// System.ArgumentException:
        /// startIndex is greater than or equal to the length of value minus 7, and is
        /// less than or equal to the length of value minus 1.
        ///
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static double ToDouble(byte[] value, int startIndex)
        {
            throw new NotImplementedException();
        }

        ///
        /// <summary>
        /// Returns a 16-bit signed integer converted from two bytes at a specified position
        /// in a byte array.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A 16-bit signed integer formed by two bytes beginning at startIndex.
        ///
        /// Exceptions:
        /// System.ArgumentException:
        /// startIndex equals the length of value minus 1.
        ///
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static short ToInt16(byte[] value, int startIndex)
        {
            return !IsLittleEndian ? BitConverter.ToInt16(value, startIndex) : BitConverter.ToInt16(value.Reverse().ToArray(), value.Length - sizeof(short) - startIndex);
        }

        ///
        /// <summary>
        /// Returns a 32-bit signed integer converted from four bytes at a specified
        /// position in a byte array.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A 32-bit signed integer formed by four bytes beginning at startIndex.
        ///
        /// Exceptions:
        /// System.ArgumentException:
        /// startIndex is greater than or equal to the length of value minus 3, and is
        /// less than or equal to the length of value minus 1.
        ///
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static int ToInt32(byte[] value, int startIndex)
        {
            return !IsLittleEndian ? BitConverter.ToInt32(value, startIndex) : BitConverter.ToInt32(value.Reverse().ToArray(), value.Length - sizeof(int) - startIndex);
        }

        ///
        /// <summary>
        /// Returns a 64-bit signed integer converted from eight bytes at a specified
        /// position in a byte array.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A 64-bit signed integer formed by eight bytes beginning at startIndex.
        ///
        /// Exceptions:
        /// System.ArgumentException:
        /// startIndex is greater than or equal to the length of value minus 7, and is
        /// less than or equal to the length of value minus 1.
        ///
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static long ToInt64(byte[] value, int startIndex)
        {
            return !IsLittleEndian ? BitConverter.ToInt64(value, startIndex) : BitConverter.ToInt64(value.Reverse().ToArray(), value.Length - sizeof(long) - startIndex);
        }

        ///
        /// <summary>
        /// Returns a single-precision floating point number converted from four bytes
        /// at a specified position in a byte array.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A single-precision floating point number formed by four bytes beginning at
        /// startIndex.
        ///
        /// Exceptions:
        /// System.ArgumentException:
        /// startIndex is greater than or equal to the length of value minus 3, and is
        /// less than or equal to the length of value minus 1.
        ///
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static float ToSingle(byte[] value, int startIndex)
        {
            return !IsLittleEndian ? BitConverter.ToSingle(value, startIndex) : BitConverter.ToSingle(value.Reverse().ToArray(), value.Length - sizeof(float) - startIndex);
        }

        ///
        /// <summary>
        /// Converts the numeric value of each element of a specified array of bytes
        /// to its equivalent hexadecimal string representation.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// Returns:
        /// A System.String of hexadecimal pairs separated by hyphens, where each pair
        /// represents the corresponding element in value; for example, "7F-2C-4A".
        ///
        /// Exceptions:
        /// System.ArgumentNullException:
        /// value is null.
        ///</summary>
        public static string ToString(byte[] value)
        {
            return !IsLittleEndian ? BitConverter.ToString(value) : BitConverter.ToString(value.Reverse().ToArray());
        }

        ///
        /// <summary>
        /// Converts the numeric value of each element of a specified subarray of bytes
        /// to its equivalent hexadecimal string representation.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A System.String of hexadecimal pairs separated by hyphens, where each pair
        /// represents the corresponding element in a subarray of value; for example,
        /// "7F-2C-4A".
        ///
        /// Exceptions:
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static string ToString(byte[] value, int startIndex)
        {
            return !IsLittleEndian ? BitConverter.ToString(value, startIndex) : BitConverter.ToString(value.Reverse().ToArray(), startIndex);
        }

        ///
        /// <summary>
        /// Converts the numeric value of each element of a specified subarray of bytes
        /// to its equivalent hexadecimal string representation.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// length:
        /// The number of array elements in value to convert.
        ///
        /// Returns:
        /// A System.String of hexadecimal pairs separated by hyphens, where each pair
        /// represents the corresponding element in a subarray of value; for example,
        /// "7F-2C-4A".
        ///
        /// Exceptions:
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex or length is less than zero. -or- startIndex is greater than
        /// zero and is greater than or equal to the length of value.
        ///
        /// System.ArgumentException:
        /// The combination of startIndex and length does not specify a position within
        /// value; that is, the startIndex parameter is greater than the length of value
        /// minus the length parameter.
        ///</summary>
        public static string ToString(byte[] value, int startIndex, int length)
        {
            return !IsLittleEndian ? BitConverter.ToString(value, startIndex, length) : BitConverter.ToString(value.Reverse().ToArray(), startIndex, length);
        }

        ///
        /// <summary>
        /// Returns a 16-bit unsigned integer converted from two bytes at a specified
        /// position in a byte array.
        ///
        /// Parameters:
        /// value:
        /// The array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A 16-bit unsigned integer formed by two bytes beginning at startIndex.
        ///
        /// Exceptions:
        /// System.ArgumentException:
        /// startIndex equals the length of value minus 1.
        ///
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            return !IsLittleEndian ? BitConverter.ToUInt16(value, startIndex) : BitConverter.ToUInt16(value.Reverse().ToArray(), value.Length - sizeof(ushort) - startIndex);
        }

        ///
        /// <summary>
        /// Returns a 32-bit unsigned integer converted from four bytes at a specified
        /// position in a byte array.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A 32-bit unsigned integer formed by four bytes beginning at startIndex.
        ///
        /// Exceptions:
        /// System.ArgumentException:
        /// startIndex is greater than or equal to the length of value minus 3, and is
        /// less than or equal to the length of value minus 1.
        ///
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return !IsLittleEndian ? BitConverter.ToUInt32(value, startIndex) : BitConverter.ToUInt32(value.Reverse().ToArray(), value.Length - sizeof(uint) - startIndex);
        }

        ///
        /// <summary>
        /// Returns a 64-bit unsigned integer converted from eight bytes at a specified
        /// position in a byte array.
        ///
        /// Parameters:
        /// value:
        /// An array of bytes.
        ///
        /// startIndex:
        /// The starting position within value.
        ///
        /// Returns:
        /// A 64-bit unsigned integer formed by the eight bytes beginning at startIndex.
        ///
        /// Exceptions:
        /// System.ArgumentException:
        /// startIndex is greater than or equal to the length of value minus 7, and is
        /// less than or equal to the length of value minus 1.
        ///
        /// System.ArgumentNullException:
        /// value is null.
        ///
        /// System.ArgumentOutOfRangeException:
        /// startIndex is less than zero or greater than the length of value minus 1.
        ///</summary>
        public static ulong ToUInt64(byte[] value, int startIndex)
        {
            return !IsLittleEndian ? BitConverter.ToUInt64(value, startIndex) : BitConverter.ToUInt64(value.Reverse().ToArray(), value.Length - sizeof(ulong) - startIndex);
        }

        public static Guid ToGuid(byte[] value, int startIndex)
        {
            return new Guid(ToUInt32(value, 0 + startIndex),
                ToUInt16(value, 4 + startIndex),
                ToUInt16(value, 6 + startIndex),
                value[8 + startIndex + 0], value[8 + startIndex + 1],
                value[8 + startIndex + 2], value[8 + startIndex + 3],
                value[8 + startIndex + 5], value[8 + startIndex + 5],
                value[8 + startIndex + 6], value[8 + startIndex + 7]);
        }
    }
}