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
// Copyright © 2011-2020 Natalia Portillo
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

namespace Aaru.DiscImages
{
    public partial class RsIde
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(sectorSize != 256 &&
               sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";

                return false;
            }

            if(sectors > 63 * 16 * 1024)
            {
                ErrorMessage = "Too many sectors";

                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";

                return false;
            }

            imageInfo = new ImageInfo
            {
                MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors
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

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            if(tag != MediaTagType.ATA_IDENTIFY)
            {
                ErrorMessage = $"Unsupported media tag {tag}.";

                return false;
            }

            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            identify = new byte[106];
            Array.Copy(data, 0, identify, 0, 106);

            return true;
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length != imageInfo.SectorSize)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            writingStream.Seek((long)((ulong)Marshal.SizeOf<RsIdeHeader>() + (sectorAddress * imageInfo.SectorSize)),
                               SeekOrigin.Begin);

            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";

            return true;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length % imageInfo.SectorSize != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            writingStream.Seek((long)((ulong)Marshal.SizeOf<RsIdeHeader>() + (sectorAddress * imageInfo.SectorSize)),
                               SeekOrigin.Begin);

            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";

            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";

                return false;
            }

            if(imageInfo.Cylinders == 0)
            {
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads           = 16;
                imageInfo.SectorsPerTrack = 63;

                while(imageInfo.Cylinders == 0)
                {
                    imageInfo.Heads--;

                    if(imageInfo.Heads == 0)
                    {
                        imageInfo.SectorsPerTrack--;
                        imageInfo.Heads = 16;
                    }

                    imageInfo.Cylinders = (uint)(imageInfo.Sectors / imageInfo.Heads / imageInfo.SectorsPerTrack);

                    if(imageInfo.Cylinders       == 0 &&
                       imageInfo.Heads           == 0 &&
                       imageInfo.SectorsPerTrack == 0)
                        break;
                }
            }

            var header = new RsIdeHeader
            {
                magic    = signature, identify = new byte[106], dataOff = (ushort)Marshal.SizeOf<RsIdeHeader>(),
                revision = 1, reserved         = new byte[11]
            };

            if(imageInfo.SectorSize == 256)
                header.flags = RsIdeFlags.HalfSectors;

            if(identify == null)
            {
                var ataId = new Identify.IdentifyDevice
                {
                    GeneralConfiguration =
                        CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.UltraFastIDE |
                        CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.Fixed        |
                        CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.NotMFM       |
                        CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.SoftSector,
                    Cylinders       = (ushort)imageInfo.Cylinders, Heads              = (ushort)imageInfo.Heads,
                    SectorsPerTrack = (ushort)imageInfo.SectorsPerTrack, VendorWord47 = 0x80,
                    Capabilities = CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.DMASupport |
                                   CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.IORDY      |
                                   CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.LBASupport,
                    ExtendedIdentify =
                        CommonTypes.Structs.Devices.ATA.Identify.ExtendedIdentifyBit.Words54to58Valid,
                    CurrentCylinders       = (ushort)imageInfo.Cylinders, CurrentHeads = (ushort)imageInfo.Heads,
                    CurrentSectorsPerTrack = (ushort)imageInfo.SectorsPerTrack,
                    CurrentSectors         = (uint)imageInfo.Sectors, LBASectors = (uint)imageInfo.Sectors,
                    DMASupported           = CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0,
                    DMAActive              = CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0
                };

                if(string.IsNullOrEmpty(imageInfo.DriveManufacturer))
                    imageInfo.DriveManufacturer = "Aaru";

                if(string.IsNullOrEmpty(imageInfo.DriveModel))
                    imageInfo.DriveModel = "";

                if(string.IsNullOrEmpty(imageInfo.DriveFirmwareRevision))
                    Version.GetVersion();

                if(string.IsNullOrEmpty(imageInfo.DriveSerialNumber))
                    imageInfo.DriveSerialNumber = $"{new Random().NextDouble():16X}";

                byte[] ataIdBytes = new byte[Marshal.SizeOf<Identify.IdentifyDevice>()];
                IntPtr ptr        = System.Runtime.InteropServices.Marshal.AllocHGlobal(512);
                System.Runtime.InteropServices.Marshal.StructureToPtr(ataId, ptr, true);

                System.Runtime.InteropServices.Marshal.Copy(ptr, ataIdBytes, 0,
                                                            Marshal.SizeOf<Identify.IdentifyDevice>());

                System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);

                Array.Copy(ScrambleAtaString(imageInfo.DriveManufacturer + " " + imageInfo.DriveModel, 40), 0,
                           ataIdBytes, 27 * 2, 40);

                Array.Copy(ScrambleAtaString(imageInfo.DriveFirmwareRevision, 8), 0, ataIdBytes, 23 * 2, 8);
                Array.Copy(ScrambleAtaString(imageInfo.DriveSerialNumber, 20), 0, ataIdBytes, 10    * 2, 20);
                Array.Copy(ataIdBytes, 0, header.identify, 0, 106);
            }
            else
                Array.Copy(identify, 0, header.identify, 0, 106);

            byte[] hdr    = new byte[Marshal.SizeOf<RsIdeHeader>()];
            IntPtr hdrPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<RsIdeHeader>());
            System.Runtime.InteropServices.Marshal.StructureToPtr(header, hdrPtr, true);
            System.Runtime.InteropServices.Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            imageInfo.DriveManufacturer     = metadata.DriveManufacturer;
            imageInfo.DriveModel            = metadata.DriveModel;
            imageInfo.DriveFirmwareRevision = metadata.DriveFirmwareRevision;
            imageInfo.DriveSerialNumber     = metadata.DriveSerialNumber;

            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(cylinders > ushort.MaxValue)
            {
                ErrorMessage = "Too many cylinders.";

                return false;
            }

            if(heads > ushort.MaxValue)
            {
                ErrorMessage = "Too many heads.";

                return false;
            }

            if(sectorsPerTrack > ushort.MaxValue)
            {
                ErrorMessage = "Too many sectors per track.";

                return false;
            }

            imageInfo.SectorsPerTrack = sectorsPerTrack;
            imageInfo.Heads           = heads;
            imageInfo.Cylinders       = cylinders;

            return true;
        }

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        public bool SetCicmMetadata(CICMMetadataType metadata) => false;
    }
}