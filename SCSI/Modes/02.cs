// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 02.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 02h: Disconnect-reconnect page.
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
    #region Mode Page 0x02: Disconnect-reconnect page
    /// <summary>Disconnect-reconnect page Page code 0x02 16 bytes in SCSI-2, SPC-1, SPC-2, SPC-3, SPC-4, SPC-5</summary>
    public struct ModePage_02
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>How full should be the buffer prior to attempting a reselection</summary>
        public byte BufferFullRatio;
        /// <summary>How empty should be the buffer prior to attempting a reselection</summary>
        public byte BufferEmptyRatio;
        /// <summary>Max. time in 100 µs increments that the target is permitted to assert BSY without a REQ/ACK</summary>
        public ushort BusInactivityLimit;
        /// <summary>Min. time in 100 µs increments to wait after releasing the bus before attempting reselection</summary>
        public ushort DisconnectTimeLimit;
        /// <summary>
        ///     Max. time in 100 µs increments allowed to use the bus before disconnecting, if granted the privilege and not
        ///     restricted by <see cref="DTDC" />
        /// </summary>
        public ushort ConnectTimeLimit;
        /// <summary>Maximum amount of data before disconnecting in 512 bytes increments</summary>
        public ushort MaxBurstSize;
        /// <summary>Data transfer disconnect control</summary>
        public byte DTDC;

        /// <summary>Target shall not transfer data for a command during the same interconnect tenancy</summary>
        public bool DIMM;
        /// <summary>Wether to use fair or unfair arbitration when requesting an interconnect tenancy</summary>
        public byte FairArbitration;
        /// <summary>Max. ammount of data in 512 bytes increments that may be transferred for a command along with the command</summary>
        public ushort FirstBurstSize;
        /// <summary>Target is allowed to re-order the data transfer</summary>
        public bool EMDP;
    }

    public static ModePage_02? DecodeModePage_02(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x02)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 12)
            return null;

        var decoded = new ModePage_02();

        decoded.PS                  |= (pageResponse[0] & 0x80) == 0x80;
        decoded.BufferFullRatio     =  pageResponse[2];
        decoded.BufferEmptyRatio    =  pageResponse[3];
        decoded.BusInactivityLimit  =  (ushort)((pageResponse[4]  << 8) + pageResponse[5]);
        decoded.DisconnectTimeLimit =  (ushort)((pageResponse[6]  << 8) + pageResponse[7]);
        decoded.ConnectTimeLimit    =  (ushort)((pageResponse[8]  << 8) + pageResponse[9]);
        decoded.MaxBurstSize        =  (ushort)((pageResponse[10] << 8) + pageResponse[11]);

        if(pageResponse.Length >= 13)
        {
            decoded.EMDP            |= (pageResponse[12] & 0x80) == 0x80;
            decoded.DIMM            |= (pageResponse[12] & 0x08) == 0x08;
            decoded.FairArbitration =  (byte)((pageResponse[12] & 0x70) >> 4);
            decoded.DTDC            =  (byte)(pageResponse[12] & 0x07);
        }

        if(pageResponse.Length >= 16)
            decoded.FirstBurstSize = (ushort)((pageResponse[14] << 8) + pageResponse[15]);

        return decoded;
    }

    public static string PrettifyModePage_02(byte[] pageResponse) =>
        PrettifyModePage_02(DecodeModePage_02(pageResponse));

    public static string PrettifyModePage_02(ModePage_02? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_02 page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine("SCSI Disconnect-Reconnect mode page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.BufferFullRatio > 0)
            sb.AppendFormat("\t{0} ratio of buffer that shall be full prior to attempting a reselection",
                            page.BufferFullRatio).AppendLine();

        if(page.BufferEmptyRatio > 0)
            sb.AppendFormat("\t{0} ratio of buffer that shall be empty prior to attempting a reselection",
                            page.BufferEmptyRatio).AppendLine();

        if(page.BusInactivityLimit > 0)
            sb.AppendFormat("\t{0} µs maximum permitted to assert BSY without a REQ/ACK handshake",
                            page.BusInactivityLimit * 100).AppendLine();

        if(page.DisconnectTimeLimit > 0)
            sb.AppendFormat("\t{0} µs maximum permitted wait after releasing the bus before attempting reselection",
                            page.DisconnectTimeLimit * 100).AppendLine();

        if(page.ConnectTimeLimit > 0)
            sb.
                AppendFormat("\t{0} µs allowed to use the bus before disconnecting, if granted the privilege and not restricted",
                             page.ConnectTimeLimit * 100).AppendLine();

        if(page.MaxBurstSize > 0)
            sb.AppendFormat("\t{0} bytes maximum can be transferred before disconnecting", page.MaxBurstSize * 512).
               AppendLine();

        if(page.FirstBurstSize > 0)
            sb.
                AppendFormat("\t{0} bytes maximum can be transferred for a command along with the disconnect command",
                             page.FirstBurstSize * 512).AppendLine();

        if(page.DIMM)
            sb.AppendLine("\tTarget shall not transfer data for a command during the same interconnect tenancy");

        if(page.EMDP)
            sb.AppendLine("\tTarget is allowed to re-order the data transfer");

        switch(page.DTDC)
        {
            case 0:
                sb.AppendLine("\tData transfer disconnect control is not used");

                break;
            case 1:
                sb.AppendLine("\tAll data for a command shall be transferred within a single interconnect tenancy");

                break;
            case 3:
                sb.AppendLine("\tAll data and the response for a command shall be transferred within a single interconnect tenancy");

                break;
            default:
                sb.AppendFormat("\tReserved data transfer disconnect control value {0}", page.DTDC).AppendLine();

                break;
        }

        return sb.ToString();
    }
    #endregion Mode Page 0x02: Disconnect-reconnect page
}