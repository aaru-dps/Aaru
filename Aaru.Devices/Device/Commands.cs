// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Commands.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Sends commands to devices.
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
using System.Diagnostics.CodeAnalysis;
using Aaru.Decoders.ATA;

namespace Aaru.Devices
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed partial class Device
    {
        /// <summary>Sends a SCSI command to this device</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense">
        ///     <c>True</c> if SCSI command returned non-OK status and <paramref name="senseBuffer" /> contains
        ///     SCSI sense
        /// </param>
        public int SendScsiCommand(byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout,
                                   ScsiDirection direction, out double duration, out bool sense)
        {
            // We need a timeout
            if(timeout == 0)
                timeout = Timeout > 0 ? Timeout : 15;

            if(!(_remote is null))
                return _remote.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, direction, out duration,
                                               out sense);

            return Command.SendScsiCommand(PlatformId, FileHandle, cdb, ref buffer, out senseBuffer, timeout, direction,
                                           out duration, out sense);
        }

        /// <summary>Sends an ATA/ATAPI command to this device using CHS addressing</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="registers">ATA registers.</param>
        /// <param name="errorRegisters">Status/error registers.</param>
        /// <param name="protocol">ATA Protocol.</param>
        /// <param name="transferRegister">Indicates which register indicates the transfer length</param>
        /// <param name="buffer">Buffer for ATA/ATAPI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="transferBlocks">
        ///     If set to <c>true</c>, transfer is indicated in blocks, otherwise, it is indicated in
        ///     bytes.
        /// </param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA/ATAPI command returned non-OK status</param>
        public int SendAtaCommand(AtaRegistersChs registers, out AtaErrorRegistersChs errorRegisters,
                                  AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                  uint timeout, bool transferBlocks, out double duration, out bool sense)
        {
            // We need a timeout
            if(timeout == 0)
                timeout = Timeout > 0 ? Timeout : 15;

            if(!(_remote is null))
                return _remote.SendAtaCommand(registers, out errorRegisters, protocol, transferRegister, ref buffer,
                                              timeout, transferBlocks, out duration, out sense);

            return Command.SendAtaCommand(PlatformId, FileHandle, registers, out errorRegisters, protocol,
                                          transferRegister, ref buffer, timeout, transferBlocks, out duration,
                                          out sense);
        }

        /// <summary>Sends an ATA/ATAPI command to this device using 28-bit LBA addressing</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="registers">ATA registers.</param>
        /// <param name="errorRegisters">Status/error registers.</param>
        /// <param name="protocol">ATA Protocol.</param>
        /// <param name="transferRegister">Indicates which register indicates the transfer length</param>
        /// <param name="buffer">Buffer for ATA/ATAPI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="transferBlocks">
        ///     If set to <c>true</c>, transfer is indicated in blocks, otherwise, it is indicated in
        ///     bytes.
        /// </param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA/ATAPI command returned non-OK status</param>
        public int SendAtaCommand(AtaRegistersLba28 registers, out AtaErrorRegistersLba28 errorRegisters,
                                  AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                  uint timeout, bool transferBlocks, out double duration, out bool sense)
        {
            // We need a timeout
            if(timeout == 0)
                timeout = Timeout > 0 ? Timeout : 15;

            if(!(_remote is null))
                return _remote.SendAtaCommand(registers, out errorRegisters, protocol, transferRegister, ref buffer,
                                              timeout, transferBlocks, out duration, out sense);

            return Command.SendAtaCommand(PlatformId, FileHandle, registers, out errorRegisters, protocol,
                                          transferRegister, ref buffer, timeout, transferBlocks, out duration,
                                          out sense);
        }

        /// <summary>Sends an ATA/ATAPI command to this device using 48-bit LBA addressing</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="registers">ATA registers.</param>
        /// <param name="errorRegisters">Status/error registers.</param>
        /// <param name="protocol">ATA Protocol.</param>
        /// <param name="transferRegister">Indicates which register indicates the transfer length</param>
        /// <param name="buffer">Buffer for ATA/ATAPI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="transferBlocks">
        ///     If set to <c>true</c>, transfer is indicated in blocks, otherwise, it is indicated in
        ///     bytes.
        /// </param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA/ATAPI command returned non-OK status</param>
        public int SendAtaCommand(AtaRegistersLba48 registers, out AtaErrorRegistersLba48 errorRegisters,
                                  AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                  uint timeout, bool transferBlocks, out double duration, out bool sense)
        {
            // We need a timeout
            if(timeout == 0)
                timeout = Timeout > 0 ? Timeout : 15;

            if(!(_remote is null))
                return _remote.SendAtaCommand(registers, out errorRegisters, protocol, transferRegister, ref buffer,
                                              timeout, transferBlocks, out duration, out sense);

            return Command.SendAtaCommand(PlatformId, FileHandle, registers, out errorRegisters, protocol,
                                          transferRegister, ref buffer, timeout, transferBlocks, out duration,
                                          out sense);
        }

        /// <summary>Sends a MMC/SD command to this device</summary>
        /// <returns>The result of the command.</returns>
        /// <param name="command">MMC/SD opcode</param>
        /// <param name="buffer">Buffer for MMC/SD command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if MMC/SD returned non-OK status</param>
        /// <param name="write"><c>True</c> if data is sent from host to card</param>
        /// <param name="isApplication"><c>True</c> if command should be preceded with CMD55</param>
        /// <param name="flags">Flags indicating kind and place of response</param>
        /// <param name="blocks">How many blocks to transfer</param>
        /// <param name="argument">Command argument</param>
        /// <param name="response">Response registers</param>
        /// <param name="blockSize">Size of block in bytes</param>
        public int SendMmcCommand(MmcCommands command, bool write, bool isApplication, MmcFlags flags, uint argument,
                                  uint blockSize, uint blocks, ref byte[] buffer, out uint[] response,
                                  out double duration, out bool sense, uint timeout = 15)
        {
            // We need a timeout
            if(timeout == 0)
                timeout = Timeout > 0 ? Timeout : 15;

            switch(command)
            {
                case MmcCommands.SendCid when _cachedCid != null:
                {
                    DateTime start = DateTime.Now;
                    buffer = new byte[_cachedCid.Length];
                    Array.Copy(_cachedCid, buffer, buffer.Length);
                    response = new uint[4];
                    sense    = false;
                    DateTime end = DateTime.Now;
                    duration = (end - start).TotalMilliseconds;

                    return 0;
                }
                case MmcCommands.SendCsd when _cachedCid != null:
                {
                    DateTime start = DateTime.Now;
                    buffer = new byte[_cachedCsd.Length];
                    Array.Copy(_cachedCsd, buffer, buffer.Length);
                    response = new uint[4];
                    sense    = false;
                    DateTime end = DateTime.Now;
                    duration = (end - start).TotalMilliseconds;

                    return 0;
                }
                case (MmcCommands)SecureDigitalCommands.SendScr when _cachedScr != null:
                {
                    DateTime start = DateTime.Now;
                    buffer = new byte[_cachedScr.Length];
                    Array.Copy(_cachedScr, buffer, buffer.Length);
                    response = new uint[4];
                    sense    = false;
                    DateTime end = DateTime.Now;
                    duration = (end - start).TotalMilliseconds;

                    return 0;
                }
                case (MmcCommands)SecureDigitalCommands.SendOperatingCondition when _cachedOcr != null:
                case MmcCommands.SendOpCond when _cachedOcr                                    != null:
                {
                    DateTime start = DateTime.Now;
                    buffer = new byte[_cachedOcr.Length];
                    Array.Copy(_cachedOcr, buffer, buffer.Length);
                    response = new uint[4];
                    sense    = false;
                    DateTime end = DateTime.Now;
                    duration = (end - start).TotalMilliseconds;

                    return 0;
                }
            }

            return _remote is null
                       ? Command.SendMmcCommand(PlatformId, FileHandle, command, write, isApplication, flags, argument,
                                                blockSize, blocks, ref buffer, out response, out duration, out sense,
                                                timeout) : _remote.SendMmcCommand(command, write, isApplication, flags,
                           argument, blockSize, blocks, ref buffer, out response, out duration, out sense,
                           timeout);
        }

        public class MmcSingleCommand
        {
            public uint        argument;
            public uint        blocks;
            public uint        blockSize;
            public byte[]      buffer;
            public MmcCommands command;
            public MmcFlags    flags;
            public bool        isApplication;
            public uint[]      response;
            public bool        write;
        }

        public int SendMultipleMmcCommands(MmcSingleCommand[] commands, out double duration, out bool sense,
                                           uint timeout = 15)
        {
            // We need a timeout
            if(timeout == 0)
                timeout = Timeout > 0 ? Timeout : 15;

            if(_remote is null)
                return Command.SendMultipleMmcCommands(PlatformId, FileHandle, commands, out duration, out sense,
                                                       timeout);

            if(_remote.ServerProtocolVersion >= 2)
                return _remote.SendMultipleMmcCommands(commands, out duration, out sense, timeout);

            int error = 0;
            duration = 0;
            sense    = false;

            foreach(MmcSingleCommand command in commands)
            {
                int singleError = _remote.SendMmcCommand(command.command, command.write, command.isApplication,
                                                         command.flags, command.argument, command.blockSize,
                                                         command.blocks, ref command.buffer, out command.response,
                                                         out double cmdDuration, out bool cmdSense, timeout);

                if(error       == 0 &&
                   singleError != 0)
                    error = singleError;

                duration += cmdDuration;

                if(cmdSense)
                    sense = true;
            }

            return error;
        }

        public bool ReOpen()
        {
            if(!(_remote is null))
                return _remote.ReOpen();

            int ret = Command.ReOpen(PlatformId, _devicePath, FileHandle, out object newFileHandle);

            FileHandle = newFileHandle;

            Error     = ret != 0;
            LastError = ret;

            return Error;
        }

        public bool BufferedOsRead(out byte[] buffer, long offset, uint length, out double duration)
        {
            if(!(_remote is null))
                return _remote.BufferedOsRead(out buffer, offset, length, out duration);

            int ret = Command.BufferedOsRead(PlatformId, FileHandle, out buffer, offset, length, out duration);

            Error     = ret != 0;
            LastError = ret;

            return Error;
        }
    }
}