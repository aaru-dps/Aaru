// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ScsiCommands.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
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

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer)
        {
            return ScsiInquiry(out buffer, out senseBuffer, Timeout);
        }

        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, out double duration)
        {
            return ScsiInquiry(out buffer, out senseBuffer, Timeout, out duration);
        }

        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, uint timeout)
        {
            double duration;
            return ScsiInquiry(out buffer, out senseBuffer, timeout, out duration);
        }

        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer = new byte[5];
            senseBuffer = new byte[32];
            byte[] cdb = { (byte)Enums.ScsiCommands.Inquiry, 0, 0, 0, 5, 0 };
            bool sense;

            SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, Enums.ScsiDirection.In, out duration, out sense);

            if (sense)
                return true;

            byte pagesLength = (byte)(buffer[4] + 5);

            cdb = new byte[] { (byte)Enums.ScsiCommands.Inquiry, 0, 0, 0, pagesLength, 0 };
            buffer = new byte[pagesLength];
            senseBuffer = new byte[32];

            SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, Enums.ScsiDirection.In, out duration, out sense);

            return sense;
        }

        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page)
        {
            return ScsiInquiry(out buffer, out senseBuffer, page, Timeout);
        }

        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page, out double duration)
        {
            return ScsiInquiry(out buffer, out senseBuffer, page, Timeout, out duration);
        }

        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page, uint timeout)
        {
            double duration;
            return ScsiInquiry(out buffer, out senseBuffer, page, timeout, out duration);
        }

        public bool ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, byte page, uint timeout, out double duration)
        {
            buffer = new byte[5];
            senseBuffer = new byte[32];
            byte[] cdb = { (byte)Enums.ScsiCommands.Inquiry, 1, page, 0, 5, 0 };
            bool sense;

            SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, Enums.ScsiDirection.In, out duration, out sense);

            if (sense)
                return true;

            byte pagesLength = (byte)(buffer[4] + 5);

            cdb = new byte[] { (byte)Enums.ScsiCommands.Inquiry, 1, page, 0, pagesLength, 0 };
            buffer = new byte[pagesLength];
            senseBuffer = new byte[32];

            SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, Enums.ScsiDirection.In, out duration, out sense);

            return sense;
        }
    }
}

