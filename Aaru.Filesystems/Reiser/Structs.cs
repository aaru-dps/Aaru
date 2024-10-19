// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser filesystem plugin
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

using System;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Reiser v3 filesystem</summary>
public sealed partial class Reiser
{
#region Nested type: JournalParameters

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct JournalParameters
    {
        public readonly uint journal_1stblock;
        public readonly uint journal_dev;
        public readonly uint journal_size;
        public readonly uint journal_trans_max;
        public readonly uint journal_magic;
        public readonly uint journal_max_batch;
        public readonly uint journal_max_commit_age;
        public readonly uint journal_max_trans_age;
    }

#endregion

#region Nested type: Superblock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Superblock
    {
        public readonly uint              block_count;
        public readonly uint              free_blocks;
        public readonly uint              root_block;
        public readonly JournalParameters journal;
        public readonly ushort            blocksize;
        public readonly ushort            oid_maxsize;
        public readonly ushort            oid_cursize;
        public readonly ushort            umount_state;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] magic;
        public readonly ushort fs_state;
        public readonly uint   hash_function_code;
        public readonly ushort tree_height;
        public readonly ushort bmap_nr;
        public readonly ushort version;
        public readonly ushort reserved_for_journal;
        public readonly uint   inode_generation;
        public readonly uint   flags;
        public readonly Guid   uuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] label;
        public readonly ushort mnt_count;
        public readonly ushort max_mnt_count;
        public readonly uint   last_check;
        public readonly uint   check_interval;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 76)]
        public readonly byte[] unused;
    }

#endregion
}