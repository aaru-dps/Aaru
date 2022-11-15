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
//     Writes NHD r0 disk images.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Schemas;

namespace Aaru.DiscImages;

public sealed partial class Nhdr0
{
    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint sectorSize)
    {
        if(sectorSize != 0)
        {
            ErrorMessage = "Unsupported sector size";

            return false;
        }

        if(!SupportedMediaTypes.Contains(mediaType))
        {
            ErrorMessage = $"Unsupported media format {mediaType}";

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
            _writingStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
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
        ErrorMessage = "Writing media tags is not supported.";

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

        if(data.Length != _imageInfo.SectorSize)
        {
            ErrorMessage = "Incorrect data size";

            return false;
        }

        if(sectorAddress >= _imageInfo.Sectors)
        {
            ErrorMessage = "Tried to write past image size";

            return false;
        }

        _writingStream.Seek((long)((ulong)Marshal.SizeOf<Header>() + (sectorAddress * _imageInfo.SectorSize)),
                            SeekOrigin.Begin);

        _writingStream.Write(data, 0, data.Length);

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

        if(sectorAddress + length > _imageInfo.Sectors)
        {
            ErrorMessage = "Tried to write past image size";

            return false;
        }

        _writingStream.Seek((long)((ulong)Marshal.SizeOf<Header>() + (sectorAddress * _imageInfo.SectorSize)),
                            SeekOrigin.Begin);

        _writingStream.Write(data, 0, data.Length);

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        ErrorMessage = "Writing sectors with tags is not supported.";

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        ErrorMessage = "Writing sectors with tags is not supported.";

        return false;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = "Image is not opened for writing";

            return false;
        }

        if(_imageInfo.Cylinders == 0)
        {
            _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 8 / 17);
            _imageInfo.Heads           = 8;
            _imageInfo.SectorsPerTrack = 17;

            while(_imageInfo.Cylinders == 0)
            {
                _imageInfo.Heads--;

                if(_imageInfo.Heads == 0)
                {
                    _imageInfo.SectorsPerTrack--;
                    _imageInfo.Heads = 16;
                }

                _imageInfo.Cylinders = (uint)(_imageInfo.Sectors / _imageInfo.Heads / _imageInfo.SectorsPerTrack);

                if(_imageInfo.Cylinders == 0 &&
                   _imageInfo is { Heads: 0, SectorsPerTrack: 0 })
                    break;
            }
        }

        var header = new Header
        {
            szFileID   = _signature,
            szComment  = new byte[0x100],
            dwHeadSize = Marshal.SizeOf<Header>(),
            dwCylinder = (byte)_imageInfo.Cylinders,
            wHead      = (byte)_imageInfo.Heads,
            wSect      = (byte)_imageInfo.SectorsPerTrack,
            wSectLen   = (byte)_imageInfo.SectorSize,
            reserved2  = new byte[2],
            reserved3  = new byte[0xE0]
        };

        if(!string.IsNullOrEmpty(_imageInfo.Comments))
        {
            byte[] commentBytes = Encoding.GetEncoding("shift_jis").GetBytes(_imageInfo.Comments);

            Array.Copy(commentBytes, 0, header.szComment, 0,
                       commentBytes.Length >= 0x100 ? 0x100 : commentBytes.Length);
        }

        byte[] hdr    = new byte[Marshal.SizeOf<Header>()];
        nint   hdrPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<Header>());
        System.Runtime.InteropServices.Marshal.StructureToPtr(header, hdrPtr, true);
        System.Runtime.InteropServices.Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(hdrPtr);

        _writingStream.Seek(0, SeekOrigin.Begin);
        _writingStream.Write(hdr, 0, hdr.Length);

        _writingStream.Flush();
        _writingStream.Close();
        IsWriting = false;

        return true;
    }

    /// <inheritdoc />
    public bool SetMetadata(ImageInfo metadata)
    {
        _imageInfo.Comments = metadata.Comments;

        return true;
    }

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
    {
        if(cylinders > int.MaxValue)
        {
            ErrorMessage = "Too many cylinders.";

            return false;
        }

        if(heads > short.MaxValue)
        {
            ErrorMessage = "Too many heads.";

            return false;
        }

        if(sectorsPerTrack > short.MaxValue)
        {
            ErrorMessage = "Too many sectors per track.";

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