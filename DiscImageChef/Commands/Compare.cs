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
            DicConsole.DebugWriteLine("Compare command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Compare command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Compare command", "--input1={0}", options.InputFile1);
            DicConsole.DebugWriteLine("Compare command", "--input2={0}", options.InputFile2);

            FiltersList filtersList = new FiltersList();
            Filter inputFilter1 = filtersList.GetFilter(options.InputFile1);
            filtersList = new FiltersList();
            Filter inputFilter2 = filtersList.GetFilter(options.InputFile2);

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

            ImagePlugin input1Format = ImageFormat.Detect(inputFilter1);
            ImagePlugin input2Format = ImageFormat.Detect(inputFilter2);

            if(input1Format == null)
            {
                DicConsole.ErrorWriteLine("Input file 1 format not identified, not proceeding with comparison.");
                return;
            }

            if(options.Verbose)
                DicConsole.VerboseWriteLine("Input file 1 format identified by {0} ({1}).", input1Format.Name,
                                            input1Format.PluginUuid);
            else DicConsole.WriteLine("Input file 1 format identified by {0}.", input1Format.Name);

            if(input2Format == null)
            {
                DicConsole.ErrorWriteLine("Input file 2 format not identified, not proceeding with comparison.");
                return;
            }

            if(options.Verbose)
                DicConsole.VerboseWriteLine("Input file 2 format identified by {0} ({1}).", input2Format.Name,
                                            input2Format.PluginUuid);
            else DicConsole.WriteLine("Input file 2 format identified by {0}.", input2Format.Name);

            input1Format.OpenImage(inputFilter1);
            input2Format.OpenImage(inputFilter2);

            Core.Statistics.AddMediaFormat(input1Format.GetImageFormat());
            Core.Statistics.AddMediaFormat(input2Format.GetImageFormat());
            Core.Statistics.AddMedia(input1Format.ImageInfo.MediaType, false);
            Core.Statistics.AddMedia(input2Format.ImageInfo.MediaType, false);
            Core.Statistics.AddFilter(inputFilter1.Name);
            Core.Statistics.AddFilter(inputFilter2.Name);

            StringBuilder sb = new StringBuilder();

            if(options.Verbose)
            {
                sb.AppendLine("\tDisc image 1\tDisc image 2");
                sb.AppendLine("================================");
                sb.AppendFormat("File\t{0}\t{1}", options.InputFile1, options.InputFile2).AppendLine();
                sb.AppendFormat("Disc image format\t{0}\t{1}", input1Format.Name, input2Format.Name).AppendLine();
            }
            else
            {
                sb.AppendFormat("Disc image 1: {0}", options.InputFile1).AppendLine();
                sb.AppendFormat("Disc image 2: {0}", options.InputFile2).AppendLine();
            }

            bool imagesDiffer = false;

            ImageInfo image1Info = new ImageInfo();
            ImageInfo image2Info = new ImageInfo();
            List<Session> image1Sessions = new List<Session>();
            List<Session> image2Sessions = new List<Session>();
            Dictionary<MediaTagType, byte[]> image1DiskTags = new Dictionary<MediaTagType, byte[]>();
            Dictionary<MediaTagType, byte[]> image2DiskTags = new Dictionary<MediaTagType, byte[]>();

            image1Info.ImageHasPartitions = input1Format.ImageHasPartitions();
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            try { image1Sessions = input1Format.GetSessions(); }
            catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            image1Info.ImageHasSessions |= image1Sessions.Count > 0;
            image1Info.ImageSize = input1Format.GetImageSize();
            image1Info.Sectors = input1Format.GetSectors();
            image1Info.SectorSize = input1Format.GetSectorSize();
            image1Info.ImageCreationTime = input1Format.GetImageCreationTime();
            image1Info.ImageLastModificationTime = input1Format.GetImageLastModificationTime();
            image1Info.MediaType = input1Format.GetMediaType();
            try { image1Info.ImageVersion = input1Format.GetImageVersion(); }
            catch { image1Info.ImageVersion = null; }
            try { image1Info.ImageApplication = input1Format.GetImageApplication(); }
            catch { image1Info.ImageApplication = null; }
            try { image1Info.ImageApplicationVersion = input1Format.GetImageApplicationVersion(); }
            catch { image1Info.ImageApplicationVersion = null; }
            try { image1Info.ImageCreator = input1Format.GetImageCreator(); }
            catch { image1Info.ImageCreator = null; }
            try { image1Info.ImageName = input1Format.GetImageName(); }
            catch { image1Info.ImageName = null; }
            try { image1Info.ImageComments = input1Format.GetImageComments(); }
            catch { image1Info.ImageComments = null; }
            try { image1Info.MediaManufacturer = input1Format.GetMediaManufacturer(); }
            catch { image1Info.MediaManufacturer = null; }
            try { image1Info.MediaModel = input1Format.GetMediaModel(); }
            catch { image1Info.MediaModel = null; }
            try { image1Info.MediaSerialNumber = input1Format.GetMediaSerialNumber(); }
            catch { image1Info.MediaSerialNumber = null; }
            try { image1Info.MediaBarcode = input1Format.GetMediaBarcode(); }
            catch { image1Info.MediaBarcode = null; }
            try { image1Info.MediaPartNumber = input1Format.GetMediaPartNumber(); }
            catch { image1Info.MediaPartNumber = null; }
            try { image1Info.MediaSequence = input1Format.GetMediaSequence(); }
            catch { image1Info.MediaSequence = 0; }
            try { image1Info.LastMediaSequence = input1Format.GetLastDiskSequence(); }
            catch { image1Info.LastMediaSequence = 0; }
            try { image1Info.DriveManufacturer = input1Format.GetDriveManufacturer(); }
            catch { image1Info.DriveManufacturer = null; }
            try { image1Info.DriveModel = input1Format.GetDriveModel(); }
            catch { image1Info.DriveModel = null; }
            try { image1Info.DriveSerialNumber = input1Format.GetDriveSerialNumber(); }
            catch { image1Info.DriveSerialNumber = null; }
            try { image1Info.DriveFirmwareRevision = input1Format.ImageInfo.DriveFirmwareRevision; }
            catch { image1Info.DriveFirmwareRevision = null; }
            foreach(MediaTagType disktag in Enum.GetValues(typeof(MediaTagType)))
            {
                try
                {
                    byte[] temparray = input1Format.ReadDiskTag(disktag);
                    image1DiskTags.Add(disktag, temparray);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            image2Info.ImageHasPartitions = input2Format.ImageHasPartitions();
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            try { image2Sessions = input2Format.GetSessions(); }
            catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            image2Info.ImageHasSessions |= image2Sessions.Count > 0;
            image2Info.ImageSize = input2Format.GetImageSize();
            image2Info.Sectors = input2Format.GetSectors();
            image2Info.SectorSize = input2Format.GetSectorSize();
            image2Info.ImageCreationTime = input2Format.GetImageCreationTime();
            image2Info.ImageLastModificationTime = input2Format.GetImageLastModificationTime();
            image2Info.MediaType = input2Format.GetMediaType();
            try { image2Info.ImageVersion = input2Format.GetImageVersion(); }
            catch { image2Info.ImageVersion = null; }
            try { image2Info.ImageApplication = input2Format.GetImageApplication(); }
            catch { image2Info.ImageApplication = null; }
            try { image2Info.ImageApplicationVersion = input2Format.GetImageApplicationVersion(); }
            catch { image2Info.ImageApplicationVersion = null; }
            try { image2Info.ImageCreator = input2Format.GetImageCreator(); }
            catch { image2Info.ImageCreator = null; }
            try { image2Info.ImageName = input2Format.GetImageName(); }
            catch { image2Info.ImageName = null; }
            try { image2Info.ImageComments = input2Format.GetImageComments(); }
            catch { image2Info.ImageComments = null; }
            try { image2Info.MediaManufacturer = input2Format.GetMediaManufacturer(); }
            catch { image2Info.MediaManufacturer = null; }
            try { image2Info.MediaModel = input2Format.GetMediaModel(); }
            catch { image2Info.MediaModel = null; }
            try { image2Info.MediaSerialNumber = input2Format.GetMediaSerialNumber(); }
            catch { image2Info.MediaSerialNumber = null; }
            try { image2Info.MediaBarcode = input2Format.GetMediaBarcode(); }
            catch { image2Info.MediaBarcode = null; }
            try { image2Info.MediaPartNumber = input2Format.GetMediaPartNumber(); }
            catch { image2Info.MediaPartNumber = null; }
            try { image2Info.MediaSequence = input2Format.GetMediaSequence(); }
            catch { image2Info.MediaSequence = 0; }
            try { image2Info.LastMediaSequence = input2Format.GetLastDiskSequence(); }
            catch { image2Info.LastMediaSequence = 0; }
            try { image2Info.DriveManufacturer = input2Format.GetDriveManufacturer(); }
            catch { image2Info.DriveManufacturer = null; }
            try { image2Info.DriveModel = input2Format.GetDriveModel(); }
            catch { image2Info.DriveModel = null; }
            try { image2Info.DriveSerialNumber = input2Format.GetDriveSerialNumber(); }
            catch { image2Info.DriveSerialNumber = null; }
            try { image2Info.DriveFirmwareRevision = input2Format.ImageInfo.DriveFirmwareRevision; }
            catch { image2Info.DriveFirmwareRevision = null; }
            foreach(MediaTagType disktag in Enum.GetValues(typeof(MediaTagType)))
            {
                try
                {
                    byte[] temparray = input2Format.ReadDiskTag(disktag);
                    image2DiskTags.Add(disktag, temparray);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            if(options.Verbose)
            {
                sb.AppendFormat("Has partitions?\t{0}\t{1}", image1Info.ImageHasPartitions,
                                image2Info.ImageHasPartitions).AppendLine();
                sb.AppendFormat("Has sessions?\t{0}\t{1}", image1Info.ImageHasSessions, image2Info.ImageHasSessions)
                  .AppendLine();
                sb.AppendFormat("Image size\t{0}\t{1}", image1Info.ImageSize, image2Info.ImageSize).AppendLine();
                sb.AppendFormat("Sectors\t{0}\t{1}", image1Info.Sectors, image2Info.Sectors).AppendLine();
                sb.AppendFormat("Sector size\t{0}\t{1}", image1Info.SectorSize, image2Info.SectorSize).AppendLine();
                sb.AppendFormat("Creation time\t{0}\t{1}", image1Info.ImageCreationTime, image2Info.ImageCreationTime)
                  .AppendLine();
                sb.AppendFormat("Last modification time\t{0}\t{1}", image1Info.ImageLastModificationTime,
                                image2Info.ImageLastModificationTime).AppendLine();
                sb.AppendFormat("Disk type\t{0}\t{1}", image1Info.MediaType, image2Info.MediaType).AppendLine();
                sb.AppendFormat("Image version\t{0}\t{1}", image1Info.ImageVersion, image2Info.ImageVersion)
                  .AppendLine();
                sb.AppendFormat("Image application\t{0}\t{1}", image1Info.ImageApplication, image2Info.ImageApplication)
                  .AppendLine();
                sb.AppendFormat("Image application version\t{0}\t{1}", image1Info.ImageApplicationVersion,
                                image2Info.ImageApplicationVersion).AppendLine();
                sb.AppendFormat("Image creator\t{0}\t{1}", image1Info.ImageCreator, image2Info.ImageCreator)
                  .AppendLine();
                sb.AppendFormat("Image name\t{0}\t{1}", image1Info.ImageName, image2Info.ImageName).AppendLine();
                sb.AppendFormat("Image comments\t{0}\t{1}", image1Info.ImageComments, image2Info.ImageComments)
                  .AppendLine();
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

            if(image1Info.ImageHasPartitions != image2Info.ImageHasPartitions)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image partitioned status differ");
            }
            if(image1Info.ImageHasSessions != image2Info.ImageHasSessions)
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
            if(image1Info.ImageCreationTime != image2Info.ImageCreationTime)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image creation time differ");
            }
            if(image1Info.ImageLastModificationTime != image2Info.ImageLastModificationTime)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image last modification time differ");
            }
            if(image1Info.MediaType != image2Info.MediaType)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Disk type differ");
            }
            if(image1Info.ImageVersion != image2Info.ImageVersion)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image version differ");
            }
            if(image1Info.ImageApplication != image2Info.ImageApplication)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image application differ");
            }
            if(image1Info.ImageApplicationVersion != image2Info.ImageApplicationVersion)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image application version differ");
            }
            if(image1Info.ImageCreator != image2Info.ImageCreator)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image creator differ");
            }
            if(image1Info.ImageName != image2Info.ImageName)
            {
                imagesDiffer = true;
                if(!options.Verbose) sb.AppendLine("Image name differ");
            }
            if(image1Info.ImageComments != image2Info.ImageComments)
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
            else leastSectors = image1Info.Sectors;

            DicConsole.WriteLine("Comparing sectors...");

            for(ulong sector = 0; sector < leastSectors; sector++)
            {
                DicConsole.Write("\rComparing sector {0} of {1}...", sector + 1, leastSectors);
                try
                {
                    byte[] image1Sector = input1Format.ReadSector(sector);
                    byte[] image2Sector = input2Format.ReadSector(sector);
                    bool different, sameSize;
                    ArrayHelpers.CompareBytes(out different, out sameSize, image1Sector, image2Sector);
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
                catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            DicConsole.WriteLine();

            if(imagesDiffer) sb.AppendLine("Images differ");
            else sb.AppendLine("Images do not differ");

            DicConsole.WriteLine(sb.ToString());

            Core.Statistics.AddCommand("compare");
        }
    }
}