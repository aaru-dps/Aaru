// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 3C_HP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes HP MODE PAGE 3Ch: Device Time Mode page.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    #region HP Mode Page 0x3C: Device Time Mode page
    public struct HP_ModePage_3C
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public bool   LT;
        public bool   WT;
        public bool   PT;
        public ushort CurrentPowerOn;
        public uint   PowerOnTime;
        public bool   UTC;
        public bool   NTP;
        public uint   WorldTime;
        public byte   LibraryHours;
        public byte   LibraryMinutes;
        public byte   LibrarySeconds;
        public uint   CumulativePowerOn;
    }

    public static HP_ModePage_3C? DecodeHPModePage_3C(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x3C)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length != 36)
            return null;

        var decoded = new HP_ModePage_3C();

        decoded.PS             |= (pageResponse[0] & 0x80) == 0x80;
        decoded.LT             |= (pageResponse[2] & 0x04) == 0x04;
        decoded.WT             |= (pageResponse[2] & 0x02) == 0x02;
        decoded.PT             |= (pageResponse[2] & 0x01) == 0x01;
        decoded.CurrentPowerOn =  (ushort)((pageResponse[6] << 8) + pageResponse[7]);

        decoded.PowerOnTime = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) +
                                     pageResponse[11]);

        decoded.UTC |= (pageResponse[14] & 0x02) == 0x02;
        decoded.NTP |= (pageResponse[14] & 0x01) == 0x01;

        decoded.WorldTime = (uint)((pageResponse[16] << 24) + (pageResponse[17] << 16) + (pageResponse[18] << 8) +
                                   pageResponse[19]);

        decoded.LibraryHours   = pageResponse[23];
        decoded.LibraryMinutes = pageResponse[24];
        decoded.LibrarySeconds = pageResponse[25];

        decoded.CumulativePowerOn = (uint)((pageResponse[32] << 24) + (pageResponse[33] << 16) +
                                           (pageResponse[34] << 8)  + pageResponse[35]);

        return decoded;
    }

    public static string PrettifyHPModePage_3C(byte[] pageResponse) =>
        PrettifyHPModePage_3C(DecodeHPModePage_3C(pageResponse));

    public static string PrettifyHPModePage_3C(HP_ModePage_3C? modePage)
    {
        if(!modePage.HasValue)
            return null;

        HP_ModePage_3C page = modePage.Value;
        var            sb   = new StringBuilder();

        sb.AppendLine("HP Device Time Mode Page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.PT)
        {
            sb.AppendFormat("\tDrive has been powered up {0} times", page.CurrentPowerOn);

            sb.AppendFormat("\tDrive has been powered up since {0} this time",
                            TimeSpan.FromSeconds(page.PowerOnTime)).AppendLine();

            sb.AppendFormat("\tDrive has been powered up a total of {0}",
                            TimeSpan.FromSeconds(page.CumulativePowerOn)).AppendLine();
        }

        if(page.WT)
        {
            sb.AppendFormat("\tDrive's date/time is: {0}", DateHandlers.UnixUnsignedToDateTime(page.WorldTime)).
               AppendLine();

            if(page.UTC)
                sb.AppendLine("\tDrive's time is UTC");

            if(page.NTP)
                sb.AppendLine("\tDrive's time is synchronized with a NTP source");
        }

        if(page.LT)
            sb.AppendFormat("\tLibrary time is {0}",
                            new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, page.LibraryHours,
                                         page.LibraryMinutes, page.LibrarySeconds)).AppendLine();

        return sb.ToString();
    }
    #endregion HP Mode Page 0x3C: Device Time Mode page
}