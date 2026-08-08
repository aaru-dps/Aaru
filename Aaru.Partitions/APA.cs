// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : APA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages PlayStation 2 APA partitions.
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of PlayStation 2 APA partitions</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Constants document the full on-disk layout")]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "APA is the scheme's name")]
public sealed class APA : IPartition
{
    const string MODULE_NAME = "APA partitioning plugin";

    const uint   APA_MAGIC          = 0x00415041; // 'APA\0'
    const int    APA_HEADER_SIZE    = 1024;       // 2 sectors of 512 bytes
    const int    APA_IDMAX          = 32;
    const int    APA_PASSMAX        = 8;
    const int    APA_MAXSUB         = 64;
    const int    APA_MBR_MAGIC_LEN  = 32;
    const int    APA_NNAME          = 128;
    const uint   APA_RESV_MAIN      = 4 * 1024 * 1024 / 512; // 8192 sectors (4 MiB)
    const uint   APA_RESV_SUB       = 4 * 1024        / 512; // 8 sectors (4 KiB)
    const ushort APA_TYPE_FREE      = 0x0000;
    const ushort APA_TYPE_MBR       = 0x0001;
    const ushort APA_TYPE_EXT2_SWAP = 0x0082;
    const ushort APA_TYPE_EXT2      = 0x0083;
    const ushort APA_TYPE_REISER    = 0x0088;
    const ushort APA_TYPE_PFS       = 0x0100;
    const ushort APA_TYPE_CFS       = 0x0101;
    const ushort APA_TYPE_HDL       = 0x1337;
    const ushort APA_FLAG_SUB       = 0x0001;

    const string MBR_MAGIC = "Sony Computer Entertainment Inc.";

#region Nested type: ApaPs2Time

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ApaPs2Time
    {
        public readonly byte   unused;
        public readonly byte   sec;
        public readonly byte   min;
        public readonly byte   hour;
        public readonly byte   day;
        public readonly byte   month;
        public readonly ushort year;
    }

#endregion

#region Nested type: ApaSub

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ApaSub
    {
        public readonly uint start;
        public readonly uint length;
    }

#endregion

#region Nested type: ApaMbr

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ApaMbr
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = APA_MBR_MAGIC_LEN)]
        public readonly byte[] magic;
        public readonly uint       version;
        public readonly uint       nsector;
        public readonly ApaPs2Time created;
        public readonly uint       osdStart;
        public readonly uint       osdSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public readonly byte[] padding3;
    }

#endregion

#region Nested type: ApaHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ApaHeader
    {
        public readonly uint checksum;
        public readonly uint magic;
        public readonly uint next;
        public readonly uint prev;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = APA_IDMAX)]
        public readonly byte[] id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = APA_PASSMAX)]
        public readonly byte[] rpwd;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = APA_PASSMAX)]
        public readonly byte[] fpwd;
        public readonly uint       start;
        public readonly uint       length;
        public readonly ushort     type;
        public readonly ushort     flags;
        public readonly uint       nsub;
        public readonly ApaPs2Time created;
        public readonly uint       main;
        public readonly uint       number;
        public readonly uint       modver;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public readonly uint[] padding1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = APA_NNAME)]
        public readonly byte[] name;
        public readonly ApaMbr mbr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = APA_MAXSUB)]
        public readonly ApaSub[] subs;
    }

