// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MediaInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'media-info' verb.
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

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Media.Info;
using DiscImageChef.Decoders.Bluray;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Decoders.Xbox;
using DiscImageChef.Devices;
using Mono.Options;
using BCA = DiscImageChef.Decoders.Bluray.BCA;
using Cartridge = DiscImageChef.Decoders.DVD.Cartridge;
using DDS = DiscImageChef.Decoders.DVD.DDS;
using DMI = DiscImageChef.Decoders.Xbox.DMI;
using Spare = DiscImageChef.Decoders.DVD.Spare;

namespace DiscImageChef.Commands
{
    class MediaInfoCommand : Command
    {
        string devicePath;
        string outputPrefix;
        bool   showHelp;

        public MediaInfoCommand() : base("media-info", "Gets information about the media inserted on a device.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS] devicepath",
                "",
                Help,
                {"output-prefix|w=", "Write binary responses from device with that prefix.", s => outputPrefix = s},
                {
                    "help|h|?", "Show this message and exit.",
                    v => showHelp = v != null
                }
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = Options.Parse(arguments);

            if(showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);
                return (int)ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();
            if(MainClass.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
            if(MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            if(extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int)ErrorNumber.UnexpectedArgumentCount;
            }

            if(extra.Count == 0)
            {
                DicConsole.ErrorWriteLine("Missing device path.");
                return (int)ErrorNumber.MissingArgument;
            }

            devicePath = extra[0];

            DicConsole.DebugWriteLine("Media-Info command", "--debug={0}",         MainClass.Debug);
            DicConsole.DebugWriteLine("Media-Info command", "--device={0}",        devicePath);
            DicConsole.DebugWriteLine("Media-Info command", "--output-prefix={0}", outputPrefix);
            DicConsole.DebugWriteLine("Media-Info command", "--verbose={0}",       MainClass.Verbose);

            if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Device dev = new Device(devicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
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
                    DoScsiMediaInfo(outputPrefix, dev);
                    break;
                default: throw new NotSupportedException("Unknown device type.");
            }

            Statistics.AddCommand("media-info");

            return (int)ErrorNumber.NoError;
        }

        static void DoAtaMediaInfo()
        {
            DicConsole.ErrorWriteLine("Please use device-info command for ATA devices.");
        }

        static void DoNvmeMediaInfo(string outputPrefix, Device dev)
        {
            throw new NotImplementedException("NVMe devices not yet supported.");
        }

        static void DoSdMediaInfo()
        {
            DicConsole.ErrorWriteLine("Please use device-info command for MMC/SD devices.");
        }

        static void DoScsiMediaInfo(string outputPrefix, Device dev)
        {
            ScsiInfo scsiInfo = new ScsiInfo(dev);

            if(!scsiInfo.MediaInserted) return;

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
                    if(scsiInfo.ReadCapacity != null)
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readcapacity.bin", "SCSI READ CAPACITY",
                                         scsiInfo.ReadCapacity);

                    if(scsiInfo.ReadCapacity16 != null)
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readcapacity16.bin",
                                         "SCSI READ CAPACITY(16)", scsiInfo.ReadCapacity16);

