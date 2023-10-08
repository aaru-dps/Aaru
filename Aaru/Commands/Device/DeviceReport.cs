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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Text.Json;
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
using Aaru.Helpers;
using Aaru.Localization;
using Spectre.Console;
using Command = System.CommandLine.Command;
using Profile = Aaru.Decoders.SCSI.MMC.Profile;

namespace Aaru.Commands.Device;

sealed class DeviceReportCommand : Command
{
    const string MODULE_NAME = "Device-Report command";

    public DeviceReportCommand() : base("report", UI.Device_Report_Command_Description)
    {
        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Device_path,
            Name        = "device-path"
        });

        Add(new Option<bool>(new[]
        {
            "--trap-disc", "-t"
        }, () => false, UI.Device_report_using_trap_disc));

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());
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

            AaruConsole.WriteExceptionEvent += ex => { stderrConsole.WriteException(ex); };
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

        Statistics.AddCommand("device-report");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",   debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--device={0}",  Markup.Escape(devicePath ?? ""));
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}", verbose);

        if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
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

        bool isAdmin = dev is Devices.Remote.Device remoteDev2 ? remoteDev2.IsAdmin : DetectOS.IsAdmin;

        if(!isAdmin)
        {
            AaruConsole.ErrorWriteLine(UI.Device_report_must_be_run_as_admin);

            AaruConsole.ErrorWriteLine(UI.Not_continuing);

            return (int)ErrorNumber.NotPermitted;
        }

        var report = new DeviceReport
        {
            Manufacturer = dev.Manufacturer,
            Model        = dev.Model,
            Revision     = dev.FirmwareRevision,
            Type         = dev.Type
        };

        var    removable = false;
        string jsonFile;

        switch(string.IsNullOrWhiteSpace(dev.Manufacturer))
        {
            case false when !string.IsNullOrWhiteSpace(dev.FirmwareRevision):
                jsonFile = dev.Manufacturer + "_" + dev.Model + "_" + dev.FirmwareRevision + ".json";

                break;
            case false:
                jsonFile = dev.Manufacturer + "_" + dev.Model + ".json";

                break;
            default:
            {
                if(!string.IsNullOrWhiteSpace(dev.FirmwareRevision))
                    jsonFile = dev.Model + "_" + dev.FirmwareRevision + ".json";
                else
                    jsonFile = dev.Model + ".json";

                break;
            }
        }

        jsonFile = jsonFile.Replace('\\', '_').Replace('/', '_').Replace('?', '_');

        if(trapDisc && dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
        {
            AaruConsole.ErrorWriteLine(UI.Device_does_not_report_with_trap_discs);

            return (int)ErrorNumber.InvalidArgument;
        }

        var reporter = new Core.Devices.Report.DeviceReport(dev);

        if(dev.IsUsb)
        {
            if(AnsiConsole.Confirm($"[italic]{UI.Is_the_device_natively_USB}[/]"))
            {
                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(Localization.Core.Querying_USB_information).IsIndeterminate();
                    report.USB = reporter.UsbReport();
                });

                report.USB.RemovableMedia = AnsiConsole.Confirm($"[italic]{UI.Is_the_media_removable}[/]");

                removable = report.USB.RemovableMedia;
            }
        }

        if(dev.IsFireWire)
        {
            if(AnsiConsole.Confirm($"[italic]{UI.Is_the_device_natively_FireWire}[/]"))
            {
                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying FireWire information...").IsIndeterminate();
                    report.FireWire = reporter.FireWireReport();
                });

                report.FireWire.RemovableMedia = AnsiConsole.Confirm($"[italic]{UI.Is_the_media_removable}[/]");

                removable = report.FireWire.RemovableMedia;
            }
        }

        if(dev.IsPcmcia)
        {
            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(Localization.Core.Querying_PCMCIA_information).IsIndeterminate();
                report.PCMCIA = reporter.PcmciaReport();
            });
        }

        byte[] buffer = Array.Empty<byte>();
        string mediumTypeName;
        string mediumModel;

        switch(dev.Type)
        {
            case DeviceType.ATA:
            {
                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(Localization.Core.Querying_ATA_IDENTIFY).IsIndeterminate();
                    dev.AtaIdentify(out buffer, out _, dev.Timeout, out _);
                });

                if(!Identify.Decode(buffer).HasValue)
                    break;

                report.ATA = new Ata
                {
                    Identify = Core.Devices.Report.DeviceReport.ClearIdentify(buffer)
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
                                                                                    Removable) ==
                        true)
                    removable = AnsiConsole.Confirm($"[italic]{UI.Is_the_media_removable}[/]");

                if(removable)
                {
                    AaruConsole.WriteLine(UI.Please_remove_any_media);

                    System.Console.ReadKey(true);

                    Core.Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask(Localization.Core.Querying_ATA_IDENTIFY).IsIndeterminate();
                        dev.AtaIdentify(out buffer, out _, dev.Timeout, out _);
                    });

                    report.ATA.Identify = Core.Devices.Report.DeviceReport.ClearIdentify(buffer);
                    List<TestedMedia> mediaTests = new();

                    while(AnsiConsole.Confirm($"[italic]{UI.Do_you_have_media_you_can_insert}[/]"))
                    {
                        AaruConsole.WriteLine(UI.Please_insert_it_in_the_drive);
                        System.Console.ReadKey(true);

                        mediumTypeName =
                            AnsiConsole.Ask<string>(Localization.Core.Please_write_description_of_media_type);

                        mediumModel = AnsiConsole.Ask<string>(Localization.Core.Please_write_media_model);

                        TestedMedia mediaTest = reporter.ReportAtaMedia();
                        mediaTest.MediumTypeName = mediumTypeName;
                        mediaTest.Model          = mediumModel;

                        mediaTests.Add(mediaTest);
                    }

                    report.ATA.RemovableMedias = mediaTests;
                }
                else
                    report.ATA.ReadCapabilities = reporter.ReportAta(report.ATA.IdentifyDevice.Value);

                break;
            }

            case DeviceType.MMC:
                report.MultiMediaCard = reporter.MmcSdReport();

                break;
            case DeviceType.SecureDigital:
                report.SecureDigital = reporter.MmcSdReport();

                break;
            case DeviceType.NVMe:
                throw new NotImplementedException(Localization.Core.NVMe_devices_not_yet_supported);
            case DeviceType.ATAPI:
                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(Localization.Core.Querying_ATAPI_IDENTIFY).IsIndeterminate();
                    dev.AtapiIdentify(out buffer, out _, dev.Timeout, out _);
                });

                if(Identify.Decode(buffer).HasValue)
                {
                    report.ATAPI = new Ata
                    {
                        Identify = Core.Devices.Report.DeviceReport.ClearIdentify(buffer)
                    };
                }

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
                    case PeripheralDeviceTypes.SCSIZonedBlockDevice:
                        break;
                    default:
                    {
                        AaruConsole.ErrorWriteLine(UI.Unsupported_device_type_for_report);

                        throw new IOException();
                    }
                }

                if(!dev.IsUsb && !dev.IsFireWire && dev.IsRemovable)
                    removable = AnsiConsole.Confirm($"[italic]{UI.Is_the_media_removable_flash_is_not}[/]");

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
                                ctx.AddTask(UI.Asking_drive_to_unload_tape).IsIndeterminate();

                                dev.Unload(out buffer, dev.Timeout, out _);
                            });

                            break;
                    }

                    AaruConsole.WriteLine(UI.Please_remove_any_media);

                    System.Console.ReadKey(true);
                }

                report.SCSI = reporter.ReportScsiInquiry();

                if(report.SCSI == null)
                    break;

                report.SCSI.EVPDPages =
                    reporter.ReportEvpdPages(StringHandlers.CToString(report.SCSI.Inquiry?.VendorIdentification)?.
                                                            Trim().
                                                            ToLowerInvariant());

                reporter.ReportScsiModes(ref report, out byte[] cdromMode, out MediumTypes mediumType);

                string mediumManufacturer;
                byte[] senseBuffer = Array.Empty<byte>();
                var    sense       = true;

                switch(dev.ScsiType)
                {
                    case PeripheralDeviceTypes.MultiMediaDevice:
                    {
                        if(dev.IsUsb &&
                           mediumType is MediumTypes.UnknownBlockDevice or MediumTypes.ReadOnlyBlockDevice
                                                                        or MediumTypes.ReadWriteBlockDevice)
                            goto default;

                        bool iomegaRev =
                            dev.Manufacturer.Equals("iomega", StringComparison.InvariantCultureIgnoreCase) &&
                            dev.Model.StartsWith("rrd", StringComparison.InvariantCultureIgnoreCase);

                        if(trapDisc)
                        {
                            if(iomegaRev)
                            {
                                AaruConsole.ErrorWriteLine(UI.Device_does_not_report_with_trap_discs);

                                return (int)ErrorNumber.InvalidArgument;
                            }

                            if(!AnsiConsole.Confirm($"[italic]{UI.Sure_report_trap_disc}[/]"))
                                return (int)ErrorNumber.NoError;

                            if(!AnsiConsole.Confirm($"[italic]{UI.Do_you_have_audio_trap_disc}[/]"))
                            {
                                AaruConsole.ErrorWriteLine(UI.Please_burn_audio_trap_disc);

                                return (int)ErrorNumber.NoError;
                            }

                            if(AnsiConsole.Confirm($"[italic]{UI.Do_you_have_GD_ROM_disc}[/]"))
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
                                    foreach(Profile prof in from desc in ftr.Descriptors
                                                            where desc.Code == 0x0000
                                                            select Features.Decode_0000(desc.Data)
                                                            into ftr0000
                                                            where ftr0000 != null
                                                            from prof in ftr0000.Value.Profiles
                                                            select prof)
                                    {
                                        switch(prof.Number)
                                        {
                                            case ProfileNumber.CDROM:
                                            case ProfileNumber.CDR:
                                            case ProfileNumber.CDRW:
                                                mediaTypes.Add("CD-ROM");
                                                mediaTypes.Add("Audio CD");
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

                            if(cdromMode != null && !iomegaRev)
                            {
                                mediaTypes.Add("CD-ROM");
                                mediaTypes.Add("Audio CD");
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

                            if(report.SCSI.MultiMediaDevice.Features != null && !iomegaRev)
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
                                    mediaTypes.Add("Audio CD");
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
                            if(mediaTypes.Count == 0 || mediaTypes.Contains("CD-ROM"))
                            {
                                mediaTypes.Add("CD-ROM");
                                mediaTypes.Add("Audio CD");
                                mediaTypes.Add("CD-R");
                                mediaTypes.Add("CD-RW Ultra Speed (marked 16x or higher)");
                                mediaTypes.Add("CD-RW High Speed (marked between 8x and 12x)");
                                mediaTypes.Add("CD-RW (marked 4x or lower)");
                                mediaTypes.Add("Enhanced CD (aka E-CD, CD-Plus or CD+)");
                            }

                            mediaTypes = mediaTypes.Distinct().ToList();
                            mediaTypes.Sort();

                            bool tryPlextor      = false,
                                 tryHldtst       = false,
                                 tryPioneer      = false,
                                 tryNec          = false,
                                 tryMediaTekF106 = false;

                            tryPlextor |=
                                dev.Manufacturer.Equals("plextor", StringComparison.InvariantCultureIgnoreCase);
                            tryHldtst |=
                                dev.Manufacturer.Equals("hl-dt-st", StringComparison.InvariantCultureIgnoreCase);
                            tryPioneer |=
                                dev.Manufacturer.Equals("pioneer", StringComparison.InvariantCultureIgnoreCase);
                            tryNec |= dev.Manufacturer.Equals("nec", StringComparison.InvariantCultureIgnoreCase);

                            if(!iomegaRev)
                            {
                                if(!tryPlextor)
                                {
                                    tryPlextor |=
                                        AnsiConsole.
                                            Confirm($"[italic]{UI.Do_you_want_to_try_Plextor_commands} [red]{UI.This_is_dangerous}[/][/]",
                                                    false);
                                }

                                if(!tryNec)
                                {
                                    tryNec |=
                                        AnsiConsole.
                                            Confirm($"[italic]{UI.Do_you_want_to_try_NEC_commands} [red]{UI.This_is_dangerous}[/][/]",
                                                    false);
                                }

                                if(!tryPioneer)
                                {
                                    tryPioneer |=
                                        AnsiConsole.
                                            Confirm($"[italic]{UI.Do_you_want_to_try_Pioneer_commands} [red]{UI.This_is_dangerous}[/][/]",
                                                    false);
                                }

                                if(!tryHldtst)
                                {
                                    tryHldtst |=
                                        AnsiConsole.
                                            Confirm($"[italic]{UI.Do_you_want_to_try_HLDTST_commands} [red]{UI.This_is_dangerous}[/][/]",
                                                    false);
                                }

                                tryMediaTekF106 =
                                    AnsiConsole.
                                        Confirm($"[italic]{UI.Do_you_want_to_try_MediaTek_commands} [red]{UI.This_is_dangerous}[/][/]",
                                                false);
                            }

                            if(dev.Model.StartsWith("PD-", StringComparison.Ordinal))
                                mediaTypes.Add("PD-650");

                            List<TestedMedia> mediaTests = new();

                            foreach(string mediaType in mediaTypes)
                            {
                                if(!AnsiConsole.Confirm($"[italic]{string.Format(UI.Do_you_have_a_0_disc, mediaType)
                                }[/]"))
                                    continue;

                                AaruConsole.WriteLine(UI.Please_insert_it_in_the_drive);

                                System.Console.ReadKey(true);

                                var mediaIsRecognized = true;

                                Core.Spectre.ProgressSingleSpinner(ctx =>
                                {
                                    ctx.AddTask(Localization.Core.Waiting_for_drive_to_become_ready).IsIndeterminate();
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
                                                var leftRetries = 50;

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
                                                var leftRetries = 50;

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
                                                var leftRetries = 50;

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
                                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                                           Localization.Core.Device_not_ready_Sense,
                                                                           decSense.Value.SenseKey, decSense.Value.ASC,
                                                                           decSense.Value.ASCQ);

                                                mediaIsRecognized = false;

                                                break;
                                        }
                                    }
                                    else
                                    {
                                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                                   Localization.Core.
                                                                       Got_sense_status_but_no_sense_buffer);

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

                                    if((mediaTest.SupportsReadLong == true || mediaTest.SupportsReadLong16 == true) &&
                                       mediaTest.LongBlockSize == mediaTest.BlockSize                               &&
                                       AnsiConsole.Confirm($"[italic]{Localization.Core.Try_to_find_SCSI_READ_LONG_size
                                       }[/]"))
                                    {
                                        AnsiConsole.Progress().
                                                    AutoClear(true).
                                                    HideCompleted(true).
                                                    Columns(new TaskDescriptionColumn(), new ProgressBarColumn(),
                                                            new PercentageColumn()).
                                                    Start(ctx =>
                                                    {
                                                        ProgressTask task =
                                                            ctx.AddTask(Localization.Core.Trying_READ_LONG);

                                                        task.MaxValue = ushort.MaxValue;

                                                        for(var i = (ushort)(mediaTest.BlockSize ?? 0);; i++)
                                                        {
                                                            task.Description =
                                                                string.
                                                                    Format(Localization.Core.Trying_READ_LONG_with_size_0,
                                                                           i);

                                                            task.Value = i;

                                                            sense = mediaTest.SupportsReadLong16 == true
                                                                        ? dev.ReadLong16(out buffer, out senseBuffer,
                                                                            false, 0, i,
                                                                            dev.Timeout, out _)
                                                                        : dev.ReadLong10(out buffer, out senseBuffer,
                                                                            false, false,
                                                                            0, i, dev.Timeout, out _);

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
                                            ctx.AddTask(Localization.Core.Trying_SCSI_READ_LONG_10).IsIndeterminate();

                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                   (ushort)(mediaTest.LongBlockSize ??
                                                                            mediaTest.BlockSize ?? 0), dev.Timeout,
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
                                            ctx.AddTask(Localization.Core.Trying_SCSI_READ_LONG_16).IsIndeterminate();

                                            sense = dev.ReadLong16(out buffer, out senseBuffer, false, 0,
                                                                   mediaTest.LongBlockSize ?? mediaTest.BlockSize ?? 0,
                                                                   dev.Timeout, out _);
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

                        while(AnsiConsole.Confirm($"[italic]{UI.Do_you_have_media_you_can_insert}[/]"))
                        {
                            AaruConsole.WriteLine(UI.Please_insert_it_in_the_drive);

                            System.Console.ReadKey(true);

                            mediumTypeName =
                                AnsiConsole.Ask<string>(Localization.Core.Please_write_description_of_media_type);

                            mediumManufacturer =
                                AnsiConsole.Ask<string>(Localization.Core.Please_write_media_manufacturer);

                            mediumModel = AnsiConsole.Ask<string>(Localization.Core.Please_write_media_model);

                            var mediaIsRecognized = true;

                            Core.Spectre.ProgressSingleSpinner(ctx =>
                            {
                                ctx.AddTask(Localization.Core.Waiting_for_drive_to_become_ready).IsIndeterminate();

                                sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);
                                AaruConsole.DebugWriteLine(MODULE_NAME, "sense = {0}", sense);

                                if(!sense)
                                    return;

                                DecodedSense? decSense = Sense.Decode(senseBuffer);

                                if(decSense.HasValue)
                                {
                                    switch(decSense.Value.ASC)
                                    {
                                        case 0x3A:
                                        {
                                            var leftRetries = 50;

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
                                            var leftRetries = 50;

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
                                            var leftRetries = 50;

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
                                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                                       Localization.Core.Device_not_ready_Sense,
                                                                       decSense.Value.SenseKey, decSense.Value.ASC,
                                                                       decSense.Value.ASCQ);

                                            mediaIsRecognized = false;

                                            break;
                                    }
                                }
                                else
                                {
                                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                                               Localization.Core.Got_sense_status_but_no_sense_buffer);

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
                                ctx.AddTask(UI.Asking_drive_to_unload_tape).IsIndeterminate();

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
                            Localization.Core.Media_Type_Name_MMD_140A,
                            Localization.Core.Media_Type_Name_MDW_60,
                            Localization.Core.Media_Type_Name_MDW_74,
                            Localization.Core.Media_Type_Name_MDW_80,
                            Localization.Core.Media_Type_Name_MiniDisc
                        };

                        mediaTypes.Sort();

                        List<TestedMedia> mediaTests = new();

                        foreach(string mediaType in mediaTypes)
                        {
                            if(!AnsiConsole.Confirm($"[italic]{string.Format(UI.Do_you_have_a_0_disc, mediaType)}[/]"))
                                continue;

                            AaruConsole.WriteLine(UI.Please_insert_it_in_the_drive);

                            System.Console.ReadKey(true);

                            var mediaIsRecognized = true;

                            Core.Spectre.ProgressSingleSpinner(ctx =>
                            {
                                ctx.AddTask(Localization.Core.Waiting_for_drive_to_become_ready).IsIndeterminate();

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
                                            var leftRetries = 50;

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
                                            var leftRetries = 50;

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
                                            var leftRetries = 50;

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
                                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                                       Localization.Core.Device_not_ready_Sense,
                                                                       decSense.Value.SenseKey, decSense.Value.ASC,
                                                                       decSense.Value.ASCQ);

                                            mediaIsRecognized = false;

                                            break;
                                    }
                                }
                                else
                                {
                                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                                               Localization.Core.Got_sense_status_but_no_sense_buffer);

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
                                   AnsiConsole.Confirm($"[italic]{Localization.Core.Try_to_find_SCSI_READ_LONG_size
                                   }[/]"))
                                {
                                    AnsiConsole.Progress().
                                                AutoClear(true).
                                                HideCompleted(true).
                                                Columns(new TaskDescriptionColumn(), new ProgressBarColumn(),
                                                        new PercentageColumn()).
                                                Start(ctx =>
                                                {
                                                    ProgressTask task = ctx.AddTask(Localization.Core.Trying_READ_LONG);

                                                    task.MaxValue = ushort.MaxValue;

                                                    for(var i = (ushort)(mediaTest.BlockSize ?? 0);; i++)
                                                    {
                                                        task.Value = i;

                                                        task.Description =
                                                            string.
                                                                Format(Localization.Core.Trying_READ_LONG_with_size_0,
                                                                       i);

                                                        sense = mediaTest.SupportsReadLong16 == true
                                                                    ? dev.ReadLong16(out buffer, out senseBuffer, false,
                                                                        0, i,
                                                                        dev.Timeout, out _)
                                                                    : dev.ReadLong10(out buffer, out senseBuffer, false,
                                                                        false, 0,
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

                                if(mediaTest.SupportsReadLong == true && mediaTest.LongBlockSize != mediaTest.BlockSize)
                                {
                                    Core.Spectre.ProgressSingleSpinner(ctx =>
                                    {
                                        ctx.AddTask(Localization.Core.Trying_SCSI_READ_LONG_10).IsIndeterminate();

                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                               (ushort)(mediaTest.LongBlockSize ??
                                                                        mediaTest.BlockSize ?? 0), dev.Timeout, out _);
                                    });

                                    if(!sense)
                                        mediaTest.ReadLong10Data = buffer;
                                }

                                if(mediaTest.SupportsReadLong16 == true &&
                                   mediaTest.LongBlockSize      != mediaTest.BlockSize)
                                {
                                    Core.Spectre.ProgressSingleSpinner(ctx =>
                                    {
                                        ctx.AddTask(Localization.Core.Trying_SCSI_READ_LONG_16).IsIndeterminate();

                                        sense = dev.ReadLong16(out buffer, out senseBuffer, false, 0,
                                                               (ushort)(mediaTest.LongBlockSize ??
                                                                        mediaTest.BlockSize ?? 0), dev.Timeout, out _);
                                    });

                                    if(!sense)
                                        mediaTest.ReadLong16Data = buffer;
                                }
                            }

                            if(mediaType == Localization.Core.Media_Type_Name_MMD_140A)
                                mediaTest.MediumTypeName = "MMD-140A";
                            else if(mediaType == Localization.Core.Media_Type_Name_MDW_60)
                                mediaTest.MediumTypeName = "MDW-60";
                            else if(mediaType == Localization.Core.Media_Type_Name_MDW_74)
                                mediaTest.MediumTypeName = "MDW-74";
                            else if(mediaType == Localization.Core.Media_Type_Name_MDW_80)
                                mediaTest.MediumTypeName = "MDW-80";
                            else if(mediaType == Localization.Core.Media_Type_Name_MiniDisc)
                                mediaTest.MediumTypeName = "MiniDisc";

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

                            while(AnsiConsole.Confirm($"[italic]{UI.Do_you_have_media_you_can_insert}[/]"))
                            {
                                AaruConsole.WriteLine(UI.Please_insert_it_in_the_drive);

                                System.Console.ReadKey(true);

                                mediumTypeName =
                                    AnsiConsole.Ask<string>(Localization.Core.Please_write_description_of_media_type);

                                mediumManufacturer =
                                    AnsiConsole.Ask<string>(Localization.Core.Please_write_media_manufacturer);

                                mediumModel = AnsiConsole.Ask<string>(Localization.Core.Please_write_media_model);

                                var mediaIsRecognized = true;

                                Core.Spectre.ProgressSingleSpinner(ctx =>
                                {
                                    ctx.AddTask(Localization.Core.Waiting_for_drive_to_become_ready).IsIndeterminate();
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
                                                var leftRetries = 20;

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
                                            case 0x04 when decSense.Value.ASCQ == 0x01:
                                            {
                                                var leftRetries = 20;

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
                                                mediaIsRecognized = false;

                                                break;
                                        }
                                    }
                                    else
                                        mediaIsRecognized = false;
                                });

                                var mediaTest = new TestedMedia();

                                if(mediaIsRecognized)
                                {
                                    mediaTest = reporter.ReportScsiMedia();

                                    if((mediaTest.SupportsReadLong == true || mediaTest.SupportsReadLong16 == true) &&
                                       mediaTest.LongBlockSize == mediaTest.BlockSize                               &&
                                       AnsiConsole.Confirm($"[italic]{Localization.Core.Try_to_find_SCSI_READ_LONG_size
                                       }[/]"))
                                    {
                                        AnsiConsole.Progress().
                                                    AutoClear(true).
                                                    HideCompleted(true).
                                                    Columns(new TaskDescriptionColumn(), new ProgressBarColumn(),
                                                            new PercentageColumn()).
                                                    Start(ctx =>
                                                    {
                                                        ProgressTask task =
                                                            ctx.AddTask(Localization.Core.Trying_READ_LONG);

                                                        task.MaxValue = ushort.MaxValue;

                                                        for(var i = (ushort)(mediaTest.BlockSize ?? 0);; i++)
                                                        {
                                                            task.Value = i;

                                                            task.Description =
                                                                string.
                                                                    Format(Localization.Core.Trying_READ_LONG_with_size_0,
                                                                           i);

                                                            sense = mediaTest.SupportsReadLong16 == true
                                                                        ? dev.ReadLong16(out buffer, out senseBuffer,
                                                                            false, 0, i,
                                                                            dev.Timeout, out _)
                                                                        : dev.ReadLong10(out buffer, out senseBuffer,
                                                                            false, false,
                                                                            0, i, dev.Timeout, out _);

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
                                            ctx.AddTask(Localization.Core.Trying_SCSI_READ_LONG_10).IsIndeterminate();

                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                   (ushort)(mediaTest.LongBlockSize ??
                                                                            mediaTest.BlockSize ?? 0), dev.Timeout,
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
                                            ctx.AddTask(Localization.Core.Trying_SCSI_READ_LONG_16).IsIndeterminate();

                                            sense = dev.ReadLong16(out buffer, out senseBuffer, false, 0,
                                                                   (ushort)(mediaTest.LongBlockSize ??
                                                                            mediaTest.BlockSize ?? 0), dev.Timeout,
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
                                if(AnsiConsole.Confirm($"[italic]{Localization.Core.Try_to_find_SCSI_READ_LONG_size
                                }[/]"))
                                {
                                    AnsiConsole.Progress().
                                                AutoClear(true).
                                                HideCompleted(true).
                                                Columns(new TaskDescriptionColumn(), new ProgressBarColumn(),
                                                        new PercentageColumn()).
                                                Start(ctx =>
                                                {
                                                    ProgressTask task = ctx.AddTask(Localization.Core.Trying_READ_LONG);

                                                    task.MaxValue = ushort.MaxValue;

                                                    for(var i = (ushort)(report.SCSI.ReadCapabilities.BlockSize ?? 0);;
                                                        i++)
                                                    {
                                                        task.Value = i;

                                                        task.Description =
                                                            string.
                                                                Format(Localization.Core.Trying_READ_LONG_with_size_0,
                                                                       i);

                                                        sense = report.SCSI.ReadCapabilities.SupportsReadLong16 == true
                                                                    ? dev.ReadLong16(out buffer, out senseBuffer, false,
                                                                        0, i,
                                                                        dev.Timeout, out _)
                                                                    : dev.ReadLong10(out buffer, out senseBuffer, false,
                                                                        false, 0,
                                                                        i, dev.Timeout, out _);

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
                               report.SCSI.ReadCapabilities.LongBlockSize    != report.SCSI.ReadCapabilities.BlockSize)
                            {
                                Core.Spectre.ProgressSingleSpinner(ctx =>
                                {
                                    ctx.AddTask(Localization.Core.Trying_SCSI_READ_LONG_10).IsIndeterminate();

                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                           (ushort)(report.SCSI.ReadCapabilities.LongBlockSize ??
                                                                    report.SCSI.ReadCapabilities.BlockSize ?? 0),
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
                                    ctx.AddTask(Localization.Core.Trying_SCSI_READ_LONG_16).IsIndeterminate();

                                    sense = dev.ReadLong16(out buffer, out senseBuffer, false, 0,
                                                           report.SCSI.ReadCapabilities.LongBlockSize ??
                                                           report.SCSI.ReadCapabilities.BlockSize ?? 0, dev.Timeout,
                                                           out _);
                                });

                                if(!sense)
                                    report.SCSI.ReadCapabilities.ReadLong16Data = buffer;
                            }
                        }

                        break;
                    }
                }

                break;
            default:
                throw new NotSupportedException(Localization.Core.Unknown_device_type);
        }

        var jsonFs = new FileStream(jsonFile, FileMode.Create);

        JsonSerializer.Serialize(jsonFs, report, typeof(DeviceReport), DeviceReportContext.Default);

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