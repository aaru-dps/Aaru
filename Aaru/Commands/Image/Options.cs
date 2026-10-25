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
//     Lists all options supported by writable media image formats.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Localization;
using Aaru.Logging;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Aaru.Commands.Image;

sealed class ListOptionsCommand : Command<ListOptionsCommand.Settings>
{
    const string MODULE_NAME = "List-Options command";

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)

    {
        MainClass.PrintCopyright();

        AaruLogging.Debug(MODULE_NAME, "--debug={0}",   settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}", settings.Verbose);
        Statistics.AddCommand("list-options");

        PluginRegister plugins = PluginRegister.Singleton;

        AaruLogging.WriteLine(UI.Read_Write_media_images_options);
        AaruLogging.Information(UI.Read_Write_media_images_options);

        foreach(IBaseWritableImage plugin in plugins.WritableImages.Values)
        {
            if(plugin is null) continue;

            var options = plugin.SupportedOptions.ToList();

            if(options.Count == 0) continue;

            var table = new Table
            {
                Title = new TableTitle(string.Format(UI.Options_for_0, plugin.Name))
            };

            table.AddColumn(new TableColumn(new Markup(UI.Title_Name).Centered()));
            table.AddColumn(new TableColumn(new Markup(UI.Title_Type).Centered()));
            table.AddColumn(new TableColumn(new Markup(UI.Default).Centered()));
            table.AddColumn(new TableColumn(new Markup(UI.Title_Description).Centered()));
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            foreach((string name, Type type, string description, object @default) option in
                    options.OrderBy(static t => t.name))
            {
                table.AddRow($"[darkgreen]{Markup.Escape(option.name)}[/]",
                             $"[italic][olive]{TypeToString(option.type)}[/][/]",
                             $"[italic][aqua]{option.@default?.ToString() ?? ""}[/][/]",
                             $"[slateblue1]{Markup.Escape(option.description)}[/]");

                AaruLogging.Information("({Name}) - {Type} - {Default} - {Description}",
                                        option.name,
                                        TypeToString(option.type),
                                        option.@default?.ToString() ?? "",
                                        option.description);
            }

            AnsiConsole.Write(table);
            AaruLogging.WriteLine();
        }

        return (int)ErrorNumber.NoError;
    }

    [NotNull]
    static string TypeToString([NotNull] Type type)
    {
        if(type == typeof(bool)) return UI.TypeToString_boolean;

        if(type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long))
            return UI.TypeToString_signed_number;

        if(type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong))
            return UI.TypeToString_number;

        if(type == typeof(float) || type == typeof(double)) return UI.TypeToString_float_number;

        if(type == typeof(Guid)) return UI.TypeToString_uuid;

        return type == typeof(string) ? UI.TypeToString_string : type.ToString();
    }

#region Nested type: Settings

    public class Settings : ImageFamily {}

#endregion
}