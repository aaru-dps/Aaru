// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Configure.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'configure' verb.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Settings;
using Mono.Options;

namespace DiscImageChef.Commands
{
    class ConfigureCommand : Command
    {
        readonly bool gdprChange;
        bool          autoCall;
        bool          showHelp;

        public ConfigureCommand(bool gdprChange, bool autoCall) : base("configure",
                                                                       "Configures user settings and statistics.")
        {
            this.gdprChange = gdprChange;
            this.autoCall   = autoCall;
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name}",
                "",
                Help,
                {"help|h|?", "Show this message and exit.", v => showHelp = v != null}
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            if(!autoCall)
            {
                List<string> extra = Options.Parse(arguments);

                if(showHelp)
                {
                    Options.WriteOptionDescriptions(CommandSet.Out);
                    return (int)ErrorNumber.HelpRequested;
                }

                MainClass.PrintCopyright();
                if(MainClass.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                if(MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

                if(extra.Count != 0)
                {
                    DicConsole.ErrorWriteLine("Too many arguments.");
                    return (int)ErrorNumber.UnexpectedArgumentCount;
                }
            }

            if(gdprChange)
            {
                DicConsole.WriteLine("In compliance with the European Union General Data Protection Regulation 2016/679 (GDPR),\n"    +
                                     "we must give you the following information about DiscImageChef and ask if you want to opt-in\n" +
                                     "in some information sharing.");
                DicConsole.WriteLine();
                DicConsole.WriteLine("Disclaimer: Because DiscImageChef is an open source software this information, and therefore,\n" +
                                     "compliance with GDPR only holds true if you obtained a certificated copy from its original\n"    +
                                     "authors. In case of doubt, close DiscImageChef now and ask in our IRC support channel.");
                DicConsole.WriteLine();
                DicConsole.WriteLine("For any information sharing your IP address may be stored in our server, in a way that is not\n" +
                                     "possible for any person, manual, or automated process, to link with your identity, unless\n"     +
                                     "specified otherwise.");
            }

            ConsoleKeyInfo pressedKey = new ConsoleKeyInfo();

            #region Device reports
            DicConsole.WriteLine();
            DicConsole.WriteLine(
                                 "With the 'device-report' command, DiscImageChef creates a report of a device, that includes its\n"       +
                                 "manufacturer, model, firmware revision and/or version, attached bus, size, and supported commands.\n"    +
                                 "The serial number of the device is not stored in the report. If used with the debug parameter,\n"        +
                                 "extra information about the device will be stored in the report. This information is known to contain\n" +
                                 "the device serial number in non-standard places that prevent the automatic removal of it on a handful\n" +
                                 "of devices. A human-readable copy of the report in XML format is always created in the same directory\n" +
                                 "where DiscImageChef is being run from.");

            while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
            {
                DicConsole.Write("Do you want to save device reports in shared folder of your computer? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                DicConsole.WriteLine();
            }

            Settings.Settings.Current.SaveReportsGlobally = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();
            DicConsole.WriteLine();
            DicConsole.WriteLine("Sharing a report with us will send it to our server, that's in the european union territory, where it\n"      +
                                 "will be manually analized by an european union citizen to remove any trace of personal identification\n"      +
                                 "from it. Once that is done, it will be shared in our stats website, https://www.discimagechef.app\n"       +
                                 "These report will be used to improve DiscImageChef support, and in some cases, to provide emulation of the\n" +
                                 "devices to other open-source projects. In any case, no information linking the report to you will be stored.");
            while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
            {
                DicConsole.Write("Do you want to share your device reports with us? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                DicConsole.WriteLine();
            }

            Settings.Settings.Current.ShareReports = pressedKey.Key == ConsoleKey.Y;
            #endregion Device reports

            #region Statistics
            DicConsole.WriteLine();
            DicConsole.WriteLine("DiscImageChef can store some usage statistics. These statistics are limited to the number of times a\n"        +
                                 "command is executed, a filesystem, partition, or device is used, the operating system version, and other.\n"   +
                                 "In no case, any information besides pure statistical usage numbers is stored, and they're just joint to the\n" +
                                 "pool with no way of using them to identify you.");

            pressedKey = new ConsoleKeyInfo();
            while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
            {
                DicConsole.Write("Do you want to save stats about your DiscImageChef usage? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                DicConsole.WriteLine();
            }

            if(pressedKey.Key == ConsoleKey.Y)
            {
                Settings.Settings.Current.Stats = new StatsSettings();

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to share your stats anonymously? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.ShareStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about benchmarks? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.BenchmarkStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about command usage? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.CommandStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found devices? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.DeviceStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found filesystems? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.FilesystemStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found file filters? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.FilterStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found media image formats? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.MediaImageStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about scanned media? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.MediaScanStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found partitioning schemes? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.PartitionStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about media types? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.MediaStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about media image verifications? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.VerifyStats = pressedKey.Key == ConsoleKey.Y;
            }
            else Settings.Settings.Current.Stats = null;
            #endregion Statistics

            Settings.Settings.Current.GdprCompliance = DicSettings.GdprLevel;
            Settings.Settings.SaveSettings();
            return (int)ErrorNumber.NoError;
        }
    }
}