// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Policy;

namespace DiscImageChef.Decoders.SCSI
{
    public static class EVPD
    {
        /// <summary>
        /// Decodes VPD page 0x00: Supported VPD pages
        /// </summary>
        /// <returns>A byte array containing all supported VPD pages.</returns>
        /// <param name="page">Page 0x00.</param>
        public static byte[] DecodePage00(byte[] page)
        {
            if(page == null)
                return null;

            if(page[1] != 0)
                return null;

            if(page.Length != page[3] + 4)
                return null;

            byte[] decoded = new byte[page.Length - 4];

            Array.Copy(page, 4, decoded, 0, page.Length - 4);

            return decoded;
        }

        /// <summary>
        /// Decides VPD pages 0x01 to 0x7F: ASCII Information
        /// </summary>
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

            byte[] ascii = new byte[page[4]];

            Array.Copy(page, 5, ascii, 0, page[4]);

            return StringHandlers.CToString(ascii);
        }

        /// <summary>
        /// Decodes VPD page 0x80: Unit Serial Number
        /// </summary>
        /// <returns>The unit serial number.</returns>
        /// <param name="page">Page 0x80.</param>
        public static string DecodePage80(byte[] page)
        {
            if(page == null)
                return null;

            if(page[1] != 0x80)
                return null;

            if(page.Length != page[3] + 4)
                return null;

            byte[] ascii = new byte[page.Length - 4];

            Array.Copy(page, 4, ascii, 0, page.Length - 4);

            return StringHandlers.CToString(ascii);
        }

        #region EVPD Page 0x81: Implemented operating definition page

        /// <summary>
        /// Implemented operating definition page
        /// Page code 0x81
        /// </summary>
        public struct Page_81
        {
            /// <summary>
            /// The peripheral qualifier.
            /// </summary>
            public PeripheralQualifiers PeripheralQualifier;
            /// <summary>
            /// The type of the peripheral device.
            /// </summary>
            public PeripheralDeviceTypes PeripheralDeviceType;
            /// <summary>
            /// The page code.
            /// </summary>
            public byte PageCode;
            /// <summary>
            /// The length of the page.
            /// </summary>
            public byte PageLength;
            /// <summary>
            /// Current operating definition
            /// </summary>
            public ScsiDefinitions Current;
            /// <summary>
            /// Default operating definition
            /// </summary>
            public ScsiDefinitions Default;
            /// <summary>
            /// Support operating definition list
            /// </summary>
            public ScsiDefinitions[] Supported;
        }

