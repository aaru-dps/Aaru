// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Expert Witness Format disk images.
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
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.Compression;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Images;

public sealed partial class Ewf
{
    /// <summary>Reads and decompresses a chunk, using cache when possible.</summary>
    ErrorNumber ReadChunk(ulong chunkIndex, out byte[] chunkData)
    {
        chunkData = null;

        // Check cache
        if(_chunkCache.TryGetValue(chunkIndex, out chunkData)) return ErrorNumber.NoError;

        if(!_chunkTable.TryGetValue(chunkIndex, out (int segmentIndex, long offset, uint size, bool compressed) chunk))
        {
            AaruLogging.Error(MODULE_NAME, "Chunk {0} not found in chunk table", chunkIndex);

            return ErrorNumber.SectorNotFound;
        }

        Stream segStream = _segmentStreams[chunk.segmentIndex];
        segStream.Seek(chunk.offset, SeekOrigin.Begin);

        if(chunk.compressed)
        {
            var compressedData = new byte[chunk.size];
            segStream.EnsureRead(compressedData, 0, (int)chunk.size);

            if(_isV2 && _compressionMethod == EwfCompressionMethod.Bzip2)
            {
                // BZip2 decompression
                chunkData = new byte[_chunkSize];
                BZip2.DecodeBuffer(compressedData, chunkData);
            }
            else
            {
                // Zlib/Deflate decompression (default for v1, and deflate method for v2)
                chunkData = DecompressZlib(compressedData, (int)_chunkSize);
            }
        }
        else
        {
            // Uncompressed chunk: data + 4-byte Adler-32 checksum
            uint dataLen = chunk.size >= 4 ? chunk.size - 4 : chunk.size;
            chunkData = new byte[dataLen];
            segStream.EnsureRead(chunkData, 0, (int)dataLen);

            // Skip checksum (we trust the data for now)
            if(chunk.size >= 4) segStream.Seek(4, SeekOrigin.Current);
        }

        // Cache chunk
        if(_chunkCache.Count >= _maxChunkCache) _chunkCache.Clear();

        _chunkCache[chunkIndex] = chunkData;

        return ErrorNumber.NoError;
    }

    /// <summary>Determines the sector status by checking against the bad sectors list.</summary>
    SectorStatus GetSectorStatus(ulong sectorAddress)
    {
        foreach((ulong start, uint count) in _badSectors)
        {
            if(sectorAddress >= start && sectorAddress < start + count) return SectorStatus.Errored;
        }

        return SectorStatus.Dumped;
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
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(sectorAddress >= _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        // Check sector cache
        if(_sectorCache.TryGetValue(sectorAddress, out buffer))
        {
            sectorStatus = GetSectorStatus(sectorAddress);

            return ErrorNumber.NoError;
        }

        // Calculate which chunk contains this sector
        ulong chunkIndex = sectorAddress / _sectorsPerChunk;

        // Read and decompress the chunk
        ErrorNumber errno = ReadChunk(chunkIndex, out byte[] chunkData);

        if(errno != ErrorNumber.NoError) return errno;

        // Extract the requested sector from the chunk
        ulong sectorInChunk = sectorAddress % _sectorsPerChunk;
        var   sectorOffset  = (long)(sectorInChunk * _bytesPerSector);

        // Handle last chunk which may be smaller
        if(sectorOffset + _bytesPerSector > chunkData.Length) return ErrorNumber.OutOfRange;

        buffer = new byte[_bytesPerSector];
        Array.Copy(chunkData, sectorOffset, buffer, 0, _bytesPerSector);

        sectorStatus = GetSectorStatus(sectorAddress);

        // Cache sector
        if(_sectorCache.Count >= MAX_CACHED_SECTORS) _sectorCache.Clear();

        _sectorCache[sectorAddress] = buffer;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        buffer       = new byte[length * _bytesPerSector];
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, negative, out byte[] sector, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            Array.Copy(sector, 0, buffer, i * _bytesPerSector, _bytesPerSector);
            sectorStatus[i] = status;
        }

        return ErrorNumber.NoError;
    }

#endregion
}