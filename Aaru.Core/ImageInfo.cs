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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.ATA;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.PCMCIA;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.Xbox;
using Aaru.Helpers;
using Aaru.Logging;
using Humanizer;
using Sentry;
using Spectre.Console;
using Spectre.Console.Json;
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using Inquiry = Aaru.Decoders.SCSI.Inquiry;
using Session = Aaru.CommonTypes.Structs.Session;
using Track = Aaru.CommonTypes.Structs.Track;
using Tuple = Aaru.Decoders.PCMCIA.Tuple;

namespace Aaru.Core;

/// <summary>Image information operations</summary>
public static class ImageInfo
{
    const string MODULE_NAME = "Image information";

    /// <summary>Prints image information to console</summary>
    /// <param name="imageFormat">Media image</param>
    public static void PrintImageInfo(IBaseImage imageFormat)
    {
        AaruLogging.WriteLine(Localization.Core.Image_information_WithMarkup);

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Version))
        {
            AaruLogging.WriteLine(Localization.Core.Format_0_version_1_WithMarkup,
                                  Markup.Escape(imageFormat.Format),
                                  Markup.Escape(imageFormat.Info.Version));
        }
        else
            AaruLogging.WriteLine(Localization.Core.Format_0_WithMarkup, Markup.Escape(imageFormat.Format));

        switch(string.IsNullOrWhiteSpace(imageFormat.Info.Application))
        {
            case false when !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion):
                AaruLogging.WriteLine(Localization.Core.Was_created_with_0_version_1_WithMarkup,
                                      Markup.Escape(imageFormat.Info.Application),
                                      Markup.Escape(imageFormat.Info.ApplicationVersion));

                break;
            case false:
                AaruLogging.WriteLine(Localization.Core.Was_created_with_0_WithMarkup,
                                      Markup.Escape(imageFormat.Info.Application));

                break;
        }

        AaruLogging.WriteLine(Localization.Core.Image_without_headers_is_0_bytes_long, imageFormat.Info.ImageSize);

        AaruLogging.WriteLine(Localization.Core.Contains_a_media_of_0_sectors_with_a_maximum_sector_size_of_1_bytes_etc,
                              imageFormat.Info.Sectors,
                              imageFormat.Info.SectorSize,
                              ByteSize.FromBytes(imageFormat.Info.Sectors * imageFormat.Info.SectorSize).Humanize());

        if(imageFormat.Info.NegativeSectors > 0 || imageFormat.Info.OverflowSectors > 0)
            AaruLogging.WriteLine(Localization.Core.Image_has_0_leadin_and_1_leadout_sectors_WithMarkup,
                                  imageFormat.Info.NegativeSectors,
                                  imageFormat.Info.OverflowSectors);

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
            AaruLogging.WriteLine(Localization.Core.Created_by_0_WithMarkup, Markup.Escape(imageFormat.Info.Creator));

        if(imageFormat.Info.CreationTime != DateTime.MinValue)
            AaruLogging.WriteLine(Localization.Core.Created_on_0, imageFormat.Info.CreationTime);

        if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
            AaruLogging.WriteLine(Localization.Core.Last_modified_on_0, imageFormat.Info.LastModificationTime);

        AaruLogging.WriteLine(Localization.Core.Contains_a_media_of_type_0_and_XML_type_1_WithMarkup,
                              imageFormat.Info.MediaType.Humanize(),
                              imageFormat.Info.MetadataMediaType);

        AaruLogging.WriteLine(imageFormat.Info.HasPartitions
                                  ? Localization.Core.Has_partitions
                                  : Localization.Core.Doesnt_have_partitions);

        AaruLogging.WriteLine(imageFormat.Info.HasSessions
                                  ? Localization.Core.Has_sessions
                                  : Localization.Core.Doesnt_have_sessions);

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Comments))
            AaruLogging.WriteLine(Localization.Core.Comments_0_WithMarkup, Markup.Escape(imageFormat.Info.Comments));

        if(imageFormat.Info.MediaSequence != 0 && imageFormat.Info.LastMediaSequence != 0)
        {
            AaruLogging.WriteLine(Localization.Core.Media_is_number_0_on_a_set_of_1_medias,
                                  imageFormat.Info.MediaSequence,
                                  imageFormat.Info.LastMediaSequence);
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
        {
            AaruLogging.WriteLine(Localization.Core.Media_title_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaTitle));
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
        {
            AaruLogging.WriteLine(Localization.Core.Media_manufacturer_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaManufacturer));
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
        {
            AaruLogging.WriteLine(Localization.Core.Media_model_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaModel));
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
        {
            AaruLogging.WriteLine(Localization.Core.Media_serial_number_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaSerialNumber));
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
        {
            AaruLogging.WriteLine(Localization.Core.Media_barcode_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaBarcode));
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
        {
            AaruLogging.WriteLine(Localization.Core.Media_part_number_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaPartNumber));
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
        {
            AaruLogging.WriteLine(Localization.Core.Drive_manufacturer_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.DriveManufacturer));
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
        {
            AaruLogging.WriteLine(Localization.Core.Drive_model_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.DriveModel));
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
        {
            AaruLogging.WriteLine(Localization.Core.Drive_serial_number_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.DriveSerialNumber));
        }

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
        {
            AaruLogging.WriteLine(Localization.Core.Drive_firmware_info_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.DriveFirmwareRevision));
        }

        if(imageFormat.Info.Cylinders > 0                                      &&
           imageFormat.Info is { Heads: > 0, SectorsPerTrack: > 0 }            &&
           imageFormat.Info.MetadataMediaType != MetadataMediaType.OpticalDisc &&
           imageFormat is not ITapeImage { IsTape: true })
        {
            AaruLogging.WriteLine(Localization.Core.Media_geometry_0_cylinders_1_heads_2_sectors_per_track_WithMarkup,
                                  imageFormat.Info.Cylinders,
                                  imageFormat.Info.Heads,
                                  imageFormat.Info.SectorsPerTrack);
        }

        if(imageFormat.Info.ReadableMediaTags is { Count: > 0 })
        {
            AaruLogging.WriteLine(Localization.Core.Contains_0_readable_media_tags_WithMarkup,
                                  imageFormat.Info.ReadableMediaTags.Count);

            foreach(MediaTagType tag in imageFormat.Info.ReadableMediaTags.OrderBy(static t => t.Humanize()))
                AaruLogging.WriteLine("[italic][rosybrown]{0}[/][/]", Markup.Escape(tag.Humanize()));
        }

        if(imageFormat.Info.ReadableSectorTags is { Count: > 0 })
        {
            AaruLogging.WriteLine(Localization.Core.Contains_0_readable_sector_tags_WithMarkup,
                                  imageFormat.Info.ReadableSectorTags.Count);

            foreach(SectorTagType tag in imageFormat.Info.ReadableSectorTags.OrderBy(static t => t.Humanize()))
                AaruLogging.WriteLine("[italic][rosybrown]{0}[/][/]", Markup.Escape(tag.Humanize()));
        }

        AaruLogging.WriteLine();

        if(imageFormat.Info.MetadataMediaType == MetadataMediaType.LinearMedia)
            PrintByteAddressableImageInfo(imageFormat as IByteAddressableImage);
        else
            PrintBlockImageInfo(imageFormat as IMediaImage);

        if(imageFormat.DumpHardware == null) return;

        int manufacturerLen = Localization.Core.Title_Manufacturer.Length;
        int modelLen        = Localization.Core.Title_Model.Length;
        int serialLen       = Localization.Core.Title_Serial.Length;
        int softwareLen     = Localization.Core.Title_Software.Length;
        int versionLen      = Localization.Core.Title_Version.Length;
        int osLen           = Localization.Core.Title_Operating_system.Length;
        int sectorLen       = Localization.Core.Title_Start.Length;

        foreach(DumpHardware dump in imageFormat.DumpHardware)
        {
            if(dump.Manufacturer?.Length > manufacturerLen) manufacturerLen = dump.Manufacturer.Length;

            if(dump.Model?.Length > modelLen) modelLen = dump.Model.Length;

            if(dump.Serial?.Length > serialLen) serialLen = dump.Serial.Length;

            if(dump.Software?.Name?.Length > softwareLen) softwareLen = dump.Software.Name.Length;

            if(dump.Software?.Version?.Length > versionLen) versionLen = dump.Software.Version.Length;

            if(dump.Software?.OperatingSystem?.Length > osLen) osLen = dump.Software.OperatingSystem.Length;

            foreach(Extent extent in dump.Extents)
            {
                if($"{extent.Start}".Length > sectorLen) sectorLen = $"{extent.Start}".Length;

                if($"{extent.End}".Length > sectorLen) sectorLen = $"{extent.End}".Length;
            }
        }

        var table = new Table
        {
            Title = new TableTitle(Localization.Core.Title_Dump_hardware_information)
        };

        AaruLogging.Information(Localization.Core.Title_Dump_hardware_information);

        table.AddColumn(Localization.Core.Title_Manufacturer);
        table.AddColumn(Localization.Core.Title_Model);
        table.AddColumn(Localization.Core.Title_Serial);
        table.AddColumn(Localization.Core.Title_Software);
        table.AddColumn(Localization.Core.Title_Version);
        table.AddColumn(Localization.Core.Title_Operating_system);
        table.AddColumn(Localization.Core.Title_Start);
        table.AddColumn(Localization.Core.Title_End);
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Yellow);
        table.Columns[6].RightAligned();
        table.Columns[7].RightAligned();

        foreach(DumpHardware dump in imageFormat.DumpHardware)
        {
            foreach(Extent extent in dump.Extents)
            {
                table.AddRow($"[navy]{Markup.Escape(dump.Manufacturer             ?? "")}[/]",
                             $"[navy]{Markup.Escape(dump.Model                    ?? "")}[/]",
                             $"[fuchsia]{Markup.Escape(dump.Serial                ?? "")}[/]",
                             $"[red3]{Markup.Escape(dump.Software.Name            ?? "")}[/]",
                             $"[red3]{Markup.Escape(dump.Software.Version         ?? "")}[/]",
                             $"[red3]{Markup.Escape(dump.Software.OperatingSystem ?? "")}[/]",
                             $"[lime]{extent.Start}[/]",
                             $"[lime]{extent.End}[/]");

                // Write each row to AaruLogging.Information as a line
                AaruLogging
                   .Information($"Manufacturer {dump.Manufacturer ?? ""}, model {dump.Model ?? ""}, serial {dump.Serial ?? ""}, software {dump.Software?.Name ?? ""}, version {dump.Software?.Version ?? ""}, operating system {dump.Software?.OperatingSystem ?? ""}, start {extent.Start}, end {extent.End}");
            }
        }

        AnsiConsole.Write(table);
        AaruLogging.WriteLine();
    }

    static void PrintByteAddressableImageInfo(IByteAddressableImage imageFormat)
    {
        ErrorNumber errno = imageFormat.GetMappings(out LinearMemoryMap mappings);

        if(errno != ErrorNumber.NoError) return;

        string jsonString = JsonSerializer.Serialize(mappings,
                                                     new JsonSerializerOptions
                                                     {
                                                         WriteIndented          = true,
                                                         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                                         Converters =
                                                         {
                                                             new JsonStringEnumConverter()
                                                         }
                                                     });

        AaruLogging.Information(Localization.Core.Mapping_WithMarkup);
        AaruLogging.Information(jsonString);

        AnsiConsole.Write(new Panel(new JsonText(jsonString)).Header(Localization.Core.Mapping_WithMarkup)
                                                             .Collapse()
                                                             .RoundedBorder()
                                                             .BorderColor(Color.Yellow));
    }

    static void PrintBlockImageInfo(IMediaImage imageFormat)
    {
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

                AaruLogging.WriteLine(Localization.Core.SCSI_INQUIRY_contained_in_image_WithMarkup);
                AaruLogging.Write(Inquiry.Prettify(inquiry));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATA_IDENTIFY) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.ATA_IDENTIFY, out byte[] identify);

            if(errno == ErrorNumber.NoError)

            {
                AaruLogging.WriteLine(Localization.Core.ATA_IDENTIFY_contained_in_image_WithMarkup);
                AaruLogging.Write(Identify.Prettify(identify));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATAPI_IDENTIFY) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.ATAPI_IDENTIFY, out byte[] identify);

            if(errno == ErrorNumber.NoError)

            {
                AaruLogging.WriteLine(Localization.Core.ATAPI_IDENTIFY_contained_in_image_WithMarkup);
                AaruLogging.Write(Identify.Prettify(identify));
                AaruLogging.WriteLine();
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
                    AaruLogging.WriteLine(Localization.Core.SCSI_MODE_SENSE_10_contained_in_image_WithMarkup);
                    PrintScsiModePages.Print(decMode.Value, scsiDeviceType, scsiVendorId);
                    AaruLogging.WriteLine();
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
                    AaruLogging.WriteLine(Localization.Core.SCSI_MODE_SENSE_6_contained_in_image_WithMarkup);
                    PrintScsiModePages.Print(decMode.Value, scsiDeviceType, scsiVendorId);
                    AaruLogging.WriteLine();
                }
            }
        }
        else if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SCSI_MODEPAGE_2A) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SCSI_MODEPAGE_2A, out byte[] mode2A);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.Write(Modes.PrettifyModePage_2A(mode2A));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_FullTOC) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_FullTOC, out byte[] toc);

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

                AaruLogging.WriteLine(Localization.Core.CompactDisc_Table_of_Contents_contained_in_image_WithMarkup);
                AaruLogging.Write(FullTOC.Prettify(toc));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_PMA) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_PMA, out byte[] pma);

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

                AaruLogging.WriteLine(Localization.Core.CompactDisc_Program_Memory_Area_contained_in_image_WithMarkup);

                AaruLogging.Write(PMA.Prettify(pma));
                AaruLogging.WriteLine();
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
                    var tmp = new byte[atip.Length + 4];
                    Array.Copy(atip, 0, tmp, 4, atip.Length);
                    tmp[0] = (byte)((atip.Length & 0xFF000000) >> 24);
                    tmp[1] = (byte)((atip.Length & 0xFF0000)   >> 16);
                    tmp[2] = (byte)((atip.Length & 0xFF00)     >> 8);
                    tmp[3] = (byte)(atip.Length & 0xFF);
                    atip   = tmp;
                }

                AaruLogging.WriteLine(Localization.Core
                                                  .CompactDisc_Absolute_Time_In_Pregroove_ATIP_contained_in_image_WithMarkup);

                AaruLogging.Write(ATIP.Prettify(atip));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_TEXT) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_TEXT, out byte[] cdtext);

            if(errno == ErrorNumber.NoError)
            {
                ushort dataLen = Swapping.Swap(BitConverter.ToUInt16(cdtext, 0));

                if(dataLen + 2 != cdtext.Length)
                {
                    var tmp = new byte[cdtext.Length + 4];
                    Array.Copy(cdtext, 0, tmp, 4, cdtext.Length);
                    tmp[0] = (byte)((cdtext.Length + 2 & 0xFF00) >> 8);
                    tmp[1] = (byte)(cdtext.Length + 2 & 0xFF);
                    cdtext = tmp;
                }

                AaruLogging.WriteLine(Localization.Core.CompactDisc_Lead_in_CD_Text_contained_in_image_WithMarkup);
                AaruLogging.Write(Markup.Escape(CDTextOnLeadIn.Prettify(cdtext)));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_MCN) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_MCN, out byte[] mcn);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core
                                                  .CompactDisc_Media_Catalogue_Number_contained_in_image_0_WithMarkup,
                                      Encoding.UTF8.GetString(mcn));

                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PreRecordedInfo) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVDR_PreRecordedInfo, out byte[] pri);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.DVD_RW_Pre_Recorded_Information_WithMarkup);
                AaruLogging.Write(PRI.Prettify(pri));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_PFI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVD_PFI, out byte[] pfi);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.DVD_Physical_Format_Information_contained_in_image_WithMarkup);
                AaruLogging.Write(PFI.Prettify(pfi, imageFormat.Info.MediaType));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_PFI_2ndLayer) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVD_PFI_2ndLayer, out byte[] pfi);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core
                                                  .DVD_2nd_layer_Physical_Format_Information_contained_in_image_WithMarkup);

                AaruLogging.Write(PFI.Prettify(pfi, imageFormat.Info.MediaType));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDRAM_DDS) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVDRAM_DDS, out byte[] dds);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core
                                                  .DVD_RAM_Disc_Definition_Structure_contained_in_image_WithMarkup);

                AaruLogging.Write(DDS.Prettify(dds));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PFI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVDR_PFI, out byte[] pfi);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core
                                                  .DVD_R_Physical_Format_Information_contained_in_image_WithMarkup);

                AaruLogging.Write(PFI.Prettify(pfi, imageFormat.Info.MediaType));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.BD_DI, out byte[] di);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.Bluray_Disc_Information_contained_in_image_WithMarkup);
                AaruLogging.Write(DI.Prettify(di));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DDS) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.BD_DDS, out byte[] dds);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.Bluray_Disc_Definition_Structure_contained_in_image_WithMarkup);
                AaruLogging.Write(Decoders.Bluray.DDS.Prettify(dds));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.PCMCIA_CIS) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.PCMCIA_CIS, out byte[] cis);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.PCMCIA_CIS_WithMarkup);
                Tuple[] tuples = CIS.GetTuples(cis);

                if(tuples != null)
                {
                    foreach(Tuple tuple in tuples)
                    {
                        switch(tuple.Code)
                        {
                            case TupleCodes.CISTPL_NULL:
                            case TupleCodes.CISTPL_END:
                                break;
                            case TupleCodes.CISTPL_DEVICEGEO:
                            case TupleCodes.CISTPL_DEVICEGEO_A:
                                AaruLogging.WriteLine(CIS.PrettifyDeviceGeometryTuple(tuple));

                                break;
                            case TupleCodes.CISTPL_MANFID:
                                AaruLogging.WriteLine(CIS.PrettifyManufacturerIdentificationTuple(tuple));

                                break;
                            case TupleCodes.CISTPL_VERS_1:
                                AaruLogging.WriteLine(CIS.PrettifyLevel1VersionTuple(tuple));

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
                                AaruLogging.Debug(MODULE_NAME,
                                                  Localization.Core.Invoke_Found_undecoded_tuple_ID_0,
                                                  tuple.Code);

                                break;
                            default:
                                AaruLogging.Debug(MODULE_NAME,
                                                  Localization.Core.Found_unknown_tuple_ID_0,
                                                  (byte)tuple.Code);

                                break;
                        }
                    }
                }
                else
                    AaruLogging.Debug(MODULE_NAME, Localization.Core.Could_not_get_tuples);
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CID) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SD_CID, out byte[] cid);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.SecureDigital_CID_contained_in_image_WithMarkup);
                AaruLogging.Write(Decoders.SecureDigital.Decoders.PrettifyCID(cid));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CSD) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SD_CSD, out byte[] csd);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.SecureDigital_CSD_contained_in_image_WithMarkup);
                AaruLogging.Write(Decoders.SecureDigital.Decoders.PrettifyCSD(csd));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_SCR) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SD_SCR, out byte[] scr);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.SecureDigital_SCR_contained_in_image_WithMarkup);
                AaruLogging.Write(Decoders.SecureDigital.Decoders.PrettifySCR(scr));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_OCR) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SD_OCR, out byte[] ocr);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.SecureDigital_OCR_contained_in_image_WithMarkup);
                AaruLogging.Write(Decoders.SecureDigital.Decoders.PrettifyOCR(ocr));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CID) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.MMC_CID, out byte[] cid);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.MultiMediaCard_CID_contained_in_image_WithMarkup);
                AaruLogging.Write(Decoders.MMC.Decoders.PrettifyCID(cid));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CSD) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.MMC_CSD, out byte[] csd);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.MultiMediaCard_CSD_contained_in_image_WithMarkup);
                AaruLogging.Write(Decoders.MMC.Decoders.PrettifyCSD(csd));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_ExtendedCSD) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.MMC_ExtendedCSD, out byte[] ecsd);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.MultiMediaCard_Extended_CSD_contained_in_image_WithMarkup);
                AaruLogging.Write(Decoders.MMC.Decoders.PrettifyExtendedCSD(ecsd));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_OCR) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.MMC_OCR, out byte[] ocr);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.MultiMediaCard_OCR_contained_in_image_WithMarkup);
                AaruLogging.Write(Decoders.MMC.Decoders.PrettifyOCR(ocr));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_PFI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.Xbox_PFI, out byte[] xpfi);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.Xbox_Physical_Format_Information_contained_in_image_WithMarkup);
                AaruLogging.Write(PFI.Prettify(xpfi, imageFormat.Info.MediaType));
                AaruLogging.WriteLine();
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
                        AaruLogging.WriteLine(Localization.Core.Xbox_DMI_contained_in_image_WithMarkup);
                        AaruLogging.Write(DMI.PrettifyXbox(xmi));
                        AaruLogging.WriteLine();
                    }
                }

                if(DMI.IsXbox360(xdmi))
                {
                    DMI.Xbox360DMI? xmi = DMI.DecodeXbox360(xdmi);

                    if(xmi.HasValue)
                    {
                        AaruLogging.WriteLine(Localization.Core.Xbox_360_DMI_contained_in_image_WithMarkup);
                        AaruLogging.Write(DMI.PrettifyXbox360(xmi));
                        AaruLogging.WriteLine();
                    }
                }
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_SecuritySector) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.Xbox_SecuritySector, out byte[] toc);

            if(errno == ErrorNumber.NoError)
            {
                AaruLogging.WriteLine(Localization.Core.Xbox_Security_Sectors_contained_in_image_WithMarkup);
                AaruLogging.Write(SS.Prettify(toc));
                AaruLogging.WriteLine();
            }
        }

        if(imageFormat is IFluxImage fluxImage                                                 &&
           fluxImage.GetAllFluxCaptures(out List<FluxCapture> captures) == ErrorNumber.NoError &&
           captures is { Count: > 0 })
            AaruLogging.WriteLine(Localization.Core.Image_flux_captures);

        if(imageFormat is not IOpticalMediaImage opticalImage) return;

        try
        {
            if(opticalImage.Sessions is { Count: > 0 })
            {
                var table = new Table
                {
                    Title = new TableTitle(Localization.Core.Title_Image_sessions)
                };

                AaruLogging.Information(Localization.Core.Title_Image_sessions);

                table.Border(TableBorder.Rounded);
                table.BorderColor(Color.Yellow);

                table.AddColumn(Localization.Core.Title_Session);
                table.AddColumn(Localization.Core.Title_First_track);
                table.AddColumn(Localization.Core.Title_Last_track);
                table.AddColumn(Localization.Core.Title_Start);
                table.AddColumn(Localization.Core.Title_End);
                table.Columns[0].RightAligned();
                table.Columns[1].RightAligned();
                table.Columns[2].RightAligned();
                table.Columns[3].RightAligned();
                table.Columns[4].RightAligned();

                foreach(Session session in opticalImage.Sessions)
                {
                    table.AddRow($"[navy]{session.Sequence}[/]",
                                 $"[purple]{session.StartTrack}[/]",
                                 $"[purple]{session.EndTrack}[/]",
                                 $"[lime]{session.StartSector}[/]",
                                 $"[lime]{session.EndSector}[/]");

                    // Write all the session infomation to AaruLogging.Information in a single line
                    AaruLogging
                       .Information($"Session {session.Sequence}: first track {session.StartTrack}, last track {session.EndTrack}, start sector {session.StartSector}, end sector {session.EndSector}");
                }

                AnsiConsole.Write(table);
                AaruLogging.WriteLine();
            }
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }

        if(opticalImage.ReadDpm(out DataPositionMeasurement dpm) == ErrorNumber.NoError)
        {
            AaruLogging.WriteLine(Localization.Core.DPM_0_entries_every_1_sectors_from_2_to_3_4_turns_WithMarkup,
                                  dpm.Entries.Length,
                                  dpm.NominalSpacing,
                                  dpm.Entries[0].Lba,
                                  dpm.Entries[^1].Lba,
                                  (double)dpm.Entries[^1].Angle / Dpm.UNITS_PER_TURN);

            if(dpm.Calibrations?.Length > 0)
            {
                AaruLogging.WriteLine(Localization.Core.DPM_measured_with_0_calibrations_1_layers_WithMarkup,
                                      dpm.Calibrations.Length,
                                      (dpm.LayerEnds?.Length ?? 0) + 1);
            }

            AaruLogging.WriteLine();
        }

        try
        {
            if(opticalImage.Tracks is not { Count: > 0 }) return;

            var table = new Table
            {
                Title = new TableTitle(Localization.Core.Title_Image_tracks)
            };

            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            table.AddColumn(Localization.Core.Title_Track);
            table.AddColumn(Localization.Core.Title_Type_for_media);
            table.AddColumn(Localization.Core.Title_Bps);
            table.AddColumn(Localization.Core.Title_Raw_bps);
            table.AddColumn(Localization.Core.Title_Subchannel);
            table.AddColumn(Localization.Core.Title_Pregap);
            table.AddColumn(Localization.Core.Title_Start);
            table.AddColumn(Localization.Core.Title_End);
            table.Columns[0].RightAligned();
            table.Columns[2].RightAligned();
            table.Columns[3].RightAligned();
            table.Columns[5].RightAligned();
            table.Columns[6].RightAligned();
            table.Columns[7].RightAligned();

            foreach(Track track in opticalImage.Tracks)
            {
                table.AddRow($"[teal]{track.Sequence}[/]",
                             $"[orange3]{track.Type.Humanize()}[/]",
                             $"[aqua]{track.BytesPerSector}[/]",
                             $"[aqua]{track.RawBytesPerSector}[/]",
                             $"[fuchsia]{track.SubchannelType}[/]",
                             $"[darkgreen]{track.Pregap}[/]",
                             $"[lime]{track.StartSector}[/]",
                             $"[lime]{track.EndSector}[/]");

                // Write all the track information to AaruLogging.Information in a single line
                AaruLogging
                   .Information($"Track {track.Sequence}: type {track.Type.Humanize()}, bytes per sector {track.BytesPerSector}, raw bytes per sector {track.RawBytesPerSector}, subchannel type {track.SubchannelType}, pregap {track.Pregap}, start sector {track.StartSector}, end sector {track.EndSector}");
            }

            AnsiConsole.Write(table);

            if(!opticalImage.Tracks.Any(static t => t.Indexes.Any())) return;

            AaruLogging.WriteLine();

            table = new Table
            {
                Title = new TableTitle(Localization.Core.Title_Image_indexes)
            };

            AaruLogging.Information(Localization.Core.Title_Image_indexes);

            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Yellow);

            table.AddColumn(Localization.Core.Title_Track);
            table.AddColumn(Localization.Core.Title_Index);
            table.AddColumn(Localization.Core.Title_Start);
            table.Columns[0].RightAligned();
            table.Columns[1].RightAligned();
            table.Columns[2].RightAligned();

            foreach(Track track in opticalImage.Tracks)
            {
                foreach(KeyValuePair<ushort, int> index in track.Indexes)
                {
                    table.AddRow($"[teal]{track.Sequence}[/]", $"[darkgreen]{index.Key}[/]", $"[lime]{index.Value}[/]");

                    // Write all the index information to AaruLogging.Information in a single line
                    AaruLogging.Information($"Track {track.Sequence}, index {index.Key}: start sector {index.Value}");
                }
            }

            AnsiConsole.Write(table);
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
        finally
        {
            AaruLogging.WriteLine();
        }
    }
}