// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 07_MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 07h: Verify error recovery page for MultiMedia Devices.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
#region Mode Page 0x07: Verify error recovery page for MultiMedia Devices

    /// <summary>Verify error recovery page for MultiMedia Devices Page code 0x07 8 bytes in SCSI-2, MMC-1</summary>
    public struct ModePage_07_MMC
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Error recovery parameter</summary>
        public byte Parameter;
        /// <summary>How many times to retry a verify operation</summary>
        public byte VerifyRetryCount;
    }

    public static ModePage_07_MMC? DecodeModePage_07_MMC(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40) return null;

        if((pageResponse?[0] & 0x3F) != 0x07) return null;

        if(pageResponse[1] + 2 != pageResponse.Length) return null;

        if(pageResponse.Length < 8) return null;

        var decoded = new ModePage_07_MMC();

        decoded.PS               |= (pageResponse[0] & 0x80) == 0x80;
        decoded.Parameter        =  pageResponse[2];
        decoded.VerifyRetryCount =  pageResponse[3];

        return decoded;
    }

    public static string PrettifyModePage_07_MMC(byte[] pageResponse) =>
        PrettifyModePage_07_MMC(DecodeModePage_07_MMC(pageResponse));

    public static string PrettifyModePage_07_MMC(ModePage_07_MMC? modePage)
    {
        if(!modePage.HasValue) return null;

        ModePage_07_MMC page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Verify_error_recovery_page_for_MultiMedia_Devices);

        if(page.PS) sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.VerifyRetryCount > 0)
        {
            sb.AppendFormat("\t" + Localization.Drive_will_repeat_verify_operations_0_times, page.VerifyRetryCount)
              .AppendLine();
        }

        string AllUsed              = "\t" + Localization.All_available_recovery_procedures_will_be_used + "\n";
        string CIRCRetriesUsed      = "\t" + Localization.Only_retries_and_CIRC_are_used                 + "\n";
        string RetriesUsed          = "\t" + Localization.Only_retries_are_used                          + "\n";
        string RecoveredNotReported = "\t" + Localization.Recovered_errors_will_not_be_reported          + "\n";
        string RecoveredReported    = "\t" + Localization.Recovered_errors_will_be_reported              + "\n";

        string RecoveredAbort =
            "\t" + Localization.Recovered_errors_will_be_reported_and_aborted_with_CHECK_CONDITION + "\n";

        string UnrecECCAbort     = "\t" + Localization.Unrecovered_ECC_errors_will_return_CHECK_CONDITION;
        string UnrecCIRCAbort    = "\t" + Localization.Unrecovered_CIRC_errors_will_return_CHECK_CONDITION;
        string UnrecECCNotAbort  = "\t" + Localization.Unrecovered_ECC_errors_will_not_abort_the_transfer;
        string UnrecCIRCNotAbort = "\t" + Localization.Unrecovered_CIRC_errors_will_not_abort_the_transfer;

        string UnrecECCAbortData =
            "\t" + Localization.Unrecovered_ECC_errors_will_return_CHECK_CONDITION_and_the_uncorrected_data;

        string UnrecCIRCAbortData =
            "\t" + Localization.Unrecovered_CIRC_errors_will_return_CHECK_CONDITION_and_the_uncorrected_data;

        switch(page.Parameter)
        {
            case 0x00:
                sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbort);

                break;
            case 0x01:
                sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbort);

                break;
            case 0x04:
                sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbort);

                break;
            case 0x05:
                sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbort);

                break;
            case 0x06:
                sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbort);

                break;
            case 0x07:
                sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbort);

                break;
            case 0x10:
                sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCNotAbort);

                break;
            case 0x11:
                sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCNotAbort);

                break;
            case 0x14:
                sb.AppendLine(AllUsed + RecoveredReported + UnrecECCNotAbort);

                break;
            case 0x15:
                sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCNotAbort);

                break;
            case 0x20:
                sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbortData);

                break;
            case 0x21:
                sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbortData);

                break;
            case 0x24:
                sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbortData);

                break;
            case 0x25:
                sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbortData);

                break;
            case 0x26:
                sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbortData);

                break;
            case 0x27:
                sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbortData);

                break;
            case 0x30:
                goto case 0x10;
            case 0x31:
                goto case 0x11;
            case 0x34:
                goto case 0x14;
            case 0x35:
                goto case 0x15;
            default:
                sb.AppendFormat(Localization.Unknown_recovery_parameter_0, page.Parameter).AppendLine();

                break;
        }

        return sb.ToString();
    }

#endregion Mode Page 0x07: Verify error recovery page for MultiMedia Devices
}