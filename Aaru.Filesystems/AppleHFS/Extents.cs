// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Extents.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
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

//
// EXTENTS OVERFLOW FILE - HFS FILE EXTENT MANAGEMENT
// ====================================================
//
// Overview:
// The HFS File Manager tracks which allocation blocks belong to a file by maintaining
// a list of extents (contiguous ranges of allocation blocks). Each extent is represented
// by a pair of values: the starting allocation block and the number of allocation blocks.
//
// Extent Storage Strategy:
// 1. SMALL FILES (up to 3 extents):
//    The first 3 extents are stored directly in the catalog file record:
//    - Data fork: stored in CdrFilRec.filExtRec (ExtDataRec)
//    - Resource fork: stored in CdrFilRec.filRExtRec (ExtDataRec)
//    Each ExtDataRec contains 3 ExtDescriptor structures (8 bytes each = 12 bytes total)
//    If a file has fewer than 3 extents, unused extents have xdrNumABlks = 0
//
// 2. LARGE FILES (more than 3 extents):
//    Additional extents are stored in the Extents Overflow File, organized as a B-Tree.
//    The File Manager only needs to read the extents overflow file for extents beyond
//    the first three; this minimizes disk I/O for small files.
//
// Extents Overflow File B-Tree Structure:
// ========================================
//
// EXTENT KEY FORMAT (used in both index and leaf nodes):
// - keyLen (1 byte): Length of key data after this field (value = 7, indicating 7 bytes)
// - forkType (1 byte): 0x00 for data fork, 0xFF for resource fork
// - fileNumber (4 bytes): File ID (CNID) of the file
// - startFileABN (2 bytes): Starting file allocation block number (FAB) within the file
//   This represents the allocation block offset within the file where this extent record starts.
//   Total key size: 1 + 1 + 4 + 2 = 8 bytes (includes keyLen)
//
// EXTENT DATA RECORD (data portion of leaf node records):
// Contains 3 ExtDescriptor structures for the file starting at the specified FAB:
// - xdrStABN (2 bytes): First allocation block on disk for this extent
// - xdrNumABlks (2 bytes): Number of allocation blocks in this extent
// - (repeated 3 times = 12 bytes total)
//
// Traversing the Extents Overflow B-Tree:
// =======================================
//
// 1. START: Read MDB for extents B-Tree location and initial extent records
// 2. INDEX NODES: Navigate using extent keys (fileId + startBlock as search key)
// 3. LEAF NODES: Extract extent records matching the target file ID and fork type
// 4. FORWARD LINK: Use ndFLink to traverse sibling leaf nodes for additional extents
//
// Key Characteristics:
// - Extent records are ordered by (fileNumber, forkType, startFileABN)
// - All extents for a single file/fork are stored consecutively in the B-Tree
// - Each leaf record contains up to 3 extents for a given (file, fork, startBlock)
// - Leaf node forward links allow efficient iteration through all extents
//
// Example - Reconstructing All File Extents:
// ==========================================
//
// For a file with CNID=100 and 5 data fork extents:
//
// Step 1: From catalog file, read CdrFilRec.filExtRec (ExtDataRec)
//         This contains the first 3 extents
//
// Step 2: Search extents overflow B-Tree for key (fileId=100, fork=DATA, startBlock=3)
//         This retrieves the 4th extent (stored in next ExtDataRec in overflow)
//
// Step 3: Search extents overflow B-Tree for key (fileId=100, fork=DATA, startBlock=4)
//         This retrieves the 5th extent
//
// Step 4: Search for key (fileId=100, fork=DATA, startBlock=5)
//         Returns NoSuchFile when no more extents exist
//
// This approach ensures:
// - Fast access for small files (no B-Tree lookup needed)
// - Efficient iteration through large file extents
// - Minimal memory footprint (extent records are read on-demand)
// - Proper handling of fragmented files
//

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
public sealed partial class AppleHFS
{
    /// <summary>Cache of extent records by file ID and fork type</summary>
    Dictionary<(uint fileId, ForkType forkType), List<ExtDescriptor>> _extentCache;

