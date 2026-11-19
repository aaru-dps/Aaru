// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
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

using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class HPOFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start + MEDINFO_SECTOR >= partition.End) return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, false, out byte[] bootSector, out _);

        if(errno != ErrorNumber.NoError || bootSector.Length < 512) return false;

        BiosParameterBlock bpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(bootSector);

        if(!bpb.fs_type.SequenceEqual(_type)) return false;

        errno = imagePlugin.ReadSector(partition.Start + MEDINFO_SECTOR, false, out byte[] medSector, out _);

        if(errno != ErrorNumber.NoError || medSector.Length < 8) return false;

        byte[] signature = medSector[..8];

        return signature.SequenceEqual(_medinfoSignature) || signature.SequenceEqual(_smiSignature);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        _image     = imagePlugin;
        _partition = partition;

        ErrorNumber errno = ReadSuperblocks(imagePlugin,
                                            partition,
                                            out BiosParameterBlock bpb,
                                            out MediaInformationBlock mib,
                                            out bool worm);

        if(errno != ErrorNumber.NoError) return;

        encoding = GetVolumeEncoding(mib, encoding);
        _medInfo = mib;

        var    vib         = new VolumeInformationBlock();
        bool   hasVib      = !worm && ReadVolumeInformation(out vib) == ErrorNumber.NoError;
        ushort percentFull = vib.percentFull;
        string owner       = hasVib ? DecodePadded(vib.owner, encoding) : null;
        bool   hasPercent  = hasVib;

        if(worm && ReadWormVmi(out byte[] vmi) == ErrorNumber.NoError)
        {
            owner       = DecodePadded(vmi[VMI_OWNER_OFFSET..(VMI_OWNER_OFFSET + 32)], encoding);
            percentFull = BigEndianBitConverter.ToUInt16(vmi, VMI_PERCENT_FULL_OFFSET);
            hasPercent  = true;
        }

        AaruLogging.Debug(MODULE_NAME, "mib.serial = 0x{0:X8}",       mib.serial);
        AaruLogging.Debug(MODULE_NAME, "mib.codepageType = {0}",      mib.codepageType);
        AaruLogging.Debug(MODULE_NAME, "mib.codepage = {0}",          mib.codepage);
        AaruLogging.Debug(MODULE_NAME, "mib.rps = {0}",               mib.rps);
        AaruLogging.Debug(MODULE_NAME, "mib.bps = {0}",               mib.bps);
        AaruLogging.Debug(MODULE_NAME, "mib.bps2 = {0}",              mib.bps2);
        AaruLogging.Debug(MODULE_NAME, "mib.sectors = {0}",           mib.sectors);
        AaruLogging.Debug(MODULE_NAME, "mib.unknown_e4 = 0x{0:X8}",   mib.unknown_e4);
        AaruLogging.Debug(MODULE_NAME, "mib.major = {0}",             mib.major);
        AaruLogging.Debug(MODULE_NAME, "mib.minor = {0}",             mib.minor);
        AaruLogging.Debug(MODULE_NAME, "mib.unknown_f8 = 0x{0:X4}",   mib.unknown_f8);
        AaruLogging.Debug(MODULE_NAME, "vib.dirIntentCount = 0x{0:X8}", vib.dirIntentCount);
        AaruLogging.Debug(MODULE_NAME, "vib.dirUpdateCount = 0x{0:X8}", vib.dirUpdateCount);
        AaruLogging.Debug(MODULE_NAME, "percentFull = {0}",           percentFull);

        string label   = DecodePadded(mib.volumeLabel, encoding);
        string comment = DecodePadded(mib.comment,     encoding);
        ulong  sectors = mib.sectors > 0 ? mib.sectors : partition.End - partition.Start + 1;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.HPOFS_Name);
        sb.AppendLine(worm ? Localization.HPOFS_WORM_volume : Localization.HPOFS_rewritable_volume);
        sb.AppendFormat(Localization.OEM_name_0, StringHandlers.SpacePaddedToString(bpb.oem_name)).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_sector, bpb.bps).AppendLine();
        sb.AppendFormat(Localization._0_sectors_on_volume_1_bytes, sectors, sectors * bpb.bps).AppendLine();
        sb.AppendFormat(Localization.Serial_number_0, mib.serial).AppendLine();

        if(label is not null) sb.AppendFormat(Localization.Volume_label_0, label).AppendLine();

        if(comment is not null) sb.AppendFormat(Localization.Volume_comment_0, comment).AppendLine();

        if(owner is not null) sb.AppendFormat(Localization.Volume_owner_0, owner).AppendLine();

        sb.AppendFormat(Localization.Volume_created_on_0,
                        DateHandlers.DosToDateTime(mib.creationDate, mib.creationTime))
          .AppendLine();

        sb.AppendFormat(Localization.Volume_uses_0_codepage_1,
                        mib.codepageType is > 0 and < 3
                            ? mib.codepageType == CODEPAGE_TYPE_EBCDIC
                                  ? Localization.EBCDIC
                                  : Localization.ASCII
                            : Localization.Unknown_codepage,
                        mib.codepage)
          .AppendLine();

        sb.AppendFormat(Localization.RPS_level_0,            mib.rps).AppendLine();
        sb.AppendFormat(Localization.Filesystem_version_0_1, mib.major, mib.minor).AppendLine();

        if(hasPercent) sb.AppendFormat(Localization.Volume_can_be_filled_up_to_0, percentFull).AppendLine();

        metadata = new FileSystem
        {
            Clusters               = sectors,
            ClusterSize            = bpb.bps,
            CreationDate           = DateHandlers.DosToDateTime(mib.creationDate, mib.creationTime),
            DataPreparerIdentifier = owner,
            Type                   = FS_TYPE,
            VolumeName             = label,
            VolumeSerial           = $"{mib.serial:X8}",
            SystemIdentifier       = StringHandlers.SpacePaddedToString(bpb.oem_name)
        };

        information = sb.ToString();
    }

#endregion
}