﻿// /***************************************************************************
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
//     Writes MaxiDisk disk images.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Schemas;

namespace Aaru.DiscImages
{
    public sealed partial class MaxiDisk
    {
        /// <inheritdoc />
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(CountBits.Count(sectorSize) != 1 ||
               sectorSize                  > 16384)
            {
                ErrorMessage = "Unsupported sector size";

                return false;
            }

            if(sectors > 90 * 2 * 255)
            {
                ErrorMessage = "Too many sectors";

                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupported media format {mediaType}";

                return false;
            }

            (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding encoding, bool
                variableSectorsPerTrack, MediaType type) geometry = Geometry.GetGeometry(mediaType);

            _imageInfo = new ImageInfo
            {
                MediaType       = mediaType,
                SectorSize      = sectorSize,
                Sectors         = sectors,
                Cylinders       = geometry.cylinders,
                Heads           = geometry.heads,
                SectorsPerTrack = geometry.sectorsPerTrack
            };

            if(_imageInfo.Cylinders > 90)
            {
                ErrorMessage = "Too many cylinders";

                return false;
            }

            if(_imageInfo.Heads > 2)
            {
                ErrorMessage = "Too many heads";

                return false;
            }

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

            var header = new Header
            {
                diskType        = (byte)HdkDiskTypes.Dos2880,
                cylinders       = (byte)_imageInfo.Cylinders,
                heads           = (byte)_imageInfo.Heads,
                sectorsPerTrack = (byte)_imageInfo.SectorsPerTrack
            };

            for(uint i = _imageInfo.SectorSize / 128; i > 1;)
            {
                header.bytesPerSector++;
                i >>= 1;
            }

            byte[] hdr    = new byte[Marshal.SizeOf<Header>()];
            IntPtr hdrPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<Header>());
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
        public bool SetMetadata(ImageInfo metadata) => true;

        /// <inheritdoc />
        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(cylinders > 90)
            {
                ErrorMessage = "Too many cylinders.";

                return false;
            }

            if(heads > 2)
            {
                ErrorMessage = "Too many heads.";

                return false;
            }

            if(sectorsPerTrack > byte.MaxValue)
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
}