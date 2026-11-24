// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SmartFileSystem plugin.
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
public sealed partial class SFS
{
#region Nested type: BlockHeader

    /// <summary>Standard block header found before every type of block in SFS, except data blocks</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct BlockHeader
    {
        /// <summary>4 character ID string of this block</summary>
        public uint id;
        /// <summary>
        ///     The checksum. SUM of all LONGs in a block plus one, then negated.
        ///     When checking, sum of all longs should be zero.
        /// </summary>
        public uint checksum;
        /// <summary>The block number where this block is stored (self-reference for validation)</summary>
        public uint ownBlock;
    }

#endregion

#region Nested type: RootBlock

    /// <summary>
    ///     Root block structure. SFS has two root blocks, one at the start and one at the end of the partition.
    ///     The one with the highest sequence number is valid.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct RootBlock
    {
        /// <summary>Block ID ('SFS\0')</summary>
        public uint blockId;
        /// <summary>Block checksum</summary>
        public uint blockChecksum;
        /// <summary>Block self-pointer for validation</summary>
        public uint blockSelfPointer;
        /// <summary>Version number of the filesystem block structure</summary>
        public ushort version;
        /// <summary>Sequence number - the root with the highest sequence number is valid</summary>
        public ushort sequence;
        /// <summary>Creation date (when first formatted), cannot be changed</summary>
        public uint datecreated;
        /// <summary>Various settings flags</summary>
        public Flags bits;
        /// <summary>Padding</summary>
        public byte padding1;
        /// <summary>Padding</summary>
        public ushort padding2;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] reserved1;
        /// <summary>First byte of partition from start of disk (64-bit)</summary>
        public ulong firstbyte;
        /// <summary>Last byte of partition, excluding this one (64-bit)</summary>
        public ulong lastbyte;
        /// <summary>Size of this partition in blocks</summary>
        public uint totalblocks;
        /// <summary>Block size used</summary>
        public uint blocksize;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] reserved2;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] reserved3;
        /// <summary>Location of the bitmap</summary>
        public uint bitmapbase;
        /// <summary>Location of first admin space container</summary>
        public uint adminspacecontainer;
        /// <summary>Location of the root object container</summary>
        public uint rootobjectcontainer;
        /// <summary>Location of the root of the extent bnode B-tree</summary>
        public uint extentbnoderoot;
        /// <summary>Location of the root of the object node tree</summary>
        public uint objectnoderoot;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] reserved4;
    }

#endregion

#region Nested type: RootInfo

    /// <summary>Root information structure containing various disk format information</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct RootInfo
    {
        /// <summary>Amount in blocks which deleted files consume</summary>
        public uint deletedBlocks;
        /// <summary>Number of deleted files in recycled</summary>
        public uint deletedFiles;
        /// <summary>Cached number of free blocks on disk</summary>
        public uint freeBlocks;
        /// <summary>Date created</summary>
        public uint dateCreated;
        /// <summary>Block which was most recently allocated</summary>
        public uint lastAllocatedBlock;
        /// <summary>AdminSpaceContainer which most recently was used to allocate a block</summary>
        public uint lastAllocatedAdminSpace;
        /// <summary>ExtentNode which was most recently created</summary>
        public uint lastAllocatedExtentNode;
        /// <summary>ObjectNode which was most recently created</summary>
        public uint lastAllocatedObjectNode;
        /// <summary>Roving pointer for allocation</summary>
        public uint rovingPointer;
    }

#endregion

#region Nested type: ExtentBNode

    /// <summary>Extent B-tree node, used to track file data extents (SFS\0 version - 14 bytes)</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct ExtentBNode
    {
        /// <summary>Key (block number where extent starts)</summary>
        public uint key;
        /// <summary>Next extent in chain (0 if last)</summary>
        public uint next;
        /// <summary>Previous extent in chain (high bit set means points to object node)</summary>
        public uint prev;
        /// <summary>The size in blocks of the region this extent controls (16-bit in SFS\0)</summary>
        public ushort blocks;
    }

