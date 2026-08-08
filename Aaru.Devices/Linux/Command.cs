// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Command.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains a high level representation of the Linux syscalls used to
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Interop;
using Aaru.Decoders.ATA;

namespace Aaru.Devices.Linux;

[SuppressMessage("ReSharper",
                 "ClassCannotBeInstantiated",
                 Justification = "Instantiated through the platform-specific factory when running on Linux.")]
partial class Device
{
    /// <inheritdoc />
    public override unsafe int SendScsiCommand(Span<byte> cdb, ref byte[] buffer, uint timeout, ScsiDirection direction,
                                               out double duration, out bool sense)
    {
        // Defaults
        if(timeout == 0) timeout = Timeout > 0 ? Timeout : 15;
        duration = 0;
        sense    = false;
        SenseBuffer.Clear();

        if(buffer == null) return -1;

        int len = buffer.Length;
        int rc;

        if(len == 0)
        {
            // Zero-length command: no data buffer
            var ioHdr0 = new SgIoHdrT
            {
                interface_id    = 'S',
                cmd_len         = (byte)cdb.Length,
                mx_sb_len       = (byte)SenseBuffer.Length,
                dxfer_direction = ScsiIoctlDirection.None,
                dxfer_len       = 0,
                dxferp          = IntPtr.Zero,
                cmdp            = (IntPtr)CdbPtr,
                sbp             = (IntPtr)SensePtr,
                timeout         = timeout * 1000,
                flags           = 0
            };

            // Load CDB
            cdb.CopyTo(new Span<byte>(CdbPtr, cdb.Length));

            long t0 = Stopwatch.GetTimestamp();
            rc = Extern.ioctlSg(_fileDescriptor, LinuxIoctl.SgIo, ref ioHdr0);
            if(rc < 0) rc = Marshal.GetLastWin32Error();
            sense |= (ioHdr0.info & SgInfo.OkMask) != SgInfo.Ok;

            duration = ioHdr0.duration > 0
                           ? ioHdr0.duration
                           : (Stopwatch.GetTimestamp() - t0) * 1000.0 / Stopwatch.Frequency;

            return rc;
        }

        ScsiIoctlDirection dir = direction switch
                                 {
                                     ScsiDirection.In            => ScsiIoctlDirection.In,
                                     ScsiDirection.Out           => ScsiIoctlDirection.Out,
                                     ScsiDirection.Bidirectional => ScsiIoctlDirection.Unspecified,
                                     ScsiDirection.None          => ScsiIoctlDirection.None,
                                     _                           => ScsiIoctlDirection.Unknown
                                 };

        // Prepare sg_io_hdr
        var ioHdr = new SgIoHdrT
        {
            interface_id    = 'S',
            cmd_len         = (byte)cdb.Length,
            mx_sb_len       = (byte)SenseBuffer.Length,
            dxfer_direction = dir,
            dxfer_len       = (uint)len,
            cmdp            = (IntPtr)CdbPtr,
            sbp             = (IntPtr)SensePtr,
            timeout         = timeout * 1000,

            // Direct I/O can help for larger transfers; benchmark threshold for your device
            flags = (uint)(len >= 4096 ? SgFlags.DirectIo : 0)
        };

        // Load CDB into pinned CDB buffer
        cdb.CopyTo(new Span<byte>(CdbPtr, cdb.Length));

        long tStart = Stopwatch.GetTimestamp();

        if(direction == ScsiDirection.Out)
        {
            // Zero-copy OUT: pin and pass managed buffer directly
            fixed(byte* pBuf = buffer)
            {
                ioHdr.dxferp = (IntPtr)pBuf;
                rc           = Extern.ioctlSg(_fileDescriptor, LinuxIoctl.SgIo, ref ioHdr);
            }
        }
        else
        {
            // IN or BiDir → use pooled, aligned native buffer
            EnsureCapacityAligned((nuint)len);
            ioHdr.dxferp = (IntPtr)_nativeBuffer;

            var nativeSpan = new Span<byte>((void*)_nativeBuffer, len);

            // BiDir prefill (protocol dependent); IN has nothing to send
            if(direction == ScsiDirection.Bidirectional) buffer.AsSpan().CopyTo(nativeSpan);

            rc = Extern.ioctlSg(_fileDescriptor, LinuxIoctl.SgIo, ref ioHdr);

            // Copy back only the bytes actually transferred if resid is sane
            int bytesToCopy                                        = len;
            if(ioHdr.resid >= 0 && ioHdr.resid <= len) bytesToCopy = len - ioHdr.resid;

            if(bytesToCopy > 0) nativeSpan[..bytesToCopy].CopyTo(buffer);
        }

        if(rc < 0) rc = Marshal.GetLastWin32Error();

        sense |= (ioHdr.info & SgInfo.OkMask) != SgInfo.Ok;

        duration = ioHdr.duration > 0
                       ? ioHdr.duration
                       : (Stopwatch.GetTimestamp() - tStart) * 1000.0 / Stopwatch.Frequency;

        return rc;
    }


