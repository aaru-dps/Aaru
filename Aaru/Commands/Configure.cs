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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Settings;

namespace Aaru.Commands
{
    internal sealed class ConfigureCommand : Command
    {
        public ConfigureCommand() : base("configure", "Configures user settings and statistics.") =>
            Handler = CommandHandler.Create((Func<bool, bool, int>)Invoke);

        int Invoke(bool debug, bool verbose)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            return DoConfigure(false);
        }

        internal int DoConfigure(bool gdprChange)
        {
            if(gdprChange)
            {
                AaruConsole.
                    WriteLine("In compliance with the European Union General Data Protection Regulation 2016/679 (GDPR),\n" +
                              "we must give you the following information about Aaru and ask if you want to opt-in\n" +
                              "in some information sharing.");

                AaruConsole.WriteLine();

                AaruConsole.
                    WriteLine("Disclaimer: Because Aaru is an open source software this information, and therefore,\n" +
                              "compliance with GDPR only holds true if you obtained a certificated copy from its original\n" +
                              "authors. In case of doubt, close Aaru now and ask in our IRC support channel.");

                AaruConsole.WriteLine();

                AaruConsole.
                    WriteLine("For any information sharing your IP address may be stored in our server, in a way that is not\n" +
                              "possible for any person, manual, or automated process, to link with your identity, unless\n" +
                              "specified otherwise.");
            }

            var pressedKey = new ConsoleKeyInfo();

            AaruConsole.WriteLine();

            AaruConsole.
                WriteLine("Do you want to enable the decryption of copy protected media (also known as DRM),\n"    +
                          "like for example DVD Video CSS encryption.\n"                                           +
                          "Consult your local laws before enabling it, as this is illegal in some countries, or\n" +
                          "only legal under some circumstances.");

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.Write("Do you want to enable decryption of copy protected media? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.EnableDecryption = pressedKey.Key == ConsoleKey.Y;

            #region Device reports
            AaruConsole.WriteLine();

            AaruConsole.
                WriteLine("With the 'device-report' command, Aaru creates a report of a device, that includes its\n" +
                          "manufacturer, model, firmware revision and/or version, attached bus, size, and supported commands.\n" +
                          "The serial number of the device is not stored in the report. If used with the debug parameter,\n" +
                          "extra information about the device will be stored in the report. This information is known to contain\n" +
                          "the device serial number in non-standard places that prevent the automatic removal of it on a handful\n" +
                          "of devices. A human-readable copy of the report in XML format is always created in the same directory\n" +
                          "where Aaru is being run from.");

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.Write("Do you want to save device reports in shared folder of your computer? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.SaveReportsGlobally = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();
            AaruConsole.WriteLine();

            AaruConsole.
                WriteLine("Sharing a report with us will send it to our server, that's in the european union territory, where it\n" +
                          "will be manually analyzed by an european union citizen to remove any trace of personal identification\n" +
                          "from it. Once that is done, it will be shared in our stats website, https://www.aaru.app\n" +
                          "These report will be used to improve Aaru support, and in some cases, to provide emulation of the\n" +
                          "devices to other open-source projects. In any case, no information linking the report to you will be stored.");

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.Write("Do you want to share your device reports with us? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                AaruConsole.WriteLine();
            }

            Settings.Settings.Current.ShareReports = pressedKey.Key == ConsoleKey.Y;
            #endregion Device reports

            #region Statistics
            AaruConsole.WriteLine();

            AaruConsole.
                WriteLine("Aaru can store some usage statistics. These statistics are limited to the number of times a\n" +
                          "command is executed, a filesystem, partition, or device is used, the operating system version, and other.\n" +
                          "In no case, any information besides pure statistical usage numbers is stored, and they're just joint to the\n" +
                          "pool with no way of using them to identify you.");

            pressedKey = new ConsoleKeyInfo();

            while(pressedKey.Key != ConsoleKey.Y &&
                  pressedKey.Key != ConsoleKey.N)
            {
                AaruConsole.Write("Do you want to save stats about your Aaru usage? (Y/N): ");
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
                    AaruConsole.Write("Do you want to share your stats (anonymously)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    AaruConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.ShareStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    AaruConsole.Write("Do you want to gather statistics about command usage? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    AaruConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.CommandStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    AaruConsole.Write("Do you want to gather statistics about found devices? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    AaruConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.DeviceStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    AaruConsole.Write("Do you want to gather statistics about found filesystems? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    AaruConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.FilesystemStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    AaruConsole.Write("Do you want to gather statistics about found file filters? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    AaruConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.FilterStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    AaruConsole.Write("Do you want to gather statistics about found media image formats? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    AaruConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.MediaImageStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    AaruConsole.Write("Do you want to gather statistics about scanned media? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    AaruConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.MediaScanStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    AaruConsole.Write("Do you want to gather statistics about found partitioning schemes? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    AaruConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.PartitionStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    AaruConsole.Write("Do you want to gather statistics about media types? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    AaruConsole.WriteLine();
                }

                Settings.Settings.Current.Stats.MediaStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();

                while(pressedKey.Key != ConsoleKey.Y &&
                      pressedKey.Key != ConsoleKey.N)
                {
                    AaruConsole.Write("Do you want to gather statistics about media image verifications? (Y/N): ");
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
}