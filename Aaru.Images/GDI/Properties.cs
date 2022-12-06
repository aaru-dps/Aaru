﻿// /***************************************************************************
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
//     Contains properties for Dreamcast GDI disc images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages
{
    public sealed partial class Gdi
    {
        /// <inheritdoc />
        public string Name => "Dreamcast GDI image";
        /// <inheritdoc />
        public Guid Id => new Guid("281ECBF2-D2A7-414C-8497-1A33F6DCB2DD");
        /// <inheritdoc />
        public ImageInfo Info => _imageInfo;
        /// <inheritdoc />
        public string Author => "Natalia Portillo";
        /// <inheritdoc />
        public string Format => "Dreamcast GDI image";

        /// <inheritdoc />
        public List<Partition> Partitions { get; private set; }

        /// <inheritdoc />
        public List<Track> Tracks
        {
            get
            {
                List<Track> tracks = new List<Track>();

                foreach(GdiTrack gdiTrack in _discImage.Tracks)
                {
                    var track = new Track
                    {
                        TrackDescription       = null,
                        TrackStartSector       = gdiTrack.StartSector,
                        TrackPregap            = gdiTrack.Pregap,
                        TrackSession           = (ushort)(gdiTrack.HighDensity ? 2 : 1),
                        TrackSequence          = gdiTrack.Sequence,
                        TrackType              = gdiTrack.TrackType,
                        TrackFilter            = gdiTrack.TrackFilter,
                        TrackFile              = gdiTrack.TrackFile,
                        TrackFileOffset        = (ulong)gdiTrack.Offset,
                        TrackFileType          = "BINARY",
                        TrackRawBytesPerSector = gdiTrack.Bps,
                        TrackBytesPerSector    = gdiTrack.TrackType == TrackType.Data ? 2048 : 2352,
                        TrackSubchannelType    = TrackSubchannelType.None
                    };

                    track.TrackEndSector = track.TrackStartSector + gdiTrack.Sectors - 1;

                    tracks.Add(track);
                }

                return tracks;
            }
        }

        /// <inheritdoc />
        public List<Session> Sessions => _discImage.Sessions;
        /// <inheritdoc />
        public List<DumpHardwareType> DumpHardware => null;
        /// <inheritdoc />
        public CICMMetadataType CicmMetadata => null;
    }
}