// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Extents.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Aaru.Filesystems;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
/// <summary>Implements detection of Apple Hierarchical File System Plus (HFS+)</summary>
public sealed partial class AppleHFSPlus
{
    /// <summary>
    ///     Reads overflow extents from the Extents Overflow File for a resource fork.
    ///     The Extents Overflow File is a B-Tree that stores additional extent records for files
    ///     that have more than 8 extents (the maximum that can be stored in the catalog file).
    /// </summary>
    /// <param name="fileEntry">The file entry containing the resource fork</param>
    /// <param name="allExtents">List to append overflow extents to</param>
    /// <returns>Error number</returns>
    private ErrorNumber ReadResourceForkOverflowExtents(FileEntry fileEntry, List<HFSPlusExtentDescriptor> allExtents)
    {
        if(_volumeHeader.extentsFile.totalBlocks == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "ReadResourceForkOverflowExtents: No Extents Overflow File present");

            return ErrorNumber.NoError;
        }

        // Ensure the Extents Overflow File header is loaded
        ErrorNumber headerErr = EnsureExtentsFileHeaderLoaded();

        if(headerErr != ErrorNumber.NoError) return headerErr;

        AaruLogging.Debug(MODULE_NAME,
                          "ReadResourceForkOverflowExtents: Searching Extents Overflow File for resource fork extents (CNID={0})",
                          fileEntry.CNID);

        // Search the Extents Overflow File B-Tree for extent records with:
        // - CNID = fileEntry.CNID
        // - ForkType = 0xFF (resource fork)
        // The search uses a custom predicate that looks for extent records
        ErrorNumber errno = SearchExtentsOverflowFile(fileEntry.CNID, 0xFF, allExtents);

