// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Ata28.cs
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
            return ReadDma(out buffer, out statusRegisters, true, lba, count, timeout, out duration);
        }

        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, bool retry, uint lba, byte count, uint timeout, out double duration)
        {
            if (count == 0)
                buffer = new byte[512 * 256];
            else
                buffer = new byte[512 * count];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            if(retry)
                registers.command = (byte)AtaCommands.ReadDmaRetry;
            else
                registers.command = (byte)AtaCommands.ReadDma;
            registers.sectorCount = count;
            registers.deviceHead = (byte)((lba & 0xF000000) / 0x1000000);
            registers.lbaHigh = (byte)((lba & 0xFF0000) / 0x10000);
            registers.lbaMid = (byte)((lba & 0xFF00) / 0x100);
            registers.lbaLow = (byte)((lba & 0xFF) / 0x1);
            registers.deviceHead += 0x40;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ DMA took {0} ms.", duration);

            return sense;
        }

        public bool ReadMultiple(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, uint lba, byte count, uint timeout, out double duration)
        {
            if (count == 0)
                buffer = new byte[512 * 256];
            else
                buffer = new byte[512 * count];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.ReadMultiple;
            registers.sectorCount = count;
            registers.deviceHead = (byte)((lba & 0xF000000) / 0x1000000);
            registers.lbaHigh = (byte)((lba & 0xFF0000) / 0x10000);
            registers.lbaMid = (byte)((lba & 0xFF00) / 0x100);
            registers.lbaLow = (byte)((lba & 0xFF) / 0x1);
            registers.deviceHead += 0x40;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ MULTIPLE took {0} ms.", duration);

            return sense;
        }

        public bool ReadNativeMaxAddress(out uint lba, out AtaErrorRegistersLBA28 statusRegisters, uint timeout, out double duration)
        {
            lba = 0;
            byte[] buffer = new byte[0];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.ReadNativeMaxAddress;
            registers.deviceHead += 0x40;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            if ((statusRegisters.status & 0x23) == 0)
            {
                lba += (uint)(statusRegisters.deviceHead & 0xF);
                lba *= 0x1000000;
                lba += (uint)(statusRegisters.lbaHigh << 16);
                lba += (uint)(statusRegisters.lbaMid << 8);
                lba += statusRegisters.lbaLow;
            }

            DicConsole.DebugWriteLine("ATA Device", "READ NATIVE MAX ADDRESS took {0} ms.", duration);

            return sense;
        }

        public bool Read(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, uint lba, byte count, uint timeout, out double duration)
        {
            return Read(out buffer, out statusRegisters, true, lba, count, timeout, out duration);
        }

        public bool Read(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, bool retry, uint lba, byte count, uint timeout, out double duration)
        {
            if (count == 0)
                buffer = new byte[512 * 256];
            else
                buffer = new byte[512 * count];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            if(retry)
                registers.command = (byte)AtaCommands.ReadRetry;
            else
                registers.command = (byte)AtaCommands.Read;
            registers.sectorCount = count;
            registers.deviceHead = (byte)((lba & 0xF000000) / 0x1000000);
            registers.lbaHigh = (byte)((lba & 0xFF0000) / 0x10000);
            registers.lbaMid = (byte)((lba & 0xFF00) / 0x100);
            registers.lbaLow = (byte)((lba & 0xFF) / 0x1);
            registers.deviceHead += 0x40;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ SECTORS took {0} ms.", duration);

            return sense;
        }


        public bool ReadLong(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, uint lba, uint blockSize, uint timeout, out double duration)
        {
            return ReadLong(out buffer, out statusRegisters, true, lba, blockSize, timeout, out duration);
        }

        public bool ReadLong(out byte[] buffer, out AtaErrorRegistersLBA28 statusRegisters, bool retry, uint lba, uint blockSize, uint timeout, out double duration)
        {
            buffer = new byte[blockSize];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            if(retry)
                registers.command = (byte)AtaCommands.ReadLongRetry;
            else
                registers.command = (byte)AtaCommands.ReadLong;
            registers.sectorCount = 1;
            registers.deviceHead = (byte)((lba & 0xF000000) / 0x1000000);
            registers.lbaHigh = (byte)((lba & 0xFF0000) / 0x10000);
            registers.lbaMid = (byte)((lba & 0xFF00) / 0x100);
            registers.lbaLow = (byte)((lba & 0xFF) / 0x1);
            registers.deviceHead += 0x40;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ LONG took {0} ms.", duration);

            return sense;
        }

        public bool Seek(out AtaErrorRegistersLBA28 statusRegisters, uint lba, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersLBA28 registers = new AtaRegistersLBA28();
            bool sense;

            registers.command = (byte)AtaCommands.Seek;
            registers.deviceHead = (byte)((lba & 0xF000000) / 0x1000000);
            registers.lbaHigh = (byte)((lba & 0xFF0000) / 0x10000);
            registers.lbaMid = (byte)((lba & 0xFF00) / 0x100);
            registers.lbaLow = (byte)((lba & 0xFF) / 0x1);
            registers.deviceHead += 0x40;

            lastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData, AtaTransferRegister.NoTransfer,
                                       ref buffer, timeout, false, out duration, out sense);
            error = lastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SEEK took {0} ms.", duration);

            return sense;
        }
    }
}

