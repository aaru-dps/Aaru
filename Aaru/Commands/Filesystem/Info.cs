// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'fs-info' command.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
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
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Commands.Filesystem;

sealed class FilesystemInfoCommand : Command<FilesystemInfoCommand.Settings>
{
    const string MODULE_NAME = "Fs-info command";

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        Statistics.AddCommand("fs-info");

        AaruLogging.Debug(MODULE_NAME, "--debug={0}",       settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--encoding={0}",    Markup.Escape(settings.Encoding ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--filesystems={0}", settings.Filesystems);
        AaruLogging.Debug(MODULE_NAME, "--input={0}",       Markup.Escape(settings.ImagePath ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--partitions={0}",  settings.Partitions);
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}",     settings.Verbose);

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

        PluginRegister plugins = PluginRegister.Singleton;

        var checkRaw = false;

        try
        {
            IMediaImage imageFormat = null;
            IBaseImage  baseImage   = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
                baseImage   = ImageFormat.Detect(inputFilter);
                imageFormat = baseImage as IMediaImage;
            });

            if(baseImage == null)
            {
                AaruLogging.WriteLine(UI.Image_format_not_identified_not_proceeding_with_analysis);

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            if(imageFormat == null)
            {
                AaruLogging.WriteLine(UI.Command_not_supported_for_this_image_type);

                return (int)ErrorNumber.InvalidArgument;
            }

            if(settings.Verbose)
                AaruLogging.Verbose(UI.Image_format_identified_by_0_1, Markup.Escape(imageFormat.Name), imageFormat.Id);
            else
                AaruLogging.WriteLine(UI.Image_format_identified_by_0, Markup.Escape(imageFormat.Name));

            AaruLogging.WriteLine();

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

                if(settings.Verbose)
                {
                    ImageInfo.PrintImageInfo(imageFormat);
                    AaruLogging.WriteLine();
                }

                Statistics.AddMediaFormat(imageFormat.Format);
                Statistics.AddMedia(imageFormat.Info.MediaType, false);
                Statistics.AddFilter(inputFilter.Name);
            }
            catch(Exception ex)
            {
                AaruLogging.Error(UI.Unable_to_open_image_format);
                AaruLogging.Error(Localization.Core.Error_0, ex.Message);
                AaruLogging.Exception(ex, ex.Message);

                return (int)ErrorNumber.CannotOpenFormat;
            }

            List<string> idPlugins = null;
            IFilesystem  fs;
            string       information;

