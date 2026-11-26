// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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

using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class JXFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        return ReadSuperBlock(imagePlugin, partition, out _, out _) == ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        if(ReadSuperBlock(imagePlugin, partition, out SuperBlock superBlock, out _) != ErrorNumber.NoError) return;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(Localization.JXFS_Name);
        sb.AppendFormat(Localization.Volume_version_0, superBlock.version).AppendLine();

        sb.AppendFormat(Localization.Volume_starts_on_device_byte_0_and_ends_on_byte_1,
                        superBlock.firstByte,
                        superBlock.lastByte)
          .AppendLine();

        sb.AppendFormat(Localization.Volume_has_0_blocks_of_1_bytes_each,
                        superBlock.totalBlocks,
                        superBlock.blockSize)
          .AppendLine();

        sb.AppendFormat(Localization.Volume_created_on_0, _amigaEpoch.AddSeconds(superBlock.creationDate))
          .AppendLine();

        if((superBlock.flags & VolumeFlags.CaseSensitive) != 0) sb.AppendLine(Localization.Volume_is_case_sensitive);

        if((superBlock.flags & VolumeFlags.RecycleBin) != 0)
            sb.AppendLine(Localization.Volume_moves_deleted_files_to_a_recycled_folder);

        metadata = new FileSystem
        {
            Type         = FS_TYPE,
            Clusters     = superBlock.totalBlocks,
            ClusterSize  = superBlock.blockSize,
            CreationDate = _amigaEpoch.AddSeconds(superBlock.creationDate)
        };

        // The rest needs the system files
        if(Initialize(imagePlugin, partition, encoding) == ErrorNumber.NoError)
        {
            sb.AppendFormat(Localization.Volume_name_0, _rootEntry.Name).AppendLine();
            sb.AppendFormat(Localization.Volume_has_0_free_blocks, _freeBlocks).AppendLine();

            sb.AppendFormat(Localization.Volume_last_modified_on_0, TicksToDateTime(_rootEntry.Node.Date))
              .AppendLine();

            if(_unfinishedTransaction) sb.AppendLine(Localization.Volume_has_an_unfinished_transaction);

            metadata = BuildMetadata();
        }

        _directoryCache.Clear();
        _imagePlugin = null;

        information = sb.ToString();
    }

#endregion
}