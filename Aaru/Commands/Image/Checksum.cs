// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Checksum.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'checksum' command.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using Spectre.Console;

namespace Aaru.Commands.Image;

sealed class ChecksumCommand : Command
{
    // How many sectors to read at once
    const uint SECTORS_TO_READ = 256;

    // How many bytes to read at once
    const int    BYTES_TO_READ = 65536;
    const string MODULE_NAME   = "Checksum command";

    public ChecksumCommand() : base("checksum", UI.Image_Checksum_Command_Description)
    {
        AddAlias("chk");

        Add(new Option<bool>(new[] { "--adler32", "-a" }, () => false, UI.Calculates_Adler_32));

        Add(new Option<bool>("--crc16", () => true, UI.Calculates_CRC16));

        Add(new Option<bool>(new[] { "--crc32", "-c" }, () => true, UI.Calculates_CRC32));

        Add(new Option<bool>("--crc64",      () => true,  UI.Calculates_CRC64_ECMA));
        Add(new Option<bool>("--fletcher16", () => false, UI.Calculates_Fletcher_16));
        Add(new Option<bool>("--fletcher32", () => false, UI.Calculates_Fletcher_32));

        Add(new Option<bool>(new[] { "--md5", "-m" }, () => true, UI.Calculates_MD5));

        Add(new Option<bool>(new[] { "--separated-tracks", "-t" }, () => true, UI.Checksums_each_track_separately));

        Add(new Option<bool>(new[] { "--sha1", "-s" }, () => true, UI.Calculates_SHA1));

        Add(new Option<bool>("--sha256", () => false, UI.Calculates_SHA256));
        Add(new Option<bool>("--sha384", () => false, UI.Calculates_SHA384));
        Add(new Option<bool>("--sha512", () => true,  UI.Calculates_SHA512));

        Add(new Option<bool>(new[] { "--spamsum", "-f" }, () => true, UI.Calculates_SpamSum_fuzzy_hash));

        Add(new Option<bool>(new[] { "--whole-disc", "-w" }, () => true, UI.Checksums_the_whole_disc));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Media_image_path,
            Name        = "image-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());
    }

    public static int Invoke(bool debug,      bool verbose,    bool   adler32,   bool crc16, bool crc32, bool crc64,
                             bool fletcher16, bool fletcher32, bool   md5,       bool sha1, bool sha256, bool sha384,
                             bool sha512,     bool spamSum,    string imagePath, bool separatedTracks, bool wholeDisc)
    {
        MainClass.PrintCopyright();

        if(debug)
        {
            IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(System.Console.Error)
            });

