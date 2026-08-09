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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Core;
using Aaru.Core.Devices.Dumping;
using Aaru.Core.Logging;
using Aaru.Core.Media.Info;
using Aaru.Database;
using Aaru.Database.Models;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Decoders.Xbox;
using Aaru.Localization;
using Aaru.Logging;
using Humanizer;
using Spectre.Console;
using Spectre.Console.Cli;
using BCA = Aaru.Decoders.Bluray.BCA;
using Cartridge = Aaru.Decoders.DVD.Cartridge;
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using Session = Aaru.Decoders.CD.Session;
using Spare = Aaru.Decoders.DVD.Spare;

namespace Aaru.Commands.Media;

sealed class MediaInfoCommand : Command<MediaInfoCommand.Settings>
{
    const string MODULE_NAME = "Media-Info command";

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)

    {
        MainClass.PrintCopyright();

        Statistics.AddCommand("media-info");

        AaruLogging.Debug(MODULE_NAME, "debug={0}",         settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "device={0}",        Markup.Escape(settings.DevicePath   ?? ""));
        AaruLogging.Debug(MODULE_NAME, "output-prefix={0}", Markup.Escape(settings.OutputPrefix ?? ""));
        AaruLogging.Debug(MODULE_NAME, "verbose={0}",       settings.Verbose);

        string devicePath = settings.DevicePath;

        if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
            devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

        Devices.Device dev      = null;
        ErrorNumber    devErrno = ErrorNumber.NoError;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Opening_device).IsIndeterminate();
            dev = Devices.Device.Create(devicePath, out devErrno);
        });

        switch(dev)
        {
            case null:
                AaruLogging.Error(string.Format(UI.Could_not_open_device_error_0, Error.Print(devErrno)));

                return (int)ErrorNumber.CannotOpenDevice;
            case Devices.Remote.Device remoteDev:
                Statistics.AddRemote(remoteDev.RemoteApplication,
                                     remoteDev.RemoteVersion,
                                     remoteDev.RemoteOperatingSystem,
                                     remoteDev.RemoteOperatingSystemVersion,
                                     remoteDev.RemoteArchitecture);

                break;
        }

        if(dev.Error)
        {
            AaruLogging.Error(Error.Print(dev.LastError));

            return (int)ErrorNumber.CannotOpenDevice;
        }

        DeviceLog.StartLog(dev, false);

        Statistics.AddDevice(dev);

        switch(dev.Type)
        {
            case DeviceType.ATA:
                DoAtaMediaInfo();

                break;
            case DeviceType.MMC:
            case DeviceType.SecureDigital:
                DoSdMediaInfo();

                break;
            case DeviceType.NVMe:
                DoNvmeMediaInfo(settings.OutputPrefix, dev);

                break;
            case DeviceType.ATAPI:
            case DeviceType.SCSI:
                DoScsiMediaInfo(settings.Debug, settings.OutputPrefix, dev);

                break;
            default:
                throw new NotSupportedException(Localization.Core.Unknown_device_type);
        }

        return (int)ErrorNumber.NoError;
    }

    static void DoAtaMediaInfo() => AaruLogging.Error(UI.Please_use_device_info_command_for_ATA_devices);

    // ReSharper disable UnusedParameter.Local
    static void DoNvmeMediaInfo(string outputPrefix, Devices.Device dev) =>
        throw new NotImplementedException(Localization.Core.NVMe_devices_not_yet_supported);

    // ReSharper restore UnusedParameter.Local

    static void DoSdMediaInfo() => AaruLogging.Error(UI.Please_use_device_info_command_for_MMC_SD_devices);

    static void DoScsiMediaInfo(bool debug, string outputPrefix, Devices.Device dev)
    {
        ScsiInfo scsiInfo = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Retrieving_SCSI_information).IsIndeterminate();
            scsiInfo = new ScsiInfo(dev);
        });

        if(!scsiInfo.MediaInserted) return;

        if(scsiInfo.DeviceInfo.ScsiModeSense6 != null)
        {
            DataFile.WriteTo(MODULE_NAME,
                             outputPrefix,
                             "_scsi_modesense6.bin",
                             "SCSI MODE SENSE (6)",
                             scsiInfo.DeviceInfo.ScsiModeSense6);
        }

        if(scsiInfo.DeviceInfo.ScsiModeSense10 != null)
        {
            DataFile.WriteTo(MODULE_NAME,
                             outputPrefix,
                             "_scsi_modesense10.bin",
                             "SCSI MODE SENSE (10)",
                             scsiInfo.DeviceInfo.ScsiModeSense10);
        }

        switch(dev.ScsiType)
        {
            case PeripheralDeviceTypes.DirectAccess:
            case PeripheralDeviceTypes.MultiMediaDevice:
            case PeripheralDeviceTypes.OCRWDevice:
            case PeripheralDeviceTypes.OpticalDevice:
            case PeripheralDeviceTypes.SimplifiedDevice:
            case PeripheralDeviceTypes.WriteOnceDevice:
            case PeripheralDeviceTypes.BridgingExpander when dev.Model.StartsWith("MDM", StringComparison.Ordinal) ||
                                                             dev.Model.StartsWith("MDH", StringComparison.Ordinal):
                if(scsiInfo.ReadCapacity != null)
                {
                    DataFile.WriteTo(MODULE_NAME,
                                     outputPrefix,
                                     "_readcapacity.bin",
                                     "SCSI READ CAPACITY",
                                     scsiInfo.ReadCapacity);
                }

                if(scsiInfo.ReadCapacity16 != null)
                {
                    DataFile.WriteTo(MODULE_NAME,
                                     outputPrefix,
                                     "_readcapacity16.bin",
                                     "SCSI READ CAPACITY(16)",
                                     scsiInfo.ReadCapacity16);
                }

                if(scsiInfo.Blocks != 0 && scsiInfo.BlockSize != 0)
                {
                    AaruLogging.WriteLine(Localization.Core.Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2,
                                          scsiInfo.Blocks,
                                          scsiInfo.BlockSize,
                                          ByteSize.FromBytes(scsiInfo.Blocks * scsiInfo.BlockSize).ToString("0.000"));
                }

                break;
            case PeripheralDeviceTypes.SequentialAccess:
                if(scsiInfo.DensitySupport != null)
                {
                    DataFile.WriteTo(MODULE_NAME,
                                     outputPrefix,
                                     "_ssc_reportdensitysupport_media.bin",
                                     "SSC REPORT DENSITY SUPPORT (MEDIA)",
                                     scsiInfo.DensitySupport);

                    if(scsiInfo.DensitySupportHeader.HasValue)
                    {
                        AaruLogging.WriteLine($"[bold]{UI.Densities_supported_by_currently_inserted_media}:[/]");
                        AaruLogging.WriteLine(DensitySupport.PrettifyDensity(scsiInfo.DensitySupportHeader));
                    }
                }

                if(scsiInfo.MediaTypeSupport != null)
                {
                    DataFile.WriteTo(MODULE_NAME,
                                     outputPrefix,
                                     "_ssc_reportdensitysupport_medium_media.bin",
                                     "SSC REPORT DENSITY SUPPORT (MEDIUM & MEDIA)",
                                     scsiInfo.MediaTypeSupport);

                    if(scsiInfo.MediaTypeSupportHeader.HasValue)
                    {
                        AaruLogging.WriteLine($"[bold]{UI.Medium_types_currently_inserted_in_device}:[/]");
                        AaruLogging.WriteLine(DensitySupport.PrettifyMediumType(scsiInfo.MediaTypeSupportHeader));
                    }

                    AaruLogging.WriteLine(DensitySupport.PrettifyMediumType(scsiInfo.MediaTypeSupport));
                }

                break;
        }

        if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
        {
            if(scsiInfo.MmcConfiguration != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_getconfiguration_current.bin",
                                 "SCSI GET CONFIGURATION",
                                 scsiInfo.MmcConfiguration);
            }

            if(scsiInfo.RecognizedFormatLayers != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_formatlayers.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.RecognizedFormatLayers);
            }

            if(scsiInfo.WriteProtectionStatus != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_writeprotection.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.WriteProtectionStatus);
            }

            if(scsiInfo.DvdPfi != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_pfi.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdPfi);

                if(scsiInfo.DecodedPfi.HasValue)
                {
                    AaruLogging.WriteLine("[bold][blue]PFI:[/][/]");
                    AaruLogging.WriteLine(PFI.Prettify(scsiInfo.DecodedPfi));
                }
            }

            if(scsiInfo.DvdDmi != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_dmi.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdDmi);

                if(DMI.IsXbox(scsiInfo.DvdDmi))
                {
                    AaruLogging.WriteLine($"[bold]{Localization.Core.Xbox_DMI}:[/]" +
                                          $"\n{Markup.Escape(DMI.PrettifyXbox(scsiInfo.DvdDmi))}");
                }
                else if(DMI.IsXbox360(scsiInfo.DvdDmi))
                {
                    AaruLogging.WriteLine($"[bold]{Localization.Core.Xbox_360_DMI}:[/]" +
                                          $"\n{Markup.Escape(DMI.PrettifyXbox360(scsiInfo.DvdDmi))}");
                }
            }

            if(scsiInfo.DvdCmi != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_cmi.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdCmi);

                AaruLogging.WriteLine(Localization.Core.Lead_In_CMI);
                AaruLogging.WriteLine(CSS_CPRM.PrettifyLeadInCopyright(scsiInfo.DvdCmi));
            }

            if(scsiInfo.DvdDiscKey != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_disckey.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdDiscKey);
            }

            if(scsiInfo.DvdSectorCmi != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_sectorcmi.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdSectorCmi);
            }

            if(scsiInfo.DvdBca != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_bca.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdBca);
            }

            if(scsiInfo.DvdAacs != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_aacs.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdAacs);
            }

            if(scsiInfo.DvdRamDds != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvdram_dds.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdRamDds);

                AaruLogging.WriteLine($"[bold]{UI.Disc_Definition_Structure}:[/]" +
                                      $"\n{Markup.Escape(DDS.Prettify(scsiInfo.DvdRamDds))}");
            }

            if(scsiInfo.DvdRamCartridgeStatus != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvdram_status.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdRamCartridgeStatus);

                AaruLogging.WriteLine($"[bold]{Localization.Core.Medium_Status}:[/]" +
                                      $"\n{Markup.Escape(Cartridge.Prettify(scsiInfo.DvdRamCartridgeStatus))}");
            }

            if(scsiInfo.DvdRamSpareArea != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvdram_spare.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdRamSpareArea);

                AaruLogging.WriteLine($"[bold]{UI.Spare_Area_Information}:[/]" +
                                      $"\n{Markup.Escape(Spare.Prettify(scsiInfo.DvdRamSpareArea))}");
            }

            if(scsiInfo.LastBorderOutRmd != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_lastrmd.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.LastBorderOutRmd);
            }

            if(scsiInfo.DvdPreRecordedInfo != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_pri.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdPreRecordedInfo);

                if(scsiInfo.DecodedDvdPrePitInformation.HasValue)
                {
                    AaruLogging.WriteLine(Localization.Core.DVD_RW_Pre_Recorded_Information);
                    AaruLogging.WriteLine(PRI.Prettify(scsiInfo.DecodedDvdPrePitInformation));
                }
            }

            if(scsiInfo.DvdrMediaIdentifier != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvdr_mediaid.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdrMediaIdentifier);
            }

            if(scsiInfo.DvdrPhysicalInformation != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvdr_pfi.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdrPhysicalInformation);

                if(scsiInfo.DecodedDvdrPfi.HasValue)
                {
                    AaruLogging.WriteLine(Localization.Core.DVD_RW_PFI);
                    AaruLogging.WriteLine(PFI.Prettify(scsiInfo.DecodedDvdrPfi));
                }
            }

            if(scsiInfo.DvdPlusAdip != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd+_adip.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdPlusAdip);
            }

            if(scsiInfo.DvdPlusDcb != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd+_dcb.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdPlusDcb);
            }

            if(scsiInfo.HddvdCopyrightInformation != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_hddvd_cmi.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.HddvdCopyrightInformation);
            }

            if(scsiInfo.HddvdrMediumStatus != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_hddvdr_status.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.HddvdrMediumStatus);
            }

            if(scsiInfo.HddvdrLastRmd != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_hddvdr_lastrmd.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.HddvdrLastRmd);
            }

            if(scsiInfo.DvdrLayerCapacity != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvdr_layercap.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdrLayerCapacity);
            }

            if(scsiInfo.DvdrDlMiddleZoneStart != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_mzs.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdrDlMiddleZoneStart);
            }

            if(scsiInfo.DvdrDlJumpIntervalSize != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_jis.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdrDlJumpIntervalSize);
            }

            if(scsiInfo.DvdrDlManualLayerJumpStartLba != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_manuallj.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdrDlManualLayerJumpStartLba);
            }

            if(scsiInfo.DvdrDlRemapAnchorPoint != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_dvd_remapanchor.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.DvdrDlRemapAnchorPoint);
            }

            if(scsiInfo.BlurayDiscInformation != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_bd_di.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.BlurayDiscInformation);

                AaruLogging.WriteLine(Localization.Core.Bluray_Disc_Information);
