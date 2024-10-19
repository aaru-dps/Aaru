// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : F2FS filesystem plugin.
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Flash-Friendly File System (F2FS)</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class F2FS
{
#region Nested type: Superblock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    readonly struct Superblock
    {
        public readonly uint   magic;
        public readonly ushort major_ver;
        public readonly ushort minor_ver;
        public readonly uint   log_sectorsize;
        public readonly uint   log_sectors_per_block;
        public readonly uint   log_blocksize;
        public readonly uint   log_blocks_per_seg;
        public readonly uint   segs_per_sec;
        public readonly uint   secs_per_zone;
        public readonly uint   checksum_offset;
        public readonly ulong  block_count;
        public readonly uint   section_count;
        public readonly uint   segment_count;
        public readonly uint   segment_count_ckpt;
        public readonly uint   segment_count_sit;
        public readonly uint   segment_count_nat;
        public readonly uint   segment_count_ssa;
        public readonly uint   segment_count_main;
        public readonly uint   segment0_blkaddr;
        public readonly uint   cp_blkaddr;
        public readonly uint   sit_blkaddr;
        public readonly uint   nat_blkaddr;
        public readonly uint   ssa_blkaddr;
        public readonly uint   main_blkaddr;
        public readonly uint   root_ino;
        public readonly uint   node_ino;
        public readonly uint   meta_ino;
        public readonly Guid   uuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public readonly byte[] volume_name;
        public readonly uint extension_count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] extension_list1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] extension_list2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] extension_list3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] extension_list4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] extension_list5;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] extension_list6;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] extension_list7;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] extension_list8;
        public readonly uint cp_payload;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public readonly byte[] version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public readonly byte[] init_version;
        public readonly uint feature;
        public readonly byte encryption_level;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] encrypt_pw_salt;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 871)]
        public readonly byte[] reserved;
    }

#endregion
}