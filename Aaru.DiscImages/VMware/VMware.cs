// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.DiscImages
{
    public partial class VMware : IWritableImage
    {
        string                          adapter_type;
        uint                            cid;
        StreamWriter                    descriptorStream;
        Dictionary<ulong, VMwareExtent> extents;
        IFilter                         gdFilter;
        Dictionary<ulong, byte[]>       grainCache;
        ulong                           grainSize;
        uint[]                          gTable;
        bool                            hasParent;
        uint                            hwversion;
        ImageInfo                       imageInfo;
        string                          imageType;
        uint                            maxCachedGrains;
        uint                            parentCid;
        IMediaImage                     parentImage;
        string                          parentName;
        Dictionary<ulong, byte[]>       sectorCache;
        uint                            version;
        VMwareCowHeader                 vmCHdr;
        VMwareExtentHeader              vmEHdr;
        string                          writingBaseName;
        FileStream                      writingStream;

        public VMware()
        {
            imageInfo = new ImageInfo
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
    }
}