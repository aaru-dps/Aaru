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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Decoders.SCSI;

using System.Diagnostics.CodeAnalysis;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x07)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

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
        if(!modePage.HasValue)
            return null;

        ModePage_07_MMC page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine("SCSI Verify error recovery page for MultiMedia Devices:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.VerifyRetryCount > 0)
            sb.AppendFormat("\tDrive will repeat verify operations {0} times", page.VerifyRetryCount).AppendLine();

        var AllUsed              = "\tAll available recovery procedures will be used.\n";
        var CIRCRetriesUsed      = "\tOnly retries and CIRC are used.\n";
        var RetriesUsed          = "\tOnly retries are used.\n";
        var RecoveredNotReported = "\tRecovered errors will not be reported.\n";
        var RecoveredReported    = "\tRecovered errors will be reported.\n";
        var RecoveredAbort       = "\tRecovered errors will be reported and aborted with CHECK CONDITION.\n";
        var UnrecECCAbort        = "\tUnrecovered ECC errors will return CHECK CONDITION.";
        var UnrecCIRCAbort       = "\tUnrecovered CIRC errors will return CHECK CONDITION.";
        var UnrecECCNotAbort     = "\tUnrecovered ECC errors will not abort the transfer.";
        var UnrecCIRCNotAbort    = "\tUnrecovered CIRC errors will not abort the transfer.";

        var UnrecECCAbortData = "\tUnrecovered ECC errors will return CHECK CONDITION and the uncorrected data.";

        var UnrecCIRCAbortData = "\tUnrecovered CIRC errors will return CHECK CONDITION and the uncorrected data.";

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
            case 0x30: goto case 0x10;
            case 0x31: goto case 0x11;
            case 0x34: goto case 0x14;
            case 0x35: goto case 0x15;
            default:
                sb.AppendFormat("Unknown recovery parameter 0x{0:X2}", page.Parameter).AppendLine();

                break;
        }

        return sb.ToString();
    }
    #endregion Mode Page 0x07: Verify error recovery page for MultiMedia Devices
}