// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple filesystem plugin.
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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Apple File System (APFS)</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class APFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        ContainerSuperBlock nxSb;

        try
        {
            nxSb = Marshal.ByteArrayToStructureLittleEndian<ContainerSuperBlock>(sector);
        }
        catch
        {
            return false;
        }

        return nxSb.magic == APFS_CONTAINER_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        Encoding = Encoding.UTF8;
        var sbInformation = new StringBuilder();
        metadata    = new FileSystem();
        information = "";

        if(partition.Start >= partition.End)
            return;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        ContainerSuperBlock nxSb;

        try
        {
            nxSb = Marshal.ByteArrayToStructureLittleEndian<ContainerSuperBlock>(sector);
        }
        catch
        {
            return;
        }

        if(nxSb.magic != APFS_CONTAINER_MAGIC)
            return;

        sbInformation.AppendLine(Localization.Apple_File_System);
        sbInformation.AppendLine();
        sbInformation.AppendFormat(Localization._0_bytes_per_block, nxSb.blockSize).AppendLine();

        sbInformation.AppendFormat(Localization.Container_has_0_bytes_in_1_blocks,
                                   nxSb.containerBlocks * nxSb.blockSize, nxSb.containerBlocks).AppendLine();

        information = sbInformation.ToString();

        metadata = new FileSystem
        {
            Bootable    = false,
            Clusters    = nxSb.containerBlocks,
            ClusterSize = nxSb.blockSize,
            Type        = FS_TYPE
        };
    }
}