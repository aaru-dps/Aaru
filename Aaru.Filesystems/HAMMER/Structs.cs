// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : HAMMER filesystem plugin.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using hammer_crc_t = uint;
using hammer_off_t = ulong;
using hammer_tid_t = ulong;

#pragma warning disable 169

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the HAMMER filesystem</summary>
public sealed partial class HAMMER
{
#region Nested type: HammerBlockMap

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    struct HammerBlockMap
    {
        /// <summary>zone-2 offset only used by zone-4</summary>
        public hammer_off_t phys_offset;
        /// <summary>zone-X offset only used by zone-3</summary>
        public hammer_off_t first_offset;
        /// <summary>zone-X offset for allocation</summary>
        public hammer_off_t next_offset;
        /// <summary>zone-X offset only used by zone-3</summary>
        public hammer_off_t alloc_offset;
        public uint         reserved01;
        public hammer_crc_t entry_crc;
    }

#endregion

#region Nested type: SuperBlock

    /// <summary>Hammer superblock</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    readonly struct SuperBlock
    {
        /// <summary><see cref="HAMMER_FSBUF_VOLUME" /> for a valid header</summary>
        public readonly ulong vol_signature;

        /* These are relative to block device offset, not zone offsets. */
        /// <summary>offset of boot area</summary>
        public readonly long vol_bot_beg;
        /// <summary>offset of memory log</summary>
        public readonly long vol_mem_beg;
        /// <summary>offset of the first buffer in volume</summary>
        public readonly long vol_buf_beg;
        /// <summary>offset of volume EOF (on buffer boundary)</summary>
        public readonly long vol_buf_end;
        public readonly long vol_reserved01;

        /// <summary>identify filesystem</summary>
        public readonly Guid vol_fsid;
        /// <summary>identify filesystem type</summary>
        public readonly Guid vol_fstype;
        /// <summary>filesystem label</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] vol_label;

        /// <summary>volume number within filesystem</summary>
        public readonly int vol_no;
        /// <summary>number of volumes making up filesystem</summary>
        public readonly int vol_count;

        /// <summary>version control information</summary>
        public readonly uint vol_version;
        /// <summary>header crc</summary>
        public readonly hammer_crc_t vol_crc;
        /// <summary>volume flags</summary>
        public readonly uint vol_flags;
        /// <summary>the root volume number (must be 0)</summary>
        public readonly uint vol_rootvol;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly uint[] vol_reserved;

        /*
         * These fields are initialized and space is reserved in every
         * volume making up a HAMMER filesystem, but only the root volume
         * contains valid data.  Note that vol0_stat_bigblocks does not
         * include big-blocks for freemap and undomap initially allocated
         * by newfs_hammer(8).
         */
        /// <summary>total big-blocks when fs is empty</summary>
        public readonly long vol0_stat_bigblocks;
        /// <summary>number of free big-blocks</summary>
        public readonly long vol0_stat_freebigblocks;
        public readonly long vol0_reserved01;
        /// <summary>for statfs only</summary>
        public readonly long vol0_stat_inodes;
        public readonly long vol0_reserved02;
        /// <summary>B-Tree root offset in zone-8</summary>
        public readonly hammer_off_t vol0_btree_root;
        /// <summary>highest partially synchronized TID</summary>
        public readonly hammer_tid_t vol0_next_tid;
        public readonly hammer_off_t vol0_reserved03;

        /// <summary>
        ///     Blockmaps for zones.  Not all zones use a blockmap.  Note that the entire root blockmap is cached in the
        ///     hammer_mount structure.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly HammerBlockMap[] vol0_blockmap;

        /// <summary>Array of zone-2 addresses for undo FIFO.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly hammer_off_t[] vol0_undo_array;
    }

#endregion
}