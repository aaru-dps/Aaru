using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.Windows
{
    static class Command
    {
        internal static int SendScsiCommand(SafeFileHandle fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, ScsiIoctlDirection direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration = 0;
            sense = false;

            if (buffer == null)
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

            if (hasError)
                error = Marshal.GetLastWin32Error();

            Marshal.Copy(sptd_sb.sptd.DataBuffer, buffer, 0, buffer.Length);

            sense |= sptd_sb.sptd.ScsiStatus != 0;

            senseBuffer = new byte[32];
            Array.Copy(sptd_sb.SenseBuf, senseBuffer, 32);

            duration = (end - start).TotalMilliseconds;

            return error;
        }
    }
}

