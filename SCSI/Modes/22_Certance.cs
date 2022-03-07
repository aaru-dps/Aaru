// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 22_Certance.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Certance MODE PAGE 22h: Interface Control Mode Page.
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
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static partial class Modes
{
    #region Certance Mode Page 0x22: Interface Control Mode Page
    public struct Certance_ModePage_22
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        public byte BaudRate;
        public byte CmdFwd;
        public bool StopBits;
        public byte Alerts;
        public byte PortATransportType;
        public byte PortAPresentSelectionID;
        public byte NextSelectionID;
        public byte JumperedSelectionID;
        public byte TargetInitiatedBusControl;
        public bool PortAEnabled;
        public bool PortAEnabledOnPower;
    }

    public static Certance_ModePage_22? DecodeCertanceModePage_22(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x22)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length != 16)
            return null;

        var decoded = new Certance_ModePage_22();

        decoded.PS                        |= (pageResponse[0] & 0x80) == 0x80;
        decoded.BaudRate                  =  pageResponse[2];
        decoded.CmdFwd                    =  (byte)((pageResponse[3] & 0x18) >> 3);
        decoded.StopBits                  |= (pageResponse[3]       & 0x04) == 0x04;
        decoded.CmdFwd                    =  (byte)(pageResponse[3] & 0x03);
        decoded.PortATransportType        =  pageResponse[4];
        decoded.PortAPresentSelectionID   =  pageResponse[7];
        decoded.NextSelectionID           =  pageResponse[12];
        decoded.JumperedSelectionID       =  pageResponse[13];
        decoded.TargetInitiatedBusControl =  pageResponse[14];
        decoded.PortAEnabled              |= (pageResponse[15] & 0x10) == 0x10;
        decoded.PortAEnabledOnPower       |= (pageResponse[15] & 0x04) == 0x04;

        return decoded;
    }

    public static string PrettifyCertanceModePage_22(byte[] pageResponse) =>
        PrettifyCertanceModePage_22(DecodeCertanceModePage_22(pageResponse));

    public static string PrettifyCertanceModePage_22(Certance_ModePage_22? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Certance_ModePage_22 page = modePage.Value;
        var                  sb   = new StringBuilder();

        sb.AppendLine("Certance Interface Control Mode Page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        switch(page.BaudRate)
        {
            case 0:
            case 1:
            case 2:
                sb.AppendLine("\tLibrary interface will operate at 9600 baud on next reset");

                break;
            case 3:
                sb.AppendLine("\tLibrary interface will operate at 19200 baud on next reset");

                break;
            case 4:
                sb.AppendLine("\tLibrary interface will operate at 38400 baud on next reset");

                break;
            case 5:
                sb.AppendLine("\tLibrary interface will operate at 57600 baud on next reset");

                break;
            case 6:
                sb.AppendLine("\tLibrary interface will operate at 115200 baud on next reset");

                break;
            default:
                sb.AppendFormat("\tUnknown library interface baud rate code {0}", page.BaudRate).AppendLine();

                break;
        }

        sb.AppendLine(page.StopBits ? "Library interface transmits 2 stop bits per byte"
                          : "Library interface transmits 1 stop bits per byte");

        switch(page.CmdFwd)
        {
            case 0:
                sb.AppendLine("\tCommand forwarding is disabled");

                break;
            case 1:
                sb.AppendLine("\tCommand forwarding is enabled");

                break;
            default:
                sb.AppendFormat("\tUnknown command forwarding code {0}", page.CmdFwd).AppendLine();

                break;
        }

        switch(page.PortATransportType)
        {
            case 0:
                sb.AppendLine("\tPort A link is down");

                break;
            case 3:
                sb.AppendLine("\tPort A uses Parallel SCSI Ultra-160 interface");

                break;
            default:
                sb.AppendFormat("\tUnknown port A transport type code {0}", page.PortATransportType).AppendLine();

                break;
        }

        if(page.PortATransportType > 0)
            sb.AppendFormat("\tDrive responds to SCSI ID {0}", page.PortAPresentSelectionID).AppendLine();

        sb.AppendFormat("\tDrive will respond to SCSI ID {0} on Port A enabling", page.NextSelectionID).AppendLine();

        sb.AppendFormat("\tDrive jumpers choose SCSI ID {0}", page.JumperedSelectionID).AppendLine();

        sb.AppendLine(page.PortAEnabled ? "\tSCSI port is enabled" : "\tSCSI port is disabled");

        sb.AppendLine(page.PortAEnabledOnPower ? "\tSCSI port will be enabled on next power up"
                          : "\tSCSI port will be disabled on next power up");

        return sb.ToString();
    }
    #endregion Certance Mode Page 0x22: Interface Control Mode Page
}