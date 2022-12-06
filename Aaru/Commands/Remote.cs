// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Remote.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'remote' command.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// TODO: Fix errors returned

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Core;
using Remote = Aaru.Devices.Remote.Remote;

namespace Aaru.Commands
{
    internal sealed class RemoteCommand : Command
    {
        public RemoteCommand() : base("remote", "Tests connection to a Aaru Remote Server.")
        {
            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "aaru host",
                Name        = "host"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string host)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("remote");

            AaruConsole.DebugWriteLine("Remote command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Remote command", "--host={0}", host);
            AaruConsole.DebugWriteLine("Remote command", "--verbose={0}", verbose);

            try
            {
                var remote = new Remote(new Uri(host));

                Statistics.AddRemote(remote.ServerApplication, remote.ServerVersion, remote.ServerOperatingSystem,
                                     remote.ServerOperatingSystemVersion, remote.ServerArchitecture);

                AaruConsole.WriteLine("Server application: {0} {1}", remote.ServerApplication, remote.ServerVersion);

                AaruConsole.WriteLine("Server operating system: {0} {1} ({2})", remote.ServerOperatingSystem,
                                      remote.ServerOperatingSystemVersion, remote.ServerArchitecture);

                AaruConsole.WriteLine("Server maximum protocol: {0}", remote.ServerProtocolVersion);
                remote.Disconnect();
            }
            catch(Exception)
            {
                AaruConsole.ErrorWriteLine("Error connecting to host.");

                return (int)ErrorNumber.CannotOpenDevice;
            }

            return (int)ErrorNumber.NoError;
        }
    }
}