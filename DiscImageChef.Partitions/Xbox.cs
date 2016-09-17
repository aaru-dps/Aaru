// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Xbox.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Xbox partitions.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    class Xbox : PartPlugin
    {
        const uint XboxCigam = 0x46415458;
        const uint XboxMagic = 0x58544146;
        const long MemoryUnitDataOff = 0x7FF000;
        const long Xbox360SecuritySectorOff = 0x2000;
        const long Xbox360SystemCacheOff = 0x80000;
        const long Xbox360GameCacheOff = 0x8008000;
        const long Xbox368SysExtOff = 0x10C080000;
        const long Xbox360SysExt2Off = 0x118EB0000;
        const long Xbox360CompatOff = 0x120EB0000;
        const long Xbox360DataOff = 0x130EB0000;

        const long Xbox360SecuritySectorLen = 0x80000;
        const long Xbox360SystemCacheLen = 0x80000000;
        const long Xbox360GameCacheLen = 0xA0E30000;
        const long Xbox368SysExtLen = 0xCE30000;
        const long Xbox360SysExt2Len = 0x8000000;
        const long Xbox360CompatLen = 0x10000000;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Xbox360DevKitPartitionTable
        {
            public uint magic;
            public uint unknown;
            public uint contentOff;
            public uint contentLen;
            public uint dashboardOff;
            public uint dashboardLen;
        }

        const uint Xbox360DevKitMagic = 0x00020000;

        public Xbox()
        {
            Name = "Xbox partitioning";
            PluginUUID = new Guid("E3F6FB91-D358-4F22-A550-81E92D50EB78");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions)
        {
            partitions = new List<Partition>();

            byte[] sector = imagePlugin.ReadSector(0);
            if(sector.Length < 512)
                return false;

            Xbox360DevKitPartitionTable table = BigEndianMarshal.ByteArrayToStructureBigEndian<Xbox360DevKitPartitionTable>(sector);

            if(table.magic == Xbox360DevKitMagic && table.contentOff + table.contentLen <= imagePlugin.ImageInfo.sectors &&
               table.dashboardOff + table.dashboardLen <= imagePlugin.ImageInfo.sectors)
            {
                Partition contentPart = new Partition();
                contentPart.PartitionDescription = "Content volume";
                contentPart.PartitionLength = (ulong)table.contentLen * (ulong)imagePlugin.ImageInfo.sectorSize;
                contentPart.PartitionSectors = table.contentLen;
                contentPart.PartitionSequence = 1;
                contentPart.PartitionStart = (ulong)table.contentOff * (ulong)imagePlugin.ImageInfo.sectorSize;
                contentPart.PartitionStartSector = table.contentOff;

                Partition dashboardPart = new Partition();
                dashboardPart.PartitionDescription = "Dashboard volume";
                dashboardPart.PartitionLength = (ulong)table.dashboardLen * (ulong)imagePlugin.ImageInfo.sectorSize;
                dashboardPart.PartitionSectors = table.dashboardLen;
                dashboardPart.PartitionSequence = 2;
                dashboardPart.PartitionStart = (ulong)table.dashboardOff * (ulong)imagePlugin.ImageInfo.sectorSize;
                dashboardPart.PartitionStartSector = table.dashboardOff;

                partitions.Add(contentPart);
                partitions.Add(dashboardPart);

                return true;
            }

            uint temp = 0;

            if(imagePlugin.ImageInfo.sectors > (ulong)(MemoryUnitDataOff / imagePlugin.ImageInfo.sectorSize))
            {
                sector = imagePlugin.ReadSector((ulong)(MemoryUnitDataOff / imagePlugin.ImageInfo.sectorSize));
                temp = BitConverter.ToUInt32(sector, 0);

                if(temp == XboxCigam)
                {
                    Partition sysCachePart = new Partition();
                    sysCachePart.PartitionDescription = "System cache";
                    sysCachePart.PartitionLength = MemoryUnitDataOff;
                    sysCachePart.PartitionSectors = (ulong)(MemoryUnitDataOff / imagePlugin.ImageInfo.sectorSize);
                    sysCachePart.PartitionSequence = 1;
                    sysCachePart.PartitionStart = 0;
                    sysCachePart.PartitionStartSector = 0;

                    Partition dataPart = new Partition();
                    dataPart.PartitionDescription = "Data volume";
                    dataPart.PartitionLength = (ulong)imagePlugin.ImageInfo.sectors * (ulong)imagePlugin.ImageInfo.sectorSize - MemoryUnitDataOff;
                    dataPart.PartitionSectors = imagePlugin.ImageInfo.sectors - sysCachePart.PartitionSectors;
                    dataPart.PartitionSequence = 2;
                    dataPart.PartitionStart = MemoryUnitDataOff;
                    dataPart.PartitionStartSector = sysCachePart.PartitionSectors;

                    partitions.Add(sysCachePart);
                    partitions.Add(dataPart);

                    return true;
                }
            }

            if(imagePlugin.ImageInfo.sectors > (ulong)(Xbox360DataOff / imagePlugin.ImageInfo.sectorSize))
            {
                sector = imagePlugin.ReadSector((ulong)(Xbox360DataOff / imagePlugin.ImageInfo.sectorSize));
                temp = BitConverter.ToUInt32(sector, 0);

                if(temp == XboxCigam)
                {
                    Partition securityPart = new Partition();
                    securityPart.PartitionDescription = "Security sectors";
                    securityPart.PartitionLength = Xbox360SecuritySectorLen;
                    securityPart.PartitionSectors = (ulong)(Xbox360SecuritySectorLen / imagePlugin.ImageInfo.sectorSize);
                    securityPart.PartitionSequence = 1;
                    securityPart.PartitionStart = Xbox360SecuritySectorOff;
                    securityPart.PartitionStartSector = (ulong)(Xbox360SecuritySectorOff / imagePlugin.ImageInfo.sectorSize);

                    Partition sysCachePart = new Partition();
                    sysCachePart.PartitionDescription = "System cache";
                    sysCachePart.PartitionLength = Xbox360SystemCacheLen;
                    sysCachePart.PartitionSectors = (ulong)(Xbox360SystemCacheLen / imagePlugin.ImageInfo.sectorSize);
                    sysCachePart.PartitionSequence = 2;
                    sysCachePart.PartitionStart = Xbox360SystemCacheOff;
                    sysCachePart.PartitionStartSector = (ulong)(Xbox360SystemCacheOff / imagePlugin.ImageInfo.sectorSize);

                    Partition gameCachePart = new Partition();
                    gameCachePart.PartitionDescription = "Game cache";
                    gameCachePart.PartitionLength = Xbox360GameCacheLen;
                    gameCachePart.PartitionSectors = (ulong)(Xbox360GameCacheLen / imagePlugin.ImageInfo.sectorSize);
                    gameCachePart.PartitionSequence = 3;
                    gameCachePart.PartitionStart = Xbox360GameCacheOff;
                    gameCachePart.PartitionStartSector = (ulong)(Xbox360GameCacheOff / imagePlugin.ImageInfo.sectorSize);

                    Partition sysExtPart = new Partition();
                    sysExtPart.PartitionDescription = "System volume";
                    sysExtPart.PartitionLength = Xbox368SysExtLen;
                    sysExtPart.PartitionSectors = (ulong)(Xbox368SysExtLen / imagePlugin.ImageInfo.sectorSize);
                    sysExtPart.PartitionSequence = 4;
                    sysExtPart.PartitionStart = Xbox368SysExtOff;
                    sysExtPart.PartitionStartSector = (ulong)(Xbox368SysExtOff / imagePlugin.ImageInfo.sectorSize);

                    Partition sysExt2Part = new Partition();
                    sysExt2Part.PartitionDescription = "System volume 2";
                    sysExt2Part.PartitionLength = Xbox360SysExt2Len;
                    sysExt2Part.PartitionSectors = (ulong)(Xbox360SysExt2Len / imagePlugin.ImageInfo.sectorSize);
                    sysExt2Part.PartitionSequence = 5;
                    sysExt2Part.PartitionStart = Xbox360SysExt2Off;
                    sysExt2Part.PartitionStartSector = (ulong)(Xbox360SysExt2Off / imagePlugin.ImageInfo.sectorSize);

                    Partition xbox1Part = new Partition();
                    xbox1Part.PartitionDescription = "Xbox backwards compatibility";
                    xbox1Part.PartitionLength = Xbox360CompatLen;
                    xbox1Part.PartitionSectors = (ulong)(Xbox360CompatLen / imagePlugin.ImageInfo.sectorSize);
                    xbox1Part.PartitionSequence = 6;
                    xbox1Part.PartitionStart = Xbox360CompatOff;
                    xbox1Part.PartitionStartSector = (ulong)(Xbox360CompatOff / imagePlugin.ImageInfo.sectorSize);

                    Partition dataPart = new Partition();
                    dataPart.PartitionDescription = "Data volume";
                    dataPart.PartitionSequence = 7;
                    dataPart.PartitionStart = Xbox360DataOff;
                    dataPart.PartitionStartSector = (ulong)(Xbox360DataOff / imagePlugin.ImageInfo.sectorSize);
                    dataPart.PartitionSectors = imagePlugin.ImageInfo.sectors - dataPart.PartitionStartSector;
                    dataPart.PartitionLength = (ulong)dataPart.PartitionSectors * (ulong)imagePlugin.ImageInfo.sectorSize;

                    partitions.Add(securityPart);
                    partitions.Add(sysCachePart);
                    partitions.Add(gameCachePart);
                    partitions.Add(sysExtPart);
                    partitions.Add(sysExt2Part);
                    partitions.Add(xbox1Part);
                    partitions.Add(dataPart);

                    return true;
                }
            }

            return false;
        }
    }
}