// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Compare.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'compare' command.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using Aaru.Core.Graphics;
using Aaru.Core.Image;
using Aaru.Helpers;
using Aaru.Localization;
using Aaru.Logging;
using Sentry;
using Spectre.Console;
using Spectre.Console.Cli;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;

namespace Aaru.Commands.Image;

sealed class CompareCommand : Command<CompareCommand.Settings>
{
    const string MODULE_NAME = "Compare command";

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        Statistics.AddCommand("compare");

        AaruLogging.Debug(MODULE_NAME, "--debug={0}",        settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--input1={0}",       Markup.Escape(settings.ImagePath1 ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--input2={0}",       Markup.Escape(settings.ImagePath2 ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--long-sectors={0}", settings.LongSectors);
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}",      settings.Verbose);
        AaruLogging.Debug(MODULE_NAME, "--dpm-graph={0}",    Markup.Escape(settings.DpmGraph ?? ""));

        IFilter inputFilter1 = null;
        IFilter inputFilter2 = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_first_file_filter).IsIndeterminate();
            inputFilter1 = PluginRegister.Singleton.GetFilter(settings.ImagePath1);
        });

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_second_file_filter).IsIndeterminate();
            inputFilter2 = PluginRegister.Singleton.GetFilter(settings.ImagePath2);
        });

        if(inputFilter1 == null)
        {
            AaruLogging.Error(UI.Cannot_open_first_input_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        if(inputFilter2 == null)
        {
            AaruLogging.Error(UI.Cannot_open_second_input_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        IBaseImage input1Format = null;
        IBaseImage input2Format = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_first_image_format).IsIndeterminate();
            input1Format = ImageFormat.Detect(inputFilter1);
        });

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_second_image_format).IsIndeterminate();
            input2Format = ImageFormat.Detect(inputFilter2);
        });

        if(input1Format == null)
        {
            AaruLogging.Error(UI.First_input_file_format_not_identified);

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        if(settings.Verbose)
            AaruLogging.Verbose(UI.First_input_file_format_identified_by_0_1, input1Format.Name, input1Format.Id);
        else
            AaruLogging.WriteLine(UI.First_input_file_format_identified_by_0, input1Format.Name);

        if(input2Format == null)
        {
            AaruLogging.Error(UI.Second_input_file_format_not_identified);

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        if(settings.Verbose)
            AaruLogging.Verbose(UI.Second_input_file_format_identified_by_0_1, input2Format.Name, input2Format.Id);
        else
            AaruLogging.WriteLine(UI.Second_input_file_format_identified_by_0, input2Format.Name);

        ErrorNumber opened1 = ErrorNumber.NoData;
        ErrorNumber opened2 = ErrorNumber.NoData;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Opening_first_image_file).IsIndeterminate();
            opened1 = input1Format.Open(inputFilter1);
        });

        if(opened1 != ErrorNumber.NoError)
        {
            AaruLogging.WriteLine(UI.Unable_to_open_first_image_format);
            AaruLogging.WriteLine(Localization.Core.Error_0, opened1);

            return (int)opened1;
        }

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Opening_second_image_file).IsIndeterminate();
            opened2 = input2Format.Open(inputFilter2);
        });

        if(opened2 != ErrorNumber.NoError)
        {
            AaruLogging.WriteLine(UI.Unable_to_open_second_image_format);
            AaruLogging.WriteLine(Localization.Core.Error_0, opened2);

            return (int)opened2;
        }

        Statistics.AddMediaFormat(input1Format.Format);
        Statistics.AddMediaFormat(input2Format.Format);
        Statistics.AddMedia(input1Format.Info.MediaType, false);
        Statistics.AddMedia(input2Format.Info.MediaType, false);
        Statistics.AddFilter(inputFilter1.Name);
        Statistics.AddFilter(inputFilter2.Name);

        var   sb    = new StringBuilder();
        Table table = new();
        table.AddColumn("");
        table.AddColumn(new TableColumn(new Markup(UI.Title_First_Media_image).Centered()));
        table.AddColumn(new TableColumn(new Markup(UI.Title_Second_Media_image).Centered()));
        table.Columns[0].RightAligned();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Yellow);

        if(settings.Verbose)
        {
            table.AddRow(UI.Title_File,
                         $"[navy]{Markup.Escape(settings.ImagePath1)}[/]",
                         $"[navy]{Markup.Escape(settings.ImagePath2)}[/]");

            table.AddRow(UI.Title_Media_image_format,
                         $"[fuchsia]{input1Format.Name}[/]",
                         $"[fuchsia]{input2Format.Name}[/]");
        }
        else
        {
            sb
               .AppendFormat($"[bold][green]{UI.Title_First_Media_image}:[/] [italic]{Markup.Escape(settings.ImagePath1)}[/][/]")
               .AppendLine();

            sb
               .AppendFormat($"[bold][red]{UI.Title_Second_Media_image}:[/] [italic]{Markup.Escape(settings.ImagePath2)}[/][/]")
               .AppendLine();
        }

        var         imagesDiffer = false;
        ErrorNumber errno;

        ImageInfo                        image1Info       = input1Format.Info;
        ImageInfo                        image2Info       = input2Format.Info;
        Dictionary<MediaTagType, byte[]> image1DiskTags   = [];
        Dictionary<MediaTagType, byte[]> image2DiskTags   = [];
        var                              input1MediaImage = input1Format as IMediaImage;
        var                              input2MediaImage = input2Format as IMediaImage;

        if(input1MediaImage != null)
        {
            foreach(MediaTagType diskTag in Enum.GetValues(typeof(MediaTagType)))
            {
                errno = input1MediaImage.ReadMediaTag(diskTag, out byte[] tempArray);

                if(errno == ErrorNumber.NoError) image1DiskTags.Add(diskTag, tempArray);
            }
        }

        if(input2MediaImage != null)
        {
            foreach(MediaTagType diskTag in Enum.GetValues(typeof(MediaTagType)))
            {
                errno = input2MediaImage.ReadMediaTag(diskTag, out byte[] tempArray);

                if(errno == ErrorNumber.NoError) image2DiskTags.Add(diskTag, tempArray);
            }
        }

        if(settings.Verbose)
        {
            table.AddRow(UI.Has_partitions_Question,
                         image1Info.HasPartitions
                             ? $"[green]{image1Info.HasPartitions}[/]"
                             : $"[red]{image1Info.HasPartitions}[/]",
                         image2Info.HasPartitions
                             ? $"[green]{image2Info.HasPartitions}[/]"
                             : $"[red]{image2Info.HasPartitions}[/]");

            table.AddRow(UI.Has_sessions_Question,
                         image1Info.HasSessions
                             ? $"[green]{image1Info.HasSessions}[/]"
                             : $"[red]{image1Info.HasSessions}[/]",
                         image2Info.HasSessions
                             ? $"[green]{image2Info.HasSessions}[/]"
                             : $"[red]{image2Info.HasSessions}[/]");

            table.AddRow(UI.Title_Image_size, $"[teal]{image1Info.ImageSize}[/]", $"[teal]{image2Info.ImageSize}[/]");

            table.AddRow(UI.Title_Sectors, $"[lime]{image1Info.Sectors}[/]", $"[lime]{image2Info.Sectors}[/]");

            table.AddRow(UI.Title_Sector_size,
                         $"[aqua]{image1Info.SectorSize}[/]",
                         $"[aqua]{image2Info.SectorSize}[/]");

            table.AddRow(UI.Title_Creation_time,
                         $"[yellow3]{image1Info.CreationTime.ToString(CultureInfo.CurrentCulture)}[/]",
                         $"[yellow3]{image2Info.CreationTime.ToString(CultureInfo.CurrentCulture)}[/]");

            table.AddRow(UI.Title_Last_modification_time,
                         $"[yellow3]{image1Info.LastModificationTime.ToString(CultureInfo.CurrentCulture)}[/]",
                         $"[yellow3]{image2Info.LastModificationTime.ToString(CultureInfo.CurrentCulture)}[/]");

            table.AddRow(UI.Title_Media_type,
                         $"[orange3]{image1Info.MediaType}[/]",
                         $"[orange3]{image2Info.MediaType}[/]");

            table.AddRow(UI.Title_Image_version,
                         $"[rosybrown]{image1Info.Version ?? ""}[/]",
                         $"[rosybrown]{image2Info.Version ?? ""}[/]");

            table.AddRow(UI.Title_Image_application,
                         $"[fuchsia]{image1Info.Application ?? ""}[/]",
                         $"[fuchsia]{image2Info.Application ?? ""}[/]");

            table.AddRow(UI.Title_Image_application_version,
                         $"[rosybrown]{image1Info.ApplicationVersion ?? ""}[/]",
                         $"[rosybrown]{image2Info.ApplicationVersion ?? ""}[/]");

            table.AddRow(UI.Title_Image_creator,
                         $"[blue]{Markup.Escape(image1Info.Creator ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.Creator ?? "")}[/]");

            table.AddRow(UI.Title_Image_name,
                         $"[blue]{Markup.Escape(image1Info.MediaTitle ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.MediaTitle ?? "")}[/]");

            table.AddRow(UI.Title_Image_comments,
                         $"[blue]{Markup.Escape(image1Info.Comments ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.Comments ?? "")}[/]");

            table.AddRow(UI.Title_Media_manufacturer,
                         $"[blue]{Markup.Escape(image1Info.MediaManufacturer ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.MediaManufacturer ?? "")}[/]");

            table.AddRow(UI.Title_Media_model,
                         $"[blue]{Markup.Escape(image1Info.MediaModel ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.MediaModel ?? "")}[/]");

            table.AddRow(UI.Title_Media_serial_number,
                         $"[blue]{Markup.Escape(image1Info.MediaSerialNumber ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.MediaSerialNumber ?? "")}[/]");

            table.AddRow(UI.Title_Media_barcode,
                         $"[blue]{Markup.Escape(image1Info.MediaBarcode ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.MediaBarcode ?? "")}[/]");

            table.AddRow(UI.Title_Media_part_number,
                         $"[blue]{Markup.Escape(image1Info.MediaPartNumber ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.MediaPartNumber ?? "")}[/]");

            table.AddRow(UI.Title_Media_sequence,
                         $"[teal]{image1Info.MediaSequence}[/]",
                         $"[teal]{image2Info.MediaSequence}[/]");

            table.AddRow(UI.Title_Last_media_on_sequence,
                         $"[teal]{image1Info.LastMediaSequence}[/]",
                         $"[teal]{image2Info.LastMediaSequence}[/]");

            table.AddRow(UI.Title_Drive_manufacturer,
                         $"[blue]{Markup.Escape(image1Info.DriveManufacturer ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.DriveManufacturer ?? "")}[/]");

            table.AddRow(UI.Title_Drive_firmware_revision,
                         $"[blue]{Markup.Escape(image1Info.DriveFirmwareRevision ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.DriveFirmwareRevision ?? "")}[/]");

            table.AddRow(UI.Title_Drive_model,
                         $"[blue]{Markup.Escape(image1Info.DriveModel ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.DriveModel ?? "")}[/]");

            table.AddRow(UI.Title_Drive_serial_number,
                         $"[blue]{Markup.Escape(image1Info.DriveSerialNumber ?? "")}[/]",
                         $"[blue]{Markup.Escape(image2Info.DriveSerialNumber ?? "")}[/]");

            foreach(MediaTagType diskTag in
                    (Enum.GetValues(typeof(MediaTagType)) as MediaTagType[]).OrderBy(static e => e.ToString()))
            {
                table.AddRow(string.Format(UI.Has_tag_0_Question, diskTag),
                             image1DiskTags.ContainsKey(diskTag)
                                 ? $"[green]{image1DiskTags.ContainsKey(diskTag)}[/]"
                                 : $"[red]{image1DiskTags.ContainsKey(diskTag)}[/]",
                             image2DiskTags.ContainsKey(diskTag)
                                 ? $"[green]{image2DiskTags.ContainsKey(diskTag)}[/]"
                                 : $"[red]{image2DiskTags.ContainsKey(diskTag)}[/]");
            }
        }

        ulong leastSectors = 0;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Comparing_media_image_characteristics).IsIndeterminate();

            if(image1Info.HasPartitions != image2Info.HasPartitions)
            {
                imagesDiffer = true;

                if(!settings.Verbose) sb.AppendLine(UI.Image_partitioned_status_differ);
            }

            if(image1Info.HasSessions != image2Info.HasSessions)
            {
                imagesDiffer = true;

                if(!settings.Verbose) sb.AppendLine(UI.Image_session_status_differ);
            }

            if(image1Info.Sectors != image2Info.Sectors)
            {
                imagesDiffer = true;

                if(!settings.Verbose) sb.AppendLine(UI.Image_sectors_differ);
            }

            if(image1Info.SectorSize != image2Info.SectorSize)
            {
                imagesDiffer = true;

                if(!settings.Verbose) sb.AppendLine(UI.Image_sector_size_differ);
            }

            if(image1Info.MediaType != image2Info.MediaType)
            {
                imagesDiffer = true;

                if(!settings.Verbose) sb.AppendLine(UI.Media_type_differs);
            }

            if(image1Info.Sectors < image2Info.Sectors)
            {
                imagesDiffer = true;
                leastSectors = image1Info.Sectors;

                if(!settings.Verbose) sb.AppendLine(UI.Second_image_has_more_sectors);
            }
            else if(image1Info.Sectors > image2Info.Sectors)
            {
                imagesDiffer = true;
                leastSectors = image2Info.Sectors;

                if(!settings.Verbose) sb.AppendLine(UI.First_image_has_more_sectors);
            }
            else
                leastSectors = image1Info.Sectors;
        });

        var input1ByteAddressable = input1Format as IByteAddressableImage;
        var input2ByteAddressable = input2Format as IByteAddressableImage;

        if(input1ByteAddressable is null && input2ByteAddressable is not null) imagesDiffer = true;

        if(input1ByteAddressable is not null && input2ByteAddressable is null) imagesDiffer = true;

        if(input1MediaImage is null && input2MediaImage is not null) imagesDiffer = true;

        if(input1MediaImage is not null && input2MediaImage is null) imagesDiffer = true;

        var differentContents = false;

        if(input1MediaImage is not null && input2MediaImage is not null)
        {
            AnsiConsole.Progress()
                       .AutoClear(true)
                       .HideCompleted(true)
                       .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                       .Start(ctx =>
                        {
                            ProgressTask task = ctx.AddTask(UI.Comparing_sectors);
                            task.MaxValue = leastSectors;

                            for(ulong sector = 0; sector < leastSectors; sector++)
                            {
                                task.Value       = sector;
                                task.Description = string.Format(UI.Comparing_sector_0_of_1, sector + 1, leastSectors);

                                try
                                {
                                    errno = settings.LongSectors
                                                ? input1MediaImage.ReadSectorLong(sector,
                                                    false,
                                                    out byte[] image1Sector,
                                                    out _)
                                                : input1MediaImage.ReadSector(sector, false, out image1Sector, out _);

                                    if(errno != ErrorNumber.NoError)
                                    {
                                        AaruLogging.Error(string.Format(UI.Error_0_reading_sector_1_from_first_image,
                                                                        errno,
                                                                        sector));
                                    }

                                    errno = settings.LongSectors
                                                ? input2MediaImage.ReadSectorLong(sector,
                                                    false,
                                                    out byte[] image2Sector,
                                                    out _)
                                                : input2MediaImage.ReadSector(sector, false, out image2Sector, out _);

                                    if(errno != ErrorNumber.NoError)
                                    {
                                        AaruLogging.Error(string.Format(UI.Error_0_reading_sector_1_from_second_image,
                                                                        errno,
                                                                        sector));
                                    }

                                    ArrayHelpers.CompareBytes(out bool different,
                                                              out bool sameSize,
                                                              image1Sector,
                                                              image2Sector);

                                    if(different)
                                    {
                                        imagesDiffer      = true;
                                        differentContents = true;
                                        AaruLogging.Debug(MODULE_NAME, "Sector {0} is different", sector);
                                    }
                                    else if(!sameSize)
                                    {
                                        imagesDiffer      = true;
                                        differentContents = true;

                                        AaruLogging.Debug(MODULE_NAME,
                                                          "Sector {0} has different sizes ({1} bytes in image 1, {2} in image 2) but are otherwise identical",
                                                          sector,
                                                          image1Sector.LongLength,
                                                          image2Sector.LongLength);
                                    }
                                }
                                catch(Exception ex)
                                {
                                    SentrySdk.CaptureException(ex);
                                }
                            }
                        });
        }

        if(input1ByteAddressable is not null && input2ByteAddressable is not null)
        {
            AnsiConsole.Progress()
                       .AutoClear(true)
                       .HideCompleted(true)
                       .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                       .Start(ctx =>
                        {
                            ProgressTask task = ctx.AddTask(UI.Comparing_images);
                            task.IsIndeterminate = true;

                            var    data1 = new byte[input1ByteAddressable.Info.Sectors];
                            var    data2 = new byte[input2ByteAddressable.Info.Sectors];
                            byte[] tmp;

                            input1ByteAddressable.ReadBytes(data1, 0, data1.Length, out int bytesRead);

                            if(bytesRead != data1.Length)
                            {
                                tmp = new byte[bytesRead];
                                Array.Copy(data1, 0, tmp, 0, bytesRead);
                                data1 = tmp;
                            }

                            input2ByteAddressable.ReadBytes(data2, 0, data2.Length, out bytesRead);

                            if(bytesRead != data2.Length)
                            {
                                tmp = new byte[bytesRead];
                                Array.Copy(data2, 0, tmp, 0, bytesRead);
                                data2 = tmp;
                            }

                            ArrayHelpers.CompareBytes(out bool different, out bool sameSize, data1, data2);

                            if(!different && sameSize) return;

                            imagesDiffer      = true;
                            differentContents = true;
                        });
        }

        AaruLogging.WriteLine();

        sb.AppendLine(imagesDiffer ? UI.Images_differ : UI.Images_do_not_differ);

        sb.AppendLine(differentContents ? UI.Images_have_different_contents : UI.Images_have_identical_contents);

        if(settings.Verbose)
            AnsiConsole.Write(table);
        else
            AaruLogging.WriteLine(sb.ToString());

        if(input1Format is IOpticalMediaImage input1Optical && input2Format is IOpticalMediaImage input2Optical)
            CompareDpm(input1Optical, input2Optical, settings);

        return (int)ErrorNumber.NoError;
    }

    /// <summary>Compares, only to report it, the Data Position Measurements of two images</summary>
    static void CompareDpm(IOpticalMediaImage image1, IOpticalMediaImage image2, Settings settings)
    {
        bool has1 = image1.ReadDpm(out DataPositionMeasurement dpm1) == ErrorNumber.NoError;
        bool has2 = image2.ReadDpm(out DataPositionMeasurement dpm2) == ErrorNumber.NoError;

        if(!has1 && !has2) return;

        if(!has2)
        {
            AaruLogging.WriteLine(UI.Only_first_image_has_DPM);

            return;
        }

        if(!has1)
        {
            AaruLogging.WriteLine(UI.Only_second_image_has_DPM);

            return;
        }

        DpmComparisonResult result = DpmComparison.Compare(dpm1, dpm2);

        AaruLogging.WriteLine();
        AaruLogging.WriteLine($"[bold]{UI.Title_DPM_comparison}[/]");

        if(result.TimingUnitDiffers) AaruLogging.WriteLine(UI.DPM_timing_units_differ);

        if(result.TrackPathDiffers) AaruLogging.WriteLine(UI.DPM_track_paths_differ);

        if(result.LayersDiffer) AaruLogging.WriteLine(UI.DPM_layer_boundaries_differ);

        if(result.SpeedDiffers) AaruLogging.WriteLine(UI.DPM_speeds_differ);

        if(result.SpacingDiffers) AaruLogging.WriteLine(UI.DPM_spacings_differ_compared_every_0_sectors, result.Spacing);

        if(result.Verdict == DpmComparisonVerdict.NotComparable)
        {
            AaruLogging.WriteLine(UI.DPM_not_comparable_WithMarkup);

            return;
        }

        Table table = new();
        table.AddColumn("");
        table.AddColumn("");
        table.HideHeaders();
        table.Columns[0].RightAligned();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Yellow);

        table.AddRow(UI.Title_DPM_compared_range,
                     $"[lime]{string.Format(UI.DPM_sectors_0_to_1_every_2, result.FirstLba, result.LastLba, result.Spacing)}[/]");

        table.AddRow(UI.Title_DPM_bins,
                     $"[teal]{string.Format(UI.DPM_0_bins_1_not_measured, result.Bins, result.ExcludedBins)}[/]");

        string[] scales = new string[result.Scales.Length];

        for(int i = 0; i < scales.Length; i++)
        {
            scales[i] = string.Format(UI.DPM_scale_0_1_ppm, result.Scales[i], (result.Scales[i] - 1.0) * 1e6);

            if(scales.Length > 1) scales[i] = string.Format(UI.DPM_layer_0_1, i, scales[i]);
        }

        table.AddRow(UI.Title_DPM_calibration_scale, $"[aqua]{string.Join(", ", scales)}[/]");

        table.AddRow(UI.Title_DPM_noise,
                     $"[aqua]{string.Format(UI.DPM_noise_0_1, result.Noise1, result.Noise2)}[/]");

        table.AddRow(UI.Title_DPM_threshold, $"[aqua]{result.Threshold:P2}[/]");

        table.AddRow(UI.Title_DPM_deviation,
                     $"[aqua]{Markup.Escape(string.Format(UI.DPM_deviation_0_1_2, result.MedianDeviation, result.Percentile95Deviation, result.MaximumDeviation))}[/]");

        table.AddRow(UI.Title_DPM_isolated_outliers, $"[teal]{result.IsolatedOutliers}[/]");

        AnsiConsole.Write(table);

        AaruLogging.WriteLine(result.Verdict == DpmComparisonVerdict.Match
                                  ? UI.DPM_match_WithMarkup
                                  : string.Format(UI.DPM_differ_in_0_regions_WithMarkup, result.Differences.Length));

        if(result.Differences.Length > 0)
        {
            const int maxRegions = 10;

            Table regions = new()
            {
                Title = new TableTitle(UI.Title_DPM_differing_regions)
            };

            regions.AddColumn(UI.Title_DPM_first_sector);
            regions.AddColumn(UI.Title_DPM_last_sector);
            regions.AddColumn(UI.Title_DPM_mean_deviation);
            regions.AddColumn(UI.Title_DPM_maximum_deviation);

            foreach(TableColumn column in regions.Columns) column.RightAligned();

            regions.Border(TableBorder.Rounded);
            regions.BorderColor(Color.Yellow);

            int shown = settings.Verbose ? result.Differences.Length : Math.Min(result.Differences.Length, maxRegions);

            for(int i = 0; i < shown; i++)
            {
                DpmDifference difference = result.Differences[i];

                regions.AddRow($"[lime]{difference.FirstLba}[/]",
                               $"[lime]{difference.LastLba}[/]",
                               $"[aqua]{difference.MeanDeviation:+0.00%;-0.00%}[/]",
                               $"[aqua]{difference.MaximumDeviation:+0.00%;-0.00%}[/]");
            }

            AnsiConsole.Write(regions);

            if(shown < result.Differences.Length)
                AaruLogging.WriteLine(UI.DPM_and_0_more_regions, result.Differences.Length - shown);
        }

        if(string.IsNullOrWhiteSpace(settings.DpmGraph)) return;

        try
        {
            DpmGraph.WriteComparison(result,
                                     settings.DpmGraph,
                                     Path.GetFileName(settings.ImagePath1),
                                     Path.GetFileName(settings.ImagePath2));

            AaruLogging.WriteLine(UI.Written_DPM_comparison_graph_to_0, Markup.Escape(settings.DpmGraph));
        }
        catch(Exception ex)
        {
            AaruLogging.Exception(ex, Localization.Core.Error_0, ex.Message);
        }
    }

#region Nested type: Settings

    public class Settings : ImageFamily
    {
        [LocalizedDescription(nameof(UI.First_media_image_path))]
        [CommandArgument(0, "<image-path1>")]
        public string ImagePath1 { get; init; }
        [LocalizedDescription(nameof(UI.Second_media_image_path))]
        [CommandArgument(1, "<image-path1>")]
        public string ImagePath2 { get; init; }
        [LocalizedDescription(nameof(UI.Use_long_sectors))]
        [DefaultValue(false)]
        [CommandOption("-r|--long-sectors")]
        public bool LongSectors { get; init; }
        [LocalizedDescription(nameof(UI.DPM_graph_help))]
        [DefaultValue(null)]
        [CommandOption("--dpm-graph")]
        public string DpmGraph { get; init; }
    }

#endregion
}