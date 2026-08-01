// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BTree.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System Plus plugin.
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

// ReSharper disable InconsistentNaming

using System;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Aaru.Filesystems;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
// B-tree search semantics follow Apple's reference implementation (BTreeTreeOps.c / BTreeNodeOps.c):
// binary search of the record offset array within a node, keyed descent from the root taking the
// greatest key less than or equal to the search key at each index node.
/// <summary>Implements detection of Apple Hierarchical File System Plus (HFS+)</summary>
public sealed partial class AppleHFSPlus
{
    /// <summary>Size in bytes of a B-tree node descriptor</summary>
    const int BT_NODE_DESCRIPTOR_SIZE = 14;

    /// <summary>Reads a B-tree node by node number</summary>
    delegate ErrorNumber NodeReader(uint nodeNumber, out byte[] nodeData);

    /// <summary>
    ///     Compares the search key against the raw trial key starting at <paramref name="keyOffset" /> (which
    ///     points at the keyLength field). Returns a negative value, zero, or a positive value as the search key
    ///     sorts before, equal to, or after the trial key.
    /// </summary>
    delegate int KeyComparer(byte[] nodeData, int keyOffset);

    /// <summary>Visits a record in a leaf node. Returns true to stop the enumeration.</summary>
    delegate bool RecordVisitor(byte[] nodeData, ushort recordOffset);

    /// <summary>Reads the record offset array entry for a record index, validating it against the node bounds</summary>
    static bool TryGetRecordOffset(byte[]     nodeData, ushort nodeSize, ushort numRecords, int index,
                                   out ushort recordOffset)
    {
        recordOffset = 0;

        int offsetPointerOffset = nodeSize - 2 * (index + 1);

        if(offsetPointerOffset < BT_NODE_DESCRIPTOR_SIZE || offsetPointerOffset + 2 > nodeData.Length) return false;

        recordOffset = BigEndianBitConverter.ToUInt16(nodeData, offsetPointerOffset);

        // Records live between the node descriptor and the offset array
        return recordOffset >= BT_NODE_DESCRIPTOR_SIZE && recordOffset < nodeSize - 2 * numRecords;
    }

    /// <summary>
    ///     Binary searches the records of a single B-tree node, following Apple's SearchNode. On an exact match
    ///     returns true with <paramref name="index" /> set to the matching record. On a miss returns false with
    ///     <paramref name="index" /> set to the insertion point (index of the first record greater than the search
    ///     key, which may equal numRecords).
    /// </summary>
    static bool SearchBTreeNode(byte[] nodeData, ushort nodeSize, ushort numRecords, KeyComparer compare, out int index)
    {
        var lowerBound = 0;
        int upperBound = numRecords - 1;

        while(lowerBound <= upperBound)
        {
            int mid = lowerBound + upperBound >> 1;

            if(!TryGetRecordOffset(nodeData, nodeSize, numRecords, mid, out ushort recordOffset))
            {
                // Corrupt offset array; treat as not found at the current lower bound
                index = lowerBound;

                return false;
            }

            int result = compare(nodeData, recordOffset);

            if(result < 0)
                upperBound = mid - 1;
            else if(result > 0)
                lowerBound = mid + 1;
            else
            {
                index = mid;

                return true;
            }
        }

        index = lowerBound;

        return false;
    }

