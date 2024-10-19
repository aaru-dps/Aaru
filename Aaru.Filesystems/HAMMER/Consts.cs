// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : HAMMER filesystem plugin.
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

#pragma warning disable 169

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the HAMMER filesystem</summary>
public sealed partial class HAMMER
{
    const ulong HAMMER_FSBUF_VOLUME     = 0xC8414D4DC5523031;
    const ulong HAMMER_FSBUF_VOLUME_REV = 0x313052C54D4D41C8;
    const uint  HAMMER_VOLHDR_SIZE      = 1928;
    const int   HAMMER_BIGBLOCK_SIZE    = 8192 * 1024;

    const string FS_TYPE = "hammer";
}