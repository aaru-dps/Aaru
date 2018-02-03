// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Checksum.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'checksum' verb.
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
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;
using Schemas;

namespace DiscImageChef.Commands
{
    static class Checksum
    {
        // How many sectors to read at once
        const uint SECTORS_TO_READ = 256;

        internal static void DoChecksum(ChecksumOptions options)
        {
            DicConsole.DebugWriteLine("Checksum command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Checksum command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Checksum command", "--separated-tracks={0}", options.SeparatedTracks);
            DicConsole.DebugWriteLine("Checksum command", "--whole-disc={0}", options.WholeDisc);
            DicConsole.DebugWriteLine("Checksum command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Checksum command", "--adler32={0}", options.DoAdler32);
            DicConsole.DebugWriteLine("Checksum command", "--crc16={0}", options.DoCrc16);
            DicConsole.DebugWriteLine("Checksum command", "--crc32={0}", options.DoCrc32);
            DicConsole.DebugWriteLine("Checksum command", "--crc64={0}", options.DoCrc64);
            DicConsole.DebugWriteLine("Checksum command", "--md5={0}", options.DoMd5);
            DicConsole.DebugWriteLine("Checksum command", "--ripemd160={0}", options.DoRipemd160);
            DicConsole.DebugWriteLine("Checksum command", "--sha1={0}", options.DoSha1);
            DicConsole.DebugWriteLine("Checksum command", "--sha256={0}", options.DoSha256);
            DicConsole.DebugWriteLine("Checksum command", "--sha384={0}", options.DoSha384);
            DicConsole.DebugWriteLine("Checksum command", "--sha512={0}", options.DoSha512);
            DicConsole.DebugWriteLine("Checksum command", "--spamsum={0}", options.DoSpamSum);
            DicConsole.DebugWriteLine("Checksum command", "--fletcher16={0}", options.DoFletcher16);
            DicConsole.DebugWriteLine("Checksum command", "--fletcher32={0}", options.DoFletcher32);

            FiltersList filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(options.InputFile);

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
            EnableChecksum enabledChecksums = new EnableChecksum();

            if(options.DoAdler32) enabledChecksums |= EnableChecksum.Adler32;
            if(options.DoCrc16) enabledChecksums |= EnableChecksum.Crc16;
            if(options.DoCrc32) enabledChecksums |= EnableChecksum.Crc32;
            if(options.DoCrc64) enabledChecksums |= EnableChecksum.Crc64;
            if(options.DoMd5) enabledChecksums |= EnableChecksum.Md5;
            if(options.DoRipemd160) enabledChecksums |= EnableChecksum.Ripemd160;
            if(options.DoSha1) enabledChecksums |= EnableChecksum.Sha1;
            if(options.DoSha256) enabledChecksums |= EnableChecksum.Sha256;
            if(options.DoSha384) enabledChecksums |= EnableChecksum.Sha384;
            if(options.DoSha512) enabledChecksums |= EnableChecksum.Sha512;
            if(options.DoSpamSum) enabledChecksums |= EnableChecksum.SpamSum;
            if(options.DoFletcher16) enabledChecksums |= EnableChecksum.Fletcher16;
            if(options.DoFletcher32) enabledChecksums |= EnableChecksum.Fletcher32;

            Core.Checksum mediaChecksum = null;

            if(inputFormat.Info.HasPartitions)
                try
                {
                    Core.Checksum trackChecksum = null;

                    if(options.WholeDisc) mediaChecksum = new Core.Checksum(enabledChecksums);

                    ulong previousTrackEnd = 0;

                    List<Track> inputTracks = inputFormat.Tracks;
                    foreach(Track currentTrack in inputTracks)
                    {
                        if(currentTrack.TrackStartSector - previousTrackEnd != 0 && options.WholeDisc)
                            for(ulong i = previousTrackEnd + 1; i < currentTrack.TrackStartSector; i++)
                            {
                                DicConsole.Write("\rHashing track-less sector {0}", i);

                                byte[] hiddenSector = inputFormat.ReadSector(i);

                                mediaChecksum?.Update(hiddenSector);
                            }

                        DicConsole.DebugWriteLine("Checksum command",
                                                  "Track {0} starts at sector {1} and ends at sector {2}",
                                                  currentTrack.TrackSequence, currentTrack.TrackStartSector,
                                                  currentTrack.TrackEndSector);

                        if(options.SeparatedTracks) trackChecksum = new Core.Checksum(enabledChecksums);

                        ulong sectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;
                        DicConsole.WriteLine("Track {0} has {1} sectors", currentTrack.TrackSequence, sectors);

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= SECTORS_TO_READ)
                            {
                                sector = inputFormat.ReadSectors(doneSectors, SECTORS_TO_READ,
                                                                 currentTrack.TrackSequence);
                                DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors,
                                                 currentTrack.TrackSequence, doneSectors + SECTORS_TO_READ);
                                doneSectors += SECTORS_TO_READ;
                            }
                            else
                            {
                                sector = inputFormat.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                 currentTrack.TrackSequence);
                                DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors,
                                                 currentTrack.TrackSequence, doneSectors + (sectors - doneSectors));
                                doneSectors += sectors - doneSectors;
                            }

