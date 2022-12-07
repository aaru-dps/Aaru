// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Xia filesystem plugin.
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection for the Xia filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local"), SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class Xia
{
    /// <summary>Xia superblock</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        /// <summary>1st sector reserved for boot</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public readonly byte[] s_boot_segment;
        /// <summary>the name says it</summary>
        public readonly uint s_zone_size;
        /// <summary>volume size, zone aligned</summary>
        public readonly uint s_nzones;
        /// <summary># of inodes</summary>
        public readonly uint s_ninodes;
        /// <summary># of data zones</summary>
        public readonly uint s_ndatazones;
        /// <summary># of imap zones</summary>
        public readonly uint s_imap_zones;
        /// <summary># of zmap zones</summary>
        public readonly uint s_zmap_zones;
        /// <summary>first data zone</summary>
        public readonly uint s_firstdatazone;
        /// <summary>z size = 1KB &lt;&lt; z shift</summary>
        public readonly uint s_zone_shift;
        /// <summary>max size of a single file</summary>
        public readonly uint s_max_size;
        /// <summary>reserved</summary>
        public readonly uint s_reserved0;
        /// <summary>reserved</summary>
        public readonly uint s_reserved1;
        /// <summary>reserved</summary>
        public readonly uint s_reserved2;
        /// <summary>reserved</summary>
        public readonly uint s_reserved3;
        /// <summary>first kernel zone</summary>
        public readonly uint s_firstkernzone;
        /// <summary>kernel size in zones</summary>
        public readonly uint s_kernzones;
        /// <summary>magic number for xiafs</summary>
        public readonly uint s_magic;
    }

    /// <summary>Xia directory entry</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryEntry
    {
        public readonly uint   d_ino;
        public readonly ushort d_rec_len;
        public readonly byte   d_name_len;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = XIAFS_NAME_LEN + 1)]
        public readonly byte[] d_name;
    }

    /// <summary>Xia inode</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Inode
    {
        public readonly ushort i_mode;
        public readonly ushort i_nlinks;
        public readonly ushort i_uid;
        public readonly ushort i_gid;
        public readonly uint   i_size;
        public readonly uint   i_ctime;
        public readonly uint   i_atime;
        public readonly uint   i_mtime;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = XIAFS_NUM_BLOCK_POINTERS)]
        public readonly uint[] i_zone;
    }
}