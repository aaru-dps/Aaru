using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Decoders.ATA;

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
        public DicNopReason reasonCode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] spare;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string reason;

        public int errno;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketCommandOpenDevice
    {
        public DicPacketHeader hdr;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string device_path;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketCmdScsi
    {
        public DicPacketHeader hdr;
        public uint cdb_len;
        public uint buf_len;
        public int direction;
        public uint timeout;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketResScsi
    {
        public DicPacketHeader hdr;
        public uint sense_len;
        public uint buf_len;
        public uint duration;
        public uint sense;
        public uint error_no;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketCmdAtaChs
    {
        public DicPacketHeader hdr;
        public uint buf_len;
        public AtaRegistersChs registers;
        public byte protocol;
        public byte transferRegister;
        public byte transferBlocks;
        public byte spare;
        public uint timeout;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketResAtaChs
    {
        public DicPacketHeader hdr;
        public uint buf_len;
        public AtaErrorRegistersChs registers;
        public uint duration;
        public uint sense;
        public uint error_no;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketCmdAtaLba28
    {
        public DicPacketHeader hdr;
        public uint buf_len;
        public AtaRegistersLba28 registers;
        public byte protocol;
        public byte transferRegister;
        public byte transferBlocks;
        public byte spare;
        public uint timeout;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketResAtaLba28
    {
        public DicPacketHeader hdr;
        public uint buf_len;
        public AtaErrorRegistersLba28 registers;
        public uint duration;
        public uint sense;
        public uint error_no;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketCmdAtaLba48
    {
        public DicPacketHeader hdr;
        public uint buf_len;
        public AtaRegistersLba48 registers;
        public byte protocol;
        public byte transferRegister;
        public byte transferBlocks;
        public byte spare;
        public uint timeout;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketResAtaLba48
    {
        public DicPacketHeader hdr;
        public uint buf_len;
        public AtaErrorRegistersLba48 registers;
        public uint duration;
        public uint sense;
        public uint error_no;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketCmdSdhci
    {
        public DicPacketHeader hdr;
        public MmcCommands command;
        [MarshalAs(UnmanagedType.U1)] public bool write;
        [MarshalAs(UnmanagedType.U1)] public bool application;
        public MmcFlags flags;
        public uint argument;
        public uint block_size;
        public uint blocks;
        public uint buf_len;
        public uint timeout;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketResSdhci
    {
        public DicPacketHeader hdr;
        public uint buf_len;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] response;

        public uint duration;
        public uint sense;
        public uint error_no;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketCmdGetDeviceType
    {
        public DicPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DicPacketResGetDeviceType
    {
        public DicPacketHeader hdr;
        public DeviceType device_type;
    }

    public struct DicPacketCmdGetSdhciRegisters
    {
        public DicPacketHeader hdr;
    }

    public struct DicPacketResGetSdhciRegisters
    {
        public DicPacketHeader hdr;
        [MarshalAs(UnmanagedType.U1)] public bool isSdhci;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] csd;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] cid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] ocr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] scr;
    }

    public struct DicPacketCmdGetUsbData
    {
        public DicPacketHeader hdr;
    }

    public struct DicPacketResGetUsbData
    {
        public DicPacketHeader hdr;
        [MarshalAs(UnmanagedType.U1)] public bool isUsb;
        public ushort descLen;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65536)]
        public byte[] descriptors;

        public ushort idVendor;
        public ushort idProduct;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public string manufacturer;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public string product;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public string serial;
    }

    public struct DicPacketCmdGetFireWireData
    {
        public DicPacketHeader hdr;
    }

    public struct DicPacketResGetFireWireData
    {
        public DicPacketHeader hdr;
        [MarshalAs(UnmanagedType.U1)] public bool isFireWire;
        public uint idModel;
        public uint idVendor;
        public ulong guid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public string vendor;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public string model;
    }

    public struct DicPacketCmdGetPcmciaData
    {
        public DicPacketHeader hdr;
    }

    public struct DicPacketResGetPcmciaData
    {
        public DicPacketHeader hdr;
        [MarshalAs(UnmanagedType.U1)] public bool isPcmcia;
        public ushort cis_len;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65536)]
        public byte[] cis;
    }
}