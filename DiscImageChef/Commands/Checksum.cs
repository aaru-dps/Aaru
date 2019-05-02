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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Mono.Options;
using Schemas;

namespace DiscImageChef.Commands
{
    class ChecksumCommand : Command
    {
        // How many sectors to read at once
        const uint SECTORS_TO_READ = 256;

        bool   doAdler32 = true;
        bool   doCrc16   = true;
        bool   doCrc32   = true;
        bool   doCrc64;
        bool   doFletcher16;
        bool   doFletcher32;
        bool   doMd5 = true;
        bool   doRipemd160;
        bool   doSha1 = true;
        bool   doSha256;
        bool   doSha384;
        bool   doSha512;
        bool   doSpamSum = true;
        string inputFile;
        bool   separatedTracks = true;
        bool   showHelp;
        bool   wholeDisc = true;

        public ChecksumCommand() : base("checksum", "Checksums an image.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS] imagefile",
                "",
                Help,
                {"adler32|a", "Calculates Adler-32.", b => doAdler32                            = b != null},
                {"crc16", "Calculates CRC16.", b => doCrc16                                     = b != null},
                {"crc32|c", "Calculates CRC32.", b => doCrc32                                   = b != null},
                {"crc64", "Calculates CRC64 (ECMA).", b => doCrc64                              = b != null},
                {"fletcher16", "Calculates Fletcher-16.", b => doFletcher16                     = b != null},
                {"fletcher32", "Calculates Fletcher-32.", b => doFletcher32                     = b != null},
                {"md5|m", "Calculates MD5.", b => doMd5                                         = b != null},
                {"ripemd160", "Calculates RIPEMD160.", b => doRipemd160                         = b != null},
                {"separated-tracks|t", "Checksums each track separately.", b => separatedTracks = b != null},
                {"sha1|s", "Calculates SHA1.", b => doSha1                                      = b != null},
                {"sha256", "Calculates SHA256.", b => doSha256                                  = b != null},
                {"sha384", "Calculates SHA384.", b => doSha384                                  = b != null},
                {"sha512", "Calculates SHA512.", b => doSha512                                  = b != null},
                {"spamsum|f", "Calculates SpamSum fuzzy hash.", b => doSpamSum                  = b != null},
                {"whole-disc|w", "Checksums the whole disc.", b => wholeDisc                    = b != null},
                {"help|h|?", "Show this message and exit.", v => showHelp                       = v != null}
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
            Statistics.AddCommand("checksum");

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

            DicConsole.DebugWriteLine("Checksum command", "--adler32={0}",          doAdler32);
            DicConsole.DebugWriteLine("Checksum command", "--crc16={0}",            doCrc16);
            DicConsole.DebugWriteLine("Checksum command", "--crc32={0}",            doCrc32);
            DicConsole.DebugWriteLine("Checksum command", "--crc64={0}",            doCrc64);
            DicConsole.DebugWriteLine("Checksum command", "--debug={0}",            MainClass.Debug);
            DicConsole.DebugWriteLine("Checksum command", "--fletcher16={0}",       doFletcher16);
            DicConsole.DebugWriteLine("Checksum command", "--fletcher32={0}",       doFletcher32);
            DicConsole.DebugWriteLine("Checksum command", "--input={0}",            inputFile);
            DicConsole.DebugWriteLine("Checksum command", "--md5={0}",              doMd5);
            DicConsole.DebugWriteLine("Checksum command", "--ripemd160={0}",        doRipemd160);
            DicConsole.DebugWriteLine("Checksum command", "--separated-tracks={0}", separatedTracks);
            DicConsole.DebugWriteLine("Checksum command", "--sha1={0}",             doSha1);
            DicConsole.DebugWriteLine("Checksum command", "--sha256={0}",           doSha256);
            DicConsole.DebugWriteLine("Checksum command", "--sha384={0}",           doSha384);
            DicConsole.DebugWriteLine("Checksum command", "--sha512={0}",           doSha512);
            DicConsole.DebugWriteLine("Checksum command", "--spamsum={0}",          doSpamSum);
            DicConsole.DebugWriteLine("Checksum command", "--verbose={0}",          MainClass.Verbose);
            DicConsole.DebugWriteLine("Checksum command", "--whole-disc={0}",       wholeDisc);

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
            EnableChecksum enabledChecksums = new EnableChecksum();

            if(doAdler32) enabledChecksums    |= EnableChecksum.Adler32;
            if(doCrc16) enabledChecksums      |= EnableChecksum.Crc16;
            if(doCrc32) enabledChecksums      |= EnableChecksum.Crc32;
            if(doCrc64) enabledChecksums      |= EnableChecksum.Crc64;
            if(doMd5) enabledChecksums        |= EnableChecksum.Md5;
            if(doRipemd160) enabledChecksums  |= EnableChecksum.Ripemd160;
            if(doSha1) enabledChecksums       |= EnableChecksum.Sha1;
            if(doSha256) enabledChecksums     |= EnableChecksum.Sha256;
            if(doSha384) enabledChecksums     |= EnableChecksum.Sha384;
            if(doSha512) enabledChecksums     |= EnableChecksum.Sha512;
            if(doSpamSum) enabledChecksums    |= EnableChecksum.SpamSum;
            if(doFletcher16) enabledChecksums |= EnableChecksum.Fletcher16;
            if(doFletcher32) enabledChecksums |= EnableChecksum.Fletcher32;

            Checksum mediaChecksum = null;

            switch(inputFormat)
            {
                case IOpticalMediaImage opticalInput when opticalInput.Tracks != null:
                    try
                    {
                        Checksum trackChecksum = null;

                        if(wholeDisc) mediaChecksum = new Checksum(enabledChecksums);

                        ulong previousTrackEnd = 0;

                        List<Track> inputTracks = opticalInput.Tracks;
                        foreach(Track currentTrack in inputTracks)
                        {
                            if(currentTrack.TrackStartSector - previousTrackEnd != 0 && wholeDisc)
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

                            if(separatedTracks) trackChecksum = new Checksum(enabledChecksums);

                            ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                            ulong doneSectors = 0;
                            DicConsole.WriteLine("Track {0} has {1} sectors", currentTrack.TrackSequence, sectors);

                            while(doneSectors < sectors)
                            {
                                byte[] sector;

                                if(sectors - doneSectors >= SECTORS_TO_READ)
                                {
                                    sector = opticalInput.ReadSectors(doneSectors, SECTORS_TO_READ,
                                                                      currentTrack.TrackSequence);
                                    DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors,
                                                     currentTrack.TrackSequence, doneSectors + SECTORS_TO_READ);
                                    doneSectors += SECTORS_TO_READ;
                                }
                                else
                                {
                                    sector = opticalInput.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                      currentTrack.TrackSequence);
                                    DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors,
                                                     currentTrack.TrackSequence, doneSectors + (sectors - doneSectors));
                                    doneSectors += sectors - doneSectors;
                                }

                                if(wholeDisc) mediaChecksum?.Update(sector);

                                if(separatedTracks) trackChecksum?.Update(sector);
                            }

                            DicConsole.WriteLine();

                            if(separatedTracks)
                                if(trackChecksum != null)
                                    foreach(ChecksumType chk in trackChecksum.End())
                                        DicConsole.WriteLine("Track {0}'s {1}: {2}", currentTrack.TrackSequence,
                                                             chk.type, chk.Value);

                            previousTrackEnd = currentTrack.TrackEndSector;
                        }

