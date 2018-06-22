// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Ata28.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ATA commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains 28-bit LBA ATA commands.
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
        public bool ReadBuffer(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                               out double duration)
        {
            buffer = new byte[512];
            AtaRegistersLba28 registers = new AtaRegistersLba28 {Command = (byte)AtaCommands.ReadBuffer};

            LastError = SendAtaCommand(registers,                      out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer,          timeout, false,
                                       out duration,
                                       out bool sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ BUFFER took {0} ms.", duration);

            return sense;
        }

        public bool ReadBufferDma(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                                  out double duration)
        {
            buffer = new byte[512];
            AtaRegistersLba28 registers = new AtaRegistersLba28 {Command = (byte)AtaCommands.ReadBufferDma};

            LastError = SendAtaCommand(registers,  out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout,             false,           out duration, out bool sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ BUFFER DMA took {0} ms.", duration);

            return sense;
        }

        public bool ReadDma(out byte[] buffer,  out AtaErrorRegistersLba28 statusRegisters, uint lba, byte count,
                            uint       timeout, out double                 duration)
        {
            return ReadDma(out buffer, out statusRegisters, true, lba, count, timeout, out duration);
        }

        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, bool       retry, uint lba,
                            byte       count,  uint                       timeout,         out double duration)
        {
            buffer = count == 0 ? new byte[512 * 256] : new byte[512 * count];
            AtaRegistersLba28 registers = new AtaRegistersLba28
            {
                SectorCount = count,
                DeviceHead  = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh     = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid      = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow      = (byte)((lba & 0xFF)      / 0x1),
                Command     = retry ? (byte)AtaCommands.ReadDmaRetry : (byte)AtaCommands.ReadDma
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma,
                                       AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out bool sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ DMA took {0} ms.", duration);

            return sense;
        }

        public bool ReadMultiple(out byte[] buffer,  out AtaErrorRegistersLba28 statusRegisters, uint lba, byte count,
                                 uint       timeout, out double                 duration)
        {
            buffer = count == 0 ? new byte[512 * 256] : new byte[512 * count];
            AtaRegistersLba28 registers = new AtaRegistersLba28
            {
                Command     = (byte)AtaCommands.ReadMultiple,
                SectorCount = count,
                DeviceHead  = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh     = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid      = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow      = (byte)((lba & 0xFF)      / 0x1)
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers,                       out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer,          timeout, true,
                                       out duration,
                                       out bool sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ MULTIPLE took {0} ms.", duration);

            return sense;
        }

        public bool ReadNativeMaxAddress(out uint   lba, out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                                         out double duration)
        {
            lba = 0;
            byte[]            buffer    = new byte[0];
            AtaRegistersLba28 registers = new AtaRegistersLba28 {Command = (byte)AtaCommands.ReadNativeMaxAddress};

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers,                      out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer,          timeout, false,
                                       out duration,
                                       out bool sense);
            Error = LastError != 0;

            if((statusRegisters.Status & 0x23) == 0)
            {
                lba += (uint)(statusRegisters.DeviceHead & 0xF);
                lba *= 0x1000000;
                lba += (uint)(statusRegisters.LbaHigh << 16);
                lba += (uint)(statusRegisters.LbaMid  << 8);
                lba += statusRegisters.LbaLow;
            }

            DicConsole.DebugWriteLine("ATA Device", "READ NATIVE MAX ADDRESS took {0} ms.", duration);

            return sense;
        }

        public bool Read(out byte[] buffer,  out AtaErrorRegistersLba28 statusRegisters, uint lba, byte count,
                         uint       timeout, out double                 duration)
        {
            return Read(out buffer, out statusRegisters, true, lba, count, timeout, out duration);
        }

        public bool Read(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, bool       retry, uint lba,
                         byte       count,  uint                       timeout,         out double duration)
        {
            buffer = count == 0 ? new byte[512 * 256] : new byte[512 * count];
            AtaRegistersLba28 registers = new AtaRegistersLba28
            {
                SectorCount = count,
                DeviceHead  = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh     = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid      = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow      = (byte)((lba & 0xFF)      / 0x1),
                Command     = retry ? (byte)AtaCommands.ReadRetry : (byte)AtaCommands.Read
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers,                       out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer,          timeout, true,
                                       out duration,
                                       out bool sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ SECTORS took {0} ms.", duration);

            return sense;
        }

        public bool ReadLong(out byte[] buffer,  out AtaErrorRegistersLba28 statusRegisters, uint lba, uint blockSize,
                             uint       timeout, out double                 duration)
        {
            return ReadLong(out buffer, out statusRegisters, true, lba, blockSize, timeout, out duration);
        }

        public bool ReadLong(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, bool retry,
                             uint       lba,
                             uint       blockSize, uint timeout, out double duration)
        {
            buffer = new byte[blockSize];
            AtaRegistersLba28 registers = new AtaRegistersLba28
            {
                SectorCount = 1,
                DeviceHead  = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh     = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid      = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow      = (byte)((lba & 0xFF)      / 0x1),
                Command     = retry ? (byte)AtaCommands.ReadLongRetry : (byte)AtaCommands.ReadLong
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers,                       out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer,          timeout, true,
                                       out duration,
                                       out bool sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ LONG took {0} ms.", duration);

            return sense;
        }

        public bool Seek(out AtaErrorRegistersLba28 statusRegisters, uint lba, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersLba28 registers = new AtaRegistersLba28
            {
                Command    = (byte)AtaCommands.Seek,
                DeviceHead = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh    = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid     = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow     = (byte)((lba & 0xFF)      / 0x1)
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers,                      out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer,          timeout, false,
                                       out duration,
                                       out bool sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SEEK took {0} ms.", duration);

            return sense;
        }
    }
}