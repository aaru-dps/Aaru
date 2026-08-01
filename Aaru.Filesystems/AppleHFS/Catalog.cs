// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Catalog.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Aaru.Filesystems;

public sealed partial class AppleHFS
{
    /// <summary>
    ///     Macintosh case-insensitive character ordering table.
    /// </summary>
    static readonly byte[] _caseOrder =
    [
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11,
        0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x22, 0x23, 0x28,
        0x29, 0x2A, 0x2B, 0x2C, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C,
        0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x57, 0x59, 0x5D, 0x5F, 0x66, 0x68,
        0x6A, 0x6C, 0x72, 0x74, 0x76, 0x78, 0x7A, 0x7E, 0x8C, 0x8E, 0x90, 0x92, 0x95, 0x97, 0x9E, 0xA0, 0xA2, 0xA4,
        0xA7, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0x4E, 0x48, 0x57, 0x59, 0x5D, 0x5F, 0x66, 0x68, 0x6A, 0x6C, 0x72, 0x74,
        0x76, 0x78, 0x7A, 0x7E, 0x8C, 0x8E, 0x90, 0x92, 0x95, 0x97, 0x9E, 0xA0, 0xA2, 0xA4, 0xA7, 0xAF, 0xB0, 0xB1,
        0xB2, 0xB3, 0x4A, 0x4C, 0x5A, 0x60, 0x7B, 0x7F, 0x98, 0x4F, 0x49, 0x51, 0x4A, 0x4B, 0x4C, 0x5A, 0x60, 0x63,
        0x64, 0x65, 0x6E, 0x6F, 0x70, 0x71, 0x7B, 0x84, 0x85, 0x86, 0x7F, 0x80, 0x9A, 0x9B, 0x9C, 0x98, 0xB4, 0xB5,
        0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0x94, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF, 0xC0, 0x4D, 0x81, 0xC1, 0xC2, 0xC3, 0xC4,
        0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0x55, 0x8A, 0xCC, 0x4D, 0x81, 0xCD, 0xCE, 0xCF, 0xD0, 0xD1, 0xD2,
        0xD3, 0x26, 0x27, 0xD4, 0x20, 0x49, 0x4B, 0x80, 0x82, 0x82, 0xD5, 0xD6, 0x24, 0x25, 0x2D, 0x2E, 0xD7, 0xD8,
        0xA6, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9,
        0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB,
        0xFC, 0xFD, 0xFE, 0xFF
    ];

#region B-tree search state

    /// <summary>B-tree search state</summary>
    struct CatalogFindData
    {
        /// <summary>Raw data of the current node</summary>
        public byte[] NodeData;
        /// <summary>Number of the current node</summary>
        public uint NodeNumber;
        /// <summary>Index of the current record within the node</summary>
        public int Record;
        /// <summary>Byte offset of the key within the node</summary>
        public int KeyOffset;
        /// <summary>Length of the key in bytes</summary>
        public int KeyLength;
        /// <summary>Byte offset of the data entry (after key) within the node</summary>
        public int EntryOffset;
        /// <summary>Length of the data entry in bytes</summary>
        public int EntryLength;
        /// <summary>Whether an exact match was found</summary>
        public bool ExactMatch;
    }

#endregion

#region Node I/O

