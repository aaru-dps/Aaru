// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Statistics.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'stats' command.
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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aaru.CommonTypes.Enums;
using Aaru.Database;
using Aaru.Database.Models;
using Aaru.Localization;
using Aaru.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Command = Aaru.Database.Models.Command;

namespace Aaru.Commands.Database;

sealed class StatisticsCommand : AsyncCommand<StatisticsCommand.Settings>
{
    protected override async Task<int> ExecuteAsync(CommandContext    context, Settings settings,
                                                    CancellationToken cancellationToken)

    {
        MainClass.PrintCopyright();

        AaruLogging.Information(UI.Database_statistics_command);

        var ctx = AaruContext.Create(Aaru.Settings.Settings.LocalDbPath);

        if(!ctx.Commands.Any()     &&
           !ctx.Filesystems.Any()  &&
           !ctx.Filters.Any()      &&
           !ctx.MediaFormats.Any() &&
           !ctx.Medias.Any()       &&
           !ctx.Partitions.Any()   &&
           !ctx.SeenDevices.Any())
        {
            AaruLogging.WriteLine(UI.There_are_no_statistics);
            AaruLogging.Information(UI.There_are_no_statistics);

            return (int)ErrorNumber.NothingFound;
        }

        var   thereAreStats = false;
        Table table;

        if(ctx.Commands.Any())
        {
            table = new Table
            {
                Title = new TableTitle(UI.Commands_statistics)
            };

            AaruLogging.Information(UI.Commands_statistics);

            table.AddColumn(new TableColumn(new Markup(UI.Title_Command).Centered()));
            table.AddColumn(new TableColumn(new Markup(UI.Title_Times_used).Centered()));
            table.Columns[1].RightAligned();
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            if(ctx.Commands.Any(static c => c.Name == "analyze"))
            {
                foreach(Command oldAnalyze in ctx.Commands.Where(static c => c.Name == "analyze"))
                {
                    oldAnalyze.Name = "fs-info";
                    ctx.Commands.Update(oldAnalyze);
                }

                ulong count = 0;

                foreach(Command fsInfo in ctx.Commands.Where(static c => c.Name == "fs-info" && c.Synchronized))
                {
                    count += fsInfo.Count;
                    ctx.Remove(fsInfo);
                }

                if(count > 0)
                {
                    await ctx.Commands.AddAsync(new Command
                    {
                        Count        = count,
                        Name         = "fs-info",
                        Synchronized = true
                    });
                }

                await ctx.SaveChangesAsync(cancellationToken);
            }

            foreach(string command in ctx.Commands.Select(static c => c.Name).Distinct().OrderBy(static c => c))
            {
                ulong count = ctx.Commands.Where(c => c.Name == command && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == command && !c.Synchronized);

                if(count == 0) continue;

                table.AddRow($"[italic][purple]{Markup.Escape(command)}[/][/]", $"[italic][aqua]{count}[/][/]");
                AaruLogging.Information("({Command}) - {Count}", command, count);
                thereAreStats = true;
            }

            AnsiConsole.Write(table);
            AaruLogging.WriteLine();
        }

        if(ctx.Filters.Any())
        {
            table = new Table
            {
                Title = new TableTitle(UI.Filters_statistics)
            };

            AaruLogging.Information(UI.Filters_statistics);

            table.AddColumn(new TableColumn(new Markup(UI.Title_Filter).Centered()));
            table.AddColumn(new TableColumn(new Markup(UI.Title_Times_used).Centered()));
            table.Columns[1].RightAligned();
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            foreach(string filter in ctx.Filters.Select(static c => c.Name).Distinct().OrderBy(static c => c))
            {
                ulong count = ctx.Filters.Where(c => c.Name == filter && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Filters.LongCount(c => c.Name == filter && !c.Synchronized);

                if(count == 0) continue;

                table.AddRow($"[italic][purple]{Markup.Escape(filter)}[/][/]", $"[italic][aqua]{count}[/][/]");
                AaruLogging.Information("({Filter}) - {Count}", filter, count);
                thereAreStats = true;
            }

            AnsiConsole.Write(table);
            AaruLogging.WriteLine();
        }

        if(ctx.MediaFormats.Any())
        {
            table = new Table
            {
                Title = new TableTitle(UI.Media_image_format_statistics)
            };

            AaruLogging.Information(UI.Media_image_format_statistics);

            table.AddColumn(new TableColumn(new Markup(UI.Title_Format).Centered()));
            table.AddColumn(new TableColumn(new Markup(UI.Title_Times_used).Centered()));
            table.Columns[1].RightAligned();
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            foreach(string format in ctx.MediaFormats.Select(static c => c.Name).Distinct().OrderBy(static c => c))
            {
                ulong count = ctx.MediaFormats.Where(c => c.Name == format && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.MediaFormats.LongCount(c => c.Name == format && !c.Synchronized);

                if(count == 0) continue;

                table.AddRow($"[italic][purple]{Markup.Escape(format)}[/][/]", $"[italic][aqua]{count}[/][/]");
                AaruLogging.Information("({Format}) - {Count}", format, count);
                thereAreStats = true;
            }

            AnsiConsole.Write(table);
            AaruLogging.WriteLine();
        }

        if(ctx.Partitions.Any())
        {
            table = new Table
            {
                Title = new TableTitle(UI.Partitioning_scheme_statistics)
            };

            AaruLogging.Information(UI.Partitioning_scheme_statistics);

            table.AddColumn(new TableColumn(new Markup(UI.Title_Scheme).Centered()));
            table.AddColumn(new TableColumn(new Markup(UI.Title_Times_used).Centered()));
            table.Columns[1].RightAligned();
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            foreach(string partition in ctx.Partitions.Select(static c => c.Name).Distinct().OrderBy(static c => c))
            {
                ulong count = ctx.Partitions.Where(c => c.Name == partition && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Partitions.LongCount(c => c.Name == partition && !c.Synchronized);

                if(count == 0) continue;

                table.AddRow($"[italic][purple]{Markup.Escape(partition)}[/][/]", $"[italic][aqua]{count}[/][/]");
                AaruLogging.Information("({Partition}) - {Count}", partition, count);
                thereAreStats = true;
            }

            AnsiConsole.Write(table);
            AaruLogging.WriteLine();
        }

        if(ctx.Filesystems.Any())
        {
            table = new Table
            {
                Title = new TableTitle(UI.Filesystem_statistics)
            };

            AaruLogging.Information(UI.Filesystem_statistics);

            table.AddColumn(new TableColumn(new Markup(UI.Title_Filesystem).Centered()));
            table.AddColumn(new TableColumn(new Markup(UI.Title_Times_used).Centered()));
            table.Columns[1].RightAligned();
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            foreach(string filesystem in ctx.Filesystems.Select(static c => c.Name).Distinct().OrderBy(static c => c))
            {
                ulong count = ctx.Filesystems.Where(c => c.Name == filesystem && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Filesystems.LongCount(c => c.Name == filesystem && !c.Synchronized);

                if(count == 0) continue;

                table.AddRow($"[italic][purple]{Markup.Escape(filesystem)}[/][/]", $"[italic][aqua]{count}[/][/]");
                AaruLogging.Information("({Filesystem}) - {Count}", filesystem, count);
                thereAreStats = true;
            }

            AnsiConsole.Write(table);
            AaruLogging.WriteLine();
        }

        if(ctx.SeenDevices.Any())
        {
            table = new Table
            {
                Title = new TableTitle(UI.Device_statistics)
            };

            AaruLogging.Information(UI.Device_statistics);

            table.AddColumn(UI.Title_Manufacturer);
            table.AddColumn(UI.Title_Model);
            table.AddColumn(UI.Title_Revision);
            table.AddColumn(UI.Title_Bus);
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            foreach(DeviceStat ds in ctx.SeenDevices.OrderBy(static ds => ds.Manufacturer)
                                        .ThenBy(static ds => ds.Model)
                                        .ThenBy(static ds => ds.Revision)
                                        .ThenBy(static ds => ds.Bus))
            {
                table.AddRow($"[italic][blue]{Markup.Escape(ds.Manufacturer ?? "")}[/][/]",
                             $"[italic][purple]{Markup.Escape(ds.Model      ?? "")}[/][/]",
                             $"[italic][teal]{Markup.Escape(ds.Revision     ?? "")}[/][/]",
                             $"[italic][rosybrown]{Markup.Escape(ds.Bus     ?? "")}[/][/]");

                AaruLogging.Information("({Manufacturer}) - {Model} {Revision} ({Bus})",
                                        ds.Manufacturer ?? "",
                                        ds.Model        ?? "",
                                        ds.Revision     ?? "",
                                        ds.Bus          ?? "");
            }

            AnsiConsole.Write(table);
            AaruLogging.WriteLine();
            thereAreStats = true;
        }

        if(ctx.Medias.Any(static ms => ms.Real))
        {
            table = new Table
            {
                Title = new TableTitle(UI.Media_found_in_real_device_statistics)
            };

            AaruLogging.Information(UI.Media_found_in_real_device_statistics);

            table.AddColumn(new TableColumn(new Markup($"[bold][purple]{Localization.Core.Title_Type_for_media}[/][/]")
                                               .Centered()));

            table.AddColumn(new TableColumn(new Markup(UI.Title_Times_used).Centered()));
            table.Columns[1].RightAligned();
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            foreach(string media in ctx.Medias.Where(static ms => ms.Real)
                                       .Select(static ms => ms.Type)
                                       .Distinct()
                                       .OrderBy(static ms => ms))
            {
                ulong count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && c.Real)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && c.Real);

                if(count <= 0) continue;

                table.AddRow($"[italic][purple]{Markup.Escape(media)}[/][/]", $"[italic][aqua]{count}[/][/]");
                AaruLogging.Information("({Media}) - {Count}", media, count);

                thereAreStats = true;
            }

            AnsiConsole.Write(table);
            AaruLogging.WriteLine();
        }

        if(ctx.Medias.Any(static ms => !ms.Real))
        {
            table = new Table
            {
                Title = new TableTitle(UI.Media_found_in_images_statistics)
            };

            AaruLogging.Information(UI.Media_found_in_images_statistics);

            table.AddColumn(new TableColumn(new Markup($"[bold][purple]{Localization.Core.Title_Type_for_media}[/][/]")
                                               .Centered()));

            table.AddColumn(new TableColumn(new Markup(UI.Title_Times_used).Centered()));
            table.Columns[1].RightAligned();
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            foreach(string media in ctx.Medias.Where(static ms => !ms.Real)
                                       .Select(static ms => ms.Type)
                                       .Distinct()
                                       .OrderBy(static ms => ms))
            {
                ulong count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && !c.Real)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && !c.Real);

                if(count <= 0) continue;

                table.AddRow($"[italic][purple]{Markup.Escape(media)}[/][/]", $"[italic][aqua]{count}[/][/]");
                AaruLogging.Information("({Media}) - {Count}", media, count);

                thereAreStats = true;
            }

            AnsiConsole.Write(table);
            AaruLogging.WriteLine();
        }

        if(thereAreStats) return (int)ErrorNumber.NoError;

        AaruLogging.WriteLine(UI.There_are_no_statistics);
        AaruLogging.Information(UI.There_are_no_statistics);

        return (int)ErrorNumber.NoError;
    }

#region Nested type: Settings

    public class Settings : DatabaseFamily {}

#endregion
}