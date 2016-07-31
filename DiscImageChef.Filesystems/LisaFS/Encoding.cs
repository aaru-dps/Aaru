// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Encoding.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple LisaRoman to Unicode converters.
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

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        /// <summary>
        /// The Lisa to Unicode character map.
        /// MacRoman is a superset of LisaRoman.
        /// </summary>
        static readonly char[] LisaRomanTable = {
            // 0x00
            '\u0000','\u0001','\u0002','\u0003','\u0004','\u0005','\u0006','\u0007',
            // 0x08
            '\u0008','\u0009','\u000A','\u000B','\u000C','\u000D','\u000E','\u000F',
            // 0x10
            '\u0010','\u0011','\u0012','\u0013','\u0014','\u0015','\u0016','\u0017',
            // 0x18
            '\u0018','\u0019','\u001A','\u001B','\u001C','\u001D','\u001E','\u001F',
            // 0x20
            '\u0020','\u0021','\u0022','\u0023','\u0024','\u0025','\u0026','\u0027',
            // 0x28
            '\u0028','\u0029','\u002A','\u002B','\u002C','\u002D','\u002E','\u002F',
            // 0x30
            '\u0030','\u0031','\u0032','\u0033','\u0034','\u0035','\u0036','\u0037',
            // 0x38
            '\u0038','\u0039','\u003A','\u003B','\u003C','\u003D','\u003E','\u003F',
            // 0x40
            '\u0040','\u0041','\u0042','\u0043','\u0044','\u0045','\u0046','\u0047',
            // 0x48
            '\u0048','\u0049','\u004A','\u004B','\u004C','\u004D','\u004E','\u004F',
            // 0x50
            '\u0050','\u0051','\u0052','\u0053','\u0054','\u0055','\u0056','\u0057',
            // 0x58
            '\u0058','\u0059','\u005A','\u005B','\u005C','\u005D','\u005E','\u005F',
            // 0x60
            '\u0060','\u0061','\u0062','\u0063','\u0064','\u0065','\u0066','\u0067',
            // 0x68
            '\u0068','\u0069','\u006A','\u006B','\u006C','\u006D','\u006E','\u006F',
            // 0x70
            '\u0070','\u0071','\u0072','\u0073','\u0074','\u0075','\u0076','\u0077',
            // 0x78
            '\u0078','\u0079','\u007A','\u007B','\u007C','\u007D','\u007E','\u2588',
            // 0x80
            '\u00C4','\u00C5','\u00C7','\u00C9','\u00D1','\u00D6','\u00DC','\u00E1',
            // 0x88
            '\u00E0','\u00E2','\u00E4','\u00E3','\u00E5','\u00E7','\u00E9','\u00E8',
            // 0x90
            '\u00EA','\u00EB','\u00ED','\u00EC','\u00EE','\u00EF','\u00F1','\u00F3',
            // 0x98
            '\u00F2','\u00F4','\u00F6','\u00F5','\u00FA','\u00F9','\u00FB','\u00FC',
            // 0xA0
            '\u2020','\u00B0','\u00A2','\u00A3','\u00A7','\u2022','\u00B6','\u00DF',
            // 0xA8
            '\u00AE','\u00A9','\u2122','\u00B4','\u00A8','\u2260','\u00C6','\u00D8',
            // 0xB0
            '\u221E','\u00B1','\u2264','\u2265','\u00A5','\u00B5','\u2202','\u2211',
            // 0xB8
            '\u220F','\u03C0','\u222B','\u00AA','\u00BA','\u03A9','\u00E6','\u00F8',
            // 0xC0
            '\u00BF','\u00A1','\u00AC','\u221A','\u0192','\u2248','\u2206','\u00AB',
            // 0xC8
            '\u00BB','\u2026','\u00A0','\u00C0','\u00C3','\u00D5','\u0152','\u0153',
            // 0xD0
            '\u2013','\u2014','\u201C','\u201D','\u2018','\u2019','\u00F7','\u25CA',
            // 0xD8
            '\u00FF','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000',
            // 0xE0
            '\u0000','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000',
            // 0xE8
            '\u0000','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000',
            // 0xF0
            '\u0000','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000',
            // 0xF8
            '\u0000','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000','\u0000'
        };

        /// <summary>
        /// Converts a LisaRoman character to an Unicode character
        /// </summary>
        /// <returns>Unicode character.</returns>
        /// <param name="character">LisaRoman character.</param>
        static char GetChar(byte character)
        {
            return LisaRomanTable[character];
        }

        /// <summary>
        /// Converts a LisaRoman string, null-terminated or null-paded, to a C# string
        /// </summary>
        /// <returns>The C# string.</returns>
        /// <param name="str">LisaRoman string.</param>
        static string GetString(byte[] str)
        {
            string uni = "";

            foreach(byte b in str)
            {
                if(b == 0x00)
                    break;

                uni += LisaRomanTable[b];
            }

            return uni;
        }

        /// <summary>
        /// Converts a LisaRoman string, in Pascal length-prefixed format, to a C# string
        /// </summary>
        /// <returns>The C# string.</returns>
        /// <param name="PascalString">The LisaRoman string in Pascal format.</param>
        static string GetStringFromPascal(byte[] PascalString)
        {
            if(PascalString == null)
                return null;

            string uni = "";

            byte length = PascalString[0];

            if(length > PascalString.Length - 1)
                length = (byte)(PascalString.Length - 1);

            for(int i = 1; i < length + 1; i++)
            {
                uni += LisaRomanTable[PascalString[i]];
            }

            return uni;
        }
    }
}

