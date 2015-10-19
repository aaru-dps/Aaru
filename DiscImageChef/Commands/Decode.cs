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
using DiscImageChef.Console;

namespace DiscImageChef.Commands
{
    public static class Decode
    {
        public static void doDecode(DecodeSubOptions options)
        {
            DicConsole.DebugWriteLine("Decode command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Decode command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Decode command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Decode command", "--start={0}", options.StartSector);
            DicConsole.DebugWriteLine("Decode command", "--length={0}", options.Length);
            DicConsole.DebugWriteLine("Decode command", "--disk-tags={0}", options.DiskTags);
            DicConsole.DebugWriteLine("Decode command", "--sector-tags={0}", options.SectorTags);

            ImagePlugin inputFormat = ImageFormat.Detect(options.InputFile);

            if (inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not decoding");
                return;
            }

            inputFormat.OpenImage(options.InputFile);

            if (options.DiskTags)
            {
                if (inputFormat.ImageInfo.readableDiskTags.Count == 0)
                    DicConsole.WriteLine("There are no disk tags in chosen disc image.");
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
                                        DicConsole.WriteLine("Error reading SCSI INQUIRY response from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("SCSI INQUIRY command response:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.SCSI.PrettifySCSIInquiry(inquiry));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case DiskTagType.ATA_IDENTIFY:
                                {
                                    byte[] identify = inputFormat.ReadDiskTag(DiskTagType.ATA_IDENTIFY);
                                    if (identify == null)
                                        DicConsole.WriteLine("Error reading ATA IDENTIFY DEVICE response from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("ATA IDENTIFY DEVICE command response:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.ATA.PrettifyIdentifyDevice(identify));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case DiskTagType.ATAPI_IDENTIFY:
                                {
                                    byte[] identify = inputFormat.ReadDiskTag(DiskTagType.ATAPI_IDENTIFY);
                                    if (identify == null)
                                        DicConsole.WriteLine("Error reading ATA IDENTIFY PACKET DEVICE response from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("ATA IDENTIFY PACKET DEVICE command response:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.ATA.PrettifyIdentifyDevice(identify));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case DiskTagType.CD_ATIP:
                                {
                                    byte[] atip = inputFormat.ReadDiskTag(DiskTagType.CD_ATIP);
                                    if (atip == null)
                                        DicConsole.WriteLine("Error reading CD ATIP from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD ATIP:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.ATIP.PrettifyCDATIP(atip));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case DiskTagType.CD_FullTOC:
                                {
                                    byte[] fulltoc = inputFormat.ReadDiskTag(DiskTagType.CD_FullTOC);
                                    if (fulltoc == null)
                                        DicConsole.WriteLine("Error reading CD full TOC from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD full TOC:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.FullTOC.PrettifyCDFullTOC(fulltoc));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case DiskTagType.CD_PMA:
                                {
                                    byte[] pma = inputFormat.ReadDiskTag(DiskTagType.CD_PMA);
                                    if (pma == null)
                                        DicConsole.WriteLine("Error reading CD PMA from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD PMA:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.PMA.PrettifyCDPMA(pma));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case DiskTagType.CD_SessionInfo:
                                {
                                    byte[] sessioninfo = inputFormat.ReadDiskTag(DiskTagType.CD_SessionInfo);
                                    if (sessioninfo == null)
                                        DicConsole.WriteLine("Error reading CD session information from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD session information:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.Session.PrettifyCDSessionInfo(sessioninfo));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case DiskTagType.CD_TEXT:
                                {
                                    byte[] cdtext = inputFormat.ReadDiskTag(DiskTagType.CD_TEXT);
                                    if (cdtext == null)
                                        DicConsole.WriteLine("Error reading CD-TEXT from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD-TEXT:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.CDTextOnLeadIn.PrettifyCDTextLeadIn(cdtext));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case DiskTagType.CD_TOC:
                                {
                                    byte[] toc = inputFormat.ReadDiskTag(DiskTagType.CD_TOC);
                                    if (toc == null)
                                        DicConsole.WriteLine("Error reading CD TOC from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD TOC:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.TOC.PrettifyCDTOC(toc));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            default:
                                DicConsole.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.", tag);
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
                        DicConsole.WriteLine("Value \"{0}\" is not a valid number for length.", options.Length);
                        DicConsole.WriteLine("Not decoding sectors tags");
                        return;
                    }
                }

                if (inputFormat.ImageInfo.readableSectorTags.Count == 0)
                    DicConsole.WriteLine("There are no sector tags in chosen disc image.");
                else
                {
                    foreach (SectorTagType tag in inputFormat.ImageInfo.readableSectorTags)
                    {
                        switch (tag)
                        {
                            default:
                                DicConsole.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.", tag);
                                break;
                        }
                    }
                }
            }
        }
    }
}

