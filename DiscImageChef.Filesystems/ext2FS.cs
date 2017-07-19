// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ext2FS.cs
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    // Information from the Linux kernel
    public class ext2FS : Filesystem
    {
        public ext2FS()
        {
            Name = "Linux extended Filesystem 2, 3 and 4";
            PluginUUID = new Guid("6AA91B88-150B-4A7B-AD56-F84FB2DF4184");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public ext2FS(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Linux extended Filesystem 2, 3 and 4";
            PluginUUID = new Guid("6AA91B88-150B-4A7B-AD56-F84FB2DF4184");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if((2 + partition.PartitionStartSector) >= partition.PartitionEndSector)
                return false;

            byte[] sb_sector = imagePlugin.ReadSector(2 + partition.PartitionStartSector);

            ushort magic = BitConverter.ToUInt16(sb_sector, 0x038);

            if(magic == ext2FSMagic || magic == ext2OldFSMagic)
                return true;
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
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

            if(imagePlugin.GetSectorSize() < 1024)
                sb_size_in_sectors = 1024 / imagePlugin.GetSectorSize();
            else
                sb_size_in_sectors = 1;

            if(sb_size_in_sectors == 0)
            {
                information = "Error calculating size in sectors of ext2/3/4 superblocks";
                return;
            }

            byte[] sb_sector = imagePlugin.ReadSectors(2 + partition.PartitionStartSector, sb_size_in_sectors);
            IntPtr sbPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(sb_sector, 0, sbPtr, 512);
            supblk = (ext2FSSuperBlock)Marshal.PtrToStructure(sbPtr, typeof(ext2FSSuperBlock));
            Marshal.FreeHGlobal(sbPtr);

            xmlFSType = new Schemas.FileSystemType();

            if(supblk.magic == ext2OldFSMagic)
            {
                sb.AppendLine("ext2 (old) filesystem");
                xmlFSType.Type = "ext2";
            }
            else if(supblk.magic == ext2FSMagic)
            {
                ext3 |= (supblk.ftr_compat & EXT3_FEATURE_COMPAT_HAS_JOURNAL) == EXT3_FEATURE_COMPAT_HAS_JOURNAL || (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_RECOVER) == EXT3_FEATURE_INCOMPAT_RECOVER || (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV;

                if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_HUGE_FILE) == EXT4_FEATURE_RO_COMPAT_HUGE_FILE ||
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

                if(new_ext2)
                {
                    sb.AppendLine("ext2 filesystem");
                    xmlFSType.Type = "ext2";
                }
                if(ext3)
                {
                    sb.AppendLine("ext3 filesystem");
                    xmlFSType.Type = "ext3";
                }
                if(ext4)
                {
                    sb.AppendLine("ext4 filesystem");
                    xmlFSType.Type = "ext4";
                }
            }
            else
            {
                information = "Not a ext2/3/4 filesystem" + Environment.NewLine;
                return;
            }

            string ext_os;
            switch(supblk.creator_os)
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

            xmlFSType.SystemIdentifier = ext_os;

            if(supblk.mkfs_t > 0)
            {
                sb.AppendFormat("Volume was created on {0} for {1}", DateHandlers.UNIXUnsignedToDateTime(supblk.mkfs_t), ext_os).AppendLine();
                xmlFSType.CreationDate = DateHandlers.UNIXUnsignedToDateTime(supblk.mkfs_t);
                xmlFSType.CreationDateSpecified = true;
            }
            else
                sb.AppendFormat("Volume was created for {0}", ext_os).AppendLine();

            byte[] temp_lo, temp_hi;
            byte[] temp_bytes = new byte[8];
            ulong blocks, reserved, free;

            if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_64BIT) == EXT4_FEATURE_INCOMPAT_64BIT)
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

            if(supblk.block_size == 0) // Then it is 1024 bytes
                supblk.block_size = 1024;

            sb.AppendFormat("Volume has {0} blocks of {1} bytes, for a total of {2} bytes", blocks, 1024 << (int)supblk.block_size, blocks * (ulong)(1024 << (int)supblk.block_size)).AppendLine();
            xmlFSType.Clusters = (long)blocks;
            xmlFSType.ClusterSize = 1024 << (int)supblk.block_size;
            if(supblk.mount_t > 0 || supblk.mount_c > 0)
            {
                if(supblk.mount_t > 0)
                    sb.AppendFormat("Last mounted on {0}", DateHandlers.UNIXUnsignedToDateTime(supblk.mount_t)).AppendLine();
                if(supblk.max_mount_c != -1)
                    sb.AppendFormat("Volume has been mounted {0} times of a maximum of {1} mounts before checking", supblk.mount_c, supblk.max_mount_c).AppendLine();
                else
                    sb.AppendFormat("Volume has been mounted {0} times with no maximum no. of mounts before checking", supblk.mount_c).AppendLine();
                if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.last_mount_dir, CurrentEncoding)))
                    sb.AppendFormat("Last mounted on: \"{0}\"", StringHandlers.CToString(supblk.last_mount_dir, CurrentEncoding)).AppendLine();
                if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.mount_options, CurrentEncoding)))
                    sb.AppendFormat("Last used mount options were: {0}", StringHandlers.CToString(supblk.mount_options, CurrentEncoding)).AppendLine();
            }
            else
            {
                sb.AppendLine("Volume has never been mounted");
                if(supblk.max_mount_c != -1)
                    sb.AppendFormat("Volume can be mounted {0} times before checking", supblk.max_mount_c).AppendLine();
                else
                    sb.AppendLine("Volume has no maximum no. of mounts before checking");
            }

            if(supblk.check_t > 0)
            {
                if(supblk.check_inv > 0)
                    sb.AppendFormat("Last checked on {0} (should check every {1} seconds)", DateHandlers.UNIXUnsignedToDateTime(supblk.check_t), supblk.check_inv).AppendLine();
                else
                    sb.AppendFormat("Last checked on {0}", DateHandlers.UNIXUnsignedToDateTime(supblk.check_t)).AppendLine();
            }
            else
            {
                if(supblk.check_inv > 0)
                    sb.AppendFormat("Volume has never been checked (should check every {0})", supblk.check_inv).AppendLine();
                else
                    sb.AppendLine("Volume has never been checked");
            }

            if(supblk.write_t > 0)
            {
                sb.AppendFormat("Last written on {0}", DateHandlers.UNIXUnsignedToDateTime(supblk.write_t)).AppendLine();
                xmlFSType.ModificationDate = DateHandlers.UNIXUnsignedToDateTime(supblk.write_t);
                xmlFSType.ModificationDateSpecified = true;
            }
            else
                sb.AppendLine("Volume has never been written");

            xmlFSType.Dirty = true;
            switch(supblk.state)
            {
                case EXT2_VALID_FS:
                    sb.AppendLine("Volume is clean");
                    xmlFSType.Dirty = false;
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

            if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.volume_name, CurrentEncoding)))
            {
                sb.AppendFormat("Volume name: \"{0}\"", StringHandlers.CToString(supblk.volume_name, CurrentEncoding)).AppendLine();
                xmlFSType.VolumeName = StringHandlers.CToString(supblk.volume_name, CurrentEncoding);
            }

            switch(supblk.err_behaviour)
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

            if(supblk.revision > 0)
                sb.AppendFormat("Filesystem revision: {0}.{1}", supblk.revision, supblk.minor_revision).AppendLine();

            if(supblk.uuid != Guid.Empty)
            {
                sb.AppendFormat("Volume UUID: {0}", supblk.uuid).AppendLine();
                xmlFSType.VolumeSerial = supblk.uuid.ToString();
            }

            if(supblk.kbytes_written > 0)
                sb.AppendFormat("{0} KiB has been written on volume", supblk.kbytes_written).AppendLine();

            sb.AppendFormat("{0} reserved and {1} free blocks", reserved, free).AppendLine();
            xmlFSType.FreeClusters = (long)free;
            xmlFSType.FreeClustersSpecified = true;
            sb.AppendFormat("{0} inodes with {1} free inodes ({2}%)", supblk.inodes, supblk.free_inodes, supblk.free_inodes * 100 / supblk.inodes).AppendLine();
            if(supblk.first_inode > 0)
                sb.AppendFormat("First inode is {0}", supblk.first_inode).AppendLine();
            if(supblk.frag_size > 0)
                sb.AppendFormat("{0} bytes per fragment", supblk.frag_size).AppendLine();
            if(supblk.blocks_per_grp > 0 && supblk.flags_per_grp > 0 && supblk.inodes_per_grp > 0)
                sb.AppendFormat("{0} blocks, {1} flags and {2} inodes per group", supblk.blocks_per_grp, supblk.flags_per_grp, supblk.inodes_per_grp).AppendLine();
            if(supblk.first_block > 0)
                sb.AppendFormat("{0} is first data block", supblk.first_block).AppendLine();
            sb.AppendFormat("Default UID: {0}, GID: {1}", supblk.default_uid, supblk.default_gid).AppendLine();
            if(supblk.block_group_no > 0)
                sb.AppendFormat("Block group number is {0}", supblk.block_group_no).AppendLine();
            if(supblk.desc_grp_size > 0)
                sb.AppendFormat("Group descriptor size is {0} bytes", supblk.desc_grp_size).AppendLine();
            if(supblk.first_meta_bg > 0)
                sb.AppendFormat("First metablock group is {0}", supblk.first_meta_bg).AppendLine();
            if(supblk.raid_stride > 0)
                sb.AppendFormat("RAID stride: {0}", supblk.raid_stride).AppendLine();
            if(supblk.raid_stripe_width > 0)
                sb.AppendFormat("{0} blocks on all data disks", supblk.raid_stripe_width).AppendLine();
            if(supblk.mmp_interval > 0 && supblk.mmp_block > 0)
                sb.AppendFormat("{0} seconds for multi-mount protection wait, on block {1}", supblk.mmp_interval, supblk.mmp_block).AppendLine();
            if(supblk.flex_bg_grp_size > 0)
                sb.AppendFormat("{0} Flexible block group size", supblk.flex_bg_grp_size).AppendLine();
            if(supblk.hash_seed_1 > 0 && supblk.hash_seed_2 > 0 && supblk.hash_seed_3 > 0 && supblk.hash_seed_4 > 0)
                sb.AppendFormat("Hash seed: {0:X8}{1:X8}{2:X8}{3:X8}, version {4}", supblk.hash_seed_1, supblk.hash_seed_2, supblk.hash_seed_3, supblk.hash_seed_4, supblk.hash_version).AppendLine();

            if((supblk.ftr_compat & EXT3_FEATURE_COMPAT_HAS_JOURNAL) == EXT3_FEATURE_COMPAT_HAS_JOURNAL ||
                (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV)
            {
                sb.AppendLine("Volume is journaled");
                if(supblk.journal_uuid != Guid.Empty)
                    sb.AppendFormat("Journal UUID: {0}", supblk.journal_uuid).AppendLine();
                sb.AppendFormat("Journal has inode {0}", supblk.journal_inode).AppendLine();
                if((supblk.ftr_compat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV && supblk.journal_dev > 0)
                    sb.AppendFormat("Journal is on device {0}", supblk.journal_dev).AppendLine();
                if(supblk.jnl_backup_type > 0)
                    sb.AppendFormat("Journal backup type: {0}", supblk.jnl_backup_type).AppendLine();
                if(supblk.last_orphan > 0)
                    sb.AppendFormat("Last orphaned inode is {0}", supblk.last_orphan).AppendLine();
                else
                    sb.AppendLine("There are no orphaned inodes");
            }

            if(ext4)
            {
                if(supblk.snapshot_id > 0)
                    sb.AppendFormat("Active snapshot has ID {0}, on inode {1}, with {2} blocks reserved, list starting on block {3}", supblk.snapshot_id,
                        supblk.snapshot_inum, supblk.snapshot_blocks, supblk.snapshot_list).AppendLine();

                if(supblk.error_count > 0)
                {
                    sb.AppendFormat("{0} errors registered", supblk.error_count).AppendLine();
                    sb.AppendFormat("First error occurred on {0}, last on {1}", DateHandlers.UNIXUnsignedToDateTime(supblk.first_error_t), DateHandlers.UNIXUnsignedToDateTime(supblk.last_error_t)).AppendLine();
                    sb.AppendFormat("First error inode is {0}, last is {1}", supblk.first_error_inode, supblk.last_error_inode).AppendLine();
                    sb.AppendFormat("First error block is {0}, last is {1}", supblk.first_error_block, supblk.last_error_block).AppendLine();
                    sb.AppendFormat("First error function is \"{0}\", last is \"{1}\"", supblk.first_error_func, supblk.last_error_func).AppendLine();
                }
            }

            sb.AppendFormat("Flags…:").AppendLine();
            if((supblk.flags & EXT2_FLAGS_SIGNED_HASH) == EXT2_FLAGS_SIGNED_HASH)
                sb.AppendLine("Signed directory hash is in use");
            if((supblk.flags & EXT2_FLAGS_UNSIGNED_HASH) == EXT2_FLAGS_UNSIGNED_HASH)
                sb.AppendLine("Unsigned directory hash is in use");
            if((supblk.flags & EXT2_FLAGS_TEST_FILESYS) == EXT2_FLAGS_TEST_FILESYS)
                sb.AppendLine("Volume is testing development code");
            if((supblk.flags & 0xFFFFFFF8) != 0)
                sb.AppendFormat("Unknown set flags: {0:X8}", supblk.flags);

            sb.AppendLine();

            sb.AppendFormat("Default mount options…:").AppendLine();
            if((supblk.default_mnt_opts & EXT2_DEFM_DEBUG) == EXT2_DEFM_DEBUG)
                sb.AppendLine("(debug): Enable debugging code");
            if((supblk.default_mnt_opts & EXT2_DEFM_BSDGROUPS) == EXT2_DEFM_BSDGROUPS)
                sb.AppendLine("(bsdgroups): Emulate BSD behaviour when creating new files");
            if((supblk.default_mnt_opts & EXT2_DEFM_XATTR_USER) == EXT2_DEFM_XATTR_USER)
                sb.AppendLine("(user_xattr): Enable user-specified extended attributes");
            if((supblk.default_mnt_opts & EXT2_DEFM_ACL) == EXT2_DEFM_ACL)
                sb.AppendLine("(acl): Enable POSIX ACLs");
            if((supblk.default_mnt_opts & EXT2_DEFM_UID16) == EXT2_DEFM_UID16)
                sb.AppendLine("(uid16): Disable 32bit UIDs and GIDs");
            if((supblk.default_mnt_opts & EXT3_DEFM_JMODE_DATA) == EXT3_DEFM_JMODE_DATA)
                sb.AppendLine("(journal_data): Journal data and metadata");
            if((supblk.default_mnt_opts & EXT3_DEFM_JMODE_ORDERED) == EXT3_DEFM_JMODE_ORDERED)
                sb.AppendLine("(journal_data_ordered): Write data before journaling metadata");
            if((supblk.default_mnt_opts & EXT3_DEFM_JMODE_WBACK) == EXT3_DEFM_JMODE_WBACK)
                sb.AppendLine("(journal_data_writeback): Write journal before data");
            if((supblk.default_mnt_opts & 0xFFFFFE20) != 0)
                sb.AppendFormat("Unknown set default mount options: {0:X8}", supblk.default_mnt_opts);

            sb.AppendLine();

            sb.AppendFormat("Compatible features…:").AppendLine();
            if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_DIR_PREALLOC) == EXT2_FEATURE_COMPAT_DIR_PREALLOC)
                sb.AppendLine("Pre-allocate directories");
            if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_IMAGIC_INODES) == EXT2_FEATURE_COMPAT_IMAGIC_INODES)
                sb.AppendLine("imagic inodes ?");
            if((supblk.ftr_compat & EXT3_FEATURE_COMPAT_HAS_JOURNAL) == EXT3_FEATURE_COMPAT_HAS_JOURNAL)
                sb.AppendLine("Has journal (ext3)");
            if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_EXT_ATTR) == EXT2_FEATURE_COMPAT_EXT_ATTR)
                sb.AppendLine("Has extended attribute blocks");
            if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_RESIZE_INO) == EXT2_FEATURE_COMPAT_RESIZE_INO)
                sb.AppendLine("Has online filesystem resize reservations");
            if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_DIR_INDEX) == EXT2_FEATURE_COMPAT_DIR_INDEX)
                sb.AppendLine("Can use hashed indexes on directories");
            if((supblk.ftr_compat & 0xFFFFFFC0) != 0)
                sb.AppendFormat("Unknown compatible features: {0:X8}", supblk.ftr_compat);

            sb.AppendLine();

            sb.AppendFormat("Compatible features if read-only…:").AppendLine();
            if((supblk.ftr_ro_compat & EXT2_FEATURE_RO_COMPAT_SPARSE_SUPER) == EXT2_FEATURE_RO_COMPAT_SPARSE_SUPER)
                sb.AppendLine("Reduced number of superblocks");
            if((supblk.ftr_ro_compat & EXT2_FEATURE_RO_COMPAT_LARGE_FILE) == EXT2_FEATURE_RO_COMPAT_LARGE_FILE)
                sb.AppendLine("Can have files bigger than 2GiB");
            if((supblk.ftr_ro_compat & EXT2_FEATURE_RO_COMPAT_BTREE_DIR) == EXT2_FEATURE_RO_COMPAT_BTREE_DIR)
                sb.AppendLine("Uses B-Tree for directories");
            if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_HUGE_FILE) == EXT4_FEATURE_RO_COMPAT_HUGE_FILE)
                sb.AppendLine("Can have files bigger than 2TiB (ext4)");
            if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_GDT_CSUM) == EXT4_FEATURE_RO_COMPAT_GDT_CSUM)
                sb.AppendLine("Group descriptor checksums and sparse inode table (ext4)");
            if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_DIR_NLINK) == EXT4_FEATURE_RO_COMPAT_DIR_NLINK)
                sb.AppendLine("More than 32000 directory entries (ext4)");
            if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE) == EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE)
                sb.AppendLine("Supports nanosecond timestamps and creation time (ext4)");
            if((supblk.ftr_ro_compat & 0xFFFFFF80) != 0)
                sb.AppendFormat("Unknown read-only compatible features: {0:X8}", supblk.ftr_ro_compat);

            sb.AppendLine();

            sb.AppendFormat("Incompatible features…:").AppendLine();
            if((supblk.ftr_incompat & EXT2_FEATURE_INCOMPAT_COMPRESSION) == EXT2_FEATURE_INCOMPAT_COMPRESSION)
                sb.AppendLine("Uses compression");
            if((supblk.ftr_incompat & EXT2_FEATURE_INCOMPAT_FILETYPE) == EXT2_FEATURE_INCOMPAT_FILETYPE)
                sb.AppendLine("Filetype in directory entries");
            if((supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_RECOVER) == EXT3_FEATURE_INCOMPAT_RECOVER)
                sb.AppendLine("Journal needs recovery (ext3)");
            if((supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV)
                sb.AppendLine("Has journal on another device (ext3)");
            if((supblk.ftr_incompat & EXT2_FEATURE_INCOMPAT_META_BG) == EXT2_FEATURE_INCOMPAT_META_BG)
                sb.AppendLine("Reduced block group backups");
            if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_EXTENTS) == EXT4_FEATURE_INCOMPAT_EXTENTS)
                sb.AppendLine("Volume use extents (ext4)");
            if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_64BIT) == EXT4_FEATURE_INCOMPAT_64BIT)
                sb.AppendLine("Supports volumes bigger than 2^32 blocks (ext4)");
            if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_MMP) == EXT4_FEATURE_INCOMPAT_MMP)
                sb.AppendLine("Multi-mount protection (ext4)");
            if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_FLEX_BG) == EXT4_FEATURE_INCOMPAT_FLEX_BG)
                sb.AppendLine("Flexible block group metadata location (ext4)");
            if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_EA_INODE) == EXT4_FEATURE_INCOMPAT_EA_INODE)
                sb.AppendLine("Extended attributes can reside in inode (ext4)");
            if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_DIRDATA) == EXT4_FEATURE_INCOMPAT_DIRDATA)
                sb.AppendLine("Data can reside in directory entry (ext4)");
            if((supblk.ftr_incompat & 0xFFFFF020) != 0)
                sb.AppendFormat("Unknown incompatible features: {0:X8}", supblk.ftr_incompat);

            information = sb.ToString();
        }

        /// <summary>
        /// Same magic for ext2, ext3 and ext4
        /// </summary>
        public const ushort ext2FSMagic = 0xEF53;

        public const ushort ext2OldFSMagic = 0xEF51;

        /// <summary>
        /// ext2/3/4 superblock
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ext2FSSuperBlock
        {
            /// <summary>0x000, inodes on volume</summary>
            public uint inodes;
            /// <summary>0x004, blocks on volume</summary>
            public uint blocks;
            /// <summary>0x008, reserved blocks</summary>
            public uint reserved_blocks;
            /// <summary>0x00C, free blocks count</summary>
            public uint free_blocks;
            /// <summary>0x010, free inodes count</summary>
            public uint free_inodes;
            /// <summary>0x014, first data block</summary>
            public uint first_block;
            /// <summary>0x018, block size</summary>
            public uint block_size;
            /// <summary>0x01C, fragment size</summary>
            public int frag_size;
            /// <summary>0x020, blocks per group</summary>
            public uint blocks_per_grp;
            /// <summary>0x024, fragments per group</summary>
            public uint flags_per_grp;
            /// <summary>0x028, inodes per group</summary>
            public uint inodes_per_grp;
            /// <summary>0x02C, last mount time</summary>
            public uint mount_t;
            /// <summary>0x030, last write time</summary>
            public uint write_t;
            /// <summary>0x034, mounts count</summary>
            public ushort mount_c;
            /// <summary>0x036, max mounts</summary>
            public short max_mount_c;
            /// <summary>0x038, (little endian)</summary>
            public ushort magic;
            /// <summary>0x03A, filesystem state</summary>
            public ushort state;
            /// <summary>0x03C, behaviour on errors</summary>
            public ushort err_behaviour;
            /// <summary>0x03E, From 0.5b onward</summary>
            public ushort minor_revision;
            /// <summary>0x040, last check time</summary>
            public uint check_t;
            /// <summary>0x044, max time between checks</summary>
            public uint check_inv;

            // From 0.5a onward
            /// <summary>0x048, Creation OS</summary>
            public uint creator_os;
            /// <summary>0x04C, Revison level</summary>
            public uint revision;
            /// <summary>0x050, Default UID for reserved blocks</summary>
            public ushort default_uid;
            /// <summary>0x052, Default GID for reserved blocks</summary>
            public ushort default_gid;

            // From 0.5b onward
            /// <summary>0x054, First unreserved inode</summary>
            public uint first_inode;
            /// <summary>0x058, inode size</summary>
            public ushort inode_size;
            /// <summary>0x05A, Block group number of THIS superblock</summary>
            public ushort block_group_no;
            /// <summary>0x05C, Compatible features set</summary>
            public uint ftr_compat;
            /// <summary>0x060, Incompatible features set</summary>
            public uint ftr_incompat;

            // Found on Linux 2.0.40
            /// <summary>0x064, Read-only compatible features set</summary>
            public uint ftr_ro_compat;

            // Found on Linux 2.1.132
            /// <summary>0x068, 16 bytes, UUID</summary>
            public Guid uuid;
            /// <summary>0x078, 16 bytes, volume name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] volume_name;
            /// <summary>0x088, 64 bytes, where last mounted</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] last_mount_dir;
            /// <summary>0x0C8, Usage bitmap algorithm, for compression</summary>
            public uint algo_usage_bmp;
            /// <summary>0x0CC, Block to try to preallocate</summary>
            public byte prealloc_blks;
            /// <summary>0x0CD, Blocks to try to preallocate for directories</summary>
            public byte prealloc_dir_blks;
            /// <summary>0x0CE, Per-group desc for online growth</summary>
            public ushort rsrvd_gdt_blocks;

            // Found on Linux 2.4
            // ext3
            /// <summary>0x0D0, 16 bytes, UUID of journal superblock</summary>
            public Guid journal_uuid;
            /// <summary>0x0E0, inode no. of journal file</summary>
            public uint journal_inode;
            /// <summary>0x0E4, device no. of journal file</summary>
            public uint journal_dev;
            /// <summary>0x0E8, Start of list of inodes to delete</summary>
            public uint last_orphan;
            /// <summary>0x0EC, First byte of 128bit HTREE hash seed</summary>
            public uint hash_seed_1;
            /// <summary>0x0F0, Second byte of 128bit HTREE hash seed</summary>
            public uint hash_seed_2;
            /// <summary>0x0F4, Third byte of 128bit HTREE hash seed</summary>
            public uint hash_seed_3;
            /// <summary>0x0F8, Fourth byte of 128bit HTREE hash seed</summary>
            public uint hash_seed_4;
            /// <summary>0x0FC, Hash version</summary>
            public byte hash_version;
            /// <summary>0x0FD, Journal backup type</summary>
            public byte jnl_backup_type;
            /// <summary>0x0FE, Size of group descriptor</summary>
            public ushort desc_grp_size;
            /// <summary>0x100, Default mount options</summary>
            public uint default_mnt_opts;
            /// <summary>0x104, First metablock block group</summary>
            public uint first_meta_bg;

            // Introduced with ext4, some can be ext3
            /// <summary>0x108, Filesystem creation time</summary>
            public uint mkfs_t;

            /// <summary>Backup of the journal inode</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public uint[] jnl_blocks;

            // Following 3 fields are valid if EXT4_FEATURE_COMPAT_64BIT is set
            /// <summary>0x14C, High 32bits of blocks no.</summary>
            public uint blocks_hi;
            /// <summary>0x150, High 32bits of reserved blocks no.</summary>
            public uint reserved_blocks_hi;
            /// <summary>0x154, High 32bits of free blocks no.</summary>
            public uint free_blocks_hi;
            /// <summary>0x158, inodes minimal size in bytes</summary>
            public ushort min_inode_size;
            /// <summary>0x15A, Bytes reserved by new inodes</summary>
            public ushort rsv_inode_size;
            /// <summary>0x15C, Flags</summary>
            public uint flags;
            /// <summary>0x160, RAID stride</summary>
            public ushort raid_stride;
            /// <summary>0x162, Waiting seconds in MMP check</summary>
            public ushort mmp_interval;
            /// <summary>0x164, Block for multi-mount protection</summary>
            public ulong mmp_block;
            /// <summary>0x16C, Blocks on all data disks (N*stride)</summary>
            public uint raid_stripe_width;
            /// <summary>0x170, FLEX_BG group size</summary>
            public byte flex_bg_grp_size;
            /// <summary>0x171 Metadata checksum algorithm</summary>
            public byte checksum_type;
            /// <summary>0x172 Versioning level for encryption</summary>
            public byte encryption_level;
            /// <summary>0x173 Padding</summary>
            public ushort padding;
            // Following are introduced with ext4
            /// <summary>0x174, Kibibytes written in volume lifetime</summary>
            public ulong kbytes_written;
            /// <summary>0x17C, Active snapshot inode number</summary>
            public uint snapshot_inum;
            /// <summary>0x180, Active snapshot sequential ID</summary>
            public uint snapshot_id;
            /// <summary>0x184, Reserved blocks for active snapshot's future use</summary>
            public ulong snapshot_blocks;
            /// <summary>0x18C, inode number of the on-disk start of the snapshot list</summary>
            public uint snapshot_list;

            // Optional ext4 error-handling features
            /// <summary>0x190, total registered filesystem errors</summary>
            public uint error_count;
            /// <summary>0x194, time on first error</summary>
            public uint first_error_t;
            /// <summary>0x198, inode involved in first error</summary>
            public uint first_error_inode;
            /// <summary>0x19C, block involved of first error</summary>
            public ulong first_error_block;
            /// <summary>0x1A0, 32 bytes, function where the error happened</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] first_error_func;
            /// <summary>0x1B0, line number where error happened</summary>
            public uint first_error_line;
            /// <summary>0x1B4, time of most recent error</summary>
            public uint last_error_t;
            /// <summary>0x1B8, inode involved in last error</summary>
            public uint last_error_inode;
            /// <summary>0x1BC, line number where error happened</summary>
            public uint last_error_line;
            /// <summary>0x1C0, block involved of last error</summary>
            public ulong last_error_block;
            /// <summary>0x1C8, 32 bytes, function where the error happened</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] last_error_func;
            // End of optional error-handling features

            // 0x1D8, 64 bytes, last used mount options</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] mount_options;

            /// <summary>Inode for user quota</summary>
            public uint usr_quota_inum;
            /// <summary>Inode for group quota</summary>
            public uint grp_quota_inum;
            /// <summary>Overhead clusters in volume</summary>
            public uint overhead_clusters;
            /// <summary>Groups with sparse_super2 SBs</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] backup_bgs;
            /// <summary>Encryption algorithms in use</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] encrypt_algos;
            /// <summary>Salt used for string2key algorithm</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] encrypt_pw_salt;
            /// <summary>Inode number of lost+found</summary>
            public uint lpf_inum;
            /// <summary>Inode number for tracking project quota</summary>
            public uint prj_quota_inum;
            /// <summary>crc32c(uuid) if csum_seed is set</summary>
            public uint checksum_seed;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 98)]
            public byte[] reserved;
            /// <summary>crc32c(superblock)</summary>
            public uint checksum;
        }

        // ext? filesystem states
        /// <summary>Cleanly-unmounted volume</summary>
        public const ushort EXT2_VALID_FS = 0x0001;
        /// <summary>Dirty volume</summary>
        public const ushort EXT2_ERROR_FS = 0x0002;
        /// <summary>Recovering orphan files</summary>
        public const ushort EXT3_ORPHAN_FS = 0x0004;

        // ext? default mount flags
        /// <summary>Enable debugging messages</summary>
        public const uint EXT2_DEFM_DEBUG = 0x000001;
        /// <summary>Emulates BSD behaviour on new file creation</summary>
        public const uint EXT2_DEFM_BSDGROUPS = 0x000002;
        /// <summary>Enable user xattrs</summary>
        public const uint EXT2_DEFM_XATTR_USER = 0x000004;
        /// <summary>Enable POSIX ACLs</summary>
        public const uint EXT2_DEFM_ACL = 0x000008;
        /// <summary>Use 16bit UIDs</summary>
        public const uint EXT2_DEFM_UID16 = 0x000010;
        /// <summary>Journal data mode</summary>
        public const uint EXT3_DEFM_JMODE_DATA = 0x000040;
        /// <summary>Journal ordered mode</summary>
        public const uint EXT3_DEFM_JMODE_ORDERED = 0x000080;
        /// <summary>Journal writeback mode</summary>
        public const uint EXT3_DEFM_JMODE_WBACK = 0x000100;

        // Behaviour on errors
        /// <summary>Continue execution</summary>
        public const ushort EXT2_ERRORS_CONTINUE = 1;
        /// <summary>Remount fs read-only</summary>
        public const ushort EXT2_ERRORS_RO = 2;
        /// <summary>Panic</summary>
        public const ushort EXT2_ERRORS_PANIC = 3;

        // OS codes
        public const uint EXT2_OS_LINUX = 0;
        public const uint EXT2_OS_HURD = 1;
        public const uint EXT2_OS_MASIX = 2;
        public const uint EXT2_OS_FREEBSD = 3;
        public const uint EXT2_OS_LITES = 4;

        // Revision levels
        /// <summary>The good old (original) format</summary>
        public const uint EXT2_GOOD_OLD_REV = 0;
        /// <summary>V2 format w/ dynamic inode sizes</summary>
        public const uint EXT2_DYNAMIC_REV = 1;

        // Compatible features
        /// <summary>Pre-allocate directories</summary>
        public const uint EXT2_FEATURE_COMPAT_DIR_PREALLOC = 0x00000001;
        /// <summary>imagic inodes ?</summary>
        public const uint EXT2_FEATURE_COMPAT_IMAGIC_INODES = 0x00000002;
        /// <summary>Has journal (it's ext3)</summary>
        public const uint EXT3_FEATURE_COMPAT_HAS_JOURNAL = 0x00000004;
        /// <summary>EA blocks</summary>
        public const uint EXT2_FEATURE_COMPAT_EXT_ATTR = 0x00000008;
        /// <summary>Online filesystem resize reservations</summary>
        public const uint EXT2_FEATURE_COMPAT_RESIZE_INO = 0x00000010;
        /// <summary>Can use hashed indexes on directories</summary>
        public const uint EXT2_FEATURE_COMPAT_DIR_INDEX = 0x00000020;

        // Read-only compatible features
        /// <summary>Reduced number of superblocks</summary>
        public const uint EXT2_FEATURE_RO_COMPAT_SPARSE_SUPER = 0x00000001;
        /// <summary>Can have files bigger than 2GiB</summary>
        public const uint EXT2_FEATURE_RO_COMPAT_LARGE_FILE = 0x00000002;
        /// <summary>Use B-Tree for directories</summary>
        public const uint EXT2_FEATURE_RO_COMPAT_BTREE_DIR = 0x00000004;
        /// <summary>Can have files bigger than 2TiB *ext4*</summary>
        public const uint EXT4_FEATURE_RO_COMPAT_HUGE_FILE = 0x00000008;
        /// <summary>Group descriptor checksums and sparse inode table *ext4*</summary>
        public const uint EXT4_FEATURE_RO_COMPAT_GDT_CSUM = 0x00000010;
        /// <summary>More than 32000 directory entries *ext4*</summary>
        public const uint EXT4_FEATURE_RO_COMPAT_DIR_NLINK = 0x00000020;
        /// <summary>Nanosecond timestamps and creation time *ext4*</summary>
        public const uint EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE = 0x00000040;

        // Incompatible features
        /// <summary>Uses compression</summary>
        public const uint EXT2_FEATURE_INCOMPAT_COMPRESSION = 0x00000001;
        /// <summary>Filetype in directory entries</summary>
        public const uint EXT2_FEATURE_INCOMPAT_FILETYPE = 0x00000002;
        /// <summary>Journal needs recovery *ext3*</summary>
        public const uint EXT3_FEATURE_INCOMPAT_RECOVER = 0x00000004;
        /// <summary>Has journal on another device *ext3*</summary>
        public const uint EXT3_FEATURE_INCOMPAT_JOURNAL_DEV = 0x00000008;
        /// <summary>Reduced block group backups</summary>
        public const uint EXT2_FEATURE_INCOMPAT_META_BG = 0x00000010;
        /// <summary>Volume use extents *ext4*</summary>
        public const uint EXT4_FEATURE_INCOMPAT_EXTENTS = 0x00000040;
        /// <summary>Supports volumes bigger than 2^32 blocks *ext4*</summary>
        public const uint EXT4_FEATURE_INCOMPAT_64BIT = 0x00000080;
        /// <summary>Multi-mount protection *ext4*</summary>
        public const uint EXT4_FEATURE_INCOMPAT_MMP = 0x00000100;
        /// <summary>Flexible block group metadata location *ext4*</summary>
        public const uint EXT4_FEATURE_INCOMPAT_FLEX_BG = 0x00000200;
        /// <summary>EA in inode *ext4*</summary>
        public const uint EXT4_FEATURE_INCOMPAT_EA_INODE = 0x00000400;
        /// <summary>Data can reside in directory entry *ext4*</summary>
        public const uint EXT4_FEATURE_INCOMPAT_DIRDATA = 0x00001000;

        // Miscellaneous filesystem flags
        /// <summary>Signed dirhash in use</summary>
        public const uint EXT2_FLAGS_SIGNED_HASH = 0x00000001;
        /// <summary>Unsigned dirhash in use</summary>
        public const uint EXT2_FLAGS_UNSIGNED_HASH = 0x00000002;
        /// <summary>Testing development code</summary>
        public const uint EXT2_FLAGS_TEST_FILESYS = 0x00000004;

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}