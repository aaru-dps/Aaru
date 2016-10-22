// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : OCR.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;

namespace DiscImageChef.Decoders.SecureDigital
{
    public class OCR
    {
        public bool PowerUp;
        public bool CCS;
        public bool UHS;
        public bool OneEight;
        public bool ThreeFive;
        public bool ThreeFour;
        public bool ThreeThree;
        public bool ThreeTwo;
        public bool ThreeOne;
        public bool ThreeZero;
        public bool TwoNine;
        public bool TwoEight;
        public bool TwoSeven;
        public bool LowPower;
    }

    public partial class Decoders
    {
        public static OCR DecodeOCR(uint response)
        {
            OCR ocr = new OCR();

            ocr.PowerUp = (response & 0x80000000) == 0x80000000;
            ocr.CCS = (response & 0x40000000) == 0x40000000;
            ocr.PowerUp = (response & 0x20000000) == 0x20000000;
            ocr.OneEight = (response & 0x01000000) == 0x01000000;
            ocr.ThreeFive = (response & 0x00800000) == 0x00800000;
            ocr.ThreeFour = (response & 0x00400000) == 0x00400000;
            ocr.ThreeThree = (response & 0x00200000) == 0x00200000;
            ocr.ThreeTwo = (response & 0x00100000) == 0x00100000;
            ocr.ThreeOne = (response & 0x00080000) == 0x00080000;
            ocr.ThreeZero = (response & 0x00040000) == 0x00040000;
            ocr.TwoNine = (response & 0x00020000) == 0x00020000;
            ocr.TwoEight = (response & 0x00010000) == 0x00010000;
            ocr.TwoSeven = (response & 0x00008000) == 0x00008000;
            ocr.LowPower = (response & 0x00000080) == 0x00000080;

            return ocr;
        }

        public static OCR DecodeOCR(byte[] response)
        {
            if(response == null)
                return null;

            if(response.Length != 4)
                return null;

            return DecodeOCR(BitConverter.ToUInt32(response, 0));
        }

        public static string PrettifyOCR(OCR ocr)
        {
            if(ocr == null)
                return null;

            StringBuilder sb = new StringBuilder();
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

        public static string PrettifyOCR(byte[] response)
        {
            return PrettifyOCR(DecodeOCR(response));
        }

        public static string PrettifyOCR(uint response)
        {
            return PrettifyOCR(DecodeOCR(response));
        }
    }
}
