// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class Nhdr0
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize != 0)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try { writingStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None); }
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
            ErrorMessage = "Writing media tags is not supported.";
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

            writingStream
               .Seek((long)((ulong)Marshal.SizeOf(typeof(Nhdr0Header)) + sectorAddress * imageInfo.SectorSize),
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

            writingStream
               .Seek((long)((ulong)Marshal.SizeOf(typeof(Nhdr0Header)) + sectorAddress * imageInfo.SectorSize),
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
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 8 / 17);
                imageInfo.Heads           = 8;
                imageInfo.SectorsPerTrack = 17;

                while(imageInfo.Cylinders == 0)
                {
                    imageInfo.Heads--;

                    if(imageInfo.Heads == 0)
                    {
                        imageInfo.SectorsPerTrack--;
                        imageInfo.Heads = 16;
                    }

                    imageInfo.Cylinders = (uint)(imageInfo.Sectors / imageInfo.Heads / imageInfo.SectorsPerTrack);

                    if(imageInfo.Cylinders == 0 && imageInfo.Heads == 0 && imageInfo.SectorsPerTrack == 0) break;
                }
            }

            Nhdr0Header header = new Nhdr0Header
            {
                szFileID   = signature,
                szComment  = new byte[0x100],
                dwHeadSize = Marshal.SizeOf(typeof(Nhdr0Header)),
                dwCylinder = (byte)imageInfo.Cylinders,
                wHead      = (byte)imageInfo.Heads,
                wSect      = (byte)imageInfo.SectorsPerTrack,
                wSectLen   = (byte)imageInfo.SectorSize,
                reserved2  = new byte[2],
                reserved3  = new byte[0xE0]
            };

            if(!string.IsNullOrEmpty(imageInfo.Comments))
            {
                byte[] commentBytes = Encoding.GetEncoding("shift_jis").GetBytes(imageInfo.Comments);
                Array.Copy(commentBytes, 0, header.szComment, 0,
                           commentBytes.Length >= 0x100 ? 0x100 : commentBytes.Length);
            }

            byte[] hdr    = new byte[Marshal.SizeOf(header)];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.StructureToPtr(header, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            writingStream.Flush();
            writingStream.Close();
            IsWriting = false;

            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            imageInfo.Comments = metadata.Comments;
            return true;
        }

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