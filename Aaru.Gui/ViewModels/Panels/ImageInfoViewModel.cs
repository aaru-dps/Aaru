// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the opened image information panel.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.Xbox;
using Aaru.Gui.Models;
using Aaru.Gui.ViewModels.Tabs;
using Aaru.Gui.ViewModels.Windows;
using Aaru.Gui.Views.Tabs;
using Aaru.Gui.Views.Windows;
using Aaru.Helpers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using Schemas;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Gui.ViewModels.Panels;

public sealed class ImageInfoViewModel : ViewModelBase
{
    readonly IFilter     _filter;
    readonly IMediaImage _imageFormat;
    readonly string      _imagePath;
    DecodeMediaTags      _decodeMediaTags;
    ImageChecksum        _imageChecksum;
    ImageConvert         _imageConvert;
    ImageEntropy         _imageEntropy;
    ImageSidecar         _imageSidecar;
    ImageVerify          _imageVerify;
    ViewSector           _viewSector;

    public ImageInfoViewModel(string imagePath, IFilter filter, IMediaImage imageFormat, Window view)

    {
        _imagePath   = imagePath;
        _filter      = filter;
        _imageFormat = imageFormat;
        IAssetLoader assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        MediaTagsList         = new ObservableCollection<string>();
        SectorTagsList        = new ObservableCollection<string>();
        Sessions              = new ObservableCollection<Session>();
        Tracks                = new ObservableCollection<Track>();
        DumpHardwareList      = new ObservableCollection<DumpHardwareModel>();
        EntropyCommand        = ReactiveCommand.Create(ExecuteEntropyCommand);
        VerifyCommand         = ReactiveCommand.Create(ExecuteVerifyCommand);
        ChecksumCommand       = ReactiveCommand.Create(ExecuteChecksumCommand);
        ConvertCommand        = ReactiveCommand.Create(ExecuteConvertCommand);
        CreateSidecarCommand  = ReactiveCommand.Create(ExecuteCreateSidecarCommand);
        ViewSectorsCommand    = ReactiveCommand.Create(ExecuteViewSectorsCommand);
        DecodeMediaTagCommand = ReactiveCommand.Create(ExecuteDecodeMediaTagCommand);

        var genericHddIcon =
            new Bitmap(assets.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/32x32/drive-harddisk.png")));

        var genericOpticalIcon =
            new Bitmap(assets.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/32x32/drive-optical.png")));

        var genericFolderIcon =
            new Bitmap(assets.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/32x32/inode-directory.png")));

        var mediaResource = new Uri($"avares://Aaru.Gui/Assets/Logos/Media/{imageFormat.Info.MediaType}.png");

        MediaLogo = assets.Exists(mediaResource)
                        ? new Bitmap(assets.Open(mediaResource))
                        : imageFormat.Info.XmlMediaType == XmlMediaType.BlockMedia
                            ? genericHddIcon
                            : imageFormat.Info.XmlMediaType == XmlMediaType.OpticalDisc
                                ? genericOpticalIcon
                                : genericFolderIcon;

        ImagePathText       = $"Path: {imagePath}";
        FilterText          = $"Filter: {filter.Name}";
        ImageIdentifiedText = $"Image format identified by {imageFormat.Name} ({imageFormat.Id}).";

        ImageFormatText = !string.IsNullOrWhiteSpace(imageFormat.Info.Version)
                              ? $"Format: {imageFormat.Format} version {imageFormat.Info.Version}"
                              : $"Format: {imageFormat.Format}";

        ImageSizeText = $"Image without headers is {imageFormat.Info.ImageSize} bytes long";

        SectorsText =
            $"Contains a media of {imageFormat.Info.Sectors} sectors with a maximum sector size of {imageFormat.Info.SectorSize} bytes (if all sectors are of the same size this would be {imageFormat.Info.Sectors * imageFormat.Info.SectorSize} bytes)";

        MediaTypeText =
            $"Contains a media of type {imageFormat.Info.MediaType} and XML type {imageFormat.Info.XmlMediaType}";

        HasPartitionsText = $"{(imageFormat.Info.HasPartitions ? "Has" : "Doesn't have")} partitions";
        HasSessionsText   = $"{(imageFormat.Info.HasSessions ? "Has" : "Doesn't have")} sessions";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application))
            ApplicationText = !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion)
                                  ? $"Was created with {imageFormat.Info.Application} version {imageFormat.Info.ApplicationVersion}"
                                  : $"Was created with {imageFormat.Info.Application}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
            CreatorText = $"Created by: {imageFormat.Info.Creator}";

        if(imageFormat.Info.CreationTime != DateTime.MinValue)
            CreationTimeText = $"Created on {imageFormat.Info.CreationTime}";

        if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
            LastModificationTimeText = $"Last modified on {imageFormat.Info.LastModificationTime}";

        if(imageFormat.Info.MediaSequence     != 0 &&
           imageFormat.Info.LastMediaSequence != 0)
            MediaSequenceText =
                $"Media is number {imageFormat.Info.MediaSequence} on a set of {imageFormat.Info.LastMediaSequence} medias";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
            MediaTitleText = $"Media title: {imageFormat.Info.MediaTitle}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
            MediaManufacturerText = $"Media manufacturer: {imageFormat.Info.MediaManufacturer}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
            MediaModelText = $"Media model: {imageFormat.Info.MediaModel}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
            MediaSerialNumberText = $"Media serial number: {imageFormat.Info.MediaSerialNumber}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
            MediaBarcodeText = $"Media barcode: {imageFormat.Info.MediaBarcode}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
            MediaPartNumberText = $"Media part number: {imageFormat.Info.MediaPartNumber}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
            DriveManufacturerText = $"Drive manufacturer: {imageFormat.Info.DriveManufacturer}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
            DriveModelText = $"Drive model: {imageFormat.Info.DriveModel}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
            DriveSerialNumberText = $"Drive serial number: {imageFormat.Info.DriveSerialNumber}";

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
            DriveFirmwareRevisionText = $"Drive firmware info: {imageFormat.Info.DriveFirmwareRevision}";

        if(imageFormat.Info.Cylinders       > 0                         &&
           imageFormat.Info.Heads           > 0                         &&
           imageFormat.Info.SectorsPerTrack > 0                         &&
           imageFormat.Info.XmlMediaType    != XmlMediaType.OpticalDisc &&
           (!(imageFormat is ITapeImage tapeImage) || !tapeImage.IsTape))
            MediaGeometryText =
                $"Media geometry: {imageFormat.Info.Cylinders} cylinders, {imageFormat.Info.Heads} heads, {imageFormat.Info.SectorsPerTrack} sectors per track";

        if(imageFormat.Info.ReadableMediaTags       != null &&
           imageFormat.Info.ReadableMediaTags.Count > 0)
            foreach(MediaTagType tag in imageFormat.Info.ReadableMediaTags.OrderBy(t => t))
                MediaTagsList.Add(tag.ToString());

        if(imageFormat.Info.ReadableSectorTags       != null &&
           imageFormat.Info.ReadableSectorTags.Count > 0)
            foreach(SectorTagType tag in imageFormat.Info.ReadableSectorTags.OrderBy(t => t))
                SectorTagsList.Add(tag.ToString());

        PeripheralDeviceTypes scsiDeviceType  = PeripheralDeviceTypes.DirectAccess;
        byte[]                scsiInquiryData = null;
        Inquiry?              scsiInquiry     = null;
        Modes.DecodedMode?    scsiMode        = null;
        byte[]                scsiModeSense6  = null;
        byte[]                scsiModeSense10 = null;
        ErrorNumber           errno;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_INQUIRY) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SCSI_INQUIRY, out scsiInquiryData);

            if(errno == ErrorNumber.NoError)
            {
                scsiDeviceType = (PeripheralDeviceTypes)(scsiInquiryData[0] & 0x1F);

                scsiInquiry = Inquiry.Decode(scsiInquiryData);
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODESENSE_6) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SCSI_MODESENSE_6, out scsiModeSense6);

