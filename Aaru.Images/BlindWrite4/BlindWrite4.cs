// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite4.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages BlindWrite 4 disc images.
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

namespace Aaru.Images;

// TODO: Too many unknowns, plus a completely unknown footer, to make this writable
/// <inheritdoc />
/// <summary>Implements reading BlindWrite 4 disc images</summary>
public sealed partial class BlindWrite4 : IOpticalMediaImage
{
    const string            MODULE_NAME = "BlindWrite4 plugin";
    List<TrackDescriptor>   _bwTracks;
    IFilter                 _dataFilter, _subFilter;
    Header                  _header;
    ImageInfo               _imageInfo;
    Stream                  _imageStream;
    Dictionary<uint, ulong> _offsetMap;
    Dictionary<uint, byte>  _trackFlags;

    public BlindWrite4() => _imageInfo = new ImageInfo
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