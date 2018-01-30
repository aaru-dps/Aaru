// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Text;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.Bluray;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.Xbox;
using DiscImageChef.DiscImages;
using DDS = DiscImageChef.Decoders.DVD.DDS;
using DMI = DiscImageChef.Decoders.Xbox.DMI;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Core
{
    public static class ImageInfo
    {
        public static void PrintImageInfo(IMediaImage imageFormat)
        {
            DicConsole.WriteLine("Image information:");
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Version))
                DicConsole.WriteLine("Format: {0} version {1}", imageFormat.Format, imageFormat.Info.Version);
            else DicConsole.WriteLine("Format: {0}",            imageFormat.Format);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application) &&
               !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion))
                DicConsole.WriteLine("Was created with {0} version {1}", imageFormat.Info.Application,
                                     imageFormat.Info.ApplicationVersion);
            else if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application))
                DicConsole.WriteLine("Was created with {0}",                imageFormat.Info.Application);
            DicConsole.WriteLine("Image without headers is {0} bytes long", imageFormat.Info.ImageSize);
            DicConsole.WriteLine("Contains a media of {0} sectors with a maximum sector size of {1} bytes (if all sectors are of the same size this would be {2} bytes)",
                                 imageFormat.Info.Sectors, imageFormat.Info.SectorSize,
                                 imageFormat.Info.Sectors * imageFormat.Info.SectorSize);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
                DicConsole.WriteLine("Created by: {0}", imageFormat.Info.Creator);
            if(imageFormat.Info.CreationTime != DateTime.MinValue)
                DicConsole.WriteLine("Created on {0}", imageFormat.Info.CreationTime);
            if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
                DicConsole.WriteLine("Last modified on {0}", imageFormat.Info.LastModificationTime);
            DicConsole.WriteLine("Contains a media of type {0} and XML type {1}", imageFormat.Info.MediaType,
                                 imageFormat.Info.XmlMediaType);
            DicConsole.WriteLine("{0} partitions", imageFormat.Info.HasPartitions ? "Has" : "Doesn't have");
            DicConsole.WriteLine("{0} sessions",   imageFormat.Info.HasSessions ? "Has" : "Doesn't have");
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Comments))
                DicConsole.WriteLine("Comments: {0}", imageFormat.Info.Comments);
            if(imageFormat.Info.MediaSequence != 0 && imageFormat.Info.LastMediaSequence != 0)
                DicConsole.WriteLine("Media is number {0} on a set of {1} medias", imageFormat.Info.MediaSequence,
                                     imageFormat.Info.LastMediaSequence);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
                DicConsole.WriteLine("Media title: {0}", imageFormat.Info.MediaTitle);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
                DicConsole.WriteLine("Media manufacturer: {0}", imageFormat.Info.MediaManufacturer);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
                DicConsole.WriteLine("Media model: {0}", imageFormat.Info.MediaModel);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
                DicConsole.WriteLine("Media serial number: {0}", imageFormat.Info.MediaSerialNumber);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
                DicConsole.WriteLine("Media barcode: {0}", imageFormat.Info.MediaBarcode);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
                DicConsole.WriteLine("Media part number: {0}", imageFormat.Info.MediaPartNumber);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
                DicConsole.WriteLine("Drive manufacturer: {0}", imageFormat.Info.DriveManufacturer);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
                DicConsole.WriteLine("Drive model: {0}", imageFormat.Info.DriveModel);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
                DicConsole.WriteLine("Drive serial number: {0}", imageFormat.Info.DriveSerialNumber);
            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
                DicConsole.WriteLine("Drive firmware info: {0}", imageFormat.Info.DriveFirmwareRevision);
            if(imageFormat.Info.Cylinders > 0 && imageFormat.Info.Heads > 0 && imageFormat.Info.SectorsPerTrack > 0 && imageFormat.Info.XmlMediaType != XmlMediaType.OpticalDisc)
                DicConsole.WriteLine("Media geometry: {0} cylinders, {1} heads, {2} sectors per track",
                                     imageFormat.Info.Cylinders, imageFormat.Info.Heads,
                                     imageFormat.Info.SectorsPerTrack);

            if(imageFormat.Info.ReadableMediaTags != null && imageFormat.Info.ReadableMediaTags.Count > 0)
            {
                DicConsole.WriteLine("Contains {0} readable media tags:", imageFormat.Info.ReadableMediaTags.Count);
                foreach(MediaTagType tag in imageFormat.Info.ReadableMediaTags.OrderBy(t => t)) DicConsole.Write("{0} ", tag);
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableSectorTags != null && imageFormat.Info.ReadableSectorTags.Count > 0)
            {
                DicConsole.WriteLine("Contains {0} readable sector tags:", imageFormat.Info.ReadableSectorTags.Count);
                foreach(SectorTagType tag in imageFormat.Info.ReadableSectorTags.OrderBy(t => t)) DicConsole.Write("{0} ", tag);
                DicConsole.WriteLine();
            }

            DicConsole.WriteLine();
            PeripheralDeviceTypes scsiDeviceType = PeripheralDeviceTypes.DirectAccess;
            byte[]                scsiVendorId   = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SCSI_INQUIRY))
            {
                byte[] inquiry = imageFormat.ReadDiskTag(MediaTagType.SCSI_INQUIRY);

                scsiDeviceType = (PeripheralDeviceTypes)(inquiry[0] & 0x1F);
                if(inquiry.Length >= 16)
                {
                    scsiVendorId = new byte[8];
                    Array.Copy(inquiry, 8, scsiVendorId, 0, 8);
                }

                DicConsole.WriteLine("SCSI INQUIRY contained in image:");
                DicConsole.Write("{0}", Inquiry.Prettify(inquiry));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
            {
                byte[] identify = imageFormat.ReadDiskTag(MediaTagType.ATA_IDENTIFY);

                DicConsole.WriteLine("ATA IDENTIFY contained in image:");
                DicConsole.Write("{0}", Identify.Prettify(identify));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.ATAPI_IDENTIFY))
            {
                byte[] identify = imageFormat.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY);

                DicConsole.WriteLine("ATAPI IDENTIFY contained in image:");
                DicConsole.Write("{0}", Identify.Prettify(identify));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SCSI_MODESENSE_10))
            {
                byte[]             modeSense10 = imageFormat.ReadDiskTag(MediaTagType.SCSI_MODESENSE_10);
                Modes.DecodedMode? decMode     = Modes.DecodeMode10(modeSense10, scsiDeviceType);

                if(decMode.HasValue)
                {
                    DicConsole.WriteLine("SCSI MODE SENSE (10) contained in image:");
                    PrintScsiModePages.Print(decMode.Value, scsiDeviceType, scsiVendorId);
                    DicConsole.WriteLine();
                }
            }
            else if(imageFormat.Info.ReadableMediaTags != null &&
                    imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SCSI_MODESENSE_6))
            {
                byte[]             modeSense6 = imageFormat.ReadDiskTag(MediaTagType.SCSI_MODESENSE_6);
                Modes.DecodedMode? decMode    = Modes.DecodeMode6(modeSense6, scsiDeviceType);

                if(decMode.HasValue)
                {
                    DicConsole.WriteLine("SCSI MODE SENSE (6) contained in image:");
                    PrintScsiModePages.Print(decMode.Value, scsiDeviceType, scsiVendorId);
                    DicConsole.WriteLine();
                }
            }
            else if(imageFormat.Info.ReadableMediaTags != null &&
                    imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SCSI_MODEPAGE_2A))
            {
                byte[] mode2A = imageFormat.ReadDiskTag(MediaTagType.SCSI_MODEPAGE_2A);

                DicConsole.Write("{0}", Modes.PrettifyModePage_2A(mode2A));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_FullTOC))
            {
                byte[] toc = imageFormat.ReadDiskTag(MediaTagType.CD_FullTOC);

                DicConsole.WriteLine("CompactDisc Table of Contents contained in image:");
                DicConsole.Write("{0}", FullTOC.Prettify(toc));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_PMA))
            {
                byte[] pma = imageFormat.ReadDiskTag(MediaTagType.CD_PMA);

                DicConsole.WriteLine("CompactDisc Power Management Area contained in image:");
                DicConsole.Write("{0}", PMA.Prettify(pma));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_ATIP))
            {
                byte[] atip = imageFormat.ReadDiskTag(MediaTagType.CD_ATIP);

                DicConsole.WriteLine("CompactDisc Absolute Time In Pregroove (ATIP) contained in image:");
                DicConsole.Write("{0}", ATIP.Prettify(atip));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_TEXT))
            {
                byte[] cdtext = imageFormat.ReadDiskTag(MediaTagType.CD_TEXT);

                DicConsole.WriteLine("CompactDisc Lead-in's CD-Text contained in image:");
                DicConsole.Write("{0}", CDTextOnLeadIn.Prettify(cdtext));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_MCN))
            {
                byte[] mcn = imageFormat.ReadDiskTag(MediaTagType.CD_MCN);

                DicConsole.WriteLine("CompactDisc Media Catalogue Number contained in image: {0}",
                                     Encoding.UTF8.GetString(mcn));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVD_PFI))
            {
                byte[] pfi = imageFormat.ReadDiskTag(MediaTagType.DVD_PFI);

                DicConsole.WriteLine("DVD Physical Format Information contained in image:");
                DicConsole.Write("{0}", PFI.Prettify(pfi));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDRAM_DDS))
            {
                byte[] dds = imageFormat.ReadDiskTag(MediaTagType.DVDRAM_DDS);

                DicConsole.WriteLine("DVD-RAM Disc Definition Structure contained in image:");
                DicConsole.Write("{0}", DDS.Prettify(dds));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDR_PFI))
            {
                byte[] pfi = imageFormat.ReadDiskTag(MediaTagType.DVDR_PFI);

                DicConsole.WriteLine("DVD-R Physical Format Information contained in image:");
                DicConsole.Write("{0}", PFI.Prettify(pfi));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.BD_DI))
            {
                byte[] di = imageFormat.ReadDiskTag(MediaTagType.BD_DI);

                DicConsole.WriteLine("Bluray Disc Information contained in image:");
                DicConsole.Write("{0}", DI.Prettify(di));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.BD_DDS))
            {
                byte[] dds = imageFormat.ReadDiskTag(MediaTagType.BD_DDS);

                DicConsole.WriteLine("Bluray Disc Definition Structure contained in image:");
                DicConsole.Write("{0}", Decoders.Bluray.DDS.Prettify(dds));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.PCMCIA_CIS))
            {
                byte[] cis = imageFormat.ReadDiskTag(MediaTagType.PCMCIA_CIS);

                DicConsole.WriteLine("PCMCIA CIS:");
                Tuple[] tuples = CIS.GetTuples(cis);
                if(tuples != null)
                    foreach(Tuple tuple in tuples)
                        switch(tuple.Code)
                        {
                            case TupleCodes.CISTPL_NULL:
                            case TupleCodes.CISTPL_END: break;
                            case TupleCodes.CISTPL_DEVICEGEO:
                            case TupleCodes.CISTPL_DEVICEGEO_A:
                                DicConsole.WriteLine("{0}", CIS.PrettifyDeviceGeometryTuple(tuple));
                                break;
                            case TupleCodes.CISTPL_MANFID:
                                DicConsole.WriteLine("{0}", CIS.PrettifyManufacturerIdentificationTuple(tuple));
                                break;
                            case TupleCodes.CISTPL_VERS_1:
                                DicConsole.WriteLine("{0}", CIS.PrettifyLevel1VersionTuple(tuple));
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
                                DicConsole.DebugWriteLine("Device-Info command", "Found undecoded tuple ID {0}",
                                                          tuple.Code);
                                break;
                            default:
                                DicConsole.DebugWriteLine("Device-Info command", "Found unknown tuple ID 0x{0:X2}",
                                                          (byte)tuple.Code);
                                break;
                        }
                else DicConsole.DebugWriteLine("Device-Info command", "Could not get tuples");
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SD_CID))
            {
                byte[] cid = imageFormat.ReadDiskTag(MediaTagType.SD_CID);

                DicConsole.WriteLine("SecureDigital CID contained in image:");
                DicConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyCID(cid));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SD_CSD))
            {
                byte[] csd = imageFormat.ReadDiskTag(MediaTagType.SD_CSD);

                DicConsole.WriteLine("SecureDigital CSD contained in image:");
                DicConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyCSD(csd));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SD_SCR))
            {
                byte[] scr = imageFormat.ReadDiskTag(MediaTagType.SD_SCR);

                DicConsole.WriteLine("SecureDigital SCR contained in image:");
                DicConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifySCR(scr));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SD_OCR))
            {
                byte[] ocr = imageFormat.ReadDiskTag(MediaTagType.SD_OCR);

                DicConsole.WriteLine("SecureDigital OCR contained in image:");
                DicConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyOCR(ocr));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_CID))
            {
                byte[] cid = imageFormat.ReadDiskTag(MediaTagType.MMC_CID);

                DicConsole.WriteLine("MultiMediaCard CID contained in image:");
                DicConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyCID(cid));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_CSD))
            {
                byte[] csd = imageFormat.ReadDiskTag(MediaTagType.MMC_CSD);

                DicConsole.WriteLine("MultiMediaCard CSD contained in image:");
                DicConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyCSD(csd));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_ExtendedCSD))
            {
                byte[] ecsd = imageFormat.ReadDiskTag(MediaTagType.MMC_ExtendedCSD);

                DicConsole.WriteLine("MultiMediaCard ExtendedCSD contained in image:");
                DicConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyExtendedCSD(ecsd));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_OCR))
            {
                byte[] ocr = imageFormat.ReadDiskTag(MediaTagType.MMC_OCR);

                DicConsole.WriteLine("MultiMediaCard OCR contained in image:");
                DicConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyOCR(ocr));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.Xbox_PFI))
            {
                byte[] xpfi = imageFormat.ReadDiskTag(MediaTagType.Xbox_PFI);

                DicConsole.WriteLine("Xbox Physical Format Information contained in image:");
                DicConsole.Write("{0}", PFI.Prettify(xpfi));
                DicConsole.WriteLine();
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.Xbox_DMI))
            {
                byte[] xdmi = imageFormat.ReadDiskTag(MediaTagType.Xbox_DMI);

                if(DMI.IsXbox(xdmi))
                {
                    DMI.XboxDMI? xmi = DMI.DecodeXbox(xdmi);
                    if(xmi.HasValue)
                    {
                        DicConsole.WriteLine("Xbox DMI contained in image:");
                        DicConsole.Write("{0}", DMI.PrettifyXbox(xmi));
                        DicConsole.WriteLine();
                    }
                }

                if(DMI.IsXbox360(xdmi))
                {
                    DMI.Xbox360DMI? xmi = DMI.DecodeXbox360(xdmi);
                    if(xmi.HasValue)
                    {
                        DicConsole.WriteLine("Xbox 360 DMI contained in image:");
                        DicConsole.Write("{0}", DMI.PrettifyXbox360(xmi));
                        DicConsole.WriteLine();
                    }
                }
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.Xbox_SecuritySector))
            {
                byte[] toc = imageFormat.ReadDiskTag(MediaTagType.Xbox_SecuritySector);

                DicConsole.WriteLine("Xbox Security Sectors contained in image:");
                DicConsole.Write("{0}", SS.Prettify(toc));
                DicConsole.WriteLine();
            }
        }
    }
}