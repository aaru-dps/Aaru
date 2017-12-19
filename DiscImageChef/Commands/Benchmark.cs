// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Benchmark.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'benchmark' verb.
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

using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    public static class Benchmark
    {
        public static void doBenchmark(BenchmarkOptions options)
        {
            Dictionary<string, double> checksumTimes = new Dictionary<string, double>();
            Core.Benchmark.InitProgressEvent += Progress.InitProgress;
            Core.Benchmark.UpdateProgressEvent += Progress.UpdateProgress;
            Core.Benchmark.EndProgressEvent += Progress.EndProgress;

            BenchmarkResults results = Core.Benchmark.Do(options.BufferSize * 1024 * 1024, options.BlockSize);

            DicConsole.WriteLine("Took {0} seconds to fill buffer, {1:F3} MiB/sec.", results.fillTime,
                                 results.fillSpeed);
            DicConsole.WriteLine("Took {0} seconds to read buffer, {1:F3} MiB/sec.", results.readTime,
                                 results.readSpeed);
            DicConsole.WriteLine("Took {0} seconds to entropy buffer, {1:F3} MiB/sec.", results.entropyTime,
                                 results.entropySpeed);

            foreach(KeyValuePair<string, BenchmarkEntry> entry in results.entries)
            {
                checksumTimes.Add(entry.Key, entry.Value.timeSpan);
                DicConsole.WriteLine("Took {0} seconds to {1} buffer, {2:F3} MiB/sec.", entry.Value.timeSpan, entry.Key,
                                     entry.Value.speed);
            }

            DicConsole.WriteLine("Took {0} seconds to do all algorithms at the same time, {1} MiB/sec.",
                                 results.totalTime, results.totalSpeed);
            DicConsole.WriteLine("Took {0} seconds to do all algorithms sequentially, {1} MiB/sec.",
                                 results.separateTime, results.separateSpeed);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Max memory used is {0} bytes", results.maxMemory);
            DicConsole.WriteLine("Min memory used is {0} bytes", results.minMemory);

            Core.Statistics.AddCommand("benchmark");
            Core.Statistics.AddBenchmark(checksumTimes, results.entropyTime, results.totalTime, results.separateTime,
                                         results.maxMemory, results.minMemory);
        }
    }
}