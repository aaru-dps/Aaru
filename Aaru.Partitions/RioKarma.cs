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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of Rio Karma partitions</summary>
public sealed class RioKarma : IPartition
{
    const ushort KARMA_MAGIC = 0xAB56;
    const byte   ENTRY_MAGIC = 0x4D;

    /// <inheritdoc />
    public string Name => Localization.RioKarma_Name;
    /// <inheritdoc />
    public Guid Id => new("246A6D93-4F1A-1F8A-344D-50187A5513A9");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = null;
        ErrorNumber errno = imagePlugin.ReadSector(sectorOffset, out byte[] sector);

        if(errno         != ErrorNumber.NoError ||
           sector.Length < 512)
            return false;

        Table table = Marshal.ByteArrayToStructureLittleEndian<Table>(sector);

        if(table.magic != KARMA_MAGIC)
            return false;

        ulong counter = 0;

        partitions = (from entry in table.entries let part = new Partition
                         {
                             Start    = entry.offset,
                             Offset   = (ulong)(entry.offset * sector.Length),
                             Size     = entry.size,
                             Length   = (ulong)(entry.size * sector.Length),
                             Type     = Localization.Rio_Karma,
                             Sequence = counter++,
                             Scheme   = Name
                         } where entry.type == ENTRY_MAGIC select part).ToList();

        return true;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Table
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 270)]
        public readonly byte[] reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly Entry[] entries;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 208)]
        public readonly byte[] padding;
        public readonly ushort magic;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Entry
    {
        public readonly uint reserved;
        public readonly byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] reserved2;
        public readonly uint offset;
        public readonly uint size;
    }
}