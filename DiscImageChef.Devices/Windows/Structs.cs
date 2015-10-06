using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    struct ScsiPassThroughDirect
    {
        public ushort Length;
        public byte ScsiStatus;
        public byte PathId;
        public byte TargetId;
        public byte Lun;
        public byte CdbLength;
        public byte SenseInfoLength;
        [MarshalAs(UnmanagedType.U1)]
        public ScsiIoctlDirection DataIn;
        public uint DataTransferLength;
        public uint TimeOutValue;
        public IntPtr DataBuffer;
        public uint SenseInfoOffset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Cdb;    
    };

    [StructLayout(LayoutKind.Sequential)]
    struct ScsiPassThroughDirectAndSenseBuffer {
        public ScsiPassThroughDirect sptd;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] SenseBuf;
    }
}

