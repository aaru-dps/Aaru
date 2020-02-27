using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.Decoders.ATA;

namespace Aaru.Devices.Remote
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketHeader
    {
        public uint remote_id;
        public uint packet_id;

        public uint len;
        public byte version;
        public AaruPacketType packetType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] spare;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketHello
    {
        public AaruPacketHeader hdr;

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
    public struct AaruPacketCommandListDevices
    {
        public AaruPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResponseListDevices
    {
        public readonly AaruPacketHeader hdr;
        public readonly ushort devices;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketNop
    {
        public AaruPacketHeader hdr;
        public AaruNopReason reasonCode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] spare;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string reason;

        public int errno;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCommandOpenDevice
    {
        public AaruPacketHeader hdr;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string device_path;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdScsi
    {
        public AaruPacketHeader hdr;
        public uint cdb_len;
        public uint buf_len;
        public int direction;
        public uint timeout;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResScsi
    {
        public AaruPacketHeader hdr;
        public uint sense_len;
        public uint buf_len;
        public uint duration;
        public uint sense;
        public uint error_no;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdAtaChs
    {
        public AaruPacketHeader hdr;
        public uint buf_len;
        public AtaRegistersChs registers;
        public byte protocol;
        public byte transferRegister;
        [MarshalAs(UnmanagedType.U1)] public bool transferBlocks;
        public byte spare;
        public uint timeout;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResAtaChs
    {
        public AaruPacketHeader hdr;
        public uint buf_len;
        public AtaErrorRegistersChs registers;
        public uint duration;
        public uint sense;
        public uint error_no;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdAtaLba28
    {
        public AaruPacketHeader hdr;
        public uint buf_len;
        public AtaRegistersLba28 registers;
        public byte protocol;
        public byte transferRegister;
        [MarshalAs(UnmanagedType.U1)] public bool transferBlocks;
        public byte spare;
        public uint timeout;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResAtaLba28
    {
        public AaruPacketHeader hdr;
        public uint buf_len;
        public AtaErrorRegistersLba28 registers;
        public uint duration;
        public uint sense;
        public uint error_no;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdAtaLba48
    {
        public AaruPacketHeader hdr;
        public uint buf_len;
        public AtaRegistersLba48 registers;
        public byte protocol;
        public byte transferRegister;
        [MarshalAs(UnmanagedType.U1)] public bool transferBlocks;
        public byte spare;
        public uint timeout;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResAtaLba48
    {
        public AaruPacketHeader hdr;
        public uint buf_len;
        public AtaErrorRegistersLba48 registers;
        public uint duration;
        public uint sense;
        public uint error_no;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdSdhci
    {
        public AaruPacketHeader hdr;
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
    public struct AaruPacketResSdhci
    {
        public AaruPacketHeader hdr;
        public uint buf_len;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] response;

        public uint duration;
        public uint sense;
        public uint error_no;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdGetDeviceType
    {
        public AaruPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResGetDeviceType
    {
        public AaruPacketHeader hdr;
        public DeviceType device_type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdGetSdhciRegisters
    {
        public AaruPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResGetSdhciRegisters
    {
        public AaruPacketHeader hdr;
        [MarshalAs(UnmanagedType.U1)] public bool isSdhci;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] csd;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] cid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] ocr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] scr;

        public uint csd_len;
        public uint cid_len;
        public uint ocr_len;
        public uint scr_len;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdGetUsbData
    {
        public AaruPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResGetUsbData
    {
        public AaruPacketHeader hdr;
        [MarshalAs(UnmanagedType.U1)] public bool isUsb;
        public ushort descLen;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65536)]
        public byte[] descriptors;

        public ushort idVendor;
        public ushort idProduct;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string manufacturer;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string product;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string serial;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdGetFireWireData
    {
        public AaruPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResGetFireWireData
    {
        public AaruPacketHeader hdr;
        [MarshalAs(UnmanagedType.U1)] public bool isFireWire;
        public uint idModel;
        public uint idVendor;
        public ulong guid;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string vendor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string model;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdGetPcmciaData
    {
        public AaruPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResGetPcmciaData
    {
        public AaruPacketHeader hdr;
        [MarshalAs(UnmanagedType.U1)] public bool isPcmcia;
        public ushort cis_len;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65536)]
        public byte[] cis;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdClose
    {
        public AaruPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketCmdAmIRoot
    {
        public AaruPacketHeader hdr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AaruPacketResAmIRoot
    {
        public AaruPacketHeader hdr;
        public uint am_i_root;
    }
}