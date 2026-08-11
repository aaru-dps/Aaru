// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Index.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft NT File System plugin.
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
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class NTFS
{
    /// <summary>Reads the directory entries for a directory, caching them as the filesystem is read-only.</summary>
    /// <param name="mftRecordNumber">MFT record number of the directory.</param>
    /// <param name="entries">Output dictionary mapping file names to MFT references.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber GetDirectoryEntries(uint mftRecordNumber, out Dictionary<string, ulong> entries)
    {
        if(_directoryCache.TryGetValue(mftRecordNumber, out entries)) return ErrorNumber.NoError;

        ErrorNumber errno = ReadDirectoryEntries(mftRecordNumber, out entries);

        if(errno == ErrorNumber.NoError) _directoryCache[mftRecordNumber] = entries;

        return errno;
    }

    /// <summary>Reads the directory entries for an arbitrary directory given its MFT record number.</summary>
    /// <param name="mftRecordNumber">MFT record number of the directory.</param>
    /// <param name="entries">Output dictionary mapping file names to MFT references.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadDirectoryEntries(uint mftRecordNumber, out Dictionary<string, ulong> entries)
    {
        entries = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);

        ErrorNumber errno = ReadMftRecord(mftRecordNumber, out byte[] recordData);

        if(errno != ErrorNumber.NoError) return errno;

        MftRecord header = ParseMftRecordHeader(recordData);

        if(header.magic != NtfsRecordMagic.File)
        {
            AaruLogging.Debug(MODULE_NAME, "MFT record {0} has invalid magic", mftRecordNumber);

            return ErrorNumber.InvalidArgument;
        }

        if(!header.flags.HasFlag(MftRecordFlags.InUse))
        {
            AaruLogging.Debug(MODULE_NAME, "MFT record {0} is not in use", mftRecordNumber);

            return ErrorNumber.NoSuchFile;
        }

        // Check that this is a directory
        if(!header.flags.HasFlag(MftRecordFlags.IsDirectory))
        {
            AaruLogging.Debug(MODULE_NAME, "MFT record {0} is not a directory", mftRecordNumber);

            return ErrorNumber.NotDirectory;
        }

        return CacheDirectoryEntries(recordData, header, mftRecordNumber, entries);
    }

    /// <summary>Caches the root directory entries from the $INDEX_ROOT attribute in the root MFT record.</summary>
    /// <param name="recordData">Raw root directory MFT record data after USA fixup.</param>
    /// <param name="header">Parsed MFT record header.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber CacheRootDirectory(byte[] recordData, in MftRecord header) =>
        CacheDirectoryEntries(recordData, header, (uint)SystemFileNumber.Root, _rootDirectoryCache);

    /// <summary>Caches directory entries from the $INDEX_ROOT attribute in an MFT record.</summary>
    /// <param name="recordData">Raw directory MFT record data after USA fixup.</param>
    /// <param name="header">Parsed MFT record header.</param>
    /// <param name="mftRecordNumber">MFT record number (for attribute list traversal).</param>
    /// <param name="cache">Target dictionary to populate with file names and MFT references.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber CacheDirectoryEntries(byte[]                    recordData, in MftRecord header, uint mftRecordNumber,
                                      Dictionary<string, ulong> cache)
    {
        // Find the $INDEX_ROOT attribute across base + extension records
        ErrorNumber findErrno = FindAttributes(recordData,
                                               header,
                                               mftRecordNumber,
                                               AttributeType.IndexRoot,
                                               INDEX_NAME,
                                               out List<FoundAttribute> indexRootAttrs);

        if(findErrno != ErrorNumber.NoError) return findErrno;

        foreach(FoundAttribute attr in indexRootAttrs)
        {
            byte nonResident = attr.RecordData[attr.Offset + 8];

            if(nonResident != 0) continue;

            var valueOffset = BitConverter.ToUInt16(attr.RecordData, attr.Offset + 0x14);
            var valueLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 0x10);

            int valueStart = attr.Offset + valueOffset;

            if(valueStart + valueLength > attr.RecordData.Length) continue;

            // Parse the INDEX_ROOT header
            IndexRoot indexRoot =
                Marshal.ByteArrayToStructureLittleEndian<IndexRoot>(attr.RecordData, valueStart, (int)valueLength);

            // Only process directory ($FILE_NAME) indexes
            if(indexRoot.type != AttributeType.FileName) continue;

            // The IndexHeader starts at offset 0x10 within INDEX_ROOT
            int indexHeaderOffset = valueStart + 0x10;

            // Parse index entries starting from entries_offset relative to the IndexHeader
            int entriesStart = indexHeaderOffset + (int)indexRoot.index.entries_offset;
            int entriesEnd   = indexHeaderOffset + (int)indexRoot.index.index_length;

            ParseIndexEntries(attr.RecordData, entriesStart, entriesEnd, cache);

            // If the index has sub-nodes (LARGE_INDEX), we also need to read $INDEX_ALLOCATION
            if(indexRoot.index.flags.HasFlag(IndexHeaderFlags.LargeIndex))
            {
                // Assemble $INDEX_ALLOCATION data runs from all extents
                ErrorNumber idxAllocErrno = AssembleNonResidentRuns(mftRecordNumber,
                                                                    AttributeType.IndexAllocation,
                                                                    INDEX_NAME,
                                                                    out List<(long offset, long length)> idxDataRuns,
                                                                    out _,
                                                                    out _,
                                                                    out _,
                                                                    out _);

                if(idxAllocErrno == ErrorNumber.NoError && idxDataRuns.Count > 0) ReadIndexBlocks(idxDataRuns, cache);
            }

            return ErrorNumber.NoError;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "Could not find $INDEX_ROOT attribute in directory MFT record {0}",
                          mftRecordNumber);

        return ErrorNumber.InvalidArgument;
    }

    /// <summary>Parses index entries from index data and caches file names and MFT references.</summary>
    /// <param name="data">Buffer containing the index entries.</param>
    /// <param name="start">Start offset of the first entry.</param>
    /// <param name="end">End offset (exclusive) of the entries area.</param>
    /// <param name="cache">Target dictionary to populate with file names and MFT references.</param>
    void ParseIndexEntries(byte[] data, int start, int end, Dictionary<string, ulong> cache)
    {
        int pos = start;

        while(pos + Marshal.SizeOf<IndexEntryHeader>() <= end)
        {
            IndexEntryHeader entryHeader =
                Marshal.ByteArrayToStructureLittleEndian<IndexEntryHeader>(data,
                                                                           pos,
                                                                           Marshal.SizeOf<IndexEntryHeader>());

            // End entry marker
            if(entryHeader.flags.HasFlag(IndexEntryFlags.End)) break;

            if(entryHeader.length == 0) break;

            // The key (FILE_NAME attribute) follows the IndexEntryHeader (16 bytes)
            if(entryHeader.key_length > 0)
            {
                int fileNameOffset = pos + Marshal.SizeOf<IndexEntryHeader>();

                if(fileNameOffset + Marshal.SizeOf<FileNameAttribute>() <= data.Length)
                {
                    FileNameAttribute fnAttr =
                        Marshal.ByteArrayToStructureLittleEndian<FileNameAttribute>(data,
                            fileNameOffset,
                            Marshal.SizeOf<FileNameAttribute>());

                    // Skip DOS-only names when a Win32 name (or Win32+DOS) exists
                    if(fnAttr.file_name_type != FileNameNamespace.Dos)
                    {
                        // File name characters follow the FileNameAttribute struct
                        int nameDataOffset = fileNameOffset + Marshal.SizeOf<FileNameAttribute>();

                        int nameBytes = fnAttr.file_name_length * 2;

                        if(nameDataOffset + nameBytes <= data.Length && fnAttr.file_name_length > 0)
                        {
                            string fileName = Encoding.Unicode.GetString(data, nameDataOffset, nameBytes);

                            // Skip . and .. entries to avoid infinite recursion
                            if(fileName is "." or "..")
                            {
                                pos += entryHeader.length;

                                continue;
                            }

                            // The MFT reference is the lower 48 bits of indexed_file_or_data
                            ulong mftRef = entryHeader.indexed_file_or_data & 0x0000FFFFFFFFFFFF;

                            cache.TryAdd(fileName, mftRef);
                        }
                    }
                }
            }

            pos += entryHeader.length;
        }
    }

    /// <summary>
    ///     Parses the NTFS data run (mapping pairs) encoding to produce a list of (cluster offset, cluster length)
    ///     pairs.
    /// </summary>
    /// <param name="data">Buffer containing the mapping pairs.</param>
    /// <param name="offset">Start offset of the mapping pairs.</param>
    /// <param name="end">End offset (exclusive) of the attribute.</param>
    /// <returns>List of (absolute cluster offset, length in clusters) tuples.</returns>
    List<(long offset, long length)> ParseDataRuns(byte[] data, int offset, int end)
    {
        List<(long offset, long length)> runs          = [];
        long                             currentOffset = 0;

        if(end > data.Length) end = data.Length;

        while(offset < end)
        {
            byte header = data[offset];

            if(header == 0) break;

            int lengthSize = header      & 0x0F;
            int offsetSize = header >> 4 & 0x0F;

            offset++;

            if(offset + lengthSize + offsetSize > end) break;

            // Read run length (unsigned)
            long runLength = 0;

            for(var i = 0; i < lengthSize; i++) runLength |= (long)data[offset + i] << i * 8;

            offset += lengthSize;

            // Read run offset (signed, relative to previous)
            long runOffset = 0;

            for(var i = 0; i < offsetSize; i++) runOffset |= (long)data[offset + i] << i * 8;

            // Sign-extend if negative
            if(offsetSize > 0 && (data[offset + offsetSize - 1] & 0x80) != 0)
                for(int i = offsetSize; i < 8; i++)
                    runOffset |= (long)0xFF << i * 8;

            offset += offsetSize;

            // Sparse run (offset == 0 with no offset bytes)
            if(offsetSize == 0)
            {
                runs.Add((0, runLength));

                continue;
            }

            currentOffset += runOffset;
            runs.Add((currentOffset, runLength));
        }

        return runs;
    }

    /// <summary>Reads index blocks from data runs and parses their entries into the directory cache.</summary>
    /// <param name="dataRuns">List of (absolute cluster offset, length in clusters) tuples.</param>
    /// <param name="cache">Target dictionary to populate with file names and MFT references.</param>
    void ReadIndexBlocks(List<(long offset, long length)> dataRuns, Dictionary<string, ulong> cache)
    {
        foreach((long clusterOffset, long clusterLength) in dataRuns)
        {
            // Skip sparse runs
            if(clusterOffset == 0 && clusterLength > 0) continue;

            long sectorStart  = clusterOffset * _sectorsPerCluster;
            long totalSectors = clusterLength * _sectorsPerCluster;

            // Read all sectors in this run
            ErrorNumber errno = _image.ReadSectors(_partition.Start + (ulong)sectorStart,
                                                   false,
                                                   (uint)totalSectors,
                                                   out byte[] runData,
                                                   out _);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "Error reading index allocation at cluster {0}: {1}",
                                  clusterOffset,
                                  errno);

                continue;
            }

            // Parse each index block within this run
            for(long blockOffset = 0;
                (ulong)blockOffset + _indexBlockSize <= (ulong)runData.Length;
                blockOffset += _indexBlockSize)
            {
                var blockData = new byte[_indexBlockSize];
                Array.Copy(runData, blockOffset, blockData, 0, _indexBlockSize);

                // Apply USA fixup to this index block
                errno = ApplyUsaFixup(blockData);

                if(errno != ErrorNumber.NoError) continue;

                // Verify INDX magic
                var magic = (NtfsRecordMagic)BitConverter.ToUInt32(blockData, 0);

                if(magic != NtfsRecordMagic.Indx) continue;

                // Parse the IndexBlock header to find entries
                IndexBlock indexBlock =
                    Marshal.ByteArrayToStructureLittleEndian<IndexBlock>(blockData, 0, Marshal.SizeOf<IndexBlock>());

                // Index header is at offset 0x18 within the IndexBlock
                var indexHeaderOffset = 0x18;
                int entriesStart      = indexHeaderOffset + (int)indexBlock.index.entries_offset;
                int entriesEnd        = indexHeaderOffset + (int)indexBlock.index.index_length;

                ParseIndexEntries(blockData, entriesStart, entriesEnd, cache);
            }
        }
    }
}