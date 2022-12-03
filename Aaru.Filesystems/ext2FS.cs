// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the Linux extended filesystem v2, v3 and v4</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed class ext2FS : IFilesystem
{
    const int SB_POS = 0x400;

    /// <summary>Same magic for ext2, ext3 and ext4</summary>
    const ushort EXT2_MAGIC = 0xEF53;

    const ushort EXT2_MAGIC_OLD = 0xEF51;

    // ext? filesystem states
    /// <summary>Cleanly-unmounted volume</summary>
    const ushort EXT2_VALID_FS = 0x0001;
    /// <summary>Dirty volume</summary>
    const ushort EXT2_ERROR_FS = 0x0002;
    /// <summary>Recovering orphan files</summary>
    const ushort EXT3_ORPHAN_FS = 0x0004;

    // ext? default mount flags
    /// <summary>Enable debugging messages</summary>
    const uint EXT2_DEFM_DEBUG = 0x000001;
    /// <summary>Emulates BSD behaviour on new file creation</summary>
    const uint EXT2_DEFM_BSDGROUPS = 0x000002;
    /// <summary>Enable user xattrs</summary>
    const uint EXT2_DEFM_XATTR_USER = 0x000004;
    /// <summary>Enable POSIX ACLs</summary>
    const uint EXT2_DEFM_ACL = 0x000008;
    /// <summary>Use 16bit UIDs</summary>
    const uint EXT2_DEFM_UID16 = 0x000010;
    /// <summary>Journal data mode</summary>
    const uint EXT3_DEFM_JMODE_DATA = 0x000040;
    /// <summary>Journal ordered mode</summary>
    const uint EXT3_DEFM_JMODE_ORDERED = 0x000080;
    /// <summary>Journal writeback mode</summary>
    const uint EXT3_DEFM_JMODE_WBACK = 0x000100;

    // Behaviour on errors
    /// <summary>Continue execution</summary>
    const ushort EXT2_ERRORS_CONTINUE = 1;
    /// <summary>Remount fs read-only</summary>
    const ushort EXT2_ERRORS_RO = 2;
    /// <summary>Panic</summary>
    const ushort EXT2_ERRORS_PANIC = 3;

    // OS codes
    const uint EXT2_OS_LINUX   = 0;
    const uint EXT2_OS_HURD    = 1;
    const uint EXT2_OS_MASIX   = 2;
    const uint EXT2_OS_FREEBSD = 3;
    const uint EXT2_OS_LITES   = 4;

    // Revision levels
    /// <summary>The good old (original) format</summary>
    const uint EXT2_GOOD_OLD_REV = 0;
    /// <summary>V2 format w/ dynamic inode sizes</summary>
    const uint EXT2_DYNAMIC_REV = 1;

    // Compatible features
    /// <summary>Pre-allocate directories</summary>
    const uint EXT2_FEATURE_COMPAT_DIR_PREALLOC = 0x00000001;
    /// <summary>imagic inodes ?</summary>
    const uint EXT2_FEATURE_COMPAT_IMAGIC_INODES = 0x00000002;
    /// <summary>Has journal (it's ext3)</summary>
    const uint EXT3_FEATURE_COMPAT_HAS_JOURNAL = 0x00000004;
    /// <summary>EA blocks</summary>
    const uint EXT2_FEATURE_COMPAT_EXT_ATTR = 0x00000008;
    /// <summary>Online filesystem resize reservations</summary>
    const uint EXT2_FEATURE_COMPAT_RESIZE_INO = 0x00000010;
    /// <summary>Can use hashed indexes on directories</summary>
    const uint EXT2_FEATURE_COMPAT_DIR_INDEX = 0x00000020;

    // Read-only compatible features
    /// <summary>Reduced number of superblocks</summary>
    const uint EXT2_FEATURE_RO_COMPAT_SPARSE_SUPER = 0x00000001;
    /// <summary>Can have files bigger than 2GiB</summary>
    const uint EXT2_FEATURE_RO_COMPAT_LARGE_FILE = 0x00000002;
    /// <summary>Use B-Tree for directories</summary>
    const uint EXT2_FEATURE_RO_COMPAT_BTREE_DIR = 0x00000004;
    /// <summary>Can have files bigger than 2TiB *ext4*</summary>
    const uint EXT4_FEATURE_RO_COMPAT_HUGE_FILE = 0x00000008;
    /// <summary>Group descriptor checksums and sparse inode table *ext4*</summary>
    const uint EXT4_FEATURE_RO_COMPAT_GDT_CSUM = 0x00000010;
    /// <summary>More than 32000 directory entries *ext4*</summary>
    const uint EXT4_FEATURE_RO_COMPAT_DIR_NLINK = 0x00000020;
    /// <summary>Nanosecond timestamps and creation time *ext4*</summary>
    const uint EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE = 0x00000040;

    // Incompatible features
    /// <summary>Uses compression</summary>
    const uint EXT2_FEATURE_INCOMPAT_COMPRESSION = 0x00000001;
    /// <summary>Filetype in directory entries</summary>
    const uint EXT2_FEATURE_INCOMPAT_FILETYPE = 0x00000002;
    /// <summary>Journal needs recovery *ext3*</summary>
    const uint EXT3_FEATURE_INCOMPAT_RECOVER = 0x00000004;
    /// <summary>Has journal on another device *ext3*</summary>
    const uint EXT3_FEATURE_INCOMPAT_JOURNAL_DEV = 0x00000008;
    /// <summary>Reduced block group backups</summary>
    const uint EXT2_FEATURE_INCOMPAT_META_BG = 0x00000010;
    /// <summary>Volume use extents *ext4*</summary>
    const uint EXT4_FEATURE_INCOMPAT_EXTENTS = 0x00000040;
    /// <summary>Supports volumes bigger than 2^32 blocks *ext4*</summary>

    // ReSharper disable once InconsistentNaming
    const uint EXT4_FEATURE_INCOMPAT_64BIT = 0x00000080;
    /// <summary>Multi-mount protection *ext4*</summary>
    const uint EXT4_FEATURE_INCOMPAT_MMP = 0x00000100;
    /// <summary>Flexible block group metadata location *ext4*</summary>
    const uint EXT4_FEATURE_INCOMPAT_FLEX_BG = 0x00000200;
    /// <summary>EA in inode *ext4*</summary>
    const uint EXT4_FEATURE_INCOMPAT_EA_INODE = 0x00000400;
    /// <summary>Data can reside in directory entry *ext4*</summary>
    const uint EXT4_FEATURE_INCOMPAT_DIRDATA = 0x00001000;

    // Miscellaneous filesystem flags
    /// <summary>Signed dirhash in use</summary>
    const uint EXT2_FLAGS_SIGNED_HASH = 0x00000001;
    /// <summary>Unsigned dirhash in use</summary>
    const uint EXT2_FLAGS_UNSIGNED_HASH = 0x00000002;
    /// <summary>Testing development code</summary>
    const uint EXT2_FLAGS_TEST_FILESYS = 0x00000004;

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.ext2FS_Name_Linux_extended_Filesystem_2_3_and_4;
    /// <inheritdoc />
    public Guid Id => new("6AA91B88-150B-4A7B-AD56-F84FB2DF4184");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

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

    const string FS_TYPE_EXT2 = "ext2";
    const string FS_TYPE_EXT3 = "ext3";
    const string FS_TYPE_EXT4 = "ext4";

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

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

        XmlFsType = new FileSystemType();

        switch(supblk.magic)
        {
            case EXT2_MAGIC_OLD:
                sb.AppendLine(Localization.ext2_old_filesystem);
                XmlFsType.Type = FS_TYPE_EXT2;

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
                    XmlFsType.Type = FS_TYPE_EXT2;
                }

                if(ext3)
                {
                    sb.AppendLine(Localization.ext3_filesystem);
                    XmlFsType.Type = FS_TYPE_EXT3;
                }

                if(ext4)
                {
                    sb.AppendLine(Localization.ext4_filesystem);
                    XmlFsType.Type = FS_TYPE_EXT4;
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

        XmlFsType.SystemIdentifier = extOs;

        if(supblk.mkfs_t > 0)
        {
            sb.AppendFormat(Localization.Volume_was_created_on_0_for_1,
                            DateHandlers.UnixUnsignedToDateTime(supblk.mkfs_t), extOs).AppendLine();

            XmlFsType.CreationDate          = DateHandlers.UnixUnsignedToDateTime(supblk.mkfs_t);
            XmlFsType.CreationDateSpecified = true;
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

        XmlFsType.Clusters    = blocks;
        XmlFsType.ClusterSize = (uint)(1024 << (int)supblk.block_size);

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

            if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.last_mount_dir, Encoding)))
                sb.AppendFormat(Localization.Last_mounted_at_0,
                                StringHandlers.CToString(supblk.last_mount_dir, Encoding)).AppendLine();

            if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.mount_options, Encoding)))
                sb.AppendFormat(Localization.Last_used_mount_options_were_0,
                                StringHandlers.CToString(supblk.mount_options, Encoding)).AppendLine();
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

            XmlFsType.ModificationDate          = DateHandlers.UnixUnsignedToDateTime(supblk.write_t);
            XmlFsType.ModificationDateSpecified = true;
        }
        else
            sb.AppendLine("Volume has never been written");

        XmlFsType.Dirty = true;

        switch(supblk.state)
        {
            case EXT2_VALID_FS:
                sb.AppendLine(Localization.Volume_is_clean);
                XmlFsType.Dirty = false;

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

        if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.volume_name, Encoding)))
        {
            sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(supblk.volume_name, Encoding)).
               AppendLine();

            XmlFsType.VolumeName = StringHandlers.CToString(supblk.volume_name, Encoding);
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
            XmlFsType.VolumeSerial = supblk.uuid.ToString();
        }

        if(supblk.kbytes_written > 0)
            sb.AppendFormat(Localization._0_KiB_has_been_written_on_volume, supblk.kbytes_written).AppendLine();

        sb.AppendFormat(Localization._0_reserved_and_1_free_blocks, reserved, free).AppendLine();
        XmlFsType.FreeClusters          = free;
        XmlFsType.FreeClustersSpecified = true;

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