    /// <summary>Read a catalog B-tree node by number, handling extents overflow</summary>
    ErrorNumber ReadCatalogNode(uint nodeNumber, out byte[] nodeData)
    {
        nodeData = null;

        int   nodeSize       = _catalogBTreeHeader.bthNodeSize;
        ulong nodeByteOffset = (ulong)nodeNumber * (uint)nodeSize;

        // First try the 3 extents from the MDB
        if(_mdb.drCTExtRec.xdr == null) return ErrorNumber.InvalidArgument;

        ErrorNumber result = ReadNodeFromExtents(_mdb.drCTExtRec.xdr, nodeByteOffset, nodeSize, out nodeData);

        if(result == ErrorNumber.NoError) return ErrorNumber.NoError;

        // Node not in the first 3 extents — search the extents overflow B-tree
        // for additional catalog file extents (CNID = kCatalogFileCnid = 4)
        if(_extentsBTreeHeader.bthDepth == 0) return ErrorNumber.InvalidArgument;

        // Count blocks in the first 3 extents to know the starting FABN for overflow
        ushort firstBlocks = _mdb.drCTExtRec.xdr.TakeWhile(static ext => ext.xdrNumABlks != 0)
                                 .Aggregate<ExtDescriptor, ushort>(0,
                                                                   static (current, ext) =>
                                                                       (ushort)(current + ext.xdrNumABlks));

        // Build the complete extent list for the catalog file
        ErrorNumber errno = SearchExtentsOverflowBTree(kCatalogFileCnid,
                                                       ForkType.Data,
                                                       firstBlocks,
                                                       out List<ExtDescriptor> overflowExtents);

        if(errno != ErrorNumber.NoError) return ErrorNumber.InvalidArgument;

        // Adjust byte offset: subtract the bytes covered by the first 3 extents
        ulong firstExtentsBytes = (ulong)firstBlocks * _mdb.drAlBlkSiz;

        if(nodeByteOffset < firstExtentsBytes) return ErrorNumber.InvalidArgument;

        ulong adjustedOffset = nodeByteOffset - firstExtentsBytes;

        return ReadNodeFromExtents(overflowExtents, adjustedOffset, nodeSize, out nodeData);
    }

