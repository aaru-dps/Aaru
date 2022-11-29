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
//     Writes RS-IDE disk images.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.Helpers;
using Schemas;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.DiscImages;

public sealed partial class RsIde
{
    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint sectorSize)
    {
        if(sectorSize != 256 &&
           sectorSize != 512)
        {
            ErrorMessage = Localization.Unsupported_sector_size;

            return false;
        }

        if(sectors > 63 * 16 * 1024)
        {
            ErrorMessage = Localization.Too_many_sectors;

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
        if(tag != MediaTagType.ATA_IDENTIFY)
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_tag_0, tag);

            return false;
        }

        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        _identify = new byte[106];
        Array.Copy(data, 0, _identify, 0, 106);

        return true;
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

        _writingStream.Seek((long)((ulong)Marshal.SizeOf<Header>() + (sectorAddress * _imageInfo.SectorSize)),
                            SeekOrigin.Begin);

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

                if(_imageInfo.Cylinders == 0 &&
                   _imageInfo is { Heads: 0, SectorsPerTrack: 0 })
                    break;
            }
        }

        var header = new Header
        {
            magic    = _signature,
            identify = new byte[106],
            dataOff  = (ushort)Marshal.SizeOf<Header>(),
            revision = 1,
            reserved = new byte[11]
        };

        if(_imageInfo.SectorSize == 256)
            header.flags = RsIdeFlags.HalfSectors;

        if(_identify == null)
        {
            var ataId = new Identify.IdentifyDevice
            {
                GeneralConfiguration = CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.UltraFastIDE |
                                       CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.Fixed        |
                                       CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.NotMFM       |
                                       CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.SoftSector,
                Cylinders       = (ushort)_imageInfo.Cylinders,
                Heads           = (ushort)_imageInfo.Heads,
                SectorsPerTrack = (ushort)_imageInfo.SectorsPerTrack,
                VendorWord47    = 0x80,
                Capabilities = CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.DMASupport |
                               CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.IORDY      |
                               CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.LBASupport,
                ExtendedIdentify       = CommonTypes.Structs.Devices.ATA.Identify.ExtendedIdentifyBit.Words54to58Valid,
                CurrentCylinders       = (ushort)_imageInfo.Cylinders,
                CurrentHeads           = (ushort)_imageInfo.Heads,
                CurrentSectorsPerTrack = (ushort)_imageInfo.SectorsPerTrack,
                CurrentSectors         = (uint)_imageInfo.Sectors,
                LBASectors             = (uint)_imageInfo.Sectors,
                DMASupported           = CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0,
                DMAActive              = CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0
            };

            if(string.IsNullOrEmpty(_imageInfo.DriveManufacturer))
                _imageInfo.DriveManufacturer = "Aaru";

            if(string.IsNullOrEmpty(_imageInfo.DriveModel))
                _imageInfo.DriveModel = "";

            if(string.IsNullOrEmpty(_imageInfo.DriveFirmwareRevision))
                Version.GetVersion();

            if(string.IsNullOrEmpty(_imageInfo.DriveSerialNumber))
                _imageInfo.DriveSerialNumber = $"{new Random().NextDouble():16X}";

            byte[] ataIdBytes = new byte[Marshal.SizeOf<Identify.IdentifyDevice>()];
            nint   ptr        = System.Runtime.InteropServices.Marshal.AllocHGlobal(512);
            System.Runtime.InteropServices.Marshal.StructureToPtr(ataId, ptr, true);

            System.Runtime.InteropServices.Marshal.Copy(ptr, ataIdBytes, 0, Marshal.SizeOf<Identify.IdentifyDevice>());

            System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);

            Array.Copy(ScrambleAtaString(_imageInfo.DriveManufacturer + " " + _imageInfo.DriveModel, 40), 0, ataIdBytes,
                       27 * 2, 40);

            Array.Copy(ScrambleAtaString(_imageInfo.DriveFirmwareRevision, 8), 0, ataIdBytes, 23 * 2, 8);
            Array.Copy(ScrambleAtaString(_imageInfo.DriveSerialNumber, 20), 0, ataIdBytes, 10    * 2, 20);
            Array.Copy(ataIdBytes, 0, header.identify, 0, 106);
        }
        else
            Array.Copy(_identify, 0, header.identify, 0, 106);

        byte[] hdr    = new byte[Marshal.SizeOf<Header>()];
        nint   hdrPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<Header>());
        System.Runtime.InteropServices.Marshal.StructureToPtr(header, hdrPtr, true);
        System.Runtime.InteropServices.Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(hdrPtr);

        _writingStream.Seek(0, SeekOrigin.Begin);
        _writingStream.Write(hdr, 0, hdr.Length);

        _writingStream.Flush();
        _writingStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetMetadata(ImageInfo metadata)
    {
        _imageInfo.DriveManufacturer     = metadata.DriveManufacturer;
        _imageInfo.DriveModel            = metadata.DriveModel;
        _imageInfo.DriveFirmwareRevision = metadata.DriveFirmwareRevision;
        _imageInfo.DriveSerialNumber     = metadata.DriveSerialNumber;

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

        if(heads > ushort.MaxValue)
        {
            ErrorMessage = Localization.Too_many_heads;

            return false;
        }

        if(sectorsPerTrack > ushort.MaxValue)
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
    public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetCicmMetadata(CICMMetadataType metadata) => false;
}