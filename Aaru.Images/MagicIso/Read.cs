// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads MagicISO UIF disc images.
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
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;
using Session = Aaru.CommonTypes.Structs.Session;
using Track = Aaru.CommonTypes.Structs.Track;

namespace Aaru.Images;

public sealed partial class MagicIso
{
    /// <summary>Determines media type from the total sector count of an optical disc.</summary>
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

    /// <summary>Parses the 64-byte little-endian BBIS footer from a raw buffer.</summary>
    static BbisFooter ParseFooter(byte[] buffer) => new()
    {
        signature    = BitConverter.ToUInt32(buffer, 0),
        footerSize   = BitConverter.ToUInt32(buffer, 4),
        version      = BitConverter.ToUInt16(buffer, 8),
        imageType    = BitConverter.ToUInt16(buffer, 10),
        unknown1     = BitConverter.ToUInt16(buffer, 12),
        padding      = BitConverter.ToUInt16(buffer, 14),
        sectors      = BitConverter.ToUInt32(buffer, 16),
        sectorSize   = BitConverter.ToUInt32(buffer, 20),
        unknown2     = BitConverter.ToUInt32(buffer, 24),
        blhrOffset   = BitConverter.ToUInt64(buffer, 28),
        blhrBbisSize = BitConverter.ToUInt32(buffer, 36),
        hash         = buffer[40..56],
        unknown3     = BitConverter.ToUInt32(buffer, 56),
        unknown4     = BitConverter.ToUInt32(buffer, 60)
    };

    /// <summary>Inflates a zlib-compressed payload using <see cref="ZLibStream" />.</summary>
    static byte[] InflateZlib(byte[] compressed, int expectedSize)
    {
        var output = new byte[expectedSize];

        using var ms   = new MemoryStream(compressed);
        using var zlib = new ZLibStream(ms, CompressionMode.Decompress);

        var read = 0;

        while(read < expectedSize)
        {
            int n = zlib.Read(output, read, expectedSize - read);

            if(n == 0) break;

            read += n;
        }

        if(read != expectedSize) Array.Resize(ref output, read);

        return output;
    }

    /// <summary>Reads a BLHR-style descriptor header (16 bytes, little-endian) at the stream's current position.</summary>
    static BlhrHeader ReadDescriptorHeader(Stream stream)
    {
        var buffer = new byte[16];
        stream.EnsureRead(buffer, 0, 16);

        return new BlhrHeader
        {
            signature = BitConverter.ToUInt32(buffer, 0),
            size      = BitConverter.ToUInt32(buffer, 4),
            version   = BitConverter.ToUInt32(buffer, 8),
            num       = BitConverter.ToUInt32(buffer, 12)
        };
    }

    /// <summary>Parses the BLHR entry array after decompression.</summary>
    static BlhrEntry[] ParseBlhrEntries(byte[] decompressed, uint count)
    {
        var entries = new BlhrEntry[count];

        for(uint i = 0; i < count; i++)
        {
            var off = (int)(i * 24);

            entries[i] = new BlhrEntry
            {
                offset         = BitConverter.ToUInt64(decompressed, off),
                compressedSize = BitConverter.ToUInt32(decompressed, off + 8),
                startSector    = BitConverter.ToUInt32(decompressed, off + 12),
                sectorCount    = BitConverter.ToUInt32(decompressed, off + 16),
                type           = BitConverter.ToUInt32(decompressed, off + 20)
            };
        }

        return entries;
    }

    /// <summary>Locates the BLHR entry that covers a given output sector. Returns the index or -1 when not found.</summary>
    int FindEntryIndex(uint outputSector)
    {
        var lo     = 0;
        int hi     = _blhrStartSectors.Length - 1;
        int result = -1;

        while(lo <= hi)
        {
            int mid = lo + hi >> 1;

            if(_blhrStartSectors[mid] <= outputSector)
            {
                result = mid;
                lo     = mid + 1;
            }
            else
                hi = mid - 1;
        }

        return result;
    }

