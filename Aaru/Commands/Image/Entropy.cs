// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Entropy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'entropy' command.
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

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using Spectre.Console;

namespace Aaru.Commands.Image;

sealed class EntropyCommand : Command
{
    static ProgressTask _progressTask1;
    static ProgressTask _progressTask2;

    public EntropyCommand() : base("entropy", UI.Image_Entropy_Command_Description)
    {
        Add(new Option<bool>(new[]
        {
            "--duplicated-sectors", "-p"
        }, () => true, UI.Calculates_how_many_sectors_are_duplicated));

        Add(new Option<bool>(new[]
        {
            "--separated-tracks", "-t"
        }, () => true, UI.Calculates_entropy_for_each_track_separately));

        Add(new Option<bool>(new[]
        {
            "--whole-disc", "-w"
        }, () => true, UI.Calculates_entropy_for_the_whole_disc));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Media_image_path,
            Name        = "image-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool debug, bool verbose, bool duplicatedSectors, string imagePath, bool separatedTracks,
                             bool wholeDisc)
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

        Statistics.AddCommand("entropy");

        AaruConsole.DebugWriteLine("Entropy command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("Entropy command", "--duplicated-sectors={0}", duplicatedSectors);
        AaruConsole.DebugWriteLine("Entropy command", "--input={0}", imagePath);
        AaruConsole.DebugWriteLine("Entropy command", "--separated-tracks={0}", separatedTracks);
        AaruConsole.DebugWriteLine("Entropy command", "--verbose={0}", verbose);
        AaruConsole.DebugWriteLine("Entropy command", "--whole-disc={0}", wholeDisc);

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
            AaruConsole.WriteLine(UI.Error_0, opened);

            return (int)opened;
        }

        Statistics.AddMediaFormat(inputFormat.Format);
        Statistics.AddMedia(inputFormat.Info.MediaType, false);
        Statistics.AddFilter(inputFilter.Name);

        var entropyCalculator = new Entropy(debug, inputFormat);

        AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                    Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).Start(ctx =>
                    {
                        entropyCalculator.InitProgressEvent += () =>
                        {
                            _progressTask1 = ctx.AddTask("Progress");
                        };

                        entropyCalculator.InitProgress2Event += () =>
                        {
                            _progressTask2 = ctx.AddTask("Progress");
                        };

                        entropyCalculator.UpdateProgressEvent += (text, current, maximum) =>
                        {
                            _progressTask1             ??= ctx.AddTask("Progress");
                            _progressTask1.Description =   Markup.Escape(text);
                            _progressTask1.Value       =   current;
                            _progressTask1.MaxValue    =   maximum;
                        };

                        entropyCalculator.UpdateProgress2Event += (text, current, maximum) =>
                        {
                            _progressTask2             ??= ctx.AddTask("Progress");
                            _progressTask2.Description =   Markup.Escape(text);
                            _progressTask2.Value       =   current;
                            _progressTask2.MaxValue    =   maximum;
                        };

                        entropyCalculator.EndProgressEvent += () =>
                        {
                            _progressTask1?.StopTask();
                            _progressTask1 = null;
                        };

                        entropyCalculator.EndProgress2Event += () =>
                        {
                            _progressTask2?.StopTask();
                            _progressTask2 = null;
                        };

                        if(wholeDisc && inputFormat is IOpticalMediaImage opticalFormat)
                        {
                            if(opticalFormat.Sessions?.Count > 1)
                            {
                                AaruConsole.ErrorWriteLine(UI.
                                                               Calculating_disc_entropy_of_multisession_images_is_not_yet_implemented);

                                wholeDisc = false;
                            }

                            if(opticalFormat.Tracks?.Count == 1)
                                separatedTracks = false;
                        }

                        if(separatedTracks)
                        {
                            EntropyResults[] tracksEntropy =
                                entropyCalculator.CalculateTracksEntropy(duplicatedSectors);

                            foreach(EntropyResults trackEntropy in tracksEntropy)
                            {
                                AaruConsole.WriteLine(UI.Entropy_for_track_0_is_1, trackEntropy.Track,
                                                      trackEntropy.Entropy);

                                if(trackEntropy.UniqueSectors != null)
                                    AaruConsole.WriteLine(UI.Track_0_has_1_unique_sectors_2, trackEntropy.Track,
                                                          trackEntropy.UniqueSectors,
                                                          (double)trackEntropy.UniqueSectors / trackEntropy.Sectors);
                            }
                        }

                        if(!wholeDisc)
                            return;

                        EntropyResults entropy = inputFormat.Info.XmlMediaType == XmlMediaType.LinearMedia
                                                     ? entropyCalculator.CalculateLinearMediaEntropy()
                                                     : entropyCalculator.CalculateMediaEntropy(duplicatedSectors);

                        AaruConsole.WriteLine(UI.Entropy_for_disk_is_0, entropy.Entropy);

                        if(entropy.UniqueSectors != null)
                            AaruConsole.WriteLine(UI.Disk_has_0_unique_sectors_1, entropy.UniqueSectors,
                                                  (double)entropy.UniqueSectors / entropy.Sectors);
                    });

        return (int)ErrorNumber.NoError;
    }
}