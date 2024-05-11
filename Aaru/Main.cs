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
// Copyright © 2011-2024 Natalia Portillo
// Copyright © 2020-2024 Rebecca Wallander
// ****************************************************************************/

using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
using Aaru.Localization;
using Aaru.Settings;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Aaru;

class MainClass
{
    static string                                _assemblyCopyright;
    static string                                _assemblyTitle;
    static AssemblyInformationalVersionAttribute _assemblyVersion;

    public static async Task<int> Main([NotNull] string[] args)
    {
        IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(System.Console.Error)
        });

        object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        _assemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
        attributes     = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

        _assemblyVersion =
            Attribute.GetCustomAttribute(typeof(MainClass).Assembly, typeof(AssemblyInformationalVersionAttribute)) as
                AssemblyInformationalVersionAttribute;

        _assemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

        if(args.Length == 1 && args[0].Equals("gui", StringComparison.InvariantCultureIgnoreCase))
            return Gui.Main.Start(args);

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

        AaruContext ctx = null;

        try
        {
            ctx = AaruContext.Create(Settings.Settings.LocalDbPath, false);
            await ctx.Database.MigrateAsync();
        }
        catch(NotSupportedException)
        {
            try
            {
                if(ctx is not null)
                {
                    await ctx.Database.CloseConnectionAsync();
                    await ctx.DisposeAsync();
                }
            }
            catch(Exception)
            {
                // Should not ever arrive here, but if it does, keep trying to replace it anyway
            }

            File.Delete(Settings.Settings.LocalDbPath);
            ctx = AaruContext.Create(Settings.Settings.LocalDbPath);
            await ctx.Database.EnsureCreatedAsync();

            await ctx.Database
                     .ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT PRIMARY KEY, \"ProductVersion\" TEXT)");

            foreach(string migration in await ctx.Database.GetPendingMigrationsAsync())
            {
#pragma warning disable EF1002
                await ctx.Database
                         .ExecuteSqlRawAsync($"INSERT INTO \"__EFMigrationsHistory\" (MigrationId, ProductVersion) VALUES ('{
                             migration}', '0.0.0')");
#pragma warning restore EF1002
            }

            await ctx.SaveChangesAsync();
        }

        // Remove duplicates
        foreach(var duplicate in ctx.SeenDevices.AsEnumerable()
                                    .GroupBy(a => new
                                     {
                                         a.Manufacturer,
                                         a.Model,
                                         a.Revision,
                                         a.Bus
                                     })
                                    .Where(a => a.Count() > 1)
                                    .Distinct()
                                    .Select(a => a.Key))
        {
            ctx.RemoveRange(ctx.SeenDevices
                               .Where(d => d.Manufacturer == duplicate.Manufacturer &&
                                           d.Model        == duplicate.Model        &&
                                           d.Revision     == duplicate.Revision     &&
                                           d.Bus          == duplicate.Bus)
                               .Skip(1));
        }

        // Remove nulls
        ctx.RemoveRange(ctx.SeenDevices.Where(d => d.Manufacturer == null && d.Model == null && d.Revision == null));

        await ctx.SaveChangesAsync();

        var mainDbUpdate = false;

        if(!File.Exists(Settings.Settings.MainDbPath))
        {
            mainDbUpdate = true;
            await UpdateCommand.DoUpdateAsync(true);
        }

        var mainContext = AaruContext.Create(Settings.Settings.MainDbPath, false);

        if((await mainContext.Database.GetPendingMigrationsAsync()).Any())
        {
            AaruConsole.WriteLine(UI.New_database_version_updating);

            try
            {
                File.Delete(Settings.Settings.MainDbPath);
            }
            catch(Exception)
            {
                AaruConsole.ErrorWriteLine(UI.Exception_trying_to_remove_old_database_version);
                AaruConsole.ErrorWriteLine(UI.Please_manually_remove_file_at_0, Settings.Settings.MainDbPath);

                return (int)ErrorNumber.CannotRemoveDatabase;
            }

            await mainContext.Database.CloseConnectionAsync();
            await mainContext.DisposeAsync();
            await UpdateCommand.DoUpdateAsync(true);
        }

        // GDPR level compliance does not match and there are no arguments or the arguments are neither GUI neither configure.
        if(Settings.Settings.Current.GdprCompliance < DicSettings.GDPR_LEVEL &&
           (args.Length < 1 ||
            args.Length >= 1                                                          &&
            !args[0].Equals("gui",       StringComparison.InvariantCultureIgnoreCase) &&
            !args[0].Equals("configure", StringComparison.InvariantCultureIgnoreCase)))
            new ConfigureCommand().DoConfigure(true);

        Statistics.LoadStats();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // There are too many places that depend on this being inited to be sure all are covered, so init it here.
        PluginBase.Init();

        var rootCommand = new RootCommand();

        rootCommand.AddGlobalOption(new Option<bool>(["--verbose", "-v"], () => false, UI.Shows_verbose_output));

        rootCommand.AddGlobalOption(new Option<bool>(["--debug", "-d"],
                                                     () => false,
                                                     UI.Shows_debug_output_from_plugins));

        Option<bool> pauseOption = new(["--pause"], () => false, UI.Pauses_before_exiting);

        rootCommand.AddGlobalOption(pauseOption);

        rootCommand.Description = $"{_assemblyTitle} {_assemblyVersion?.InformationalVersion}\n{_assemblyCopyright}";

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

        int ret = await rootCommand.InvokeAsync(args);

        await Statistics.SaveStats();

        if(!rootCommand.Parse(args).RootCommandResult.GetValueForOption(pauseOption)) return ret;

        AaruConsole.WriteLine(UI.Press_any_key_to_exit);
        System.Console.ReadKey();

        return ret;
    }

    internal static void PrintCopyright()
    {
        AaruConsole.WriteLine("[bold][red]{0}[/] [green]{1}[/][/]",
                              _assemblyTitle,
                              _assemblyVersion?.InformationalVersion);

        AaruConsole.WriteLine("[bold][blue]{0}[/][/]", _assemblyCopyright);
        AaruConsole.WriteLine();
    }
}