// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Anex86 disk images.
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
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class Anex86
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < Marshal.SizeOf<Anex86Header>()) return false;

            byte[] hdrB = new byte[Marshal.SizeOf<Anex86Header>()];
            stream.Read(hdrB, 0, hdrB.Length);

            fdihdr = Marshal.SpanToStructureLittleEndian<Anex86Header>(hdrB);

            imageInfo.MediaType = Geometry.GetMediaType(((ushort)fdihdr.cylinders, (byte)fdihdr.heads,
                                                         (ushort)fdihdr.spt, (uint)fdihdr.bps, MediaEncoding.MFM,
                                                         false));
            if(imageInfo.MediaType == MediaType.Unknown) imageInfo.MediaType = MediaType.GENERIC_HDD;

            DicConsole.DebugWriteLine("Anex86 plugin", "MediaType: {0}", imageInfo.MediaType);

            imageInfo.ImageSize            = (ulong)fdihdr.dskSize;
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = (ulong)(fdihdr.cylinders * fdihdr.heads * fdihdr.spt);
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.SectorSize           = (uint)fdihdr.bps;
            imageInfo.Cylinders            = (uint)fdihdr.cylinders;
            imageInfo.Heads                = (uint)fdihdr.heads;
            imageInfo.SectorsPerTrack      = (uint)fdihdr.spt;

            anexImageFilter = imageFilter;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Stream stream = anexImageFilter.GetDataForkStream();

            stream.Seek((long)((ulong)fdihdr.hdrSize + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }
    }
}