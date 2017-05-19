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

            DateTime start = DateTime.Now;
            bool hasError = Extern.DeviceIoControlScsi(fd, WindowsIoctl.IOCTL_SCSI_PASS_THROUGH_DIRECT, ref sptd_sb, (uint)Marshal.SizeOf(sptd_sb), ref sptd_sb,
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
    }
}

