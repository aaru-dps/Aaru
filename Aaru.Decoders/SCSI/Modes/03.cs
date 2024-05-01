// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 03.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 03h: Format device page.
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

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
#region Mode Page 0x03: Format device page

    /// <summary>Disconnect-reconnect page Page code 0x03 24 bytes in SCSI-2, SBC-1</summary>
    public struct ModePage_03
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Tracks per zone to use in dividing the capacity for the purpose of allocating alternate sectors</summary>
        public ushort TracksPerZone;
        /// <summary>Number of sectors per zone that shall be reserved for defect handling</summary>
        public ushort AltSectorsPerZone;
        /// <summary>Number of tracks per zone that shall be reserved for defect handling</summary>
        public ushort AltTracksPerZone;
        /// <summary>Number of tracks per LUN that shall be reserved for defect handling</summary>
        public ushort AltTracksPerLun;
        /// <summary>Number of physical sectors per track</summary>
        public ushort SectorsPerTrack;
        /// <summary>Bytes per physical sector</summary>
        public ushort BytesPerSector;
        /// <summary>Interleave value, target dependent</summary>
        public ushort Interleave;
        /// <summary>Sectors between last block of one track and first block of the next</summary>
        public ushort TrackSkew;
        /// <summary>Sectors between last block of a cylinder and first block of the next one</summary>
        public ushort CylinderSkew;
        /// <summary>Soft-sectored</summary>
        public bool SSEC;
        /// <summary>Hard-sectored</summary>
        public bool HSEC;
        /// <summary>Removable</summary>
        public bool RMB;
        /// <summary>
        ///     If set, address are allocated progressively in a surface before going to the next. Otherwise, it goes by
        ///     cylinders
        /// </summary>
        public bool SURF;
    }

    public static ModePage_03? DecodeModePage_03(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40) return null;

        if((pageResponse?[0] & 0x3F) != 0x03) return null;

        if(pageResponse[1] + 2 != pageResponse.Length) return null;

        if(pageResponse.Length < 24) return null;

        var decoded = new ModePage_03();

        decoded.PS                |= (pageResponse[0] & 0x80) == 0x80;
        decoded.TracksPerZone     =  (ushort)((pageResponse[2]  << 8) + pageResponse[3]);
        decoded.AltSectorsPerZone =  (ushort)((pageResponse[4]  << 8) + pageResponse[5]);
        decoded.AltTracksPerZone  =  (ushort)((pageResponse[6]  << 8) + pageResponse[7]);
        decoded.AltTracksPerLun   =  (ushort)((pageResponse[8]  << 8) + pageResponse[9]);
        decoded.SectorsPerTrack   =  (ushort)((pageResponse[10] << 8) + pageResponse[11]);
        decoded.BytesPerSector    =  (ushort)((pageResponse[12] << 8) + pageResponse[13]);
        decoded.Interleave        =  (ushort)((pageResponse[14] << 8) + pageResponse[15]);
        decoded.TrackSkew         =  (ushort)((pageResponse[16] << 8) + pageResponse[17]);
        decoded.CylinderSkew      =  (ushort)((pageResponse[18] << 8) + pageResponse[19]);
        decoded.SSEC              |= (pageResponse[20] & 0x80) == 0x80;
        decoded.HSEC              |= (pageResponse[20] & 0x40) == 0x40;
        decoded.RMB               |= (pageResponse[20] & 0x20) == 0x20;
        decoded.SURF              |= (pageResponse[20] & 0x10) == 0x10;

        return decoded;
    }

    public static string PrettifyModePage_03(byte[] pageResponse) =>
        PrettifyModePage_03(DecodeModePage_03(pageResponse));

    public static string PrettifyModePage_03(ModePage_03? modePage)
    {
        if(!modePage.HasValue) return null;

        ModePage_03 page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Format_device_page);

        if(page.PS) sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        sb.AppendFormat("\t" +
                        Localization
                           ._0_tracks_per_zone_to_use_in_dividing_the_capacity_for_the_purpose_of_allocating_alternate_sectors,
                        page.TracksPerZone)
          .AppendLine();

        sb.AppendFormat("\t" + Localization._0_sectors_per_zone_that_shall_be_reserved_for_defect_handling,
                        page.AltSectorsPerZone)
          .AppendLine();

        sb.AppendFormat("\t" + Localization._0_tracks_per_zone_that_shall_be_reserved_for_defect_handling,
                        page.AltTracksPerZone)
          .AppendLine();

        sb.AppendFormat("\t" + Localization._0_tracks_per_LUN_that_shall_be_reserved_for_defect_handling,
                        page.AltTracksPerLun)
          .AppendLine();

        sb.AppendFormat("\t" + Localization._0_physical_sectors_per_track,          page.SectorsPerTrack).AppendLine();
        sb.AppendFormat("\t" + Localization._0_Bytes_per_physical_sector,           page.BytesPerSector).AppendLine();
        sb.AppendFormat("\t" + Localization.Target_dependent_interleave_value_is_0, page.Interleave).AppendLine();

        sb.AppendFormat("\t" + Localization._0_sectors_between_last_block_of_one_track_and_first_block_of_the_next,
                        page.TrackSkew)
          .AppendLine();

        sb.AppendFormat("\t" + Localization._0_sectors_between_last_block_of_a_cylinder_and_first_block_of_the_next_one,
                        page.CylinderSkew)
          .AppendLine();

        if(page.SSEC) sb.AppendLine("\t" + Localization.Drive_supports_soft_sectoring_format);

        if(page.HSEC) sb.AppendLine("\t" + Localization.Drive_supports_hard_sectoring_format);

        if(page.RMB) sb.AppendLine("\t" + Localization.Drive_media_is_removable);

        sb.AppendLine(page.SURF
                          ? "\t" +
                            Localization
                               .Sector_addressing_is_progressively_incremented_in_one_surface_before_going_to_the_next
                          : "\t" +
                            Localization
                               .Sector_addressing_is_progressively_incremented_in_one_cylinder_before_going_to_the_next);

        return sb.ToString();
    }

#endregion Mode Page 0x03: Format device page
}