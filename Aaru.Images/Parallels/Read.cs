// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Parallels disk images.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class Parallels
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] pHdrB = new byte[Marshal.SizeOf<ParallelsHeader>()];
            stream.Read(pHdrB, 0, Marshal.SizeOf<ParallelsHeader>());
            pHdr = Marshal.ByteArrayToStructureLittleEndian<ParallelsHeader>(pHdrB);

            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.magic = {0}",
                                      StringHandlers.CToString(pHdr.magic));
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.version = {0}",      pHdr.version);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.heads = {0}",        pHdr.heads);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.cylinders = {0}",    pHdr.cylinders);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.cluster_size = {0}", pHdr.cluster_size);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.bat_entries = {0}",  pHdr.bat_entries);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.sectors = {0}",      pHdr.sectors);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.in_use = 0x{0:X8}",  pHdr.in_use);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.data_off = {0}",     pHdr.data_off);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.flags = {0}",        pHdr.flags);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.ext_off = {0}",      pHdr.ext_off);

            extended = parallelsExtMagic.SequenceEqual(pHdr.magic);
            DicConsole.DebugWriteLine("Parallels plugin", "pHdr.extended = {0}", extended);

            DicConsole.DebugWriteLine("Parallels plugin", "Reading BAT");
            bat = new uint[pHdr.bat_entries];
            byte[] batB = new byte[pHdr.bat_entries * 4];
            stream.Read(batB, 0, batB.Length);
            for(int i = 0; i < bat.Length; i++) bat[i] = BitConverter.ToUInt32(batB, i * 4);

            clusterBytes = pHdr.cluster_size * 512;
            if(pHdr.data_off > 0) dataOffset = pHdr.data_off * 512;
            else
                dataOffset =
                    (stream.Position / clusterBytes + stream.Position % clusterBytes) * clusterBytes;

            sectorCache = new Dictionary<ulong, byte[]>();

            empty = (pHdr.flags & PARALLELS_EMPTY) == PARALLELS_EMPTY;

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = pHdr.sectors;
            imageInfo.SectorSize           = 512;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.ImageSize            = pHdr.sectors * 512;
            imageInfo.Cylinders            = pHdr.cylinders;
            imageInfo.Heads                = pHdr.heads;
            imageInfo.SectorsPerTrack      = (uint)(imageInfo.Sectors / imageInfo.Cylinders / imageInfo.Heads);
            imageStream                    = stream;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(empty) return new byte[512];

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            ulong index  = sectorAddress / pHdr.cluster_size;
            ulong secOff = sectorAddress % pHdr.cluster_size;

            uint  batOff = bat[index];
            ulong imageOff;

            if(batOff == 0) return new byte[512];

            if(extended) imageOff = batOff * clusterBytes;
            else imageOff         = batOff * 512;

            byte[] cluster = new byte[clusterBytes];
            imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            imageStream.Read(cluster, 0, (int)clusterBytes);
            sector = new byte[512];
            Array.Copy(cluster, (int)(secOff * 512), sector, 0, 512);

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

            if(empty) return new byte[512 * length];

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}