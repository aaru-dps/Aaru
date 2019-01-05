// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'device-info' verb.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Devices;
using Mono.Options;
using DeviceInfo = DiscImageChef.Core.Devices.Info.DeviceInfo;

namespace DiscImageChef.Commands
{
    class DeviceInfoCommand : Command
    {
        string devicePath;
        string outputPrefix;

        bool showHelp;

        public DeviceInfoCommand() : base("device-info", "Gets information about a device.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS] devicepath",
                "",
                Help,
                {"output-prefix|w=", "Name of character encoding to use.", s => outputPrefix = s},
                {"help|h|?", "Show this message and exit.", v => showHelp                    = v != null}
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = Options.Parse(arguments);

            if(showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);
                return 0;
            }

            MainClass.PrintCopyright();
            if(MainClass.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
            if(MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            if(extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return 1;
            }

            if(extra.Count == 0)
            {
                DicConsole.ErrorWriteLine("Missing device path.");
                return 1;
            }

            devicePath = extra[0];

            DicConsole.DebugWriteLine("Device-Info command", "--debug={0}",         MainClass.Debug);
            DicConsole.DebugWriteLine("Device-Info command", "--device={0}",        devicePath);
            DicConsole.DebugWriteLine("Device-Info command", "--output-prefix={0}", outputPrefix);
            DicConsole.DebugWriteLine("Device-Info command", "--verbose={0}",       MainClass.Verbose);

            if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Device dev = new Device(devicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return 1;
            }

            Statistics.AddDevice(dev);

            if(dev.IsUsb)
            {
                DicConsole.WriteLine("USB device");
                if(dev.UsbDescriptors != null)
                    DicConsole.WriteLine("USB descriptor is {0} bytes", dev.UsbDescriptors.Length);
                DicConsole.WriteLine("USB Vendor ID: {0:X4}",  dev.UsbVendorId);
                DicConsole.WriteLine("USB Product ID: {0:X4}", dev.UsbProductId);
                DicConsole.WriteLine("USB Manufacturer: {0}",  dev.UsbManufacturerString);
                DicConsole.WriteLine("USB Product: {0}",       dev.UsbProductString);
                DicConsole.WriteLine("USB Serial number: {0}", dev.UsbSerialString);
                DicConsole.WriteLine();
            }

            if(dev.IsFireWire)
            {
                DicConsole.WriteLine("FireWire device");
                DicConsole.WriteLine("FireWire Vendor ID: {0:X6}", dev.FireWireVendor);
                DicConsole.WriteLine("FireWire Model ID: {0:X6}",  dev.FireWireModel);
                DicConsole.WriteLine("FireWire Manufacturer: {0}", dev.FireWireVendorName);
                DicConsole.WriteLine("FireWire Model: {0}",        dev.FireWireModelName);
                DicConsole.WriteLine("FireWire GUID: {0:X16}",     dev.FireWireGuid);
                DicConsole.WriteLine();
            }

            if(dev.IsPcmcia)
            {
                DicConsole.WriteLine("PCMCIA device");
                DicConsole.WriteLine("PCMCIA CIS is {0} bytes", dev.Cis.Length);
                Tuple[] tuples = CIS.GetTuples(dev.Cis);
                if(tuples != null)
                    foreach(Tuple tuple in tuples)
                        switch(tuple.Code)
                        {
                            case TupleCodes.CISTPL_NULL:
                            case TupleCodes.CISTPL_END: break;
                            case TupleCodes.CISTPL_DEVICEGEO:
                            case TupleCodes.CISTPL_DEVICEGEO_A:
                                DicConsole.WriteLine("{0}", CIS.PrettifyDeviceGeometryTuple(tuple));
                                break;
                            case TupleCodes.CISTPL_MANFID:
                                DicConsole.WriteLine("{0}", CIS.PrettifyManufacturerIdentificationTuple(tuple));
                                break;
                            case TupleCodes.CISTPL_VERS_1:
                                DicConsole.WriteLine("{0}", CIS.PrettifyLevel1VersionTuple(tuple));
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
                                DicConsole.DebugWriteLine("Device-Info command", "Found undecoded tuple ID {0}",
                                                          tuple.Code);
                                break;
                            default:
                                DicConsole.DebugWriteLine("Device-Info command", "Found unknown tuple ID 0x{0:X2}",
                                                          (byte)tuple.Code);
                                break;
                        }
                else DicConsole.DebugWriteLine("Device-Info command", "Could not get tuples");
            }

            DeviceInfo devInfo = new DeviceInfo(dev);

            if(devInfo.AtaIdentify != null)
            {
                DataFile.WriteTo("Device-Info command", outputPrefix, "_ata_identify.bin", "ATA IDENTIFY",
                                 devInfo.AtaIdentify);

                DicConsole.WriteLine(Identify.Prettify(devInfo.AtaIdentify));

                if(devInfo.AtaMcptError.HasValue)
                {
                    DicConsole.WriteLine("Device supports the Media Card Pass Through Command Set");
                    switch(devInfo.AtaMcptError.Value.DeviceHead & 0x7)
                    {
                        case 0:
                            DicConsole.WriteLine("Device reports incorrect media card type");
                            break;
                        case 1:
                            DicConsole.WriteLine("Device contains a Secure Digital card");
                            break;
                        case 2:
                            DicConsole.WriteLine("Device contains a MultiMediaCard ");
                            break;
                        case 3:
                            DicConsole.WriteLine("Device contains a Secure Digital I/O card");
                            break;
                        case 4:
                            DicConsole.WriteLine("Device contains a Smart Media card");
                            break;
                        default:
                            DicConsole.WriteLine("Device contains unknown media card type {0}",
                                                 devInfo.AtaMcptError.Value.DeviceHead & 0x07);
                            break;
                    }

                    if((devInfo.AtaMcptError.Value.DeviceHead & 0x08) == 0x08)
                        DicConsole.WriteLine("Media card is write protected");

                    ushort specificData = (ushort)(devInfo.AtaMcptError.Value.CylinderHigh * 0x100 +
                                                   devInfo.AtaMcptError.Value.CylinderLow);
                    if(specificData != 0) DicConsole.WriteLine("Card specific data: 0x{0:X4}", specificData);
                }
            }

            if(devInfo.AtapiIdentify != null)
            {
                DataFile.WriteTo("Device-Info command", outputPrefix, "_atapi_identify.bin", "ATAPI IDENTIFY",
                                 devInfo.AtapiIdentify);

                DicConsole.WriteLine(Identify.Prettify(devInfo.AtapiIdentify));
            }

            if(devInfo.ScsiInquiry != null)
            {
                if(dev.Type != DeviceType.ATAPI) DicConsole.WriteLine("SCSI device");

                DataFile.WriteTo("Device-Info command", outputPrefix, "_scsi_inquiry.bin", "SCSI INQUIRY",
                                 devInfo.ScsiInquiryData);

                DicConsole.WriteLine(Inquiry.Prettify(devInfo.ScsiInquiry));

                if(devInfo.ScsiEvpdPages != null)
                    foreach(KeyValuePair<byte, byte[]> page in devInfo.ScsiEvpdPages)
                        if(page.Key >= 0x01 && page.Key <= 0x7F)
                        {
                            DicConsole.WriteLine("ASCII Page {0:X2}h: {1}", page.Key, EVPD.DecodeASCIIPage(page.Value));

                            DataFile.WriteTo("Device-Info command", outputPrefix, page.Value);
                        }
                        else if(page.Key == 0x80)
                        {
                            DicConsole.WriteLine("Unit Serial Number: {0}", EVPD.DecodePage80(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0x81)
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_81(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0x82)
                        {
                            DicConsole.WriteLine("ASCII implemented operating definitions: {0}",
                                                 EVPD.DecodePage82(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0x83)
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_83(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0x84)
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_84(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0x85)
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_85(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0x86)
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_86(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0x89)
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_89(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xB0)
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_B0(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xB1)
                        {
                            DicConsole.WriteLine("Manufacturer-assigned Serial Number: {0}",
                                                 EVPD.DecodePageB1(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xB2)
                        {
                            DicConsole.WriteLine("TapeAlert Supported Flags Bitmap: 0x{0:X16}",
                                                 EVPD.DecodePageB2(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xB3)
                        {
                            DicConsole.WriteLine("Automation Device Serial Number: {0}", EVPD.DecodePageB3(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xB4)
                        {
                            DicConsole.WriteLine("Data Transfer Device Element Address: 0x{0}",
                                                 EVPD.DecodePageB4(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xC0 &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "quantum")
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_Quantum(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xC0 &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "seagate")
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_Seagate(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xC0 &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "ibm")
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_IBM(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xC1 &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "ibm")
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_C1_IBM(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if((page.Key == 0xC0 || page.Key == 0xC1) &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "certance")
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_C1_Certance(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if((page.Key == 0xC2 || page.Key == 0xC3 || page.Key == 0xC4 || page.Key == 0xC5 ||
                                 page.Key == 0xC6) &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "certance")
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_C2_C3_C4_C5_C6_Certance(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if((page.Key == 0xC0 || page.Key == 0xC1 || page.Key == 0xC2 || page.Key == 0xC3 ||
                                 page.Key == 0xC4 || page.Key == 0xC5) && StringHandlers
                                                                         .CToString(devInfo.ScsiInquiry.Value
                                                                                           .VendorIdentification)
                                                                         .ToLowerInvariant().Trim() == "hp")
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_to_C5_HP(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else if(page.Key == 0xDF &&
                                StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification)
                                              .ToLowerInvariant().Trim() == "certance")
                        {
                            DicConsole.WriteLine("{0}", EVPD.PrettifyPage_DF_Certance(page.Value));
                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }
                        else
                        {
                            if(page.Key == 0x00) continue;

                            DicConsole.DebugWriteLine("Device-Info command", "Found undecoded SCSI VPD page 0x{0:X2}",
                                                      page.Key);

                            DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                        }

                if(devInfo.ScsiModeSense6 != null)
                    DataFile.WriteTo("Device-Info command", outputPrefix, "_scsi_modesense6.bin", "SCSI MODE SENSE",
                                     devInfo.ScsiModeSense6);

                if(devInfo.ScsiModeSense10 != null)
                    DataFile.WriteTo("Device-Info command", outputPrefix, "_scsi_modesense10.bin", "SCSI MODE SENSE",
                                     devInfo.ScsiModeSense10);

                if(devInfo.ScsiMode.HasValue)
                    PrintScsiModePages.Print(devInfo.ScsiMode.Value,
                                             (PeripheralDeviceTypes)devInfo.ScsiInquiry.Value.PeripheralDeviceType,
                                             devInfo.ScsiInquiry.Value.VendorIdentification);

                if(devInfo.MmcConfiguration != null)
                {
                    DataFile.WriteTo("Device-Info command", outputPrefix, "_mmc_getconfiguration.bin",
                                     "MMC GET CONFIGURATION", devInfo.MmcConfiguration);

                    Features.SeparatedFeatures ftr = Features.Separate(devInfo.MmcConfiguration);

                    DicConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION length is {0} bytes",
                                              ftr.DataLength);
                    DicConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION current profile is {0:X4}h",
                                              ftr.CurrentProfile);
                    if(ftr.Descriptors != null)
                    {
                        DicConsole.WriteLine("SCSI MMC GET CONFIGURATION Features:");
                        foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                        {
                            DicConsole.DebugWriteLine("Device-Info command", "Feature {0:X4}h", desc.Code);

                            switch(desc.Code)
                            {
                                case 0x0000:
                                    DicConsole.WriteLine(Features.Prettify_0000(desc.Data));
                                    break;
                                case 0x0001:
                                    DicConsole.WriteLine(Features.Prettify_0001(desc.Data));
                                    break;
                                case 0x0002:
                                    DicConsole.WriteLine(Features.Prettify_0002(desc.Data));
                                    break;
                                case 0x0003:
                                    DicConsole.WriteLine(Features.Prettify_0003(desc.Data));
                                    break;
                                case 0x0004:
                                    DicConsole.WriteLine(Features.Prettify_0004(desc.Data));
                                    break;
                                case 0x0010:
                                    DicConsole.WriteLine(Features.Prettify_0010(desc.Data));
                                    break;
                                case 0x001D:
                                    DicConsole.WriteLine(Features.Prettify_001D(desc.Data));
                                    break;
                                case 0x001E:
                                    DicConsole.WriteLine(Features.Prettify_001E(desc.Data));
                                    break;
                                case 0x001F:
                                    DicConsole.WriteLine(Features.Prettify_001F(desc.Data));
                                    break;
                                case 0x0020:
                                    DicConsole.WriteLine(Features.Prettify_0020(desc.Data));
                                    break;
                                case 0x0021:
                                    DicConsole.WriteLine(Features.Prettify_0021(desc.Data));
                                    break;
                                case 0x0022:
                                    DicConsole.WriteLine(Features.Prettify_0022(desc.Data));
                                    break;
                                case 0x0023:
                                    DicConsole.WriteLine(Features.Prettify_0023(desc.Data));
                                    break;
                                case 0x0024:
                                    DicConsole.WriteLine(Features.Prettify_0024(desc.Data));
                                    break;
                                case 0x0025:
                                    DicConsole.WriteLine(Features.Prettify_0025(desc.Data));
                                    break;
                                case 0x0026:
                                    DicConsole.WriteLine(Features.Prettify_0026(desc.Data));
                                    break;
                                case 0x0027:
                                    DicConsole.WriteLine(Features.Prettify_0027(desc.Data));
                                    break;
                                case 0x0028:
                                    DicConsole.WriteLine(Features.Prettify_0028(desc.Data));
                                    break;
                                case 0x0029:
                                    DicConsole.WriteLine(Features.Prettify_0029(desc.Data));
                                    break;
                                case 0x002A:
                                    DicConsole.WriteLine(Features.Prettify_002A(desc.Data));
                                    break;
                                case 0x002B:
                                    DicConsole.WriteLine(Features.Prettify_002B(desc.Data));
                                    break;
                                case 0x002C:
                                    DicConsole.WriteLine(Features.Prettify_002C(desc.Data));
                                    break;
                                case 0x002D:
                                    DicConsole.WriteLine(Features.Prettify_002D(desc.Data));
                                    break;
                                case 0x002E:
                                    DicConsole.WriteLine(Features.Prettify_002E(desc.Data));
                                    break;
                                case 0x002F:
                                    DicConsole.WriteLine(Features.Prettify_002F(desc.Data));
                                    break;
                                case 0x0030:
                                    DicConsole.WriteLine(Features.Prettify_0030(desc.Data));
                                    break;
                                case 0x0031:
                                    DicConsole.WriteLine(Features.Prettify_0031(desc.Data));
                                    break;
                                case 0x0032:
                                    DicConsole.WriteLine(Features.Prettify_0032(desc.Data));
                                    break;
                                case 0x0033:
                                    DicConsole.WriteLine(Features.Prettify_0033(desc.Data));
                                    break;
                                case 0x0035:
                                    DicConsole.WriteLine(Features.Prettify_0035(desc.Data));
                                    break;
                                case 0x0037:
                                    DicConsole.WriteLine(Features.Prettify_0037(desc.Data));
                                    break;
                                case 0x0038:
                                    DicConsole.WriteLine(Features.Prettify_0038(desc.Data));
                                    break;
                                case 0x003A:
                                    DicConsole.WriteLine(Features.Prettify_003A(desc.Data));
                                    break;
                                case 0x003B:
                                    DicConsole.WriteLine(Features.Prettify_003B(desc.Data));
                                    break;
                                case 0x0040:
                                    DicConsole.WriteLine(Features.Prettify_0040(desc.Data));
                                    break;
                                case 0x0041:
                                    DicConsole.WriteLine(Features.Prettify_0041(desc.Data));
                                    break;
                                case 0x0042:
                                    DicConsole.WriteLine(Features.Prettify_0042(desc.Data));
                                    break;
                                case 0x0050:
                                    DicConsole.WriteLine(Features.Prettify_0050(desc.Data));
                                    break;
                                case 0x0051:
                                    DicConsole.WriteLine(Features.Prettify_0051(desc.Data));
                                    break;
                                case 0x0080:
                                    DicConsole.WriteLine(Features.Prettify_0080(desc.Data));
                                    break;
                                case 0x0100:
                                    DicConsole.WriteLine(Features.Prettify_0100(desc.Data));
                                    break;
                                case 0x0101:
                                    DicConsole.WriteLine(Features.Prettify_0101(desc.Data));
                                    break;
                                case 0x0102:
                                    DicConsole.WriteLine(Features.Prettify_0102(desc.Data));
                                    break;
                                case 0x0103:
                                    DicConsole.WriteLine(Features.Prettify_0103(desc.Data));
                                    break;
                                case 0x0104:
                                    DicConsole.WriteLine(Features.Prettify_0104(desc.Data));
                                    break;
                                case 0x0105:
                                    DicConsole.WriteLine(Features.Prettify_0105(desc.Data));
                                    break;
                                case 0x0106:
                                    DicConsole.WriteLine(Features.Prettify_0106(desc.Data));
                                    break;
                                case 0x0107:
                                    DicConsole.WriteLine(Features.Prettify_0107(desc.Data));
                                    break;
                                case 0x0108:
                                    DicConsole.WriteLine(Features.Prettify_0108(desc.Data));
                                    break;
                                case 0x0109:
                                    DicConsole.WriteLine(Features.Prettify_0109(desc.Data));
                                    break;
                                case 0x010A:
                                    DicConsole.WriteLine(Features.Prettify_010A(desc.Data));
                                    break;
                                case 0x010B:
                                    DicConsole.WriteLine(Features.Prettify_010B(desc.Data));
                                    break;
                                case 0x010C:
                                    DicConsole.WriteLine(Features.Prettify_010C(desc.Data));
                                    break;
                                case 0x010D:
                                    DicConsole.WriteLine(Features.Prettify_010D(desc.Data));
                                    break;
                                case 0x010E:
                                    DicConsole.WriteLine(Features.Prettify_010E(desc.Data));
                                    break;
                                case 0x0110:
                                    DicConsole.WriteLine(Features.Prettify_0110(desc.Data));
                                    break;
                                case 0x0113:
                                    DicConsole.WriteLine(Features.Prettify_0113(desc.Data));
                                    break;
                                case 0x0142:
                                    DicConsole.WriteLine(Features.Prettify_0142(desc.Data));
                                    break;
                                default:
                                    DicConsole.WriteLine("Found unknown feature code {0:X4}h", desc.Code);
                                    break;
                            }
                        }
                    }
                    else
                        DicConsole.DebugWriteLine("Device-Info command",
                                                  "GET CONFIGURATION returned no feature descriptors");
                }

                if(devInfo.PlextorFeatures != null)
                {
                    if(devInfo.PlextorFeatures.Eeprom != null)
                    {
                        DataFile.WriteTo("Device-Info command", outputPrefix, "_plextor_eeprom.bin",
                                         "PLEXTOR READ EEPROM", devInfo.PlextorFeatures.Eeprom);

                        DicConsole.WriteLine("Drive has loaded a total of {0} discs", devInfo.PlextorFeatures.Discs);
                        DicConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds reading CDs",
                                             devInfo.PlextorFeatures.CdReadTime      / 3600,
                                             devInfo.PlextorFeatures.CdReadTime / 60 % 60,
                                             devInfo.PlextorFeatures.CdReadTime      % 60);
                        DicConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds writing CDs",
                                             devInfo.PlextorFeatures.CdWriteTime      / 3600,
                                             devInfo.PlextorFeatures.CdWriteTime / 60 % 60,
                                             devInfo.PlextorFeatures.CdWriteTime      % 60);
                        if(devInfo.PlextorFeatures.IsDvd)
                        {
                            DicConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds reading DVDs",
                                                 devInfo.PlextorFeatures.DvdReadTime      / 3600,
                                                 devInfo.PlextorFeatures.DvdReadTime / 60 % 60,
                                                 devInfo.PlextorFeatures.DvdReadTime      % 60);
                            DicConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds writing DVDs",
                                                 devInfo.PlextorFeatures.DvdWriteTime      / 3600,
                                                 devInfo.PlextorFeatures.DvdWriteTime / 60 % 60,
                                                 devInfo.PlextorFeatures.DvdWriteTime      % 60);
                        }
                    }

                    if(devInfo.PlextorFeatures.PoweRec)
                    {
                        DicConsole.Write("Drive supports PoweRec");

                        if(devInfo.PlextorFeatures.PoweRecEnabled)
                        {
                            DicConsole.Write(", has it enabled");

                            if(devInfo.PlextorFeatures.PoweRecRecommendedSpeed > 0)
                                DicConsole.WriteLine(" and recommends {0} Kb/sec.",
                                                     devInfo.PlextorFeatures.PoweRecRecommendedSpeed);
                            else DicConsole.WriteLine(".");

                            if(devInfo.PlextorFeatures.PoweRecSelected > 0)
                                DicConsole
                                   .WriteLine("Selected PoweRec speed for currently inserted media is {0} Kb/sec ({1}x)",
                                              devInfo.PlextorFeatures.PoweRecSelected,
                                              devInfo.PlextorFeatures.PoweRecSelected / 177);
                            if(devInfo.PlextorFeatures.PoweRecMax > 0)
                                DicConsole
                                   .WriteLine("Maximum PoweRec speed for currently inserted media is {0} Kb/sec ({1}x)",
                                              devInfo.PlextorFeatures.PoweRecMax,
                                              devInfo.PlextorFeatures.PoweRecMax / 177);
                            if(devInfo.PlextorFeatures.PoweRecLast > 0)
                                DicConsole.WriteLine("Last used PoweRec was {0} Kb/sec ({1}x)",
                                                     devInfo.PlextorFeatures.PoweRecLast,
                                                     devInfo.PlextorFeatures.PoweRecLast / 177);
                        }
                        else
                        {
                            DicConsole.WriteLine(".");
                            DicConsole.WriteLine("PoweRec is disabled");
                        }
                    }

                    if(devInfo.PlextorFeatures.SilentMode)
                    {
                        DicConsole.WriteLine("Drive supports Plextor SilentMode");
                        if(devInfo.PlextorFeatures.SilentModeEnabled)
                        {
                            DicConsole.WriteLine("Plextor SilentMode is enabled:");
                            DicConsole.WriteLine(devInfo.PlextorFeatures.AccessTimeLimit == 2
                                                     ? "\tAccess time is slow"
                                                     : "\tAccess time is fast");

                            if(devInfo.PlextorFeatures.CdReadSpeedLimit > 0)
                                DicConsole.WriteLine("\tCD read speed limited to {0}x",
                                                     devInfo.PlextorFeatures.CdReadSpeedLimit);
                            if(devInfo.PlextorFeatures.DvdReadSpeedLimit > 0 && devInfo.PlextorFeatures.IsDvd)
                                DicConsole.WriteLine("\tDVD read speed limited to {0}x",
                                                     devInfo.PlextorFeatures.DvdReadSpeedLimit);
                            if(devInfo.PlextorFeatures.CdWriteSpeedLimit > 0)
                                DicConsole.WriteLine("\tCD write speed limited to {0}x",
                                                     devInfo.PlextorFeatures.CdWriteSpeedLimit);
                        }
                    }

                    if(devInfo.PlextorFeatures.GigaRec) DicConsole.WriteLine("Drive supports Plextor GigaRec");
                    if(devInfo.PlextorFeatures.SecuRec) DicConsole.WriteLine("Drive supports Plextor SecuRec");
                    if(devInfo.PlextorFeatures.SpeedRead)
                    {
                        DicConsole.Write("Drive supports Plextor SpeedRead");
                        if(devInfo.PlextorFeatures.SpeedReadEnabled) DicConsole.WriteLine("and has it enabled");
                        else DicConsole.WriteLine();
                    }

                    if(devInfo.PlextorFeatures.Hiding)
                    {
                        DicConsole.WriteLine("Drive supports hiding CD-Rs and forcing single session");

                        if(devInfo.PlextorFeatures.HidesRecordables)
                            DicConsole.WriteLine("Drive currently hides CD-Rs");
                        if(devInfo.PlextorFeatures.HidesSessions)
                            DicConsole.WriteLine("Drive currently forces single session");
                    }

                    if(devInfo.PlextorFeatures.VariRec) DicConsole.WriteLine("Drive supports Plextor VariRec");

                    if(devInfo.PlextorFeatures.IsDvd)
                    {
                        if(devInfo.PlextorFeatures.VariRecDvd)
                            DicConsole.WriteLine("Drive supports Plextor VariRec for DVDs");
                        if(devInfo.PlextorFeatures.BitSetting)
                            DicConsole.WriteLine("Drive supports bitsetting DVD+R book type");
                        if(devInfo.PlextorFeatures.BitSettingDl)
                            DicConsole.WriteLine("Drive supports bitsetting DVD+R DL book type");
                        if(devInfo.PlextorFeatures.DvdPlusWriteTest)
                            DicConsole.WriteLine("Drive supports test writing DVD+");
                    }
                }

                if(devInfo.ScsiInquiry.Value.KreonPresent)
                {
                    DicConsole.WriteLine("Drive has kreon firmware:");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse))
                        DicConsole.WriteLine("\tCan do challenge/response with Xbox discs");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs))
                        DicConsole.WriteLine("\tCan read and descrypt SS from Xbox discs");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock))
                        DicConsole.WriteLine("\tCan set xtreme unlock state with Xbox discs");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock))
                        DicConsole.WriteLine("\tCan set wxripper unlock state with Xbox discs");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse360))
                        DicConsole.WriteLine("\tCan do challenge/response with Xbox 360 discs");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs360))
                        DicConsole.WriteLine("\tCan read and descrypt SS from Xbox 360 discs");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock360))
                        DicConsole.WriteLine("\tCan set xtreme unlock state with Xbox 360 discs");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock360))
                        DicConsole.WriteLine("\tCan set wxripper unlock state with Xbox 360 discs");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.Lock))
                        DicConsole.WriteLine("\tCan set locked state");
                    if(devInfo.KreonFeatures.HasFlag(KreonFeatures.ErrorSkipping))
                        DicConsole.WriteLine("\tCan skip read errors");
                }

                if(devInfo.BlockLimits != null)
                {
                    DataFile.WriteTo("Device-Info command", outputPrefix, "_ssc_readblocklimits.bin",
                                     "SSC READ BLOCK LIMITS", devInfo.BlockLimits);
                    DicConsole.WriteLine("Block limits for device:");
                    DicConsole.WriteLine(BlockLimits.Prettify(devInfo.BlockLimits));
                }

                if(devInfo.DensitySupport != null)
                {
                    DataFile.WriteTo("Device-Info command", outputPrefix, "_ssc_reportdensitysupport.bin",
                                     "SSC REPORT DENSITY SUPPORT", devInfo.DensitySupport);
                    if(devInfo.DensitySupportHeader.HasValue)
                    {
                        DicConsole.WriteLine("Densities supported by device:");
                        DicConsole.WriteLine(DensitySupport.PrettifyDensity(devInfo.DensitySupportHeader));
                    }
                }

                if(devInfo.MediumDensitySupport != null)
                {
                    DataFile.WriteTo("Device-Info command", outputPrefix, "_ssc_reportdensitysupport_medium.bin",
                                     "SSC REPORT DENSITY SUPPORT (MEDIUM)", devInfo.MediumDensitySupport);
                    if(devInfo.MediaTypeSupportHeader.HasValue)
                    {
                        DicConsole.WriteLine("Medium types supported by device:");
                        DicConsole.WriteLine(DensitySupport.PrettifyMediumType(devInfo.MediaTypeSupportHeader));
                    }

                    DicConsole.WriteLine(DensitySupport.PrettifyMediumType(devInfo.MediumDensitySupport));
                }
            }

            switch(dev.Type)
            {
                case DeviceType.MMC:
                {
                    bool noInfo = true;

                    if(devInfo.CID != null)
                    {
                        noInfo = false;
                        DataFile.WriteTo("Device-Info command", outputPrefix, "_mmc_cid.bin", "MMC CID", devInfo.CID);
                        DicConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyCID(devInfo.CID));
                    }

                    if(devInfo.CSD != null)
                    {
                        noInfo = false;
                        DataFile.WriteTo("Device-Info command", outputPrefix, "_mmc_csd.bin", "MMC CSD", devInfo.CSD);
                        DicConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyCSD(devInfo.CSD));
                    }

                    if(devInfo.OCR != null)
                    {
                        noInfo = false;
                        DataFile.WriteTo("Device-Info command", outputPrefix, "_mmc_ocr.bin", "MMC OCR", devInfo.OCR);
                        DicConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyOCR(devInfo.OCR));
                    }

                    if(devInfo.ExtendedCSD != null)
                    {
                        noInfo = false;
                        DataFile.WriteTo("Device-Info command", outputPrefix, "_mmc_ecsd.bin", "MMC Extended CSD",
                                         devInfo.ExtendedCSD);
                        DicConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyExtendedCSD(devInfo.ExtendedCSD));
                    }

                    if(noInfo) DicConsole.WriteLine("Could not get any kind of information from the device !!!");
                }
                    break;
                case DeviceType.SecureDigital:
                {
                    bool noInfo = true;

                    if(devInfo.CID != null)
                    {
                        noInfo = false;
                        DataFile.WriteTo("Device-Info command", outputPrefix, "_sd_cid.bin", "SecureDigital CID",
                                         devInfo.CID);
                        DicConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyCID(devInfo.CID));
                    }

                    if(devInfo.CSD != null)
                    {
                        noInfo = false;
                        DataFile.WriteTo("Device-Info command", outputPrefix, "_sd_csd.bin", "SecureDigital CSD",
                                         devInfo.CSD);
                        DicConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyCSD(devInfo.CSD));
                    }

                    if(devInfo.OCR != null)
                    {
                        noInfo = false;
                        DataFile.WriteTo("Device-Info command", outputPrefix, "_sd_ocr.bin", "SecureDigital OCR",
                                         devInfo.OCR);
                        DicConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyOCR(devInfo.OCR));
                    }

                    if(devInfo.SCR != null)
                    {
                        noInfo = false;
                        DataFile.WriteTo("Device-Info command", outputPrefix, "_sd_scr.bin", "SecureDigital SCR",
                                         devInfo.SCR);
                        DicConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifySCR(devInfo.SCR));
                    }

                    if(noInfo) DicConsole.WriteLine("Could not get any kind of information from the device !!!");
                }
                    break;
            }

            Statistics.AddCommand("device-info");

            dev.Close();

            return 0;
        }
    }
}