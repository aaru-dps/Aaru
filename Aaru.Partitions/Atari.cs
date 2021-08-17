// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Atari.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Atari ST GEMDOS partitions.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Partitions
{
    /// <summary>
    /// Implements decoding of Atari GEMDOS partitions
    /// </summary>
    public sealed class AtariPartitions : IPartition
    {
        const uint TypeGEMDOS     = 0x0047454D;
        const uint TypeBigGEMDOS  = 0x0042474D;
        const uint TypeExtended   = 0x0058474D;
        const uint TypeLinux      = 0x004C4E58;
        const uint TypeSwap       = 0x00535750;
        const uint TypeRAW        = 0x00524157;
        const uint TypeNetBSD     = 0x004E4244;
        const uint TypeNetBSDSwap = 0x004E4253;
        const uint TypeSysV       = 0x00554E58;
        const uint TypeMac        = 0x004D4143;
        const uint TypeMinix      = 0x004D4958;
        const uint TypeMinix2     = 0x004D4E58;

        /// <inheritdoc />
        public string Name   => "Atari partitions";
        /// <inheritdoc />
        public Guid   Id     => new Guid("d1dd0f24-ec39-4c4d-9072-be31919a3b5e");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            byte[] sector = imagePlugin.ReadSector(sectorOffset);

            if(sector.Length < 512)
                return false;

            var table = new AtariTable
            {
                boot       = new byte[342],
                icdEntries = new AtariEntry[8],
                unused     = new byte[12],
                entries    = new AtariEntry[4]
            };

            Array.Copy(sector, 0, table.boot, 0, 342);

            for(int i = 0; i < 8; i++)
            {
                table.icdEntries[i].type   = BigEndianBitConverter.ToUInt32(sector, 342 + (i * 12) + 0);
                table.icdEntries[i].start  = BigEndianBitConverter.ToUInt32(sector, 342 + (i * 12) + 4);
                table.icdEntries[i].length = BigEndianBitConverter.ToUInt32(sector, 342 + (i * 12) + 8);
            }

            Array.Copy(sector, 438, table.unused, 0, 12);

            table.size = BigEndianBitConverter.ToUInt32(sector, 450);

            for(int i = 0; i < 4; i++)
            {
                table.entries[i].type   = BigEndianBitConverter.ToUInt32(sector, 454 + (i * 12) + 0);
                table.entries[i].start  = BigEndianBitConverter.ToUInt32(sector, 454 + (i * 12) + 4);
                table.entries[i].length = BigEndianBitConverter.ToUInt32(sector, 454 + (i * 12) + 8);
            }

            table.badStart  = BigEndianBitConverter.ToUInt32(sector, 502);
            table.badLength = BigEndianBitConverter.ToUInt32(sector, 506);
            table.checksum  = BigEndianBitConverter.ToUInt16(sector, 510);

            var sha1Ctx = new Sha1Context();
            sha1Ctx.Update(table.boot);
            AaruConsole.DebugWriteLine("Atari partition plugin", "Boot code SHA1: {0}", sha1Ctx.End());

            for(int i = 0; i < 8; i++)
            {
                AaruConsole.DebugWriteLine("Atari partition plugin", "table.icdEntries[{0}].flag = 0x{1:X2}", i,
                                           (table.icdEntries[i].type & 0xFF000000) >> 24);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.icdEntries[{0}].type = 0x{1:X6}", i,
                                           table.icdEntries[i].type & 0x00FFFFFF);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.icdEntries[{0}].start = {1}", i,
                                           table.icdEntries[i].start);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.icdEntries[{0}].length = {1}", i,
                                           table.icdEntries[i].length);
            }

            AaruConsole.DebugWriteLine("Atari partition plugin", "table.size = {0}", table.size);

            for(int i = 0; i < 4; i++)
            {
                AaruConsole.DebugWriteLine("Atari partition plugin", "table.entries[{0}].flag = 0x{1:X2}", i,
                                           (table.entries[i].type & 0xFF000000) >> 24);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.entries[{0}].type = 0x{1:X6}", i,
                                           table.entries[i].type & 0x00FFFFFF);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.entries[{0}].start = {1}", i,
                                           table.entries[i].start);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.entries[{0}].length = {1}", i,
                                           table.entries[i].length);
            }

            AaruConsole.DebugWriteLine("Atari partition plugin", "table.badStart = {0}", table.badStart);
            AaruConsole.DebugWriteLine("Atari partition plugin", "table.badLength = {0}", table.badLength);
            AaruConsole.DebugWriteLine("Atari partition plugin", "table.checksum = 0x{0:X4}", table.checksum);

            bool  validTable        = false;
            ulong partitionSequence = 0;

            for(int i = 0; i < 4; i++)
            {
                uint type = table.entries[i].type & 0x00FFFFFF;

                switch(type)
                {
                    case TypeGEMDOS:
                    case TypeBigGEMDOS:
                    case TypeLinux:
                    case TypeSwap:
                    case TypeRAW:
                    case TypeNetBSD:
                    case TypeNetBSDSwap:
                    case TypeSysV:
                    case TypeMac:
                    case TypeMinix:
                    case TypeMinix2:
                        validTable = true;

                        if(table.entries[i].start <= imagePlugin.Info.Sectors)
                        {
                            if(table.entries[i].start + table.entries[i].length > imagePlugin.Info.Sectors)
                                AaruConsole.DebugWriteLine("Atari partition plugin",
                                                           "WARNING: End of partition goes beyond device size");

                            ulong sectorSize = imagePlugin.Info.SectorSize;

                            if(sectorSize == 2448 ||
                               sectorSize == 2352)
                                sectorSize = 2048;

                            byte[] partType = new byte[3];
                            partType[0] = (byte)((type & 0xFF0000) >> 16);
                            partType[1] = (byte)((type & 0x00FF00) >> 8);
                            partType[2] = (byte)(type & 0x0000FF);

                            var part = new Partition
                            {
                                Size     = table.entries[i].length * sectorSize,
                                Length   = table.entries[i].length,
                                Sequence = partitionSequence,
                                Name     = "",
                                Offset   = table.entries[i].start * sectorSize,
                                Start    = table.entries[i].start,
                                Type     = Encoding.ASCII.GetString(partType),
                                Scheme   = Name
                            };

                            switch(type)
                            {
                                case TypeGEMDOS:
                                    part.Description = "Atari GEMDOS partition";

                                    break;
                                case TypeBigGEMDOS:
                                    part.Description = "Atari GEMDOS partition bigger than 32 MiB";

                                    break;
                                case TypeLinux:
                                    part.Description = "Linux partition";

                                    break;
                                case TypeSwap:
                                    part.Description = "Swap partition";

                                    break;
                                case TypeRAW:
                                    part.Description = "RAW partition";

                                    break;
                                case TypeNetBSD:
                                    part.Description = "NetBSD partition";

                                    break;
                                case TypeNetBSDSwap:
                                    part.Description = "NetBSD swap partition";

                                    break;
                                case TypeSysV:
                                    part.Description = "Atari UNIX partition";

                                    break;
                                case TypeMac:
                                    part.Description = "Macintosh partition";

                                    break;
                                case TypeMinix:
                                case TypeMinix2:
                                    part.Description = "MINIX partition";

                                    break;
                                default:
                                    part.Description = "Unknown partition type";

                                    break;
                            }

                            partitions.Add(part);
                            partitionSequence++;
                        }

                        break;
                    case TypeExtended:
                        byte[] extendedSector = imagePlugin.ReadSector(table.entries[i].start);
                        var    extendedTable  = new AtariTable();
                        extendedTable.entries = new AtariEntry[4];

                        for(int j = 0; j < 4; j++)
                        {
                            extendedTable.entries[j].type =
                                BigEndianBitConverter.ToUInt32(extendedSector, 454 + (j * 12) + 0);

                            extendedTable.entries[j].start =
                                BigEndianBitConverter.ToUInt32(extendedSector, 454 + (j * 12) + 4);

                            extendedTable.entries[j].length =
                                BigEndianBitConverter.ToUInt32(extendedSector, 454 + (j * 12) + 8);
                        }

                        for(int j = 0; j < 4; j++)
                        {
                            uint extendedType = extendedTable.entries[j].type & 0x00FFFFFF;

                            if(extendedType != TypeGEMDOS     &&
                               extendedType != TypeBigGEMDOS  &&
                               extendedType != TypeLinux      &&
                               extendedType != TypeSwap       &&
                               extendedType != TypeRAW        &&
                               extendedType != TypeNetBSD     &&
                               extendedType != TypeNetBSDSwap &&
                               extendedType != TypeSysV       &&
                               extendedType != TypeMac        &&
                               extendedType != TypeMinix      &&
                               extendedType != TypeMinix2)
                                continue;

                            validTable = true;

                            if(extendedTable.entries[j].start > imagePlugin.Info.Sectors)
                                continue;

                            if(extendedTable.entries[j].start + extendedTable.entries[j].length >
                               imagePlugin.Info.Sectors)
                                AaruConsole.DebugWriteLine("Atari partition plugin",
                                                           "WARNING: End of partition goes beyond device size");

                            ulong sectorSize = imagePlugin.Info.SectorSize;

                            if(sectorSize == 2448 ||
                               sectorSize == 2352)
                                sectorSize = 2048;

                            byte[] partType = new byte[3];
                            partType[0] = (byte)((extendedType & 0xFF0000) >> 16);
                            partType[1] = (byte)((extendedType & 0x00FF00) >> 8);
                            partType[2] = (byte)(extendedType & 0x0000FF);

                            var part = new Partition
                            {
                                Size     = extendedTable.entries[j].length * sectorSize,
                                Length   = extendedTable.entries[j].length,
                                Sequence = partitionSequence,
                                Name     = "",
                                Offset   = extendedTable.entries[j].start * sectorSize,
                                Start    = extendedTable.entries[j].start,
                                Type     = Encoding.ASCII.GetString(partType),
                                Scheme   = Name
                            };

                            switch(extendedType)
                            {
                                case TypeGEMDOS:
                                    part.Description = "Atari GEMDOS partition";

                                    break;
                                case TypeBigGEMDOS:
                                    part.Description = "Atari GEMDOS partition bigger than 32 MiB";

                                    break;
                                case TypeLinux:
                                    part.Description = "Linux partition";

                                    break;
                                case TypeSwap:
                                    part.Description = "Swap partition";

                                    break;
                                case TypeRAW:
                                    part.Description = "RAW partition";

                                    break;
                                case TypeNetBSD:
                                    part.Description = "NetBSD partition";

                                    break;
                                case TypeNetBSDSwap:
                                    part.Description = "NetBSD swap partition";

                                    break;
                                case TypeSysV:
                                    part.Description = "Atari UNIX partition";

                                    break;
                                case TypeMac:
                                    part.Description = "Macintosh partition";

                                    break;
                                case TypeMinix:
                                case TypeMinix2:
                                    part.Description = "MINIX partition";

                                    break;
                                default:
                                    part.Description = "Unknown partition type";

                                    break;
                            }

                            partitions.Add(part);
                            partitionSequence++;
                        }

                        break;
                }
            }

            if(!validTable)
                return partitions.Count > 0;

            for(int i = 0; i < 8; i++)
            {
                uint type = table.icdEntries[i].type & 0x00FFFFFF;

                if(type != TypeGEMDOS     &&
                   type != TypeBigGEMDOS  &&
                   type != TypeLinux      &&
                   type != TypeSwap       &&
                   type != TypeRAW        &&
                   type != TypeNetBSD     &&
                   type != TypeNetBSDSwap &&
                   type != TypeSysV       &&
                   type != TypeMac        &&
                   type != TypeMinix      &&
                   type != TypeMinix2)
                    continue;

                if(table.icdEntries[i].start > imagePlugin.Info.Sectors)
                    continue;

                if(table.icdEntries[i].start + table.icdEntries[i].length > imagePlugin.Info.Sectors)
                    AaruConsole.DebugWriteLine("Atari partition plugin",
                                               "WARNING: End of partition goes beyond device size");

                ulong sectorSize = imagePlugin.Info.SectorSize;

                if(sectorSize == 2448 ||
                   sectorSize == 2352)
                    sectorSize = 2048;

                byte[] partType = new byte[3];
                partType[0] = (byte)((type & 0xFF0000) >> 16);
                partType[1] = (byte)((type & 0x00FF00) >> 8);
                partType[2] = (byte)(type & 0x0000FF);

                var part = new Partition
                {
                    Size     = table.icdEntries[i].length * sectorSize,
                    Length   = table.icdEntries[i].length,
                    Sequence = partitionSequence,
                    Name     = "",
                    Offset   = table.icdEntries[i].start * sectorSize,
                    Start    = table.icdEntries[i].start,
                    Type     = Encoding.ASCII.GetString(partType),
                    Scheme   = Name
                };

                switch(type)
                {
                    case TypeGEMDOS:
                        part.Description = "Atari GEMDOS partition";

                        break;
                    case TypeBigGEMDOS:
                        part.Description = "Atari GEMDOS partition bigger than 32 MiB";

                        break;
                    case TypeLinux:
                        part.Description = "Linux partition";

                        break;
                    case TypeSwap:
                        part.Description = "Swap partition";

                        break;
                    case TypeRAW:
                        part.Description = "RAW partition";

                        break;
                    case TypeNetBSD:
                        part.Description = "NetBSD partition";

                        break;
                    case TypeNetBSDSwap:
                        part.Description = "NetBSD swap partition";

                        break;
                    case TypeSysV:
                        part.Description = "Atari UNIX partition";

                        break;
                    case TypeMac:
                        part.Description = "Macintosh partition";

                        break;
                    case TypeMinix:
                    case TypeMinix2:
                        part.Description = "MINIX partition";

                        break;
                    default:
                        part.Description = "Unknown partition type";

                        break;
                }

                partitions.Add(part);
                partitionSequence++;
            }

            return partitions.Count > 0;
        }

        /// <summary>Atari partition entry</summary>
        struct AtariEntry
        {
            /// <summary>First byte flag, three bytes type in ASCII. Flag bit 0 = active Flag bit 7 = bootable</summary>
            public uint type;
            /// <summary>Starting sector</summary>
            public uint start;
            /// <summary>Length in sectors</summary>
            public uint length;
        }

        struct AtariTable
        {
            /// <summary>Boot code for 342 bytes</summary>
            public byte[] boot;
            /// <summary>8 extra entries for ICDPro driver</summary>
            public AtariEntry[] icdEntries;
            /// <summary>Unused, 12 bytes</summary>
            public byte[] unused;
            /// <summary>Disk size in sectors</summary>
            public uint size;
            /// <summary>4 partition entries</summary>
            public AtariEntry[] entries;
            /// <summary>Starting sector of bad block list</summary>
            public uint badStart;
            /// <summary>Length in sectors of bad block list</summary>
            public uint badLength;
            /// <summary>Checksum for bootable disks</summary>
            public ushort checksum;
        }
    }
}