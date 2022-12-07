// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Locus filesystem plugin
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
//     License aint with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// Commit count

using commitcnt_t = System.Int32;

// Disk address
using daddr_t = System.Int32;

// Fstore
using fstore_t = System.Int32;

// Global File System number
using gfs_t = System.Int32;

// Inode number
using ino_t = System.Int32;

// Filesystem pack number
using pckno_t = System.Int16;

// Timestamp
using time_t = System.Int32;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Locus filesystem</summary>
public sealed partial class Locus
{
    const int NICINOD    = 325;
    const int NICFREE    = 600;
    const int OLDNICINOD = 700;
    const int OLDNICFREE = 500;

    const uint LOCUS_MAGIC     = 0xFFEEDDCD;
    const uint LOCUS_CIGAM     = 0xCDDDEEFF;
    const uint LOCUS_MAGIC_OLD = 0xFFEEDDCC;
    const uint LOCUS_CIGAM_OLD = 0xCCDDEEFF;

    const string FS_TYPE = "locus";
}