// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NEC.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : NEC vendor commands
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using DiscImageChef.Console;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        /// <summary>
        /// Sends the NEC READ CD-DA command
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer"/> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the NEC READ CD-DA response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        public bool NecReadCdDa(out byte[] buffer, out byte[] senseBuffer, uint lba, uint transferLength, uint timeout, out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];
            bool sense;

            cdb[0] = (byte)ScsiCommands.NEC_ReadCdDa;
            cdb[2] = (byte)((lba & 0xFF000000) >> 24);
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00) >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);

            buffer = new byte[2352 * transferLength];

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "READ CD-DA took {0} ms.", duration);

            return sense;
        }
    }
}

