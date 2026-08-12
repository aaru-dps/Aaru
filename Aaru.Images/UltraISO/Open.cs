// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Open.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Opens UltraISO disc images.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Aaru.Logging;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class UltraISO
{
#region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < ISZ_HEADER_BASE_SIZE) return ErrorNumber.InvalidArgument;

        // Read the full extended header size (64 bytes) if possible, otherwise base (48)
        int headerReadSize = stream.Length >= ISZ_HEADER_EXTENDED_SIZE
                                 ? ISZ_HEADER_EXTENDED_SIZE
                                 : ISZ_HEADER_BASE_SIZE;

        var headerBytes = new byte[ISZ_HEADER_EXTENDED_SIZE];
        stream.EnsureRead(headerBytes, 0, headerReadSize);

        _header = Marshal.ByteArrayToStructureLittleEndian<IszHeader>(headerBytes);

        // Validate signature
        if(_header.signature != ISZ_SIGNATURE) return ErrorNumber.InvalidArgument;

        // Validate version
        if(_header.version > 1)
        {
            AaruLogging.Error("Unsupported ISZ version: {0}", _header.version);

            return ErrorNumber.NotSupported;
        }

        // Check encryption
        if(_header.encryptionType != IszEncryption.None)
        {
            AaruLogging.Error("Encrypted ISZ images are not supported");

            return ErrorNumber.NotSupported;
        }

        AaruLogging.Debug(MODULE_NAME, "header.headerSize = {0}",             _header.headerSize);
        AaruLogging.Debug(MODULE_NAME, "header.version = {0}",                _header.version);
        AaruLogging.Debug(MODULE_NAME, "header.volumeSerialNumber = {0}",     _header.volumeSerialNumber);
        AaruLogging.Debug(MODULE_NAME, "header.sectorSize = {0}",             _header.sectorSize);
        AaruLogging.Debug(MODULE_NAME, "header.totalSectors = {0}",           _header.totalSectors);
        AaruLogging.Debug(MODULE_NAME, "header.encryptionType = {0}",         _header.encryptionType);
        AaruLogging.Debug(MODULE_NAME, "header.segmentSize = {0}",            _header.segmentSize);
        AaruLogging.Debug(MODULE_NAME, "header.numChunks = {0}",              _header.numChunks);
        AaruLogging.Debug(MODULE_NAME, "header.chunkSize = {0}",              _header.chunkSize);
        AaruLogging.Debug(MODULE_NAME, "header.pointerLength = {0}",          _header.pointerLength);
        AaruLogging.Debug(MODULE_NAME, "header.segmentNumber = {0}",          _header.segmentNumber);
        AaruLogging.Debug(MODULE_NAME, "header.chunkTableOffset = 0x{0:X}",   _header.chunkTableOffset);
        AaruLogging.Debug(MODULE_NAME, "header.segmentTableOffset = 0x{0:X}", _header.segmentTableOffset);
        AaruLogging.Debug(MODULE_NAME, "header.dataOffset = 0x{0:X}",         _header.dataOffset);

        if(_header.headerSize > ISZ_HEADER_BASE_SIZE)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "header.checksum1 = 0x{0:X8} (actual: 0x{1:X8})",
                              _header.checksum1,
                              ~_header.checksum1);

            AaruLogging.Debug(MODULE_NAME, "header.dataSize = {0}", _header.dataSize);
            AaruLogging.Debug(MODULE_NAME, "header.unknown = {0}",  _header.unknown);

            AaruLogging.Debug(MODULE_NAME,
                              "header.checksum2 = 0x{0:X8} (actual: 0x{1:X8})",
                              _header.checksum2,
                              ~_header.checksum2);
        }

        // Read or create segment table
        if(_header.segmentTableOffset != 0)
        {
            if(!ReadSegmentTable(stream)) return ErrorNumber.InvalidArgument;
        }
        else
            CreateSingleSegment();

        // Open segment streams
        ErrorNumber partError = OpenSegmentStreams(imageFilter, stream);

        if(partError != ErrorNumber.NoError) return partError;

        // Read chunk table / index
        if(!ReadChunkIndex(stream)) return ErrorNumber.InvalidArgument;

        // Calculate image properties
        ulong totalSectors = _header.totalSectors;

        // Set up image info
        _imageInfo.Sectors              = totalSectors;
        _imageInfo.SectorSize           = _header.sectorSize;
        _imageInfo.ImageSize            = (ulong)_header.totalSectors * _header.sectorSize;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;
        _imageInfo.MediaType            = CalculateMediaType(totalSectors);
        _imageInfo.Application          = "UltraISO";

        // Build tracks, sessions, and partitions for a single data track
        Tracks =
        [
            new Track
            {
                BytesPerSector    = _header.sectorSize,
                RawBytesPerSector = _header.sectorSize,
                Sequence          = 1,
                Session           = 1,
                StartSector       = 0,
                EndSector         = totalSectors - 1,
                Type              = TrackType.CdMode1,
                SubchannelType    = TrackSubchannelType.None,
                Description       = string.Format(Localization.Track_0, 1),
                File              = imageFilter.Filename,
                FileType          = "BINARY",
                Filter            = imageFilter,
                Indexes = new Dictionary<ushort, int>
                {
                    [0] = -150,
                    [1] = 0
                }
            }
        ];

        Sessions =
        [
            new Session
            {
                Sequence    = 1,
                StartTrack  = 1,
                EndTrack    = 1,
                StartSector = 0,
                EndSector   = totalSectors - 1
            }
        ];

        Partitions =
        [
            new Partition
            {
                Sequence    = 0,
                Start       = 0,
                Length      = totalSectors,
                Offset      = 0,
                Size        = totalSectors * _header.sectorSize,
                Description = string.Format(Localization.Track_0, 1),
                Type        = "MODE1/2048"
            }
        ];

        // Store the filter and initialize caches
        _imageFilter           = imageFilter;
        _chunkCache            = new Dictionary<int, byte[]>();
        _currentChunkCacheSize = 0;
        _sectorCache           = new Dictionary<ulong, byte[]>();
        _inflateBuffer         = new byte[_header.chunkSize];
        _ioBuffer              = new byte[_header.chunkSize];
        _sectorBuilder         = new SectorBuilder();

        return ErrorNumber.NoError;
    }

