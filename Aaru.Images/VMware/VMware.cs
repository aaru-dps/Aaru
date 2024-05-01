// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VMware.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages VMware disk images.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

/// <inheritdoc />
/// <summary>Implements reading VMware disk images</summary>
public sealed partial class VMware : IWritableImage
{
    const string              MODULE_NAME = "VMware plugin";
    string                    _adapterType;
    uint                      _cid;
    StreamWriter              _descriptorStream;
    Dictionary<ulong, Extent> _extents;
    IFilter                   _gdFilter;
    Dictionary<ulong, byte[]> _grainCache;
    ulong                     _grainSize;
    uint[]                    _gTable;
    bool                      _hasParent;
    uint                      _hwversion;
    ImageInfo                 _imageInfo;
    string                    _imageType;
    uint                      _maxCachedGrains;
    uint                      _parentCid;
    IMediaImage               _parentImage;
    string                    _parentName;
    Dictionary<ulong, byte[]> _sectorCache;
    uint                      _version;
    CowHeader                 _vmCHdr;
    ExtentHeader              _vmEHdr;
    string                    _writingBaseName;
    FileStream                _writingStream;

    public VMware() => _imageInfo = new ImageInfo
    {
        ReadableSectorTags    = new List<SectorTagType>(),
        ReadableMediaTags     = new List<MediaTagType>(),
        HasPartitions         = false,
        HasSessions           = false,
        Version               = null,
        Application           = "VMware",
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