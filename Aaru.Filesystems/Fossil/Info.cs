// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Fossil filesystem plugin
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the Plan-9 Fossil on-disk filesystem</summary>
public sealed partial class Fossil
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        ulong hdrSector = HEADER_POS / imagePlugin.Info.SectorSize;

        if(partition.Start + hdrSector > imagePlugin.Info.Sectors)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + hdrSector, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        Header hdr = Marshal.ByteArrayToStructureBigEndian<Header>(sector);

        AaruConsole.DebugWriteLine("Fossil plugin", Localization.magic_at_0_expected_1, hdr.magic, FOSSIL_HDR_MAGIC);

        return hdr.magic == FOSSIL_HDR_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        // Technically everything on Plan 9 from Bell Labs is in UTF-8
        Encoding    = Encoding.UTF8;
        information = "";
        metadata    = new FileSystem();

        if(imagePlugin.Info.SectorSize < 512)
            return;

        ulong hdrSector = HEADER_POS / imagePlugin.Info.SectorSize;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + hdrSector, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        Header hdr = Marshal.ByteArrayToStructureBigEndian<Header>(sector);

        AaruConsole.DebugWriteLine("Fossil plugin", Localization.magic_at_0_expected_1, hdr.magic, FOSSIL_HDR_MAGIC);

        var sb = new StringBuilder();

        sb.AppendLine(Localization.Fossil_filesystem);
        sb.AppendFormat(Localization.Filesystem_version_0, hdr.version).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_block, hdr.blockSize).AppendLine();
        sb.AppendFormat(Localization.Superblock_resides_in_block_0, hdr.super).AppendLine();
        sb.AppendFormat(Localization.Labels_resides_in_block_0, hdr.label).AppendLine();
        sb.AppendFormat(Localization.Data_starts_at_block_0, hdr.data).AppendLine();
        sb.AppendFormat(Localization.Volume_has_0_blocks, hdr.end).AppendLine();

        ulong sbLocation = (hdr.super * (hdr.blockSize / imagePlugin.Info.SectorSize)) + partition.Start;

        metadata = new FileSystem
        {
            Type        = FS_TYPE,
            ClusterSize = hdr.blockSize,
            Clusters    = hdr.end
        };

        if(sbLocation <= partition.End)
        {
            imagePlugin.ReadSector(sbLocation, out sector);
            SuperBlock fsb = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sector);

            AaruConsole.DebugWriteLine("Fossil plugin", Localization.magic_0_expected_1, fsb.magic, FOSSIL_SB_MAGIC);

            if(fsb.magic == FOSSIL_SB_MAGIC)
            {
                sb.AppendFormat(Localization.Epoch_low_0, fsb.epochLow).AppendLine();
                sb.AppendFormat(Localization.Epoch_high_0, fsb.epochHigh).AppendLine();
                sb.AppendFormat(Localization.Next_QID_0, fsb.qid).AppendLine();
                sb.AppendFormat(Localization.Active_root_block_0, fsb.active).AppendLine();
                sb.AppendFormat(Localization.Next_root_block_0, fsb.next).AppendLine();
                sb.AppendFormat(Localization.Current_root_block_0, fsb.current).AppendLine();
                sb.AppendFormat(Localization.Volume_label_0, StringHandlers.CToString(fsb.name, Encoding)).AppendLine();
                metadata.VolumeName = StringHandlers.CToString(fsb.name, Encoding);
            }
        }

        information = sb.ToString();
    }
}