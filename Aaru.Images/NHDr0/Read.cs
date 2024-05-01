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
//     Reads NHD r0 disk images.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class Nhdr0
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
        var shiftjis = Encoding.GetEncoding("shift_jis");

        if(stream.Length < Marshal.SizeOf<Header>()) return ErrorNumber.InvalidArgument;

        var hdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(hdrB, 0, hdrB.Length);

        _nhdhdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrB);

        _imageInfo.MediaType = MediaType.GENERIC_HDD;

        _imageInfo.ImageSize            = (ulong)(stream.Length - _nhdhdr.dwHeadSize);
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = (ulong)(_nhdhdr.dwCylinder * _nhdhdr.wHead * _nhdhdr.wSect);
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.SectorSize           = (uint)_nhdhdr.wSectLen;
        _imageInfo.Cylinders            = (uint)_nhdhdr.dwCylinder;
        _imageInfo.Heads                = (uint)_nhdhdr.wHead;
        _imageInfo.SectorsPerTrack      = (uint)_nhdhdr.wSect;
        _imageInfo.Comments             = StringHandlers.CToString(_nhdhdr.szComment, shiftjis);

        _nhdImageFilter = imageFilter;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        buffer = new byte[length * _imageInfo.SectorSize];

        Stream stream = _nhdImageFilter.GetDataForkStream();

        stream.Seek((long)((ulong)_nhdhdr.dwHeadSize + sectorAddress * _imageInfo.SectorSize), SeekOrigin.Begin);

        stream.EnsureRead(buffer, 0, (int)(length * _imageInfo.SectorSize));

        return ErrorNumber.NoError;
    }

#endregion
}