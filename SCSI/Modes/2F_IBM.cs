// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 2F_IBM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes IBM MODE PAGE 2Fh: Behaviour Configuration Mode page.
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

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "UnassignedField.Global")]
public static partial class Modes
{
    #region IBM Mode Page 0x2F: Behaviour Configuration Mode page
    public struct IBM_ModePage_2F
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public byte FenceBehaviour;
        public byte CleanBehaviour;
        public byte WORMEmulation;
        public byte SenseDataBehaviour;
        public bool CCDM;
        public bool DDEOR;
        public bool CLNCHK;
        public byte FirmwareUpdateBehaviour;
        public byte UOE_D;
        public byte UOE_F;
        public byte UOE_C;
    }

    public static IBM_ModePage_2F? DecodeIBMModePage_2F(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x2F)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

        return new IBM_ModePage_2F
        {
            PS                      = (pageResponse[0] & 0x80) == 0x80,
            FenceBehaviour          = pageResponse[2],
            CleanBehaviour          = pageResponse[3],
            WORMEmulation           = pageResponse[4],
            SenseDataBehaviour      = pageResponse[5],
            CCDM                    = (pageResponse[6] & 0x04) == 0x04,
            DDEOR                   = (pageResponse[6] & 0x02) == 0x02,
            CLNCHK                  = (pageResponse[6] & 0x01) == 0x01,
            FirmwareUpdateBehaviour = pageResponse[7],
            UOE_C                   = (byte)((pageResponse[8] & 0x30) >> 4),
            UOE_F                   = (byte)((pageResponse[8] & 0x0C) >> 2)
        };
    }

    public static string PrettifyIBMModePage_2F(byte[] pageResponse) =>
        PrettifyIBMModePage_2F(DecodeIBMModePage_2F(pageResponse));

    public static string PrettifyIBMModePage_2F(IBM_ModePage_2F? modePage)
    {
        if(!modePage.HasValue)
            return null;

        IBM_ModePage_2F page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.IBM_Behaviour_Configuration_Mode_Page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        switch(page.FenceBehaviour)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Fence_behaviour_is_normal);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Panic_fence_behaviour_is_enabled);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_fence_behaviour_code_0, page.FenceBehaviour).AppendLine();

                break;
        }

        switch(page.CleanBehaviour)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Cleaning_behaviour_is_normal);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Drive_will_periodically_request_cleaning);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_cleaning_behaviour_code_0, page.CleanBehaviour).
                   AppendLine();

                break;
        }

        switch(page.WORMEmulation)
        {
            case 0:
                sb.AppendLine("\t" + Localization.WORM_emulation_is_disabled);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.WORM_emulation_is_enabled);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_WORM_emulation_code_0, page.WORMEmulation).AppendLine();

                break;
        }

        switch(page.SenseDataBehaviour)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Uses_35_bytes_sense_data);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Uses_96_bytes_sense_data);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_sense_data_behaviour_code_0, page.WORMEmulation).
                   AppendLine();

                break;
        }

        if(page.CLNCHK)
            sb.AppendLine("\t" + Localization.Drive_will_set_Check_Condition_when_cleaning_is_needed);

        if(page.DDEOR)
            sb.AppendLine("\t" + Localization.No_deferred_error_will_be_reported_to_a_rewind_command);

        if(page.CCDM)
            sb.AppendLine("\t" + Localization.Drive_will_set_Check_Condition_when_the_criteria_for_Dead_Media_is_met);

        if(page.FirmwareUpdateBehaviour > 0)
            sb.AppendLine("\t" + Localization.Drive_will_not_accept_downlevel_firmware_via_an_FMR_tape);

        if(page.UOE_C == 1)
            sb.AppendLine("\t" + Localization.Drive_will_eject_cleaning_cartridges_on_error);

        if(page.UOE_F == 1)
            sb.AppendLine("\t" + Localization.Drive_will_eject_firmware_cartridges_on_error);

        if(page.UOE_D == 1)
            sb.AppendLine("\t" + Localization.Drive_will_eject_data_cartridges_on_error);

        return sb.ToString();
    }
    #endregion IBM Mode Page 0x2F: Behaviour Configuration Mode page
}