            if(settings.Partitions)
            {
                List<Partition> partitionsList = null;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Enumerating_partitions).IsIndeterminate();
                    partitionsList = Core.Partitions.GetAll(imageFormat);
                });

                Core.Partitions.AddSchemesToStats(partitionsList);

                if(partitionsList.Count == 0)
                {
                    AaruLogging.Debug(MODULE_NAME, UI.No_partitions_found);

                    if(!settings.Filesystems)
                    {
                        AaruLogging.WriteLine(UI.No_partitions_found_not_searching_for_filesystems);

                        return (int)ErrorNumber.NothingFound;
                    }

                    checkRaw = true;
                }
                else
                {
                    AaruLogging.WriteLine(UI._0_partitions_found, partitionsList.Count);

                    for(var i = 0; i < partitionsList.Count; i++)
                    {
                        Table table = new()
                        {
                            Title = new TableTitle(string.Format(UI.Partition_0, partitionsList[i].Sequence))
                        };

                        AaruLogging.Information(UI.Partition_0, partitionsList[i].Sequence);

                        table.AddColumn("");
                        table.AddColumn("");
                        table.HideHeaders();

                        table.AddRow(UI.Title_Name, $"[darkgreen]{Markup.Escape(partitionsList[i].Name ?? "")}[/]");
                        table.AddRow(UI.Title_Type, $"[olive]{Markup.Escape(partitionsList[i].Type     ?? "")}[/]");

                        table.AddRow(Localization.Core.Title_Start,
                                     string.Format(UI.sector_0_byte_1,
                                                   partitionsList[i].Start,
                                                   partitionsList[i].Offset));

                        table.AddRow(UI.Title_Length,
                                     string.Format(UI._0_sectors_1_bytes,
                                                   partitionsList[i].Length,
                                                   partitionsList[i].Size));

                        table.AddRow(UI.Title_Scheme, $"[purple]{Markup.Escape(partitionsList[i].Scheme ?? "")}[/]");

                        table.AddRow(UI.Title_Description,
                                     $"[slateblue1]{Markup.Escape(partitionsList[i].Description ?? "")}[/]");

                        AaruLogging.Information($"{UI.Title_Name}: {Markup.Escape(partitionsList[i].Name ?? "")}");
                        AaruLogging.Information($"{UI.Title_Type}: {Markup.Escape(partitionsList[i].Type ?? "")}");

                        AaruLogging
                           .Information($"{Localization.Core.Title_Start}: {string.Format(UI.sector_0_byte_1, partitionsList[i].Start, partitionsList[i].Offset)}");

                        AaruLogging
                           .Information($"{UI.Title_Length}: {string.Format(UI._0_sectors_1_bytes, partitionsList[i].Length, partitionsList[i].Size)}");

                        AaruLogging.Information($"{UI.Title_Scheme}: {Markup.Escape(partitionsList[i].Scheme ?? "")}");

                        AaruLogging
                           .Information($"{UI.Title_Description}: {Markup.Escape(partitionsList[i].Description ?? "")}");

                        AaruLogging.WriteLine();

                        AnsiConsole.Write(table);

                        if(!settings.Filesystems) continue;

                        Core.Spectre.ProgressSingleSpinner(ctx =>
                        {
                            ctx.AddTask(UI.Identifying_filesystems_on_partition).IsIndeterminate();
                            Core.Filesystems.Identify(imageFormat, out idPlugins, partitionsList[i]);
                        });

                        switch(idPlugins.Count)
                        {
                            case 0:
                                AaruLogging.WriteLine($"[bold]{UI.Filesystem_not_identified}[/]");

                                break;
                            case > 1:
                            {
                                AaruLogging.WriteLine($"[italic]{string.Format(UI.Identified_by_0_plugins,
                                                                               idPlugins.Count)}[/]");

                                foreach(string pluginName in idPlugins)
                                {
                                    if(!plugins.Filesystems.TryGetValue(pluginName, out fs)) continue;
                                    if(fs is null) continue;

                                    AaruLogging.WriteLine($"[bold]{string.Format(UI.As_identified_by_0, fs.Name)
                                    }[/]");

                                    fs.GetInformation(imageFormat,
                                                      partitionsList[i],
                                                      encodingClass,
                                                      out information,
                                                      out FileSystem fsMetadata);

                                    AaruLogging.Write("{0}", information);
                                    Statistics.AddFilesystem(fsMetadata.Type);
                                }

                                break;
                            }
                            default:
                            {
                                plugins.Filesystems.TryGetValue(idPlugins[0], out fs);

                                if(fs is null) continue;

                                AaruLogging.WriteLine($"[bold]{string.Format(UI.Identified_by_0, fs.Name)}[/]");

                                fs.GetInformation(imageFormat,
                                                  partitionsList[i],
                                                  encodingClass,
                                                  out information,
                                                  out FileSystem fsMetadata);

                                AaruLogging.Write("{0}", information);
                                Statistics.AddFilesystem(fsMetadata.Type);

                                break;
                            }
                        }

                        AaruLogging.WriteLine();
                    }
                }
            }

            if(checkRaw)
            {
                var wholePart = new Partition
                {
                    Name   = Localization.Core.Whole_device,
                    Length = imageFormat.Info.Sectors,
                    Size   = imageFormat.Info.Sectors * imageFormat.Info.SectorSize
                };

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Identifying_filesystems).IsIndeterminate();
                    Core.Filesystems.Identify(imageFormat, out idPlugins, wholePart);
                });

                switch(idPlugins.Count)
                {
                    case 0:
                        AaruLogging.WriteLine($"[bold]{UI.Filesystem_not_identified}[/]");

                        break;
                    case > 1:
                    {
                        AaruLogging.WriteLine($"[italic]{string.Format(UI.Identified_by_0_plugins, idPlugins.Count)
                        }[/]");

                        foreach(string pluginName in idPlugins)
                        {
                            if(!plugins.Filesystems.TryGetValue(pluginName, out fs)) continue;
                            if(fs is null) continue;

                            AaruLogging.WriteLine($"[bold]{string.Format(UI.As_identified_by_0, fs.Name)}[/]");

                            fs.GetInformation(imageFormat,
                                              wholePart,
                                              encodingClass,
                                              out information,
                                              out FileSystem fsMetadata);

                            AaruLogging.Write("{0}", information);
                            Statistics.AddFilesystem(fsMetadata.Type);
                        }

                        break;
                    }
                    default:
                    {
                        plugins.Filesystems.TryGetValue(idPlugins[0], out fs);

                        if(fs is null) break;

                        AaruLogging.WriteLine($"[bold]{string.Format(UI.Identified_by_0, fs.Name)}[/]");

                        fs.GetInformation(imageFormat,
                                          wholePart,
                                          encodingClass,
                                          out information,
                                          out FileSystem fsMetadata);

                        AaruLogging.Write("{0}", information);
                        Statistics.AddFilesystem(fsMetadata.Type);

                        break;
                    }
                }
            }
        }
        catch(Exception ex)
        {
            AaruLogging.Error(Markup.Escape(string.Format(UI.Error_reading_file_0, ex.Message)));
            AaruLogging.Exception(ex, UI.Error_reading_file_0, ex.Message);

            return (int)ErrorNumber.UnexpectedException;
        }

        return (int)ErrorNumber.NoError;
    }

#region Nested type: Settings

    public class Settings : FilesystemFamily
    {
        [LocalizedDescription(nameof(UI.Name_of_character_encoding_to_use))]
        [DefaultValue(null)]
        [CommandOption("-e|--encoding")]
        public string Encoding { get; init; }
        [LocalizedDescription(nameof(UI.Searches_and_interprets_partitions))]
        [DefaultValue(true)]
        [CommandOption("-p|--partitions")]
        public bool Partitions { get; init; }
        [LocalizedDescription(nameof(UI.Searches_and_prints_information_about_filesystems))]
        [CommandOption("-f|--filesystems")]
        [DefaultValue(true)]
        public bool Filesystems { get; init; }
        [LocalizedDescription(nameof(UI.Media_image_path))]
        [CommandArgument(0, "<image-path>")]
        public string ImagePath { get; init; }
    }

#endregion
}