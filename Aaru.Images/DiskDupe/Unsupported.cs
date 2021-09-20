// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Unsupported.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains features unsupported by DiskDupe DDI disk images.
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
// Copyright © 2021 Michael Drüing
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;

namespace Aaru.DiscImages
{
    public sealed partial class DiskDupe
    {
        /// <inheritdoc />
        public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
        {
            buffer = null;

            return ErrorNumber.NotSupported;
        }

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        /// <inheritdoc />
        public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer)
        {
            buffer = null;

            return ErrorNumber.NotSupported;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
        {
            buffer = null;

            return ErrorNumber.NotSupported;
        }
    }
}