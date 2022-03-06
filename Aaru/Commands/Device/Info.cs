// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'info' command.
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
// Copyright © 2011-2022 Natalia Portillo
// Copyright © 2021-2022 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core;
using Aaru.Database;
using Aaru.Database.Models;
using Aaru.Decoders.DVD;
using Aaru.Decoders.PCMCIA;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Devices;
using Aaru.Helpers;
using Spectre.Console;
using Command = System.CommandLine.Command;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;
using Inquiry = Aaru.Decoders.SCSI.Inquiry;
using Tuple = Aaru.Decoders.PCMCIA.Tuple;

namespace Aaru.Commands.Device;

internal sealed class DeviceInfoCommand : Command
{
    public DeviceInfoCommand() : base("info", "Gets information about a device.")
    {
        Add(new Option(new[]
            {
                "--output-prefix", "-w"
            }, "Prefix for saving binary information from device.")
            {
                Argument = new Argument<string>(() => null),
                Required = false
            });

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = "Device path",
            Name        = "device-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool debug, bool verbose, string devicePath, string outputPrefix)
    {
        MainClass.PrintCopyright();

        if(debug)
        {
            IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(System.Console.Error)
            });

            AaruConsole.DebugWriteLineEvent += (format, objects) =>
            {
                if(objects is null)
                    stderrConsole.MarkupLine(format);
                else
                    stderrConsole.MarkupLine(format, objects);
            };
        }

