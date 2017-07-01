// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Human68k.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
using DiscImageChef.Console;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    public class Human68K : PartPlugin
    {
        const uint X68kMagic = 0x5836384B;

        public Human68K()
        {
            Name = "Human 68k partitions";
            PluginUUID = new Guid("246A6D93-4F1A-1F8A-344D-50187A5513A9");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions)
        {
            partitions = new List<Partition>();

            byte[] sector;
            ulong sectsPerUnit = 0;

            DicConsole.DebugWriteLine("Human68k plugin", "sectorSize = {0}", imagePlugin.GetSectorSize());

            switch(imagePlugin.GetSectorSize())
            {
                case 256:
                    sector = imagePlugin.ReadSector(4);
                    sectsPerUnit = 1;
                    break;
                case 512:
                    sector = imagePlugin.ReadSector(4);
                    sectsPerUnit = 2;
                    break;
                case 1024:
                    sector = imagePlugin.ReadSector(2);
                    sectsPerUnit = 1;
                    break;
                default:
                    return false;
            }

            X68kTable table = BigEndianMarshal.ByteArrayToStructureBigEndian<X68kTable>(sector);
            for(int i = 0; i < table.entries.Length; i++)
                table.entries[i] = BigEndianMarshal.SwapStructureMembersEndian(table.entries[i]);

            DicConsole.DebugWriteLine("Human68k plugin", "table.signature = {0:X4}", table.magic);
            DicConsole.DebugWriteLine("Human68k plugin", "table.size = {0:X4}", table.size);
            DicConsole.DebugWriteLine("Human68k plugin", "table.size2 = {0:X4}", table.size2);
            DicConsole.DebugWriteLine("Human68k plugin", "table.unknown = {0:X4}", table.unknown);

            if(table.magic != X68kMagic)
                return false;

            ulong counter = 0;

            foreach(X68kEntry entry in table.entries)
            {
                DicConsole.DebugWriteLine("Human68k plugin", "entry.name = {0}", StringHandlers.CToString(entry.name, Encoding.GetEncoding(932)));
                DicConsole.DebugWriteLine("Human68k plugin", "entry.stateStart = {0}", entry.stateStart);
                DicConsole.DebugWriteLine("Human68k plugin", "entry.length = {0}", entry.length);
                DicConsole.DebugWriteLine("Human68k plugin", "sectsPerUnit = {0} {1}", sectsPerUnit, imagePlugin.GetSectorSize());

                Partition part = new Partition();
                part.PartitionStartSector = (entry.stateStart & 0xFFFFFF) * sectsPerUnit;
                part.PartitionStart = part.PartitionStartSector * (ulong)sector.Length;
                part.PartitionSectors = entry.length * sectsPerUnit;
                part.PartitionLength = part.PartitionSectors * (ulong)sector.Length;
                part.PartitionType = StringHandlers.CToString(entry.name, Encoding.GetEncoding(932));
                part.PartitionSequence = counter;
                if(entry.length > 0)
                {
                    partitions.Add(part);
                    counter++;
                }
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct X68kTable
        {
            public uint magic;
            public uint size;
            public uint size2;
            public uint unknown;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public X68kEntry[] entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct X68kEntry
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] name;
            public uint stateStart;
            public uint length;
        }
    }
}