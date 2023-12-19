// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : EVPD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI EVPDs.
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Helpers;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class EVPD
{
    /// <summary>Decodes VPD page 0x00: Supported VPD pages</summary>
    /// <returns>A byte array containing all supported VPD pages.</returns>
    /// <param name="page">Page 0x00.</param>
    public static byte[] DecodePage00(byte[] page)
    {
        if(page?[1] != 0)
            return null;

        if(page.Length != page[3] + 4)
            return null;

        var decoded = new byte[page.Length - 4];

        Array.Copy(page, 4, decoded, 0, page.Length - 4);

        return decoded;
    }

    /// <summary>Decides VPD pages 0x01 to 0x7F: ASCII Information</summary>
    /// <returns>An ASCII string with the contents of the page.</returns>
    /// <param name="page">Page 0x01-0x7F.</param>
    public static string DecodeASCIIPage(byte[] page)
    {
        if(page == null)
            return null;

        if(page[1] == 0 || page[1] > 0x7F)
            return null;

        if(page.Length != page[3] + 4)
            return null;

        var ascii = new byte[page[4]];

        Array.Copy(page, 5, ascii, 0, page[4]);

        return StringHandlers.CToString(ascii);
    }

    /// <summary>Decodes VPD page 0x80: Unit Serial Number</summary>
    /// <returns>The unit serial number.</returns>
    /// <param name="page">Page 0x80.</param>
    public static string DecodePage80(byte[] page)
    {
        if(page?[1] != 0x80)
            return null;

        if(page.Length != page[3] + 4)
            return null;

        var ascii = new byte[page.Length - 4];

        Array.Copy(page, 4, ascii, 0, page.Length - 4);

        for(var i = 0; i < ascii.Length - 1; i++)
        {
            if(ascii[i] < 0x20)
                return null;
        }

        return StringHandlers.CToString(ascii);
    }

    /// <summary>Decodes VPD page 0x82: ASCII implemented operating definition</summary>
    /// <returns>ASCII implemented operating definition.</returns>
    /// <param name="page">Page 0x82.</param>
    public static string DecodePage82(byte[] page)
    {
        if(page?[1] != 0x82)
            return null;

        if(page.Length != page[3] + 4)
            return null;

        var ascii = new byte[page.Length - 4];

        Array.Copy(page, 4, ascii, 0, page.Length - 4);

        return StringHandlers.CToString(ascii);
    }

#region EVPD Page 0xB1: Manufacturer-assigned Serial Number page

    public static string DecodePageB1(byte[] page)
    {
        if(page?[1] != 0xB1)
            return null;

        if(page.Length != page[3] + 4)
            return null;

        var ascii = new byte[page.Length - 4];

        Array.Copy(page, 4, ascii, 0, page.Length - 4);

        return StringHandlers.CToString(ascii).Trim();
    }

#endregion EVPD Page 0xB1: Manufacturer-assigned Serial Number page

#region EVPD Page 0xB2: TapeAlert Supported Flags page

    public static ulong DecodePageB2(byte[] page)
    {
        if(page?[1] != 0xB2)
            return 0;

        if(page.Length != 12)
            return 0;

        var bitmap = new byte[8];

        Array.Copy(page, 4, bitmap, 0, 8);

        return BitConverter.ToUInt64(bitmap.Reverse().ToArray(), 0);
    }

#endregion EVPD Page 0xB2: TapeAlert Supported Flags page

#region EVPD Page 0xB3: Automation Device Serial Number page

    public static string DecodePageB3(byte[] page)
    {
        if(page?[1] != 0xB3)
            return null;

        if(page.Length != page[3] + 4)
            return null;

        var ascii = new byte[page.Length - 4];

        Array.Copy(page, 4, ascii, 0, page.Length - 4);

        return StringHandlers.CToString(ascii).Trim();
    }

#endregion EVPD Page 0xB3: Automation Device Serial Number page

#region EVPD Page 0xB4: Data Transfer Device Element Address page

    public static string DecodePageB4(byte[] page)
    {
        if(page?[1] != 0xB3)
            return null;

        if(page.Length != page[3] + 4)
            return null;

        var element = new byte[page.Length - 4];
        var sb      = new StringBuilder();

        foreach(byte b in element)
            sb.Append($"{b:X2}");

        return sb.ToString();
    }

#endregion EVPD Page 0xB4: Data Transfer Device Element Address page

#region EVPD Page 0x81: Implemented operating definition page

    /// <summary>Implemented operating definition page Page code 0x81</summary>
    public struct Page_81
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        /// <summary>Current operating definition</summary>
        public ScsiDefinitions Current;
        /// <summary>Default operating definition</summary>
        public ScsiDefinitions Default;
        /// <summary>Support operating definition list</summary>
        public ScsiDefinitions[] Supported;
    }

    public static Page_81? DecodePage_81(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0x81)
            return null;

        if(pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 6)
            return null;

        var decoded = new Page_81
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4),
            Current              = (ScsiDefinitions)(pageResponse[4] & 0x7F),
            Default              = (ScsiDefinitions)(pageResponse[5] & 0x7F)
        };

        var                   position    = 6;
        List<ScsiDefinitions> definitions = new();

        while(position < pageResponse.Length)
        {
            var definition = (ScsiDefinitions)(pageResponse[position] & 0x7F);
            position++;
            definitions.Add(definition);
        }

        decoded.Supported = definitions.ToArray();

        return decoded;
    }

    public static string PrettifyPage_81(byte[] pageResponse) => PrettifyPage_81(DecodePage_81(pageResponse));

    public static string DefinitionToString(ScsiDefinitions definition) => definition switch
                                                                           {
                                                                               ScsiDefinitions.Current => "",
                                                                               ScsiDefinitions.CCS     => "CCS",
                                                                               ScsiDefinitions.SCSI1   => "SCSI-1",
                                                                               ScsiDefinitions.SCSI2   => "SCSI-2",
                                                                               ScsiDefinitions.SCSI3   => "SCSI-3",
                                                                               _ =>
                                                                                   $"Unknown definition code {(byte)definition}"
                                                                           };

    public static string PrettifyPage_81(Page_81? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_81 page = modePage.Value;
        var     sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Implemented_operating_definitions);

        sb.AppendFormat("\t" + Localization.Default_operating_definition_0, DefinitionToString(page.Current)).
           AppendLine();

        sb.AppendFormat("\t" + Localization.Current_operating_definition_0, DefinitionToString(page.Current)).
           AppendLine();

        if(page.Supported.Length == 0)
        {
            sb.AppendLine("\t" + Localization.There_are_no_supported_definitions);

            return sb.ToString();
        }

        sb.AppendLine("\t" + Localization.Supported_operating_definitions);

        foreach(ScsiDefinitions definition in page.Supported)
            sb.AppendFormat("\t" + "\t{0}", DefinitionToString(definition)).AppendLine();

        return sb.ToString();
    }

#endregion EVPD Page 0x81: Implemented operating definition page

