// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Analyze.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'image analyze' command.
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
using System.Text.Json;
using System.Threading;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using Aaru.Localization;
using Aaru.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using DumpExtent = Aaru.CommonTypes.AaruMetadata.Extent;
using DumpHardware = Aaru.CommonTypes.AaruMetadata.DumpHardware;
using ImageInfo = Aaru.Core.ImageInfo;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Commands.Image;

sealed class AnalyzeCommand : Command<AnalyzeCommand.Settings>
{
    const string MODULE_NAME = "Image-analyze command";

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        Statistics.AddCommand("image-analyze");

        Dictionary<string, string> parsedOptions = Options.Parse(settings.Options);

        if(!parsedOptions.ContainsKey("debug")) parsedOptions.Add("debug", settings.Debug.ToString());

        AaruLogging.Debug(MODULE_NAME, "--debug={0}",              settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--encoding={0}",           Markup.Escape(settings.Encoding   ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--input={0}",              Markup.Escape(settings.ImagePath  ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--namespace={0}",          Markup.Escape(settings.Namespace  ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--options={0}",            Markup.Escape(settings.Options    ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--resume-file={0}",        Markup.Escape(settings.ResumeFile ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--ignore-subchannels={0}", settings.IgnoreSubchannels);
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}",            settings.Verbose);

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
            }
            catch(ArgumentException)
            {
                AaruLogging.Error(UI.Specified_encoding_is_not_supported);

                return (int)ErrorNumber.EncodingUnknown;
            }
        }

        try
        {
            IBaseImage  baseImage   = null;
            IMediaImage imageFormat = null;

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

            string resumePath         = FindResumePath(settings.ImagePath, settings.ResumeFile);
            bool   explicitResumePath = !string.IsNullOrWhiteSpace(settings.ResumeFile);

            if(!LoadResume(resumePath, explicitResumePath, out Resume resume)) return (int)ErrorNumber.InvalidArgument;

            if(resume != null && !string.IsNullOrWhiteSpace(resumePath))
            {
                AaruLogging.WriteLine(UI.Using_resume_file_0, Markup.Escape(resumePath));
                AaruLogging.WriteLine();
            }

            List<AffectedExtent> affectedExtents = GetAffectedExtents(baseImage, resume, settings.IgnoreSubchannels);

            if(affectedExtents.Count == 0)
            {
                AaruLogging.Error(UI.No_sector_analysis_data_available);

                return (int)ErrorNumber.NoData;
            }

            PrintAffectedExtents(affectedExtents, imageFormat as IOpticalMediaImage);
            AnalyzeFilesystems(imageFormat, encodingClass, parsedOptions, settings.Namespace, affectedExtents);
        }
        catch(Exception ex)
        {
            AaruLogging.Error(string.Format(UI.Error_reading_file_0, Markup.Escape(ex.Message)));
            AaruLogging.Exception(ex, UI.Error_reading_file_0, ex.Message);

            return (int)ErrorNumber.UnexpectedException;
        }

        return (int)ErrorNumber.NoError;
    }

    static void AnalyzeFilesystems(IMediaImage imageFormat,       Encoding encoding, Dictionary<string, string> options,
                                   string      filenameNamespace, List<AffectedExtent> affectedExtents)
    {
        List<Partition> partitions = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Enumerating_partitions).IsIndeterminate();
            partitions = Core.Partitions.GetAll(imageFormat);
        });

        if(partitions == null || partitions.Count == 0)
        {
            partitions =
            [
                new Partition
                {
                    Name   = Localization.Core.Whole_device,
                    Length = imageFormat.Info.Sectors,
                    Size   = imageFormat.Info.Sectors * imageFormat.Info.SectorSize
                }
            ];
        }

        PluginRegister plugins              = PluginRegister.Singleton;
        var            anyFilesystemMatched = false;

        foreach(Partition partition in partitions)
        {
            List<(ulong start, ulong end)> partitionExtents = FilterExtentsForPartition(affectedExtents, partition);

            if(partitionExtents.Count == 0) continue;

            List<string> idPlugins = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Identifying_filesystems_on_partition).IsIndeterminate();
                Core.Filesystems.Identify(imageFormat, out idPlugins, partition);
            });

            if(idPlugins == null || idPlugins.Count == 0) continue;

            foreach(string pluginName in idPlugins.Distinct())
            {
                if(!plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out IReadOnlyFilesystem fs)) continue;

                ErrorNumber mountError = ErrorNumber.NoData;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Mounting_filesystem).IsIndeterminate();
                    mountError = fs.Mount(imageFormat, partition, encoding, options, filenameNamespace);
                });

                if(mountError != ErrorNumber.NoError)
                {
                    AaruLogging.Error(UI.Unable_to_mount_volume_error_0, mountError.ToString());

                    continue;
                }

                anyFilesystemMatched = true;
                Statistics.AddFilesystem(fs.Metadata.Type);

                ErrorNumber          filesError = ErrorNumber.NoData;
                List<FileSectorInfo> files      = null;

                AnsiConsole.Progress()
                           .AutoClear(true)
                           .HideCompleted(true)
                           .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                           .Start(ctx =>
                            {
                                ProgressTask progressTask = null;

                                filesError = fs.GetFilesWithAffectedSectors(partitionExtents,
                                                                            out files,
                                                                            () =>
                                                                            {
                                                                                progressTask =
                                                                                    ctx.AddTask(UI.Reading_directory);
                                                                            },
                                                                            (text, current, maximum) =>
                                                                            {
                                                                                progressTask ??= ctx.AddTask(text);

                                                                                progressTask.Description     = text;
                                                                                progressTask.IsIndeterminate = false;

                                                                                progressTask.MaxValue =
                                                                                    maximum > 0 ? maximum : 1;

                                                                                progressTask.Value =
                                                                                    current > progressTask.MaxValue
                                                                                        ? progressTask.MaxValue
                                                                                        : current;
                                                                            },
                                                                            text =>
                                                                            {
                                                                                progressTask ??= ctx.AddTask(text);

                                                                                progressTask.Description     = text;
                                                                                progressTask.IsIndeterminate = true;
                                                                            },
                                                                            () =>
                                                                            {
                                                                                progressTask?.StopTask();
                                                                                progressTask = null;
                                                                            });
                            });

                switch(filesError)
                {
                    case ErrorNumber.NoError:
                        PrintFilesystemResults(fs, files, affectedExtents);

                        break;
                    case ErrorNumber.NotSupported:
                    case ErrorNumber.NotImplemented:
                        AaruLogging.WriteLine(UI.Sector_to_file_analysis_not_yet_implemented_for_0,
                                              Markup.Escape(fs.Name));

                        AaruLogging.WriteLine();

                        break;
                    default:
                        AaruLogging.Error(UI.Unable_to_mount_volume_error_0, filesError.ToString());

                        break;
                }

                fs.Unmount();
            }
        }

        if(!anyFilesystemMatched)
        {
            AaruLogging.WriteLine($"[bold]{UI.Filesystem_not_identified}[/]");
            AaruLogging.WriteLine();
        }
    }

    static List<AffectedExtent> GetAffectedExtents(IBaseImage image, Resume resume, bool ignoreSubchannels)
    {
        List<AffectedExtent> affected     = [];
        ulong                totalSectors = image.Info.Sectors;

        if(totalSectors == 0) return affected;

        ulong lastSector = totalSectors - 1;

        List<(ulong start, ulong end)> badSectors = SectorsToExtents(resume?.BadBlocks, lastSector);

        List<(ulong start, ulong end)> subchannelSectors =
            SectorsToExtents(resume?.BadSubchannels?.Select(static s => (ulong)s), lastSector);

        List<(ulong start, ulong end)> dumpedExtents = GetDumpedExtents(image.DumpHardware, resume?.Tries, lastSector);

        foreach((ulong start, ulong end) extent in badSectors)
            affected.Add(new AffectedExtent(AffectedSectorKind.Error, extent.start, extent.end));

        if(!ignoreSubchannels)
        {
            foreach((ulong start, ulong end) extent in subchannelSectors)
                affected.Add(new AffectedExtent(AffectedSectorKind.Subchannel, extent.start, extent.end));
        }

        if(dumpedExtents.Count > 0)
        {
            foreach((ulong start, ulong end) extent in GetUndumpedExtents(totalSectors, dumpedExtents))
                affected.Add(new AffectedExtent(AffectedSectorKind.Undumped, extent.start, extent.end));
        }

        return NormalizeAffectedExtents(affected);
    }

    static List<(ulong start, ulong end)> GetDumpedExtents(IEnumerable<DumpHardware> imageDumpHardware,
                                                           IEnumerable<DumpHardware> resumeDumpHardware,
                                                           ulong                     lastSector)
    {
        List<(ulong start, ulong end)> extents = [];

        AddDumpHardwareExtents(imageDumpHardware,  extents, lastSector);
        AddDumpHardwareExtents(resumeDumpHardware, extents, lastSector);

        return NormalizeExtents(extents);
    }

    static void AddDumpHardwareExtents(IEnumerable<DumpHardware> dumpHardware, List<(ulong start, ulong end)> extents,
                                       ulong                     lastSector)
    {
        if(dumpHardware == null) return;

        foreach(DumpHardware hardware in dumpHardware)
        {
            if(hardware?.Extents == null) continue;

            foreach(DumpExtent extent in hardware.Extents)
            {
                if(extent       == null) continue;
                if(extent.Start > lastSector) continue;

                ulong clampedEnd = extent.End > lastSector ? lastSector : extent.End;

                if(clampedEnd < extent.Start) continue;

                extents.Add((extent.Start, clampedEnd));
            }
        }
    }

    static List<(ulong start, ulong end)> GetUndumpedExtents(ulong                          totalSectors,
                                                             List<(ulong start, ulong end)> dumpedExtents)
    {
        List<(ulong start, ulong end)> undumpedExtents = [];

        if(totalSectors == 0) return undumpedExtents;

        ulong lastSector = totalSectors - 1;

        if(dumpedExtents.Count == 0)
        {
            undumpedExtents.Add((0, lastSector));

            return undumpedExtents;
        }

        ulong nextSector = 0;

        foreach((ulong start, ulong end) dumpedExtent in dumpedExtents)
        {
            if(dumpedExtent.start > nextSector) undumpedExtents.Add((nextSector, dumpedExtent.start - 1));

            if(dumpedExtent.end >= lastSector)
            {
                nextSector = totalSectors;

                break;
            }

            nextSector = dumpedExtent.end + 1;
        }

        if(nextSector < totalSectors) undumpedExtents.Add((nextSector, lastSector));

        return undumpedExtents;
    }

    static List<(ulong start, ulong end)> SectorsToExtents(IEnumerable<ulong> sectors, ulong lastSector)
    {
        List<(ulong start, ulong end)> extents = [];

        if(sectors == null) return extents;

        var orderedSectors = sectors.Where(sector => sector <= lastSector)
                                    .Distinct()
                                    .OrderBy(static sector => sector)
                                    .ToList();

        if(orderedSectors.Count == 0) return extents;

        ulong start = orderedSectors[0];
        ulong end   = orderedSectors[0];

        for(var i = 1; i < orderedSectors.Count; i++)
        {
            if(orderedSectors[i] == end + 1)
            {
                end = orderedSectors[i];

                continue;
            }

            extents.Add((start, end));
            start = orderedSectors[i];
            end   = orderedSectors[i];
        }

        extents.Add((start, end));

        return extents;
    }

    static List<(ulong start, ulong end)> NormalizeExtents(IEnumerable<(ulong start, ulong end)> extents)
    {
        var orderedExtents = extents.Where(static extent => extent.end >= extent.start)
                                    .OrderBy(static extent => extent.start)
                                    .ThenBy(static extent => extent.end)
                                    .ToList();

        List<(ulong start, ulong end)> normalizedExtents = [];

        if(orderedExtents.Count == 0) return normalizedExtents;

        (ulong start, ulong end) currentExtent = orderedExtents[0];

        for(var i = 1; i < orderedExtents.Count; i++)
        {
            if(orderedExtents[i].start <= currentExtent.end + 1)
            {
                currentExtent.end = Math.Max(currentExtent.end, orderedExtents[i].end);

                continue;
            }

            normalizedExtents.Add(currentExtent);
            currentExtent = orderedExtents[i];
        }

        normalizedExtents.Add(currentExtent);

        return normalizedExtents;
    }

    static List<AffectedExtent> NormalizeAffectedExtents(IEnumerable<AffectedExtent> extents)
    {
        List<AffectedExtent>           normalized = [];
        List<(ulong start, ulong end)> claimed    = [];

        AffectedSectorKind[] priorities =
        [
            AffectedSectorKind.Error, AffectedSectorKind.Subchannel, AffectedSectorKind.Undumped
        ];

        foreach(AffectedSectorKind kind in priorities)
        {
            List<(ulong start, ulong end)> kindExtents = NormalizeExtents(extents.Where(extent => extent.Kind == kind)
                                                                             .Select(extent => (extent.Start,
                                                                                              extent.End)));

            if(claimed.Count > 0) kindExtents = SubtractExtents(kindExtents, claimed);

            foreach((ulong start, ulong end) extent in kindExtents)
                normalized.Add(new AffectedExtent(kind, extent.start, extent.end));

            claimed = NormalizeExtents(claimed.Concat(kindExtents));
        }

        return normalized.OrderBy(static extent => extent.Start)
                         .ThenBy(static extent => extent.End)
                         .ThenBy(static extent => extent.Kind)
                         .ToList();
    }

    static List<(ulong start, ulong end)> SubtractExtents(IEnumerable<(ulong start, ulong end)> sourceExtents,
                                                          IEnumerable<(ulong start, ulong end)> excludedExtents)
    {
        List<(ulong start, ulong end)> normalizedSource   = NormalizeExtents(sourceExtents);
        List<(ulong start, ulong end)> normalizedExcluded = NormalizeExtents(excludedExtents);
        List<(ulong start, ulong end)> remaining          = [];

        foreach((ulong start, ulong end) source in normalizedSource)
        {
            List<(ulong start, ulong end)> fragments = [source];

            foreach((ulong start, ulong end) excluded in normalizedExcluded)
            {
                if(fragments.Count == 0) break;

                List<(ulong start, ulong end)> nextFragments = [];

                foreach((ulong start, ulong end) fragment in fragments)
                {
                    if(excluded.end < fragment.start || excluded.start > fragment.end)
                    {
                        nextFragments.Add(fragment);

                        continue;
                    }

                    if(excluded.start > fragment.start) nextFragments.Add((fragment.start, excluded.start - 1));

                    if(excluded.end < fragment.end) nextFragments.Add((excluded.end + 1, fragment.end));
                }

                fragments = nextFragments;
            }

            remaining.AddRange(fragments);
        }

        return NormalizeExtents(remaining);
    }

    static List<(ulong start, ulong end)> FilterExtentsForPartition(IEnumerable<AffectedExtent> affectedExtents,
                                                                    Partition                   partition)
    {
        List<(ulong start, ulong end)> partitionExtents = [];

        foreach(AffectedExtent affectedExtent in affectedExtents)
        {
            if(affectedExtent.End < partition.Start || affectedExtent.Start > partition.End) continue;

            partitionExtents.Add((Math.Max(affectedExtent.Start, partition.Start),
                                  Math.Min(affectedExtent.End, partition.End)));
        }

        return NormalizeExtents(partitionExtents);
    }

    static string FindResumePath(string imagePath, string explicitResumePath)
    {
        if(!string.IsNullOrWhiteSpace(explicitResumePath)) return explicitResumePath;
        if(string.IsNullOrWhiteSpace(imagePath)) return null;

        List<string> candidates = [imagePath + ".resume.json", imagePath + ".resume.xml"];

        string directoryName            = Path.GetDirectoryName(imagePath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);

        if(string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            return candidates.Distinct().FirstOrDefault(File.Exists);

        candidates.Add(Path.Combine(directoryName, fileNameWithoutExtension + ".resume.json"));
        candidates.Add(Path.Combine(directoryName, fileNameWithoutExtension + ".resume.xml"));

        return candidates.Distinct().FirstOrDefault(File.Exists);
    }

    static bool LoadResume(string resumePath, bool explicitResumePath, out Resume resume)
    {
        resume = null;

        if(string.IsNullOrWhiteSpace(resumePath)) return true;

        if(!File.Exists(resumePath))
        {
            if(!explicitResumePath) return true;

            AaruLogging.Error(UI.Could_not_find_resume_file);

            return false;
        }

        try
        {
            if(resumePath.EndsWith(".resume.json", StringComparison.CurrentCultureIgnoreCase))
            {
                FileStream fs = new(resumePath, FileMode.Open);

                resume = (JsonSerializer.Deserialize(fs, typeof(ResumeJson), ResumeJsonContext.Default) as ResumeJson)
                  ?.Resume;

                fs.Close();
            }
            else
            {
#pragma warning disable IL2026
                XmlSerializer xs = new(typeof(Resume));
#pragma warning restore IL2026

                StreamReader sr = new(resumePath);

#pragma warning disable IL2026
                resume = (Resume)xs.Deserialize(sr);
#pragma warning restore IL2026

                sr.Close();
            }
        }
        catch(Exception ex)
        {
            AaruLogging.Error(UI.Incorrect_resume_file_cannot_use_it);
            AaruLogging.Exception(ex, UI.Incorrect_resume_file_cannot_use_it);

            return !explicitResumePath;
        }

        return true;
    }

    static void PrintAffectedExtents(IEnumerable<AffectedExtent> affectedExtents, IOpticalMediaImage opticalImage)
    {
        Table table = new()
        {
            Title = new TableTitle($"[bold]{UI.Affected_sector_extents}[/]")
        };

        table.AddColumn(new TableColumn($"[underline]{UI.Title_Type}[/]")
        {
            NoWrap = true
        });

        table.AddColumn(new TableColumn($"[underline]{Localization.Core.Title_Start}[/]")
        {
            NoWrap    = true,
            Alignment = Justify.Right
        });

        table.AddColumn(new TableColumn($"[underline]{Localization.Core.Title_End}[/]")
        {
            NoWrap    = true,
            Alignment = Justify.Right
        });

        table.AddColumn(new TableColumn($"[underline]{UI.Title_Description}[/]")
        {
            NoWrap = false
        });

        foreach(AffectedExtent affectedExtent in affectedExtents)
        {
            table.AddRow(Markup.Escape(GetAffectedExtentType(affectedExtent.Kind)),
                         affectedExtent.Start.ToString(),
                         affectedExtent.End.ToString(),
                         Markup.Escape(GetAffectedExtentDescription(affectedExtent, opticalImage)));
        }

        AnsiConsole.Write(table);
        AaruLogging.WriteLine();
    }

    static void PrintFilesystemResults(IReadOnlyFilesystem           fs, IReadOnlyList<FileSectorInfo> files,
                                       IReadOnlyList<AffectedExtent> affectedExtents)
    {
        AaruLogging.WriteLine($"[bold]{string.Format(UI.Identified_by_0, fs.Name)}[/]");

        if(files == null || files.Count == 0)
        {
            AaruLogging.WriteLine(UI.No_files_in_0_overlap_the_affected_sectors, Markup.Escape(fs.Name));
            AaruLogging.WriteLine();

            return;
        }

        bool hasXattrs = files.Any(static file => !string.IsNullOrWhiteSpace(file.Stream));

        Table table = new();

        table.AddColumn(new TableColumn(UI.Title_File_path)
        {
            NoWrap = false
        });

        table.AddColumn(new TableColumn($"[underline]{UI.Affected_sector_extents}[/]")
        {
            NoWrap = false
        });

        if(hasXattrs)
        {
            table.AddColumn(new TableColumn($"[underline]{UI.Title_Xattr_or_stream}[/]")
            {
                NoWrap = true
            });
        }

        foreach(FileSectorInfo file in files.OrderBy(static file => file.Path).ThenBy(static file => file.Stream))
        {
            var extents = string.Join(", ",
                                      file.AffectedSectors.OrderBy(static extent => extent.Start)
                                          .Select(static extent => $"{extent.Start}-{extent.End}"));

            AffectedSectorKind? fileKind = GetFileAffectedSectorKind(file, affectedExtents);

            if(hasXattrs)
            {
                table.AddRow(ColorizeCell(file.Path ?? string.Empty,   fileKind),
                             ColorizeCell(extents,                     fileKind),
                             ColorizeCell(file.Stream ?? string.Empty, fileKind));
            }
            else
                table.AddRow(ColorizeCell(file.Path ?? string.Empty, fileKind), ColorizeCell(extents, fileKind));
        }

        AnsiConsole.Write(table);
        AaruLogging.WriteLine();
    }

    static AffectedSectorKind? GetFileAffectedSectorKind(FileSectorInfo                file,
                                                         IReadOnlyList<AffectedExtent> affectedExtents)
    {
        if(file?.AffectedSectors      == null ||
           file.AffectedSectors.Count == 0    ||
           affectedExtents            == null ||
           affectedExtents.Count      == 0)
            return null;

        AffectedSectorKind[] priorities =
        [
            AffectedSectorKind.Error, AffectedSectorKind.Subchannel, AffectedSectorKind.Undumped
        ];

        foreach(AffectedSectorKind kind in priorities)
        {
            foreach((ulong Start, ulong End) fileExtent in file.AffectedSectors)
            {
                foreach(AffectedExtent affectedExtent in affectedExtents)
                {
                    if(affectedExtent.Kind != kind) continue;
                    if(fileExtent.End < affectedExtent.Start || fileExtent.Start > affectedExtent.End) continue;

                    return kind;
                }
            }
        }

        return null;
    }

    static string ColorizeCell(string value, AffectedSectorKind? kind)
    {
        string escapedValue = Markup.Escape(value ?? string.Empty);

        if(!kind.HasValue) return escapedValue;

        return $"[{GetAffectedSectorColor(kind.Value)}]{escapedValue}[/]";
    }

    static string GetAffectedSectorColor(AffectedSectorKind kind)
    {
        switch(kind)
        {
            case AffectedSectorKind.Error:
                return "red";
            case AffectedSectorKind.Subchannel:
                return "yellow3";
            default:
                return "slateblue1";
        }
    }

    static string GetAffectedExtentType(AffectedSectorKind kind)
    {
        switch(kind)
        {
            case AffectedSectorKind.Error:
                return UI.Errored_sectors;
            case AffectedSectorKind.Subchannel:
                return UI.Damaged_subchannel_sectors;
            default:
                return UI.Undumped_sectors;
        }
    }

    static string GetAffectedExtentDescription(AffectedExtent affectedExtent, IOpticalMediaImage opticalImage)
    {
        if(opticalImage == null) return Localization.Core.Whole_device;

        return DescribeOpticalExtent(affectedExtent.Start, affectedExtent.End, opticalImage);
    }

    static string DescribeOpticalExtent(ulong startSector, ulong endSector, IOpticalMediaImage opticalImage)
    {
        if(opticalImage.Tracks == null || opticalImage.Tracks.Count == 0) return UI.Outside_known_track_bounds;

        List<string>  descriptions = [];
        var           tracks = opticalImage.Tracks.OrderBy(static track => track.StartSector).ToList();
        List<Session> sessions = opticalImage.Sessions?.OrderBy(static session => session.StartSector).ToList() ?? [];
        ulong         currentSector = startSector;

        while(currentSector <= endSector)
        {
            Track currentTrack =
                tracks.FirstOrDefault(track => currentSector >= track.StartSector && currentSector <= track.EndSector);

            if(currentTrack != null)
            {
                ulong segmentEnd = Math.Min(endSector, currentTrack.EndSector);

                descriptions.Add($"[{currentSector}-{segmentEnd}] {string.Format(UI.Track_0_session_1_2,
                    currentTrack.Sequence,
                    currentTrack.Session,
                    currentTrack.Type)}");

                if(segmentEnd == ulong.MaxValue) break;

                currentSector = segmentEnd + 1;

                continue;
            }

            Track nextTrack = tracks.FirstOrDefault(track => track.StartSector > currentSector);
            ulong gapEnd    = nextTrack == null ? endSector : Math.Min(endSector, nextTrack.StartSector - 1);

            Session previousSession = sessions.Where(session => session.EndSector < currentSector)
                                              .OrderByDescending(static session => session.EndSector)
                                              .FirstOrDefault();

            Session nextSession = sessions.Where(session => session.StartSector > currentSector)
                                          .OrderBy(static session => session.StartSector)
                                          .FirstOrDefault();

            if(previousSession.Sequence > 0 && nextSession.Sequence > 0)
            {
                descriptions
                   .Add($"[{currentSector}-{gapEnd}] {string.Format(UI.Intersession_area_between_sessions_0_and_1,
                                                                    previousSession.Sequence,
                                                                    nextSession.Sequence)}");
            }
            else
                descriptions.Add($"[{currentSector}-{gapEnd}] {UI.Outside_known_track_bounds}");

            if(gapEnd == ulong.MaxValue) break;

            currentSector = gapEnd + 1;
        }

        return string.Join("; ", descriptions);
    }

#region Nested type: Settings

    public class Settings : ImageFamily
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
        [LocalizedDescription(nameof(UI.Ignore_subchannels_during_analysis))]
        [CommandOption("--ignore-subchannel|--ignore-subchannels")]
        public bool IgnoreSubchannels { get; init; }
        [LocalizedDescription(nameof(UI.Resume_file_to_use_for_analysis))]
        [CommandOption("-r|--resume-file")]
        [DefaultValue(null)]
        public string ResumeFile { get; init; }
        [LocalizedDescription(nameof(UI.Media_image_path))]
        [CommandArgument(0, "<image-path>")]
        public string ImagePath { get; init; }
    }

#endregion

#region Nested type: AffectedExtent

    struct AffectedExtent
    {
        public AffectedExtent(AffectedSectorKind kind, ulong start, ulong end)
        {
            Kind  = kind;
            Start = start;
            End   = end;
        }

        public AffectedSectorKind Kind  { get; }
        public ulong              Start { get; }
        public ulong              End   { get; }
    }

    enum AffectedSectorKind
    {
        Error,
        Undumped,
        Subchannel
    }

#endregion
}