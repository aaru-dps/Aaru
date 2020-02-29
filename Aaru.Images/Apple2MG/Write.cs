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
//     Writes XGS emulator disk images.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Schemas;

namespace Aaru.DiscImages
{
    public partial class Apple2Mg
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(sectorSize != 512)
                if(sectorSize != 256 ||
                   (mediaType != MediaType.Apple32SS && mediaType != MediaType.Apple33SS))
                {
                    ErrorMessage = "Unsupported sector size";

                    return false;
                }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";

                return false;
            }

            if(sectors > uint.MaxValue)
            {
                ErrorMessage = "Too many sectors";

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
            ErrorMessage = "Unsupported feature";

            return false;
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

            writingStream.Seek((long)(0x40 + (sectorAddress * imageInfo.SectorSize)), SeekOrigin.Begin);
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

            writingStream.Seek((long)(0x40 + (sectorAddress * imageInfo.SectorSize)), SeekOrigin.Begin);
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

            writingStream.Seek(0x40 + (17 * 16 * 256), SeekOrigin.Begin);
            byte[] tmp = new byte[256];
            writingStream.Read(tmp, 0, tmp.Length);

            bool isDos = tmp[0x01] == 17 && tmp[0x02] < 16 && tmp[0x27] <= 122 && tmp[0x34] == 35 && tmp[0x35] == 16 &&
                         tmp[0x36] == 0  && tmp[0x37] == 1;

            imageHeader = new A2ImgHeader
            {
                Blocks     = (uint)(imageInfo.Sectors * imageInfo.SectorSize) / 512, Creator = CREATOR_AARU,
                DataOffset = 0x40,
                DataSize =
                    (uint)(imageInfo.Sectors * imageInfo.SectorSize),
                Flags = (uint)(imageInfo.LastMediaSequence != 0 ? VALID_VOLUME_NUMBER + (imageInfo.MediaSequence & 0xFF)
                                   : 0),
                HeaderSize = 0x40, ImageFormat = isDos ? SectorOrder.Dos : SectorOrder.ProDos, Magic = MAGIC,
                Version    = 1
            };

            if(!string.IsNullOrEmpty(imageInfo.Comments))
            {
                writingStream.Seek(0, SeekOrigin.End);
                tmp                       = Encoding.UTF8.GetBytes(imageInfo.Comments);
                imageHeader.CommentOffset = (uint)writingStream.Position;
                imageHeader.CommentSize   = (uint)(tmp.Length + 1);
                writingStream.Write(tmp, 0, tmp.Length);
                writingStream.WriteByte(0);
            }

            byte[] hdr    = new byte[Marshal.SizeOf<A2ImgHeader>()];
            IntPtr hdrPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(Marshal.SizeOf<A2ImgHeader>());
            System.Runtime.InteropServices.Marshal.StructureToPtr(imageHeader, hdrPtr, true);
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
            imageInfo.Comments          = metadata.Comments;
            imageInfo.LastMediaSequence = metadata.LastMediaSequence;
            imageInfo.MediaSequence     = metadata.MediaSequence;

            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        public bool SetCicmMetadata(CICMMetadataType metadata) => false;
    }
}