        if(verbose)
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };

        Statistics.AddCommand("device-info");

        AaruConsole.DebugWriteLine("Device-Info command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("Device-Info command", "--device={0}", devicePath);
        AaruConsole.DebugWriteLine("Device-Info command", "--output-prefix={0}", outputPrefix);
        AaruConsole.DebugWriteLine("Device-Info command", "--verbose={0}", verbose);

        if(devicePath.Length == 2   &&
           devicePath[1]     == ':' &&
           devicePath[0]     != '/' &&
           char.IsLetter(devicePath[0]))
            devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

        Devices.Device dev;

        try
        {
            dev = new Devices.Device(devicePath);

            if(dev.IsRemote)
                Statistics.AddRemote(dev.RemoteApplication, dev.RemoteVersion, dev.RemoteOperatingSystem,
                                     dev.RemoteOperatingSystemVersion, dev.RemoteArchitecture);

            if(dev.Error)
            {
                AaruConsole.ErrorWriteLine(Error.Print(dev.LastError));

                return (int)ErrorNumber.CannotOpenDevice;
            }
        }
        catch(DeviceException e)
        {
            AaruConsole.ErrorWriteLine(e.Message);

            return (int)ErrorNumber.CannotOpenDevice;
        }

        Statistics.AddDevice(dev);

        Table table;

        if(dev.IsUsb)
        {
            table = new Table
            {
                Title = new TableTitle("[bold]USB device[/]")
            };

            table.HideHeaders();
            table.AddColumn("");
            table.AddColumn("");
            table.Columns[0].RightAligned();

            if(dev.UsbDescriptors != null)
                table.AddRow("Descriptor size", $"{dev.UsbDescriptors.Length}");

            table.AddRow("Vendor ID", $"{dev.UsbVendorId:X4}");
            table.AddRow("Product ID", $"{dev.UsbProductId:X4}");
            table.AddRow("Manufacturer", $"{Markup.Escape(dev.UsbManufacturerString ?? "")}");
            table.AddRow("Product", $"{Markup.Escape(dev.UsbProductString           ?? "")}");
            table.AddRow("Serial number", $"{Markup.Escape(dev.UsbSerialString      ?? "")}");

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
        }

        if(dev.IsFireWire)
        {
            table = new Table
            {
                Title = new TableTitle("[bold]FireWire device[/]")
            };

            table.HideHeaders();
            table.AddColumn("");
            table.AddColumn("");
            table.Columns[0].RightAligned();

            table.AddRow("Vendor ID", $"{dev.FireWireVendor:X6}");
            table.AddRow("Model ID", $"{dev.FireWireModel:X6}");
            table.AddRow("Vendor", $"{Markup.Escape(dev.FireWireVendorName ?? "")}");
            table.AddRow("Model", $"{Markup.Escape(dev.FireWireModelName   ?? "")}");
            table.AddRow("GUID", $"{dev.FireWireGuid:X16}");

            AnsiConsole.Render(table);
            AaruConsole.WriteLine();
        }

        if(dev.IsPcmcia)
        {
            AaruConsole.WriteLine("[bold]PCMCIA device[/]");
            AaruConsole.WriteLine("PCMCIA CIS is {0} bytes", dev.Cis.Length);
            Tuple[] tuples = CIS.GetTuples(dev.Cis);

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
                            AaruConsole.DebugWriteLine("Device-Info command", "Found undecoded tuple ID {0}",
                                                       tuple.Code);

                            break;
                        default:
                            AaruConsole.DebugWriteLine("Device-Info command", "Found unknown tuple ID 0x{0:X2}",
                                                       (byte)tuple.Code);

                            break;
                    }
            else
                AaruConsole.DebugWriteLine("Device-Info command", "Could not get tuples");
        }

        var devInfo = new DeviceInfo(dev);

        if(devInfo.AtaIdentify != null)
        {
            DataFile.WriteTo("Device-Info command", outputPrefix, "_ata_identify.bin", "ATA IDENTIFY",
                             devInfo.AtaIdentify);

            Identify.IdentifyDevice? decodedIdentify = Identify.Decode(devInfo.AtaIdentify);
            AaruConsole.WriteLine(Decoders.ATA.Identify.Prettify(decodedIdentify));

            if(devInfo.AtaMcptError.HasValue)
            {
                AaruConsole.WriteLine("Device supports the Media Card Pass Through Command Set");

                switch(devInfo.AtaMcptError.Value.DeviceHead & 0x7)
                {
                    case 0:
                        AaruConsole.WriteLine("Device reports incorrect media card type");

                        break;
                    case 1:
                        AaruConsole.WriteLine("Device contains a Secure Digital card");

                        break;
                    case 2:
                        AaruConsole.WriteLine("Device contains a MultiMediaCard ");

                        break;
                    case 3:
                        AaruConsole.WriteLine("Device contains a Secure Digital I/O card");

                        break;
                    case 4:
                        AaruConsole.WriteLine("Device contains a Smart Media card");

                        break;
                    default:
                        AaruConsole.WriteLine("Device contains unknown media card type {0}",
                                              devInfo.AtaMcptError.Value.DeviceHead & 0x07);

                        break;
                }

                if((devInfo.AtaMcptError.Value.DeviceHead & 0x08) == 0x08)
                    AaruConsole.WriteLine("Media card is write protected");

                ushort specificData = (ushort)((devInfo.AtaMcptError.Value.CylinderHigh * 0x100) +
                                               devInfo.AtaMcptError.Value.CylinderLow);

                if(specificData != 0)
                    AaruConsole.WriteLine("Card specific data: 0x{0:X4}", specificData);
            }

            if(decodedIdentify.HasValue)
            {
                Identify.IdentifyDevice ataid = decodedIdentify.Value;

                ulong blocks;

                if(ataid.CurrentCylinders       > 0 &&
                   ataid.CurrentHeads           > 0 &&
                   ataid.CurrentSectorsPerTrack > 0)
                {
                    blocks =
                        (ulong)Math.Max(ataid.CurrentCylinders * ataid.CurrentHeads * ataid.CurrentSectorsPerTrack,
                                        ataid.CurrentSectors);
                }
                else
                {
                    blocks = (ulong)(ataid.Cylinders * ataid.Heads * ataid.SectorsPerTrack);
                }

                if(ataid.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
                    blocks = ataid.LBASectors;

                if(ataid.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48))
                    blocks = ataid.LBA48Sectors;

                bool removable = ataid.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.Removable);

                MediaType mediaType = MediaTypeFromDevice.GetFromAta(dev.Manufacturer, dev.Model, removable,
                                                                     dev.IsCompactFlash, dev.IsPcmcia, blocks);

                AaruConsole.WriteLine(removable ? "Media identified as {0}" : "Device identified as {0}",
                                      mediaType);

                Statistics.AddMedia(mediaType, true);
            }
        }

        if(devInfo.AtapiIdentify != null)
        {
            DataFile.WriteTo("Device-Info command", outputPrefix, "_atapi_identify.bin", "ATAPI IDENTIFY",
                             devInfo.AtapiIdentify);

            AaruConsole.WriteLine(Decoders.ATA.Identify.Prettify(devInfo.AtapiIdentify));
        }

        if(devInfo.ScsiInquiry != null)
        {
            if(dev.Type != DeviceType.ATAPI)
                AaruConsole.WriteLine("[bold]SCSI device[/]");

            DataFile.WriteTo("Device-Info command", outputPrefix, "_scsi_inquiry.bin", "SCSI INQUIRY",
                             devInfo.ScsiInquiryData);

            AaruConsole.WriteLine(Inquiry.Prettify(devInfo.ScsiInquiry));

            if(devInfo.ScsiEvpdPages != null)
                foreach(KeyValuePair<byte, byte[]> page in devInfo.ScsiEvpdPages)
                    if(page.Key >= 0x01 &&
                       page.Key <= 0x7F)
                    {
                        AaruConsole.WriteLine("ASCII Page {0:X2}h: {1}", page.Key,
                                              EVPD.DecodeASCIIPage(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, page.Value);
                    }
                    else if(page.Key == 0x80)
                    {
                        AaruConsole.WriteLine("Unit Serial Number: {0}", EVPD.DecodePage80(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0x81)
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_81(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0x82)
                    {
                        AaruConsole.WriteLine("ASCII implemented operating definitions: {0}",
                                              EVPD.DecodePage82(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0x83)
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_83(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0x84)
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_84(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0x85)
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_85(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0x86)
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_86(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0x89)
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_89(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xB0)
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_B0(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xB1)
                    {
                        AaruConsole.WriteLine("Manufacturer-assigned Serial Number: {0}",
                                              EVPD.DecodePageB1(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xB2)
                    {
                        AaruConsole.WriteLine("TapeAlert Supported Flags Bitmap: 0x{0:X16}",
                                              EVPD.DecodePageB2(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xB3)
                    {
                        AaruConsole.WriteLine("Automation Device Serial Number: {0}",
                                              EVPD.DecodePageB3(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xB4)
                    {
                        AaruConsole.WriteLine("Data Transfer Device Element Address: 0x{0}",
                                              EVPD.DecodePageB4(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xC0 &&
                            StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                           ToLowerInvariant().Trim() == "quantum")
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_Quantum(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xC0 &&
                            StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                           ToLowerInvariant().Trim() == "seagate")
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_Seagate(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xC0 &&
                            StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                           ToLowerInvariant().Trim() == "ibm")
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_IBM(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xC1 &&
                            StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                           ToLowerInvariant().Trim() == "ibm")
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C1_IBM(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if((page.Key == 0xC0 || page.Key == 0xC1) &&
                            StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                           ToLowerInvariant().Trim() == "certance")
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_C1_Certance(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if((page.Key == 0xC2 || page.Key == 0xC3 || page.Key == 0xC4 || page.Key == 0xC5 ||
                             page.Key == 0xC6) &&
                            StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                           ToLowerInvariant().Trim() == "certance")
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C2_C3_C4_C5_C6_Certance(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if((page.Key == 0xC0 || page.Key == 0xC1 || page.Key == 0xC2 || page.Key == 0xC3 ||
                             page.Key == 0xC4 || page.Key == 0xC5) &&
                            StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                           ToLowerInvariant().Trim() == "hp")
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_to_C5_HP(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else if(page.Key == 0xDF &&
                            StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                           ToLowerInvariant().Trim() == "certance")
                    {
                        AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_DF_Certance(page.Value));

                        DataFile.WriteTo("Device-Info command", outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                         $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);
                    }
                    else
                    {
                        if(page.Key == 0x00)
                            continue;

                        AaruConsole.DebugWriteLine("Device-Info command", "Found undecoded SCSI VPD page 0x{0:X2}",
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

                AaruConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION length is {0} bytes",
                                           ftr.DataLength);

                AaruConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION current profile is {0:X4}h",
                                           ftr.CurrentProfile);

                if(ftr.Descriptors != null)
                {
                    AaruConsole.WriteLine("[bold]SCSI MMC GET CONFIGURATION Features:[/]");

                    foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                    {
                        AaruConsole.DebugWriteLine("Device-Info command", "Feature {0:X4}h", desc.Code);

                        switch(desc.Code)
                        {
                            case 0x0000:
                                AaruConsole.WriteLine(Features.Prettify_0000(desc.Data));

                                break;
                            case 0x0001:
                                AaruConsole.WriteLine(Features.Prettify_0001(desc.Data));

                                break;
                            case 0x0002:
                                AaruConsole.WriteLine(Features.Prettify_0002(desc.Data));

                                break;
                            case 0x0003:
                                AaruConsole.WriteLine(Features.Prettify_0003(desc.Data));

                                break;
                            case 0x0004:
                                AaruConsole.WriteLine(Features.Prettify_0004(desc.Data));

                                break;
                            case 0x0010:
                                AaruConsole.WriteLine(Features.Prettify_0010(desc.Data));

                                break;
                            case 0x001D:
                                AaruConsole.WriteLine(Features.Prettify_001D(desc.Data));

                                break;
                            case 0x001E:
                                AaruConsole.WriteLine(Features.Prettify_001E(desc.Data));

                                break;
                            case 0x001F:
                                AaruConsole.WriteLine(Features.Prettify_001F(desc.Data));

                                break;
                            case 0x0020:
                                AaruConsole.WriteLine(Features.Prettify_0020(desc.Data));

                                break;
                            case 0x0021:
                                AaruConsole.WriteLine(Features.Prettify_0021(desc.Data));

                                break;
                            case 0x0022:
                                AaruConsole.WriteLine(Features.Prettify_0022(desc.Data));

                                break;
                            case 0x0023:
                                AaruConsole.WriteLine(Features.Prettify_0023(desc.Data));

                                break;
                            case 0x0024:
                                AaruConsole.WriteLine(Features.Prettify_0024(desc.Data));

                                break;
                            case 0x0025:
                                AaruConsole.WriteLine(Features.Prettify_0025(desc.Data));

                                break;
                            case 0x0026:
                                AaruConsole.WriteLine(Features.Prettify_0026(desc.Data));

                                break;
                            case 0x0027:
                                AaruConsole.WriteLine(Features.Prettify_0027(desc.Data));

                                break;
                            case 0x0028:
                                AaruConsole.WriteLine(Features.Prettify_0028(desc.Data));

                                break;
                            case 0x0029:
                                AaruConsole.WriteLine(Features.Prettify_0029(desc.Data));

                                break;
                            case 0x002A:
                                AaruConsole.WriteLine(Features.Prettify_002A(desc.Data));

                                break;
                            case 0x002B:
                                AaruConsole.WriteLine(Features.Prettify_002B(desc.Data));

                                break;
                            case 0x002C:
                                AaruConsole.WriteLine(Features.Prettify_002C(desc.Data));

                                break;
                            case 0x002D:
                                AaruConsole.WriteLine(Features.Prettify_002D(desc.Data));

                                break;
                            case 0x002E:
                                AaruConsole.WriteLine(Features.Prettify_002E(desc.Data));

                                break;
                            case 0x002F:
                                AaruConsole.WriteLine(Features.Prettify_002F(desc.Data));

                                break;
                            case 0x0030:
                                AaruConsole.WriteLine(Features.Prettify_0030(desc.Data));

                                break;
                            case 0x0031:
                                AaruConsole.WriteLine(Features.Prettify_0031(desc.Data));

                                break;
                            case 0x0032:
                                AaruConsole.WriteLine(Features.Prettify_0032(desc.Data));

                                break;
                            case 0x0033:
                                AaruConsole.WriteLine(Features.Prettify_0033(desc.Data));

                                break;
                            case 0x0035:
                                AaruConsole.WriteLine(Features.Prettify_0035(desc.Data));

                                break;
                            case 0x0037:
                                AaruConsole.WriteLine(Features.Prettify_0037(desc.Data));

                                break;
                            case 0x0038:
                                AaruConsole.WriteLine(Features.Prettify_0038(desc.Data));

                                break;
                            case 0x003A:
                                AaruConsole.WriteLine(Features.Prettify_003A(desc.Data));

                                break;
                            case 0x003B:
                                AaruConsole.WriteLine(Features.Prettify_003B(desc.Data));

                                break;
                            case 0x0040:
                                AaruConsole.WriteLine(Features.Prettify_0040(desc.Data));

                                break;
                            case 0x0041:
                                AaruConsole.WriteLine(Features.Prettify_0041(desc.Data));

                                break;
                            case 0x0042:
                                AaruConsole.WriteLine(Features.Prettify_0042(desc.Data));

                                break;
                            case 0x0050:
                                AaruConsole.WriteLine(Features.Prettify_0050(desc.Data));

                                break;
                            case 0x0051:
                                AaruConsole.WriteLine(Features.Prettify_0051(desc.Data));

                                break;
                            case 0x0080:
                                AaruConsole.WriteLine(Features.Prettify_0080(desc.Data));

                                break;
                            case 0x0100:
                                AaruConsole.WriteLine(Features.Prettify_0100(desc.Data));

                                break;
                            case 0x0101:
                                AaruConsole.WriteLine(Features.Prettify_0101(desc.Data));

                                break;
                            case 0x0102:
                                AaruConsole.WriteLine(Features.Prettify_0102(desc.Data));

                                break;
                            case 0x0103:
                                AaruConsole.WriteLine(Features.Prettify_0103(desc.Data));

                                break;
                            case 0x0104:
                                AaruConsole.WriteLine(Features.Prettify_0104(desc.Data));

                                break;
                            case 0x0105:
                                AaruConsole.WriteLine(Features.Prettify_0105(desc.Data));

                                break;
                            case 0x0106:
                                AaruConsole.WriteLine(Features.Prettify_0106(desc.Data));

                                break;
                            case 0x0107:
                                AaruConsole.WriteLine(Features.Prettify_0107(desc.Data));

                                break;
                            case 0x0108:
                                AaruConsole.WriteLine(Features.Prettify_0108(desc.Data));

                                break;
                            case 0x0109:
                                AaruConsole.WriteLine(Features.Prettify_0109(desc.Data));

                                break;
                            case 0x010A:
                                AaruConsole.WriteLine(Features.Prettify_010A(desc.Data));

                                break;
                            case 0x010B:
                                AaruConsole.WriteLine(Features.Prettify_010B(desc.Data));

                                break;
                            case 0x010C:
                                AaruConsole.WriteLine(Features.Prettify_010C(desc.Data));

                                break;
                            case 0x010D:
                                AaruConsole.WriteLine(Features.Prettify_010D(desc.Data));

                                break;
                            case 0x010E:
                                AaruConsole.WriteLine(Features.Prettify_010E(desc.Data));

                                break;
                            case 0x0110:
                                AaruConsole.WriteLine(Features.Prettify_0110(desc.Data));

                                break;
                            case 0x0113:
                                AaruConsole.WriteLine(Features.Prettify_0113(desc.Data));

                                break;
                            case 0x0142:
                                AaruConsole.WriteLine(Features.Prettify_0142(desc.Data));

                                break;
                            default:
                                AaruConsole.WriteLine("Found unknown feature code {0:X4}h", desc.Code);

                                break;
                        }
                    }
                }
                else
                {
                    AaruConsole.DebugWriteLine("Device-Info command",
                                               "GET CONFIGURATION returned no feature descriptors");
                }
            }

            if(devInfo.RPC != null)
                AaruConsole.WriteLine(CSS_CPRM.PrettifyRegionalPlaybackControlState(devInfo.RPC));

            if(devInfo.PlextorFeatures?.Eeprom != null)
            {
                DataFile.WriteTo("Device-Info command", outputPrefix, "_plextor_eeprom.bin", "PLEXTOR READ EEPROM",
                                 devInfo.PlextorFeatures.Eeprom);

                AaruConsole.WriteLine("Drive has loaded a total of {0} discs", devInfo.PlextorFeatures.Discs);

                AaruConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds reading CDs",
                                      devInfo.PlextorFeatures.CdReadTime      / 3600,
                                      devInfo.PlextorFeatures.CdReadTime / 60 % 60,
                                      devInfo.PlextorFeatures.CdReadTime      % 60);

                AaruConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds writing CDs",
                                      devInfo.PlextorFeatures.CdWriteTime      / 3600,
                                      devInfo.PlextorFeatures.CdWriteTime / 60 % 60,
                                      devInfo.PlextorFeatures.CdWriteTime      % 60);

                if(devInfo.PlextorFeatures.IsDvd)
                {
                    AaruConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds reading DVDs",
                                          devInfo.PlextorFeatures.DvdReadTime      / 3600,
                                          devInfo.PlextorFeatures.DvdReadTime / 60 % 60,
                                          devInfo.PlextorFeatures.DvdReadTime      % 60);

                    AaruConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds writing DVDs",
                                          devInfo.PlextorFeatures.DvdWriteTime      / 3600,
                                          devInfo.PlextorFeatures.DvdWriteTime / 60 % 60,
                                          devInfo.PlextorFeatures.DvdWriteTime      % 60);
                }
            }

            if(devInfo.PlextorFeatures?.PoweRec == true)
            {
                AaruConsole.Write("Drive supports PoweRec");

                if(devInfo.PlextorFeatures.PoweRecEnabled)
                {
                    AaruConsole.Write(", has it enabled");

                    if(devInfo.PlextorFeatures.PoweRecRecommendedSpeed > 0)
                        AaruConsole.WriteLine(" and recommends {0} Kb/sec.",
                                              devInfo.PlextorFeatures.PoweRecRecommendedSpeed);
                    else
                        AaruConsole.WriteLine(".");

                    if(devInfo.PlextorFeatures.PoweRecSelected > 0)
                        AaruConsole.
                            WriteLine("Selected PoweRec speed for currently inserted media is {0} Kb/sec ({1}x)",
                                      devInfo.PlextorFeatures.PoweRecSelected,
                                      devInfo.PlextorFeatures.PoweRecSelected / 177);

                    if(devInfo.PlextorFeatures.PoweRecMax > 0)
                        AaruConsole.
                            WriteLine("Maximum PoweRec speed for currently inserted media is {0} Kb/sec ({1}x)",
                                      devInfo.PlextorFeatures.PoweRecMax, devInfo.PlextorFeatures.PoweRecMax / 177);

                    if(devInfo.PlextorFeatures.PoweRecLast > 0)
                        AaruConsole.WriteLine("Last used PoweRec was {0} Kb/sec ({1}x)",
                                              devInfo.PlextorFeatures.PoweRecLast,
                                              devInfo.PlextorFeatures.PoweRecLast / 177);
                }
                else
                {
                    AaruConsole.WriteLine(".");
                    AaruConsole.WriteLine("PoweRec is disabled");
                }
            }

            if(devInfo.PlextorFeatures?.SilentMode == true)
            {
                AaruConsole.WriteLine("Drive supports Plextor SilentMode");

                if(devInfo.PlextorFeatures.SilentModeEnabled)
                {
                    AaruConsole.WriteLine("Plextor SilentMode is enabled:");

                    AaruConsole.WriteLine(devInfo.PlextorFeatures.AccessTimeLimit == 2 ? "\tAccess time is slow"
                                              : "\tAccess time is fast");

                    if(devInfo.PlextorFeatures.CdReadSpeedLimit > 0)
                        AaruConsole.WriteLine("\tCD read speed limited to {0}x",
                                              devInfo.PlextorFeatures.CdReadSpeedLimit);

                    if(devInfo.PlextorFeatures.DvdReadSpeedLimit > 0 &&
                       devInfo.PlextorFeatures.IsDvd)
                        AaruConsole.WriteLine("\tDVD read speed limited to {0}x",
                                              devInfo.PlextorFeatures.DvdReadSpeedLimit);

                    if(devInfo.PlextorFeatures.CdWriteSpeedLimit > 0)
                        AaruConsole.WriteLine("\tCD write speed limited to {0}x",
                                              devInfo.PlextorFeatures.CdWriteSpeedLimit);
                }
            }

            if(devInfo.PlextorFeatures?.GigaRec == true)
                AaruConsole.WriteLine("Drive supports Plextor GigaRec");

            if(devInfo.PlextorFeatures?.SecuRec == true)
                AaruConsole.WriteLine("Drive supports Plextor SecuRec");

            if(devInfo.PlextorFeatures?.SpeedRead == true)
            {
                AaruConsole.Write("Drive supports Plextor SpeedRead");

                if(devInfo.PlextorFeatures.SpeedReadEnabled)
                    AaruConsole.WriteLine("and has it enabled");
                else
                    AaruConsole.WriteLine();
            }

            if(devInfo.PlextorFeatures?.Hiding == true)
            {
                AaruConsole.WriteLine("Drive supports hiding CD-Rs and forcing single session");

                if(devInfo.PlextorFeatures.HidesRecordables)
                    AaruConsole.WriteLine("Drive currently hides CD-Rs");

                if(devInfo.PlextorFeatures.HidesSessions)
                    AaruConsole.WriteLine("Drive currently forces single session");
            }

            if(devInfo.PlextorFeatures?.VariRec == true)
                AaruConsole.WriteLine("Drive supports Plextor VariRec");

            if(devInfo.PlextorFeatures?.IsDvd == true)
            {
                if(devInfo.PlextorFeatures.VariRecDvd)
                    AaruConsole.WriteLine("Drive supports Plextor VariRec for DVDs");

                if(devInfo.PlextorFeatures.BitSetting)
                    AaruConsole.WriteLine("Drive supports bitsetting DVD+R book type");

                if(devInfo.PlextorFeatures.BitSettingDl)
                    AaruConsole.WriteLine("Drive supports bitsetting DVD+R DL book type");

                if(devInfo.PlextorFeatures.DvdPlusWriteTest)
                    AaruConsole.WriteLine("Drive supports test writing DVD+");
            }

            if(devInfo.ScsiInquiry.Value.KreonPresent)
            {
                AaruConsole.WriteLine("[bold]Drive has kreon firmware:[/]");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse))
                    AaruConsole.WriteLine("\tCan do challenge/response with Xbox discs");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs))
                    AaruConsole.WriteLine("\tCan read and decrypt SS from Xbox discs");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock))
                    AaruConsole.WriteLine("\tCan set xtreme unlock state with Xbox discs");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock))
                    AaruConsole.WriteLine("\tCan set wxripper unlock state with Xbox discs");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse360))
                    AaruConsole.WriteLine("\tCan do challenge/response with Xbox 360 discs");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs360))
                    AaruConsole.WriteLine("\tCan read and decrypt SS from Xbox 360 discs");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock360))
                    AaruConsole.WriteLine("\tCan set xtreme unlock state with Xbox 360 discs");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock360))
                    AaruConsole.WriteLine("\tCan set wxripper unlock state with Xbox 360 discs");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.Lock))
                    AaruConsole.WriteLine("\tCan set locked state");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.ErrorSkipping))
                    AaruConsole.WriteLine("\tCan skip read errors");
            }

            if(devInfo.BlockLimits != null)
            {
                DataFile.WriteTo("Device-Info command", outputPrefix, "_ssc_readblocklimits.bin",
                                 "SSC READ BLOCK LIMITS", devInfo.BlockLimits);

                AaruConsole.WriteLine("Block limits for device:");
                AaruConsole.WriteLine(BlockLimits.Prettify(devInfo.BlockLimits));
            }

            if(devInfo.DensitySupport != null)
            {
                DataFile.WriteTo("Device-Info command", outputPrefix, "_ssc_reportdensitysupport.bin",
                                 "SSC REPORT DENSITY SUPPORT", devInfo.DensitySupport);

                if(devInfo.DensitySupportHeader.HasValue)
                {
                    AaruConsole.WriteLine("Densities supported by device:");
                    AaruConsole.WriteLine(DensitySupport.PrettifyDensity(devInfo.DensitySupportHeader));
                }
            }

            if(devInfo.MediumDensitySupport != null)
            {
                DataFile.WriteTo("Device-Info command", outputPrefix, "_ssc_reportdensitysupport_medium.bin",
                                 "SSC REPORT DENSITY SUPPORT (MEDIUM)", devInfo.MediumDensitySupport);

                if(devInfo.MediaTypeSupportHeader.HasValue)
                {
                    AaruConsole.WriteLine("Medium types supported by device:");
                    AaruConsole.WriteLine(DensitySupport.PrettifyMediumType(devInfo.MediaTypeSupportHeader));
                }

                AaruConsole.WriteLine(DensitySupport.PrettifyMediumType(devInfo.MediumDensitySupport));
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
                    AaruConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyCID(devInfo.CID));
                }

                if(devInfo.CSD != null)
                {
                    noInfo = false;
                    DataFile.WriteTo("Device-Info command", outputPrefix, "_mmc_csd.bin", "MMC CSD", devInfo.CSD);
                    AaruConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyCSD(devInfo.CSD));
                }

                if(devInfo.OCR != null)
                {
                    noInfo = false;
                    DataFile.WriteTo("Device-Info command", outputPrefix, "_mmc_ocr.bin", "MMC OCR", devInfo.OCR);
                    AaruConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyOCR(devInfo.OCR));
                }

                if(devInfo.ExtendedCSD != null)
                {
                    noInfo = false;

                    DataFile.WriteTo("Device-Info command", outputPrefix, "_mmc_ecsd.bin", "MMC Extended CSD",
                                     devInfo.ExtendedCSD);

                    AaruConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyExtendedCSD(devInfo.ExtendedCSD));
                }

                if(noInfo)
                    AaruConsole.WriteLine("Could not get any kind of information from the device !!!");
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

                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyCID(devInfo.CID));
                }

                if(devInfo.CSD != null)
                {
                    noInfo = false;

                    DataFile.WriteTo("Device-Info command", outputPrefix, "_sd_csd.bin", "SecureDigital CSD",
                                     devInfo.CSD);

                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyCSD(devInfo.CSD));
                }

                if(devInfo.OCR != null)
                {
                    noInfo = false;

                    DataFile.WriteTo("Device-Info command", outputPrefix, "_sd_ocr.bin", "SecureDigital OCR",
                                     devInfo.OCR);

                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyOCR(devInfo.OCR));
                }

                if(devInfo.SCR != null)
                {
                    noInfo = false;

                    DataFile.WriteTo("Device-Info command", outputPrefix, "_sd_scr.bin", "SecureDigital SCR",
                                     devInfo.SCR);

                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifySCR(devInfo.SCR));
                }

                if(noInfo)
                    AaruConsole.WriteLine("Could not get any kind of information from the device !!!");
            }

                break;
        }

        dev.Close();

        AaruConsole.WriteLine();

        // Open main database
        var ctx = AaruContext.Create(Settings.Settings.MainDbPath);

        // Search for device in main database
        Aaru.Database.Models.Device dbDev =
            ctx.Devices.FirstOrDefault(d => d.Manufacturer == dev.Manufacturer && d.Model == dev.Model &&
                                            d.Revision     == dev.FirmwareRevision);

        if(dbDev is null)
            AaruConsole.
                WriteLine("Device not in database, please create a device report and attach it to a Github issue.");
        else
        {
            AaruConsole.WriteLine($"Device in database since {dbDev.LastSynchronized}.");

            if(dbDev.OptimalMultipleSectorsRead > 0)
                AaruConsole.WriteLine($"Optimal multiple read is {dbDev.LastSynchronized} sectors.");
        }

        if(dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
            return (int)ErrorNumber.NoError;

        // Search for read offset in main database
        CdOffset cdOffset =
            ctx.CdOffsets.FirstOrDefault(d => (d.Manufacturer == dev.Manufacturer ||
                                               d.Manufacturer == dev.Manufacturer.Replace('/', '-')) &&
                                              (d.Model == dev.Model || d.Model == dev.Model.Replace('/', '-')));

        AaruConsole.WriteLine(cdOffset is null ? "CD reading offset not found in database."
                                  : $"CD reading offset is {cdOffset.Offset} samples ({cdOffset.Offset * 4} bytes).");

        return (int)ErrorNumber.NoError;
    }
}