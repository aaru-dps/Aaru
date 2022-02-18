// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 12_13_14.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGEs 12h, 13h, 14h: Medium partition page (2-4).
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
        #region Mode Pages 0x12, 0x13, 0x14: Medium partition page (2-4)
        /// <summary>Medium partition page (2-4) Page codes 0x12, 0x13 and 0x14</summary>
        public struct ModePage_12_13_14
        {
            /// <summary>Parameters can be saved</summary>
            public bool PS;
            /// <summary>Array of partition sizes in units defined in mode page 11</summary>
            public ushort[] PartitionSizes;
        }

        public static ModePage_12_13_14? DecodeModePage_12_13_14(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x12 &&
               (pageResponse[0] & 0x3F) != 0x13 &&
               (pageResponse[0] & 0x3F) != 0x14)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 2)
                return null;

            var decoded = new ModePage_12_13_14();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.PartitionSizes = new ushort[(pageResponse.Length - 2) / 2];

            for(int i = 2; i < pageResponse.Length; i += 2)
            {
                decoded.PartitionSizes[(i - 2) / 2] =  (ushort)(pageResponse[i] << 8);
                decoded.PartitionSizes[(i - 2) / 2] += pageResponse[i + 1];
            }

            return decoded;
        }

        public static string PrettifyModePage_12_13_14(byte[] pageResponse) =>
            PrettifyModePage_12_13_14(DecodeModePage_12_13_14(pageResponse));

        public static string PrettifyModePage_12_13_14(ModePage_12_13_14? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_12_13_14 page = modePage.Value;
            var               sb   = new StringBuilder();

            sb.AppendLine("SCSI medium partition page (extra):");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\tMedium has defined {0} partitions", page.PartitionSizes.Length).AppendLine();

            for(int i = 0; i < page.PartitionSizes.Length; i++)
                sb.AppendFormat("\tPartition {0} is {1} units long", i, page.PartitionSizes[i]).AppendLine();

            return sb.ToString();
        }
        #endregion Mode Pages 0x12, 0x13, 0x14: Medium partition page (2-4)
    }
}