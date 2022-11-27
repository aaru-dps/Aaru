// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Inquiry.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI INQUIRY responses.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Helpers;
using Aaru.Localization;

namespace Aaru.Decoders.SCSI;

// Information from the following standards:
// T9/375-D revision 10l
// T10/995-D revision 10
// T10/1236-D revision 20
// T10/1416-D revision 23
// T10/1731-D revision 16
// T10/502 revision 05
// RFC 7144
// ECMA-111
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Inquiry
{
    public static string Prettify(CommonTypes.Structs.Devices.SCSI.Inquiry? SCSIInquiryResponse)
    {
        if(SCSIInquiryResponse == null)
            return null;

        CommonTypes.Structs.Devices.SCSI.Inquiry response = SCSIInquiryResponse.Value;

        var sb = new StringBuilder();

        sb.AppendFormat(Localization.Device_vendor_0,
                        VendorString.Prettify(StringHandlers.CToString(response.VendorIdentification).Trim())).
           AppendLine();

        sb.AppendFormat(Localization.Device_name_0, StringHandlers.CToString(response.ProductIdentification).Trim()).
           AppendLine();

        sb.AppendFormat(Localization.Device_release_level_0,
                        StringHandlers.CToString(response.ProductRevisionLevel).Trim()).AppendLine();

        switch((PeripheralQualifiers)response.PeripheralQualifier)
        {
            case PeripheralQualifiers.Supported:
                sb.AppendLine(Localization.Device_is_connected_and_supported);

                break;
            case PeripheralQualifiers.Unconnected:
                sb.AppendLine(Localization.Device_is_supported_but_not_connected);

                break;
            case PeripheralQualifiers.Reserved:
                sb.AppendLine(Localization.Reserved_value_set_in_Peripheral_Qualifier_field);

                break;
            case PeripheralQualifiers.Unsupported:
                sb.AppendLine(Localization.Device_is_connected_but_unsupported);

                break;
            default:
                sb.AppendFormat(Localization.Vendor_value_0_set_in_Peripheral_Qualifier_field,
                                response.PeripheralQualifier).AppendLine();

                break;
        }

        switch((PeripheralDeviceTypes)response.PeripheralDeviceType)
        {
            case PeripheralDeviceTypes.DirectAccess: //0x00,
                sb.AppendLine(Localization.Direct_access_device);

                break;
            case PeripheralDeviceTypes.SequentialAccess: //0x01,
                sb.AppendLine(Localization.Sequential_access_device);

                break;
            case PeripheralDeviceTypes.PrinterDevice: //0x02,
                sb.AppendLine(Localization.Printer_device);

                break;
            case PeripheralDeviceTypes.ProcessorDevice: //0x03,
                sb.AppendLine(Localization.Processor_device);

                break;
            case PeripheralDeviceTypes.WriteOnceDevice: //0x04,
                sb.AppendLine(Localization.Write_once_device);

                break;
            case PeripheralDeviceTypes.MultiMediaDevice: //0x05,
                sb.AppendLine(Localization.CD_ROM_DVD_etc_device);

                break;
            case PeripheralDeviceTypes.ScannerDevice: //0x06,
                sb.AppendLine(Localization.Scanner_device);

                break;
            case PeripheralDeviceTypes.OpticalDevice: //0x07,
                sb.AppendLine(Localization.Optical_memory_device);

                break;
            case PeripheralDeviceTypes.MediumChangerDevice: //0x08,
                sb.AppendLine(Localization.Medium_change_device);

                break;
            case PeripheralDeviceTypes.CommsDevice: //0x09,
                sb.AppendLine(Localization.Communications_device);

                break;
            case PeripheralDeviceTypes.PrePressDevice1: //0x0A,
                sb.AppendLine(Localization.Graphics_arts_pre_press_device_defined_in_ASC_IT8);

                break;
            case PeripheralDeviceTypes.PrePressDevice2: //0x0B,
                sb.AppendLine(Localization.Graphics_arts_pre_press_device_defined_in_ASC_IT8);

                break;
            case PeripheralDeviceTypes.ArrayControllerDevice: //0x0C,
                sb.AppendLine(Localization.Array_controller_device);

                break;
            case PeripheralDeviceTypes.EnclosureServiceDevice: //0x0D,
                sb.AppendLine(Localization.Enclosure_services_device);

                break;
            case PeripheralDeviceTypes.SimplifiedDevice: //0x0E,
                sb.AppendLine(Localization.Simplified_direct_access_device);

                break;
            case PeripheralDeviceTypes.OCRWDevice: //0x0F,
                sb.AppendLine(Localization.Optical_card_reader_writer_device);

                break;
            case PeripheralDeviceTypes.BridgingExpander: //0x10,
                sb.AppendLine(Localization.Bridging_Expanders);

                break;
            case PeripheralDeviceTypes.ObjectDevice: //0x11,
                sb.AppendLine(Localization.Object_based_Storage_Device);

                break;
            case PeripheralDeviceTypes.ADCDevice: //0x12,
                sb.AppendLine(Localization.Automation_Drive_Interface);

                break;
            case PeripheralDeviceTypes.SCSISecurityManagerDevice: //0x13,
                sb.AppendLine(Localization.Security_Manager_Device);

                break;
            case PeripheralDeviceTypes.SCSIZonedBlockDevice: //0x14
                sb.AppendLine(Localization.Host_managed_zoned_block_device);

                break;
            case PeripheralDeviceTypes.WellKnownDevice: //0x1E,
                sb.AppendLine(Localization.Well_known_logical_unit);

                break;
            case PeripheralDeviceTypes.UnknownDevice: //0x1F
                sb.AppendLine(Localization.Unknown_or_no_device_type);

                break;
            default:
                sb.AppendFormat(Localization.Unknown_device_type_field_value_0, response.PeripheralDeviceType).
                   AppendLine();

                break;
        }

        switch((ANSIVersions)response.ANSIVersion)
        {
            case ANSIVersions.ANSINoVersion:
                sb.AppendLine(Localization.Device_does_not_claim_to_comply_with_any_SCSI_ANSI_standard);

                break;
            case ANSIVersions.ANSI1986Version:
                sb.AppendLine(Localization.Device_claims_to_comply_with_ANSI_X3_131_1986_SCSI_1);

                break;
            case ANSIVersions.ANSI1994Version:
                sb.AppendLine(Localization.Device_claims_to_comply_with_ANSI_X3_131_1994_SCSI_2);

                break;
            case ANSIVersions.ANSI1997Version:
                sb.AppendLine(Localization.Device_claims_to_comply_with_ANSI_X3_301_1997_SPC_1);

                break;
            case ANSIVersions.ANSI2001Version:
                sb.AppendLine(Localization.Device_claims_to_comply_with_ANSI_X3_351_2001_SPC_2);

                break;
            case ANSIVersions.ANSI2005Version:
                sb.AppendLine(Localization.Device_claims_to_comply_with_ANSI_X3_408_2005_SPC_3);

                break;
            case ANSIVersions.ANSI2008Version:
                sb.AppendLine(Localization.Device_claims_to_comply_with_ANSI_X3_408_2005_SPC_4);

                break;
            default:
                sb.AppendFormat(Localization.Device_claims_to_comply_with_unknown_SCSI_ANSI_standard_value_0,
                                response.ANSIVersion).AppendLine();

                break;
        }

        switch((ECMAVersions)response.ECMAVersion)
        {
            case ECMAVersions.ECMANoVersion:
                sb.AppendLine(Localization.Device_does_not_claim_to_comply_with_any_SCSI_ECMA_standard);

                break;
            case ECMAVersions.ECMA111:
                sb.AppendLine(Localization.Device_claims_to_comply_ECMA_111_Small_Computer_System_Interface_SCSI);

                break;
            default:
                sb.AppendFormat(Localization.Device_claims_to_comply_with_unknown_SCSI_ECMA_standard_value_0,
                                response.ECMAVersion).AppendLine();

                break;
        }

        switch((ISOVersions)response.ISOVersion)
        {
            case ISOVersions.ISONoVersion:
                sb.AppendLine(Localization.Device_does_not_claim_to_comply_with_any_SCSI_ISO_IEC_standard);

                break;
            case ISOVersions.ISO1995Version:
                sb.AppendLine(Localization.Device_claims_to_comply_with_ISO_IEC_9316_1995);

                break;
            default:
                sb.AppendFormat(Localization.Device_claims_to_comply_with_unknown_SCSI_ISO_IEC_standard_value_0,
                                response.ISOVersion).AppendLine();

                break;
        }

        if(response.RMB)
            sb.AppendLine(Localization.Device_is_removable);

        if(response.AERC)
            sb.AppendLine(Localization.Device_supports_Asynchronous_Event_Reporting_Capability);

        if(response.TrmTsk)
            sb.AppendLine(Localization.Device_supports_TERMINATE_TASK_command);

        if(response.NormACA)
            sb.AppendLine(Localization.Device_supports_setting_Normal_ACA);

        if(response.HiSup)
            sb.AppendLine(Localization.Device_supports_LUN_hierarchical_addressing);

        if(response.SCCS)
            sb.AppendLine(Localization.Device_contains_an_embedded_storage_array_controller);

        if(response.ACC)
            sb.AppendLine(Localization.Device_contains_an_Access_Control_Coordinator);

        if(response.ThreePC)
            sb.AppendLine(Localization.Device_supports_third_party_copy_commands);

        if(response.Protect)
            sb.AppendLine(Localization.Device_supports_protection_information);

        if(response.BQue)
            sb.AppendLine(Localization.Device_supports_basic_queueing);

        if(response.EncServ)
            sb.AppendLine(Localization.Device_contains_an_embedded_enclosure_services_component);

        if(response.MultiP)
            sb.AppendLine(Localization.Multi_port_device);

        if(response.MChngr)
            sb.AppendLine(Localization.Device_contains_or_is_attached_to_a_medium_changer);

        if(response.ACKREQQ)
            sb.AppendLine(Localization.Device_supports_request_and_acknowledge_handshakes);

        if(response.Addr32)
            sb.AppendLine(Localization.Device_supports_32_bit_wide_SCSI_addresses);

        if(response.Addr16)
            sb.AppendLine(Localization.Device_supports_16_bit_wide_SCSI_addresses);

        if(response.RelAddr)
            sb.AppendLine(Localization.Device_supports_relative_addressing);

        if(response.WBus32)
            sb.AppendLine(Localization.Device_supports_32_bit_wide_data_transfers);

        if(response.WBus16)
            sb.AppendLine(Localization.Device_supports_16_bit_wide_data_transfers);

        if(response.Sync)
            sb.AppendLine(Localization.Device_supports_synchronous_data_transfer);

        if(response.Linked)
            sb.AppendLine(Localization.Device_supports_linked_commands);

        if(response.TranDis)
            sb.AppendLine(Localization.Device_supports_CONTINUE_TASK_and_TARGET_TRANSFER_DISABLE_commands);

        if(response.QAS)
            sb.AppendLine(Localization.Device_supports_Quick_Arbitration_and_Selection);

        if(response.CmdQue)
            sb.AppendLine(Localization.Device_supports_TCQ_queue);

        if(response.IUS)
            sb.AppendLine(Localization.Device_supports_information_unit_transfers);

        if(response.SftRe)
            sb.AppendLine(Localization.Device_implements_RESET_as_a_soft_reset);
    #if DEBUG
        if(response.VS1)
            sb.AppendLine(Localization.Vendor_specific_bit_5_on_byte_6_of_INQUIRY_response_is_set);
    #endif

        switch((TGPSValues)response.TPGS)
        {
            case TGPSValues.NotSupported:
                sb.AppendLine(Localization.Device_does_not_support_asymmetrical_access);

                break;
            case TGPSValues.OnlyImplicit:
                sb.AppendLine(Localization.Device_only_supports_implicit_asymmetrical_access);

                break;
            case TGPSValues.OnlyExplicit:
                sb.AppendLine(Localization.Device_only_supports_explicit_asymmetrical_access);

                break;
            case TGPSValues.Both:
                sb.AppendLine(Localization.Device_supports_implicit_and_explicit_asymmetrical_access);

                break;
            default:
                sb.AppendFormat(Localization.Unknown_value_in_TPGS_field_0, response.TPGS).AppendLine();

                break;
        }

        switch((SPIClocking)response.Clocking)
        {
            case SPIClocking.ST:
                sb.AppendLine(Localization.Device_supports_only_ST_clocking);

                break;
            case SPIClocking.DT:
                sb.AppendLine(Localization.Device_supports_only_DT_clocking);

                break;
            case SPIClocking.Reserved:
                sb.AppendLine(Localization.Reserved_value_0x02_found_in_SPI_clocking_field);

                break;
            case SPIClocking.STandDT:
                sb.AppendLine(Localization.Device_supports_ST_and_DT_clocking);

                break;
            default:
                sb.AppendFormat(Localization.Unknown_value_in_SPI_clocking_field_0, response.Clocking).AppendLine();

                break;
        }

        if(response.VersionDescriptors != null)
            foreach(ushort VersionDescriptor in response.VersionDescriptors)
                switch(VersionDescriptor)
                {
                    case 0xFFFF:
                    case 0x0000: break;
                    case 0x0020:
                        sb.AppendLine(Localization.Device_complies_with_SAM_no_version_claimed);

                        break;
                    case 0x003B:
                        sb.AppendLine(Localization.Device_complies_with_SAM_T10_0994_D_revision_18);

                        break;
                    case 0x003C:
                        sb.AppendLine(Localization.Device_complies_with_SAM_ANSI_INCITS_270_1996);

                        break;
                    case 0x0040:
                        sb.AppendLine(Localization.Device_complies_with_SAM_2_no_version_claimed);

                        break;
                    case 0x0054:
                        sb.AppendLine(Localization.Device_complies_with_SAM_2_T10_1157_D_revision_23);

                        break;
                    case 0x0055:
                        sb.AppendLine(Localization.Device_complies_with_SAM_2_T10_1157_D_revision_24);

                        break;
                    case 0x005C:
                        sb.AppendLine(Localization.Device_complies_with_SAM_2_ANSI_INCITS_366_2003);

                        break;
                    case 0x005E:
                        sb.AppendLine(Localization.Device_complies_with_SAM_2_ISO_IEC_14776_412);

                        break;
                    case 0x0060:
                        sb.AppendLine(Localization.Device_complies_with_SAM_3_no_version_claimed);

                        break;
                    case 0x0062:
                        sb.AppendLine(Localization.Device_complies_with_SAM_3_T10_1561_D_revision_7);

                        break;
                    case 0x0075:
                        sb.AppendLine(Localization.Device_complies_with_SAM_3_T10_1561_D_revision_13);

                        break;
                    case 0x0076:
                        sb.AppendLine(Localization.Device_complies_with_SAM_3_T10_1561_D_revision_14);

                        break;
                    case 0x0077:
                        sb.AppendLine(Localization.Device_complies_with_SAM_3_ANSI_INCITS_402_2005);

                        break;
                    case 0x0080:
                        sb.AppendLine(Localization.Device_complies_with_SAM_4_no_version_claimed);

                        break;
                    case 0x0087:
                        sb.AppendLine(Localization.Device_complies_with_SAM_4_T10_1683_D_revision_13);

                        break;
                    case 0x008B:
                        sb.AppendLine(Localization.Device_complies_with_SAM_4_T10_1683_D_revision_14);

                        break;
                    case 0x0090:
                        sb.AppendLine(Localization.Device_complies_with_SAM_4_ANSI_INCITS_447_2008);

                        break;
                    case 0x0092:
                        sb.AppendLine(Localization.Device_complies_with_SAM_4_ISO_IEC_14776_414);

                        break;
                    case 0x00A0:
                        sb.AppendLine(Localization.Device_complies_with_SAM_5_no_version_claimed);

                        break;
                    case 0x00A2:
                        sb.AppendLine(Localization.Device_complies_with_SAM_5_T10_2104_D_revision_4);

                        break;
                    case 0x00A4:
                        sb.AppendLine(Localization.Device_complies_with_SAM_5_T10_2104_D_revision_20);

                        break;
                    case 0x00A6:
                        sb.AppendLine(Localization.Device_complies_with_SAM_5_T10_2104_D_revision_21);

                        break;
                    case 0x00C0:
                        sb.AppendLine(Localization.Device_complies_with_SAM_6_no_version_claimed);

                        break;
                    case 0x0120:
                        sb.AppendLine(Localization.Device_complies_with_SPC_no_version_claimed);

                        break;
                    case 0x013B:
                        sb.AppendLine(Localization.Device_complies_with_SPC_T10_0995_D_revision_11a);

                        break;
                    case 0x013C:
                        sb.AppendLine(Localization.Device_complies_with_SPC_ANSI_INCITS_301_1997);

                        break;
                    case 0x0140:
                        sb.AppendLine(Localization.Device_complies_with_MMC_no_version_claimed);

                        break;
                    case 0x015B:
                        sb.AppendLine(Localization.Device_complies_with_MMC_T10_1048_D_revision_10a);

                        break;
                    case 0x015C:
                        sb.AppendLine(Localization.Device_complies_with_MMC_ANSI_INCITS_304_1997);

                        break;
                    case 0x0160:
                        sb.AppendLine(Localization.Device_complies_with_SCC_no_version_claimed);

                        break;
                    case 0x017B:
                        sb.AppendLine(Localization.Device_complies_with_SCC_T10_1047_D_revision_06c);

                        break;
                    case 0x017C:
                        sb.AppendLine(Localization.Device_complies_with_SCC_ANSI_INCITS_276_1997);

                        break;
                    case 0x0180:
                        sb.AppendLine(Localization.Device_complies_with_SBC_no_version_claimed);

                        break;
                    case 0x019B:
                        sb.AppendLine(Localization.Device_complies_with_SBC_T10_0996_D_revision_08c);

                        break;
                    case 0x019C:
                        sb.AppendLine(Localization.Device_complies_with_SBC_ANSI_INCITS_306_1998);

                        break;
                    case 0x01A0:
                        sb.AppendLine(Localization.Device_complies_with_SMC_no_version_claimed);

                        break;
                    case 0x01BB:
                        sb.AppendLine(Localization.Device_complies_with_SMC_T10_0999_D_revision_10a);

                        break;
                    case 0x01BC:
                        sb.AppendLine(Localization.Device_complies_with_SMC_ANSI_INCITS_314_1998);

                        break;
                    case 0x01BE:
                        sb.AppendLine(Localization.Device_complies_with_SMC_ISO_IEC_14776_351);

                        break;
                    case 0x01C0:
                        sb.AppendLine(Localization.Device_complies_with_SES_no_version_claimed);

                        break;
                    case 0x01DB:
                        sb.AppendLine(Localization.Device_complies_with_SES_T10_1212_D_revision_08b);

                        break;
                    case 0x01DC:
                        sb.AppendLine(Localization.Device_complies_with_SES_ANSI_INCITS_305_1998);

                        break;
                    case 0x01DD:
                        sb.AppendLine(Localization.
                                          Device_complies_with_SES_T10_1212_revision_08b_Amendment_ANSI_INCITS_305_AM1_2000);

                        break;
                    case 0x01DE:
                        sb.AppendLine(Localization.
                                          Device_complies_with_SES_ANSI_INCITS_305_1998_Amendment_ANSI_INCITS_305_AM1_2000);

                        break;
                    case 0x01E0:
                        sb.AppendLine(Localization.Device_complies_with_SCC_2_no_version_claimed);

                        break;
                    case 0x01FB:
                        sb.AppendLine(Localization.Device_complies_with_SCC_2_T10_1125_D_revision_04);

                        break;
                    case 0x01FC:
                        sb.AppendLine(Localization.Device_complies_with_SCC_2_ANSI_INCITS_318_1998);

                        break;
                    case 0x0200:
                        sb.AppendLine(Localization.Device_complies_with_SSC_no_version_claimed);

                        break;
                    case 0x0201:
                        sb.AppendLine(Localization.Device_complies_with_SSC_T10_0997_D_revision_17);

                        break;
                    case 0x0207:
                        sb.AppendLine(Localization.Device_complies_with_SSC_T10_0997_D_revision_22);

                        break;
                    case 0x021C:
                        sb.AppendLine(Localization.Device_complies_with_SSC_ANSI_INCITS_335_2000);

                        break;
                    case 0x0220:
                        sb.AppendLine(Localization.Device_complies_with_RBC_no_version_claimed);

                        break;
                    case 0x0238:
                        sb.AppendLine(Localization.Device_complies_with_RBC_T10_1240_D_revision_10a);

                        break;
                    case 0x023C:
                        sb.AppendLine(Localization.Device_complies_with_RBC_ANSI_INCITS_330_2000);

                        break;
                    case 0x0240:
                        sb.AppendLine(Localization.Device_complies_with_MMC_2_no_version_claimed);

                        break;
                    case 0x0255:
                        sb.AppendLine(Localization.Device_complies_with_MMC_2_T10_1228_D_revision_11);

                        break;
                    case 0x025B:
                        sb.AppendLine(Localization.Device_complies_with_MMC_2_T10_1228_D_revision_11a);

                        break;
                    case 0x025C:
                        sb.AppendLine(Localization.Device_complies_with_MMC_2_ANSI_INCITS_333_2000);

                        break;
                    case 0x0260:
                        sb.AppendLine(Localization.Device_complies_with_SPC_2_no_version_claimed);

                        break;
                    case 0x0267:
                        sb.AppendLine(Localization.Device_complies_with_SPC_2_T10_1236_D_revision_12);

                        break;
                    case 0x0269:
                        sb.AppendLine(Localization.Device_complies_with_SPC_2_T10_1236_D_revision_18);

                        break;
                    case 0x0275:
                        sb.AppendLine(Localization.Device_complies_with_SPC_2_T10_1236_D_revision_19);

                        break;
                    case 0x0276:
                        sb.AppendLine(Localization.Device_complies_with_SPC_2_T10_1236_D_revision_20);

                        break;
                    case 0x0277:
                        sb.AppendLine(Localization.Device_complies_with_SPC_2_ANSI_INCITS_351_2001);

                        break;
                    case 0x0278:
                        sb.AppendLine(Localization.Device_complies_with_SPC_2_ISO_IEC_14776_452);

                        break;
                    case 0x0280:
                        sb.AppendLine(Localization.Device_complies_with_OCRW_no_version_claimed);

                        break;
                    case 0x029E:
                        sb.AppendLine(Localization.Device_complies_with_OCRW_ISO_IEC_14776_381);

                        break;
                    case 0x02A0:
                        sb.AppendLine(Localization.Device_complies_with_MMC_3_no_version_claimed);

                        break;
                    case 0x02B5:
                        sb.AppendLine(Localization.Device_complies_with_MMC_3_T10_1363_D_revision_9);

                        break;
                    case 0x02B6:
                        sb.AppendLine(Localization.Device_complies_with_MMC_3_T10_1363_D_revision_10g);

                        break;
                    case 0x02B8:
                        sb.AppendLine(Localization.Device_complies_with_MMC_3_ANSI_INCITS_360_2002);

                        break;
                    case 0x02E0:
                        sb.AppendLine(Localization.Device_complies_with_SMC_2_no_version_claimed);

                        break;
                    case 0x02F5:
                        sb.AppendLine(Localization.Device_complies_with_SMC_2_T10_1383_D_revision_5);

                        break;
                    case 0x02FC:
                        sb.AppendLine(Localization.Device_complies_with_SMC_2_T10_1383_D_revision_6);

                        break;
                    case 0x02FD:
                        sb.AppendLine(Localization.Device_complies_with_SMC_2_T10_1383_D_revision_7);

                        break;
                    case 0x02FE:
                        sb.AppendLine(Localization.Device_complies_with_SMC_2_ANSI_INCITS_382_2004);

                        break;
                    case 0x0300:
                        sb.AppendLine(Localization.Device_complies_with_SPC_3_no_version_claimed);

                        break;
                    case 0x0301:
                        sb.AppendLine(Localization.Device_complies_with_SPC_3_T10_1416_D_revision_7);

                        break;
                    case 0x0307:
                        sb.AppendLine(Localization.Device_complies_with_SPC_3_T10_1416_D_revision_21);

                        break;
                    case 0x030F:
                        sb.AppendLine(Localization.Device_complies_with_SPC_3_T10_1416_D_revision_22);

                        break;
                    case 0x0312:
                        sb.AppendLine(Localization.Device_complies_with_SPC_3_T10_1416_D_revision_23);

                        break;
                    case 0x0314:
                        sb.AppendLine(Localization.Device_complies_with_SPC_3_ANSI_INCITS_408_2005);

                        break;
                    case 0x0316:
                        sb.AppendLine(Localization.Device_complies_with_SPC_3_ISO_IEC_14776_453);

                        break;
                    case 0x0320:
                        sb.AppendLine(Localization.Device_complies_with_SBC_2_no_version_claimed);

                        break;
                    case 0x0322:
                        sb.AppendLine(Localization.Device_complies_with_SBC_2_T10_1417_D_revision_5a);

                        break;
                    case 0x0324:
                        sb.AppendLine(Localization.Device_complies_with_SBC_2_T10_1417_D_revision_15);

                        break;
                    case 0x033B:
                        sb.AppendLine(Localization.Device_complies_with_SBC_2_T10_1417_D_revision_16);

                        break;
                    case 0x033D:
                        sb.AppendLine(Localization.Device_complies_with_SBC_2_ANSI_INCITS_405_2005);

                        break;
                    case 0x033E:
                        sb.AppendLine(Localization.Device_complies_with_SBC_2_ISO_IEC_14776_322);

                        break;
                    case 0x0340:
                        sb.AppendLine(Localization.Device_complies_with_OSD_no_version_claimed);

                        break;
                    case 0x0341:
                        sb.AppendLine(Localization.Device_complies_with_OSD_T10_1355_D_revision_0);

                        break;
                    case 0x0342:
                        sb.AppendLine(Localization.Device_complies_with_OSD_T10_1355_D_revision_7a);

                        break;
                    case 0x0343:
                        sb.AppendLine(Localization.Device_complies_with_OSD_T10_1355_D_revision_8);

                        break;
                    case 0x0344:
                        sb.AppendLine(Localization.Device_complies_with_OSD_T10_1355_D_revision_9);

                        break;
                    case 0x0355:
                        sb.AppendLine(Localization.Device_complies_with_OSD_T10_1355_D_revision_10);

                        break;
                    case 0x0356:
                        sb.AppendLine(Localization.Device_complies_with_OSD_ANSI_INCITS_400_2004);

                        break;
                    case 0x0360:
                        sb.AppendLine(Localization.Device_complies_with_SSC_2_no_version_claimed);

                        break;
                    case 0x0374:
                        sb.AppendLine(Localization.Device_complies_with_SSC_2_T10_1434_D_revision_7);

                        break;
                    case 0x0375:
                        sb.AppendLine(Localization.Device_complies_with_SSC_2_T10_1434_D_revision_9);

                        break;
                    case 0x037D:
                        sb.AppendLine(Localization.Device_complies_with_SSC_2_ANSI_INCITS_380_2003);

                        break;
                    case 0x0380:
                        sb.AppendLine(Localization.Device_complies_with_BCC_no_version_claimed);

                        break;
                    case 0x03A0:
                        sb.AppendLine(Localization.Device_complies_with_MMC_4_no_version_claimed);

                        break;
                    case 0x03B0:
                        sb.AppendLine(Localization.Device_complies_with_MMC_4_T10_1545_D_revision_5);

                        break;
                    case 0x03B1:
                        sb.AppendLine(Localization.Device_complies_with_MMC_4_T10_1545_D_revision_5a);

                        break;
                    case 0x03BD:
                        sb.AppendLine(Localization.Device_complies_with_MMC_4_T10_1545_D_revision_3);

                        break;
                    case 0x03BE:
                        sb.AppendLine(Localization.Device_complies_with_MMC_4_T10_1545_D_revision_3d);

                        break;
                    case 0x03BF:
                        sb.AppendLine(Localization.Device_complies_with_MMC_4_ANSI_INCITS_401_2005);

                        break;
                    case 0x03C0:
                        sb.AppendLine(Localization.Device_complies_with_ADC_no_version_claimed);

                        break;
                    case 0x03D5:
                        sb.AppendLine(Localization.Device_complies_with_ADC_T10_1558_D_revision_6);

                        break;
                    case 0x03D6:
                        sb.AppendLine(Localization.Device_complies_with_ADC_T10_1558_D_revision_7);

                        break;
                    case 0x03D7:
                        sb.AppendLine(Localization.Device_complies_with_ADC_ANSI_INCITS_403_2005);

                        break;
                    case 0x03E0:
                        sb.AppendLine(Localization.Device_complies_with_SES_2_no_version_claimed);

                        break;
                    case 0x03E1:
                        sb.AppendLine(Localization.Device_complies_with_SES_2_T10_1559_D_revision_16);

                        break;
                    case 0x03E7:
                        sb.AppendLine(Localization.Device_complies_with_SES_2_T10_1559_D_revision_19);

                        break;
                    case 0x03EB:
                        sb.AppendLine(Localization.Device_complies_with_SES_2_T10_1559_D_revision_20);

                        break;
                    case 0x03F0:
                        sb.AppendLine(Localization.Device_complies_with_SES_2_ANSI_INCITS_448_2008);

                        break;
                    case 0x03F2:
                        sb.AppendLine(Localization.Device_complies_with_SES_2_ISO_IEC_14776_372);

                        break;
                    case 0x0400:
                        sb.AppendLine(Localization.Device_complies_with_SSC_3_no_version_claimed);

                        break;
                    case 0x0403:
                        sb.AppendLine(Localization.Device_complies_with_SSC_3_T10_1611_D_revision_04a);

                        break;
                    case 0x0407:
                        sb.AppendLine(Localization.Device_complies_with_SSC_3_T10_1611_D_revision_05);

                        break;
                    case 0x0409:
                        sb.AppendLine(Localization.Device_complies_with_SSC_3_ANSI_INCITS_467_2011);

                        break;
                    case 0x040B:
                        sb.AppendLine(Localization.Device_complies_with_SSC_3_ISO_IEC_14776_333_2013);

                        break;
                    case 0x0420:
                        sb.AppendLine(Localization.Device_complies_with_MMC_5_no_version_claimed);

                        break;
                    case 0x042F:
                        sb.AppendLine(Localization.Device_complies_with_MMC_5_T10_1675_D_revision_03);

                        break;
                    case 0x0431:
                        sb.AppendLine(Localization.Device_complies_with_MMC_5_T10_1675_D_revision_03b);

                        break;
                    case 0x0432:
                        sb.AppendLine(Localization.Device_complies_with_MMC_5_T10_1675_D_revision_04);

                        break;
                    case 0x0434:
                        sb.AppendLine(Localization.Device_complies_with_MMC_5_ANSI_INCITS_430_2007);

                        break;
                    case 0x0440:
                        sb.AppendLine(Localization.Device_complies_with_OSD_2_no_version_claimed);

                        break;
                    case 0x0444:
                        sb.AppendLine(Localization.Device_complies_with_OSD_2_T10_1729_D_revision_4);

                        break;
                    case 0x0446:
                        sb.AppendLine(Localization.Device_complies_with_OSD_2_T10_1729_D_revision_5);

                        break;
                    case 0x0448:
                        sb.AppendLine(Localization.Device_complies_with_OSD_2_ANSI_INCITS_458_2011);

                        break;
                    case 0x0460:
                        sb.AppendLine(Localization.Device_complies_with_SPC_4_no_version_claimed);

                        break;
                    case 0x0461:
                        sb.AppendLine(Localization.Device_complies_with_SPC_4_T10_BSR_INCITS_513_revision_16);

                        break;
                    case 0x0462:
                        sb.AppendLine(Localization.Device_complies_with_SPC_4_T10_BSR_INCITS_513_revision_18);

                        break;
                    case 0x0463:
                        sb.AppendLine(Localization.Device_complies_with_SPC_4_T10_BSR_INCITS_513_revision_23);

                        break;
                    case 0x0466:
                        sb.AppendLine(Localization.Device_complies_with_SPC_4_T10_BSR_INCITS_513_revision_36);

                        break;
                    case 0x0468:
                        sb.AppendLine(Localization.Device_complies_with_SPC_4_T10_BSR_INCITS_513_revision_37);

                        break;
                    case 0x0469:
                        sb.AppendLine(Localization.Device_complies_with_SPC_4_T10_BSR_INCITS_513_revision_37a);

                        break;
                    case 0x046C:
                        sb.AppendLine(Localization.Device_complies_with_SPC_4_ANSI_INCITS_513_2015);

                        break;
                    case 0x0480:
                        sb.AppendLine(Localization.Device_complies_with_SMC_3_no_version_claimed);

                        break;
                    case 0x0482:
                        sb.AppendLine(Localization.Device_complies_with_SMC_3_T10_1730_D_revision_15);

                        break;
                    case 0x0484:
                        sb.AppendLine(Localization.Device_complies_with_SMC_3_T10_1730_D_revision_16);

                        break;
                    case 0x0486:
                        sb.AppendLine(Localization.Device_complies_with_SMC_3_ANSI_INCITS_484_2012);

                        break;
                    case 0x04A0:
                        sb.AppendLine(Localization.Device_complies_with_ADC_2_no_version_claimed);

                        break;
                    case 0x04A7:
                        sb.AppendLine(Localization.Device_complies_with_ADC_2_T10_1741_D_revision_7);

                        break;
                    case 0x04AA:
                        sb.AppendLine(Localization.Device_complies_with_ADC_2_T10_1741_D_revision_8);

                        break;
                    case 0x04AC:
                        sb.AppendLine(Localization.Device_complies_with_ADC_2_ANSI_INCITS_441_2008);

                        break;
                    case 0x04C0:
                        sb.AppendLine(Localization.Device_complies_with_SBC_3_no_version_claimed);

                        break;
                    case 0x04C3:
                        sb.AppendLine(Localization.Device_complies_with_SBC_3_T10_BSR_INCITS_514_revision_35);

                        break;
                    case 0x04C5:
                        sb.AppendLine(Localization.Device_complies_with_SBC_3_T10_BSR_INCITS_514_revision_36);

                        break;
                    case 0x04C8:
                        sb.AppendLine(Localization.Device_complies_with_SBC_3_ANSI_INCITS_514_2014);

                        break;
                    case 0x04E0:
                        sb.AppendLine(Localization.Device_complies_with_MMC_6_no_version_claimed);

                        break;
                    case 0x04E3:
                        sb.AppendLine(Localization.Device_complies_with_MMC_6_T10_1836_D_revision_02b);

                        break;
                    case 0x04E5:
                        sb.AppendLine(Localization.Device_complies_with_MMC_6_T10_1836_D_revision_02g);

                        break;
                    case 0x04E6:
                        sb.AppendLine(Localization.Device_complies_with_MMC_6_ANSI_INCITS_468_2010);

                        break;
                    case 0x04E7:
                        sb.AppendLine(Localization.
                                          Device_complies_with_MMC_6_ANSI_INCITS_468_2010_MMC_6_AM1_ANSI_INCITS_468_2010_AM_1);

                        break;
                    case 0x0500:
                        sb.AppendLine(Localization.Device_complies_with_ADC_3_no_version_claimed);

                        break;
                    case 0x0502:
                        sb.AppendLine(Localization.Device_complies_with_ADC_3_T10_1895_D_revision_04);

                        break;
                    case 0x0504:
                        sb.AppendLine(Localization.Device_complies_with_ADC_3_T10_1895_D_revision_05);

                        break;
                    case 0x0506:
                        sb.AppendLine(Localization.Device_complies_with_ADC_3_T10_1895_D_revision_05a);

                        break;
                    case 0x050A:
                        sb.AppendLine(Localization.Device_complies_with_ADC_3_ANSI_INCITS_497_2012);

                        break;
                    case 0x0520:
                        sb.AppendLine(Localization.Device_complies_with_SSC_4_no_version_claimed);

                        break;
                    case 0x0523:
                        sb.AppendLine(Localization.Device_complies_with_SSC_4_T10_BSR_INCITS_516_revision_2);

                        break;
                    case 0x0525:
                        sb.AppendLine(Localization.Device_complies_with_SSC_4_T10_BSR_INCITS_516_revision_3);

                        break;
                    case 0x0527:
                        sb.AppendLine(Localization.Device_complies_with_SSC_4_ANSI_INCITS_516_2013);

                        break;
                    case 0x0560:
                        sb.AppendLine(Localization.Device_complies_with_OSD_3_no_version_claimed);

                        break;
                    case 0x0580:
                        sb.AppendLine(Localization.Device_complies_with_SES_3_no_version_claimed);

                        break;
                    case 0x05A0:
                        sb.AppendLine(Localization.Device_complies_with_SSC_5_no_version_claimed);

                        break;
                    case 0x05C0:
                        sb.AppendLine(Localization.Device_complies_with_SPC_5_no_version_claimed);

                        break;
                    case 0x05E0:
                        sb.AppendLine(Localization.Device_complies_with_SFSC_no_version_claimed);

                        break;
                    case 0x05E3:
                        sb.AppendLine(Localization.Device_complies_with_SFSC_BSR_INCITS_501_revision_01);

                        break;
                    case 0x0600:
                        sb.AppendLine(Localization.Device_complies_with_SBC_4_no_version_claimed);

                        break;
                    case 0x0620:
                        sb.AppendLine(Localization.Device_complies_with_ZBC_no_version_claimed);

                        break;
                    case 0x0622:
                        sb.AppendLine(Localization.Device_complies_with_ZBC_BSR_INCITS_536_revision_02);

                        break;
                    case 0x0640:
                        sb.AppendLine(Localization.Device_complies_with_ADC_4_no_version_claimed);

                        break;
                    case 0x0820:
                        sb.AppendLine(Localization.Device_complies_with_SSA_TL2_no_version_claimed);

                        break;
                    case 0x083B:
                        sb.AppendLine(Localization.Device_complies_with_SSA_TL2_T10_1_1147_D_revision_05b);

                        break;
                    case 0x083C:
                        sb.AppendLine(Localization.Device_complies_with_SSA_TL2_ANSI_INCITS_308_1998);

                        break;
                    case 0x0840:
                        sb.AppendLine(Localization.Device_complies_with_SSA_TL1_no_version_claimed);

                        break;
                    case 0x085B:
                        sb.AppendLine(Localization.Device_complies_with_SSA_TL1_T10_1_0989_D_revision_10b);

                        break;
                    case 0x085C:
                        sb.AppendLine(Localization.Device_complies_with_SSA_TL1_ANSI_INCITS_295_1996);

                        break;
                    case 0x0860:
                        sb.AppendLine(Localization.Device_complies_with_SSA_S3P_no_version_claimed);

                        break;
                    case 0x087B:
                        sb.AppendLine(Localization.Device_complies_with_SSA_S3P_T10_1_1051_D_revision_05b);

                        break;
                    case 0x087C:
                        sb.AppendLine(Localization.Device_complies_with_SSA_S3P_ANSI_INCITS_309_1998);

                        break;
                    case 0x0880:
                        sb.AppendLine(Localization.Device_complies_with_SSA_S2P_no_version_claimed);

                        break;
                    case 0x089B:
                        sb.AppendLine(Localization.Device_complies_with_SSA_S2P_T10_1_1121_D_revision_07b);

                        break;
                    case 0x089C:
                        sb.AppendLine(Localization.Device_complies_with_SSA_S2P_ANSI_INCITS_294_1996);

                        break;
                    case 0x08A0:
                        sb.AppendLine(Localization.Device_complies_with_SIP_no_version_claimed);

                        break;
                    case 0x08BB:
                        sb.AppendLine(Localization.Device_complies_with_SIP_T10_0856_D_revision_10);

                        break;
                    case 0x08BC:
                        sb.AppendLine(Localization.Device_complies_with_SIP_ANSI_INCITS_292_1997);

                        break;
                    case 0x08C0:
                        sb.AppendLine(Localization.Device_complies_with_FCP_no_version_claimed);

                        break;
                    case 0x08DB:
                        sb.AppendLine(Localization.Device_complies_with_FCP_T10_0993_D_revision_12);

                        break;
                    case 0x08DC:
                        sb.AppendLine(Localization.Device_complies_with_FCP_ANSI_INCITS_269_1996);

                        break;
                    case 0x08E0:
                        sb.AppendLine(Localization.Device_complies_with_SBP_2_no_version_claimed);

                        break;
                    case 0x08FB:
                        sb.AppendLine(Localization.Device_complies_with_SBP_2_T10_1155_D_revision_04);

                        break;
                    case 0x08FC:
                        sb.AppendLine(Localization.Device_complies_with_SBP_2_ANSI_INCITS_325_1998);

                        break;
                    case 0x0900:
                        sb.AppendLine(Localization.Device_complies_with_FCP_2_no_version_claimed);

                        break;
                    case 0x0901:
                        sb.AppendLine(Localization.Device_complies_with_FCP_2_T10_1144_D_revision_4);

                        break;
                    case 0x0915:
                        sb.AppendLine(Localization.Device_complies_with_FCP_2_T10_1144_D_revision_7);

                        break;
                    case 0x0916:
                        sb.AppendLine(Localization.Device_complies_with_FCP_2_T10_1144_D_revision_7a);

                        break;
                    case 0x0917:
                        sb.AppendLine(Localization.Device_complies_with_FCP_2_ANSI_INCITS_350_2003);

                        break;
                    case 0x0918:
                        sb.AppendLine(Localization.Device_complies_with_FCP_2_T10_1144_D_revision_8);

                        break;
                    case 0x0920:
                        sb.AppendLine(Localization.Device_complies_with_SST_no_version_claimed);

                        break;
                    case 0x0935:
                        sb.AppendLine(Localization.Device_complies_with_SST_T10_1380_D_revision_8b);

                        break;
                    case 0x0940:
                        sb.AppendLine(Localization.Device_complies_with_SRP_no_version_claimed);

                        break;
                    case 0x0954:
                        sb.AppendLine(Localization.Device_complies_with_SRP_T10_1415_D_revision_10);

                        break;
                    case 0x0955:
                        sb.AppendLine(Localization.Device_complies_with_SRP_T10_1415_D_revision_16a);

                        break;
                    case 0x095C:
                        sb.AppendLine(Localization.Device_complies_with_SRP_ANSI_INCITS_365_2002);

                        break;
                    case 0x0960:
                        sb.AppendLine(Localization.Device_complies_with_iSCSI_no_version_claimed);

                        break;
                    case 0x0961:
                    case 0x0962:
                    case 0x0963:
                    case 0x0964:
                    case 0x0965:
                    case 0x0966:
                    case 0x0967:
                    case 0x0968:
                    case 0x0969:
                    case 0x096A:
                    case 0x096B:
                    case 0x096C:
                    case 0x096D:
                    case 0x096E:
                    case 0x096F:
                    case 0x0970:
                    case 0x0971:
                    case 0x0972:
                    case 0x0973:
                    case 0x0974:
                    case 0x0975:
                    case 0x0976:
                    case 0x0977:
                    case 0x0978:
                    case 0x0979:
                    case 0x097A:
                    case 0x097B:
                    case 0x097C:
                    case 0x097D:
                    case 0x097E:
                    case 0x097F:
                        sb.AppendFormat(Localization.Device_complies_with_iSCSI_revision_0, VersionDescriptor & 0x1F).
                           AppendLine();

                        break;
                    case 0x0980:
                        sb.AppendLine(Localization.Device_complies_with_SBP_3_no_version_claimed);

                        break;
                    case 0x0982:
                        sb.AppendLine(Localization.Device_complies_with_SBP_3_T10_1467_D_revision_1f);

                        break;
                    case 0x0994:
                        sb.AppendLine(Localization.Device_complies_with_SBP_3_T10_1467_D_revision_3);

                        break;
                    case 0x099A:
                        sb.AppendLine(Localization.Device_complies_with_SBP_3_T10_1467_D_revision_4);

                        break;
                    case 0x099B:
                        sb.AppendLine(Localization.Device_complies_with_SBP_3_T10_1467_D_revision_5);

                        break;
                    case 0x099C:
                        sb.AppendLine(Localization.Device_complies_with_SBP_3_ANSI_INCITS_375_2004);

                        break;
                    case 0x09C0:
                        sb.AppendLine(Localization.Device_complies_with_ADP_no_version_claimed);

                        break;
                    case 0x09E0:
                        sb.AppendLine(Localization.Device_complies_with_ADT_no_version_claimed);

                        break;
                    case 0x09F9:
                        sb.AppendLine(Localization.Device_complies_with_ADT_T10_1557_D_revision_11);

                        break;
                    case 0x09FA:
                        sb.AppendLine(Localization.Device_complies_with_ADT_T10_1557_D_revision_14);

                        break;
                    case 0x09FD:
                        sb.AppendLine(Localization.Device_complies_with_ADT_ANSI_INCITS_406_2005);

                        break;
                    case 0x0A00:
                        sb.AppendLine(Localization.Device_complies_with_FCP_3_no_version_claimed);

                        break;
                    case 0x0A07:
                        sb.AppendLine(Localization.Device_complies_with_FCP_3_T10_1560_D_revision_3f);

                        break;
                    case 0x0A0F:
                        sb.AppendLine(Localization.Device_complies_with_FCP_3_T10_1560_D_revision_4);

                        break;
                    case 0x0A11:
                        sb.AppendLine(Localization.Device_complies_with_FCP_3_ANSI_INCITS_416_2006);

                        break;
                    case 0x0A1C:
                        sb.AppendLine(Localization.Device_complies_with_FCP_3_ISO_IEC_14776_223);

                        break;
                    case 0x0A20:
                        sb.AppendLine(Localization.Device_complies_with_ADT_2_no_version_claimed);

                        break;
                    case 0x0A22:
                        sb.AppendLine(Localization.Device_complies_with_ADT_2_T10_1742_D_revision_06);

                        break;
                    case 0x0A27:
                        sb.AppendLine(Localization.Device_complies_with_ADT_2_T10_1742_D_revision_08);

                        break;
                    case 0x0A28:
                        sb.AppendLine(Localization.Device_complies_with_ADT_2_T10_1742_D_revision_09);

                        break;
                    case 0x0A2B:
                        sb.AppendLine(Localization.Device_complies_with_ADT_2_ANSI_INCITS_472_2011);

                        break;
                    case 0x0A40:
                        sb.AppendLine(Localization.Device_complies_with_FCP_4_no_version_claimed);

                        break;
                    case 0x0A42:
                        sb.AppendLine(Localization.Device_complies_with_FCP_4_T10_1828_D_revision_01);

                        break;
                    case 0x0A44:
                        sb.AppendLine(Localization.Device_complies_with_FCP_4_T10_1828_D_revision_02);

                        break;
                    case 0x0A45:
                        sb.AppendLine(Localization.Device_complies_with_FCP_4_T10_1828_D_revision_02b);

                        break;
                    case 0x0A46:
                        sb.AppendLine(Localization.Device_complies_with_FCP_4_ANSI_INCITS_481_2012);

                        break;
                    case 0x0A60:
                        sb.AppendLine(Localization.Device_complies_with_ADT_3_no_version_claimed);

                        break;
                    case 0x0AA0:
                        sb.AppendLine(Localization.Device_complies_with_SPI_no_version_claimed);

                        break;
                    case 0x0AB9:
                        sb.AppendLine(Localization.Device_complies_with_SPI_T10_0855_D_revision_15a);

                        break;
                    case 0x0ABA:
                        sb.AppendLine(Localization.Device_complies_with_SPI_ANSI_INCITS_253_1995);

                        break;
                    case 0x0ABB:
                        sb.AppendLine(Localization.
                                          Device_complies_with_SPI_T10_0855_D_revision_15a_with_SPI_Amnd_revision_3a);

                        break;
                    case 0x0ABC:
                        sb.AppendLine(Localization.
                                          Device_complies_with_SPI_ANSI_INCITS_253_1995_with_SPI_Amnd_ANSI_INCITS_253_AM1_1998);

                        break;
                    case 0x0AC0:
                        sb.AppendLine(Localization.Device_complies_with_Fast_20_no_version_claimed);

                        break;
                    case 0x0ADB:
                        sb.AppendLine(Localization.Device_complies_with_Fast_20_T10_1071_revision_06);

                        break;
                    case 0x0ADC:
                        sb.AppendLine(Localization.Device_complies_with_Fast_20_ANSI_INCITS_277_1996);

                        break;
                    case 0x0AE0:
                        sb.AppendLine(Localization.Device_complies_with_SPI_2_no_version_claimed);

                        break;
                    case 0x0AFB:
                        sb.AppendLine(Localization.Device_complies_with_SPI_2_T10_1142_D_revision_20b);

                        break;
                    case 0x0AFC:
                        sb.AppendLine(Localization.Device_complies_with_SPI_2_ANSI_INCITS_302_1999);

                        break;
                    case 0x0B00:
                        sb.AppendLine(Localization.Device_complies_with_SPI_3_no_version_claimed);

                        break;
                    case 0x0B18:
                        sb.AppendLine(Localization.Device_complies_with_SPI_3_T10_1302_D_revision_10);

                        break;
                    case 0x0B19:
                        sb.AppendLine(Localization.Device_complies_with_SPI_3_T10_1302_D_revision_13a);

                        break;
                    case 0x0B1A:
                        sb.AppendLine(Localization.Device_complies_with_SPI_3_T10_1302_D_revision_14);

                        break;
                    case 0x0B1C:
                        sb.AppendLine(Localization.Device_complies_with_SPI_3_ANSI_INCITS_336_2000);

                        break;
                    case 0x0B20:
                        sb.AppendLine(Localization.Device_complies_with_EPI_no_version_claimed);

                        break;
                    case 0x0B3B:
                        sb.AppendLine(Localization.Device_complies_with_EPI_T10_1134_revision_16);

                        break;
                    case 0x0B3C:
                        sb.AppendLine(Localization.Device_complies_with_EPI_ANSI_INCITS_TR_23_1999);

                        break;
                    case 0x0B40:
                        sb.AppendLine(Localization.Device_complies_with_SPI_4_no_version_claimed);

                        break;
                    case 0x0B54:
                        sb.AppendLine(Localization.Device_complies_with_SPI_4_T10_1365_D_revision_7);

                        break;
                    case 0x0B55:
                        sb.AppendLine(Localization.Device_complies_with_SPI_4_T10_1365_D_revision_9);

                        break;
                    case 0x0B56:
                        sb.AppendLine(Localization.Device_complies_with_SPI_4_ANSI_INCITS_362_2002);

                        break;
                    case 0x0B59:
                        sb.AppendLine(Localization.Device_complies_with_SPI_4_T10_1365_D_revision_10);

                        break;
                    case 0x0B60:
                        sb.AppendLine(Localization.Device_complies_with_SPI_5_no_version_claimed);

                        break;
                    case 0x0B79:
                        sb.AppendLine(Localization.Device_complies_with_SPI_5_T10_1525_D_revision_3);

                        break;
                    case 0x0B7A:
                        sb.AppendLine(Localization.Device_complies_with_SPI_5_T10_1525_D_revision_5);

                        break;
                    case 0x0B7B:
                        sb.AppendLine(Localization.Device_complies_with_SPI_5_T10_1525_D_revision_6);

                        break;
                    case 0x0B7C:
                        sb.AppendLine(Localization.Device_complies_with_SPI_5_ANSI_INCITS_367_2003);

                        break;
                    case 0x0BE0:
                        sb.AppendLine(Localization.Device_complies_with_SAS_no_version_claimed);

                        break;
                    case 0x0BE1:
                        sb.AppendLine(Localization.Device_complies_with_SAS_T10_1562_D_revision_01);

                        break;
                    case 0x0BF5:
                        sb.AppendLine(Localization.Device_complies_with_SAS_T10_1562_D_revision_03);

                        break;
                    case 0x0BFA:
                        sb.AppendLine(Localization.Device_complies_with_SAS_T10_1562_D_revision_04);

                        break;
                    case 0x0BFB:
                        sb.AppendLine(Localization.Device_complies_with_SAS_T10_1562_D_revision_04);

                        break;
                    case 0x0BFC:
                        sb.AppendLine(Localization.Device_complies_with_SAS_T10_1562_D_revision_05);

                        break;
                    case 0x0BFD:
                        sb.AppendLine(Localization.Device_complies_with_SAS_ANSI_INCITS_376_2003);

                        break;
                    case 0x0C00:
                        sb.AppendLine(Localization.Device_complies_with_SAS_1_1_no_version_claimed);

                        break;
                    case 0x0C07:
                        sb.AppendLine(Localization.Device_complies_with_SAS_1_1_T10_1601_D_revision_9);

                        break;
                    case 0x0C0F:
                        sb.AppendLine(Localization.Device_complies_with_SAS_1_1_T10_1601_D_revision_10);

                        break;
                    case 0x0C11:
                        sb.AppendLine(Localization.Device_complies_with_SAS_1_1_ANSI_INCITS_417_2006);

                        break;
                    case 0x0C12:
                        sb.AppendLine(Localization.Device_complies_with_SAS_1_1_ISO_IEC_14776_151);

                        break;
                    case 0x0C20:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_no_version_claimed);

                        break;
                    case 0x0C23:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_T10_1760_D_revision_14);

                        break;
                    case 0x0C27:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_T10_1760_D_revision_15);

                        break;
                    case 0x0C28:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_T10_1760_D_revision_16);

                        break;
                    case 0x0C2A:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_ANSI_INCITS_457_2010);

                        break;
                    case 0x0C40:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_1_no_version_claimed);

                        break;
                    case 0x0C48:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_1_T10_2125_D_revision_04);

                        break;
                    case 0x0C4A:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_1_T10_2125_D_revision_06);

                        break;
                    case 0x0C4B:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_1_T10_2125_D_revision_07);

                        break;
                    case 0x0C4E:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_1_ANSI_INCITS_478_2011);

                        break;
                    case 0x0C4F:
                        sb.AppendLine(Localization.
                                          Device_complies_with_SAS_2_1_ANSI_INCITS_478_2011_w__Amnd_1_ANSI_INCITS_478_AM1_2014);

                        break;
                    case 0x0C52:
                        sb.AppendLine(Localization.Device_complies_with_SAS_2_1_ISO_IEC_14776_153);

                        break;
                    case 0x0C60:
                        sb.AppendLine(Localization.Device_complies_with_SAS_3_no_version_claimed);

                        break;
                    case 0x0C63:
                        sb.AppendLine(Localization.Device_complies_with_SAS_3_T10_BSR_INCITS_519_revision_05a);

                        break;
                    case 0x0C65:
                        sb.AppendLine(Localization.Device_complies_with_SAS_3_T10_BSR_INCITS_519_revision_06);

                        break;
                    case 0x0C68:
                        sb.AppendLine(Localization.Device_complies_with_SAS_3_ANSI_INCITS_519_2014);

                        break;
                    case 0x0C80:
                        sb.AppendLine(Localization.Device_complies_with_SAS_4_no_version_claimed);

                        break;
                    case 0x0D20:
                        sb.AppendLine(Localization.Device_complies_with_FC_PH_no_version_claimed);

                        break;
                    case 0x0D3B:
                        sb.AppendLine(Localization.Device_complies_with_FC_PH_ANSI_INCITS_230_1994);

                        break;
                    case 0x0D3C:
                        sb.AppendLine(Localization.
                                          Device_complies_with_FC_PH_ANSI_INCITS_230_1994_with_Amnd_1_ANSI_INCITS_230_AM1_1996);

                        break;
                    case 0x0D40:
                        sb.AppendLine(Localization.Device_complies_with_FC_AL_no_version_claimed);

                        break;
                    case 0x0D5C:
                        sb.AppendLine(Localization.Device_complies_with_FC_AL_ANSI_INCITS_272_1996);

                        break;
                    case 0x0D60:
                        sb.AppendLine(Localization.Device_complies_with_FC_AL_2_no_version_claimed);

                        break;
                    case 0x0D61:
                        sb.AppendLine(Localization.Device_complies_with_FC_AL_2_T11_1133_D_revision_7_0);

                        break;
                    case 0x0D63:
                        sb.AppendLine(Localization.
                                          Device_complies_with_FC_AL_2_ANSI_INCITS_332_1999_with_AM1_2003___AM2_2006);

                        break;
                    case 0x0D64:
                        sb.AppendLine(Localization.
                                          Device_complies_with_FC_AL_2_ANSI_INCITS_332_1999_with_Amnd_2_AM2_2006);

                        break;
                    case 0x0D65:
                        sb.AppendLine(Localization.Device_complies_with_FC_AL_2_ISO_IEC_14165_122_with_AM1___AM2);

                        break;
                    case 0x0D7C:
                        sb.AppendLine(Localization.Device_complies_with_FC_AL_2_ANSI_INCITS_332_1999);

                        break;
                    case 0x0D7D:
                        sb.AppendLine(Localization.
                                          Device_complies_with_FC_AL_2_ANSI_INCITS_332_1999_with_Amnd_1_AM1_2003);

                        break;
                    case 0x0D80:
                        sb.AppendLine(Localization.Device_complies_with_FC_PH_3_no_version_claimed);

                        break;
                    case 0x0D9C:
                        sb.AppendLine(Localization.Device_complies_with_FC_PH_3_ANSI_INCITS_303_1998);

                        break;
                    case 0x0DA0:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_no_version_claimed);

                        break;
                    case 0x0DB7:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_T11_1331_D_revision_1_2);

                        break;
                    case 0x0DB8:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_T11_1331_D_revision_1_7);

                        break;
                    case 0x0DBC:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_ANSI_INCITS_373_2003);

                        break;
                    case 0x0DBD:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_ISO_IEC_14165_251);

                        break;
                    case 0x0DC0:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_no_version_claimed);

                        break;
                    case 0x0DDC:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_ANSI_INCITS_352_2002);

                        break;
                    case 0x0DE0:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_2_no_version_claimed);

                        break;
                    case 0x0DE2:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_2_T11_1506_D_revision_5_0);

                        break;
                    case 0x0DE4:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_2_ANSI_INCITS_404_2006);

                        break;
                    case 0x0E00:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_2_no_version_claimed);

                        break;
                    case 0x0E02:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_2_ANSI_INCITS_242_2007);

                        break;
                    case 0x0E03:
                        sb.AppendLine(Localization.
                                          Device_complies_with_FC_FS_2_ANSI_INCITS_242_2007_with_AM1_ANSI_INCITS_242_AM1_2007);

                        break;
                    case 0x0E20:
                        sb.AppendLine(Localization.Device_complies_with_FC_LS_no_version_claimed);

                        break;
                    case 0x0E21:
                        sb.AppendLine(Localization.Device_complies_with_FC_LS_T11_1620_D_revision_1_62);

                        break;
                    case 0x0E29:
                        sb.AppendLine(Localization.Device_complies_with_FC_LS_ANSI_INCITS_433_2007);

                        break;
                    case 0x0E40:
                        sb.AppendLine(Localization.Device_complies_with_FC_SP_no_version_claimed);

                        break;
                    case 0x0E42:
                        sb.AppendLine(Localization.Device_complies_with_FC_SP_T11_1570_D_revision_1_6);

                        break;
                    case 0x0E45:
                        sb.AppendLine(Localization.Device_complies_with_FC_SP_ANSI_INCITS_426_2007);

                        break;
                    case 0x0E60:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_3_no_version_claimed);

                        break;
                    case 0x0E62:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_3_T11_1625_D_revision_2_0);

                        break;
                    case 0x0E68:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_3_T11_1625_D_revision_2_1);

                        break;
                    case 0x0E6A:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_3_T11_1625_D_revision_4_0);

                        break;
                    case 0x0E6E:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_3_ANSI_INCITS_460_2011);

                        break;
                    case 0x0E80:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_4_no_version_claimed);

                        break;
                    case 0x0E82:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_4_T11_1647_D_revision_8_0);

                        break;
                    case 0x0E88:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_4_ANSI_INCITS_450_2009);

                        break;
                    case 0x0EA0:
                        sb.AppendLine(Localization.Device_complies_with_FC_10GFC_no_version_claimed);

                        break;
                    case 0x0EA2:
                        sb.AppendLine(Localization.Device_complies_with_FC_10GFC_ANSI_INCITS_364_2003);

                        break;
                    case 0x0EA3:
                        sb.AppendLine(Localization.Device_complies_with_FC_10GFC_ISO_IEC_14165_116);

                        break;
                    case 0x0EA5:
                        sb.AppendLine(Localization.Device_complies_with_FC_10GFC_ISO_IEC_14165_116_with_AM1);

                        break;
                    case 0x0EA6:
                        sb.AppendLine(Localization.
                                          Device_complies_with_FC_10GFC_ANSI_INCITS_364_2003_with_AM1_ANSI_INCITS_364_AM1_2007);

                        break;
                    case 0x0EC0:
                        sb.AppendLine(Localization.Device_complies_with_FC_SP_2_no_version_claimed);

                        break;
                    case 0x0EE0:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_3_no_version_claimed);

                        break;
                    case 0x0EE2:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_3_T11_1861_D_revision_0_9);

                        break;
                    case 0x0EE7:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_3_T11_1861_D_revision_1_0);

                        break;
                    case 0x0EE9:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_3_T11_1861_D_revision_1_10);

                        break;
                    case 0x0EEB:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_3_ANSI_INCITS_470_2011);

                        break;
                    case 0x0F00:
                        sb.AppendLine(Localization.Device_complies_with_FC_LS_2_no_version_claimed);

                        break;
                    case 0x0F03:
                        sb.AppendLine(Localization.Device_complies_with_FC_LS_2_T11_2103_D_revision_2_11);

                        break;
                    case 0x0F05:
                        sb.AppendLine(Localization.Device_complies_with_FC_LS_2_T11_2103_D_revision_2_21);

                        break;
                    case 0x0F07:
                        sb.AppendLine(Localization.Device_complies_with_FC_LS_2_ANSI_INCITS_477_2011);

                        break;
                    case 0x0F20:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_5_no_version_claimed);

                        break;
                    case 0x0F27:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_5_T11_2118_D_revision_2_00);

                        break;
                    case 0x0F28:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_5_T11_2118_D_revision_3_00);

                        break;
                    case 0x0F2A:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_5_T11_2118_D_revision_6_00);

                        break;
                    case 0x0F2B:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_5_T11_2118_D_revision_6_10);

                        break;
                    case 0x0F2E:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_5_ANSI_INCITS_479_2011);

                        break;
                    case 0x0F40:
                        sb.AppendLine(Localization.Device_complies_with_FC_PI_6_no_version_claimed);

                        break;
                    case 0x0F60:
                        sb.AppendLine(Localization.Device_complies_with_FC_FS_4_no_version_claimed);

                        break;
                    case 0x0F80:
                        sb.AppendLine(Localization.Device_complies_with_FC_LS_3_no_version_claimed);

                        break;
                    case 0x12A0:
                        sb.AppendLine(Localization.Device_complies_with_FC_SCM_no_version_claimed);

                        break;
                    case 0x12A3:
                        sb.AppendLine(Localization.Device_complies_with_FC_SCM_T11_1824DT_revision_1_0);

                        break;
                    case 0x12A5:
                        sb.AppendLine(Localization.Device_complies_with_FC_SCM_T11_1824DT_revision_1_1);

                        break;
                    case 0x12A7:
                        sb.AppendLine(Localization.Device_complies_with_FC_SCM_T11_1824DT_revision_1_4);

                        break;
                    case 0x12AA:
                        sb.AppendLine(Localization.Device_complies_with_FC_SCM_INCITS_TR_47_2012);

                        break;
                    case 0x12C0:
                        sb.AppendLine(Localization.Device_complies_with_FC_DA_2_no_version_claimed);

                        break;
                    case 0x12C3:
                        sb.AppendLine(Localization.Device_complies_with_FC_DA_2_T11_1870DT_revision_1_04);

                        break;
                    case 0x12C5:
                        sb.AppendLine(Localization.Device_complies_with_FC_DA_2_T11_1870DT_revision_1_06);

                        break;
                    case 0x12C9:
                        sb.AppendLine(Localization.Device_complies_with_FC_DA_2_INCITS_TR_49_2012);

                        break;
                    case 0x12E0:
                        sb.AppendLine(Localization.Device_complies_with_FC_DA_no_version_claimed);

                        break;
                    case 0x12E2:
                        sb.AppendLine(Localization.Device_complies_with_FC_DA_T11_1513_DT_revision_3_1);

                        break;
                    case 0x12E8:
                        sb.AppendLine(Localization.Device_complies_with_FC_DA_ANSI_INCITS_TR_36_2004);

                        break;
                    case 0x12E9:
                        sb.AppendLine(Localization.Device_complies_with_FC_DA_ISO_IEC_14165_341);

                        break;
                    case 0x1300:
                        sb.AppendLine(Localization.Device_complies_with_FC_Tape_no_version_claimed);

                        break;
                    case 0x1301:
                        sb.AppendLine(Localization.Device_complies_with_FC_Tape_T11_1315_revision_1_16);

                        break;
                    case 0x131B:
                        sb.AppendLine(Localization.Device_complies_with_FC_Tape_T11_1315_revision_1_17);

                        break;
                    case 0x131C:
                        sb.AppendLine(Localization.Device_complies_with_FC_Tape_ANSI_INCITS_TR_24_1999);

                        break;
                    case 0x1320:
                        sb.AppendLine(Localization.Device_complies_with_FC_FLA_no_version_claimed);

                        break;
                    case 0x133B:
                        sb.AppendLine(Localization.Device_complies_with_FC_FLA_T11_1235_revision_7);

                        break;
                    case 0x133C:
                        sb.AppendLine(Localization.Device_complies_with_FC_FLA_ANSI_INCITS_TR_20_1998);

                        break;
                    case 0x1340:
                        sb.AppendLine(Localization.Device_complies_with_FC_PLDA_no_version_claimed);

                        break;
                    case 0x135B:
                        sb.AppendLine(Localization.Device_complies_with_FC_PLDA_T11_1162_revision_2_1);

                        break;
                    case 0x135C:
                        sb.AppendLine(Localization.Device_complies_with_FC_PLDA_ANSI_INCITS_TR_19_1998);

                        break;
                    case 0x1360:
                        sb.AppendLine(Localization.Device_complies_with_SSA_PH2_no_version_claimed);

                        break;
                    case 0x137B:
                        sb.AppendLine(Localization.Device_complies_with_SSA_PH2_T10_1_1145_D_revision_09c);

                        break;
                    case 0x137C:
                        sb.AppendLine(Localization.Device_complies_with_SSA_PH2_ANSI_INCITS_293_1996);

                        break;
                    case 0x1380:
                        sb.AppendLine(Localization.Device_complies_with_SSA_PH3_no_version_claimed);

                        break;
                    case 0x139B:
                        sb.AppendLine(Localization.Device_complies_with_SSA_PH3_T10_1_1146_D_revision_05b);

                        break;
                    case 0x139C:
                        sb.AppendLine(Localization.Device_complies_with_SSA_PH3_ANSI_INCITS_307_1998);

                        break;
                    case 0x14A0:
                        sb.AppendLine(Localization.Device_complies_with_IEEE_1394_no_version_claimed);

                        break;
                    case 0x14BD:
                        sb.AppendLine(Localization.Device_complies_with_ANSI_IEEE_1394_1995);

                        break;
                    case 0x14C0:
                        sb.AppendLine(Localization.Device_complies_with_IEEE_1394a_no_version_claimed);

                        break;
                    case 0x14E0:
                        sb.AppendLine(Localization.Device_complies_with_IEEE_1394b_no_version_claimed);

                        break;
                    case 0x15E0:
                        sb.AppendLine(Localization.Device_complies_with_ATA_ATAPI_6_no_version_claimed);

                        break;
                    case 0x15FD:
                        sb.AppendLine(Localization.Device_complies_with_ATA_ATAPI_6_ANSI_INCITS_361_2002);

                        break;
                    case 0x1600:
                        sb.AppendLine(Localization.Device_complies_with_ATA_ATAPI_7_no_version_claimed);

                        break;
                    case 0x1602:
                        sb.AppendLine(Localization.Device_complies_with_ATA_ATAPI_7_T13_1532_D_revision_3);

                        break;
                    case 0x161C:
                        sb.AppendLine(Localization.Device_complies_with_ATA_ATAPI_7_ANSI_INCITS_397_2005);

                        break;
                    case 0x161E:
                        sb.AppendLine(Localization.Device_complies_with_ATA_ATAPI_7_ISO_IEC_24739);

                        break;
                    case 0x1620:
                        sb.AppendLine(Localization.Device_complies_with_ATA_ATAPI_8_ATA8_AAM_no_version_claimed);

                        break;
                    case 0x1621:
                        sb.AppendLine(Localization.
                                          Device_complies_with_ATA_ATAPI_8_ATA8_APT_Parallel_Transport_no_version_claimed);

                        break;
                    case 0x1622:
                        sb.AppendLine(Localization.
                                          Device_complies_with_ATA_ATAPI_8_ATA8_AST_Serial_Transport_no_version_claimed);

                        break;
                    case 0x1623:
                        sb.AppendLine(Localization.
                                          Device_complies_with_ATA_ATAPI_8_ATA8_ACS_ATA_ATAPI_Command_Set_no_version_claimed);

                        break;
                    case 0x1628:
                        sb.AppendLine(Localization.Device_complies_with_ATA_ATAPI_8_ATA8_AAM_ANSI_INCITS_451_2008);

                        break;
                    case 0x162A:
                        sb.AppendLine(Localization.
                                          Device_complies_with_ATA_ATAPI_8_ATA8_ACS_ANSI_INCITS_452_2009_w__Amendment_1);

                        break;
                    case 0x1728:
                        sb.AppendLine(Localization.
                                          Device_complies_with_Universal_Serial_Bus_Specification__Revision_1_1);

                        break;
                    case 0x1729:
                        sb.AppendLine(Localization.
                                          Device_complies_with_Universal_Serial_Bus_Specification__Revision_2_0);

                        break;
                    case 0x1730:
                        sb.AppendLine(Localization.
                                          Device_complies_with_USB_Mass_Storage_Class_Bulk_Only_Transport__Revision_1_0);

                        break;
                    case 0x1740:
                        sb.AppendLine(Localization.Device_complies_with_UAS_no_version_claimed);

                        break;
                    case 0x1743:
                        sb.AppendLine(Localization.Device_complies_with_UAS_T10_2095_D_revision_02);

                        break;
                    case 0x1747:
                        sb.AppendLine(Localization.Device_complies_with_UAS_T10_2095_D_revision_04);

                        break;
                    case 0x1748:
                        sb.AppendLine(Localization.Device_complies_with_UAS_ANSI_INCITS_471_2010);

                        break;
                    case 0x1749:
                        sb.AppendLine(Localization.Device_complies_with_UAS_ISO_IEC_14776_251_2014);

                        break;
                    case 0x1761:
                        sb.AppendLine(Localization.Device_complies_with_ACS_2_no_version_claimed);

                        break;
                    case 0x1762:
                        sb.AppendLine(Localization.Device_complies_with_ACS_2_ANSI_INCITS_482_2013);

                        break;
                    case 0x1765:
                        sb.AppendLine(Localization.Device_complies_with_ACS_3_no_version_claimed);

                        break;
                    case 0x1780:
                        sb.AppendLine(Localization.Device_complies_with_UAS_2_no_version_claimed);

                        break;
                    case 0x1EA0:
                        sb.AppendLine(Localization.Device_complies_with_SAT_no_version_claimed);

                        break;
                    case 0x1EA7:
                        sb.AppendLine(Localization.Device_complies_with_SAT_T10_1711_D_revision_8);

                        break;
                    case 0x1EAB:
                        sb.AppendLine(Localization.Device_complies_with_SAT_T10_1711_D_revision_9);

                        break;
                    case 0x1EAD:
                        sb.AppendLine(Localization.Device_complies_with_SAT_ANSI_INCITS_431_2007);

                        break;
                    case 0x1EC0:
                        sb.AppendLine(Localization.Device_complies_with_SAT_2_no_version_claimed);

                        break;
                    case 0x1EC4:
                        sb.AppendLine(Localization.Device_complies_with_SAT_2_T10_1826_D_revision_06);

                        break;
                    case 0x1EC8:
                        sb.AppendLine(Localization.Device_complies_with_SAT_2_T10_1826_D_revision_09);

                        break;
                    case 0x1ECA:
                        sb.AppendLine(Localization.Device_complies_with_SAT_2_ANSI_INCITS_465_2010);

                        break;
                    case 0x1EE0:
                        sb.AppendLine(Localization.Device_complies_with_SAT_3_no_version_claimed);

                        break;
                    case 0x1EE2:
                        sb.AppendLine(Localization.Device_complies_with_SAT_3_T10_BSR_INCITS_517_revision_4);

                        break;
                    case 0x1EE4:
                        sb.AppendLine(Localization.Device_complies_with_SAT_3_T10_BSR_INCITS_517_revision_7);

                        break;
                    case 0x1EE8:
                        sb.AppendLine(Localization.Device_complies_with_SAT_3_ANSI_INCITS_517_2015);

                        break;
                    case 0x1F00:
                        sb.AppendLine(Localization.Device_complies_with_SAT_4_no_version_claimed);

                        break;
                    case 0x20A0:
                        sb.AppendLine(Localization.Device_complies_with_SPL_no_version_claimed);

                        break;
                    case 0x20A3:
                        sb.AppendLine(Localization.Device_complies_with_SPL_T10_2124_D_revision_6a);

                        break;
                    case 0x20A5:
                        sb.AppendLine(Localization.Device_complies_with_SPL_T10_2124_D_revision_7);

                        break;
                    case 0x20A7:
                        sb.AppendLine(Localization.Device_complies_with_SPL_ANSI_INCITS_476_2011);

                        break;
                    case 0x20A8:
                        sb.AppendLine(Localization.
                                          Device_complies_with_SPL_ANSI_INCITS_476_2011_SPL_AM1_INCITS_476_AM1_2012);

                        break;
                    case 0x20AA:
                        sb.AppendLine(Localization.Device_complies_with_SPL_ISO_IEC_14776_261_2012);

                        break;
                    case 0x20C0:
                        sb.AppendLine(Localization.Device_complies_with_SPL_2_no_version_claimed);

                        break;
                    case 0x20C2:
                        sb.AppendLine(Localization.Device_complies_with_SPL_2_T10_BSR_INCITS_505_revision_4);

                        break;
                    case 0x20C4:
                        sb.AppendLine(Localization.Device_complies_with_SPL_2_T10_BSR_INCITS_505_revision_5);

                        break;
                    case 0x20C8:
                        sb.AppendLine(Localization.Device_complies_with_SPL_2_ANSI_INCITS_505_2013);

                        break;
                    case 0x20E0:
                        sb.AppendLine(Localization.Device_complies_with_SPL_3_no_version_claimed);

                        break;
                    case 0x20E4:
                        sb.AppendLine(Localization.Device_complies_with_SPL_3_T10_BSR_INCITS_492_revision_6);

                        break;
                    case 0x20E6:
                        sb.AppendLine(Localization.Device_complies_with_SPL_3_T10_BSR_INCITS_492_revision_7);

                        break;
                    case 0x20E8:
                        sb.AppendLine(Localization.Device_complies_with_SPL_3_ANSI_INCITS_492_2015);

                        break;
                    case 0x2100:
                        sb.AppendLine(Localization.Device_complies_with_SPL_4_no_version_claimed);

                        break;
                    case 0x21E0:
                        sb.AppendLine(Localization.Device_complies_with_SOP_no_version_claimed);

                        break;
                    case 0x21E4:
                        sb.AppendLine(Localization.Device_complies_with_SOP_T10_BSR_INCITS_489_revision_4);

                        break;
                    case 0x21E6:
                        sb.AppendLine(Localization.Device_complies_with_SOP_T10_BSR_INCITS_489_revision_5);

                        break;
                    case 0x21E8:
                        sb.AppendLine(Localization.Device_complies_with_SOP_ANSI_INCITS_489_2014);

                        break;
                    case 0x2200:
                        sb.AppendLine(Localization.Device_complies_with_PQI_no_version_claimed);

                        break;
                    case 0x2204:
                        sb.AppendLine(Localization.Device_complies_with_PQI_T10_BSR_INCITS_490_revision_6);

                        break;
                    case 0x2206:
                        sb.AppendLine(Localization.Device_complies_with_PQI_T10_BSR_INCITS_490_revision_7);

                        break;
                    case 0x2208:
                        sb.AppendLine(Localization.Device_complies_with_PQI_ANSI_INCITS_490_2014);

                        break;
                    case 0x2220:
                        sb.AppendLine(Localization.Device_complies_with_SOP_2_no_version_claimed);

                        break;
                    case 0x2240:
                        sb.AppendLine(Localization.Device_complies_with_PQI_2_no_version_claimed);

                        break;
                    case 0xFFC0:
                        sb.AppendLine(Localization.Device_complies_with_IEEE_1667_no_version_claimed);

                        break;
                    case 0xFFC1:
                        sb.AppendLine(Localization.Device_complies_with_IEEE_1667_2006);

                        break;
                    case 0xFFC2:
                        sb.AppendLine(Localization.Device_complies_with_IEEE_1667_2009);

                        break;
                    default:
                        sb.AppendFormat(Localization.Device_complies_with_unknown_standard_code_0, VersionDescriptor).
                           AppendLine();

                        break;
                }

        #region Quantum vendor prettifying
        if(response.QuantumPresent &&
           StringHandlers.CToString(response.VendorIdentification).ToLowerInvariant().Trim() == "quantum")
        {
            sb.AppendLine(Localization.Quantum_vendor_specific_information);

            switch(response.Qt_ProductFamily)
            {
                case 0:
                    sb.AppendLine(Localization.Product_family_is_not_specified);

                    break;
                case 1:
                    sb.AppendLine(Localization.Product_family_is_2_6_GB);

                    break;
                case 2:
                    sb.AppendLine(Localization.Product_family_is_6_0_GB);

                    break;
                case 3:
                    sb.AppendLine(Localization.Product_family_is_10_0_20_0_GB);

                    break;
                case 5:
                    sb.AppendLine(Localization.Product_family_is_20_0_40_0_GB);

                    break;
                case 6:
                    sb.AppendLine(Localization.Product_family_is_15_0_30_0_GB);

                    break;
                default:
                    sb.AppendFormat(Localization.Product_family_0, response.Qt_ProductFamily).AppendLine();

                    break;
            }

            sb.AppendFormat(Localization.Release_firmware_0, response.Qt_ReleasedFirmware).AppendLine();

            sb.AppendFormat(Localization.Firmware_version_0_1, response.Qt_FirmwareMajorVersion,
                            response.Qt_FirmwareMinorVersion).AppendLine();

            sb.AppendFormat(Localization.EEPROM_format_version_0_1, response.Qt_EEPROMFormatMajorVersion,
                            response.Qt_EEPROMFormatMinorVersion).AppendLine();

            sb.AppendFormat(Localization.Firmware_personality_0, response.Qt_FirmwarePersonality).AppendLine();
            sb.AppendFormat(Localization.Firmware_sub_personality_0, response.Qt_FirmwareSubPersonality).AppendLine();

            sb.AppendFormat(Localization.Tape_directory_format_version_0, response.Qt_TapeDirectoryFormatVersion).
               AppendLine();

            sb.AppendFormat(Localization.Controller_hardware_version_0, response.Qt_ControllerHardwareVersion).
               AppendLine();

            sb.AppendFormat(Localization.Drive_EEPROM_version_0, response.Qt_DriveEEPROMVersion).AppendLine();
            sb.AppendFormat(Localization.Drive_hardware_version_0, response.Qt_DriveHardwareVersion).AppendLine();

            sb.AppendFormat(Localization.Media_loader_firmware_version_0, response.Qt_MediaLoaderFirmwareVersion).
               AppendLine();

            sb.AppendFormat(Localization.Media_loader_hardware_version_0, response.Qt_MediaLoaderHardwareVersion).
               AppendLine();

            sb.AppendFormat(Localization.Media_loader_mechanical_version_0, response.Qt_MediaLoaderMechanicalVersion).
               AppendLine();

            if(response.Qt_LibraryPresent)
                sb.AppendLine(Localization.Library_is_present);

            if(response.Qt_MediaLoaderPresent)
                sb.AppendLine(Localization.Media_loader_is_present);

            sb.AppendFormat(Localization.Module_revision_0, StringHandlers.CToString(response.Qt_ModuleRevision)).
               AppendLine();
        }
        #endregion Quantum vendor prettifying

        #region IBM vendor prettifying
        if(response.IBMPresent &&
           StringHandlers.CToString(response.VendorIdentification).ToLowerInvariant().Trim() == "ibm")
        {
            sb.AppendLine(Localization.IBM_vendor_specific_information);

            if(response.IBM_PerformanceLimit == 0)
                sb.AppendLine(Localization.Performance_is_not_limited);
            else
                sb.AppendFormat(Localization.Performance_is_limited_using_factor_0, response.IBM_PerformanceLimit);

            if(response.IBM_AutDis)
                sb.AppendLine(Localization.Automation_is_disabled);

            sb.AppendFormat(Localization.IBM_OEM_Specific_Field_0, response.IBM_OEMSpecific).AppendLine();
        }
        #endregion IBM vendor prettifying

        #region HP vendor prettifying
        if(response.HPPresent &&
           StringHandlers.CToString(response.VendorIdentification).ToLowerInvariant().Trim() == "hp")
        {
            sb.AppendLine(Localization.HP_vendor_specific_information);

            if(response.HP_WORM)
                sb.AppendFormat(Localization.Device_supports_WORM_version_0, response.HP_WORMVersion).AppendLine();

            byte[] OBDRSign =
            {
                0x24, 0x44, 0x52, 0x2D, 0x31, 0x30
            };

            if(OBDRSign.SequenceEqual(response.HP_OBDR))
                sb.AppendLine(Localization.Device_supports_Tape_Disaster_Recovery);
        }
        #endregion HP vendor prettifying

        #region Seagate vendor prettifying
        if((response.SeagatePresent || response.Seagate2Present || response.Seagate3Present) &&
           StringHandlers.CToString(response.VendorIdentification).ToLowerInvariant().Trim() == "seagate")
        {
            sb.AppendLine(Localization.Seagate_vendor_specific_information);

            if(response.SeagatePresent)
                sb.AppendFormat(Core.Drive_serial_number_0,
                                StringHandlers.CToString(response.Seagate_DriveSerialNumber)).AppendLine();

            if(response.Seagate2Present)
                sb.AppendFormat(Localization.Drive_copyright_0, StringHandlers.CToString(response.Seagate_Copyright)).
                   AppendLine();

            if(response.Seagate3Present)
                sb.AppendFormat(Localization.Drive_servo_part_number_0,
                                PrintHex.ByteArrayToHexArrayString(response.Seagate_ServoPROMPartNo, 40)).AppendLine();
        }
        #endregion Seagate vendor prettifying

        #region Kreon vendor prettifying
        if(response.KreonPresent)
            sb.AppendFormat(Localization.Drive_is_flashed_with_Kreon_firmware_0,
                            StringHandlers.CToString(response.KreonVersion)).AppendLine();
        #endregion Kreon vendor prettifying

    #if DEBUG
        if(response.DeviceTypeModifier != 0)
            sb.AppendFormat(Localization.Vendor_device_type_modifier_0, response.DeviceTypeModifier).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat(Localization.Reserved_byte_five_bits_two_to_one_0, response.Reserved2).AppendLine();

        if(response.Reserved3 != 0)
            sb.AppendFormat(Localization.Reserved_byte_56_bits_seven_to_four_0, response.Reserved3).AppendLine();

        if(response.Reserved4 != 0)
            sb.AppendFormat(Localization.Reserved_byte_57, response.Reserved4).AppendLine();

        if(response.Reserved5 != null)
        {
            sb.AppendLine(Localization.Reserved_bytes_74_to_95);
            sb.AppendLine("============================================================");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.Reserved5, 60));
            sb.AppendLine("============================================================");
        }

        if(response is { VendorSpecific: {}, IsHiMD: true })
            if(response.KreonPresent)
            {
                byte[] vendor = new byte[7];
                Array.Copy(response.VendorSpecific, 11, vendor, 0, 7);
                sb.AppendLine(Localization.Vendor_specific_bytes_47_to_55);
                sb.AppendLine("============================================================");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(vendor, 60));
                sb.AppendLine("============================================================");
            }
            else
            {
                sb.AppendLine(Localization.Vendor_specific_bytes_36_to_55);
                sb.AppendLine("============================================================");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VendorSpecific, 60));
                sb.AppendLine("============================================================");
            }

        if(response.IsHiMD)
        {
            sb.AppendLine(Localization.Hi_MD_device_);

            if(response.HiMDSpecific != null)
            {
                sb.AppendLine(Localization.Hi_MD_specific_bytes_44_to_55);
                sb.AppendLine("============================================================");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.HiMDSpecific, 60));
                sb.AppendLine("============================================================");
            }
        }

        if(response.VendorSpecific2 == null)
            return sb.ToString();

        sb.AppendFormat(Localization.Vendor_specific_bytes_96_to_0, response.AdditionalLength + 4).AppendLine();
        sb.AppendLine("============================================================");
        sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VendorSpecific2, 60));
        sb.AppendLine("============================================================");
    #endif

        return sb.ToString();
    }

    public static string Prettify(byte[] SCSIInquiryResponse)
    {
        CommonTypes.Structs.Devices.SCSI.Inquiry? decoded =
            CommonTypes.Structs.Devices.SCSI.Inquiry.Decode(SCSIInquiryResponse);

        return Prettify(decoded);
    }
}