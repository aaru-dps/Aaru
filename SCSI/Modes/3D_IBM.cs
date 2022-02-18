// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 3D_IBM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes IBM MODE PAGE 3D: Behaviour Configuration Mode page.
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
        #region IBM Mode Page 0x3D: Behaviour Configuration Mode page
        public struct IBM_ModePage_3D
        {
            /// <summary>Parameters can be saved</summary>
            public bool PS;
            public ushort NumberOfWraps;
        }

        public static IBM_ModePage_3D? DecodeIBMModePage_3D(byte[] pageResponse)
        {
            if((pageResponse?[0] & 0x40) == 0x40)
                return null;

            if((pageResponse?[0] & 0x3F) != 0x3D)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 5)
                return null;

            var decoded = new IBM_ModePage_3D();

            decoded.PS            |= (pageResponse[0] & 0x80) == 0x80;
            decoded.NumberOfWraps =  (ushort)((pageResponse[3] << 8) + pageResponse[4]);

            return decoded;
        }

        public static string PrettifyIBMModePage_3D(byte[] pageResponse) =>
            PrettifyIBMModePage_3D(DecodeIBMModePage_3D(pageResponse));

        public static string PrettifyIBMModePage_3D(IBM_ModePage_3D? modePage)
        {
            if(!modePage.HasValue)
                return null;

            IBM_ModePage_3D page = modePage.Value;
            var             sb   = new StringBuilder();

            sb.AppendLine("IBM LEOT Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\t{0} wraps", page.NumberOfWraps).AppendLine();

            return sb.ToString();
        }
        #endregion IBM Mode Page 0x3D: Behaviour Configuration Mode page
    }
}