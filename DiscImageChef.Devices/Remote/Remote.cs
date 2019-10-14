using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using Marshal = DiscImageChef.Helpers.Marshal;
using Version = DiscImageChef.CommonTypes.Interop.Version;

namespace DiscImageChef.Devices.Remote
{
    public class Remote : IDisposable
    {
        private readonly string _host;
        private readonly Socket _socket;

        public Remote(string host)
        {
            _host = host;
            var ipHostEntry = Dns.GetHostEntry(host);
            var ipAddress = ipHostEntry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            if (ipAddress is null)
            {
                DicConsole.ErrorWriteLine("Host not found");
                throw new SocketException(11001);
            }

            var ipEndPoint = new IPEndPoint(ipAddress, 6666);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _socket.Connect(ipEndPoint);

            DicConsole.WriteLine("Connected to {0}", host);

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            var len = _socket.Receive(hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                throw new IOException();
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                throw new ArgumentException();
            }

            byte[] buf;

            if (hdr.packetType != DicPacketType.Hello)
            {
                if (hdr.packetType != DicPacketType.Nop)
                {
                    DicConsole.ErrorWriteLine("Expected Hello Packet, got packet type {0}...", hdr.packetType);
                    throw new ArgumentException();
                }

                buf = new byte[hdr.len];
                len = _socket.Receive(buf, buf.Length, SocketFlags.None);

                if (len < buf.Length)
                {
                    DicConsole.ErrorWriteLine("Could not read from the network...");
                    throw new IOException();
                }

                var nop = Marshal.ByteArrayToStructureLittleEndian<DicPacketNop>(buf);

                DicConsole.ErrorWriteLine($"{nop.reason}");
                throw new ArgumentException();
            }

            if (hdr.version != Consts.PacketVersion)
            {
                DicConsole.ErrorWriteLine("Unrecognized packet version...");
                throw new ArgumentException();
            }

            buf = new byte[hdr.len];
            len = _socket.Receive(buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                throw new IOException();
            }

            var serverHello = Marshal.ByteArrayToStructureLittleEndian<DicPacketHello>(buf);

            ServerApplication = serverHello.application;
            ServerVersion = serverHello.version;
            ServerOperatingSystem = serverHello.sysname;
            ServerOperatingSystemVersion = serverHello.release;
            ServerArchitecture = serverHello.machine;
            ServerProtocolVersion = serverHello.maxProtocol;

            var clientHello = new DicPacketHello
            {
                application = "DiscImageChef",
                version = Version.GetVersion(),
                maxProtocol = Consts.MaxProtocol,
                sysname = DetectOS.GetPlatformName(
                    DetectOS.GetRealPlatformID(), DetectOS.GetVersion()),
                release = DetectOS.GetVersion(),
                machine = RuntimeInformation.ProcessArchitecture.ToString(),
                hdr = new DicPacketHeader
                {
                    id = Consts.PacketId,
                    len = (uint) Marshal.SizeOf<DicPacketHello>(),
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.Hello
                }
            };

            buf = Marshal.StructureToByteArrayLittleEndian(clientHello);

            len = _socket.Send(buf, SocketFlags.None);

            if (len >= buf.Length) return;

            DicConsole.ErrorWriteLine("Could not write to the network...");
            throw new IOException();
        }

        public string ServerApplication { get; }
        public string ServerVersion { get; }
        public string ServerOperatingSystem { get; }
        public string ServerOperatingSystemVersion { get; }
        public string ServerArchitecture { get; }
        public int ServerProtocolVersion { get; }

        public void Dispose()
        {
            Disconnect();
        }

        public void Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public DeviceInfo[] ListDevices()
        {
            var cmdPkt = new DicPacketCommandListDevices
            {
                hdr = new DicPacketHeader
                {
                    id = Consts.PacketId,
                    len = (uint) Marshal.SizeOf<DicPacketCommandListDevices>(),
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandListDevices
                }
            };

            var buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");

                return new DeviceInfo[0];
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = _socket.Receive(hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return new DeviceInfo[0];
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return new DeviceInfo[0];
            }

            if (hdr.packetType != DicPacketType.ResponseListDevices)
            {
                if (hdr.packetType != DicPacketType.Nop)
                {
                    DicConsole.ErrorWriteLine("Expected List Devices Response Packet, got packet type {0}...",
                        hdr.packetType);
                    return new DeviceInfo[0];
                }

                buf = new byte[hdr.len];
                len = _socket.Receive(buf, buf.Length, SocketFlags.None);

                if (len < buf.Length)
                {
                    DicConsole.ErrorWriteLine("Could not read from the network...");
                    return new DeviceInfo[0];
                }

                var nop = Marshal.ByteArrayToStructureLittleEndian<DicPacketNop>(buf);

                DicConsole.ErrorWriteLine($"{nop.reason}");
                return new DeviceInfo[0];
            }

            if (hdr.version != Consts.PacketVersion)
            {
                DicConsole.ErrorWriteLine("Unrecognized packet version...");
                return new DeviceInfo[0];
            }

            buf = new byte[hdr.len];
            len = _socket.Receive(buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return new DeviceInfo[0];
            }

            var response = Marshal.ByteArrayToStructureLittleEndian<DicPacketResponseListDevices>(buf);
            var devices = new List<DeviceInfo>();
            var offset = Marshal.SizeOf<DicPacketResponseListDevices>();
            var devInfoLen = Marshal.SizeOf<DeviceInfo>();

            for (ushort i = 0; i < response.devices; i++)
            {
                var dev = Marshal.ByteArrayToStructureLittleEndian<DeviceInfo>(buf, offset, devInfoLen);
                dev.Path = dev.Path[0] == '/' ? $"dic://{_host}{dev.Path}" : $"dic://{_host}/{dev.Path}";
                devices.Add(dev);
                offset += devInfoLen;
            }

            return devices.ToArray();
        }

