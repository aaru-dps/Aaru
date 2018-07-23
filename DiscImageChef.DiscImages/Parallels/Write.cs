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
//     Writes Parallels disk images.
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
// Copyright © 2011-2018 Natalia Portillo
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
    public partial class Parallels
    {
        // TODO: Support extended
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

            if(sectors * sectorSize / DEFAULT_CLUSTER_SIZE > uint.MaxValue)
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

            uint batEntries = (uint)(sectors * sectorSize / DEFAULT_CLUSTER_SIZE);
            if(sectors * sectorSize % DEFAULT_CLUSTER_SIZE > 0) batEntries++;
            uint headerSectors = (uint)Marshal.SizeOf(typeof(ParallelsHeader)) + batEntries * 4;
            if((uint)Marshal.SizeOf(typeof(ParallelsHeader)) + batEntries % 4 > 0) headerSectors++;

            pHdr = new ParallelsHeader
            {
                magic        = parallelsMagic,
                version      = PARALLELS_VERSION,
                sectors      = sectors,
                in_use       = PARALLELS_CLOSED,
                bat_entries  = batEntries,
                data_off     = headerSectors,
                cluster_size = DEFAULT_CLUSTER_SIZE / 512
            };

            bat                    = new uint[batEntries];
            currentWritingPosition = headerSectors * 512;

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

            if(data.Length != 512)
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

            ulong index  = sectorAddress / pHdr.cluster_size;
            ulong secOff = sectorAddress % pHdr.cluster_size;

            uint batOff = bat[index];

            if(batOff == 0)
            {
                batOff                 =  (uint)(currentWritingPosition / 512);
                bat[index]             =  batOff;
                currentWritingPosition += pHdr.cluster_size * 512;
            }

            ulong imageOff = batOff * 512;

            writingStream.Seek((long)imageOff,     SeekOrigin.Begin);
            writingStream.Seek((long)secOff * 512, SeekOrigin.Current);
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

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data)) return true;

            for(uint i = 0; i < length; i++)
            {
                byte[] tmp = new byte[512];
                Array.Copy(data, i * 512, tmp, 0, 512);
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

        public bool SetTracks(List<Track> tracks)
        {
            ErrorMessage = "Unsupported feature";
            return false;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            if(pHdr.cylinders == 0)
            {
                pHdr.cylinders            = (uint)(imageInfo.Sectors / 16 / 63);
                pHdr.heads                = 16;
                imageInfo.SectorsPerTrack = 63;

                while(pHdr.cylinders == 0)
                {
                    pHdr.heads--;

                    if(pHdr.heads == 0)
                    {
                        imageInfo.SectorsPerTrack--;
                        pHdr.heads = 16;
                    }

                    pHdr.cylinders = (uint)(imageInfo.Sectors / pHdr.heads / imageInfo.SectorsPerTrack);

                    if(pHdr.cylinders == 0 && pHdr.heads == 0 && imageInfo.SectorsPerTrack == 0) break;
                }
            }

            byte[] hdr    = new byte[Marshal.SizeOf(typeof(ParallelsHeader))];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ParallelsHeader)));
            Marshal.StructureToPtr(pHdr, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            for(long i = 0; i < bat.LongLength; i++) writingStream.Write(BitConverter.GetBytes(bat[i]), 0, 4);

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            pHdr.cylinders = cylinders;
            pHdr.heads     = heads;
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

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware)
        {
            // Not supported
            return false;
        }

        public bool SetCicmMetadata(CICMMetadataType metadata)
        {
            // Not supported
            return false;
        }
    }
}