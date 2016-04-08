/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Main.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Main program loop.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Contains the main program loop.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Reflection;
using DiscImageChef.Console;
using DiscImageChef.Settings;
using CommandLine;

namespace DiscImageChef
{
    class MainClass
    {
        public static void Main(string [] args)
        {
            object [] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string AssemblyTitle = ((AssemblyTitleAttribute)attributes [0]).Title;
            attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            Version AssemblyVersion = typeof(MainClass).Assembly.GetName().Version;
            string AssemblyCopyright = ((AssemblyCopyrightAttribute)attributes [0]).Copyright;

            DicConsole.WriteLineEvent += System.Console.WriteLine;
            DicConsole.WriteEvent += System.Console.Write;
            DicConsole.ErrorWriteLineEvent += System.Console.Error.WriteLine;

            DicConsole.WriteLine("{0} {1}", AssemblyTitle, AssemblyVersion);
            DicConsole.WriteLine("{0}", AssemblyCopyright);
            DicConsole.WriteLine();

            Settings.Settings.LoadSettings();
            Core.Statistics.LoadStats();

            Parser.Default.ParseArguments<AnalyzeOptions, CompareOptions, ChecksumOptions, EntropyOptions, VerifyOptions, PrintHexOptions,
                  DecodeOptions, DeviceInfoOptions, MediaInfoOptions, MediaScanOptions, FormatsOptions, BenchmarkOptions, CreateSidecarOptions,
                    DumpMediaOptions, DeviceReportOptions, ConfigureOptions, StatsOptions>(args)
                  .WithParsed<AnalyzeOptions>(opts =>
                  {
                      if (opts.Debug)
                          DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if (opts.Verbose)
                          DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      Commands.Analyze.doAnalyze(opts);
                  })
                .WithParsed<CompareOptions>(opts =>
                 {
                     if (opts.Debug)
                         DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                     if (opts.Verbose)
                         DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                     Commands.Compare.doCompare(opts);
                 })
            .WithParsed<ChecksumOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.Checksum.doChecksum(opts);
            })

            .WithParsed<EntropyOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.Entropy.doEntropy(opts);
            })

            .WithParsed<VerifyOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.Verify.doVerify(opts);
            })

            .WithParsed<PrintHexOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.PrintHex.doPrintHex(opts);
            })

            .WithParsed<DecodeOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.Decode.doDecode(opts);
            })

            .WithParsed<DeviceInfoOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.DeviceInfo.doDeviceInfo(opts);
            })

            .WithParsed<MediaInfoOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.MediaInfo.doMediaInfo(opts);
            })

            .WithParsed<MediaScanOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.MediaScan.doMediaScan(opts);
            })

            .WithParsed<FormatsOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.Formats.ListFormats(opts);
            })

            .WithParsed<BenchmarkOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.Benchmark.doBenchmark(opts);
            })

            .WithParsed<CreateSidecarOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.CreateSidecar.doSidecar(opts);
            })

            .WithParsed<DumpMediaOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.DumpMedia.doDumpMedia(opts);
            })

            .WithParsed<DeviceReportOptions>(opts =>
            {
                if (opts.Debug)
                    DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                if (opts.Verbose)
                    DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                Commands.DeviceReport.doDeviceReport(opts);
            })

                  .WithParsed<ConfigureOptions>(opts => { Commands.Configure.doConfigure();})
                  .WithParsed<StatsOptions>(opts => { Commands.Statistics.showStats(); })
                  .WithNotParsed(errs => Environment.Exit(1));

            Core.Statistics.SaveStats();
        }
    }
}

