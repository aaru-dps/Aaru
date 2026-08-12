// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Open.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Opens Expert Witness Format disk images.
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
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Images;

public sealed partial class Ewf
{
#region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < SIGNATURE_LENGTH) return ErrorNumber.InvalidArgument;

        // Read signature to determine version
        var signatureBytes = new byte[SIGNATURE_LENGTH];
        stream.EnsureRead(signatureBytes, 0, SIGNATURE_LENGTH);

        _isV2 = signatureBytes.AsSpan().SequenceEqual(EVF2_SIGNATURE) ||
                signatureBytes.AsSpan().SequenceEqual(LEF2_SIGNATURE);

        AaruLogging.Debug(MODULE_NAME, "EWF version {0}", _isV2 ? "2" : "1");

        // Discover all segment files
        _segmentStreams = [];
        List<IFilter> segmentFilters = DiscoverSegments(imageFilter);

        if(segmentFilters.Count == 0) return ErrorNumber.InvalidArgument;

        foreach(IFilter filter in segmentFilters) _segmentStreams.Add(filter.GetDataForkStream());

        // Initialize data structures
        _chunkTable   = new Dictionary<ulong, (int segmentIndex, long offset, uint size, bool compressed)>();
        _chunkCache   = new Dictionary<ulong, byte[]>();
        _sectorCache  = new Dictionary<ulong, byte[]>();
        _badSectors   = [];
        _headerValues = new Dictionary<string, string>();
        _md5Stored    = null;
        _sha1Stored   = null;
        _isSmart      = false;

        _sectorsPerChunk = DEFAULT_SECTORS_PER_CHUNK;
        _bytesPerSector  = DEFAULT_BYTES_PER_SECTOR;
        _chunkSize       = DEFAULT_CHUNK_SIZE;

        ulong totalSectors = 0;
        uint  chsC         = 0;
        uint  chsH         = 0;
        uint  chsS         = 0;
        byte  mediaType    = 0;
        byte  mediaFlags   = 0;
        var   volumeFound  = false;

        var   sessionEntries = new List<(ulong startSector, uint flags)>();
        ulong currentChunk   = 0;

        byte[] headerData  = null;
        byte[] header2Data = null;

        // Process each segment file
        for(var segIdx = 0; segIdx < _segmentStreams.Count; segIdx++)
        {
            Stream segStream = _segmentStreams[segIdx];

            if(_isV2)
            {
                ParseSegmentV2(segStream,
                               segIdx,
                               ref currentChunk,
                               ref volumeFound,
                               ref totalSectors,
                               ref mediaType,
                               ref mediaFlags,
                               ref chsC,
                               ref chsH,
                               ref chsS,
                               sessionEntries);
            }
            else
            {
                ParseSegmentV1(segStream,
                               segIdx,
                               ref currentChunk,
                               ref volumeFound,
                               ref totalSectors,
                               ref headerData,
                               ref header2Data,
                               ref mediaType,
                               ref mediaFlags,
                               ref chsC,
                               ref chsH,
                               ref chsS,
                               sessionEntries);
            }
        }

        if(!volumeFound)
        {
            AaruLogging.Error(MODULE_NAME, "No volume section found in any segment file");

            return ErrorNumber.InvalidArgument;
        }

        _chunkSize     = _sectorsPerChunk * _bytesPerSector;
        _maxChunkCache = (int)(MAX_CACHE_SIZE / _chunkSize);

        if(_maxChunkCache < 1) _maxChunkCache = 1;

        // Parse header metadata (prefer header2 over header)
        if(header2Data is { Length: > 0 })
            _headerValues                                    = ParseHeaderText(header2Data, true);
        else if(headerData is { Length: > 0 }) _headerValues = ParseHeaderText(headerData,  false);

        // Populate image info
        _imageInfo.Sectors              = totalSectors;
        _imageInfo.SectorSize           = _bytesPerSector;
        _imageInfo.Cylinders            = chsC;
        _imageInfo.Heads                = chsH;
        _imageInfo.SectorsPerTrack      = chsS;
        _imageInfo.ImageSize            = totalSectors * _bytesPerSector;
        _imageInfo.Application          = "EnCase";
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;

