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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;

namespace Aaru.DiscImages
{
    public partial class Blu
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            imageHeader = new BluHeader {DeviceName = new byte[0x0D]};

            byte[] header = new byte[0x17];
            stream.Read(header, 0, 0x17);
            Array.Copy(header, 0, imageHeader.DeviceName, 0, 0x0D);
            imageHeader.DeviceType    = BigEndianBitConverter.ToUInt32(header, 0x0C) & 0x00FFFFFF;
            imageHeader.DeviceBlocks  = BigEndianBitConverter.ToUInt32(header, 0x11) & 0x00FFFFFF;
            imageHeader.BytesPerBlock = BigEndianBitConverter.ToUInt16(header, 0x15);

            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceName = \"{0}\"",
                                      StringHandlers.CToString(imageHeader.DeviceName));
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceType = {0}",    imageHeader.DeviceType);
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceBlock = {0}",   imageHeader.DeviceBlocks);
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.bytesPerBlock = {0}", imageHeader.BytesPerBlock);

            for(int i = 0; i < 0xD; i++)
                if(imageHeader.DeviceName[i] < 0x20)
                    return false;

            if((imageHeader.BytesPerBlock & 0xFE00) != 0x200) return false;

            stream.Seek(0, SeekOrigin.Begin);
            header = new byte[imageHeader.BytesPerBlock];
            stream.Read(header, 0, imageHeader.BytesPerBlock);

            imageInfo.SectorSize = 0x200;

            imageInfo.Sectors   = imageHeader.DeviceBlocks;
            imageInfo.ImageSize = imageHeader.DeviceBlocks * imageHeader.BytesPerBlock;
            bptag               = imageHeader.BytesPerBlock - 0x200;
            byte[] hdrTag = new byte[bptag];
            Array.Copy(header, 0x200, hdrTag, 0, bptag);

            switch(StringHandlers.CToString(imageHeader.DeviceName))
            {
                case PROFILE_NAME:
                    imageInfo.MediaType =
                        imageInfo.Sectors == 0x2600 ? MediaType.AppleProfile : MediaType.GENERIC_HDD;
                    imageInfo.Cylinders       = 152;
                    imageInfo.Heads           = 4;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case PROFILE10_NAME:
                    imageInfo.MediaType =
                        imageInfo.Sectors == 0x4C00 ? MediaType.AppleProfile : MediaType.GENERIC_HDD;
                    imageInfo.Cylinders       = 304;
                    imageInfo.Heads           = 4;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case WIDGET_NAME:
                    imageInfo.MediaType =
                        imageInfo.Sectors == 0x4C00 ? MediaType.AppleWidget : MediaType.GENERIC_HDD;
                    imageInfo.Cylinders       = 304;
                    imageInfo.Heads           = 4;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case PRIAM_NAME:
                    imageInfo.MediaType =
                        imageInfo.Sectors == 0x022C7C ? MediaType.PriamDataTower : MediaType.GENERIC_HDD;
                    // This values are invented...
                    imageInfo.Cylinders       = 419;
                    imageInfo.Heads           = 4;
                    imageInfo.SectorsPerTrack = 85;
                    break;
                default:
                    imageInfo.MediaType       = MediaType.GENERIC_HDD;
                    imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                    imageInfo.Heads           = 16;
                    imageInfo.SectorsPerTrack = 63;
                    break;
            }

            imageInfo.Application = StringHandlers.CToString(hdrTag);

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());

            bluImageFilter = imageFilter;

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            if(bptag > 0) imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);

            DicConsole.VerboseWriteLine("BLU image contains a disk of type {0}", imageInfo.MediaType);

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            int          seek   = 0;
            int          read   = 0x200;
            int          skip   = bptag;

            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * imageHeader.BytesPerBlock), SeekOrigin.Begin);

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

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            if(bptag == 0) throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            int          seek   = 0x200;
            int          read   = bptag;
            int          skip   = 0;

            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * imageHeader.BytesPerBlock), SeekOrigin.Begin);

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

        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageHeader.BytesPerBlock];
            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * imageHeader.BytesPerBlock), SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }
    }
}