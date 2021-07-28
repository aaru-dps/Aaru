// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Ata48.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ATA commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains 48-bit LBA ATA commands.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;
using Aaru.Decoders.ATA;

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        public bool GetNativeMaxAddressExt(out ulong lba, out AtaErrorRegistersLba48 statusRegisters, uint timeout,
                                           out double duration)
        {
            lba = 0;
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersLba48
            {
                Command = (byte)AtaCommands.NativeMaxAddress,
                Feature = 0x0000
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            if((statusRegisters.Status & 0x23) == 0)
            {
                lba = (ulong)((statusRegisters.LbaHighCurrent << 16) + (statusRegisters.LbaMidCurrent << 8) +
                              statusRegisters.LbaLowCurrent);

                lba <<= 24;

                lba += (ulong)((statusRegisters.LbaHighPrevious << 16) + (statusRegisters.LbaMidPrevious << 8) +
                               statusRegisters.LbaLowPrevious);
            }

            AaruConsole.DebugWriteLine("ATA Device", "GET NATIVE MAX ADDRESS EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersLba48 statusRegisters, ulong lba, ushort count,
                            uint timeout, out double duration)
        {
            buffer = count == 0 ? new byte[512 * 65536] : new byte[512 * count];

            var registers = new AtaRegistersLba48
            {
                Command         = (byte)AtaCommands.ReadDmaExt,
                SectorCount     = count,
                LbaHighPrevious = (byte)((lba & 0xFF0000000000) / 0x10000000000),
                LbaMidPrevious  = (byte)((lba & 0xFF00000000)   / 0x100000000),
                LbaLowPrevious  = (byte)((lba & 0xFF000000)     / 0x1000000),
                LbaHighCurrent  = (byte)((lba & 0xFF0000)       / 0x10000),
                LbaMidCurrent   = (byte)((lba & 0xFF00)         / 0x100),
                LbaLowCurrent   = (byte)(lba & 0xFF)
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ DMA EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadLog(out byte[] buffer, out AtaErrorRegistersLba48 statusRegisters, byte logAddress,
                            ushort pageNumber, ushort count, uint timeout, out double duration)
        {
            buffer = new byte[512 * count];

            var registers = new AtaRegistersLba48
            {
                Command        = (byte)AtaCommands.ReadLogExt,
                SectorCount    = count,
                LbaMidCurrent  = (byte)(pageNumber & 0xFF),
                LbaMidPrevious = (byte)((pageNumber & 0xFF00) / 0x100)
            };

            registers.LbaLowCurrent = logAddress;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ LOG EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadLogDma(out byte[] buffer, out AtaErrorRegistersLba48 statusRegisters, byte logAddress,
                               ushort pageNumber, ushort count, uint timeout, out double duration)
        {
            buffer = new byte[512 * count];

            var registers = new AtaRegistersLba48
            {
                Command        = (byte)AtaCommands.ReadLogDmaExt,
                SectorCount    = count,
                LbaMidCurrent  = (byte)(pageNumber & 0xFF),
                LbaMidPrevious = (byte)((pageNumber & 0xFF00) / 0x100)
            };

            registers.LbaLowCurrent = logAddress;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ LOG DMA EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadMultiple(out byte[] buffer, out AtaErrorRegistersLba48 statusRegisters, ulong lba, ushort count,
                                 uint timeout, out double duration)
        {
            buffer = count == 0 ? new byte[512 * 65536] : new byte[512 * count];

            var registers = new AtaRegistersLba48
            {
                Command         = (byte)AtaCommands.ReadMultipleExt,
                SectorCount     = count,
                LbaHighPrevious = (byte)((lba & 0xFF0000000000) / 0x10000000000),
                LbaMidPrevious  = (byte)((lba & 0xFF00000000)   / 0x100000000),
                LbaLowPrevious  = (byte)((lba & 0xFF000000)     / 0x1000000),
                LbaHighCurrent  = (byte)((lba & 0xFF0000)       / 0x10000),
                LbaMidCurrent   = (byte)((lba & 0xFF00)         / 0x100),
                LbaLowCurrent   = (byte)(lba & 0xFF)
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ MULTIPLE EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadNativeMaxAddress(out ulong lba, out AtaErrorRegistersLba48 statusRegisters, uint timeout,
                                         out double duration)
        {
            lba = 0;
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersLba48
            {
                Command = (byte)AtaCommands.ReadNativeMaxAddressExt
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            if((statusRegisters.Status & 0x23) == 0)
            {
                lba = (ulong)((statusRegisters.LbaHighCurrent << 16) + (statusRegisters.LbaMidCurrent << 8) +
                              statusRegisters.LbaLowCurrent);

                lba <<= 24;

                lba += (ulong)((statusRegisters.LbaHighPrevious << 16) + (statusRegisters.LbaMidPrevious << 8) +
                               statusRegisters.LbaLowPrevious);
            }

            AaruConsole.DebugWriteLine("ATA Device", "READ NATIVE MAX ADDRESS EXT took {0} ms.", duration);

            return sense;
        }

        public bool Read(out byte[] buffer, out AtaErrorRegistersLba48 statusRegisters, ulong lba, ushort count,
                         uint timeout, out double duration)
        {
            buffer = count == 0 ? new byte[512 * 65536] : new byte[512 * count];

            var registers = new AtaRegistersLba48
            {
                Command         = (byte)AtaCommands.ReadExt,
                SectorCount     = count,
                LbaHighPrevious = (byte)((lba & 0xFF0000000000) / 0x10000000000),
                LbaMidPrevious  = (byte)((lba & 0xFF00000000)   / 0x100000000),
                LbaLowPrevious  = (byte)((lba & 0xFF000000)     / 0x1000000),
                LbaHighCurrent  = (byte)((lba & 0xFF0000)       / 0x10000),
                LbaMidCurrent   = (byte)((lba & 0xFF00)         / 0x100),
                LbaLowCurrent   = (byte)(lba & 0xFF)
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ SECTORS EXT took {0} ms.", duration);

            return sense;
        }
    }
}