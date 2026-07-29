// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Redumper raw DVD (.sdram + .state) and Blu-ray (.sbram + .state) images.
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
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class Redumper
{
    #region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        string path = imageFilter.Path;
        _ngcwRegularDataSectors = 0;

        if (string.IsNullOrEmpty(path)) return ErrorNumber.InvalidArgument;

        string basePath = path[..^".state".Length];
        string sdramPath = basePath + ".sdram";
        string sbramPath = basePath + ".sbram";
        long stateLength = imageFilter.DataForkLength;

        bool sdramOk = File.Exists(sdramPath) &&
                       new FileInfo(sdramPath).Length % RECORDING_FRAME_SIZE == 0 &&
                       stateLength == new FileInfo(sdramPath).Length / RECORDING_FRAME_SIZE;

        bool sbramOk = File.Exists(sbramPath) &&
                       new FileInfo(sbramPath).Length % BD_DATA_FRAME_SIZE == 0 &&
                       stateLength == new FileInfo(sbramPath).Length / BD_DATA_FRAME_SIZE;

        // Fall back to DVD if both are valid
        if (sdramOk && sbramOk)
        {
            AaruLogging.Debug(MODULE_NAME, "Both SDRAM and SBRAM are found, falling back to DVD");
            return OpenDvd(imageFilter, basePath, sdramPath);
        }

        if (sdramOk)
        {
            AaruLogging.Debug(MODULE_NAME, "SDRAM is found, opening as DVD");
            return OpenDvd(imageFilter, basePath, sdramPath);
        }

        if (sbramOk)
        {
            AaruLogging.Debug(MODULE_NAME, "SBRAM is found, opening as Blu-ray");
            return OpenBd(imageFilter, basePath, sbramPath);
        }

        return ErrorNumber.NoSuchFile;
    }


    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        if (!_mediaTags.TryGetValue(tag, out byte[] data)) return ErrorNumber.NoData;

        buffer = new byte[data.Length];
        Array.Copy(data, buffer, data.Length);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        if (_isBluRay) return ReadSectorForBd(sectorAddress, negative, out buffer, out sectorStatus);

        return ReadSectorForDvd(sectorAddress, negative, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer = null;
        sectorStatus = null;

        buffer = new byte[length * USER_DATA_SIZE];
        sectorStatus = new SectorStatus[length];

        for (uint i = 0; i < length; i++)
        {
            ulong addr = negative ? sectorAddress - i : sectorAddress + i;
            ErrorNumber errno = ReadSector(addr, negative, out byte[] sector, out SectorStatus status);

            if (errno != ErrorNumber.NoError) return errno;

            Array.Copy(sector, 0, buffer, i * USER_DATA_SIZE, USER_DATA_SIZE);
            sectorStatus[i] = status;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        if (_isBluRay) return ReadSectorLongForBd(sectorAddress, negative, out buffer, out sectorStatus);

        return ReadSectorLongForNgcw(sectorAddress, negative, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer = null;
        sectorStatus = null;

        int longSize = _isBluRay ? BD_DATA_FRAME_SIZE : DVD_SECTOR_SIZE;

        buffer = new byte[length * (uint)longSize];
        sectorStatus = new SectorStatus[length];

        for (uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSectorLong(sectorAddress + i, negative, out byte[] sector, out SectorStatus status);

            if (errno != ErrorNumber.NoError) return errno;

            Array.Copy(sector, 0, buffer, i * longSize, longSize);
            sectorStatus[i] = status;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ReadSectorsTag(sectorAddress, negative, 1, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if (_isBluRay) return ReadSectorsTagForBd(sectorAddress, negative, length, tag, out buffer);

        return ReadSectorsTagForDvd(sectorAddress, negative, length, tag, out buffer);
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
        ReadSector(sectorAddress, false, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer,
                                      out SectorStatus sectorStatus) =>
        ReadSectorLong(sectorAddress, false, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, uint track, out byte[] buffer,
                                   out SectorStatus[] sectorStatus) =>
        ReadSectors(sectorAddress, false, length, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, uint track, out byte[] buffer,
                                       out SectorStatus[] sectorStatus) =>
        ReadSectorsLong(sectorAddress, false, length, out buffer, out sectorStatus);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer) =>
        ReadSectorTag(sectorAddress, false, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, false, length, tag, out buffer);

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) => Tracks;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => Tracks;

    #endregion

    /// <summary>Maps a Redumper state byte to an Aaru SectorStatus.</summary>
    static SectorStatus MapState(byte state) =>
        state switch
        {
            0 => SectorStatus.NotDumped,  // ERROR_SKIP
            1 => SectorStatus.Errored,    // ERROR_C2
            _ => SectorStatus.Dumped      // SUCCESS_C2_OFF (2), SUCCESS_SCSI_OFF (3), SUCCESS (4)
        };

    /// <summary>
    ///     Loads a SCSI READ DISC/DVD STRUCTURE response sidecar, strips the 4-byte parameter list header,
    ///     and stores the payload as a media tag.
    /// </summary>
    void LoadScsiSidecar(string basePath, string primarySuffix, string fallbackSuffix, MediaTagType tag)
    {
        string path = basePath + primarySuffix;

        if (!File.Exists(path))
        {
            if (fallbackSuffix is null) return;

            path = basePath + fallbackSuffix;

            if (!File.Exists(path)) return;
        }

        byte[] data = File.ReadAllBytes(path);

        if (data.Length <= SCSI_HEADER_SIZE) return;

        byte[] payload = new byte[data.Length - SCSI_HEADER_SIZE];
        Array.Copy(data, SCSI_HEADER_SIZE, payload, 0, payload.Length);

        _mediaTags[tag] = payload;
        AaruLogging.Debug(MODULE_NAME, Localization.Found_media_tag_0, tag);
    }
}
