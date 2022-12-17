// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft exFAT filesystem plugin.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from https://www.sans.org/reading-room/whitepapers/forensics/reverse-engineering-microsoft-exfat-file-system-33274
/// <inheritdoc />
/// <summary>Implements detection of the exFAT filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed partial class exFAT
{
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
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        var sb = new StringBuilder();
        metadata = new FileSystem();

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

        metadata.ClusterSize  = (uint)((1 << vbr.sectorShift) * (1 << vbr.clusterShift));
        metadata.Clusters     = vbr.clusterHeapLength;
        metadata.Dirty        = vbr.flags.HasFlag(VolumeFlags.VolumeDirty);
        metadata.Type         = FS_TYPE;
        metadata.VolumeSerial = $"{vbr.volumeSerial:X8}";

        information = sb.ToString();
    }
}