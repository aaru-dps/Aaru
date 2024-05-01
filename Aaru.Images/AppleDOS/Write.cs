// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes interleaved Apple ][ disk images.
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
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;

namespace Aaru.Images;

public sealed partial class AppleDos
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize)
    {
        if(sectorSize != 256)
        {
            ErrorMessage = Localization.Unsupported_sector_size;

            return false;
        }

        if(mediaType != MediaType.Apple32SS && mediaType != MediaType.Apple33SS)
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

            return false;
        }

        if(mediaType == MediaType.Apple32SS && sectors != 455 || mediaType == MediaType.Apple33SS && sectors != 560)
        {
            ErrorMessage = Localization.Incorrect_number_of_sectors_for_media;

            return false;
        }

        _imageInfo = new ImageInfo
        {
            MediaType  = mediaType,
            SectorSize = sectorSize,
            Sectors    = sectors
        };

        try
        {
            _writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch(IOException ex)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, ex.Message);
            AaruConsole.WriteException(ex);

            return false;
        }

        _extension = Path.GetExtension(path);

        if(mediaType == MediaType.Apple32SS)
        {
            _dos32     = true;
            _extension = ".d13";
        }

        _deinterleaved = new byte[35 * (_dos32 ? 13 : 16) * 256];

        IsWriting    = true;
        ErrorMessage = null;

        return true;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress) => WriteSectors(data, sectorAddress, 1);

    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(data.Length % _imageInfo.SectorSize != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress + length > _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        Array.Copy(data, 0, _deinterleaved, (int)(sectorAddress * _imageInfo.SectorSize), data.Length);

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        byte[] tmp;

        if(_dos32)
            tmp = _deinterleaved;
        else
        {
            bool isDos = _deinterleaved[0x11001] == 17  &&
                         _deinterleaved[0x11002] < 16   &&
                         _deinterleaved[0x11027] <= 122 &&
                         _deinterleaved[0x11034] == 35  &&
                         _deinterleaved[0x11035] == 16  &&
                         _deinterleaved[0x11036] == 0   &&
                         _deinterleaved[0x11037] == 1;

            tmp = new byte[_deinterleaved.Length];

            int[] offsets = _extension == ".do"
                                ? isDos ? _deinterleave : _interleave
                                : isDos
                                    ? _interleave
                                    : _deinterleave;

            for(var t = 0; t < 35; t++)
            {
                for(var s = 0; s < 16; s++)
                    Array.Copy(_deinterleaved, t * 16 * 256 + offsets[s] * 256, tmp, t * 16 * 256 + s * 256, 256);
            }
        }

        _writingStream.Seek(0, SeekOrigin.Begin);
        _writingStream.Write(tmp, 0, tmp.Length);

        _writingStream.Flush();
        _writingStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo) => true;

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata) => false;

#endregion
}