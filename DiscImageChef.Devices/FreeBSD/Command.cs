// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Command.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FeeBSD direct device access.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using DiscImageChef.Console;
using DiscImageChef.Devices.Linux;
using static DiscImageChef.Devices.FreeBSD.Extern;

namespace DiscImageChef.Devices.FreeBSD
{
    static class Command
    {
        const int CAM_MAX_CDBLEN = 16;

        /// <summary>
        /// Sends a SCSI command (64-bit arch)
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="dev">CAM device</param>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if SCSI error returned non-OK status and <paramref name="senseBuffer"/> contains SCSI sense</param>
        internal static int SendScsiCommand64(IntPtr dev, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, ccb_flags direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration = 0;
            sense = false;

            if(buffer == null)
                return -1;

            IntPtr ccbPtr = cam_getccb(dev);
            IntPtr cdbPtr = IntPtr.Zero;

            if(ccbPtr.ToInt64() == 0)
            {
                sense = true;
                return Marshal.GetLastWin32Error();
            }

            ccb_scsiio64 csio = (ccb_scsiio64)Marshal.PtrToStructure(ccbPtr, typeof(ccb_scsiio64));
            csio.ccb_h.func_code = xpt_opcode.XPT_SCSI_IO;
            csio.ccb_h.flags = direction;
            csio.ccb_h.xflags = 0;
            csio.ccb_h.retry_count = 1;
            csio.ccb_h.cbfcnp = IntPtr.Zero;
            csio.ccb_h.timeout = timeout;
            csio.data_ptr = Marshal.AllocHGlobal(buffer.Length);
            csio.dxfer_len = (uint)buffer.Length;
            csio.sense_len = 32;
            csio.cdb_len = (byte)cdb.Length;
            // TODO: Create enum?
            csio.tag_action = 0x20;
            csio.cdb_bytes = new byte[CAM_MAX_CDBLEN];
            if(cdb.Length <= CAM_MAX_CDBLEN)
                Array.Copy(cdb, 0, csio.cdb_bytes, 0, cdb.Length);
            else
            {
                cdbPtr = Marshal.AllocHGlobal(cdb.Length);
                byte[] cdbPtrBytes = BitConverter.GetBytes(cdbPtr.ToInt64());
                Array.Copy(cdbPtrBytes, 0, csio.cdb_bytes, 0, IntPtr.Size);
                csio.ccb_h.flags |= ccb_flags.CAM_CDB_POINTER;
            }
            csio.ccb_h.flags |= ccb_flags.CAM_DEV_QFRZDIS;

            Marshal.Copy(buffer, 0, csio.data_ptr, buffer.Length);
            Marshal.StructureToPtr(csio, ccbPtr, false);

            DateTime start = DateTime.UtcNow;
            int error = cam_send_ccb(dev, ccbPtr);
            DateTime end = DateTime.UtcNow;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            csio = (ccb_scsiio64)Marshal.PtrToStructure(ccbPtr, typeof(ccb_scsiio64));

            if((csio.ccb_h.status & cam_status.CAM_STATUS_MASK) != cam_status.CAM_REQ_CMP &&
               (csio.ccb_h.status & cam_status.CAM_STATUS_MASK) != cam_status.CAM_SCSI_STATUS_ERROR)
            {
                error = Marshal.GetLastWin32Error();
                DicConsole.DebugWriteLine("FreeBSD devices", "CAM status {0} error {1}", csio.ccb_h.status, error);
                sense = true;
            }

            if((csio.ccb_h.status & cam_status.CAM_STATUS_MASK) == cam_status.CAM_SCSI_STATUS_ERROR)
            {
                sense = true;
                senseBuffer = new byte[1];
                senseBuffer[0] = csio.scsi_status;
            }

            if((csio.ccb_h.status & cam_status.CAM_AUTOSNS_VALID) != 0)
            {
                if(csio.sense_len - csio.sense_resid > 0)
                {
                    sense = (csio.ccb_h.status & cam_status.CAM_STATUS_MASK) == cam_status.CAM_SCSI_STATUS_ERROR;
                    senseBuffer = new byte[csio.sense_len - csio.sense_resid];
                    senseBuffer[0] = csio.sense_data.error_code;
                    Array.Copy(csio.sense_data.sense_buf, 0, senseBuffer, 1, senseBuffer.Length - 1);
                }
            }

            buffer = new byte[csio.dxfer_len];
            cdb = new byte[csio.cdb_len];

            Marshal.Copy(csio.data_ptr, buffer, 0, buffer.Length);
            if(csio.ccb_h.flags.HasFlag(ccb_flags.CAM_CDB_POINTER))
                Marshal.Copy(new IntPtr(BitConverter.ToInt64(csio.cdb_bytes, 0)), cdb, 0, cdb.Length);
            else
                Array.Copy(csio.cdb_bytes, 0, cdb, 0, cdb.Length);
            duration = (end - start).TotalMilliseconds;

            if(csio.ccb_h.flags.HasFlag(ccb_flags.CAM_CDB_POINTER))
                Marshal.FreeHGlobal(cdbPtr);
            Marshal.FreeHGlobal(csio.data_ptr);
            cam_freeccb(ccbPtr);

            return error;
        }