#endregion

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.APA_Name;

    /// <inheritdoc />
    public Guid Id => new("B6A8B66B-B194-4E14-802F-B684D4B59B2F");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = [];

        // APA always starts at sector 0
        if(sectorOffset != 0) return false;

        // Need at least 2 sectors to read the MBR header
        if(imagePlugin.Info.Sectors < 2) return false;

        // APA uses 512-byte sectors
        if(imagePlugin.Info.SectorSize != 512) return false;

        // Read the MBR header (2 sectors = 1024 bytes)
        ErrorNumber errno = imagePlugin.ReadSectors(0, false, 2, out byte[] sector, out _);

        if(errno != ErrorNumber.NoError || sector.Length < APA_HEADER_SIZE) return false;

        ApaHeader mbrHeader = Marshal.ByteArrayToStructureLittleEndian<ApaHeader>(sector);

        // Check APA magic
        if(mbrHeader.magic != APA_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME, Localization.APA_magic_not_found);

            return false;
        }

        // Verify checksum
        if(!VerifyChecksum(sector))
        {
            AaruLogging.Debug(MODULE_NAME, Localization.APA_invalid_checksum);

            return false;
        }

        // Verify MBR magic string
        string mbrMagicString = Encoding.ASCII.GetString(mbrHeader.mbr.magic, 0, APA_MBR_MAGIC_LEN).TrimEnd('\0');

        if(mbrMagicString != MBR_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME, Localization.APA_MBR_magic_not_found);

            return false;
        }

        AaruLogging.Debug(MODULE_NAME, Localization.APA_MBR_version_0, mbrHeader.mbr.version);

        AaruLogging.Debug(MODULE_NAME, Localization.APA_total_sectors_0, mbrHeader.mbr.nsector);

        // Traverse the linked list of partitions
        ulong sequence = 0;
        uint  nextLba  = mbrHeader.next;
        var   visited  = new HashSet<uint>();

        while(nextLba != 0)
        {
            // Prevent infinite loops from circular references
            if(!visited.Add(nextLba))
            {
                AaruLogging.Debug(MODULE_NAME, Localization.APA_circular_reference_at_sector_0, nextLba);

                break;
            }

            // Sanity check
            if((ulong)nextLba + 2 > imagePlugin.Info.Sectors) break;

            errno = imagePlugin.ReadSectors(nextLba, false, 2, out byte[] headerBytes, out _);

            if(errno != ErrorNumber.NoError) break;

            if(headerBytes.Length < APA_HEADER_SIZE) break;

            ApaHeader header = Marshal.ByteArrayToStructureLittleEndian<ApaHeader>(headerBytes);

            if(header.magic != APA_MAGIC) break;

            if(!VerifyChecksum(headerBytes))
            {
                AaruLogging.Debug(MODULE_NAME, Localization.APA_invalid_checksum_at_sector_0, nextLba);

                break;
            }

            // Skip free and MBR partitions
            if(header.type != APA_TYPE_FREE && header.type != APA_TYPE_MBR && header.length > APA_RESV_MAIN)
            {
                string partId   = Encoding.ASCII.GetString(header.id,   0, APA_IDMAX).TrimEnd('\0');
                string partName = Encoding.ASCII.GetString(header.name, 0, APA_NNAME).TrimEnd('\0');
                string typeName = TypeToString(header.type);

                if(string.IsNullOrEmpty(partName) && !string.IsNullOrEmpty(partId)) partName = partId;

                // Reserves 4 MiB (8192 sectors) at the start of main partitions,
                // and 4 KiB (8 sectors) for sub-partitions.
                ulong dataStart  = (ulong)header.start + APA_RESV_MAIN;
                uint  dataLength = header.length       - APA_RESV_MAIN;

                var part = new Partition
                {
                    Start    = dataStart,
                    Offset   = dataStart * imagePlugin.Info.SectorSize,
                    Length   = dataLength,
                    Size     = (ulong)dataLength * imagePlugin.Info.SectorSize,
                    Type     = typeName,
                    Name     = partName,
                    Sequence = sequence,
                    Scheme   = Name
                };

                partitions.Add(part);
                sequence++;

                // Add sub-partitions
                for(uint i = 0; i < header.nsub && i < APA_MAXSUB; i++)
                {
                    if(header.subs[i].length <= APA_RESV_SUB) continue;

                    ulong subDataStart  = (ulong)header.subs[i].start + APA_RESV_SUB;
                    uint  subDataLength = header.subs[i].length       - APA_RESV_SUB;

                    var sub = new Partition
                    {
                        Start    = subDataStart,
                        Offset   = subDataStart * imagePlugin.Info.SectorSize,
                        Length   = subDataLength,
                        Size     = (ulong)subDataLength * imagePlugin.Info.SectorSize,
                        Type     = typeName,
                        Name     = string.Format(Localization.APA_sub_partition_0_of_1, i, partId),
                        Sequence = sequence,
                        Scheme   = Name
                    };

                    partitions.Add(sub);
                    sequence++;
                }
            }

            nextLba = header.next;
        }

        return partitions.Count > 0;
    }

#endregion

    static bool VerifyChecksum(byte[] headerBytes)
    {
        // Checksum is at offset 0 (first u32)
        // Sum all u32 words from offset 4 to end of header (words 1-255)
        var  storedChecksum = BitConverter.ToUInt32(headerBytes, 0);
        uint calculatedSum  = 0;

        for(var i = 1; i < 256; i++) calculatedSum += BitConverter.ToUInt32(headerBytes, i * 4);

        return storedChecksum == calculatedSum;
    }

    static string TypeToString(ushort type) => type switch
                                               {
                                                   APA_TYPE_MBR => Localization.APA_type_MBR,
                                                   APA_TYPE_EXT2_SWAP => Localization.APA_type_EXT2_swap,
                                                   APA_TYPE_EXT2 => Localization.APA_type_EXT2,
                                                   APA_TYPE_REISER => Localization.APA_type_ReiserFS,
                                                   APA_TYPE_PFS => Localization.APA_type_PFS,
                                                   APA_TYPE_CFS => Localization.APA_type_CFS,
                                                   APA_TYPE_HDL => Localization.APA_type_HDLoader,
                                                   _ => string.Format(Localization.APA_type_unknown_0, type)
                                               };
}