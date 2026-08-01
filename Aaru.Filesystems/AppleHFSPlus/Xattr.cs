// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
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
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
/// <summary>Implements detection of Apple Hierarchical File System Plus (HFS+)</summary>
public sealed partial class AppleHFSPlus
{
    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        xattrs = [];

        // Get file entry
        ErrorNumber error = GetFileEntry(normalizedPath, out CatalogEntry entry);

        if(error != ErrorNumber.NoError) return error;

        // Only files can have these xattrs, not directories
        if(entry is not FileEntry fileEntry) return ErrorNumber.NoError;

        // Add Finder Info xattr (always present for files)
        xattrs.Add(Xattrs.XATTR_APPLE_FINDER_INFO);

        // Add HFS+ creator and type xattrs
        xattrs.Add(Xattrs.XATTR_APPLE_HFS_CREATOR);
        xattrs.Add(Xattrs.XATTR_APPLE_HFS_OSTYPE);

        // Add Resource Fork xattr if it exists and is non-empty
        if(fileEntry.ResourceForkLogicalSize > 0) xattrs.Add(Xattrs.XATTR_APPLE_RESOURCE_FORK);

        // Read attributes from the attributes B-tree file if it exists
        if(_attributesFile != null)
        {
            ErrorNumber attrError = ListAttributesFromBTree(fileEntry.CNID, xattrs);

            if(attrError != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "ListXAttr: Failed to read attributes from B-tree for CNID {0}: {1}",
                                  fileEntry.CNID,
                                  attrError);
            }
        }

        xattrs.Sort();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        buf = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(string.IsNullOrEmpty(xattr)) return ErrorNumber.InvalidArgument;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        // Get file entry
        ErrorNumber error = GetFileEntry(normalizedPath, out CatalogEntry entry);

        if(error != ErrorNumber.NoError) return error;

        // Only files can have these xattrs
        if(entry is not FileEntry fileEntry) return ErrorNumber.NoSuchExtendedAttribute;

        // Handle Finder Info xattr: concatenate FInfo (16 bytes) + FXInfo (16 bytes) = 32 bytes
        if(string.Equals(xattr, Xattrs.XATTR_APPLE_FINDER_INFO, StringComparison.OrdinalIgnoreCase))
        {
            buf = new byte[32];

            // FInfo (16 bytes)
            byte[] finderInfoBytes = Marshal.StructureToByteArrayBigEndian(fileEntry.FinderInfo);
            Array.Copy(finderInfoBytes, 0, buf, 0, Math.Min(finderInfoBytes.Length, 16));

            // FXInfo (16 bytes)
            byte[] extendedFinderInfoBytes = Marshal.StructureToByteArrayBigEndian(fileEntry.ExtendedFinderInfo);
            Array.Copy(extendedFinderInfoBytes, 0, buf, 16, Math.Min(extendedFinderInfoBytes.Length, 16));

            return ErrorNumber.NoError;
        }

        // Handle HFS+ creator xattr (4 bytes, as stored)
        if(string.Equals(xattr, Xattrs.XATTR_APPLE_HFS_CREATOR, StringComparison.OrdinalIgnoreCase))
        {
            buf = BitConverter.GetBytes(fileEntry.FinderInfo.fdCreator);

            return ErrorNumber.NoError;
        }

        // Handle HFS+ type xattr (4 bytes, as stored)
        if(string.Equals(xattr, Xattrs.XATTR_APPLE_HFS_OSTYPE, StringComparison.OrdinalIgnoreCase))
        {
            buf = BitConverter.GetBytes(fileEntry.FinderInfo.fdType);

            return ErrorNumber.NoError;
        }

        // Handle Resource Fork xattr
        if(string.Equals(xattr, Xattrs.XATTR_APPLE_RESOURCE_FORK, StringComparison.OrdinalIgnoreCase))
        {
            return fileEntry.ResourceForkLogicalSize == 0
                       ? ErrorNumber.NoSuchExtendedAttribute
                       : ReadResourceFork(fileEntry, out buf);
        }

        // Try to read from attributes B-tree file
        if(_attributesFile != null)
        {
            ErrorNumber attrError = GetAttributeFromBTree(fileEntry.CNID, xattr, out buf);

            if(attrError == ErrorNumber.NoError) return ErrorNumber.NoError;

            if(attrError != ErrorNumber.NoSuchExtendedAttribute)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "GetXattr: Failed to read attribute '{0}' from B-tree for CNID {1}: {2}",
                                  xattr,
                                  fileEntry.CNID,
                                  attrError);
            }
        }

        return ErrorNumber.NoSuchExtendedAttribute;
    }

    /// <summary>Gets a catalog entry (file or directory) by path</summary>
    /// <param name="path">Path to the file or directory</param>
    /// <param name="entry">The catalog entry if found</param>
    /// <returns>Error number</returns>
    private ErrorNumber GetFileEntry(string path, out CatalogEntry entry)
    {
        entry = null;

        // Normalize path
        string normalizedPath = string.IsNullOrEmpty(path) ? "/" : path;

        // Root directory case
        if(string.Equals(normalizedPath, "/", StringComparison.OrdinalIgnoreCase))
            return ErrorNumber.InvalidArgument; // Root is a directory, not a file

        // Parse path components
        string cutPath = normalizedPath.StartsWith("/", StringComparison.Ordinal)
                             ? normalizedPath[1..]
                             : normalizedPath;

        string[] pieces = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, CatalogEntry> currentDirectory = _rootDirectoryCache;
        uint                             currentDirCNID   = kHFSRootFolderID;

        // Traverse through all but the last path component
        for(var p = 0; p < pieces.Length - 1; p++)
        {
            string component = pieces[p];

            // Convert colons back to slashes for catalog lookup
            component = component.Replace(":", "/");

            // Look for the component in current directory
            CatalogEntry foundEntry = null;

            currentDirectory?.TryGetValue(component, out foundEntry);

            if(foundEntry == null) return ErrorNumber.NoSuchFile;

            // Check if it's a directory
            if(foundEntry.Type != (int)BTreeRecordType.kHFSPlusFolderRecord) return ErrorNumber.NotDirectory;

            // Update current directory info
            currentDirCNID = foundEntry.CNID;

            // Load next directory level
            ErrorNumber cacheErr = CacheDirectoryIfNeeded(currentDirCNID);

            if(cacheErr != ErrorNumber.NoError) return cacheErr;

            currentDirectory = GetDirectoryEntries(currentDirCNID);

            if(currentDirectory == null) return ErrorNumber.NoSuchFile;
        }

        // Now look for the final component
        string lastComponent = pieces[^1];

        // Convert colons back to slashes for catalog lookup
        lastComponent = lastComponent.Replace(":", "/");

        if(currentDirectory != null && currentDirectory.TryGetValue(lastComponent, out entry))
            return ErrorNumber.NoError;

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>Reads the resource fork of a file</summary>
    /// <param name="fileEntry">The file entry to read the resource fork from</param>
    /// <param name="buf">Buffer containing the resource fork data</param>
    /// <returns>Error number</returns>
    private ErrorNumber ReadResourceFork(FileEntry fileEntry, out byte[] buf)
    {
        buf = null;

        if(fileEntry.ResourceForkLogicalSize == 0) return ErrorNumber.NoSuchExtendedAttribute;

        ulong logicalSize = fileEntry.ResourceForkLogicalSize;

        AaruLogging.Debug(MODULE_NAME,
                          "ReadResourceFork: Reading {0} bytes from resource fork (CNID={1})",
                          logicalSize,
                          fileEntry.CNID);

        buf = new byte[logicalSize];
        ulong bytesRead = 0;

        // Get all extents for this fork (may include overflow extents from Extents Overflow File)
        ErrorNumber extentErr = GetResourceForkExtents(fileEntry, out List<HFSPlusExtentDescriptor> allExtents);

        if(extentErr != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "ReadResourceFork: Failed to get extents for resource fork of file {0}: {1}",
                              fileEntry.CNID,
                              extentErr);

            return extentErr;
        }

        if(allExtents == null || allExtents.Count == 0)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "ReadResourceFork: No extents found for resource fork of file {0}",
                              fileEntry.CNID);

            buf = [];

            return ErrorNumber.NoError;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "ReadResourceFork: Got {0} total extents for resource fork of file {1}",
                          allExtents.Count,
                          fileEntry.CNID);

        // Read each extent
        foreach(HFSPlusExtentDescriptor extent in allExtents)
        {
            if(extent.blockCount == 0) break;

            ulong extentSizeBytes = (ulong)extent.blockCount * _volumeHeader.blockSize;
            ulong toRead          = Math.Min(extentSizeBytes, logicalSize - bytesRead);

            // Calculate sector offset for this extent
            // HFS+ uses allocation blocks as the unit, which are blockSize bytes
            ulong blockOffsetBytes = (ulong)extent.startBlock * _volumeHeader.blockSize;

            // Convert to device sector address
            // For wrapped volumes, blocks start after the HFS+ volume offset
            // For pure HFS+, _hfsPlusVolumeOffset is 0
            ulong deviceSector = ((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffsetBytes) /
                                 _sectorSize;

            var byteOffset = (uint)(((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffsetBytes) %
                                    _sectorSize);

            var sectorsToRead = (uint)((toRead + byteOffset + _sectorSize - 1) / _sectorSize);

            AaruLogging.Debug(MODULE_NAME,
                              "ReadResourceFork: Reading extent: startBlock={0}, blockCount={1}, size={2} bytes at sector {3}",
                              extent.startBlock,
                              extent.blockCount,
                              toRead,
                              deviceSector);

            ErrorNumber readErr = _imagePlugin.ReadSectors(deviceSector,
                                                           false,
                                                           sectorsToRead,
                                                           out byte[] sectorData,
                                                           out _);

            if(readErr != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadResourceFork: Failed to read sectors: {0}", readErr);

                return readErr;
            }

            if(sectorData == null || sectorData.Length == 0)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadResourceFork: Got empty sector data");

                return ErrorNumber.InvalidArgument;
            }

            // Copy data from sector buffer to output buffer, accounting for offset
            uint bytesToCopy = Math.Min((uint)toRead, (uint)(sectorData.Length - byteOffset));

            AaruLogging.Debug(MODULE_NAME,
                              "ReadResourceFork: Read {0} bytes from extent, copying {1} bytes from offset {2}",
                              sectorData.Length,
                              bytesToCopy,
                              byteOffset);

            Array.Copy(sectorData, (int)byteOffset, buf, (int)bytesRead, bytesToCopy);

            bytesRead += bytesToCopy;

            if(bytesRead >= logicalSize) break;
        }

        if(bytesRead < logicalSize)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "ReadResourceFork: Warning - Read only {0} bytes out of {1} bytes for resource fork of file {2}",
                              bytesRead,
                              logicalSize,
                              fileEntry.CNID);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Gets all extents for a resource fork, including overflow extents from the Extents Overflow File</summary>
    /// <param name="fileEntry">The file entry containing the resource fork</param>
    /// <param name="allExtents">List of all extent descriptors for the resource fork</param>
    /// <returns>Error number</returns>
    private ErrorNumber GetResourceForkExtents(FileEntry fileEntry, out List<HFSPlusExtentDescriptor> allExtents)
    {
        allExtents = [];

        // Add the first 8 extents from the fork data
        for(var i = 0; i < fileEntry.ResourceForkExtents.extentDescriptors.Length; i++)
        {
            HFSPlusExtentDescriptor extent = fileEntry.ResourceForkExtents.extentDescriptors[i];

            if(extent.blockCount == 0) break;

            allExtents.Add(extent);

            AaruLogging.Debug(MODULE_NAME,
                              "GetResourceForkExtents: Adding extent from fork data: startBlock={0}, blockCount={1}",
                              extent.startBlock,
                              extent.blockCount);
        }

        // If we've got all extents (less than 8), we're done
        if(allExtents.Count < 8) return ErrorNumber.NoError;

        // If total blocks match, we have all extents
        uint totalBlocksFromExtents                                                  = 0;
        foreach(HFSPlusExtentDescriptor extent in allExtents) totalBlocksFromExtents += extent.blockCount;

        if(totalBlocksFromExtents >= fileEntry.ResourceForkTotalBlocks) return ErrorNumber.NoError;

        // We need to read overflow extents from the Extents Overflow File
        AaruLogging.Debug(MODULE_NAME,
                          "GetResourceForkExtents: Need to read overflow extents for resource fork (CNID={0})",
                          fileEntry.CNID);

        // Read overflow extents from the Extents Overflow File
        ErrorNumber overflowErr = ReadResourceForkOverflowExtents(fileEntry, allExtents);

        if(overflowErr != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "GetResourceForkExtents: Failed to read overflow extents: {0}", overflowErr);

            return overflowErr;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Lists all attributes for a file from the attributes B-tree</summary>
    /// <param name="fileID">CNID of the file</param>
    /// <param name="xattrs">List to append attribute names to</param>
    /// <returns>Error number</returns>
    private ErrorNumber ListAttributesFromBTree(uint fileID, List<string> xattrs)
    {
        if(_attributesFile == null) return ErrorNumber.NotSupported;

        try
        {
            // Search the attributes B-tree for all attributes belonging to this file
            // Attributes are keyed by (fileID, startBlock, attrName)
            // We need to find all records where fileID matches and startBlock == 0 (primary records)

            ErrorNumber searchError = SearchAttributeBTree(fileID, null, out List<string> attributeNames);

            if(searchError != ErrorNumber.NoError) return searchError;

            if(attributeNames != null && attributeNames.Count > 0)
            {
                foreach(string attrName in attributeNames)
                {
                    // Skip protected/internal attributes
                    if(attrName.StartsWith("com.apple.system.", StringComparison.Ordinal)) continue;

                    if(!xattrs.Contains(attrName)) xattrs.Add(attrName);
                }
            }

            return ErrorNumber.NoError;
        }
        catch(Exception ex)
        {
            AaruLogging.Debug(MODULE_NAME, "ListAttributesFromBTree: Exception - {0}", ex.Message);

            return ErrorNumber.InOutError;
        }
    }

    /// <summary>Gets an attribute value from the attributes B-tree</summary>
    /// <param name="fileID">CNID of the file</param>
    /// <param name="attrName">Name of the attribute</param>
    /// <param name="buf">Output buffer with attribute data</param>
    /// <returns>Error number</returns>
    private ErrorNumber GetAttributeFromBTree(uint fileID, string attrName, out byte[] buf)
    {
        buf = null;

        if(_attributesFile == null) return ErrorNumber.NotSupported;

        try
        {
            // Search for the specific attribute
            ErrorNumber searchError = SearchAttributeBTree(fileID, attrName, out HFSPlusAttrRecord attrRecord);

            if(searchError != ErrorNumber.NoError) return searchError;

            // Extract data based on record type
            switch((BTAttributeRecordType)attrRecord.recordType)
            {
                case BTAttributeRecordType.kHFSPlusAttrInlineData:
                    // Inline data - data is stored directly in the B-tree node
                    uint dataSize = attrRecord.attrData.attrSize;

                    if(dataSize == 0)
                    {
                        buf = [];

                        return ErrorNumber.NoError;
                    }

                    // Read the full record to get all the data
                    ErrorNumber readError = ReadAttributeData(fileID, attrName, dataSize, out buf);

                    return readError;

                case BTAttributeRecordType.kHFSPlusAttrForkData:
                    // Extent-based attribute - data is stored in allocation blocks
                    return ReadAttributeForkData(fileID, attrName, ref attrRecord.forkData, out buf);

                default:
                    AaruLogging.Debug(MODULE_NAME,
                                      "GetAttributeFromBTree: Unknown attribute record type {0}",
                                      attrRecord.recordType);

                    return ErrorNumber.NotSupported;
            }
        }
        catch(Exception ex)
        {
            AaruLogging.Debug(MODULE_NAME, "GetAttributeFromBTree: Exception - {0}", ex.Message);

            return ErrorNumber.InOutError;
        }
    }

    /// <summary>Searches the attributes B-tree for attributes</summary>
    /// <param name="fileID">CNID of the file</param>
    /// <param name="attrName">Ignored; all attribute names of the file are listed</param>
    /// <param name="attributeNames">Output list of attribute names</param>
    /// <returns>Error number</returns>
    private ErrorNumber SearchAttributeBTree(uint fileID, string attrName, out List<string> attributeNames)
    {
        attributeNames = [];

        if(_attributesFile == null) return ErrorNumber.NotSupported;

        ErrorNumber headerErr = GetAttributesBTreeHeader(out BTHeaderRec attrBTreeHeader);

        if(headerErr != ErrorNumber.NoError) return headerErr;

        if(attrBTreeHeader.treeDepth == 0 || attrBTreeHeader.rootNode == 0) return ErrorNumber.NoError;

        List<string> names = attributeNames;

        // Keyed range scan: descend to the file's first attribute record and walk forward until
        // the file CNID changes
        return SearchBTreeRange((uint n, out byte[] d) => ReadAttributeNode(n, attrBTreeHeader.nodeSize, out d),
                                in attrBTreeHeader,
                                MakeAttributeKeyComparer(fileID, "", 0),
                                (nodeData, recordOffset) =>
                                {
                                    if(recordOffset + 14 > nodeData.Length) return true;

                                    var keyFileID  = BigEndianBitConverter.ToUInt32(nodeData, recordOffset + 4);
                                    var startBlock = BigEndianBitConverter.ToUInt32(nodeData, recordOffset + 8);

                                    if(keyFileID != fileID) return true; // Past the file's attributes: stop

                                    // Only primary records name an attribute
                                    if(startBlock != 0) return false;

                                    var attrNameLen = BigEndianBitConverter.ToUInt16(nodeData, recordOffset + 12);

                                    if(attrNameLen == 0 || recordOffset + 14 + attrNameLen * 2 > nodeData.Length)
                                        return false;

                                    var nameChars = new char[attrNameLen];

                                    for(var j = 0; j < attrNameLen; j++)
                                    {
                                        nameChars[j] =
                                            (char)BigEndianBitConverter.ToUInt16(nodeData, recordOffset + 14 + j * 2);
                                    }

                                    string name = new(nameChars);

                                    if(!string.IsNullOrEmpty(name) && !names.Contains(name)) names.Add(name);

                                    return false;
                                });
    }

    /// <summary>
    ///     Finds the leaf record of a specific attribute via keyed B-tree search. On success returns the leaf
    ///     node data and the record offset within it.
    /// </summary>
    /// <param name="fileID">CNID of the file</param>
    /// <param name="attrName">Attribute name to search for</param>
    /// <param name="nodeData">Leaf node containing the record</param>
    /// <param name="recordOffset">Offset of the record within the leaf node</param>
    /// <returns>Error number</returns>
    private ErrorNumber FindAttributeRecord(uint fileID, string attrName, out byte[] nodeData, out ushort recordOffset)
    {
        nodeData     = null;
        recordOffset = 0;

        if(_attributesFile == null || string.IsNullOrEmpty(attrName)) return ErrorNumber.NotSupported;

        ErrorNumber headerErr = GetAttributesBTreeHeader(out BTHeaderRec attrBTreeHeader);

        if(headerErr != ErrorNumber.NoError) return headerErr;

        if(attrBTreeHeader.treeDepth == 0 || attrBTreeHeader.rootNode == 0) return ErrorNumber.NoSuchExtendedAttribute;

        ErrorNumber errno = SearchBTree((uint n, out byte[] d) => ReadAttributeNode(n, attrBTreeHeader.nodeSize, out d),
                                        in attrBTreeHeader,
                                        MakeAttributeKeyComparer(fileID, attrName, 0),
                                        out uint _,
                                        out byte[] leafNodeData,
                                        out int recordIndex,
                                        out bool exactMatch);

        if(errno == ErrorNumber.NoSuchFile) return ErrorNumber.NoSuchExtendedAttribute;

        if(errno != ErrorNumber.NoError) return errno;

        if(!exactMatch) return ErrorNumber.NoSuchExtendedAttribute;

        int ndSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BTNodeDescriptor));

        if(leafNodeData.Length < ndSize) return ErrorNumber.InvalidArgument;

        BTNodeDescriptor nodeDesc = Marshal.ByteArrayToStructureBigEndian<BTNodeDescriptor>(leafNodeData, 0, ndSize);

        if(!TryGetRecordOffset(leafNodeData,
                               attrBTreeHeader.nodeSize,
                               nodeDesc.numRecords,
                               recordIndex,
                               out recordOffset))
            return ErrorNumber.InvalidArgument;

        nodeData = leafNodeData;

        return ErrorNumber.NoError;
    }

    /// <summary>Searches the attributes B-tree for a specific attribute</summary>
    /// <param name="fileID">CNID of the file</param>
    /// <param name="attrName">Attribute name to search for</param>
    /// <param name="attrRecord">Output attribute record</param>
    /// <returns>Error number</returns>
    private ErrorNumber SearchAttributeBTree(uint fileID, string attrName, out HFSPlusAttrRecord attrRecord)
    {
        attrRecord = default(HFSPlusAttrRecord);

        ErrorNumber errno = FindAttributeRecord(fileID, attrName, out byte[] nodeData, out ushort recordOffset);

        if(errno != ErrorNumber.NoError) return errno;

        return ParseAttributeRecordAt(nodeData, recordOffset, out attrRecord)
                   ? ErrorNumber.NoError
                   : ErrorNumber.InvalidArgument;
    }

    /// <summary>Parses the attribute record data following the key at the given record offset</summary>
    private static bool ParseAttributeRecordAt(byte[] nodeData, ushort recordOffset, out HFSPlusAttrRecord attrRecord)
    {
        attrRecord = default(HFSPlusAttrRecord);

        if(recordOffset + 2 > nodeData.Length) return false;

        var keyLength  = BigEndianBitConverter.ToUInt16(nodeData, recordOffset);
        int dataOffset = recordOffset + 2 + keyLength;

        if(dataOffset + 4 > nodeData.Length) return false;

        var recordType = BigEndianBitConverter.ToUInt32(nodeData, dataOffset);

        if(recordType == (uint)BTAttributeRecordType.kHFSPlusAttrInlineData)
        {
            int attrDataSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(HFSPlusAttrData));

            if(dataOffset + attrDataSize > nodeData.Length) return false;

            attrRecord.recordType = recordType;

            attrRecord.attrData =
                Marshal.ByteArrayToStructureBigEndian<HFSPlusAttrData>(nodeData, dataOffset, attrDataSize);

            return true;
        }

        if(recordType == (uint)BTAttributeRecordType.kHFSPlusAttrForkData)
        {
            int forkDataSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(HFSPlusAttrForkData));

            if(dataOffset + forkDataSize > nodeData.Length) return false;

            attrRecord.recordType = recordType;

            attrRecord.forkData =
                Marshal.ByteArrayToStructureBigEndian<HFSPlusAttrForkData>(nodeData, dataOffset, forkDataSize);

            return true;
        }

        return false;
    }

    /// <summary>Reads the attributes B-tree header, caching it after the first read</summary>
    /// <param name="header">Output B-tree header</param>
    /// <returns>Error number</returns>
    private ErrorNumber GetAttributesBTreeHeader(out BTHeaderRec header)
    {
        if(_attributesBTreeHeader.HasValue)
        {
            header = _attributesBTreeHeader.Value;

            return ErrorNumber.NoError;
        }

        ErrorNumber errno = ReadAttributesBTreeHeader(out header);

        if(errno == ErrorNumber.NoError) _attributesBTreeHeader = header;

        return errno;
    }

    /// <summary>Reads inline attribute data</summary>
    /// <param name="fileID">CNID of the file</param>
    /// <param name="attrName">Attribute name</param>
    /// <param name="dataSize">Size of data to read</param>
    /// <param name="buf">Output buffer</param>
    /// <returns>Error number</returns>
    private ErrorNumber ReadAttributeData(uint fileID, string attrName, uint dataSize, out byte[] buf)
    {
        buf = null;

        if(dataSize == 0)
        {
            buf = [];

            return ErrorNumber.NoError;
        }

        ErrorNumber errno = FindAttributeRecord(fileID, attrName, out byte[] nodeData, out ushort recordOffset);

        if(errno != ErrorNumber.NoError) return errno;

        var keyLength  = BigEndianBitConverter.ToUInt16(nodeData, recordOffset);
        int dataOffset = recordOffset + 2 + keyLength;

        if(dataOffset + 4 > nodeData.Length) return ErrorNumber.InvalidArgument;

        var recordType = BigEndianBitConverter.ToUInt32(nodeData, dataOffset);

        if(recordType != (uint)BTAttributeRecordType.kHFSPlusAttrInlineData) return ErrorNumber.InvalidArgument;

        // Inline data follows recordType(4) + reserved(8) + attrSize(4)
        int attrDataOffset = dataOffset + 16;

        if(attrDataOffset + dataSize > nodeData.Length) return ErrorNumber.InvalidArgument;

        buf = new byte[dataSize];
        Array.Copy(nodeData, attrDataOffset, buf, 0, (int)dataSize);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads extent-based attribute data</summary>
    /// <param name="fileID">CNID of the file</param>
    /// <param name="attrName">Attribute name</param>
    /// <param name="forkData">Fork data containing extents</param>
    /// <param name="buf">Output buffer</param>
    /// <returns>Error number</returns>
    private ErrorNumber ReadAttributeForkData(uint       fileID, string attrName, ref HFSPlusAttrForkData forkData,
                                              out byte[] buf)
    {
        buf = null;

        ulong logicalSize = forkData.theFork.logicalSize;

        if(logicalSize == 0)
        {
            buf = [];

            return ErrorNumber.NoError;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "ReadAttributeForkData: Reading {0} bytes for attr '{1}' (CNID={2})",
                          logicalSize,
                          attrName,
                          fileID);

        buf = new byte[logicalSize];
        ulong bytesRead = 0;

        // Get all extents for this attribute (may include overflow extents)
        List<HFSPlusExtentDescriptor> allExtents = [];

        // Add the first 8 extents from the fork data
        foreach(HFSPlusExtentDescriptor extent in forkData.theFork.extents.extentDescriptors)
        {
            if(extent.blockCount == 0) break;

            allExtents.Add(extent);
        }

        // Check if we need overflow extents
        uint totalBlocksFromExtents = 0;

        foreach(HFSPlusExtentDescriptor extent in allExtents) totalBlocksFromExtents += extent.blockCount;

        if(totalBlocksFromExtents < forkData.theFork.totalBlocks)
        {
            // We need to read overflow extents from the Extents Overflow File
            AaruLogging.Debug(MODULE_NAME,
                              "ReadAttributeForkData: Need to read overflow extents for attribute '{0}' (CNID={1})",
                              attrName,
                              fileID);

            // For now, we'll just use what we have
            // A full implementation would read overflow extents from the attributes B-tree
        }

        // Read each extent
        foreach(HFSPlusExtentDescriptor extent in allExtents)
        {
            if(extent.blockCount == 0) break;

            ulong extentSizeBytes = (ulong)extent.blockCount * _volumeHeader.blockSize;
            ulong toRead          = Math.Min(extentSizeBytes, logicalSize - bytesRead);

            // Calculate sector offset for this extent
            ulong blockOffsetBytes = (ulong)extent.startBlock * _volumeHeader.blockSize;

            ulong deviceSector = ((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffsetBytes) /
                                 _sectorSize;

            var byteOffset = (uint)(((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffsetBytes) %
                                    _sectorSize);

            var sectorsToRead = (uint)((toRead + byteOffset + _sectorSize - 1) / _sectorSize);

            ErrorNumber readErr = _imagePlugin.ReadSectors(deviceSector,
                                                           false,
                                                           sectorsToRead,
                                                           out byte[] sectorData,
                                                           out _);

            if(readErr != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadAttributeForkData: Failed to read sectors: {0}", readErr);

                return readErr;
            }

            if(sectorData == null || sectorData.Length == 0)
            {
                AaruLogging.Debug(MODULE_NAME, "ReadAttributeForkData: Got empty sector data");

                return ErrorNumber.InvalidArgument;
            }

            // Copy data from sector buffer to output buffer
            uint bytesToCopy = Math.Min((uint)toRead, (uint)(sectorData.Length - byteOffset));

            Array.Copy(sectorData, (int)byteOffset, buf, (int)bytesRead, bytesToCopy);

            bytesRead += bytesToCopy;

            if(bytesRead >= logicalSize) break;
        }

        if(bytesRead < logicalSize)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "ReadAttributeForkData: Warning - Read only {0} bytes out of {1} bytes for attribute '{2}' (CNID={3})",
                              bytesRead,
                              logicalSize,
                              attrName,
                              fileID);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the attributes B-tree header</summary>
    /// <param name="header">Output B-tree header</param>
    /// <returns>Error number</returns>
    private ErrorNumber ReadAttributesBTreeHeader(out BTHeaderRec header)
    {
        header = default(BTHeaderRec);

        if(_attributesFile == null) return ErrorNumber.NotSupported;

        HFSPlusForkData attrFork = _attributesFile.Value;

        if(attrFork.totalBlocks                             == 0    ||
           attrFork.extents.extentDescriptors               == null ||
           attrFork.extents.extentDescriptors.Length        == 0    ||
           attrFork.extents.extentDescriptors[0].blockCount == 0)
            return ErrorNumber.InvalidArgument;

        HFSPlusExtentDescriptor firstExtent = attrFork.extents.extentDescriptors[0];

        // Calculate byte offset of the attributes file
        ulong attrFileOffset = firstExtent.startBlock * _volumeHeader.blockSize;

        // Convert to device sector address
        ulong deviceSector = ((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + attrFileOffset) / _sectorSize;

        var byteOffset = (uint)(((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + attrFileOffset) %
                                _sectorSize);

        // Read the header node (assume 8192 bytes as typical for attributes file)
        uint sectorsToRead = (8192 + byteOffset + _sectorSize - 1) / _sectorSize;

        ErrorNumber errno =
            _imagePlugin.ReadSectors(deviceSector, false, sectorsToRead, out byte[] headerSectors, out _);

        if(errno != ErrorNumber.NoError) return errno;

        if(headerSectors.Length < byteOffset + System.Runtime.InteropServices.Marshal.SizeOf(typeof(BTNodeDescriptor)))
            return ErrorNumber.InvalidArgument;

        // Parse node descriptor
        int ndSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BTNodeDescriptor));

        BTNodeDescriptor nodeDesc =
            Marshal.ByteArrayToStructureBigEndian<BTNodeDescriptor>(headerSectors, (int)byteOffset, ndSize);

        if(nodeDesc.kind != BTNodeKind.kBTHeaderNode) return ErrorNumber.InvalidArgument;

        // Parse B-tree header (follows node descriptor)
        int bhSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BTHeaderRec));

        if(headerSectors.Length < byteOffset + ndSize + bhSize) return ErrorNumber.InvalidArgument;

        header = Marshal.ByteArrayToStructureBigEndian<BTHeaderRec>(headerSectors, (int)(byteOffset + ndSize), bhSize);

        return ErrorNumber.NoError;
    }

    /// <summary>Reads an attributes B-tree node by node number</summary>
    /// <param name="nodeNumber">Node number to read</param>
    /// <param name="nodeSize">Size of each node</param>
    /// <param name="nodeData">Output node data</param>
    /// <returns>Error number</returns>
    private ErrorNumber ReadAttributeNode(uint nodeNumber, ushort nodeSize, out byte[] nodeData)
    {
        nodeData = null;

        if(_attributesFile == null) return ErrorNumber.NotSupported;

        HFSPlusForkData attrFork = _attributesFile.Value;

        if(attrFork.extents.extentDescriptors == null || attrFork.extents.extentDescriptors.Length == 0)
            return ErrorNumber.InvalidArgument;

        // Calculate byte offset of node within the attributes file
        ulong nodeOffset = (ulong)nodeNumber * nodeSize;

        // Find which extent contains this offset
        ulong currentOffset = 0;

        foreach(HFSPlusExtentDescriptor extent in attrFork.extents.extentDescriptors)
        {
            if(extent.blockCount == 0) break;

            ulong extentSizeInBytes = (ulong)extent.blockCount * _volumeHeader.blockSize;

            if(nodeOffset < currentOffset + extentSizeInBytes)
            {
                // Found the extent containing this node
                ulong offsetInExtent = nodeOffset                                         - currentOffset;
                ulong blockOffset    = (ulong)extent.startBlock * _volumeHeader.blockSize + offsetInExtent;

                // Convert to device sector address
                ulong deviceSector = ((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffset) /
                                     _sectorSize;

                var byteOffset = (uint)(((_partitionStart + _hfsPlusVolumeOffset) * _sectorSize + blockOffset) %
                                        _sectorSize);

                uint sectorsToRead = (nodeSize + byteOffset + _sectorSize - 1) / _sectorSize;

                ErrorNumber errno =
                    _imagePlugin.ReadSectors(deviceSector, false, sectorsToRead, out byte[] sectorData, out _);

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