    /// <summary>Read a node from a list of extent descriptors at a given byte offset within them</summary>
    ErrorNumber ReadNodeFromExtents(IReadOnlyList<ExtDescriptor> extents, ulong byteOffsetInExtents, int nodeSize,
                                    out byte[]                   nodeData)
    {
        nodeData = null;

        ulong extentFileOffset = 0;

        foreach(ExtDescriptor ext in extents)
        {
            if(ext.xdrNumABlks == 0) break;

            ulong extentSizeBytes = (ulong)ext.xdrNumABlks * _mdb.drAlBlkSiz;

            if(byteOffsetInExtents >= extentFileOffset + extentSizeBytes)
            {
                extentFileOffset += extentSizeBytes;

                continue;
            }

            // Found the extent containing this node
            ulong offsetInExtent  = byteOffsetInExtents                   - extentFileOffset;
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

#endregion

#region B-tree primitives

    /// <summary>Compare two HFS strings using Macintosh lexical ordering</summary>
    static int HfsStrCmp(byte[] s1, int off1, int len1, byte[] s2, int off2, int len2)
    {
        if(len1 < 0) len1 = 0;
        if(len2 < 0) len2 = 0;

        int len = Math.Min(len1, len2);

        for(var i = 0; i < len; i++)
        {
            int diff = _caseOrder[s1[off1 + i]] - _caseOrder[s2[off2 + i]];

            if(diff != 0) return diff;
        }

        return len1 - len2;
    }

    /// <summary>Compare two catalog B-tree keys: first by ParID, then by name</summary>
    static int CatKeyCmp(byte[] key1, int off1, byte[] key2, int off2)
    {
        // ParID is at offset +2 (after keyLen and reserved), 4 bytes big-endian
        var parId1 = BigEndianBitConverter.ToUInt32(key1, off1 + 2);
        var parId2 = BigEndianBitConverter.ToUInt32(key2, off2 + 2);

        if(parId1 != parId2) return parId1 < parId2 ? -1 : 1;

        // Name: nameLen at offset +6, name bytes at offset +7
        byte nameLen1 = key1[off1 + 6];
        byte nameLen2 = key2[off2 + 6];

        return HfsStrCmp(key1, off1 + 7, nameLen1, key2, off2 + 7, nameLen2);
    }

    /// <summary>Build a catalog search key from a parent ID and optional name</summary>
    static byte[] CatBuildKey(uint parentId, byte[] name, byte nameLen)
    {
        // Key layout: [keyLen(1)] [reserved(1)] [ParID(4,BE)] [nameLen(1)] [name(0-31)]
        // Allocate max key size for safe comparison against index node keys
        var key        = new byte[38]; // max_key_len (37) + 1
        var keyDataLen = (byte)(6 + nameLen);
        key[0] = keyDataLen;
        key[1] = 0; // reserved
        key[2] = (byte)(parentId >> 24);
        key[3] = (byte)(parentId >> 16);
        key[4] = (byte)(parentId >> 8);
        key[5] = (byte)parentId;
        key[6] = nameLen;

        if(nameLen > 0 && name != null) Array.Copy(name, 0, key, 7, Math.Min(nameLen, (byte)31));

        return key;
    }

    /// <summary>Get the offset and length of a record within a B-tree node</summary>
    static bool BRecLenOff(byte[] nodeData, int nodeSize, int rec, out int off, out int len)
    {
        // Offset table is at end of node, growing backwards.
        // Record N's offset is at nodeSize - (N+1)*2.
        // We read 4 bytes at nodeSize - (rec+2)*2 to get offsets for rec+1 and rec.
        int dataOff = nodeSize - (rec + 2) * 2;

        if(dataOff < 0 || dataOff + 4 > nodeData.Length)
        {
            off = 0;
            len = 0;

            return false;
        }

        int nextOff = BigEndianBitConverter.ToUInt16(nodeData, dataOff);
        int recOff  = BigEndianBitConverter.ToUInt16(nodeData, dataOff + 2);

        off = recOff;
        len = nextOff - recOff;

        return len >= 0 && off >= 14 && off < nodeSize;
    }

    /// <summary>Get the total key length for a record (including the keyLen byte and padding)</summary>
    int BRecKeyLen(byte[] nodeData, int nodeSize, int rec, NodeType nodeType)
    {
        if(nodeType != NodeType.ndIndxNode && nodeType != NodeType.ndLeafNode) return 0;

        // HFS (no BIGKEYS, no VARIDXKEYS): index nodes use fixed max_key_len + 1
        if(nodeType == NodeType.ndIndxNode) return _catalogBTreeHeader.bthKeyLen + 1;

        // Leaf node: variable key length = (keyLenByte | 1) + 1
        int recOffPos = nodeSize - (rec + 1) * 2;

        if(recOffPos < 0 || recOffPos + 2 > nodeData.Length) return 0;

        int recOff = BigEndianBitConverter.ToUInt16(nodeData, recOffPos);

        if(recOff == 0 || recOff >= nodeSize) return 0;

        byte keyByte = nodeData[recOff];
        int  keyLen  = (keyByte | 1) + 1;

        return keyLen > _catalogBTreeHeader.bthKeyLen + 1 ? 0 : keyLen;
    }

#endregion

#region B-tree search

    /// <summary>Binary search within a single B-tree node for the best matching record</summary>
    /// <remarks>
    ///     On success, <paramref name="fd" /> points to the largest record whose key is &lt;= the search key.
    /// </remarks>
    ErrorNumber BRecFind(byte[] nodeData, int nodeSize, NodeType nodeType, byte[] searchKey, ref CatalogFindData fd)
    {
        int ndDescSize = Marshal.SizeOf(typeof(NodeDescriptor));

        NodeDescriptor desc = Helpers.Marshal.ByteArrayToStructureBigEndian<NodeDescriptor>(nodeData, 0, ndDescSize);

        var b   = 0;
        int e   = desc.ndNRecs - 1;
        int off = 0, len = 0, keyLen = 0;
        var rec = 0;

        fd.ExactMatch = false;

        // Binary search: find the record whose key best matches (not greater than) the search key
        while(b <= e)
        {
            rec = (e + b) / 2;

            if(!BRecLenOff(nodeData, nodeSize, rec, out off, out len)) return ErrorNumber.InvalidArgument;

            keyLen = BRecKeyLen(nodeData, nodeSize, rec, nodeType);

            if(keyLen == 0) return ErrorNumber.InvalidArgument;

            int cmpVal = CatKeyCmp(nodeData, off, searchKey, 0);

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

        // If the final position differs from e and e is valid, re-read offsets for record e
        if(!fd.ExactMatch && rec != e && e >= 0)
        {
            if(!BRecLenOff(nodeData, nodeSize, e, out off, out len)) return ErrorNumber.InvalidArgument;

            keyLen = BRecKeyLen(nodeData, nodeSize, e, nodeType);

            if(keyLen == 0) return ErrorNumber.InvalidArgument;
        }

        fd.Record      = e;
        fd.KeyOffset   = off;
        fd.KeyLength   = keyLen;
        fd.EntryOffset = off + keyLen;
        fd.EntryLength = len - keyLen;

        return ErrorNumber.NoError;
    }

    /// <summary>Search the catalog B-tree from root to leaf for a given key</summary>
    /// <remarks>
    ///     Returns <see cref="ErrorNumber.NoError" /> on exact match,
    ///     <see cref="ErrorNumber.NoSuchFile" /> when the key is not found (fd still populated).
    /// </remarks>
    ErrorNumber BTreeFind(byte[] searchKey, ref CatalogFindData fd)
    {
        uint nidx = _catalogBTreeHeader.bthRoot;

        if(nidx == 0) return ErrorNumber.NoSuchFile;

        int nodeSize   = _catalogBTreeHeader.bthNodeSize;
        int height     = _catalogBTreeHeader.bthDepth;
        int ndDescSize = Marshal.SizeOf(typeof(NodeDescriptor));

        for(;;)
        {
            ErrorNumber errno = ReadCatalogNode(nidx, out byte[] nodeData);

            if(errno != ErrorNumber.NoError) return errno;

            NodeDescriptor desc =
                Helpers.Marshal.ByteArrayToStructureBigEndian<NodeDescriptor>(nodeData, 0, ndDescSize);

            // Validate node height
            if(desc.ndNHeight != height)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "B*Tree inconsistency: expected height {0}, got {1} at node {2}",
                                  height,
                                  (int)desc.ndNHeight,
                                  nidx);

                return ErrorNumber.InvalidArgument;
            }

            // Decrement height and validate node type
            height--;
            NodeType expectedType = height > 0 ? NodeType.ndIndxNode : NodeType.ndLeafNode;

            if(desc.ndType != expectedType)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "B*Tree inconsistency: expected type {0}, got {1} at node {2}",
                                  expectedType,
                                  desc.ndType,
                                  nidx);

                return ErrorNumber.InvalidArgument;
            }

            // Binary search within this node
            errno = BRecFind(nodeData, nodeSize, desc.ndType, searchKey, ref fd);

            if(errno != ErrorNumber.NoError) return errno;

            fd.NodeData   = nodeData;
            fd.NodeNumber = nidx;

            // At leaf level — done
            if(height == 0) return fd.ExactMatch ? ErrorNumber.NoError : ErrorNumber.NoSuchFile;

            // Index node: must have a valid record to follow
            if(fd.Record < 0) return ErrorNumber.InvalidArgument;

            // Read child node pointer (4 bytes big-endian at EntryOffset)
            if(fd.EntryOffset + 4 > nodeData.Length) return ErrorNumber.InvalidArgument;

            nidx = BigEndianBitConverter.ToUInt32(nodeData, fd.EntryOffset);
        }
    }

    /// <summary>Move forward by <paramref name="cnt" /> records in the leaf node chain</summary>
    ErrorNumber BRecGoto(ref CatalogFindData fd, int cnt)
    {
        int nodeSize   = _catalogBTreeHeader.bthNodeSize;
        int ndDescSize = Marshal.SizeOf(typeof(NodeDescriptor));

        NodeDescriptor desc = Helpers.Marshal.ByteArrayToStructureBigEndian<NodeDescriptor>(fd.NodeData, 0, ndDescSize);

        while(cnt >= desc.ndNRecs - fd.Record)
        {
            cnt       -= desc.ndNRecs - fd.Record;
            fd.Record =  0;

            uint nextIdx = desc.ndFLink;

            if(nextIdx == 0) return ErrorNumber.NoSuchFile;

            ErrorNumber errno = ReadCatalogNode(nextIdx, out byte[] nodeData);

            if(errno != ErrorNumber.NoError) return errno;

            fd.NodeData   = nodeData;
            fd.NodeNumber = nextIdx;

            desc = Helpers.Marshal.ByteArrayToStructureBigEndian<NodeDescriptor>(nodeData, 0, ndDescSize);
        }

        fd.Record += cnt;

        // Update offsets for the new record position
        if(!BRecLenOff(fd.NodeData, nodeSize, fd.Record, out int off, out int len)) return ErrorNumber.InvalidArgument;

        int keyLen = BRecKeyLen(fd.NodeData, nodeSize, fd.Record, desc.ndType);

        if(keyLen == 0) return ErrorNumber.InvalidArgument;

        fd.KeyOffset   = off;
        fd.KeyLength   = keyLen;
        fd.EntryOffset = off + keyLen;
        fd.EntryLength = len - keyLen;

        return ErrorNumber.NoError;
    }

