// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 0A.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 0Ah: Control mode page.
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
#region Mode Page 0x0A: Control mode page

    /// <summary>Control mode page Page code 0x0A 8 bytes in SCSI-2 12 bytes in SPC-1, SPC-2, SPC-3, SPC-4, SPC-5</summary>
    public struct ModePage_0A
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>If set, target shall report log exception conditions</summary>
        public bool RLEC;
        /// <summary>Queue algorithm modifier</summary>
        public byte QueueAlgorithm;
        /// <summary>
        ///     If set all remaining suspended I/O processes shall be aborted after the contingent allegiance condition or
        ///     extended contingent allegiance condition
        /// </summary>
        public byte QErr;
        /// <summary>Tagged queuing is disabled</summary>
        public bool DQue;
        /// <summary>Extended Contingent Allegiance is enabled</summary>
        public bool EECA;
        /// <summary>Target may issue an asynchronous event notification upon completing its initialization</summary>
        public bool RAENP;
        /// <summary>Target may issue an asynchronous event notification instead of a unit attention condition</summary>
        public bool UAAENP;
        /// <summary>Target may issue an asynchronous event notification instead of a deferred error</summary>
        public bool EAENP;
        /// <summary>Minimum time in ms after initialization before attempting asynchronous event notifications</summary>
        public ushort ReadyAENHoldOffPeriod;

        /// <summary>Global logging target save disabled</summary>
        public bool GLTSD;
        /// <summary>CHECK CONDITION should be reported rather than a long busy condition</summary>
        public bool RAC;
        /// <summary>Software write protect is active</summary>
        public bool SWP;
        /// <summary>Maximum time in 100 ms units allowed to remain busy. 0xFFFF == unlimited.</summary>
        public ushort BusyTimeoutPeriod;

        /// <summary>Task set type</summary>
        public byte TST;
        /// <summary>Tasks aborted by other initiator's actions should be terminated with TASK ABORTED</summary>
        public bool TAS;
        /// <summary>Action to be taken when a medium is inserted</summary>
        public byte AutoloadMode;
        /// <summary>Time in seconds to complete an extended self-test</summary>
        public byte ExtendedSelfTestCompletionTime;

        /// <summary>All tasks received in nexus with ACA ACTIVE is set and an ACA condition is established shall terminate</summary>
        public bool TMF_ONLY;
        /// <summary>
        ///     Device shall return descriptor format sense data when returning sense data in the same transactions as a CHECK
        ///     CONDITION
        /// </summary>
        public bool D_SENSE;
        /// <summary>Unit attention interlocks control</summary>
        public byte UA_INTLCK_CTRL;
        /// <summary>LOGICAL BLOCK APPLICATION TAG should not be modified</summary>
        public bool ATO;

        /// <summary>Protector information checking is disabled</summary>
        public bool DPICZ;
        /// <summary>No unit attention on release</summary>
        public bool NUAR;
        /// <summary>Application Tag mode page is enabled</summary>
        public bool ATMPE;
        /// <summary>Abort any write command without protection information</summary>
        public bool RWWP;
        /// <summary>Supportes block lengths and protection information</summary>
        public bool SBLP;
    }

    public static ModePage_0A? DecodeModePage_0A(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x0A)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

        var decoded = new ModePage_0A();

        decoded.PS   |= (pageResponse[0] & 0x80) == 0x80;
        decoded.RLEC |= (pageResponse[2] & 0x01) == 0x01;

        decoded.QueueAlgorithm = (byte)((pageResponse[3] & 0xF0) >> 4);
        decoded.QErr           = (byte)((pageResponse[3] & 0x06) >> 1);

        decoded.DQue   |= (pageResponse[3] & 0x01) == 0x01;
        decoded.EECA   |= (pageResponse[4] & 0x80) == 0x80;
        decoded.RAENP  |= (pageResponse[4] & 0x04) == 0x04;
        decoded.UAAENP |= (pageResponse[4] & 0x02) == 0x02;
        decoded.EAENP  |= (pageResponse[4] & 0x01) == 0x01;

        decoded.ReadyAENHoldOffPeriod = (ushort)((pageResponse[6] << 8) + pageResponse[7]);

        if(pageResponse.Length < 10)
            return decoded;

        // SPC-1
        decoded.GLTSD |= (pageResponse[2] & 0x02) == 0x02;
        decoded.RAC   |= (pageResponse[4] & 0x40) == 0x40;
        decoded.SWP   |= (pageResponse[4] & 0x08) == 0x08;

        decoded.BusyTimeoutPeriod = (ushort)((pageResponse[8] << 8) + pageResponse[9]);

        // SPC-2
        decoded.TST               =  (byte)((pageResponse[2] & 0xE0) >> 5);
        decoded.TAS               |= (pageResponse[4]       & 0x80) == 0x80;
        decoded.AutoloadMode      =  (byte)(pageResponse[5] & 0x07);
        decoded.BusyTimeoutPeriod =  (ushort)((pageResponse[10] << 8) + pageResponse[11]);

        // SPC-3
        decoded.TMF_ONLY       |= (pageResponse[2] & 0x10) == 0x10;
        decoded.D_SENSE        |= (pageResponse[2] & 0x04) == 0x04;
        decoded.UA_INTLCK_CTRL =  (byte)((pageResponse[4] & 0x30) >> 4);
        decoded.TAS            |= (pageResponse[5] & 0x40) == 0x40;
        decoded.ATO            |= (pageResponse[5] & 0x80) == 0x80;

        // SPC-5
        decoded.DPICZ |= (pageResponse[2] & 0x08) == 0x08;
        decoded.NUAR  |= (pageResponse[3] & 0x08) == 0x08;
        decoded.ATMPE |= (pageResponse[5] & 0x20) == 0x20;
        decoded.RWWP  |= (pageResponse[5] & 0x10) == 0x10;
        decoded.SBLP  |= (pageResponse[5] & 0x08) == 0x08;

        return decoded;
    }

    public static string PrettifyModePage_0A(byte[] pageResponse) =>
        PrettifyModePage_0A(DecodeModePage_0A(pageResponse));

    public static string PrettifyModePage_0A(ModePage_0A? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_0A page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Control_mode_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.RLEC)
            sb.AppendLine("\t" + Localization.If_set_target_shall_report_log_exception_conditions);

        if(page.DQue)
            sb.AppendLine("\t" + Localization.Tagged_queuing_is_disabled);

        if(page.EECA)
            sb.AppendLine("\t" + Localization.Extended_Contingent_Allegiance_is_enabled);

        if(page.RAENP)
        {
            sb.AppendLine("\t" + Localization.
                              Target_may_issue_an_asynchronous_event_notification_upon_completing_its_initialization);
        }

        if(page.UAAENP)
        {
            sb.AppendLine("\t" + Localization.
                              Target_may_issue_an_asynchronous_event_notification_instead_of_a_unit_attention_condition);
        }

        if(page.EAENP)
        {
            sb.AppendLine("\t" + Localization.
                              Target_may_issue_an_asynchronous_event_notification_instead_of_a_deferred_error);
        }

        if(page.GLTSD)
            sb.AppendLine("\t" + Localization.Global_logging_target_save_disabled);

        if(page.RAC)
            sb.AppendLine("\t" + Localization.CHECK_CONDITION_should_be_reported_rather_than_a_long_busy_condition);

        if(page.SWP)
            sb.AppendLine("\t" + Localization.Software_write_protect_is_enabled);

        if(page.TAS)
        {
            sb.AppendLine("\t" + Localization.
                              Tasks_aborted_by_other_initiator_s_actions_should_be_terminated_with_TASK_ABORTED);
        }

        if(page.TMF_ONLY)
        {
            sb.AppendLine("\t" + Localization.
                              All_tasks_received_in_nexus_with_ACA_ACTIVE_is_set_and_an_ACA_condition_is_established_shall_terminate);
        }

        if(page.D_SENSE)
        {
            sb.AppendLine("\t" + Localization.
                              Device_shall_return_descriptor_format_sense_data_when_returning_sense_data_in_the_same_transactions_as_a_CHECK_CONDITION);
        }

        if(page.ATO)
            sb.AppendLine("\t" + Localization.LOGICAL_BLOCK_APPLICATION_TAG_should_not_be_modified);

        if(page.DPICZ)
            sb.AppendLine("\t" + Localization.Protector_information_checking_is_disabled);

        if(page.NUAR)
            sb.AppendLine("\t" + Localization.No_unit_attention_on_release);

        if(page.ATMPE)
            sb.AppendLine("\t" + Localization.Application_Tag_mode_page_is_enabled);

        if(page.RWWP)
            sb.AppendLine("\t" + Localization.Abort_any_write_command_without_protection_information);

        if(page.SBLP)
            sb.AppendLine("\t" + Localization.Supports_block_lengths_and_protection_information);

        switch(page.TST)
        {
            case 0:
                sb.AppendLine("\t" + Localization.The_logical_unit_maintains_one_task_set_for_all_nexuses);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.The_logical_unit_maintains_separate_task_sets_for_each_nexus);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_Task_set_type_0, page.TST).AppendLine();

                break;
        }

        switch(page.QueueAlgorithm)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Commands_should_be_sent_strictly_ordered);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Commands_can_be_reordered_in_any_manner);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_Queue_Algorithm_Modifier_0, page.QueueAlgorithm).
                   AppendLine();

                break;
        }

        switch(page.QErr)
        {
            case 0:
                sb.AppendLine("\t" + Localization.
                                  If_ACA_is_established_the_task_set_commands_shall_resume_after_it_is_cleared_otherwise_they_shall_terminate_with_CHECK_CONDITION);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.
                                  All_the_affected_commands_in_the_task_set_shall_be_aborted_when_CHECK_CONDITION_is_returned);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.
                                  Affected_commands_in_the_task_set_belonging_with_the_CHECK_CONDITION_nexus_shall_be_aborted);

                break;
            default:
                sb.AppendLine("\t" + Localization.Reserved_QErr_value_2_is_set);

                break;
        }

        switch(page.UA_INTLCK_CTRL)
        {
            case 0:
                sb.AppendLine("\t" + Localization.LUN_shall_clear_unit_attention_condition_reported_in_the_same_nexus);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.
                                  LUN_shall_not_clear_unit_attention_condition_reported_in_the_same_nexus);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.
                                  LUN_shall_not_clear_unit_attention_condition_reported_in_the_same_nexus_and_shall_establish_a_unit_attention_condition_for_the_initiator);

                break;
            default:
                sb.AppendLine("\t" + Localization.Reserved_UA_INTLCK_CTRL_value_1_is_set);

                break;
        }

        switch(page.AutoloadMode)
        {
            case 0:
                sb.AppendLine("\t" + Localization.On_medium_insertion_it_shall_be_loaded_for_full_access);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.
                                  On_medium_insertion_it_shall_be_loaded_for_auxiliary_memory_access_only);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.On_medium_insertion_it_shall_not_be_loaded);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Reserved_autoload_mode_0_set, page.AutoloadMode).AppendLine();

                break;
        }

        if(page.ReadyAENHoldOffPeriod > 0)
        {
            sb.
                AppendFormat(
                    "\t" + Localization._0_ms_before_attempting_asynchronous_event_notifications_after_initialization,
                    page.ReadyAENHoldOffPeriod).AppendLine();
        }

        if(page.BusyTimeoutPeriod > 0)
        {
            if(page.BusyTimeoutPeriod == 0xFFFF)
                sb.AppendLine("\t" + Localization.There_is_no_limit_on_the_maximum_time_that_is_allowed_to_remain_busy);
            else
            {
                sb.AppendFormat("\t" + Localization.A_maximum_of_0_ms_are_allowed_to_remain_busy,
                                page.BusyTimeoutPeriod * 100).AppendLine();
            }
        }

        if(page.ExtendedSelfTestCompletionTime > 0)
        {
            sb.AppendFormat("\t" + Localization._0_seconds_to_complete_extended_self_test,
                            page.ExtendedSelfTestCompletionTime);
        }

        return sb.ToString();
    }

