// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 3E_HP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes HP MODE PAGE 3Eh: CD-ROM Emulation/Disaster Recovery Mode page.
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
        #region HP Mode Page 0x3E: CD-ROM Emulation/Disaster Recovery Mode page
        public struct HP_ModePage_3E
        {
            /// <summary>Parameters can be saved</summary>
            public bool PS;
            public bool NonAuto;
            public bool CDmode;
        }

        public static HP_ModePage_3E? DecodeHPModePage_3E(byte[] pageResponse)
        {
            if((pageResponse?[0] & 0x40) == 0x40)
                return null;

            if((pageResponse?[0] & 0x3F) != 0x3E)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 4)
                return null;

            var decoded = new HP_ModePage_3E();

            decoded.PS      |= (pageResponse[0] & 0x80) == 0x80;
            decoded.NonAuto |= (pageResponse[2] & 0x02) == 0x02;
            decoded.CDmode  |= (pageResponse[2] & 0x01) == 0x01;

            return decoded;
        }

        public static string PrettifyHPModePage_3E(byte[] pageResponse) =>
            PrettifyHPModePage_3E(DecodeHPModePage_3E(pageResponse));

        public static string PrettifyHPModePage_3E(HP_ModePage_3E? modePage)
        {
            if(!modePage.HasValue)
                return null;

            HP_ModePage_3E page = modePage.Value;
            var            sb   = new StringBuilder();

            sb.AppendLine("HP CD-ROM Emulation/Disaster Recovery Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendLine(page.CDmode ? "\tDrive is emulating a CD-ROM drive"
                              : "\tDrive is not emulating a CD-ROM drive");

            if(page.NonAuto)
                sb.AppendLine("\tDrive will not exit emulation automatically");

            return sb.ToString();
        }
        #endregion HP Mode Page 0x3E: CD-ROM Emulation/Disaster Recovery Mode page
    }
}