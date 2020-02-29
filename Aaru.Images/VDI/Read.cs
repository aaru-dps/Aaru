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
//     Reads VirtualBox disk images.
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
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages
{
    public partial class Vdi
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] vHdrB = new byte[Marshal.SizeOf<VdiHeader>()];
            stream.Read(vHdrB, 0, Marshal.SizeOf<VdiHeader>());
            vHdr = Marshal.ByteArrayToStructureLittleEndian<VdiHeader>(vHdrB);

            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.creator = {0}", vHdr.creator);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.magic = {0}", vHdr.magic);

            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.version = {0}.{1}", vHdr.majorVersion,
                                       vHdr.minorVersion);

            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.headerSize = {0}", vHdr.headerSize);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.imageType = {0}", vHdr.imageType);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.imageFlags = {0}", vHdr.imageFlags);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.description = {0}", vHdr.comments);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.offsetBlocks = {0}", vHdr.offsetBlocks);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.offsetData = {0}", vHdr.offsetData);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.cylinders = {0}", vHdr.cylinders);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.heads = {0}", vHdr.heads);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.spt = {0}", vHdr.spt);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.sectorSize = {0}", vHdr.sectorSize);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.size = {0}", vHdr.size);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blockSize = {0}", vHdr.blockSize);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blockExtraData = {0}", vHdr.blockExtraData);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blocks = {0}", vHdr.blocks);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.allocatedBlocks = {0}", vHdr.allocatedBlocks);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.uuid = {0}", vHdr.uuid);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.snapshotUuid = {0}", vHdr.snapshotUuid);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.linkUuid = {0}", vHdr.linkUuid);
            AaruConsole.DebugWriteLine("VirtualBox plugin", "vHdr.parentUuid = {0}", vHdr.parentUuid);

            if(vHdr.imageType != VdiImageType.Normal)
                throw new
                    FeatureSupportedButNotImplementedImageException($"Support for image type {vHdr.imageType} not yet implemented");

            DateTime start = DateTime.UtcNow;
            AaruConsole.DebugWriteLine("VirtualBox plugin", "Reading Image Block Map");
            stream.Seek(vHdr.offsetBlocks, SeekOrigin.Begin);
            byte[] ibmB = new byte[vHdr.blocks * 4];
            stream.Read(ibmB, 0, ibmB.Length);
            ibm = MemoryMarshal.Cast<byte, uint>(ibmB).ToArray();
            DateTime end = DateTime.UtcNow;

            AaruConsole.DebugWriteLine("VirtualBox plugin", "Reading Image Block Map took {0} ms",
                                       (end - start).TotalMilliseconds);

            sectorCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = vHdr.size / vHdr.sectorSize;
            imageInfo.ImageSize            = vHdr.size;
            imageInfo.SectorSize           = vHdr.sectorSize;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.Comments             = vHdr.comments;
            imageInfo.Version              = $"{vHdr.majorVersion}.{vHdr.minorVersion}";

            switch(vHdr.creator)
            {
                case SUN_VDI:
                    imageInfo.Application = "Sun VirtualBox";

                    break;
                case SUN_OLD_VDI:
                    imageInfo.Application = "Sun xVM";

                    break;
                case ORACLE_VDI:
                    imageInfo.Application = "Oracle VirtualBox";

                    break;
                case QEMUVDI:
                    imageInfo.Application = "QEMU";

                    break;
                case INNOTEK_VDI:
                case INNOTEK_OLD_VDI:
                    imageInfo.Application = "innotek VirtualBox";

                    break;
                case DIC_VDI:
                    imageInfo.Application = "DiscImageChef";

                    break;
                case DIC_AARU:
                    imageInfo.Application = "Aaru";

                    break;
            }

            imageStream = stream;

            imageInfo.Cylinders       = vHdr.cylinders;
            imageInfo.Heads           = vHdr.heads;
            imageInfo.SectorsPerTrack = vHdr.spt;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector))
                return sector;

            ulong index  = (sectorAddress * vHdr.sectorSize) / vHdr.blockSize;
            ulong secOff = (sectorAddress * vHdr.sectorSize) % vHdr.blockSize;

            uint ibmOff = ibm[(int)index];

            if(ibmOff == VDI_EMPTY)
                return new byte[vHdr.sectorSize];

            ulong imageOff = vHdr.offsetData + (ibmOff * vHdr.blockSize);

            byte[] cluster = new byte[vHdr.blockSize];
            imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            imageStream.Read(cluster, 0, (int)vHdr.blockSize);
            sector = new byte[vHdr.sectorSize];
            Array.Copy(cluster, (int)secOff, sector, 0, vHdr.sectorSize);

            if(sectorCache.Count > MAX_CACHED_SECTORS)
                sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({sectorAddress} + {length}) than available ({imageInfo.Sectors})");

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}