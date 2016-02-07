// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AtaCommands.cs
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
// Contains ATA commands
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
        /// <summary>
        /// Sends the ATA IDENTIFY DEVICE command to the device, using default device timeout
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="statusRegisters"/> contains the error registers.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="statusRegisters">Status registers.</param>
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersCHS statusRegisters)
        {
            return AtaIdentify(out buffer, out statusRegisters, Timeout);
        }

        /// <summary>
        /// Sends the ATA IDENTIFY DEVICE command to the device, using default device timeout
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="statusRegisters"/> contains the error registers.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="statusRegisters">Status registers.</param>
        /// <param name="duration">Duration.</param>
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersCHS statusRegisters, out double duration)
        {
            return AtaIdentify(out buffer, out statusRegisters, Timeout, out duration);
        }

        /// <summary>
        /// Sends the ATA IDENTIFY DEVICE command to the device
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="statusRegisters"/> contains the error registers.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="statusRegisters">Status registers.</param>
        /// <param name="timeout">Timeout.</param>
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersCHS statusRegisters, uint timeout)
        {
            double duration;
            return AtaIdentify(out buffer, out statusRegisters, timeout, out duration);
        }

        /// <summary>
        /// Sends the ATA IDENTIFY DEVICE command to the device
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="statusRegisters"/> contains the error registers.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="statusRegisters">Status registers.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersCHS statusRegisters, uint timeout, out double duration)
        {
            buffer = new byte[512];
            AtaRegistersCHS registers = new AtaRegistersCHS();
            bool sense;

            registers.command = (byte)AtaCommands.IdentifyDevice;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.NoTransfer,
                ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "IDENTIFY DEVICE took {0} ms.", duration);

            return sense;
        }

        public bool GetNativeMaxAddressExt(out ulong lba, out AtaErrorRegistersLBA48 statusRegisters, uint timeout, out double duration)
        {
            lba = 0;
            AtaRegistersLBA48 registers = new AtaRegistersLBA48();
            bool sense;
            byte[] buffer = new byte[0];

            registers.command = (byte)AtaCommands.NativeMaxAddress;
            registers.feature = 0x0000;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            if ((statusRegisters.status & 0x23) == 0)
            {
                lba = statusRegisters.lbaHigh;
                lba *= 0x100000000;
                lba += (ulong)(statusRegisters.lbaMid << 16);
                lba += statusRegisters.lbaLow;
            }

            DicConsole.DebugWriteLine("ATA Device", "GET NATIVE MAX ADDRESS EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadBuffer(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, uint timeout, out double duration)
        {
            buffer = new byte[512];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.ReadBuffer;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ BUFFER took {0} ms.", duration);

            return sense;
        }

        public bool ReadBufferDma(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, uint timeout, out double duration)
        {
            buffer = new byte[512];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.ReadBufferDma;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ BUFFER DMA took {0} ms.", duration);

            return sense;
        }

        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, uint lba, byte count, uint timeout, out double duration)
        {
            if (count == 0)
                buffer = new byte[512 * 256];
            else
                buffer = new byte[512 * count];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.ReadDmaRetry;
            registers.sectorCount = count;
            registers.lbaHigh = (byte)((lba & 0xFF0000) / 0x10000);
            registers.lbaMid = (byte)((lba & 0xFF00) / 0x100);
            registers.lbaLow = (byte)((lba & 0xFF) / 0x1);

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ DMA took {0} ms.", duration);

            return sense;
        }

        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersLBA48 statusRegisters, ulong lba, ushort count, uint timeout, out double duration)
        {
            if (count == 0)
                buffer = new byte[512 * 65536];
            else
                buffer = new byte[512 * count];
            AtaRegistersLBA48 registers = new AtaRegistersLBA48();
            bool sense;

            registers.command = (byte)AtaCommands.ReadDmaExt;
            registers.sectorCount = count;
            registers.lbaHigh = (ushort)((lba & 0xFFFF00000000) / 0x100000000);
            registers.lbaMid = (ushort)((lba & 0xFFFF0000) / 0x10000);
            registers.lbaLow = (ushort)((lba & 0xFFFF) / 0x1);

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ DMA EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadLog(out byte[] buffer, out AtaErrorRegistersLBA48 statusRegisters, byte logAddress, ushort pageNumber, ushort count, uint timeout, out double duration)
        {
            buffer = new byte[512 * count];
            AtaRegistersLBA48 registers = new AtaRegistersLBA48();
            bool sense;

            registers.command = (byte)AtaCommands.ReadLogExt;
            registers.sectorCount = count;
            registers.lbaLow = (ushort)((pageNumber & 0xFF) * 0x100);
            registers.lbaLow += logAddress;
            registers.lbaHigh = (ushort)((pageNumber & 0xFF00) / 0x100);

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ LOG EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadLogDma(out byte[] buffer, out AtaErrorRegistersLBA48 statusRegisters, byte logAddress, ushort pageNumber, ushort count, uint timeout, out double duration)
        {
            buffer = new byte[512 * count];
            AtaRegistersLBA48 registers = new AtaRegistersLBA48();
            bool sense;

            registers.command = (byte)AtaCommands.ReadLogDmaExt;
            registers.sectorCount = count;
            registers.lbaLow = (ushort)((pageNumber & 0xFF) * 0x100);
            registers.lbaLow += logAddress;
            registers.lbaHigh = (ushort)((pageNumber & 0xFF00) / 0x100);

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ LOG DMA EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadMultiple(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, byte count, uint lba, uint timeout, out double duration)
        {
            if (count == 0)
                buffer = new byte[512 * 256];
            else
                buffer = new byte[512 * count];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.ReadMultiple;
            registers.sectorCount = count;
            registers.lbaHigh = (byte)((lba & 0xFF0000) / 0x10000);
            registers.lbaMid = (byte)((lba & 0xFF00) / 0x100);
            registers.lbaLow = (byte)((lba & 0xFF) / 0x1);

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ MULTIPLE took {0} ms.", duration);

            return sense;
        }

        public bool ReadMultiple(out byte[] buffer, out AtaErrorRegistersLBA48 statusRegisters, ulong lba, ushort count, uint timeout, out double duration)
        {
            if (count == 0)
                buffer = new byte[512 * 65536];
            else
                buffer = new byte[512 * count];
            AtaRegistersLBA48 registers = new AtaRegistersLBA48();
            bool sense;

            registers.command = (byte)AtaCommands.ReadMultipleExt;
            registers.sectorCount = count;
            registers.lbaHigh = (ushort)((lba & 0xFFFF00000000) / 0x100000000);
            registers.lbaMid = (ushort)((lba & 0xFFFF0000) / 0x10000);
            registers.lbaLow = (ushort)((lba & 0xFFFF) / 0x1);

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ MULTIPLE EXT took {0} ms.", duration);

            return sense;
        }

        public bool ReadSectors(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, byte count, uint lba, uint timeout, out double duration)
        {
            if (count == 0)
                buffer = new byte[512 * 256];
            else
                buffer = new byte[512 * count];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.ReadRetry;
            registers.sectorCount = count;
            registers.lbaHigh = (byte)((lba & 0xFF0000) / 0x10000);
            registers.lbaMid = (byte)((lba & 0xFF00) / 0x100);
            registers.lbaLow = (byte)((lba & 0xFF) / 0x1);

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ SECTORS took {0} ms.", duration);

            return sense;
        }

        public bool ReadSectors(out byte[] buffer, out AtaErrorRegistersLBA48 statusRegisters, ulong lba, ushort count, uint timeout, out double duration)
        {
            if (count == 0)
                buffer = new byte[512 * 65536];
            else
                buffer = new byte[512 * count];
            AtaRegistersLBA48 registers = new AtaRegistersLBA48();
            bool sense;

            registers.command = (byte)AtaCommands.ReadExt;
            registers.sectorCount = count;
            registers.lbaHigh = (ushort)((lba & 0xFFFF00000000) / 0x100000000);
            registers.lbaMid = (ushort)((lba & 0xFFFF0000) / 0x10000);
            registers.lbaLow = (ushort)((lba & 0xFFFF) / 0x1);

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ SECTORS EXT took {0} ms.", duration);

            return sense;
        }
    }
}

