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

using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;

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
                if(Core.Statistics.AllStats.Commands.ConvertImage > 0)
                    DicConsole.WriteLine("You have called the Convert-Image command {0} times",
                                         Core.Statistics.AllStats.Commands.ConvertImage);
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
                if(Core.Statistics.AllStats.Commands.ImageInfo > 0)
                    DicConsole.WriteLine("You have called the Image-Info command {0} times",
                                         Core.Statistics.AllStats.Commands.ImageInfo);
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
                    DicConsole.WriteLine("Media type {0} has been found {1} times in a {2} image.", ms.type, ms.Value,
                                         ms.real ? "real" : "media");

                DicConsole.WriteLine();
                thereAreStats = true;
            }

            if(!thereAreStats) DicConsole.WriteLine("There are no statistics.");
        }
    }
}