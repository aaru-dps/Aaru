﻿// /***************************************************************************
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
//     Contains properties for VirtualBox disk images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public sealed partial class Vdi
    {
        /// <inheritdoc />
        public ImageInfo Info => _imageInfo;

        /// <inheritdoc />
        public string Name => "VirtualBox Disk Image";
        /// <inheritdoc />
        public Guid Id => new Guid("E314DE35-C103-48A3-AD36-990F68523C46");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";
        /// <inheritdoc />
        public string Format => "VDI";
        /// <inheritdoc />
        public List<DumpHardwareType> DumpHardware => null;
        /// <inheritdoc />
        public CICMMetadataType CicmMetadata => null;
        /// <inheritdoc />
        public IEnumerable<MediaTagType> SupportedMediaTags => new MediaTagType[]
            {};
        /// <inheritdoc />
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[]
            {};
        /// <inheritdoc />
        public IEnumerable<MediaType> SupportedMediaTypes => new[]
        {
            MediaType.Unknown, MediaType.GENERIC_HDD, MediaType.FlashDrive, MediaType.CompactFlash,
            MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII,
            MediaType.PCCardTypeIV
        };

        // TODO: Add cluster size option
        /// <inheritdoc />
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
            new (string name, Type type, string description, object @default)[]
                {};
        /// <inheritdoc />
        public IEnumerable<string> KnownExtensions => new[]
        {
            ".vdi"
        };
        /// <inheritdoc />
        public bool IsWriting { get; private set; }
        /// <inheritdoc />
        public string ErrorMessage { get; private set; }
    }
}