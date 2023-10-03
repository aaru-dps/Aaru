// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System Plus plugin.
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
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from Apple TechNote 1150: https://developer.apple.com/legacy/library/technotes/tn/tn1150.html
/// <inheritdoc />
/// <summary>Implements detection of Apple Hierarchical File System Plus (HFS+)</summary>
public sealed partial class AppleHFSPlus
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        ulong hfspOffset;

        uint sectorsToRead = 0x800 / imagePlugin.Info.SectorSize;

        if(0x800 % imagePlugin.Info.SectorSize > 0)
            sectorsToRead++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sectorsToRead, out byte[] vhSector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(vhSector.Length < 0x800)
            return false;

        var drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x400);

        if(drSigWord == AppleCommon.HFS_MAGIC) // "BD"
        {
            drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x47C); // Read embedded HFS+ signature

            if(drSigWord == AppleCommon.HFSP_MAGIC) // "H+"
            {
                // ReSharper disable once InconsistentNaming
                var xdrStABNt = BigEndianBitConverter.ToUInt16(vhSector, 0x47E);

                var drAlBlkSiz = BigEndianBitConverter.ToUInt32(vhSector, 0x414);

                var drAlBlSt = BigEndianBitConverter.ToUInt16(vhSector, 0x41C);

                hfspOffset = (ulong)((drAlBlSt * 512 + xdrStABNt * drAlBlkSiz) / imagePlugin.Info.SectorSize);
            }
            else
                hfspOffset = 0;
        }
        else
            hfspOffset = 0;

        errno = imagePlugin.ReadSectors(partition.Start + hfspOffset, sectorsToRead,
                                        out vhSector); // Read volume header

        if(errno != ErrorNumber.NoError)
            return false;

        drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x400);

        return drSigWord is AppleCommon.HFSP_MAGIC or AppleCommon.HFSX_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        var vh = new VolumeHeader();

        ulong hfspOffset;
        bool  wrapped;

        uint sectorsToRead = 0x800 / imagePlugin.Info.SectorSize;

        if(0x800 % imagePlugin.Info.SectorSize > 0)
            sectorsToRead++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sectorsToRead, out byte[] vhSector);

        if(errno != ErrorNumber.NoError)
            return;

        var drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x400);

        if(drSigWord == AppleCommon.HFS_MAGIC) // "BD"
        {
            drSigWord = BigEndianBitConverter.ToUInt16(vhSector, 0x47C); // Read embedded HFS+ signature

            if(drSigWord == AppleCommon.HFSP_MAGIC) // "H+"
            {
                // ReSharper disable once InconsistentNaming
                var xdrStABNt = BigEndianBitConverter.ToUInt16(vhSector, 0x47E);

                var drAlBlkSiz = BigEndianBitConverter.ToUInt32(vhSector, 0x414);

                var drAlBlSt = BigEndianBitConverter.ToUInt16(vhSector, 0x41C);

                hfspOffset = (ulong)((drAlBlSt * 512 + xdrStABNt * drAlBlkSiz) / imagePlugin.Info.SectorSize);
                wrapped    = true;
            }
            else
            {
                hfspOffset = 0;
                wrapped    = false;
            }
        }
        else
        {
            hfspOffset = 0;
            wrapped    = false;
        }

        errno = imagePlugin.ReadSectors(partition.Start + hfspOffset, sectorsToRead,
                                        out vhSector); // Read volume header

        if(errno != ErrorNumber.NoError)
            return;

        vh.signature = BigEndianBitConverter.ToUInt16(vhSector, 0x400);

        if(vh.signature != AppleCommon.HFSP_MAGIC &&
           vh.signature != AppleCommon.HFSX_MAGIC)
            return;

        var sb = new StringBuilder();

        switch(vh.signature)
        {
            case 0x482B:
                sb.AppendLine(Localization.HFS_filesystem);

                break;
            case 0x4858:
                sb.AppendLine(Localization.HFSX_filesystem);

                break;
        }

        if(wrapped)
            sb.AppendLine(Localization.Volume_is_wrapped_inside_an_HFS_volume);

        var tmp = new byte[0x400];
        Array.Copy(vhSector, 0x400, tmp, 0, 0x400);
        vhSector = tmp;

        vh = Marshal.ByteArrayToStructureBigEndian<VolumeHeader>(vhSector);

        if(vh.version is 4 or 5)
        {
            sb.AppendFormat(Localization.Filesystem_version_is_0, vh.version).AppendLine();

            if((vh.attributes & 0x80) == 0x80)
                sb.AppendLine(Localization.Volume_is_locked_by_hardware);

            if((vh.attributes & 0x100) == 0x100)
                sb.AppendLine(Localization.Volume_is_unmounted);

            if((vh.attributes & 0x200) == 0x200)
                sb.AppendLine(Localization.There_are_bad_blocks_in_the_extents_file);

            if((vh.attributes & 0x400) == 0x400)
                sb.AppendLine(Localization.Volume_does_not_need_cache);

            if((vh.attributes & 0x800) == 0x800)
                sb.AppendLine(Localization.Volume_state_is_inconsistent);

            if((vh.attributes & 0x1000) == 0x1000)
                sb.AppendLine(Localization.There_are_reused_CNIDs);

            if((vh.attributes & 0x2000) == 0x2000)
                sb.AppendLine(Localization.Volume_is_journaled);

            if((vh.attributes & 0x8000) == 0x8000)
                sb.AppendLine(Localization.Volume_is_locked_by_software);

            sb.AppendFormat(Localization.Implementation_that_last_mounted_the_volume_0,
                            Encoding.ASCII.GetString(vh.lastMountedVersion)).AppendLine();

            if((vh.attributes & 0x2000) == 0x2000)
                sb.AppendFormat(Localization.Journal_starts_at_allocation_block_0, vh.journalInfoBlock).AppendLine();

            sb.AppendFormat(Localization.Creation_date_0, DateHandlers.MacToDateTime(vh.createDate)).AppendLine();

            sb.AppendFormat(Localization.Last_modification_date_0, DateHandlers.MacToDateTime(vh.modifyDate)).
               AppendLine();

            if(vh.backupDate > 0)
            {
                sb.AppendFormat(Localization.Last_backup_date_0, DateHandlers.MacToDateTime(vh.backupDate)).
                   AppendLine();
            }
            else
                sb.AppendLine(Localization.Volume_has_never_been_backed_up);

            if(vh.backupDate > 0)
            {
                sb.AppendFormat(Localization.Last_check_date_0, DateHandlers.MacToDateTime(vh.checkedDate)).
                   AppendLine();
            }
            else
                sb.AppendLine(Localization.Volume_has_never_been_checked_up);

            sb.AppendFormat(Localization._0_files_in_volume, vh.fileCount).AppendLine();
            sb.AppendFormat(Localization._0_directories_in_volume, vh.folderCount).AppendLine();
            sb.AppendFormat(Localization._0_bytes_per_allocation_block, vh.blockSize).AppendLine();
            sb.AppendFormat(Localization._0_allocation_blocks, vh.totalBlocks).AppendLine();
            sb.AppendFormat(Localization._0_free_blocks, vh.freeBlocks).AppendLine();
            sb.AppendFormat(Localization.Next_allocation_block_0, vh.nextAllocation).AppendLine();
            sb.AppendFormat(Localization.Resource_fork_clump_size_0_bytes, vh.rsrcClumpSize).AppendLine();
            sb.AppendFormat(Localization.Data_fork_clump_size_0_bytes, vh.dataClumpSize).AppendLine();
            sb.AppendFormat(Localization.Next_unused_CNID_0, vh.nextCatalogID).AppendLine();
            sb.AppendFormat(Localization.Volume_has_been_mounted_writable_0_times, vh.writeCount).AppendLine();
            sb.AppendFormat(Localization.Allocation_File_is_0_bytes, vh.allocationFile_logicalSize).AppendLine();
            sb.AppendFormat(Localization.Extents_File_is_0_bytes, vh.extentsFile_logicalSize).AppendLine();
            sb.AppendFormat(Localization.Catalog_File_is_0_bytes, vh.catalogFile_logicalSize).AppendLine();
            sb.AppendFormat(Localization.Attributes_File_is_0_bytes, vh.attributesFile_logicalSize).AppendLine();
            sb.AppendFormat(Localization.Startup_File_is_0_bytes, vh.startupFile_logicalSize).AppendLine();
            sb.AppendLine(Localization.Finder_info);
            sb.AppendFormat(Localization.CNID_of_bootable_system_directory_0,        vh.drFndrInfo0).AppendLine();
            sb.AppendFormat(Localization.CNID_of_first_run_application_directory_0,  vh.drFndrInfo1).AppendLine();
            sb.AppendFormat(Localization.CNID_of_previously_opened_directory_0,      vh.drFndrInfo2).AppendLine();
            sb.AppendFormat(Localization.CNID_of_bootable_Mac_OS_8_or_9_directory_0, vh.drFndrInfo3).AppendLine();
            sb.AppendFormat(Localization.CNID_of_bootable_Mac_OS_X_directory_0,      vh.drFndrInfo5).AppendLine();

            if(vh.drFndrInfo6 != 0 &&
               vh.drFndrInfo7 != 0)
                sb.AppendFormat(Localization.Mac_OS_X_Volume_ID_0_1, vh.drFndrInfo6, vh.drFndrInfo7).AppendLine();

            metadata = new FileSystem();

            if(vh.backupDate > 0)
                metadata.BackupDate = DateHandlers.MacToDateTime(vh.backupDate);

            metadata.Bootable    |= vh.drFndrInfo0 != 0 || vh.drFndrInfo3 != 0 || vh.drFndrInfo5 != 0;
            metadata.Clusters    =  vh.totalBlocks;
            metadata.ClusterSize =  vh.blockSize;

            if(vh.createDate > 0)
                metadata.CreationDate = DateHandlers.MacToDateTime(vh.createDate);

            metadata.Dirty        = (vh.attributes & 0x100) != 0x100;
            metadata.Files        = vh.fileCount;
            metadata.FreeClusters = vh.freeBlocks;

            if(vh.modifyDate > 0)
                metadata.ModificationDate = DateHandlers.MacToDateTime(vh.modifyDate);

            metadata.Type = vh.signature switch
                            {
                                0x482B => FS_TYPE_HFSP,
                                0x4858 => FS_TYPE_HFSX,
                                _      => metadata.Type
                            };

            if(vh.drFndrInfo6 != 0 &&
               vh.drFndrInfo7 != 0)
                metadata.VolumeSerial = $"{vh.drFndrInfo6:X8}{vh.drFndrInfo7:X8}";

            metadata.SystemIdentifier = Encoding.ASCII.GetString(vh.lastMountedVersion);
        }
        else
        {
            sb.AppendFormat(Localization.Filesystem_version_is_0, vh.version).AppendLine();
            sb.AppendLine(Localization.This_version_is_not_supported_yet);
        }

        information = sb.ToString();
    }

#endregion
}