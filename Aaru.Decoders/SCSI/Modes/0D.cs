// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 0D.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 0Dh: CD-ROM parameteres page.
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
#region Mode Page 0x0D: CD-ROM parameteres page

    /// <summary>CD-ROM parameteres page Page code 0x0D 8 bytes in SCSI-2, MMC-1, MMC-2, MMC-3</summary>
    public struct ModePage_0D
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Time the drive shall remain in hold track state after seek or read</summary>
        public byte InactivityTimerMultiplier;
        /// <summary>Seconds per Minute</summary>
        public ushort SecondsPerMinute;
        /// <summary>Frames per Second</summary>
        public ushort FramesPerSecond;
    }

    public static ModePage_0D? DecodeModePage_0D(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40) return null;

        if((pageResponse?[0] & 0x3F) != 0x0D) return null;

        if(pageResponse[1] + 2 != pageResponse.Length) return null;

        if(pageResponse.Length < 8) return null;

        var decoded = new ModePage_0D();

        decoded.PS                        |= (pageResponse[0]       & 0x80) == 0x80;
        decoded.InactivityTimerMultiplier =  (byte)(pageResponse[3] & 0xF);
        decoded.SecondsPerMinute          =  (ushort)((pageResponse[4] << 8) + pageResponse[5]);
        decoded.FramesPerSecond           =  (ushort)((pageResponse[6] << 8) + pageResponse[7]);

        return decoded;
    }

    public static string PrettifyModePage_0D(byte[] pageResponse) =>
        PrettifyModePage_0D(DecodeModePage_0D(pageResponse));

    public static string PrettifyModePage_0D(ModePage_0D? modePage)
    {
        if(!modePage.HasValue) return null;

        ModePage_0D page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_CD_ROM_parameters_page);

        if(page.PS) sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        switch(page.InactivityTimerMultiplier)
        {
            case 0:
                sb.AppendLine("\t" +
                              Localization
                                 .Drive_will_remain_in_track_hold_state_a_vendor_specified_time_after_a_seek_or_read);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_125_ms_after_a_seek_or_read);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_250_ms_after_a_seek_or_read);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_500_ms_after_a_seek_or_read);

                break;
            case 4:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_1_second_after_a_seek_or_read);

                break;
            case 5:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_2_seconds_after_a_seek_or_read);

                break;
            case 6:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_4_seconds_after_a_seek_or_read);

                break;
            case 7:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_8_seconds_after_a_seek_or_read);

                break;
            case 8:
                sb.AppendLine("\t" +
                              Localization.Drive_will_remain_in_track_hold_state_16_seconds_after_a_seek_or_read);

                break;
            case 9:
                sb.AppendLine("\t" +
                              Localization.Drive_will_remain_in_track_hold_state_32_seconds_after_a_seek_or_read);

                break;
            case 10:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_1_minute_after_a_seek_or_read);

                break;
            case 11:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_2_minutes_after_a_seek_or_read);

                break;
            case 12:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_4_minutes_after_a_seek_or_read);

                break;
            case 13:
                sb.AppendLine("\t" + Localization.Drive_will_remain_in_track_hold_state_8_minutes_after_a_seek_or_read);

                break;
            case 14:
                sb.AppendLine("\t" +
                              Localization.Drive_will_remain_in_track_hold_state_16_minutes_after_a_seek_or_read);

                break;
            case 15:
                sb.AppendLine("\t" +
                              Localization.Drive_will_remain_in_track_hold_state_32_minutes_after_a_seek_or_read);

                break;
        }

        if(page.SecondsPerMinute > 0)
            sb.AppendFormat("\t" + Localization.Each_minute_has_0_seconds, page.SecondsPerMinute).AppendLine();

        if(page.FramesPerSecond > 0)
            sb.AppendFormat("\t" + Localization.Each_second_has_0_frames, page.FramesPerSecond).AppendLine();

        return sb.ToString();
    }

#endregion Mode Page 0x0D: CD-ROM parameteres page
}