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
// Copyright © 2011-2023 Natalia Portillo
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
using Aaru.Console;
using Aaru.Decoders.ATA;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.PCMCIA;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.Xbox;
using Aaru.Helpers;
using Spectre.Console;
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
    /// <summary>Prints image information to console</summary>
    /// <param name="imageFormat">Media image</param>
    public static void PrintImageInfo(IBaseImage imageFormat)
    {
        AaruConsole.WriteLine(Localization.Core.Image_information_WithMarkup);

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Version))
            AaruConsole.WriteLine(Localization.Core.Format_0_version_1_WithMarkup, Markup.Escape(imageFormat.Format),
                                  Markup.Escape(imageFormat.Info.Version));
        else
            AaruConsole.WriteLine(Localization.Core.Format_0_WithMarkup, Markup.Escape(imageFormat.Format));

        switch(string.IsNullOrWhiteSpace(imageFormat.Info.Application))
        {
            case false when !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion):
                AaruConsole.WriteLine(Localization.Core.Was_created_with_0_version_1_WithMarkup,
                                      Markup.Escape(imageFormat.Info.Application),
                                      Markup.Escape(imageFormat.Info.ApplicationVersion));

                break;
            case false:
                AaruConsole.WriteLine(Localization.Core.Was_created_with_0_WithMarkup,
                                      Markup.Escape(imageFormat.Info.Application));

                break;
        }

        AaruConsole.WriteLine(Localization.Core.Image_without_headers_is_0_bytes_long, imageFormat.Info.ImageSize);

        AaruConsole.WriteLine(Localization.Core.Contains_a_media_of_0_sectors_with_a_maximum_sector_size_of_1_bytes_etc,
                              imageFormat.Info.Sectors, imageFormat.Info.SectorSize,
                              imageFormat.Info.Sectors * imageFormat.Info.SectorSize);

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
            AaruConsole.WriteLine(Localization.Core.Created_by_0_WithMarkup, Markup.Escape(imageFormat.Info.Creator));

        if(imageFormat.Info.CreationTime != DateTime.MinValue)
            AaruConsole.WriteLine(Localization.Core.Created_on_0, imageFormat.Info.CreationTime);

        if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
            AaruConsole.WriteLine(Localization.Core.Last_modified_on_0, imageFormat.Info.LastModificationTime);

        AaruConsole.WriteLine(Localization.Core.Contains_a_media_of_type_0_and_XML_type_1_WithMarkup,
                              imageFormat.Info.MediaType, imageFormat.Info.MetadataMediaType);

        AaruConsole.WriteLine(imageFormat.Info.HasPartitions ? Localization.Core.Has_partitions
                                  : Localization.Core.Doesnt_have_partitions);

        AaruConsole.WriteLine(imageFormat.Info.HasSessions ? Localization.Core.Has_sessions
                                  : Localization.Core.Doesnt_have_sessions);

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.Comments))
            AaruConsole.WriteLine(Localization.Core.Comments_0_WithMarkup, Markup.Escape(imageFormat.Info.Comments));

        if(imageFormat.Info.MediaSequence     != 0 &&
           imageFormat.Info.LastMediaSequence != 0)
            AaruConsole.WriteLine(Localization.Core.Media_is_number_0_on_a_set_of_1_medias,
                                  imageFormat.Info.MediaSequence, imageFormat.Info.LastMediaSequence);

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
            AaruConsole.WriteLine(Localization.Core.Media_title_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaTitle));

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
            AaruConsole.WriteLine(Localization.Core.Media_manufacturer_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaManufacturer));

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
            AaruConsole.WriteLine(Localization.Core.Media_model_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaModel));

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
            AaruConsole.WriteLine(Localization.Core.Media_serial_number_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaSerialNumber));

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
            AaruConsole.WriteLine(Localization.Core.Media_barcode_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaBarcode));

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
            AaruConsole.WriteLine(Localization.Core.Media_part_number_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.MediaPartNumber));

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
            AaruConsole.WriteLine(Localization.Core.Drive_manufacturer_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.DriveManufacturer));

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
            AaruConsole.WriteLine(Localization.Core.Drive_model_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.DriveModel));

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
            AaruConsole.WriteLine(Localization.Core.Drive_serial_number_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.DriveSerialNumber));

        if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
            AaruConsole.WriteLine(Localization.Core.Drive_firmware_info_0_WithMarkup,
                                  Markup.Escape(imageFormat.Info.DriveFirmwareRevision));

        if(imageFormat.Info.Cylinders > 0                                      &&
           imageFormat.Info is { Heads: > 0, SectorsPerTrack: > 0 }            &&
           imageFormat.Info.MetadataMediaType != MetadataMediaType.OpticalDisc &&
           imageFormat is not ITapeImage { IsTape: true })
            AaruConsole.WriteLine(Localization.Core.Media_geometry_0_cylinders_1_heads_2_sectors_per_track_WithMarkup,
                                  imageFormat.Info.Cylinders, imageFormat.Info.Heads, imageFormat.Info.SectorsPerTrack);

        if(imageFormat.Info.ReadableMediaTags is { Count: > 0 })
        {
            AaruConsole.WriteLine(Localization.Core.Contains_0_readable_media_tags_WithMarkup,
                                  imageFormat.Info.ReadableMediaTags.Count);

            foreach(MediaTagType tag in imageFormat.Info.ReadableMediaTags.OrderBy(t => t))
                AaruConsole.Write("[italic]{0}[/] ", Markup.Escape(tag.ToString()));

            AaruConsole.WriteLine();
        }

        if(imageFormat.Info.ReadableSectorTags is { Count: > 0 })
        {
            AaruConsole.WriteLine(Localization.Core.Contains_0_readable_sector_tags_WithMarkup,
                                  imageFormat.Info.ReadableSectorTags.Count);

            foreach(SectorTagType tag in imageFormat.Info.ReadableSectorTags.OrderBy(t => t))
                AaruConsole.Write("[italic]{0}[/] ", tag);

            AaruConsole.WriteLine();
        }

        AaruConsole.WriteLine();

        if(imageFormat.Info.MetadataMediaType == MetadataMediaType.LinearMedia)
            PrintByteAddressableImageInfo(imageFormat as IByteAddressableImage);
        else
            PrintBlockImageInfo(imageFormat as IMediaImage);

        if(imageFormat.DumpHardware == null)
            return;

        int manufacturerLen = Localization.Core.Title_Manufacturer.Length;
        int modelLen        = Localization.Core.Title_Model.Length;
        int serialLen       = Localization.Core.Title_Serial.Length;
        int softwareLen     = Localization.Core.Title_Software.Length;
        int versionLen      = Localization.Core.Title_Version.Length;
        int osLen           = Localization.Core.Title_Operating_system.Length;
        int sectorLen       = Localization.Core.Title_Start.Length;

        foreach(DumpHardware dump in imageFormat.DumpHardware)
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

            foreach(Extent extent in dump.Extents)
            {
                if($"{extent.Start}".Length > sectorLen)
                    sectorLen = $"{extent.Start}".Length;

                if($"{extent.End}".Length > sectorLen)
                    sectorLen = $"{extent.End}".Length;
            }
        }

        var table = new Table
        {
            Title = new TableTitle(Localization.Core.Title_Dump_hardware_information)
        };

        table.AddColumn(Localization.Core.Title_Manufacturer);
        table.AddColumn(Localization.Core.Title_Model);
        table.AddColumn(Localization.Core.Title_Serial);
        table.AddColumn(Localization.Core.Title_Software);
        table.AddColumn(Localization.Core.Title_Version);
        table.AddColumn(Localization.Core.Title_Operating_system);
        table.AddColumn(Localization.Core.Title_Start);
        table.AddColumn(Localization.Core.Title_End);

        foreach(DumpHardware dump in imageFormat.DumpHardware)
        {
            foreach(Extent extent in dump.Extents)
                table.AddRow(Markup.Escape(dump.Manufacturer ?? ""), Markup.Escape(dump.Model ?? ""),
                             Markup.Escape(dump.Serial ?? ""), Markup.Escape(dump.Software.Name ?? ""),
                             Markup.Escape(dump.Software.Version ?? ""),
                             Markup.Escape(dump.Software.OperatingSystem ?? ""), extent.Start.ToString(),
                             extent.End.ToString());
        }

        AaruConsole.WriteLine();
    }

    static void PrintByteAddressableImageInfo(IByteAddressableImage imageFormat)
    {
        ErrorNumber errno = imageFormat.GetMappings(out LinearMemoryMap mappings);

        if(errno != ErrorNumber.NoError)
            return;

        AaruConsole.WriteLine(Localization.Core.Mapping_WithMarkup);

        AaruConsole.WriteLine("{0}", Markup.Escape(JsonSerializer.Serialize(mappings, new JsonSerializerOptions
        {
            WriteIndented          = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        })));
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

                AaruConsole.WriteLine(Localization.Core.SCSI_INQUIRY_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Inquiry.Prettify(inquiry));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATA_IDENTIFY) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.ATA_IDENTIFY, out byte[] identify);

            if(errno == ErrorNumber.NoError)

            {
                AaruConsole.WriteLine(Localization.Core.ATA_IDENTIFY_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Identify.Prettify(identify));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.ATAPI_IDENTIFY) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.ATAPI_IDENTIFY, out byte[] identify);

            if(errno == ErrorNumber.NoError)

            {
                AaruConsole.WriteLine(Localization.Core.ATAPI_IDENTIFY_contained_in_image_WithMarkup);
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
                    AaruConsole.WriteLine(Localization.Core.SCSI_MODE_SENSE_10_contained_in_image_WithMarkup);
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
                    AaruConsole.WriteLine(Localization.Core.SCSI_MODE_SENSE_6_contained_in_image_WithMarkup);
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

                AaruConsole.WriteLine(Localization.Core.CompactDisc_Table_of_Contents_contained_in_image_WithMarkup);
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

                AaruConsole.WriteLine(Localization.Core.
                                                   CompactDisc_Power_Management_Area_contained_in_image_WithMarkup);

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

                AaruConsole.WriteLine(Localization.Core.
                                                   CompactDisc_Absolute_Time_In_Pregroove_ATIP_contained_in_image_WithMarkup);

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

                AaruConsole.WriteLine(Localization.Core.CompactDisc_Lead_in_CD_Text_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", CDTextOnLeadIn.Prettify(cdtext));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.CD_MCN) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.CD_MCN, out byte[] mcn);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.
                    WriteLine(Localization.Core.CompactDisc_Media_Catalogue_Number_contained_in_image_0_WithMarkup,
                              Encoding.UTF8.GetString(mcn));

                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PreRecordedInfo) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVDR_PreRecordedInfo, out byte[] pri);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.DVD_RW_Pre_Recorded_Information_WithMarkup);
                AaruConsole.Write("{0}", PRI.Prettify(pri));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVD_PFI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVD_PFI, out byte[] pfi);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.DVD_Physical_Format_Information_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", PFI.Prettify(pfi, imageFormat.Info.MediaType));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDRAM_DDS) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVDRAM_DDS, out byte[] dds);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.
                                                   DVD_RAM_Disc_Definition_Structure_contained_in_image_WithMarkup);

                AaruConsole.Write("{0}", DDS.Prettify(dds));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.DVDR_PFI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.DVDR_PFI, out byte[] pfi);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.
                                                   DVD_R_Physical_Format_Information_contained_in_image_WithMarkup);

                AaruConsole.Write("{0}", PFI.Prettify(pfi, imageFormat.Info.MediaType));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.BD_DI, out byte[] di);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.Bluray_Disc_Information_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", DI.Prettify(di));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.BD_DDS) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.BD_DDS, out byte[] dds);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.Bluray_Disc_Definition_Structure_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Decoders.Bluray.DDS.Prettify(dds));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.PCMCIA_CIS) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.PCMCIA_CIS, out byte[] cis);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.PCMCIA_CIS_WithMarkup);
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
                                AaruConsole.DebugWriteLine("Device-Info command",
                                                           Localization.Core.Invoke_Found_undecoded_tuple_ID_0,
                                                           tuple.Code);

                                break;
                            default:
                                AaruConsole.DebugWriteLine("Device-Info command",
                                                           Localization.Core.Found_unknown_tuple_ID_0,
                                                           (byte)tuple.Code);

                                break;
                        }
                else
                    AaruConsole.DebugWriteLine("Device-Info command", Localization.Core.Could_not_get_tuples);
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CID) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SD_CID, out byte[] cid);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.SecureDigital_CID_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyCID(cid));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_CSD) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SD_CSD, out byte[] csd);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.SecureDigital_CSD_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyCSD(csd));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_SCR) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SD_SCR, out byte[] scr);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.SecureDigital_SCR_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifySCR(scr));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.SD_OCR) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.SD_OCR, out byte[] ocr);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.SecureDigital_OCR_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Decoders.SecureDigital.Decoders.PrettifyOCR(ocr));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CID) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.MMC_CID, out byte[] cid);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.MultiMediaCard_CID_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyCID(cid));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_CSD) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.MMC_CSD, out byte[] csd);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.MultiMediaCard_CSD_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyCSD(csd));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_ExtendedCSD) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.MMC_ExtendedCSD, out byte[] ecsd);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.MultiMediaCard_Extended_CSD_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyExtendedCSD(ecsd));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.MMC_OCR) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.MMC_OCR, out byte[] ocr);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.MultiMediaCard_OCR_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", Decoders.MMC.Decoders.PrettifyOCR(ocr));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat.Info.ReadableMediaTags?.Contains(MediaTagType.Xbox_PFI) == true)
        {
            errno = imageFormat.ReadMediaTag(MediaTagType.Xbox_PFI, out byte[] xpfi);

            if(errno == ErrorNumber.NoError)
            {
                AaruConsole.WriteLine(Localization.Core.Xbox_Physical_Format_Information_contained_in_image_WithMarkup);
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
                        AaruConsole.WriteLine(Localization.Core.Xbox_DMI_contained_in_image_WithMarkup);
                        AaruConsole.Write("{0}", DMI.PrettifyXbox(xmi));
                        AaruConsole.WriteLine();
                    }
                }

                if(DMI.IsXbox360(xdmi))
                {
                    DMI.Xbox360DMI? xmi = DMI.DecodeXbox360(xdmi);

                    if(xmi.HasValue)
                    {
                        AaruConsole.WriteLine(Localization.Core.Xbox_360_DMI_contained_in_image_WithMarkup);
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
                AaruConsole.WriteLine(Localization.Core.Xbox_Security_Sectors_contained_in_image_WithMarkup);
                AaruConsole.Write("{0}", SS.Prettify(toc));
                AaruConsole.WriteLine();
            }
        }

        if(imageFormat is IFluxImage fluxImage)
            AaruConsole.WriteLine(Localization.Core.Image_flux_captures);

        if(imageFormat is not IOpticalMediaImage opticalImage)
            return;

        try
        {
            if(opticalImage.Sessions is { Count: > 0 })
            {
                var table = new Table
                {
                    Title = new TableTitle(Localization.Core.Title_Image_sessions)
                };

                table.AddColumn(Localization.Core.Title_Session);
                table.AddColumn(Localization.Core.Title_First_track);
                table.AddColumn(Localization.Core.Title_Last_track);
                table.AddColumn(Localization.Core.Title_Start);
                table.AddColumn(Localization.Core.Title_End);

                foreach(Session session in opticalImage.Sessions)
                    table.AddRow(session.Sequence.ToString(), session.StartTrack.ToString(),
                                 session.EndTrack.ToString(), session.StartSector.ToString(),
                                 session.EndSector.ToString());

                AnsiConsole.Write(table);
                AaruConsole.WriteLine();
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            if(opticalImage.Tracks is not { Count: > 0 })
                return;

            var table = new Table
            {
                Title = new TableTitle(Localization.Core.Title_Image_tracks)
            };

            table.AddColumn(Localization.Core.Title_Track);
            table.AddColumn(Localization.Core.Title_Type_for_media);
            table.AddColumn(Localization.Core.Title_Bps);
            table.AddColumn(Localization.Core.Title_Raw_bps);
            table.AddColumn(Localization.Core.Title_Subchannel);
            table.AddColumn(Localization.Core.Title_Pregap);
            table.AddColumn(Localization.Core.Title_Start);
            table.AddColumn(Localization.Core.Title_End);

            foreach(Track track in opticalImage.Tracks)
                table.AddRow(track.Sequence.ToString(), track.Type.ToString(), track.BytesPerSector.ToString(),
                             track.RawBytesPerSector.ToString(), track.SubchannelType.ToString(),
                             track.Pregap.ToString(), track.StartSector.ToString(), track.EndSector.ToString());

            AnsiConsole.Write(table);

            if(!opticalImage.Tracks.Any(t => t.Indexes.Any()))
                return;

            AaruConsole.WriteLine();

            table = new Table
            {
                Title = new TableTitle(Localization.Core.Title_Image_indexes)
            };

            table.AddColumn(Localization.Core.Title_Track);
            table.AddColumn(Localization.Core.Title_Index);
            table.AddColumn(Localization.Core.Title_Start);

            foreach(Track track in opticalImage.Tracks)
                foreach(KeyValuePair<ushort, int> index in track.Indexes)
                    table.AddRow(track.Sequence.ToString(), index.Key.ToString(), index.Value.ToString());

            AnsiConsole.Write(table);
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
}