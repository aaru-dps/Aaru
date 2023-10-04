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

        sb.AppendLine(Localization.Certance_Drive_Capabilities_Control_Mode_Page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        switch(page.OperatingSystemsSupport)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Operating_systems_support_is_standard_LTO);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Operating_systems_support_is_unknown_code_0,
                                page.OperatingSystemsSupport).
                   AppendLine();

                break;
        }

        if(page.FirmwareTestControl == page.FirmwareTestControl2)
        {
            switch(page.FirmwareTestControl)
            {
                case 0:
                    sb.AppendLine("\t" + Localization.Factory_test_code_is_disabled);

                    break;
                case 1:
                    sb.AppendLine("\t" + Localization.Factory_test_code_1_is_disabled);

                    break;
                case 2:
                    sb.AppendLine("\t" + Localization.Factory_test_code_2_is_disabled);

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Unknown_factory_test_code_0, page.FirmwareTestControl).
                       AppendLine();

                    break;
            }
        }

        switch(page.ExtendedPOSTMode)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Power_On_Self_Test_is_enabled);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Power_On_Self_Test_is_disabled);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_Power_On_Self_Test_code_0, page.ExtendedPOSTMode).
                   AppendLine();

                break;
        }

        switch(page.DataCompressionControl)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Compression_is_controlled_using_mode_pages_0Fh_and_10h);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Compression_is_enabled_and_not_controllable);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Compression_is_disabled_and_not_controllable);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_compression_control_code_0, page.DataCompressionControl).
                   AppendLine();

                break;
        }

        if(page.HostUnloadOverride)
            sb.AppendLine("\t" + Localization.SCSI_UNLOAD_command_will_not_eject_the_cartridge);

        sb.Append("\t" +
                  Localization.
                      How_should_tapes_be_unloaded_in_a_power_cycle_tape_incompatibility_firmware_download_or_cleaning_end);

        switch(page.AutoUnloadMode)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Tape_will_stay_threaded_at_beginning);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Tape_will_be_unthreaded);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Tape_will_be_unthreaded_and_unloaded);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Data_tapes_will_be_threaded_at_beginning_rest_will_be_unloaded);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_auto_unload_code_0, page.AutoUnloadMode).AppendLine();

                break;
        }

        return sb.ToString();
    }

#endregion Certance Mode Page 0x21: Drive Capabilities Control Mode page
}