#region EVPD Page 0x83: Device identification page

    public enum IdentificationAssociation : byte
    {
        /// <summary>Identifier field is associated with the addressed logical unit</summary>
        LogicalUnit = 0,
        /// <summary>Identifier field is associated with the target port</summary>
        TargetPort = 1,
        /// <summary>Identifier field is associated with the target device that contains the LUN</summary>
        TargetDevice = 2
    }

    public enum IdentificationCodeSet : byte
    {
        /// <summary>Identifier is binary</summary>
        Binary = 1,
        /// <summary>Identifier is pure ASCII</summary>
        ASCII = 2,
        /// <summary>Identifier is in UTF-8</summary>
        UTF8 = 3
    }

    public enum IdentificationTypes : byte
    {
        /// <summary>No assignment authority was used and there is no guarantee the identifier is unique</summary>
        NoAuthority = 0,
        /// <summary>Concatenates vendor and product identifier from INQUIRY plus unit serial number from page 80h</summary>
        Inquiry = 1,
        /// <summary>Identifier is a 64-bit IEEE EUI-64, or extended</summary>
        EUI = 2,
        /// <summary>Identifier is compatible with 64-bit FC-PH Name_Identifier</summary>
        NAA = 3,
        /// <summary>Identifier to relative port in device</summary>
        Relative = 4,
        /// <summary>Identifier to group of target ports in device</summary>
        TargetPortGroup = 5,
        /// <summary>Identifier to group of target LUNs in device</summary>
        LogicalUnitGroup = 6,
        /// <summary>MD5 of device identification values</summary>
        MD5 = 7,
        /// <summary>SCSI name string</summary>
        SCSI = 8,
        /// <summary>Protocol specific port identifier</summary>
        ProtocolSpecific = 9
    }

    public struct IdentificatonDescriptor
    {
        /// <summary>Protocol identifier</summary>
        public ProtocolIdentifiers ProtocolIdentifier;
        /// <summary>Defines how the identifier is stored</summary>
        public IdentificationCodeSet CodeSet;
        /// <summary>Set if protocol identifier is valid</summary>
        public bool PIV;
        /// <summary>Identifies which decide the identifier associates with</summary>
        public IdentificationAssociation Association;
        /// <summary>Defines the type of the identifier</summary>
        public IdentificationTypes Type;
        /// <summary>Length of the identifier</summary>
        public byte Length;
        /// <summary>Identifier as a string if applicable</summary>
        public string ASCII;
        /// <summary>Binary identifier</summary>
        public byte[] Binary;
    }

    /// <summary>Device identification page Page code 0x83</summary>
    public struct Page_83
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        /// <summary>The descriptors.</summary>
        public IdentificatonDescriptor[] Descriptors;
    }

    public static Page_83? DecodePage_83(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0x83)
            return null;

        if(pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 6)
            return null;

        var decoded = new Page_83
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4)
        };

        var                           position    = 4;
        List<IdentificatonDescriptor> descriptors = new();

        while(position < pageResponse.Length)
        {
            var descriptor = new IdentificatonDescriptor
            {
                ProtocolIdentifier = (ProtocolIdentifiers)((pageResponse[position] & 0xF0) >> 4),
                CodeSet            = (IdentificationCodeSet)(pageResponse[position] & 0x0F),
                PIV                = (pageResponse[position + 1]                    & 0x80) == 0x80,
                Association        = (IdentificationAssociation)((pageResponse[position + 1] & 0x30) >> 4),
                Type               = (IdentificationTypes)(pageResponse[position + 1] & 0x0F),
                Length             = pageResponse[position + 3]
            };

            descriptor.Binary = new byte[descriptor.Length];

            if(descriptor.Length + position + 4 >= pageResponse.Length)
                descriptor.Length = (byte)(pageResponse.Length - position - 4);

            Array.Copy(pageResponse, position + 4, descriptor.Binary, 0, descriptor.Length);

            descriptor.ASCII = descriptor.CodeSet switch
                               {
                                   IdentificationCodeSet.ASCII => StringHandlers.CToString(descriptor.Binary),
                                   IdentificationCodeSet.UTF8  => Encoding.UTF8.GetString(descriptor.Binary),
                                   _                           => ""
                               };

            position += 4 + descriptor.Length;
            descriptors.Add(descriptor);
        }

        decoded.Descriptors = descriptors.ToArray();

        return decoded;
    }

    public static string PrettifyPage_83(byte[] pageResponse) => PrettifyPage_83(DecodePage_83(pageResponse));

    public static string PrettifyPage_83(Page_83? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_83 page = modePage.Value;
        var     sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Device_identification);

        if(page.Descriptors.Length == 0)
        {
            sb.AppendLine("\t" + Localization.There_are_no_identifiers);

            return sb.ToString();
        }

        foreach(IdentificatonDescriptor descriptor in page.Descriptors)
        {
            switch(descriptor.Association)
            {
                case IdentificationAssociation.LogicalUnit:
                    sb.AppendLine("\t" + Localization.Identifier_belongs_to_addressed_logical_unit);

                    break;
                case IdentificationAssociation.TargetPort:
                    sb.AppendLine("\t" + Localization.Identifier_belongs_to_target_port);

                    break;
                case IdentificationAssociation.TargetDevice:
                    sb.AppendLine("\t" +
                                  Localization.
                                      Identifier_belongs_to_target_device_that_contains_the_addressed_logical_unit);

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Identifier_has_unknown_association_with_code_0,
                                    (byte)descriptor.Association).
                       AppendLine();

                    break;
            }

            if(descriptor.PIV)
            {
                string protocol = descriptor.ProtocolIdentifier switch
                                  {
                                      ProtocolIdentifiers.ADT => Localization.Automation_Drive_Interface_Transport,
                                      ProtocolIdentifiers.ATA => Localization.AT_Attachment_Interface__ATA_ATAPI_,
                                      ProtocolIdentifiers.FibreChannel => Localization.Fibre_Channel,
                                      ProtocolIdentifiers.Firewire => Localization.IEEE_1394,
                                      ProtocolIdentifiers.iSCSI => Localization.Internet_SCSI,
                                      ProtocolIdentifiers.NoProtocol => Localization.no_specific_protocol,
                                      ProtocolIdentifiers.PCIe => Localization.PCI_Express,
                                      ProtocolIdentifiers.RDMAP => Localization.SCSI_Remote_Direct_Memory_Access,
                                      ProtocolIdentifiers.SAS => Localization.Serial_Attachment_SCSI,
                                      ProtocolIdentifiers.SCSI => Localization.Parallel_SCSI,
                                      ProtocolIdentifiers.SCSIe => Localization.SCSI_over_PCI_Express,
                                      ProtocolIdentifiers.SSA => Localization.SSA,
                                      ProtocolIdentifiers.UAS => Localization.USB_Attached_SCSI,
                                      _ => string.Format(Localization.unknown_code_protocol_0,
                                                         (byte)descriptor.ProtocolIdentifier)
                                  };

                sb.AppendFormat("\t" + Localization.Descriptor_refers_to_0_protocol, protocol).AppendLine();
            }

            switch(descriptor.Type)
            {
                case IdentificationTypes.NoAuthority:
                    switch(descriptor.CodeSet)
                    {
                        case IdentificationCodeSet.ASCII:
                        case IdentificationCodeSet.UTF8:
                            sb.AppendFormat("\t" + Localization.Vendor_descriptor_contains_0, descriptor.ASCII).
                               AppendLine();

                            break;
                        case IdentificationCodeSet.Binary:
                            sb.AppendFormat("\t" + Localization.Vendor_descriptor_contains_binary_data_hex_0,
                                            PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                               AppendLine();

                            break;
                        default:
                            sb.AppendFormat("\t" + Localization.Vendor_descriptor_contains_unknown_kind_1_of_data_hex_0,
                                            PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40),
                                            (byte)descriptor.CodeSet).
                               AppendLine();

                            break;
                    }

                    break;
                case IdentificationTypes.Inquiry:
                    switch(descriptor.CodeSet)
                    {
                        case IdentificationCodeSet.ASCII:
                        case IdentificationCodeSet.UTF8:
                            sb.AppendFormat("\t" + Localization.Inquiry_descriptor_contains_0, descriptor.ASCII).
                               AppendLine();

                            break;
                        case IdentificationCodeSet.Binary:
                            sb.AppendFormat("\t" + Localization.Inquiry_descriptor_contains_binary_data_hex_0,
                                            PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                               AppendLine();

                            break;
                        default:
                            sb.
                                AppendFormat("\t" + Localization.Inquiry_descriptor_contains_unknown_kind_1_of_data_hex_0,
                                             PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40),
                                             (byte)descriptor.CodeSet).
                                AppendLine();

                            break;
                    }

                    break;
                case IdentificationTypes.EUI:
                    if(descriptor.CodeSet is IdentificationCodeSet.ASCII or IdentificationCodeSet.UTF8)
                        sb.AppendFormat("\t" + Localization.IEEE_EUI_64_0, descriptor.ASCII).AppendLine();
                    else
                    {
                        sb.AppendFormat("\t" + Localization.IEEE_EUI_64_0_X2, descriptor.Binary[0]);

                        for(var i = 1; i < descriptor.Binary.Length; i++)
                            sb.Append($":{descriptor.Binary[i]:X2}");

                        sb.AppendLine();
                    }

                    break;
                case IdentificationTypes.NAA:
                    if(descriptor.CodeSet is IdentificationCodeSet.ASCII or IdentificationCodeSet.UTF8)
                        sb.AppendFormat("\t" + Localization.NAA_0, descriptor.ASCII).AppendLine();
                    else
                    {
                        sb.AppendFormat("\t" + Localization.NAA_0_X2, descriptor.Binary[0]);

                        for(var i = 1; i < descriptor.Binary.Length; i++)
                            sb.Append($":{descriptor.Binary[i]:X2}");

                        sb.AppendLine();
                    }

                    break;
                case IdentificationTypes.Relative:
                    if(descriptor.CodeSet is IdentificationCodeSet.ASCII or IdentificationCodeSet.UTF8)
                    {
                        sb.AppendFormat("\t" + Localization.Relative_target_port_identifier_0, descriptor.ASCII).
                           AppendLine();
                    }
                    else
                    {
                        sb.AppendFormat("\t"                        + Localization.Relative_target_port_identifier_0,
                                        (descriptor.Binary[2] << 8) + descriptor.Binary[3]).
                           AppendLine();
                    }

                    break;
                case IdentificationTypes.TargetPortGroup:
                    if(descriptor.CodeSet is IdentificationCodeSet.ASCII or IdentificationCodeSet.UTF8)
                        sb.AppendFormat("\t" + Localization.Target_group_identifier_0, descriptor.ASCII).AppendLine();
                    else
                    {
                        sb.AppendFormat("\t"                        + Localization.Target_group_identifier_0,
                                        (descriptor.Binary[2] << 8) + descriptor.Binary[3]).
                           AppendLine();
                    }

                    break;
                case IdentificationTypes.LogicalUnitGroup:
                    if(descriptor.CodeSet is IdentificationCodeSet.ASCII or IdentificationCodeSet.UTF8)
                    {
                        sb.AppendFormat("\t" + Localization.Logical_unit_group_identifier_0, descriptor.ASCII).
                           AppendLine();
                    }
                    else
                    {
                        sb.AppendFormat("\t"                        + Localization.Logical_unit_group_identifier_0,
                                        (descriptor.Binary[2] << 8) + descriptor.Binary[3]).
                           AppendLine();
                    }

                    break;
                case IdentificationTypes.MD5:
                    if(descriptor.CodeSet is IdentificationCodeSet.ASCII or IdentificationCodeSet.UTF8)
                    {
                        sb.AppendFormat("\t" + Localization.MD5_logical_unit_identifier_0, descriptor.ASCII).
                           AppendLine();
                    }
                    else
                    {
                        sb.AppendFormat("\t" + Localization.MD5_logical_unit_identifier_0_x2, descriptor.Binary[0]);

                        for(var i = 1; i < descriptor.Binary.Length; i++)
                            sb.Append($"{descriptor.Binary[i]:x2}");

                        sb.AppendLine();
                    }

                    break;
                case IdentificationTypes.SCSI:
                    if(descriptor.CodeSet is IdentificationCodeSet.ASCII or IdentificationCodeSet.UTF8)
                    {
                        sb.AppendFormat("\t" + Localization.SCSI_name_string_identifier_0, descriptor.ASCII).
                           AppendLine();
                    }
                    else
                    {
                        sb.AppendFormat("\t" + Localization.SCSI_name_string_identifier_hex_0,
                                        PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                           AppendLine();
                    }

                    break;
                case IdentificationTypes.ProtocolSpecific:
                {
                    if(descriptor.PIV)
                    {
                        switch(descriptor.ProtocolIdentifier)
                        {
                            case ProtocolIdentifiers.ADT:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_Automation_Drive_Interface_Transport_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.ATA:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_ATA_ATAPI_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.FibreChannel:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_Fibre_Channel_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.Firewire:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_IEEE_1394_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.iSCSI:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_Internet_SCSI_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.NoProtocol:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_unknown_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.PCIe:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_PCI_Express_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.RDMAP:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_SCSI_Remote_Direct_Memory_Access_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.SAS:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_Serial_Attachment_SCSI_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.SCSI:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_Parallel_SCSI_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.SSA:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_SSA_specific_descriptor_with_unknown_format_hex_0,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                            case ProtocolIdentifiers.SCSIe:
                                sb.AppendFormat("\t" + Localization.Protocol_SCSIe_specific_descriptor_Routing_ID_is_0,
                                                (descriptor.Binary[0] << 8) + descriptor.Binary[1]).
                                   AppendLine();

                                break;
                            case ProtocolIdentifiers.UAS:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_UAS_specific_descriptor_USB_address_0_interface_1,
                                                 descriptor.Binary[0] & 0x7F, descriptor.Binary[2]).
                                    AppendLine();

                                break;
                            default:
                                sb.
                                    AppendFormat("\t" + Localization.Protocol_unknown_code_0_specific_descriptor_with_unknown_format_hex_1,
                                                 (byte)descriptor.ProtocolIdentifier,
                                                 PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).
                                    AppendLine();

                                break;
                        }
                    }
                }

                    break;
                default:
                    switch(descriptor.CodeSet)
                    {
                        case IdentificationCodeSet.ASCII:
                        case IdentificationCodeSet.UTF8:
                            sb.AppendFormat("\t" + Localization.Unknown_descriptor_type_1_contains_0, descriptor.ASCII,
                                            (byte)descriptor.Type).
                               AppendLine();

                            break;
                        case IdentificationCodeSet.Binary:
                            sb.AppendFormat("\t" + Localization.Unknown_descriptor_type_1_contains_binary_data_hex_0,
                                            PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40),
                                            (byte)descriptor.Type).
                               AppendLine();

                            break;
                        default:
                            sb.
                                AppendFormat(Localization.Inquiry_descriptor_type_2_contains_unknown_kind_1_of_data_hex_0,
                                             PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40),
                                             (byte)descriptor.CodeSet, (byte)descriptor.Type).
                                AppendLine();

                            break;
                    }

                    break;
            }
        }

        return sb.ToString();
    }

