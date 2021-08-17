// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SecureDigital.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SecureDigital and MultiMediaCard commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SecureDigital commands.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        /// <summary>Reads the status register from a SecureDigital device</summary>
        /// <param name="buffer">Data buffer</param>
        /// <param name="response">Response</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadSdStatus(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[64];

            LastError = SendMmcCommand((MmcCommands)SecureDigitalCommands.SendStatus, false, true,
                                       MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc, 0, 64, 1,
                                       ref buffer, out response, out duration, out bool sense, timeout);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SecureDigital Device", "SD_STATUS took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads the OCR register from a SecureDigital device</summary>
        /// <param name="buffer">Data buffer</param>
        /// <param name="response">Response</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadSdocr(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[4];

            LastError = SendMmcCommand((MmcCommands)SecureDigitalCommands.SendOperatingCondition, false, true,
                                       MmcFlags.ResponseSpiR3 | MmcFlags.ResponseR3 | MmcFlags.CommandBcr, 0, 4, 1,
                                       ref buffer, out response, out duration, out bool sense, timeout);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SecureDigital Device", "SD_SEND_OP_COND took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads the SCR register from a SecureDigital device</summary>
        /// <param name="buffer">Data buffer</param>
        /// <param name="response">Response</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadScr(out byte[] buffer, out uint[] response, uint timeout, out double duration)
        {
            buffer = new byte[8];

            LastError = SendMmcCommand((MmcCommands)SecureDigitalCommands.SendScr, false, true,
                                       MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc, 0, 8, 1,
                                       ref buffer, out response, out duration, out bool sense, timeout);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SecureDigital Device", "SEND_SCR took {0} ms.", duration);

            return sense;
        }
    }
}