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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using Aaru.Decoders.ATA;

namespace Aaru.Devices;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class Device
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
    public virtual int SendScsiCommand(byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout,
                                       ScsiDirection direction, out double duration, out bool sense)
    {
        duration    = 0;
        sense       = true;
        senseBuffer = null;

        return -1;
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
    public virtual int SendAtaCommand(AtaRegistersChs registers, out AtaErrorRegistersChs errorRegisters,
                                      AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                      uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        duration       = 0;
        sense          = true;
        errorRegisters = default;

        return -1;
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
    public virtual int SendAtaCommand(AtaRegistersLba28 registers, out AtaErrorRegistersLba28 errorRegisters,
                                      AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                      uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        errorRegisters = default;
        duration       = 0;
        sense          = true;

        return -1;
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
    public virtual int SendAtaCommand(AtaRegistersLba48 registers, out AtaErrorRegistersLba48 errorRegisters,
                                      AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                      uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        errorRegisters = default;
        duration       = 0;
        sense          = true;

        return -1;
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
    public virtual int SendMmcCommand(MmcCommands command, bool write, bool isApplication, MmcFlags flags,
                                      uint argument, uint blockSize, uint blocks, ref byte[] buffer,
                                      out uint[] response, out double duration, out bool sense, uint timeout = 15)
    {
        response = null;
        duration = 0;
        sense    = true;

        return -1;
    }

    /// <summary>Encapsulates a single MMC command to send in a queue</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal")]
    public class MmcSingleCommand
    {
        /// <summary>Command argument</summary>
        public uint argument;
        /// <summary>How many blocks to transfer</summary>
        public uint blocks;
        /// <summary>Size of block in bytes</summary>
        public uint blockSize;
        /// <summary>Buffer for MMC/SD command response</summary>
        public byte[] buffer;
        /// <summary>MMC/SD opcode</summary>
        public MmcCommands command;
        /// <summary>Flags indicating kind and place of response</summary>
        public MmcFlags flags;
        /// <summary><c>True</c> if command should be preceded with CMD55</summary>
        public bool isApplication;
        /// <summary>Response registers</summary>
        public uint[] response;
        /// <summary><c>True</c> if data is sent from host to card</summary>
        public bool write;
    }

    /// <summary>
    ///     Concatenates a queue of commands to be send to a remote SecureDigital or MultiMediaCard attached to an SDHCI
    ///     controller
    /// </summary>
    /// <param name="commands">List of commands</param>
    /// <param name="duration">Duration to execute all commands, in milliseconds</param>
    /// <param name="sense">Set to <c>true</c> if any of the commands returned an error status, <c>false</c> otherwise</param>
    /// <param name="timeout">Maximum allowed time to execute a single command</param>
    /// <returns>0 if no error occurred, otherwise, errno</returns>
    public virtual int SendMultipleMmcCommands(MmcSingleCommand[] commands, out double duration, out bool sense,
                                               uint timeout = 15)
    {
        duration = 0;
        sense    = true;

        return -1;
    }

    /// <summary>Closes then immediately reopens a device</summary>
    /// <returns>Returned error number if any</returns>
    public virtual bool ReOpen() => false;

    /// <summary>Reads data using operating system buffers.</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="offset">Offset in remote device to start reading, in bytes</param>
    /// <param name="length">Number of bytes to read</param>
    /// <param name="duration">Total time in milliseconds the reading took</param>
    /// <returns><c>true</c> if there was an error, <c>false</c> otherwise</returns>
    public virtual bool BufferedOsRead(out byte[] buffer, long offset, uint length, out double duration)
    {
        buffer   = null;
        duration = 0;

        return false;
    }
}