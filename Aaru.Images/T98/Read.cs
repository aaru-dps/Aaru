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
//     Reads T98 disk images.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class T98
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length % 256 != 0)
            return ErrorNumber.InvalidArgument;

        var hdrB = new byte[256];
        stream.EnsureRead(hdrB, 0, hdrB.Length);

        for(var i = 4; i < 256; i++)
        {
            if(hdrB[i] != 0)
                return ErrorNumber.InvalidArgument;
        }

        var cylinders = BitConverter.ToInt32(hdrB, 0);

        _imageInfo.MediaType = MediaType.GENERIC_HDD;

        _imageInfo.ImageSize            = (ulong)(stream.Length - 256);
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = (ulong)(stream.Length / 256 - 1);
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.SectorSize           = 256;
        _imageInfo.Cylinders            = (uint)cylinders;
        _imageInfo.Heads                = 8;
        _imageInfo.SectorsPerTrack      = 33;

        _t98ImageFilter = imageFilter;

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

        Stream stream = _t98ImageFilter.GetDataForkStream();

        stream.Seek((long)(256 + sectorAddress * _imageInfo.SectorSize), SeekOrigin.Begin);

        stream.EnsureRead(buffer, 0, (int)(length * _imageInfo.SectorSize));

        return ErrorNumber.NoError;
    }

#endregion
}