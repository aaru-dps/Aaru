/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Checksum.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Verbs.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the 'checksum' verb.
 
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
using DiscImageChef.ImagePlugins;
using DiscImageChef.Checksums;
using System.Collections.Generic;
using DiscImageChef.Console;

namespace DiscImageChef.Commands
{
    public static class Entropy
    {
        public static void doEntropy(EntropySubOptions options)
        {
            DicConsole.DebugWriteLine("Entropy command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Entropy command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Entropy command", "--separated-tracks={0}", options.SeparatedTracks);
            DicConsole.DebugWriteLine("Entropy command", "--whole-disc={0}", options.WholeDisc);
            DicConsole.DebugWriteLine("Entropy command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Entropy command", "--duplicated-sectors={0}", options.DuplicatedSectors);

            ImagePlugin inputFormat = ImageFormat.Detect(options.InputFile);

            if (inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not checksumming");
                return;
            }

            inputFormat.OpenImage(options.InputFile);

            if (options.SeparatedTracks)
            {
                try
                {
                    List<Track> inputTracks = inputFormat.GetTracks();

                    foreach (Track currentTrack in inputTracks)
                    {
                        SHA1Context sha1ctxTrack = new SHA1Context();
                        ulong[] entTable = new ulong[256];
                        ulong trackSize = 0;
                        List<string> uniqueSectorsPerTrack = new List<string>();

                        ulong sectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        DicConsole.WriteLine("Track {0} has {1} sectors", currentTrack.TrackSequence, sectors);

                        for (ulong i = currentTrack.TrackStartSector; i <= currentTrack.TrackEndSector; i++)
                        {
                            DicConsole.Write("\rEntropying sector {0} of track {1}", i + 1, currentTrack.TrackSequence);
                            byte[] sector = inputFormat.ReadSector(i, currentTrack.TrackSequence);

                            if(options.DuplicatedSectors)
                            {
                                byte[] garbage;
                                string sectorHash = sha1ctxTrack.Data(sector, out garbage);
                                if(!uniqueSectorsPerTrack.Contains(sectorHash))
                                    uniqueSectorsPerTrack.Add(sectorHash);
                            }

                            foreach(byte b in sector)
                                entTable[b]++;

                            trackSize += (ulong)sector.LongLength;
                        }

                        double entropy = 0;
                        foreach(ulong l in entTable)
                        {
                            double frequency = (double)l/(double)trackSize;
                            entropy += -(frequency * Math.Log(frequency, 2));
                        }
                        
                        DicConsole.WriteLine("Entropy for track {0} is {1:F4}.", currentTrack.TrackSequence, entropy);

                        if(options.DuplicatedSectors)
                            DicConsole.WriteLine("Track {0} has {1} unique sectors ({1:P3})", currentTrack.TrackSequence, uniqueSectorsPerTrack.Count, (double)uniqueSectorsPerTrack.Count/(double)sectors);

                        DicConsole.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    if (options.Debug)
                        DicConsole.DebugWriteLine("Could not get tracks because {0}", ex.Message);
                    else
                        DicConsole.ErrorWriteLine("Unable to get separate tracks, not calculating their entropy");
                }
            }


            if (options.WholeDisc)
            {
                SHA1Context sha1Ctx = new SHA1Context();
                ulong[] entTable = new ulong[256];
                ulong diskSize = 0;
                List<string> uniqueSectors = new List<string>();

                ulong sectors = inputFormat.GetSectors();
                DicConsole.WriteLine("Sectors {0}", sectors);

                sha1Ctx.Init();

                for (ulong i = 0; i < sectors; i++)
                {
                    DicConsole.Write("\rEntropying sector {0}", i + 1);
                    byte[] sector = inputFormat.ReadSector(i);

                    if(options.DuplicatedSectors)
                    {
                        byte[] garbage;
                        string sectorHash = sha1Ctx.Data(sector, out garbage);
                        if(!uniqueSectors.Contains(sectorHash))
                            uniqueSectors.Add(sectorHash);
                    }

                    foreach(byte b in sector)
                        entTable[b]++;

                    diskSize += (ulong)sector.LongLength;

                }

                double entropy = 0;
                foreach(ulong l in entTable)
                {
                    double frequency = (double)l/(double)diskSize;
                    entropy += -(frequency * Math.Log(frequency, 2));
                }

                DicConsole.WriteLine();

                DicConsole.WriteLine("Entropy for disk is {0:F4}.", entropy);

                if(options.DuplicatedSectors)
                    DicConsole.WriteLine("Disk has {0} unique sectors ({1:P3})", uniqueSectors.Count, (double)uniqueSectors.Count/(double)sectors);


            }
        }
    }
}

