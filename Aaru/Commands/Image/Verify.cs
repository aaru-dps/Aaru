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
using System.CommandLine.Invocation;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Spectre.Console;

namespace Aaru.Commands.Image;

internal sealed class VerifyCommand : Command
{
    public VerifyCommand() : base("verify", "Verifies a disc image integrity, and if supported, sector integrity.")
    {
        Add(new Option(new[]
            {
                "--verify-disc", "-w"
            }, "Verify disc image if supported.")
            {
                Argument = new Argument<bool>(() => true),
                Required = false
            });

        Add(new Option(new[]
            {
                "--verify-sectors", "-s"
            }, "Verify all sectors if supported.")
            {
                Argument = new Argument<bool>(() => true),
                Required = false
            });

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = "Disc image path",
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
            ctx.AddTask("Identifying file filter...").IsIndeterminate();
            inputFilter = filtersList.GetFilter(imagePath);
        });

        if(inputFilter == null)
        {
            AaruConsole.ErrorWriteLine("Cannot open specified file.");

            return (int)ErrorNumber.CannotOpenFile;
        }

        IBaseImage inputFormat = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Identifying image format...").IsIndeterminate();
            inputFormat = ImageFormat.Detect(inputFilter);
        });

        if(inputFormat == null)
        {
            AaruConsole.ErrorWriteLine("Unable to recognize image format, not verifying");

            return (int)ErrorNumber.FormatNotFound;
        }

        ErrorNumber opened = ErrorNumber.NoData;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Opening image file...").IsIndeterminate();
            opened = inputFormat.Open(inputFilter);
        });

        if(opened != ErrorNumber.NoError)
        {
            AaruConsole.WriteLine("Unable to open image format");
            AaruConsole.WriteLine("Error {0}", opened);

            return (int)opened;
        }

        Statistics.AddMediaFormat(inputFormat.Format);
        Statistics.AddMedia(inputFormat.Info.MediaType, false);
        Statistics.AddFilter(inputFilter.Name);

        bool? correctImage   = null;
        long  errorSectors   = 0;
        bool? correctSectors = null;
        long  unknownSectors = 0;

        var verifiableImage        = inputFormat as IVerifiableImage;
        var verifiableSectorsImage = inputFormat as IVerifiableSectorsImage;

        if(verifiableImage is null &&
           verifiableSectorsImage is null)
        {
            AaruConsole.ErrorWriteLine("The specified image does not support any kind of verification");

            return (int)ErrorNumber.NotVerifiable;
        }

        if(verifyDisc && verifiableImage != null)
        {
            bool?    discCheckStatus = null;
            TimeSpan checkTime       = new();

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Verifying image checksums...").IsIndeterminate();

                DateTime startCheck = DateTime.UtcNow;
                discCheckStatus = verifiableImage.VerifyMediaImage();
                DateTime endCheck = DateTime.UtcNow;
                checkTime = endCheck - startCheck;
            });

            switch(discCheckStatus)
            {
                case true:
                    AaruConsole.WriteLine("Disc image checksums are correct");

                    break;
                case false:
                    AaruConsole.WriteLine("Disc image checksums are incorrect");

                    break;
                case null:
                    AaruConsole.WriteLine("Disc image does not contain checksums");

                    break;
            }

            correctImage = discCheckStatus;
            AaruConsole.VerboseWriteLine("Checking disc image checksums took {0} seconds", checkTime.TotalSeconds);
        }

        if(verifySectors)
        {
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
                                ProgressTask discTask = ctx.AddTask("Checking tracks...");
                                discTask.MaxValue = inputTracks.Count;

                                foreach(Track currentTrack in inputTracks)
                                {
                                    discTask.Description =
                                        $"Checking track {discTask.Value + 1} of {inputTracks.Count}";

                                    ulong remainingSectors = currentTrack.EndSector - currentTrack.StartSector + 1;

                                    ulong currentSector = 0;

                                    ProgressTask trackTask = ctx.AddTask("Checking sector");
                                    trackTask.MaxValue = remainingSectors;

                                    while(remainingSectors > 0)
                                    {
                                        trackTask.Description =
                                            $"Checking sector {currentSectorAll} of {inputFormat.Info.Sectors}, on track {currentTrack.Sequence}";

                                        List<ulong> tempFailingLbas;
                                        List<ulong> tempUnknownLbas;

                                        if(remainingSectors < 512)
                                            opticalMediaImage.VerifySectors(currentSector, (uint)remainingSectors,
                                                                            currentTrack.Sequence,
                                                                            out tempFailingLbas,
                                                                            out tempUnknownLbas);
                                        else
                                            opticalMediaImage.VerifySectors(currentSector, 512,
                                                                            currentTrack.Sequence,
                                                                            out tempFailingLbas,
                                                                            out tempUnknownLbas);

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
                                ProgressTask diskTask = ctx.AddTask("Checking sectors...");
                                diskTask.MaxValue = inputFormat.Info.Sectors;

                                startCheck = DateTime.UtcNow;

                                while(remainingSectors > 0)
                                {
                                    diskTask.Description =
                                        $"Checking sector {currentSector} of {inputFormat.Info.Sectors}";

                                    List<ulong> tempFailingLbas;
                                    List<ulong> tempUnknownLbas;

                                    if(remainingSectors < 512)
                                        verifiableSectorsImage.VerifySectors(currentSector, (uint)remainingSectors,
                                                                             out tempFailingLbas, out tempUnknownLbas);
                                    else
                                        verifiableSectorsImage.VerifySectors(currentSector, 512,
                                                                             out tempFailingLbas, out tempUnknownLbas);

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

            TimeSpan checkTime = endCheck - startCheck;

            if(unknownSectors > 0)
                AaruConsole.WriteLine("There is at least one sector that does not contain a checksum");

            if(errorSectors > 0)
                AaruConsole.WriteLine("There is at least one sector with incorrect checksum or errors");

            if(unknownSectors == 0 &&
               errorSectors   == 0)
                AaruConsole.WriteLine("All sector checksums are correct");

            AaruConsole.VerboseWriteLine("Checking sector checksums took {0} seconds", checkTime.TotalSeconds);

            if(verbose)
            {
                AaruConsole.VerboseWriteLine("[red]LBAs with error:[/]");

                if(failingLbas.Count == (int)inputFormat.Info.Sectors)
                    AaruConsole.VerboseWriteLine("\t[red]all sectors.[/]");
                else
                    foreach(ulong t in failingLbas)
                        AaruConsole.VerboseWriteLine("\t{0}", t);

                AaruConsole.WriteLine("[yellow3_1]LBAs without checksum:[/]");

                if(unknownLbas.Count == (int)inputFormat.Info.Sectors)
                    AaruConsole.VerboseWriteLine("\t[yellow3_1]all sectors.[/]");
                else
                    foreach(ulong t in unknownLbas)
                        AaruConsole.VerboseWriteLine("\t{0}", t);
            }

            AaruConsole.WriteLine("[italic]Total sectors...........[/] {0}", inputFormat.Info.Sectors);
            AaruConsole.WriteLine("[italic]Total errors............[/] {0}", failingLbas.Count);
            AaruConsole.WriteLine("[italic]Total unknowns..........[/] {0}", unknownLbas.Count);
            AaruConsole.WriteLine("[italic]Total errors+unknowns...[/] {0}", failingLbas.Count + unknownLbas.Count);

            if(failingLbas.Count > 0)
                correctSectors = false;
            else if((ulong)unknownLbas.Count < inputFormat.Info.Sectors)
                correctSectors = true;
        }

        switch(correctImage)
        {
            case null when correctSectors is null:   return (int)ErrorNumber.NotVerifiable;
            case null when correctSectors == false:  return (int)ErrorNumber.BadSectorsImageNotVerified;
            case null when correctSectors == true:   return (int)ErrorNumber.CorrectSectorsImageNotVerified;
            case false when correctSectors is null:  return (int)ErrorNumber.BadImageSectorsNotVerified;
            case false when correctSectors == false: return (int)ErrorNumber.BadImageBadSectors;
            case false when correctSectors == true:  return (int)ErrorNumber.CorrectSectorsBadImage;
            case true when correctSectors is null:   return (int)ErrorNumber.CorrectImageSectorsNotVerified;
            case true when correctSectors == false:  return (int)ErrorNumber.CorrectImageBadSectors;
            case true when correctSectors == true:   return (int)ErrorNumber.NoError;
        }

        return (int)ErrorNumber.NoError;
    }
}