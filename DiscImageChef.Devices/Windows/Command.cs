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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.Decoders.ATA;
using Microsoft.Win32.SafeHandles;

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
        internal static int SendScsiCommand(SafeFileHandle fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer,
                                            uint timeout, ScsiIoctlDirection direction, out double duration,
                                            out bool sense)
        {
            senseBuffer = null;
            duration = 0;
            sense = false;

            if(buffer == null) return -1;

            ScsiPassThroughDirectAndSenseBuffer sptdSb = new ScsiPassThroughDirectAndSenseBuffer();
            sptdSb.sptd = new ScsiPassThroughDirect();
            sptdSb.SenseBuf = new byte[32];
            sptdSb.sptd.Cdb = new byte[16];
            Array.Copy(cdb, sptdSb.sptd.Cdb, cdb.Length);
            sptdSb.sptd.Length = (ushort)Marshal.SizeOf(sptdSb.sptd);
            sptdSb.sptd.CdbLength = (byte)cdb.Length;
            sptdSb.sptd.SenseInfoLength = (byte)sptdSb.SenseBuf.Length;
            sptdSb.sptd.DataIn = direction;
            sptdSb.sptd.DataTransferLength = (uint)buffer.Length;
            sptdSb.sptd.TimeOutValue = timeout;
            sptdSb.sptd.DataBuffer = Marshal.AllocHGlobal(buffer.Length);
            sptdSb.sptd.SenseInfoOffset = (uint)Marshal.SizeOf(sptdSb.sptd);

            uint k = 0;
            int error = 0;

            Marshal.Copy(buffer, 0, sptdSb.sptd.DataBuffer, buffer.Length);

            DateTime start = DateTime.Now;
            bool hasError = !Extern.DeviceIoControlScsi(fd, WindowsIoctl.IoctlScsiPassThroughDirect, ref sptdSb,
                                                        (uint)Marshal.SizeOf(sptdSb), ref sptdSb,
                                                        (uint)Marshal.SizeOf(sptdSb), ref k, IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(hasError) error = Marshal.GetLastWin32Error();

            Marshal.Copy(sptdSb.sptd.DataBuffer, buffer, 0, buffer.Length);

            sense |= sptdSb.sptd.ScsiStatus != 0;

            senseBuffer = new byte[32];
            Array.Copy(sptdSb.SenseBuf, senseBuffer, 32);

            duration = (end - start).TotalMilliseconds;

            Marshal.FreeHGlobal(sptdSb.sptd.DataBuffer);

            return error;
        }

        internal static int SendAtaCommand(SafeFileHandle fd, AtaRegistersCHS registers,
                                           out AtaErrorRegistersCHS errorRegisters, AtaProtocol protocol,
                                           ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersCHS();

            if(buffer == null) return -1;

            uint offsetForBuffer = (uint)(Marshal.SizeOf(typeof(AtaPassThroughDirect)) + Marshal.SizeOf(typeof(uint)));

            AtaPassThroughDirectWithBuffer aptdBuf = new AtaPassThroughDirectWithBuffer
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

            if(protocol == AtaProtocol.PioIn || protocol == AtaProtocol.UDmaIn || protocol == AtaProtocol.Dma)
                aptdBuf.aptd.AtaFlags = AtaFlags.DataIn;
            else if(protocol == AtaProtocol.PioOut || protocol == AtaProtocol.UDmaOut)
                aptdBuf.aptd.AtaFlags = AtaFlags.DataOut;

            switch(protocol)
            {
                case AtaProtocol.Dma:
                case AtaProtocol.DmaQueued:
                case AtaProtocol.FpDma:
                case AtaProtocol.UDmaIn:
                case AtaProtocol.UDmaOut:
                    aptdBuf.aptd.AtaFlags |= AtaFlags.Dma;
                    break;
            }

            // Unknown if needed
            aptdBuf.aptd.AtaFlags |= AtaFlags.DrdyRequired;

            uint k = 0;
            int error = 0;

            Array.Copy(buffer, 0, aptdBuf.dataBuffer, 0, buffer.Length);

            DateTime start = DateTime.Now;
            sense = !Extern.DeviceIoControlAta(fd, WindowsIoctl.IoctlAtaPassThrough, ref aptdBuf,
                                               (uint)Marshal.SizeOf(aptdBuf), ref aptdBuf,
                                               (uint)Marshal.SizeOf(aptdBuf), ref k, IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(sense) error = Marshal.GetLastWin32Error();

            Array.Copy(aptdBuf.dataBuffer, 0, buffer, 0, buffer.Length);

            duration = (end - start).TotalMilliseconds;

            errorRegisters.command = aptdBuf.aptd.CurrentTaskFile.Command;
            errorRegisters.cylinderHigh = aptdBuf.aptd.CurrentTaskFile.CylinderHigh;
            errorRegisters.cylinderLow = aptdBuf.aptd.CurrentTaskFile.CylinderLow;
            errorRegisters.deviceHead = aptdBuf.aptd.CurrentTaskFile.DeviceHead;
            errorRegisters.error = aptdBuf.aptd.CurrentTaskFile.Error;
            errorRegisters.sector = aptdBuf.aptd.CurrentTaskFile.SectorNumber;
            errorRegisters.sectorCount = aptdBuf.aptd.CurrentTaskFile.SectorCount;
            errorRegisters.status = aptdBuf.aptd.CurrentTaskFile.Status;

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }

        internal static int SendAtaCommand(SafeFileHandle fd, AtaRegistersLBA28 registers,
                                           out AtaErrorRegistersLBA28 errorRegisters, AtaProtocol protocol,
                                           ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersLBA28();

            if(buffer == null) return -1;

            uint offsetForBuffer = (uint)(Marshal.SizeOf(typeof(AtaPassThroughDirect)) + Marshal.SizeOf(typeof(uint)));

            AtaPassThroughDirectWithBuffer aptdBuf = new AtaPassThroughDirectWithBuffer
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
                aptdBuf.aptd.AtaFlags = AtaFlags.DataIn;
            else if(protocol == AtaProtocol.PioOut || protocol == AtaProtocol.UDmaOut)
                aptdBuf.aptd.AtaFlags = AtaFlags.DataOut;

            switch(protocol)
            {
                case AtaProtocol.Dma:
                case AtaProtocol.DmaQueued:
                case AtaProtocol.FpDma:
                case AtaProtocol.UDmaIn:
                case AtaProtocol.UDmaOut:
                    aptdBuf.aptd.AtaFlags |= AtaFlags.Dma;
                    break;
            }

            // Unknown if needed
            aptdBuf.aptd.AtaFlags |= AtaFlags.DrdyRequired;

            uint k = 0;
            int error = 0;

            Array.Copy(buffer, 0, aptdBuf.dataBuffer, 0, buffer.Length);

            DateTime start = DateTime.Now;
            sense = !Extern.DeviceIoControlAta(fd, WindowsIoctl.IoctlAtaPassThrough, ref aptdBuf,
                                               (uint)Marshal.SizeOf(aptdBuf), ref aptdBuf,
                                               (uint)Marshal.SizeOf(aptdBuf), ref k, IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(sense) error = Marshal.GetLastWin32Error();

            Array.Copy(aptdBuf.dataBuffer, 0, buffer, 0, buffer.Length);

            duration = (end - start).TotalMilliseconds;

            errorRegisters.command = aptdBuf.aptd.CurrentTaskFile.Command;
            errorRegisters.lbaHigh = aptdBuf.aptd.CurrentTaskFile.CylinderHigh;
            errorRegisters.lbaMid = aptdBuf.aptd.CurrentTaskFile.CylinderLow;
            errorRegisters.deviceHead = aptdBuf.aptd.CurrentTaskFile.DeviceHead;
            errorRegisters.error = aptdBuf.aptd.CurrentTaskFile.Error;
            errorRegisters.lbaLow = aptdBuf.aptd.CurrentTaskFile.SectorNumber;
            errorRegisters.sectorCount = aptdBuf.aptd.CurrentTaskFile.SectorCount;
            errorRegisters.status = aptdBuf.aptd.CurrentTaskFile.Status;

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }

        internal static int SendAtaCommand(SafeFileHandle fd, AtaRegistersLBA48 registers,
                                           out AtaErrorRegistersLBA48 errorRegisters, AtaProtocol protocol,
                                           ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersLBA48();

            if(buffer == null) return -1;

            uint offsetForBuffer = (uint)(Marshal.SizeOf(typeof(AtaPassThroughDirect)) + Marshal.SizeOf(typeof(uint)));

            AtaPassThroughDirectWithBuffer aptdBuf = new AtaPassThroughDirectWithBuffer
            {
                aptd = new AtaPassThroughDirect
                {
                    TimeOutValue = timeout,
                    DataBuffer = (IntPtr)offsetForBuffer,
                    Length = (ushort)Marshal.SizeOf(typeof(AtaPassThroughDirect)),
                    DataTransferLength = (uint)buffer.Length,
                    PreviousTaskFile =
                        new AtaTaskFile
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
                aptdBuf.aptd.AtaFlags = AtaFlags.DataIn;
            else if(protocol == AtaProtocol.PioOut || protocol == AtaProtocol.UDmaOut)
                aptdBuf.aptd.AtaFlags = AtaFlags.DataOut;

            switch(protocol)
            {
                case AtaProtocol.Dma:
                case AtaProtocol.DmaQueued:
                case AtaProtocol.FpDma:
                case AtaProtocol.UDmaIn:
                case AtaProtocol.UDmaOut:
                    aptdBuf.aptd.AtaFlags |= AtaFlags.Dma;
                    break;
            }

            // Unknown if needed
            aptdBuf.aptd.AtaFlags |= AtaFlags.DrdyRequired;

            uint k = 0;
            int error = 0;

            Array.Copy(buffer, 0, aptdBuf.dataBuffer, 0, buffer.Length);

            DateTime start = DateTime.Now;
            sense = !Extern.DeviceIoControlAta(fd, WindowsIoctl.IoctlAtaPassThrough, ref aptdBuf,
                                               (uint)Marshal.SizeOf(aptdBuf), ref aptdBuf,
                                               (uint)Marshal.SizeOf(aptdBuf), ref k, IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(sense) error = Marshal.GetLastWin32Error();

            Array.Copy(aptdBuf.dataBuffer, 0, buffer, 0, buffer.Length);

            duration = (end - start).TotalMilliseconds;

            errorRegisters.sectorCount = (ushort)((aptdBuf.aptd.PreviousTaskFile.SectorCount << 8) +
                                                  aptdBuf.aptd.CurrentTaskFile.SectorCount);
            errorRegisters.lbaLow = (ushort)((aptdBuf.aptd.PreviousTaskFile.SectorNumber << 8) +
                                             aptdBuf.aptd.CurrentTaskFile.SectorNumber);
            errorRegisters.lbaMid = (ushort)((aptdBuf.aptd.PreviousTaskFile.CylinderLow << 8) +
                                             aptdBuf.aptd.CurrentTaskFile.CylinderLow);
            errorRegisters.lbaHigh = (ushort)((aptdBuf.aptd.PreviousTaskFile.CylinderHigh << 8) +
                                              aptdBuf.aptd.CurrentTaskFile.CylinderHigh);
            errorRegisters.command = aptdBuf.aptd.CurrentTaskFile.Command;
            errorRegisters.deviceHead = aptdBuf.aptd.CurrentTaskFile.DeviceHead;
            errorRegisters.error = aptdBuf.aptd.CurrentTaskFile.Error;
            errorRegisters.status = aptdBuf.aptd.CurrentTaskFile.Status;

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }

        internal static int SendIdeCommand(SafeFileHandle fd, AtaRegistersCHS registers,
                                           out AtaErrorRegistersCHS errorRegisters, AtaProtocol protocol,
                                           ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersCHS();

            if(buffer == null || buffer.Length > 512) return -1;

            IdePassThroughDirect iptd = new IdePassThroughDirect
            {
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
                DataBufferSize = 512,
                DataBuffer = new byte[512],
            };

            uint k = 0;
            int error = 0;

            Array.Copy(buffer, 0, iptd.DataBuffer, 0, buffer.Length);

            DateTime start = DateTime.Now;
            sense = !Extern.DeviceIoControlIde(fd, WindowsIoctl.IoctlIdePassThrough, ref iptd,
                                               (uint)Marshal.SizeOf(iptd), ref iptd, (uint)Marshal.SizeOf(iptd), ref k,
                                               IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(sense) error = Marshal.GetLastWin32Error();

            buffer = new byte[k - 12];
            Array.Copy(iptd.DataBuffer, 0, buffer, 0, buffer.Length);

            duration = (end - start).TotalMilliseconds;

            errorRegisters.command = iptd.CurrentTaskFile.Command;
            errorRegisters.cylinderHigh = iptd.CurrentTaskFile.CylinderHigh;
            errorRegisters.cylinderLow = iptd.CurrentTaskFile.CylinderLow;
            errorRegisters.deviceHead = iptd.CurrentTaskFile.DeviceHead;
            errorRegisters.error = iptd.CurrentTaskFile.Error;
            errorRegisters.sector = iptd.CurrentTaskFile.SectorNumber;
            errorRegisters.sectorCount = iptd.CurrentTaskFile.SectorCount;
            errorRegisters.status = iptd.CurrentTaskFile.Status;

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }

        internal static int SendIdeCommand(SafeFileHandle fd, AtaRegistersLBA28 registers,
                                           out AtaErrorRegistersLBA28 errorRegisters, AtaProtocol protocol,
                                           ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration = 0;
            sense = false;
            errorRegisters = new AtaErrorRegistersLBA28();

            if(buffer == null) return -1;

            uint offsetForBuffer = (uint)(Marshal.SizeOf(typeof(AtaPassThroughDirect)) + Marshal.SizeOf(typeof(uint)));

            IdePassThroughDirect iptd = new IdePassThroughDirect
            {
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
                DataBufferSize = 512,
                DataBuffer = new byte[512],
            };

            uint k = 0;
            int error = 0;

            Array.Copy(buffer, 0, iptd.DataBuffer, 0, buffer.Length);

            DateTime start = DateTime.Now;
            sense = !Extern.DeviceIoControlIde(fd, WindowsIoctl.IoctlIdePassThrough, ref iptd,
                                               (uint)Marshal.SizeOf(iptd), ref iptd, (uint)Marshal.SizeOf(iptd), ref k,
                                               IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(sense) error = Marshal.GetLastWin32Error();

            buffer = new byte[k - 12];
            Array.Copy(iptd.DataBuffer, 0, buffer, 0, buffer.Length);

            duration = (end - start).TotalMilliseconds;

            errorRegisters.command = iptd.CurrentTaskFile.Command;
            errorRegisters.lbaHigh = iptd.CurrentTaskFile.CylinderHigh;
            errorRegisters.lbaMid = iptd.CurrentTaskFile.CylinderLow;
            errorRegisters.deviceHead = iptd.CurrentTaskFile.DeviceHead;
            errorRegisters.error = iptd.CurrentTaskFile.Error;
            errorRegisters.lbaLow = iptd.CurrentTaskFile.SectorNumber;
            errorRegisters.sectorCount = iptd.CurrentTaskFile.SectorCount;
            errorRegisters.status = iptd.CurrentTaskFile.Status;

            sense = errorRegisters.error != 0 || (errorRegisters.status & 0xA5) != 0;

            return error;
        }

        internal static uint GetDeviceNumber(SafeFileHandle deviceHandle)
        {
            StorageDeviceNumber sdn = new StorageDeviceNumber();
            sdn.deviceNumber = -1;
            uint k = 0;
            if(!Extern.DeviceIoControlGetDeviceNumber(deviceHandle, WindowsIoctl.IoctlStorageGetDeviceNumber,
                                                      IntPtr.Zero, 0, ref sdn, (uint)Marshal.SizeOf(sdn), ref k,
                                                      IntPtr.Zero)) return uint.MaxValue;

            return (uint)sdn.deviceNumber;
        }

        internal static string GetDevicePath(SafeFileHandle fd)
        {
            uint devNumber = GetDeviceNumber(fd);

            if(devNumber == uint.MaxValue) return null;

            SafeFileHandle hDevInfo = Extern.SetupDiGetClassDevs(ref Consts.GuidDevinterfaceDisk, IntPtr.Zero,
                                                                 IntPtr.Zero,
                                                                 DeviceGetClassFlags.Present |
                                                                 DeviceGetClassFlags.DeviceInterface);

            if(hDevInfo.IsInvalid) return null;

            uint index = 0;
            DeviceInterfaceData spdid = new DeviceInterfaceData();
            spdid.cbSize = Marshal.SizeOf(spdid);

            byte[] buffer;

            while(true)
            {
                buffer = new byte[2048];

                if(!Extern.SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref Consts.GuidDevinterfaceDisk, index,
                                                       ref spdid)) break;

                uint size = 0;

                Extern.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref spdid, IntPtr.Zero, 0, ref size, IntPtr.Zero);

                if(size > 0 && size < buffer.Length)
                {
                    buffer[0] = (byte)(IntPtr.Size == 8
                                           ? IntPtr.Size
                                           : IntPtr.Size + Marshal.SystemDefaultCharSize); // Funny...

                    IntPtr pspdidd = Marshal.AllocHGlobal(buffer.Length);
                    Marshal.Copy(buffer, 0, pspdidd, buffer.Length);

                    bool result =
                        Extern.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref spdid, pspdidd, size, ref size,
                                                               IntPtr.Zero);

                    buffer = new byte[size];
                    Marshal.Copy(pspdidd, buffer, 0, buffer.Length);
                    Marshal.FreeHGlobal(pspdidd);

                    if(result)
                    {
                        string devicePath = Encoding.Unicode.GetString(buffer, 4, (int)size - 4);
                        SafeFileHandle hDrive = Extern.CreateFile(devicePath, 0, FileShare.Read | FileShare.Write,
                                                                  IntPtr.Zero, FileMode.OpenExisting, 0, IntPtr.Zero);

                        if(!hDrive.IsInvalid)
                        {
                            uint newDeviceNumber = GetDeviceNumber(hDrive);

                            if(newDeviceNumber == devNumber)
                            {
                                Extern.CloseHandle(hDrive);
                                return devicePath;
                            }
                        }

                        Extern.CloseHandle(hDrive);
                    }
                }

                index++;
            }

            Extern.SetupDiDestroyDeviceInfoList(hDevInfo);
            return null;
        }

        internal static bool IsSdhci(SafeFileHandle fd)
        {
            SffdiskQueryDeviceProtocolData queryData1 = new SffdiskQueryDeviceProtocolData();
            queryData1.size = (ushort)Marshal.SizeOf(queryData1);
            uint bytesReturned;
            Extern.DeviceIoControl(fd, WindowsIoctl.IoctlSffdiskQueryDeviceProtocol, IntPtr.Zero, 0, ref queryData1,
                                   queryData1.size, out bytesReturned, IntPtr.Zero);
            return queryData1.protocolGuid.Equals(Consts.GuidSffProtocolSd);
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
        internal static int SendMmcCommand(SafeFileHandle fd, MmcCommands command, bool write, bool isApplication,
                                           MmcFlags flags, uint argument, uint blockSize, uint blocks,
                                           ref byte[] buffer, out uint[] response, out double duration, out bool sense,
                                           uint timeout = 0)
        {
            SffdiskDeviceCommandData commandData = new SffdiskDeviceCommandData();
            SdCmdDescriptor commandDescriptor = new SdCmdDescriptor();
            commandData.size = (ushort)Marshal.SizeOf(commandData);
            commandData.command = SffdiskDcmd.DeviceCommand;
            commandData.protocolArgumentSize = (ushort)Marshal.SizeOf(commandDescriptor);
            commandData.deviceDataBufferSize = blockSize * blocks;
            commandDescriptor.commandCode = (byte)command;
            commandDescriptor.cmdClass = isApplication ? SdCommandClass.AppCmd : SdCommandClass.Standard;
            commandDescriptor.transferDirection = write ? SdTransferDirection.Write : SdTransferDirection.Read;
            commandDescriptor.transferType = flags.HasFlag(MmcFlags.CommandAdtc)
                                                 ? SdTransferType.SingleBlock
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
            if(flags.HasFlag(MmcFlags.ResponseR6)) commandDescriptor.responseType = SdResponseType.R6;

            byte[] commandB = new byte[commandData.size + commandData.protocolArgumentSize +
                                        commandData.deviceDataBufferSize];
            IntPtr hBuf = Marshal.AllocHGlobal(commandB.Length);
            Marshal.StructureToPtr(commandData, hBuf, true);
            IntPtr descriptorOffset = new IntPtr(hBuf.ToInt32() + commandData.size);
            Marshal.StructureToPtr(commandDescriptor, descriptorOffset, true);
            Marshal.Copy(hBuf, commandB, 0, commandB.Length);
            Marshal.FreeHGlobal(hBuf);

            uint bytesReturned;
            int error = 0;
            DateTime start = DateTime.Now;
            sense = !Extern.DeviceIoControl(fd, WindowsIoctl.IoctlSffdiskDeviceCommand, commandB,
                                            (uint)commandB.Length, commandB, (uint)commandB.Length,
                                            out bytesReturned, IntPtr.Zero);
            DateTime end = DateTime.Now;

            if(sense) error = Marshal.GetLastWin32Error();

            buffer = new byte[blockSize * blocks];
            Buffer.BlockCopy(commandB, commandB.Length - buffer.Length, buffer, 0, buffer.Length);

            response = new uint[4];
            duration = (end - start).TotalMilliseconds;

            return error;
        }
    }
}