// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes ATA IDENTIFY DEVICE response.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Localization;

namespace Aaru.Decoders.ATA;

// Information from following standards:
// T10-791D rev. 4c (ATA)
// T10-948D rev. 4c (ATA-2)
// T13-1153D rev. 18 (ATA/ATAPI-4)
// T13-1321D rev. 3 (ATA/ATAPI-5)
// T13-1410D rev. 3b (ATA/ATAPI-6)
// T13-1532D rev. 4b (ATA/ATAPI-7)
// T13-1699D rev. 3f (ATA8-ACS)
// T13-1699D rev. 4a (ATA8-ACS)
// T13-2015D rev. 2 (ACS-2)
// T13-2161D rev. 5 (ACS-3)
// CF+ & CF Specification rev. 1.4 (CFA)
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Identify
{
    public static string Prettify(byte[] IdentifyDeviceResponse)
    {
        if(IdentifyDeviceResponse.Length != 512) return null;

        CommonTypes.Structs.Devices.ATA.Identify.IdentifyDevice? decoded =
            CommonTypes.Structs.Devices.ATA.Identify.Decode(IdentifyDeviceResponse);

        return Prettify(decoded);
    }

    public static string Prettify(CommonTypes.Structs.Devices.ATA.Identify.IdentifyDevice? IdentifyDeviceResponse)
    {
        if(IdentifyDeviceResponse == null) return null;

        var sb = new StringBuilder();

        var atapi = false;
        var cfa   = false;

        CommonTypes.Structs.Devices.ATA.Identify.IdentifyDevice ATAID = IdentifyDeviceResponse.Value;

        if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                         .NonMagnetic))
        {
            if((ushort)ATAID.GeneralConfiguration != 0x848A)
                atapi = true;
            else
                cfa = true;
        }

        if(atapi)
            sb.AppendLine(Localization.ATAPI_device);
        else if(cfa)
            sb.AppendLine(Localization.CompactFlash_device);
        else
            sb.AppendLine(Localization.ATA_device);

        if(ATAID.Model != "") sb.AppendFormat(Core.Model_0, ATAID.Model).AppendLine();

        if(ATAID.FirmwareRevision != "") sb.AppendFormat(Core.Firmware_revision_0, ATAID.FirmwareRevision).AppendLine();

        if(ATAID.SerialNumber != "") sb.AppendFormat(Core.Serial_number_0, ATAID.SerialNumber).AppendLine();

        if(ATAID.AdditionalPID != "")
            sb.AppendFormat(Localization.Additional_product_ID_0, ATAID.AdditionalPID).AppendLine();

        if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeSet) &&
           !ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeClear))
        {
            if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MediaSerial))
            {
                if(ATAID.MediaManufacturer != "")
                    sb.AppendFormat(Core.Media_manufacturer_0, ATAID.MediaManufacturer).AppendLine();

                if(ATAID.MediaSerial != "") sb.AppendFormat(Core.Media_serial_number_0, ATAID.MediaSerial).AppendLine();
            }

            if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.WWN))
                sb.AppendFormat(Localization.World_Wide_Name_0, ATAID.WWN).AppendLine();
        }

        bool ata1 = false,
             ata2 = false,
             ata3 = false,
             ata4 = false,
             ata5 = false,
             ata6 = false,
             ata7 = false,
             acs  = false,
             acs2 = false,
             acs3 = false,
             acs4 = false;

        if((ushort)ATAID.MajorVersion == 0x0000 || (ushort)ATAID.MajorVersion == 0xFFFF)
        {
            // Obsolete in ATA-2, if present, device supports ATA-1
            ata1 |=
                ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                              .FastIDE) ||
                ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                              .SlowIDE) ||
                ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                              .UltraFastIDE);

            ata2 |= ATAID.ExtendedIdentify.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.ExtendedIdentifyBit
                                                              .Words64to70Valid);

            if(!ata1 && !ata2 && !atapi && !cfa) ata2 = true;

            ata4 |= atapi;
            ata3 |= cfa;

            if(cfa && ata1) ata1 = false;

            if(cfa && ata2) ata2 = false;

            ata5 |= ATAID.Signature == 0xA5;
        }
        else
        {
            ata1 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.Ata1);
            ata2 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.Ata2);
            ata3 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.Ata3);
            ata4 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.AtaAtapi4);
            ata5 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.AtaAtapi5);
            ata6 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.AtaAtapi6);
            ata7 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.AtaAtapi7);
            acs  |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.Ata8ACS);
            acs2 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.ACS2);
            acs3 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.ACS3);
            acs4 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.ACS4);
        }

        var maxatalevel = 0;
        var minatalevel = 255;
        sb.Append(Localization.Supported_ATA_versions);

        if(ata1)
        {
            sb.Append("ATA-1 ");
            maxatalevel = 1;
            minatalevel = 1;
        }

        if(ata2)
        {
            sb.Append("ATA-2 ");
            maxatalevel = 2;

            if(minatalevel > 2) minatalevel = 2;
        }

        if(ata3)
        {
            sb.Append("ATA-3 ");
            maxatalevel = 3;

            if(minatalevel > 3) minatalevel = 3;
        }

        if(ata4)
        {
            sb.Append("ATA/ATAPI-4 ");
            maxatalevel = 4;

            if(minatalevel > 4) minatalevel = 4;
        }

        if(ata5)
        {
            sb.Append("ATA/ATAPI-5 ");
            maxatalevel = 5;

            if(minatalevel > 5) minatalevel = 5;
        }

        if(ata6)
        {
            sb.Append("ATA/ATAPI-6 ");
            maxatalevel = 6;

            if(minatalevel > 6) minatalevel = 6;
        }

        if(ata7)
        {
            sb.Append("ATA/ATAPI-7 ");
            maxatalevel = 7;

            if(minatalevel > 7) minatalevel = 7;
        }

        if(acs)
        {
            sb.Append("ATA8-ACS ");
            maxatalevel = 8;

            if(minatalevel > 8) minatalevel = 8;
        }

        if(acs2)
        {
            sb.Append("ATA8-ACS2 ");
            maxatalevel = 9;

            if(minatalevel > 9) minatalevel = 9;
        }

        if(acs3)
        {
            sb.Append("ATA8-ACS3 ");
            maxatalevel = 10;

            if(minatalevel > 10) minatalevel = 10;
        }

        if(acs4)
        {
            sb.Append("ATA8-ACS4 ");
            maxatalevel = 11;

            if(minatalevel > 11) minatalevel = 11;
        }

        sb.AppendLine();

        sb.Append(Localization.Maximum_ATA_revision_supported);

        if(maxatalevel >= 3)
        {
            switch(ATAID.MinorVersion)
            {
                case 0x0000:
                case 0xFFFF:
                    sb.AppendLine(Localization.Minor_ATA_version_not_specified);

                    break;
                case 0x0001:
                    sb.AppendLine(Localization.ATA_ATA_1_X3T9_2_781D_prior_to_revision_4);

                    break;
                case 0x0002:
                    sb.AppendLine(Localization.ATA_1_published_ANSI_X3_221_1994);

                    break;
                case 0x0003:
                    sb.AppendLine(Localization.ATA_ATA_1_X3T9_2_781D_revision_4);

                    break;
                case 0x0004:
                    sb.AppendLine(Localization.ATA_2_published_ANSI_X3_279_1996);

                    break;
                case 0x0005:
                    sb.AppendLine(Localization.ATA_2_X3T10_948D_prior_to_revision_2k);

                    break;
                case 0x0006:
                    sb.AppendLine(Localization.ATA_3_X3T10_2008D_revision_1);

                    break;
                case 0x0007:
                    sb.AppendLine(Localization.ATA_2_X3T10_948D_revision_2k);

                    break;
                case 0x0008:
                    sb.AppendLine(Localization.ATA_3_X3T10_2008D_revision_0);

                    break;
                case 0x0009:
                    sb.AppendLine(Localization.ATA_2_X3T10_948D_revision_3);

                    break;
                case 0x000A:
                    sb.AppendLine(Localization.ATA_3_published_ANSI_X3_298_1997);

                    break;
                case 0x000B:
                    sb.AppendLine(Localization.ATA_3_X3T10_2008D_revision_6);

                    break;
                case 0x000C:
                    sb.AppendLine(Localization.ATA_3_X3T13_2008D_revision_7);

                    break;
                case 0x000D:
                    sb.AppendLine(Localization.ATA_ATAPI_4_X3T13_1153D_revision_6);

                    break;
                case 0x000E:
                    sb.AppendLine(Localization.ATA_ATAPI_4_T13_1153D_revision_13);

                    break;
                case 0x000F:
                    sb.AppendLine(Localization.ATA_ATAPI_4_X3T13_1153D_revision_7);

                    break;
                case 0x0010:
                    sb.AppendLine(Localization.ATA_ATAPI_4_T13_1153D_revision_18);

                    break;
                case 0x0011:
                    sb.AppendLine(Localization.ATA_ATAPI_4_T13_1153D_revision_15);

                    break;
                case 0x0012:
                    sb.AppendLine(Localization.ATA_ATAPI_4_published_ANSI_INCITS_317_1998);

                    break;
                case 0x0013:
                    sb.AppendLine(Localization.ATA_ATAPI_5_T13_1321D_revision_3);

                    break;
                case 0x0014:
                    sb.AppendLine(Localization.ATA_ATAPI_4_T13_1153D_revision_14);

                    break;
                case 0x0015:
                    sb.AppendLine(Localization.ATA_ATAPI_5_T13_1321D_revision_1);

                    break;
                case 0x0016:
                    sb.AppendLine(Localization.ATA_ATAPI_5_published_ANSI_INCITS_340_2000);

                    break;
                case 0x0017:
                    sb.AppendLine(Localization.ATA_ATAPI_4_T13_1153D_revision_17);

                    break;
                case 0x0018:
                    sb.AppendLine(Localization.ATA_ATAPI_6_T13_1410D_revision_0);

                    break;
                case 0x0019:
                    sb.AppendLine(Localization.ATA_ATAPI_6_T13_1410D_revision_3a);

                    break;
                case 0x001A:
                    sb.AppendLine(Localization.ATA_ATAPI_7_T13_1532D_revision_1);

                    break;
                case 0x001B:
                    sb.AppendLine(Localization.ATA_ATAPI_6_T13_1410D_revision_2);

                    break;
                case 0x001C:
                    sb.AppendLine(Localization.ATA_ATAPI_6_T13_1410D_revision_1);

                    break;
                case 0x001D:
                    sb.AppendLine(Localization.ATA_ATAPI_7_published_ANSI_INCITS_397_2005);

                    break;
                case 0x001E:
                    sb.AppendLine(Localization.ATA_ATAPI_7_T13_1532D_revision_0);

                    break;
                case 0x001F:
                    sb.AppendLine(Localization.ACS_3_Revision_3b);

                    break;
                case 0x0021:
                    sb.AppendLine(Localization.ATA_ATAPI_7_T13_1532D_revision_4a);

                    break;
                case 0x0022:
                    sb.AppendLine(Localization.ATA_ATAPI_6_published_ANSI_INCITS_361_2002);

                    break;
                case 0x0027:
                    sb.AppendLine(Localization.ATA8_ACS_revision_3c);

                    break;
                case 0x0028:
                    sb.AppendLine(Localization.ATA8_ACS_revision_6);

                    break;
                case 0x0029:
                    sb.AppendLine(Localization.ATA8_ACS_revision_4);

                    break;
                case 0x0031:
                    sb.AppendLine(Localization.ACS_2_Revision_2);

                    break;
                case 0x0033:
                    sb.AppendLine(Localization.ATA8_ACS_Revision_3e);

                    break;
                case 0x0039:
                    sb.AppendLine(Localization.ATA8_ACS_Revision_4c);

                    break;
                case 0x0042:
                    sb.AppendLine(Localization.ATA8_ACS_Revision_3f);

                    break;
                case 0x0052:
                    sb.AppendLine(Localization.ATA8_ACS_revision_3b);

                    break;
                case 0x006D:
                    sb.AppendLine(Localization.ACS_3_Revision_5);

                    break;
                case 0x0082:
                    sb.AppendLine(Localization.ACS_2_published_ANSI_INCITS_482_2012);

                    break;
                case 0x0107:
                    sb.AppendLine(Localization.ATA8_ACS_revision_2d);

                    break;
                case 0x0110:
                    sb.AppendLine(Localization.ACS_2_Revision_3);

                    break;
                case 0x011B:
                    sb.AppendLine(Localization.ACS_3_Revision_4);

                    break;
                default:
                    sb.AppendFormat(Localization.Unknown_ATA_revision_0, ATAID.MinorVersion).AppendLine();

                    break;
            }
        }

        switch((ATAID.TransportMajorVersion & 0xF000) >> 12)
        {
            case 0x0:
                sb.Append(Localization.Parallel_ATA_device);

                if((ATAID.TransportMajorVersion & 0x0002) == 0x0002) sb.Append("ATA/ATAPI-7 ");

                if((ATAID.TransportMajorVersion & 0x0001) == 0x0001) sb.Append("ATA8-APT ");

                sb.AppendLine();

                break;
            case 0x1:
                sb.Append(Localization.Serial_ATA_device);

                if((ATAID.TransportMajorVersion & 0x0001) == 0x0001) sb.Append("ATA8-AST ");

                if((ATAID.TransportMajorVersion & 0x0002) == 0x0002) sb.Append("SATA 1.0a ");

                if((ATAID.TransportMajorVersion & 0x0004) == 0x0004) sb.Append("SATA II Extensions ");

                if((ATAID.TransportMajorVersion & 0x0008) == 0x0008) sb.Append("SATA 2.5 ");

                if((ATAID.TransportMajorVersion & 0x0010) == 0x0010) sb.Append("SATA 2.6 ");

                if((ATAID.TransportMajorVersion & 0x0020) == 0x0020) sb.Append("SATA 3.0 ");

                if((ATAID.TransportMajorVersion & 0x0040) == 0x0040) sb.Append("SATA 3.1 ");

                sb.AppendLine();

                break;
            case 0xE:
                sb.AppendLine(Localization.SATA_Express_device);

                break;
            default:
                sb.AppendFormat(Localization.Unknown_transport_type_0, (ATAID.TransportMajorVersion & 0xF000) >> 12)
                  .AppendLine();

                break;
        }

        if(atapi)
        {
            // Bits 12 to 8, SCSI Peripheral Device Type
            switch((PeripheralDeviceTypes)(((ushort)ATAID.GeneralConfiguration & 0x1F00) >> 8))
            {
                case PeripheralDeviceTypes.DirectAccess: //0x00,
                    sb.AppendLine(Localization.ATAPI_Direct_access_device);

                    break;
                case PeripheralDeviceTypes.SequentialAccess: //0x01,
                    sb.AppendLine(Localization.ATAPI_Sequential_access_device);

                    break;
                case PeripheralDeviceTypes.PrinterDevice: //0x02,
                    sb.AppendLine(Localization.ATAPI_Printer_device);

                    break;
                case PeripheralDeviceTypes.ProcessorDevice: //0x03,
                    sb.AppendLine(Localization.ATAPI_Processor_device);

                    break;
                case PeripheralDeviceTypes.WriteOnceDevice: //0x04,
                    sb.AppendLine(Localization.ATAPI_Write_once_device);

                    break;
                case PeripheralDeviceTypes.MultiMediaDevice: //0x05,
                    sb.AppendLine(Localization.ATAPI_CD_ROM_DVD_etc_device);

                    break;
                case PeripheralDeviceTypes.ScannerDevice: //0x06,
                    sb.AppendLine(Localization.ATAPI_Scanner_device);

                    break;
                case PeripheralDeviceTypes.OpticalDevice: //0x07,
                    sb.AppendLine(Localization.ATAPI_Optical_memory_device);

                    break;
                case PeripheralDeviceTypes.MediumChangerDevice: //0x08,
                    sb.AppendLine(Localization.ATAPI_Medium_change_device);

                    break;
                case PeripheralDeviceTypes.CommsDevice: //0x09,
                    sb.AppendLine(Localization.ATAPI_Communications_device);

                    break;
                case PeripheralDeviceTypes.PrePressDevice1: //0x0A,
                    sb.AppendLine(Localization.ATAPI_Graphics_arts_pre_press_device_defined_in_ASC_IT8);

                    break;
                case PeripheralDeviceTypes.PrePressDevice2: //0x0B,
                    sb.AppendLine(Localization.ATAPI_Graphics_arts_pre_press_device_defined_in_ASC_IT8);

                    break;
                case PeripheralDeviceTypes.ArrayControllerDevice: //0x0C,
                    sb.AppendLine(Localization.ATAPI_Array_controller_device);

                    break;
                case PeripheralDeviceTypes.EnclosureServiceDevice: //0x0D,
                    sb.AppendLine(Localization.ATAPI_Enclosure_services_device);

                    break;
                case PeripheralDeviceTypes.SimplifiedDevice: //0x0E,
                    sb.AppendLine(Localization.ATAPI_Simplified_direct_access_device);

                    break;
                case PeripheralDeviceTypes.OCRWDevice: //0x0F,
                    sb.AppendLine(Localization.ATAPI_Optical_card_reader_writer_device);

                    break;
                case PeripheralDeviceTypes.BridgingExpander: //0x10,
                    sb.AppendLine(Localization.ATAPI_Bridging_Expanders);

                    break;
                case PeripheralDeviceTypes.ObjectDevice: //0x11,
                    sb.AppendLine(Localization.ATAPI_Object_based_Storage_Device);

                    break;
                case PeripheralDeviceTypes.ADCDevice: //0x12,
                    sb.AppendLine(Localization.ATAPI_Automation_Drive_Interface);

                    break;
                case PeripheralDeviceTypes.WellKnownDevice: //0x1E,
                    sb.AppendLine(Localization.ATAPI_Well_known_logical_unit);

                    break;
                case PeripheralDeviceTypes.UnknownDevice: //0x1F
                    sb.AppendLine(Localization.ATAPI_Unknown_or_no_device_type);

                    break;
                default:
                    sb.AppendFormat(Localization.ATAPI_Unknown_device_type_field_value_0,
                                    ((ushort)ATAID.GeneralConfiguration & 0x1F00) >> 8)
                      .AppendLine();

                    break;
            }

            // ATAPI DRQ behaviour
            switch(((ushort)ATAID.GeneralConfiguration & 0x60) >> 5)
            {
                case 0:
                    sb.AppendLine(Localization.Device_shall_set_DRQ_within_3_ms_of_receiving_PACKET);

                    break;
                case 1:
                    sb.AppendLine(Localization.Device_shall_assert_INTRQ_when_DRQ_is_set_to_one);

                    break;
                case 2:
                    sb.AppendLine(Localization.Device_shall_set_DRQ_within_50_µs_of_receiving_PACKET);

                    break;
                default:
                    sb.AppendFormat(Localization.Unknown_ATAPI_DRQ_behaviour_code_0,
                                    ((ushort)ATAID.GeneralConfiguration & 0x60) >> 5)
                      .AppendLine();

                    break;
            }

            // ATAPI PACKET size
            switch((ushort)ATAID.GeneralConfiguration & 0x03)
            {
                case 0:
                    sb.AppendLine(Localization.ATAPI_device_uses_12_byte_command_packet);

                    break;
                case 1:
                    sb.AppendLine(Localization.ATAPI_device_uses_16_byte_command_packet);

                    break;
                default:
                    sb.AppendFormat(Localization.Unknown_ATAPI_packet_size_code_0,
                                    (ushort)ATAID.GeneralConfiguration & 0x03)
                      .AppendLine();

                    break;
            }
        }
        else if(!cfa)
        {
            if(minatalevel >= 5)
            {
                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .IncompleteResponse))
                    sb.AppendLine(Localization.Incomplete_identify_response);
            }

            if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                             .NonMagnetic))
                sb.AppendLine(Localization.Device_uses_non_magnetic_media);

            if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                             .Removable))
                sb.AppendLine(Localization.Device_is_removable);

            if(minatalevel <= 5)
            {
                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .Fixed))
                    sb.AppendLine(Localization.Device_is_fixed);
            }

            if(ata1)
            {
                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .SlowIDE))
                    sb.AppendLine(Localization.Device_transfer_rate_less_than_5_Mbs);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .FastIDE))
                    sb.AppendLine(Localization.Device_transfer_rate_is_more_5_Mbs_less_10_Mbs);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .UltraFastIDE))
                    sb.AppendLine(Localization.Device_transfer_rate_more_than_10_Mbs);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .SoftSector))
                    sb.AppendLine(Localization.Device_is_soft_sectored);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .HardSector))
                    sb.AppendLine(Localization.Device_is_hard_sectored);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .NotMFM))
                    sb.AppendLine(Localization.Device_is_not_MFM_encoded);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .FormatGapReq))
                    sb.AppendLine(Localization.Format_speed_tolerance_gap_is_required);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .TrackOffset))
                    sb.AppendLine(Localization.Track_offset_option_is_available);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .DataStrobeOffset))
                    sb.AppendLine(Localization.Data_strobe_offset_option_is_available);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .RotationalSpeedTolerance))
                    sb.AppendLine(Localization.Rotational_speed_tolerance_is_higher_than_0_5_percent);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .SpindleControl))
                    sb.AppendLine(Localization.Spindle_motor_control_is_implemented);

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit
                                                                 .HighHeadSwitch))
                    sb.AppendLine(Localization.Head_switch_time_is_bigger_than_15_µs);
            }
        }

        if(ATAID.NominalRotationRate != 0x0000 && ATAID.NominalRotationRate != 0xFFFF)
        {
            if(ATAID.NominalRotationRate == 0x0001)
                sb.AppendLine(Localization.Device_does_not_rotate);
            else
                sb.AppendFormat(Localization.Device_rotates_at_0_rpm, ATAID.NominalRotationRate).AppendLine();
        }

        uint logicalSectorSize = 0;

        if(!atapi)
        {
            uint physicalSectorSize;

            if((ATAID.PhysLogSectorSize & 0x8000) == 0x0000 && (ATAID.PhysLogSectorSize & 0x4000) == 0x4000)
            {
                if((ATAID.PhysLogSectorSize & 0x1000) == 0x1000)
                {
                    if(ATAID.LogicalSectorWords <= 255 || ATAID.LogicalAlignment == 0xFFFF)
                        logicalSectorSize = 512;
                    else
                        logicalSectorSize = ATAID.LogicalSectorWords * 2;
                }
                else
                    logicalSectorSize = 512;

                if((ATAID.PhysLogSectorSize & 0x2000) == 0x2000)
                    physicalSectorSize = logicalSectorSize * (uint)Math.Pow(2, ATAID.PhysLogSectorSize & 0xF);
                else
                    physicalSectorSize = logicalSectorSize;
            }
            else
            {
                logicalSectorSize  = 512;
                physicalSectorSize = 512;
            }

            sb.AppendFormat(Localization.Physical_sector_size_0_bytes, physicalSectorSize).AppendLine();
            sb.AppendFormat(Localization.Logical_sector_size_0_bytes,  logicalSectorSize).AppendLine();

            if(logicalSectorSize                 != physicalSectorSize &&
               (ATAID.LogicalAlignment & 0x8000) == 0x0000             &&
               (ATAID.LogicalAlignment & 0x4000) == 0x4000)
            {
                sb.AppendFormat(Localization.Logical_sector_starts_at_offset_0_from_physical_sector,
                                ATAID.LogicalAlignment & 0x3FFF)
                  .AppendLine();
            }

            if(minatalevel <= 5)
            {
                if(ATAID.CurrentCylinders > 0 && ATAID is { CurrentHeads: > 0, CurrentSectorsPerTrack: > 0 })
                {
                    sb.AppendFormat(Localization.Cylinders_0_max_1_current, ATAID.Cylinders, ATAID.CurrentCylinders)
                      .AppendLine();

                    sb.AppendFormat(Localization.Heads_0_max_1_current, ATAID.Heads, ATAID.CurrentHeads).AppendLine();

                    sb.AppendFormat(Localization.Sectors_per_track_0_max_1_current,
                                    ATAID.SectorsPerTrack,
                                    ATAID.CurrentSectorsPerTrack)
                      .AppendLine();

                    sb.AppendFormat(Localization.Sectors_addressable_in_CHS_mode_0_max_1_current,
                                    ATAID.Cylinders * ATAID.Heads * ATAID.SectorsPerTrack,
                                    ATAID.CurrentSectors)
                      .AppendLine();
                }
                else
                {
                    sb.AppendFormat(Localization.Cylinders_0,         ATAID.Cylinders).AppendLine();
                    sb.AppendFormat(Localization.Heads_0,             ATAID.Heads).AppendLine();
                    sb.AppendFormat(Localization.Sectors_per_track_0, ATAID.SectorsPerTrack).AppendLine();

                    sb.AppendFormat(Localization.Sectors_addressable_in_CHS_mode_0,
                                    ATAID.Cylinders * ATAID.Heads * ATAID.SectorsPerTrack)
                      .AppendLine();
                }
            }

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.LBASupport))
                sb.AppendFormat(Localization._0_sectors_in_28_bit_LBA_mode, ATAID.LBASectors).AppendLine();

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.LBA48))
                sb.AppendFormat(Localization._0_sectors_in_48_bit_LBA_mode, ATAID.LBA48Sectors).AppendLine();

            if(minatalevel <= 5)
            {
                if(ATAID.CurrentSectors > 0)
                {
                    sb.AppendFormat(Localization.Device_size_in_CHS_mode_0_bytes_1_Mb_2_MiB,
                                    (ulong)ATAID.CurrentSectors                     * logicalSectorSize,
                                    (ulong)ATAID.CurrentSectors * logicalSectorSize / 1000 / 1000,
                                    (ulong)ATAID.CurrentSectors * 512               / 1024 / 1024)
                      .AppendLine();
                }
                else
                {
                    var currentSectors = (ulong)(ATAID.Cylinders * ATAID.Heads * ATAID.SectorsPerTrack);

                    sb.AppendFormat(Localization.Device_size_in_CHS_mode_0_bytes_1_Mb_2_MiB,
                                    currentSectors                     * logicalSectorSize,
                                    currentSectors * logicalSectorSize / 1000 / 1000,
                                    currentSectors * 512               / 1024 / 1024)
                      .AppendLine();
                }
            }

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.LBASupport))
            {
                switch((ulong)ATAID.LBASectors * logicalSectorSize / 1024 / 1024)
                {
                    case > 1000000:
                        sb.AppendFormat(Localization.Device_size_in_28_bit_LBA_mode_0_bytes_1_Tb_2_TiB,
                                        (ulong)ATAID.LBASectors                     * logicalSectorSize,
                                        (ulong)ATAID.LBASectors * logicalSectorSize / 1000 / 1000 / 1000 / 1000,
                                        (ulong)ATAID.LBASectors * 512               / 1024 / 1024 / 1024 / 1024)
                          .AppendLine();

                        break;
                    case > 1000:
                        sb.AppendFormat(Localization.Device_size_in_28_bit_LBA_mode_0_bytes_1_Gb_2_GiB,
                                        (ulong)ATAID.LBASectors                     * logicalSectorSize,
                                        (ulong)ATAID.LBASectors * logicalSectorSize / 1000 / 1000 / 1000,
                                        (ulong)ATAID.LBASectors * 512               / 1024 / 1024 / 1024)
                          .AppendLine();

                        break;
                    default:
                        sb.AppendFormat(Localization.Device_size_in_28_bit_LBA_mode_0_bytes_1_Mb_2_MiB,
                                        (ulong)ATAID.LBASectors                     * logicalSectorSize,
                                        (ulong)ATAID.LBASectors * logicalSectorSize / 1000 / 1000,
                                        (ulong)ATAID.LBASectors * 512               / 1024 / 1024)
                          .AppendLine();

                        break;
                }
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.LBA48))
            {
                if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ExtSectors))
                {
                    switch(ATAID.ExtendedUserSectors * logicalSectorSize / 1024 / 1024)
                    {
                        case > 1000000:
                            sb.AppendFormat(Localization.Device_size_in_48_bit_LBA_mode_0_bytes_1_Tb_2_TiB,
                                            ATAID.ExtendedUserSectors                     * logicalSectorSize,
                                            ATAID.ExtendedUserSectors * logicalSectorSize / 1000 / 1000 / 1000 / 1000,
                                            ATAID.ExtendedUserSectors * logicalSectorSize / 1024 / 1024 / 1024 / 1024)
                              .AppendLine();

                            break;
                        case > 1000:
                            sb.AppendFormat(Localization.Device_size_in_48_bit_LBA_mode_0_bytes_1_Gb_2_GiB,
                                            ATAID.ExtendedUserSectors                     * logicalSectorSize,
                                            ATAID.ExtendedUserSectors * logicalSectorSize / 1000 / 1000 / 1000,
                                            ATAID.ExtendedUserSectors * logicalSectorSize / 1024 / 1024 / 1024)
                              .AppendLine();

                            break;
                        default:
                            sb.AppendFormat(Localization.Device_size_in_48_bit_LBA_mode_0_bytes_1_Mb_2_MiB,
                                            ATAID.ExtendedUserSectors                     * logicalSectorSize,
                                            ATAID.ExtendedUserSectors * logicalSectorSize / 1000 / 1000,
                                            ATAID.ExtendedUserSectors * logicalSectorSize / 1024 / 1024)
                              .AppendLine();

                            break;
                    }
                }
                else
                {
                    switch(ATAID.LBA48Sectors * logicalSectorSize / 1024 / 1024)
                    {
                        case > 1000000:
                            sb.AppendFormat(Localization.Device_size_in_48_bit_LBA_mode_0_bytes_1_Tb_2_TiB,
                                            ATAID.LBA48Sectors                     * logicalSectorSize,
                                            ATAID.LBA48Sectors * logicalSectorSize / 1000 / 1000 / 1000 / 1000,
                                            ATAID.LBA48Sectors * logicalSectorSize / 1024 / 1024 / 1024 / 1024)
                              .AppendLine();

                            break;
                        case > 1000:
                            sb.AppendFormat(Localization.Device_size_in_48_bit_LBA_mode_0_bytes_1_Gb_2_GiB,
                                            ATAID.LBA48Sectors                     * logicalSectorSize,
                                            ATAID.LBA48Sectors * logicalSectorSize / 1000 / 1000 / 1000,
                                            ATAID.LBA48Sectors * logicalSectorSize / 1024 / 1024 / 1024)
                              .AppendLine();

                            break;
                        default:
                            sb.AppendFormat(Localization.Device_size_in_48_bit_LBA_mode_0_bytes_1_Mb_2_MiB,
                                            ATAID.LBA48Sectors                     * logicalSectorSize,
                                            ATAID.LBA48Sectors * logicalSectorSize / 1000 / 1000,
                                            ATAID.LBA48Sectors * logicalSectorSize / 1024 / 1024)
                              .AppendLine();

                            break;
                    }
                }
            }

            if(ata1 || cfa)
            {
                if(cfa) sb.AppendFormat(Localization._0_sectors_in_card, ATAID.SectorsPerCard).AppendLine();

                if(ATAID.UnformattedBPT > 0)
                    sb.AppendFormat(Localization._0_bytes_per_unformatted_track, ATAID.UnformattedBPT).AppendLine();

                if(ATAID.UnformattedBPS > 0)
                    sb.AppendFormat(Localization._0_bytes_per_unformatted_sector, ATAID.UnformattedBPS).AppendLine();
            }
        }

        if((ushort)ATAID.SpecificConfiguration != 0x0000 && (ushort)ATAID.SpecificConfiguration != 0xFFFF)
        {
            switch(ATAID.SpecificConfiguration)
            {
                case CommonTypes.Structs.Devices.ATA.Identify.SpecificConfigurationEnum.RequiresSetIncompleteResponse:
                    sb.AppendLine(Localization
                                     .Device_requires_SET_FEATURES_to_spin_up_and_IDENTIFY_DEVICE_response_is_incomplete);

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.SpecificConfigurationEnum.RequiresSetCompleteResponse:
                    sb.AppendLine(Localization
                                     .Device_requires_SET_FEATURES_to_spin_up_and_IDENTIFY_DEVICE_response_is_complete);

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.SpecificConfigurationEnum
                                .NotRequiresSetIncompleteResponse:
                    sb.AppendLine(Localization
                                     .Device_does_not_require_SET_FEATURES_to_spin_up_and_IDENTIFY_DEVICE_response_is_incomplete);

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.SpecificConfigurationEnum.NotRequiresSetCompleteResponse:
                    sb.AppendLine(Localization
                                     .Device_does_not_require_SET_FEATURES_to_spin_up_and_IDENTIFY_DEVICE_response_is_complete);

                    break;
                default:
                    sb.AppendFormat(Localization.Unknown_device_specific_configuration_0,
                                    (ushort)ATAID.SpecificConfiguration)
                      .AppendLine();

                    break;
            }
        }

        // Obsolete since ATA-2, however, it is yet used in ATA-8 devices
        if(ATAID.BufferSize != 0x0000 &&
           ATAID.BufferSize != 0xFFFF &&
           ATAID.BufferType != 0x0000 &&
           ATAID.BufferType != 0xFFFF)
        {
            switch(ATAID.BufferType)
            {
                case 1:
                    sb.AppendFormat(Localization._0_KiB_of_single_ported_single_sector_buffer,
                                    ATAID.BufferSize * 512 / 1024)
                      .AppendLine();

                    break;
                case 2:
                    sb.AppendFormat(Localization._0_KiB_of_dual_ported_multi_sector_buffer,
                                    ATAID.BufferSize * 512 / 1024)
                      .AppendLine();

                    break;
                case 3:
                    sb.AppendFormat(Localization._0_KiB_of_dual_ported_multi_sector_buffer_with_read_caching,
                                    ATAID.BufferSize * 512 / 1024)
                      .AppendLine();

                    break;
                default:
                    sb.AppendFormat(Localization._0_KiB_of_unknown_type_1_buffer,
                                    ATAID.BufferSize * 512 / 1024,
                                    ATAID.BufferType)
                      .AppendLine();

                    break;
            }
        }

        if(ATAID.EccBytes != 0x0000 && ATAID.EccBytes != 0xFFFF)
            sb.AppendFormat(Localization.READ_WRITE_LONG_has_0_extra_bytes, ATAID.EccBytes).AppendLine();

        sb.AppendLine();

        sb.Append(Localization.Device_capabilities);

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.StandardStandbyTimer))
            sb.AppendLine().Append(Localization.Standby_time_values_are_standard);

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.IORDY))
        {
            sb.AppendLine()
              .Append(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit
                                                            .CanDisableIORDY)
                          ? Localization.IORDY_is_supported_and_can_be_disabled
                          : Localization.IORDY_is_supported);
        }

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.DMASupport))
            sb.AppendLine().Append(Localization.DMA_is_supported);

        if(ATAID.Capabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit2.MustBeSet) &&
           !ATAID.Capabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit2.MustBeClear))
        {
            if(ATAID.Capabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit2
                                                      .SpecificStandbyTimer))
                sb.AppendLine().Append(Localization.Device_indicates_a_specific_minimum_standby_timer_value);
        }

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.MultipleValid))
        {
            sb.AppendLine()
              .AppendFormat(Localization.A_maximum_of_0_sectors_can_be_transferred_per_interrupt_on_READ_WRITE_MULTIPLE,
                            ATAID.MultipleSectorNumber);

            sb.AppendLine()
              .AppendFormat(Localization.Device_supports_setting_a_maximum_of_0_sectors, ATAID.MultipleMaxSectors);
        }

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.PhysicalAlignment1) ||
           ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.PhysicalAlignment0))
        {
            sb.AppendLine()
              .AppendFormat(Localization.Long_Physical_Alignment_setting_is_0, (ushort)ATAID.Capabilities & 0x03);
        }

        if(ata1)
        {
            if(ATAID.TrustedComputing.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TrustedComputingBit
                                                         .TrustedComputing))
                sb.AppendLine().Append(Localization.Device_supports_doubleword_IO);
        }

        if(atapi)
        {
            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.InterleavedDMA))
                sb.AppendLine().Append(Localization.ATAPI_device_supports_interleaved_DMA);

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.CommandQueue))
                sb.AppendLine().Append(Localization.ATAPI_device_supports_command_queueing);

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.OverlapOperation))
                sb.AppendLine().Append(Localization.ATAPI_device_supports_overlapped_operations);

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit
                                                     .RequiresATASoftReset))
                sb.AppendLine().Append(Localization.ATAPI_device_requires_ATA_software_reset);
        }

        if(minatalevel <= 3)
        {
            sb.AppendLine().AppendFormat(Localization.PIO_timing_mode_0, ATAID.PIOTransferTimingMode);
            sb.AppendLine().AppendFormat(Localization.DMA_timing_mode_0, ATAID.DMATransferTimingMode);
        }

        sb.AppendLine().Append(Localization.Advanced_PIO);

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0)) sb.Append("PIO0 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1)) sb.Append("PIO1 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2)) sb.Append("PIO2 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3)) sb.Append("PIO3 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4)) sb.Append("PIO4 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5)) sb.Append("PIO5 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6)) sb.Append("PIO6 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7)) sb.Append("PIO7 ");

        if(minatalevel <= 3 && !atapi)
        {
            sb.AppendLine().Append(Localization.Single_word_DMA);

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
            {
                sb.Append("DMA0 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
                    sb.Append(Localization._active_);
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
            {
                sb.Append("DMA1 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
                    sb.Append(Localization._active_);
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
            {
                sb.Append("DMA2 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
                    sb.Append(Localization._active_);
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
            {
                sb.Append("DMA3 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
                    sb.Append(Localization._active_);
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
            {
                sb.Append("DMA4 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
                    sb.Append(Localization._active_);
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
            {
                sb.Append("DMA5 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
                    sb.Append(Localization._active_);
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
            {
                sb.Append("DMA6 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
                    sb.Append(Localization._active_);
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
            {
                sb.Append("DMA7 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
                    sb.Append(Localization._active_);
            }
        }

        sb.AppendLine().Append(Localization.Multi_word_DMA);

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
        {
            sb.Append("MDMA0 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
                sb.Append(Localization._active_);
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
        {
            sb.Append("MDMA1 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
                sb.Append(Localization._active_);
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
        {
            sb.Append("MDMA2 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
                sb.Append(Localization._active_);
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
        {
            sb.Append("MDMA3 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
                sb.Append(Localization._active_);
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
        {
            sb.Append("MDMA4 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
                sb.Append(Localization._active_);
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
        {
            sb.Append("MDMA5 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
                sb.Append(Localization._active_);
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
        {
            sb.Append("MDMA6 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
                sb.Append(Localization._active_);
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
        {
            sb.Append("MDMA7 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
                sb.Append(Localization._active_);
        }

        sb.AppendLine().Append(Localization.Ultra_DMA);

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
        {
            sb.Append("UDMA0 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
                sb.Append(Localization._active_);
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
        {
            sb.Append("UDMA1 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
                sb.Append(Localization._active_);
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
        {
            sb.Append("UDMA2 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
                sb.Append(Localization._active_);
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
        {
            sb.Append("UDMA3 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
                sb.Append(Localization._active_);
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
        {
            sb.Append("UDMA4 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
                sb.Append(Localization._active_);
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
        {
            sb.Append("UDMA5 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
                sb.Append(Localization._active_);
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
        {
            sb.Append("UDMA6 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
                sb.Append(Localization._active_);
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
        {
            sb.Append("UDMA7 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
                sb.Append(Localization._active_);
        }

        if(ATAID.MinMDMACycleTime != 0 && ATAID.RecMDMACycleTime != 0)
        {
            sb.AppendLine()
              .AppendFormat(Localization.At_minimum_0_ns_transfer_cycle_time_per_word_in_MDMA_1_ns_recommended,
                            ATAID.MinMDMACycleTime,
                            ATAID.RecMDMACycleTime);
        }

        if(ATAID.MinPIOCycleTimeNoFlow != 0)
        {
            sb.AppendLine()
              .AppendFormat(Localization.At_minimum_0_ns_transfer_cycle_time_per_word_in_PIO_without_flow_control,
                            ATAID.MinPIOCycleTimeNoFlow);
        }

        if(ATAID.MinPIOCycleTimeFlow != 0)
        {
            sb.AppendLine()
              .AppendFormat(Localization.At_minimum_0_ns_transfer_cycle_time_per_word_in_PIO_with_IORDY_flow_control,
                            ATAID.MinPIOCycleTimeFlow);
        }

        if(ATAID.MaxQueueDepth != 0)
            sb.AppendLine().AppendFormat(Localization._0_depth_of_queue_maximum, ATAID.MaxQueueDepth + 1);

        if(atapi)
        {
            if(ATAID.PacketBusRelease != 0)
            {
                sb.AppendLine()
                  .AppendFormat(Localization._0_ns_typical_to_release_bus_from_receipt_of_PACKET,
                                ATAID.PacketBusRelease);
            }

            if(ATAID.ServiceBusyClear != 0)
            {
                sb.AppendLine()
                  .AppendFormat(Localization._0_ns_typical_to_clear_BSY_bit_from_receipt_of_SERVICE,
                                ATAID.ServiceBusyClear);
            }
        }

        if((ATAID.TransportMajorVersion & 0xF000) >> 12 == 0x1 || (ATAID.TransportMajorVersion & 0xF000) >> 12 == 0xE)
        {
            if(!ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.Clear))
            {
                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                             .Gen1Speed))
                    sb.AppendLine().Append(Localization.SATA_1_5Gbs_is_supported);

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                             .Gen2Speed))
                    sb.AppendLine().Append(Localization.SATA_3_0Gbs_is_supported);

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                             .Gen3Speed))
                    sb.AppendLine().Append(Localization.SATA_6_0Gbs_is_supported);

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                             .PowerReceipt))
                {
                    sb.AppendLine()
                      .Append(Localization.Receipt_of_host_initiated_power_management_requests_is_supported);
                }

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                             .PHYEventCounter))
                    sb.AppendLine().Append(Localization.PHY_Event_counters_are_supported);

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                             .HostSlumbTrans))
                {
                    sb.AppendLine()
                      .Append(Localization.Supports_host_automatic_partial_to_slumber_transitions_is_supported);
                }

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                             .DevSlumbTrans))
                {
                    sb.AppendLine()
                      .Append(Localization.Supports_device_automatic_partial_to_slumber_transitions_is_supported);
                }

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.NCQ))
                {
                    sb.AppendLine().Append(Localization.NCQ_is_supported);

                    if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                                 .NCQPriority))
                        sb.AppendLine().Append(Localization.NCQ_priority_is_supported);

                    if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                                 .UnloadNCQ))
                        sb.AppendLine().Append(Localization.Unload_is_supported_with_outstanding_NCQ_commands);
                }
            }

            if(!ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2.Clear))
            {
                if(!ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                              .Clear) &&
                   ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.NCQ))
                {
                    if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2
                                                                  .NCQMgmt))
                        sb.AppendLine().Append(Localization.NCQ_queue_management_is_supported);

                    if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2
                                                                  .NCQStream))
                        sb.AppendLine().Append(Localization.NCQ_streaming_is_supported);
                }

                if(atapi)
                {
                    if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2
                                                                  .HostEnvDetect))
                        sb.AppendLine().Append(Localization.ATAPI_device_supports_host_environment_detection);

                    if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2
                                                                  .DevAttSlimline))
                    {
                        sb.AppendLine()
                          .Append(Localization.ATAPI_device_supports_attention_on_slimline_connected_devices);
                    }
                }

                //sb.AppendFormat("Negotiated speed = {0}", ((ushort)ATAID.SATACapabilities2 & 0x000E) >> 1);
            }
        }

        if(ATAID.InterseekDelay != 0x0000 && ATAID.InterseekDelay != 0xFFFF)
        {
            sb.AppendLine()
              .AppendFormat(Localization._0_microseconds_of_interseek_delay_for_ISO_7779_acoustic_testing,
                            ATAID.InterseekDelay);
        }

        if((ushort)ATAID.DeviceFormFactor != 0x0000 && (ushort)ATAID.DeviceFormFactor != 0xFFFF)
        {
            switch(ATAID.DeviceFormFactor)
            {
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.FiveAndQuarter:
                    sb.AppendLine().Append(Localization.Device_nominal_size_is_5_25);

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.ThreeAndHalf:
                    sb.AppendLine().Append(Localization.Device_nominal_size_is_3_5);

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.TwoAndHalf:
                    sb.AppendLine().Append(Localization.Device_nominal_size_is_2_5);

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.OnePointEight:
                    sb.AppendLine().Append(Localization.Device_nominal_size_is_1_8);

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.LessThanOnePointEight:
                    sb.AppendLine().Append(Localization.Device_nominal_size_is_smaller_than_1_8);

                    break;
                default:
                    sb.AppendLine()
                      .AppendFormat(Localization.Device_nominal_size_field_value_0_is_unknown, ATAID.DeviceFormFactor);

                    break;
            }
        }

        if(atapi)
        {
            if(ATAID.ATAPIByteCount > 0)
                sb.AppendLine().AppendFormat(Localization._0_bytes_count_limit_for_ATAPI, ATAID.ATAPIByteCount);
        }

        if(cfa)
        {
            if((ATAID.CFAPowerMode & 0x8000) == 0x8000)
            {
                sb.AppendLine().Append(Localization.CompactFlash_device_supports_power_mode_1);

                if((ATAID.CFAPowerMode & 0x2000) == 0x2000)
                    sb.AppendLine().Append(Localization.CompactFlash_power_mode_1_required_for_one_or_more_commands);

                if((ATAID.CFAPowerMode & 0x1000) == 0x1000)
                    sb.AppendLine().Append(Localization.CompactFlash_power_mode_1_is_disabled);

                sb.AppendLine()
                  .AppendFormat(Localization.CompactFlash_device_uses_a_maximum_of_0_mA, ATAID.CFAPowerMode & 0x0FFF);
            }
        }

        sb.AppendLine();

        sb.AppendLine().Append(Localization.Command_set_and_features);

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Nop))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Nop)
                          ? Localization.NOP_is_supported_and_enabled
                          : Localization.NOP_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.ReadBuffer))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.ReadBuffer)
                          ? Localization.READ_BUFFER_is_supported_and_enabled
                          : Localization.READ_BUFFER_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.WriteBuffer))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit
                                                                 .WriteBuffer)
                          ? Localization.WRITE_BUFFER_is_supported_and_enabled
                          : Localization.WRITE_BUFFER_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.HPA))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.HPA)
                          ? Localization.Host_Protected_Area_is_supported_and_enabled
                          : Localization.Host_Protected_Area_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.DeviceReset))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit
                                                                 .DeviceReset)
                          ? Localization.DEVICE_RESET_is_supported_and_enabled
                          : Localization._);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Service))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Service)
                          ? Localization.SERVICE_interrupt_is_supported_and_enabled
                          : Localization.SERVICE_interrupt_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Release))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Release)
                          ? Localization.Release_is_supported_and_enabled
                          : Localization.Release_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.LookAhead))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.LookAhead)
                          ? Localization.Look_ahead_read_is_supported_and_enabled
                          : Localization.Look_ahead_read_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.WriteCache))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.WriteCache)
                          ? Localization.Write_cache_is_supported_and_enabled
                          : Localization.Write_cache_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Packet))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Packet)
                          ? Localization.PACKET_is_supported_and_enabled
                          : Localization.PACKET_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.PowerManagement))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit
                                                                 .PowerManagement)
                          ? Localization.Power_management_is_supported_and_enabled
                          : Localization.Power_management_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.RemovableMedia))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit
                                                                 .RemovableMedia)
                          ? Localization.Removable_media_feature_set_is_supported_and_enabled
                          : Localization.Removable_media_feature_set_is_supported);
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.SecurityMode))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit
                                                                 .SecurityMode)
                          ? Localization.Security_mode_is_supported_and_enabled
                          : Localization.Security_mode_is_supported);
        }

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.LBASupport))
            sb.AppendLine().Append(Localization._28_bit_LBA_is_supported);

        if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.MustBeSet) &&
           !ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.MustBeClear))
        {
            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.LBA48))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .LBA48)
                              ? Localization._48_bit_LBA_is_supported_and_enabled
                              : Localization._48_bit_LBA_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.FlushCache))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .FlushCache)
                              ? Localization.FLUSH_CACHE_is_supported_and_enabled
                              : Localization.FLUSH_CACHE_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.FlushCacheExt))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .FlushCacheExt)
                              ? Localization.FLUSH_CACHE_EXT_is_supported_and_enabled
                              : Localization.FLUSH_CACHE_EXT_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.DCO))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.DCO)
                              ? Localization.Device_Configuration_Overlay_feature_set_is_supported_and_enabled
                              : Localization.Device_Configuration_Overlay_feature_set_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.AAM))
            {
                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.AAM))
                {
                    sb.AppendLine()
                      .AppendFormat(Localization
                                       .Automatic_Acoustic_Management_is_supported_and_enabled_with_value_0_vendor_recommends_1,
                                    ATAID.CurrentAAM,
                                    ATAID.RecommendedAAM);
                }
                else
                    sb.AppendLine().Append(Localization.Automatic_Acoustic_Management_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.SetMax))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .SetMax)
                              ? Localization.SET_MAX_security_extension_is_supported_and_enabled
                              : Localization.SET_MAX_security_extension_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                    .AddressOffsetReservedAreaBoot))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .AddressOffsetReservedAreaBoot)
                              ? Localization.Address_Offset_Reserved_Area_Boot_is_supported_and_enabled
                              : Localization.Address_Offset_Reserved_Area_Boot_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.SetFeaturesRequired))
                sb.AppendLine().Append(Localization.SET_FEATURES_is_required_before_spin_up);

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.PowerUpInStandby))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .PowerUpInStandby)
                              ? Localization.Power_up_in_standby_is_supported_and_enabled
                              : Localization.Power_up_in_standby_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.RemovableNotification))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .RemovableNotification)
                              ? Localization.Removable_Media_Status_Notification_is_supported_and_enabled
                              : Localization.Removable_Media_Status_Notification_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.APM))
            {
                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.APM))
                {
                    sb.AppendLine()
                      .AppendFormat(Localization.Advanced_Power_Management_is_supported_and_enabled_with_value_0,
                                    ATAID.CurrentAPM);
                }
                else
                    sb.AppendLine().Append(Localization.Advanced_Power_Management_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.CompactFlash))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .CompactFlash)
                              ? Localization.CompactFlash_feature_set_is_supported_and_enabled
                              : Localization.CompactFlash_feature_set_is_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.RWQueuedDMA))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .RWQueuedDMA)
                              ? Localization.READ_DMA_QUEUED_and_WRITE_DMA_QUEUED_are_supported_and_enabled
                              : Localization.READ_DMA_QUEUED_and_WRITE_DMA_QUEUED_are_supported);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.DownloadMicrocode))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2
                                                                      .DownloadMicrocode)
                              ? Localization.DOWNLOAD_MICROCODE_is_supported_and_enabled
                              : Localization.DOWNLOAD_MICROCODE_is_supported);
            }
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.SMART))
        {
            sb.AppendLine()
              .Append(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.SMART)
                          ? Localization.SMART_is_supported_and_enabled
                          : Localization.SMART_is_supported);
        }

        if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit.Supported))
            sb.AppendLine().Append(Localization.SMART_Command_Transport_is_supported);

        if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeSet) &&
           !ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeClear))
        {
            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.SMARTSelfTest))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3
                                                                      .SMARTSelfTest)
                              ? Localization.SMART_self_testing_is_supported_and_enabled
                              : Localization.SMART_self_testing_is_supported);
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.SMARTLog))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3
                                                                      .SMARTLog)
                              ? Localization.SMART_error_logging_is_supported_and_enabled
                              : Localization.SMART_error_logging_is_supported);
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.IdleImmediate))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3
                                                                      .IdleImmediate)
                              ? Localization.IDLE_IMMEDIATE_with_UNLOAD_FEATURE_is_supported_and_enabled
                              : Localization.IDLE_IMMEDIATE_with_UNLOAD_FEATURE_is_supported);
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.WriteURG))
                sb.AppendLine().Append(Localization.URG_bit_is_supported_in_WRITE_STREAM_DMA_EXT_and_WRITE_STREAM_EXT);

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.ReadURG))
                sb.AppendLine().Append(Localization.URG_bit_is_supported_in_READ_STREAM_DMA_EXT_and_READ_STREAM_EXT);

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.WWN))
                sb.AppendLine().Append(Localization.Device_has_a_World_Wide_Name);

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.FUAWriteQ))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3
                                                                      .FUAWriteQ)
                              ? Localization.WRITE_DMA_QUEUED_FUA_EXT_is_supported_and_enabled
                              : Localization.WRITE_DMA_QUEUED_FUA_EXT_is_supported);
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.FUAWrite))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3
                                                                      .FUAWrite)
                              ? Localization.WRITE_DMA_FUA_EXT_and_WRITE_MULTIPLE_FUA_EXT_are_supported_and_enabled
                              : Localization.WRITE_DMA_FUA_EXT_and_WRITE_MULTIPLE_FUA_EXT_are_supported);
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.GPL))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.GPL)
                              ? Localization.General_Purpose_Logging_is_supported_and_enabled
                              : Localization.General_Purpose_Logging_is_supported);
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.Streaming))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3
                                                                      .Streaming)
                              ? Localization.Streaming_feature_set_is_supported_and_enabled
                              : Localization.Streaming_feature_set_is_supported);
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MCPT))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MCPT)
                              ? Localization.Media_Card_Pass_Through_command_set_is_supported_and_enabled
                              : Localization.Media_Card_Pass_Through_command_set_is_supported);
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MediaSerial))
            {
                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3
                                                               .MediaSerial))
                    sb.AppendLine().Append(Localization.Media_Serial_is_supported_and_valid);

                sb.AppendLine().Append(Localization.Media_Serial_is_supported);
            }
        }

        if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.MustBeSet) &&
           !ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.MustBeClear))
        {
            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.DSN))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.DSN)
                              ? Localization.DSN_feature_set_is_supported_and_enabled
                              : Localization.DSN_feature_set_is_supported);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.AMAC))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.AMAC)
                              ? Localization.Accessible_Max_Address_Configuration_is_supported_and_enabled
                              : Localization.Accessible_Max_Address_Configuration_is_supported);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.ExtPowerCond))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4
                                                                      .ExtPowerCond)
                              ? Localization.Extended_Power_Conditions_are_supported_and_enabled
                              : Localization.Extended_Power_Conditions_are_supported);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.ExtStatusReport))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4
                                                                      .ExtStatusReport)
                              ? Localization.Extended_Status_Reporting_is_supported_and_enabled
                              : Localization.Extended_Status_Reporting_is_supported);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.FreeFallControl))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4
                                                                      .FreeFallControl)
                              ? Localization.Free_fall_control_feature_set_is_supported_and_enabled
                              : Localization.Free_fall_control_feature_set_is_supported);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4
                                                    .SegmentedDownloadMicrocode))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4
                                                                      .SegmentedDownloadMicrocode)
                              ? Localization.Segmented_feature_in_DOWNLOAD_MICROCODE_is_supported_and_enabled
                              : Localization.Segmented_feature_in_DOWNLOAD_MICROCODE_is_supported);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.RWDMAExtGpl))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4
                                                                      .RWDMAExtGpl)
                              ? Localization.READ_WRITE_DMA_EXT_GPL_are_supported_and_enabled
                              : Localization.READ_WRITE_DMA_EXT_GPL_are_supported);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.WriteUnc))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4
                                                                      .WriteUnc)
                              ? Localization.WRITE_UNCORRECTABLE_is_supported_and_enabled
                              : Localization.WRITE_UNCORRECTABLE_is_supported);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.WRV))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.WRV)
                              ? Localization.Write_Read_Verify_is_supported_and_enabled
                              : Localization.Write_Read_Verify_is_supported);

                sb.AppendLine()
                  .AppendFormat(Localization._0_sectors_for_Write_Read_Verify_mode_two, ATAID.WRVSectorCountMode2);

                sb.AppendLine()
                  .AppendFormat(Localization._0_sectors_for_Write_Read_Verify_mode_three, ATAID.WRVSectorCountMode3);

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.WRV))
                    sb.AppendLine().AppendFormat(Localization.Current_Write_Read_Verify_mode_0, ATAID.WRVMode);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.DT1825))
            {
                sb.AppendLine()
                  .Append(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4
                                                                      .DT1825)
                              ? Localization.DT1825_is_supported_and_enabled
                              : Localization.DT1825_is_supported);
            }
        }

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.BlockErase))
            sb.AppendLine().Append(Localization.BLOCK_ERASE_EXT_is_supported);

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.Overwrite))
            sb.AppendLine().Append(Localization.OVERWRITE_EXT_is_supported);

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.CryptoScramble))
            sb.AppendLine().Append(Localization.CRYPTO_SCRAMBLE_EXT_is_supported);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.DeviceConfDMA))
        {
            sb.AppendLine()
              .Append(Localization.DEVICE_CONFIGURATION_IDENTIFY_DMA_and_DEVICE_CONFIGURATION_SET_DMA_are_supported);
        }

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ReadBufferDMA))
            sb.AppendLine().Append(Localization.READ_BUFFER_DMA_is_supported);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.WriteBufferDMA))
            sb.AppendLine().Append(Localization.WRITE_BUFFER_DMA_is_supported);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.DownloadMicroCodeDMA))
            sb.AppendLine().Append(Localization.DOWNLOAD_MICROCODE_DMA_is_supported);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.SetMaxDMA))
            sb.AppendLine().Append(Localization.SET_PASSWORD_DMA_and_SET_UNLOCK_DMA_are_supported);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.Ata28))
            sb.AppendLine().Append(Localization.Not_all_28_bit_commands_are_supported);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.CFast))
            sb.AppendLine().Append(Localization.Device_follows_CFast_specification);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.IEEE1667))
            sb.AppendLine().Append(Localization.Device_follows_IEEE_1667);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.DeterministicTrim))
        {
            sb.AppendLine().Append(Localization.Read_after_TRIM_is_deterministic);

            if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ReadZeroTrim))
                sb.AppendLine().Append(Localization.Read_after_TRIM_returns_empty_data);
        }

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.LongPhysSectorAligError))
            sb.AppendLine().Append(Localization.Device_supports_Long_Physical_Sector_Alignment_Error_Reporting_Control);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.Encrypted))
            sb.AppendLine().Append(Localization.Device_encrypts_all_user_data);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.AllCacheNV))
            sb.AppendLine().Append(Localization.Device_s_write_cache_is_non_volatile);

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ZonedBit0) ||
           ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ZonedBit1))
            sb.AppendLine().Append(Localization.Device_is_zoned);

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.Sanitize))
        {
            sb.AppendLine().Append(Localization.Sanitize_feature_set_is_supported);

            sb.AppendLine()
              .Append(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3
                                                             .SanitizeCommands)
                          ? Localization.Sanitize_commands_are_specified_by_ACS_3_or_higher
                          : Localization.Sanitize_commands_are_specified_by_ACS_2);

            if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3
                                                      .SanitizeAntifreeze))
                sb.AppendLine().Append(Localization.SANITIZE_ANTIFREEZE_LOCK_EXT_is_supported);
        }

        if(!ata1 && maxatalevel >= 8)
        {
            if(ATAID.TrustedComputing.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TrustedComputingBit.Set)    &&
               !ATAID.TrustedComputing.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TrustedComputingBit.Clear) &&
               ATAID.TrustedComputing.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TrustedComputingBit
                                                         .TrustedComputing))
                sb.AppendLine().Append(Localization.Trusted_Computing_feature_set_is_supported);
        }

        if((ATAID.TransportMajorVersion & 0xF000) >> 12 == 0x1 || (ATAID.TransportMajorVersion & 0xF000) >> 12 == 0xE)
        {
            if(!ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.Clear))
            {
                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit
                                                             .ReadLogDMAExt))
                    sb.AppendLine().Append(Localization.READ_LOG_DMA_EXT_is_supported);
            }

            if(!ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2.Clear))
            {
                if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2
                                                              .FPDMAQ))
                    sb.AppendLine().Append(Localization.RECEIVE_FPDMA_QUEUED_and_SEND_FPDMA_QUEUED_are_supported);
            }

            if(!ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.Clear))
            {
                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                         .NonZeroBufferOffset))
                {
                    sb.AppendLine()
                      .Append(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                                           .NonZeroBufferOffset)
                                  ? Localization.Non_zero_buffer_offsets_are_supported_and_enabled
                                  : Localization.Non_zero_buffer_offsets_are_supported);
                }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.DMASetup))
                {
                    sb.AppendLine()
                      .Append(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                                           .DMASetup)
                                  ? Localization.DMA_Setup_auto_activation_is_supported_and_enabled
                                  : Localization.DMA_Setup_auto_activation_is_supported);
                }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.InitPowerMgmt))
                {
                    sb.AppendLine()
                      .Append(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                                           .InitPowerMgmt)
                                  ? Localization.Device_initiated_power_management_is_supported_and_enabled
                                  : Localization.Device_initiated_power_management_is_supported);
                }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.InOrderData))
                {
                    sb.AppendLine()
                      .Append(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                                           .InOrderData)
                                  ? Localization.In_order_data_delivery_is_supported_and_enabled
                                  : Localization.In_order_data_delivery_is_supported);
                }

                switch(atapi)
                {
                    case false:
                    {
                        if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                                 .HardwareFeatureControl))
                        {
                            sb.AppendLine()
                              .Append(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify
                                                                           .SATAFeaturesBit.HardwareFeatureControl)
                                          ? Localization.Hardware_Feature_Control_is_supported_and_enabled
                                          : Localization.Hardware_Feature_Control_is_supported);
                        }

                        break;
                    }
                    case true:
                    {
                        if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                                 .AsyncNotification))
                        {
                            sb.AppendLine()
                              .Append(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify
                                                                           .SATAFeaturesBit.AsyncNotification)
                                          ? Localization.Asynchronous_notification_is_supported_and_enabled
                                          : Localization.Asynchronous_notification_is_supported);
                        }

                        break;
                    }
                }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                         .SettingsPreserve))
                {
                    sb.AppendLine()
                      .Append(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                                           .SettingsPreserve)
                                  ? Localization.Software_Settings_Preservation_is_supported_and_enabled
                                  : Localization.Software_Settings_Preservation_is_supported);
                }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.NCQAutoSense))
                    sb.AppendLine().Append(Localization.NCQ_Autosense_is_supported);

                if(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit
                                                                .EnabledSlumber))
                    sb.AppendLine().Append(Localization.Automatic_Partial_to_Slumber_transitions_are_enabled);
            }
        }

        if((ATAID.RemovableStatusSet & 0x03) > 0)
            sb.AppendLine().Append(Localization.Removable_Media_Status_Notification_feature_set_is_supported);

        if(ATAID.FreeFallSensitivity != 0x00 && ATAID.FreeFallSensitivity != 0xFF)
            sb.AppendLine().AppendFormat(Localization.Free_fall_sensitivity_set_to_0, ATAID.FreeFallSensitivity);

        if(ATAID.DataSetMgmt.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.DataSetMgmtBit.Trim))
            sb.AppendLine().Append("TRIM is supported");

        if(ATAID.DataSetMgmtSize > 0)
        {
            sb.AppendLine()
              .AppendFormat(Localization.DATA_SET_MANAGEMENT_can_receive_a_maximum_of_0_blocks_of_512_bytes,
                            ATAID.DataSetMgmtSize);
        }

        sb.AppendLine().AppendLine();

        if(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit.Supported))
        {
            sb.AppendLine(Localization.Security);

            if(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit.Enabled))
            {
                sb.AppendLine(Localization.Security_is_enabled);

                sb.AppendLine(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit
                                                                      .Locked)
                                  ? Localization.Security_is_locked
                                  : Localization.Security_is_not_locked);

                sb.AppendLine(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit
                                                                      .Frozen)
                                  ? Localization.Security_is_frozen
                                  : Localization.Security_is_not_frozen);

                sb.AppendLine(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit
                                                                      .Expired)
                                  ? Localization.Security_count_has_expired
                                  : Localization.Security_count_has_not_expired);

                sb.AppendLine(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit
                                                                      .Maximum)
                                  ? Localization.Security_level_is_maximum
                                  : Localization.Security_level_is_high);
            }
            else
                sb.AppendLine(Localization.Security_is_not_enabled);

            if(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit.Enhanced))
                sb.AppendLine(Localization.Supports_enhanced_security_erase);

            sb.AppendFormat(Localization._0_minutes_to_complete_secure_erase, ATAID.SecurityEraseTime * 2).AppendLine();

            if(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit.Enhanced))
            {
                sb.AppendFormat(Localization._0_minutes_to_complete_enhanced_secure_erase,
                                ATAID.EnhancedSecurityEraseTime * 2)
                  .AppendLine();
            }

            sb.AppendFormat(Localization.Master_password_revision_code_0, ATAID.MasterPasswordRevisionCode)
              .AppendLine();
        }

        if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeSet)    &&
           !ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeClear) &&
           ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.Streaming))
        {
            sb.AppendLine().AppendLine(Localization.Streaming);
            sb.AppendFormat(Localization.Minimum_request_size_is_0,              ATAID.StreamMinReqSize);
            sb.AppendFormat(Localization.Streaming_transfer_time_in_PIO_is_0,    ATAID.StreamTransferTimePIO);
            sb.AppendFormat(Localization.Streaming_transfer_time_in_DMA_is_0,    ATAID.StreamTransferTimeDMA);
            sb.AppendFormat(Localization.Streaming_access_latency_is_0,          ATAID.StreamAccessLatency);
            sb.AppendFormat(Localization.Streaming_performance_granularity_is_0, ATAID.StreamPerformanceGranularity);
        }

        if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit.Supported))
        {
            sb.AppendLine().AppendLine(Localization.SMART_Command_Transport_SCT);

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit
                                                            .LongSectorAccess))
                sb.AppendLine(Localization.SCT_Long_Sector_Address_is_supported);

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit
                                                            .WriteSame))
                sb.AppendLine(Localization.SCT_Write_Same_is_supported);

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit
                                                            .ErrorRecoveryControl))
                sb.AppendLine(Localization.SCT_Error_Recovery_Control_is_supported);

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit
                                                            .FeaturesControl))
                sb.AppendLine(Localization.SCT_Features_Control_is_supported);

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit
                                                            .DataTables))
                sb.AppendLine(Localization.SCT_Data_Tables_are_supported);
        }

        if((ATAID.NVCacheCaps & 0x0010) == 0x0010)
        {
            sb.AppendLine().AppendLine(Localization.Non_Volatile_Cache);
            sb.AppendLine().AppendFormat(Localization.Version_0, (ATAID.NVCacheCaps & 0xF000) >> 12).AppendLine();

            if((ATAID.NVCacheCaps & 0x0001) == 0x0001)
            {
                sb.Append((ATAID.NVCacheCaps & 0x0002) == 0x0002
                              ? Localization.Power_mode_feature_set_is_supported_and_enabled
                              : Localization.Power_mode_feature_set_is_supported);

                sb.AppendLine();

                sb.AppendLine().AppendFormat(Localization.Version_0, (ATAID.NVCacheCaps & 0x0F00) >> 8).AppendLine();
            }

            sb.AppendLine()
              .AppendFormat(Localization.Non_Volatile_Cache_is_0_bytes, ATAID.NVCacheSize * logicalSectorSize)
              .AppendLine();
        }

