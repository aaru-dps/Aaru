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
//     Decodes SecureDigital OCR.
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

namespace Aaru.Decoders.SecureDigital
{
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "UnassignedField.Global"),
     SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public class OCR
    {
        public bool CCS;
        public bool LowPower;
        public bool OneEight;
        public bool PowerUp;
        public bool ThreeFive;
        public bool ThreeFour;
        public bool ThreeOne;
        public bool ThreeThree;
        public bool ThreeTwo;
        public bool ThreeZero;
        public bool TwoEight;
        public bool TwoNine;
        public bool TwoSeven;
        public bool UHS;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static partial class Decoders
    {
        public static OCR DecodeOCR(uint response)
        {
            response = Swapping.Swap(response);

            return new OCR
            {
                PowerUp    = (response & 0x80000000) == 0x80000000,
                CCS        = (response & 0x40000000) == 0x40000000,
                UHS        = (response & 0x20000000) == 0x20000000,
                OneEight   = (response & 0x01000000) == 0x01000000,
                ThreeFive  = (response & 0x00800000) == 0x00800000,
                ThreeFour  = (response & 0x00400000) == 0x00400000,
                ThreeThree = (response & 0x00200000) == 0x00200000,
                ThreeTwo   = (response & 0x00100000) == 0x00100000,
                ThreeOne   = (response & 0x00080000) == 0x00080000,
                ThreeZero  = (response & 0x00040000) == 0x00040000,
                TwoNine    = (response & 0x00020000) == 0x00020000,
                TwoEight   = (response & 0x00010000) == 0x00010000,
                TwoSeven   = (response & 0x00008000) == 0x00008000,
                LowPower   = (response & 0x00000080) == 0x00000080
            };
        }

        public static OCR DecodeOCR(byte[] response) =>
            response?.Length != 4 ? null : DecodeOCR(BitConverter.ToUInt32(response, 0));

        public static string PrettifyOCR(OCR ocr)
        {
            if(ocr == null)
                return null;

            var sb = new StringBuilder();
            sb.AppendLine("SecureDigital Operation Conditions Register:");

            if(!ocr.PowerUp)
                sb.AppendLine("\tDevice is powering up");

            if(ocr.CCS)
                sb.AppendLine("\tDevice is SDHC, SDXC or higher");

            if(ocr.UHS)
                sb.AppendLine("\tDevice is UHS-II or higher");

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

            if(ocr.OneEight)
                sb.AppendLine("\tDevice can switch to work with 1.8V supply");

            if(ocr.LowPower)
                sb.AppendLine("\tDevice is in low power mode");

            return sb.ToString();
        }

        public static string PrettifyOCR(byte[] response) => PrettifyOCR(DecodeOCR(response));

        public static string PrettifyOCR(uint response) => PrettifyOCR(DecodeOCR(response));
    }
}