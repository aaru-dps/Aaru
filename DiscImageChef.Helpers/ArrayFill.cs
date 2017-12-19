// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ArrayFill.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Fills an array with a specified value.
//
// --[ License ] --------------------------------------------------------------
//
//     No license specified by creator.
//
//     Published on https://github.com/mykohsu/Extensions/blob/master/ArrayExtensions.cs
//
//     Assuming open source.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// Copyright(C) 2014 mykohsu
// ****************************************************************************/

using System;
using System.Text;

namespace DiscImageChef
{
    public static partial class ArrayHelpers
    {
        public static void ArrayFill<T>(T[] destinationArray, T value)
        {
            // if called with a single value, wrap the value in an array and call the main function
            ArrayFill(destinationArray, new T[] {value});
        }

        public static void ArrayFill<T>(T[] destinationArray, T[] value)
        {
            if(destinationArray == null) { throw new ArgumentNullException(nameof(destinationArray)); }

            if(value.Length > destinationArray.Length)
            {
                throw new ArgumentException("Length of value array must not be more than length of destination");
            }

            // set the initial array value
            Array.Copy(value, destinationArray, value.Length);

            int arrayToFillHalfLength = destinationArray.Length / 2;
            int copyLength;

            for(copyLength = value.Length; copyLength < arrayToFillHalfLength; copyLength <<= 1)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationArray.Length - copyLength);
        }

        public static string ByteArrayToHex(byte[] array)
        {
            return ByteArrayToHex(array, false);
        }

        public static string ByteArrayToHex(byte[] array, bool upper)
        {
            StringBuilder sb = new StringBuilder();
            for(long i = 0; i < array.LongLength; i++) sb.AppendFormat("{0:x2}", array[i]);

            return upper ? sb.ToString().ToUpper() : sb.ToString();
        }
    }
}