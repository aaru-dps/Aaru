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
//     Reads MaxiDisk disk images.
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

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class MaxiDisk
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        if(stream.Length < 8)
            return ErrorNumber.InvalidArgument;

        byte[] buffer = new byte[8];
        stream.Seek(0, SeekOrigin.Begin);
        stream.EnsureRead(buffer, 0, buffer.Length);

        Header tmpHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(buffer);

        // This is hardcoded
        // But its possible values are unknown...
        //if(tmp_header.diskType > 11)
        //    return false;

        // Only floppies supported
        if(tmpHeader.heads is 0 or > 2)
            return ErrorNumber.InvalidArgument;

        // No floppies with more than this?
        if(tmpHeader.cylinders > 90)
            return ErrorNumber.InvalidArgument;

        // Maximum supported bps is 16384
        if(tmpHeader.bytesPerSector > 7)
            return ErrorNumber.InvalidArgument;

        int expectedFileSize = (tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack *
                                (128 << tmpHeader.bytesPerSector)) + 8;

        if(expectedFileSize != stream.Length)
            return ErrorNumber.InvalidArgument;

        _imageInfo.Cylinders       = tmpHeader.cylinders;
        _imageInfo.Heads           = tmpHeader.heads;
        _imageInfo.SectorsPerTrack = tmpHeader.sectorsPerTrack;
        _imageInfo.Sectors         = (ulong)(tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack);
        _imageInfo.SectorSize      = (uint)(128 << tmpHeader.bytesPerSector);

        _hdkImageFilter = imageFilter;

        _imageInfo.ImageSize            = (ulong)(stream.Length - 8);
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;

        _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                      (ushort)_imageInfo.SectorsPerTrack, _imageInfo.SectorSize,
                                                      MediaEncoding.MFM, false));

        _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

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

        Stream stream = _hdkImageFilter.GetDataForkStream();
        stream.Seek((long)(8 + (sectorAddress * _imageInfo.SectorSize)), SeekOrigin.Begin);
        stream.EnsureRead(buffer, 0, (int)(length * _imageInfo.SectorSize));

        return ErrorNumber.NoError;
    }
}