// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PC98.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages NEC PC-9800 partitions.
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
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    class PC98 : PartPlugin
    {
        const ushort IntelMagic = 0xAA55;

        public PC98()
        {
            Name = "NEC PC-9800 partitions table";
            PluginUUID = new Guid("27333401-C7C2-447D-961C-22AD0641A09A\n");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions)
        {
            partitions = new List<Partition>();

            byte[] bootSector = imagePlugin.ReadSector(0);
            byte[] sector = imagePlugin.ReadSector(1);
            if(sector.Length < 512)
                return false;
            if(bootSector[0x1FE] != 0x55 || bootSector[0x1FF] != 0xAA)
                return false;

            PC98Table table = new PC98Table();
            IntPtr tablePtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(sector, 0, tablePtr, 512);
            table = (PC98Table)Marshal.PtrToStructure(tablePtr, typeof(PC98Table));
            Marshal.FreeHGlobal(tablePtr);

            ulong counter = 0;

            foreach(PC98Partition entry in table.entries)
            {
                if(entry.dp_ssect != entry.dp_esect &&
                   entry.dp_shd != entry.dp_ehd &&
                   entry.dp_scyl != entry.dp_ecyl &&
                   entry.dp_ecyl > 0)
                {

                    Partition part = new Partition();
                    part.PartitionStartSector = CHStoLBA(entry.dp_scyl, entry.dp_shd, entry.dp_ssect);
                    part.PartitionStart = part.PartitionStartSector * imagePlugin.GetSectorSize();
                    part.PartitionSectors = CHStoLBA(entry.dp_ecyl, entry.dp_ehd, entry.dp_esect) - part.PartitionStartSector;
                    part.PartitionLength = part.PartitionSectors * imagePlugin.GetSectorSize();
                    part.PartitionType = string.Format("{0}", (entry.dp_sid << 8) | entry.dp_mid);
                    part.PartitionName = StringHandlers.CToString(entry.dp_name, Encoding.GetEncoding(932));
                    part.PartitionSequence = counter;

                    if((entry.dp_sid & 0x7F) == 0x44 &&
                       (entry.dp_mid & 0x7F) == 0x14 &&
                        part.PartitionStartSector < imagePlugin.ImageInfo.sectors &&
                            part.PartitionSectors + part.PartitionStartSector <= imagePlugin.ImageInfo.sectors)
                    {
                        partitions.Add(part);
                        counter++;
                    }
                }
            }

            return partitions.Count > 0;
        }

        static uint CHStoLBA(ushort cyl, byte head, byte sector)
        {
#pragma warning disable IDE0004 // Remove Unnecessary Cast
            return (((uint)cyl * 16) + (uint)head) * 63 + (uint)sector - 1;
#pragma warning restore IDE0004 // Remove Unnecessary Cast
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PC98Table
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public PC98Partition[] entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PC98Partition
        {
            public byte dp_mid;
            public byte dp_sid;
            public byte dp_dum1;
            public byte dp_dum2;
            public byte dp_ipl_sct;
            public byte dp_ipl_head;
            public ushort dp_ipl_cyl;
            public byte dp_ssect;
            public byte dp_shd;
            public ushort dp_scyl;
            public byte dp_esect;
            public byte dp_ehd;
            public ushort dp_ecyl;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] dp_name;
        }
    }
}