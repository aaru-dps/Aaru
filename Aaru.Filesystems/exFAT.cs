// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : exFAT.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft exFAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Microsoft exFAT filesystem and shows information.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

// Information from https://www.sans.org/reading-room/whitepapers/forensics/reverse-engineering-microsoft-exfat-file-system-33274
/// <inheritdoc />
/// <summary>Implements detection of the exFAT filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed class exFAT : IFilesystem
{
    readonly Guid _oemFlashParameterGuid = new("0A0C7E46-3399-4021-90C8-FA6D389C4BA2");

    readonly byte[] _signature =
    {
        0x45, 0x58, 0x46, 0x41, 0x54, 0x20, 0x20, 0x20
    };

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.exFAT_Name;
    /// <inheritdoc />
    public Guid Id => new("8271D088-1533-4CB3-AC28-D802B68BB95C");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(12 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] vbrSector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(vbrSector.Length < 512)
            return false;

        VolumeBootRecord vbr = Marshal.ByteArrayToStructureLittleEndian<VolumeBootRecord>(vbrSector);

        return _signature.SequenceEqual(vbr.signature);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        var sb = new StringBuilder();
        XmlFsType = new FileSystemType();

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] vbrSector);

        if(errno != ErrorNumber.NoError)
            return;

        VolumeBootRecord vbr = Marshal.ByteArrayToStructureLittleEndian<VolumeBootRecord>(vbrSector);

        errno = imagePlugin.ReadSector(9 + partition.Start, out byte[] parametersSector);

        if(errno != ErrorNumber.NoError)
            return;

        OemParameterTable parametersTable =
            Marshal.ByteArrayToStructureLittleEndian<OemParameterTable>(parametersSector);

        errno = imagePlugin.ReadSector(11 + partition.Start, out byte[] chkSector);

        if(errno != ErrorNumber.NoError)
            return;

        ChecksumSector chksector = Marshal.ByteArrayToStructureLittleEndian<ChecksumSector>(chkSector);

        sb.AppendLine(Localization.Microsoft_exFAT);
        sb.AppendFormat(Localization.Partition_offset_0, vbr.offset).AppendLine();

        sb.AppendFormat(Localization.Volume_has_0_sectors_of_1_bytes_each_for_a_total_of_2_bytes, vbr.sectors,
                        1 << vbr.sectorShift, vbr.sectors * (ulong)(1 << vbr.sectorShift)).AppendLine();

        sb.AppendFormat(Localization.Volume_uses_clusters_of_0_sectors_1_bytes_each, 1 << vbr.clusterShift,
                        (1 << vbr.sectorShift) * (1 << vbr.clusterShift)).AppendLine();

        sb.AppendFormat(Localization.First_FAT_starts_at_sector_0_and_runs_for_1_sectors, vbr.fatOffset, vbr.fatLength).
           AppendLine();

        sb.AppendFormat(Localization.Volume_uses_0_FATs, vbr.fats).AppendLine();

        sb.AppendFormat(Localization.Cluster_heap_starts_at_sector_0_contains_1_clusters_and_is_2_used,
                        vbr.clusterHeapOffset, vbr.clusterHeapLength, vbr.heapUsage).AppendLine();

        sb.AppendFormat(Localization.Root_directory_starts_at_cluster_0, vbr.rootDirectoryCluster).AppendLine();

        sb.AppendFormat(Localization.Filesystem_revision_is_0_1, (vbr.revision & 0xFF00) >> 8, vbr.revision & 0xFF).
           AppendLine();

        sb.AppendFormat(Localization.Volume_serial_number_0_X8, vbr.volumeSerial).AppendLine();
        sb.AppendFormat(Localization.BIOS_drive_is_0, vbr.drive).AppendLine();

        if(vbr.flags.HasFlag(VolumeFlags.SecondFatActive))
            sb.AppendLine(Localization.Second_FAT_is_in_use);

        if(vbr.flags.HasFlag(VolumeFlags.VolumeDirty))
            sb.AppendLine(Localization.Volume_is_dirty);

        if(vbr.flags.HasFlag(VolumeFlags.MediaFailure))
            sb.AppendLine(Localization.Underlying_media_presented_errors);

        int count = 1;

        foreach(OemParameter parameter in parametersTable.parameters)
        {
            if(parameter.OemParameterType == _oemFlashParameterGuid)
            {
                sb.AppendFormat(Localization.OEM_Parameters_0, count).AppendLine();
                sb.AppendFormat("\t" + Localization._0_bytes_in_erase_block, parameter.eraseBlockSize).AppendLine();
                sb.AppendFormat("\t" + Localization._0_bytes_per_page, parameter.pageSize).AppendLine();
                sb.AppendFormat("\t" + Localization._0_spare_blocks, parameter.spareBlocks).AppendLine();

                sb.AppendFormat("\t" + Localization._0_nanoseconds_random_access_time, parameter.randomAccessTime).
                   AppendLine();

                sb.AppendFormat("\t" + Localization._0_nanoseconds_program_time, parameter.programTime).AppendLine();

                sb.AppendFormat("\t" + Localization._0_nanoseconds_read_cycle_time, parameter.readCycleTime).
                   AppendLine();

                sb.AppendFormat("\t" + Localization._0_nanoseconds_write_cycle_time, parameter.writeCycleTime).
                   AppendLine();
            }
            else if(parameter.OemParameterType != Guid.Empty)
                sb.AppendFormat(Localization.Found_unknown_parameter_type_0, parameter.OemParameterType).AppendLine();

            count++;
        }

        sb.AppendFormat(Localization.Checksum_0_X8, chksector.checksum[0]).AppendLine();

        XmlFsType.ClusterSize  = (uint)((1 << vbr.sectorShift) * (1 << vbr.clusterShift));
        XmlFsType.Clusters     = vbr.clusterHeapLength;
        XmlFsType.Dirty        = vbr.flags.HasFlag(VolumeFlags.VolumeDirty);
        XmlFsType.Type         = FS_TYPE;
        XmlFsType.VolumeSerial = $"{vbr.volumeSerial:X8}";

        information = sb.ToString();
    }

    const string FS_TYPE = "exfat";

    [Flags]
    enum VolumeFlags : ushort
    {
        SecondFatActive = 1, VolumeDirty = 2, MediaFailure = 4,
        ClearToZero     = 8
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct VolumeBootRecord
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] signature;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)]
        public readonly byte[] zero;
        public readonly ulong       offset;
        public readonly ulong       sectors;
        public readonly uint        fatOffset;
        public readonly uint        fatLength;
        public readonly uint        clusterHeapOffset;
        public readonly uint        clusterHeapLength;
        public readonly uint        rootDirectoryCluster;
        public readonly uint        volumeSerial;
        public readonly ushort      revision;
        public readonly VolumeFlags flags;
        public readonly byte        sectorShift;
        public readonly byte        clusterShift;
        public readonly byte        fats;
        public readonly byte        drive;
        public readonly byte        heapUsage;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)]
        public readonly byte[] reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)]
        public readonly byte[] bootCode;
        public readonly ushort bootSignature;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct OemParameter
    {
        public readonly Guid OemParameterType;
        public readonly uint eraseBlockSize;
        public readonly uint pageSize;
        public readonly uint spareBlocks;
        public readonly uint randomAccessTime;
        public readonly uint programTime;
        public readonly uint readCycleTime;
        public readonly uint writeCycleTime;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct OemParameterTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly OemParameter[] parameters;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ChecksumSector
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly uint[] checksum;
    }
}