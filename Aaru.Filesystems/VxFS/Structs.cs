// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Veritas File System plugin.
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

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Veritas filesystem</summary>
public sealed partial class VxFS
{
#region Nested type: SuperBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        /// <summary>Magic number</summary>
        public readonly uint vs_magic;
        /// <summary>VxFS version</summary>
        public readonly int vs_version;
        /// <summary>create time - secs</summary>
        public readonly uint vs_ctime;
        /// <summary>create time - usecs</summary>
        public readonly uint vs_cutime;
        /// <summary>unused</summary>
        public readonly int __unused1;
        /// <summary>unused</summary>
        public readonly int __unused2;
        /// <summary>obsolete</summary>
        public readonly int vs_old_logstart;
        /// <summary>obsolete</summary>
        public readonly int vs_old_logend;
        /// <summary>block size</summary>
        public readonly int vs_bsize;
        /// <summary>number of blocks</summary>
        public readonly int vs_size;
        /// <summary>number of data blocks</summary>
        public readonly int vs_dsize;
        /// <summary>obsolete</summary>
        public readonly uint vs_old_ninode;
        /// <summary>obsolete</summary>
        public readonly int vs_old_nau;
        /// <summary>unused</summary>
        public readonly int __unused3;
        /// <summary>obsolete</summary>
        public readonly int vs_old_defiextsize;
        /// <summary>obsolete</summary>
        public readonly int vs_old_ilbsize;
        /// <summary>size of immediate data area</summary>
        public readonly int vs_immedlen;
        /// <summary>number of direct extentes</summary>
        public readonly int vs_ndaddr;
        /// <summary>address of first AU</summary>
        public readonly int vs_firstau;
        /// <summary>offset of extent map in AU</summary>
        public readonly int vs_emap;
        /// <summary>offset of inode map in AU</summary>
        public readonly int vs_imap;
        /// <summary>offset of ExtOp. map in AU</summary>
        public readonly int vs_iextop;
        /// <summary>offset of inode list in AU</summary>
        public readonly int vs_istart;
        /// <summary>offset of fdblock in AU</summary>
        public readonly int vs_bstart;
        /// <summary>aufirst + emap</summary>
        public readonly int vs_femap;
        /// <summary>aufirst + imap</summary>
        public readonly int vs_fimap;
        /// <summary>aufirst + iextop</summary>
        public readonly int vs_fiextop;
        /// <summary>aufirst + istart</summary>
        public readonly int vs_fistart;
        /// <summary>aufirst + bstart</summary>
        public readonly int vs_fbstart;
        /// <summary>number of entries in indir</summary>
        public readonly int vs_nindir;
        /// <summary>length of AU in blocks</summary>
        public readonly int vs_aulen;
        /// <summary>length of imap in blocks</summary>
        public readonly int vs_auimlen;
        /// <summary>length of emap in blocks</summary>
        public readonly int vs_auemlen;
        /// <summary>length of ilist in blocks</summary>
        public readonly int vs_auilen;
        /// <summary>length of pad in blocks</summary>
        public readonly int vs_aupad;
        /// <summary>data blocks in AU</summary>
        public readonly int vs_aublocks;
        /// <summary>log base 2 of aublocks</summary>
        public readonly int vs_maxtier;
        /// <summary>number of inodes per blk</summary>
        public readonly int vs_inopb;
        /// <summary>obsolete</summary>
        public readonly int vs_old_inopau;
        /// <summary>obsolete</summary>
        public readonly int vs_old_inopilb;
        /// <summary>obsolete</summary>
        public readonly int vs_old_ndiripau;
        /// <summary>size of indirect addr ext.</summary>
        public readonly int vs_iaddrlen;
        /// <summary>log base 2 of bsize</summary>
        public readonly int vs_bshift;
        /// <summary>log base 2 of inobp</summary>
        public readonly int vs_inoshift;
        /// <summary>~( bsize - 1 )</summary>
        public readonly int vs_bmask;
        /// <summary>bsize - 1</summary>
        public readonly int vs_boffmask;
        /// <summary>old_inopilb - 1</summary>
        public readonly int vs_old_inomask;
        /// <summary>checksum of V1 data</summary>
        public readonly int vs_checksum;
        /// <summary>number of free blocks</summary>
        public readonly int vs_free;
        /// <summary>number of free inodes</summary>
        public readonly int vs_ifree;
        /// <summary>number of free extents by size</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly int[] vs_efree;
        /// <summary>flags ?!?</summary>
        public readonly int vs_flags;
        /// <summary>filesystem has been changed</summary>
        public readonly byte vs_mod;
        /// <summary>clean FS</summary>
        public readonly byte vs_clean;
        /// <summary>unused</summary>
        public readonly ushort __unused4;
        /// <summary>mount time log ID</summary>
        public readonly uint vs_firstlogid;
        /// <summary>last time written - sec</summary>
        public readonly uint vs_wtime;
        /// <summary>last time written - usec</summary>
        public readonly uint vs_wutime;
        /// <summary>FS name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] vs_fname;
        /// <summary>FS pack name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] vs_fpack;
        /// <summary>log format version</summary>
        public readonly int vs_logversion;
        /// <summary>unused</summary>
        public readonly int __unused5;
        /// <summary>OLT extent and replica</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly int[] vs_oltext;
        /// <summary>OLT extent size</summary>
        public readonly int vs_oltsize;
        /// <summary>size of inode map</summary>
        public readonly int vs_iauimlen;
        /// <summary>size of IAU in blocks</summary>
        public readonly int vs_iausize;
        /// <summary>size of inode in bytes</summary>
        public readonly int vs_dinosize;
        /// <summary>indir levels per inode</summary>
        public readonly int vs_old_dniaddr;
        /// <summary>checksum of V2 RO</summary>
        public readonly int vs_checksum2;
    }

#endregion
}