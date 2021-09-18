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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Helpers;
using Spectre.Console;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;

namespace Aaru.Commands.Image
{
    internal sealed class CompareCommand : Command
    {
        public CompareCommand() : base("compare", "Compares two disc images.")
        {
            AddAlias("cmp");

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "First media image path",
                Name        = "image-path1"
            });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Second media image path",
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
                ctx.AddTask("Identifying file 1 filter...").IsIndeterminate();
                inputFilter1 = filtersList.GetFilter(imagePath1);
            });

            filtersList = new FiltersList();

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Identifying file 2 filter...").IsIndeterminate();
                inputFilter2 = filtersList.GetFilter(imagePath2);
            });

            if(inputFilter1 == null)
            {
                AaruConsole.ErrorWriteLine("Cannot open input file 1");

                return (int)ErrorNumber.CannotOpenFile;
            }

            if(inputFilter2 == null)
            {
                AaruConsole.ErrorWriteLine("Cannot open input file 2");

                return (int)ErrorNumber.CannotOpenFile;
            }

            IMediaImage input1Format = null;
            IMediaImage input2Format = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Identifying image 1 format...").IsIndeterminate();
                input1Format = ImageFormat.Detect(inputFilter1);
            });

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Identifying image 1 format...").IsIndeterminate();
                input2Format = ImageFormat.Detect(inputFilter2);
            });

            if(input1Format == null)
            {
                AaruConsole.ErrorWriteLine("Input file 1 format not identified, not proceeding with comparison.");

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            if(verbose)
                AaruConsole.VerboseWriteLine("Input file 1 format identified by {0} ({1}).", input1Format.Name,
                                             input1Format.Id);
            else
                AaruConsole.WriteLine("Input file 1 format identified by {0}.", input1Format.Name);

            if(input2Format == null)
            {
                AaruConsole.ErrorWriteLine("Input file 2 format not identified, not proceeding with comparison.");

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            if(verbose)
                AaruConsole.VerboseWriteLine("Input file 2 format identified by {0} ({1}).", input2Format.Name,
                                             input2Format.Id);
            else
                AaruConsole.WriteLine("Input file 2 format identified by {0}.", input2Format.Name);

            ErrorNumber opened1 = ErrorNumber.NoData;
            ErrorNumber opened2 = ErrorNumber.NoData;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Opening image 1 file...").IsIndeterminate();
                opened1 = input1Format.Open(inputFilter1);
            });

            if(opened1 != ErrorNumber.NoError)
            {
                AaruConsole.WriteLine("Unable to open image 1 format");
                AaruConsole.WriteLine("Error {0}", opened1);

                return (int)opened1;
            }

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Opening image 2 file...").IsIndeterminate();
                opened2 = input2Format.Open(inputFilter2);
            });

            if(opened2 != ErrorNumber.NoError)
            {
                AaruConsole.WriteLine("Unable to open image 2 format");
                AaruConsole.WriteLine("Error {0}", opened2);

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
            table.AddColumn("Disc image 1");
            table.AddColumn("Disc image 2");
            table.Columns[0].RightAligned();

            if(verbose)
            {
                table.AddRow("File", Markup.Escape(imagePath1), Markup.Escape(imagePath2));
                table.AddRow("Disc image format", input1Format.Name, input2Format.Name);
            }
            else
            {
                sb.AppendFormat("[bold]Disc image 1:[/] {0}", imagePath1).AppendLine();
                sb.AppendFormat("[bold]Disc image 2:[/] {0}", imagePath2).AppendLine();
            }

            bool        imagesDiffer = false;
            ErrorNumber errno;

            ImageInfo                        image1Info     = input1Format.Info;
            ImageInfo                        image2Info     = input2Format.Info;
            Dictionary<MediaTagType, byte[]> image1DiskTags = new();
            Dictionary<MediaTagType, byte[]> image2DiskTags = new();

            foreach(MediaTagType diskTag in Enum.GetValues(typeof(MediaTagType)))
            {
                errno = input1Format.ReadMediaTag(diskTag, out byte[] tempArray);

                if(errno == ErrorNumber.NoError)
                    image1DiskTags.Add(diskTag, tempArray);
            }

            foreach(MediaTagType diskTag in Enum.GetValues(typeof(MediaTagType)))
            {
                errno = input2Format.ReadMediaTag(diskTag, out byte[] tempArray);

                if(errno == ErrorNumber.NoError)
                    image2DiskTags.Add(diskTag, tempArray);
            }

            if(verbose)
            {
                table.AddRow("Has partitions?", image1Info.HasPartitions.ToString(),
                             image2Info.HasPartitions.ToString());

                table.AddRow("Has sessions?", image1Info.HasSessions.ToString(), image2Info.HasSessions.ToString());

                table.AddRow("Image size", image1Info.ImageSize.ToString(), image2Info.ImageSize.ToString());

                table.AddRow("Sectors", image1Info.Sectors.ToString(), image2Info.Sectors.ToString());

                table.AddRow("Sector size", image1Info.SectorSize.ToString(), image2Info.SectorSize.ToString());

                table.AddRow("Creation time", image1Info.CreationTime.ToString(), image2Info.CreationTime.ToString());

                table.AddRow("Last modification time", image1Info.LastModificationTime.ToString(),
                             image2Info.LastModificationTime.ToString());

                table.AddRow("Disk type", image1Info.MediaType.ToString(), image2Info.MediaType.ToString());

                table.AddRow("Image version", image1Info.Version ?? "", image2Info.Version ?? "");

                table.AddRow("Image application", image1Info.Application ?? "", image2Info.Application ?? "");

                table.AddRow("Image application version", image1Info.ApplicationVersion ?? "",
                             image2Info.ApplicationVersion                              ?? "");

                table.AddRow("Image creator", image1Info.Creator ?? "", image2Info.Creator ?? "");

                table.AddRow("Image name", image1Info.MediaTitle ?? "", image2Info.MediaTitle ?? "");

                table.AddRow("Image comments", image1Info.Comments ?? "", image2Info.Comments ?? "");

                table.AddRow("Disk manufacturer", image1Info.MediaManufacturer ?? "",
                             image2Info.MediaManufacturer                      ?? "");

                table.AddRow("Disk model", image1Info.MediaModel ?? "", image2Info.MediaModel ?? "");

                table.AddRow("Disk serial number", image1Info.MediaSerialNumber ?? "",
                             image2Info.MediaSerialNumber                       ?? "");

                table.AddRow("Disk barcode", image1Info.MediaBarcode ?? "", image2Info.MediaBarcode ?? "");

                table.AddRow("Disk part no.", image1Info.MediaPartNumber ?? "", image2Info.MediaPartNumber ?? "");

                table.AddRow("Disk sequence", image1Info.MediaSequence.ToString(), image2Info.MediaSequence.ToString());

                table.AddRow("Last disk on sequence", image1Info.LastMediaSequence.ToString(),
                             image2Info.LastMediaSequence.ToString());

                table.AddRow("Drive manufacturer", image1Info.DriveManufacturer ?? "",
                             image2Info.DriveManufacturer                       ?? "");

                table.AddRow("Drive firmware revision", image1Info.DriveFirmwareRevision ?? "",
                             image2Info.DriveFirmwareRevision                            ?? "");

                table.AddRow("Drive model", image1Info.DriveModel ?? "", image2Info.DriveModel ?? "");

                table.AddRow("Drive serial number", image1Info.DriveSerialNumber ?? "",
                             image2Info.DriveSerialNumber                        ?? "");

                foreach(MediaTagType diskTag in
                    (Enum.GetValues(typeof(MediaTagType)) as MediaTagType[]).OrderBy(e => e.ToString()))
                    table.AddRow($"Has {diskTag}?", image1DiskTags.ContainsKey(diskTag).ToString(),
                                 image2DiskTags.ContainsKey(diskTag).ToString());
            }

            ulong leastSectors = 0;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Comparing disk image characteristics").IsIndeterminate();

                if(image1Info.HasPartitions != image2Info.HasPartitions)
                {
                    imagesDiffer = true;

                    if(!verbose)
                        sb.AppendLine("Image partitioned status differ");
                }

                if(image1Info.HasSessions != image2Info.HasSessions)
                {
                    imagesDiffer = true;

                    if(!verbose)
                        sb.AppendLine("Image session status differ");
                }

                if(image1Info.Sectors != image2Info.Sectors)
                {
                    imagesDiffer = true;

                    if(!verbose)
                        sb.AppendLine("Image sectors differ");
                }

                if(image1Info.SectorSize != image2Info.SectorSize)
                {
                    imagesDiffer = true;

                    if(!verbose)
                        sb.AppendLine("Image sector size differ");
                }

                if(image1Info.MediaType != image2Info.MediaType)
                {
                    imagesDiffer = true;

                    if(!verbose)
                        sb.AppendLine("Disk type differ");
                }

                if(image1Info.Sectors < image2Info.Sectors)
                {
                    imagesDiffer = true;
                    leastSectors = image1Info.Sectors;

                    if(!verbose)
                        sb.AppendLine("Image 2 has more sectors");
                }
                else if(image1Info.Sectors > image2Info.Sectors)
                {
                    imagesDiffer = true;
                    leastSectors = image2Info.Sectors;

                    if(!verbose)
                        sb.AppendLine("Image 1 has more sectors");
                }
                else
                    leastSectors = image1Info.Sectors;
            });

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            ProgressTask task = ctx.AddTask("Comparing sectors...");
                            task.MaxValue = leastSectors;

                            for(ulong sector = 0; sector < leastSectors; sector++)
                            {
                                task.Value       = sector;
                                task.Description = $"Comparing sector {sector + 1} of {leastSectors}...";

                                try
                                {
                                    byte[] image1Sector = input1Format.ReadSector(sector);
                                    byte[] image2Sector = input2Format.ReadSector(sector);

                                    ArrayHelpers.CompareBytes(out bool different, out bool sameSize, image1Sector,
                                                              image2Sector);

                                    if(different)
                                    {
                                        imagesDiffer = true;

                                        //       sb.AppendFormat("Sector {0} is different", sector).AppendLine();
                                    }
                                    else if(!sameSize)
                                    {
                                        imagesDiffer = true;

                                        /*     sb.
                                                 AppendFormat("Sector {0} has different sizes ({1} bytes in image 1, {2} in image 2) but are otherwise identical",
                                                              sector, image1Sector.LongLength, image2Sector.LongLength).AppendLine();*/
                                    }
                                }
                                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                catch
                                {
                                    // ignored
                                }
                                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                            }
                        });

            AaruConsole.WriteLine();

            sb.AppendLine(imagesDiffer ? "Images differ" : "Images do not differ");

            if(verbose)
                AnsiConsole.Render(table);
            else
                AaruConsole.WriteLine(sb.ToString());

            return (int)ErrorNumber.NoError;
        }
    }
}