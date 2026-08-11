// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Radix50.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : RT-11 file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the RT-11 file system and shows information.
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
// ****************************************************************************/

namespace Aaru.Filesystems;

// Information from http://www.trailing-edge.com/~shoppa/rt11fs/
/// <inheritdoc />
public sealed partial class RT11
{
    /// <summary>Decodes a Radix-50 encoded filename</summary>
    /// <param name="word1">First word of filename</param>
    /// <param name="word2">Second word of filename</param>
    /// <param name="type">File type word</param>
    /// <returns>Decoded filename with extension</returns>
    static string DecodeRadix50Filename(ushort word1, ushort word2, ushort type)
    {
        // Decode 6-character filename (2 words, 3 chars per word)
        string filename = DecodeRadix50Word(word1) + DecodeRadix50Word(word2);
        filename = filename.TrimEnd();

        // Decode 3-character file type
        string fileType = DecodeRadix50Word(type).TrimEnd();

        return string.IsNullOrEmpty(fileType) ? filename : $"{filename}.{fileType}";
    }

    /// <summary>Decodes a single Radix-50 word (3 characters)</summary>
    /// <param name="word">Radix-50 encoded word</param>
    /// <returns>Decoded 3-character string</returns>
    static string DecodeRadix50Word(ushort word)
    {
        // Radix-50 character set: " ABCDEFGHIJKLMNOPQRSTUVWXYZ$.%0123456789"
        const string radix50Chars = " ABCDEFGHIJKLMNOPQRSTUVWXYZ$.%0123456789";

        // Maximum legal Radix-50 word is 39*1600 + 39*40 + 39 = 63999
        if(word >= 64000) return "";

        var chars = new char[3];

        // Extract 3 characters (each is 0-39, requiring ~5.3 bits, packed in base-40)
        chars[2] = radix50Chars[word        % 40];
        chars[1] = radix50Chars[word / 40   % 40];
        chars[0] = radix50Chars[word / 1600 % 40];

        return new string(chars);
    }
}