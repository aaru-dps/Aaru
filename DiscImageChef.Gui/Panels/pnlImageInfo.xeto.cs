// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : pnlDeviceInfo.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the device information panel.
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
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.Xbox;
using DiscImageChef.Devices;
using DiscImageChef.Gui.Tabs;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Schemas;
using Session = DiscImageChef.CommonTypes.Structs.Session;

namespace DiscImageChef.Gui.Panels
{
    public class pnlImageInfo : Panel
    {
        public pnlImageInfo(string imagePath, IFilter filter, IMediaImage imageFormat)
        {
            XamlReader.Load(this);

            lblImagePath.Text   = $"Path: {imagePath}";
            lblFilter.Text      = $"Filter: {filter.Name}";
            lblImageFormat.Text = $"Image format identified by {imageFormat.Name} ({imageFormat.Id}).";
            lblImageFormat.Text = !string.IsNullOrWhiteSpace(imageFormat.Info.Version)
                                      ? $"Format: {imageFormat.Format} version {imageFormat.Info.Version}"
                                      : $"Format: {imageFormat.Format}";
            lblImageSize.Text = $"Image without headers is {imageFormat.Info.ImageSize} bytes long";
            lblSectors.Text =
                $"Contains a media of {imageFormat.Info.Sectors} sectors with a maximum sector size of {imageFormat.Info.SectorSize} bytes (if all sectors are of the same size this would be {imageFormat.Info.Sectors * imageFormat.Info.SectorSize} bytes)";
            lblMediaType.Text =
                $"Contains a media of type {imageFormat.Info.MediaType} and XML type {imageFormat.Info.XmlMediaType}";
            lblHasPartitions.Text = $"{(imageFormat.Info.HasPartitions ? "Has" : "Doesn't have")} partitions";
            lblHasSessions.Text   = $"{(imageFormat.Info.HasSessions ? "Has" : "Doesn't have")} sessions";

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application))
            {
                lblApplication.Visible = true;
                lblApplication.Text = !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion)
                                          ? $"Was created with {imageFormat.Info.Application} version {imageFormat.Info.ApplicationVersion}"
                                          : $"Was created with {imageFormat.Info.Application}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
            {
                lblCreator.Visible = true;
                lblCreator.Text    = $"Created by: {imageFormat.Info.Creator}";
            }

            if(imageFormat.Info.CreationTime != DateTime.MinValue)
            {
                lblCreationTime.Visible = true;
                lblCreationTime.Text    = $"Created on {imageFormat.Info.CreationTime}";
            }

