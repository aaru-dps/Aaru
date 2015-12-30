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
using System.Collections.Generic;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using DiscImageChef.Plugins;
using System.Reflection;
using DiscImageChef.Console;

namespace DiscImageChef
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            string invokedVerb = "";
            object invokedVerbInstance = null;

            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options,
                (verb, subOptions) =>
            {
                // if parsing succeeds the verb name and correct instance
                // will be passed to onVerbCommand delegate (string,object)
                invokedVerb = verb;
                invokedVerbInstance = subOptions;
            }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string AssemblyTitle = ((AssemblyTitleAttribute) attributes[0]).Title;
            attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            Version AssemblyVersion = typeof(MainClass).Assembly.GetName().Version;
            string AssemblyCopyright  = ((AssemblyCopyrightAttribute) attributes[0]).Copyright;

            DicConsole.WriteLineEvent += System.Console.WriteLine;
            DicConsole.WriteEvent += System.Console.Write;
            DicConsole.ErrorWriteLineEvent += System.Console.Error.WriteLine;

            DicConsole.WriteLine("{0} {1}", AssemblyTitle, AssemblyVersion);
            DicConsole.WriteLine("{0}", AssemblyCopyright);
            DicConsole.WriteLine();

            switch (invokedVerb)
            {
                case "analyze":
                    AnalyzeSubOptions AnalyzeOptions = (AnalyzeSubOptions)invokedVerbInstance;
                    if (AnalyzeOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (AnalyzeOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.Analyze.doAnalyze(AnalyzeOptions);
                    break;
                case "compare":
                    CompareSubOptions CompareOptions = (CompareSubOptions)invokedVerbInstance;
                    if (CompareOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (CompareOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.Compare.doCompare(CompareOptions);
                    break;
                case "checksum":
                    ChecksumSubOptions ChecksumOptions = (ChecksumSubOptions)invokedVerbInstance;
                    if (ChecksumOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (ChecksumOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.Checksum.doChecksum(ChecksumOptions);
                    break;
                case "entropy":
                    EntropySubOptions entropyOptions = (EntropySubOptions)invokedVerbInstance;
                    if (entropyOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (entropyOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.Entropy.doEntropy(entropyOptions);
                    break;
                case "verify":
                    VerifySubOptions VerifyOptions = (VerifySubOptions)invokedVerbInstance;
                    if (VerifyOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (VerifyOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.Verify.doVerify(VerifyOptions);
                    break;
                case "printhex":
                    PrintHexSubOptions PrintHexOptions = (PrintHexSubOptions)invokedVerbInstance;
                    if (PrintHexOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (PrintHexOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.PrintHex.doPrintHex(PrintHexOptions);
                    break;
                case "decode":
                    DecodeSubOptions DecodeOptions = (DecodeSubOptions)invokedVerbInstance;
                    if (DecodeOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (DecodeOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.Decode.doDecode(DecodeOptions);
                    break;
                case "formats":
                    FormatsSubOptions FormatsOptions = (FormatsSubOptions)invokedVerbInstance;
                    if (FormatsOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (FormatsOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.Formats.ListFormats(FormatsOptions);
                    break;
                case "device-info":
                    DeviceInfoSubOptions DeviceInfoOptions = (DeviceInfoSubOptions)invokedVerbInstance;
                    if (DeviceInfoOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (DeviceInfoOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.DeviceInfo.doDeviceInfo(DeviceInfoOptions);
                    break;
                case "media-info":
                    MediaInfoSubOptions MediaInfoOptions = (MediaInfoSubOptions)invokedVerbInstance;
                    if (MediaInfoOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (MediaInfoOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.MediaInfo.doMediaInfo(MediaInfoOptions);
                    break;
                case "benchmark":
                    BenchmarkSubOptions BenchmarkOptions = (BenchmarkSubOptions)invokedVerbInstance;
                    if (BenchmarkOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (BenchmarkOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.Benchmark.doBenchmark(BenchmarkOptions);
                    break;
                case "create-sidecar":
                    CreateSidecarSubOptions CreateSidecarOptions = (CreateSidecarSubOptions)invokedVerbInstance;
                    if (CreateSidecarOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (CreateSidecarOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.CreateSidecar.doSidecar(CreateSidecarOptions);
                    break;
                case "media-scan":
                    MediaScanSubOptions MediaScanOptions = (MediaScanSubOptions)invokedVerbInstance;
                    if (MediaScanOptions.Debug)
                        DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
                    if (MediaScanOptions.Verbose)
                        DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
                    Commands.MediaScan.doMediaScan(MediaScanOptions);
                    break;
                default:
                    throw new ArgumentException("Should never arrive here!");
            }
        }
    }
}

