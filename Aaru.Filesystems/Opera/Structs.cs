// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
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
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

public sealed partial class OperaFS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        /// <summary>0x000, Record type, must be 1</summary>
        public readonly byte record_type;
        /// <summary>0x001, 5 bytes, "ZZZZZ"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] sync_bytes;
        /// <summary>0x006, Record version, must be 1</summary>
        public readonly byte record_version;
        /// <summary>0x007, Volume flags</summary>
        public readonly byte volume_flags;
        /// <summary>0x008, 32 bytes, volume comment</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NAME)]
        public readonly byte[] volume_comment;
        /// <summary>0x028, 32 bytes, volume label</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NAME)]
        public readonly byte[] volume_label;
        /// <summary>0x048, Volume ID</summary>
        public readonly uint volume_id;
        /// <summary>0x04C, Block size in bytes</summary>
        public readonly uint block_size;
        /// <summary>0x050, Blocks in volume</summary>
        public readonly uint block_count;
        /// <summary>0x054, Root directory ID</summary>
        public readonly uint root_dirid;
        /// <summary>0x058, Root directory blocks</summary>
        public readonly uint rootdir_blocks;
        /// <summary>0x05C, Root directory block size</summary>
        public readonly uint rootdir_bsize;
        /// <summary>0x060, Last root directory copy</summary>
        public readonly uint last_root_copy;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryHeader
    {
        /// <summary>Next block from this directory, -1 if last</summary>
        public readonly int next_block;
        /// <summary>Previous block from this directory, -1 if first</summary>
        public readonly int prev_block;
        /// <summary>Directory flags</summary>
        public readonly uint flags;
        /// <summary>Offset to first free unused byte in the directory</summary>
        public readonly uint first_free;
        /// <summary>Offset to first directory entry</summary>
        public readonly uint first_used;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryEntry
    {
        /// <summary>File flags, see <see cref="FileFlags" /></summary>
        public readonly uint flags;
        /// <summary>Unique file identifier</summary>
        public readonly uint id;
        /// <summary>Entry type</summary>
        public readonly uint type;
        /// <summary>Block size</summary>
        public readonly uint block_size;
        /// <summary>Size in bytes</summary>
        public readonly uint byte_count;
        /// <summary>Block count</summary>
        public readonly uint block_count;
        /// <summary>Unknown</summary>
        public readonly uint burst;
        /// <summary>Unknown</summary>
        public readonly uint gap;
        /// <summary>Filename</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NAME)]
        public readonly byte[] name;
        /// <summary>Last copy</summary>
        public readonly uint last_copy;
    }

    sealed class DirectoryEntryWithPointers
    {
        public DirectoryEntry Entry;
        public uint[]         Pointers;
    }

    sealed class OperaFileNode : IFileNode
    {
        internal DirectoryEntryWithPointers _dentry;
        /// <inheritdoc />
        public string Path { get; init; }
        /// <inheritdoc />
        public long Length { get; init; }
        /// <inheritdoc />
        public long Offset { get; set; }
    }
}