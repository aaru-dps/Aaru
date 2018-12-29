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
//     Reads CisCopy disk images.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;

namespace DiscImageChef.DiscImages
{
    public partial class CisCopy
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            DiskType type = (DiskType)stream.ReadByte();
            byte     tracks;

            switch(type)
            {
                case DiskType.MD1DD8:
                case DiskType.MD1DD:
                case DiskType.MD2DD8:
                case DiskType.MD2DD:
                    tracks = 80;
                    break;
                case DiskType.MF2DD:
                case DiskType.MD2HD:
                case DiskType.MF2HD:
                    tracks = 160;
                    break;
                default: throw new ImageNotSupportedException($"Incorrect disk type {(byte)type}");
            }

            byte[] trackBytes = new byte[tracks];
            stream.Read(trackBytes, 0, tracks);

            Compression cmpr = (Compression)stream.ReadByte();

            if(cmpr != Compression.None)
                throw new FeatureSupportedButNotImplementedImageException("Compressed images are not supported.");

            int tracksize = 0;

            switch(type)
            {
                case DiskType.MD1DD8:
                case DiskType.MD2DD8:
                    tracksize = 8 * 512;
                    break;
                case DiskType.MD1DD:
                case DiskType.MD2DD:
                case DiskType.MF2DD:
                    tracksize = 9 * 512;
                    break;
                case DiskType.MD2HD:
                    tracksize = 15 * 512;
                    break;
                case DiskType.MF2HD:
                    tracksize = 18 * 512;
                    break;
            }

            int headstep                                                   = 1;
            if(type == DiskType.MD1DD || type == DiskType.MD1DD8) headstep = 2;

            MemoryStream decodedImage = new MemoryStream();

            for(int i = 0; i < tracks; i += headstep)
            {
                byte[] track = new byte[tracksize];

                if((TrackType)trackBytes[i] == TrackType.Copied) stream.Read(track, 0, tracksize);
                else ArrayHelpers.ArrayFill(track, (byte)0xF6);

                decodedImage.Write(track, 0, tracksize);
            }

            imageInfo.Application          = "CisCopy";
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = imageFilter.GetFilename();
            imageInfo.ImageSize            = (ulong)(stream.Length - 2 - trackBytes.Length);
            imageInfo.SectorSize           = 512;

            switch(type)
            {
                case DiskType.MD1DD8:
                    imageInfo.MediaType       = MediaType.DOS_525_SS_DD_8;
                    imageInfo.Sectors         = 40 * 1 * 8;
                    imageInfo.Heads           = 1;
                    imageInfo.Cylinders       = 40;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case DiskType.MD2DD8:
                    imageInfo.MediaType       = MediaType.DOS_525_DS_DD_8;
                    imageInfo.Sectors         = 40 * 2 * 8;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 40;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case DiskType.MD1DD:
                    imageInfo.MediaType       = MediaType.DOS_525_SS_DD_9;
                    imageInfo.Sectors         = 40 * 1 * 9;
                    imageInfo.Heads           = 1;
                    imageInfo.Cylinders       = 40;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case DiskType.MD2DD:
                    imageInfo.MediaType       = MediaType.DOS_525_DS_DD_9;
                    imageInfo.Sectors         = 40 * 2 * 9;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 40;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case DiskType.MF2DD:
                    imageInfo.MediaType       = MediaType.DOS_35_DS_DD_9;
                    imageInfo.Sectors         = 80 * 2 * 9;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 80;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case DiskType.MD2HD:
                    imageInfo.MediaType       = MediaType.DOS_525_HD;
                    imageInfo.Sectors         = 80 * 2 * 15;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 80;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case DiskType.MF2HD:
                    imageInfo.MediaType       = MediaType.DOS_35_HD;
                    imageInfo.Sectors         = 80 * 2 * 18;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 80;
                    imageInfo.SectorsPerTrack = 18;
                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            decodedDisk            = decodedImage.ToArray();

            decodedImage.Close();

            DicConsole.VerboseWriteLine("CisCopy image contains a disk of type {0}", imageInfo.MediaType);

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Array.Copy(decodedDisk, (int)sectorAddress * imageInfo.SectorSize, buffer, 0,
                       length                          * imageInfo.SectorSize);

            return buffer;
        }
    }
}