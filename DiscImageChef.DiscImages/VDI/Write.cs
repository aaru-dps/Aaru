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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class Vdi
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            if(sectors * sectorSize / DEFAULT_BLOCK_SIZE > uint.MaxValue)
            {
                ErrorMessage = "Too many sectors for selected cluster size";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try { writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            uint ibmEntries = (uint)(sectors * sectorSize / DEFAULT_BLOCK_SIZE);
            if(sectors * sectorSize % DEFAULT_BLOCK_SIZE > 0) ibmEntries++;

            uint headerSectors = 1 + ibmEntries * 4 / sectorSize;
            if(ibmEntries * 4 % sectorSize != 0) headerSectors++;
            ibm                    = new uint[ibmEntries];
            currentWritingPosition = headerSectors * sectorSize;

            vHdr = new VdiHeader
            {
                creator      = DIC_VDI,
                magic        = VDI_MAGIC,
                majorVersion = 1,
                minorVersion = 1,
                headerSize   = Marshal.SizeOf(typeof(VdiHeader)) - 72,
                imageType    = VdiImageType.Normal,
                offsetBlocks = sectorSize,
                offsetData   = currentWritingPosition,
                sectorSize   = sectorSize,
                size         = sectors * sectorSize,
                blockSize    = DEFAULT_BLOCK_SIZE,
                blocks       = ibmEntries,
                uuid         = Guid.NewGuid(),
                snapshotUuid = Guid.NewGuid()
            };

            for(uint i = 0; i < ibmEntries; i++) ibm[i] = VDI_EMPTY;

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

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

            ulong index  = sectorAddress * vHdr.sectorSize / vHdr.blockSize;
            ulong secOff = sectorAddress * vHdr.sectorSize % vHdr.blockSize;

            uint ibmOff = ibm[index];

            if(ibmOff == VDI_EMPTY)
            {
                ibmOff                 =  (currentWritingPosition - vHdr.offsetData) / vHdr.blockSize;
                ibm[index]             =  ibmOff;
                currentWritingPosition += vHdr.blockSize;
                vHdr.allocatedBlocks++;
            }

            ulong imageOff = vHdr.offsetData + ibmOff * vHdr.blockSize;

            writingStream.Seek((long)imageOff, SeekOrigin.Begin);
            writingStream.Seek((long)secOff,   SeekOrigin.Current);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";
            return true;
        }

        // TODO: This can be optimized
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

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

            for(uint i = 0; i < length; i++)
            {
                byte[] tmp = new byte[imageInfo.SectorSize];
                Array.Copy(data, i * imageInfo.SectorSize, tmp, 0, imageInfo.SectorSize);
                if(!WriteSector(tmp, sectorAddress + i)) return false;
            }

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

            if(!string.IsNullOrEmpty(imageInfo.Comments))
                vHdr.comments = imageInfo.Comments.Length > 255
                                    ? imageInfo.Comments.Substring(0, 255)
                                    : imageInfo.Comments;

            if(vHdr.cylinders == 0)
            {
                vHdr.cylinders = (uint)(imageInfo.Sectors / 16 / 63);
                vHdr.heads     = 16;
                vHdr.spt       = 63;

                while(vHdr.cylinders == 0)
                {
                    vHdr.heads--;

                    if(vHdr.heads == 0)
                    {
                        vHdr.spt--;
                        vHdr.heads = 16;
                    }

                    vHdr.cylinders = (uint)(imageInfo.Sectors / vHdr.heads / vHdr.spt);

                    if(vHdr.cylinders == 0 && vHdr.heads == 0 && vHdr.spt == 0) break;
                }
            }

            byte[] hdr    = new byte[Marshal.SizeOf(typeof(VdiHeader))];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VdiHeader)));
            Marshal.StructureToPtr(vHdr, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            writingStream.Seek(vHdr.offsetBlocks, SeekOrigin.Begin);
            for(long i = 0; i < ibm.LongLength; i++) writingStream.Write(BitConverter.GetBytes(ibm[i]), 0, 4);

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            imageInfo.Comments = metadata.Comments;
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            vHdr.cylinders = cylinders;
            vHdr.heads     = heads;
            vHdr.spt       = sectorsPerTrack;
            return true;
        }

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