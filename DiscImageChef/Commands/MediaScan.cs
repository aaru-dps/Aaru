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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Console;
using DiscImageChef.Core.Devices.Scanning;
using DiscImageChef.Devices;

namespace DiscImageChef.Commands
{
    public static class MediaScan
    {
        public static void doMediaScan(MediaScanOptions options)
        {
            DicConsole.DebugWriteLine("Media-Scan command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Media-Scan command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Media-Scan command", "--device={0}", options.DevicePath);
            DicConsole.DebugWriteLine("Media-Scan command", "--mhdd-log={0}", options.MHDDLogPath);
            DicConsole.DebugWriteLine("Media-Scan command", "--ibg-log={0}", options.IBGLogPath);

            if(!System.IO.File.Exists(options.DevicePath))
            {
                DicConsole.ErrorWriteLine("Specified device does not exist.");
                return;
            }

            if(options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + char.ToUpper(options.DevicePath[0]) + ':';
            }

            Device dev = new Device(options.DevicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Core.Statistics.AddDevice(dev);

            ScanResults results = new ScanResults();

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    results = ATA.Scan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    results = SecureDigital.Scan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                case DeviceType.NVMe:
                    results = NVMe.Scan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    results = SCSI.Scan(options.MHDDLogPath, options.IBGLogPath, options.DevicePath, dev);
                    break;
                default:
                    throw new NotSupportedException("Unknown device type.");
            }

            DicConsole.WriteLine("Took a total of {0} seconds ({1} processing commands).", results.totalTime, results.processingTime);
            DicConsole.WriteLine("Avegare speed: {0:F3} MiB/sec.", results.avgSpeed);
            DicConsole.WriteLine("Fastest speed burst: {0:F3} MiB/sec.", results.maxSpeed);
            DicConsole.WriteLine("Slowest speed burst: {0:F3} MiB/sec.", results.minSpeed);
            DicConsole.WriteLine("Summary:");
            DicConsole.WriteLine("{0} sectors took less than 3 ms.", results.A);
            DicConsole.WriteLine("{0} sectors took less than 10 ms but more than 3 ms.", results.B);
            DicConsole.WriteLine("{0} sectors took less than 50 ms but more than 10 ms.", results.C);
            DicConsole.WriteLine("{0} sectors took less than 150 ms but more than 50 ms.", results.D);
            DicConsole.WriteLine("{0} sectors took less than 500 ms but more than 150 ms.", results.E);
            DicConsole.WriteLine("{0} sectors took more than 500 ms.", results.F);
            DicConsole.WriteLine("{0} sectors could not be read.", results.unreadableSectors.Count);
            if(results.unreadableSectors.Count > 0)
            {
                foreach(ulong bad in results.unreadableSectors)
                    DicConsole.WriteLine("Sector {0} could not be read", bad);
            }
            DicConsole.WriteLine();

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if(results.seekTotal != 0 || results.seekMin != double.MaxValue || results.seekMax != double.MinValue)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                DicConsole.WriteLine("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                                 results.seekTimes, results.seekMax, results.seekMin, results.seekTotal / 1000);

            Core.Statistics.AddMediaScan((long)results.A, (long)results.B, (long)results.C, (long)results.D, (long)results.E, (long)results.F, (long)results.blocks, (long)results.errored, (long)(results.blocks - results.errored));
            Core.Statistics.AddCommand("media-scan");
        }
    }
}

