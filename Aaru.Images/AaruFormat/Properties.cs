// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for Aaru Format disk images.
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public partial class AaruFormat
    {
        public OpticalImageCapabilities OpticalCapabilities => OpticalImageCapabilities.CanStoreAudioTracks    |
                                                               OpticalImageCapabilities.CanStoreDataTracks     |
                                                               OpticalImageCapabilities.CanStorePregaps        |
                                                               OpticalImageCapabilities.CanStoreSubchannelRw   |
                                                               OpticalImageCapabilities.CanStoreSessions       |
                                                               OpticalImageCapabilities.CanStoreIsrc           |
                                                               OpticalImageCapabilities.CanStoreCdText         |
                                                               OpticalImageCapabilities.CanStoreMcn            |
                                                               OpticalImageCapabilities.CanStoreRawData        |
                                                               OpticalImageCapabilities.CanStoreCookedData     |
                                                               OpticalImageCapabilities.CanStoreMultipleTracks |
                                                               OpticalImageCapabilities.CanStoreNotCdSessions  |
                                                               OpticalImageCapabilities.CanStoreNotCdTracks;
        public ImageInfo              Info         => imageInfo;
        public string                 Name         => "Aaru Format";
        public Guid                   Id           => new Guid("49360069-1784-4A2F-B723-0C844D610B0A");
        public string                 Format       => "Aaru";
        public string                 Author       => "Natalia Portillo";
        public List<Partition>        Partitions   { get; private set; }
        public List<Track>            Tracks       { get; private set; }
        public List<Session>          Sessions     { get; private set; }
        public List<DumpHardwareType> DumpHardware { get; private set; }
        public CICMMetadataType       CicmMetadata { get; private set; }
        public IEnumerable<MediaTagType> SupportedMediaTags =>
            Enum.GetValues(typeof(MediaTagType)).Cast<MediaTagType>();
        public IEnumerable<SectorTagType> SupportedSectorTags =>
            Enum.GetValues(typeof(SectorTagType)).Cast<SectorTagType>();
        public IEnumerable<MediaType> SupportedMediaTypes => Enum.GetValues(typeof(MediaType)).Cast<MediaType>();
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions => new[]
        {
            ("sectors_per_block", typeof(uint),
             "How many sectors to store per block (will be rounded to next power of two)", 4096U),
            ("dictionary", typeof(uint), "Size, in bytes, of the LZMA dictionary", (uint)(1 << 25)),
            ("max_ddt_size", typeof(uint),
             "Maximum size, in mebibytes, for in-memory DDT. If image needs a bigger one, it will be on-disk", 256U),
            ("md5", typeof(bool), "Calculate and store MD5 of image's user data", (object)false),
            ("sha1", typeof(bool), "Calculate and store SHA1 of image's user data", (object)false),
            ("sha256", typeof(bool), "Calculate and store SHA256 of image's user data", (object)false),
            ("spamsum", typeof(bool), "Calculate and store SpamSum of image's user data", (object)false),
            ("deduplicate", typeof(bool),
             "Store only unique sectors. This consumes more memory and is slower, but it's enabled by default",
             (object)true),
            ("compress", typeof(bool), "Compress user data blocks. Other blocks will always be compressed",
             (object)true)
        };
        public IEnumerable<string> KnownExtensions => new[]
        {
            ".dicf", ".aaru", ".aaruformat", ".aaruf", ".aif"
        };
        public bool   IsWriting    { get; private set; }
        public string ErrorMessage { get; private set; }
    }
}