#endregion EVPD Page 0x83: Device identification page

#region EVPD Page 0x84: Software Interface Identification page

    public struct SoftwareIdentifier
    {
        /// <summary>EUI-48 identifier</summary>
        public byte[] Identifier;
    }

    /// <summary>Software Interface Identification page Page code 0x84</summary>
    public struct Page_84
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        /// <summary>The descriptors.</summary>
        public SoftwareIdentifier[] Identifiers;
    }

    public static Page_84? DecodePage_84(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0x84)
            return null;

        if(pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 10)
            return null;

        var decoded = new Page_84
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4)
        };

        var                      position    = 4;
        List<SoftwareIdentifier> identifiers = new();

        while(position < pageResponse.Length)
        {
            var identifier = new SoftwareIdentifier
            {
                Identifier = new byte[6]
            };

            Array.Copy(pageResponse, position, identifier.Identifier, 0, 6);
            identifiers.Add(identifier);
            position += 6;
        }

        decoded.Identifiers = identifiers.ToArray();

        return decoded;
    }

    public static string PrettifyPage_84(byte[] pageResponse) => PrettifyPage_84(DecodePage_84(pageResponse));

    public static string PrettifyPage_84(Page_84? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_84 page = modePage.Value;
        var     sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Software_Interface_Identifiers);

        if(page.Identifiers.Length == 0)
        {
            sb.AppendLine("\t" + Localization.There_are_no_identifiers);

            return sb.ToString();
        }

        foreach(SoftwareIdentifier identifier in page.Identifiers)
        {
            sb.AppendFormat("\t" + "{0:X2}", identifier.Identifier[0]);

            for(var i = 1; i < identifier.Identifier.Length; i++)
                sb.Append($":{identifier.Identifier[i]:X2}");

            sb.AppendLine();
        }

        return sb.ToString();
    }

#endregion EVPD Page 0x84: Software Interface Identification page

#region EVPD Page 0x85: Management Network Addresses page

    public enum NetworkServiceTypes : byte
    {
        Unspecified    = 0,
        StorageConf    = 1,
        Diagnostics    = 2,
        Status         = 3,
        Logging        = 4,
        CodeDownload   = 5,
        CopyService    = 6,
        Administrative = 7
    }

    public struct NetworkDescriptor
    {
        /// <summary>Identifies which device the identifier associates with</summary>
        public IdentificationAssociation Association;
        /// <summary>Defines the type of the identifier</summary>
        public NetworkServiceTypes Type;
        /// <summary>Length of the identifier</summary>
        public ushort Length;
        /// <summary>Binary identifier</summary>
        public byte[] Address;
    }

    /// <summary>Device identification page Page code 0x85</summary>
    public struct Page_85
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public ushort PageLength;
        /// <summary>The descriptors.</summary>
        public NetworkDescriptor[] Descriptors;
    }

    public static Page_85? DecodePage_85(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0x85)
            return null;

        if((pageResponse[2] << 8) + pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 4)
            return null;

        var decoded = new Page_85
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (ushort)((pageResponse[2] << 8) + pageResponse[3] + 4)
        };

        var                     position    = 4;
        List<NetworkDescriptor> descriptors = new();

        while(position < pageResponse.Length)
        {
            var descriptor = new NetworkDescriptor
            {
                Association = (IdentificationAssociation)((pageResponse[position] & 0x60) >> 5),
                Type        = (NetworkServiceTypes)(pageResponse[position] & 0x1F),
                Length      = (ushort)((pageResponse[position + 2] << 8) + pageResponse[position + 3])
            };

            descriptor.Address = new byte[descriptor.Length];
            Array.Copy(pageResponse, position + 4, descriptor.Address, 0, descriptor.Length);

            position += 4 + descriptor.Length;
            descriptors.Add(descriptor);
        }

        decoded.Descriptors = descriptors.ToArray();

        return decoded;
    }

    public static string PrettifyPage_85(byte[] pageResponse) => PrettifyPage_85(DecodePage_85(pageResponse));

    public static string PrettifyPage_85(Page_85? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_85 page = modePage.Value;
        var     sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Management_Network_Addresses);

        if(page.Descriptors.Length == 0)
        {
            sb.AppendLine("\t" + Localization.There_are_no_addresses);

            return sb.ToString();
        }

        foreach(NetworkDescriptor descriptor in page.Descriptors)
        {
            switch(descriptor.Association)
            {
                case IdentificationAssociation.LogicalUnit:
                    sb.AppendLine("\t" + Localization.Identifier_belongs_to_addressed_logical_unit);

                    break;
                case IdentificationAssociation.TargetPort:
                    sb.AppendLine("\t" + Localization.Identifier_belongs_to_target_port);

                    break;
                case IdentificationAssociation.TargetDevice:
                    sb.AppendLine("\t" +
                                  Localization.
                                      Identifier_belongs_to_target_device_that_contains_the_addressed_logical_unit);

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Identifier_has_unknown_association_with_code_0,
                                    (byte)descriptor.Association).
                       AppendLine();

                    break;
            }

            switch(descriptor.Type)
            {
                case NetworkServiceTypes.CodeDownload:
                    sb.AppendFormat(Localization.Address_for_code_download_0,
                                    StringHandlers.CToString(descriptor.Address)).
                       AppendLine();

                    break;
                case NetworkServiceTypes.Diagnostics:
                    sb.AppendFormat(Localization.Address_for_diagnostics_0,
                                    StringHandlers.CToString(descriptor.Address)).
                       AppendLine();

                    break;
                case NetworkServiceTypes.Logging:
                    sb.AppendFormat(Localization.Address_for_logging_0, StringHandlers.CToString(descriptor.Address)).
                       AppendLine();

                    break;
                case NetworkServiceTypes.Status:
                    sb.AppendFormat(Localization.Address_for_status_0, StringHandlers.CToString(descriptor.Address)).
                       AppendLine();

                    break;
                case NetworkServiceTypes.StorageConf:
                    sb.AppendFormat(Localization.Address_for_storage_configuration_service_0,
                                    StringHandlers.CToString(descriptor.Address)).
                       AppendLine();

                    break;
                case NetworkServiceTypes.Unspecified:
                    sb.AppendFormat(Localization.Unspecified_address_0, StringHandlers.CToString(descriptor.Address)).
                       AppendLine();

                    break;
                case NetworkServiceTypes.CopyService:
                    sb.AppendFormat(Localization.Address_for_copy_service_0,
                                    StringHandlers.CToString(descriptor.Address)).
                       AppendLine();

                    break;
                case NetworkServiceTypes.Administrative:
                    sb.AppendFormat(Localization.Address_for_administrative_configuration_service_0,
                                    StringHandlers.CToString(descriptor.Address)).
                       AppendLine();

                    break;
                default:
                    sb.AppendFormat(Localization.Address_of_unknown_type_1_0,
                                    StringHandlers.CToString(descriptor.Address), (byte)descriptor.Type).
                       AppendLine();

                    break;
            }
        }

        return sb.ToString();
    }

