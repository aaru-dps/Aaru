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
//     Reads Ray Arachelian's disk images.
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

using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class RayDim
    {
        /// <inheritdoc />
        public ErrorNumber Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < Marshal.SizeOf<Header>())
                return ErrorNumber.InvalidArgument;

            byte[] buffer = new byte[Marshal.SizeOf<Header>()];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            Header header = Marshal.ByteArrayToStructureLittleEndian<Header>(buffer);

            string signature = StringHandlers.CToString(header.signature);

            var   sx = new Regex(REGEX_SIGNATURE);
            Match sm = sx.Match(signature);

            if(!sm.Success)
                return ErrorNumber.InvalidArgument;

            _imageInfo.ApplicationVersion = $"{sm.Groups["major"].Value}.{sm.Groups["minor"].Value}";

            _imageInfo.Cylinders       = (uint)(header.cylinders + 1);
            _imageInfo.Heads           = (uint)(header.heads     + 1);
            _imageInfo.SectorsPerTrack = header.sectorsPerTrack;
            _imageInfo.Sectors         = _imageInfo.Cylinders * _imageInfo.Heads * _imageInfo.SectorsPerTrack;
            _imageInfo.SectorSize      = 512;

            byte[] sectors = new byte[_imageInfo.SectorsPerTrack * _imageInfo.SectorSize];
            _disk = new MemoryStream();

            for(int i = 0; i < _imageInfo.SectorsPerTrack * _imageInfo.SectorSize; i++)
            {
                stream.Read(sectors, 0, sectors.Length);
                stream.Seek(_imageInfo.SectorsPerTrack, SeekOrigin.Current);
                _disk.Write(sectors, 0, sectors.Length);
            }

            _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                          (ushort)_imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                          false));

            switch(_imageInfo.MediaType)
            {
                case MediaType.NEC_525_HD
                    when header.diskType == RayDiskTypes.Mf2hd || header.diskType == RayDiskTypes.Mf2ed:
                    _imageInfo.MediaType = MediaType.NEC_35_HD_8;

                    break;
                case MediaType.DOS_525_HD
                    when header.diskType == RayDiskTypes.Mf2hd || header.diskType == RayDiskTypes.Mf2ed:
                    _imageInfo.MediaType = MediaType.NEC_35_HD_15;

                    break;
                case MediaType.RX50 when header.diskType == RayDiskTypes.Md2dd || header.diskType == RayDiskTypes.Md2hd:
                    _imageInfo.MediaType = MediaType.ATARI_35_SS_DD;

                    break;
            }

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) =>
            ReadSectors(sectorAddress, 1, out buffer);

        /// <inheritdoc />
        public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
        {
            buffer = null;

            if(sectorAddress > _imageInfo.Sectors - 1)
                return ErrorNumber.OutOfRange;

            if(sectorAddress + length > _imageInfo.Sectors)
                return ErrorNumber.OutOfRange;

            buffer = new byte[length * _imageInfo.SectorSize];

            _disk.Seek((long)(sectorAddress    * _imageInfo.SectorSize), SeekOrigin.Begin);
            _disk.Read(buffer, 0, (int)(length * _imageInfo.SectorSize));

            return ErrorNumber.NoError;
        }
    }
}