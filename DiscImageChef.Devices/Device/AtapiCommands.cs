// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AtapiCommands.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Direct device access
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Contains ATAPI commands
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
        public bool AtapiIdentify(out byte[] buffer, out Structs.AtaErrorRegistersCHS statusRegisters)
        {
            return AtapiIdentify(out buffer, out statusRegisters, Timeout);
        }

        public bool AtapiIdentify(out byte[] buffer, out Structs.AtaErrorRegistersCHS statusRegisters, out double duration)
        {
            return AtapiIdentify(out buffer, out statusRegisters, Timeout, out duration);
        }

        public bool AtapiIdentify(out byte[] buffer, out Structs.AtaErrorRegistersCHS statusRegisters, uint timeout)
        {
            double duration;
            return AtapiIdentify(out buffer, out statusRegisters, timeout, out duration);
        }

        public bool AtapiIdentify(out byte[] buffer, out Structs.AtaErrorRegistersCHS statusRegisters, uint timeout, out double duration)
        {
            buffer = new byte[512];
            Structs.AtaRegistersCHS registers = new Structs.AtaRegistersCHS();
            bool sense;

            registers.command = (byte)Enums.AtaCommands.IdentifyPacketDevice;

            lastError = SendAtaCommand(registers, out statusRegisters, Enums.AtaProtocol.PioOut, Enums.AtaTransferRegister.NoTransfer,
                ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            return sense;
        }
    }
}

