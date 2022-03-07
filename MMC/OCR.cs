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

namespace Aaru.Decoders.MMC;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

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
        sb.AppendLine("MultiMediaCard Operation Conditions Register:");

        if(!ocr.PowerUp)
            sb.AppendLine("\tDevice is powering up");

        switch(ocr.AccessMode)
        {
            case 0:
                sb.AppendLine("\tDevice is byte addressed");

                break;
            case 2:
                sb.AppendLine("\tDevice is sector addressed");

                break;
            default:
                sb.AppendFormat("\tUnknown device access mode {0}", ocr.AccessMode).AppendLine();

                break;
        }

        if(ocr.ThreeFive)
            sb.AppendLine("\tDevice can work with supply 3.5~3.6V");

        if(ocr.ThreeFour)
            sb.AppendLine("\tDevice can work with supply 3.4~3.5V");

        if(ocr.ThreeThree)
            sb.AppendLine("\tDevice can work with supply 3.3~3.4V");

        if(ocr.ThreeTwo)
            sb.AppendLine("\tDevice can work with supply 3.2~3.3V");

        if(ocr.ThreeOne)
            sb.AppendLine("\tDevice can work with supply 3.1~3.2V");

        if(ocr.TwoNine)
            sb.AppendLine("\tDevice can work with supply 2.9~3.0V");

        if(ocr.TwoEight)
            sb.AppendLine("\tDevice can work with supply 2.8~2.9V");

        if(ocr.TwoSeven)
            sb.AppendLine("\tDevice can work with supply 2.7~2.8V");

        if(ocr.TwoSix)
            sb.AppendLine("\tDevice can work with supply 2.6~2.7V");

        if(ocr.TwoFive)
            sb.AppendLine("\tDevice can work with supply 2.5~2.6V");

        if(ocr.TwoFour)
            sb.AppendLine("\tDevice can work with supply 2.4~2.5V");

        if(ocr.TwoThree)
            sb.AppendLine("\tDevice can work with supply 2.3~2.4V");

        if(ocr.TwoTwo)
            sb.AppendLine("\tDevice can work with supply 2.2~2.3V");

        if(ocr.TwoOne)
            sb.AppendLine("\tDevice can work with supply 2.1~2.2V");

        if(ocr.TwoZero)
            sb.AppendLine("\tDevice can work with supply 2.0~2.1V");

        if(ocr.OneSix)
            sb.AppendLine("\tDevice can work with supply 1.65~1.95V");

        return sb.ToString();
    }

    public static string PrettifyOCR(byte[] response) => PrettifyOCR(DecodeOCR(response));

    public static string PrettifyOCR(uint response) => PrettifyOCR(DecodeOCR(response));
}