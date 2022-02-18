// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Unsupported.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains features unsupported by Apridisk disk images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;

namespace Aaru.DiscImages
{
    public sealed partial class Apridisk
    {
        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
    }
}