#endregion

#region Catalog operations

    /// <summary>
    ///     Find a catalog entry by its CNID by first finding its thread record,
    ///     then following the thread to the actual record.
    /// </summary>
    ErrorNumber CatFindBrec(uint cnid, ref CatalogFindData fd)
    {
        // Step 1: Build thread key (cnid, empty name) and find the thread record
        byte[] searchKey = CatBuildKey(cnid, null, 0);

        ErrorNumber errno = BTreeFind(searchKey, ref fd);

        if(errno != ErrorNumber.NoError) return errno;

        // Step 2: Read the thread record to get the parent ID and name
        int entryOff = fd.EntryOffset;

        if(entryOff + 15 > fd.NodeData.Length) return ErrorNumber.InvalidArgument;

        byte recordType = fd.NodeData[entryOff];

        if(recordType != kCatalogRecordTypeDirectoryThread && recordType != kCatalogRecordTypeFileThread)
        {
            AaruLogging.Debug(MODULE_NAME, "Bad thread record type {0} for CNID {1}", recordType, cnid);

            return ErrorNumber.InvalidArgument;
        }

        // Thread record layout:
        //   [0]    type (1 byte)
        //   [1]    reserved (1 byte)
        //   [2..9] thdResrv (8 bytes)
        //  [10..13] thdParID (4 bytes BE)
        //  [14]    thdCName length (1 byte)
        //  [15..]  thdCName data (up to 31 bytes)
        var  thdParId   = BigEndianBitConverter.ToUInt32(fd.NodeData, entryOff + 10);
        byte thdNameLen = fd.NodeData[entryOff                                 + 14];

        if(thdNameLen > 31)
        {
            AaruLogging.Debug(MODULE_NAME, "Bad catalog name length {0}", thdNameLen);

            return ErrorNumber.InvalidArgument;
        }

        var thdName = new byte[thdNameLen];

        if(thdNameLen > 0) Array.Copy(fd.NodeData, entryOff + 15, thdName, 0, thdNameLen);

        // Step 3: Build key with parent ID and name from thread, search again
        searchKey = CatBuildKey(thdParId, thdName, thdNameLen);

        return BTreeFind(searchKey, ref fd);
    }

    /// <summary>Parse a catalog record (directory or file) from raw node data</summary>
    static CatalogEntry ParseCatalogRecord(byte[] nodeData, int entryOffset, string name, uint parentId)
    {
        if(entryOffset >= nodeData.Length) return null;

        byte recordType = nodeData[entryOffset];

        switch(recordType)
        {
            case kCatalogRecordTypeDirectory:
            {
                int size = Marshal.SizeOf(typeof(CdrDirRec));

                if(entryOffset + size > nodeData.Length) return null;

                CdrDirRec dirRec =
                    Helpers.Marshal.ByteArrayToStructureBigEndian<CdrDirRec>(nodeData, entryOffset, size);

                return new DirectoryEntry
                {
                    Name               = name,
                    CNID               = dirRec.dirDirID,
                    ParentID           = parentId,
                    Type               = kCatalogRecordTypeDirectory,
                    Valence            = dirRec.dirVal,
                    CreationDate       = dirRec.dirCrDat,
                    ModificationDate   = dirRec.dirMdDat,
                    BackupDate         = dirRec.dirBkDat,
                    FinderInfo         = dirRec.dirUsrInfo,
                    ExtendedFinderInfo = dirRec.dirFndrInfo
                };
            }
            case kCatalogRecordTypeFile:
            {
                int size = Marshal.SizeOf(typeof(CdrFilRec));

                if(entryOffset + size > nodeData.Length) return null;

                CdrFilRec filRec =
                    Helpers.Marshal.ByteArrayToStructureBigEndian<CdrFilRec>(nodeData, entryOffset, size);

                return new FileEntry
                {
                    Name                     = name,
                    CNID                     = filRec.filFlNum,
                    ParentID                 = parentId,
                    Type                     = kCatalogRecordTypeFile,
                    FinderInfo               = filRec.filUsrWds,
                    ExtendedFinderInfo       = filRec.filFndrInfo,
                    DataForkLogicalSize      = filRec.filLgLen,
                    DataForkPhysicalSize     = filRec.filPyLen,
                    DataForkStartBlock       = filRec.filStBlk,
                    DataForkExtents          = filRec.filExtRec,
                    ResourceForkLogicalSize  = filRec.filRLgLen,
                    ResourceForkPhysicalSize = filRec.filRPyLen,
                    ResourceForkStartBlock   = filRec.filRStBlk,
                    ResourceForkExtents      = filRec.filRExtRec,
                    CreationDate             = filRec.filCrDat,
                    ModificationDate         = filRec.filMdDat,
                    BackupDate               = filRec.filBkDat
                };
            }
            default:
                return null;
        }
    }

    /// <summary>Extracts a catalog name from raw byte data using the configured encoding</summary>
    string ExtractCatalogName(byte[] data, int offset, byte length)
    {
        if(length == 0 || length > 31 || offset < 0 || offset + length > data.Length) return "";

        var nameBytes = new byte[length];
        Array.Copy(data, offset, nameBytes, 0, length);

        return StringHandlers.CToString(nameBytes, _encoding);
    }

