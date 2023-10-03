// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NILFS2 filesystem plugin.
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

// ReSharper disable UnusedMember.Local

using System;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the New Implementation of a Log-structured File System v2</summary>
public sealed partial class NILFS2
{
#region Nested type: Superblock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Superblock
    {
        public readonly uint   rev_level;
        public readonly ushort minor_rev_level;
        public readonly ushort magic;
        public readonly ushort bytes;
        public readonly ushort flags;
        public readonly uint   crc_seed;
        public readonly uint   sum;
        public readonly uint   log_block_size;
        public readonly ulong  nsegments;
        public readonly ulong  dev_size;
        public readonly ulong  first_data_block;
        public readonly uint   blocks_per_segment;
        public readonly uint   r_segments_percentage;
        public readonly ulong  last_cno;
        public readonly ulong  last_pseg;
        public readonly ulong  last_seq;
        public readonly ulong  free_blocks_count;
        public readonly ulong  ctime;
        public readonly ulong  mtime;
        public readonly ulong  wtime;
        public readonly ushort mnt_count;
        public readonly ushort max_mnt_count;
        public readonly State  state;
        public readonly ushort errors;
        public readonly ulong  lastcheck;
        public readonly uint   checkinterval;
        public readonly uint   creator_os;
        public readonly ushort def_resuid;
        public readonly ushort def_resgid;
        public readonly uint   first_ino;
        public readonly ushort inode_size;
        public readonly ushort dat_entry_size;
        public readonly ushort checkpoint_size;
        public readonly ushort segment_usage_size;
        public readonly Guid   uuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public readonly byte[] volume_name;
        public readonly uint  c_interval;
        public readonly uint  c_block_max;
        public readonly ulong feature_compat;
        public readonly ulong feature_compat_ro;
        public readonly ulong feature_incompat;
    }

#endregion
}