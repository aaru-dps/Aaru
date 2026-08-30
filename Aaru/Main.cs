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
// Copyright © 2011-2026 Natalia Portillo
// Copyright © 2020-2026 Rebecca Wallander
// ****************************************************************************/

using System;
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
using Aaru.Core;
using Aaru.Database;
using Aaru.Localization;
using Aaru.Logging;
using Aaru.Settings;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Sentry;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using ListOptionsCommand = Aaru.Commands.Filesystem.ListOptionsCommand;

namespace Aaru;

class MainClass
{
    static        string                                _assemblyCopyright;
    static        string                                _assemblyTitle;
    static        AssemblyInformationalVersionAttribute _assemblyVersion;
    public static bool                                  PauseBeforeExiting { get; set; }

    public static async Task<int> Main([NotNull] string[] args)
    {
        object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        _assemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
        attributes     = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

        _assemblyVersion =
            Attribute.GetCustomAttribute(typeof(MainClass).Assembly, typeof(AssemblyInformationalVersionAttribute)) as
                AssemblyInformationalVersionAttribute;

        _assemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

        if(args.Length == 1 && args[0].Equals("gui", StringComparison.InvariantCultureIgnoreCase))
            return Gui.Main.Start(args);

        try
        {
            AaruLogging.WriteLineEvent += static (format, objects) =>
            {
                string formatted = SafeFormat(format, objects);

                SafeMarkup(formatted);
                AnsiConsole.WriteLine();

                if(formatted is null) return;

                Log.Information(SafeRemoveMarkup(formatted));
            };

            AaruLogging.WriteEvent += static (format, objects) =>
            {
                string formatted = SafeFormat(format, objects);

                if(formatted is null) return;

                SafeMarkup(formatted);

                Log.Information(SafeRemoveMarkup(formatted));
            };

            AaruLogging.ErrorEvent   += Log.Error;
            AaruLogging.VerboseEvent += Log.Verbose;

            AaruLogging.DebugEvent += static (module, format, objects) =>
                Log.Debug($"[blue]({module})[/] {SafeFormat(format, objects)}");

            AaruLogging.WriteExceptionEvent += static (ex, message, objects) =>
            {
                // Display exception on console
                AnsiConsole.WriteException(ex);

                // Also display the message if provided
                if(!string.IsNullOrEmpty(message))
                {
                    SafeMarkup(SafeFormat(message, objects));
                    AnsiConsole.WriteLine();
                }

                // Log to file with full exception details
                if(string.IsNullOrEmpty(message))
                    Log.Error(ex, "Exception occurred");
                else if(objects == null || objects.Length == 0)
                    Log.Error(ex, message);
                else
                    Log.Error(ex, message, objects);
            };

            AaruLogging.InformationEvent += Log.Information;

            Settings.Settings.LoadSettings();

            // Ask an existing user for crash-report consent once, so an upgrade is not silently opted out. A new
            // user (GDPR level below the current one) is asked by the configuration wizard further down, and an
            // explicit `configure` run asks on its own, so skip both to avoid asking twice. Only prompt when stdin
            // is a terminal: Spectre's Confirm throws "Failed to read input in non-interactive mode" otherwise,
            // which would abort every scripted, piped or service run, and any tool driving Aaru. When we cannot
            // ask, leave the flag unset so a later interactive run asks, and keep reporting off until then.
            bool consentAskedElsewhere =
                Settings.Settings.Current.GdprCompliance < DicSettings.GDPR_LEVEL ||
                args.Length >= 1 && args[0].Equals("configure", StringComparison.InvariantCultureIgnoreCase);

            if(!Settings.Settings.Current.HasConsentBeenAsked &&
               !consentAskedElsewhere                        &&
               !Console.IsInputRedirected)
            {
                new ConfigureCommand().AskCrashReportConsent();
                Settings.Settings.SaveSettings();
            }

            // Crash reporting is opt-in, so it cannot start before the stored settings have been read. Anything
            // that fails earlier than this goes unreported, because at that point we do not yet know if we may.
            if(Settings.Settings.Current.ShareCrashReports)
            {
                SentrySdk.Init(static options =>
                {
                    // A Sentry Data Source Name (DSN) is required.
                    // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
                    // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
                    if(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SENTRY_DSN")))
                        options.Dsn = "https://153a04fb97b78bb57a8013b8b30db04f@sentry.claunia.com/8";

                    // This option is recommended. It enables Sentry's "Release Health" feature.
                    options.AutoSessionTracking = true;

                    // Set TracesSampleRate to 1.0 to capture 100%
                    // of transactions for tracing.
                    // We recommend adjusting this value in production.
                    options.TracesSampleRate = 1.0;

                    options.IsGlobalModeEnabled = true;
                });

                SentrySdk.ConfigureScope(static scope => scope.SetExtra("Args", Environment.GetCommandLineArgs()));
            }

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
                catch(Exception ex)
                {
                    // Should not ever arrive here, but if it does, keep trying to replace it anyway
                    SentrySdk.CaptureException(ex);
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
                                        .GroupBy(static a => new
                                         {
                                             a.Manufacturer,
                                             a.Model,
                                             a.Revision,
                                             a.Bus
                                         })
                                        .Where(static a => a.Count() > 1)
                                        .Distinct()
                                        .Select(static a => a.Key))
            {
                ctx.RemoveRange(ctx.SeenDevices
                                   .Where(d => d.Manufacturer == duplicate.Manufacturer &&
                                               d.Model        == duplicate.Model        &&
                                               d.Revision     == duplicate.Revision     &&
                                               d.Bus          == duplicate.Bus)
                                   .Skip(1));
            }

            // Remove nulls
            ctx.RemoveRange(ctx.SeenDevices.Where(static d => d.Manufacturer == null &&
                                                              d.Model        == null &&
                                                              d.Revision     == null));

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
                AaruLogging.WriteLine(UI.New_database_version_updating);

                try
                {
                    File.Delete(Settings.Settings.MainDbPath);
                }
                catch(Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    AaruLogging.Error(UI.Exception_trying_to_remove_old_database_version);
                    AaruLogging.Error(UI.Please_manually_remove_file_at_0, Settings.Settings.MainDbPath);

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

            var app = new CommandApp();

            app.Configure(static config =>
            {
                config.PropagateExceptions();

                config.UseAssemblyInformationalVersion();

                config.AddBranch<ArchiveFamily>("archive",
                                                static archive =>
                                                {
                                                    archive.SetDescription(UI.Archive_Command_Family_Description);

                                                    archive.AddCommand<ArchiveListCommand>("list")
                                                           .WithAlias("l")
                                                           .WithAlias("ls")
                                                           .WithDescription(UI.Archive_List_Command_Description);

                                                    archive.AddCommand<ArchiveExtractCommand>("extract")
                                                           .WithAlias("x")
                                                           .WithDescription(UI.Archive_Extract_Command_Description);

                                                    archive.AddCommand<ArchiveInfoCommand>("info")
                                                           .WithAlias("i")
                                                           .WithDescription(UI.Archive_Info_Command_Description);
                                                })
                      .WithAlias("arc");

                config.AddBranch<DeviceFamily>("device",
                                               static device =>
                                               {
                                                   device.SetDescription(UI.Device_Command_Family_Description);

                                                   device.AddCommand<DeviceReportCommand>("report")
                                                         .WithDescription(UI.Device_Report_Command_Description);

                                                   device.AddCommand<DeviceInfoCommand>("info")
                                                         .WithAlias("i")
                                                         .WithDescription(UI.Device_Info_Command_Description);

                                                   device.AddCommand<ListDevicesCommand>("list")
                                                         .WithAlias("l")
                                                         .WithAlias("ls")
                                                         .WithDescription(UI.Device_List_Command_Description);
                                               })
                      .WithAlias("dev");

                config.AddBranch<FilesystemFamily>("filesystem",
                                                   static fs =>
                                                   {
                                                       fs.SetDescription(UI.Filesystem_Command_Family_Description);

                                                       fs.AddCommand<ExtractFilesCommand>("extract")
                                                         .WithAlias("x")
                                                         .WithDescription(UI.Filesystem_Extract_Command_Description);

                                                       fs.AddCommand<FilesystemInfoCommand>("info")
                                                         .WithAlias("i")
                                                         .WithDescription(UI.Filesystem_Info_Command_Description);

                                                       fs.AddCommand<LsCommand>("list")
                                                         .WithAlias("ls")
                                                         .WithDescription(UI.Filesystem_List_Command_Description);

                                                       fs.AddCommand<ListOptionsCommand>("options")
                                                         .WithDescription(UI.Filesystem_Options_Command_Description);
                                                   })
                      .WithAlias("fs")
                      .WithAlias("fi");

                config.AddBranch<ImageFamily>("image",
                                              static image =>
                                              {
                                                  image.SetDescription(UI.Image_Command_Family_Description);

                                                  image.AddCommand<AnalyzeCommand>("analyze")
                                                       .WithDescription(UI.Image_Analyze_Command_Description);

                                                  image.AddCommand<ChecksumCommand>("checksum")
                                                       .WithAlias("chk")
                                                       .WithDescription(UI.Image_Checksum_Command_Description);

                                                  image.AddCommand<CompareCommand>("compare")
                                                       .WithAlias("cmp")
                                                       .WithDescription(UI.Image_Compare_Command_Description);

                                                  image.AddCommand<ConvertImageCommand>("convert")
                                                       .WithAlias("cvt")
                                                       .WithDescription(UI.Image_Convert_Command_Description);

                                                  image.AddCommand<CreateSidecarCommand>("create-sidecar")
                                                       .WithAlias("cs")
                                                       .WithDescription(UI.Image_Create_Sidecar_Command_Description);

                                                  image.AddCommand<DecodeCommand>("decode")
                                                       .WithDescription(UI.Image_Decode_Command_Description);

                                                  image.AddCommand<EntropyCommand>("entropy")
                                                       .WithDescription(UI.Image_Entropy_Command_Description);

                                                  image.AddCommand<ImageInfoCommand>("info")
                                                       .WithAlias("i")
                                                       .WithDescription(UI.Image_Info_Command_Description);

                                                  image.AddCommand<Commands.Image.ListOptionsCommand>("options")
                                                       .WithDescription(UI.Image_Options_Command_Description);

                                                  image.AddCommand<PrintHexCommand>("print-hex")
                                                       .WithAlias("ph")
                                                       .WithDescription(UI.Image_Print_Command_Description);

                                                  image.AddCommand<VerifyCommand>("verify")
                                                       .WithAlias("v")
                                                       .WithDescription(UI.Image_Verify_Command_Description);

                                                  image.AddCommand<MergeCommand>("merge")
                                                       .WithDescription(UI.Image_Merge_Command_Description);

                                                  image.AddCommand<WriteMetadataCommand>("write-metadata")
                                                       .WithDescription(UI.Image_Write_Metadata_Command_Description);
                                              })
                      .WithAlias("i")
                      .WithAlias("img");

                config.AddBranch<MediaFamily>("media",
                                              static media =>
                                              {
                                                  media.SetDescription(UI.Media_Command_Family_Description);

                                                  media.AddCommand<MediaInfoCommand>("info")
                                                       .WithAlias("i")
                                                       .WithDescription(UI.Media_Info_Command_Description);

                                                  media.AddCommand<MediaScanCommand>("scan")
                                                       .WithAlias("s")
                                                       .WithDescription(UI.Media_Scan_Command_Description);

                                                  media.AddCommand<DumpMediaCommand>("dump")
                                                       .WithAlias("d")
                                                       .WithDescription(UI.Media_Dump_Command_Description);
                                              })
                      .WithAlias("m");

                config.AddBranch<DatabaseFamily>("database",
                                                 static db =>
                                                 {
                                                     db.SetDescription(UI.Database_Command_Family_Description);

                                                     db.AddCommand<StatisticsCommand>("stats")
                                                       .WithDescription(UI.Database_Stats_Command_Description);

                                                     db.AddCommand<UpdateCommand>("update")
                                                       .WithDescription(UI.Database_Update_Command_Description);
                                                 })
                      .WithAlias("db");

                config.AddCommand<ConfigureCommand>("configure")
                      .WithAlias("cfg")
                      .WithDescription(UI.Configure_Command_Description);

                config.AddCommand<FormatsCommand>("formats")
                      .WithAlias("fmt")
                      .WithDescription(UI.List_Formats_Command_Description);

                config.AddCommand<ListEncodingsCommand>("list-encodings")
                      .WithAlias("le")
                      .WithDescription(UI.List_Encodings_Command_Description);

                config.AddCommand<ListNamespacesCommand>("list-namespaces")
                      .WithAlias("ln")
                      .WithDescription(UI.List_Namespaces_Command_Description);

                config.AddCommand<RemoteCommand>("remote")
                      .WithAlias("rem")
                      .WithDescription(UI.Remote_Command_Description);

                config.AddCommand<MetadataSchemaCommand>("metadata-schema")
                      .WithDescription(UI.Generates_the_JSON_schema_for_Aaru_metadata_files);

                config.SetInterceptor(new LoggingInterceptor());
                config.SetInterceptor(new PausingInterceptor());
            });

            int ret = await app.RunAsync(args);

            await Statistics.SaveStatsAsync();

            if(!PauseBeforeExiting) return ret;

            AaruLogging.WriteLine(UI.Press_any_key_to_exit);
            Console.ReadKey();

            return ret;
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);
            AnsiConsole.WriteException(ex);

            return (int)ErrorNumber.UnexpectedException;
        }
    }

    internal static void PrintCopyright()
    {
        AnsiConsole.MarkupLine("[bold][red]{0}[/] [green]{1}[/][/]",
                               _assemblyTitle,
                               _assemblyVersion?.InformationalVersion);

        AnsiConsole.MarkupLine("[bold][blue]{0}[/][/]", _assemblyCopyright);
        AnsiConsole.MarkupLine("[bold][orange3]If you like this software, please contribute at [/][blue]https://patreon.com/claunia[/][/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    ///     Applies the format arguments to a composite format string, returning it unchanged if it is not a valid
    ///     composite format string. Filesystem and image plugins print data read from the media, and that data can contain
    ///     anything, including braces.
    /// </summary>
    /// <param name="format">A composite format string</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" /></param>
    /// <returns>Formatted string</returns>
    static string SafeFormat(string format, object[] arg)
    {
        if(format is null || arg is null || arg.Length == 0) return format;

        try
        {
            return string.Format(format, arg);
        }
        catch(FormatException)
        {
            return format;
        }
    }

    /// <summary>
    ///     Writes a string to the console interpreting the markup in it, falling back to printing it verbatim if the
    ///     markup is not valid. Filesystem and image plugins print data read from the media, and that data can contain
    ///     anything, including square brackets.
    /// </summary>
    /// <param name="text">String to write</param>
    static void SafeMarkup(string text)
    {
        if(string.IsNullOrEmpty(text)) return;

        try
        {
            AnsiConsole.Markup(text);
        }
        catch(InvalidOperationException)
        {
            AnsiConsole.Write(text);
        }
    }

    /// <summary>Removes the markup from a string, returning it unchanged if the markup is not valid</summary>
    /// <param name="text">String to remove the markup from</param>
    /// <returns>String without markup</returns>
    static string SafeRemoveMarkup(string text)
    {
        try
        {
            return Markup.Remove(text);
        }
        catch(InvalidOperationException)
        {
            return text;
        }
    }
}
