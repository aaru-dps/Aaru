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
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class CisCopy
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
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
            default:
                AaruConsole.ErrorWriteLine(string.Format(Localization.Incorrect_disk_type_0, (byte)type));

                return ErrorNumber.InvalidArgument;
        }

        byte[] trackBytes = new byte[tracks];
        stream.EnsureRead(trackBytes, 0, tracks);

        var cmpr = (Compression)stream.ReadByte();

        if(cmpr != Compression.None)
        {
            AaruConsole.ErrorWriteLine(Localization.Compressed_images_are_not_supported);

            return ErrorNumber.NotImplemented;
        }

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

        if(type is DiskType.MD1DD or DiskType.MD1DD8)
            headStep = 2;

        var decodedImage = new MemoryStream();

        for(int i = 0; i < tracks; i += headStep)
        {
            byte[] track = new byte[trackSize];

            if((TrackType)trackBytes[i] == TrackType.Copied)
                stream.EnsureRead(track, 0, trackSize);
            else
                ArrayHelpers.ArrayFill(track, (byte)0xF6);

            decodedImage.Write(track, 0, trackSize);
        }

        _imageInfo.Application          = "CisCopy";
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = imageFilter.Filename;
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

        AaruConsole.VerboseWriteLine(Localization.CisCopy_image_contains_a_disk_of_type_0, _imageInfo.MediaType);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        buffer = new byte[length * _imageInfo.SectorSize];

        Array.Copy(_decodedDisk, (int)sectorAddress * _imageInfo.SectorSize, buffer, 0, length * _imageInfo.SectorSize);

        return ErrorNumber.NoError;
    }
}