#endregion EVPD Page 0x85: Management Network Addresses page

#region EVPD Page 0x86: Extended INQUIRY data page

    /// <summary>Device identification page Page code 0x86</summary>
    public struct Page_86
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        /// <summary>Indicates how a device server activates microcode</summary>
        public byte ActivateMicrocode;
        /// <summary>Protection types supported by device</summary>
        public byte SPT;
        /// <summary>Checks logical block guard field</summary>
        public bool GRD_CHK;
        /// <summary>Checks logical block application tag</summary>
        public bool APP_CHK;
        /// <summary>Checks logical block reference</summary>
        public bool REF_CHK;
        /// <summary>Supports unit attention condition sense key specific data</summary>
        public bool UASK_SUP;
        /// <summary>Supports grouping</summary>
        public bool GROUP_SUP;
        /// <summary>Supports priority</summary>
        public bool PRIOR_SUP;
        /// <summary>Supports head of queue</summary>
        public bool HEADSUP;
        /// <summary>Supports ordered</summary>
        public bool ORDSUP;
        /// <summary>Supports simple</summary>
        public bool SIMPSUP;
        /// <summary>Supports marking a block as uncorrectable</summary>
        public bool WU_SUP;
        /// <summary>Supports disabling correction on WRITE LONG</summary>
        public bool CRD_SUP;
        /// <summary>Supports a non-volatile cache</summary>
        public bool NV_SUP;
        /// <summary>Supports a volatile cache</summary>
        public bool V_SUP;
        /// <summary>Disable protection information checks</summary>
        public bool NO_PI_CHK;
        /// <summary>Protection information interval supported</summary>
        public bool P_I_I_SUP;
        /// <summary>Clears all LUNs unit attention when clearing one</summary>
        public bool LUICLR;
        /// <summary>Referrals support</summary>
        public bool R_SUP;
        /// <summary>History snapshots release effects</summary>
        public bool HSSRELEF;
        /// <summary>Capability based command security</summary>
        public bool CBCS;
        /// <summary>Indicates how it handles microcode updating with multiple nexuxes</summary>
        public byte Nexus;
        /// <summary>Time to complete extended self-test</summary>
        public ushort ExtendedTestMinutes;
        /// <summary>Power on activation support</summary>
        public bool POA_SUP;
        /// <summary>Hard reset actication</summary>
        public bool HRA_SUP;
        /// <summary>Vendor specific activation</summary>
        public bool VSA_SUP;
        /// <summary>Maximum length in bytes of sense data</summary>
        public byte MaximumSenseLength;
    }

    public static Page_86? DecodePage_86(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0x86)
            return null;

        if(pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 64)
            return null;

        return new Page_86
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4),
            ActivateMicrocode    = (byte)((pageResponse[4] & 0xC0) >> 6),
            SPT                  = (byte)((pageResponse[4] & 0x38) >> 3),
            GRD_CHK              = (pageResponse[4]       & 0x04) == 0x04,
            APP_CHK              = (pageResponse[4]       & 0x02) == 0x02,
            REF_CHK              = (pageResponse[4]       & 0x01) == 0x01,
            UASK_SUP             = (pageResponse[5]       & 0x20) == 0x20,
            GROUP_SUP            = (pageResponse[5]       & 0x10) == 0x10,
            PRIOR_SUP            = (pageResponse[5]       & 0x08) == 0x08,
            HEADSUP              = (pageResponse[5]       & 0x04) == 0x04,
            ORDSUP               = (pageResponse[5]       & 0x02) == 0x02,
            SIMPSUP              = (pageResponse[5]       & 0x01) == 0x01,
            WU_SUP               = (pageResponse[6]       & 0x08) == 0x08,
            CRD_SUP              = (pageResponse[6]       & 0x04) == 0x04,
            NV_SUP               = (pageResponse[6]       & 0x02) == 0x02,
            V_SUP                = (pageResponse[6]       & 0x01) == 0x01,
            NO_PI_CHK            = (pageResponse[7]       & 0x20) == 0x20,
            P_I_I_SUP            = (pageResponse[7]       & 0x10) == 0x10,
            LUICLR               = (pageResponse[7]       & 0x01) == 0x01,
            R_SUP                = (pageResponse[8]       & 0x10) == 0x10,
            HSSRELEF             = (pageResponse[8]       & 0x02) == 0x02,
            CBCS                 = (pageResponse[8]       & 0x01) == 0x01,
            Nexus                = (byte)(pageResponse[9] & 0x0F),
            ExtendedTestMinutes  = (ushort)((pageResponse[10] << 8) + pageResponse[11]),
            POA_SUP              = (pageResponse[12] & 0x80) == 0x80,
            HRA_SUP              = (pageResponse[12] & 0x40) == 0x40,
            VSA_SUP              = (pageResponse[12] & 0x20) == 0x20,
            MaximumSenseLength   = pageResponse[13]
        };
    }

    public static string PrettifyPage_86(byte[] pageResponse) => PrettifyPage_86(DecodePage_86(pageResponse));

    public static string PrettifyPage_86(Page_86? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_86 page = modePage.Value;
        var     sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Extended_INQUIRY_Data);

        switch(page.PeripheralDeviceType)
        {
            case PeripheralDeviceTypes.DirectAccess:
            case PeripheralDeviceTypes.SCSIZonedBlockDevice:
                switch(page.SPT)
                {
                    case 0:
                        sb.AppendLine(Localization.Logical_unit_supports_type_1_protection);

                        break;
                    case 1:
                        sb.AppendLine(Localization.Logical_unit_supports_types_1_and_2_protection);

                        break;
                    case 2:
                        sb.AppendLine(Localization.Logical_unit_supports_type_2_protection);

                        break;
                    case 3:
                        sb.AppendLine(Localization.Logical_unit_supports_types_1_and_3_protection);

                        break;
                    case 4:
                        sb.AppendLine(Localization.Logical_unit_supports_type_3_protection);

                        break;
                    case 5:
                        sb.AppendLine(Localization.Logical_unit_supports_types_2_and_3_protection);

                        break;
                    case 7:
                        sb.AppendLine(Localization.Logical_unit_supports_types_1_2_and_3_protection);

                        break;
                    default:
                        sb.AppendFormat(Localization.Logical_unit_supports_unknown_protection_defined_by_code_0,
                                        page.SPT).
                           AppendLine();

                        break;
                }

                break;
            case PeripheralDeviceTypes.SequentialAccess when page.SPT == 1:
                sb.AppendLine(Localization.Logical_unit_supports_logical_block_protection);

                break;
        }

        if(page.GRD_CHK)
            sb.AppendLine(Localization.Device_checks_the_logical_block_guard);

        if(page.APP_CHK)
            sb.AppendLine(Localization.Device_checks_the_logical_block_application_tag);

        if(page.REF_CHK)
            sb.AppendLine(Localization.Device_checks_the_logical_block_reference_tag);

        if(page.UASK_SUP)
            sb.AppendLine(Localization.Device_supports_unit_attention_condition_sense_key_specific_data);

        if(page.GROUP_SUP)
            sb.AppendLine(Localization.Device_supports_grouping);

        if(page.PRIOR_SUP)
            sb.AppendLine(Localization.Device_supports_priority);

        if(page.HEADSUP)
            sb.AppendLine(Localization.Device_supports_head_of_queue);

        if(page.ORDSUP)
            sb.AppendLine(Localization.Device_supports_the_ORDERED_task_attribute);

        if(page.SIMPSUP)
            sb.AppendLine(Localization.Device_supports_the_SIMPLE_task_attribute);

        if(page.WU_SUP)
            sb.AppendLine(Localization.Device_supports_marking_a_block_as_uncorrectable_with_WRITE_LONG);

        if(page.CRD_SUP)
            sb.AppendLine(Localization.Device_supports_disabling_correction_with_WRITE_LONG);

        if(page.NV_SUP)
            sb.AppendLine(Localization.Device_has_a_non_volatile_cache);

        if(page.V_SUP)
            sb.AppendLine(Localization.Device_has_a_volatile_cache);

        if(page.NO_PI_CHK)
            sb.AppendLine(Localization.Device_has_disabled_protection_information_checks);

        if(page.P_I_I_SUP)
            sb.AppendLine(Localization.Device_supports_protection_information_intervals);

        if(page.LUICLR)
        {
            sb.AppendLine(Localization.
                              Device_clears_any_unit_attention_condition_in_all_LUNs_after_reporting_for_any_LUN);
        }

        if(page.R_SUP)
            sb.AppendLine(Localization.Device_supports_referrals);

        if(page.HSSRELEF)
            sb.AppendLine(Localization.Device_implements_alternate_reset_handling);

        if(page.CBCS)
            sb.AppendLine(Localization.Device_supports_capability_based_command_security);

        if(page.POA_SUP)
            sb.AppendLine(Localization.Device_supports_power_on_activation_for_new_microcode);

        if(page.HRA_SUP)
            sb.AppendLine(Localization.Device_supports_hard_reset_activation_for_new_microcode);

        if(page.VSA_SUP)
            sb.AppendLine(Localization.Device_supports_vendor_specific_activation_for_new_microcode);

        if(page.ExtendedTestMinutes > 0)
        {
            sb.AppendFormat(Localization.Extended_self_test_takes_0_to_complete,
                            TimeSpan.FromMinutes(page.ExtendedTestMinutes)).
               AppendLine();
        }

        if(page.MaximumSenseLength > 0)
        {
            sb.AppendFormat(Localization.Device_supports_a_maximum_of_0_bytes_for_sense_data, page.MaximumSenseLength).
               AppendLine();
        }

        return sb.ToString();
    }

