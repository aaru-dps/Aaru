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
//     Contains properties for XGS emulator disk images.
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
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class Apple2Mg
    {
        public ImageInfo Info => imageInfo;

        public string Name   => "Apple 2IMG";
        public Guid   Id     => new Guid("CBAF8824-BA5F-415F-953A-19A03519B2D1");
        public string Author => "Natalia Portillo";
        public string Format => "Apple 2IMG";

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[] { };
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.Apple32SS, MediaType.Apple33SS, MediaType.AppleSonySS, MediaType.AppleSonyDS,
                MediaType.DOS_35_HD, MediaType.Unknown, MediaType.GENERIC_HDD, MediaType.FlashDrive,
                MediaType.CompactFlash, MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII,
                MediaType.PCCardTypeIII, MediaType.PCCardTypeIV
            };
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
            new (string name, Type type, string description, object @default)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".2mg"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }
    }
}