// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CloneCD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages CloneCD disc images.
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

// TODO: CloneCD stores subchannel deinterleaved
/// <inheritdoc />
/// <summary>Implements reading and writing CloneCD disc images</summary>
public sealed partial class CloneCd : IWritableOpticalImage
{
    string                  _catalog; // TODO: Use it
    IFilter                 _ccdFilter;
    byte[]                  _cdtext;
    StreamReader            _cueStream;
    IFilter                 _dataFilter;
    Stream                  _dataStream;
    StreamWriter            _descriptorStream;
    byte[]                  _fullToc;
    ImageInfo               _imageInfo;
    Dictionary<uint, ulong> _offsetMap;
    bool                    _scrambled;
    IFilter                 _subFilter;
    Stream                  _subStream;
    Dictionary<byte, byte>  _trackFlags;
    string                  _writingBaseName;

    public CloneCd() => _imageInfo = new ImageInfo
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

    const string MODULE_NAME = "CloneCD plugin";
}