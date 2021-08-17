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
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using Inquiry = Aaru.Decoders.SCSI.Inquiry;
using Session = Aaru.CommonTypes.Structs.Session;
using Tuple = Aaru.Decoders.PCMCIA.Tuple;

namespace Aaru.Core
{
    /// <summary>
    /// Image information operations
    /// </summary>
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

        /// <summary>
        /// Prints image information to console
        /// </summary>
        /// <param name="imageFormat">Media image</param>
        public static void PrintImageInfo(IMediaImage imageFormat)
        {
            AaruConsole.WriteLine("Image information:");

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Version))
                AaruConsole.WriteLine("Format: {0} version {1}", imageFormat.Format, imageFormat.Info.Version);
            else
                AaruConsole.WriteLine("Format: {0}", imageFormat.Format);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application) &&
               !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion))
                AaruConsole.WriteLine("Was created with {0} version {1}", imageFormat.Info.Application,
                                      imageFormat.Info.ApplicationVersion);
            else if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application))
                AaruConsole.WriteLine("Was created with {0}", imageFormat.Info.Application);

            AaruConsole.WriteLine("Image without headers is {0} bytes long", imageFormat.Info.ImageSize);

            AaruConsole.
                WriteLine("Contains a media of {0} sectors with a maximum sector size of {1} bytes (if all sectors are of the same size this would be {2} bytes)",
                          imageFormat.Info.Sectors, imageFormat.Info.SectorSize,
                          imageFormat.Info.Sectors * imageFormat.Info.SectorSize);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
                AaruConsole.WriteLine("Created by: {0}", imageFormat.Info.Creator);

            if(imageFormat.Info.CreationTime != DateTime.MinValue)
                AaruConsole.WriteLine("Created on {0}", imageFormat.Info.CreationTime);

            if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
                AaruConsole.WriteLine("Last modified on {0}", imageFormat.Info.LastModificationTime);

            AaruConsole.WriteLine("Contains a media of type {0} and XML type {1}", imageFormat.Info.MediaType,
                                  imageFormat.Info.XmlMediaType);

            AaruConsole.WriteLine("{0} partitions", imageFormat.Info.HasPartitions ? "Has" : "Doesn't have");
            AaruConsole.WriteLine("{0} sessions", imageFormat.Info.HasSessions ? "Has" : "Doesn't have");

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Comments))
                AaruConsole.WriteLine("Comments: {0}", imageFormat.Info.Comments);

            if(imageFormat.Info.MediaSequence     != 0 &&
               imageFormat.Info.LastMediaSequence != 0)
                AaruConsole.WriteLine("Media is number {0} on a set of {1} medias", imageFormat.Info.MediaSequence,
                                      imageFormat.Info.LastMediaSequence);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
                AaruConsole.WriteLine("Media title: {0}", imageFormat.Info.MediaTitle);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
                AaruConsole.WriteLine("Media manufacturer: {0}", imageFormat.Info.MediaManufacturer);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
                AaruConsole.WriteLine("Media model: {0}", imageFormat.Info.MediaModel);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
                AaruConsole.WriteLine("Media serial number: {0}", imageFormat.Info.MediaSerialNumber);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
                AaruConsole.WriteLine("Media barcode: {0}", imageFormat.Info.MediaBarcode);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
                AaruConsole.WriteLine("Media part number: {0}", imageFormat.Info.MediaPartNumber);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
                AaruConsole.WriteLine("Drive manufacturer: {0}", imageFormat.Info.DriveManufacturer);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
                AaruConsole.WriteLine("Drive model: {0}", imageFormat.Info.DriveModel);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
                AaruConsole.WriteLine("Drive serial number: {0}", imageFormat.Info.DriveSerialNumber);

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
                AaruConsole.WriteLine("Drive firmware info: {0}", imageFormat.Info.DriveFirmwareRevision);

            if(imageFormat.Info.Cylinders       > 0                         &&
               imageFormat.Info.Heads           > 0                         &&
               imageFormat.Info.SectorsPerTrack > 0                         &&
               imageFormat.Info.XmlMediaType    != XmlMediaType.OpticalDisc &&
               (!(imageFormat is ITapeImage tapeImage) || !tapeImage.IsTape))
                AaruConsole.WriteLine("Media geometry: {0} cylinders, {1} heads, {2} sectors per track",
                                      imageFormat.Info.Cylinders, imageFormat.Info.Heads,
                                      imageFormat.Info.SectorsPerTrack);

            if(imageFormat.Info.ReadableMediaTags       != null &&
               imageFormat.Info.ReadableMediaTags.Count > 0)
            {
                AaruConsole.WriteLine("Contains {0} readable media tags:", imageFormat.Info.ReadableMediaTags.Count);

                foreach(MediaTagType tag in imageFormat.Info.ReadableMediaTags.OrderBy(t => t))
                    AaruConsole.Write("{0} ", tag);

                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableSectorTags       != null &&
               imageFormat.Info.ReadableSectorTags.Count > 0)
            {
                AaruConsole.WriteLine("Contains {0} readable sector tags:", imageFormat.Info.ReadableSectorTags.Count);

                foreach(SectorTagType tag in imageFormat.Info.ReadableSectorTags.OrderBy(t => t))
                    AaruConsole.Write("{0} ", tag);

                AaruConsole.WriteLine();
            }

            AaruConsole.WriteLine();
            PeripheralDeviceTypes scsiDeviceType = PeripheralDeviceTypes.DirectAccess;
            byte[]                scsiVendorId   = null;

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_INQUIRY) == true)
            {
                byte[] inquiry = imageFormat.ReadDiskTag(MediaTagType.SCSI_INQUIRY);

                scsiDeviceType = (PeripheralDeviceTypes)(inquiry[0] & 0x1F);

                if(inquiry.Length >= 16)
                {
                    scsiVendorId = new byte[8];
                    Array.Copy(inquiry, 8, scsiVendorId, 0, 8);
                }

                AaruConsole.WriteLine("SCSI INQUIRY contained in image:");
                AaruConsole.Write("{0}", Inquiry.Prettify(inquiry));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATA_IDENTIFY) == true)
            {
                byte[] identify = imageFormat.ReadDiskTag(MediaTagType.ATA_IDENTIFY);

                AaruConsole.WriteLine("ATA IDENTIFY contained in image:");
                AaruConsole.Write("{0}", Identify.Prettify(identify));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATAPI_IDENTIFY) == true)
            {
                byte[] identify = imageFormat.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY);

                AaruConsole.WriteLine("ATAPI IDENTIFY contained in image:");
                AaruConsole.Write("{0}", Identify.Prettify(identify));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODESENSE_10) == true)
            {
                byte[]             modeSense10 = imageFormat.ReadDiskTag(MediaTagType.SCSI_MODESENSE_10);
                Modes.DecodedMode? decMode     = Modes.DecodeMode10(modeSense10, scsiDeviceType);

                if(decMode.HasValue)
                {
                    AaruConsole.WriteLine("SCSI MODE SENSE (10) contained in image:");
                    PrintScsiModePages.Print(decMode.Value, scsiDeviceType, scsiVendorId);
                    AaruConsole.WriteLine();
                }
            }
            else if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODESENSE_6) == true)
            {
                byte[]             modeSense6 = imageFormat.ReadDiskTag(MediaTagType.SCSI_MODESENSE_6);
                Modes.DecodedMode? decMode    = Modes.DecodeMode6(modeSense6, scsiDeviceType);

                if(decMode.HasValue)
                {
                    AaruConsole.WriteLine("SCSI MODE SENSE (6) contained in image:");
                    PrintScsiModePages.Print(decMode.Value, scsiDeviceType, scsiVendorId);
                    AaruConsole.WriteLine();
                }
            }
            else if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODEPAGE_2A) == true)
            {
                byte[] mode2A = imageFormat.ReadDiskTag(MediaTagType.SCSI_MODEPAGE_2A);

                AaruConsole.Write("{0}", Modes.PrettifyModePage_2A(mode2A));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_FullTOC) == true)
            {
                byte[] toc = imageFormat.ReadDiskTag(MediaTagType.CD_FullTOC);

                if(toc.Length > 0)
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

                    AaruConsole.WriteLine("CompactDisc Table of Contents contained in image:");
                    AaruConsole.Write("{0}", FullTOC.Prettify(toc));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_PMA) == true)
            {
                byte[] pma = imageFormat.ReadDiskTag(MediaTagType.CD_PMA);

                if(pma.Length > 0)
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

                    AaruConsole.WriteLine("CompactDisc Power Management Area contained in image:");
                    AaruConsole.Write("{0}", PMA.Prettify(pma));
                    AaruConsole.WriteLine();
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_ATIP) == true)
            {
                byte[] atip = imageFormat.ReadDiskTag(MediaTagType.CD_ATIP);

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

                AaruConsole.WriteLine("CompactDisc Absolute Time In Pregroove (ATIP) contained in image:");
                AaruConsole.Write("{0}", ATIP.Prettify(atip));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_TEXT) == true)
            {
                byte[] cdtext = imageFormat.ReadDiskTag(MediaTagType.CD_TEXT);

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

                AaruConsole.WriteLine("CompactDisc Lead-in's CD-Text contained in image:");
                AaruConsole.Write("{0}", CDTextOnLeadIn.Prettify(cdtext));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_MCN) == true)
            {
                byte[] mcn = imageFormat.ReadDiskTag(MediaTagType.CD_MCN);

                AaruConsole.WriteLine("CompactDisc Media Catalogue Number contained in image: {0}",
                                      Encoding.UTF8.GetString(mcn));

                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PreRecordedInfo) == true)
            {
                byte[] pri = imageFormat.ReadDiskTag(MediaTagType.DVDR_PreRecordedInfo);

                AaruConsole.WriteLine("DVD-R(W) Pre-Recorded Information:");
                AaruConsole.Write("{0}", PRI.Prettify(pri));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_PFI) == true)
            {
                byte[] pfi = imageFormat.ReadDiskTag(MediaTagType.DVD_PFI);

                AaruConsole.WriteLine("DVD Physical Format Information contained in image:");
                AaruConsole.Write("{0}", PFI.Prettify(pfi, imageFormat.Info.MediaType));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDRAM_DDS) == true)
            {
                byte[] dds = imageFormat.ReadDiskTag(MediaTagType.DVDRAM_DDS);

                AaruConsole.WriteLine("DVD-RAM Disc Definition Structure contained in image:");
                AaruConsole.Write("{0}", DDS.Prettify(dds));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PFI) == true)
            {
                byte[] pfi = imageFormat.ReadDiskTag(MediaTagType.DVDR_PFI);

                AaruConsole.WriteLine("DVD-R Physical Format Information contained in image:");
                AaruConsole.Write("{0}", PFI.Prettify(pfi, imageFormat.Info.MediaType));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DI) == true)
            {
                byte[] di = imageFormat.ReadDiskTag(MediaTagType.BD_DI);

                AaruConsole.WriteLine("Bluray Disc Information contained in image:");
                AaruConsole.Write("{0}", DI.Prettify(di));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DDS) == true)
            {
                byte[] dds = imageFormat.ReadDiskTag(MediaTagType.BD_DDS);

                AaruConsole.WriteLine("Bluray Disc Definition Structure contained in image:");
                AaruConsole.Write("{0}", Decoders.Bluray.DDS.Prettify(dds));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.PCMCIA_CIS) == true)
            {
                byte[] cis = imageFormat.ReadDiskTag(MediaTagType.PCMCIA_CIS);

                AaruConsole.WriteLine("PCMCIA CIS:");
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

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CID) == true)
            {
                byte[] cid = imageFormat.ReadDiskTag(MediaTagType.SD_CID);

                AaruConsole.WriteLine("SecureDigital CID contained in image:");
                AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyCID(cid));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CSD) == true)
            {
                byte[] csd = imageFormat.ReadDiskTag(MediaTagType.SD_CSD);

                AaruConsole.WriteLine("SecureDigital CSD contained in image:");
                AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyCSD(csd));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_SCR) == true)
            {
                byte[] scr = imageFormat.ReadDiskTag(MediaTagType.SD_SCR);

                AaruConsole.WriteLine("SecureDigital SCR contained in image:");
                AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifySCR(scr));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_OCR) == true)
            {
                byte[] ocr = imageFormat.ReadDiskTag(MediaTagType.SD_OCR);

                AaruConsole.WriteLine("SecureDigital OCR contained in image:");
                AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyOCR(ocr));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CID) == true)
            {
                byte[] cid = imageFormat.ReadDiskTag(MediaTagType.MMC_CID);

                AaruConsole.WriteLine("MultiMediaCard CID contained in image:");
                AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyCID(cid));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CSD) == true)
            {
                byte[] csd = imageFormat.ReadDiskTag(MediaTagType.MMC_CSD);

                AaruConsole.WriteLine("MultiMediaCard CSD contained in image:");
                AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyCSD(csd));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_ExtendedCSD) == true)
            {
                byte[] ecsd = imageFormat.ReadDiskTag(MediaTagType.MMC_ExtendedCSD);

                AaruConsole.WriteLine("MultiMediaCard ExtendedCSD contained in image:");
                AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyExtendedCSD(ecsd));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_OCR) == true)
            {
                byte[] ocr = imageFormat.ReadDiskTag(MediaTagType.MMC_OCR);

                AaruConsole.WriteLine("MultiMediaCard OCR contained in image:");
                AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyOCR(ocr));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_PFI) == true)
            {
                byte[] xpfi = imageFormat.ReadDiskTag(MediaTagType.Xbox_PFI);

                AaruConsole.WriteLine("Xbox Physical Format Information contained in image:");
                AaruConsole.Write("{0}", PFI.Prettify(xpfi, imageFormat.Info.MediaType));
                AaruConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_DMI) == true)
            {
                byte[] xdmi = imageFormat.ReadDiskTag(MediaTagType.Xbox_DMI);

                if(DMI.IsXbox(xdmi))
                {
                    DMI.XboxDMI? xmi = DMI.DecodeXbox(xdmi);

                    if(xmi.HasValue)
                    {
                        AaruConsole.WriteLine("Xbox DMI contained in image:");
                        AaruConsole.Write("{0}", DMI.PrettifyXbox(xmi));
                        AaruConsole.WriteLine();
                    }
                }

                if(DMI.IsXbox360(xdmi))
                {
                    DMI.Xbox360DMI? xmi = DMI.DecodeXbox360(xdmi);

                    if(xmi.HasValue)
                    {
                        AaruConsole.WriteLine("Xbox 360 DMI contained in image:");
                        AaruConsole.Write("{0}", DMI.PrettifyXbox360(xmi));
                        AaruConsole.WriteLine();
                    }
                }
            }

            if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_SecuritySector) == true)
            {
                byte[] toc = imageFormat.ReadDiskTag(MediaTagType.Xbox_SecuritySector);

                AaruConsole.WriteLine("Xbox Security Sectors contained in image:");
                AaruConsole.Write("{0}", SS.Prettify(toc));
                AaruConsole.WriteLine();
            }

            if(imageFormat is IOpticalMediaImage opticalImage)
            {
                try
                {
                    if(opticalImage.Sessions       != null &&
                       opticalImage.Sessions.Count > 0)
                    {
                        AaruConsole.WriteLine("Image sessions:");

                        AaruConsole.WriteLine("{0,-9}{1,-13}{2,-12}{3,-12}{4,-12}", "Session", "First track",
                                              "Last track", "Start", "End");

                        AaruConsole.WriteLine("=========================================================");

                        foreach(Session session in opticalImage.Sessions)
                            AaruConsole.WriteLine("{0,-9}{1,-13}{2,-12}{3,-12}{4,-12}", session.SessionSequence,
                                                  session.StartTrack, session.EndTrack, session.StartSector,
                                                  session.EndSector);

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
                        AaruConsole.WriteLine("Image tracks:");

                        AaruConsole.WriteLine("{0,-7}{1,-17}{2,-6}{3,-8}{4,-12}{5,-8}{6,-12}{7,-12}", "Track", "Type",
                                              "Bps", "Raw bps", "Subchannel", "Pregap", "Start", "End");

                        AaruConsole.
                            WriteLine("=================================================================================");

                        foreach(Track track in opticalImage.Tracks)
                            AaruConsole.WriteLine("{0,-7}{1,-17}{2,-6}{3,-8}{4,-12}{5,-8}{6,-12}{7,-12}",
                                                  track.TrackSequence, track.TrackType, track.TrackBytesPerSector,
                                                  track.TrackRawBytesPerSector, track.TrackSubchannelType,
                                                  track.TrackPregap, track.TrackStartSector, track.TrackEndSector);

                        if(opticalImage.Tracks.Any(t => t.Indexes.Any()))
                        {
                            AaruConsole.WriteLine();

                            AaruConsole.WriteLine("Track indexes:");

                            AaruConsole.WriteLine("{0,-7}{1,-7}{2,-12}", "Track", "Index", "Start");

                            AaruConsole.WriteLine("=======================");

                            foreach(Track track in opticalImage.Tracks)
                                foreach(KeyValuePair<ushort, int> index in track.Indexes)
                                    AaruConsole.WriteLine("{0,-7}{1,-7}{2,-12}", track.TrackSequence, index.Key,
                                                          index.Value);
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

            manufacturerLen += 2;
            modelLen        += 2;
            serialLen       += 2;
            softwareLen     += 2;
            versionLen      += 2;
            osLen           += 2;
            sectorLen       += 2;
            sectorLen       += 2;

            char[] separator = new char[manufacturerLen + modelLen + serialLen + softwareLen + versionLen + osLen +
                                        sectorLen       + sectorLen];

            for(int i = 0; i < separator.Length; i++)
                separator[i] = '=';

            string format =
                $"{{0,-{manufacturerLen}}}{{1,-{modelLen}}}{{2,-{serialLen}}}{{3,-{softwareLen}}}{{4,-{versionLen}}}{{5,-{osLen}}}{{6,-{sectorLen}}}{{7,-{sectorLen}}}";

            AaruConsole.WriteLine("Dump hardware information:");

            AaruConsole.WriteLine(format, MANUFACTURER_STRING, MODEL_STRING, SERIAL_STRING, SOFTWARE_STRING,
                                  VERSION_STRING, OS_STRING, START_STRING, END_STRING);

            AaruConsole.WriteLine(new string(separator));

            foreach(DumpHardwareType dump in imageFormat.DumpHardware)
            {
                foreach(ExtentType extent in dump.Extents)
                    AaruConsole.WriteLine(format, dump.Manufacturer, dump.Model, dump.Serial, dump.Software.Name,
                                          dump.Software.Version, dump.Software.OperatingSystem, extent.Start,
                                          extent.End);
            }

            AaruConsole.WriteLine();
        }
    }
}