                        if(opticalInput.Info.Sectors - previousTrackEnd != 0 && wholeDisc)
                            for(ulong i = previousTrackEnd + 1; i < opticalInput.Info.Sectors; i++)
                            {
                                DicConsole.Write("\rHashing track-less sector {0}", i);

                                byte[] hiddenSector = inputFormat.ReadSector(i);
                                mediaChecksum?.Update(hiddenSector);
                            }

                        if(wholeDisc)
                            if(mediaChecksum != null)
                                foreach(ChecksumType chk in mediaChecksum.End())
                                    DicConsole.WriteLine("Disk's {0}: {1}", chk.type, chk.Value);
                    }
                    catch(Exception ex)
                    {
                        if(MainClass.Debug) DicConsole.DebugWriteLine("Could not get tracks because {0}", ex.Message);
                        else DicConsole.WriteLine("Unable to get separate tracks, not checksumming them");
                    }

                    break;

                case ITapeImage tapeImage when tapeImage.IsTape && tapeImage.Files?.Count > 0:
                {
                    Checksum trackChecksum = null;

                    if(wholeDisc) mediaChecksum = new Checksum(enabledChecksums);

                    ulong previousTrackEnd = 0;

                    foreach(TapeFile currentFile in tapeImage.Files)
                    {
                        if(currentFile.FirstBlock - previousTrackEnd != 0 && wholeDisc)
                            for(ulong i = previousTrackEnd + 1; i < currentFile.FirstBlock; i++)
                            {
                                DicConsole.Write("\rHashing file-less block {0}", i);

                                byte[] hiddenSector = inputFormat.ReadSector(i);

                                mediaChecksum?.Update(hiddenSector);
                            }

                        DicConsole.DebugWriteLine("Checksum command",
                                                  "Track {0} starts at sector {1} and ends at block {2}",
                                                  currentFile.File, currentFile.FirstBlock, currentFile.LastBlock);

                        if(separatedTracks) trackChecksum = new Checksum(enabledChecksums);

                        ulong sectors     = currentFile.LastBlock - currentFile.FirstBlock + 1;
                        ulong doneSectors = 0;
                        DicConsole.WriteLine("File {0} has {1} sectors", currentFile.File, sectors);

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= SECTORS_TO_READ)
                            {
                                sector = tapeImage.ReadSectors(doneSectors + currentFile.FirstBlock, SECTORS_TO_READ);
                                DicConsole.Write("\rHashings blocks {0} to {2} of file {1}", doneSectors,
                                                 currentFile.File, doneSectors + SECTORS_TO_READ);
                                doneSectors += SECTORS_TO_READ;
                            }
                            else
                            {
                                sector = tapeImage.ReadSectors(doneSectors + currentFile.FirstBlock,
                                                               (uint)(sectors - doneSectors));
                                DicConsole.Write("\rHashings blocks {0} to {2} of file {1}", doneSectors,
                                                 currentFile.File, doneSectors + (sectors - doneSectors));
                                doneSectors += sectors - doneSectors;
                            }

                            if(wholeDisc) mediaChecksum?.Update(sector);

                            if(separatedTracks) trackChecksum?.Update(sector);
                        }

