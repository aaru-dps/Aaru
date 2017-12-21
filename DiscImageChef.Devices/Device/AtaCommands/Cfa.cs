// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        public bool TranslateSector(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, uint lba,
                                    uint timeout, out double duration)
        {
            buffer = new byte[512];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.TranslateSector;
            registers.deviceHead = (byte)((lba & 0xF000000) / 0x1000000);
            registers.lbaHigh = (byte)((lba & 0xFF0000) / 0x10000);
            registers.lbaMid = (byte)((lba & 0xFF00) / 0x100);
            registers.lbaLow = (byte)((lba & 0xFF) / 0x1);
            registers.deviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "CFA TRANSLATE SECTOR took {0} ms.", duration);

            return sense;
        }

        public bool TranslateSector(out byte[] buffer, out AtaErrorRegistersCHS statusRegisters, ushort cylinder,
                                    byte head, byte sector, uint timeout, out double duration)
        {
            buffer = new byte[512];
            AtaRegistersCHS registers = new AtaRegistersCHS();
            bool sense;

            registers.command = (byte)AtaCommands.TranslateSector;
            registers.cylinderHigh = (byte)((cylinder & 0xFF00) / 0x100);
            registers.cylinderLow = (byte)((cylinder & 0xFF) / 0x1);
            registers.sector = sector;
            registers.deviceHead = (byte)(head & 0x0F);

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "CFA TRANSLATE SECTOR took {0} ms.", duration);

            return sense;
        }

        public bool RequestExtendedErrorCode(out byte errorCode, out AtaErrorRegistersLBA28 statusRegisters,
                                             uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.RequestSense;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out sense);
            Error = LastError != 0;

            errorCode = statusRegisters.error;

            DicConsole.DebugWriteLine("ATA Device", "CFA REQUEST EXTENDED ERROR CODE took {0} ms.", duration);

            return sense;
        }
    }
}