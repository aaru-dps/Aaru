// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Certance.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Certance vendor commands
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
        /// Parks the load arm in preparation for transport
        /// </summary>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool CertancePark(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return CertanceParkUnpark(out senseBuffer, true, timeout, out duration);
        }

        /// <summary>
        /// Unparks the load arm prior to operation
        /// </summary>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool CertanceUnpark(out byte[] senseBuffer, uint timeout, out double duration)
        {
            return CertanceParkUnpark(out senseBuffer, false, timeout, out duration);
        }

        /// <summary>
        /// Parks the load arm in preparation for transport or unparks it prior to operation
        /// </summary>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="park">If set to <c>true</c>, parks the load arm</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool CertanceParkUnpark(out byte[] senseBuffer, bool park, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            byte[] cdb = new byte[6];
            senseBuffer = new byte[32];
            bool sense;

            cdb[0] = (byte)ScsiCommands.Certance_ParkUnpark;
            if(park)
                cdb[4] = 1;

            lastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("SCSI Device", "CERTANCE PARK UNPARK took {0} ms.", duration);

            return sense;
        }
    }
}