            AaruConsole.DebugWriteLineEvent += (format, objects) =>
            {
                if(objects is null)
                    stderrConsole.MarkupLine(format);
                else
                    stderrConsole.MarkupLine(format, objects);
            };
        }

        if(verbose)
        {
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };
        }

        Statistics.AddCommand("checksum");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--adler32={0}",          adler32);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--crc16={0}",            crc16);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--crc32={0}",            crc32);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--crc64={0}",            crc64);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",            debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--fletcher16={0}",       fletcher16);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--fletcher32={0}",       fletcher32);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}",            imagePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--md5={0}",              md5);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--separated-tracks={0}", separatedTracks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--sha1={0}",             sha1);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--sha256={0}",           sha256);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--sha384={0}",           sha384);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--sha512={0}",           sha512);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--spamsum={0}",          spamSum);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}",          verbose);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--whole-disc={0}",       wholeDisc);

        var     filtersList = new FiltersList();
        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = filtersList.GetFilter(imagePath);
        });

        if(inputFilter == null)
        {
            AaruConsole.ErrorWriteLine(UI.Cannot_open_specified_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        IBaseImage inputFormat = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
            inputFormat = ImageFormat.Detect(inputFilter);
        });

        if(inputFormat == null)
        {
            AaruConsole.ErrorWriteLine(UI.Unable_to_recognize_image_format_not_checksumming);

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        ErrorNumber opened = ErrorNumber.NoData;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
            opened = inputFormat.Open(inputFilter);
        });

        if(opened != ErrorNumber.NoError)
        {
            AaruConsole.WriteLine(UI.Unable_to_open_image_format);
            AaruConsole.WriteLine(Localization.Core.Error_0, opened);

            return (int)opened;
        }

        Statistics.AddMediaFormat(inputFormat.Format);
        Statistics.AddMedia(inputFormat.Info.MediaType, false);
        Statistics.AddFilter(inputFilter.Name);
        var enabledChecksums = new EnableChecksum();

        if(adler32)
            enabledChecksums |= EnableChecksum.Adler32;

        if(crc16)
            enabledChecksums |= EnableChecksum.Crc16;

        if(crc32)
            enabledChecksums |= EnableChecksum.Crc32;

        if(crc64)
            enabledChecksums |= EnableChecksum.Crc64;

        if(md5)
            enabledChecksums |= EnableChecksum.Md5;

        if(sha1)
            enabledChecksums |= EnableChecksum.Sha1;

        if(sha256)
            enabledChecksums |= EnableChecksum.Sha256;

        if(sha384)
            enabledChecksums |= EnableChecksum.Sha384;

        if(sha512)
            enabledChecksums |= EnableChecksum.Sha512;

        if(spamSum)
            enabledChecksums |= EnableChecksum.SpamSum;

        if(fletcher16)
            enabledChecksums |= EnableChecksum.Fletcher16;

        if(fletcher32)
            enabledChecksums |= EnableChecksum.Fletcher32;

        Checksum mediaChecksum = null;

        ErrorNumber errno = ErrorNumber.NoError;

        switch(inputFormat)
        {
            case IOpticalMediaImage { Tracks: not null } opticalInput:
                try
                {
                    Checksum trackChecksum = null;

                    if(wholeDisc)
                        mediaChecksum = new Checksum(enabledChecksums);

                    List<Track> inputTracks = opticalInput.Tracks;

                    AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                                Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                                Start(ctx =>
                                {
                                    ProgressTask discTask = ctx.AddTask(Localization.Core.Hashing_tracks);
                                    discTask.MaxValue = inputTracks.Count;

                                    foreach(Track currentTrack in inputTracks)
                                    {
                                        discTask.Description =
                                            string.Format(UI.Hashing_track_0_of_1, discTask.Value + 1,
                                                          inputTracks.Count);

                                        ProgressTask trackTask = ctx.AddTask(UI.Hashing_sector);

                                        /*
                                        if(currentTrack.StartSector - previousTrackEnd != 0 && wholeDisc)
                                            for(ulong i = previousTrackEnd + 1; i < currentTrack.StartSector; i++)
                                            {
                                                AaruConsole.Write("\rHashing track-less sector {0}", i);

                                                byte[] hiddenSector = inputFormat.ReadSector(i);

                                                mediaChecksum?.Update(hiddenSector);
                                            }
                                        */

                                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                                   UI.
                                                                       Track_0_starts_at_sector_1_and_ends_at_sector_2,
                                                                   currentTrack.Sequence,
                                                                   currentTrack.StartSector,
                                                                   currentTrack.EndSector);

                                        if(separatedTracks)
                                            trackChecksum = new Checksum(enabledChecksums);

                                        ulong sectors = currentTrack.EndSector - currentTrack.StartSector + 1;

                                        trackTask.MaxValue = sectors;

                                        ulong doneSectors = 0;

                                        while(doneSectors < sectors)
                                        {
                                            byte[] sector;

                                            if(sectors - doneSectors >= SECTORS_TO_READ)
                                            {
                                                errno = opticalInput.ReadSectors(doneSectors, SECTORS_TO_READ,
                                                    currentTrack.Sequence, out sector);

                                                trackTask.Description =
                                                    string.Format(UI.Hashing_sectors_0_to_2_of_track_1,
                                                                  doneSectors,
                                                                  currentTrack.Sequence,
                                                                  doneSectors + SECTORS_TO_READ);

                                                if(errno != ErrorNumber.NoError)
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine(string.
                                                                           Format(
                                                                               UI.
                                                                                   Error_0_while_reading_1_sectors_from_sector_2,
                                                                               errno, SECTORS_TO_READ,
                                                                               doneSectors));

                                                    return;
                                                }

                                                doneSectors += SECTORS_TO_READ;
                                            }
                                            else
                                            {
                                                errno = opticalInput.ReadSectors(doneSectors,
                                                    (uint)(sectors - doneSectors), currentTrack.Sequence,
                                                    out sector);

                                                trackTask.Description =
                                                    string.Format(UI.Hashing_sectors_0_to_2_of_track_1,
                                                                  doneSectors,
                                                                  currentTrack.Sequence,
                                                                  doneSectors + (sectors - doneSectors));

                                                if(errno != ErrorNumber.NoError)
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine(string.
                                                                           Format(
                                                                               UI.
                                                                                   Error_0_while_reading_1_sectors_from_sector_2,
                                                                               errno, sectors - doneSectors,
                                                                               doneSectors));

                                                    return;
                                                }

                                                doneSectors += sectors - doneSectors;
                                            }

                                            if(wholeDisc)
                                                mediaChecksum?.Update(sector);

                                            if(separatedTracks)
                                                trackChecksum?.Update(sector);

                                            trackTask.Value = doneSectors;
                                        }

                                        trackTask.StopTask();
                                        AaruConsole.WriteLine();

                                        if(!separatedTracks)
                                            continue;

                                        if(trackChecksum == null)
                                            continue;

                                        foreach(CommonTypes.AaruMetadata.Checksum chk in trackChecksum.End())
                                        {
                                            AaruConsole.
                                                WriteLine($"[bold]{string.Format(UI.Checksums_Track_0_has_1,
                                                    currentTrack.Sequence, chk.Type)}[/] {chk.Value}");
                                        }

                                        discTask.Increment(1);
                                    }

                                    /*
                                    if(opticalInput.Info.Sectors - previousTrackEnd != 0 && wholeDisc)
                                        for(ulong i = previousTrackEnd + 1; i < opticalInput.Info.Sectors; i++)
                                        {
                                            AaruConsole.Write("\rHashing track-less sector {0}", i);

                                            byte[] hiddenSector = inputFormat.ReadSector(i);
                                            mediaChecksum?.Update(hiddenSector);
                                        }
                                    */

                                    if(!wholeDisc)
                                        return;

                                    if(mediaChecksum == null)
                                        return;

                                    AaruConsole.WriteLine();

                                    foreach(CommonTypes.AaruMetadata.Checksum chk in mediaChecksum.End())
                                    {
                                        AaruConsole.
                                            WriteLine($"[bold]{string.Format(UI.Checksums_Disc_has_0, chk.Type)
                                            }:[/] {chk.Value}");
                                    }
                                });

                    if(errno != ErrorNumber.NoError)
                        return (int)errno;
                }
                catch(Exception ex)
                {
                    if(debug)
                        AaruConsole.DebugWriteLine(Localization.Core.Could_not_get_tracks_because_0, ex.Message);
                    else
                        AaruConsole.WriteLine("Unable to get separate tracks, not checksumming them");
                }

                break;

            case ITapeImage { IsTape: true, Files.Count: > 0 } tapeImage:
            {
                Checksum trackChecksum = null;

                if(wholeDisc)
                    mediaChecksum = new Checksum(enabledChecksums);

                ulong previousFileEnd = 0;

                AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                            Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                            Start(ctx =>
                            {
                                ProgressTask tapeTask = ctx.AddTask(Localization.Core.Hashing_files);
                                tapeTask.MaxValue = tapeImage.Files.Count;

                                foreach(TapeFile currentFile in tapeImage.Files)
                                {
                                    tapeTask.Description =
                                        string.Format(UI.Hashing_file_0_of_1, currentFile.File,
                                                      tapeImage.Files.Count);

                                    if(currentFile.FirstBlock - previousFileEnd != 0 && wholeDisc)
                                    {
                                        ProgressTask preFileTask = ctx.AddTask(UI.Hashing_sector);
                                        preFileTask.MaxValue = currentFile.FirstBlock - previousFileEnd;

                                        for(ulong i = previousFileEnd + 1; i < currentFile.FirstBlock; i++)
                                        {
                                            preFileTask.Description =
                                                string.Format(UI.Hashing_file_less_block_0, i);

                                            errno = tapeImage.ReadSector(i, out byte[] hiddenSector);

                                            if(errno != ErrorNumber.NoError)
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine(string.Format(UI.Error_0_while_reading_block_1,
                                                                       errno, i));

                                                return;
                                            }

                                            mediaChecksum?.Update(hiddenSector);
                                            preFileTask.Increment(1);
                                        }

                                        preFileTask.StopTask();
                                    }

                                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                                               UI.File_0_starts_at_block_1_and_ends_at_block_2,
                                                               currentFile.File, currentFile.FirstBlock,
                                                               currentFile.LastBlock);

                                    if(separatedTracks)
                                        trackChecksum = new Checksum(enabledChecksums);

                                    ulong sectors     = currentFile.LastBlock - currentFile.FirstBlock + 1;
                                    ulong doneSectors = 0;

                                    ProgressTask fileTask = ctx.AddTask(UI.Hashing_sector);
                                    fileTask.MaxValue = sectors;

                                    while(doneSectors < sectors)
                                    {
                                        byte[] sector;

                                        if(sectors - doneSectors >= SECTORS_TO_READ)
                                        {
                                            errno = tapeImage.ReadSectors(doneSectors + currentFile.FirstBlock,
                                                                          SECTORS_TO_READ, out sector);

                                            if(errno != ErrorNumber.NoError)
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine(string.
                                                                       Format(
                                                                           UI.
                                                                               Error_0_while_reading_1_sectors_from_sector_2,
                                                                           errno, SECTORS_TO_READ,
                                                                           doneSectors +
                                                                           currentFile.FirstBlock));

                                                return;
                                            }

                                            fileTask.Description =
                                                string.Format(UI.Hashing_blocks_0_to_2_of_file_1, doneSectors,
                                                              currentFile.File, doneSectors + SECTORS_TO_READ);

                                            doneSectors += SECTORS_TO_READ;
                                        }
                                        else
                                        {
                                            errno = tapeImage.ReadSectors(doneSectors + currentFile.FirstBlock,
                                                                          (uint)(sectors - doneSectors),
                                                                          out sector);

                                            if(errno != ErrorNumber.NoError)
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine(string.
                                                                       Format(
                                                                           UI.
                                                                               Error_0_while_reading_1_sectors_from_sector_2,
                                                                           errno, sectors - doneSectors,
                                                                           doneSectors +
                                                                           currentFile.FirstBlock));

                                                return;
                                            }

                                            fileTask.Description =
                                                string.Format(UI.Hashing_blocks_0_to_2_of_file_1, doneSectors,
                                                              currentFile.File,
                                                              doneSectors + (sectors - doneSectors));

                                            doneSectors += sectors - doneSectors;
                                        }

                                        fileTask.Value = doneSectors;

                                        if(wholeDisc)
                                            mediaChecksum?.Update(sector);

                                        if(separatedTracks)
                                            trackChecksum?.Update(sector);
                                    }

                                    fileTask.StopTask();
                                    AaruConsole.WriteLine();

                                    if(separatedTracks)
                                    {
                                        if(trackChecksum != null)
                                        {
                                            foreach(CommonTypes.AaruMetadata.Checksum chk in trackChecksum.End())
                                            {
                                                AaruConsole.
                                                    WriteLine($"[bold]{string.Format(UI.Checksums_File_0_has_1,
                                                        currentFile.File, chk.Type)}[/]: {chk.Value}");
                                            }
                                        }
                                    }

                                    previousFileEnd = currentFile.LastBlock;

                                    tapeTask.Increment(1);
                                }

                                if(tapeImage.Info.Sectors - previousFileEnd == 0 ||
                                   !wholeDisc)
                                    return;

                                ProgressTask postFileTask = ctx.AddTask(UI.Hashing_sector);
                                postFileTask.MaxValue = tapeImage.Info.Sectors - previousFileEnd;

                                for(ulong i = previousFileEnd + 1; i < tapeImage.Info.Sectors; i++)
                                {
                                    postFileTask.Description = string.Format(UI.Hashing_file_less_block_0, i);

                                    errno = tapeImage.ReadSector(i, out byte[] hiddenSector);

                                    if(errno != ErrorNumber.NoError)
                                    {
                                        AaruConsole.ErrorWriteLine(string.Format(UI.Error_0_while_reading_block_1,
                                                                       errno, i));

                                        return;
                                    }

                                    mediaChecksum?.Update(hiddenSector);
                                    postFileTask.Increment(1);
                                }
                            });

                if(errno != ErrorNumber.NoError)
                    return (int)errno;

                if(wholeDisc && mediaChecksum != null)
                {
                    AaruConsole.WriteLine();

                    foreach(CommonTypes.AaruMetadata.Checksum chk in mediaChecksum.End())
                    {
                        AaruConsole.WriteLine($"[bold]{string.Format(UI.Checksums_Tape_has_0, chk.Type)}[/] {chk.Value
                        }");
                    }
                }

                break;
            }

            case IByteAddressableImage { Info.MetadataMediaType: MetadataMediaType.LinearMedia } byteAddressableImage:
            {
                mediaChecksum = new Checksum(enabledChecksums);

                AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                            Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                            Start(ctx =>
                            {
                                ProgressTask imageTask = ctx.AddTask(UI.Hashing_image);
                                ulong        length    = byteAddressableImage.Info.Sectors;
                                imageTask.MaxValue = length;
                                ulong doneBytes = 0;
                                var   data      = new byte[BYTES_TO_READ];

                                while(doneBytes < length)
                                {
                                    int bytesRead;

                                    if(length - doneBytes >= BYTES_TO_READ)
                                    {
                                        errno = byteAddressableImage.ReadBytes(data, 0, BYTES_TO_READ,
                                                                               out bytesRead);

                                        if(errno != ErrorNumber.NoError)
                                        {
                                            AaruConsole.
                                                ErrorWriteLine(string.
                                                                   Format(UI.Error_0_while_reading_1_bytes_from_2,
                                                                          errno, BYTES_TO_READ, doneBytes));

                                            return;
                                        }

                                        imageTask.Description =
                                            string.Format(UI.Hashing_bytes_0_to_1, doneBytes,
                                                          doneBytes + BYTES_TO_READ);

                                        doneBytes += (ulong)bytesRead;

                                        if(bytesRead == 0)
                                            break;
                                    }
                                    else
                                    {
                                        errno = byteAddressableImage.ReadBytes(data, 0, (int)(length - doneBytes),
                                                                               out bytesRead);

                                        if(errno != ErrorNumber.NoError)
                                        {
                                            AaruConsole.
                                                ErrorWriteLine(string.
                                                                   Format(UI.Error_0_while_reading_1_bytes_from_2,
                                                                          errno, length - doneBytes,
                                                                          doneBytes));

                                            return;
                                        }

                                        imageTask.Description =
                                            string.Format(UI.Hashing_bytes_0_to_1, doneBytes,
                                                          doneBytes + (length - doneBytes));

                                        doneBytes += length - doneBytes;
                                    }

                                    mediaChecksum.Update(data);
                                    imageTask.Value = doneBytes;
                                }
                            });

                if(errno != ErrorNumber.NoError)
                    return (int)errno;

                AaruConsole.WriteLine();

                foreach(CommonTypes.AaruMetadata.Checksum chk in mediaChecksum.End())
                    AaruConsole.WriteLine($"[bold]{string.Format(UI.Checksums_Media_has_0, chk.Type)}[/] {chk.Value}");

                break;
            }

            default:
            {
                var mediaImage = inputFormat as IMediaImage;
                mediaChecksum = new Checksum(enabledChecksums);

                AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                            Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                            Start(ctx =>
                            {
                                ProgressTask diskTask = ctx.AddTask(Localization.Core.Hashing_sectors);
                                ulong        sectors  = mediaImage.Info.Sectors;
                                diskTask.MaxValue = sectors;
                                ulong doneSectors = 0;

                                while(doneSectors < sectors)
                                {
                                    byte[] sector;

                                    if(sectors - doneSectors >= SECTORS_TO_READ)
                                    {
                                        errno = mediaImage.ReadSectors(doneSectors, SECTORS_TO_READ, out sector);

                                        if(errno != ErrorNumber.NoError)
                                        {
                                            AaruConsole.
                                                ErrorWriteLine(string.
                                                                   Format(
                                                                       UI.Error_0_while_reading_1_sectors_from_sector_2,
                                                                       errno, SECTORS_TO_READ,
                                                                       doneSectors));

                                            return;
                                        }

                                        diskTask.Description =
                                            string.Format(UI.Hashing_sectors_0_to_1, doneSectors,
                                                          doneSectors + SECTORS_TO_READ);

                                        doneSectors += SECTORS_TO_READ;
                                    }
                                    else
                                    {
                                        errno = mediaImage.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                       out sector);

                                        if(errno != ErrorNumber.NoError)
                                        {
                                            AaruConsole.
                                                ErrorWriteLine(string.
                                                                   Format(
                                                                       UI.Error_0_while_reading_1_sectors_from_sector_2,
                                                                       errno, sectors - doneSectors,
                                                                       doneSectors));

                                            return;
                                        }

                                        diskTask.Description =
                                            string.Format(UI.Hashing_sectors_0_to_1, doneSectors,
                                                          doneSectors + (sectors - doneSectors));

                                        doneSectors += sectors - doneSectors;
                                    }

                                    mediaChecksum.Update(sector);
                                    diskTask.Value = doneSectors;
                                }
                            });

                if(errno != ErrorNumber.NoError)
                    return (int)errno;

                AaruConsole.WriteLine();

                foreach(CommonTypes.AaruMetadata.Checksum chk in mediaChecksum.End())
                    AaruConsole.WriteLine($"[bold]{string.Format(UI.Checksums_Disk_has_0, chk.Type)}[/] {chk.Value}");

                break;
            }
        }

        return (int)ErrorNumber.NoError;
    }
}