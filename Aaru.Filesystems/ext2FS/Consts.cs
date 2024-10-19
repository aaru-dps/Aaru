// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the Linux extended filesystem v2, v3 and v4</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed partial class ext2FS
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

    const string FS_TYPE_EXT2 = "ext2";
    const string FS_TYPE_EXT3 = "ext3";
    const string FS_TYPE_EXT4 = "ext4";
}