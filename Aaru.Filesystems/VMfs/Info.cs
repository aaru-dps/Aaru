// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : VMware file system plugin.
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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the VMware filesystem</summary>
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class VMfs
{
    const string FS_TYPE = "vmfs";

#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End) return false;

        ulong vmfsSuperOff = VMFS_BASE / imagePlugin.Info.SectorSize;

        if(partition.Start + vmfsSuperOff > partition.End) return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + vmfsSuperOff, out byte[] sector);

        if(errno != ErrorNumber.NoError) return false;

        var magic = BitConverter.ToUInt32(sector, 0x00);

        return magic == VMFS_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.UTF8;
        information =   "";
        metadata    =   new FileSystem();
        ulong       vmfsSuperOff = VMFS_BASE / imagePlugin.Info.SectorSize;
        ErrorNumber errno        = imagePlugin.ReadSector(partition.Start + vmfsSuperOff, out byte[] sector);

        if(errno != ErrorNumber.NoError) return;

        VolumeInfo volInfo = Marshal.ByteArrayToStructureLittleEndian<VolumeInfo>(sector);

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.VMware_file_system);

        var ctimeSecs     = (uint)(volInfo.ctime / 1000000);
        var ctimeNanoSecs = (uint)(volInfo.ctime % 1000000);
        var mtimeSecs     = (uint)(volInfo.mtime / 1000000);
        var mtimeNanoSecs = (uint)(volInfo.mtime % 1000000);

        sbInformation.AppendFormat(Localization.Volume_version_0, volInfo.version).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(volInfo.name, encoding))
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_size_0_bytes, volInfo.size * 256).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_UUID_0,       volInfo.uuid).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_created_on_0,
                                   DateHandlers.UnixUnsignedToDateTime(ctimeSecs, ctimeNanoSecs))
                     .AppendLine();

        sbInformation.AppendFormat(Localization.Volume_last_modified_on_0,
                                   DateHandlers.UnixUnsignedToDateTime(mtimeSecs, mtimeNanoSecs))
                     .AppendLine();

        information = sbInformation.ToString();

        metadata = new FileSystem
        {
            Type             = FS_TYPE,
            CreationDate     = DateHandlers.UnixUnsignedToDateTime(ctimeSecs, ctimeNanoSecs),
            ModificationDate = DateHandlers.UnixUnsignedToDateTime(mtimeSecs, mtimeNanoSecs),
            Clusters         = volInfo.size * 256 / imagePlugin.Info.SectorSize,
            ClusterSize      = imagePlugin.Info.SectorSize,
            VolumeSerial     = volInfo.uuid.ToString()
        };
    }

#endregion
}