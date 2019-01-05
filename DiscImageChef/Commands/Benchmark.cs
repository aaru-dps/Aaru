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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Mono.Options;

namespace DiscImageChef.Commands
{
    class BenchmarkCommand : Command
    {
        int  blockSize  = 512;
        int  bufferSize = 128;
        bool showHelp;

        public BenchmarkCommand() : base("benchmark", "Benchmarks hashing and entropy calculation.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS]",
                "",
                Help,
                {"block-size|b=", "Block size.", (int                i) => blockSize  = i},
                {"buffer-size|s=", "Buffer size in mebibytes.", (int i) => bufferSize = i},
                {"help|h|?", "Show this message and exit.", v => showHelp             = v != null}
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
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

            DicConsole.DebugWriteLine("Benchmark command", "--debug={0}",   MainClass.Debug);
            DicConsole.DebugWriteLine("Benchmark command", "--verbose={0}", MainClass.Verbose);

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

            Statistics.AddCommand("benchmark");
            return (int)ErrorNumber.NoError;
        }
    }
}