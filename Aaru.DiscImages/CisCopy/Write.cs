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
//     Writes CisCopy disk images.
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class CisCopy
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

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try { writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            DiskType diskType;
            switch(mediaType)
            {
                case MediaType.DOS_35_DS_DD_9:
                    diskType = DiskType.MF2DD;
                    break;
                case MediaType.DOS_35_HD:
                    diskType = DiskType.MF2HD;
                    break;
                case MediaType.DOS_525_DS_DD_8:
                    diskType = DiskType.MD2DD8;
                    break;
                case MediaType.DOS_525_DS_DD_9:
                    diskType = DiskType.MD2DD;
                    break;
                case MediaType.DOS_525_HD:
                    diskType = DiskType.MD2HD;
                    break;
                case MediaType.DOS_525_SS_DD_8:
                    diskType = DiskType.MD1DD8;
                    break;
                case MediaType.DOS_525_SS_DD_9:
                    diskType = DiskType.MD1DD;
                    break;
                default:
                    ErrorMessage = $"Unsupport media format {mediaType}";
                    return false;
            }

            writingStream.WriteByte((byte)diskType);

            byte tracks = 0;

            switch(diskType)
            {
                case DiskType.MD1DD8:
                case DiskType.MD1DD:
                case DiskType.MD2DD8:
                case DiskType.MD2DD:
                    tracks = 80;
                    break;
                case DiskType.MF2DD:
                case DiskType.MD2HD:
                case DiskType.MF2HD:
                    tracks = 160;
                    break;
            }

            int headstep                                                           = 1;
            if(diskType == DiskType.MD1DD || diskType == DiskType.MD1DD8) headstep = 2;

            for(int i = 0; i < tracks; i += headstep)
            {
                writingStream.WriteByte((byte)TrackType.Copied);
                if(headstep == 2) writingStream.WriteByte(0);
            }

            writingStream.WriteByte((byte)Compression.None);
            writingOffset = writingStream.Position;

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

            writingStream.Seek(writingOffset + (long)(sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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

            writingStream.Seek(writingOffset + (long)(sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        public bool SetMetadata(ImageInfo metadata) => true;

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

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