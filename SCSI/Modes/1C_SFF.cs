// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 1C_SFF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 1Ch: Timer & Protect page.
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
#region Mode Page 0x1C: Timer & Protect page

    /// <summary>Timer &amp; Protect page Page code 0x1C 8 bytes in INF-8070</summary>
    public struct ModePage_1C_SFF
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Time the device shall remain in the current state after seek, read or write operation</summary>
        public byte InactivityTimeMultiplier;
        /// <summary>Disabled until power cycle</summary>
        public bool DISP;
        /// <summary>Software Write Protect until Power-down</summary>
        public bool SWPP;
    }

    public static ModePage_1C_SFF? DecodeModePage_1C_SFF(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x1C)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

        var decoded = new ModePage_1C_SFF();

        decoded.PS   |= (pageResponse[0] & 0x80) == 0x80;
        decoded.DISP |= (pageResponse[2] & 0x02) == 0x02;
        decoded.SWPP |= (pageResponse[3] & 0x01) == 0x01;

        decoded.InactivityTimeMultiplier = (byte)(pageResponse[3] & 0x0F);

        return decoded;
    }

    public static string PrettifyModePage_1C_SFF(byte[] pageResponse) =>
        PrettifyModePage_1C_SFF(DecodeModePage_1C_SFF(pageResponse));

    public static string PrettifyModePage_1C_SFF(ModePage_1C_SFF? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_1C_SFF page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Timer_Protect_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.DISP)
            sb.AppendLine("\t" + Localization.Drive_is_disabled_until_power_is_cycled);

        if(page.SWPP)
            sb.AppendLine("\t" + Localization.Drive_is_software_write_protected_until_powered_down);

        switch(page.InactivityTimeMultiplier)
        {
            case 0:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_a_vendor_specified_time_after_a_seek_read_or_write_operation);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_125_ms_after_a_seek_read_or_write_operation);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_250_ms_after_a_seek_read_or_write_operation);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_500_ms_after_a_seek_read_or_write_operation);

                break;
            case 4:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_1_second_after_a_seek_read_or_write_operation);

                break;
            case 5:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_2_seconds_after_a_seek_read_or_write_operation);

                break;
            case 6:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_4_seconds_after_a_seek_read_or_write_operation);

                break;
            case 7:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_8_seconds_after_a_seek_read_or_write_operation);

                break;
            case 8:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_16_seconds_after_a_seek_read_or_write_operation);

                break;
            case 9:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_32_seconds_after_a_seek_read_or_write_operation);

                break;
            case 10:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_1_minute_after_a_seek_read_or_write_operation);

                break;
            case 11:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_2_minutes_after_a_seek_read_or_write_operation);

                break;
            case 12:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_4_minutes_after_a_seek_read_or_write_operation);

                break;
            case 13:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_8_minutes_after_a_seek_read_or_write_operation);

                break;
            case 14:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_16_minutes_after_a_seek_read_or_write_operation);

                break;
            case 15:
                sb.AppendLine("\t" + Localization.
                                  Drive_will_remain_in_same_status_32_minutes_after_a_seek_read_or_write_operation);

                break;
        }

        return sb.ToString();
    }

#endregion Mode Page 0x1C: Timer & Protect page
}