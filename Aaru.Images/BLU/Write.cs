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
//     Writes Basic Lisa Utility disk images.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders;
using Aaru.Helpers;
using Schemas;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.DiscImages
{
    public sealed partial class Blu
    {
        /// <inheritdoc />
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";

                return false;
            }

            if(sectors > 0xFFFFFF)
            {
                ErrorMessage = "Too many sectors";

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
                _writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
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
            int longSectorSize = _imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;

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

            if(sectorAddress >= _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            _writingStream.Seek(longSectorSize + ((long)sectorAddress * longSectorSize), SeekOrigin.Begin);
            _writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";

            return true;
        }

        /// <inheritdoc />
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            int longSectorSize = _imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;

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

            _writingStream.Seek(longSectorSize + ((long)sectorAddress * longSectorSize), SeekOrigin.Begin);
            _writingStream.Write(data, 0, data.Length);

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

            if(sectorAddress >= _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            int longSectorSize = _imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;

            byte[] oldTag;
            byte[] newTag;

            switch(data.Length - 512)
            {
                // Sony tag, convert to Profile
                case 12 when longSectorSize == 532:
                    oldTag = new byte[12];
                    Array.Copy(data, 512, oldTag, 0, 12);
                    newTag = LisaTag.DecodeSonyTag(oldTag)?.ToProfile().GetBytes();

                    break;

                // Sony tag, convert to Priam
                case 12:
                    oldTag = new byte[12];
                    Array.Copy(data, 512, oldTag, 0, 12);
                    newTag = LisaTag.DecodeSonyTag(oldTag)?.ToPriam().GetBytes();

                    break;

                // Profile tag, copy to Profile
                case 20 when longSectorSize == 532:
                    newTag = new byte[20];
                    Array.Copy(data, 512, newTag, 0, 20);

                    break;

                // Profile tag, convert to Priam
                case 20:
                    oldTag = new byte[20];
                    Array.Copy(data, 512, oldTag, 0, 20);
                    newTag = LisaTag.DecodeProfileTag(oldTag)?.ToPriam().GetBytes();

                    break;

                // Priam tag, convert to Profile
                case 24 when longSectorSize == 532:
                    oldTag = new byte[24];
                    Array.Copy(data, 512, oldTag, 0, 24);
                    newTag = LisaTag.DecodePriamTag(oldTag)?.ToProfile().GetBytes();

                    break;

                // Priam tag, copy to Priam
                case 24:
                    newTag = new byte[24];
                    Array.Copy(data, 512, newTag, 0, 24);

                    break;
                case 0:
                    newTag = null;

                    break;
                default:
                    ErrorMessage = "Incorrect data size";

                    return false;
            }

            newTag ??= new byte[longSectorSize - 512];

            _writingStream.Seek(longSectorSize + ((long)sectorAddress * longSectorSize), SeekOrigin.Begin);
            _writingStream.Write(data, 0, 512);
            _writingStream.Write(newTag, 0, newTag.Length);

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

            if(sectorAddress + length > _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            int  longSectorSize  = _imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;
            long givenSectorSize = data.Length / length;

            switch(givenSectorSize)
            {
                case 536:
                case 532:
                case 524:
                case 512: break;
                default:
                    ErrorMessage = "Incorrect data size";

                    return false;
            }

            for(uint i = 0; i < length; i++)
            {
                byte[] oldTag;
                byte[] newTag;

                switch(givenSectorSize - 512)
                {
                    // Sony tag, convert to Profile
                    case 12 when longSectorSize == 532:
                        oldTag = new byte[12];
                        Array.Copy(data, (givenSectorSize * i) + 512, oldTag, 0, 12);
                        newTag = LisaTag.DecodeSonyTag(oldTag)?.ToProfile().GetBytes();

                        break;

                    // Sony tag, convert to Priam
                    case 12:
                        oldTag = new byte[12];
                        Array.Copy(data, (givenSectorSize * i) + 512, oldTag, 0, 12);
                        newTag = LisaTag.DecodeSonyTag(oldTag)?.ToPriam().GetBytes();

                        break;

                    // Profile tag, copy to Profile
                    case 20 when longSectorSize == 532:
                        newTag = new byte[20];
                        Array.Copy(data, (givenSectorSize * i) + 512, newTag, 0, 20);

                        break;

                    // Profile tag, convert to Priam
                    case 20:
                        oldTag = new byte[20];
                        Array.Copy(data, (givenSectorSize * i) + 512, oldTag, 0, 20);
                        newTag = LisaTag.DecodeProfileTag(oldTag)?.ToPriam().GetBytes();

                        break;

                    // Priam tag, convert to Profile
                    case 24 when longSectorSize == 532:
                        oldTag = new byte[24];
                        Array.Copy(data, (givenSectorSize * i) + 512, oldTag, 0, 24);
                        newTag = LisaTag.DecodePriamTag(oldTag)?.ToProfile().GetBytes();

                        break;

                    // Priam tag, copy to Priam
                    case 24:
                        newTag = new byte[24];
                        Array.Copy(data, (givenSectorSize * i) + 512, newTag, 0, 24);

                        break;
                    case 0:
                        newTag = null;

                        break;
                    default:
                        ErrorMessage = "Incorrect data size";

                        return false;
                }

                newTag ??= new byte[longSectorSize - 512];

                _writingStream.Seek(longSectorSize + ((long)sectorAddress * longSectorSize), SeekOrigin.Begin);
                _writingStream.Write(data, (int)(givenSectorSize          * i), 512);
                _writingStream.Write(newTag, 0, newTag.Length);
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

            byte[] markerTag = Encoding.UTF8.GetBytes("Aaru " + Version.GetVersion());
            byte[] driveName;
            byte[] driveType      = new byte[3];
            byte[] driveBlocks    = BigEndianBitConverter.GetBytes((uint)_imageInfo.Sectors);
            int    longSectorSize = _imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;
            byte[] blockSize      = BigEndianBitConverter.GetBytes((ushort)longSectorSize);

            switch(_imageInfo.MediaType)
            {
                case MediaType.AppleProfile when _imageInfo.Sectors == 0x4C00:
                    driveName = Encoding.ASCII.GetBytes(PROFILE10_NAME);

                    break;
                case MediaType.AppleWidget when _imageInfo.Sectors == 0x4C00:
                    driveType[1] = 0x01;
                    driveName    = Encoding.ASCII.GetBytes(PROFILE10_NAME);

                    break;
                case MediaType.PriamDataTower when _imageInfo.Sectors == 0x22C7C:
                    driveType[1] = 0xFF;
                    driveName    = Encoding.ASCII.GetBytes(PRIAM_NAME);

                    break;
                default:
                    driveName = Encoding.ASCII.GetBytes(PROFILE_NAME);

                    break;
            }

            _writingStream.Seek(0, SeekOrigin.Begin);
            _writingStream.Write(driveName, 0, driveName.Length >= 0xD ? 0xD : driveName.Length);
            _writingStream.Seek(0xD, SeekOrigin.Begin);
            _writingStream.Write(driveType, 0, 3);
            _writingStream.Seek(0x12, SeekOrigin.Begin);
            _writingStream.Write(driveBlocks, 1, 3);
            _writingStream.Seek(0x15, SeekOrigin.Begin);
            _writingStream.Write(blockSize, 1, 2);
            _writingStream.Seek(512, SeekOrigin.Begin);

            _writingStream.Write(markerTag, 0,
                                 markerTag.Length >= longSectorSize - 512 ? longSectorSize - 512 : markerTag.Length);

            _writingStream.Flush();
            _writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        /// <inheritdoc />
        public bool SetMetadata(ImageInfo metadata) => true;

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
}