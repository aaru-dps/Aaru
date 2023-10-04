// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Command.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Windows direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains a high level representation of the Windows syscalls used to
//     directly interface devices.
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
using System.Runtime.InteropServices;
using Aaru.Decoders.ATA;
using Microsoft.Win32.SafeHandles;

namespace Aaru.Devices.Windows;

[SuppressMessage("ReSharper", "UnusedParameter.Global")]
partial class Device
{
    /// <inheritdoc />
    public override int SendScsiCommand(byte[]        cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout,
                                        ScsiDirection direction, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        senseBuffer = null;
        duration    = 0;
        sense       = false;

        if(buffer == null)
            return -1;

        ScsiIoctlDirection dir = direction switch
                                 {
                                     ScsiDirection.In  => ScsiIoctlDirection.In,
                                     ScsiDirection.Out => ScsiIoctlDirection.Out,
                                     _                 => ScsiIoctlDirection.Unspecified
                                 };

        var sptdSb = new ScsiPassThroughDirectAndSenseBuffer
        {
            SenseBuf = new byte[32],
            sptd = new ScsiPassThroughDirect
            {
                Cdb                = new byte[16],
                CdbLength          = (byte)cdb.Length,
                SenseInfoLength    = 32,
                DataIn             = dir,
                DataTransferLength = (uint)buffer.Length,
                TimeOutValue       = timeout,
                DataBuffer         = Marshal.AllocHGlobal(buffer.Length)
            }
        };

        sptdSb.sptd.Length          = (ushort)Marshal.SizeOf(sptdSb.sptd);
        sptdSb.sptd.SenseInfoOffset = (uint)Marshal.SizeOf(sptdSb.sptd);
        Array.Copy(cdb, sptdSb.sptd.Cdb, cdb.Length);

        uint k     = 0;
        var  error = 0;

        Marshal.Copy(buffer, 0, sptdSb.sptd.DataBuffer, buffer.Length);

        var cmdStopwatch = new Stopwatch();
        cmdStopwatch.Start();

        bool hasError = !Extern.DeviceIoControlScsi(_fileHandle, WindowsIoctl.IoctlScsiPassThroughDirect, ref sptdSb,
                                                    (uint)Marshal.SizeOf(sptdSb), ref sptdSb,
                                                    (uint)Marshal.SizeOf(sptdSb), ref k, IntPtr.Zero);

        cmdStopwatch.Stop();

        if(hasError)
            error = Marshal.GetLastWin32Error();

        Marshal.Copy(sptdSb.sptd.DataBuffer, buffer, 0, buffer.Length);

        sense |= sptdSb.sptd.ScsiStatus != 0;

        senseBuffer = new byte[64];
        Array.Copy(sptdSb.SenseBuf, senseBuffer, 32);

        duration = cmdStopwatch.Elapsed.TotalMilliseconds;

        Marshal.FreeHGlobal(sptdSb.sptd.DataBuffer);

        return error;
    }

