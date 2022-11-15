// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of DEC disklabels</summary>
public sealed class DEC : IPartition
{
    const int PT_MAGIC = 0x032957;
    const int PT_VALID = 1;

    /// <inheritdoc />
    public string Name => "DEC disklabel";
    /// <inheritdoc />
    public Guid Id => new("58CEC3B7-3B93-4D47-86EE-D6DADE9D444F");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<CommonTypes.Partition> partitions, ulong sectorOffset)
    {
        partitions = new List<CommonTypes.Partition>();

        if(31 + sectorOffset >= imagePlugin.Info.Sectors)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(31 + sectorOffset, out byte[] sector);

        if(errno         != ErrorNumber.NoError ||
           sector.Length < 512)
            return false;

        Label table = Marshal.ByteArrayToStructureLittleEndian<Label>(sector);

        if(table.pt_magic != PT_MAGIC ||
           table.pt_valid != PT_VALID)
            return false;

        ulong counter = 0;

        foreach(CommonTypes.Partition part in table.pt_part.Select(entry => new CommonTypes.Partition
                {
                    Start    = entry.pi_blkoff,
                    Offset   = (ulong)(entry.pi_blkoff * sector.Length),
                    Size     = (ulong)entry.pi_nblocks,
                    Length   = (ulong)(entry.pi_nblocks * sector.Length),
                    Sequence = counter,
                    Scheme   = Name
                }).Where(part => part.Size > 0))
        {
            partitions.Add(part);
            counter++;
        }

        return true;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Label
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 440)]
        public readonly byte[] padding;
        public readonly int pt_magic;
        public readonly int pt_valid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly Partition[] pt_part;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Partition
    {
        public readonly int  pi_nblocks;
        public readonly uint pi_blkoff;
    }
}