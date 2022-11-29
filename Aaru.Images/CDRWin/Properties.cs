// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for CDRWin cuesheets (cue/bin).
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages;

public sealed partial class CdrWin
{
    /// <inheritdoc />
    public OpticalImageCapabilities OpticalCapabilities => OpticalImageCapabilities.CanStoreAudioTracks    |
                                                           OpticalImageCapabilities.CanStoreDataTracks     |
                                                           OpticalImageCapabilities.CanStorePregaps        |
                                                           OpticalImageCapabilities.CanStoreSessions       |
                                                           OpticalImageCapabilities.CanStoreIsrc           |
                                                           OpticalImageCapabilities.CanStoreCdText         |
                                                           OpticalImageCapabilities.CanStoreMcn            |
                                                           OpticalImageCapabilities.CanStoreRawData        |
                                                           OpticalImageCapabilities.CanStoreCookedData     |
                                                           OpticalImageCapabilities.CanStoreMultipleTracks |
                                                           OpticalImageCapabilities.CanStoreNotCdSessions  |
                                                           OpticalImageCapabilities.CanStoreNotCdTracks    |
                                                           OpticalImageCapabilities.CanStoreIndexes;
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;
    /// <inheritdoc />
    public string Name => Localization.CdrWin_Name;
    /// <inheritdoc />
    public Guid Id => new("664568B2-15D4-4E64-8A7A-20BDA8B8386F");
    /// <inheritdoc />
    public string Format => "CDRWin CUESheet";
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
    /// <inheritdoc />
    public List<Partition> Partitions { get; private set; }

    /// <inheritdoc />
    public List<Track> Tracks
    {
        get
        {
            List<Track> tracks = new();

            ulong       previousStartSector = 0;
            const ulong gdRomSession2Offset = 45000;
            string      previousTrackFile   = "";

            foreach(CdrWinTrack cdrTrack in _discImage.Tracks)
            {
                var aaruTrack = new Track
                {
                    Description       = cdrTrack.Title,
                    Pregap            = (ulong)cdrTrack.Pregap,
                    Session           = cdrTrack.Session,
                    Sequence          = cdrTrack.Sequence,
                    Type              = _isCd ? CdrWinTrackTypeToTrackType(cdrTrack.TrackType) : TrackType.Data,
                    File              = cdrTrack.TrackFile.DataFilter.Filename,
                    Filter            = cdrTrack.TrackFile.DataFilter,
                    FileOffset        = cdrTrack.TrackFile.Offset,
                    FileType          = cdrTrack.TrackFile.FileType,
                    RawBytesPerSector = cdrTrack.Bps,
                    BytesPerSector    = CdrWinTrackTypeToCookedBytesPerSector(cdrTrack.TrackType)
                };

                if(aaruTrack.Sequence == 1 && _isCd)
                {
                    aaruTrack.Pregap     = 150;
                    aaruTrack.Indexes[0] = -150;
                }

                if(previousTrackFile == aaruTrack.File ||
                   previousTrackFile == "")
                    if(cdrTrack.Indexes.TryGetValue(0, out int idx0))
                        if(idx0 > 0)
                            aaruTrack.StartSector = (ulong)idx0;
                        else
                            aaruTrack.StartSector = 0;
                    else if(cdrTrack.Indexes.TryGetValue(1, out int idx1))
                        aaruTrack.StartSector = (ulong)idx1;
                    else
                        aaruTrack.StartSector += previousStartSector;
                else
                    aaruTrack.StartSector += previousStartSector;

                foreach((ushort index, int position) in cdrTrack.Indexes)
                    aaruTrack.Indexes[index] = position;

                previousTrackFile = cdrTrack.TrackFile.DataFilter.Filename;

                aaruTrack.EndSector = aaruTrack.StartSector + cdrTrack.Sectors - 1;

                if(_discImage.IsRedumpGigadisc &&
                   cdrTrack.Session    == 2    &&
                   previousStartSector < gdRomSession2Offset)
                {
                    aaruTrack.StartSector = (ulong)cdrTrack.Indexes[0];
                    aaruTrack.EndSector   = gdRomSession2Offset + cdrTrack.Sectors - (uint)cdrTrack.Pregap - 1;
                }

                if(cdrTrack.TrackType == CDRWIN_TRACK_TYPE_CDG)
                {
                    aaruTrack.SubchannelFilter = cdrTrack.TrackFile.DataFilter;
                    aaruTrack.SubchannelFile   = cdrTrack.TrackFile.DataFilter.Filename;
                    aaruTrack.SubchannelOffset = cdrTrack.TrackFile.Offset;
                    aaruTrack.SubchannelType   = TrackSubchannelType.RawInterleaved;
                }
                else
                    aaruTrack.SubchannelType = TrackSubchannelType.None;

                if(!_isCd)
                {
                    aaruTrack.Pregap = 0;
                    aaruTrack.Indexes?.Clear();
                }

                tracks.Add(aaruTrack);
                previousStartSector = aaruTrack.EndSector + 1;
            }

            return tracks;
        }
    }

