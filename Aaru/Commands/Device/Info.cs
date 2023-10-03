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
// Copyright © 2011-2023 Natalia Portillo
// Copyright © 2021-2023 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
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
using Aaru.Localization;
using Humanizer;
using Humanizer.Localisation;
using Spectre.Console;
using Command = System.CommandLine.Command;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;
using Inquiry = Aaru.Decoders.SCSI.Inquiry;
using Tuple = Aaru.Decoders.PCMCIA.Tuple;

namespace Aaru.Commands.Device;

sealed class DeviceInfoCommand : Command
{
    const string MODULE_NAME = "Device-Info command";

    public DeviceInfoCommand() : base("info", UI.Device_Info_Command_Description)
    {
        Add(new Option<string>(new[] { "--output-prefix", "-w" }, () => null, UI.Prefix_for_saving_binary_information));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Device_path,
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
        {
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };
        }

        Statistics.AddCommand("device-info");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",         debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--device={0}",        devicePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--output-prefix={0}", outputPrefix);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}",       verbose);

        if(devicePath.Length == 2   &&
           devicePath[1]     == ':' &&
           devicePath[0]     != '/' &&
           char.IsLetter(devicePath[0]))
            devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

        var dev = Devices.Device.Create(devicePath, out ErrorNumber devErrno);

        switch(dev)
        {
            case null:
                AaruConsole.ErrorWriteLine(string.Format(UI.Could_not_open_device_error_0, devErrno));

                return (int)devErrno;
            case Devices.Remote.Device remoteDev:
                Statistics.AddRemote(remoteDev.RemoteApplication, remoteDev.RemoteVersion,
                                     remoteDev.RemoteOperatingSystem, remoteDev.RemoteOperatingSystemVersion,
                                     remoteDev.RemoteArchitecture);

                break;
        }

        if(dev.Error)
        {
            AaruConsole.ErrorWriteLine(Error.Print(dev.LastError));

            return (int)ErrorNumber.CannotOpenDevice;
        }

        Statistics.AddDevice(dev);

        Table table;

        if(dev.IsUsb)
        {
            table = new Table
            {
                Title = new TableTitle($"[bold]{UI.Title_USB_device}[/]")
            };

            table.HideHeaders();
            table.AddColumn("");
            table.AddColumn("");
            table.Columns[0].RightAligned();

            if(dev.UsbDescriptors != null)
                table.AddRow(UI.Title_Descriptor_size, $"{dev.UsbDescriptors.Length}");

            table.AddRow(UI.Title_Vendor_ID,     $"{dev.UsbVendorId:X4}");
            table.AddRow(UI.Title_Product_ID,    $"{dev.UsbProductId:X4}");
            table.AddRow(UI.Title_Manufacturer,  Markup.Escape(dev.UsbManufacturerString ?? ""));
            table.AddRow(UI.Title_Product,       Markup.Escape(dev.UsbProductString      ?? ""));
            table.AddRow(UI.Title_Serial_number, Markup.Escape(dev.UsbSerialString       ?? ""));

            AnsiConsole.Write(table);
            AaruConsole.WriteLine();
        }

        if(dev.IsFireWire)
        {
            table = new Table
            {
                Title = new TableTitle($"[bold]{UI.Title_FireWire_device}[/]")
            };

            table.HideHeaders();
            table.AddColumn("");
            table.AddColumn("");
            table.Columns[0].RightAligned();

            table.AddRow(UI.Title_Vendor_ID, $"{dev.FireWireVendor:X6}");
            table.AddRow(UI.Title_Model_ID,  $"{dev.FireWireModel:X6}");
            table.AddRow(UI.Title_Vendor,    $"{Markup.Escape(dev.FireWireVendorName ?? "")}");
            table.AddRow(UI.Title_Model,     $"{Markup.Escape(dev.FireWireModelName  ?? "")}");
            table.AddRow(UI.Title_GUID,      $"{dev.FireWireGuid:X16}");

            AnsiConsole.Write(table);
            AaruConsole.WriteLine();
        }

        if(dev.IsPcmcia)
        {
            AaruConsole.WriteLine($"[bold]{UI.Title_PCMCIA_device}[/]");
            AaruConsole.WriteLine(UI.PCMCIA_CIS_is_0_bytes, dev.Cis.Length);
            Tuple[] tuples = CIS.GetTuples(dev.Cis);

            if(tuples != null)
            {
                foreach(Tuple tuple in tuples)
                {
                    switch(tuple.Code)
                    {
                        case TupleCodes.CISTPL_NULL:
                        case TupleCodes.CISTPL_END:
                            break;
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
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization.Core.Invoke_Found_undecoded_tuple_ID_0, tuple.Code);

                            break;
                        default:
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization.Core.Found_unknown_tuple_ID_0, (byte)tuple.Code);

                            break;
                    }
                }
            }
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Could_not_get_tuples);
        }

        var devInfo = new DeviceInfo(dev);

        if(devInfo.AtaIdentify != null)
        {
            DataFile.WriteTo(MODULE_NAME, outputPrefix, "_ata_identify.bin", "ATA IDENTIFY",
                             devInfo.AtaIdentify);

            Identify.IdentifyDevice? decodedIdentify = Identify.Decode(devInfo.AtaIdentify);
            AaruConsole.WriteLine(Decoders.ATA.Identify.Prettify(decodedIdentify));

            if(devInfo.AtaMcptError.HasValue)
            {
                AaruConsole.WriteLine(Localization.Core.Device_supports_MCPT_Command_Set);

                switch(devInfo.AtaMcptError.Value.DeviceHead & 0x7)
                {
                    case 0:
                        AaruConsole.WriteLine(Localization.Core.Device_reports_incorrect_media_card_type);

                        break;
                    case 1:
                        AaruConsole.WriteLine(Localization.Core.Device_contains_SD_card);

                        break;
                    case 2:
                        AaruConsole.WriteLine(Localization.Core.Device_contains_MMC);

                        break;
                    case 3:
                        AaruConsole.WriteLine(Localization.Core.Device_contains_SDIO_card);

                        break;
                    case 4:
                        AaruConsole.WriteLine(Localization.Core.Device_contains_SM_card);

                        break;
                    default:
                        AaruConsole.WriteLine(Localization.Core.Device_contains_unknown_media_card_type_0,
                                              devInfo.AtaMcptError.Value.DeviceHead & 0x07);

                        break;
                }

                if((devInfo.AtaMcptError.Value.DeviceHead & 0x08) == 0x08)
                    AaruConsole.WriteLine(Localization.Core.Media_card_is_write_protected);

                var specificData = (ushort)(devInfo.AtaMcptError.Value.CylinderHigh * 0x100 +
                                            devInfo.AtaMcptError.Value.CylinderLow);

                if(specificData != 0)
                    AaruConsole.WriteLine(Localization.Core.Card_specific_data_0, specificData);
            }

            if(decodedIdentify.HasValue)
            {
                Identify.IdentifyDevice ataid = decodedIdentify.Value;

                ulong blocks;

                if(ataid is { CurrentCylinders: > 0, CurrentHeads: > 0, CurrentSectorsPerTrack: > 0 })
                {
                    blocks = (ulong)Math.Max(ataid.CurrentCylinders * ataid.CurrentHeads * ataid.CurrentSectorsPerTrack,
                                             ataid.CurrentSectors);
                }
                else
                    blocks = (ulong)(ataid.Cylinders * ataid.Heads * ataid.SectorsPerTrack);

                if(ataid.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
                    blocks = ataid.LBASectors;

                if(ataid.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48))
                    blocks = ataid.LBA48Sectors;

                bool removable = ataid.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.Removable);

                MediaType mediaType = MediaTypeFromDevice.GetFromAta(dev.Manufacturer, dev.Model, removable,
                                                                     dev.IsCompactFlash, dev.IsPcmcia, blocks);

                AaruConsole.
                    WriteLine(
                        removable ? Localization.Core.Media_identified_as_0 : Localization.Core.Device_identified_as_0,
                        mediaType);

                Statistics.AddMedia(mediaType, true);
            }
        }

        if(devInfo.AtapiIdentify != null)
        {
            DataFile.WriteTo(MODULE_NAME, outputPrefix, "_atapi_identify.bin", "ATAPI IDENTIFY",
                             devInfo.AtapiIdentify);

            AaruConsole.WriteLine(Decoders.ATA.Identify.Prettify(devInfo.AtapiIdentify));
        }

        if(devInfo.ScsiInquiry != null)
        {
            if(dev.Type != DeviceType.ATAPI)
                AaruConsole.WriteLine($"[bold]{UI.Title_SCSI_device}[/]");

            DataFile.WriteTo(MODULE_NAME, outputPrefix, "_scsi_inquiry.bin", UI.Title_SCSI_INQUIRY,
                             devInfo.ScsiInquiryData);

            AaruConsole.WriteLine(Inquiry.Prettify(devInfo.ScsiInquiry));

            if(devInfo.ScsiEvpdPages != null)
            {
                foreach(KeyValuePair<byte, byte[]> page in devInfo.ScsiEvpdPages)
                {
                    switch(page.Key)
                    {
                        case >= 0x01 and <= 0x7F:
                            AaruConsole.WriteLine(Localization.Core.ASCII_Page_0_1, page.Key,
                                                  EVPD.DecodeASCIIPage(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, page.Value);

                            break;
                        case 0x80:
                            AaruConsole.WriteLine(Localization.Core.Unit_Serial_Number_0,
                                                  EVPD.DecodePage80(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0x81:
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_81(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0x82:
                            AaruConsole.WriteLine(Localization.Core.ASCII_implemented_operating_definitions_0,
                                                  EVPD.DecodePage82(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0x83:
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_83(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0x84:
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_84(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0x85:
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_85(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0x86:
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_86(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0x89:
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_89(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xB0:
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_B0(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xB1:
                            AaruConsole.WriteLine(Localization.Core.Manufacturer_assigned_Serial_Number_0,
                                                  EVPD.DecodePageB1(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xB2:
                            AaruConsole.WriteLine(Localization.Core.TapeAlert_Supported_Flags_Bitmap_0,
                                                  EVPD.DecodePageB2(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xB3:
                            AaruConsole.WriteLine(Localization.Core.Automation_Device_Serial_Number_0,
                                                  EVPD.DecodePageB3(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xB4:
                            AaruConsole.WriteLine(Localization.Core.Data_Transfer_Device_Element_Address_0,
                                                  EVPD.DecodePageB4(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xC0 when StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                                      ToLowerInvariant().Trim() == "quantum":
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_Quantum(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xC0 when StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                                      ToLowerInvariant().Trim() == "seagate":
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_Seagate(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xC0 when StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                                      ToLowerInvariant().Trim() == "ibm":
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_IBM(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xC1 when StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                                      ToLowerInvariant().Trim() == "ibm":
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C1_IBM(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xC0 or 0xC1
                            when StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                                ToLowerInvariant().Trim() == "certance":
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_C1_Certance(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xC2 or 0xC3 or 0xC4 or 0xC5 or 0xC6
                            when StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                                ToLowerInvariant().Trim() == "certance":
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C2_C3_C4_C5_C6_Certance(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC4 or 0xC5
                            when StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                                ToLowerInvariant().Trim() == "hp":
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_C0_to_C5_HP(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        case 0xDF when StringHandlers.CToString(devInfo.ScsiInquiry.Value.VendorIdentification).
                                                      ToLowerInvariant().Trim() == "certance":
                            AaruConsole.WriteLine("{0}", EVPD.PrettifyPage_DF_Certance(page.Value));

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        default:
                        {
                            if(page.Key == 0x00)
                                continue;

                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization.Core.Found_undecoded_SCSI_VPD_page_0, page.Key);

                            DataFile.WriteTo(MODULE_NAME, outputPrefix, $"_scsi_evpd_{page.Key:X2}h.bin",
                                             $"SCSI INQUIRY EVPD {page.Key:X2}h", page.Value);

                            break;
                        }
                    }
                }
            }

            if(devInfo.ScsiModeSense6 != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_scsi_modesense6.bin", "SCSI MODE SENSE",
                                 devInfo.ScsiModeSense6);
            }

            if(devInfo.ScsiModeSense10 != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_scsi_modesense10.bin", "SCSI MODE SENSE",
                                 devInfo.ScsiModeSense10);
            }

            if(devInfo.ScsiMode.HasValue)
            {
                PrintScsiModePages.Print(devInfo.ScsiMode.Value,
                                         (PeripheralDeviceTypes)devInfo.ScsiInquiry.Value.PeripheralDeviceType,
                                         devInfo.ScsiInquiry.Value.VendorIdentification);
            }

            if(devInfo.MmcConfiguration != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_mmc_getconfiguration.bin",
                                 "MMC GET CONFIGURATION", devInfo.MmcConfiguration);

                Features.SeparatedFeatures ftr = Features.Separate(devInfo.MmcConfiguration);

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.GET_CONFIGURATION_length_is_0,
                                           ftr.DataLength);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Core.GET_CONFIGURATION_current_profile_is_0,
                                           ftr.CurrentProfile);

                if(ftr.Descriptors != null)
                {
                    AaruConsole.WriteLine($"[bold]{UI.Title_SCSI_MMC_GET_CONFIGURATION_Features}[/]");

                    foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Feature_0, desc.Code);

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
                                AaruConsole.WriteLine(Localization.Core.Found_unknown_feature_code_0, desc.Code);

                                break;
                        }
                    }
                }
                else
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Core.GET_CONFIGURATION_returned_no_feature_descriptors);
                }
            }

            if(devInfo.RPC != null)
                AaruConsole.WriteLine(CSS_CPRM.PrettifyRegionalPlaybackControlState(devInfo.RPC));

            if(devInfo.PlextorFeatures?.Eeprom != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_plextor_eeprom.bin", "PLEXTOR READ EEPROM",
                                 devInfo.PlextorFeatures.Eeprom);

                AaruConsole.WriteLine(Localization.Core.Drive_has_loaded_a_total_of_0_discs,
                                      devInfo.PlextorFeatures.Discs);

                AaruConsole.WriteLine(Localization.Core.Drive_has_spent_0_reading_CDs,
                                      devInfo.PlextorFeatures.CdReadTime.Seconds().Humanize(minUnit: TimeUnit.Second));

                AaruConsole.WriteLine(Localization.Core.Drive_has_spent_0_writing_CDs,
                                      devInfo.PlextorFeatures.CdWriteTime.Seconds().Humanize(minUnit: TimeUnit.Second));

                if(devInfo.PlextorFeatures.IsDvd)
                {
                    AaruConsole.WriteLine(Localization.Core.Drive_has_spent_0_reading_DVDs,
                                          devInfo.PlextorFeatures.DvdReadTime.Seconds().
                                                  Humanize(minUnit: TimeUnit.Second));

                    AaruConsole.WriteLine(Localization.Core.Drive_has_spent_0_writing_DVDs,
                                          devInfo.PlextorFeatures.DvdWriteTime.Seconds().
                                                  Humanize(minUnit: TimeUnit.Second));
                }
            }

            if(devInfo.PlextorFeatures?.PoweRec == true)
            {
                if(devInfo.PlextorFeatures.PoweRecEnabled)
                {
                    if(devInfo.PlextorFeatures.PoweRecRecommendedSpeed > 0)
                    {
                        AaruConsole.WriteLine(Localization.Core.Drive_supports_PoweRec_is_enabled_and_recommends_0,
                                              devInfo.PlextorFeatures.PoweRecRecommendedSpeed);
                    }
                    else
                        AaruConsole.WriteLine(Localization.Core.Drive_supports_PoweRec_and_has_it_enabled);

                    if(devInfo.PlextorFeatures.PoweRecSelected > 0)
                    {
                        AaruConsole.
                            WriteLine(Localization.Core.Selected_PoweRec_speed_for_currently_inserted_media_is_0_1,
                                      devInfo.PlextorFeatures.PoweRecSelected,
                                      devInfo.PlextorFeatures.PoweRecSelected / 177);
                    }

                    if(devInfo.PlextorFeatures.PoweRecMax > 0)
                    {
                        AaruConsole.
                            WriteLine(Localization.Core.Maximum_PoweRec_speed_for_currently_inserted_media_is_0_1,
                                      devInfo.PlextorFeatures.PoweRecMax, devInfo.PlextorFeatures.PoweRecMax / 177);
                    }

                    if(devInfo.PlextorFeatures.PoweRecLast > 0)
                    {
                        AaruConsole.WriteLine(Localization.Core.Last_used_PoweRec_was_0_1,
                                              devInfo.PlextorFeatures.PoweRecLast,
                                              devInfo.PlextorFeatures.PoweRecLast / 177);
                    }
                }
                else
                    AaruConsole.WriteLine(Localization.Core.Drive_supports_PoweRec_and_has_it_disabled);
            }

            if(devInfo.PlextorFeatures?.SilentMode == true)
            {
                AaruConsole.WriteLine(Localization.Core.Drive_supports_Plextor_SilentMode);

                if(devInfo.PlextorFeatures.SilentModeEnabled)
                {
                    AaruConsole.WriteLine(Localization.Core.Plextor_SilentMode_is_enabled);

                    AaruConsole.WriteLine("\t" + (devInfo.PlextorFeatures.AccessTimeLimit == 2
                                                      ? Localization.Core.Access_time_is_slow
                                                      : Localization.Core.Access_time_is_fast));

                    if(devInfo.PlextorFeatures.CdReadSpeedLimit > 0)
                    {
                        AaruConsole.WriteLine("\t" + Localization.Core.CD_read_speed_limited_to_0,
                                              devInfo.PlextorFeatures.CdReadSpeedLimit);
                    }

                    if(devInfo.PlextorFeatures.DvdReadSpeedLimit > 0 &&
                       devInfo.PlextorFeatures.IsDvd)
                    {
                        AaruConsole.WriteLine("\t" + Localization.Core.DVD_read_speed_limited_to_0,
                                              devInfo.PlextorFeatures.DvdReadSpeedLimit);
                    }

                    if(devInfo.PlextorFeatures.CdWriteSpeedLimit > 0)
                    {
                        AaruConsole.WriteLine("\t" + Localization.Core.CD_write_speed_limited_to_0,
                                              devInfo.PlextorFeatures.CdWriteSpeedLimit);
                    }
                }
            }

            if(devInfo.PlextorFeatures?.GigaRec == true)
                AaruConsole.WriteLine(Localization.Core.Drive_supports_Plextor_GigaRec);

            if(devInfo.PlextorFeatures?.SecuRec == true)
                AaruConsole.WriteLine(Localization.Core.Drive_supports_Plextor_SecuRec);

            if(devInfo.PlextorFeatures?.SpeedRead == true)
            {
                AaruConsole.WriteLine(devInfo.PlextorFeatures.SpeedReadEnabled
                                          ? Localization.Core.Drive_supports_Plextor_SpeedRead_and_has_it_enabled
                                          : Localization.Core.Drive_supports_Plextor_SpeedRead);
            }

            if(devInfo.PlextorFeatures?.Hiding == true)
            {
                AaruConsole.WriteLine(Localization.Core.Drive_supports_hiding_CDRs_and_forcing_single_session);

                if(devInfo.PlextorFeatures.HidesRecordables)
                    AaruConsole.WriteLine(Localization.Core.Drive_currently_hides_CDRs);

                if(devInfo.PlextorFeatures.HidesSessions)
                    AaruConsole.WriteLine(Localization.Core.Drive_currently_forces_single_session);
            }

            if(devInfo.PlextorFeatures?.VariRec == true)
                AaruConsole.WriteLine(Localization.Core.Drive_supports_Plextor_VariRec);

            if(devInfo.PlextorFeatures?.IsDvd == true)
            {
                if(devInfo.PlextorFeatures.VariRecDvd)
                    AaruConsole.WriteLine(Localization.Core.Drive_supports_Plextor_VariRec_for_DVDs);

                if(devInfo.PlextorFeatures.BitSetting)
                    AaruConsole.WriteLine(Localization.Core.Drive_supports_bitsetting_DVD_R_book_type);

                if(devInfo.PlextorFeatures.BitSettingDl)
                    AaruConsole.WriteLine(Localization.Core.Drive_supports_bitsetting_DVD_R_DL_book_type);

                if(devInfo.PlextorFeatures.DvdPlusWriteTest)
                    AaruConsole.WriteLine(Localization.Core.Drive_supports_test_writing_DVD_Plus);
            }

            if(devInfo.ScsiInquiry.Value.KreonPresent)
            {
                AaruConsole.WriteLine($"[bold]{UI.Title_Drive_has_kreon_firmware}[/]");

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse))
                    AaruConsole.WriteLine("\t" + Localization.Core.Can_do_challenge_response_with_Xbox_discs);

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs))
                    AaruConsole.WriteLine("\t" + Localization.Core.Can_read_and_decrypt_SS_from_Xbox_discs);

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock))
                    AaruConsole.WriteLine("\t" + Localization.Core.Can_set_xtreme_unlock_state_with_Xbox_discs);

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock))
                    AaruConsole.WriteLine("\t" + Localization.Core.Can_set_wxripper_unlock_state_with_Xbox_discs);

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse360))
                    AaruConsole.WriteLine("\t" + Localization.Core.Can_do_challenge_response_with_Xbox_360_discs);

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs360))
                    AaruConsole.WriteLine("\t" + Localization.Core.Can_read_and_decrypt_SS_from_Xbox_360_discs);

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock360))
                    AaruConsole.WriteLine("\t" + Localization.Core.Can_set_xtreme_unlock_state_with_Xbox_360_discs);

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock360))
                    AaruConsole.WriteLine("\t" + Localization.Core.Can_set_wxripper_unlock_state_with_Xbox_360_discs);

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.Lock))
                    AaruConsole.WriteLine("\t" + Localization.Core.Can_set_Kreon_locked_state);

                if(devInfo.KreonFeatures.HasFlag(KreonFeatures.ErrorSkipping))
                    AaruConsole.WriteLine("\t" + Localization.Core.Kreon_Can_skip_read_errors);
            }

            if(devInfo.BlockLimits != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_ssc_readblocklimits.bin",
                                 "SSC READ BLOCK LIMITS", devInfo.BlockLimits);

                AaruConsole.WriteLine(Localization.Core.Block_limits_for_device);
                AaruConsole.WriteLine(BlockLimits.Prettify(devInfo.BlockLimits));
            }

            if(devInfo.DensitySupport != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_ssc_reportdensitysupport.bin",
                                 "SSC REPORT DENSITY SUPPORT", devInfo.DensitySupport);

                if(devInfo.DensitySupportHeader.HasValue)
                {
                    AaruConsole.WriteLine(UI.Densities_supported_by_device);
                    AaruConsole.WriteLine(DensitySupport.PrettifyDensity(devInfo.DensitySupportHeader));
                }
            }

            if(devInfo.MediumDensitySupport != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_ssc_reportdensitysupport_medium.bin",
                                 "SSC REPORT DENSITY SUPPORT (MEDIUM)", devInfo.MediumDensitySupport);

                if(devInfo.MediaTypeSupportHeader.HasValue)
                {
                    AaruConsole.WriteLine(UI.Medium_types_supported_by_device);
                    AaruConsole.WriteLine(DensitySupport.PrettifyMediumType(devInfo.MediaTypeSupportHeader));
                }

                AaruConsole.WriteLine(DensitySupport.PrettifyMediumType(devInfo.MediumDensitySupport));
            }
        }

        switch(dev.Type)
        {
            case DeviceType.MMC:
            {
                var noInfo = true;

                if(devInfo.CID != null)
                {
                    noInfo = false;
                    DataFile.WriteTo(MODULE_NAME, outputPrefix, "_mmc_cid.bin", "MMC CID", devInfo.CID);
                    AaruConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyCID(devInfo.CID));
                }

                if(devInfo.CSD != null)
                {
                    noInfo = false;
                    DataFile.WriteTo(MODULE_NAME, outputPrefix, "_mmc_csd.bin", "MMC CSD", devInfo.CSD);
                    AaruConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyCSD(devInfo.CSD));
                }

                if(devInfo.OCR != null)
                {
                    noInfo = false;
                    DataFile.WriteTo(MODULE_NAME, outputPrefix, "_mmc_ocr.bin", "MMC OCR", devInfo.OCR);
                    AaruConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyOCR(devInfo.OCR));
                }

                if(devInfo.ExtendedCSD != null)
                {
                    noInfo = false;

                    DataFile.WriteTo(MODULE_NAME, outputPrefix, "_mmc_ecsd.bin", "MMC Extended CSD",
                                     devInfo.ExtendedCSD);

                    AaruConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyExtendedCSD(devInfo.ExtendedCSD));
                }

                if(noInfo)
                    AaruConsole.WriteLine("Could not get any kind of information from the device !!!");
            }

                break;
            case DeviceType.SecureDigital:
            {
                var noInfo = true;

                if(devInfo.CID != null)
                {
                    noInfo = false;

                    DataFile.WriteTo(MODULE_NAME, outputPrefix, "_sd_cid.bin", "SecureDigital CID",
                                     devInfo.CID);

                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyCID(devInfo.CID));
                }

                if(devInfo.CSD != null)
                {
                    noInfo = false;

                    DataFile.WriteTo(MODULE_NAME, outputPrefix, "_sd_csd.bin", "SecureDigital CSD",
                                     devInfo.CSD);

                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyCSD(devInfo.CSD));
                }

                if(devInfo.OCR != null)
                {
                    noInfo = false;

                    DataFile.WriteTo(MODULE_NAME, outputPrefix, "_sd_ocr.bin", "SecureDigital OCR",
                                     devInfo.OCR);

                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyOCR(devInfo.OCR));
                }

                if(devInfo.SCR != null)
                {
                    noInfo = false;

                    DataFile.WriteTo(MODULE_NAME, outputPrefix, "_sd_scr.bin", "SecureDigital SCR",
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
            AaruConsole.WriteLine(Localization.Core.Device_not_in_database);
        else
        {
            AaruConsole.WriteLine(string.Format(Localization.Core.Device_in_database_since_0, dbDev.LastSynchronized));

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

        AaruConsole.WriteLine(cdOffset is null
                                  ? "CD reading offset not found in database."
                                  : $"CD reading offset is {cdOffset.Offset} samples ({cdOffset.Offset * 4} bytes).");

        return (int)ErrorNumber.NoError;
    }
}