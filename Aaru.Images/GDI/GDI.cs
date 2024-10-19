// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : GDI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Dreamcast GDI disc images.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;

namespace Aaru.Images;

// TODO: There seems no be no clear definition on how to treat pregaps that are not included in the file, so this is just appending it to start of track
// TODO: This format doesn't support to specify pregaps that are included in the file (like Redump ones)
/// <inheritdoc />
/// <summary>Implements reading Dreamcast GDI disc images</summary>
[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class Gdi : IOpticalMediaImage
{
    const string MODULE_NAME = "GDI plugin";
    ulong        _densitySeparationSectors;
    GdiDisc      _discImage;
    StreamReader _gdiStream;
    ImageInfo    _imageInfo;
    Stream       _imageStream;
    /// <summary>Dictionary, index is track #, value is track number, or 0 if a TOC</summary>
    Dictionary<uint, ulong> _offsetMap;
    SectorBuilder _sectorBuilder;

    public Gdi() => _imageInfo = new ImageInfo
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