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
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Devices.Dumping;
using Aaru.Core.Media.Info;
using Aaru.Database;
using Aaru.Database.Models;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Decoders.Xbox;
using Aaru.Devices;
using BCA = Aaru.Decoders.Bluray.BCA;
using Cartridge = Aaru.Decoders.DVD.Cartridge;
using Command = System.CommandLine.Command;
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using Session = Aaru.Decoders.CD.Session;
using Spare = Aaru.Decoders.DVD.Spare;

namespace Aaru.Commands.Media
{
    internal sealed class MediaInfoCommand : Command
    {
        public MediaInfoCommand() : base("info", "Gets information about the media inserted on a device.")
        {
            Add(new Option(new[]
                {
                    "--output-prefix", "-w"
                }, "Write binary responses from device with that prefix.")
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
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("media-info");

            AaruConsole.DebugWriteLine("Media-Info command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Media-Info command", "--device={0}", devicePath);
            AaruConsole.DebugWriteLine("Media-Info command", "--output-prefix={0}", outputPrefix);
            AaruConsole.DebugWriteLine("Media-Info command", "--verbose={0}", verbose);

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
                    DoNvmeMediaInfo(outputPrefix, dev);

                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    DoScsiMediaInfo(debug, outputPrefix, dev);

                    break;
                default: throw new NotSupportedException("Unknown device type.");
            }

            return (int)ErrorNumber.NoError;
        }

        static void DoAtaMediaInfo() => AaruConsole.ErrorWriteLine("Please use device-info command for ATA devices.");

        static void DoNvmeMediaInfo(string outputPrefix, Devices.Device dev) =>
            throw new NotImplementedException("NVMe devices not yet supported.");

        static void DoSdMediaInfo() => AaruConsole.ErrorWriteLine("Please use device-info command for MMC/SD devices.");

        static void DoScsiMediaInfo(bool debug, string outputPrefix, Devices.Device dev)
        {
            var scsiInfo = new ScsiInfo(dev);

            if(!scsiInfo.MediaInserted)
                return;

            if(scsiInfo.DeviceInfo.ScsiModeSense6 != null)
                DataFile.WriteTo("Media-Info command", outputPrefix, "_scsi_modesense6.bin", "SCSI MODE SENSE (6)",
                                 scsiInfo.DeviceInfo.ScsiModeSense6);

            if(scsiInfo.DeviceInfo.ScsiModeSense10 != null)
                DataFile.WriteTo("Media-Info command", outputPrefix, "_scsi_modesense10.bin", "SCSI MODE SENSE (10)",
                                 scsiInfo.DeviceInfo.ScsiModeSense10);

            switch(dev.ScsiType)
            {
                case PeripheralDeviceTypes.DirectAccess:
                case PeripheralDeviceTypes.MultiMediaDevice:
                case PeripheralDeviceTypes.OCRWDevice:
                case PeripheralDeviceTypes.OpticalDevice:
                case PeripheralDeviceTypes.SimplifiedDevice:
                case PeripheralDeviceTypes.WriteOnceDevice:
                case PeripheralDeviceTypes.BridgingExpander
                    when dev.Model.StartsWith("MDM", StringComparison.Ordinal) ||
                         dev.Model.StartsWith("MDH", StringComparison.Ordinal):
                    if(scsiInfo.ReadCapacity != null)
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readcapacity.bin", "SCSI READ CAPACITY",
                                         scsiInfo.ReadCapacity);

                    if(scsiInfo.ReadCapacity16 != null)
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readcapacity16.bin",
                                         "SCSI READ CAPACITY(16)", scsiInfo.ReadCapacity16);