        return errno;
    }

    /// <summary>
    ///     Searches the Extents Overflow File B-Tree for extent records matching a CNID and fork type.
    ///     Performs a keyed descent to the first record of the fork and iterates forward, so extents are
    ///     appended in start block order.
    /// </summary>
    /// <param name="cnid">Catalog Node ID to search for</param>
    /// <param name="forkType">Fork type (0 for data, 0xFF for resource)</param>
    /// <param name="allExtents">List to append found extents to</param>
    /// <returns>Error number</returns>
    private ErrorNumber SearchExtentsOverflowFile(uint cnid, byte forkType, List<HFSPlusExtentDescriptor> allExtents)
    {
        ErrorNumber headerErr = EnsureExtentsFileHeaderLoaded();

        if(headerErr != ErrorNumber.NoError) return headerErr;

        // Empty tree: no overflow extents recorded
        if(_extentsFileHeader.treeDepth == 0 || _extentsFileHeader.rootNode == 0) return ErrorNumber.NoError;

        int extentRecordSize = Marshal.SizeOf(typeof(HFSPlusExtentRecord));

        return SearchBTreeRange(ReadExtentsFileNode,
                                in _extentsFileHeader,
                                MakeExtentKeyComparer(cnid, forkType, 0),
                                (nodeData, recordOffset) =>
                                {
                                    if(recordOffset + 12 > nodeData.Length) return true;

                                    // Extent key: keyLength(2) + forkType(1) + pad(1) + fileID(4) + startBlock(4)
                                    var  keyLength      = BigEndianBitConverter.ToUInt16(nodeData, recordOffset);
                                    byte recordForkType = nodeData[recordOffset                                 + 2];
                                    var  recordCNID     = BigEndianBitConverter.ToUInt32(nodeData, recordOffset + 4);

                                    // Past the fork's records: stop
                                    if(recordCNID != cnid || recordForkType != forkType) return true;

                                    int extentDataOffset = recordOffset + 2 + keyLength;

                                    if(extentDataOffset + extentRecordSize > nodeData.Length) return true;

                                    HFSPlusExtentRecord extentRecord =
                                        Helpers.Marshal.ByteArrayToStructureBigEndian<HFSPlusExtentRecord>(nodeData,
                                            extentDataOffset,
                                            extentRecordSize);

                                    foreach(HFSPlusExtentDescriptor extent in extentRecord.extentDescriptors
                                               .TakeWhile(static extent => extent.blockCount != 0))
                                    {
                                        allExtents.Add(extent);

                                        AaruLogging.Debug(MODULE_NAME,
                                                          "SearchExtentsOverflowFile: Added overflow extent: startBlock={0}, blockCount={1}",
                                                          extent.startBlock,
                                                          extent.blockCount);
                                    }

                                    return false;
                                });
    }

    /// <summary>
    ///     Ensures the Extents Overflow File B-Tree header has been read and cached.
    ///     This is called lazily on first access to the Extents Overflow File.
    /// </summary>
    /// <returns>Error number</returns>
    private ErrorNumber EnsureExtentsFileHeaderLoaded()
    {
        if(_extentsHeaderLoaded) return ErrorNumber.NoError;

        if(_volumeHeader.extentsFile.totalBlocks == 0) return ErrorNumber.InvalidArgument; // No Extents Overflow File

        AaruLogging.Debug(MODULE_NAME, "EnsureExtentsFileHeaderLoaded: Reading Extents Overflow File header");

        HFSPlusForkData extentsFork = _volumeHeader.extentsFile;

        if(extentsFork.extents.extentDescriptors               == null ||
           extentsFork.extents.extentDescriptors.Length        == 0    ||
           extentsFork.extents.extentDescriptors[0].blockCount == 0)
            return ErrorNumber.InvalidArgument;

        // The header node is at the start of the first extent. The node size is not known yet, but
        // the node descriptor and B-tree header record fit in the first 512 bytes of the node
        ulong extentsFileOffset = (ulong)extentsFork.extents.extentDescriptors[0].startBlock * _volumeHeader.blockSize;

        ulong deviceSector = ((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + extentsFileOffset) / _sectorSize;

        var byteOffset = (uint)(((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + extentsFileOffset) %
                                _sectorSize);

        int ndSize     = Marshal.SizeOf(typeof(BTNodeDescriptor));
        int headerSize = Marshal.SizeOf(typeof(BTHeaderRec));

        uint sectorsToRead = ((uint)(ndSize + headerSize) + byteOffset + _sectorSize - 1) / _sectorSize;

        ErrorNumber errno = _imagePlugin.ReadSectors(deviceSector,
                                                     false,
                                                     sectorsToRead,
                                                     out byte[] headerSectors,
                                                     out _);

        if(errno != ErrorNumber.NoError) return errno;

        if(headerSectors.Length < byteOffset + ndSize + headerSize) return ErrorNumber.InvalidArgument;

        BTNodeDescriptor nodeDesc =
            Helpers.Marshal.ByteArrayToStructureBigEndian<BTNodeDescriptor>(headerSectors, (int)byteOffset, ndSize);

        if(nodeDesc.kind != BTNodeKind.kBTHeaderNode) return ErrorNumber.InvalidArgument;

        _extentsFileHeader =
            Helpers.Marshal.ByteArrayToStructureBigEndian<BTHeaderRec>(headerSectors,
                                                                       (int)(byteOffset + ndSize),
                                                                       headerSize);

        _extentsHeaderLoaded = true;

        AaruLogging.Debug(MODULE_NAME,
                          "EnsureExtentsFileHeaderLoaded: Extents File B-Tree header: depth={0}, rootNode={1}, nodeSize={2}",
                          _extentsFileHeader.treeDepth,
                          _extentsFileHeader.rootNode,
                          _extentsFileHeader.nodeSize);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Reads a node from the Extents Overflow File by node number. The extents file's own extents are
    ///     guaranteed to fit in the volume header's 8 inline descriptors, so no overflow lookup is needed.
    /// </summary>
    /// <param name="nodeNumber">The node number to read</param>
    /// <param name="nodeData">The node data read from disk</param>
    /// <returns>Error number</returns>
    private ErrorNumber ReadExtentsFileNode(uint nodeNumber, out byte[] nodeData)
    {
        nodeData = null;

        ErrorNumber headerErr = EnsureExtentsFileHeaderLoaded();

        if(headerErr != ErrorNumber.NoError) return headerErr;

        return ReadBTreeNode(_volumeHeader.extentsFile.extents.extentDescriptors,
                             _extentsFileHeader.nodeSize,
                             nodeNumber,
                             out nodeData);
    }
}