#endregion Mode Page 0x0A: Control mode page

#region Mode Page 0x0A subpage 0x01: Control Extension mode page

    /// <summary>Control Extension mode page Page code 0x0A Subpage code 0x01 32 bytes in SPC-3, SPC-4, SPC-5</summary>
    public struct ModePage_0A_S01
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Timestamp outside this standard</summary>
        public bool TCMOS;
        /// <summary>SCSI precedence</summary>
        public bool SCSIP;
        /// <summary>Implicit Asymmetric Logical Unit Access Enabled</summary>
        public bool IALUAE;
        /// <summary>Initial task priority</summary>
        public byte InitialPriority;

        /// <summary>Device life control disabled</summary>
        public bool DLC;
        /// <summary>Maximum size of SENSE data in bytes</summary>
        public byte MaximumSenseLength;
    }

    public static ModePage_0A_S01? DecodeModePage_0A_S01(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) != 0x40)
            return null;

        if((pageResponse[0] & 0x3F) != 0x0A)
            return null;

        if(pageResponse[1] != 0x01)
            return null;

        if((pageResponse[2] << 8) + pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 32)
            return null;

        var decoded = new ModePage_0A_S01();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.IALUAE |= (pageResponse[4] & 0x01) == 0x01;
        decoded.SCSIP  |= (pageResponse[4] & 0x02) == 0x02;
        decoded.TCMOS  |= (pageResponse[4] & 0x04) == 0x04;

        decoded.InitialPriority = (byte)(pageResponse[5] & 0x0F);

        return decoded;
    }

    public static string PrettifyModePage_0A_S01(byte[] pageResponse) =>
        PrettifyModePage_0A_S01(DecodeModePage_0A_S01(pageResponse));

    public static string PrettifyModePage_0A_S01(ModePage_0A_S01? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_0A_S01 page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Control_extension_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.TCMOS)
        {
            if(page.SCSIP)
            {
                sb.AppendLine("\t" + Localization.
                                  S01_Timestamp_can_be_initialized_by_methods_outside_of_the_SCSI_standards_but_SCSI_SET_TIMESTAMP_shall_take_precedence_over_them);
            }
            else
            {
                sb.AppendLine("\t" + Localization.
                                  Timestamp_can_be_initialized_by_methods_outside_of_the_SCSI_standards);
            }
        }

        if(page.IALUAE)
            sb.AppendLine("\t" + Localization.Implicit_Asymmetric_Logical_Unit_Access_is_enabled);

        sb.AppendFormat("\t" + Localization.Initial_priority_is_0, page.InitialPriority).AppendLine();

        if(page.DLC)
            sb.AppendLine("\t" + Localization.Device_will_not_degrade_performance_to_extend_its_life);

        if(page.MaximumSenseLength > 0)
        {
            sb.AppendFormat("\t" + Localization.Maximum_sense_data_would_be_0_bytes, page.MaximumSenseLength).
               AppendLine();
        }

        return sb.ToString();
    }

#endregion Mode Page 0x0A subpage 0x01: Control Extension mode page
}