        public static Page_81? DecodePage_81(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if(pageResponse[1] != 0x81)
                return null;

            if(pageResponse[3] + 4 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 6)
                return null;

            Page_81 decoded = new Page_81();

            decoded.PeripheralQualifier = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5);
            decoded.PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F);
            decoded.PageLength = (byte)(pageResponse[3] + 4);
            decoded.Current = (ScsiDefinitions)(pageResponse[4] & 0x7F);
            decoded.Default = (ScsiDefinitions)(pageResponse[5] & 0x7F);

            int position = 6;
            List<ScsiDefinitions> definitions = new List<ScsiDefinitions>();

            while(position < pageResponse.Length)
            {
                ScsiDefinitions definition = (ScsiDefinitions)(pageResponse[position] & 0x7F);
                position++;
                definitions.Add(definition);
            }

            decoded.Supported = definitions.ToArray();

            return decoded;
        }

        public static string PrettifyPage_81(byte[] pageResponse)
        {
            return PrettifyPage_81(DecodePage_81(pageResponse));
        }

        public static string DefinitionToString(ScsiDefinitions definition)
        {
            switch(definition)
            {
                case ScsiDefinitions.Current:
                    return "";
                case ScsiDefinitions.CCS:
                    return "CCS";
                case ScsiDefinitions.SCSI1:
                    return "SCSI-1";
                case ScsiDefinitions.SCSI2:
                    return "SCSI-2";
                case ScsiDefinitions.SCSI3:
                    return "SCSI-3";
                default:
                    return string.Format("Unknown definition code {0}", (byte)definition);
            }
        }

        public static string PrettifyPage_81(Page_81? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Page_81 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Implemented operating definitions:");

            sb.AppendFormat("\tDefault operating definition: {0}", DefinitionToString(page.Current)).AppendLine();
            sb.AppendFormat("\tCurrent operating definition: {0}", DefinitionToString(page.Current)).AppendLine();

            if(page.Supported.Length == 0)
            {
                sb.AppendLine("\tThere are no supported definitions");
                return sb.ToString();
            }

            sb.AppendLine("\tSupported operating definitions:");
            foreach(ScsiDefinitions definition in page.Supported)
                sb.AppendFormat("\t\t{0}", DefinitionToString(definition)).AppendLine();

            return sb.ToString();
        }

        #endregion EVPD Page 0x81: Implemented operating definition page

        /// <summary>
        /// Decodes VPD page 0x82: ASCII implemented operating definition
        /// </summary>
        /// <returns>ASCII implemented operating definition.</returns>
        /// <param name="page">Page 0x82.</param>
        public static string DecodePage82(byte[] page)
        {
            if(page == null)
                return null;

            if(page[1] != 0x82)
                return null;

            if(page.Length != page[3] + 4)
                return null;

            byte[] ascii = new byte[page.Length - 4];

            Array.Copy(page, 4, ascii, 0, page.Length - 4);

            return StringHandlers.CToString(ascii);
        }

        #region EVPD Page 0x83: Device identification page

        public enum IdentificationAssociation : byte
        {
            /// <summary>
            /// Identifier field is associated with the addressed logical unit
            /// </summary>
            LogicalUnit = 0,
            /// <summary>
            /// Identifier field is associated with the target port
            /// </summary>
            TargetPort = 1,
            /// <summary>
            /// Identifier field is associated with the target device that contains the LUN
            /// </summary>
            TargetDevice = 2
        }

        public enum IdentificationCodeSet : byte
        {
            /// <summary>
            /// Identifier is binary
            /// </summary>
            Binary = 1,
            /// <summary>
            /// Identifier is pure ASCII
            /// </summary>
            ASCII = 2,
            /// <summary>
            /// Identifier is in UTF-8
            /// </summary>
            UTF8 = 3
        }

        public enum IdentificationTypes : byte
        {
            /// <summary>
            /// No assignment authority was used and there is no guarantee the identifier is unique
            /// </summary>
            NoAuthority = 0,
            /// <summary>
            /// Concatenates vendor and product identifier from INQUIRY plus unit serial number from page 80h
            /// </summary>
            Inquiry = 1,
            /// <summary>
            /// Identifier is a 64-bit IEEE EUI-64, or extended
            /// </summary>
            EUI = 2,
            /// <summary>
            /// Identifier is compatible with 64-bit FC-PH Name_Identifier
            /// </summary>
            NAA = 3,
            /// <summary>
            /// Identifier to relative port in device
            /// </summary>
            Relative = 4,
            /// <summary>
            /// Identifier to group of target ports in device
            /// </summary>
            TargetPortGroup = 5,
            /// <summary>
            /// Identifier to group of target LUNs in device
            /// </summary>
            LogicalUnitGroup = 6,
            /// <summary>
            /// MD5 of device identification values
            /// </summary>
            MD5 = 7,
            /// <summary>
            /// SCSI name string
            /// </summary>
            SCSI = 8,
            /// <summary>
            /// Protocol specific port identifier
            /// </summary>
            ProtocolSpecific = 9
        }

        public struct IdentificatonDescriptor
        {
            /// <summary>
            /// Protocol identifier
            /// </summary>
            public ProtocolIdentifiers ProtocolIdentifier;
            /// <summary>
            /// Defines how the identifier is stored
            /// </summary>
            public IdentificationCodeSet CodeSet;
            /// <summary>
            /// Set if protocol identifier is valid
            /// </summary>
            public bool PIV;
            /// <summary>
            /// Identifies which decide the identifier associates with
            /// </summary>
            public IdentificationAssociation Association;
            /// <summary>
            /// Defines the type of the identifier
            /// </summary>
            public IdentificationTypes Type;
            /// <summary>
            /// Length of the identifier
            /// </summary>
            public byte Length;
            /// <summary>
            /// Identifier as a string if applicable
            /// </summary>
            public string ASCII;
            /// <summary>
            /// Binary identifier
            /// </summary>
            public byte[] Binary;
        }

        /// <summary>
        /// Device identification page
        /// Page code 0x83
        /// </summary>
        public struct Page_83
        {
            /// <summary>
            /// The peripheral qualifier.
            /// </summary>
            public PeripheralQualifiers PeripheralQualifier;
            /// <summary>
            /// The type of the peripheral device.
            /// </summary>
            public PeripheralDeviceTypes PeripheralDeviceType;
            /// <summary>
            /// The page code.
            /// </summary>
            public byte PageCode;
            /// <summary>
            /// The length of the page.
            /// </summary>
            public byte PageLength;
            /// <summary>
            /// The descriptors.
            /// </summary>
            public IdentificatonDescriptor[] Descriptors;
        }

        public static Page_83? DecodePage_83(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if(pageResponse[1] != 0x83)
                return null;

            if(pageResponse[3] + 4 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 6)
                return null;

            Page_83 decoded = new Page_83();

            decoded.PeripheralQualifier = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5);
            decoded.PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F);
            decoded.PageLength = (byte)(pageResponse[3] + 4);

            int position = 4;
            List<IdentificatonDescriptor> descriptors = new List<IdentificatonDescriptor>();

            while(position < pageResponse.Length)
            {
                IdentificatonDescriptor descriptor = new IdentificatonDescriptor();
                descriptor.ProtocolIdentifier = (ProtocolIdentifiers)((pageResponse[position] & 0xF0) >> 4);
                descriptor.CodeSet = (IdentificationCodeSet)(pageResponse[position] & 0x0F);
                descriptor.PIV |= (pageResponse[position + 1] & 0x80) == 0x80;
                descriptor.Association = (IdentificationAssociation)((pageResponse[position + 1] & 0x30) >> 4);
                descriptor.Type = (IdentificationTypes)(pageResponse[position + 1] & 0x0F);
                descriptor.Length = pageResponse[position + 3];
                descriptor.Binary = new byte[descriptor.Length];
                Array.Copy(pageResponse, position + 4, descriptor.Binary, 0, descriptor.Length);
                if(descriptor.CodeSet == IdentificationCodeSet.ASCII)
                    descriptor.ASCII = StringHandlers.CToString(descriptor.Binary);
                else if(descriptor.CodeSet == IdentificationCodeSet.UTF8)
                    descriptor.ASCII = Encoding.UTF8.GetString(descriptor.Binary);
                else
                    descriptor.ASCII = "";

                position += 4 + descriptor.Length;
                descriptors.Add(descriptor);
            }

            decoded.Descriptors = descriptors.ToArray();

            return decoded;
        }

        public static string PrettifyPage_83(byte[] pageResponse)
        {
            return PrettifyPage_83(DecodePage_83(pageResponse));
        }

        public static string PrettifyPage_83(Page_83? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Page_83 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Device identification:");

            if(page.Descriptors.Length == 0)
            {
                sb.AppendLine("\tThere are no identifiers");
                return sb.ToString();
            }

            foreach(IdentificatonDescriptor descriptor in page.Descriptors)
            {
                switch(descriptor.Association)
                {
                    case IdentificationAssociation.LogicalUnit:
                        sb.AppendLine("\tIdentifier belongs to addressed logical unit");
                        break;
                    case IdentificationAssociation.TargetPort:
                        sb.AppendLine("\tIdentifier belongs to target port");
                        break;
                    case IdentificationAssociation.TargetDevice:
                        sb.AppendLine("\tIdentifier belongs to target device that contains the addressed logical unit");
                        break;
                    default:
                        sb.AppendFormat("\tIndentifier has unknown association with code {0}", (byte)descriptor.Association).AppendLine();
                        break;
                }

                if(descriptor.PIV)
                {
                    string protocol = "";
                    switch(descriptor.ProtocolIdentifier)
                    {
                        case ProtocolIdentifiers.ADT:
                            protocol = "Automation/Drive Interface Transport";
                            break;
                        case ProtocolIdentifiers.ATA:
                            protocol = "AT Attachment Interface (ATA/ATAPI)";
                            break;
                        case ProtocolIdentifiers.FibreChannel:
                            protocol = "Fibre Channel";
                            break;
                        case ProtocolIdentifiers.Firewire:
                            protocol = "IEEE 1394";
                            break;
                        case ProtocolIdentifiers.iSCSI:
                            protocol = "Internet SCSI";
                            break;
                        case ProtocolIdentifiers.NoProtocol:
                            protocol = "no specific";
                            break;
                        case ProtocolIdentifiers.PCIe:
                            protocol = "PCI Express";
                            break;
                        case ProtocolIdentifiers.RDMAP:
                            protocol = "SCSI Remote Direct Memory Access";
                            break;
                        case ProtocolIdentifiers.SAS:
                            protocol = "Serial Attachment SCSI";
                            break;
                        case ProtocolIdentifiers.SCSI:
                            protocol = "Parallel SCSI";
                            break;
                        case ProtocolIdentifiers.SCSIe:
                            protocol = "SCSI over PCI Express";
                            break;
                        case ProtocolIdentifiers.SSA:
                            protocol = "SSA";
                            break;
                        case ProtocolIdentifiers.UAS:
                            protocol = "USB Attached SCSI";
                            break;
                        default:
                            protocol = string.Format("unknown code {0}", (byte)descriptor.ProtocolIdentifier);
                            break;
                    }
                    sb.AppendFormat("\tDescriptor referes to {0} protocol", protocol).AppendLine();
                }

                switch(descriptor.Type)
                {
                    case IdentificationTypes.NoAuthority:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tVendor descriptor contains: {0}", descriptor.ASCII).AppendLine();
                        else if(descriptor.CodeSet == IdentificationCodeSet.Binary)
                            sb.AppendFormat("\tVendor descriptor contains binary data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                        else
                            sb.AppendFormat("\tVendor descriptor contains unknown kind {1} of data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40), (byte)descriptor.CodeSet).AppendLine();
                        break;
                    case IdentificationTypes.Inquiry:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tInquiry descriptor contains: {0}", descriptor.ASCII).AppendLine();
                        else if(descriptor.CodeSet == IdentificationCodeSet.Binary)
                            sb.AppendFormat("\tInquiry descriptor contains binary data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                        else
                            sb.AppendFormat("\tInquiry descriptor contains unknown kind {1} of data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40), (byte)descriptor.CodeSet).AppendLine();
                        break;
                    case IdentificationTypes.EUI:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tIEEE EUI-64: {0}", descriptor.ASCII).AppendLine();
                        else
                        {
                            sb.AppendFormat("\tIEEE EUI-64: {0:X2}", descriptor.Binary[0]);
                            for(int i = 1; i < descriptor.Binary.Length; i++)
                                sb.AppendFormat(":{0:X2}", descriptor.Binary[i]);
                            sb.AppendLine();
                        }
                        break;
                    case IdentificationTypes.NAA:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tNAA: {0}", descriptor.ASCII).AppendLine();
                        else
                        {
                            sb.AppendFormat("\tNAA: {0:X2}", descriptor.Binary[0]);
                            for(int i = 1; i < descriptor.Binary.Length; i++)
                                sb.AppendFormat(":{0:X2}", descriptor.Binary[i]);
                            sb.AppendLine();
                        }
                        break;
                    case IdentificationTypes.Relative:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tRelative target port identifier: {0}", descriptor.ASCII).AppendLine();
                        else
                            sb.AppendFormat("\tRelative target port identifier: {0}", (descriptor.Binary[2] << 8) + descriptor.Binary[3]).AppendLine();
                        break;
                    case IdentificationTypes.TargetPortGroup:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tTarget group identifier: {0}", descriptor.ASCII).AppendLine();
                        else
                            sb.AppendFormat("\tTarget group identifier: {0}", (descriptor.Binary[2] << 8) + descriptor.Binary[3]).AppendLine();
                        break;
                    case IdentificationTypes.LogicalUnitGroup:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tLogical unit group identifier: {0}", descriptor.ASCII).AppendLine();
                        else
                            sb.AppendFormat("\tLogical unit group identifier: {0}", (descriptor.Binary[2] << 8) + descriptor.Binary[3]).AppendLine();
                        break;
                    case IdentificationTypes.MD5:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tMD5 logical unit identifier: {0}", descriptor.ASCII).AppendLine();
                        else
                        {
                            sb.AppendFormat("\tMD5 logical unit identifier: {0:x2}", descriptor.Binary[0]);
                            for(int i = 1; i < descriptor.Binary.Length; i++)
                                sb.AppendFormat("{0:x2}", descriptor.Binary[i]);
                            sb.AppendLine();
                        }
                        break;
                    case IdentificationTypes.SCSI:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tSCSI name string identifier: {0}", descriptor.ASCII).AppendLine();
                        else
                        {
                            sb.AppendFormat("\tSCSI name string identifier (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                        }
                        break;
                    case IdentificationTypes.ProtocolSpecific:
                        {
                            if(descriptor.PIV)
                            {
                                switch(descriptor.ProtocolIdentifier)
                                {
                                    case ProtocolIdentifiers.ADT:
                                        sb.AppendFormat("\tProtocol (Automation/Drive Interface Transport) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.ATA:
                                        sb.AppendFormat("\tProtocol (ATA/ATAPI) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.FibreChannel:
                                        sb.AppendFormat("\tProtocol (Fibre Channel) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.Firewire:
                                        sb.AppendFormat("\tProtocol (IEEE 1394) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.iSCSI:
                                        sb.AppendFormat("\tProtocol (Internet SCSI) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.NoProtocol:
                                        sb.AppendFormat("\tProtocol (unknown) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.PCIe:
                                        sb.AppendFormat("\tProtocol (PCI Express) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.RDMAP:
                                        sb.AppendFormat("\tProtocol (SCSI Remote Direct Memory Access) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.SAS:
                                        sb.AppendFormat("\tProtocol (Serial Attachment SCSI) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.SCSI:
                                        sb.AppendFormat("\tProtocol (Parallel SCSI) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.SSA:
                                        sb.AppendFormat("\tProtocol (SSA) specific descriptor with unknown format (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.SCSIe:
                                        sb.AppendFormat("\tProtocol (SCSIe) specific descriptor: Routing ID is {0}", (descriptor.Binary[0] << 8) + descriptor.Binary[1]).AppendLine();
                                        break;
                                    case ProtocolIdentifiers.UAS:
                                        sb.AppendFormat("\tProtocol (UAS) specific descriptor: USB address {0} interface {1}", descriptor.Binary[0] & 0x7F, descriptor.Binary[2]).AppendLine();
                                        break;
                                    default:
                                        sb.AppendFormat("\tProtocol (unknown code {0}) specific descriptor with unknown format (hex): {1}", (byte)descriptor.ProtocolIdentifier, PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                                        break;
                                }
                            }
                        }
                        break;
                    default:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII || descriptor.CodeSet == IdentificationCodeSet.UTF8)
                            sb.AppendFormat("\tUnknown descriptor type {1} contains: {0}", descriptor.ASCII, (byte)descriptor.Type).AppendLine();
                        else if(descriptor.CodeSet == IdentificationCodeSet.Binary)
                            sb.AppendFormat("\tUnknown descriptor type {1} contains binary data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40), (byte)descriptor.Type).AppendLine();
                        else
                            sb.AppendFormat("Inquiry descriptor type {2} contains unknown kind {1} of data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40), (byte)descriptor.CodeSet, (byte)descriptor.Type).AppendLine();
                        break;
                }
            }

            return sb.ToString();
        }

        #endregion EVPD Page 0x83: Device identification page

        #region EVPD Page 0x84: Software Interface Identification page

        public struct SoftwareIdentifier
        {
            /// <summary>
            /// EUI-48 identifier
            /// </summary>
            public byte[] Identifier;
        }

        /// <summary>
        /// Software Interface Identification page
        /// Page code 0x84
        /// </summary>
        public struct Page_84
        {
            /// <summary>
            /// The peripheral qualifier.
            /// </summary>
            public PeripheralQualifiers PeripheralQualifier;
            /// <summary>
            /// The type of the peripheral device.
            /// </summary>
            public PeripheralDeviceTypes PeripheralDeviceType;
            /// <summary>
            /// The page code.
            /// </summary>
            public byte PageCode;
            /// <summary>
            /// The length of the page.
            /// </summary>
            public byte PageLength;
            /// <summary>
            /// The descriptors.
            /// </summary>
            public SoftwareIdentifier[] Identifiers;
        }

        public static Page_84? DecodePage_84(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if(pageResponse[1] != 0x84)
                return null;

            if(pageResponse[3] + 4 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 10)
                return null;

            Page_84 decoded = new Page_84();

            decoded.PeripheralQualifier = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5);
            decoded.PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F);
            decoded.PageLength = (byte)(pageResponse[3] + 4);

            int position = 4;
            List<SoftwareIdentifier> identifiers = new List<SoftwareIdentifier>();

            while(position < pageResponse.Length)
            {
                SoftwareIdentifier identifier = new SoftwareIdentifier();
                identifier.Identifier = new byte[6];
                Array.Copy(pageResponse, position, identifier.Identifier, 0, 6);
                identifiers.Add(identifier);
                position += 6;
            }

            decoded.Identifiers = identifiers.ToArray();

            return decoded;
        }

        public static string PrettifyPage_84(byte[] pageResponse)
        {
            return PrettifyPage_84(DecodePage_84(pageResponse));
        }

        public static string PrettifyPage_84(Page_84? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Page_84 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Software Interface Identifiers:");

            if(page.Identifiers.Length == 0)
            {
                sb.AppendLine("\tThere are no identifiers");
                return sb.ToString();
            }

            foreach(SoftwareIdentifier identifier in page.Identifiers)
            {
                sb.AppendFormat("\t{0:X2}", identifier.Identifier[0]);
                for(int i = 1; i < identifier.Identifier.Length; i++)
                sb.AppendFormat(":{0:X2}", identifier.Identifier[i]);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion EVPD Page 0x84: Software Interface Identification page

        #region EVPD Page 0x85: Management Network Addresses page

        public enum NetworkServiceTypes : byte
        {
            Unspecified = 0,
            StorageConf = 1,
            Diagnostics = 2,
            Status = 3,
            Logging = 4,
            CodeDownload = 5,
            CopyService = 6,
            Administrative = 7
        }

        public struct NetworkDescriptor
        {
            /// <summary>
            /// Identifies which device the identifier associates with
            /// </summary>
            public IdentificationAssociation Association;
            /// <summary>
            /// Defines the type of the identifier
            /// </summary>
            public NetworkServiceTypes Type;
            /// <summary>
            /// Length of the identifier
            /// </summary>
            public ushort Length;
            /// <summary>
            /// Binary identifier
            /// </summary>
            public byte[] Address;
        }

        /// <summary>
        /// Device identification page
        /// Page code 0x85
        /// </summary>
        public struct Page_85
        {
            /// <summary>
            /// The peripheral qualifier.
            /// </summary>
            public PeripheralQualifiers PeripheralQualifier;
            /// <summary>
            /// The type of the peripheral device.
            /// </summary>
            public PeripheralDeviceTypes PeripheralDeviceType;
            /// <summary>
            /// The page code.
            /// </summary>
            public byte PageCode;
            /// <summary>
            /// The length of the page.
            /// </summary>
            public ushort PageLength;
            /// <summary>
            /// The descriptors.
            /// </summary>
            public NetworkDescriptor[] Descriptors;
        }

        public static Page_85? DecodePage_85(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if(pageResponse[1] != 0x85)
                return null;

            if((pageResponse[2] << 8) + pageResponse[3] + 4 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 4)
                return null;

            Page_85 decoded = new Page_85();

            decoded.PeripheralQualifier = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5);
            decoded.PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F);
            decoded.PageLength = (ushort)((pageResponse[2] << 8) + pageResponse[3] + 4);

            int position = 4;
            List<NetworkDescriptor> descriptors = new List<NetworkDescriptor>();

            while(position < pageResponse.Length)
            {
                NetworkDescriptor descriptor = new NetworkDescriptor();
                descriptor.Association = (IdentificationAssociation)((pageResponse[position] & 0x60) >> 5);
                descriptor.Type = (NetworkServiceTypes)(pageResponse[position] & 0x1F);
                descriptor.Length = (ushort)((pageResponse[position + 2] << 8) + pageResponse[position + 3]);
                descriptor.Address = new byte[descriptor.Length];
                Array.Copy(pageResponse, position + 4, descriptor.Address, 0, descriptor.Length);

                position += 4 + descriptor.Length;
                descriptors.Add(descriptor);
            }

            decoded.Descriptors = descriptors.ToArray();

            return decoded;
        }

        public static string PrettifyPage_85(byte[] pageResponse)
        {
            return PrettifyPage_85(DecodePage_85(pageResponse));
        }

        public static string PrettifyPage_85(Page_85? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Page_85 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Management Network Addresses:");

            if(page.Descriptors.Length == 0)
            {
                sb.AppendLine("\tThere are no addresses");
                return sb.ToString();
            }

            foreach(NetworkDescriptor descriptor in page.Descriptors)
            {
                switch(descriptor.Association)
                {
                    case IdentificationAssociation.LogicalUnit:
                        sb.AppendLine("\tIdentifier belongs to addressed logical unit");
                        break;
                    case IdentificationAssociation.TargetPort:
                        sb.AppendLine("\tIdentifier belongs to target port");
                        break;
                    case IdentificationAssociation.TargetDevice:
                        sb.AppendLine("\tIdentifier belongs to target device that contains the addressed logical unit");
                        break;
                    default:
                        sb.AppendFormat("\tIndentifier has unknown association with code {0}", (byte)descriptor.Association).AppendLine();
                        break;
                }

                switch(descriptor.Type)
                {
                    case NetworkServiceTypes.CodeDownload:
                        sb.AppendFormat("Address for code download: {0}", StringHandlers.CToString(descriptor.Address)).AppendLine();
                        break;
                    case NetworkServiceTypes.Diagnostics:
                        sb.AppendFormat("Address for diagnostics: {0}", StringHandlers.CToString(descriptor.Address)).AppendLine();
                        break;
                    case NetworkServiceTypes.Logging:
                        sb.AppendFormat("Address for logging: {0}", StringHandlers.CToString(descriptor.Address)).AppendLine();
                        break;
                    case NetworkServiceTypes.Status:
                        sb.AppendFormat("Address for status: {0}", StringHandlers.CToString(descriptor.Address)).AppendLine();
                        break;
                    case NetworkServiceTypes.StorageConf:
                        sb.AppendFormat("Address for storage configuration service: {0}", StringHandlers.CToString(descriptor.Address)).AppendLine();
                        break;
                    case NetworkServiceTypes.Unspecified:
                        sb.AppendFormat("Unspecified address: {0}", StringHandlers.CToString(descriptor.Address)).AppendLine();
                        break;
                    case NetworkServiceTypes.CopyService:
                        sb.AppendFormat("Address for copy service: {0}", StringHandlers.CToString(descriptor.Address)).AppendLine();
                        break;
                    case NetworkServiceTypes.Administrative:
                        sb.AppendFormat("Address for administrative configuration service: {0}", StringHandlers.CToString(descriptor.Address)).AppendLine();
                        break;
                    default:
                        sb.AppendFormat("Address of unknown type {1}: {0}", StringHandlers.CToString(descriptor.Address), (byte)descriptor.Type).AppendLine();
                        break;
                }
            }

            return sb.ToString();
        }

        #endregion EVPD Page 0x85: Management Network Addresses page

        #region EVPD Page 0x86: Extended INQUIRY data page

        /// <summary>
        /// Device identification page
        /// Page code 0x86
        /// </summary>
        public struct Page_86
        {
            /// <summary>
            /// The peripheral qualifier.
            /// </summary>
            public PeripheralQualifiers PeripheralQualifier;
            /// <summary>
            /// The type of the peripheral device.
            /// </summary>
            public PeripheralDeviceTypes PeripheralDeviceType;
            /// <summary>
            /// The page code.
            /// </summary>
            public byte PageCode;
            /// <summary>
            /// The length of the page.
            /// </summary>
            public byte PageLength;
            /// <summary>
            /// Indicates how a device server activates microcode
            /// </summary>
            public byte ActivateMicrocode;
            /// <summary>
            /// Protection types supported by device
            /// </summary>
            public byte SPT;
            /// <summary>
            /// Checks logical block guard field
            /// </summary>
            public bool GRD_CHK;
            /// <summary>
            /// Checks logical block application tag
            /// </summary>
            public bool APP_CHK;
            /// <summary>
            /// Checks logical block reference
            /// </summary>
            public bool REF_CHK;
            /// <summary>
            /// Supports unit attention condition sense key specific data
            /// </summary>
            public bool UASK_SUP;
            /// <summary>
            /// Supports grouping
            /// </summary>
            public bool GROUP_SUP;
            /// <summary>
            /// Supports priority
            /// </summary>
            public bool PRIOR_SUP;
            /// <summary>
            /// Supports head of queue
            /// </summary>
            public bool HEADSUP;
            /// <summary>
            /// Supports ordered
            /// </summary>
            public bool ORDSUP;
            /// <summary>
            /// Supports simple
            /// </summary>
            public bool SIMPSUP;
            /// <summary>
            /// Supports marking a block as uncorrectable
            /// </summary>
            public bool WU_SUP;
            /// <summary>
            /// Supports disabling correction on WRITE LONG
            /// </summary>
            public bool CRD_SUP;
            /// <summary>
            /// Supports a non-volatile cache
            /// </summary>
            public bool NV_SUP;
            /// <summary>
            /// Supports a volatile cache
            /// </summary>
            public bool V_SUP;
            /// <summary>
            /// Disable protection information checks
            /// </summary>
            public bool NO_PI_CHK;
            /// <summary>
            /// Protection information interval supported
            /// </summary>
            public bool P_I_I_SUP;
            /// <summary>
            /// Clears all LUNs unit attention when clearing one
            /// </summary>
            public bool LUICLR;
            /// <summary>
            /// Referrals support
            /// </summary>
            public bool R_SUP;
            /// <summary>
            /// History snapshots release effects
            /// </summary>
            public bool HSSRELEF;
            /// <summary>
            /// Capability based command security
            /// </summary>
            public bool CBCS;
            /// <summary>
            /// Indicates how it handles microcode updating with multiple nexuxes
            /// </summary>
            public byte Nexus;
            /// <summary>
            /// Time to complete extended self-test
            /// </summary>
            public ushort ExtendedTestMinutes;
            /// <summary>
            /// Power on activation support
            /// </summary>
            public bool POA_SUP;
            /// <summary>
            /// Hard reset actication
            /// </summary>
            public bool HRA_SUP;
            /// <summary>
            /// Vendor specific activation
            /// </summary>
            public bool VSA_SUP;
            /// <summary>
            /// Maximum length in bytes of sense data
            /// </summary>
            public byte MaximumSenseLength;
        }

        public static Page_86? DecodePage_86(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if(pageResponse[1] != 0x86)
                return null;

            if(pageResponse[3] + 4 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 64)
                return null;

            Page_86 decoded = new Page_86();

            decoded.PeripheralQualifier = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5);
            decoded.PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F);
            decoded.PageLength = (byte)(pageResponse[3] + 4);

            decoded.ActivateMicrocode = (byte)((pageResponse[4] & 0xC0) >> 6);
            decoded.SPT = (byte)((pageResponse[4] & 0x38) >> 3);
            decoded.GRD_CHK |= (pageResponse[4] & 0x04) == 0x04;
            decoded.APP_CHK |= (pageResponse[4] & 0x02) == 0x02;
            decoded.REF_CHK |= (pageResponse[4] & 0x01) == 0x01;
            decoded.UASK_SUP |= (pageResponse[5] & 0x20) == 0x20;
            decoded.GROUP_SUP |= (pageResponse[5] & 0x10) == 0x10;
            decoded.PRIOR_SUP |= (pageResponse[5] & 0x08) == 0x08;
            decoded.HEADSUP |= (pageResponse[5] & 0x04) == 0x04;
            decoded.ORDSUP |= (pageResponse[5] & 0x02) == 0x02;
            decoded.SIMPSUP |= (pageResponse[5] & 0x01) == 0x01;
            decoded.WU_SUP |= (pageResponse[6] & 0x08) == 0x08;
            decoded.CRD_SUP |= (pageResponse[6] & 0x04) == 0x04;
            decoded.NV_SUP |= (pageResponse[6] & 0x02) == 0x02;
            decoded.V_SUP |= (pageResponse[6] & 0x01) == 0x01;
            decoded.NO_PI_CHK |= (pageResponse[7] & 0x20) == 0x20;
            decoded.P_I_I_SUP |= (pageResponse[7] & 0x10) == 0x10;
            decoded.LUICLR |= (pageResponse[7] & 0x01) == 0x01;
            decoded.R_SUP |= (pageResponse[8] & 0x10) == 0x10;
            decoded.HSSRELEF |= (pageResponse[8] & 0x02) == 0x02;
            decoded.CBCS |= (pageResponse[8] & 0x01) == 0x01;
            decoded.Nexus = (byte)(pageResponse[9] & 0x0F);
            decoded.ExtendedTestMinutes = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.POA_SUP |= (pageResponse[12] & 0x80) == 0x80;
            decoded.HRA_SUP |= (pageResponse[12] & 0x40) == 0x40;
            decoded.VSA_SUP |= (pageResponse[12] & 0x20) == 0x20;
            decoded.MaximumSenseLength = pageResponse[13];

            return decoded;
        }

        public static string PrettifyPage_86(byte[] pageResponse)
        {
            return PrettifyPage_86(DecodePage_86(pageResponse));
        }

        public static string PrettifyPage_86(Page_86? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Page_86 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Extended INQUIRY Data:");

            if(page.PeripheralDeviceType == PeripheralDeviceTypes.DirectAccess ||
               page.PeripheralDeviceType == PeripheralDeviceTypes.SCSIZonedBlockDEvice)
            {
                switch(page.SPT)
                {
                    case 0:
                        sb.AppendLine("Logical unit supports type 1 protection");
                        break;
                    case 1:
                        sb.AppendLine("Logical unit supports types 1 and 2 protection");
                        break;
                    case 2:
                        sb.AppendLine("Logical unit supports type 2 protection");
                        break;
                    case 3:
                        sb.AppendLine("Logical unit supports types 1 and 3 protection");
                        break;
                    case 4:
                        sb.AppendLine("Logical unit supports type 3 protection");
                        break;
                    case 5:
                        sb.AppendLine("Logical unit supports types 2 and 3 protection");
                        break;
                    case 7:
                        sb.AppendLine("Logical unit supports types 1, 2 and 3 protection");
                        break;
                    default:
                        sb.AppendFormat("Logical unit supports unknown protection defined by code {0}", (byte)page.SPT).AppendLine();
                        break;
                }
            }
            else if(page.PeripheralDeviceType == PeripheralDeviceTypes.SequentialAccess && page.SPT == 1)
                sb.AppendLine("Logical unit supports logical block protection");

            if(page.GRD_CHK)
                sb.AppendLine("Device checks the logical block guard");
            if(page.APP_CHK)
                sb.AppendLine("Device checks the logical block application tag");
            if(page.REF_CHK)
                sb.AppendLine("Device checks the logical block reference tag");
            if(page.UASK_SUP)
                sb.AppendLine("Device supports unit attention condition sense key specific data");
            if(page.GROUP_SUP)
                sb.AppendLine("Device supports grouping");
            if(page.PRIOR_SUP)
                sb.AppendLine("Device supports priority");
            if(page.HEADSUP)
                sb.AppendLine("Device supports head of queue");
            if(page.ORDSUP)
                sb.AppendLine("Device supports the ORDERED task attribute");
            if(page.SIMPSUP)
                sb.AppendLine("Device supports the SIMPLE task attribute");
            if(page.WU_SUP)
                sb.AppendLine("Device supports marking a block as uncorrectable with WRITE LONG");
            if(page.CRD_SUP)
                sb.AppendLine("Device supports disabling correction with WRITE LONG");
            if(page.NV_SUP)
                sb.AppendLine("Device has a non-volatile cache");
            if(page.V_SUP)
                sb.AppendLine("Device has a volatile cache");
            if(page.NO_PI_CHK)
                sb.AppendLine("Device has disabled protection information checks");
            if(page.P_I_I_SUP)
                sb.AppendLine("Device supports protection information intervals");
            if(page.LUICLR)
                sb.AppendLine("Device clears any unit attention condition in all LUNs after reporting for any LUN");
            if(page.R_SUP)
                sb.AppendLine("Device supports referrals");
            if(page.HSSRELEF)
                sb.AppendLine("Devoce implements alternate reset handling");
            if(page.CBCS)
                sb.AppendLine("Device supports capability-based command security");
            if(page.POA_SUP)
                sb.AppendLine("Device supports power-on activation for new microcode");
            if(page.HRA_SUP)
                sb.AppendLine("Device supports hard reset activation for new microcode");
            if(page.VSA_SUP)
                sb.AppendLine("Device supports vendor specific activation for new microcode");

            if(page.ExtendedTestMinutes > 0)
                sb.AppendFormat("Extended self-test takes {0} to complete", TimeSpan.FromMinutes(page.ExtendedTestMinutes)).AppendLine();

            if(page.MaximumSenseLength > 0)
                sb.AppendFormat("Device supports a maximum of {0} bytes for sense data", page.MaximumSenseLength).AppendLine();

            return sb.ToString();
        }

        #endregion EVPD Page 0x86: Extended INQUIRY data page

        #region EVPD Page 0x89: ATA Information page

        /// <summary>
        /// ATA Information page
        /// Page code 0x89
        /// </summary>
        public struct Page_89
        {
            /// <summary>
            /// The peripheral qualifier.
            /// </summary>
            public PeripheralQualifiers PeripheralQualifier;
            /// <summary>
            /// The type of the peripheral device.
            /// </summary>
            public PeripheralDeviceTypes PeripheralDeviceType;
            /// <summary>
            /// The page code.
            /// </summary>
            public byte PageCode;
            /// <summary>
            /// The length of the page.
            /// </summary>
            public ushort PageLength;
            /// <summary>
            /// Contains the SAT vendor identification
            /// </summary>
            public byte[] VendorIdentification;
            /// <summary>
            /// Contains the SAT product identification
            /// </summary>
            public byte[] ProductIdentification;
            /// <summary>
            /// Contains the SAT revision level
            /// </summary>
            public byte[] ProductRevisionLevel;
            /// <summary>
            /// Contains the ATA device signature
            /// </summary>
            public byte[] Signature;
            /// <summary>
            /// Contains the command code used to identify the device
            /// </summary>
            public byte CommandCode;
            /// <summary>
            /// Contains the response to ATA IDENTIFY (PACKET) DEVICE
            /// </summary>
            public byte[] IdentifyData;
        }

        public static Page_89? DecodePage_89(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if(pageResponse[1] != 0x89)
                return null;

            if((pageResponse[2] << 8) + pageResponse[3] + 4 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 572)
                return null;

            Page_89 decoded = new Page_89();

            decoded.PeripheralQualifier = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5);
            decoded.PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F);
            decoded.PageLength = (ushort)((pageResponse[2] << 8) + pageResponse[3] + 4);

            decoded.VendorIdentification = new byte[8];
            decoded.ProductIdentification = new byte[16];
            decoded.ProductRevisionLevel = new byte[4];
            decoded.Signature = new byte[20];
            decoded.IdentifyData = new byte[512];

            Array.Copy(pageResponse, 8, decoded.VendorIdentification, 0, 8);
            Array.Copy(pageResponse, 8, decoded.ProductIdentification, 0, 16);
            Array.Copy(pageResponse, 8, decoded.ProductRevisionLevel, 0, 4);
            Array.Copy(pageResponse, 8, decoded.Signature, 0, 20);
            decoded.CommandCode = pageResponse[56];
            Array.Copy(pageResponse, 8, decoded.IdentifyData, 0, 512);

            return decoded;
        }

        public static string PrettifyPage_89(byte[] pageResponse)
        {
            return PrettifyPage_89(DecodePage_89(pageResponse));
        }

        // TODO: Decode ATA signature?
        public static string PrettifyPage_89(Page_89? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Page_89 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI to ATA Translation Layer Data:");

            sb.AppendFormat("\tTranslation layer vendor: {0}", VendorString.Prettify(StringHandlers.CToString(page.VendorIdentification).Trim())).AppendLine();
            sb.AppendFormat("\tTranslation layer name: {0}", StringHandlers.CToString(page.ProductIdentification).Trim()).AppendLine();
            sb.AppendFormat("\tTranslation layer release level: {0}", StringHandlers.CToString(page.ProductRevisionLevel).Trim()).AppendLine();
            switch(page.CommandCode)
            {
                case 0xEC:
                    sb.AppendLine("\tDevice responded to ATA IDENTIFY DEVICE command.");
                    break;
                case 0xA1:
                    sb.AppendLine("\tDevice responded to ATA IDENTIFY PACKET DEVICE command.");
                    break;
                default:
                    sb.AppendFormat("\tDevice responded to ATA command {0:X2}h", page.CommandCode).AppendLine();
                    break;
            }
            switch(page.Signature[0])
            {
                case 0x00:
                    sb.AppendLine("\tDevice uses Parallel ATA.");
                    break;
                case 0x34:
                    sb.AppendLine("\tDevice uses Serial ATA.");
                    break;
                default:
                    sb.AppendFormat("\tDevice uses unknown transport with code {0}", page.Signature[0]).AppendLine();
                    break;
            }

            ATA.Identify.IdentifyDevice? id = ATA.Identify.Decode(page.IdentifyData);
            if(id.HasValue)
            {
                sb.AppendLine("\tATA IDENTIFY information follows:");
                sb.AppendFormat("{0}", ATA.Identify.Prettify(id)).AppendLine();
            }
            else
                sb.AppendLine("\tCould not decode ATA IDENTIFY information");

            return sb.ToString();
        }

        #endregion EVPD Page 0x89: ATA Information page

        #region EVPD Page 0xC0 (Quantum): Firmware Build Information page

        /// <summary>
        /// Firmware Build Information page
        /// Page code 0xC0 (Quantum)
        /// </summary>
        public struct Page_C0_Quantum
        {
            /// <summary>
            /// The peripheral qualifier.
            /// </summary>
            public PeripheralQualifiers PeripheralQualifier;
            /// <summary>
            /// The type of the peripheral device.
            /// </summary>
            public PeripheralDeviceTypes PeripheralDeviceType;
            /// <summary>
            /// The page code.
            /// </summary>
            public byte PageCode;
            /// <summary>
            /// The length of the page.
            /// </summary>
            public byte PageLength;
            /// <summary>
            /// Servo firmware checksum
            /// </summary>
            public ushort ServoFirmwareChecksum;
            /// <summary>
            /// Servo EEPROM checksum
            /// </summary>
            public ushort ServoEEPROMChecksum;
            /// <summary>
            /// Read/Write firmware checksum
            /// </summary>
            public uint ReadWriteFirmwareChecksum;
            /// <summary>
            /// Read/Write firmware build data
            /// </summary>
            public byte[] ReadWriteFirmwareBuildData;
        }

        public static Page_C0_Quantum? DecodePage_C0_Quantum(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if(pageResponse[1] != 0xC0)
                return null;

            if(pageResponse[3] != 20)
                return null;

            if(pageResponse.Length != 36)
                return null;

            Page_C0_Quantum decoded = new Page_C0_Quantum();

            decoded.PeripheralQualifier = (PeripheralQualifiers)((pageResponse[0] & 0xE0) >> 5);
            decoded.PeripheralDeviceType = (PeripheralDeviceTypes)(pageResponse[0] & 0x1F);
            decoded.PageLength = (byte)(pageResponse[3] + 4);

            decoded.ServoFirmwareChecksum = (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.ServoEEPROMChecksum = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.ReadWriteFirmwareChecksum = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);
            decoded.ReadWriteFirmwareBuildData = new byte[24];
            Array.Copy(pageResponse, 12, decoded.ReadWriteFirmwareBuildData, 0, 24);

            return decoded;
        }

        public static string PrettifyPage_C0_Quantum(byte[] pageResponse)
        {
            return PrettifyPage_C0_Quantum(DecodePage_C0_Quantum(pageResponse));
        }

        // TODO: Decode ATA signature?
        public static string PrettifyPage_C0_Quantum(Page_C0_Quantum? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Page_C0_Quantum page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Quantum Firmware Build Information page:");

            sb.AppendFormat("\tServo firmware checksum: 0x{0:X4}", page.ServoFirmwareChecksum).AppendLine();
            sb.AppendFormat("\tEEPROM firmware checksum: 0x{0:X4}", page.ServoEEPROMChecksum).AppendLine();
            sb.AppendFormat("\tRead/write firmware checksum: 0x{0:X8}", page.ReadWriteFirmwareChecksum).AppendLine();
            sb.AppendFormat("\tRead/write firmware build date: {0}", StringHandlers.CToString(page.ReadWriteFirmwareBuildData)).AppendLine();

            return sb.ToString();
        }

        #endregion EVPD Page 0xC0 (Quantum): Firmware Build Information page

    }
}

