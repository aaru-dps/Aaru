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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.CommandLine.Invocation;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Database;
using Aaru.Database.Models;
using Command = System.CommandLine.Command;

namespace Aaru.Commands.Database
{
    internal sealed class StatisticsCommand : Command
    {
        public StatisticsCommand() : base("stats", "Shows statistics.") =>
            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));

        public static int Invoke(bool debug, bool verbose)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

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

            bool thereAreStats = false;

            if(ctx.Commands.Any())
            {
                AaruConsole.WriteLine("Commands statistics");
                AaruConsole.WriteLine("===================");

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

                foreach(string command in ctx.Commands.OrderBy(c => c.Name).Select(c => c.Name).Distinct())
                {
                    ulong count = ctx.Commands.Where(c => c.Name == command && c.Synchronized).Select(c => c.Count).
                                      FirstOrDefault();

                    count += (ulong)ctx.Commands.LongCount(c => c.Name == command && !c.Synchronized);

                    if(count == 0)
                        continue;

                    AaruConsole.WriteLine("You have called the {0} command {1} times.", command, count);
                    thereAreStats = true;
                }

                AaruConsole.WriteLine();
            }

            if(ctx.Filters.Any())
            {
                AaruConsole.WriteLine("Filters statistics");
                AaruConsole.WriteLine("==================");

                foreach(string filter in ctx.Filters.OrderBy(c => c.Name).Select(c => c.Name).Distinct())
                {
                    ulong count = ctx.Filters.Where(c => c.Name == filter && c.Synchronized).Select(c => c.Count).
                                      FirstOrDefault();

                    count += (ulong)ctx.Filters.LongCount(c => c.Name == filter && !c.Synchronized);

                    if(count == 0)
                        continue;

                    AaruConsole.WriteLine("Filter {0} has been found {1} times.", filter, count);
                    thereAreStats = true;
                }

                AaruConsole.WriteLine();
            }

            if(ctx.MediaFormats.Any())
            {
                AaruConsole.WriteLine("Media image statistics");
                AaruConsole.WriteLine("======================");

                foreach(string format in ctx.MediaFormats.OrderBy(c => c.Name).Select(c => c.Name).Distinct())
                {
                    ulong count = ctx.MediaFormats.Where(c => c.Name == format && c.Synchronized).Select(c => c.Count).
                                      FirstOrDefault();

                    count += (ulong)ctx.MediaFormats.LongCount(c => c.Name == format && !c.Synchronized);

                    if(count == 0)
                        continue;

                    AaruConsole.WriteLine("Format {0} has been found {1} times.", format, count);
                    thereAreStats = true;
                }

                AaruConsole.WriteLine();
            }

            if(ctx.Partitions.Any())
            {
                AaruConsole.WriteLine("Partition statistics");
                AaruConsole.WriteLine("====================");

                foreach(string partition in ctx.Partitions.OrderBy(c => c.Name).Select(c => c.Name).Distinct())
                {
                    ulong count = ctx.Partitions.Where(c => c.Name == partition && c.Synchronized).Select(c => c.Count).
                                      FirstOrDefault();

                    count += (ulong)ctx.Partitions.LongCount(c => c.Name == partition && !c.Synchronized);

                    if(count == 0)
                        continue;

                    AaruConsole.WriteLine("Partitioning scheme {0} has been found {1} times.", partition, count);
                    thereAreStats = true;
                }

                AaruConsole.WriteLine();
            }

            if(ctx.Filesystems.Any())
            {
                AaruConsole.WriteLine("Filesystem statistics");
                AaruConsole.WriteLine("=====================");

                foreach(string filesystem in ctx.Filesystems.OrderBy(c => c.Name).Select(c => c.Name).Distinct())
                {
                    ulong count = ctx.Filesystems.Where(c => c.Name == filesystem && c.Synchronized).
                                      Select(c => c.Count).FirstOrDefault();

                    count += (ulong)ctx.Filesystems.LongCount(c => c.Name == filesystem && !c.Synchronized);

                    if(count == 0)
                        continue;

                    AaruConsole.WriteLine("Filesystem {0} has been found {1} times.", filesystem, count);
                    thereAreStats = true;
                }

                AaruConsole.WriteLine();
            }

            if(ctx.SeenDevices.Any())
            {
                AaruConsole.WriteLine("Device statistics");
                AaruConsole.WriteLine("=================");

                foreach(DeviceStat ds in ctx.SeenDevices.OrderBy(ds => ds.Manufacturer).ThenBy(ds => ds.Model).
                                             ThenBy(ds => ds.Revision).ThenBy(ds => ds.Bus))
                    AaruConsole.
                        WriteLine("Device model {0}, manufactured by {1}, with revision {2} and attached via {3}.",
                                  ds.Model, ds.Manufacturer, ds.Revision, ds.Bus);

                AaruConsole.WriteLine();
                thereAreStats = true;
            }

            if(ctx.Medias.Any())
            {
                AaruConsole.WriteLine("Media statistics");
                AaruConsole.WriteLine("================");

                foreach(string media in ctx.Medias.OrderBy(ms => ms.Type).Select(ms => ms.Type).Distinct())
                {
                    ulong count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && c.Real).
                                      Select(c => c.Count).FirstOrDefault();

                    count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && c.Real);

                    if(count > 0)
                    {
                        AaruConsole.WriteLine("Media type {0} has been found {1} times in a real device.", media,
                                              count);

                        thereAreStats = true;
                    }

                    count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && !c.Real).Select(c => c.Count).
                                FirstOrDefault();

                    count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && !c.Real);

                    if(count == 0)
                        continue;

                    AaruConsole.WriteLine("Media type {0} has been found {1} times in a media image.", media, count);
                    thereAreStats = true;
                }

                AaruConsole.WriteLine();
            }

            if(!thereAreStats)
                AaruConsole.WriteLine("There are no statistics.");

            return (int)ErrorNumber.NoError;
        }
    }
}