using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.Remote
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct DicPacketHeader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string id;

        public uint len;
        public byte version;
        public DicPacketType packetType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] spare;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct DicPacketHello
    {
        public DicPacketHeader hdr;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string application;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string version;

        public byte maxProtocol;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] spare;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string sysname;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string release;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string machine;
    }
}