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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable NotAccessedField.Local

using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Aaru.Filesystems;

// Information from Apple ProDOS 8 Technical Reference
/// <inheritdoc />
/// <summary>Implements detection of Apple ProDOS filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class ProDOSPlugin
{
#region Nested type: DirectoryBlock

    struct DirectoryBlock
    {
        /// <summary>Pointer to previous directory block Offset 0x00, 2 bytes</summary>
        public ushort zero;
        /// <summary>Pointer to next directory block, 0 if last Offset 0x02, 2 bytes</summary>
        public ushort next_pointer;
        /// <summary>Directory entries Offset 0x2F, 39 bytes each, 13 entries</summary>
        public Entry[] entries;
    }

#endregion

#region Nested type: DirectoryHeader

    struct DirectoryHeader
    {
        /// <summary>Constant 0x0E Offset 0x04, mask 0xF0</summary>
        public byte storage_type;
        /// <summary>Length of volume_name pascal string Offset 0x04, mask 0x0F</summary>
        public byte name_length;
        /// <summary>The name of the directory. Offset 0x05, 15 bytes</summary>
        public string directory_name;
        /// <summary>Reserved for future expansion Offset 0x14, 8 bytes</summary>
        public ulong reserved;
        /// <summary>Creation time of the volume Offset 0x1C, 4 bytes</summary>
        public DateTime creation_time;
        /// <summary>Version number of the volume format Offset 0x20, 1 byte</summary>
        public byte version;
        /// <summary>Reserved for future use Offset 0x21, 1 byte</summary>
        public byte min_version;
        /// <summary>Permissions for the volume Offset 0x22, 1 byte</summary>
        public byte access;
        /// <summary>Length of an entry in this directory Const 0x27 Offset 0x23, 1 byte</summary>
        public byte entry_length;
        /// <summary>Number of entries per block Const 0x0D Offset 0x24, 1 byte</summary>
        public byte entries_per_block;
        /// <summary>Number of active files in this directory Offset 0x25, 2 bytes</summary>
        public ushort file_count;
        /// <summary>Block address of parent directory block that contains this entry Offset 0x27, 2 bytes</summary>
        public ushort parent_pointer;
        /// <summary>Entry number within the block indicated in parent_pointer Offset 0x29, 1 byte</summary>
        public byte parent_entry_number;
        /// <summary>Length of the entry that holds this directory, in the parent entry Const 0x27 Offset 0x2A, 1 byte</summary>
        public byte parent_entry_length;
    }

#endregion

#region Nested type: DirectoryKeyBlock

    struct DirectoryKeyBlock
    {
        /// <summary>Always 0 Offset 0x00, 2 bytes</summary>
        public ushort zero;
        /// <summary>Pointer to next directory block, 0 if last Offset 0x02, 2 bytes</summary>
        public ushort next_pointer;
        /// <summary>Directory header Offset 0x04, 39 bytes</summary>
        public DirectoryHeader header;
        /// <summary>Directory entries Offset 0x2F, 39 bytes each, 12 entries</summary>
        public Entry[] entries;
    }

#endregion

#region Nested type: Entry

    /// <summary>ProDOS directory entry, decoded structure</summary>
    struct Entry
    {
        /// <summary>Type of file pointed by this entry Offset 0x00, mask 0xF0</summary>
        public byte storage_type;
        /// <summary>Length of name_length pascal string Offset 0x00, mask 0x0F</summary>
        public byte name_length;
        /// <summary>Pascal string of file name Offset 0x01, 15 bytes</summary>
        public string file_name;
        /// <summary>Descriptor of internal structure of the file Offset 0x10, 1 byte</summary>
        public byte file_type;
        /// <summary>
        ///     Block address of master index block for tree files. Block address of index block for sapling files. Block
        ///     address of block for seedling files. Offset 0x11, 2 bytes
        /// </summary>
        public ushort key_pointer;
        /// <summary>Blocks used by file or directory, including index blocks. Offset 0x13, 2 bytes</summary>
        public ushort blocks_used;
        /// <summary>Size of file in bytes Offset 0x15, 3 bytes</summary>
        public uint EOF;
        /// <summary>File creation datetime Offset 0x18, 4 bytes</summary>
        public DateTime creation_time;
        /// <summary>Version of ProDOS that created this file Offset 0x1C, 1 byte</summary>
        public byte version;
        /// <summary>Minimum version of ProDOS needed to access this file Offset 0x1D, 1 byte</summary>
        public byte min_version;
        /// <summary>File permissions Offset 0x1E, 1 byte</summary>
        public byte access;
        /// <summary>General purpose field to store additional information about file format Offset 0x1F, 2 bytes</summary>
        public ushort aux_type;
        /// <summary>File last modification date time Offset 0x21, 4 bytes</summary>
        public DateTime last_mod;
        /// <summary>Block address pointer to key block of the directory containing this entry Offset 0x25, 2 bytes</summary>
        public ushort header_pointer;
    }

#endregion

#region Nested type: IndexBlock

    struct IndexBlock
    {
        /// <summary>Up to 256 pointers to blocks, 0 to indicate the block is sparsed (non-allocated)</summary>
        public ushort[] block_pointer;
    }

#endregion

#region Nested type: MasterIndexBlock

    struct MasterIndexBlock
    {
        /// <summary>Up to 128 pointers to index blocks</summary>
        public ushort[] index_block_pointer;
    }

#endregion

#region Nested type: RootDirectoryHeader

    struct RootDirectoryHeader
    {
        /// <summary>Constant 0x0F Offset 0x04, mask 0xF0</summary>
        public byte storage_type;
        /// <summary>Length of volume_name pascal string Offset 0x04, mask 0x0F</summary>
        public byte name_length;
        /// <summary>The name of the volume. Offset 0x05, 15 bytes</summary>
        public string volume_name;
        /// <summary>Reserved for future expansion Offset 0x14, 8 bytes</summary>
        public ulong reserved;
        /// <summary>Creation time of the volume Offset 0x1C, 4 bytes</summary>
        public DateTime creation_time;
        /// <summary>Version number of the volume format Offset 0x20, 1 byte</summary>
        public byte version;
        /// <summary>Reserved for future use Offset 0x21, 1 byte</summary>
        public byte min_version;
        /// <summary>Permissions for the volume Offset 0x22, 1 byte</summary>
        public byte access;
        /// <summary>Length of an entry in this directory Const 0x27 Offset 0x23, 1 byte</summary>
        public byte entry_length;
        /// <summary>Number of entries per block Const 0x0D Offset 0x24, 1 byte</summary>
        public byte entries_per_block;
        /// <summary>Number of active files in this directory Offset 0x25, 2 bytes</summary>
        public ushort file_count;
        /// <summary>
        ///     Block address of the first block of the volume's bitmap, one for every 4096 blocks or fraction Offset 0x27, 2
        ///     bytes
        /// </summary>
        public ushort bit_map_pointer;
        /// <summary>Total number of blocks in the volume Offset 0x29, 2 bytes</summary>
        public ushort total_blocks;
    }

#endregion

#region Nested type: RootDirectoryKeyBlock

    struct RootDirectoryKeyBlock
    {
        /// <summary>Always 0 Offset 0x00, 2 bytes</summary>
        public ushort zero;
        /// <summary>Pointer to next directory block, 0 if last Offset 0x02, 2 bytes</summary>
        public ushort next_pointer;
        /// <summary>Directory header Offset 0x04, 39 bytes</summary>
        public RootDirectoryHeader header;
        /// <summary>Directory entries Offset 0x2F, 39 bytes each, 12 entries</summary>
        public Entry[] entries;
    }

#endregion
}