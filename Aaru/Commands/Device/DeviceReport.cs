// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReport.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'report' command.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core;
using Aaru.Database;
using Aaru.Database.Models;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Devices;
using Aaru.Helpers;
using Newtonsoft.Json;
using Spectre.Console;
using Command = System.CommandLine.Command;
using DeviceReport = Aaru.Core.Devices.Report.DeviceReport;
using Profile = Aaru.Decoders.SCSI.MMC.Profile;

namespace Aaru.Commands.Device
{
    internal sealed class DeviceReportCommand : Command
    {
        public DeviceReportCommand() : base("report",
                                            "Tests the device capabilities and creates an JSON report of them.")
        {
            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Device path",
                Name        = "device-path"
            });

            Add(new Option(new[]
                {
                    "--trap-disc", "-t"
                }, "Does a device report using a trap disc.")
                {
                    Argument = new Argument<bool>(() => false),
                    Required = false
                });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string devicePath, bool trapDisc)
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

            Statistics.AddCommand("device-report");

            AaruConsole.DebugWriteLine("Device-Report command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Device-Report command", "--device={0}", devicePath);
            AaruConsole.DebugWriteLine("Device-Report command", "--verbose={0}", verbose);

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

            bool isAdmin = dev.IsRemote ? dev.IsRemoteAdmin : DetectOS.IsAdmin;

            if(!isAdmin)
            {
                AaruConsole.
                    ErrorWriteLine("Because of the commands sent to a device, device report must be run with administrative privileges.");

                AaruConsole.ErrorWriteLine("Not continuing.");

                return (int)ErrorNumber.NotPermitted;
            }

            var report = new DeviceReportV2
            {
                Manufacturer = dev.Manufacturer,
                Model        = dev.Model,
                Revision     = dev.FirmwareRevision,
                Type         = dev.Type
            };

            bool   removable = false;
            string jsonFile;

            if(!string.IsNullOrWhiteSpace(dev.Manufacturer) &&
               !string.IsNullOrWhiteSpace(dev.FirmwareRevision))
                jsonFile = dev.Manufacturer + "_" + dev.Model + "_" + dev.FirmwareRevision + ".json";
            else if(!string.IsNullOrWhiteSpace(dev.Manufacturer))
                jsonFile = dev.Manufacturer + "_" + dev.Model + ".json";
            else if(!string.IsNullOrWhiteSpace(dev.FirmwareRevision))
                jsonFile = dev.Model + "_" + dev.FirmwareRevision + ".json";
            else
                jsonFile = dev.Model + ".json";

            jsonFile = jsonFile.Replace('\\', '_').Replace('/', '_').Replace('?', '_');

            if(trapDisc && dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
            {
                AaruConsole.ErrorWriteLine("This device type does not support doing reports with trap discs.");

                return (int)ErrorNumber.InvalidArgument;
            }

            var reporter = new DeviceReport(dev);

            ConsoleKeyInfo pressedKey;

            if(dev.IsUsb)
            {
                if(AnsiConsole.Confirm("[italic]Is the device natively USB (in case of doubt, press Y)?[/]"))
                {
                    Core.Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Querying USB information...").IsIndeterminate();
                        report.USB = reporter.UsbReport();
                    });

                    report.USB.RemovableMedia =
                        AnsiConsole.Confirm("[italic]Is the media removable from the reading/writing elements?[/]");

                    removable = report.USB.RemovableMedia;
                }
            }

            if(dev.IsFireWire)
            {
                if(AnsiConsole.Confirm("[italic]Is the device natively FireWire (in case of doubt, press Y)?[/]"))
                {
                    Core.Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Querying FireWire information...").IsIndeterminate();
                        report.FireWire = reporter.FireWireReport();
                    });

                    report.FireWire.RemovableMedia =
                        AnsiConsole.Confirm("[italic]Is the media removable from the reading/writing elements?[/]");

                    removable = report.FireWire.RemovableMedia;
                }
            }

            if(dev.IsPcmcia)
                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying PCMCIA information...").IsIndeterminate();
                    report.PCMCIA = reporter.PcmciaReport();
                });

            byte[] buffer = Array.Empty<byte>();
            string mediumTypeName;
            string mediumModel;

            switch(dev.Type)
            {
                case DeviceType.ATA:
                {
                    Core.Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Querying ATA IDENTIFY...").IsIndeterminate();
                        dev.AtaIdentify(out buffer, out _, dev.Timeout, out _);
                    });

                    if(!Identify.Decode(buffer).HasValue)
                        break;

                    report.ATA = new Ata
                    {
                        Identify = DeviceReport.ClearIdentify(buffer)
                    };

                    if(report.ATA.IdentifyDevice == null)
                        break;

                    if((ushort)report.ATA.IdentifyDevice?.GeneralConfiguration == 0x848A)
                    {
                        report.CompactFlash = true;
                        removable           = false;
                    }
                    else if(!removable &&
                            report.ATA.IdentifyDevice?.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.
                                Removable) == true)
                        removable =
                            AnsiConsole.Confirm("[italic]Is the media removable from the reading/writing elements?[/]");

                    if(removable)
                    {
                        AaruConsole.
                            WriteLine("Please remove any media from the device and press any key when it is out.");

                        System.Console.ReadKey(true);

                        Core.Spectre.ProgressSingleSpinner(ctx =>
                        {
                            ctx.AddTask("Querying ATA IDENTIFY...").IsIndeterminate();
                            dev.AtaIdentify(out buffer, out _, dev.Timeout, out _);
                        });

                        report.ATA.Identify = DeviceReport.ClearIdentify(buffer);
                        List<TestedMedia> mediaTests = new();

                        while(AnsiConsole.Confirm("[italic]Do you have media that you can insert in the drive?[/]"))
                        {
                            AaruConsole.WriteLine("Please insert it in the drive and press any key when it is ready.");
                            System.Console.ReadKey(true);

                            mediumTypeName =
                                AnsiConsole.
                                    Ask<string>("Please write a description of the media type and press enter: ");

                            mediumModel = AnsiConsole.Ask<string>("Please write the media model and press enter: ");

                            TestedMedia mediaTest = reporter.ReportAtaMedia();
                            mediaTest.MediumTypeName = mediumTypeName;
                            mediaTest.Model          = mediumModel;

                            mediaTests.Add(mediaTest);
                        }

                        report.ATA.RemovableMedias = mediaTests;
                    }
                    else
                    {
                        report.ATA.ReadCapabilities = reporter.ReportAta(report.ATA.IdentifyDevice.Value);
                    }

                    break;
                }

                case DeviceType.MMC:
                    report.MultiMediaCard = reporter.MmcSdReport();

                    break;
                case DeviceType.SecureDigital:
                    report.SecureDigital = reporter.MmcSdReport();

                    break;
                case DeviceType.NVMe: throw new NotImplementedException("NVMe devices not yet supported.");
                case DeviceType.ATAPI:
                    Core.Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Querying ATAPI IDENTIFY...").IsIndeterminate();
                        dev.AtapiIdentify(out buffer, out _, dev.Timeout, out _);
                    });

                    if(Identify.Decode(buffer).HasValue)
                        report.ATAPI = new Ata
                        {
                            Identify = DeviceReport.ClearIdentify(buffer)
                        };

                    goto case DeviceType.SCSI;
                case DeviceType.SCSI:
                    switch(dev.ScsiType)
                    {
                        case PeripheralDeviceTypes.DirectAccess:
                        case PeripheralDeviceTypes.SequentialAccess:
                        case PeripheralDeviceTypes.WriteOnceDevice:
                        case PeripheralDeviceTypes.MultiMediaDevice:
                        case PeripheralDeviceTypes.OpticalDevice:
                        case PeripheralDeviceTypes.BridgingExpander
                            when dev.Model.StartsWith("MDM", StringComparison.Ordinal) ||
                                 dev.Model.StartsWith("MDH", StringComparison.Ordinal):
                        case PeripheralDeviceTypes.SCSIZonedBlockDevice: break;
                        default:
                        {
                            AaruConsole.ErrorWriteLine("Unsupported device type, report cannot be created");

                            throw new IOException();
                        }
                    }

                    if(!dev.IsUsb      &&
                       !dev.IsFireWire &&
                       dev.IsRemovable)
                        removable =
                            AnsiConsole.
                                Confirm("[italic]Is the media removable from the reading/writing elements (flash memories ARE NOT removable)?[/]");

                    if(removable)
                    {
                        switch(dev.ScsiType)
                        {
                            case PeripheralDeviceTypes.MultiMediaDevice:
                            case PeripheralDeviceTypes.BridgingExpander
                                when dev.Model.StartsWith("MDM", StringComparison.Ordinal) ||
                                     dev.Model.StartsWith("MDH", StringComparison.Ordinal):
                                dev.AllowMediumRemoval(out buffer, dev.Timeout, out _);
                                dev.EjectTray(out buffer, dev.Timeout, out _);

                                break;
                            case PeripheralDeviceTypes.SequentialAccess:
                                dev.SpcAllowMediumRemoval(out buffer, dev.Timeout, out _);

                                Core.Spectre.ProgressSingleSpinner(ctx =>
                                {
                                    ctx.AddTask("Asking drive to unload tape (can take a few minutes)...").
                                        IsIndeterminate();

                                    dev.Unload(out buffer, dev.Timeout, out _);
                                });

                                break;
                        }

                        AaruConsole.
                            WriteLine("Please remove any media from the device and press any key when it is out.");

                        System.Console.ReadKey(true);
                    }

                    report.SCSI = reporter.ReportScsiInquiry();

                    if(report.SCSI == null)
                        break;

                    report.SCSI.EVPDPages =
                        reporter.ReportEvpdPages(StringHandlers.CToString(report.SCSI.Inquiry?.VendorIdentification)?.
                                                                Trim().ToLowerInvariant());

                    reporter.ReportScsiModes(ref report, out byte[] cdromMode, out MediumTypes mediumType);

                    string mediumManufacturer;
                    byte[] senseBuffer = Array.Empty<byte>();
                    bool   sense       = true;

                    switch(dev.ScsiType)
                    {
                        case PeripheralDeviceTypes.MultiMediaDevice:
                        {
                            if(dev.IsUsb &&
                               (mediumType == MediumTypes.UnknownBlockDevice  ||
                                mediumType == MediumTypes.ReadOnlyBlockDevice ||
                                mediumType == MediumTypes.ReadWriteBlockDevice))
                                goto default;

                            bool iomegaRev = dev.Manufacturer.ToLowerInvariant() == "iomega" && dev.Model.
                                                 ToLowerInvariant().
                                                 StartsWith("rrd", StringComparison.OrdinalIgnoreCase);

                            if(trapDisc)
                            {
                                if(iomegaRev)
                                {
                                    AaruConsole.
                                        ErrorWriteLine("This device type does not support doing reports with trap discs.");

                                    return (int)ErrorNumber.InvalidArgument;
                                }

                                if(
                                    !AnsiConsole.
                                        Confirm("[italic]Are you sure you want to do a report using a trap disc and the swapping method?\n" +
                                                "This method can damage the drive, or the disc, and requires some ability.\n" +
                                                "In you are unsure, please press N to not continue.[/]"))
                                    return (int)ErrorNumber.NoError;

                                if(!AnsiConsole.
                                       Confirm("[italic]Do you have an audio trap disc (if unsure press N)?[/]"))
                                {
                                    AaruConsole.ErrorWriteLine("Please burn an audio trap disc before continuing...");

                                    return (int)ErrorNumber.NoError;
                                }

                                if(AnsiConsole.Confirm("[italic]Do you have a GD-ROM disc (if unsure press N)?[/]"))
                                    reporter.ReportGdRomSwapTrick(ref report);
                                else
                                    return (int)ErrorNumber.NoError;
                            }
                            else
                            {
                                List<string> mediaTypes = new();

                                report.SCSI.MultiMediaDevice = new Mmc
                                {
                                    ModeSense2AData = cdromMode,
                                    Features        = reporter.ReportMmcFeatures()
                                };

                                if(report.SCSI.MultiMediaDevice.Features?.BinaryData != null)
                                {
                                    Features.SeparatedFeatures ftr =
                                        Features.Separate(report.SCSI.MultiMediaDevice.Features.BinaryData);

                                    if(ftr.Descriptors != null)
                                    {
                                        foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                                        {
                                            if(desc.Code != 0x0000)
                                                continue;

                                            Feature_0000? ftr0000 = Features.Decode_0000(desc.Data);

                                            if(ftr0000 == null)
                                                continue;

                                            foreach(Profile prof in ftr0000.Value.Profiles)
                                            {
                                                switch(prof.Number)
                                                {
                                                    case ProfileNumber.CDROM:
                                                    case ProfileNumber.CDR:
                                                    case ProfileNumber.CDRW:
                                                        mediaTypes.Add("CD-ROM");
                                                        mediaTypes.Add("Audio CD (without data tracks)");
                                                        mediaTypes.Add("Enhanced CD (aka E-CD, CD-Plus or CD+)");
                                                        mediaTypes.Add("CD-R");
                                                        mediaTypes.Add("CD-RW Ultra Speed (marked 16x or higher)");
                                                        mediaTypes.Add("CD-RW High Speed (marked between 8x and 12x)");
                                                        mediaTypes.Add("CD-RW (marked 4x or lower)");

                                                        break;
                                                    case ProfileNumber.DVDRWRes:
                                                    case ProfileNumber.DVDRWSeq:
                                                    case ProfileNumber.DVDRDLSeq:
                                                    case ProfileNumber.DVDRDLJump:
                                                    case ProfileNumber.DVDRWDL:
                                                    case ProfileNumber.DVDDownload:
                                                    case ProfileNumber.DVDRWPlus:
                                                    case ProfileNumber.DVDRPlus:
                                                    case ProfileNumber.DVDRSeq:
                                                    case ProfileNumber.DVDRWDLPlus:
                                                    case ProfileNumber.DVDRDLPlus:

                                                    case ProfileNumber.DVDROM:
                                                        mediaTypes.Add("DVD-ROM");
                                                        mediaTypes.Add("DVD-R");
                                                        mediaTypes.Add("DVD-RW");
                                                        mediaTypes.Add("DVD+R");
                                                        mediaTypes.Add("DVD+RW");
                                                        mediaTypes.Add("DVD-R DL");
                                                        mediaTypes.Add("DVD+R DL");
                                                        mediaTypes.Add("Nintendo GameCube game");
                                                        mediaTypes.Add("Nintendo Wii game");

                                                        break;
                                                    case ProfileNumber.DVDRAM:
                                                        mediaTypes.Add("DVD-RAM (1st gen, marked 2.6Gb or 5.2Gb)");
                                                        mediaTypes.Add("DVD-RAM (2nd gen, marked 4.7Gb or 9.4Gb)");

                                                        break;
                                                    case ProfileNumber.DDCDROM:
                                                    case ProfileNumber.DDCDR:
                                                    case ProfileNumber.DDCDRW:
                                                        mediaTypes.Add("DDCD-ROM");
                                                        mediaTypes.Add("DDCD-R");
                                                        mediaTypes.Add("DDCD-RW");

                                                        break;
                                                    case ProfileNumber.BDROM:
                                                    case ProfileNumber.BDRSeq:
                                                    case ProfileNumber.BDRRdm:
                                                    case ProfileNumber.BDRE:
                                                        mediaTypes.Add("BD-ROM");
                                                        mediaTypes.Add("BD-R HTL (not LTH)");
                                                        mediaTypes.Add("BD-RE");
                                                        mediaTypes.Add("BD-R LTH");
                                                        mediaTypes.Add("BD-R Triple Layer (100Gb)");
                                                        mediaTypes.Add("BD-R Quad Layer (128Gb)");
                                                        mediaTypes.Add("Ultra HD Blu-ray movie");
                                                        mediaTypes.Add("PlayStation 3 game");
                                                        mediaTypes.Add("PlayStation 4 game");
                                                        mediaTypes.Add("PlayStation 5 game");
                                                        mediaTypes.Add("Xbox One game");
                                                        mediaTypes.Add("Nintendo Wii U game");

                                                        break;
                                                    case ProfileNumber.HDDVDROM:
                                                    case ProfileNumber.HDDVDR:
                                                    case ProfileNumber.HDDVDRW:
                                                    case ProfileNumber.HDDVDRDL:
                                                    case ProfileNumber.HDDVDRWDL:
                                                        mediaTypes.Add("HD DVD-ROM");
                                                        mediaTypes.Add("HD DVD-R");
                                                        mediaTypes.Add("HD DVD-RW");

                                                        break;
                                                    case ProfileNumber.HDDVDRAM:
                                                        mediaTypes.Add("HD DVD-RAM");

                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if(cdromMode != null &&
                                   !iomegaRev)
                                {
                                    mediaTypes.Add("CD-ROM");
                                    mediaTypes.Add("Audio CD (without data tracks)");
                                    mediaTypes.Add("Enhanced CD (aka E-CD, CD-Plus or CD+)");

                                    if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadCDR)
                                        mediaTypes.Add("CD-R");

                                    if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadCDRW)
                                    {
                                        mediaTypes.Add("CD-RW Ultra Speed (marked 16x or higher)");
                                        mediaTypes.Add("CD-RW High Speed (marked between 8x and 12x)");
                                        mediaTypes.Add("CD-RW (marked 4x or lower)");
                                    }

                                    if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadDVDROM)
                                        mediaTypes.Add("DVD-ROM");

                                    if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadDVDRAM)
                                    {
                                        mediaTypes.Add("DVD-RAM (1st gen, marked 2.6Gb or 5.2Gb)");
                                        mediaTypes.Add("DVD-RAM (2nd gen, marked 4.7Gb or 9.4Gb)");
                                    }

                                    if(report.SCSI.MultiMediaDevice.ModeSense2A.ReadDVDR)
                                        mediaTypes.Add("DVD-R");
                                }

                                if(report.SCSI.MultiMediaDevice.Features != null &&
                                   !iomegaRev)
                                {
                                    if(report.SCSI.MultiMediaDevice.Features.CanReadBD      ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadBDR     ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadBDRE1   ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadBDRE2   ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadBDROM   ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadOldBDR  ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadOldBDRE ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadOldBDROM)
                                    {
                                        mediaTypes.Add("BD-ROM");
                                        mediaTypes.Add("BD-R HTL (not LTH)");
                                        mediaTypes.Add("BD-RE");
                                        mediaTypes.Add("BD-R LTH");
                                        mediaTypes.Add("BD-R Triple Layer (100Gb)");
                                        mediaTypes.Add("BD-R Quad Layer (128Gb)");
                                        mediaTypes.Add("Ultra HD Blu-ray movie");
                                        mediaTypes.Add("PlayStation 3 game");
                                        mediaTypes.Add("PlayStation 4 game");
                                        mediaTypes.Add("PlayStation 5 game");
                                        mediaTypes.Add("Xbox One game");
                                        mediaTypes.Add("Nintendo Wii U game");
                                    }

                                    if(report.SCSI.MultiMediaDevice.Features.CanReadCD ||
                                       report.SCSI.MultiMediaDevice.Features.MultiRead)
                                    {
                                        mediaTypes.Add("CD-ROM");
                                        mediaTypes.Add("Audio CD (without data tracks)");
                                        mediaTypes.Add("Enhanced CD (aka E-CD, CD-Plus or CD+)");
                                        mediaTypes.Add("CD-R");
                                        mediaTypes.Add("CD-RW Ultra Speed (marked 16x or higher)");
                                        mediaTypes.Add("CD-RW High Speed (marked between 8x and 12x)");
                                        mediaTypes.Add("CD-RW (marked 4x or lower)");
                                    }

                                    if(report.SCSI.MultiMediaDevice.Features.CanReadCDMRW)
                                        mediaTypes.Add("CD-MRW");

                                    if(report.SCSI.MultiMediaDevice.Features.CanReadDDCD)
                                    {
                                        mediaTypes.Add("DDCD-ROM");
                                        mediaTypes.Add("DDCD-R");
                                        mediaTypes.Add("DDCD-RW");
                                    }

                                    if(report.SCSI.MultiMediaDevice.Features.CanReadDVD        ||
                                       report.SCSI.MultiMediaDevice.Features.DVDMultiRead      ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusR   ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRDL ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRW  ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRWDL)
                                    {
                                        mediaTypes.Add("DVD-ROM");
                                        mediaTypes.Add("DVD-R");
                                        mediaTypes.Add("DVD-RW");
                                        mediaTypes.Add("DVD+R");
                                        mediaTypes.Add("DVD+RW");
                                        mediaTypes.Add("DVD-R DL");
                                        mediaTypes.Add("DVD+R DL");
                                        mediaTypes.Add("Nintendo GameCube game");
                                        mediaTypes.Add("Nintendo Wii game");
                                        mediaTypes.Add("DVD-RAM (1st gen, marked 2.6Gb or 5.2Gb)");
                                        mediaTypes.Add("DVD-RAM (2nd gen, marked 4.7Gb or 9.4Gb)");
                                    }

                                    if(report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusMRW)
                                        mediaTypes.Add("DVD+MRW");

                                    if(report.SCSI.MultiMediaDevice.Features.CanReadHDDVD ||
                                       report.SCSI.MultiMediaDevice.Features.CanReadHDDVDR)
                                    {
                                        mediaTypes.Add("HD DVD-ROM");
                                        mediaTypes.Add("HD DVD-R");
                                        mediaTypes.Add("HD DVD-RW");
                                    }

                                    if(report.SCSI.MultiMediaDevice.Features.CanReadHDDVDRAM)
                                        mediaTypes.Add("HD DVD-RAM");
                                }

                                if(iomegaRev)
                                {
                                    mediaTypes.Add("REV 35Gb");
                                    mediaTypes.Add("REV 70Gb");
                                    mediaTypes.Add("REV 120Gb");
                                }

                                // Very old CD drives do not contain mode page 2Ah neither GET CONFIGURATION, so just try all CDs on them
                                // Also don't get confident, some drives didn't know CD-RW but are able to read them
                                if(mediaTypes.Count == 0 ||
                                   mediaTypes.Contains("CD-ROM"))
                                {
                                    mediaTypes.Add("CD-ROM");
                                    mediaTypes.Add("Audio CD (without data tracks)");
                                    mediaTypes.Add("CD-R");
                                    mediaTypes.Add("CD-RW Ultra Speed (marked 16x or higher)");
                                    mediaTypes.Add("CD-RW High Speed (marked between 8x and 12x)");
                                    mediaTypes.Add("CD-RW (marked 4x or lower)");
                                    mediaTypes.Add("Enhanced CD (aka E-CD, CD-Plus or CD+)");
                                }

                                mediaTypes = mediaTypes.Distinct().ToList();
                                mediaTypes.Sort();

                                bool tryPlextor      = false, tryHldtst = false, tryPioneer = false, tryNec = false,
                                     tryMediaTekF106 = false;

                                tryPlextor |= dev.Manufacturer.ToLowerInvariant() == "plextor";
                                tryHldtst  |= dev.Manufacturer.ToLowerInvariant() == "hl-dt-st";
                                tryPioneer |= dev.Manufacturer.ToLowerInvariant() == "pioneer";
                                tryNec     |= dev.Manufacturer.ToLowerInvariant() == "nec";

                                if(!iomegaRev)
                                {
                                    if(!tryPlextor)
                                        tryPlextor |=
                                            AnsiConsole.
                                                Confirm("[italic]Do you have want to try Plextor vendor commands? [red]THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N')[/][/]",
                                                        false);

                                    if(!tryNec)
                                        tryNec |=
                                            AnsiConsole.
                                                Confirm("[italic]Do you have want to try NEC vendor commands? [red]THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N')[/][/]",
                                                        false);

                                    if(!tryPioneer)
                                        tryPioneer |=
                                            AnsiConsole.
                                                Confirm("[italic]Do you have want to try Pioneer vendor commands? [red]THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N')[/][/]",
                                                        false);

                                    if(!tryHldtst)
                                        tryHldtst |=
                                            AnsiConsole.
                                                Confirm("[italic]Do you have want to try HL-DT-ST (aka LG) vendor commands? [red]THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N')[/][/]",
                                                        false);

                                    tryMediaTekF106 =
                                        AnsiConsole.
                                            Confirm("[italic]Do you have want to try MediaTek vendor command F1h subcommand 06h? [red]THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N')[/][/]",
                                                    false);
                                }

                                if(dev.Model.StartsWith("PD-", StringComparison.Ordinal))
                                    mediaTypes.Add("PD-650");

                                List<TestedMedia> mediaTests = new();

                                foreach(string mediaType in mediaTypes)
                                {
                                    if(!AnsiConsole.
                                           Confirm($"[italic]Do you have a {mediaType} disc that you can insert in the drive?[/]"))
                                        continue;

                                    AaruConsole.
                                        WriteLine("Please insert it in the drive and press any key when it is ready.");

                                    System.Console.ReadKey(true);

                                    bool mediaIsRecognized = true;

                                    Core.Spectre.ProgressSingleSpinner(ctx =>
                                    {
                                        ctx.AddTask("Waiting for drive to become ready").IsIndeterminate();
                                        sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                        if(!sense)
                                            return;

                                        DecodedSense? decSense = Sense.Decode(senseBuffer);

                                        if(decSense.HasValue)
                                        {
                                            switch(decSense.Value.ASC)
                                            {
                                                case 0x3A:
                                                {
                                                    int leftRetries = 50;

                                                    while(leftRetries > 0)
                                                    {
                                                        Thread.Sleep(2000);

                                                        sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout,
                                                            out _);

                                                        if(!sense)
                                                            break;

                                                        leftRetries--;
                                                    }

                                                    AaruConsole.WriteLine();

                                                    mediaIsRecognized &= !sense;

                                                    break;
                                                }

                                                // These should be trapped by the OS but seems in some cases they're not
                                                case 0x04 when decSense.Value.ASCQ == 0x01:
                                                {
                                                    int leftRetries = 50;

                                                    while(leftRetries > 0)
                                                    {
                                                        Thread.Sleep(2000);

                                                        sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout,
                                                            out _);

                                                        if(!sense)
                                                            break;

                                                        leftRetries--;
                                                    }

                                                    AaruConsole.WriteLine();

                                                    mediaIsRecognized &= !sense;

                                                    break;
                                                }
                                                case 0x28:
                                                {
                                                    int leftRetries = 50;

                                                    while(leftRetries > 0)
                                                    {
                                                        Thread.Sleep(2000);

                                                        sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout,
                                                            out _);

                                                        if(!sense)
                                                            break;

                                                        leftRetries--;
                                                    }

                                                    AaruConsole.WriteLine();

                                                    mediaIsRecognized &= !sense;

                                                    break;
                                                }
                                                default:
                                                    AaruConsole.DebugWriteLine("Device-Report command",
                                                                               "Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                                               decSense.Value.SenseKey,
                                                                               decSense.Value.ASC, decSense.Value.ASCQ);

                                                    mediaIsRecognized = false;

                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            AaruConsole.DebugWriteLine("Device-Report command",
                                                                       "Got sense status but no sense buffer");

                                            mediaIsRecognized = false;
                                        }
                                    });

                                    var mediaTest = new TestedMedia();

                                    if(mediaIsRecognized)
                                    {
                                        mediaTest = reporter.ReportMmcMedia(mediaType, tryPlextor, tryPioneer, tryNec,
                                                                            tryHldtst, tryMediaTekF106);

                                        if(mediaTest is null)
                                            continue;

                                        if((mediaTest.SupportsReadLong   == true ||
                                            mediaTest.SupportsReadLong16 == true)         &&
                                           mediaTest.LongBlockSize == mediaTest.BlockSize &&
                                           AnsiConsole.
                                               Confirm("[italic]Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours)[/]"))
                                        {
                                            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                                                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(),
                                                                new PercentageColumn()).Start(ctx =>
                                                        {
                                                            ProgressTask task = ctx.AddTask("Trying to READ LONG...");
                                                            task.MaxValue = ushort.MaxValue;

                                                            for(ushort i = (ushort)mediaTest.BlockSize;; i++)
                                                            {
                                                                task.Description =
                                                                    $"Trying to READ LONG with a size of {i} bytes...";

                                                                task.Value = i;

                                                                sense = mediaTest.SupportsReadLong16 == true
                                                                            ? dev.ReadLong16(out buffer,
                                                                                out senseBuffer, false, 0, i,
                                                                                dev.Timeout, out _)
                                                                            : dev.ReadLong10(out buffer,
                                                                                out senseBuffer, false, false, 0,
                                                                                i, dev.Timeout, out _);

                                                                if(!sense)
                                                                {
                                                                    mediaTest.LongBlockSize = i;

                                                                    break;
                                                                }

                                                                if(i == ushort.MaxValue)
                                                                    break;
                                                            }
                                                        });
                                        }

                                        if(mediaTest.SupportsReadLong == true &&
                                           mediaTest.LongBlockSize    != mediaTest.BlockSize)
                                        {
                                            Core.Spectre.ProgressSingleSpinner(ctx =>
                                            {
                                                ctx.AddTask("Trying SCSI READ LONG (10)...").IsIndeterminate();

                                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                       (ushort)mediaTest.LongBlockSize, dev.Timeout,
                                                                       out _);
                                            });

                                            if(!sense)
                                                mediaTest.ReadLong10Data = buffer;
                                        }

                                        if(mediaTest.SupportsReadLong16 == true &&
                                           mediaTest.LongBlockSize      != mediaTest.BlockSize)
                                        {
                                            Core.Spectre.ProgressSingleSpinner(ctx =>
                                            {
                                                ctx.AddTask("Trying SCSI READ LONG (16)...").IsIndeterminate();

                                                sense = dev.ReadLong16(out buffer, out senseBuffer, false, 0,
                                                                       mediaTest.LongBlockSize.Value, dev.Timeout,
                                                                       out _);
                                            });

                                            if(!sense)
                                                mediaTest.ReadLong16Data = buffer;
                                        }
                                    }

                                    mediaTest.MediumTypeName    = mediaType;
                                    mediaTest.MediaIsRecognized = mediaIsRecognized;
                                    mediaTests.Add(mediaTest);

                                    dev.AllowMediumRemoval(out buffer, dev.Timeout, out _);
                                    dev.EjectTray(out buffer, dev.Timeout, out _);
                                }

                                report.SCSI.MultiMediaDevice.TestedMedia = mediaTests;
                            }
                        }

                            break;
                        case PeripheralDeviceTypes.SequentialAccess:
                        {
                            report.SCSI.SequentialDevice = reporter.ReportScsiSsc();

                            List<TestedSequentialMedia> seqTests = new();

                            while(AnsiConsole.Confirm("[italic]Do you have media that you can insert in the drive?[/]"))
                            {
                                AaruConsole.
                                    WriteLine("Please insert it in the drive and press any key when it is ready.");

                                System.Console.ReadKey(true);

                                mediumTypeName =
                                    AnsiConsole.
                                        Ask<string>("Please write a description of the media type and press enter: ");

                                mediumManufacturer =
                                    AnsiConsole.Ask<string>("Please write the media manufacturer and press enter: ");

                                mediumModel = AnsiConsole.Ask<string>("Please write the media model and press enter: ");

                                bool mediaIsRecognized = true;

                                Core.Spectre.ProgressSingleSpinner(ctx =>
                                {
                                    ctx.AddTask("Waiting for drive to become ready").IsIndeterminate();

                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);
                                    AaruConsole.DebugWriteLine("Device reporting", "sense = {0}", sense);

                                    if(!sense)
                                        return;

                                    DecodedSense? decSense = Sense.Decode(senseBuffer);

                                    if(decSense.HasValue)
                                    {
                                        switch(decSense.Value.ASC)
                                        {
                                            case 0x3A:
                                            {
                                                int leftRetries = 50;

                                                while(leftRetries > 0)
                                                {
                                                    Thread.Sleep(2000);

                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                    if(!sense)
                                                        break;

                                                    leftRetries--;
                                                }

                                                AaruConsole.WriteLine();

                                                mediaIsRecognized &= !sense;

                                                break;
                                            }

                                            // These should be trapped by the OS but seems in some cases they're not
                                            case 0x04 when decSense.Value.ASCQ == 0x01:
                                            {
                                                int leftRetries = 50;

                                                while(leftRetries > 0)
                                                {
                                                    Thread.Sleep(2000);

                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                    if(!sense)
                                                        break;

                                                    leftRetries--;
                                                }

                                                AaruConsole.WriteLine();

                                                mediaIsRecognized &= !sense;

                                                break;
                                            }
                                            case 0x28:
                                            {
                                                int leftRetries = 50;

                                                while(leftRetries > 0)
                                                {
                                                    Thread.Sleep(2000);

                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                    if(!sense)
                                                        break;

                                                    leftRetries--;
                                                }

                                                AaruConsole.WriteLine();

                                                mediaIsRecognized &= !sense;

                                                break;
                                            }
                                            default:
                                                AaruConsole.DebugWriteLine("Device-Report command",
                                                                           "Device not ready. Sense {0} ASC {1:X2}h ASCQ {2:X2}h",
                                                                           decSense.Value.SenseKey, decSense.Value.ASC,
                                                                           decSense.Value.ASCQ);

                                                mediaIsRecognized = false;

                                                break;
                                        }
                                    }
                                    else
                                    {
                                        AaruConsole.DebugWriteLine("Device-Report command",
                                                                   "Got sense status but no sense buffer");

                                        mediaIsRecognized = false;
                                    }
                                });

                                var seqTest = new TestedSequentialMedia();

                                if(mediaIsRecognized)
                                    seqTest = reporter.ReportSscMedia();

                                seqTest.MediumTypeName    = mediumTypeName;
                                seqTest.Manufacturer      = mediumManufacturer;
                                seqTest.Model             = mediumModel;
                                seqTest.MediaIsRecognized = mediaIsRecognized;

                                seqTests.Add(seqTest);

                                Core.Spectre.ProgressSingleSpinner(ctx =>
                                {
                                    ctx.AddTask("Asking drive to unload tape (can take a few minutes)...").
                                        IsIndeterminate();

                                    dev.SpcAllowMediumRemoval(out buffer, dev.Timeout, out _);
                                    dev.Unload(out buffer, dev.Timeout, out _);
                                });
                            }

                            report.SCSI.SequentialDevice.TestedMedia = seqTests;
                        }

                            break;
                        case PeripheralDeviceTypes.BridgingExpander
                            when dev.Model.StartsWith("MDM", StringComparison.Ordinal) ||
                                 dev.Model.StartsWith("MDH", StringComparison.Ordinal):
                        {
                            List<string> mediaTypes = new()
                            {
                                "MD DATA (140Mb data MiniDisc)",
                                "60 minutes rewritable MiniDisc",
                                "74 minutes rewritable MiniDisc",
                                "80 minutes rewritable MiniDisc",
                                "Embossed Audio MiniDisc"
                            };

                            mediaTypes.Sort();

                            List<TestedMedia> mediaTests = new();

                            foreach(string mediaType in mediaTypes)
                            {
                                if(!AnsiConsole.
                                       Confirm($"[italic]Do you have a {mediaType} disc that you can insert in the drive?[/]"))
                                    continue;

                                AaruConsole.
                                    WriteLine("Please insert it in the drive and press any key when it is ready.");

                                System.Console.ReadKey(true);

                                bool mediaIsRecognized = true;

                                Core.Spectre.ProgressSingleSpinner(ctx =>
                                {
                                    ctx.AddTask("Waiting for drive to become ready").IsIndeterminate();

                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                    if(!sense)
                                        return;

                                    DecodedSense? decSense = Sense.Decode(senseBuffer);

                                    if(decSense.HasValue)
                                    {
                                        switch(decSense.Value.ASC)
                                        {
                                            case 0x3A:
                                            {
                                                int leftRetries = 50;

                                                while(leftRetries > 0)
                                                {
                                                    Thread.Sleep(2000);

                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                    if(!sense)
                                                        break;

                                                    leftRetries--;
                                                }

                                                mediaIsRecognized &= !sense;

                                                break;
                                            }

                                            // These should be trapped by the OS but seems in some cases they're not
                                            case 0x04 when decSense.Value.ASCQ == 0x01:
                                            {
                                                int leftRetries = 50;

                                                while(leftRetries > 0)
                                                {
                                                    Thread.Sleep(2000);

                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                    if(!sense)
                                                        break;

                                                    leftRetries--;
                                                }

                                                mediaIsRecognized &= !sense;

                                                break;
                                            }
                                            case 0x28:
                                            {
                                                int leftRetries = 50;

                                                while(leftRetries > 0)
                                                {
                                                    Thread.Sleep(2000);

                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                                    if(!sense)
                                                        break;

                                                    leftRetries--;
                                                }

                                                mediaIsRecognized &= !sense;

                                                break;
                                            }
                                            default:
                                                AaruConsole.DebugWriteLine("Device-Report command",
                                                                           "Device not ready. Sense {0}h ASC {1:X2}h ASCQ {2:X2}h",
                                                                           decSense.Value.SenseKey, decSense.Value.ASC,
                                                                           decSense.Value.ASCQ);

                                                mediaIsRecognized = false;

                                                break;
                                        }
                                    }
                                    else
                                    {
                                        AaruConsole.DebugWriteLine("Device-Report command",
                                                                   "Got sense status but no sense buffer");

                                        mediaIsRecognized = false;
                                    }
                                });

                                var mediaTest = new TestedMedia();

                                if(mediaIsRecognized)
                                {
                                    mediaTest = reporter.ReportScsiMedia();

                                    if(mediaTest is null)
                                        continue;

                                    if((mediaTest.SupportsReadLong == true || mediaTest.SupportsReadLong16 == true) &&
                                       mediaTest.LongBlockSize == mediaTest.BlockSize                               &&
                                       AnsiConsole.
                                           Confirm("[italic]Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours)[/]"))
                                    {
                                        AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                                                    Columns(new TaskDescriptionColumn(), new ProgressBarColumn(),
                                                            new PercentageColumn()).Start(ctx =>
                                                    {
                                                        ProgressTask task = ctx.AddTask("Trying to READ LONG...");
                                                        task.MaxValue = ushort.MaxValue;

                                                        for(ushort i = (ushort)mediaTest.BlockSize;; i++)
                                                        {
                                                            task.Value = i;

                                                            task.Description =
                                                                $"Trying to READ LONG with a size of {i} bytes...";

                                                            sense = mediaTest.SupportsReadLong16 == true
                                                                        ? dev.ReadLong16(out buffer, out senseBuffer,
                                                                            false, 0, i, dev.Timeout, out _)
                                                                        : dev.ReadLong10(out buffer, out senseBuffer,
                                                                            false, false, 0, i, dev.Timeout,
                                                                            out _);

                                                            if(!sense)
                                                            {
                                                                mediaTest.LongBlockSize = i;

                                                                break;
                                                            }

                                                            if(i == ushort.MaxValue)
                                                                break;
                                                        }

                                                        AaruConsole.WriteLine();
                                                    });
                                    }

                                    if(mediaTest.SupportsReadLong == true &&
                                       mediaTest.LongBlockSize    != mediaTest.BlockSize)
                                    {
                                        Core.Spectre.ProgressSingleSpinner(ctx =>
                                        {
                                            ctx.AddTask("Trying SCSI READ LONG (10)...").IsIndeterminate();

                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                   (ushort)mediaTest.LongBlockSize, dev.Timeout, out _);
                                        });

                                        if(!sense)
                                            mediaTest.ReadLong10Data = buffer;
                                    }

                                    if(mediaTest.SupportsReadLong16 == true &&
                                       mediaTest.LongBlockSize      != mediaTest.BlockSize)
                                    {
                                        Core.Spectre.ProgressSingleSpinner(ctx =>
                                        {
                                            ctx.AddTask("Trying SCSI READ LONG (16)...").IsIndeterminate();

                                            sense = dev.ReadLong16(out buffer, out senseBuffer, false, 0,
                                                                   (ushort)mediaTest.LongBlockSize, dev.Timeout, out _);
                                        });

                                        if(!sense)
                                            mediaTest.ReadLong16Data = buffer;
                                    }
                                }

                                switch(mediaType)
                                {
                                    case "MD DATA (140Mb data MiniDisc)":
                                        mediaTest.MediumTypeName = "MMD-140A";

                                        break;
                                    case "60 minutes rewritable MiniDisc":
                                        mediaTest.MediumTypeName = "MDW-60";

                                        break;
                                    case "74 minutes rewritable MiniDisc":
                                        mediaTest.MediumTypeName = "MDW-74";

                                        break;
                                    case "80 minutes rewritable MiniDisc":
                                        mediaTest.MediumTypeName = "MDW-80";

                                        break;
                                    case "Embossed Audio MiniDisc":
                                        mediaTest.MediumTypeName = "MiniDisc";

                                        break;
                                }

                                mediaTest.Manufacturer      = "SONY";
                                mediaTest.MediaIsRecognized = mediaIsRecognized;
                                mediaTests.Add(mediaTest);

                                dev.AllowMediumRemoval(out buffer, dev.Timeout, out _);
                                dev.EjectTray(out buffer, dev.Timeout, out _);
                            }

                            report.SCSI.RemovableMedias = mediaTests;
                        }

                            break;
                        default:
                        {
                            if(removable)
                            {
                                List<TestedMedia> mediaTests = new();

                                while(AnsiConsole.
                                    Confirm("[italic]Do you have media that you can insert in the drive?[/]"))
                                {
                                    AaruConsole.
                                        WriteLine("Please insert it in the drive and press any key when it is ready.");

                                    System.Console.ReadKey(true);

                                    mediumTypeName =
                                        AnsiConsole.
                                            Ask<
                                                string>("Please write a description of the media type and press enter: ");

                                    mediumManufacturer =
                                        AnsiConsole.
                                            Ask<string>("Please write the media manufacturer and press enter: ");

                                    mediumModel =
                                        AnsiConsole.Ask<string>("Please write the media model and press enter: ");

                                    bool mediaIsRecognized = true;

                                    Core.Spectre.ProgressSingleSpinner(ctx =>
                                    {
                                        ctx.AddTask("Waiting for drive to become ready").IsIndeterminate();
                                        sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

                                        if(!sense)
                                            return;

                                        DecodedSense? decSense = Sense.Decode(senseBuffer);

                                        if(decSense.HasValue)
                                            switch(decSense.Value.ASC)
                                            {
                                                case 0x3A:
                                                {
                                                    int leftRetries = 20;

                                                    while(leftRetries > 0)
                                                    {
                                                        Thread.Sleep(2000);

                                                        sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout,
                                                            out _);

                                                        if(!sense)
                                                            break;

                                                        leftRetries--;
                                                    }

                                                    mediaIsRecognized &= !sense;

                                                    break;
                                                }
                                                case 0x04 when decSense.Value.ASCQ == 0x01:
                                                {
                                                    int leftRetries = 20;

                                                    while(leftRetries > 0)
                                                    {
                                                        Thread.Sleep(2000);

                                                        sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout,
                                                            out _);

                                                        if(!sense)
                                                            break;

                                                        leftRetries--;
                                                    }

                                                    mediaIsRecognized &= !sense;

                                                    break;
                                                }
                                                default:
                                                    mediaIsRecognized = false;

                                                    break;
                                            }
                                        else
                                            mediaIsRecognized = false;
                                    });

                                    var mediaTest = new TestedMedia();

                                    if(mediaIsRecognized)
                                    {
                                        mediaTest = reporter.ReportScsiMedia();

                                        if((mediaTest.SupportsReadLong   == true ||
                                            mediaTest.SupportsReadLong16 == true)         &&
                                           mediaTest.LongBlockSize == mediaTest.BlockSize &&
                                           AnsiConsole.
                                               Confirm("[italic]Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours)[/]"))
                                        {
                                            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                                                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(),
                                                                new PercentageColumn()).Start(ctx =>
                                                        {
                                                            ProgressTask task = ctx.AddTask("Trying to READ LONG...");
                                                            task.MaxValue = ushort.MaxValue;

                                                            for(ushort i = (ushort)mediaTest.BlockSize;; i++)
                                                            {
                                                                task.Value = i;

                                                                task.Description =
                                                                    $"Trying to READ LONG with a size of {i} bytes...";

                                                                sense = mediaTest.SupportsReadLong16 == true
                                                                            ? dev.ReadLong16(out buffer,
                                                                                out senseBuffer, false, 0, i,
                                                                                dev.Timeout, out _)
                                                                            : dev.ReadLong10(out buffer,
                                                                                out senseBuffer, false, false, 0,
                                                                                i, dev.Timeout, out _);

                                                                if(!sense)
                                                                {
                                                                    mediaTest.LongBlockSize = i;

                                                                    break;
                                                                }

                                                                if(i == ushort.MaxValue)
                                                                    break;
                                                            }

                                                            AaruConsole.WriteLine();
                                                        });
                                        }

                                        if(mediaTest.SupportsReadLong == true &&
                                           mediaTest.LongBlockSize    != mediaTest.BlockSize)
                                        {
                                            Core.Spectre.ProgressSingleSpinner(ctx =>
                                            {
                                                ctx.AddTask("Trying SCSI READ LONG (10)...").IsIndeterminate();

                                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                       (ushort)mediaTest.LongBlockSize, dev.Timeout,
                                                                       out _);
                                            });

                                            if(!sense)
                                                mediaTest.ReadLong10Data = buffer;
                                        }

                                        if(mediaTest.SupportsReadLong16 == true &&
                                           mediaTest.LongBlockSize      != mediaTest.BlockSize)
                                        {
                                            Core.Spectre.ProgressSingleSpinner(ctx =>
                                            {
                                                ctx.AddTask("Trying SCSI READ LONG (16)...").IsIndeterminate();

                                                sense = dev.ReadLong16(out buffer, out senseBuffer, false, 0,
                                                                       (ushort)mediaTest.LongBlockSize, dev.Timeout,
                                                                       out _);
                                            });

                                            if(!sense)
                                                mediaTest.ReadLong16Data = buffer;
                                        }
                                    }

                                    mediaTest.MediumTypeName    = mediumTypeName;
                                    mediaTest.Manufacturer      = mediumManufacturer;
                                    mediaTest.Model             = mediumModel;
                                    mediaTest.MediaIsRecognized = mediaIsRecognized;

                                    mediaTests.Add(mediaTest);

                                    dev.AllowMediumRemoval(out buffer, dev.Timeout, out _);
                                    dev.EjectTray(out buffer, dev.Timeout, out _);
                                }

                                report.SCSI.RemovableMedias = mediaTests;
                            }
                            else
                            {
                                report.SCSI.ReadCapabilities = reporter.ReportScsi();

                                if((report.SCSI.ReadCapabilities.SupportsReadLong   == true ||
                                    report.SCSI.ReadCapabilities.SupportsReadLong16 == true) &&
                                   report.SCSI.ReadCapabilities.LongBlockSize == report.SCSI.ReadCapabilities.BlockSize)
                                {
                                    if(AnsiConsole.
                                        Confirm("[italic]Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours)[/]"))
                                    {
                                        AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                                                    Columns(new TaskDescriptionColumn(), new ProgressBarColumn(),
                                                            new PercentageColumn()).Start(ctx =>
                                                    {
                                                        ProgressTask task = ctx.AddTask("Trying to READ LONG...");
                                                        task.MaxValue = ushort.MaxValue;

                                                        for(ushort i = (ushort)report.SCSI.ReadCapabilities.BlockSize;;
                                                            i++)
                                                        {
                                                            task.Value = i;

                                                            task.Description =
                                                                $"Trying to READ LONG with a size of {i} bytes...";

                                                            sense =
                                                                report.SCSI.ReadCapabilities.SupportsReadLong16 == true
                                                                    ? dev.ReadLong16(out buffer, out senseBuffer, false,
                                                                        0, i, dev.Timeout, out _)
                                                                    : dev.ReadLong10(out buffer, out senseBuffer, false,
                                                                        false, 0, i, dev.Timeout, out _);

                                                            if(!sense)
                                                            {
                                                                report.SCSI.ReadCapabilities.LongBlockSize = i;

                                                                break;
                                                            }

                                                            if(i == ushort.MaxValue)
                                                                break;
                                                        }

                                                        AaruConsole.WriteLine();
                                                    });
                                    }
                                }

                                if(report.SCSI.ReadCapabilities.SupportsReadLong == true &&
                                   report.SCSI.ReadCapabilities.LongBlockSize != report.SCSI.ReadCapabilities.BlockSize)
                                {
                                    Core.Spectre.ProgressSingleSpinner(ctx =>
                                    {
                                        ctx.AddTask("Trying SCSI READ LONG (10)...").IsIndeterminate();

                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                               (ushort)report.SCSI.ReadCapabilities.LongBlockSize,
                                                               dev.Timeout, out _);
                                    });

                                    if(!sense)
                                        report.SCSI.ReadCapabilities.ReadLong10Data = buffer;
                                }

                                if(report.SCSI.ReadCapabilities.SupportsReadLong16 == true &&
                                   report.SCSI.ReadCapabilities.LongBlockSize != report.SCSI.ReadCapabilities.BlockSize)
                                {
                                    Core.Spectre.ProgressSingleSpinner(ctx =>
                                    {
                                        ctx.AddTask("Trying SCSI READ LONG (16)...").IsIndeterminate();

                                        sense = dev.ReadLong16(out buffer, out senseBuffer, false, 0,
                                                               report.SCSI.ReadCapabilities.LongBlockSize.Value,
                                                               dev.Timeout, out _);
                                    });

                                    if(!sense)
                                        report.SCSI.ReadCapabilities.ReadLong16Data = buffer;
                                }
                            }

                            break;
                        }
                    }

                    break;
                default: throw new NotSupportedException("Unknown device type.");
            }

            var jsonFs = new FileStream(jsonFile, FileMode.Create);
            var jsonSw = new StreamWriter(jsonFs);

            jsonSw.Write(JsonConvert.SerializeObject(report, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }));

            jsonSw.Close();
            jsonFs.Close();

            using(var ctx = AaruContext.Create(Settings.Settings.LocalDbPath))
            {
                ctx.Reports.Add(new Report(report));
                ctx.SaveChanges();
            }

            // TODO:
            if(Settings.Settings.Current.ShareReports)
                Remote.SubmitReport(report);

            return (int)ErrorNumber.NoError;
        }
    }
}
