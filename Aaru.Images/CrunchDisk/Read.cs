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
//     Reads CrunchDisk disk images.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Images;

public sealed partial class CrunchDisk
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        if(stream.Length < HEADER_SIZE) return ErrorNumber.InvalidArgument;

        stream.Seek(0, SeekOrigin.Begin);

        var headerBytes = new byte[HEADER_SIZE];
        stream.EnsureRead(headerBytes, 0, HEADER_SIZE);

        _header = Marshal.ByteArrayToStructureBigEndian<Header>(headerBytes);

        if(_header.Id != HEADER_MAGIC) return ErrorNumber.InvalidArgument;

        if(_header.IsPassword != 0)
        {
            AaruLogging.Debug(MODULE_NAME, Localization.CrunchDisk_image_is_password_protected);

            return ErrorNumber.NotSupported;
        }

        if(_header.PackerType > (ushort)PackerType.PowerPacker)
        {
            AaruLogging.Debug(MODULE_NAME, Localization.CrunchDisk_unsupported_packer_type_0, _header.PackerType);

            return ErrorNumber.NotSupported;
        }

        if(_header.HighCyl        < _header.LowCyl ||
           _header.Heads          == 0             ||
           _header.BlocksPerTrack == 0             ||
           _header.BlockSize      == 0)
            return ErrorNumber.InvalidArgument;

        uint  cylinders     = _header.HighCyl - _header.LowCyl + 1;
        uint  sectorsPerCyl = _header.BlocksPerTrack * _header.Heads;
        uint  cylinderSize  = sectorsPerCyl          * _header.BlockSize;
        ulong totalSectors  = (ulong)cylinders       * sectorsPerCyl;

        // Reject geometries the file cannot plausibly hold; also prevents huge hostile allocations
        if(totalSectors * _header.BlockSize > int.MaxValue) return ErrorNumber.InvalidArgument;

        AaruLogging.Debug(MODULE_NAME,
                          Localization.CrunchDisk_0_cylinders_1_heads_2_sectors_per_track_3_bytes_per_sector,
                          cylinders,
                          _header.Heads,
                          _header.BlocksPerTrack,
                          _header.BlockSize);

        // Compute PowerPacker offset sizes if needed
        byte[] offsetSizes = null;

        if(_header.PackerType == (ushort)PackerType.PowerPacker) offsetSizes = ComputeOffsetSizes(_header.Efficiency);

        // Allocate buffer for the full decompressed disk data
        _decompressedData = new byte[totalSectors * _header.BlockSize];

        var cylHeaderBytes = new byte[CYLINDER_HEADER_SIZE];
        var outputOffset   = 0;

        for(uint cyl = _header.LowCyl; cyl <= _header.HighCyl; cyl++)
        {
            stream.EnsureRead(cylHeaderBytes, 0, CYLINDER_HEADER_SIZE);

            var cylMagic   = BigEndianBitConverter.ToUInt32(cylHeaderBytes, 0);
            var cylDataLen = BigEndianBitConverter.ToUInt32(cylHeaderBytes, 4);

            switch(cylMagic)
            {
                case CYL_UNCOMPRESSED:
                {
                    if(cylDataLen > cylinderSize) return ErrorNumber.InvalidArgument;

                    // Raw cylinder data, read directly into output
                    stream.EnsureRead(_decompressedData, outputOffset, (int)cylDataLen);

                    break;
                }

                case CYL_COMPRESSED:
                {
                    var packedData = new byte[cylDataLen];
                    stream.EnsureRead(packedData, 0, (int)cylDataLen);

                    switch((PackerType)_header.PackerType)
                    {
                        case PackerType.Stored:
                        {
                            // Data is byte-interleaved only, no compression
                            var resorted = new byte[cylinderSize];

                            ResortCylinderData(packedData, resorted, _header.BlockSize, sectorsPerCyl, cylDataLen);

                            Buffer.BlockCopy(resorted, 0, _decompressedData, outputOffset, (int)cylinderSize);

                            break;
                        }

                        case PackerType.PowerPacker:
                        {
                            // Decompress with PowerPacker, then de-interleave
                            byte[] decompressed =
                                PowerPackerDecompress(packedData, (int)cylDataLen, (int)cylinderSize, offsetSizes);

                            var resorted = new byte[cylinderSize];

                            ResortCylinderData(decompressed, resorted, _header.BlockSize, sectorsPerCyl, cylinderSize);

                            Buffer.BlockCopy(resorted, 0, _decompressedData, outputOffset, (int)cylinderSize);

                            break;
                        }

                        default:
                            return ErrorNumber.NotSupported;
                    }

                    break;
                }

                default:
                    AaruLogging.Debug(MODULE_NAME, Localization.CrunchDisk_unexpected_cylinder_magic_0, cylMagic);

                    return ErrorNumber.InvalidArgument;
            }

            outputOffset += (int)cylinderSize;
        }

        _imageInfo.Cylinders       = cylinders;
        _imageInfo.Heads           = _header.Heads;
        _imageInfo.SectorsPerTrack = _header.BlocksPerTrack;
        _imageInfo.SectorSize      = _header.BlockSize;
        _imageInfo.Sectors         = totalSectors;
        _imageInfo.ImageSize       = (ulong)_decompressedData.Length;

        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);

        _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                      (ushort)_imageInfo.SectorsPerTrack, (ushort)_imageInfo.SectorSize,
                                                      MediaEncoding.MFM, false));

        if(_imageInfo.MediaType == MediaType.Unknown)
            AaruLogging.Debug(MODULE_NAME, Localization.CrunchDisk_unknown_media_type);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress >= _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        buffer = new byte[_imageInfo.SectorSize];

        Array.Copy(_decompressedData, (long)sectorAddress * _imageInfo.SectorSize, buffer, 0, _imageInfo.SectorSize);

        sectorStatus = SectorStatus.Dumped;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress >= _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, false, out byte[] sector, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sector, 0, sector.Length);
            sectorStatus[i] = status;
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}