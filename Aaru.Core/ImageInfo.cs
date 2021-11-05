// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prints image information to console.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.ATA;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.PCMCIA;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.Xbox;
using Aaru.Helpers;
using Schemas;
using Spectre.Console;
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using Inquiry = Aaru.Decoders.SCSI.Inquiry;
using Session = Aaru.CommonTypes.Structs.Session;
using Tuple = Aaru.Decoders.PCMCIA.Tuple;

namespace Aaru.Core
{
    /// <summary>Image information operations</summary>
    public static class ImageInfo
    {
        const string MANUFACTURER_STRING = "Manufacturer";
        const string MODEL_STRING        = "Model";
        const string SERIAL_STRING       = "Serial";
        const string SOFTWARE_STRING     = "Software";
        const string VERSION_STRING      = "Version";
        const string OS_STRING           = "Operating system";
        const string START_STRING        = "Start";
        const string END_STRING          = "End";

        /// <summary>Prints image information to console</summary>
        /// <param name="imageFormat">Media image</param>
        public static void PrintImageInfo(IMediaImage imageFormat)
        {
            Table table;

            AaruConsole.WriteLine("[bold]Image information:[/]");

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Version))
                AaruConsole.WriteLine("[bold]Format:[/] [italic]{0}[/] version {1}", Markup.Escape(imageFormat.Format),
                                      Markup.Escape(imageFormat.Info.Version));
            else
                AaruConsole.WriteLine("[bold]Format:[/] [italic]{0}[/]", Markup.Escape(imageFormat.Format));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application) &&
               !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion))
                AaruConsole.WriteLine("Was created with [italic]{0}[/] version [italic]{1}[/]",
                                      Markup.Escape(imageFormat.Info.Application),
                                      Markup.Escape(imageFormat.Info.ApplicationVersion));
            else if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application))
                AaruConsole.WriteLine("Was created with [italic]{0}[/]", Markup.Escape(imageFormat.Info.Application));

            AaruConsole.WriteLine("Image without headers is {0} bytes long", imageFormat.Info.ImageSize);

            AaruConsole.
                WriteLine("Contains a media of {0} sectors with a maximum sector size of {1} bytes (if all sectors are of the same size this would be {2} bytes)",
                          imageFormat.Info.Sectors, imageFormat.Info.SectorSize,
                          imageFormat.Info.Sectors * imageFormat.Info.SectorSize);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
                AaruConsole.WriteLine("[bold]Created by:[/] {0}", Markup.Escape(imageFormat.Info.Creator));

            if(imageFormat.Info.CreationTime != DateTime.MinValue)
                AaruConsole.WriteLine("Created on {0}", imageFormat.Info.CreationTime);

            if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
                AaruConsole.WriteLine("Last modified on {0}", imageFormat.Info.LastModificationTime);

            AaruConsole.WriteLine("Contains a media of type [italic]{0}[/] and XML type [italic]{1}[/]",
                                  imageFormat.Info.MediaType, imageFormat.Info.XmlMediaType);

            AaruConsole.WriteLine("{0} partitions", imageFormat.Info.HasPartitions ? "Has" : "Doesn't have");
            AaruConsole.WriteLine("{0} sessions", imageFormat.Info.HasSessions ? "Has" : "Doesn't have");

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Comments))
                AaruConsole.WriteLine("[bold]Comments:[/] {0}", Markup.Escape(imageFormat.Info.Comments));

            if(imageFormat.Info.MediaSequence     != 0 &&
               imageFormat.Info.LastMediaSequence != 0)
                AaruConsole.WriteLine("Media is number {0} on a set of {1} medias", imageFormat.Info.MediaSequence,
                                      imageFormat.Info.LastMediaSequence);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
                AaruConsole.WriteLine("[bold]Media title:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.MediaTitle));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
                AaruConsole.WriteLine("[bold]Media manufacturer:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.MediaManufacturer));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
                AaruConsole.WriteLine("[bold]Media model:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.MediaModel));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
                AaruConsole.WriteLine("[bold]Media serial number:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.MediaSerialNumber));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
                AaruConsole.WriteLine("[bold]Media barcode:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.MediaBarcode));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
                AaruConsole.WriteLine("[bold]Media part number:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.MediaPartNumber));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
                AaruConsole.WriteLine("[bold]Drive manufacturer:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.DriveManufacturer));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
                AaruConsole.WriteLine("[bold]Drive model:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.DriveModel));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
                AaruConsole.WriteLine("[bold]Drive serial number:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.DriveSerialNumber));

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
                AaruConsole.WriteLine("[bold]Drive firmware info:[/] [italic]{0}[/]",
                                      Markup.Escape(imageFormat.Info.DriveFirmwareRevision));

            if(imageFormat.Info.Cylinders       > 0                         &&
               imageFormat.Info.Heads           > 0                         &&
               imageFormat.Info.SectorsPerTrack > 0                         &&
               imageFormat.Info.XmlMediaType    != XmlMediaType.OpticalDisc &&
               (!(imageFormat is ITapeImage tapeImage) || !tapeImage.IsTape))
                AaruConsole.
                    WriteLine("[bold]Media geometry:[/] [italic]{0} cylinders, {1} heads, {2} sectors per track[/]",
                              imageFormat.Info.Cylinders, imageFormat.Info.Heads, imageFormat.Info.SectorsPerTrack);

            if(imageFormat.Info.ReadableMediaTags       != null &&
               imageFormat.Info.ReadableMediaTags.Count > 0)
            {
                AaruConsole.WriteLine("[bold]Contains {0} readable media tags:[/]",
                                      imageFormat.Info.ReadableMediaTags.Count);

                foreach(MediaTagType tag in imageFormat.Info.ReadableMediaTags.OrderBy(t => t))
                    AaruConsole.Write("[italic]{0}[/] ", Markup.Escape(tag.ToString()));

                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableSectorTags       != null &&
               imageFormat.Info.ReadableSectorTags.Count > 0)
            {
                AaruConsole.WriteLine("[bold]Contains {0} readable sector tags:[/]",
                                      imageFormat.Info.ReadableSectorTags.Count);

                foreach(SectorTagType tag in imageFormat.Info.ReadableSectorTags.OrderBy(t => t))
                    AaruConsole.Write("[italic]{0}[/] ", tag);

                AaruConsole.WriteLine();
            }

            AaruConsole.WriteLine();
            PeripheralDeviceTypes scsiDeviceType = PeripheralDeviceTypes.DirectAccess;
            byte[]                scsiVendorId   = null;
            ErrorNumber           errno;

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_INQUIRY) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.SCSI_INQUIRY, out byte[] inquiry);

                if(errno == ErrorNumber.NoError)
                {
                    scsiDeviceType = (PeripheralDeviceTypes)(inquiry[0] & 0x1F);

                    if(inquiry.Length >= 16)
                    {
                        scsiVendorId = new byte[8];
                        Array.Copy(inquiry, 8, scsiVendorId, 0, 8);
                    }

                    AaruConsole.WriteLine("[bold]SCSI INQUIRY contained in image:[/]");
                    AaruConsole.Write("{0}", Inquiry.Prettify(inquiry));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATA_IDENTIFY) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.ATA_IDENTIFY, out byte[] identify);

                if(errno == ErrorNumber.NoError)

                {
                    AaruConsole.WriteLine("[bold]ATA IDENTIFY contained in image:[/]");
                    AaruConsole.Write("{0}", Identify.Prettify(identify));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATAPI_IDENTIFY) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.ATAPI_IDENTIFY, out byte[] identify);

                if(errno == ErrorNumber.NoError)

                {
                    AaruConsole.WriteLine("[bold]ATAPI IDENTIFY contained in image:[/]");
                    AaruConsole.Write("{0}", Identify.Prettify(identify));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODESENSE_10) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.SCSI_MODESENSE_10, out byte[] modeSense10);

                if(errno == ErrorNumber.NoError)

                {
                    Modes.DecodedMode? decMode = Modes.DecodeMode10(modeSense10, scsiDeviceType);

                    if(decMode.HasValue)
                    {
                        AaruConsole.WriteLine("[bold]SCSI MODE SENSE (10) contained in image:[/]");
                        PrintScsiModePages.Print(decMode.Value, scsiDeviceType, scsiVendorId);
                        AaruConsole.WriteLine();
                    }
                }
            }
            else if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODESENSE_6) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.SCSI_MODESENSE_6, out byte[] modeSense6);

                if(errno == ErrorNumber.NoError)
                {
                    Modes.DecodedMode? decMode = Modes.DecodeMode6(modeSense6, scsiDeviceType);

                    if(decMode.HasValue)
                    {
                        AaruConsole.WriteLine("[bold]SCSI MODE SENSE (6) contained in image:[/]");
                        PrintScsiModePages.Print(decMode.Value, scsiDeviceType, scsiVendorId);
                        AaruConsole.WriteLine();
                    }
                }
            }
            else if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODEPAGE_2A) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.SCSI_MODEPAGE_2A, out byte[] mode2A);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.Write("{0}", Modes.PrettifyModePage_2A(mode2A));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_FullTOC) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.CD_FullTOC, out byte[] toc);

                if(errno      == ErrorNumber.NoError &&
                   toc.Length > 0)
                {
                    ushort dataLen = Swapping.Swap(BitConverter.ToUInt16(toc, 0));

                    if(dataLen + 2 != toc.Length)
                    {
                        byte[] tmp = new byte[toc.Length + 2];
                        Array.Copy(toc, 0, tmp, 2, toc.Length);
                        tmp[0] = (byte)((toc.Length & 0xFF00) >> 8);
                        tmp[1] = (byte)(toc.Length & 0xFF);
                        toc    = tmp;
                    }

                    AaruConsole.WriteLine("[bold]CompactDisc Table of Contents contained in image:[/]");
                    AaruConsole.Write("{0}", FullTOC.Prettify(toc));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_PMA) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.CD_PMA, out byte[] pma);

                if(errno      == ErrorNumber.NoError &&
                   pma.Length > 0)
                {
                    ushort dataLen = Swapping.Swap(BitConverter.ToUInt16(pma, 0));

                    if(dataLen + 2 != pma.Length)
                    {
                        byte[] tmp = new byte[pma.Length + 2];
                        Array.Copy(pma, 0, tmp, 2, pma.Length);
                        tmp[0] = (byte)((pma.Length & 0xFF00) >> 8);
                        tmp[1] = (byte)(pma.Length & 0xFF);
                        pma    = tmp;
                    }

                    AaruConsole.WriteLine("[bold]CompactDisc Power Management Area contained in image:[/]");
                    AaruConsole.Write("{0}", PMA.Prettify(pma));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_ATIP) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.CD_ATIP, out byte[] atip);

                if(errno == ErrorNumber.NoError)
                {
                    uint dataLen = Swapping.Swap(BitConverter.ToUInt32(atip, 0));

                    if(dataLen + 4 != atip.Length)
                    {
                        byte[] tmp = new byte[atip.Length + 4];
                        Array.Copy(atip, 0, tmp, 4, atip.Length);
                        tmp[0] = (byte)((atip.Length & 0xFF000000) >> 24);
                        tmp[1] = (byte)((atip.Length & 0xFF0000)   >> 16);
                        tmp[2] = (byte)((atip.Length & 0xFF00)     >> 8);
                        tmp[3] = (byte)(atip.Length & 0xFF);
                        atip   = tmp;
                    }

                    AaruConsole.WriteLine("[bold]CompactDisc Absolute Time In Pregroove (ATIP) contained in image:[/]");
                    AaruConsole.Write("{0}", ATIP.Prettify(atip));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_TEXT) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.CD_TEXT, out byte[] cdtext);

                if(errno == ErrorNumber.NoError)
                {
                    uint dataLen = Swapping.Swap(BitConverter.ToUInt32(cdtext, 0));

                    if(dataLen + 4 != cdtext.Length)
                    {
                        byte[] tmp = new byte[cdtext.Length + 4];
                        Array.Copy(cdtext, 0, tmp, 4, cdtext.Length);
                        tmp[0] = (byte)((cdtext.Length & 0xFF000000) >> 24);
                        tmp[1] = (byte)((cdtext.Length & 0xFF0000)   >> 16);
                        tmp[2] = (byte)((cdtext.Length & 0xFF00)     >> 8);
                        tmp[3] = (byte)(cdtext.Length & 0xFF);
                        cdtext = tmp;
                    }

                    AaruConsole.WriteLine("[bold]CompactDisc Lead-in's CD-Text contained in image:[/]");
                    AaruConsole.Write("{0}", CDTextOnLeadIn.Prettify(cdtext));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_MCN) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.CD_MCN, out byte[] mcn);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]CompactDisc Media Catalogue Number contained in image:[/] {0}",
                                          Encoding.UTF8.GetString(mcn));

                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PreRecordedInfo) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.DVDR_PreRecordedInfo, out byte[] pri);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]DVD-R(W) Pre-Recorded Information:[/]");
                    AaruConsole.Write("{0}", PRI.Prettify(pri));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_PFI) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.DVD_PFI, out byte[] pfi);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]DVD Physical Format Information contained in image:[/]");
                    AaruConsole.Write("{0}", PFI.Prettify(pfi, imageFormat.Info.MediaType));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDRAM_DDS) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.DVDRAM_DDS, out byte[] dds);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]DVD-RAM Disc Definition Structure contained in image:[/]");
                    AaruConsole.Write("{0}", DDS.Prettify(dds));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PFI) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.DVDR_PFI, out byte[] pfi);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]DVD-R Physical Format Information contained in image:[/]");
                    AaruConsole.Write("{0}", PFI.Prettify(pfi, imageFormat.Info.MediaType));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DI) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.BD_DI, out byte[] di);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]Bluray Disc Information contained in image:[/]");
                    AaruConsole.Write("{0}", DI.Prettify(di));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DDS) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.BD_DDS, out byte[] dds);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]Bluray Disc Definition Structure contained in image:[/]");
                    AaruConsole.Write("{0}", Decoders.Bluray.DDS.Prettify(dds));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.PCMCIA_CIS) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.PCMCIA_CIS, out byte[] cis);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]PCMCIA CIS:[/]");
                    Tuple[] tuples = CIS.GetTuples(cis);

                    if(tuples != null)
                        foreach(Tuple tuple in tuples)
                            switch(tuple.Code)
                            {
                                case TupleCodes.CISTPL_NULL:
                                case TupleCodes.CISTPL_END: break;
                                case TupleCodes.CISTPL_DEVICEGEO:
                                case TupleCodes.CISTPL_DEVICEGEO_A:
                                    AaruConsole.WriteLine("{0}", CIS.PrettifyDeviceGeometryTuple(tuple));

                                    break;
                                case TupleCodes.CISTPL_MANFID:
                                    AaruConsole.WriteLine("{0}", CIS.PrettifyManufacturerIdentificationTuple(tuple));

                                    break;
                                case TupleCodes.CISTPL_VERS_1:
                                    AaruConsole.WriteLine("{0}", CIS.PrettifyLevel1VersionTuple(tuple));

                                    break;
                                case TupleCodes.CISTPL_ALTSTR:
                                case TupleCodes.CISTPL_BAR:
                                case TupleCodes.CISTPL_BATTERY:
                                case TupleCodes.CISTPL_BYTEORDER:
                                case TupleCodes.CISTPL_CFTABLE_ENTRY:
                                case TupleCodes.CISTPL_CFTABLE_ENTRY_CB:
                                case TupleCodes.CISTPL_CHECKSUM:
                                case TupleCodes.CISTPL_CONFIG:
                                case TupleCodes.CISTPL_CONFIG_CB:
                                case TupleCodes.CISTPL_DATE:
                                case TupleCodes.CISTPL_DEVICE:
                                case TupleCodes.CISTPL_DEVICE_A:
                                case TupleCodes.CISTPL_DEVICE_OA:
                                case TupleCodes.CISTPL_DEVICE_OC:
                                case TupleCodes.CISTPL_EXTDEVIC:
                                case TupleCodes.CISTPL_FORMAT:
                                case TupleCodes.CISTPL_FORMAT_A:
                                case TupleCodes.CISTPL_FUNCE:
                                case TupleCodes.CISTPL_FUNCID:
                                case TupleCodes.CISTPL_GEOMETRY:
                                case TupleCodes.CISTPL_INDIRECT:
                                case TupleCodes.CISTPL_JEDEC_A:
                                case TupleCodes.CISTPL_JEDEC_C:
                                case TupleCodes.CISTPL_LINKTARGET:
                                case TupleCodes.CISTPL_LONGLINK_A:
                                case TupleCodes.CISTPL_LONGLINK_C:
                                case TupleCodes.CISTPL_LONGLINK_CB:
                                case TupleCodes.CISTPL_LONGLINK_MFC:
                                case TupleCodes.CISTPL_NO_LINK:
                                case TupleCodes.CISTPL_ORG:
                                case TupleCodes.CISTPL_PWR_MGMNT:
                                case TupleCodes.CISTPL_SPCL:
                                case TupleCodes.CISTPL_SWIL:
                                case TupleCodes.CISTPL_VERS_2:
                                    AaruConsole.DebugWriteLine("Device-Info command", "Found undecoded tuple ID {0}",
                                                               tuple.Code);

                                    break;
                                default:
                                    AaruConsole.DebugWriteLine("Device-Info command", "Found unknown tuple ID 0x{0:X2}",
                                                               (byte)tuple.Code);

                                    break;
                            }
                    else
                        AaruConsole.DebugWriteLine("Device-Info command", "Could not get tuples");
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CID) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.SD_CID, out byte[] cid);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]SecureDigital CID contained in image:[/]");
                    AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyCID(cid));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CSD) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.SD_CSD, out byte[] csd);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]SecureDigital CSD contained in image:[/]");
                    AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyCSD(csd));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_SCR) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.SD_SCR, out byte[] scr);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]SecureDigital SCR contained in image:[/]");
                    AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifySCR(scr));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_OCR) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.SD_OCR, out byte[] ocr);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]SecureDigital OCR contained in image:[/]");
                    AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyOCR(ocr));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CID) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.MMC_CID, out byte[] cid);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]MultiMediaCard CID contained in image:[/]");
                    AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyCID(cid));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CSD) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.MMC_CSD, out byte[] csd);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]MultiMediaCard CSD contained in image:[/]");
                    AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyCSD(csd));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_ExtendedCSD) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.MMC_ExtendedCSD, out byte[] ecsd);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]MultiMediaCard ExtendedCSD contained in image:[/]");
                    AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyExtendedCSD(ecsd));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_OCR) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.MMC_OCR, out byte[] ocr);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]MultiMediaCard OCR contained in image:[/]");
                    AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyOCR(ocr));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_PFI) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.Xbox_PFI, out byte[] xpfi);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]Xbox Physical Format Information contained in image:[/]");
                    AaruConsole.Write("{0}", PFI.Prettify(xpfi, imageFormat.Info.MediaType));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_DMI) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.Xbox_DMI, out byte[] xdmi);

                if(errno == ErrorNumber.NoError)
                {
                    if(DMI.IsXbox(xdmi))
                    {
                        DMI.XboxDMI? xmi = DMI.DecodeXbox(xdmi);

                        if(xmi.HasValue)
                        {
                            AaruConsole.WriteLine("[bold]Xbox DMI contained in image:[/]");
                            AaruConsole.Write("{0}", DMI.PrettifyXbox(xmi));
                            AaruConsole.WriteLine();
                        }
                    }

                    if(DMI.IsXbox360(xdmi))
                    {
                        DMI.Xbox360DMI? xmi = DMI.DecodeXbox360(xdmi);

                        if(xmi.HasValue)
                        {
                            AaruConsole.WriteLine("[bold]Xbox 360 DMI contained in image:[/]");
                            AaruConsole.Write("{0}", DMI.PrettifyXbox360(xmi));
                            AaruConsole.WriteLine();
                        }
                    }
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_SecuritySector) == true)
            {
                errno = imageFormat.ReadMediaTag(MediaTagType.Xbox_SecuritySector, out byte[] toc);

                if(errno == ErrorNumber.NoError)
                {
                    AaruConsole.WriteLine("[bold]Xbox Security Sectors contained in image:[/]");
                    AaruConsole.Write("{0}", SS.Prettify(toc));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat is IOpticalMediaImage opticalImage)
            {
                try
                {
                    if(opticalImage.Sessions       != null &&
                       opticalImage.Sessions.Count > 0)
                    {
                        table = new Table
                        {
                            Title = new TableTitle("Image sessions")
                        };

                        table.AddColumn("Session");
                        table.AddColumn("First track");
                        table.AddColumn("Last track");
                        table.AddColumn("Start");
                        table.AddColumn("End");

                        foreach(Session session in opticalImage.Sessions)
                            table.AddRow(session.Sequence.ToString(), session.StartTrack.ToString(),
                                         session.EndTrack.ToString(), session.StartSector.ToString(),
                                         session.EndSector.ToString());

                        AnsiConsole.Render(table);
                        AaruConsole.WriteLine();
                    }
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if(opticalImage.Tracks       != null &&
                       opticalImage.Tracks.Count > 0)
                    {
                        table = new Table
                        {
                            Title = new TableTitle("Image tracks")
                        };

                        table.AddColumn("Track");
                        table.AddColumn("Type");
                        table.AddColumn("Bps");
                        table.AddColumn("Raw bps");
                        table.AddColumn("Subchannel");
                        table.AddColumn("Pregap");
                        table.AddColumn("Start");
                        table.AddColumn("End");

                        foreach(Track track in opticalImage.Tracks)
                            table.AddRow(track.Sequence.ToString(), track.Type.ToString(),
                                         track.BytesPerSector.ToString(), track.RawBytesPerSector.ToString(),
                                         track.SubchannelType.ToString(), track.Pregap.ToString(),
                                         track.StartSector.ToString(), track.EndSector.ToString());

                        AnsiConsole.Render(table);

                        if(opticalImage.Tracks.Any(t => t.Indexes.Any()))
                        {
                            AaruConsole.WriteLine();

                            table = new Table
                            {
                                Title = new TableTitle("Image indexes")
                            };

                            table.AddColumn("Track");
                            table.AddColumn("Index");
                            table.AddColumn("Start");

                            foreach(Track track in opticalImage.Tracks)
                                foreach(KeyValuePair<ushort, int> index in track.Indexes)
                                    table.AddRow(track.Sequence.ToString(), index.Key.ToString(),
                                                 index.Value.ToString());

                            AnsiConsole.Render(table);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.DumpHardware == null)
                return;

            int manufacturerLen = MANUFACTURER_STRING.Length;
            int modelLen        = MODEL_STRING.Length;
            int serialLen       = SERIAL_STRING.Length;
            int softwareLen     = SOFTWARE_STRING.Length;
            int versionLen      = VERSION_STRING.Length;
            int osLen           = OS_STRING.Length;
            int sectorLen       = START_STRING.Length;

            foreach(DumpHardwareType dump in imageFormat.DumpHardware)
            {
                if(dump.Manufacturer?.Length > manufacturerLen)
                    manufacturerLen = dump.Manufacturer.Length;

                if(dump.Model?.Length > modelLen)
                    modelLen = dump.Model.Length;

                if(dump.Serial?.Length > serialLen)
                    serialLen = dump.Serial.Length;

                if(dump.Software?.Name?.Length > softwareLen)
                    softwareLen = dump.Software.Name.Length;

                if(dump.Software?.Version?.Length > versionLen)
                    versionLen = dump.Software.Version.Length;

                if(dump.Software?.OperatingSystem?.Length > osLen)
                    osLen = dump.Software.OperatingSystem.Length;

                foreach(ExtentType extent in dump.Extents)
                {
                    if($"{extent.Start}".Length > sectorLen)
                        sectorLen = $"{extent.Start}".Length;

                    if($"{extent.End}".Length > sectorLen)
                        sectorLen = $"{extent.End}".Length;
                }
            }

            table = new Table
            {
                Title = new TableTitle("Dump hardware information")
            };

            table.AddColumn(MANUFACTURER_STRING);
            table.AddColumn(MODEL_STRING);
            table.AddColumn(SERIAL_STRING);
            table.AddColumn(SOFTWARE_STRING);
            table.AddColumn(VERSION_STRING);
            table.AddColumn(OS_STRING);
            table.AddColumn(START_STRING);
            table.AddColumn(END_STRING);

            foreach(DumpHardwareType dump in imageFormat.DumpHardware)
            {
                foreach(ExtentType extent in dump.Extents)
                    table.AddRow(Markup.Escape(dump.Manufacturer ?? ""), Markup.Escape(dump.Model ?? ""),
                                 Markup.Escape(dump.Serial ?? ""), Markup.Escape(dump.Software.Name ?? ""),
                                 Markup.Escape(dump.Software.Version ?? ""),
                                 Markup.Escape(dump.Software.OperatingSystem ?? ""), extent.Start.ToString(),
                                 extent.End.ToString());
            }

            AaruConsole.WriteLine();
        }
    }
}