// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux extended filesystem 2, 3 and 4 plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Linux extended filesystem 2, 3 and 4 and shows information.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the Linux extended filesystem v2, v3 and v4</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed partial class ext2FS
{
    /// <summary>ext2/3/4 superblock</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "InconsistentNaming")]
    struct SuperBlock
    {
        /// <summary>0x000, inodes on volume</summary>
        public readonly uint inodes;
        /// <summary>0x004, blocks on volume</summary>
        public readonly uint blocks;
        /// <summary>0x008, reserved blocks</summary>
        public readonly uint reserved_blocks;
        /// <summary>0x00C, free blocks count</summary>
        public readonly uint free_blocks;
        /// <summary>0x010, free inodes count</summary>
        public readonly uint free_inodes;
        /// <summary>0x014, first data block</summary>
        public readonly uint first_block;
        /// <summary>0x018, block size</summary>
        public uint block_size;
        /// <summary>0x01C, fragment size</summary>
        public readonly int frag_size;
        /// <summary>0x020, blocks per group</summary>
        public readonly uint blocks_per_grp;
        /// <summary>0x024, fragments per group</summary>
        public readonly uint flags_per_grp;
        /// <summary>0x028, inodes per group</summary>
        public readonly uint inodes_per_grp;
        /// <summary>0x02C, last mount time</summary>
        public readonly uint mount_t;
        /// <summary>0x030, last write time</summary>
        public readonly uint write_t;
        /// <summary>0x034, mounts count</summary>
        public readonly ushort mount_c;
        /// <summary>0x036, max mounts</summary>
        public readonly short max_mount_c;
        /// <summary>0x038, (little endian)</summary>
        public readonly ushort magic;
        /// <summary>0x03A, filesystem state</summary>
        public readonly ushort state;
        /// <summary>0x03C, behaviour on errors</summary>
        public readonly ushort err_behaviour;
        /// <summary>0x03E, From 0.5b onward</summary>
        public readonly ushort minor_revision;
        /// <summary>0x040, last check time</summary>
        public readonly uint check_t;
        /// <summary>0x044, max time between checks</summary>
        public readonly uint check_inv;

        // From 0.5a onward
        /// <summary>0x048, Creation OS</summary>
        public readonly uint creator_os;
        /// <summary>0x04C, Revison level</summary>
        public readonly uint revision;
        /// <summary>0x050, Default UID for reserved blocks</summary>
        public readonly ushort default_uid;
        /// <summary>0x052, Default GID for reserved blocks</summary>
        public readonly ushort default_gid;

        // From 0.5b onward
        /// <summary>0x054, First unreserved inode</summary>
        public readonly uint first_inode;
        /// <summary>0x058, inode size</summary>
        public readonly ushort inode_size;
        /// <summary>0x05A, Block group number of THIS superblock</summary>
        public readonly ushort block_group_no;
        /// <summary>0x05C, Compatible features set</summary>
        public readonly uint ftr_compat;
        /// <summary>0x060, Incompatible features set</summary>
        public readonly uint ftr_incompat;

        // Found on Linux 2.0.40
        /// <summary>0x064, Read-only compatible features set</summary>
        public readonly uint ftr_ro_compat;

        // Found on Linux 2.1.132
        /// <summary>0x068, 16 bytes, UUID</summary>
        public readonly Guid uuid;
        /// <summary>0x078, 16 bytes, volume name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] volume_name;
        /// <summary>0x088, 64 bytes, where last mounted</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] last_mount_dir;
        /// <summary>0x0C8, Usage bitmap algorithm, for compression</summary>
        public readonly uint algo_usage_bmp;
        /// <summary>0x0CC, Block to try to preallocate</summary>
        public readonly byte prealloc_blks;
        /// <summary>0x0CD, Blocks to try to preallocate for directories</summary>
        public readonly byte prealloc_dir_blks;
        /// <summary>0x0CE, Per-group desc for online growth</summary>
        public readonly ushort rsrvd_gdt_blocks;

        // Found on Linux 2.4
        // ext3
        /// <summary>0x0D0, 16 bytes, UUID of journal superblock</summary>
        public readonly Guid journal_uuid;
        /// <summary>0x0E0, inode no. of journal file</summary>
        public readonly uint journal_inode;
        /// <summary>0x0E4, device no. of journal file</summary>
        public readonly uint journal_dev;
        /// <summary>0x0E8, Start of list of inodes to delete</summary>
        public readonly uint last_orphan;
        /// <summary>0x0EC, First byte of 128bit HTREE hash seed</summary>
        public readonly uint hash_seed_1;
        /// <summary>0x0F0, Second byte of 128bit HTREE hash seed</summary>
        public readonly uint hash_seed_2;
        /// <summary>0x0F4, Third byte of 128bit HTREE hash seed</summary>
        public readonly uint hash_seed_3;
        /// <summary>0x0F8, Fourth byte of 128bit HTREE hash seed</summary>
        public readonly uint hash_seed_4;
        /// <summary>0x0FC, Hash version</summary>
        public readonly byte hash_version;
        /// <summary>0x0FD, Journal backup type</summary>
        public readonly byte jnl_backup_type;
        /// <summary>0x0FE, Size of group descriptor</summary>
        public readonly ushort desc_grp_size;
        /// <summary>0x100, Default mount options</summary>
        public readonly uint default_mnt_opts;
        /// <summary>0x104, First metablock block group</summary>
        public readonly uint first_meta_bg;

        // Introduced with ext4, some can be ext3
        /// <summary>0x108, Filesystem creation time</summary>
        public readonly uint mkfs_t;

        /// <summary>Backup of the journal inode</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public readonly uint[] jnl_blocks;

        // Following 3 fields are valid if EXT4_FEATURE_COMPAT_64BIT is set
        /// <summary>0x14C, High 32bits of blocks no.</summary>
        public readonly uint blocks_hi;
        /// <summary>0x150, High 32bits of reserved blocks no.</summary>
        public readonly uint reserved_blocks_hi;
        /// <summary>0x154, High 32bits of free blocks no.</summary>
        public readonly uint free_blocks_hi;
        /// <summary>0x158, inodes minimal size in bytes</summary>
        public readonly ushort min_inode_size;
        /// <summary>0x15A, Bytes reserved by new inodes</summary>
        public readonly ushort rsv_inode_size;
        /// <summary>0x15C, Flags</summary>
        public readonly uint flags;
        /// <summary>0x160, RAID stride</summary>
        public readonly ushort raid_stride;
        /// <summary>0x162, Waiting seconds in MMP check</summary>
        public readonly ushort mmp_interval;
        /// <summary>0x164, Block for multi-mount protection</summary>
        public readonly ulong mmp_block;
        /// <summary>0x16C, Blocks on all data disks (N*stride)</summary>
        public readonly uint raid_stripe_width;
        /// <summary>0x170, FLEX_BG group size</summary>
        public readonly byte flex_bg_grp_size;
        /// <summary>0x171 Metadata checksum algorithm</summary>
        public readonly byte checksum_type;
        /// <summary>0x172 Versioning level for encryption</summary>
        public readonly byte encryption_level;
        /// <summary>0x173 Padding</summary>
        public readonly ushort padding;

        // Following are introduced with ext4
        /// <summary>0x174, Kibibytes written in volume lifetime</summary>
        public readonly ulong kbytes_written;
        /// <summary>0x17C, Active snapshot inode number</summary>
        public readonly uint snapshot_inum;
        /// <summary>0x180, Active snapshot sequential ID</summary>
        public readonly uint snapshot_id;
        /// <summary>0x184, Reserved blocks for active snapshot's future use</summary>
        public readonly ulong snapshot_blocks;
        /// <summary>0x18C, inode number of the on-disk start of the snapshot list</summary>
        public readonly uint snapshot_list;

        // Optional ext4 error-handling features
        /// <summary>0x190, total registered filesystem errors</summary>
        public readonly uint error_count;
        /// <summary>0x194, time on first error</summary>
        public readonly uint first_error_t;
        /// <summary>0x198, inode involved in first error</summary>
        public readonly uint first_error_inode;
        /// <summary>0x19C, block involved of first error</summary>
        public readonly ulong first_error_block;
        /// <summary>0x1A0, 32 bytes, function where the error happened</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] first_error_func;
        /// <summary>0x1B0, line number where error happened</summary>
        public readonly uint first_error_line;
        /// <summary>0x1B4, time of most recent error</summary>
        public readonly uint last_error_t;
        /// <summary>0x1B8, inode involved in last error</summary>
        public readonly uint last_error_inode;
        /// <summary>0x1BC, line number where error happened</summary>
        public readonly uint last_error_line;
        /// <summary>0x1C0, block involved of last error</summary>
        public readonly ulong last_error_block;
        /// <summary>0x1C8, 32 bytes, function where the error happened</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] last_error_func;

        // End of optional error-handling features

        // 0x1D8, 64 bytes, last used mount options</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] mount_options;

        /// <summary>Inode for user quota</summary>
        public readonly uint usr_quota_inum;
        /// <summary>Inode for group quota</summary>
        public readonly uint grp_quota_inum;
        /// <summary>Overhead clusters in volume</summary>
        public readonly uint overhead_clusters;
        /// <summary>Groups with sparse_super2 SBs</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly uint[] backup_bgs;
        /// <summary>Encryption algorithms in use</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] encrypt_algos;
        /// <summary>Salt used for string2key algorithm</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] encrypt_pw_salt;
        /// <summary>Inode number of lost+found</summary>
        public readonly uint lpf_inum;
        /// <summary>Inode number for tracking project quota</summary>
        public readonly uint prj_quota_inum;
        /// <summary>crc32c(uuid) if csum_seed is set</summary>
        public readonly uint checksum_seed;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 98)]
        public readonly byte[] reserved;
        /// <summary>crc32c(superblock)</summary>
        public readonly uint checksum;
    }
}