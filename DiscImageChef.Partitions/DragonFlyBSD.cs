// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DragonFlyBSD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages DragonFly BSD 64-bit disklabels.
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
    class DragonFlyBSD : PartPlugin
    {
        const uint DISK_MAGIC64 = 0xC4464C59;

        public DragonFlyBSD()
        {
            Name = "DragonFly BSD 64-bit disklabel";
            PluginUUID = new Guid("D49E41A6-D952-4760-9D94-03DAE2450C5F");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions)
        {
            partitions = new List<Partition>();
            uint nSectors = 2048 / imagePlugin.GetSectorSize();

            byte[] sectors = imagePlugin.ReadSectors(0, nSectors);
            if(sectors.Length < 2048)
                return false;

            Disklabel64 disklabel = new Disklabel64();
            IntPtr labelPtr = Marshal.AllocHGlobal(2048);
            Marshal.Copy(sectors, 0, labelPtr, 2048);
            disklabel = (Disklabel64)Marshal.PtrToStructure(labelPtr, typeof(Disklabel64));
            Marshal.FreeHGlobal(labelPtr);

            if(disklabel.d_magic != 0xC4464C59)
                return false;

            ulong counter = 0;

            foreach(Partition64 entry in disklabel.d_partitions)
            {
                Partition part = new Partition();
                part.PartitionStartSector = entry.p_boffset;
                part.PartitionStart = entry.p_boffset;
                part.PartitionLength = entry.p_bsize;
                part.PartitionSectors = entry.p_bsize;
                if((BSD.fsType)entry.p_fstype == BSD.fsType.Other)
                    part.PartitionType = entry.p_type_uuid.ToString();
                else
                    part.PartitionType = BSD.fsTypeToString((BSD.fsType)entry.p_fstype);
                part.PartitionName = entry.p_stor_uuid.ToString();
                part.PartitionSequence = counter;
                if(entry.p_bsize > 0 && entry.p_boffset > 0)
                {
                    partitions.Add(part);
                    counter++;
                }
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Disklabel64
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] d_reserved0;
            public uint d_magic;
            public uint d_crc;
            public uint d_align;
            public uint d_npartitions;
            public Guid d_stor_uuid;
            public ulong d_total_size;
            public ulong d_bbase;
            public ulong d_pbase;
            public ulong d_pstop;
            public ulong d_abase;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] d_packname;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] d_reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public Partition64[] d_partitions;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Partition64
        {
            public ulong p_boffset;
            public ulong p_bsize;
            public byte p_fstype;
            public byte p_unused01;
            public byte p_unused02;
            public byte p_unused03;
            public uint p_unused04;
            public uint p_unused05;
            public uint p_unused06;
            public Guid p_type_uuid;
            public Guid p_stor_uuid;
        }
    }
}