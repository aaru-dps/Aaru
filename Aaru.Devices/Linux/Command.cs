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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Interop;
using Aaru.Decoders.ATA;

namespace Aaru.Devices.Linux
{
    internal static class Command
    {
        /// <summary>Sends a SCSI command</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense">
        ///     <c>True</c> if SCSI error returned non-OK status and <paramref name="senseBuffer" /> contains SCSI
        ///     sense
        /// </param>
        internal static int SendScsiCommand(int fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout,
                                            ScsiIoctlDirection direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration    = 0;
            sense       = false;

            if(buffer == null)
                return -1;

            var ioHdr = new SgIoHdrT();

            senseBuffer = new byte[64];

            ioHdr.interface_id    = 'S';
            ioHdr.cmd_len         = (byte)cdb.Length;
            ioHdr.mx_sb_len       = (byte)senseBuffer.Length;
            ioHdr.dxfer_direction = direction;
            ioHdr.dxfer_len       = (uint)buffer.Length;
            ioHdr.dxferp          = Marshal.AllocHGlobal(buffer.Length);
            ioHdr.cmdp            = Marshal.AllocHGlobal(cdb.Length);
            ioHdr.sbp             = Marshal.AllocHGlobal(senseBuffer.Length);
            ioHdr.timeout         = timeout * 1000;
            ioHdr.flags           = (uint)SgFlags.DirectIo;

            Marshal.Copy(buffer, 0, ioHdr.dxferp, buffer.Length);
            Marshal.Copy(cdb, 0, ioHdr.cmdp, cdb.Length);
            Marshal.Copy(senseBuffer, 0, ioHdr.sbp, senseBuffer.Length);

            DateTime start = DateTime.UtcNow;
            int      error = Extern.ioctlSg(fd, LinuxIoctl.SgIo, ref ioHdr);
            DateTime end   = DateTime.UtcNow;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            Marshal.Copy(ioHdr.dxferp, buffer, 0, buffer.Length);
            Marshal.Copy(ioHdr.cmdp, cdb, 0, cdb.Length);
            Marshal.Copy(ioHdr.sbp, senseBuffer, 0, senseBuffer.Length);

            sense |= (ioHdr.info & SgInfo.OkMask) != SgInfo.Ok;

            duration = ioHdr.duration > 0 ? ioHdr.duration : (end - start).TotalMilliseconds;

            Marshal.FreeHGlobal(ioHdr.dxferp);
            Marshal.FreeHGlobal(ioHdr.cmdp);
            Marshal.FreeHGlobal(ioHdr.sbp);

            return error;
        }

        /// <summary>Converts ATA protocol to SG_IO direction</summary>
        /// <param name="protocol">ATA protocol</param>
        /// <returns>SG_IO direction</returns>
        static ScsiIoctlDirection AtaProtocolToScsiDirection(AtaProtocol protocol)
        {
            switch(protocol)
            {
                case AtaProtocol.DeviceDiagnostic:
                case AtaProtocol.DeviceReset:
                case AtaProtocol.HardReset:
                case AtaProtocol.NonData:
                case AtaProtocol.SoftReset:
                case AtaProtocol.ReturnResponse: return ScsiIoctlDirection.None;
                case AtaProtocol.PioIn:
                case AtaProtocol.UDmaIn: return ScsiIoctlDirection.In;
                case AtaProtocol.PioOut:
                case AtaProtocol.UDmaOut: return ScsiIoctlDirection.Out;
                default: return ScsiIoctlDirection.Unspecified;
            }
        }

