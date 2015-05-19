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

namespace DiscImageChef.Commands
{
    public static class Entropy
    {
        public static void doEntropy(EntropySubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--separated-tracks={0}", options.SeparatedTracks);
                Console.WriteLine("--whole-disc={0}", options.WholeDisc);
                Console.WriteLine("--input={0}", options.InputFile);
                Console.WriteLine("--duplicated-sectors={0}", options.DuplicatedSectors);
            }

            ImagePlugin inputFormat = ImageFormat.Detect(options.InputFile);

            if (inputFormat == null)
            {
                Console.WriteLine("Unable to recognize image format, not checksumming");
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
                        Console.WriteLine("Track {0} has {1} sectors", currentTrack.TrackSequence, sectors);

                        for (ulong i = currentTrack.TrackStartSector; i <= currentTrack.TrackEndSector; i++)
                        {
                            Console.Write("\rEntropying sector {0} of track {1}", i + 1, currentTrack.TrackSequence);
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
                        
                        Console.WriteLine("Entropy for track {0} is {1:F4}.", currentTrack.TrackSequence, entropy);

                        if(options.DuplicatedSectors)
                            Console.WriteLine("Track {0} has {1} unique sectors ({1:P3})", currentTrack.TrackSequence, uniqueSectorsPerTrack.Count, (double)uniqueSectorsPerTrack.Count/(double)sectors);

                        Console.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    if (options.Debug)
                        Console.WriteLine("Could not get tracks because {0}", ex.Message);
                    else
                        Console.WriteLine("Unable to get separate tracks, not calculating their entropy");
                }
            }


            if (options.WholeDisc)
            {
                SHA1Context sha1Ctx = new SHA1Context();
                ulong[] entTable = new ulong[256];
                ulong diskSize = 0;
                List<string> uniqueSectors = new List<string>();

                ulong sectors = inputFormat.GetSectors();
                Console.WriteLine("Sectors {0}", sectors);

                sha1Ctx.Init();

                for (ulong i = 0; i < sectors; i++)
                {
                    Console.Write("\rEntropying sector {0}", i + 1);
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

                Console.WriteLine();

                Console.WriteLine("Entropy for disk is {0:F4}.", entropy);

                if(options.DuplicatedSectors)
                    Console.WriteLine("Disk has {0} unique sectors ({1:P3})", uniqueSectors.Count, (double)uniqueSectors.Count/(double)sectors);


            }
        }
    }
}

