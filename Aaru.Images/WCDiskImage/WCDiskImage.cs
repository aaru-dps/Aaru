// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : WCDiskImage.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages floppy disk images created with d2f by DataPackRat
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
// Copyright © 2018-2019 Michael Drüing
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages;

/// <inheritdoc />
/// <summary>Manages floppy disk images created with d2f by DataPackRat</summary>
[SuppressMessage("ReSharper", "NotAccessedField.Local"), SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class WCDiskImage : IMediaImage
{
    readonly Dictionary<(int cylinder, int head, int sector), bool> _badSectors =
        new Dictionary<(int cylinder, int head, int sector), bool>();
    /// <summary>The file header after the image has been opened</summary>
    FileHeader _fileHeader;
    ImageInfo _imageInfo;

    /* the sectors are cached here */
    readonly Dictionary<(int cylinder, int head, int sector), byte[]> _sectorCache =
        new Dictionary<(int cylinder, int head, int sector), byte[]>();

    /// <summary>The ImageFilter we're reading from, after the file has been opened</summary>
    IFilter _wcImageFilter;

    /// <summary>Manages floppy disk images created with d2f by DataPackRat</summary>
    public WCDiskImage() => _imageInfo = new ImageInfo
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