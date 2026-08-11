// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BeOS old filesystem plugin.
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
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class BOFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, false, out byte[] sector, out _);

        if(errno != ErrorNumber.NoError) return false;

        Track0 track0 = Marshal.ByteArrayToStructureBigEndian<Track0>(sector);

        return track0.VersionNumber is 0x30000 && track0.BytesPerSector is 512;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("iso-8859-15");
        information =   "";
        metadata    =   new FileSystem();

        var sb = new StringBuilder();

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, false, out byte[] sector, out _);

        if(errno != ErrorNumber.NoError) return;

        Track0 track0 = Marshal.ByteArrayToStructureBigEndian<Track0>(sector);

        sb.AppendFormat(Localization.Filesystem_version_0_1, track0.VersionNumber >> 16, track0.VersionNumber & 0xFFFF)
          .AppendLine();

        sb.AppendFormat(Localization.Block_size_0_bytes, track0.BytesPerSector).AppendLine();
        sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(track0.VolumeName, encoding)).AppendLine();

        sb.AppendFormat(Localization._0_blocks_in_volume_1_bytes,
                        track0.TotalSectors,
                        track0.TotalSectors * track0.BytesPerSector)
          .AppendLine();

        sb.AppendFormat(Localization._0_used_blocks_1_bytes,
                        track0.SectorsUsed,
                        track0.SectorsUsed * track0.BytesPerSector)
          .AppendLine();

        sb.AppendLine(track0.CleanShutdown != 0 ? Localization.Filesystem_is_clean : Localization.Filesystem_is_dirty);
        sb.AppendFormat(Localization.Creation_date_0, DateHandlers.UnixToDateTime(track0.FormatDate)).AppendLine();
        sb.AppendFormat(Localization.First_allocation_bitmap_sector_0, track0.FirstBitMapSector).AppendLine();
        sb.AppendFormat(Localization.Allocation_bitmap_size_0_sectors, track0.BitMapSize).AppendLine();
        sb.AppendFormat(Localization.First_directory_sector_0, track0.FirstDirectorySector).AppendLine();

        sb.AppendFormat(Localization.Hint_of_first_free_sector_for_a_directory_0, track0.DirectoryBlockHint)
          .AppendLine();

        sb.AppendFormat(Localization.Hint_of_first_free_sector_for_a_file_0, track0.FreeSectorHint).AppendLine();
        sb.AppendLine(track0.Removable != 0 ? Localization.Volume_is_removable : Localization.Volume_is_not_removable);
        sb.AppendFormat(Localization.Root_reference_0, track0.RootReference).AppendLine();
        sb.AppendFormat(Localization.Root_mode_0,      track0.RootMode).AppendLine();

        information = sb.ToString();

        metadata = new FileSystem
        {
            Clusters     = (ulong)track0.TotalSectors,
            ClusterSize  = (uint)track0.BytesPerSector,
            Dirty        = track0.CleanShutdown == 0,
            FreeClusters = (ulong)(track0.TotalSectors - track0.SectorsUsed),
            Type         = FS_TYPE,
            VolumeName   = StringHandlers.CToString(track0.VolumeName, encoding)
        };
    }
}