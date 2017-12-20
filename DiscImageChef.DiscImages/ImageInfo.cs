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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.DiscImages
{
    public struct ImageInfo
    {
        public bool ImageHasPartitions;
        public bool ImageHasSessions;
        public ulong ImageSize;
        public ulong Sectors;
        public uint SectorSize;
        public List<MediaTagType> ReadableMediaTags;
        public List<SectorTagType> ReadableSectorTags;
        public string ImageVersion;
        public string ImageApplication;
        public string ImageApplicationVersion;
        public string ImageCreator;
        public DateTime ImageCreationTime;
        public DateTime ImageLastModificationTime;
        public string ImageName;
        public string ImageComments;
        public string MediaManufacturer;
        public string MediaModel;
        public string MediaSerialNumber;
        public string MediaBarcode;
        public string MediaPartNumber;
        public MediaType MediaType;
        public int MediaSequence;
        public int LastMediaSequence;
        public string DriveManufacturer;
        public string DriveModel;
        public string DriveSerialNumber;
        public string DriveFirmwareRevision;
        public XmlMediaType XmlMediaType;
        // CHS geometry...
        public uint Cylinders;
        public uint Heads;
        public uint SectorsPerTrack;
    }
}