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
//     Contains properties for cdrdao cuesheets (toc/bin).
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public sealed partial class Cdrdao
    {
        /// <inheritdoc />
        public OpticalImageCapabilities OpticalCapabilities => OpticalImageCapabilities.CanStoreAudioTracks    |
                                                               OpticalImageCapabilities.CanStoreDataTracks     |
                                                               OpticalImageCapabilities.CanStorePregaps        |
                                                               OpticalImageCapabilities.CanStoreSubchannelRw   |
                                                               OpticalImageCapabilities.CanStoreIsrc           |
                                                               OpticalImageCapabilities.CanStoreCdText         |
                                                               OpticalImageCapabilities.CanStoreMcn            |
                                                               OpticalImageCapabilities.CanStoreRawData        |
                                                               OpticalImageCapabilities.CanStoreCookedData     |
                                                               OpticalImageCapabilities.CanStoreMultipleTracks |
                                                               OpticalImageCapabilities.CanStoreIndexes;
        /// <inheritdoc />
        public ImageInfo Info => _imageInfo;
        /// <inheritdoc />
        public string Name => "CDRDAO tocfile";
        /// <inheritdoc />
        public Guid Id => new("04D7BA12-1BE8-44D4-97A4-1B48A505463E");
        /// <inheritdoc />
        public string Format => "CDRDAO tocfile";
        /// <inheritdoc />
        public string Author => "Natalia Portillo";
        /// <inheritdoc />
        public List<Partition> Partitions { get; private set; }

        /// <inheritdoc />
        public List<Session> Sessions
        {
            get
            {
                Track firstTrack = Tracks.First(t => t.Sequence == Tracks.Min(m => m.Sequence));
                Track lastTrack  = Tracks.First(t => t.Sequence == Tracks.Max(m => m.Sequence));

                return new List<Session>
                {
                    new()
                    {
                        Sequence    = 1,
                        StartSector = firstTrack.StartSector,
                        EndSector   = lastTrack.EndSector,
                        StartTrack  = firstTrack.Sequence,
                        EndTrack    = lastTrack.Sequence
                    }
                };
            }
        }

        /// <inheritdoc />
        public List<Track> Tracks
        {
            get
            {
                List<Track> tracks = new();

                foreach(CdrdaoTrack cdrTrack in _discimage.Tracks)
                {
                    var aaruTrack = new Track
                    {
                        Description       = cdrTrack.Title,
                        StartSector       = cdrTrack.StartSector,
                        Pregap            = cdrTrack.Pregap,
                        Session           = 1,
                        Sequence          = cdrTrack.Sequence,
                        Type              = CdrdaoTrackTypeToTrackType(cdrTrack.Tracktype),
                        Filter            = cdrTrack.Trackfile.Datafilter,
                        File              = cdrTrack.Trackfile.Datafilter.GetFilename(),
                        FileOffset        = cdrTrack.Trackfile.Offset,
                        FileType          = cdrTrack.Trackfile.Filetype,
                        RawBytesPerSector = cdrTrack.Bps,
                        BytesPerSector    = CdrdaoTrackTypeToCookedBytesPerSector(cdrTrack.Tracktype)
                    };

                    aaruTrack.EndSector = aaruTrack.StartSector + cdrTrack.Sectors - 1;

                    if(!cdrTrack.Indexes.TryGetValue(0, out aaruTrack.StartSector))
                        cdrTrack.Indexes.TryGetValue(1, out aaruTrack.StartSector);

                    if(cdrTrack.Subchannel)
                    {
                        aaruTrack.SubchannelType = cdrTrack.Packedsubchannel ? TrackSubchannelType.PackedInterleaved
                                                       : TrackSubchannelType.RawInterleaved;

                        aaruTrack.SubchannelFilter = cdrTrack.Trackfile.Datafilter;
                        aaruTrack.SubchannelFile   = cdrTrack.Trackfile.Datafilter.GetFilename();
                        aaruTrack.SubchannelOffset = cdrTrack.Trackfile.Offset;
                    }
                    else
                        aaruTrack.SubchannelType = TrackSubchannelType.None;

                    if(aaruTrack.Sequence == 1)
                    {
                        aaruTrack.Pregap = 150;

                        if(cdrTrack.Indexes.Count == 0)
                        {
                            aaruTrack.Indexes[0] = -150;
                            aaruTrack.Indexes[1] = 0;
                        }
                        else if(!cdrTrack.Indexes.ContainsKey(0))
                        {
                            aaruTrack.Indexes[0] = -150;

                            foreach(KeyValuePair<int, ulong> idx in cdrTrack.Indexes.OrderBy(i => i.Key))
                                aaruTrack.Indexes[(ushort)idx.Key] = (int)idx.Value;
                        }
                    }
                    else
                        foreach(KeyValuePair<int, ulong> idx in cdrTrack.Indexes.OrderBy(i => i.Key))
                            aaruTrack.Indexes[(ushort)idx.Key] = (int)idx.Value;

                    tracks.Add(aaruTrack);
                }

                return tracks;
            }
        }

        /// <inheritdoc />
        public List<DumpHardwareType> DumpHardware => null;
        /// <inheritdoc />
        public CICMMetadataType CicmMetadata => null;

        // TODO: Decode CD-Text to text
        /// <inheritdoc />
        public IEnumerable<MediaTagType> SupportedMediaTags => new[]
        {
            MediaTagType.CD_MCN
        };
        /// <inheritdoc />
        public IEnumerable<SectorTagType> SupportedSectorTags => new[]
        {
            SectorTagType.CdSectorEcc, SectorTagType.CdSectorEccP, SectorTagType.CdSectorEccQ,
            SectorTagType.CdSectorEdc, SectorTagType.CdSectorHeader, SectorTagType.CdSectorSubchannel,
            SectorTagType.CdSectorSubHeader, SectorTagType.CdSectorSync, SectorTagType.CdTrackFlags,
            SectorTagType.CdTrackIsrc
        };
        /// <inheritdoc />
        public IEnumerable<MediaType> SupportedMediaTypes => new[]
        {
            MediaType.CD, MediaType.CDDA, MediaType.CDEG, MediaType.CDG, MediaType.CDI, MediaType.CDMIDI,
            MediaType.CDMRW, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROMXA, MediaType.CDRW,
            MediaType.CDV, MediaType.DDCD, MediaType.DDCDR, MediaType.DDCDRW, MediaType.MEGACD, MediaType.PS1CD,
            MediaType.PS2CD, MediaType.SuperCDROM2, MediaType.SVCD, MediaType.SATURNCD, MediaType.ThreeDO,
            MediaType.VCD, MediaType.VCDHD, MediaType.NeoGeoCD, MediaType.PCFX, MediaType.CDTV, MediaType.CD32,
            MediaType.Nuon, MediaType.Playdia, MediaType.Pippin, MediaType.FMTOWNS, MediaType.MilCD, MediaType.VideoNow,
            MediaType.VideoNowColor, MediaType.VideoNowXp, MediaType.CVD, MediaType.PCD
        };
        /// <inheritdoc />
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions => new[]
        {
            ("separate", typeof(bool), "Write each track to a separate file.", (object)false)
        };
        /// <inheritdoc />
        public IEnumerable<string> KnownExtensions => new[]
        {
            ".toc"
        };
        /// <inheritdoc />
        public bool IsWriting { get; private set; }
        /// <inheritdoc />
        public string ErrorMessage { get; private set; }
    }
}