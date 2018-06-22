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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Partitions
{
    public class Xbox : IPartition
    {
        const uint XboxCigam                = 0x46415458;
        const uint XboxMagic                = 0x58544146;
        const long MemoryUnitDataOff        = 0x7FF000;
        const long Xbox360SecuritySectorOff = 0x2000;
        const long Xbox360SystemCacheOff    = 0x80000;
        const long Xbox360GameCacheOff      = 0x8008000;
        const long Xbox368SysExtOff         = 0x10C080000;
        const long Xbox360SysExt2Off        = 0x118EB0000;
        const long Xbox360CompatOff         = 0x120EB0000;
        const long Xbox360DataOff           = 0x130EB0000;

        const long Xbox360SecuritySectorLen = 0x80000;
        const long Xbox360SystemCacheLen    = 0x80000000;
        const long Xbox360GameCacheLen      = 0xA0E30000;
        const long Xbox368SysExtLen         = 0xCE30000;
        const long Xbox360SysExt2Len        = 0x8000000;
        const long Xbox360CompatLen         = 0x10000000;

        const uint XBOX360_DEVKIT_MAGIC = 0x00020000;

        public string Name => "Xbox partitioning";
        public Guid   Id   => new Guid("E3F6FB91-D358-4F22-A550-81E92D50EB78");

        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            // Xbox partitions always start on 0
            if(sectorOffset != 0) return false;

            byte[] sector = imagePlugin.ReadSector(0);
            if(sector.Length < 512) return false;

            Xbox360DevKitPartitionTable table =
                BigEndianMarshal.ByteArrayToStructureBigEndian<Xbox360DevKitPartitionTable>(sector);

            if(table.magic                             == XBOX360_DEVKIT_MAGIC     &&
               table.contentOff   + table.contentLen   <= imagePlugin.Info.Sectors &&
               table.dashboardOff + table.dashboardLen <= imagePlugin.Info.Sectors)
            {
                Partition contentPart = new Partition
                {
                    Description = "Content volume",
                    Size        = (ulong)table.contentLen * imagePlugin.Info.SectorSize,
                    Length      = table.contentLen,
                    Sequence    = 1,
                    Offset      = (ulong)table.contentOff * imagePlugin.Info.SectorSize,
                    Start       = table.contentOff,
                    Scheme      = Name
                };

                Partition dashboardPart = new Partition
                {
                    Description = "Dashboard volume",
                    Size        = (ulong)table.dashboardLen * imagePlugin.Info.SectorSize,
                    Length      = table.dashboardLen,
                    Sequence    = 2,
                    Offset      = (ulong)table.dashboardOff * imagePlugin.Info.SectorSize,
                    Start       = table.dashboardOff,
                    Scheme      = Name
                };

                partitions.Add(contentPart);
                partitions.Add(dashboardPart);

                return true;
            }

            uint temp;

            if(imagePlugin.Info.Sectors > (ulong)(MemoryUnitDataOff / imagePlugin.Info.SectorSize))
            {
                sector = imagePlugin.ReadSector((ulong)(MemoryUnitDataOff / imagePlugin.Info.SectorSize));
                temp   = BitConverter.ToUInt32(sector, 0);

                if(temp == XboxCigam)
                {
                    Partition sysCachePart = new Partition
                    {
                        Description = "System cache",
                        Size        = MemoryUnitDataOff,
                        Length      = (ulong)(MemoryUnitDataOff / imagePlugin.Info.SectorSize),
                        Sequence    = 1,
                        Offset      = 0,
                        Start       = 0,
                        Scheme      = Name
                    };

                    Partition dataPart = new Partition
                    {
                        Description = "Data volume",
                        Size        = imagePlugin.Info.Sectors * imagePlugin.Info.SectorSize - MemoryUnitDataOff,
                        Length      = imagePlugin.Info.Sectors                               - sysCachePart.Length,
                        Sequence    = 2,
                        Offset      = MemoryUnitDataOff,
                        Start       = sysCachePart.Length,
                        Scheme      = Name
                    };

                    partitions.Add(sysCachePart);
                    partitions.Add(dataPart);

                    return true;
                }
            }

            if(imagePlugin.Info.Sectors <= (ulong)(Xbox360DataOff / imagePlugin.Info.SectorSize)) return false;

            {
                sector = imagePlugin.ReadSector((ulong)(Xbox360DataOff / imagePlugin.Info.SectorSize));
                temp   = BitConverter.ToUInt32(sector, 0);

                if(temp != XboxCigam) return false;

                Partition securityPart = new Partition
                {
                    Description = "Security sectors",
                    Size        = Xbox360SecuritySectorLen,
                    Length      = (ulong)(Xbox360SecuritySectorLen / imagePlugin.Info.SectorSize),
                    Sequence    = 1,
                    Offset      = Xbox360SecuritySectorOff,
                    Start       = (ulong)(Xbox360SecuritySectorOff / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                Partition sysCachePart = new Partition
                {
                    Description = "System cache",
                    Size        = Xbox360SystemCacheLen,
                    Length      = (ulong)(Xbox360SystemCacheLen / imagePlugin.Info.SectorSize),
                    Sequence    = 2,
                    Offset      = Xbox360SystemCacheOff,
                    Start       = (ulong)(Xbox360SystemCacheOff / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                Partition gameCachePart = new Partition
                {
                    Description = "Game cache",
                    Size        = Xbox360GameCacheLen,
                    Length      = (ulong)(Xbox360GameCacheLen / imagePlugin.Info.SectorSize),
                    Sequence    = 3,
                    Offset      = Xbox360GameCacheOff,
                    Start       = (ulong)(Xbox360GameCacheOff / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                Partition sysExtPart = new Partition
                {
                    Description = "System volume",
                    Size        = Xbox368SysExtLen,
                    Length      = (ulong)(Xbox368SysExtLen / imagePlugin.Info.SectorSize),
                    Sequence    = 4,
                    Offset      = Xbox368SysExtOff,
                    Start       = (ulong)(Xbox368SysExtOff / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                Partition sysExt2Part = new Partition
                {
                    Description = "System volume 2",
                    Size        = Xbox360SysExt2Len,
                    Length      = (ulong)(Xbox360SysExt2Len / imagePlugin.Info.SectorSize),
                    Sequence    = 5,
                    Offset      = Xbox360SysExt2Off,
                    Start       = (ulong)(Xbox360SysExt2Off / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                Partition xbox1Part = new Partition
                {
                    Description = "Xbox backwards compatibility",
                    Size        = Xbox360CompatLen,
                    Length      = (ulong)(Xbox360CompatLen / imagePlugin.Info.SectorSize),
                    Sequence    = 6,
                    Offset      = Xbox360CompatOff,
                    Start       = (ulong)(Xbox360CompatOff / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                Partition dataPart = new Partition
                {
                    Description = "Data volume",
                    Sequence    = 7,
                    Offset      = Xbox360DataOff,
                    Start       = (ulong)(Xbox360DataOff / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };
                dataPart.Length = imagePlugin.Info.Sectors - dataPart.Start;
                dataPart.Size   = dataPart.Length * imagePlugin.Info.SectorSize;

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
    }
}