#endregion

    /// <summary>Deobfuscates ISZ data by XORing with the bitwise NOT of the ISZ signature</summary>
    static void Deobfuscate(byte[] data, int length)
    {
        byte[] mask = [0xB6, 0x8C, 0xA5, 0xDE]; // ~'I', ~'s', ~'Z', ~'!'

        for(var i = 0; i < length; i++) data[i] ^= mask[i % 4];
    }

    /// <summary>Generates a segment filename from the main .isz filename</summary>
    /// <param name="mainFilename">Path to the main .isz file</param>
    /// <param name="segmentIndex">1-based segment index</param>
    /// <returns>Path to the segment file</returns>
    static string GetSegmentFilename(string mainFilename, int segmentIndex)
    {
        // Replace last two characters of filename with segment index (e.g., .isz → .i01, .i02)
        string withoutLastTwo = mainFilename[..^2];

        return withoutLastTwo + segmentIndex.ToString("D2");
    }

    /// <summary>Determines media type based on total sector count</summary>
    static MediaType CalculateMediaType(ulong totalSectors) => totalSectors switch
                                                               {
                                                                   <= 360000   => MediaType.CD,
                                                                   <= 2295104  => MediaType.DVDPR,
                                                                   <= 2298496  => MediaType.DVDR,
                                                                   <= 4171712  => MediaType.DVDRDL,
                                                                   <= 4173824  => MediaType.DVDPRDL,
                                                                   <= 24438784 => MediaType.BDR,
                                                                   <= 62500864 => MediaType.BDRXL,
                                                                   _           => MediaType.Unknown
                                                               };

    /// <summary>Decodes a chunk pointer entry into type and length</summary>
    /// <param name="chunkPtr">Raw chunk pointer bytes (little-endian)</param>
    /// <param name="ptrLen">Number of bytes in the pointer</param>
    /// <param name="type">Decoded chunk type</param>
    /// <param name="length">Decoded compressed length</param>
    static void DecodeChunkPointer(byte[] chunkPtr, byte ptrLen, out IszChunkType type, out uint length)
    {
        uint value = 0;

        for(var b = 0; b < ptrLen; b++) value |= (uint)chunkPtr[b] << b * 8;

        var  typeBits = 2;
        int  lenBits  = ptrLen * 8       - typeBits;
        uint lenMask  = (1u << lenBits)  - 1;
        uint typeMask = (1u << typeBits) - 1;

        length = value & lenMask;
        type   = (IszChunkType)(value >> lenBits & typeMask);
    }

    /// <summary>Reads the segment table from the stream</summary>
    bool ReadSegmentTable(Stream stream)
    {
        stream.Seek(_header.segmentTableOffset, SeekOrigin.Begin);

        // First pass: count segments (terminated by entry with size == 0)
        var segmentCount = 0;
        var entryBytes   = new byte[ISZ_SEGMENT_SIZE];

        for(;;)
        {
            int read = stream.Read(entryBytes, 0, ISZ_SEGMENT_SIZE);

            if(read < ISZ_SEGMENT_SIZE)
            {
                AaruLogging.Error("Failed to read segment table entry");

                return false;
            }

            Deobfuscate(entryBytes, ISZ_SEGMENT_SIZE);

            IszSegment seg = Marshal.ByteArrayToStructureLittleEndian<IszSegment>(entryBytes);

            if(seg.size == 0) break;

            segmentCount++;
        }

        if(segmentCount == 0)
        {
            AaruLogging.Error("No segments found in segment table");

            return false;
        }

        // Second pass: read segments
        _segmentTable = new IszSegment[segmentCount];
        stream.Seek(_header.segmentTableOffset, SeekOrigin.Begin);

        for(var s = 0; s < segmentCount; s++)
        {
            stream.EnsureRead(entryBytes, 0, ISZ_SEGMENT_SIZE);
            Deobfuscate(entryBytes, ISZ_SEGMENT_SIZE);

            _segmentTable[s] = Marshal.ByteArrayToStructureLittleEndian<IszSegment>(entryBytes);

            AaruLogging.Debug(MODULE_NAME,
                              "Segment {0}: size={1}, numChunks={2}, firstChunk={3}, chunkOffset={4}, leftSize={5}",
                              s,
                              _segmentTable[s].size,
                              _segmentTable[s].numChunks,
                              _segmentTable[s].firstChunkNumber,
                              _segmentTable[s].chunkOffset,
                              _segmentTable[s].leftSize);
        }

        AaruLogging.Debug(MODULE_NAME, "Read {0} segments", segmentCount);

        return true;
    }

    /// <summary>Creates a single synthetic segment for non-segmented images</summary>
    void CreateSingleSegment()
    {
        _segmentTable =
        [
            new IszSegment
            {
                size             = (ulong)_header.totalSectors * _header.sectorSize,
                numChunks        = _header.numChunks,
                firstChunkNumber = 0,
                chunkOffset      = _header.dataOffset,
                leftSize         = 0
            }
        ];

        AaruLogging.Debug(MODULE_NAME,
                          "Created single segment: size={0}, numChunks={1}, chunkOffset={2}",
                          _segmentTable[0].size,
                          _segmentTable[0].numChunks,
                          _segmentTable[0].chunkOffset);
    }

    /// <summary>Opens streams for all segment files</summary>
    ErrorNumber OpenSegmentStreams(IFilter imageFilter, Stream mainStream)
    {
        _partTable   = new IszPart[_segmentTable.Length];
        _partStreams = [mainStream];

        // First segment uses the main stream
        _partTable[0] = new IszPart
        {
            stream = mainStream,
            offset = _segmentTable[0].chunkOffset,
            start  = 0,
            end    = 0 // Will be computed after chunk index is read
        };

        string basePath = Path.Combine(imageFilter.ParentFolder, imageFilter.Filename);

        for(var s = 1; s < _segmentTable.Length; s++)
        {
            string segPath = GetSegmentFilename(basePath, s);

            if(!File.Exists(segPath))
            {
                AaruLogging.Error("Cannot find segment file: {0}", segPath);

                return ErrorNumber.NoSuchFile;
            }

            Stream segStream = new FileStream(segPath, FileMode.Open, FileAccess.Read);
            _partStreams.Add(segStream);

            _partTable[s] = new IszPart
            {
                stream = segStream,
                offset = _segmentTable[s].chunkOffset,
                start  = 0, // Will be computed after chunk index is read
                end    = 0
            };

            AaruLogging.Debug(MODULE_NAME, "Opened segment file: {0}", segPath);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads the chunk index/table and computes offsets</summary>
    bool ReadChunkIndex(Stream stream)
    {
        uint numChunks = _header.numChunks;

        if(numChunks == 0)
        {
            AaruLogging.Error("No chunks in ISZ file");

            return false;
        }

        _chunkTable = new IszChunk[numChunks];

        // Do we have a chunk pointer table?
        if(_header.chunkTableOffset != 0)
        {
            if(_header.pointerLength > 4)
            {
                AaruLogging.Error("Unsupported pointer length: {0}", _header.pointerLength);

                return false;
            }

            var chunkBufSize = (int)(numChunks * _header.pointerLength);
            var chunkBuffer  = new byte[chunkBufSize];

            stream.Seek(_header.chunkTableOffset, SeekOrigin.Begin);
            stream.EnsureRead(chunkBuffer, 0, chunkBufSize);

            // Deobfuscate chunk table
            Deobfuscate(chunkBuffer, chunkBufSize);

            // Decode chunk pointers
            var ptrBytes = new byte[4];

            for(uint i = 0; i < numChunks; i++)
            {
                var srcOffset = (int)(i * _header.pointerLength);
                Array.Clear(ptrBytes, 0, 4);
                Array.Copy(chunkBuffer, srcOffset, ptrBytes, 0, _header.pointerLength);

                DecodeChunkPointer(ptrBytes, _header.pointerLength, out IszChunkType type, out uint length);

                _chunkTable[i] = new IszChunk
                {
                    type   = type,
                    length = length
                };
            }
        }
        else
        {
            // No chunk table: all chunks are raw data
            for(uint i = 0; i < numChunks; i++)
            {
                _chunkTable[i] = new IszChunk
                {
                    type = IszChunkType.Data,
                    length = i == numChunks - 1
                                 ? _header.totalSectors * _header.sectorSize % _header.chunkSize
                                 : _header.chunkSize
                };

                // If last chunk size is 0 (exact multiple), use full chunk size
                if(_chunkTable[i].length == 0) _chunkTable[i].length = _header.chunkSize;
            }
        }

        // Compute offsets and segment assignments
        var lastSegment = 0;

        for(uint i = 0; i < numChunks; i++)
        {
            // Compute cumulative offset
            if(i == 0)
            {
                _chunkTable[i].offset         = 0;
                _chunkTable[i].adjustedOffset = 0;
            }
            else
            {
                _chunkTable[i].offset         = _chunkTable[i - 1].offset         + _chunkTable[i - 1].length;
                _chunkTable[i].adjustedOffset = _chunkTable[i - 1].adjustedOffset + _chunkTable[i - 1].length;
            }

            // Determine which segment holds this chunk
            for(var s = 0; s < _segmentTable.Length; s++)
            {
                if(i >= _segmentTable[s].firstChunkNumber &&
                   i < _segmentTable[s].firstChunkNumber + _segmentTable[s].numChunks)
                    _chunkTable[i].segment = (byte)s;
            }

            // Reset adjusted offset when crossing into a new segment
            if(_chunkTable[i].segment > lastSegment)
            {
                lastSegment                   = _chunkTable[i].segment;
                _chunkTable[i].adjustedOffset = 0;
            }
        }

        AaruLogging.Debug(MODULE_NAME, "Parsed {0} chunks", numChunks);

        return true;
    }
}