using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.Remote
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketHeader
    {
        public ulong id;

        public uint len;
        public byte version;
        public DicPacketType packetType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] spare;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketHello
    {
        public DicPacketHeader hdr;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string application;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string version;

        public byte maxProtocol;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] spare;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string sysname;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string release;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string machine;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketCommandListDevices
    {
        public DicPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketResponseListDevices
    {
        public readonly DicPacketHeader hdr;
        public readonly ushort devices;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketNop
    {
        public DicPacketHeader hdr;
        public byte reasonCode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] spare;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string reason;
    }
}