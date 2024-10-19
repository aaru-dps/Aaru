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
//     Reads DIM disk images.
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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class Dim
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < DATA_OFFSET) return ErrorNumber.InvalidArgument;

        long diskSize = stream.Length - DATA_OFFSET;

        _comment = new byte[60];
        _hdrId   = new byte[13];
        stream.Seek(0, SeekOrigin.Begin);
        _dskType = (DiskType)stream.ReadByte();
        stream.Seek(0xAB, SeekOrigin.Begin);
        stream.EnsureRead(_hdrId, 0, 13);
        stream.Seek(0xC2, SeekOrigin.Begin);
        stream.EnsureRead(_comment, 0, 60);

        if(!_headerId.SequenceEqual(_hdrId)) return ErrorNumber.InvalidArgument;

        _imageInfo.MediaType = MediaType.Unknown;

        switch(_dskType)
        {
            // 8 spt, 1024 bps
            case DiskType.Hd2:
                if(diskSize % (2 * 8 * 1024) != 0)
                {
                    AaruConsole.ErrorWriteLine(Localization.DIM_shows_unknown_image_with_0_tracks,
                                               diskSize / (2 * 8 * 1024));

                    return ErrorNumber.NotSupported;
                }

                if(diskSize / (2 * 8 * 1024) == 77) _imageInfo.MediaType = MediaType.SHARP_525;

                _imageInfo.SectorSize = 1024;

                break;

            // 9 spt, 1024 bps
            case DiskType.Hs2:
                if(diskSize % (2 * 9 * 512) != 0)
                {
                    AaruConsole.ErrorWriteLine(Localization.DIM_shows_unknown_image_with_0_tracks,
                                               diskSize / (2 * 9 * 512));

                    return ErrorNumber.NotSupported;
                }

                if(diskSize / (2 * 9 * 512) == 80) _imageInfo.MediaType = MediaType.SHARP_525_9;

                _imageInfo.SectorSize = 512;

                break;

            // 15 spt, 512 bps
            case DiskType.Hc2:
                if(diskSize % (2 * 15 * 512) != 0)
                {
                    AaruConsole.ErrorWriteLine(Localization.DIM_shows_unknown_image_with_0_tracks,
                                               diskSize / (2 * 15 * 512));

                    return ErrorNumber.NotSupported;
                }

                if(diskSize / (2 * 15 * 512) == 80) _imageInfo.MediaType = MediaType.DOS_525_HD;

                _imageInfo.SectorSize = 512;

                break;

            // 9 spt, 1024 bps
            case DiskType.Hde2:
                if(diskSize % (2 * 9 * 512) != 0)
                {
                    AaruConsole.ErrorWriteLine(Localization.DIM_shows_unknown_image_with_0_tracks,
                                               diskSize / (2 * 9 * 512));

                    return ErrorNumber.NotSupported;
                }

                if(diskSize / (2 * 9 * 512) == 80) _imageInfo.MediaType = MediaType.SHARP_35_9;

                _imageInfo.SectorSize = 512;

                break;

            // 18 spt, 512 bps
            case DiskType.Hq2:
                if(diskSize % (2 * 18 * 512) != 0)
                {
                    AaruConsole.ErrorWriteLine(Localization.DIM_shows_unknown_image_with_0_tracks,
                                               diskSize / (2 * 18 * 512));

                    return ErrorNumber.NotSupported;
                }

                if(diskSize / (2 * 18 * 512) == 80) _imageInfo.MediaType = MediaType.DOS_35_HD;

                _imageInfo.SectorSize = 512;

                break;

            // 26 spt, 256 bps
            case DiskType.N88:
                if(diskSize % (2 * 26 * 256) == 0)
                {
                    if(diskSize % (2 * 26 * 256) == 77) _imageInfo.MediaType = MediaType.NEC_8_DD;

                    _imageInfo.SectorSize = 256;
                }
                else if(diskSize % (2 * 26 * 128) == 0)
                {
                    if(diskSize % (2 * 26 * 128) == 77) _imageInfo.MediaType = MediaType.NEC_8_SD;

                    _imageInfo.SectorSize = 256;
                }
                else
                {
                    AaruConsole.ErrorWriteLine(Localization.DIM_shows_unknown_image_with_0_tracks,
                                               diskSize / (2 * 26 * 256));

                    return ErrorNumber.NotSupported;
                }

                break;
            default:
                return ErrorNumber.InvalidArgument;
        }

        AaruConsole.VerboseWriteLine(Localization.DIM_image_contains_a_disk_of_type_0, _imageInfo.MediaType);

        if(!string.IsNullOrEmpty(_imageInfo.Comments))
            AaruConsole.VerboseWriteLine(Localization.DIM_comments_0, _imageInfo.Comments);

        _dimImageFilter = imageFilter;

        _imageInfo.ImageSize            = (ulong)diskSize;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = _imageInfo.ImageSize / _imageInfo.SectorSize;
        _imageInfo.Comments             = StringHandlers.CToString(_comment, Encoding.GetEncoding(932));
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;

        switch(_imageInfo.MediaType)
        {
            case MediaType.SHARP_525:
                _imageInfo.Cylinders       = 77;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 8;

                break;
            case MediaType.SHARP_525_9:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 9;

                break;
            case MediaType.DOS_525_HD:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 15;

                break;
            case MediaType.SHARP_35_9:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 9;

                break;
            case MediaType.DOS_35_HD:
                _imageInfo.Cylinders       = 80;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 18;

                break;
            case MediaType.NEC_8_DD:
            case MediaType.NEC_8_SD:
                _imageInfo.Cylinders       = 77;
                _imageInfo.Heads           = 2;
                _imageInfo.SectorsPerTrack = 26;

                break;
        }

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

        Stream stream = _dimImageFilter.GetDataForkStream();

        stream.Seek((long)(DATA_OFFSET + sectorAddress * _imageInfo.SectorSize), SeekOrigin.Begin);

        stream.EnsureRead(buffer, 0, (int)(length * _imageInfo.SectorSize));

        return ErrorNumber.NoError;
    }

#endregion
}