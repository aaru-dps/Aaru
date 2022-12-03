// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple DOS filesystem and shows information.
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
using Claunia.Encoding;
using Schemas;
using Encoding = System.Text.Encoding;

namespace Aaru.Filesystems;

public sealed partial class AppleDOS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.Sectors != 455 &&
           imagePlugin.Info.Sectors != 560)
            return false;

        if(partition.Start             > 0 ||
           imagePlugin.Info.SectorSize != 256)
            return false;

        int spt = imagePlugin.Info.Sectors == 455 ? 13 : 16;

        ErrorNumber errno = imagePlugin.ReadSector((ulong)(17 * spt), out byte[] vtocB);

        if(errno != ErrorNumber.NoError)
            return false;

        _vtoc = Marshal.ByteArrayToStructureLittleEndian<Vtoc>(vtocB);

        return _vtoc.catalogSector < spt && _vtoc.maxTrackSectorPairsPerSector <= 122 && _vtoc.sectorsPerTrack == spt &&
               _vtoc.bytesPerSector == 256;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? new Apple2();
        information = "";
        var sb = new StringBuilder();

        int spt = imagePlugin.Info.Sectors == 455 ? 13 : 16;

        ErrorNumber errno = imagePlugin.ReadSector((ulong)(17 * spt), out byte[] vtocB);

        if(errno != ErrorNumber.NoError)
            return;

        _vtoc = Marshal.ByteArrayToStructureLittleEndian<Vtoc>(vtocB);

        sb.AppendLine(Localization.AppleDOS_Name);
        sb.AppendLine();

        sb.AppendFormat(Localization.Catalog_starts_at_sector_0_of_track_1, _vtoc.catalogSector, _vtoc.catalogTrack).
           AppendLine();

        sb.AppendFormat(Localization.File_system_initialized_by_DOS_release_0, _vtoc.dosRelease).AppendLine();
        sb.AppendFormat(Localization.Disk_volume_number_0, _vtoc.volumeNumber).AppendLine();
        sb.AppendFormat(Localization.Sectors_allocated_at_most_in_track_0, _vtoc.lastAllocatedSector).AppendLine();
        sb.AppendFormat(Localization._0_tracks_in_volume, _vtoc.tracks).AppendLine();
        sb.AppendFormat(Localization._0_sectors_per_track, _vtoc.sectorsPerTrack).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_sector, _vtoc.bytesPerSector).AppendLine();

        sb.AppendLine(_vtoc.allocationDirection > 0 ? Localization.Track_allocation_is_forward
                          : Localization.Track_allocation_is_reverse);

        information = sb.ToString();

        XmlFsType = new FileSystemType
        {
            Bootable    = true,
            Clusters    = imagePlugin.Info.Sectors,
            ClusterSize = imagePlugin.Info.SectorSize,
            Type        = FS_TYPE
        };
    }
}