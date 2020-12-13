// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Remote.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru Remote.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implementation of the Aaru Remote protocol.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

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
        readonly string _host;
        readonly Socket _socket;

        public Remote(string host)
        {
            _host = host;

            if(!IPAddress.TryParse(host, out IPAddress ipAddress))
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(host);

                ipAddress = ipHostEntry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            }

            if(ipAddress is null)
            {
                AaruConsole.ErrorWriteLine("Host not found");

                throw new SocketException(11001);
            }

            var ipEndPoint = new IPEndPoint(ipAddress, 6666);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _socket.Connect(ipEndPoint);

            AaruConsole.WriteLine("Connected to {0}", host);

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            int len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                throw new IOException();
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                throw new ArgumentException();
            }

            byte[] buf;

            if(hdr.packetType != AaruPacketType.Hello)
            {
                if(hdr.packetType != AaruPacketType.Nop)
                {
                    AaruConsole.ErrorWriteLine("Expected Hello Packet, got packet type {0}...", hdr.packetType);

                    throw new ArgumentException();
                }

                buf = new byte[hdr.len];
                len = Receive(_socket, buf, buf.Length, SocketFlags.None);

                if(len < buf.Length)
                {
                    AaruConsole.ErrorWriteLine("Could not read from the network...");

                    throw new IOException();
                }

                AaruPacketNop nop = Marshal.ByteArrayToStructureLittleEndian<AaruPacketNop>(buf);

                AaruConsole.ErrorWriteLine($"{nop.reason}");

                throw new ArgumentException();
            }

            if(hdr.version != Consts.PACKET_VERSION)
            {
                AaruConsole.ErrorWriteLine("Unrecognized packet version...");

                throw new ArgumentException();
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                throw new IOException();
            }

            AaruPacketHello serverHello = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHello>(buf);

            ServerApplication            = serverHello.application;
            ServerVersion                = serverHello.version;
            ServerOperatingSystem        = serverHello.sysname;
            ServerOperatingSystemVersion = serverHello.release;
            ServerArchitecture           = serverHello.machine;
            ServerProtocolVersion        = serverHello.maxProtocol;

            var clientHello = new AaruPacketHello
            {
                application = "Aaru",
                version     = Version.GetVersion(),
                maxProtocol = Consts.MAX_PROTOCOL,
                sysname     = DetectOS.GetPlatformName(DetectOS.GetRealPlatformID(), DetectOS.GetVersion()),
                release     = DetectOS.GetVersion(),
                machine     = RuntimeInformation.ProcessArchitecture.ToString(),
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketHello>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.Hello
                }
            };

            buf = Marshal.StructureToByteArrayLittleEndian(clientHello);

            len = _socket.Send(buf, SocketFlags.None);

            if(len >= buf.Length)
                return;

            AaruConsole.ErrorWriteLine("Could not write to the network...");

            throw new IOException();
        }

        public string ServerApplication            { get; }
        public string ServerVersion                { get; }
        public string ServerOperatingSystem        { get; }
        public string ServerOperatingSystemVersion { get; }
        public string ServerArchitecture           { get; }
        public int    ServerProtocolVersion        { get; }

        public bool IsRoot
        {
            get
            {
                var cmdPkt = new AaruPacketCmdAmIRoot
                {
                    hdr = new AaruPacketHeader
                    {
                        remote_id  = Consts.REMOTE_ID,
                        packet_id  = Consts.PACKET_ID,
                        len        = (uint)Marshal.SizeOf<AaruPacketCmdAmIRoot>(),
                        version    = Consts.PACKET_VERSION,
                        packetType = AaruPacketType.CommandAmIRoot
                    }
                };

                byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

                int len = _socket.Send(buf, SocketFlags.None);

                if(len != buf.Length)
                {
                    AaruConsole.ErrorWriteLine("Could not write to the network...");

                    return false;
                }

                byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

                len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

                if(len < hdrBuf.Length)
                {
                    AaruConsole.ErrorWriteLine("Could not read from the network...");

                    return false;
                }

                AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

                if(hdr.remote_id != Consts.REMOTE_ID ||
                   hdr.packet_id != Consts.PACKET_ID)
                {
                    AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                    return false;
                }

                if(hdr.packetType != AaruPacketType.ResponseAmIRoot)
                {
                    AaruConsole.ErrorWriteLine("Expected Am I Root? Response Packet, got packet type {0}...",
                                               hdr.packetType);

                    return false;
                }

                buf = new byte[hdr.len];
                len = Receive(_socket, buf, buf.Length, SocketFlags.None);

                if(len < buf.Length)
                {
                    AaruConsole.ErrorWriteLine("Could not read from the network...");

                    return false;
                }

                AaruPacketResAmIRoot res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResAmIRoot>(buf);

                return res.am_i_root != 0;
            }
        }

        public void Dispose() => Disconnect();

        public void Disconnect()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch(ObjectDisposedException)
            {
                // Ignore if already disposed
            }
        }

        public DeviceInfo[] ListDevices()
        {
            var cmdPkt = new AaruPacketCommandListDevices
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCommandListDevices>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandListDevices
                }
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return new DeviceInfo[0];
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return new DeviceInfo[0];
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return new DeviceInfo[0];
            }

            if(hdr.packetType != AaruPacketType.ResponseListDevices)
            {
                if(hdr.packetType != AaruPacketType.Nop)
                {
                    AaruConsole.ErrorWriteLine("Expected List Devices Response Packet, got packet type {0}...",
                                               hdr.packetType);

                    return new DeviceInfo[0];
                }

                buf = new byte[hdr.len];
                len = Receive(_socket, buf, buf.Length, SocketFlags.None);

                if(len < buf.Length)
                {
                    AaruConsole.ErrorWriteLine("Could not read from the network...");

                    return new DeviceInfo[0];
                }

                AaruPacketNop nop = Marshal.ByteArrayToStructureLittleEndian<AaruPacketNop>(buf);

                AaruConsole.ErrorWriteLine($"{nop.reason}");

                return new DeviceInfo[0];
            }

            if(hdr.version != Consts.PACKET_VERSION)
            {
                AaruConsole.ErrorWriteLine("Unrecognized packet version...");

                return new DeviceInfo[0];
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return new DeviceInfo[0];
            }

            AaruPacketResponseListDevices response =
                Marshal.ByteArrayToStructureLittleEndian<AaruPacketResponseListDevices>(buf);

            List<DeviceInfo> devices    = new List<DeviceInfo>();
            int              offset     = Marshal.SizeOf<AaruPacketResponseListDevices>();
            int              devInfoLen = Marshal.SizeOf<DeviceInfo>();

            for(ushort i = 0; i < response.devices; i++)
            {
                DeviceInfo dev = Marshal.ByteArrayToStructureLittleEndian<DeviceInfo>(buf, offset, devInfoLen);
                dev.Path = dev.Path[0] == '/' ? $"aaru://{_host}{dev.Path}" : $"aaru://{_host}/{dev.Path}";
                devices.Add(dev);
                offset += devInfoLen;
            }

            return devices.ToArray();
        }

        public bool Open(string devicePath, out int lastError)
        {
            lastError = 0;

            var cmdPkt = new AaruPacketCommandOpenDevice
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCommandOpenDevice>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandOpen
                },
                device_path = devicePath
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");
                lastError = -1;

                return false;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");
                lastError = -1;

                return false;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");
                lastError = -1;

                return false;
            }

            if(hdr.packetType != AaruPacketType.Nop)
            {
                AaruConsole.ErrorWriteLine("Expected List Devices Response Packet, got packet type {0}...",
                                           hdr.packetType);

                lastError = -1;

                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");
                lastError = -1;

                return false;
            }

            AaruPacketNop nop = Marshal.ByteArrayToStructureLittleEndian<AaruPacketNop>(buf);

            switch(nop.reasonCode)
            {
                case AaruNopReason.OpenOk:         return true;
                case AaruNopReason.NotImplemented: throw new NotImplementedException($"{nop.reason}");
            }

            AaruConsole.ErrorWriteLine($"{nop.reason}");
            lastError = nop.errno;

            return false;
        }

        public int SendScsiCommand(byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout,
                                   ScsiDirection direction, out double duration, out bool sense)
        {
            senseBuffer = null;
            duration    = 0;
            sense       = true;

            var cmdPkt = new AaruPacketCmdScsi
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandScsi
                },
                direction = (int)direction,
                timeout   = timeout * 1000
            };

            if(cdb != null)
                cmdPkt.cdb_len = (uint)cdb.Length;

            if(buffer != null)
                cmdPkt.buf_len = (uint)buffer.Length;

            cmdPkt.hdr.len = (uint)(Marshal.SizeOf<AaruPacketCmdScsi>() + cmdPkt.cdb_len + cmdPkt.buf_len);

            byte[] pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            byte[] buf    = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<AaruPacketCmdScsi>());

            if(cdb != null)
                Array.Copy(cdb, 0, buf, Marshal.SizeOf<AaruPacketCmdScsi>(), cmdPkt.cdb_len);

            if(buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<AaruPacketCmdScsi>() + cmdPkt.cdb_len, cmdPkt.buf_len);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return -1;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return -1;
            }

            if(hdr.packetType != AaruPacketType.ResponseScsi)
            {
                AaruConsole.ErrorWriteLine("Expected SCSI Response Packet, got packet type {0}...", hdr.packetType);

                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketResScsi res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResScsi>(buf);

            senseBuffer = new byte[res.sense_len];
            Array.Copy(buf, Marshal.SizeOf<AaruPacketResScsi>(), senseBuffer, 0, res.sense_len);
            buffer = new byte[res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<AaruPacketResScsi>() + res.sense_len, buffer, 0, res.buf_len);
            duration = res.duration;
            sense    = res.sense != 0;

            return (int)res.error_no;
        }

        public int SendAtaCommand(AtaRegistersChs registers, out AtaErrorRegistersChs errorRegisters,
                                  AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                  uint timeout, bool transferBlocks, out double duration, out bool sense)
        {
            duration       = 0;
            sense          = true;
            errorRegisters = new AtaErrorRegistersChs();

            var cmdPkt = new AaruPacketCmdAtaChs
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandAtaChs
                },
                registers        = registers,
                protocol         = (byte)protocol,
                transferRegister = (byte)transferRegister,
                transferBlocks   = transferBlocks,
                timeout          = timeout * 1000
            };

            if(buffer != null)
                cmdPkt.buf_len = (uint)buffer.Length;

            cmdPkt.hdr.len = (uint)(Marshal.SizeOf<AaruPacketCmdAtaChs>() + cmdPkt.buf_len);

            byte[] pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            byte[] buf    = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<AaruPacketCmdAtaChs>());

            if(buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<AaruPacketCmdAtaChs>(), cmdPkt.buf_len);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return -1;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return -1;
            }

            if(hdr.packetType != AaruPacketType.ResponseAtaChs)
            {
                AaruConsole.ErrorWriteLine("Expected ATA CHS Response Packet, got packet type {0}...", hdr.packetType);

                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketResAtaChs res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResAtaChs>(buf);

            buffer = new byte[res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<AaruPacketResAtaChs>(), buffer, 0, res.buf_len);
            duration       = res.duration;
            sense          = res.sense != 0;
            errorRegisters = res.registers;

            return (int)res.error_no;
        }

        public int SendAtaCommand(AtaRegistersLba28 registers, out AtaErrorRegistersLba28 errorRegisters,
                                  AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                  uint timeout, bool transferBlocks, out double duration, out bool sense)
        {
            duration       = 0;
            sense          = true;
            errorRegisters = new AtaErrorRegistersLba28();

            var cmdPkt = new AaruPacketCmdAtaLba28
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandAtaLba28
                },
                registers        = registers,
                protocol         = (byte)protocol,
                transferRegister = (byte)transferRegister,
                transferBlocks   = transferBlocks,
                timeout          = timeout * 1000
            };

            if(buffer != null)
                cmdPkt.buf_len = (uint)buffer.Length;

            cmdPkt.hdr.len = (uint)(Marshal.SizeOf<AaruPacketCmdAtaLba28>() + cmdPkt.buf_len);

            byte[] pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            byte[] buf    = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<AaruPacketCmdAtaLba28>());

            if(buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<AaruPacketCmdAtaLba28>(), cmdPkt.buf_len);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return -1;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return -1;
            }

            if(hdr.packetType != AaruPacketType.ResponseAtaLba28)
            {
                AaruConsole.ErrorWriteLine("Expected ATA LBA28 Response Packet, got packet type {0}...",
                                           hdr.packetType);

                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketResAtaLba28 res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResAtaLba28>(buf);

            buffer = new byte[res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<AaruPacketResAtaLba28>(), buffer, 0, res.buf_len);
            duration       = res.duration;
            sense          = res.sense != 0;
            errorRegisters = res.registers;

            return (int)res.error_no;
        }

        public int SendAtaCommand(AtaRegistersLba48 registers, out AtaErrorRegistersLba48 errorRegisters,
                                  AtaProtocol protocol, AtaTransferRegister transferRegister, ref byte[] buffer,
                                  uint timeout, bool transferBlocks, out double duration, out bool sense)
        {
            duration       = 0;
            sense          = true;
            errorRegisters = new AtaErrorRegistersLba48();

            var cmdPkt = new AaruPacketCmdAtaLba48
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandAtaLba48
                },
                registers        = registers,
                protocol         = (byte)protocol,
                transferRegister = (byte)transferRegister,
                transferBlocks   = transferBlocks,
                timeout          = timeout * 1000
            };

            if(buffer != null)
                cmdPkt.buf_len = (uint)buffer.Length;

            cmdPkt.hdr.len = (uint)(Marshal.SizeOf<AaruPacketCmdAtaLba48>() + cmdPkt.buf_len);

            byte[] pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            byte[] buf    = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<AaruPacketCmdAtaLba48>());

            if(buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<AaruPacketCmdAtaLba48>(), cmdPkt.buf_len);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return -1;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return -1;
            }

            if(hdr.packetType != AaruPacketType.ResponseAtaLba48)
            {
                AaruConsole.ErrorWriteLine("Expected ATA LBA48 Response Packet, got packet type {0}...",
                                           hdr.packetType);

                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketResAtaLba48 res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResAtaLba48>(buf);

            buffer = new byte[res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<AaruPacketResAtaLba48>(), buffer, 0, res.buf_len);
            duration       = res.duration;
            sense          = res.sense != 0;
            errorRegisters = res.registers;

            return (int)res.error_no;
        }

        public int SendMmcCommand(MmcCommands command, bool write, bool isApplication, MmcFlags flags, uint argument,
                                  uint blockSize, uint blocks, ref byte[] buffer, out uint[] response,
                                  out double duration, out bool sense, uint timeout = 0)
        {
            duration = 0;
            sense    = true;
            response = null;

            var cmdPkt = new AaruPacketCmdSdhci
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandSdhci
                },
                command = new AaruCmdSdhci
                {
                    command     = command,
                    write       = write,
                    application = isApplication,
                    flags       = flags,
                    argument    = argument,
                    block_size  = blockSize,
                    blocks      = blocks,
                    timeout     = timeout * 1000
                }
            };

            if(buffer != null)
                cmdPkt.command.buf_len = (uint)buffer.Length;

            cmdPkt.hdr.len = (uint)(Marshal.SizeOf<AaruPacketCmdSdhci>() + cmdPkt.command.buf_len);

            byte[] pktBuf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);
            byte[] buf    = new byte[cmdPkt.hdr.len];

            Array.Copy(pktBuf, 0, buf, 0, Marshal.SizeOf<AaruPacketCmdSdhci>());

            if(buffer != null)
                Array.Copy(buffer, 0, buf, Marshal.SizeOf<AaruPacketCmdSdhci>(), cmdPkt.command.buf_len);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return -1;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return -1;
            }

            if(hdr.packetType != AaruPacketType.ResponseSdhci)
            {
                AaruConsole.ErrorWriteLine("Expected SDHCI Response Packet, got packet type {0}...", hdr.packetType);

                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketResSdhci res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResSdhci>(buf);

            buffer = new byte[res.res.buf_len];
            Array.Copy(buf, Marshal.SizeOf<AaruPacketResSdhci>(), buffer, 0, res.res.buf_len);
            duration    = res.res.duration;
            sense       = res.res.sense != 0;
            response    = new uint[4];
            response[0] = res.res.response[0];
            response[1] = res.res.response[1];
            response[2] = res.res.response[2];
            response[3] = res.res.response[3];

            return (int)res.res.error_no;
        }

        public DeviceType GetDeviceType()
        {
            var cmdPkt = new AaruPacketCmdGetDeviceType
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCmdGetDeviceType>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandGetType
                }
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return DeviceType.Unknown;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return DeviceType.Unknown;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return DeviceType.Unknown;
            }

            if(hdr.packetType != AaruPacketType.ResponseGetType)
            {
                AaruConsole.ErrorWriteLine("Expected Device Type Response Packet, got packet type {0}...",
                                           hdr.packetType);

                return DeviceType.Unknown;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return DeviceType.Unknown;
            }

            AaruPacketResGetDeviceType res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResGetDeviceType>(buf);

            return res.device_type;
        }

        public bool GetSdhciRegisters(out byte[] csd, out byte[] cid, out byte[] ocr, out byte[] scr)
        {
            csd = null;
            cid = null;
            ocr = null;
            scr = null;

            var cmdPkt = new AaruPacketCmdGetSdhciRegisters
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCmdGetSdhciRegisters>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandGetSdhciRegisters
                }
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return false;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return false;
            }

            if(hdr.packetType != AaruPacketType.ResponseGetSdhciRegisters)
            {
                AaruConsole.ErrorWriteLine("Expected Device Type Response Packet, got packet type {0}...",
                                           hdr.packetType);

                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketResGetSdhciRegisters res =
                Marshal.ByteArrayToStructureLittleEndian<AaruPacketResGetSdhciRegisters>(buf);

            if(res.csd_len > 0)
            {
                if(res.csd_len > 16)
                    res.csd_len = 16;

                csd = new byte[res.csd_len];

                Array.Copy(res.csd, 0, csd, 0, res.csd_len);
            }

            if(res.cid_len > 0)
            {
                if(res.cid_len > 16)
                    res.cid_len = 16;

                cid = new byte[res.cid_len];

                Array.Copy(res.cid, 0, cid, 0, res.cid_len);
            }

            if(res.ocr_len > 0)
            {
                if(res.ocr_len > 16)
                    res.ocr_len = 16;

                ocr = new byte[res.ocr_len];

                Array.Copy(res.ocr, 0, ocr, 0, res.ocr_len);
            }

            if(res.scr_len > 0)
            {
                if(res.scr_len > 16)
                    res.scr_len = 16;

                scr = new byte[res.scr_len];

                Array.Copy(res.scr, 0, scr, 0, res.scr_len);
            }

            return res.isSdhci;
        }

        public bool GetUsbData(out byte[] descriptors, out ushort idVendor, out ushort idProduct,
                               out string manufacturer, out string product, out string serial)
        {
            descriptors  = null;
            idVendor     = 0;
            idProduct    = 0;
            manufacturer = null;
            product      = null;
            serial       = null;

            var cmdPkt = new AaruPacketCmdGetUsbData
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCmdGetUsbData>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandGetUsbData
                }
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return false;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return false;
            }

            if(hdr.packetType != AaruPacketType.ResponseGetUsbData)
            {
                AaruConsole.ErrorWriteLine("Expected USB Data Response Packet, got packet type {0}...", hdr.packetType);

                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketResGetUsbData res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResGetUsbData>(buf);

            if(!res.isUsb)
                return false;

            descriptors = new byte[res.descLen];
            Array.Copy(res.descriptors, 0, descriptors, 0, res.descLen);
            idVendor     = res.idVendor;
            idProduct    = res.idProduct;
            manufacturer = res.manufacturer;
            product      = res.product;
            serial       = res.serial;

            return true;
        }

        public bool GetFireWireData(out uint idVendor, out uint idProduct, out ulong guid, out string vendor,
                                    out string model)
        {
            idVendor  = 0;
            idProduct = 0;
            guid      = 0;
            vendor    = null;
            model     = null;

            var cmdPkt = new AaruPacketCmdGetFireWireData
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCmdGetFireWireData>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandGetFireWireData
                }
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return false;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return false;
            }

            if(hdr.packetType != AaruPacketType.ResponseGetFireWireData)
            {
                AaruConsole.ErrorWriteLine("Expected FireWire Data Response Packet, got packet type {0}...",
                                           hdr.packetType);

                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketResGetFireWireData res =
                Marshal.ByteArrayToStructureLittleEndian<AaruPacketResGetFireWireData>(buf);

            if(!res.isFireWire)
                return false;

            idVendor  = res.idVendor;
            idProduct = res.idModel;
            guid      = res.guid;
            vendor    = res.vendor;
            model     = res.model;

            return true;
        }

        public bool GetPcmciaData(out byte[] cis)
        {
            cis = null;

            var cmdPkt = new AaruPacketCmdGetPcmciaData
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCmdGetPcmciaData>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandGetPcmciaData
                }
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return false;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return false;
            }

            if(hdr.packetType != AaruPacketType.ResponseGetPcmciaData)
            {
                AaruConsole.ErrorWriteLine("Expected PCMCIA Data Response Packet, got packet type {0}...",
                                           hdr.packetType);

                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketResGetPcmciaData res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResGetPcmciaData>(buf);

            if(!res.isPcmcia)
                return false;

            cis = res.cis;

            return true;
        }

        static int Receive(Socket socket, byte[] buffer, int size, SocketFlags socketFlags)
        {
            int offset = 0;

            while(size > 0)
            {
                int got = socket.Receive(buffer, offset, size, socketFlags);

                if(got <= 0)
                    break;

                offset += got;
                size   -= got;
            }

            return offset;
        }

        public void Close()
        {
            var cmdPkt = new AaruPacketCmdClose
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCmdClose>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandCloseDevice
                }
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            try
            {
                _socket.Send(buf, SocketFlags.None);
            }
            catch(ObjectDisposedException)
            {
                // Ignore if already disposed
            }
        }

        public int SendMultipleMmcCommands(Device.MmcSingleCommand[] commands, out double duration, out bool sense,
                                           uint timeout = 0)
        {
            if(ServerProtocolVersion < 2)
                return SendMultipleMmcCommandsV1(commands, out duration, out sense, timeout);

            sense    = false;
            duration = 0;

            long packetSize = Marshal.SizeOf<AaruPacketMultiCmdSdhci>() +
                              (Marshal.SizeOf<AaruCmdSdhci>() * commands.LongLength);

            foreach(Device.MmcSingleCommand command in commands)
                packetSize += command.buffer?.Length ?? 0;

            var packet = new AaruPacketMultiCmdSdhci
            {
                cmd_count = (ulong)commands.LongLength,
                hdr = new AaruPacketHeader
                {
                    len        = (uint)packetSize,
                    packetType = AaruPacketType.MultiCommandSdhci,
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    version    = Consts.PACKET_VERSION
                }
            };

            byte[] buf = new byte[packetSize];
            byte[] tmp = Marshal.StructureToByteArrayLittleEndian(packet);

            Array.Copy(tmp, 0, buf, 0, tmp.Length);

            int off = tmp.Length;

            foreach(Device.MmcSingleCommand command in commands)
            {
                var cmd = new AaruCmdSdhci
                {
                    application = command.isApplication,
                    argument    = command.argument,
                    block_size  = command.blockSize,
                    blocks      = command.blocks,
                    buf_len     = (uint)(command.buffer?.Length ?? 0),
                    command     = command.command,
                    flags       = command.flags,
                    timeout     = timeout,
                    write       = command.write
                };

                tmp = Marshal.StructureToByteArrayLittleEndian(cmd);
                Array.Copy(tmp, 0, buf, off, tmp.Length);

                off += tmp.Length;
            }

            foreach(Device.MmcSingleCommand command in commands)
            {
                if((command.buffer?.Length ?? 0) == 0)
                    continue;

                Array.Copy(command.buffer, 0, buf, off, command.buffer.Length);

                off += command.buffer.Length;
            }

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return -1;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return -1;
            }

            if(hdr.packetType != AaruPacketType.ResponseMultiSdhci)
            {
                AaruConsole.ErrorWriteLine("Expected multi MMC/SD command Response Packet, got packet type {0}...",
                                           hdr.packetType);

                return -1;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return -1;
            }

            AaruPacketMultiCmdSdhci res = Marshal.ByteArrayToStructureLittleEndian<AaruPacketMultiCmdSdhci>(buf);

            if(res.cmd_count != (ulong)commands.Length)
            {
                AaruConsole.ErrorWriteLine("Expected the response to {0} SD/MMC commands, but got {1} responses...",
                                           commands.Length, res.cmd_count);

                return -1;
            }

            off = Marshal.SizeOf<AaruPacketMultiCmdSdhci>();

            int error = 0;

            foreach(Device.MmcSingleCommand command in commands)
            {
                AaruResSdhci cmdRes =
                    Marshal.ByteArrayToStructureLittleEndian<AaruResSdhci>(buf, off, Marshal.SizeOf<AaruResSdhci>());

                command.response =  cmdRes.response;
                duration         += cmdRes.duration;

                if(cmdRes.error_no != 0 &&
                   error           == 0)
                    error = (int)cmdRes.error_no;

                if(cmdRes.sense != 0)
                    sense = true;

                if(cmdRes.buf_len > 0)
                    command.buffer = new byte[cmdRes.buf_len];

                off += Marshal.SizeOf<AaruResSdhci>();
            }

            foreach(Device.MmcSingleCommand command in commands)
            {
                Array.Copy(buf, off, command.buffer, 0, command.buffer.Length);
                off += command.buffer.Length;
            }

            return error;
        }

        int SendMultipleMmcCommandsV1(Device.MmcSingleCommand[] commands, out double duration, out bool sense,
                                      uint timeout)
        {
            sense    = false;
            duration = 0;
            int error = 0;

            foreach(Device.MmcSingleCommand command in commands)
            {
                error = SendMmcCommand(command.command, command.write, command.isApplication, command.flags,
                                       command.argument, command.blockSize, command.blocks, ref command.buffer,
                                       out command.response, out double cmdDuration, out bool cmdSense, timeout);

                if(cmdSense)
                    sense = true;

                duration += cmdDuration;
            }

            return error;
        }

        public bool ReOpen()
        {
            var cmdPkt = new AaruPacketCmdReOpen
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCmdReOpen>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandReOpenDevice
                }
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return false;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return false;
            }

            if(hdr.packetType != AaruPacketType.Nop)
            {
                AaruConsole.ErrorWriteLine("Expected NOP Packet, got packet type {0}...", hdr.packetType);

                return false;
            }

            if(hdr.version != Consts.PACKET_VERSION)
            {
                AaruConsole.ErrorWriteLine("Unrecognized packet version...");

                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketNop nop = Marshal.ByteArrayToStructureLittleEndian<AaruPacketNop>(buf);

            switch(nop.reasonCode)
            {
                case AaruNopReason.ReOpenOk: return true;
                case AaruNopReason.CloseError:
                case AaruNopReason.OpenError:
                    AaruConsole.ErrorWriteLine("ReOpen error closing device...");

                    break;
                default:
                    AaruConsole.ErrorWriteLine("ReOpen error {0} with reason: {1}...", nop.errno, nop.reason);

                    break;
            }

            return false;
        }

        public bool BufferedOsRead(out byte[] buffer, long offset, uint length, out double duration)
        {
            duration = 0;
            buffer   = null;

            var cmdPkt = new AaruPacketCmdOsRead
            {
                hdr = new AaruPacketHeader
                {
                    remote_id  = Consts.REMOTE_ID,
                    packet_id  = Consts.PACKET_ID,
                    len        = (uint)Marshal.SizeOf<AaruPacketCmdOsRead>(),
                    version    = Consts.PACKET_VERSION,
                    packetType = AaruPacketType.CommandOsRead
                },
                length = length,
                offset = (ulong)offset
            };

            byte[] buf = Marshal.StructureToByteArrayLittleEndian(cmdPkt);

            int len = _socket.Send(buf, SocketFlags.None);

            if(len != buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not write to the network...");

                return false;
            }

            byte[] hdrBuf = new byte[Marshal.SizeOf<AaruPacketHeader>()];

            len = Receive(_socket, hdrBuf, hdrBuf.Length, SocketFlags.Peek);

            if(len < hdrBuf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketHeader hdr = Marshal.ByteArrayToStructureLittleEndian<AaruPacketHeader>(hdrBuf);

            if(hdr.remote_id != Consts.REMOTE_ID ||
               hdr.packet_id != Consts.PACKET_ID)
            {
                AaruConsole.ErrorWriteLine("Received data is not an Aaru Remote Packet...");

                return false;
            }

            if(hdr.packetType != AaruPacketType.ResponseOsRead)
            {
                AaruConsole.ErrorWriteLine("Expected OS Read Response Packet, got packet type {0}...", hdr.packetType);

                return false;
            }

            if(hdr.version != Consts.PACKET_VERSION)
            {
                AaruConsole.ErrorWriteLine("Unrecognized packet version...");

                return false;
            }

            buf = new byte[hdr.len];
            len = Receive(_socket, buf, buf.Length, SocketFlags.None);

            if(len < buf.Length)
            {
                AaruConsole.ErrorWriteLine("Could not read from the network...");

                return false;
            }

            AaruPacketResOsRead osRead = Marshal.ByteArrayToStructureLittleEndian<AaruPacketResOsRead>(buf);

            duration = osRead.duration;

            if(osRead.errno != 0)
            {
                AaruConsole.ErrorWriteLine("Remote error {0} in OS Read...", osRead.errno);

                return false;
            }

            buffer = new byte[length];
            Array.Copy(buf, Marshal.SizeOf<AaruPacketResOsRead>(), buffer, 0, length);

            return true;
        }
    }
}