        /// <summary>
        /// Sends a SCSI command (32-bit arch)
        /// </summary>
        /// <returns>0 if no error occurred, otherwise, errno</returns>
        /// <param name="dev">CAM device</param>
        /// <param name="cdb">SCSI CDB</param>
        /// <param name="buffer">Buffer for SCSI command response</param>
        /// <param name="senseBuffer">Buffer with the SCSI sense</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="direction">SCSI command transfer direction</param>
        /// <param name="duration">Time it took to execute the command in milliseconds</param>
        /// <param name="sense"><c>True</c> if SCSI error returned non-OK status and <paramref name="senseBuffer"/> contains SCSI sense</param>
        internal static int SendScsiCommand(IntPtr dev, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, ccb_flags direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration = 0;
            sense = false;

            if(buffer == null)
                return -1;

            IntPtr ccbPtr = cam_getccb(dev);
            IntPtr cdbPtr = IntPtr.Zero;

            if(ccbPtr.ToInt32() == 0)
            {
                sense = true;
                return Marshal.GetLastWin32Error();
            }

            ccb_scsiio csio = (ccb_scsiio)Marshal.PtrToStructure(ccbPtr, typeof(ccb_scsiio));
            csio.ccb_h.func_code = xpt_opcode.XPT_SCSI_IO;
            csio.ccb_h.flags = direction;
            csio.ccb_h.xflags = 0;
            csio.ccb_h.retry_count = 1;
            csio.ccb_h.cbfcnp = IntPtr.Zero;
            csio.ccb_h.timeout = timeout;
            csio.data_ptr = Marshal.AllocHGlobal(buffer.Length);
            csio.dxfer_len = (uint)buffer.Length;
            csio.sense_len = 32;
            csio.cdb_len = (byte)cdb.Length;
            // TODO: Create enum?
            csio.tag_action = 0x20;
            csio.cdb_bytes = new byte[CAM_MAX_CDBLEN];
            if(cdb.Length <= CAM_MAX_CDBLEN)
                Array.Copy(cdb, 0, csio.cdb_bytes, 0, cdb.Length);
            else
            {
                cdbPtr = Marshal.AllocHGlobal(cdb.Length);
                byte[] cdbPtrBytes = BitConverter.GetBytes(cdbPtr.ToInt32());
                Array.Copy(cdbPtrBytes, 0, csio.cdb_bytes, 0, IntPtr.Size);
                csio.ccb_h.flags |= ccb_flags.CAM_CDB_POINTER;
            }
            csio.ccb_h.flags |= ccb_flags.CAM_DEV_QFRZDIS;

            Marshal.Copy(buffer, 0, csio.data_ptr, buffer.Length);
            Marshal.StructureToPtr(csio, ccbPtr, false);

            DateTime start = DateTime.UtcNow;
            int error = cam_send_ccb(dev, ccbPtr);
            DateTime end = DateTime.UtcNow;

            if(error < 0)
                error = Marshal.GetLastWin32Error();

            csio = (ccb_scsiio)Marshal.PtrToStructure(ccbPtr, typeof(ccb_scsiio));

            if((csio.ccb_h.status & cam_status.CAM_STATUS_MASK) != cam_status.CAM_REQ_CMP &&
               (csio.ccb_h.status & cam_status.CAM_STATUS_MASK) != cam_status.CAM_SCSI_STATUS_ERROR)
            {
                error = Marshal.GetLastWin32Error();
                DicConsole.DebugWriteLine("FreeBSD devices", "CAM status {0} error {1}", csio.ccb_h.status, error);
                sense = true;
            }

            if((csio.ccb_h.status & cam_status.CAM_STATUS_MASK) == cam_status.CAM_SCSI_STATUS_ERROR)
            {
                sense = true;
                senseBuffer = new byte[1];
                senseBuffer[0] = csio.scsi_status;
            }

            if((csio.ccb_h.status & cam_status.CAM_AUTOSNS_VALID) != 0)
            {
                if(csio.sense_len - csio.sense_resid > 0)
                {
                    sense = (csio.ccb_h.status & cam_status.CAM_STATUS_MASK) == cam_status.CAM_SCSI_STATUS_ERROR;
                    senseBuffer = new byte[csio.sense_len - csio.sense_resid];
                    senseBuffer[0] = csio.sense_data.error_code;
                    Array.Copy(csio.sense_data.sense_buf, 0, senseBuffer, 1, senseBuffer.Length - 1);
                }
            }

            buffer = new byte[csio.dxfer_len];
            cdb = new byte[csio.cdb_len];

            Marshal.Copy(csio.data_ptr, buffer, 0, buffer.Length);
            if(csio.ccb_h.flags.HasFlag(ccb_flags.CAM_CDB_POINTER))
                Marshal.Copy(new IntPtr(BitConverter.ToInt32(csio.cdb_bytes, 0)), cdb, 0, cdb.Length);
            else
                Array.Copy(csio.cdb_bytes, 0, cdb, 0, cdb.Length);
            duration = (end - start).TotalMilliseconds;

            if(csio.ccb_h.flags.HasFlag(ccb_flags.CAM_CDB_POINTER))
                Marshal.FreeHGlobal(cdbPtr);
            Marshal.FreeHGlobal(csio.data_ptr);
            cam_freeccb(ccbPtr);

            return error;
        }
    }
}

