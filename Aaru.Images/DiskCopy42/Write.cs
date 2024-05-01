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
//     Writes Apple DiskCopy 4.2 disk images.
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

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using Claunia.Encoding;

namespace Aaru.Images;

public sealed partial class DiskCopy42
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize)
    {
        header = new Header();
        var tags   = false;
        var macosx = false;

        if(options != null && options.TryGetValue("macosx", out string tmpOption)) bool.TryParse(tmpOption, out macosx);

        if(sectorSize != 512)
        {
            ErrorMessage = Localization.Unsupported_sector_size;

            return false;
        }

        switch(mediaType)
        {
            case MediaType.AppleFileWare:
                header.FmtByte = kSigmaFmtByteTwiggy;
                header.Format  = kSigmaFormatTwiggy;
                twiggy         = true;

                // TODO
                ErrorMessage = Localization.Twiggy_write_support_not_yet_implemented;

                return false;
            case MediaType.AppleHD20:
                if(sectors != 39040)
                {
                    ErrorMessage = Localization.Incorrect_number_of_sectors_for_Apple_HD20_image;

                    return false;
                }

                header.FmtByte = kFmtNotStandard;
                header.Format  = kNotStandardFormat;
                tags           = true;

                break;
            case MediaType.AppleProfile:
                if(sectors != 9728 && sectors != 19456)
                {
                    ErrorMessage = Localization.Incorrect_number_of_sectors_for_Apple_Profile_image;

                    return false;
                }

                header.FmtByte = kFmtNotStandard;
                header.Format  = kNotStandardFormat;
                tags           = true;

                break;
            case MediaType.AppleSonyDS:
                if(sectors != 1600)
                {
                    ErrorMessage = Localization.Incorrect_number_of_sectors_for_Apple_MF2DD_image;

                    return false;
                }

                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte800K;
                header.Format  = kSonyFormat800K;
                tags           = true;

                break;
            case MediaType.AppleSonySS:
                if(sectors != 800)
                {
                    ErrorMessage = Localization.Incorrect_number_of_sectors_for_Apple_MF1DD_image;

                    return false;
                }

                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte400K;
                header.Format  = kSonyFormat400K;
                tags           = true;

                break;
            case MediaType.AppleWidget:
                if(sectors != 39040)
                {
                    ErrorMessage = Localization.Incorrect_number_of_sectors_for_Apple_Widget_image;

                    return false;
                }

                header.FmtByte = kFmtNotStandard;
                header.Format  = kNotStandardFormat;
                tags           = true;

                break;
            case MediaType.DOS_35_DS_DD_9:
                if(sectors != 1440)
                {
                    ErrorMessage = Localization.Incorrect_number_of_sectors_for_MF2DD_image;

                    return false;
                }

                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte720K;
                header.Format  = kSonyFormat720K;

                break;
            case MediaType.DOS_35_HD:
                if(sectors != 2880)
                {
                    ErrorMessage = Localization.Incorrect_number_of_sectors_for_MF2HD_image;

                    return false;
                }

                header.Format  = kSonyFmtByte1440K;
                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte1440K;

                break;
            case MediaType.DMF:
                if(sectors != 3360)
                {
                    ErrorMessage = Localization.Incorrect_number_of_sectors_for_DMF_image;

                    return false;
                }

                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte1680K;
                header.Format  = kSonyFormat1680K;

                break;
            default:
                ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

                return false;
        }

        dataOffset      = 0x54;
        tagOffset       = header.TagSize != 0 ? 0x54 + header.DataSize : 0;
        header.DiskName = "-Aaru converted image-";
        header.Valid    = 1;
        header.DataSize = (uint)(sectors * 512);

        if(tags) header.TagSize = (uint)(sectors * 12);

        imageInfo = new ImageInfo
        {
            MediaType  = mediaType,
            SectorSize = sectorSize,
            Sectors    = sectors
        };

        try
        {
            writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch(IOException ex)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, ex.Message);
            AaruConsole.WriteException(ex);

            return false;
        }

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

        if(sectorAddress >= imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        writingStream.Seek((long)(dataOffset + sectorAddress * 512), SeekOrigin.Begin);
        writingStream.Write(data, 0, data.Length);

        ErrorMessage = "";

        return true;
    }

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

        if(sectorAddress + length > imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        writingStream.Seek((long)(dataOffset + sectorAddress * 512), SeekOrigin.Begin);
        writingStream.Write(data, 0, data.Length);

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(header.TagSize == 0)
        {
            ErrorMessage = "Image does not support tags";

            return false;
        }

        if(data.Length != 524)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress >= imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        writingStream.Seek((long)(dataOffset + sectorAddress * 512), SeekOrigin.Begin);
        writingStream.Write(data, 0, 512);
        writingStream.Seek((long)(tagOffset + sectorAddress * 12), SeekOrigin.Begin);
        writingStream.Write(data, 512, 12);

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(header.TagSize == 0)
        {
            ErrorMessage = "Image does not support tags";

            return false;
        }

        if(data.Length % 524 != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress + length > imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        for(uint i = 0; i < length; i++)
        {
            writingStream.Seek((long)(dataOffset + (sectorAddress + i) * 512), SeekOrigin.Begin);
            writingStream.Write(data, (int)(i                          * 524 + 0), 512);

            writingStream.Seek((long)(tagOffset + (sectorAddress + i) * 12), SeekOrigin.Begin);

            writingStream.Write(data, (int)(i * 524 + 512), 12);
        }

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        // No tags where written
        if(writingStream.Length == 0x54 + header.DataSize) header.TagSize = 0;

        writingStream.Seek(0x54, SeekOrigin.Begin);
        var data = new byte[header.DataSize];
        writingStream.EnsureRead(data, 0, (int)header.DataSize);
        header.DataChecksum = CheckSum(data);
        writingStream.Seek(0x54 + header.DataSize, SeekOrigin.Begin);
        data = new byte[header.TagSize];
        writingStream.EnsureRead(data, 0, (int)header.TagSize);
        header.TagChecksum = CheckSum(data);

        writingStream.Seek(0, SeekOrigin.Begin);

        if(header.DiskName.Length > 63) header.DiskName = header.DiskName[..63];

        writingStream.WriteByte((byte)header.DiskName.Length);
        Encoding macRoman = new MacRoman();
        writingStream.Write(macRoman.GetBytes(header.DiskName), 0, header.DiskName.Length);

        writingStream.Seek(64, SeekOrigin.Begin);
        writingStream.Write(BigEndianBitConverter.GetBytes(header.DataSize),     0, 4);
        writingStream.Write(BigEndianBitConverter.GetBytes(header.TagSize),      0, 4);
        writingStream.Write(BigEndianBitConverter.GetBytes(header.DataChecksum), 0, 4);
        writingStream.Write(BigEndianBitConverter.GetBytes(header.TagChecksum),  0, 4);
        writingStream.WriteByte(header.Format);
        writingStream.WriteByte(header.FmtByte);
        writingStream.WriteByte(1);
        writingStream.WriteByte(0);

        writingStream.Flush();
        writingStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo)
    {
        header.DiskName = imageInfo.MediaTitle ?? "-Aaru converted image-";

        return true;
    }

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

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