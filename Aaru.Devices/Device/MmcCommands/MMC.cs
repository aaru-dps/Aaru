// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SecureDigital and MultiMediaCard commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains MultiMediaCard commands.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Console;

// ReSharper disable UnusedMember.Global

namespace Aaru.Devices;

public partial class Device
{
    protected static bool ReadMultipleBlockCannotSetBlockCount;

    /// <summary>Reads the CSD register from a SecureDigital or MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadCsd(out byte[] buffer, out uint[] response, uint timeout, out double duration)
    {
        buffer = new byte[16];

        LastError = SendMmcCommand(MmcCommands.SendCsd,
                                   false,
                                   false,
                                   MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 | MmcFlags.CommandAc,
                                   0,
                                   16,
                                   1,
                                   ref buffer,
                                   out response,
                                   out duration,
                                   out bool sense,
                                   timeout);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME, Localization.SEND_CSD_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads the CID register from a SecureDigital or MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadCid(out byte[] buffer, out uint[] response, uint timeout, out double duration)
    {
        buffer = new byte[16];

        LastError = SendMmcCommand(MmcCommands.SendCid,
                                   false,
                                   false,
                                   MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 | MmcFlags.CommandAc,
                                   0,
                                   16,
                                   1,
                                   ref buffer,
                                   out response,
                                   out duration,
                                   out bool sense,
                                   timeout);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME, Localization.SEND_CID_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads the OCR register from a MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadOcr(out byte[] buffer, out uint[] response, uint timeout, out double duration)
    {
        buffer = new byte[4];

        LastError = SendMmcCommand(MmcCommands.SendOpCond,
                                   false,
                                   true,
                                   MmcFlags.ResponseSpiR3 | MmcFlags.ResponseR3 | MmcFlags.CommandBcr,
                                   0,
                                   4,
                                   1,
                                   ref buffer,
                                   out response,
                                   out duration,
                                   out bool sense,
                                   timeout);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME, Localization.SEND_OP_COND_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads the extended CSD from a MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadExtendedCsd(out byte[] buffer, out uint[] response, uint timeout, out double duration)
    {
        buffer = new byte[512];

        LastError = SendMmcCommand(MmcCommands.SendExtCsd,
                                   false,
                                   false,
                                   MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc,
                                   0,
                                   512,
                                   1,
                                   ref buffer,
                                   out response,
                                   out duration,
                                   out bool sense,
                                   timeout);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME, Localization.SEND_EXT_CSD_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sets the block length for transfers from a SecureDigital or MultiMediaCard device</summary>
    /// <param name="length">Block length in bytes</param>
    /// <param name="response">Response</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool SetBlockLength(uint length, out uint[] response, uint timeout, out double duration)
    {
        byte[] buffer = Array.Empty<byte>();

        LastError = SendMmcCommand(MmcCommands.SetBlocklen,
                                   false,
                                   false,
                                   MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAc,
                                   length,
                                   0,
                                   0,
                                   ref buffer,
                                   out response,
                                   out duration,
                                   out bool sense,
                                   timeout);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME, Localization.SET_BLOCKLEN_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads blocks from a SecureDigital or MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="lba">LBA to start reading from</param>
    /// <param name="blockSize">Block size in bytes</param>
    /// <param name="transferLength">Number of bytes to transfer</param>
    /// <param name="byteAddressed">Card is byte-addressed</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool Read(out byte[] buffer,        out uint[] response, uint lba, uint blockSize, ushort transferLength,
                     bool       byteAddressed, uint       timeout,  out double duration)
    {
        var sense = true;
        buffer   = null;
        response = null;
        duration = -1;

        if(transferLength <= 1)
            return ReadSingleBlock(out buffer, out response, lba, blockSize, byteAddressed, timeout, out duration);

        if(!ReadMultipleBlockCannotSetBlockCount)
        {
            sense = ReadMultipleBlock(out buffer,
                                      out response,
                                      lba,
                                      blockSize,
                                      transferLength,
                                      byteAddressed,
                                      timeout,
                                      out duration);
        }

        if(ReadMultipleBlockCannotSetBlockCount)
        {
            return ReadMultipleUsingSingle(out buffer,
                                           out response,
                                           lba,
                                           blockSize,
                                           transferLength,
                                           byteAddressed,
                                           timeout,
                                           out duration);
        }

        return sense;
    }

    /// <summary>Reads a single block from a SecureDigital or MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="lba">LBA to start reading from</param>
    /// <param name="blockSize">Block size in bytes</param>
    /// <param name="byteAddressed">Card is byte-addressed</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadSingleBlock(out byte[] buffer,  out uint[] response, uint lba, uint blockSize, bool byteAddressed,
                                uint       timeout, out double duration)
    {
        uint address;
        buffer   = new byte[blockSize];
        response = null;

        if(byteAddressed)
            address = lba * blockSize;
        else
            address = lba;

        LastError = SendMmcCommand(MmcCommands.ReadSingleBlock,
                                   false,
                                   false,
                                   MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc,
                                   address,
                                   blockSize,
                                   1,
                                   ref buffer,
                                   out response,
                                   out duration,
                                   out bool sense,
                                   timeout);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME, Localization.READ_SINGLE_BLOCK_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads multiple blocks from a SecureDigital or MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="lba">LBA to start reading from</param>
    /// <param name="blockSize">Block size in bytes</param>
    /// <param name="transferLength">Number of bytes to transfer</param>
    /// <param name="byteAddressed">Card is byte-addressed</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadMultipleBlock(out byte[] buffer,         out uint[] response, uint lba, uint blockSize,
                                  ushort     transferLength, bool byteAddressed, uint timeout, out double duration)
    {
        buffer = new byte[transferLength * blockSize];
        uint address;
        response = null;

        if(byteAddressed)
            address = lba * blockSize;
        else
            address = lba;

        LastError = SendMmcCommand(MmcCommands.ReadMultipleBlock,
                                   false,
                                   false,
                                   MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc,
                                   address,
                                   blockSize,
                                   transferLength,
                                   ref buffer,
                                   out response,
                                   out duration,
                                   out bool sense,
                                   timeout);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME,
                                   transferLength > 1
                                       ? Localization.READ_MULTIPLE_BLOCK_took_0_ms
                                       : Localization.READ_SINGLE_BLOCK_took_0_ms,
                                   duration);

        return sense;
    }

    /// <summary>Reads blocks using a single block read from a SecureDigital or MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="lba">LBA to start reading from</param>
    /// <param name="blockSize">Block size in bytes</param>
    /// <param name="transferLength">Number of bytes to transfer</param>
    /// <param name="byteAddressed">Card is byte-addressed</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadMultipleUsingSingle(out byte[] buffer, out uint[] response, uint lba, uint blockSize,
                                        ushort transferLength, bool byteAddressed, uint timeout, out double duration)
    {
        buffer = new byte[transferLength * blockSize];
        var blockBuffer = new byte[blockSize];
        duration = 0;
        var sense = true;
        response = null;

        for(uint i = 0; i < transferLength; i++)
        {
            uint address;

            if(byteAddressed)
                address = (lba + i) * blockSize;
            else
                address = lba + i;

            LastError = SendMmcCommand(MmcCommands.ReadSingleBlock,
                                       false,
                                       false,
                                       MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc,
                                       address,
                                       blockSize,
                                       1,
                                       ref blockBuffer,
                                       out response,
                                       out double blockDuration,
                                       out sense,
                                       timeout);

            Error = LastError != 0;

            duration += blockDuration;

            if(Error || sense) break;

            Array.Copy(blockBuffer, 0, buffer, i * blockSize, blockSize);
        }

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME, Localization.Multiple_READ_SINGLE_BLOCKs_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads status register from a MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadStatus(out byte[] buffer, out uint[] response, uint timeout, out double duration)
    {
        buffer = new byte[4];

        LastError = SendMmcCommand(MmcCommands.SendStatus,
                                   false,
                                   true,
                                   MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAc,
                                   0,
                                   4,
                                   1,
                                   ref buffer,
                                   out response,
                                   out duration,
                                   out bool sense,
                                   timeout);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME, Localization.SEND_STATUS_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads blocks with block count from a SecureDigital or MultiMediaCard device</summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="response">Response</param>
    /// <param name="lba">LBA to start reading from</param>
    /// <param name="blockSize">Block size in bytes</param>
    /// <param name="transferLength">Number of bytes to transfer</param>
    /// <param name="byteAddressed">Card is byte-addressed</param>
    /// <param name="timeout">Timeout to wait for command execution</param>
    /// <param name="duration">Time the device took to execute the command in milliseconds</param>
    /// <returns><c>true</c> if the device set an error condition, <c>false</c> otherwise</returns>
    public bool ReadWithBlockCount(out byte[] buffer,         out uint[] response, uint lba, uint blockSize,
                                   ushort     transferLength, bool byteAddressed, uint timeout, out double duration)
    {
        uint address  = byteAddressed ? lba * blockSize : lba;
        var  commands = new MmcSingleCommand[3];

        // SET_BLOCK_COUNT
        commands[0] = new MmcSingleCommand
        {
            command       = MmcCommands.SetBlockCount,
            write         = false,
            isApplication = false,
            flags         = MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAc,
            argument      = transferLength,
            blockSize     = 0,
            blocks        = 0,
            buffer        = Array.Empty<byte>()
        };

        // READ_MULTIPLE_BLOCK
        commands[1] = new MmcSingleCommand
        {
            command       = MmcCommands.ReadMultipleBlock,
            write         = false,
            isApplication = false,
            flags         = MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc,
            argument      = address,
            blockSize     = blockSize,
            blocks        = transferLength,
            buffer        = new byte[transferLength * blockSize]
        };

        // STOP_TRANSMISSION
        // Needed if the previous command fails
        commands[2] = new MmcSingleCommand
        {
            command       = MmcCommands.StopTransmission,
            write         = false,
            isApplication = false,
            flags         = MmcFlags.ResponseSpiR1B | MmcFlags.ResponseR1B | MmcFlags.CommandAc,
            argument      = 0,
            blockSize     = 0,
            blocks        = 0,
            buffer        = Array.Empty<byte>()
        };

        LastError = SendMultipleMmcCommands(commands, out duration, out bool sense, timeout);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(MMC_MODULE_NAME, Localization.READ_MULTIPLE_BLOCK_took_0_ms, duration);

        buffer   = commands[1].buffer;
        response = commands[1].response;

        return sense;
    }
}