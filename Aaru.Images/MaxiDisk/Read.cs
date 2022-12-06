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
//     Reads MaxiDisk disk images.
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
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class MaxiDisk
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 8)
                return false;

            byte[] buffer = new byte[8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            Header tmpHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(buffer);

            // This is hardcoded
            // But its possible values are unknown...
            //if(tmp_header.diskType > 11)
            //    return false;

            // Only floppies supported
            if(tmpHeader.heads == 0 ||
               tmpHeader.heads > 2)
                return false;

            // No floppies with more than this?
            if(tmpHeader.cylinders > 90)
                return false;

            // Maximum supported bps is 16384
            if(tmpHeader.bytesPerSector > 7)
                return false;

            int expectedFileSize = (tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack *
                                    (128 << tmpHeader.bytesPerSector)) + 8;

            if(expectedFileSize != stream.Length)
                return false;

            _imageInfo.Cylinders       = tmpHeader.cylinders;
            _imageInfo.Heads           = tmpHeader.heads;
            _imageInfo.SectorsPerTrack = tmpHeader.sectorsPerTrack;
            _imageInfo.Sectors         = (ulong)(tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack);
            _imageInfo.SectorSize      = (uint)(128 << tmpHeader.bytesPerSector);

            _hdkImageFilter = imageFilter;

            _imageInfo.ImageSize            = (ulong)(stream.Length - 8);
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

            _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                          (ushort)_imageInfo.SectorsPerTrack, _imageInfo.SectorSize,
                                                          MediaEncoding.MFM, false));

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * _imageInfo.SectorSize];

            Stream stream = _hdkImageFilter.GetDataForkStream();
            stream.Seek((long)(8 + (sectorAddress * _imageInfo.SectorSize)), SeekOrigin.Begin);
            stream.Read(buffer, 0, (int)(length * _imageInfo.SectorSize));

            return buffer;
        }
    }
}