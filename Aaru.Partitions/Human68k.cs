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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Attributes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of Sharp's Human68K partitions</summary>
public sealed partial class Human68K : IPartition
{
    // ReSharper disable once InconsistentNaming
    const uint   X68K_MAGIC  = 0x5836384B;
    const string MODULE_NAME = "Human68k partitions plugin";

#region Nested type: Entry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Entry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] name;
        public uint stateStart;
        public uint length;
    }

#endregion

#region Nested type: Table

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Table
    {
        public uint magic;
        public uint size;
        public uint size2;
        public uint unknown;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public Entry[] entries;
    }

#endregion

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.Human68K_Name;

    /// <inheritdoc />
    public Guid Id => new("246A6D93-4F1A-1F8A-344D-50187A5513A9");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = [];

        byte[]      sector;
        ulong       sectsPerUnit;
        ErrorNumber errno;

        AaruLogging.Debug(MODULE_NAME, "sectorSize = {0}", imagePlugin.Info.SectorSize);

        if(sectorOffset + 4 >= imagePlugin.Info.Sectors) return false;

        switch(imagePlugin.Info.SectorSize)
        {
            case 256:
                // The table lives at byte offset 2048 and the partition unit is 1024 bytes
                errno        = imagePlugin.ReadSector(8 + sectorOffset, false, out sector, out _);
                sectsPerUnit = 4;

                break;
            case 512:
                errno        = imagePlugin.ReadSector(4 + sectorOffset, false, out sector, out _);
                sectsPerUnit = 2;

                break;
            case 1024:
                errno        = imagePlugin.ReadSector(2 + sectorOffset, false, out sector, out _);
                sectsPerUnit = 1;

                break;
            default:
                return false;
        }

        if(errno != ErrorNumber.NoError) return false;

        Table table = Marshal.ByteArrayToStructureBigEndian<Table>(sector);

        AaruLogging.Debug(MODULE_NAME, "table.magic = {0:X4}", table.magic);

        if(table.magic != X68K_MAGIC) return false;

        AaruLogging.Debug(MODULE_NAME, "table.size = {0:X4}",    table.size);
        AaruLogging.Debug(MODULE_NAME, "table.size2 = {0:X4}",   table.size2);
        AaruLogging.Debug(MODULE_NAME, "table.unknown = {0:X4}", table.unknown);

        ulong counter = 0;

        foreach(Entry entry in table.entries)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "entry.name = {0}",
                              StringHandlers.CToString(entry.name, Encoding.GetEncoding(932)));

            AaruLogging.Debug(MODULE_NAME, "entry.stateStart = {0}", entry.stateStart);
            AaruLogging.Debug(MODULE_NAME, "entry.length = {0}",     entry.length);

            AaruLogging.Debug(MODULE_NAME, "sectsPerUnit = {0} {1}", sectsPerUnit, imagePlugin.Info.SectorSize);

            var part = new Partition
            {
                Start    = (entry.stateStart & 0xFFFFFF) * sectsPerUnit + sectorOffset,
                Length   = entry.length                  * sectsPerUnit,
                Type     = StringHandlers.CToString(entry.name, Encoding.GetEncoding(932)),
                Sequence = counter,
                Scheme   = Name
            };

            part.Offset = part.Start  * imagePlugin.Info.SectorSize;
            part.Size   = part.Length * imagePlugin.Info.SectorSize;

            if(entry.length == 0) continue;

            if(part.Start >= imagePlugin.Info.Sectors || part.End >= imagePlugin.Info.Sectors) continue;

            partitions.Add(part);
            counter++;
        }

        return partitions.Count > 0;
    }

#endregion
}