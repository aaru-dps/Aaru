// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SGI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages SGI DVHs.
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
    class SGI : PartPlugin
    {
        const int SGI_MAGIC = 0x0BE5A941;

        public SGI()
        {
            Name = "SGI Disk Volume Header";
            PluginUUID = new Guid("AEF5AB45-4880-4CE8-8735-F0A402E2E5F2");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions)
        {
            partitions = new List<Partition>();

            byte[] sector = imagePlugin.ReadSector(0);
            if(sector.Length < 512)
                return false;

            SGILabel disklabel = BigEndianMarshal.ByteArrayToStructureBigEndian<SGILabel>(sector);
            for(int i = 0; i < disklabel.volume.Length; i++)
                disklabel.volume[i] = BigEndianMarshal.SwapStructureMembersEndian(disklabel.volume[i]);
            for(int i = 0; i < disklabel.partitions.Length; i++)
                disklabel.partitions[i] = BigEndianMarshal.SwapStructureMembersEndian(disklabel.partitions[i]);

            if(disklabel.magic != SGI_MAGIC)
                return false;

            ulong counter = 0;

            foreach(SGIPartition entry in disklabel.partitions)
            {
                Partition part = new Partition();
                part.PartitionStartSector = entry.first_block;
                part.PartitionStart = (entry.first_block * 512);
                part.PartitionLength = entry.num_blocks;
                part.PartitionSectors = (entry.num_blocks * 512);
                part.PartitionType = string.Format("{0}", entry.type);
                part.PartitionSequence = counter;
                if(part.PartitionLength > 0)
                {
                    partitions.Add(part);
                    counter++;
                }
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SGILabel
        {
            /// <summary></summary>
            public uint magic;
            /// <summary></summary>
            public ushort root_part_num;
            /// <summary></summary>
            public ushort swap_part_num;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] boot_file;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] device_params;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public SGIVolume[] volume;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public SGIPartition[] partitions;
            /// <summary></summary>
            public uint csum;
            /// <summary></summary>
            public uint padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SGIVolume
        {
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] name;
            /// <summary></summary>
            public uint block_num;
            /// <summary></summary>
            public uint num_bytes;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SGIPartition
        {
            /// <summary></summary>
            public uint num_blocks;
            /// <summary></summary>
            public uint first_block;
            /// <summary></summary>
            public uint type;
        }
    }
}