#if DEBUG
        sb.AppendLine();

        if(ATAID.VendorWord9 != 0x0000 && ATAID.VendorWord9 != 0xFFFF)
            sb.AppendFormat(Localization.Word_nine_0, ATAID.VendorWord9).AppendLine();

        if((ATAID.VendorWord47 & 0x7F) != 0x7F && (ATAID.VendorWord47 & 0x7F) != 0x00)
            sb.AppendFormat(Localization.Word_47_bits_15_to_8_0, ATAID.VendorWord47).AppendLine();

        if(ATAID.VendorWord51 != 0x00 && ATAID.VendorWord51 != 0xFF)
            sb.AppendFormat(Localization.Word_51_bits_7_to_0_0, ATAID.VendorWord51).AppendLine();

        if(ATAID.VendorWord52 != 0x00 && ATAID.VendorWord52 != 0xFF)
            sb.AppendFormat(Localization.Word_52_bits_7_to_0_0, ATAID.VendorWord52).AppendLine();

        if(ATAID.ReservedWord64 != 0x00 && ATAID.ReservedWord64 != 0xFF)
            sb.AppendFormat(Localization.Word_64_bits_15_to_8_0, ATAID.ReservedWord64).AppendLine();

        if(ATAID.ReservedWord70 != 0x0000 && ATAID.ReservedWord70 != 0xFFFF)
            sb.AppendFormat(Localization.Word_70_0, ATAID.ReservedWord70).AppendLine();

        if(ATAID.ReservedWord73 != 0x0000 && ATAID.ReservedWord73 != 0xFFFF)
            sb.AppendFormat(Localization.Word_73_0, ATAID.ReservedWord73).AppendLine();

        if(ATAID.ReservedWord74 != 0x0000 && ATAID.ReservedWord74 != 0xFFFF)
            sb.AppendFormat(Localization.Word_74_0, ATAID.ReservedWord74).AppendLine();

        if(ATAID.ReservedWord116 != 0x0000 && ATAID.ReservedWord116 != 0xFFFF)
            sb.AppendFormat(Localization.Word_116_0, ATAID.ReservedWord116).AppendLine();

        for(var i = 0; i < ATAID.ReservedWords121.Length; i++)
        {
            if(ATAID.ReservedWords121[i] != 0x0000 && ATAID.ReservedWords121[i] != 0xFFFF)
                sb.AppendFormat(Localization.Word_1_0, ATAID.ReservedWords121[i], 121 + i).AppendLine();
        }

        for(var i = 0; i < ATAID.ReservedWords129.Length; i++)
        {
            if(ATAID.ReservedWords129[i] != 0x0000 && ATAID.ReservedWords129[i] != 0xFFFF)
                sb.AppendFormat(Localization.Word_1_0, ATAID.ReservedWords129[i], 129 + i).AppendLine();
        }

        for(var i = 0; i < ATAID.ReservedCFA.Length; i++)
        {
            if(ATAID.ReservedCFA[i] != 0x0000 && ATAID.ReservedCFA[i] != 0xFFFF)
                sb.AppendFormat(Localization.Word_1_CFA_0, ATAID.ReservedCFA[i], 161 + i).AppendLine();
        }

        if(ATAID.ReservedWord174 != 0x0000 && ATAID.ReservedWord174 != 0xFFFF)
            sb.AppendFormat(Localization.Word_174_0, ATAID.ReservedWord174).AppendLine();

        if(ATAID.ReservedWord175 != 0x0000 && ATAID.ReservedWord175 != 0xFFFF)
            sb.AppendFormat(Localization.Word_175_0, ATAID.ReservedWord175).AppendLine();

        if(ATAID.ReservedCEATAWord207 != 0x0000 && ATAID.ReservedCEATAWord207 != 0xFFFF)
            sb.AppendFormat(Localization.Word_207_CE_ATA_0, ATAID.ReservedCEATAWord207).AppendLine();

        if(ATAID.ReservedCEATAWord208 != 0x0000 && ATAID.ReservedCEATAWord208 != 0xFFFF)
            sb.AppendFormat(Localization.Word_208_CE_ATA_0, ATAID.ReservedCEATAWord208).AppendLine();

        if(ATAID.NVReserved != 0x00 && ATAID.NVReserved != 0xFF)
            sb.AppendFormat(Localization.Word_219_bits_15_to_8_0, ATAID.NVReserved).AppendLine();

        if(ATAID.WRVReserved != 0x00 && ATAID.WRVReserved != 0xFF)
            sb.AppendFormat(Localization.Word_220_bits_15_to_8_0, ATAID.WRVReserved).AppendLine();

        if(ATAID.ReservedWord221 != 0x0000 && ATAID.ReservedWord221 != 0xFFFF)
            sb.AppendFormat(Localization.Word_221_0, ATAID.ReservedWord221).AppendLine();

        for(var i = 0; i < ATAID.ReservedCEATA224.Length; i++)
        {
            if(ATAID.ReservedCEATA224[i] != 0x0000 && ATAID.ReservedCEATA224[i] != 0xFFFF)
                sb.AppendFormat(Localization.Word_1_CE_ATA_0, ATAID.ReservedCEATA224[i], 224 + i).AppendLine();
        }

        for(var i = 0; i < ATAID.ReservedWords.Length; i++)
        {
            if(ATAID.ReservedWords[i] != 0x0000 && ATAID.ReservedWords[i] != 0xFFFF)
                sb.AppendFormat(Localization.Word_1_0, ATAID.ReservedWords[i], 236 + i).AppendLine();
        }
#endif
        return sb.ToString();
    }
}