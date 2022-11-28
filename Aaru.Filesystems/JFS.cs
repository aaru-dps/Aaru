// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : JFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : IBM JFS filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the IBM JFS filesystem and shows information.
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

// ReSharper disable UnusedMember.Local

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of IBM's Journaled File System</summary>
public sealed class JFS : IFilesystem
{
    const uint JFS_BOOT_BLOCKS_SIZE = 0x8000;
    const uint JFS_MAGIC            = 0x3153464A;

    const string FS_TYPE = "jfs";

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.JFS_Name;
    /// <inheritdoc />
    public Guid Id => new("D3BE2A41-8F28-4055-94DC-BB6C72A0E9C4");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

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
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";
        var         sb          = new StringBuilder();
        uint        bootSectors = JFS_BOOT_BLOCKS_SIZE / imagePlugin.Info.SectorSize;
        ErrorNumber errno       = imagePlugin.ReadSector(partition.Start + bootSectors, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < 512)
            return;

        SuperBlock jfsSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

        sb.AppendLine(Localization.JFS_filesystem);
        sb.AppendFormat(Localization.Version_0, jfsSb.s_version).AppendLine();
        sb.AppendFormat(Localization._0_blocks_of_1_bytes, jfsSb.s_size, jfsSb.s_bsize).AppendLine();
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
            sb.AppendLine(Localization.Volume_has_log_withing_itself);

        if(jfsSb.s_flags.HasFlag(Flags.InlineMoving))
            sb.AppendLine(Localization.Volume_has_log_withing_itself_and_is_moving_it_out);

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
            sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(jfsSb.s_fpack, Encoding)).AppendLine();
        else
            sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(jfsSb.s_label, Encoding)).AppendLine();

        sb.AppendFormat(Localization.Volume_UUID_0, jfsSb.s_uuid).AppendLine();

        XmlFsType = new FileSystemType
        {
            Type = FS_TYPE,
            Clusters = jfsSb.s_size,
            ClusterSize = jfsSb.s_bsize,
            Bootable = true,
            VolumeName = StringHandlers.CToString(jfsSb.s_version == 1 ? jfsSb.s_fpack : jfsSb.s_label, Encoding),
            VolumeSerial = $"{jfsSb.s_uuid}",
            ModificationDate = DateHandlers.UnixUnsignedToDateTime(jfsSb.s_time.tv_sec, jfsSb.s_time.tv_nsec),
            ModificationDateSpecified = true
        };

        if(jfsSb.s_state != 0)
            XmlFsType.Dirty = true;

        information = sb.ToString();
    }

    [Flags, SuppressMessage("ReSharper", "InconsistentNaming")]
    enum Flags : uint
    {
        Unicode      = 0x00000001, RemountRO = 0x00000002, Continue    = 0x00000004,
        Panic        = 0x00000008, UserQuota = 0x00000010, GroupQuota  = 0x00000020,
        NoJournal    = 0x00000040, Discard   = 0x00000080, GroupCommit = 0x00000100,
        LazyCommit   = 0x00000200, Temporary = 0x00000400, InlineLog   = 0x00000800,
        InlineMoving = 0x00001000, BadSAIT   = 0x00010000, Sparse      = 0x00020000,
        DASDEnabled  = 0x00040000, DASDPrime = 0x00080000, SwapBytes   = 0x00100000,
        DirIndex     = 0x00200000, Linux     = 0x10000000, DFS         = 0x20000000,
        OS2          = 0x40000000, AIX       = 0x80000000
    }

    [Flags]
    enum State : uint
    {
        Clean   = 0, Mounted  = 1, Dirty = 2,
        Logredo = 4, Extendfs = 8
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Extent
    {
        /// <summary>Leftmost 24 bits are extent length, rest 8 bits are most significant for <see cref="addr2" /></summary>
        public readonly uint len_addr;
        public readonly uint addr2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct TimeStruct
    {
        public readonly uint tv_sec;
        public readonly uint tv_nsec;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        public readonly uint       s_magic;
        public readonly uint       s_version;
        public readonly ulong      s_size;
        public readonly uint       s_bsize;
        public readonly ushort     s_l2bsize;
        public readonly ushort     s_l2bfactor;
        public readonly uint       s_pbsize;
        public readonly ushort     s_l1pbsize;
        public readonly ushort     pad;
        public readonly uint       s_agsize;
        public readonly Flags      s_flags;
        public readonly State      s_state;
        public readonly uint       s_compress;
        public readonly Extent     s_ait2;
        public readonly Extent     s_aim2;
        public readonly uint       s_logdev;
        public readonly uint       s_logserial;
        public readonly Extent     s_logpxd;
        public readonly Extent     s_fsckpxd;
        public readonly TimeStruct s_time;
        public readonly uint       s_fsckloglen;
        public readonly sbyte      s_fscklog;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] s_fpack;
        public readonly ulong  s_xsize;
        public readonly Extent s_xfsckpxd;
        public readonly Extent s_xlogpxd;
        public readonly Guid   s_uuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] s_label;
        public readonly Guid s_loguuid;
    }
}