    /// <summary>
    ///     Searches a B-tree from the root for the given key, following Apple's SearchTree. Descends index nodes
    ///     via the greatest key less than or equal to the search key. On return the leaf node containing the key
    ///     position is provided along with the record index (the insertion point when no exact match exists;
    ///     it may equal the leaf's record count, meaning the position is at the start of the next leaf).
    /// </summary>
    ErrorNumber SearchBTree(NodeReader readNode, in BTHeaderRec header, KeyComparer compare, out uint leafNodeNumber,
                            out byte[] leafNodeData, out int recordIndex, out bool exactMatch)
    {
        leafNodeNumber = 0;
        leafNodeData   = null;
        recordIndex    = 0;
        exactMatch     = false;

        if(header.treeDepth == 0 || header.rootNode == 0) return ErrorNumber.NoSuchFile;

        uint nodeNumber = header.rootNode;
        int  level      = header.treeDepth;

        while(level >= 1)
        {
            if(nodeNumber == 0 || header.totalNodes != 0 && nodeNumber >= header.totalNodes)
                return ErrorNumber.InvalidArgument;

            ErrorNumber errno = readNode(nodeNumber, out byte[] nodeData);

            if(errno != ErrorNumber.NoError) return errno;

            if(nodeData.Length < BT_NODE_DESCRIPTOR_SIZE) return ErrorNumber.InvalidArgument;

            BTNodeDescriptor nodeDesc =
                Helpers.Marshal.ByteArrayToStructureBigEndian<BTNodeDescriptor>(nodeData, 0, BT_NODE_DESCRIPTOR_SIZE);

            // Corruption checks: the node's height must match the level we expect, and only the
            // bottom level may contain leaf nodes
            if(nodeDesc.height != level) return ErrorNumber.InvalidArgument;

            if(level == 1)
            {
                if(nodeDesc.kind != BTNodeKind.kBTLeafNode) return ErrorNumber.InvalidArgument;
            }
            else if(nodeDesc.kind != BTNodeKind.kBTIndexNode) return ErrorNumber.InvalidArgument;

            if(nodeDesc.numRecords == 0) return ErrorNumber.InvalidArgument;

            bool found = SearchBTreeNode(nodeData, header.nodeSize, nodeDesc.numRecords, compare, out int index);

            if(level == 1)
            {
                leafNodeNumber = nodeNumber;
                leafNodeData   = nodeData;
                recordIndex    = index;
                exactMatch     = found;

                return ErrorNumber.NoError;
            }

            // Index node: descend via the greatest key less than or equal to the search key.
            // If the search key is smaller than every key, still descend the leftmost child.
            if(!found && index != 0) index--;

            if(index >= nodeDesc.numRecords) index = nodeDesc.numRecords - 1;

            if(!TryGetRecordOffset(nodeData, header.nodeSize, nodeDesc.numRecords, index, out ushort recordOffset))
                return ErrorNumber.InvalidArgument;

            if(recordOffset + 2 > nodeData.Length) return ErrorNumber.InvalidArgument;

            var keyLength = BigEndianBitConverter.ToUInt16(nodeData, recordOffset);

            if(header.maxKeyLength != 0 && keyLength > header.maxKeyLength) return ErrorNumber.InvalidArgument;

            // In index nodes key data is padded to even length; the child pointer follows the key
            int childPtrOffset = recordOffset + 2 + keyLength + (keyLength & 1);

            if(childPtrOffset + 4 > nodeData.Length) return ErrorNumber.InvalidArgument;

            nodeNumber = BigEndianBitConverter.ToUInt32(nodeData, childPtrOffset);
            level--;
        }

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>
    ///     Iteratively walks leaf nodes forward via fLink starting at the given node and record index, calling
    ///     the visitor for each record. The walk stops when the visitor returns true, the leaf chain ends, or
    ///     more nodes than the tree contains have been visited (fLink cycle protection).
    /// </summary>
    ErrorNumber EnumerateLeafRecords(NodeReader readNode,         in BTHeaderRec header, uint startLeafNode,
                                     int        startRecordIndex, RecordVisitor  visitor)
    {
        uint currentNode  = startLeafNode;
        int  currentIndex = startRecordIndex;
        uint visitedNodes = 0;

        while(currentNode != 0)
        {
            if(header.totalNodes != 0 && ++visitedNodes > header.totalNodes) return ErrorNumber.InvalidArgument;

            ErrorNumber errno = readNode(currentNode, out byte[] nodeData);

            if(errno != ErrorNumber.NoError) return errno;

            if(nodeData.Length < BT_NODE_DESCRIPTOR_SIZE) return ErrorNumber.InvalidArgument;

            BTNodeDescriptor nodeDesc =
                Helpers.Marshal.ByteArrayToStructureBigEndian<BTNodeDescriptor>(nodeData, 0, BT_NODE_DESCRIPTOR_SIZE);

            if(nodeDesc.kind != BTNodeKind.kBTLeafNode) return ErrorNumber.InvalidArgument;

            for(int i = currentIndex; i < nodeDesc.numRecords; i++)
            {
                if(!TryGetRecordOffset(nodeData, header.nodeSize, nodeDesc.numRecords, i, out ushort recordOffset))
                    continue;

                if(visitor(nodeData, recordOffset)) return ErrorNumber.NoError;
            }

            currentNode  = nodeDesc.fLink;
            currentIndex = 0;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Performs a keyed range scan: descends the tree to the first record not less than the search key, then
    ///     enumerates records forward from that position until the visitor stops the walk.
    /// </summary>
    ErrorNumber SearchBTreeRange(NodeReader readNode, in BTHeaderRec header, KeyComparer compare, RecordVisitor visitor)
    {
        ErrorNumber errno = SearchBTree(readNode,
                                        in header,
                                        compare,
                                        out uint leafNodeNumber,
                                        out byte[] _,
                                        out int recordIndex,
                                        out bool _);

        if(errno == ErrorNumber.NoSuchFile) return ErrorNumber.NoError; // Empty tree, nothing to enumerate

        if(errno != ErrorNumber.NoError) return errno;

        return EnumerateLeafRecords(readNode, in header, leafNodeNumber, recordIndex, visitor);
    }

    /// <summary>
    ///     Creates a key comparer for the catalog B-tree following Apple's CompareExtendedCatalogKeys:
    ///     parent CNID first, then the node name using the volume's comparison rules. An empty name on either
    ///     side is ordered by name length.
    /// </summary>
    KeyComparer MakeCatalogKeyComparer(uint searchParentID, string searchName)
    {
        char[] searchChars = searchName.ToCharArray();

        return (nodeData, keyOffset) =>
        {
            // Catalog key: keyLength(2) + parentID(4) + nameLength(2) + UTF-16BE name
            if(keyOffset + 8 > nodeData.Length) return 1;

            var trialParentID = BigEndianBitConverter.ToUInt32(nodeData, keyOffset + 2);

            if(searchParentID != trialParentID) return searchParentID < trialParentID ? -1 : 1;

            var trialNameLength = BigEndianBitConverter.ToUInt16(nodeData, keyOffset + 6);

            if(keyOffset + 8 + trialNameLength * 2 > nodeData.Length) return 1;

            if(searchChars.Length == 0 || trialNameLength == 0) return searchChars.Length - trialNameLength;

            Span<char> trialName =
                trialNameLength <= 256 ? stackalloc char[trialNameLength] : new char[trialNameLength];

            HfsPlusUnicode.DecodeBigEndianName(nodeData.AsSpan(keyOffset + 8, trialNameLength * 2), trialName);

            return _isCaseSensitive
                       ? HfsPlusUnicode.BinaryCompare(searchChars, trialName)
                       : HfsPlusUnicode.FastUnicodeCompare(searchChars, trialName);
        };
    }

    /// <summary>
    ///     Creates a key comparer for the extents overflow B-tree following Apple's CompareExtentKeysPlus:
    ///     file CNID first, then fork type, then start block. Note the comparison order differs from the
    ///     on-disk field order (forkType is stored before fileID).
    /// </summary>
    static KeyComparer MakeExtentKeyComparer(uint searchFileID, byte searchForkType, uint searchStartBlock) =>
        (nodeData, keyOffset) =>
        {
            // Extent key: keyLength(2) + forkType(1) + pad(1) + fileID(4) + startBlock(4)
            if(keyOffset + 12 > nodeData.Length) return 1;

            byte trialForkType   = nodeData[keyOffset                                 + 2];
            var  trialFileID     = BigEndianBitConverter.ToUInt32(nodeData, keyOffset + 4);
            var  trialStartBlock = BigEndianBitConverter.ToUInt32(nodeData, keyOffset + 8);

            if(searchFileID != trialFileID) return searchFileID < trialFileID ? -1 : 1;

            if(searchForkType != trialForkType) return searchForkType < trialForkType ? -1 : 1;

            if(searchStartBlock != trialStartBlock) return searchStartBlock < trialStartBlock ? -1 : 1;

            return 0;
        };

    /// <summary>
    ///     Creates a key comparer for the attributes B-tree following Apple's hfs_attrkeycompare: file CNID
    ///     first, then attribute name (always binary, never case-folded), then start block.
    /// </summary>
    static KeyComparer MakeAttributeKeyComparer(uint searchFileID, string searchName, uint searchStartBlock)
    {
        char[] searchChars = searchName.ToCharArray();

        return (nodeData, keyOffset) =>
        {
            // Attribute key: keyLength(2) + pad(2) + fileID(4) + startBlock(4) + attrNameLen(2) + UTF-16BE name
            if(keyOffset + 14 > nodeData.Length) return 1;

            var trialFileID = BigEndianBitConverter.ToUInt32(nodeData, keyOffset + 4);

            if(searchFileID != trialFileID) return searchFileID < trialFileID ? -1 : 1;

            var trialNameLength = BigEndianBitConverter.ToUInt16(nodeData, keyOffset + 12);

            if(keyOffset + 14 + trialNameLength * 2 > nodeData.Length) return 1;

            Span<char> trialName =
                trialNameLength <= 128 ? stackalloc char[trialNameLength] : new char[trialNameLength];

            HfsPlusUnicode.DecodeBigEndianName(nodeData.AsSpan(keyOffset + 14, trialNameLength * 2), trialName);

            int nameOrder = HfsPlusUnicode.BinaryCompare(searchChars, trialName);

            if(nameOrder != 0) return nameOrder;

            var trialStartBlock = BigEndianBitConverter.ToUInt32(nodeData, keyOffset + 8);

            if(searchStartBlock != trialStartBlock) return searchStartBlock < trialStartBlock ? -1 : 1;

            return 0;
        };
    }

    /// <summary>
    ///     Reads a B-tree node from a special file by walking a list of its extents. The list is normally the
    ///     8 inline extents of the fork; callers that support fragmented special files pass a merged list that
    ///     also contains the overflow extents.
    /// </summary>
    ErrorNumber ReadBTreeNode(System.Collections.Generic.IList<HFSPlusExtentDescriptor> extents,    ushort     nodeSize,
                              uint                                                      nodeNumber, out byte[] nodeData)
    {
        nodeData = null;

        if(extents == null || extents.Count == 0 || nodeSize == 0) return ErrorNumber.InvalidArgument;

        // Calculate byte offset of node within the fork
        ulong nodeOffset = (ulong)nodeNumber * nodeSize;

        // Find which extent contains this offset
        ulong currentOffset = 0;

        foreach(HFSPlusExtentDescriptor extent in extents)
        {
            if(extent.blockCount == 0) break;

            ulong extentSizeInBytes = (ulong)extent.blockCount * _volumeHeader.blockSize;

            if(nodeOffset < currentOffset + extentSizeInBytes)
            {
                // Found the extent containing this node
                ulong offsetInExtent = nodeOffset                                         - currentOffset;
                ulong blockOffset    = (ulong)extent.startBlock * _volumeHeader.blockSize + offsetInExtent;

                // Convert to device sector address
                // For wrapped volumes, blocks start after the HFS+ volume offset
                // For pure HFS+, _hfsPlusVolumeOffset is 0
                ulong deviceSector = ((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffset) /
                                     _sectorSize;

                var byteOffset = (uint)(((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffset) %
                                        _sectorSize);

                uint sectorsToRead = (nodeSize + byteOffset + _sectorSize - 1) / _sectorSize;

                ErrorNumber errno = _imagePlugin.ReadSectors(deviceSector,
                                                             false,
                                                             sectorsToRead,
                                                             out byte[] sectorData,
                                                             out _);

                if(errno != ErrorNumber.NoError) return errno;

                if(sectorData.Length < byteOffset + nodeSize) return ErrorNumber.InvalidArgument;

                nodeData = new byte[nodeSize];
                Array.Copy(sectorData, (int)byteOffset, nodeData, 0, nodeSize);

                return ErrorNumber.NoError;
            }

            currentOffset += extentSizeInBytes;
        }

        return ErrorNumber.InvalidArgument;
    }
}