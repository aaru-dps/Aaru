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

using System;
using System.Collections.Generic;
using System.Linq;
using DiscImageChef.Checksums;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;

namespace DiscImageChef.Commands
{
    static class Entropy
    {
        internal static void DoEntropy(EntropyOptions options)
        {
            DicConsole.DebugWriteLine("Entropy command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Entropy command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Entropy command", "--separated-tracks={0}", options.SeparatedTracks);
            DicConsole.DebugWriteLine("Entropy command", "--whole-disc={0}", options.WholeDisc);
            DicConsole.DebugWriteLine("Entropy command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Entropy command", "--duplicated-sectors={0}", options.DuplicatedSectors);

            FiltersList filtersList = new FiltersList();
            Filter inputFilter = filtersList.GetFilter(options.InputFile);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            ImagePlugin inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not checksumming");
                return;
            }

            inputFormat.OpenImage(inputFilter);
            Core.Statistics.AddMediaFormat(inputFormat.GetImageFormat());
            Core.Statistics.AddMedia(inputFormat.ImageInfo.MediaType, false);
            Core.Statistics.AddFilter(inputFilter.Name);
            double entropy = 0;
            ulong[] entTable;
            ulong sectors;

            if(options.SeparatedTracks)
                try
                {
                    List<Track> inputTracks = inputFormat.GetTracks();

                    foreach(Track currentTrack in inputTracks)
                    {
                        Sha1Context sha1CtxTrack = new Sha1Context();
                        entTable = new ulong[256];
                        ulong trackSize = 0;
                        List<string> uniqueSectorsPerTrack = new List<string>();

                        sectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        DicConsole.WriteLine("Track {0} has {1} sectors", currentTrack.TrackSequence, sectors);

                        for(ulong i = currentTrack.TrackStartSector; i <= currentTrack.TrackEndSector; i++)
                        {
                            DicConsole.Write("\rEntropying sector {0} of track {1}", i + 1, currentTrack.TrackSequence);
                            byte[] sector = inputFormat.ReadSector(i, currentTrack.TrackSequence);

                            if(options.DuplicatedSectors)
                            {
                                string sectorHash = sha1CtxTrack.Data(sector, out _);
                                if(!uniqueSectorsPerTrack.Contains(sectorHash)) uniqueSectorsPerTrack.Add(sectorHash);
                            }

                            foreach(byte b in sector) entTable[b]++;

                            trackSize += (ulong)sector.LongLength;
                        }

                        entropy += entTable.Select(l => (double)l / (double)trackSize)
                                           .Select(frequency => -(frequency * Math.Log(frequency, 2))).Sum();

                        DicConsole.WriteLine("Entropy for track {0} is {1:F4}.", currentTrack.TrackSequence, entropy);

                        if(options.DuplicatedSectors)
                            DicConsole.WriteLine("Track {0} has {1} unique sectors ({1:P3})",
                                                 currentTrack.TrackSequence, uniqueSectorsPerTrack.Count,
                                                 (double)uniqueSectorsPerTrack.Count / (double)sectors);

                        DicConsole.WriteLine();
                    }
                }
                catch(Exception ex)
                {
                    if(options.Debug) DicConsole.DebugWriteLine("Could not get tracks because {0}", ex.Message);
                    else DicConsole.ErrorWriteLine("Unable to get separate tracks, not calculating their entropy");
                }

            if(!options.WholeDisc) return;

            Sha1Context sha1Ctx = new Sha1Context();
            entTable = new ulong[256];
            ulong diskSize = 0;
            List<string> uniqueSectors = new List<string>();

            sectors = inputFormat.GetSectors();
            DicConsole.WriteLine("Sectors {0}", sectors);

            sha1Ctx.Init();

            for(ulong i = 0; i < sectors; i++)
            {
                DicConsole.Write("\rEntropying sector {0}", i + 1);
                byte[] sector = inputFormat.ReadSector(i);

                if(options.DuplicatedSectors)
                {
                    string sectorHash = sha1Ctx.Data(sector, out _);
                    if(!uniqueSectors.Contains(sectorHash)) uniqueSectors.Add(sectorHash);
                }

                foreach(byte b in sector) entTable[b]++;

                diskSize += (ulong)sector.LongLength;
            }

            entropy += entTable.Select(l => (double)l / (double)diskSize)
                               .Select(frequency => -(frequency * Math.Log(frequency, 2))).Sum();

            DicConsole.WriteLine();

            DicConsole.WriteLine("Entropy for disk is {0:F4}.", entropy);

            if(options.DuplicatedSectors)
                DicConsole.WriteLine("Disk has {0} unique sectors ({1:P3})", uniqueSectors.Count,
                                     (double)uniqueSectors.Count / (double)sectors);

            Core.Statistics.AddCommand("entropy");
        }
    }
}