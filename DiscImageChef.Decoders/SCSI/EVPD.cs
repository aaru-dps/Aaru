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

        public enum IdentificationCodeSet : byte
        {
            /// <summary>
            /// Identifier is binary
            /// </summary>
            Binary = 1,
            /// <summary>
            /// Identifier is pure ASCII
            /// </summary>
            ASCII = 2
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
            /// Identifier is a 64-bit IEEE EUI-64
            /// </summary>
            EUI = 2,
            /// <summary>
            /// Identifier is a 64-bit FC-PH Name_Identifier
            /// </summary>
            FCPH = 3
        }

        public struct IdentificatonDescriptor
        {
            /// <summary>
            /// Defines how the identifier is stored
            /// </summary>
            public IdentificationCodeSet CodeSet;
            /// <summary>
            /// Defines the type of the identifier
            /// </summary>
            public IdentificationTypes Type;
            /// <summary>
            /// Length of the identifier
            /// </summary>
            public byte Length;
            /// <summary>
            /// Identifier as an ASCII string if applicable
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
                descriptor.CodeSet = (IdentificationCodeSet)(pageResponse[position] & 0x0F);
                descriptor.Type = (IdentificationTypes)(pageResponse[position + 1] & 0x0F);
                descriptor.Length = pageResponse[position + 3];
                descriptor.Binary = new byte[descriptor.Length];
                Array.Copy(pageResponse, position + 4, descriptor.Binary, 0, descriptor.Length);
                if(descriptor.CodeSet == IdentificationCodeSet.ASCII)
                    descriptor.ASCII = StringHandlers.CToString(descriptor.Binary);
                else
                    descriptor.ASCII = "";

                position += 4 + descriptor.Length;
            }

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

            sb.AppendLine("SCSI Device identification page:");

            if(page.Descriptors.Length == 0)
            {
                sb.AppendLine("\tThere are no identifiers");
                return sb.ToString();
            }

            foreach(IdentificatonDescriptor descriptor in page.Descriptors)
            {
                switch(descriptor.Type)
                {
                    case IdentificationTypes.NoAuthority:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII)
                            sb.AppendFormat("Vendor descriptor contains: {0}", descriptor.ASCII).AppendLine();
                        else if(descriptor.CodeSet == IdentificationCodeSet.Binary)
                            sb.AppendFormat("Vendor descriptor contains binary data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                        else
                            sb.AppendFormat("Vendor descriptor contains unknown kind {1} of data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40), (byte)descriptor.CodeSet).AppendLine();
                        break;
                    case IdentificationTypes.Inquiry:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII)
                            sb.AppendFormat("Inquiry descriptor contains: {0}", descriptor.ASCII).AppendLine();
                        else if(descriptor.CodeSet == IdentificationCodeSet.Binary)
                            sb.AppendFormat("Inquiry descriptor contains binary data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40)).AppendLine();
                        else
                            sb.AppendFormat("Inquiry descriptor contains unknown kind {1} of data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40), (byte)descriptor.CodeSet).AppendLine();
                        break;
                    case IdentificationTypes.EUI:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII)
                            sb.AppendFormat("IEEE EUI-64: {0}", descriptor.ASCII).AppendLine();
                        else
                            sb.AppendFormat("IEEE EUI-64: {0:X16}", descriptor.Binary).AppendLine();
                        break;
                    case IdentificationTypes.FCPH:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII)
                            sb.AppendFormat("FC-PH Name_Identifier: {0}", descriptor.ASCII).AppendLine();
                        else
                            sb.AppendFormat("FC-PH Name_Identifier: {0:X16}", descriptor.Binary).AppendLine();
                        break;
                    default:
                        if(descriptor.CodeSet == IdentificationCodeSet.ASCII)
                            sb.AppendFormat("Unknown descriptor type {1} contains: {0}", descriptor.ASCII, (byte)descriptor.Type).AppendLine();
                        else if(descriptor.CodeSet == IdentificationCodeSet.Binary)
                            sb.AppendFormat("Inquiry descriptor type {1} contains binary data (hex): {0}", PrintHex.ByteArrayToHexArrayString(descriptor.Binary, 40), (byte)descriptor.Type).AppendLine();
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

