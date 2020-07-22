// /***************************************************************************
// Aaru Data Preservation Suite
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
    public sealed partial class Anex86
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < Marshal.SizeOf<Anex86Header>())
                return false;

            byte[] hdrB = new byte[Marshal.SizeOf<Anex86Header>()];
            stream.Read(hdrB, 0, hdrB.Length);

            _fdihdr = Marshal.SpanToStructureLittleEndian<Anex86Header>(hdrB);

            _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_fdihdr.cylinders, (byte)_fdihdr.heads,
                                                          (ushort)_fdihdr.spt, (uint)_fdihdr.bps, MediaEncoding.MFM,
                                                          false));

            if(_imageInfo.MediaType == MediaType.Unknown)
                _imageInfo.MediaType = MediaType.GENERIC_HDD;

            AaruConsole.DebugWriteLine("Anex86 plugin", "MediaType: {0}", _imageInfo.MediaType);

            _imageInfo.ImageSize            = (ulong)_fdihdr.dskSize;
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            _imageInfo.Sectors              = (ulong)(_fdihdr.cylinders * _fdihdr.heads * _fdihdr.spt);
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            _imageInfo.SectorSize           = (uint)_fdihdr.bps;
            _imageInfo.Cylinders            = (uint)_fdihdr.cylinders;
            _imageInfo.Heads                = (uint)_fdihdr.heads;
            _imageInfo.SectorsPerTrack      = (uint)_fdihdr.spt;

            _anexImageFilter = imageFilter;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * _imageInfo.SectorSize];

            Stream stream = _anexImageFilter.GetDataForkStream();

            stream.Seek((long)((ulong)_fdihdr.hdrSize + (sectorAddress * _imageInfo.SectorSize)), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * _imageInfo.SectorSize));

            return buffer;
        }
    }
}