                        DicConsole.WriteLine();

                        if(separatedTracks)
                            if(trackChecksum != null)
                                foreach(ChecksumType chk in trackChecksum.End())
                                    DicConsole.WriteLine("File {0}'s {1}: {2}", currentFile.File, chk.type, chk.Value);

                        previousTrackEnd = currentFile.LastBlock;
                    }

                    if(tapeImage.Info.Sectors - previousTrackEnd != 0 && wholeDisc)
                        for(ulong i = previousTrackEnd + 1; i < tapeImage.Info.Sectors; i++)
                        {
                            DicConsole.Write("\rHashing file-less sector {0}", i);

                            byte[] hiddenSector = inputFormat.ReadSector(i);
                            mediaChecksum?.Update(hiddenSector);
                        }

                    if(wholeDisc)
                        if(mediaChecksum != null)
                            foreach(ChecksumType chk in mediaChecksum.End())
                                DicConsole.WriteLine("Tape's {0}: {1}", chk.type, chk.Value);
                    break;
                }

                default:
                {
                    mediaChecksum = new Checksum(enabledChecksums);

                    ulong sectors = inputFormat.Info.Sectors;
                    DicConsole.WriteLine("Sectors {0}", sectors);
                    ulong doneSectors = 0;

                    while(doneSectors < sectors)
                    {
                        byte[] sector;

                        if(sectors - doneSectors >= SECTORS_TO_READ)
                        {
                            sector = inputFormat.ReadSectors(doneSectors, SECTORS_TO_READ);
                            DicConsole.Write("\rHashings sectors {0} to {1}", doneSectors,
                                             doneSectors + SECTORS_TO_READ);
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
                    break;
                }
            }

            return (int)ErrorNumber.NoError;
        }
    }
}