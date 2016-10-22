// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SecureDigital.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Console;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        public bool ReadSDStatus(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[64];
            bool sense = false;

            lastError = SendMmcCommand((MmcCommands)SecureDigitalCommands.SendStatus, false, true, MmcFlags.ResponseSPI_R1 | MmcFlags.Response_R1 | MmcFlags.CommandADTC,
                                       0, 64, 1, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SecureDigital Device", "SD_STATUS took {0} ms.", duration);

            return sense;
        }

        public bool ReadSDOCR(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[4];
            bool sense = false;

            lastError = SendMmcCommand((MmcCommands)SecureDigitalCommands.SendOperatingCondition, false, true, MmcFlags.ResponseSPI_R3 | MmcFlags.Response_R3 | MmcFlags.CommandBCR,
                                       0, 4, 1, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SecureDigital Device", "SD_SEND_OP_COND took {0} ms.", duration);

            return sense;
        }
    
        public bool ReadSCR(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[8];
            bool sense = false;

            lastError = SendMmcCommand((MmcCommands)SecureDigitalCommands.SendSCR, false, true, MmcFlags.ResponseSPI_R1 | MmcFlags.Response_R1 | MmcFlags.CommandADTC,
                                       0, 8, 1, ref buffer, out response, out duration, out sense, timeout);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SecureDigital Device", "SEND_SCR took {0} ms.", duration);

            return sense;
        }
    }
}
