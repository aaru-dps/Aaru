/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : SCSI.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Decoders.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Decodes SCSI structures.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Decoders.SCSI
{
    /// <summary>
    /// Information from the following standards:
    /// T9/375-D revision 10l
    /// T10/995-D revision 10
    /// T10/1236-D revision 20
    /// T10/1416-D revision 23
    /// T10/1731-D revision 16
    /// T10/502 revision 05
    /// RFC 7144
    /// ECMA-111
    /// </summary>
    public static class Inquiry
    {
        #region Public methods

        public static SCSIInquiry? Decode(byte[] SCSIInquiryResponse)
        {
            if (SCSIInquiryResponse == null)
                return null;

            if (SCSIInquiryResponse.Length < 36 && SCSIInquiryResponse.Length != 5)
            {
                DicConsole.DebugWriteLine("SCSI INQUIRY decoder", "INQUIRY response is {0} bytes, less than minimum of 36 bytes, decoded data can be incorrect, not decoding.", SCSIInquiryResponse.Length);
                return null;
            }

            if (SCSIInquiryResponse.Length != SCSIInquiryResponse[4] + 5)
            {
                DicConsole.DebugWriteLine("SCSI INQUIRY decoder", "INQUIRY response length ({0} bytes) is different than specified in length field ({1} bytes), decoded data can be incorrect, not decoding.", SCSIInquiryResponse.Length, SCSIInquiryResponse[4] + 4);
                return null;
            }

            SCSIInquiry decoded = new SCSIInquiry();

            if (SCSIInquiryResponse.Length >= 1)
            {
                decoded.PeripheralQualifier = (byte)((SCSIInquiryResponse[0] & 0xE0) >> 5);
                decoded.PeripheralDeviceType = (byte)(SCSIInquiryResponse[0] & 0x1F);
            }
            if (SCSIInquiryResponse.Length >= 2)
            {
                decoded.RMB = Convert.ToBoolean((SCSIInquiryResponse[1] & 0x80));
                decoded.DeviceTypeModifier = (byte)(SCSIInquiryResponse[1] & 0x7F);
            }
            if (SCSIInquiryResponse.Length >= 3)
            {
                decoded.ISOVersion = (byte)((SCSIInquiryResponse[2] & 0xC0) >> 6);
                decoded.ECMAVersion = (byte)((SCSIInquiryResponse[2] & 0x38) >> 3);
                decoded.ANSIVersion = (byte)(SCSIInquiryResponse[2] & 0x07);
            }
            if (SCSIInquiryResponse.Length >= 4)
            {
                decoded.AERC = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x80));
                decoded.TrmTsk = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x40));
                decoded.NormACA = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x20));
                decoded.HiSup = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x10));
                decoded.ResponseDataFormat = (byte)(SCSIInquiryResponse[3] & 0x07);
            }
            if (SCSIInquiryResponse.Length >= 5)
                decoded.AdditionalLength = SCSIInquiryResponse[4];
            if (SCSIInquiryResponse.Length >= 6)
            {
                decoded.SCCS = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x80));
                decoded.ACC = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x40));
                decoded.TPGS = (byte)((SCSIInquiryResponse[5] & 0x30) >> 4);
                decoded.ThreePC = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x08));
                decoded.Reserved2 = (byte)((SCSIInquiryResponse[5] & 0x06) >> 1);
                decoded.Protect = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 7)
            {
                decoded.BQue = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x80));
                decoded.EncServ = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x40));
                decoded.VS1 = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x20));
                decoded.MultiP = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x10));
                decoded.MChngr = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x08));
                decoded.ACKREQQ = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x04));
                decoded.Addr32 = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x02));
                decoded.Addr16 = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 8)
            {
                decoded.RelAddr = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x80));
                decoded.WBus32 = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x40));
                decoded.WBus16 = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x20));
                decoded.Sync = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x10));
                decoded.Linked = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x08));
                decoded.TranDis = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x04));
                decoded.CmdQue = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x02));
                decoded.SftRe = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 16)
            {
                decoded.VendorIdentification = new byte[8];
                Array.Copy(SCSIInquiryResponse, 8, decoded.VendorIdentification, 0, 8);
            }
            if (SCSIInquiryResponse.Length >= 32)
            {
                decoded.ProductIdentification = new byte[16];
                Array.Copy(SCSIInquiryResponse, 16, decoded.ProductIdentification, 0, 16);
            }
            if (SCSIInquiryResponse.Length >= 36)
            {
                decoded.ProductRevisionLevel = new byte[4];
                Array.Copy(SCSIInquiryResponse, 32, decoded.ProductRevisionLevel, 0, 4);
            }
            if (SCSIInquiryResponse.Length >= 56)
            {
                decoded.VendorSpecific = new byte[20];
                Array.Copy(SCSIInquiryResponse, 36, decoded.VendorSpecific, 0, 20);
            }
            if (SCSIInquiryResponse.Length >= 57)
            {
                decoded.Reserved3 = (byte)((SCSIInquiryResponse[56] & 0xF0) >> 4);
                decoded.Clocking = (byte)((SCSIInquiryResponse[56] & 0x0C) >> 2);
                decoded.QAS = Convert.ToBoolean((SCSIInquiryResponse[56] & 0x02));
                decoded.IUS = Convert.ToBoolean((SCSIInquiryResponse[56] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 58)
                decoded.Reserved4 = SCSIInquiryResponse[57];
            if (SCSIInquiryResponse.Length >= 60)
            {
                int descriptorsNo;

                if (SCSIInquiryResponse.Length >= 74)
                    descriptorsNo = 8;
                else
                    descriptorsNo = (SCSIInquiryResponse.Length - 58) / 2;
                
                decoded.VersionDescriptors = new ushort[descriptorsNo];
                for (int i = 0; i < descriptorsNo; i++)
                {
                    decoded.VersionDescriptors[i] = BitConverter.ToUInt16(SCSIInquiryResponse, 58 + (i * 2));
                }
            }
            if (SCSIInquiryResponse.Length >= 75 && SCSIInquiryResponse.Length < 96)
            {
                decoded.Reserved5 = new byte[SCSIInquiryResponse.Length - 74];
                Array.Copy(SCSIInquiryResponse, 74, decoded.Reserved5, 0, SCSIInquiryResponse.Length - 74);
            }
            if (SCSIInquiryResponse.Length >= 96)
            {
                decoded.Reserved5 = new byte[22];
                Array.Copy(SCSIInquiryResponse, 74, decoded.Reserved5, 0, 22);
            }
            if (SCSIInquiryResponse.Length > 96)
            {
                decoded.VendorSpecific2 = new byte[SCSIInquiryResponse.Length - 96];
                Array.Copy(SCSIInquiryResponse, 96, decoded.VendorSpecific2, 0, SCSIInquiryResponse.Length - 96);
            }

            return decoded;
        }

        public static string Prettify(SCSIInquiry? SCSIInquiryResponse)
        {
            if (SCSIInquiryResponse == null)
                return null;

            SCSIInquiry response = SCSIInquiryResponse.Value;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Device vendor: {0}", VendorString.Prettify(StringHandlers.CToString(response.VendorIdentification).Trim())).AppendLine();
            sb.AppendFormat("Device name: {0}", StringHandlers.CToString(response.ProductIdentification).Trim()).AppendLine();
            sb.AppendFormat("Device release level: {0}", StringHandlers.CToString(response.ProductRevisionLevel).Trim()).AppendLine();
            switch ((PeripheralQualifiers)response.PeripheralQualifier)
            {
                case PeripheralQualifiers.Supported:
                    sb.AppendLine("Device is connected and supported.");
                    break;
                case PeripheralQualifiers.Unconnected:
                    sb.AppendLine("Device is supported but not connected.");
                    break;
                case PeripheralQualifiers.Reserved:
                    sb.AppendLine("Reserved value set in Peripheral Qualifier field.");
                    break;
                case PeripheralQualifiers.Unsupported:
                    sb.AppendLine("Device is connected but unsupported.");
                    break;
                default:
                    sb.AppendFormat("Vendor value {0} set in Peripheral Qualifier field.", response.PeripheralQualifier).AppendLine();
                    break;
            }

            switch ((PeripheralDeviceTypes)response.PeripheralDeviceType)
            {
                case PeripheralDeviceTypes.DirectAccess: //0x00,
                    sb.AppendLine("Direct-access device");
                    break;
                case PeripheralDeviceTypes.SequentialAccess: //0x01,
                    sb.AppendLine("Sequential-access device");
                    break;
                case PeripheralDeviceTypes.PrinterDevice: //0x02,
                    sb.AppendLine("Printer device");
                    break;
                case PeripheralDeviceTypes.ProcessorDevice: //0x03,
                    sb.AppendLine("Processor device");
                    break;
                case PeripheralDeviceTypes.WriteOnceDevice: //0x04,
                    sb.AppendLine("Write-once device");
                    break;
                case PeripheralDeviceTypes.MultiMediaDevice: //0x05,
                    sb.AppendLine("CD-ROM/DVD/etc device");
                    break;
                case PeripheralDeviceTypes.ScannerDevice: //0x06,
                    sb.AppendLine("Scanner device");
                    break;
                case PeripheralDeviceTypes.OpticalDevice: //0x07,
                    sb.AppendLine("Optical memory device");
                    break;
                case PeripheralDeviceTypes.MediumChangerDevice: //0x08,
                    sb.AppendLine("Medium change device");
                    break;
                case PeripheralDeviceTypes.CommsDevice: //0x09,
                    sb.AppendLine("Communications device");
                    break;
                case PeripheralDeviceTypes.PrePressDevice1: //0x0A,
                    sb.AppendLine("Graphics arts pre-press device (defined in ASC IT8)");
                    break;
                case PeripheralDeviceTypes.PrePressDevice2: //0x0B,
                    sb.AppendLine("Graphics arts pre-press device (defined in ASC IT8)");
                    break;
                case PeripheralDeviceTypes.ArrayControllerDevice: //0x0C,
                    sb.AppendLine("Array controller device");
                    break;
                case PeripheralDeviceTypes.EnclosureServiceDevice: //0x0D,
                    sb.AppendLine("Enclosure services device");
                    break;
                case PeripheralDeviceTypes.SimplifiedDevice: //0x0E,
                    sb.AppendLine("Simplified direct-access device");
                    break;
                case PeripheralDeviceTypes.OCRWDevice: //0x0F,
                    sb.AppendLine("Optical card reader/writer device");
                    break;
                case PeripheralDeviceTypes.BridgingExpander: //0x10,
                    sb.AppendLine("Bridging Expanders");
                    break;
                case PeripheralDeviceTypes.ObjectDevice: //0x11,
                    sb.AppendLine("Object-based Storage Device");
                    break;
                case PeripheralDeviceTypes.ADCDevice: //0x12,
                    sb.AppendLine("Automation/Drive Interface");
                    break;
                case PeripheralDeviceTypes.SCSISecurityManagerDevice: //0x13,
                    sb.AppendLine("Security Manager Device");
                    break;
                case PeripheralDeviceTypes.SCSIZonedBlockDEvice: //0x14
                    sb.AppendLine("Host managed zoned block device");
                    break;
                case PeripheralDeviceTypes.WellKnownDevice: //0x1E,
                    sb.AppendLine("Well known logical unit");
                    break;
                case PeripheralDeviceTypes.UnknownDevice: //0x1F
                    sb.AppendLine("Unknown or no device type");
                    break;
                default:
                    sb.AppendFormat("Unknown device type field value 0x{0:X2}", response.PeripheralDeviceType).AppendLine();
                    break;
            }

            switch ((ANSIVersions)response.ANSIVersion)
            {
                case ANSIVersions.ANSINoVersion:
                    sb.AppendLine("Device does not claim to comply with any SCSI ANSI standard");
                    break;
                case ANSIVersions.ANSI1986Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.131:1986 (SCSI-1)");
                    break;
                case ANSIVersions.ANSI1994Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.131:1994 (SCSI-2)");
                    break;
                case ANSIVersions.ANSI1997Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.301:1997 (SPC-1)");
                    break;
                case ANSIVersions.ANSI2001Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.351:2001 (SPC-2)");
                    break;
                case ANSIVersions.ANSI2005Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.408:2005 (SPC-3)");
                    break;
                case ANSIVersions.ANSI2008Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.408:2005 (SPC-4)");
                    break;
                default:
                    sb.AppendFormat("Device claims to comply with unknown SCSI ANSI standard value 0x{0:X2})", response.ANSIVersion).AppendLine();
                    break;
            }

            switch ((ECMAVersions)response.ECMAVersion)
            {
                case ECMAVersions.ECMANoVersion:
                    sb.AppendLine("Device does not claim to comply with any SCSI ECMA standard");
                    break;
                case ECMAVersions.ECMA111:
                    sb.AppendLine("Device claims to comply ECMA-111: Small Computer System Interface SCSI");
                    break;
                default:
                    sb.AppendFormat("Device claims to comply with unknown SCSI ECMA standard value 0x{0:X2})", response.ECMAVersion).AppendLine();
                    break;
            }

            switch ((ISOVersions)response.ISOVersion)
            {
                case ISOVersions.ISONoVersion:
                    sb.AppendLine("Device does not claim to comply with any SCSI ISO/IEC standard");
                    break;
                case ISOVersions.ISO1995Version:
                    sb.AppendLine("Device claims to comply with ISO/IEC 9316:1995");
                    break;
                default:
                    sb.AppendFormat("Device claims to comply with unknown SCSI ISO/IEC standard value 0x{0:X2})", response.ISOVersion).AppendLine();
                    break;
            }

            if (response.RMB)
                sb.AppendLine("Device is removable");
            if (response.AERC)
                sb.AppendLine("Device supports Asynchronous Event Reporting Capability");
            if (response.TrmTsk)
                sb.AppendLine("Device supports TERMINATE TASK command");
            if (response.NormACA)
                sb.AppendLine("Device supports setting Normal ACA");
            if (response.HiSup)
                sb.AppendLine("Device supports LUN hierarchical addressing");
            if (response.SCCS)
                sb.AppendLine("Device contains an embedded storage array controller");
            if (response.ACC)
                sb.AppendLine("Device contains an Access Control Coordinator");
            if (response.ThreePC)
                sb.AppendLine("Device supports third-party copy commands");
            if (response.Protect)
                sb.AppendLine("Device supports protection information");
            if (response.BQue)
                sb.AppendLine("Device supports basic queueing");
            if (response.EncServ)
                sb.AppendLine("Device contains an embedded enclosure services component");
            if (response.MultiP)
                sb.AppendLine("Multi-port device");
            if (response.MChngr)
                sb.AppendLine("Device contains or is attached to a medium changer");
            if (response.ACKREQQ)
                sb.AppendLine("Device supports request and acknowledge handshakes");
            if (response.Addr32)
                sb.AppendLine("Device supports 32-bit wide SCSI addresses");
            if (response.Addr16)
                sb.AppendLine("Device supports 16-bit wide SCSI addresses");
            if (response.RelAddr)
                sb.AppendLine("Device supports relative addressing");
            if (response.WBus32)
                sb.AppendLine("Device supports 32-bit wide data transfers");
            if (response.WBus16)
                sb.AppendLine("Device supports 16-bit wide data transfers");
            if (response.Sync)
                sb.AppendLine("Device supports synchronous data transfer");
            if (response.Linked)
                sb.AppendLine("Device supports linked commands");
            if (response.TranDis)
                sb.AppendLine("Device supports CONTINUE TASK and TARGET TRANSFER DISABLE commands");
            if (response.QAS)
                sb.AppendLine("Device supports Quick Arbitration and Selection");
            if (response.CmdQue)
                sb.AppendLine("Device supports TCQ queue");
            if (response.IUS)
                sb.AppendLine("Device supports information unit transfers");
            if (response.SftRe)
                sb.AppendLine("Device implements RESET as a soft reset");
            #if DEBUG
            if (response.VS1)
                sb.AppendLine("Vendor specific bit 5 on byte 6 of INQUIRY response is set");
            #endif

            switch ((TGPSValues)response.TPGS)
            {
                case TGPSValues.NotSupported:
                    sb.AppendLine("Device does not support assymetrical access");
                    break;
                case TGPSValues.OnlyImplicit:
                    sb.AppendLine("Device only supports implicit assymetrical access");
                    break;
                case TGPSValues.OnlyExplicit:
                    sb.AppendLine("Device only supports explicit assymetrical access");
                    break;
                case TGPSValues.Both:
                    sb.AppendLine("Device supports implicit and explicit assymetrical access");
                    break;
                default:
                    sb.AppendFormat("Unknown value in TPGS field 0x{0:X2}", response.TPGS).AppendLine();
                    break;
            }

            switch ((SPIClocking)response.Clocking)
            {
                case SPIClocking.ST:
                    sb.AppendLine("Device supports only ST clocking");
                    break;
                case SPIClocking.DT:
                    sb.AppendLine("Device supports only DT clocking");
                    break;
                case SPIClocking.Reserved:
                    sb.AppendLine("Reserved value 0x02 found in SPI clocking field");
                    break;
                case SPIClocking.STandDT:
                    sb.AppendLine("Device supports ST and DT clocking");
                    break;
                default:
                    sb.AppendFormat("Unknown value in SPI clocking field 0x{0:X2}", response.Clocking).AppendLine();
                    break;
            }

            if (response.VersionDescriptors != null)
            {
                foreach (UInt16 VersionDescriptor in response.VersionDescriptors)
                {
                    switch (VersionDescriptor)
                    {
                        case 0xFFFF:
                        case 0x0000:
                            break;
                        case 0x0020:
                            sb.AppendLine("Device complies with SAM (no version claimed)");
                            break;
                        case 0x003B:
                            sb.AppendLine("Device complies with SAM T10/0994-D revision 18");
                            break;
                        case 0x003C:
                            sb.AppendLine("Device complies with SAM ANSI INCITS 270-1996");
                            break;
                        case 0x0040:
                            sb.AppendLine("Device complies with SAM-2 (no version claimed)");
                            break;
                        case 0x0054:
                            sb.AppendLine("Device complies with SAM-2 T10/1157-D revision 23");
                            break;
                        case 0x0055:
                            sb.AppendLine("Device complies with SAM-2 T10/1157-D revision 24");
                            break;
                        case 0x005C:
                            sb.AppendLine("Device complies with SAM-2 ANSI INCITS 366-2003");
                            break;
                        case 0x005E:
                            sb.AppendLine("Device complies with SAM-2 ISO/IEC 14776-412");
                            break;
                        case 0x0060:
                            sb.AppendLine("Device complies with SAM-3 (no version claimed)");
                            break;
                        case 0x0062:
                            sb.AppendLine("Device complies with SAM-3 T10/1561-D revision 7");
                            break;
                        case 0x0075:
                            sb.AppendLine("Device complies with SAM-3 T10/1561-D revision 13");
                            break;
                        case 0x0076:
                            sb.AppendLine("Device complies with SAM-3 T10/1561-D revision 14");
                            break;
                        case 0x0077:
                            sb.AppendLine("Device complies with SAM-3 ANSI INCITS 402-2005");
                            break;
                        case 0x0080:
                            sb.AppendLine("Device complies with SAM-4 (no version claimed)");
                            break;
                        case 0x0087:
                            sb.AppendLine("Device complies with SAM-4 T10/1683-D revision 13");
                            break;
                        case 0x008B:
                            sb.AppendLine("Device complies with SAM-4 T10/1683-D revision 14");
                            break;
                        case 0x0090:
                            sb.AppendLine("Device complies with SAM-4 ANSI INCITS 447-2008");
                            break;
                        case 0x0092:
                            sb.AppendLine("Device complies with SAM-4 ISO/IEC 14776-414");
                            break;
                        case 0x00A0:
                            sb.AppendLine("Device complies with SAM-5 (no version claimed)");
                            break;
                        case 0x00A2:
                            sb.AppendLine("Device complies with SAM-5 T10/2104-D revision 4");
                            break;
                        case 0x00A4:
                            sb.AppendLine("Device complies with SAM-5 T10/2104-D revision 20");
                            break;
                        case 0x00A6:
                            sb.AppendLine("Device complies with SAM-5 T10/2104-D revision 21");
                            break;
                        case 0x00C0:
                            sb.AppendLine("Device complies with SAM-6 (no version claimed)");
                            break;
                        case 0x0120:
                            sb.AppendLine("Device complies with SPC (no version claimed)");
                            break;
                        case 0x013B:
                            sb.AppendLine("Device complies with SPC T10/0995-D revision 11a");
                            break;
                        case 0x013C:
                            sb.AppendLine("Device complies with SPC ANSI INCITS 301-1997");
                            break;
                        case 0x0140:
                            sb.AppendLine("Device complies with MMC (no version claimed)");
                            break;
                        case 0x015B:
                            sb.AppendLine("Device complies with MMC T10/1048-D revision 10a");
                            break;
                        case 0x015C:
                            sb.AppendLine("Device complies with MMC ANSI INCITS 304-1997");
                            break;
                        case 0x0160:
                            sb.AppendLine("Device complies with SCC (no version claimed)");
                            break;
                        case 0x017B:
                            sb.AppendLine("Device complies with SCC T10/1047-D revision 06c");
                            break;
                        case 0x017C:
                            sb.AppendLine("Device complies with SCC ANSI INCITS 276-1997");
                            break;
                        case 0x0180:
                            sb.AppendLine("Device complies with SBC (no version claimed)");
                            break;
                        case 0x019B:
                            sb.AppendLine("Device complies with SBC T10/0996-D revision 08c");
                            break;
                        case 0x019C:
                            sb.AppendLine("Device complies with SBC ANSI INCITS 306-1998");
                            break;
                        case 0x01A0:
                            sb.AppendLine("Device complies with SMC (no version claimed)");
                            break;
                        case 0x01BB:
                            sb.AppendLine("Device complies with SMC T10/0999-D revision 10a");
                            break;
                        case 0x01BC:
                            sb.AppendLine("Device complies with SMC ANSI INCITS 314-1998");
                            break;
                        case 0x01BE:
                            sb.AppendLine("Device complies with SMC ISO/IEC 14776-351");
                            break;
                        case 0x01C0:
                            sb.AppendLine("Device complies with SES (no version claimed)");
                            break;
                        case 0x01DB:
                            sb.AppendLine("Device complies with SES T10/1212-D revision 08b");
                            break;
                        case 0x01DC:
                            sb.AppendLine("Device complies with SES ANSI INCITS 305-1998");
                            break;
                        case 0x01DD:
                            sb.AppendLine("Device complies with SES T10/1212 revision 08b w/ Amendment ANSI INCITS.305/AM1-2000");
                            break;
                        case 0x01DE:
                            sb.AppendLine("Device complies with SES ANSI INCITS 305-1998 w/ Amendment ANSI INCITS.305/AM1-2000");
                            break;
                        case 0x01E0:
                            sb.AppendLine("Device complies with SCC-2 (no version claimed)");
                            break;
                        case 0x01FB:
                            sb.AppendLine("Device complies with SCC-2 T10/1125-D revision 04");
                            break;
                        case 0x01FC:
                            sb.AppendLine("Device complies with SCC-2 ANSI INCITS 318-1998");
                            break;
                        case 0x0200:
                            sb.AppendLine("Device complies with SSC (no version claimed)");
                            break;
                        case 0x0201:
                            sb.AppendLine("Device complies with SSC T10/0997-D revision 17");
                            break;
                        case 0x0207:
                            sb.AppendLine("Device complies with SSC T10/0997-D revision 22");
                            break;
                        case 0x021C:
                            sb.AppendLine("Device complies with SSC ANSI INCITS 335-2000");
                            break;
                        case 0x0220:
                            sb.AppendLine("Device complies with RBC (no version claimed)");
                            break;
                        case 0x0238:
                            sb.AppendLine("Device complies with RBC T10/1240-D revision 10a");
                            break;
                        case 0x023C:
                            sb.AppendLine("Device complies with RBC ANSI INCITS 330-2000");
                            break;
                        case 0x0240:
                            sb.AppendLine("Device complies with MMC-2 (no version claimed)");
                            break;
                        case 0x0255:
                            sb.AppendLine("Device complies with MMC-2 T10/1228-D revision 11");
                            break;
                        case 0x025B:
                            sb.AppendLine("Device complies with MMC-2 T10/1228-D revision 11a");
                            break;
                        case 0x025C:
                            sb.AppendLine("Device complies with MMC-2 ANSI INCITS 333-2000");
                            break;
                        case 0x0260:
                            sb.AppendLine("Device complies with SPC-2 (no version claimed)");
                            break;
                        case 0x0267:
                            sb.AppendLine("Device complies with SPC-2 T10/1236-D revision 12");
                            break;
                        case 0x0269:
                            sb.AppendLine("Device complies with SPC-2 T10/1236-D revision 18");
                            break;
                        case 0x0275:
                            sb.AppendLine("Device complies with SPC-2 T10/1236-D revision 19");
                            break;
                        case 0x0276:
                            sb.AppendLine("Device complies with SPC-2 T10/1236-D revision 20");
                            break;
                        case 0x0277:
                            sb.AppendLine("Device complies with SPC-2 ANSI INCITS 351-2001");
                            break;
                        case 0x0278:
                            sb.AppendLine("Device complies with SPC-2 ISO/IEC 14776-452");
                            break;
                        case 0x0280:
                            sb.AppendLine("Device complies with OCRW (no version claimed)");
                            break;
                        case 0x029E:
                            sb.AppendLine("Device complies with OCRW ISO/IEC 14776-381");
                            break;
                        case 0x02A0:
                            sb.AppendLine("Device complies with MMC-3 (no version claimed)");
                            break;
                        case 0x02B5:
                            sb.AppendLine("Device complies with MMC-3 T10/1363-D revision 9");
                            break;
                        case 0x02B6:
                            sb.AppendLine("Device complies with MMC-3 T10/1363-D revision 10g");
                            break;
                        case 0x02B8:
                            sb.AppendLine("Device complies with MMC-3 ANSI INCITS 360-2002");
                            break;
                        case 0x02E0:
                            sb.AppendLine("Device complies with SMC-2 (no version claimed)");
                            break;
                        case 0x02F5:
                            sb.AppendLine("Device complies with SMC-2 T10/1383-D revision 5");
                            break;
                        case 0x02FC:
                            sb.AppendLine("Device complies with SMC-2 T10/1383-D revision 6");
                            break;
                        case 0x02FD:
                            sb.AppendLine("Device complies with SMC-2 T10/1383-D revision 7");
                            break;
                        case 0x02FE:
                            sb.AppendLine("Device complies with SMC-2 ANSI INCITS 382-2004");
                            break;
                        case 0x0300:
                            sb.AppendLine("Device complies with SPC-3 (no version claimed)");
                            break;
                        case 0x0301:
                            sb.AppendLine("Device complies with SPC-3 T10/1416-D revision 7");
                            break;
                        case 0x0307:
                            sb.AppendLine("Device complies with SPC-3 T10/1416-D revision 21");
                            break;
                        case 0x030F:
                            sb.AppendLine("Device complies with SPC-3 T10/1416-D revision 22");
                            break;
                        case 0x0312:
                            sb.AppendLine("Device complies with SPC-3 T10/1416-D revision 23");
                            break;
                        case 0x0314:
                            sb.AppendLine("Device complies with SPC-3 ANSI INCITS 408-2005");
                            break;
                        case 0x0316:
                            sb.AppendLine("Device complies with SPC-3 ISO/IEC 14776-453");
                            break;
                        case 0x0320:
                            sb.AppendLine("Device complies with SBC-2 (no version claimed)");
                            break;
                        case 0x0322:
                            sb.AppendLine("Device complies with SBC-2 T10/1417-D revision 5a");
                            break;
                        case 0x0324:
                            sb.AppendLine("Device complies with SBC-2 T10/1417-D revision 15");
                            break;
                        case 0x033B:
                            sb.AppendLine("Device complies with SBC-2 T10/1417-D revision 16");
                            break;
                        case 0x033D:
                            sb.AppendLine("Device complies with SBC-2 ANSI INCITS 405-2005");
                            break;
                        case 0x033E:
                            sb.AppendLine("Device complies with SBC-2 ISO/IEC 14776-322");
                            break;
                        case 0x0340:
                            sb.AppendLine("Device complies with OSD (no version claimed)");
                            break;
                        case 0x0341:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 0");
                            break;
                        case 0x0342:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 7a");
                            break;
                        case 0x0343:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 8");
                            break;
                        case 0x0344:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 9");
                            break;
                        case 0x0355:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 10");
                            break;
                        case 0x0356:
                            sb.AppendLine("Device complies with OSD ANSI INCITS 400-2004");
                            break;
                        case 0x0360:
                            sb.AppendLine("Device complies with SSC-2 (no version claimed)");
                            break;
                        case 0x0374:
                            sb.AppendLine("Device complies with SSC-2 T10/1434-D revision 7");
                            break;
                        case 0x0375:
                            sb.AppendLine("Device complies with SSC-2 T10/1434-D revision 9");
                            break;
                        case 0x037D:
                            sb.AppendLine("Device complies with SSC-2 ANSI INCITS 380-2003");
                            break;
                        case 0x0380:
                            sb.AppendLine("Device complies with BCC (no version claimed)");
                            break;
                        case 0x03A0:
                            sb.AppendLine("Device complies with MMC-4 (no version claimed)");
                            break;
                        case 0x03B0:
                            sb.AppendLine("Device complies with MMC-4 T10/1545-D revision 5");
                            break;
                        case 0x03B1:
                            sb.AppendLine("Device complies with MMC-4 T10/1545-D revision 5a");
                            break;
                        case 0x03BD:
                            sb.AppendLine("Device complies with MMC-4 T10/1545-D revision 3");
                            break;
                        case 0x03BE:
                            sb.AppendLine("Device complies with MMC-4 T10/1545-D revision 3d");
                            break;
                        case 0x03BF:
                            sb.AppendLine("Device complies with MMC-4 ANSI INCITS 401-2005");
                            break;
                        case 0x03C0:
                            sb.AppendLine("Device complies with ADC (no version claimed)");
                            break;
                        case 0x03D5:
                            sb.AppendLine("Device complies with ADC T10/1558-D revision 6");
                            break;
                        case 0x03D6:
                            sb.AppendLine("Device complies with ADC T10/1558-D revision 7");
                            break;
                        case 0x03D7:
                            sb.AppendLine("Device complies with ADC ANSI INCITS 403-2005");
                            break;
                        case 0x03E0:
                            sb.AppendLine("Device complies with SES-2 (no version claimed)");
                            break;
                        case 0x03E1:
                            sb.AppendLine("Device complies with SES-2 T10/1559-D revision 16");
                            break;
                        case 0x03E7:
                            sb.AppendLine("Device complies with SES-2 T10/1559-D revision 19");
                            break;
                        case 0x03EB:
                            sb.AppendLine("Device complies with SES-2 T10/1559-D revision 20");
                            break;
                        case 0x03F0:
                            sb.AppendLine("Device complies with SES-2 ANSI INCITS 448-2008");
                            break;
                        case 0x03F2:
                            sb.AppendLine("Device complies with SES-2 ISO/IEC 14776-372");
                            break;
                        case 0x0400:
                            sb.AppendLine("Device complies with SSC-3 (no version claimed)");
                            break;
                        case 0x0403:
                            sb.AppendLine("Device complies with SSC-3 T10/1611-D revision 04a");
                            break;
                        case 0x0407:
                            sb.AppendLine("Device complies with SSC-3 T10/1611-D revision 05");
                            break;
                        case 0x0409:
                            sb.AppendLine("Device complies with SSC-3 ANSI INCITS 467-2011");
                            break;
                        case 0x040B:
                            sb.AppendLine("Device complies with SSC-3 ISO/IEC 14776-333:2013");
                            break;
                        case 0x0420:
                            sb.AppendLine("Device complies with MMC-5 (no version claimed)");
                            break;
                        case 0x042F:
                            sb.AppendLine("Device complies with MMC-5 T10/1675-D revision 03");
                            break;
                        case 0x0431:
                            sb.AppendLine("Device complies with MMC-5 T10/1675-D revision 03b");
                            break;
                        case 0x0432:
                            sb.AppendLine("Device complies with MMC-5 T10/1675-D revision 04");
                            break;
                        case 0x0434:
                            sb.AppendLine("Device complies with MMC-5 ANSI INCITS 430-2007");
                            break;
                        case 0x0440:
                            sb.AppendLine("Device complies with OSD-2 (no version claimed)");
                            break;
                        case 0x0444:
                            sb.AppendLine("Device complies with OSD-2 T10/1729-D revision 4");
                            break;
                        case 0x0446:
                            sb.AppendLine("Device complies with OSD-2 T10/1729-D revision 5");
                            break;
                        case 0x0448:
                            sb.AppendLine("Device complies with OSD-2 ANSI INCITS 458-2011");
                            break;
                        case 0x0460:
                            sb.AppendLine("Device complies with SPC-4 (no version claimed)");
                            break;
                        case 0x0461:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 16");
                            break;
                        case 0x0462:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 18");
                            break;
                        case 0x0463:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 23");
                            break;
                        case 0x0466:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 36");
                            break;
                        case 0x0468:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 37");
                            break;
                        case 0x0469:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 37a");
                            break;
                        case 0x046C:
                            sb.AppendLine("Device complies with SPC-4 ANSI INCITS 513-2015");
                            break;
                        case 0x0480:
                            sb.AppendLine("Device complies with SMC-3 (no version claimed)");
                            break;
                        case 0x0482:
                            sb.AppendLine("Device complies with SMC-3 T10/1730-D revision 15");
                            break;
                        case 0x0484:
                            sb.AppendLine("Device complies with SMC-3 T10/1730-D revision 16");
                            break;
                        case 0x0486:
                            sb.AppendLine("Device complies with SMC-3 ANSI INCITS 484-2012");
                            break;
                        case 0x04A0:
                            sb.AppendLine("Device complies with ADC-2 (no version claimed)");
                            break;
                        case 0x04A7:
                            sb.AppendLine("Device complies with ADC-2 T10/1741-D revision 7");
                            break;
                        case 0x04AA:
                            sb.AppendLine("Device complies with ADC-2 T10/1741-D revision 8");
                            break;
                        case 0x04AC:
                            sb.AppendLine("Device complies with ADC-2 ANSI INCITS 441-2008");
                            break;
                        case 0x04C0:
                            sb.AppendLine("Device complies with SBC-3 (no version claimed)");
                            break;
                        case 0x04C3:
                            sb.AppendLine("Device complies with SBC-3 T10/BSR INCITS 514 revision 35");
                            break;
                        case 0x04C5:
                            sb.AppendLine("Device complies with SBC-3 T10/BSR INCITS 514 revision 36");
                            break;
                        case 0x04C8:
                            sb.AppendLine("Device complies with SBC-3 ANSI INCITS 514-2014");
                            break;
                        case 0x04E0:
                            sb.AppendLine("Device complies with MMC-6 (no version claimed)");
                            break;
                        case 0x04E3:
                            sb.AppendLine("Device complies with MMC-6 T10/1836-D revision 02b");
                            break;
                        case 0x04E5:
                            sb.AppendLine("Device complies with MMC-6 T10/1836-D revision 02g");
                            break;
                        case 0x04E6:
                            sb.AppendLine("Device complies with MMC-6 ANSI INCITS 468-2010");
                            break;
                        case 0x04E7:
                            sb.AppendLine("Device complies with MMC-6 ANSI INCITS 468-2010 + MMC-6/AM1 ANSI INCITS 468-2010/AM 1");
                            break;
                        case 0x0500:
                            sb.AppendLine("Device complies with ADC-3 (no version claimed)");
                            break;
                        case 0x0502:
                            sb.AppendLine("Device complies with ADC-3 T10/1895-D revision 04");
                            break;
                        case 0x0504:
                            sb.AppendLine("Device complies with ADC-3 T10/1895-D revision 05");
                            break;
                        case 0x0506:
                            sb.AppendLine("Device complies with ADC-3 T10/1895-D revision 05a");
                            break;
                        case 0x050A:
                            sb.AppendLine("Device complies with ADC-3 ANSI INCITS 497-2012");
                            break;
                        case 0x0520:
                            sb.AppendLine("Device complies with SSC-4 (no version claimed)");
                            break;
                        case 0x0523:
                            sb.AppendLine("Device complies with SSC-4 T10/BSR INCITS 516 revision 2");
                            break;
                        case 0x0525:
                            sb.AppendLine("Device complies with SSC-4 T10/BSR INCITS 516 revision 3");
                            break;
                        case 0x0527:
                            sb.AppendLine("Device complies with SSC-4 ANSI INCITS 516-2013");
                            break;
                        case 0x0560:
                            sb.AppendLine("Device complies with OSD-3 (no version claimed)");
                            break;
                        case 0x0580:
                            sb.AppendLine("Device complies with SES-3 (no version claimed)");
                            break;
                        case 0x05A0:
                            sb.AppendLine("Device complies with SSC-5 (no version claimed)");
                            break;
                        case 0x05C0:
                            sb.AppendLine("Device complies with SPC-5 (no version claimed)");
                            break;
                        case 0x05E0:
                            sb.AppendLine("Device complies with SFSC (no version claimed)");
                            break;
                        case 0x05E3:
                            sb.AppendLine("Device complies with SFSC BSR INCITS 501 revision 01");
                            break;
                        case 0x0600:
                            sb.AppendLine("Device complies with SBC-4 (no version claimed)");
                            break;
                        case 0x0620:
                            sb.AppendLine("Device complies with ZBC (no version claimed)");
                            break;
                        case 0x0622:
                            sb.AppendLine("Device complies with ZBC BSR INCITS 536 revision 02");
                            break;
                        case 0x0640:
                            sb.AppendLine("Device complies with ADC-4 (no version claimed)");
                            break;
                        case 0x0820:
                            sb.AppendLine("Device complies with SSA-TL2 (no version claimed)");
                            break;
                        case 0x083B:
                            sb.AppendLine("Device complies with SSA-TL2 T10.1/1147-D revision 05b");
                            break;
                        case 0x083C:
                            sb.AppendLine("Device complies with SSA-TL2 ANSI INCITS 308-1998");
                            break;
                        case 0x0840:
                            sb.AppendLine("Device complies with SSA-TL1 (no version claimed)");
                            break;
                        case 0x085B:
                            sb.AppendLine("Device complies with SSA-TL1 T10.1/0989-D revision 10b");
                            break;
                        case 0x085C:
                            sb.AppendLine("Device complies with SSA-TL1 ANSI INCITS 295-1996");
                            break;
                        case 0x0860:
                            sb.AppendLine("Device complies with SSA-S3P (no version claimed)");
                            break;
                        case 0x087B:
                            sb.AppendLine("Device complies with SSA-S3P T10.1/1051-D revision 05b");
                            break;
                        case 0x087C:
                            sb.AppendLine("Device complies with SSA-S3P ANSI INCITS 309-1998");
                            break;
                        case 0x0880:
                            sb.AppendLine("Device complies with SSA-S2P (no version claimed)");
                            break;
                        case 0x089B:
                            sb.AppendLine("Device complies with SSA-S2P T10.1/1121-D revision 07b");
                            break;
                        case 0x089C:
                            sb.AppendLine("Device complies with SSA-S2P ANSI INCITS 294-1996");
                            break;
                        case 0x08A0:
                            sb.AppendLine("Device complies with SIP (no version claimed)");
                            break;
                        case 0x08BB:
                            sb.AppendLine("Device complies with SIP T10/0856-D revision 10");
                            break;
                        case 0x08BC:
                            sb.AppendLine("Device complies with SIP ANSI INCITS 292-1997");
                            break;
                        case 0x08C0:
                            sb.AppendLine("Device complies with FCP (no version claimed)");
                            break;
                        case 0x08DB:
                            sb.AppendLine("Device complies with FCP T10/0993-D revision 12");
                            break;
                        case 0x08DC:
                            sb.AppendLine("Device complies with FCP ANSI INCITS 269-1996");
                            break;
                        case 0x08E0:
                            sb.AppendLine("Device complies with SBP-2 (no version claimed)");
                            break;
                        case 0x08FB:
                            sb.AppendLine("Device complies with SBP-2 T10/1155-D revision 04");
                            break;
                        case 0x08FC:
                            sb.AppendLine("Device complies with SBP-2 ANSI INCITS 325-1998");
                            break;
                        case 0x0900:
                            sb.AppendLine("Device complies with FCP-2 (no version claimed)");
                            break;
                        case 0x0901:
                            sb.AppendLine("Device complies with FCP-2 T10/1144-D revision 4");
                            break;
                        case 0x0915:
                            sb.AppendLine("Device complies with FCP-2 T10/1144-D revision 7");
                            break;
                        case 0x0916:
                            sb.AppendLine("Device complies with FCP-2 T10/1144-D revision 7a");
                            break;
                        case 0x0917:
                            sb.AppendLine("Device complies with FCP-2 ANSI INCITS 350-2003");
                            break;
                        case 0x0918:
                            sb.AppendLine("Device complies with FCP-2 T10/1144-D revision 8");
                            break;
                        case 0x0920:
                            sb.AppendLine("Device complies with SST (no version claimed)");
                            break;
                        case 0x0935:
                            sb.AppendLine("Device complies with SST T10/1380-D revision 8b");
                            break;
                        case 0x0940:
                            sb.AppendLine("Device complies with SRP (no version claimed)");
                            break;
                        case 0x0954:
                            sb.AppendLine("Device complies with SRP T10/1415-D revision 10");
                            break;
                        case 0x0955:
                            sb.AppendLine("Device complies with SRP T10/1415-D revision 16a");
                            break;
                        case 0x095C:
                            sb.AppendLine("Device complies with SRP ANSI INCITS 365-2002");
                            break;
                        case 0x0960:
                            sb.AppendLine("Device complies with iSCSI (no version claimed)");
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
                            sb.AppendFormat("Device complies with iSCSI revision {0}", VersionDescriptor & 0x1F).AppendLine();
                            break;
                        case 0x0980:
                            sb.AppendLine("Device complies with SBP-3 (no version claimed)");
                            break;
                        case 0x0982:
                            sb.AppendLine("Device complies with SBP-3 T10/1467-D revision 1f");
                            break;
                        case 0x0994:
                            sb.AppendLine("Device complies with SBP-3 T10/1467-D revision 3");
                            break;
                        case 0x099A:
                            sb.AppendLine("Device complies with SBP-3 T10/1467-D revision 4");
                            break;
                        case 0x099B:
                            sb.AppendLine("Device complies with SBP-3 T10/1467-D revision 5");
                            break;
                        case 0x099C:
                            sb.AppendLine("Device complies with SBP-3 ANSI INCITS 375-2004");
                            break;
                        case 0x09C0:
                            sb.AppendLine("Device complies with ADP (no version claimed)");
                            break;
                        case 0x09E0:
                            sb.AppendLine("Device complies with ADT (no version claimed)");
                            break;
                        case 0x09F9:
                            sb.AppendLine("Device complies with ADT T10/1557-D revision 11");
                            break;
                        case 0x09FA:
                            sb.AppendLine("Device complies with ADT T10/1557-D revision 14");
                            break;
                        case 0x09FD:
                            sb.AppendLine("Device complies with ADT ANSI INCITS 406-2005");
                            break;
                        case 0x0A00:
                            sb.AppendLine("Device complies with FCP-3 (no version claimed)");
                            break;
                        case 0x0A07:
                            sb.AppendLine("Device complies with FCP-3 T10/1560-D revision 3f");
                            break;
                        case 0x0A0F:
                            sb.AppendLine("Device complies with FCP-3 T10/1560-D revision 4");
                            break;
                        case 0x0A11:
                            sb.AppendLine("Device complies with FCP-3 ANSI INCITS 416-2006");
                            break;
                        case 0x0A1C:
                            sb.AppendLine("Device complies with FCP-3 ISO/IEC 14776-223");
                            break;
                        case 0x0A20:
                            sb.AppendLine("Device complies with ADT-2 (no version claimed)");
                            break;
                        case 0x0A22:
                            sb.AppendLine("Device complies with ADT-2 T10/1742-D revision 06");
                            break;
                        case 0x0A27:
                            sb.AppendLine("Device complies with ADT-2 T10/1742-D revision 08");
                            break;
                        case 0x0A28:
                            sb.AppendLine("Device complies with ADT-2 T10/1742-D revision 09");
                            break;
                        case 0x0A2B:
                            sb.AppendLine("Device complies with ADT-2 ANSI INCITS 472-2011");
                            break;
                        case 0x0A40:
                            sb.AppendLine("Device complies with FCP-4 (no version claimed)");
                            break;
                        case 0x0A42:
                            sb.AppendLine("Device complies with FCP-4 T10/1828-D revision 01");
                            break;
                        case 0x0A44:
                            sb.AppendLine("Device complies with FCP-4 T10/1828-D revision 02");
                            break;
                        case 0x0A45:
                            sb.AppendLine("Device complies with FCP-4 T10/1828-D revision 02b");
                            break;
                        case 0x0A46:
                            sb.AppendLine("Device complies with FCP-4 ANSI INCITS 481-2012");
                            break;
                        case 0x0A60:
                            sb.AppendLine("Device complies with ADT-3 (no version claimed)");
                            break;
                        case 0x0AA0:
                            sb.AppendLine("Device complies with SPI (no version claimed)");
                            break;
                        case 0x0AB9:
                            sb.AppendLine("Device complies with SPI T10/0855-D revision 15a");
                            break;
                        case 0x0ABA:
                            sb.AppendLine("Device complies with SPI ANSI INCITS 253-1995");
                            break;
                        case 0x0ABB:
                            sb.AppendLine("Device complies with SPI T10/0855-D revision 15a with SPI Amnd revision 3a");
                            break;
                        case 0x0ABC:
                            sb.AppendLine("Device complies with SPI ANSI INCITS 253-1995 with SPI Amnd ANSI INCITS 253/AM1-1998");
                            break;
                        case 0x0AC0:
                            sb.AppendLine("Device complies with Fast-20 (no version claimed)");
                            break;
                        case 0x0ADB:
                            sb.AppendLine("Device complies with Fast-20 T10/1071 revision 06");
                            break;
                        case 0x0ADC:
                            sb.AppendLine("Device complies with Fast-20 ANSI INCITS 277-1996");
                            break;
                        case 0x0AE0:
                            sb.AppendLine("Device complies with SPI-2 (no version claimed)");
                            break;
                        case 0x0AFB:
                            sb.AppendLine("Device complies with SPI-2 T10/1142-D revision 20b");
                            break;
                        case 0x0AFC:
                            sb.AppendLine("Device complies with SPI-2 ANSI INCITS 302-1999");
                            break;
                        case 0x0B00:
                            sb.AppendLine("Device complies with SPI-3 (no version claimed)");
                            break;
                        case 0x0B18:
                            sb.AppendLine("Device complies with SPI-3 T10/1302-D revision 10");
                            break;
                        case 0x0B19:
                            sb.AppendLine("Device complies with SPI-3 T10/1302-D revision 13a");
                            break;
                        case 0x0B1A:
                            sb.AppendLine("Device complies with SPI-3 T10/1302-D revision 14");
                            break;
                        case 0x0B1C:
                            sb.AppendLine("Device complies with SPI-3 ANSI INCITS 336-2000");
                            break;
                        case 0x0B20:
                            sb.AppendLine("Device complies with EPI (no version claimed)");
                            break;
                        case 0x0B3B:
                            sb.AppendLine("Device complies with EPI T10/1134 revision 16");
                            break;
                        case 0x0B3C:
                            sb.AppendLine("Device complies with EPI ANSI INCITS TR-23 1999");
                            break;
                        case 0x0B40:
                            sb.AppendLine("Device complies with SPI-4 (no version claimed)");
                            break;
                        case 0x0B54:
                            sb.AppendLine("Device complies with SPI-4 T10/1365-D revision 7");
                            break;
                        case 0x0B55:
                            sb.AppendLine("Device complies with SPI-4 T10/1365-D revision 9");
                            break;
                        case 0x0B56:
                            sb.AppendLine("Device complies with SPI-4 ANSI INCITS 362-2002");
                            break;
                        case 0x0B59:
                            sb.AppendLine("Device complies with SPI-4 T10/1365-D revision 10");
                            break;
                        case 0x0B60:
                            sb.AppendLine("Device complies with SPI-5 (no version claimed)");
                            break;
                        case 0x0B79:
                            sb.AppendLine("Device complies with SPI-5 T10/1525-D revision 3");
                            break;
                        case 0x0B7A:
                            sb.AppendLine("Device complies with SPI-5 T10/1525-D revision 5");
                            break;
                        case 0x0B7B:
                            sb.AppendLine("Device complies with SPI-5 T10/1525-D revision 6");
                            break;
                        case 0x0B7C:
                            sb.AppendLine("Device complies with SPI-5 ANSI INCITS 367-2003");
                            break;
                        case 0x0BE0:
                            sb.AppendLine("Device complies with SAS (no version claimed)");
                            break;
                        case 0x0BE1:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 01");
                            break;
                        case 0x0BF5:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 03");
                            break;
                        case 0x0BFA:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 04");
                            break;
                        case 0x0BFB:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 04");
                            break;
                        case 0x0BFC:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 05");
                            break;
                        case 0x0BFD:
                            sb.AppendLine("Device complies with SAS ANSI INCITS 376-2003");
                            break;
                        case 0x0C00:
                            sb.AppendLine("Device complies with SAS-1.1 (no version claimed)");
                            break;
                        case 0x0C07:
                            sb.AppendLine("Device complies with SAS-1.1 T10/1601-D revision 9");
                            break;
                        case 0x0C0F:
                            sb.AppendLine("Device complies with SAS-1.1 T10/1601-D revision 10");
                            break;
                        case 0x0C11:
                            sb.AppendLine("Device complies with SAS-1.1 ANSI INCITS 417-2006");
                            break;
                        case 0x0C12:
                            sb.AppendLine("Device complies with SAS-1.1 ISO/IEC 14776-151");
                            break;
                        case 0x0C20:
                            sb.AppendLine("Device complies with SAS-2 (no version claimed)");
                            break;
                        case 0x0C23:
                            sb.AppendLine("Device complies with SAS-2 T10/1760-D revision 14");
                            break;
                        case 0x0C27:
                            sb.AppendLine("Device complies with SAS-2 T10/1760-D revision 15");
                            break;
                        case 0x0C28:
                            sb.AppendLine("Device complies with SAS-2 T10/1760-D revision 16");
                            break;
                        case 0x0C2A:
                            sb.AppendLine("Device complies with SAS-2 ANSI INCITS 457-2010");
                            break;
                        case 0x0C40:
                            sb.AppendLine("Device complies with SAS-2.1 (no version claimed)");
                            break;
                        case 0x0C48:
                            sb.AppendLine("Device complies with SAS-2.1 T10/2125-D revision 04");
                            break;
                        case 0x0C4A:
                            sb.AppendLine("Device complies with SAS-2.1 T10/2125-D revision 06");
                            break;
                        case 0x0C4B:
                            sb.AppendLine("Device complies with SAS-2.1 T10/2125-D revision 07");
                            break;
                        case 0x0C4E:
                            sb.AppendLine("Device complies with SAS-2.1 ANSI INCITS 478-2011");
                            break;
                        case 0x0C4F:
                            sb.AppendLine("Device complies with SAS-2.1 ANSI INCITS 478-2011 w/ Amnd 1 ANSI INCITS 478/AM1-2014");
                            break;
                        case 0x0C52:
                            sb.AppendLine("Device complies with SAS-2.1 ISO/IEC 14776-153");
                            break;
                        case 0x0C60:
                            sb.AppendLine("Device complies with SAS-3 (no version claimed)");
                            break;
                        case 0x0C63:
                            sb.AppendLine("Device complies with SAS-3 T10/BSR INCITS 519 revision 05a");
                            break;
                        case 0x0C65:
                            sb.AppendLine("Device complies with SAS-3 T10/BSR INCITS 519 revision 06");
                            break;
                        case 0x0C68:
                            sb.AppendLine("Device complies with SAS-3 ANSI INCITS 519-2014");
                            break;
                        case 0x0C80:
                            sb.AppendLine("Device complies with SAS-4 (no version claimed)");
                            break;
                        case 0x0D20:
                            sb.AppendLine("Device complies with FC-PH (no version claimed)");
                            break;
                        case 0x0D3B:
                            sb.AppendLine("Device complies with FC-PH ANSI INCITS 230-1994");
                            break;
                        case 0x0D3C:
                            sb.AppendLine("Device complies with FC-PH ANSI INCITS 230-1994 with Amnd 1 ANSI INCITS 230/AM1-1996");
                            break;
                        case 0x0D40:
                            sb.AppendLine("Device complies with FC-AL (no version claimed)");
                            break;
                        case 0x0D5C:
                            sb.AppendLine("Device complies with FC-AL ANSI INCITS 272-1996");
                            break;
                        case 0x0D60:
                            sb.AppendLine("Device complies with FC-AL-2 (no version claimed)");
                            break;
                        case 0x0D61:
                            sb.AppendLine("Device complies with FC-AL-2 T11/1133-D revision 7.0");
                            break;
                        case 0x0D63:
                            sb.AppendLine("Device complies with FC-AL-2 ANSI INCITS 332-1999 with AM1-2003 & AM2-2006");
                            break;
                        case 0x0D64:
                            sb.AppendLine("Device complies with FC-AL-2 ANSI INCITS 332-1999 with Amnd 2 AM2-2006");
                            break;
                        case 0x0D65:
                            sb.AppendLine("Device complies with FC-AL-2 ISO/IEC 14165-122 with AM1 & AM2");
                            break;
                        case 0x0D7C:
                            sb.AppendLine("Device complies with FC-AL-2 ANSI INCITS 332-1999");
                            break;
                        case 0x0D7D:
                            sb.AppendLine("Device complies with FC-AL-2 ANSI INCITS 332-1999 with Amnd 1 AM1-2003");
                            break;
                        case 0x0D80:
                            sb.AppendLine("Device complies with FC-PH-3 (no version claimed)");
                            break;
                        case 0x0D9C:
                            sb.AppendLine("Device complies with FC-PH-3 ANSI INCITS 303-1998");
                            break;
                        case 0x0DA0:
                            sb.AppendLine("Device complies with FC-FS (no version claimed)");
                            break;
                        case 0x0DB7:
                            sb.AppendLine("Device complies with FC-FS T11/1331-D revision 1.2");
                            break;
                        case 0x0DB8:
                            sb.AppendLine("Device complies with FC-FS T11/1331-D revision 1.7");
                            break;
                        case 0x0DBC:
                            sb.AppendLine("Device complies with FC-FS ANSI INCITS 373-2003");
                            break;
                        case 0x0DBD:
                            sb.AppendLine("Device complies with FC-FS ISO/IEC 14165-251");
                            break;
                        case 0x0DC0:
                            sb.AppendLine("Device complies with FC-PI (no version claimed)");
                            break;
                        case 0x0DDC:
                            sb.AppendLine("Device complies with FC-PI ANSI INCITS 352-2002");
                            break;
                        case 0x0DE0:
                            sb.AppendLine("Device complies with FC-PI-2 (no version claimed)");
                            break;
                        case 0x0DE2:
                            sb.AppendLine("Device complies with FC-PI-2 T11/1506-D revision 5.0");
                            break;
                        case 0x0DE4:
                            sb.AppendLine("Device complies with FC-PI-2 ANSI INCITS 404-2006");
                            break;
                        case 0x0E00:
                            sb.AppendLine("Device complies with FC-FS-2 (no version claimed)");
                            break;
                        case 0x0E02:
                            sb.AppendLine("Device complies with FC-FS-2 ANSI INCITS 242-2007");
                            break;
                        case 0x0E03:
                            sb.AppendLine("Device complies with FC-FS-2 ANSI INCITS 242-2007 with AM1 ANSI INCITS 242/AM1-2007");
                            break;
                        case 0x0E20:
                            sb.AppendLine("Device complies with FC-LS (no version claimed)");
                            break;
                        case 0x0E21:
                            sb.AppendLine("Device complies with FC-LS T11/1620-D revision 1.62");
                            break;
                        case 0x0E29:
                            sb.AppendLine("Device complies with FC-LS ANSI INCITS 433-2007");
                            break;
                        case 0x0E40:
                            sb.AppendLine("Device complies with FC-SP (no version claimed)");
                            break;
                        case 0x0E42:
                            sb.AppendLine("Device complies with FC-SP T11/1570-D revision 1.6");
                            break;
                        case 0x0E45:
                            sb.AppendLine("Device complies with FC-SP ANSI INCITS 426-2007");
                            break;
                        case 0x0E60:
                            sb.AppendLine("Device complies with FC-PI-3 (no version claimed)");
                            break;
                        case 0x0E62:
                            sb.AppendLine("Device complies with FC-PI-3 T11/1625-D revision 2.0");
                            break;
                        case 0x0E68:
                            sb.AppendLine("Device complies with FC-PI-3 T11/1625-D revision 2.1");
                            break;
                        case 0x0E6A:
                            sb.AppendLine("Device complies with FC-PI-3 T11/1625-D revision 4.0");
                            break;
                        case 0x0E6E:
                            sb.AppendLine("Device complies with FC-PI-3 ANSI INCITS 460-2011");
                            break;
                        case 0x0E80:
                            sb.AppendLine("Device complies with FC-PI-4 (no version claimed)");
                            break;
                        case 0x0E82:
                            sb.AppendLine("Device complies with FC-PI-4 T11/1647-D revision 8.0");
                            break;
                        case 0x0E88:
                            sb.AppendLine("Device complies with FC-PI-4 ANSI INCITS 450-2009");
                            break;
                        case 0x0EA0:
                            sb.AppendLine("Device complies with FC 10GFC (no version claimed)");
                            break;
                        case 0x0EA2:
                            sb.AppendLine("Device complies with FC 10GFC ANSI INCITS 364-2003");
                            break;
                        case 0x0EA3:
                            sb.AppendLine("Device complies with FC 10GFC ISO/IEC 14165-116");
                            break;
                        case 0x0EA5:
                            sb.AppendLine("Device complies with FC 10GFC ISO/IEC 14165-116 with AM1");
                            break;
                        case 0x0EA6:
                            sb.AppendLine("Device complies with FC 10GFC ANSI INCITS 364-2003 with AM1 ANSI INCITS 364/AM1-2007");
                            break;
                        case 0x0EC0:
                            sb.AppendLine("Device complies with FC-SP-2 (no version claimed)");
                            break;
                        case 0x0EE0:
                            sb.AppendLine("Device complies with FC-FS-3 (no version claimed)");
                            break;
                        case 0x0EE2:
                            sb.AppendLine("Device complies with FC-FS-3 T11/1861-D revision 0.9");
                            break;
                        case 0x0EE7:
                            sb.AppendLine("Device complies with FC-FS-3 T11/1861-D revision 1.0");
                            break;
                        case 0x0EE9:
                            sb.AppendLine("Device complies with FC-FS-3 T11/1861-D revision 1.10");
                            break;
                        case 0x0EEB:
                            sb.AppendLine("Device complies with FC-FS-3 ANSI INCITS 470-2011");
                            break;
                        case 0x0F00:
                            sb.AppendLine("Device complies with FC-LS-2 (no version claimed)");
                            break;
                        case 0x0F03:
                            sb.AppendLine("Device complies with FC-LS-2 T11/2103-D revision 2.11");
                            break;
                        case 0x0F05:
                            sb.AppendLine("Device complies with FC-LS-2 T11/2103-D revision 2.21");
                            break;
                        case 0x0F07:
                            sb.AppendLine("Device complies with FC-LS-2 ANSI INCITS 477-2011");
                            break;
                        case 0x0F20:
                            sb.AppendLine("Device complies with FC-PI-5 (no version claimed)");
                            break;
                        case 0x0F27:
                            sb.AppendLine("Device complies with FC-PI-5 T11/2118-D revision 2.00");
                            break;
                        case 0x0F28:
                            sb.AppendLine("Device complies with FC-PI-5 T11/2118-D revision 3.00");
                            break;
                        case 0x0F2A:
                            sb.AppendLine("Device complies with FC-PI-5 T11/2118-D revision 6.00");
                            break;
                        case 0x0F2B:
                            sb.AppendLine("Device complies with FC-PI-5 T11/2118-D revision 6.10");
                            break;
                        case 0x0F2E:
                            sb.AppendLine("Device complies with FC-PI-5 ANSI INCITS 479-2011");
                            break;
                        case 0x0F40:
                            sb.AppendLine("Device complies with FC-PI-6 (no version claimed)");
                            break;
                        case 0x0F60:
                            sb.AppendLine("Device complies with FC-FS-4 (no version claimed)");
                            break;
                        case 0x0F80:
                            sb.AppendLine("Device complies with FC-LS-3 (no version claimed)");
                            break;
                        case 0x12A0:
                            sb.AppendLine("Device complies with FC-SCM (no version claimed)");
                            break;
                        case 0x12A3:
                            sb.AppendLine("Device complies with FC-SCM T11/1824DT revision 1.0");
                            break;
                        case 0x12A5:
                            sb.AppendLine("Device complies with FC-SCM T11/1824DT revision 1.1");
                            break;
                        case 0x12A7:
                            sb.AppendLine("Device complies with FC-SCM T11/1824DT revision 1.4");
                            break;
                        case 0x12AA:
                            sb.AppendLine("Device complies with FC-SCM INCITS TR-47 2012");
                            break;
                        case 0x12C0:
                            sb.AppendLine("Device complies with FC-DA-2 (no version claimed)");
                            break;
                        case 0x12C3:
                            sb.AppendLine("Device complies with FC-DA-2 T11/1870DT revision 1.04");
                            break;
                        case 0x12C5:
                            sb.AppendLine("Device complies with FC-DA-2 T11/1870DT revision 1.06");
                            break;
                        case 0x12C9:
                            sb.AppendLine("Device complies with FC-DA-2 INCITS TR-49 2012");
                            break;
                        case 0x12E0:
                            sb.AppendLine("Device complies with FC-DA (no version claimed)");
                            break;
                        case 0x12E2:
                            sb.AppendLine("Device complies with FC-DA T11/1513-DT revision 3.1");
                            break;
                        case 0x12E8:
                            sb.AppendLine("Device complies with FC-DA ANSI INCITS TR-36 2004");
                            break;
                        case 0x12E9:
                            sb.AppendLine("Device complies with FC-DA ISO/IEC 14165-341");
                            break;
                        case 0x1300:
                            sb.AppendLine("Device complies with FC-Tape (no version claimed)");
                            break;
                        case 0x1301:
                            sb.AppendLine("Device complies with FC-Tape T11/1315 revision 1.16");
                            break;
                        case 0x131B:
                            sb.AppendLine("Device complies with FC-Tape T11/1315 revision 1.17");
                            break;
                        case 0x131C:
                            sb.AppendLine("Device complies with FC-Tape ANSI INCITS TR-24 1999");
                            break;
                        case 0x1320:
                            sb.AppendLine("Device complies with FC-FLA (no version claimed)");
                            break;
                        case 0x133B:
                            sb.AppendLine("Device complies with FC-FLA T11/1235 revision 7");
                            break;
                        case 0x133C:
                            sb.AppendLine("Device complies with FC-FLA ANSI INCITS TR-20 1998");
                            break;
                        case 0x1340:
                            sb.AppendLine("Device complies with FC-PLDA (no version claimed)");
                            break;
                        case 0x135B:
                            sb.AppendLine("Device complies with FC-PLDA T11/1162 revision 2.1");
                            break;
                        case 0x135C:
                            sb.AppendLine("Device complies with FC-PLDA ANSI INCITS TR-19 1998");
                            break;
                        case 0x1360:
                            sb.AppendLine("Device complies with SSA-PH2 (no version claimed)");
                            break;
                        case 0x137B:
                            sb.AppendLine("Device complies with SSA-PH2 T10.1/1145-D revision 09c");
                            break;
                        case 0x137C:
                            sb.AppendLine("Device complies with SSA-PH2 ANSI INCITS 293-1996");
                            break;
                        case 0x1380:
                            sb.AppendLine("Device complies with SSA-PH3 (no version claimed)");
                            break;
                        case 0x139B:
                            sb.AppendLine("Device complies with SSA-PH3 T10.1/1146-D revision 05b");
                            break;
                        case 0x139C:
                            sb.AppendLine("Device complies with SSA-PH3 ANSI INCITS 307-1998");
                            break;
                        case 0x14A0:
                            sb.AppendLine("Device complies with IEEE 1394 (no version claimed)");
                            break;
                        case 0x14BD:
                            sb.AppendLine("Device complies with ANSI IEEE 1394-1995");
                            break;
                        case 0x14C0:
                            sb.AppendLine("Device complies with IEEE 1394a (no version claimed)");
                            break;
                        case 0x14E0:
                            sb.AppendLine("Device complies with IEEE 1394b (no version claimed)");
                            break;
                        case 0x15E0:
                            sb.AppendLine("Device complies with ATA/ATAPI-6 (no version claimed)");
                            break;
                        case 0x15FD:
                            sb.AppendLine("Device complies with ATA/ATAPI-6 ANSI INCITS 361-2002");
                            break;
                        case 0x1600:
                            sb.AppendLine("Device complies with ATA/ATAPI-7 (no version claimed)");
                            break;
                        case 0x1602:
                            sb.AppendLine("Device complies with ATA/ATAPI-7 T13/1532-D revision 3");
                            break;
                        case 0x161C:
                            sb.AppendLine("Device complies with ATA/ATAPI-7 ANSI INCITS 397-2005");
                            break;
                        case 0x161E:
                            sb.AppendLine("Device complies with ATA/ATAPI-7 ISO/IEC 24739");
                            break;
                        case 0x1620:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-AAM (no version claimed)");
                            break;
                        case 0x1621:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-APT Parallel Transport (no version claimed)");
                            break;
                        case 0x1622:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-AST Serial Transport (no version claimed)");
                            break;
                        case 0x1623:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-ACS ATA/ATAPI Command Set (no version claimed)");
                            break;
                        case 0x1628:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-AAM ANSI INCITS 451-2008");
                            break;
                        case 0x162A:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-ACS ANSI INCITS 452-2009 w/ Amendment 1");
                            break;
                        case 0x1728:
                            sb.AppendLine("Device complies with Universal Serial Bus Specification, Revision 1.1");
                            break;
                        case 0x1729:
                            sb.AppendLine("Device complies with Universal Serial Bus Specification, Revision 2.0");
                            break;
                        case 0x1730:
                            sb.AppendLine("Device complies with USB Mass Storage Class Bulk-Only Transport, Revision 1.0");
                            break;
                        case 0x1740:
                            sb.AppendLine("Device complies with UAS (no version claimed)");
                            break;
                        case 0x1743:
                            sb.AppendLine("Device complies with UAS T10/2095-D revision 02");
                            break;
                        case 0x1747:
                            sb.AppendLine("Device complies with UAS T10/2095-D revision 04");
                            break;
                        case 0x1748:
                            sb.AppendLine("Device complies with UAS ANSI INCITS 471-2010");
                            break;
                        case 0x1749:
                            sb.AppendLine("Device complies with UAS ISO/IEC 14776-251:2014");
                            break;
                        case 0x1761:
                            sb.AppendLine("Device complies with ACS-2 (no version claimed)");
                            break;
                        case 0x1762:
                            sb.AppendLine("Device complies with ACS-2 ANSI INCITS 482-2013");
                            break;
                        case 0x1765:
                            sb.AppendLine("Device complies with ACS-3 (no version claimed)");
                            break;
                        case 0x1780:
                            sb.AppendLine("Device complies with UAS-2 (no version claimed)");
                            break;
                        case 0x1EA0:
                            sb.AppendLine("Device complies with SAT (no version claimed)");
                            break;
                        case 0x1EA7:
                            sb.AppendLine("Device complies with SAT T10/1711-D revision 8");
                            break;
                        case 0x1EAB:
                            sb.AppendLine("Device complies with SAT T10/1711-D revision 9");
                            break;
                        case 0x1EAD:
                            sb.AppendLine("Device complies with SAT ANSI INCITS 431-2007");
                            break;
                        case 0x1EC0:
                            sb.AppendLine("Device complies with SAT-2 (no version claimed)");
                            break;
                        case 0x1EC4:
                            sb.AppendLine("Device complies with SAT-2 T10/1826-D revision 06");
                            break;
                        case 0x1EC8:
                            sb.AppendLine("Device complies with SAT-2 T10/1826-D revision 09");
                            break;
                        case 0x1ECA:
                            sb.AppendLine("Device complies with SAT-2 ANSI INCITS 465-2010");
                            break;
                        case 0x1EE0:
                            sb.AppendLine("Device complies with SAT-3 (no version claimed)");
                            break;
                        case 0x1EE2:
                            sb.AppendLine("Device complies with SAT-3 T10/BSR INCITS 517 revision 4");
                            break;
                        case 0x1EE4:
                            sb.AppendLine("Device complies with SAT-3 T10/BSR INCITS 517 revision 7");
                            break;
                        case 0x1EE8:
                            sb.AppendLine("Device complies with SAT-3 ANSI INCITS 517-2015");
                            break;
                        case 0x1F00:
                            sb.AppendLine("Device complies with SAT-4 (no version claimed)");
                            break;
                        case 0x20A0:
                            sb.AppendLine("Device complies with SPL (no version claimed)");
                            break;
                        case 0x20A3:
                            sb.AppendLine("Device complies with SPL T10/2124-D revision 6a");
                            break;
                        case 0x20A5:
                            sb.AppendLine("Device complies with SPL T10/2124-D revision 7");
                            break;
                        case 0x20A7:
                            sb.AppendLine("Device complies with SPL ANSI INCITS 476-2011");
                            break;
                        case 0x20A8:
                            sb.AppendLine("Device complies with SPL ANSI INCITS 476-2011 + SPL AM1 INCITS 476/AM1 2012");
                            break;
                        case 0x20AA:
                            sb.AppendLine("Device complies with SPL ISO/IEC 14776-261:2012");
                            break;
                        case 0x20C0:
                            sb.AppendLine("Device complies with SPL-2 (no version claimed)");
                            break;
                        case 0x20C2:
                            sb.AppendLine("Device complies with SPL-2 T10/BSR INCITS 505 revision 4");
                            break;
                        case 0x20C4:
                            sb.AppendLine("Device complies with SPL-2 T10/BSR INCITS 505 revision 5");
                            break;
                        case 0x20C8:
                            sb.AppendLine("Device complies with SPL-2 ANSI INCITS 505-2013");
                            break;
                        case 0x20E0:
                            sb.AppendLine("Device complies with SPL-3 (no version claimed)");
                            break;
                        case 0x20E4:
                            sb.AppendLine("Device complies with SPL-3 T10/BSR INCITS 492 revision 6");
                            break;
                        case 0x20E6:
                            sb.AppendLine("Device complies with SPL-3 T10/BSR INCITS 492 revision 7");
                            break;
                        case 0x20E8:
                            sb.AppendLine("Device complies with SPL-3 ANSI INCITS 492-2015");
                            break;
                        case 0x2100:
                            sb.AppendLine("Device complies with SPL-4 (no version claimed)");
                            break;
                        case 0x21E0:
                            sb.AppendLine("Device complies with SOP (no version claimed)");
                            break;
                        case 0x21E4:
                            sb.AppendLine("Device complies with SOP T10/BSR INCITS 489 revision 4");
                            break;
                        case 0x21E6:
                            sb.AppendLine("Device complies with SOP T10/BSR INCITS 489 revision 5");
                            break;
                        case 0x21E8:
                            sb.AppendLine("Device complies with SOP ANSI INCITS 489-2014");
                            break;
                        case 0x2200:
                            sb.AppendLine("Device complies with PQI (no version claimed)");
                            break;
                        case 0x2204:
                            sb.AppendLine("Device complies with PQI T10/BSR INCITS 490 revision 6");
                            break;
                        case 0x2206:
                            sb.AppendLine("Device complies with PQI T10/BSR INCITS 490 revision 7");
                            break;
                        case 0x2208:
                            sb.AppendLine("Device complies with PQI ANSI INCITS 490-2014");
                            break;
                        case 0x2220:
                            sb.AppendLine("Device complies with SOP-2 (no version claimed)");
                            break;
                        case 0x2240:
                            sb.AppendLine("Device complies with PQI-2 (no version claimed)");
                            break;
                        case 0xFFC0:
                            sb.AppendLine("Device complies with IEEE 1667 (no version claimed)");
                            break;
                        case 0xFFC1:
                            sb.AppendLine("Device complies with IEEE 1667-2006");
                            break;
                        case 0xFFC2:
                            sb.AppendLine("Device complies with IEEE 1667-2009");
                            break;
                        default:
                            sb.AppendFormat("Device complies with unknown standard code 0x{0:X4}", VersionDescriptor).AppendLine();
                            break;
                    }
                }
            }

            #if DEBUG
            if(response.DeviceTypeModifier != 0)
                sb.AppendFormat("Vendor's device type modifier = 0x{0:X2}", response.DeviceTypeModifier).AppendLine();
            if(response.Reserved2 != 0)
                sb.AppendFormat("Reserved byte 5, bits 2 to 1 = 0x{0:X2}", response.Reserved2).AppendLine();
            if(response.Reserved3 != 0)
                sb.AppendFormat("Reserved byte 56, bits 7 to 4 = 0x{0:X2}", response.Reserved3).AppendLine();
            if(response.Reserved4 != 0)
                sb.AppendFormat("Reserved byte 57 = 0x{0:X2}", response.Reserved4).AppendLine();

            if (response.Reserved5 != null)
            {
                sb.AppendLine("Reserved bytes 74 to 95");
                sb.AppendLine("============================================================");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.Reserved5, 60));
                sb.AppendLine("============================================================");
            }

            if (response.VendorSpecific != null)
            {
                sb.AppendLine("Vendor-specific bytes 36 to 55");
                sb.AppendLine("============================================================");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VendorSpecific, 60));
                sb.AppendLine("============================================================");
            }

            if (response.VendorSpecific2 != null)
            {
                sb.AppendFormat("Vendor-specific bytes 96 to {0}", response.AdditionalLength+4).AppendLine();
                sb.AppendLine("============================================================");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VendorSpecific2, 60));
                sb.AppendLine("============================================================");
            }
            #endif

            return sb.ToString();
        }

        public static string Prettify(byte[] SCSIInquiryResponse)
        {
            SCSIInquiry? decoded = Decode(SCSIInquiryResponse);
            return Prettify(decoded);
        }

        #endregion Public methods

        #region Public structures

        // SCSI INQUIRY command response
        public struct SCSIInquiry
        {
            /// <summary>
            /// Peripheral qualifier
            /// Byte 0, bits 7 to 5
            /// </summary>
            public byte PeripheralQualifier;
            /// <summary>
            /// Peripheral device type
            /// Byte 0, bits 4 to 0
            /// </summary>
            public byte PeripheralDeviceType;
            /// <summary>
            /// Removable device
            /// Byte 1, bit 7
            /// </summary>
            public bool RMB;
            /// <summary>
            /// SCSI-1 vendor-specific qualification codes
            /// Byte 1, bits 6 to 0
            /// </summary>
            public byte DeviceTypeModifier;
            /// <summary>
            /// ISO/IEC SCSI Standard Version
            /// Byte 2, bits 7 to 6, mask = 0xC0, >> 6
            /// </summary>
            public byte ISOVersion;
            /// <summary>
            /// ECMA SCSI Standard Version
            /// Byte 2, bits 5 to 3, mask = 0x38, >> 3
            /// </summary>
            public byte ECMAVersion;
            /// <summary>
            /// ANSI SCSI Standard Version
            /// Byte 2, bits 2 to 0, mask = 0x07
            /// </summary>
            public byte ANSIVersion;
            /// <summary>
            /// Asynchronous Event Reporting Capability supported
            /// Byte 3, bit 7
            /// </summary>
            public bool AERC;
            /// <summary>
            /// Device supports TERMINATE TASK command
            /// Byte 3, bit 6
            /// </summary>
            public bool TrmTsk;
            /// <summary>
            /// Supports setting Normal ACA
            /// Byte 3, bit 5
            /// </summary>
            public bool NormACA;
            /// <summary>
            /// Supports LUN hierarchical addressing
            /// Byte 3, bit 4
            /// </summary>
            public bool HiSup;
            /// <summary>
            /// Responde data format
            /// Byte 3, bit 3 to 0
            /// </summary>
            public byte ResponseDataFormat;
            /// <summary>
            /// Lenght of total INQUIRY response minus 4
            /// Byte 4
            /// </summary>
            public byte AdditionalLength;
            /// <summary>
            /// Device contains an embedded storage array controller
            /// Byte 5, bit 7
            /// </summary>
            public bool SCCS;
            /// <summary>
            /// Device contains an Access Control Coordinator
            /// Byte 5, bit 6
            /// </summary>
            public bool ACC;
            /// <summary>
            /// Supports asymetrical logical unit access
            /// Byte 5, bits 5 to 4
            /// </summary>
            public byte TPGS;
            /// <summary>
            /// Supports third-party copy commands
            /// Byte 5, bit 3
            /// </summary>
            public bool ThreePC;
            /// <summary>
            /// Reserved
            /// Byte 5, bits 2 to 1
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Supports protection information
            /// Byte 5, bit 0
            /// </summary>
            public bool Protect;
            /// <summary>
            /// Supports basic queueing
            /// Byte 6, bit 7
            /// </summary>
            public bool BQue;
            /// <summary>
            /// Device contains an embedded enclosure services component
            /// Byte 6, bit 6
            /// </summary>
            public bool EncServ;
            /// <summary>
            /// Vendor-specific
            /// Byte 6, bit 5
            /// </summary>
            public bool VS1;
            /// <summary>
            /// Multi-port device
            /// Byte 6, bit 4
            /// </summary>
            public bool MultiP;
            /// <summary>
            /// Device contains or is attached to a medium changer
            /// Byte 6, bit 3
            /// </summary>
            public bool MChngr;
            /// <summary>
            /// Device supports request and acknowledge handshakes
            /// Byte 6, bit 2
            /// </summary>
            public bool ACKREQQ;
            /// <summary>
            /// Supports 32-bit wide SCSI addresses
            /// Byte 6, bit 1
            /// </summary>
            public bool Addr32;
            /// <summary>
            /// Supports 16-bit wide SCSI addresses
            /// Byte 6, bit 0
            /// </summary>
            public bool Addr16;
            /// <summary>
            /// Device supports relative addressing
            /// Byte 7, bit 7
            /// </summary>
            public bool RelAddr;
            /// <summary>
            /// Supports 32-bit wide data transfers
            /// Byte 7, bit 6
            /// </summary>
            public bool WBus32;
            /// <summary>
            /// Supports 16-bit wide data transfers
            /// Byte 7, bit 5
            /// </summary>
            public bool WBus16;
            /// <summary>
            /// Supports synchronous data transfer
            /// Byte 7, bit 4
            /// </summary>
            public bool Sync;
            /// <summary>
            /// Supports linked commands
            /// Byte 7, bit 3
            /// </summary>
            public bool Linked;
            /// <summary>
            /// Supports CONTINUE TASK and TARGET TRANSFER DISABLE commands
            /// Byte 7, bit 2
            /// </summary>
            public bool TranDis;
            /// <summary>
            /// Supports TCQ queue
            /// Byte 7, bit 1
            /// </summary>
            public bool CmdQue;
            /// <summary>
            /// Indicates that the devices responds to RESET with soft reset
            /// Byte 7, bit 0
            /// </summary>
            public bool SftRe;
            /// <summary>
            /// Vendor identification
            /// Bytes 8 to 15
            /// </summary>
            public byte[] VendorIdentification;
            /// <summary>
            /// Product identification
            /// Bytes 16 to 31
            /// </summary>
            public byte[] ProductIdentification;
            /// <summary>
            /// Product revision level
            /// Bytes 32 to 35
            /// </summary>
            public byte[] ProductRevisionLevel;
            /// <summary>
            /// Vendor-specific data
            /// Bytes 36 to 55
            /// </summary>
            public byte[] VendorSpecific;
            /// <summary>
            /// Byte 56, bits 7 to 4
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Supported SPI clocking
            /// Byte 56, bits 3 to 2
            /// </summary>
            public byte Clocking;
            /// <summary>
            /// Device supports Quick Arbitration and Selection
            /// Byte 56, bit 1
            /// </summary>
            public bool QAS;
            /// <summary>
            /// Supports information unit transfers
            /// Byte 56, bit 0
            /// </summary>
            public bool IUS;
            /// <summary>
            /// Reserved
            /// Byte 57
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Array of version descriptors
            /// Bytes 58 to 73
            /// </summary>
            public UInt16[] VersionDescriptors;
            /// <summary>
            /// Reserved
            /// Bytes 74 to 95
            /// </summary>
            public byte[] Reserved5;
            /// <summary>
            /// Reserved
            /// Bytes 96 to end
            /// </summary>
            public byte[] VendorSpecific2;
        }

        #endregion Public structures
    }
}

