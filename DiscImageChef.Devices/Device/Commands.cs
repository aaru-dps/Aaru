// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Commands.cs
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
    }
}

