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
//     Writes Parallels disk images.
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
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class Parallels
{
    // TODO: Support extended
    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint sectorSize)
    {
        if(sectorSize != 512)
        {
            ErrorMessage = Localization.Unsupported_sector_size;

            return false;
        }

        if(!SupportedMediaTypes.Contains(mediaType))
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

            return false;
        }

        if(sectors * sectorSize / DEFAULT_CLUSTER_SIZE > uint.MaxValue)
        {
            ErrorMessage = Localization.Too_many_sectors_for_selected_cluster_size;

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
        catch(IOException e)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, e.Message);

            return false;
        }

        uint batEntries = (uint)(sectors * sectorSize / DEFAULT_CLUSTER_SIZE);

        if(sectors * sectorSize % DEFAULT_CLUSTER_SIZE > 0)
            batEntries++;

        uint headerSectors = (uint)Marshal.SizeOf<Header>() + (batEntries * 4);

        if((uint)Marshal.SizeOf<Header>() + (batEntries % 4) > 0)
            headerSectors++;

        _pHdr = new Header
        {
            magic        = _magic,
            version      = PARALLELS_VERSION,
            sectors      = sectors,
            in_use       = PARALLELS_CLOSED,
            bat_entries  = batEntries,
            data_off     = headerSectors,
            cluster_size = DEFAULT_CLUSTER_SIZE / 512
        };

        _bat                    = new uint[batEntries];
        _currentWritingPosition = headerSectors * 512;

        IsWriting    = true;
        ErrorMessage = null;

        return true;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        ErrorMessage = Localization.Writing_media_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(data.Length != 512)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress >= _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        // Ignore empty sectors
        if(ArrayHelpers.ArrayIsNullOrEmpty(data))
            return true;

        ulong index  = sectorAddress / _pHdr.cluster_size;
        ulong secOff = sectorAddress % _pHdr.cluster_size;

        uint batOff = _bat[index];

        if(batOff == 0)
        {
            batOff                  =  (uint)(_currentWritingPosition / 512);
            _bat[index]             =  batOff;
            _currentWritingPosition += _pHdr.cluster_size * 512L;
        }

        ulong imageOff = batOff * 512UL;

        _writingStream.Seek((long)imageOff, SeekOrigin.Begin);
        _writingStream.Seek((long)secOff * 512, SeekOrigin.Current);
        _writingStream.Write(data, 0, data.Length);

        ErrorMessage = "";

        return true;
    }

    // TODO: This can be optimized
    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(data.Length % 512 != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress + length > _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        // Ignore empty sectors
        if(ArrayHelpers.ArrayIsNullOrEmpty(data))
            return true;

        for(uint i = 0; i < length; i++)
        {
            byte[] tmp = new byte[512];
            Array.Copy(data, i * 512, tmp, 0, 512);

            if(!WriteSector(tmp, sectorAddress + i))
                return false;
        }

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

        if(_pHdr.cylinders == 0)
        {
            _pHdr.cylinders            = (uint)(_imageInfo.Sectors / 16 / 63);
            _pHdr.heads                = 16;
            _imageInfo.SectorsPerTrack = 63;

            while(_pHdr.cylinders == 0)
            {
                _pHdr.heads--;

                if(_pHdr.heads == 0)
                {
                    _imageInfo.SectorsPerTrack--;
                    _pHdr.heads = 16;
                }

                _pHdr.cylinders = (uint)(_imageInfo.Sectors / _pHdr.heads / _imageInfo.SectorsPerTrack);

                if(_pHdr is { cylinders: 0, heads: 0 } &&
                   _imageInfo.SectorsPerTrack == 0)
                    break;
            }
        }

        byte[] hdr    = new byte[Marshal.SizeOf<Header>()];
        nint   hdrPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<Header>());
        System.Runtime.InteropServices.Marshal.StructureToPtr(_pHdr, hdrPtr, true);
        System.Runtime.InteropServices.Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(hdrPtr);

        _writingStream.Seek(0, SeekOrigin.Begin);
        _writingStream.Write(hdr, 0, hdr.Length);

        for(long i = 0; i < _bat.LongLength; i++)
            _writingStream.Write(BitConverter.GetBytes(_bat[i]), 0, 4);

        _writingStream.Flush();
        _writingStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo) => true;

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
    {
        _pHdr.cylinders = cylinders;
        _pHdr.heads     = heads;

        return true;
    }

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
}