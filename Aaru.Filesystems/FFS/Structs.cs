// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BSD Fast File System plugin.
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
using time_t = int;
using ufs_daddr_t = int;

namespace Aaru.Filesystems;

// Using information from Linux kernel headers
/// <inheritdoc />
/// <summary>Implements detection of BSD Fast File System (FFS, aka UNIX File System)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class FFSPlugin
{
#region Nested type: csum

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct csum
    {
        /// <summary>number of directories</summary>
        public int cs_ndir;
        /// <summary>number of free blocks</summary>
        public int cs_nbfree;
        /// <summary>number of free inodes</summary>
        public int cs_nifree;
        /// <summary>number of free frags</summary>
        public int cs_nffree;
    }

#endregion

#region Nested type: csum_total

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct csum_total
    {
        /// <summary>number of directories</summary>
        public long cs_ndir;
        /// <summary>number of free blocks</summary>
        public long cs_nbfree;
        /// <summary>number of free inodes</summary>
        public long cs_nifree;
        /// <summary>number of free frags</summary>
        public long cs_nffree;
        /// <summary>number of free clusters</summary>
        public long cs_numclusters;
        /// <summary>future expansion</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly long[] cs_spare;
    }

#endregion

#region Nested type: SuperBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SuperBlock
    {
        /// <summary>linked list of file systems</summary>
        public readonly uint fs_link;
        /// <summary>used for incore super blocks on Sun: uint fs_rolled; // logging only: fs fully rolled</summary>
        public readonly uint fs_rlink;
        /// <summary>addr of super-block in filesys</summary>
        public readonly int fs_sblkno;
        /// <summary>offset of cyl-block in filesys</summary>
        public readonly int fs_cblkno;
        /// <summary>offset of inode-blocks in filesys</summary>
        public readonly int fs_iblkno;
        /// <summary>offset of first data after cg</summary>
        public readonly int fs_dblkno;
        /// <summary>cylinder group offset in cylinder</summary>
        public readonly int fs_old_cgoffset;
        /// <summary>used to calc mod fs_ntrak</summary>
        public readonly int fs_old_cgmask;
        /// <summary>last time written</summary>
        public readonly int fs_old_time;
        /// <summary>number of blocks in fs</summary>
        public readonly int fs_old_size;
        /// <summary>number of data blocks in fs</summary>
        public readonly int fs_old_dsize;
        /// <summary>number of cylinder groups</summary>
        public readonly int fs_ncg;
        /// <summary>size of basic blocks in fs</summary>
        public readonly int fs_bsize;
        /// <summary>size of frag blocks in fs</summary>
        public readonly int fs_fsize;
        /// <summary>number of frags in a block in fs</summary>
        public readonly int fs_frag;
        /* these are configuration parameters */
        /// <summary>minimum percentage of free blocks</summary>
        public readonly int fs_minfree;
        /// <summary>num of ms for optimal next block</summary>
        public readonly int fs_old_rotdelay;
        /// <summary>disk revolutions per second</summary>
        public readonly int fs_old_rps;
        /* these fields can be computed from the others */
        /// <summary>``blkoff'' calc of blk offsets</summary>
        public readonly int fs_bmask;
        /// <summary>``fragoff'' calc of frag offsets</summary>
        public readonly int fs_fmask;
        /// <summary>``lblkno'' calc of logical blkno</summary>
        public readonly int fs_bshift;
        /// <summary>``numfrags'' calc number of frags</summary>
        public readonly int fs_fshift;
        /* these are configuration parameters */
        /// <summary>max number of contiguous blks</summary>
        public readonly int fs_maxcontig;
        /// <summary>max number of blks per cyl group</summary>
        public readonly int fs_maxbpg;
        /* these fields can be computed from the others */
        /// <summary>block to frag shift</summary>
        public readonly int fs_fragshift;
        /// <summary>fsbtodb and dbtofsb shift constant</summary>
        public readonly int fs_fsbtodb;
        /// <summary>actual size of super block</summary>
        public readonly int fs_sbsize;
        /// <summary>csum block offset</summary>
        public readonly int fs_csmask;
        /// <summary>csum block number</summary>
        public readonly int fs_csshift;
        /// <summary>value of NINDIR</summary>
        public readonly int fs_nindir;
        /// <summary>value of INOPB</summary>
        public readonly uint fs_inopb;
        /// <summary>value of NSPF</summary>
        public readonly int fs_old_nspf;
        /* yet another configuration parameter */
        /// <summary>optimization preference, see below On SVR: int fs_state; // file system state</summary>
        public readonly int fs_optim;
        /// <summary># sectors/track including spares</summary>
        public readonly int fs_old_npsect;
        /// <summary>hardware sector interleave</summary>
        public readonly int fs_old_interleave;
        /// <summary>sector 0 skew, per track On A/UX: int fs_state; // file system state</summary>
        public readonly int fs_old_trackskew;
        /// <summary>unique filesystem id On old: int fs_headswitch; // head switch time, usec</summary>
        public readonly int fs_id_1;
        /// <summary>unique filesystem id On old: int fs_trkseek; // track-to-track seek, usec</summary>
        public readonly int fs_id_2;
        /* sizes determined by number of cylinder groups and their sizes */
        /// <summary>blk addr of cyl grp summary area</summary>
        public readonly int fs_old_csaddr;
        /// <summary>size of cyl grp summary area</summary>
        public readonly int fs_cssize;
        /// <summary>cylinder group size</summary>
        public readonly int fs_cgsize;
        /* these fields are derived from the hardware */
        /// <summary>tracks per cylinder</summary>
        public readonly int fs_old_ntrak;
        /// <summary>sectors per track</summary>
        public readonly int fs_old_nsect;
        /// <summary>sectors per cylinder</summary>
        public readonly int fs_old_spc;
        /* this comes from the disk driver partitioning */
        /// <summary>cylinders in filesystem</summary>
        public readonly int fs_old_ncyl;
        /* these fields can be computed from the others */
        /// <summary>cylinders per group</summary>
        public readonly int fs_old_cpg;
        /// <summary>inodes per group</summary>
        public readonly int fs_ipg;
        /// <summary>blocks per group * fs_frag</summary>
        public readonly int fs_fpg;
        /* this data must be re-computed after crashes */
        /// <summary>cylinder summary information</summary>
        public csum fs_old_cstotal;
        /* these fields are cleared at mount time */
        /// <summary>super block modified flag</summary>
        public readonly sbyte fs_fmod;
        /// <summary>filesystem is clean flag</summary>
        public readonly sbyte fs_clean;
        /// <summary>mounted read-only flag</summary>
        public readonly sbyte fs_ronly;
        /// <summary>old FS_ flags</summary>
        public readonly sbyte fs_old_flags;
        /// <summary>name mounted on</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 468)]
        public readonly byte[] fs_fsmnt;
        /// <summary>volume name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] fs_volname;
        /// <summary>system-wide uid</summary>
        public readonly ulong fs_swuid;
        /// <summary>due to alignment of fs_swuid</summary>
        public readonly int fs_pad;
        /* these fields retain the current block allocation info */
        /// <summary>last cg searched</summary>
        public readonly int fs_cgrotor;
        /// <summary>padding; was list of fs_cs buffers</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public readonly uint[] fs_ocsp;
        /// <summary>(u) # of contig. allocated dirs</summary>
        public readonly uint fs_contigdirs;
        /// <summary>(u) cg summary info buffer</summary>
        public readonly uint fs_csp;
        /// <summary>(u) max cluster in each cyl group</summary>
        public readonly uint fs_maxcluster;
        /// <summary>(u) used by snapshots to track fs</summary>
        public readonly uint fs_active;
        /// <summary>cyl per cycle in postbl</summary>
        public readonly int fs_old_cpc;
        /// <summary>maximum blocking factor permitted</summary>
        public readonly int fs_maxbsize;
        /// <summary>number of unreferenced inodes</summary>
        public readonly long fs_unrefs;
        /// <summary>size of underlying GEOM provider</summary>
        public readonly long fs_providersize;
        /// <summary>size of area reserved for metadata</summary>
        public readonly long fs_metaspace;
        /// <summary>old rotation block list head</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public readonly long[] fs_sparecon64;
        /// <summary>byte offset of standard superblock</summary>
        public readonly long fs_sblockloc;
        /// <summary>(u) cylinder summary information</summary>
        public csum_total fs_cstotal;
        /// <summary>last time written</summary>
        public readonly long fs_time;
        /// <summary>number of blocks in fs</summary>
        public readonly long fs_size;
        /// <summary>number of data blocks in fs</summary>
        public readonly long fs_dsize;
        /// <summary>blk addr of cyl grp summary area</summary>
        public readonly long fs_csaddr;
        /// <summary>(u) blocks being freed</summary>
        public readonly long fs_pendingblocks;
        /// <summary>(u) inodes being freed</summary>
        public readonly uint fs_pendinginodes;
        /// <summary>list of snapshot inode numbers</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly uint[] fs_snapinum;
        /// <summary>expected average file size</summary>
        public readonly uint fs_avgfilesize;
        /// <summary>expected # of files per directory</summary>
        public readonly uint fs_avgfpdir;
        /// <summary>save real cg size to use fs_bsize</summary>
        public readonly int fs_save_cgsize;
        /// <summary>Last mount or fsck time.</summary>
        public readonly long fs_mtime;
        /// <summary>SUJ free list</summary>
        public readonly int fs_sujfree;
        /// <summary>reserved for future constants</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
        public readonly int[] fs_sparecon32;
        /// <summary>see FS_ flags below</summary>
        public readonly int fs_flags;
        /// <summary>size of cluster summary array</summary>
        public readonly int fs_contigsumsize;
        /// <summary>max length of an internal symlink</summary>
        public readonly int fs_maxsymlinklen;
        /// <summary>format of on-disk inodes</summary>
        public readonly int fs_old_inodefmt;
        /// <summary>maximum representable file size</summary>
        public readonly ulong fs_maxfilesize;
        /// <summary>~fs_bmask for use with 64-bit size</summary>
        public readonly long fs_qbmask;
        /// <summary>~fs_fmask for use with 64-bit size</summary>
        public readonly long fs_qfmask;
        /// <summary>validate fs_clean field</summary>
        public readonly int fs_state;
        /// <summary>format of positional layout tables</summary>
        public readonly int fs_old_postblformat;
        /// <summary>number of rotational positions</summary>
        public readonly int fs_old_nrpos;
        /// <summary>(short) rotation block list head</summary>
        public readonly int fs_old_postbloff;
        /// <summary>(uchar_t) blocks for each rotation</summary>
        public readonly int fs_old_rotbloff;
        /// <summary>magic number</summary>
        public readonly uint fs_magic;
        /// <summary>list of blocks for each rotation</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public readonly byte[] fs_rotbl;
    }

#endregion
}