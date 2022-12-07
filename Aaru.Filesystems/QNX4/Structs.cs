// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : QNX4 filesystem plugin.
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

/// <inheritdoc />
/// <summary>Implements detection of QNX 4 filesystem</summary>
[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class QNX4
{
    struct Extent
    {
        public uint Block;
        public uint Length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Inode
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] di_fname;
        public readonly uint   di_size;
        public readonly Extent di_first_xtnt;
        public readonly uint   di_xblk;
        public readonly uint   di_ftime;
        public readonly uint   di_mtime;
        public readonly uint   di_atime;
        public readonly uint   di_ctime;
        public readonly ushort di_num_xtnts;
        public readonly ushort di_mode;
        public readonly ushort di_uid;
        public readonly ushort di_gid;
        public readonly ushort di_nlink;
        public readonly uint   di_zero;
        public readonly byte   di_type;
        public readonly byte   di_status;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct LinkInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public readonly byte[] dl_fname;
        public readonly uint dl_inode_blk;
        public readonly byte dl_inode_ndx;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] dl_spare;
        public readonly byte dl_status;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ExtentBlock
    {
        public readonly uint next_xblk;
        public readonly uint prev_xblk;
        public readonly byte num_xtnts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] spare;
        public readonly uint num_blocks;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public readonly Extent[] xtnts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] signature;
        public readonly Extent first_xtnt;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Superblock
    {
        public readonly Inode rootDir;
        public readonly Inode inode;
        public readonly Inode boot;
        public readonly Inode altBoot;
    }
}