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
//     Reads Nintendo Wii U compressed disc images (WUX format).
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
using Aaru.Helpers;
using Aaru.Logging;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class Wux
{
#region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        if(stream.Length < WUX_HEADER_SIZE) return ErrorNumber.InvalidArgument;

        stream.Seek(0, SeekOrigin.Begin);

        var headerBytes = new byte[WUX_HEADER_SIZE];
        stream.EnsureRead(headerBytes, 0, (int)WUX_HEADER_SIZE);

        WuxHeader header = Marshal.SpanToStructureLittleEndian<WuxHeader>(headerBytes);

        if(header.Magic != WUX_MAGIC) return ErrorNumber.InvalidArgument;

        if(header.SectorSize != WIIU_PHYSICAL_SECTOR) return ErrorNumber.InvalidArgument;

        _sectorCount = (uint)(header.UncompressedSize / WIIU_PHYSICAL_SECTOR);

        AaruLogging.Debug(MODULE_NAME, "WUX uncompressed size: {0} bytes", header.UncompressedSize);
        AaruLogging.Debug(MODULE_NAME, "WUX physical sector count: {0}",   _sectorCount);

        // Read sector index table
        var indexBytes = new byte[_sectorCount * 4];
        stream.EnsureRead(indexBytes, 0, indexBytes.Length);

        _sectorIndex = new uint[_sectorCount];

        for(uint i = 0; i < _sectorCount; i++) _sectorIndex[i] = BitConverter.ToUInt32(indexBytes, (int)(i * 4));

        // Data starts at next sector-aligned offset after header + index table
        ulong rawDataOffset = WUX_HEADER_SIZE + (ulong)indexBytes.Length;
        _dataOffset = rawDataOffset           + WIIU_PHYSICAL_SECTOR - 1 & ~((ulong)WIIU_PHYSICAL_SECTOR - 1);

        AaruLogging.Debug(MODULE_NAME, "WUX data offset: 0x{0:X}", _dataOffset);

        _imageFilter = imageFilter;

        _imageInfo.MediaType            = MediaType.WUOD;
        _imageInfo.SectorSize           = WIIU_LOGICAL_SECTOR;
        _imageInfo.Sectors              = (ulong)_sectorCount * LOGICAL_PER_PHYSICAL;
        _imageInfo.ImageSize            = header.UncompressedSize;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;
        _imageInfo.HasPartitions        = true;
        _imageInfo.HasSessions          = true;

        // Load .key sidecar
        _mediaTags = new Dictionary<MediaTagType, byte[]>();
        string basename  = imageFilter.BasePath;
        string extension = Path.GetExtension(imageFilter.Filename)?.ToLower();
        basename = basename[..^(extension?.Length ?? basename.Length)];

        string keyPath = basename + ".key";

        if(File.Exists(keyPath))
        {
            byte[] keyData = File.ReadAllBytes(keyPath);

            if(keyData.Length == 16)
            {
                _mediaTags[MediaTagType.WiiUDiscKey] = keyData;
                AaruLogging.Debug(MODULE_NAME, "Found Wii U disc key sidecar");
            }
        }

        _imageInfo.ReadableMediaTags = [.._mediaTags.Keys];

        // Set up single track covering the entire disc
        Tracks =
        [
            new Track
            {
                Sequence    = 1,
                Session     = 1,
                Type        = TrackType.Data,
                StartSector = 0,
                EndSector   = _imageInfo.Sectors - 1,
                Pregap      = 0,
                FileType    = "BINARY",
                Filter      = imageFilter,
                File        = imageFilter.Filename
            }
        ];

        Sessions =
        [
            new Session
            {
                Sequence    = 1,
                StartSector = 0,
                EndSector   = _imageInfo.Sectors - 1,
                StartTrack  = 1,
                EndTrack    = 1
            }
        ];

        Partitions =
        [
            new Partition
            {
                Sequence = 0,
                Start    = 0,
                Length   = _imageInfo.Sectors,
                Size     = _imageInfo.Sectors * _imageInfo.SectorSize,
                Offset   = 0,
                Type     = "Nintendo Wii U Optical Disc"
            }
        ];

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(!_mediaTags.TryGetValue(tag, out byte[] data)) return ErrorNumber.NoData;

        buffer = new byte[data.Length];
        Array.Copy(data, buffer, data.Length);

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
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.InvalidArgument;

        if(sectorAddress >= _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        // Map logical sector to position within WUX
        var physicalSector = (uint)(sectorAddress / LOGICAL_PER_PHYSICAL);
        var sectorOffset   = (uint)(sectorAddress % LOGICAL_PER_PHYSICAL);

        if(physicalSector >= _sectorCount) return ErrorNumber.OutOfRange;

        uint mappedSector = _sectorIndex[physicalSector];

        ulong fileOffset = _dataOffset                                +
                           (ulong)mappedSector * WIIU_PHYSICAL_SECTOR +
                           (ulong)sectorOffset * WIIU_LOGICAL_SECTOR;

        buffer = new byte[WIIU_LOGICAL_SECTOR];

        Stream stream = _imageFilter.GetDataForkStream();
        stream.Seek((long)fileOffset, SeekOrigin.Begin);
        stream.EnsureRead(buffer, 0, (int)WIIU_LOGICAL_SECTOR);

        sectorStatus = SectorStatus.Dumped;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus) =>
        ReadSector(sectorAddress, negative, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.InvalidArgument;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        buffer       = new byte[length * WIIU_LOGICAL_SECTOR];
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, false, out byte[] sector, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            Array.Copy(sector, 0, buffer, i * WIIU_LOGICAL_SECTOR, WIIU_LOGICAL_SECTOR);
            sectorStatus[i] = status;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus) =>
        ReadSectors(sectorAddress, negative, length, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus) =>
        ReadSector(sectorAddress, false, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, uint track, out byte[] buffer,
                                      out SectorStatus sectorStatus) =>
        ReadSector(sectorAddress, false, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                   out SectorStatus[] sectorStatus) =>
        ReadSectors(sectorAddress, false, length, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                       out SectorStatus[] sectorStatus) =>
        ReadSectors(sectorAddress, false, length, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) => Tracks;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => Tracks;

#endregion
}