#endregion

#region Directory caching

    /// <summary>Caches the root directory record from the catalog B-Tree</summary>
    ErrorNumber CacheRootDirectory()
    {
        CatalogFindData fd = default;

        ErrorNumber errno = CatFindBrec(kRootCnid, ref fd);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Failed to find root directory via B-tree: {0}", errno);

            return errno;
        }

        // Parse the directory record
        int cdrDirRecSize = Marshal.SizeOf(typeof(CdrDirRec));

        if(fd.EntryOffset + cdrDirRecSize > fd.NodeData.Length) return ErrorNumber.InvalidArgument;

        byte recordType = fd.NodeData[fd.EntryOffset];

        if(recordType != kCatalogRecordTypeDirectory)
        {
            AaruLogging.Debug(MODULE_NAME, "Root catalog record is not a directory (type={0})", recordType);

            return ErrorNumber.InvalidArgument;
        }

        _rootDirectory =
            Helpers.Marshal.ByteArrayToStructureBigEndian<CdrDirRec>(fd.NodeData, fd.EntryOffset, cdrDirRecSize);

        if(_rootDirectory.dirDirID != kRootCnid)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Root directory CNID mismatch: expected {0}, got {1}",
                              kRootCnid,
                              _rootDirectory.dirDirID);

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "Root directory cached: CNID={0}, valence={1}",
                          _rootDirectory.dirDirID,
                          _rootDirectory.dirVal);

        return ErrorNumber.NoError;
    }

    /// <summary>Caches all entries from the root directory</summary>
    ErrorNumber CacheRootDirectoryEntries()
    {
        _rootDirectoryCache = [];

        return CacheDirectoryEntries(kRootCnid, _rootDirectoryCache);
    }

    /// <summary>
    ///     Caches directory entries for a given CNID if not already cached.
    ///     Returns immediately if CNID is kRootCnid (already cached separately).
    /// </summary>
    ErrorNumber CacheDirectoryIfNeeded(uint cnid)
    {
        _directoryCaches ??= [];

        if(cnid == kRootCnid) return ErrorNumber.NoError;

        return _directoryCaches.ContainsKey(cnid) ? ErrorNumber.NoError : CacheDirectory(cnid);
    }

    /// <summary>Caches all entries for a directory by its CNID</summary>
    ErrorNumber CacheDirectory(uint cnid)
    {
        _directoryCaches ??= [];

        Dictionary<string, CatalogEntry> entries = [];

        ErrorNumber errno = CacheDirectoryEntries(cnid, entries);

        if(errno != ErrorNumber.NoError) return errno;

        _directoryCaches[cnid] = entries;

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Populates a dictionary with all entries (files and directories) whose parent is the given CNID.
    /// </summary>
    ErrorNumber CacheDirectoryEntries(uint cnid, Dictionary<string, CatalogEntry> entries)
    {
        // Search for the thread record (cnid, empty name)
        byte[]          searchKey = CatBuildKey(cnid, null, 0);
        CatalogFindData fd        = default;

        ErrorNumber errno = BTreeFind(searchKey, ref fd);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Thread record not found for CNID {0}: {1}", cnid, errno);

            return errno;
        }

        // Advance past the thread record to the first child entry
        errno = BRecGoto(ref fd, 1);

        if(errno != ErrorNumber.NoError)
        {
            // No children (empty directory) or end of tree
            return errno == ErrorNumber.NoSuchFile ? ErrorNumber.NoError : errno;
        }

        // Read entries while the key's ParID matches our directory
        for(;;)
        {
            // Check that the current key's ParID still matches
            if(fd.KeyOffset + 7 > fd.NodeData.Length) break;

            var keyParId = BigEndianBitConverter.ToUInt32(fd.NodeData, fd.KeyOffset + 2);

            if(keyParId != cnid) break;

            // Extract name from key
            byte   nameLen = fd.NodeData[fd.KeyOffset                     + 6];
            string name    = ExtractCatalogName(fd.NodeData, fd.KeyOffset + 7, nameLen);

            if(!string.IsNullOrEmpty(name))
            {
                CatalogEntry entry = ParseCatalogRecord(fd.NodeData, fd.EntryOffset, name, keyParId);

                if(entry != null) entries[name] = entry;
            }

            // Advance to next record
            errno = BRecGoto(ref fd, 1);

            if(errno != ErrorNumber.NoError) break;
        }

        AaruLogging.Debug(MODULE_NAME, "Cached {0} entries for CNID {1}", entries.Count, cnid);

        return ErrorNumber.NoError;
    }

    /// <summary>Gets cached directory entries for a given CNID</summary>
    Dictionary<string, CatalogEntry> GetDirectoryEntries(uint cnid)
    {
        if(cnid == kRootCnid) return _rootDirectoryCache;

        if(_directoryCaches == null) return null;

        _directoryCaches.TryGetValue(cnid, out Dictionary<string, CatalogEntry> entries);

        return entries;
    }

#endregion
}