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
//     Contains properties for DIM disk images.
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
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public sealed partial class Dim
    {
        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        /// <inheritdoc />
        public string Name => "DIM Disk Image";
        /// <inheritdoc />
        public Guid Id => new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");
        /// <inheritdoc />
        public ImageInfo Info => _imageInfo;
        /// <inheritdoc />
        public string Author => "Natalia Portillo";
        /// <inheritdoc />
        public string Format => "DIM disk image";
        /// <inheritdoc />
        public List<DumpHardwareType> DumpHardware => null;
        /// <inheritdoc />
        public CICMMetadataType CicmMetadata => null;
    }
}