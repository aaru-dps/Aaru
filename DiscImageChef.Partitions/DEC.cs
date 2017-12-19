// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DEC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages DEC disklabels.
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
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    public class DEC : PartPlugin
    {
        const int PT_MAGIC = 0x032957;
        const int PT_VALID = 1;

        public DEC()
        {
            Name = "DEC disklabel";
            PluginUUID = new Guid("58CEC3B7-3B93-4D47-86EE-D6DADE9D444F");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            if(31 + sectorOffset >= imagePlugin.GetSectors()) return false;

            byte[] sector = imagePlugin.ReadSector(31 + sectorOffset);
            if(sector.Length < 512) return false;

            DECLabel table = new DECLabel();
            IntPtr tablePtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(sector, 0, tablePtr, 512);
            table = (DECLabel)Marshal.PtrToStructure(tablePtr, typeof(DECLabel));
            Marshal.FreeHGlobal(tablePtr);

            if(table.pt_magic != PT_MAGIC || table.pt_valid != PT_VALID) return false;

            ulong counter = 0;

            foreach(DECPartition entry in table.pt_part)
            {
                Partition part = new Partition
                {
                    Start = entry.pi_blkoff,
                    Offset = (ulong)(entry.pi_blkoff * sector.Length),
                    Size = (ulong)entry.pi_nblocks,
                    Length = (ulong)(entry.pi_nblocks * sector.Length),
                    Sequence = counter,
                    Scheme = Name
                };
                if(part.Size > 0)
                {
                    partitions.Add(part);
                    counter++;
                }
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DECLabel
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 440)] public byte[] padding;
            public int pt_magic;
            public int pt_valid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public DECPartition[] pt_part;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DECPartition
        {
            public int pi_nblocks;
            public uint pi_blkoff;
        }
    }
}