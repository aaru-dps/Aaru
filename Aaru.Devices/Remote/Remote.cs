using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interop;
using Aaru.Console;
using Aaru.Decoders.ATA;
using Marshal = Aaru.Helpers.Marshal;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Devices.Remote
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

            var len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                throw new IOException();
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
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
                len = Receive(_socket, buf, buf.Length, SocketFlags.None);

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
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

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
                application = "Aaru",
                version = Version.GetVersion(),
                maxProtocol = Consts.MaxProtocol,
                sysname = DetectOS.GetPlatformName(
                    DetectOS.GetRealPlatformID(), DetectOS.GetVersion()),
                release = DetectOS.GetVersion(),
                machine = RuntimeInformation.ProcessArchitecture.ToString(),
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
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

        public bool IsRoot
        {
            get
            {
                var cmdPkt = new DicPacketCmdAmIRoot
                {
                    hdr = new DicPacketHeader
                    {
                        remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                        len = (uint) Marshal.SizeOf<DicPacketCmdAmIRoot>(),
                        version = Consts.PacketVersion,
                        packetType = DicPacketType.CommandAmIRoot
                    }
                };

                var buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

                var len = _socket.Send(buf, SocketFlags.None);

                if (len != buf.Length)
                {
                    DicConsole.ErrorWriteLine("Could not write to the network...");
                    return false;
                }

                var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

                len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

                if (len < hdrBuf.Length)
                {
                    DicConsole.ErrorWriteLine("Could not read from the network...");
                    return false;
                }

                var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

                if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
                {
                    DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                    return false;
                }

                if (hdr.packetType != DicPacketType.ResponseAmIRoot)
                {
                    DicConsole.ErrorWriteLine("Expected Am I Root? Response Packet, got packet type {0}...",
                        hdr.packetType);
                    return false;
                }

                buf = new byte[hdr.len];
                len = Receive(_socket, buf, buf.Length, SocketFlags.None);

                if (len < buf.Length)
                {
                    DicConsole.ErrorWriteLine("Could not read from the network...");
                    return false;
                }

                var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResAmIRoot>(buf);

                return res.am_i_root != 0;
            }
        }

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
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
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

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return new DeviceInfo[0];
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
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
                len = Receive(_socket, buf, buf.Length, SocketFlags.None);

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
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

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

        public bool Open(string devicePath, out int lastError)
        {
            lastError = 0;

            var cmdPkt = new DicPacketCommandOpenDevice
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
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
                lastError = -1;
                return false;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                lastError = -1;
                return false;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                lastError = -1;
                return false;
            }

            if (hdr.packetType != DicPacketType.Nop)
            {
                DicConsole.ErrorWriteLine("Expected List Devices Response Packet, got packet type {0}...",
                    hdr.packetType);
                lastError = -1;
                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                lastError = -1;
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
            lastError = nop.errno;
            return false;
        }

        public int SendScsiCommand(byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout,
            ScsiDirection direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration = 0;
            sense = true;

            var cmdPkt = new DicPacketCmdScsi
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandScsi
                },
                direction = (int) direction,
                timeout = timeout * 1000
            };

            if (cdb != null)
                cmdPkt.cdb_len = (uint) cdb.Length;
            if (buffer != null)
                cmdPkt.buf_len = (uint) buffer.Length;

            cmdPkt.hdr.len = (uint) (Marshal.SizeOf<DicPacketCmdScsi>() + cmdPkt.cdb_len + cmdPkt.buf_len);

            var pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            var buf = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<DicPacketCmdScsi>());

            if (cdb != null)
                Array.Copy(cdb, 0, buf, Marshal.SizeOf<DicPacketCmdScsi>(), cmdPkt.cdb_len);
            if (buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<DicPacketCmdScsi>() + cmdPkt.cdb_len, cmdPkt.buf_len);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return -1;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return -1;
            }

            if (hdr.packetType != DicPacketType.ResponseScsi)
            {
                DicConsole.ErrorWriteLine("Expected SCSI Response Packet, got packet type {0}...",
                    hdr.packetType);
                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResScsi>(buf);

            senseBuffer = new byte[res.sense_len];
            Array.Copy(buf, Marshal.SizeOf<DicPacketResScsi>(), senseBuffer, 0, res.sense_len);
            buffer = new byte[res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<DicPacketResScsi>() + res.sense_len, buffer, 0, res.buf_len);
            duration = res.duration;
            sense = res.sense != 0;

            return (int) res.error_no;
        }

        public int SendAtaCommand(AtaRegistersChs registers, out AtaErrorRegistersChs errorRegisters,
            AtaProtocol protocol, AtaTransferRegister transferRegister,
            ref byte[] buffer,
            uint timeout, bool transferBlocks,
            out double duration, out bool sense)
        {
            duration = 0;
            sense = true;
            errorRegisters = new AtaErrorRegistersChs();

            var cmdPkt = new DicPacketCmdAtaChs
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandAtaChs
                },
                registers = registers,
                protocol = (byte) protocol,
                transferRegister = (byte) transferRegister,
                transferBlocks = transferBlocks,
                timeout = timeout * 1000
            };

            if (buffer != null)
                cmdPkt.buf_len = (uint) buffer.Length;

            cmdPkt.hdr.len = (uint) (Marshal.SizeOf<DicPacketCmdAtaChs>() + cmdPkt.buf_len);

            var pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            var buf = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<DicPacketCmdAtaChs>());

            if (buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<DicPacketCmdAtaChs>(), cmdPkt.buf_len);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return -1;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return -1;
            }

            if (hdr.packetType != DicPacketType.ResponseAtaChs)
            {
                DicConsole.ErrorWriteLine("Expected ATA CHS Response Packet, got packet type {0}...",
                    hdr.packetType);
                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResAtaChs>(buf);

            buffer = new byte[res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<DicPacketResAtaChs>(), buffer, 0, res.buf_len);
            duration = res.duration;
            sense = res.sense != 0;
            errorRegisters = res.registers;

            return (int) res.error_no;
        }

        public int SendAtaCommand(AtaRegistersLba28 registers, out AtaErrorRegistersLba28 errorRegisters,
            AtaProtocol protocol, AtaTransferRegister transferRegister,
            ref byte[] buffer,
            uint timeout, bool transferBlocks,
            out double duration, out bool sense)
        {
            duration = 0;
            sense = true;
            errorRegisters = new AtaErrorRegistersLba28();

            var cmdPkt = new DicPacketCmdAtaLba28
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandAtaLba28
                },
                registers = registers,
                protocol = (byte) protocol,
                transferRegister = (byte) transferRegister,
                transferBlocks = transferBlocks,
                timeout = timeout * 1000
            };

            if (buffer != null)
                cmdPkt.buf_len = (uint) buffer.Length;

            cmdPkt.hdr.len = (uint) (Marshal.SizeOf<DicPacketCmdAtaLba28>() + cmdPkt.buf_len);

            var pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            var buf = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<DicPacketCmdAtaLba28>());

            if (buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<DicPacketCmdAtaLba28>(), cmdPkt.buf_len);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return -1;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return -1;
            }

            if (hdr.packetType != DicPacketType.ResponseAtaLba28)
            {
                DicConsole.ErrorWriteLine("Expected ATA LBA28 Response Packet, got packet type {0}...",
                    hdr.packetType);
                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResAtaLba28>(buf);

            buffer = new byte[res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<DicPacketResAtaLba28>(), buffer, 0, res.buf_len);
            duration = res.duration;
            sense = res.sense != 0;
            errorRegisters = res.registers;

            return (int) res.error_no;
        }

        public int SendAtaCommand(AtaRegistersLba48 registers, out AtaErrorRegistersLba48 errorRegisters,
            AtaProtocol protocol, AtaTransferRegister transferRegister,
            ref byte[] buffer,
            uint timeout, bool transferBlocks,
            out double duration, out bool sense)
        {
            duration = 0;
            sense = true;
            errorRegisters = new AtaErrorRegistersLba48();

            var cmdPkt = new DicPacketCmdAtaLba48
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandAtaLba48
                },
                registers = registers,
                protocol = (byte) protocol,
                transferRegister = (byte) transferRegister,
                transferBlocks = transferBlocks,
                timeout = timeout * 1000
            };

            if (buffer != null)
                cmdPkt.buf_len = (uint) buffer.Length;

            cmdPkt.hdr.len = (uint) (Marshal.SizeOf<DicPacketCmdAtaLba48>() + cmdPkt.buf_len);

            var pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            var buf = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<DicPacketCmdAtaLba48>());

            if (buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<DicPacketCmdAtaLba48>(), cmdPkt.buf_len);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return -1;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return -1;
            }

            if (hdr.packetType != DicPacketType.ResponseAtaLba48)
            {
                DicConsole.ErrorWriteLine("Expected ATA LBA48 Response Packet, got packet type {0}...",
                    hdr.packetType);
                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResAtaLba48>(buf);

            buffer = new byte[res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<DicPacketResAtaLba48>(), buffer, 0, res.buf_len);
            duration = res.duration;
            sense = res.sense != 0;
            errorRegisters = res.registers;

            return (int) res.error_no;
        }

        public int SendMmcCommand(MmcCommands command, bool write, bool isApplication, MmcFlags flags,
            uint argument,
            uint blockSize, uint blocks, ref byte[] buffer, out uint[] response,
            out double duration, out bool sense, uint timeout = 0)
        {
            duration = 0;
            sense = true;
            response = null;

            var cmdPkt = new DicPacketCmdSdhci
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandAtaLba48
                },
                command = command,
                write = write,
                application = isApplication,
                flags = flags,
                argument = argument,
                block_size = blockSize,
                blocks = blocks,
                timeout = timeout * 1000
            };

            if (buffer != null)
                cmdPkt.buf_len = (uint) buffer.Length;

            cmdPkt.hdr.len = (uint) (Marshal.SizeOf<DicPacketCmdSdhci>() + cmdPkt.buf_len);

            var pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            var buf = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<DicPacketCmdSdhci>());

            if (buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<DicPacketCmdSdhci>(), cmdPkt.buf_len);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return -1;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return -1;
            }

            if (hdr.packetType != DicPacketType.ResponseSdhci)
            {
                DicConsole.ErrorWriteLine("Expected SDHCI Response Packet, got packet type {0}...",
                    hdr.packetType);
                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return -1;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResSdhci>(buf);

            buffer = new byte[res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<DicPacketResSdhci>(), buffer, 0, res.buf_len);
            duration = res.duration;
            sense = res.sense != 0;
            response = new uint[4];
            response[0] = res.response[0];
            response[1] = res.response[1];
            response[2] = res.response[2];
            response[3] = res.response[3];

            return (int) res.error_no;
        }

        public DeviceType GetDeviceType()
        {
            var cmdPkt = new DicPacketCmdGetDeviceType
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    len = (uint) Marshal.SizeOf<DicPacketCmdGetDeviceType>(),
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandGetType
                }
            };

            var buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return DeviceType.Unknown;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return DeviceType.Unknown;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return DeviceType.Unknown;
            }

            if (hdr.packetType != DicPacketType.ResponseGetType)
            {
                DicConsole.ErrorWriteLine("Expected Device Type Response Packet, got packet type {0}...",
                    hdr.packetType);
                return DeviceType.Unknown;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return DeviceType.Unknown;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResGetDeviceType>(buf);

            return res.device_type;
        }

        public bool GetSdhciRegisters(out byte[] csd, out byte[] cid, out byte[] ocr, out byte[] scr)
        {
            csd = null;
            cid = null;
            ocr = null;
            scr = null;

            var cmdPkt = new DicPacketCmdGetSdhciRegisters
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    len = (uint) Marshal.SizeOf<DicPacketCmdGetSdhciRegisters>(),
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandGetSdhciRegisters
                }
            };

            var buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return false;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return false;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return false;
            }

            if (hdr.packetType != DicPacketType.ResponseGetSdhciRegisters)
            {
                DicConsole.ErrorWriteLine("Expected Device Type Response Packet, got packet type {0}...",
                    hdr.packetType);
                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return false;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResGetSdhciRegisters>(buf);

            if (res.csd_len > 0)
            {
                if (res.csd_len > 16)
                    res.csd_len = 16;

                csd = new byte[res.csd_len];

                Array.Copy(res.csd, 0, csd, 0, res.csd_len);
            }

            if (res.cid_len > 0)
            {
                if (res.cid_len > 16)
                    res.cid_len = 16;

                cid = new byte[res.cid_len];

                Array.Copy(res.cid, 0, cid, 0, res.cid_len);
            }

            if (res.ocr_len > 0)
            {
                if (res.ocr_len > 16)
                    res.ocr_len = 16;

                ocr = new byte[res.ocr_len];

                Array.Copy(res.ocr, 0, ocr, 0, res.ocr_len);
            }

            if (res.scr_len > 0)
            {
                if (res.scr_len > 16)
                    res.scr_len = 16;

                scr = new byte[res.scr_len];

                Array.Copy(res.scr, 0, scr, 0, res.scr_len);
            }

            return res.isSdhci;
        }

        public bool GetUsbData(out byte[] descriptors, out ushort idVendor, out ushort idProduct,
            out string manufacturer, out string product, out string serial)
        {
            descriptors = null;
            idVendor = 0;
            idProduct = 0;
            manufacturer = null;
            product = null;
            serial = null;

            var cmdPkt = new DicPacketCmdGetUsbData
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    len = (uint) Marshal.SizeOf<DicPacketCmdGetUsbData>(),
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandGetUsbData
                }
            };

            var buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return false;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return false;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return false;
            }

            if (hdr.packetType != DicPacketType.ResponseGetUsbData)
            {
                DicConsole.ErrorWriteLine("Expected USB Data Response Packet, got packet type {0}...",
                    hdr.packetType);
                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return false;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResGetUsbData>(buf);

            if (!res.isUsb)
                return false;

            descriptors = new byte[res.descLen];
            Array.Copy(res.descriptors, 0, descriptors, 0, res.descLen);
            idVendor = res.idVendor;
            idProduct = res.idProduct;
            manufacturer = res.manufacturer;
            product = res.product;
            serial = res.serial;

            return true;
        }

        public bool GetFireWireData(out uint idVendor, out uint idProduct,
            out ulong guid, out string vendor, out string model)
        {
            idVendor = 0;
            idProduct = 0;
            guid = 0;
            vendor = null;
            model = null;

            var cmdPkt = new DicPacketCmdGetFireWireData
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    len = (uint) Marshal.SizeOf<DicPacketCmdGetFireWireData>(),
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandGetFireWireData
                }
            };

            var buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return false;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return false;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return false;
            }

            if (hdr.packetType != DicPacketType.ResponseGetFireWireData)
            {
                DicConsole.ErrorWriteLine("Expected FireWire Data Response Packet, got packet type {0}...",
                    hdr.packetType);
                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return false;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResGetFireWireData>(buf);

            if (!res.isFireWire)
                return false;

            idVendor = res.idVendor;
            idProduct = res.idModel;
            guid = res.guid;
            vendor = res.vendor;
            model = res.model;

            return true;
        }

        public bool GetPcmciaData(out byte[] cis)
        {
            cis = null;

            var cmdPkt = new DicPacketCmdGetPcmciaData
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    len = (uint) Marshal.SizeOf<DicPacketCmdGetPcmciaData>(),
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandGetPcmciaData
                }
            };

            var buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            var len = _socket.Send(buf, SocketFlags.None);

            if (len != buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not write to the network...");
                return false;
            }

            var hdrBuf = new byte[Marshal.SizeOf<DicPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if (len < hdrBuf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return false;
            }

            var hdr = Marshal.ByteArrayToStructureLittleEndian<DicPacketHeader>(hdrBuf);

            if (hdr.remote_id != Consts.RemoteId || hdr.packet_id != Consts.PacketId)
            {
                DicConsole.ErrorWriteLine("Received data is not a DIC Remote Packet...");
                return false;
            }

            if (hdr.packetType != DicPacketType.ResponseGetPcmciaData)
            {
                DicConsole.ErrorWriteLine("Expected PCMCIA Data Response Packet, got packet type {0}...",
                    hdr.packetType);
                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if (len < buf.Length)
            {
                DicConsole.ErrorWriteLine("Could not read from the network...");
                return false;
            }

            var res = Marshal.ByteArrayToStructureLittleEndian<DicPacketResGetPcmciaData>(buf);

            if (!res.isPcmcia)
                return false;

            cis = res.cis;

            return true;
        }

        private static int Receive(Socket socket, byte[] buffer, int size, SocketFlags socketFlags)
        {
            int gotten;
            var offset = 0;

            while (size > 0)
            {
                gotten = socket.Receive(buffer, offset, size, socketFlags);

                if (gotten <= 0) break;

                offset += gotten;
                size -= gotten;
            }

            return offset;
        }

        public void Close()
        {
            var cmdPkt = new DicPacketCmdClose
            {
                hdr = new DicPacketHeader
                {
                    remote_id = Consts.RemoteId, packet_id = Consts.PacketId,
                    len = (uint) Marshal.SizeOf<DicPacketCmdClose>(),
                    version = Consts.PacketVersion,
                    packetType = DicPacketType.CommandCloseDevice
                }
            };

            var buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            _socket.Send(buf, SocketFlags.None);
        }
    }
}