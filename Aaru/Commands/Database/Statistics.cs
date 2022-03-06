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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.CommandLine.Invocation;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Database;
using Aaru.Database.Models;
using Spectre.Console;
using Command = System.CommandLine.Command;

namespace Aaru.Commands.Database;

internal sealed class StatisticsCommand : Command
{
    public StatisticsCommand() : base("stats", "Shows statistics.") =>
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

        var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        if(!ctx.Commands.Any()     &&
           !ctx.Filesystems.Any()  &&
           !ctx.Filters.Any()      &&
           !ctx.MediaFormats.Any() &&
           !ctx.Medias.Any()       &&
           !ctx.Partitions.Any()   &&
           !ctx.SeenDevices.Any())
        {
            AaruConsole.WriteLine("There are no statistics.");

            return (int)ErrorNumber.NothingFound;
        }

        bool  thereAreStats = false;
        Table table;

        if(ctx.Commands.Any())
        {
            table = new Table
            {
                Title = new TableTitle("Commands statistics")
            };

            table.AddColumn("Command");
            table.AddColumn("Times used");
            table.Columns[1].RightAligned();

            if(ctx.Commands.Any(c => c.Name == "analyze"))
            {
                foreach(Aaru.Database.Models.Command oldAnalyze in ctx.Commands.Where(c => c.Name == "analyze"))
                {
                    oldAnalyze.Name = "fs-info";
                    ctx.Commands.Update(oldAnalyze);
                }

                ulong count = 0;

                foreach(Aaru.Database.Models.Command fsInfo in ctx.Commands.Where(c => c.Name == "fs-info" &&
                            c.Synchronized))
                {
                    count += fsInfo.Count;
                    ctx.Remove(fsInfo);
                }

                if(count > 0)
                    ctx.Commands.Add(new Aaru.Database.Models.Command
                    {
                        Count        = count,
                        Name         = "fs-info",
                        Synchronized = true
                    });

                ctx.SaveChanges();
            }

            foreach(string command in ctx.Commands.Select(c => c.Name).Distinct().OrderBy(c => c))
            {
                ulong count = ctx.Commands.Where(c => c.Name == command && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == command && !c.Synchronized);

                if(count == 0)
                    continue;

                table.AddRow(Markup.Escape(command), $"{count}");
                thereAreStats = true;
            }

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
        }

        if(ctx.Filters.Any())
        {
            table = new Table
            {
                Title = new TableTitle("Filters statistics")
            };

            table.AddColumn("Filter");
            table.AddColumn("Times found");
            table.Columns[1].RightAligned();

            foreach(string filter in ctx.Filters.Select(c => c.Name).Distinct().OrderBy(c => c))
            {
                ulong count = ctx.Filters.Where(c => c.Name == filter && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Filters.LongCount(c => c.Name == filter && !c.Synchronized);

                if(count == 0)
                    continue;

                table.AddRow(Markup.Escape(filter), $"{count}");
                thereAreStats = true;
            }

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
        }

        if(ctx.MediaFormats.Any())
        {
            table = new Table
            {
                Title = new TableTitle("Media image format statistics")
            };

            table.AddColumn("Format");
            table.AddColumn("Times found");
            table.Columns[1].RightAligned();

            foreach(string format in ctx.MediaFormats.Select(c => c.Name).Distinct().OrderBy(c => c))
            {
                ulong count = ctx.MediaFormats.Where(c => c.Name == format && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.MediaFormats.LongCount(c => c.Name == format && !c.Synchronized);

                if(count == 0)
                    continue;

                table.AddRow(Markup.Escape(format), $"{count}");
                thereAreStats = true;
            }

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
        }

        if(ctx.Partitions.Any())
        {
            table = new Table
            {
                Title = new TableTitle("Partitioning scheme statistics")
            };

            table.AddColumn("Scheme");
            table.AddColumn("Times found");
            table.Columns[1].RightAligned();

            foreach(string partition in ctx.Partitions.Select(c => c.Name).Distinct().OrderBy(c => c))
            {
                ulong count = ctx.Partitions.Where(c => c.Name == partition && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Partitions.LongCount(c => c.Name == partition && !c.Synchronized);

                if(count == 0)
                    continue;

                table.AddRow(Markup.Escape(partition), $"{count}");
                thereAreStats = true;
            }

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
        }

        if(ctx.Filesystems.Any())
        {
            table = new Table
            {
                Title = new TableTitle("Filesystem statistics")
            };

            table.AddColumn("Filesystem");
            table.AddColumn("Times found");
            table.Columns[1].RightAligned();

            foreach(string filesystem in ctx.Filesystems.Select(c => c.Name).Distinct().OrderBy(c => c))
            {
                ulong count = ctx.Filesystems.Where(c => c.Name == filesystem && c.Synchronized).
                                  Select(c => c.Count).FirstOrDefault();

                count += (ulong)ctx.Filesystems.LongCount(c => c.Name == filesystem && !c.Synchronized);

                if(count == 0)
                    continue;

                table.AddRow(Markup.Escape(filesystem), $"{count}");
                thereAreStats = true;
            }

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
        }

        if(ctx.SeenDevices.Any())
        {
            table = new Table
            {
                Title = new TableTitle("Device statistics")
            };

            table.AddColumn("Manufacturer");
            table.AddColumn("Model");
            table.AddColumn("Revision");
            table.AddColumn("Bus");

            foreach(DeviceStat ds in ctx.SeenDevices.OrderBy(ds => ds.Manufacturer).ThenBy(ds => ds.Model).
                                         ThenBy(ds => ds.Revision).ThenBy(ds => ds.Bus))
                table.AddRow(Markup.Escape(ds.Manufacturer ?? ""), Markup.Escape(ds.Model ?? ""),
                             Markup.Escape(ds.Revision     ?? ""), Markup.Escape(ds.Bus   ?? ""));

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
            thereAreStats = true;
        }

        if(ctx.Medias.Any(ms => ms.Real))
        {
            table = new Table
            {
                Title = new TableTitle("Media found in real device statistics")
            };

            table.AddColumn("Type");
            table.AddColumn("Times found");
            table.Columns[1].RightAligned();

            foreach(string media in ctx.Medias.Where(ms => ms.Real).Select(ms => ms.Type).Distinct().
                                        OrderBy(ms => ms))
            {
                ulong count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && c.Real).
                                  Select(c => c.Count).FirstOrDefault();

                count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && c.Real);

                if(count <= 0)
                    continue;

                table.AddRow(Markup.Escape(media), $"{count}");

                thereAreStats = true;
            }

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
        }

        if(ctx.Medias.Any(ms => !ms.Real))
        {
            table = new Table
            {
                Title = new TableTitle("Media found in images statistics")
            };

            table.AddColumn("Type");
            table.AddColumn("Times found");
            table.Columns[1].RightAligned();

            foreach(string media in ctx.Medias.Where(ms => !ms.Real).Select(ms => ms.Type).Distinct().
                                        OrderBy(ms => ms))
            {
                ulong count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && !c.Real).
                                  Select(c => c.Count).FirstOrDefault();

                count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && !c.Real);

                if(count <= 0)
                    continue;

                table.AddRow(Markup.Escape(media), $"{count}");

                thereAreStats = true;
            }

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
        }

        if(!thereAreStats)
            AaruConsole.WriteLine("There are no statistics.");

        return (int)ErrorNumber.NoError;
    }
}