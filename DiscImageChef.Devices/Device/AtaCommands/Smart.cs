// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Smart.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ATA commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains S.M.A.R.T. commands.
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

// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Smart.cs
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
        public bool SmartDisable(out AtaErrorRegistersLBA28 statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.Smart;
            registers.feature = (byte)AtaSmartSubCommands.Disable;
            registers.lbaHigh = 0xC2;
            registers.lbaMid = 0x4F;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SMART DISABLE OPERATIONS took {0} ms.", duration);

            return sense;
        }

        public bool SmartEnableAttributeAutosave(out AtaErrorRegistersLBA28 statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.Smart;
            registers.feature = (byte)AtaSmartSubCommands.EnableDisableAttributeAutosave;
            registers.lbaHigh = 0xC2;
            registers.lbaMid = 0x4F;
            registers.sectorCount = 0xF1;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SMART ENABLE ATTRIBUTE AUTOSAVE took {0} ms.", duration);

            return sense;
        }

        public bool SmartDisableAttributeAutosave(out AtaErrorRegistersLBA28 statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.Smart;
            registers.feature = (byte)AtaSmartSubCommands.Enable;
            registers.lbaHigh = 0xC2;
            registers.lbaMid = 0x4F;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SMART ENABLE OPERATIONS took {0} ms.", duration);

            return sense;
        }

        public bool SmartExecuteOffLineImmediate(out AtaErrorRegistersLBA28 statusRegisters, byte subcommand, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.Smart;
            registers.feature = (byte)AtaSmartSubCommands.ExecuteOfflineImmediate;
            registers.lbaHigh = 0xC2;
            registers.lbaMid = 0x4F;
            registers.lbaLow = subcommand;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SMART EXECUTE OFF-LINE IMMEDIATE took {0} ms.", duration);

            return sense;
        }

        public bool SmartReadData(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, uint timeout, out double duration)
        {
            buffer = new byte[512];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.Smart;
            registers.feature = (byte)AtaSmartSubCommands.ReadData;
            registers.lbaHigh = 0xC2;
            registers.lbaMid = 0x4F;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SMART READ DATA took {0} ms.", duration);

            return sense;
        }

        public bool SmartReadLog(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, byte logAddress, uint timeout, out double duration)
        {
            buffer = new byte[512];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.Smart;
            registers.feature = (byte)AtaSmartSubCommands.ReadLog;
            registers.lbaHigh = 0xC2;
            registers.lbaMid = 0x4F;
            registers.lbaLow = logAddress;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SMART READ LOG took {0} ms.", duration);

            return sense;
        }

        public bool SmartReturnStatus(out AtaErrorRegistersLBA28 statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.Smart;
            registers.feature = (byte)AtaSmartSubCommands.ReturnStatus;
            registers.lbaHigh = 0xC2;
            registers.lbaMid = 0x4F;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SMART RETURN STATUS took {0} ms.", duration);

            return sense;
        }
    }
}