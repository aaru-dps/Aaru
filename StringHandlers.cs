// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : StringHandlers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Convert byte arrays to C# strings.
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
using System.Text;

namespace DiscImageChef
{
    public static class StringHandlers
    {
        /// <summary>
        /// Converts a null-terminated (aka C string) ASCII byte array to a C# string
        /// </summary>
        /// <returns>The corresponding C# string</returns>
        /// <param name="CString">A null-terminated (aka C string) ASCII byte array</param>
        public static string CToString(byte[] CString)
        {
            return CToString(CString, Encoding.ASCII);
        }

        /// <summary>
        /// Converts a null-terminated (aka C string) byte array with the specified encoding to a C# string
        /// </summary>
        /// <returns>The corresponding C# string</returns>
        /// <param name="CString">A null-terminated (aka C string) byte array in the specified encoding</param>
        /// <param name="encoding">Encoding.</param>
        public static string CToString(byte[] CString, Encoding encoding, bool twoBytes = false, int start = 0)
        {
            if(CString == null)
                return null;

            int len = 0;

            for(int i = start; i < CString.Length; i++)
            {
                if(CString[i] == 0)
                {
                    if(twoBytes)
                    {
                        if((i + 1) < CString.Length && CString[i + 1] == 0)
                        {
                            len++;
                            break;
                        }
  //                      if((i + 1) == CString.Length)
//                            break;
                    }
                    else
                        break;
                }

                len++;
            }

            byte[] dest = new byte[len];
            Array.Copy(CString, start, dest, 0, len);

            return len == 0 ? "" : encoding.GetString(dest);
        }

        /// <summary>
        /// Converts a length-prefixed (aka Pascal string) ASCII byte array to a C# string
        /// </summary>
        /// <returns>The corresponding C# string</returns>
        /// <param name="PascalString">A length-prefixed (aka Pascal string) ASCII byte array</param>
        public static string PascalToString(byte[] PascalString)
        {
            return PascalToString(PascalString, Encoding.ASCII);
        }

        /// <summary>
        /// Converts a length-prefixed (aka Pascal string) ASCII byte array to a C# string
        /// </summary>
        /// <returns>The corresponding C# string</returns>
        /// <param name="PascalString">A length-prefixed (aka Pascal string) ASCII byte array</param>
        /// <param name="encoding">Encoding.</param>
        public static string PascalToString(byte[] PascalString, Encoding encoding, int start = 0)
        {
            if(PascalString == null)
                return null;

            byte length = PascalString[start];
            int len = 0;

            for(int i = start + 1; i < length + 1 && i < PascalString.Length; i++)
            {
                if(PascalString[i] == 0)
                    break;
                
                len++;
            }

            byte[] dest = new byte[len];
            Array.Copy(PascalString, start + 1, dest, 0, len);

            return len == 0 ? "" : encoding.GetString(dest);
        }

        /// <summary>
        /// Converts a space (' ', 0x20, ASCII SPACE) padded ASCII byte array to a C# string
        /// </summary>
        /// <returns>The corresponding C# string</returns>
        /// <param name="SpacePaddedString">A space (' ', 0x20, ASCII SPACE) padded ASCII byte array</param>
        public static string SpacePaddedToString(byte[] SpacePaddedString)
        {
            return SpacePaddedToString(SpacePaddedString, Encoding.ASCII);
        }

        /// <summary>
        /// Converts a space (' ', 0x20, ASCII SPACE) padded ASCII byte array to a C# string
        /// </summary>
        /// <returns>The corresponding C# string</returns>
        /// <param name="SpacePaddedString">A space (' ', 0x20, ASCII SPACE) padded ASCII byte array</param>
        /// <param name="encoding">Encoding.</param>
        public static string SpacePaddedToString(byte[] SpacePaddedString, Encoding encoding, int start = 0)
        {
            if(SpacePaddedString == null)
                return null;

            int len = start;

            for(int i = SpacePaddedString.Length; i >= start; i--)
            {
                if(i == start)
                    return "";

                if(SpacePaddedString[i - 1] != 0x20)
                {
                    len = i;
                    break;
                }
            }

            return len == 0 ? "" : encoding.GetString(SpacePaddedString, start, len);
        }

        /// <summary>
        /// Converts an OSTA compressed unicode byte array to a C# string
        /// </summary>
        /// <returns>The C# string.</returns>
        /// <param name="dstring">OSTA compressed unicode byte array.</param>
        public static string DecompressUnicode(byte[] dstring)
        {
            ushort unicode;
            byte compId = dstring[0];
            string temp = "";

            if(compId != 8 && compId != 16)
                return null;

            for(int byteIndex = 1; byteIndex < dstring.Length;)
            {
                if(compId == 16)
                    unicode = (ushort)(dstring[byteIndex++] << 8);
                else
                    unicode = 0;

                if(byteIndex < dstring.Length)
                    unicode |= dstring[byteIndex++];

                if(unicode == 0)
                    break;

                temp += Encoding.Unicode.GetString(System.BitConverter.GetBytes(unicode));
            }

            return temp;
        }
    }
}

