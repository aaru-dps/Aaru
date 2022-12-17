// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
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
using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
public sealed partial class AppleHFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        byte[]      mdbSector;
        ushort      drSigWord;
        ErrorNumber errno;

        if(imagePlugin.Info.SectorSize is 2352 or 2448 or 2048)
        {
            errno = imagePlugin.ReadSectors(partition.Start, 2, out mdbSector);

            if(errno != ErrorNumber.NoError)
                return false;

            foreach(int offset in new[]
                    {
                        0, 0x200, 0x400, 0x600, 0x800, 0xA00
                    }.Where(offset => mdbSector.Length >= offset + 0x7C + 2))
            {
                drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, offset);

                if(drSigWord != AppleCommon.HFS_MAGIC)
                    continue;

                drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, offset + 0x7C); // Seek to embedded HFS+ signature

                return drSigWord != AppleCommon.HFSP_MAGIC;
            }
        }
        else
        {
            errno = imagePlugin.ReadSector(2 + partition.Start, out mdbSector);

            if(errno != ErrorNumber.NoError)
                return false;

            if(mdbSector.Length < 0x7C + 2)
                return false;

            drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0);

            if(drSigWord != AppleCommon.HFS_MAGIC)
                return false;

            drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0x7C); // Seek to embedded HFS+ signature

            return drSigWord != AppleCommon.HFSP_MAGIC;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("macintosh");
        information =   "";
        metadata    =   new FileSystem();

        var sb = new StringBuilder();

        byte[]      bbSector  = null;
        byte[]      mdbSector = null;
        ushort      drSigWord;
        ErrorNumber errno;

        bool apmFromHddOnCd = false;

        if(imagePlugin.Info.SectorSize is 2352 or 2448 or 2048)
        {
            errno = imagePlugin.ReadSectors(partition.Start, 2, out byte[] tmpSector);

            if(errno != ErrorNumber.NoError)
                return;

            foreach(int offset in new[]
                    {
                        0, 0x200, 0x400, 0x600, 0x800, 0xA00
                    })
            {
                drSigWord = BigEndianBitConverter.ToUInt16(tmpSector, offset);

                if(drSigWord != AppleCommon.HFS_MAGIC)
                    continue;

                bbSector  = new byte[1024];
                mdbSector = new byte[512];

                if(offset >= 0x400)
                    Array.Copy(tmpSector, offset - 0x400, bbSector, 0, 1024);

                Array.Copy(tmpSector, offset, mdbSector, 0, 512);
                apmFromHddOnCd = true;

                break;
            }

            if(!apmFromHddOnCd)
                return;
        }
        else
        {
            errno = imagePlugin.ReadSector(2 + partition.Start, out mdbSector);

            if(errno != ErrorNumber.NoError)
                return;

            drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0);

            if(drSigWord == AppleCommon.HFS_MAGIC)
            {
                errno = imagePlugin.ReadSector(partition.Start, out bbSector);

                if(errno != ErrorNumber.NoError)
                    return;
            }
            else
                return;
        }

        MasterDirectoryBlock mdb = Marshal.ByteArrayToStructureBigEndian<MasterDirectoryBlock>(mdbSector);

        sb.AppendLine(Localization.Name_Apple_Hierarchical_File_System);
        sb.AppendLine();

        if(apmFromHddOnCd)
            sb.AppendLine(Localization.HFS_uses_512_bytes_sector_while_device_uses_2048_bytes_sector).AppendLine();

        sb.AppendLine(Localization.Master_Directory_Block);
        sb.AppendFormat(Localization.Creation_date_0, DateHandlers.MacToDateTime(mdb.drCrDate)).AppendLine();
        sb.AppendFormat(Localization.Last_modification_date_0, DateHandlers.MacToDateTime(mdb.drLsMod)).AppendLine();

        if(mdb.drVolBkUp > 0)
        {
            sb.AppendFormat(Localization.Last_backup_date_0, DateHandlers.MacToDateTime(mdb.drVolBkUp)).AppendLine();
            sb.AppendFormat(Localization.Backup_sequence_number_0, mdb.drVSeqNum).AppendLine();
        }
        else
            sb.AppendLine(Localization.Volume_has_never_been_backed_up);

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.HardwareLock))
            sb.AppendLine(Localization.Volume_is_locked_by_hardware);

        sb.AppendLine(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Unmounted) ? Localization.Volume_was_unmonted
                          : Localization.Volume_is_mounted);

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.SparedBadBlocks))
            sb.AppendLine(Localization.Volume_has_spared_bad_blocks);

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.DoesNotNeedCache))
            sb.AppendLine(Localization.Volume_does_not_need_cache);

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.BootInconsistent))
            sb.AppendLine(Localization.Boot_volume_is_inconsistent);

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.ReusedIds))
            sb.AppendLine(Localization.There_are_reused_CNIDs);

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Journaled))
            sb.AppendLine(Localization.Volume_is_journaled);

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Inconsistent))
            sb.AppendLine(Localization.Volume_is_seriously_inconsistent);

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.SoftwareLock))
            sb.AppendLine(Localization.Volume_is_locked_by_software);

        sb.AppendFormat(Localization._0_files_on_root_directory, mdb.drNmFls).AppendLine();
        sb.AppendFormat(Localization._0_directories_on_root_directory, mdb.drNmRtDirs).AppendLine();
        sb.AppendFormat(Localization._0_files_on_volume, mdb.drFilCnt).AppendLine();
        sb.AppendFormat(Localization._0_directories_on_volume, mdb.drDirCnt).AppendLine();
        sb.AppendFormat(Localization.Volume_write_count_0, mdb.drWrCnt).AppendLine();

        sb.AppendFormat(Localization.Volume_bitmap_starting_sector_in_512_bytes_0, mdb.drVBMSt).AppendLine();
        sb.AppendFormat(Localization.Next_allocation_block_0, mdb.drAllocPtr).AppendLine();
        sb.AppendFormat(Localization._0_volume_allocation_blocks, mdb.drNmAlBlks).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_allocation_block, mdb.drAlBlkSiz).AppendLine();
        sb.AppendFormat(Localization._0_bytes_to_allocate_when_extending_a_file, mdb.drClpSiz).AppendLine();
        sb.AppendFormat(Localization._0_bytes_to_allocate_when_extending_a_Extents_B_Tree, mdb.drXTClpSiz).AppendLine();
        sb.AppendFormat(Localization._0_bytes_to_allocate_when_extending_a_Catalog_B_Tree, mdb.drCTClpSiz).AppendLine();
        sb.AppendFormat(Localization.Sector_of_first_allocation_block_0, mdb.drAlBlSt).AppendLine();
        sb.AppendFormat(Localization.Next_unused_CNID_0, mdb.drNxtCNID).AppendLine();
        sb.AppendFormat(Localization._0_unused_allocation_blocks, mdb.drFreeBks).AppendLine();

        sb.AppendFormat(Localization._0_bytes_in_the_Extents_B_Tree, mdb.drXTFlSize).AppendLine();
        sb.AppendFormat(Localization._0_bytes_in_the_Catalog_B_Tree, mdb.drCTFlSize).AppendLine();

        sb.AppendFormat(Localization.Volume_name_0, StringHandlers.PascalToString(mdb.drVN, encoding)).AppendLine();

        sb.AppendLine(Localization.Finder_info);
        sb.AppendFormat(Localization.CNID_of_bootable_system_directory_0, mdb.drFndrInfo0).AppendLine();
        sb.AppendFormat(Localization.CNID_of_first_run_application_directory_0, mdb.drFndrInfo1).AppendLine();
        sb.AppendFormat(Localization.CNID_of_previously_opened_directory_0, mdb.drFndrInfo2).AppendLine();
        sb.AppendFormat(Localization.CNID_of_bootable_Mac_OS_8_or_9_directory_0, mdb.drFndrInfo3).AppendLine();
        sb.AppendFormat(Localization.CNID_of_bootable_Mac_OS_X_directory_0, mdb.drFndrInfo5).AppendLine();

        if(mdb.drFndrInfo6 != 0 &&
           mdb.drFndrInfo7 != 0)
            sb.AppendFormat(Localization.Mac_OS_X_Volume_ID_0_1, mdb.drFndrInfo6, mdb.drFndrInfo7).AppendLine();

        if(mdb.drEmbedSigWord == AppleCommon.HFSP_MAGIC)
        {
            sb.AppendLine(Localization.Volume_wraps_a_HFS_Plus_volume);
            sb.AppendFormat(Localization.Starting_block_of_the_HFS_Plus_volume_0, mdb.xdrStABNt).AppendLine();
            sb.AppendFormat(Localization.Allocations_blocks_of_the_HFS_Plus_volume_0, mdb.xdrNumABlks).AppendLine();
        }
        else
        {
            sb.AppendFormat(Localization._0_blocks_in_volume_cache, mdb.drVCSize).AppendLine();
            sb.AppendFormat(Localization._0_blocks_in_volume_bitmap_cache, mdb.drVBMCSize).AppendLine();
            sb.AppendFormat(Localization._0_blocks_in_volume_common_cache, mdb.drCtlCSize).AppendLine();
        }

        string bootBlockInfo = AppleCommon.GetBootBlockInformation(bbSector, encoding);

        if(bootBlockInfo != null)
        {
            sb.AppendLine(Localization.Volume_is_bootable);
            sb.AppendLine();
            sb.AppendLine(bootBlockInfo);
        }
        else if(mdb.drFndrInfo0 != 0 ||
                mdb.drFndrInfo3 != 0 ||
                mdb.drFndrInfo5 != 0)
            sb.AppendLine(Localization.Volume_is_bootable);
        else
            sb.AppendLine(Localization.Volume_is_not_bootable);

        information = sb.ToString();

        metadata = new FileSystem();

        if(mdb.drVolBkUp > 0)
        {
            metadata.BackupDate = DateHandlers.MacToDateTime(mdb.drVolBkUp);
        }

        metadata.Bootable = bootBlockInfo   != null || mdb.drFndrInfo0 != 0 || mdb.drFndrInfo3 != 0 ||
                            mdb.drFndrInfo5 != 0;

        metadata.Clusters    = mdb.drNmAlBlks;
        metadata.ClusterSize = mdb.drAlBlkSiz;

        if(mdb.drCrDate > 0)
        {
            metadata.CreationDate = DateHandlers.MacToDateTime(mdb.drCrDate);
        }

        metadata.Dirty        = !mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Unmounted);
        metadata.Files        = mdb.drFilCnt;
        metadata.FreeClusters = mdb.drFreeBks;

        if(mdb.drLsMod > 0)
        {
            metadata.ModificationDate = DateHandlers.MacToDateTime(mdb.drLsMod);
        }

        metadata.Type       = FS_TYPE;
        metadata.VolumeName = StringHandlers.PascalToString(mdb.drVN, encoding);

        if(mdb.drFndrInfo6 != 0 &&
           mdb.drFndrInfo7 != 0)
            metadata.VolumeSerial = $"{mdb.drFndrInfo6:X8}{mdb.drFndrInfo7:X8}";
    }
}