// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RioKarma.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Rio Karma partitions.
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
    public class RioKarma : PartPlugin
    {
        const ushort KarmaMagic = 0xAB56;
        const byte EntryMagic = 0x4D;

        public RioKarma()
        {
            Name = "Rio Karma partitioning";
            PluginUUID = new Guid("246A6D93-4F1A-1F8A-344D-50187A5513A9");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            byte[] sector = imagePlugin.ReadSector(sectorOffset);
            if(sector.Length < 512) return false;

            RioKarmaTable table = new RioKarmaTable();
            IntPtr tablePtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(sector, 0, tablePtr, 512);
            table = (RioKarmaTable)Marshal.PtrToStructure(tablePtr, typeof(RioKarmaTable));
            Marshal.FreeHGlobal(tablePtr);

            if(table.magic != KarmaMagic) return false;

            ulong counter = 0;

            foreach(RioKarmaEntry entry in table.entries)
            {
                Partition part = new Partition
                {
                    Start = entry.offset,
                    Offset = (ulong)(entry.offset * sector.Length),
                    Size = entry.size,
                    Length = (ulong)(entry.size * sector.Length),
                    Type = "Rio Karma",
                    Sequence = counter,
                    Scheme = Name
                };
                if(entry.type == EntryMagic)
                {
                    partitions.Add(part);
                    counter++;
                }
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RioKarmaTable
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 270)] public byte[] reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public RioKarmaEntry[] entries;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 208)] public byte[] padding;
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RioKarmaEntry
        {
            public uint reserved;
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] reserved2;
            public uint offset;
            public uint size;
        }
    }
}