// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 06.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 06h: Optical memory page.
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

namespace Aaru.Decoders.SCSI
{
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static partial class Modes
    {
        #region Mode Page 0x06: Optical memory page
        /// <summary>Optical memory page Page code 0x06 4 bytes in SCSI-2</summary>
        public struct ModePage_06
        {
            /// <summary>Parameters can be saved</summary>
            public bool PS;
            /// <summary>Report updated block read</summary>
            public bool RUBR;
        }

        public static ModePage_06? DecodeModePage_06(byte[] pageResponse)
        {
            if((pageResponse?[0] & 0x40) == 0x40)
                return null;

            if((pageResponse?[0] & 0x3F) != 0x06)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 4)
                return null;

            var decoded = new ModePage_06();

            decoded.PS   |= (pageResponse[0] & 0x80) == 0x80;
            decoded.RUBR |= (pageResponse[2] & 0x01) == 0x01;

            return decoded;
        }

        public static string PrettifyModePage_06(byte[] pageResponse) =>
            PrettifyModePage_06(DecodeModePage_06(pageResponse));

        public static string PrettifyModePage_06(ModePage_06? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_06 page = modePage.Value;
            var         sb   = new StringBuilder();

            sb.AppendLine("SCSI optical memory:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.RUBR)
                sb.AppendLine("\tOn reading an updated block drive will return RECOVERED ERROR");

            return sb.ToString();
        }
        #endregion Mode Page 0x06: Optical memory page
    }
}