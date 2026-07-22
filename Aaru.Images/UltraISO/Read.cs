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
//     Reads UltraISO disc images.
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
using System.IO.Compression;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Logging;
using Aaru.Compression;

namespace Aaru.Images;

public sealed partial class UltraISO
{
    /// <summary>Decompresses a chunk and returns the decompressed data</summary>
    ErrorNumber DecompressChunk(int chunkIndex, out byte[] decompressedData)
    {
        decompressedData = null;

        if(chunkIndex < 0 || chunkIndex >= _chunkTable.Length) return ErrorNumber.OutOfRange;

        // Check chunk cache
        if(_chunkCache.TryGetValue(chunkIndex, out byte[] cachedChunk))
        {
            decompressedData = cachedChunk;

            return ErrorNumber.NoError;
        }

        IszChunk chunk = _chunkTable[chunkIndex];

        // Determine the decompressed size for this chunk
        uint decompressedSize = _header.chunkSize;

        // Last chunk may be smaller
        ulong totalImageSize = (ulong)_header.totalSectors * _header.sectorSize;
        ulong chunkStart     = (ulong)chunkIndex           * _header.chunkSize;

        if(chunkStart + decompressedSize > totalImageSize) decompressedSize = (uint)(totalImageSize - chunkStart);

        switch(chunk.type)
        {
            case IszChunkType.Zero:
                decompressedData = new byte[decompressedSize];

                break;

            case IszChunkType.Data:
            {
                decompressedData = new byte[decompressedSize];
                ReadChunkData(chunk, decompressedData, (int)chunk.length);

                break;
            }

            case IszChunkType.Zlib:
            {
                var compressedData = new byte[chunk.length];
                ReadChunkData(chunk, compressedData, (int)chunk.length);

                decompressedData = new byte[decompressedSize];

                using var compressedStream = new MemoryStream(compressedData);

                // Skip the 2-byte zlib header
                compressedStream.Seek(2, SeekOrigin.Begin);

                using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
                var       totalRead     = 0;

                while(totalRead < decompressedSize)
                {
                    int read = deflateStream.Read(decompressedData, totalRead, (int)decompressedSize - totalRead);

                    if(read == 0) break;

                    totalRead += read;
                }

                break;
            }

            case IszChunkType.Bz2:
            {
                var compressedData = new byte[chunk.length];
                ReadChunkData(chunk, compressedData, (int)chunk.length);

                // Restore BZh header: first 3 bytes should be 'B', 'Z', 'h'
                compressedData[0] = (byte)'B';
                compressedData[1] = (byte)'Z';
                compressedData[2] = (byte)'h';

                decompressedData = new byte[decompressedSize];

                BZip2.DecodeBuffer(compressedData, decompressedData);

                break;
            }

            default:
                AaruLogging.Error("Unknown chunk type {0} for chunk {1}", chunk.type, chunkIndex);

                return ErrorNumber.NotSupported;
        }

        // Cache the decompressed chunk (evict if cache is full)
        while(_currentChunkCacheSize + decompressedData.Length > MAX_CACHE_SIZE && _chunkCache.Count > 0)
        {
            // Evict first entry
            using Dictionary<int, byte[]>.Enumerator enumerator = _chunkCache.GetEnumerator();

            if(!enumerator.MoveNext()) break;

            _currentChunkCacheSize -= (uint)enumerator.Current.Value.Length;
            _chunkCache.Remove(enumerator.Current.Key);
        }

        _chunkCache[chunkIndex] =  decompressedData;
        _currentChunkCacheSize  += (uint)decompressedData.Length;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads raw (possibly compressed) chunk data from the correct segment stream</summary>
    void ReadChunkData(IszChunk chunk, byte[] destination, int length)
    {
        byte    segment = chunk.segment;
        IszPart part    = _partTable[segment];
        Stream  stream  = part.stream;

        var seekPosition = (long)(part.offset + chunk.adjustedOffset);
        stream.Seek(seekPosition, SeekOrigin.Begin);

        // Check if this chunk might span across segments (leftSize > 0)
        if(segment < _segmentTable.Length - 1 && _segmentTable[segment + 1].leftSize > 0)
        {
            long available = stream.Length - seekPosition;

            if(available < length)
            {
                // Read what's available from the current segment
                var firstPart = (int)available;
                stream.ReadExactly(destination, 0, firstPart);

                // Read the rest from the next segment
                var     nextSeg    = (byte)(segment + 1);
                IszPart nextPart   = _partTable[nextSeg];
                Stream  nextStream = nextPart.stream;
                long    nextOffset = (long)nextPart.offset - (int)_segmentTable[nextSeg].leftSize;

                nextStream.Seek(nextOffset, SeekOrigin.Begin);
                nextStream.ReadExactly(destination, firstPart, length - firstPart);

                return;
            }
        }

        stream.ReadExactly(destination, 0, length);
    }

#region IOpticalMediaImage Members

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
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(sectorAddress >= _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        // Check sector cache
        if(_sectorCache.TryGetValue(sectorAddress, out byte[] cachedSector))
        {
            buffer       = cachedSector;
            sectorStatus = SectorStatus.Dumped;

            return ErrorNumber.NoError;
        }

        // Find which chunk contains this sector
        uint sectorsPerChunk = _header.chunkSize / _header.sectorSize;
        var  chunkIndex      = (int)(sectorAddress  / sectorsPerChunk);
        var  sectorInChunk   = (uint)(sectorAddress % sectorsPerChunk);

        ErrorNumber errno = DecompressChunk(chunkIndex, out byte[] decompressedData);

        if(errno != ErrorNumber.NoError) return errno;

        // Extract the sector from the decompressed chunk
        buffer = new byte[_header.sectorSize];
        uint offset = sectorInChunk * _header.sectorSize;

        // Handle last chunk which may be smaller
        if(offset + _header.sectorSize > decompressedData.Length)
        {
            var available = (uint)(decompressedData.Length - offset);
            Array.Copy(decompressedData, offset, buffer, 0, Math.Min(available, _header.sectorSize));
        }
        else
            Array.Copy(decompressedData, offset, buffer, 0, _header.sectorSize);

        sectorStatus = SectorStatus.Dumped;

        // Cache the sector
        if(_sectorCache.Count >= MAX_CACHED_SECTORS) _sectorCache.Clear();

        _sectorCache[sectorAddress] = buffer;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms       = new MemoryStream();
        var statuses = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, track, out byte[] sector, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sector, 0, sector.Length);
            statuses[i] = status;
        }

