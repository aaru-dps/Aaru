// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Bluray.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Redumper raw Blu-ray (.sbram + .state) images.
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
// Copyright © 2011-2026 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.Bluray;
using Aaru.Helpers;
using Aaru.Logging;
using Humanizer;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Images;

public sealed partial class Redumper
{
    ErrorNumber OpenBd(IFilter imageFilter, string basePath, string sbramPath)
    {
        _isBluRay = true;
        _lbaStart = BD_LBA_START;
        _bdNintendo = false;

        long stateLength = imageFilter.DataForkLength;
        long sbramLength = new FileInfo(sbramPath).Length;

        if (sbramLength % BD_DATA_FRAME_SIZE != 0) return ErrorNumber.InvalidArgument;

        _totalFrames = sbramLength / BD_DATA_FRAME_SIZE;

        if (stateLength != _totalFrames) return ErrorNumber.InvalidArgument;

        _imageFilter = imageFilter;

        Stream stateStream = imageFilter.GetDataForkStream();
        _stateData = new byte[stateLength];
        stateStream.Seek(0, SeekOrigin.Begin);
        stateStream.EnsureRead(_stateData, 0, (int)stateLength);

        _ramFilter = PluginRegister.Singleton.GetFilter(sbramPath);

        if (_ramFilter is null) return ErrorNumber.NoSuchFile;

        _ramPath = sbramPath;

        long negativeLbaCount = Math.Min(-(long)_lbaStart, _totalFrames);
        long positiveLbaCount = Math.Max(0, _totalFrames + _lbaStart);

        _imageInfo.NegativeSectors = (uint)negativeLbaCount;
        _imageInfo.Sectors = (ulong)positiveLbaCount;
        _imageInfo.SectorSize = USER_DATA_SIZE;
        _imageInfo.ImageSize = _imageInfo.Sectors * USER_DATA_SIZE;

        _imageInfo.CreationTime = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.MetadataMediaType = MetadataMediaType.OpticalDisc;
        _imageInfo.HasPartitions = true;
        _imageInfo.HasSessions = true;
        _imageInfo.MediaType = MediaType.BDROM;

        _mediaTags = new Dictionary<MediaTagType, byte[]>();
        LoadBdMediaTagSidecars(basePath);

        _imageInfo.ReadableMediaTags = [.. _mediaTags.Keys];

        _imageInfo.ReadableSectorTags = [SectorTagType.BluRaySectorEdc];

        Tracks =
        [
            new Track
            {
                Sequence          = 1,
                Session           = 1,
                Type              = TrackType.Data,
                StartSector       = 0,
                EndSector         = _imageInfo.Sectors > 0 ? _imageInfo.Sectors - 1 : 0,
                Pregap            = 0,
                FileType          = "BINARY",
                Filter            = _ramFilter,
                File              = sbramPath,
                BytesPerSector    = USER_DATA_SIZE,
                RawBytesPerSector = BD_DATA_FRAME_SIZE
            }
        ];

        Sessions =
        [
            new Session
            {
                Sequence    = 1,
                StartSector = 0,
                EndSector   = _imageInfo.Sectors > 0 ? _imageInfo.Sectors - 1 : 0,
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
                Name     = string.Format(Localization.Track_0, 1),
                Type     = Tracks[0].Type.Humanize(),
                Scheme   = Localization.Optical_disc_track
            }
        ];

        return ErrorNumber.NoError;
    }

    ErrorNumber ReadSectorForBd(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer = new byte[USER_DATA_SIZE];
        ErrorNumber errno = ReadSectorLongForBd(sectorAddress, negative, out byte[] longBuf, out sectorStatus);

        if (errno != ErrorNumber.NoError) return errno;

        Array.Copy(longBuf, 0, buffer, 0, USER_DATA_SIZE);

        return ErrorNumber.NoError;
    }

    ErrorNumber ReadSectorLongForBd(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer = null;
        sectorStatus = SectorStatus.NotDumped;

        int lba = negative ? -(int)sectorAddress : (int)sectorAddress;

        long frameIndex = (long)lba - _lbaStart;

        if (frameIndex < 0 || frameIndex >= _totalFrames) return ErrorNumber.OutOfRange;

        sectorStatus = MapState(_stateData[frameIndex]);

        byte[] frame = ReadBdFrame(frameIndex);

        if (frame is null) return ErrorNumber.InvalidArgument;

        if (sectorStatus != SectorStatus.Dumped)
        {
            buffer = frame;

            return ErrorNumber.NoError;
        }

        byte[] work = new byte[BD_DATA_FRAME_SIZE];
        Array.Copy(frame, 0, work, 0, BD_DATA_FRAME_SIZE);
        DataFrame.Descramble(work, lba, _bdNintendo);
        buffer = work;

        return ErrorNumber.NoError;
    }


    /// <summary>Reads one 2052-byte BD DataFrame from .sbram.</summary>
    byte[] ReadBdFrame(long frameIndex)
    {
        Stream stream = _ramFilter.GetDataForkStream();
        long offset = frameIndex * BD_DATA_FRAME_SIZE;

        if (offset + BD_DATA_FRAME_SIZE > stream.Length) return null;

        var frame = new byte[BD_DATA_FRAME_SIZE];
        stream.Seek(offset, SeekOrigin.Begin);
        stream.EnsureRead(frame, 0, BD_DATA_FRAME_SIZE);

        return frame;
    }

    ErrorNumber ReadSectorsTagForBd(ulong sectorAddress, bool negative, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if (tag != SectorTagType.BluRaySectorEdc) return ErrorNumber.NotSupported;

        buffer = new byte[4 * length];

        for (uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSectorLong(sectorAddress + i, negative, out byte[] sector, out _);

            if (errno != ErrorNumber.NoError) return errno;

            Array.Copy(sector, 2048, buffer, i * 4, 4);
        }

        return ErrorNumber.NoError;
    }


    /// <summary>Loads BD sidecars: BCA (SCSI strip, same as device dump), Nintendo heuristic from .physical.</summary>
    void LoadBdMediaTagSidecars(string basePath)
    {
        TryBdNintendoFromPhysicalSidecar(basePath);

        LoadScsiSidecar(basePath, ".physical", null, MediaTagType.BD_DI);

        LoadScsiSidecar(basePath, ".bca", null, MediaTagType.BD_BCA);
    }

    /// <summary>
    ///     If a single-layer style .physical sidecar exists and the payload (after SCSI header strip) is all zero,
    ///     treat the dump as Nintendo BD scrambling (redumper bd_extract_iso / dvd_dump).
    /// </summary>
    void TryBdNintendoFromPhysicalSidecar(string basePath)
    {
        string path = basePath + ".physical";

        if (!File.Exists(path)) return;

        byte[] data = File.ReadAllBytes(path);

        if (data.Length <= SCSI_HEADER_SIZE) return;

        byte[] payload = new byte[data.Length - SCSI_HEADER_SIZE];
        Array.Copy(data, SCSI_HEADER_SIZE, payload, 0, payload.Length);

        if (payload.Length > 0 && payload.All(b => b == 0)) _bdNintendo = true;
    }
}