            if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
            {
                lblLastModificationTime.Visible = true;
                lblLastModificationTime.Text    = $"Last modified on {imageFormat.Info.LastModificationTime}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.Comments))
            {
                grpComments.Visible = true;
                txtComments.Text    = imageFormat.Info.Comments;
            }

            if(imageFormat.Info.MediaSequence != 0 && imageFormat.Info.LastMediaSequence != 0)
            {
                lblMediaSequence.Visible = true;
                lblMediaSequence.Text =
                    $"Media is number {imageFormat.Info.MediaSequence} on a set of {imageFormat.Info.LastMediaSequence} medias";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
            {
                lblMediaTitle.Visible = true;
                lblMediaTitle.Text    = $"Media title: {imageFormat.Info.MediaTitle}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
            {
                lblMediaManufacturer.Visible = true;
                lblMediaManufacturer.Text    = $"Media manufacturer: {imageFormat.Info.MediaManufacturer}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
            {
                lblMediaModel.Visible = true;
                lblMediaModel.Text    = $"Media model: {imageFormat.Info.MediaModel}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
            {
                lblMediaSerialNumber.Visible = true;
                lblMediaSerialNumber.Text    = $"Media serial number: {imageFormat.Info.MediaSerialNumber}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
            {
                lblMediaBarcode.Visible = true;
                lblMediaBarcode.Text    = $"Media barcode: {imageFormat.Info.MediaBarcode}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
            {
                lblMediaPartNumber.Visible = true;
                lblMediaPartNumber.Text    = $"Media part number: {imageFormat.Info.MediaPartNumber}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
            {
                lblDriveManufacturer.Visible = true;
                lblDriveManufacturer.Text    = $"Drive manufacturer: {imageFormat.Info.DriveManufacturer}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
            {
                lblDriveModel.Visible = true;
                lblDriveModel.Text    = $"Drive model: {imageFormat.Info.DriveModel}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
            {
                lblDriveSerialNumber.Visible = true;
                lblDriveSerialNumber.Text    = $"Drive serial number: {imageFormat.Info.DriveSerialNumber}";
            }

            if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
            {
                lblDriveFirmwareRevision.Visible = true;
                lblDriveFirmwareRevision.Text    = $"Drive firmware info: {imageFormat.Info.DriveFirmwareRevision}";
            }

            if(imageFormat.Info.Cylinders       > 0 && imageFormat.Info.Heads > 0 &&
               imageFormat.Info.SectorsPerTrack > 0 &&
               imageFormat.Info.XmlMediaType    != XmlMediaType.OpticalDisc)
            {
                lblMediaGeometry.Visible = true;
                lblMediaGeometry.Text =
                    $"Media geometry: {imageFormat.Info.Cylinders} cylinders, {imageFormat.Info.Heads} heads, {imageFormat.Info.SectorsPerTrack} sectors per track";
            }

            grpMediaInfo.Visible = lblMediaSequence.Visible     || lblMediaTitle.Visible ||
                                   lblMediaManufacturer.Visible ||
                                   lblMediaModel.Visible        || lblMediaSerialNumber.Visible ||
                                   lblMediaBarcode.Visible      ||
                                   lblMediaPartNumber.Visible;
            grpDriveInfo.Visible = lblDriveManufacturer.Visible || lblDriveModel.Visible            ||
                                   lblDriveSerialNumber.Visible || lblDriveFirmwareRevision.Visible ||
                                   lblMediaGeometry.Visible;

            if(imageFormat.Info.ReadableMediaTags != null && imageFormat.Info.ReadableMediaTags.Count > 0)
            {
                TreeGridItemCollection mediaTagList = new TreeGridItemCollection();

                treeMediaTags.Columns.Add(new GridColumn {HeaderText = "Tag", DataCell = new TextBoxCell(0)});

                treeMediaTags.AllowMultipleSelection = false;
                treeMediaTags.ShowHeader             = false;
                treeMediaTags.DataStore              = mediaTagList;

                foreach(MediaTagType tag in imageFormat.Info.ReadableMediaTags.OrderBy(t => t))
                    mediaTagList.Add(new TreeGridItem {Values = new object[] {tag.ToString()}});

                grpMediaTags.Visible = true;
            }

            if(imageFormat.Info.ReadableSectorTags != null && imageFormat.Info.ReadableSectorTags.Count > 0)
            {
                TreeGridItemCollection sectorTagList = new TreeGridItemCollection();

                treeSectorTags.Columns.Add(new GridColumn {HeaderText = "Tag", DataCell = new TextBoxCell(0)});

                treeSectorTags.AllowMultipleSelection = false;
                treeSectorTags.ShowHeader             = false;
                treeSectorTags.DataStore              = sectorTagList;

                foreach(SectorTagType tag in imageFormat.Info.ReadableSectorTags.OrderBy(t => t))
                    sectorTagList.Add(new TreeGridItem {Values = new object[] {tag.ToString()}});

                grpSectorTags.Visible = true;
            }

            PeripheralDeviceTypes scsiDeviceType  = PeripheralDeviceTypes.DirectAccess;
            byte[]                scsiInquiryData = null;
            Inquiry.SCSIInquiry?  scsiInquiry     = null;
            Modes.DecodedMode?    scsiMode        = null;
            byte[]                scsiModeSense6  = null;
            byte[]                scsiModeSense10 = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SCSI_INQUIRY))
            {
                scsiInquiryData = imageFormat.ReadDiskTag(MediaTagType.SCSI_INQUIRY);

                scsiDeviceType = (PeripheralDeviceTypes)(scsiInquiryData[0] & 0x1F);

                scsiInquiry = Inquiry.Decode(scsiInquiryData);
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SCSI_MODESENSE_6))
            {
                scsiModeSense6 = imageFormat.ReadDiskTag(MediaTagType.SCSI_MODESENSE_6);
                scsiMode       = Modes.DecodeMode6(scsiModeSense6, scsiDeviceType);
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SCSI_MODESENSE_10))
            {
                scsiModeSense10 = imageFormat.ReadDiskTag(MediaTagType.SCSI_MODESENSE_10);
                scsiMode        = Modes.DecodeMode10(scsiModeSense10, scsiDeviceType);
            }

            tabScsiInfo tabScsiInfo = new tabScsiInfo();
            tabScsiInfo.LoadData(scsiInquiryData, scsiInquiry, null, scsiMode, scsiDeviceType, scsiModeSense6,
                                 scsiModeSense10, null);
            tabInfos.Pages.Add(tabScsiInfo);

            byte[] ataIdentify   = null;
            byte[] atapiIdentify = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                ataIdentify = imageFormat.ReadDiskTag(MediaTagType.ATA_IDENTIFY);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.ATAPI_IDENTIFY))
                atapiIdentify = imageFormat.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY);

            tabAtaInfo tabAtaInfo = new tabAtaInfo();
            tabAtaInfo.LoadData(ataIdentify, atapiIdentify, null);
            tabInfos.Pages.Add(tabAtaInfo);

            byte[]                 toc                  = null;
            TOC.CDTOC?             decodedToc           = null;
            byte[]                 fullToc              = null;
            FullTOC.CDFullTOC?     decodedFullToc       = null;
            byte[]                 pma                  = null;
            byte[]                 atip                 = null;
            ATIP.CDATIP?           decodedAtip          = null;
            byte[]                 cdtext               = null;
            CDTextOnLeadIn.CDText? decodedCdText        = null;
            string                 mediaCatalogueNumber = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_TOC))
            {
                toc = imageFormat.ReadDiskTag(MediaTagType.CD_TOC);

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

                    decodedToc = TOC.Decode(toc);
                }
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_FullTOC))
            {
                fullToc = imageFormat.ReadDiskTag(MediaTagType.CD_FullTOC);

                if(fullToc.Length > 0)
                {
                    ushort dataLen = Swapping.Swap(BitConverter.ToUInt16(fullToc, 0));
                    if(dataLen + 2 != fullToc.Length)
                    {
                        byte[] tmp = new byte[fullToc.Length + 2];
                        Array.Copy(fullToc, 0, tmp, 2, fullToc.Length);
                        tmp[0]  = (byte)((fullToc.Length & 0xFF00) >> 8);
                        tmp[1]  = (byte)(fullToc.Length & 0xFF);
                        fullToc = tmp;
                    }

                    decodedFullToc = FullTOC.Decode(fullToc);
                }
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_PMA))
            {
                pma = imageFormat.ReadDiskTag(MediaTagType.CD_PMA);

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
                }
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_ATIP))
            {
                atip = imageFormat.ReadDiskTag(MediaTagType.CD_ATIP);

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

                decodedAtip = ATIP.Decode(atip);
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_TEXT))
            {
                cdtext = imageFormat.ReadDiskTag(MediaTagType.CD_TEXT);

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

                decodedCdText = CDTextOnLeadIn.Decode(cdtext);
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.CD_MCN))
            {
                byte[] mcn = imageFormat.ReadDiskTag(MediaTagType.CD_MCN);

                mediaCatalogueNumber = Encoding.UTF8.GetString(mcn);
            }

            tabCompactDiscInfo tabCompactDiscInfo = new tabCompactDiscInfo();
            tabCompactDiscInfo.LoadData(toc, atip, null, null, fullToc, pma, cdtext, decodedToc, decodedAtip, null,
                                        decodedFullToc, decodedCdText, null, mediaCatalogueNumber, null);
            tabInfos.Pages.Add(tabCompactDiscInfo);

            byte[]                         dvdPfi                    = null;
            byte[]                         dvdDmi                    = null;
            byte[]                         dvdCmi                    = null;
            byte[]                         hddvdCopyrightInformation = null;
            byte[]                         dvdBca                    = null;
            PFI.PhysicalFormatInformation? decodedPfi                = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVD_PFI))
            {
                dvdPfi     = imageFormat.ReadDiskTag(MediaTagType.DVD_PFI);
                decodedPfi = PFI.Decode(dvdPfi);
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVD_DMI))
                dvdDmi = imageFormat.ReadDiskTag(MediaTagType.DVD_DMI);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVD_CMI))
                dvdCmi = imageFormat.ReadDiskTag(MediaTagType.DVD_CMI);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.HDDVD_CPI))
                hddvdCopyrightInformation = imageFormat.ReadDiskTag(MediaTagType.HDDVD_CPI);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVD_BCA))
                dvdBca = imageFormat.ReadDiskTag(MediaTagType.DVD_BCA);

            tabDvdInfo tabDvdInfo = new tabDvdInfo();
            tabDvdInfo.LoadData(imageFormat.Info.MediaType, dvdPfi, dvdDmi, dvdCmi, hddvdCopyrightInformation, dvdBca,
                                null, decodedPfi);
            tabInfos.Pages.Add(tabDvdInfo);

            byte[] dvdRamDds                     = null;
            byte[] dvdRamCartridgeStatus         = null;
            byte[] dvdRamSpareArea               = null;
            byte[] lastBorderOutRmd              = null;
            byte[] dvdPreRecordedInfo            = null;
            byte[] dvdrMediaIdentifier           = null;
            byte[] dvdrPhysicalInformation       = null;
            byte[] hddvdrMediumStatus            = null;
            byte[] dvdrLayerCapacity             = null;
            byte[] dvdrDlMiddleZoneStart         = null;
            byte[] dvdrDlJumpIntervalSize        = null;
            byte[] dvdrDlManualLayerJumpStartLba = null;
            byte[] dvdPlusAdip                   = null;
            byte[] dvdPlusDcb                    = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDRAM_DDS))
                dvdRamDds = imageFormat.ReadDiskTag(MediaTagType.DVDRAM_DDS);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDRAM_MediumStatus))
                dvdRamCartridgeStatus = imageFormat.ReadDiskTag(MediaTagType.DVDRAM_MediumStatus);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDRAM_SpareArea))
                dvdRamSpareArea = imageFormat.ReadDiskTag(MediaTagType.DVDRAM_SpareArea);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDR_RMD))
                lastBorderOutRmd = imageFormat.ReadDiskTag(MediaTagType.DVDR_RMD);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDR_PreRecordedInfo))
                dvdPreRecordedInfo = imageFormat.ReadDiskTag(MediaTagType.DVDR_PreRecordedInfo);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDR_MediaIdentifier))
                dvdrMediaIdentifier = imageFormat.ReadDiskTag(MediaTagType.DVDR_MediaIdentifier);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDR_PFI))
                dvdrPhysicalInformation = imageFormat.ReadDiskTag(MediaTagType.DVDR_PFI);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.HDDVD_MediumStatus))
                hddvdrMediumStatus = imageFormat.ReadDiskTag(MediaTagType.HDDVD_MediumStatus);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDDL_LayerCapacity))
                dvdrLayerCapacity = imageFormat.ReadDiskTag(MediaTagType.DVDDL_LayerCapacity);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDDL_MiddleZoneAddress))
                dvdrDlMiddleZoneStart = imageFormat.ReadDiskTag(MediaTagType.DVDDL_MiddleZoneAddress);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDDL_JumpIntervalSize))
                dvdrDlJumpIntervalSize = imageFormat.ReadDiskTag(MediaTagType.DVDDL_JumpIntervalSize);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVDDL_ManualLayerJumpLBA))
                dvdrDlManualLayerJumpStartLba = imageFormat.ReadDiskTag(MediaTagType.DVDDL_ManualLayerJumpLBA);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DVD_ADIP))
                dvdPlusAdip = imageFormat.ReadDiskTag(MediaTagType.DVD_ADIP);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.DCB))
                dvdPlusDcb = imageFormat.ReadDiskTag(MediaTagType.DCB);

            tabDvdWritableInfo tabDvdWritableInfo = new tabDvdWritableInfo();
            tabDvdWritableInfo.LoadData(imageFormat.Info.MediaType, dvdRamDds, dvdRamCartridgeStatus, dvdRamSpareArea,
                                        lastBorderOutRmd, dvdPreRecordedInfo, dvdrMediaIdentifier,
                                        dvdrPhysicalInformation, hddvdrMediumStatus, null, dvdrLayerCapacity,
                                        dvdrDlMiddleZoneStart, dvdrDlJumpIntervalSize, dvdrDlManualLayerJumpStartLba,
                                        null, dvdPlusAdip, dvdPlusDcb);
            tabInfos.Pages.Add(tabDvdWritableInfo);

            byte[] blurayBurstCuttingArea     = null;
            byte[] blurayCartridgeStatus      = null;
            byte[] blurayDds                  = null;
            byte[] blurayDiscInformation      = null;
            byte[] blurayPowResources         = null;
            byte[] bluraySpareAreaInformation = null;
            byte[] blurayTrackResources       = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.BD_BCA))
                blurayBurstCuttingArea = imageFormat.ReadDiskTag(MediaTagType.BD_BCA);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.BD_CartridgeStatus))
                blurayCartridgeStatus = imageFormat.ReadDiskTag(MediaTagType.BD_CartridgeStatus);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.BD_DDS))
                blurayDds = imageFormat.ReadDiskTag(MediaTagType.BD_DDS);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.BD_DI))
                blurayDiscInformation = imageFormat.ReadDiskTag(MediaTagType.BD_DI);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_POWResourcesInformation))
                blurayPowResources = imageFormat.ReadDiskTag(MediaTagType.MMC_POWResourcesInformation);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.BD_SpareArea))
                bluraySpareAreaInformation = imageFormat.ReadDiskTag(MediaTagType.BD_SpareArea);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_TrackResourcesInformation))
                bluraySpareAreaInformation = imageFormat.ReadDiskTag(MediaTagType.MMC_TrackResourcesInformation);

            tabBlurayInfo tabBlurayInfo = new tabBlurayInfo();
            tabBlurayInfo.LoadData(blurayDiscInformation, blurayBurstCuttingArea, blurayDds, blurayCartridgeStatus,
                                   bluraySpareAreaInformation, blurayPowResources, blurayTrackResources, null, null);
            tabInfos.Pages.Add(tabBlurayInfo);

            byte[]             xboxDmi                   = null;
            byte[]             xboxSecuritySector        = null;
            SS.SecuritySector? decodedXboxSecuritySector = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.Xbox_DMI))
                xboxDmi = imageFormat.ReadDiskTag(MediaTagType.Xbox_DMI);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.Xbox_SecuritySector))
            {
                xboxSecuritySector        = imageFormat.ReadDiskTag(MediaTagType.Xbox_SecuritySector);
                decodedXboxSecuritySector = SS.Decode(xboxSecuritySector);
            }

            tabXboxInfo tabXboxInfo = new tabXboxInfo();
            tabXboxInfo.LoadData(null, xboxDmi, xboxSecuritySector, decodedXboxSecuritySector);
            tabInfos.Pages.Add(tabXboxInfo);

            byte[] pcmciaCis = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                pcmciaCis = imageFormat.ReadDiskTag(MediaTagType.PCMCIA_CIS);

            tabPcmciaInfo tabPcmciaInfo = new tabPcmciaInfo();
            tabPcmciaInfo.LoadData(pcmciaCis);
            tabInfos.Pages.Add(tabPcmciaInfo);

            DeviceType deviceType  = DeviceType.Unknown;
            byte[]     cid         = null;
            byte[]     csd         = null;
            byte[]     ocr         = null;
            byte[]     extendedCsd = null;
            byte[]     scr         = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SD_CID))
            {
                cid        = imageFormat.ReadDiskTag(MediaTagType.SD_CID);
                deviceType = DeviceType.SecureDigital;
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SD_CSD))
            {
                csd        = imageFormat.ReadDiskTag(MediaTagType.SD_CSD);
                deviceType = DeviceType.SecureDigital;
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SD_OCR))
            {
                ocr        = imageFormat.ReadDiskTag(MediaTagType.SD_OCR);
                deviceType = DeviceType.SecureDigital;
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.SD_SCR))
            {
                scr        = imageFormat.ReadDiskTag(MediaTagType.SD_SCR);
                deviceType = DeviceType.SecureDigital;
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_CID))
            {
                cid        = imageFormat.ReadDiskTag(MediaTagType.MMC_CID);
                deviceType = DeviceType.MMC;
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_CSD))
            {
                csd        = imageFormat.ReadDiskTag(MediaTagType.MMC_CSD);
                deviceType = DeviceType.MMC;
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_OCR))
            {
                ocr        = imageFormat.ReadDiskTag(MediaTagType.MMC_OCR);
                deviceType = DeviceType.MMC;
            }

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.MMC_ExtendedCSD))
            {
                extendedCsd = imageFormat.ReadDiskTag(MediaTagType.MMC_ExtendedCSD);
                deviceType  = DeviceType.MMC;
            }

            tabSdMmcInfo tabSdMmcInfo = new tabSdMmcInfo();
            tabSdMmcInfo.LoadData(deviceType, cid, csd, ocr, extendedCsd, scr);
            tabInfos.Pages.Add(tabSdMmcInfo);

            try
            {
                if(imageFormat.Sessions != null && imageFormat.Sessions.Count > 0)
                {
                    TreeGridItemCollection sessionList = new TreeGridItemCollection();

                    treeSessions.Columns.Add(new GridColumn {HeaderText = "Session", DataCell = new TextBoxCell(0)});
                    treeSessions.Columns.Add(new GridColumn
                    {
                        HeaderText = "First track", DataCell = new TextBoxCell(1)
                    });
                    treeSessions.Columns.Add(new GridColumn {HeaderText = "Last track", DataCell = new TextBoxCell(2)});
                    treeSessions.Columns.Add(new GridColumn {HeaderText = "Start", DataCell      = new TextBoxCell(3)});
                    treeSessions.Columns.Add(new GridColumn {HeaderText = "End", DataCell        = new TextBoxCell(4)});

                    treeSessions.AllowMultipleSelection = false;
                    treeSessions.ShowHeader             = true;
                    treeSessions.DataStore              = sessionList;

                    foreach(Session session in imageFormat.Sessions)
                        sessionList.Add(new TreeGridItem
                        {
                            Values = new object[]
                            {
                                session.SessionSequence, session.StartTrack, session.EndTrack,
                                session.StartSector, session.EndSector
                            }
                        });

                    tabSessions.Visible = true;
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                if(imageFormat.Tracks != null && imageFormat.Tracks.Count > 0)
                {
                    TreeGridItemCollection tracksList = new TreeGridItemCollection();

                    treeTracks.Columns.Add(new GridColumn {HeaderText = "Track", DataCell      = new TextBoxCell(0)});
                    treeTracks.Columns.Add(new GridColumn {HeaderText = "Type", DataCell       = new TextBoxCell(1)});
                    treeTracks.Columns.Add(new GridColumn {HeaderText = "Bps", DataCell        = new TextBoxCell(2)});
                    treeTracks.Columns.Add(new GridColumn {HeaderText = "Raw bps", DataCell    = new TextBoxCell(3)});
                    treeTracks.Columns.Add(new GridColumn {HeaderText = "Subchannel", DataCell = new TextBoxCell(4)});
                    treeTracks.Columns.Add(new GridColumn {HeaderText = "Pregap", DataCell     = new TextBoxCell(5)});
                    treeTracks.Columns.Add(new GridColumn {HeaderText = "Start", DataCell      = new TextBoxCell(6)});
                    treeTracks.Columns.Add(new GridColumn {HeaderText = "End", DataCell        = new TextBoxCell(7)});

                    treeTracks.AllowMultipleSelection = false;
                    treeTracks.ShowHeader             = true;
                    treeTracks.DataStore              = tracksList;

                    foreach(Track track in imageFormat.Tracks)
                        tracksList.Add(new TreeGridItem
                        {
                            Values = new object[]
                            {
                                track.TrackSequence, track.TrackType, track.TrackBytesPerSector,
                                track.TrackRawBytesPerSector, track.TrackSubchannelType,
                                track.TrackPregap, track.TrackStartSector, track.TrackEndSector
                            }
                        });

                    tabTracks.Visible = true;
                }
            }
            catch
            {
                // ignored
            }

            if(imageFormat.DumpHardware == null) return;

            TreeGridItemCollection dumpHardwareList = new TreeGridItemCollection();

            treeDumpHardware.Columns.Add(new GridColumn {HeaderText = "Manufacturer", DataCell = new TextBoxCell(0)});
            treeDumpHardware.Columns.Add(new GridColumn {HeaderText = "Model", DataCell        = new TextBoxCell(1)});
            treeDumpHardware.Columns.Add(new GridColumn {HeaderText = "Serial", DataCell       = new TextBoxCell(2)});
            treeDumpHardware.Columns.Add(new GridColumn {HeaderText = "Software", DataCell     = new TextBoxCell(3)});
            treeDumpHardware.Columns.Add(new GridColumn {HeaderText = "Version", DataCell      = new TextBoxCell(4)});
            treeDumpHardware.Columns.Add(new GridColumn
            {
                HeaderText = "Operating system", DataCell = new TextBoxCell(5)
            });
            treeDumpHardware.Columns.Add(new GridColumn {HeaderText = "Start", DataCell = new TextBoxCell(6)});
            treeDumpHardware.Columns.Add(new GridColumn {HeaderText = "End", DataCell   = new TextBoxCell(7)});

            treeDumpHardware.AllowMultipleSelection = false;
            treeDumpHardware.ShowHeader             = true;
            treeDumpHardware.DataStore              = dumpHardwareList;

            foreach(DumpHardwareType dump in imageFormat.DumpHardware)
            {
                foreach(ExtentType extent in dump.Extents)
                    dumpHardwareList.Add(new TreeGridItem
                    {
                        Values = new object[]
                        {
                            dump.Manufacturer, dump.Model, dump.Serial, dump.Software.Name,
                            dump.Software.Version, dump.Software.OperatingSystem,
                            extent.Start, extent.End
                        }
                    });
            }

            tabDumpHardware.Visible = true;
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        TabControl   tabInfos;
        Label        lblImagePath;
        Label        lblFilter;
        Label        lblImageFormat;
        Label        lblApplication;
        Label        lblImageSize;
        Label        lblSectors;
        Label        lblCreator;
        Label        lblCreationTime;
        Label        lblLastModificationTime;
        Label        lblMediaType;
        Label        lblHasPartitions;
        Label        lblHasSessions;
        Label        lblComments;
        TextArea     txtComments;
        Label        lblMediaSequence;
        Label        lblMediaTitle;
        Label        lblMediaManufacturer;
        Label        lblMediaModel;
        Label        lblMediaSerialNumber;
        Label        lblMediaBarcode;
        Label        lblMediaPartNumber;
        Label        lblDriveManufacturer;
        Label        lblDriveModel;
        Label        lblDriveSerialNumber;
        Label        lblDriveFirmwareRevision;
        Label        lblMediaGeometry;
        GroupBox     grpComments;
        GroupBox     grpMediaInfo;
        GroupBox     grpDriveInfo;
        GroupBox     grpMediaTags;
        TreeGridView treeMediaTags;
        GroupBox     grpSectorTags;
        TreeGridView treeSectorTags;
        TabPage      tabSessions;
        TreeGridView treeSessions;
        TabPage      tabTracks;
        TreeGridView treeTracks;
        TabPage      tabDumpHardware;
        TreeGridView treeDumpHardware;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}