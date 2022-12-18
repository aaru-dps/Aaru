// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Decoders;
using Aaru.Helpers;
using Claunia.Encoding;
using Encoding = System.Text.Encoding;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class LisaFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.ReadableSectorTags?.Contains(SectorTagType.AppleSectorTag) != true)
            return false;

        // Minimal LisaOS disk is 3.5" single sided double density, 800 sectors
        if(imagePlugin.Info.Sectors < 800)
            return false;

        int beforeMddf = -1;

        // LisaOS searches sectors until tag tells MDDF resides there, so we'll search 100 sectors
        for(int i = 0; i < 100; i++)
        {
            ErrorNumber errno = imagePlugin.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag, out byte[] tag);

            if(errno != ErrorNumber.NoError)
                continue;

            DecodeTag(tag, out LisaTag.PriamTag searchTag);

            AaruConsole.DebugWriteLine("LisaFS plugin", Localization.Sector_0_file_ID_1, i, searchTag.FileId);

            if(beforeMddf       == -1 &&
               searchTag.FileId == FILEID_LOADER_SIGNED)
                beforeMddf = i - 1;

            if(searchTag.FileId != FILEID_MDDF)
                continue;

            errno = imagePlugin.ReadSector((ulong)i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                continue;

            var infoMddf = new MDDF
            {
                mddf_block                   = BigEndianBitConverter.ToUInt32(sector, 0x6C),
                volsize_minus_one            = BigEndianBitConverter.ToUInt32(sector, 0x70),
                volsize_minus_mddf_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x74),
                vol_size                     = BigEndianBitConverter.ToUInt32(sector, 0x78),
                blocksize                    = BigEndianBitConverter.ToUInt16(sector, 0x7C),
                datasize                     = BigEndianBitConverter.ToUInt16(sector, 0x7E)
            };

            AaruConsole.DebugWriteLine("LisaFS plugin", Localization.Current_sector_0, i);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.mddf_block = {0}", infoMddf.mddf_block);
            AaruConsole.DebugWriteLine("LisaFS plugin", "Disk size = {0} sectors", imagePlugin.Info.Sectors);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.vol_size = {0} sectors", infoMddf.vol_size);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.vol_size - 1 = {0}", infoMddf.volsize_minus_one);

            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.vol_size - mddf.mddf_block -1 = {0}",
                                       infoMddf.volsize_minus_mddf_minus_one);

            AaruConsole.DebugWriteLine("LisaFS plugin", "Disk sector = {0} bytes", imagePlugin.Info.SectorSize);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.blocksize = {0} bytes", infoMddf.blocksize);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.datasize = {0} bytes", infoMddf.datasize);

            if(infoMddf.mddf_block != i - beforeMddf)
                return false;

            if(infoMddf.vol_size > imagePlugin.Info.Sectors)
                return false;

            if(infoMddf.vol_size - 1 != infoMddf.volsize_minus_one)
                return false;

            if(infoMddf.vol_size - i - 1 != infoMddf.volsize_minus_mddf_minus_one - beforeMddf)
                return false;

            if(infoMddf.datasize > infoMddf.blocksize)
                return false;

            if(infoMddf.blocksize < imagePlugin.Info.SectorSize)
                return false;

            return infoMddf.datasize == imagePlugin.Info.SectorSize;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    = new LisaRoman();
        information = "";
        metadata    = new FileSystem();
        var sb = new StringBuilder();

        if(imagePlugin.Info.ReadableSectorTags?.Contains(SectorTagType.AppleSectorTag) != true)
            return;

        // Minimal LisaOS disk is 3.5" single sided double density, 800 sectors
        if(imagePlugin.Info.Sectors < 800)
            return;

        int beforeMddf = -1;

        // LisaOS searches sectors until tag tells MDDF resides there, so we'll search 100 sectors
        for(int i = 0; i < 100; i++)
        {
            ErrorNumber errno = imagePlugin.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag, out byte[] tag);

            if(errno != ErrorNumber.NoError)
                continue;

            DecodeTag(tag, out LisaTag.PriamTag searchTag);

            AaruConsole.DebugWriteLine("LisaFS plugin", Localization.Sector_0_file_ID_1, i, searchTag.FileId);

            if(beforeMddf       == -1 &&
               searchTag.FileId == FILEID_LOADER_SIGNED)
                beforeMddf = i - 1;

            if(searchTag.FileId != FILEID_MDDF)
                continue;

            errno = imagePlugin.ReadSector((ulong)i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                continue;

            var    infoMddf = new MDDF();
            byte[] pString  = new byte[33];

            infoMddf.fsversion = BigEndianBitConverter.ToUInt16(sector, 0x00);
            infoMddf.volid     = BigEndianBitConverter.ToUInt64(sector, 0x02);
            infoMddf.volnum    = BigEndianBitConverter.ToUInt16(sector, 0x0A);
            Array.Copy(sector, 0x0C, pString, 0, 33);
            infoMddf.volname  = StringHandlers.PascalToString(pString, encoding);
            infoMddf.unknown1 = sector[0x2D];
            Array.Copy(sector, 0x2E, pString, 0, 33);

            // Prevent garbage
            infoMddf.password       = pString[0] <= 32 ? StringHandlers.PascalToString(pString, encoding) : "";
            infoMddf.unknown2       = sector[0x4F];
            infoMddf.machine_id     = BigEndianBitConverter.ToUInt32(sector, 0x50);
            infoMddf.master_copy_id = BigEndianBitConverter.ToUInt32(sector, 0x54);
            uint lisaTime = BigEndianBitConverter.ToUInt32(sector, 0x58);
            infoMddf.dtvc                         = DateHandlers.LisaToDateTime(lisaTime);
            lisaTime                              = BigEndianBitConverter.ToUInt32(sector, 0x5C);
            infoMddf.dtcc                         = DateHandlers.LisaToDateTime(lisaTime);
            lisaTime                              = BigEndianBitConverter.ToUInt32(sector, 0x60);
            infoMddf.dtvb                         = DateHandlers.LisaToDateTime(lisaTime);
            lisaTime                              = BigEndianBitConverter.ToUInt32(sector, 0x64);
            infoMddf.dtvs                         = DateHandlers.LisaToDateTime(lisaTime);
            infoMddf.unknown3                     = BigEndianBitConverter.ToUInt32(sector, 0x68);
            infoMddf.mddf_block                   = BigEndianBitConverter.ToUInt32(sector, 0x6C);
            infoMddf.volsize_minus_one            = BigEndianBitConverter.ToUInt32(sector, 0x70);
            infoMddf.volsize_minus_mddf_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x74);
            infoMddf.vol_size                     = BigEndianBitConverter.ToUInt32(sector, 0x78);
            infoMddf.blocksize                    = BigEndianBitConverter.ToUInt16(sector, 0x7C);
            infoMddf.datasize                     = BigEndianBitConverter.ToUInt16(sector, 0x7E);
            infoMddf.unknown4                     = BigEndianBitConverter.ToUInt16(sector, 0x80);
            infoMddf.unknown5                     = BigEndianBitConverter.ToUInt32(sector, 0x82);
            infoMddf.unknown6                     = BigEndianBitConverter.ToUInt32(sector, 0x86);
            infoMddf.clustersize                  = BigEndianBitConverter.ToUInt16(sector, 0x8A);
            infoMddf.fs_size                      = BigEndianBitConverter.ToUInt32(sector, 0x8C);
            infoMddf.unknown7                     = BigEndianBitConverter.ToUInt32(sector, 0x90);
            infoMddf.srec_ptr                     = BigEndianBitConverter.ToUInt32(sector, 0x94);
            infoMddf.unknown9                     = BigEndianBitConverter.ToUInt16(sector, 0x98);
            infoMddf.srec_len                     = BigEndianBitConverter.ToUInt16(sector, 0x9A);
            infoMddf.unknown10                    = BigEndianBitConverter.ToUInt32(sector, 0x9C);
            infoMddf.unknown11                    = BigEndianBitConverter.ToUInt32(sector, 0xA0);
            infoMddf.unknown12                    = BigEndianBitConverter.ToUInt32(sector, 0xA4);
            infoMddf.unknown13                    = BigEndianBitConverter.ToUInt32(sector, 0xA8);
            infoMddf.unknown14                    = BigEndianBitConverter.ToUInt32(sector, 0xAC);
            infoMddf.filecount                    = BigEndianBitConverter.ToUInt16(sector, 0xB0);
            infoMddf.unknown15                    = BigEndianBitConverter.ToUInt32(sector, 0xB2);
            infoMddf.unknown16                    = BigEndianBitConverter.ToUInt32(sector, 0xB6);
            infoMddf.freecount                    = BigEndianBitConverter.ToUInt32(sector, 0xBA);
            infoMddf.unknown17                    = BigEndianBitConverter.ToUInt16(sector, 0xBE);
            infoMddf.unknown18                    = BigEndianBitConverter.ToUInt32(sector, 0xC0);
            infoMddf.overmount_stamp              = BigEndianBitConverter.ToUInt64(sector, 0xC4);
            infoMddf.serialization                = BigEndianBitConverter.ToUInt32(sector, 0xCC);
            infoMddf.unknown19                    = BigEndianBitConverter.ToUInt32(sector, 0xD0);
            infoMddf.unknown_timestamp            = BigEndianBitConverter.ToUInt32(sector, 0xD4);
            infoMddf.unknown20                    = BigEndianBitConverter.ToUInt32(sector, 0xD8);
            infoMddf.unknown21                    = BigEndianBitConverter.ToUInt32(sector, 0xDC);
            infoMddf.unknown22                    = BigEndianBitConverter.ToUInt32(sector, 0xE0);
            infoMddf.unknown23                    = BigEndianBitConverter.ToUInt32(sector, 0xE4);
            infoMddf.unknown24                    = BigEndianBitConverter.ToUInt32(sector, 0xE8);
            infoMddf.unknown25                    = BigEndianBitConverter.ToUInt32(sector, 0xEC);
            infoMddf.unknown26                    = BigEndianBitConverter.ToUInt32(sector, 0xF0);
            infoMddf.unknown27                    = BigEndianBitConverter.ToUInt32(sector, 0xF4);
            infoMddf.unknown28                    = BigEndianBitConverter.ToUInt32(sector, 0xF8);
            infoMddf.unknown29                    = BigEndianBitConverter.ToUInt32(sector, 0xFC);
            infoMddf.unknown30                    = BigEndianBitConverter.ToUInt32(sector, 0x100);
            infoMddf.unknown31                    = BigEndianBitConverter.ToUInt32(sector, 0x104);
            infoMddf.unknown32                    = BigEndianBitConverter.ToUInt32(sector, 0x108);
            infoMddf.unknown33                    = BigEndianBitConverter.ToUInt32(sector, 0x10C);
            infoMddf.unknown34                    = BigEndianBitConverter.ToUInt32(sector, 0x110);
            infoMddf.unknown35                    = BigEndianBitConverter.ToUInt32(sector, 0x114);
            infoMddf.backup_volid                 = BigEndianBitConverter.ToUInt64(sector, 0x118);
            infoMddf.label_size                   = BigEndianBitConverter.ToUInt16(sector, 0x120);
            infoMddf.fs_overhead                  = BigEndianBitConverter.ToUInt16(sector, 0x122);
            infoMddf.result_scavenge              = BigEndianBitConverter.ToUInt16(sector, 0x124);
            infoMddf.boot_code                    = BigEndianBitConverter.ToUInt16(sector, 0x126);
            infoMddf.boot_environ                 = BigEndianBitConverter.ToUInt16(sector, 0x6C);
            infoMddf.unknown36                    = BigEndianBitConverter.ToUInt32(sector, 0x12A);
            infoMddf.unknown37                    = BigEndianBitConverter.ToUInt32(sector, 0x12E);
            infoMddf.unknown38                    = BigEndianBitConverter.ToUInt32(sector, 0x132);
            infoMddf.vol_sequence                 = BigEndianBitConverter.ToUInt16(sector, 0x136);
            infoMddf.vol_left_mounted             = sector[0x138];

            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown1 = 0x{0:X2} ({0})", infoMddf.unknown1);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown2 = 0x{0:X2} ({0})", infoMddf.unknown2);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown3 = 0x{0:X8} ({0})", infoMddf.unknown3);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown4 = 0x{0:X4} ({0})", infoMddf.unknown4);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown5 = 0x{0:X8} ({0})", infoMddf.unknown5);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown6 = 0x{0:X8} ({0})", infoMddf.unknown6);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown7 = 0x{0:X8} ({0})", infoMddf.unknown7);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown9 = 0x{0:X4} ({0})", infoMddf.unknown9);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown10 = 0x{0:X8} ({0})", infoMddf.unknown10);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown11 = 0x{0:X8} ({0})", infoMddf.unknown11);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown12 = 0x{0:X8} ({0})", infoMddf.unknown12);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown13 = 0x{0:X8} ({0})", infoMddf.unknown13);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown14 = 0x{0:X8} ({0})", infoMddf.unknown14);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown15 = 0x{0:X8} ({0})", infoMddf.unknown15);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown16 = 0x{0:X8} ({0})", infoMddf.unknown16);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown17 = 0x{0:X4} ({0})", infoMddf.unknown17);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown18 = 0x{0:X8} ({0})", infoMddf.unknown18);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown19 = 0x{0:X8} ({0})", infoMddf.unknown19);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown20 = 0x{0:X8} ({0})", infoMddf.unknown20);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown21 = 0x{0:X8} ({0})", infoMddf.unknown21);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown22 = 0x{0:X8} ({0})", infoMddf.unknown22);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown23 = 0x{0:X8} ({0})", infoMddf.unknown23);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown24 = 0x{0:X8} ({0})", infoMddf.unknown24);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown25 = 0x{0:X8} ({0})", infoMddf.unknown25);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown26 = 0x{0:X8} ({0})", infoMddf.unknown26);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown27 = 0x{0:X8} ({0})", infoMddf.unknown27);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown28 = 0x{0:X8} ({0})", infoMddf.unknown28);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown29 = 0x{0:X8} ({0})", infoMddf.unknown29);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown30 = 0x{0:X8} ({0})", infoMddf.unknown30);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown31 = 0x{0:X8} ({0})", infoMddf.unknown31);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown32 = 0x{0:X8} ({0})", infoMddf.unknown32);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown33 = 0x{0:X8} ({0})", infoMddf.unknown33);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown34 = 0x{0:X8} ({0})", infoMddf.unknown34);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown35 = 0x{0:X8} ({0})", infoMddf.unknown35);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown36 = 0x{0:X8} ({0})", infoMddf.unknown36);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown37 = 0x{0:X8} ({0})", infoMddf.unknown37);
            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown38 = 0x{0:X8} ({0})", infoMddf.unknown38);

            AaruConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown_timestamp = 0x{0:X8} ({0}, {1})",
                                       infoMddf.unknown_timestamp,
                                       DateHandlers.LisaToDateTime(infoMddf.unknown_timestamp));

            if(infoMddf.mddf_block != i - beforeMddf)
                return;

            if(infoMddf.vol_size > imagePlugin.Info.Sectors)
                return;

            if(infoMddf.vol_size - 1 != infoMddf.volsize_minus_one)
                return;

            if(infoMddf.vol_size - i - 1 != infoMddf.volsize_minus_mddf_minus_one - beforeMddf)
                return;

            if(infoMddf.datasize > infoMddf.blocksize)
                return;

            if(infoMddf.blocksize < imagePlugin.Info.SectorSize)
                return;

            if(infoMddf.datasize != imagePlugin.Info.SectorSize)
                return;

            switch(infoMddf.fsversion)
            {
                case LISA_V1:
                    sb.AppendLine("LisaFS v1");

                    break;
                case LISA_V2:
                    sb.AppendLine("LisaFS v2");

                    break;
                case LISA_V3:
                    sb.AppendLine("LisaFS v3");

                    break;
                default:
                    sb.AppendFormat(Localization.Unknown_LisaFS_version_0, infoMddf.fsversion).AppendLine();

                    break;
            }

            sb.AppendFormat(Localization.Volume_name_0, infoMddf.volname).AppendLine();
            sb.AppendFormat(Localization.Volume_password_0, infoMddf.password).AppendLine();
            sb.AppendFormat(Localization.Volume_ID_0_X16, infoMddf.volid).AppendLine();
            sb.AppendFormat(Localization.Backup_volume_ID_0, infoMddf.backup_volid).AppendLine();

            sb.AppendFormat(Localization.Master_copy_ID_0, infoMddf.master_copy_id).AppendLine();

            sb.AppendFormat(Localization.Volume_is_number_0_of_1, infoMddf.volnum, infoMddf.vol_sequence).AppendLine();

            sb.AppendFormat(Localization.Serial_number_of_Lisa_computer_that_created_this_volume_0,
                            infoMddf.machine_id).AppendLine();

            sb.AppendFormat(Localization.Serial_number_of_Lisa_computer_that_can_use_this_volume_software_0,
                            infoMddf.serialization).AppendLine();

            sb.AppendFormat(Localization.Volume_created_on_0, infoMddf.dtvc).AppendLine();
            sb.AppendFormat(Localization.Volume_catalog_created_on_0, infoMddf.dtcc).AppendLine();
            sb.AppendFormat(Localization.Volume_backed_up_on_0, infoMddf.dtvb).AppendLine();
            sb.AppendFormat(Localization.Volume_scavenged_on_0, infoMddf.dtvs).AppendLine();
            sb.AppendFormat(Localization.MDDF_is_in_block_0, infoMddf.mddf_block + beforeMddf).AppendLine();
            sb.AppendFormat(Localization.There_are_0_reserved_blocks_before_volume, beforeMddf).AppendLine();
            sb.AppendFormat(Localization._0_blocks_minus_one, infoMddf.volsize_minus_one).AppendLine();

            sb.AppendFormat(Localization._0_blocks_minus_one_minus_MDDF_offset, infoMddf.volsize_minus_mddf_minus_one).
               AppendLine();

            sb.AppendFormat(Localization._0_blocks_in_volume, infoMddf.vol_size).AppendLine();
            sb.AppendFormat(Localization._0_bytes_per_sector_uncooked, infoMddf.blocksize).AppendLine();
            sb.AppendFormat(Localization._0_bytes_per_sector, infoMddf.datasize).AppendLine();
            sb.AppendFormat(Localization._0_blocks_per_cluster, infoMddf.clustersize).AppendLine();
            sb.AppendFormat(Localization._0_blocks_in_filesystem, infoMddf.fs_size).AppendLine();
            sb.AppendFormat(Localization._0_files_in_volume, infoMddf.filecount).AppendLine();
            sb.AppendFormat(Localization._0_blocks_free, infoMddf.freecount).AppendLine();
            sb.AppendFormat(Localization._0_bytes_in_LisaInfo, infoMddf.label_size).AppendLine();
            sb.AppendFormat(Localization.Filesystem_overhead_0, infoMddf.fs_overhead).AppendLine();
            sb.AppendFormat(Localization.Scavenger_result_code_0, infoMddf.result_scavenge).AppendLine();
            sb.AppendFormat(Localization.Boot_code_0, infoMddf.boot_code).AppendLine();
            sb.AppendFormat(Localization.Boot_environment_0, infoMddf.boot_environ).AppendLine();
            sb.AppendFormat(Localization.Overmount_stamp_0, infoMddf.overmount_stamp).AppendLine();

            sb.AppendFormat(Localization.S_Records_start_at_0_and_spans_for_1_blocks,
                            infoMddf.srec_ptr + infoMddf.mddf_block + beforeMddf, infoMddf.srec_len).AppendLine();

            sb.AppendLine(infoMddf.vol_left_mounted == 0 ? Localization.Volume_is_clean : Localization.Volume_is_dirty);

            information = sb.ToString();

            metadata = new FileSystem();

            if(DateTime.Compare(infoMddf.dtvb, DateHandlers.LisaToDateTime(0)) > 0)
            {
                metadata.BackupDate = infoMddf.dtvb;
            }

            metadata.Clusters    = infoMddf.vol_size;
            metadata.ClusterSize = (uint)(infoMddf.clustersize * infoMddf.datasize);

            if(DateTime.Compare(infoMddf.dtvc, DateHandlers.LisaToDateTime(0)) > 0)
            {
                metadata.CreationDate = infoMddf.dtvc;
            }

            metadata.Dirty        = infoMddf.vol_left_mounted != 0;
            metadata.Files        = infoMddf.filecount;
            metadata.FreeClusters = infoMddf.freecount;
            metadata.Type         = FS_TYPE;
            metadata.VolumeName   = infoMddf.volname;
            metadata.VolumeSerial = $"{infoMddf.volid:X16}";

            return;
        }
    }
}