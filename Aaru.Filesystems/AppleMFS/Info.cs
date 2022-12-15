// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
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
using Claunia.Encoding;
using Encoding = System.Text.Encoding;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from Inside Macintosh Volume II
public sealed partial class AppleMFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(2 + partition.Start, out byte[] mdbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        ushort drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0x000);

        return drSigWord == MFS_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? new MacRoman();
        information = "";

        var sb = new StringBuilder();

        var mdb = new MasterDirectoryBlock();

        ErrorNumber errno = imagePlugin.ReadSector(2 + partition.Start, out byte[] mdbSector);

        if(errno != ErrorNumber.NoError)
            return;

        errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bbSector);

        if(errno != ErrorNumber.NoError)
            return;

        mdb.drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0x000);

        if(mdb.drSigWord != MFS_MAGIC)
            return;

        mdb.drCrDate   = BigEndianBitConverter.ToUInt32(mdbSector, 0x002);
        mdb.drLsBkUp   = BigEndianBitConverter.ToUInt32(mdbSector, 0x006);
        mdb.drAtrb     = (AppleCommon.VolumeAttributes)BigEndianBitConverter.ToUInt16(mdbSector, 0x00A);
        mdb.drNmFls    = BigEndianBitConverter.ToUInt16(mdbSector, 0x00C);
        mdb.drDirSt    = BigEndianBitConverter.ToUInt16(mdbSector, 0x00E);
        mdb.drBlLen    = BigEndianBitConverter.ToUInt16(mdbSector, 0x010);
        mdb.drNmAlBlks = BigEndianBitConverter.ToUInt16(mdbSector, 0x012);
        mdb.drAlBlkSiz = BigEndianBitConverter.ToUInt32(mdbSector, 0x014);
        mdb.drClpSiz   = BigEndianBitConverter.ToUInt32(mdbSector, 0x018);
        mdb.drAlBlSt   = BigEndianBitConverter.ToUInt16(mdbSector, 0x01C);
        mdb.drNxtFNum  = BigEndianBitConverter.ToUInt32(mdbSector, 0x01E);
        mdb.drFreeBks  = BigEndianBitConverter.ToUInt16(mdbSector, 0x022);
        mdb.drVNSiz    = mdbSector[0x024];
        byte[] variableSize = new byte[mdb.drVNSiz + 1];
        Array.Copy(mdbSector, 0x024, variableSize, 0, mdb.drVNSiz + 1);
        mdb.drVN = StringHandlers.PascalToString(variableSize, Encoding);

        sb.AppendLine(Localization.AppleMFS_Name);
        sb.AppendLine();
        sb.AppendLine(Localization.Master_Directory_Block);
        sb.AppendFormat(Localization.Creation_date_0, DateHandlers.MacToDateTime(mdb.drCrDate)).AppendLine();
        sb.AppendFormat(Localization.Last_backup_date_0, DateHandlers.MacToDateTime(mdb.drLsBkUp)).AppendLine();

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

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Inconsistent))
            sb.AppendLine(Localization.Volume_is_seriously_inconsistent);

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.SoftwareLock))
            sb.AppendLine(Localization.Volume_is_locked_by_software);

        sb.AppendFormat(Localization._0_files_on_volume, mdb.drNmFls).AppendLine();
        sb.AppendFormat(Localization.First_directory_sector_0, mdb.drDirSt).AppendLine();
        sb.AppendFormat(Localization._0_sectors_in_directory, mdb.drBlLen).AppendLine();
        sb.AppendFormat(Localization._0_volume_allocation_blocks, mdb.drNmAlBlks + 1).AppendLine();
        sb.AppendFormat(Localization.Size_of_allocation_blocks_0_bytes, mdb.drAlBlkSiz).AppendLine();
        sb.AppendFormat(Localization._0_bytes_to_allocate, mdb.drClpSiz).AppendLine();
        sb.AppendFormat(Localization.First_allocation_block_number_two_starts_in_sector_0, mdb.drAlBlSt).AppendLine();
        sb.AppendFormat(Localization.Next_unused_file_number_0, mdb.drNxtFNum).AppendLine();
        sb.AppendFormat(Localization._0_unused_allocation_blocks, mdb.drFreeBks).AppendLine();
        sb.AppendFormat(Localization.Volume_name_0, mdb.drVN).AppendLine();

        string bootBlockInfo = AppleCommon.GetBootBlockInformation(bbSector, Encoding);

        if(bootBlockInfo != null)
        {
            sb.AppendLine(Localization.Volume_is_bootable);
            sb.AppendLine();
            sb.AppendLine(bootBlockInfo);
        }
        else
            sb.AppendLine(Localization.Volume_is_not_bootable);

        information = sb.ToString();

        Metadata = new FileSystem();

        if(mdb.drLsBkUp > 0)
        {
            Metadata.BackupDate = DateHandlers.MacToDateTime(mdb.drLsBkUp);
        }

        Metadata.Bootable    = bootBlockInfo != null;
        Metadata.Clusters    = mdb.drNmAlBlks;
        Metadata.ClusterSize = mdb.drAlBlkSiz;

        if(mdb.drCrDate > 0)
        {
            Metadata.CreationDate = DateHandlers.MacToDateTime(mdb.drCrDate);
        }

        Metadata.Files        = mdb.drNmFls;
        Metadata.FreeClusters = mdb.drFreeBks;
        Metadata.Type         = FS_TYPE;
        Metadata.VolumeName   = mdb.drVN;
    }
}