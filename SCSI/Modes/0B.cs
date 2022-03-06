// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 0B.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 0Bh: Medium types supported page.
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

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    #region Mode Page 0x0B: Medium types supported page
    /// <summary>Disconnect-reconnect page Page code 0x0B 8 bytes in SCSI-2</summary>
    public struct ModePage_0B
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public MediumTypes MediumType1;
        public MediumTypes MediumType2;
        public MediumTypes MediumType3;
        public MediumTypes MediumType4;
    }

    public static ModePage_0B? DecodeModePage_0B(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x0B)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

        var decoded = new ModePage_0B();

        decoded.PS          |= (pageResponse[0] & 0x80) == 0x80;
        decoded.MediumType1 =  (MediumTypes)pageResponse[4];
        decoded.MediumType2 =  (MediumTypes)pageResponse[5];
        decoded.MediumType3 =  (MediumTypes)pageResponse[6];
        decoded.MediumType4 =  (MediumTypes)pageResponse[7];

        return decoded;
    }

    public static string PrettifyModePage_0B(byte[] pageResponse) =>
        PrettifyModePage_0B(DecodeModePage_0B(pageResponse));

    public static string PrettifyModePage_0B(ModePage_0B? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_0B page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine("SCSI Medium types supported page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.MediumType1 != MediumTypes.Default)
            sb.AppendFormat("Supported medium type one: {0}", GetMediumTypeDescription(page.MediumType1)).
               AppendLine();

        if(page.MediumType2 != MediumTypes.Default)
            sb.AppendFormat("Supported medium type two: {0}", GetMediumTypeDescription(page.MediumType2)).
               AppendLine();

        if(page.MediumType3 != MediumTypes.Default)
            sb.AppendFormat("Supported medium type three: {0}", GetMediumTypeDescription(page.MediumType3)).
               AppendLine();

        if(page.MediumType4 != MediumTypes.Default)
            sb.AppendFormat("Supported medium type four: {0}", GetMediumTypeDescription(page.MediumType4)).
               AppendLine();

        return sb.ToString();
    }
    #endregion Mode Page 0x0B: Medium types supported page
}