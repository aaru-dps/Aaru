// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Parallels.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Parallels disk images.
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

// ReSharper disable NotAccessedField.Local

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

/// <inheritdoc />
/// <summary>Implements reading and writing Parallels' disk images</summary>
public sealed partial class Parallels : IWritableImage
{
    const string              MODULE_NAME = "Parallels plugin";
    uint[]                    _bat;
    uint                      _clusterBytes;
    long                      _currentWritingPosition;
    long                      _dataOffset;
    bool                      _empty;
    bool                      _extended;
    ImageInfo                 _imageInfo;
    Stream                    _imageStream;
    Header                    _pHdr;
    Dictionary<ulong, byte[]> _sectorCache;
    FileStream                _writingStream;

    public Parallels() => _imageInfo = new ImageInfo
    {
        ReadableSectorTags    = new List<SectorTagType>(),
        ReadableMediaTags     = new List<MediaTagType>(),
        HasPartitions         = false,
        HasSessions           = false,
        Version               = "2",
        Application           = "Parallels",
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