// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Locus filesystem plugin
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
//     License aint with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// Commit count

using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using commitcnt_t = System.Int32;

// Disk address
using daddr_t = System.Int32;

// Fstore
using fstore_t = System.Int32;

// Global File System number
using gfs_t = System.Int32;

// Inode number
using ino_t = System.Int32;

// Filesystem pack number
using pckno_t = System.Int16;

// Timestamp
using time_t = System.Int32;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Locus filesystem</summary>
public sealed partial class Locus
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        for(ulong location = 0; location <= 8; location++)
        {
            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            if(partition.Start + location + sbSize >= imagePlugin.Info.Sectors)
                break;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + location, sbSize, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return false;

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return false;

            Superblock locusSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            AaruConsole.DebugWriteLine("Locus plugin", Localization.magic_at_1_equals_0, locusSb.s_magic, location);

            if(locusSb.s_magic is LOCUS_MAGIC or LOCUS_CIGAM or LOCUS_MAGIC_OLD or LOCUS_CIGAM_OLD)
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        if(imagePlugin.Info.SectorSize < 512)
            return;

        var    locusSb = new Superblock();
        byte[] sector  = null;

        for(ulong location = 0; location <= 8; location++)
        {
            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + location, sbSize, out sector);

            if(errno != ErrorNumber.NoError)
                continue;

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return;

            locusSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            if(locusSb.s_magic is LOCUS_MAGIC or LOCUS_CIGAM or LOCUS_MAGIC_OLD or LOCUS_CIGAM_OLD)
                break;
        }

        // We don't care about old version for information
        if(locusSb.s_magic != LOCUS_MAGIC     &&
           locusSb.s_magic != LOCUS_CIGAM     &&
           locusSb.s_magic != LOCUS_MAGIC_OLD &&
           locusSb.s_magic != LOCUS_CIGAM_OLD)
            return;

        // Numerical arrays are not important for information so no need to swap them
        if(locusSb.s_magic is LOCUS_CIGAM or LOCUS_CIGAM_OLD)
        {
            locusSb         = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);
            locusSb.s_flags = (Flags)Swapping.Swap((ushort)locusSb.s_flags);
        }

        var sb = new StringBuilder();

        sb.AppendLine(locusSb.s_magic == LOCUS_MAGIC_OLD ? Localization.Locus_filesystem_old
                          : Localization.Locus_filesystem);

        int blockSize = locusSb.s_version == Version.SB_SB4096 ? 4096 : 1024;

        // ReSharper disable once InconsistentNaming
        string s_fsmnt = StringHandlers.CToString(locusSb.s_fsmnt, Encoding);

        // ReSharper disable once InconsistentNaming
        string s_fpack = StringHandlers.CToString(locusSb.s_fpack, Encoding);

        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_magic = 0x{0:X8}", locusSb.s_magic);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_gfs = {0}", locusSb.s_gfs);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_fsize = {0}", locusSb.s_fsize);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_lwm = {0}", locusSb.s_lwm);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_hwm = {0}", locusSb.s_hwm);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_llst = {0}", locusSb.s_llst);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_fstore = {0}", locusSb.s_fstore);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_time = {0}", locusSb.s_time);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_tfree = {0}", locusSb.s_tfree);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_isize = {0}", locusSb.s_isize);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_nfree = {0}", locusSb.s_nfree);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_flags = {0}", locusSb.s_flags);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_tinode = {0}", locusSb.s_tinode);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_lasti = {0}", locusSb.s_lasti);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_nbehind = {0}", locusSb.s_nbehind);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_gfspack = {0}", locusSb.s_gfspack);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_ninode = {0}", locusSb.s_ninode);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_flock = {0}", locusSb.s_flock);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_ilock = {0}", locusSb.s_ilock);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_fmod = {0}", locusSb.s_fmod);
        AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_version = {0}", locusSb.s_version);

        sb.AppendFormat(Localization.Superblock_last_modified_on_0, DateHandlers.UnixToDateTime(locusSb.s_time)).
           AppendLine();

        sb.AppendFormat(Localization.Volume_has_0_blocks_of_1_bytes_each_total_2_bytes, locusSb.s_fsize, blockSize,
                        locusSb.s_fsize * blockSize).AppendLine();

        sb.AppendFormat(Localization._0_blocks_free_1_bytes, locusSb.s_tfree, locusSb.s_tfree * blockSize).AppendLine();
        sb.AppendFormat(Localization.Inode_list_uses_0_blocks, locusSb.s_isize).AppendLine();
        sb.AppendFormat(Localization._0_free_inodes, locusSb.s_tinode).AppendLine();
        sb.AppendFormat(Localization.Next_free_inode_search_will_start_at_inode_0, locusSb.s_lasti).AppendLine();

        sb.AppendFormat(Localization.There_are_an_estimate_of_0_free_inodes_before_next_search_start,
                        locusSb.s_nbehind).AppendLine();

        if(locusSb.s_flags.HasFlag(Flags.SB_RDONLY))
            sb.AppendLine(Localization.Read_only_volume);

        if(locusSb.s_flags.HasFlag(Flags.SB_CLEAN))
            sb.AppendLine(Localization.Clean_volume);

        if(locusSb.s_flags.HasFlag(Flags.SB_DIRTY))
            sb.AppendLine(Localization.Dirty_volume);

        if(locusSb.s_flags.HasFlag(Flags.SB_RMV))
            sb.AppendLine(Localization.Removable_volume);

        if(locusSb.s_flags.HasFlag(Flags.SB_PRIMPACK))
            sb.AppendLine(Localization.This_is_the_primary_pack);

        if(locusSb.s_flags.HasFlag(Flags.SB_REPLTYPE))
            sb.AppendLine(Localization.Replicated_volume);

        if(locusSb.s_flags.HasFlag(Flags.SB_USER))
            sb.AppendLine(Localization.User_replicated_volume);

        if(locusSb.s_flags.HasFlag(Flags.SB_BACKBONE))
            sb.AppendLine(Localization.Backbone_volume);

        if(locusSb.s_flags.HasFlag(Flags.SB_NFS))
            sb.AppendLine(Localization.NFS_volume);

        if(locusSb.s_flags.HasFlag(Flags.SB_BYHAND))
            sb.AppendLine(Localization.Volume_inhibits_automatic_fsck);

        if(locusSb.s_flags.HasFlag(Flags.SB_NOSUID))
            sb.AppendLine(Localization.Set_uid_set_gid_is_disabled);

        if(locusSb.s_flags.HasFlag(Flags.SB_SYNCW))
            sb.AppendLine(Localization.Volume_uses_synchronous_writes);

        sb.AppendFormat(Localization.Volume_label_0, s_fsmnt).AppendLine();
        sb.AppendFormat(Localization.Physical_volume_name_0, s_fpack).AppendLine();
        sb.AppendFormat(Localization.Global_File_System_number_0, locusSb.s_gfs).AppendLine();
        sb.AppendFormat(Localization.Global_File_System_pack_number_0, locusSb.s_gfspack).AppendLine();

        information = sb.ToString();

        XmlFsType = new FileSystemType
        {
            Type        = FS_TYPE,
            ClusterSize = (uint)blockSize,
            Clusters    = (ulong)locusSb.s_fsize,

            // Sometimes it uses one, or the other. Use the bigger
            VolumeName = string.IsNullOrEmpty(s_fsmnt) ? s_fpack : s_fsmnt,
            ModificationDate = DateHandlers.UnixToDateTime(locusSb.s_time),
            ModificationDateSpecified = true,
            Dirty = !locusSb.s_flags.HasFlag(Flags.SB_CLEAN) || locusSb.s_flags.HasFlag(Flags.SB_DIRTY),
            FreeClusters = (ulong)locusSb.s_tfree,
            FreeClustersSpecified = true
        };
    }
}