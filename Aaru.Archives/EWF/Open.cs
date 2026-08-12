// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Open.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : EWF logical evidence plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Opens Expert Witness Format logical evidence files.
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
using System.IO;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Archives;

public sealed partial class EwfArchive
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < SIGNATURE_LENGTH) return ErrorNumber.InvalidArgument;

        Stream stream = filter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        var signatureBytes = new byte[SIGNATURE_LENGTH];
        stream.ReadExactly(signatureBytes, 0, SIGNATURE_LENGTH);

        _isV2 = signatureBytes.AsSpan().SequenceEqual(LEF2_SIGNATURE);

        // Discover segment files
        _segmentStreams = [];
        List<IFilter> segmentFilters = DiscoverSegments(filter);

        foreach(IFilter f in segmentFilters) _segmentStreams.Add(f.GetDataForkStream());

        // Initialize data structures
        _chunkTable = new Dictionary<ulong, (int segmentIndex, long offset, uint size, bool compressed)>();
        _chunkCache = new Dictionary<ulong, byte[]>();
        _entries    = [];

        _sectorsPerChunk = DEFAULT_SECTORS_PER_CHUNK;
        _bytesPerSector  = DEFAULT_BYTES_PER_SECTOR;
        _chunkSize       = DEFAULT_CHUNK_SIZE;

        ulong currentChunk = 0;
        var   volumeFound  = false;

        byte[] ltreeDecompressed = null;

        // Process each segment file
        for(var segIdx = 0; segIdx < _segmentStreams.Count; segIdx++)
        {
            Stream segStream = _segmentStreams[segIdx];

            if(_isV2)
                ParseSegmentV2(segStream, segIdx, ref currentChunk, ref volumeFound, ref ltreeDecompressed);
            else
                ParseSegmentV1(segStream, segIdx, ref currentChunk, ref volumeFound, ref ltreeDecompressed);
        }

        // Reject chunk geometries that are zero or overflow, chunk size is used as divisor
        if(_sectorsPerChunk == 0 || _bytesPerSector == 0 || (ulong)_sectorsPerChunk * _bytesPerSector > int.MaxValue)
            return ErrorNumber.InvalidArgument;

        _chunkSize     = _sectorsPerChunk * _bytesPerSector;
        _maxChunkCache = (int)(MAX_CACHE_SIZE / _chunkSize);

        if(_maxChunkCache < 1) _maxChunkCache = 1;

        // Parse ltree data into file entries
        if(ltreeDecompressed is { Length: > 0 }) _entries = ParseLtreeData(ltreeDecompressed);

        AaruLogging.Debug(MODULE_NAME, "Parsed {0} file entries from ltree", _entries.Count);
        AaruLogging.Debug(MODULE_NAME, "Chunk table has {0} entries",        _chunkTable.Count);

        Opened = true;

        return ErrorNumber.NoError;
    }

