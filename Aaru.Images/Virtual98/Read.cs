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
//     Reads Virtual98 disk images.
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
using System.IO;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class Virtual98
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            var shiftjis = Encoding.GetEncoding("shift_jis");

            if(stream.Length < Marshal.SizeOf<Virtual98Header>())
                return false;

            byte[] hdrB = new byte[Marshal.SizeOf<Virtual98Header>()];
            stream.Read(hdrB, 0, hdrB.Length);

            _v98Hdr = Marshal.ByteArrayToStructureLittleEndian<Virtual98Header>(hdrB);

            _imageInfo.MediaType = MediaType.GENERIC_HDD;

            _imageInfo.ImageSize            = (ulong)(stream.Length - 0xDC);
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            _imageInfo.Sectors              = _v98Hdr.totals;
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            _imageInfo.SectorSize           = _v98Hdr.sectorsize;
            _imageInfo.Cylinders            = _v98Hdr.cylinders;
            _imageInfo.Heads                = _v98Hdr.surfaces;
            _imageInfo.SectorsPerTrack      = _v98Hdr.sectors;
            _imageInfo.Comments             = StringHandlers.CToString(_v98Hdr.comment, shiftjis);

            _nhdImageFilter = imageFilter;

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

            Stream stream = _nhdImageFilter.GetDataForkStream();

            // V98 are lazy allocated
            if((long)(0xDC + (sectorAddress * _imageInfo.SectorSize)) >= stream.Length)
                return buffer;

            stream.Seek((long)(0xDC + (sectorAddress * _imageInfo.SectorSize)), SeekOrigin.Begin);

            int toRead = (int)(length * _imageInfo.SectorSize);

            if(toRead + stream.Position > stream.Length)
                toRead = (int)(stream.Length - stream.Position);

            stream.Read(buffer, 0, toRead);

            return buffer;
        }
    }
}