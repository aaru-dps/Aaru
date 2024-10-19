// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 07.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 07h: Verify error recovery page.
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
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static partial class Modes
{
#region Mode Page 0x07: Verify error recovery page

    /// <summary>Disconnect-reconnect page Page code 0x07 12 bytes in SCSI-2, SBC-1, SBC-2</summary>
    public struct ModePage_07
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Enable early recovery</summary>
        public bool EER;
        /// <summary>Post error reporting</summary>
        public bool PER;
        /// <summary>Disable transfer on error</summary>
        public bool DTE;
        /// <summary>Disable correction</summary>
        public bool DCR;
        /// <summary>How many times to retry a verify operation</summary>
        public byte VerifyRetryCount;
        /// <summary>How many bits of largest data burst error is maximum to apply error correction on it</summary>
        public byte CorrectionSpan;
        /// <summary>Maximum time in ms to use in data error recovery procedures</summary>
        public ushort RecoveryTimeLimit;
    }

    public static ModePage_07? DecodeModePage_07(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40) return null;

        if((pageResponse?[0] & 0x3F) != 0x07) return null;

        if(pageResponse[1] + 2 != pageResponse.Length) return null;

        if(pageResponse.Length < 12) return null;

        var decoded = new ModePage_07();

        decoded.PS  |= (pageResponse[0] & 0x80) == 0x80;
        decoded.EER |= (pageResponse[2] & 0x08) == 0x08;
        decoded.PER |= (pageResponse[2] & 0x04) == 0x04;
        decoded.DTE |= (pageResponse[2] & 0x02) == 0x02;
        decoded.DCR |= (pageResponse[2] & 0x01) == 0x01;

        decoded.VerifyRetryCount  = pageResponse[3];
        decoded.CorrectionSpan    = pageResponse[4];
        decoded.RecoveryTimeLimit = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

        return decoded;
    }

    public static string PrettifyModePage_07(byte[] pageResponse) =>
        PrettifyModePage_07(DecodeModePage_07(pageResponse));

    public static string PrettifyModePage_07(ModePage_07? modePage)
    {
        if(!modePage.HasValue) return null;

        ModePage_07 page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Verify_error_recovery_page);

        if(page.PS) sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.EER) sb.AppendLine("\t" + Localization.Drive_will_use_the_most_expedient_form_of_error_recovery_first);

        if(page.PER) sb.AppendLine("\t" + Localization.Drive_shall_report_recovered_errors);

        if(page.DTE) sb.AppendLine("\t" + Localization.Transfer_will_be_terminated_upon_error_detection);

        if(page.DCR) sb.AppendLine("\t" + Localization.Error_correction_is_disabled);

        if(page.VerifyRetryCount > 0)
        {
            sb.AppendFormat("\t" + Localization.Drive_will_repeat_verify_operations_0_times, page.VerifyRetryCount)
              .AppendLine();
        }

        if(page.RecoveryTimeLimit > 0)
        {
            sb.AppendFormat("\t" + Localization.Drive_will_employ_a_maximum_of_0_ms_to_recover_data,
                            page.RecoveryTimeLimit)
              .AppendLine();
        }

        return sb.ToString();
    }

#endregion Mode Page 0x07: Verify error recovery page
}