// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NEC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NEC vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for NEC SCSI devices.
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

using Aaru.Console;

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        /// <summary>Sends the NEC READ CD-DA command</summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the NEC READ CD-DA response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        public bool NecReadCdDa(out byte[] buffer, out byte[] senseBuffer, uint lba, uint transferLength, uint timeout,
                                out double duration)
        {
            senseBuffer = new byte[64];
            byte[] cdb = new byte[10];

            cdb[0] = (byte)ScsiCommands.NecReadCdDa;
            cdb[2] = (byte)((lba & 0xFF000000) >> 24);
            cdb[3] = (byte)((lba & 0xFF0000)   >> 16);
            cdb[4] = (byte)((lba & 0xFF00)     >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);

            buffer = new byte[2352 * transferLength];

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "READ CD-DA took {0} ms.", duration);

            return sense;
        }
    }
}