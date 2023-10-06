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
//     Writes Connectix and Microsoft Virtual PC disk images.
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
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using Version = System.Version;

namespace Aaru.Images;

public sealed partial class Vhd
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize)
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

        _writingStream.Seek((long)(0 + sectorAddress * 512), SeekOrigin.Begin);
        _writingStream.Write(data, 0, data.Length);

        ErrorMessage = "";

        return true;
    }

    // TODO: Implement dynamic
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

        _writingStream.Seek((long)(0 + sectorAddress * 512), SeekOrigin.Begin);
        _writingStream.Write(data, 0, data.Length);

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

        Version thisVersion = GetType().Assembly.GetName().Version ?? new Version();

        if(_imageInfo.Cylinders == 0)
        {
            _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
            _imageInfo.Heads           = 16;
            _imageInfo.SectorsPerTrack = 63;

            while(_imageInfo.Cylinders == 0)
            {
                _imageInfo.Heads--;

                if(_imageInfo.Heads == 0)
                {
                    _imageInfo.SectorsPerTrack--;
                    _imageInfo.Heads = 16;
                }

                _imageInfo.Cylinders = (uint)(_imageInfo.Sectors / _imageInfo.Heads / _imageInfo.SectorsPerTrack);

                if(_imageInfo.Cylinders == 0 && _imageInfo is { Heads: 0, SectorsPerTrack: 0 })
                    break;
            }
        }

        var footer = new HardDiskFooter
        {
            Cookie = IMAGE_COOKIE,
            Features = FEATURES_RESERVED,
            Version = VERSION1,
            Timestamp = (uint)(DateTime.Now - new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
            CreatorApplication = CREATOR_AARU,
            CreatorVersion = (uint)(((thisVersion.Major & 0xFF) << 24) +
                                    ((thisVersion.Minor & 0xFF) << 16) +
                                    ((thisVersion.Build & 0xFF) << 8)  +
                                    (thisVersion.Revision & 0xFF)),
            CreatorHostOs = DetectOS.GetRealPlatformID() == PlatformID.MacOSX ? CREATOR_MACINTOSH : CREATOR_WINDOWS,
            DiskType      = TYPE_FIXED,
            UniqueId      = Guid.NewGuid(),
            DiskGeometry = ((_imageInfo.Cylinders & 0xFFFF) << 16) +
                           ((_imageInfo.Heads     & 0xFF)   << 8)  +
                           (_imageInfo.SectorsPerTrack & 0xFF),
            OriginalSize = _imageInfo.Sectors * 512,
            CurrentSize  = _imageInfo.Sectors * 512
        };

        footer.Offset = footer.DiskType == TYPE_FIXED ? ulong.MaxValue : 512;

        var footerBytes = new byte[512];
        Array.Copy(BigEndianBitConverter.GetBytes(footer.Cookie),             0, footerBytes, 0x00, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.Features),           0, footerBytes, 0x08, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.Version),            0, footerBytes, 0x0C, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.Offset),             0, footerBytes, 0x10, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.Timestamp),          0, footerBytes, 0x18, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.CreatorApplication), 0, footerBytes, 0x1C, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.CreatorVersion),     0, footerBytes, 0x20, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.CreatorHostOs),      0, footerBytes, 0x24, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.OriginalSize),       0, footerBytes, 0x28, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.CurrentSize),        0, footerBytes, 0x30, 8);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.DiskGeometry),       0, footerBytes, 0x38, 4);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.DiskType),           0, footerBytes, 0x3C, 4);
        Array.Copy(footer.UniqueId.ToByteArray(),                             0, footerBytes, 0x44, 4);

        footer.Checksum = VhdChecksum(footerBytes);
        Array.Copy(BigEndianBitConverter.GetBytes(footer.Checksum), 0, footerBytes, 0x40, 4);

        _writingStream.Seek((long)(footer.DiskType == TYPE_FIXED ? footer.OriginalSize : 0), SeekOrigin.Begin);
        _writingStream.Write(footerBytes, 0, 512);

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
        if(cylinders > 0xFFFF)
        {
            ErrorMessage = Localization.Too_many_cylinders;

            return false;
        }

        if(heads > 0xFF)
        {
            ErrorMessage = Localization.Too_many_heads;

            return false;
        }

        if(sectorsPerTrack > 0xFF)
        {
            ErrorMessage = Localization.Too_many_sectors_per_track;

            return false;
        }

        _imageInfo.SectorsPerTrack = sectorsPerTrack;
        _imageInfo.Heads           = heads;
        _imageInfo.Cylinders       = cylinders;

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata) => false;

#endregion
}