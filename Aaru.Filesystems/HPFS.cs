// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPFS.cs
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
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

// Information from an old unnamed document
/// <inheritdoc />
/// <summary>Implements detection of IBM's High Performance File System (HPFS)</summary>
public sealed class HPFS : IFilesystem
{
    const string FS_TYPE = "hpfs";
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.HPFS_Name;
    /// <inheritdoc />
    public Guid Id => new("33513B2C-f590-4acb-8bf2-0b1d5e19dec5");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

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

        uint magic1 = BitConverter.ToUInt32(hpfsSbSector, 0x000);
        uint magic2 = BitConverter.ToUInt32(hpfsSbSector, 0x004);

        return magic1 == 0xF995E849 && magic2 == 0xFA53E9C5;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("ibm850");
        information = "";

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
            sb.AppendFormat(Localization.File_system_type_0_Should_be_HPFS, bpb.fs_type).AppendLine();
            sb.AppendFormat(Localization.Superblock_magic1_0_Should_be_0xF995E849, hpfsSb.magic1).AppendLine();
            sb.AppendFormat(Localization.Superblock_magic2_0_Should_be_0xFA53E9C5, hpfsSb.magic2).AppendLine();
            sb.AppendFormat(Localization.Spareblock_magic1_0_Should_be_0xF9911849, sp.magic1).AppendLine();
            sb.AppendFormat(Localization.Spareblock_magic2_0_Should_be_0xFA5229C5, sp.magic2).AppendLine();
        }

        sb.AppendFormat(Localization.OEM_name_0, StringHandlers.CToString(bpb.oem_name)).AppendLine();
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
        sb.AppendFormat(Localization.Volume_label_0, StringHandlers.CToString(bpb.volume_label, Encoding)).AppendLine();

        //          sb.AppendFormat("Filesystem type: \"{0}\"", hpfs_bpb.fs_type).AppendLine();

        DateTime lastChk   = DateHandlers.UnixToDateTime(hpfsSb.last_chkdsk);
        DateTime lastOptim = DateHandlers.UnixToDateTime(hpfsSb.last_optim);

        sb.AppendFormat(Localization.HPFS_version_0, hpfsSb.version).AppendLine();
        sb.AppendFormat(Localization.Functional_version_0, hpfsSb.func_version).AppendLine();
        sb.AppendFormat(Localization.Sector_of_root_directory_FNode_0, hpfsSb.root_fnode).AppendLine();
        sb.AppendFormat(Localization._0_sectors_are_marked_bad, hpfsSb.badblocks).AppendLine();
        sb.AppendFormat(Localization.Sector_of_free_space_bitmaps_0, hpfsSb.bitmap_lsn).AppendLine();
        sb.AppendFormat(Localization.Sector_of_bad_blocks_list_0, hpfsSb.badblock_lsn).AppendLine();

        if(hpfsSb.last_chkdsk > 0)
            sb.AppendFormat(Localization.Date_of_last_integrity_check_0, lastChk).AppendLine();
        else
            sb.AppendLine(Localization.Filesystem_integrity_has_never_been_checked);

        if(hpfsSb.last_optim > 0)
            sb.AppendFormat(Localization.Date_of_last_optimization_0, lastOptim).AppendLine();
        else
            sb.AppendLine(Localization.Filesystem_has_never_been_optimized);

        sb.AppendFormat(Localization.Directory_band_has_0_sectors, hpfsSb.dband_sectors).AppendLine();
        sb.AppendFormat(Localization.Directory_band_starts_at_sector_0, hpfsSb.dband_start).AppendLine();
        sb.AppendFormat(Localization.Directory_band_ends_at_sector_0, hpfsSb.dband_last).AppendLine();
        sb.AppendFormat(Localization.Sector_of_directory_band_bitmap_0, hpfsSb.dband_bitmap).AppendLine();
        sb.AppendFormat(Localization.Sector_of_ACL_directory_0, hpfsSb.acl_start).AppendLine();

        sb.AppendFormat(Localization.Sector_of_Hotfix_directory_0, sp.hotfix_start).AppendLine();
        sb.AppendFormat(Localization._0_used_Hotfix_entries, sp.hotfix_used).AppendLine();
        sb.AppendFormat(Localization._0_total_Hotfix_entries, sp.hotfix_entries).AppendLine();
        sb.AppendFormat(Localization._0_free_spare_DNodes, sp.spare_dnodes_free).AppendLine();
        sb.AppendFormat(Localization._0_total_spare_DNodes, sp.spare_dnodes).AppendLine();
        sb.AppendFormat(Localization.Sector_of_codepage_directory_0, sp.codepage_lsn).AppendLine();
        sb.AppendFormat(Localization._0_codepages_used_in_the_volume, sp.codepages).AppendLine();
        sb.AppendFormat(Localization.SuperBlock_CRC32_0, sp.sb_crc32).AppendLine();
        sb.AppendFormat(Localization.SpareBlock_CRC32_0, sp.sp_crc32).AppendLine();

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

        XmlFsType = new FileSystemType();

        // Theoretically everything from BPB to SB is boot code, should I hash everything or only the sector loaded by BIOS itself?
        if(bpb.jump[0]    == 0xEB &&
           bpb.jump[1]    > 0x3C  &&
           bpb.jump[1]    < 0x80  &&
           bpb.signature2 == 0xAA55)
        {
            XmlFsType.Bootable = true;
            string bootChk = Sha1Context.Data(bpb.boot_code, out byte[] _);
            sb.AppendLine(Localization.Volume_is_bootable);
            sb.AppendFormat(Localization.Boot_code_SHA1_0, bootChk).AppendLine();
        }

        XmlFsType.Dirty            |= (sp.flags1 & 0x01) == 0x01;
        XmlFsType.Clusters         =  hpfsSb.sectors;
        XmlFsType.ClusterSize      =  bpb.bps;
        XmlFsType.Type             =  FS_TYPE;
        XmlFsType.VolumeName       =  StringHandlers.CToString(bpb.volume_label, Encoding);
        XmlFsType.VolumeSerial     =  $"{bpb.serial_no:X8}";
        XmlFsType.SystemIdentifier =  StringHandlers.CToString(bpb.oem_name);

        information = sb.ToString();
    }

    /// <summary>BIOS Parameter Block, at sector 0</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BiosParameterBlock
    {
        /// <summary>0x000, Jump to boot code</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>0x003, OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>0x00B, Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>0x00D, Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>0x00E, Reserved sectors between BPB and... does it have sense in HPFS?</summary>
        public readonly ushort rsectors;
        /// <summary>0x010, Number of FATs... seriously?</summary>
        public readonly byte fats_no;
        /// <summary>0x011, Number of entries on root directory... ok</summary>
        public readonly ushort root_ent;
        /// <summary>0x013, Sectors in volume... doubt it</summary>
        public readonly ushort sectors;
        /// <summary>0x015, Media descriptor</summary>
        public readonly byte media;
        /// <summary>0x016, Sectors per FAT... again</summary>
        public readonly ushort spfat;
        /// <summary>0x018, Sectors per track... you're kidding</summary>
        public readonly ushort sptrk;
        /// <summary>0x01A, Heads... stop!</summary>
        public readonly ushort heads;
        /// <summary>0x01C, Hidden sectors before BPB</summary>
        public readonly uint hsectors;
        /// <summary>0x024, Sectors in volume if &gt; 65535...</summary>
        public readonly uint big_sectors;
        /// <summary>0x028, Drive number</summary>
        public readonly byte drive_no;
        /// <summary>0x029, Volume flags?</summary>
        public readonly byte nt_flags;
        /// <summary>0x02A, EPB signature, 0x29</summary>
        public readonly byte signature;
        /// <summary>0x02B, Volume serial number</summary>
        public readonly uint serial_no;
        /// <summary>0x02F, Volume label, 11 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] volume_label;
        /// <summary>0x03A, Filesystem type, 8 bytes, space-padded ("HPFS    ")</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] fs_type;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 448)]
        public readonly byte[] boot_code;
        /// <summary>0x1FE, 0xAA55</summary>
        public readonly ushort signature2;
    }

    /// <summary>HPFS superblock at sector 16</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        /// <summary>0x000, 0xF995E849</summary>
        public readonly uint magic1;
        /// <summary>0x004, 0xFA53E9C5</summary>
        public readonly uint magic2;
        /// <summary>0x008, HPFS version</summary>
        public readonly byte version;
        /// <summary>0x009, 2 if &lt;= 4 GiB, 3 if &gt; 4 GiB</summary>
        public readonly byte func_version;
        /// <summary>0x00A, Alignment</summary>
        public readonly ushort dummy;
        /// <summary>0x00C, LSN pointer to root fnode</summary>
        public readonly uint root_fnode;
        /// <summary>0x010, Sectors on volume</summary>
        public readonly uint sectors;
        /// <summary>0x014, Bad blocks on volume</summary>
        public readonly uint badblocks;
        /// <summary>0x018, LSN pointer to volume bitmap</summary>
        public readonly uint bitmap_lsn;
        /// <summary>0x01C, 0</summary>
        public readonly uint zero1;
        /// <summary>0x020, LSN pointer to badblock directory</summary>
        public readonly uint badblock_lsn;
        /// <summary>0x024, 0</summary>
        public readonly uint zero2;
        /// <summary>0x028, Time of last CHKDSK</summary>
        public readonly int last_chkdsk;
        /// <summary>0x02C, Time of last optimization</summary>
        public readonly int last_optim;
        /// <summary>0x030, Sectors of dir band</summary>
        public readonly uint dband_sectors;
        /// <summary>0x034, Start sector of dir band</summary>
        public readonly uint dband_start;
        /// <summary>0x038, Last sector of dir band</summary>
        public readonly uint dband_last;
        /// <summary>0x03C, LSN of free space bitmap</summary>
        public readonly uint dband_bitmap;
        /// <summary>0x040, Can be used for volume name (32 bytes)</summary>
        public readonly ulong zero3;
        /// <summary>0x048, ...</summary>
        public readonly ulong zero4;
        /// <summary>0x04C, ...</summary>
        public readonly ulong zero5;
        /// <summary>0x050, ...;</summary>
        public readonly ulong zero6;
        /// <summary>0x058, LSN pointer to ACLs (only HPFS386)</summary>
        public readonly uint acl_start;
    }

    /// <summary>HPFS spareblock at sector 17</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SpareBlock
    {
        /// <summary>0x000, 0xF9911849</summary>
        public readonly uint magic1;
        /// <summary>0x004, 0xFA5229C5</summary>
        public readonly uint magic2;
        /// <summary>0x008, HPFS flags</summary>
        public readonly byte flags1;
        /// <summary>0x009, HPFS386 flags</summary>
        public readonly byte flags2;
        /// <summary>0x00A, Alignment</summary>
        public readonly ushort dummy;
        /// <summary>0x00C, LSN of hotfix directory</summary>
        public readonly uint hotfix_start;
        /// <summary>0x010, Used hotfixes</summary>
        public readonly uint hotfix_used;
        /// <summary>0x014, Total hotfixes available</summary>
        public readonly uint hotfix_entries;
        /// <summary>0x018, Unused spare dnodes</summary>
        public readonly uint spare_dnodes_free;
        /// <summary>0x01C, Length of spare dnodes list</summary>
        public readonly uint spare_dnodes;
        /// <summary>0x020, LSN of codepage directory</summary>
        public readonly uint codepage_lsn;
        /// <summary>0x024, Number of codepages used</summary>
        public readonly uint codepages;
        /// <summary>0x028, SuperBlock CRC32 (only HPFS386)</summary>
        public readonly uint sb_crc32;
        /// <summary>0x02C, SpareBlock CRC32 (only HPFS386)</summary>
        public readonly uint sp_crc32;
    }
}