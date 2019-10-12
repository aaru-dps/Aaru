// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Remote.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'remote' verb.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Mono.Options;

namespace DiscImageChef.Commands
{
    internal class RemoteCommand : Command
    {
        private string host;
        private bool showHelp;

        public RemoteCommand() : base("remote", "Tests connection to a DiscImageChef Remote Server.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS] host",
                "",
                Help,
                {"help|h|?", "Show this message and exit.", v => showHelp = v != null}
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            var extra = Options.Parse(arguments);

            if (showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);
                return (int) ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();
            if (MainClass.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
            if (MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
//            Statistics.AddCommand("remote");

            if (extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int) ErrorNumber.UnexpectedArgumentCount;
            }

            if (extra.Count == 0)
            {
                DicConsole.ErrorWriteLine("Missing input image.");
                return (int) ErrorNumber.MissingArgument;
            }

            host = extra[0];

            DicConsole.DebugWriteLine("Remote command", "--debug={0}", MainClass.Debug);
            DicConsole.DebugWriteLine("Remote command", "--host={0}", host);
            DicConsole.DebugWriteLine("Remote command", "--verbose={0}", MainClass.Verbose);

            var ipHostEntry = Dns.GetHostEntry(host);
            var ipAddress = ipHostEntry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            if (ipAddress is null)
            {
                DicConsole.ErrorWriteLine("Host not found");
                return (int) Errno.ENODEV;
            }

            var ipEndPoint = new IPEndPoint(ipAddress, 6666);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(ipEndPoint);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                DicConsole.Write(e.Message);
            }

            return (int) ErrorNumber.NoError;
        }
    }
}