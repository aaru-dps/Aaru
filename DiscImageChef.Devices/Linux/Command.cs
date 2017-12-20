// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Devices.Linux
{
    static class Command
    {
        /// <summary>
        /// Sends a SCSI command
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="fd">File handle</param>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if SCSI error returned non-OK status and <paramref name="senseBuffer"/> contains SCSI sense</param>
        internal static int SendScsiCommand(int fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout,
                                            ScsiIoctlDirection direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration = 0;
            sense = false;

            if(buffer == null) return -1;

            SgIoHdrT ioHdr = new SgIoHdrT();

            senseBuffer = new byte[32];

            ioHdr.interface_id = 'S';
            ioHdr.cmd_len = (byte)cdb.Length;
            ioHdr.mx_sb_len = (byte)senseBuffer.Length;
            ioHdr.dxfer_direction = direction;
            ioHdr.dxfer_len = (uint)buffer.Length;
            ioHdr.dxferp = Marshal.AllocHGlobal(buffer.Length);
            ioHdr.cmdp = Marshal.AllocHGlobal(cdb.Length);
            ioHdr.sbp = Marshal.AllocHGlobal(senseBuffer.Length);
            ioHdr.timeout = timeout * 1000;

            Marshal.Copy(buffer, 0, ioHdr.dxferp, buffer.Length);
            Marshal.Copy(cdb, 0, ioHdr.cmdp, cdb.Length);
            Marshal.Copy(senseBuffer, 0, ioHdr.sbp, senseBuffer.Length);

            DateTime start = DateTime.UtcNow;
            int error = Extern.ioctlSg(fd, LinuxIoctl.SgIo, ref ioHdr);
            DateTime end = DateTime.UtcNow;

            if(error < 0) error = Marshal.GetLastWin32Error();

            Marshal.Copy(ioHdr.dxferp, buffer, 0, buffer.Length);
            Marshal.Copy(ioHdr.cmdp, cdb, 0, cdb.Length);
            Marshal.Copy(ioHdr.sbp, senseBuffer, 0, senseBuffer.Length);

            sense |= (ioHdr.info & SgInfo.OkMask) != SgInfo.Ok;

            if(ioHdr.duration > 0) duration = ioHdr.duration;
            else duration = (end - start).TotalMilliseconds;

            Marshal.FreeHGlobal(ioHdr.dxferp);
            Marshal.FreeHGlobal(ioHdr.cmdp);
            Marshal.FreeHGlobal(ioHdr.sbp);

            return error;
        }

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

        internal static int SendAtaCommand(int fd, AtaRegistersCHS registers, out AtaErrorRegistersCHS errorRegisters,
                                           AtaProtocol protocol, AtaTransferRegister transferRegister,
                                           ref byte[] buffer, uint timeout, bool transferBlocks, out double duration,
                                           out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersCHS();

            if(buffer == null) return -1;

            byte[] cdb = new byte[16];
            cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
            cdb[1] = (byte)(((byte)protocol << 1) & 0x1E);
            if(transferRegister != AtaTransferRegister.NoTransfer && protocol != AtaProtocol.NonData)
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

                if(transferBlocks) cdb[2] |= 0x04;

                cdb[2] |= (byte)((int)transferRegister & 0x03);
            }

            //cdb[2] |= 0x20;

            cdb[4] = registers.feature;
            cdb[6] = registers.sectorCount;
            cdb[8] = registers.sector;
            cdb[10] = registers.cylinderLow;
            cdb[12] = registers.cylinderHigh;
            cdb[13] = registers.deviceHead;
            cdb[14] = registers.command;

            byte[] senseBuffer;
            int error = SendScsiCommand(fd, cdb, ref buffer, out senseBuffer, timeout,
                                        AtaProtocolToScsiDirection(protocol), out duration, out sense);

            if(senseBuffer.Length < 22 || senseBuffer[8] != 0x09 && senseBuffer[9] != 0x0C) return error;

            errorRegisters.error = senseBuffer[11];

            errorRegisters.sectorCount = senseBuffer[13];
            errorRegisters.sector = senseBuffer[15];
            errorRegisters.cylinderLow = senseBuffer[17];
            errorRegisters.cylinderHigh = senseBuffer[19];
            errorRegisters.deviceHead = senseBuffer[20];
            errorRegisters.status = senseBuffer[21];

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }

        internal static int SendAtaCommand(int fd, AtaRegistersLBA28 registers,
                                           out AtaErrorRegistersLBA28 errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersLBA28();

            if(buffer == null) return -1;

            byte[] cdb = new byte[16];
            cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
            cdb[1] = (byte)(((byte)protocol << 1) & 0x1E);
            if(transferRegister != AtaTransferRegister.NoTransfer && protocol != AtaProtocol.NonData)
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

                if(transferBlocks) cdb[2] |= 0x04;

                cdb[2] |= (byte)((int)transferRegister & 0x03);
            }

            cdb[2] |= 0x20;

            cdb[4] = registers.feature;
            cdb[6] = registers.sectorCount;
            cdb[8] = registers.lbaLow;
            cdb[10] = registers.lbaMid;
            cdb[12] = registers.lbaHigh;
            cdb[13] = registers.deviceHead;
            cdb[14] = registers.command;

            byte[] senseBuffer;
            int error = SendScsiCommand(fd, cdb, ref buffer, out senseBuffer, timeout,
                                        AtaProtocolToScsiDirection(protocol), out duration, out sense);

            if(senseBuffer.Length < 22 || senseBuffer[8] != 0x09 && senseBuffer[9] != 0x0C) return error;

            errorRegisters.error = senseBuffer[11];

            errorRegisters.sectorCount = senseBuffer[13];
            errorRegisters.lbaLow = senseBuffer[15];
            errorRegisters.lbaMid = senseBuffer[17];
            errorRegisters.lbaHigh = senseBuffer[19];
            errorRegisters.deviceHead = senseBuffer[20];
            errorRegisters.status = senseBuffer[21];

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }

        internal static int SendAtaCommand(int fd, AtaRegistersLBA48 registers,
                                           out AtaErrorRegistersLBA48 errorRegisters, AtaProtocol protocol,
                                           AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
                                           bool transferBlocks, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersLBA48();

            if(buffer == null) return -1;

            byte[] cdb = new byte[16];
            cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
            cdb[1] = (byte)(((byte)protocol << 1) & 0x1E);
            cdb[1] |= 0x01;
            if(transferRegister != AtaTransferRegister.NoTransfer && protocol != AtaProtocol.NonData)
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

                if(transferBlocks) cdb[2] |= 0x04;

                cdb[2] |= (byte)((int)transferRegister & 0x03);
            }

            cdb[2] |= 0x20;

            cdb[3] = (byte)((registers.feature & 0xFF00) >> 8);
            cdb[4] = (byte)(registers.feature & 0xFF);
            cdb[5] = (byte)((registers.sectorCount & 0xFF00) >> 8);
            cdb[6] = (byte)(registers.sectorCount & 0xFF);
            cdb[7] = (byte)((registers.lbaLow & 0xFF00) >> 8);
            cdb[8] = (byte)(registers.lbaLow & 0xFF);
            cdb[9] = (byte)((registers.lbaMid & 0xFF00) >> 8);
            cdb[10] = (byte)(registers.lbaMid & 0xFF);
            cdb[11] = (byte)((registers.lbaHigh & 0xFF00) >> 8);
            cdb[12] = (byte)(registers.lbaHigh & 0xFF);
            cdb[13] = registers.deviceHead;
            cdb[14] = registers.command;

            byte[] senseBuffer;
            int error = SendScsiCommand(fd, cdb, ref buffer, out senseBuffer, timeout,
                                        AtaProtocolToScsiDirection(protocol), out duration, out sense);

            if(senseBuffer.Length < 22 || senseBuffer[8] != 0x09 && senseBuffer[9] != 0x0C) return error;

            errorRegisters.error = senseBuffer[11];

            errorRegisters.sectorCount = (ushort)((senseBuffer[12] << 8) + senseBuffer[13]);
            errorRegisters.lbaLow = (ushort)((senseBuffer[14] << 8) + senseBuffer[15]);
            errorRegisters.lbaMid = (ushort)((senseBuffer[16] << 8) + senseBuffer[17]);
            errorRegisters.lbaHigh = (ushort)((senseBuffer[18] << 8) + senseBuffer[19]);
            errorRegisters.deviceHead = senseBuffer[20];
            errorRegisters.status = senseBuffer[21];

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            sense |= error != 0;

            return error;
        }

        /// <summary>
        /// Sends a MMC/SD command
        /// </summary>
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
            sense = false;

            if(buffer == null) return -1;

            MmcIocCmd ioCmd = new MmcIocCmd();

            IntPtr bufPtr = Marshal.AllocHGlobal(buffer.Length);

            ioCmd.write_flag = write;
            ioCmd.is_ascmd = isApplication;
            ioCmd.opcode = (uint)command;
            ioCmd.arg = argument;
            ioCmd.flags = flags;
            ioCmd.blksz = blockSize;
            ioCmd.blocks = blocks;
            if(timeout > 0)
            {
                ioCmd.data_timeout_ns = timeout * 1000000000;
                ioCmd.cmd_timeout_ms = timeout * 1000;
            }
            ioCmd.data_ptr = (ulong)bufPtr;

            Marshal.Copy(buffer, 0, bufPtr, buffer.Length);

            DateTime start = DateTime.UtcNow;
            int error = Extern.ioctlMmc(fd, LinuxIoctl.MmcIocCmd, ref ioCmd);
            DateTime end = DateTime.UtcNow;

            sense |= error < 0;

            if(error < 0) error = Marshal.GetLastWin32Error();

            Marshal.Copy(bufPtr, buffer, 0, buffer.Length);

            response = ioCmd.response;
            duration = (end - start).TotalMilliseconds;

            Marshal.FreeHGlobal(bufPtr);

            return error;
        }

        public static string ReadLink(string path)
        {
            IntPtr buf = Marshal.AllocHGlobal(4096);
            int resultSize;

            if(Interop.DetectOS.Is64Bit())
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

            byte[] resultString = new byte[resultSize];
            Marshal.Copy(buf, resultString, 0, resultSize);
            Marshal.FreeHGlobal(buf);
            return System.Text.Encoding.ASCII.GetString(resultString);
        }
    }
}