// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 10.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 10h: XOR control mode page.
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
    #region Mode Page 0x10: XOR control mode page
    /// <summary>XOR control mode page Page code 0x10 24 bytes in SBC-1, SBC-2</summary>
    public struct ModePage_10
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Disables XOR operations</summary>
        public bool XORDIS;
        /// <summary>Maximum transfer length in blocks for a XOR command</summary>
        public uint MaxXorWrite;
        /// <summary>Maximum regenerate length in blocks</summary>
        public uint MaxRegenSize;
        /// <summary>Maximum transfer length in blocks for READ during a rebuild</summary>
        public uint MaxRebuildRead;
        /// <summary>Minimum time in ms between READs during a rebuild</summary>
        public ushort RebuildDelay;
    }

    public static ModePage_10? DecodeModePage_10(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x10)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 24)
            return null;

        var decoded = new ModePage_10();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.XORDIS |= (pageResponse[2] & 0x02) == 0x02;

        decoded.MaxXorWrite = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) +
                                     pageResponse[7]);

        decoded.MaxRegenSize = (uint)((pageResponse[12] << 24) + (pageResponse[13] << 16) + (pageResponse[14] << 8) +
                                      pageResponse[15]);

        decoded.MaxRebuildRead = (uint)((pageResponse[16] << 24) + (pageResponse[17] << 16) + (pageResponse[18] << 8) +
                                        pageResponse[19]);

        decoded.RebuildDelay = (ushort)((pageResponse[22] << 8) + pageResponse[23]);

        return decoded;
    }

    public static string PrettifyModePage_10(byte[] pageResponse) =>
        PrettifyModePage_10(DecodeModePage_10(pageResponse));

    public static string PrettifyModePage_10(ModePage_10? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_10 page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine("SCSI XOR control mode page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.XORDIS)
            sb.AppendLine("\tXOR operations are disabled");
        else
        {
            if(page.MaxXorWrite > 0)
                sb.AppendFormat("\tDrive accepts a maximum of {0} blocks in a single XOR WRITE command",
                                page.MaxXorWrite).AppendLine();

            if(page.MaxRegenSize > 0)
                sb.AppendFormat("\tDrive accepts a maximum of {0} blocks in a REGENERATE command", page.MaxRegenSize).
                   AppendLine();

            if(page.MaxRebuildRead > 0)
                sb.AppendFormat("\tDrive accepts a maximum of {0} blocks in a READ command during rebuild",
                                page.MaxRebuildRead).AppendLine();

            if(page.RebuildDelay > 0)
                sb.AppendFormat("\tDrive needs a minimum of {0} ms between READ commands during rebuild",
                                page.RebuildDelay).AppendLine();
        }

        return sb.ToString();
    }
    #endregion Mode Page 0x10: XOR control mode page
}