    /// <summary>Materialises a byte range from the virtual decompressed image.</summary>
    ErrorNumber DecodeLinearRange(ulong byteOffset, int length, out byte[] buffer)
    {
        buffer = null;

        if(length <= 0)
        {
            buffer = [];

            return ErrorNumber.NoError;
        }

        ulong totalBytes = (ulong)_footer.sectors * _footer.sectorSize;

        if(byteOffset >= totalBytes || byteOffset + (ulong)length > totalBytes) return ErrorNumber.OutOfRange;

        buffer = new byte[length];
        var filled = 0;

        ulong sectorSize = _footer.sectorSize;

        while(filled < length)
        {
            ulong currentOffset = byteOffset + (ulong)filled;
            var   currentSector = (uint)(currentOffset / sectorSize);
            var   sectorOffset  = (uint)(currentOffset % sectorSize);

            int idx = FindEntryIndex(currentSector);

            if(idx < 0) return ErrorNumber.SectorNotFound;

            BlhrEntry entry          = _blhrEntries.Values[idx];
            ulong     entrySectorEnd = (ulong)entry.startSector + entry.sectorCount;

            if(currentSector >= entrySectorEnd) return ErrorNumber.SectorNotFound;

            var bytesInEntry = (int)(entrySectorEnd * sectorSize - currentOffset);
            int toCopy       = Math.Min(length - filled, bytesInEntry);

            ErrorNumber errno = GetChunkData(idx, entry, out byte[] chunkData);

            if(errno != ErrorNumber.NoError) return errno;

            uint sectorWithinChunk    = currentSector - entry.startSector;
            var  chunkDataStartOffset = (int)(sectorWithinChunk * sectorSize + sectorOffset);

            Array.Copy(chunkData, chunkDataStartOffset, buffer, filled, toCopy);
            filled += toCopy;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Returns (decompressing if needed) the raw bytes for a BLHR entry.</summary>
    ErrorNumber GetChunkData(int entryIndex, BlhrEntry entry, out byte[] chunkData)
    {
        chunkData = null;

        var cacheKey = (uint)entryIndex;

        if(_chunkCache.TryGetValue(cacheKey, out chunkData)) return ErrorNumber.NoError;

        var decodedSize = (int)(entry.sectorCount * _footer.sectorSize);

        switch(entry.type)
        {
            case BLOCK_TYPE_ZERO:
                chunkData = new byte[decodedSize];

                break;
            case BLOCK_TYPE_RAW:
            {
                chunkData = new byte[decodedSize];

                if(entry.compressedSize > 0)
                {
                    _imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                    var copyLen = (int)Math.Min(entry.compressedSize, (uint)decodedSize);
                    _imageStream.EnsureRead(chunkData, 0, copyLen);
                }

                break;
            }
            case BLOCK_TYPE_ZLIB:
            {
                var compressed = new byte[entry.compressedSize];
                _imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                _imageStream.EnsureRead(compressed, 0, (int)entry.compressedSize);
                chunkData = InflateZlib(compressed, decodedSize);

                if(chunkData.Length < decodedSize)
                {
                    var padded = new byte[decodedSize];
                    Array.Copy(chunkData, 0, padded, 0, chunkData.Length);
                    chunkData = padded;
                }

                break;
            }
            default:
                AaruLogging.Error(Localization.MagicIso_unsupported_block_type_0, entry.type);

                return ErrorNumber.NotSupported;
        }

        if(_currentChunkCacheSize + decodedSize > MAX_CACHE_SIZE)
        {
            _chunkCache.Clear();
            _currentChunkCacheSize = 0;
        }

        _chunkCache[cacheKey]  =  chunkData;
        _currentChunkCacheSize += (uint)decodedSize;

        return ErrorNumber.NoError;
    }

    /// <summary>Builds the single-track layout used for ISO-type UIF images.</summary>
    void BuildIsoLayout(IFilter imageFilter)
    {
        ulong totalSectors = _footer.sectors;

        _magicIsoTracks =
        [
            new MagicIsoTrack
            {
                sequence             = 1,
                session              = 1,
                virtualByteOffset    = 0,
                sectorSize           = 2048,
                cookedBytesPerSector = 2048,
                startSector          = 0,
                endSector            = totalSectors - 1,
                index0               = -150,
                index1               = 0,
                nrgMode              = NRG_MODE_MODE1
            }
        ];

        Tracks =
        [
            new Track
            {
                Sequence          = 1,
                Session           = 1,
                Type              = TrackType.CdMode1,
                StartSector       = 0,
                EndSector         = totalSectors - 1,
                BytesPerSector    = 2048,
                RawBytesPerSector = 2048,
                File              = imageFilter.Filename,
                FileType          = "BINARY",
                Filter            = imageFilter,
                SubchannelType    = TrackSubchannelType.None,
                Description       = string.Format(Localization.Track_0, 1),
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
                Size        = totalSectors * 2048,
                Description = string.Format(Localization.Track_0, 1),
                Type        = "MODE1/2048"
            }
        ];

        _imageInfo.MediaType = CalculateMediaType(totalSectors);
    }

    /// <summary>
    ///     Parses the BLMS track descriptor and produces a multi-track BIN-style layout. Returns
    ///     <see cref="ErrorNumber.NoData" /> when the descriptor is absent or malformed so the caller can fall back.
    /// </summary>
    ErrorNumber BuildBlmsLayout(byte[] blms, IFilter imageFilter)
    {
        if(blms == null || blms.Length < 0x40) return ErrorNumber.NoData;

        int blmsLen = blms.Length;
        var tracks  = new List<MagicIsoTrack>();

        for(var p = 0x40; p + 68 <= blmsLen; p += 68)
        {
            // High "Q sub-channel" control flags (0xA0/0xA1/0xA2) signal lead-in/lead-out entries, not tracks.
            if((blms[p + 3] & 0xA0) != 0) continue;

            byte trackNumber = blms[p                        + 3];
            byte adr         = blms[p                        + 1];
            byte mm          = blms[p                        + 8];
            byte ssRaw       = blms[p                        + 9];
            byte ff          = blms[p                        + 10];
            byte mode        = blms[p                        + 11];
            var  sectorSize  = BitConverter.ToUInt32(blms, p + 24);

            if(sectorSize is not (2048 or 2336 or 2352)) sectorSize = 2352;

            // Stored MSF includes the 2-second lead-in used by NRG/BIN formats.
            int absoluteMsf           = mm * 60 * 75 + ssRaw * 75 + ff;
            int startLba              = absoluteMsf  - 150;
            if(startLba < 0) startLba = 0;

            bool isAudio = adr is 0x10 or 0x12;

            uint nrgMode;
            var  rawMode1 = false;
            var  rawMode2 = false;
            uint cookedBps;

            if(isAudio)
            {
                nrgMode    = NRG_MODE_AUDIO;
                cookedBps  = 2352;
                sectorSize = 2352;
            }
            else
            {
                switch(mode)
                {
                    case 1:
                        nrgMode   = NRG_MODE_MODE1;
                        cookedBps = 2048;
                        rawMode1  = sectorSize == 2352;

                        break;
                    case 2:
                        nrgMode   = NRG_MODE_MODE2_FORM1;
                        cookedBps = 2048;
                        rawMode2  = sectorSize == 2352;

                        break;
                    default:
                        nrgMode   = NRG_MODE_MODE1;
                        cookedBps = sectorSize == 2352 ? 2048u : sectorSize;
                        rawMode1  = sectorSize == 2352;

                        break;
                }
            }

            var track = new MagicIsoTrack
            {
                sequence             = trackNumber,
                session              = 1,
                virtualByteOffset    = (ulong)startLba * sectorSize,
                sectorSize           = sectorSize,
                cookedBytesPerSector = cookedBps,
                startSector          = (ulong)startLba,
                index0               = startLba == 0 ? -150 : startLba,
                index1               = startLba == 0 ? 0 : startLba + 150,
                nrgMode              = nrgMode,
                rawMode1             = rawMode1,
                rawMode2             = rawMode2
            };

            tracks.Add(track);
        }

        if(tracks.Count == 0) return ErrorNumber.NoData;

        tracks.Sort((a, b) => a.sequence.CompareTo(b.sequence));

        // Compute end sectors from the byte layout of the decompressed stream.
        for(var i = 0; i < tracks.Count; i++)
        {
            MagicIsoTrack t = tracks[i];

            ulong endByte = i + 1 < tracks.Count
                                ? tracks[i + 1].virtualByteOffset
                                : (ulong)_footer.sectors * _footer.sectorSize;

            ulong lengthSectors = (endByte - t.virtualByteOffset) / t.sectorSize;
            t.endSector = t.startSector + lengthSectors - 1;
            tracks[i]   = t;
        }

        return FinalizeOpticalLayout(tracks, imageFilter);
    }

    /// <summary>Parses the NRG trailer embedded in the decompressed stream and produces a multi-track layout.</summary>
    ErrorNumber BuildNrgLayout(IFilter imageFilter)
    {
        ulong totalBytes = (ulong)_footer.sectors * _footer.sectorSize;
        uint  sectorSize = _footer.sectorSize;

        // Read the last sector to locate the NER5/NERO anchor.
        ErrorNumber errno = DecodeLinearRange(totalBytes - sectorSize, (int)sectorSize, out byte[] lastSector);

        if(errno != ErrorNumber.NoError) return errno;

        ulong nrgChunkOffset = 0;
        var   foundAnchor    = false;
        var   anchorIsV2     = false;

        for(int i = lastSector.Length - 12; i >= 0; i--)
        {
            uint tag = BinaryPrimitives.ReadUInt32BigEndian(lastSector.AsSpan(i, 4));

            if(tag == NRG_ANCHOR_NER5)
            {
                nrgChunkOffset = BinaryPrimitives.ReadUInt64BigEndian(lastSector.AsSpan(i + 4, 8));
                foundAnchor    = true;
                anchorIsV2     = true;

                break;
            }

            if(tag != NRG_ANCHOR_NERO) continue;

            nrgChunkOffset = BinaryPrimitives.ReadUInt32BigEndian(lastSector.AsSpan(i + 4, 4));
            foundAnchor    = true;

            break;
        }

        if(!foundAnchor || nrgChunkOffset >= totalBytes)
        {
            AaruLogging.Error(Localization.MagicIso_NRG_trailer_not_found);

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "NRG anchor offset = {0} (v{1})", nrgChunkOffset, anchorIsV2 ? 2 : 1);

        // Read the trailer payload into memory (bounded from the anchor offset up to the end of the stream).
        var trailerLength = (int)(totalBytes - nrgChunkOffset);

        if(trailerLength < 8) return ErrorNumber.InvalidArgument;

        errno = DecodeLinearRange(nrgChunkOffset, trailerLength, out byte[] trailer);

        if(errno != ErrorNumber.NoError) return errno;

        var   tracks       = new List<MagicIsoTrack>();
        ulong totalLba     = 0;
        var   pos          = 0;
        uint  currentTrack = 1;

        while(pos + 8 <= trailer.Length)
        {
            uint chunkId   = BinaryPrimitives.ReadUInt32BigEndian(trailer.AsSpan(pos,     4));
            uint chunkSize = BinaryPrimitives.ReadUInt32BigEndian(trailer.AsSpan(pos + 4, 4));
            pos += 8;

            if(chunkId == NRG_CHUNK_END || chunkId == NRG_ANCHOR_NER5 || chunkId == NRG_ANCHOR_NERO) break;

            if(pos + chunkSize > trailer.Length) break;

            ReadOnlySpan<byte> payload = trailer.AsSpan(pos, (int)chunkSize);
            pos += (int)chunkSize;

            switch(chunkId)
            {
                case NRG_CHUNK_DAOX:
                case NRG_CHUNK_DAOI:
                {
                    if(chunkSize < 22) break;

                    int indexWidth = chunkId == NRG_CHUNK_DAOX ? 8 : 4;
                    int recordSize = 10 + 4 + 4 + indexWidth * 3;
                    var cursor     = 22;

                    while(cursor + recordSize <= chunkSize)
                    {
                        int recStart = cursor;

                        // ISRC + other metadata occupies the first 10 bytes of each record.
                        cursor += 10;

                        ushort recSectorSize = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(cursor + 2, 2));
                        cursor += 4;
                        byte modeByte = payload[cursor];
                        cursor += 4;
                        ulong index0 = ReadNrgIndex(payload, cursor, indexWidth);
                        cursor += indexWidth;
                        ulong index1 = ReadNrgIndex(payload, cursor, indexWidth);
                        cursor += indexWidth;
                        ulong index2 = ReadNrgIndex(payload, cursor, indexWidth);
                        cursor += indexWidth;

                        if(recSectorSize == 0) recSectorSize = (ushort)sectorSize;

                        // MagicISO mis-declares raw data tracks as mode 0/2/3 with sector size 2352.
                        var  rawMode1 = false;
                        var  rawMode2 = false;
                        uint nrgMode  = modeByte;

                        if(recSectorSize == 2352)
                        {
                            switch(modeByte)
                            {
                                case NRG_MODE_MODE1:
                                    rawMode1 = true;

                                    break;
                                case NRG_MODE_MODE2_FORM1:
                                case NRG_MODE_MODE2_RAW:
                                    rawMode2 = true;
                                    nrgMode  = NRG_MODE_MODE2_FORM1;

                                    break;
                            }
                        }

                        uint cookedBps = nrgMode == NRG_MODE_AUDIO
                                             ? 2352u
                                             : recSectorSize == 2352
                                                 ? 2048u
                                                 : recSectorSize == 2336
                                                     ? 2336u
                                                     : 2048u;

                        ulong trackBytes  = index2 - index1;
                        ulong sectorCount = recSectorSize == 0 ? 0 : trackBytes / recSectorSize;

                        var track = new MagicIsoTrack
                        {
                            sequence             = currentTrack,
                            session              = 1,
                            virtualByteOffset    = index1,
                            sectorSize           = recSectorSize,
                            cookedBytesPerSector = cookedBps,
                            startSector          = totalLba,
                            endSector            = totalLba + sectorCount - 1,
                            index0 = (int)totalLba -
                                     (int)((index1 - index0) / (recSectorSize == 0 ? 1u : recSectorSize)),
                            index1   = (int)totalLba,
                            nrgMode  = nrgMode,
                            rawMode1 = rawMode1,
                            rawMode2 = rawMode2
                        };

                        tracks.Add(track);
                        totalLba += sectorCount;
                        currentTrack++;

                        // Discard any remainder inside the record (shouldn't happen but keeps the parser robust).
                        int recEnd                 = recStart + recordSize;
                        if(cursor < recEnd) cursor = recEnd;
                    }

                    break;
                }
                case NRG_CHUNK_ETNF:
                case NRG_CHUNK_ETN2:
                {
                    int indexWidth = chunkId == NRG_CHUNK_ETN2 ? 8 : 4;
                    int recordSize = indexWidth * 2 + 4 + 4 + 4;
                    var cursor     = 0;

                    while(cursor + recordSize <= chunkSize)
                    {
                        ulong start = ReadNrgIndex(payload, cursor, indexWidth);
                        cursor += indexWidth;
                        ulong end = ReadNrgIndex(payload, cursor, indexWidth);
                        cursor += indexWidth;
                        byte modeByte = payload[cursor];
                        cursor += 4;
                        cursor += 4 + 4;

                        const ushort recSectorSize = 2352;
                        ulong        trackBytes    = end - start;
                        ulong        sectorCount   = trackBytes / recSectorSize;

                        uint nrgMode  = modeByte;
                        bool rawMode1 = modeByte == NRG_MODE_MODE1;
                        bool rawMode2 = modeByte is NRG_MODE_MODE2_FORM1 or NRG_MODE_MODE2_RAW;

                        if(rawMode2) nrgMode = NRG_MODE_MODE2_FORM1;

                        uint cookedBps = modeByte == NRG_MODE_AUDIO ? 2352u : 2048u;

                        tracks.Add(new MagicIsoTrack
                        {
                            sequence             = currentTrack,
                            session              = 1,
                            virtualByteOffset    = start,
                            sectorSize           = recSectorSize,
                            cookedBytesPerSector = cookedBps,
                            startSector          = totalLba,
                            endSector            = totalLba + sectorCount - 1,
                            index0               = (int)totalLba,
                            index1               = (int)totalLba,
                            nrgMode              = nrgMode,
                            rawMode1             = rawMode1,
                            rawMode2             = rawMode2
                        });

                        totalLba += sectorCount;
                        currentTrack++;
                    }

                    break;
                }
                case NRG_CHUNK_CDTX:
                    _cdText = payload.ToArray();

                    if(!_imageInfo.ReadableMediaTags.Contains(MediaTagType.CD_TEXT))
                        _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);

                    break;
            }
        }

        if(tracks.Count == 0)
        {
            AaruLogging.Error(Localization.MagicIso_NRG_trailer_has_no_tracks);

            return ErrorNumber.InvalidArgument;
        }

        return FinalizeOpticalLayout(tracks, imageFilter);
    }

