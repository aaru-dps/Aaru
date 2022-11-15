// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads DiskDupe DDI disk images.
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
// Copyright © 2021-2022 Michael Drüing
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class DiskDupe
{
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        var         fHeader      = new FileHeader();
        TrackInfo[] trackMap     = null;
        long[]      trackOffsets = null;

        if(!TryReadHeader(stream, ref fHeader, ref trackMap, ref trackOffsets))
            return ErrorNumber.InvalidArgument;

        AaruConsole.DebugWriteLine("DiskDupe Plugin",
                                   "Detected DiskDupe DDI image with {0} tracks and {1} sectors per track.",
                                   _diskTypes[fHeader.diskType].cyl, _diskTypes[fHeader.diskType].spt);

        _imageInfo.Cylinders       = _diskTypes[fHeader.diskType].cyl;
        _imageInfo.Heads           = _diskTypes[fHeader.diskType].hd;
        _imageInfo.SectorsPerTrack = _diskTypes[fHeader.diskType].spt;
        _imageInfo.SectorSize      = 512; // only 512 bytes per sector supported
        _imageInfo.Sectors         = _imageInfo.Heads   * _imageInfo.Cylinders * _imageInfo.SectorsPerTrack;
        _imageInfo.ImageSize       = _imageInfo.Sectors * _imageInfo.SectorSize;

        _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);

        _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                      (ushort)_imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                      false));

        // save some variables for later use
        _fileHeader     = fHeader;
        _ddiImageFilter = imageFilter;
        _trackMap       = trackMap;
        _trackOffsets   = trackOffsets;

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;
        int trackNum     = (int)(sectorAddress / _imageInfo.SectorsPerTrack);
        int sectorOffset = (int)(sectorAddress % _imageInfo.SectorsPerTrack);

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(trackNum > 2 * _imageInfo.Cylinders)
            return ErrorNumber.SectorNotFound;

        buffer = new byte[_imageInfo.SectorSize];

        if(_trackMap[trackNum].present != 1)
            Array.Clear(buffer, 0, (int)_imageInfo.SectorSize);
        else
        {
            Stream strm = _ddiImageFilter.GetDataForkStream();

            strm.Seek(_trackOffsets[trackNum] + (sectorOffset * _imageInfo.SectorSize), SeekOrigin.Begin);

            strm.EnsureRead(buffer, 0, (int)_imageInfo.SectorSize);
        }

        return ErrorNumber.NoError;
    }

    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }
}