    /// <summary>Converts ATA protocol to SG_IO direction</summary>
    /// <param name="protocol">ATA protocol</param>
    /// <returns>SG_IO direction</returns>
    static ScsiDirection AtaProtocolToScsiDirection(AtaProtocol protocol)
    {
        return protocol switch
               {
                   AtaProtocol.DeviceDiagnostic
                    or AtaProtocol.DeviceReset
                    or AtaProtocol.HardReset
                    or AtaProtocol.NonData
                    or AtaProtocol.SoftReset
                    or AtaProtocol.ReturnResponse => ScsiDirection.None,
                   AtaProtocol.PioIn or AtaProtocol.UDmaIn   => ScsiDirection.In,
                   AtaProtocol.PioOut or AtaProtocol.UDmaOut => ScsiDirection.Out,
                   _                                         => ScsiDirection.Unspecified
               };
    }

    /// <inheritdoc />
    public override int SendAtaCommand(AtaRegistersChs registers, out AtaErrorRegistersChs errorRegisters,
                                       AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                       uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0) timeout = Timeout > 0 ? Timeout : 15;

        duration       = 0;
        sense          = false;
        errorRegisters = new AtaErrorRegistersChs();

        if(buffer == null) return -1;

        var cdb = new byte[16];
        cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
        cdb[1] = (byte)((byte)protocol << 1 & 0x1E);

        if(transferRegister != AtaTransferRegister.NoTransfer && protocol != AtaProtocol.NonData)
        {
            cdb[2] = protocol switch
                     {
                         AtaProtocol.PioIn or AtaProtocol.UDmaIn => 0x08,
                         _                                       => 0x00
                     };

            if(transferBlocks) cdb[2] |= 0x04;

            cdb[2] |= (byte)((int)transferRegister & 0x03);
        }

        //cdb[2] |= 0x20;

        cdb[4]  = registers.Feature;
        cdb[6]  = registers.SectorCount;
        cdb[8]  = registers.Sector;
        cdb[10] = registers.CylinderLow;
        cdb[12] = registers.CylinderHigh;
        cdb[13] = registers.DeviceHead;
        cdb[14] = registers.Command;

        int error = SendScsiCommand(cdb,
                                    ref buffer,
                                    timeout,
                                    AtaProtocolToScsiDirection(protocol),
                                    out duration,
                                    out sense);

        if(SenseBuffer.Length < 22 || SenseBuffer[8] != 0x09 || SenseBuffer[9] != 0x0C) return error;

        errorRegisters.Error = SenseBuffer[11];

        errorRegisters.SectorCount  = SenseBuffer[13];
        errorRegisters.Sector       = SenseBuffer[15];
        errorRegisters.CylinderLow  = SenseBuffer[17];
        errorRegisters.CylinderHigh = SenseBuffer[19];
        errorRegisters.DeviceHead   = SenseBuffer[20];
        errorRegisters.Status       = SenseBuffer[21];

        sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0;

