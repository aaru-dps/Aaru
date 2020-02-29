// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;
using Aaru.Decoders.ATA;

namespace Aaru.Devices
{
    public partial class Device
    {
        public bool SmartDisable(out AtaErrorRegistersLba28 statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.Smart, Feature = (byte)AtaSmartSubCommands.Disable, LbaHigh = 0xC2,
                LbaMid  = 0x4F
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SMART DISABLE OPERATIONS took {0} ms.", duration);

            return sense;
        }

        public bool SmartEnableAttributeAutosave(out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                                                 out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.Smart, Feature = (byte)AtaSmartSubCommands.EnableDisableAttributeAutosave,
                LbaHigh = 0xC2, LbaMid                     = 0x4F, SectorCount = 0xF1
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SMART ENABLE ATTRIBUTE AUTOSAVE took {0} ms.", duration);

            return sense;
        }

        public bool SmartDisableAttributeAutosave(out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                                                  out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.Smart, Feature = (byte)AtaSmartSubCommands.EnableDisableAttributeAutosave,
                LbaHigh = 0xC2, LbaMid                     = 0x4F
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SMART DISABLE ATTRIBUTE AUTOSAVE took {0} ms.", duration);

            return sense;
        }

        public bool SmartEnable(out AtaErrorRegistersLba28 statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.Smart, Feature = (byte)AtaSmartSubCommands.Enable, LbaHigh = 0xC2,
                LbaMid  = 0x4F
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SMART ENABLE OPERATIONS took {0} ms.", duration);

            return sense;
        }

        public bool SmartExecuteOffLineImmediate(out AtaErrorRegistersLba28 statusRegisters, byte subcommand,
                                                 uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.Smart, Feature = (byte)AtaSmartSubCommands.ExecuteOfflineImmediate,
                LbaHigh = 0xC2, LbaMid                     = 0x4F, LbaLow = subcommand
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SMART EXECUTE OFF-LINE IMMEDIATE took {0} ms.", duration);

            return sense;
        }

        public bool SmartReadData(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                                  out double duration)
        {
            buffer = new byte[512];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.Smart, Feature = (byte)AtaSmartSubCommands.ReadData, LbaHigh = 0xC2,
                LbaMid  = 0x4F
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SMART READ DATA took {0} ms.", duration);

            return sense;
        }

        public bool SmartReadLog(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, byte logAddress,
                                 uint timeout, out double duration)
        {
            buffer = new byte[512];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.Smart, Feature = (byte)AtaSmartSubCommands.ReadLog, LbaHigh = 0xC2,
                LbaMid  = 0x4F, LbaLow                     = logAddress
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SMART READ LOG took {0} ms.", duration);

            return sense;
        }

        public bool SmartReturnStatus(out AtaErrorRegistersLba28 statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.Smart, Feature = (byte)AtaSmartSubCommands.ReturnStatus, LbaHigh = 0xC2,
                LbaMid  = 0x4F
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SMART RETURN STATUS took {0} ms.", duration);

            return sense;
        }
    }
}