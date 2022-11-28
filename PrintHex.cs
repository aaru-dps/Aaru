// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PrintHex.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prints a byte array as hexadecimal.
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

using System.Text;
using Aaru.Console;

namespace Aaru.Helpers;

/// <summary>Helper operations to get hexadecimal representations of byte arrays</summary>
public static class PrintHex
{
    /// <summary>Prints a byte array as hexadecimal values to the console</summary>
    /// <param name="array">Array</param>
    /// <param name="width">Width of line</param>
    public static void PrintHexArray(byte[] array, int width = 16) =>
        AaruConsole.WriteLine(ByteArrayToHexArrayString(array, width));

    /// <summary>Prints a byte array as hexadecimal values to a string</summary>
    /// <param name="array">Array</param>
    /// <param name="width">Width of line</param>
    /// <param name="color">Use ANSI escape colors for sections</param>
    /// <returns>String containing hexadecimal values</returns>
    public static string ByteArrayToHexArrayString(byte[] array, int width = 16, bool color = false)
    {
        if(array is null)
            return null;

        // TODO: Color list
        // TODO: Allow to change width
        string str          = Localization.Offset;
        int    rows         = array.Length / width;
        int    last         = array.Length % width;
        int    offsetLength = $"{array.Length:X}".Length;
        var    sb           = new StringBuilder();

        switch(last)
        {
            case > 0:
                rows++;

                break;
            case 0:
                last = width;

                break;
        }

        if(offsetLength < str.Length)
            offsetLength = str.Length;

        while(str.Length < offsetLength)
            str += ' ';

        if(color)
            sb.Append("\u001b[36m");

        sb.Append(str);
        sb.Append("  ");

        for(int i = 0; i < width; i++)
            sb.AppendFormat(" {0:X2}", i);

        if(color)
            sb.Append("\u001b[0m");

        sb.AppendLine();

        int b = 0;

        string format = $"{{0:X{offsetLength}}}";

        for(int i = 0; i < rows; i++)
        {
            if(color)
                sb.Append("\u001b[36m");

            sb.AppendFormat(format, b);

            if(color)
                sb.Append("\u001b[0m");

            sb.Append("  ");
            int lastBytes  = i == rows - 1 ? last : width;
            int lastSpaces = width - lastBytes;

            for(int j = 0; j < lastBytes; j++)
            {
                sb.AppendFormat(" {0:X2}", array[b]);
                b++;
            }

            for(int j = 0; j < lastSpaces; j++)
                sb.Append("   ");

            b -= lastBytes;
            sb.Append("   ");

            for(int j = 0; j < lastBytes; j++)
            {
                int v = array[b];
                sb.Append(v is > 31 and < 127 or > 159 ? (char)v : '.');
                b++;
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}