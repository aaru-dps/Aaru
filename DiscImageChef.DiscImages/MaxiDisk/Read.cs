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
//     Reads MaxiDisk disk images.
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
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.DiscImages
{
    public partial class MaxiDisk
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 8) return false;

            byte[] buffer = new byte[8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            HdkHeader tmpHeader = (HdkHeader)Marshal.PtrToStructure(ftrPtr, typeof(HdkHeader));
            Marshal.FreeHGlobal(ftrPtr);

            // This is hardcoded
            // But its possible values are unknown...
            //if(tmp_header.diskType > 11)
            //    return false;

            // Only floppies supported
            if(tmpHeader.heads == 0 || tmpHeader.heads > 2) return false;

            // No floppies with more than this?
            if(tmpHeader.cylinders > 90) return false;

            // Maximum supported bps is 16384
            if(tmpHeader.bytesPerSector > 7) return false;

            int expectedFileSize = tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack *
                                   (128 << tmpHeader.bytesPerSector) + 8;

            if(expectedFileSize != stream.Length) return false;

            imageInfo.Cylinders       = tmpHeader.cylinders;
            imageInfo.Heads           = tmpHeader.heads;
            imageInfo.SectorsPerTrack = tmpHeader.sectorsPerTrack;
            imageInfo.Sectors         = (ulong)(tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack);
            imageInfo.SectorSize      = (uint)(128 << tmpHeader.bytesPerSector);

            hdkImageFilter = imageFilter;

            imageInfo.ImageSize            = (ulong)(stream.Length - 8);
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

            imageInfo.MediaType = Geometry.GetMediaType(((ushort)imageInfo.Cylinders, (byte)imageInfo.Heads,
                                                         (ushort)imageInfo.SectorsPerTrack, imageInfo.SectorSize,
                                                         MediaEncoding.MFM, false));

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

            Stream stream = hdkImageFilter.GetDataForkStream();
            stream.Seek((long)(8 + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }
    }
}