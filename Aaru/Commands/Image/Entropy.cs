// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Entropy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'entropy' command.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.CommandLine;
using System.CommandLine.Invocation;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;

namespace Aaru.Commands.Image
{
    internal sealed class EntropyCommand : Command
    {
        public EntropyCommand() : base("entropy", "Calculates entropy and/or duplicated sectors of an image.")
        {
            Add(new Option(new[]
                {
                    "--duplicated-sectors", "-p"
                }, "Calculates how many sectors are duplicated (have same exact data in user area).")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--separated-tracks", "-t"
                }, "Calculates entropy for each track separately.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--whole-disc", "-w"
                }, "Calculates entropy for the whole disc.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Media image path",
                Name        = "image-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, bool duplicatedSectors, string imagePath,
                                 bool separatedTracks, bool wholeDisc)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("entropy");

            AaruConsole.DebugWriteLine("Entropy command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Entropy command", "--duplicated-sectors={0}", duplicatedSectors);
            AaruConsole.DebugWriteLine("Entropy command", "--input={0}", imagePath);
            AaruConsole.DebugWriteLine("Entropy command", "--separated-tracks={0}", separatedTracks);
            AaruConsole.DebugWriteLine("Entropy command", "--verbose={0}", verbose);
            AaruConsole.DebugWriteLine("Entropy command", "--whole-disc={0}", wholeDisc);

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            if(inputFilter == null)
            {
                AaruConsole.ErrorWriteLine("Cannot open specified file.");

                return (int)ErrorNumber.CannotOpenFile;
            }

            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                AaruConsole.ErrorWriteLine("Unable to recognize image format, not checksumming");

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            inputFormat.Open(inputFilter);
            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Statistics.AddFilter(inputFilter.Name);

            var entropyCalculator = new Entropy(debug, inputFormat);
            entropyCalculator.InitProgressEvent    += Progress.InitProgress;
            entropyCalculator.InitProgress2Event   += Progress.InitProgress2;
            entropyCalculator.UpdateProgressEvent  += Progress.UpdateProgress;
            entropyCalculator.UpdateProgress2Event += Progress.UpdateProgress2;
            entropyCalculator.EndProgressEvent     += Progress.EndProgress;
            entropyCalculator.EndProgress2Event    += Progress.EndProgress2;

            if(wholeDisc                                       &&
               inputFormat is IOpticalMediaImage opticalFormat &&
               opticalFormat.Sessions?.Count > 1)
            {
                AaruConsole.ErrorWriteLine("Calculating disc entropy of multisession images is not yet implemented.");
                wholeDisc = false;
            }

            if(separatedTracks)
            {
                EntropyResults[] tracksEntropy = entropyCalculator.CalculateTracksEntropy(duplicatedSectors);

                foreach(EntropyResults trackEntropy in tracksEntropy)
                {
                    AaruConsole.WriteLine("Entropy for track {0} is {1:F4}.", trackEntropy.Track, trackEntropy.Entropy);

                    if(trackEntropy.UniqueSectors != null)
                        AaruConsole.WriteLine("Track {0} has {1} unique sectors ({2:P3})", trackEntropy.Track,
                                              trackEntropy.UniqueSectors,
                                              (double)trackEntropy.UniqueSectors / trackEntropy.Sectors);
                }
            }

            if(!wholeDisc)
                return (int)ErrorNumber.NoError;

            EntropyResults entropy = entropyCalculator.CalculateMediaEntropy(duplicatedSectors);

            AaruConsole.WriteLine("Entropy for disk is {0:F4}.", entropy.Entropy);

            if(entropy.UniqueSectors != null)
                AaruConsole.WriteLine("Disk has {0} unique sectors ({1:P3})", entropy.UniqueSectors,
                                      (double)entropy.UniqueSectors / entropy.Sectors);

            return (int)ErrorNumber.NoError;
        }
    }
}