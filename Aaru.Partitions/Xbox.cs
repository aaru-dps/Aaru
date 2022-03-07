// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Partitions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Marshal = Aaru.Helpers.Marshal;

/// <inheritdoc />
/// <summary>Implements decoding of Xbox partitions</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class Xbox : IPartition
{
    const uint XBOX_CIGAM                  = 0x46415458;
    const uint XBOX_MAGIC                  = 0x58544146;
    const long MEMORY_UNIT_DATA_OFF        = 0x7FF000;
    const long XBOX360_SECURITY_SECTOR_OFF = 0x2000;
    const long XBOX360_SYSTEM_CACHE_OFF    = 0x80000;
    const long XBOX360_GAME_CACHE_OFF      = 0x8008000;
    const long XBOX368_SYS_EXT_OFF         = 0x10C080000;
    const long XBOX360_SYS_EXT2_OFF        = 0x118EB0000;
    const long XBOX360_COMPAT_OFF          = 0x120EB0000;
    const long XBOX_360DATA_OFF            = 0x130EB0000;
    const long XBOX360_SECURITY_SECTOR_LEN = 0x80000;
    const long XBOX360_SYSTEM_CACHE_LEN    = 0x80000000;
    const long XBOX360_GAME_CACHE_LEN      = 0xA0E30000;
    const long XBOX368_SYS_EXT_LEN         = 0xCE30000;
    const long XBOX360_SYS_EXT2_LEN        = 0x8000000;
    const long XBOX360_COMPAT_LEN          = 0x10000000;
    const uint XBOX360_DEVKIT_MAGIC        = 0x00020000;

    /// <inheritdoc />
    public string Name => "Xbox partitioning";
    /// <inheritdoc />
    public Guid Id => new("E3F6FB91-D358-4F22-A550-81E92D50EB78");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = new List<Partition>();

        // Xbox partitions always start on 0
        if(sectorOffset != 0)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0, out byte[] sector);

        if(errno         != ErrorNumber.NoError ||
           sector.Length < 512)
            return false;

        Xbox360DevKitPartitionTable table = Marshal.ByteArrayToStructureBigEndian<Xbox360DevKitPartitionTable>(sector);

        if(table.magic                             == XBOX360_DEVKIT_MAGIC     &&
           table.contentOff   + table.contentLen   <= imagePlugin.Info.Sectors &&
           table.dashboardOff + table.dashboardLen <= imagePlugin.Info.Sectors)
        {
            var contentPart = new Partition
            {
                Description = "Content volume",
                Size        = (ulong)table.contentLen * imagePlugin.Info.SectorSize,
                Length      = table.contentLen,
                Sequence    = 1,
                Offset      = (ulong)table.contentOff * imagePlugin.Info.SectorSize,
                Start       = table.contentOff,
                Scheme      = Name
            };

            var dashboardPart = new Partition
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

        if(imagePlugin.Info.Sectors > (ulong)(MEMORY_UNIT_DATA_OFF / imagePlugin.Info.SectorSize))
        {
            errno = imagePlugin.ReadSector((ulong)(MEMORY_UNIT_DATA_OFF / imagePlugin.Info.SectorSize), out sector);

            if(errno == ErrorNumber.NoError)
            {
                temp = BitConverter.ToUInt32(sector, 0);

                if(temp == XBOX_CIGAM)
                {
                    var sysCachePart = new Partition
                    {
                        Description = "System cache",
                        Size        = MEMORY_UNIT_DATA_OFF,
                        Length      = (ulong)(MEMORY_UNIT_DATA_OFF / imagePlugin.Info.SectorSize),
                        Sequence    = 1,
                        Offset      = 0,
                        Start       = 0,
                        Scheme      = Name
                    };

                    var dataPart = new Partition
                    {
                        Description = "Data volume",
                        Size        = imagePlugin.Info.Sectors * imagePlugin.Info.SectorSize - MEMORY_UNIT_DATA_OFF,
                        Length      = imagePlugin.Info.Sectors                               - sysCachePart.Length,
                        Sequence    = 2,
                        Offset      = MEMORY_UNIT_DATA_OFF,
                        Start       = sysCachePart.Length,
                        Scheme      = Name
                    };

                    partitions.Add(sysCachePart);
                    partitions.Add(dataPart);

                    return true;
                }
            }
        }

        if(imagePlugin.Info.Sectors <= (ulong)(XBOX_360DATA_OFF / imagePlugin.Info.SectorSize))
            return false;

        {
            errno = imagePlugin.ReadSector((ulong)(XBOX_360DATA_OFF / imagePlugin.Info.SectorSize), out sector);

            if(errno == ErrorNumber.NoError)
            {
                temp = BitConverter.ToUInt32(sector, 0);

                if(temp != XBOX_CIGAM)
                    return false;

                var securityPart = new Partition
                {
                    Description = "Security sectors",
                    Size        = XBOX360_SECURITY_SECTOR_LEN,
                    Length      = (ulong)(XBOX360_SECURITY_SECTOR_LEN / imagePlugin.Info.SectorSize),
                    Sequence    = 1,
                    Offset      = XBOX360_SECURITY_SECTOR_OFF,
                    Start       = (ulong)(XBOX360_SECURITY_SECTOR_OFF / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                var sysCachePart = new Partition
                {
                    Description = "System cache",
                    Size        = XBOX360_SYSTEM_CACHE_LEN,
                    Length      = (ulong)(XBOX360_SYSTEM_CACHE_LEN / imagePlugin.Info.SectorSize),
                    Sequence    = 2,
                    Offset      = XBOX360_SYSTEM_CACHE_OFF,
                    Start       = (ulong)(XBOX360_SYSTEM_CACHE_OFF / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                var gameCachePart = new Partition
                {
                    Description = "Game cache",
                    Size        = XBOX360_GAME_CACHE_LEN,
                    Length      = (ulong)(XBOX360_GAME_CACHE_LEN / imagePlugin.Info.SectorSize),
                    Sequence    = 3,
                    Offset      = XBOX360_GAME_CACHE_OFF,
                    Start       = (ulong)(XBOX360_GAME_CACHE_OFF / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                var sysExtPart = new Partition
                {
                    Description = "System volume",
                    Size        = XBOX368_SYS_EXT_LEN,
                    Length      = (ulong)(XBOX368_SYS_EXT_LEN / imagePlugin.Info.SectorSize),
                    Sequence    = 4,
                    Offset      = XBOX368_SYS_EXT_OFF,
                    Start       = (ulong)(XBOX368_SYS_EXT_OFF / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                var sysExt2Part = new Partition
                {
                    Description = "System volume 2",
                    Size        = XBOX360_SYS_EXT2_LEN,
                    Length      = (ulong)(XBOX360_SYS_EXT2_LEN / imagePlugin.Info.SectorSize),
                    Sequence    = 5,
                    Offset      = XBOX360_SYS_EXT2_OFF,
                    Start       = (ulong)(XBOX360_SYS_EXT2_OFF / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                var xbox1Part = new Partition
                {
                    Description = "Xbox backwards compatibility",
                    Size        = XBOX360_COMPAT_LEN,
                    Length      = (ulong)(XBOX360_COMPAT_LEN / imagePlugin.Info.SectorSize),
                    Sequence    = 6,
                    Offset      = XBOX360_COMPAT_OFF,
                    Start       = (ulong)(XBOX360_COMPAT_OFF / imagePlugin.Info.SectorSize),
                    Scheme      = Name
                };

                var dataPart = new Partition
                {
                    Description = "Data volume",
                    Sequence    = 7,
                    Offset      = XBOX_360DATA_OFF,
                    Start       = (ulong)(XBOX_360DATA_OFF / imagePlugin.Info.SectorSize),
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

        return false;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Xbox360DevKitPartitionTable
    {
        public readonly uint magic;
        public readonly uint unknown;
        public readonly uint contentOff;
        public readonly uint contentLen;
        public readonly uint dashboardOff;
        public readonly uint dashboardLen;
    }
}