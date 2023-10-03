// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : IBM JFS filesystem plugin
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

// ReSharper disable UnusedMember.Local

using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of IBM's Journaled File System</summary>
public sealed partial class JFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        uint bootSectors = JFS_BOOT_BLOCKS_SIZE / imagePlugin.Info.SectorSize;

        if(partition.Start + bootSectors >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + bootSectors, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length < 512)
            return false;

        SuperBlock jfsSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

        return jfsSb.s_magic == JFS_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("iso-8859-15");
        information =   "";
        metadata    =   new FileSystem();
        var         sb          = new StringBuilder();
        uint        bootSectors = JFS_BOOT_BLOCKS_SIZE / imagePlugin.Info.SectorSize;
        ErrorNumber errno       = imagePlugin.ReadSector(partition.Start + bootSectors, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < 512)
            return;

        SuperBlock jfsSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

        sb.AppendLine(Localization.JFS_filesystem);
        sb.AppendFormat(Localization.Version_0,                      jfsSb.s_version).AppendLine();
        sb.AppendFormat(Localization._0_blocks_of_1_bytes,           jfsSb.s_size, jfsSb.s_bsize).AppendLine();
        sb.AppendFormat(Localization._0_blocks_per_allocation_group, jfsSb.s_agsize).AppendLine();

        if(jfsSb.s_flags.HasFlag(Flags.Unicode))
            sb.AppendLine(Localization.Volume_uses_Unicode_for_directory_entries);

        if(jfsSb.s_flags.HasFlag(Flags.RemountRO))
            sb.AppendLine(Localization.Volume_remounts_read_only_on_error);

        if(jfsSb.s_flags.HasFlag(Flags.Continue))
            sb.AppendLine(Localization.Volume_continues_on_error);

        if(jfsSb.s_flags.HasFlag(Flags.Panic))
            sb.AppendLine(Localization.Volume_panics_on_error);

        if(jfsSb.s_flags.HasFlag(Flags.UserQuota))
            sb.AppendLine(Localization.Volume_has_user_quotas_enabled);

        if(jfsSb.s_flags.HasFlag(Flags.GroupQuota))
            sb.AppendLine(Localization.Volume_has_group_quotas_enabled);

        if(jfsSb.s_flags.HasFlag(Flags.NoJournal))
            sb.AppendLine(Localization.Volume_is_not_using_any_journal);

        if(jfsSb.s_flags.HasFlag(Flags.Discard))
            sb.AppendLine(Localization.Volume_sends_TRIM_UNMAP_commands_to_underlying_device);

        if(jfsSb.s_flags.HasFlag(Flags.GroupCommit))
            sb.AppendLine(Localization.Volume_commits_in_groups_of_1);

        if(jfsSb.s_flags.HasFlag(Flags.LazyCommit))
            sb.AppendLine(Localization.Volume_commits_lazy);

        if(jfsSb.s_flags.HasFlag(Flags.Temporary))
            sb.AppendLine(Localization.Volume_does_not_commit_to_log);

        if(jfsSb.s_flags.HasFlag(Flags.InlineLog))
            sb.AppendLine(Localization.Volume_has_log_within_itself);

        if(jfsSb.s_flags.HasFlag(Flags.InlineMoving))
            sb.AppendLine(Localization.Volume_has_log_within_itself_and_is_moving_it_out);

        if(jfsSb.s_flags.HasFlag(Flags.BadSAIT))
            sb.AppendLine(Localization.Volume_has_bad_current_secondary_ait);

        if(jfsSb.s_flags.HasFlag(Flags.Sparse))
            sb.AppendLine(Localization.Volume_supports_sparse_files);

        if(jfsSb.s_flags.HasFlag(Flags.DASDEnabled))
            sb.AppendLine(Localization.Volume_has_DASD_limits_enabled);

        if(jfsSb.s_flags.HasFlag(Flags.DASDPrime))
            sb.AppendLine(Localization.Volume_primes_DASD_on_boot);

        if(jfsSb.s_flags.HasFlag(Flags.SwapBytes))
            sb.AppendLine(Localization.Volume_is_in_a_big_endian_system);

        if(jfsSb.s_flags.HasFlag(Flags.DirIndex))
            sb.AppendLine(Localization.Volume_has_persistent_indexes);

        if(jfsSb.s_flags.HasFlag(Flags.Linux))
            sb.AppendLine(Localization.Volume_supports_Linux);

        if(jfsSb.s_flags.HasFlag(Flags.DFS))
            sb.AppendLine(Localization.Volume_supports_DCE_DFS_LFS);

        if(jfsSb.s_flags.HasFlag(Flags.OS2))
            sb.AppendLine(Localization.Volume_supports_OS2_and_is_case_insensitive);

        if(jfsSb.s_flags.HasFlag(Flags.AIX))
            sb.AppendLine(Localization.Volume_supports_AIX);

        if(jfsSb.s_state != 0)
            sb.AppendLine(Localization.Volume_is_dirty);

        sb.AppendFormat(Localization.Volume_was_last_updated_on_0_,
                        DateHandlers.UnixUnsignedToDateTime(jfsSb.s_time.tv_sec, jfsSb.s_time.tv_nsec)).AppendLine();

        if(jfsSb.s_version == 1)
            sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(jfsSb.s_fpack, encoding)).AppendLine();
        else
            sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(jfsSb.s_label, encoding)).AppendLine();

        sb.AppendFormat(Localization.Volume_UUID_0, jfsSb.s_uuid).AppendLine();

        metadata = new FileSystem
        {
            Type             = FS_TYPE,
            Clusters         = jfsSb.s_size,
            ClusterSize      = jfsSb.s_bsize,
            Bootable         = true,
            VolumeName       = StringHandlers.CToString(jfsSb.s_version == 1 ? jfsSb.s_fpack : jfsSb.s_label, encoding),
            VolumeSerial     = $"{jfsSb.s_uuid}",
            ModificationDate = DateHandlers.UnixUnsignedToDateTime(jfsSb.s_time.tv_sec, jfsSb.s_time.tv_nsec)
        };

        if(jfsSb.s_state != 0)
            metadata.Dirty = true;

        information = sb.ToString();
    }

#endregion
}