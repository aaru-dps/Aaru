// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 1C.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 1Ch: Informational exceptions control page.
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
#region Mode Page 0x1C: Informational exceptions control page

    /// <summary>Informational exceptions control page Page code 0x1C 12 bytes in SPC-1, SPC-2, SPC-3, SPC-4</summary>
    public struct ModePage_1C
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Informational exception operations should not affect performance</summary>
        public bool Perf;
        /// <summary>Disable informational exception operations</summary>
        public bool DExcpt;
        /// <summary>Create a test device failure at next interval time</summary>
        public bool Test;
        /// <summary>Log informational exception conditions</summary>
        public bool LogErr;
        /// <summary>Method of reporting informational exceptions</summary>
        public byte MRIE;
        /// <summary>100 ms period to report an informational exception condition</summary>
        public uint IntervalTimer;
        /// <summary>How many times to report informational exceptions</summary>
        public uint ReportCount;

        /// <summary>Enable background functions</summary>
        public bool EBF;
        /// <summary>Warning reporting enabled</summary>
        public bool EWasc;

        /// <summary>Enable reporting of background self-test errors</summary>
        public bool EBACKERR;
    }

    public static ModePage_1C? DecodeModePage_1C(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x1C)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

        var decoded = new ModePage_1C();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.Perf   |= (pageResponse[2] & 0x80) == 0x80;
        decoded.DExcpt |= (pageResponse[2] & 0x08) == 0x08;
        decoded.Test   |= (pageResponse[2] & 0x04) == 0x04;
        decoded.LogErr |= (pageResponse[2] & 0x01) == 0x01;

        decoded.MRIE = (byte)(pageResponse[3] & 0x0F);

        decoded.IntervalTimer = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) +
                                       pageResponse[7]);

        decoded.EBF   |= (pageResponse[2] & 0x20) == 0x20;
        decoded.EWasc |= (pageResponse[2] & 0x10) == 0x10;

        decoded.EBACKERR |= (pageResponse[2] & 0x02) == 0x02;

        if(pageResponse.Length >= 12)
        {
            decoded.ReportCount = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) +
                                         pageResponse[11]);
        }

        return decoded;
    }

    public static string PrettifyModePage_1C(byte[] pageResponse) =>
        PrettifyModePage_1C(DecodeModePage_1C(pageResponse));

    public static string PrettifyModePage_1C(ModePage_1C? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_1C page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Informational_exceptions_control_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.DExcpt)
            sb.AppendLine("\t" + Localization.Informational_exceptions_are_disabled);
        else
        {
            sb.AppendLine("\t" + Localization.Informational_exceptions_are_enabled);

            switch(page.MRIE)
            {
                case 0:
                    sb.AppendLine("\t" + Localization.No_reporting_of_informational_exception_condition);

                    break;
                case 1:
                    sb.AppendLine("\t" + Localization.Asynchronous_event_reporting_of_informational_exceptions);

                    break;
                case 2:
                    sb.AppendLine("\t" + Localization.Generate_unit_attention_on_informational_exceptions);

                    break;
                case 3:
                    sb.AppendLine("\t" + Localization.
                                      Conditionally_generate_recovered_error_on_informational_exceptions);

                    break;
                case 4:
                    sb.AppendLine("\t" + Localization.
                                      Unconditionally_generate_recovered_error_on_informational_exceptions);

                    break;
                case 5:
                    sb.AppendLine("\t" + Localization.Generate_no_sense_on_informational_exceptions);

                    break;
                case 6:
                    sb.AppendLine("\t" + Localization.Only_report_informational_exception_condition_on_request);

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Unknown_method_of_reporting_0, page.MRIE).AppendLine();

                    break;
            }

            if(page.Perf)
            {
                sb.AppendLine("\t" + Localization.
                                  Informational_exceptions_reporting_should_not_affect_drive_performance);
            }

            if(page.Test)
                sb.AppendLine("\t" + Localization.A_test_informational_exception_will_raise_on_next_timer);

            if(page.LogErr)
                sb.AppendLine("\t" + Localization.Drive_shall_log_informational_exception_conditions);

            if(page.IntervalTimer > 0)
            {
                if(page.IntervalTimer == 0xFFFFFFFF)
                    sb.AppendLine("\t" + Localization.Timer_interval_is_vendor_specific);
                else
                    sb.AppendFormat("\t" + Localization.Timer_interval_is_0_ms, page.IntervalTimer * 100).AppendLine();
            }

            if(page.ReportCount > 0)
            {
                sb.AppendFormat(
                    "\t" + Localization.Informational_exception_conditions_will_be_reported_a_maximum_of_0_times,
                    page.ReportCount);
            }
        }

        if(page.EWasc)
            sb.AppendLine("\t" + Localization.Warning_reporting_is_enabled);

        if(page.EBF)
            sb.AppendLine("\t" + Localization.Background_functions_are_enabled);

        if(page.EBACKERR)
            sb.AppendLine("\t" + Localization.Drive_will_report_background_self_test_errors);

        return sb.ToString();
    }

