// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VHDX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Microsoft Hyper-V disk images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

// ReSharper disable NotAccessedField.Local

namespace Aaru.DiscImages
{
    /// <inheritdoc />
    /// <summary>
    /// Implements reading Hyper-V disk images
    /// </summary>
    public sealed partial class Vhdx : IMediaImage
    {
        long                      _batOffset;
        ulong[]                   _blockAllocationTable;
        Dictionary<ulong, byte[]> _blockCache;
        long                      _chunkRatio;
        ulong                     _dataBlocks;
        bool                      _hasParent;
        ImageInfo                 _imageInfo;
        Stream                    _imageStream;
        uint                      _logicalSectorSize;
        int                       _maxBlockCache;
        int                       _maxSectorCache;
        long                      _metadataOffset;
        Guid                      _page83Data;
        IMediaImage               _parentImage;
        uint                      _physicalSectorSize;
        byte[]                    _sectorBitmap;
        ulong[]                   _sectorBitmapPointers;
        Dictionary<ulong, byte[]> _sectorCache;
        FileParameters            _vFileParms;
        Header                    _vHdr;
        Identifier                _id;
        ulong                     _virtualDiskSize;
        MetadataTableHeader       _vMetHdr;
        MetadataTableEntry[]      _vMets;
        ParentLocatorHeader       _vParHdr;
        ParentLocatorEntry[]      _vPars;
        RegionTableHeader         _vRegHdr;
        RegionTableEntry[]        _vRegs;

        public Vhdx() => _imageInfo = new ImageInfo
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
}