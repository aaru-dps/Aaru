// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Main.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Main program loop.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the main program loop.
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
// Copyright © 2020-2022 Rebecca Wallander
// ****************************************************************************/

using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Aaru.Commands;
using Aaru.Commands.Archive;
using Aaru.Commands.Database;
using Aaru.Commands.Device;
using Aaru.Commands.Filesystem;
using Aaru.Commands.Image;
using Aaru.Commands.Media;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Core;
using Aaru.Database;
using Aaru.Settings;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Aaru;

internal class MainClass
{
    static string                                _assemblyCopyright;
    static string                                _assemblyTitle;
    static AssemblyInformationalVersionAttribute _assemblyVersion;

    public static int Main([NotNull] string[] args)
    {
        IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(System.Console.Error)
        });

        object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        _assemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
        attributes     = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

        _assemblyVersion =
            Attribute.GetCustomAttribute(typeof(MainClass).Assembly, typeof(AssemblyInformationalVersionAttribute))
                as AssemblyInformationalVersionAttribute;

        _assemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

        if(args.Length                == 1 &&
           args[0].ToLowerInvariant() == "gui")
        {
            return Gui.Main.Start(args);
        }

        AaruConsole.WriteLineEvent += (format, objects) =>
        {
            if(objects is null)
                AnsiConsole.MarkupLine(format);
            else
                AnsiConsole.MarkupLine(format, objects);
        };

        AaruConsole.WriteEvent += (format, objects) =>
        {
            if(objects is null)
                AnsiConsole.Markup(format);
            else
                AnsiConsole.Markup(format, objects);
        };

        AaruConsole.ErrorWriteLineEvent += (format, objects) =>
        {
            if(objects is null)
                stderrConsole.MarkupLine(format);
            else
                stderrConsole.MarkupLine(format, objects);
        };

        Settings.Settings.LoadSettings();

        AaruContext ctx =null;

        try
        {
            ctx = AaruContext.Create(Settings.Settings.LocalDbPath, false);
            ctx.Database.Migrate();
        }
        catch(NotSupportedException)
        {
            try
            {
                ctx?.Database.CloseConnection();
                ctx?.Dispose();
            }
            catch(Exception)
            {
                // Should not ever arrive here, but if it does, keep trying to replace it anyway
            }

            File.Delete(Settings.Settings.LocalDbPath);
            ctx = AaruContext.Create(Settings.Settings.LocalDbPath);
            ctx.Database.EnsureCreated();

            ctx.Database.
                ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT PRIMARY KEY, \"ProductVersion\" TEXT)");

            foreach(string migration in ctx.Database.GetPendingMigrations())
            {
                ctx.Database.
                    ExecuteSqlRaw($"INSERT INTO \"__EFMigrationsHistory\" (MigrationId, ProductVersion) VALUES ('{migration}', '0.0.0')");
            }

            ctx.SaveChanges();
        }

        // Remove duplicates
        foreach(var duplicate in ctx.SeenDevices.AsEnumerable()!.GroupBy(a => new
                {
                    a.Manufacturer,
                    a.Model,
                    a.Revision,
                    a.Bus
                }).Where(a => a.Count() > 1).Distinct().Select(a => a.Key))
            ctx.RemoveRange(ctx.SeenDevices!.
                                Where(d => d.Manufacturer == duplicate.Manufacturer && d.Model == duplicate.Model &&
                                           d.Revision     == duplicate.Revision     && d.Bus == duplicate.Bus).Skip(1));

        // Remove nulls
        ctx.RemoveRange(ctx.SeenDevices!.Where(d => d.Manufacturer == null && d.Model == null &&
                                                    d.Revision     == null));

        ctx.SaveChanges();

        bool mainDbUpdate = false;

        if(!File.Exists(Settings.Settings.MainDbPath))
        {
            mainDbUpdate = true;
            UpdateCommand.DoUpdate(true);
        }

        var mainContext = AaruContext.Create(Settings.Settings.MainDbPath, false);

        if(mainContext.Database.GetPendingMigrations().Any())
        {
            AaruConsole.WriteLine("New database version, updating...");

            try
            {
                File.Delete(Settings.Settings.MainDbPath);
            }
            catch(Exception)
            {
                AaruConsole.ErrorWriteLine("Exception trying to remove old database version, cannot continue...");
                AaruConsole.ErrorWriteLine("Please manually remove file at {0}", Settings.Settings.MainDbPath);

                return (int)ErrorNumber.CannotRemoveDatabase;
            }

            mainContext.Database.CloseConnection();
            mainContext.Dispose();
            UpdateCommand.DoUpdate(true);
        }

        // GDPR level compliance does not match and there are no arguments or the arguments are neither GUI neither configure.
        if(Settings.Settings.Current.GdprCompliance < DicSettings.GDPR_LEVEL &&
           (args.Length < 1 || (args.Length                >= 1 && args[0].ToLowerInvariant() != "gui" &&
                                args[0].ToLowerInvariant() != "configure")))
            new ConfigureCommand().DoConfigure(true);

        Statistics.LoadStats();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var rootCommand = new RootCommand();

        rootCommand.AddGlobalOption(new Option(new[]
                                    {
                                        "--verbose", "-v"
                                    }, "Shows verbose output.")
                                    {
                                        Argument = new Argument<bool>(() => false)
                                    });

        rootCommand.AddGlobalOption(new Option(new[]
                                    {
                                        "--debug", "-d"
                                    }, "Shows debug output from plugins.")
                                    {
                                        Argument = new Argument<bool>(() => false)
                                    });

        rootCommand.AddGlobalOption(new Option(new[]
                                    {
                                        "--pause"
                                    }, "Pauses before exiting.")
                                    {
                                        Argument = new Argument<bool>(() => false)
                                    });

        rootCommand.Description =
            $"{_assemblyTitle} {_assemblyVersion?.InformationalVersion}\n{_assemblyCopyright}";

        rootCommand.AddCommand(new DatabaseFamily(mainDbUpdate));
        rootCommand.AddCommand(new DeviceFamily());
        rootCommand.AddCommand(new FilesystemFamily());
        rootCommand.AddCommand(new ImageFamily());
        rootCommand.AddCommand(new MediaFamily());
        rootCommand.AddCommand(new ArchiveFamily());

        rootCommand.AddCommand(new ConfigureCommand());
        rootCommand.AddCommand(new FormatsCommand());
        rootCommand.AddCommand(new ListEncodingsCommand());
        rootCommand.AddCommand(new ListNamespacesCommand());
        rootCommand.AddCommand(new RemoteCommand());

        int ret = rootCommand.Invoke(args);

        Statistics.SaveStats();

        if(rootCommand.Parse(args).RootCommandResult.ValueForOption("--pause")?.Equals(true) != true)
            return ret;

        AaruConsole.WriteLine("Press any key to exit.");
        System.Console.ReadKey();

        return ret;
    }

    internal static void PrintCopyright()
    {
        AaruConsole.WriteLine("[bold][red]{0}[/] [green]{1}[/][/]", _assemblyTitle,
                              _assemblyVersion?.InformationalVersion);

        AaruConsole.WriteLine("[bold][blue]{0}[/][/]", _assemblyCopyright);
        AaruConsole.WriteLine();
    }
}