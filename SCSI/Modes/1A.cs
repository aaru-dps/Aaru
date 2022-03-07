// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 1A.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 1Ah: Power condition page.
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

namespace Aaru.Decoders.SCSI;

using System.Diagnostics.CodeAnalysis;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    #region Mode Page 0x1A: Power condition page
    /// <summary>Power condition page Page code 0x1A 12 bytes in SPC-1, SPC-2, SPC-3, SPC-4 40 bytes in SPC-5</summary>
    public struct ModePage_1A
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Idle timer activated</summary>
        public bool Idle;
        /// <summary>Standby timer activated</summary>
        public bool Standby;
        /// <summary>Idle timer</summary>
        public uint IdleTimer;
        /// <summary>Standby timer</summary>
        public uint StandbyTimer;

        /// <summary>Interactions between background functions and power management</summary>
        public byte PM_BG_Precedence;
        /// <summary>Standby timer Y activated</summary>
        public bool Standby_Y;
        /// <summary>Idle timer B activated</summary>
        public bool Idle_B;
        /// <summary>Idle timer C activated</summary>
        public bool Idle_C;
        /// <summary>Idle timer B</summary>
        public uint IdleTimer_B;
        /// <summary>Idle timer C</summary>
        public uint IdleTimer_C;
        /// <summary>Standby timer Y</summary>
        public uint StandbyTimer_Y;
        public byte CCF_Idle;
        public byte CCF_Standby;
        public byte CCF_Stopped;
    }

    public static ModePage_1A? DecodeModePage_1A(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x1A)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 12)
            return null;

        var decoded = new ModePage_1A();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.Standby |= (pageResponse[3] & 0x01) == 0x01;
        decoded.Idle    |= (pageResponse[3] & 0x02) == 0x02;

        decoded.IdleTimer = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) +
                                   pageResponse[7]);

        decoded.StandbyTimer = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) +
                                      pageResponse[11]);

        if(pageResponse.Length < 40)
            return decoded;

        decoded.PM_BG_Precedence =  (byte)((pageResponse[2] & 0xC0) >> 6);
        decoded.Standby_Y        |= (pageResponse[2] & 0x01) == 0x01;
        decoded.Idle_B           |= (pageResponse[3] & 0x04) == 0x04;
        decoded.Idle_C           |= (pageResponse[3] & 0x08) == 0x08;

        decoded.IdleTimer_B = (uint)((pageResponse[12] << 24) + (pageResponse[13] << 16) + (pageResponse[14] << 8) +
                                     pageResponse[15]);

        decoded.IdleTimer_C = (uint)((pageResponse[16] << 24) + (pageResponse[17] << 16) + (pageResponse[18] << 8) +
                                     pageResponse[19]);

        decoded.StandbyTimer_Y = (uint)((pageResponse[20] << 24) + (pageResponse[21] << 16) + (pageResponse[22] << 8) +
                                        pageResponse[23]);

        decoded.CCF_Idle    = (byte)((pageResponse[39] & 0xC0) >> 6);
        decoded.CCF_Standby = (byte)((pageResponse[39] & 0x30) >> 4);
        decoded.CCF_Stopped = (byte)((pageResponse[39] & 0x0C) >> 2);

        return decoded;
    }

    public static string PrettifyModePage_1A(byte[] pageResponse) =>
        PrettifyModePage_1A(DecodeModePage_1A(pageResponse));

    public static string PrettifyModePage_1A(ModePage_1A? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_1A page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine("SCSI Power condition page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.Standby   && page.StandbyTimer   > 0 ||
           page.Standby_Y && page.StandbyTimer_Y > 0)
        {
            if(page.Standby &&
               page.StandbyTimer > 0)
                sb.AppendFormat("\tStandby timer Z is set to {0} ms", page.StandbyTimer * 100).AppendLine();

            if(page.Standby_Y &&
               page.StandbyTimer_Y > 0)
                sb.AppendFormat("\tStandby timer Y is set to {0} ms", page.StandbyTimer_Y * 100).AppendLine();
        }
        else
            sb.AppendLine("\tDrive will not enter standy mode");

        if(page.Idle   && page.IdleTimer   > 0 ||
           page.Idle_B && page.IdleTimer_B > 0 ||
           page.Idle_C && page.IdleTimer_C > 0)
        {
            if(page.Idle &&
               page.IdleTimer > 0)
                sb.AppendFormat("\tIdle timer A is set to {0} ms", page.IdleTimer * 100).AppendLine();

            if(page.Idle_B &&
               page.IdleTimer_B > 0)
                sb.AppendFormat("\tIdle timer B is set to {0} ms", page.IdleTimer_B * 100).AppendLine();

            if(page.Idle_C &&
               page.IdleTimer_C > 0)
                sb.AppendFormat("\tIdle timer C is set to {0} ms", page.IdleTimer_C * 100).AppendLine();
        }
        else
            sb.AppendLine("\tDrive will not enter idle mode");

        switch(page.PM_BG_Precedence)
        {
            case 0: break;
            case 1:
                sb.AppendLine("\tPerforming background functions take precedence over maintaining low power conditions");

                break;
            case 2:
                sb.AppendLine("\tMaintaining low power conditions take precedence over performing background functions");

                break;
        }

        return sb.ToString();
    }
    #endregion Mode Page 0x1A: Power condition page

    #region Mode Page 0x1A subpage 0x01: Power Consumption mode page
    /// <summary>Power Consumption mode page Page code 0x1A Subpage code 0x01 16 bytes in SPC-5</summary>
    public struct ModePage_1A_S01
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Active power level</summary>
        public byte ActiveLevel;
        /// <summary>Power Consumption VPD identifier in use</summary>
        public byte PowerConsumptionIdentifier;
    }

    public static ModePage_1A_S01? DecodeModePage_1A_S01(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) != 0x40)
            return null;

        if((pageResponse[0] & 0x3F) != 0x1A)
            return null;

        if(pageResponse[1] != 0x01)
            return null;

        if((pageResponse[2] << 8) + pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 16)
            return null;

        var decoded = new ModePage_1A_S01();

        decoded.PS                         |= (pageResponse[0]       & 0x80) == 0x80;
        decoded.ActiveLevel                =  (byte)(pageResponse[6] & 0x03);
        decoded.PowerConsumptionIdentifier =  pageResponse[7];

        return decoded;
    }

    public static string PrettifyModePage_1A_S01(byte[] pageResponse) =>
        PrettifyModePage_1A_S01(DecodeModePage_1A_S01(pageResponse));

    public static string PrettifyModePage_1A_S01(ModePage_1A_S01? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_1A_S01 page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine("SCSI Power Consumption page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        switch(page.ActiveLevel)
        {
            case 0:
                sb.AppendFormat("\tDevice power consumption is dictated by identifier {0} of Power Consumption VPD",
                                page.PowerConsumptionIdentifier).AppendLine();

                break;
            case 1:
                sb.AppendLine("\tDevice is in highest relative power consumption level");

                break;
            case 2:
                sb.AppendLine("\tDevice is in intermediate relative power consumption level");

                break;
            case 3:
                sb.AppendLine("\tDevice is in lowest relative power consumption level");

                break;
        }

        return sb.ToString();
    }
    #endregion Mode Page 0x1A subpage 0x01: Power Consumption mode page
}