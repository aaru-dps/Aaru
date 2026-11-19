// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MiniDisc File System plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class MDFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        uint sectorsPerBlock = GetSectorsPerBlock(imagePlugin);

        if(sectorsPerBlock == 0) return false;

        if((partition.End - partition.Start + 1) / sectorsPerBlock <= DEFAULT_VD_LBA) return false;

        return FindVolumeDescriptor(imagePlugin, partition, sectorsPerBlock, out _, out _);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        uint sectorsPerBlock = GetSectorsPerBlock(imagePlugin);

        if(sectorsPerBlock == 0) return;

        if(!FindVolumeDescriptor(imagePlugin, partition, sectorsPerBlock, out VolumeDescriptor vd, out ulong lba))
            return;


        AaruLogging.Debug(MODULE_NAME, "Volume descriptor found at sector {0}", lba);
        AaruLogging.Debug(MODULE_NAME, "vd.ClusterSize = {0}",     vd.ClusterSize);
        AaruLogging.Debug(MODULE_NAME, "vd.NumRecordable = {0}",   vd.NumRecordable);
        AaruLogging.Debug(MODULE_NAME, "vd.MaxIdNum = {0}",        vd.MaxIdNum);
        AaruLogging.Debug(MODULE_NAME, "vd.VolAttr = 0x{0:X4}",    vd.VolAttr);
        AaruLogging.Debug(MODULE_NAME, "vd.VsbLoc = {0}",          vd.VsbLoc);
        AaruLogging.Debug(MODULE_NAME, "vd.VsbNum = {0}",          vd.VsbNum);
        AaruLogging.Debug(MODULE_NAME, "vd.MtbLoc = {0}",          vd.MtbLoc);
        AaruLogging.Debug(MODULE_NAME, "vd.MtbNum = {0}",          vd.MtbNum);
        AaruLogging.Debug(MODULE_NAME, "vd.ErbLoc = {0}",          vd.ErbLoc);
        AaruLogging.Debug(MODULE_NAME, "vd.ErbNum = {0}",          vd.ErbNum);
        AaruLogging.Debug(MODULE_NAME, "vd.DrbLoc = {0}",          vd.DrbLoc);
        AaruLogging.Debug(MODULE_NAME, "vd.DrbNum = {0}",          vd.DrbNum);
        AaruLogging.Debug(MODULE_NAME, "vd.DirLen = {0}",          vd.DirLen);
        AaruLogging.Debug(MODULE_NAME, "vd.NumChild = {0}",        vd.NumChild);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(Localization.MDFS_Name);
        sb.AppendFormat(Localization.Volume_version_0, vd.Version).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_sector, vd.BlockSize).AppendLine();

        sb.AppendFormat(Localization.MDFS_0_allocation_units_of_1_sectors_2_bytes,
                        vd.NumAlloc,
                        vd.AllocSize,
                        vd.AllocationUnitBytes)
          .AppendLine();

        sb.AppendFormat(Localization.MDFS_0_free_1_used_2_defective_allocation_units,
                        vd.NumAvailable,
                        vd.NumUsed,
                        vd.NumDefective)
          .AppendLine();

        sb.AppendFormat(Localization.MDFS_0_directories_and_1_files, vd.NumDir, vd.NumFile).AppendLine();

        sb.AppendFormat(Localization.MDFS_Volume_management_area_at_sector_0_1_bytes, vd.VmaLoc, vd.VmaLen)
          .AppendLine();

        if(vd.Mirrored) sb.AppendLine(Localization.MDFS_Volume_management_area_is_mirrored);

        if((vd.VolAttr & VOL_PROTECT) != 0) sb.AppendLine(Localization.MDFS_Volume_is_write_protected);

        if(vd.WideExtents) sb.AppendLine(Localization.MDFS_Volume_uses_32_bit_extent_records);

        string label        = DecodeString(vd.VolId,  vd.VolIdCharset,  encoding);
        string boot         = DecodeString(vd.BootId, vd.BootIdCharset, encoding);
        string publisher    = DecodeString(vd.PubId,  vd.PubIdCharset,  encoding);
        string dataPreparer = DecodeString(vd.PrepId, vd.PrepIdCharset, encoding);
        string application  = DecodeString(vd.ApplId, vd.ApplIdCharset, encoding);

        if(label        != "") sb.AppendFormat(Localization.Volume_label_0,             label).AppendLine();
        if(boot         != "") sb.AppendFormat(Localization.System_identifier_0,        boot).AppendLine();
        if(publisher    != "") sb.AppendFormat(Localization.Publisher_identifier_0,     publisher).AppendLine();
        if(dataPreparer != "") sb.AppendFormat(Localization.Data_preparer_identifier_0, dataPreparer).AppendLine();
        if(application  != "") sb.AppendFormat(Localization.Application_identifier_0,   application).AppendLine();

        DateTime? created   = ToDateTime(vd.CrTime);
        DateTime? modified  = ToDateTime(vd.MoTime);
        DateTime? effective = ToDateTime(vd.EfTime);
        DateTime? expires   = ToDateTime(vd.ExTime);

        if(created  is not null) sb.AppendFormat(Localization.Volume_created_on_0,       created).AppendLine();
        if(modified is not null) sb.AppendFormat(Localization.Volume_last_modified_on_0, modified).AppendLine();
        if(effective is not null) sb.AppendFormat(Localization.Volume_effective_date_0,  effective).AppendLine();
        if(expires  is not null) sb.AppendFormat(Localization.Volume_expiration_date_0,  expires).AppendLine();

        metadata = BuildMetadata(vd, encoding);

        information = sb.ToString();
    }

#endregion

    /// <summary>Builds the filesystem metadata from a volume descriptor</summary>
    static FileSystem BuildMetadata(in VolumeDescriptor vd, Encoding encoding)
    {
        string label        = DecodeString(vd.VolId,  vd.VolIdCharset,  encoding);
        string boot         = DecodeString(vd.BootId, vd.BootIdCharset, encoding);
        string publisher    = DecodeString(vd.PubId,  vd.PubIdCharset,  encoding);
        string dataPreparer = DecodeString(vd.PrepId, vd.PrepIdCharset, encoding);
        string application  = DecodeString(vd.ApplId, vd.ApplIdCharset, encoding);

        return new FileSystem
        {
            Type                   = FS_TYPE,
            Clusters               = vd.NumAlloc,
            ClusterSize            = vd.AllocationUnitBytes,
            FreeClusters           = vd.NumAvailable,
            Files                  = vd.NumFile,
            VolumeName             = label        == "" ? null : label,
            SystemIdentifier       = boot         == "" ? null : boot,
            PublisherIdentifier    = publisher    == "" ? null : publisher,
            DataPreparerIdentifier = dataPreparer == "" ? null : dataPreparer,
            ApplicationIdentifier  = application  == "" ? null : application,
            CreationDate           = ToDateTime(vd.CrTime),
            ModificationDate       = ToDateTime(vd.MoTime),
            EffectiveDate          = ToDateTime(vd.EfTime),
            ExpirationDate         = ToDateTime(vd.ExTime)
        };
    }
}