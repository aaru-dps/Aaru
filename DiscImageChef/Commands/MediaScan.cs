// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MediaScan.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'media-scan' verb.
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

using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Scanning;
using DiscImageChef.Devices;
using Mono.Options;

namespace DiscImageChef.Commands
{
    internal class MediaScanCommand : Command
    {
        private string devicePath;
        private string ibgLogPath;
        private string mhddLogPath;
        private bool showHelp;

        public MediaScanCommand() : base("media-scan", "Scans the media inserted on a device.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS] devicepath",
                "",
                Help,
                {"mhdd-log|mw=", "Write a log of the scan in the format used by MHDD.", s => mhddLogPath = s},
                {"ibg-log|b=", "Write a log of the scan in the format used by ImgBurn.", s => ibgLogPath = s},
                {"help|h|?", "Show this message and exit.", v => showHelp = v != null}
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            var extra = Options.Parse(arguments);

            if (showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);
                return (int) ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();
            if (MainClass.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
            if (MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
            Statistics.AddCommand("media-scan");

            if (extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int) ErrorNumber.UnexpectedArgumentCount;
            }

            if (extra.Count == 0)
            {
                DicConsole.ErrorWriteLine("Missing device path.");
                return (int) ErrorNumber.MissingArgument;
            }

            devicePath = extra[0];

            DicConsole.DebugWriteLine("Media-Scan command", "--debug={0}", MainClass.Debug);
            DicConsole.DebugWriteLine("Media-Scan command", "--device={0}", devicePath);
            DicConsole.DebugWriteLine("Media-Scan command", "--ibg-log={0}", ibgLogPath);
            DicConsole.DebugWriteLine("Media-Scan command", "--mhdd-log={0}", mhddLogPath);
            DicConsole.DebugWriteLine("Media-Scan command", "--verbose={0}", MainClass.Verbose);

            if (devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Device dev;
            try
            {
                dev = new Device(devicePath);

                if (dev.Error)
                {
                    DicConsole.ErrorWriteLine(Error.Print(dev.LastError));
                    return (int) ErrorNumber.CannotOpenDevice;
                }
            }
            catch (DeviceException e)
            {
                DicConsole.ErrorWriteLine(e.Message ?? Error.Print(e.LastError));
                return (int) ErrorNumber.CannotOpenDevice;
            }

            Statistics.AddDevice(dev);

            var scanner = new MediaScan(mhddLogPath, ibgLogPath, devicePath, dev);
            scanner.UpdateStatus += Progress.UpdateStatus;
            scanner.StoppingErrorMessage += Progress.ErrorMessage;
            scanner.UpdateProgress += Progress.UpdateProgress;
            scanner.PulseProgress += Progress.PulseProgress;
            scanner.InitProgress += Progress.InitProgress;
            scanner.EndProgress += Progress.EndProgress;
            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                scanner.Abort();
            };
            var results = scanner.Scan();

            DicConsole.WriteLine("Took a total of {0} seconds ({1} processing commands).", results.TotalTime,
                results.ProcessingTime);
            DicConsole.WriteLine("Average speed: {0:F3} MiB/sec.", results.AvgSpeed);
            DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", results.MaxSpeed);
            DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", results.MinSpeed);
            DicConsole.WriteLine("Summary:");
            DicConsole.WriteLine("{0} sectors took less than 3 ms.", results.A);
            DicConsole.WriteLine("{0} sectors took less than 10 ms but more than 3 ms.", results.B);
            DicConsole.WriteLine("{0} sectors took less than 50 ms but more than 10 ms.", results.C);
            DicConsole.WriteLine("{0} sectors took less than 150 ms but more than 50 ms.", results.D);
            DicConsole.WriteLine("{0} sectors took less than 500 ms but more than 150 ms.", results.E);
            DicConsole.WriteLine("{0} sectors took more than 500 ms.", results.F);
            DicConsole.WriteLine("{0} sectors could not be read.",
                results.UnreadableSectors.Count);
            if (results.UnreadableSectors.Count > 0)
                foreach (var bad in results.UnreadableSectors)
                    DicConsole.WriteLine("Sector {0} could not be read", bad);

            DicConsole.WriteLine();

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (results.SeekTotal != 0 || results.SeekMin != double.MaxValue || results.SeekMax != double.MinValue)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                DicConsole.WriteLine(
                    "Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                    results.SeekTimes, results.SeekMax, results.SeekMin, results.SeekTotal / 1000);

            dev.Close();
            return (int) ErrorNumber.NoError;
        }
    }
}