        return error;
    }

    /// <inheritdoc />
    public override int SendAtaCommand(AtaRegistersLba28 registers, out AtaErrorRegistersLba28 errorRegisters,
                                       AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                       uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0) timeout = Timeout > 0 ? Timeout : 15;

        duration       = 0;
        sense          = false;
        errorRegisters = new AtaErrorRegistersLba28();

        if(buffer == null) return -1;

        var cdb = new byte[16];
        cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
        cdb[1] = (byte)((byte)protocol << 1 & 0x1E);

        if(transferRegister != AtaTransferRegister.NoTransfer && protocol != AtaProtocol.NonData)
        {
            cdb[2] = protocol switch
                     {
                         AtaProtocol.PioIn or AtaProtocol.UDmaIn => 0x08,
                         _                                       => 0x00
                     };

            if(transferBlocks) cdb[2] |= 0x04;

            cdb[2] |= (byte)((int)transferRegister & 0x03);
        }

        cdb[2] |= 0x20;

        cdb[4]  = registers.Feature;
        cdb[6]  = registers.SectorCount;
        cdb[8]  = registers.LbaLow;
        cdb[10] = registers.LbaMid;
        cdb[12] = registers.LbaHigh;
        cdb[13] = registers.DeviceHead;
        cdb[14] = registers.Command;

        int error = SendScsiCommand(cdb,
                                    ref buffer,
                                    timeout,
                                    AtaProtocolToScsiDirection(protocol),
                                    out duration,
                                    out sense);

        if(SenseBuffer.Length < 22 || SenseBuffer[8] != 0x09 || SenseBuffer[9] != 0x0C) return error;

        errorRegisters.Error = SenseBuffer[11];

        errorRegisters.SectorCount = SenseBuffer[13];
        errorRegisters.LbaLow      = SenseBuffer[15];
        errorRegisters.LbaMid      = SenseBuffer[17];
        errorRegisters.LbaHigh     = SenseBuffer[19];
        errorRegisters.DeviceHead  = SenseBuffer[20];
        errorRegisters.Status      = SenseBuffer[21];

        sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0;

        return error;
    }

    /// <inheritdoc />
    public override int SendAtaCommand(AtaRegistersLba48 registers, out AtaErrorRegistersLba48 errorRegisters,
                                       AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                       uint timeout, bool transferBlocks, out double duration, out bool sense)
    {
        // We need a timeout
        if(timeout == 0) timeout = Timeout > 0 ? Timeout : 15;

        duration       = 0;
        sense          = false;
        errorRegisters = new AtaErrorRegistersLba48();

        if(buffer == null) return -1;

        var cdb = new byte[16];
        cdb[0] =  (byte)ScsiCommands.AtaPassThrough16;
        cdb[1] =  (byte)((byte)protocol << 1 & 0x1E);
        cdb[1] |= 0x01;

        if(transferRegister != AtaTransferRegister.NoTransfer && protocol != AtaProtocol.NonData)
        {
            cdb[2] = protocol switch
                     {
                         AtaProtocol.PioIn or AtaProtocol.UDmaIn => 0x08,
                         _                                       => 0x00
                     };

            if(transferBlocks) cdb[2] |= 0x04;

            cdb[2] |= (byte)((int)transferRegister & 0x03);
        }

        cdb[2] |= 0x20;

        cdb[3]  = (byte)((registers.Feature & 0xFF00) >> 8);
        cdb[4]  = (byte)(registers.Feature & 0xFF);
        cdb[5]  = (byte)((registers.SectorCount & 0xFF00) >> 8);
        cdb[6]  = (byte)(registers.SectorCount & 0xFF);
        cdb[7]  = registers.LbaLowPrevious;
        cdb[8]  = registers.LbaLowCurrent;
        cdb[9]  = registers.LbaMidPrevious;
        cdb[10] = registers.LbaMidCurrent;
        cdb[11] = registers.LbaHighPrevious;
        cdb[12] = registers.LbaHighCurrent;
        cdb[13] = registers.DeviceHead;
        cdb[14] = registers.Command;

        int error = SendScsiCommand(cdb,
                                    ref buffer,
                                    timeout,
                                    AtaProtocolToScsiDirection(protocol),
                                    out duration,
                                    out sense);

        if(SenseBuffer.Length < 22 || SenseBuffer[8] != 0x09 || SenseBuffer[9] != 0x0C) return error;

        errorRegisters.Error = SenseBuffer[11];

        errorRegisters.SectorCount     = (ushort)((SenseBuffer[12] << 8) + SenseBuffer[13]);
        errorRegisters.LbaLowPrevious  = SenseBuffer[14];
        errorRegisters.LbaLowCurrent   = SenseBuffer[15];
        errorRegisters.LbaMidPrevious  = SenseBuffer[16];
        errorRegisters.LbaMidCurrent   = SenseBuffer[17];
        errorRegisters.LbaHighPrevious = SenseBuffer[18];
        errorRegisters.LbaHighCurrent  = SenseBuffer[19];
        errorRegisters.DeviceHead      = SenseBuffer[20];
        errorRegisters.Status          = SenseBuffer[21];

        sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0;

        sense |= error != 0;

        return error;
    }

    /// <inheritdoc />
    public override int SendMmcCommand(MmcCommands command,  bool       write,     bool isApplication, MmcFlags flags,
                                       uint        argument, uint       blockSize, uint blocks, ref byte[] buffer,
                                       out uint[]  response, out double duration,  out bool sense, uint timeout = 15)
    {
        // We need a timeout
        if(timeout == 0) timeout = Timeout > 0 ? Timeout : 15;

        var cmdStopwatch = new Stopwatch();

        switch(command)
        {
            case MmcCommands.SendCid when CachedCid != null:
            {
                cmdStopwatch.Restart();
                buffer = new byte[CachedCid.Length];
                Array.Copy(CachedCid, buffer, buffer.Length);
                response = new uint[4];
                sense    = false;
                cmdStopwatch.Stop();
                duration = cmdStopwatch.Elapsed.TotalMilliseconds;

                return 0;
            }
            case MmcCommands.SendCsd when CachedCsd != null:
            {
                cmdStopwatch.Restart();
                buffer = new byte[CachedCsd.Length];
                Array.Copy(CachedCsd, buffer, buffer.Length);
                response = new uint[4];
                sense    = false;
                cmdStopwatch.Stop();
                duration = cmdStopwatch.Elapsed.TotalMilliseconds;

                return 0;
            }
            case (MmcCommands)SecureDigitalCommands.SendScr when CachedScr != null:
            {
                cmdStopwatch.Restart();
                buffer = new byte[CachedScr.Length];
                Array.Copy(CachedScr, buffer, buffer.Length);
                response = new uint[4];
                sense    = false;
                cmdStopwatch.Stop();
                duration = cmdStopwatch.Elapsed.TotalMilliseconds;

                return 0;
            }
            case (MmcCommands)SecureDigitalCommands.SendOperatingCondition when CachedOcr != null:
            case MmcCommands.SendOpCond when CachedOcr                                    != null:
            {
                cmdStopwatch.Restart();
                buffer = new byte[CachedOcr.Length];
                Array.Copy(CachedOcr, buffer, buffer.Length);
                response = new uint[4];
                sense    = false;
                cmdStopwatch.Stop();
                duration = cmdStopwatch.Elapsed.TotalMilliseconds;

                return 0;
            }
        }

        response = null;
        duration = 0;
        sense    = false;

        if(buffer == null) return -1;

        var ioCmd = new MmcIocCmd();

        nint bufPtr = Marshal.AllocHGlobal(buffer.Length);

        ioCmd.write_flag = write;
        ioCmd.is_ascmd   = isApplication;
        ioCmd.opcode     = (uint)command;
        ioCmd.arg        = argument;
        ioCmd.flags      = flags;
        ioCmd.blksz      = blockSize;
        ioCmd.blocks     = blocks;

        if(timeout > 0)
        {
            // Both fields are 32-bit in the kernel ABI, so clamp instead of overflowing
            ioCmd.data_timeout_ns = (uint)Math.Min(timeout * 1000000000UL, uint.MaxValue);
            ioCmd.cmd_timeout_ms  = (uint)Math.Min(timeout * 1000UL,       uint.MaxValue);
        }

        ioCmd.data_ptr = (ulong)bufPtr;

        Marshal.Copy(buffer, 0, bufPtr, buffer.Length);

        var stopWatch = new Stopwatch();
        stopWatch.Restart();
        int error = Extern.ioctlMmc(_fileDescriptor, LinuxIoctl.MmcIocCmd, ref ioCmd);
        stopWatch.Stop();

        sense |= error < 0;

        if(error < 0) error = Marshal.GetLastWin32Error();

        Marshal.Copy(bufPtr, buffer, 0, buffer.Length);

        response = ioCmd.response;
        duration = stopWatch.Elapsed.TotalMilliseconds;

        Marshal.FreeHGlobal(bufPtr);

        return error;
    }

    /// <inheritdoc />
    public override int SendMultipleMmcCommands(MmcSingleCommand[] commands, out double duration, out bool sense,
                                                uint               timeout = 15)
    {
        // We need a timeout
        if(timeout == 0) timeout = Timeout > 0 ? Timeout : 15;

        duration = 0;
        sense    = false;

        // Create array for buffers
        var bufferPointers = new nint[commands.Length];

        // Allocate memory for the array for commands
        var ioMultiCmd = new byte[sizeof(ulong) + Marshal.SizeOf<MmcIocCmd>() * commands.Length];

        // First value of array is uint64 with count of commands
        Array.Copy(BitConverter.GetBytes((ulong)commands.Length), 0, ioMultiCmd, 0, sizeof(ulong));

        int off = sizeof(ulong);

        for(var i = 0; i < commands.Length; i++)
        {
            // Create command
            var ioCmd = new MmcIocCmd();

            // Allocate buffer
            bufferPointers[i] = Marshal.AllocHGlobal(commands[i].buffer.Length);

            // Define command
            ioCmd.write_flag = commands[i].write;
            ioCmd.is_ascmd   = commands[i].isApplication;
            ioCmd.opcode     = (uint)commands[i].command;
            ioCmd.arg        = commands[i].argument;
            ioCmd.flags      = commands[i].flags;
            ioCmd.blksz      = commands[i].blockSize;
            ioCmd.blocks     = commands[i].blocks;

            if(timeout > 0)
            {
                // Both fields are 32-bit in the kernel ABI, so clamp instead of overflowing
                ioCmd.data_timeout_ns = (uint)Math.Min(timeout * 1000000000UL, uint.MaxValue);
                ioCmd.cmd_timeout_ms  = (uint)Math.Min(timeout * 1000UL,       uint.MaxValue);
            }

            ioCmd.data_ptr = (ulong)bufferPointers[i];

            // Copy buffer to unmanaged space
            Marshal.Copy(commands[i].buffer, 0, bufferPointers[i], commands[i].buffer.Length);

            // Copy command to array
            byte[] ioCmdBytes = Helpers.Marshal.StructureToByteArrayLittleEndian(ioCmd);
            Array.Copy(ioCmdBytes, 0, ioMultiCmd, off, Marshal.SizeOf<MmcIocCmd>());

            // Advance pointer
            off += Marshal.SizeOf<MmcIocCmd>();
        }

        // Allocate unmanaged memory for array of commands
        nint ioMultiCmdPtr = Marshal.AllocHGlobal(ioMultiCmd.Length);

        // Copy array of commands to unmanaged memory
        Marshal.Copy(ioMultiCmd, 0, ioMultiCmdPtr, ioMultiCmd.Length);

        // Send command
        var cmdStopwatch = new Stopwatch();
        cmdStopwatch.Start();
        int error = Extern.ioctlMmcMulti(_fileDescriptor, LinuxIoctl.MmcIocMultiCmd, ioMultiCmdPtr);
        cmdStopwatch.Stop();

        sense |= error < 0;

        if(error < 0) error = Marshal.GetLastWin32Error();

        duration = cmdStopwatch.Elapsed.TotalMilliseconds;

        off = sizeof(ulong);

        // Copy array from unmanaged memory
        Marshal.Copy(ioMultiCmdPtr, ioMultiCmd, 0, ioMultiCmd.Length);

        // TODO: Use real pointers this is too slow
        for(var i = 0; i < commands.Length; i++)
        {
            var tmp = new byte[Marshal.SizeOf<MmcIocCmd>()];

            // Copy command to managed space
            Array.Copy(ioMultiCmd, off, tmp, 0, tmp.Length);
            MmcIocCmd command = Helpers.Marshal.ByteArrayToStructureLittleEndian<MmcIocCmd>(tmp);

            // Copy response
            commands[i].response = command.response;

            // Copy buffer to managed space
            Marshal.Copy(bufferPointers[i], commands[i].buffer, 0, commands[i].buffer.Length);

            // Free buffer
            Marshal.FreeHGlobal(bufferPointers[i]);

            // Advance pointer
            off += Marshal.SizeOf<MmcIocCmd>();
        }

        // Free unmanaged memory
        Marshal.FreeHGlobal(ioMultiCmdPtr);

        return error;
    }

    /// <inheritdoc />
    public override bool ReOpen()

    {
        int ret = Extern.close(_fileDescriptor);

        if(ret < 0)
        {
            LastError = Marshal.GetLastWin32Error();
            Error     = true;

            return true;
        }

        int newFd = Extern.open(DevicePath, FileFlags.ReadWrite | FileFlags.NonBlocking | FileFlags.CreateNew);

        if(newFd >= 0)
        {
            Error           = false;
            _fileDescriptor = newFd;

            return false;
        }

        int error = Marshal.GetLastWin32Error();

        if(error != 13 && error != 30)
        {
            LastError = Marshal.GetLastWin32Error();
            Error     = true;

            return true;
        }

        newFd = Extern.open(DevicePath, FileFlags.Readonly | FileFlags.NonBlocking);

        if(newFd < 0)
        {
            LastError = Marshal.GetLastWin32Error();
            Error     = true;

            return true;
        }

        Error           = false;
        _fileDescriptor = newFd;

        return false;
    }

    /// <summary>Reads the contents of a symbolic link</summary>
    /// <param name="path">Path to the symbolic link</param>
    /// <returns>Contents of the symbolic link</returns>
    static string ReadLink(string path)
    {
        nint buf = Marshal.AllocHGlobal(4096);
        int  resultSize;

        if(DetectOS.Is64Bit)
        {
            long result64 = Extern.readlink64(path, buf, 4096);

            if(result64 <= 0) return null;

            resultSize = (int)result64;
        }
        else
        {
            int result = Extern.readlink(path, buf, 4096);

            if(result <= 0) return null;

            resultSize = result;
        }

        var resultString = new byte[resultSize];
        Marshal.Copy(buf, resultString, 0, resultSize);
        Marshal.FreeHGlobal(buf);

        return Encoding.ASCII.GetString(resultString);
    }

    /// <inheritdoc />
    public override bool BufferedOsRead(out byte[] buffer, long offset, uint length, out double duration)

    {
        buffer = new byte[length];

        var cmdStopwatch = new Stopwatch();
        cmdStopwatch.Start();

        long sense = Extern.lseek(_fileDescriptor, offset, SeekWhence.Begin);

        if(sense < 0)
        {
            cmdStopwatch.Stop();
            duration = cmdStopwatch.Elapsed.TotalMilliseconds;

            Error     = true;
            LastError = Marshal.GetLastWin32Error();

            return true;
        }

        sense = DetectOS.Is64Bit
                    ? Extern.read64(_fileDescriptor, buffer, length)
                    : Extern.read(_fileDescriptor, buffer, (int)length);

        cmdStopwatch.Stop();
        duration = cmdStopwatch.Elapsed.TotalMilliseconds;

        int errno = Marshal.GetLastWin32Error();

        if(sense == length)
            errno                 = 0;
        else if(errno == 0) errno = -22;

        LastError = errno;
        Error     = errno != 0;

        return errno != 0;
    }
}