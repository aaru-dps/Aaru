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
//     Writes VirtualBox disk images.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Images;

public sealed partial class Vdi
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

        if(sectors * sectorSize / DEFAULT_BLOCK_SIZE > uint.MaxValue)
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
        catch(IOException ex)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, ex.Message);
            AaruConsole.WriteException(ex);

            return false;
        }

        var ibmEntries = (uint)(sectors * sectorSize / DEFAULT_BLOCK_SIZE);

        if(sectors * sectorSize % DEFAULT_BLOCK_SIZE > 0) ibmEntries++;

        uint headerSectors = 1 + ibmEntries * 4 / sectorSize;

        if(ibmEntries * 4 % sectorSize != 0) headerSectors++;

        _ibm                    = new uint[ibmEntries];
        _currentWritingPosition = headerSectors * sectorSize;

        _vHdr = new Header
        {
            creator      = DIC_AARU,
            magic        = VDI_MAGIC,
            majorVersion = 1,
            minorVersion = 1,
            headerSize   = Marshal.SizeOf<Header>() - 72,
            imageType    = VdiImageType.Normal,
            offsetBlocks = sectorSize,
            offsetData   = (uint)_currentWritingPosition,
            sectorSize   = sectorSize,
            size         = sectors * sectorSize,
            blockSize    = DEFAULT_BLOCK_SIZE,
            blocks       = ibmEntries,
            uuid         = Guid.NewGuid(),
            snapshotUuid = Guid.NewGuid()
        };

        for(uint i = 0; i < ibmEntries; i++) _ibm[i] = VDI_EMPTY;

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

        if(data.Length != _imageInfo.SectorSize)
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
        if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

        ulong index  = sectorAddress * _vHdr.sectorSize / _vHdr.blockSize;
        ulong secOff = sectorAddress * _vHdr.sectorSize % _vHdr.blockSize;

        uint ibmOff = _ibm[index];

        if(ibmOff == VDI_EMPTY)
        {
            ibmOff                  =  (uint)((_currentWritingPosition - _vHdr.offsetData) / _vHdr.blockSize);
            _ibm[index]             =  ibmOff;
            _currentWritingPosition += _vHdr.blockSize;
            _vHdr.allocatedBlocks++;
        }

        ulong imageOff = _vHdr.offsetData + (ulong)ibmOff * _vHdr.blockSize;

        _writingStream.Seek((long)imageOff, SeekOrigin.Begin);
        _writingStream.Seek((long)secOff,   SeekOrigin.Current);
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

        // Ignore empty sectors
        if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

        for(uint i = 0; i < length; i++)
        {
            var tmp = new byte[_imageInfo.SectorSize];
            Array.Copy(data, i * _imageInfo.SectorSize, tmp, 0, _imageInfo.SectorSize);

            if(!WriteSector(tmp, sectorAddress + i)) return false;
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

        if(!string.IsNullOrEmpty(_imageInfo.Comments))
            _vHdr.comments = _imageInfo.Comments.Length > 255 ? _imageInfo.Comments[..255] : _imageInfo.Comments;

        if(_vHdr.logicalCylinders == 0)
        {
            _vHdr.logicalCylinders = (uint)(_imageInfo.Sectors / 16 / 63);
            _vHdr.logicalHeads     = 16;
            _vHdr.logicalSpt       = 63;

            while(_vHdr.logicalCylinders == 0)
            {
                _vHdr.logicalHeads--;

                if(_vHdr.logicalHeads == 0)
                {
                    _vHdr.logicalSpt--;
                    _vHdr.logicalHeads = 16;
                }

                _vHdr.logicalCylinders = (uint)(_imageInfo.Sectors / _vHdr.logicalHeads / _vHdr.logicalSpt);

                if(_vHdr.logicalCylinders == 0 && _vHdr is { logicalHeads: 0, logicalSpt: 0 }) break;
            }
        }

        var  hdr    = new byte[Marshal.SizeOf<Header>()];
        nint hdrPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<Header>());
        System.Runtime.InteropServices.Marshal.StructureToPtr(_vHdr, hdrPtr, true);
        System.Runtime.InteropServices.Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(hdrPtr);

        _writingStream.Seek(0, SeekOrigin.Begin);
        _writingStream.Write(hdr, 0, hdr.Length);

        _writingStream.Seek(_vHdr.offsetBlocks, SeekOrigin.Begin);
        _writingStream.Write(MemoryMarshal.Cast<uint, byte>(_ibm).ToArray(), 0, 4 * _ibm.Length);

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

        return true;
    }

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
    {
        _vHdr.cylinders = cylinders;
        _vHdr.heads     = heads;
        _vHdr.spt       = sectorsPerTrack;

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

#endregion
}