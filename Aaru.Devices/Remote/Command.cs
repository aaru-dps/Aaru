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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aaru.Decoders.ATA;

namespace Aaru.Devices.Remote;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class Device
{
    /// <inheritdoc />
    public override int SendScsiCommand(byte[]        cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout,
                                        ScsiDirection direction, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        return _remote.SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, direction, out duration, out sense);
    }

    /// <inheritdoc />
    public override int SendAtaCommand(AtaRegistersChs registers, out AtaErrorRegistersChs errorRegisters,
                                       AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                       uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        return _remote.SendAtaCommand(registers, out errorRegisters, protocol, transferRegister, ref buffer, timeout,
                                      transferBlocks, out duration, out sense);
    }

    /// <inheritdoc />
    public override int SendAtaCommand(AtaRegistersLba28 registers, out AtaErrorRegistersLba28 errorRegisters,
                                       AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                       uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        return _remote.SendAtaCommand(registers, out errorRegisters, protocol, transferRegister, ref buffer, timeout,
                                      transferBlocks, out duration, out sense);
    }

    /// <inheritdoc />
    public override int SendAtaCommand(AtaRegistersLba48 registers, out AtaErrorRegistersLba48 errorRegisters,
                                       AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                       uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        return _remote.SendAtaCommand(registers, out errorRegisters, protocol, transferRegister, ref buffer, timeout,
                                      transferBlocks, out duration, out sense);
    }

    /// <inheritdoc />
    public override int SendMmcCommand(MmcCommands command,  bool       write,     bool isApplication, MmcFlags flags,
                                       uint        argument, uint       blockSize, uint blocks, ref byte[] buffer,
                                       out uint[]  response, out double duration,  out bool sense, uint timeout = 15)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        var cmdStopwatch = new Stopwatch();

        switch(command)
        {
            case MmcCommands.SendCid when _cachedCid != null:
            {
                cmdStopwatch.Restart();
                buffer = new byte[_cachedCid.Length];
                Array.Copy(_cachedCid, buffer, buffer.Length);
                response = new uint[4];
                sense    = false;
                cmdStopwatch.Stop();
                duration = cmdStopwatch.Elapsed.TotalMilliseconds;

                return 0;
            }
            case MmcCommands.SendCsd when _cachedCid != null:
            {
                cmdStopwatch.Restart();
                buffer = new byte[_cachedCsd.Length];
                Array.Copy(_cachedCsd, buffer, buffer.Length);
                response = new uint[4];
                sense    = false;
                cmdStopwatch.Stop();
                duration = cmdStopwatch.Elapsed.TotalMilliseconds;

                return 0;
            }
            case (MmcCommands)SecureDigitalCommands.SendScr when _cachedScr != null:
            {
                cmdStopwatch.Restart();
                buffer = new byte[_cachedScr.Length];
                Array.Copy(_cachedScr, buffer, buffer.Length);
                response = new uint[4];
                sense    = false;
                cmdStopwatch.Stop();
                duration = cmdStopwatch.Elapsed.TotalMilliseconds;

                return 0;
            }
            case (MmcCommands)SecureDigitalCommands.SendOperatingCondition when _cachedOcr != null:
            case MmcCommands.SendOpCond when _cachedOcr                                    != null:
            {
                cmdStopwatch.Restart();
                buffer = new byte[_cachedOcr.Length];
                Array.Copy(_cachedOcr, buffer, buffer.Length);
                response = new uint[4];
                sense    = false;
                cmdStopwatch.Stop();
                duration = cmdStopwatch.Elapsed.TotalMilliseconds;

                return 0;
            }
        }

        return _remote.SendMmcCommand(command, write, isApplication, flags, argument, blockSize, blocks, ref buffer,
                                      out response, out duration, out sense, timeout);
    }

    /// <inheritdoc />
    public override int SendMultipleMmcCommands(MmcSingleCommand[] commands, out double duration, out bool sense,
                                                uint               timeout = 15)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        if(_remote.ServerProtocolVersion >= 2)
            return _remote.SendMultipleMmcCommands(commands, out duration, out sense, timeout);

        var error = 0;
        duration = 0;
        sense    = false;

        foreach(MmcSingleCommand command in commands)
        {
            int singleError = _remote.SendMmcCommand(command.command, command.write, command.isApplication,
                                                     command.flags, command.argument, command.blockSize, command.blocks,
                                                     ref command.buffer, out command.response, out double cmdDuration,
                                                     out bool cmdSense, timeout);

            if(error == 0 && singleError != 0)
                error = singleError;

            duration += cmdDuration;

            if(cmdSense)
                sense = true;
        }

        return error;
    }

    /// <inheritdoc />
    public override bool ReOpen() => _remote.ReOpen();

    /// <inheritdoc />
    public override bool BufferedOsRead(out byte[] buffer, long offset, uint length, out double duration) =>
        _remote.BufferedOsRead(out buffer, offset, length, out duration);
}