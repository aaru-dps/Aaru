// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BeOS filesystem plugin.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

// Information from Practical Filesystem Design, ISBN 1-55860-497-9
/// <inheritdoc />
/// <summary>Implements detection of the Be (new) filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class BeFS
{
    // Little endian constants (that is, as read by .NET :p)
    const uint BEFS_MAGIC1 = 0x42465331;
    const uint BEFS_MAGIC2 = 0xDD121031;
    const uint BEFS_MAGIC3 = 0x15B6830E;
    const uint BEFS_ENDIAN = 0x42494745;

    // Big endian constants
    const uint BEFS_CIGAM1 = 0x31534642;
    const uint BEFS_NAIDNE = 0x45474942;

    // Common constants
    const uint BEFS_CLEAN = 0x434C454E;
    const uint BEFS_DIRTY = 0x44495254;

    const string FS_TYPE = "befs";
}