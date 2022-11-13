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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Decoders.SCSI;

using System.Diagnostics.CodeAnalysis;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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

        sb.AppendLine("SCSI Control mode page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.RLEC)
            sb.AppendLine("\tIf set, target shall report log exception conditions");

        if(page.DQue)
            sb.AppendLine("\tTagged queuing is disabled");

        if(page.EECA)
            sb.AppendLine("\tExtended Contingent Allegiance is enabled");

        if(page.RAENP)
            sb.AppendLine("\tTarget may issue an asynchronous event notification upon completing its initialization");

        if(page.UAAENP)
            sb.AppendLine("\tTarget may issue an asynchronous event notification instead of a unit attention condition");

        if(page.EAENP)
            sb.AppendLine("\tTarget may issue an asynchronous event notification instead of a deferred error");

        if(page.GLTSD)
            sb.AppendLine("\tGlobal logging target save disabled");

        if(page.RAC)
            sb.AppendLine("\tCHECK CONDITION should be reported rather than a long busy condition");

        if(page.SWP)
            sb.AppendLine("\tSoftware write protect is active");

        if(page.TAS)
            sb.AppendLine("\tTasks aborted by other initiator's actions should be terminated with TASK ABORTED");

        if(page.TMF_ONLY)
            sb.AppendLine("\tAll tasks received in nexus with ACA ACTIVE is set and an ACA condition is established shall terminate");

        if(page.D_SENSE)
            sb.AppendLine("\tDevice shall return descriptor format sense data when returning sense data in the same transactions as a CHECK CONDITION");

        if(page.ATO)
            sb.AppendLine("\tLOGICAL BLOCK APPLICATION TAG should not be modified");

        if(page.DPICZ)
            sb.AppendLine("\tProtector information checking is disabled");

        if(page.NUAR)
            sb.AppendLine("\tNo unit attention on release");

        if(page.ATMPE)
            sb.AppendLine("\tApplication Tag mode page is enabled");

        if(page.RWWP)
            sb.AppendLine("\tAbort any write command without protection information");

        if(page.SBLP)
            sb.AppendLine("\tSupports block lengths and protection information");

        switch(page.TST)
        {
            case 0:
                sb.AppendLine("\tThe logical unit maintains one task set for all nexuses");

                break;
            case 1:
                sb.AppendLine("\tThe logical unit maintains separate task sets for each nexus");

                break;
            default:
                sb.AppendFormat("\tUnknown Task set type {0}", page.TST).AppendLine();

                break;
        }

        switch(page.QueueAlgorithm)
        {
            case 0:
                sb.AppendLine("\tCommands should be sent strictly ordered");

                break;
            case 1:
                sb.AppendLine("\tCommands can be reordered in any manner");

                break;
            default:
                sb.AppendFormat("\tUnknown Queue Algorithm Modifier {0}", page.QueueAlgorithm).AppendLine();

                break;
        }

        switch(page.QErr)
        {
            case 0:
                sb.AppendLine("\tIf ACA is established, the task set commands shall resume after it is cleared, otherwise they shall terminate with CHECK CONDITION");

                break;
            case 1:
                sb.AppendLine("\tAll the affected commands in the task set shall be aborted when CHECK CONDITION is returned");

                break;
            case 3:
                sb.AppendLine("\tAffected commands in the task set belonging with the CHECK CONDITION nexus shall be aborted");

                break;
            default:
                sb.AppendLine("\tReserved QErr value 2 is set");

                break;
        }

        switch(page.UA_INTLCK_CTRL)
        {
            case 0:
                sb.AppendLine("\tLUN shall clear unit attention condition reported in the same nexus");

                break;
            case 2:
                sb.AppendLine("\tLUN shall not clear unit attention condition reported in the same nexus");

                break;
            case 3:
                sb.AppendLine("\tLUN shall not clear unit attention condition reported in the same nexus and shall establish a unit attention condition for the initiator");

                break;
            default:
                sb.AppendLine("\tReserved UA_INTLCK_CTRL value 1 is set");

                break;
        }

        switch(page.AutoloadMode)
        {
            case 0:
                sb.AppendLine("\tOn medium insertion, it shall be loaded for full access");

                break;
            case 1:
                sb.AppendLine("\tOn medium insertion, it shall be loaded for auxiliary memory access only");

                break;
            case 2:
                sb.AppendLine("\tOn medium insertion, it shall not be loaded");

                break;
            default:
                sb.AppendFormat("\tReserved autoload mode {0} set", page.AutoloadMode).AppendLine();

                break;
        }

        if(page.ReadyAENHoldOffPeriod > 0)
            sb.AppendFormat("\t{0} ms before attempting asynchronous event notifications after initialization",
                            page.ReadyAENHoldOffPeriod).AppendLine();

        if(page.BusyTimeoutPeriod > 0)
            if(page.BusyTimeoutPeriod == 0xFFFF)
                sb.AppendLine("\tThere is no limit on the maximum time that is allowed to remain busy");
            else
                sb.AppendFormat("\tA maximum of {0} ms are allowed to remain busy", page.BusyTimeoutPeriod * 100).
                   AppendLine();

        if(page.ExtendedSelfTestCompletionTime > 0)
            sb.AppendFormat("\t{0} seconds to complete extended self-test", page.ExtendedSelfTestCompletionTime);

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

        sb.AppendLine("SCSI Control extension page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.TCMOS)
        {
            sb.Append("\tTimestamp can be initialized by methods outside of the SCSI standards");

            if(page.SCSIP)
                sb.Append(", but SCSI's SET TIMESTAMP shall take precedence over them");

            sb.AppendLine();
        }

        if(page.IALUAE)
            sb.AppendLine("\tImplicit Asymmetric Logical Unit Access is enabled");

        sb.AppendFormat("\tInitial priority is {0}", page.InitialPriority).AppendLine();

        if(page.DLC)
            sb.AppendLine("\tDevice will not degrade performance to extend its life");

        if(page.MaximumSenseLength > 0)
            sb.AppendFormat("\tMaximum sense data would be {0} bytes", page.MaximumSenseLength).AppendLine();

        return sb.ToString();
    }
    #endregion Mode Page 0x0A subpage 0x01: Control Extension mode page
}