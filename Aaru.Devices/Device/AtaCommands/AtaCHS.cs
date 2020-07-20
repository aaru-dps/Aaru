// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;
using Aaru.Decoders.ATA;

namespace Aaru.Devices
{
    public partial class Device
    {
        /// <summary>Sends the ATA IDENTIFY DEVICE command to the device, using default device timeout</summary>
        /// <returns><c>true</c> if the command failed and <paramref name="statusRegisters" /> contains the error registers.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="statusRegisters">Status registers.</param>
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs statusRegisters) =>
            AtaIdentify(out buffer, out statusRegisters, Timeout);

        /// <summary>Sends the ATA IDENTIFY DEVICE command to the device, using default device timeout</summary>
        /// <returns><c>true</c> if the command failed and <paramref name="statusRegisters" /> contains the error registers.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="statusRegisters">Status registers.</param>
        /// <param name="duration">Duration.</param>
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, out double duration) =>
            AtaIdentify(out buffer, out statusRegisters, Timeout, out duration);

        /// <summary>Sends the ATA IDENTIFY DEVICE command to the device</summary>
        /// <returns><c>true</c> if the command failed and <paramref name="statusRegisters" /> contains the error registers.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="statusRegisters">Status registers.</param>
        /// <param name="timeout">Timeout.</param>
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, uint timeout) =>
            AtaIdentify(out buffer, out statusRegisters, timeout, out _);

        /// <summary>Sends the ATA IDENTIFY DEVICE command to the device</summary>
        /// <returns><c>true</c> if the command failed and <paramref name="statusRegisters" /> contains the error registers.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="statusRegisters">Status registers.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, uint timeout,
                                out double duration)
        {
            buffer = new byte[512];

            var registers = new AtaRegistersChs
            {
                Command = (byte)AtaCommands.IdentifyDevice
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, false, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "IDENTIFY DEVICE took {0} ms.", duration);

            return sense;
        }

        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, ushort cylinder, byte head,
                            byte sector, byte count, uint timeout, out double duration) =>
            ReadDma(out buffer, out statusRegisters, true, cylinder, head, sector, count, timeout, out duration);

        public bool ReadDma(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, bool retry, ushort cylinder,
                            byte head, byte sector, byte count, uint timeout, out double duration)
        {
            buffer = count == 0 ? new byte[512 * 256] : new byte[512 * count];

            var registers = new AtaRegistersChs
            {
                SectorCount  = count,
                CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100),
                CylinderLow  = (byte)((cylinder & 0xFF)   / 0x1),
                DeviceHead   = (byte)(head & 0x0F),
                Sector       = sector,
                Command      = retry ? (byte)AtaCommands.ReadDmaRetry : (byte)AtaCommands.ReadDma
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.Dma, AtaTransferRegister.SectorCount,
                                       ref buffer, timeout, true, out duration, out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ DMA took {0} ms.", duration);

            return sense;
        }

        public bool ReadMultiple(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, ushort cylinder,
                                 byte head, byte sector, byte count, uint timeout, out double duration)
        {
            buffer = count == 0 ? new byte[512 * 256] : new byte[512 * count];

            var registers = new AtaRegistersChs
            {
                Command      = (byte)AtaCommands.ReadMultiple,
                SectorCount  = count,
                CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100),
                CylinderLow  = (byte)((cylinder & 0xFF)   / 0x1),
                DeviceHead   = (byte)(head & 0x0F),
                Sector       = sector
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ MULTIPLE took {0} ms.", duration);

            return sense;
        }

        public bool Read(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, ushort cylinder, byte head,
                         byte sector, byte count, uint timeout, out double duration) =>
            Read(out buffer, out statusRegisters, true, cylinder, head, sector, count, timeout, out duration);

        public bool Read(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, bool retry, ushort cylinder,
                         byte head, byte sector, byte count, uint timeout, out double duration)
        {
            buffer = count == 0 ? new byte[512 * 256] : new byte[512 * count];

            var registers = new AtaRegistersChs
            {
                Command      = retry ? (byte)AtaCommands.ReadRetry : (byte)AtaCommands.Read,
                SectorCount  = count,
                CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100),
                CylinderLow  = (byte)((cylinder & 0xFF)   / 0x1),
                DeviceHead   = (byte)(head & 0x0F),
                Sector       = sector
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ SECTORS took {0} ms.", duration);

            return sense;
        }

        public bool ReadLong(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, ushort cylinder, byte head,
                             byte sector, uint blockSize, uint timeout, out double duration) =>
            ReadLong(out buffer, out statusRegisters, true, cylinder, head, sector, blockSize, timeout, out duration);

        public bool ReadLong(out byte[] buffer, out AtaErrorRegistersChs statusRegisters, bool retry, ushort cylinder,
                             byte head, byte sector, uint blockSize, uint timeout, out double duration)
        {
            buffer = new byte[blockSize];

            var registers = new AtaRegistersChs
            {
                Command      = retry ? (byte)AtaCommands.ReadLongRetry : (byte)AtaCommands.ReadLong,
                SectorCount  = 1,
                CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100),
                CylinderLow  = (byte)((cylinder & 0xFF)   / 0x1),
                DeviceHead   = (byte)(head & 0x0F),
                Sector       = sector
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.PioIn,
                                       AtaTransferRegister.SectorCount, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "READ LONG took {0} ms.", duration);

            return sense;
        }

        public bool Seek(out AtaErrorRegistersChs statusRegisters, ushort cylinder, byte head, byte sector,
                         uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersChs
            {
                Command      = (byte)AtaCommands.Seek,
                CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100),
                CylinderLow  = (byte)((cylinder & 0xFF)   / 0x1),
                DeviceHead   = (byte)(head & 0x0F),
                Sector       = sector
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SEEK took {0} ms.", duration);

            return sense;
        }

        public bool SetFeatures(out AtaErrorRegistersChs statusRegisters, AtaFeatures feature, uint timeout,
                                out double duration) =>
            SetFeatures(out statusRegisters, feature, 0, 0, 0, 0, timeout, out duration);

        public bool SetFeatures(out AtaErrorRegistersChs statusRegisters, AtaFeatures feature, ushort cylinder,
                                byte head, byte sector, byte sectorCount, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersChs
            {
                Command      = (byte)AtaCommands.SetFeatures,
                CylinderHigh = (byte)((cylinder & 0xFF00) / 0x100),
                CylinderLow  = (byte)((cylinder & 0xFF)   / 0x1),
                DeviceHead   = (byte)(head & 0x0F),
                Sector       = sector,
                SectorCount  = sectorCount,
                Feature      = (byte)feature
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "SET FEATURES took {0} ms.", duration);

            return sense;
        }

        public bool DoorLock(out AtaErrorRegistersChs statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersChs
            {
                Command = (byte)AtaCommands.DoorLock
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "DOOR LOCK took {0} ms.", duration);

            return sense;
        }

        public bool DoorUnlock(out AtaErrorRegistersChs statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersChs
            {
                Command = (byte)AtaCommands.DoorUnLock
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "DOOR UNLOCK took {0} ms.", duration);

            return sense;
        }

        public bool MediaEject(out AtaErrorRegistersChs statusRegisters, uint timeout, out double duration)
        {
            byte[] buffer = new byte[0];

            var registers = new AtaRegistersChs
            {
                Command = (byte)AtaCommands.MediaEject
            };

            LastError = SendAtaCommand(registers, out statusRegisters, AtaProtocol.NonData,
                                       AtaTransferRegister.NoTransfer, ref buffer, timeout, true, out duration,
                                       out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("ATA Device", "MEDIA EJECT took {0} ms.", duration);

            return sense;
        }
    }
}