// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BigEndianMarshal.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides marshalling for big-endian data.
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
using System.Reflection;
using System.Runtime.InteropServices;

namespace DiscImageChef
{
    public static class BigEndianMarshal
    {
        /// <summary>
        ///     Marshals a big endian structure from a byte array.
        ///     Nested structures are still marshalled as little endian.
        /// </summary>
        /// <returns>The structure.</returns>
        /// <param name="bytes">Byte array.</param>
        /// <typeparam name="T">Structure type.</typeparam>
        public static T ByteArrayToStructureBigEndian<T>(byte[] bytes) where T : struct
        {
            GCHandle ptr = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T str = (T)Marshal.PtrToStructure(ptr.AddrOfPinnedObject(), typeof(T));
            ptr.Free();
            return SwapStructureMembersEndian(str);
        }

        /// <summary>
        ///     Swaps endian of structure members that correspond to numerical types.
        ///     Does not traverse nested structures.
        /// </summary>
        /// <returns>The structure with its members endian swapped.</returns>
        /// <param name="str">The structure.</param>
        /// <typeparam name="T">Structure type.</typeparam>
        public static T SwapStructureMembersEndian<T>(T str) where T : struct
        {
            Type t = str.GetType();
            FieldInfo[] fieldInfo = t.GetFields();
            foreach(FieldInfo fi in fieldInfo)
                if(fi.FieldType == typeof(short))
                {
                    short int16 = (short)fi.GetValue(str);
                    byte[] int16_b = BitConverter.GetBytes(int16);
                    byte[] int16_r = int16_b.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(str), BitConverter.ToInt16(int16_r, 0));
                }
                else if(fi.FieldType == typeof(int))
                {
                    int int32 = (int)fi.GetValue(str);
                    byte[] int32_b = BitConverter.GetBytes(int32);
                    byte[] int32_r = int32_b.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(str), BitConverter.ToInt32(int32_r, 0));
                }
                else if(fi.FieldType == typeof(long))
                {
                    long int64 = (long)fi.GetValue(str);
                    byte[] int64_b = BitConverter.GetBytes(int64);
                    byte[] int64_r = int64_b.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(str), BitConverter.ToInt64(int64_r, 0));
                }
                else if(fi.FieldType == typeof(ushort))
                {
                    ushort uint16 = (ushort)fi.GetValue(str);
                    byte[] uint16_b = BitConverter.GetBytes(uint16);
                    byte[] uint16_r = uint16_b.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(str), BitConverter.ToUInt16(uint16_r, 0));
                }
                else if(fi.FieldType == typeof(uint))
                {
                    uint uint32 = (uint)fi.GetValue(str);
                    byte[] uint32_b = BitConverter.GetBytes(uint32);
                    byte[] uint32_r = uint32_b.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(str), BitConverter.ToUInt32(uint32_r, 0));
                }
                else if(fi.FieldType == typeof(ulong))
                {
                    ulong uint64 = (ulong)fi.GetValue(str);
                    byte[] uint64_b = BitConverter.GetBytes(uint64);
                    byte[] uint64_r = uint64_b.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(str), BitConverter.ToUInt64(uint64_r, 0));
                }
                else if(fi.FieldType == typeof(float))
                {
                    float flt = (float)fi.GetValue(str);
                    byte[] flt_b = BitConverter.GetBytes(flt);
                    byte[] flt_r = flt_b.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(str), BitConverter.ToSingle(flt_r, 0));
                }
                else if(fi.FieldType == typeof(double))
                {
                    double dbl = (double)fi.GetValue(str);
                    byte[] dbl_b = BitConverter.GetBytes(dbl);
                    byte[] dbl_r = dbl_b.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(str), BitConverter.ToDouble(dbl_r, 0));
                }

            return str;
        }
    }
}