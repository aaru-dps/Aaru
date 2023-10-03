// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : OS/2 High Performance File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the OS/2 High Performance File System and shows information.
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
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from an old unnamed document
/// <inheritdoc />
/// <summary>Implements detection of IBM's High Performance File System (HPFS)</summary>
public sealed partial class HPFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(16 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno =
            imagePlugin.ReadSector(16 + partition.Start,
                                   out byte[] hpfsSbSector); // Seek to superblock, on logical sector 16

        if(errno != ErrorNumber.NoError)
            return false;

        var magic1 = BitConverter.ToUInt32(hpfsSbSector, 0x000);
        var magic2 = BitConverter.ToUInt32(hpfsSbSector, 0x004);

        return magic1 == 0xF995E849 && magic2 == 0xFA53E9C5;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("ibm850");
        information =   "";
        metadata    =   new FileSystem();

        var sb = new StringBuilder();

        ErrorNumber errno =
            imagePlugin.ReadSector(0 + partition.Start,
                                   out byte[] hpfsBpbSector); // Seek to BIOS parameter block, on logical sector 0

        if(errno != ErrorNumber.NoError)
            return;

        errno = imagePlugin.ReadSector(16 + partition.Start,
                                       out byte[] hpfsSbSector); // Seek to superblock, on logical sector 16

        if(errno != ErrorNumber.NoError)
            return;

        errno = imagePlugin.ReadSector(17 + partition.Start,
                                       out byte[] hpfsSpSector); // Seek to spareblock, on logical sector 17

        if(errno != ErrorNumber.NoError)
            return;

        BiosParameterBlock bpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(hpfsBpbSector);

        SuperBlock hpfsSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(hpfsSbSector);

        SpareBlock sp = Marshal.ByteArrayToStructureLittleEndian<SpareBlock>(hpfsSpSector);

        if(StringHandlers.CToString(bpb.fs_type) != "HPFS    " ||
           hpfsSb.magic1                         != 0xF995E849 ||
           hpfsSb.magic2                         != 0xFA53E9C5 ||
           sp.magic1                             != 0xF9911849 ||
           sp.magic2                             != 0xFA5229C5)
        {
            sb.AppendLine(Localization.This_may_not_be_HPFS_following_information_may_be_not_correct);
            sb.AppendFormat(Localization.File_system_type_0_Should_be_HPFS,        bpb.fs_type).AppendLine();
            sb.AppendFormat(Localization.Superblock_magic1_0_Should_be_0xF995E849, hpfsSb.magic1).AppendLine();
            sb.AppendFormat(Localization.Superblock_magic2_0_Should_be_0xFA53E9C5, hpfsSb.magic2).AppendLine();
            sb.AppendFormat(Localization.Spareblock_magic1_0_Should_be_0xF9911849, sp.magic1).AppendLine();
            sb.AppendFormat(Localization.Spareblock_magic2_0_Should_be_0xFA5229C5, sp.magic2).AppendLine();
        }

        sb.AppendFormat(Localization.OEM_name_0,          StringHandlers.CToString(bpb.oem_name)).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_sector, bpb.bps).AppendLine();

        //          sb.AppendFormat("{0} sectors per cluster", hpfs_bpb.spc).AppendLine();
        //          sb.AppendFormat("{0} reserved sectors", hpfs_bpb.rsectors).AppendLine();
        //          sb.AppendFormat("{0} FATs", hpfs_bpb.fats_no).AppendLine();
        //          sb.AppendFormat("{0} entries on root directory", hpfs_bpb.root_ent).AppendLine();
        //          sb.AppendFormat("{0} mini sectors on volume", hpfs_bpb.sectors).AppendLine();
        sb.AppendFormat(Localization.Media_descriptor_0, bpb.media).AppendLine();

        //          sb.AppendFormat("{0} sectors per FAT", hpfs_bpb.spfat).AppendLine();
        //          sb.AppendFormat("{0} sectors per track", hpfs_bpb.sptrk).AppendLine();
        //          sb.AppendFormat("{0} heads", hpfs_bpb.heads).AppendLine();
        sb.AppendFormat(Localization._0_sectors_hidden_before_BPB, bpb.hsectors).AppendLine();

        sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes, hpfsSb.sectors, hpfsSb.sectors * bpb.bps).
           AppendLine();

        //          sb.AppendFormat("{0} sectors on volume ({1} bytes)", hpfs_bpb.big_sectors, hpfs_bpb.big_sectors * hpfs_bpb.bps).AppendLine();
        sb.AppendFormat(Localization.BIOS_drive_number_0, bpb.drive_no).AppendLine();
        sb.AppendFormat(Localization.NT_Flags_0, bpb.nt_flags).AppendLine();
        sb.AppendFormat(Localization.Signature_0, bpb.signature).AppendLine();
        sb.AppendFormat(Localization.Serial_number_0, bpb.serial_no).AppendLine();
        sb.AppendFormat(Localization.Volume_label_0, StringHandlers.CToString(bpb.volume_label, encoding)).AppendLine();

        //          sb.AppendFormat("Filesystem type: \"{0}\"", hpfs_bpb.fs_type).AppendLine();

        DateTime lastChk   = DateHandlers.UnixToDateTime(hpfsSb.last_chkdsk);
        DateTime lastOptim = DateHandlers.UnixToDateTime(hpfsSb.last_optim);

        sb.AppendFormat(Localization.HPFS_version_0,                   hpfsSb.version).AppendLine();
        sb.AppendFormat(Localization.Functional_version_0,             hpfsSb.func_version).AppendLine();
        sb.AppendFormat(Localization.Sector_of_root_directory_FNode_0, hpfsSb.root_fnode).AppendLine();
        sb.AppendFormat(Localization._0_sectors_are_marked_bad,        hpfsSb.badblocks).AppendLine();
        sb.AppendFormat(Localization.Sector_of_free_space_bitmaps_0,   hpfsSb.bitmap_lsn).AppendLine();
        sb.AppendFormat(Localization.Sector_of_bad_blocks_list_0,      hpfsSb.badblock_lsn).AppendLine();

        if(hpfsSb.last_chkdsk > 0)
            sb.AppendFormat(Localization.Date_of_last_integrity_check_0, lastChk).AppendLine();
        else
            sb.AppendLine(Localization.Filesystem_integrity_has_never_been_checked);

        if(hpfsSb.last_optim > 0)
            sb.AppendFormat(Localization.Date_of_last_optimization_0, lastOptim).AppendLine();
        else
            sb.AppendLine(Localization.Filesystem_has_never_been_optimized);

        sb.AppendFormat(Localization.Directory_band_has_0_sectors,      hpfsSb.dband_sectors).AppendLine();
        sb.AppendFormat(Localization.Directory_band_starts_at_sector_0, hpfsSb.dband_start).AppendLine();
        sb.AppendFormat(Localization.Directory_band_ends_at_sector_0,   hpfsSb.dband_last).AppendLine();
        sb.AppendFormat(Localization.Sector_of_directory_band_bitmap_0, hpfsSb.dband_bitmap).AppendLine();
        sb.AppendFormat(Localization.Sector_of_ACL_directory_0,         hpfsSb.acl_start).AppendLine();

        sb.AppendFormat(Localization.Sector_of_Hotfix_directory_0,    sp.hotfix_start).AppendLine();
        sb.AppendFormat(Localization._0_used_Hotfix_entries,          sp.hotfix_used).AppendLine();
        sb.AppendFormat(Localization._0_total_Hotfix_entries,         sp.hotfix_entries).AppendLine();
        sb.AppendFormat(Localization._0_free_spare_DNodes,            sp.spare_dnodes_free).AppendLine();
        sb.AppendFormat(Localization._0_total_spare_DNodes,           sp.spare_dnodes).AppendLine();
        sb.AppendFormat(Localization.Sector_of_codepage_directory_0,  sp.codepage_lsn).AppendLine();
        sb.AppendFormat(Localization._0_codepages_used_in_the_volume, sp.codepages).AppendLine();
        sb.AppendFormat(Localization.SuperBlock_CRC32_0,              sp.sb_crc32).AppendLine();
        sb.AppendFormat(Localization.SpareBlock_CRC32_0,              sp.sp_crc32).AppendLine();

        sb.AppendLine(Localization.Flags);
        sb.AppendLine((sp.flags1 & 0x01) == 0x01 ? Localization.Filesystem_is_dirty : Localization.Filesystem_is_clean);

        if((sp.flags1 & 0x02) == 0x02)
            sb.AppendLine(Localization.Spare_directory_blocks_are_in_use);

        if((sp.flags1 & 0x04) == 0x04)
            sb.AppendLine(Localization.Hotfixes_are_in_use);

        if((sp.flags1 & 0x08) == 0x08)
            sb.AppendLine(Localization.Disk_contains_bad_sectors);

        if((sp.flags1 & 0x10) == 0x10)
            sb.AppendLine(Localization.Disk_has_a_bad_bitmap);

        if((sp.flags1 & 0x20) == 0x20)
            sb.AppendLine(Localization.Filesystem_was_formatted_fast);

        if((sp.flags1 & 0x40) == 0x40)
            sb.AppendLine(Localization.Unknown_flag_0x40_on_flags1_is_active);

        if((sp.flags1 & 0x80) == 0x80)
            sb.AppendLine(Localization.Filesystem_has_been_mounted_by_an_old_IFS);

        if((sp.flags2 & 0x01) == 0x01)
            sb.AppendLine(Localization.Install_DASD_limits);

        if((sp.flags2 & 0x02) == 0x02)
            sb.AppendLine(Localization.Resync_DASD_limits);

        if((sp.flags2 & 0x04) == 0x04)
            sb.AppendLine(Localization.DASD_limits_are_operational);

        if((sp.flags2 & 0x08) == 0x08)
            sb.AppendLine(Localization.Multimedia_is_active);

        if((sp.flags2 & 0x10) == 0x10)
            sb.AppendLine(Localization.DCE_ACLs_are_active);

        if((sp.flags2 & 0x20) == 0x20)
            sb.AppendLine(Localization.DASD_limits_are_dirty);

        if((sp.flags2 & 0x40) == 0x40)
            sb.AppendLine(Localization.Unknown_flag_0x40_on_flags2_is_active);

        if((sp.flags2 & 0x80) == 0x80)
            sb.AppendLine(Localization.Unknown_flag_0x80_on_flags2_is_active);

        metadata = new FileSystem();

        // Theoretically everything from BPB to SB is boot code, should I hash everything or only the sector loaded by BIOS itself?
        if(bpb.jump[0]    == 0xEB &&
           bpb.jump[1]    > 0x3C  &&
           bpb.jump[1]    < 0x80  &&
           bpb.signature2 == 0xAA55)
        {
            metadata.Bootable = true;
            string bootChk = Sha1Context.Data(bpb.boot_code, out byte[] _);
            sb.AppendLine(Localization.Volume_is_bootable);
            sb.AppendFormat(Localization.Boot_code_SHA1_0, bootChk).AppendLine();
        }

        metadata.Dirty            |= (sp.flags1 & 0x01) == 0x01;
        metadata.Clusters         =  hpfsSb.sectors;
        metadata.ClusterSize      =  bpb.bps;
        metadata.Type             =  FS_TYPE;
        metadata.VolumeName       =  StringHandlers.CToString(bpb.volume_label, encoding);
        metadata.VolumeSerial     =  $"{bpb.serial_no:X8}";
        metadata.SystemIdentifier =  StringHandlers.CToString(bpb.oem_name);

        information = sb.ToString();
    }

#endregion
}