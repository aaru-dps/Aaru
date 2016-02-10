// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MCPT.cs
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
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        public bool EnableMediaCardPassThrough(out AtaErrorRegistersCHS statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersCHS registers = new AtaRegistersCHS();
            bool sense;

            registers.command = (byte)AtaCommands.CheckMediaCardType;
            registers.feature = 1;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "CHECK MEDIA CARD TYPE took {0} ms.", duration);

            return sense;
        }

        public bool DisableMediaCardPassThrough(out AtaErrorRegistersCHS statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersCHS registers = new AtaRegistersCHS();
            bool sense;

            registers.command = (byte)AtaCommands.CheckMediaCardType;
            registers.feature = 0;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "CHECK MEDIA CARD TYPE took {0} ms.", duration);

            return sense;
        }
    }
}

