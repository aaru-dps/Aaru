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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using Spectre.Console;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Commands.Filesystem;

sealed class FilesystemInfoCommand : Command
{
    const string MODULE_NAME = "Fs-info command";

    public FilesystemInfoCommand() : base("info", UI.Filesystem_Info_Command_Description)
    {
        Add(new Option<string>(new[]
        {
            "--encoding", "-e"
        }, () => null, UI.Name_of_character_encoding_to_use));

        Add(new Option<bool>(new[]
        {
            "--filesystems", "-f"
        }, () => true, UI.Searches_and_prints_information_about_filesystems));

        Add(new Option<bool>(new[]
        {
            "--partitions", "-p"
        }, () => true, UI.Searches_and_interprets_partitions));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Media_image_path,
            Name        = "image-path"
        });

        Handler = CommandHandler.Create(typeof(FilesystemInfoCommand).GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool verbose, bool debug, string encoding, bool filesystems, bool partitions,
                             string imagePath)
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

        Statistics.AddCommand("fs-info");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}", debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--encoding={0}", encoding);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--filesystems={0}", filesystems);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}", imagePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--partitions={0}", partitions);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}", verbose);

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

        Encoding encodingClass = null;

        if(encoding != null)
            try
            {
                encodingClass = Claunia.Encoding.Encoding.GetEncoding(encoding);

                if(verbose)
                    AaruConsole.VerboseWriteLine(UI.encoding_for_0, encodingClass.EncodingName);
            }
            catch(ArgumentException)
            {
                AaruConsole.ErrorWriteLine(UI.Specified_encoding_is_not_supported);

                return (int)ErrorNumber.EncodingUnknown;
            }

        PluginBase plugins = PluginBase.Singleton;

        bool checkRaw = false;

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
                AaruConsole.WriteLine(UI.Image_format_not_identified_not_proceeding_with_analysis);

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            if(imageFormat == null)
            {
                AaruConsole.WriteLine(UI.Command_not_supported_for_this_image_type);

                return (int)ErrorNumber.InvalidArgument;
            }

            if(verbose)
                AaruConsole.VerboseWriteLine(UI.Image_format_identified_by_0_1, imageFormat.Name, imageFormat.Id);
            else
                AaruConsole.WriteLine(UI.Image_format_identified_by_0, imageFormat.Name);

            AaruConsole.WriteLine();

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

                if(verbose)
                {
                    ImageInfo.PrintImageInfo(imageFormat);
                    AaruConsole.WriteLine();
                }

                Statistics.AddMediaFormat(imageFormat.Format);
                Statistics.AddMedia(imageFormat.Info.MediaType, false);
                Statistics.AddFilter(inputFilter.Name);
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine(UI.Unable_to_open_image_format);
                AaruConsole.ErrorWriteLine(Localization.Core.Error_0, ex.Message);
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Stack_trace_0, ex.StackTrace);

                return (int)ErrorNumber.CannotOpenFormat;
            }

            List<string> idPlugins = null;
            Type         pluginType;
            string       information;

            if(partitions)
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
                    AaruConsole.DebugWriteLine(MODULE_NAME, UI.No_partitions_found);

                    if(!filesystems)
                    {
                        AaruConsole.WriteLine(UI.No_partitions_found_not_searching_for_filesystems);

                        return (int)ErrorNumber.NothingFound;
                    }

                    checkRaw = true;
                }
                else
                {
                    AaruConsole.WriteLine(UI._0_partitions_found, partitionsList.Count);

                    for(int i = 0; i < partitionsList.Count; i++)
                    {
                        Table table = new()
                        {
                            Title = new TableTitle(string.Format(UI.Partition_0, partitionsList[i].Sequence))
                        };

                        table.AddColumn("");
                        table.AddColumn("");
                        table.HideHeaders();

                        table.AddRow(UI.Title_Name, Markup.Escape(partitionsList[i].Name ?? ""));
                        table.AddRow(UI.Title_Type, Markup.Escape(partitionsList[i].Type ?? ""));

                        table.AddRow(Localization.Core.Title_Start,
                                     string.Format(UI.sector_0_byte_1, partitionsList[i].Start,
                                                   partitionsList[i].Offset));

                        table.AddRow(UI.Title_Length,
                                     string.Format(UI._0_sectors_1_bytes, partitionsList[i].Length,
                                                   partitionsList[i].Size));

                        table.AddRow(UI.Title_Scheme, Markup.Escape(partitionsList[i].Scheme           ?? ""));
                        table.AddRow(UI.Title_Description, Markup.Escape(partitionsList[i].Description ?? ""));

                        AnsiConsole.Write(table);

                        if(!filesystems)
                            continue;

                        Core.Spectre.ProgressSingleSpinner(ctx =>
                        {
                            ctx.AddTask(UI.Identifying_filesystems_on_partition).IsIndeterminate();
                            Core.Filesystems.Identify(imageFormat, out idPlugins, partitionsList[i]);
                        });

                        switch(idPlugins.Count)
                        {
                            case 0:
                                AaruConsole.WriteLine($"[bold]{UI.Filesystem_not_identified}[/]");

                                break;
                            case > 1:
                            {
                                AaruConsole.WriteLine($"[italic]{string.Format(UI.Identified_by_0_plugins,
                                                                               idPlugins.Count)}[/]");

                                foreach(string pluginName in idPlugins)
                                    if(plugins.Filesystems.TryGetValue(pluginName, out pluginType))
                                    {
                                        if(Activator.CreateInstance(pluginType) is not IFilesystem fs)
                                            continue;

                                        AaruConsole.WriteLine($"[bold]{string.Format(UI.As_identified_by_0, fs.Name)
                                        }[/]");

                                        fs.GetInformation(imageFormat, partitionsList[i], encodingClass,
                                                          out information, out FileSystem fsMetadata);

                                        AaruConsole.Write(information);
                                        Statistics.AddFilesystem(fsMetadata.Type);
                                    }

                                break;
                            }
                            default:
                            {
                                plugins.Filesystems.TryGetValue(idPlugins[0], out pluginType);

                                if(pluginType == null ||
                                   Activator.CreateInstance(pluginType) is not IFilesystem fs)
                                    continue;

                                AaruConsole.WriteLine($"[bold]{string.Format(UI.Identified_by_0, fs.Name)}[/]");

                                fs.GetInformation(imageFormat, partitionsList[i], encodingClass, out information,
                                                  out FileSystem fsMetadata);

                                AaruConsole.Write("{0}", information);
                                Statistics.AddFilesystem(fsMetadata.Type);

                                break;
                            }
                        }

                        AaruConsole.WriteLine();
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
                        AaruConsole.WriteLine($"[bold]{UI.Filesystem_not_identified}[/]");

                        break;
                    case > 1:
                    {
                        AaruConsole.WriteLine($"[italic]{string.Format(UI.Identified_by_0_plugins, idPlugins.Count)
                        }[/]");

                        foreach(string pluginName in idPlugins)
                            if(plugins.Filesystems.TryGetValue(pluginName, out pluginType))
                            {
                                if(Activator.CreateInstance(pluginType) is not IFilesystem fs)
                                    continue;

                                AaruConsole.WriteLine($"[bold]{string.Format(UI.As_identified_by_0, fs.Name)}[/]");

                                fs.GetInformation(imageFormat, wholePart, encodingClass, out information,
                                                  out FileSystem fsMetadata);

                                AaruConsole.Write(information);
                                Statistics.AddFilesystem(fsMetadata.Type);
                            }

                        break;
                    }
                    default:
                    {
                        plugins.Filesystems.TryGetValue(idPlugins[0], out pluginType);

                        if(pluginType == null ||
                           Activator.CreateInstance(pluginType) is not IFilesystem fs)
                            break;

                        AaruConsole.WriteLine($"[bold]{string.Format(UI.Identified_by_0, fs.Name)}[/]");

                        fs.GetInformation(imageFormat, wholePart, encodingClass, out information,
                                          out FileSystem fsMetadata);

                        AaruConsole.Write(information);
                        Statistics.AddFilesystem(fsMetadata.Type);

                        break;
                    }
                }
            }
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine(string.Format(UI.Error_reading_file_0, ex.Message));
            AaruConsole.DebugWriteLine(MODULE_NAME, ex.StackTrace);

            return (int)ErrorNumber.UnexpectedException;
        }

        return (int)ErrorNumber.NoError;
    }
}