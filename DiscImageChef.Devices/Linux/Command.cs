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

namespace DiscImageChef.Devices.Linux
{
    static class Command
    {
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

            return error;
        }
    }
}

