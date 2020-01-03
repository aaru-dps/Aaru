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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    internal class BenchmarkCommand : Command
    {
        public BenchmarkCommand() : base("benchmark", "Benchmarks hashing and entropy calculation.")
        {
            Add(new Option(new[]
                {
                    "--block-size", "-b"
                }, "Block size.")
                {
                    Argument = new Argument<int>(() => 512), Required = false
                });

            Add(new Option(new[]
                {
                    "--buffer-size", "-s"
                }, "Buffer size in mebibytes.")
                {
                    Argument = new Argument<int>(() => 128), Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Disc image path", Name = "image-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, int blockSize, int bufferSize)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("benchmark");

            DicConsole.DebugWriteLine("Benchmark command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Benchmark command", "--verbose={0}", verbose);

            Benchmark.InitProgressEvent   += Progress.InitProgress;
            Benchmark.UpdateProgressEvent += Progress.UpdateProgress;
            Benchmark.EndProgressEvent    += Progress.EndProgress;

            BenchmarkResults results = Benchmark.Do(bufferSize * 1024 * 1024, blockSize);

            DicConsole.WriteLine("Took {0} seconds to fill buffer, {1:F3} MiB/sec.", results.FillTime,
                                 results.FillSpeed);

            DicConsole.WriteLine("Took {0} seconds to read buffer, {1:F3} MiB/sec.", results.ReadTime,
                                 results.ReadSpeed);

            DicConsole.WriteLine("Took {0} seconds to entropy buffer, {1:F3} MiB/sec.", results.EntropyTime,
                                 results.EntropySpeed);

            foreach(KeyValuePair<string, BenchmarkEntry> entry in results.Entries)
                DicConsole.WriteLine("Took {0} seconds to {1} buffer, {2:F3} MiB/sec.", entry.Value.TimeSpan, entry.Key,
                                     entry.Value.Speed);

            DicConsole.WriteLine("Took {0} seconds to do all algorithms at the same time, {1:F3} MiB/sec.",
                                 results.TotalTime, results.TotalSpeed);

            DicConsole.WriteLine("Took {0} seconds to do all algorithms sequentially, {1:F3} MiB/sec.",
                                 results.SeparateTime, results.SeparateSpeed);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Max memory used is {0} bytes", results.MaxMemory);
            DicConsole.WriteLine("Min memory used is {0} bytes", results.MinMemory);

            return(int)ErrorNumber.NoError;
        }
    }
}