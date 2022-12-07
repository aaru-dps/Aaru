// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNIX System V filesystem plugin.
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

// ReSharper disable NotAccessedField.Local

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the UNIX System V filesystem</summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "UnusedMember.Local"),
 SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class SysVfs
{
    // Old XENIX use different offsets
    #pragma warning disable CS0649
    struct XenixSuperBlock
    {
        /// <summary>0x000, index of first data zone</summary>
        public ushort s_isize;
        /// <summary>0x002, total number of zones of this volume</summary>
        public uint s_fsize;

        // the start of the free block list:
        /// <summary>0x006, blocks in s_free, &lt;=100</summary>
        public ushort s_nfree;
        /// <summary>0x008, 100 entries, 50 entries for Xenix 3, first free block list chunk</summary>
        public uint[] s_free;

        // the cache of free inodes:
        /// <summary>0x198 (0xD0), number of inodes in s_inode, &lt;= 100</summary>
        public ushort s_ninode;
        /// <summary>0x19A (0xD2), 100 entries, some free inodes</summary>
        public ushort[] s_inode;
        /// <summary>0x262 (0x19A), free block list manipulation lock</summary>
        public byte s_flock;
        /// <summary>0x263 (0x19B), inode cache manipulation lock</summary>
        public byte s_ilock;
        /// <summary>0x264 (0x19C), superblock modification flag</summary>
        public byte s_fmod;
        /// <summary>0x265 (0x19D), read-only mounted flag</summary>
        public byte s_ronly;
        /// <summary>0x266 (0x19E), time of last superblock update</summary>
        public int s_time;
        /// <summary>0x26A (0x1A2), total number of free zones</summary>
        public uint s_tfree;
        /// <summary>0x26E (0x1A6), total number of free inodes</summary>
        public ushort s_tinode;
        /// <summary>0x270 (0x1A8), blocks per cylinder</summary>
        public ushort s_cylblks;
        /// <summary>0x272 (0x1AA), blocks per gap</summary>
        public ushort s_gapblks;
        /// <summary>0x274 (0x1AC), device information ??</summary>
        public ushort s_dinfo0;
        /// <summary>0x276 (0x1AE), device information ??</summary>
        public ushort s_dinfo1;
        /// <summary>0x278 (0x1B0), 6 bytes, volume name</summary>
        public string s_fname;
        /// <summary>0x27E (0x1B6), 6 bytes, pack name</summary>
        public string s_fpack;
        /// <summary>0x284 (0x1BC), 0x46 if volume is clean</summary>
        public byte s_clean;
        /// <summary>0x285 (0x1BD), 371 bytes, 51 bytes for Xenix 3</summary>
        public byte[] s_fill;
        /// <summary>0x3F8 (0x1F0), magic</summary>
        public uint s_magic;
        /// <summary>0x3FC (0x1F4), filesystem type (1 = 512 bytes/blk, 2 = 1024 bytes/blk, 3 = 2048 bytes/blk)</summary>
        public uint s_type;
    }
    #pragma warning restore CS0649

    #pragma warning disable CS0649
    struct SystemVRelease4SuperBlock
    {
        /// <summary>0x000, index of first data zone</summary>
        public ushort s_isize;
        /// <summary>0x002, padding</summary>
        public ushort s_pad0;
        /// <summary>0x004, total number of zones of this volume</summary>
        public uint s_fsize;

        // the start of the free block list:
        /// <summary>0x008, blocks in s_free, &lt;=100</summary>
        public ushort s_nfree;
        /// <summary>0x00A, padding</summary>
        public ushort s_pad1;
        /// <summary>0x00C, 50 entries, first free block list chunk</summary>
        public uint[] s_free;

        // the cache of free inodes:
        /// <summary>0x0D4, number of inodes in s_inode, &lt;= 100</summary>
        public ushort s_ninode;
        /// <summary>0x0D6, padding</summary>
        public ushort s_pad2;
        /// <summary>0x0D8, 100 entries, some free inodes</summary>
        public ushort[] s_inode;
        /// <summary>0x1A0, free block list manipulation lock</summary>
        public byte s_flock;
        /// <summary>0x1A1, inode cache manipulation lock</summary>
        public byte s_ilock;
        /// <summary>0x1A2, superblock modification flag</summary>
        public byte s_fmod;
        /// <summary>0x1A3, read-only mounted flag</summary>
        public byte s_ronly;
        /// <summary>0x1A4, time of last superblock update</summary>
        public uint s_time;
        /// <summary>0x1A8, blocks per cylinder</summary>
        public ushort s_cylblks;
        /// <summary>0x1AA, blocks per gap</summary>
        public ushort s_gapblks;
        /// <summary>0x1AC, device information ??</summary>
        public ushort s_dinfo0;
        /// <summary>0x1AE, device information ??</summary>
        public ushort s_dinfo1;
        /// <summary>0x1B0, total number of free zones</summary>
        public uint s_tfree;
        /// <summary>0x1B4, total number of free inodes</summary>
        public ushort s_tinode;
        /// <summary>0x1B6, padding</summary>
        public ushort s_pad3;
        /// <summary>0x1B8, 6 bytes, volume name</summary>
        public string s_fname;
        /// <summary>0x1BE, 6 bytes, pack name</summary>
        public string s_fpack;
        /// <summary>0x1C4, 48 bytes</summary>
        public byte[] s_fill;
        /// <summary>0x1F4, if s_state == (0x7C269D38 - s_time) then filesystem is clean</summary>
        public uint s_state;
        /// <summary>0x1F8, magic</summary>
        public uint s_magic;
        /// <summary>0x1FC, filesystem type (1 = 512 bytes/blk, 2 = 1024 bytes/blk)</summary>
        public uint s_type;
    }
    #pragma warning restore CS0649

    #pragma warning disable CS0649
    struct SystemVRelease2SuperBlock
    {
        /// <summary>0x000, index of first data zone</summary>
        public ushort s_isize;
        /// <summary>0x002, total number of zones of this volume</summary>
        public uint s_fsize;

        // the start of the free block list:
        /// <summary>0x006, blocks in s_free, &lt;=100</summary>
        public ushort s_nfree;
        /// <summary>0x008, 50 entries, first free block list chunk</summary>
        public uint[] s_free;

        // the cache of free inodes:
        /// <summary>0x0D0, number of inodes in s_inode, &lt;= 100</summary>
        public ushort s_ninode;
        /// <summary>0x0D2, 100 entries, some free inodes</summary>
        public ushort[] s_inode;
        /// <summary>0x19A, free block list manipulation lock</summary>
        public byte s_flock;
        /// <summary>0x19B, inode cache manipulation lock</summary>
        public byte s_ilock;
        /// <summary>0x19C, superblock modification flag</summary>
        public byte s_fmod;
        /// <summary>0x19D, read-only mounted flag</summary>
        public byte s_ronly;
        /// <summary>0x19E, time of last superblock update</summary>
        public uint s_time;
        /// <summary>0x1A2, blocks per cylinder</summary>
        public ushort s_cylblks;
        /// <summary>0x1A4, blocks per gap</summary>
        public ushort s_gapblks;
        /// <summary>0x1A6, device information ??</summary>
        public ushort s_dinfo0;
        /// <summary>0x1A8, device information ??</summary>
        public ushort s_dinfo1;
        /// <summary>0x1AA, total number of free zones</summary>
        public uint s_tfree;
        /// <summary>0x1AE, total number of free inodes</summary>
        public ushort s_tinode;
        /// <summary>0x1B0, 6 bytes, volume name</summary>
        public string s_fname;
        /// <summary>0x1B6, 6 bytes, pack name</summary>
        public string s_fpack;
        /// <summary>0x1BC, 56 bytes</summary>
        public byte[] s_fill;
        /// <summary>0x1F4, if s_state == (0x7C269D38 - s_time) then filesystem is clean</summary>
        public uint s_state;
        /// <summary>0x1F8, magic</summary>
        public uint s_magic;
        /// <summary>0x1FC, filesystem type (1 = 512 bytes/blk, 2 = 1024 bytes/blk)</summary>
        public uint s_type;
    }
    #pragma warning restore CS0649

    #pragma warning disable CS0649
    struct UNIX7thEditionSuperBlock
    {
        /// <summary>0x000, index of first data zone</summary>
        public ushort s_isize;
        /// <summary>0x002, total number of zones of this volume</summary>
        public uint s_fsize;

        // the start of the free block list:
        /// <summary>0x006, blocks in s_free, &lt;=100</summary>
        public ushort s_nfree;
        /// <summary>0x008, 50 entries, first free block list chunk</summary>
        public uint[] s_free;

        // the cache of free inodes:
        /// <summary>0x0D0, number of inodes in s_inode, &lt;= 100</summary>
        public ushort s_ninode;
        /// <summary>0x0D2, 100 entries, some free inodes</summary>
        public ushort[] s_inode;
        /// <summary>0x19A, free block list manipulation lock</summary>
        public byte s_flock;
        /// <summary>0x19B, inode cache manipulation lock</summary>
        public byte s_ilock;
        /// <summary>0x19C, superblock modification flag</summary>
        public byte s_fmod;
        /// <summary>0x19D, read-only mounted flag</summary>
        public byte s_ronly;
        /// <summary>0x19E, time of last superblock update</summary>
        public uint s_time;
        /// <summary>0x1A2, total number of free zones</summary>
        public uint s_tfree;
        /// <summary>0x1A6, total number of free inodes</summary>
        public ushort s_tinode;
        /// <summary>0x1A8, interleave factor</summary>
        public ushort s_int_m;
        /// <summary>0x1AA, interleave factor</summary>
        public ushort s_int_n;
        /// <summary>0x1AC, 6 bytes, volume name</summary>
        public string s_fname;
        /// <summary>0x1B2, 6 bytes, pack name</summary>
        public string s_fpack;
    }
    #pragma warning restore CS0649

    #pragma warning disable CS0649
    struct CoherentSuperBlock
    {
        /// <summary>0x000, index of first data zone</summary>
        public ushort s_isize;
        /// <summary>0x002, total number of zones of this volume</summary>
        public uint s_fsize;

        // the start of the free block list:
        /// <summary>0x006, blocks in s_free, &lt;=100</summary>
        public ushort s_nfree;
        /// <summary>0x008, 64 entries, first free block list chunk</summary>
        public uint[] s_free;

        // the cache of free inodes:
        /// <summary>0x108, number of inodes in s_inode, &lt;= 100</summary>
        public ushort s_ninode;
        /// <summary>0x10A, 100 entries, some free inodes</summary>
        public ushort[] s_inode;
        /// <summary>0x1D2, free block list manipulation lock</summary>
        public byte s_flock;
        /// <summary>0x1D3, inode cache manipulation lock</summary>
        public byte s_ilock;
        /// <summary>0x1D4, superblock modification flag</summary>
        public byte s_fmod;
        /// <summary>0x1D5, read-only mounted flag</summary>
        public byte s_ronly;
        /// <summary>0x1D6, time of last superblock update</summary>
        public uint s_time;
        /// <summary>0x1DE, total number of free zones</summary>
        public uint s_tfree;
        /// <summary>0x1E2, total number of free inodes</summary>
        public ushort s_tinode;
        /// <summary>0x1E4, interleave factor</summary>
        public ushort s_int_m;
        /// <summary>0x1E6, interleave factor</summary>
        public ushort s_int_n;
        /// <summary>0x1E8, 6 bytes, volume name</summary>
        public string s_fname;
        /// <summary>0x1EE, 6 bytes, pack name</summary>
        public string s_fpack;
        /// <summary>0x1F4, zero-filled</summary>
        public uint s_unique;
    }
    #pragma warning restore CS0649
}