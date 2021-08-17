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
//     Reads RS-IDE disk images.
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class RsIde
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[Marshal.SizeOf<Header>()];
            stream.Read(hdrB, 0, hdrB.Length);

            Header hdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrB);

            if(!hdr.magic.SequenceEqual(_signature))
                return false;

            _dataOff = hdr.dataOff;

            _imageInfo.MediaType            = MediaType.GENERIC_HDD;
            _imageInfo.SectorSize           = (uint)(hdr.flags.HasFlag(RsIdeFlags.HalfSectors) ? 256 : 512);
            _imageInfo.ImageSize            = (ulong)(stream.Length - _dataOff);
            _imageInfo.Sectors              = _imageInfo.ImageSize / _imageInfo.SectorSize;
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            _imageInfo.Version              = $"{hdr.revision >> 8}.{hdr.revision & 0x0F}";

            if(!ArrayHelpers.ArrayIsNullOrEmpty(hdr.identify))
            {
                _identify = new byte[512];
                Array.Copy(hdr.identify, 0, _identify, 0, hdr.identify.Length);
                Identify.IdentifyDevice? ataId = CommonTypes.Structs.Devices.ATA.Identify.Decode(_identify);

                if(ataId.HasValue)
                {
                    _imageInfo.ReadableMediaTags.Add(MediaTagType.ATA_IDENTIFY);
                    _imageInfo.Cylinders             = ataId.Value.Cylinders;
                    _imageInfo.Heads                 = ataId.Value.Heads;
                    _imageInfo.SectorsPerTrack       = ataId.Value.SectorsPerCard;
                    _imageInfo.DriveFirmwareRevision = ataId.Value.FirmwareRevision;
                    _imageInfo.DriveModel            = ataId.Value.Model;
                    _imageInfo.DriveSerialNumber     = ataId.Value.SerialNumber;
                    _imageInfo.MediaSerialNumber     = ataId.Value.MediaSerial;
                    _imageInfo.MediaManufacturer     = ataId.Value.MediaManufacturer;
                }
            }

            if(_imageInfo.Cylinders       == 0 ||
               _imageInfo.Heads           == 0 ||
               _imageInfo.SectorsPerTrack == 0)
            {
                _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
                _imageInfo.Heads           = 16;
                _imageInfo.SectorsPerTrack = 63;
            }

            _rsIdeImageFilter = imageFilter;

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

            Stream stream = _rsIdeImageFilter.GetDataForkStream();

            stream.Seek((long)(_dataOff + (sectorAddress * _imageInfo.SectorSize)), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * _imageInfo.SectorSize));

            return buffer;
        }

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(!_imageInfo.ReadableMediaTags.Contains(tag) ||
               tag != MediaTagType.ATA_IDENTIFY)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            byte[] buffer = new byte[512];
            Array.Copy(_identify, 0, buffer, 0, 512);

            return buffer;
        }
    }
}