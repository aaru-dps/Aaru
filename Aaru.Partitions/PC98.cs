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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of NEC PC-9800 partitions</summary>
public sealed class PC98 : IPartition
{
    const string MODULE_NAME = "PC-98 partitions plugin";
    /// <inheritdoc />
    public string Name => Localization.PC98_Name;
    /// <inheritdoc />
    public Guid Id => new("27333401-C7C2-447D-961C-22AD0641A09A");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<CommonTypes.Partition> partitions, ulong sectorOffset)
    {
        partitions = new List<CommonTypes.Partition>();

        if(sectorOffset != 0)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0, out byte[] bootSector);

        if(errno          != ErrorNumber.NoError ||
           bootSector[^2] != 0x55                ||
           bootSector[^1] != 0xAA)
            return false;

        errno = imagePlugin.ReadSector(1, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        // Prevent false positives with some FAT BPBs
        if(Encoding.ASCII.GetString(bootSector, 0x36, 3) == Localization.FAT)
            return false;

        Table table = Marshal.ByteArrayToStructureLittleEndian<Table>(sector);

        ulong counter = 0;

        foreach(Partition entry in table.entries)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_mid = {0}", entry.dp_mid);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_sid = {0}", entry.dp_sid);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_dum1 = {0}", entry.dp_dum1);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_dum2 = {0}", entry.dp_dum2);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_ipl_sct = {0}", entry.dp_ipl_sct);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_ipl_head = {0}", entry.dp_ipl_head);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_ipl_cyl = {0}", entry.dp_ipl_cyl);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_ssect = {0}", entry.dp_ssect);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_shd = {0}", entry.dp_shd);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_scyl = {0}", entry.dp_scyl);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_esect = {0}", entry.dp_esect);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_ehd = {0}", entry.dp_ehd);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_ecyl = {0}", entry.dp_ecyl);

            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.dp_name = \"{0}\"",
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

            AaruConsole.DebugWriteLine(MODULE_NAME, "part.Start = {0}", part.Start);
            AaruConsole.DebugWriteLine(MODULE_NAME, "part.Type = {0}", part.Type);
            AaruConsole.DebugWriteLine(MODULE_NAME, "part.Name = {0}", part.Name);
            AaruConsole.DebugWriteLine(MODULE_NAME, "part.Sequence = {0}", part.Sequence);
            AaruConsole.DebugWriteLine(MODULE_NAME, "part.Offset = {0}", part.Offset);
            AaruConsole.DebugWriteLine(MODULE_NAME, "part.Length = {0}", part.Length);
            AaruConsole.DebugWriteLine(MODULE_NAME, "part.Size = {0}", part.Size);

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
            case 0x01: return Localization.FAT12;
            case 0x04: return Localization.PC_UX;
            case 0x06: return Localization.N88_BASIC_86;

            // Supposedly for FAT16 < 32 MiB, seen in bigger partitions
            case 0x11:
            case 0x21: return Localization.FAT16;
            case 0x28:
            case 0x41:
            case 0x48: return Localization.Windows_Volume_Set;
            case 0x44: return Localization.FreeBSD;
            case 0x61: return Localization.FAT32;
            case 0x62: return Localization.Linux;
            default:   return Localization.Unknown_partition_type;
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