                            if(options.WholeDisc) mediaChecksum?.Update(sector);

                            if(options.SeparatedTracks) trackChecksum?.Update(sector);
                        }

                        DicConsole.WriteLine();

                        if(options.SeparatedTracks)
                            if(trackChecksum != null)
                                foreach(ChecksumType chk in trackChecksum.End())
                                    DicConsole.WriteLine("Track {0}'s {1}: {2}", currentTrack.TrackSequence, chk.type,
                                                         chk.Value);

                        previousTrackEnd = currentTrack.TrackEndSector;
                    }

                    if(inputFormat.Info.Sectors - previousTrackEnd != 0 && options.WholeDisc)
                        for(ulong i = previousTrackEnd + 1; i < inputFormat.Info.Sectors; i++)
                        {
                            DicConsole.Write("\rHashing track-less sector {0}", i);

                            byte[] hiddenSector = inputFormat.ReadSector(i);
                            mediaChecksum?.Update(hiddenSector);
                        }

                    if(options.WholeDisc)
                        if(mediaChecksum != null)
                            foreach(ChecksumType chk in mediaChecksum.End())
                                DicConsole.WriteLine("Disk's {0}: {1}", chk.type, chk.Value);
                }
                catch(Exception ex)
                {
                    if(options.Debug) DicConsole.DebugWriteLine("Could not get tracks because {0}", ex.Message);
                    else DicConsole.WriteLine("Unable to get separate tracks, not checksumming them");
                }
            else
            {
                mediaChecksum = new Core.Checksum(enabledChecksums);

                ulong sectors = inputFormat.Info.Sectors;
                DicConsole.WriteLine("Sectors {0}", sectors);
                ulong doneSectors = 0;

                while(doneSectors < sectors)
                {
                    byte[] sector;

                    if(sectors - doneSectors >= SECTORS_TO_READ)
                    {
                        sector = inputFormat.ReadSectors(doneSectors, SECTORS_TO_READ);
                        DicConsole.Write("\rHashings sectors {0} to {1}", doneSectors, doneSectors + SECTORS_TO_READ);
                        doneSectors += SECTORS_TO_READ;
                    }
                    else
                    {
                        sector = inputFormat.ReadSectors(doneSectors, (uint)(sectors - doneSectors));
                        DicConsole.Write("\rHashings sectors {0} to {1}", doneSectors,
                                         doneSectors + (sectors - doneSectors));
                        doneSectors += sectors - doneSectors;
                    }

                    mediaChecksum.Update(sector);
                }

                DicConsole.WriteLine();

                foreach(ChecksumType chk in mediaChecksum.End())
                    DicConsole.WriteLine("Disk's {0}: {1}", chk.type, chk.Value);
            }

            Core.Statistics.AddCommand("checksum");
        }
    }
}