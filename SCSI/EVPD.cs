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
    }
}

