// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
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

public sealed partial class OperaFS
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        var syncBytes = new byte[5];

        byte recordType = sbSector[0x000];
        Array.Copy(sbSector, 0x001, syncBytes, 0, 5);
        byte recordVersion = sbSector[0x006];

        if(recordType    != 1 ||
           recordVersion != 1)
            return false;

        return Encoding.ASCII.GetString(syncBytes) == SYNC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        // TODO: Find correct default encoding
        encoding    = Encoding.ASCII;
        information = "";
        metadata    = new FileSystem();
        var superBlockmetadata = new StringBuilder();

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return;

        SuperBlock sb = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sbSector);

        if(sb.record_type    != 1 ||
           sb.record_version != 1)
            return;

        if(Encoding.ASCII.GetString(sb.sync_bytes) != SYNC)
            return;

        superBlockmetadata.AppendFormat(Localization.Opera_filesystem_disc).AppendLine();

        if(!string.IsNullOrEmpty(StringHandlers.CToString(sb.volume_label, encoding)))
        {
            superBlockmetadata.
                AppendFormat(Localization.Volume_label_0, StringHandlers.CToString(sb.volume_label, encoding)).
                AppendLine();
        }

        if(!string.IsNullOrEmpty(StringHandlers.CToString(sb.volume_comment, encoding)))
        {
            superBlockmetadata.
                AppendFormat(Localization.Volume_comment_0, StringHandlers.CToString(sb.volume_comment, encoding)).
                AppendLine();
        }

        superBlockmetadata.AppendFormat(Localization.Volume_identifier_0_X8, sb.volume_id).AppendLine();
        superBlockmetadata.AppendFormat(Localization.Block_size_0_bytes,     sb.block_size).AppendLine();

        if(imagePlugin.Info.SectorSize is 2336 or 2352 or 2448)
        {
            if(sb.block_size != 2048)
            {
                superBlockmetadata.
                    AppendFormat(
                        Localization.WARNING_Filesystem_indicates_0_bytes_block_while_device_indicates_1_bytes_block,
                        sb.block_size, 2048);
            }
        }
        else if(imagePlugin.Info.SectorSize != sb.block_size)
        {
            superBlockmetadata.
                AppendFormat(
                    Localization.WARNING_Filesystem_indicates_0_bytes_block_while_device_indicates_1_bytes_block,
                    sb.block_size, imagePlugin.Info.SectorSize);
        }

        superBlockmetadata.
            AppendFormat(Localization.Volume_size_0_blocks_1_bytes, sb.block_count, sb.block_size * sb.block_count).
            AppendLine();

        if(sb.block_count > imagePlugin.Info.Sectors)
        {
            superBlockmetadata.
                AppendFormat(Localization.WARNING__Filesystem_indicates_0_blocks_while_device_indicates_1_blocks,
                             sb.block_count, imagePlugin.Info.Sectors);
        }

        superBlockmetadata.AppendFormat(Localization.Root_directory_identifier_0,       sb.root_dirid).AppendLine();
        superBlockmetadata.AppendFormat(Localization.Root_directory_block_size_0_bytes, sb.rootdir_bsize).AppendLine();

        superBlockmetadata.AppendFormat(Localization.Root_directory_size_0_blocks_1_bytes, sb.rootdir_blocks,
                                        sb.rootdir_bsize * sb.rootdir_blocks).AppendLine();

        superBlockmetadata.AppendFormat(Localization.Last_root_directory_copy_0, sb.last_root_copy).AppendLine();

        information = superBlockmetadata.ToString();

        metadata = new FileSystem
        {
            Type        = FS_TYPE,
            VolumeName  = StringHandlers.CToString(sb.volume_label, encoding),
            ClusterSize = sb.block_size,
            Clusters    = sb.block_count
        };
    }

#endregion
}