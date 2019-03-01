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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Helpers;

namespace DiscImageChef.DiscImages
{
    public partial class RsIde
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[Marshal.SizeOf<RsIdeHeader>()];
            stream.Read(hdrB, 0, hdrB.Length);

            RsIdeHeader hdr = Marshal.ByteArrayToStructureLittleEndian<RsIdeHeader>(hdrB);

            if(!hdr.magic.SequenceEqual(signature)) return false;

            dataOff = hdr.dataOff;

            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.SectorSize           = (uint)(hdr.flags.HasFlag(RsIdeFlags.HalfSectors) ? 256 : 512);
            imageInfo.ImageSize            = (ulong)(stream.Length - dataOff);
            imageInfo.Sectors              = imageInfo.ImageSize / imageInfo.SectorSize;
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.Version              = $"{hdr.revision >> 8}.{hdr.revision & 0x0F}";

            if(!ArrayHelpers.ArrayIsNullOrEmpty(hdr.identify))
            {
                identify = new byte[512];
                Array.Copy(hdr.identify, 0, identify, 0, hdr.identify.Length);
                Identify.IdentifyDevice? ataId = Decoders.ATA.Identify.Decode(identify);

                if(ataId.HasValue)
                {
                    imageInfo.ReadableMediaTags.Add(MediaTagType.ATA_IDENTIFY);
                    imageInfo.Cylinders             = ataId.Value.Cylinders;
                    imageInfo.Heads                 = ataId.Value.Heads;
                    imageInfo.SectorsPerTrack       = ataId.Value.SectorsPerCard;
                    imageInfo.DriveFirmwareRevision = ataId.Value.FirmwareRevision;
                    imageInfo.DriveModel            = ataId.Value.Model;
                    imageInfo.DriveSerialNumber     = ataId.Value.SerialNumber;
                    imageInfo.MediaSerialNumber     = ataId.Value.MediaSerial;
                    imageInfo.MediaManufacturer     = ataId.Value.MediaManufacturer;
                }
            }

            if(imageInfo.Cylinders == 0 || imageInfo.Heads == 0 || imageInfo.SectorsPerTrack == 0)
            {
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads           = 16;
                imageInfo.SectorsPerTrack = 63;
            }

            rsIdeImageFilter = imageFilter;

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

            Stream stream = rsIdeImageFilter.GetDataForkStream();

            stream.Seek((long)(dataOff + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(!imageInfo.ReadableMediaTags.Contains(tag) || tag != MediaTagType.ATA_IDENTIFY)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            byte[] buffer = new byte[512];
            Array.Copy(identify, 0, buffer, 0, 512);
            return buffer;
        }
    }
}