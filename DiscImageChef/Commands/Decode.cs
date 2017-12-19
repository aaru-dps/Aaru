// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Decode.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'decode' verb.
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

using DiscImageChef.ImagePlugins;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    public static class Decode
    {
        public static void doDecode(DecodeOptions options)
        {
            DicConsole.DebugWriteLine("Decode command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Decode command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Decode command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Decode command", "--start={0}", options.StartSector);
            DicConsole.DebugWriteLine("Decode command", "--length={0}", options.Length);
            DicConsole.DebugWriteLine("Decode command", "--disk-tags={0}", options.DiskTags);
            DicConsole.DebugWriteLine("Decode command", "--sector-tags={0}", options.SectorTags);

            FiltersList filtersList = new FiltersList();
            Filter inputFilter = filtersList.GetFilter(options.InputFile);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            ImagePlugin inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not decoding");
                return;
            }

            inputFormat.OpenImage(inputFilter);
            Core.Statistics.AddMediaFormat(inputFormat.GetImageFormat());
            Core.Statistics.AddMedia(inputFormat.ImageInfo.mediaType, false);
            Core.Statistics.AddFilter(inputFilter.Name);

            if(options.DiskTags)
            {
                if(inputFormat.ImageInfo.readableMediaTags.Count == 0)
                    DicConsole.WriteLine("There are no disk tags in chosen disc image.");
                else
                {
                    foreach(MediaTagType tag in inputFormat.ImageInfo.readableMediaTags)
                    {
                        switch(tag)
                        {
                            case MediaTagType.SCSI_INQUIRY:
                                {
                                    byte[] inquiry = inputFormat.ReadDiskTag(MediaTagType.SCSI_INQUIRY);
                                    if(inquiry == null)
                                        DicConsole.WriteLine("Error reading SCSI INQUIRY response from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("SCSI INQUIRY command response:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.SCSI.Inquiry.Prettify(inquiry));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case MediaTagType.ATA_IDENTIFY:
                                {
                                    byte[] identify = inputFormat.ReadDiskTag(MediaTagType.ATA_IDENTIFY);
                                    if(identify == null)
                                        DicConsole.WriteLine("Error reading ATA IDENTIFY DEVICE response from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("ATA IDENTIFY DEVICE command response:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.ATA.Identify.Prettify(identify));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case MediaTagType.ATAPI_IDENTIFY:
                                {
                                    byte[] identify = inputFormat.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY);
                                    if(identify == null)
                                        DicConsole.WriteLine("Error reading ATA IDENTIFY PACKET DEVICE response from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("ATA IDENTIFY PACKET DEVICE command response:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.ATA.Identify.Prettify(identify));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case MediaTagType.CD_ATIP:
                                {
                                    byte[] atip = inputFormat.ReadDiskTag(MediaTagType.CD_ATIP);
                                    if(atip == null)
                                        DicConsole.WriteLine("Error reading CD ATIP from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD ATIP:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.ATIP.Prettify(atip));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case MediaTagType.CD_FullTOC:
                                {
                                    byte[] fulltoc = inputFormat.ReadDiskTag(MediaTagType.CD_FullTOC);
                                    if(fulltoc == null)
                                        DicConsole.WriteLine("Error reading CD full TOC from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD full TOC:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.FullTOC.Prettify(fulltoc));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case MediaTagType.CD_PMA:
                                {
                                    byte[] pma = inputFormat.ReadDiskTag(MediaTagType.CD_PMA);
                                    if(pma == null)
                                        DicConsole.WriteLine("Error reading CD PMA from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD PMA:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.PMA.Prettify(pma));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case MediaTagType.CD_SessionInfo:
                                {
                                    byte[] sessioninfo = inputFormat.ReadDiskTag(MediaTagType.CD_SessionInfo);
                                    if(sessioninfo == null)
                                        DicConsole.WriteLine("Error reading CD session information from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD session information:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.Session.Prettify(sessioninfo));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case MediaTagType.CD_TEXT:
                                {
                                    byte[] cdtext = inputFormat.ReadDiskTag(MediaTagType.CD_TEXT);
                                    if(cdtext == null)
                                        DicConsole.WriteLine("Error reading CD-TEXT from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD-TEXT:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.CDTextOnLeadIn.Prettify(cdtext));
                                        DicConsole.WriteLine("================================================================================");
                                    }
                                    break;
                                }
                            case MediaTagType.CD_TOC:
                                {
                                    byte[] toc = inputFormat.ReadDiskTag(MediaTagType.CD_TOC);
                                    if(toc == null)
                                        DicConsole.WriteLine("Error reading CD TOC from disc image");
                                    else
                                    {
                                        DicConsole.WriteLine("CD TOC:");
                                        DicConsole.WriteLine("================================================================================");
                                        DicConsole.WriteLine(Decoders.CD.TOC.Prettify(toc));
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

            if(options.SectorTags)
            {
                ulong length;

                if(options.Length.ToLowerInvariant() == "all")
                    length = inputFormat.GetSectors() - 1;
                else
                {
                    if(!ulong.TryParse(options.Length, out length))
                    {
                        DicConsole.WriteLine("Value \"{0}\" is not a valid number for length.", options.Length);
                        DicConsole.WriteLine("Not decoding sectors tags");
                        return;
                    }
                }

                if(inputFormat.ImageInfo.readableSectorTags.Count == 0)
                    DicConsole.WriteLine("There are no sector tags in chosen disc image.");
                else
                {
                    foreach(SectorTagType tag in inputFormat.ImageInfo.readableSectorTags)
                    {
                        switch(tag)
                        {
                            default:
                                DicConsole.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.", tag);
                                break;
                        }
                    }
                }
            }

            Core.Statistics.AddCommand("decode");
        }
    }
}

