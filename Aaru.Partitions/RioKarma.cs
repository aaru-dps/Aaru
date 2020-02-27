// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions
{
    public class RioKarma : IPartition
    {
        const ushort KARMA_MAGIC = 0xAB56;
        const byte   ENTRY_MAGIC = 0x4D;

        public string Name   => "Rio Karma partitioning";
        public Guid   Id     => new Guid("246A6D93-4F1A-1F8A-344D-50187A5513A9");
        public string Author => "Natalia Portillo";

        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = null;
            byte[] sector = imagePlugin.ReadSector(sectorOffset);
            if(sector.Length < 512) return false;

            RioKarmaTable table = Marshal.ByteArrayToStructureLittleEndian<RioKarmaTable>(sector);

            if(table.magic != KARMA_MAGIC) return false;

            ulong counter = 0;

            partitions = (from entry in table.entries
                          let part = new Partition
                          {
                              Start    = entry.offset,
                              Offset   = (ulong)(entry.offset * sector.Length),
                              Size     = entry.size,
                              Length   = (ulong)(entry.size * sector.Length),
                              Type     = "Rio Karma",
                              Sequence = counter++,
                              Scheme   = Name
                          }
                          where entry.type == ENTRY_MAGIC
                          select part).ToList();

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RioKarmaTable
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 270)]
            public byte[] reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public RioKarmaEntry[] entries;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 208)]
            public byte[] padding;
            public ushort magic;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RioKarmaEntry
        {
            public uint reserved;
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] reserved2;
            public uint offset;
            public uint size;
        }
    }
}