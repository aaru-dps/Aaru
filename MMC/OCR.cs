// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : OCR.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes MultiMediaCard OCR.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.MMC;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public class OCR
{
    public byte AccessMode;
    public bool OneSix;
    public bool PowerUp;
    public bool ThreeFive;
    public bool ThreeFour;
    public bool ThreeOne;
    public bool ThreeThree;
    public bool ThreeTwo;
    public bool ThreeZero;
    public bool TwoEight;
    public bool TwoFive;
    public bool TwoFour;
    public bool TwoNine;
    public bool TwoOne;
    public bool TwoSeven;
    public bool TwoSix;
    public bool TwoThree;
    public bool TwoTwo;
    public bool TwoZero;
}

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Decoders
{
    public static OCR DecodeOCR(uint response)
    {
        response = Swapping.Swap(response);

        return new OCR
        {
            PowerUp    = (response & 0x80000000) == 0x80000000,
            AccessMode = (byte)((response & 0x60000000) >> 29),
            ThreeFive  = (response & 0x00800000) == 0x00800000,
            ThreeFour  = (response & 0x00400000) == 0x00400000,
            ThreeThree = (response & 0x00200000) == 0x00200000,
            ThreeTwo   = (response & 0x00100000) == 0x00100000,
            ThreeOne   = (response & 0x00080000) == 0x00080000,
            ThreeZero  = (response & 0x00040000) == 0x00040000,
            TwoNine    = (response & 0x00020000) == 0x00020000,
            TwoEight   = (response & 0x00010000) == 0x00010000,
            TwoSeven   = (response & 0x00008000) == 0x00008000,
            TwoSix     = (response & 0x00004000) == 0x00004000,
            TwoFive    = (response & 0x00002000) == 0x00002000,
            TwoFour    = (response & 0x00001000) == 0x00001000,
            TwoThree   = (response & 0x00000800) == 0x00000800,
            TwoTwo     = (response & 0x00000400) == 0x00000400,
            TwoOne     = (response & 0x00000200) == 0x00000200,
            TwoZero    = (response & 0x00000100) == 0x00000100,
            OneSix     = (response & 0x00000080) == 0x00000080
        };
    }

    public static OCR DecodeOCR(byte[] response) =>
        response?.Length != 4 ? null : DecodeOCR(BitConverter.ToUInt32(response, 0));

    public static string PrettifyOCR(OCR ocr)
    {
        if(ocr == null)
            return null;

        var sb = new StringBuilder();
        sb.AppendLine(Localization.MultiMediaCard_Operation_Conditions_Register);

        if(!ocr.PowerUp)
            sb.AppendLine("\t" + Localization.Device_is_powering_up);

        switch(ocr.AccessMode)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_is_byte_addressed);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_is_sector_addressed);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_device_access_mode_0, ocr.AccessMode).AppendLine();

                break;
        }

        if(ocr.ThreeFive)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_3_5_3_6V);

        if(ocr.ThreeFour)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_3_4_3_5V);

        if(ocr.ThreeThree)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_3_3_3_4V);

        if(ocr.ThreeTwo)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_3_2_3_3V);

        if(ocr.ThreeOne)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_3_1_3_2V);

        if(ocr.TwoNine)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_9_3_0V);

        if(ocr.TwoEight)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_8_2_9V);

        if(ocr.TwoSeven)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_7_2_8V);

        if(ocr.TwoSix)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_6_2_7V);

        if(ocr.TwoFive)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_5_2_6V);

        if(ocr.TwoFour)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_4_2_5V);

        if(ocr.TwoThree)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_3_2_4V);

        if(ocr.TwoTwo)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_2_2_3V);

        if(ocr.TwoOne)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_1_2_2V);

        if(ocr.TwoZero)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_2_0_2_1V);

        if(ocr.OneSix)
            sb.AppendLine("\t" + Localization.Device_can_work_with_supply_1_65_1_95V);

        return sb.ToString();
    }

    public static string PrettifyOCR(byte[] response) => PrettifyOCR(DecodeOCR(response));

    public static string PrettifyOCR(uint response) => PrettifyOCR(DecodeOCR(response));
}