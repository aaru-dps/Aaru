/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Compare.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Verbs.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the 'compare' verb.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using DiscImageChef.ImagePlugins;
using System.Text;
using System.Collections.Generic;
using DiscImageChef.Console;

namespace DiscImageChef.Commands
{
    public static class Compare
    {
        public static void doCompare(CompareSubOptions options)
        {
            DicConsole.DebugWriteLine("Compare command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Compare command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Compare command", "--input1={0}", options.InputFile1);
            DicConsole.DebugWriteLine("Compare command", "--input2={0}", options.InputFile2);

            if (!System.IO.File.Exists(options.InputFile1))
            {
                System.Console.Error.WriteLine("Input file 1 does not exist.");
                return;
            }

            if (!System.IO.File.Exists(options.InputFile2))
            {
                System.Console.Error.WriteLine("Input file 2 does not exist.");
                return;
            }

            ImagePlugin input1Format = ImageFormat.Detect(options.InputFile1);
            ImagePlugin input2Format = ImageFormat.Detect(options.InputFile2);

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

            input1Format.OpenImage(options.InputFile1);
            input2Format.OpenImage(options.InputFile2);

            StringBuilder sb = new StringBuilder();

            if (options.Verbose)
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
            Dictionary<DiskTagType, byte[]> image1DiskTags = new Dictionary<DiskTagType, byte[]>();
            Dictionary<DiskTagType, byte[]> image2DiskTags = new Dictionary<DiskTagType, byte[]>();

            image1Info.imageHasPartitions = input1Format.ImageHasPartitions();
            try{ image1Sessions = input1Format.GetSessions(); } catch{}
            if (image1Sessions.Count > 0)
                image1Info.imageHasSessions = true;
            image1Info.imageSize = input1Format.GetImageSize();
            image1Info.sectors = input1Format.GetSectors();
            image1Info.sectorSize = input1Format.GetSectorSize();
            image1Info.imageCreationTime = input1Format.GetImageCreationTime();
            image1Info.imageLastModificationTime = input1Format.GetImageLastModificationTime();
            image1Info.diskType = input1Format.GetDiskType();
            try{ image1Info.imageVersion = input1Format.GetImageVersion(); } catch{ image1Info.imageVersion = null;}
            try{ image1Info.imageApplication = input1Format.GetImageApplication(); } catch{ image1Info.imageApplication = null;}
            try{ image1Info.imageApplicationVersion = input1Format.GetImageApplicationVersion(); } catch{ image1Info.imageApplicationVersion = null;}
            try{ image1Info.imageCreator = input1Format.GetImageCreator(); } catch{ image1Info.imageCreator = null;}
            try{ image1Info.imageName = input1Format.GetImageName(); } catch{ image1Info.imageName = null;}
            try{ image1Info.imageComments = input1Format.GetImageComments(); } catch{ image1Info.imageComments = null;}
            try{ image1Info.diskManufacturer = input1Format.GetDiskManufacturer(); } catch{ image1Info.diskManufacturer = null;}
            try{ image1Info.diskModel = input1Format.GetDiskModel(); } catch{ image1Info.diskModel = null;}
            try{ image1Info.diskSerialNumber = input1Format.GetDiskSerialNumber(); } catch{ image1Info.diskSerialNumber = null;}
            try{ image1Info.diskBarcode = input1Format.GetDiskBarcode(); } catch{ image1Info.diskBarcode = null;}
            try{ image1Info.diskPartNumber = input1Format.GetDiskPartNumber(); } catch{ image1Info.diskPartNumber = null;}
            try{ image1Info.diskSequence = input1Format.GetDiskSequence(); } catch{ image1Info.diskSequence = 0;}
            try{ image1Info.lastDiskSequence = input1Format.GetLastDiskSequence(); } catch{ image1Info.lastDiskSequence = 0;}
            try{ image1Info.driveManufacturer = input1Format.GetDriveManufacturer(); } catch{ image1Info.driveManufacturer = null;}
            try{ image1Info.driveModel = input1Format.GetDriveModel(); } catch{ image1Info.driveModel = null;}
            try{ image1Info.driveSerialNumber = input1Format.GetDriveSerialNumber(); } catch{ image1Info.driveSerialNumber = null;}
            foreach (DiskTagType disktag in Enum.GetValues(typeof(DiskTagType)))
            {
                try{
                    byte[] temparray = input1Format.ReadDiskTag(disktag);
                    image1DiskTags.Add(disktag, temparray);
                }
                catch{
                }
            }

            image2Info.imageHasPartitions = input2Format.ImageHasPartitions();
            try{ image2Sessions = input2Format.GetSessions(); } catch{}
            if (image2Sessions.Count > 0)
                image2Info.imageHasSessions = true;
            image2Info.imageSize = input2Format.GetImageSize();
            image2Info.sectors = input2Format.GetSectors();
            image2Info.sectorSize = input2Format.GetSectorSize();
            image2Info.imageCreationTime = input2Format.GetImageCreationTime();
            image2Info.imageLastModificationTime = input2Format.GetImageLastModificationTime();
            image2Info.diskType = input2Format.GetDiskType();
            try{ image2Info.imageVersion = input2Format.GetImageVersion(); } catch{ image2Info.imageVersion = null;}
            try{ image2Info.imageApplication = input2Format.GetImageApplication(); } catch{ image2Info.imageApplication = null;}
            try{ image2Info.imageApplicationVersion = input2Format.GetImageApplicationVersion(); } catch{ image2Info.imageApplicationVersion = null;}
            try{ image2Info.imageCreator = input2Format.GetImageCreator(); } catch{ image2Info.imageCreator = null;}
            try{ image2Info.imageName = input2Format.GetImageName(); } catch{ image2Info.imageName = null;}
            try{ image2Info.imageComments = input2Format.GetImageComments(); } catch{ image2Info.imageComments = null;}
            try{ image2Info.diskManufacturer = input2Format.GetDiskManufacturer(); } catch{ image2Info.diskManufacturer = null;}
            try{ image2Info.diskModel = input2Format.GetDiskModel(); } catch{ image2Info.diskModel = null;}
            try{ image2Info.diskSerialNumber = input2Format.GetDiskSerialNumber(); } catch{ image2Info.diskSerialNumber = null;}
            try{ image2Info.diskBarcode = input2Format.GetDiskBarcode(); } catch{ image2Info.diskBarcode = null;}
            try{ image2Info.diskPartNumber = input2Format.GetDiskPartNumber(); } catch{ image2Info.diskPartNumber = null;}
            try{ image2Info.diskSequence = input2Format.GetDiskSequence(); } catch{ image2Info.diskSequence = 0;}
            try{ image2Info.lastDiskSequence = input2Format.GetLastDiskSequence(); } catch{ image2Info.lastDiskSequence = 0;}
            try{ image2Info.driveManufacturer = input2Format.GetDriveManufacturer(); } catch{ image2Info.driveManufacturer = null;}
            try{ image2Info.driveModel = input2Format.GetDriveModel(); } catch{ image2Info.driveModel = null;}
            try{ image2Info.driveSerialNumber = input2Format.GetDriveSerialNumber(); } catch{ image2Info.driveSerialNumber = null;}
            foreach (DiskTagType disktag in Enum.GetValues(typeof(DiskTagType)))
            {
                try{
                    byte[] temparray = input2Format.ReadDiskTag(disktag);
                    image2DiskTags.Add(disktag, temparray);
                }
                catch{
                }
            }

            if (options.Verbose)
            {
                sb.AppendFormat("Has partitions?\t{0}\t{1}", image1Info.imageHasPartitions, image2Info.imageHasPartitions).AppendLine();
                sb.AppendFormat("Has sessions?\t{0}\t{1}", image1Info.imageHasSessions, image2Info.imageHasSessions).AppendLine();
                sb.AppendFormat("Image size\t{0}\t{1}", image1Info.imageSize, image2Info.imageSize).AppendLine();
                sb.AppendFormat("Sectors\t{0}\t{1}", image1Info.sectors, image2Info.sectors).AppendLine();
                sb.AppendFormat("Sector size\t{0}\t{1}", image1Info.sectorSize, image2Info.sectorSize).AppendLine();
                sb.AppendFormat("Creation time\t{0}\t{1}", image1Info.imageCreationTime, image2Info.imageCreationTime).AppendLine();
                sb.AppendFormat("Last modification time\t{0}\t{1}", image1Info.imageLastModificationTime, image2Info.imageLastModificationTime).AppendLine();
                sb.AppendFormat("Disk type\t{0}\t{1}", image1Info.diskType, image2Info.diskType).AppendLine();
                sb.AppendFormat("Image version\t{0}\t{1}", image1Info.imageVersion, image2Info.imageVersion).AppendLine();
                sb.AppendFormat("Image application\t{0}\t{1}", image1Info.imageApplication, image2Info.imageApplication).AppendLine();
                sb.AppendFormat("Image application version\t{0}\t{1}", image1Info.imageApplicationVersion, image2Info.imageApplicationVersion).AppendLine();
                sb.AppendFormat("Image creator\t{0}\t{1}", image1Info.imageCreator, image2Info.imageCreator).AppendLine();
                sb.AppendFormat("Image name\t{0}\t{1}", image1Info.imageName, image2Info.imageName).AppendLine();
                sb.AppendFormat("Image comments\t{0}\t{1}", image1Info.imageComments, image2Info.imageComments).AppendLine();
                sb.AppendFormat("Disk manufacturer\t{0}\t{1}", image1Info.diskManufacturer, image2Info.diskManufacturer).AppendLine();
                sb.AppendFormat("Disk model\t{0}\t{1}", image1Info.diskModel, image2Info.diskModel).AppendLine();
                sb.AppendFormat("Disk serial number\t{0}\t{1}", image1Info.diskSerialNumber, image2Info.diskSerialNumber).AppendLine();
                sb.AppendFormat("Disk barcode\t{0}\t{1}", image1Info.diskBarcode, image2Info.diskBarcode).AppendLine();
                sb.AppendFormat("Disk part no.\t{0}\t{1}", image1Info.diskPartNumber, image2Info.diskPartNumber).AppendLine();
                sb.AppendFormat("Disk sequence\t{0}\t{1}", image1Info.diskSequence, image2Info.diskSequence).AppendLine();
                sb.AppendFormat("Last disk on sequence\t{0}\t{1}", image1Info.lastDiskSequence, image2Info.lastDiskSequence).AppendLine();
                sb.AppendFormat("Drive manufacturer\t{0}\t{1}", image1Info.driveManufacturer, image2Info.driveManufacturer).AppendLine();
                sb.AppendFormat("Drive model\t{0}\t{1}", image1Info.driveModel, image2Info.driveModel).AppendLine();
                sb.AppendFormat("Drive serial number\t{0}\t{1}", image1Info.driveSerialNumber, image2Info.driveSerialNumber).AppendLine();
                foreach (DiskTagType disktag in Enum.GetValues(typeof(DiskTagType)))
                {
                    sb.AppendFormat("Has {0}?\t{1}\t{2}", disktag, image1DiskTags.ContainsKey(disktag), image2DiskTags.ContainsKey(disktag)).AppendLine();
                }
            }

            DicConsole.WriteLine("Comparing disk image characteristics");

            if (image1Info.imageHasPartitions != image2Info.imageHasPartitions)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image partitioned status differ");
            }
            if (image1Info.imageHasSessions != image2Info.imageHasSessions)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image session status differ");
            }
            if (image1Info.imageSize != image2Info.imageSize)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image size differ");
            }
            if (image1Info.sectors != image2Info.sectors)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image sectors differ");
            }
            if (image1Info.sectorSize != image2Info.sectorSize)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image sector size differ");
            }
            if (image1Info.imageCreationTime != image2Info.imageCreationTime)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image creation time differ");
            }
            if (image1Info.imageLastModificationTime != image2Info.imageLastModificationTime)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image last modification time differ");
            }
            if (image1Info.diskType != image2Info.diskType)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Disk type differ");
            }
            if (image1Info.imageVersion != image2Info.imageVersion)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image version differ");
            }
            if (image1Info.imageApplication != image2Info.imageApplication)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image application differ");
            }
            if (image1Info.imageApplicationVersion != image2Info.imageApplicationVersion)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image application version differ");
            }
            if (image1Info.imageCreator != image2Info.imageCreator)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image creator differ");
            }
            if (image1Info.imageName != image2Info.imageName)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image name differ");
            }
            if (image1Info.imageComments != image2Info.imageComments)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Image comments differ");
            }
            if (image1Info.diskManufacturer != image2Info.diskManufacturer)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Disk manufacturer differ");
            }
            if (image1Info.diskModel != image2Info.diskModel)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Disk model differ");
            }
            if (image1Info.diskSerialNumber != image2Info.diskSerialNumber)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Disk serial number differ");
            }
            if (image1Info.diskBarcode != image2Info.diskBarcode)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Disk barcode differ");
            }
            if (image1Info.diskPartNumber != image2Info.diskPartNumber)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Disk part number differ");
            }
            if (image1Info.diskSequence != image2Info.diskSequence)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Disk sequence differ");
            }
            if (image1Info.lastDiskSequence != image2Info.lastDiskSequence)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Last disk in sequence differ");
            }
            if (image1Info.driveManufacturer != image2Info.driveManufacturer)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Drive manufacturer differ");
            }
            if (image1Info.driveModel != image2Info.driveModel)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Drive model differ");
            }
            if (image1Info.driveSerialNumber != image2Info.driveSerialNumber)
            {
                imagesDiffer = true;
                if (!options.Verbose)
                    sb.AppendLine("Drive serial number differ");
            }

            UInt64 leastSectors;
            if (image1Info.sectors < image2Info.sectors)
            {
                imagesDiffer = true;
                leastSectors = image1Info.sectors;
                if (!options.Verbose)
                    sb.AppendLine("Image 2 has more sectors");
            }
            else if (image1Info.sectors > image2Info.sectors)
            {
                imagesDiffer = true;
                leastSectors = image2Info.sectors;
                if (!options.Verbose)
                    sb.AppendLine("Image 1 has more sectors");
            }
            else
                leastSectors = image1Info.sectors;

            DicConsole.WriteLine("Comparing sectors...");

            for (UInt64 sector = 0; sector < leastSectors; sector++)
            {
                DicConsole.Write("\rComparing sector {0} of {1}...", sector+1, leastSectors);
                try
                {
                    byte[] image1Sector = input1Format.ReadSector(sector);
                    byte[] image2Sector = input2Format.ReadSector(sector);
                    bool different, sameSize;
                    CompareBytes(out different, out sameSize, image1Sector, image2Sector);
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
                catch{}
            }
            DicConsole.WriteLine();

            if (imagesDiffer)
                sb.AppendLine("Images differ");
            else
                sb.AppendLine("Images do not differ");

            DicConsole.WriteLine(sb.ToString());
        }

        private static void CompareBytes(out bool different, out bool sameSize, byte[] compareArray1, byte[] compareArray2)
        {
            different = false;
            sameSize = true;

            Int64 leastBytes;
            if (compareArray1.LongLength < compareArray2.LongLength)
            {
                sameSize = false;
                leastBytes = compareArray1.LongLength;
            }
            else if (compareArray1.LongLength > compareArray2.LongLength)
            {
                sameSize = false;
                leastBytes = compareArray2.LongLength;
            }
            else
                leastBytes = compareArray1.LongLength;

            for (Int64 i = 0; i < leastBytes; i++)
            {
                if (compareArray1[i] != compareArray2[i])
                {
                    different = true;
                    return;
                }
            }
        }
    }
}

