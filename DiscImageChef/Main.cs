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
using DiscImageChef.Commands;
using DiscImageChef.Console;
using DiscImageChef.Settings;
using Statistics = DiscImageChef.Core.Statistics;

namespace DiscImageChef
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            DicConsole.WriteLineEvent      += System.Console.WriteLine;
            DicConsole.WriteEvent          += System.Console.Write;
            DicConsole.ErrorWriteLineEvent += System.Console.Error.WriteLine;

            Settings.Settings.LoadSettings();
            if(Settings.Settings.Current.GdprCompliance < DicSettings.GdprLevel) Configure.DoConfigure(true);
            Statistics.LoadStats();
            if(Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.ShareStats)
                Statistics.SubmitStats();

            Parser.Default.ParseArguments(args, typeof(AnalyzeOptions), typeof(BenchmarkOptions),
                                          typeof(ChecksumOptions), typeof(CompareOptions), typeof(ConfigureOptions),
                                          typeof(ConvertImageOptions), typeof(CreateSidecarOptions),
                                          typeof(DecodeOptions), typeof(DeviceInfoOptions), typeof(DeviceReportOptions),
                                          typeof(DumpMediaOptions), typeof(EntropyOptions), typeof(ExtractFilesOptions),
                                          typeof(FormatsOptions), typeof(ImageInfoOptions), typeof(ListDevicesOptions),
                                          typeof(ListEncodingsOptions), typeof(ListOptionsOptions), typeof(LsOptions),
                                          typeof(MediaInfoOptions), typeof(MediaScanOptions), typeof(PrintHexOptions),
                                          typeof(StatsOptions), typeof(VerifyOptions))
                  .WithParsed<AnalyzeOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Analyze.DoAnalyze(opts);
                   }).WithParsed<CompareOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Compare.DoCompare(opts);
                   }).WithParsed<ChecksumOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Checksum.DoChecksum(opts);
                   }).WithParsed<EntropyOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Entropy.DoEntropy(opts);
                   }).WithParsed<VerifyOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Verify.DoVerify(opts);
                   }).WithParsed<PrintHexOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Commands.PrintHex.DoPrintHex(opts);
                   }).WithParsed<DecodeOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Decode.DoDecode(opts);
                   }).WithParsed<DeviceInfoOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       DeviceInfo.DoDeviceInfo(opts);
                   }).WithParsed<MediaInfoOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       MediaInfo.DoMediaInfo(opts);
                   }).WithParsed<MediaScanOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       MediaScan.DoMediaScan(opts);
                   }).WithParsed<FormatsOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Formats.ListFormats(opts);
                   }).WithParsed<BenchmarkOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Benchmark.DoBenchmark(opts);
                   }).WithParsed<CreateSidecarOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       CreateSidecar.DoSidecar(opts);
                   }).WithParsed<DumpMediaOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       DumpMedia.DoDumpMedia(opts);
                   }).WithParsed<DeviceReportOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       DeviceReport.DoDeviceReport(opts);
                   }).WithParsed<LsOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       Ls.DoLs(opts);
                   }).WithParsed<ExtractFilesOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       ExtractFiles.DoExtractFiles(opts);
                   }).WithParsed<ListDevicesOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       ListDevices.DoListDevices(opts);
                   }).WithParsed<ListEncodingsOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       ListEncodings.DoList();
                   }).WithParsed<ListOptionsOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       ListOptions.DoList();
                   }).WithParsed<ConvertImageOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       ConvertImage.DoConvert(opts);
                   }).WithParsed<ImageInfoOptions>(opts =>
                   {
                       if(opts.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
                       if(opts.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                       PrintCopyright();
                       ImageInfo.GetImageInfo(opts);
                   }).WithParsed<ConfigureOptions>(opts =>
                   {
                       PrintCopyright();
                       Configure.DoConfigure(false);
                   }).WithParsed<StatsOptions>(opts =>
                   {
                       PrintCopyright();
                       Commands.Statistics.ShowStats();
                   }).WithNotParsed(errs => Environment.Exit(1));

            Statistics.SaveStats();
        }

        static void PrintCopyright()
        {
            object[] attributes =
                typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string assemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AssemblyInformationalVersionAttribute assemblyVersion =
                Attribute.GetCustomAttribute(typeof(MainClass).Assembly, typeof(AssemblyInformationalVersionAttribute))
                    as AssemblyInformationalVersionAttribute;
            string assemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            DicConsole.WriteLine("{0} {1}", assemblyTitle, assemblyVersion?.InformationalVersion);
            DicConsole.WriteLine("{0}",     assemblyCopyright);
            DicConsole.WriteLine();
        }
    }
}