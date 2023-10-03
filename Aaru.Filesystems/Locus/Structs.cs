// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Locus filesystem plugin
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
//     License aint with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// Commit count

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using commitcnt_t = int;

// Disk address
using daddr_t = int;

// Fstore
using fstore_t = int;

// Global File System number
using gfs_t = int;

// Inode number
using ino_t = int;

// Filesystem pack number
using pckno_t = short;

// Timestamp
using time_t = int;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Locus filesystem</summary>
public sealed partial class Locus
{
#region Nested type: OldSuperblock

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct OldSuperblock
    {
        public readonly uint s_magic; /* identifies this as a locus filesystem */
        /* defined as a constant below */
        public readonly gfs_t   s_gfs;   /* global filesystem number */
        public readonly daddr_t s_fsize; /* size in blocks of entire volume */
        /* several ints for replicated filsystems */
        public readonly commitcnt_t s_lwm; /* all prior commits propagated */
        public readonly commitcnt_t s_hwm; /* highest commit propagated */
        /* oldest committed version in the list.
         * llst mod NCMTLST is the offset of commit #llst in the list,
         * which wraps around from there.
         */
        public readonly commitcnt_t s_llst;
        public readonly fstore_t s_fstore; /* filesystem storage bit mask; if the
                   filsys is replicated and this is not a
                   primary or backbone copy, this bit mask
                   determines which files are stored */

        public readonly time_t  s_time;  /* last super block update */
        public readonly daddr_t s_tfree; /* total free blocks*/

        public readonly ino_t   s_isize;   /* size in blocks of i-list */
        public readonly short   s_nfree;   /* number of addresses in s_free */
        public readonly Flags   s_flags;   /* filsys flags, defined below */
        public readonly ino_t   s_tinode;  /* total free inodes */
        public readonly ino_t   s_lasti;   /* start place for circular search */
        public readonly ino_t   s_nbehind; /* est # free inodes before s_lasti */
        public readonly pckno_t s_gfspack; /* global filesystem pack number */
        public readonly short   s_ninode;  /* number of i-nodes in s_inode */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly short[] s_dinfo; /* interleave stuff */

        //#define s_m s_dinfo[0]
        //#define s_skip  s_dinfo[0]      /* AIX defines  */
        //#define s_n s_dinfo[1]
        //#define s_cyl   s_dinfo[1]      /* AIX defines  */
        public readonly byte    s_flock;   /* lock during free list manipulation */
        public readonly byte    s_ilock;   /* lock during i-list manipulation */
        public readonly byte    s_fmod;    /* super block modified flag */
        public readonly Version s_version; /* version of the data format in fs. */
        /*  defined below. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] s_fsmnt; /* name of this file system */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] s_fpack; /* name of this physical volume */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = OLDNICINOD)]
        public readonly ino_t[] s_inode; /* free i-node list */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = OLDNICFREE)]
        public readonly daddr_t[] su_free; /* free block list for non-replicated filsys */
        public readonly byte s_byteorder;  /* byte order of integers */
    }

#endregion

#region Nested type: Superblock

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Superblock
    {
        public readonly uint s_magic; /* identifies this as a locus filesystem */
        /* defined as a constant below */
        public readonly gfs_t   s_gfs;   /* global filesystem number */
        public readonly daddr_t s_fsize; /* size in blocks of entire volume */
        /* several ints for replicated filesystems */
        public readonly commitcnt_t s_lwm; /* all prior commits propagated */
        public readonly commitcnt_t s_hwm; /* highest commit propagated */
        /* oldest committed version in the list.
         * llst mod NCMTLST is the offset of commit #llst in the list,
         * which wraps around from there.
         */
        public readonly commitcnt_t s_llst;
        public readonly fstore_t s_fstore; /* filesystem storage bit mask; if the
                   filsys is replicated and this is not a
                   primary or backbone copy, this bit mask
                   determines which files are stored */

        public readonly time_t  s_time;  /* last super block update */
        public readonly daddr_t s_tfree; /* total free blocks*/

        public readonly ino_t   s_isize;   /* size in blocks of i-list */
        public readonly short   s_nfree;   /* number of addresses in s_free */
        public          Flags   s_flags;   /* filsys flags, defined below */
        public readonly ino_t   s_tinode;  /* total free inodes */
        public readonly ino_t   s_lasti;   /* start place for circular search */
        public readonly ino_t   s_nbehind; /* est # free inodes before s_lasti */
        public readonly pckno_t s_gfspack; /* global filesystem pack number */
        public readonly short   s_ninode;  /* number of i-nodes in s_inode */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly short[] s_dinfo; /* interleave stuff */

        //#define s_m s_dinfo[0]
        //#define s_skip  s_dinfo[0]      /* AIX defines  */
        //#define s_n s_dinfo[1]
        //#define s_cyl   s_dinfo[1]      /* AIX defines  */
        public readonly byte    s_flock;   /* lock during free list manipulation */
        public readonly byte    s_ilock;   /* lock during i-list manipulation */
        public readonly byte    s_fmod;    /* super block modified flag */
        public readonly Version s_version; /* version of the data format in fs. */
        /*  defined below. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] s_fsmnt; /* name of this file system */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] s_fpack; /* name of this physical volume */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NICINOD)]
        public readonly ino_t[] s_inode; /* free i-node list */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NICFREE)]
        public readonly daddr_t[] su_free; /* free block list for non-replicated filsys */
        public readonly byte s_byteorder;  /* byte order of integers */
    }

#endregion
}