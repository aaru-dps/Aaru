// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Entropy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'entropy' verb.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Mono.Options;

namespace DiscImageChef.Commands
{
    class EntropyCommand : Command
    {
        bool   duplicatedSectors = true;
        string inputFile;
        bool   separatedTracks = true;

        bool showHelp;
        bool wholeDisc = true;

        public EntropyCommand() : base("entropy", "Calculates entropy and/or duplicated sectors of an image.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS] imagefile",
                "",
                Help,
                {
                    "duplicated-sectors|p",
                    "Calculates how many sectors are duplicated (have same exact data in user area).",
                    b => duplicatedSectors = b != null
                },
                {
                    "separated-tracks|t", "Calculates entropy for each track separately.",
                    b => separatedTracks = b != null
                },
                {"whole-disc|w", "Calculates entropy for the whole disc.", b => wholeDisc = b != null},
                {"help|h|?", "Show this message and exit.", v => showHelp                 = v != null}
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
            Statistics.AddCommand("entropy");

            if(extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int)ErrorNumber.UnexpectedArgumentCount;
            }

            if(extra.Count == 0)
            {
                DicConsole.ErrorWriteLine("Missing input image.");
                return (int)ErrorNumber.MissingArgument;
            }

            inputFile = extra[0];

            DicConsole.DebugWriteLine("Entropy command", "--debug={0}",              MainClass.Debug);
            DicConsole.DebugWriteLine("Entropy command", "--duplicated-sectors={0}", duplicatedSectors);
            DicConsole.DebugWriteLine("Entropy command", "--input={0}",              inputFile);
            DicConsole.DebugWriteLine("Entropy command", "--separated-tracks={0}",   separatedTracks);
            DicConsole.DebugWriteLine("Entropy command", "--verbose={0}",            MainClass.Verbose);
            DicConsole.DebugWriteLine("Entropy command", "--whole-disc={0}",         wholeDisc);

            FiltersList filtersList = new FiltersList();
            IFilter     inputFilter = filtersList.GetFilter(inputFile);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return (int)ErrorNumber.CannotOpenFile;
            }

            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not checksumming");
                return (int)ErrorNumber.UnrecognizedFormat;
            }

            inputFormat.Open(inputFilter);
            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Statistics.AddFilter(inputFilter.Name);

            Entropy entropyCalculator = new Entropy(MainClass.Debug, MainClass.Verbose, inputFormat);
            entropyCalculator.InitProgressEvent    += Progress.InitProgress;
            entropyCalculator.InitProgress2Event   += Progress.InitProgress2;
            entropyCalculator.UpdateProgressEvent  += Progress.UpdateProgress;
            entropyCalculator.UpdateProgress2Event += Progress.UpdateProgress2;
            entropyCalculator.EndProgressEvent     += Progress.EndProgress;
            entropyCalculator.EndProgress2Event    += Progress.EndProgress2;

            if(separatedTracks)
            {
                EntropyResults[] tracksEntropy = entropyCalculator.CalculateTracksEntropy(duplicatedSectors);
                foreach(EntropyResults trackEntropy in tracksEntropy)
                {
                    DicConsole.WriteLine("Entropy for track {0} is {1:F4}.", trackEntropy.Track, trackEntropy.Entropy);
                    if(trackEntropy.UniqueSectors != null)
                        DicConsole.WriteLine("Track {0} has {1} unique sectors ({2:P3})", trackEntropy.Track,
                                             trackEntropy.UniqueSectors,
                                             (double)trackEntropy.UniqueSectors / (double)trackEntropy.Sectors);
                }
            }

            if(!wholeDisc)
            {
                return (int)ErrorNumber.NoError;
            }

            EntropyResults entropy = entropyCalculator.CalculateMediaEntropy(duplicatedSectors);

            DicConsole.WriteLine("Entropy for disk is {0:F4}.", entropy.Entropy);
            if(entropy.UniqueSectors != null)
                DicConsole.WriteLine("Disk has {0} unique sectors ({1:P3})", entropy.UniqueSectors,
                                     (double)entropy.UniqueSectors / (double)entropy.Sectors);

            return (int)ErrorNumber.NoError;
        }
    }
}