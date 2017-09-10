// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Devices.Windows
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
        internal static int SendScsiCommand(SafeFileHandle fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, ScsiIoctlDirection direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration = 0;
            sense = false;

            if(buffer == null)
                return -1;

            ScsiPassThroughDirectAndSenseBuffer sptd_sb = new ScsiPassThroughDirectAndSenseBuffer();
            sptd_sb.sptd = new ScsiPassThroughDirect();
            sptd_sb.SenseBuf = new byte[32];
            sptd_sb.sptd.Cdb = new byte[16];
            Array.Copy(cdb, sptd_sb.sptd.Cdb, cdb.Length);
            sptd_sb.sptd.Length = (ushort)Marshal.SizeOf(sptd_sb.sptd);
            sptd_sb.sptd.CdbLength = (byte)cdb.Length;
            sptd_sb.sptd.SenseInfoLength = (byte)sptd_sb.SenseBuf.Length;
            sptd_sb.sptd.DataIn = direction;
            sptd_sb.sptd.DataTransferLength = (uint)buffer.Length;
            sptd_sb.sptd.TimeOutValue = timeout;
            sptd_sb.sptd.DataBuffer = Marshal.AllocHGlobal(buffer.Length);
            sptd_sb.sptd.SenseInfoOffset = (uint)Marshal.SizeOf(sptd_sb.sptd);

            uint k = 0;
            int error = 0;

            Marshal.Copy(buffer, 0, sptd_sb.sptd.DataBuffer, buffer.Length);

            DateTime start = DateTime.Now;
            bool hasError = !Extern.DeviceIoControlScsi(fd, WindowsIoctl.IOCTL_SCSI_PASS_THROUGH_DIRECT, ref sptd_sb, (uint)Marshal.SizeOf(sptd_sb), ref sptd_sb,
                            (uint)Marshal.SizeOf(sptd_sb), ref k, IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(hasError)
                error = Marshal.GetLastWin32Error();

            Marshal.Copy(sptd_sb.sptd.DataBuffer, buffer, 0, buffer.Length);

            sense |= sptd_sb.sptd.ScsiStatus != 0;

            senseBuffer = new byte[32];
            Array.Copy(sptd_sb.SenseBuf, senseBuffer, 32);

            duration = (end - start).TotalMilliseconds;

            Marshal.FreeHGlobal(sptd_sb.sptd.DataBuffer);

            return error;
        }

        internal static int SendAtaCommand(SafeFileHandle fd, AtaRegistersCHS registers, out AtaErrorRegistersCHS errorRegisters,
                                           AtaProtocol protocol, ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersCHS();

            if(buffer == null)
                return -1;

            uint offsetForBuffer = (uint)(Marshal.SizeOf(typeof(AtaPassThroughDirect)) + Marshal.SizeOf(typeof(uint)));

            AtaPassThroughDirectWithBuffer aptd_buf = new AtaPassThroughDirectWithBuffer
            {
                aptd = new AtaPassThroughDirect
                {
                    TimeOutValue = timeout,
                    DataBuffer = (IntPtr)offsetForBuffer,
                    Length = (ushort)Marshal.SizeOf(typeof(AtaPassThroughDirect)),
                    DataTransferLength = (uint)buffer.Length,
                    PreviousTaskFile = new AtaTaskFile(),
                    CurrentTaskFile = new AtaTaskFile
                    {
                        Command = registers.command,
                        CylinderHigh = registers.cylinderHigh,
                        CylinderLow = registers.cylinderLow,
                        DeviceHead = registers.deviceHead,
                        Features = registers.feature,
                        SectorCount = registers.sectorCount,
                        SectorNumber = registers.sector
                    },
                },
                dataBuffer = new byte[64 * 512]
            };

            if (protocol == AtaProtocol.PioIn || protocol == AtaProtocol.UDmaIn || protocol == AtaProtocol.Dma)
                aptd_buf.aptd.AtaFlags = AtaFlags.DataIn;
            else if(protocol == AtaProtocol.PioOut || protocol == AtaProtocol.UDmaOut)
                aptd_buf.aptd.AtaFlags = AtaFlags.DataOut;

            switch(protocol)
            {
                case AtaProtocol.Dma:
                case AtaProtocol.DmaQueued:
                case AtaProtocol.FPDma:
                case AtaProtocol.UDmaIn:
                case AtaProtocol.UDmaOut:
                    aptd_buf.aptd.AtaFlags |= AtaFlags.DMA;
                    break;
            }

            // Unknown if needed
            aptd_buf.aptd.AtaFlags |= AtaFlags.DrdyRequired;

            uint k = 0;
            int error = 0;

            Array.Copy(buffer, 0, aptd_buf.dataBuffer, 0, buffer.Length);

            DateTime start = DateTime.Now;
            sense = !Extern.DeviceIoControlAta(fd, WindowsIoctl.IOCTL_ATA_PASS_THROUGH, ref aptd_buf, (uint)Marshal.SizeOf(aptd_buf), ref aptd_buf,
                            (uint)Marshal.SizeOf(aptd_buf), ref k, IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(sense)
                error = Marshal.GetLastWin32Error();

            Array.Copy(aptd_buf.dataBuffer, 0, buffer, 0, buffer.Length);

            duration = (end - start).TotalMilliseconds;

            errorRegisters.command = aptd_buf.aptd.CurrentTaskFile.Command;
            errorRegisters.cylinderHigh = aptd_buf.aptd.CurrentTaskFile.CylinderHigh;
            errorRegisters.cylinderLow = aptd_buf.aptd.CurrentTaskFile.CylinderLow;
            errorRegisters.deviceHead = aptd_buf.aptd.CurrentTaskFile.DeviceHead;
            errorRegisters.error = aptd_buf.aptd.CurrentTaskFile.Error;
            errorRegisters.sector = aptd_buf.aptd.CurrentTaskFile.SectorNumber;
            errorRegisters.sectorCount = aptd_buf.aptd.CurrentTaskFile.SectorCount;
            errorRegisters.status = aptd_buf.aptd.CurrentTaskFile.Status;

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }

        internal static int SendAtaCommand(SafeFileHandle fd, AtaRegistersLBA28 registers, out AtaErrorRegistersLBA28 errorRegisters,
                                           AtaProtocol protocol, ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersLBA28();

            if(buffer == null)
                return -1;

            uint offsetForBuffer = (uint)(Marshal.SizeOf(typeof(AtaPassThroughDirect)) + Marshal.SizeOf(typeof(uint)));

            AtaPassThroughDirectWithBuffer aptd_buf = new AtaPassThroughDirectWithBuffer
            {
                aptd = new AtaPassThroughDirect
                {
                    TimeOutValue = timeout,
                    DataBuffer = (IntPtr)offsetForBuffer,
                    Length = (ushort)Marshal.SizeOf(typeof(AtaPassThroughDirect)),
                    DataTransferLength = (uint)buffer.Length,
                    PreviousTaskFile = new AtaTaskFile(),
                    CurrentTaskFile = new AtaTaskFile
                    {
                        Command = registers.command,
                        CylinderHigh = registers.lbaHigh,
                        CylinderLow = registers.lbaMid,
                        DeviceHead = registers.deviceHead,
                        Features = registers.feature,
                        SectorCount = registers.sectorCount,
                        SectorNumber = registers.lbaLow
                    },
                },
                dataBuffer = new byte[64 * 512]
            };

            if(protocol == AtaProtocol.PioIn || protocol == AtaProtocol.UDmaIn || protocol == AtaProtocol.Dma)
                aptd_buf.aptd.AtaFlags = AtaFlags.DataIn;
            else if(protocol == AtaProtocol.PioOut || protocol == AtaProtocol.UDmaOut)
                aptd_buf.aptd.AtaFlags = AtaFlags.DataOut;

            switch(protocol)
            {
                case AtaProtocol.Dma:
                case AtaProtocol.DmaQueued:
                case AtaProtocol.FPDma:
                case AtaProtocol.UDmaIn:
                case AtaProtocol.UDmaOut:
                    aptd_buf.aptd.AtaFlags |= AtaFlags.DMA;
                    break;
            }

            // Unknown if needed
            aptd_buf.aptd.AtaFlags |= AtaFlags.DrdyRequired;

            uint k = 0;
            int error = 0;

            Array.Copy(buffer, 0, aptd_buf.dataBuffer, 0, buffer.Length);

            DateTime start = DateTime.Now;
            sense = !Extern.DeviceIoControlAta(fd, WindowsIoctl.IOCTL_ATA_PASS_THROUGH, ref aptd_buf, (uint)Marshal.SizeOf(aptd_buf), ref aptd_buf,
                            (uint)Marshal.SizeOf(aptd_buf), ref k, IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(sense)
                error = Marshal.GetLastWin32Error();

            Array.Copy(aptd_buf.dataBuffer, 0, buffer, 0, buffer.Length);

            duration = (end - start).TotalMilliseconds;

            errorRegisters.command = aptd_buf.aptd.CurrentTaskFile.Command;
            errorRegisters.lbaHigh = aptd_buf.aptd.CurrentTaskFile.CylinderHigh;
            errorRegisters.lbaMid = aptd_buf.aptd.CurrentTaskFile.CylinderLow;
            errorRegisters.deviceHead = aptd_buf.aptd.CurrentTaskFile.DeviceHead;
            errorRegisters.error = aptd_buf.aptd.CurrentTaskFile.Error;
            errorRegisters.lbaLow = aptd_buf.aptd.CurrentTaskFile.SectorNumber;
            errorRegisters.sectorCount = aptd_buf.aptd.CurrentTaskFile.SectorCount;
            errorRegisters.status = aptd_buf.aptd.CurrentTaskFile.Status;

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }

        internal static int SendAtaCommand(SafeFileHandle fd, AtaRegistersLBA48 registers, out AtaErrorRegistersLBA48 errorRegisters,
                                   AtaProtocol protocol, ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersLBA48();

            if(buffer == null)
                return -1;

            uint offsetForBuffer = (uint)(Marshal.SizeOf(typeof(AtaPassThroughDirect)) + Marshal.SizeOf(typeof(uint)));

            AtaPassThroughDirectWithBuffer aptd_buf = new AtaPassThroughDirectWithBuffer
            {
                aptd = new AtaPassThroughDirect
                {
                    TimeOutValue = timeout,
                    DataBuffer = (IntPtr)offsetForBuffer,
                    Length = (ushort)Marshal.SizeOf(typeof(AtaPassThroughDirect)),
                    DataTransferLength = (uint)buffer.Length,
                    PreviousTaskFile = new AtaTaskFile
                    {
                        CylinderHigh = (byte)((registers.lbaHigh & 0xFF00) >> 8),
                        CylinderLow = (byte)((registers.lbaMid & 0xFF00) >> 8),
                        Features = (byte)((registers.feature & 0xFF00) >> 8),
                        SectorCount = (byte)((registers.sectorCount & 0xFF00) >> 8),
                        SectorNumber = (byte)((registers.lbaLow & 0xFF00) >> 8)
                    },
                    CurrentTaskFile = new AtaTaskFile
                    {
                        Command = registers.command,
                        CylinderHigh = (byte)(registers.lbaHigh & 0xFF),
                        CylinderLow = (byte)(registers.lbaMid & 0xFF),
                        DeviceHead = registers.deviceHead,
                        Features = (byte)(registers.feature & 0xFF),
                        SectorCount = (byte)(registers.sectorCount & 0xFF),
                        SectorNumber = (byte)(registers.lbaLow & 0xFF)
                    },
                },
                dataBuffer = new byte[64 * 512]
            };

            if(protocol == AtaProtocol.PioIn || protocol == AtaProtocol.UDmaIn || protocol == AtaProtocol.Dma)
                aptd_buf.aptd.AtaFlags = AtaFlags.DataIn;
            else if(protocol == AtaProtocol.PioOut || protocol == AtaProtocol.UDmaOut)
                aptd_buf.aptd.AtaFlags = AtaFlags.DataOut;

            switch(protocol)
            {
                case AtaProtocol.Dma:
                case AtaProtocol.DmaQueued:
                case AtaProtocol.FPDma:
                case AtaProtocol.UDmaIn:
                case AtaProtocol.UDmaOut:
                    aptd_buf.aptd.AtaFlags |= AtaFlags.DMA;
                    break;
            }

            // Unknown if needed
            aptd_buf.aptd.AtaFlags |= AtaFlags.DrdyRequired;

            uint k = 0;
            int error = 0;

            Array.Copy(buffer, 0, aptd_buf.dataBuffer, 0, buffer.Length);

            DateTime start = DateTime.Now;
            sense = !Extern.DeviceIoControlAta(fd, WindowsIoctl.IOCTL_ATA_PASS_THROUGH, ref aptd_buf, (uint)Marshal.SizeOf(aptd_buf), ref aptd_buf,
                            (uint)Marshal.SizeOf(aptd_buf), ref k, IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(sense)
                error = Marshal.GetLastWin32Error();

            Array.Copy(aptd_buf.dataBuffer, 0, buffer, 0, buffer.Length);

            duration = (end - start).TotalMilliseconds;

            errorRegisters.sectorCount = (ushort)((aptd_buf.aptd.PreviousTaskFile.SectorCount << 8) + aptd_buf.aptd.CurrentTaskFile.SectorCount);
            errorRegisters.lbaLow = (ushort)((aptd_buf.aptd.PreviousTaskFile.SectorNumber << 8) + aptd_buf.aptd.CurrentTaskFile.SectorNumber);
            errorRegisters.lbaMid = (ushort)((aptd_buf.aptd.PreviousTaskFile.CylinderLow << 8) + aptd_buf.aptd.CurrentTaskFile.CylinderLow);
            errorRegisters.lbaHigh = (ushort)((aptd_buf.aptd.PreviousTaskFile.CylinderHigh << 8) + aptd_buf.aptd.CurrentTaskFile.CylinderHigh);
            errorRegisters.command = aptd_buf.aptd.CurrentTaskFile.Command;
            errorRegisters.deviceHead = aptd_buf.aptd.CurrentTaskFile.DeviceHead;
            errorRegisters.error = aptd_buf.aptd.CurrentTaskFile.Error;
            errorRegisters.status = aptd_buf.aptd.CurrentTaskFile.Status;

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }
    }
}

