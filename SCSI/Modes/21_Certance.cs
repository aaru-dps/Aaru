// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 21_Certance.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Certance MODE PAGE 21h: Drive Capabilities Control Mode page.
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
    #region Certance Mode Page 0x21: Drive Capabilities Control Mode page
    public struct Certance_ModePage_21
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public byte OperatingSystemsSupport;
        public byte FirmwareTestControl2;
        public byte ExtendedPOSTMode;
        public byte InquiryStringControl;
        public byte FirmwareTestControl;
        public byte DataCompressionControl;
        public bool HostUnloadOverride;
        public byte AutoUnloadMode;
    }

    public static Certance_ModePage_21? DecodeCertanceModePage_21(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x21)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length != 9)
            return null;

        var decoded = new Certance_ModePage_21();

        decoded.PS                      |= (pageResponse[0] & 0x80) == 0x80;
        decoded.OperatingSystemsSupport =  pageResponse[2];
        decoded.FirmwareTestControl2    =  pageResponse[3];
        decoded.ExtendedPOSTMode        =  pageResponse[4];
        decoded.InquiryStringControl    =  pageResponse[5];
        decoded.FirmwareTestControl     =  pageResponse[6];
        decoded.DataCompressionControl  =  pageResponse[7];
        decoded.HostUnloadOverride      |= (pageResponse[8]       & 0x80) == 0x80;
        decoded.AutoUnloadMode          =  (byte)(pageResponse[8] & 0x7F);

        return decoded;
    }

    public static string PrettifyCertanceModePage_21(byte[] pageResponse) =>
        PrettifyCertanceModePage_21(DecodeCertanceModePage_21(pageResponse));

    public static string PrettifyCertanceModePage_21(Certance_ModePage_21? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Certance_ModePage_21 page = modePage.Value;
        var                  sb   = new StringBuilder();

        sb.AppendLine("Certance Drive Capabilities Control Mode Page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        switch(page.OperatingSystemsSupport)
        {
            case 0:
                sb.AppendLine("\tOperating systems support is standard LTO");

                break;
            default:
                sb.AppendFormat("\tOperating systems support is unknown code {0}", page.OperatingSystemsSupport).
                   AppendLine();

                break;
        }

        if(page.FirmwareTestControl == page.FirmwareTestControl2)
            switch(page.FirmwareTestControl)
            {
                case 0:
                    sb.AppendLine("\tFactory test code is disabled");

                    break;
                case 1:
                    sb.AppendLine("\tFactory test code 1 is disabled");

                    break;
                case 2:
                    sb.AppendLine("\tFactory test code 2 is disabled");

                    break;
                default:
                    sb.AppendFormat("\tUnknown factory test code {0}", page.FirmwareTestControl).AppendLine();

                    break;
            }

        switch(page.ExtendedPOSTMode)
        {
            case 0:
                sb.AppendLine("\tPower-On Self-Test is enabled");

                break;
            case 1:
                sb.AppendLine("\tPower-On Self-Test is disable");

                break;
            default:
                sb.AppendFormat("\tUnknown Power-On Self-Test code {0}", page.ExtendedPOSTMode).AppendLine();

                break;
        }

        switch(page.DataCompressionControl)
        {
            case 0:
                sb.AppendLine("\tCompression is controlled using mode pages 0Fh and 10h");

                break;
            case 1:
                sb.AppendLine("\tCompression is enabled and not controllable");

                break;
            case 2:
                sb.AppendLine("\tCompression is disabled and not controllable");

                break;
            default:
                sb.AppendFormat("\tUnknown compression control code {0}", page.DataCompressionControl).AppendLine();

                break;
        }

        if(page.HostUnloadOverride)
            sb.AppendLine("\tSCSI UNLOAD command will not eject the cartridge");

        sb.Append("\tHow should tapes be unloaded in a power cycle, tape incompatibility, firmware download or cleaning end: ");

        switch(page.AutoUnloadMode)
        {
            case 0:
                sb.AppendLine("\tTape will stay threaded at beginning");

                break;
            case 1:
                sb.AppendLine("\tTape will be unthreaded");

                break;
            case 2:
                sb.AppendLine("\tTape will be unthreaded and unloaded");

                break;
            case 3:
                sb.AppendLine("\tData tapes will be threaded at beginning, rest will be unloaded");

                break;
            default:
                sb.AppendFormat("\tUnknown auto unload code {0}", page.AutoUnloadMode).AppendLine();

                break;
        }

        return sb.ToString();
    }
    #endregion Certance Mode Page 0x21: Drive Capabilities Control Mode page
}