#endregion EVPD Page 0x86: Extended INQUIRY data page

#region EVPD Page 0x89: ATA Information page

    /// <summary>ATA Information page Page code 0x89</summary>
    public struct Page_89
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public ushort PageLength;
        /// <summary>Contains the SAT vendor identification</summary>
        public byte[] VendorIdentification;
        /// <summary>Contains the SAT product identification</summary>
        public byte[] ProductIdentification;
        /// <summary>Contains the SAT revision level</summary>
        public byte[] ProductRevisionLevel;
        /// <summary>Contains the ATA device signature</summary>
        public byte[] Signature;
        /// <summary>Contains the command code used to identify the device</summary>
        public byte CommandCode;
        /// <summary>Contains the response to ATA IDENTIFY (PACKET) DEVICE</summary>
        public byte[] IdentifyData;
    }

    public static Page_89? DecodePage_89(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0x89)
            return null;

        if((pageResponse[2] << 8) + pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 572)
            return null;

        var decoded = new Page_89
        {
            PeripheralQualifier   = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType  = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength            = (ushort)((pageResponse[2] << 8) + pageResponse[3] + 4),
            VendorIdentification  = new byte[8],
            ProductIdentification = new byte[16],
            ProductRevisionLevel  = new byte[4],
            Signature             = new byte[20],
            IdentifyData          = new byte[512]
        };

        Array.Copy(pageResponse, 8,  decoded.VendorIdentification,  0, 8);
        Array.Copy(pageResponse, 16, decoded.ProductIdentification, 0, 16);
        Array.Copy(pageResponse, 32, decoded.ProductRevisionLevel,  0, 4);
        Array.Copy(pageResponse, 36, decoded.Signature,             0, 20);
        decoded.CommandCode = pageResponse[56];
        Array.Copy(pageResponse, 60, decoded.IdentifyData, 0, 512);

        return decoded;
    }

    public static string PrettifyPage_89(byte[] pageResponse) => PrettifyPage_89(DecodePage_89(pageResponse));

    // TODO: Decode ATA signature?
    public static string PrettifyPage_89(Page_89? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_89 page = modePage.Value;
        var     sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_to_ATA_Translation_Layer_Data);

        sb.AppendFormat("\t" + Localization.Translation_layer_vendor_0,
                        VendorString.Prettify(StringHandlers.CToString(page.VendorIdentification).Trim())).
           AppendLine();

        sb.AppendFormat("\t" + Localization.Translation_layer_name_0,
                        StringHandlers.CToString(page.ProductIdentification).Trim()).
           AppendLine();

        sb.AppendFormat("\t" + Localization.Translation_layer_release_level_0,
                        StringHandlers.CToString(page.ProductRevisionLevel).Trim()).
           AppendLine();

        switch(page.CommandCode)
        {
            case 0xEC:
                sb.AppendLine("\t" + Localization.Device_responded_to_ATA_IDENTIFY_DEVICE_command);

                break;
            case 0xA1:
                sb.AppendLine("\t" + Localization.Device_responded_to_ATA_IDENTIFY_PACKET_DEVICE_command);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Device_responded_to_ATA_command_0, page.CommandCode).AppendLine();

                break;
        }

        switch(page.Signature[0])
        {
            case 0x00:
                sb.AppendLine("\t" + Localization.Device_uses_Parallel_ATA);

                break;
            case 0x34:
                sb.AppendLine("\t" + Localization.Device_uses_Serial_ATA);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Device_uses_unknown_transport_with_code_0, page.Signature[0]).
                   AppendLine();

                break;
        }

        Identify.IdentifyDevice? id = Identify.Decode(page.IdentifyData);

        if(id != null)
        {
            sb.AppendLine("\t" + Localization.ATA_IDENTIFY_information_follows);
            sb.Append($"{ATA.Identify.Prettify(id)}").AppendLine();
        }
        else
            sb.AppendLine("\t" + Localization.Could_not_decode_ATA_IDENTIFY_information);

        return sb.ToString();
    }

#endregion EVPD Page 0x89: ATA Information page

#region EVPD Page 0xC0 (Quantum): Firmware Build Information page

    /// <summary>Firmware Build Information page Page code 0xC0 (Quantum)</summary>
    public struct Page_C0_Quantum
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        /// <summary>Servo firmware checksum</summary>
        public ushort ServoFirmwareChecksum;
        /// <summary>Servo EEPROM checksum</summary>
        public ushort ServoEEPROMChecksum;
        /// <summary>Read/Write firmware checksum</summary>
        public uint ReadWriteFirmwareChecksum;
        /// <summary>Read/Write firmware build data</summary>
        public byte[] ReadWriteFirmwareBuildData;
    }

    public static Page_C0_Quantum? DecodePage_C0_Quantum(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0xC0)
            return null;

        if(pageResponse[3] != 20)
            return null;

        if(pageResponse.Length != 36)
            return null;

        var decoded = new Page_C0_Quantum
        {
            PeripheralQualifier   = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType  = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength            = (byte)(pageResponse[3]          + 4),
            ServoFirmwareChecksum = (ushort)((pageResponse[4] << 8) + pageResponse[5]),
            ServoEEPROMChecksum   = (ushort)((pageResponse[6] << 8) + pageResponse[7]),
            ReadWriteFirmwareChecksum = (uint)((pageResponse[8]  << 24) +
                                               (pageResponse[9]  << 16) +
                                               (pageResponse[10] << 8)  +
                                               pageResponse[11]),
            ReadWriteFirmwareBuildData = new byte[24]
        };

        Array.Copy(pageResponse, 12, decoded.ReadWriteFirmwareBuildData, 0, 24);

        return decoded;
    }

    public static string PrettifyPage_C0_Quantum(byte[] pageResponse) =>
        PrettifyPage_C0_Quantum(DecodePage_C0_Quantum(pageResponse));

    public static string PrettifyPage_C0_Quantum(Page_C0_Quantum? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_C0_Quantum page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.Quantum_Quantum_Firmware_Build_Information_page);

        sb.AppendFormat("\t" + Localization.Quantum_Servo_firmware_checksum_0, page.ServoFirmwareChecksum).AppendLine();
        sb.AppendFormat("\t" + Localization.Quantum_EEPROM_firmware_checksum_0, page.ServoEEPROMChecksum).AppendLine();

        sb.AppendFormat("\t" + Localization.Quantum_Read_write_firmware_checksum_0, page.ReadWriteFirmwareChecksum).
           AppendLine();

        sb.AppendFormat("\t" + Localization.Quantum_Read_write_firmware_build_date_0,
                        StringHandlers.CToString(page.ReadWriteFirmwareBuildData)).
           AppendLine();

        return sb.ToString();
    }

