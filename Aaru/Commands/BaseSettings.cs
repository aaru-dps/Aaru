using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Aaru.CommonTypes;
using Aaru.Localization;
using Spectre.Console.Cli;

namespace Aaru.Commands;

public class BaseSettings : CommandSettings
{
    [CommandOption("-v|--verbose")]
    [LocalizedDescription(nameof(UI.Shows_verbose_output))]
    [DefaultValue(false)]
    public bool Verbose
    {
        get => field || Debug;
        init;
    }

    [CommandOption("--debug")]
    [LocalizedDescription(nameof(UI.Shows_debug_output_from_plugins))]
    [DefaultValue(false)]
    public bool Debug { get; init; }

    [CommandOption("--logfile <PATH>")]
    [LocalizedDescription(nameof(UI.Path_to_log_file))]
    public string? LogFile { get; set; }

    [CommandOption("--pause")]
    [LocalizedDescription(nameof(UI.Pauses_before_exiting))]
    [DefaultValue(false)]
    public bool Pause { get; init; }

    /// <summary>
    ///     Moves the options declared in this class to just after the first command in the command line, so they work
    ///     in any position. Spectre.Console.Cli only accepts an option inherited from a branch right after the branch
    ///     name, and would otherwise reject <c>aaru media dump … --logfile dump.log</c>.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Command line arguments with the global options moved after the first command</returns>
    public static string[] RelocateGlobalOptions(string[] args)
    {
        Dictionary<string, bool> globalOptions = GetGlobalOptions();
        List<string>             moved         = [];
        List<string>             rest          = [];

        for(var i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            // Everything after the separator belongs to the command verbatim
            if(arg == "--")
            {
                rest.AddRange(args[i..]);

                break;
            }

            int    separator = arg.StartsWith("--", StringComparison.Ordinal) ? arg.IndexOfAny(['=', ':']) : -1;
            string name      = separator > 0 ? arg[..separator] : arg;

            if(!globalOptions.TryGetValue(name, out bool takesValue))
            {
                rest.Add(arg);

                continue;
            }

            moved.Add(arg);

            if(separator > 0 || i + 1 >= args.Length) continue;

            string next = args[i + 1];

            // Take the value along, the same way Spectre.Console.Cli would assign it
            if(takesValue ? !next.StartsWith('-') : bool.TryParse(next, out _))
            {
                moved.Add(next);
                i++;
            }
        }

        if(moved.Count == 0) return args;

        int command = rest.FindIndex(static a => !a.StartsWith('-'));

        if(command < 0) return args;

        rest.InsertRange(command + 1, moved);

        return rest.ToArray();
    }

    /// <summary>Gets the option names declared in this class</summary>
    /// <remarks>
    ///     A command must not declare an option with any of these names for itself, as it would be moved away from it.
    ///     This is why <c>--debug</c> has no short name: <c>image verify</c> uses <c>-d</c> for <c>--dimensions</c>.
    /// </remarks>
    /// <returns>Option names, and whether each one takes a value</returns>
    static Dictionary<string, bool> GetGlobalOptions()
    {
        Dictionary<string, bool> names = [];

        foreach(PropertyInfo property in typeof(BaseSettings).GetProperties(BindingFlags.DeclaredOnly |
                                                                             BindingFlags.Instance     |
                                                                             BindingFlags.Public))
        {
            CommandOptionAttribute option = property.GetCustomAttribute<CommandOptionAttribute>();

            if(option is null) continue;

            bool takesValue = option.ValueName is not null;

            foreach(string longName in option.LongNames) names[$"--{longName}"] = takesValue;

            foreach(string shortName in option.ShortNames) names[$"-{shortName}"] = takesValue;
        }

        return names;
    }
}