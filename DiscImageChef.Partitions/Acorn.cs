// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Acorn.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Acorn FileCore partitions.
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
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    class Acorn : PartPlugin
    {
        const ulong ADFS_SB_POS = 0xC00;
        const uint LINUX_MAGIC = 0xDEAFA1DE;
        const uint SWAP_MAGIC = 0xDEAFAB1E;
        const uint RISCIX_MAGIC = 0x4A657320;
        const uint TYPE_LINUX = 9;
        const uint TYPE_RISCIX_MFM = 1;
        const uint TYPE_RISCIX_SCSI = 2;
        const uint TYPE_MASK = 15;

        public Acorn()
        {
            Name = "Acorn FileCore partitions";
            PluginUUID = new Guid("A7C8FEBE-8D00-4933-B9F3-42184C8BA808");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions)
        {
            partitions = new List<Partition>();

            ulong sbSector;

            if(imagePlugin.GetSectorSize() > ADFS_SB_POS)
                sbSector = 0;
            else
                sbSector = ADFS_SB_POS / imagePlugin.GetSectorSize();

            byte[] sector = imagePlugin.ReadSector(sbSector);

            if(sector.Length < 512)
                return false;

            AcornBootBlock bootBlock = new AcornBootBlock();
            IntPtr bbPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(sector, 0, bbPtr, 512);
            bootBlock = (AcornBootBlock)Marshal.PtrToStructure(bbPtr, typeof(AcornBootBlock));
            Marshal.FreeHGlobal(bbPtr);

            int checksum = 0;
            for(int i = 0; i < 0x1FF; i++)
                checksum = ((checksum & 0xFF) + (checksum >> 8) + sector[i]);

            int heads = bootBlock.discRecords.heads + ((bootBlock.discRecords.lowsector >> 6) & 1);
            int secCyl = bootBlock.discRecords.spt * heads;
            int mapSector = bootBlock.startCylinder * secCyl;

            byte[] map = imagePlugin.ReadSector((ulong)mapSector);

            ulong counter = 0;

            if((bootBlock.flags & TYPE_MASK) == TYPE_LINUX)
            {
                LinuxTable table = new LinuxTable();
                IntPtr tablePtr = Marshal.AllocHGlobal(512);
                Marshal.Copy(map, 0, tablePtr, 512);
                table = (LinuxTable)Marshal.PtrToStructure(tablePtr, typeof(LinuxTable));
                Marshal.FreeHGlobal(tablePtr);

                foreach(LinuxEntry entry in table.entries)
                {
                    Partition part = new Partition();
                    part.PartitionStartSector = (ulong)(mapSector + entry.start);
                    part.PartitionStart = part.PartitionStartSector * (ulong)sector.Length;
                    part.PartitionLength = entry.size;
                    part.PartitionSectors = (ulong)(entry.size * sector.Length);
                    part.PartitionSequence = counter;
                    if(entry.magic == LINUX_MAGIC || entry.magic == SWAP_MAGIC)
                    {
                        partitions.Add(part);
                        counter++;
                    }
                }
            }
            else if((bootBlock.flags & TYPE_MASK) == TYPE_RISCIX_MFM ||
                    (bootBlock.flags & TYPE_MASK) == TYPE_RISCIX_SCSI)
            {
                RiscIxTable table = new RiscIxTable();
                IntPtr tablePtr = Marshal.AllocHGlobal(512);
                Marshal.Copy(map, 0, tablePtr, 512);
                table = (RiscIxTable)Marshal.PtrToStructure(tablePtr, typeof(RiscIxTable));
                Marshal.FreeHGlobal(tablePtr);

                if(table.magic == RISCIX_MAGIC)
                {
                    foreach(RiscIxEntry entry in table.partitions)
                    {
                        Partition part = new Partition();
                        part.PartitionStartSector = (ulong)(mapSector + entry.start);
                        part.PartitionStart = part.PartitionStartSector * (ulong)sector.Length;
                        part.PartitionLength = entry.length;
                        part.PartitionSectors = (ulong)(entry.length * sector.Length);
                        part.PartitionName = StringHandlers.CToString(entry.name, Encoding.GetEncoding("iso-8859-1"));
                        part.PartitionSequence = counter;
                        if(entry.length > 0)
                        {
                            partitions.Add(part);
                            counter++;
                        }
                    }
                }
            }

            return !(partitions.Count == 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DiscRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1C0)]
            public byte[] spare;
            public byte log2secsize;
            public byte spt;
            public byte heads;
            public byte density;
            public byte idlen;
            public byte log2bpmb;
            public byte skew;
            public byte bootoption;
            public byte lowsector;
            public byte nzones;
            public ushort zone_spare;
            public uint root;
            public uint disc_size;
            public ushort disc_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] disc_name;
            public uint disc_type;
            public uint disc_size_high;
            public byte flags;
            public byte nzones_high;
            public uint format_version;
            public uint root_size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AcornBootBlock
        {
            public DiscRecord discRecords;
            public byte flags;
            public ushort startCylinder;
            public byte checksum;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct LinuxTable
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
            public LinuxEntry[] entries;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct LinuxEntry
        {
            public uint magic;
            public uint start;
            public uint size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RiscIxTable
        {
            public uint magic;
            public uint date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public RiscIxEntry[] partitions;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RiscIxEntry
        {
            public uint start;
            public uint length;
            public uint one;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] name;
        }
    }
}