                    if(scsiInfo.Blocks    != 0 &&
                       scsiInfo.BlockSize != 0)
                    {
                        ulong totalSize = scsiInfo.Blocks * scsiInfo.BlockSize;

                        if(totalSize > 1099511627776)
                            AaruConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2:F3} TiB)",
                                                  scsiInfo.Blocks, scsiInfo.BlockSize, totalSize / 1099511627776d);
                        else if(totalSize > 1073741824)
                            AaruConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2:F3} GiB)",
                                                  scsiInfo.Blocks, scsiInfo.BlockSize, totalSize / 1073741824d);
                        else if(totalSize > 1048576)
                            AaruConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2:F3} MiB)",
                                                  scsiInfo.Blocks, scsiInfo.BlockSize, totalSize / 1048576d);
                        else if(totalSize > 1024)
                            AaruConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2:F3} KiB)",
                                                  scsiInfo.Blocks, scsiInfo.BlockSize, totalSize / 1024d);
                        else
                            AaruConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2} bytes)",
                                                  scsiInfo.Blocks, scsiInfo.BlockSize, totalSize);
                    }

                    break;
                case PeripheralDeviceTypes.SequentialAccess:
                    if(scsiInfo.DensitySupport != null)
                    {
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_ssc_reportdensitysupport_media.bin",
                                         "SSC REPORT DENSITY SUPPORT (MEDIA)", scsiInfo.DensitySupport);

                        if(scsiInfo.DensitySupportHeader.HasValue)
                        {
                            AaruConsole.WriteLine("Densities supported by currently inserted media:");
                            AaruConsole.WriteLine(DensitySupport.PrettifyDensity(scsiInfo.DensitySupportHeader));
                        }
                    }

                    if(scsiInfo.MediaTypeSupport != null)
                    {
                        DataFile.WriteTo("Media-Info command", outputPrefix,
                                         "_ssc_reportdensitysupport_medium_media.bin",
                                         "SSC REPORT DENSITY SUPPORT (MEDIUM & MEDIA)", scsiInfo.MediaTypeSupport);

                        if(scsiInfo.MediaTypeSupportHeader.HasValue)
                        {
                            AaruConsole.WriteLine("Medium types currently inserted in device:");
                            AaruConsole.WriteLine(DensitySupport.PrettifyMediumType(scsiInfo.MediaTypeSupportHeader));
                        }

                        AaruConsole.WriteLine(DensitySupport.PrettifyMediumType(scsiInfo.MediaTypeSupport));
                    }

                    break;
            }

            if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
            {
                if(scsiInfo.MmcConfiguration != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_getconfiguration_current.bin",
                                     "SCSI GET CONFIGURATION", scsiInfo.MmcConfiguration);

                if(scsiInfo.RecognizedFormatLayers != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_formatlayers.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.RecognizedFormatLayers);

                if(scsiInfo.WriteProtectionStatus != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_writeprotection.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.WriteProtectionStatus);

                if(scsiInfo.DvdPfi != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_pfi.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdPfi);

                    if(scsiInfo.DecodedPfi.HasValue)
                        AaruConsole.WriteLine("PFI:\n{0}", PFI.Prettify(scsiInfo.DecodedPfi));
                }

                if(scsiInfo.DvdDmi != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_dmi.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdDmi);

                    if(DMI.IsXbox(scsiInfo.DvdDmi))
                        AaruConsole.WriteLine("Xbox DMI:\n{0}", DMI.PrettifyXbox(scsiInfo.DvdDmi));
                    else if(DMI.IsXbox360(scsiInfo.DvdDmi))
                        AaruConsole.WriteLine("Xbox 360 DMI:\n{0}", DMI.PrettifyXbox360(scsiInfo.DvdDmi));
                }

                if(scsiInfo.DvdCmi != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_cmi.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdCmi);

                    AaruConsole.WriteLine("Lead-In CMI:\n{0}", CSS_CPRM.PrettifyLeadInCopyright(scsiInfo.DvdCmi));
                }

                if(scsiInfo.DvdDiscKey != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_disckey.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdDiscKey);

                if(scsiInfo.DvdSectorCmi != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_sectorcmi.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdSectorCmi);

                if(scsiInfo.DvdBca != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_bca.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdBca);

                if(scsiInfo.DvdAacs != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_aacs.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdAacs);

                if(scsiInfo.DvdRamDds != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdram_dds.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdRamDds);

                    AaruConsole.WriteLine("Disc Definition Structure:\n{0}", DDS.Prettify(scsiInfo.DvdRamDds));
                }

                if(scsiInfo.DvdRamCartridgeStatus != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdram_status.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdRamCartridgeStatus);

                    AaruConsole.WriteLine("Medium Status:\n{0}", Cartridge.Prettify(scsiInfo.DvdRamCartridgeStatus));
                }

                if(scsiInfo.DvdRamSpareArea != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdram_spare.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdRamSpareArea);

                    AaruConsole.WriteLine("Spare Area Information:\n{0}", Spare.Prettify(scsiInfo.DvdRamSpareArea));
                }

                if(scsiInfo.LastBorderOutRmd != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_lastrmd.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.LastBorderOutRmd);

                if(scsiInfo.DvdPreRecordedInfo != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_pri.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdPreRecordedInfo);

                    if(scsiInfo.DecodedDvdPrePitInformation.HasValue)
                        AaruConsole.WriteLine("DVD-R(W) Pre-Recorded Information:\n{0}",
                                              PRI.Prettify(scsiInfo.DecodedDvdPrePitInformation));
                }

                if(scsiInfo.DvdrMediaIdentifier != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdr_mediaid.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdrMediaIdentifier);

                if(scsiInfo.DvdrPhysicalInformation != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdr_pfi.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdrPhysicalInformation);

                    if(scsiInfo.DecodedDvdrPfi.HasValue)
                        AaruConsole.WriteLine("DVD-R(W) PFI:\n{0}", PFI.Prettify(scsiInfo.DecodedDvdrPfi));
                }

                if(scsiInfo.DvdPlusAdip != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd+_adip.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdPlusAdip);

                if(scsiInfo.DvdPlusDcb != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd+_dcb.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdPlusDcb);

                if(scsiInfo.HddvdCopyrightInformation != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_hddvd_cmi.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.HddvdCopyrightInformation);

                if(scsiInfo.HddvdrMediumStatus != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_hddvdr_status.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.HddvdrMediumStatus);

                if(scsiInfo.HddvdrLastRmd != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_hddvdr_lastrmd.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.HddvdrLastRmd);

                if(scsiInfo.DvdrLayerCapacity != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdr_layercap.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdrLayerCapacity);

                if(scsiInfo.DvdrDlMiddleZoneStart != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_mzs.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdrDlMiddleZoneStart);

                if(scsiInfo.DvdrDlJumpIntervalSize != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_jis.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdrDlJumpIntervalSize);

                if(scsiInfo.DvdrDlManualLayerJumpStartLba != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_manuallj.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdrDlManualLayerJumpStartLba);

                if(scsiInfo.DvdrDlRemapAnchorPoint != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_remapanchor.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdrDlRemapAnchorPoint);

                if(scsiInfo.BlurayDiscInformation != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_di.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayDiscInformation);

                    AaruConsole.WriteLine("Blu-ray Disc Information:\n{0}",
                                          DI.Prettify(scsiInfo.BlurayDiscInformation));
                }

                if(scsiInfo.BlurayPac != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_pac.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayPac);

                if(scsiInfo.BlurayBurstCuttingArea != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_bca.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayBurstCuttingArea);

                    AaruConsole.WriteLine("Blu-ray Burst Cutting Area:\n{0}",
                                          BCA.Prettify(scsiInfo.BlurayBurstCuttingArea));
                }

                if(scsiInfo.BlurayDds != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_dds.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayDds);

                    AaruConsole.WriteLine("Blu-ray Disc Definition Structure:\n{0}",
                                          Decoders.Bluray.DDS.Prettify(scsiInfo.BlurayDds));
                }

                if(scsiInfo.BlurayCartridgeStatus != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_cartstatus.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayCartridgeStatus);

                    AaruConsole.WriteLine("Blu-ray Cartridge Status:\n{0}",
                                          Decoders.Bluray.Cartridge.Prettify(scsiInfo.BlurayCartridgeStatus));
                }

                if(scsiInfo.BluraySpareAreaInformation != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_spare.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BluraySpareAreaInformation);

                    AaruConsole.WriteLine("Blu-ray Spare Area Information:\n{0}",
                                          Decoders.Bluray.Spare.Prettify(scsiInfo.BluraySpareAreaInformation));
                }

                if(scsiInfo.BlurayRawDfl != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_dfl.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayRawDfl);

                if(scsiInfo.BlurayTrackResources != null)
                {
                    AaruConsole.WriteLine("Track Resources Information:\n{0}",
                                          DiscInformation.Prettify(scsiInfo.BlurayTrackResources));

                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscinformation_001b.bin",
                                     "SCSI READ DISC INFORMATION", scsiInfo.BlurayTrackResources);
                }

                if(scsiInfo.BlurayPowResources != null)
                {
                    AaruConsole.WriteLine("POW Resources Information:\n{0}",
                                          DiscInformation.Prettify(scsiInfo.BlurayPowResources));

                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscinformation_010b.bin",
                                     "SCSI READ DISC INFORMATION", scsiInfo.BlurayPowResources);
                }

                if(scsiInfo.Toc != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_toc.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.Toc);

                    if(scsiInfo.DecodedToc.HasValue)
                        AaruConsole.WriteLine("TOC:\n{0}", TOC.Prettify(scsiInfo.DecodedToc));
                }

                if(scsiInfo.Atip != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_atip.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.Atip);

                    if(scsiInfo.DecodedAtip != null)
                        AaruConsole.WriteLine("ATIP:\n{0}", ATIP.Prettify(scsiInfo.DecodedAtip));
                }

                if(scsiInfo.DiscInformation != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscinformation_000b.bin",
                                     "SCSI READ DISC INFORMATION", scsiInfo.DiscInformation);

                    if(scsiInfo.DecodedDiscInformation.HasValue)
                        AaruConsole.WriteLine("Standard Disc Information:\n{0}",
                                              DiscInformation.Prettify000b(scsiInfo.DecodedDiscInformation));
                }

                if(scsiInfo.Session != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_session.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.Session);

                    if(scsiInfo.DecodedSession.HasValue)
                        AaruConsole.WriteLine("Session information:\n{0}", Session.Prettify(scsiInfo.DecodedSession));
                }

                if(scsiInfo.RawToc != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_rawtoc.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.RawToc);

                    if(scsiInfo.FullToc.HasValue)
                        AaruConsole.WriteLine("Raw TOC:\n{0}", FullTOC.Prettify(scsiInfo.RawToc));
                }

                if(scsiInfo.Pma != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_pma.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.Pma);

                    AaruConsole.WriteLine("PMA:\n{0}", PMA.Prettify(scsiInfo.Pma));
                }

                if(scsiInfo.CdTextLeadIn != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_cdtext.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.CdTextLeadIn);

                    if(scsiInfo.DecodedCdTextLeadIn.HasValue)
                        AaruConsole.WriteLine("CD-TEXT on Lead-In:\n{0}",
                                              CDTextOnLeadIn.Prettify(scsiInfo.DecodedCdTextLeadIn));
                }

                if(!string.IsNullOrEmpty(scsiInfo.Mcn))
                    AaruConsole.WriteLine("MCN: {0}", scsiInfo.Mcn);

                if(scsiInfo.Isrcs != null)
                    foreach(KeyValuePair<byte, string> isrc in scsiInfo.Isrcs)
                        AaruConsole.WriteLine("Track's {0} ISRC: {1}", isrc.Key, isrc.Value);

                if(scsiInfo.XboxSecuritySector != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_xbox_ss.bin", "KREON EXTRACT SS",
                                     scsiInfo.XboxSecuritySector);

                if(scsiInfo.DecodedXboxSecuritySector.HasValue)
                    AaruConsole.WriteLine("Xbox Security Sector:\n{0}",
                                          SS.Prettify(scsiInfo.DecodedXboxSecuritySector));

                if(scsiInfo.XgdInfo != null)
                {
                    AaruConsole.WriteLine("Video layer 0 size: {0} sectors", scsiInfo.XgdInfo.L0Video);
                    AaruConsole.WriteLine("Video layer 1 size: {0} sectors", scsiInfo.XgdInfo.L1Video);
                    AaruConsole.WriteLine("Middle zone size: {0} sectors", scsiInfo.XgdInfo.MiddleZone);
                    AaruConsole.WriteLine("Game data size: {0} sectors", scsiInfo.XgdInfo.GameSize);
                    AaruConsole.WriteLine("Total size: {0} sectors", scsiInfo.XgdInfo.TotalSize);
                    AaruConsole.WriteLine("Real layer break: {0}", scsiInfo.XgdInfo.LayerBreak);
                    AaruConsole.WriteLine();
                }
            }

            if(scsiInfo.MediaSerialNumber != null)
            {
                DataFile.WriteTo("Media-Info command", outputPrefix, "_mediaserialnumber.bin",
                                 "SCSI READ MEDIA SERIAL NUMBER", scsiInfo.MediaSerialNumber);

                AaruConsole.Write("Media Serial Number: ");

                for(int i = 4; i < scsiInfo.MediaSerialNumber.Length; i++)
                    AaruConsole.Write("{0:X2}", scsiInfo.MediaSerialNumber[i]);

                AaruConsole.WriteLine();
            }

            AaruConsole.WriteLine("Media identified as {0}", scsiInfo.MediaType);
            Statistics.AddMedia(scsiInfo.MediaType, true);

            if(scsiInfo.Toc    != null ||
               scsiInfo.RawToc != null)
            {
                Track[] tracks = Dump.GetCdTracks(dev, null, false, out long lastSector, null, null, null, out _, null,
                                                  null);

                if(tracks != null)
                {
                    uint firstLba = (uint)tracks.Min(t => t.TrackStartSector);

                    bool supportsPqSubchannel = Dump.SupportsPqSubchannel(dev, null, null, firstLba);
                    bool supportsRwSubchannel = Dump.SupportsRwSubchannel(dev, null, null, firstLba);

                    // Open main database
                    var ctx = AaruContext.Create(Settings.Settings.MainDbPath);

                    // Search for device in main database
                    Aaru.Database.Models.Device dbDev =
                        ctx.Devices.FirstOrDefault(d => d.Manufacturer == dev.Manufacturer && d.Model == dev.Model &&
                                                        d.Revision     == dev.FirmwareRevision);

                    Dump.SolveTrackPregaps(dev, null, null, tracks, supportsPqSubchannel, supportsRwSubchannel, dbDev,
                                           out bool inexactPositioning, false);

                    for(int t = 1; t < tracks.Length; t++)
                        tracks[t - 1].TrackEndSector = tracks[t].TrackStartSector - 1;

                    tracks[^1].TrackEndSector = (ulong)lastSector;

                    AaruConsole.WriteLine();
                    AaruConsole.WriteLine("Track calculations:");

                    if(inexactPositioning)
                        AaruConsole.
                            WriteLine("WARNING: The drive has returned incorrect Q positioning when calculating pregaps. A best effort has been tried but they may be incorrect.");

                    if(firstLba > 0)
                        AaruConsole.WriteLine("Hidden track starts at LBA {0}, ends at LBA {1}", 0, firstLba - 1);

                    foreach(Track track in tracks)
                        AaruConsole.
                            WriteLine("Track {0} starts at LBA {1}, ends at LBA {2}, has a pregap of {3} sectors and is of type {4}",
                                      track.TrackSequence, track.TrackStartSector, track.TrackEndSector,
                                      track.TrackPregap, track.TrackType);

                    AaruConsole.WriteLine();
                    AaruConsole.WriteLine("Offsets:");

                    // Search for read offset in main database
                    CdOffset cdOffset =
                        ctx.CdOffsets.FirstOrDefault(d => (d.Manufacturer == dev.Manufacturer ||
                                                           d.Manufacturer == dev.Manufacturer.Replace('/', '-')) &&
                                                          (d.Model == dev.Model ||
                                                           d.Model == dev.Model.Replace('/', '-')));

                    CompactDisc.GetOffset(cdOffset, dbDev, debug, dev, scsiInfo.MediaType, null, tracks, null,
                                          out int? driveOffset, out int? combinedOffset, out _);

                    if(combinedOffset is null)
                    {
                        if(driveOffset is null)
                        {
                            AaruConsole.WriteLine("Drive reading offset not found in database.");
                            AaruConsole.WriteLine("Disc offset cannot be calculated.");
                        }
                        else
                        {
                            AaruConsole.
                                WriteLine($"Drive reading offset is {driveOffset} bytes ({driveOffset / 4} samples).");

                            AaruConsole.WriteLine("Disc write offset is unknown.");
                        }
                    }
                    else
                    {
                        int offsetBytes = combinedOffset.Value;

                        if(driveOffset is null)
                        {
                            AaruConsole.WriteLine("Drive reading offset not found in database.");

                            AaruConsole.
                                WriteLine($"Combined disc and drive offset are {offsetBytes} bytes ({offsetBytes / 4} samples).");
                        }
                        else
                        {
                            AaruConsole.
                                WriteLine($"Drive reading offset is {driveOffset} bytes ({driveOffset / 4} samples).");

                            AaruConsole.
                                WriteLine($"Combined offset is {offsetBytes} bytes ({offsetBytes / 4} samples)");

                            int? discOffset = offsetBytes - driveOffset;

                            AaruConsole.WriteLine($"Disc offset is {discOffset} bytes ({discOffset / 4} samples)");
                        }
                    }
                }
            }

            dev.Close();
        }
    }
}