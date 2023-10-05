// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : dump(8) file system plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies backups created with dump(8) shows information.
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
using ufs_daddr_t = int;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements identification of a dump(8) image (virtual filesystem on a file)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Dump
{
#region Nested type: DInode

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DInode
    {
        public readonly ushort di_mode;      /*   0: IFMT, permissions; see below. */
        public readonly short  di_nlink;     /*   2: File link count. */
        public readonly int    inumber;      /*   4: Lfs: inode number. */
        public readonly ulong  di_size;      /*   8: File byte count. */
        public readonly int    di_atime;     /*  16: Last access time. */
        public readonly int    di_atimensec; /*  20: Last access time. */
        public readonly int    di_mtime;     /*  24: Last modified time. */
        public readonly int    di_mtimensec; /*  28: Last modified time. */
        public readonly int    di_ctime;     /*  32: Last inode change time. */
        public readonly int    di_ctimensec; /*  36: Last inode change time. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NDADDR)]
        public readonly int[] di_db; /*  40: Direct disk blocks. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NIADDR)]
        public readonly int[] di_ib;    /*  88: Indirect disk blocks. */
        public readonly uint di_flags;  /* 100: Status flags (chflags). */
        public readonly uint di_blocks; /* 104: Blocks actually held. */
        public readonly int  di_gen;    /* 108: Generation number. */
        public readonly uint di_uid;    /* 112: File owner. */
        public readonly uint di_gid;    /* 116: File group. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly int[] di_spare; /* 120: Reserved; currently unused */
    }

#endregion

#region Nested type: s_spcl

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct s_spcl
    {
        public readonly int    c_type;     /* record type (see below) */
        public readonly int    c_date;     /* date of this dump */
        public readonly int    c_ddate;    /* date of previous dump */
        public readonly int    c_volume;   /* dump volume number */
        public readonly int    c_tapea;    /* logical block of this record */
        public readonly uint   c_inumber;  /* number of inode */
        public readonly int    c_magic;    /* magic number (see above) */
        public readonly int    c_checksum; /* record checksum */
        public readonly DInode c_dinode;   /* ownership and mode of inode */
        public readonly int    c_count;    /* number of valid c_addr entries */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = TP_NINDIR)]
        public readonly byte[] c_addr; /* 1 => data; 0 => hole in inode */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LBLSIZE)]
        public readonly byte[] c_label; /* dump label */
        public readonly int c_level;    /* level of this dump */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NAMELEN)]
        public readonly byte[] c_filesys; /* name of dumpped file system */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NAMELEN)]
        public readonly byte[] c_dev; /* name of dumpped device */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NAMELEN)]
        public readonly byte[] c_host;    /* name of dumpped host */
        public readonly int  c_flags;     /* additional information */
        public readonly int  c_firstrec;  /* first record on volume */
        public readonly long c_ndate;     /* date of this dump */
        public readonly long c_nddate;    /* date of previous dump */
        public readonly long c_ntapea;    /* logical block of this record */
        public readonly long c_nfirstrec; /* first record on volume */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly int[] c_spare; /* reserved for future uses */
    }

#endregion

#region Nested type: spcl_aix

    // 32-bit AIX format record
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct spcl_aix
    {
        /// <summary>Record type</summary>
        public readonly int c_type;
        /// <summary>Dump date</summary>
        public readonly int c_date;
        /// <summary>Previous dump date</summary>
        public readonly int c_ddate;
        /// <summary>Dump volume number</summary>
        public readonly int c_volume;
        /// <summary>Logical block of this record</summary>
        public readonly int c_tapea;
        public readonly uint c_inumber;
        public readonly uint c_magic;
        public readonly int  c_checksum;

        // Unneeded for now
        /*
        public bsd_dinode  bsd_c_dinode;
        public int c_count;
        public char c_addr[TP_NINDIR];
        public int xix_flag;
        public dinode xix_dinode;
        */
    }

#endregion

#region Nested type: spcl16

    // Old 16-bit format record
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct spcl16
    {
        /// <summary>Record type</summary>
        public readonly short c_type;
        /// <summary>Dump date</summary>
        public int c_date;
        /// <summary>Previous dump date</summary>
        public int c_ddate;
        /// <summary>Dump volume number</summary>
        public readonly short c_volume;
        /// <summary>Logical block of this record</summary>
        public readonly int c_tapea;
        /// <summary>Inode number</summary>
        public readonly ushort c_inumber;
        /// <summary>Magic number</summary>
        public readonly ushort c_magic;
        /// <summary>Record checksum</summary>
        public readonly int c_checksum;

        // Unneeded for now
        /*
        struct dinode  c_dinode;
        int c_count;
        char c_addr[BSIZE];
        */
    }

#endregion
}