#endregion EVPD Page 0xC0 (Quantum): Firmware Build Information page

#region EVPD Pages 0xC0, 0xC1 (Certance): Drive component revision level pages

    /// <summary>Drive component revision level pages Page codes 0xC0, 0xC1 (Certance)</summary>
    public struct Page_C0_C1_Certance
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        public byte[] Component;
        public byte[] Version;
        public byte[] Date;
        public byte[] Variant;
    }

    public static Page_C0_C1_Certance? DecodePage_C0_C1_Certance(byte[] pageResponse)
    {
        if(pageResponse == null)
            return null;

        if(pageResponse[1] != 0xC0 && pageResponse[1] != 0xC1)
            return null;

        if(pageResponse[3] != 92)
            return null;

        if(pageResponse.Length != 96)
            return null;

        var decoded = new Page_C0_C1_Certance
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4),
            Component            = new byte[26],
            Version              = new byte[19],
            Date                 = new byte[24],
            Variant              = new byte[23]
        };

        Array.Copy(pageResponse, 4,  decoded.Component, 0, 26);
        Array.Copy(pageResponse, 30, decoded.Version,   0, 19);
        Array.Copy(pageResponse, 49, decoded.Date,      0, 24);
        Array.Copy(pageResponse, 73, decoded.Variant,   0, 23);

        return decoded;
    }

    public static string PrettifyPage_C0_C1_Certance(byte[] pageResponse) =>
        PrettifyPage_C0_C1_Certance(DecodePage_C0_C1_Certance(pageResponse));

    public static string PrettifyPage_C0_C1_Certance(Page_C0_C1_Certance? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_C0_C1_Certance page = modePage.Value;
        var                 sb   = new StringBuilder();

        sb.AppendLine(Localization.Certance_Certance_Drive_Component_Revision_Levels_page);

        sb.AppendFormat("\t" + Localization.Certance_Component_0, StringHandlers.CToString(page.Component)).
           AppendLine();

        sb.AppendFormat("\t" + Localization.Certance_Version_0, StringHandlers.CToString(page.Version)).AppendLine();
        sb.AppendFormat("\t" + Localization.Certance_Date_0,    StringHandlers.CToString(page.Date)).AppendLine();
        sb.AppendFormat("\t" + Localization.Certance_Variant_0, StringHandlers.CToString(page.Variant)).AppendLine();

        return sb.ToString();
    }

#endregion EVPD Pages 0xC0, 0xC1 (Certance): Drive component revision level pages

#region EVPD Pages 0xC2, 0xC3, 0xC4, 0xC5, 0xC6 (Certance): Drive component serial number pages

    /// <summary>Drive component serial number pages Page codes 0xC2, 0xC3, 0xC4, 0xC5, 0xC6 (Certance)</summary>
    public struct Page_C2_C3_C4_C5_C6_Certance
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        public byte[] SerialNumber;
    }

    public static Page_C2_C3_C4_C5_C6_Certance? DecodePage_C2_C3_C4_C5_C6_Certance(byte[] pageResponse)
    {
        if(pageResponse == null)
            return null;

        if(pageResponse[1] != 0xC2 &&
           pageResponse[1] != 0xC3 &&
           pageResponse[1] != 0xC4 &&
           pageResponse[1] != 0xC5 &&
           pageResponse[1] != 0xC6)
            return null;

        if(pageResponse[3] != 12)
            return null;

        if(pageResponse.Length != 16)
            return null;

        var decoded = new Page_C2_C3_C4_C5_C6_Certance
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4),
            SerialNumber         = new byte[12]
        };

        Array.Copy(pageResponse, 4, decoded.SerialNumber, 0, 12);

        return decoded;
    }

    public static string PrettifyPage_C2_C3_C4_C5_C6_Certance(byte[] pageResponse) =>
        PrettifyPage_C2_C3_C4_C5_C6_Certance(DecodePage_C2_C3_C4_C5_C6_Certance(pageResponse));

    public static string PrettifyPage_C2_C3_C4_C5_C6_Certance(Page_C2_C3_C4_C5_C6_Certance? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_C2_C3_C4_C5_C6_Certance page = modePage.Value;
        var                          sb   = new StringBuilder();

        sb.AppendLine(Localization.Certance_Certance_Drive_Component_Serial_Number_page);

        switch(page.PageCode)
        {
            case 0xC2:
                sb.AppendFormat("\t" + Localization.Certance_Head_Assembly_Serial_Number_0,
                                StringHandlers.CToString(page.SerialNumber)).
                   AppendLine();

                break;
            case 0xC3:
                sb.AppendFormat("\t" + Localization.Certance_Reel_Motor_1_Serial_Number_0,
                                StringHandlers.CToString(page.SerialNumber)).
                   AppendLine();

                break;
            case 0xC4:
                sb.AppendFormat("\t" + Localization.Certance_Reel_Motor_2_Serial_Number_0,
                                StringHandlers.CToString(page.SerialNumber)).
                   AppendLine();

                break;
            case 0xC5:
                sb.AppendFormat("\t" + Localization.Certance_Board_Serial_Number_0,
                                StringHandlers.CToString(page.SerialNumber)).
                   AppendLine();

                break;
            case 0xC6:
                sb.AppendFormat("\t" + Localization.Certance_Base_Mechanical_Serial_Number_0,
                                StringHandlers.CToString(page.SerialNumber)).
                   AppendLine();

                break;
        }

        return sb.ToString();
    }

#endregion EVPD Pages 0xC0, 0xC1 (Certance): Drive component revision level pages

