// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Main.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Main program loop.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the main program loop.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Reflection;
using DiscImageChef.Console;
using DiscImageChef.Settings;
using CommandLine;

namespace DiscImageChef
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            DicConsole.WriteLineEvent += System.Console.WriteLine;
            DicConsole.WriteEvent += System.Console.Write;
            DicConsole.ErrorWriteLineEvent += System.Console.Error.WriteLine;

            Settings.Settings.LoadSettings();
            Core.Statistics.LoadStats();

            Parser.Default.ParseArguments<AnalyzeOptions, CompareOptions, ChecksumOptions, EntropyOptions, VerifyOptions, PrintHexOptions,
                  DecodeOptions, DeviceInfoOptions, MediaInfoOptions, MediaScanOptions, FormatsOptions, BenchmarkOptions, CreateSidecarOptions,
                  DumpMediaOptions, DeviceReportOptions, ConfigureOptions, StatsOptions, LsOptions, ExtractFilesOptions>(args)
                  .WithParsed<AnalyzeOptions>(opts =>
                  {
                      if(opts.Debug)
                          DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose)
                          DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Analyze.doAnalyze(opts);
                  })
                .WithParsed<CompareOptions>(opts =>
                 {
                     if(opts.Debug)
                         DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                     if(opts.Verbose)
                         DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                     PrintCopyright();
                     Commands.Compare.doCompare(opts);
                 })
            .WithParsed<ChecksumOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.Checksum.doChecksum(opts);
            })

            .WithParsed<EntropyOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.Entropy.doEntropy(opts);
            })

            .WithParsed<VerifyOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.Verify.doVerify(opts);
            })

            .WithParsed<PrintHexOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.PrintHex.doPrintHex(opts);
            })

            .WithParsed<DecodeOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.Decode.doDecode(opts);
            })

            .WithParsed<DeviceInfoOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.DeviceInfo.doDeviceInfo(opts);
            })

            .WithParsed<MediaInfoOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.MediaInfo.doMediaInfo(opts);
            })

            .WithParsed<MediaScanOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.MediaScan.doMediaScan(opts);
            })

            .WithParsed<FormatsOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.Formats.ListFormats(opts);
            })

            .WithParsed<BenchmarkOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.Benchmark.doBenchmark(opts);
            })

            .WithParsed<CreateSidecarOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.CreateSidecar.doSidecar(opts);
            })

            .WithParsed<DumpMediaOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.DumpMedia.doDumpMedia(opts);
            })

            .WithParsed<DeviceReportOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.DeviceReport.doDeviceReport(opts);
            })

            .WithParsed<LsOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.Ls.doLs(opts);
            })

            .WithParsed<ExtractFilesOptions>(opts =>
            {
                if(opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if(opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                PrintCopyright();
                Commands.ExtractFiles.doExtractFiles(opts);
            })


                  .WithParsed<ConfigureOptions>(opts => { PrintCopyright(); Commands.Configure.doConfigure(); })
                  .WithParsed<StatsOptions>(opts => { PrintCopyright(); Commands.Statistics.showStats(); })
                  .WithNotParsed(errs => Environment.Exit(1));

            Core.Statistics.SaveStats();
        }

        static void PrintCopyright()
        {
            object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string AssemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            Version AssemblyVersion = typeof(MainClass).Assembly.GetName().Version;
            string AssemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            DicConsole.WriteLine("{0} {1}", AssemblyTitle, AssemblyVersion);
            DicConsole.WriteLine("{0}", AssemblyCopyright);
            DicConsole.WriteLine();
        }
    }
}

