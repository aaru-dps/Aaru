// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Sun.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Collections.Generic;
using DiscImageChef.Console;

namespace DiscImageChef.PartPlugins
{
    class SunDisklabel : PartPlugin
    {
        const UInt16 SUN_MAGIC = 0xDABE;
        const UInt32 VTOC_MAGIC = 0x600DDEEE;

        public enum SunTypes : ushort
        {
            SunEmpty = 0x0000,
            SunBoot = 0x0001,
            SunRoot = 0x0002,
            SunSwap = 0x0003,
            SunUsr = 0x0004,
            SunWholeDisk = 0x0005,
            SunStand = 0x0006,
            SunVar = 0x0007,
            SunHome = 0x0008,
            LinuxSwap = 0x0082,
            Linux = 0x0083,
            LVM = 0x008E,
            LinuxRaid = 0x00FD
        }

        [Flags]
        public enum SunFlags : ushort
        {
            NoMount = 0x0001,
            ReadOnly = 0x0010,
        }

        public SunDisklabel()
        {
            Name = "Sun Disklabel";
            PluginUUID = new Guid("50F35CC4-8375-4445-8DCB-1BA550C931A3");
        }

        public override bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<CommonTypes.Partition> partitions)
        {
            partitions = new List<CommonTypes.Partition>();

            byte[] sunSector = imagePlugin.ReadSector(0);
            byte[] tmpString;
            SunDiskLabel sdl = new SunDiskLabel();
            sdl.vtoc = new SunVTOC();
            sdl.spare = new byte[148];
            sdl.vtoc.infos = new SunInfo[8];
            sdl.vtoc.bootinfo = new uint[3];
            sdl.vtoc.reserved = new byte[40];
            sdl.vtoc.timestamp = new uint[8];
            sdl.partitions = new SunPartition[8];

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            tmpString = new byte[128];
            Array.Copy(sunSector, 0, tmpString, 0, 128);
            sdl.info = StringHandlers.CToString(tmpString);
            sdl.vtoc.version = BigEndianBitConverter.ToUInt32(sunSector, 0x80 + 0x00);

            tmpString = new byte[8];
            Array.Copy(sunSector, 0x80 + 0x04, tmpString, 0, 8);
            sdl.vtoc.volname = StringHandlers.CToString(tmpString);
            sdl.vtoc.nparts = BigEndianBitConverter.ToUInt16(sunSector, 0x80 + 0x0C);
            for (int i = 0; i < 8; i++)
            {
                sdl.vtoc.infos[i] = new SunInfo();
                sdl.vtoc.infos[i].id = BigEndianBitConverter.ToUInt16(sunSector, 0x80 + 0x0E + i * 4 + 0x00);
                sdl.vtoc.infos[i].flags = BigEndianBitConverter.ToUInt16(sunSector, 0x80 + 0x0E + i * 4 + 0x02);
            }
            sdl.vtoc.padding = BigEndianBitConverter.ToUInt16(sunSector, 0x80 + 0x2E);
            for (int i = 0; i < 3; i++)
                sdl.vtoc.bootinfo[i] = BigEndianBitConverter.ToUInt32(sunSector, 0x80 + 0x30 + i * 4);
            sdl.vtoc.sanity = BigEndianBitConverter.ToUInt32(sunSector, 0x80 + 0x3C);
            Array.Copy(sunSector, 0x80 + 0x40, sdl.vtoc.reserved, 0, 40);
            for (int i = 0; i < 8; i++)
                sdl.vtoc.timestamp[i] = BigEndianBitConverter.ToUInt32(sunSector, 0x80 + 0x68 + i * 4);

            sdl.write_reinstruct = BigEndianBitConverter.ToUInt32(sunSector, 0x108);
            sdl.read_reinstruct = BigEndianBitConverter.ToUInt32(sunSector, 0x10C);
            Array.Copy(sunSector, 0x110, sdl.spare, 0, 148);
            sdl.rspeed = BigEndianBitConverter.ToUInt16(sunSector, 0x1A4);
            sdl.pcylcount = BigEndianBitConverter.ToUInt16(sunSector, 0x1A6);
            sdl.sparecyl = BigEndianBitConverter.ToUInt16(sunSector, 0x1A8);
            sdl.gap1 = BigEndianBitConverter.ToUInt16(sunSector, 0x1AA);
            sdl.gap2 = BigEndianBitConverter.ToUInt16(sunSector, 0x1AC);
            sdl.ilfact = BigEndianBitConverter.ToUInt16(sunSector, 0x1AE);
            sdl.ncyl = BigEndianBitConverter.ToUInt16(sunSector, 0x1B0);
            sdl.nacyl = BigEndianBitConverter.ToUInt16(sunSector, 0x1B2);
            sdl.ntrks = BigEndianBitConverter.ToUInt16(sunSector, 0x1B4);
            sdl.nsect = BigEndianBitConverter.ToUInt16(sunSector, 0x1B6);
            sdl.bhead = BigEndianBitConverter.ToUInt16(sunSector, 0x1B8);
            sdl.ppart = BigEndianBitConverter.ToUInt16(sunSector, 0x1BA);

            for (int i = 0; i < 8; i++)
            {
                sdl.partitions[i] = new SunPartition();
                sdl.partitions[i].start_cylinder = BigEndianBitConverter.ToUInt32(sunSector, 0x1BC + i * 8 + 0x00);
                sdl.partitions[i].num_sectors = BigEndianBitConverter.ToUInt32(sunSector, 0x1BC + i * 8 + 0x04);
            }

            sdl.magic = BigEndianBitConverter.ToUInt16(sunSector, 0x1FC);
            sdl.csum = BigEndianBitConverter.ToUInt16(sunSector, 0x1FE);

            ushort csum = 0;
            for (int i = 0; i < 510; i += 2)
                csum ^= BigEndianBitConverter.ToUInt16(sunSector, i);

            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.info = {0}", sdl.info);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.vtoc.version = {0}", sdl.vtoc.version);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.vtoc.volname = {0}", sdl.vtoc.volname);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.vtoc.nparts = {0}", sdl.vtoc.nparts);
            for (int i = 0; i < 8; i++)
            {
                DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.vtoc.infos[{1}].id = 0x{0:X4}", sdl.vtoc.infos[i].id, i);
                DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.vtoc.infos[{1}].flags = 0x{0:X4}", sdl.vtoc.infos[i].flags, i);
            }
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.vtoc.padding = 0x{0:X4}", sdl.vtoc.padding);
            for (int i = 0; i < 3; i++)
                DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.vtoc.bootinfo[{1}].id = 0x{0:X8}", sdl.vtoc.bootinfo[i], i);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.vtoc.sanity = 0x{0:X8}", sdl.vtoc.sanity);
            for (int i = 0; i < 8; i++)
                DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.vtoc.timestamp[{1}] = 0x{0:X8}", sdl.vtoc.timestamp[i], i);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.rspeed = {0}", sdl.rspeed);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.pcylcount = {0}", sdl.pcylcount);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.sparecyl = {0}", sdl.sparecyl);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.gap1 = {0}", sdl.gap1);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.gap2 = {0}", sdl.gap2);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.ilfact = {0}", sdl.ilfact);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.ncyl = {0}", sdl.ncyl);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.nacyl = {0}", sdl.nacyl);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.ntrks = {0}", sdl.ntrks);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.nsect = {0}", sdl.nsect);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.bhead = {0}", sdl.bhead);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.ppart = {0}", sdl.ppart);
            for (int i = 0; i < 8; i++)
            {
                DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.partitions[{1}].start_cylinder = {0}", sdl.partitions[i].start_cylinder, i);
                DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.partitions[{1}].num_sectors = {0}", sdl.partitions[i].num_sectors, i);
            }
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.magic = 0x{0:X4}", sdl.magic);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "sdl.csum = 0x{0:X4}", sdl.csum);
            DicConsole.DebugWriteLine("Sun Disklabel plugin", "csum = 0x{0:X4}", csum);

            ulong sectorsPerCylinder = (ulong)(sdl.nsect * sdl.ntrks);

            if (sectorsPerCylinder == 0 || sectorsPerCylinder * sdl.pcylcount > imagePlugin.GetSectors() || sdl.magic != SUN_MAGIC)
                return false;

            for (int i = 0; i < 8; i++)
            {
                if ((SunTypes)sdl.vtoc.infos[i].id != SunTypes.SunWholeDisk && sdl.partitions[i].num_sectors > 0)
                {
                    CommonTypes.Partition part = new CommonTypes.Partition();
                    part.PartitionDescription = SunFlagsToString((SunFlags)sdl.vtoc.infos[i].flags);
                    part.PartitionLength = (ulong)sdl.partitions[i].num_sectors * (ulong)imagePlugin.GetSectorSize();
                    part.PartitionName = "";
                    part.PartitionSectors = sdl.partitions[i].num_sectors;
                    part.PartitionSequence = (ulong)i;
                    part.PartitionStart = (ulong)sdl.partitions[i].start_cylinder * (ulong)sectorsPerCylinder * (ulong)imagePlugin.GetSectorSize();
                    part.PartitionStartSector = sdl.partitions[i].start_cylinder * sectorsPerCylinder;
                    part.PartitionType = SunIdToString((SunTypes)sdl.vtoc.infos[i].id);

                    if (part.PartitionStartSector > imagePlugin.GetSectors() || (part.PartitionStartSector + part.PartitionSectors) > imagePlugin.GetSectors())
                        return false;

                    partitions.Add(part);
                }
            }

            return true;
        }

        public static string SunFlagsToString(SunFlags flags)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (flags.HasFlag(SunFlags.NoMount))
                sb.AppendLine("Unmountable");
            if (flags.HasFlag(SunFlags.ReadOnly))
                sb.AppendLine("Read-only");
            return sb.ToString();
        }

        public static string SunIdToString(SunTypes id)
        {
            switch (id)
            {
                case SunTypes.Linux:
                    return "Linux";
                case SunTypes.LinuxRaid:
                return "Linux RAID";
                case SunTypes.LinuxSwap:
                    return "Linux swap";
                case SunTypes.LVM:
                    return "LVM";
                case SunTypes.SunBoot:
                    return "Sun boot";
                case SunTypes.SunEmpty:
                    return "Empty";
                case SunTypes.SunHome:
                    return "Sun /home";
                case SunTypes.SunRoot:
                    return "Sun /";
                case SunTypes.SunStand:
                    return "Sun /stand";
                case SunTypes.SunSwap:
                    return "Sun swap";
                case SunTypes.SunUsr:
                    return "Sun /usr";
                case SunTypes.SunVar:
                    return "Sun /var";
                case SunTypes.SunWholeDisk:
                    return "Whole disk";
                default:
                    return "Unknown";
            }
        }

        public struct SunDiskLabel
        {
            /// <summary>
            /// Offset 0x000: Informative string, 128 bytes
            /// </summary>
            public string info;
            /// <summary>
            /// Offset 0x080: Volume Table Of Contents
            /// </summary>
            public SunVTOC vtoc;
            /// <summary>
            /// Offset 0x108: Sectors to skip on writes
            /// </summary>
            public uint write_reinstruct;
            /// <summary>
            /// Offset 0x10C: Sectors to skip in reads
            /// </summary>
            public uint read_reinstruct;
            /// <summary>
            /// Offset 0x110: Unused, 148 bytes
            /// </summary>
            public byte[] spare;
            /// <summary>
            /// Offset 0x1A4: Rotational speed
            /// </summary>
            public ushort rspeed;
            /// <summary>
            /// Offset 0x1A6: Physical cylinder count
            /// </summary>
            public ushort pcylcount;
            /// <summary>
            /// Offset 0x1A8: Extra sectors per cylinder
            /// </summary>
            public ushort sparecyl;
            /// <summary>
            /// Offset 0x1AA: Obsolete, gap
            /// </summary>
            public ushort gap1;
            /// <summary>
            /// Offset 0x1AC: Obsolete, gap
            /// </summary>
            public ushort gap2;
            /// <summary>
            /// Offset 0x1AE: Interleave factor
            /// </summary>
            public ushort ilfact;
            /// <summary>
            /// Offset 0x1B0: Cylinders
            /// </summary>
            public ushort ncyl;
            /// <summary>
            /// Offset 0x1B2: Alternate cylinders
            /// </summary>
            public ushort nacyl;
            /// <summary>
            /// Offset 0x1B4: Tracks per cylinder
            /// </summary>
            public ushort ntrks;
            /// <summary>
            /// Offset 0x1B6: Sectors per track
            /// </summary>
            public ushort nsect;
            /// <summary>
            /// Offset 0x1B8: Label head offset
            /// </summary>
            public ushort bhead;
            /// <summary>
            /// Offset 0x1BA: Physical partition 
            /// </summary>
            public ushort ppart;
            /// <summary>
            /// Offset 0x1BC: Partitions
            /// </summary>
            public SunPartition[] partitions;
            /// <summary>
            /// Offset 0x1FC: 
            /// </summary>
            public ushort magic;
            /// <summary>
            /// Offset 0x1FE: XOR of label
            /// </summary>
            public ushort csum;
        }

        public struct SunVTOC
        {
            /// <summary>
            /// Offset 0x00: VTOC version
            /// </summary>
            public uint version;
            /// <summary>
            /// Offset 0x04: Volume name, 8 bytes
            /// </summary>
            public string volname;
            /// <summary>
            /// Offset 0x0C: Number of partitions
            /// </summary>
            public ushort nparts;
            /// <summary>
            /// Offset 0x0E: Partition information, 8 entries
            /// </summary>
            public SunInfo[] infos;
            /// <summary>
            /// Offset 0x2E: Padding
            /// </summary>
            public ushort padding;
            /// <summary>
            /// Offset 0x30: Information needed by mboot, 3 entries
            /// </summary>
            public uint[] bootinfo;
            /// <summary>
            /// Offset 0x3C: VTOC magic
            /// </summary>
            public uint sanity;
            /// <summary>
            /// Offset 0x40: Reserved, 40 bytes
            /// </summary>
            public byte[] reserved;
            /// <summary>
            /// Offset 0x68: Partition timestamps, 8 entries
            /// </summary>
            public uint[] timestamp;
        }

        public struct SunInfo
        {
            /// <summary>
            /// Offset 0x00: Partition ID
            /// </summary>
            public ushort id;
            /// <summary>
            /// Offset 0x02: Partition flags
            /// </summary>
            public ushort flags;
        }

        public struct SunPartition
        {
            /// <summary>
            /// Offset 0x00: Starting cylinder
            /// </summary>
            public uint start_cylinder;
            /// <summary>
            /// Offset 0x02: Sectors
            /// </summary>
            public uint num_sectors;
        }
    }
}