        if(_isSmart) _imageInfo.Application = "SMART";

        // Map header values to image info
        if(_headerValues.TryGetValue("a", out string description)) _imageInfo.Comments = description;

        if(_headerValues.TryGetValue("av", out string appVersion)) _imageInfo.ApplicationVersion = appVersion;

        if(_headerValues.TryGetValue("ov", out string osVersion)) _imageInfo.Creator = osVersion;

        if(_headerValues.TryGetValue("md", out string model)) _imageInfo.MediaModel = model;

        if(_headerValues.TryGetValue("sn", out string serial)) _imageInfo.MediaSerialNumber = serial;

        // Determine media type
        if(_isSmart)
        {
            _imageInfo.MediaType         = MediaType.GENERIC_HDD;
            _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;
        }
        else
        {
            var ewfMediaType = (EwfMediaType)mediaType;

            switch(ewfMediaType)
            {
                case EwfMediaType.Fixed:
                    _imageInfo.MediaType         = MediaType.GENERIC_HDD;
                    _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

                    break;
                case EwfMediaType.Removable:
                    _imageInfo.MediaType         = MediaType.FlashDrive;
                    _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

                    break;
                case EwfMediaType.Optical:
                    _imageInfo.MetadataMediaType = MetadataMediaType.OpticalDisc;

                    // Refine optical type based on sector size and count
                    if(_bytesPerSector == 2048)
                    {
                        _imageInfo.MediaType = totalSectors <= 360000
                                                   ? MediaType.CDROM
                                                   : totalSectors <= 2295104
                                                       ? MediaType.DVDROM
                                                       : MediaType.BDROM;
                    }
                    else
                        _imageInfo.MediaType = MediaType.CD;

                    break;
                case EwfMediaType.Memory:
                    _imageInfo.MediaType         = MediaType.GENERIC_HDD;
                    _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

                    break;
                default:
                    _imageInfo.MediaType         = MediaType.Unknown;
                    _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

                    break;
            }
        }

        // Build sessions and tracks
        if(_imageInfo.MetadataMediaType == MetadataMediaType.OpticalDisc)
        {
            _imageInfo.HasSessions   = true;
            _imageInfo.HasPartitions = true;

            BuildSessionsAndTracks(sessionEntries, totalSectors, _bytesPerSector, out _sessions, out _tracks);
        }
        else
        {
            _sessions = null;
            _tracks   = null;
        }

        // Build metadata from header values
        _metadata = null;

        if(_headerValues.Count > 0) BuildAaruMetadata();

        _imageInfo.Version = _isV2 ? "2" : "1";

        AaruLogging.Debug(MODULE_NAME, "Image has {0} sectors of {1} bytes", totalSectors, _bytesPerSector);
        AaruLogging.Debug(MODULE_NAME, "Chunk table has {0} entries",        _chunkTable.Count);

        return ErrorNumber.NoError;
    }

