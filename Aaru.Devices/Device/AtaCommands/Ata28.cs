// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Console;
using Aaru.Decoders.ATA;

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        /// <summary>Reads the drive buffer using PIO transfer</summary>
        /// <param name="buffer">Buffer that contains the read data</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadBuffer(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                               out double duration)
        {
            buffer = new byte[512];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.ReadBuffer
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ BUFFER took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads the drive buffer using DMA transfer</summary>
        /// <param name="buffer">Buffer that contains the read data</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadBufferDma(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                                  out double duration)
        {
            buffer = new byte[512];

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.ReadBufferDma
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ BUFFER DMA took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads sectors using 28-bit addressing and DMA transfer, retrying on error</summary>
        /// <param name="buffer">Buffer that contains the read data</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="lba">LBA of read start</param>
        /// <param name="count">How many blocks to read, or 0 to indicate 256 blocks</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint lba, byte count,
                            uint timeout, out double duration) =>
            ReadDma(out buffer, out statusRegisters, true, lba, count, timeout, out duration);

        /// <summary>Reads sectors using 48-bit addressing and DMA transfer</summary>
        /// <param name="buffer">Buffer that contains the read data</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="retry">Retry on error</param>
        /// <param name="lba">LBA of read start</param>
        /// <param name="count">How many blocks to read, or 0 to indicate 256 blocks</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, bool retry, uint lba,
                            byte count, uint timeout, out double duration)
        {
            buffer = count == 0 ? new byte[512 * 256] : new byte[512 * count];

            var registers = new AtaRegistersLba28
            {
                SectorCount = count,
                DeviceHead  = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh     = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid      = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow      = (byte)((lba & 0xFF)      / 0x1),
                Command     = retry ? (byte)AtaCommands.ReadDmaRetry : (byte)AtaCommands.ReadDma
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ DMA took {0} ms.", duration);

            return sense;
        }

        /// <summary>
        ///     Reads sectors using 28-bit addressing and PIO transfer, sending an interrupt only after all the sectors have
        ///     been transferred
        /// </summary>
        /// <param name="buffer">Buffer that contains the read data</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="lba">LBA of read start</param>
        /// <param name="count">How many blocks to read, or 0 to indicate 256 blocks</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadMultiple(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint lba, byte count,
                                 uint timeout, out double duration)
        {
            buffer = count == 0 ? new byte[512 * 256] : new byte[512 * count];

            var registers = new AtaRegistersLba28
            {
                Command     = (byte)AtaCommands.ReadMultiple,
                SectorCount = count,
                DeviceHead  = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh     = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid      = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow      = (byte)((lba & 0xFF)      / 0x1)
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ MULTIPLE took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads native max address using 28-bit addressing</summary>
        /// <param name="lba">Maximum addressable block</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadNativeMaxAddress(out uint lba, out AtaErrorRegistersLba28 statusRegisters, uint timeout,
                                         out double duration)
        {
            lba = 0;
            byte[] buffer = Array.Empty<byte>();

            var registers = new AtaRegistersLba28
            {
                Command = (byte)AtaCommands.ReadNativeMaxAddress
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
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

            AaruConsole.DebugWriteLine("ATA Device", "READ NATIVE MAX ADDRESS took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads sectors using 28-bit addressing and PIO transfer, retrying on error</summary>
        /// <param name="buffer">Buffer that contains the read data</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="lba">LBA of read start</param>
        /// <param name="count">How many blocks to read, or 0 to indicate 256 blocks</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool Read(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint lba, byte count,
                         uint timeout, out double duration) =>
            Read(out buffer, out statusRegisters, true, lba, count, timeout, out duration);

        /// <summary>Reads sectors using 28-bit addressing and PIO transfer, retrying on error</summary>
        /// <param name="buffer">Buffer that contains the read data</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="retry">Retry on error</param>
        /// <param name="lba">LBA of read start</param>
        /// <param name="count">How many blocks to read, or 0 to indicate 256 blocks</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool Read(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, bool retry, uint lba,
                         byte count, uint timeout, out double duration)
        {
            buffer = count == 0 ? new byte[512 * 256] : new byte[512 * count];

            var registers = new AtaRegistersLba28
            {
                SectorCount = count,
                DeviceHead  = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh     = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid      = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow      = (byte)((lba & 0xFF)      / 0x1),
                Command     = retry ? (byte)AtaCommands.ReadRetry : (byte)AtaCommands.Read
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ SECTORS took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads a long sector using 28-bit addressing and PIO transfer, retrying on error</summary>
        /// <param name="buffer">Buffer that contains the read data</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="lba">LBA of read start</param>
        /// <param name="blockSize">Size in bytes of the long sector</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadLong(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, uint lba, uint blockSize,
                             uint timeout, out double duration) =>
            ReadLong(out buffer, out statusRegisters, true, lba, blockSize, timeout, out duration);

        /// <summary>Reads a long sector using 28-bit addressing and PIO transfer, retrying on error</summary>
        /// <param name="buffer">Buffer that contains the read data</param>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="retry">Retry on error</param>
        /// <param name="lba">LBA of read start</param>
        /// <param name="blockSize">Size in bytes of the long sector</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool ReadLong(out byte[] buffer, out AtaErrorRegistersLba28 statusRegisters, bool retry, uint lba,
                             uint blockSize, uint timeout, out double duration)
        {
            buffer = new byte[blockSize];

            var registers = new AtaRegistersLba28
            {
                SectorCount = 1,
                DeviceHead  = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh     = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid      = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow      = (byte)((lba & 0xFF)      / 0x1),
                Command     = retry ? (byte)AtaCommands.ReadLongRetry : (byte)AtaCommands.ReadLong
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ LONG took {0} ms.", duration);

            return sense;
        }

        /// <summary>Sets the reading mechanism ready to read the specified block using 28-bit LBA addressing</summary>
        /// <param name="statusRegisters">Returned status registers</param>
        /// <param name="lba">LBA to position reading mechanism ready to read</param>
        /// <param name="timeout">Timeout to wait for command execution</param>
        /// <param name="duration">Time the device took to execute the command in milliseconds</param>
        /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
        public bool Seek(out AtaErrorRegistersLba28 statusRegisters, uint lba, uint timeout, out double duration)
        {
            byte[] buffer = Array.Empty<byte>();

            var registers = new AtaRegistersLba28
            {
                Command    = (byte)AtaCommands.Seek,
                DeviceHead = (byte)((lba & 0xF000000) / 0x1000000),
                LbaHigh    = (byte)((lba & 0xFF0000)  / 0x10000),
                LbaMid     = (byte)((lba & 0xFF00)    / 0x100),
                LbaLow     = (byte)((lba & 0xFF)      / 0x1)
            };

            registers.DeviceHead += 0x40;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SEEK took {0} ms.", duration);

            return sense;
        }
    }
}