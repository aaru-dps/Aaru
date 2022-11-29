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

public sealed partial class Gdi
{
    /// <inheritdoc />
    public string Name => Localization.Gdi_Name;
    /// <inheritdoc />
    public Guid Id => new("281ECBF2-D2A7-414C-8497-1A33F6DCB2DD");
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
    /// <inheritdoc />
    public string Format => "Dreamcast GDI image";

    /// <inheritdoc />
    public List<Partition> Partitions { get; private set; }

    /// <inheritdoc />
    public List<Track> Tracks
    {
        get
        {
            List<Track> tracks = new();

            foreach(GdiTrack gdiTrack in _discImage.Tracks)
            {
                var track = new Track
                {
                    Description       = null,
                    StartSector       = gdiTrack.StartSector,
                    Pregap            = gdiTrack.Pregap,
                    Session           = (ushort)(gdiTrack.HighDensity ? 2 : 1),
                    Sequence          = gdiTrack.Sequence,
                    Type              = gdiTrack.TrackType,
                    Filter            = gdiTrack.TrackFilter,
                    File              = gdiTrack.TrackFile,
                    FileOffset        = (ulong)gdiTrack.Offset,
                    FileType          = "BINARY",
                    RawBytesPerSector = gdiTrack.Bps,
                    BytesPerSector    = gdiTrack.TrackType == TrackType.Data ? 2048 : 2352,
                    SubchannelType    = TrackSubchannelType.None
                };

                track.EndSector = track.StartSector + gdiTrack.Sectors - 1;

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