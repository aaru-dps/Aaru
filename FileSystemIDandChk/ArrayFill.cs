/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : ArrayFill.cs
Version        : 1.0
Author(s)      : https://github.com/mykohsu
 
Component      : Helpers

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Fills an array with a
 
--[ License ] --------------------------------------------------------------
 
    No license specified by creator.

    Published on https://github.com/mykohsu/Extensions/blob/master/ArrayExtensions.cs

    Assuming open source.

----------------------------------------------------------------------------
Copyright (C) 2014 mykohsu
****************************************************************************/
//$Id$
using System;

namespace FileSystemIDandChk
{
    public static class ArrayHelpers
    {
        public static void ArrayFill<T>(T[] destinationArray, T value)
        {
            // if called with a single value, wrap the value in an array and call the main function
            ArrayFill<T>(destinationArray, new T[] { value });
        }

        public static void ArrayFill<T>(T[] destinationArray, T[] value)
        {
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }

            if (value.Length > destinationArray.Length)
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
    }
}