#endregion

    List<IFilter> DiscoverSegments(IFilter primaryFilter)
    {
        List<IFilter> segments    = [primaryFilter];
        string        currentPath = primaryFilter.BasePath;

        while(true)
        {
            string nextPath = GetNextSegmentFilename(currentPath);

            if(nextPath == null) break;

            IFilter nextFilter = PluginRegister.Singleton.GetFilter(nextPath);

            if(nextFilter == null) break;

            segments.Add(nextFilter);
            currentPath = nextPath;
        }

        AaruLogging.Debug(MODULE_NAME, "Found {0} segment files", segments.Count);

        return segments;
    }

    void ParseSegmentV1(Stream     segStream, int segIdx, ref ulong currentChunk, ref bool volumeFound,
                        ref byte[] ltreeDecompressed)
    {
        segStream.Seek(FILE_HEADER_V1_SIZE, SeekOrigin.Begin);

        while(segStream.Position < segStream.Length)
        {
            long sectionStart = segStream.Position;

            var descBytes = new byte[SECTION_DESCRIPTOR_V1_SIZE];
            int bytesRead = segStream.Read(descBytes, 0, SECTION_DESCRIPTOR_V1_SIZE);

            if(bytesRead < SECTION_DESCRIPTOR_V1_SIZE) break;

            EwfSectionDescriptorV1 descriptor =
                Marshal.ByteArrayToStructureLittleEndian<EwfSectionDescriptorV1>(descBytes);

            string sectionType = Encoding.ASCII.GetString(descriptor.type_string).TrimEnd('\0');
            long   dataSize    = (long)descriptor.size - SECTION_DESCRIPTOR_V1_SIZE;

            AaruLogging.Debug(MODULE_NAME,
                              "Section type: {0} at offset {1}, next at {2}, size {3}",
                              sectionType,
                              sectionStart,
                              descriptor.next_offset,
                              descriptor.size);

            switch(sectionType)
            {
                case SECTION_TYPE_VOLUME:
                case SECTION_TYPE_DATA:
                    if(!volumeFound) ParseVolumeSection(segStream, dataSize, ref volumeFound);

                    break;

                case SECTION_TYPE_TABLE:
                case SECTION_TYPE_TABLE2:
                    ParseTableSectionV1(segStream, segIdx, dataSize, ref currentChunk, (long)descriptor.next_offset);

                    break;

                case SECTION_TYPE_LTREE:
                    if(ltreeDecompressed == null && dataSize > LTREE_HEADER_SIZE)
                        ltreeDecompressed = ParseLtreeSection(segStream, dataSize);

                    break;
            }

            if(descriptor.next_offset == 0 || sectionType is SECTION_TYPE_DONE or SECTION_TYPE_NEXT) break;

            segStream.Seek((long)descriptor.next_offset, SeekOrigin.Begin);
        }
    }

    void ParseVolumeSection(Stream segStream, long dataSize, ref bool volumeFound)
    {
        if(dataSize < VOLUME_SECTION_SIZE_SMART) return;

        var volumeData = new byte[dataSize];
        segStream.ReadExactly(volumeData, 0, (int)dataSize);

        if(dataSize < VOLUME_SECTION_SIZE_ENCASE)
        {
            EwfVolumeSmartSection smartVol =
                Marshal.ByteArrayToStructureLittleEndian<EwfVolumeSmartSection>(volumeData);

            _sectorsPerChunk = smartVol.sectors_per_chunk;
            _bytesPerSector  = smartVol.bytes_per_sector;
        }
        else
        {
            EwfVolumeSection vol = Marshal.ByteArrayToStructureLittleEndian<EwfVolumeSection>(volumeData);

            _sectorsPerChunk = vol.sectors_per_chunk;
            _bytesPerSector  = vol.bytes_per_sector;
        }

        volumeFound = true;
    }

    void ParseTableSectionV1(Stream segStream, int segIdx, long dataSize, ref ulong currentChunk,
                             long   nextSectionOffset)
    {
        if(dataSize < System.Runtime.InteropServices.Marshal.SizeOf<EwfTableHeaderV1>()) return;

        var tableHeaderBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfTableHeaderV1>()];
        segStream.ReadExactly(tableHeaderBytes, 0, tableHeaderBytes.Length);

        EwfTableHeaderV1 tableHeader = Marshal.ByteArrayToStructureLittleEndian<EwfTableHeaderV1>(tableHeaderBytes);

        var entryCount = (int)tableHeader.number_of_entries;
        var entryData  = new byte[entryCount * 4];
        segStream.ReadExactly(entryData, 0, entryData.Length);

        // Skip entries checksum
        segStream.Seek(4, SeekOrigin.Current);

        for(var i = 0; i < entryCount; i++)
        {
            var  rawEntry   = BitConverter.ToUInt32(entryData, i * 4);
            bool compressed = (rawEntry & TABLE_ENTRY_V1_COMPRESSED_FLAG) != 0;
            var  offset     = (long)(tableHeader.base_offset + (rawEntry & TABLE_ENTRY_V1_OFFSET_MASK));

            uint size;

            if(i + 1 < entryCount)
            {
                var nextRawEntry = BitConverter.ToUInt32(entryData, (i + 1) * 4);
                var nextOffset   = (long)(tableHeader.base_offset + (nextRawEntry & TABLE_ENTRY_V1_OFFSET_MASK));
                size = (uint)(nextOffset - offset);
            }
            else
            {
                if(nextSectionOffset > offset)
                    size = (uint)(nextSectionOffset - offset);
                else
                    size = _chunkSize + 4;
            }

            ulong chunkIndex = currentChunk + (ulong)i;

            if(!_chunkTable.ContainsKey(chunkIndex)) _chunkTable[chunkIndex] = (segIdx, offset, size, compressed);
        }

        currentChunk += (ulong)entryCount;
    }

    byte[] ParseLtreeSection(Stream segStream, long dataSize)
    {
        // Read ltree header (48 bytes)
        var headerBytes = new byte[LTREE_HEADER_SIZE];
        segStream.ReadExactly(headerBytes, 0, LTREE_HEADER_SIZE);

        EwfLtreeHeader ltreeHeader = Marshal.ByteArrayToStructureLittleEndian<EwfLtreeHeader>(headerBytes);

        AaruLogging.Debug(MODULE_NAME, "Ltree data size: {0}", ltreeHeader.data_size);

        // Read the compressed ltree data
        long compressedSize = dataSize - LTREE_HEADER_SIZE;

        if(compressedSize <= 0) return null;

        var compressedData = new byte[compressedSize];
        segStream.ReadExactly(compressedData, 0, (int)compressedSize);

        // Decompress
        try
        {
            return DecompressZlib(compressedData, (int)ltreeHeader.data_size);
        }
        catch(Exception ex)
        {
            AaruLogging.Debug(MODULE_NAME, "Failed to decompress ltree data: {0}", ex.Message);

            return null;
        }
    }

    void ParseSegmentV2(Stream     segStream, int segIdx, ref ulong currentChunk, ref bool volumeFound,
                        ref byte[] ltreeDecompressed)
    {
        segStream.Seek(0, SeekOrigin.Begin);

        var headerBytes = new byte[FILE_HEADER_V2_SIZE];
        segStream.ReadExactly(headerBytes, 0, FILE_HEADER_V2_SIZE);

        EwfFileHeaderV2 fileHeader = Marshal.ByteArrayToStructureLittleEndian<EwfFileHeaderV2>(headerBytes);
        _compressionMethod = (EwfCompressionMethod)fileHeader.compression_method;

        int  descriptorSize = System.Runtime.InteropServices.Marshal.SizeOf<EwfSectionDescriptorV2>();
        long position       = segStream.Length - descriptorSize;

        while(position >= FILE_HEADER_V2_SIZE)
        {
            segStream.Seek(position, SeekOrigin.Begin);

            var descBytes = new byte[descriptorSize];
            segStream.ReadExactly(descBytes, 0, descriptorSize);

            EwfSectionDescriptorV2 descriptor =
                Marshal.ByteArrayToStructureLittleEndian<EwfSectionDescriptorV2>(descBytes);

            var  sectionType = (EwfSectionTypeV2)descriptor.type;
            long dataStart   = position - (long)descriptor.data_size;

            switch(sectionType)
            {
                case EwfSectionTypeV2.SectorTable:
                    segStream.Seek(dataStart, SeekOrigin.Begin);
                    ParseTableSectionV2(segStream, segIdx, (long)descriptor.data_size, ref currentChunk);

                    break;

                case EwfSectionTypeV2.SingleFilesTree:
                    if(ltreeDecompressed == null && (long)descriptor.data_size > LTREE_HEADER_SIZE)
                    {
                        segStream.Seek(dataStart, SeekOrigin.Begin);
                        ltreeDecompressed = ParseLtreeSection(segStream, (long)descriptor.data_size);
                    }

                    break;
            }

            if(descriptor.previous_offset == 0) break;

            position = (long)descriptor.previous_offset;
        }
    }

    void ParseTableSectionV2(Stream segStream, int segIdx, long dataSize, ref ulong currentChunk)
    {
        if(dataSize < System.Runtime.InteropServices.Marshal.SizeOf<EwfTableHeaderV2>()) return;

        var tableHeaderBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfTableHeaderV2>()];
        segStream.ReadExactly(tableHeaderBytes, 0, tableHeaderBytes.Length);

        EwfTableHeaderV2 tableHeader = Marshal.ByteArrayToStructureLittleEndian<EwfTableHeaderV2>(tableHeaderBytes);

        ulong firstChunk = tableHeader.first_chunk_number;
        var   entryCount = (int)tableHeader.number_of_entries;
        int   entrySize  = System.Runtime.InteropServices.Marshal.SizeOf<EwfTableEntryV2>();

        for(var i = 0; i < entryCount; i++)
        {
            var entryBytes = new byte[entrySize];
            segStream.ReadExactly(entryBytes, 0, entrySize);

            EwfTableEntryV2 entry      = Marshal.ByteArrayToStructureLittleEndian<EwfTableEntryV2>(entryBytes);
            bool            compressed = (entry.chunk_data_flags & 0x00000001) == 0;
            ulong           chunkIndex = firstChunk + (ulong)i;

            if(!_chunkTable.ContainsKey(chunkIndex))
                _chunkTable[chunkIndex] = (segIdx, (long)entry.chunk_data_offset, entry.chunk_data_size, compressed);
        }

        if(firstChunk + (ulong)entryCount > currentChunk) currentChunk = firstChunk + (ulong)entryCount;
    }
}