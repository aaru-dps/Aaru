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
//     Reads Basic Lisa Utility disk images.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class Blu
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            _imageHeader = new BluHeader
            {
                DeviceName = new byte[0x0D]
            };

            byte[] header = new byte[0x17];
            stream.Read(header, 0, 0x17);
            Array.Copy(header, 0, _imageHeader.DeviceName, 0, 0x0D);
            _imageHeader.DeviceType    = BigEndianBitConverter.ToUInt32(header, 0x0C) & 0x00FFFFFF;
            _imageHeader.DeviceBlocks  = BigEndianBitConverter.ToUInt32(header, 0x11) & 0x00FFFFFF;
            _imageHeader.BytesPerBlock = BigEndianBitConverter.ToUInt16(header, 0x15);

            AaruConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceName = \"{0}\"",
                                       StringHandlers.CToString(_imageHeader.DeviceName));

            AaruConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceType = {0}", _imageHeader.DeviceType);
            AaruConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceBlock = {0}", _imageHeader.DeviceBlocks);
            AaruConsole.DebugWriteLine("BLU plugin", "ImageHeader.bytesPerBlock = {0}", _imageHeader.BytesPerBlock);

            for(int i = 0; i < 0xD; i++)
                if(_imageHeader.DeviceName[i] < 0x20)
                    return false;

            if((_imageHeader.BytesPerBlock & 0xFE00) != 0x200)
                return false;

            stream.Seek(0, SeekOrigin.Begin);
            header = new byte[_imageHeader.BytesPerBlock];
            stream.Read(header, 0, _imageHeader.BytesPerBlock);

            _imageInfo.SectorSize = 0x200;

            _imageInfo.Sectors   = _imageHeader.DeviceBlocks;
            _imageInfo.ImageSize = _imageHeader.DeviceBlocks * _imageHeader.BytesPerBlock;
            _bptag               = _imageHeader.BytesPerBlock - 0x200;
            byte[] hdrTag = new byte[_bptag];
            Array.Copy(header, 0x200, hdrTag, 0, _bptag);

            switch(StringHandlers.CToString(_imageHeader.DeviceName))
            {
                case PROFILE_NAME:
                    _imageInfo.MediaType =
                        _imageInfo.Sectors == 0x2600 ? MediaType.AppleProfile : MediaType.GENERIC_HDD;

                    _imageInfo.Cylinders       = 152;
                    _imageInfo.Heads           = 4;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case PROFILE10_NAME:
                    _imageInfo.MediaType =
                        _imageInfo.Sectors == 0x4C00 ? MediaType.AppleProfile : MediaType.GENERIC_HDD;

                    _imageInfo.Cylinders       = 304;
                    _imageInfo.Heads           = 4;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case WIDGET_NAME:
                    _imageInfo.MediaType = _imageInfo.Sectors == 0x4C00 ? MediaType.AppleWidget : MediaType.GENERIC_HDD;

                    _imageInfo.Cylinders       = 304;
                    _imageInfo.Heads           = 4;
                    _imageInfo.SectorsPerTrack = 16;

                    break;
                case PRIAM_NAME:
                    _imageInfo.MediaType = _imageInfo.Sectors == 0x022C7C ? MediaType.PriamDataTower
                                               : MediaType.GENERIC_HDD;

                    // This values are invented...
                    _imageInfo.Cylinders       = 419;
                    _imageInfo.Heads           = 4;
                    _imageInfo.SectorsPerTrack = 85;

                    break;
                default:
                    _imageInfo.MediaType       = MediaType.GENERIC_HDD;
                    _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
                    _imageInfo.Heads           = 16;
                    _imageInfo.SectorsPerTrack = 63;

                    break;
            }

            _imageInfo.Application = StringHandlers.CToString(hdrTag);

            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());

            _bluImageFilter = imageFilter;

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            if(_bptag > 0)
                _imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);

            AaruConsole.VerboseWriteLine("BLU image contains a disk of type {0}", _imageInfo.MediaType);

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var buffer = new MemoryStream();
            int seek   = 0;
            int read   = 0x200;
            int skip   = _bptag;

            Stream stream = _bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * _imageHeader.BytesPerBlock), SeekOrigin.Begin);

            for(int i = 0; i < length; i++)
            {
                stream.Seek(seek, SeekOrigin.Current);
                byte[] sector = new byte[read];
                stream.Read(sector, 0, read);
                buffer.Write(sector, 0, read);
                stream.Seek(skip, SeekOrigin.Current);
            }

            return buffer.ToArray();
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            if(_bptag == 0)
                throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var buffer = new MemoryStream();
            int seek   = 0x200;
            int read   = _bptag;
            int skip   = 0;

            Stream stream = _bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * _imageHeader.BytesPerBlock), SeekOrigin.Begin);

            for(int i = 0; i < length; i++)
            {
                stream.Seek(seek, SeekOrigin.Current);
                byte[] sector = new byte[read];
                stream.Read(sector, 0, read);
                buffer.Write(sector, 0, read);
                stream.Seek(skip, SeekOrigin.Current);
            }

            return buffer.ToArray();
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * _imageHeader.BytesPerBlock];
            Stream stream = _bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * _imageHeader.BytesPerBlock), SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }
    }
}