#endregion

#region Nested type: ExtentBNode2

    /// <summary>Extent B-tree node, used to track file data extents (SFS\2 version - 16 bytes)</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct ExtentBNode2
    {
        /// <summary>Key (block number where extent starts)</summary>
        public uint key;
        /// <summary>Next extent in chain (0 if last)</summary>
        public uint next;
        /// <summary>Previous extent in chain (high bit set means points to object node)</summary>
        public uint prev;
        /// <summary>The size in blocks of the region this extent controls (32-bit in SFS\2)</summary>
        public uint blocks;
    }

#endregion

#region Nested type: ObjectContainer

    /// <summary>
    ///     Object container structure used to hold various objects which have the same parent directory.
    ///     Objects are linked in a doubly linked list.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct ObjectContainer
    {
        /// <summary>Block header</summary>
        public BlockHeader header;
        /// <summary>Node number of the parent object</summary>
        public uint parent;
        /// <summary>Next ObjectContainer belonging to this directory, or zero if last</summary>
        public uint next;
        /// <summary>Previous ObjectContainer belonging to this directory, or zero if first</summary>
        public uint previous;

        // Followed by variable number of Object structures
    }

#endregion

#region Nested type: Object

    /// <summary>
    ///     Object structure describing a file or directory (SFS\0 version - 25 bytes fixed).
    ///     Multiple objects can be stored in an ObjectContainer block.
    ///     Note: In SFS\2 (version 4) files have a 48-bit size stored as a 16-bit sizeh (high bits) followed by
    ///     a 32-bit size (low bits) at offset 16, and directories have firstdirblock at offset 16 followed by
    ///     2 unused bytes, making the structure 27 bytes. This is handled manually during parsing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Object
    {
        /// <summary>Owner user ID (reserved, must be zero)</summary>
        public ushort ownerUid;
        /// <summary>Owner group ID (reserved, must be zero)</summary>
        public ushort ownerGid;
        /// <summary>Object node number uniquely identifying this object</summary>
        public uint objectNode;
        /// <summary>Protection bits (default 0x0000000F = R, W, E, D set)</summary>
        public uint protection;
        /// <summary>
        ///     For files: first data block. For directories: hash table block.
        ///     Use ObjectBits to determine which union member to use.
        /// </summary>
        public uint dataOrHashtable;
        /// <summary>
        ///     For files: size in bytes. For directories: first dir block.
        ///     Use ObjectBits to determine which union member to use.
        /// </summary>
        public uint sizeOrFirstDirBlock;
        /// <summary>Date of last modification (seconds from 1-1-1978)</summary>
        public uint dateModified;
        /// <summary>Object type bits (file/directory/link, etc.)</summary>
        public ObjectBits bits;

        // Followed by: name (zero-terminated), then comment (zero-terminated)
    }

#endregion

#region Nested type: HashTable

    /// <summary>
    ///     Hash table structure stored in a separate block.
    ///     Contains hash chains for fast file lookup by name.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct HashTable
    {
        /// <summary>Block header</summary>
        public BlockHeader header;
        /// <summary>Node number of the parent object</summary>
        public uint parent;

        // Followed by array of node numbers (hash entries)
    }

#endregion

#region Nested type: SoftLink

    /// <summary>Soft link structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct SoftLink
    {
        /// <summary>Block header</summary>
        public BlockHeader header;
        /// <summary>Node number of the parent object</summary>
        public uint parent;
        /// <summary>Next block</summary>
        public uint next;
        /// <summary>Previous block</summary>
        public uint previous;

        // Followed by: link path string (zero-terminated)
    }

#endregion

