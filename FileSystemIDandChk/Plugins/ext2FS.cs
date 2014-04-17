/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : ext2FS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies ext2, ext3 and ext4 filesystems and shows information.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Text;
using FileSystemIDandChk;

// Information from the Linux kernel
namespace FileSystemIDandChk.Plugins
{
    class ext2FS : Plugin
    {
        public ext2FS(PluginBase Core)
        {
            Name = "Linux extended Filesystem 2, 3 and 4";
            PluginUUID = new Guid("6AA91B88-150B-4A7B-AD56-F84FB2DF4184");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            byte[] sb_sector = imagePlugin.ReadSector(2 + partitionOffset);

            UInt16 magic = BitConverter.ToUInt16(sb_sector, 0x038);
			
            if (magic == ext2FSMagic || magic == ext2OldFSMagic)
                return true;
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();

            ext2FSSuperBlock supblk = new ext2FSSuperBlock();
            byte[] forstrings;
            bool new_ext2 = false;
            bool ext3 = false;
            bool ext4 = false;

            byte[] guid_a = new byte[16];
            byte[] guid_b = new byte[16];

            uint sb_size_in_sectors;

            if (imagePlugin.GetSectorSize() < 1024)
                sb_size_in_sectors = 1024 / imagePlugin.GetSectorSize();
            else
                sb_size_in_sectors = 1;

            if (sb_size_in_sectors == 0)
            {
                information = "Error calculating size in sectors of ext2/3/4 superblocks";
                return;
            }

            byte[] sb_sector = imagePlugin.ReadSectors(2 + partitionOffset, sb_size_in_sectors);
            supblk.inodes = BitConverter.ToUInt32(sb_sector, 0x000);
            supblk.blocks = BitConverter.ToUInt32(sb_sector, 0x004);
            supblk.reserved_blocks = BitConverter.ToUInt32(sb_sector, 0x008);
            supblk.free_blocks = BitConverter.ToUInt32(sb_sector, 0x00C);
            supblk.free_inodes = BitConverter.ToUInt32(sb_sector, 0x010);
            supblk.first_block = BitConverter.ToUInt32(sb_sector, 0x014);
            supblk.block_size = BitConverter.ToUInt32(sb_sector, 0x018);
            supblk.frag_size = BitConverter.ToInt32(sb_sector, 0x01C);
            supblk.blocks_per_grp = BitConverter.ToUInt32(sb_sector, 0x020);
            supblk.flags_per_grp = BitConverter.ToUInt32(sb_sector, 0x024);
            supblk.inodes_per_grp = BitConverter.ToUInt32(sb_sector, 0x028);
            supblk.mount_t = BitConverter.ToUInt32(sb_sector, 0x02C);
            supblk.write_t = BitConverter.ToUInt32(sb_sector, 0x030);
            supblk.mount_c = BitConverter.ToUInt16(sb_sector, 0x034);
            supblk.max_mount_c = BitConverter.ToInt16(sb_sector, 0x036);
            supblk.magic = BitConverter.ToUInt16(sb_sector, 0x038);
            supblk.state = BitConverter.ToUInt16(sb_sector, 0x03A);
            supblk.err_behaviour = BitConverter.ToUInt16(sb_sector, 0x03C);
            supblk.minor_revision = BitConverter.ToUInt16(sb_sector, 0x03E);
            supblk.check_t = BitConverter.ToUInt32(sb_sector, 0x040);
            supblk.check_inv = BitConverter.ToUInt32(sb_sector, 0x044);
            // From 0.5a onward
            supblk.creator_os = BitConverter.ToUInt32(sb_sector, 0x048);
            supblk.revision = BitConverter.ToUInt32(sb_sector, 0x04C);
            supblk.default_uid = BitConverter.ToUInt16(sb_sector, 0x050);
            supblk.default_gid = BitConverter.ToUInt16(sb_sector, 0x052);
            // From 0.5b onward
            supblk.first_inode = BitConverter.ToUInt32(sb_sector, 0x054);
            supblk.inode_size = BitConverter.ToUInt16(sb_sector, 0x058);
            supblk.block_group_no = BitConverter.ToUInt16(sb_sector, 0x05A);
            supblk.ftr_compat = BitConverter.ToUInt32(sb_sector, 0x05C);
            supblk.ftr_incompat = BitConverter.ToUInt32(sb_sector, 0x060);
            supblk.ftr_ro_compat = BitConverter.ToUInt32(sb_sector, 0x064);
            // Volume UUID
            Array.Copy(sb_sector, 0x068, guid_a, 0, 16);
            guid_b[0] = guid_a[3];
            guid_b[1] = guid_a[2];
            guid_b[2] = guid_a[1];
            guid_b[3] = guid_a[0];
            guid_b[4] = guid_a[5];
            guid_b[5] = guid_a[4];
            guid_b[6] = guid_a[7];
            guid_b[7] = guid_a[6];
            guid_b[8] = guid_a[8];
            guid_b[9] = guid_a[9];
            guid_b[10] = guid_a[10];
            guid_b[11] = guid_a[11];
            guid_b[12] = guid_a[12];
            guid_b[13] = guid_a[13];
            guid_b[14] = guid_a[14];
            guid_b[15] = guid_a[15];
            supblk.uuid = new Guid(guid_b);
            // End of volume UUID
            forstrings = new byte[16];
            Array.Copy(sb_sector, 0x078, forstrings, 0, 16);
            supblk.volume_name = StringHandlers.CToString(forstrings);
            forstrings = new byte[64];
            Array.Copy(sb_sector, 0x088, forstrings, 0, 64);
            supblk.last_mount_dir = StringHandlers.CToString(forstrings);
            supblk.algo_usage_bmp = BitConverter.ToUInt32(sb_sector, 0x0C8);
            supblk.prealloc_blks = sb_sector[0x0CC];
            supblk.prealloc_dir_blks = sb_sector[0x0CD];
            supblk.rsrvd_gdt_blocks = BitConverter.ToUInt16(sb_sector, 0x0CE);
            // ext3
            Array.Copy(sb_sector, 0x0D0, guid_a, 0, 16);
            guid_b[0] = guid_a[3];
            guid_b[1] = guid_a[2];
            guid_b[2] = guid_a[1];
            guid_b[3] = guid_a[0];
            guid_b[4] = guid_a[5];
            guid_b[5] = guid_a[4];
            guid_b[6] = guid_a[7];
            guid_b[7] = guid_a[6];
            guid_b[8] = guid_a[8];
            guid_b[9] = guid_a[9];
            guid_b[10] = guid_a[10];
            guid_b[11] = guid_a[11];
            guid_b[12] = guid_a[12];
            guid_b[13] = guid_a[13];
            guid_b[14] = guid_a[14];
            guid_b[15] = guid_a[15];
            supblk.journal_uuid = new Guid(guid_b);
            supblk.journal_inode = BitConverter.ToUInt32(sb_sector, 0x0E0);
            supblk.journal_dev = BitConverter.ToUInt32(sb_sector, 0x0E4);
            supblk.last_orphan = BitConverter.ToUInt32(sb_sector, 0x0E8);
            supblk.hash_seed_1 = BitConverter.ToUInt32(sb_sector, 0x0EC);
            supblk.hash_seed_2 = BitConverter.ToUInt32(sb_sector, 0x0F0);
            supblk.hash_seed_3 = BitConverter.ToUInt32(sb_sector, 0x0F4);
            supblk.hash_seed_4 = BitConverter.ToUInt32(sb_sector, 0x0F8);
            supblk.hash_version = sb_sector[0x0FC];
            supblk.jnl_backup_type = sb_sector[0x0FD];
            supblk.desc_grp_size = BitConverter.ToUInt16(sb_sector, 0x0FE);
            supblk.default_mnt_opts = BitConverter.ToUInt32(sb_sector, 0x100);
            supblk.first_meta_bg = BitConverter.ToUInt32(sb_sector, 0x104);
            // ext4
            supblk.mkfs_t = BitConverter.ToUInt32(sb_sector, 0x108);
            supblk.blocks_hi = BitConverter.ToUInt32(sb_sector, 0x14C);
            supblk.reserved_blocks_hi = BitConverter.ToUInt32(sb_sector, 0x150);
            supblk.free_blocks_hi = BitConverter.ToUInt32(sb_sector, 0x154);
            supblk.min_inode_size = BitConverter.ToUInt16(sb_sector, 0x158);
            supblk.rsv_inode_size = BitConverter.ToUInt16(sb_sector, 0x15A);
            supblk.flags = BitConverter.ToUInt32(sb_sector, 0x15C);
            supblk.raid_stride = BitConverter.ToUInt16(sb_sector, 0x160);
            supblk.mmp_interval = BitConverter.ToUInt16(sb_sector, 0x162);
            supblk.mmp_block = BitConverter.ToUInt64(sb_sector, 0x164);
            supblk.raid_stripe_width = BitConverter.ToUInt32(sb_sector, 0x16C);
            supblk.flex_bg_grp_size = sb_sector[0x170];
            supblk.kbytes_written = BitConverter.ToUInt64(sb_sector, 0x174);
            supblk.snapshot_inum = BitConverter.ToUInt32(sb_sector, 0x17C);
            supblk.snapshot_id = BitConverter.ToUInt32(sb_sector, 0x180);
            supblk.snapshot_blocks = BitConverter.ToUInt64(sb_sector, 0x184);
            supblk.snapshot_list = BitConverter.ToUInt32(sb_sector, 0x18C);
            supblk.error_count = BitConverter.ToUInt32(sb_sector, 0x190);
            supblk.first_error_t = BitConverter.ToUInt32(sb_sector, 0x194);
            supblk.first_error_inode = BitConverter.ToUInt32(sb_sector, 0x198);
            supblk.first_error_block = BitConverter.ToUInt64(sb_sector, 0x19C);
            forstrings = new byte[32];
            Array.Copy(sb_sector, 0x1A0, forstrings, 0, 32);
            supblk.first_error_func = StringHandlers.CToString(forstrings);
            supblk.first_error_line = BitConverter.ToUInt32(sb_sector, 0x1B0);
            supblk.last_error_t = BitConverter.ToUInt32(sb_sector, 0x1B4);
            supblk.last_error_inode = BitConverter.ToUInt32(sb_sector, 0x1B8);
            supblk.last_error_line = BitConverter.ToUInt32(sb_sector, 0x1BC);
            supblk.last_error_block = BitConverter.ToUInt64(sb_sector, 0x1C0);
            forstrings = new byte[32];
            Array.Copy(sb_sector, 0x1C8, forstrings, 0, 32);
            supblk.last_error_func = StringHandlers.CToString(forstrings);
            forstrings = new byte[64];
            Array.Copy(sb_sector, 0x1D8, forstrings, 0, 64);
            supblk.mount_options = StringHandlers.CToString(forstrings);

            if (supblk.magic == ext2OldFSMagic)
            {
                sb.AppendLine("ext2 (old) filesystem");
            }
            else if (supblk.magic == ext2FSMagic)
            {
                ext3 |= (supblk.ftr_compat & EXT3_FEATURE_COMPAT_HAS_JOURNAL) == EXT3_FEATURE_COMPAT_HAS_JOURNAL || (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_RECOVER) == EXT3_FEATURE_INCOMPAT_RECOVER || (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV;

                if ((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_HUGE_FILE) == EXT4_FEATURE_RO_COMPAT_HUGE_FILE ||
                    (supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_GDT_CSUM) == EXT4_FEATURE_RO_COMPAT_GDT_CSUM ||
                    (supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_DIR_NLINK) == EXT4_FEATURE_RO_COMPAT_DIR_NLINK ||
                    (supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE) == EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE ||
                    (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_64BIT) == EXT4_FEATURE_INCOMPAT_64BIT ||
                    (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_MMP) == EXT4_FEATURE_INCOMPAT_MMP ||
                    (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_FLEX_BG) == EXT4_FEATURE_INCOMPAT_FLEX_BG ||
                    (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_EA_INODE) == EXT4_FEATURE_INCOMPAT_EA_INODE ||
                    (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_DIRDATA) == EXT4_FEATURE_INCOMPAT_DIRDATA)
                {
                    ext3 = false;
                    ext4 = true;
                }

                new_ext2 |= !ext3 && !ext4;

                if (new_ext2)
                    sb.AppendLine("ext2 filesystem");
                if (ext3)
                    sb.AppendLine("ext3 filesystem");
                if (ext4)
                    sb.AppendLine("ext4 filesystem");
            }
            else
            {
                information = "Not a ext2/3/4 filesystem" + Environment.NewLine;
                return;
            }

            string ext_os;
            switch (supblk.creator_os)
            {
                case EXT2_OS_FREEBSD:
                    ext_os = "FreeBSD";
                    break;
                case EXT2_OS_HURD:
                    ext_os = "Hurd";
                    break;
                case EXT2_OS_LINUX:
                    ext_os = "Linux";
                    break;
                case EXT2_OS_LITES:
                    ext_os = "Lites";
                    break;
                case EXT2_OS_MASIX:
                    ext_os = "MasIX";
                    break;
                default:
                    ext_os = string.Format("Unknown OS ({0})", supblk.creator_os);
                    break;
            }

            if (supblk.mkfs_t > 0)
                sb.AppendFormat("Volume was created on {0} for {1}", DateHandlers.UNIXUnsignedToDateTime(supblk.mkfs_t), ext_os).AppendLine();
            else
                sb.AppendFormat("Volume was created for {0}", ext_os).AppendLine();

            byte[] temp_lo, temp_hi;
            byte[] temp_bytes = new byte[8];
            UInt64 blocks, reserved, free;

            if ((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_64BIT) == EXT4_FEATURE_INCOMPAT_64BIT)
            {
                temp_lo = BitConverter.GetBytes(supblk.blocks);
                temp_hi = BitConverter.GetBytes(supblk.blocks_hi);
                temp_bytes[0] = temp_lo[0];
                temp_bytes[1] = temp_lo[1];
                temp_bytes[2] = temp_lo[2];
                temp_bytes[3] = temp_lo[3];
                temp_bytes[4] = temp_hi[0];
                temp_bytes[5] = temp_hi[1];
                temp_bytes[6] = temp_hi[2];
                temp_bytes[7] = temp_hi[3];
                blocks = BitConverter.ToUInt64(temp_bytes, 0);

                temp_lo = BitConverter.GetBytes(supblk.reserved_blocks);
                temp_hi = BitConverter.GetBytes(supblk.reserved_blocks_hi);
                temp_bytes[0] = temp_lo[0];
                temp_bytes[1] = temp_lo[1];
                temp_bytes[2] = temp_lo[2];
                temp_bytes[3] = temp_lo[3];
                temp_bytes[4] = temp_hi[0];
                temp_bytes[5] = temp_hi[1];
                temp_bytes[6] = temp_hi[2];
                temp_bytes[7] = temp_hi[3];
                reserved = BitConverter.ToUInt64(temp_bytes, 0);

                temp_lo = BitConverter.GetBytes(supblk.free_blocks);
                temp_hi = BitConverter.GetBytes(supblk.free_blocks_hi);
                temp_bytes[0] = temp_lo[0];
                temp_bytes[1] = temp_lo[1];
                temp_bytes[2] = temp_lo[2];
                temp_bytes[3] = temp_lo[3];
                temp_bytes[4] = temp_hi[0];
                temp_bytes[5] = temp_hi[1];
                temp_bytes[6] = temp_hi[2];
                temp_bytes[7] = temp_hi[3];
                free = BitConverter.ToUInt64(temp_bytes, 0);
            }
            else
            {
                blocks = supblk.blocks;
                reserved = supblk.reserved_blocks;
                free = supblk.free_blocks;
            }

            if (supblk.block_size == 0) // Then it is 1024 bytes
				supblk.block_size = 1024;

            sb.AppendFormat("Volume has {0} blocks of {1} bytes, for a total of {2} bytes", blocks, supblk.block_size, blocks * supblk.block_size).AppendLine();
            if (supblk.mount_t > 0 || supblk.mount_c > 0)
            {
                if (supblk.mount_t > 0)
                    sb.AppendFormat("Last mounted on {0}", DateHandlers.UNIXUnsignedToDateTime(supblk.mount_t)).AppendLine();
                if (supblk.max_mount_c != -1)
                    sb.AppendFormat("Volume has been mounted {0} times of a maximum of {1} mounts before checking", supblk.mount_c, supblk.max_mount_c).AppendLine();
                else
                    sb.AppendFormat("Volume has been mounted {0} times with no maximum no. of mounts before checking", supblk.mount_c).AppendLine();
                if (supblk.last_mount_dir != "")
                    sb.AppendFormat("Last mounted on: \"{0}\"", supblk.last_mount_dir).AppendLine();
                if (supblk.mount_options != "")
                    sb.AppendFormat("Last used mount options were: {0}", supblk.mount_options).AppendLine();
            }
            else
            {
                sb.AppendLine("Volume has never been mounted");
                if (supblk.max_mount_c != -1)
                    sb.AppendFormat("Volume can be mounted {0} times before checking", supblk.max_mount_c).AppendLine();
                else
                    sb.AppendLine("Volume has no maximum no. of mounts before checking");
            }

            if (supblk.check_t > 0)
            {
                if (supblk.check_inv > 0)
                    sb.AppendFormat("Last checked on {0} (should check every {1} seconds)", DateHandlers.UNIXUnsignedToDateTime(supblk.check_t), supblk.check_inv).AppendLine();
                else
                    sb.AppendFormat("Last checked on {0}", DateHandlers.UNIXUnsignedToDateTime(supblk.check_t)).AppendLine();
            }
            else
            {
                if (supblk.check_inv > 0)
                    sb.AppendFormat("Volume has never been checked (should check every {0})", supblk.check_inv).AppendLine();
                else
                    sb.AppendLine("Volume has never been checked");
            }

            if (supblk.write_t > 0)
                sb.AppendFormat("Last written on {0}", DateHandlers.UNIXUnsignedToDateTime(supblk.write_t)).AppendLine();
            else
                sb.AppendLine("Volume has never been written");

            switch (supblk.state)
            {
                case EXT2_VALID_FS:
                    sb.AppendLine("Volume is clean");
                    break;
                case EXT2_ERROR_FS:
                    sb.AppendLine("Volume is dirty");
                    break;
                case EXT3_ORPHAN_FS:
                    sb.AppendLine("Volume is recovering orphan files");
                    break;
                default:
                    sb.AppendFormat("Volume is in an unknown state ({0})", supblk.state).AppendLine();
                    break;
            }

            if (supblk.volume_name != "")
                sb.AppendFormat("Volume name: \"{0}\"", supblk.volume_name).AppendLine();

            switch (supblk.err_behaviour)
            {
                case EXT2_ERRORS_CONTINUE:
                    sb.AppendLine("On errors, filesystem should continue");
                    break;
                case EXT2_ERRORS_RO:
                    sb.AppendLine("On errors, filesystem should remount read-only");
                    break;
                case EXT2_ERRORS_PANIC:
                    sb.AppendLine("On errors, filesystem should panic");
                    break;
                default:
                    sb.AppendFormat("On errors filesystem will do an unknown thing ({0})", supblk.err_behaviour).AppendLine();
                    break;
            }

            if (supblk.revision > 0)
                sb.AppendFormat("Filesystem revision: {0}.{1}", supblk.revision, supblk.minor_revision).AppendLine();

            if (supblk.uuid != Guid.Empty)
                sb.AppendFormat("Volume UUID: {0}", supblk.uuid).AppendLine();

            if (supblk.kbytes_written > 0)
                sb.AppendFormat("{0} KiB has been written on volume", supblk.kbytes_written).AppendLine();

            sb.AppendFormat("{0} reserved and {1} free blocks", reserved, free).AppendLine();
            sb.AppendFormat("{0} inodes with {1} free inodes ({2}%)", supblk.inodes, supblk.free_inodes, supblk.free_inodes * 100 / supblk.inodes).AppendLine();
            if (supblk.first_inode > 0)
                sb.AppendFormat("First inode is {0}", supblk.first_inode).AppendLine();
            if (supblk.frag_size > 0)
                sb.AppendFormat("{0} bytes per fragment", supblk.frag_size).AppendLine();
            if (supblk.blocks_per_grp > 0 && supblk.flags_per_grp > 0 && supblk.inodes_per_grp > 0)
                sb.AppendFormat("{0} blocks, {1} flags and {2} inodes per group", supblk.blocks_per_grp, supblk.flags_per_grp, supblk.inodes_per_grp).AppendLine();
            if (supblk.first_block > 0)
                sb.AppendFormat("{0} is first data block", supblk.first_block).AppendLine();
            sb.AppendFormat("Default UID: {0}, GID: {1}", supblk.default_uid, supblk.default_gid).AppendLine();
            if (supblk.block_group_no > 0)
                sb.AppendFormat("Block group number is {0}", supblk.block_group_no).AppendLine();
            if (supblk.desc_grp_size > 0)
                sb.AppendFormat("Group descriptor size is {0} bytes", supblk.desc_grp_size).AppendLine();
            if (supblk.first_meta_bg > 0)
                sb.AppendFormat("First metablock group is {0}", supblk.first_meta_bg).AppendLine();
            if (supblk.raid_stride > 0)
                sb.AppendFormat("RAID stride: {0}", supblk.raid_stride).AppendLine();
            if (supblk.raid_stripe_width > 0)
                sb.AppendFormat("{0} blocks on all data disks", supblk.raid_stripe_width).AppendLine();
            if (supblk.mmp_interval > 0 && supblk.mmp_block > 0)
                sb.AppendFormat("{0} seconds for multi-mount protection wait, on block {1}", supblk.mmp_interval, supblk.mmp_block).AppendLine();
            if (supblk.flex_bg_grp_size > 0)
                sb.AppendFormat("{0} Flexible block group size", supblk.flex_bg_grp_size).AppendLine();
            if (supblk.hash_seed_1 > 0 && supblk.hash_seed_2 > 0 && supblk.hash_seed_3 > 0 && supblk.hash_seed_4 > 0)
                sb.AppendFormat("Hash seed: {0:X8}{1:X8}{2:X8}{3:X8}, version {4}", supblk.hash_seed_1, supblk.hash_seed_2, supblk.hash_seed_3, supblk.hash_seed_4, supblk.hash_version).AppendLine();

            if ((supblk.ftr_compat & EXT3_FEATURE_COMPAT_HAS_JOURNAL) == EXT3_FEATURE_COMPAT_HAS_JOURNAL ||
                (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV)
            {
                sb.AppendLine("Volume is journaled");
                if (supblk.journal_uuid != Guid.Empty)
                    sb.AppendFormat("Journal UUID: {0}", supblk.journal_uuid).AppendLine();
                sb.AppendFormat("Journal has inode {0}", supblk.journal_inode).AppendLine();
                if ((supblk.ftr_compat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV && supblk.journal_dev > 0)
                    sb.AppendFormat("Journal is on device {0}", supblk.journal_dev).AppendLine();
                if (supblk.jnl_backup_type > 0)
                    sb.AppendFormat("Journal backup type: {0}", supblk.jnl_backup_type).AppendLine();
                if (supblk.last_orphan > 0)
                    sb.AppendFormat("Last orphaned inode is {0}", supblk.last_orphan).AppendLine();
                else
                    sb.AppendLine("There are no orphaned inodes");
            }

            if (ext4)
            {
                if (supblk.snapshot_id > 0)
                    sb.AppendFormat("Active snapshot has ID {0}, on inode {1}, with {2} blocks reserved, list starting on block {3}", supblk.snapshot_id,
                        supblk.snapshot_inum, supblk.snapshot_blocks, supblk.snapshot_list).AppendLine();

                if (supblk.error_count > 0)
                {
                    sb.AppendFormat("{0} errors registered", supblk.error_count).AppendLine();
                    sb.AppendFormat("First error occurred on {0}, last on {1}", DateHandlers.UNIXUnsignedToDateTime(supblk.first_error_t), DateHandlers.UNIXUnsignedToDateTime(supblk.last_error_t)).AppendLine();
                    sb.AppendFormat("First error inode is {0}, last is {1}", supblk.first_error_inode, supblk.last_error_inode).AppendLine();
                    sb.AppendFormat("First error block is {0}, last is {1}", supblk.first_error_block, supblk.last_error_block).AppendLine();
                    sb.AppendFormat("First error function is \"{0}\", last is \"{1}\"", supblk.first_error_func, supblk.last_error_func).AppendLine();
                }
            }

            sb.AppendFormat("Flags…:").AppendLine();
            if ((supblk.flags & EXT2_FLAGS_SIGNED_HASH) == EXT2_FLAGS_SIGNED_HASH)
                sb.AppendLine("Signed directory hash is in use");
            if ((supblk.flags & EXT2_FLAGS_UNSIGNED_HASH) == EXT2_FLAGS_UNSIGNED_HASH)
                sb.AppendLine("Unsigned directory hash is in use");
            if ((supblk.flags & EXT2_FLAGS_TEST_FILESYS) == EXT2_FLAGS_TEST_FILESYS)
                sb.AppendLine("Volume is testing development code");
            if ((supblk.flags & 0xFFFFFFF8) != 0)
                sb.AppendFormat("Unknown set flags: {0:X8}", supblk.flags);

            sb.AppendLine();

            sb.AppendFormat("Default mount options…:").AppendLine();
            if ((supblk.default_mnt_opts & EXT2_DEFM_DEBUG) == EXT2_DEFM_DEBUG)
                sb.AppendLine("(debug): Enable debugging code");
            if ((supblk.default_mnt_opts & EXT2_DEFM_BSDGROUPS) == EXT2_DEFM_BSDGROUPS)
                sb.AppendLine("(bsdgroups): Emulate BSD behaviour when creating new files");
            if ((supblk.default_mnt_opts & EXT2_DEFM_XATTR_USER) == EXT2_DEFM_XATTR_USER)
                sb.AppendLine("(user_xattr): Enable user-specified extended attributes");
            if ((supblk.default_mnt_opts & EXT2_DEFM_ACL) == EXT2_DEFM_ACL)
                sb.AppendLine("(acl): Enable POSIX ACLs");
            if ((supblk.default_mnt_opts & EXT2_DEFM_UID16) == EXT2_DEFM_UID16)
                sb.AppendLine("(uid16): Disable 32bit UIDs and GIDs");
            if ((supblk.default_mnt_opts & EXT3_DEFM_JMODE_DATA) == EXT3_DEFM_JMODE_DATA)
                sb.AppendLine("(journal_data): Journal data and metadata");
            if ((supblk.default_mnt_opts & EXT3_DEFM_JMODE_ORDERED) == EXT3_DEFM_JMODE_ORDERED)
                sb.AppendLine("(journal_data_ordered): Write data before journaling metadata");
            if ((supblk.default_mnt_opts & EXT3_DEFM_JMODE_WBACK) == EXT3_DEFM_JMODE_WBACK)
                sb.AppendLine("(journal_data_writeback): Write journal before data");
            if ((supblk.default_mnt_opts & 0xFFFFFE20) != 0)
                sb.AppendFormat("Unknown set default mount options: {0:X8}", supblk.default_mnt_opts);

            sb.AppendLine();

            sb.AppendFormat("Compatible features…:").AppendLine();
            if ((supblk.ftr_compat & EXT2_FEATURE_COMPAT_DIR_PREALLOC) == EXT2_FEATURE_COMPAT_DIR_PREALLOC)
                sb.AppendLine("Pre-allocate directories");
            if ((supblk.ftr_compat & EXT2_FEATURE_COMPAT_IMAGIC_INODES) == EXT2_FEATURE_COMPAT_IMAGIC_INODES)
                sb.AppendLine("imagic inodes ?");
            if ((supblk.ftr_compat & EXT3_FEATURE_COMPAT_HAS_JOURNAL) == EXT3_FEATURE_COMPAT_HAS_JOURNAL)
                sb.AppendLine("Has journal (ext3)");
            if ((supblk.ftr_compat & EXT2_FEATURE_COMPAT_EXT_ATTR) == EXT2_FEATURE_COMPAT_EXT_ATTR)
                sb.AppendLine("Has extended attribute blocks");
            if ((supblk.ftr_compat & EXT2_FEATURE_COMPAT_RESIZE_INO) == EXT2_FEATURE_COMPAT_RESIZE_INO)
                sb.AppendLine("Has online filesystem resize reservations");
            if ((supblk.ftr_compat & EXT2_FEATURE_COMPAT_DIR_INDEX) == EXT2_FEATURE_COMPAT_DIR_INDEX)
                sb.AppendLine("Can use hashed indexes on directories");
            if ((supblk.ftr_compat & 0xFFFFFFC0) != 0)
                sb.AppendFormat("Unknown compatible features: {0:X8}", supblk.ftr_compat);

            sb.AppendLine();

            sb.AppendFormat("Compatible features if read-only…:").AppendLine();
            if ((supblk.ftr_ro_compat & EXT2_FEATURE_RO_COMPAT_SPARSE_SUPER) == EXT2_FEATURE_RO_COMPAT_SPARSE_SUPER)
                sb.AppendLine("Reduced number of superblocks");
            if ((supblk.ftr_ro_compat & EXT2_FEATURE_RO_COMPAT_LARGE_FILE) == EXT2_FEATURE_RO_COMPAT_LARGE_FILE)
                sb.AppendLine("Can have files bigger than 2GiB");
            if ((supblk.ftr_ro_compat & EXT2_FEATURE_RO_COMPAT_BTREE_DIR) == EXT2_FEATURE_RO_COMPAT_BTREE_DIR)
                sb.AppendLine("Uses B-Tree for directories");
            if ((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_HUGE_FILE) == EXT4_FEATURE_RO_COMPAT_HUGE_FILE)
                sb.AppendLine("Can have files bigger than 2TiB (ext4)");
            if ((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_GDT_CSUM) == EXT4_FEATURE_RO_COMPAT_GDT_CSUM)
                sb.AppendLine("Group descriptor checksums and sparse inode table (ext4)");
            if ((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_DIR_NLINK) == EXT4_FEATURE_RO_COMPAT_DIR_NLINK)
                sb.AppendLine("More than 32000 directory entries (ext4)");
            if ((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE) == EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE)
                sb.AppendLine("Supports nanosecond timestamps and creation time (ext4)");
            if ((supblk.ftr_ro_compat & 0xFFFFFF80) != 0)
                sb.AppendFormat("Unknown read-only compatible features: {0:X8}", supblk.ftr_ro_compat);

            sb.AppendLine();

            sb.AppendFormat("Incompatible features…:").AppendLine();
            if ((supblk.ftr_incompat & EXT2_FEATURE_INCOMPAT_COMPRESSION) == EXT2_FEATURE_INCOMPAT_COMPRESSION)
                sb.AppendLine("Uses compression");
            if ((supblk.ftr_incompat & EXT2_FEATURE_INCOMPAT_FILETYPE) == EXT2_FEATURE_INCOMPAT_FILETYPE)
                sb.AppendLine("Filetype in directory entries");
            if ((supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_RECOVER) == EXT3_FEATURE_INCOMPAT_RECOVER)
                sb.AppendLine("Journal needs recovery (ext3)");
            if ((supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV)
                sb.AppendLine("Has journal on another device (ext3)");
            if ((supblk.ftr_incompat & EXT2_FEATURE_INCOMPAT_META_BG) == EXT2_FEATURE_INCOMPAT_META_BG)
                sb.AppendLine("Reduced block group backups");
            if ((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_EXTENTS) == EXT4_FEATURE_INCOMPAT_EXTENTS)
                sb.AppendLine("Volume use extents (ext4)");
            if ((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_64BIT) == EXT4_FEATURE_INCOMPAT_64BIT)
                sb.AppendLine("Supports volumes bigger than 2^32 blocks (ext4)");
            if ((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_MMP) == EXT4_FEATURE_INCOMPAT_MMP)
                sb.AppendLine("Multi-mount protection (ext4)");
            if ((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_FLEX_BG) == EXT4_FEATURE_INCOMPAT_FLEX_BG)
                sb.AppendLine("Flexible block group metadata location (ext4)");
            if ((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_EA_INODE) == EXT4_FEATURE_INCOMPAT_EA_INODE)
                sb.AppendLine("Extended attributes can reside in inode (ext4)");
            if ((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_DIRDATA) == EXT4_FEATURE_INCOMPAT_DIRDATA)
                sb.AppendLine("Data can reside in directory entry (ext4)");
            if ((supblk.ftr_incompat & 0xFFFFF020) != 0)
                sb.AppendFormat("Unknown incompatible features: {0:X8}", supblk.ftr_incompat);

            information = sb.ToString();
        }

        public const UInt16 ext2FSMagic = 0xEF53;
        // Same for ext3 and ext4
        public const UInt16 ext2OldFSMagic = 0xEF51;
        // Size = 536 bytes
        public struct ext2FSSuperBlock
        {
            public UInt32 inodes;
            // 0x000, inodes on volume
            public UInt32 blocks;
            // 0x004, blocks on volume
            public UInt32 reserved_blocks;
            // 0x008, reserved blocks
            public UInt32 free_blocks;
            // 0x00C, free blocks count
            public UInt32 free_inodes;
            // 0x010, free inodes count
            public UInt32 first_block;
            // 0x014, first data block
            public UInt32 block_size;
            // 0x018, block size
            public Int32 frag_size;
            // 0x01C, fragment size
            public UInt32 blocks_per_grp;
            // 0x020, blocks per group
            public UInt32 flags_per_grp;
            // 0x024, fragments per group
            public UInt32 inodes_per_grp;
            // 0x028, inodes per group
            public UInt32 mount_t;
            // 0x02C, last mount time
            public UInt32 write_t;
            // 0x030, last write time
            public UInt16 mount_c;
            // 0x034, mounts count
            public Int16 max_mount_c;
            // 0x036, max mounts
            public UInt16 magic;
            // 0x038, (little endian)
            public UInt16 state;
            // 0x03A, filesystem state
            public UInt16 err_behaviour;
            // 0x03C, behaviour on errors
            public UInt16 minor_revision;
            // 0x03E, From 0.5b onward
            public UInt32 check_t;
            // 0x040, last check time
            public UInt32 check_inv;
            // 0x044, max time between checks
            // From 0.5a onward
            public UInt32 creator_os;
            // 0x048, Creation OS
            public UInt32 revision;
            // 0x04C, Revison level
            public UInt16 default_uid;
            // 0x050, Default UID for reserved blocks
            public UInt16 default_gid;
            // 0x052, Default GID for reserved blocks
            // From 0.5b onward
            public UInt32 first_inode;
            // 0x054, First unreserved inode
            public UInt16 inode_size;
            // 0x058, inode size
            public UInt16 block_group_no;
            // 0x05A, Block group number of THIS superblock
            public UInt32 ftr_compat;
            // 0x05C, Compatible features set
            public UInt32 ftr_incompat;
            // 0x060, Incompatible features set
            // Found on Linux 2.0.40
            public UInt32 ftr_ro_compat;
            // 0x064, Read-only compatible features set
            // Found on Linux 2.1.132
            public Guid uuid;
            // 0x068, 16 bytes, UUID
            public string volume_name;
            // 0x078, 16 bytes, volume name
            public string last_mount_dir;
            // 0x088, 64 bytes, where last mounted
            public UInt32 algo_usage_bmp;
            // 0x0C8, Usage bitmap algorithm, for compression
            public byte prealloc_blks;
            // 0x0CC, Block to try to preallocate
            public byte prealloc_dir_blks;
            // 0x0CD, Blocks to try to preallocate for directories
            public UInt16 rsrvd_gdt_blocks;
            // 0x0CE, Per-group desc for online growth
            // Found on Linux 2.4
            // ext3
            public Guid journal_uuid;
            // 0x0D0, 16 bytes, UUID of journal superblock
            public UInt32 journal_inode;
            // 0x0E0, inode no. of journal file
            public UInt32 journal_dev;
            // 0x0E4, device no. of journal file
            public UInt32 last_orphan;
            // 0x0E8, Start of list of inodes to delete
            public UInt32 hash_seed_1;
            // 0x0EC, First byte of 128bit HTREE hash seed
            public UInt32 hash_seed_2;
            // 0x0F0, Second byte of 128bit HTREE hash seed
            public UInt32 hash_seed_3;
            // 0x0F4, Third byte of 128bit HTREE hash seed
            public UInt32 hash_seed_4;
            // 0x0F8, Fourth byte of 128bit HTREE hash seed
            public byte hash_version;
            // 0x0FC, Hash version
            public byte jnl_backup_type;
            // 0x0FD, Journal backup type
            public UInt16 desc_grp_size;
            // 0x0FE, Size of group descriptor
            public UInt32 default_mnt_opts;
            // 0x100, Default mount options
            public UInt32 first_meta_bg;
            // 0x104, First metablock block group
            // Introduced with ext4, some can be ext3
            public UInt32 mkfs_t;
            // 0x108, Filesystem creation time
            // Follows 17 uint32 (68 bytes) of journal inode backup
            // Following 3 fields are valid if EXT4_FEATURE_COMPAT_64BIT is set
            public UInt32 blocks_hi;
            // 0x14C, High 32bits of blocks no.
            public UInt32 reserved_blocks_hi;
            // 0x150, High 32bits of reserved blocks no.
            public UInt32 free_blocks_hi;
            // 0x154, High 32bits of free blocks no.
            public UInt16 min_inode_size;
            // 0x158, inodes minimal size in bytes
            public UInt16 rsv_inode_size;
            // 0x15A, Bytes reserved by new inodes
            public UInt32 flags;
            // 0x15C, Flags
            public UInt16 raid_stride;
            // 0x160, RAID stride
            public UInt16 mmp_interval;
            // 0x162, Waiting seconds in MMP check
            public UInt64 mmp_block;
            // 0x164, Block for multi-mount protection
            public UInt32 raid_stripe_width;
            // 0x16C, Blocks on all data disks (N*stride)
            public byte flex_bg_grp_size;
            // 0x170, FLEX_BG group size
            public byte padding;
            // 0x171
            public UInt16 padding2;
            // 0x172
            // Following are introduced with ext4
            public UInt64 kbytes_written;
            // 0x174, Kibibytes written in volume lifetime
            public UInt32 snapshot_inum;
            // 0x17C, Active snapshot inode number
            public UInt32 snapshot_id;
            // 0x180, Active snapshot sequential ID
            public UInt64 snapshot_blocks;
            // 0x184, Reserved blocks for active snapshot's future use
            public UInt32 snapshot_list;
            // 0x18C, inode number of the on-disk start of the snapshot list
            // Optional ext4 error-handling features
            public UInt32 error_count;
            // 0x190, total registered filesystem errors
            public UInt32 first_error_t;
            // 0x194, time on first error
            public UInt32 first_error_inode;
            // 0x198, inode involved in first error
            public UInt64 first_error_block;
            // 0x19C, block involved of first error
            public string first_error_func;
            // 0x1A0, 32 bytes, function where the error happened
            public UInt32 first_error_line;
            // 0x1B0, line number where error happened
            public UInt32 last_error_t;
            // 0x1B4, time of most recent error
            public UInt32 last_error_inode;
            // 0x1B8, inode involved in last error
            public UInt32 last_error_line;
            // 0x1BC, line number where error happened
            public UInt64 last_error_block;
            // 0x1C0, block involved of last error
            public string last_error_func;
            // 0x1C8, 32 bytes, function where the error happened
            // End of optional error-handling features
            public string mount_options;
            // 0x1D8, 64 bytes, last used mount options
        }
        // ext? filesystem states
        public const UInt16 EXT2_VALID_FS = 0x0001;
        // Cleanly-unmounted volume
        public const UInt16 EXT2_ERROR_FS = 0x0002;
        // Dirty volume
        public const UInt16 EXT3_ORPHAN_FS = 0x0004;
        // Recovering orphan files
        // ext? default mount flags
        public const UInt32 EXT2_DEFM_DEBUG = 0x000001;
        // Enable debugging messages
        public const UInt32 EXT2_DEFM_BSDGROUPS = 0x000002;
        // Emulates BSD behaviour on new file creation
        public const UInt32 EXT2_DEFM_XATTR_USER = 0x000004;
        // Enable user xattrs
        public const UInt32 EXT2_DEFM_ACL = 0x000008;
        // Enable POSIX ACLs
        public const UInt32 EXT2_DEFM_UID16 = 0x000010;
        // Use 16bit UIDs
        public const UInt32 EXT3_DEFM_JMODE_DATA = 0x000040;
        // Journal data mode
        public const UInt32 EXT3_DEFM_JMODE_ORDERED = 0x000080;
        // Journal ordered mode
        public const UInt32 EXT3_DEFM_JMODE_WBACK = 0x000100;
        // Journal writeback mode
        // Behaviour on errors
        public const UInt16 EXT2_ERRORS_CONTINUE = 1;
        // Continue execution
        public const UInt16 EXT2_ERRORS_RO = 2;
        // Remount fs read-only
        public const UInt16 EXT2_ERRORS_PANIC = 3;
        // Panic
        // OS codes
        public const UInt32 EXT2_OS_LINUX = 0;
        public const UInt32 EXT2_OS_HURD = 1;
        public const UInt32 EXT2_OS_MASIX = 2;
        public const UInt32 EXT2_OS_FREEBSD = 3;
        public const UInt32 EXT2_OS_LITES = 4;
        // Revision levels
        public const UInt32 EXT2_GOOD_OLD_REV = 0;
        /* The good old (original) format */
        public const UInt32 EXT2_DYNAMIC_REV = 1;
        /* V2 format w/ dynamic inode sizes */
        // Compatible features
        public const UInt32 EXT2_FEATURE_COMPAT_DIR_PREALLOC = 0x00000001;
        // Pre-allocate directories
        public const UInt32 EXT2_FEATURE_COMPAT_IMAGIC_INODES = 0x00000002;
        // imagic inodes ?
        public const UInt32 EXT3_FEATURE_COMPAT_HAS_JOURNAL = 0x00000004;
        // Has journal (it's ext3)
        public const UInt32 EXT2_FEATURE_COMPAT_EXT_ATTR = 0x00000008;
        // EA blocks
        public const UInt32 EXT2_FEATURE_COMPAT_RESIZE_INO = 0x00000010;
        // Online filesystem resize reservations
        public const UInt32 EXT2_FEATURE_COMPAT_DIR_INDEX = 0x00000020;
        // Can use hashed indexes on directories
        // Read-only compatible features
        public const UInt32 EXT2_FEATURE_RO_COMPAT_SPARSE_SUPER = 0x00000001;
        // Reduced number of superblocks
        public const UInt32 EXT2_FEATURE_RO_COMPAT_LARGE_FILE = 0x00000002;
        // Can have files bigger than 2GiB
        public const UInt32 EXT2_FEATURE_RO_COMPAT_BTREE_DIR = 0x00000004;
        // Use B-Tree for directories
        public const UInt32 EXT4_FEATURE_RO_COMPAT_HUGE_FILE = 0x00000008;
        // Can have files bigger than 2TiB *ext4*
        public const UInt32 EXT4_FEATURE_RO_COMPAT_GDT_CSUM = 0x00000010;
        // Group descriptor checksums and sparse inode table *ext4*
        public const UInt32 EXT4_FEATURE_RO_COMPAT_DIR_NLINK = 0x00000020;
        // More than 32000 directory entries *ext4*
        public const UInt32 EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE = 0x00000040;
        // Nanosecond timestamps and creation time *ext4*
        // Incompatible features
        public const UInt32 EXT2_FEATURE_INCOMPAT_COMPRESSION = 0x00000001;
        // Uses compression
        public const UInt32 EXT2_FEATURE_INCOMPAT_FILETYPE = 0x00000002;
        // Filetype in directory entries
        public const UInt32 EXT3_FEATURE_INCOMPAT_RECOVER = 0x00000004;
        // Journal needs recovery *ext3*
        public const UInt32 EXT3_FEATURE_INCOMPAT_JOURNAL_DEV = 0x00000008;
        // Has journal on another device *ext3*
        public const UInt32 EXT2_FEATURE_INCOMPAT_META_BG = 0x00000010;
        // Reduced block group backups
        public const UInt32 EXT4_FEATURE_INCOMPAT_EXTENTS = 0x00000040;
        // Volume use extents *ext4*
        public const UInt32 EXT4_FEATURE_INCOMPAT_64BIT = 0x00000080;
        // Supports volumes bigger than 2^32 blocks *ext4*
        public const UInt32 EXT4_FEATURE_INCOMPAT_MMP = 0x00000100;
        // Multi-mount protection *ext4*
        public const UInt32 EXT4_FEATURE_INCOMPAT_FLEX_BG = 0x00000200;
        // Flexible block group metadata location *ext4*
        public const UInt32 EXT4_FEATURE_INCOMPAT_EA_INODE = 0x00000400;
        // EA in inode *ext4*
        public const UInt32 EXT4_FEATURE_INCOMPAT_DIRDATA = 0x00001000;
        // Data can reside in directory entry *ext4*
        // Miscellaneous filesystem flags
        public const UInt32 EXT2_FLAGS_SIGNED_HASH = 0x00000001;
        // Signed dirhash in use
        public const UInt32 EXT2_FLAGS_UNSIGNED_HASH = 0x00000002;
        // Unsigned dirhash in use
        public const UInt32 EXT2_FLAGS_TEST_FILESYS = 0x00000004;
        // Testing development code
    }
}