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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    public class Xbox : PartPlugin
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
                contentPart.Description = "Content volume";
                contentPart.Size = (ulong)table.contentLen * (ulong)imagePlugin.ImageInfo.sectorSize;
                contentPart.Length = table.contentLen;
                contentPart.Sequence = 1;
                contentPart.Offset = (ulong)table.contentOff * (ulong)imagePlugin.ImageInfo.sectorSize;
                contentPart.Start = table.contentOff;

                Partition dashboardPart = new Partition();
                dashboardPart.Description = "Dashboard volume";
                dashboardPart.Size = (ulong)table.dashboardLen * (ulong)imagePlugin.ImageInfo.sectorSize;
                dashboardPart.Length = table.dashboardLen;
                dashboardPart.Sequence = 2;
                dashboardPart.Offset = (ulong)table.dashboardOff * (ulong)imagePlugin.ImageInfo.sectorSize;
                dashboardPart.Start = table.dashboardOff;

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
                    sysCachePart.Description = "System cache";
                    sysCachePart.Size = MemoryUnitDataOff;
                    sysCachePart.Length = (ulong)(MemoryUnitDataOff / imagePlugin.ImageInfo.sectorSize);
                    sysCachePart.Sequence = 1;
                    sysCachePart.Offset = 0;
                    sysCachePart.Start = 0;

                    Partition dataPart = new Partition();
                    dataPart.Description = "Data volume";
                    dataPart.Size = (ulong)imagePlugin.ImageInfo.sectors * (ulong)imagePlugin.ImageInfo.sectorSize - MemoryUnitDataOff;
                    dataPart.Length = imagePlugin.ImageInfo.sectors - sysCachePart.Length;
                    dataPart.Sequence = 2;
                    dataPart.Offset = MemoryUnitDataOff;
                    dataPart.Start = sysCachePart.Length;

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
                    securityPart.Description = "Security sectors";
                    securityPart.Size = Xbox360SecuritySectorLen;
                    securityPart.Length = (ulong)(Xbox360SecuritySectorLen / imagePlugin.ImageInfo.sectorSize);
                    securityPart.Sequence = 1;
                    securityPart.Offset = Xbox360SecuritySectorOff;
                    securityPart.Start = (ulong)(Xbox360SecuritySectorOff / imagePlugin.ImageInfo.sectorSize);

                    Partition sysCachePart = new Partition();
                    sysCachePart.Description = "System cache";
                    sysCachePart.Size = Xbox360SystemCacheLen;
                    sysCachePart.Length = (ulong)(Xbox360SystemCacheLen / imagePlugin.ImageInfo.sectorSize);
                    sysCachePart.Sequence = 2;
                    sysCachePart.Offset = Xbox360SystemCacheOff;
                    sysCachePart.Start = (ulong)(Xbox360SystemCacheOff / imagePlugin.ImageInfo.sectorSize);

                    Partition gameCachePart = new Partition();
                    gameCachePart.Description = "Game cache";
                    gameCachePart.Size = Xbox360GameCacheLen;
                    gameCachePart.Length = (ulong)(Xbox360GameCacheLen / imagePlugin.ImageInfo.sectorSize);
                    gameCachePart.Sequence = 3;
                    gameCachePart.Offset = Xbox360GameCacheOff;
                    gameCachePart.Start = (ulong)(Xbox360GameCacheOff / imagePlugin.ImageInfo.sectorSize);

                    Partition sysExtPart = new Partition();
                    sysExtPart.Description = "System volume";
                    sysExtPart.Size = Xbox368SysExtLen;
                    sysExtPart.Length = (ulong)(Xbox368SysExtLen / imagePlugin.ImageInfo.sectorSize);
                    sysExtPart.Sequence = 4;
                    sysExtPart.Offset = Xbox368SysExtOff;
                    sysExtPart.Start = (ulong)(Xbox368SysExtOff / imagePlugin.ImageInfo.sectorSize);

                    Partition sysExt2Part = new Partition();
                    sysExt2Part.Description = "System volume 2";
                    sysExt2Part.Size = Xbox360SysExt2Len;
                    sysExt2Part.Length = (ulong)(Xbox360SysExt2Len / imagePlugin.ImageInfo.sectorSize);
                    sysExt2Part.Sequence = 5;
                    sysExt2Part.Offset = Xbox360SysExt2Off;
                    sysExt2Part.Start = (ulong)(Xbox360SysExt2Off / imagePlugin.ImageInfo.sectorSize);

                    Partition xbox1Part = new Partition();
                    xbox1Part.Description = "Xbox backwards compatibility";
                    xbox1Part.Size = Xbox360CompatLen;
                    xbox1Part.Length = (ulong)(Xbox360CompatLen / imagePlugin.ImageInfo.sectorSize);
                    xbox1Part.Sequence = 6;
                    xbox1Part.Offset = Xbox360CompatOff;
                    xbox1Part.Start = (ulong)(Xbox360CompatOff / imagePlugin.ImageInfo.sectorSize);

                    Partition dataPart = new Partition();
                    dataPart.Description = "Data volume";
                    dataPart.Sequence = 7;
                    dataPart.Offset = Xbox360DataOff;
                    dataPart.Start = (ulong)(Xbox360DataOff / imagePlugin.ImageInfo.sectorSize);
                    dataPart.Length = imagePlugin.ImageInfo.sectors - dataPart.Start;
                    dataPart.Size = (ulong)dataPart.Length * (ulong)imagePlugin.ImageInfo.sectorSize;

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