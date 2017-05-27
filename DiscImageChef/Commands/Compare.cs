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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.ImagePlugins;
using System.Text;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    public static class Compare
    {
        public static void doCompare(CompareOptions options)
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
            else
            {
                if(options.Verbose)
                    DicConsole.VerboseWriteLine("Input file 1 format identified by {0} ({1}).", input1Format.Name, input1Format.PluginUUID);
                else
                    DicConsole.WriteLine("Input file 1 format identified by {0}.", input1Format.Name);
            }

            if(input2Format == null)
            {
                DicConsole.ErrorWriteLine("Input file 2 format not identified, not proceeding with comparison.");
                return;
            }
            else
            {
                if(options.Verbose)
                    DicConsole.VerboseWriteLine("Input file 2 format identified by {0} ({1}).", input2Format.Name, input2Format.PluginUUID);
                else
                    DicConsole.WriteLine("Input file 2 format identified by {0}.", input2Format.Name);
            }

            input1Format.OpenImage(inputFilter1);
            input2Format.OpenImage(inputFilter2);

            Core.Statistics.AddMediaFormat(input1Format.GetImageFormat());
            Core.Statistics.AddMediaFormat(input2Format.GetImageFormat());
            Core.Statistics.AddMedia(input1Format.ImageInfo.mediaType, false);
            Core.Statistics.AddMedia(input2Format.ImageInfo.mediaType, false);
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

            image1Info.imageHasPartitions = input1Format.ImageHasPartitions();
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            try { image1Sessions = input1Format.GetSessions(); } catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            image1Info.imageHasSessions |= image1Sessions.Count > 0;
            image1Info.imageSize = input1Format.GetImageSize();
            image1Info.sectors = input1Format.GetSectors();
            image1Info.sectorSize = input1Format.GetSectorSize();
            image1Info.imageCreationTime = input1Format.GetImageCreationTime();
            image1Info.imageLastModificationTime = input1Format.GetImageLastModificationTime();
            image1Info.mediaType = input1Format.GetMediaType();
            try { image1Info.imageVersion = input1Format.GetImageVersion(); } catch { image1Info.imageVersion = null; }
            try { image1Info.imageApplication = input1Format.GetImageApplication(); } catch { image1Info.imageApplication = null; }
            try { image1Info.imageApplicationVersion = input1Format.GetImageApplicationVersion(); } catch { image1Info.imageApplicationVersion = null; }
            try { image1Info.imageCreator = input1Format.GetImageCreator(); } catch { image1Info.imageCreator = null; }
            try { image1Info.imageName = input1Format.GetImageName(); } catch { image1Info.imageName = null; }
            try { image1Info.imageComments = input1Format.GetImageComments(); } catch { image1Info.imageComments = null; }
            try { image1Info.mediaManufacturer = input1Format.GetMediaManufacturer(); } catch { image1Info.mediaManufacturer = null; }
            try { image1Info.mediaModel = input1Format.GetMediaModel(); } catch { image1Info.mediaModel = null; }
            try { image1Info.mediaSerialNumber = input1Format.GetMediaSerialNumber(); } catch { image1Info.mediaSerialNumber = null; }
            try { image1Info.mediaBarcode = input1Format.GetMediaBarcode(); } catch { image1Info.mediaBarcode = null; }
            try { image1Info.mediaPartNumber = input1Format.GetMediaPartNumber(); } catch { image1Info.mediaPartNumber = null; }
            try { image1Info.mediaSequence = input1Format.GetMediaSequence(); } catch { image1Info.mediaSequence = 0; }
            try { image1Info.lastMediaSequence = input1Format.GetLastDiskSequence(); } catch { image1Info.lastMediaSequence = 0; }
            try { image1Info.driveManufacturer = input1Format.GetDriveManufacturer(); } catch { image1Info.driveManufacturer = null; }
            try { image1Info.driveModel = input1Format.GetDriveModel(); } catch { image1Info.driveModel = null; }
            try { image1Info.driveSerialNumber = input1Format.GetDriveSerialNumber(); } catch { image1Info.driveSerialNumber = null; }
            try { image1Info.driveFirmwareRevision = input1Format.ImageInfo.driveFirmwareRevision; } catch { image1Info.driveFirmwareRevision = null; }
            foreach(MediaTagType disktag in Enum.GetValues(typeof(MediaTagType)))
            {
                try
                {
                    byte[] temparray = input1Format.ReadDiskTag(disktag);
                    image1DiskTags.Add(disktag, temparray);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                }
            }

            image2Info.imageHasPartitions = input2Format.ImageHasPartitions();
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            try { image2Sessions = input2Format.GetSessions(); } catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            image2Info.imageHasSessions |= image2Sessions.Count > 0;
            image2Info.imageSize = input2Format.GetImageSize();
            image2Info.sectors = input2Format.GetSectors();
            image2Info.sectorSize = input2Format.GetSectorSize();
            image2Info.imageCreationTime = input2Format.GetImageCreationTime();
            image2Info.imageLastModificationTime = input2Format.GetImageLastModificationTime();
            image2Info.mediaType = input2Format.GetMediaType();
            try { image2Info.imageVersion = input2Format.GetImageVersion(); } catch { image2Info.imageVersion = null; }
            try { image2Info.imageApplication = input2Format.GetImageApplication(); } catch { image2Info.imageApplication = null; }
            try { image2Info.imageApplicationVersion = input2Format.GetImageApplicationVersion(); } catch { image2Info.imageApplicationVersion = null; }
            try { image2Info.imageCreator = input2Format.GetImageCreator(); } catch { image2Info.imageCreator = null; }
            try { image2Info.imageName = input2Format.GetImageName(); } catch { image2Info.imageName = null; }
            try { image2Info.imageComments = input2Format.GetImageComments(); } catch { image2Info.imageComments = null; }
            try { image2Info.mediaManufacturer = input2Format.GetMediaManufacturer(); } catch { image2Info.mediaManufacturer = null; }
            try { image2Info.mediaModel = input2Format.GetMediaModel(); } catch { image2Info.mediaModel = null; }
            try { image2Info.mediaSerialNumber = input2Format.GetMediaSerialNumber(); } catch { image2Info.mediaSerialNumber = null; }
            try { image2Info.mediaBarcode = input2Format.GetMediaBarcode(); } catch { image2Info.mediaBarcode = null; }
            try { image2Info.mediaPartNumber = input2Format.GetMediaPartNumber(); } catch { image2Info.mediaPartNumber = null; }
            try { image2Info.mediaSequence = input2Format.GetMediaSequence(); } catch { image2Info.mediaSequence = 0; }
            try { image2Info.lastMediaSequence = input2Format.GetLastDiskSequence(); } catch { image2Info.lastMediaSequence = 0; }
            try { image2Info.driveManufacturer = input2Format.GetDriveManufacturer(); } catch { image2Info.driveManufacturer = null; }
            try { image2Info.driveModel = input2Format.GetDriveModel(); } catch { image2Info.driveModel = null; }
            try { image2Info.driveSerialNumber = input2Format.GetDriveSerialNumber(); } catch { image2Info.driveSerialNumber = null; }
            try { image2Info.driveFirmwareRevision = input2Format.ImageInfo.driveFirmwareRevision; } catch { image2Info.driveFirmwareRevision = null; }
            foreach(MediaTagType disktag in Enum.GetValues(typeof(MediaTagType)))
            {
                try
                {
                    byte[] temparray = input2Format.ReadDiskTag(disktag);
                    image2DiskTags.Add(disktag, temparray);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                }
            }

            if(options.Verbose)
            {
                sb.AppendFormat("Has partitions?\t{0}\t{1}", image1Info.imageHasPartitions, image2Info.imageHasPartitions).AppendLine();
                sb.AppendFormat("Has sessions?\t{0}\t{1}", image1Info.imageHasSessions, image2Info.imageHasSessions).AppendLine();
                sb.AppendFormat("Image size\t{0}\t{1}", image1Info.imageSize, image2Info.imageSize).AppendLine();
                sb.AppendFormat("Sectors\t{0}\t{1}", image1Info.sectors, image2Info.sectors).AppendLine();
                sb.AppendFormat("Sector size\t{0}\t{1}", image1Info.sectorSize, image2Info.sectorSize).AppendLine();
                sb.AppendFormat("Creation time\t{0}\t{1}", image1Info.imageCreationTime, image2Info.imageCreationTime).AppendLine();
                sb.AppendFormat("Last modification time\t{0}\t{1}", image1Info.imageLastModificationTime, image2Info.imageLastModificationTime).AppendLine();
                sb.AppendFormat("Disk type\t{0}\t{1}", image1Info.mediaType, image2Info.mediaType).AppendLine();
                sb.AppendFormat("Image version\t{0}\t{1}", image1Info.imageVersion, image2Info.imageVersion).AppendLine();
                sb.AppendFormat("Image application\t{0}\t{1}", image1Info.imageApplication, image2Info.imageApplication).AppendLine();
                sb.AppendFormat("Image application version\t{0}\t{1}", image1Info.imageApplicationVersion, image2Info.imageApplicationVersion).AppendLine();
                sb.AppendFormat("Image creator\t{0}\t{1}", image1Info.imageCreator, image2Info.imageCreator).AppendLine();
                sb.AppendFormat("Image name\t{0}\t{1}", image1Info.imageName, image2Info.imageName).AppendLine();
                sb.AppendFormat("Image comments\t{0}\t{1}", image1Info.imageComments, image2Info.imageComments).AppendLine();
                sb.AppendFormat("Disk manufacturer\t{0}\t{1}", image1Info.mediaManufacturer, image2Info.mediaManufacturer).AppendLine();
                sb.AppendFormat("Disk model\t{0}\t{1}", image1Info.mediaModel, image2Info.mediaModel).AppendLine();
                sb.AppendFormat("Disk serial number\t{0}\t{1}", image1Info.mediaSerialNumber, image2Info.mediaSerialNumber).AppendLine();
                sb.AppendFormat("Disk barcode\t{0}\t{1}", image1Info.mediaBarcode, image2Info.mediaBarcode).AppendLine();
                sb.AppendFormat("Disk part no.\t{0}\t{1}", image1Info.mediaPartNumber, image2Info.mediaPartNumber).AppendLine();
                sb.AppendFormat("Disk sequence\t{0}\t{1}", image1Info.mediaSequence, image2Info.mediaSequence).AppendLine();
                sb.AppendFormat("Last disk on sequence\t{0}\t{1}", image1Info.lastMediaSequence, image2Info.lastMediaSequence).AppendLine();
                sb.AppendFormat("Drive manufacturer\t{0}\t{1}", image1Info.driveManufacturer, image2Info.driveManufacturer).AppendLine();
                sb.AppendFormat("Drive firmware revision\t{0}\t{1}", image1Info.driveFirmwareRevision, image2Info.driveFirmwareRevision).AppendLine();
                sb.AppendFormat("Drive model\t{0}\t{1}", image1Info.driveModel, image2Info.driveModel).AppendLine();
                sb.AppendFormat("Drive serial number\t{0}\t{1}", image1Info.driveSerialNumber, image2Info.driveSerialNumber).AppendLine();
                foreach(MediaTagType disktag in Enum.GetValues(typeof(MediaTagType)))
                {
                    sb.AppendFormat("Has {0}?\t{1}\t{2}", disktag, image1DiskTags.ContainsKey(disktag), image2DiskTags.ContainsKey(disktag)).AppendLine();
                }
            }

            DicConsole.WriteLine("Comparing disk image characteristics");

            if(image1Info.imageHasPartitions != image2Info.imageHasPartitions)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image partitioned status differ");
            }
            if(image1Info.imageHasSessions != image2Info.imageHasSessions)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image session status differ");
            }
            if(image1Info.imageSize != image2Info.imageSize)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image size differ");
            }
            if(image1Info.sectors != image2Info.sectors)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image sectors differ");
            }
            if(image1Info.sectorSize != image2Info.sectorSize)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image sector size differ");
            }
            if(image1Info.imageCreationTime != image2Info.imageCreationTime)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image creation time differ");
            }
            if(image1Info.imageLastModificationTime != image2Info.imageLastModificationTime)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image last modification time differ");
            }
            if(image1Info.mediaType != image2Info.mediaType)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Disk type differ");
            }
            if(image1Info.imageVersion != image2Info.imageVersion)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image version differ");
            }
            if(image1Info.imageApplication != image2Info.imageApplication)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image application differ");
            }
            if(image1Info.imageApplicationVersion != image2Info.imageApplicationVersion)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image application version differ");
            }
            if(image1Info.imageCreator != image2Info.imageCreator)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image creator differ");
            }
            if(image1Info.imageName != image2Info.imageName)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image name differ");
            }
            if(image1Info.imageComments != image2Info.imageComments)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Image comments differ");
            }
            if(image1Info.mediaManufacturer != image2Info.mediaManufacturer)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Disk manufacturer differ");
            }
            if(image1Info.mediaModel != image2Info.mediaModel)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Disk model differ");
            }
            if(image1Info.mediaSerialNumber != image2Info.mediaSerialNumber)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Disk serial number differ");
            }
            if(image1Info.mediaBarcode != image2Info.mediaBarcode)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Disk barcode differ");
            }
            if(image1Info.mediaPartNumber != image2Info.mediaPartNumber)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Disk part number differ");
            }
            if(image1Info.mediaSequence != image2Info.mediaSequence)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Disk sequence differ");
            }
            if(image1Info.lastMediaSequence != image2Info.lastMediaSequence)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Last disk in sequence differ");
            }
            if(image1Info.driveManufacturer != image2Info.driveManufacturer)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Drive manufacturer differ");
            }
            if(image1Info.driveModel != image2Info.driveModel)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Drive model differ");
            }
            if(image1Info.driveSerialNumber != image2Info.driveSerialNumber)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Drive serial number differ");
            }
            if(image1Info.driveFirmwareRevision != image2Info.driveFirmwareRevision)
            {
                imagesDiffer = true;
                if(!options.Verbose)
                    sb.AppendLine("Drive firmware revision differ");
            }

            ulong leastSectors;
            if(image1Info.sectors < image2Info.sectors)
            {
                imagesDiffer = true;
                leastSectors = image1Info.sectors;
                if(!options.Verbose)
                    sb.AppendLine("Image 2 has more sectors");
            }
            else if(image1Info.sectors > image2Info.sectors)
            {
                imagesDiffer = true;
                leastSectors = image2Info.sectors;
                if(!options.Verbose)
                    sb.AppendLine("Image 1 has more sectors");
            }
            else
                leastSectors = image1Info.sectors;

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
                        sb.AppendFormat("Sector {0} has different sizes ({1} bytes in image 1, {2} in image 2) but are otherwise identical",
                            sector, image1Sector.LongLength, image2Sector.LongLength).AppendLine();
                    }
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }
            DicConsole.WriteLine();

            if(imagesDiffer)
                sb.AppendLine("Images differ");
            else
                sb.AppendLine("Images do not differ");

            DicConsole.WriteLine(sb.ToString());

            Core.Statistics.AddCommand("compare");
        }
    }
}

