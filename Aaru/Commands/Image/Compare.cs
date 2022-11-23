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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Globalization;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Helpers;
using Aaru.Localization;
using Spectre.Console;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;

namespace Aaru.Commands.Image;

sealed class CompareCommand : Command
{
    public CompareCommand() : base("compare", UI.Image_Compare_Command_Description)
    {
        AddAlias("cmp");

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.First_media_image_path,
            Name        = "image-path1"
        });

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Second_media_image_path,
            Name        = "image-path2"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool debug, bool verbose, string imagePath1, string imagePath2)
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

        Statistics.AddCommand("compare");

        AaruConsole.DebugWriteLine("Compare command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("Compare command", "--input1={0}", imagePath1);
        AaruConsole.DebugWriteLine("Compare command", "--input2={0}", imagePath2);
        AaruConsole.DebugWriteLine("Compare command", "--verbose={0}", verbose);

        var     filtersList  = new FiltersList();
        IFilter inputFilter1 = null;
        IFilter inputFilter2 = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_first_file_filter).IsIndeterminate();
            inputFilter1 = filtersList.GetFilter(imagePath1);
        });

        filtersList = new FiltersList();

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_second_file_filter).IsIndeterminate();
            inputFilter2 = filtersList.GetFilter(imagePath2);
        });

        if(inputFilter1 == null)
        {
            AaruConsole.ErrorWriteLine(UI.Cannot_open_first_input_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        if(inputFilter2 == null)
        {
            AaruConsole.ErrorWriteLine(UI.Cannot_open_second_input_file);

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
            AaruConsole.ErrorWriteLine(UI.First_input_file_format_not_identified);

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        if(verbose)
            AaruConsole.VerboseWriteLine(UI.First_input_file_format_identified_by_0_1, input1Format.Name,
                                         input1Format.Id);
        else
            AaruConsole.WriteLine(UI.First_input_file_format_identified_by_0, input1Format.Name);

        if(input2Format == null)
        {
            AaruConsole.ErrorWriteLine(UI.Second_input_file_format_not_identified);

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        if(verbose)
            AaruConsole.VerboseWriteLine(UI.Second_input_file_format_identified_by_0_1, input2Format.Name,
                                         input2Format.Id);
        else
            AaruConsole.WriteLine(UI.Second_input_file_format_identified_by_0, input2Format.Name);

        ErrorNumber opened1 = ErrorNumber.NoData;
        ErrorNumber opened2 = ErrorNumber.NoData;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Opening_first_image_file).IsIndeterminate();
            opened1 = input1Format.Open(inputFilter1);
        });

        if(opened1 != ErrorNumber.NoError)
        {
            AaruConsole.WriteLine(UI.Unable_to_open_first_image_format);
            AaruConsole.WriteLine(Localization.Core.Error_0, opened1);

            return (int)opened1;
        }

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Opening_second_image_file).IsIndeterminate();
            opened2 = input2Format.Open(inputFilter2);
        });

        if(opened2 != ErrorNumber.NoError)
        {
            AaruConsole.WriteLine(UI.Unable_to_open_second_image_format);
            AaruConsole.WriteLine(Localization.Core.Error_0, opened2);

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
        table.AddColumn(UI.Title_First_Media_image);
        table.AddColumn(UI.Title_Second_Media_image);
        table.Columns[0].RightAligned();

        if(verbose)
        {
            table.AddRow(UI.Title_File, Markup.Escape(imagePath1), Markup.Escape(imagePath2));
            table.AddRow(UI.Title_Media_image_format, input1Format.Name, input2Format.Name);
        }
        else
        {
            sb.AppendFormat($"[bold]{UI.Title_First_Media_image}:[/] {imagePath1}").AppendLine();
            sb.AppendFormat($"[bold]{UI.Title_Second_Media_image}:[/] {imagePath2}").AppendLine();
        }

        bool        imagesDiffer = false;
        ErrorNumber errno;

        ImageInfo                        image1Info       = input1Format.Info;
        ImageInfo                        image2Info       = input2Format.Info;
        Dictionary<MediaTagType, byte[]> image1DiskTags   = new();
        Dictionary<MediaTagType, byte[]> image2DiskTags   = new();
        var                              input1MediaImage = input1Format as IMediaImage;
        var                              input2MediaImage = input2Format as IMediaImage;

        if(input1MediaImage != null)
            foreach(MediaTagType diskTag in Enum.GetValues(typeof(MediaTagType)))
            {
                errno = input1MediaImage.ReadMediaTag(diskTag, out byte[] tempArray);

                if(errno == ErrorNumber.NoError)
                    image1DiskTags.Add(diskTag, tempArray);
            }

        if(input2MediaImage != null)
            foreach(MediaTagType diskTag in Enum.GetValues(typeof(MediaTagType)))
            {
                errno = input2MediaImage.ReadMediaTag(diskTag, out byte[] tempArray);

                if(errno == ErrorNumber.NoError)
                    image2DiskTags.Add(diskTag, tempArray);
            }

        if(verbose)
        {
            table.AddRow(UI.Has_partitions_Question, image1Info.HasPartitions.ToString(),
                         image2Info.HasPartitions.ToString());

            table.AddRow(UI.Has_sessions_Question, image1Info.HasSessions.ToString(),
                         image2Info.HasSessions.ToString());

            table.AddRow(UI.Title_Image_size, image1Info.ImageSize.ToString(), image2Info.ImageSize.ToString());

            table.AddRow(UI.Title_Sectors, image1Info.Sectors.ToString(), image2Info.Sectors.ToString());

            table.AddRow(UI.Title_Sector_size, image1Info.SectorSize.ToString(), image2Info.SectorSize.ToString());

            table.AddRow(UI.Title_Creation_time, image1Info.CreationTime.ToString(CultureInfo.CurrentCulture),
                         image2Info.CreationTime.ToString(CultureInfo.CurrentCulture));

            table.AddRow(UI.Title_Last_modification_time,
                         image1Info.LastModificationTime.ToString(CultureInfo.CurrentCulture),
                         image2Info.LastModificationTime.ToString(CultureInfo.CurrentCulture));

            table.AddRow(UI.Title_Media_type, image1Info.MediaType.ToString(), image2Info.MediaType.ToString());

            table.AddRow(UI.Title_Image_version, image1Info.Version ?? "", image2Info.Version ?? "");

            table.AddRow(UI.Title_Image_application, image1Info.Application ?? "", image2Info.Application ?? "");

            table.AddRow(UI.Title_Image_application_version, image1Info.ApplicationVersion ?? "",
                         image2Info.ApplicationVersion                                     ?? "");

            table.AddRow(UI.Title_Image_creator, image1Info.Creator ?? "", image2Info.Creator ?? "");

            table.AddRow(UI.Title_Image_name, image1Info.MediaTitle ?? "", image2Info.MediaTitle ?? "");

            table.AddRow(UI.Title_Image_comments, image1Info.Comments ?? "", image2Info.Comments ?? "");

            table.AddRow(UI.Title_Media_manufacturer, image1Info.MediaManufacturer ?? "",
                         image2Info.MediaManufacturer                              ?? "");

            table.AddRow(UI.Title_Media_model, image1Info.MediaModel ?? "", image2Info.MediaModel ?? "");

            table.AddRow(UI.Title_Media_serial_number, image1Info.MediaSerialNumber ?? "",
                         image2Info.MediaSerialNumber                               ?? "");

            table.AddRow(UI.Title_Media_barcode, image1Info.MediaBarcode ?? "", image2Info.MediaBarcode ?? "");

            table.AddRow(UI.Title_Media_part_number, image1Info.MediaPartNumber ?? "",
                         image2Info.MediaPartNumber                             ?? "");

            table.AddRow(UI.Title_Media_sequence, image1Info.MediaSequence.ToString(),
                         image2Info.MediaSequence.ToString());

            table.AddRow(UI.Title_Last_media_on_sequence, image1Info.LastMediaSequence.ToString(),
                         image2Info.LastMediaSequence.ToString());

            table.AddRow(UI.Title_Drive_manufacturer, image1Info.DriveManufacturer ?? "",
                         image2Info.DriveManufacturer                              ?? "");

            table.AddRow(UI.Title_Drive_firmware_revision, image1Info.DriveFirmwareRevision ?? "",
                         image2Info.DriveFirmwareRevision                                   ?? "");

            table.AddRow(UI.Title_Drive_model, image1Info.DriveModel ?? "", image2Info.DriveModel ?? "");

            table.AddRow(UI.Title_Drive_serial_number, image1Info.DriveSerialNumber ?? "",
                         image2Info.DriveSerialNumber                               ?? "");

            foreach(MediaTagType diskTag in
                    (Enum.GetValues(typeof(MediaTagType)) as MediaTagType[]).OrderBy(e => e.ToString()))
                table.AddRow(string.Format(UI.Has_tag_0_Question, diskTag),
                             image1DiskTags.ContainsKey(diskTag).ToString(),
                             image2DiskTags.ContainsKey(diskTag).ToString());
        }

        ulong leastSectors = 0;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Comparing_media_image_characteristics).IsIndeterminate();

            if(image1Info.HasPartitions != image2Info.HasPartitions)
            {
                imagesDiffer = true;

                if(!verbose)
                    sb.AppendLine(UI.Image_partitioned_status_differ);
            }

            if(image1Info.HasSessions != image2Info.HasSessions)
            {
                imagesDiffer = true;

                if(!verbose)
                    sb.AppendLine(UI.Image_session_status_differ);
            }

            if(image1Info.Sectors != image2Info.Sectors)
            {
                imagesDiffer = true;

                if(!verbose)
                    sb.AppendLine(UI.Image_sectors_differ);
            }

            if(image1Info.SectorSize != image2Info.SectorSize)
            {
                imagesDiffer = true;

                if(!verbose)
                    sb.AppendLine(UI.Image_sector_size_differ);
            }

            if(image1Info.MediaType != image2Info.MediaType)
            {
                imagesDiffer = true;

                if(!verbose)
                    sb.AppendLine(UI.Media_type_differs);
            }

            if(image1Info.Sectors < image2Info.Sectors)
            {
                imagesDiffer = true;
                leastSectors = image1Info.Sectors;

                if(!verbose)
                    sb.AppendLine(UI.Second_image_has_more_sectors);
            }
            else if(image1Info.Sectors > image2Info.Sectors)
            {
                imagesDiffer = true;
                leastSectors = image2Info.Sectors;

                if(!verbose)
                    sb.AppendLine(UI.First_image_has_more_sectors);
            }
            else
                leastSectors = image1Info.Sectors;
        });

        var input1ByteAddressable = input1Format as IByteAddressableImage;
        var input2ByteAddressable = input2Format as IByteAddressableImage;

        if(input1ByteAddressable is null &&
           input2ByteAddressable is not null)
            imagesDiffer = true;

        if(input1ByteAddressable is not null &&
           input2ByteAddressable is null)
            imagesDiffer = true;

        if(input1MediaImage is null &&
           input2MediaImage is not null)
            imagesDiffer = true;

        if(input1MediaImage is not null &&
           input2MediaImage is null)
            imagesDiffer = true;

        if(input1MediaImage is not null &&
           input2MediaImage is not null)
            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            ProgressTask task = ctx.AddTask(UI.Comparing_sectors);
                            task.MaxValue = leastSectors;

                            for(ulong sector = 0; sector < leastSectors; sector++)
                            {
                                task.Value       = sector;
                                task.Description = string.Format(UI.Comparing_sector_0_of_1, sector + 1, leastSectors);

                                try
                                {
                                    errno = input1MediaImage.ReadSector(sector, out byte[] image1Sector);

                                    if(errno != ErrorNumber.NoError)
                                        AaruConsole.
                                            ErrorWriteLine(string.Format(UI.Error_0_reading_sector_1_from_first_image,
                                                                         errno, sector));

                                    errno = input2MediaImage.ReadSector(sector, out byte[] image2Sector);

                                    if(errno != ErrorNumber.NoError)
                                        AaruConsole.
                                            ErrorWriteLine(string.Format(UI.Error_0_reading_sector_1_from_second_image,
                                                                         errno, sector));

                                    ArrayHelpers.CompareBytes(out bool different, out bool sameSize, image1Sector,
                                                              image2Sector);

                                    if(different)
                                        imagesDiffer = true;

                                    //       sb.AppendFormat("Sector {0} is different", sector).AppendLine();
                                    else if(!sameSize)
                                        imagesDiffer = true;
                                    /*     sb.
                                                 AppendFormat("Sector {0} has different sizes ({1} bytes in image 1, {2} in image 2) but are otherwise identical",
                                                              sector, image1Sector.LongLength, image2Sector.LongLength).AppendLine();*/
                                }
                                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                catch
                                {
                                    // ignored
                                }
                                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                            }
                        });

        if(input1ByteAddressable is not null &&
           input2ByteAddressable is not null)
            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            ProgressTask task = ctx.AddTask(UI.Comparing_images);
                            task.IsIndeterminate = true;

                            byte[] data1 = new byte[input1ByteAddressable.Info.Sectors];
                            byte[] data2 = new byte[input2ByteAddressable.Info.Sectors];
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

                            if(different)
                                imagesDiffer = true;
                            else if(!sameSize)
                                imagesDiffer = true;
                        });

        AaruConsole.WriteLine();

        sb.AppendLine(imagesDiffer ? UI.Images_differ : UI.Images_do_not_differ);

        if(verbose)
            AnsiConsole.Write(table);
        else
            AaruConsole.WriteLine(sb.ToString());

        return (int)ErrorNumber.NoError;
    }
}