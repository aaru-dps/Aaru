// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Fujitsu.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Fujitsu vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for Fujitsu SCSI devices.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        public bool FujitsuDisplay(out byte[] senseBuffer, bool flash, FujitsuDisplayModes mode, string firstHalf,
                                   string secondHalf, uint timeout, out double duration)
        {
            byte[] tmp;
            byte[] firstHalfBytes = new byte[8];
            byte[] secondHalfBytes = new byte[8];
            byte[] buffer = new byte[17];
            bool displayLen = false;
            bool halfMsg = false;
            byte[] cdb = new byte[10];
            bool sense;

            if(!string.IsNullOrWhiteSpace(firstHalf))
            {
                tmp = Encoding.ASCII.GetBytes(firstHalf);
                Array.Copy(tmp, 0, firstHalfBytes, 0, 8);
            }
            if(!string.IsNullOrWhiteSpace(secondHalf))
            {
                tmp = Encoding.ASCII.GetBytes(secondHalf);
                Array.Copy(tmp, 0, secondHalfBytes, 0, 8);
            }

            if(mode != FujitsuDisplayModes.Half)
                if(!ArrayHelpers.ArrayIsNullOrWhiteSpace(firstHalfBytes) &&
                   !ArrayHelpers.ArrayIsNullOrWhiteSpace(secondHalfBytes))
                {
                    displayLen = true;
                    halfMsg = false;
                }
                else if(ArrayHelpers.ArrayIsNullOrWhiteSpace(firstHalfBytes) &&
                        !ArrayHelpers.ArrayIsNullOrWhiteSpace(secondHalfBytes))
                {
                    displayLen = false;
                    halfMsg = false;
                }
                else if(!ArrayHelpers.ArrayIsNullOrWhiteSpace(firstHalfBytes) &&
                        ArrayHelpers.ArrayIsNullOrWhiteSpace(secondHalfBytes))
                {
                    displayLen = false;
                    halfMsg = true;
                }
                else
                {
                    displayLen = false;
                    halfMsg = false;
                }

            buffer[0] = (byte)((byte)mode << 5);
            if(displayLen) buffer[0] += 0x10;
            if(flash) buffer[0] += 0x08;
            if(halfMsg) buffer[0] += 0x04;
            buffer[0] += 0x01; // Always ASCII

            Array.Copy(firstHalfBytes, 0, buffer, 1, 8);
            Array.Copy(secondHalfBytes, 0, buffer, 9, 8);

            cdb[0] = (byte)ScsiCommands.FujitsuDisplay;
            cdb[6] = (byte)buffer.Length;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.Out, out duration,
                                        out sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "FUJITSU DISPLAY took {0} ms.", duration);

            return sense;
        }
    }
}