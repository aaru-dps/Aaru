// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions
{
    /// <inheritdoc />
    /// <summary>Implements decoding of NEC PC-9800 partitions</summary>
    public sealed class PC98 : IPartition
    {
        /// <inheritdoc />
        public string Name => "NEC PC-9800 partition table";
        /// <inheritdoc />
        public Guid Id => new Guid("27333401-C7C2-447D-961C-22AD0641A09A");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<CommonTypes.Partition> partitions,
                                   ulong sectorOffset)
        {
            partitions = new List<CommonTypes.Partition>();

            if(sectorOffset != 0)
                return false;

            byte[] bootSector = imagePlugin.ReadSector(0);
            byte[] sector     = imagePlugin.ReadSector(1);

            if(bootSector[^2] != 0x55 ||
               bootSector[^1] != 0xAA)
                return false;

            // Prevent false positives with some FAT BPBs
            if(Encoding.ASCII.GetString(bootSector, 0x36, 3) == "FAT")
                return false;

            Table table = Marshal.ByteArrayToStructureLittleEndian<Table>(sector);

            ulong counter = 0;

            foreach(Partition entry in table.entries)
            {
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_mid = {0}", entry.dp_mid);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_sid = {0}", entry.dp_sid);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_dum1 = {0}", entry.dp_dum1);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_dum2 = {0}", entry.dp_dum2);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_ipl_sct = {0}", entry.dp_ipl_sct);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_ipl_head = {0}", entry.dp_ipl_head);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_ipl_cyl = {0}", entry.dp_ipl_cyl);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_ssect = {0}", entry.dp_ssect);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_shd = {0}", entry.dp_shd);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_scyl = {0}", entry.dp_scyl);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_esect = {0}", entry.dp_esect);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_ehd = {0}", entry.dp_ehd);
                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_ecyl = {0}", entry.dp_ecyl);

                AaruConsole.DebugWriteLine("PC98 plugin", "entry.dp_name = \"{0}\"",
                                           StringHandlers.CToString(entry.dp_name, Encoding.GetEncoding(932)));

                if(entry.dp_scyl  == entry.dp_ecyl                   ||
                   entry.dp_ecyl  <= 0                               ||
                   entry.dp_scyl  > imagePlugin.Info.Cylinders       ||
                   entry.dp_ecyl  > imagePlugin.Info.Cylinders       ||
                   entry.dp_shd   > imagePlugin.Info.Heads           ||
                   entry.dp_ehd   > imagePlugin.Info.Heads           ||
                   entry.dp_ssect > imagePlugin.Info.SectorsPerTrack ||
                   entry.dp_esect > imagePlugin.Info.SectorsPerTrack)
                    continue;

                var part = new CommonTypes.Partition
                {
                    Start = CHS.ToLBA(entry.dp_scyl, entry.dp_shd, (uint)(entry.dp_ssect + 1), imagePlugin.Info.Heads,
                                      imagePlugin.Info.SectorsPerTrack),
                    Type     = DecodePC98Sid(entry.dp_sid),
                    Name     = StringHandlers.CToString(entry.dp_name, Encoding.GetEncoding(932)).Trim(),
                    Sequence = counter,
                    Scheme   = Name
                };

                part.Offset = part.Start * imagePlugin.Info.SectorSize;

                part.Length = CHS.ToLBA(entry.dp_ecyl, entry.dp_ehd, (uint)(entry.dp_esect + 1), imagePlugin.Info.Heads,
                                        imagePlugin.Info.SectorsPerTrack) - part.Start;

                part.Size = part.Length * imagePlugin.Info.SectorSize;

                AaruConsole.DebugWriteLine("PC98 plugin", "part.Start = {0}", part.Start);
                AaruConsole.DebugWriteLine("PC98 plugin", "part.Type = {0}", part.Type);
                AaruConsole.DebugWriteLine("PC98 plugin", "part.Name = {0}", part.Name);
                AaruConsole.DebugWriteLine("PC98 plugin", "part.Sequence = {0}", part.Sequence);
                AaruConsole.DebugWriteLine("PC98 plugin", "part.Offset = {0}", part.Offset);
                AaruConsole.DebugWriteLine("PC98 plugin", "part.Length = {0}", part.Length);
                AaruConsole.DebugWriteLine("PC98 plugin", "part.Size = {0}", part.Size);

                if(((entry.dp_mid & 0x20) != 0x20 && (entry.dp_mid & 0x44) != 0x44) ||
                   part.Start >= imagePlugin.Info.Sectors                           ||
                   part.End   > imagePlugin.Info.Sectors)
                    continue;

                partitions.Add(part);
                counter++;
            }

            return partitions.Count > 0;
        }

        static string DecodePC98Sid(byte sid)
        {
            switch(sid & 0x7F)
            {
                case 0x01: return "FAT12";
                case 0x04: return "PC-UX";
                case 0x06: return "N88-BASIC(86)";

                // Supposedly for FAT16 < 32 MiB, seen in bigger partitions
                case 0x11:
                case 0x21: return "FAT16";
                case 0x28:
                case 0x41:
                case 0x48: return "Windows Volume Set";
                case 0x44: return "FreeBSD";
                case 0x61: return "FAT32";
                case 0x62: return "Linux";
                default:   return "Unknown";
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Table
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly Partition[] entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Partition
        {
            /// <summary>Some ID, if 0x80 bit is set, it is bootable</summary>
            public readonly byte dp_mid;
            /// <summary>Some ID, if 0x80 bit is set, it is active</summary>
            public readonly byte dp_sid;
            public readonly byte   dp_dum1;
            public readonly byte   dp_dum2;
            public readonly byte   dp_ipl_sct;
            public readonly byte   dp_ipl_head;
            public readonly ushort dp_ipl_cyl;
            public readonly byte   dp_ssect;
            public readonly byte   dp_shd;
            public readonly ushort dp_scyl;
            public readonly byte   dp_esect;
            public readonly byte   dp_ehd;
            public readonly ushort dp_ecyl;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] dp_name;
        }
    }
}