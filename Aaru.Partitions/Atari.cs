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
// Copyright © 2011-2023 Natalia Portillo
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
    /// <inheritdoc />
    /// <summary>Implements decoding of Atari GEMDOS partitions</summary>
    public sealed class AtariPartitions : IPartition
    {
        const uint TYPE_GEMDOS      = 0x0047454D;
        const uint TYPE_BIG_GEMDOS  = 0x0042474D;
        const uint TYPE_EXTENDED    = 0x0058474D;
        const uint TYPE_LINUX       = 0x004C4E58;
        const uint TYPE_SWAP        = 0x00535750;
        const uint TYPE_RAW         = 0x00524157;
        const uint TYPE_NETBSD      = 0x004E4244;
        const uint TYPE_NETBSD_SWAP = 0x004E4253;
        const uint TYPE_SYSTEM_V    = 0x00554E58;
        const uint TYPE_MAC         = 0x004D4143;
        const uint TYPE_MINIX       = 0x004D4958;
        const uint TYPE_MINIX2      = 0x004D4E58;

        /// <inheritdoc />
        public string Name => "Atari partitions";
        /// <inheritdoc />
        public Guid Id => new Guid("d1dd0f24-ec39-4c4d-9072-be31919a3b5e");
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
                Boot       = new byte[342],
                IcdEntries = new AtariEntry[8],
                Unused     = new byte[12],
                Entries    = new AtariEntry[4]
            };

            Array.Copy(sector, 0, table.Boot, 0, 342);

            for(int i = 0; i < 8; i++)
            {
                table.IcdEntries[i].Type   = BigEndianBitConverter.ToUInt32(sector, 342 + (i * 12) + 0);
                table.IcdEntries[i].Start  = BigEndianBitConverter.ToUInt32(sector, 342 + (i * 12) + 4);
                table.IcdEntries[i].Length = BigEndianBitConverter.ToUInt32(sector, 342 + (i * 12) + 8);
            }

            Array.Copy(sector, 438, table.Unused, 0, 12);

            table.Size = BigEndianBitConverter.ToUInt32(sector, 450);

            for(int i = 0; i < 4; i++)
            {
                table.Entries[i].Type   = BigEndianBitConverter.ToUInt32(sector, 454 + (i * 12) + 0);
                table.Entries[i].Start  = BigEndianBitConverter.ToUInt32(sector, 454 + (i * 12) + 4);
                table.Entries[i].Length = BigEndianBitConverter.ToUInt32(sector, 454 + (i * 12) + 8);
            }

            table.BadStart  = BigEndianBitConverter.ToUInt32(sector, 502);
            table.BadLength = BigEndianBitConverter.ToUInt32(sector, 506);
            table.Checksum  = BigEndianBitConverter.ToUInt16(sector, 510);

            var sha1Ctx = new Sha1Context();
            sha1Ctx.Update(table.Boot);
            AaruConsole.DebugWriteLine("Atari partition plugin", "Boot code SHA1: {0}", sha1Ctx.End());

            for(int i = 0; i < 8; i++)
            {
                AaruConsole.DebugWriteLine("Atari partition plugin", "table.icdEntries[{0}].flag = 0x{1:X2}", i,
                                           (table.IcdEntries[i].Type & 0xFF000000) >> 24);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.icdEntries[{0}].type = 0x{1:X6}", i,
                                           table.IcdEntries[i].Type & 0x00FFFFFF);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.icdEntries[{0}].start = {1}", i,
                                           table.IcdEntries[i].Start);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.icdEntries[{0}].length = {1}", i,
                                           table.IcdEntries[i].Length);
            }

            AaruConsole.DebugWriteLine("Atari partition plugin", "table.size = {0}", table.Size);

            for(int i = 0; i < 4; i++)
            {
                AaruConsole.DebugWriteLine("Atari partition plugin", "table.entries[{0}].flag = 0x{1:X2}", i,
                                           (table.Entries[i].Type & 0xFF000000) >> 24);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.entries[{0}].type = 0x{1:X6}", i,
                                           table.Entries[i].Type & 0x00FFFFFF);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.entries[{0}].start = {1}", i,
                                           table.Entries[i].Start);

                AaruConsole.DebugWriteLine("Atari partition plugin", "table.entries[{0}].length = {1}", i,
                                           table.Entries[i].Length);
            }

            AaruConsole.DebugWriteLine("Atari partition plugin", "table.badStart = {0}", table.BadStart);
            AaruConsole.DebugWriteLine("Atari partition plugin", "table.badLength = {0}", table.BadLength);
            AaruConsole.DebugWriteLine("Atari partition plugin", "table.checksum = 0x{0:X4}", table.Checksum);

            bool  validTable        = false;
            ulong partitionSequence = 0;

            for(int i = 0; i < 4; i++)
            {
                uint type = table.Entries[i].Type & 0x00FFFFFF;

                switch(type)
                {
                    case TYPE_GEMDOS:
                    case TYPE_BIG_GEMDOS:
                    case TYPE_LINUX:
                    case TYPE_SWAP:
                    case TYPE_RAW:
                    case TYPE_NETBSD:
                    case TYPE_NETBSD_SWAP:
                    case TYPE_SYSTEM_V:
                    case TYPE_MAC:
                    case TYPE_MINIX:
                    case TYPE_MINIX2:
                        validTable = true;

                        if(table.Entries[i].Start <= imagePlugin.Info.Sectors)
                        {
                            if(table.Entries[i].Start + table.Entries[i].Length > imagePlugin.Info.Sectors)
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
                                Size     = table.Entries[i].Length * sectorSize,
                                Length   = table.Entries[i].Length,
                                Sequence = partitionSequence,
                                Name     = "",
                                Offset   = table.Entries[i].Start * sectorSize,
                                Start    = table.Entries[i].Start,
                                Type     = Encoding.ASCII.GetString(partType),
                                Scheme   = Name
                            };

                            switch(type)
                            {
                                case TYPE_GEMDOS:
                                    part.Description = "Atari GEMDOS partition";

                                    break;
                                case TYPE_BIG_GEMDOS:
                                    part.Description = "Atari GEMDOS partition bigger than 32 MiB";

                                    break;
                                case TYPE_LINUX:
                                    part.Description = "Linux partition";

                                    break;
                                case TYPE_SWAP:
                                    part.Description = "Swap partition";

                                    break;
                                case TYPE_RAW:
                                    part.Description = "RAW partition";

                                    break;
                                case TYPE_NETBSD:
                                    part.Description = "NetBSD partition";

                                    break;
                                case TYPE_NETBSD_SWAP:
                                    part.Description = "NetBSD swap partition";

                                    break;
                                case TYPE_SYSTEM_V:
                                    part.Description = "Atari UNIX partition";

                                    break;
                                case TYPE_MAC:
                                    part.Description = "Macintosh partition";

                                    break;
                                case TYPE_MINIX:
                                case TYPE_MINIX2:
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
                    case TYPE_EXTENDED:
                        byte[] extendedSector = imagePlugin.ReadSector(table.Entries[i].Start);
                        var    extendedTable  = new AtariTable();
                        extendedTable.Entries = new AtariEntry[4];

                        for(int j = 0; j < 4; j++)
                        {
                            extendedTable.Entries[j].Type =
                                BigEndianBitConverter.ToUInt32(extendedSector, 454 + (j * 12) + 0);

                            extendedTable.Entries[j].Start =
                                BigEndianBitConverter.ToUInt32(extendedSector, 454 + (j * 12) + 4);

                            extendedTable.Entries[j].Length =
                                BigEndianBitConverter.ToUInt32(extendedSector, 454 + (j * 12) + 8);
                        }

                        for(int j = 0; j < 4; j++)
                        {
                            uint extendedType = extendedTable.Entries[j].Type & 0x00FFFFFF;

                            if(extendedType != TYPE_GEMDOS      &&
                               extendedType != TYPE_BIG_GEMDOS  &&
                               extendedType != TYPE_LINUX       &&
                               extendedType != TYPE_SWAP        &&
                               extendedType != TYPE_RAW         &&
                               extendedType != TYPE_NETBSD      &&
                               extendedType != TYPE_NETBSD_SWAP &&
                               extendedType != TYPE_SYSTEM_V    &&
                               extendedType != TYPE_MAC         &&
                               extendedType != TYPE_MINIX       &&
                               extendedType != TYPE_MINIX2)
                                continue;

                            validTable = true;

                            if(extendedTable.Entries[j].Start > imagePlugin.Info.Sectors)
                                continue;

                            if(extendedTable.Entries[j].Start + extendedTable.Entries[j].Length >
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
                                Size     = extendedTable.Entries[j].Length * sectorSize,
                                Length   = extendedTable.Entries[j].Length,
                                Sequence = partitionSequence,
                                Name     = "",
                                Offset   = extendedTable.Entries[j].Start * sectorSize,
                                Start    = extendedTable.Entries[j].Start,
                                Type     = Encoding.ASCII.GetString(partType),
                                Scheme   = Name
                            };

                            switch(extendedType)
                            {
                                case TYPE_GEMDOS:
                                    part.Description = "Atari GEMDOS partition";

                                    break;
                                case TYPE_BIG_GEMDOS:
                                    part.Description = "Atari GEMDOS partition bigger than 32 MiB";

                                    break;
                                case TYPE_LINUX:
                                    part.Description = "Linux partition";

                                    break;
                                case TYPE_SWAP:
                                    part.Description = "Swap partition";

                                    break;
                                case TYPE_RAW:
                                    part.Description = "RAW partition";

                                    break;
                                case TYPE_NETBSD:
                                    part.Description = "NetBSD partition";

                                    break;
                                case TYPE_NETBSD_SWAP:
                                    part.Description = "NetBSD swap partition";

                                    break;
                                case TYPE_SYSTEM_V:
                                    part.Description = "Atari UNIX partition";

                                    break;
                                case TYPE_MAC:
                                    part.Description = "Macintosh partition";

                                    break;
                                case TYPE_MINIX:
                                case TYPE_MINIX2:
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
                uint type = table.IcdEntries[i].Type & 0x00FFFFFF;

                if(type != TYPE_GEMDOS      &&
                   type != TYPE_BIG_GEMDOS  &&
                   type != TYPE_LINUX       &&
                   type != TYPE_SWAP        &&
                   type != TYPE_RAW         &&
                   type != TYPE_NETBSD      &&
                   type != TYPE_NETBSD_SWAP &&
                   type != TYPE_SYSTEM_V    &&
                   type != TYPE_MAC         &&
                   type != TYPE_MINIX       &&
                   type != TYPE_MINIX2)
                    continue;

                if(table.IcdEntries[i].Start > imagePlugin.Info.Sectors)
                    continue;

                if(table.IcdEntries[i].Start + table.IcdEntries[i].Length > imagePlugin.Info.Sectors)
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
                    Size     = table.IcdEntries[i].Length * sectorSize,
                    Length   = table.IcdEntries[i].Length,
                    Sequence = partitionSequence,
                    Name     = "",
                    Offset   = table.IcdEntries[i].Start * sectorSize,
                    Start    = table.IcdEntries[i].Start,
                    Type     = Encoding.ASCII.GetString(partType),
                    Scheme   = Name
                };

                switch(type)
                {
                    case TYPE_GEMDOS:
                        part.Description = "Atari GEMDOS partition";

                        break;
                    case TYPE_BIG_GEMDOS:
                        part.Description = "Atari GEMDOS partition bigger than 32 MiB";

                        break;
                    case TYPE_LINUX:
                        part.Description = "Linux partition";

                        break;
                    case TYPE_SWAP:
                        part.Description = "Swap partition";

                        break;
                    case TYPE_RAW:
                        part.Description = "RAW partition";

                        break;
                    case TYPE_NETBSD:
                        part.Description = "NetBSD partition";

                        break;
                    case TYPE_NETBSD_SWAP:
                        part.Description = "NetBSD swap partition";

                        break;
                    case TYPE_SYSTEM_V:
                        part.Description = "Atari UNIX partition";

                        break;
                    case TYPE_MAC:
                        part.Description = "Macintosh partition";

                        break;
                    case TYPE_MINIX:
                    case TYPE_MINIX2:
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
            public uint Type;
            /// <summary>Starting sector</summary>
            public uint Start;
            /// <summary>Length in sectors</summary>
            public uint Length;
        }

        struct AtariTable
        {
            /// <summary>Boot code for 342 bytes</summary>
            public byte[] Boot;
            /// <summary>8 extra entries for ICDPro driver</summary>
            public AtariEntry[] IcdEntries;
            /// <summary>Unused, 12 bytes</summary>
            public byte[] Unused;
            /// <summary>Disk size in sectors</summary>
            public uint Size;
            /// <summary>4 partition entries</summary>
            public AtariEntry[] Entries;
            /// <summary>Starting sector of bad block list</summary>
            public uint BadStart;
            /// <summary>Length in sectors of bad block list</summary>
            public uint BadLength;
            /// <summary>Checksum for bootable disks</summary>
            public ushort Checksum;
        }
    }
}