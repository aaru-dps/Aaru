// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DirHelpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Universal Disk Format plugin.
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
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class UDF
{
    /// <summary>
    ///     Reads the contents of a directory from its ICB
    /// </summary>
    /// <param name="icb">ICB of the directory</param>
    /// <param name="entries">Dictionary of directory entries keyed by filename</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadDirectoryContents(LongAllocationDescriptor icb, out Dictionary<string, UdfDirectoryEntry> entries)
    {
        // Case-sensitive: UDF volumes can contain sibling names differing only in case
        entries = [];

        // Read the File Entry for this directory (using partition-aware read for metadata partition support)
        ErrorNumber errno = ReadSectorFromPartition(icb.extentLocation.logicalBlockNumber,
                                                    icb.extentLocation.partitionReferenceNumber,
                                                    _partitionStartingLocation,
                                                    out byte[] feBuffer);

        if(errno != ErrorNumber.NoError) return errno;

        // Parse as unified file entry info (handles both FileEntry and ExtendedFileEntry)
        errno = ParseFileEntryInfo(feBuffer, out UdfFileEntryInfo fileEntryInfo);

        if(errno != ErrorNumber.NoError) return errno;

        // Check this is a directory
        if(fileEntryInfo.IcbTag.fileType != FileType.Directory) return ErrorNumber.NotDirectory;

        // Read directory data based on allocation descriptor type
        // The allocation descriptor type is in bits 0-2 of the flags
        var adType = (byte)((ushort)fileEntryInfo.IcbTag.flags & 0x07);

        errno = ReadFileDataFromInfo(fileEntryInfo,
                                     feBuffer,
                                     adType,
                                     icb.extentLocation.partitionReferenceNumber,
                                     out byte[] directoryData);

        if(errno != ErrorNumber.NoError) return errno;

        // Parse File Identifier Descriptors from directory data
        var offset  = 0;
        int fidSize = System.Runtime.InteropServices.Marshal.SizeOf<FileIdentifierDescriptor>();

        while(offset < directoryData.Length)
        {
            // Check we have enough data for the fixed part of the FID
            if(offset + fidSize > directoryData.Length) break;

            FileIdentifierDescriptor fid =
                Marshal.ByteArrayToStructureLittleEndian<FileIdentifierDescriptor>(directoryData, offset, fidSize);

            if(fid.tag.tagIdentifier != TagIdentifier.FileIdentifierDescriptor) break;

            // Calculate the total length of this FID entry
            // Fixed part + lengthOfImplementationUse + lengthOfFileIdentifier + padding to 4-byte boundary
            int fidLength = fidSize + fid.lengthOfImplementationUse + fid.lengthOfFileIdentifier;

            // Pad to 4-byte boundary
            fidLength = fidLength + 3 & ~3;

            // Skip parent directory entry (has Parent flag and empty filename)
            if(fid.fileCharacteristics.HasFlag(FileCharacteristics.Parent))
            {
                offset += fidLength;

                continue;
            }

            // Skip deleted entries
            if(fid.fileCharacteristics.HasFlag(FileCharacteristics.Deleted))
            {
                offset += fidLength;

                continue;
            }

            // Extract the filename
            // The filename is at offset fidSize + lengthOfImplementationUse
            int filenameOffset = offset + fidSize + fid.lengthOfImplementationUse;

            if(filenameOffset + fid.lengthOfFileIdentifier > directoryData.Length) break;

            var filenameBytes = new byte[fid.lengthOfFileIdentifier];
            Array.Copy(directoryData, filenameOffset, filenameBytes, 0, fid.lengthOfFileIdentifier);

            string filename = StringHandlers.DecompressUnicode(filenameBytes);

            if(!string.IsNullOrEmpty(filename))
            {
                entries[filename] = new UdfDirectoryEntry
                {
                    Filename            = filename,
                    Icb                 = fid.icb,
                    FileCharacteristics = fid.fileCharacteristics
                };
            }

            offset += fidLength;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Reads file data from a FileEntry based on allocation descriptor type
    /// </summary>
    /// <param name="fileEntry">The file entry</param>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="adType">Allocation descriptor type (0-3)</param>
    /// <param name="data">The file data</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadFileData(FileEntry  fileEntry, byte[] feBuffer, byte adType, ushort partitionReferenceNumber,
                             out byte[] data)
    {
        data = null;

        // The allocation descriptors follow the extended attributes in the FileEntry
        // FileEntry fixed size is 176 bytes
        const int fileEntryFixedSize = 176;
        int       adOffset           = fileEntryFixedSize + (int)fileEntry.lengthOfExtendedAttributes;
        var       adLength           = (int)fileEntry.lengthOfAllocationDescriptors;


        switch(adType)
        {
            case 0: // Short Allocation Descriptors
                return ReadDataFromShortAd(feBuffer,
                                           adOffset,
                                           adLength,
                                           (long)fileEntry.informationLength,
                                           partitionReferenceNumber,
                                           out data);

            case 1: // Long Allocation Descriptors
                return ReadDataFromLongAd(feBuffer, adOffset, adLength, (long)fileEntry.informationLength, out data);

            case 2: // Extended Allocation Descriptors - not supported in UDF 1.02
                return ErrorNumber.NotSupported;

            case 3: // Data is embedded in the allocation descriptor area
                data = new byte[fileEntry.informationLength];
                Array.Copy(feBuffer, adOffset, data, 0, (int)fileEntry.informationLength);

                return ErrorNumber.NoError;

            default:
                return ErrorNumber.InvalidArgument;
        }
    }

    /// <summary>
    ///     Reads file data using Short Allocation Descriptors (8-byte descriptors with partition-relative addresses)
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="adOffset">Offset within the buffer where allocation descriptors start</param>
    /// <param name="adLength">Total length of allocation descriptors in bytes</param>
    /// <param name="dataLength">Expected file data length</param>
    /// <param name="data">The file data read from the extents</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadDataFromShortAd(byte[] feBuffer,                 int        adOffset, int adLength, long dataLength,
                                    ushort partitionReferenceNumber, out byte[] data)
    {
        data = null;

        // Sanity check: reject files larger than 1GB to prevent memory exhaustion
        if(dataLength > 1073741824) // 1GB
            return ErrorNumber.InvalidArgument;

        ErrorNumber errno = CollectAllocationDescriptors(feBuffer,
                                                         adOffset,
                                                         adLength,
                                                         0,
                                                         partitionReferenceNumber,
                                                         out List<UdfExtent> extents);

        if(errno != ErrorNumber.NoError) return errno;

        return ReadExtentsFull(extents, dataLength, out data);
    }

    /// <summary>
    ///     Reads file data using Long Allocation Descriptors (16-byte descriptors with partition reference numbers)
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="adOffset">Offset within the buffer where allocation descriptors start</param>
    /// <param name="adLength">Total length of allocation descriptors in bytes</param>
    /// <param name="dataLength">Expected file data length</param>
    /// <param name="data">The file data read from the extents</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadDataFromLongAd(byte[] feBuffer, int adOffset, int adLength, long dataLength, out byte[] data)
    {
        data = null;

        // Sanity check: reject files larger than 1GB to prevent memory exhaustion
        if(dataLength > 1073741824) // 1GB
            return ErrorNumber.InvalidArgument;

        // Long ADs carry their own partition reference; the "default" value only applies to short ADs,
        // so we pass 0 here (unused for adType==1).
        ErrorNumber errno = CollectAllocationDescriptors(feBuffer,
                                                         adOffset,
                                                         adLength,
                                                         1,
                                                         0,
                                                         out List<UdfExtent> extents);

        if(errno != ErrorNumber.NoError) return errno;

        return ReadExtentsFull(extents, dataLength, out data);
    }

    /// <summary>
    ///     Finds a directory entry by name: exact match first (case-differing siblings are legal on UDF), then
    ///     case-insensitively. The exact match is a dictionary lookup so huge directories are not scanned per file.
    /// </summary>
    /// <param name="entries">Directory entries</param>
    /// <param name="name">Name to look for</param>
    /// <param name="entry">Found entry</param>
    /// <returns>True if found</returns>
    static bool TryGetEntry(Dictionary<string, UdfDirectoryEntry> entries, string name, out UdfDirectoryEntry entry)
    {
        if(entries.TryGetValue(name, out entry)) return true;

        foreach(KeyValuePair<string, UdfDirectoryEntry> kvp in entries)
        {
            if(!kvp.Key.Equals(name, StringComparison.OrdinalIgnoreCase)) continue;

            entry = kvp.Value;

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Traverses the directory tree to find a directory at a given path
    /// </summary>
    /// <param name="path">Path to the directory</param>
    /// <param name="entries">Directory entries if found</param>
    /// <returns>Error number</returns>
    ErrorNumber GetDirectoryEntries(string path, out Dictionary<string, UdfDirectoryEntry> entries)
    {
        entries = null;

        if(string.IsNullOrWhiteSpace(path) || path == "/")
        {
            entries = _rootDirectoryCache;

            return ErrorNumber.NoError;
        }

        string cutPath = path.StartsWith("/", StringComparison.Ordinal) ? path[1..] : path;

        // Check cache first
        if(_directoryCache.TryGetValue(cutPath.ToLowerInvariant(), out entries)) return ErrorNumber.NoError;

        string[] pieces = cutPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        Dictionary<string, UdfDirectoryEntry> currentDirectory = _rootDirectoryCache;
        var                                   currentPath      = "";

        for(var i = 0; i < pieces.Length; i++)
        {
            // Find the entry in current directory
            if(!TryGetEntry(currentDirectory, pieces[i], out UdfDirectoryEntry entry)) return ErrorNumber.NoSuchFile;

            if(!entry.FileCharacteristics.HasFlag(FileCharacteristics.Directory)) return ErrorNumber.NotDirectory;

            currentPath = i == 0 ? pieces[0] : $"{currentPath}/{pieces[i]}";

            // Check cache for this path
            if(_directoryCache.TryGetValue(currentPath.ToLowerInvariant(), out currentDirectory)) continue;

            // Read directory contents
            ErrorNumber errno = ReadDirectoryContents(entry.Icb, out currentDirectory);

            if(errno != ErrorNumber.NoError) return errno;

            // Cache this directory
            _directoryCache[currentPath.ToLowerInvariant()] = currentDirectory;
        }

        entries = currentDirectory;

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Parses a buffer containing either a FileEntry or ExtendedFileEntry into a unified UdfFileEntryInfo
    /// </summary>
    /// <param name="feBuffer">The buffer containing the file entry</param>
    /// <param name="info">The parsed file entry info</param>
    /// <returns>Error number</returns>
    ErrorNumber ParseFileEntryInfo(byte[] feBuffer, out UdfFileEntryInfo info)
    {
        info = null;

        if(feBuffer == null || feBuffer.Length < 16) return ErrorNumber.InvalidArgument;

        var tagId = (TagIdentifier)BitConverter.ToUInt16(feBuffer, 0);

        switch(tagId)
        {
            case TagIdentifier.FileEntry:
            {
                FileEntry fe = Marshal.ByteArrayToStructureLittleEndian<FileEntry>(feBuffer);

                info = new UdfFileEntryInfo
                {
                    IsExtended                    = false,
                    IcbTag                        = fe.icbTag,
                    Uid                           = fe.uid,
                    Gid                           = fe.gid,
                    Permissions                   = fe.permissions,
                    FileLinkCount                 = fe.fileLinkCount,
                    InformationLength             = fe.informationLength,
                    LogicalBlocksRecorded         = fe.logicalBlocksRecorded,
                    AccessTime                    = fe.accessTime,
                    ModificationTime              = fe.modificationTime,
                    CreationTime                  = default(Timestamp), // Not available in FileEntry
                    AttributeTime                 = fe.attributeTime,
                    ExtendedAttributeICB          = fe.extendedAttributeICB,
                    StreamDirectoryICB            = default(LongAllocationDescriptor), // Not available in FileEntry
                    UniqueId                      = fe.uniqueId,
                    LengthOfExtendedAttributes    = fe.lengthOfExtendedAttributes,
                    LengthOfAllocationDescriptors = fe.lengthOfAllocationDescriptors
                };

                return ErrorNumber.NoError;
            }

            case TagIdentifier.ExtendedFileEntry:
            {
                ExtendedFileEntry efe = Marshal.ByteArrayToStructureLittleEndian<ExtendedFileEntry>(feBuffer);

                info = new UdfFileEntryInfo
                {
                    IsExtended                    = true,
                    IcbTag                        = efe.icbTag,
                    Uid                           = efe.uid,
                    Gid                           = efe.gid,
                    Permissions                   = efe.permissions,
                    FileLinkCount                 = efe.fileLinkCount,
                    InformationLength             = efe.informationLength,
                    LogicalBlocksRecorded         = efe.logicalBlocksRecorded,
                    AccessTime                    = efe.accessTime,
                    ModificationTime              = efe.modificationTime,
                    CreationTime                  = efe.creationTime,
                    AttributeTime                 = efe.attributeTime,
                    ExtendedAttributeICB          = efe.extendedAttributeICB,
                    StreamDirectoryICB            = efe.streamDirectoryICB,
                    UniqueId                      = efe.uniqueId,
                    LengthOfExtendedAttributes    = efe.lengthOfExtendedAttributes,
                    LengthOfAllocationDescriptors = efe.lengthOfAllocationDescriptors
                };

                return ErrorNumber.NoError;
            }

            default:
                return ErrorNumber.InvalidArgument;
        }
    }

    /// <summary>
    ///     Reads file data from a UdfFileEntryInfo based on allocation descriptor type
    /// </summary>
    /// <param name="info">The file entry info</param>
    /// <param name="feBuffer">The buffer containing the file entry sector</param>
    /// <param name="adType">Allocation descriptor type (0-3)</param>
    /// <param name="data">The file data</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadFileDataFromInfo(UdfFileEntryInfo info,                     byte[]     feBuffer, byte adType,
                                     ushort           partitionReferenceNumber, out byte[] data)
    {
        data = null;

        // Sanity check: reject files larger than 1GB to prevent memory exhaustion
        if(info.InformationLength > 1073741824) // 1GB
            return ErrorNumber.InvalidArgument;

        // The allocation descriptors follow the extended attributes in the file entry
        // FileEntry fixed size is 176 bytes, ExtendedFileEntry fixed size is 216 bytes
        int fixedSize = info.IsExtended ? 216 : 176;
        int adOffset  = fixedSize + (int)info.LengthOfExtendedAttributes;
        var adLength  = (int)info.LengthOfAllocationDescriptors;

        switch(adType)
        {
            case 0: // Short Allocation Descriptors
                return ReadDataFromShortAd(feBuffer,
                                           adOffset,
                                           adLength,
                                           (long)info.InformationLength,
                                           partitionReferenceNumber,
                                           out data);

            case 1: // Long Allocation Descriptors
                return ReadDataFromLongAd(feBuffer, adOffset, adLength, (long)info.InformationLength, out data);

            case 2: // Extended Allocation Descriptors - not fully supported
                return ErrorNumber.NotSupported;

            case 3: // Data is embedded in the allocation descriptor area
                data = new byte[info.InformationLength];
                Array.Copy(feBuffer, adOffset, data, 0, (int)info.InformationLength);

                return ErrorNumber.NoError;

            default:
                return ErrorNumber.InvalidArgument;
        }
    }

    /// <summary>
    ///     Reads a specific byte range from a file using Short Allocation Descriptors
    ///     without loading the entire file into memory
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="adOffset">Offset within the buffer where allocation descriptors start</param>
    /// <param name="adLength">Total length of allocation descriptors in bytes</param>
    /// <param name="fileOffset">Byte offset in the file to start reading from</param>
    /// <param name="readLength">Number of bytes to read</param>
    /// <param name="buffer">Buffer to read into</param>
    /// <param name="bytesRead">Number of bytes actually read</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadDataFromShortAdRange(byte[] feBuffer, int adOffset, int adLength, long fileOffset, long readLength,
                                         byte[] buffer,   ushort partitionReferenceNumber, out long bytesRead)
    {
        bytesRead = 0;

        ErrorNumber errno = CollectAllocationDescriptors(feBuffer,
                                                         adOffset,
                                                         adLength,
                                                         0,
                                                         partitionReferenceNumber,
                                                         out List<UdfExtent> extents);

        if(errno != ErrorNumber.NoError) return errno;

        AaruLogging.Debug(MODULE_NAME,
                          "ReadDataFromShortAdRange: collected {0} extents, fileOffset={1}, readLength={2}",
                          extents.Count,
                          fileOffset,
                          readLength);

        return ReadExtentsRange(extents, fileOffset, readLength, buffer, out bytesRead);
    }

    /// <summary>
    ///     Reads a specific byte range from a file using Long Allocation Descriptors
    ///     without loading the entire file into memory
    /// </summary>
    ErrorNumber ReadDataFromLongAdRange(byte[] feBuffer, int adOffset, int adLength, long fileOffset, long readLength,
                                        byte[] buffer,   out long bytesRead)
    {
        bytesRead = 0;

        ErrorNumber errno = CollectAllocationDescriptors(feBuffer,
                                                         adOffset,
                                                         adLength,
                                                         1,
                                                         0,
                                                         out List<UdfExtent> extents);

        if(errno != ErrorNumber.NoError) return errno;

        return ReadExtentsRange(extents, fileOffset, readLength, buffer, out bytesRead);
    }

    /// <summary>
    ///     Walks the chain of allocation descriptors starting at the given offset inside the file entry buffer,
    ///     transparently following any type-3 <c>EXT_NEXT_EXTENT_ALLOCDESCS</c> continuation pointers,
    ///     and returns a flat list of data extents (types 0, 1, and 2).
    /// </summary>
    /// <param name="feBuffer">Buffer currently holding allocation descriptors (initially the file entry sector)</param>
    /// <param name="adOffset">Offset within <paramref name="feBuffer" /> where allocation descriptors start</param>
    /// <param name="adLength">Length in bytes of the allocation descriptor area</param>
    /// <param name="adType">Allocation descriptor type (0 = short, 1 = long)</param>
    /// <param name="defaultPartitionReference">Default partition reference (used for short descriptors)</param>
    /// <param name="extents">Resulting flat list of data extents</param>
    /// <returns>Error number</returns>
    ErrorNumber CollectAllocationDescriptors(byte[] feBuffer,                  int adOffset, int adLength, byte adType,
                                             ushort defaultPartitionReference, out List<UdfExtent> extents)
    {
        extents = [];

        if(adType != 0 && adType != 1) return ErrorNumber.NotSupported;

        int sadSize   = System.Runtime.InteropServices.Marshal.SizeOf<ShortAllocationDescriptor>();
        int ladSize   = System.Runtime.InteropServices.Marshal.SizeOf<LongAllocationDescriptor>();
        int entrySize = adType == 0 ? sadSize : ladSize;

        // Size of the Allocation Extent Descriptor header (ECMA-167 4/14.5):
        // DescriptorTag (16) + previousAllocExtLocation (4) + lengthAllocDescs (4) = 24 bytes
        const int aedHeaderSize = 24;

        // Guard against runaway continuation chains (matches Linux UDF_MAX_INDIR_EXTS semantics)
        const int maxIndirections = 1024;
        var       indirections    = 0;

        byte[] curBuffer       = feBuffer;
        int    curOffset       = adOffset;
        int    curLength       = adLength;
        ushort curPartitionRef = defaultPartitionReference;

        while(true)
        {
            int adPos = curOffset;
            int adEnd = curOffset + curLength;

            if(adEnd > curBuffer.Length) adEnd = curBuffer.Length;

            var followedContinuation = false;

            while(adPos + entrySize <= adEnd)
            {
                uint   rawLength;
                uint   block;
                ushort partRef;

                if(adType == 0)
                {
                    ShortAllocationDescriptor sad =
                        Marshal.ByteArrayToStructureLittleEndian<ShortAllocationDescriptor>(curBuffer, adPos, sadSize);

                    rawLength = sad.extentLength;
                    block     = sad.extentLocation;
                    partRef   = curPartitionRef;
                }
                else
                {
                    LongAllocationDescriptor lad =
                        Marshal.ByteArrayToStructureLittleEndian<LongAllocationDescriptor>(curBuffer, adPos, ladSize);

                    rawLength = lad.extentLength;
                    block     = lad.extentLocation.logicalBlockNumber;
                    partRef   = lad.extentLocation.partitionReferenceNumber;
                }

                // Skip entries with rawLength == 0 (zero-length spacers may appear in
                // the middle of the AD area on some PS2 bridge discs). Linux stops here,
                // but Linux also can't read these discs at all. As a preservation tool we
                // walk the full lengthAllocDescs range and skip zero entries.
                if(rawLength == 0)
                {
                    adPos += entrySize;

                    continue;
                }

                uint length = rawLength & 0x3FFFFFFF;
                var  type   = (byte)(rawLength >> 30);

                AaruLogging.Debug(MODULE_NAME,
                                  "  AD@{0}: rawLen=0x{1:X8} type={2} len={3} block={4} partRef={5}",
                                  adPos,
                                  rawLength,
                                  type,
                                  length,
                                  block,
                                  partRef);

                if(type == 3)
                {
                    // Continuation pointer: stop processing this AD block and jump to the referenced one.
                    // On ANY failure following the continuation (I/O error, malformed AED header, runaway
                    // chain) fall back gracefully and return the extents collected so far with NoError.
                    // Linux's udf_next_aext is similarly forgiving: it does not validate the AED tag.
                    // Returning an error here would cause the whole extraction to bail out with a 0-byte
                    // output file for files with continuation chains, which is strictly worse than
                    // returning partial data.
                    if(++indirections > maxIndirections)
                    {
                        AaruLogging.Debug(MODULE_NAME,
                                          "    DIAG: maxIndirections exceeded ({0}), returning {1} extents",
                                          indirections,
                                          extents.Count);

                        return ErrorNumber.NoError;
                    }

                    // Read one sector first to get the AED header and determine the actual
                    // AD area size from lengthAllocDescs.
                    ErrorNumber errno = ReadSectorFromPartition(block,
                                                                partRef,
                                                                _partitionStartingLocation,
                                                                out byte[] contBuffer);

                    if(errno != ErrorNumber.NoError) return ErrorNumber.NoError;

                    if(contBuffer == null || contBuffer.Length < aedHeaderSize) return ErrorNumber.NoError;

                    // lengthAllocDescs lives at offset 20 of the Allocation Extent Descriptor
                    // (16-byte DescriptorTag + 4-byte previousAllocExtLocation). We intentionally do NOT
                    // validate the descriptor tag, matching Linux's udf_next_aext behavior.
                    long lengthAllocDescs = BitConverter.ToUInt32(contBuffer, 20);

                    // If the AED says 0 bytes of allocation descriptors, nothing more to walk.
                    if(lengthAllocDescs <= 0) return ErrorNumber.NoError;

                    // Clamp lengthAllocDescs to the type-3 extent length as a corruption guard.
                    // Per ECMA-167, the extent length should be >= aedHeaderSize + lengthAllocDescs.
                    if(length > 0 && lengthAllocDescs > length - aedHeaderSize)
                        lengthAllocDescs = length              - aedHeaderSize;

                    // If the AD area fits in the sector already read (Linux-style single-block),
                    // use it directly. Otherwise, read exactly the sectors needed for the AED.
                    // Some discs (PS2 bridge) have multi-block AED extents that are valid per
                    // ECMA-167 but don't use per-block daisy-chain type-3 pointers.
                    if(aedHeaderSize + lengthAllocDescs > contBuffer.Length)
                    {
                        var  totalAedBytes = (uint)(aedHeaderSize         + lengthAllocDescs);
                        uint aedSectors    = (totalAedBytes + _sectorSize - 1) / _sectorSize;

                        errno = ReadSectorsFromPartition(block,
                                                         partRef,
                                                         _partitionStartingLocation,
                                                         aedSectors,
                                                         out contBuffer);

                        if(errno != ErrorNumber.NoError) return ErrorNumber.NoError;

                        if(contBuffer == null || contBuffer.Length < aedHeaderSize) return ErrorNumber.NoError;
                    }

                    // Final clamp to actual buffer size.
                    long maxLengthAllocDescs = contBuffer.Length - aedHeaderSize;

                    if(lengthAllocDescs > maxLengthAllocDescs) lengthAllocDescs = maxLengthAllocDescs;

                    curBuffer       = contBuffer;
                    curOffset       = aedHeaderSize;
                    curLength       = (int)lengthAllocDescs;
                    curPartitionRef = partRef;

                    followedContinuation = true;

                    break; // restart outer loop with new buffer
                }

                extents.Add(new UdfExtent(block, partRef, length, type));

                adPos += entrySize;
            }

            if(!followedContinuation)
            {
                long totalExtentBytes = 0;

                foreach(UdfExtent e in extents) totalExtentBytes += e.Length;

                AaruLogging.Debug(MODULE_NAME,
                                  "CollectAllocationDescriptors: done, collected {0} extents, totalBytes={1}, indirections={2}",
                                  extents.Count,
                                  totalExtentBytes,
                                  indirections);

                return ErrorNumber.NoError;
            }
        }
    }

    /// <summary>
    ///     Reads all of <paramref name="dataLength" /> bytes from the flat extent list into a newly allocated buffer.
    ///     Sparse extents (types 1 and 2) are materialized as zeros.
    /// </summary>
    ErrorNumber ReadExtentsFull(List<UdfExtent> extents, long dataLength, out byte[] data)
    {
        data = new byte[dataLength];

        if(dataLength == 0) return ErrorNumber.NoError;

        return ReadExtentsRange(extents, 0, dataLength, data, out _);
    }

    /// <summary>
    ///     Reads a byte range from the flat extent list into <paramref name="buffer" />.
    ///     Sparse extents (types 1 and 2) contribute zeros to the output.
    /// </summary>
    ErrorNumber ReadExtentsRange(List<UdfExtent> extents, long fileOffset, long readLength, byte[] buffer,
                                 out long        bytesRead)
    {
        bytesRead = 0;

        if(readLength <= 0) return ErrorNumber.NoError;

        long currentOffset = 0;
        var  bufferOffset  = 0;
        long targetEnd     = fileOffset + readLength;

        foreach(UdfExtent extent in extents)
        {
            if(bytesRead >= readLength) break;

            uint extentLength = extent.Length;

            if(extentLength == 0) continue;

            long extentEnd = currentOffset + extentLength;

            if(currentOffset >= targetEnd) break;

            if(extentEnd > fileOffset)
            {
                long offsetInExtent = currentOffset < fileOffset ? fileOffset - currentOffset : 0;
                long toRead         = extentLength - offsetInExtent;

                if(currentOffset + offsetInExtent + toRead > targetEnd)
                    toRead = targetEnd - (currentOffset + offsetInExtent);

                if(toRead > 0)
                {
                    var copyLen = (int)Math.Min(toRead, buffer.Length - bufferOffset);

                    if(copyLen > 0)
                    {
                        if(extent.Type == 0)
                        {
                            // Recorded and allocated: read from disk
                            long sectorOffset            = offsetInExtent / _sectorSize;
                            long byteOffsetInFirstSector = offsetInExtent % _sectorSize;

                            var sectorsToRead =
                                (uint)((byteOffsetInFirstSector + copyLen + _sectorSize - 1) / _sectorSize);

                            ErrorNumber errno = ReadSectorsFromPartition((uint)(extent.LogicalBlock + sectorOffset),
                                                                         extent.PartitionReferenceNumber,
                                                                         _partitionStartingLocation,
                                                                         sectorsToRead,
                                                                         out byte[] extentData);

                            if(errno != ErrorNumber.NoError)
                            {
                                // On out-of-range or I/O error, zero-fill this region instead
                                // of aborting. PS2 bridge discs with non-standard multi-block
                                // AEDs may include some bogus extent entries mixed with valid
                                // ones. Returning partial zeros is better than truncating the
                                // entire file.
                                Array.Clear(buffer, bufferOffset, copyLen);
                                bytesRead    += copyLen;
                                bufferOffset += copyLen;

                                continue;
                            }

                            Array.Copy(extentData, (int)byteOffsetInFirstSector, buffer, bufferOffset, copyLen);
                        }
                        else
                        {
                            // Sparse (type 1: not recorded but allocated, type 2: not recorded and not allocated):
                            // data is defined to be all zeros.
                            Array.Clear(buffer, bufferOffset, copyLen);
                        }

                        bytesRead    += copyLen;
                        bufferOffset += copyLen;
                    }
                }
            }

            currentOffset += extentLength;
        }

        // If the extent list doesn't cover the requested range (sparse file whose allocation descriptors
        // describe fewer bytes than informationLength), the tail is implicitly a hole of zeros. Zero-fill
        // the remainder of the requested range. Linux's UDF driver does the same by returning a zero
        // page for any logical offset that maps beyond the last recorded extent.
        if(bytesRead < readLength && bufferOffset < buffer.Length)
        {
            long toZero = Math.Min(readLength - bytesRead, buffer.Length - bufferOffset);

            if(toZero > 0)
            {
                Array.Clear(buffer, bufferOffset, (int)toZero);
                bytesRead += toZero;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Reads a specific byte range from a file based on allocation descriptor type,
    ///     without loading the entire file into memory
    /// </summary>
    ErrorNumber ReadFileDataFromInfoRange(UdfFileEntryInfo info,       byte[] feBuffer, byte   adType, long fileOffset,
                                          long             readLength, byte[] buffer,   ushort partitionReferenceNumber,
                                          out long         bytesRead)
    {
        bytesRead = 0;

        // Sanity checks
        if((ulong)fileOffset >= info.InformationLength) return ErrorNumber.NoError;

        if(fileOffset < 0) return ErrorNumber.InvalidArgument;

        // Adjust read length to not exceed file bounds
        if(fileOffset + readLength > (long)info.InformationLength)
            readLength = (long)info.InformationLength - fileOffset;

        int fixedSize = info.IsExtended ? 216 : 176;
        int adOffset  = fixedSize + (int)info.LengthOfExtendedAttributes;
        var adLength  = (int)info.LengthOfAllocationDescriptors;

        switch(adType)
        {
            case 0: // Short Allocation Descriptors
                return ReadDataFromShortAdRange(feBuffer,
                                                adOffset,
                                                adLength,
                                                fileOffset,
                                                readLength,
                                                buffer,
                                                partitionReferenceNumber,
                                                out bytesRead);

            case 1: // Long Allocation Descriptors
                return ReadDataFromLongAdRange(feBuffer,
                                               adOffset,
                                               adLength,
                                               fileOffset,
                                               readLength,
                                               buffer,
                                               out bytesRead);

            case 2: // Extended Allocation Descriptors
                return ErrorNumber.NotSupported;

            case 3: // Data is embedded in the allocation descriptor area
                // For embedded data, we still need to read it all at once
                long toRead = Math.Min(readLength, (long)info.InformationLength - fileOffset);
                Array.Copy(feBuffer, adOffset + (int)fileOffset, buffer, 0, (int)toRead);
                bytesRead = toRead;

                return ErrorNumber.NoError;

            default:
                return ErrorNumber.InvalidArgument;
        }
    }

    /// <summary>
    ///     Reads the named streams (UDF 2.00+) for a file from its stream directory ICB
    /// </summary>
    /// <param name="streamDirIcb">The stream directory ICB</param>
    /// <param name="streams">List of named streams</param>
    /// <returns>Error number</returns>
    ErrorNumber ReadNamedStreams(LongAllocationDescriptor streamDirIcb, out List<UdfNamedStream> streams)
    {
        streams = [];

        // Stream directory ICB with all zeros means no streams
        if(streamDirIcb.extentLength == 0) return ErrorNumber.NoError;

        // Read the stream directory file entry (using partition-aware read for metadata partition support)
        ErrorNumber errno = ReadSectorFromPartition(streamDirIcb.extentLocation.logicalBlockNumber,
                                                    streamDirIcb.extentLocation.partitionReferenceNumber,
                                                    _partitionStartingLocation,
                                                    out byte[] sdBuffer);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ParseFileEntryInfo(sdBuffer, out UdfFileEntryInfo streamDirInfo);

        if(errno != ErrorNumber.NoError) return errno;

        // Read the stream directory data
        var adType = (byte)((ushort)streamDirInfo.IcbTag.flags & 0x07);

        errno = ReadFileDataFromInfo(streamDirInfo,
                                     sdBuffer,
                                     adType,
                                     streamDirIcb.extentLocation.partitionReferenceNumber,
                                     out byte[] streamDirData);

        if(errno != ErrorNumber.NoError) return errno;

        // Parse File Identifier Descriptors for each named stream
        var offset  = 0;
        int fidSize = System.Runtime.InteropServices.Marshal.SizeOf<FileIdentifierDescriptor>();

        while(offset < streamDirData.Length)
        {
            if(offset + fidSize > streamDirData.Length) break;

            FileIdentifierDescriptor fid =
                Marshal.ByteArrayToStructureLittleEndian<FileIdentifierDescriptor>(streamDirData, offset, fidSize);

            if(fid.tag.tagIdentifier != TagIdentifier.FileIdentifierDescriptor) break;

            int fidLength = fidSize + fid.lengthOfImplementationUse + fid.lengthOfFileIdentifier;
            fidLength = fidLength + 3 & ~3;

            // Skip parent entry
            if(fid.fileCharacteristics.HasFlag(FileCharacteristics.Parent))
            {
                offset += fidLength;

                continue;
            }

            // Extract stream name
            int filenameOffset = offset + fidSize + fid.lengthOfImplementationUse;

            if(filenameOffset + fid.lengthOfFileIdentifier > streamDirData.Length) break;

            var filenameBytes = new byte[fid.lengthOfFileIdentifier];
            Array.Copy(streamDirData, filenameOffset, filenameBytes, 0, fid.lengthOfFileIdentifier);

            string streamName = StringHandlers.DecompressUnicode(filenameBytes);

            if(!string.IsNullOrEmpty(streamName))
            {
                // Read the stream file entry to get its length (using partition-aware read)
                ErrorNumber streamErr = ReadSectorFromPartition(fid.icb.extentLocation.logicalBlockNumber,
                                                                fid.icb.extentLocation.partitionReferenceNumber,
                                                                _partitionStartingLocation,
                                                                out byte[] streamBuffer);

                if(streamErr == ErrorNumber.NoError)
                {
                    if(ParseFileEntryInfo(streamBuffer, out UdfFileEntryInfo streamInfo) == ErrorNumber.NoError)
                    {
                        streams.Add(new UdfNamedStream
                        {
                            Name      = streamName,
                            XattrName = MapStreamNameToXattr(streamName),
                            Icb       = fid.icb,
                            Length    = streamInfo.InformationLength
                        });
                    }
                }
            }

            offset += fidLength;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Maps a UDF named stream name to the appropriate xattr name
    /// </summary>
    /// <param name="streamName">The UDF stream name</param>
    /// <returns>The mapped xattr name</returns>
    static string MapStreamNameToXattr(string streamName) => streamName switch
                                                             {
                                                                 STREAM_MAC_RESOURCE_FORK => Xattrs
                                                                    .XATTR_APPLE_RESOURCE_FORK,
                                                                 STREAM_OS2_EA =>
                                                                     null, // OS/2 EAs are decoded separately
                                                                 STREAM_NT_ACL => "com.microsoft.ntacl",
                                                                 STREAM_UNIX_ACL => "org.posix.acl",
                                                                 STREAM_BACKUP =>
                                                                     null, // Backup time is handled separately
                                                                 STREAM_POWER_CAL => "org.osta.udf.powercalibration",
                                                                 _ => streamName // Return as-is for other streams
                                                             };
}