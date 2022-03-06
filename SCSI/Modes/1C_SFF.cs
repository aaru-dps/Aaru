// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 1C_SFF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 1Ch: Timer & Protect page.
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
    #region Mode Page 0x1C: Timer & Protect page
    /// <summary>Timer &amp; Protect page Page code 0x1C 8 bytes in INF-8070</summary>
    public struct ModePage_1C_SFF
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Time the device shall remain in the current state after seek, read or write operation</summary>
        public byte InactivityTimeMultiplier;
        /// <summary>Disabled until power cycle</summary>
        public bool DISP;
        /// <summary>Software Write Protect until Power-down</summary>
        public bool SWPP;
    }

    public static ModePage_1C_SFF? DecodeModePage_1C_SFF(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x1C)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

        var decoded = new ModePage_1C_SFF();

        decoded.PS   |= (pageResponse[0] & 0x80) == 0x80;
        decoded.DISP |= (pageResponse[2] & 0x02) == 0x02;
        decoded.SWPP |= (pageResponse[3] & 0x01) == 0x01;

        decoded.InactivityTimeMultiplier = (byte)(pageResponse[3] & 0x0F);

        return decoded;
    }

    public static string PrettifyModePage_1C_SFF(byte[] pageResponse) =>
        PrettifyModePage_1C_SFF(DecodeModePage_1C_SFF(pageResponse));

    public static string PrettifyModePage_1C_SFF(ModePage_1C_SFF? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_1C_SFF page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine("SCSI Timer & Protect page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.DISP)
            sb.AppendLine("\tDrive is disabled until power is cycled");

        if(page.SWPP)
            sb.AppendLine("\tDrive is software write-protected until powered down");

        switch(page.InactivityTimeMultiplier)
        {
            case 0:
                sb.AppendLine("\tDrive will remain in same status a vendor-specified time after a seek, read or write operation");

                break;
            case 1:
                sb.AppendLine("\tDrive will remain in same status 125 ms after a seek, read or write operation");

                break;
            case 2:
                sb.AppendLine("\tDrive will remain in same status 250 ms after a seek, read or write operation");

                break;
            case 3:
                sb.AppendLine("\tDrive will remain in same status 500 ms after a seek, read or write operation");

                break;
            case 4:
                sb.AppendLine("\tDrive will remain in same status 1 second after a seek, read or write operation");

                break;
            case 5:
                sb.AppendLine("\tDrive will remain in same status 2 seconds after a seek, read or write operation");

                break;
            case 6:
                sb.AppendLine("\tDrive will remain in same status 4 seconds after a seek, read or write operation");

                break;
            case 7:
                sb.AppendLine("\tDrive will remain in same status 8 seconds after a seek, read or write operation");

                break;
            case 8:
                sb.AppendLine("\tDrive will remain in same status 16 seconds after a seek, read or write operation");

                break;
            case 9:
                sb.AppendLine("\tDrive will remain in same status 32 seconds after a seek, read or write operation");

                break;
            case 10:
                sb.AppendLine("\tDrive will remain in same status 1 minute after a seek, read or write operation");

                break;
            case 11:
                sb.AppendLine("\tDrive will remain in same status 2 minutes after a seek, read or write operation");

                break;
            case 12:
                sb.AppendLine("\tDrive will remain in same status 4 minutes after a seek, read or write operation");

                break;
            case 13:
                sb.AppendLine("\tDrive will remain in same status 8 minutes after a seek, read or write operation");

                break;
            case 14:
                sb.AppendLine("\tDrive will remain in same status 16 minutes after a seek, read or write operation");

                break;
            case 15:
                sb.AppendLine("\tDrive will remain in same status 32 minutes after a seek, read or write operation");

                break;
        }

        return sb.ToString();
    }
    #endregion Mode Page 0x1C: Timer & Protect page
}