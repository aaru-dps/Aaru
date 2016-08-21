// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : EndianSwapStructure.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DiscImageChef.Helpers
{
    public static class BigEndianStructure
    {
        // TODO: Check this works
        /// <summary>
        /// Marshals a big-endian byte array to a C# structure. Dunno if it works with nested structures.
        /// </summary>
        /// <returns>The big endian byte array.</returns>
        /// <param name="bytes">Byte array.</param>
        /// <typeparam name="T">C# structure type.</typeparam>
        public static T ByteArrayToStructureBigEndian<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            Type t = stuff.GetType();
            FieldInfo[] fieldInfo = t.GetFields();
            foreach(FieldInfo fi in fieldInfo)
            {
                if(fi.FieldType == typeof(short))
                {
                    short i16 = (short)fi.GetValue(stuff);
                    byte[] b16 = BitConverter.GetBytes(i16);
                    byte[] b16r = b16.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(stuff), BitConverter.ToInt16(b16r, 0));
                }
                else if(fi.FieldType == typeof(int))
                {
                    int i32 = (int)fi.GetValue(stuff);
                    byte[] b32 = BitConverter.GetBytes(i32);
                    byte[] b32r = b32.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(stuff), BitConverter.ToInt32(b32r, 0));
                }
                else if(fi.FieldType == typeof(long))
                {
                    long i64 = (long)fi.GetValue(stuff);
                    byte[] b64 = BitConverter.GetBytes(i64);
                    byte[] b64r = b64.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(stuff), BitConverter.ToInt64(b64r, 0));
                }
                else if(fi.FieldType == typeof(ushort))
                {
                    ushort i16 = (ushort)fi.GetValue(stuff);
                    byte[] b16 = BitConverter.GetBytes(i16);
                    byte[] b16r = b16.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(stuff), BitConverter.ToUInt16(b16r, 0));
                }
                else if(fi.FieldType == typeof(uint))
                {
                    uint i32 = (uint)fi.GetValue(stuff);
                    byte[] b32 = BitConverter.GetBytes(i32);
                    byte[] b32r = b32.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(stuff), BitConverter.ToUInt32(b32r, 0));
                }
                else if(fi.FieldType == typeof(ulong))
                {
                    ulong i64 = (ulong)fi.GetValue(stuff);
                    byte[] b64 = BitConverter.GetBytes(i64);
                    byte[] b64r = b64.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(stuff), BitConverter.ToUInt64(b64r, 0));
                }
                else if(fi.FieldType == typeof(float))
                {
                    float iflt = (float)fi.GetValue(stuff);
                    byte[] bflt = BitConverter.GetBytes(iflt);
                    byte[] bfltr = bflt.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(stuff), BitConverter.ToSingle(bfltr, 0));
                }
                else if(fi.FieldType == typeof(double))
                {
                    double idbl = (double)fi.GetValue(stuff);
                    byte[] bdbl = BitConverter.GetBytes(idbl);
                    byte[] bdblr = bdbl.Reverse().ToArray();
                    fi.SetValueDirect(__makeref(stuff), BitConverter.ToDouble(bdblr, 0));
                }
            }
            return stuff;
        }
    }
}

