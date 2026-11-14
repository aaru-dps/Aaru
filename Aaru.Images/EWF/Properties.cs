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
//     Contains properties for Expert Witness Format disk images.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Humanizer;
using Partition = Aaru.CommonTypes.Partition;
using Track = Aaru.CommonTypes.Structs.Track;

namespace Aaru.Images;

public sealed partial class Ewf
{
#region IOpticalMediaImage Members

    /// <inheritdoc />

    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.Ewf_Name;

    /// <inheritdoc />
    public Guid Id => new("E5F3B186-2382-4BDE-89B2-A6FC2B464C6F");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public string Format => _isSmart
                                ? "Expert Witness Format (SMART)"
                                : _isV2
                                    ? "Expert Witness Format v2"
                                    : "Expert Witness Format";

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata => _metadata;

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => [];

    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => [];

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes =>
    [
        MediaType.Unknown, MediaType.GENERIC_HDD, MediaType.FlashDrive, MediaType.CompactFlash,
        MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII,
        MediaType.PCCardTypeIV, MediaType.CD, MediaType.CDDA, MediaType.CDG, MediaType.CDEG, MediaType.CDI,
        MediaType.CDROM, MediaType.CDROMXA, MediaType.CDPLUS, MediaType.CDR, MediaType.CDRW, MediaType.CDMRW,
        MediaType.DVDROM, MediaType.DVDR, MediaType.DVDRW, MediaType.DVDPR, MediaType.DVDPRW, MediaType.DVDRDL,
        MediaType.DVDPRWDL, MediaType.DVDPRDL, MediaType.DVDRAM, MediaType.DVDRWDL, MediaType.BDROM, MediaType.BDR,
        MediaType.BDRE, MediaType.BDRXL, MediaType.BDREXL
    ];

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions => [];

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => [".E01", ".Ex01", ".s01"];

    /// <inheritdoc />
    public bool IsWriting => false;

    /// <inheritdoc />
    public string ErrorMessage => null;

    /// <inheritdoc />
    public List<Track> Tracks
    {
        get
        {
            if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc) return null;

            return _tracks;
        }
    }

    /// <inheritdoc />
    public List<Session> Sessions
    {
        get
        {
            if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc) return null;

            return _sessions;
        }
    }

    /// <inheritdoc />
    public List<Partition> Partitions
    {
        get
        {
            if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc) return null;

            CommonTypes.Enums.TrackType type =
                _tracks is { Count: > 0 } ? _tracks[0].Type : CommonTypes.Enums.TrackType.CdMode1;

            List<Partition> parts =
            [
                new()
                {
                    Start    = 0,
                    Length   = _imageInfo.Sectors,
                    Offset   = 0,
                    Sequence = 0,
                    Name     = string.Format(Localization.Track_0, 1),
                    Type     = type.Humanize(),
                    Size     = _imageInfo.Sectors * _imageInfo.SectorSize,
                    Scheme   = Localization.Optical_disc_track
                }
            ];

            return parts;
        }
    }

#endregion
}