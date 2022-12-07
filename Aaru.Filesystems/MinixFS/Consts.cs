// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MINIX filesystem plugin.
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

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the MINIX filesystem</summary>
public sealed partial class MinixFS
{
    /// <summary>Minix v1, 14 char filenames</summary>
    const ushort MINIX_MAGIC = 0x137F;
    /// <summary>Minix v1, 30 char filenames</summary>
    const ushort MINIX_MAGIC2 = 0x138F;
    /// <summary>Minix v2, 14 char filenames</summary>
    const ushort MINIX2_MAGIC = 0x2468;
    /// <summary>Minix v2, 30 char filenames</summary>
    const ushort MINIX2_MAGIC2 = 0x2478;
    /// <summary>Minix v3, 60 char filenames</summary>
    const ushort MINIX3_MAGIC = 0x4D5A;

    // Byteswapped
    /// <summary>Minix v1, 14 char filenames</summary>
    const ushort MINIX_CIGAM = 0x7F13;
    /// <summary>Minix v1, 30 char filenames</summary>
    const ushort MINIX_CIGAM2 = 0x8F13;
    /// <summary>Minix v2, 14 char filenames</summary>
    const ushort MINIX2_CIGAM = 0x6824;
    /// <summary>Minix v2, 30 char filenames</summary>
    const ushort MINIX2_CIGAM2 = 0x7824;
    /// <summary>Minix v3, 60 char filenames</summary>
    const ushort MINIX3_CIGAM = 0x5A4D;

    const string FS_TYPE_V1 = "minix";
    const string FS_TYPE_V2 = "minix2";
    const string FS_TYPE_V3 = "minix3";
}