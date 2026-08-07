// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Ls.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'ls' command.
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using Aaru.Helpers;
using Aaru.Localization;
using Aaru.Logging;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Commands.Filesystem;

sealed class LsCommand : Command<LsCommand.Settings>
{
    const string MODULE_NAME = "Ls command";

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        AaruLogging.Debug(MODULE_NAME, "--debug={0}",    settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--encoding={0}", Markup.Escape(settings.Encoding  ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--input={0}",    Markup.Escape(settings.ImagePath ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--options={0}",  Markup.Escape(settings.Options   ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}",  settings.Verbose);
        AaruLogging.Debug(MODULE_NAME, "--volume={0}",   settings.Volume?.ToString() ?? "all");
        Statistics.AddCommand("ls");

        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = PluginRegister.Singleton.GetFilter(settings.ImagePath);
        });

        Dictionary<string, string> parsedOptions = Options.Parse(settings.Options);
        AaruLogging.Debug(MODULE_NAME, UI.Parsed_options);

        foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
            AaruLogging.Debug(MODULE_NAME, "{0} = {1}", parsedOption.Key, parsedOption.Value);

        parsedOptions.Add("debug", settings.Debug.ToString());

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
                AaruLogging.WriteLine(UI.Image_format_not_identified_not_proceeding_with_listing);

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

                AaruLogging.Debug(MODULE_NAME, UI.Image_without_headers_is_0_bytes, imageFormat.Info.ImageSize);

                AaruLogging.Debug(MODULE_NAME, UI.Image_has_0_sectors, imageFormat.Info.Sectors);

                AaruLogging.Debug(MODULE_NAME, UI.Image_identifies_media_type_as_0, imageFormat.Info.MediaType);

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

            List<Partition> partitions = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Enumerating_partitions).IsIndeterminate();
                partitions = Core.Partitions.GetAll(imageFormat);
            });

            Core.Partitions.AddSchemesToStats(partitions);

            if(partitions.Count == 0)
            {
                AaruLogging.Debug(MODULE_NAME, UI.No_partitions_found);

                partitions.Add(new Partition
                {
                    Description = Localization.Core.Whole_device,
                    Length      = imageFormat.Info.Sectors,
                    Offset      = 0,
                    Size        = imageFormat.Info.SectorSize * imageFormat.Info.Sectors,
                    Sequence    = 1,
                    Start       = 0
                });
            }

            AaruLogging.WriteLine(UI._0_partitions_found, partitions.Count);

            var volumeCounter = 0;

            for(var i = 0; i < partitions.Count; i++)
            {
                // Skip partitions if we've already found and listed the requested volume
                if(settings.Volume.HasValue && volumeCounter > settings.Volume.Value) break;

                List<string> idPlugins = null;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Identifying_filesystems_on_partition).IsIndeterminate();
                    Core.Filesystems.Identify(imageFormat, out idPlugins, partitions[i]);
                });

                if(idPlugins.Count == 0)
                {
                    if(!settings.Volume.HasValue)
                    {
                        AaruLogging.WriteLine();
                        AaruLogging.WriteLine($"[bold]{string.Format(UI.Partition_0, partitions[i].Sequence)}[/]");
                        AaruLogging.WriteLine(UI.Filesystem_not_identified);
                    }
                }
                else
                {
                    ErrorNumber error = ErrorNumber.InvalidArgument;

                    // Enumerate all readable filesystem plugins identified on this partition. Each one
                    // counts as a distinct volume for the purposes of --volume selection. This is needed
                    // for images where a single partition hosts multiple overlapping filesystems
                    // (e.g. an ISO9660 + UDF bridge disc, HFS wrapper over HFS+, etc.).
                    var readablePlugins = new List<(string Name, IReadOnlyFilesystem Fs)>();

                    foreach(string pluginName in idPlugins)
                    {
                        if(!plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out IReadOnlyFilesystem fs)) continue;
                        if(fs is null) continue;

                        readablePlugins.Add((pluginName, fs));
                    }

                    if(readablePlugins.Count == 0)
                    {
                        if(!settings.Volume.HasValue)
                        {
                            AaruLogging.WriteLine();
                            AaruLogging.WriteLine($"[bold]{string.Format(UI.Partition_0, partitions[i].Sequence)}[/]");

                            if(idPlugins.Count > 1)
                            {
                                AaruLogging.WriteLine($"[italic]{
                                    string.Format(UI.Identified_by_0_plugins, idPlugins.Count)}[/]");
                            }
                            else
                                AaruLogging.WriteLine($"[bold]{string.Format(UI.Identified_by_0, idPlugins[0])}[/]");
                        }

                        continue;
                    }

                    var printedPartitionHeader = false;

                    foreach((_, IReadOnlyFilesystem fs) in readablePlugins)
                    {
                        int currentVolume = volumeCounter;

                        // Advance the counter so each readable filesystem occupies its own volume slot.
                        volumeCounter++;

                        if(settings.Volume.HasValue && settings.Volume.Value != currentVolume) continue;

                        if(!printedPartitionHeader)
                        {
                            AaruLogging.WriteLine();
                            AaruLogging.WriteLine($"[bold]{string.Format(UI.Partition_0, partitions[i].Sequence)}[/]");

                            if(readablePlugins.Count > 1)
                            {
                                AaruLogging.WriteLine($"[italic]{
                                    string.Format(UI.Identified_by_0_plugins, readablePlugins.Count)}[/]");
                            }

                            printedPartitionHeader = true;
                        }

                        AaruLogging.WriteLine(readablePlugins.Count > 1
                                                  ? $"[bold]{string.Format(UI.As_identified_by_0, fs.Name)}[/]"
                                                  : $"[bold]{string.Format(UI.Identified_by_0,    fs.Name)}[/]");

                        Core.Spectre.ProgressSingleSpinner(ctx =>
                        {
                            ctx.AddTask(UI.Mounting_filesystem).IsIndeterminate();

                            error = fs.Mount(imageFormat,
                                             partitions[i],
                                             encodingClass,
                                             parsedOptions,
                                             settings.Namespace);
                        });

                        if(error == ErrorNumber.NoError)
                        {
                            ListFilesInDir("/", fs);

                            Statistics.AddFilesystem(fs.Metadata.Type);
                        }
                        else
                            AaruLogging.Error(UI.Unable_to_mount_volume_error_0, error.ToString());

                        // Targeting a single volume: stop once it has been processed.
                        if(settings.Volume.HasValue) break;
                    }
                }
            }
        }
        catch(Exception ex)
        {
            AaruLogging.Error(string.Format(UI.Error_reading_file_0, Markup.Escape(ex.Message)));
            AaruLogging.Exception(ex, UI.Error_reading_file_0, ex.Message);

            return (int)ErrorNumber.UnexpectedException;
        }

        return (int)ErrorNumber.NoError;
    }

    static void ListFilesInDir(string path, [NotNull] IReadOnlyFilesystem fs)
    {
        ErrorNumber error = ErrorNumber.InvalidArgument;
        IDirNode    node  = null;

        if(path.StartsWith('/')) path = path[1..];

        AaruLogging.WriteLine("{0}",
                              string.IsNullOrEmpty(path)
                                  ? UI.Root_directory
                                  : string.Format(UI.Directory_0, Markup.Escape(path)));

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Reading_directory).IsIndeterminate();
            error = fs.OpenDir(path, out node);
        });

        if(error != ErrorNumber.NoError)
        {
            AaruLogging.Error(UI.Error_0_reading_directory_1, error.ToString(), path);

            return;
        }

        Dictionary<string, FileEntryInfo> stats = new();

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Retrieving_file_information).IsIndeterminate();

            while(fs.ReadDir(node, out string entry) == ErrorNumber.NoError && entry is not null)
            {
                fs.Stat(path + "/" + entry, out FileEntryInfo stat);

                stats[entry] = stat;
            }

            fs.CloseDir(node);
        });

        var table = new Table();

        table.Border(TableBorder.None);

        table.AddColumn(new TableColumn($"[underline]{UI.Title_Attributes}[/]")
        {
            NoWrap    = true,
            Alignment = Justify.Right
        });

        table.AddColumn(new TableColumn($"[underline]{UI.Title_Size}[/]")
        {
            NoWrap    = true,
            Alignment = Justify.Right,
            Width     = 12
        });

        table.AddColumn(new TableColumn($"[underline]{UI.Title_Date_modified}[/]")
        {
            NoWrap    = true,
            Alignment = Justify.Center,
            Width     = 20
        });

        table.AddColumn(new TableColumn($"[underline]{UI.Title_Name}[/]")
        {
            NoWrap    = true,
            Alignment = Justify.Left
        });

        foreach(KeyValuePair<string, FileEntryInfo> entry in stats.OrderBy(static e =>
                                                                               e.Value?.Attributes
                                                                                  .HasFlag(FileAttributes.Directory) ==
                                                                               false))
        {
            if(entry.Value != null)
            {
                if(entry.Value.Attributes.HasFlag(FileAttributes.Directory))
                {
                    table.AddRow($"[gold3]{entry.Value.Attributes.ToAttributeChars()}[/]",
                                 "",
                                 "",
                                 $"[{DirColorsParser.Instance.DirectoryColor}]{Markup.Escape(entry.Key)}[/]");

                    AaruLogging.Information($"{entry.Value.Attributes.ToAttributeChars()} {entry.Key}");
                }
                else
                {
                    if(!DirColorsParser.Instance.ExtensionColors.TryGetValue(Path.GetExtension(entry.Key) ?? "",
                                                                             out string color))
                        color = DirColorsParser.Instance.NormalColor;

                    table.AddRow($"[gold3]{entry.Value.Attributes.ToAttributeChars()}[/]",
                                 $"[lime]{entry.Value.Length}[/]",
                                 $"[dodgerblue1]{entry.Value.LastWriteTimeUtc:s}[/]",
                                 $"[{color}]{Markup.Escape(entry.Key)}[/]");

                    AaruLogging
                       .Information($"{entry.Value.Attributes.ToAttributeChars()} {entry.Value.Length} {entry.Value.LastWriteTimeUtc:s} {entry.Key}");
                }


                error = fs.ListXAttr(path + "/" + entry.Key, out List<string> xattrs);

                if(error != ErrorNumber.NoError) continue;

                foreach(string xattr in xattrs)
                {
                    byte[] xattrBuf = [];
                    error = fs.GetXattr(path + "/" + entry.Key, xattr, ref xattrBuf);

                    if(error != ErrorNumber.NoError) continue;

                    table.AddRow("", $"[lime]{xattrBuf.Length}[/]", "", $"[fuchsia]{Markup.Escape(xattr)}[/]");

                    AaruLogging.Information($"{xattrBuf.Length} {xattr}");
                }
            }
            else
            {
                table.AddRow("", "", "", $"[green]{Markup.Escape(entry.Key)}[/]");

                AaruLogging.Information(entry.Key);
            }
        }

        AnsiConsole.Write(table);
        AaruLogging.WriteLine();


        foreach(KeyValuePair<string, FileEntryInfo> subdirectory in
                stats.Where(static e => e.Value?.Attributes.HasFlag(FileAttributes.Directory) == true))
            ListFilesInDir(path + "/" + subdirectory.Key, fs);
    }

#region Nested type: Settings

    public class Settings : FilesystemFamily
    {
        [LocalizedDescription(nameof(UI.Name_of_character_encoding_to_use))]
        [CommandOption("-e|--encoding")]
        [DefaultValue(null)]
        public string Encoding { get; init; }
        [LocalizedDescription(nameof(UI.Comma_separated_name_value_pairs_of_filesystem_options))]
        [CommandOption("-O|--options")]
        [DefaultValue(null)]
        public string Options { get; init; }
        [LocalizedDescription(nameof(UI.Namespace_to_use_for_filenames))]
        [CommandOption("-n|--namespace")]
        [DefaultValue(null)]
        public string Namespace { get; init; }
        [LocalizedDescription(nameof(UI.List_only_specified_volume_number))]
        [CommandOption("--volume")]
        [DefaultValue(null)]
        public int? Volume { get; init; }
        [LocalizedDescription(nameof(UI.Media_image_path))]
        [CommandArgument(0, "<image-path>")]
        public string ImagePath { get; init; }
    }

#endregion
}