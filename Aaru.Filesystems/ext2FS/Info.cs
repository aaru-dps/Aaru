// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
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
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the Linux extended filesystem v2, v3 and v4</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed partial class ext2FS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        ulong sbSectorOff = SB_POS / imagePlugin.Info.SectorSize;
        uint  sbOff       = SB_POS % imagePlugin.Info.SectorSize;

        if(sbSectorOff + partition.Start >= partition.End)
            return false;

        int  sbSizeInBytes   = Marshal.SizeOf<SuperBlock>();
        uint sbSizeInSectors = (uint)(sbSizeInBytes / imagePlugin.Info.SectorSize);

        if(sbSizeInBytes % imagePlugin.Info.SectorSize > 0)
            sbSizeInSectors++;

        ErrorNumber errno =
            imagePlugin.ReadSectors(sbSectorOff + partition.Start, sbSizeInSectors, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        byte[] sb = new byte[sbSizeInBytes];

        if(sbOff + sbSizeInBytes > sbSector.Length)
            return false;

        Array.Copy(sbSector, sbOff, sb, 0, sbSizeInBytes);

        ushort magic = BitConverter.ToUInt16(sb, 0x038);

        return magic is EXT2_MAGIC or EXT2_MAGIC_OLD;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("iso-8859-15");
        information =   "";
        metadata    =   new FileSystem();

        var sb = new StringBuilder();

        bool newExt2 = false;
        bool ext3    = false;
        bool ext4    = false;

        int  sbSizeInBytes   = Marshal.SizeOf<SuperBlock>();
        uint sbSizeInSectors = (uint)(sbSizeInBytes / imagePlugin.Info.SectorSize);

        if(sbSizeInBytes % imagePlugin.Info.SectorSize > 0)
            sbSizeInSectors++;

        ulong sbSectorOff = SB_POS / imagePlugin.Info.SectorSize;
        uint  sbOff       = SB_POS % imagePlugin.Info.SectorSize;

        ErrorNumber errno =
            imagePlugin.ReadSectors(sbSectorOff + partition.Start, sbSizeInSectors, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return;

        byte[] sblock = new byte[sbSizeInBytes];
        Array.Copy(sbSector, sbOff, sblock, 0, sbSizeInBytes);
        SuperBlock supblk = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sblock);

        metadata = new FileSystem();

        switch(supblk.magic)
        {
            case EXT2_MAGIC_OLD:
                sb.AppendLine(Localization.ext2_old_filesystem);
                metadata.Type = FS_TYPE_EXT2;

                break;
            case EXT2_MAGIC:
                ext3 |= (supblk.ftr_compat   & EXT3_FEATURE_COMPAT_HAS_JOURNAL)   == EXT3_FEATURE_COMPAT_HAS_JOURNAL ||
                        (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_RECOVER)     == EXT3_FEATURE_INCOMPAT_RECOVER   ||
                        (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV;

                if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_HUGE_FILE)   == EXT4_FEATURE_RO_COMPAT_HUGE_FILE   ||
                   (supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_GDT_CSUM)    == EXT4_FEATURE_RO_COMPAT_GDT_CSUM    ||
                   (supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_DIR_NLINK)   == EXT4_FEATURE_RO_COMPAT_DIR_NLINK   ||
                   (supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE) == EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE ||
                   (supblk.ftr_incompat  & EXT4_FEATURE_INCOMPAT_64BIT)        == EXT4_FEATURE_INCOMPAT_64BIT        ||
                   (supblk.ftr_incompat  & EXT4_FEATURE_INCOMPAT_MMP)          == EXT4_FEATURE_INCOMPAT_MMP          ||
                   (supblk.ftr_incompat  & EXT4_FEATURE_INCOMPAT_FLEX_BG)      == EXT4_FEATURE_INCOMPAT_FLEX_BG      ||
                   (supblk.ftr_incompat  & EXT4_FEATURE_INCOMPAT_EA_INODE)     == EXT4_FEATURE_INCOMPAT_EA_INODE     ||
                   (supblk.ftr_incompat  & EXT4_FEATURE_INCOMPAT_DIRDATA)      == EXT4_FEATURE_INCOMPAT_DIRDATA)
                {
                    ext3 = false;
                    ext4 = true;
                }

                newExt2 |= !ext3 && !ext4;

                if(newExt2)
                {
                    sb.AppendLine(Localization.ext2_filesystem);
                    metadata.Type = FS_TYPE_EXT2;
                }

                if(ext3)
                {
                    sb.AppendLine(Localization.ext3_filesystem);
                    metadata.Type = FS_TYPE_EXT3;
                }

                if(ext4)
                {
                    sb.AppendLine(Localization.ext4_filesystem);
                    metadata.Type = FS_TYPE_EXT4;
                }

                break;
            default:
                information = Localization.Not_an_ext2_3_4_filesystem + Environment.NewLine;

                return;
        }

        string extOs = supblk.creator_os switch
        {
            EXT2_OS_FREEBSD => "FreeBSD",
            EXT2_OS_HURD    => "Hurd",
            EXT2_OS_LINUX   => "Linux",
            EXT2_OS_LITES   => "Lites",
            EXT2_OS_MASIX   => "MasIX",
            _               => string.Format(Localization.Unknown_OS_0, supblk.creator_os)
        };

        metadata.SystemIdentifier = extOs;

        if(supblk.mkfs_t > 0)
        {
            sb.AppendFormat(Localization.Volume_was_created_on_0_for_1,
                            DateHandlers.UnixUnsignedToDateTime(supblk.mkfs_t), extOs).AppendLine();

            metadata.CreationDate = DateHandlers.UnixUnsignedToDateTime(supblk.mkfs_t);
        }
        else
            sb.AppendFormat(Localization.Volume_was_created_for_0, extOs).AppendLine();

        byte[] tempBytes = new byte[8];
        ulong  blocks, reserved, free;

        if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_64BIT) == EXT4_FEATURE_INCOMPAT_64BIT)
        {
            byte[] tempLo = BitConverter.GetBytes(supblk.blocks);
            byte[] tempHi = BitConverter.GetBytes(supblk.blocks_hi);
            tempBytes[0] = tempLo[0];
            tempBytes[1] = tempLo[1];
            tempBytes[2] = tempLo[2];
            tempBytes[3] = tempLo[3];
            tempBytes[4] = tempHi[0];
            tempBytes[5] = tempHi[1];
            tempBytes[6] = tempHi[2];
            tempBytes[7] = tempHi[3];
            blocks       = BitConverter.ToUInt64(tempBytes, 0);

            tempLo       = BitConverter.GetBytes(supblk.reserved_blocks);
            tempHi       = BitConverter.GetBytes(supblk.reserved_blocks_hi);
            tempBytes[0] = tempLo[0];
            tempBytes[1] = tempLo[1];
            tempBytes[2] = tempLo[2];
            tempBytes[3] = tempLo[3];
            tempBytes[4] = tempHi[0];
            tempBytes[5] = tempHi[1];
            tempBytes[6] = tempHi[2];
            tempBytes[7] = tempHi[3];
            reserved     = BitConverter.ToUInt64(tempBytes, 0);

            tempLo       = BitConverter.GetBytes(supblk.free_blocks);
            tempHi       = BitConverter.GetBytes(supblk.free_blocks_hi);
            tempBytes[0] = tempLo[0];
            tempBytes[1] = tempLo[1];
            tempBytes[2] = tempLo[2];
            tempBytes[3] = tempLo[3];
            tempBytes[4] = tempHi[0];
            tempBytes[5] = tempHi[1];
            tempBytes[6] = tempHi[2];
            tempBytes[7] = tempHi[3];
            free         = BitConverter.ToUInt64(tempBytes, 0);
        }
        else
        {
            blocks   = supblk.blocks;
            reserved = supblk.reserved_blocks;
            free     = supblk.free_blocks;
        }

        if(supblk.block_size == 0) // Then it is 1024 bytes
            supblk.block_size = 1024;

        sb.AppendFormat(Localization.Volume_has_0_blocks_of_1_bytes_for_a_total_of_2_bytes, blocks,
                        1024 << (int)supblk.block_size, blocks * (ulong)(1024 << (int)supblk.block_size)).AppendLine();

        metadata.Clusters    = blocks;
        metadata.ClusterSize = (uint)(1024 << (int)supblk.block_size);

        if(supblk.mount_t > 0 ||
           supblk.mount_c > 0)
        {
            if(supblk.mount_t > 0)
                sb.AppendFormat(Localization.Last_mounted_on_0, DateHandlers.UnixUnsignedToDateTime(supblk.mount_t)).
                   AppendLine();

            if(supblk.max_mount_c != -1)
                sb.AppendFormat(Localization.Volume_has_been_mounted_0_times_of_a_maximum_of_1_mounts_before_checking,
                                supblk.mount_c, supblk.max_mount_c).AppendLine();
            else
                sb.
                    AppendFormat(Localization.Volume_has_been_mounted_0_times_with_no_maximum_no_of_mounts_before_checking,
                                 supblk.mount_c).AppendLine();

            if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.last_mount_dir, encoding)))
                sb.AppendFormat(Localization.Last_mounted_at_0,
                                StringHandlers.CToString(supblk.last_mount_dir, encoding)).AppendLine();

            if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.mount_options, encoding)))
                sb.AppendFormat(Localization.Last_used_mount_options_were_0,
                                StringHandlers.CToString(supblk.mount_options, encoding)).AppendLine();
        }
        else
        {
            sb.AppendLine(Localization.Volume_has_never_been_mounted);

            if(supblk.max_mount_c != -1)
                sb.AppendFormat(Localization.Volume_can_be_mounted_0_times_before_checking, supblk.max_mount_c).
                   AppendLine();
            else
                sb.AppendLine(Localization.Volume_has_no_maximum_no_of_mounts_before_checking);
        }

        if(supblk.check_t > 0)
            if(supblk.check_inv > 0)
                sb.AppendFormat(Localization.Last_checked_on_0_should_check_every_1_seconds,
                                DateHandlers.UnixUnsignedToDateTime(supblk.check_t), supblk.check_inv).AppendLine();
            else
                sb.AppendFormat(Localization.Last_checked_on_0, DateHandlers.UnixUnsignedToDateTime(supblk.check_t)).
                   AppendLine();
        else
        {
            if(supblk.check_inv > 0)
                sb.AppendFormat(Localization.Volume_has_never_been_checked_should_check_every_0_, supblk.check_inv).
                   AppendLine();
            else
                sb.AppendLine(Localization.Volume_has_never_been_checked);
        }

        if(supblk.write_t > 0)
        {
            sb.AppendFormat(Localization.Last_written_on_0, DateHandlers.UnixUnsignedToDateTime(supblk.write_t)).
               AppendLine();

            metadata.ModificationDate = DateHandlers.UnixUnsignedToDateTime(supblk.write_t);
        }
        else
            sb.AppendLine("Volume has never been written");

        metadata.Dirty = true;

        switch(supblk.state)
        {
            case EXT2_VALID_FS:
                sb.AppendLine(Localization.Volume_is_clean);
                metadata.Dirty = false;

                break;
            case EXT2_ERROR_FS:
                sb.AppendLine(Localization.Volume_is_dirty);

                break;
            case EXT3_ORPHAN_FS:
                sb.AppendLine(Localization.Volume_is_recovering_orphan_files);

                break;
            default:
                sb.AppendFormat(Localization.Volume_is_in_an_unknown_state_0, supblk.state).AppendLine();

                break;
        }

        if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.volume_name, encoding)))
        {
            sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(supblk.volume_name, encoding)).
               AppendLine();

            metadata.VolumeName = StringHandlers.CToString(supblk.volume_name, encoding);
        }

        switch(supblk.err_behaviour)
        {
            case EXT2_ERRORS_CONTINUE:
                sb.AppendLine(Localization.On_errors_filesystem_should_continue);

                break;
            case EXT2_ERRORS_RO:
                sb.AppendLine(Localization.On_errors_filesystem_should_remount_read_only);

                break;
            case EXT2_ERRORS_PANIC:
                sb.AppendLine(Localization.On_errors_filesystem_should_panic);

                break;
            default:
                sb.AppendFormat(Localization.On_errors_filesystem_will_do_an_unknown_thing_0, supblk.err_behaviour).
                   AppendLine();

                break;
        }

        if(supblk.revision > 0)
            sb.AppendFormat(Localization.Filesystem_revision_0_1, supblk.revision, supblk.minor_revision).AppendLine();

        if(supblk.uuid != Guid.Empty)
        {
            sb.AppendFormat(Localization.Volume_UUID_0, supblk.uuid).AppendLine();
            metadata.VolumeSerial = supblk.uuid.ToString();
        }

        if(supblk.kbytes_written > 0)
            sb.AppendFormat(Localization._0_KiB_has_been_written_on_volume, supblk.kbytes_written).AppendLine();

        sb.AppendFormat(Localization._0_reserved_and_1_free_blocks, reserved, free).AppendLine();
        metadata.FreeClusters = free;

        sb.AppendFormat(Localization._0_inodes_with_1_free_inodes_2, supblk.inodes, supblk.free_inodes,
                        supblk.free_inodes * 100 / supblk.inodes).AppendLine();

        if(supblk.first_inode > 0)
            sb.AppendFormat(Localization.First_inode_is_0, supblk.first_inode).AppendLine();

        if(supblk.frag_size > 0)
            sb.AppendFormat(Localization._0_bytes_per_fragment, supblk.frag_size).AppendLine();

        if(supblk.blocks_per_grp > 0 &&
           supblk is { flags_per_grp: > 0, inodes_per_grp: > 0 })
            sb.AppendFormat(Localization._0_blocks_1_flags_and_2_inodes_per_group, supblk.blocks_per_grp,
                            supblk.flags_per_grp, supblk.inodes_per_grp).AppendLine();

        if(supblk.first_block > 0)
            sb.AppendFormat(Localization._0_is_first_data_block, supblk.first_block).AppendLine();

        sb.AppendFormat(Localization.Default_UID_0_GID_1, supblk.default_uid, supblk.default_gid).AppendLine();

        if(supblk.block_group_no > 0)
            sb.AppendFormat(Localization.Block_group_number_is_0, supblk.block_group_no).AppendLine();

        if(supblk.desc_grp_size > 0)
            sb.AppendFormat(Localization.Group_descriptor_size_is_0_bytes, supblk.desc_grp_size).AppendLine();

        if(supblk.first_meta_bg > 0)
            sb.AppendFormat(Localization.First_metablock_group_is_0, supblk.first_meta_bg).AppendLine();

        if(supblk.raid_stride > 0)
            sb.AppendFormat(Localization.RAID_stride_0, supblk.raid_stride).AppendLine();

        if(supblk.raid_stripe_width > 0)
            sb.AppendFormat(Localization._0_blocks_on_all_data_disks, supblk.raid_stripe_width).AppendLine();

        if(supblk is { mmp_interval: > 0, mmp_block: > 0 })
            sb.AppendFormat(Localization._0_seconds_for_multi_mount_protection_wait_on_block_1, supblk.mmp_interval,
                            supblk.mmp_block).AppendLine();

        if(supblk.flex_bg_grp_size > 0)
            sb.AppendFormat(Localization._0_Flexible_block_group_size, supblk.flex_bg_grp_size).AppendLine();

        if(supblk is { hash_seed_1: > 0, hash_seed_2: > 0 } and { hash_seed_3: > 0, hash_seed_4: > 0 })
            sb.AppendFormat(Localization.Hash_seed_0_1_2_3_version_4, supblk.hash_seed_1, supblk.hash_seed_2,
                            supblk.hash_seed_3, supblk.hash_seed_4, supblk.hash_version).AppendLine();

        if((supblk.ftr_compat   & EXT3_FEATURE_COMPAT_HAS_JOURNAL)   == EXT3_FEATURE_COMPAT_HAS_JOURNAL ||
           (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV)
        {
            sb.AppendLine(Localization.Volume_is_journaled);

            if(supblk.journal_uuid != Guid.Empty)
                sb.AppendFormat(Localization.Journal_UUID_0, supblk.journal_uuid).AppendLine();

            sb.AppendFormat(Localization.Journal_has_inode_0, supblk.journal_inode).AppendLine();

            if((supblk.ftr_compat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV &&
               supblk.journal_dev                                      > 0)
                sb.AppendFormat(Localization.Journal_is_on_device_0, supblk.journal_dev).AppendLine();

            if(supblk.jnl_backup_type > 0)
                sb.AppendFormat(Localization.Journal_backup_type_0, supblk.jnl_backup_type).AppendLine();

            if(supblk.last_orphan > 0)
                sb.AppendFormat(Localization.Last_orphaned_inode_is_0, supblk.last_orphan).AppendLine();
            else
                sb.AppendLine(Localization.There_are_no_orphaned_inodes);
        }

        if(ext4)
        {
            if(supblk.snapshot_id > 0)
                sb.
                    AppendFormat(Localization.Active_snapshot_has_ID_0_on_inode_1_with_2_blocks_reserved_list_starting_on_block_3,
                                 supblk.snapshot_id, supblk.snapshot_inum, supblk.snapshot_blocks,
                                 supblk.snapshot_list).AppendLine();

            if(supblk.error_count > 0)
            {
                sb.AppendFormat(Localization._0_errors_registered, supblk.error_count).AppendLine();

                sb.AppendFormat(Localization.First_error_occurred_on_0_last_on_1,
                                DateHandlers.UnixUnsignedToDateTime(supblk.first_error_t),
                                DateHandlers.UnixUnsignedToDateTime(supblk.last_error_t)).AppendLine();

                sb.AppendFormat(Localization.First_error_inode_is_0_last_is_1, supblk.first_error_inode,
                                supblk.last_error_inode).AppendLine();

                sb.AppendFormat(Localization.First_error_block_is_0_last_is_1, supblk.first_error_block,
                                supblk.last_error_block).AppendLine();

                sb.AppendFormat(Localization.First_error_function_is_0_last_is_1, supblk.first_error_func,
                                supblk.last_error_func).AppendLine();
            }
        }

        sb.AppendFormat(Localization.Flags_ellipsis).AppendLine();

        if((supblk.flags & EXT2_FLAGS_SIGNED_HASH) == EXT2_FLAGS_SIGNED_HASH)
            sb.AppendLine(Localization.Signed_directory_hash_is_in_use);

        if((supblk.flags & EXT2_FLAGS_UNSIGNED_HASH) == EXT2_FLAGS_UNSIGNED_HASH)
            sb.AppendLine(Localization.Unsigned_directory_hash_is_in_use);

        if((supblk.flags & EXT2_FLAGS_TEST_FILESYS) == EXT2_FLAGS_TEST_FILESYS)
            sb.AppendLine(Localization.Volume_is_testing_development_code);

        if((supblk.flags & 0xFFFFFFF8) != 0)
            sb.AppendFormat(Localization.Unknown_set_flags_0, supblk.flags);

        sb.AppendLine();

        sb.AppendFormat(Localization.Default_mount_options).AppendLine();

        if((supblk.default_mnt_opts & EXT2_DEFM_DEBUG) == EXT2_DEFM_DEBUG)
            sb.AppendLine(Localization.debug_Enable_debugging_code);

        if((supblk.default_mnt_opts & EXT2_DEFM_BSDGROUPS) == EXT2_DEFM_BSDGROUPS)
            sb.AppendLine(Localization.bsdgroups_Emulate_BSD_behaviour_when_creating_new_files);

        if((supblk.default_mnt_opts & EXT2_DEFM_XATTR_USER) == EXT2_DEFM_XATTR_USER)
            sb.AppendLine(Localization.user_xattr_Enable_user_specified_extended_attributes);

        if((supblk.default_mnt_opts & EXT2_DEFM_ACL) == EXT2_DEFM_ACL)
            sb.AppendLine(Localization.acl_Enable_POSIX_ACLs);

        if((supblk.default_mnt_opts & EXT2_DEFM_UID16) == EXT2_DEFM_UID16)
            sb.AppendLine(Localization.uid16_Disable_32bit_UIDs_and_GIDs);

        if((supblk.default_mnt_opts & EXT3_DEFM_JMODE_DATA) == EXT3_DEFM_JMODE_DATA)
            sb.AppendLine(Localization.journal_data_Journal_data_and_metadata);

        if((supblk.default_mnt_opts & EXT3_DEFM_JMODE_ORDERED) == EXT3_DEFM_JMODE_ORDERED)
            sb.AppendLine(Localization.journal_data_ordered_Write_data_before_journaling_metadata);

        if((supblk.default_mnt_opts & EXT3_DEFM_JMODE_WBACK) == EXT3_DEFM_JMODE_WBACK)
            sb.AppendLine(Localization.journal_data_writeback_Write_journal_before_data);

        if((supblk.default_mnt_opts & 0xFFFFFE20) != 0)
            sb.AppendFormat(Localization.Unknown_set_default_mount_options_0, supblk.default_mnt_opts);

        sb.AppendLine();

        sb.AppendFormat(Localization.Compatible_features).AppendLine();

        if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_DIR_PREALLOC) == EXT2_FEATURE_COMPAT_DIR_PREALLOC)
            sb.AppendLine(Localization.Pre_allocate_directories);

        if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_IMAGIC_INODES) == EXT2_FEATURE_COMPAT_IMAGIC_INODES)
            sb.AppendLine(Localization.imagic_inodes__);

        if((supblk.ftr_compat & EXT3_FEATURE_COMPAT_HAS_JOURNAL) == EXT3_FEATURE_COMPAT_HAS_JOURNAL)
            sb.AppendLine(Localization.Has_journal_ext3);

        if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_EXT_ATTR) == EXT2_FEATURE_COMPAT_EXT_ATTR)
            sb.AppendLine(Localization.Has_extended_attribute_blocks);

        if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_RESIZE_INO) == EXT2_FEATURE_COMPAT_RESIZE_INO)
            sb.AppendLine(Localization.Has_online_filesystem_resize_reservations);

        if((supblk.ftr_compat & EXT2_FEATURE_COMPAT_DIR_INDEX) == EXT2_FEATURE_COMPAT_DIR_INDEX)
            sb.AppendLine(Localization.Can_use_hashed_indexes_on_directories);

        if((supblk.ftr_compat & 0xFFFFFFC0) != 0)
            sb.AppendFormat(Localization.Unknown_compatible_features_0, supblk.ftr_compat);

        sb.AppendLine();

        sb.AppendFormat(Localization.Compatible_features_if_read_only).AppendLine();

        if((supblk.ftr_ro_compat & EXT2_FEATURE_RO_COMPAT_SPARSE_SUPER) == EXT2_FEATURE_RO_COMPAT_SPARSE_SUPER)
            sb.AppendLine(Localization.Reduced_number_of_superblocks);

        if((supblk.ftr_ro_compat & EXT2_FEATURE_RO_COMPAT_LARGE_FILE) == EXT2_FEATURE_RO_COMPAT_LARGE_FILE)
            sb.AppendLine(Localization.Can_have_files_bigger_than_2GiB);

        if((supblk.ftr_ro_compat & EXT2_FEATURE_RO_COMPAT_BTREE_DIR) == EXT2_FEATURE_RO_COMPAT_BTREE_DIR)
            sb.AppendLine(Localization.Uses_B_Tree_for_directories);

        if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_HUGE_FILE) == EXT4_FEATURE_RO_COMPAT_HUGE_FILE)
            sb.AppendLine(Localization.Can_have_files_bigger_than_2TiB_ext4);

        if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_GDT_CSUM) == EXT4_FEATURE_RO_COMPAT_GDT_CSUM)
            sb.AppendLine(Localization.Group_descriptor_checksums_and_sparse_inode_table_ext4);

        if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_DIR_NLINK) == EXT4_FEATURE_RO_COMPAT_DIR_NLINK)
            sb.AppendLine(Localization.More_than_32000_directory_entries_ext4);

        if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE) == EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE)
            sb.AppendLine(Localization.Supports_nanosecond_timestamps_and_creation_time_ext4);

        if((supblk.ftr_ro_compat & 0xFFFFFF80) != 0)
            sb.AppendFormat(Localization.Unknown_read_only_compatible_features_0, supblk.ftr_ro_compat);

        sb.AppendLine();

        sb.AppendFormat(Localization.Incompatible_features).AppendLine();

        if((supblk.ftr_incompat & EXT2_FEATURE_INCOMPAT_COMPRESSION) == EXT2_FEATURE_INCOMPAT_COMPRESSION)
            sb.AppendLine(Localization.Uses_compression);

        if((supblk.ftr_incompat & EXT2_FEATURE_INCOMPAT_FILETYPE) == EXT2_FEATURE_INCOMPAT_FILETYPE)
            sb.AppendLine(Localization.Filetype_in_directory_entries);

        if((supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_RECOVER) == EXT3_FEATURE_INCOMPAT_RECOVER)
            sb.AppendLine(Localization.Journal_needs_recovery_ext3);

        if((supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV)
            sb.AppendLine(Localization.Has_journal_on_another_device_ext3);

        if((supblk.ftr_incompat & EXT2_FEATURE_INCOMPAT_META_BG) == EXT2_FEATURE_INCOMPAT_META_BG)
            sb.AppendLine(Localization.Reduced_block_group_backups);

        if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_EXTENTS) == EXT4_FEATURE_INCOMPAT_EXTENTS)
            sb.AppendLine(Localization.Volume_use_extents_ext4);

        if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_64BIT) == EXT4_FEATURE_INCOMPAT_64BIT)
            sb.AppendLine(Localization.Supports_volumes_bigger_than_2_32_blocks_ext4);

        if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_MMP) == EXT4_FEATURE_INCOMPAT_MMP)
            sb.AppendLine(Localization.Multi_mount_protection_ext4);

        if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_FLEX_BG) == EXT4_FEATURE_INCOMPAT_FLEX_BG)
            sb.AppendLine(Localization.Flexible_block_group_metadata_location_ext4);

        if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_EA_INODE) == EXT4_FEATURE_INCOMPAT_EA_INODE)
            sb.AppendLine(Localization.Extended_attributes_can_reside_in_inode_ext4);

        if((supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_DIRDATA) == EXT4_FEATURE_INCOMPAT_DIRDATA)
            sb.AppendLine(Localization.Data_can_reside_in_directory_entry_ext4);

        if((supblk.ftr_incompat & 0xFFFFF020) != 0)
            sb.AppendFormat(Localization.Unknown_incompatible_features_0, supblk.ftr_incompat);

        information = sb.ToString();
    }
}