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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.Partitions
{
    public class Acorn : IPartition
    {
        const ulong ADFS_SB_POS      = 0xC00;
        const uint  LINUX_MAGIC      = 0xDEAFA1DE;
        const uint  SWAP_MAGIC       = 0xDEAFAB1E;
        const uint  RISCIX_MAGIC     = 0x4A657320;
        const uint  TYPE_LINUX       = 9;
        const uint  TYPE_RISCIX_MFM  = 1;
        const uint  TYPE_RISCIX_SCSI = 2;
        const uint  TYPE_MASK        = 15;

        public string Name   => "Acorn FileCore partitions";
        public Guid   Id     => new Guid("A7C8FEBE-8D00-4933-B9F3-42184C8BA808");
        public string Author => "Natalia Portillo";

        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            ulong sbSector;

            // RISC OS always checks for the partition on 0. Afaik no emulator chains it.
            if(sectorOffset != 0) return false;

            if(imagePlugin.Info.SectorSize > ADFS_SB_POS) sbSector = 0;
            else sbSector                                          = ADFS_SB_POS / imagePlugin.Info.SectorSize;

            byte[] sector = imagePlugin.ReadSector(sbSector);

            if(sector.Length < 512) return false;

            IntPtr bbPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(sector, 0, bbPtr, 512);
            AcornBootBlock bootBlock = (AcornBootBlock)Marshal.PtrToStructure(bbPtr, typeof(AcornBootBlock));
            Marshal.FreeHGlobal(bbPtr);

            int checksum                            = 0;
            for(int i = 0; i < 0x1FF; i++) checksum = (checksum & 0xFF) + (checksum >> 8) + sector[i];

            int heads     = bootBlock.discRecord.heads + ((bootBlock.discRecord.lowsector >> 6) & 1);
            int secCyl    = bootBlock.discRecord.spt * heads;
            int mapSector = bootBlock.startCylinder  * secCyl;

            if((ulong)mapSector >= imagePlugin.Info.Sectors) return false;

            byte[] map = imagePlugin.ReadSector((ulong)mapSector);

            ulong counter = 0;

            if(checksum == bootBlock.checksum)
            {
                Partition part = new Partition
                {
                    Size =
                        (ulong)bootBlock.discRecord.disc_size_high * 0x100000000 + bootBlock.discRecord.disc_size,
                    Length =
                        ((ulong)bootBlock.discRecord.disc_size_high * 0x100000000 +
                         bootBlock.discRecord.disc_size) / imagePlugin.Info.SectorSize,
                    Type = "ADFS",
                    Name = StringHandlers.CToString(bootBlock.discRecord.disc_name,
                                                    Encoding.GetEncoding("iso-8859-1"))
                };
                if(part.Size > 0)
                {
                    partitions.Add(part);
                    counter++;
                }
            }

            switch(bootBlock.flags & TYPE_MASK)
            {
                case TYPE_LINUX:
                {
                    IntPtr tablePtr = Marshal.AllocHGlobal(512);
                    Marshal.Copy(map, 0, tablePtr, 512);
                    LinuxTable table = (LinuxTable)Marshal.PtrToStructure(tablePtr, typeof(LinuxTable));
                    Marshal.FreeHGlobal(tablePtr);

                    foreach(LinuxEntry entry in table.entries)
                    {
                        Partition part = new Partition
                        {
                            Start    = (ulong)(mapSector + entry.start),
                            Size     = entry.size,
                            Length   = (ulong)(entry.size * sector.Length),
                            Sequence = counter,
                            Scheme   = Name
                        };
                        part.Offset = part.Start * (ulong)sector.Length;
                        if(entry.magic != LINUX_MAGIC && entry.magic != SWAP_MAGIC) continue;

                        partitions.Add(part);
                        counter++;
                    }

                    break;
                }
                case TYPE_RISCIX_MFM:
                case TYPE_RISCIX_SCSI:
                {
                    IntPtr tablePtr = Marshal.AllocHGlobal(512);
                    Marshal.Copy(map, 0, tablePtr, 512);
                    RiscIxTable table = (RiscIxTable)Marshal.PtrToStructure(tablePtr, typeof(RiscIxTable));
                    Marshal.FreeHGlobal(tablePtr);

                    if(table.magic == RISCIX_MAGIC)
                        foreach(RiscIxEntry entry in table.partitions)
                        {
                            Partition part = new Partition
                            {
                                Start    = (ulong)(mapSector + entry.start),
                                Size     = entry.length,
                                Length   = (ulong)(entry.length * sector.Length),
                                Name     = StringHandlers.CToString(entry.name, Encoding.GetEncoding("iso-8859-1")),
                                Sequence = counter,
                                Scheme   = Name
                            };
                            part.Offset = part.Start * (ulong)sector.Length;
                            if(entry.length <= 0) continue;

                            partitions.Add(part);
                            counter++;
                        }

                    break;
                }
            }

            return partitions.Count != 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DiscRecord
        {
            public byte   log2secsize;
            public byte   spt;
            public byte   heads;
            public byte   density;
            public byte   idlen;
            public byte   log2bpmb;
            public byte   skew;
            public byte   bootoption;
            public byte   lowsector;
            public byte   nzones;
            public ushort zone_spare;
            public uint   root;
            public uint   disc_size;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1C0)]
            public byte[] spare;
            public DiscRecord discRecord;
            public byte       flags;
            public ushort     startCylinder;
            public byte       checksum;
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