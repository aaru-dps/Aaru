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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    static class Entropy
    {
        internal static void DoEntropy(EntropyOptions options)
        {
            DicConsole.DebugWriteLine("Entropy command", "--debug={0}",              options.Debug);
            DicConsole.DebugWriteLine("Entropy command", "--verbose={0}",            options.Verbose);
            DicConsole.DebugWriteLine("Entropy command", "--separated-tracks={0}",   options.SeparatedTracks);
            DicConsole.DebugWriteLine("Entropy command", "--whole-disc={0}",         options.WholeDisc);
            DicConsole.DebugWriteLine("Entropy command", "--input={0}",              options.InputFile);
            DicConsole.DebugWriteLine("Entropy command", "--duplicated-sectors={0}", options.DuplicatedSectors);

            FiltersList filtersList = new FiltersList();
            IFilter     inputFilter = filtersList.GetFilter(options.InputFile);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not checksumming");
                return;
            }

            inputFormat.Open(inputFilter);
            Core.Statistics.AddMediaFormat(inputFormat.Format);
            Core.Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Core.Statistics.AddFilter(inputFilter.Name);

            Core.Entropy entropyCalculator = new Core.Entropy(options.Debug, options.Verbose, inputFormat);
            entropyCalculator.InitProgressEvent    += Progress.InitProgress;
            entropyCalculator.InitProgress2Event   += Progress.InitProgress2;
            entropyCalculator.UpdateProgressEvent  += Progress.UpdateProgress;
            entropyCalculator.UpdateProgress2Event += Progress.UpdateProgress2;
            entropyCalculator.EndProgressEvent     += Progress.EndProgress;
            entropyCalculator.EndProgress2Event    += Progress.EndProgress2;

            if(options.SeparatedTracks)
            {
                EntropyResults[] tracksEntropy = entropyCalculator.CalculateTracksEntropy(options.DuplicatedSectors);
                foreach(EntropyResults trackEntropy in tracksEntropy)
                {
                    DicConsole.WriteLine("Entropy for track {0} is {1:F4}.", trackEntropy.Track, trackEntropy.Entropy);
                    if(trackEntropy.UniqueSectors != null)
                        DicConsole.WriteLine("Track {0} has {1} unique sectors ({2:P3})", trackEntropy.Track,
                                             trackEntropy.UniqueSectors,
                                             (double)trackEntropy.UniqueSectors / (double)trackEntropy.Sectors);
                }
            }

            if(!options.WholeDisc)
            {
                Core.Statistics.AddCommand("entropy");
                return;
            }

            EntropyResults entropy = entropyCalculator.CalculateMediaEntropy(options.DuplicatedSectors);

            DicConsole.WriteLine("Entropy for disk is {0:F4}.", entropy.Entropy);
            if(entropy.UniqueSectors != null)
                DicConsole.WriteLine("Disk has {0} unique sectors ({1:P3})", entropy.UniqueSectors,
                                     (double)entropy.UniqueSectors / (double)entropy.Sectors);

            Core.Statistics.AddCommand("entropy");
        }
    }
}