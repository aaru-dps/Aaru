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
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using Schemas;

namespace DiscImageChef.Commands
{
    public static class Checksum
    {
        // How many sectors to read at once
        const uint sectorsToRead = 256;

        public static void doChecksum(ChecksumOptions options)
        {
            DicConsole.DebugWriteLine("Checksum command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Checksum command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Checksum command", "--separated-tracks={0}", options.SeparatedTracks);
            DicConsole.DebugWriteLine("Checksum command", "--whole-disc={0}", options.WholeDisc);
            DicConsole.DebugWriteLine("Checksum command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Checksum command", "--adler32={0}", options.DoAdler32);
            DicConsole.DebugWriteLine("Checksum command", "--crc16={0}", options.DoCRC16);
            DicConsole.DebugWriteLine("Checksum command", "--crc32={0}", options.DoCRC32);
            DicConsole.DebugWriteLine("Checksum command", "--crc64={0}", options.DoCRC64);
            DicConsole.DebugWriteLine("Checksum command", "--md5={0}", options.DoMD5);
            DicConsole.DebugWriteLine("Checksum command", "--ripemd160={0}", options.DoRIPEMD160);
            DicConsole.DebugWriteLine("Checksum command", "--sha1={0}", options.DoSHA1);
            DicConsole.DebugWriteLine("Checksum command", "--sha256={0}", options.DoSHA256);
            DicConsole.DebugWriteLine("Checksum command", "--sha384={0}", options.DoSHA384);
            DicConsole.DebugWriteLine("Checksum command", "--sha512={0}", options.DoSHA512);
            DicConsole.DebugWriteLine("Checksum command", "--spamsum={0}", options.DoSpamSum);

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
            Core.Statistics.AddMedia(inputFormat.ImageInfo.mediaType, false);
            Core.Statistics.AddFilter(inputFilter.Name);
            EnableChecksum enabledChecksums = new EnableChecksum();

            if(options.DoAdler32) enabledChecksums |= EnableChecksum.Adler32;
            if(options.DoCRC16) enabledChecksums |= EnableChecksum.CRC16;
            if(options.DoCRC32) enabledChecksums |= EnableChecksum.CRC32;
            if(options.DoCRC64) enabledChecksums |= EnableChecksum.CRC64;
            if(options.DoMD5) enabledChecksums |= EnableChecksum.MD5;
            if(options.DoRIPEMD160) enabledChecksums |= EnableChecksum.RIPEMD160;
            if(options.DoSHA1) enabledChecksums |= EnableChecksum.SHA1;
            if(options.DoSHA256) enabledChecksums |= EnableChecksum.SHA256;
            if(options.DoSHA384) enabledChecksums |= EnableChecksum.SHA384;
            if(options.DoSHA512) enabledChecksums |= EnableChecksum.SHA512;
            if(options.DoSpamSum) enabledChecksums |= EnableChecksum.SpamSum;

            Core.Checksum mediaChecksum = null;

            if(inputFormat.ImageInfo.imageHasPartitions)
            {
                try
                {
                    Core.Checksum trackChecksum = null;

                    if(options.WholeDisc) mediaChecksum = new Core.Checksum(enabledChecksums);

                    ulong previousTrackEnd = 0;

                    List<Track> inputTracks = inputFormat.GetTracks();
                    foreach(Track currentTrack in inputTracks)
                    {
                        if((currentTrack.TrackStartSector - previousTrackEnd) != 0 && options.WholeDisc)
                        {
                            for(ulong i = previousTrackEnd + 1; i < currentTrack.TrackStartSector; i++)
                            {
                                DicConsole.Write("\rHashing track-less sector {0}", i);

                                byte[] hiddenSector = inputFormat.ReadSector(i);

                                mediaChecksum.Update(hiddenSector);
                            }
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

                            if((sectors - doneSectors) >= sectorsToRead)
                            {
                                sector = inputFormat.ReadSectors(doneSectors, sectorsToRead,
                                                                 currentTrack.TrackSequence);
                                DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors,
                                                 currentTrack.TrackSequence, doneSectors + sectorsToRead);
                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                sector = inputFormat.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                 currentTrack.TrackSequence);
                                DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors,
                                                 currentTrack.TrackSequence, doneSectors + (sectors - doneSectors));
                                doneSectors += (sectors - doneSectors);
                            }

                            if(options.WholeDisc) mediaChecksum.Update(sector);

                            if(options.SeparatedTracks) trackChecksum.Update(sector);
                        }

                        DicConsole.WriteLine();

                        if(options.SeparatedTracks)
                        {
                            foreach(ChecksumType chk in trackChecksum.End())
                                DicConsole.WriteLine("Track {0}'s {1}: {2}", currentTrack.TrackSequence, chk.type,
                                                     chk.Value);
                        }

                        previousTrackEnd = currentTrack.TrackEndSector;
                    }

                    if((inputFormat.GetSectors() - previousTrackEnd) != 0 && options.WholeDisc)
                    {
                        for(ulong i = previousTrackEnd + 1; i < inputFormat.GetSectors(); i++)
                        {
                            DicConsole.Write("\rHashing track-less sector {0}", i);

                            byte[] hiddenSector = inputFormat.ReadSector(i);
                            mediaChecksum.Update(hiddenSector);
                        }
                    }

                    if(options.WholeDisc)
                    {
                        foreach(ChecksumType chk in mediaChecksum.End())
                            DicConsole.WriteLine("Disk's {0}: {1}", chk.type, chk.Value);
                    }
                }
                catch(Exception ex)
                {
                    if(options.Debug) DicConsole.DebugWriteLine("Could not get tracks because {0}", ex.Message);
                    else DicConsole.WriteLine("Unable to get separate tracks, not checksumming them");
                }
            }
            else
            {
                mediaChecksum = new Core.Checksum(enabledChecksums);

                ulong sectors = inputFormat.GetSectors();
                DicConsole.WriteLine("Sectors {0}", sectors);
                ulong doneSectors = 0;

                while(doneSectors < sectors)
                {
                    byte[] sector;

                    if((sectors - doneSectors) >= sectorsToRead)
                    {
                        sector = inputFormat.ReadSectors(doneSectors, sectorsToRead);
                        DicConsole.Write("\rHashings sectors {0} to {1}", doneSectors, doneSectors + sectorsToRead);
                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        sector = inputFormat.ReadSectors(doneSectors, (uint)(sectors - doneSectors));
                        DicConsole.Write("\rHashings sectors {0} to {1}", doneSectors,
                                         doneSectors + (sectors - doneSectors));
                        doneSectors += (sectors - doneSectors);
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