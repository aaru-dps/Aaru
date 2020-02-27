// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Cfa.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ATA commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains Compact Flash Association commands.
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
        public bool TranslateSector(out byte[] buffer,  out AtaErrorRegistersLba28 statusRegisters, uint lba,
                                    uint       timeout, out double                 duration)
        {
            buffer = new byte[512];
            AtaRegistersLba28 registers = new AtaRegistersLba28
            {
                Command    = (byte)AtaCommands.TranslateSector,
                DeviceHead = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh    = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid     = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow     = (byte)((lba & 0xFF)      / 0x1)
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers,                      out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer,          timeout, false,
                                       out duration,
                                       out bool sense);
            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "CFA TRANSLATE SECTOR took {0} ms.", duration);

            return sense;
        }

        public bool TranslateSector(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, ushort cylinder,
                                    byte       head,   byte                     sector,          uint   timeout,
                                    out double duration)
        {
            buffer = new byte[512];
            AtaRegistersChs registers = new AtaRegistersChs
            {
                Command      = (byte)AtaCommands.TranslateSector,
                CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100),
                CylinderLow  = (byte)((cylinder & 0xFF)   / 0x1),
                Sector       = sector,
                DeviceHead   = (byte)(head & 0x0F)
            };

            LastError = SendAtaCommand(registers,                      out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer,          timeout, false,
                                       out duration,
                                       out bool sense);
            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "CFA TRANSLATE SECTOR took {0} ms.", duration);

            return sense;
        }

        public bool RequestExtendedErrorCode(out byte errorCode, out AtaErrorRegistersLba28 statusRegisters,
                                             uint     timeout,   out double                 duration)
        {
            byte[]            buffer    = new byte[0];
            AtaRegistersLba28 registers = new AtaRegistersLba28 {Command = (byte)AtaCommands.RequestSense};

            LastError = SendAtaCommand(registers,                      out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer,          timeout, false,
                                       out duration,
                                       out bool sense);
            Error = LastError != 0;

            errorCode = statusRegisters.Error;

            AaruConsole.DebugWriteLine("ATA Device", "CFA REQUEST EXTENDED ERROR CODE took {0} ms.", duration);

            return sense;
        }
    }
}