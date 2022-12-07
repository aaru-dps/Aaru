// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SmartFileSystem plugin.
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

using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Smart File System</summary>
public sealed partial class SFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magic = BigEndianBitConverter.ToUInt32(sector, 0x00);

        return magic is SFS_MAGIC or SFS2_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        information = "";
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");
        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] rootBlockSector);

        if(errno != ErrorNumber.NoError)
            return;

        RootBlock rootBlock = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockSector);

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.SmartFileSystem);

        sbInformation.AppendFormat(Localization.Volume_version_0, rootBlock.version).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_starts_on_device_byte_0_and_ends_on_byte_1, rootBlock.firstbyte,
                                   rootBlock.lastbyte).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_has_0_blocks_of_1_bytes_each, rootBlock.totalblocks,
                                   rootBlock.blocksize).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_created_on_0,
                                   DateHandlers.UnixUnsignedToDateTime(rootBlock.datecreated).AddYears(8)).AppendLine();

        sbInformation.AppendFormat(Localization.Bitmap_starts_in_block_0, rootBlock.bitmapbase).AppendLine();

        sbInformation.AppendFormat(Localization.Admin_space_container_starts_in_block_0, rootBlock.adminspacecontainer).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Root_object_container_starts_in_block_0, rootBlock.rootobjectcontainer).
                      AppendLine();

        sbInformation.
            AppendFormat(Localization.Root_node_of_the_extent_B_tree_resides_in_block_0, rootBlock.extentbnoderoot).
            AppendLine();

        sbInformation.
            AppendFormat(Localization.Root_node_of_the_object_B_tree_resides_in_block_0, rootBlock.objectnoderoot).
            AppendLine();

        if(rootBlock.bits.HasFlag(Flags.CaseSensitive))
            sbInformation.AppendLine(Localization.Volume_is_case_sensitive);

        if(rootBlock.bits.HasFlag(Flags.RecycledFolder))
            sbInformation.AppendLine(Localization.Volume_moves_deleted_files_to_a_recycled_folder);

        information = sbInformation.ToString();

        XmlFsType = new FileSystemType
        {
            CreationDate          = DateHandlers.UnixUnsignedToDateTime(rootBlock.datecreated).AddYears(8),
            CreationDateSpecified = true,
            Clusters              = rootBlock.totalblocks,
            ClusterSize           = rootBlock.blocksize,
            Type                  = FS_TYPE
        };
    }
}