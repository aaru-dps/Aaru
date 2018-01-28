// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Compare.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'compare' verb.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;

namespace DiscImageChef.Commands
{
    static class Compare
    {
        internal static void DoCompare(CompareOptions options)
        {
            DicConsole.DebugWriteLine("Compare command", "--debug={0}",   options.Debug);
            DicConsole.DebugWriteLine("Compare command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Compare command", "--input1={0}",  options.InputFile1);
            DicConsole.DebugWriteLine("Compare command", "--input2={0}",  options.InputFile2);

            FiltersList filtersList  = new FiltersList();
            IFilter     inputFilter1 = filtersList.GetFilter(options.InputFile1);
            filtersList              = new FiltersList();
            IFilter inputFilter2     = filtersList.GetFilter(options.InputFile2);

            if(inputFilter1 == null)
            {
                DicConsole.ErrorWriteLine("Cannot open input file 1");
                return;
            }

            if(inputFilter2 == null)
            {
                DicConsole.ErrorWriteLine("Cannot open input file 2");
                return;
            }

            IMediaImage input1Format = ImageFormat.Detect(inputFilter1);
            IMediaImage input2Format = ImageFormat.Detect(inputFilter2);

            if(input1Format == null)
            {
                DicConsole.ErrorWriteLine("Input file 1 format not identified, not proceeding with comparison.");
                return;
            }

            if(options.Verbose)
                DicConsole.VerboseWriteLine("Input file 1 format identified by {0} ({1}).", input1Format.Name,
                                            input1Format.Id);
            else DicConsole.WriteLine("Input file 1 format identified by {0}.", input1Format.Name);

            if(input2Format == null)
            {
                DicConsole.ErrorWriteLine("Input file 2 format not identified, not proceeding with comparison.");
                return;
            }

            if(options.Verbose)
                DicConsole.VerboseWriteLine("Input file 2 format identified by {0} ({1}).", input2Format.Name,
                                            input2Format.Id);
            else DicConsole.WriteLine("Input file 2 format identified by {0}.", input2Format.Name);

            input1Format.Open(inputFilter1);
            input2Format.Open(inputFilter2);

            Core.Statistics.AddMediaFormat(input1Format.Format);
            Core.Statistics.AddMediaFormat(input2Format.Format);
            Core.Statistics.AddMedia(input1Format.Info.MediaType, false);
            Core.Statistics.AddMedia(input2Format.Info.MediaType, false);
            Core.Statistics.AddFilter(inputFilter1.Name);
            Core.Statistics.AddFilter(inputFilter2.Name);

            StringBuilder sb = new StringBuilder();

            if(options.Verbose)
            {
                sb.AppendLine("\tDisc image 1\tDisc image 2");
                sb.AppendLine("================================");
                sb.AppendFormat("File\t{0}\t{1}",              options.InputFile1, options.InputFile2).AppendLine();
                sb.AppendFormat("Disc image format\t{0}\t{1}", input1Format.Name,  input2Format.Name).AppendLine();
            }
            else
            {
                sb.AppendFormat("Disc image 1: {0}", options.InputFile1).AppendLine();
                sb.AppendFormat("Disc image 2: {0}", options.InputFile2).AppendLine();
            }

            bool imagesDiffer = false;

            DiscImages.ImageInfo             image1Info     = new DiscImages.ImageInfo();
            DiscImages.ImageInfo             image2Info     = new DiscImages.ImageInfo();
            List<Session>                    image1Sessions = new List<Session>();
            List<Session>                    image2Sessions = new List<Session>();
            Dictionary<MediaTagType, byte[]> image1DiskTags = new Dictionary<MediaTagType, byte[]>();
            Dictionary<MediaTagType, byte[]> image2DiskTags = new Dictionary<MediaTagType, byte[]>();

            image1Info.HasPartitions = input1Format.Info.HasPartitions;
            #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            try { image1Sessions = input1Format.Sessions; }
            catch
            {
                // ignored
            }
            #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            image1Info.HasSessions           |= image1Sessions.Count > 0;
            image1Info.ImageSize             =  input1Format.Info.ImageSize;
            image1Info.Sectors               =  input1Format.Info.Sectors;
            image1Info.SectorSize            =  input1Format.Info.SectorSize;
            image1Info.CreationTime          =  input1Format.Info.CreationTime;
            image1Info.LastModificationTime  =  input1Format.Info.LastModificationTime;
            image1Info.MediaType             =  input1Format.Info.MediaType;
            image1Info.Version               =  input1Format.Info.Version;
            image1Info.Application           =  input1Format.Info.Application;
            image1Info.ApplicationVersion    =  input1Format.Info.ApplicationVersion;
            image1Info.Creator               =  input1Format.Info.Creator;
            image1Info.MediaTitle            =  input1Format.Info.MediaTitle;
            image1Info.Comments              =  input1Format.Info.Comments;
            image1Info.MediaManufacturer     =  input1Format.Info.MediaManufacturer;
            image1Info.MediaModel            =  input1Format.Info.MediaModel;
            image1Info.MediaSerialNumber     =  input1Format.Info.MediaSerialNumber;
            image1Info.MediaBarcode          =  input1Format.Info.MediaBarcode;
            image1Info.MediaPartNumber       =  input1Format.Info.MediaPartNumber;
            image1Info.MediaSequence         =  input1Format.Info.MediaSequence;
            image1Info.LastMediaSequence     =  input1Format.Info.LastMediaSequence;
            image1Info.DriveManufacturer     =  input1Format.Info.DriveManufacturer;
            image1Info.DriveModel            =  input1Format.Info.DriveModel;
            image1Info.DriveSerialNumber     =  input1Format.Info.DriveSerialNumber;
            image1Info.DriveFirmwareRevision =  input1Format.Info.DriveFirmwareRevision;
            foreach(MediaTagType disktag in Enum.GetValues(typeof(MediaTagType)))
            {
                try
                {
                    byte[] temparray = input1Format.ReadDiskTag(disktag);
                    image1DiskTags.Add(disktag, temparray);
                }
                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
                {
                    // ignored
                }
                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            image2Info.HasPartitions = input2Format.Info.HasPartitions;
            #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            try { image2Sessions = input2Format.Sessions; }
            catch
            {
                // ignored
            }
            #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            image2Info.HasSessions           |= image2Sessions.Count > 0;
            image2Info.ImageSize             =  input2Format.Info.ImageSize;
            image2Info.Sectors               =  input2Format.Info.Sectors;
            image2Info.SectorSize            =  input2Format.Info.SectorSize;
            image2Info.CreationTime          =  input2Format.Info.CreationTime;
            image2Info.LastModificationTime  =  input2Format.Info.LastModificationTime;
            image2Info.MediaType             =  input2Format.Info.MediaType;
            image2Info.Version               =  input2Format.Info.Version;
            image2Info.Application           =  input2Format.Info.Application;
            image2Info.ApplicationVersion    =  input2Format.Info.ApplicationVersion;
            image2Info.Creator               =  input2Format.Info.Creator;
            image2Info.MediaTitle            =  input2Format.Info.MediaTitle;
            image2Info.Comments              =  input2Format.Info.Comments;
            image2Info.MediaManufacturer     =  input2Format.Info.MediaManufacturer;
            image2Info.MediaModel            =  input2Format.Info.MediaModel;
            image2Info.MediaSerialNumber     =  input2Format.Info.MediaSerialNumber;
            image2Info.MediaBarcode          =  input2Format.Info.MediaBarcode;
            image2Info.MediaPartNumber       =  input2Format.Info.MediaPartNumber;
            image2Info.MediaSequence         =  input2Format.Info.MediaSequence;
            image2Info.LastMediaSequence     =  input2Format.Info.LastMediaSequence;
            image2Info.DriveManufacturer     =  input2Format.Info.DriveManufacturer;
            image2Info.DriveModel            =  input2Format.Info.DriveModel;
            image2Info.DriveSerialNumber     =  input2Format.Info.DriveSerialNumber;
            image2Info.DriveFirmwareRevision =  input2Format.Info.DriveFirmwareRevision;
            foreach(MediaTagType disktag in Enum.GetValues(typeof(MediaTagType)))
            {
                try
                {
                    byte[] temparray = input2Format.ReadDiskTag(disktag);
                    image2DiskTags.Add(disktag, temparray);
                }
                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
                {
                    // ignored
                }
                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            if(options.Verbose)
            {
                sb.AppendFormat("Has partitions?\t{0}\t{1}", image1Info.HasPartitions, image2Info.HasPartitions)
                  .AppendLine();
                sb.AppendFormat("Has sessions?\t{0}\t{1}", image1Info.HasSessions, image2Info.HasSessions)
                  .AppendLine();
                sb.AppendFormat("Image size\t{0}\t{1}",    image1Info.ImageSize,    image2Info.ImageSize).AppendLine();
                sb.AppendFormat("Sectors\t{0}\t{1}",       image1Info.Sectors,      image2Info.Sectors).AppendLine();
                sb.AppendFormat("Sector size\t{0}\t{1}",   image1Info.SectorSize,   image2Info.SectorSize).AppendLine();
                sb.AppendFormat("Creation time\t{0}\t{1}", image1Info.CreationTime, image2Info.CreationTime)
                  .AppendLine();
                sb.AppendFormat("Last modification time\t{0}\t{1}", image1Info.LastModificationTime,
                                image2Info.LastModificationTime).AppendLine();
                sb.AppendFormat("Disk type\t{0}\t{1}", image1Info.MediaType, image2Info.MediaType)
                  .AppendLine();
                sb.AppendFormat("Image version\t{0}\t{1}",     image1Info.Version,     image2Info.Version).AppendLine();
                sb.AppendFormat("Image application\t{0}\t{1}", image1Info.Application, image2Info.Application)
                  .AppendLine();
                sb.AppendFormat("Image application version\t{0}\t{1}", image1Info.ApplicationVersion,
                                image2Info.ApplicationVersion).AppendLine();
                sb.AppendFormat("Image creator\t{0}\t{1}",  image1Info.Creator,    image2Info.Creator).AppendLine();
                sb.AppendFormat("Image name\t{0}\t{1}",     image1Info.MediaTitle, image2Info.MediaTitle).AppendLine();
                sb.AppendFormat("Image comments\t{0}\t{1}", image1Info.Comments,   image2Info.Comments).AppendLine();
                sb.AppendFormat("Disk manufacturer\t{0}\t{1}", image1Info.MediaManufacturer,
                                image2Info.MediaManufacturer).AppendLine();
                sb.AppendFormat("Disk model\t{0}\t{1}", image1Info.MediaModel, image2Info.MediaModel).AppendLine();
                sb.AppendFormat("Disk serial number\t{0}\t{1}", image1Info.MediaSerialNumber,
                                image2Info.MediaSerialNumber).AppendLine();
                sb.AppendFormat("Disk barcode\t{0}\t{1}", image1Info.MediaBarcode, image2Info.MediaBarcode)
                  .AppendLine();
                sb.AppendFormat("Disk part no.\t{0}\t{1}", image1Info.MediaPartNumber, image2Info.MediaPartNumber)
                  .AppendLine();
                sb.AppendFormat("Disk sequence\t{0}\t{1}", image1Info.MediaSequence, image2Info.MediaSequence)
                  .AppendLine();
                sb.AppendFormat("Last disk on sequence\t{0}\t{1}", image1Info.LastMediaSequence,
                                image2Info.LastMediaSequence).AppendLine();
                sb.AppendFormat("Drive manufacturer\t{0}\t{1}", image1Info.DriveManufacturer,
                                image2Info.DriveManufacturer).AppendLine();
                sb.AppendFormat("Drive firmware revision\t{0}\t{1}", image1Info.DriveFirmwareRevision,
                                image2Info.DriveFirmwareRevision).AppendLine();
                sb.AppendFormat("Drive model\t{0}\t{1}", image1Info.DriveModel, image2Info.DriveModel).AppendLine();
                sb.AppendFormat("Drive serial number\t{0}\t{1}", image1Info.DriveSerialNumber,
                                image2Info.DriveSerialNumber).AppendLine();
                foreach(MediaTagType disktag in Enum.GetValues(typeof(MediaTagType)))
                    sb.AppendFormat("Has {0}?\t{1}\t{2}", disktag, image1DiskTags.ContainsKey(disktag),
                                    image2DiskTags.ContainsKey(disktag)).AppendLine();
            }

            DicConsole.WriteLine("Comparing disk image characteristics");

            if(image1Info.HasPartitions != image2Info.HasPartitions)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image partitioned status differ");
            }

            if(image1Info.HasSessions != image2Info.HasSessions)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image session status differ");
            }

            if(image1Info.ImageSize != image2Info.ImageSize)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image size differ");
            }

