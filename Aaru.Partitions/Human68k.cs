// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Human68k.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Human68k (Sharp X68000) partitions.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions
{
    /// <inheritdoc />
    /// <summary>Implements decoding of Sharp's Human68K partitions</summary>
    public sealed class Human68K : IPartition
    {
        // ReSharper disable once InconsistentNaming
        const uint X68K_MAGIC = 0x5836384B;

        /// <inheritdoc />
        public string Name => "Human 68k partitions";
        /// <inheritdoc />
        public Guid Id => new("246A6D93-4F1A-1F8A-344D-50187A5513A9");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            byte[]      sector;
            ulong       sectsPerUnit;
            ErrorNumber errno;

            AaruConsole.DebugWriteLine("Human68k plugin", "sectorSize = {0}", imagePlugin.Info.SectorSize);

            if(sectorOffset + 4 >= imagePlugin.Info.Sectors)
                return false;

            switch(imagePlugin.Info.SectorSize)
            {
                case 256:
                    errno        = imagePlugin.ReadSector(4 + sectorOffset, out sector);
                    sectsPerUnit = 1;

                    break;
                case 512:
                    errno        = imagePlugin.ReadSector(4 + sectorOffset, out sector);
                    sectsPerUnit = 2;

                    break;
                case 1024:
                    errno        = imagePlugin.ReadSector(2 + sectorOffset, out sector);
                    sectsPerUnit = 1;

                    break;
                default: return false;
            }

            if(errno != ErrorNumber.NoError)
                return false;

            Table table = Marshal.ByteArrayToStructureBigEndian<Table>(sector);

            AaruConsole.DebugWriteLine("Human68k plugin", "table.magic = {0:X4}", table.magic);

            if(table.magic != X68K_MAGIC)
                return false;

            for(int i = 0; i < table.entries.Length; i++)
                table.entries[i] = (Entry)Marshal.SwapStructureMembersEndian(table.entries[i]);

            AaruConsole.DebugWriteLine("Human68k plugin", "table.size = {0:X4}", table.size);
            AaruConsole.DebugWriteLine("Human68k plugin", "table.size2 = {0:X4}", table.size2);
            AaruConsole.DebugWriteLine("Human68k plugin", "table.unknown = {0:X4}", table.unknown);

            ulong counter = 0;

            foreach(Entry entry in table.entries)
            {
                AaruConsole.DebugWriteLine("Human68k plugin", "entry.name = {0}",
                                           StringHandlers.CToString(entry.name, Encoding.GetEncoding(932)));

                AaruConsole.DebugWriteLine("Human68k plugin", "entry.stateStart = {0}", entry.stateStart);
                AaruConsole.DebugWriteLine("Human68k plugin", "entry.length = {0}", entry.length);

                AaruConsole.DebugWriteLine("Human68k plugin", "sectsPerUnit = {0} {1}", sectsPerUnit,
                                           imagePlugin.Info.SectorSize);

                var part = new Partition
                {
                    Start    = (entry.stateStart & 0xFFFFFF) * sectsPerUnit,
                    Length   = entry.length                  * sectsPerUnit,
                    Type     = StringHandlers.CToString(entry.name, Encoding.GetEncoding(932)),
                    Sequence = counter,
                    Scheme   = Name
                };

                part.Offset = part.Start  * (ulong)sector.Length;
                part.Size   = part.Length * (ulong)sector.Length;

                if(entry.length <= 0)
                    continue;

                partitions.Add(part);
                counter++;
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Table
        {
            public readonly uint magic;
            public readonly uint size;
            public readonly uint size2;
            public readonly uint unknown;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly Entry[] entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Entry
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] name;
            public readonly uint stateStart;
            public readonly uint length;
        }
    }
}