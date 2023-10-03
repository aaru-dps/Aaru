// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 3D_HP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes HP MODE PAGE 3Dh: Extended Reset Mode page.
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
#region HP Mode Page 0x3D: Extended Reset Mode page

    public struct HP_ModePage_3D
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public byte ResetBehaviour;
    }

    public static HP_ModePage_3D? DecodeHPModePage_3D(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x3D)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length != 4)
            return null;

        var decoded = new HP_ModePage_3D();

        decoded.PS             |= (pageResponse[0]       & 0x80) == 0x80;
        decoded.ResetBehaviour =  (byte)(pageResponse[2] & 0x03);

        return decoded;
    }

    public static string PrettifyHPModePage_3D(byte[] pageResponse) =>
        PrettifyHPModePage_3D(DecodeHPModePage_3D(pageResponse));

    public static string PrettifyHPModePage_3D(HP_ModePage_3D? modePage)
    {
        if(!modePage.HasValue)
            return null;

        HP_ModePage_3D page = modePage.Value;
        var            sb   = new StringBuilder();

        sb.AppendLine(Localization.HP_Extended_Reset_Mode_Page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        switch(page.ResetBehaviour)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Normal_reset_behaviour);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Drive_will_flush_and_position_itself_on_a_LUN_or_target_reset);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Drive_will_maintain_position_on_a_LUN_or_target_reset);

                break;
        }

        return sb.ToString();
    }

#endregion HP Mode Page 0x3D: Extended Reset Mode page
}