            if(image1Info.Sectors != image2Info.Sectors)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image sectors differ");
            }

            if(image1Info.SectorSize != image2Info.SectorSize)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image sector size differ");
            }

            if(image1Info.CreationTime != image2Info.CreationTime)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image creation time differ");
            }

            if(image1Info.LastModificationTime != image2Info.LastModificationTime)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image last modification time differ");
            }

            if(image1Info.MediaType != image2Info.MediaType)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Disk type differ");
            }

            if(image1Info.Version != image2Info.Version)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image version differ");
            }

            if(image1Info.Application != image2Info.Application)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image application differ");
            }

            if(image1Info.ApplicationVersion != image2Info.ApplicationVersion)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image application version differ");
            }

            if(image1Info.Creator != image2Info.Creator)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image creator differ");
            }

            if(image1Info.MediaTitle != image2Info.MediaTitle)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image name differ");
            }

            if(image1Info.Comments != image2Info.Comments)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image comments differ");
            }

            if(image1Info.MediaManufacturer != image2Info.MediaManufacturer)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Disk manufacturer differ");
            }

            if(image1Info.MediaModel != image2Info.MediaModel)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Disk model differ");
            }

            if(image1Info.MediaSerialNumber != image2Info.MediaSerialNumber)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Disk serial number differ");
            }

            if(image1Info.MediaBarcode != image2Info.MediaBarcode)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Disk barcode differ");
            }

            if(image1Info.MediaPartNumber != image2Info.MediaPartNumber)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Disk part number differ");
            }

            if(image1Info.MediaSequence != image2Info.MediaSequence)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Disk sequence differ");
            }

            if(image1Info.LastMediaSequence != image2Info.LastMediaSequence)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Last disk in sequence differ");
            }

            if(image1Info.DriveManufacturer != image2Info.DriveManufacturer)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Drive manufacturer differ");
            }

            if(image1Info.DriveModel != image2Info.DriveModel)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Drive model differ");
            }

            if(image1Info.DriveSerialNumber != image2Info.DriveSerialNumber)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Drive serial number differ");
            }

            if(image1Info.DriveFirmwareRevision != image2Info.DriveFirmwareRevision)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Drive firmware revision differ");
            }

            ulong leastSectors;
            if(image1Info.Sectors < image2Info.Sectors)
            {
                imagesDiffer = true;
                leastSectors = image1Info.Sectors;
                if(!options.Verbose) sb.AppendLine("Image 2 has more sectors");
            }
            else if(image1Info.Sectors > image2Info.Sectors)
            {
                imagesDiffer = true;
                leastSectors = image2Info.Sectors;
                if(!options.Verbose) sb.AppendLine("Image 1 has more sectors");
            }
            else
                leastSectors = image1Info.Sectors;

            DicConsole.WriteLine("Comparing sectors...");

            for(ulong sector = 0; sector < leastSectors; sector++)
            {
                DicConsole.Write("\rComparing sector {0} of {1}...", sector + 1, leastSectors);
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
                        sb
                           .AppendFormat("Sector {0} has different sizes ({1} bytes in image 1, {2} in image 2) but are otherwise identical",
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

            DicConsole.WriteLine();

            sb.AppendLine(imagesDiffer ? "Images differ" : "Images do not differ");

            DicConsole.WriteLine(sb.ToString());

            Core.Statistics.AddCommand("compare");
        }
    }
}