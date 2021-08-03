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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Structs;
using Schemas;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages
{
    public sealed partial class ZZZRawImage
    {
        public OpticalImageCapabilities OpticalCapabilities => OpticalImageCapabilities.CanStoreDataTracks |
                                                               OpticalImageCapabilities.CanStoreCookedData;
        public string Name => "Raw Disk Image";

        // Non-random UUID to recognize this specific plugin
        public Guid      Id     => new Guid("12345678-AAAA-BBBB-CCCC-123456789000");
        public ImageInfo Info   => _imageInfo;
        public string    Author => "Natalia Portillo";
        public string    Format => "Raw disk image (sector by sector copy)";

        public List<Track> Tracks
        {
            get
            {
                if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                    return null;

                var trk = new Track
                {
                    TrackBytesPerSector = _rawCompactDisc ? _mode2
                                                                ? 2336
                                                                : 2048 : (int)_imageInfo.SectorSize,
                    TrackEndSector         = _imageInfo.Sectors - 1,
                    TrackFile              = _rawImageFilter?.GetFilename() ?? _basePath,
                    TrackFileOffset        = 0,
                    TrackFileType          = "BINARY",
                    TrackRawBytesPerSector = _rawCompactDisc ? 2352 : (int)_imageInfo.SectorSize,
                    TrackSequence          = 1,
                    TrackStartSector       = 0,
                    TrackSubchannelType =
                        _hasSubchannel ? TrackSubchannelType.RawInterleaved : TrackSubchannelType.None,
                    TrackType = _rawCompactDisc ? _mode2
                                                      ? TrackType.CdMode2Formless
                                                      : TrackType.CdMode1 : TrackType.Data,
                    TrackSession = 1
                };

                if(_imageInfo.MediaType == MediaType.CD   ||
                   _imageInfo.MediaType == MediaType.CDRW ||
                   _imageInfo.MediaType == MediaType.CDR)
                {
                    trk.TrackPregap = 150;
                    trk.Indexes[0]  = -150;
                    trk.Indexes[1]  = 0;

                    if(trk.TrackType == TrackType.Data)
                        trk.TrackType = TrackType.CdMode1;
                }

                List<Track> lst = new List<Track>
                {
                    trk
                };

                return lst;
            }
        }

        public List<Session> Sessions
        {
            get
            {
                if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                    throw new FeatureUnsupportedImageException("Feature not supported by image format");

                var sess = new Session
                {
                    EndSector       = _imageInfo.Sectors - 1,
                    EndTrack        = 1,
                    SessionSequence = 1,
                    StartSector     = 0,
                    StartTrack      = 1
                };

                List<Session> lst = new List<Session>
                {
                    sess
                };

                return lst;
            }
        }

        public List<Partition> Partitions
        {
            get
            {
                if(_imageInfo.XmlMediaType != XmlMediaType.OpticalDisc)
                    return null;

                List<Partition> parts = new List<Partition>();

                var part = new Partition
                {
                    Start    = 0,
                    Length   = _imageInfo.Sectors,
                    Offset   = 0,
                    Sequence = 0,
                    Type = _rawCompactDisc
                               ? _mode2
                                     ? "MODE2/2352"
                                     : "MODE1/2352"
                               : _imageInfo.MediaType == MediaType.PD650 || _imageInfo.MediaType == MediaType.PD650_WORM
                                   ? "DATA/512"
                                   : "MODE1/2048",
                    Size = _imageInfo.Sectors * _imageInfo.SectorSize
                };

                parts.Add(part);

                return parts;
            }
        }

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata { get; private set; }
        public IEnumerable<MediaTagType> SupportedMediaTags => _readWriteSidecars.Concat(_writeOnlySidecars).
            OrderBy(t => t.tag).Select(t => t.tag).ToArray();

        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[]
            {};

        public IEnumerable<MediaType> SupportedMediaTypes
        {
            get
            {
                List<MediaType> types = new List<MediaType>();

                foreach(MediaType type in Enum.GetValues(typeof(MediaType)))
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
                        case MediaType.ECMA_78: continue;
                        default:
                            types.Add(type);

                            break;
                    }

                return types;
            }
        }

        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
            new (string name, Type type, string description, object @default)[]
                {};
        public IEnumerable<string> KnownExtensions => new[]
        {
            ".adf", ".adl", ".d81", ".dsk", ".hdf", ".ima", ".img", ".iso", ".ssd", ".st", ".1kn", ".2kn", ".4kn",
            ".8kn", ".16kn", ".32kn", ".64kn", ".512e", ".512", ".128", ".256"
        };
        public bool   IsWriting    { get; private set; }
        public string ErrorMessage { get; private set; }
    }
}