// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft NT File System plugin.
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

// Information from Inside Windows NT
/// <inheritdoc />
/// <summary>Implements detection of the New Technology File System (NTFS)</summary>
public sealed partial class NTFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        byte[] eigthBytes = new byte[8];

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] ntfsBpb);

        if(errno != ErrorNumber.NoError)
            return false;

        Array.Copy(ntfsBpb, 0x003, eigthBytes, 0, 8);
        string oemName = StringHandlers.CToString(eigthBytes);

        if(oemName != "NTFS    ")
            return false;

        byte   fatsNo    = ntfsBpb[0x010];
        ushort spFat     = BitConverter.ToUInt16(ntfsBpb, 0x016);
        ushort signature = BitConverter.ToUInt16(ntfsBpb, 0x1FE);

        if(fatsNo != 0)
            return false;

        if(spFat != 0)
            return false;

        return signature == 0xAA55;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = Encoding.Unicode;
        information = "";

        var sb = new StringBuilder();

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] ntfsBpb);

        if(errno != ErrorNumber.NoError)
            return;

        BiosParameterBlock ntfsBb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(ntfsBpb);

        sb.AppendFormat(Localization._0_bytes_per_sector, ntfsBb.bps).AppendLine();
        sb.AppendFormat(Localization._0_sectors_per_cluster_1_bytes, ntfsBb.spc, ntfsBb.spc * ntfsBb.bps).AppendLine();

        //          sb.AppendFormat("{0} reserved sectors", ntfs_bb.rsectors).AppendLine();
        //          sb.AppendFormat("{0} FATs", ntfs_bb.fats_no).AppendLine();
        //          sb.AppendFormat("{0} entries in the root folder", ntfs_bb.root_ent).AppendLine();
        //          sb.AppendFormat("{0} sectors on volume (small)", ntfs_bb.sml_sectors).AppendLine();
        sb.AppendFormat(Localization.Media_descriptor_0, ntfsBb.media).AppendLine();

        //          sb.AppendFormat("{0} sectors per FAT", ntfs_bb.spfat).AppendLine();
        sb.AppendFormat(Localization._0_sectors_per_track, ntfsBb.sptrk).AppendLine();
        sb.AppendFormat(Localization._0_heads, ntfsBb.heads).AppendLine();
        sb.AppendFormat(Localization._0_hidden_sectors_before_filesystem, ntfsBb.hsectors).AppendLine();

        //          sb.AppendFormat("{0} sectors on volume (big)", ntfs_bb.big_sectors).AppendLine();
        sb.AppendFormat(Localization.BIOS_drive_number_0, ntfsBb.drive_no).AppendLine();

        //          sb.AppendFormat("NT flags: 0x{0:X2}", ntfs_bb.nt_flags).AppendLine();
        //          sb.AppendFormat("Signature 1: 0x{0:X2}", ntfs_bb.signature1).AppendLine();
        sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes, ntfsBb.sectors, ntfsBb.sectors * ntfsBb.bps).
           AppendLine();

        sb.AppendFormat(Localization.Cluster_where_MFT_starts_0, ntfsBb.mft_lsn).AppendLine();
        sb.AppendFormat(Localization.Cluster_where_MFTMirr_starts_0, ntfsBb.mftmirror_lsn).AppendLine();

        if(ntfsBb.mft_rc_clusters > 0)
            sb.AppendFormat(Localization._0_clusters_per_MFT_record_1_bytes, ntfsBb.mft_rc_clusters,
                            ntfsBb.mft_rc_clusters * ntfsBb.bps * ntfsBb.spc).AppendLine();
        else
            sb.AppendFormat(Localization._0_bytes_per_MFT_record, 1 << -ntfsBb.mft_rc_clusters).AppendLine();

        if(ntfsBb.index_blk_cts > 0)
            sb.AppendFormat(Localization._0_clusters_per_Index_block_1_bytes, ntfsBb.index_blk_cts,
                            ntfsBb.index_blk_cts * ntfsBb.bps * ntfsBb.spc).AppendLine();
        else
            sb.AppendFormat(Localization._0_bytes_per_Index_block, 1 << -ntfsBb.index_blk_cts).AppendLine();

        sb.AppendFormat(Localization.Volume_serial_number_0_X16, ntfsBb.serial_no).AppendLine();

        //          sb.AppendFormat("Signature 2: 0x{0:X4}", ntfs_bb.signature2).AppendLine();

        Metadata = new FileSystem();

        if(ntfsBb.jump[0]    == 0xEB &&
           ntfsBb.jump[1]    > 0x4E  &&
           ntfsBb.jump[1]    < 0x80  &&
           ntfsBb.signature2 == 0xAA55)
        {
            Metadata.Bootable = true;
            string bootChk = Sha1Context.Data(ntfsBb.boot_code, out _);
            sb.AppendLine(Localization.Volume_is_bootable);
            sb.AppendFormat(Localization.Boot_code_SHA1_0, bootChk).AppendLine();
        }

        Metadata.ClusterSize  = (uint)(ntfsBb.spc      * ntfsBb.bps);
        Metadata.Clusters     = (ulong)(ntfsBb.sectors / ntfsBb.spc);
        Metadata.VolumeSerial = $"{ntfsBb.serial_no:X16}";
        Metadata.Type         = FS_TYPE;

        information = sb.ToString();
    }
}