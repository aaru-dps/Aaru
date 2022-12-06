﻿// /***************************************************************************
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
// Copyright © 2011-2023 Natalia Portillo
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
    public sealed partial class CisCopy
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            var  type = (DiskType)stream.ReadByte();
            byte tracks;

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

            var cmpr = (Compression)stream.ReadByte();

            if(cmpr != Compression.None)
                throw new FeatureSupportedButNotImplementedImageException("Compressed images are not supported.");

            int trackSize = 0;

            switch(type)
            {
                case DiskType.MD1DD8:
                case DiskType.MD2DD8:
                    trackSize = 8 * 512;

                    break;
                case DiskType.MD1DD:
                case DiskType.MD2DD:
                case DiskType.MF2DD:
                    trackSize = 9 * 512;

                    break;
                case DiskType.MD2HD:
                    trackSize = 15 * 512;

                    break;
                case DiskType.MF2HD:
                    trackSize = 18 * 512;

                    break;
            }

            int headStep = 1;

            if(type == DiskType.MD1DD ||
               type == DiskType.MD1DD8)
                headStep = 2;

            var decodedImage = new MemoryStream();

            for(int i = 0; i < tracks; i += headStep)
            {
                byte[] track = new byte[trackSize];

                if((TrackType)trackBytes[i] == TrackType.Copied)
                    stream.Read(track, 0, trackSize);
                else
                    ArrayHelpers.ArrayFill(track, (byte)0xF6);

                decodedImage.Write(track, 0, trackSize);
            }

            _imageInfo.Application          = "CisCopy";
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = imageFilter.GetFilename();
            _imageInfo.ImageSize            = (ulong)(stream.Length - 2 - trackBytes.Length);
            _imageInfo.SectorSize           = 512;

            switch(type)
            {
                case DiskType.MD1DD8:
                    _imageInfo.MediaType       = MediaType.DOS_525_SS_DD_8;
                    _imageInfo.Sectors         = 40 * 1 * 8;
                    _imageInfo.Heads           = 1;
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case DiskType.MD2DD8:
                    _imageInfo.MediaType       = MediaType.DOS_525_DS_DD_8;
                    _imageInfo.Sectors         = 40 * 2 * 8;
                    _imageInfo.Heads           = 2;
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.SectorsPerTrack = 8;

                    break;
                case DiskType.MD1DD:
                    _imageInfo.MediaType       = MediaType.DOS_525_SS_DD_9;
                    _imageInfo.Sectors         = 40 * 1 * 9;
                    _imageInfo.Heads           = 1;
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case DiskType.MD2DD:
                    _imageInfo.MediaType       = MediaType.DOS_525_DS_DD_9;
                    _imageInfo.Sectors         = 40 * 2 * 9;
                    _imageInfo.Heads           = 2;
                    _imageInfo.Cylinders       = 40;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case DiskType.MF2DD:
                    _imageInfo.MediaType       = MediaType.DOS_35_DS_DD_9;
                    _imageInfo.Sectors         = 80 * 2 * 9;
                    _imageInfo.Heads           = 2;
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.SectorsPerTrack = 9;

                    break;
                case DiskType.MD2HD:
                    _imageInfo.MediaType       = MediaType.DOS_525_HD;
                    _imageInfo.Sectors         = 80 * 2 * 15;
                    _imageInfo.Heads           = 2;
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.SectorsPerTrack = 15;

                    break;
                case DiskType.MF2HD:
                    _imageInfo.MediaType       = MediaType.DOS_35_HD;
                    _imageInfo.Sectors         = 80 * 2 * 18;
                    _imageInfo.Heads           = 2;
                    _imageInfo.Cylinders       = 80;
                    _imageInfo.SectorsPerTrack = 18;

                    break;
            }

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            _decodedDisk            = decodedImage.ToArray();

            decodedImage.Close();

            AaruConsole.VerboseWriteLine("CisCopy image contains a disk of type {0}", _imageInfo.MediaType);

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

            Array.Copy(_decodedDisk, (int)sectorAddress * _imageInfo.SectorSize, buffer, 0,
                       length                           * _imageInfo.SectorSize);

            return buffer;
        }
    }
}