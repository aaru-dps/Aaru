// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Statistics.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'stats' verb.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Console;
using DiscImageChef.Metadata;

namespace DiscImageChef.Commands
{
    static class Statistics
    {
        internal static void ShowStats()
        {
            if(Core.Statistics.AllStats == null)
            {
                DicConsole.WriteLine("There are no statistics.");
                return;
            }

            bool thereAreStats = false;

            if(Core.Statistics.AllStats.Commands != null)
            {
                DicConsole.WriteLine("Commands statistics");
                DicConsole.WriteLine("===================");
                if(Core.Statistics.AllStats.Commands.Analyze > 0)
                    DicConsole.WriteLine("You have called the Analyze command {0} times",
                                         Core.Statistics.AllStats.Commands.Analyze);
                if(Core.Statistics.AllStats.Commands.Benchmark > 0)
                    DicConsole.WriteLine("You have called the Benchmark command {0} times",
                                         Core.Statistics.AllStats.Commands.Benchmark);
                if(Core.Statistics.AllStats.Commands.Checksum > 0)
                    DicConsole.WriteLine("You have called the Checksum command {0} times",
                                         Core.Statistics.AllStats.Commands.Checksum);
                if(Core.Statistics.AllStats.Commands.Compare > 0)
                    DicConsole.WriteLine("You have called the Compare command {0} times",
                                         Core.Statistics.AllStats.Commands.Compare);
                if(Core.Statistics.AllStats.Commands.CreateSidecar > 0)
                    DicConsole.WriteLine("You have called the Create-Sidecar command {0} times",
                                         Core.Statistics.AllStats.Commands.CreateSidecar);
                if(Core.Statistics.AllStats.Commands.Decode > 0)
                    DicConsole.WriteLine("You have called the Decode command {0} times",
                                         Core.Statistics.AllStats.Commands.Decode);
                if(Core.Statistics.AllStats.Commands.DeviceInfo > 0)
                    DicConsole.WriteLine("You have called the Device-Info command {0} times",
                                         Core.Statistics.AllStats.Commands.DeviceInfo);
                if(Core.Statistics.AllStats.Commands.DeviceReport > 0)
                    DicConsole.WriteLine("You have called the Device-Report command {0} times",
                                         Core.Statistics.AllStats.Commands.DeviceReport);
                if(Core.Statistics.AllStats.Commands.DumpMedia > 0)
                    DicConsole.WriteLine("You have called the Dump-Media command {0} times",
                                         Core.Statistics.AllStats.Commands.DumpMedia);
                if(Core.Statistics.AllStats.Commands.Entropy > 0)
                    DicConsole.WriteLine("You have called the Entropy command {0} times",
                                         Core.Statistics.AllStats.Commands.Entropy);
                if(Core.Statistics.AllStats.Commands.Formats > 0)
                    DicConsole.WriteLine("You have called the Formats command {0} times",
                                         Core.Statistics.AllStats.Commands.Formats);
                if(Core.Statistics.AllStats.Commands.MediaInfo > 0)
                    DicConsole.WriteLine("You have called the Media-Info command {0} times",
                                         Core.Statistics.AllStats.Commands.MediaInfo);
                if(Core.Statistics.AllStats.Commands.MediaScan > 0)
                    DicConsole.WriteLine("You have called the Media-Scan command {0} times",
                                         Core.Statistics.AllStats.Commands.MediaScan);
                if(Core.Statistics.AllStats.Commands.PrintHex > 0)
                    DicConsole.WriteLine("You have called the Print-Hex command {0} times",
                                         Core.Statistics.AllStats.Commands.PrintHex);
                if(Core.Statistics.AllStats.Commands.Verify > 0)
                    DicConsole.WriteLine("You have called the Verify command {0} times",
                                         Core.Statistics.AllStats.Commands.Verify);
                DicConsole.WriteLine();
                thereAreStats = true;
            }

            if(Core.Statistics.AllStats.Benchmark != null)
            {
                DicConsole.WriteLine("Benchmark statistics");
                DicConsole.WriteLine("====================");
                foreach(ChecksumStats chk in Core.Statistics.AllStats.Benchmark.Checksum)
                {
                    DicConsole.WriteLine("Took {0} seconds to calculate {1} algorithm", chk.Value, chk.algorithm);
                }

                DicConsole.WriteLine("Took {0} seconds to calculate all algorithms sequentially",
                                     Core.Statistics.AllStats.Benchmark.Sequential);
                DicConsole.WriteLine("Took {0} seconds to calculate all algorithms at the same time",
                                     Core.Statistics.AllStats.Benchmark.All);
                DicConsole.WriteLine("Took {0} seconds to calculate entropy",
                                     Core.Statistics.AllStats.Benchmark.Entropy);
                DicConsole.WriteLine("Used a maximum of {0} bytes of memory",
                                     Core.Statistics.AllStats.Benchmark.MaxMemory);
                DicConsole.WriteLine("Used a minimum of {0} bytes of memory",
                                     Core.Statistics.AllStats.Benchmark.MinMemory);
                DicConsole.WriteLine();
                thereAreStats = true;
            }

            if(Core.Statistics.AllStats.Filters != null && Core.Statistics.AllStats.Filters.Count > 0)
            {
                DicConsole.WriteLine("Filters statistics");
                DicConsole.WriteLine("==================");
                foreach(NameValueStats nvs in Core.Statistics.AllStats.Filters)
                    DicConsole.WriteLine("Filter {0} has been found {1} times.", nvs.name, nvs.Value);

                DicConsole.WriteLine();
                thereAreStats = true;
            }

            if(Core.Statistics.AllStats.MediaImages != null && Core.Statistics.AllStats.MediaImages.Count > 0)
            {
                DicConsole.WriteLine("Media image statistics");
                DicConsole.WriteLine("======================");
                foreach(NameValueStats nvs in Core.Statistics.AllStats.MediaImages)
                    DicConsole.WriteLine("Format {0} has been found {1} times.", nvs.name, nvs.Value);

                DicConsole.WriteLine();
                thereAreStats = true;
            }

            if(Core.Statistics.AllStats.Partitions != null && Core.Statistics.AllStats.Partitions.Count > 0)
            {
                DicConsole.WriteLine("Partition statistics");
                DicConsole.WriteLine("====================");
                foreach(NameValueStats nvs in Core.Statistics.AllStats.Partitions)
                    DicConsole.WriteLine("Partitioning scheme {0} has been found {1} times.", nvs.name, nvs.Value);

                DicConsole.WriteLine();
                thereAreStats = true;
            }

            if(Core.Statistics.AllStats.Filesystems != null && Core.Statistics.AllStats.Filesystems.Count > 0)
            {
                DicConsole.WriteLine("Filesystem statistics");
                DicConsole.WriteLine("=====================");
                foreach(NameValueStats nvs in Core.Statistics.AllStats.Filesystems)
                    DicConsole.WriteLine("Filesystem {0} has been found {1} times.", nvs.name, nvs.Value);

                DicConsole.WriteLine();
                thereAreStats = true;
            }

            if(Core.Statistics.AllStats.Devices != null && Core.Statistics.AllStats.Devices.Count > 0)
            {
                DicConsole.WriteLine("Device statistics");
                DicConsole.WriteLine("=================");
                foreach(DeviceStats ds in Core.Statistics.AllStats.Devices)
                    DicConsole
                        .WriteLine("Device model {0}, manufactured by {1}, with revision {2} and attached via {3}.",
                                   ds.Model, ds.Manufacturer, ds.Revision, ds.Bus);

                DicConsole.WriteLine();
                thereAreStats = true;
            }

            if(Core.Statistics.AllStats.Medias != null && Core.Statistics.AllStats.Medias.Count > 0)
            {
                DicConsole.WriteLine("Media statistics");
                DicConsole.WriteLine("================");
                foreach(MediaStats ms in Core.Statistics.AllStats.Medias)
                {
                    if(ms.real)
                        DicConsole.WriteLine("Media type {0} has been found {1} times in a real device.", ms.type,
                                             ms.Value);
                    else
                        DicConsole.WriteLine("Media type {0} has been found {1} times in a media image.", ms.type,
                                             ms.Value);
                }

                DicConsole.WriteLine();
                thereAreStats = true;
            }

            if(Core.Statistics.AllStats.MediaScan != null)
            {
                DicConsole.WriteLine("Media scan statistics");
                DicConsole.WriteLine("=====================");
                DicConsole.WriteLine("Scanned a total of {0} sectors",
                                     Core.Statistics.AllStats.MediaScan.Sectors.Total);
                DicConsole.WriteLine("{0} of them correctly", Core.Statistics.AllStats.MediaScan.Sectors.Correct);
                DicConsole.WriteLine("{0} of them had errors", Core.Statistics.AllStats.MediaScan.Sectors.Error);
                DicConsole.WriteLine("{0} of them took less than 3 ms",
                                     Core.Statistics.AllStats.MediaScan.Times.LessThan3ms);
                DicConsole.WriteLine("{0} of them took less than 10 ms but more than 3 ms",
                                     Core.Statistics.AllStats.MediaScan.Times.LessThan10ms);
                DicConsole.WriteLine("{0} of them took less than 50 ms but more than 10 ms",
                                     Core.Statistics.AllStats.MediaScan.Times.LessThan50ms);
                DicConsole.WriteLine("{0} of them took less than 150 ms but more than 50 ms",
                                     Core.Statistics.AllStats.MediaScan.Times.LessThan150ms);
                DicConsole.WriteLine("{0} of them took less than 500 ms but more than 150 ms",
                                     Core.Statistics.AllStats.MediaScan.Times.LessThan500ms);
                DicConsole.WriteLine("{0} of them took less than more than 500 ms",
                                     Core.Statistics.AllStats.MediaScan.Times.MoreThan500ms);
                thereAreStats = true;
            }

            if(Core.Statistics.AllStats.Verify != null)
            {
                DicConsole.WriteLine("Verification statistics");
                DicConsole.WriteLine("=======================");
                DicConsole.WriteLine("{0} media images has been correctly verified",
                                     Core.Statistics.AllStats.Verify.MediaImages.Correct);
                DicConsole.WriteLine("{0} media images has been determined as containing errors",
                                     Core.Statistics.AllStats.Verify.MediaImages.Failed);
                DicConsole.WriteLine("{0} sectors has been verified", Core.Statistics.AllStats.Verify.Sectors.Total);
                DicConsole.WriteLine("{0} sectors has been determined correct",
                                     Core.Statistics.AllStats.Verify.Sectors.Correct);
                DicConsole.WriteLine("{0} sectors has been determined to contain errors",
                                     Core.Statistics.AllStats.Verify.Sectors.Error);
                DicConsole.WriteLine("{0} sectors could not be determined as correct or not",
                                     Core.Statistics.AllStats.Verify.Sectors.Unverifiable);
                thereAreStats = true;
            }

            if(!thereAreStats) DicConsole.WriteLine("There are no statistics.");
        }
    }
}