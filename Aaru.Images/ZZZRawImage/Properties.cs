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
//     Contains properties for raw image, that is, user data sector by sector copy.
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Partition = Aaru.CommonTypes.Partition;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages;

public sealed partial class ZZZRawImage
{
#region IWritableOpticalImage Members

    /// <inheritdoc />
    public OpticalImageCapabilities OpticalCapabilities => OpticalImageCapabilities.CanStoreDataTracks |
                                                           OpticalImageCapabilities.CanStoreCookedData;

    /// <inheritdoc />
    public string Name => Localization.ZZZRawImage_Name;

    // Non-random UUID to recognize this specific plugin
    /// <inheritdoc />
    public Guid Id => new("12345678-AAAA-BBBB-CCCC-123456789000");

    /// <inheritdoc />
    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public string Format => "Raw disk image (sector by sector copy)";

    /// <inheritdoc />
    public List<Track> Tracks
    {
        get
        {
            if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc)
                return null;

            var trk = new Track
            {
                BytesPerSector    = _rawCompactDisc ? _mode2 ? 2336 : 2048 : (int)_imageInfo.SectorSize,
                EndSector         = _imageInfo.Sectors - 1,
                File              = _rawImageFilter?.Filename ?? _basePath,
                FileOffset        = 0,
                FileType          = "BINARY",
                RawBytesPerSector = _rawCompactDisc ? 2352 : (int)_imageInfo.SectorSize,
                Sequence          = 1,
                StartSector       = 0,
                SubchannelType    = _hasSubchannel ? TrackSubchannelType.RawInterleaved : TrackSubchannelType.None,
                Type = _toastXa        ? TrackType.CdMode2Form1 :
                       _rawCompactDisc ? _mode2 ? TrackType.CdMode2Formless : TrackType.CdMode1 : TrackType.Data,
                Session = 1
            };

            if(_imageInfo.MediaType is MediaType.CD or MediaType.CDRW or MediaType.CDR)
            {
                trk.Pregap     = 150;
                trk.Indexes[0] = -150;
                trk.Indexes[1] = 0;

                if(trk.Type == TrackType.Data)
                    trk.Type = TrackType.CdMode1;
            }

            List<Track> lst = new()
            {
                trk
            };

            return lst;
        }
    }

    /// <inheritdoc />
    public List<Session> Sessions
    {
        get
        {
            if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc)
                return null;

            var sess = new Session
            {
                EndSector   = _imageInfo.Sectors - 1,
                EndTrack    = 1,
                Sequence    = 1,
                StartSector = 0,
                StartTrack  = 1
            };

            List<Session> lst = new()
            {
                sess
            };

            return lst;
        }
    }

    /// <inheritdoc />
    public List<Partition> Partitions
    {
        get
        {
            if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc)
                return null;

            List<Partition> parts = new();

            var part = new Partition
            {
                Start = 0,
                Length = _imageInfo.Sectors,
                Offset = 0,
                Sequence = 0,
                Type = _rawCompactDisc ? _mode2 || _toastXa ? "MODE2/2352" : "MODE1/2352" :
                       _imageInfo.MediaType is MediaType.PD650 or MediaType.PD650_WORM ? "DATA/512" : "MODE1/2048",
                Size = _imageInfo.Sectors * _imageInfo.SectorSize
            };

            parts.Add(part);

            return parts;
        }
    }

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata { get; private set; }

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => _readWriteSidecars.Concat(_writeOnlySidecars).
                                                                              OrderBy(t => t.tag).
                                                                              Select(t => t.tag).
                                                                              ToArray();

    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => Array.Empty<SectorTagType>();

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes
    {
        get
        {
            List<MediaType> types = new();

            foreach(MediaType type in Enum.GetValues(typeof(MediaType)))
            {
                switch(type)
                {
                    // TODO: Implement support for writing formats with different track 0 bytes per sector
                    case MediaType.IBM33FD_256:
                    case MediaType.IBM33FD_512:
                    case MediaType.IBM43FD_128:
                    case MediaType.IBM43FD_256:
                    case MediaType.IBM53FD_256:
                    case MediaType.IBM53FD_512:
                    case MediaType.IBM53FD_1024:
                    case MediaType.ECMA_99_8:
                    case MediaType.ECMA_99_15:
                    case MediaType.ECMA_99_26:
                    case MediaType.ECMA_66:
                    case MediaType.ECMA_69_8:
                    case MediaType.ECMA_69_15:
                    case MediaType.ECMA_69_26:
                    case MediaType.ECMA_70:
                    case MediaType.ECMA_78:
                        continue;
                    default:
                        types.Add(type);

                        break;
                }
            }

            return types;
        }
    }

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description, object @default)>();

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".adf", ".adl", ".d81", ".dsk", ".hdf", ".ima", ".img", ".iso", ".ssd", ".st", ".1kn", ".2kn", ".4kn", ".8kn",
        ".16kn", ".32kn", ".64kn", ".512e", ".512", ".128", ".256"
    };

    /// <inheritdoc />
    public bool IsWriting { get; private set; }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }

#endregion
}