        /// <summary>Sends an ATA command in CHS mode</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA error returned non-OK status</param>
        /// <param name="registers">Registers to send to drive</param>
        /// <param name="errorRegisters">Registers returned by drive</param>
        /// <param name="protocol">ATA protocol to use</param>
        /// <param name="transferRegister">Which register contains the transfer count</param>
        /// <param name="transferBlocks">Set to <c>true</c> if the transfer count is in blocks, otherwise it is in bytes</param>
        internal static int SendAtaCommand(int fd, AtaRegistersChs registers, out AtaErrorRegistersChs errorRegisters,
                                           AtaProtocol protocol, AtaTransferRegister transferRegister,
                                           ref byte[] buffer, uint timeout, bool transferBlocks, out double duration,
                                           out bool sense)
        {
            duration       = 0;
            sense          = false;
            errorRegisters = new AtaErrorRegistersChs();

            if(buffer == null)
                return -1;

            byte[] cdb = new byte[16];
            cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
            cdb[1] = (byte)(((byte)protocol << 1) & 0x1E);

            if(transferRegister != AtaTransferRegister.NoTransfer &&
               protocol         != AtaProtocol.NonData)
            {
                switch(protocol)
                {
                    case AtaProtocol.PioIn:
                    case AtaProtocol.UDmaIn:
                        cdb[2] = 0x08;

                        break;
                    default:
                        cdb[2] = 0x00;

                        break;
                }

                if(transferBlocks)
                    cdb[2] |= 0x04;

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

            int error = SendScsiCommand(fd, cdb, ref buffer, out byte[] senseBuffer, timeout,
                                        AtaProtocolToScsiDirection(protocol), out duration, out sense);

            if(senseBuffer.Length < 22 ||
               (senseBuffer[8] != 0x09 && senseBuffer[9] != 0x0C))
                return error;

            errorRegisters.Error = senseBuffer[11];

            errorRegisters.SectorCount  = senseBuffer[13];
            errorRegisters.Sector       = senseBuffer[15];
            errorRegisters.CylinderLow  = senseBuffer[17];
            errorRegisters.CylinderHigh = senseBuffer[19];
            errorRegisters.DeviceHead   = senseBuffer[20];
            errorRegisters.Status       = senseBuffer[21];

            sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0;

            return error;
        }

        /// <summary>Sends an ATA command in 28-bit LBA mode</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA error returned non-OK status</param>
        /// <param name="registers">Registers to send to drive</param>
        /// <param name="errorRegisters">Registers returned by drive</param>
        /// <param name="protocol">ATA protocol to use</param>
        /// <param name="transferRegister">Which register contains the transfer count</param>
        /// <param name="transferBlocks">Set to <c>true</c> if the transfer count is in blocks, otherwise it is in bytes</param>
        internal static int SendAtaCommand(int fd, AtaRegistersLba28 registers,
                                           out AtaErrorRegistersLba28 errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            duration       = 0;
            sense          = false;
            errorRegisters = new AtaErrorRegistersLba28();

            if(buffer == null)
                return -1;

            byte[] cdb = new byte[16];
            cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
            cdb[1] = (byte)(((byte)protocol << 1) & 0x1E);

            if(transferRegister != AtaTransferRegister.NoTransfer &&
               protocol         != AtaProtocol.NonData)
            {
                switch(protocol)
                {
                    case AtaProtocol.PioIn:
                    case AtaProtocol.UDmaIn:
                        cdb[2] = 0x08;

                        break;
                    default:
                        cdb[2] = 0x00;

                        break;
                }

                if(transferBlocks)
                    cdb[2] |= 0x04;

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

            int error = SendScsiCommand(fd, cdb, ref buffer, out byte[] senseBuffer, timeout,
                                        AtaProtocolToScsiDirection(protocol), out duration, out sense);

            if(senseBuffer.Length < 22 ||
               (senseBuffer[8] != 0x09 && senseBuffer[9] != 0x0C))
                return error;

            errorRegisters.Error = senseBuffer[11];

            errorRegisters.SectorCount = senseBuffer[13];
            errorRegisters.LbaLow      = senseBuffer[15];
            errorRegisters.LbaMid      = senseBuffer[17];
            errorRegisters.LbaHigh     = senseBuffer[19];
            errorRegisters.DeviceHead  = senseBuffer[20];
            errorRegisters.Status      = senseBuffer[21];

            sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0;

            return error;
        }

        /// <summary>Sends an ATA command in 48-bit LBA mode</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA error returned non-OK status</param>
        /// <param name="registers">Registers to send to drive</param>
        /// <param name="errorRegisters">Registers returned by drive</param>
        /// <param name="protocol">ATA protocol to use</param>
        /// <param name="transferRegister">Which register contains the transfer count</param>
        /// <param name="transferBlocks">Set to <c>true</c> if the transfer count is in blocks, otherwise it is in bytes</param>
        internal static int SendAtaCommand(int fd, AtaRegistersLba48 registers,
                                           out AtaErrorRegistersLba48 errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            duration       = 0;
            sense          = false;
            errorRegisters = new AtaErrorRegistersLba48();

            if(buffer == null)
                return -1;

            byte[] cdb = new byte[16];
            cdb[0] =  (byte)ScsiCommands.AtaPassThrough16;
            cdb[1] =  (byte)(((byte)protocol << 1) & 0x1E);
            cdb[1] |= 0x01;

            if(transferRegister != AtaTransferRegister.NoTransfer &&
               protocol         != AtaProtocol.NonData)
            {
                switch(protocol)
                {
                    case AtaProtocol.PioIn:
                    case AtaProtocol.UDmaIn:
                        cdb[2] = 0x08;

                        break;
                    default:
                        cdb[2] = 0x00;

                        break;
                }

                if(transferBlocks)
                    cdb[2] |= 0x04;

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

            int error = SendScsiCommand(fd, cdb, ref buffer, out byte[] senseBuffer, timeout,
                                        AtaProtocolToScsiDirection(protocol), out duration, out sense);

            if(senseBuffer.Length < 22 ||
               (senseBuffer[8] != 0x09 && senseBuffer[9] != 0x0C))
                return error;

            errorRegisters.Error = senseBuffer[11];

            errorRegisters.SectorCount     = (ushort)((senseBuffer[12] << 8) + senseBuffer[13]);
            errorRegisters.LbaLowPrevious  = senseBuffer[14];
            errorRegisters.LbaLowCurrent   = senseBuffer[15];
            errorRegisters.LbaMidPrevious  = senseBuffer[16];
            errorRegisters.LbaMidCurrent   = senseBuffer[17];
            errorRegisters.LbaHighPrevious = senseBuffer[18];
            errorRegisters.LbaHighCurrent  = senseBuffer[19];
            errorRegisters.DeviceHead      = senseBuffer[20];
            errorRegisters.Status          = senseBuffer[21];

            sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0;

            sense |= error != 0;

            return error;
        }

        /// <summary>Sends a MMC/SD command</summary>
        /// <returns>The result of the command.</returns>
        /// <param name="fd">File handle</param>
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
        internal static int SendMmcCommand(int fd, MmcCommands command, bool write, bool isApplication, MmcFlags flags,
                                           uint argument, uint blockSize, uint blocks, ref byte[] buffer,
                                           out uint[] response, out double duration, out bool sense, uint timeout = 0)
        {
            response = null;
            duration = 0;
            sense    = false;

            if(buffer == null)
                return -1;

            var ioCmd = new MmcIocCmd();

            IntPtr bufPtr = Marshal.AllocHGlobal(buffer.Length);

            ioCmd.write_flag = write;
            ioCmd.is_ascmd   = isApplication;
            ioCmd.opcode     = (uint)command;
            ioCmd.arg        = argument;
            ioCmd.flags      = flags;
            ioCmd.blksz      = blockSize;
            ioCmd.blocks     = blocks;

            if(timeout > 0)
            {
                ioCmd.data_timeout_ns = timeout * 1000000000;
                ioCmd.cmd_timeout_ms  = timeout * 1000;
            }

            ioCmd.data_ptr = (ulong)bufPtr;

            Marshal.Copy(buffer, 0, bufPtr, buffer.Length);

            DateTime start = DateTime.UtcNow;
            int      error = Extern.ioctlMmc(fd, LinuxIoctl.MmcIocCmd, ref ioCmd);
            DateTime end   = DateTime.UtcNow;

            sense |= error < 0;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            Marshal.Copy(bufPtr, buffer, 0, buffer.Length);

            response = ioCmd.response;
            duration = (end - start).TotalMilliseconds;

            Marshal.FreeHGlobal(bufPtr);

            return error;
        }

        internal static int SendMultipleMmcCommands(int fd, Device.MmcSingleCommand[] commands, out double duration,
                                                    out bool sense, uint timeout = 0)
        {
            duration = 0;
            sense    = false;
            int off;

            // Create array for buffers
            IntPtr[] bufferPointers = new IntPtr[commands.Length];

            // Allocate memory for the array for commands
            byte[] ioMultiCmd = new byte[sizeof(ulong) + (Marshal.SizeOf<MmcIocCmd>() * commands.Length)];

            // First value of array is uint64 with count of commands
            Array.Copy(BitConverter.GetBytes((ulong)commands.Length), 0, ioMultiCmd, 0, sizeof(ulong));

            off = sizeof(ulong);

            for(int i = 0; i < commands.Length; i++)
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
                    ioCmd.data_timeout_ns = timeout * 1000000000;
                    ioCmd.cmd_timeout_ms  = timeout * 1000;
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
            IntPtr ioMultiCmdPtr = Marshal.AllocHGlobal(ioMultiCmd.Length);

            // Copy array of commands to unmanaged memory
            Marshal.Copy(ioMultiCmd, 0, ioMultiCmdPtr, ioMultiCmd.Length);

            // Send command
            DateTime start = DateTime.UtcNow;
            int      error = Extern.ioctlMmcMulti(fd, LinuxIoctl.MmcIocMultiCmd, ioMultiCmdPtr);
            DateTime end   = DateTime.UtcNow;

            sense |= error < 0;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            duration = (end - start).TotalMilliseconds;

            off = sizeof(ulong);

            // Copy array from unmanaged memory
            Marshal.Copy(ioMultiCmdPtr, ioMultiCmd, 0, ioMultiCmd.Length);

            // TODO: Use real pointers this is too slow
            for(int i = 0; i < commands.Length; i++)
            {
                byte[] tmp = new byte[Marshal.SizeOf<MmcIocCmd>()];

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

        internal static int ReOpen(string devicePath, int fd, out object newFd)
        {
            newFd = -1;

            int ret = Extern.close(fd);

            if(ret < 0)
                return Marshal.GetLastWin32Error();

            newFd = Extern.open(devicePath, FileFlags.ReadWrite | FileFlags.NonBlocking | FileFlags.CreateNew);

            if((int)newFd >= 0)
                return 0;

            int error = Marshal.GetLastWin32Error();

            if(error != 13 &&
               error != 30)
                return Marshal.GetLastWin32Error();

            newFd = Extern.open(devicePath, FileFlags.Readonly | FileFlags.NonBlocking);

            return (int)newFd < 0 ? Marshal.GetLastWin32Error() : 0;
        }

        /// <summary>Reads the contents of a symbolic link</summary>
        /// <param name="path">Path to the symbolic link</param>
        /// <returns>Contents of the symbolic link</returns>
        internal static string ReadLink(string path)
        {
            IntPtr buf = Marshal.AllocHGlobal(4096);
            int    resultSize;

            if(DetectOS.Is64Bit)
            {
                long result64 = Extern.readlink64(path, buf, 4096);

                if(result64 <= 0)
                    return null;

                resultSize = (int)result64;
            }
            else
            {
                int result = Extern.readlink(path, buf, 4096);

                if(result <= 0)
                    return null;

                resultSize = result;
            }

            byte[] resultString = new byte[resultSize];
            Marshal.Copy(buf, resultString, 0, resultSize);
            Marshal.FreeHGlobal(buf);

            return Encoding.ASCII.GetString(resultString);
        }

        internal static int BufferedOsRead(int fd, out byte[] buffer, long offset, uint length, out double duration)
        {
            buffer = new byte[length];

            DateTime start = DateTime.Now;

            long sense = Extern.lseek(fd, offset, SeekWhence.Begin);

            DateTime end = DateTime.Now;

            if(sense < 0)
            {
                duration = (end - start).TotalMilliseconds;

                return Marshal.GetLastWin32Error();
            }

            sense = DetectOS.Is64Bit ? Extern.read64(fd, buffer, length) : Extern.read(fd, buffer, (int)length);

            end      = DateTime.Now;
            duration = (end - start).TotalMilliseconds;

            int errno = Marshal.GetLastWin32Error();

            if(sense == length)
                errno = 0;
            else if(errno == 0)
                errno = -22;

            return errno;
        }
    }
}