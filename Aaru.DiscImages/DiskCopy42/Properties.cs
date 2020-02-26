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
//     Contains properties for Apple DiskCopy 4.2 disk images.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class DiskCopy42
    {
        public ImageInfo                  Info                => imageInfo;
        public string                     Name                => "Apple DiskCopy 4.2";
        public Guid                       Id                  => new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
        public string                     Author              => "Natalia Portillo";
        public List<DumpHardwareType>     DumpHardware        => null;
        public CICMMetadataType           CicmMetadata        => null;
        public string                     Format              => "Apple DiskCopy 4.2";
        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new[] {SectorTagType.AppleSectorTag};
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.AppleFileWare, MediaType.AppleHD20, MediaType.AppleProfile, MediaType.AppleSonyDS,
                MediaType.AppleSonySS, MediaType.AppleWidget, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,
                MediaType.DMF
            };
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
            new[] {("macosx", typeof(bool), "Use Mac OS X format byte", (object)false)};
        public IEnumerable<string> KnownExtensions => new[] {".dc42", ".diskcopy42", ".image"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }
    }
}