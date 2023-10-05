// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SGI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages SGI DVHs (Disk Volume Headers).
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

#pragma warning disable 169
#pragma warning disable 649

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of the SGI Disk Volume Header</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class SGI : IPartition
{
    const int    SGI_MAGIC   = 0x0BE5A941;
    const string MODULE_NAME = "SGI Volume Header plugin";

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.SGI_Name;

    /// <inheritdoc />
    public Guid Id => new("AEF5AB45-4880-4CE8-8735-F0A402E2E5F2");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<CommonTypes.Partition> partitions, ulong sectorOffset)
    {
        partitions = new List<CommonTypes.Partition>();

        ErrorNumber errno = imagePlugin.ReadSector(sectorOffset, out byte[] sector);

        if(errno != ErrorNumber.NoError || sector.Length < 512)
            return false;

        Label dvh = Marshal.ByteArrayToStructureBigEndian<Label>(sector);

        for(var i = 0; i < dvh.volume.Length; i++)
            dvh.volume[i] = (Volume)Marshal.SwapStructureMembersEndian(dvh.volume[i]);

        for(var i = 0; i < dvh.partitions.Length; i++)
            dvh.partitions[i] = (Partition)Marshal.SwapStructureMembersEndian(dvh.partitions[i]);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.dvh_magic_equals_0_X8_should_be_1_X8, dvh.magic,
                                   SGI_MAGIC);

        if(dvh.magic != SGI_MAGIC)
            return false;

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.root_part_num = {0}", dvh.root_part_num);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.swap_part_num = {0}", dvh.swap_part_num);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.boot_file = \"{0}\"", StringHandlers.CToString(dvh.boot_file));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_skew = {0}", dvh.device_params.dp_skew);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_gap1 = {0}", dvh.device_params.dp_gap1);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_gap2 = {0}", dvh.device_params.dp_gap2);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_spares_cyl = {0}",
                                   dvh.device_params.dp_spares_cyl);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_cyls = {0}",  dvh.device_params.dp_cyls);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_shd0 = {0}",  dvh.device_params.dp_shd0);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_trks0 = {0}", dvh.device_params.dp_trks0);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_ctq_depth = {0}", dvh.device_params.dp_ctq_depth);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_cylshi = {0}", dvh.device_params.dp_cylshi);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_secs = {0}", dvh.device_params.dp_secs);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_secbytes = {0}", dvh.device_params.dp_secbytes);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_interleave = {0}",
                                   dvh.device_params.dp_interleave);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_flags = {0}", dvh.device_params.dp_flags);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_datarate = {0}", dvh.device_params.dp_datarate);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_nretries = {0}", dvh.device_params.dp_nretries);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_mspw = {0}",  dvh.device_params.dp_mspw);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_xgap1 = {0}", dvh.device_params.dp_xgap1);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_xsync = {0}", dvh.device_params.dp_xsync);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_xrdly = {0}", dvh.device_params.dp_xrdly);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_xgap2 = {0}", dvh.device_params.dp_xgap2);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_xrgate = {0}", dvh.device_params.dp_xrgate);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.device_params.dp_xwcont = {0}", dvh.device_params.dp_xwcont);

        ulong counter = 0;

        for(var i = 0; i < dvh.partitions.Length; i++)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.partitions[{0}].num_blocks = {1}", i,
                                       dvh.partitions[i].num_blocks);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.partitions[{0}].first_block = {1}", i,
                                       dvh.partitions[i].first_block);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dvh.partitions[{0}].type = {1}", i, dvh.partitions[i].type);

            var part = new CommonTypes.Partition
            {
                Start    = dvh.partitions[i].first_block * dvh.device_params.dp_secbytes / imagePlugin.Info.SectorSize,
                Offset   = dvh.partitions[i].first_block * dvh.device_params.dp_secbytes,
                Length   = dvh.partitions[i].num_blocks * dvh.device_params.dp_secbytes / imagePlugin.Info.SectorSize,
                Size     = dvh.partitions[i].num_blocks * dvh.device_params.dp_secbytes,
                Type     = TypeToString(dvh.partitions[i].type),
                Sequence = counter,
                Scheme   = Name
            };

            if(part.Size <= 0 || dvh.partitions[i].type is SGIType.Header or SGIType.Volume)
                continue;

            partitions.Add(part);
            counter++;
        }

        return true;
    }

