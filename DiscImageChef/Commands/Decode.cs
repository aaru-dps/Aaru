/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : PrintHex.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Verbs.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the 'decode' verb.
 
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

namespace DiscImageChef.Commands
{
    public static class Decode
    {
        public static void doDecode(DecodeSubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--input={0}", options.InputFile);
                Console.WriteLine("--start={0}", options.StartSector);
                Console.WriteLine("--length={0}", options.Length);
                Console.WriteLine("--disk-tags={0}", options.DiskTags);
                Console.WriteLine("--sector-tags={0}", options.SectorTags);
            }

            ImagePlugin inputFormat = ImageFormat.Detect(options.InputFile);

            if (inputFormat == null)
            {
                Console.WriteLine("Unable to recognize image format, not verifying");
                return;
            }

            inputFormat.OpenImage(options.InputFile);

            if (options.DiskTags)
            {
                if (inputFormat.ImageInfo.readableDiskTags.Count == 0)
                    Console.WriteLine("There are no disk tags in chosen disc image.");
                else
                {
                    foreach (DiskTagType tag in inputFormat.ImageInfo.readableDiskTags)
                    {
                        switch (tag)
                        {
                            case DiskTagType.SCSI_INQUIRY:
                                {
                                    byte[] inquiry = inputFormat.ReadDiskTag(DiskTagType.SCSI_INQUIRY);
                                    if (inquiry == null)
                                        Console.WriteLine("Error reading SCSI INQUIRY response from disc image");
                                    else
                                    {
                                        Console.WriteLine("SCSI INQUIRY command response:");
                                        Console.WriteLine("================================================================================");
                                        Console.WriteLine(Decoders.SCSI.PrettifySCSIInquiry(inquiry));
                                        Console.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            default:
                                Console.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.", tag);
                                break;
                        }
                    }
                }
            }

            if (options.SectorTags)
            {
                UInt64 length;

                if (options.Length.ToLowerInvariant() == "all")
                    length = inputFormat.GetSectors() - 1;
                else
                {
                    if (!UInt64.TryParse(options.Length, out length))
                    {
                        Console.WriteLine("Value \"{0}\" is not a valid number for length.", options.Length);
                        Console.WriteLine("Not decoding sectors tags");
                        return;
                    }
                }

                if (inputFormat.ImageInfo.readableSectorTags.Count == 0)
                    Console.WriteLine("There are no sector tags in chosen disc image.");
                else
                {
                    foreach (SectorTagType tag in inputFormat.ImageInfo.readableSectorTags)
                    {
                        switch (tag)
                        {
                            default:
                                Console.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.", tag);
                                break;
                        }
                    }
                }
            }
        }
    }
}

