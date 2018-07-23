// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Ray Arachelian's disk images.
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.DiscImages
{
    public partial class RayDim
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < Marshal.SizeOf(typeof(RayHdr))) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(RayHdr))];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            RayHdr header = (RayHdr)Marshal.PtrToStructure(ftrPtr, typeof(RayHdr));
            Marshal.FreeHGlobal(ftrPtr);

            string signature = StringHandlers.CToString(header.signature);

            Regex sx = new Regex(REGEX_SIGNATURE);
            Match sm = sx.Match(signature);

            if(!sm.Success) return false;

            imageInfo.ApplicationVersion = $"{sm.Groups["major"].Value}.{sm.Groups["minor"].Value}";

            imageInfo.Cylinders       = (uint)(header.cylinders + 1);
            imageInfo.Heads           = (uint)(header.heads     + 1);
            imageInfo.SectorsPerTrack = header.sectorsPerTrack;
            imageInfo.Sectors         = imageInfo.Cylinders * imageInfo.Heads * imageInfo.SectorsPerTrack;
            imageInfo.SectorSize      = 512;

            byte[] sectors = new byte[imageInfo.SectorsPerTrack * imageInfo.SectorSize];
            disk = new MemoryStream();

            for(int i = 0; i < imageInfo.SectorsPerTrack * imageInfo.SectorSize; i++)
            {
                stream.Read(sectors, 0, sectors.Length);
                stream.Seek(imageInfo.SectorsPerTrack, SeekOrigin.Current);
                disk.Write(sectors, 0, sectors.Length);
            }

            imageInfo.MediaType = Geometry.GetMediaType(((ushort)imageInfo.Cylinders, (byte)imageInfo.Heads,
                                                         (ushort)imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                         false));

            switch(imageInfo.MediaType)
            {
                case MediaType.NEC_525_HD
                    when header.diskType == RayDiskTypes.Mf2hd || header.diskType == RayDiskTypes.Mf2ed:
                    imageInfo.MediaType = MediaType.NEC_35_HD_8;
                    break;
                case MediaType.DOS_525_HD
                    when header.diskType == RayDiskTypes.Mf2hd || header.diskType == RayDiskTypes.Mf2ed:
                    imageInfo.MediaType = MediaType.NEC_35_HD_15;
                    break;
                case MediaType.RX50 when header.diskType == RayDiskTypes.Md2dd || header.diskType == RayDiskTypes.Md2hd:
                    imageInfo.MediaType = MediaType.ATARI_35_SS_DD;
                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            disk.Seek((long)(sectorAddress    * imageInfo.SectorSize), SeekOrigin.Begin);
            disk.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }
    }
}