// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Catalog.cs
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
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Aaru.Filesystems;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
/// <summary>Implements detection of Apple Hierarchical File System Plus (HFS+)</summary>
public sealed partial class AppleHFSPlus
{
    /// <summary>Merged extent list of the catalog file (inline extents plus any overflow extents)</summary>
    List<HFSPlusExtentDescriptor> _catalogExtents;
    /// <summary>Whether overflow extents for the catalog file have already been merged</summary>
    bool                          _catalogOverflowLoaded;

    /// <summary>Reads a catalog B-Tree node by node number</summary>
    ErrorNumber ReadCatalogNode(uint nodeNumber, out byte[] nodeData)
    {
        if(_catalogExtents == null)
        {
            _catalogExtents = [];

            if(_volumeHeader.catalogFile.extents.extentDescriptors != null)
            {
                foreach(HFSPlusExtentDescriptor extent in _volumeHeader.catalogFile.extents.extentDescriptors)
                {
                    if(extent.blockCount == 0) break;

                    _catalogExtents.Add(extent);
                }
            }
        }

        ErrorNumber errno = ReadBTreeNode(_catalogExtents, _catalogBTreeHeader.nodeSize, nodeNumber, out nodeData);

        if(errno != ErrorNumber.InvalidArgument || _catalogOverflowLoaded) return errno;

        // The node may live beyond the 8 inline extents: merge overflow extents for the catalog
        // file from the extents overflow B-tree and retry once
        _catalogOverflowLoaded = true;

        ErrorNumber overflowErr = SearchExtentsOverflowFile(kHFSCatalogFileID, 0x00, _catalogExtents);

        if(overflowErr != ErrorNumber.NoError) return errno;

        return ReadBTreeNode(_catalogExtents, _catalogBTreeHeader.nodeSize, nodeNumber, out nodeData);
    }

    /// <summary>
    ///     Performs a keyed range scan of the catalog B-tree: descends the tree to the first record whose key is
    ///     not less than (parentID, name), then visits records forward until the visitor stops the walk.
    /// </summary>
    ErrorNumber SearchCatalogRange(uint parentID, string name, RecordVisitor visitor) =>
        SearchBTreeRange(ReadCatalogNode, in _catalogBTreeHeader, MakeCatalogKeyComparer(parentID, name), visitor);

    /// <summary>
    ///     Scans all catalog records of a directory (all keys with the given parent CNID) and adds folder and
    ///     file records to the given dictionary, keyed by name. Thread records are skipped.
    /// </summary>
    ErrorNumber ScanDirectoryRecords(uint cnid, Dictionary<string, CatalogEntry> entries) => SearchCatalogRange(cnid,
        "",
        (nodeData, recordOffset) =>
        {
            if(recordOffset + 6 > nodeData.Length) return true;

            var parentID = BigEndianBitConverter.ToUInt32(nodeData, recordOffset + 2);

            if(parentID < cnid) return false; // Defensive: should not happen after keyed descent

            if(parentID > cnid) return true; // Past the directory: stop

            if(!ParseCatalogRecordToEntry(nodeData, recordOffset, out string entryName, out CatalogEntry entry))
                return false;

            if(!string.IsNullOrEmpty(entryName)) entries[entryName] = entry;

            return false;
        });

