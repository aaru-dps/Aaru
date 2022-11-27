// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 00_SFF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 00h: Drive Operation Mode page.
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
    #region Mode Page 0x00: Drive Operation Mode page
    /// <summary>Drive Operation Mode page Page code 0x00 4 bytes in INF-8070</summary>
    public struct ModePage_00_SFF
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Select LUN Mode</summary>
        public bool SLM;
        /// <summary>Select LUN for rewritable</summary>
        public bool SLR;
        /// <summary>Disable verify for WRITE</summary>
        public bool DVW;
        /// <summary>Disable deferred error</summary>
        public bool DDE;
    }

    public static ModePage_00_SFF? DecodeModePage_00_SFF(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x00)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 4)
            return null;

        var decoded = new ModePage_00_SFF();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.SLM |= (pageResponse[2] & 0x80) == 0x80;
        decoded.SLR |= (pageResponse[2] & 0x40) == 0x40;
        decoded.DVW |= (pageResponse[2] & 0x20) == 0x20;

        decoded.DDE |= (pageResponse[3] & 0x10) == 0x10;

        return decoded;
    }

    public static string PrettifyModePage_00_SFF(byte[] pageResponse) =>
        PrettifyModePage_00_SFF(DecodeModePage_00_SFF(pageResponse));

    public static string PrettifyModePage_00_SFF(ModePage_00_SFF? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_00_SFF page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Drive_Operation_Mode_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.DVW)
            sb.AppendLine("\t" + Localization.Verifying_after_writing_is_disabled);

        if(page.DDE)
            sb.AppendLine("\t" + Localization.Drive_will_abort_when_a_writing_error_is_detected);

        if(!page.SLM)
            return sb.ToString();

        sb.Append("\t" + Localization.Drive_has_two_LUNs_with_rewritable_being);
        sb.AppendLine(page.SLR ? "LUN 1" : "LUN 0");

        return sb.ToString();
    }
    #endregion Mode Page 0x00: Drive Operation Mode page
}