// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class CdrWin
    {
        public ImageInfo       Info       => imageInfo;
        public string          Name       => "CDRWin cuesheet";
        public Guid            Id         => new Guid("664568B2-15D4-4E64-8A7A-20BDA8B8386F");
        public string          Format     => "CDRWin CUESheet";
        public List<Partition> Partitions { get; private set; }
        public List<Track> Tracks
        {
            get
            {
                List<Track> tracks = new List<Track>();

                ulong previousStartSector = 0;

                foreach(CdrWinTrack cdrTrack in discimage.Tracks)
                {
                    Track dicTrack = new Track
                    {
                        Indexes                = cdrTrack.Indexes,
                        TrackDescription       = cdrTrack.Title,
                        TrackStartSector       = previousStartSector,
                        TrackPregap            = cdrTrack.Pregap,
                        TrackSession           = cdrTrack.Session,
                        TrackSequence          = cdrTrack.Sequence,
                        TrackType              = CdrWinTrackTypeToTrackType(cdrTrack.Tracktype),
                        TrackFile              = cdrTrack.Trackfile.Datafilter.GetFilename(),
                        TrackFilter            = cdrTrack.Trackfile.Datafilter,
                        TrackFileOffset        = cdrTrack.Trackfile.Offset,
                        TrackFileType          = cdrTrack.Trackfile.Filetype,
                        TrackRawBytesPerSector = cdrTrack.Bps,
                        TrackBytesPerSector    = CdrWinTrackTypeToCookedBytesPerSector(cdrTrack.Tracktype)
                    };
                    dicTrack.TrackEndSector = dicTrack.TrackStartSector + cdrTrack.Sectors - 1;

                    /*if(!cdrTrack.Indexes.TryGetValue(0, out dicTrack.TrackStartSector))
                        cdrTrack.Indexes.TryGetValue(1, out dicTrack.TrackStartSector);*/
                    if(cdrTrack.Tracktype == CDRWIN_TRACK_TYPE_CDG)
                    {
                        dicTrack.TrackSubchannelFilter = cdrTrack.Trackfile.Datafilter;
                        dicTrack.TrackSubchannelFile   = cdrTrack.Trackfile.Datafilter.GetFilename();
                        dicTrack.TrackSubchannelOffset = cdrTrack.Trackfile.Offset;
                        dicTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;
                    }
                    else dicTrack.TrackSubchannelType = TrackSubchannelType.None;

                    tracks.Add(dicTrack);
                    previousStartSector = dicTrack.TrackEndSector + 1;
                }

                return tracks;
            }
        }
        public List<Session>             Sessions           => discimage.Sessions;
        public List<DumpHardwareType>    DumpHardware       => null;
        public CICMMetadataType          CicmMetadata       => null;
        public IEnumerable<MediaTagType> SupportedMediaTags => new[] {MediaTagType.CD_MCN, MediaTagType.CD_TEXT};
        public IEnumerable<SectorTagType> SupportedSectorTags =>
            new[]
            {
                SectorTagType.CdSectorEcc, SectorTagType.CdSectorEccP, SectorTagType.CdSectorEccQ,
                SectorTagType.CdSectorEdc, SectorTagType.CdSectorHeader, SectorTagType.CdSectorSubHeader,
                SectorTagType.CdSectorSync, SectorTagType.CdTrackFlags, SectorTagType.CdTrackIsrc
            };
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.BDR, MediaType.BDRE, MediaType.BDREXL, MediaType.BDROM, MediaType.BDRXL, MediaType.CBHD,
                MediaType.CD, MediaType.CDDA, MediaType.CDEG, MediaType.CDG, MediaType.CDI, MediaType.CDMIDI,
                MediaType.CDMRW, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROMXA, MediaType.CDRW,
                MediaType.CDV, MediaType.DDCD, MediaType.DDCDR, MediaType.DDCDRW, MediaType.DVDDownload,
                MediaType.DVDPR, MediaType.DVDPRDL, MediaType.DVDPRW, MediaType.DVDPRWDL, MediaType.DVDR,
                MediaType.DVDRAM, MediaType.DVDRDL, MediaType.DVDROM, MediaType.DVDRW, MediaType.DVDRWDL, MediaType.EVD,
                MediaType.FDDVD, MediaType.DTSCD, MediaType.FVD, MediaType.HDDVDR, MediaType.HDDVDRAM,
                MediaType.HDDVDRDL, MediaType.HDDVDROM, MediaType.HDDVDRW, MediaType.HDDVDRWDL, MediaType.HDVMD,
                MediaType.HVD, MediaType.JaguarCD, MediaType.MEGACD, MediaType.PD650, MediaType.PD650_WORM,
                MediaType.PS1CD, MediaType.PS2CD, MediaType.PS2DVD, MediaType.PS3BD, MediaType.PS3DVD, MediaType.PS4BD,
                MediaType.SuperCDROM2, MediaType.SVCD, MediaType.SVOD, MediaType.SATURNCD, MediaType.ThreeDO,
                MediaType.UDO, MediaType.UDO2, MediaType.UDO2_WORM, MediaType.UMD, MediaType.VCD, MediaType.VCDHD,
                MediaType.NeoGeoCD, MediaType.PCFX
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new[] {("separate", typeof(bool), "Write each track to a separate file.")};
        public IEnumerable<string> KnownExtensions => new[] {".cue"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }
    }
}