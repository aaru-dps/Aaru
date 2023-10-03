// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CreateSidecar.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'create-sidecar' command.
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
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using JetBrains.Annotations;
using Spectre.Console;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace Aaru.Commands.Image;

sealed class CreateSidecarCommand : Command
{
    const  string       MODULE_NAME = "Create sidecar command";
    static ProgressTask _progressTask1;
    static ProgressTask _progressTask2;

    public CreateSidecarCommand() : base("create-sidecar", UI.Image_Create_Sidecar_Command_Description)
    {
        Add(new Option<int>(new[] { "--block-size", "-b" }, () => 512, UI.Tape_block_size_argument_help));

        Add(new Option<string>(new[] { "--encoding", "-e" }, () => null, UI.Name_of_character_encoding_to_use));

        Add(new Option<bool>(new[] { "--tape", "-t" }, () => false, UI.Tape_argument_input_help));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Media_image_path,
            Name        = "image-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool   debug,     bool verbose, uint blockSize, [CanBeNull] string encodingName,
                             string imagePath, bool tape)
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

        Statistics.AddCommand("create-sidecar");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--block-size={0}", blockSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",      debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--encoding={0}",   encodingName);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}",      imagePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--tape={0}",       tape);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}",    verbose);

        Encoding encodingClass = null;

        if(encodingName != null)
        {
            try
            {
                encodingClass = Claunia.Encoding.Encoding.GetEncoding(encodingName);

                if(verbose)
                    AaruConsole.VerboseWriteLine(UI.encoding_for_0, encodingClass.EncodingName);
            }
            catch(ArgumentException)
            {
                AaruConsole.ErrorWriteLine(UI.Specified_encoding_is_not_supported);

                return (int)ErrorNumber.EncodingUnknown;
            }
        }

        if(File.Exists(imagePath))
        {
            if(tape)
            {
                AaruConsole.ErrorWriteLine(UI.You_cannot_use_tape_option_when_input_is_a_file);

                return (int)ErrorNumber.NotDirectory;
            }

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

            try
            {
                IBaseImage imageFormat = null;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                                                   {
                                                       ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
                                                       imageFormat = ImageFormat.Detect(inputFilter);
                                                   });

                if(imageFormat == null)
                {
                    AaruConsole.WriteLine(UI.Image_format_not_identified_not_proceeding_with_sidecar_creation);

                    return (int)ErrorNumber.UnrecognizedFormat;
                }

                if(verbose)
                    AaruConsole.VerboseWriteLine(UI.Image_format_identified_by_0_1, imageFormat.Name, imageFormat.Id);
                else
                    AaruConsole.WriteLine(UI.Image_format_identified_by_0, imageFormat.Name);

                try
                {
                    ErrorNumber opened = ErrorNumber.NoData;

                    Core.Spectre.ProgressSingleSpinner(ctx =>
                                                       {
                                                           ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
                                                           opened = imageFormat.Open(inputFilter);
                                                       });

                    if(opened != ErrorNumber.NoError)
                    {
                        AaruConsole.WriteLine(UI.Unable_to_open_image_format);
                        AaruConsole.WriteLine(Localization.Core.Error_0, opened);

                        return (int)opened;
                    }

                    AaruConsole.DebugWriteLine(MODULE_NAME, UI.Correctly_opened_image_file);
                }
                catch(Exception ex)
                {
                    AaruConsole.ErrorWriteLine(UI.Unable_to_open_image_format);
                    AaruConsole.ErrorWriteLine(Localization.Core.Error_0, ex.Message);

                    return (int)ErrorNumber.CannotOpenFormat;
                }

                Statistics.AddMediaFormat(imageFormat.Format);
                Statistics.AddFilter(inputFilter.Name);

                var      sidecarClass = new Sidecar(imageFormat, imagePath, inputFilter.Id, encodingClass);
                Metadata sidecar      = new();

                AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                            Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                            Start(ctx =>
                                  {
                                      sidecarClass.InitProgressEvent += () =>
                                                                        {
                                                                            _progressTask1 = ctx.AddTask("Progress");
                                                                        };

                                      sidecarClass.InitProgressEvent2 += () =>
                                                                         {
                                                                             _progressTask2 = ctx.AddTask("Progress");
                                                                         };

                                      sidecarClass.UpdateProgressEvent += (text, current, maximum) =>
                                                                          {
                                                                              _progressTask1 ??=
                                                                                  ctx.AddTask("Progress");
                                                                              _progressTask1.Description =
                                                                                  Markup.Escape(text);
                                                                              _progressTask1.Value    = current;
                                                                              _progressTask1.MaxValue = maximum;
                                                                          };

                                      sidecarClass.UpdateProgressEvent2 += (text, current, maximum) =>
                                                                           {
                                                                               _progressTask2 ??=
                                                                                   ctx.AddTask("Progress");
                                                                               _progressTask2.Description =
                                                                                   Markup.Escape(text);
                                                                               _progressTask2.Value    = current;
                                                                               _progressTask2.MaxValue = maximum;
                                                                           };

                                      sidecarClass.EndProgressEvent += () =>
                                                                       {
                                                                           _progressTask1?.StopTask();
                                                                           _progressTask1 = null;
                                                                       };

                                      sidecarClass.EndProgressEvent2 += () =>
                                                                        {
                                                                            _progressTask2?.StopTask();
                                                                            _progressTask2 = null;
                                                                        };

                                      sidecarClass.UpdateStatusEvent += text =>
                                                                        {
                                                                            AaruConsole.WriteLine(Markup.Escape(text));
                                                                        };

                                      System.Console.CancelKeyPress += (_, e) =>
                                                                       {
                                                                           e.Cancel = true;
                                                                           sidecarClass.Abort();
                                                                       };

                                      sidecar = sidecarClass.Create();
                                  });

                Core.Spectre.ProgressSingleSpinner(ctx =>
                                                   {
                                                       ctx.AddTask(Localization.Core.Writing_metadata_sidecar).
                                                           IsIndeterminate();

                                                       var jsonFs =
                                                           new
                                                               FileStream(Path.Combine(Path.GetDirectoryName(imagePath) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(imagePath) + ".metadata.json"),
                                                                          FileMode.Create);

                                                       JsonSerializer.Serialize(jsonFs, new MetadataJson
                                                       {
                                                           AaruMetadata = sidecar
                                                       }, typeof(MetadataJson), MetadataJsonContext.Default);

                                                       jsonFs.Close();
                                                   });
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine(string.Format(UI.Error_reading_file_0, ex.Message));
                AaruConsole.DebugWriteLine(MODULE_NAME, ex.StackTrace);

                return (int)ErrorNumber.UnexpectedException;
            }
        }
        else if(Directory.Exists(imagePath))
        {
            if(!tape)
            {
                AaruConsole.ErrorWriteLine(Localization.Core.Cannot_create_a_sidecar_from_a_directory);

                return (int)ErrorNumber.IsDirectory;
            }

            string[] contents = Directory.GetFiles(imagePath, "*", SearchOption.TopDirectoryOnly);
            var      files    = contents.Where(file => new FileInfo(file).Length % blockSize == 0).ToList();

            files.Sort(StringComparer.CurrentCultureIgnoreCase);

            var      sidecarClass = new Sidecar();
            Metadata sidecar      = new();

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                              {
                                  sidecarClass.InitProgressEvent += () => { _progressTask1 = ctx.AddTask("Progress"); };

                                  sidecarClass.InitProgressEvent2 += () =>
                                                                     {
                                                                         _progressTask2 = ctx.AddTask("Progress");
                                                                     };

                                  sidecarClass.UpdateProgressEvent += (text, current, maximum) =>
                                                                      {
                                                                          _progressTask1 ??= ctx.AddTask("Progress");
                                                                          _progressTask1.Description =
                                                                              Markup.Escape(text);
                                                                          _progressTask1.Value    = current;
                                                                          _progressTask1.MaxValue = maximum;
                                                                      };

                                  sidecarClass.UpdateProgressEvent2 += (text, current, maximum) =>
                                                                       {
                                                                           _progressTask2 ??= ctx.AddTask("Progress");
                                                                           _progressTask2.Description =
                                                                               Markup.Escape(text);
                                                                           _progressTask2.Value    = current;
                                                                           _progressTask2.MaxValue = maximum;
                                                                       };

                                  sidecarClass.EndProgressEvent += () =>
                                                                   {
                                                                       _progressTask1?.StopTask();
                                                                       _progressTask1 = null;
                                                                   };

                                  sidecarClass.EndProgressEvent2 += () =>
                                                                    {
                                                                        _progressTask2?.StopTask();
                                                                        _progressTask2 = null;
                                                                    };

                                  sidecarClass.UpdateStatusEvent += text =>
                                                                    {
                                                                        AaruConsole.WriteLine(Markup.Escape(text));
                                                                    };

                                  System.Console.CancelKeyPress += (_, e) =>
                                                                   {
                                                                       e.Cancel = true;
                                                                       sidecarClass.Abort();
                                                                   };

                                  sidecar = sidecarClass.BlockTape(Path.GetFileName(imagePath), files, blockSize);
                              });

            Core.Spectre.ProgressSingleSpinner(ctx =>
                                               {
                                                   ctx.AddTask(Localization.Core.Writing_metadata_sidecar).
                                                       IsIndeterminate();

                                                   var jsonFs =
                                                       new
                                                           FileStream(Path.Combine(Path.GetDirectoryName(imagePath) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(imagePath) + ".metadata.json"),
                                                                      FileMode.Create);

                                                   JsonSerializer.Serialize(jsonFs, new MetadataJson
                                                   {
                                                       AaruMetadata = sidecar
                                                   }, typeof(MetadataJson), MetadataJsonContext.Default);

                                                   jsonFs.Close();
                                               });
        }
        else
            AaruConsole.ErrorWriteLine(UI.The_specified_input_file_cannot_be_found);

        return (int)ErrorNumber.NoError;
    }
}