#region EVPD Page 0xDF (Certance): Drive status pages

    /// <summary>Drive status pages Page codes 0xDF (Certance)</summary>
    public struct Page_DF_Certance
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        /// <summary>Command forwarding</summary>
        public byte CmdFwd;
        /// <summary>Alerts</summary>
        public bool Alerts;
        /// <summary>Removable prevention</summary>
        public bool NoRemov;
        /// <summary>Unit reservation</summary>
        public bool UnitRsvd;
        /// <summary>Needs cleaning</summary>
        public bool Clean;
        /// <summary>Tape threaded</summary>
        public bool Threaded;
        /// <summary>Commands await forwarding</summary>
        public bool Lun1Cmd;
        /// <summary>Autoload mode</summary>
        public byte AutoloadMode;
        /// <summary>Cartridge type</summary>
        public byte CartridgeType;
        /// <summary>Cartridge format</summary>
        public byte CartridgeFormat;
        /// <summary>Cartridge capacity in 10e9 bytes</summary>
        public ushort CartridgeCapacity;
        /// <summary>Port A transport type</summary>
        public byte PortATransportType;
        /// <summary>Port A SCSI ID</summary>
        public byte PortASelectionID;
        /// <summary>Total number of head-tape contact time</summary>
        public uint OperatingHours;
        /// <summary>ID that reserved the device</summary>
        public ulong InitiatorID;
        /// <summary>Cartridge serial number</summary>
        public byte[] CartridgeSerialNumber;
    }

    public static Page_DF_Certance? DecodePage_DF_Certance(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0xDF)
            return null;

        if(pageResponse[3] != 60)
            return null;

        if(pageResponse.Length != 64)
            return null;

        var decoded = new Page_DF_Certance
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4),
            CmdFwd               = (byte)((pageResponse[5] & 0xC0) >> 5),
            Alerts               = (pageResponse[5]       & 0x20) == 0x20,
            NoRemov              = (pageResponse[5]       & 0x08) == 0x08,
            UnitRsvd             = (pageResponse[5]       & 0x04) == 0x04,
            Clean                = (pageResponse[5]       & 0x01) == 0x01,
            Threaded             = (pageResponse[6]       & 0x10) == 0x10,
            Lun1Cmd              = (pageResponse[6]       & 0x08) == 0x08,
            AutoloadMode         = (byte)(pageResponse[6] & 0x07),
            CartridgeType        = pageResponse[8],
            CartridgeFormat      = pageResponse[9],
            CartridgeCapacity    = (ushort)((pageResponse[10] << 8) + pageResponse[11] + 4),
            PortATransportType   = pageResponse[12],
            PortASelectionID     = pageResponse[15],
            OperatingHours = (uint)((pageResponse[20] << 24) +
                                    (pageResponse[21] << 16) +
                                    (pageResponse[22] << 8)  +
                                    pageResponse[23]),
            CartridgeSerialNumber = new byte[32]
        };

        var buf = new byte[8];
        Array.Copy(pageResponse, 24, buf, 0, 8);
        decoded.InitiatorID = BitConverter.ToUInt64(buf.Reverse().ToArray(), 0);
        Array.Copy(pageResponse, 32, decoded.CartridgeSerialNumber, 0, 32);

        return decoded;
    }

    public static string PrettifyPage_DF_Certance(byte[] pageResponse) =>
        PrettifyPage_DF_Certance(DecodePage_DF_Certance(pageResponse));

    public static string PrettifyPage_DF_Certance(Page_DF_Certance? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_DF_Certance page = modePage.Value;
        var              sb   = new StringBuilder();

        sb.AppendLine(Localization.Certance_drive_status_page);

        switch(page.CmdFwd)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Command_forwarding_is_disabled);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Command_forwarding_is_enabled);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_command_forwarding_code_0, page.CmdFwd).AppendLine();

                break;
        }

        if(page.Alerts)
            sb.AppendLine("\t" + Localization.Alerts_are_enabled);

        if(page.NoRemov)
            sb.AppendLine("\t" + Localization.Cartridge_removable_is_prevented);

        if(page.UnitRsvd)
            sb.AppendFormat("\t" + Localization.Unit_is_reserved_by_initiator_ID_0, page.InitiatorID).AppendLine();

        if(page.Clean)
            sb.AppendLine("\t" + Localization.Device_needs_cleaning_cartridge);

        if(page.Threaded)
            sb.AppendLine("\t" + Localization.Cartridge_tape_is_threaded);

        if(page.Lun1Cmd)
            sb.AppendLine("\t" + Localization.There_are_commands_pending_to_be_forwarded);

        switch(page.AutoloadMode)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Cartridge_will_be_loaded_and_threaded_on_insertion);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Cartridge_will_be_loaded_but_not_threaded_on_insertion);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Cartridge_will_not_be_loaded);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_autoloading_mode_code_0, page.AutoloadMode).AppendLine();

                break;
        }

        switch(page.PortATransportType)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Port_A_link_is_down);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Port_A_uses_Parallel_SCSI_Ultra_160_interface);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_port_A_transport_type_code_0, page.PortATransportType).
                   AppendLine();

                break;
        }

        if(page.PortATransportType > 0)
            sb.AppendFormat("\t" + Localization.Drive_responds_to_SCSI_ID_0, page.PortASelectionID).AppendLine();

        sb.AppendFormat("\t" + Localization.Drive_has_been_operating_0, TimeSpan.FromHours(page.OperatingHours)).
           AppendLine();

        if(page.CartridgeType > 0)
        {
            switch(page.CartridgeFormat)
            {
                case 0:
                    sb.AppendLine("\t" + Localization.Inserted_cartridge_is_LTO);

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Unknown_cartridge_format_code_0, page.CartridgeType).
                       AppendLine();

                    break;
            }

            switch(page.CartridgeType)
            {
                case 0:
                    sb.AppendLine("\t" + Localization.There_is_no_cartridge_inserted);

                    break;
                case 1:
                    sb.AppendLine("\t" + Localization.Cleaning_cartridge_inserted);

                    break;
                case 2:
                    sb.AppendLine("\t" + Localization.Unknown_data_cartridge_inserted);

                    break;
                case 3:
                    sb.AppendLine("\t" + Localization.Firmware_cartridge_inserted);

                    break;
                case 4:
                    sb.AppendLine("\t" + Localization.LTO_Ultrium_1_Type_A_cartridge_inserted);

                    break;
                case 5:
                    sb.AppendLine("\t" + Localization.LTO_Ultrium_1_Type_B_cartridge_inserted);

                    break;
                case 6:
                    sb.AppendLine("\t" + Localization.LTO_Ultrium_1_Type_C_cartridge_inserted);

                    break;
                case 7:
                    sb.AppendLine("\t" + Localization.LTO_Ultrium_1_Type_D_cartridge_inserted);

                    break;
                case 8:
                    sb.AppendLine("\t" + Localization.LTO_Ultrium_2_cartridge_inserted);

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Unknown_cartridge_type_code_0, page.CartridgeType).AppendLine();

                    break;
            }

            sb.AppendFormat("\t" + Localization.Cartridge_has_an_uncompressed_capability_of_0_gigabytes,
                            page.CartridgeCapacity).
               AppendLine();

            sb.AppendFormat("\t" + Localization.Cartridge_serial_number_0,
                            StringHandlers.SpacePaddedToString(page.CartridgeSerialNumber)).
               AppendLine();
        }
        else
            sb.AppendLine("\t" + Localization.There_is_no_cartridge_inserted);

        return sb.ToString();
    }

#endregion EVPD Page 0xDF (Certance): Drive status pages

#region EVPD Page 0xC0 (IBM): Drive Component Revision Levels page

    /// <summary>Drive Component Revision Levels page Page code 0xC0 (IBM)</summary>
    public struct Page_C0_IBM
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        public byte[] CodeName;
        public byte[] Date;
    }

    public static Page_C0_IBM? DecodePage_C0_IBM(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0xC0)
            return null;

        if(pageResponse[3] != 39)
            return null;

        if(pageResponse.Length != 43)
            return null;

        var decoded = new Page_C0_IBM
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4),
            CodeName             = new byte[12],
            Date                 = new byte[8]
        };

        Array.Copy(pageResponse, 4,  decoded.CodeName, 0, 12);
        Array.Copy(pageResponse, 23, decoded.Date,     0, 8);

        return decoded;
    }

    public static string PrettifyPage_C0_IBM(byte[] pageResponse) =>
        PrettifyPage_C0_IBM(DecodePage_C0_IBM(pageResponse));

    public static string PrettifyPage_C0_IBM(Page_C0_IBM? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_C0_IBM page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.IBM_Drive_Component_Revision_Levels_page);

        sb.AppendFormat("\t" + Localization.Code_name_0,     StringHandlers.CToString(page.CodeName)).AppendLine();
        sb.AppendFormat("\t" + Localization.Certance_Date_0, StringHandlers.CToString(page.Date)).AppendLine();

        return sb.ToString();
    }

#endregion EVPD Page 0xC0 (IBM): Drive Component Revision Levels page

#region EVPD Page 0xC1 (IBM): Drive Serial Numbers page

    /// <summary>Drive Serial Numbers page Page code 0xC1 (IBM)</summary>
    public struct Page_C1_IBM
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        public byte[] ManufacturingSerial;
        public byte[] ReportedSerial;
    }

    public static Page_C1_IBM? DecodePage_C1_IBM(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0xC1)
            return null;

        if(pageResponse[3] != 24)
            return null;

        if(pageResponse.Length != 28)
            return null;

        var decoded = new Page_C1_IBM
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4),
            ManufacturingSerial  = new byte[12],
            ReportedSerial       = new byte[12]
        };

        Array.Copy(pageResponse, 4,  decoded.ManufacturingSerial, 0, 12);
        Array.Copy(pageResponse, 16, decoded.ReportedSerial,      0, 12);

        return decoded;
    }

    public static string PrettifyPage_C1_IBM(byte[] pageResponse) =>
        PrettifyPage_C1_IBM(DecodePage_C1_IBM(pageResponse));

    public static string PrettifyPage_C1_IBM(Page_C1_IBM? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_C1_IBM page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.IBM_Drive_Serial_Numbers_page);

        sb.AppendFormat("\t" + Localization.Manufacturing_serial_number_0,
                        StringHandlers.CToString(page.ManufacturingSerial)).
           AppendLine();

        sb.AppendFormat("\t" + Localization.Reported_serial_number_0, StringHandlers.CToString(page.ReportedSerial)).
           AppendLine();

        return sb.ToString();
    }

#endregion EVPD Page 0xC1 (IBM): Drive Serial Numbers page

#region EVPD Page 0xB0: Sequential-access device capabilities page

    /// <summary>Sequential-access device capabilities page Page code 0xB0</summary>
    public struct Page_B0
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public ushort PageLength;
        public bool TSMC;
        public bool WORM;
    }

    public static Page_B0? DecodePage_B0(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0xB0)
            return null;

        if((pageResponse[2] << 8) + pageResponse[3] + 4 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 5)
            return null;

        var decoded = new Page_B0
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (ushort)((pageResponse[2] << 8) + pageResponse[3] + 4),
            TSMC                 = (pageResponse[4] & 0x02) == 0x02,
            WORM                 = (pageResponse[4] & 0x01) == 0x01
        };

        return decoded;
    }

    public static string PrettifyPage_B0(byte[] pageResponse) => PrettifyPage_B0(DecodePage_B0(pageResponse));

    public static string PrettifyPage_B0(Page_B0? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_B0 page = modePage.Value;
        var     sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Sequential_access_Device_Capabilities);

        if(page.WORM)
            sb.AppendLine("\t" + Localization.Device_supports_WORM_media);

        if(page.TSMC)
            sb.AppendLine("\t" + Localization.Device_supports_Tape_Stream_Mirroring);

        return sb.ToString();
    }

