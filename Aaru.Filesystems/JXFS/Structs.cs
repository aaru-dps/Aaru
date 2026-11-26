// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Attributes;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class JXFS
{
#region Nested type: SuperBlock

    /// <summary>Superblock, in the first block of the volume and again in the last one</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct SuperBlock
    {
        /// <summary>0x00, "JXF\4"</summary>
        public uint magic;
        /// <summary>0x04, block checksum</summary>
        public uint checksum;
        /// <summary>0x08, block number of this block, 0 for the main superblock</summary>
        public uint ownBlock;
        /// <summary>0x0C, unknown</summary>
        public uint reserved1;
        /// <summary>0x10, unknown</summary>
        public uint reserved2;
        /// <summary>0x14, version, 0x11</summary>
        public ushort version;
        /// <summary>0x16, unknown</summary>
        public ushort reserved3;
        /// <summary>0x18, creation time, in seconds since 1978-01-01</summary>
        public uint creationDate;
        /// <summary>0x1C, volume flags</summary>
        public VolumeFlags flags;
        /// <summary>0x1D, unknown</summary>
        public byte reserved4;
        /// <summary>0x1E, unknown</summary>
        public ushort reserved5;
        /// <summary>0x20, unknown</summary>
        public uint reserved6;
        /// <summary>0x24, unknown</summary>
        public uint reserved7;
        /// <summary>0x28, device byte where the volume starts</summary>
        public ulong firstByte;
        /// <summary>0x30, device byte where the volume ends, exclusive</summary>
        public ulong lastByte;
        /// <summary>0x38, blocks in the volume</summary>
        public uint totalBlocks;
        /// <summary>0x3C, block size in bytes</summary>
        public uint blockSize;
    }

#endregion

#region Nested type: Node

    /// <summary>A 44-byte node from the <c>:node:</c> system file, describing one file, directory or link</summary>
    struct Node
    {
        /// <summary>0x00, node number</summary>
        public uint Number;
        /// <summary>0x04, AmigaDOS protection with the RWED bits inverted</summary>
        public uint Protection;
        /// <summary>0x08, attribute block relative to <c>:misc:</c>, 0 if none</summary>
        public uint AttributeBlock;
        /// <summary>0x0C, 48-bit file size</summary>
        public ulong Size;
        /// <summary>0x0C, first directory container for directories, soft link block for links</summary>
        public uint Pointer;
        /// <summary>0x12, first extent; for small files, the first data block in the top 28 bits</summary>
        public ulong Extent1;
        /// <summary>0x18, second extent</summary>
        public ulong Extent2;
        /// <summary>0x1E, first extent container block relative to <c>:misc:</c>, 0 if none</summary>
        public uint ExtentBlock;
        /// <summary>0x22, type and flags</summary>
        public NodeFlags Flags;
        /// <summary>0x24, modification time, in 40-bit ticks (1/50 s) since 1978-01-01</summary>
        public ulong Date;
    }

#endregion

#region Nested type: Extent

    /// <summary>A run of contiguous blocks</summary>
    struct Extent
    {
        /// <summary>First block</summary>
        public ulong Start;
        /// <summary>Blocks in the run</summary>
        public ulong Blocks;
    }

#endregion

#region Nested type: DirectoryEntry

    /// <summary>An entry from a directory container, with the node it points to</summary>
    struct DirectoryEntry
    {
        /// <summary>Entry key, container number shifted left 6 bits plus the slot</summary>
        public uint Key;
        /// <summary>Entry name</summary>
        public string Name;
        /// <summary>Node the entry points to</summary>
        public Node Node;
    }

#endregion
}