            if(errno == ErrorNumber.NoError)
                scsiMode = Modes.DecodeMode6(scsiModeSense6, scsiDeviceType);
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODESENSE_10) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SCSI_MODESENSE_10, out scsiModeSense10);

            if(errno == ErrorNumber.NoError)
                scsiMode = Modes.DecodeMode10(scsiModeSense10, scsiDeviceType);
        }

        ScsiInfo = new ScsiInfo
        {
            DataContext = new ScsiInfoViewModel(scsiInquiryData, scsiInquiry, null, scsiMode, scsiDeviceType,
                                                scsiModeSense6, scsiModeSense10, null, view)
        };

        byte[] ataIdentify   = null;
        byte[] atapiIdentify = null;
        errno = ErrorNumber.NoData;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATA_IDENTIFY) == true)
            errno = imageFormat.ReadMediaTag(MediaTagType.ATA_IDENTIFY, out ataIdentify);

        else if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATAPI_IDENTIFY) == true)
            errno = imageFormat.ReadMediaTag(MediaTagType.ATAPI_IDENTIFY, out atapiIdentify);

        if(errno == ErrorNumber.NoError)
            AtaInfo = new AtaInfo
            {
                DataContext = new AtaInfoViewModel(ataIdentify, atapiIdentify, null, view)
            };

        byte[]                 toc                  = null;
        TOC.CDTOC?             decodedToc           = null;
        byte[]                 fullToc              = null;
        FullTOC.CDFullTOC?     decodedFullToc       = null;
        byte[]                 pma                  = null;
        byte[]                 atip                 = null;
        ATIP.CDATIP            decodedAtip          = null;
        byte[]                 cdtext               = null;
        CDTextOnLeadIn.CDText? decodedCdText        = null;
        string                 mediaCatalogueNumber = null;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_TOC) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_TOC, out toc);

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

                decodedToc = TOC.Decode(toc);
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_FullTOC) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_FullTOC, out fullToc);

            if(errno          == ErrorNumber.NoError &&
               fullToc.Length > 0)
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

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_PMA) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_PMA, out pma);

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
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_ATIP) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_ATIP, out atip);

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

                decodedAtip = ATIP.Decode(atip);
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_TEXT) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_TEXT, out cdtext);

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

                decodedCdText = CDTextOnLeadIn.Decode(cdtext);
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_MCN) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_MCN, out byte[] mcn);

            if(errno == ErrorNumber.NoError)
                mediaCatalogueNumber = Encoding.UTF8.GetString(mcn);
        }

        CompactDiscInfo = new CompactDiscInfo
        {
            DataContext = new CompactDiscInfoViewModel(toc, atip, null, null, fullToc, pma, cdtext, decodedToc,
                                                       decodedAtip, null, decodedFullToc, decodedCdText, null,
                                                       mediaCatalogueNumber, null, view)
        };

        byte[]                         dvdPfi                    = null;
        byte[]                         dvdDmi                    = null;
        byte[]                         dvdCmi                    = null;
        byte[]                         hddvdCopyrightInformation = null;
        byte[]                         dvdBca                    = null;
        PFI.PhysicalFormatInformation? decodedPfi                = null;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_PFI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVD_PFI, out dvdPfi);

            if(errno == ErrorNumber.NoError)
                decodedPfi = PFI.Decode(dvdPfi, imageFormat.Info.MediaType);
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_DMI) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVD_DMI, out dvdDmi);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_CMI) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVD_CMI, out dvdCmi);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.HDDVD_CPI) == true)
            imageFormat.ReadMediaTag(MediaTagType.HDDVD_CPI, out hddvdCopyrightInformation);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_BCA) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVD_BCA, out dvdBca);

        DvdInfo = new DvdInfo
        {
            DataContext = new DvdInfoViewModel(imageFormat.Info.MediaType, dvdPfi, dvdDmi, dvdCmi,
                                               hddvdCopyrightInformation, dvdBca, null, decodedPfi, view)
        };

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

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDRAM_DDS) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDRAM_DDS, out dvdRamDds);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDRAM_MediumStatus) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDRAM_MediumStatus, out dvdRamCartridgeStatus);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDRAM_SpareArea) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDRAM_SpareArea, out dvdRamSpareArea);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_RMD) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDR_RMD, out lastBorderOutRmd);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PreRecordedInfo) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDR_PreRecordedInfo, out dvdPreRecordedInfo);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_MediaIdentifier) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDR_MediaIdentifier, out dvdrMediaIdentifier);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PFI) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDR_PFI, out dvdrPhysicalInformation);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.HDDVD_MediumStatus) == true)
            imageFormat.ReadMediaTag(MediaTagType.HDDVD_MediumStatus, out hddvdrMediumStatus);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDDL_LayerCapacity) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDDL_LayerCapacity, out dvdrLayerCapacity);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDDL_MiddleZoneAddress) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDDL_MiddleZoneAddress, out dvdrDlMiddleZoneStart);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDDL_JumpIntervalSize) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDDL_JumpIntervalSize, out dvdrDlJumpIntervalSize);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDDL_ManualLayerJumpLBA) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVDDL_ManualLayerJumpLBA, out dvdrDlManualLayerJumpStartLba);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_ADIP) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVD_ADIP, out dvdPlusAdip);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DCB) == true)
            imageFormat.ReadMediaTag(MediaTagType.DCB, out dvdPlusDcb);

        DvdWritableInfo = new DvdWritableInfo
        {
            DataContext = new DvdWritableInfoViewModel(imageFormat.Info.MediaType, dvdRamDds, dvdRamCartridgeStatus,
                                                       dvdRamSpareArea, lastBorderOutRmd, dvdPreRecordedInfo,
                                                       dvdrMediaIdentifier, dvdrPhysicalInformation,
                                                       hddvdrMediumStatus, null, dvdrLayerCapacity,
                                                       dvdrDlMiddleZoneStart, dvdrDlJumpIntervalSize,
                                                       dvdrDlManualLayerJumpStartLba, null, dvdPlusAdip, dvdPlusDcb,
                                                       view)
        };

        byte[] blurayBurstCuttingArea     = null;
        byte[] blurayCartridgeStatus      = null;
        byte[] blurayDds                  = null;
        byte[] blurayDiscInformation      = null;
        byte[] blurayPowResources         = null;
        byte[] bluraySpareAreaInformation = null;
        byte[] blurayTrackResources       = null;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_BCA) == true)
            imageFormat.ReadMediaTag(MediaTagType.BD_BCA, out blurayBurstCuttingArea);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_CartridgeStatus) == true)
            imageFormat.ReadMediaTag(MediaTagType.BD_CartridgeStatus, out blurayCartridgeStatus);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DDS) == true)
            imageFormat.ReadMediaTag(MediaTagType.BD_DDS, out blurayDds);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DI) == true)
            imageFormat.ReadMediaTag(MediaTagType.BD_DI, out blurayDiscInformation);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_POWResourcesInformation) == true)
            imageFormat.ReadMediaTag(MediaTagType.MMC_POWResourcesInformation, out blurayPowResources);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_SpareArea) == true)
            imageFormat.ReadMediaTag(MediaTagType.BD_SpareArea, out bluraySpareAreaInformation);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_TrackResourcesInformation) == true)
            imageFormat.ReadMediaTag(MediaTagType.MMC_TrackResourcesInformation, out blurayTrackResources);

        BlurayInfo = new BlurayInfo
        {
            DataContext = new BlurayInfoViewModel(blurayDiscInformation, blurayBurstCuttingArea, blurayDds,
                                                  blurayCartridgeStatus, bluraySpareAreaInformation,
                                                  blurayPowResources, blurayTrackResources, null, null, view)
        };

        byte[]             xboxDmi                   = null;
        byte[]             xboxSecuritySector        = null;
        SS.SecuritySector? decodedXboxSecuritySector = null;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_DMI) == true)
            imageFormat.ReadMediaTag(MediaTagType.Xbox_DMI, out xboxDmi);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_SecuritySector) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.Xbox_SecuritySector, out xboxSecuritySector);

            if(errno == ErrorNumber.NoError)
                decodedXboxSecuritySector = SS.Decode(xboxSecuritySector);
        }

        XboxInfo = new XboxInfo
        {
            DataContext = new XboxInfoViewModel(null, xboxDmi, xboxSecuritySector, decodedXboxSecuritySector, view)
        };

        errno = ErrorNumber.NoData;
        byte[] pcmciaCis = null;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.PCMCIA_CIS) == true)
            errno = imageFormat.ReadMediaTag(MediaTagType.PCMCIA_CIS, out pcmciaCis);

        if(errno == ErrorNumber.NoError)
            PcmciaInfo = new PcmciaInfo
            {
                DataContext = new PcmciaInfoViewModel(pcmciaCis, view)
            };

        DeviceType deviceType  = DeviceType.Unknown;
        byte[]     cid         = null;
        byte[]     csd         = null;
        byte[]     ocr         = null;
        byte[]     extendedCsd = null;
        byte[]     scr         = null;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CID) == true)
        {
            imageFormat.ReadMediaTag(MediaTagType.SD_CID, out cid);
            deviceType = DeviceType.SecureDigital;
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CSD) == true)
        {
            imageFormat.ReadMediaTag(MediaTagType.SD_CSD, out csd);
            deviceType = DeviceType.SecureDigital;
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_OCR) == true)
        {
            imageFormat.ReadMediaTag(MediaTagType.SD_OCR, out ocr);
            deviceType = DeviceType.SecureDigital;
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_SCR) == true)
        {
            imageFormat.ReadMediaTag(MediaTagType.SD_SCR, out scr);
            deviceType = DeviceType.SecureDigital;
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CID) == true)
        {
            imageFormat.ReadMediaTag(MediaTagType.MMC_CID, out cid);
            deviceType = DeviceType.MMC;
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CSD) == true)
        {
            imageFormat.ReadMediaTag(MediaTagType.MMC_CSD, out csd);
            deviceType = DeviceType.MMC;
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_OCR) == true)
        {
            imageFormat.ReadMediaTag(MediaTagType.MMC_OCR, out ocr);
            deviceType = DeviceType.MMC;
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_ExtendedCSD) == true)
        {
            imageFormat.ReadMediaTag(MediaTagType.MMC_ExtendedCSD, out extendedCsd);
            deviceType = DeviceType.MMC;
        }

        SdMmcInfo = new SdMmcInfo
        {
            DataContext = new SdMmcInfoViewModel(deviceType, cid, csd, ocr, extendedCsd, scr)
        };

        if(imageFormat is IOpticalMediaImage opticalMediaImage)
        {
            try
            {
                if(opticalMediaImage.Sessions       != null &&
                   opticalMediaImage.Sessions.Count > 0)
                    foreach(Session session in opticalMediaImage.Sessions)
                        Sessions.Add(session);
            }
            catch
            {
                // ignored
            }

            try
            {
                if(opticalMediaImage.Tracks       != null &&
                   opticalMediaImage.Tracks.Count > 0)
                    foreach(Track track in opticalMediaImage.Tracks)
                        Tracks.Add(track);
            }
            catch
            {
                // ignored
            }
        }

        if(imageFormat.DumpHardware is null)
            return;

        foreach(DumpHardwareType dump in imageFormat.DumpHardware)
        {
            foreach(ExtentType extent in dump.Extents)
                DumpHardwareList.Add(new DumpHardwareModel
                {
                    Manufacturer    = dump.Manufacturer,
                    Model           = dump.Model,
                    Serial          = dump.Serial,
                    SoftwareName    = dump.Software.Name,
                    SoftwareVersion = dump.Software.Version,
                    OperatingSystem = dump.Software.OperatingSystem,
                    Start           = extent.Start,
                    End             = extent.End
                });
        }
    }

    public ScsiInfo                                ScsiInfo                  { get; }
    public AtaInfo                                 AtaInfo                   { get; }
    public CompactDiscInfo                         CompactDiscInfo           { get; }
    public DvdInfo                                 DvdInfo                   { get; }
    public DvdWritableInfo                         DvdWritableInfo           { get; }
    public BlurayInfo                              BlurayInfo                { get; }
    public XboxInfo                                XboxInfo                  { get; }
    public PcmciaInfo                              PcmciaInfo                { get; }
    public SdMmcInfo                               SdMmcInfo                 { get; }
    public Bitmap                                  MediaLogo                 { get; }
    public string                                  ImagePathText             { get; }
    public string                                  FilterText                { get; }
    public string                                  ImageIdentifiedText       { get; }
    public string                                  MediaTypeText             { get; set; }
    public string                                  SectorsText               { get; set; }
    public string                                  HasPartitionsText         { get; set; }
    public string                                  HasSessionsText           { get; set; }
    public string                                  ApplicationText           { get; set; }
    public string                                  CreatorText               { get; set; }
    public string                                  CreationTimeText          { get; set; }
    public string                                  LastModificationTimeText  { get; set; }
    public string                                  MediaSequenceText         { get; set; }
    public string                                  MediaTitleText            { get; set; }
    public string                                  MediaManufacturerText     { get; set; }
    public string                                  MediaModelText            { get; set; }
    public string                                  MediaSerialNumberText     { get; set; }
    public string                                  MediaBarcodeText          { get; set; }
    public string                                  MediaPartNumberText       { get; set; }
    public string                                  CommentsText              => _imageFormat.Info.Comments;
    public string                                  DriveManufacturerText     { get; set; }
    public string                                  DriveModelText            { get; set; }
    public string                                  DriveSerialNumberText     { get; set; }
    public string                                  DriveFirmwareRevisionText { get; set; }
    public string                                  MediaGeometryText         { get; set; }
    public ObservableCollection<string>            MediaTagsList             { get; }
    public ObservableCollection<string>            SectorTagsList            { get; }
    public string                                  ImageSizeText             { get; set; }
    public string                                  ImageFormatText           { get; set; }
    public ObservableCollection<Session>           Sessions                  { get; }
    public ObservableCollection<Track>             Tracks                    { get; }
    public ObservableCollection<DumpHardwareModel> DumpHardwareList          { get; }
    public ReactiveCommand<Unit, Unit>             EntropyCommand            { get; }
    public ReactiveCommand<Unit, Unit>             VerifyCommand             { get; }
    public ReactiveCommand<Unit, Unit>             ChecksumCommand           { get; }
    public ReactiveCommand<Unit, Unit>             ConvertCommand            { get; }
    public ReactiveCommand<Unit, Unit>             CreateSidecarCommand      { get; }
    public ReactiveCommand<Unit, Unit>             ViewSectorsCommand        { get; }
    public ReactiveCommand<Unit, Unit>             DecodeMediaTagCommand     { get; }
    public bool DriveInformationVisible => DriveManufacturerText != null || DriveModelText            != null ||
                                           DriveSerialNumberText != null || DriveFirmwareRevisionText != null ||
                                           MediaGeometryText     != null;
    public bool MediaInformationVisible => MediaSequenceText     != null || MediaTitleText   != null ||
                                           MediaManufacturerText != null || MediaModelText   != null ||
                                           MediaSerialNumberText != null || MediaBarcodeText != null ||
                                           MediaPartNumberText   != null;

    void ExecuteEntropyCommand()
    {
        if(_imageEntropy != null)
        {
            _imageEntropy.Show();

            return;
        }

        _imageEntropy             = new ImageEntropy();
        _imageEntropy.DataContext = new ImageEntropyViewModel(_imageFormat, _imageEntropy);

        _imageEntropy.Closed += (sender, args) => _imageEntropy = null;

        _imageEntropy.Show();
    }

    void ExecuteVerifyCommand()
    {
        if(_imageVerify != null)
        {
            _imageVerify.Show();

            return;
        }

        _imageVerify             = new ImageVerify();
        _imageVerify.DataContext = new ImageVerifyViewModel(_imageFormat, _imageVerify);

        _imageVerify.Closed += (sender, args) => _imageVerify = null;

        _imageVerify.Show();
    }

    void ExecuteChecksumCommand()
    {
        if(_imageChecksum != null)
        {
            _imageChecksum.Show();

            return;
        }

        _imageChecksum             = new ImageChecksum();
        _imageChecksum.DataContext = new ImageChecksumViewModel(_imageFormat, _imageChecksum);

        _imageChecksum.Closed += (sender, args) => _imageChecksum = null;

        _imageChecksum.Show();
    }

    void ExecuteConvertCommand()
    {
        if(_imageConvert != null)
        {
            _imageConvert.Show();

            return;
        }

        _imageConvert             = new ImageConvert();
        _imageConvert.DataContext = new ImageConvertViewModel(_imageFormat, _imagePath, _imageConvert);

        _imageConvert.Closed += (sender, args) => _imageConvert = null;

        _imageConvert.Show();
    }

    void ExecuteCreateSidecarCommand()
    {
        if(_imageSidecar != null)
        {
            _imageSidecar.Show();

            return;
        }

        _imageSidecar = new ImageSidecar();

        // TODO: Pass thru chosen default encoding
        _imageSidecar.DataContext =
            new ImageSidecarViewModel(_imageFormat, _imagePath, _filter.Id, null, _imageSidecar);

        _imageSidecar.Closed += (sender, args) => _imageSidecar = null;

        _imageSidecar.Show();
    }

    void ExecuteViewSectorsCommand()
    {
        if(_viewSector != null)
        {
            _viewSector.Show();

            return;
        }

        _viewSector = new ViewSector
        {
            DataContext = new ViewSectorViewModel(_imageFormat)
        };

        _viewSector.Closed += (sender, args) => _viewSector = null;

        _viewSector.Show();
    }

    void ExecuteDecodeMediaTagCommand()
    {
        if(_decodeMediaTags != null)
        {
            _decodeMediaTags.Show();

            return;
        }

        _decodeMediaTags = new DecodeMediaTags
        {
            DataContext = new DecodeMediaTagsViewModel(_imageFormat)
        };

        _decodeMediaTags.Closed += (sender, args) => _decodeMediaTags = null;

        _decodeMediaTags.Show();
    }
}