// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 1B.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 1Bh: Removable Block Access Capabilities page.
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
        #region Mode Page 0x1B: Removable Block Access Capabilities page
        /// <summary>Removable Block Access Capabilities page Page code 0x1B 12 bytes in INF-8070</summary>
        public struct ModePage_1B
        {
            /// <summary>Parameters can be saved</summary>
            public bool PS;
            /// <summary>Supports reporting progress of format</summary>
            public bool SRFP;
            /// <summary>Non-CD Optical Device</summary>
            public bool NCD;
            /// <summary>Phase change dual device supporting a CD and a Non-CD Optical devices</summary>
            public bool SML;
            /// <summary>Total number of LUNs</summary>
            public byte TLUN;
            /// <summary>System Floppy Type device</summary>
            public bool SFLP;
        }

        public static ModePage_1B? DecodeModePage_1B(byte[] pageResponse)
        {
            if((pageResponse?[0] & 0x40) == 0x40)
                return null;

            if((pageResponse?[0] & 0x3F) != 0x1B)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 12)
                return null;

            var decoded = new ModePage_1B();

            decoded.PS   |= (pageResponse[0] & 0x80) == 0x80;
            decoded.SFLP |= (pageResponse[2] & 0x80) == 0x80;
            decoded.SRFP |= (pageResponse[2] & 0x40) == 0x40;
            decoded.NCD  |= (pageResponse[3] & 0x80) == 0x80;
            decoded.SML  |= (pageResponse[3] & 0x40) == 0x40;

            decoded.TLUN = (byte)(pageResponse[3] & 0x07);

            return decoded;
        }

        public static string PrettifyModePage_1B(byte[] pageResponse) =>
            PrettifyModePage_1B(DecodeModePage_1B(pageResponse));

        public static string PrettifyModePage_1B(ModePage_1B? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_1B page = modePage.Value;
            var         sb   = new StringBuilder();

            sb.AppendLine("SCSI Removable Block Access Capabilities page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.SFLP)
                sb.AppendLine("\tDrive can be used as a system floppy device");

            if(page.SRFP)
                sb.AppendLine("\tDrive supports reporting progress of format");

            if(page.NCD)
                sb.AppendLine("\tDrive is a Non-CD Optical Device");

            if(page.SML)
                sb.AppendLine("\tDevice is a dual device supporting CD and Non-CD Optical");

            if(page.TLUN > 0)
                sb.AppendFormat("\tDrive supports {0} LUNs", page.TLUN).AppendLine();

            return sb.ToString();
        }
        #endregion Mode Page 0x1B: Removable Block Access Capabilities page
    }
}