// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Ar plugin.
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

namespace Aaru.Archives;

public sealed partial class Ar
{
    /// <summary>Size of the archive global magic header.</summary>
    const int MAGIC_LENGTH = 8;

    /// <summary>Size of each file entry header.</summary>
    const int HEADER_SIZE = 60;

    /// <summary>Maximum Unix timestamp convertible by <see cref="System.DateTimeOffset.FromUnixTimeSeconds" /></summary>
    const long MAX_UNIX_TIME = 253402300799;

    // Header field offsets and sizes
    const int NAME_OFFSET      = 0;
    const int NAME_LENGTH      = 16;
    const int TIMESTAMP_OFFSET = 16;
    const int TIMESTAMP_LENGTH = 12;
    const int UID_OFFSET       = 28;
    const int UID_LENGTH       = 6;
    const int GID_OFFSET       = 34;
    const int GID_LENGTH       = 6;
    const int MODE_OFFSET      = 40;
    const int MODE_LENGTH      = 8;
    const int SIZE_OFFSET      = 48;
    const int SIZE_LENGTH      = 10;

    // Terminator bytes at offsets 58-59
    const byte TERMINATOR_BYTE_1 = 0x60; // '`'
    const byte TERMINATOR_BYTE_2 = 0x0A; // '\n'

    /// <summary>Archive magic: <c>!&lt;arch&gt;\n</c></summary>
    static readonly byte[] MAGIC = "!<arch>\n"u8.ToArray();
}