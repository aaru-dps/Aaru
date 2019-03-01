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
//     Reads partclone disk images.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Extents;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Helpers;

namespace DiscImageChef.DiscImages
{
    public partial class PartClone
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] pHdrB = new byte[Marshal.SizeOf<PartCloneHeader>()];
            stream.Read(pHdrB, 0, Marshal.SizeOf<PartCloneHeader>());
            pHdr = Marshal.ByteArrayToStructureLittleEndian<PartCloneHeader>(pHdrB);

            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.magic = {0}", StringHandlers.CToString(pHdr.magic));
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.filesystem = {0}",
                                      StringHandlers.CToString(pHdr.filesystem));
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.version = {0}",
                                      StringHandlers.CToString(pHdr.version));
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.blockSize = {0}",   pHdr.blockSize);
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.deviceSize = {0}",  pHdr.deviceSize);
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.totalBlocks = {0}", pHdr.totalBlocks);
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.usedBlocks = {0}",  pHdr.usedBlocks);

            byteMap = new byte[pHdr.totalBlocks];
            DicConsole.DebugWriteLine("PartClone plugin", "Reading bytemap {0} bytes", byteMap.Length);
            stream.Read(byteMap, 0, byteMap.Length);

            byte[] bitmagic = new byte[8];
            stream.Read(bitmagic, 0, 8);

            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.bitmagic = {0}", StringHandlers.CToString(bitmagic));

            if(!biTmAgIc.SequenceEqual(bitmagic))
                throw new ImageNotSupportedException("Could not find partclone BiTmAgIc, not continuing...");

            dataOff = stream.Position;
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.dataOff = {0}", dataOff);

            DicConsole.DebugWriteLine("PartClone plugin", "Filling extents");
            DateTime start = DateTime.Now;
            extents    = new ExtentsULong();
            extentsOff = new Dictionary<ulong, ulong>();
            bool  current     = byteMap[0] > 0;
            ulong blockOff    = 0;
            ulong extentStart = 0;

            for(ulong i = 1; i < pHdr.totalBlocks; i++)
            {
                bool next = byteMap[i] > 0;

                // Flux
                if(next != current)
                    if(next)
                    {
                        extentStart = i;
                        extentsOff.Add(i, ++blockOff);
                    }
                    else
                    {
                        extents.Add(extentStart, i);
                        extentsOff.TryGetValue(extentStart, out _);
                    }

                if(next && current) blockOff++;

                current = next;
            }

            DateTime end = DateTime.Now;
            DicConsole.DebugWriteLine("PartClone plugin", "Took {0} seconds to fill extents",
                                      (end - start).TotalSeconds);

            sectorCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = pHdr.totalBlocks;
            imageInfo.SectorSize           = pHdr.blockSize;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.ImageSize            = (ulong)(stream.Length - (4096 + 0x40 + (long)pHdr.totalBlocks));
            imageStream                    = stream;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(byteMap[sectorAddress] == 0) return new byte[pHdr.blockSize];

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            long imageOff = dataOff + (long)(BlockOffset(sectorAddress) * (pHdr.blockSize + CRC_SIZE));

            sector = new byte[pHdr.blockSize];
            imageStream.Seek(imageOff, SeekOrigin.Begin);
            imageStream.Read(sector, 0, (int)pHdr.blockSize);

            if(sectorCache.Count > MAX_CACHED_SECTORS) sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream ms = new MemoryStream();

            bool allEmpty = true;
            for(uint i = 0; i < length; i++)
                if(byteMap[sectorAddress + i] != 0)
                {
                    allEmpty = false;
                    break;
                }

            if(allEmpty) return new byte[pHdr.blockSize * length];

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}