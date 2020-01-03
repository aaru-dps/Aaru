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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.CommandLine;
using System.CommandLine.Invocation;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Scanning;
using DiscImageChef.Devices;

namespace DiscImageChef.Commands.Media
{
    internal class MediaScanCommand : Command
    {
        public MediaScanCommand() : base("scan", "Scans the media inserted on a device.")
        {
            Add(new Option(new[]
                {
                    "--mhdd-log", "-m"
                }, "Write a log of the scan in the format used by MHDD.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option(new[]
                {
                    "--ibg-log", "-b"
                }, "Write a log of the scan in the format used by ImgBurn.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Device path", Name = "device-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string devicePath, string ibgLog, string mhddLog)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("media-scan");

            DicConsole.DebugWriteLine("Media-Scan command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Media-Scan command", "--device={0}", devicePath);
            DicConsole.DebugWriteLine("Media-Scan command", "--ibg-log={0}", ibgLog);
            DicConsole.DebugWriteLine("Media-Scan command", "--mhdd-log={0}", mhddLog);
            DicConsole.DebugWriteLine("Media-Scan command", "--verbose={0}", verbose);

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
                    DicConsole.ErrorWriteLine(Error.Print(dev.LastError));

                    return(int)ErrorNumber.CannotOpenDevice;
                }
            }
            catch(DeviceException e)
            {
                DicConsole.ErrorWriteLine(e.Message ?? Error.Print(e.LastError));

                return(int)ErrorNumber.CannotOpenDevice;
            }

            Statistics.AddDevice(dev);

            var scanner = new MediaScan(mhddLog, ibgLog, devicePath, dev);
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
            DicConsole.WriteLine("{0} sectors could not be read.", results.UnreadableSectors.Count);

            if(results.UnreadableSectors.Count > 0)
                foreach(ulong bad in results.UnreadableSectors)
                    DicConsole.WriteLine("Sector {0} could not be read", bad);

            DicConsole.WriteLine();

            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if(results.SeekTotal != 0               ||
               results.SeekMin   != double.MaxValue ||
               results.SeekMax   != double.MinValue)

                // ReSharper restore CompareOfFloatsByEqualityOperator
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                DicConsole.WriteLine("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                                     results.SeekTimes, results.SeekMax, results.SeekMin, results.SeekTotal / 1000);

            dev.Close();

            return(int)ErrorNumber.NoError;
        }
    }
}