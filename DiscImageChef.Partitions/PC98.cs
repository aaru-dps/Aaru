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
using DiscImageChef.Console;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    public class PC98 : PartPlugin
    {
        const ushort IntelMagic = 0xAA55;

        public PC98()
        {
            Name = "NEC PC-9800 partitions table";
            PluginUUID = new Guid("27333401-C7C2-447D-961C-22AD0641A09A\n");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            byte[] bootSector = imagePlugin.ReadSector(sectorOffset);
            byte[] sector = imagePlugin.ReadSector(1 + sectorOffset);
            if(bootSector[bootSector.Length-2] != 0x55 || bootSector[bootSector.Length - 1] != 0xAA)
                return false;

            PC98Table table = new PC98Table();
            IntPtr tablePtr = Marshal.AllocHGlobal(256);
            Marshal.Copy(sector, 0, tablePtr, 256);
            table = (PC98Table)Marshal.PtrToStructure(tablePtr, typeof(PC98Table));
            Marshal.FreeHGlobal(tablePtr);

            ulong counter = 0;

            foreach(PC98Partition entry in table.entries)
            {
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_mid = {0}", entry.dp_mid);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_sid = {0}", entry.dp_sid);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_dum1 = {0}", entry.dp_dum1);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_dum2 = {0}", entry.dp_dum2);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_ipl_sct = {0}", entry.dp_ipl_sct);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_ipl_head = {0}", entry.dp_ipl_head);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_ipl_cyl = {0}", entry.dp_ipl_cyl);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_ssect = {0}", entry.dp_ssect);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_shd = {0}", entry.dp_shd);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_scyl = {0}", entry.dp_scyl);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_esect = {0}", entry.dp_esect);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_ehd = {0}", entry.dp_ehd);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_ecyl = {0}", entry.dp_ecyl);
                DicConsole.DebugWriteLine("PC98 plugin", "entry.dp_name = \"{0}\"", StringHandlers.CToString(entry.dp_name, Encoding.GetEncoding(932)));

                if(entry.dp_ssect != entry.dp_esect &&
                   entry.dp_shd != entry.dp_ehd &&
                   entry.dp_scyl != entry.dp_ecyl &&
                   entry.dp_ecyl > 0)
                {

                    Partition part = new Partition
                    {
                        Start = Helpers.CHS.ToLBA(entry.dp_scyl, entry.dp_shd, entry.dp_ssect, imagePlugin.ImageInfo.heads, imagePlugin.ImageInfo.sectorsPerTrack),
                        Type = string.Format("{0}", (entry.dp_sid << 8) | entry.dp_mid),
                        Name = StringHandlers.CToString(entry.dp_name, Encoding.GetEncoding(932)),
                        Sequence = counter,
                        Scheme = Name
                    };
                    part.Offset = part.Start * imagePlugin.GetSectorSize();
                    part.Length = Helpers.CHS.ToLBA(entry.dp_ecyl, entry.dp_ehd, entry.dp_esect, imagePlugin.ImageInfo.heads, imagePlugin.ImageInfo.sectorsPerTrack) - part.Start;
                    part.Size = part.Length * imagePlugin.GetSectorSize();

                    if((entry.dp_sid & 0x7F) == 0x44 &&
                       (entry.dp_mid & 0x7F) == 0x14 &&
                        part.Start < imagePlugin.ImageInfo.sectors &&
                            part.Length + part.Start <= imagePlugin.ImageInfo.sectors)
                    {
                        partitions.Add(part);
                        counter++;
                    }
                }
            }

            return partitions.Count > 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PC98Table
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
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