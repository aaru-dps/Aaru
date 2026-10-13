using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Aaru.Commands;
using Aaru.CommonTypes.Interop;
using Aaru.Localization;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Spectre;
using Spectre.Console;
using Spectre.Console.Cli;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;

public class LoggingInterceptor : ICommandInterceptor
{
    private readonly LoggingLevelSwitch _levelSwitch = new();

#region ICommandInterceptor Members

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if(settings is not BaseSettings global) return;

        // Set log level
        _levelSwitch.MinimumLevel = global.Debug || global.Verbose
                                        ? LogEventLevel.Verbose
                                        : LogEventLevel.Information;

        // Serilog orders Verbose below Debug, so a Verbose minimum also passes Debug events;
        // filter them out unless --debug was given
        bool debugEnabled = global.Debug;

        // Configure Serilog
        LoggerConfiguration loggerConfig = new LoggerConfiguration().MinimumLevel.ControlledBy(_levelSwitch)
                                                                    .Filter.ByExcluding(e =>
                                                                         e.Level == LogEventLevel.Debug &&
                                                                         !debugEnabled)
                                                                    .Enrich.FromLogContext()
                                                                    .WriteTo.Logger(static lc => lc
                                                                        .Filter.ByExcluding(static e =>
                                                                             e.Level == LogEventLevel.Information)
                                                                        .WriteTo.Spectre(renderTextAsMarkup: true)
                                                                        .WriteTo.Sentry(static o =>
                                                                         {
                                                                             o.Dsn =
                                                                                 "https://153a04fb97b78bb57a8013b8b30db04f@sentry.claunia.com/8";

                                                                             // What to record as Sentry Breadcrumbs
                                                                             o.MinimumBreadcrumbLevel =
                                                                                 LogEventLevel.Debug;

                                                                             // What to send to Sentry as Events
                                                                             o.MinimumEventLevel = LogEventLevel.Error;

                                                                             // If you already call SentrySdk.Init elsewhere:
                                                                             // o.InitializeSdk = false;
                                                                         }));

        // If logfile is present, add file sink and redirect Spectre.Console output
        if(!string.IsNullOrWhiteSpace(global.LogFile))
        {
            loggerConfig = loggerConfig.Enrich.FromLogContext()
                                       .WriteTo.Logger(lc => lc.Enrich.With<StripMarkupEnricher>()
                                                               .WriteTo.File(global.LogFile,
                                                                             outputTemplate:
                                                                             "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {CleanMessage:lj}{NewLine}{Exception:lj}"));
        }

        Log.Logger = loggerConfig.CreateLogger();

        if(global.LogFile == null) return;

        // Serilog swallows failures in sinks and enrichers, report them so a line missing from the log is noticed
        SelfLog.Enable(Console.Error);

        Log.Information(Core.Start_logging_at_0, DateTime.Now);

        PlatformID platId  = DetectOS.GetRealPlatformID();
        string     platVer = DetectOS.GetVersion();

        var assemblyVersion =
            Attribute.GetCustomAttribute(typeof(LoggingInterceptor).Assembly,
                                         typeof(AssemblyInformationalVersionAttribute)) as
                AssemblyInformationalVersionAttribute;

        Log.Information(Core.Program_information);
        Log.Information("Aaru Data Preservation Suite {InformationalVersion}", assemblyVersion?.InformationalVersion);
        Log.Information(Core.Running_in_0_architecture,                        RuntimeInformation.ProcessArchitecture);
        Log.Information(Core.Running_in_0_bit,                                 Environment.Is64BitProcess ? 64 : 32);

        Log.Information(DetectOS.IsAdmin ? Core.Running_as_superuser_Yes : Core.Running_as_superuser_No);
#if DEBUG
        Log.Information(Core.DEBUG_version);
#endif
        Log.Information(Core.Log_section_separator);
        Log.Information("");

        Log.Information(Core.System_information);

        Log.Information("{PlatformName} {PlatformVersion} ({ProcessorArchitecture} {Bittage}-bit)",
                        DetectOS.GetPlatformName(platId, platVer),
                        platVer,
                        RuntimeInformation.OSArchitecture,
                        Environment.Is64BitOperatingSystem ? 64 : 32);

        Log.Information(RuntimeInformation.FrameworkDescription);
        Log.Information(Core.Log_section_separator);

        Log.Information("");
    }

#endregion

#region Nested type: StripMarkupEnricher

    public class StripMarkupEnricher : ILogEventEnricher
    {
#region ILogEventEnricher Members

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // Render the full message (with all tokens applied)
            string rendered = logEvent.RenderMessage();

            // Remove the Spectre markup, keeping the text verbatim if it is not valid markup. Throwing here would make
            // Serilog drop the event from the log file while it is still shown on the console.
            string cleaned;

            try
            {
                cleaned = Markup.Remove(rendered);
            }
            catch(InvalidOperationException)
            {
                cleaned = rendered;
            }

            // Attach a new property CleanMessage
            LogEventProperty prop = propertyFactory.CreateProperty("CleanMessage", cleaned);
            logEvent.AddOrUpdateProperty(prop);
        }

#endregion
    }

#endregion
}