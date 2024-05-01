// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties Alcohol 120% disc images.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Images;

public sealed partial class Alcohol120
{
#region IWritableOpticalImage Members

    /// <inheritdoc />
    public OpticalImageCapabilities OpticalCapabilities => OpticalImageCapabilities.CanStoreAudioTracks  |
                                                           OpticalImageCapabilities.CanStoreDataTracks   |
                                                           OpticalImageCapabilities.CanStoreSubchannelRw |

                                                           // TODO: Disabled until 6.0
                                                           //OpticalImageCapabilities.CanStoreSessions     |
                                                           OpticalImageCapabilities.CanStoreIsrc       |
                                                           OpticalImageCapabilities.CanStoreCdText     |
                                                           OpticalImageCapabilities.CanStoreMcn        |
                                                           OpticalImageCapabilities.CanStoreRawData    |
                                                           OpticalImageCapabilities.CanStoreCookedData |
                                                           OpticalImageCapabilities.CanStoreMultipleTracks;

    /// <inheritdoc />

    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.Alcohol120_Name;

    /// <inheritdoc />
    public Guid Id => new("A78FBEBA-0307-4915-BDE3-B8A3B57F843F");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public string Format => "Alcohol 120% Media Descriptor Structure";

    /// <inheritdoc />
    public List<Partition> Partitions { get; private set; }

    /// <inheritdoc />
    public List<CommonTypes.Structs.Track> Tracks
    {
        get
        {
            if(_writingTracks != null && _alcTracks == null) return _writingTracks;

            List<CommonTypes.Structs.Track> tracks = new();
            _alcTracks ??= new Dictionary<int, Track>();

            foreach(Track alcTrack in _alcTracks.Values)
            {
                ushort sessionNo =
                    (from session in Sessions
                     where alcTrack.point >= session.StartTrack && alcTrack.point <= session.EndTrack
                     select session.Sequence).FirstOrDefault();

                if(!_alcTrackExtras.TryGetValue(alcTrack.point, out TrackExtra alcExtra)) continue;

                var aaruTrack = new CommonTypes.Structs.Track
                {
                    StartSector       = alcTrack.startLba,
                    EndSector         = alcTrack.startLba + alcExtra.sectors - 1,
                    Pregap            = alcExtra.pregap,
                    Session           = sessionNo,
                    Sequence          = alcTrack.point,
                    Type              = TrackModeToTrackType(alcTrack.mode),
                    Filter            = _alcImage,
                    File              = _alcImage.Filename,
                    FileOffset        = alcTrack.startOffset,
                    FileType          = "BINARY",
                    RawBytesPerSector = alcTrack.sectorSize,
                    BytesPerSector    = TrackModeToCookedBytesPerSector(alcTrack.mode)
                };

                if(alcExtra.pregap > 0) aaruTrack.Indexes.Add(0, (int)(alcTrack.startLba - alcExtra.pregap));

                aaruTrack.Indexes.Add(1, (int)alcTrack.startLba);

                if(aaruTrack.Indexes.ContainsKey(0) && aaruTrack.Indexes[0] >= 0)
                    aaruTrack.StartSector = (ulong)aaruTrack.Indexes[0];

                switch(alcTrack.subMode)
                {
                    case SubchannelMode.Interleaved:
                        aaruTrack.SubchannelFilter = _alcImage;
                        aaruTrack.SubchannelFile   = _alcImage.Filename;
                        aaruTrack.SubchannelOffset = alcTrack.startOffset;
                        aaruTrack.SubchannelType   = TrackSubchannelType.RawInterleaved;

                        break;
                    case SubchannelMode.None:
                        aaruTrack.SubchannelType = TrackSubchannelType.None;

                        break;
                }

                if(_header.type != MediumType.CD && _header.type != MediumType.CDR && _header.type != MediumType.CDRW)
                {
                    aaruTrack.Pregap = 0;
                    aaruTrack.Indexes?.Clear();
                }

                tracks.Add(aaruTrack);
            }

            return tracks;
        }
    }

    /// <inheritdoc />
    public List<CommonTypes.Structs.Session> Sessions { get; private set; }

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => new[]
    {
        MediaTagType.CD_FullTOC, MediaTagType.DVD_BCA, MediaTagType.DVD_DMI, MediaTagType.DVD_PFI
    };

    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => new[]
    {
        SectorTagType.CdSectorEcc, SectorTagType.CdSectorEccP, SectorTagType.CdSectorEccQ, SectorTagType.CdSectorEdc,
        SectorTagType.CdSectorHeader, SectorTagType.CdSectorSubHeader, SectorTagType.CdSectorSync,
        SectorTagType.CdTrackFlags, SectorTagType.CdSectorSubchannel
    };

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.BDR, MediaType.BDRE, MediaType.BDREXL, MediaType.BDROM, MediaType.UHDBD, MediaType.BDRXL,
        MediaType.CBHD, MediaType.CD, MediaType.CDDA, MediaType.CDEG, MediaType.CDG, MediaType.CDI, MediaType.CDMIDI,
        MediaType.CDMRW, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROMXA, MediaType.CDRW,
        MediaType.CDV, MediaType.DVDDownload, MediaType.DVDPR, MediaType.DVDPRDL, MediaType.DVDPRW, MediaType.DVDPRWDL,
        MediaType.DVDR, MediaType.DVDRAM, MediaType.DVDRDL, MediaType.DVDROM, MediaType.DVDRW, MediaType.DVDRWDL,
        MediaType.EVD, MediaType.FDDVD, MediaType.DTSCD, MediaType.FVD, MediaType.HDDVDR, MediaType.HDDVDRAM,
        MediaType.HDDVDRDL, MediaType.HDDVDROM, MediaType.HDDVDRW, MediaType.HDDVDRWDL, MediaType.HDVMD, MediaType.HVD,
        MediaType.JaguarCD, MediaType.MEGACD, MediaType.PS1CD, MediaType.PS2CD, MediaType.PS2DVD, MediaType.PS3BD,
        MediaType.PS3DVD, MediaType.PS4BD, MediaType.PS5BD, MediaType.SuperCDROM2, MediaType.SVCD, MediaType.SVOD,
        MediaType.SATURNCD, MediaType.ThreeDO, MediaType.UDO, MediaType.UDO2, MediaType.UDO2_WORM, MediaType.UMD,
        MediaType.VCD, MediaType.VCDHD, MediaType.NeoGeoCD, MediaType.PCFX, MediaType.CDTV, MediaType.CD32,
        MediaType.Nuon, MediaType.Playdia, MediaType.Pippin, MediaType.FMTOWNS, MediaType.MilCD, MediaType.VideoNow,
        MediaType.VideoNowColor, MediaType.VideoNowXp, MediaType.CVD, MediaType.PCD
    };

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description, object @default)>();

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".mds"
    };

    /// <inheritdoc />
    public bool IsWriting { get; private set; }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }

#endregion
}