#endregion

    static string TypeToString(SGIType typ) => typ switch
                                               {
                                                   SGIType.Header    => Localization.Volume_header,
                                                   SGIType.TrkRepl   => Localization.Track_replacements,
                                                   SGIType.SecRepl   => Localization.Sector_replacements,
                                                   SGIType.Swap      => Localization.Raw_data_swap,
                                                   SGIType.Bsd       => Localization._4_2_BSD_Fast_File_System,
                                                   SGIType.SystemV   => Localization.UNIX_System_V,
                                                   SGIType.Volume    => Localization.Whole_device,
                                                   SGIType.EFS       => Localization.EFS,
                                                   SGIType.Lvol      => Localization.Logical_volume,
                                                   SGIType.Rlvol     => Localization.Raw_logical_volume,
                                                   SGIType.XFS       => Localization.XFS,
                                                   SGIType.Xlvol     => Localization.XFS_log_device,
                                                   SGIType.Rxlvol    => Localization.XLV_volume,
                                                   SGIType.Xvm       => Localization.SGI_XVM,
                                                   SGIType.LinuxSwap => Localization.Linux_swap,
                                                   SGIType.Linux     => Localization.Linux,
                                                   SGIType.LinuxRAID => Localization.Linux_RAID,
                                                   _                 => Localization.Unknown_partition_type
                                               };

#region Nested type: DeviceParameters

    struct DeviceParameters
    {
        public byte   dp_skew;
        public byte   dp_gap1;
        public byte   dp_gap2;
        public byte   dp_spares_cyl;
        public ushort dp_cyls;
        public ushort dp_shd0;
        public ushort dp_trks0;
        public byte   dp_ctq_depth;
        public byte   dp_cylshi;
        public ushort dp_unused;
        public ushort dp_secs;
        public ushort dp_secbytes;
        public ushort dp_interleave;
        public uint   dp_flags;
        public uint   dp_datarate;
        public uint   dp_nretries;
        public uint   dp_mspw;
        public ushort dp_xgap1;
        public ushort dp_xsync;
        public ushort dp_xrdly;
        public ushort dp_xgap2;
        public ushort dp_xrgate;
        public ushort dp_xwcont;
    }

#endregion

#region Nested type: Label

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Label
    {
        /// <summary></summary>
        public readonly uint magic;
        /// <summary></summary>
        public readonly short root_part_num;
        /// <summary></summary>
        public readonly short swap_part_num;
        /// <summary></summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] boot_file;
        /// <summary></summary>
        public readonly DeviceParameters device_params;
        /// <summary></summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public readonly Volume[] volume;
        /// <summary></summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly Partition[] partitions;
        /// <summary></summary>
        public readonly uint csum;
        /// <summary></summary>
        public readonly uint padding;
    }

#endregion

#region Nested type: Partition

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Partition
    {
        /// <summary></summary>
        public readonly uint num_blocks;
        /// <summary></summary>
        public readonly uint first_block;
        /// <summary></summary>
        public readonly SGIType type;
    }

#endregion

#region Nested type: SGIType

    enum SGIType : uint
    {
        Header    = 0,
        TrkRepl   = 1,
        SecRepl   = 2,
        Swap      = 3,
        Bsd       = 4,
        SystemV   = 5,
        Volume    = 6,
        EFS       = 7,
        Lvol      = 8,
        Rlvol     = 9,
        XFS       = 0xA,
        Xlvol     = 0xB,
        Rxlvol    = 0xC,
        Xvm       = 0x0D,
        LinuxSwap = 0x82,
        Linux     = 0x83,
        LinuxRAID = 0xFD
    }

#endregion

#region Nested type: Volume

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Volume
    {
        /// <summary></summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] name;
        /// <summary></summary>
        public readonly uint block_num;
        /// <summary></summary>
        public readonly uint num_bytes;
    }

#endregion
}