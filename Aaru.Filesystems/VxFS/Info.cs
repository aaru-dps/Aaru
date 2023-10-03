// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Veritas File System plugin.
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

/// <inheritdoc />
/// <summary>Implements detection of the Veritas filesystem</summary>
public sealed partial class VxFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        ulong vmfsSuperOff = VXFS_BASE / imagePlugin.Info.SectorSize;

        if(partition.Start + vmfsSuperOff >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + vmfsSuperOff, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        var magic = BitConverter.ToUInt32(sector, 0x00);

        return magic == VXFS_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.UTF8;
        information =   "";
        metadata    =   new FileSystem();
        ulong       vmfsSuperOff = VXFS_BASE / imagePlugin.Info.SectorSize;
        ErrorNumber errno        = imagePlugin.ReadSector(partition.Start + vmfsSuperOff, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        SuperBlock vxSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.Veritas_file_system);

        sbInformation.AppendFormat(Localization.Volume_version_0, vxSb.vs_version).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(vxSb.vs_fname, encoding)).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_has_0_blocks_of_1_bytes_each, vxSb.vs_bsize, vxSb.vs_size).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_has_0_inodes_per_block, vxSb.vs_inopb).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_free_inodes,      vxSb.vs_ifree).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_free_blocks,      vxSb.vs_free).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_created_on_0,
                                   DateHandlers.UnixUnsignedToDateTime(vxSb.vs_ctime, vxSb.vs_cutime)).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_last_modified_on_0,
                                   DateHandlers.UnixUnsignedToDateTime(vxSb.vs_wtime, vxSb.vs_wutime)).AppendLine();

        if(vxSb.vs_clean != 0)
            sbInformation.AppendLine(Localization.Volume_is_dirty);

        information = sbInformation.ToString();

        metadata = new FileSystem
        {
            Type             = FS_TYPE,
            CreationDate     = DateHandlers.UnixUnsignedToDateTime(vxSb.vs_ctime, vxSb.vs_cutime),
            ModificationDate = DateHandlers.UnixUnsignedToDateTime(vxSb.vs_wtime, vxSb.vs_wutime),
            Clusters         = (ulong)vxSb.vs_size,
            ClusterSize      = (uint)vxSb.vs_bsize,
            Dirty            = vxSb.vs_clean != 0,
            FreeClusters     = (ulong)vxSb.vs_free
        };
    }

#endregion
}