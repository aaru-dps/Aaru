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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public sealed partial class CdrWin
    {
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
        public ImageInfo       Info       => _imageInfo;
        public string          Name       => "CDRWin cuesheet";
        public Guid            Id         => new Guid("664568B2-15D4-4E64-8A7A-20BDA8B8386F");
        public string          Format     => "CDRWin CUESheet";
        public string          Author     => "Natalia Portillo";
        public List<Partition> Partitions { get; private set; }

        public List<Track> Tracks
        {
            get
            {
                List<Track> tracks = new List<Track>();

                ulong  previousStartSector = 0;
                ulong  gdRomSession2Offset = 45000;
                string previousTrackFile   = "";

                foreach(CdrWinTrack cdrTrack in _discImage.Tracks)
                {
                    var aaruTrack = new Track
                    {
                        TrackDescription       = cdrTrack.Title,
                        TrackPregap            = (ulong)cdrTrack.Pregap,
                        TrackSession           = cdrTrack.Session,
                        TrackSequence          = cdrTrack.Sequence,
                        TrackType              = CdrWinTrackTypeToTrackType(cdrTrack.TrackType),
                        TrackFile              = cdrTrack.TrackFile.DataFilter.GetFilename(),
                        TrackFilter            = cdrTrack.TrackFile.DataFilter,
                        TrackFileOffset        = cdrTrack.TrackFile.Offset,
                        TrackFileType          = cdrTrack.TrackFile.FileType,
                        TrackRawBytesPerSector = cdrTrack.Bps,
                        TrackBytesPerSector    = CdrWinTrackTypeToCookedBytesPerSector(cdrTrack.TrackType)
                    };

                    if(aaruTrack.TrackSequence == 1)
                    {
                        aaruTrack.TrackPregap = 150;
                        aaruTrack.Indexes[0]  = -150;
                    }

                    if(previousTrackFile == aaruTrack.TrackFile ||
                       previousTrackFile == "")
                        if(cdrTrack.Indexes.TryGetValue(0, out int idx0))
                            if(idx0 > 0)
                                aaruTrack.TrackStartSector = (ulong)idx0;
                            else
                                aaruTrack.TrackStartSector = 0;
                        else if(cdrTrack.Indexes.TryGetValue(1, out int idx1))
                            aaruTrack.TrackStartSector = (ulong)idx1;
                        else
                            aaruTrack.TrackStartSector += previousStartSector;
                    else
                        aaruTrack.TrackStartSector += previousStartSector;

                    foreach((ushort index, int position) in cdrTrack.Indexes)
                        aaruTrack.Indexes[index] = position;

                    if(_discImage.IsRedumpGigadisc &&
                       cdrTrack.Session    == 2    &&
                       previousStartSector < gdRomSession2Offset)
                        aaruTrack.TrackStartSector = gdRomSession2Offset;

                    previousTrackFile = cdrTrack.TrackFile.DataFilter.GetFilename();

                    aaruTrack.TrackEndSector = aaruTrack.TrackStartSector + cdrTrack.Sectors - 1;

                    if(cdrTrack.TrackType == CDRWIN_TRACK_TYPE_CDG)
                    {
                        aaruTrack.TrackSubchannelFilter = cdrTrack.TrackFile.DataFilter;
                        aaruTrack.TrackSubchannelFile   = cdrTrack.TrackFile.DataFilter.GetFilename();
                        aaruTrack.TrackSubchannelOffset = cdrTrack.TrackFile.Offset;
                        aaruTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;
                    }
                    else
                        aaruTrack.TrackSubchannelType = TrackSubchannelType.None;

                    if(_imageInfo.MediaType != MediaType.CD            &&
                       _imageInfo.MediaType != MediaType.CDDA          &&
                       _imageInfo.MediaType != MediaType.CDG           &&
                       _imageInfo.MediaType != MediaType.CDEG          &&
                       _imageInfo.MediaType != MediaType.CDI           &&
                       _imageInfo.MediaType != MediaType.CDROM         &&
                       _imageInfo.MediaType != MediaType.CDROMXA       &&
                       _imageInfo.MediaType != MediaType.CDPLUS        &&
                       _imageInfo.MediaType != MediaType.CDMO          &&
                       _imageInfo.MediaType != MediaType.CDR           &&
                       _imageInfo.MediaType != MediaType.CDRW          &&
                       _imageInfo.MediaType != MediaType.CDMRW         &&
                       _imageInfo.MediaType != MediaType.VCD           &&
                       _imageInfo.MediaType != MediaType.SVCD          &&
                       _imageInfo.MediaType != MediaType.PCD           &&
                       _imageInfo.MediaType != MediaType.DTSCD         &&
                       _imageInfo.MediaType != MediaType.CDMIDI        &&
                       _imageInfo.MediaType != MediaType.CDV           &&
                       _imageInfo.MediaType != MediaType.CDIREADY      &&
                       _imageInfo.MediaType != MediaType.FMTOWNS       &&
                       _imageInfo.MediaType != MediaType.PS1CD         &&
                       _imageInfo.MediaType != MediaType.PS2CD         &&
                       _imageInfo.MediaType != MediaType.MEGACD        &&
                       _imageInfo.MediaType != MediaType.SATURNCD      &&
                       _imageInfo.MediaType != MediaType.GDROM         &&
                       _imageInfo.MediaType != MediaType.GDR           &&
                       _imageInfo.MediaType != MediaType.MilCD         &&
                       _imageInfo.MediaType != MediaType.SuperCDROM2   &&
                       _imageInfo.MediaType != MediaType.JaguarCD      &&
                       _imageInfo.MediaType != MediaType.ThreeDO       &&
                       _imageInfo.MediaType != MediaType.PCFX          &&
                       _imageInfo.MediaType != MediaType.NeoGeoCD      &&
                       _imageInfo.MediaType != MediaType.CDTV          &&
                       _imageInfo.MediaType != MediaType.CD32          &&
                       _imageInfo.MediaType != MediaType.Playdia       &&
                       _imageInfo.MediaType != MediaType.Pippin        &&
                       _imageInfo.MediaType != MediaType.VideoNow      &&
                       _imageInfo.MediaType != MediaType.VideoNowColor &&
                       _imageInfo.MediaType != MediaType.VideoNowXp    &&
                       _imageInfo.MediaType != MediaType.CVD)
                    {
                        aaruTrack.TrackPregap = 0;
                        aaruTrack.Indexes?.Clear();
                    }

                    tracks.Add(aaruTrack);
                    previousStartSector = aaruTrack.TrackEndSector + 1;
                }

                return tracks;
            }
        }

        public List<Session>          Sessions     => _discImage.Sessions;
        public List<DumpHardwareType> DumpHardware { get; private set; }
        public CICMMetadataType       CicmMetadata => null;
        public IEnumerable<MediaTagType> SupportedMediaTags => new[]
        {
            MediaTagType.CD_MCN, MediaTagType.CD_TEXT
        };
        public IEnumerable<SectorTagType> SupportedSectorTags => new[]
        {
            SectorTagType.CdSectorEcc, SectorTagType.CdSectorEccP, SectorTagType.CdSectorEccQ,
            SectorTagType.CdSectorEdc, SectorTagType.CdSectorHeader, SectorTagType.CdSectorSubHeader,
            SectorTagType.CdSectorSync, SectorTagType.CdTrackFlags, SectorTagType.CdTrackIsrc
        };
        public IEnumerable<MediaType> SupportedMediaTypes => new[]
        {
            MediaType.BDR, MediaType.BDRE, MediaType.BDREXL, MediaType.BDROM, MediaType.UHDBD, MediaType.BDRXL,
            MediaType.CBHD, MediaType.CD, MediaType.CDDA, MediaType.CDEG, MediaType.CDG, MediaType.CDI,
            MediaType.CDMIDI, MediaType.CDMRW, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROMXA,
            MediaType.CDRW, MediaType.CDV, MediaType.DDCD, MediaType.DDCDR, MediaType.DDCDRW, MediaType.DVDDownload,
            MediaType.DVDPR, MediaType.DVDPRDL, MediaType.DVDPRW, MediaType.DVDPRWDL, MediaType.DVDR, MediaType.DVDRAM,
            MediaType.DVDRDL, MediaType.DVDROM, MediaType.DVDRW, MediaType.DVDRWDL, MediaType.EVD, MediaType.FDDVD,
            MediaType.DTSCD, MediaType.FVD, MediaType.HDDVDR, MediaType.HDDVDRAM, MediaType.HDDVDRDL,
            MediaType.HDDVDROM, MediaType.HDDVDRW, MediaType.HDDVDRWDL, MediaType.HDVMD, MediaType.HVD,
            MediaType.JaguarCD, MediaType.MEGACD, MediaType.PS1CD, MediaType.PS2CD, MediaType.PS2DVD, MediaType.PS3BD,
            MediaType.PS3DVD, MediaType.PS4BD, MediaType.PS5BD, MediaType.SuperCDROM2, MediaType.SVCD, MediaType.SVOD,
            MediaType.SATURNCD, MediaType.ThreeDO, MediaType.UDO, MediaType.UDO2, MediaType.UDO2_WORM, MediaType.UMD,
            MediaType.VCD, MediaType.VCDHD, MediaType.NeoGeoCD, MediaType.PCFX, MediaType.CDTV, MediaType.CD32,
            MediaType.Nuon, MediaType.Playdia, MediaType.Pippin, MediaType.FMTOWNS, MediaType.MilCD, MediaType.VideoNow,
            MediaType.VideoNowColor, MediaType.VideoNowXp, MediaType.CVD, MediaType.PCD
        };
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions => new[]
        {
            ("separate", typeof(bool), "Write each track to a separate file.", (object)false)
        };
        public IEnumerable<string> KnownExtensions => new[]
        {
            ".cue"
        };
        public bool   IsWriting    { get; private set; }
        public string ErrorMessage { get; private set; }
    }
}