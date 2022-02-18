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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    // Information from the Linux kernel
    /// <inheritdoc />
    /// <summary>Implements detection of the Linux extended filesystem v2, v3 and v4</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
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
        public string Name => "Linux extended Filesystem 2, 3 and 4";
        /// <inheritdoc />
        public Guid Id => new Guid("6AA91B88-150B-4A7B-AD56-F84FB2DF4184");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

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

            byte[] sbSector = imagePlugin.ReadSectors(sbSectorOff + partition.Start, sbSizeInSectors);
            byte[] sb       = new byte[sbSizeInBytes];

            if(sbOff + sbSizeInBytes > sbSector.Length)
                return false;

            Array.Copy(sbSector, sbOff, sb, 0, sbSizeInBytes);

            ushort magic = BitConverter.ToUInt16(sb, 0x038);

            return magic == EXT2_MAGIC || magic == EXT2_MAGIC_OLD;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
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

            byte[] sbSector = imagePlugin.ReadSectors(sbSectorOff + partition.Start, sbSizeInSectors);
            byte[] sblock   = new byte[sbSizeInBytes];
            Array.Copy(sbSector, sbOff, sblock, 0, sbSizeInBytes);
            SuperBlock supblk = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sblock);

            XmlFsType = new FileSystemType();

            switch(supblk.magic)
            {
                case EXT2_MAGIC_OLD:
                    sb.AppendLine("ext2 (old) filesystem");
                    XmlFsType.Type = "ext2";

                    break;
                case EXT2_MAGIC:
                    ext3 |= (supblk.ftr_compat & EXT3_FEATURE_COMPAT_HAS_JOURNAL) == EXT3_FEATURE_COMPAT_HAS_JOURNAL ||
                            (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_RECOVER) == EXT3_FEATURE_INCOMPAT_RECOVER ||
                            (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) ==
                            EXT3_FEATURE_INCOMPAT_JOURNAL_DEV;

                    if((supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_HUGE_FILE) == EXT4_FEATURE_RO_COMPAT_HUGE_FILE ||
                       (supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_GDT_CSUM)  == EXT4_FEATURE_RO_COMPAT_GDT_CSUM  ||
                       (supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_DIR_NLINK) == EXT4_FEATURE_RO_COMPAT_DIR_NLINK ||
                       (supblk.ftr_ro_compat & EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE) ==
                       EXT4_FEATURE_RO_COMPAT_EXTRA_ISIZE                                                       ||
                       (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_64BIT)    == EXT4_FEATURE_INCOMPAT_64BIT    ||
                       (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_MMP)      == EXT4_FEATURE_INCOMPAT_MMP      ||
                       (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_FLEX_BG)  == EXT4_FEATURE_INCOMPAT_FLEX_BG  ||
                       (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_EA_INODE) == EXT4_FEATURE_INCOMPAT_EA_INODE ||
                       (supblk.ftr_incompat & EXT4_FEATURE_INCOMPAT_DIRDATA)  == EXT4_FEATURE_INCOMPAT_DIRDATA)
                    {
                        ext3 = false;
                        ext4 = true;
                    }

                    newExt2 |= !ext3 && !ext4;

                    if(newExt2)
                    {
                        sb.AppendLine("ext2 filesystem");
                        XmlFsType.Type = "ext2";
                    }

                    if(ext3)
                    {
                        sb.AppendLine("ext3 filesystem");
                        XmlFsType.Type = "ext3";
                    }

                    if(ext4)
                    {
                        sb.AppendLine("ext4 filesystem");
                        XmlFsType.Type = "ext4";
                    }

                    break;
                default:
                    information = "Not a ext2/3/4 filesystem" + Environment.NewLine;

                    return;
            }

            string extOs;

            switch(supblk.creator_os)
            {
                case EXT2_OS_FREEBSD:
                    extOs = "FreeBSD";

                    break;
                case EXT2_OS_HURD:
                    extOs = "Hurd";

                    break;
                case EXT2_OS_LINUX:
                    extOs = "Linux";

                    break;
                case EXT2_OS_LITES:
                    extOs = "Lites";

                    break;
                case EXT2_OS_MASIX:
                    extOs = "MasIX";

                    break;
                default:
                    extOs = $"Unknown OS ({supblk.creator_os})";

                    break;
            }

            XmlFsType.SystemIdentifier = extOs;

            if(supblk.mkfs_t > 0)
            {
                sb.AppendFormat("Volume was created on {0} for {1}", DateHandlers.UnixUnsignedToDateTime(supblk.mkfs_t),
                                extOs).AppendLine();

                XmlFsType.CreationDate          = DateHandlers.UnixUnsignedToDateTime(supblk.mkfs_t);
                XmlFsType.CreationDateSpecified = true;
            }
            else
                sb.AppendFormat("Volume was created for {0}", extOs).AppendLine();

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

            sb.AppendFormat("Volume has {0} blocks of {1} bytes, for a total of {2} bytes", blocks,
                            1024 << (int)supblk.block_size, blocks * (ulong)(1024 << (int)supblk.block_size)).
               AppendLine();

            XmlFsType.Clusters    = blocks;
            XmlFsType.ClusterSize = (uint)(1024 << (int)supblk.block_size);

            if(supblk.mount_t > 0 ||
               supblk.mount_c > 0)
            {
                if(supblk.mount_t > 0)
                    sb.AppendFormat("Last mounted on {0}", DateHandlers.UnixUnsignedToDateTime(supblk.mount_t)).
                       AppendLine();

                if(supblk.max_mount_c != -1)
                    sb.AppendFormat("Volume has been mounted {0} times of a maximum of {1} mounts before checking",
                                    supblk.mount_c, supblk.max_mount_c).AppendLine();
                else
                    sb.AppendFormat("Volume has been mounted {0} times with no maximum no. of mounts before checking",
                                    supblk.mount_c).AppendLine();

                if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.last_mount_dir, Encoding)))
                    sb.AppendFormat("Last mounted on: \"{0}\"",
                                    StringHandlers.CToString(supblk.last_mount_dir, Encoding)).AppendLine();

                if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.mount_options, Encoding)))
                    sb.AppendFormat("Last used mount options were: {0}",
                                    StringHandlers.CToString(supblk.mount_options, Encoding)).AppendLine();
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
                if(supblk.check_inv > 0)
                    sb.AppendFormat("Last checked on {0} (should check every {1} seconds)",
                                    DateHandlers.UnixUnsignedToDateTime(supblk.check_t), supblk.check_inv).AppendLine();
                else
                    sb.AppendFormat("Last checked on {0}", DateHandlers.UnixUnsignedToDateTime(supblk.check_t)).
                       AppendLine();
            else
            {
                if(supblk.check_inv > 0)
                    sb.AppendFormat("Volume has never been checked (should check every {0})", supblk.check_inv).
                       AppendLine();
                else
                    sb.AppendLine("Volume has never been checked");
            }

            if(supblk.write_t > 0)
            {
                sb.AppendFormat("Last written on {0}", DateHandlers.UnixUnsignedToDateTime(supblk.write_t)).
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
                    sb.AppendLine("Volume is clean");
                    XmlFsType.Dirty = false;

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

            if(!string.IsNullOrEmpty(StringHandlers.CToString(supblk.volume_name, Encoding)))
            {
                sb.AppendFormat("Volume name: \"{0}\"", StringHandlers.CToString(supblk.volume_name, Encoding)).
                   AppendLine();

                XmlFsType.VolumeName = StringHandlers.CToString(supblk.volume_name, Encoding);
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
                    sb.AppendFormat("On errors filesystem will do an unknown thing ({0})", supblk.err_behaviour).
                       AppendLine();

                    break;
            }

            if(supblk.revision > 0)
                sb.AppendFormat("Filesystem revision: {0}.{1}", supblk.revision, supblk.minor_revision).AppendLine();

            if(supblk.uuid != Guid.Empty)
            {
                sb.AppendFormat("Volume UUID: {0}", supblk.uuid).AppendLine();
                XmlFsType.VolumeSerial = supblk.uuid.ToString();
            }

            if(supblk.kbytes_written > 0)
                sb.AppendFormat("{0} KiB has been written on volume", supblk.kbytes_written).AppendLine();

            sb.AppendFormat("{0} reserved and {1} free blocks", reserved, free).AppendLine();
            XmlFsType.FreeClusters          = free;
            XmlFsType.FreeClustersSpecified = true;

            sb.AppendFormat("{0} inodes with {1} free inodes ({2}%)", supblk.inodes, supblk.free_inodes,
                            supblk.free_inodes * 100 / supblk.inodes).AppendLine();

            if(supblk.first_inode > 0)
                sb.AppendFormat("First inode is {0}", supblk.first_inode).AppendLine();

            if(supblk.frag_size > 0)
                sb.AppendFormat("{0} bytes per fragment", supblk.frag_size).AppendLine();

            if(supblk.blocks_per_grp > 0 &&
               supblk.flags_per_grp  > 0 &&
               supblk.inodes_per_grp > 0)
                sb.AppendFormat("{0} blocks, {1} flags and {2} inodes per group", supblk.blocks_per_grp,
                                supblk.flags_per_grp, supblk.inodes_per_grp).AppendLine();

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

            if(supblk.mmp_interval > 0 &&
               supblk.mmp_block    > 0)
                sb.AppendFormat("{0} seconds for multi-mount protection wait, on block {1}", supblk.mmp_interval,
                                supblk.mmp_block).AppendLine();

            if(supblk.flex_bg_grp_size > 0)
                sb.AppendFormat("{0} Flexible block group size", supblk.flex_bg_grp_size).AppendLine();

            if(supblk.hash_seed_1 > 0 &&
               supblk.hash_seed_2 > 0 &&
               supblk.hash_seed_3 > 0 &&
               supblk.hash_seed_4 > 0)
                sb.AppendFormat("Hash seed: {0:X8}{1:X8}{2:X8}{3:X8}, version {4}", supblk.hash_seed_1,
                                supblk.hash_seed_2, supblk.hash_seed_3, supblk.hash_seed_4, supblk.hash_version).
                   AppendLine();

            if((supblk.ftr_compat   & EXT3_FEATURE_COMPAT_HAS_JOURNAL)   == EXT3_FEATURE_COMPAT_HAS_JOURNAL ||
               (supblk.ftr_incompat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV)
            {
                sb.AppendLine("Volume is journaled");

                if(supblk.journal_uuid != Guid.Empty)
                    sb.AppendFormat("Journal UUID: {0}", supblk.journal_uuid).AppendLine();

                sb.AppendFormat("Journal has inode {0}", supblk.journal_inode).AppendLine();

                if((supblk.ftr_compat & EXT3_FEATURE_INCOMPAT_JOURNAL_DEV) == EXT3_FEATURE_INCOMPAT_JOURNAL_DEV &&
                   supblk.journal_dev                                      > 0)
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
                    sb.
                        AppendFormat("Active snapshot has ID {0}, on inode {1}, with {2} blocks reserved, list starting on block {3}",
                                     supblk.snapshot_id, supblk.snapshot_inum, supblk.snapshot_blocks,
                                     supblk.snapshot_list).AppendLine();

                if(supblk.error_count > 0)
                {
                    sb.AppendFormat("{0} errors registered", supblk.error_count).AppendLine();

                    sb.AppendFormat("First error occurred on {0}, last on {1}",
                                    DateHandlers.UnixUnsignedToDateTime(supblk.first_error_t),
                                    DateHandlers.UnixUnsignedToDateTime(supblk.last_error_t)).AppendLine();

                    sb.AppendFormat("First error inode is {0}, last is {1}", supblk.first_error_inode,
                                    supblk.last_error_inode).AppendLine();

                    sb.AppendFormat("First error block is {0}, last is {1}", supblk.first_error_block,
                                    supblk.last_error_block).AppendLine();

                    sb.AppendFormat("First error function is \"{0}\", last is \"{1}\"", supblk.first_error_func,
                                    supblk.last_error_func).AppendLine();
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
}