    /// <summary>Safely validates and initializes an ExtDataRec structure</summary>
    private static ExtDataRec EnsureValidExtDataRec(ExtDataRec extents)
    {
        // If the xdr array is null, create a new one with all zeros
        if(extents.xdr != null) return extents;

        extents.xdr = new ExtDescriptor[3];

        for(var i = 0; i < 3; i++)
        {
            extents.xdr[i].xdrStABN    = 0;
            extents.xdr[i].xdrNumABlks = 0;
        }

        return extents;
    }

    /// <summary>Initializes the extents overflow file B-Tree header information</summary>
    ErrorNumber InitializeExtentsOverflowBTree()
    {
        // Get the extents B-Tree header information from the MDB
        // The extents B-Tree extent records are stored in drXTExtRec
        // The extents B-Tree total size is stored in drXTFlSize
        // We need to read the B-Tree header node (node 0) to get tree information

        ErrorNumber errno = ReadExtentsHeader(out BTHdrRed bthdr);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Failed to read extents B-Tree header");

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME,
                          $"Extents B-Tree: depth={bthdr.bthDepth}, rootNode={bthdr.bthRoot}, " +
                          $"nodeSize={bthdr.bthNodeSize}");

        _extentsBTreeHeader = bthdr;
        _extentCache        = [];

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the extents B-Tree header node</summary>
    ErrorNumber ReadExtentsHeader(out BTHdrRed bthdr)
    {
        bthdr = default(BTHdrRed);

        // Read node 0 (header node) using the extent information from MDB
        // ReadNode uses a fixed 512-byte read, which is enough for the header
        ErrorNumber errno = ReadNode(0, _mdb.drXTExtRec, out byte[] nodeData);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Failed to read extents B-Tree header node");

            return errno;
        }

