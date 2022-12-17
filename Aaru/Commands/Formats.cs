// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Formats.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'formats' command.
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

sealed class FormatsCommand : Command
{
    public FormatsCommand() : base("formats", UI.List_Formats_Command_Description) =>
        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));

    public static int Invoke(bool verbose, bool debug)
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

        Statistics.AddCommand("formats");

        AaruConsole.DebugWriteLine("Formats command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("Formats command", "--verbose={0}", verbose);

        PluginBase plugins     = GetPluginBase.Instance;
        var        filtersList = new FiltersList();

        Table table = new()
        {
            Title = new TableTitle(string.Format(UI.Supported_filters_0, filtersList.Filters.Count))
        };

        if(verbose)
            table.AddColumn(UI.Title_GUID);

        table.AddColumn(UI.Title_Filter);

        foreach(KeyValuePair<string, IFilter> kvp in filtersList.Filters)
            if(verbose)
                table.AddRow(kvp.Value.Id.ToString(), Markup.Escape(kvp.Value.Name));
            else
                table.AddRow(Markup.Escape(kvp.Value.Name));

        AnsiConsole.Write(table);

        AaruConsole.WriteLine();

        table = new Table
        {
            Title = new TableTitle(string.Format(UI.Read_only_media_image_formats_0,
                                                 plugins.MediaImages.Count(t => !t.Value.GetInterfaces().
                                                                               Contains(typeof(IWritableImage)))))
        };

        if(verbose)
            table.AddColumn(UI.Title_GUID);

        table.AddColumn(UI.Title_Media_image_format);

        foreach(KeyValuePair<string, Type> kvp in plugins.MediaImages.Where(t => !t.Value.GetInterfaces().
                                                                                Contains(typeof(IWritableImage))))
        {
            if(Activator.CreateInstance(kvp.Value) is not IMediaImage imagePlugin)
                continue;

            if(verbose)
                table.AddRow(imagePlugin.Id.ToString(), Markup.Escape(imagePlugin.Name));
            else
                table.AddRow(Markup.Escape(imagePlugin.Name));
        }

        AnsiConsole.Write(table);

        AaruConsole.WriteLine();

        table = new Table
        {
            Title = new TableTitle(string.Format(UI.Read_write_media_image_formats_0, plugins.WritableImages.Count))
        };

        if(verbose)
            table.AddColumn(UI.Title_GUID);

        table.AddColumn(UI.Title_Media_image_format);

        foreach(KeyValuePair<string, Type> kvp in plugins.WritableImages)
        {
            if(Activator.CreateInstance(kvp.Value) is not IBaseWritableImage plugin)
                continue;

            if(verbose)
                table.AddRow(plugin.Id.ToString(), Markup.Escape(plugin.Name));
            else
                table.AddRow(Markup.Escape(plugin.Name));
        }

        AnsiConsole.Write(table);

        AaruConsole.WriteLine();

        table = new Table
        {
            Title = new TableTitle(string.Format(UI.Supported_filesystems_for_identification_and_information_only_0,
                                                 plugins.Filesystems.Count(t => !t.Value.GetInterfaces().
                                                                               Contains(typeof(
                                                                                   IReadOnlyFilesystem)))))
        };

        if(verbose)
            table.AddColumn(UI.Title_GUID);

        table.AddColumn(UI.Title_Filesystem);

        foreach(KeyValuePair<string, Type> kvp in plugins.Filesystems.Where(t => !t.Value.GetInterfaces().
                                                                                Contains(typeof(
                                                                                    IReadOnlyFilesystem))))
        {
            if(Activator.CreateInstance(kvp.Value) is not IFilesystem fs)
                continue;

            if(verbose)
                table.AddRow(fs.Id.ToString(), Markup.Escape(fs.Name));
            else
                table.AddRow(Markup.Escape(fs.Name));
        }

        AnsiConsole.Write(table);

        AaruConsole.WriteLine();

        table = new Table
        {
            Title = new TableTitle(string.Format(UI.Supported_filesystems_that_can_read_their_contents_0,
                                                 plugins.ReadOnlyFilesystems.Count))
        };

        if(verbose)
            table.AddColumn(UI.Title_GUID);

        table.AddColumn(UI.Title_Filesystem);

        foreach(KeyValuePair<string, Type> kvp in plugins.ReadOnlyFilesystems)
        {
            if(Activator.CreateInstance(kvp.Value) is not IReadOnlyFilesystem fs)
                continue;

            if(verbose)
                table.AddRow(fs.Id.ToString(), Markup.Escape(fs.Name));
            else
                table.AddRow(Markup.Escape(fs.Name));
        }

        AnsiConsole.Write(table);

        AaruConsole.WriteLine();

        table = new Table
        {
            Title = new TableTitle(string.Format(UI.Supported_partitioning_schemes_0, plugins.Partitions.Count))
        };

        if(verbose)
            table.AddColumn(UI.Title_GUID);

        table.AddColumn(UI.Title_Scheme);

        foreach(KeyValuePair<string, Type> kvp in plugins.Partitions)
        {
            if(Activator.CreateInstance(kvp.Value) is not IPartition part)
                continue;

            if(verbose)
                table.AddRow(part.Id.ToString(), Markup.Escape(part.Name));
            else
                table.AddRow(Markup.Escape(part.Name));
        }

        AnsiConsole.Write(table);

        AaruConsole.WriteLine();

        table = new Table
        {
            Title = new TableTitle(string.Format(UI.Supported_archive_formats_0, plugins.Archives.Count))
        };

        if(verbose)
            table.AddColumn(UI.Title_GUID);

        table.AddColumn("Archive format");

        foreach(KeyValuePair<string, Type> kvp in plugins.Archives)
        {
            if(Activator.CreateInstance(kvp.Value) is not IArchive archive)
                continue;

            if(verbose)
                table.AddRow(archive.Id.ToString(), Markup.Escape(archive.Name));
            else
                table.AddRow(Markup.Escape(archive.Name));
        }

        AnsiConsole.Write(table);

        return (int)ErrorNumber.NoError;
    }
}