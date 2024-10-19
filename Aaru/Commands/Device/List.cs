// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : List.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'list' command.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Core;
using Aaru.Devices;
using Aaru.Localization;
using JetBrains.Annotations;
using Spectre.Console;

namespace Aaru.Commands.Device;

sealed class ListDevicesCommand : Command
{
    const string MODULE_NAME = "List-Devices command";

    public ListDevicesCommand() : base("list", UI.Device_List_Command_Description)
    {
        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ZeroOrOne,
            Description = UI.aaruremote_host,
            Name        = "aaru-remote-host"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());
    }

    public static int Invoke(bool debug, bool verbose, [CanBeNull] string aaruRemoteHost)
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

            AaruConsole.WriteExceptionEvent += ex => { stderrConsole.WriteException(ex); };
        }

        if(verbose)
        {
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };
        }

        Statistics.AddCommand("list-devices");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",   debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}", verbose);

        DeviceInfo[] devices = Devices.Device.ListDevices(out bool isRemote,
                                                          out string serverApplication,
                                                          out string serverVersion,
                                                          out string serverOperatingSystem,
                                                          out string serverOperatingSystemVersion,
                                                          out string serverArchitecture,
                                                          aaruRemoteHost);

        if(isRemote)
        {
            Statistics.AddRemote(serverApplication,
                                 serverVersion,
                                 serverOperatingSystem,
                                 serverOperatingSystemVersion,
                                 serverArchitecture);
        }

        if(devices == null || devices.Length == 0)
            AaruConsole.WriteLine(UI.No_known_devices_attached);
        else
        {
            Table table = new();
            table.AddColumn(UI.Path);
            table.AddColumn(UI.Title_Vendor);
            table.AddColumn(UI.Title_Model);
            table.AddColumn(UI.Serial);
            table.AddColumn(UI.Title_Bus);
            table.AddColumn(UI.Supported_Question);

            foreach(DeviceInfo dev in devices.OrderBy(d => d.Path))
            {
                table.AddRow(Markup.Escape(dev.Path   ?? ""),
                             Markup.Escape(dev.Vendor ?? ""),
                             Markup.Escape(dev.Model  ?? ""),
                             Markup.Escape(dev.Serial ?? ""),
                             Markup.Escape(dev.Bus    ?? ""),
                             dev.Supported ? "[green]✓[/]" : "[red]✗[/]");
            }

            AnsiConsole.Write(table);
        }

        return (int)ErrorNumber.NoError;
    }
}