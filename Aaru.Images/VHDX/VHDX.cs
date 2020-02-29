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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    public partial class Vhdx : IMediaImage
    {
        long                      batOffset;
        ulong[]                   blockAllocationTable;
        Dictionary<ulong, byte[]> blockCache;
        long                      chunkRatio;
        ulong                     dataBlocks;
        bool                      hasParent;
        ImageInfo                 imageInfo;
        Stream                    imageStream;
        uint                      logicalSectorSize;
        int                       maxBlockCache;
        int                       maxSectorCache;
        long                      metadataOffset;
        Guid                      page83Data;
        IMediaImage               parentImage;
        uint                      physicalSectorSize;
        byte[]                    sectorBitmap;
        ulong[]                   sectorBitmapPointers;
        Dictionary<ulong, byte[]> sectorCache;
        VhdxFileParameters        vFileParms;
        VhdxHeader                vHdr;
        VhdxIdentifier            vhdxId;
        ulong                     virtualDiskSize;
        VhdxMetadataTableHeader   vMetHdr;
        VhdxMetadataTableEntry[]  vMets;
        VhdxParentLocatorHeader   vParHdr;
        VhdxParentLocatorEntry[]  vPars;
        VhdxRegionTableHeader     vRegHdr;
        VhdxRegionTableEntry[]    vRegs;

        public Vhdx() => imageInfo = new ImageInfo
        {
            ReadableSectorTags = new List<SectorTagType>(), ReadableMediaTags = new List<MediaTagType>(),
            HasPartitions      = false, HasSessions                           = false, Version = null,
            Application        = null,
            ApplicationVersion = null, Creator = null, Comments = null,
            MediaManufacturer  = null,
            MediaModel         = null, MediaSerialNumber = null, MediaBarcode = null,
            MediaPartNumber    = null,
            MediaSequence      = 0, LastMediaSequence = 0, DriveManufacturer = null,
            DriveModel         = null,
            DriveSerialNumber  = null, DriveFirmwareRevision = null
        };
    }
}