// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Command.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FreeBSD direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains a high level representation of the FreeBSD syscalls used to
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Aaru.Console;
using Aaru.Decoders.ATA;
using static Aaru.Devices.FreeBSD.Extern;

namespace Aaru.Devices.FreeBSD
{
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags"), Obsolete]
    internal static class Command
    {
        const int CAM_MAX_CDBLEN = 16;

        /// <summary>Sends a SCSI command (64-bit arch)</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="dev">CAM device</param>
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
        [Obsolete]
        internal static int SendScsiCommand64(IntPtr dev, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer,
                                              uint timeout, CcbFlags direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration    = 0;
            sense       = false;

            if(buffer == null)
                return -1;

            IntPtr ccbPtr = cam_getccb(dev);
            IntPtr cdbPtr = IntPtr.Zero;

            if(ccbPtr.ToInt64() == 0)
            {
                sense = true;

                return Marshal.GetLastWin32Error();
            }

            var csio = (CcbScsiio64)Marshal.PtrToStructure(ccbPtr, typeof(CcbScsiio64));
            csio.ccb_h.func_code   = XptOpcode.XptScsiIo;
            csio.ccb_h.flags       = direction;
            csio.ccb_h.xflags      = 0;
            csio.ccb_h.retry_count = 1;
            csio.ccb_h.cbfcnp      = IntPtr.Zero;
            csio.ccb_h.timeout     = timeout;
            csio.data_ptr          = Marshal.AllocHGlobal(buffer.Length);
            csio.dxfer_len         = (uint)buffer.Length;
            csio.sense_len         = 32;
            csio.cdb_len           = (byte)cdb.Length;

            // TODO: Create enum?
            csio.tag_action = 0x20;
            csio.cdb_bytes  = new byte[CAM_MAX_CDBLEN];

            if(cdb.Length <= CAM_MAX_CDBLEN)
                Array.Copy(cdb, 0, csio.cdb_bytes, 0, cdb.Length);
            else
            {
                cdbPtr = Marshal.AllocHGlobal(cdb.Length);
                byte[] cdbPtrBytes = BitConverter.GetBytes(cdbPtr.ToInt64());
                Array.Copy(cdbPtrBytes, 0, csio.cdb_bytes, 0, IntPtr.Size);
                csio.ccb_h.flags |= CcbFlags.CamCdbPointer;
            }

            csio.ccb_h.flags |= CcbFlags.CamDevQfrzdis;

            Marshal.Copy(buffer, 0, csio.data_ptr, buffer.Length);
            Marshal.StructureToPtr(csio, ccbPtr, false);

            DateTime start = DateTime.UtcNow;
            int      error = cam_send_ccb(dev, ccbPtr);
            DateTime end   = DateTime.UtcNow;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            csio = (CcbScsiio64)Marshal.PtrToStructure(ccbPtr, typeof(CcbScsiio64));

            if((csio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamReqCmp &&
               (csio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamScsiStatusError)
            {
                error = Marshal.GetLastWin32Error();
                AaruConsole.DebugWriteLine("FreeBSD devices", "CAM status {0} error {1}", csio.ccb_h.status, error);
                sense = true;
            }

            if((csio.ccb_h.status & CamStatus.CamStatusMask) == CamStatus.CamScsiStatusError)
            {
                sense          = true;
                senseBuffer    = new byte[1];
                senseBuffer[0] = csio.scsi_status;
            }

            if((csio.ccb_h.status & CamStatus.CamAutosnsValid) != 0)
                if(csio.sense_len - csio.sense_resid > 0)
                {
                    sense          = (csio.ccb_h.status & CamStatus.CamStatusMask) == CamStatus.CamScsiStatusError;
                    senseBuffer    = new byte[csio.sense_len - csio.sense_resid];
                    senseBuffer[0] = csio.sense_data.error_code;
                    Array.Copy(csio.sense_data.sense_buf, 0, senseBuffer, 1, senseBuffer.Length - 1);
                }

            buffer = new byte[csio.dxfer_len];
            cdb    = new byte[csio.cdb_len];

            Marshal.Copy(csio.data_ptr, buffer, 0, buffer.Length);

            if(csio.ccb_h.flags.HasFlag(CcbFlags.CamCdbPointer))
                Marshal.Copy(new IntPtr(BitConverter.ToInt64(csio.cdb_bytes, 0)), cdb, 0, cdb.Length);
            else
                Array.Copy(csio.cdb_bytes, 0, cdb, 0, cdb.Length);

            duration = (end - start).TotalMilliseconds;

            if(csio.ccb_h.flags.HasFlag(CcbFlags.CamCdbPointer))
                Marshal.FreeHGlobal(cdbPtr);

            Marshal.FreeHGlobal(csio.data_ptr);
            cam_freeccb(ccbPtr);

            return error;
        }

        /// <summary>Sends a SCSI command (32-bit arch)</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="dev">CAM device</param>
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
        [Obsolete]
        internal static int SendScsiCommand(IntPtr dev, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer,
                                            uint timeout, CcbFlags direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration    = 0;
            sense       = false;

            if(buffer == null)
                return -1;

            IntPtr ccbPtr = cam_getccb(dev);
            IntPtr cdbPtr = IntPtr.Zero;

            if(ccbPtr.ToInt32() == 0)
            {
                sense = true;

                return Marshal.GetLastWin32Error();
            }

            var csio = (CcbScsiio)Marshal.PtrToStructure(ccbPtr, typeof(CcbScsiio));
            csio.ccb_h.func_code   = XptOpcode.XptScsiIo;
            csio.ccb_h.flags       = direction;
            csio.ccb_h.xflags      = 0;
            csio.ccb_h.retry_count = 1;
            csio.ccb_h.cbfcnp      = IntPtr.Zero;
            csio.ccb_h.timeout     = timeout;
            csio.data_ptr          = Marshal.AllocHGlobal(buffer.Length);
            csio.dxfer_len         = (uint)buffer.Length;
            csio.sense_len         = 32;
            csio.cdb_len           = (byte)cdb.Length;

            // TODO: Create enum?
            csio.tag_action = 0x20;
            csio.cdb_bytes  = new byte[CAM_MAX_CDBLEN];

            if(cdb.Length <= CAM_MAX_CDBLEN)
                Array.Copy(cdb, 0, csio.cdb_bytes, 0, cdb.Length);
            else
            {
                cdbPtr = Marshal.AllocHGlobal(cdb.Length);
                byte[] cdbPtrBytes = BitConverter.GetBytes(cdbPtr.ToInt32());
                Array.Copy(cdbPtrBytes, 0, csio.cdb_bytes, 0, IntPtr.Size);
                csio.ccb_h.flags |= CcbFlags.CamCdbPointer;
            }

            csio.ccb_h.flags |= CcbFlags.CamDevQfrzdis;

            Marshal.Copy(buffer, 0, csio.data_ptr, buffer.Length);
            Marshal.StructureToPtr(csio, ccbPtr, false);

            DateTime start = DateTime.UtcNow;
            int      error = cam_send_ccb(dev, ccbPtr);
            DateTime end   = DateTime.UtcNow;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            csio = (CcbScsiio)Marshal.PtrToStructure(ccbPtr, typeof(CcbScsiio));

            if((csio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamReqCmp &&
               (csio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamScsiStatusError)
            {
                error = Marshal.GetLastWin32Error();
                AaruConsole.DebugWriteLine("FreeBSD devices", "CAM status {0} error {1}", csio.ccb_h.status, error);
                sense = true;
            }

            if((csio.ccb_h.status & CamStatus.CamStatusMask) == CamStatus.CamScsiStatusError)
            {
                sense          = true;
                senseBuffer    = new byte[1];
                senseBuffer[0] = csio.scsi_status;
            }

            if((csio.ccb_h.status & CamStatus.CamAutosnsValid) != 0)
                if(csio.sense_len - csio.sense_resid > 0)
                {
                    sense          = (csio.ccb_h.status & CamStatus.CamStatusMask) == CamStatus.CamScsiStatusError;
                    senseBuffer    = new byte[csio.sense_len - csio.sense_resid];
                    senseBuffer[0] = csio.sense_data.error_code;
                    Array.Copy(csio.sense_data.sense_buf, 0, senseBuffer, 1, senseBuffer.Length - 1);
                }

            buffer = new byte[csio.dxfer_len];
            cdb    = new byte[csio.cdb_len];

            Marshal.Copy(csio.data_ptr, buffer, 0, buffer.Length);

            if(csio.ccb_h.flags.HasFlag(CcbFlags.CamCdbPointer))
                Marshal.Copy(new IntPtr(BitConverter.ToInt32(csio.cdb_bytes, 0)), cdb, 0, cdb.Length);
            else
                Array.Copy(csio.cdb_bytes, 0, cdb, 0, cdb.Length);

            duration = (end - start).TotalMilliseconds;

            if(csio.ccb_h.flags.HasFlag(CcbFlags.CamCdbPointer))
                Marshal.FreeHGlobal(cdbPtr);

            Marshal.FreeHGlobal(csio.data_ptr);
            cam_freeccb(ccbPtr);

            return error;
        }

        /// <summary>Converts ATA protocol to CAM flags</summary>
        /// <param name="protocol">ATA protocol</param>
        /// <returns>CAM flags</returns>
        [Obsolete]
        static CcbFlags AtaProtocolToCamFlags(AtaProtocol protocol)
        {
            switch(protocol)
            {
                case AtaProtocol.DeviceDiagnostic:
                case AtaProtocol.DeviceReset:
                case AtaProtocol.HardReset:
                case AtaProtocol.NonData:
                case AtaProtocol.SoftReset:
                case AtaProtocol.ReturnResponse: return CcbFlags.CamDirNone;
                case AtaProtocol.PioIn:
                case AtaProtocol.UDmaIn: return CcbFlags.CamDirIn;
                case AtaProtocol.PioOut:
                case AtaProtocol.UDmaOut: return CcbFlags.CamDirOut;
                default: return CcbFlags.CamDirNone;
            }
        }

        /// <summary>Sends an ATA command in CHS mode</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="dev">CAM device</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA error returned non-OK status</param>
        /// <param name="registers">Registers to send to drive</param>
        /// <param name="errorRegisters">Registers returned by drive</param>
        /// <param name="protocol">ATA protocol to use</param>
        [Obsolete]
        internal static int SendAtaCommand(IntPtr dev, AtaRegistersChs registers,
                                           out AtaErrorRegistersChs errorRegisters, AtaProtocol protocol,
                                           ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration       = 0;
            sense          = false;
            errorRegisters = new AtaErrorRegistersChs();

            if(buffer == null)
                return -1;

            IntPtr ccbPtr = cam_getccb(dev);

            var ataio = (CcbAtaio)Marshal.PtrToStructure(ccbPtr, typeof(CcbAtaio));
            ataio.ccb_h.func_code   =  XptOpcode.XptAtaIo;
            ataio.ccb_h.flags       =  AtaProtocolToCamFlags(protocol);
            ataio.ccb_h.xflags      =  0;
            ataio.ccb_h.retry_count =  1;
            ataio.ccb_h.cbfcnp      =  IntPtr.Zero;
            ataio.ccb_h.timeout     =  timeout;
            ataio.data_ptr          =  Marshal.AllocHGlobal(buffer.Length);
            ataio.dxfer_len         =  (uint)buffer.Length;
            ataio.ccb_h.flags       |= CcbFlags.CamDevQfrzdis;
            ataio.cmd.flags         =  CamAtaIoFlags.NeedResult;

            switch(protocol)
            {
                case AtaProtocol.Dma:
                case AtaProtocol.DmaQueued:
                case AtaProtocol.UDmaIn:
                case AtaProtocol.UDmaOut:
                    ataio.cmd.flags |= CamAtaIoFlags.Dma;

                    break;
                case AtaProtocol.FpDma:
                    ataio.cmd.flags |= CamAtaIoFlags.Fpdma;

                    break;
            }

            ataio.cmd.command      = registers.Command;
            ataio.cmd.lba_high     = registers.CylinderHigh;
            ataio.cmd.lba_mid      = registers.CylinderLow;
            ataio.cmd.device       = (byte)(0x40 | registers.DeviceHead);
            ataio.cmd.features     = registers.Feature;
            ataio.cmd.sector_count = registers.SectorCount;
            ataio.cmd.lba_low      = registers.Sector;

            Marshal.Copy(buffer, 0, ataio.data_ptr, buffer.Length);
            Marshal.StructureToPtr(ataio, ccbPtr, false);

            DateTime start = DateTime.UtcNow;
            int      error = cam_send_ccb(dev, ccbPtr);
            DateTime end   = DateTime.UtcNow;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            ataio = (CcbAtaio)Marshal.PtrToStructure(ccbPtr, typeof(CcbAtaio));

            if((ataio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamReqCmp &&
               (ataio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamAtaStatusError)
            {
                error = Marshal.GetLastWin32Error();
                AaruConsole.DebugWriteLine("FreeBSD devices", "CAM status {0} error {1}", ataio.ccb_h.status, error);
                sense = true;
            }

            if((ataio.ccb_h.status & CamStatus.CamStatusMask) == CamStatus.CamAtaStatusError)
                sense = true;

            errorRegisters.CylinderHigh = ataio.res.lba_high;
            errorRegisters.CylinderLow  = ataio.res.lba_mid;
            errorRegisters.DeviceHead   = ataio.res.device;
            errorRegisters.Error        = ataio.res.error;
            errorRegisters.Sector       = ataio.res.lba_low;
            errorRegisters.SectorCount  = ataio.res.sector_count;
            errorRegisters.Status       = ataio.res.status;

            buffer = new byte[ataio.dxfer_len];

            Marshal.Copy(ataio.data_ptr, buffer, 0, buffer.Length);
            duration = (end - start).TotalMilliseconds;

            Marshal.FreeHGlobal(ataio.data_ptr);
            cam_freeccb(ccbPtr);

            sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0 || error != 0;

            return error;
        }

        /// <summary>Sends an ATA command in 28-bit LBA mode</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="dev">CAM device</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA error returned non-OK status</param>
        /// <param name="registers">Registers to send to drive</param>
        /// <param name="errorRegisters">Registers returned by drive</param>
        /// <param name="protocol">ATA protocol to use</param>
        [Obsolete]
        internal static int SendAtaCommand(IntPtr dev, AtaRegistersLba28 registers,
                                           out AtaErrorRegistersLba28 errorRegisters, AtaProtocol protocol,
                                           ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration       = 0;
            sense          = false;
            errorRegisters = new AtaErrorRegistersLba28();

            if(buffer == null)
                return -1;

            IntPtr ccbPtr = cam_getccb(dev);

            var ataio = (CcbAtaio)Marshal.PtrToStructure(ccbPtr, typeof(CcbAtaio));
            ataio.ccb_h.func_code   =  XptOpcode.XptAtaIo;
            ataio.ccb_h.flags       =  AtaProtocolToCamFlags(protocol);
            ataio.ccb_h.xflags      =  0;
            ataio.ccb_h.retry_count =  1;
            ataio.ccb_h.cbfcnp      =  IntPtr.Zero;
            ataio.ccb_h.timeout     =  timeout;
            ataio.data_ptr          =  Marshal.AllocHGlobal(buffer.Length);
            ataio.dxfer_len         =  (uint)buffer.Length;
            ataio.ccb_h.flags       |= CcbFlags.CamDevQfrzdis;
            ataio.cmd.flags         =  CamAtaIoFlags.NeedResult;

            switch(protocol)
            {
                case AtaProtocol.Dma:
                case AtaProtocol.DmaQueued:
                case AtaProtocol.UDmaIn:
                case AtaProtocol.UDmaOut:
                    ataio.cmd.flags |= CamAtaIoFlags.Dma;

                    break;
                case AtaProtocol.FpDma:
                    ataio.cmd.flags |= CamAtaIoFlags.Fpdma;

                    break;
            }

            ataio.cmd.command      = registers.Command;
            ataio.cmd.lba_high     = registers.LbaHigh;
            ataio.cmd.lba_mid      = registers.LbaMid;
            ataio.cmd.device       = (byte)(0x40 | registers.DeviceHead);
            ataio.cmd.features     = registers.Feature;
            ataio.cmd.sector_count = registers.SectorCount;
            ataio.cmd.lba_low      = registers.LbaLow;

            Marshal.Copy(buffer, 0, ataio.data_ptr, buffer.Length);
            Marshal.StructureToPtr(ataio, ccbPtr, false);

            DateTime start = DateTime.UtcNow;
            int      error = cam_send_ccb(dev, ccbPtr);
            DateTime end   = DateTime.UtcNow;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            ataio = (CcbAtaio)Marshal.PtrToStructure(ccbPtr, typeof(CcbAtaio));

            if((ataio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamReqCmp &&
               (ataio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamAtaStatusError)
            {
                error = Marshal.GetLastWin32Error();
                AaruConsole.DebugWriteLine("FreeBSD devices", "CAM status {0} error {1}", ataio.ccb_h.status, error);
                sense = true;
            }

            if((ataio.ccb_h.status & CamStatus.CamStatusMask) == CamStatus.CamAtaStatusError)
                sense = true;

            errorRegisters.LbaHigh     = ataio.res.lba_high;
            errorRegisters.LbaMid      = ataio.res.lba_mid;
            errorRegisters.DeviceHead  = ataio.res.device;
            errorRegisters.Error       = ataio.res.error;
            errorRegisters.LbaLow      = ataio.res.lba_low;
            errorRegisters.SectorCount = ataio.res.sector_count;
            errorRegisters.Status      = ataio.res.status;

            buffer = new byte[ataio.dxfer_len];

            Marshal.Copy(ataio.data_ptr, buffer, 0, buffer.Length);
            duration = (end - start).TotalMilliseconds;

            Marshal.FreeHGlobal(ataio.data_ptr);
            cam_freeccb(ccbPtr);

            sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0 || error != 0;

            return error;
        }

        /// <summary>Sends an ATA command in 48-bit mode</summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="dev">CAM device</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if ATA error returned non-OK status</param>
        /// <param name="registers">Registers to send to drive</param>
        /// <param name="errorRegisters">Registers returned by drive</param>
        /// <param name="protocol">ATA protocol to use</param>
        [Obsolete]
        internal static int SendAtaCommand(IntPtr dev, AtaRegistersLba48 registers,
                                           out AtaErrorRegistersLba48 errorRegisters, AtaProtocol protocol,
                                           ref byte[] buffer, uint timeout, out double duration, out bool sense)
        {
            duration       = 0;
            sense          = false;
            errorRegisters = new AtaErrorRegistersLba48();

            // 48-bit ATA CAM commands can crash FreeBSD < 9.2-RELEASE
            if((Environment.Version.Major == 9 && Environment.Version.Minor < 2) ||
               Environment.Version.Major < 9)
                return -1;

            if(buffer == null)
                return -1;

            IntPtr ccbPtr = cam_getccb(dev);

            var ataio = (CcbAtaio)Marshal.PtrToStructure(ccbPtr, typeof(CcbAtaio));
            ataio.ccb_h.func_code   =  XptOpcode.XptAtaIo;
            ataio.ccb_h.flags       =  AtaProtocolToCamFlags(protocol);
            ataio.ccb_h.xflags      =  0;
            ataio.ccb_h.retry_count =  1;
            ataio.ccb_h.cbfcnp      =  IntPtr.Zero;
            ataio.ccb_h.timeout     =  timeout;
            ataio.data_ptr          =  Marshal.AllocHGlobal(buffer.Length);
            ataio.dxfer_len         =  (uint)buffer.Length;
            ataio.ccb_h.flags       |= CcbFlags.CamDevQfrzdis;
            ataio.cmd.flags         =  CamAtaIoFlags.NeedResult | CamAtaIoFlags.ExtendedCommand;

            switch(protocol)
            {
                case AtaProtocol.Dma:
                case AtaProtocol.DmaQueued:
                case AtaProtocol.UDmaIn:
                case AtaProtocol.UDmaOut:
                    ataio.cmd.flags |= CamAtaIoFlags.Dma;

                    break;
                case AtaProtocol.FpDma:
                    ataio.cmd.flags |= CamAtaIoFlags.Fpdma;

                    break;
            }

            ataio.cmd.lba_high_exp     = registers.LbaHighCurrent;
            ataio.cmd.lba_mid_exp      = registers.LbaMidCurrent;
            ataio.cmd.features_exp     = (byte)((registers.Feature     & 0xFF00) >> 8);
            ataio.cmd.sector_count_exp = (byte)((registers.SectorCount & 0xFF00) >> 8);
            ataio.cmd.lba_low_exp      = registers.LbaLowCurrent;
            ataio.cmd.lba_high         = registers.LbaHighPrevious;
            ataio.cmd.lba_mid          = registers.LbaMidPrevious;
            ataio.cmd.features         = (byte)(registers.Feature     & 0xFF);
            ataio.cmd.sector_count     = (byte)(registers.SectorCount & 0xFF);
            ataio.cmd.lba_low          = registers.LbaLowPrevious;
            ataio.cmd.command          = registers.Command;
            ataio.cmd.device           = (byte)(0x40 | registers.DeviceHead);

            Marshal.Copy(buffer, 0, ataio.data_ptr, buffer.Length);
            Marshal.StructureToPtr(ataio, ccbPtr, false);

            DateTime start = DateTime.UtcNow;
            int      error = cam_send_ccb(dev, ccbPtr);
            DateTime end   = DateTime.UtcNow;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            ataio = (CcbAtaio)Marshal.PtrToStructure(ccbPtr, typeof(CcbAtaio));

            if((ataio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamReqCmp &&
               (ataio.ccb_h.status & CamStatus.CamStatusMask) != CamStatus.CamAtaStatusError)
            {
                error = Marshal.GetLastWin32Error();
                AaruConsole.DebugWriteLine("FreeBSD devices", "CAM status {0} error {1}", ataio.ccb_h.status, error);
                sense = true;
            }

            if((ataio.ccb_h.status & CamStatus.CamStatusMask) == CamStatus.CamAtaStatusError)
                sense = true;

            errorRegisters.SectorCount     = (ushort)((ataio.res.sector_count_exp << 8) + ataio.res.sector_count);
            errorRegisters.LbaLowCurrent   = ataio.res.lba_low_exp;
            errorRegisters.LbaMidCurrent   = ataio.res.lba_mid_exp;
            errorRegisters.LbaHighCurrent  = ataio.res.lba_high_exp;
            errorRegisters.LbaLowPrevious  = ataio.res.lba_low;
            errorRegisters.LbaMidPrevious  = ataio.res.lba_mid;
            errorRegisters.LbaHighPrevious = ataio.res.lba_high;
            errorRegisters.DeviceHead      = ataio.res.device;
            errorRegisters.Error           = ataio.res.error;
            errorRegisters.Status          = ataio.res.status;

            buffer = new byte[ataio.dxfer_len];

            Marshal.Copy(ataio.data_ptr, buffer, 0, buffer.Length);
            duration = (end - start).TotalMilliseconds;

            Marshal.FreeHGlobal(ataio.data_ptr);
            cam_freeccb(ccbPtr);

            sense = errorRegisters.Error != 0 || (errorRegisters.Status & 0xA5) != 0 || error != 0;

            return error;
        }
    }
}