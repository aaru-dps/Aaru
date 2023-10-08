// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Options.cs
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

using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using JetBrains.Annotations;
using Spectre.Console;

namespace Aaru.Commands.Filesystem;

sealed class ListOptionsCommand : Command
{
    const string MODULE_NAME = "List-Options command";

    public ListOptionsCommand() : base("options", UI.Filesystem_Options_Command_Description) =>
        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());

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

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",   debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}", verbose);
        Statistics.AddCommand("list-options");

        PluginRegister plugins = PluginRegister.Singleton;

        AaruConsole.WriteLine(UI.Read_only_filesystems_options);

        foreach(IReadOnlyFilesystem fs in plugins.ReadOnlyFilesystems.Values)
        {
            if(fs is null)
                continue;

            var options = fs.SupportedOptions.ToList();

            if(options.Count == 0)
                continue;

            var table = new Table
            {
                Title = new TableTitle(string.Format(UI.Options_for_0, fs.Name))
            };

            table.AddColumn(UI.Title_Name);
            table.AddColumn(UI.Title_Type);
            table.AddColumn(UI.Title_Description);

            foreach((string name, Type type, string description) option in options.OrderBy(t => t.name))
            {
                table.AddRow(Markup.Escape(option.name), $"[italic]{TypeToString(option.type)}[/]",
                             Markup.Escape(option.description));
            }

            AnsiConsole.Write(table);
            AaruConsole.WriteLine();
        }

        return (int)ErrorNumber.NoError;
    }

    [NotNull]
    static string TypeToString([NotNull] Type type)
    {
        if(type == typeof(bool))
            return UI.TypeToString_boolean;

        if(type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long))
            return UI.TypeToString_signed_number;

        if(type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong))
            return UI.TypeToString_number;

        if(type == typeof(float) || type == typeof(double))
            return UI.TypeToString_float_number;

        if(type == typeof(Guid))
            return UI.TypeToString_uuid;

        return type == typeof(string) ? UI.TypeToString_string : type.ToString();
    }
}