    /// <inheritdoc />
    public override int SendAtaCommand(AtaRegistersChs registers, out AtaErrorRegistersChs errorRegisters,
                                       AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                       uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        duration       = 0;
        sense          = false;
        errorRegisters = new AtaErrorRegistersChs();

        if(buffer == null)
            return -1;

        var aptd = new AtaPassThroughDirect
        {
            TimeOutValue       = timeout,
            DataBuffer         = Marshal.AllocHGlobal(buffer.Length),
            Length             = (ushort)Marshal.SizeOf(typeof(AtaPassThroughDirect)),
            DataTransferLength = (uint)buffer.Length,
            PreviousTaskFile   = new AtaTaskFile(),
            CurrentTaskFile = new AtaTaskFile
            {
                Command      = registers.Command,
                CylinderHigh = registers.CylinderHigh,
                CylinderLow  = registers.CylinderLow,
                DeviceHead   = registers.DeviceHead,
                Features     = registers.Feature,
                SectorCount  = registers.SectorCount,
                SectorNumber = registers.Sector
            }
        };

        switch(protocol)
        {
            case AtaProtocol.PioIn:
            case AtaProtocol.UDmaIn:
            case AtaProtocol.Dma:
                aptd.AtaFlags = AtaFlags.DataIn;

                break;
            case AtaProtocol.PioOut:
            case AtaProtocol.UDmaOut:
                aptd.AtaFlags = AtaFlags.DataOut;

                break;
        }

        switch(protocol)
        {
            case AtaProtocol.Dma:
            case AtaProtocol.DmaQueued:
            case AtaProtocol.FpDma:
            case AtaProtocol.UDmaIn:
            case AtaProtocol.UDmaOut:
                aptd.AtaFlags |= AtaFlags.Dma;

                break;
        }

        // Unknown if needed
        aptd.AtaFlags |= AtaFlags.DrdyRequired;

        uint k     = 0;
        var  error = 0;

        Marshal.Copy(buffer, 0, aptd.DataBuffer, buffer.Length);

        var cmdStopwatch = new Stopwatch();
        cmdStopwatch.Start();

        sense = !Extern.DeviceIoControlAta(_fileHandle, WindowsIoctl.IoctlAtaPassThroughDirect, ref aptd,
                                           (uint)Marshal.SizeOf(aptd), ref aptd, (uint)Marshal.SizeOf(aptd), ref k,
                                           IntPtr.Zero);

        cmdStopwatch.Stop();

        if(sense)
            error = Marshal.GetLastWin32Error();

        Marshal.Copy(aptd.DataBuffer, buffer, 0, buffer.Length);

        duration = cmdStopwatch.Elapsed.TotalMilliseconds;

        errorRegisters.CylinderHigh = aptd.CurrentTaskFile.CylinderHigh;
        errorRegisters.CylinderLow  = aptd.CurrentTaskFile.CylinderLow;
        errorRegisters.DeviceHead   = aptd.CurrentTaskFile.DeviceHead;
        errorRegisters.Error        = aptd.CurrentTaskFile.Error;
        errorRegisters.Sector       = aptd.CurrentTaskFile.SectorNumber;
        errorRegisters.SectorCount  = aptd.CurrentTaskFile.SectorCount;
        errorRegisters.Status       = aptd.CurrentTaskFile.Status;

        sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0;

        Marshal.FreeHGlobal(aptd.DataBuffer);

        return error;
    }

    /// <inheritdoc />
    public override int SendAtaCommand(AtaRegistersLba28 registers, out AtaErrorRegistersLba28 errorRegisters,
                                       AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                       uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        duration       = 0;
        sense          = false;
        errorRegisters = new AtaErrorRegistersLba28();

        if(buffer == null)
            return -1;

        var aptd = new AtaPassThroughDirect
        {
            TimeOutValue       = timeout,
            DataBuffer         = Marshal.AllocHGlobal(buffer.Length),
            Length             = (ushort)Marshal.SizeOf(typeof(AtaPassThroughDirect)),
            DataTransferLength = (uint)buffer.Length,
            PreviousTaskFile   = new AtaTaskFile(),
            CurrentTaskFile = new AtaTaskFile
            {
                Command      = registers.Command,
                CylinderHigh = registers.LbaHigh,
                CylinderLow  = registers.LbaMid,
                DeviceHead   = registers.DeviceHead,
                Features     = registers.Feature,
                SectorCount  = registers.SectorCount,
                SectorNumber = registers.LbaLow
            }
        };

        switch(protocol)
        {
            case AtaProtocol.PioIn:
            case AtaProtocol.UDmaIn:
            case AtaProtocol.Dma:
                aptd.AtaFlags = AtaFlags.DataIn;

                break;
            case AtaProtocol.PioOut:
            case AtaProtocol.UDmaOut:
                aptd.AtaFlags = AtaFlags.DataOut;

                break;
        }

        switch(protocol)
        {
            case AtaProtocol.Dma:
            case AtaProtocol.DmaQueued:
            case AtaProtocol.FpDma:
            case AtaProtocol.UDmaIn:
            case AtaProtocol.UDmaOut:
                aptd.AtaFlags |= AtaFlags.Dma;

                break;
        }

        // Unknown if needed
        aptd.AtaFlags |= AtaFlags.DrdyRequired;

        uint k     = 0;
        var  error = 0;

        Marshal.Copy(buffer, 0, aptd.DataBuffer, buffer.Length);

        var cmdStopwatch = new Stopwatch();
        cmdStopwatch.Start();

        sense = !Extern.DeviceIoControlAta(_fileHandle, WindowsIoctl.IoctlAtaPassThroughDirect, ref aptd,
                                           (uint)Marshal.SizeOf(aptd), ref aptd, (uint)Marshal.SizeOf(aptd), ref k,
                                           IntPtr.Zero);

        cmdStopwatch.Stop();

        if(sense)
            error = Marshal.GetLastWin32Error();

        Marshal.Copy(aptd.DataBuffer, buffer, 0, buffer.Length);

        duration = cmdStopwatch.Elapsed.TotalMilliseconds;

        errorRegisters.LbaHigh     = aptd.CurrentTaskFile.CylinderHigh;
        errorRegisters.LbaMid      = aptd.CurrentTaskFile.CylinderLow;
        errorRegisters.DeviceHead  = aptd.CurrentTaskFile.DeviceHead;
        errorRegisters.Error       = aptd.CurrentTaskFile.Error;
        errorRegisters.LbaLow      = aptd.CurrentTaskFile.SectorNumber;
        errorRegisters.SectorCount = aptd.CurrentTaskFile.SectorCount;
        errorRegisters.Status      = aptd.CurrentTaskFile.Status;

        sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0;

        Marshal.FreeHGlobal(aptd.DataBuffer);

        return error;
    }