    /// <summary>Reads an index value (32-bit or 64-bit big-endian) from an NRG DAO/ETN record.</summary>
    static ulong ReadNrgIndex(ReadOnlySpan<byte> payload, int offset, int width) => width == 8
        ? BinaryPrimitives.ReadUInt64BigEndian(payload.Slice(offset, 8))
        : BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(offset, 4));

    /// <summary>Finalises <see cref="Tracks" />, <see cref="Sessions" /> and <see cref="Partitions" /> from parsed tracks.</summary>
    ErrorNumber FinalizeOpticalLayout(List<MagicIsoTrack> tracks, IFilter imageFilter)
    {
        _magicIsoTracks = tracks;
        Tracks          = [];
        Sessions        = [];
        Partitions      = [];

        ulong discSectors = 0;

        foreach(MagicIsoTrack t in tracks)
        {
            var track = new Track
            {
                Sequence          = t.sequence,
                Session           = t.session,
                StartSector       = t.startSector,
                EndSector         = t.endSector,
                BytesPerSector    = (int)t.cookedBytesPerSector,
                RawBytesPerSector = (int)t.sectorSize,
                File              = imageFilter.Filename,
                FileType          = "BINARY",
                Filter            = imageFilter,
                SubchannelType    = TrackSubchannelType.None,
                Description       = string.Format(Localization.Track_0, t.sequence),
                Type              = NrgModeToTrackType(t.nrgMode),
                Indexes = new Dictionary<ushort, int>
                {
                    [1] = t.index1
                }
            };

            if(t.index0 < t.index1) track.Indexes[0] = t.index0;

            Tracks.Add(track);

            Partitions.Add(new Partition
            {
                Sequence    = t.sequence - 1,
                Start       = t.startSector,
                Length      = t.endSector - t.startSector + 1,
                Offset      = t.virtualByteOffset,
                Size        = (t.endSector - t.startSector + 1) * t.sectorSize,
                Description = string.Format(Localization.Track_0, t.sequence),
                Type        = track.Type.ToString()
            });

            if(t.endSector + 1 > discSectors) discSectors = t.endSector + 1;

            if(t.rawMode1 || t.rawMode2)
            {
                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                if(t.rawMode2 && !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
            }
        }

        Sessions.Add(new Session
        {
            Sequence    = 1,
            StartTrack  = tracks[0].sequence,
            EndTrack    = tracks[^1].sequence,
            StartSector = tracks[0].startSector,
            EndSector   = tracks[^1].endSector
        });

        _imageInfo.MediaType = CalculateMediaType(discSectors);

        return ErrorNumber.NoError;
    }

    static TrackType NrgModeToTrackType(uint nrgMode) => nrgMode switch
                                                         {
                                                             NRG_MODE_AUDIO       => TrackType.Audio,
                                                             NRG_MODE_MODE2_FORM1 => TrackType.CdMode2Form1,
                                                             NRG_MODE_MODE2_RAW   => TrackType.CdMode2Formless,
                                                             _                    => TrackType.CdMode1
                                                         };

    static ErrorNumber CopyTagSlice(byte[] raw, uint length, int offset, int size, out byte[] buffer)
    {
        buffer = new byte[length * size];

        for(uint i = 0; i < length; i++) Array.Copy(raw, i * 2352 + offset, buffer, i * size, size);

        return ErrorNumber.NoError;
    }

    MagicIsoTrack? FindTrackBySequence(uint sequence)
    {
        foreach(MagicIsoTrack t in _magicIsoTracks)
            if(t.sequence == sequence)
                return t;

        return null;
    }

    MagicIsoTrack? FindTrackByAbsoluteLba(ulong lba)
    {
        foreach(MagicIsoTrack t in _magicIsoTracks)
            if(lba >= t.startSector && lba <= t.endSector)
                return t;

        return null;
    }

    static ErrorNumber SetStatusAndReturn(out SectorStatus status, SectorStatus[] arr)
    {
        status = arr is { Length: > 0 } ? arr[0] : SectorStatus.NotDumped;

        return ErrorNumber.NoError;
    }

    static ErrorNumber StatusFail(out SectorStatus status)
    {
        status = SectorStatus.NotDumped;

        return ErrorNumber.InvalidArgument;
    }

#region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        if(stream.Length < BBIS_FOOTER_SIZE) return ErrorNumber.InvalidArgument;

        stream.Seek(-BBIS_FOOTER_SIZE, SeekOrigin.End);
        var footerBytes = new byte[BBIS_FOOTER_SIZE];
        stream.EnsureRead(footerBytes, 0, BBIS_FOOTER_SIZE);
        _footer = ParseFooter(footerBytes);

        if(_footer.signature != BBIS_SIGNATURE)
        {
            AaruLogging.Error(Localization.MagicIso_bbis_signature_not_found);

            return ErrorNumber.InvalidArgument;
        }

        AaruLogging.Debug(MODULE_NAME, "footer.version = {0}",    _footer.version);
        AaruLogging.Debug(MODULE_NAME, "footer.imageType = {0}",  _footer.imageType);
        AaruLogging.Debug(MODULE_NAME, "footer.sectors = {0}",    _footer.sectors);
        AaruLogging.Debug(MODULE_NAME, "footer.sectorSize = {0}", _footer.sectorSize);
        AaruLogging.Debug(MODULE_NAME, "footer.blhrOffset = {0}", _footer.blhrOffset);

        if(_footer.sectors == 0 || _footer.sectorSize == 0) return ErrorNumber.InvalidArgument;

        // Seek to the BLHR descriptor and read its header.
        stream.Seek((long)_footer.blhrOffset, SeekOrigin.Begin);
        BlhrHeader blhrHeader = ReadDescriptorHeader(stream);

        if(blhrHeader.signature == BSDR_SIGNATURE)
        {
            AaruLogging.Error(Localization.MagicIso_password_protected_images_are_not_supported);

            return ErrorNumber.NotSupported;
        }

        if(blhrHeader.signature != BLHR_SIGNATURE)
        {
            AaruLogging.Error(Localization.MagicIso_blhr_signature_not_found);

            return ErrorNumber.InvalidArgument;
        }

        var compressedBlhrSize = (int)(blhrHeader.size - 8);
        var uncompressedSize   = (int)(blhrHeader.num * 24u);

        if(compressedBlhrSize <= 0 || uncompressedSize <= 0) return ErrorNumber.InvalidArgument;

        var compressedBlhr = new byte[compressedBlhrSize];
        stream.EnsureRead(compressedBlhr, 0, compressedBlhrSize);

        byte[]      blhrTable = InflateZlib(compressedBlhr, uncompressedSize);
        BlhrEntry[] entries   = ParseBlhrEntries(blhrTable, blhrHeader.num);

        _blhrEntries      = new SortedList<uint, BlhrEntry>(entries.Length);
        _blhrStartSectors = new uint[entries.Length];

        for(var i = 0; i < entries.Length; i++)
        {
            _blhrEntries[entries[i].startSector] = entries[i];
            _blhrStartSectors[i]                 = entries[i].startSector;
        }

        Array.Sort(_blhrStartSectors);

        _chunkCache            = new Dictionary<uint, byte[]>();
        _currentChunkCacheSize = 0;
        _sectorCache           = new Dictionary<ulong, byte[]>();
        _sectorBuilder         = new SectorBuilder();
        _imageStream           = stream;
        _imageFilter           = imageFilter;

        // Common image info
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Application          = "MagicISO";
        _imageInfo.Version              = $"{_footer.version}";
        _imageInfo.ImageSize            = (ulong)_footer.sectors * _footer.sectorSize;
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;
        _imageInfo.SectorSize           = _footer.sectorSize;

        // Attempt to read optional BLMS/BLSS descriptors after the BLHR payload.
        byte[] blmsData  = null;
        var    blssIsNrg = false;

        if(_footer.imageType == IMAGE_TYPE_MIXED && stream.Position + 16 <= stream.Length)
        {
            BlhrHeader blms = ReadDescriptorHeader(stream);

            if(blms.signature == BLMS_SIGNATURE)
            {
                var compBlms = new byte[blms.size - 8];
                stream.EnsureRead(compBlms, 0, compBlms.Length);

                try
                {
                    blmsData = InflateZlib(compBlms, (int)blms.num);
                }
                catch(Exception ex)
                {
                    AaruLogging.Debug(MODULE_NAME, "BLMS inflate failed: {0}", ex.Message);
                }
            }

            if(stream.Position + 16 <= stream.Length)
            {
                BlhrHeader blss = ReadDescriptorHeader(stream);

                if(blss.signature == BLSS_SIGNATURE)
                {
                    // BLSS descriptor header is followed by 4 extra bytes before its payload.
                    stream.Seek(4, SeekOrigin.Current);

                    if(blss.num == 0) blssIsNrg = true;
                }
            }
        }

        _variant = _footer.imageType switch
                   {
                       IMAGE_TYPE_ISO                  => UifVariant.Iso,
                       IMAGE_TYPE_MIXED when blssIsNrg => UifVariant.NrgTrailer,
                       IMAGE_TYPE_MIXED                => UifVariant.NrgTrailer,
                       _                               => UifVariant.Iso
                   };

        if(_footer.imageType == IMAGE_TYPE_ISO)
            BuildIsoLayout(imageFilter);
        else if(_footer.imageType == IMAGE_TYPE_MIXED)
        {
            ErrorNumber layoutError = ErrorNumber.NoData;

            if(!blssIsNrg && blmsData != null) layoutError = BuildBlmsLayout(blmsData, imageFilter);

            if(layoutError != ErrorNumber.NoError) layoutError = BuildNrgLayout(imageFilter);

            if(layoutError != ErrorNumber.NoError) return layoutError;
        }
        else
        {
            AaruLogging.Error(Localization.MagicIso_unsupported_image_type_0, _footer.imageType);

            return ErrorNumber.NotSupported;
        }

        _imageInfo.Sectors = Tracks[^1].EndSector + 1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDPM(out uint dpmStartSector, out uint dpmResolution, out uint numberOfDpmEntries, out ulong[] dpm)
    {
        dpmStartSector     = 0;
        dpmResolution      = 0;
        numberOfDpmEntries = 0;
        dpm                = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorDPM(ulong sectorAddress, out ulong? dpm)
    {
        dpm = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus) =>
        ReadSectors(sectorAddress, 1, track, out buffer, out SectorStatus[] s) == ErrorNumber.NoError
            ? SetStatusAndReturn(out sectorStatus, s)
            : StatusFail(out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.NotSupported;

        MagicIsoTrack? found = FindTrackByAbsoluteLba(sectorAddress);

        if(found is null) return ErrorNumber.SectorNotFound;

        MagicIsoTrack t = found.Value;

        return ReadSector(sectorAddress - t.startSector, t.sequence, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        MagicIsoTrack? found = FindTrackBySequence(track);

        if(found is null) return ErrorNumber.SectorNotFound;

        MagicIsoTrack t = found.Value;

        if(sectorAddress + length > t.endSector - t.startSector + 1) return ErrorNumber.OutOfRange;

        // Read raw sectors from the virtual stream, then strip to the cooked user-data size when required.
        ulong       byteOffset = t.virtualByteOffset + sectorAddress * t.sectorSize;
        var         rawLength  = (int)(length * t.sectorSize);
        ErrorNumber errno      = DecodeLinearRange(byteOffset, rawLength, out byte[] raw);

        if(errno != ErrorNumber.NoError) return errno;

        if(t.sectorSize == t.cookedBytesPerSector)
            buffer = raw;
        else
        {
            // Raw 2352 → cooked user data
            buffer = new byte[length * t.cookedBytesPerSector];

            for(uint i = 0; i < length; i++)
            {
                var srcOff = (int)(i * t.sectorSize);

                if(t.rawMode1 && t.sectorSize == 2352)
                    Array.Copy(raw,
                               srcOff + 16,
                               buffer,
                               (int)(i * t.cookedBytesPerSector),
                               (int)t.cookedBytesPerSector);
                else if(t.rawMode2 && t.sectorSize == 2352)
                    Array.Copy(raw,
                               srcOff + 24,
                               buffer,
                               (int)(i * t.cookedBytesPerSector),
                               (int)t.cookedBytesPerSector);
                else
                    Array.Copy(raw, srcOff, buffer, (int)(i * t.cookedBytesPerSector), (int)t.cookedBytesPerSector);
            }
        }

        sectorStatus = Enumerable.Repeat(SectorStatus.Dumped, (int)length).ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        MagicIsoTrack? found = FindTrackByAbsoluteLba(sectorAddress);

        if(found is null) return ErrorNumber.SectorNotFound;

        MagicIsoTrack t = found.Value;

        return ReadSectors(sectorAddress - t.startSector, length, t.sequence, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        ErrorNumber errno = ReadSectorsLong(sectorAddress, 1, track, out buffer, out SectorStatus[] statuses);
        sectorStatus = errno == ErrorNumber.NoError ? statuses[0] : SectorStatus.NotDumped;

        return errno;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.NotSupported;

        MagicIsoTrack? found = FindTrackByAbsoluteLba(sectorAddress);

        if(found is null) return ErrorNumber.SectorNotFound;

        MagicIsoTrack t = found.Value;

        return ReadSectorLong(sectorAddress - t.startSector, t.sequence, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        MagicIsoTrack? found = FindTrackBySequence(track);

        if(found is null) return ErrorNumber.SectorNotFound;

        MagicIsoTrack t = found.Value;

        if(sectorAddress + length > t.endSector - t.startSector + 1) return ErrorNumber.OutOfRange;

        // If underlying data is already raw 2352, return it verbatim.
        if(t.sectorSize == 2352)
        {
            ulong       byteOffset = t.virtualByteOffset + sectorAddress * 2352;
            ErrorNumber errno      = DecodeLinearRange(byteOffset, (int)(length * 2352), out buffer);

            if(errno != ErrorNumber.NoError) return errno;

            sectorStatus = Enumerable.Repeat(SectorStatus.Dumped, (int)length).ToArray();

            return ErrorNumber.NoError;
        }

        // Otherwise reconstruct 2352-byte sectors from 2048-byte user data.
        if(t.cookedBytesPerSector != 2048 || t.nrgMode != NRG_MODE_MODE1) return ErrorNumber.NotSupported;

        ErrorNumber userErrno =
            ReadSectors(sectorAddress, length, track, out byte[] userData, out SectorStatus[] userStatus);

        if(userErrno != ErrorNumber.NoError) return userErrno;

        buffer       = new byte[length * 2352];
        sectorStatus = userStatus;

        for(uint i = 0; i < length; i++)
        {
            var raw = new byte[2352];
            Array.Copy(userData, i * 2048, raw, 16, 2048);
            _sectorBuilder.ReconstructPrefix(ref raw, TrackType.CdMode1, (long)(t.startSector + sectorAddress + i));
            _sectorBuilder.ReconstructEcc(ref raw, TrackType.CdMode1);
            Array.Copy(raw, 0, buffer, i * 2352, 2352);
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        MagicIsoTrack? found = FindTrackByAbsoluteLba(sectorAddress);

        if(found is null) return ErrorNumber.SectorNotFound;

        MagicIsoTrack t = found.Value;

        return ReadSectorsLong(sectorAddress - t.startSector, length, t.sequence, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, track, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(negative) return ErrorNumber.NotSupported;

        MagicIsoTrack? found = FindTrackByAbsoluteLba(sectorAddress);

        if(found is null) return ErrorNumber.SectorNotFound;

        MagicIsoTrack t = found.Value;

        return ReadSectorTag(sectorAddress - t.startSector, t.sequence, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(tag == SectorTagType.CdTrackFlags)
        {
            MagicIsoTrack? trackFlagsTrack = FindTrackBySequence(track);

            if(trackFlagsTrack is null) return ErrorNumber.SectorNotFound;

            buffer = [trackFlagsTrack.Value.nrgMode == NRG_MODE_AUDIO ? (byte)0 : (byte)4];

            return ErrorNumber.NoError;
        }

        ErrorNumber errno = ReadSectorsLong(sectorAddress, length, track, out byte[] raw, out _);

        if(errno != ErrorNumber.NoError) return errno;

        switch(tag)
        {
            case SectorTagType.CdSectorSync:
                return CopyTagSlice(raw, length, 0, 12, out buffer);
            case SectorTagType.CdSectorHeader:
                return CopyTagSlice(raw, length, 12, 4, out buffer);
            case SectorTagType.CdSectorSubHeader:
                return CopyTagSlice(raw, length, 16, 8, out buffer);
            case SectorTagType.CdSectorEdc:
                return CopyTagSlice(raw, length, 2064, 4, out buffer);
            case SectorTagType.CdSectorEccP:
                return CopyTagSlice(raw, length, 2076, 172, out buffer);
            case SectorTagType.CdSectorEccQ:
                return CopyTagSlice(raw, length, 2248, 104, out buffer);
            case SectorTagType.CdSectorEcc:
                return CopyTagSlice(raw, length, 2076, 276, out buffer);
            default:
                return ErrorNumber.NotSupported;
        }
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(negative) return ErrorNumber.NotSupported;

        MagicIsoTrack? found = FindTrackByAbsoluteLba(sectorAddress);

        if(found is null) return ErrorNumber.SectorNotFound;

        MagicIsoTrack t = found.Value;

        return ReadSectorsTag(sectorAddress - t.startSector, length, t.sequence, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        switch(tag)
        {
            case MediaTagType.CD_TEXT:
                if(_cdText == null) return ErrorNumber.NoData;

                buffer = (byte[])_cdText.Clone();

                return ErrorNumber.NoError;
            case MediaTagType.CD_MCN:
                if(_upc == null) return ErrorNumber.NoData;

                buffer = (byte[])_upc.Clone();

                return ErrorNumber.NoError;
            default:
                return ErrorNumber.NotSupported;
        }
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) => session.Sequence == 1 ? Tracks : null;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => session == 1 ? Tracks : null;

#endregion
}