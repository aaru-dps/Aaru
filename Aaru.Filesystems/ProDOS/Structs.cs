// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple ProDOS filesystem plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Aaru.Filesystems;

// Information from Apple ProDOS 8 Technical Reference
/// <inheritdoc />
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class ProDOSPlugin
{
#region Nested type: DirectoryBlockHeader

    /// <summary>ProDOS directory block header (on-disk structure, 4 bytes)</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DirectoryBlockHeader
    {
        /// <summary>Pointer to previous directory block, 0 if first Offset 0x00, 2 bytes</summary>
        public ushort prev;
        /// <summary>Pointer to next directory block, 0 if last Offset 0x02, 2 bytes</summary>
        public ushort next;
    }

#endregion

#region Nested type: VolumeDirectoryHeader

    /// <summary>ProDOS volume directory header (on-disk structure, 39 bytes)</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VolumeDirectoryHeader
    {
        /// <summary>Storage type (0xF0) and name length. Offset 0x00, 1 byte</summary>
        public byte storage_type_name_length;
        /// <summary>Volume name. Offset 0x01, 15 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] volume_name;
        /// <summary>Reserved. Offset 0x10, 6 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] reserved;
        /// <summary>GS/OS case bits for volume name. Offset 0x16, 2 bytes</summary>
        public ushort case_bits;
        /// <summary>Creation date. Offset 0x18, 2 bytes</summary>
        public ushort creation_date;
        /// <summary>Creation time. Offset 0x1A, 2 bytes</summary>
        public ushort creation_time;
        /// <summary>Version. Offset 0x1C, 1 byte</summary>
        public byte   version;
        /// <summary>Minimum version. Offset 0x1D, 1 byte</summary>
        public byte   min_version;
        /// <summary>Access flags. Offset 0x1E, 1 byte</summary>
        public byte   access;
        /// <summary>Entry length (always 0x27). Offset 0x1F, 1 byte</summary>
        public byte   entry_length;
        /// <summary>Entries per block (always 0x0D). Offset 0x20, 1 byte</summary>
        public byte   entries_per_block;
        /// <summary>Number of active entries. Offset 0x21, 2 bytes</summary>
        public ushort entry_count;
        /// <summary>Bitmap start block. Offset 0x23, 2 bytes</summary>
        public ushort bitmap_block;
        /// <summary>Total blocks on volume. Offset 0x25, 2 bytes</summary>
        public ushort total_blocks;
    }

#endregion

#region Nested type: DirectoryHeader

    /// <summary>ProDOS directory header (on-disk structure, 39 bytes)</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DirectoryHeader
    {
        /// <summary>Storage type (0xE0) and name length Offset 0x00, 1 byte</summary>
        public byte storage_type_name_length;
        /// <summary>Directory name Offset 0x01, 15 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] directory_name;
        /// <summary>Reserved Offset 0x10, 8 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] reserved;
        /// <summary>Creation date Offset 0x18, 2 bytes</summary>
        public ushort creation_date;
        /// <summary>Creation time Offset 0x1A, 2 bytes</summary>
        public ushort creation_time;
        /// <summary>GS/OS case bits for filename Offset 0x1C, 2 bytes</summary>
        public ushort case_bits;
        /// <summary>Access flags Offset 0x1E, 1 byte</summary>
        public byte   access;
        /// <summary>Entry length (always 0x27) Offset 0x1F, 1 byte</summary>
        public byte   entry_length;
        /// <summary>Entries per block (always 0x0D) Offset 0x20, 1 byte</summary>
        public byte   entries_per_block;
        /// <summary>Number of active entries Offset 0x21, 2 bytes</summary>
        public ushort entry_count;
        /// <summary>Parent directory block Offset 0x23, 2 bytes</summary>
        public ushort parent_block;
        /// <summary>Entry number in parent block Offset 0x25, 1 byte</summary>
        public byte   parent_entry;
        /// <summary>Parent entry length Offset 0x26, 1 byte</summary>
        public byte   parent_entry_length;
    }

#endregion

#region Nested type: Entry

    /// <summary>ProDOS directory entry (on-disk structure, 39 bytes)</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Entry
    {
        /// <summary>Storage type (high nibble) and name length (low nibble) Offset 0x00, 1 byte</summary>
        public byte storage_type_name_length;
        /// <summary>File name Offset 0x01, 15 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] file_name;
        /// <summary>File type Offset 0x10, 1 byte</summary>
        public byte   file_type;
        /// <summary>Key block pointer Offset 0x11, 2 bytes</summary>
        public ushort key_pointer;
        /// <summary>Blocks used Offset 0x13, 2 bytes</summary>
        public ushort blocks_used;
        /// <summary>EOF (file size), 3 bytes little-endian Offset 0x15, 3 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] eof;
        /// <summary>Creation date Offset 0x18, 2 bytes</summary>
        public ushort creation_date;
        /// <summary>Creation time Offset 0x1A, 2 bytes</summary>
        public ushort creation_time;
        /// <summary>GS/OS case bits for filename Offset 0x1C, 2 bytes</summary>
        public ushort case_bits;
        /// <summary>Access flags Offset 0x1E, 1 byte</summary>
        public byte   access;
        /// <summary>Auxiliary type Offset 0x1F, 2 bytes</summary>
        public ushort aux_type;
        /// <summary>Modification date Offset 0x21, 2 bytes</summary>
        public ushort modification_date;
        /// <summary>Modification time Offset 0x23, 2 bytes</summary>
        public ushort modification_time;
        /// <summary>Header (directory key block) pointer Offset 0x25, 2 bytes</summary>
        public ushort header_pointer;
    }

#endregion

#region Nested type: IndirectBlock

    /// <summary>ProDOS indirect block (on-disk structure, 512 bytes)</summary>
    /// <remarks>
    ///     Block pointers are stored with LSB in first 256 bytes and MSB in second 256 bytes.
    ///     To get block pointer N: (msbyte[N] &lt;&lt; 8) | lsbyte[N]
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IndirectBlock
    {
        /// <summary>Low bytes of up to 256 block pointers Offset 0x00, 256 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] lsbyte;
        /// <summary>High bytes of up to 256 block pointers Offset 0x100, 256 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] msbyte;
    }

#endregion

#region Nested type: ForkEntry

    /// <summary>ProDOS fork descriptor entry (on-disk structure, 8 bytes)</summary>
    /// <remarks>Used in extended file key blocks to describe data and resource forks</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ForkEntry
    {
        /// <summary>Storage type (0x01=seedling, 0x02=sapling, 0x03=tree) Offset 0x00, 1 byte</summary>
        public byte   storage_type;
        /// <summary>Key block pointer Offset 0x01, 2 bytes</summary>
        public ushort key_block;
        /// <summary>Blocks used by this fork Offset 0x03, 2 bytes</summary>
        public ushort blocks_used;
        /// <summary>EOF (size) of this fork, 3 bytes little-endian Offset 0x05, 3 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] eof;
    }

#endregion

#region Nested type: ExtendedKeyBlock

    /// <summary>ProDOS extended file key block (on-disk structure, 512 bytes)</summary>
    /// <remarks>
    ///     Used for storage type 0x50 (extended files with resource fork).
    ///     Contains descriptors for both data fork and resource fork.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ExtendedKeyBlock
    {
        /// <summary>Data fork descriptor Offset 0x00, 8 bytes</summary>
        public ForkEntry data_fork;
        /// <summary>Reserved Offset 0x08, 248 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 248)]
        public byte[] reserved1;
        /// <summary>Resource fork descriptor Offset 0x100, 8 bytes</summary>
        public ForkEntry resource_fork;
        /// <summary>Reserved Offset 0x108, 248 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 248)]
        public byte[] reserved2;
    }

#endregion
}