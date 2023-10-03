// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Alcohol120.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Alcohol 120% disc images.
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

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages;

/// <inheritdoc />
/// <summary>Implements reading and writing Alcohol 120% disk images</summary>
public sealed partial class Alcohol120 : IWritableOpticalImage
{
    Footer                                  _alcFooter;
    IFilter                                 _alcImage;
    Dictionary<int, Session>                _alcSessions;
    Dictionary<int, Dictionary<int, Track>> _alcToc;
    Dictionary<int, TrackExtra>             _alcTrackExtras;
    Dictionary<int, Track>                  _alcTracks;
    byte[]                                  _bca;
    FileStream                              _descriptorStream;
    byte[]                                  _dmi;
    byte[]                                  _fullToc;
    Header                                  _header;
    ImageInfo                               _imageInfo;
    Stream                                  _imageStream;
    bool                                    _isDvd;
    Dictionary<uint, ulong>                 _offsetMap;
    byte[]                                  _pfi;
    Dictionary<byte, byte>                  _trackFlags;
    List<CommonTypes.Structs.Track>         _writingTracks;

    public Alcohol120() => _imageInfo = new ImageInfo
    {
        ReadableSectorTags    = new List<SectorTagType>(),
        ReadableMediaTags     = new List<MediaTagType>(),
        HasPartitions         = true,
        HasSessions           = true,
        Version               = null,
        Application           = null,
        ApplicationVersion    = null,
        Creator               = null,
        Comments              = null,
        MediaManufacturer     = null,
        MediaModel            = null,
        MediaSerialNumber     = null,
        MediaBarcode          = null,
        MediaPartNumber       = null,
        MediaSequence         = 0,
        LastMediaSequence     = 0,
        DriveManufacturer     = null,
        DriveModel            = null,
        DriveSerialNumber     = null,
        DriveFirmwareRevision = null
    };

    const string MODULE_NAME = "Alcohol 120% plugin";
}