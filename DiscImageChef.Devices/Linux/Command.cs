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

