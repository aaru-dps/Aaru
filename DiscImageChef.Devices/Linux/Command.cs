// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Command.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Linux direct device access
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Contains a high level representation of the Linux syscalls used to directly
// interface devices
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
        internal static int SendScsiCommand(int fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, ScsiIoctlDirection direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration = 0;
            sense = false;

            if (buffer == null)
                return -1;

            sg_io_hdr_t io_hdr = new sg_io_hdr_t();

            senseBuffer = new byte[32];

            io_hdr.interface_id = 'S';
            io_hdr.cmd_len = (byte)cdb.Length;
            io_hdr.mx_sb_len = (byte)senseBuffer.Length;
            io_hdr.dxfer_direction = direction;
            io_hdr.dxfer_len = (uint)buffer.Length;
            io_hdr.dxferp = Marshal.AllocHGlobal(buffer.Length);
            io_hdr.cmdp = Marshal.AllocHGlobal(cdb.Length);
            io_hdr.sbp = Marshal.AllocHGlobal(senseBuffer.Length);
            io_hdr.timeout = timeout * 1000;

            Marshal.Copy(buffer, 0, io_hdr.dxferp, buffer.Length);
            Marshal.Copy(cdb, 0, io_hdr.cmdp, cdb.Length);
            Marshal.Copy(senseBuffer, 0, io_hdr.sbp, senseBuffer.Length);

            int error = Extern.ioctlSg(fd, LinuxIoctl.SG_IO, ref io_hdr);

            if (error < 0)
                error = Marshal.GetLastWin32Error();

            Marshal.Copy(io_hdr.dxferp, buffer, 0, buffer.Length);
            Marshal.Copy(io_hdr.cmdp, cdb, 0, cdb.Length);
            Marshal.Copy(io_hdr.sbp, senseBuffer, 0, senseBuffer.Length);

            sense |= (io_hdr.info & SgInfo.OkMask) != SgInfo.Ok;

            duration = (double)io_hdr.duration;

            Marshal.FreeHGlobal(io_hdr.dxferp);
            Marshal.FreeHGlobal(io_hdr.cmdp);
            Marshal.FreeHGlobal(io_hdr.sbp);

            return error;
        }

        static ScsiIoctlDirection AtaProtocolToScsiDirection(AtaProtocol protocol)
        {
            switch (protocol)
            {
                case AtaProtocol.DeviceDiagnostic:
                case AtaProtocol.DeviceReset:
                case AtaProtocol.HardReset:
                case AtaProtocol.NonData:
                case AtaProtocol.SoftReset:
                case AtaProtocol.ReturnResponse:
                    return ScsiIoctlDirection.None;
                case AtaProtocol.PioIn:
                case AtaProtocol.UDmaIn:
                    return ScsiIoctlDirection.In;
                case AtaProtocol.PioOut:
                case AtaProtocol.UDmaOut:
                    return ScsiIoctlDirection.Out;
                default:
                    return ScsiIoctlDirection.Unspecified;
            }
        }

        internal static int SendAtaCommand(int fd, AtaRegistersCHS registers,
            out AtaErrorRegistersCHS errorRegisters, AtaProtocol protocol,
            AtaTransferRegister transferRegister, ref byte[] buffer, uint timeout,
            bool transferBlocks, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersCHS();

            if (buffer == null)
                return -1;

            byte[] cdb = new byte[16];
            cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
            cdb[1] = (byte)(((byte)protocol << 1) & 0x1E);
            if (transferRegister != AtaTransferRegister.NoTransfer &&
               protocol != AtaProtocol.NonData)
            {
                switch (protocol)
                {
                    case AtaProtocol.PioIn:
                    case AtaProtocol.UDmaIn:
                        cdb[2] = 0x08;
                        break;
                    default:
                        cdb[2] = 0x00;
                        break;
                }

                if (transferBlocks)
                    cdb[2] |= 0x04;

                cdb[2] |= (byte)((int)transferRegister & 0x03);
            }

            cdb[2] |= 0x20;

            cdb[4] = registers.feature;
            cdb[6] = registers.sectorCount;
            cdb[8] = registers.sector;
            cdb[10] = registers.cylinderLow;
            cdb[12] = registers.cylinderHigh;
            cdb[13] = registers.deviceHead;
            cdb[14] = registers.command;

            byte[] senseBuffer;
            int error = SendScsiCommand(fd, cdb, ref buffer, out senseBuffer, timeout, AtaProtocolToScsiDirection(protocol), out duration, out sense);

            if (senseBuffer.Length < 22 || (senseBuffer[8] != 0x09 && senseBuffer[9] != 0x0C))
                return error;

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

            if (buffer == null)
                return -1;

            byte[] cdb = new byte[16];
            cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
            cdb[1] = (byte)(((byte)protocol << 1) & 0x1E);
            if (transferRegister != AtaTransferRegister.NoTransfer &&
                protocol != AtaProtocol.NonData)
            {
                switch (protocol)
                {
                    case AtaProtocol.PioIn:
                    case AtaProtocol.UDmaIn:
                        cdb[2] = 0x08;
                        break;
                    default:
                        cdb[2] = 0x00;
                        break;
                }

                if (transferBlocks)
                    cdb[2] |= 0x04;

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
            int error = SendScsiCommand(fd, cdb, ref buffer, out senseBuffer, timeout, AtaProtocolToScsiDirection(protocol), out duration, out sense);

            if (senseBuffer.Length < 22 || (senseBuffer[8] != 0x09 && senseBuffer[9] != 0x0C))
                return error;

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

            if (buffer == null)
                return -1;

            byte[] cdb = new byte[16];
            cdb[0] = (byte)ScsiCommands.AtaPassThrough16;
            cdb[1] |= 0x01;
            cdb[1] = (byte)(((byte)protocol << 1) & 0x1E);
            if (transferRegister != AtaTransferRegister.NoTransfer &&
                protocol != AtaProtocol.NonData)
            {
                switch (protocol)
                {
                    case AtaProtocol.PioIn:
                    case AtaProtocol.UDmaIn:
                        cdb[2] = 0x08;
                        break;
                    default:
                        cdb[2] = 0x00;
                        break;
                }

                if (transferBlocks)
                    cdb[2] |= 0x04;

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
            int error = SendScsiCommand(fd, cdb, ref buffer, out senseBuffer, timeout, AtaProtocolToScsiDirection(protocol), out duration, out sense);

            if (senseBuffer.Length < 22 || (senseBuffer[8] != 0x09 && senseBuffer[9] != 0x0C))
                return error;

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

        public static string ReadLink(string path)
        {
            IntPtr buf = Marshal.AllocHGlobal(int.MaxValue);
            int resultSize;

            if (Interop.DetectOS.Is64Bit())
            {
                long result64 = Extern.readlink64(path, buf, (long)int.MaxValue);
                if (result64 <= 0)
                    return null;
                
                resultSize = (int)result64;
            }
            else
            {
                int result = Extern.readlink(path, buf, int.MaxValue);
                if (result <= 0)
                    return null;

                resultSize = result;
            }

            byte[] resultString = new byte[resultSize];
            Marshal.Copy(buf, resultString, 0, resultSize);
            Marshal.FreeHGlobal(buf);
            return System.Text.Encoding.ASCII.GetString(resultString);
        }
    }
}