        int nodeDescSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NodeDescriptor));

        // Verify this is a header node
        NodeDescriptor nodeDesc = Marshal.ByteArrayToStructureBigEndian<NodeDescriptor>(nodeData, 0, nodeDescSize);

        if(nodeDesc.ndType != NodeType.ndHdrNode)
        {
            // On volumes with no overflow extents, the extents B-tree may be allocated but zeroed
            // out (no header node written). Only a fully zeroed node qualifies as that case; any
            // other content means we misread the device (a zeroed buffer decodes as ndIndxNode)
            // or the volume is corrupt, and it must surface as an error, not an empty tree.
            if(nodeData.All(b => b == 0))
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "ReadExtentsHeader: node 0 is all zeroes, treating as empty extents tree");

                // Return a zeroed header — caller will see depth=0 and know tree is empty
                return ErrorNumber.NoError;
            }

            AaruLogging.Debug(MODULE_NAME, $"ReadExtentsHeader: expected header node, got type={nodeDesc.ndType}");

            return ErrorNumber.InvalidArgument;
        }

        if(nodeDesc.ndNHeight != 0 || nodeDesc.ndNRecs != 3)
        {
            AaruLogging.Debug(MODULE_NAME,
                              $"ReadExtentsHeader: invalid header node, height={nodeDesc.ndNHeight}, nRecs={nodeDesc.ndNRecs}");

            return ErrorNumber.InvalidArgument;
        }

        // B-Tree header record starts immediately after the node descriptor (14 bytes)

        if(nodeData.Length < nodeDescSize + Marshal.SizeOf<BTHdrRed>()) return ErrorNumber.InvalidArgument;

        bthdr = Marshal.ByteArrayToStructureBigEndian<BTHdrRed>(nodeData, nodeDescSize, Marshal.SizeOf<BTHdrRed>());

        // Validate, following Apple's VerifyHeader (lf_hfs_btree_misc_ops.c) and fsck checks
        if(bthdr.bthNodeSize is not (512 or 1024 or 2048 or 4096 or 8192 or 16384 or 32768))
        {
            AaruLogging.Debug(MODULE_NAME, $"ReadExtentsHeader: invalid node size {bthdr.bthNodeSize}");

            return ErrorNumber.InvalidArgument;
        }

        // Empty tree invariant: depth and root are both zero or both non-zero
        if(bthdr.bthDepth == 0 != (bthdr.bthRoot == 0))
        {
            AaruLogging.Debug(MODULE_NAME,
                              $"ReadExtentsHeader: inconsistent empty tree, depth={bthdr.bthDepth}, root={bthdr.bthRoot}");

            return ErrorNumber.InvalidArgument;
        }

        if(bthdr.bthNNodes > 0 &&
           (bthdr.bthRoot >= bthdr.bthNNodes || bthdr.bthFNode >= bthdr.bthNNodes || bthdr.bthLNode >= bthdr.bthNNodes))
        {
            AaruLogging.Debug(MODULE_NAME,
                              $"ReadExtentsHeader: node number out of range, root={bthdr.bthRoot}, fNode={bthdr.bthFNode}, lNode={bthdr.bthLNode}, totalNodes={bthdr.bthNNodes}");

            return ErrorNumber.InvalidArgument;
        }

        // Extents max key length must be 7
        if(bthdr.bthKeyLen is 0 or 7) return ErrorNumber.NoError;

        AaruLogging.Debug(MODULE_NAME, $"ReadExtentsHeader: invalid extents max_key_len {bthdr.bthKeyLen}");

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Reads a node using extents from an extent descriptor record</summary>
    ErrorNumber ReadNode(uint nodeNumber, ExtDataRec extents, out byte[] nodeData)
    {
        nodeData = null;

        // Ensure the extents struct is properly initialized
        extents = EnsureValidExtDataRec(extents);

        const uint nodeSize = 512; // Default HFS node size

        // Calculate the byte offset of this node within the file
        ulong nodeByteOffset = (ulong)nodeNumber * nodeSize;

        // Walk through extents to find the one containing this byte offset
        ulong extentFileOffset = 0;

        for(var i = 0; i < 3; i++)
        {
            if(extents.xdr[i].xdrNumABlks == 0) break;

            ulong extentSizeBytes = (ulong)extents.xdr[i].xdrNumABlks * _mdb.drAlBlkSiz;

            if(nodeByteOffset >= extentFileOffset + extentSizeBytes)
            {
                extentFileOffset += extentSizeBytes;

                continue;
            }

            // Found the extent containing this node
            ulong offsetInExtent  = nodeByteOffset                                   - extentFileOffset;
            ulong blockByteOffset = (ulong)extents.xdr[i].xdrStABN * _mdb.drAlBlkSiz + offsetInExtent;
            ulong hfsSector512    = _mdb.drAlBlSt                                    + blockByteOffset / 512;

            HfsOffsetToDeviceSector(hfsSector512, out ulong deviceSector, out uint byteOffset);

            // Carry the sub-512 remainder dropped by the truncating division above
            byteOffset += (uint)(blockByteOffset % 512);

            AaruLogging.Debug(MODULE_NAME,
                              $"ReadNode: node={nodeNumber}, extent=({extents.xdr[i].xdrStABN},{extents.xdr[i].xdrNumABlks}), " +
                              $"hfsSector512={hfsSector512}, deviceSector={deviceSector}, byteOffset={byteOffset}");

            uint sectorsNeeded = (nodeSize + byteOffset + _sectorSize - 1) / _sectorSize;

            ErrorNumber errno =
                _imagePlugin.ReadSectors(deviceSector, false, sectorsNeeded, out byte[] sectorData, out _);

            if(errno != ErrorNumber.NoError) return errno;

            if(sectorData == null || sectorData.Length < (int)byteOffset + nodeSize) return ErrorNumber.InvalidArgument;

            nodeData = new byte[nodeSize];
            Array.Copy(sectorData, (int)byteOffset, nodeData, 0, nodeSize);

            return ErrorNumber.NoError;
        }

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Gets all extents for a file by searching the extents B-Tree</summary>
    ErrorNumber GetFileExtents(uint                    fileId, ForkType forkType, ExtDataRec firstExtents,
                               out List<ExtDescriptor> allExtents)
    {
        allExtents = [];

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Check cache first
        (uint fileId, ForkType forkType) cacheKey = (fileId, forkType);

        if(_extentCache.TryGetValue(cacheKey, out List<ExtDescriptor> cached))
        {
            allExtents = cached;

            return ErrorNumber.NoError;
        }

        // First, add the three extents that are stored in the catalog record
        // Ensure xdr array is properly initialized
        if(firstExtents.xdr != null)
        {
            for(var i = 0; i < 3; i++)
            {
                if(i >= firstExtents.xdr.Length || firstExtents.xdr[i].xdrNumABlks == 0) break;

                allExtents.Add(firstExtents.xdr[i]);

                AaruLogging.Debug(MODULE_NAME,
                                  "Adding extent from catalog: startBlock={0}, numBlocks={1}",
                                  firstExtents.xdr[i].xdrStABN,
                                  firstExtents.xdr[i].xdrNumABlks);
            }
        }

        // If all three initial extents are full, we might have overflow extents
        bool hasThreeExtents = firstExtents.xdr?.Length >= 3 && firstExtents.xdr[2].xdrNumABlks != 0;

        if(hasThreeExtents && _extentsBTreeHeader.bthDepth > 0)
        {
            // Calculate the starting FABN for overflow: sum of first 3 extents' block counts
            ushort firstBlocks = 0;

            for(var i = 0; i < 3; i++) firstBlocks += firstExtents.xdr[i].xdrNumABlks;

            ErrorNumber errno =
                SearchExtentsOverflowBTree(fileId, forkType, firstBlocks, out List<ExtDescriptor> overflowExtents);

            if(errno != ErrorNumber.NoError)
            {
                // If no overflow extents found, it's ok - file just has 3 extents
                if(errno != ErrorNumber.NoSuchFile) return errno;
            }
            else if(overflowExtents is { Count: > 0 })
            {
                // Combine initial and overflow extents
                allExtents.AddRange(overflowExtents);

                AaruLogging.Debug(MODULE_NAME,
                                  "Added {0} overflow extents for file {1}",
                                  overflowExtents.Count,
                                  fileId);
            }
        }

        if(allExtents.Count == 0) return ErrorNumber.NoSuchFile;

        // Cache the result
        _extentCache[cacheKey] = allExtents;

        return ErrorNumber.NoError;
    }

    /// <summary>Searches the extents overflow B-Tree for extent records of a specific file</summary>
    ErrorNumber SearchExtentsOverflowBTree(uint                    fileId, ForkType forkType, ushort startFabn,
                                           out List<ExtDescriptor> extents)
    {
        extents = [];

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(_extentsBTreeHeader.bthRoot == 0 || _extentsBTreeHeader.bthDepth == 0) return ErrorNumber.NoSuchFile;

        ushort searchFabn = startFabn;

        for(;;)
        {
            byte[]          searchKey = ExtBuildKey(fileId, searchFabn, (byte)forkType);
            ExtentsFindData fd        = default;

            ErrorNumber errno = ExtBTreeFind(searchKey, ref fd);

            if(errno != ErrorNumber.NoError) break;

            // Verify the found record matches our file and fork
            if(fd.EntryOffset + 12 > fd.NodeData.Length) break;

            // Read the key from the found record to verify match
            var  foundFileId = BigEndianBitConverter.ToUInt32(fd.NodeData, fd.KeyOffset + 2);
            byte foundFork   = fd.NodeData[fd.KeyOffset                                 + 1];

            if(foundFileId != fileId || foundFork != (byte)forkType) break;

            // Extract the 3 extents from the data record
            int dataOff = fd.EntryOffset;

            for(var j = 0; j < 3; j++)
            {
                if(dataOff + j * 4 + 4 > fd.NodeData.Length) break;

                var startBlock = BigEndianBitConverter.ToUInt16(fd.NodeData, dataOff + j * 4);
                var numBlocks  = BigEndianBitConverter.ToUInt16(fd.NodeData, dataOff + j * 4 + 2);

                if(numBlocks == 0) break;

                extents.Add(new ExtDescriptor
                {
                    xdrStABN    = startBlock,
                    xdrNumABlks = numBlocks
                });

                searchFabn += numBlocks;
            }

            // If the record had fewer than 3 extents, no more overflow
            if(extents.Count == 0 || extents[^1].xdrNumABlks == 0) break;

            // Search for next overflow record at updated FABN
        }

        return extents.Count == 0 ? ErrorNumber.NoSuchFile : ErrorNumber.NoError;
    }

#region Extents B-tree primitives

    /// <summary>Build an extents B-tree search key</summary>
    static byte[] ExtBuildKey(uint fileId, ushort fabn, byte forkType)
    {
        // Key layout: [keyLen(1)] [forkType(1)] [fileNum(4,BE)] [FABN(2,BE)]
        var key = new byte[8];
        key[0] = 7; // keyLen = 7 bytes after this field
        key[1] = forkType;
        key[2] = (byte)(fileId >> 24);
        key[3] = (byte)(fileId >> 16);
        key[4] = (byte)(fileId >> 8);
        key[5] = (byte)fileId;
        key[6] = (byte)(fabn >> 8);
        key[7] = (byte)fabn;

        return key;
    }

    /// <summary>Compare two extents B-tree keys</summary>
    static int ExtKeyCmp(byte[] key1, int off1, byte[] key2, int off2)
    {
        // FNum at offset +2, 4 bytes BE
        var fnum1 = BigEndianBitConverter.ToUInt32(key1, off1 + 2);
        var fnum2 = BigEndianBitConverter.ToUInt32(key2, off2 + 2);

        if(fnum1 != fnum2) return fnum1 < fnum2 ? -1 : 1;

        // FkType at offset +1
        byte fk1 = key1[off1 + 1];
        byte fk2 = key2[off2 + 1];

        if(fk1 != fk2) return fk1 < fk2 ? -1 : 1;

        // FABN at offset +6, 2 bytes BE
        var fabn1 = BigEndianBitConverter.ToUInt16(key1, off1 + 6);
        var fabn2 = BigEndianBitConverter.ToUInt16(key2, off2 + 6);

        if(fabn1 == fabn2) return 0;

        return fabn1 < fabn2 ? -1 : 1;
    }

    /// <summary>Search state for extents B-tree</summary>
    struct ExtentsFindData
    {
        public byte[] NodeData;
        public int    Record;
        public int    KeyOffset;
        public int    EntryOffset;
        public bool   ExactMatch;
    }

    /// <summary>Read an extents B-tree node by number</summary>
    ErrorNumber ReadExtentsNode(uint nodeNumber, out byte[] nodeData)
    {
        nodeData = null;

        int   nodeSize       = _extentsBTreeHeader.bthNodeSize;
        ulong nodeByteOffset = (ulong)nodeNumber * (uint)nodeSize;

        if(_mdb.drXTExtRec.xdr == null) return ErrorNumber.InvalidArgument;

        ulong extentFileOffset = 0;

        foreach(ExtDescriptor ext in _mdb.drXTExtRec.xdr)
        {
            if(ext.xdrNumABlks == 0) break;

            ulong extentSizeBytes = (ulong)ext.xdrNumABlks * _mdb.drAlBlkSiz;

            if(nodeByteOffset >= extentFileOffset + extentSizeBytes)
            {
                extentFileOffset += extentSizeBytes;

                continue;
            }

            ulong offsetInExtent  = nodeByteOffset                        - extentFileOffset;
            ulong blockByteOffset = (ulong)ext.xdrStABN * _mdb.drAlBlkSiz + offsetInExtent;
            ulong hfsSector512    = _mdb.drAlBlSt                         + blockByteOffset / 512;

            HfsOffsetToDeviceSector(hfsSector512, out ulong deviceSector, out uint byteOffset);

            // Carry the sub-512 remainder dropped by the truncating division above
            byteOffset += (uint)(blockByteOffset % 512);

            uint sectorsToRead = ((uint)nodeSize + byteOffset + _sectorSize - 1) / _sectorSize;

            ErrorNumber errno =
                _imagePlugin.ReadSectors(deviceSector, false, sectorsToRead, out byte[] sectorData, out _);

            if(errno != ErrorNumber.NoError) return errno;

            if(sectorData.Length < (int)byteOffset + nodeSize) return ErrorNumber.InvalidArgument;

            nodeData = new byte[nodeSize];
            Array.Copy(sectorData, (int)byteOffset, nodeData, 0, nodeSize);

            return ErrorNumber.NoError;
        }

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Get key length for an extents B-tree record</summary>
    int ExtBRecKeyLen(byte[] nodeData, int nodeSize, int rec, NodeType nodeType)
    {
        if(nodeType != NodeType.ndIndxNode && nodeType != NodeType.ndLeafNode) return 0;

        // HFS extents: index nodes use fixed max_key_len + 1
        if(nodeType == NodeType.ndIndxNode) return _extentsBTreeHeader.bthKeyLen + 1;

        // Leaf node: variable key length = (keyLenByte | 1) + 1
        int recOffPos = nodeSize - (rec + 1) * 2;

        if(recOffPos < 0 || recOffPos + 2 > nodeData.Length) return 0;

        int recOff = BigEndianBitConverter.ToUInt16(nodeData, recOffPos);

        if(recOff == 0 || recOff >= nodeSize) return 0;

        byte keyByte = nodeData[recOff];
        int  keyLen  = (keyByte | 1) + 1;

        return keyLen > _extentsBTreeHeader.bthKeyLen + 1 ? 0 : keyLen;
    }

    /// <summary>Binary search within a single extents B-tree node</summary>
    ErrorNumber ExtBRecFind(byte[] nodeData, int nodeSize, NodeType nodeType, byte[] searchKey, ref ExtentsFindData fd)
    {
        int ndDescSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NodeDescriptor));

        NodeDescriptor desc = Marshal.ByteArrayToStructureBigEndian<NodeDescriptor>(nodeData, 0, ndDescSize);

        var b   = 0;
        int e   = desc.ndNRecs - 1;
        int off = 0, len = 0, keyLen = 0;
        var rec = 0;

        fd.ExactMatch = false;

        while(b <= e)
        {
            rec = (e + b) / 2;

            if(!BRecLenOff(nodeData, nodeSize, rec, out off, out len)) return ErrorNumber.InvalidArgument;

            keyLen = ExtBRecKeyLen(nodeData, nodeSize, rec, nodeType);

            if(keyLen == 0) return ErrorNumber.InvalidArgument;

            int cmpVal = ExtKeyCmp(nodeData, off, searchKey, 0);

            if(cmpVal == 0)
            {
                e             = rec;
                fd.ExactMatch = true;

                break;
            }

            if(cmpVal < 0)
                b = rec + 1;
            else
                e = rec - 1;
        }

        if(!fd.ExactMatch && rec != e && e >= 0)
        {
            if(!BRecLenOff(nodeData, nodeSize, e, out off, out len)) return ErrorNumber.InvalidArgument;

            keyLen = ExtBRecKeyLen(nodeData, nodeSize, e, nodeType);

            if(keyLen == 0) return ErrorNumber.InvalidArgument;
        }

        fd.Record      = e;
        fd.KeyOffset   = off;
        fd.EntryOffset = off + keyLen;

        return ErrorNumber.NoError;
    }

    /// <summary>Search the extents B-tree from root to leaf</summary>
    ErrorNumber ExtBTreeFind(byte[] searchKey, ref ExtentsFindData fd)
    {
        uint nidx = _extentsBTreeHeader.bthRoot;

        if(nidx == 0) return ErrorNumber.NoSuchFile;

        int nodeSize   = _extentsBTreeHeader.bthNodeSize;
        int height     = _extentsBTreeHeader.bthDepth;
        int ndDescSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NodeDescriptor));

        for(;;)
        {
            ErrorNumber errno = ReadExtentsNode(nidx, out byte[] nodeData);

            if(errno != ErrorNumber.NoError) return errno;

            NodeDescriptor desc = Marshal.ByteArrayToStructureBigEndian<NodeDescriptor>(nodeData, 0, ndDescSize);

            if(desc.ndNHeight != height) return ErrorNumber.InvalidArgument;

            height--;
            NodeType expectedType = height > 0 ? NodeType.ndIndxNode : NodeType.ndLeafNode;

            if(desc.ndType != expectedType) return ErrorNumber.InvalidArgument;

            errno = ExtBRecFind(nodeData, nodeSize, desc.ndType, searchKey, ref fd);

            if(errno != ErrorNumber.NoError) return errno;

            fd.NodeData = nodeData;

            if(height == 0) return fd.ExactMatch ? ErrorNumber.NoError : ErrorNumber.NoSuchFile;

            if(fd.Record < 0) return ErrorNumber.InvalidArgument;

            if(fd.EntryOffset + 4 > nodeData.Length) return ErrorNumber.InvalidArgument;

            nidx = BigEndianBitConverter.ToUInt32(nodeData, fd.EntryOffset);
        }
    }

#endregion
}