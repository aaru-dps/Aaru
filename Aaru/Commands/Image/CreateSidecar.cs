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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Localization;
using Aaru.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace Aaru.Commands.Image;

sealed class CreateSidecarCommand : Command<CreateSidecarCommand.Settings>
{
    const  string       MODULE_NAME = "Create sidecar command";
    static ProgressTask _progressTask1;
    static ProgressTask _progressTask2;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        Statistics.AddCommand("create-sidecar");

        AaruLogging.Debug(MODULE_NAME, "--block-size={0}",     settings.BlockSize);
        AaruLogging.Debug(MODULE_NAME, "--debug={0}",          settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--encoding={0}",       settings.Encoding);
        AaruLogging.Debug(MODULE_NAME, "--input={0}",          settings.ImagePath);
        AaruLogging.Debug(MODULE_NAME, "--tape={0}",           settings.Tape);
        AaruLogging.Debug(MODULE_NAME, "--enable-spamsum={0}", settings.EnableSpamsum);
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}",        settings.Verbose);

        Encoding encodingClass = null;

        if(settings.Encoding != null)
        {
            try
            {
                encodingClass = Claunia.Encoding.Encoding.GetEncoding(settings.Encoding);

                if(settings.Verbose) AaruLogging.Verbose(UI.encoding_for_0, encodingClass.EncodingName);
            }
            catch(ArgumentException)
            {
                AaruLogging.Error(UI.Specified_encoding_is_not_supported);

                return (int)ErrorNumber.EncodingUnknown;
            }
        }

