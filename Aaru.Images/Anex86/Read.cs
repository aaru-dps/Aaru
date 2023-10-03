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
//     Reads Anex86 disk images.
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
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class Anex86
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < Marshal.SizeOf<Header>())
            return ErrorNumber.InvalidArgument;

        byte[] hdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(hdrB, 0, hdrB.Length);

        _header = Marshal.SpanToStructureLittleEndian<Header>(hdrB);

        _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_header.cylinders, (byte)_header.heads,
                                                      (ushort)_header.spt, (uint)_header.bps, MediaEncoding.MFM,
                                                      false));

        if(_imageInfo.MediaType == MediaType.Unknown)
            _imageInfo.MediaType = MediaType.GENERIC_HDD;

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.MediaType_0, _imageInfo.MediaType);

        _imageInfo.ImageSize            = (ulong)_header.dskSize;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = (ulong)(_header.cylinders * _header.heads * _header.spt);
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.SectorSize           = (uint)_header.bps;
        _imageInfo.Cylinders            = (uint)_header.cylinders;
        _imageInfo.Heads                = (uint)_header.heads;
        _imageInfo.SectorsPerTrack      = (uint)_header.spt;

        _anexImageFilter = imageFilter;

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

        Stream stream = _anexImageFilter.GetDataForkStream();

        stream.Seek((long)((ulong)_header.hdrSize + (sectorAddress * _imageInfo.SectorSize)), SeekOrigin.Begin);

        stream.EnsureRead(buffer, 0, (int)(length * _imageInfo.SectorSize));

        return ErrorNumber.NoError;
    }
}