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
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Helpers;
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
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("compare");

            AaruConsole.DebugWriteLine("Compare command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Compare command", "--input1={0}", imagePath1);
            AaruConsole.DebugWriteLine("Compare command", "--input2={0}", imagePath2);
            AaruConsole.DebugWriteLine("Compare command", "--verbose={0}", verbose);

            var     filtersList  = new FiltersList();
            IFilter inputFilter1 = filtersList.GetFilter(imagePath1);
            filtersList = new FiltersList();
            IFilter inputFilter2 = filtersList.GetFilter(imagePath2);

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

            IMediaImage input1Format = ImageFormat.Detect(inputFilter1);
            IMediaImage input2Format = ImageFormat.Detect(inputFilter2);

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

            input1Format.Open(inputFilter1);
            input2Format.Open(inputFilter2);

            Statistics.AddMediaFormat(input1Format.Format);
            Statistics.AddMediaFormat(input2Format.Format);
            Statistics.AddMedia(input1Format.Info.MediaType, false);
            Statistics.AddMedia(input2Format.Info.MediaType, false);
            Statistics.AddFilter(inputFilter1.Name);
            Statistics.AddFilter(inputFilter2.Name);

            var sb = new StringBuilder();

            if(verbose)
            {
                sb.AppendFormat("{0,50}{1,20}", "Disc image 1", "Disc image 2").AppendLine();
                sb.AppendLine("================================================================================================");
                sb.AppendFormat("{0,-38}{1}", "File", imagePath1).AppendLine();

                sb.AppendFormat("                                                          {0}", imagePath2).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Disc image format", input1Format.Name, input2Format.Name).
                   AppendLine();
            }
            else
            {
                sb.AppendFormat("Disc image 1: {0}", imagePath1).AppendLine();
                sb.AppendFormat("Disc image 2: {0}", imagePath2).AppendLine();
            }

            bool imagesDiffer = false;

            ImageInfo                        image1Info     = input1Format.Info;
            ImageInfo                        image2Info     = input2Format.Info;
            Dictionary<MediaTagType, byte[]> image1DiskTags = new Dictionary<MediaTagType, byte[]>();
            Dictionary<MediaTagType, byte[]> image2DiskTags = new Dictionary<MediaTagType, byte[]>();

            foreach(MediaTagType diskTag in Enum.GetValues(typeof(MediaTagType)))
            {
                try
                {
                    byte[] tempArray = input1Format.ReadDiskTag(diskTag);
                    image1DiskTags.Add(diskTag, tempArray);
                }
                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
                {
                    // ignored
                }
                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            foreach(MediaTagType diskTag in Enum.GetValues(typeof(MediaTagType)))
            {
                try
                {
                    byte[] tempArray = input2Format.ReadDiskTag(diskTag);
                    image2DiskTags.Add(diskTag, tempArray);
                }
                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
                {
                    // ignored
                }
                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            if(verbose)
            {
                sb.AppendFormat("{0,-38}{1,-20}{2}", "Has partitions?", image1Info.HasPartitions,
                                image2Info.HasPartitions).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Has sessions?", image1Info.HasSessions, image2Info.HasSessions).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Image size", image1Info.ImageSize, image2Info.ImageSize).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Sectors", image1Info.Sectors, image2Info.Sectors).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Sector size", image1Info.SectorSize, image2Info.SectorSize).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Creation time", image1Info.CreationTime, image2Info.CreationTime).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Last modification time", image1Info.LastModificationTime,
                                image2Info.LastModificationTime).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Disk type", image1Info.MediaType, image2Info.MediaType).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Image version", image1Info.Version, image2Info.Version).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Image application", image1Info.Application,
                                image2Info.Application).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Image application version", image1Info.ApplicationVersion,
                                image2Info.ApplicationVersion).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Image creator", image1Info.Creator, image2Info.Creator).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Image name", image1Info.MediaTitle, image2Info.MediaTitle).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Image comments", image1Info.Comments, image2Info.Comments).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Image comments", image1Info.Comments, image2Info.Comments).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Disk manufacturer", image1Info.MediaManufacturer,
                                image2Info.MediaManufacturer).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Disk model", image1Info.MediaModel, image2Info.MediaModel).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Disk serial number", image1Info.MediaSerialNumber,
                                image2Info.MediaSerialNumber).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Disk barcode", image1Info.MediaBarcode, image2Info.MediaBarcode).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Disk part no.", image1Info.MediaPartNumber,
                                image2Info.MediaPartNumber).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Disk sequence", image1Info.MediaSequence,
                                image2Info.MediaSequence).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Last disk on sequence", image1Info.LastMediaSequence,
                                image2Info.LastMediaSequence).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Drive manufacturer", image1Info.DriveManufacturer,
                                image2Info.DriveManufacturer).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Drive firmware revision", image1Info.DriveFirmwareRevision,
                                image2Info.DriveFirmwareRevision).AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Drive model", image1Info.DriveModel, image2Info.DriveModel).
                   AppendLine();

                sb.AppendFormat("{0,-38}{1,-20}{2}", "Drive serial number", image1Info.DriveSerialNumber,
                                image2Info.DriveSerialNumber).AppendLine();

                foreach(MediaTagType diskTag in
                    (Enum.GetValues(typeof(MediaTagType)) as MediaTagType[]).OrderBy(e => e.ToString()))
                    sb.AppendFormat("{0,-38}{1,-20}{2}", $"Has {diskTag}?", image1DiskTags.ContainsKey(diskTag),
                                    image2DiskTags.ContainsKey(diskTag)).AppendLine();
            }

            AaruConsole.WriteLine("Comparing disk image characteristics");

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

            ulong leastSectors;

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

            AaruConsole.WriteLine("Comparing sectors...");

            for(ulong sector = 0; sector < leastSectors; sector++)
            {
                AaruConsole.Write("\rComparing sector {0} of {1}...", sector + 1, leastSectors);

                try
                {
                    byte[] image1Sector = input1Format.ReadSector(sector);
                    byte[] image2Sector = input2Format.ReadSector(sector);
                    ArrayHelpers.CompareBytes(out bool different, out bool sameSize, image1Sector, image2Sector);

                    if(different)
                    {
                        imagesDiffer = true;
                        sb.AppendFormat("Sector {0} is different", sector).AppendLine();
                    }
                    else if(!sameSize)
                    {
                        imagesDiffer = true;

                        sb.
                            AppendFormat("Sector {0} has different sizes ({1} bytes in image 1, {2} in image 2) but are otherwise identical",
                                         sector, image1Sector.LongLength, image2Sector.LongLength).AppendLine();
                    }
                }
                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
                {
                    // ignored
                }
                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            AaruConsole.WriteLine();

            sb.AppendLine(imagesDiffer ? "Images differ" : "Images do not differ");

            AaruConsole.WriteLine(sb.ToString());

            return (int)ErrorNumber.NoError;
        }
    }
}