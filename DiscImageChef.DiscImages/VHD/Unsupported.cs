// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Unsupported.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains features unsupported by Connectix and Microsoft Virtual PC disk images.
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

using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.DiscImages
{
    public partial class Vhd
    {
        public byte[] ReadDiskTag(MediaTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSector(ulong sectorAddress, uint track) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSectorLong(ulong sectorAddress) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSectorLong(ulong sectorAddress, uint track) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> GetSessionTracks(Session session) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> GetSessionTracks(ushort session) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool? VerifySector(ulong sectorAddress) => null;

        public bool? VerifySector(ulong sectorAddress, uint track) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas) =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool? VerifyMediaImage() => null;
    }
}