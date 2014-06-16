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

namespace DiscImageChef
{
    class MainClass
    {

        public static bool isDebug;
        public static bool isVerbose;

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

            Console.WriteLine("{0} {1}", AssemblyTitle, AssemblyVersion);
            Console.WriteLine("{0}", AssemblyCopyright);
            Console.WriteLine();

            switch (invokedVerb)
            {
                case "analyze":
                    AnalyzeSubOptions AnalyzeOptions = (AnalyzeSubOptions)invokedVerbInstance;
                    isDebug = AnalyzeOptions.Debug;
                    isVerbose = AnalyzeOptions.Verbose;
                    Commands.Analyze.doAnalyze(AnalyzeOptions);
                    break;
                case "compare":
                    CompareSubOptions CompareOptions = (CompareSubOptions)invokedVerbInstance;
                    isDebug = CompareOptions.Debug;
                    isVerbose = CompareOptions.Verbose;
                    Commands.Compare.doCompare(CompareOptions);
                    break;
                case "checksum":
                    ChecksumSubOptions ChecksumOptions = (ChecksumSubOptions)invokedVerbInstance;
                    isDebug = ChecksumOptions.Debug;
                    isVerbose = ChecksumOptions.Verbose;
                    Commands.Checksum.doChecksum(ChecksumOptions);
                    break;
                case "verify":
                    VerifySubOptions VerifyOptions = (VerifySubOptions)invokedVerbInstance;
                    isDebug = VerifyOptions.Debug;
                    isVerbose = VerifyOptions.Verbose;
                    Commands.Verify.doVerify(VerifyOptions);
                    break;
                case "formats":
                    FormatsSubOptions FormatsOptions = (FormatsSubOptions)invokedVerbInstance;
                    isVerbose = FormatsOptions.Verbose;
                    Commands.Formats.ListFormats();
                    break;
                default:
                    throw new ArgumentException("Should never arrive here!");
            }
        }
    }
}

