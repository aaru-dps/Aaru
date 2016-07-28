// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ImageInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines a common structure with information about a disk image.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.ImagePlugins
{
    public struct ImageInfo
    {
        public bool imageHasPartitions;
        public bool imageHasSessions;
        public ulong imageSize;
        public ulong sectors;
        public uint sectorSize;
        public List<MediaTagType> readableMediaTags;
        public List<SectorTagType> readableSectorTags;
        public string imageVersion;
        public string imageApplication;
        public string imageApplicationVersion;
        public string imageCreator;
        public DateTime imageCreationTime;
        public DateTime imageLastModificationTime;
        public string imageName;
        public string imageComments;
        public string mediaManufacturer;
        public string mediaModel;
        public string mediaSerialNumber;
        public string mediaBarcode;
        public string mediaPartNumber;
        public MediaType mediaType;
        public int mediaSequence;
        public int lastMediaSequence;
        public string driveManufacturer;
        public string driveModel;
        public string driveSerialNumber;
        public XmlMediaType xmlMediaType;
    }
}