#endregion

    /// <summary>Discovers all segment files related to the primary image file.</summary>
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

    /// <summary>Parses an EWF v1 segment file, extracting all sections.</summary>
    void ParseSegmentV1(Stream segStream, int segIdx, ref ulong currentChunk, ref bool volumeFound,
                        ref ulong totalSectors, ref byte[] headerData, ref byte[] header2Data, ref byte mediaType,
                        ref byte mediaFlags, ref uint chsC, ref uint chsH, ref uint chsS,
                        List<(ulong startSector, uint flags)> sessionEntries)
    {
        // Skip file header (13 bytes)
        segStream.Seek(FILE_HEADER_V1_SIZE, SeekOrigin.Begin);

        while(segStream.Position < segStream.Length)
        {
            long sectionStart = segStream.Position;

            // Read section descriptor
            var descBytes = new byte[SECTION_DESCRIPTOR_V1_SIZE];
            int bytesRead = segStream.EnsureRead(descBytes, 0, SECTION_DESCRIPTOR_V1_SIZE);

            if(bytesRead < SECTION_DESCRIPTOR_V1_SIZE) break;

            EwfSectionDescriptorV1 descriptor =
                Marshal.ByteArrayToStructureLittleEndian<EwfSectionDescriptorV1>(descBytes);

            // Get section type string (null-terminated)
            string sectionType = Encoding.ASCII.GetString(descriptor.type_string).TrimEnd('\0');

            AaruLogging.Debug(MODULE_NAME,
                              "Section type: {0} at offset {1}, next at {2}, size {3}",
                              sectionType,
                              sectionStart,
                              descriptor.next_offset,
                              descriptor.size);

            // Calculate data size (section size minus section descriptor size)
            long dataSize = (long)descriptor.size - SECTION_DESCRIPTOR_V1_SIZE;

            switch(sectionType)
            {
                case SECTION_TYPE_HEADER:
                    if(headerData == null && dataSize > 0)
                    {
                        var compressedHeader = new byte[dataSize];
                        segStream.EnsureRead(compressedHeader, 0, (int)dataSize);

                        try
                        {
                            headerData = DecompressZlib(compressedHeader, (int)dataSize * 10);
                        }
                        catch(Exception ex)
                        {
                            AaruLogging.Debug(MODULE_NAME, "Failed to decompress header: {0}", ex.Message);
                        }
                    }

                    break;

                case SECTION_TYPE_HEADER2:
                    if(header2Data == null && dataSize > 0)
                    {
                        var compressedHeader2 = new byte[dataSize];
                        segStream.EnsureRead(compressedHeader2, 0, (int)dataSize);

                        try
                        {
                            header2Data = DecompressZlib(compressedHeader2, (int)dataSize * 10);
                        }
                        catch(Exception ex)
                        {
                            AaruLogging.Debug(MODULE_NAME, "Failed to decompress header2: {0}", ex.Message);
                        }
                    }

                    break;

                case SECTION_TYPE_VOLUME:
                case SECTION_TYPE_DATA:
                    if(!volumeFound)
                    {
                        ParseVolumeSection(segStream,
                                           dataSize,
                                           ref volumeFound,
                                           ref totalSectors,
                                           ref mediaType,
                                           ref mediaFlags,
                                           ref chsC,
                                           ref chsH,
                                           ref chsS);
                    }

                    break;

                case SECTION_TYPE_TABLE:
                case SECTION_TYPE_TABLE2:
                    // Chunk data ends where the table section itself begins
                    ParseTableSectionV1(segStream, segIdx, dataSize, ref currentChunk, sectionStart);

                    break;

                case SECTION_TYPE_HASH:
                    if(_md5Stored == null &&
                       dataSize   >= System.Runtime.InteropServices.Marshal.SizeOf<EwfHashSection>())
                    {
                        var hashBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfHashSection>()];
                        segStream.EnsureRead(hashBytes, 0, hashBytes.Length);

                        EwfHashSection hash = Marshal.ByteArrayToStructureLittleEndian<EwfHashSection>(hashBytes);

                        _md5Stored = hash.md5_hash;

                        AaruLogging.Debug(MODULE_NAME,
                                          "MD5 from hash section: {0}",
                                          BitConverter.ToString(_md5Stored).Replace("-", "").ToLowerInvariant());
                    }

                    break;

                case SECTION_TYPE_DIGEST:
                    if(dataSize >= System.Runtime.InteropServices.Marshal.SizeOf<EwfDigestSection>())
                    {
                        var digestBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfDigestSection>()];
                        segStream.EnsureRead(digestBytes, 0, digestBytes.Length);

                        EwfDigestSection digest =
                            Marshal.ByteArrayToStructureLittleEndian<EwfDigestSection>(digestBytes);

                        _md5Stored  = digest.md5_hash;
                        _sha1Stored = digest.sha1_hash;

                        AaruLogging.Debug(MODULE_NAME,
                                          "MD5 from digest section: {0}",
                                          BitConverter.ToString(_md5Stored).Replace("-", "").ToLowerInvariant());

                        AaruLogging.Debug(MODULE_NAME,
                                          "SHA1 from digest section: {0}",
                                          BitConverter.ToString(_sha1Stored).Replace("-", "").ToLowerInvariant());
                    }

                    break;

                case SECTION_TYPE_ERROR2:
                    ParseErrorSectionV1(segStream, dataSize);

                    break;

                case SECTION_TYPE_SESSION:
                    ParseSessionSectionV1(segStream, dataSize, sessionEntries);

                    break;
            }

            // Move to next section
            if(descriptor.next_offset == 0 || sectionType is SECTION_TYPE_DONE or SECTION_TYPE_NEXT) break;

            // A section pointing at or before itself would loop forever
            if((long)descriptor.next_offset <= sectionStart) break;

            segStream.Seek((long)descriptor.next_offset, SeekOrigin.Begin);
        }
    }

    /// <summary>Parses the volume (or data) section to extract media parameters.</summary>
    void ParseVolumeSection(Stream   segStream, long     dataSize,   ref bool volumeFound, ref ulong totalSectors,
                            ref byte mediaType, ref byte mediaFlags, ref uint chsC,        ref uint chsH, ref uint chsS)
    {
        if(dataSize < VOLUME_SECTION_SIZE_SMART) return;

        // Read all the volume data available
        var volumeData = new byte[dataSize];
        segStream.EnsureRead(volumeData, 0, (int)dataSize);

        // Determine format by checking if it's a small (94-byte) or large (1052-byte) volume section
        if(dataSize < VOLUME_SECTION_SIZE_ENCASE)
        {
            // Small volume section: SMART or old EnCase format (94 bytes of data)
            EwfVolumeSmartSection smartVol =
                Marshal.ByteArrayToStructureLittleEndian<EwfVolumeSmartSection>(volumeData);

            _sectorsPerChunk = smartVol.sectors_per_chunk;
            _bytesPerSector  = smartVol.bytes_per_sector;
            totalSectors     = smartVol.number_of_sectors;

            // Check SMART signature
            if(smartVol.signature != null && smartVol.signature.AsSpan().SequenceEqual(SMART_SIGNATURE))
                _isSmart = true;

            AaruLogging.Debug(MODULE_NAME,
                              "{0} volume: {1} sectors, {2} bytes/sector, {3} sectors/chunk",
                              _isSmart ? "SMART" : "Old EnCase",
                              totalSectors,
                              _bytesPerSector,
                              _sectorsPerChunk);
        }
        else
        {
            // Large volume section: EnCase 5+ format (1052 bytes of data)
            EwfVolumeSection vol = Marshal.ByteArrayToStructureLittleEndian<EwfVolumeSection>(volumeData);

            _sectorsPerChunk = vol.sectors_per_chunk;
            _bytesPerSector  = vol.bytes_per_sector;
            totalSectors     = vol.number_of_sectors;
            chsC             = vol.chs_cylinders;
            chsH             = vol.chs_heads;
            chsS             = vol.chs_sectors;
            mediaType        = vol.media_type;
            mediaFlags       = vol.media_flags;

            AaruLogging.Debug(MODULE_NAME,
                              "EnCase volume: {0} sectors, {1} bytes/sector, {2} sectors/chunk, C/H/S={3}/{4}/{5}",
                              totalSectors,
                              _bytesPerSector,
                              _sectorsPerChunk,
                              chsC,
                              chsH,
                              chsS);

            AaruLogging.Debug(MODULE_NAME,
                              "Media type: {0}, media flags: {1}, compression: {2}",
                              (EwfMediaType)vol.media_type,
                              (EwfMediaFlags)vol.media_flags,
                              (EwfCompressionLevel)vol.compression_level);
        }

        // Table parsing needs the real chunk size before all segments are done
        if(_sectorsPerChunk > 0 && _bytesPerSector > 0) _chunkSize = _sectorsPerChunk * _bytesPerSector;

        volumeFound = true;
    }

    /// <summary>Parses an EWF v1 table section, building the chunk→location mapping.</summary>
    void ParseTableSectionV1(Stream segStream, int segIdx, long dataSize, ref ulong currentChunk,
                             long   nextSectionOffset)
    {
        if(dataSize < System.Runtime.InteropServices.Marshal.SizeOf<EwfTableHeaderV1>()) return;

        // Read table header
        var tableHeaderBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfTableHeaderV1>()];
        segStream.EnsureRead(tableHeaderBytes, 0, tableHeaderBytes.Length);

        EwfTableHeaderV1 tableHeader = Marshal.ByteArrayToStructureLittleEndian<EwfTableHeaderV1>(tableHeaderBytes);

        AaruLogging.Debug(MODULE_NAME,
                          "Table: {0} entries, base offset {1}",
                          tableHeader.number_of_entries,
                          tableHeader.base_offset);

        // Read all table entries, never more than the stream can hold
        var entryCount = (int)tableHeader.number_of_entries;

        if(entryCount < 0 || (long)entryCount * 4 > segStream.Length - segStream.Position) return;

        var entryData = new byte[entryCount * 4];
        segStream.EnsureRead(entryData, 0, entryData.Length);

        // Skip entries checksum (4 bytes)
        segStream.Seek(4, SeekOrigin.Current);

        for(var i = 0; i < entryCount; i++)
        {
            var  rawEntry   = BitConverter.ToUInt32(entryData, i * 4);
            bool compressed = (rawEntry & TABLE_ENTRY_V1_COMPRESSED_FLAG) != 0;
            var  offset     = (long)(tableHeader.base_offset + (rawEntry & TABLE_ENTRY_V1_OFFSET_MASK));

            // Calculate chunk size: distance to next chunk or next section for the last entry
            uint size;

            if(i + 1 < entryCount)
            {
                var nextRawEntry = BitConverter.ToUInt32(entryData, (i + 1) * 4);
                var nextOffset   = (long)(tableHeader.base_offset + (nextRawEntry & TABLE_ENTRY_V1_OFFSET_MASK));
                size = (uint)(nextOffset - offset);
            }
            else
            {
                // For the last entry, use the next section offset to compute size
                if(nextSectionOffset > offset)
                    size = (uint)(nextSectionOffset - offset);
                else
                    size = _chunkSize + 4; // fallback: uncompressed chunk + Adler-32
            }

            ulong chunkIndex = currentChunk + (ulong)i;

            if(!_chunkTable.ContainsKey(chunkIndex)) _chunkTable[chunkIndex] = (segIdx, offset, size, compressed);
        }

        currentChunk += (ulong)entryCount;
    }

    /// <summary>Parses an EWF v1 error2 section.</summary>
    void ParseErrorSectionV1(Stream segStream, long dataSize)
    {
        if(dataSize < System.Runtime.InteropServices.Marshal.SizeOf<EwfErrorHeaderV1>()) return;

        var headerBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfErrorHeaderV1>()];
        segStream.EnsureRead(headerBytes, 0, headerBytes.Length);

        EwfErrorHeaderV1 errorHeader = Marshal.ByteArrayToStructureLittleEndian<EwfErrorHeaderV1>(headerBytes);

        int entrySize = System.Runtime.InteropServices.Marshal.SizeOf<EwfErrorEntryV1>();

        for(var i = 0; i < (int)errorHeader.number_of_entries; i++)
        {
            var entryBytes = new byte[entrySize];
            segStream.EnsureRead(entryBytes, 0, entrySize);

            EwfErrorEntryV1 entry = Marshal.ByteArrayToStructureLittleEndian<EwfErrorEntryV1>(entryBytes);

            _badSectors.Add((entry.start_sector, entry.number_of_sectors));

            AaruLogging.Debug(MODULE_NAME,
                              "Bad sector range: start={0}, count={1}",
                              entry.start_sector,
                              entry.number_of_sectors);
        }
    }

    /// <summary>Parses an EWF v1 session section.</summary>
    void ParseSessionSectionV1(Stream segStream, long dataSize, List<(ulong startSector, uint flags)> sessionEntries)
    {
        if(dataSize < System.Runtime.InteropServices.Marshal.SizeOf<EwfSessionHeaderV1>()) return;

        var headerBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfSessionHeaderV1>()];
        segStream.EnsureRead(headerBytes, 0, headerBytes.Length);

        EwfSessionHeaderV1 sessionHeader = Marshal.ByteArrayToStructureLittleEndian<EwfSessionHeaderV1>(headerBytes);

        AaruLogging.Debug(MODULE_NAME, "Session section: {0} entries", sessionHeader.number_of_entries);

        int entrySize = System.Runtime.InteropServices.Marshal.SizeOf<EwfSessionEntryV1>();

        for(var i = 0; i < (int)sessionHeader.number_of_entries; i++)
        {
            var entryBytes = new byte[entrySize];
            segStream.EnsureRead(entryBytes, 0, entrySize);

            EwfSessionEntryV1 entry = Marshal.ByteArrayToStructureLittleEndian<EwfSessionEntryV1>(entryBytes);

            sessionEntries.Add((entry.start_sector, entry.flags));

            AaruLogging.Debug(MODULE_NAME, "Session entry: start={0}, flags=0x{1:X8}", entry.start_sector, entry.flags);
        }
    }

    /// <summary>Parses an EWF v2 segment file.</summary>
    void ParseSegmentV2(Stream    segStream,    int segIdx, ref ulong currentChunk, ref bool volumeFound,
                        ref ulong totalSectors, ref byte mediaType, ref byte mediaFlags, ref uint chsC, ref uint chsH,
                        ref uint  chsS,         List<(ulong startSector, uint flags)> sessionEntries)
    {
        // Read v2 file header
        segStream.Seek(0, SeekOrigin.Begin);

        var headerBytes = new byte[FILE_HEADER_V2_SIZE];
        segStream.EnsureRead(headerBytes, 0, FILE_HEADER_V2_SIZE);

        EwfFileHeaderV2 fileHeader = Marshal.ByteArrayToStructureLittleEndian<EwfFileHeaderV2>(headerBytes);

        _compressionMethod = (EwfCompressionMethod)fileHeader.compression_method;

        AaruLogging.Debug(MODULE_NAME,
                          "EWF v2 segment {0}, compression method: {1}",
                          fileHeader.segment_number,
                          _compressionMethod);

        // EWF v2 sections are read backward from the end of the file
        int  descriptorSize = System.Runtime.InteropServices.Marshal.SizeOf<EwfSectionDescriptorV2>();
        long position       = segStream.Length - descriptorSize;

        while(position >= FILE_HEADER_V2_SIZE)
        {
            segStream.Seek(position, SeekOrigin.Begin);

            var descBytes = new byte[descriptorSize];
            segStream.EnsureRead(descBytes, 0, descriptorSize);

            EwfSectionDescriptorV2 descriptor =
                Marshal.ByteArrayToStructureLittleEndian<EwfSectionDescriptorV2>(descBytes);

            var  sectionType = (EwfSectionTypeV2)descriptor.type;
            long dataStart   = position - (long)descriptor.data_size;

            AaruLogging.Debug(MODULE_NAME,
                              "V2 section type: {0} at {1}, data size {2}, previous at {3}",
                              sectionType,
                              position,
                              descriptor.data_size,
                              descriptor.previous_offset);

            switch(sectionType)
            {
                case EwfSectionTypeV2.SectorTable:
                    segStream.Seek(dataStart, SeekOrigin.Begin);
                    ParseTableSectionV2(segStream, segIdx, (long)descriptor.data_size, ref currentChunk);

                    break;

                case EwfSectionTypeV2.DeviceInformation:
                case EwfSectionTypeV2.CaseData:
                {
                    if(dataStart < 0 || descriptor.data_size == 0) break;

                    segStream.Seek(dataStart, SeekOrigin.Begin);
                    var textBytes = new byte[descriptor.data_size];
                    segStream.EnsureRead(textBytes, 0, textBytes.Length);
                    ParseTextSectionV2(textBytes, ref volumeFound, ref totalSectors);

                    break;
                }

                case EwfSectionTypeV2.Md5Hash:
                    if(_md5Stored                 == null &&
                       (long)descriptor.data_size >= System.Runtime.InteropServices.Marshal.SizeOf<EwfMd5HashV2>())
                    {
                        segStream.Seek(dataStart, SeekOrigin.Begin);

                        var hashBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfMd5HashV2>()];
                        segStream.EnsureRead(hashBytes, 0, hashBytes.Length);

                        EwfMd5HashV2 hash = Marshal.ByteArrayToStructureLittleEndian<EwfMd5HashV2>(hashBytes);

                        _md5Stored = hash.md5_hash;
                    }

                    break;

                case EwfSectionTypeV2.Sha1Hash:
                    if(_sha1Stored                == null &&
                       (long)descriptor.data_size >= System.Runtime.InteropServices.Marshal.SizeOf<EwfSha1HashV2>())
                    {
                        segStream.Seek(dataStart, SeekOrigin.Begin);

                        var hashBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfSha1HashV2>()];
                        segStream.EnsureRead(hashBytes, 0, hashBytes.Length);

                        EwfSha1HashV2 hash = Marshal.ByteArrayToStructureLittleEndian<EwfSha1HashV2>(hashBytes);

                        _sha1Stored = hash.sha1_hash;
                    }

                    break;

                case EwfSectionTypeV2.ErrorTable:
                    segStream.Seek(dataStart, SeekOrigin.Begin);
                    ParseErrorSectionV2(segStream, (long)descriptor.data_size);

                    break;

                case EwfSectionTypeV2.SessionTable:
                    segStream.Seek(dataStart, SeekOrigin.Begin);
                    ParseSessionSectionV2(segStream, (long)descriptor.data_size, sessionEntries);

                    break;
            }

            // Move to previous section, which must be strictly before this one or we would loop forever
            if(descriptor.previous_offset == 0 || (long)descriptor.previous_offset >= position) break;

            position = (long)descriptor.previous_offset;
        }
    }

    /// <summary>
    ///     Parses an EWF v2 device information or case data section: a zlib-compressed UTF-16 table of
    ///     tab-separated keys and values. Extracts the media geometry the volume section carried in v1.
    /// </summary>
    void ParseTextSectionV2(byte[] data, ref bool volumeFound, ref ulong totalSectors)
    {
        byte[] raw = data;

        try
        {
            using var zms = new MemoryStream(data);
            using var z = new System.IO.Compression.ZLibStream(zms, System.IO.Compression.CompressionMode.Decompress);
            using var outMs = new MemoryStream();
            z.CopyTo(outMs);
            raw = outMs.ToArray();
        }
        catch(Exception)
        {
            // Not compressed, use as-is
        }

        string text = raw.Length >= 2 && raw[0] == 0xFF && raw[1] == 0xFE
                          ? Encoding.Unicode.GetString(raw, 2, raw.Length - 2)
                          : Encoding.Unicode.GetString(raw);

        string[] lines = text.Split('\n');

        for(var i = 0; i + 1 < lines.Length; i++)
        {
            string[] keys   = lines[i].TrimEnd('\r', '\0').Split('\t');
            string[] values = lines[i + 1].TrimEnd('\r', '\0').Split('\t');

            if(keys.Length < 2 || keys.Length != values.Length) continue;

            for(var k = 0; k < keys.Length; k++)
            {
                switch(keys[k])
                {
                    // Bytes per sector
                    case "bps":
                        if(uint.TryParse(values[k], out uint bps) && bps > 0) _bytesPerSector = bps;

                        break;

                    // Sectors per chunk (case data)
                    case "sb":
                        if(uint.TryParse(values[k], out uint spc) && spc > 0) _sectorsPerChunk = spc;

                        break;

                    // Total sectors (device information)
                    case "ts":
                        if(ulong.TryParse(values[k], out ulong ts) && ts > 0)
                        {
                            totalSectors = ts;
                            volumeFound  = true;
                        }

                        break;
                }
            }
        }
    }

    /// <summary>Parses an EWF v2 table section.</summary>
    void ParseTableSectionV2(Stream segStream, int segIdx, long dataSize, ref ulong currentChunk)
    {
        if(dataSize < System.Runtime.InteropServices.Marshal.SizeOf<EwfTableHeaderV2>()) return;

        var tableHeaderBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfTableHeaderV2>()];
        segStream.EnsureRead(tableHeaderBytes, 0, tableHeaderBytes.Length);

        EwfTableHeaderV2 tableHeader = Marshal.ByteArrayToStructureLittleEndian<EwfTableHeaderV2>(tableHeaderBytes);
        ulong            firstChunk  = tableHeader.first_chunk_number;
        var              entryCount  = (int)tableHeader.number_of_entries;
        int              entrySize   = System.Runtime.InteropServices.Marshal.SizeOf<EwfTableEntryV2>();

        AaruLogging.Debug(MODULE_NAME, "V2 Table: {0} entries, first chunk {1}", entryCount, firstChunk);

        for(var i = 0; i < entryCount; i++)
        {
            var entryBytes = new byte[entrySize];
            segStream.EnsureRead(entryBytes, 0, entrySize);

            EwfTableEntryV2 entry = Marshal.ByteArrayToStructureLittleEndian<EwfTableEntryV2>(entryBytes);

            // Flag 0x00000001 means compressed in v2
            bool compressed = (entry.chunk_data_flags & 0x00000001) != 0;

            ulong chunkIndex = firstChunk + (ulong)i;

            if(!_chunkTable.ContainsKey(chunkIndex))
                _chunkTable[chunkIndex] = (segIdx, (long)entry.chunk_data_offset, entry.chunk_data_size, compressed);
        }

        if(firstChunk + (ulong)entryCount > currentChunk) currentChunk = firstChunk + (ulong)entryCount;
    }

    /// <summary>Parses an EWF v2 error section.</summary>
    void ParseErrorSectionV2(Stream segStream, long dataSize)
    {
        if(dataSize < System.Runtime.InteropServices.Marshal.SizeOf<EwfErrorHeaderV2>()) return;

        var headerBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfErrorHeaderV2>()];
        segStream.EnsureRead(headerBytes, 0, headerBytes.Length);

        EwfErrorHeaderV2 errorHeader = Marshal.ByteArrayToStructureLittleEndian<EwfErrorHeaderV2>(headerBytes);
        int              entrySize   = System.Runtime.InteropServices.Marshal.SizeOf<EwfErrorEntryV2>();

        for(var i = 0; i < (int)errorHeader.number_of_entries; i++)
        {
            var entryBytes = new byte[entrySize];
            segStream.EnsureRead(entryBytes, 0, entrySize);

            EwfErrorEntryV2 entry = Marshal.ByteArrayToStructureLittleEndian<EwfErrorEntryV2>(entryBytes);

            _badSectors.Add((entry.start_sector, entry.number_of_sectors));
        }
    }

    /// <summary>Parses an EWF v2 session section.</summary>
    void ParseSessionSectionV2(Stream segStream, long dataSize, List<(ulong startSector, uint flags)> sessionEntries)
    {
        if(dataSize < System.Runtime.InteropServices.Marshal.SizeOf<EwfSessionHeaderV2>()) return;

        var headerBytes = new byte[System.Runtime.InteropServices.Marshal.SizeOf<EwfSessionHeaderV2>()];
        segStream.EnsureRead(headerBytes, 0, headerBytes.Length);

        EwfSessionHeaderV2 sessionHeader = Marshal.ByteArrayToStructureLittleEndian<EwfSessionHeaderV2>(headerBytes);
        int                entrySize     = System.Runtime.InteropServices.Marshal.SizeOf<EwfSessionEntryV2>();

        for(var i = 0; i < (int)sessionHeader.number_of_entries; i++)
        {
            var entryBytes = new byte[entrySize];
            segStream.EnsureRead(entryBytes, 0, entrySize);

            EwfSessionEntryV2 entry = Marshal.ByteArrayToStructureLittleEndian<EwfSessionEntryV2>(entryBytes);

            sessionEntries.Add((entry.start_sector, entry.flags));
        }
    }

    /// <summary>Builds AaruMetadata from parsed header values.</summary>
    void BuildAaruMetadata()
    {
        // EWF header metadata doesn't map cleanly to AaruMetadata, but we can store some info
        // in the image's Comments and Creator fields (already done above).
        // For full metadata support, the AaruMetadata object would need forensic-specific fields.
        // For now, we leave _metadata as null and expose data through ImageInfo fields.
    }
}