                    if(scsiInfo.Blocks != 0 && scsiInfo.BlockSize != 0)
                        DicConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2} bytes)",
                                             scsiInfo.Blocks, scsiInfo.BlockSize, scsiInfo.Blocks * scsiInfo.BlockSize);

                    break;
                case PeripheralDeviceTypes.SequentialAccess:
                    if(scsiInfo.DensitySupport != null)
                    {
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_ssc_reportdensitysupport_media.bin",
                                         "SSC REPORT DENSITY SUPPORT (MEDIA)", scsiInfo.DensitySupport);
                        if(scsiInfo.DensitySupportHeader.HasValue)
                        {
                            DicConsole.WriteLine("Densities supported by currently inserted media:");
                            DicConsole.WriteLine(DensitySupport.PrettifyDensity(scsiInfo.DensitySupportHeader));
                        }
                    }

                    if(scsiInfo.MediaTypeSupport != null)
                    {
                        DataFile.WriteTo("Media-Info command", outputPrefix,
                                         "_ssc_reportdensitysupport_medium_media.bin",
                                         "SSC REPORT DENSITY SUPPORT (MEDIUM & MEDIA)", scsiInfo.MediaTypeSupport);
                        if(scsiInfo.MediaTypeSupportHeader.HasValue)
                        {
                            DicConsole.WriteLine("Medium types currently inserted in device:");
                            DicConsole.WriteLine(DensitySupport.PrettifyMediumType(scsiInfo.MediaTypeSupportHeader));
                        }

                        DicConsole.WriteLine(DensitySupport.PrettifyMediumType(scsiInfo.MediaTypeSupport));
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
                        DicConsole.WriteLine("PFI:\n{0}", PFI.Prettify(scsiInfo.DecodedPfi));
                }

                if(scsiInfo.DvdDmi != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_dmi.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdDmi);
                    if(DMI.IsXbox(scsiInfo.DvdDmi))
                        DicConsole.WriteLine("Xbox DMI:\n{0}", DMI.PrettifyXbox(scsiInfo.DvdDmi));
                    else if(DMI.IsXbox360(scsiInfo.DvdDmi))
                        DicConsole.WriteLine("Xbox 360 DMI:\n{0}", DMI.PrettifyXbox360(scsiInfo.DvdDmi));
                }

                if(scsiInfo.DvdCmi != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_cmi.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdCmi);
                    DicConsole.WriteLine("Lead-In CMI:\n{0}", CSS_CPRM.PrettifyLeadInCopyright(scsiInfo.DvdCmi));
                }

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
                    DicConsole.WriteLine("Disc Definition Structure:\n{0}", DDS.Prettify(scsiInfo.DvdRamDds));
                }

                if(scsiInfo.DvdRamCartridgeStatus != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdram_status.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdRamCartridgeStatus);
                    DicConsole.WriteLine("Medium Status:\n{0}", Cartridge.Prettify(scsiInfo.DvdRamCartridgeStatus));
                }

                if(scsiInfo.DvdRamSpareArea != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdram_spare.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdRamSpareArea);
                    DicConsole.WriteLine("Spare Area Information:\n{0}", Spare.Prettify(scsiInfo.DvdRamSpareArea));
                }

                if(scsiInfo.LastBorderOutRmd != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_lastrmd.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.LastBorderOutRmd);

                if(scsiInfo.DvdPreRecordedInfo != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_pri.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdPreRecordedInfo);

                if(scsiInfo.DvdrMediaIdentifier != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdr_mediaid.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdrMediaIdentifier);
                if(scsiInfo.DvdrPhysicalInformation != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdr_pfi.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.DvdrPhysicalInformation);
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
                    DicConsole.WriteLine("Blu-ray Disc Information:\n{0}", DI.Prettify(scsiInfo.BlurayDiscInformation));
                }

                if(scsiInfo.BlurayPac != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_pac.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayPac);

                if(scsiInfo.BlurayBurstCuttingArea != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_bca.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayBurstCuttingArea);
                    DicConsole.WriteLine("Blu-ray Burst Cutting Area:\n{0}",
                                         BCA.Prettify(scsiInfo.BlurayBurstCuttingArea));
                }

                if(scsiInfo.BlurayDds != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_dds.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayDds);
                    DicConsole.WriteLine("Blu-ray Disc Definition Structure:\n{0}",
                                         Decoders.Bluray.DDS.Prettify(scsiInfo.BlurayDds));
                }

                if(scsiInfo.BlurayCartridgeStatus != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_cartstatus.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayCartridgeStatus);
                    DicConsole.WriteLine("Blu-ray Cartridge Status:\n{0}",
                                         Decoders.Bluray.Cartridge.Prettify(scsiInfo.BlurayCartridgeStatus));
                }

                if(scsiInfo.BluraySpareAreaInformation != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_spare.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BluraySpareAreaInformation);
                    DicConsole.WriteLine("Blu-ray Spare Area Information:\n{0}",
                                         Decoders.Bluray.Spare.Prettify(scsiInfo.BluraySpareAreaInformation));
                }

                if(scsiInfo.BlurayRawDfl != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_dfl.bin",
                                     "SCSI READ DISC STRUCTURE", scsiInfo.BlurayRawDfl);

                if(scsiInfo.BlurayTrackResources != null)
                {
                    DicConsole.WriteLine("Track Resources Information:\n{0}",
                                         DiscInformation.Prettify(scsiInfo.BlurayTrackResources));
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscinformation_001b.bin",
                                     "SCSI READ DISC INFORMATION", scsiInfo.BlurayTrackResources);
                }

                if(scsiInfo.BlurayPowResources != null)
                {
                    DicConsole.WriteLine("POW Resources Information:\n{0}",
                                         DiscInformation.Prettify(scsiInfo.BlurayPowResources));
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscinformation_010b.bin",
                                     "SCSI READ DISC INFORMATION", scsiInfo.BlurayPowResources);
                }

                if(scsiInfo.Toc != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_toc.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.Toc);
                    if(scsiInfo.DecodedToc.HasValue)
                        DicConsole.WriteLine("TOC:\n{0}", TOC.Prettify(scsiInfo.DecodedToc));
                }

                if(scsiInfo.Atip != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_atip.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.Atip);
                    if(scsiInfo.DecodedAtip.HasValue)
                        DicConsole.WriteLine("ATIP:\n{0}", ATIP.Prettify(scsiInfo.DecodedAtip));
                }

                if(scsiInfo.CompactDiscInformation != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscinformation_000b.bin",
                                     "SCSI READ DISC INFORMATION", scsiInfo.CompactDiscInformation);
                    if(scsiInfo.DecodedCompactDiscInformation.HasValue)
                        DicConsole.WriteLine("Standard Disc Information:\n{0}",
                                             DiscInformation.Prettify000b(scsiInfo.DecodedCompactDiscInformation));
                }

                if(scsiInfo.Session != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_session.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.Session);
                    if(scsiInfo.DecodedSession.HasValue)
                        DicConsole.WriteLine("Session information:\n{0}", Session.Prettify(scsiInfo.DecodedSession));
                }

                if(scsiInfo.RawToc != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_rawtoc.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.RawToc);
                    if(scsiInfo.FullToc.HasValue)
                        DicConsole.WriteLine("Raw TOC:\n{0}", FullTOC.Prettify(scsiInfo.RawToc));
                }

                if(scsiInfo.Pma != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_pma.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.Pma);
                    DicConsole.WriteLine("PMA:\n{0}", PMA.Prettify(scsiInfo.Pma));
                }

                if(scsiInfo.CdTextLeadIn != null)
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_cdtext.bin", "SCSI READ TOC/PMA/ATIP",
                                     scsiInfo.CdTextLeadIn);
                    if(scsiInfo.DecodedCdTextLeadIn.HasValue)
                        DicConsole.WriteLine("CD-TEXT on Lead-In:\n{0}",
                                             CDTextOnLeadIn.Prettify(scsiInfo.DecodedCdTextLeadIn));
                }

                if(!string.IsNullOrEmpty(scsiInfo.Mcn)) DicConsole.WriteLine("MCN: {0}", scsiInfo.Mcn);

                if(scsiInfo.Isrcs != null)
                    foreach(KeyValuePair<byte, string> isrc in scsiInfo.Isrcs)
                        DicConsole.WriteLine("Track's {0} ISRC: {1}", isrc.Key, isrc.Value);

                if(scsiInfo.XboxSecuritySector != null)
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_xbox_ss.bin", "KREON EXTRACT SS",
                                     scsiInfo.XboxSecuritySector);

                if(scsiInfo.DecodedXboxSecuritySector.HasValue)
                    DicConsole.WriteLine("Xbox Security Sector:\n{0}", SS.Prettify(scsiInfo.DecodedXboxSecuritySector));

                if(scsiInfo.XgdInfo != null)
                {
                    DicConsole.WriteLine("Video layer 0 size: {0} sectors", scsiInfo.XgdInfo.L0Video);
                    DicConsole.WriteLine("Video layer 1 size: {0} sectors", scsiInfo.XgdInfo.L1Video);
                    DicConsole.WriteLine("Middle zone size: {0} sectors",   scsiInfo.XgdInfo.MiddleZone);
                    DicConsole.WriteLine("Game data size: {0} sectors",     scsiInfo.XgdInfo.GameSize);
                    DicConsole.WriteLine("Total size: {0} sectors",         scsiInfo.XgdInfo.TotalSize);
                    DicConsole.WriteLine("Real layer break: {0}",           scsiInfo.XgdInfo.LayerBreak);
                    DicConsole.WriteLine();
                }
            }

            if(scsiInfo.MediaSerialNumber != null)
            {
                DataFile.WriteTo("Media-Info command", outputPrefix, "_mediaserialnumber.bin",
                                 "SCSI READ MEDIA SERIAL NUMBER", scsiInfo.MediaSerialNumber);

                DicConsole.Write("Media Serial Number: ");
                for(int i = 4; i < scsiInfo.MediaSerialNumber.Length; i++)
                    DicConsole.Write("{0:X2}", scsiInfo.MediaSerialNumber[i]);

                DicConsole.WriteLine();
            }

            DicConsole.WriteLine("Media identified as {0}", scsiInfo.MediaType);
            Statistics.AddMedia(scsiInfo.MediaType, true);

            dev.Close();
        }
    }
}