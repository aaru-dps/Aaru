// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for Basic Lisa Utility disk images.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class Blu
    {
        public ImageInfo Info   => imageInfo;
        public string    Name   => "Basic Lisa Utility";
        public Guid      Id     => new Guid("A153E2F8-4235-432D-9A7F-20807B0BCD74");
        public string    Author => "Natalia Portillo";
        public string    Format => "Basic Lisa Utility";
        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        public List<DumpHardwareType>     DumpHardware        => null;
        public CICMMetadataType           CicmMetadata        => null;
        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new[] {SectorTagType.AppleSectorTag};
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.AppleProfile, MediaType.AppleWidget, MediaType.PriamDataTower, MediaType.GENERIC_HDD,
                MediaType.Unknown, MediaType.FlashDrive, MediaType.CompactFlash, MediaType.CompactFlashType2,
                MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII, MediaType.PCCardTypeIV
            };
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
            new (string name, Type type, string description, object @default)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".blu"}; // Just invented
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }
    }
}