        public bool Open(string devicePath, out int LastError)
        {
            LastError = 0;

            var cmdPkt = new DicPacketCommandOpenDevice
            {
                hdr = new DicPacketHeader
                {
                    id = Consts.PacketId,
                    len = (uint) Marshal.SizeOf<DicPacketCommandOpenDevice>(),
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandOpen
                },
                device_path = devicePath
            };

            var buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                LastError = -1;
                return false;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = _socket.Receive(hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                LastError = -1;
                return false;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                LastError = -1;
                return false;
            }

            if (hdr.packetType != DicPacketType.Nop)
            {
                DicConsole.ErrorWriteLine("Expected List Devices Response Packet, got packet type {0}...",
                    hdr.packetType);
                LastError = -1;
                return false;
            }

            buf = new byte[hdr.len];
            len = _socket.Receive(buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                LastError = -1;
                return false;
            }

            var nop = Marshal.ByteArrayToStructureLittleEndian<DicPacketNop>(buf);

            switch (nop.reasonCode)
            {
                case DicNopReason.OpenOk:
                    return true;
                case DicNopReason.NotImplemented:
                    throw new NotImplementedException($"{nop.reason}");
            }

            DicConsole.ErrorWriteLine($"{nop.reason}");
            LastError = nop.errno;
            return false;
        }

        public int SendScsiCommand(byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout,
            ScsiDirection direction, out double duration, out bool sense)
        {
            throw new NotImplementedException("Remote SCSI commands not yet implemented...");
        }

        public int SendAtaCommand(AtaRegistersChs registers, out AtaErrorRegistersChs errorRegisters,
            AtaProtocol protocol, AtaTransferRegister transferRegister,
            ref byte[] buffer,
            uint timeout, bool transferBlocks,
            out double duration, out bool sense)
        {
            throw new NotImplementedException("Remote CHS ATA commands not yet implemented...");
        }

        public int SendAtaCommand(AtaRegistersLba28 registers, out AtaErrorRegistersLba28 errorRegisters,
            AtaProtocol protocol, AtaTransferRegister transferRegister,
            ref byte[] buffer,
            uint timeout, bool transferBlocks,
            out double duration, out bool sense)
        {
            throw new NotImplementedException("Remote 28-bit ATA commands not yet implemented...");
        }

        public int SendAtaCommand(AtaRegistersLba48 registers, out AtaErrorRegistersLba48 errorRegisters,
            AtaProtocol protocol, AtaTransferRegister transferRegister,
            ref byte[] buffer,
            uint timeout, bool transferBlocks,
            out double duration, out bool sense)
        {
            throw new NotImplementedException("Remote 48-bit ATA commands not yet implemented...");
        }

        public int SendMmcCommand(MmcCommands command, bool write, bool isApplication, MmcFlags flags,
            uint argument,
            uint blockSize, uint blocks, ref byte[] buffer, out uint[] response,
            out double duration, out bool sense, uint timeout = 0)
        {
            throw new NotImplementedException("Remote SDHCI commands not yet implemented...");
        }

        public DeviceType GetDeviceType()
        {
            throw new NotImplementedException("Getting remote device type not yet implemented...");
        }

        public bool GetSdhciRegisters(out byte[] csd, out byte[] cid, out byte[] ocr, out byte[] scr)
        {
            throw new NotImplementedException("Getting SDHCI registers not yet implemented...");
        }

        public bool GetUsbData(out byte[] descriptors, out ushort idVendor, out ushort idProduct,
            out string manufacturer, out string product, out string serial)
        {
            throw new NotImplementedException("Getting USB data not yet implemented...");
        }

        public bool GetFirewireData(out uint idVendor, out uint idProduct,
            out ulong guid, out string vendor, out string model)
        {
            throw new NotImplementedException("Getting FireWire data not yet implemented...");
        }

        public bool GetPcmciaData(out byte[] cis)
        {
            throw new NotImplementedException("Getting PCMCIA data not yet implemented...");
        }
    }
}