using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.Console;
using DiscImageChef.Helpers;
using Version = DiscImageChef.CommonTypes.Interop.Version;

namespace DiscImageChef.Devices.Remote
{
    public class Remote
    {
        private readonly Socket _socket;

        public Remote(string host)
        {
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

            if (hdr.packetType != DicPacketType.Hello)
            {
                DicConsole.ErrorWriteLine("Expected Hello Packet, got packet type {0}...", hdr.packetType);
                throw new ArgumentException();
            }

            if (hdr.version != Consts.PacketVersion)
            {
                DicConsole.ErrorWriteLine("Unrecognized packet version...");
                throw new ArgumentException();
            }

            var buf = new byte[hdr.len];
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
                machine = "", // TODO: Get architecture
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

        public void Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
    }
}