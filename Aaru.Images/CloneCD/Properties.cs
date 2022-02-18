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
//     Contains properties for CloneCD disc images.
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

namespace Aaru.DiscImages
{
    public sealed partial class CloneCd
    {
        /// <inheritdoc />
        public OpticalImageCapabilities OpticalCapabilities => OpticalImageCapabilities.CanStoreAudioTracks  |
                                                               OpticalImageCapabilities.CanStoreDataTracks   |
                                                               OpticalImageCapabilities.CanStorePregaps      |
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
        public ImageInfo Info => _imageInfo;
        /// <inheritdoc />
        public string Name => "CloneCD";
        /// <inheritdoc />
        public Guid Id => new Guid("EE9C2975-2E79-427A-8EE9-F86F19165784");
        /// <inheritdoc />
        public string Format => "CloneCD";
        /// <inheritdoc />
        public string Author => "Natalia Portillo";
        /// <inheritdoc />
        public List<Partition> Partitions { get; private set; }
        /// <inheritdoc />
        public List<Track> Tracks { get; private set; }
        /// <inheritdoc />
        public List<Session> Sessions { get; private set; }
        /// <inheritdoc />
        public List<DumpHardwareType> DumpHardware => null;
        /// <inheritdoc />
        public CICMMetadataType CicmMetadata => null;
        /// <inheritdoc />
        public IEnumerable<MediaTagType> SupportedMediaTags => new[]
        {
            MediaTagType.CD_MCN, MediaTagType.CD_FullTOC
        };
        /// <inheritdoc />
        public IEnumerable<SectorTagType> SupportedSectorTags => new[]
        {
            SectorTagType.CdSectorEcc, SectorTagType.CdSectorEccP, SectorTagType.CdSectorEccQ,
            SectorTagType.CdSectorEdc, SectorTagType.CdSectorHeader, SectorTagType.CdSectorSubHeader,
            SectorTagType.CdSectorSync, SectorTagType.CdTrackFlags, SectorTagType.CdSectorSubchannel
        };
        /// <inheritdoc />
        public IEnumerable<MediaType> SupportedMediaTypes => new[]
        {
            MediaType.CD, MediaType.CDDA, MediaType.CDEG, MediaType.CDG, MediaType.CDI, MediaType.CDMIDI,
            MediaType.CDMRW, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROMXA, MediaType.CDRW,
            MediaType.CDV, MediaType.DTSCD, MediaType.JaguarCD, MediaType.MEGACD, MediaType.PS1CD, MediaType.PS2CD,
            MediaType.SuperCDROM2, MediaType.SVCD, MediaType.SATURNCD, MediaType.ThreeDO, MediaType.VCD,
            MediaType.VCDHD, MediaType.NeoGeoCD, MediaType.PCFX, MediaType.CDTV, MediaType.CD32, MediaType.Nuon,
            MediaType.Playdia, MediaType.Pippin, MediaType.FMTOWNS, MediaType.MilCD, MediaType.VideoNow,
            MediaType.VideoNowColor, MediaType.VideoNowXp, MediaType.CVD, MediaType.PCD
        };
        /// <inheritdoc />
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
            new (string name, Type type, string description, object @default)[]
                {};
        /// <inheritdoc />
        public IEnumerable<string> KnownExtensions => new[]
        {
            ".ccd"
        };
        /// <inheritdoc />
        public bool IsWriting { get; private set; }
        /// <inheritdoc />
        public string ErrorMessage { get; private set; }
    }
}