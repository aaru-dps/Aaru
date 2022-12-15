// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Files-11 On-Disk Structure and shows information.
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
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from VMS File System Internals by Kirby McCoy
// ISBN: 1-55558-056-4
// With some hints from http://www.decuslib.com/DECUS/vmslt97b/gnusoftware/gccaxp/7_1/vms/hm2def.h
// Expects the home block to be always in sector #1 (does not check deltas)
// Assumes a sector size of 512 bytes (VMS does on HDDs and optical drives, dunno about M.O.)
// Book only describes ODS-2. Need to test ODS-1 and ODS-5
// There is an ODS with signature "DECFILES11A", yet to be seen
// Time is a 64 bit unsigned integer, tenths of microseconds since 1858/11/17 00:00:00.
// TODO: Implement checksum
/// <inheritdoc />
/// <summary>Implements detection of DEC's On-Disk Structure, aka the ODS filesystem</summary>
public sealed partial class ODS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        if(imagePlugin.Info.SectorSize < 512)
            return false;

        byte[]      magicB = new byte[12];
        ErrorNumber errno  = imagePlugin.ReadSector(1 + partition.Start, out byte[] hbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        Array.Copy(hbSector, 0x1F0, magicB, 0, 12);
        string magic = Encoding.ASCII.GetString(magicB);

        AaruConsole.DebugWriteLine("Files-11 plugin", Localization.magic_0, magic);

        if(magic is "DECFILE11A  " or "DECFILE11B  ")
            return true;

        // Optical disc
        if(imagePlugin.Info.MetadataMediaType != MetadataMediaType.OpticalDisc)
            return false;

        if(hbSector.Length < 0x400)
            return false;

        errno = imagePlugin.ReadSector(partition.Start, out hbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        Array.Copy(hbSector, 0x3F0, magicB, 0, 12);
        magic = Encoding.ASCII.GetString(magicB);

        AaruConsole.DebugWriteLine("Files-11 plugin", Localization.unaligned_magic_0, magic);

        return magic is "DECFILE11A  " or "DECFILE11B  ";
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");
        information = "";

        var sb = new StringBuilder();

        ErrorNumber errno = imagePlugin.ReadSector(1 + partition.Start, out byte[] hbSector);

        if(errno != ErrorNumber.NoError)
            return;

        HomeBlock homeblock = Marshal.ByteArrayToStructureLittleEndian<HomeBlock>(hbSector);

        // Optical disc
        if(imagePlugin.Info.MetadataMediaType         == MetadataMediaType.OpticalDisc &&
           StringHandlers.CToString(homeblock.format) != "DECFILE11A  "                &&
           StringHandlers.CToString(homeblock.format) != "DECFILE11B  ")
        {
            if(hbSector.Length < 0x400)
                return;

            errno = imagePlugin.ReadSector(partition.Start, out byte[] tmp);

            if(errno != ErrorNumber.NoError)
                return;

            hbSector = new byte[0x200];
            Array.Copy(tmp, 0x200, hbSector, 0, 0x200);

            homeblock = Marshal.ByteArrayToStructureLittleEndian<HomeBlock>(hbSector);

            if(StringHandlers.CToString(homeblock.format) != "DECFILE11A  " &&
               StringHandlers.CToString(homeblock.format) != "DECFILE11B  ")
                return;
        }

        if((homeblock.struclev & 0xFF00)              != 0x0200 ||
           (homeblock.struclev & 0xFF)                != 1      ||
           StringHandlers.CToString(homeblock.format) != "DECFILE11B  ")
            sb.AppendLine(Localization.The_following_information_may_be_incorrect_for_this_volume);

        if(homeblock.resfiles < 5 ||
           homeblock.devtype  != 0)
            sb.AppendLine(Localization.This_volume_may_be_corrupted);

        sb.AppendFormat(Localization.Volume_format_is_0,
                        StringHandlers.SpacePaddedToString(homeblock.format, Encoding)).AppendLine();

        sb.AppendFormat(Localization.Volume_is_Level_0_revision_1, (homeblock.struclev & 0xFF00) >> 8,
                        homeblock.struclev & 0xFF).AppendLine();

        sb.AppendFormat(Localization.Lowest_structure_in_the_volume_is_Level_0_revision_1,
                        (homeblock.lowstruclev & 0xFF00) >> 8, homeblock.lowstruclev & 0xFF).AppendLine();

        sb.AppendFormat(Localization.Highest_structure_in_the_volume_is_Level_0_revision_1,
                        (homeblock.highstruclev & 0xFF00) >> 8, homeblock.highstruclev & 0xFF).AppendLine();

        sb.AppendFormat(Localization._0_sectors_per_cluster_1_bytes, homeblock.cluster, homeblock.cluster * 512).
           AppendLine();

        sb.AppendFormat(Localization.This_home_block_is_on_sector_0_VBN_1, homeblock.homelbn, homeblock.homevbn).
           AppendLine();

        sb.AppendFormat(Localization.Secondary_home_block_is_on_sector_0_VBN_1, homeblock.alhomelbn,
                        homeblock.alhomevbn).AppendLine();

        sb.AppendFormat(Localization.Volume_bitmap_starts_in_sector_0_VBN_1, homeblock.ibmaplbn, homeblock.ibmapvbn).
           AppendLine();

        sb.AppendFormat(Localization.Volume_bitmap_runs_for_0_sectors_1_bytes, homeblock.ibmapsize,
                        homeblock.ibmapsize * 512).AppendLine();

        sb.AppendFormat(Localization.Backup_INDEXF_SYS_is_in_sector_0_VBN_1, homeblock.altidxlbn, homeblock.altidxvbn).
           AppendLine();

        sb.AppendFormat(Localization._0_maximum_files_on_the_volume, homeblock.maxfiles).AppendLine();
        sb.AppendFormat(Localization._0_reserved_files, homeblock.resfiles).AppendLine();

        if(homeblock is { rvn: > 0, setcount: > 0 } &&
           StringHandlers.CToString(homeblock.strucname) != "            ")
            sb.AppendFormat(Localization.Volume_is_0_of_1_in_set_2, homeblock.rvn, homeblock.setcount,
                            StringHandlers.SpacePaddedToString(homeblock.strucname, Encoding)).AppendLine();

        sb.AppendFormat(Localization.Volume_owner_is_0_ID_1,
                        StringHandlers.SpacePaddedToString(homeblock.ownername, Encoding), homeblock.volowner).
           AppendLine();

        sb.AppendFormat(Localization.Volume_label_0, StringHandlers.SpacePaddedToString(homeblock.volname, Encoding)).
           AppendLine();

        sb.AppendFormat(Localization.Drive_serial_number_0, homeblock.serialnum).AppendLine();

        sb.AppendFormat(Localization.Volume_was_created_on_0, DateHandlers.VmsToDateTime(homeblock.credate)).
           AppendLine();

        if(homeblock.revdate > 0)
            sb.AppendFormat(Localization.Volume_was_last_modified_on_0, DateHandlers.VmsToDateTime(homeblock.revdate)).
               AppendLine();

        if(homeblock.copydate > 0)
            sb.AppendFormat(Localization.Volume_copied_on_0, DateHandlers.VmsToDateTime(homeblock.copydate)).
               AppendLine();

        sb.AppendFormat(Localization.Checksums_0_and_1, homeblock.checksum1, homeblock.checksum2).AppendLine();
        sb.AppendLine(Localization.Flags);
        sb.AppendFormat(Localization.Window_0, homeblock.window).AppendLine();
        sb.AppendFormat(Localization.Cached_directories_0, homeblock.lru_lim).AppendLine();
        sb.AppendFormat(Localization.Default_allocation_0_blocks, homeblock.extend).AppendLine();

        if((homeblock.volchar & 0x01) == 0x01)
            sb.AppendLine(Localization.Readings_should_be_verified);

        if((homeblock.volchar & 0x02) == 0x02)
            sb.AppendLine(Localization.Writings_should_be_verified);

        if((homeblock.volchar & 0x04) == 0x04)
            sb.AppendLine(Localization.Files_should_be_erased_or_overwritten_when_deleted);

        if((homeblock.volchar & 0x08) == 0x08)
            sb.AppendLine(Localization.Highwater_mark_is_to_be_disabled);

        if((homeblock.volchar & 0x10) == 0x10)
            sb.AppendLine(Localization.Classification_checks_are_enabled);

        sb.AppendLine(Localization.Volume_permissions_r_read_w_write_c_create_d_delete);
        sb.AppendLine(Localization.System_owner_group_world);

        // System
        sb.Append((homeblock.protect & 0x1000) == 0x1000 ? "-" : "r");
        sb.Append((homeblock.protect & 0x2000) == 0x2000 ? "-" : "w");
        sb.Append((homeblock.protect & 0x4000) == 0x4000 ? "-" : "c");
        sb.Append((homeblock.protect & 0x8000) == 0x8000 ? "-" : "d");

        // Owner
        sb.Append((homeblock.protect & 0x100) == 0x100 ? "-" : "r");
        sb.Append((homeblock.protect & 0x200) == 0x200 ? "-" : "w");
        sb.Append((homeblock.protect & 0x400) == 0x400 ? "-" : "c");
        sb.Append((homeblock.protect & 0x800) == 0x800 ? "-" : "d");

        // Group
        sb.Append((homeblock.protect & 0x10) == 0x10 ? "-" : "r");
        sb.Append((homeblock.protect & 0x20) == 0x20 ? "-" : "w");
        sb.Append((homeblock.protect & 0x40) == 0x40 ? "-" : "c");
        sb.Append((homeblock.protect & 0x80) == 0x80 ? "-" : "d");

        // World (other)
        sb.Append((homeblock.protect & 0x1) == 0x1 ? "-" : "r");
        sb.Append((homeblock.protect & 0x2) == 0x2 ? "-" : "w");
        sb.Append((homeblock.protect & 0x4) == 0x4 ? "-" : "c");
        sb.Append((homeblock.protect & 0x8) == 0x8 ? "-" : "d");

        sb.AppendLine();

        sb.AppendLine(Localization.Unknown_structures);
        sb.AppendFormat(Localization.Security_mask_0, homeblock.sec_mask).AppendLine();
        sb.AppendFormat(Localization.File_protection_0, homeblock.fileprot).AppendLine();
        sb.AppendFormat(Localization.Record_protection_0, homeblock.recprot).AppendLine();

        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            ClusterSize  = (uint)(homeblock.cluster * 512),
            Clusters     = partition.Size / (ulong)(homeblock.cluster * 512),
            VolumeName   = StringHandlers.SpacePaddedToString(homeblock.volname, Encoding),
            VolumeSerial = $"{homeblock.serialnum:X8}"
        };

        if(homeblock.credate > 0)
        {
            Metadata.CreationDate = DateHandlers.VmsToDateTime(homeblock.credate);
        }

        if(homeblock.revdate > 0)
        {
            Metadata.ModificationDate = DateHandlers.VmsToDateTime(homeblock.revdate);
        }

        information = sb.ToString();
    }
}