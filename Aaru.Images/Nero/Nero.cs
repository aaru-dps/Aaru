// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Nero.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Nero Burning ROM disc images.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;

#pragma warning disable 414
#pragma warning disable 169

namespace Aaru.DiscImages;

/// <inheritdoc />
/// <summary>Implements reading Nero Burning ROM disc images</summary>
[SuppressMessage("ReSharper", "NotAccessedField.Local")]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
public sealed partial class Nero : IOpticalMediaImage
{
    const    string                      MODULE_NAME = "Nero plugin";
    readonly Dictionary<ushort, uint>    _neroSessions;
    readonly Dictionary<uint, NeroTrack> _neroTracks;
    readonly Dictionary<uint, ulong>     _offsetmap;
    CdText                               _cdtxt;
    CuesheetV1                           _cuesheetV1;
    CuesheetV2                           _cuesheetV2;
    DiscInformation                      _discInfo;
    ImageInfo                            _imageInfo;
    bool                                 _imageNewFormat;
    Stream                               _imageStream;
    bool                                 _isCd;
    MediaType                            _mediaType;
    DaoV1                                _neroDaov1;
    DaoV2                                _neroDaov2;
    IFilter                              _neroFilter;
    ReloChunk                            _relo;
    SectorBuilder                        _sectorBuilder;
    TaoV0                                _taoV0;
    TaoV1                                _taoV1;
    TaoV2                                _taoV2;
    TocChunk                             _toc;
    Dictionary<uint, byte>               _trackFlags;
    Dictionary<uint, byte[]>             _trackIsrCs;
    byte[]                               _upc;

    public Nero()
    {
        _imageNewFormat = false;

        _imageInfo = new ImageInfo
        {
            ReadableSectorTags = new List<SectorTagType>(),
            ReadableMediaTags  = new List<MediaTagType>()
        };

        _neroSessions = new Dictionary<ushort, uint>();
        _neroTracks   = new Dictionary<uint, NeroTrack>();
        _offsetmap    = new Dictionary<uint, ulong>();
        Sessions      = new List<CommonTypes.Structs.Session>();
        Partitions    = new List<Partition>();
    }
}