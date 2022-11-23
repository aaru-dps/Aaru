// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Verify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'verify' command.
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
// Copyright © 2011-2022 Natalia Portillo
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

sealed class VerifyCommand : Command
{
    public VerifyCommand() : base("verify", UI.Image_Verify_Command_Description)
    {
        Add(new Option<bool>(new[]
        {
            "--verify-disc", "-w"
        }, () => true, UI.Verify_disc_image_if_supported));

        Add(new Option<bool>(new[]
        {
            "--verify-sectors", "-s"
        }, () => true, UI.Verify_all_sectors_if_supported));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Disc_image_path,
            Name        = "image-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool debug, bool verbose, string imagePath, bool verifyDisc = true,
                             bool verifySectors = true)
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
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };

        Statistics.AddCommand("verify");

        AaruConsole.DebugWriteLine("Verify command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("Verify command", "--input={0}", imagePath);
        AaruConsole.DebugWriteLine("Verify command", "--verbose={0}", verbose);
        AaruConsole.DebugWriteLine("Verify command", "--verify-disc={0}", verifyDisc);
        AaruConsole.DebugWriteLine("Verify command", "--verify-sectors={0}", verifySectors);

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
            AaruConsole.ErrorWriteLine(UI.Unable_to_recognize_image_format_not_verifying);

            return (int)ErrorNumber.FormatNotFound;
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

        bool? correctImage   = null;
        bool? correctSectors = null;

        var verifiableImage        = inputFormat as IVerifiableImage;
        var verifiableSectorsImage = inputFormat as IVerifiableSectorsImage;

        if(verifiableImage is null &&
           verifiableSectorsImage is null)
        {
            AaruConsole.ErrorWriteLine(UI.The_specified_image_does_not_support_any_kind_of_verification);

            return (int)ErrorNumber.NotVerifiable;
        }

        TimeSpan checkTime;

        if(verifyDisc && verifiableImage != null)
        {
            bool? discCheckStatus = null;
            checkTime = new TimeSpan();

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Verifying_image_checksums).IsIndeterminate();

                DateTime startCheck = DateTime.UtcNow;
                discCheckStatus = verifiableImage.VerifyMediaImage();
                DateTime endCheck = DateTime.UtcNow;
                checkTime = endCheck - startCheck;
            });

            switch(discCheckStatus)
            {
                case true:
                    AaruConsole.WriteLine(UI.Disc_image_checksums_are_correct);

                    break;
                case false:
                    AaruConsole.WriteLine(UI.Disc_image_checksums_are_incorrect);

                    break;
                case null:
                    AaruConsole.WriteLine(UI.Disc_image_does_not_contain_checksums);

                    break;
            }

            correctImage = discCheckStatus;
            AaruConsole.VerboseWriteLine(UI.Checking_disc_image_checksums_took_0_seconds, checkTime.TotalSeconds);
        }

        if(!verifySectors)
            return correctImage switch
            {
                null  => (int)ErrorNumber.NotVerifiable,
                false => (int)ErrorNumber.BadImageSectorsNotVerified,
                true  => (int)ErrorNumber.CorrectImageSectorsNotVerified
            };

        DateTime    startCheck  = DateTime.Now;
        DateTime    endCheck    = startCheck;
        List<ulong> failingLbas = new();
        List<ulong> unknownLbas = new();

        if(verifiableSectorsImage is IOpticalMediaImage { Tracks: {} } opticalMediaImage)
        {
            List<Track> inputTracks      = opticalMediaImage.Tracks;
            ulong       currentSectorAll = 0;

            startCheck = DateTime.UtcNow;

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            ProgressTask discTask = ctx.AddTask(UI.Checking_tracks);
                            discTask.MaxValue = inputTracks.Count;

                            foreach(Track currentTrack in inputTracks)
                            {
                                discTask.Description =
                                    string.Format(UI.Checking_track_0_of_1, discTask.Value + 1, inputTracks.Count);

                                ulong remainingSectors = currentTrack.EndSector - currentTrack.StartSector + 1;

                                ulong currentSector = 0;

                                ProgressTask trackTask = ctx.AddTask(UI.Checking_sector);
                                trackTask.MaxValue = remainingSectors;

                                while(remainingSectors > 0)
                                {
                                    trackTask.Description =
                                        string.Format(UI.Checking_sector_0_of_1_on_track_2, currentSectorAll,
                                                      inputFormat.Info.Sectors, currentTrack.Sequence);

                                    List<ulong> tempFailingLbas;
                                    List<ulong> tempUnknownLbas;

                                    if(remainingSectors < 512)
                                        opticalMediaImage.VerifySectors(currentSector, (uint)remainingSectors,
                                                                        currentTrack.Sequence, out tempFailingLbas,
                                                                        out tempUnknownLbas);
                                    else
                                        opticalMediaImage.VerifySectors(currentSector, 512, currentTrack.Sequence,
                                                                        out tempFailingLbas, out tempUnknownLbas);

                                    failingLbas.AddRange(tempFailingLbas);

                                    unknownLbas.AddRange(tempUnknownLbas);

                                    if(remainingSectors < 512)
                                    {
                                        currentSector    += remainingSectors;
                                        currentSectorAll += remainingSectors;
                                        trackTask.Value  += remainingSectors;
                                        remainingSectors =  0;
                                    }
                                    else
                                    {
                                        currentSector    += 512;
                                        currentSectorAll += 512;
                                        trackTask.Value  += 512;
                                        remainingSectors -= 512;
                                    }
                                }

                                trackTask.StopTask();
                                discTask.Increment(1);
                            }

                            endCheck = DateTime.UtcNow;
                        });
        }
        else if(verifiableSectorsImage != null)
        {
            ulong remainingSectors = inputFormat.Info.Sectors;
            ulong currentSector    = 0;

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            ProgressTask diskTask = ctx.AddTask(UI.Checking_sectors);
                            diskTask.MaxValue = inputFormat.Info.Sectors;

                            startCheck = DateTime.UtcNow;

                            while(remainingSectors > 0)
                            {
                                diskTask.Description =
                                    string.Format(UI.Checking_sector_0_of_1, currentSector, inputFormat.Info.Sectors);

                                List<ulong> tempFailingLbas;
                                List<ulong> tempUnknownLbas;

                                if(remainingSectors < 512)
                                    verifiableSectorsImage.VerifySectors(currentSector, (uint)remainingSectors,
                                                                         out tempFailingLbas, out tempUnknownLbas);
                                else
                                    verifiableSectorsImage.VerifySectors(currentSector, 512, out tempFailingLbas,
                                                                         out tempUnknownLbas);

                                failingLbas.AddRange(tempFailingLbas);

                                unknownLbas.AddRange(tempUnknownLbas);

                                if(remainingSectors < 512)
                                {
                                    currentSector    += remainingSectors;
                                    diskTask.Value   += remainingSectors;
                                    remainingSectors =  0;
                                }
                                else
                                {
                                    currentSector    += 512;
                                    diskTask.Value   += 512;
                                    remainingSectors -= 512;
                                }
                            }

                            endCheck = DateTime.UtcNow;
                        });
        }

        checkTime = endCheck - startCheck;

        if(unknownLbas.Count > 0)
            AaruConsole.WriteLine(UI.There_is_at_least_one_sector_that_does_not_contain_a_checksum);

        if(failingLbas.Count > 0)
            AaruConsole.WriteLine(UI.There_is_at_least_one_sector_with_incorrect_checksum_or_errors);

        if(unknownLbas.Count == 0 &&
           failingLbas.Count == 0)
            AaruConsole.WriteLine(UI.All_sector_checksums_are_correct);

        AaruConsole.VerboseWriteLine(UI.Checking_sector_checksums_took_0_seconds, checkTime.TotalSeconds);

        if(verbose)
        {
            AaruConsole.VerboseWriteLine($"[red]{UI.LBAs_with_error}[/]");

            if(failingLbas.Count == (int)inputFormat.Info.Sectors)
                AaruConsole.VerboseWriteLine($"\t[red]{UI.all_sectors}[/]");
            else
                foreach(ulong t in failingLbas)
                    AaruConsole.VerboseWriteLine("\t{0}", t);

            AaruConsole.WriteLine($"[yellow3_1]{UI.LBAs_without_checksum}[/]");

            if(unknownLbas.Count == (int)inputFormat.Info.Sectors)
                AaruConsole.VerboseWriteLine($"\t[yellow3_1]{UI.all_sectors}[/]");
            else
                foreach(ulong t in unknownLbas)
                    AaruConsole.VerboseWriteLine("\t{0}", t);
        }

        // TODO: Convert to table
        AaruConsole.WriteLine($"[italic]{UI.Total_sectors}[/] {inputFormat.Info.Sectors}");
        AaruConsole.WriteLine($"[italic]{UI.Total_errors}[/] {failingLbas.Count}");
        AaruConsole.WriteLine($"[italic]{UI.Total_unknowns}[/] {unknownLbas.Count}");
        AaruConsole.WriteLine($"[italic]{UI.Total_errors_plus_unknowns}[/] {failingLbas.Count + unknownLbas.Count}");

        if(failingLbas.Count > 0)
            correctSectors = false;
        else if((ulong)unknownLbas.Count < inputFormat.Info.Sectors)
            correctSectors = true;

        return correctImage switch
        {
            null when correctSectors is null   => (int)ErrorNumber.NotVerifiable,
            null when correctSectors == false  => (int)ErrorNumber.BadSectorsImageNotVerified,
            null                               => (int)ErrorNumber.CorrectSectorsImageNotVerified,
            false when correctSectors is null  => (int)ErrorNumber.BadImageSectorsNotVerified,
            false when correctSectors == false => (int)ErrorNumber.BadImageBadSectors,
            false                              => (int)ErrorNumber.CorrectSectorsBadImage,
            true when correctSectors is null   => (int)ErrorNumber.CorrectImageSectorsNotVerified,
            true when correctSectors == false  => (int)ErrorNumber.CorrectImageBadSectors,
            true                               => (int)ErrorNumber.NoError
        };
    }
}