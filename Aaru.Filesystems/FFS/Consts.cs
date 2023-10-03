// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BSD Fast File System plugin.
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

using System.Diagnostics.CodeAnalysis;
using time_t = int;
using ufs_daddr_t = int;

namespace Aaru.Filesystems;

// Using information from Linux kernel headers
/// <inheritdoc />
/// <summary>Implements detection of BSD Fast File System (FFS, aka UNIX File System)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class FFSPlugin
{
    const uint block_size = 8192;

    // FreeBSD specifies starts at byte offsets 0, 8192, 65536 and 262144, but in other cases it's following sectors
    // Without bootcode
    const ulong sb_start_floppy = 0;

    // With bootcode
    const ulong sb_start_boot = 1;

    // Dunno, longer boot code
    const ulong sb_start_long_boot = 8;

    // Found on AT&T for MD-2D floppieslzio
    const ulong sb_start_att_dsdd = 14;

    // Found on hard disks (Atari UNIX e.g.)
    const ulong sb_start_piggy = 32;

    // MAGICs
    // UFS magic
    const uint UFS_MAGIC = 0x00011954;

    // Big-endian UFS magic
    const uint UFS_CIGAM = 0x54190100;

    // BorderWare UFS
    const uint UFS_MAGIC_BW = 0x0F242697;

    // Big-endian BorderWare UFS
    const uint UFS_CIGAM_BW = 0x9726240F;

    // UFS2 magic
    const uint UFS2_MAGIC = 0x19540119;

    // Big-endian UFS2 magic
    const uint UFS2_CIGAM = 0x19015419;

    // Incomplete newfs
    const uint UFS_BAD_MAGIC = 0x19960408;

    // Big-endian incomplete newfs
    const uint UFS_BAD_CIGAM = 0x08049619;

    const string FS_TYPE_UFS  = "ufs";
    const string FS_TYPE_UFS2 = "ufs2";
}