        if(File.Exists(settings.ImagePath))
        {
            if(settings.Tape)
            {
                AaruLogging.Error(UI.You_cannot_use_tape_option_when_input_is_a_file);

                return (int)ErrorNumber.NotDirectory;
            }

            IFilter inputFilter = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
                inputFilter = PluginRegister.Singleton.GetFilter(settings.ImagePath);
            });

            if(inputFilter == null)
            {
                AaruLogging.Error(UI.Cannot_open_specified_file);

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
                    AaruLogging.WriteLine(UI.Image_format_not_identified_not_proceeding_with_sidecar_creation);

                    return (int)ErrorNumber.UnrecognizedFormat;
                }

                if(settings.Verbose)
                {
                    AaruLogging.Verbose(UI.Image_format_identified_by_0_1,
                                        Markup.Escape(imageFormat.Name),
                                        imageFormat.Id);
                }
                else
                    AaruLogging.WriteLine(UI.Image_format_identified_by_0, Markup.Escape(imageFormat.Name));

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
                        AaruLogging.WriteLine(UI.Unable_to_open_image_format);
                        AaruLogging.WriteLine(Localization.Core.Error_0, opened);

                        return (int)opened;
                    }

                    AaruLogging.Debug(MODULE_NAME, UI.Correctly_opened_image_file);
                }
                catch(Exception ex)
                {
                    AaruLogging.Error(UI.Unable_to_open_image_format);
                    AaruLogging.Error(Localization.Core.Error_0, ex.Message);
                    AaruLogging.Exception(ex, Localization.Core.Error_0, ex.Message);

                    return (int)ErrorNumber.CannotOpenFormat;
                }

                Statistics.AddMediaFormat(imageFormat.Format);
                Statistics.AddFilter(inputFilter.Name);

                var sidecarClass = new Sidecar(imageFormat,
                                               settings.ImagePath,
                                               inputFilter.Id,
                                               encodingClass,
                                               settings.EnableSpamsum);

                Metadata sidecar = new();

                AnsiConsole.Progress()
                           .AutoClear(true)
                           .HideCompleted(true)
                           .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                           .Start(ctx =>
                            {
                                sidecarClass.InitProgressEvent += () => { _progressTask1 = ctx.AddTask("Progress"); };

                                sidecarClass.InitProgressEvent2 += () => { _progressTask2 = ctx.AddTask("Progress"); };

                                sidecarClass.UpdateProgressEvent += (text, current, maximum) =>
                                {
                                    _progressTask1             ??= ctx.AddTask("Progress");
                                    _progressTask1.Description =   text;
                                    _progressTask1.Value       =   current;
                                    _progressTask1.MaxValue    =   maximum;
                                };

                                sidecarClass.UpdateProgressEvent2 += (text, current, maximum) =>
                                {
                                    _progressTask2             ??= ctx.AddTask("Progress");
                                    _progressTask2.Description =   text;
                                    _progressTask2.Value       =   current;
                                    _progressTask2.MaxValue    =   maximum;
                                };

                                sidecarClass.EndProgressEvent += static () =>
                                {
                                    _progressTask1?.StopTask();
                                    _progressTask1 = null;
                                };

                                sidecarClass.EndProgressEvent2 += static () =>
                                {
                                    _progressTask2?.StopTask();
                                    _progressTask2 = null;
                                };

                                sidecarClass.UpdateStatusEvent += static text => { AaruLogging.WriteLine(text); };

                                Console.CancelKeyPress += (_, e) =>
                                {
                                    e.Cancel = true;
                                    sidecarClass.Abort();
                                };

                                sidecar = sidecarClass.Create();
                            });

                if(sidecarClass.Aborted) AaruLogging.WriteLine(Localization.Core.Sidecar_aborted_partial_metadata);

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(Localization.Core.Writing_metadata_sidecar).IsIndeterminate();

                    var jsonFs =
                        new FileStream(Path.Combine(Path.GetDirectoryName(settings.ImagePath) ??
                                                    throw new InvalidOperationException(),
                                                    Path.GetFileNameWithoutExtension(settings.ImagePath) +
                                                    ".metadata.json"),
                                       FileMode.Create);

                    JsonSerializer.Serialize(jsonFs,
                                             new MetadataJson
                                             {
                                                 AaruMetadata = sidecar
                                             },
                                             typeof(MetadataJson),
                                             MetadataJsonContext.Default);

                    jsonFs.Close();
                });
            }
            catch(Exception ex)
            {
                AaruLogging.Exception(ex, UI.Error_reading_file_0, ex.Message);

                return (int)ErrorNumber.UnexpectedException;
            }
        }
        else if(Directory.Exists(settings.ImagePath))
        {
            if(!settings.Tape)
            {
                AaruLogging.Error(Localization.Core.Cannot_create_a_sidecar_from_a_directory);

                return (int)ErrorNumber.IsDirectory;
            }

            string[] contents = Directory.GetFiles(settings.ImagePath, "*", SearchOption.TopDirectoryOnly);
            var      files    = contents.Where(file => new FileInfo(file).Length % settings.BlockSize == 0).ToList();

            files.Sort(StringComparer.CurrentCultureIgnoreCase);

            var      sidecarClass = new Sidecar();
            Metadata sidecar      = new();

            AnsiConsole.Progress()
                       .AutoClear(true)
                       .HideCompleted(true)
                       .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                       .Start(ctx =>
                        {
                            sidecarClass.InitProgressEvent += () => { _progressTask1 = ctx.AddTask("Progress"); };

                            sidecarClass.InitProgressEvent2 += () => { _progressTask2 = ctx.AddTask("Progress"); };

                            sidecarClass.UpdateProgressEvent += (text, current, maximum) =>
                            {
                                _progressTask1             ??= ctx.AddTask("Progress");
                                _progressTask1.Description =   text;
                                _progressTask1.Value       =   current;
                                _progressTask1.MaxValue    =   maximum;
                            };

                            sidecarClass.UpdateProgressEvent2 += (text, current, maximum) =>
                            {
                                _progressTask2             ??= ctx.AddTask("Progress");
                                _progressTask2.Description =   text;
                                _progressTask2.Value       =   current;
                                _progressTask2.MaxValue    =   maximum;
                            };

                            sidecarClass.EndProgressEvent += static () =>
                            {
                                _progressTask1?.StopTask();
                                _progressTask1 = null;
                            };

                            sidecarClass.EndProgressEvent2 += static () =>
                            {
                                _progressTask2?.StopTask();
                                _progressTask2 = null;
                            };

                            sidecarClass.UpdateStatusEvent += static text => { AaruLogging.WriteLine(text); };

                            Console.CancelKeyPress += (_, e) =>
                            {
                                e.Cancel = true;
                                sidecarClass.Abort();
                            };

                            sidecar = sidecarClass.BlockTape(Path.GetFileName(settings.ImagePath),
                                                             files,
                                                             (uint)settings.BlockSize);
                        });

            if(sidecarClass.Aborted) AaruLogging.WriteLine(Localization.Core.Sidecar_aborted_partial_metadata);

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(Localization.Core.Writing_metadata_sidecar).IsIndeterminate();

                var jsonFs =
                    new FileStream(Path.Combine(Path.GetDirectoryName(settings.ImagePath) ??
                                                throw new InvalidOperationException(),
                                                Path.GetFileNameWithoutExtension(settings.ImagePath) +
                                                ".metadata.json"),
                                   FileMode.Create);

                JsonSerializer.Serialize(jsonFs,
                                         new MetadataJson
                                         {
                                             AaruMetadata = sidecar
                                         },
                                         typeof(MetadataJson),
                                         MetadataJsonContext.Default);

                jsonFs.Close();
            });
        }
        else
            AaruLogging.Error(UI.The_specified_input_file_cannot_be_found);

        return (int)ErrorNumber.NoError;
    }

#region Nested type: Settings

    public class Settings : ImageFamily
    {
        [CommandOption("-b|--block-size")]
        [LocalizedDescription(nameof(UI.Create_sidecar_block_size_help))]
        [DefaultValue(512)]
        public int BlockSize { get; init; }
        [CommandOption("-e|--encoding")]
        [LocalizedDescription(nameof(UI.Name_of_character_encoding_to_use))]
        [DefaultValue(null)]
        public string Encoding { get; init; }
        [CommandOption("-t|--tape")]
        [LocalizedDescription(nameof(UI.Tape_argument_input_help))]
        [DefaultValue(false)]
        public bool Tape { get; init; }
        [LocalizedDescription(nameof(UI.Media_image_path))]
        [CommandArgument(0, "<image-path>")]
        public string ImagePath { get; init; }
        [CommandOption("--enable-spamsum")]
        [LocalizedDescription(nameof(UI.Enables_calculation_of_Spamsum_fuzzy_hashes))]
        [DefaultValue(false)]
        public bool EnableSpamsum { get; init; }
    }

#endregion
}