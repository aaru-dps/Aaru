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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Aaru.CommonTypes.AaruMetadata;
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
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Svg;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Sentry;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;
using Session = Aaru.CommonTypes.Structs.Session;
using Track = Aaru.CommonTypes.Structs.Track;

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
        _imagePath            = imagePath;
        _filter               = filter;
        _imageFormat          = imageFormat;
        MediaTagsList         = [];
        SectorTagsList        = [];
        Sessions              = [];
        Tracks                = [];
        DumpHardwareList      = [];
        EntropyCommand        = new RelayCommand(Entropy);
        VerifyCommand         = new RelayCommand(Verify);
        ChecksumCommand       = new RelayCommand(Checksum);
        ConvertCommand        = new RelayCommand(Convert);
        CreateSidecarCommand  = new RelayCommand(CreateSidecar);
        ViewSectorsCommand    = new RelayCommand(ViewSectors);
        DecodeMediaTagCommand = new RelayCommand(DecodeMediaTag);

        var genericHddIcon = new SvgImage
        {
            Source =
                SvgSource.Load(AssetLoader.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/drive-harddisk.svg")))
        };

        var genericOpticalIcon = new SvgImage
        {
            Source =
                SvgSource.Load(AssetLoader.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/drive-optical.svg")))
        };

        var genericFolderIcon = new SvgImage
        {
            Source =
                SvgSource.Load(AssetLoader.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/inode-directory.svg")))
        };

        // Check if we're using dark theme
        bool isDarkTheme = view.ActualThemeVariant == ThemeVariant.Dark;

        // Build the appropriate SVG path based on theme
        string svgPath = isDarkTheme
                             ? $"avares://Aaru.Gui/Assets/Logos/Media/Dark/{imageFormat.Info.MediaType}.svg"
                             : $"avares://Aaru.Gui/Assets/Logos/Media/{imageFormat.Info.MediaType}.svg";

        var mediaResource = new Uri(svgPath);

        // Fallback to light theme version if dark version doesn't exist
        if(isDarkTheme && !AssetLoader.Exists(mediaResource))
            mediaResource = new Uri($"avares://Aaru.Gui/Assets/Logos/Media/{imageFormat.Info.MediaType}.svg");

        MediaLogo = AssetLoader.Exists(mediaResource)
                        ? new SvgImage
                        {
                            Source = SvgSource.Load(AssetLoader.Open(mediaResource))
                        }
                        : imageFormat.Info.MetadataMediaType == MetadataMediaType.BlockMedia
                            ? genericHddIcon
                            : imageFormat.Info.MetadataMediaType == MetadataMediaType.OpticalDisc
                                ? genericOpticalIcon
                                : genericFolderIcon;

        ImagePathText       = string.Format(UI.Path_0,                         imagePath);
        FilterText          = string.Format(UI.Filter_0,                       filter.Name);
        ImageIdentifiedText = string.Format(UI.Image_format_identified_by_0_1, imageFormat.Name, imageFormat.Id);

        ImageFormatText = !string.IsNullOrWhiteSpace(imageFormat.Info.Version)
                              ? string.Format(Aaru.Localization.Core.Format_0_version_1_WithMarkup,
                                              imageFormat.Format,
                                              imageFormat.Info.Version)
                              : string.Format(Aaru.Localization.Core.Format_0_WithMarkup, imageFormat.Format);

        ImageSizeText = string.Format(Aaru.Localization.Core.Image_without_headers_is_0_bytes_long,
                                      imageFormat.Info.ImageSize);

        SectorsText =
            string.Format(Aaru.Localization.Core
                              .Contains_a_media_of_0_sectors_with_a_maximum_sector_size_of_1_bytes_etc,
                          imageFormat.Info.Sectors,
                          imageFormat.Info.SectorSize,
                          ByteSize.FromBytes(imageFormat.Info.Sectors * imageFormat.Info.SectorSize).Humanize());

        MediaTypeText = string.Format(Aaru.Localization.Core.Contains_a_media_of_type_0_and_XML_type_1_WithMarkup,
                                      imageFormat.Info.MediaType.Humanize(),
                                      imageFormat.Info.MetadataMediaType);

        HasPartitionsText = imageFormat.Info.HasPartitions
                                ? Aaru.Localization.Core.Has_partitions
                                : Aaru.Localization.Core.Doesnt_have_partitions;

        HasSessionsText = imageFormat.Info.HasSessions
                              ? Aaru.Localization.Core.Has_sessions
                              : Aaru.Localization.Core.Doesnt_have_sessions;

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Application))
        {
            ApplicationText = !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion)
                                  ? string.Format(Aaru.Localization.Core.Was_created_with_0_version_1_WithMarkup,
                                                  imageFormat.Info.Application,
                                                  imageFormat.Info.ApplicationVersion)
                                  : string.Format(Aaru.Localization.Core.Was_created_with_0_WithMarkup,
                                                  imageFormat.Info.Application);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
            CreatorText = string.Format(Aaru.Localization.Core.Created_by_0_WithMarkup, imageFormat.Info.Creator);

        if(imageFormat.Info.CreationTime != DateTime.MinValue)
            CreationTimeText = string.Format(Aaru.Localization.Core.Created_on_0, imageFormat.Info.CreationTime);

        if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
        {
            LastModificationTimeText =
                string.Format(Aaru.Localization.Core.Last_modified_on_0, imageFormat.Info.LastModificationTime);
        }

        if(imageFormat.Info.MediaSequence != 0 && imageFormat.Info.LastMediaSequence != 0)
        {
            MediaSequenceText = string.Format(Aaru.Localization.Core.Media_is_number_0_on_a_set_of_1_medias,
                                              imageFormat.Info.MediaSequence,
                                              imageFormat.Info.LastMediaSequence);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
        {
            MediaTitleText =
                string.Format(Aaru.Localization.Core.Media_title_0_WithMarkup, imageFormat.Info.MediaTitle);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
        {
            MediaManufacturerText = string.Format(Aaru.Localization.Core.Media_manufacturer_0_WithMarkup,
                                                  imageFormat.Info.MediaManufacturer);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
        {
            MediaModelText =
                string.Format(Aaru.Localization.Core.Media_model_0_WithMarkup, imageFormat.Info.MediaModel);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
        {
            MediaSerialNumberText = string.Format(Aaru.Localization.Core.Media_serial_number_0_WithMarkup,
                                                  imageFormat.Info.MediaSerialNumber);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
        {
            MediaBarcodeText = string.Format(Aaru.Localization.Core.Media_barcode_0_WithMarkup,
                                             imageFormat.Info.MediaBarcode);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
        {
            MediaPartNumberText = string.Format(Aaru.Localization.Core.Media_part_number_0_WithMarkup,
                                                imageFormat.Info.MediaPartNumber);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
        {
            DriveManufacturerText = string.Format(Aaru.Localization.Core.Drive_manufacturer_0_WithMarkup,
                                                  imageFormat.Info.DriveManufacturer);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
        {
            DriveModelText =
                string.Format(Aaru.Localization.Core.Drive_model_0_WithMarkup, imageFormat.Info.DriveModel);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
        {
            DriveSerialNumberText = string.Format(Aaru.Localization.Core.Drive_serial_number_0_WithMarkup,
                                                  imageFormat.Info.DriveSerialNumber);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
        {
            DriveFirmwareRevisionText = string.Format(Aaru.Localization.Core.Drive_firmware_info_0_WithMarkup,
                                                      imageFormat.Info.DriveFirmwareRevision);
        }

        if(imageFormat.Info.Cylinders > 0                                      &&
           imageFormat.Info is { Heads: > 0, SectorsPerTrack: > 0 }            &&
           imageFormat.Info.MetadataMediaType != MetadataMediaType.OpticalDisc &&
           imageFormat is not ITapeImage { IsTape: true })
        {
            MediaGeometryText = string.Format(UI.Media_geometry_0_cylinders_1_heads_2_sectors_per_track,
                                              imageFormat.Info.Cylinders,
                                              imageFormat.Info.Heads,
                                              imageFormat.Info.SectorsPerTrack);
        }

        if(imageFormat.Info.ReadableMediaTags is { Count: > 0 })
            foreach(MediaTagType tag in imageFormat.Info.ReadableMediaTags.Order())
                MediaTagsList.Add(tag.Humanize());

        if(imageFormat.Info.ReadableSectorTags is { Count: > 0 })
        {
            foreach(SectorTagType tag in imageFormat.Info.ReadableSectorTags.Order())
                SectorTagsList.Add(tag.Humanize());
        }

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

            if(errno == ErrorNumber.NoError) scsiMode = Modes.DecodeMode6(scsiModeSense6, scsiDeviceType);
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODESENSE_10) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SCSI_MODESENSE_10, out scsiModeSense10);

            if(errno == ErrorNumber.NoError) scsiMode = Modes.DecodeMode10(scsiModeSense10, scsiDeviceType);
        }

        if(scsiInquiryData != null ||
           scsiInquiry     != null ||
           scsiMode        != null ||
           scsiModeSense6  != null ||
           scsiModeSense10 != null)
        {
            ScsiInfo = new ScsiInfo
            {
                DataContext = new ScsiInfoViewModel(scsiInquiryData,
                                                    scsiInquiry,
                                                    null,
                                                    scsiMode,
                                                    scsiDeviceType,
                                                    scsiModeSense6,
                                                    scsiModeSense10,
                                                    null,
                                                    view)
            };
        }

        byte[] ataIdentify   = null;
        byte[] atapiIdentify = null;
        errno = ErrorNumber.NoData;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATA_IDENTIFY) == true)
            errno = imageFormat.ReadMediaTag(MediaTagType.ATA_IDENTIFY, out ataIdentify);

        else if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATAPI_IDENTIFY) == true)
            errno = imageFormat.ReadMediaTag(MediaTagType.ATAPI_IDENTIFY, out atapiIdentify);

        if(errno == ErrorNumber.NoError)
        {
            AtaInfo = new AtaInfo
            {
                DataContext = new AtaInfoViewModel(ataIdentify, atapiIdentify, null, view)
            };
        }

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

            if(errno == ErrorNumber.NoError && toc.Length > 0)
            {
                ushort dataLen = Swapping.Swap(BitConverter.ToUInt16(toc, 0));

                if(dataLen + 2 != toc.Length)
                {
                    var tmp = new byte[toc.Length + 2];
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

            if(errno == ErrorNumber.NoError && fullToc.Length > 0)
            {
                ushort dataLen = Swapping.Swap(BitConverter.ToUInt16(fullToc, 0));

                if(dataLen + 2 != fullToc.Length)
                {
                    var tmp = new byte[fullToc.Length + 2];
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

            if(errno == ErrorNumber.NoError && pma.Length > 0)
            {
                ushort dataLen = Swapping.Swap(BitConverter.ToUInt16(pma, 0));

                if(dataLen + 2 != pma.Length)
                {
                    var tmp = new byte[pma.Length + 2];
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
                    var tmp = new byte[atip.Length + 4];
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
                ushort dataLen = Swapping.Swap(BitConverter.ToUInt16(cdtext, 0));

                if(dataLen + 2 != cdtext.Length)
                {
                    var tmp = new byte[cdtext.Length + 2];
                    Array.Copy(cdtext, 0, tmp, 2, cdtext.Length);
                    tmp[0] = (byte)((cdtext.Length & 0xFF00) >> 8);
                    tmp[1] = (byte)(cdtext.Length & 0xFF);
                    cdtext = tmp;
                }

                decodedCdText = CDTextOnLeadIn.Decode(cdtext);
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_MCN) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_MCN, out byte[] mcn);

            if(errno == ErrorNumber.NoError) mediaCatalogueNumber = Encoding.UTF8.GetString(mcn);
        }

        if(toc != null || atip != null || fullToc != null || pma != null)
        {
            CompactDiscInfo = new CompactDiscInfo
            {
                DataContext = new CompactDiscInfoViewModel(toc,
                                                           atip,
                                                           null,
                                                           null,
                                                           fullToc,
                                                           pma,
                                                           cdtext,
                                                           decodedToc,
                                                           decodedAtip,
                                                           null,
                                                           decodedFullToc,
                                                           decodedCdText,
                                                           null,
                                                           mediaCatalogueNumber,
                                                           null,
                                                           view)
            };
        }

        byte[]                         dvdPfi                    = null;
        byte[]                         dvdDmi                    = null;
        byte[]                         dvdCmi                    = null;
        byte[]                         hddvdCopyrightInformation = null;
        byte[]                         dvdBca                    = null;
        PFI.PhysicalFormatInformation? decodedPfi                = null;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_PFI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVD_PFI, out dvdPfi);

            if(errno == ErrorNumber.NoError) decodedPfi = PFI.Decode(dvdPfi, imageFormat.Info.MediaType);
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_DMI) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVD_DMI, out dvdDmi);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_CMI) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVD_CMI, out dvdCmi);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.HDDVD_CPI) == true)
            imageFormat.ReadMediaTag(MediaTagType.HDDVD_CPI, out hddvdCopyrightInformation);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_BCA) == true)
            imageFormat.ReadMediaTag(MediaTagType.DVD_BCA, out dvdBca);

        if(dvdPfi != null || dvdDmi != null || dvdCmi != null || hddvdCopyrightInformation != null || dvdBca != null)
        {
            DvdInfo = new DvdInfo
            {
                DataContext = new DvdInfoViewModel(dvdPfi,
                                                   dvdDmi,
                                                   dvdCmi,
                                                   hddvdCopyrightInformation,
                                                   dvdBca,
                                                   null,
                                                   decodedPfi,
                                                   view)
            };
        }

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

        if(dvdRamDds                     != null ||
           dvdRamCartridgeStatus         != null ||
           dvdRamSpareArea               != null ||
           lastBorderOutRmd              != null ||
           dvdPreRecordedInfo            != null ||
           dvdrMediaIdentifier           != null ||
           dvdrPhysicalInformation       != null ||
           hddvdrMediumStatus            != null ||
           dvdrLayerCapacity             != null ||
           dvdrDlMiddleZoneStart         != null ||
           dvdrDlJumpIntervalSize        != null ||
           dvdrDlManualLayerJumpStartLba != null ||
           dvdPlusAdip                   != null ||
           dvdPlusDcb                    != null)
        {
            DvdWritableInfo = new DvdWritableInfo
            {
                DataContext = new DvdWritableInfoViewModel(dvdRamDds,
                                                           dvdRamCartridgeStatus,
                                                           dvdRamSpareArea,
                                                           lastBorderOutRmd,
                                                           dvdPreRecordedInfo,
                                                           dvdrMediaIdentifier,
                                                           dvdrPhysicalInformation,
                                                           hddvdrMediumStatus,
                                                           null,
                                                           dvdrLayerCapacity,
                                                           dvdrDlMiddleZoneStart,
                                                           dvdrDlJumpIntervalSize,
                                                           dvdrDlManualLayerJumpStartLba,
                                                           null,
                                                           dvdPlusAdip,
                                                           dvdPlusDcb,
                                                           view)
            };
        }

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

        if(blurayDiscInformation      != null ||
           blurayBurstCuttingArea     != null ||
           blurayDds                  != null ||
           blurayCartridgeStatus      != null ||
           bluraySpareAreaInformation != null ||
           blurayPowResources         != null ||
           blurayTrackResources       != null)
        {
            BlurayInfo = new BlurayInfo
            {
                DataContext = new BlurayInfoViewModel(blurayDiscInformation,
                                                      blurayBurstCuttingArea,
                                                      blurayDds,
                                                      blurayCartridgeStatus,
                                                      bluraySpareAreaInformation,
                                                      blurayPowResources,
                                                      blurayTrackResources,
                                                      null,
                                                      null,
                                                      view)
            };
        }

        byte[]             xboxDmi                   = null;
        byte[]             xboxSecuritySector        = null;
        SS.SecuritySector? decodedXboxSecuritySector = null;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_DMI) == true)
            imageFormat.ReadMediaTag(MediaTagType.Xbox_DMI, out xboxDmi);

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_SecuritySector) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.Xbox_SecuritySector, out xboxSecuritySector);

            if(errno == ErrorNumber.NoError) decodedXboxSecuritySector = SS.Decode(xboxSecuritySector);
        }

        if(xboxDmi != null || xboxSecuritySector != null)
        {
            XboxInfo = new XboxInfo
            {
                DataContext = new XboxInfoViewModel(null, xboxDmi, xboxSecuritySector, decodedXboxSecuritySector, view)
            };
        }

        errno = ErrorNumber.NoData;
        byte[] pcmciaCis = null;

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.PCMCIA_CIS) == true)
            errno = imageFormat.ReadMediaTag(MediaTagType.PCMCIA_CIS, out pcmciaCis);

        if(errno == ErrorNumber.NoError)
        {
            PcmciaInfo = new PcmciaInfo
            {
                DataContext = new PcmciaInfoViewModel(pcmciaCis, view)
            };
        }

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

        if(cid != null || csd != null || ocr != null || extendedCsd != null || scr != null)
        {
            SdMmcInfo = new SdMmcInfo
            {
                DataContext = new SdMmcInfoViewModel(deviceType, cid, csd, ocr, extendedCsd, scr)
            };
        }

        if(imageFormat is IOpticalMediaImage opticalMediaImage)
        {
            try
            {
                if(opticalMediaImage.Sessions is { Count: > 0 })
                    foreach(Session session in opticalMediaImage.Sessions)
                        Sessions.Add(session);
            }
            catch(Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }

            try
            {
                if(opticalMediaImage.Tracks is { Count: > 0 })
                    foreach(Track track in opticalMediaImage.Tracks)
                        Tracks.Add(track);
            }
            catch(Exception ex)
            {
                // ignored
                SentrySdk.CaptureException(ex);
            }
        }

        if(imageFormat is IFluxImage fluxImage)
        {
            ErrorNumber fluxError = fluxImage.GetAllFluxCaptures(out List<FluxCapture> fluxCaptures);

            if(fluxError == ErrorNumber.NoError && fluxCaptures is { Count: > 0 })
            {
                FluxInfo = new FluxInfo
                {
                    DataContext = new FluxInfoViewModel(fluxCaptures)
                };
            }
        }

        if(imageFormat.DumpHardware is null) return;

        foreach(DumpHardware dump in imageFormat.DumpHardware)
        {
            foreach(Extent extent in dump.Extents)
            {
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
    public FluxInfo                                FluxInfo                  { get; }
    public IImage                                  MediaLogo                 { get; }
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
    public ICommand                                EntropyCommand            { get; }
    public ICommand                                VerifyCommand             { get; }
    public ICommand                                ChecksumCommand           { get; }
    public ICommand                                ConvertCommand            { get; }
    public ICommand                                CreateSidecarCommand      { get; }
    public ICommand                                ViewSectorsCommand        { get; }
    public ICommand                                DecodeMediaTagCommand     { get; }

    public bool DriveInformationVisible => DriveManufacturerText     != null ||
                                           DriveModelText            != null ||
                                           DriveSerialNumberText     != null ||
                                           DriveFirmwareRevisionText != null ||
                                           MediaGeometryText         != null;

    public bool MediaInformationVisible => MediaSequenceText     != null ||
                                           MediaTitleText        != null ||
                                           MediaManufacturerText != null ||
                                           MediaModelText        != null ||
                                           MediaSerialNumberText != null ||
                                           MediaBarcodeText      != null ||
                                           MediaPartNumberText   != null;

    void Entropy()
    {
        if(_imageEntropy != null)
        {
            _imageEntropy.Show();

            return;
        }

        _imageEntropy             = new ImageEntropy();
        _imageEntropy.DataContext = new ImageEntropyViewModel(_imageFormat, _imageEntropy);

        _imageEntropy.Closed += (_, _) => _imageEntropy = null;

        _imageEntropy.Show();
    }

    void Verify()
    {
        if(_imageVerify != null)
        {
            _imageVerify.Show();

            return;
        }

        _imageVerify             = new ImageVerify();
        _imageVerify.DataContext = new ImageVerifyViewModel(_imageFormat, _imageVerify);

        _imageVerify.Closed += (_, _) => _imageVerify = null;

        _imageVerify.Show();
    }

    void Checksum()
    {
        if(_imageChecksum != null)
        {
            _imageChecksum.Show();

            return;
        }

        _imageChecksum             = new ImageChecksum();
        _imageChecksum.DataContext = new ImageChecksumViewModel(_imageFormat, _imageChecksum);

        _imageChecksum.Closed += (_, _) => _imageChecksum = null;

        _imageChecksum.Show();
    }

    void Convert()
    {
        if(_imageConvert != null)
        {
            _imageConvert.Show();

            return;
        }

        _imageConvert             = new ImageConvert();
        _imageConvert.DataContext = new ImageConvertViewModel(_imageFormat, _imagePath, _imageConvert);

        _imageConvert.Closed += (_, _) => _imageConvert = null;

        _imageConvert.Show();
    }

    void CreateSidecar()
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

        _imageSidecar.Closed += (_, _) => _imageSidecar = null;

        _imageSidecar.Show();
    }

    void ViewSectors()
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

        _viewSector.Closed += (_, _) => _viewSector = null;

        _viewSector.Show();
    }

    void DecodeMediaTag()
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

        _decodeMediaTags.Closed += (_, _) => _decodeMediaTags = null;

        _decodeMediaTags.Show();
    }
}