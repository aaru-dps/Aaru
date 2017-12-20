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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Reflection;
using CommandLine;
using DiscImageChef.Console;

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
            if(Settings.Settings.Current.Stats.ShareStats) Core.Statistics.SubmitStats();

            Parser.Default
                  .ParseArguments(args,
                                  new[]
                                  {
                                      typeof(AnalyzeOptions), typeof(CompareOptions), typeof(ChecksumOptions),
                                      typeof(EntropyOptions), typeof(VerifyOptions), typeof(PrintHexOptions),
                                      typeof(DecodeOptions), typeof(DeviceInfoOptions), typeof(MediaInfoOptions),
                                      typeof(MediaScanOptions), typeof(FormatsOptions), typeof(BenchmarkOptions),
                                      typeof(CreateSidecarOptions), typeof(DumpMediaOptions),
                                      typeof(DeviceReportOptions), typeof(ConfigureOptions), typeof(StatsOptions),
                                      typeof(LsOptions), typeof(ExtractFilesOptions), typeof(ListDevicesOptions),
                                      typeof(ListEncodingsOptions)
                                  }).WithParsed<AnalyzeOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Analyze.DoAnalyze(opts);
                  }).WithParsed<CompareOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Compare.DoCompare(opts);
                  }).WithParsed<ChecksumOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Checksum.DoChecksum(opts);
                  }).WithParsed<EntropyOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Entropy.DoEntropy(opts);
                  }).WithParsed<VerifyOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Verify.DoVerify(opts);
                  }).WithParsed<PrintHexOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.PrintHex.DoPrintHex(opts);
                  }).WithParsed<DecodeOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Decode.DoDecode(opts);
                  }).WithParsed<DeviceInfoOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.DeviceInfo.DoDeviceInfo(opts);
                  }).WithParsed<MediaInfoOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.MediaInfo.DoMediaInfo(opts);
                  }).WithParsed<MediaScanOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.MediaScan.DoMediaScan(opts);
                  }).WithParsed<FormatsOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Formats.ListFormats(opts);
                  }).WithParsed<BenchmarkOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Benchmark.DoBenchmark(opts);
                  }).WithParsed<CreateSidecarOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.CreateSidecar.DoSidecar(opts);
                  }).WithParsed<DumpMediaOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.DumpMedia.DoDumpMedia(opts);
                  }).WithParsed<DeviceReportOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.DeviceReport.DoDeviceReport(opts);
                  }).WithParsed<LsOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.Ls.DoLs(opts);
                  }).WithParsed<ExtractFilesOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.ExtractFiles.DoExtractFiles(opts);
                  }).WithParsed<ListDevicesOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.ListDevices.DoListDevices(opts);
                  }).WithParsed<ListEncodingsOptions>(opts =>
                  {
                      if(opts.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                      if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                      PrintCopyright();
                      Commands.ListEncodings.DoList(opts);
                  }).WithParsed<ConfigureOptions>(opts =>
                  {
                      PrintCopyright();
                      Commands.Configure.DoConfigure();
                  }).WithParsed<StatsOptions>(opts =>
                  {
                      PrintCopyright();
                      Commands.Statistics.ShowStats();
                  }).WithNotParsed(errs => Environment.Exit(1));

            Core.Statistics.SaveStats();
        }

        static void PrintCopyright()
        {
            object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string assemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            Version assemblyVersion = typeof(MainClass).Assembly.GetName().Version;
            string assemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            DicConsole.WriteLine("{0} {1}", assemblyTitle, assemblyVersion);
            DicConsole.WriteLine("{0}", assemblyCopyright);
            DicConsole.WriteLine();
        }
    }
}