#endregion EVPD Page 0xB0: Sequential-access device capabilities page

#region EVPD Pages 0xC0 to 0xC5 (HP): Drive component revision level pages

    /// <summary>Drive component revision level pages Page codes 0xC0 to 0xC5 (HP)</summary>
    public struct Page_C0_to_C5_HP
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        public byte[] Component;
        public byte[] Version;
        public byte[] Date;
        public byte[] Variant;
        public byte[] Copyright;
    }

    public static Page_C0_to_C5_HP? DecodePage_C0_to_C5_HP(byte[] pageResponse)
    {
        if(pageResponse == null)
            return null;

        if(pageResponse[1] != 0xC0 &&
           pageResponse[1] != 0xC1 &&
           pageResponse[1] != 0xC2 &&
           pageResponse[1] != 0xC3 &&
           pageResponse[1] != 0xC4 &&
           pageResponse[1] != 0xC5)
            return null;

        if(pageResponse.Length < 4)
            return null;

        var decoded = new Page_C0_to_C5_HP
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4),
            PageCode             = pageResponse[1]
        };

        if(pageResponse[3] == 92 && pageResponse.Length >= 96)
        {
            decoded.Component = new byte[26];
            decoded.Version   = new byte[19];
            decoded.Date      = new byte[24];
            decoded.Variant   = new byte[23];

            Array.Copy(pageResponse, 4,  decoded.Component, 0, 26);
            Array.Copy(pageResponse, 30, decoded.Version,   0, 19);
            Array.Copy(pageResponse, 49, decoded.Date,      0, 24);
            Array.Copy(pageResponse, 73, decoded.Variant,   0, 23);

            return decoded;
        }

        if(pageResponse[4] != pageResponse[3] - 1)
            return null;

        List<byte> array = new();

        const string fwRegExStr = @"Firmware Rev\s+=\s+(?<fw>\d+\.\d+)\s+Build date\s+=\s+(?<date>(\w|\d|\s*.)*)\s*$";

        const string fwcRegExStr   = @"FW_CONF\s+=\s+(?<value>0x[0-9A-Fa-f]{8})\s*$";
        const string servoRegExStr = @"Servo\s+Rev\s+=\s+(?<version>\d+\.\d+)\s*$";
        var          fwRegEx       = new Regex(fwRegExStr);
        var          fwcRegEx      = new Regex(fwcRegExStr);
        var          servoRegEx    = new Regex(servoRegExStr);

        for(var pos = 5; pos < pageResponse.Length; pos++)
        {
            if(pageResponse[pos] == 0x00)
            {
                string str        = StringHandlers.CToString(array.ToArray());
                Match  fwMatch    = fwRegEx.Match(str);
                Match  fwcMatch   = fwcRegEx.Match(str);
                Match  servoMatch = servoRegEx.Match(str);

                if(str.ToLowerInvariant().StartsWith("copyright", StringComparison.Ordinal))
                    decoded.Copyright = Encoding.ASCII.GetBytes(str);
                else if(fwMatch.Success)
                {
                    decoded.Component = "Firmware"u8.ToArray();
                    decoded.Version   = Encoding.ASCII.GetBytes(fwMatch.Groups["fw"].Value);
                    decoded.Date      = Encoding.ASCII.GetBytes(fwMatch.Groups["date"].Value);
                }
                else if(fwcMatch.Success)
                    decoded.Variant = Encoding.ASCII.GetBytes(fwMatch.Groups["value"].Value);
                else if(servoMatch.Success)
                {
                    decoded.Component = "Servo"u8.ToArray();
                    decoded.Version   = Encoding.ASCII.GetBytes(servoMatch.Groups["version"].Value);
                }

                array = new List<byte>();
            }
            else
                array.Add(pageResponse[pos]);
        }

        return decoded;
    }

    public static string PrettifyPage_C0_to_C5_HP(byte[] pageResponse) =>
        PrettifyPage_C0_to_C5_HP(DecodePage_C0_to_C5_HP(pageResponse));

    public static string PrettifyPage_C0_to_C5_HP(Page_C0_to_C5_HP? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_C0_to_C5_HP page = modePage.Value;
        var              sb   = new StringBuilder();

        switch(page.PageCode)
        {
            case 0xC0:
                sb.AppendLine(Localization.HP_Drive_Firmware_Revision_Levels_page);

                break;
            case 0xC1:
                sb.AppendLine(Localization.HP_Drive_Hardware_Revision_Levels_page);

                break;
            case 0xC2:
                sb.AppendLine(Localization.HP_Drive_PCA_Revision_Levels_page);

                break;
            case 0xC3:
                sb.AppendLine(Localization.HP_Drive_Mechanism_Revision_Levels_page);

                break;
            case 0xC4:
                sb.AppendLine(Localization.HP_Drive_Head_Assembly_Revision_Levels_page);

                break;
            case 0xC5:
                sb.AppendLine(Localization.HP_Drive_ACI_Revision_Levels_page);

                break;
        }

        if(page.Component is { Length: > 0 })
        {
            sb.AppendFormat("\t" + Localization.Certance_Component_0, StringHandlers.CToString(page.Component)).
               AppendLine();
        }

        if(page.Version is { Length: > 0 })
        {
            sb.AppendFormat("\t" + Localization.Certance_Version_0, StringHandlers.CToString(page.Version)).
               AppendLine();
        }

        if(page.Date is { Length: > 0 })
            sb.AppendFormat("\t" + Localization.Certance_Date_0, StringHandlers.CToString(page.Date)).AppendLine();

        if(page.Variant is { Length: > 0 })
        {
            sb.AppendFormat("\t" + Localization.Certance_Variant_0, StringHandlers.CToString(page.Variant)).
               AppendLine();
        }

        if(page.Copyright is { Length: > 0 })
            sb.AppendFormat("\t" + Localization.Copyright_0, StringHandlers.CToString(page.Copyright)).AppendLine();

        return sb.ToString();
    }

#endregion EVPD Pages 0xC0 to 0xC5 (HP): Drive component revision level pages

#region EVPD Page 0xC0 (Seagate): Firmware numbers page

    /// <summary>Firmware numbers page Page code 0xC0 (Seagate)</summary>
    public struct Page_C0_Seagate
    {
        /// <summary>The peripheral qualifier.</summary>
        public PeripheralQualifiers PeripheralQualifier;
        /// <summary>The type of the peripheral device.</summary>
        public PeripheralDeviceTypes PeripheralDeviceType;
        /// <summary>The page code.</summary>
        public byte PageCode;
        /// <summary>The length of the page.</summary>
        public byte PageLength;
        public byte[] ControllerFirmware;
        public byte[] BootFirmware;
        public byte[] ServoFirmware;
    }

    public static Page_C0_Seagate? DecodePage_C0_Seagate(byte[] pageResponse)
    {
        if(pageResponse?[1] != 0xC0)
            return null;

        if(pageResponse[3] != 12)
            return null;

        if(pageResponse.Length != 16)
            return null;

        var decoded = new Page_C0_Seagate
        {
            PeripheralQualifier  = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5),
            PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F),
            PageLength           = (byte)(pageResponse[3] + 4),
            PageCode             = pageResponse[1],
            ControllerFirmware   = new byte[4],
            BootFirmware         = new byte[4],
            ServoFirmware        = new byte[4]
        };

        Array.Copy(pageResponse, 4,  decoded.ControllerFirmware, 0, 4);
        Array.Copy(pageResponse, 8,  decoded.BootFirmware,       0, 4);
        Array.Copy(pageResponse, 12, decoded.ServoFirmware,      0, 4);

        return decoded;
    }

    public static string PrettifyPage_C0_Seagate(byte[] pageResponse) =>
        PrettifyPage_C0_Seagate(DecodePage_C0_Seagate(pageResponse));

    public static string PrettifyPage_C0_Seagate(Page_C0_Seagate? modePage)
    {
        if(!modePage.HasValue)
            return null;

        Page_C0_Seagate page = modePage.Value;
        var             sb   = new StringBuilder();

        sb.AppendLine(Localization.Seagate_Firmware_Numbers_page);

        sb.AppendFormat("\t" + Localization.Controller_firmware_version_0,
                        StringHandlers.CToString(page.ControllerFirmware)).
           AppendLine();

        sb.AppendFormat("\t" + Localization.Boot_firmware_version_0, StringHandlers.CToString(page.BootFirmware)).
           AppendLine();

        sb.AppendFormat("\t" + Localization.Servo_firmware_version_0, StringHandlers.CToString(page.ServoFirmware)).
           AppendLine();

        return sb.ToString();
    }

#endregion EVPD Page 0xC0 (Seagate): Firmware numbers page
}