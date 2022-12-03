// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 3B_HP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes HP MODE PAGE 3Bh: Serial Number Override Mode page.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    #region HP Mode Page 0x3B: Serial Number Override Mode page
    public struct HP_ModePage_3B
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public byte   MSN;
        public byte[] SerialNumber;
    }

    public static HP_ModePage_3B? DecodeHPModePage_3B(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x3B)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length != 16)
            return null;

        var decoded = new HP_ModePage_3B();

        decoded.PS           |= (pageResponse[0]       & 0x80) == 0x80;
        decoded.MSN          =  (byte)(pageResponse[2] & 0x03);
        decoded.SerialNumber =  new byte[10];
        Array.Copy(pageResponse, 6, decoded.SerialNumber, 0, 10);

        return decoded;
    }

    public static string PrettifyHPModePage_3B(byte[] pageResponse) =>
        PrettifyHPModePage_3B(DecodeHPModePage_3B(pageResponse));

    public static string PrettifyHPModePage_3B(HP_ModePage_3B? modePage)
    {
        if(!modePage.HasValue)
            return null;

        HP_ModePage_3B page = modePage.Value;
        var            sb   = new StringBuilder();

        sb.AppendLine(Localization.HP_Serial_Number_Override_Mode_Page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        switch(page.MSN)
        {
            case 1:
                sb.AppendLine("\t" + Localization.Serial_number_is_the_manufacturer_default_value);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Serial_number_is_not_the_manufacturer_default_value);

                break;
        }

        sb.AppendFormat("\t" + Localization.Serial_number_0, StringHandlers.CToString(page.SerialNumber)).AppendLine();

        return sb.ToString();
    }
    #endregion HP Mode Page 0x3B: Serial Number Override Mode page
}