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
//     Writes Apridisk disk images.
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages;

public sealed partial class Apridisk
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize)
    {
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
        (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

        if(cylinder >= _sectorsData.Length)
        {
            ErrorMessage = Localization.Sector_address_not_found;

            return false;
        }

        if(head >= _sectorsData[cylinder].Length)
        {
            ErrorMessage = Localization.Sector_address_not_found;

            return false;
        }

        if(sector > _sectorsData[cylinder][head].Length)
        {
            ErrorMessage = Localization.Sector_address_not_found;

            return false;
        }

        _sectorsData[cylinder][head][sector] = data;

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        for(uint i = 0; i < length; i++)
        {
            (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

            if(cylinder >= _sectorsData.Length)
            {
                ErrorMessage = Localization.Sector_address_not_found;

                return false;
            }

            if(head >= _sectorsData[cylinder].Length)
            {
                ErrorMessage = Localization.Sector_address_not_found;

                return false;
            }

            if(sector > _sectorsData[cylinder][head].Length)
            {
                ErrorMessage = Localization.Sector_address_not_found;

                return false;
            }

            _sectorsData[cylinder][head][sector] = data;
        }

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

    // TODO: Try if apridisk software supports finding other chunks, to extend metadata support
    /// <inheritdoc />
    public bool Close()
    {
        _writingStream.Seek(0, SeekOrigin.Begin);
        _writingStream.Write(_signature, 0, _signature.Length);

        var hdr = new byte[Marshal.SizeOf<Record>()];

        for(ushort c = 0; c < _imageInfo.Cylinders; c++)
        {
            for(byte h = 0; h < _imageInfo.Heads; h++)
            {
                for(byte s = 0; s < _imageInfo.SectorsPerTrack; s++)
                {
                    if(_sectorsData[c][h][s] == null || _sectorsData[c][h][s].Length == 0)
                        continue;

                    var record = new Record
                    {
                        type        = RecordType.Sector,
                        compression = CompressType.Uncompresed,
                        headerSize  = (ushort)Marshal.SizeOf<Record>(),
                        dataSize    = (uint)_sectorsData[c][h][s].Length,
                        head        = h,
                        sector      = s,
                        cylinder    = c
                    };

                    MemoryMarshal.Write(hdr, in record);

                    _writingStream.Write(hdr,                   0, hdr.Length);
                    _writingStream.Write(_sectorsData[c][h][s], 0, _sectorsData[c][h][s].Length);
                }
            }
        }

        if(!string.IsNullOrEmpty(_imageInfo.Creator))
        {
            byte[] creatorBytes = Encoding.UTF8.GetBytes(_imageInfo.Creator);

            var creatorRecord = new Record
            {
                type        = RecordType.Creator,
                compression = CompressType.Uncompresed,
                headerSize  = (ushort)Marshal.SizeOf<Record>(),
                dataSize    = (uint)creatorBytes.Length + 1,
                head        = 0,
                sector      = 0,
                cylinder    = 0
            };

            MemoryMarshal.Write(hdr, in creatorRecord);

            _writingStream.Write(hdr,          0, hdr.Length);
            _writingStream.Write(creatorBytes, 0, creatorBytes.Length);
            _writingStream.WriteByte(0); // Termination
        }

        if(!string.IsNullOrEmpty(_imageInfo.Comments))
        {
            byte[] commentBytes = Encoding.UTF8.GetBytes(_imageInfo.Comments);

            var commentRecord = new Record
            {
                type        = RecordType.Comment,
                compression = CompressType.Uncompresed,
                headerSize  = (ushort)Marshal.SizeOf<Record>(),
                dataSize    = (uint)commentBytes.Length + 1,
                head        = 0,
                sector      = 0,
                cylinder    = 0
            };

            MemoryMarshal.Write(hdr, in commentRecord);

            _writingStream.Write(hdr,          0, hdr.Length);
            _writingStream.Write(commentBytes, 0, commentBytes.Length);
            _writingStream.WriteByte(0); // Termination
        }

        _writingStream.Flush();
        _writingStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo)
    {
        _imageInfo.Comments = imageInfo.Comments;
        _imageInfo.Creator  = imageInfo.Creator;

        return true;
    }

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
    {
        if(cylinders > ushort.MaxValue)
        {
            ErrorMessage = Localization.Too_many_cylinders;

            return false;
        }

        if(heads > byte.MaxValue)
        {
            ErrorMessage = Localization.Too_many_heads;

            return false;
        }

        if(sectorsPerTrack > byte.MaxValue)
        {
            ErrorMessage = Localization.Too_many_sectors_per_track;

            return false;
        }

        _sectorsData = new byte[cylinders][][][];

        for(ushort c = 0; c < cylinders; c++)
        {
            _sectorsData[c] = new byte[heads][][];

            for(byte h = 0; h < heads; h++)
                _sectorsData[c][h] = new byte[sectorsPerTrack][];
        }

        _imageInfo.Cylinders       = cylinders;
        _imageInfo.Heads           = heads;
        _imageInfo.SectorsPerTrack = sectorsPerTrack;

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