    /// <inheritdoc />
    public List<Session> Sessions => _discImage.Sessions;
    /// <inheritdoc />
    public List<DumpHardwareType> DumpHardware { get; private set; }
    /// <inheritdoc />
    public CICMMetadataType CicmMetadata => null;
    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => new[]
    {
        MediaTagType.CD_MCN, MediaTagType.CD_TEXT
    };
    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => new[]
    {
        SectorTagType.CdSectorEcc, SectorTagType.CdSectorEccP, SectorTagType.CdSectorEccQ, SectorTagType.CdSectorEdc,
        SectorTagType.CdSectorHeader, SectorTagType.CdSectorSubHeader, SectorTagType.CdSectorSync,
        SectorTagType.CdTrackFlags, SectorTagType.CdTrackIsrc
    };
    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.BDR, MediaType.BDRE, MediaType.BDREXL, MediaType.BDROM, MediaType.UHDBD, MediaType.BDRXL,
        MediaType.CBHD, MediaType.CD, MediaType.CDDA, MediaType.CDEG, MediaType.CDG, MediaType.CDI, MediaType.CDMIDI,
        MediaType.CDMRW, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROMXA, MediaType.CDRW,
        MediaType.CDV, MediaType.DDCD, MediaType.DDCDR, MediaType.DDCDRW, MediaType.DVDDownload, MediaType.DVDPR,
        MediaType.DVDPRDL, MediaType.DVDPRW, MediaType.DVDPRWDL, MediaType.DVDR, MediaType.DVDRAM, MediaType.DVDRDL,
        MediaType.DVDROM, MediaType.DVDRW, MediaType.DVDRWDL, MediaType.EVD, MediaType.FDDVD, MediaType.DTSCD,
        MediaType.FVD, MediaType.HDDVDR, MediaType.HDDVDRAM, MediaType.HDDVDRDL, MediaType.HDDVDROM, MediaType.HDDVDRW,
        MediaType.HDDVDRWDL, MediaType.HDVMD, MediaType.HVD, MediaType.JaguarCD, MediaType.MEGACD, MediaType.PS1CD,
        MediaType.PS2CD, MediaType.PS2DVD, MediaType.PS3BD, MediaType.PS3DVD, MediaType.PS4BD, MediaType.PS5BD,
        MediaType.SuperCDROM2, MediaType.SVCD, MediaType.SVOD, MediaType.SATURNCD, MediaType.ThreeDO, MediaType.UDO,
        MediaType.UDO2, MediaType.UDO2_WORM, MediaType.UMD, MediaType.VCD, MediaType.VCDHD, MediaType.NeoGeoCD,
        MediaType.PCFX, MediaType.CDTV, MediaType.CD32, MediaType.Nuon, MediaType.Playdia, MediaType.Pippin,
        MediaType.FMTOWNS, MediaType.MilCD, MediaType.VideoNow, MediaType.VideoNowColor, MediaType.VideoNowXp,
        MediaType.CVD, MediaType.PCD
    };
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions => new[]
    {
        ("separate", typeof(bool), Localization.Write_each_track_to_a_separate_file, (object)false)
    };
    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".cue"
    };
    /// <inheritdoc />
    public bool IsWriting { get; private set; }
    /// <inheritdoc />
    public string ErrorMessage { get; private set; }
}