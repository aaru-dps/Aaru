// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AtaCHS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ATA commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains ATA commands.
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
        /// <summary>
        /// Sends the ATA IDENTIFY DEVICE command to the device, using default device timeout
        /// </summary>
        /// <returns><c>true</c> if the command failed and <paramref name="statusRegisters"/> contains the error registers.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="statusRegisters">Status registers.</param>
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs statusRegisters)
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
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, out double duration)
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
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, uint timeout)
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
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, uint timeout,
                                out double duration)
        {
            buffer = new byte[512];
            AtaRegistersChs registers = new AtaRegistersChs();
            bool sense;

            registers.Command = (byte)AtaCommands.IdentifyDevice;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "IDENTIFY DEVICE took {0} ms.", duration);

            return sense;
        }

        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, ushort cylinder, byte head,
                            byte sector, byte count, uint timeout, out double duration)
        {
            return ReadDma(out buffer, out statusRegisters, true, cylinder, head, sector, count, timeout, out duration);
        }

        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, bool retry, ushort cylinder,
                            byte head, byte sector, byte count, uint timeout, out double duration)
        {
            if(count == 0) buffer = new byte[512 * 256];
            else buffer = new byte[512 * count];
            AtaRegistersChs registers = new AtaRegistersChs();
            bool sense;

            if(retry) registers.Command = (byte)AtaCommands.ReadDmaRetry;
            else registers.Command = (byte)AtaCommands.ReadDma;
            registers.SectorCount = count;
            registers.CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100);
            registers.CylinderLow = (byte)((cylinder & 0xFF) / 0x1);
            registers.DeviceHead = (byte)(head & 0x0F);
            registers.Sector = sector;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ DMA took {0} ms.", duration);

            return sense;
        }

        public bool ReadMultiple(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, ushort cylinder,
                                 byte head, byte sector, byte count, uint timeout, out double duration)
        {
            if(count == 0) buffer = new byte[512 * 256];
            else buffer = new byte[512 * count];
            AtaRegistersChs registers = new AtaRegistersChs();
            bool sense;

            registers.Command = (byte)AtaCommands.ReadMultiple;
            registers.SectorCount = count;
            registers.CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100);
            registers.CylinderLow = (byte)((cylinder & 0xFF) / 0x1);
            registers.DeviceHead = (byte)(head & 0x0F);
            registers.Sector = sector;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ MULTIPLE took {0} ms.", duration);

            return sense;
        }

        public bool Read(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, ushort cylinder, byte head,
                         byte sector, byte count, uint timeout, out double duration)
        {
            return Read(out buffer, out statusRegisters, true, cylinder, head, sector, count, timeout, out duration);
        }

        public bool Read(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, bool retry, ushort cylinder,
                         byte head, byte sector, byte count, uint timeout, out double duration)
        {
            if(count == 0) buffer = new byte[512 * 256];
            else buffer = new byte[512 * count];
            AtaRegistersChs registers = new AtaRegistersChs();
            bool sense;

            if(retry) registers.Command = (byte)AtaCommands.ReadRetry;
            else registers.Command = (byte)AtaCommands.Read;
            registers.SectorCount = count;
            registers.CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100);
            registers.CylinderLow = (byte)((cylinder & 0xFF) / 0x1);
            registers.DeviceHead = (byte)(head & 0x0F);
            registers.Sector = sector;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ SECTORS took {0} ms.", duration);

            return sense;
        }

        public bool ReadLong(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, ushort cylinder, byte head,
                             byte sector, uint blockSize, uint timeout, out double duration)
        {
            return ReadLong(out buffer, out statusRegisters, true, cylinder, head, sector, blockSize, timeout,
                            out duration);
        }

        public bool ReadLong(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, bool retry, ushort cylinder,
                             byte head, byte sector, uint blockSize, uint timeout, out double duration)
        {
            buffer = new byte[blockSize];
            AtaRegistersChs registers = new AtaRegistersChs();
            bool sense;

            if(retry) registers.Command = (byte)AtaCommands.ReadLongRetry;
            else registers.Command = (byte)AtaCommands.ReadLong;
            registers.SectorCount = 1;
            registers.CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100);
            registers.CylinderLow = (byte)((cylinder & 0xFF) / 0x1);
            registers.DeviceHead = (byte)(head & 0x0F);
            registers.Sector = sector;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "READ LONG took {0} ms.", duration);

            return sense;
        }

        public bool Seek(out AtaErrorRegistersChs statusRegisters, ushort cylinder, byte head, byte sector,
                         uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];
            AtaRegistersChs registers = new AtaRegistersChs();
            bool sense;

            registers.Command = (byte)AtaCommands.Seek;
            registers.CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100);
            registers.CylinderLow = (byte)((cylinder & 0xFF) / 0x1);
            registers.DeviceHead = (byte)(head & 0x0F);
            registers.Sector = sector;

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, true, out duration,
                                       out sense);
            Error = LastError != 0;

            DicConsole.DebugWriteLine("ATA Device", "SEEK took {0} ms.", duration);

            return sense;
        }
    }
}