// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CharsetDetection.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Heuristic detection of Shift-JIS encoded identifiers in the volume
//     descriptors.
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
// Copyright © 2011-2026 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.Text;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    static Encoding _shiftJis;
    static bool     _shiftJisSearched;

    /// <summary>Gets the Shift-JIS encoding, or <c>null</c> if the host does not provide it</summary>
    static Encoding ShiftJis
    {
        get
        {
            if(_shiftJisSearched) return _shiftJis;

            _shiftJisSearched = true;

            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                _shiftJis = Encoding.GetEncoding("shift_jis");
            }
            catch
            {
                _shiftJis = null;
            }

            return _shiftJis;
        }
    }

    /// <summary>
    ///     Checks if a volume descriptor identifier field contains Shift-JIS encoded text. ISO 9660 does not provide any
    ///     way to declare the character set used in the primary volume descriptor, its escape sequences field is reserved,
    ///     so this can only be a heuristic over the bytes themselves.
    /// </summary>
    /// <param name="field">Identifier field contents</param>
    /// <returns><c>true</c> if the field looks like Shift-JIS, <c>false</c> if it should be decoded as usual</returns>
    internal static bool IsShiftJis(byte[] field)
    {
        if(field is null) return false;

        // Identifier fields are padded with spaces, and some implementations pad with nulls
        var len = field.Length;

        while(len > 0 && field[len - 1] is 0x20 or 0x00) len--;

        if(len == 0) return false;

        var doubleBytes = 0;
        var katakana    = 0;
        var maxKatakana = 0;

        for(var i = 0; i < len; i++)
        {
            byte b = field[i];

            // Plain ASCII, tells us nothing
            if(b is 0x00 or 0x09 or >= 0x20 and <= 0x7E)
            {
                katakana = 0;

                continue;
            }

            // JIS X 0201 half-width katakana
            if(b is >= 0xA1 and <= 0xDF)
            {
                katakana++;

                if(katakana > maxKatakana) maxKatakana = katakana;

                continue;
            }

            katakana = 0;

            // Lead byte of a double byte sequence, including the vendor extended wards
            if(b is not (>= 0x81 and <= 0x9F or >= 0xE0 and <= 0xFC)) return false;

            // A lead byte must be followed by a valid trail byte
            if(i + 1 >= len) return false;

            byte trail = field[i + 1];

            if(trail is not (>= 0x40 and <= 0x7E or >= 0x80 and <= 0xFC)) return false;

            doubleBytes++;
            i++;
        }

        return doubleBytes > 0 || maxKatakana > 1;
    }

    /// <summary>
    ///     Decodes a volume descriptor identifier field, using Shift-JIS if the field contents look like Shift-JIS, or the
    ///     specified encoding otherwise. Each field is checked on its own, as a disc can mix ASCII and Shift-JIS fields.
    /// </summary>
    /// <param name="field">Identifier field contents</param>
    /// <param name="encoding">Encoding to use when the field is not Shift-JIS</param>
    /// <param name="shiftJis">Set to <c>true</c> if the field has been decoded as Shift-JIS</param>
    /// <returns>Decoded identifier</returns>
    internal static string DecodeIdentifier(byte[] field, Encoding encoding, ref bool shiftJis)
    {
        if(!IsShiftJis(field) || ShiftJis is null)
            return Sanitize(StringHandlers.CToString(field, encoding).TrimEnd());

        shiftJis = true;

        return Sanitize(StringHandlers.CToString(field, ShiftJis).TrimEnd());
    }

    /// <summary>
    ///     Removes the control characters and escapes the console markup in an identifier. Some discs store binary data,
    ///     like copy protection signatures, in the identifier fields, and that must not corrupt the output.
    /// </summary>
    /// <param name="identifier">Decoded identifier</param>
    /// <returns>Identifier safe to print</returns>
    static string Sanitize(string identifier)
    {
        if(string.IsNullOrEmpty(identifier)) return identifier;

        var sb = new StringBuilder(identifier.Length);

        foreach(char c in identifier)
        {
            if(char.IsControl(c)) continue;

            // Aaru prints the volume descriptor information as console markup
            if(c == '[') sb.Append('[');

            sb.Append(c);
        }

        return sb.ToString().TrimEnd();
    }
}
