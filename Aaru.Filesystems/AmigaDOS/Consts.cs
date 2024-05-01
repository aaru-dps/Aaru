// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Amiga Fast File System plugin.
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

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of Amiga Fast File System (AFFS)</summary>
public sealed partial class AmigaDOSPlugin
{
    const uint FFS_MASK  = 0x444F5300;
    const uint MUFS_MASK = 0x6D754600;

    const uint TYPE_HEADER  = 2;
    const uint SUBTYPE_ROOT = 1;

    const string FS_TYPE_OFS  = "aofs";
    const string FS_TYPE_FFS  = "affs";
    const string FS_TYPE_OFS2 = "aofs2";
    const string FS_TYPE_FFS2 = "affs2";
}