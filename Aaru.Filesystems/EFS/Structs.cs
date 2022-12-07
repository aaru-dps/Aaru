// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Extent File System plugin
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
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements identification for the SGI Extent FileSystem</summary>
public sealed partial class EFS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "InconsistentNaming")]
    readonly struct Superblock
    {
        /* 0:   fs size incl. bb 0 (in bb) */
        public readonly int sb_size;
        /* 4:   first cg offset (in bb) */
        public readonly int sb_firstcg;
        /* 8:   cg size (in bb) */
        public readonly int sb_cgfsize;
        /* 12:  inodes/cg (in bb) */
        public readonly short sb_cgisize;
        /* 14:  geom: sectors/track */
        public readonly short sb_sectors;
        /* 16:  geom: heads/cylinder (unused) */
        public readonly short sb_heads;
        /* 18:  num of cg's in the filesystem */
        public readonly short sb_ncg;
        /* 20:  non-0 indicates fsck required */
        public readonly short sb_dirty;
        /* 22:  */
        public readonly short sb_pad0;
        /* 24:  superblock ctime */
        public readonly int sb_time;
        /* 28:  magic [0] */
        public readonly uint sb_magic;
        /* 32:  name of filesystem */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] sb_fname;
        /* 38:  name of filesystem pack */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] sb_fpack;
        /* 44:  bitmap size (in bytes) */
        public readonly int sb_bmsize;
        /* 48:  total free data blocks */
        public readonly int sb_tfree;
        /* 52:  total free inodes */
        public readonly int sb_tinode;
        /* 56:  bitmap offset (grown fs) */
        public readonly int sb_bmblock;
        /* 62:  repl. superblock offset */
        public readonly int sb_replsb;
        /* 64:  last allocated inode */
        public readonly int sb_lastinode;
        /* 68:  unused */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] sb_spare;
        /* 88:  checksum (all above) */
        public readonly uint sb_checksum;
    }
}