    /// <inheritdoc />
    public override int SendAtaCommand(AtaRegistersLba48 registers, out AtaErrorRegistersLba48 errorRegisters,
                                       AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                       uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        duration       = 0;
        sense          = false;
        errorRegisters = new AtaErrorRegistersLba48();

        if(buffer == null)
            return -1;

        var aptd = new AtaPassThroughDirect
        {
            TimeOutValue       = timeout,
            DataBuffer         = Marshal.AllocHGlobal(buffer.Length),
            Length             = (ushort)Marshal.SizeOf(typeof(AtaPassThroughDirect)),
            DataTransferLength = (uint)buffer.Length,
            PreviousTaskFile = new AtaTaskFile
            {
                CylinderHigh = registers.LbaHighPrevious,
                CylinderLow  = registers.LbaMidPrevious,
                Features     = (byte)((registers.Feature     & 0xFF00) >> 8),
                SectorCount  = (byte)((registers.SectorCount & 0xFF00) >> 8),
                SectorNumber = registers.LbaLowPrevious
            },
            CurrentTaskFile = new AtaTaskFile
            {
                Command      = registers.Command,
                CylinderHigh = registers.LbaHighCurrent,
                CylinderLow  = registers.LbaMidCurrent,
                DeviceHead   = registers.DeviceHead,
                Features     = (byte)(registers.Feature     & 0xFF),
                SectorCount  = (byte)(registers.SectorCount & 0xFF),
                SectorNumber = registers.LbaLowCurrent
            }
        };

        switch(protocol)
        {
            case AtaProtocol.PioIn:
            case AtaProtocol.UDmaIn:
            case AtaProtocol.Dma:
                aptd.AtaFlags = AtaFlags.DataIn;

                break;
            case AtaProtocol.PioOut:
            case AtaProtocol.UDmaOut:
                aptd.AtaFlags = AtaFlags.DataOut;

                break;
        }

        switch(protocol)
        {
            case AtaProtocol.Dma:
            case AtaProtocol.DmaQueued:
            case AtaProtocol.FpDma:
            case AtaProtocol.UDmaIn:
            case AtaProtocol.UDmaOut:
                aptd.AtaFlags |= AtaFlags.Dma;

                break;
        }

        aptd.AtaFlags |= AtaFlags.ExtendedCommand;

        // Unknown if needed
        aptd.AtaFlags |= AtaFlags.DrdyRequired;

        uint k     = 0;
        var  error = 0;

        Marshal.Copy(buffer, 0, aptd.DataBuffer, buffer.Length);

        var cmdStopwatch = new Stopwatch();
        cmdStopwatch.Start();

        sense = !Extern.DeviceIoControlAta(_fileHandle, WindowsIoctl.IoctlAtaPassThroughDirect, ref aptd,
                                           (uint)Marshal.SizeOf(aptd), ref aptd, (uint)Marshal.SizeOf(aptd), ref k,
                                           IntPtr.Zero);

        cmdStopwatch.Stop();

        if(sense)
            error = Marshal.GetLastWin32Error();

        Marshal.Copy(aptd.DataBuffer, buffer, 0, buffer.Length);

        duration = cmdStopwatch.Elapsed.TotalMilliseconds;

        errorRegisters.SectorCount = (ushort)((aptd.PreviousTaskFile.SectorCount << 8) +
                                              aptd.CurrentTaskFile.SectorCount);

        errorRegisters.LbaLowPrevious  = aptd.PreviousTaskFile.SectorNumber;
        errorRegisters.LbaMidPrevious  = aptd.PreviousTaskFile.CylinderLow;
        errorRegisters.LbaHighPrevious = aptd.PreviousTaskFile.CylinderHigh;
        errorRegisters.LbaLowCurrent   = aptd.CurrentTaskFile.SectorNumber;
        errorRegisters.LbaMidCurrent   = aptd.CurrentTaskFile.CylinderLow;
        errorRegisters.LbaHighCurrent  = aptd.CurrentTaskFile.CylinderHigh;
        errorRegisters.DeviceHead      = aptd.CurrentTaskFile.DeviceHead;
        errorRegisters.Error           = aptd.CurrentTaskFile.Error;
        errorRegisters.Status          = aptd.CurrentTaskFile.Status;

        sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0;

        Marshal.FreeHGlobal(aptd.DataBuffer);

        return error;
    }