#pragma warning disable PH2069
                AaruLogging.WriteLine("{0}", Markup.Escape(DI.Prettify(scsiInfo.BlurayDiscInformation)));
#pragma warning restore PH2069
            }

            if(scsiInfo.BlurayPac != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_bd_pac.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.BlurayPac);
            }

            if(scsiInfo.BlurayBurstCuttingArea != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_bd_bca.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.BlurayBurstCuttingArea);

                AaruLogging.WriteLine(Localization.Core.Bluray_Burst_Cutting_Area);
                AaruLogging.WriteLine(BCA.Prettify(scsiInfo.BlurayBurstCuttingArea));
            }

            if(scsiInfo.BlurayDds != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_bd_dds.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.BlurayDds);

                AaruLogging.WriteLine(Localization.Core.Bluray_Disc_Definition_Structure);
                AaruLogging.WriteLine(Decoders.Bluray.DDS.Prettify(scsiInfo.BlurayDds));
            }

            if(scsiInfo.BlurayCartridgeStatus != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_bd_cartstatus.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.BlurayCartridgeStatus);

                AaruLogging.WriteLine(Localization.Core.Bluray_Cartridge_Status);
                AaruLogging.WriteLine(Decoders.Bluray.Cartridge.Prettify(scsiInfo.BlurayCartridgeStatus));
            }

            if(scsiInfo.BluraySpareAreaInformation != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_bd_spare.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.BluraySpareAreaInformation);

                AaruLogging.WriteLine(Localization.Core.Bluray_Spare_Area_Information);
                AaruLogging.WriteLine(Decoders.Bluray.Spare.Prettify(scsiInfo.BluraySpareAreaInformation));
            }

            if(scsiInfo.BlurayRawDfl != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscstructure_bd_dfl.bin",
                                 "SCSI READ DISC STRUCTURE",
                                 scsiInfo.BlurayRawDfl);
            }

            if(scsiInfo.BlurayTrackResources != null)
            {
                AaruLogging.WriteLine($"[bold]{Localization.Core.Track_Resources_Information}:[/]" +
                                      $"\n{Markup.Escape(DiscInformation.Prettify(scsiInfo.BlurayTrackResources))}");

                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscinformation_001b.bin",
                                 "SCSI READ DISC INFORMATION",
                                 scsiInfo.BlurayTrackResources);
            }

            if(scsiInfo.BlurayPowResources != null)
            {
                AaruLogging.WriteLine($"[bold]{Localization.Core.POW_Resources_Information}:[/]" +
                                      $"\n{Markup.Escape(DiscInformation.Prettify(scsiInfo.BlurayPowResources))}");

                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscinformation_010b.bin",
                                 "SCSI READ DISC INFORMATION",
                                 scsiInfo.BlurayPowResources);
            }

            if(scsiInfo.Toc != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_toc.bin", "SCSI READ TOC/PMA/ATIP", scsiInfo.Toc);

                if(scsiInfo.DecodedToc.HasValue)
                {
                    AaruLogging.WriteLine(UI.Title_TOC);
                    AaruLogging.WriteLine(TOC.Prettify(scsiInfo.DecodedToc));
                }
            }

            if(scsiInfo.Atip != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_atip.bin", "SCSI READ TOC/PMA/ATIP", scsiInfo.Atip);

                if(scsiInfo.DecodedAtip != null)
                {
                    AaruLogging.WriteLine(UI.Title_ATIP);
                    AaruLogging.WriteLine(ATIP.Prettify(scsiInfo.DecodedAtip));
                }
            }

            if(scsiInfo.DiscInformation != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_readdiscinformation_000b.bin",
                                 "SCSI READ DISC INFORMATION",
                                 scsiInfo.DiscInformation);

                if(scsiInfo.DecodedDiscInformation.HasValue)
                {
                    AaruLogging.WriteLine(Localization.Core.Standard_Disc_Information);
                    AaruLogging.WriteLine(DiscInformation.Prettify000b(scsiInfo.DecodedDiscInformation));
                }
            }

            if(scsiInfo.Session != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_session.bin", "SCSI READ TOC/PMA/ATIP", scsiInfo.Session);

                if(scsiInfo.DecodedSession.HasValue)
                {
                    AaruLogging.WriteLine(Localization.Core.Session_information);
                    AaruLogging.WriteLine(Session.Prettify(scsiInfo.DecodedSession));
                }
            }

            if(scsiInfo.RawToc != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_rawtoc.bin", "SCSI READ TOC/PMA/ATIP", scsiInfo.RawToc);

                if(scsiInfo.FullToc.HasValue)
                {
                    AaruLogging.WriteLine(Localization.Core.Raw_TOC);
                    AaruLogging.WriteLine(FullTOC.Prettify(scsiInfo.RawToc));
                }
            }

            if(scsiInfo.Pma != null)
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_pma.bin", "SCSI READ TOC/PMA/ATIP", scsiInfo.Pma);

                AaruLogging.WriteLine($"[bold]{Localization.Core.PMA}:[/]" +
                                      $"\n[/]{Markup.Escape(PMA.Prettify(scsiInfo.Pma))}");
            }

            if(scsiInfo.CdTextLeadIn != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_cdtext.bin",
                                 "SCSI READ TOC/PMA/ATIP",
                                 scsiInfo.CdTextLeadIn);

                if(scsiInfo.DecodedCdTextLeadIn.HasValue)
                {
                    AaruLogging.WriteLine($"[bold]{Localization.Core.CD_TEXT_on_Lead_In}:[/]" +
                                          $"\n{Markup.Escape(CDTextOnLeadIn.Prettify(scsiInfo.DecodedCdTextLeadIn))}");
                }
            }

            if(!string.IsNullOrEmpty(scsiInfo.Mcn))
                AaruLogging.WriteLine($"[bold]{Localization.Core.MCN}:[/]" + $" {Markup.Escape(scsiInfo.Mcn)}");

            if(scsiInfo.Isrcs != null)
            {
                foreach(KeyValuePair<byte, string> isrc in scsiInfo.Isrcs)
                {
                    AaruLogging.WriteLine($"[bold]{string.Format(Localization.Core.Tracks_0_ISRC, isrc.Key)}:[/] {
                        Markup.Escape(isrc.Value)}");
                }
            }

            if(scsiInfo.XboxSecuritySector != null)
            {
                DataFile.WriteTo(MODULE_NAME,
                                 outputPrefix,
                                 "_xbox_ss.bin",
                                 "KREON EXTRACT SS",
                                 scsiInfo.XboxSecuritySector);
            }

            if(scsiInfo.DecodedXboxSecuritySector.HasValue)
            {
                AaruLogging.WriteLine($"[bold]{Localization.Core.Xbox_Security_Sector}:[/]" +
                                      $"\n{Markup.Escape(SS.Prettify(scsiInfo.DecodedXboxSecuritySector))}");
            }

            if(scsiInfo.XgdInfo != null)
            {
                AaruLogging.WriteLine($"[bold]{Localization.Core.Video_layer_zero_size}:[/] {scsiInfo.XgdInfo.L0Video
                } sectors");

                AaruLogging.WriteLine($"[bold]{Localization.Core.Video_layer_one_size}:[/] {scsiInfo.XgdInfo.L1Video
                } sectors");

                AaruLogging.WriteLine($"[bold]{Localization.Core.Middle_zone_size}:[/] {scsiInfo.XgdInfo.MiddleZone
                } sectors");

                AaruLogging.WriteLine($"[bold]{Localization.Core.Game_data_size}:[/] {scsiInfo.XgdInfo.GameSize
                } sectors");

                AaruLogging.WriteLine($"[bold]{Localization.Core.Total_size}:[/] {scsiInfo.XgdInfo.TotalSize} sectors");

                AaruLogging.WriteLine($"[bold]{Localization.Core.Real_layer_break}:[/] {scsiInfo.XgdInfo.LayerBreak
                } sectors");

                AaruLogging.WriteLine();
            }
        }

        if(scsiInfo.MediaSerialNumber != null)
        {
            DataFile.WriteTo(MODULE_NAME,
                             outputPrefix,
                             "_mediaserialnumber.bin",
                             "SCSI READ MEDIA SERIAL NUMBER",
                             scsiInfo.MediaSerialNumber);

            AaruLogging.Write(Localization.Core.Media_Serial_Number);

            for(var i = 4; i < scsiInfo.MediaSerialNumber.Length; i++)
                AaruLogging.Write("[teal]{0:X2}[/]", scsiInfo.MediaSerialNumber[i]);

            AaruLogging.WriteLine("");
        }

        AaruLogging.WriteLine(Localization.Core.Media_identified_as_0, scsiInfo.MediaType.Humanize());
        Statistics.AddMedia(scsiInfo.MediaType, true);

        if(scsiInfo.Toc != null || scsiInfo.RawToc != null)
        {
            Track[] tracks = Dump.GetCdTracks(dev, false, out long lastSector, null, null, null, out _, null, null);

            if(tracks != null)
            {
                var firstLba = (uint)tracks.Min(static t => t.StartSector);

                bool supportsPqSubchannel = Dump.SupportsPqSubchannel(dev, null, firstLba);
                bool supportsRwSubchannel = Dump.SupportsRwSubchannel(dev, null, firstLba);

                // Open main database
                var ctx = AaruContext.Create(Aaru.Settings.Settings.MainDbPath);

                // Search for device in main database
                Aaru.Database.Models.Device dbDev =
                    ctx.Devices.FirstOrDefault(d => d.Manufacturer == dev.Manufacturer &&
                                                    d.Model        == dev.Model        &&
                                                    d.Revision     == dev.FirmwareRevision);

                Dump.SolveTrackPregaps(dev,
                                       null,
                                       tracks,
                                       supportsPqSubchannel,
                                       supportsRwSubchannel,
                                       dbDev,
                                       out bool inexactPositioning,
                                       false);

                for(var t = 1; t < tracks.Length; t++) tracks[t - 1].EndSector = tracks[t].StartSector - 1;

                tracks[^1].EndSector = (ulong)lastSector;

                AaruLogging.WriteLine();
                AaruLogging.WriteLine(Localization.Core.Track_calculations);

                if(inexactPositioning)
                {
                    AaruLogging.WriteLine(Localization.Core
                                                      .The_drive_has_returned_incorrect_Q_positioning_calculating_pregaps);
                }

                if(firstLba > 0) AaruLogging.WriteLine(UI.Hidden_track_starts_at_LBA_0_ends_at_LBA_1, 0, firstLba - 1);

                foreach(Track track in tracks)
                {
                    AaruLogging.WriteLine(UI
                                             .Track_0_starts_at_LBA_1_ends_at_LBA_2_has_a_pregap_of_3_sectors_and_is_of_type_4,
                                          track.Sequence,
                                          track.StartSector,
                                          track.EndSector,
                                          track.Pregap,
                                          track.Type.Humanize());
                }

                AaruLogging.WriteLine();
                AaruLogging.WriteLine(Localization.Core.Offsets);

                // Search for read offset in main database
                CdOffset cdOffset =
                    ctx.CdOffsets.FirstOrDefault(d => (d.Manufacturer == dev.Manufacturer ||
                                                       d.Manufacturer == dev.Manufacturer.Replace('/', '-')) &&
                                                      (d.Model == dev.Model || d.Model == dev.Model.Replace('/', '-')));

                CompactDisc.GetOffset(cdOffset,
                                      dbDev,
                                      debug,
                                      dev,
                                      scsiInfo.MediaType,
                                      tracks,
                                      null,
                                      out int? driveOffset,
                                      out int? combinedOffset,
                                      out _);

                if(combinedOffset is null)
                {
                    if(driveOffset is null)
                    {
                        AaruLogging.WriteLine(Localization.Core.Drive_reading_offset_not_found_in_database);

                        AaruLogging.WriteLine(Localization.Core.Disc_offset_cannot_be_calculated);
                    }
                    else
                    {
                        AaruLogging.WriteLine(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                            driveOffset,
                                                            driveOffset / 4));

                        AaruLogging.WriteLine(Localization.Core.Disc_write_offset_is_unknown);
                    }
                }
                else
                {
                    int offsetBytes = combinedOffset.Value;

                    if(driveOffset is null)
                    {
                        AaruLogging.WriteLine(Localization.Core.Drive_reading_offset_not_found_in_database);

                        AaruLogging.WriteLine(string.Format(Localization.Core
                                                                        .Combined_disc_and_drive_offset_are_0_bytes_1_samples,
                                                            offsetBytes,
                                                            offsetBytes / 4));
                    }
                    else
                    {
                        AaruLogging.WriteLine(string.Format(Localization.Core.Drive_reading_offset_is_0_bytes_1_samples,
                                                            driveOffset,
                                                            driveOffset / 4));

                        AaruLogging.WriteLine(string.Format(Localization.Core.Combined_offset_is_0_bytes_1_samples,
                                                            offsetBytes,
                                                            offsetBytes / 4));

                        int? discOffset = offsetBytes - driveOffset;

                        AaruLogging.WriteLine(string.Format(Localization.Core.Disc_offset_is_0_bytes_1_samples,
                                                            discOffset,
                                                            discOffset / 4));
                    }
                }
            }
        }

        dev.Close();
    }

#region Nested type: Settings

    public class Settings : MediaFamily
    {
        [LocalizedDescription(nameof(UI.Prefix_for_saving_binary_information))]
        [DefaultValue(null)]
        [CommandOption("-w|--output-prefix")]
        public string OutputPrefix { get; init; }
        [LocalizedDescription(nameof(UI.Device_path))]
        [CommandArgument(0, "<device-path>")]
        public string DevicePath { get; init; }
    }

#endregion
}