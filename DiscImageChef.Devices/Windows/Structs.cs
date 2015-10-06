using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.Windows
{
    static class Structs
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SCSI_PASS_THROUGH_DIRECT
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
        internal struct SCSI_PASS_THROUGH_DIRECT_AND_SENSE_BUFFER {
            public SCSI_PASS_THROUGH_DIRECT sptd;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] SenseBuf;
        }
    }
}