        buffer       = ms.ToArray();
        sectorStatus = statuses;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong            sectorAddress, bool negative, out byte[] buffer,
                                  out SectorStatus sectorStatus) =>
        ReadSector(sectorAddress, 1, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus) =>
        ReadSectors(sectorAddress, length, 1, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(_imageInfo.MediaType != MediaType.CD) return ErrorNumber.NotSupported;

        ErrorNumber errno = ReadSector(sectorAddress, track, out byte[] userData, out SectorStatus _);

        if(errno != ErrorNumber.NoError) return errno;

        var fullSector = new byte[2352];
        Array.Copy(userData, 0, fullSector, 16, 2048);
        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode1, (long)sectorAddress);
        _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode1);

        buffer       = fullSector;
        sectorStatus = SectorStatus.Dumped;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(_imageInfo.MediaType != MediaType.CD) return ErrorNumber.NotSupported;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms       = new MemoryStream();
        var statuses = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSectorLong(sectorAddress + i, track, out byte[] sector, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sector, 0, sector.Length);
            statuses[i] = status;
        }

        buffer       = ms.ToArray();
        sectorStatus = statuses;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus) =>
        ReadSectorLong(sectorAddress, 1, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus) =>
        ReadSectorsLong(sectorAddress, length, 1, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.MediaType != MediaType.CD) return ErrorNumber.NotSupported;

        switch(tag)
        {
            case SectorTagType.CdTrackFlags:
                buffer = [(byte)CdFlags.DataTrack];

                return ErrorNumber.NoError;
            case SectorTagType.CdSectorSync:
            case SectorTagType.CdSectorHeader:
            case SectorTagType.CdSectorEdc:
            case SectorTagType.CdSectorEccP:
            case SectorTagType.CdSectorEccQ:
            case SectorTagType.CdSectorEcc:
                break;
            default:
                return ErrorNumber.NotSupported;
        }

        ErrorNumber errno = ReadSectorLong(sectorAddress, track, out byte[] fullSector, out SectorStatus _);

        if(errno != ErrorNumber.NoError) return errno;

        switch(tag)
        {
            case SectorTagType.CdSectorSync:
                buffer = new byte[12];
                Array.Copy(fullSector, 0, buffer, 0, 12);

                break;
            case SectorTagType.CdSectorHeader:
                buffer = new byte[4];
                Array.Copy(fullSector, 12, buffer, 0, 4);

                break;
            case SectorTagType.CdSectorEdc:
                buffer = new byte[4];
                Array.Copy(fullSector, 2064, buffer, 0, 4);

                break;
            case SectorTagType.CdSectorEccP:
                buffer = new byte[172];
                Array.Copy(fullSector, 2076, buffer, 0, 172);

                break;
            case SectorTagType.CdSectorEccQ:
                buffer = new byte[104];
                Array.Copy(fullSector, 2248, buffer, 0, 104);

                break;
            case SectorTagType.CdSectorEcc:
                buffer = new byte[276];
                Array.Copy(fullSector, 2076, buffer, 0, 276);

                break;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.MediaType != MediaType.CD) return ErrorNumber.NotSupported;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        if(tag == SectorTagType.CdTrackFlags) return ReadSectorTag(sectorAddress, track, tag, out buffer);

        using var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSectorTag(sectorAddress + i, track, tag, out byte[] tagData);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(tagData, 0, tagData.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer) =>
        ReadSectorTag(sectorAddress, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer) => ReadSectorsTag(sectorAddress, length, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) => session.Sequence == 1 ? Tracks : null;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => session == 1 ? Tracks : null;

#endregion
}