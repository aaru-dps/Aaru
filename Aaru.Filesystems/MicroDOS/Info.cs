// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MicroDOS filesystem plugin
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

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local

using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>
///     Implements detection for the MicroDOS filesystem. Information from http://www.owg.ru/mkt/BK/MKDOS.TXT Thanks
///     to tarlabnor for translating it
/// </summary>
public sealed partial class MicroDOS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(1 + partition.Start >= partition.End)
            return false;

        if(imagePlugin.Info.SectorSize < 512)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bk0);

        if(errno != ErrorNumber.NoError)
            return false;

        Block0 block0 = Marshal.ByteArrayToStructureLittleEndian<Block0>(bk0);

        return block0 is { label: MAGIC, mklabel: MAGIC2 };
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        var sb = new StringBuilder();

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bk0);

        if(errno != ErrorNumber.NoError)
            return;

        Block0 block0 = Marshal.ByteArrayToStructureLittleEndian<Block0>(bk0);

        sb.AppendLine(Localization.MicroDOS_filesystem);
        sb.AppendFormat(Localization.Volume_has_0_blocks_1_bytes, block0.blocks, block0.blocks * 512).AppendLine();

        sb.AppendFormat(Localization.Volume_has_0_blocks_used_1_bytes, block0.usedBlocks, block0.usedBlocks * 512).
           AppendLine();

        sb.AppendFormat(Localization.Volume_contains_0_files, block0.files).AppendLine();
        sb.AppendFormat(Localization.First_used_block_is_0,   block0.firstUsedBlock).AppendLine();

        metadata = new FileSystem
        {
            Type         = FS_TYPE,
            ClusterSize  = 512,
            Clusters     = block0.blocks,
            Files        = block0.files,
            FreeClusters = (ulong)(block0.blocks - block0.usedBlocks)
        };

        information = sb.ToString();
    }

#endregion
}