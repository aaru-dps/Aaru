// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ScsiInquiry.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI INQUIRY from reports.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Metadata;

namespace DiscImageChef.Server.App_Start
{
    static class ScsiInquiry
    {
        internal static List<string> Report(scsiInquiryType inquiry)
        {
            List<string> scsiOneValue = new List<string>();

            switch((PeripheralQualifiers)inquiry.PeripheralQualifier)
            {
                case PeripheralQualifiers.Supported:
                    scsiOneValue.Add("Device is connected and supported.");
                    break;
                case PeripheralQualifiers.Unconnected:
                    scsiOneValue.Add("Device is supported but not connected.");
                    break;
                case PeripheralQualifiers.Reserved:
                    scsiOneValue.Add("Reserved value set in Peripheral Qualifier field.");
                    break;
                case PeripheralQualifiers.Unsupported:
                    scsiOneValue.Add("Device is connected but unsupported.");
                    break;
                default:
                    scsiOneValue.Add(string.Format("Vendor value {0} set in Peripheral Qualifier field.",
                                                   inquiry.PeripheralQualifier));
                    break;
            }

            switch((PeripheralDeviceTypes)inquiry.PeripheralDeviceType)
            {
                case PeripheralDeviceTypes.DirectAccess: //0x00,
                    scsiOneValue.Add("Direct-access device");
                    break;
                case PeripheralDeviceTypes.SequentialAccess: //0x01,
                    scsiOneValue.Add("Sequential-access device");
                    break;
                case PeripheralDeviceTypes.PrinterDevice: //0x02,
                    scsiOneValue.Add("Printer device");
                    break;
                case PeripheralDeviceTypes.ProcessorDevice: //0x03,
                    scsiOneValue.Add("Processor device");
                    break;
                case PeripheralDeviceTypes.WriteOnceDevice: //0x04,
                    scsiOneValue.Add("Write-once device");
                    break;
                case PeripheralDeviceTypes.MultiMediaDevice: //0x05,
                    scsiOneValue.Add("CD-ROM/DVD/etc device");
                    break;
                case PeripheralDeviceTypes.ScannerDevice: //0x06,
                    scsiOneValue.Add("Scanner device");
                    break;
                case PeripheralDeviceTypes.OpticalDevice: //0x07,
                    scsiOneValue.Add("Optical memory device");
                    break;
                case PeripheralDeviceTypes.MediumChangerDevice: //0x08,
                    scsiOneValue.Add("Medium change device");
                    break;
                case PeripheralDeviceTypes.CommsDevice: //0x09,
                    scsiOneValue.Add("Communications device");
                    break;
                case PeripheralDeviceTypes.PrePressDevice1: //0x0A,
                    scsiOneValue.Add("Graphics arts pre-press device (defined in ASC IT8)");
                    break;
                case PeripheralDeviceTypes.PrePressDevice2: //0x0B,
                    scsiOneValue.Add("Graphics arts pre-press device (defined in ASC IT8)");
                    break;
                case PeripheralDeviceTypes.ArrayControllerDevice: //0x0C,
                    scsiOneValue.Add("Array controller device");
                    break;
                case PeripheralDeviceTypes.EnclosureServiceDevice: //0x0D,
                    scsiOneValue.Add("Enclosure services device");
                    break;
                case PeripheralDeviceTypes.SimplifiedDevice: //0x0E,
                    scsiOneValue.Add("Simplified direct-access device");
                    break;
                case PeripheralDeviceTypes.OCRWDevice: //0x0F,
                    scsiOneValue.Add("Optical card reader/writer device");
                    break;
                case PeripheralDeviceTypes.BridgingExpander: //0x10,
                    scsiOneValue.Add("Bridging Expanders");
                    break;
                case PeripheralDeviceTypes.ObjectDevice: //0x11,
                    scsiOneValue.Add("Object-based Storage Device");
                    break;
                case PeripheralDeviceTypes.ADCDevice: //0x12,
                    scsiOneValue.Add("Automation/Drive Interface");
                    break;
                case PeripheralDeviceTypes.SCSISecurityManagerDevice: //0x13,
                    scsiOneValue.Add("Security Manager Device");
                    break;
                case PeripheralDeviceTypes.SCSIZonedBlockDevice: //0x14
                    scsiOneValue.Add("Host managed zoned block device");
                    break;
                case PeripheralDeviceTypes.WellKnownDevice: //0x1E,
                    scsiOneValue.Add("Well known logical unit");
                    break;
                case PeripheralDeviceTypes.UnknownDevice: //0x1F
                    scsiOneValue.Add("Unknown or no device type");
                    break;
                default:
                    scsiOneValue.Add(string.Format("Unknown device type field value 0x{0:X2}",
                                                   inquiry.PeripheralDeviceType));
                    break;
            }

            switch((ANSIVersions)inquiry.ANSIVersion)
            {
                case ANSIVersions.ANSINoVersion:
                    scsiOneValue.Add("Device does not claim to comply with any SCSI ANSI standard");
                    break;
                case ANSIVersions.ANSI1986Version:
                    scsiOneValue.Add("Device claims to comply with ANSI X3.131:1986 (SCSI-1)");
                    break;
                case ANSIVersions.ANSI1994Version:
                    scsiOneValue.Add("Device claims to comply with ANSI X3.131:1994 (SCSI-2)");
                    break;
                case ANSIVersions.ANSI1997Version:
                    scsiOneValue.Add("Device claims to comply with ANSI X3.301:1997 (SPC-1)");
                    break;
                case ANSIVersions.ANSI2001Version:
                    scsiOneValue.Add("Device claims to comply with ANSI X3.351:2001 (SPC-2)");
                    break;
                case ANSIVersions.ANSI2005Version:
                    scsiOneValue.Add("Device claims to comply with ANSI X3.408:2005 (SPC-3)");
                    break;
                case ANSIVersions.ANSI2008Version:
                    scsiOneValue.Add("Device claims to comply with ANSI X3.408:2005 (SPC-4)");
                    break;
                default:
                    scsiOneValue
                        .Add(string.Format("Device claims to comply with unknown SCSI ANSI standard value 0x{0:X2})",
                                           inquiry.ANSIVersion));
                    break;
            }

            switch((ECMAVersions)inquiry.ECMAVersion)
            {
                case ECMAVersions.ECMANoVersion:
                    scsiOneValue.Add("Device does not claim to comply with any SCSI ECMA standard");
                    break;
                case ECMAVersions.ECMA111:
                    scsiOneValue.Add("Device claims to comply ECMA-111: Small Computer System Interface SCSI");
                    break;
                default:
                    scsiOneValue
                        .Add(string.Format("Device claims to comply with unknown SCSI ECMA standard value 0x{0:X2})",
                                           inquiry.ECMAVersion));
                    break;
            }

            switch((ISOVersions)inquiry.ISOVersion)
            {
                case ISOVersions.ISONoVersion:
                    scsiOneValue.Add("Device does not claim to comply with any SCSI ISO/IEC standard");
                    break;
                case ISOVersions.ISO1995Version:
                    scsiOneValue.Add("Device claims to comply with ISO/IEC 9316:1995");
                    break;
                default:
                    scsiOneValue
                        .Add(string.Format("Device claims to comply with unknown SCSI ISO/IEC standard value 0x{0:X2})",
                                           inquiry.ISOVersion));
                    break;
            }

            if(inquiry.Removable) scsiOneValue.Add("Device is removable");
            if(inquiry.AERCSupported) scsiOneValue.Add("Device supports Asynchronous Event Reporting Capability");
            if(inquiry.TerminateTaskSupported) scsiOneValue.Add("Device supports TERMINATE TASK command");
            if(inquiry.NormalACA) scsiOneValue.Add("Device supports setting Normal ACA");
            if(inquiry.HierarchicalLUN) scsiOneValue.Add("Device supports LUN hierarchical addressing");
            if(inquiry.StorageArrayController) scsiOneValue.Add("Device contains an embedded storage array controller");
            if(inquiry.AccessControlCoordinator) scsiOneValue.Add("Device contains an Access Control Coordinator");
            if(inquiry.ThirdPartyCopy) scsiOneValue.Add("Device supports third-party copy commands");
            if(inquiry.Protection) scsiOneValue.Add("Device supports protection information");
            if(inquiry.BasicQueueing) scsiOneValue.Add("Device supports basic queueing");
            if(inquiry.EnclosureServices) scsiOneValue.Add("Device contains an embedded enclosure services component");
            if(inquiry.MultiPortDevice) scsiOneValue.Add("Multi-port device");
            if(inquiry.MediumChanger) scsiOneValue.Add("Device contains or is attached to a medium changer");
            if(inquiry.ACKRequests) scsiOneValue.Add("Device supports request and acknowledge handshakes");
            if(inquiry.Address32) scsiOneValue.Add("Device supports 32-bit wide SCSI addresses");
            if(inquiry.Address16) scsiOneValue.Add("Device supports 16-bit wide SCSI addresses");
            if(inquiry.RelativeAddressing) scsiOneValue.Add("Device supports relative addressing");
            if(inquiry.WideBus32) scsiOneValue.Add("Device supports 32-bit wide data transfers");
            if(inquiry.WideBus16) scsiOneValue.Add("Device supports 16-bit wide data transfers");
            if(inquiry.SyncTransfer) scsiOneValue.Add("Device supports synchronous data transfer");
            if(inquiry.LinkedCommands) scsiOneValue.Add("Device supports linked commands");
            if(inquiry.TranferDisable)
                scsiOneValue.Add("Device supports CONTINUE TASK and TARGET TRANSFER DISABLE commands");
            if(inquiry.QAS) scsiOneValue.Add("Device supports Quick Arbitration and Selection");
            if(inquiry.TaggedCommandQueue) scsiOneValue.Add("Device supports TCQ queue");
            if(inquiry.IUS) scsiOneValue.Add("Device supports information unit transfers");
            if(inquiry.SoftReset) scsiOneValue.Add("Device implements RESET as a soft reset");

            switch((TGPSValues)inquiry.AsymmetricalLUNAccess)
            {
                case TGPSValues.NotSupported:
                    scsiOneValue.Add("Device does not support assymetrical access");
                    break;
                case TGPSValues.OnlyImplicit:
                    scsiOneValue.Add("Device only supports implicit assymetrical access");
                    break;
                case TGPSValues.OnlyExplicit:
                    scsiOneValue.Add("Device only supports explicit assymetrical access");
                    break;
                case TGPSValues.Both:
                    scsiOneValue.Add("Device supports implicit and explicit assymetrical access");
                    break;
                default:
                    scsiOneValue.Add(string.Format("Unknown value in TPGS field 0x{0:X2}",
                                                   inquiry.AsymmetricalLUNAccess));
                    break;
            }

            switch((SPIClocking)inquiry.SPIClocking)
            {
                case SPIClocking.ST:
                    scsiOneValue.Add("Device supports only ST clocking");
                    break;
                case SPIClocking.DT:
                    scsiOneValue.Add("Device supports only DT clocking");
                    break;
                case SPIClocking.Reserved:
                    scsiOneValue.Add("Reserved value 0x02 found in SPI clocking field");
                    break;
                case SPIClocking.STandDT:
                    scsiOneValue.Add("Device supports ST and DT clocking");
                    break;
                default:
                    scsiOneValue.Add(string.Format("Unknown value in SPI clocking field 0x{0:X2}",
                                                   inquiry.SPIClocking));
                    break;
            }

            if(inquiry.VersionDescriptors != null)
            {
                foreach(ushort VersionDescriptor in inquiry.VersionDescriptors)
                {
                    switch(VersionDescriptor)
                    {
                        case 0xFFFF:
                        case 0x0000: break;
                        case 0x0020:
                            scsiOneValue.Add("Device complies with SAM (no version claimed)");
                            break;
                        case 0x003B:
                            scsiOneValue.Add("Device complies with SAM T10/0994-D revision 18");
                            break;
                        case 0x003C:
                            scsiOneValue.Add("Device complies with SAM ANSI INCITS 270-1996");
                            break;
                        case 0x0040:
                            scsiOneValue.Add("Device complies with SAM-2 (no version claimed)");
                            break;
                        case 0x0054:
                            scsiOneValue.Add("Device complies with SAM-2 T10/1157-D revision 23");
                            break;
                        case 0x0055:
                            scsiOneValue.Add("Device complies with SAM-2 T10/1157-D revision 24");
                            break;
                        case 0x005C:
                            scsiOneValue.Add("Device complies with SAM-2 ANSI INCITS 366-2003");
                            break;
                        case 0x005E:
                            scsiOneValue.Add("Device complies with SAM-2 ISO/IEC 14776-412");
                            break;
                        case 0x0060:
                            scsiOneValue.Add("Device complies with SAM-3 (no version claimed)");
                            break;
                        case 0x0062:
                            scsiOneValue.Add("Device complies with SAM-3 T10/1561-D revision 7");
                            break;
                        case 0x0075:
                            scsiOneValue.Add("Device complies with SAM-3 T10/1561-D revision 13");
                            break;
                        case 0x0076:
                            scsiOneValue.Add("Device complies with SAM-3 T10/1561-D revision 14");
                            break;
                        case 0x0077:
                            scsiOneValue.Add("Device complies with SAM-3 ANSI INCITS 402-2005");
                            break;
                        case 0x0080:
                            scsiOneValue.Add("Device complies with SAM-4 (no version claimed)");
                            break;
                        case 0x0087:
                            scsiOneValue.Add("Device complies with SAM-4 T10/1683-D revision 13");
                            break;
                        case 0x008B:
                            scsiOneValue.Add("Device complies with SAM-4 T10/1683-D revision 14");
                            break;
                        case 0x0090:
                            scsiOneValue.Add("Device complies with SAM-4 ANSI INCITS 447-2008");
                            break;
                        case 0x0092:
                            scsiOneValue.Add("Device complies with SAM-4 ISO/IEC 14776-414");
                            break;
                        case 0x00A0:
                            scsiOneValue.Add("Device complies with SAM-5 (no version claimed)");
                            break;
                        case 0x00A2:
                            scsiOneValue.Add("Device complies with SAM-5 T10/2104-D revision 4");
                            break;
                        case 0x00A4:
                            scsiOneValue.Add("Device complies with SAM-5 T10/2104-D revision 20");
                            break;
                        case 0x00A6:
                            scsiOneValue.Add("Device complies with SAM-5 T10/2104-D revision 21");
                            break;
                        case 0x00C0:
                            scsiOneValue.Add("Device complies with SAM-6 (no version claimed)");
                            break;
                        case 0x0120:
                            scsiOneValue.Add("Device complies with SPC (no version claimed)");
                            break;
                        case 0x013B:
                            scsiOneValue.Add("Device complies with SPC T10/0995-D revision 11a");
                            break;
                        case 0x013C:
                            scsiOneValue.Add("Device complies with SPC ANSI INCITS 301-1997");
                            break;
                        case 0x0140:
                            scsiOneValue.Add("Device complies with MMC (no version claimed)");
                            break;
                        case 0x015B:
                            scsiOneValue.Add("Device complies with MMC T10/1048-D revision 10a");
                            break;
                        case 0x015C:
                            scsiOneValue.Add("Device complies with MMC ANSI INCITS 304-1997");
                            break;
                        case 0x0160:
                            scsiOneValue.Add("Device complies with SCC (no version claimed)");
                            break;
                        case 0x017B:
                            scsiOneValue.Add("Device complies with SCC T10/1047-D revision 06c");
                            break;
                        case 0x017C:
                            scsiOneValue.Add("Device complies with SCC ANSI INCITS 276-1997");
                            break;
                        case 0x0180:
                            scsiOneValue.Add("Device complies with SBC (no version claimed)");
                            break;
                        case 0x019B:
                            scsiOneValue.Add("Device complies with SBC T10/0996-D revision 08c");
                            break;
                        case 0x019C:
                            scsiOneValue.Add("Device complies with SBC ANSI INCITS 306-1998");
                            break;
                        case 0x01A0:
                            scsiOneValue.Add("Device complies with SMC (no version claimed)");
                            break;
                        case 0x01BB:
                            scsiOneValue.Add("Device complies with SMC T10/0999-D revision 10a");
                            break;
                        case 0x01BC:
                            scsiOneValue.Add("Device complies with SMC ANSI INCITS 314-1998");
                            break;
                        case 0x01BE:
                            scsiOneValue.Add("Device complies with SMC ISO/IEC 14776-351");
                            break;
                        case 0x01C0:
                            scsiOneValue.Add("Device complies with SES (no version claimed)");
                            break;
                        case 0x01DB:
                            scsiOneValue.Add("Device complies with SES T10/1212-D revision 08b");
                            break;
                        case 0x01DC:
                            scsiOneValue.Add("Device complies with SES ANSI INCITS 305-1998");
                            break;
                        case 0x01DD:
                            scsiOneValue
                                .Add("Device complies with SES T10/1212 revision 08b w/ Amendment ANSI INCITS.305/AM1-2000");
                            break;
                        case 0x01DE:
                            scsiOneValue
                                .Add("Device complies with SES ANSI INCITS 305-1998 w/ Amendment ANSI INCITS.305/AM1-2000");
                            break;
                        case 0x01E0:
                            scsiOneValue.Add("Device complies with SCC-2 (no version claimed)");
                            break;
                        case 0x01FB:
                            scsiOneValue.Add("Device complies with SCC-2 T10/1125-D revision 04");
                            break;
                        case 0x01FC:
                            scsiOneValue.Add("Device complies with SCC-2 ANSI INCITS 318-1998");
                            break;
                        case 0x0200:
                            scsiOneValue.Add("Device complies with SSC (no version claimed)");
                            break;
                        case 0x0201:
                            scsiOneValue.Add("Device complies with SSC T10/0997-D revision 17");
                            break;
                        case 0x0207:
                            scsiOneValue.Add("Device complies with SSC T10/0997-D revision 22");
                            break;
                        case 0x021C:
                            scsiOneValue.Add("Device complies with SSC ANSI INCITS 335-2000");
                            break;
                        case 0x0220:
                            scsiOneValue.Add("Device complies with RBC (no version claimed)");
                            break;
                        case 0x0238:
                            scsiOneValue.Add("Device complies with RBC T10/1240-D revision 10a");
                            break;
                        case 0x023C:
                            scsiOneValue.Add("Device complies with RBC ANSI INCITS 330-2000");
                            break;
                        case 0x0240:
                            scsiOneValue.Add("Device complies with MMC-2 (no version claimed)");
                            break;
                        case 0x0255:
                            scsiOneValue.Add("Device complies with MMC-2 T10/1228-D revision 11");
                            break;
                        case 0x025B:
                            scsiOneValue.Add("Device complies with MMC-2 T10/1228-D revision 11a");
                            break;
                        case 0x025C:
                            scsiOneValue.Add("Device complies with MMC-2 ANSI INCITS 333-2000");
                            break;
                        case 0x0260:
                            scsiOneValue.Add("Device complies with SPC-2 (no version claimed)");
                            break;
                        case 0x0267:
                            scsiOneValue.Add("Device complies with SPC-2 T10/1236-D revision 12");
                            break;
                        case 0x0269:
                            scsiOneValue.Add("Device complies with SPC-2 T10/1236-D revision 18");
                            break;
                        case 0x0275:
                            scsiOneValue.Add("Device complies with SPC-2 T10/1236-D revision 19");
                            break;
                        case 0x0276:
                            scsiOneValue.Add("Device complies with SPC-2 T10/1236-D revision 20");
                            break;
                        case 0x0277:
                            scsiOneValue.Add("Device complies with SPC-2 ANSI INCITS 351-2001");
                            break;
                        case 0x0278:
                            scsiOneValue.Add("Device complies with SPC-2 ISO/IEC 14776-452");
                            break;
                        case 0x0280:
                            scsiOneValue.Add("Device complies with OCRW (no version claimed)");
                            break;
                        case 0x029E:
                            scsiOneValue.Add("Device complies with OCRW ISO/IEC 14776-381");
                            break;
                        case 0x02A0:
                            scsiOneValue.Add("Device complies with MMC-3 (no version claimed)");
                            break;
                        case 0x02B5:
                            scsiOneValue.Add("Device complies with MMC-3 T10/1363-D revision 9");
                            break;
                        case 0x02B6:
                            scsiOneValue.Add("Device complies with MMC-3 T10/1363-D revision 10g");
                            break;
                        case 0x02B8:
                            scsiOneValue.Add("Device complies with MMC-3 ANSI INCITS 360-2002");
                            break;
                        case 0x02E0:
                            scsiOneValue.Add("Device complies with SMC-2 (no version claimed)");
                            break;
                        case 0x02F5:
                            scsiOneValue.Add("Device complies with SMC-2 T10/1383-D revision 5");
                            break;
                        case 0x02FC:
                            scsiOneValue.Add("Device complies with SMC-2 T10/1383-D revision 6");
                            break;
                        case 0x02FD:
                            scsiOneValue.Add("Device complies with SMC-2 T10/1383-D revision 7");
                            break;
                        case 0x02FE:
                            scsiOneValue.Add("Device complies with SMC-2 ANSI INCITS 382-2004");
                            break;
                        case 0x0300:
                            scsiOneValue.Add("Device complies with SPC-3 (no version claimed)");
                            break;
                        case 0x0301:
                            scsiOneValue.Add("Device complies with SPC-3 T10/1416-D revision 7");
                            break;
                        case 0x0307:
                            scsiOneValue.Add("Device complies with SPC-3 T10/1416-D revision 21");
                            break;
                        case 0x030F:
                            scsiOneValue.Add("Device complies with SPC-3 T10/1416-D revision 22");
                            break;
                        case 0x0312:
                            scsiOneValue.Add("Device complies with SPC-3 T10/1416-D revision 23");
                            break;
                        case 0x0314:
                            scsiOneValue.Add("Device complies with SPC-3 ANSI INCITS 408-2005");
                            break;
                        case 0x0316:
                            scsiOneValue.Add("Device complies with SPC-3 ISO/IEC 14776-453");
                            break;
                        case 0x0320:
                            scsiOneValue.Add("Device complies with SBC-2 (no version claimed)");
                            break;
                        case 0x0322:
                            scsiOneValue.Add("Device complies with SBC-2 T10/1417-D revision 5a");
                            break;
                        case 0x0324:
                            scsiOneValue.Add("Device complies with SBC-2 T10/1417-D revision 15");
                            break;
                        case 0x033B:
                            scsiOneValue.Add("Device complies with SBC-2 T10/1417-D revision 16");
                            break;
                        case 0x033D:
                            scsiOneValue.Add("Device complies with SBC-2 ANSI INCITS 405-2005");
                            break;
                        case 0x033E:
                            scsiOneValue.Add("Device complies with SBC-2 ISO/IEC 14776-322");
                            break;
                        case 0x0340:
                            scsiOneValue.Add("Device complies with OSD (no version claimed)");
                            break;
                        case 0x0341:
                            scsiOneValue.Add("Device complies with OSD T10/1355-D revision 0");
                            break;
                        case 0x0342:
                            scsiOneValue.Add("Device complies with OSD T10/1355-D revision 7a");
                            break;
                        case 0x0343:
                            scsiOneValue.Add("Device complies with OSD T10/1355-D revision 8");
                            break;
                        case 0x0344:
                            scsiOneValue.Add("Device complies with OSD T10/1355-D revision 9");
                            break;
                        case 0x0355:
                            scsiOneValue.Add("Device complies with OSD T10/1355-D revision 10");
                            break;
                        case 0x0356:
                            scsiOneValue.Add("Device complies with OSD ANSI INCITS 400-2004");
                            break;
                        case 0x0360:
                            scsiOneValue.Add("Device complies with SSC-2 (no version claimed)");
                            break;
                        case 0x0374:
                            scsiOneValue.Add("Device complies with SSC-2 T10/1434-D revision 7");
                            break;
                        case 0x0375:
                            scsiOneValue.Add("Device complies with SSC-2 T10/1434-D revision 9");
                            break;
                        case 0x037D:
                            scsiOneValue.Add("Device complies with SSC-2 ANSI INCITS 380-2003");
                            break;
                        case 0x0380:
                            scsiOneValue.Add("Device complies with BCC (no version claimed)");
                            break;
                        case 0x03A0:
                            scsiOneValue.Add("Device complies with MMC-4 (no version claimed)");
                            break;
                        case 0x03B0:
                            scsiOneValue.Add("Device complies with MMC-4 T10/1545-D revision 5");
                            break;
                        case 0x03B1:
                            scsiOneValue.Add("Device complies with MMC-4 T10/1545-D revision 5a");
                            break;
                        case 0x03BD:
                            scsiOneValue.Add("Device complies with MMC-4 T10/1545-D revision 3");
                            break;
                        case 0x03BE:
                            scsiOneValue.Add("Device complies with MMC-4 T10/1545-D revision 3d");
                            break;
                        case 0x03BF:
                            scsiOneValue.Add("Device complies with MMC-4 ANSI INCITS 401-2005");
                            break;
                        case 0x03C0:
                            scsiOneValue.Add("Device complies with ADC (no version claimed)");
                            break;
                        case 0x03D5:
                            scsiOneValue.Add("Device complies with ADC T10/1558-D revision 6");
                            break;
                        case 0x03D6:
                            scsiOneValue.Add("Device complies with ADC T10/1558-D revision 7");
                            break;
                        case 0x03D7:
                            scsiOneValue.Add("Device complies with ADC ANSI INCITS 403-2005");
                            break;
                        case 0x03E0:
                            scsiOneValue.Add("Device complies with SES-2 (no version claimed)");
                            break;
                        case 0x03E1:
                            scsiOneValue.Add("Device complies with SES-2 T10/1559-D revision 16");
                            break;
                        case 0x03E7:
                            scsiOneValue.Add("Device complies with SES-2 T10/1559-D revision 19");
                            break;
                        case 0x03EB:
                            scsiOneValue.Add("Device complies with SES-2 T10/1559-D revision 20");
                            break;
                        case 0x03F0:
                            scsiOneValue.Add("Device complies with SES-2 ANSI INCITS 448-2008");
                            break;
                        case 0x03F2:
                            scsiOneValue.Add("Device complies with SES-2 ISO/IEC 14776-372");
                            break;
                        case 0x0400:
                            scsiOneValue.Add("Device complies with SSC-3 (no version claimed)");
                            break;
                        case 0x0403:
                            scsiOneValue.Add("Device complies with SSC-3 T10/1611-D revision 04a");
                            break;
                        case 0x0407:
                            scsiOneValue.Add("Device complies with SSC-3 T10/1611-D revision 05");
                            break;
                        case 0x0409:
                            scsiOneValue.Add("Device complies with SSC-3 ANSI INCITS 467-2011");
                            break;
                        case 0x040B:
                            scsiOneValue.Add("Device complies with SSC-3 ISO/IEC 14776-333:2013");
                            break;
                        case 0x0420:
                            scsiOneValue.Add("Device complies with MMC-5 (no version claimed)");
                            break;
                        case 0x042F:
                            scsiOneValue.Add("Device complies with MMC-5 T10/1675-D revision 03");
                            break;
                        case 0x0431:
                            scsiOneValue.Add("Device complies with MMC-5 T10/1675-D revision 03b");
                            break;
                        case 0x0432:
                            scsiOneValue.Add("Device complies with MMC-5 T10/1675-D revision 04");
                            break;
                        case 0x0434:
                            scsiOneValue.Add("Device complies with MMC-5 ANSI INCITS 430-2007");
                            break;
                        case 0x0440:
                            scsiOneValue.Add("Device complies with OSD-2 (no version claimed)");
                            break;
                        case 0x0444:
                            scsiOneValue.Add("Device complies with OSD-2 T10/1729-D revision 4");
                            break;
                        case 0x0446:
                            scsiOneValue.Add("Device complies with OSD-2 T10/1729-D revision 5");
                            break;
                        case 0x0448:
                            scsiOneValue.Add("Device complies with OSD-2 ANSI INCITS 458-2011");
                            break;
                        case 0x0460:
                            scsiOneValue.Add("Device complies with SPC-4 (no version claimed)");
                            break;
                        case 0x0461:
                            scsiOneValue.Add("Device complies with SPC-4 T10/BSR INCITS 513 revision 16");
                            break;
                        case 0x0462:
                            scsiOneValue.Add("Device complies with SPC-4 T10/BSR INCITS 513 revision 18");
                            break;
                        case 0x0463:
                            scsiOneValue.Add("Device complies with SPC-4 T10/BSR INCITS 513 revision 23");
                            break;
                        case 0x0466:
                            scsiOneValue.Add("Device complies with SPC-4 T10/BSR INCITS 513 revision 36");
                            break;
                        case 0x0468:
                            scsiOneValue.Add("Device complies with SPC-4 T10/BSR INCITS 513 revision 37");
                            break;
                        case 0x0469:
                            scsiOneValue.Add("Device complies with SPC-4 T10/BSR INCITS 513 revision 37a");
                            break;
                        case 0x046C:
                            scsiOneValue.Add("Device complies with SPC-4 ANSI INCITS 513-2015");
                            break;
                        case 0x0480:
                            scsiOneValue.Add("Device complies with SMC-3 (no version claimed)");
                            break;
                        case 0x0482:
                            scsiOneValue.Add("Device complies with SMC-3 T10/1730-D revision 15");
                            break;
                        case 0x0484:
                            scsiOneValue.Add("Device complies with SMC-3 T10/1730-D revision 16");
                            break;
                        case 0x0486:
                            scsiOneValue.Add("Device complies with SMC-3 ANSI INCITS 484-2012");
                            break;
                        case 0x04A0:
                            scsiOneValue.Add("Device complies with ADC-2 (no version claimed)");
                            break;
                        case 0x04A7:
                            scsiOneValue.Add("Device complies with ADC-2 T10/1741-D revision 7");
                            break;
                        case 0x04AA:
                            scsiOneValue.Add("Device complies with ADC-2 T10/1741-D revision 8");
                            break;
                        case 0x04AC:
                            scsiOneValue.Add("Device complies with ADC-2 ANSI INCITS 441-2008");
                            break;
                        case 0x04C0:
                            scsiOneValue.Add("Device complies with SBC-3 (no version claimed)");
                            break;
                        case 0x04C3:
                            scsiOneValue.Add("Device complies with SBC-3 T10/BSR INCITS 514 revision 35");
                            break;
                        case 0x04C5:
                            scsiOneValue.Add("Device complies with SBC-3 T10/BSR INCITS 514 revision 36");
                            break;
                        case 0x04C8:
                            scsiOneValue.Add("Device complies with SBC-3 ANSI INCITS 514-2014");
                            break;
                        case 0x04E0:
                            scsiOneValue.Add("Device complies with MMC-6 (no version claimed)");
                            break;
                        case 0x04E3:
                            scsiOneValue.Add("Device complies with MMC-6 T10/1836-D revision 02b");
                            break;
                        case 0x04E5:
                            scsiOneValue.Add("Device complies with MMC-6 T10/1836-D revision 02g");
                            break;
                        case 0x04E6:
                            scsiOneValue.Add("Device complies with MMC-6 ANSI INCITS 468-2010");
                            break;
                        case 0x04E7:
                            scsiOneValue
                                .Add("Device complies with MMC-6 ANSI INCITS 468-2010 + MMC-6/AM1 ANSI INCITS 468-2010/AM 1");
                            break;
                        case 0x0500:
                            scsiOneValue.Add("Device complies with ADC-3 (no version claimed)");
                            break;
                        case 0x0502:
                            scsiOneValue.Add("Device complies with ADC-3 T10/1895-D revision 04");
                            break;
                        case 0x0504:
                            scsiOneValue.Add("Device complies with ADC-3 T10/1895-D revision 05");
                            break;
                        case 0x0506:
                            scsiOneValue.Add("Device complies with ADC-3 T10/1895-D revision 05a");
                            break;
                        case 0x050A:
                            scsiOneValue.Add("Device complies with ADC-3 ANSI INCITS 497-2012");
                            break;
                        case 0x0520:
                            scsiOneValue.Add("Device complies with SSC-4 (no version claimed)");
                            break;
                        case 0x0523:
                            scsiOneValue.Add("Device complies with SSC-4 T10/BSR INCITS 516 revision 2");
                            break;
                        case 0x0525:
                            scsiOneValue.Add("Device complies with SSC-4 T10/BSR INCITS 516 revision 3");
                            break;
                        case 0x0527:
                            scsiOneValue.Add("Device complies with SSC-4 ANSI INCITS 516-2013");
                            break;
                        case 0x0560:
                            scsiOneValue.Add("Device complies with OSD-3 (no version claimed)");
                            break;
                        case 0x0580:
                            scsiOneValue.Add("Device complies with SES-3 (no version claimed)");
                            break;
                        case 0x05A0:
                            scsiOneValue.Add("Device complies with SSC-5 (no version claimed)");
                            break;
                        case 0x05C0:
                            scsiOneValue.Add("Device complies with SPC-5 (no version claimed)");
                            break;
                        case 0x05E0:
                            scsiOneValue.Add("Device complies with SFSC (no version claimed)");
                            break;
                        case 0x05E3:
                            scsiOneValue.Add("Device complies with SFSC BSR INCITS 501 revision 01");
                            break;
                        case 0x0600:
                            scsiOneValue.Add("Device complies with SBC-4 (no version claimed)");
                            break;
                        case 0x0620:
                            scsiOneValue.Add("Device complies with ZBC (no version claimed)");
                            break;
                        case 0x0622:
                            scsiOneValue.Add("Device complies with ZBC BSR INCITS 536 revision 02");
                            break;
                        case 0x0640:
                            scsiOneValue.Add("Device complies with ADC-4 (no version claimed)");
                            break;
                        case 0x0820:
                            scsiOneValue.Add("Device complies with SSA-TL2 (no version claimed)");
                            break;
                        case 0x083B:
                            scsiOneValue.Add("Device complies with SSA-TL2 T10.1/1147-D revision 05b");
                            break;
                        case 0x083C:
                            scsiOneValue.Add("Device complies with SSA-TL2 ANSI INCITS 308-1998");
                            break;
                        case 0x0840:
                            scsiOneValue.Add("Device complies with SSA-TL1 (no version claimed)");
                            break;
                        case 0x085B:
                            scsiOneValue.Add("Device complies with SSA-TL1 T10.1/0989-D revision 10b");
                            break;
                        case 0x085C:
                            scsiOneValue.Add("Device complies with SSA-TL1 ANSI INCITS 295-1996");
                            break;
                        case 0x0860:
                            scsiOneValue.Add("Device complies with SSA-S3P (no version claimed)");
                            break;
                        case 0x087B:
                            scsiOneValue.Add("Device complies with SSA-S3P T10.1/1051-D revision 05b");
                            break;
                        case 0x087C:
                            scsiOneValue.Add("Device complies with SSA-S3P ANSI INCITS 309-1998");
                            break;
                        case 0x0880:
                            scsiOneValue.Add("Device complies with SSA-S2P (no version claimed)");
                            break;
                        case 0x089B:
                            scsiOneValue.Add("Device complies with SSA-S2P T10.1/1121-D revision 07b");
                            break;
                        case 0x089C:
                            scsiOneValue.Add("Device complies with SSA-S2P ANSI INCITS 294-1996");
                            break;
                        case 0x08A0:
                            scsiOneValue.Add("Device complies with SIP (no version claimed)");
                            break;
                        case 0x08BB:
                            scsiOneValue.Add("Device complies with SIP T10/0856-D revision 10");
                            break;
                        case 0x08BC:
                            scsiOneValue.Add("Device complies with SIP ANSI INCITS 292-1997");
                            break;
                        case 0x08C0:
                            scsiOneValue.Add("Device complies with FCP (no version claimed)");
                            break;
                        case 0x08DB:
                            scsiOneValue.Add("Device complies with FCP T10/0993-D revision 12");
                            break;
                        case 0x08DC:
                            scsiOneValue.Add("Device complies with FCP ANSI INCITS 269-1996");
                            break;
                        case 0x08E0:
                            scsiOneValue.Add("Device complies with SBP-2 (no version claimed)");
                            break;
                        case 0x08FB:
                            scsiOneValue.Add("Device complies with SBP-2 T10/1155-D revision 04");
                            break;
                        case 0x08FC:
                            scsiOneValue.Add("Device complies with SBP-2 ANSI INCITS 325-1998");
                            break;
                        case 0x0900:
                            scsiOneValue.Add("Device complies with FCP-2 (no version claimed)");
                            break;
                        case 0x0901:
                            scsiOneValue.Add("Device complies with FCP-2 T10/1144-D revision 4");
                            break;
                        case 0x0915:
                            scsiOneValue.Add("Device complies with FCP-2 T10/1144-D revision 7");
                            break;
                        case 0x0916:
                            scsiOneValue.Add("Device complies with FCP-2 T10/1144-D revision 7a");
                            break;
                        case 0x0917:
                            scsiOneValue.Add("Device complies with FCP-2 ANSI INCITS 350-2003");
                            break;
                        case 0x0918:
                            scsiOneValue.Add("Device complies with FCP-2 T10/1144-D revision 8");
                            break;
                        case 0x0920:
                            scsiOneValue.Add("Device complies with SST (no version claimed)");
                            break;
                        case 0x0935:
                            scsiOneValue.Add("Device complies with SST T10/1380-D revision 8b");
                            break;
                        case 0x0940:
                            scsiOneValue.Add("Device complies with SRP (no version claimed)");
                            break;
                        case 0x0954:
                            scsiOneValue.Add("Device complies with SRP T10/1415-D revision 10");
                            break;
                        case 0x0955:
                            scsiOneValue.Add("Device complies with SRP T10/1415-D revision 16a");
                            break;
                        case 0x095C:
                            scsiOneValue.Add("Device complies with SRP ANSI INCITS 365-2002");
                            break;
                        case 0x0960:
                            scsiOneValue.Add("Device complies with iSCSI (no version claimed)");
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
                            scsiOneValue.Add(string.Format("Device complies with iSCSI revision {0}",
                                                           VersionDescriptor & 0x1F));
                            break;
                        case 0x0980:
                            scsiOneValue.Add("Device complies with SBP-3 (no version claimed)");
                            break;
                        case 0x0982:
                            scsiOneValue.Add("Device complies with SBP-3 T10/1467-D revision 1f");
                            break;
                        case 0x0994:
                            scsiOneValue.Add("Device complies with SBP-3 T10/1467-D revision 3");
                            break;
                        case 0x099A:
                            scsiOneValue.Add("Device complies with SBP-3 T10/1467-D revision 4");
                            break;
                        case 0x099B:
                            scsiOneValue.Add("Device complies with SBP-3 T10/1467-D revision 5");
                            break;
                        case 0x099C:
                            scsiOneValue.Add("Device complies with SBP-3 ANSI INCITS 375-2004");
                            break;
                        case 0x09C0:
                            scsiOneValue.Add("Device complies with ADP (no version claimed)");
                            break;
                        case 0x09E0:
                            scsiOneValue.Add("Device complies with ADT (no version claimed)");
                            break;
                        case 0x09F9:
                            scsiOneValue.Add("Device complies with ADT T10/1557-D revision 11");
                            break;
                        case 0x09FA:
                            scsiOneValue.Add("Device complies with ADT T10/1557-D revision 14");
                            break;
                        case 0x09FD:
                            scsiOneValue.Add("Device complies with ADT ANSI INCITS 406-2005");
                            break;
                        case 0x0A00:
                            scsiOneValue.Add("Device complies with FCP-3 (no version claimed)");
                            break;
                        case 0x0A07:
                            scsiOneValue.Add("Device complies with FCP-3 T10/1560-D revision 3f");
                            break;
                        case 0x0A0F:
                            scsiOneValue.Add("Device complies with FCP-3 T10/1560-D revision 4");
                            break;
                        case 0x0A11:
                            scsiOneValue.Add("Device complies with FCP-3 ANSI INCITS 416-2006");
                            break;
                        case 0x0A1C:
                            scsiOneValue.Add("Device complies with FCP-3 ISO/IEC 14776-223");
                            break;
                        case 0x0A20:
                            scsiOneValue.Add("Device complies with ADT-2 (no version claimed)");
                            break;
                        case 0x0A22:
                            scsiOneValue.Add("Device complies with ADT-2 T10/1742-D revision 06");
                            break;
                        case 0x0A27:
                            scsiOneValue.Add("Device complies with ADT-2 T10/1742-D revision 08");
                            break;
                        case 0x0A28:
                            scsiOneValue.Add("Device complies with ADT-2 T10/1742-D revision 09");
                            break;
                        case 0x0A2B:
                            scsiOneValue.Add("Device complies with ADT-2 ANSI INCITS 472-2011");
                            break;
                        case 0x0A40:
                            scsiOneValue.Add("Device complies with FCP-4 (no version claimed)");
                            break;
                        case 0x0A42:
                            scsiOneValue.Add("Device complies with FCP-4 T10/1828-D revision 01");
                            break;
                        case 0x0A44:
                            scsiOneValue.Add("Device complies with FCP-4 T10/1828-D revision 02");
                            break;
                        case 0x0A45:
                            scsiOneValue.Add("Device complies with FCP-4 T10/1828-D revision 02b");
                            break;
                        case 0x0A46:
                            scsiOneValue.Add("Device complies with FCP-4 ANSI INCITS 481-2012");
                            break;
                        case 0x0A60:
                            scsiOneValue.Add("Device complies with ADT-3 (no version claimed)");
                            break;
                        case 0x0AA0:
                            scsiOneValue.Add("Device complies with SPI (no version claimed)");
                            break;
                        case 0x0AB9:
                            scsiOneValue.Add("Device complies with SPI T10/0855-D revision 15a");
                            break;
                        case 0x0ABA:
                            scsiOneValue.Add("Device complies with SPI ANSI INCITS 253-1995");
                            break;
                        case 0x0ABB:
                            scsiOneValue
                                .Add("Device complies with SPI T10/0855-D revision 15a with SPI Amnd revision 3a");
                            break;
                        case 0x0ABC:
                            scsiOneValue
                                .Add("Device complies with SPI ANSI INCITS 253-1995 with SPI Amnd ANSI INCITS 253/AM1-1998");
                            break;
                        case 0x0AC0:
                            scsiOneValue.Add("Device complies with Fast-20 (no version claimed)");
                            break;
                        case 0x0ADB:
                            scsiOneValue.Add("Device complies with Fast-20 T10/1071 revision 06");
                            break;
                        case 0x0ADC:
                            scsiOneValue.Add("Device complies with Fast-20 ANSI INCITS 277-1996");
                            break;
                        case 0x0AE0:
                            scsiOneValue.Add("Device complies with SPI-2 (no version claimed)");
                            break;
                        case 0x0AFB:
                            scsiOneValue.Add("Device complies with SPI-2 T10/1142-D revision 20b");
                            break;
                        case 0x0AFC:
                            scsiOneValue.Add("Device complies with SPI-2 ANSI INCITS 302-1999");
                            break;
                        case 0x0B00:
                            scsiOneValue.Add("Device complies with SPI-3 (no version claimed)");
                            break;
                        case 0x0B18:
                            scsiOneValue.Add("Device complies with SPI-3 T10/1302-D revision 10");
                            break;
                        case 0x0B19:
                            scsiOneValue.Add("Device complies with SPI-3 T10/1302-D revision 13a");
                            break;
                        case 0x0B1A:
                            scsiOneValue.Add("Device complies with SPI-3 T10/1302-D revision 14");
                            break;
                        case 0x0B1C:
                            scsiOneValue.Add("Device complies with SPI-3 ANSI INCITS 336-2000");
                            break;
                        case 0x0B20:
                            scsiOneValue.Add("Device complies with EPI (no version claimed)");
                            break;
                        case 0x0B3B:
                            scsiOneValue.Add("Device complies with EPI T10/1134 revision 16");
                            break;
                        case 0x0B3C:
                            scsiOneValue.Add("Device complies with EPI ANSI INCITS TR-23 1999");
                            break;
                        case 0x0B40:
                            scsiOneValue.Add("Device complies with SPI-4 (no version claimed)");
                            break;
                        case 0x0B54:
                            scsiOneValue.Add("Device complies with SPI-4 T10/1365-D revision 7");
                            break;
                        case 0x0B55:
                            scsiOneValue.Add("Device complies with SPI-4 T10/1365-D revision 9");
                            break;
                        case 0x0B56:
                            scsiOneValue.Add("Device complies with SPI-4 ANSI INCITS 362-2002");
                            break;
                        case 0x0B59:
                            scsiOneValue.Add("Device complies with SPI-4 T10/1365-D revision 10");
                            break;
                        case 0x0B60:
                            scsiOneValue.Add("Device complies with SPI-5 (no version claimed)");
                            break;
                        case 0x0B79:
                            scsiOneValue.Add("Device complies with SPI-5 T10/1525-D revision 3");
                            break;
                        case 0x0B7A:
                            scsiOneValue.Add("Device complies with SPI-5 T10/1525-D revision 5");
                            break;
                        case 0x0B7B:
                            scsiOneValue.Add("Device complies with SPI-5 T10/1525-D revision 6");
                            break;
                        case 0x0B7C:
                            scsiOneValue.Add("Device complies with SPI-5 ANSI INCITS 367-2003");
                            break;
                        case 0x0BE0:
                            scsiOneValue.Add("Device complies with SAS (no version claimed)");
                            break;
                        case 0x0BE1:
                            scsiOneValue.Add("Device complies with SAS T10/1562-D revision 01");
                            break;
                        case 0x0BF5:
                            scsiOneValue.Add("Device complies with SAS T10/1562-D revision 03");
                            break;
                        case 0x0BFA:
                            scsiOneValue.Add("Device complies with SAS T10/1562-D revision 04");
                            break;
                        case 0x0BFB:
                            scsiOneValue.Add("Device complies with SAS T10/1562-D revision 04");
                            break;
                        case 0x0BFC:
                            scsiOneValue.Add("Device complies with SAS T10/1562-D revision 05");
                            break;
                        case 0x0BFD:
                            scsiOneValue.Add("Device complies with SAS ANSI INCITS 376-2003");
                            break;
                        case 0x0C00:
                            scsiOneValue.Add("Device complies with SAS-1.1 (no version claimed)");
                            break;
                        case 0x0C07:
                            scsiOneValue.Add("Device complies with SAS-1.1 T10/1601-D revision 9");
                            break;
                        case 0x0C0F:
                            scsiOneValue.Add("Device complies with SAS-1.1 T10/1601-D revision 10");
                            break;
                        case 0x0C11:
                            scsiOneValue.Add("Device complies with SAS-1.1 ANSI INCITS 417-2006");
                            break;
                        case 0x0C12:
                            scsiOneValue.Add("Device complies with SAS-1.1 ISO/IEC 14776-151");
                            break;
                        case 0x0C20:
                            scsiOneValue.Add("Device complies with SAS-2 (no version claimed)");
                            break;
                        case 0x0C23:
                            scsiOneValue.Add("Device complies with SAS-2 T10/1760-D revision 14");
                            break;
                        case 0x0C27:
                            scsiOneValue.Add("Device complies with SAS-2 T10/1760-D revision 15");
                            break;
                        case 0x0C28:
                            scsiOneValue.Add("Device complies with SAS-2 T10/1760-D revision 16");
                            break;
                        case 0x0C2A:
                            scsiOneValue.Add("Device complies with SAS-2 ANSI INCITS 457-2010");
                            break;
                        case 0x0C40:
                            scsiOneValue.Add("Device complies with SAS-2.1 (no version claimed)");
                            break;
                        case 0x0C48:
                            scsiOneValue.Add("Device complies with SAS-2.1 T10/2125-D revision 04");
                            break;
                        case 0x0C4A:
                            scsiOneValue.Add("Device complies with SAS-2.1 T10/2125-D revision 06");
                            break;
                        case 0x0C4B:
                            scsiOneValue.Add("Device complies with SAS-2.1 T10/2125-D revision 07");
                            break;
                        case 0x0C4E:
                            scsiOneValue.Add("Device complies with SAS-2.1 ANSI INCITS 478-2011");
                            break;
                        case 0x0C4F:
                            scsiOneValue
                                .Add("Device complies with SAS-2.1 ANSI INCITS 478-2011 w/ Amnd 1 ANSI INCITS 478/AM1-2014");
                            break;
                        case 0x0C52:
                            scsiOneValue.Add("Device complies with SAS-2.1 ISO/IEC 14776-153");
                            break;
                        case 0x0C60:
                            scsiOneValue.Add("Device complies with SAS-3 (no version claimed)");
                            break;
                        case 0x0C63:
                            scsiOneValue.Add("Device complies with SAS-3 T10/BSR INCITS 519 revision 05a");
                            break;
                        case 0x0C65:
                            scsiOneValue.Add("Device complies with SAS-3 T10/BSR INCITS 519 revision 06");
                            break;
                        case 0x0C68:
                            scsiOneValue.Add("Device complies with SAS-3 ANSI INCITS 519-2014");
                            break;
                        case 0x0C80:
                            scsiOneValue.Add("Device complies with SAS-4 (no version claimed)");
                            break;
                        case 0x0D20:
                            scsiOneValue.Add("Device complies with FC-PH (no version claimed)");
                            break;
                        case 0x0D3B:
                            scsiOneValue.Add("Device complies with FC-PH ANSI INCITS 230-1994");
                            break;
                        case 0x0D3C:
                            scsiOneValue
                                .Add("Device complies with FC-PH ANSI INCITS 230-1994 with Amnd 1 ANSI INCITS 230/AM1-1996");
                            break;
                        case 0x0D40:
                            scsiOneValue.Add("Device complies with FC-AL (no version claimed)");
                            break;
                        case 0x0D5C:
                            scsiOneValue.Add("Device complies with FC-AL ANSI INCITS 272-1996");
                            break;
                        case 0x0D60:
                            scsiOneValue.Add("Device complies with FC-AL-2 (no version claimed)");
                            break;
                        case 0x0D61:
                            scsiOneValue.Add("Device complies with FC-AL-2 T11/1133-D revision 7.0");
                            break;
                        case 0x0D63:
                            scsiOneValue
                                .Add("Device complies with FC-AL-2 ANSI INCITS 332-1999 with AM1-2003 & AM2-2006");
                            break;
                        case 0x0D64:
                            scsiOneValue.Add("Device complies with FC-AL-2 ANSI INCITS 332-1999 with Amnd 2 AM2-2006");
                            break;
                        case 0x0D65:
                            scsiOneValue.Add("Device complies with FC-AL-2 ISO/IEC 14165-122 with AM1 & AM2");
                            break;
                        case 0x0D7C:
                            scsiOneValue.Add("Device complies with FC-AL-2 ANSI INCITS 332-1999");
                            break;
                        case 0x0D7D:
                            scsiOneValue.Add("Device complies with FC-AL-2 ANSI INCITS 332-1999 with Amnd 1 AM1-2003");
                            break;
                        case 0x0D80:
                            scsiOneValue.Add("Device complies with FC-PH-3 (no version claimed)");
                            break;
                        case 0x0D9C:
                            scsiOneValue.Add("Device complies with FC-PH-3 ANSI INCITS 303-1998");
                            break;
                        case 0x0DA0:
                            scsiOneValue.Add("Device complies with FC-FS (no version claimed)");
                            break;
                        case 0x0DB7:
                            scsiOneValue.Add("Device complies with FC-FS T11/1331-D revision 1.2");
                            break;
                        case 0x0DB8:
                            scsiOneValue.Add("Device complies with FC-FS T11/1331-D revision 1.7");
                            break;
                        case 0x0DBC:
                            scsiOneValue.Add("Device complies with FC-FS ANSI INCITS 373-2003");
                            break;
                        case 0x0DBD:
                            scsiOneValue.Add("Device complies with FC-FS ISO/IEC 14165-251");
                            break;
                        case 0x0DC0:
                            scsiOneValue.Add("Device complies with FC-PI (no version claimed)");
                            break;
                        case 0x0DDC:
                            scsiOneValue.Add("Device complies with FC-PI ANSI INCITS 352-2002");
                            break;
                        case 0x0DE0:
                            scsiOneValue.Add("Device complies with FC-PI-2 (no version claimed)");
                            break;
                        case 0x0DE2:
                            scsiOneValue.Add("Device complies with FC-PI-2 T11/1506-D revision 5.0");
                            break;
                        case 0x0DE4:
                            scsiOneValue.Add("Device complies with FC-PI-2 ANSI INCITS 404-2006");
                            break;
                        case 0x0E00:
                            scsiOneValue.Add("Device complies with FC-FS-2 (no version claimed)");
                            break;
                        case 0x0E02:
                            scsiOneValue.Add("Device complies with FC-FS-2 ANSI INCITS 242-2007");
                            break;
                        case 0x0E03:
                            scsiOneValue
                                .Add("Device complies with FC-FS-2 ANSI INCITS 242-2007 with AM1 ANSI INCITS 242/AM1-2007");
                            break;
                        case 0x0E20:
                            scsiOneValue.Add("Device complies with FC-LS (no version claimed)");
                            break;
                        case 0x0E21:
                            scsiOneValue.Add("Device complies with FC-LS T11/1620-D revision 1.62");
                            break;
                        case 0x0E29:
                            scsiOneValue.Add("Device complies with FC-LS ANSI INCITS 433-2007");
                            break;
                        case 0x0E40:
                            scsiOneValue.Add("Device complies with FC-SP (no version claimed)");
                            break;
                        case 0x0E42:
                            scsiOneValue.Add("Device complies with FC-SP T11/1570-D revision 1.6");
                            break;
                        case 0x0E45:
                            scsiOneValue.Add("Device complies with FC-SP ANSI INCITS 426-2007");
                            break;
                        case 0x0E60:
                            scsiOneValue.Add("Device complies with FC-PI-3 (no version claimed)");
                            break;
                        case 0x0E62:
                            scsiOneValue.Add("Device complies with FC-PI-3 T11/1625-D revision 2.0");
                            break;
                        case 0x0E68:
                            scsiOneValue.Add("Device complies with FC-PI-3 T11/1625-D revision 2.1");
                            break;
                        case 0x0E6A:
                            scsiOneValue.Add("Device complies with FC-PI-3 T11/1625-D revision 4.0");
                            break;
                        case 0x0E6E:
                            scsiOneValue.Add("Device complies with FC-PI-3 ANSI INCITS 460-2011");
                            break;
                        case 0x0E80:
                            scsiOneValue.Add("Device complies with FC-PI-4 (no version claimed)");
                            break;
                        case 0x0E82:
                            scsiOneValue.Add("Device complies with FC-PI-4 T11/1647-D revision 8.0");
                            break;
                        case 0x0E88:
                            scsiOneValue.Add("Device complies with FC-PI-4 ANSI INCITS 450-2009");
                            break;
                        case 0x0EA0:
                            scsiOneValue.Add("Device complies with FC 10GFC (no version claimed)");
                            break;
                        case 0x0EA2:
                            scsiOneValue.Add("Device complies with FC 10GFC ANSI INCITS 364-2003");
                            break;
                        case 0x0EA3:
                            scsiOneValue.Add("Device complies with FC 10GFC ISO/IEC 14165-116");
                            break;
                        case 0x0EA5:
                            scsiOneValue.Add("Device complies with FC 10GFC ISO/IEC 14165-116 with AM1");
                            break;
                        case 0x0EA6:
                            scsiOneValue
                                .Add("Device complies with FC 10GFC ANSI INCITS 364-2003 with AM1 ANSI INCITS 364/AM1-2007");
                            break;
                        case 0x0EC0:
                            scsiOneValue.Add("Device complies with FC-SP-2 (no version claimed)");
                            break;
                        case 0x0EE0:
                            scsiOneValue.Add("Device complies with FC-FS-3 (no version claimed)");
                            break;
                        case 0x0EE2:
                            scsiOneValue.Add("Device complies with FC-FS-3 T11/1861-D revision 0.9");
                            break;
                        case 0x0EE7:
                            scsiOneValue.Add("Device complies with FC-FS-3 T11/1861-D revision 1.0");
                            break;
                        case 0x0EE9:
                            scsiOneValue.Add("Device complies with FC-FS-3 T11/1861-D revision 1.10");
                            break;
                        case 0x0EEB:
                            scsiOneValue.Add("Device complies with FC-FS-3 ANSI INCITS 470-2011");
                            break;
                        case 0x0F00:
                            scsiOneValue.Add("Device complies with FC-LS-2 (no version claimed)");
                            break;
                        case 0x0F03:
                            scsiOneValue.Add("Device complies with FC-LS-2 T11/2103-D revision 2.11");
                            break;
                        case 0x0F05:
                            scsiOneValue.Add("Device complies with FC-LS-2 T11/2103-D revision 2.21");
                            break;
                        case 0x0F07:
                            scsiOneValue.Add("Device complies with FC-LS-2 ANSI INCITS 477-2011");
                            break;
                        case 0x0F20:
                            scsiOneValue.Add("Device complies with FC-PI-5 (no version claimed)");
                            break;
                        case 0x0F27:
                            scsiOneValue.Add("Device complies with FC-PI-5 T11/2118-D revision 2.00");
                            break;
                        case 0x0F28:
                            scsiOneValue.Add("Device complies with FC-PI-5 T11/2118-D revision 3.00");
                            break;
                        case 0x0F2A:
                            scsiOneValue.Add("Device complies with FC-PI-5 T11/2118-D revision 6.00");
                            break;
                        case 0x0F2B:
                            scsiOneValue.Add("Device complies with FC-PI-5 T11/2118-D revision 6.10");
                            break;
                        case 0x0F2E:
                            scsiOneValue.Add("Device complies with FC-PI-5 ANSI INCITS 479-2011");
                            break;
                        case 0x0F40:
                            scsiOneValue.Add("Device complies with FC-PI-6 (no version claimed)");
                            break;
                        case 0x0F60:
                            scsiOneValue.Add("Device complies with FC-FS-4 (no version claimed)");
                            break;
                        case 0x0F80:
                            scsiOneValue.Add("Device complies with FC-LS-3 (no version claimed)");
                            break;
                        case 0x12A0:
                            scsiOneValue.Add("Device complies with FC-SCM (no version claimed)");
                            break;
                        case 0x12A3:
                            scsiOneValue.Add("Device complies with FC-SCM T11/1824DT revision 1.0");
                            break;
                        case 0x12A5:
                            scsiOneValue.Add("Device complies with FC-SCM T11/1824DT revision 1.1");
                            break;
                        case 0x12A7:
                            scsiOneValue.Add("Device complies with FC-SCM T11/1824DT revision 1.4");
                            break;
                        case 0x12AA:
                            scsiOneValue.Add("Device complies with FC-SCM INCITS TR-47 2012");
                            break;
                        case 0x12C0:
                            scsiOneValue.Add("Device complies with FC-DA-2 (no version claimed)");
                            break;
                        case 0x12C3:
                            scsiOneValue.Add("Device complies with FC-DA-2 T11/1870DT revision 1.04");
                            break;
                        case 0x12C5:
                            scsiOneValue.Add("Device complies with FC-DA-2 T11/1870DT revision 1.06");
                            break;
                        case 0x12C9:
                            scsiOneValue.Add("Device complies with FC-DA-2 INCITS TR-49 2012");
                            break;
                        case 0x12E0:
                            scsiOneValue.Add("Device complies with FC-DA (no version claimed)");
                            break;
                        case 0x12E2:
                            scsiOneValue.Add("Device complies with FC-DA T11/1513-DT revision 3.1");
                            break;
                        case 0x12E8:
                            scsiOneValue.Add("Device complies with FC-DA ANSI INCITS TR-36 2004");
                            break;
                        case 0x12E9:
                            scsiOneValue.Add("Device complies with FC-DA ISO/IEC 14165-341");
                            break;
                        case 0x1300:
                            scsiOneValue.Add("Device complies with FC-Tape (no version claimed)");
                            break;
                        case 0x1301:
                            scsiOneValue.Add("Device complies with FC-Tape T11/1315 revision 1.16");
                            break;
                        case 0x131B:
                            scsiOneValue.Add("Device complies with FC-Tape T11/1315 revision 1.17");
                            break;
                        case 0x131C:
                            scsiOneValue.Add("Device complies with FC-Tape ANSI INCITS TR-24 1999");
                            break;
                        case 0x1320:
                            scsiOneValue.Add("Device complies with FC-FLA (no version claimed)");
                            break;
                        case 0x133B:
                            scsiOneValue.Add("Device complies with FC-FLA T11/1235 revision 7");
                            break;
                        case 0x133C:
                            scsiOneValue.Add("Device complies with FC-FLA ANSI INCITS TR-20 1998");
                            break;
                        case 0x1340:
                            scsiOneValue.Add("Device complies with FC-PLDA (no version claimed)");
                            break;
                        case 0x135B:
                            scsiOneValue.Add("Device complies with FC-PLDA T11/1162 revision 2.1");
                            break;
                        case 0x135C:
                            scsiOneValue.Add("Device complies with FC-PLDA ANSI INCITS TR-19 1998");
                            break;
                        case 0x1360:
                            scsiOneValue.Add("Device complies with SSA-PH2 (no version claimed)");
                            break;
                        case 0x137B:
                            scsiOneValue.Add("Device complies with SSA-PH2 T10.1/1145-D revision 09c");
                            break;
                        case 0x137C:
                            scsiOneValue.Add("Device complies with SSA-PH2 ANSI INCITS 293-1996");
                            break;
                        case 0x1380:
                            scsiOneValue.Add("Device complies with SSA-PH3 (no version claimed)");
                            break;
                        case 0x139B:
                            scsiOneValue.Add("Device complies with SSA-PH3 T10.1/1146-D revision 05b");
                            break;
                        case 0x139C:
                            scsiOneValue.Add("Device complies with SSA-PH3 ANSI INCITS 307-1998");
                            break;
                        case 0x14A0:
                            scsiOneValue.Add("Device complies with IEEE 1394 (no version claimed)");
                            break;
                        case 0x14BD:
                            scsiOneValue.Add("Device complies with ANSI IEEE 1394-1995");
                            break;
                        case 0x14C0:
                            scsiOneValue.Add("Device complies with IEEE 1394a (no version claimed)");
                            break;
                        case 0x14E0:
                            scsiOneValue.Add("Device complies with IEEE 1394b (no version claimed)");
                            break;
                        case 0x15E0:
                            scsiOneValue.Add("Device complies with ATA/ATAPI-6 (no version claimed)");
                            break;
                        case 0x15FD:
                            scsiOneValue.Add("Device complies with ATA/ATAPI-6 ANSI INCITS 361-2002");
                            break;
                        case 0x1600:
                            scsiOneValue.Add("Device complies with ATA/ATAPI-7 (no version claimed)");
                            break;
                        case 0x1602:
                            scsiOneValue.Add("Device complies with ATA/ATAPI-7 T13/1532-D revision 3");
                            break;
                        case 0x161C:
                            scsiOneValue.Add("Device complies with ATA/ATAPI-7 ANSI INCITS 397-2005");
                            break;
                        case 0x161E:
                            scsiOneValue.Add("Device complies with ATA/ATAPI-7 ISO/IEC 24739");
                            break;
                        case 0x1620:
                            scsiOneValue.Add("Device complies with ATA/ATAPI-8 ATA8-AAM (no version claimed)");
                            break;
                        case 0x1621:
                            scsiOneValue
                                .Add("Device complies with ATA/ATAPI-8 ATA8-APT Parallel Transport (no version claimed)");
                            break;
                        case 0x1622:
                            scsiOneValue
                                .Add("Device complies with ATA/ATAPI-8 ATA8-AST Serial Transport (no version claimed)");
                            break;
                        case 0x1623:
                            scsiOneValue
                                .Add("Device complies with ATA/ATAPI-8 ATA8-ACS ATA/ATAPI Command Set (no version claimed)");
                            break;
                        case 0x1628:
                            scsiOneValue.Add("Device complies with ATA/ATAPI-8 ATA8-AAM ANSI INCITS 451-2008");
                            break;
                        case 0x162A:
                            scsiOneValue
                                .Add("Device complies with ATA/ATAPI-8 ATA8-ACS ANSI INCITS 452-2009 w/ Amendment 1");
                            break;
                        case 0x1728:
                            scsiOneValue.Add("Device complies with Universal Serial Bus Specification, Revision 1.1");
                            break;
                        case 0x1729:
                            scsiOneValue.Add("Device complies with Universal Serial Bus Specification, Revision 2.0");
                            break;
                        case 0x1730:
                            scsiOneValue
                                .Add("Device complies with USB Mass Storage Class Bulk-Only Transport, Revision 1.0");
                            break;
                        case 0x1740:
                            scsiOneValue.Add("Device complies with UAS (no version claimed)");
                            break;
                        case 0x1743:
                            scsiOneValue.Add("Device complies with UAS T10/2095-D revision 02");
                            break;
                        case 0x1747:
                            scsiOneValue.Add("Device complies with UAS T10/2095-D revision 04");
                            break;
                        case 0x1748:
                            scsiOneValue.Add("Device complies with UAS ANSI INCITS 471-2010");
                            break;
                        case 0x1749:
                            scsiOneValue.Add("Device complies with UAS ISO/IEC 14776-251:2014");
                            break;
                        case 0x1761:
                            scsiOneValue.Add("Device complies with ACS-2 (no version claimed)");
                            break;
                        case 0x1762:
                            scsiOneValue.Add("Device complies with ACS-2 ANSI INCITS 482-2013");
                            break;
                        case 0x1765:
                            scsiOneValue.Add("Device complies with ACS-3 (no version claimed)");
                            break;
                        case 0x1780:
                            scsiOneValue.Add("Device complies with UAS-2 (no version claimed)");
                            break;
                        case 0x1EA0:
                            scsiOneValue.Add("Device complies with SAT (no version claimed)");
                            break;
                        case 0x1EA7:
                            scsiOneValue.Add("Device complies with SAT T10/1711-D revision 8");
                            break;
                        case 0x1EAB:
                            scsiOneValue.Add("Device complies with SAT T10/1711-D revision 9");
                            break;
                        case 0x1EAD:
                            scsiOneValue.Add("Device complies with SAT ANSI INCITS 431-2007");
                            break;
                        case 0x1EC0:
                            scsiOneValue.Add("Device complies with SAT-2 (no version claimed)");
                            break;
                        case 0x1EC4:
                            scsiOneValue.Add("Device complies with SAT-2 T10/1826-D revision 06");
                            break;
                        case 0x1EC8:
                            scsiOneValue.Add("Device complies with SAT-2 T10/1826-D revision 09");
                            break;
                        case 0x1ECA:
                            scsiOneValue.Add("Device complies with SAT-2 ANSI INCITS 465-2010");
                            break;
                        case 0x1EE0:
                            scsiOneValue.Add("Device complies with SAT-3 (no version claimed)");
                            break;
                        case 0x1EE2:
                            scsiOneValue.Add("Device complies with SAT-3 T10/BSR INCITS 517 revision 4");
                            break;
                        case 0x1EE4:
                            scsiOneValue.Add("Device complies with SAT-3 T10/BSR INCITS 517 revision 7");
                            break;
                        case 0x1EE8:
                            scsiOneValue.Add("Device complies with SAT-3 ANSI INCITS 517-2015");
                            break;
                        case 0x1F00:
                            scsiOneValue.Add("Device complies with SAT-4 (no version claimed)");
                            break;
                        case 0x20A0:
                            scsiOneValue.Add("Device complies with SPL (no version claimed)");
                            break;
                        case 0x20A3:
                            scsiOneValue.Add("Device complies with SPL T10/2124-D revision 6a");
                            break;
                        case 0x20A5:
                            scsiOneValue.Add("Device complies with SPL T10/2124-D revision 7");
                            break;
                        case 0x20A7:
                            scsiOneValue.Add("Device complies with SPL ANSI INCITS 476-2011");
                            break;
                        case 0x20A8:
                            scsiOneValue
                                .Add("Device complies with SPL ANSI INCITS 476-2011 + SPL AM1 INCITS 476/AM1 2012");
                            break;
                        case 0x20AA:
                            scsiOneValue.Add("Device complies with SPL ISO/IEC 14776-261:2012");
                            break;
                        case 0x20C0:
                            scsiOneValue.Add("Device complies with SPL-2 (no version claimed)");
                            break;
                        case 0x20C2:
                            scsiOneValue.Add("Device complies with SPL-2 T10/BSR INCITS 505 revision 4");
                            break;
                        case 0x20C4:
                            scsiOneValue.Add("Device complies with SPL-2 T10/BSR INCITS 505 revision 5");
                            break;
                        case 0x20C8:
                            scsiOneValue.Add("Device complies with SPL-2 ANSI INCITS 505-2013");
                            break;
                        case 0x20E0:
                            scsiOneValue.Add("Device complies with SPL-3 (no version claimed)");
                            break;
                        case 0x20E4:
                            scsiOneValue.Add("Device complies with SPL-3 T10/BSR INCITS 492 revision 6");
                            break;
                        case 0x20E6:
                            scsiOneValue.Add("Device complies with SPL-3 T10/BSR INCITS 492 revision 7");
                            break;
                        case 0x20E8:
                            scsiOneValue.Add("Device complies with SPL-3 ANSI INCITS 492-2015");
                            break;
                        case 0x2100:
                            scsiOneValue.Add("Device complies with SPL-4 (no version claimed)");
                            break;
                        case 0x21E0:
                            scsiOneValue.Add("Device complies with SOP (no version claimed)");
                            break;
                        case 0x21E4:
                            scsiOneValue.Add("Device complies with SOP T10/BSR INCITS 489 revision 4");
                            break;
                        case 0x21E6:
                            scsiOneValue.Add("Device complies with SOP T10/BSR INCITS 489 revision 5");
                            break;
                        case 0x21E8:
                            scsiOneValue.Add("Device complies with SOP ANSI INCITS 489-2014");
                            break;
                        case 0x2200:
                            scsiOneValue.Add("Device complies with PQI (no version claimed)");
                            break;
                        case 0x2204:
                            scsiOneValue.Add("Device complies with PQI T10/BSR INCITS 490 revision 6");
                            break;
                        case 0x2206:
                            scsiOneValue.Add("Device complies with PQI T10/BSR INCITS 490 revision 7");
                            break;
                        case 0x2208:
                            scsiOneValue.Add("Device complies with PQI ANSI INCITS 490-2014");
                            break;
                        case 0x2220:
                            scsiOneValue.Add("Device complies with SOP-2 (no version claimed)");
                            break;
                        case 0x2240:
                            scsiOneValue.Add("Device complies with PQI-2 (no version claimed)");
                            break;
                        case 0xFFC0:
                            scsiOneValue.Add("Device complies with IEEE 1667 (no version claimed)");
                            break;
                        case 0xFFC1:
                            scsiOneValue.Add("Device complies with IEEE 1667-2006");
                            break;
                        case 0xFFC2:
                            scsiOneValue.Add("Device complies with IEEE 1667-2009");
                            break;
                        default:
                            scsiOneValue.Add(string.Format("Device complies with unknown standard code 0x{0:X4}",
                                                           VersionDescriptor));
                            break;
                    }
                }
            }

            return scsiOneValue;
        }
    }
}