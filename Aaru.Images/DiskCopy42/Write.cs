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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Claunia.Encoding;
using Schemas;

public sealed partial class DiskCopy42
{
    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint sectorSize)
    {
        header = new Header();
        var tags   = false;
        var macosx = false;

        if(options != null &&
           options.TryGetValue("macosx", out string tmpOption))
            bool.TryParse(tmpOption, out macosx);

        if(sectorSize != 512)
        {
            ErrorMessage = "Unsupported sector size";

            return false;
        }

        switch(mediaType)
        {
            case MediaType.AppleFileWare:
                header.FmtByte = kSigmaFmtByteTwiggy;
                header.Format  = kSigmaFormatTwiggy;
                twiggy         = true;

                // TODO
                ErrorMessage = "Twiggy write support not yet implemented";

                return false;
            case MediaType.AppleHD20:
                if(sectors != 39040)
                {
                    ErrorMessage = "Incorrect number of sectors for Apple HD20 image";

                    return false;
                }

                header.FmtByte = kFmtNotStandard;
                header.Format  = kNotStandardFormat;
                tags           = true;

                break;
            case MediaType.AppleProfile:
                if(sectors != 9728 &&
                   sectors != 19456)
                {
                    ErrorMessage = "Incorrect number of sectors for Apple Profile image";

                    return false;
                }

                header.FmtByte = kFmtNotStandard;
                header.Format  = kNotStandardFormat;
                tags           = true;

                break;
            case MediaType.AppleSonyDS:
                if(sectors != 1600)
                {
                    ErrorMessage = "Incorrect number of sectors for Apple MF2DD image";

                    return false;
                }

                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte800K;
                header.Format  = kSonyFormat800K;
                tags           = true;

                break;
            case MediaType.AppleSonySS:
                if(sectors != 800)
                {
                    ErrorMessage = "Incorrect number of sectors for Apple MF1DD image";

                    return false;
                }

                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte400K;
                header.Format  = kSonyFormat400K;
                tags           = true;

                break;
            case MediaType.AppleWidget:
                if(sectors != 39040)
                {
                    ErrorMessage = "Incorrect number of sectors for Apple Widget image";

                    return false;
                }

                header.FmtByte = kFmtNotStandard;
                header.Format  = kNotStandardFormat;
                tags           = true;

                break;
            case MediaType.DOS_35_DS_DD_9:
                if(sectors != 1440)
                {
                    ErrorMessage = "Incorrect number of sectors for MF2DD image";

                    return false;
                }

                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte720K;
                header.Format  = kSonyFormat720K;

                break;
            case MediaType.DOS_35_HD:
                if(sectors != 2880)
                {
                    ErrorMessage = "Incorrect number of sectors for MF2HD image";

                    return false;
                }

                header.Format  = kSonyFmtByte1440K;
                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte1440K;

                break;
            case MediaType.DMF:
                if(sectors != 3360)
                {
                    ErrorMessage = "Incorrect number of sectors for DMF image";

                    return false;
                }

                header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte1680K;
                header.Format  = kSonyFormat1680K;

                break;
            default:
                ErrorMessage = $"Unsupported media format {mediaType}";

                return false;
        }

        dataOffset      = 0x54;
        tagOffset       = header.TagSize != 0 ? 0x54 + header.DataSize : 0;
        header.DiskName = "-Aaru converted image-";
        header.Valid    = 1;
        header.DataSize = (uint)(sectors * 512);

        if(tags)
            header.TagSize = (uint)(sectors * 12);

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
        catch(IOException e)
        {
            ErrorMessage = $"Could not create new image file, exception {e.Message}";

            return false;
        }

        IsWriting    = true;
        ErrorMessage = null;

        return true;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        ErrorMessage = "Unsupported feature";

        return false;
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = "Tried to write on a non-writable image";

            return false;
        }

        if(data.Length != 512)
        {
            ErrorMessage = "Incorrect data size";

            return false;
        }

        if(sectorAddress >= imageInfo.Sectors)
        {
            ErrorMessage = "Tried to write past image size";

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
            ErrorMessage = "Tried to write on a non-writable image";

            return false;
        }

        if(data.Length % 512 != 0)
        {
            ErrorMessage = "Incorrect data size";

            return false;
        }

        if(sectorAddress + length > imageInfo.Sectors)
        {
            ErrorMessage = "Tried to write past image size";

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
            ErrorMessage = "Tried to write on a non-writable image";

            return false;
        }

        if(header.TagSize == 0)
        {
            ErrorMessage = "Image does not support tags";

            return false;
        }

        if(data.Length != 524)
        {
            ErrorMessage = "Incorrect data size";

            return false;
        }

        if(sectorAddress >= imageInfo.Sectors)
        {
            ErrorMessage = "Tried to write past image size";

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
            ErrorMessage = "Tried to write on a non-writable image";

            return false;
        }

        if(header.TagSize == 0)
        {
            ErrorMessage = "Image does not support tags";

            return false;
        }

        if(data.Length % 524 != 0)
        {
            ErrorMessage = "Incorrect data size";

            return false;
        }

        if(sectorAddress + length > imageInfo.Sectors)
        {
            ErrorMessage = "Tried to write past image size";

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
            ErrorMessage = "Image is not opened for writing";

            return false;
        }

        // No tags where written
        if(writingStream.Length == 0x54 + header.DataSize)
            header.TagSize = 0;

        writingStream.Seek(0x54, SeekOrigin.Begin);
        var data = new byte[header.DataSize];
        writingStream.EnsureRead(data, 0, (int)header.DataSize);
        header.DataChecksum = CheckSum(data);
        writingStream.Seek(0x54 + header.DataSize, SeekOrigin.Begin);
        data = new byte[header.TagSize];
        writingStream.EnsureRead(data, 0, (int)header.TagSize);
        header.TagChecksum = CheckSum(data);

        writingStream.Seek(0, SeekOrigin.Begin);

        if(header.DiskName.Length > 63)
            header.DiskName = header.DiskName[..63];

        writingStream.WriteByte((byte)header.DiskName.Length);
        Encoding macRoman = new MacRoman();
        writingStream.Write(macRoman.GetBytes(header.DiskName), 0, header.DiskName.Length);

        writingStream.Seek(64, SeekOrigin.Begin);
        writingStream.Write(BigEndianBitConverter.GetBytes(header.DataSize), 0, 4);
        writingStream.Write(BigEndianBitConverter.GetBytes(header.TagSize), 0, 4);
        writingStream.Write(BigEndianBitConverter.GetBytes(header.DataChecksum), 0, 4);
        writingStream.Write(BigEndianBitConverter.GetBytes(header.TagChecksum), 0, 4);
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
    public bool SetMetadata(ImageInfo metadata)
    {
        header.DiskName = metadata.MediaTitle ?? "-Aaru converted image-";

        return true;
    }

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        ErrorMessage = "Unsupported feature";

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
    {
        ErrorMessage = "Unsupported feature";

        return false;
    }

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetCicmMetadata(CICMMetadataType metadata) => false;
}