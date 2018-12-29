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
//     Reads HD-Copy disk images.
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
// Copyright © 2017 Michael Drüing
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;

namespace DiscImageChef.DiscImages
{
    public partial class HdCopy
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] header = new byte[2 + 2 * 82];
            stream.Read(header, 0, 2 + 2 * 82);

            IntPtr hdrPtr = Marshal.AllocHGlobal(2 + 2 * 82);
            Marshal.Copy(header, 0, hdrPtr, 2 + 2 * 82);
            HdcpFileHeader fheader = (HdcpFileHeader)Marshal.PtrToStructure(hdrPtr, typeof(HdcpFileHeader));
            Marshal.FreeHGlobal(hdrPtr);
            DicConsole.DebugWriteLine("HDCP plugin",
                                      "Detected HD-Copy image with {0} tracks and {1} sectors per track.",
                                      fheader.lastCylinder + 1, fheader.sectorsPerTrack);

            imageInfo.Cylinders       = (uint)fheader.lastCylinder + 1;
            imageInfo.SectorsPerTrack = fheader.sectorsPerTrack;
            imageInfo.SectorSize      = 512; // only 512 bytes per sector supported
            imageInfo.Heads           = 2;   // only 2-sided floppies are supported
            imageInfo.Sectors         = 2 * imageInfo.Cylinders * imageInfo.SectorsPerTrack;
            imageInfo.ImageSize       = imageInfo.Sectors       * imageInfo.SectorSize;

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.MediaType = Geometry.GetMediaType(((ushort)imageInfo.Cylinders, 2,
                                                         (ushort)imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                         false));

            // the start offset of the track data
            long currentOffset = 2 + 2 * 82;

            // build table of track offsets
            for(int i = 0; i < imageInfo.Cylinders * 2; i++)
                if(fheader.trackMap[i] == 0)
                    trackOffset[i] = -1;
                else
                {
                    // track is present, read the block header
                    if(currentOffset + 3 >= stream.Length) return false;

                    byte[] blkHeader = new byte[2];
                    stream.Read(blkHeader, 0, 2);
                    short blkLength = BitConverter.ToInt16(blkHeader, 0);

                    // assume block sizes are positive
                    if(blkLength < 0) return false;

                    DicConsole.DebugWriteLine("HDCP plugin", "Track {0} offset 0x{1:x8}, size={2:x4}", i, currentOffset,
                                              blkLength);
                    trackOffset[i] = currentOffset;

                    currentOffset += 2 + blkLength;
                    // skip the block data
                    stream.Seek(blkLength, SeekOrigin.Current);
                }

            // ensure that the last track is present completely
            if(currentOffset > stream.Length) return false;

            // save some variables for later use
            fileHeader      = fheader;
            hdcpImageFilter = imageFilter;
            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            int trackNum     = (int)(sectorAddress / imageInfo.SectorsPerTrack);
            int sectorOffset = (int)(sectorAddress % imageInfo.SectorsPerTrack);

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(trackNum > 2 * imageInfo.Cylinders)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            byte[] result = new byte[imageInfo.SectorSize];
            if(trackOffset[trackNum] == -1) Array.Clear(result, 0, (int)imageInfo.SectorSize);
            else
            {
                // track is present in file, make sure it has been loaded
                if(!trackCache.ContainsKey(trackNum)) ReadTrackIntoCache(hdcpImageFilter.GetDataForkStream(), trackNum);

                Array.Copy(trackCache[trackNum], sectorOffset * imageInfo.SectorSize, result, 0, imageInfo.SectorSize);
            }

            return result;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            byte[] result = new byte[length * imageInfo.SectorSize];

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            for(int i = 0; i < length; i++)
                ReadSector(sectorAddress + (ulong)i).CopyTo(result, i * imageInfo.SectorSize);

            return result;
        }
    }
}