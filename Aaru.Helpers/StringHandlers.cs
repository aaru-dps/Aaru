// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Helpers;

/// <summary>Helper operations to work with strings</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class StringHandlers
{
    /// <summary>Converts a null-terminated (aka C string) ASCII byte array to a C# string</summary>
    /// <returns>The corresponding C# string</returns>
    /// <param name="cString">A null-terminated (aka C string) ASCII byte array</param>
    public static string CToString(byte[] cString) => CToString(cString, Encoding.ASCII);

    /// <summary>Converts a null-terminated (aka C string) byte array with the specified encoding to a C# string</summary>
    /// <returns>The corresponding C# string</returns>
    /// <param name="cString">A null-terminated (aka C string) byte array in the specified encoding</param>
    /// <param name="encoding">Encoding.</param>
    /// <param name="twoBytes">Set if encoding uses 16-bit characters.</param>
    /// <param name="start">Start decoding at this position</param>
    public static string CToString(byte[] cString, Encoding encoding, bool twoBytes = false, int start = 0)
    {
        if(cString == null) return null;

        var len = 0;

        for(int i = start; i < cString.Length; i++)
        {
            if(cString[i] == 0)
            {
                if(twoBytes)
                {
                    if(i + 1 < cString.Length && cString[i + 1] == 0)
                    {
                        len++;

                        break;
                    }
                }
                else
                    break;
            }

            len++;
        }

        if(twoBytes && len % 2 > 0) len--;

        var dest = new byte[len];
        Array.Copy(cString, start, dest, 0, len);

        return len == 0 ? "" : encoding.GetString(dest);
    }

    /// <summary>Converts a length-prefixed (aka Pascal string) ASCII byte array to a C# string</summary>
    /// <returns>The corresponding C# string</returns>
    /// <param name="pascalString">A length-prefixed (aka Pascal string) ASCII byte array</param>
    public static string PascalToString(byte[] pascalString) => PascalToString(pascalString, Encoding.ASCII);

    /// <summary>Converts a length-prefixed (aka Pascal string) ASCII byte array to a C# string</summary>
    /// <returns>The corresponding C# string</returns>
    /// <param name="pascalString">A length-prefixed (aka Pascal string) ASCII byte array</param>
    /// <param name="encoding">Encoding.</param>
    /// <param name="start">Start decoding at this position</param>
    public static string PascalToString(byte[] pascalString, Encoding encoding, int start = 0)
    {
        if(pascalString == null) return null;

        byte length = pascalString[start];
        var  len    = 0;

        for(int i = start + 1; i < length + 1 && i < pascalString.Length; i++)
        {
            if(pascalString[i] == 0) break;

            len++;
        }

        var dest = new byte[len];
        Array.Copy(pascalString, start + 1, dest, 0, len);

        return len == 0 ? "" : encoding.GetString(dest);
    }

    /// <summary>Converts a space (' ', 0x20, ASCII SPACE) padded ASCII byte array to a C# string</summary>
    /// <returns>The corresponding C# string</returns>
    /// <param name="spacePaddedString">A space (' ', 0x20, ASCII SPACE) padded ASCII byte array</param>
    public static string SpacePaddedToString(byte[] spacePaddedString) =>
        SpacePaddedToString(spacePaddedString, Encoding.ASCII);

    /// <summary>Converts a space (' ', 0x20, ASCII SPACE) padded ASCII byte array to a C# string</summary>
    /// <returns>The corresponding C# string</returns>
    /// <param name="spacePaddedString">A space (' ', 0x20, ASCII SPACE) padded ASCII byte array</param>
    /// <param name="encoding">Encoding.</param>
    /// <param name="start">Start decoding at this position</param>
    public static string SpacePaddedToString(byte[] spacePaddedString, Encoding encoding, int start = 0)
    {
        if(spacePaddedString == null) return null;

        int len = start;

        for(int i = spacePaddedString.Length; i >= start; i--)
        {
            if(i == start) return "";

            if(spacePaddedString[i - 1] == 0x20) continue;

            len = i;

            break;
        }

        return len == 0 ? "" : encoding.GetString(spacePaddedString, start, len);
    }

    /// <summary>Converts an OSTA compressed unicode byte array to a C# string</summary>
    /// <returns>The C# string.</returns>
    /// <param name="dstring">OSTA compressed unicode byte array.</param>
    public static string DecompressUnicode(byte[] dstring)
    {
        byte compId = dstring[0];
        var  temp   = "";

        if(compId != 8 && compId != 16) return null;

        for(var byteIndex = 1; byteIndex < dstring.Length;)
        {
            ushort unicode;

            if(compId == 16)
                unicode = (ushort)(dstring[byteIndex++] << 8);
            else
                unicode = 0;

            if(byteIndex < dstring.Length) unicode |= dstring[byteIndex++];

            if(unicode == 0) break;

            temp += Encoding.Unicode.GetString(BitConverter.GetBytes(unicode));
        }

        return temp;
    }
}