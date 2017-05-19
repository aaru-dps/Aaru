// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        /// <summary>
        /// Sends a SCSI command to this device
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if SCSI command returned non-OK status and <paramref name="senseBuffer"/> contains SCSI sense</param>
        public int SendScsiCommand(byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, ScsiDirection direction, out double duration, out bool sense)
        {
            return Command.SendScsiCommand(platformID, fd, cdb, ref buffer, out senseBuffer, timeout, direction, out duration, out sense);
        }

        /// <summary>
        /// Sends an ATA/ATAPI command to this device using CHS addressing
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="registers">ATA registers.</param>
        /// <param name="errorRegisters">Status/error registers.</param>
        /// <param name="protocol">ATA Protocol.</param>
        /// <param name="transferRegister">Indicates which register indicates the transfer length</param>
        /// <param name="buffer">Buffer for ATA/ATAPI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="transferBlocks">If set to <c>true</c>, transfer is indicated in blocks, otherwise, it is indicated in bytes.</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA/ATAPI command returned non-OK status</param>
        public int SendAtaCommand(AtaRegistersCHS registers, out AtaErrorRegistersCHS errorRegisters,
            AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            return Command.SendAtaCommand(platformID, fd, registers, out errorRegisters, protocol, transferRegister,
                ref buffer, timeout, transferBlocks, out duration, out sense);
        }

        /// <summary>
        /// Sends an ATA/ATAPI command to this device using 28-bit LBA addressing
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="registers">ATA registers.</param>
        /// <param name="errorRegisters">Status/error registers.</param>
        /// <param name="protocol">ATA Protocol.</param>
        /// <param name="transferRegister">Indicates which register indicates the transfer length</param>
        /// <param name="buffer">Buffer for ATA/ATAPI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="transferBlocks">If set to <c>true</c>, transfer is indicated in blocks, otherwise, it is indicated in bytes.</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA/ATAPI command returned non-OK status</param>
        public int SendAtaCommand(AtaRegistersLBA28 registers, out AtaErrorRegistersLBA28 errorRegisters,
            AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            return Command.SendAtaCommand(platformID, fd, registers, out errorRegisters, protocol, transferRegister,
                ref buffer, timeout, transferBlocks, out duration, out sense);
        }

        /// <summary>
        /// Sends an ATA/ATAPI command to this device using 48-bit LBA addressing
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="registers">ATA registers.</param>
        /// <param name="errorRegisters">Status/error registers.</param>
        /// <param name="protocol">ATA Protocol.</param>
        /// <param name="transferRegister">Indicates which register indicates the transfer length</param>
        /// <param name="buffer">Buffer for ATA/ATAPI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="transferBlocks">If set to <c>true</c>, transfer is indicated in blocks, otherwise, it is indicated in bytes.</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA/ATAPI command returned non-OK status</param>
        public int SendAtaCommand(AtaRegistersLBA48 registers, out AtaErrorRegistersLBA48 errorRegisters,
            AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            return Command.SendAtaCommand(platformID, fd, registers, out errorRegisters, protocol, transferRegister,
                ref buffer, timeout, transferBlocks, out duration, out sense);
        }

        /// <summary>
        /// Sends a MMC/SD command to this device
        /// </summary>
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
        public int SendMmcCommand(MmcCommands command, bool write, bool isApplication, MmcFlags flags,
                                  uint argument, uint blockSize, uint blocks, ref byte[] buffer, out uint[] response,
                                  out double duration, out bool sense, uint timeout = 0)
        {
            return Command.SendMmcCommand(platformID, fd, command, write, isApplication, flags, argument, blockSize, blocks,
                                          ref buffer, out response, out duration, out sense, timeout);
        }
    }
}

