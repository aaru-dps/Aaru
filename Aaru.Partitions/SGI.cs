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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Attributes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

#pragma warning disable 169
#pragma warning disable 649

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of the SGI Disk Volume Header</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names match the original IRIX headers")]
public sealed partial class SGI : IPartition
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
        partitions = [];

        ErrorNumber errno = imagePlugin.ReadSector(sectorOffset, false, out byte[] sector, out _);

        if(errno != ErrorNumber.NoError || sector.Length < 512) return false;

        Label dvh = Marshal.ByteArrayToStructureBigEndian<Label>(sector);

        AaruLogging.Debug(MODULE_NAME, Localization.dvh_magic_equals_0_X8_should_be_1_X8, dvh.magic, SGI_MAGIC);

        if(dvh.magic != SGI_MAGIC) return false;

        AaruLogging.Debug(MODULE_NAME, "dvh.root_part_num = {0}", dvh.root_part_num);
        AaruLogging.Debug(MODULE_NAME, "dvh.swap_part_num = {0}", dvh.swap_part_num);

        AaruLogging.Debug(MODULE_NAME, "dvh.boot_file = \"{0}\"", StringHandlers.CToString(dvh.boot_file));

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_skew = {0}", dvh.device_params.dp_skew);
        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_gap1 = {0}", dvh.device_params.dp_gap1);
        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_gap2 = {0}", dvh.device_params.dp_gap2);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_spares_cyl = {0}", dvh.device_params.dp_spares_cyl);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_cyls = {0}",  dvh.device_params.dp_cyls);
        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_shd0 = {0}",  dvh.device_params.dp_shd0);
        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_trks0 = {0}", dvh.device_params.dp_trks0);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_ctq_depth = {0}", dvh.device_params.dp_ctq_depth);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_cylshi = {0}", dvh.device_params.dp_cylshi);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_secs = {0}", dvh.device_params.dp_secs);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_secbytes = {0}", dvh.device_params.dp_secbytes);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_interleave = {0}", dvh.device_params.dp_interleave);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_flags = {0}", dvh.device_params.dp_flags);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_datarate = {0}", dvh.device_params.dp_datarate);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_nretries = {0}", dvh.device_params.dp_nretries);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_mspw = {0}",  dvh.device_params.dp_mspw);
        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_xgap1 = {0}", dvh.device_params.dp_xgap1);
        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_xsync = {0}", dvh.device_params.dp_xsync);
        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_xrdly = {0}", dvh.device_params.dp_xrdly);
        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_xgap2 = {0}", dvh.device_params.dp_xgap2);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_xrgate = {0}", dvh.device_params.dp_xrgate);

        AaruLogging.Debug(MODULE_NAME, "dvh.device_params.dp_xwcont = {0}", dvh.device_params.dp_xwcont);

        ulong counter = 0;

        for(var i = 0; i < dvh.partitions.Length; i++)
        {
            AaruLogging.Debug(MODULE_NAME, "dvh.partitions[{0}].num_blocks = {1}", i, dvh.partitions[i].num_blocks);

            AaruLogging.Debug(MODULE_NAME, "dvh.partitions[{0}].first_block = {1}", i, dvh.partitions[i].first_block);

            AaruLogging.Debug(MODULE_NAME, "dvh.partitions[{0}].type = {1}", i, dvh.partitions[i].type);

            var part = new CommonTypes.Partition
            {
                Start =
                    (ulong)dvh.partitions[i].first_block * dvh.device_params.dp_secbytes / imagePlugin.Info.SectorSize +
                    sectorOffset,
                Offset =
                    (ulong)dvh.partitions[i].first_block * dvh.device_params.dp_secbytes +
                    sectorOffset                         * imagePlugin.Info.SectorSize,
                Length =
                    (ulong)dvh.partitions[i].num_blocks * dvh.device_params.dp_secbytes / imagePlugin.Info.SectorSize,
                Size     = (ulong)dvh.partitions[i].num_blocks * dvh.device_params.dp_secbytes,
                Type     = TypeToString(dvh.partitions[i].type),
                Sequence = counter,
                Scheme   = Name
            };

            if(part.Size <= 0 || dvh.partitions[i].type is SGIType.Header or SGIType.Volume) continue;

            partitions.Add(part);
            counter++;
        }

        return partitions.Count > 0;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct DeviceParameters
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
    [SwapEndian]
    partial struct Label
    {
        public uint  magic;
        public short root_part_num;
        public short swap_part_num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] boot_file;
        public DeviceParameters device_params;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public Volume[] volume;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Partition[] partitions;
        public uint csum;
        public uint padding;
    }

#endregion

#region Nested type: Partition

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Partition
    {
        public uint    num_blocks;
        public uint    first_block;
        public SGIType type;
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
    [SwapEndian]
    partial struct Volume
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] name;
        public uint block_num;
        public uint num_bytes;
    }

#endregion
}