    /// <summary>Returns true if the specified handle is controlled by a SFFDISK (aka SDHCI) driver</summary>
    /// <param name="fd">Device handle</param>
    /// <returns><c>true</c> if SDHCI, false otherwise</returns>
    internal static bool IsSdhci(SafeFileHandle fd)
    {
        var queryData1 = new SffdiskQueryDeviceProtocolData();
        queryData1.size = (ushort)Marshal.SizeOf(queryData1);

        Extern.DeviceIoControl(fd, WindowsIoctl.IoctlSffdiskQueryDeviceProtocol, IntPtr.Zero, 0, ref queryData1,
                               queryData1.size, out _, IntPtr.Zero);

        return queryData1.protocolGuid.Equals(Consts.GuidSffProtocolSd) ||
               queryData1.protocolGuid.Equals(Consts.GuidSffProtocolMmc);
    }

    /// <inheritdoc />
    public override int SendMmcCommand(MmcCommands command,  bool       write,     bool isApplication, MmcFlags flags,
                                       uint        argument, uint       blockSize, uint blocks, ref byte[] buffer,
                                       out uint[]  response, out double duration,  out bool sense, uint timeout = 15)
    {
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

        var commandData       = new SffdiskDeviceCommandData();
        var commandDescriptor = new SdCmdDescriptor();
        commandData.size                    = (ushort)Marshal.SizeOf(commandData);
        commandData.command                 = SffdiskDcmd.DeviceCommand;
        commandData.protocolArgumentSize    = (ushort)Marshal.SizeOf(commandDescriptor);
        commandData.deviceDataBufferSize    = blockSize * blocks;
        commandDescriptor.commandCode       = (byte)command;
        commandDescriptor.cmdClass          = isApplication ? SdCommandClass.AppCmd : SdCommandClass.Standard;
        commandDescriptor.transferDirection = write ? SdTransferDirection.Write : SdTransferDirection.Read;

        commandDescriptor.transferType = flags.HasFlag(MmcFlags.CommandAdtc)
                                             ? command == MmcCommands.ReadMultipleBlock
                                                   ? SdTransferType.MultiBlock
                                                   : SdTransferType.SingleBlock
                                             : SdTransferType.CmdOnly;

        commandDescriptor.responseType = 0;

        if(flags.HasFlag(MmcFlags.ResponseR1) || flags.HasFlag(MmcFlags.ResponseSpiR1))
            commandDescriptor.responseType = SdResponseType.R1;

        if(flags.HasFlag(MmcFlags.ResponseR1B) || flags.HasFlag(MmcFlags.ResponseSpiR1B))
            commandDescriptor.responseType = SdResponseType.R1b;

        if(flags.HasFlag(MmcFlags.ResponseR2) || flags.HasFlag(MmcFlags.ResponseSpiR2))
            commandDescriptor.responseType = SdResponseType.R2;

        if(flags.HasFlag(MmcFlags.ResponseR3) || flags.HasFlag(MmcFlags.ResponseSpiR3))
            commandDescriptor.responseType = SdResponseType.R3;

        if(flags.HasFlag(MmcFlags.ResponseR4) || flags.HasFlag(MmcFlags.ResponseSpiR4))
            commandDescriptor.responseType = SdResponseType.R4;

        if(flags.HasFlag(MmcFlags.ResponseR5) || flags.HasFlag(MmcFlags.ResponseSpiR5))
            commandDescriptor.responseType = SdResponseType.R5;

        if(flags.HasFlag(MmcFlags.ResponseR6))
            commandDescriptor.responseType = SdResponseType.R6;

        var commandB = new byte[commandData.size + commandData.protocolArgumentSize + commandData.deviceDataBufferSize];

        Array.Copy(buffer, 0, commandB, commandData.size + commandData.protocolArgumentSize, buffer.Length);
        IntPtr hBuf = Marshal.AllocHGlobal(commandB.Length);
        Marshal.StructureToPtr(commandData, hBuf, true);
        var descriptorOffset = IntPtr.Add(hBuf, commandData.size);
        Marshal.StructureToPtr(commandDescriptor, descriptorOffset, true);
        Marshal.Copy(hBuf, commandB, 0, commandB.Length);
        Marshal.FreeHGlobal(hBuf);

        var error = 0;
        cmdStopwatch.Restart();

        sense = !Extern.DeviceIoControl(_fileHandle, WindowsIoctl.IoctlSffdiskDeviceCommand, commandB,
                                        (uint)commandB.Length, commandB, (uint)commandB.Length, out _, IntPtr.Zero);

        cmdStopwatch.Stop();

        if(sense)
            error = Marshal.GetLastWin32Error();

        buffer = new byte[blockSize * blocks];
        Buffer.BlockCopy(commandB, commandB.Length - buffer.Length, buffer, 0, buffer.Length);

        response = new uint[4];
        duration = cmdStopwatch.Elapsed.TotalMilliseconds;

        return error;
    }

