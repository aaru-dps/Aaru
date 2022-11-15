// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CDRWin.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages CDRWin cuesheets (cue/bin).
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

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;

namespace Aaru.DiscImages;

// TODO: Implement track flags
/// <inheritdoc cref="Aaru.CommonTypes.Interfaces.IWritableOpticalImage" />
/// <summary>Implements reading and writing CDRWin cuesheet disc images</summary>
public sealed partial class CdrWin : IWritableOpticalImage, IVerifiableImage
{
    IFilter      _cdrwinFilter;
    StreamReader _cueStream;
    StreamWriter _descriptorStream;
    CdrWinDisc   _discImage;
    ImageInfo    _imageInfo;
    Stream       _imageStream;
    bool         _isCd;
    uint         _lostPregap;
    bool         _negativeEnd;
    /// <summary>Dictionary, index is track #, value is File</summary>
    Dictionary<uint, ulong> _offsetMap;
    SectorBuilder                _sectorBuilder;
    bool                         _separateTracksWriting;
    Dictionary<byte, byte>       _trackFlags;
    Dictionary<byte, string>     _trackIsrcs;
    string                       _writingBaseName;
    Dictionary<uint, FileStream> _writingStreams;
    List<Track>                  _writingTracks;

    public CdrWin() => _imageInfo = new ImageInfo
    {
        ReadableSectorTags    = new List<SectorTagType>(),
        ReadableMediaTags     = new List<MediaTagType>(),
        HasPartitions         = true,
        HasSessions           = true,
        Version               = null,
        ApplicationVersion    = null,
        MediaTitle            = null,
        Creator               = null,
        MediaManufacturer     = null,
        MediaModel            = null,
        MediaPartNumber       = null,
        MediaSequence         = 0,
        LastMediaSequence     = 0,
        DriveManufacturer     = null,
        DriveModel            = null,
        DriveSerialNumber     = null,
        DriveFirmwareRevision = null
    };
}