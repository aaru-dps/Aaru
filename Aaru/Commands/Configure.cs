// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Configure.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'configure' command.
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

using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Settings;
using Spectre.Console;

namespace Aaru.Commands;

sealed class ConfigureCommand : Command
{
    public ConfigureCommand() : base("configure", "Configures user settings and statistics.") =>
        Handler = CommandHandler.Create((Func<bool, bool, int>)Invoke);

    int Invoke(bool debug, bool verbose)
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

        return DoConfigure(false);
    }

    internal int DoConfigure(bool gdprChange)
    {
        if(gdprChange)
        {
            AaruConsole.
                WriteLine("In compliance with the [bold]European Union General Data Protection Regulation 2016/679 ([italic]GDPR[/])[/],\n" +
                          "we must give you the following information about [italic]Aaru[/] and ask if you want to opt-in\n" +
                          "in some information sharing.");

            AaruConsole.WriteLine();

            AaruConsole.
                WriteLine("Disclaimer: Because [italic]Aaru[/] is an open source software this information, and therefore,\n" +
                          "compliance with [bold]GDPR[/] only holds true if you obtained a certificated copy from its original\n" +
                          "authors. In case of doubt, close [italic]Aaru[/] now and ask in our IRC support channel.");

            AaruConsole.WriteLine();

            AaruConsole.
                WriteLine("For any information sharing your IP address may be stored in our server, in a way that is not\n" +
                          "possible for any person, manual, or automated process, to link with your identity, unless\n" +
                          "specified otherwise.");
        }

        var pressedKey = new ConsoleKeyInfo();

        AaruConsole.WriteLine();

        AaruConsole.
            WriteLine("Do you want to enable the decryption of copy protected media (also known as [italic]DRM[/]),\n" +
                      "like for example [italic]DVD Video CSS[/] encryption.\n"                                        +
                      "[bold]Consult your local laws before enabling it, as this is illegal in some countries, or\n"   +
                      "only legal under some circumstances[/].");

        while(pressedKey.Key != ConsoleKey.Y &&
              pressedKey.Key != ConsoleKey.N)
        {
            AaruConsole.
                Write("[italic]Do you want to enable decryption of copy protected media?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

            pressedKey = System.Console.ReadKey();
            AaruConsole.WriteLine();
        }

        Settings.Settings.Current.EnableDecryption = pressedKey.Key == ConsoleKey.Y;

        #region Device reports
        AaruConsole.WriteLine();

        AaruConsole.
            WriteLine(
                      "With the 'device-report' command, [italic]Aaru[/] creates a report of a device, that includes its\n" +
                      "manufacturer, model, firmware revision and/or version, attached bus, size, and supported commands.\n" +
                      "The serial number of the device is not stored in the report. If used with the debug parameter,\n" +
                      "extra information about the device will be stored in the report. This information is known to contain\n" +
                      "the device serial number in non-standard places that prevent the automatic removal of it on a handful\n" +
                      "of devices. A human-readable copy of the report in XML format is always created in the same directory\n" +
                      "where [italic]Aaru[/] is being run from.");

        while(pressedKey.Key != ConsoleKey.Y &&
              pressedKey.Key != ConsoleKey.N)
        {
            AaruConsole.
                Write("[italic]Do you want to save device reports in shared folder of your computer? [bold]([green]Y[/]/[red]N[/]):[/] ");

            pressedKey = System.Console.ReadKey();
            AaruConsole.WriteLine();
        }

        Settings.Settings.Current.SaveReportsGlobally = pressedKey.Key == ConsoleKey.Y;

        pressedKey = new ConsoleKeyInfo();
        AaruConsole.WriteLine();

        AaruConsole.
            WriteLine("Sharing a report with us will send it to our server, that's in the european union territory, where it\n" +
                      "will be manually analyzed by an european union citizen to remove any trace of personal identification\n" +
                      "from it. Once that is done, it will be shared in our stats website, [italic][blue]https://www.aaru.app[/][/]\n" +
                      "These report will be used to improve [italic]Aaru[/] support, and in some cases, to provide emulation of the\n" +
                      "devices to other open-source projects. In any case, no information linking the report to you will be stored.");

        while(pressedKey.Key != ConsoleKey.Y &&
              pressedKey.Key != ConsoleKey.N)
        {
            AaruConsole.
                Write("[italic]Do you want to share your device reports with us?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

            pressedKey = System.Console.ReadKey();
            AaruConsole.WriteLine();
        }

        Settings.Settings.Current.ShareReports = pressedKey.Key == ConsoleKey.Y;
        #endregion Device reports

        #region Statistics
        AaruConsole.WriteLine();

        AaruConsole.
            WriteLine("[italic]Aaru[/] can store some usage statistics. These statistics are limited to the number of times a\n" +
                      "command is executed, a filesystem, partition, or device is used, the operating system version, and other.\n" +
                      "In no case, any information besides pure statistical usage numbers is stored, and they're just joint to the\n" +
                      "pool with no way of using them to identify you.");

        pressedKey = new ConsoleKeyInfo();

        while(pressedKey.Key != ConsoleKey.Y &&
              pressedKey.Key != ConsoleKey.N)
        {
            AaruConsole.
                Write("[italic]Do you want to save stats about your Aaru usage?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

            pressedKey = System.Console.ReadKey();
            AaruConsole.WriteLine();
        }

        if(pressedKey.Key == ConsoleKey.Y)
        {
            Settings.Settings.Current.Stats = new StatsSettings();

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to share your stats (anonymously)?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.ShareStats = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to gather statistics about command usage?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.CommandStats = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to gather statistics about found devices?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.DeviceStats = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to gather statistics about found filesystems?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.FilesystemStats = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to gather statistics about found file filters?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.FilterStats = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to gather statistics about found media image formats?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.MediaImageStats = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to gather statistics about scanned media?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.MediaScanStats = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to gather statistics about found partitioning schemes?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.PartitionStats = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to gather statistics about media types?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.MediaStats = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.
                    Write("[italic]Do you want to gather statistics about media image verifications?[/] [bold]([green]Y[/]/[red]N[/]):[/] ");

                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.Stats.VerifyStats = pressedKey.Key == ConsoleKey.Y;
        }
        else
            Settings.Settings.Current.Stats = null;
        #endregion Statistics

        Settings.Settings.Current.GdprCompliance = DicSettings.GDPR_LEVEL;
        Settings.Settings.SaveSettings();

        return (int)ErrorNumber.NoError;
    }
}