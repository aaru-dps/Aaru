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

// TODO: Fix errors returned

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Mono.Options;
using Remote = DiscImageChef.Devices.Remote.Remote;

namespace DiscImageChef.Commands
{
    internal class RemoteCommand : Command
    {
        string host;
        bool   showHelp;

        public RemoteCommand() : base("remote", "Tests connection to a DiscImageChef Remote Server.") =>
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}", "", $"usage: DiscImageChef {Name} [OPTIONS] host", "",
                Help,
                {
                    "help|h|?", "Show this message and exit.", v => showHelp = v != null
                }
            };

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = Options.Parse(arguments);

            if(showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);

                return(int)ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();

            if(MainClass.Debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(MainClass.Verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            //            Statistics.AddCommand("remote");

            if(extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");

                return(int)ErrorNumber.UnexpectedArgumentCount;
            }

            if(extra.Count == 0)
            {
                DicConsole.ErrorWriteLine("Missing input image.");

                return(int)ErrorNumber.MissingArgument;
            }

            host = extra[0];

            DicConsole.DebugWriteLine("Remote command", "--debug={0}", MainClass.Debug);
            DicConsole.DebugWriteLine("Remote command", "--host={0}", host);
            DicConsole.DebugWriteLine("Remote command", "--verbose={0}", MainClass.Verbose);

            try
            {
                var remote = new Remote(host);

                Statistics.AddRemote(remote.ServerApplication, remote.ServerVersion, remote.ServerOperatingSystem,
                                     remote.ServerOperatingSystemVersion, remote.ServerArchitecture);

                DicConsole.WriteLine("Server application: {0} {1}", remote.ServerApplication, remote.ServerVersion);

                DicConsole.WriteLine("Server operating system: {0} {1} ({2})", remote.ServerOperatingSystem,
                                     remote.ServerOperatingSystemVersion, remote.ServerArchitecture);

                DicConsole.WriteLine("Server maximum protocol: {0}", remote.ServerProtocolVersion);
                remote.Disconnect();
            }
            catch(Exception)
            {
                DicConsole.ErrorWriteLine("Error connecting to host.");

                return(int)ErrorNumber.CannotOpenDevice;
            }

            return(int)ErrorNumber.NoError;
        }
    }
}