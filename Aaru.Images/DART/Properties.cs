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
//     Contains properties for Apple Disk Archival/Retrieval Tool format.
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
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public sealed partial class Dart
    {
        /// <inheritdoc />
        public string                 Name         => "Apple Disk Archival/Retrieval Tool";
        /// <inheritdoc />
        public Guid                   Id           => new Guid("B3E06BF8-F98D-4F9B-BBE2-342C373BAF3E");
        /// <inheritdoc />
        public ImageInfo              Info         => _imageInfo;
        /// <inheritdoc />
        public string                 Author       => "Natalia Portillo";
        /// <inheritdoc />
        public string                 Format       => "Apple Disk Archival/Retrieval Tool";
        /// <inheritdoc />
        public List<DumpHardwareType> DumpHardware => null;
        /// <inheritdoc />
        public CICMMetadataType       CicmMetadata => null;
    }
}