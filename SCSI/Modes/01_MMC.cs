// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 01_MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 01h: Read error recovery page for MultiMedia Devices.
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
    public static byte[] EncodeModePage_01_MMC(ModePage_01_MMC page)
    {
        var pg = new byte[12];

        pg[0] = 0x01;
        pg[1] = 10;

        if(page.PS)
            pg[0] += 0x80;

        pg[2] = page.Parameter;
        pg[3] = page.ReadRetryCount;

        // This is from a newer version of SCSI unknown what happen for drives expecting an 8 byte page

        pg[8]  = page.WriteRetryCount;
        pg[10] = (byte)((page.RecoveryTimeLimit & 0xFF00) << 8);
        pg[11] = (byte)(page.RecoveryTimeLimit & 0xFF);

        return pg;
    }

    #region Mode Page 0x01: Read error recovery page for MultiMedia Devices
    /// <summary>
    ///     Read error recovery page for MultiMedia Devices Page code 0x01 8 bytes in SCSI-2, MMC-1 12 bytes in MMC-2,
    ///     MMC-3
    /// </summary>
    public struct ModePage_01_MMC
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Error recovery parameter</summary>
        public byte Parameter;
        /// <summary>How many times to retry a read operation</summary>
        public byte ReadRetryCount;
        /// <summary>How many times to retry a write operation</summary>
        public byte WriteRetryCount;
        /// <summary>Maximum time in ms to use in data error recovery procedures</summary>
        public ushort RecoveryTimeLimit;
    }

    public static ModePage_01_MMC? DecodeModePage_01_MMC(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x01)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

        var decoded = new ModePage_01_MMC();

        decoded.PS             |= (pageResponse[0] & 0x80) == 0x80;
        decoded.Parameter      =  pageResponse[2];
        decoded.ReadRetryCount =  pageResponse[3];

        if(pageResponse.Length < 12)
            return decoded;

        decoded.WriteRetryCount   = pageResponse[8];
        decoded.RecoveryTimeLimit = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

        return decoded;
    }

    public static string PrettifyModePage_01_MMC(byte[] pageResponse) =>
        PrettifyModePage_01_MMC(DecodeModePage_01_MMC(pageResponse));

    public static string PrettifyModePage_01_MMC(ModePage_01_MMC? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_01_MMC page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine("SCSI Read error recovery page for MultiMedia Devices:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.ReadRetryCount > 0)
            sb.AppendFormat("\tDrive will repeat read operations {0} times", page.ReadRetryCount).AppendLine();

        const string AllUsed              = "\tAll available recovery procedures will be used.\n";
        const string CIRCRetriesUsed      = "\tOnly retries and CIRC are used.\n";
        const string RetriesUsed          = "\tOnly retries are used.\n";
        const string RecoveredNotReported = "\tRecovered errors will not be reported.\n";
        const string RecoveredReported    = "\tRecovered errors will be reported.\n";
        const string RecoveredAbort       = "\tRecovered errors will be reported and aborted with CHECK CONDITION.\n";
        const string UnrecECCAbort        = "\tUnrecovered ECC errors will return CHECK CONDITION.";
        const string UnrecCIRCAbort       = "\tUnrecovered CIRC errors will return CHECK CONDITION.";
        const string UnrecECCNotAbort     = "\tUnrecovered ECC errors will not abort the transfer.";
        const string UnrecCIRCNotAbort    = "\tUnrecovered CIRC errors will not abort the transfer.";

        const string UnrecECCAbortData =
            "\tUnrecovered ECC errors will return CHECK CONDITION and the uncorrected data.";

        const string UnrecCIRCAbortData =
            "\tUnrecovered CIRC errors will return CHECK CONDITION and the uncorrected data.";

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

        if(page.WriteRetryCount > 0)
            sb.AppendFormat("\tDrive will repeat write operations {0} times", page.WriteRetryCount).AppendLine();

        if(page.RecoveryTimeLimit > 0)
            sb.AppendFormat("\tDrive will employ a maximum of {0} ms to recover data", page.RecoveryTimeLimit).
               AppendLine();

        return sb.ToString();
    }
    #endregion Mode Page 0x01: Read error recovery page for MultiMedia Devices
}