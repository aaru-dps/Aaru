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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.CommandLine;
using System.CommandLine.Invocation;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Spectre.Console;

namespace Aaru.Commands.Image
{
    internal sealed class EntropyCommand : Command
    {
        static ProgressTask _progressTask1;
        static ProgressTask _progressTask2;

        public EntropyCommand() : base("entropy", "Calculates entropy and/or duplicated sectors of an image.")
        {
            Add(new Option(new[]
                {
                    "--duplicated-sectors", "-p"
                }, "Calculates how many sectors are duplicated (have same exact data in user area).")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--separated-tracks", "-t"
                }, "Calculates entropy for each track separately.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--whole-disc", "-w"
                }, "Calculates entropy for the whole disc.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Media image path",
                Name        = "image-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, bool duplicatedSectors, string imagePath,
                                 bool separatedTracks, bool wholeDisc)
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
                ctx.AddTask("Identifying file filter...").IsIndeterminate();
                inputFilter = filtersList.GetFilter(imagePath);
            });

            if(inputFilter == null)
            {
                AaruConsole.ErrorWriteLine("Cannot open specified file.");

                return (int)ErrorNumber.CannotOpenFile;
            }

            IMediaImage inputFormat = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Identifying image format...").IsIndeterminate();
                inputFormat = ImageFormat.Detect(inputFilter);
            });

            if(inputFormat == null)
            {
                AaruConsole.ErrorWriteLine("Unable to recognize image format, not checksumming");

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            ErrorNumber opened = ErrorNumber.NoData;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Opening image file...").IsIndeterminate();
                opened = inputFormat.Open(inputFilter);
            });

            if(opened != ErrorNumber.NoError)
            {
                AaruConsole.WriteLine("Error {opened} opening image format");

                return (int)opened;
            }

            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Statistics.AddFilter(inputFilter.Name);

            var entropyCalculator = new Entropy(debug, inputFormat);

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
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
                                    AaruConsole.
                                        ErrorWriteLine("Calculating disc entropy of multisession images is not yet implemented.");

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
                                    AaruConsole.WriteLine("Entropy for track {0} is {1:F4}.", trackEntropy.Track,
                                                          trackEntropy.Entropy);

                                    if(trackEntropy.UniqueSectors != null)
                                        AaruConsole.WriteLine("Track {0} has {1} unique sectors ({2:P3})",
                                                              trackEntropy.Track, trackEntropy.UniqueSectors,
                                                              (double)trackEntropy.UniqueSectors /
                                                              trackEntropy.Sectors);
                                }
                            }

                            if(!wholeDisc)
                                return;

                            EntropyResults entropy = entropyCalculator.CalculateMediaEntropy(duplicatedSectors);

                            AaruConsole.WriteLine("Entropy for disk is {0:F4}.", entropy.Entropy);

                            if(entropy.UniqueSectors != null)
                                AaruConsole.WriteLine("Disk has {0} unique sectors ({1:P3})", entropy.UniqueSectors,
                                                      (double)entropy.UniqueSectors / entropy.Sectors);
                        });

            return (int)ErrorNumber.NoError;
        }
    }
}