#region Nested type: NodeContainer

    /// <summary>Node container structure used by node trees</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct NodeContainer
    {
        /// <summary>Block header</summary>
        public BlockHeader header;
        /// <summary>The node number of the first node in this block</summary>
        public uint nodeNumber;
        /// <summary>
        ///     Total number of nodes per NodeIndexContainer or NodeIndexContainer from this point in the tree.
        ///     If this is 1, it is a leaf container.
        /// </summary>
        public uint nodes;

        // Followed by array of block numbers (node array)
    }

#endregion

#region Nested type: Node

    /// <summary>Node structure used by node trees</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Node
    {
        /// <summary>Node data</summary>
        public uint data;
    }

#endregion

#region Nested type: ObjectNode

    /// <summary>Object node structure linking objects in hash chains</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct ObjectNode
    {
        /// <summary>Base node</summary>
        public Node node;
        /// <summary>Next object in hash chain</summary>
        public uint next;
        /// <summary>16-bit hash value</summary>
        public ushort hash16;
    }

#endregion

#region Nested type: AdminSpace

    /// <summary>Admin space entry tracking block allocation within an admin space region</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct AdminSpace
    {
        /// <summary>Starting block of this admin space</summary>
        public uint space;
        /// <summary>Bitmap of used blocks (set bits = used, bit 31 is first block)</summary>
        public uint bits;
    }

#endregion

#region Nested type: AdminSpaceContainer

    /// <summary>Admin space container holding multiple admin space entries</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct AdminSpaceContainer
    {
        /// <summary>Block header</summary>
        public BlockHeader header;
        /// <summary>Next admin space container, or zero</summary>
        public uint next;
        /// <summary>Previous admin space container, or zero</summary>
        public uint previous;
        /// <summary>
        ///     Bits 0-2: bitmap size encoding.
        ///     000=1 byte, 001=2 bytes, 010=4 bytes, 011=8 bytes,
        ///     100=16 bytes, 101=32 bytes, 110=64 bytes, 111=128 bytes
        /// </summary>
        public byte bits;
        /// <summary>Padding</summary>
        public byte pad1;
        /// <summary>Padding</summary>
        public ushort pad2;

        // Followed by array of AdminSpace entries
    }

#endregion

#region Nested type: BNode

    /// <summary>B-tree node structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct BNode
    {
        /// <summary>Key</summary>
        public uint key;
        /// <summary>Data</summary>
        public uint data;
    }

#endregion

#region Nested type: BTreeContainer

    /// <summary>B-tree container structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct BTreeContainer
    {
        /// <summary>Number of nodes in this container</summary>
        public ushort nodeCount;
        /// <summary>True if this is a leaf node</summary>
        public byte isLeaf;
        /// <summary>Node size (must be a multiple of 2)</summary>
        public byte nodeSize;

        // Followed by array of BNode entries
    }

#endregion

#region Nested type: BNodeContainer

    /// <summary>B-tree node container block</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct BNodeContainer
    {
        /// <summary>Block header</summary>
        public BlockHeader header;
        /// <summary>B-tree container</summary>
        public BTreeContainer btc;

        // Followed by BNode array
    }

#endregion

#region Nested type: Bitmap

    /// <summary>
    ///     Bitmap block structure. One bit per block on disk.
    ///     Bits are 1 if block is free, 0 if used.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Bitmap
    {
        /// <summary>Block header</summary>
        public BlockHeader header;

        // Followed by bitmap data (array of uint)
    }

#endregion

#region Nested type: TransactionStorage

    /// <summary>Transaction storage block for journaling</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct TransactionStorage
    {
        /// <summary>Block header</summary>
        public BlockHeader header;
        /// <summary>Next transaction storage block</summary>
        public uint next;

        // Followed by transaction data
    }

#endregion

#region Nested type: TransactionFailure

    /// <summary>Transaction failure marker block</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct TransactionFailure
    {
        /// <summary>Block header</summary>
        public BlockHeader header;
        /// <summary>First transaction block</summary>
        public uint firstTransaction;
    }

#endregion
}