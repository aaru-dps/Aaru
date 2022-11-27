// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 1D.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 1Dh: Medium Configuration Mode Page.
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
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    #region Mode Page 0x1D: Medium Configuration Mode Page
    public struct ModePage_1D
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public bool WORMM;
        public byte WormModeLabelRestrictions;
        public byte WormModeFilemarkRestrictions;
    }

    public static ModePage_1D? DecodeModePage_1D(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x1D)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 32)
            return null;

        var decoded = new ModePage_1D();

        decoded.PS                           |= (pageResponse[0] & 0x80) == 0x80;
        decoded.WORMM                        |= (pageResponse[2] & 0x01) == 0x01;
        decoded.WormModeLabelRestrictions    =  pageResponse[4];
        decoded.WormModeFilemarkRestrictions =  pageResponse[5];

        return decoded;
    }

    public static string PrettifyModePage_1D(byte[] pageResponse) =>
        PrettifyModePage_1D(DecodeModePage_1D(pageResponse));

    public static string PrettifyModePage_1D(ModePage_1D? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_1D page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Medium_Configuration_Mode_Page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.WORMM)
            sb.AppendLine("\t" + Localization.Drive_is_operating_in_WORM_mode);

        switch(page.WormModeLabelRestrictions)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Drive_does_not_allow_any_logical_blocks_to_be_overwritten);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Drive_allows_a_tape_header_to_be_overwritten);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Drive_allows_all_format_labels_to_be_overwritten);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_WORM_mode_label_restrictions_code_0,
                                page.WormModeLabelRestrictions).AppendLine();

                break;
        }

        switch(page.WormModeFilemarkRestrictions)
        {
            case 2:
                sb.AppendLine("\t" +
                              "Drive allows any number of filemarks immediately preceding EOD to be overwritten except filemark closes to BOP");

                break;
            case 3:
                sb.AppendLine("\t" +
                              "Drive allows any number of filemarks immediately preceding EOD to be overwritten");

                break;
            default:
                sb.AppendFormat("\t" + "Unknown WORM mode filemark restrictions code {0}",
                                page.WormModeLabelRestrictions).AppendLine();

                break;
        }

        return sb.ToString();
    }
    #endregion Mode Page 0x1D: Medium Configuration Mode Page
}