    /// <inheritdoc />
    public override int SendMultipleMmcCommands(MmcSingleCommand[] commands, out double duration, out bool sense,
                                                uint               timeout = 15)
    {
        // We need a timeout
        if(timeout == 0)
            timeout = Timeout > 0 ? Timeout : 15;

        var error = 0;
        duration = 0;
        sense    = false;

        if(commands.Length     == 3                             &&
           commands[0].command == MmcCommands.SetBlocklen       &&
           commands[1].command == MmcCommands.ReadMultipleBlock &&
           commands[2].command == MmcCommands.StopTransmission)
        {
            return SendMmcCommand(commands[1].command, commands[1].write, commands[1].isApplication, commands[1].flags,
                                  commands[1].argument, commands[1].blockSize, commands[1].blocks,
                                  ref commands[1].buffer, out commands[1].response, out duration, out sense, timeout);
        }

        foreach(MmcSingleCommand command in commands)
        {
            int singleError = SendMmcCommand(command.command, command.write, command.isApplication, command.flags,
                                             command.argument, command.blockSize, command.blocks, ref command.buffer,
                                             out command.response, out double cmdDuration, out bool cmdSense, timeout);

            if(error == 0 && singleError != 0)
                error = singleError;

            duration += cmdDuration;

            if(cmdSense)
                sense = true;
        }

        return error;
    }

    /// <inheritdoc />
    public override bool ReOpen()
    {
        Extern.CloseHandle(_fileHandle);

        SafeFileHandle newFd = Extern.CreateFile(_devicePath, FileAccess.GenericRead | FileAccess.GenericWrite,
                                                 FileShare.Read | FileShare.Write, IntPtr.Zero, FileMode.OpenExisting,
                                                 FileAttributes.Normal, IntPtr.Zero);

        if(newFd.IsInvalid)
        {
            LastError = Marshal.GetLastWin32Error();
            Error     = true;

            return true;
        }

        _fileHandle = newFd;

        return false;
    }

    /// <inheritdoc />
    public override bool BufferedOsRead(out byte[] buffer, long offset, uint length, out double duration)
    {
        buffer = new byte[length];

        var cmdStopwatch = new Stopwatch();
        cmdStopwatch.Start();

        bool sense = !Extern.SetFilePointerEx(_fileHandle, offset, out _, MoveMethod.Begin);

        cmdStopwatch.Stop();

        if(sense)
        {
            cmdStopwatch.Stop();
            duration = cmdStopwatch.Elapsed.TotalMilliseconds;

            LastError = Marshal.GetLastWin32Error();
            Error     = true;

            return true;
        }

        sense = !Extern.ReadFile(_fileHandle, buffer, length, out _, IntPtr.Zero);

        cmdStopwatch.Stop();
        duration = cmdStopwatch.Elapsed.TotalMilliseconds;

        if(sense)
        {
            Error     = true;
            LastError = Marshal.GetLastWin32Error();

            return true;
        }

        Error = false;

        return false;
    }
}