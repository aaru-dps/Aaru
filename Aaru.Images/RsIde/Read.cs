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
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class RsIde
    {
        /// <inheritdoc />
        public ErrorNumber Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[Marshal.SizeOf<Header>()];
            stream.Read(hdrB, 0, hdrB.Length);

            Header hdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrB);

            if(!hdr.magic.SequenceEqual(_signature))
                return ErrorNumber.InvalidArgument;

            _dataOff = hdr.dataOff;

            _imageInfo.MediaType            = MediaType.GENERIC_HDD;
            _imageInfo.SectorSize           = (uint)(hdr.flags.HasFlag(RsIdeFlags.HalfSectors) ? 256 : 512);
            _imageInfo.ImageSize            = (ulong)(stream.Length - _dataOff);
            _imageInfo.Sectors              = _imageInfo.ImageSize / _imageInfo.SectorSize;
            _imageInfo.CreationTime         = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
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

            Stream stream = _rsIdeImageFilter.GetDataForkStream();

            stream.Seek((long)(_dataOff + (sectorAddress * _imageInfo.SectorSize)), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * _imageInfo.SectorSize));

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
        {
            buffer = null;

            if(!_imageInfo.ReadableMediaTags.Contains(tag) ||
               tag != MediaTagType.ATA_IDENTIFY)
                return ErrorNumber.NotSupported;

            buffer = _identify?.Clone() as byte[];

            return buffer is null ? ErrorNumber.NoData : ErrorNumber.NoError;
        }
    }
}