#endregion Mode Page 0x1C: Informational exceptions control page

#region Mode Page 0x1C subpage 0x01: Background Control mode page

    /// <summary>Background Control mode page Page code 0x1A Subpage code 0x01 16 bytes in SPC-5</summary>
    public struct ModePage_1C_S01
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Suspend on log full</summary>
        public bool S_L_Full;
        /// <summary>Log only when intervention required</summary>
        public bool LOWIR;
        /// <summary>Enable background medium scan</summary>
        public bool En_Bms;
        /// <summary>Enable background pre-scan</summary>
        public bool En_Ps;
        /// <summary>Time in hours between background medium scans</summary>
        public ushort BackgroundScanInterval;
        /// <summary>Maximum time in hours for a background pre-scan to complete</summary>
        public ushort BackgroundPrescanTimeLimit;
        /// <summary>Minimum time in ms being idle before resuming a background scan</summary>
        public ushort MinIdleBeforeBgScan;
        /// <summary>Maximum time in ms to start processing commands while performing a background scan</summary>
        public ushort MaxTimeSuspendBgScan;
    }

    public static ModePage_1C_S01? DecodeModePage_1C_S01(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) != 0x40)
            return null;

        if((pageResponse[0] & 0x3F) != 0x1C)
            return null;

        if(pageResponse[1] != 0x01)
            return null;

        if((pageResponse[2] << 8) + pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 16)
            return null;

        var decoded = new ModePage_1C_S01();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.S_L_Full |= (pageResponse[4] & 0x04) == 0x04;
        decoded.LOWIR    |= (pageResponse[4] & 0x02) == 0x02;
        decoded.En_Bms   |= (pageResponse[4] & 0x01) == 0x01;
        decoded.En_Ps    |= (pageResponse[5] & 0x01) == 0x01;

        decoded.BackgroundScanInterval     = (ushort)((pageResponse[6]  << 8) + pageResponse[7]);
        decoded.BackgroundPrescanTimeLimit = (ushort)((pageResponse[8]  << 8) + pageResponse[9]);
        decoded.MinIdleBeforeBgScan        = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
        decoded.MaxTimeSuspendBgScan       = (ushort)((pageResponse[12] << 8) + pageResponse[13]);

        return decoded;
    }

    public static string PrettifyModePage_1C_S01(byte[] pageResponse) =>
        PrettifyModePage_1C_S01(DecodeModePage_1C_S01(pageResponse));

    public static string PrettifyModePage_1C_S01(ModePage_1C_S01? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_1C_S01 page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Background_Control_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.S_L_Full)
            sb.AppendLine("\t" + Localization.Background_scans_will_be_halted_if_log_is_full);

        if(page.LOWIR)
            sb.AppendLine("\t" + Localization.Background_scans_will_only_be_logged_if_they_require_intervention);

        if(page.En_Bms)
            sb.AppendLine("\t" + Localization.Background_medium_scans_are_enabled);

        if(page.En_Ps)
            sb.AppendLine("\t" + Localization.Background_pre_scans_are_enabled);

        if(page.BackgroundScanInterval > 0)
        {
            sb.
                AppendFormat(
                    "\t" + Localization.
                        _0__hours_shall_be_between_the_start_of_a_background_scan_operation_and_the_next,
                    page.BackgroundScanInterval).AppendLine();
        }

        if(page.BackgroundPrescanTimeLimit > 0)
        {
            sb.AppendFormat("\t" + Localization.Background_pre_scan_operations_can_take_a_maximum_of_0_hours,
                            page.BackgroundPrescanTimeLimit).AppendLine();
        }

        if(page.MinIdleBeforeBgScan > 0)
        {
            sb.
                AppendFormat(
                    "\t" + Localization.
                        At_least_0_ms_must_be_idle_before_resuming_a_suspended_background_scan_operation,
                    page.MinIdleBeforeBgScan).AppendLine();
        }

        if(page.MaxTimeSuspendBgScan > 0)
        {
            sb.
                AppendFormat(
                    "\t" + Localization.
                        At_most_0_ms_must_be_before_suspending_a_background_scan_operation_and_processing_received_commands,
                    page.MaxTimeSuspendBgScan).AppendLine();
        }

        return sb.ToString();
    }

#endregion Mode Page 0x1C subpage 0x01: Background Control mode page
}