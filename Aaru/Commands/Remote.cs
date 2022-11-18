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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

// TODO: Fix errors returned

using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using Spectre.Console;
using Remote = Aaru.Devices.Remote.Remote;

namespace Aaru.Commands;

sealed class RemoteCommand : Command
{
    public RemoteCommand() : base("remote", UI.Remote_Command_Description)
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
        {
            IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(System.Console.Error)
            });

            AaruConsole.DebugWriteLineEvent += (format, objects) =>
            {
                if(objects is null)
                    stderrConsole.MarkupLine(format);
                else
                    stderrConsole.MarkupLine(format, objects);
            };
        }

        if(verbose)
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };

        Statistics.AddCommand("remote");

        AaruConsole.DebugWriteLine("Remote command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("Remote command", "--host={0}", host);
        AaruConsole.DebugWriteLine("Remote command", "--verbose={0}", verbose);

        try
        {
            var remote = new Remote(new Uri(host));

            Statistics.AddRemote(remote.ServerApplication, remote.ServerVersion, remote.ServerOperatingSystem,
                                 remote.ServerOperatingSystemVersion, remote.ServerArchitecture);

            Table table = new()
            {
                Title = new TableTitle("Server information")
            };

            table.AddColumn("");
            table.AddColumn("");
            table.Columns[0].RightAligned();

            table.AddRow("Server application", $"{remote.ServerApplication} {remote.ServerVersion}");

            table.AddRow("Server operating system",
                         $"{remote.ServerOperatingSystem} {remote.ServerOperatingSystemVersion} ({
                             remote.ServerArchitecture})");

            table.AddRow("Server maximum protocol", $"{remote.ServerProtocolVersion}");

            AnsiConsole.Write(table);
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