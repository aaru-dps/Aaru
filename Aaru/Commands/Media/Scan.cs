// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Scan.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'scan' command.
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

using System.CommandLine;
using System.CommandLine.Invocation;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Devices.Scanning;
using Aaru.Devices;

namespace Aaru.Commands.Media
{
    internal sealed class MediaScanCommand : Command
    {
        public MediaScanCommand() : base("scan", "Scans the media inserted on a device.")
        {
            Add(new Option(new[]
                {
                    "--mhdd-log", "-m"
                }, "Write a log of the scan in the format used by MHDD.")
                {
                    Argument = new Argument<string>(() => null),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--ibg-log", "-b"
                }, "Write a log of the scan in the format used by ImgBurn.")
                {
                    Argument = new Argument<string>(() => null),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--use-buffered-reads"
                }, "For MMC/SD, use OS buffered reads if CMD23 is not supported.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Device path",
                Name        = "device-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string devicePath, string ibgLog, string mhddLog,
                                 bool useBufferedReads)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("media-scan");

            AaruConsole.DebugWriteLine("Media-Scan command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Media-Scan command", "--device={0}", devicePath);
            AaruConsole.DebugWriteLine("Media-Scan command", "--ibg-log={0}", ibgLog);
            AaruConsole.DebugWriteLine("Media-Scan command", "--mhdd-log={0}", mhddLog);
            AaruConsole.DebugWriteLine("Media-Scan command", "--verbose={0}", verbose);
            AaruConsole.DebugWriteLine("Media-Scan command", "--use-buffered-reads={0}", useBufferedReads);

            if(devicePath.Length == 2   &&
               devicePath[1]     == ':' &&
               devicePath[0]     != '/' &&
               char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Devices.Device dev;

            try
            {
                dev = new Devices.Device(devicePath);

                if(dev.IsRemote)
                    Statistics.AddRemote(dev.RemoteApplication, dev.RemoteVersion, dev.RemoteOperatingSystem,
                                         dev.RemoteOperatingSystemVersion, dev.RemoteArchitecture);

                if(dev.Error)
                {
                    AaruConsole.ErrorWriteLine(Error.Print(dev.LastError));

                    return (int)ErrorNumber.CannotOpenDevice;
                }
            }
            catch(DeviceException e)
            {
                AaruConsole.ErrorWriteLine(e.Message);

                return (int)ErrorNumber.CannotOpenDevice;
            }

            Statistics.AddDevice(dev);

            var scanner = new MediaScan(mhddLog, ibgLog, devicePath, dev, useBufferedReads);
            scanner.UpdateStatus         += Progress.UpdateStatus;
            scanner.StoppingErrorMessage += Progress.ErrorMessage;
            scanner.UpdateProgress       += Progress.UpdateProgress;
            scanner.PulseProgress        += Progress.PulseProgress;
            scanner.InitProgress         += Progress.InitProgress;
            scanner.EndProgress          += Progress.EndProgress;

            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                scanner.Abort();
            };

            ScanResults results = scanner.Scan();

            AaruConsole.WriteLine("Took a total of {0} seconds ({1} processing commands).", results.TotalTime,
                                  results.ProcessingTime);

            AaruConsole.WriteLine("Average speed: {0:F3} MiB/sec.", results.AvgSpeed);
            AaruConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", results.MaxSpeed);
            AaruConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", results.MinSpeed);
            AaruConsole.WriteLine("Summary:");
            AaruConsole.WriteLine("{0} sectors took less than 3 ms.", results.A);
            AaruConsole.WriteLine("{0} sectors took less than 10 ms but more than 3 ms.", results.B);
            AaruConsole.WriteLine("{0} sectors took less than 50 ms but more than 10 ms.", results.C);
            AaruConsole.WriteLine("{0} sectors took less than 150 ms but more than 50 ms.", results.D);
            AaruConsole.WriteLine("{0} sectors took less than 500 ms but more than 150 ms.", results.E);
            AaruConsole.WriteLine("{0} sectors took more than 500 ms.", results.F);
            AaruConsole.WriteLine("{0} sectors could not be read.", results.UnreadableSectors.Count);

            if(results.UnreadableSectors.Count > 0)
                foreach(ulong bad in results.UnreadableSectors)
                    AaruConsole.WriteLine("Sector {0} could not be read", bad);

            AaruConsole.WriteLine();

            if(results.SeekTotal > 0               ||
               results.SeekMin   < double.MaxValue ||
               results.SeekMax   > double.MinValue)

                AaruConsole.
                    WriteLine("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                              results.SeekTimes, results.SeekMax, results.SeekMin, results.SeekTotal / 1000);

            dev.Close();

            return (int)ErrorNumber.NoError;
        }
    }
}