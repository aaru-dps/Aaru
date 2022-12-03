// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ListNamespaces.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Lists all options supported by read-only filesystems.
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

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using Spectre.Console;

namespace Aaru.Commands;

sealed class ListNamespacesCommand : Command
{
    public ListNamespacesCommand() : base("list-namespaces", UI.List_Namespaces_Command_Description) =>
        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));

    public static int Invoke(bool debug, bool verbose)
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

        AaruConsole.DebugWriteLine("List-Namespaces command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("List-Namespaces command", "--verbose={0}", verbose);
        Statistics.AddCommand("list-namespaces");

        PluginBase plugins = GetPluginBase.Instance;

        foreach(KeyValuePair<string, IReadOnlyFilesystem> kvp in
                plugins.ReadOnlyFilesystems.Where(kvp => kvp.Value.Namespaces is not null))
        {
            Table table = new()
            {
                Title = new TableTitle(string.Format(UI.Namespaces_for_0, kvp.Value.Name))
            };

            table.AddColumn(UI.Title_Namespace);
            table.AddColumn(UI.Title_Description);

            foreach(KeyValuePair<string, string> @namespace in kvp.Value.Namespaces.OrderBy(t => t.Key))
                table.AddRow(@namespace.Key, @namespace.Value);

            AnsiConsole.Write(table);
            AaruConsole.WriteLine();
        }

        return (int)ErrorNumber.NoError;
    }
}