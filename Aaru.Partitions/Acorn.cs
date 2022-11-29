// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Acorn.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Acorn FileCore partitions.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of Acorn partitions</summary>
public sealed class Acorn : IPartition
{
    const ulong ADFS_SB_POS      = 0xC00;
    const uint  LINUX_MAGIC      = 0xDEAFA1DE;
    const uint  SWAP_MAGIC       = 0xDEAFAB1E;
    const uint  RISCIX_MAGIC     = 0x4A657320;
    const uint  TYPE_LINUX       = 9;
    const uint  TYPE_RISCIX_MFM  = 1;
    const uint  TYPE_RISCIX_SCSI = 2;
    const uint  TYPE_MASK        = 15;

    /// <inheritdoc />
    public string Name => Localization.Acorn_Name;
    /// <inheritdoc />
    public Guid Id => new("A7C8FEBE-8D00-4933-B9F3-42184C8BA808");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = new List<Partition>();

        ulong sbSector;

        // RISC OS always checks for the partition on 0. Afaik no emulator chains it.
        if(sectorOffset != 0)
            return false;

        if(imagePlugin.Info.SectorSize > ADFS_SB_POS)
            sbSector = 0;
        else
            sbSector = ADFS_SB_POS / imagePlugin.Info.SectorSize;

        ErrorNumber errno = imagePlugin.ReadSector(sbSector, out byte[] sector);

        if(errno         != ErrorNumber.NoError ||
           sector.Length < 512)
            return false;

        AcornBootBlock bootBlock = Marshal.ByteArrayToStructureLittleEndian<AcornBootBlock>(sector);

        int checksum = 0;

        for(int i = 0; i < 0x1FF; i++)
            checksum = (checksum & 0xFF) + (checksum >> 8) + sector[i];

        int heads     = bootBlock.discRecord.heads + ((bootBlock.discRecord.lowsector >> 6) & 1);
        int secCyl    = bootBlock.discRecord.spt * heads;
        int mapSector = bootBlock.startCylinder  * secCyl;

        if((ulong)mapSector >= imagePlugin.Info.Sectors)
            return false;

        errno = imagePlugin.ReadSector((ulong)mapSector, out byte[] map);

        if(errno != ErrorNumber.NoError)
            return false;

        ulong counter = 0;

        if(checksum == bootBlock.checksum)
        {
            var part = new Partition
            {
                Size = ((ulong)bootBlock.discRecord.disc_size_high * 0x100000000) + bootBlock.discRecord.disc_size,
                Length = (((ulong)bootBlock.discRecord.disc_size_high * 0x100000000) + bootBlock.discRecord.disc_size) /
                         imagePlugin.Info.SectorSize,
                Type = "ADFS",
                Name = StringHandlers.CToString(bootBlock.discRecord.disc_name, Encoding.GetEncoding("iso-8859-1"))
            };

            if(part.Size > 0)
            {
                partitions.Add(part);
                counter++;
            }
        }

        switch(bootBlock.flags & TYPE_MASK)
        {
            case TYPE_LINUX:
            {
                LinuxTable table = Marshal.ByteArrayToStructureLittleEndian<LinuxTable>(map);

                foreach(LinuxEntry entry in table.entries)
                {
                    var part = new Partition
                    {
                        Start    = (ulong)(mapSector + entry.start),
                        Size     = entry.size,
                        Length   = (ulong)(entry.size * sector.Length),
                        Sequence = counter,
                        Scheme   = Name
                    };

                    part.Offset = part.Start * (ulong)sector.Length;

                    if(entry.magic != LINUX_MAGIC &&
                       entry.magic != SWAP_MAGIC)
                        continue;

                    partitions.Add(part);
                    counter++;
                }

                break;
            }
            case TYPE_RISCIX_MFM:
            case TYPE_RISCIX_SCSI:
            {
                RiscIxTable table = Marshal.ByteArrayToStructureLittleEndian<RiscIxTable>(map);

                if(table.magic == RISCIX_MAGIC)
                    foreach(RiscIxEntry entry in table.partitions)
                    {
                        var part = new Partition
                        {
                            Start    = (ulong)(mapSector + entry.start),
                            Size     = entry.length,
                            Length   = (ulong)(entry.length * sector.Length),
                            Name     = StringHandlers.CToString(entry.name, Encoding.GetEncoding("iso-8859-1")),
                            Sequence = counter,
                            Scheme   = Name
                        };

                        part.Offset = part.Start * (ulong)sector.Length;

                        if(entry.length <= 0)
                            continue;

                        partitions.Add(part);
                        counter++;
                    }

                break;
            }
        }

        return partitions.Count != 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DiscRecord
    {
        public readonly byte   log2secsize;
        public readonly byte   spt;
        public readonly byte   heads;
        public readonly byte   density;
        public readonly byte   idlen;
        public readonly byte   log2bpmb;
        public readonly byte   skew;
        public readonly byte   bootoption;
        public readonly byte   lowsector;
        public readonly byte   nzones;
        public readonly ushort zone_spare;
        public readonly uint   root;
        public readonly uint   disc_size;
        public readonly ushort disc_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] disc_name;
        public readonly uint disc_type;
        public readonly uint disc_size_high;
        public readonly byte flags;
        public readonly byte nzones_high;
        public readonly uint format_version;
        public readonly uint root_size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AcornBootBlock
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1C0)]
        public readonly byte[] spare;
        public readonly DiscRecord discRecord;
        public readonly byte       flags;
        public readonly ushort     startCylinder;
        public readonly byte       checksum;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LinuxTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
        public readonly LinuxEntry[] entries;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LinuxEntry
    {
        public readonly uint magic;
        public readonly uint start;
        public readonly uint size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RiscIxTable
    {
        public readonly uint magic;
        public readonly uint date;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly RiscIxEntry[] partitions;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RiscIxEntry
    {
        public readonly uint start;
        public readonly uint length;
        public readonly uint one;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] name;
    }
}