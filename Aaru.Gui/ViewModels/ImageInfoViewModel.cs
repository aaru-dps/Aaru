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
using Aaru.Decoders.SCSI;
using Aaru.Gui.Models;
using Aaru.Gui.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using Schemas;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Gui.ViewModels
{
    public class ImageInfoViewModel : ViewModelBase
    {
        readonly IMediaImage _imageFormat;
        readonly Window      _view;
        IFilter              _filter;
        string               _imagePath;

        public ImageInfoViewModel(string imagePath, IFilter filter, IMediaImage imageFormat, Window view)

        {
            _imagePath   = imagePath;
            _filter      = filter;
            _imageFormat = imageFormat;
            _view        = view;
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

            ScsiInfo = new ScsiInfoTab
            {
                DataContext = new ScsiInfoViewModel(scsiInquiryData, scsiInquiry, null, scsiMode, scsiDeviceType,
                                                    scsiModeSense6, scsiModeSense10, null, _view)
            };

            byte[] ataIdentify   = null;
            byte[] atapiIdentify = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
                ataIdentify = imageFormat.ReadDiskTag(MediaTagType.ATA_IDENTIFY);

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.ATAPI_IDENTIFY))
                atapiIdentify = imageFormat.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY);

            AtaInfo = new AtaInfoTab
            {
                DataContext = new AtaInfoViewModel(ataIdentify, atapiIdentify, null, _view)
            };

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

            CompactDiscInfo = new CompactDiscInfoTab
            {
                DataContext = new CompactDiscInfoViewModel(toc, atip, null, null, fullToc, pma, cdtext, decodedToc,
                                                           decodedAtip, null, decodedFullToc, decodedCdText, null,
                                                           mediaCatalogueNumber, null, _view)
            };

            /* TODO: tabDvdInfo
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

            var tabDvdInfo = new tabDvdInfo();

            tabDvdInfo.LoadData(imageFormat.Info.MediaType, dvdPfi, dvdDmi, dvdCmi, hddvdCopyrightInformation, dvdBca,
                                null, decodedPfi);

            tabInfos.Pages.Add(tabDvdInfo);
*/
            /* TODO: tabDvdWritableinfo

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

            var tabDvdWritableInfo = new tabDvdWritableInfo();

            tabDvdWritableInfo.LoadData(imageFormat.Info.MediaType, dvdRamDds, dvdRamCartridgeStatus, dvdRamSpareArea,
                                        lastBorderOutRmd, dvdPreRecordedInfo, dvdrMediaIdentifier,
                                        dvdrPhysicalInformation, hddvdrMediumStatus, null, dvdrLayerCapacity,
                                        dvdrDlMiddleZoneStart, dvdrDlJumpIntervalSize, dvdrDlManualLayerJumpStartLba,
                                        null, dvdPlusAdip, dvdPlusDcb);

            tabInfos.Pages.Add(tabDvdWritableInfo);
*/
            /* TODO: tabBlurayInfo

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

            var tabBlurayInfo = new tabBlurayInfo();

            tabBlurayInfo.LoadData(blurayDiscInformation, blurayBurstCuttingArea, blurayDds, blurayCartridgeStatus,
                                   bluraySpareAreaInformation, blurayPowResources, blurayTrackResources, null, null);

            tabInfos.Pages.Add(tabBlurayInfo);
*/
            /* TODO: tabXboxInfo

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

            var tabXboxInfo = new tabXboxInfo();
            tabXboxInfo.LoadData(null, xboxDmi, xboxSecuritySector, decodedXboxSecuritySector);
            tabInfos.Pages.Add(tabXboxInfo);
*/
            /* TODO: tabPcmciaInfo

            byte[] pcmciaCis = null;

            if(imageFormat.Info.ReadableMediaTags != null &&
               imageFormat.Info.ReadableMediaTags.Contains(MediaTagType.PCMCIA_CIS))
                pcmciaCis = imageFormat.ReadDiskTag(MediaTagType.PCMCIA_CIS);

            var tabPcmciaInfo = new tabPcmciaInfo();
            tabPcmciaInfo.LoadData(pcmciaCis);
            tabInfos.Pages.Add(tabPcmciaInfo);
*/
            /* TODO: tabSdMmcInfo

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

            var tabSdMmcInfo = new tabSdMmcInfo();
            tabSdMmcInfo.LoadData(deviceType, cid, csd, ocr, extendedCsd, scr);
            tabInfos.Pages.Add(tabSdMmcInfo);
*/
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
                        Manufacturer    = dump.Manufacturer, Model             = dump.Model, Serial = dump.Serial,
                        SoftwareName    = dump.Software.Name, SoftwareVersion  = dump.Software.Version,
                        OperatingSystem = dump.Software.OperatingSystem, Start = extent.Start, End = extent.End
                    });
            }
        }

        public ScsiInfoTab                             ScsiInfo                  { get; }
        public AtaInfoTab                              AtaInfo                   { get; }
        public CompactDiscInfoTab                      CompactDiscInfo           { get; }
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

        protected void ExecuteEntropyCommand()
        {
            /* TODO: frmImageEntropy
            if(frmImageEntropy != null)
            {
                frmImageEntropy.Show();

                return;
            }

            frmImageEntropy = new frmImageEntropy(imageFormat);

            frmImageEntropy.Closed += (s, ea) =>
            {
                frmImageEntropy = null;
            };

            frmImageEntropy.Show();
            */
        }

        protected void ExecuteVerifyCommand()
        {
            /* TODO: frmImageVerify
            if(frmImageVerify != null)
            {
                frmImageVerify.Show();

                return;
            }

            frmImageVerify = new frmImageVerify(imageFormat);

            frmImageVerify.Closed += (s, ea) =>
            {
                frmImageVerify = null;
            };

            frmImageVerify.Show();
            */
        }

        protected void ExecuteChecksumCommand()
        {
            /* TODO: frmImageChecksum
            if(frmImageChecksum != null)
            {
                frmImageChecksum.Show();

                return;
            }

            frmImageChecksum = new frmImageChecksum(imageFormat);

            frmImageChecksum.Closed += (s, ea) =>
            {
                frmImageChecksum = null;
            };

            frmImageChecksum.Show();
            */
        }

        protected void ExecuteConvertCommand()
        {
            /* TODO: frmImageConvert
            if(frmImageConvert != null)
            {
                frmImageConvert.Show();

                return;
            }

            frmImageConvert = new frmImageConvert(imageFormat, imagePath);

            frmImageConvert.Closed += (s, ea) =>
            {
                frmImageConvert = null;
            };

            frmImageConvert.Show();
            */
        }

        protected void ExecuteCreateSidecarCommand()
        {
            /* TODO: frmImageSidecar
            if(frmImageSidecar != null)
            {
                frmImageSidecar.Show();

                return;
            }

            // TODO: Pass thru chosen default encoding
            frmImageSidecar = new frmImageSidecar(imageFormat, imagePath, filter.Id, null);

            frmImageSidecar.Closed += (s, ea) =>
            {
                frmImageSidecar = null;
            };

            frmImageSidecar.Show();
            */
        }

        protected void ExecuteViewSectorsCommand()
        {
            /* TODO: frmPrintHex
            if(frmPrintHex != null)
            {
                frmPrintHex.Show();

                return;
            }

            frmPrintHex = new frmPrintHex(imageFormat);

            frmPrintHex.Closed += (s, ea) =>
            {
                frmPrintHex = null;
            };

            frmPrintHex.Show();
            */
        }

        protected void ExecuteDecodeMediaTagCommand()
        {
            /* TODO: frmDecodeMediaTags
            if(frmDecodeMediaTags != null)
            {
                frmDecodeMediaTags.Show();

                return;
            }

            frmDecodeMediaTags = new frmDecodeMediaTags(imageFormat);

            frmDecodeMediaTags.Closed += (s, ea) =>
            {
                frmDecodeMediaTags = null;
            };

            frmDecodeMediaTags.Show();
            */
        }
    }
}