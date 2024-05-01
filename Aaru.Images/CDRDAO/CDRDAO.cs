// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CDRDAO.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages cdrdao cuesheets (toc/bin).
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;

namespace Aaru.Images;

// TODO: Doesn't support compositing from several files
// TODO: Doesn't support silences that are not in files
/// <inheritdoc />
/// <summary>Implements reading and writing cdrdao cuesheet disc images</summary>
public sealed partial class Cdrdao : IWritableOpticalImage
{
    const string MODULE_NAME = "CDRDAO plugin";
    IFilter      _cdrdaoFilter;
    StreamWriter _descriptorStream;
    CdrdaoDisc   _discimage;
    ImageInfo    _imageInfo;
    Stream       _imageStream;
    /// <summary>Dictionary, index is track #, value is File</summary>
    Dictionary<uint, ulong> _offsetmap;
    SectorBuilder                _sectorBuilder;
    bool                         _separateTracksWriting;
    StreamReader                 _tocStream;
    Dictionary<byte, byte>       _trackFlags;
    Dictionary<byte, string>     _trackIsrcs;
    string                       _writingBaseName;
    Dictionary<uint, FileStream> _writingStreams;
    List<Track>                  _writingTracks;

    public Cdrdao() => _imageInfo = new ImageInfo
    {
        ReadableSectorTags    = [],
        ReadableMediaTags     = [],
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