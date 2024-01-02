// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VHD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Connectix and Microsoft Virtual PC disk images.
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
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

/// <inheritdoc />
/// <summary>
///     Supports Connectix/Microsoft Virtual PC hard disk image format. Until Virtual PC 5 there existed no format,
///     and the hard disk image was merely a sector by sector (RAW) image with a resource fork giving information to
///     Virtual PC itself.
/// </summary>
public sealed partial class Vhd : IWritableImage
{
    const string      MODULE_NAME = "Virtual PC plugin";
    uint              _bitmapSize;
    uint[]            _blockAllocationTable;
    bool              _blockInCache;
    uint              _blockSize;
    byte[]            _cachedBlock;
    uint              _cachedBlockNumber;
    long              _cachedBlockPosition;
    long              _currentFooterPosition;
    bool              _dynamic;
    ImageInfo         _imageInfo;
    byte[][]          _locatorEntriesData;
    DateTime          _parentDateTime;
    IMediaImage       _parentImage;
    DateTime          _thisDateTime;
    DynamicDiskHeader _thisDynamic;
    IFilter           _thisFilter;
    HardDiskFooter    _thisFooter;
    FileStream        _writingStream;

    public Vhd() => _imageInfo = new ImageInfo
    {
        ReadableSectorTags    = new List<SectorTagType>(),
        ReadableMediaTags     = new List<MediaTagType>(),
        HasPartitions         = false,
        HasSessions           = false,
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
}