    /// <summary>
    ///     Parses a catalog leaf record into a <see cref="CatalogEntry" />. Returns false for thread records,
    ///     truncated records, and any record type other than folder or file.
    /// </summary>
    bool ParseCatalogRecordToEntry(byte[] nodeData, ushort recordOffset, out string entryName, out CatalogEntry entry)
    {
        entryName = null;
        entry     = null;

        if(recordOffset + 6 > nodeData.Length) return false;

        var keyLength = BigEndianBitConverter.ToUInt16(nodeData, recordOffset);
        var parentID  = BigEndianBitConverter.ToUInt32(nodeData, recordOffset + 2);

        // The record data is after the key
        int recordTypeOffset = recordOffset + 2 + keyLength;

        if(recordTypeOffset + 2 > nodeData.Length) return false;

        var recordType = BigEndianBitConverter.ToInt16(nodeData, recordTypeOffset);

        switch(recordType)
        {
            case (short)BTreeRecordType.kHFSPlusFolderRecord:
            {
                int folderRecordSize = Marshal.SizeOf(typeof(HFSPlusCatalogFolder));

                if(recordTypeOffset + folderRecordSize > nodeData.Length) return false;

                HFSPlusCatalogFolder folder =
                    Helpers.Marshal.ByteArrayToStructureBigEndian<HFSPlusCatalogFolder>(nodeData,
                        recordTypeOffset,
                        folderRecordSize);

                entryName = ExtractNameFromCatalogKey(nodeData, recordOffset);

                entry = new DirectoryEntry
                {
                    Name               = entryName,
                    CNID               = folder.folderID,
                    ParentID           = parentID,
                    Type               = (int)BTreeRecordType.kHFSPlusFolderRecord,
                    Valence            = folder.valence,
                    CreationDate       = folder.createDate,
                    ContentModDate     = folder.contentModDate,
                    AttributeModDate   = folder.attributeModDate,
                    AccessDate         = folder.accessDate,
                    BackupDate         = folder.backupDate,
                    FinderInfo         = folder.userInfo,
                    ExtendedFinderInfo = folder.finderInfo,
                    TextEncoding       = folder.textEncoding,
                    permissions        = folder.permissions
                };

                return true;
            }
            case (short)BTreeRecordType.kHFSPlusFileRecord:
            {
                int fileRecordSize = Marshal.SizeOf(typeof(HFSPlusCatalogFile));

                if(recordTypeOffset + fileRecordSize > nodeData.Length) return false;

                HFSPlusCatalogFile file =
                    Helpers.Marshal.ByteArrayToStructureBigEndian<HFSPlusCatalogFile>(nodeData,
                        recordTypeOffset,
                        fileRecordSize);

                entryName = ExtractNameFromCatalogKey(nodeData, recordOffset);

                entry = new FileEntry
                {
                    Name                     = entryName,
                    CNID                     = file.fileID,
                    ParentID                 = parentID,
                    Type                     = (int)BTreeRecordType.kHFSPlusFileRecord,
                    DataForkLogicalSize      = file.dataFork.logicalSize,
                    DataForkPhysicalSize     = file.dataFork.logicalSize,
                    DataForkTotalBlocks      = file.dataFork.totalBlocks,
                    DataForkExtents          = file.dataFork.extents,
                    ResourceForkLogicalSize  = file.resourceFork.logicalSize,
                    ResourceForkPhysicalSize = file.resourceFork.logicalSize,
                    ResourceForkTotalBlocks  = file.resourceFork.totalBlocks,
                    ResourceForkExtents      = file.resourceFork.extents,
                    CreationDate             = file.createDate,
                    ContentModDate           = file.contentModDate,
                    AttributeModDate         = file.attributeModDate,
                    AccessDate               = file.accessDate,
                    BackupDate               = file.backupDate
                };

                return true;
            }
            default:
                return false;
        }
    }

    /// <summary>Extracts the filename from a catalog key</summary>
    string ExtractNameFromCatalogKey(byte[] leafNode, ushort keyOffset)
    {
        if(keyOffset + 2 > leafNode.Length) return string.Empty;

        // The key structure is: keyLength(2) + parentID(4) + nodeName (Unicode string with length prefix)
        // nodeName format: length(2) + UTF-16 data
        int nameOffset = keyOffset + 2 + 4; // Skip keyLength and parentID

        if(nameOffset + 2 > leafNode.Length) return string.Empty;

        var nameLength = BigEndianBitConverter.ToUInt16(leafNode, nameOffset);

        // nameLength is in UTF-16 code units (2 bytes each)
        int nameDataSize = nameLength * 2;

        if(nameOffset + 2 + nameDataSize > leafNode.Length) return string.Empty;

        // Convert UTF-16 big-endian bytes to string
        try
        {
            var nameBytes = new byte[nameDataSize];
            Array.Copy(leafNode, nameOffset + 2, nameBytes, 0, nameDataSize);

            // Decode as UTF-16 big-endian
            string name = Encoding.BigEndianUnicode.GetString(nameBytes);

            return name;
        }
        catch
        {
            return string.Empty;
        }
    }

#region Nested type: HfsPlusNameComparer

    /// <summary>
    ///     Equality comparer for catalog entry dictionaries that matches the volume's key comparison rules,
    ///     so name lookups behave exactly like on-disk catalog key comparisons.
    /// </summary>
    sealed class HfsPlusNameComparer(bool caseSensitive) : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if(x == null || y == null) return x == y;

            return caseSensitive
                       ? HfsPlusUnicode.BinaryCompare(x, y)      == 0
                       : HfsPlusUnicode.FastUnicodeCompare(x, y) == 0;
        }

        public int GetHashCode(string obj) => caseSensitive ? obj.GetHashCode() : HfsPlusUnicode.GetFoldedHashCode(obj);
    }

#endregion
}