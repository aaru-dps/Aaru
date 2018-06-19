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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Threading;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Decoders.Bluray;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Decoders.Xbox;
using DiscImageChef.Devices;
using BCA = DiscImageChef.Decoders.Bluray.BCA;
using Cartridge = DiscImageChef.Decoders.DVD.Cartridge;
using DDS = DiscImageChef.Decoders.DVD.DDS;
using DMI = DiscImageChef.Decoders.Xbox.DMI;
using Spare = DiscImageChef.Decoders.DVD.Spare;

namespace DiscImageChef.Commands
{
    static class MediaInfo
    {
        internal static void DoMediaInfo(MediaInfoOptions options)
        {
            DicConsole.DebugWriteLine("Media-Info command", "--debug={0}",         options.Debug);
            DicConsole.DebugWriteLine("Media-Info command", "--verbose={0}",       options.Verbose);
            DicConsole.DebugWriteLine("Media-Info command", "--device={0}",        options.DevicePath);
            DicConsole.DebugWriteLine("Media-Info command", "--output-prefix={0}", options.OutputPrefix);

            if(options.DevicePath.Length == 2 && options.DevicePath[1] == ':' && options.DevicePath[0] != '/' &&
               char.IsLetter(options.DevicePath[0]))
                options.DevicePath = "\\\\.\\" + char.ToUpper(options.DevicePath[0]) + ':';

            Device dev = new Device(options.DevicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Core.Statistics.AddDevice(dev);

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
                    DoNvmeMediaInfo(options.OutputPrefix, dev);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    DoScsiMediaInfo(options.OutputPrefix, dev);
                    break;
                default: throw new NotSupportedException("Unknown device type.");
            }

            Core.Statistics.AddCommand("media-info");
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
            byte[]    cmdBuf;
            byte[]    senseBuf;
            bool      sense;
            MediaType dskType   = MediaType.Unknown;
            ulong     blocks    = 0;
            uint      blockSize = 0;
            int       resets    = 0;

            if(dev.IsRemovable)
            {
                deviceGotReset:
                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                if(sense)
                {
                    FixedSense? decSense = Sense.DecodeFixed(senseBuf);
                    if(decSense.HasValue)
                    {
                        // Just retry, for 5 times
                        if(decSense.Value.ASC == 0x29)
                        {
                            resets++;
                            if(resets < 5) goto deviceGotReset;
                        }

                        if(decSense.Value.ASC == 0x3A)
                        {
                            int leftRetries = 5;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Please insert media in drive");
                                return;
                            }
                        }
                        else if(decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                        {
                            int leftRetries = 10;
                            while(leftRetries > 0)
                            {
                                DicConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);
                                if(!sense) break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                                return;
                            }
                        }
                        else
                        {
                            DicConsole.ErrorWriteLine("Error testing unit was ready:\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                            return;
                        }
                    }
                    else
                    {
                        DicConsole.ErrorWriteLine("Unknown testing unit was ready.");
                        return;
                    }
                }
            }

            Modes.DecodedMode?    decMode = null;
            PeripheralDeviceTypes devType = dev.ScsiType;

            sense = dev.ModeSense10(out byte[] modeBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current,
                                    0x3F, 0xFF, 5, out _);
            if(sense || dev.Error)
                sense = dev.ModeSense10(out modeBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                        0x00, 5, out _);

            if(!sense && !dev.Error) decMode = Modes.DecodeMode10(modeBuf, devType);

            if(sense || dev.Error || !decMode.HasValue)
            {
                sense = dev.ModeSense6(out modeBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00,
                                       5, out _);
                if(sense || dev.Error)
                    sense = dev.ModeSense6(out modeBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F,
                                           0x00, 5, out _);
                if(sense || dev.Error) sense = dev.ModeSense(out modeBuf, out senseBuf, 5, out _);

                if(!sense && !dev.Error) decMode = Modes.DecodeMode6(modeBuf, devType);
            }

            if(!sense)
                DataFile.WriteTo("Media-Info command", outputPrefix, "_scsi_modesense.bin", "SCSI MODE SENSE", modeBuf);

            byte scsiMediumType     = 0;
            byte scsiDensityCode    = 0;
            bool containsFloppyPage = false;

            if(decMode.HasValue)
            {
                scsiMediumType = (byte)decMode.Value.Header.MediumType;
                if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length >= 1)
                    scsiDensityCode = (byte)decMode.Value.Header.BlockDescriptors[0].Density;

                if(decMode.Value.Pages != null)
                {
                    containsFloppyPage =
                        decMode.Value.Pages.Aggregate(containsFloppyPage,
                                                      (current, modePage) => current | (modePage.Page == 0x05));
                }
            }

            switch(dev.ScsiType)
            {
                case PeripheralDeviceTypes.DirectAccess:
                case PeripheralDeviceTypes.MultiMediaDevice:
                case PeripheralDeviceTypes.OCRWDevice:
                case PeripheralDeviceTypes.OpticalDevice:
                case PeripheralDeviceTypes.SimplifiedDevice:
                case PeripheralDeviceTypes.WriteOnceDevice:
                    sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out _);
                    if(!sense)
                    {
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readcapacity.bin", "SCSI READ CAPACITY",
                                         cmdBuf);
                        blocks    = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]);
                        blockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8)  + cmdBuf[7]);
                    }

                    if(sense || blocks == 0xFFFFFFFF)
                    {
                        sense = dev.ReadCapacity16(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense && blocks == 0)
                            if(dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
                            {
                                DicConsole.ErrorWriteLine("Unable to get media capacity");
                                DicConsole.ErrorWriteLine("{0}", Sense.PrettifySense(senseBuf));
                            }

                        if(!sense)
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readcapacity16.bin",
                                             "SCSI READ CAPACITY(16)", cmdBuf);
                            byte[] temp = new byte[8];

                            Array.Copy(cmdBuf, 0, temp, 0, 8);
                            Array.Reverse(temp);
                            blocks    = BitConverter.ToUInt64(temp, 0);
                            blockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
                        }
                    }

                    if(blocks != 0 && blockSize != 0)
                    {
                        blocks++;
                        DicConsole.WriteLine("Media has {0} blocks of {1} bytes/each. (for a total of {2} bytes)",
                                             blocks, blockSize, blocks * blockSize);
                    }

                    break;
                case PeripheralDeviceTypes.SequentialAccess:
                    byte[] medBuf;

                    sense = dev.ReportDensitySupport(out byte[] seqBuf, out senseBuf, false, dev.Timeout, out _);
                    if(!sense)
                    {
                        sense = dev.ReportDensitySupport(out medBuf, out senseBuf, true, dev.Timeout, out _);

                        if(!sense && !seqBuf.SequenceEqual(medBuf))
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_ssc_reportdensitysupport_media.bin",
                                             "SSC REPORT DENSITY SUPPORT (MEDIA)", seqBuf);
                            DensitySupport.DensitySupportHeader? dens = DensitySupport.DecodeDensity(seqBuf);
                            if(dens.HasValue)
                            {
                                DicConsole.WriteLine("Densities supported by currently inserted media:");
                                DicConsole.WriteLine(DensitySupport.PrettifyDensity(dens));
                            }
                        }
                    }

                    sense = dev.ReportDensitySupport(out seqBuf, out senseBuf, true, false, dev.Timeout, out _);
                    if(!sense)
                    {
                        sense = dev.ReportDensitySupport(out medBuf, out senseBuf, true, true, dev.Timeout, out _);

                        if(!sense && !seqBuf.SequenceEqual(medBuf))
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix,
                                             "_ssc_reportdensitysupport_medium_media.bin",
                                             "SSC REPORT DENSITY SUPPORT (MEDIUM & MEDIA)", seqBuf);
                            DensitySupport.MediaTypeSupportHeader? meds = DensitySupport.DecodeMediumType(seqBuf);
                            if(meds.HasValue)
                            {
                                DicConsole.WriteLine("Medium types currently inserted in device:");
                                DicConsole.WriteLine(DensitySupport.PrettifyMediumType(meds));
                            }

                            DicConsole.WriteLine(DensitySupport.PrettifyMediumType(seqBuf));
                        }
                    }

                    // TODO: Get a machine where 16-byte CDBs don't get DID_ABORT
                    /*
                sense = dev.ReadAttribute(out seqBuf, out senseBuf, ScsiAttributeAction.List, 0, dev.Timeout, out _);
                if (sense)
                    DicConsole.ErrorWriteLine("SCSI READ ATTRIBUTE:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_scsi_readattribute.bin", "SCSI READ ATTRIBUTE", seqBuf);
                }
                */
                    break;
            }

            if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
            {
                sense = dev.GetConfiguration(out cmdBuf, out senseBuf, 0, MmcGetConfigurationRt.Current, dev.Timeout,
                                             out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ GET CONFIGURATION:\n{0}",
                                              Sense.PrettifySense(senseBuf));
                else
                {
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_getconfiguration_current.bin",
                                     "SCSI GET CONFIGURATION", cmdBuf);

                    Features.SeparatedFeatures ftr = Features.Separate(cmdBuf);

                    DicConsole.DebugWriteLine("Media-Info command", "GET CONFIGURATION current profile is {0:X4}h",
                                              ftr.CurrentProfile);

                    switch(ftr.CurrentProfile)
                    {
                        case 0x0001:
                            dskType = MediaType.GENERIC_HDD;
                            break;
                        case 0x0005:
                            dskType = MediaType.CDMO;
                            break;
                        case 0x0008:
                            dskType = MediaType.CD;
                            break;
                        case 0x0009:
                            dskType = MediaType.CDR;
                            break;
                        case 0x000A:
                            dskType = MediaType.CDRW;
                            break;
                        case 0x0010:
                            dskType = MediaType.DVDROM;
                            break;
                        case 0x0011:
                            dskType = MediaType.DVDR;
                            break;
                        case 0x0012:
                            dskType = MediaType.DVDRAM;
                            break;
                        case 0x0013:
                        case 0x0014:
                            dskType = MediaType.DVDRW;
                            break;
                        case 0x0015:
                        case 0x0016:
                            dskType = MediaType.DVDRDL;
                            break;
                        case 0x0017:
                            dskType = MediaType.DVDRWDL;
                            break;
                        case 0x0018:
                            dskType = MediaType.DVDDownload;
                            break;
                        case 0x001A:
                            dskType = MediaType.DVDPRW;
                            break;
                        case 0x001B:
                            dskType = MediaType.DVDPR;
                            break;
                        case 0x0020:
                            dskType = MediaType.DDCD;
                            break;
                        case 0x0021:
                            dskType = MediaType.DDCDR;
                            break;
                        case 0x0022:
                            dskType = MediaType.DDCDRW;
                            break;
                        case 0x002A:
                            dskType = MediaType.DVDPRWDL;
                            break;
                        case 0x002B:
                            dskType = MediaType.DVDPRDL;
                            break;
                        case 0x0040:
                            dskType = MediaType.BDROM;
                            break;
                        case 0x0041:
                        case 0x0042:
                            dskType = MediaType.BDR;
                            break;
                        case 0x0043:
                            dskType = MediaType.BDRE;
                            break;
                        case 0x0050:
                            dskType = MediaType.HDDVDROM;
                            break;
                        case 0x0051:
                            dskType = MediaType.HDDVDR;
                            break;
                        case 0x0052:
                            dskType = MediaType.HDDVDRAM;
                            break;
                        case 0x0053:
                            dskType = MediaType.HDDVDRW;
                            break;
                        case 0x0058:
                            dskType = MediaType.HDDVDRDL;
                            break;
                        case 0x005A:
                            dskType = MediaType.HDDVDRWDL;
                            break;
                    }
                }

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.RecognizedFormatLayers, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command",
                                              "READ DISC STRUCTURE: Recognized Format Layers\n{0}",
                                              Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_formatlayers.bin",
                                     "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.WriteProtectionStatus, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Write Protection Status\n{0}",
                                              Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_writeprotection.bin",
                                     "SCSI READ DISC STRUCTURE", cmdBuf);

                // More like a drive information
                /*
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.CapabilityList, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Capability List\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_capabilitylist.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                */

                #region All DVD and HD DVD types
                if(dskType == MediaType.DVDDownload || dskType == MediaType.DVDPR    || dskType == MediaType.DVDPRDL  ||
                   dskType == MediaType.DVDPRW      || dskType == MediaType.DVDPRWDL || dskType == MediaType.DVDR     ||
                   dskType == MediaType.DVDRAM      || dskType == MediaType.DVDRDL   || dskType == MediaType.DVDROM   ||
                   dskType == MediaType.DVDRW       || dskType == MediaType.DVDRWDL  || dskType == MediaType.HDDVDR   ||
                   dskType == MediaType.HDDVDRAM    || dskType == MediaType.HDDVDRDL || dskType == MediaType.HDDVDROM ||
                   dskType == MediaType.HDDVDRW     || dskType == MediaType.HDDVDRWDL)
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out _);
                    if(sense)
                        DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: PFI\n{0}",
                                                  Sense.PrettifySense(senseBuf));
                    else
                    {
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_pfi.bin",
                                         "SCSI READ DISC STRUCTURE", cmdBuf);
                        PFI.PhysicalFormatInformation? decPfi = PFI.Decode(cmdBuf);
                        if(decPfi.HasValue)
                        {
                            DicConsole.WriteLine("PFI:\n{0}", PFI.Prettify(decPfi));

                            // False book types
                            if(dskType == MediaType.DVDROM)
                                switch(decPfi.Value.DiskCategory)
                                {
                                    case DiskCategory.DVDPR:
                                        dskType = MediaType.DVDPR;
                                        break;
                                    case DiskCategory.DVDPRDL:
                                        dskType = MediaType.DVDPRDL;
                                        break;
                                    case DiskCategory.DVDPRW:
                                        dskType = MediaType.DVDPRW;
                                        break;
                                    case DiskCategory.DVDPRWDL:
                                        dskType = MediaType.DVDPRWDL;
                                        break;
                                    case DiskCategory.DVDR:
                                        dskType = decPfi.Value.PartVersion == 6 ? MediaType.DVDRDL : MediaType.DVDR;
                                        break;
                                    case DiskCategory.DVDRAM:
                                        dskType = MediaType.DVDRAM;
                                        break;
                                    default:
                                        dskType = MediaType.DVDROM;
                                        break;
                                    case DiskCategory.DVDRW:
                                        dskType = decPfi.Value.PartVersion == 3 ? MediaType.DVDRWDL : MediaType.DVDRW;
                                        break;
                                    case DiskCategory.HDDVDR:
                                        dskType = MediaType.HDDVDR;
                                        break;
                                    case DiskCategory.HDDVDRAM:
                                        dskType = MediaType.HDDVDRAM;
                                        break;
                                    case DiskCategory.HDDVDROM:
                                        dskType = MediaType.HDDVDROM;
                                        break;
                                    case DiskCategory.HDDVDRW:
                                        dskType = MediaType.HDDVDRW;
                                        break;
                                    case DiskCategory.Nintendo:
                                        dskType = decPfi.Value.DiscSize == DVDSize.Eighty
                                                      ? MediaType.GOD
                                                      : MediaType.WOD;
                                        break;
                                    case DiskCategory.UMD:
                                        dskType = MediaType.UMD;
                                        break;
                                }
                        }
                    }

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout,
                                                  out _);
                    if(sense)
                        DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: DMI\n{0}",
                                                  Sense.PrettifySense(senseBuf));
                    else
                    {
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_dmi.bin",
                                         "SCSI READ DISC STRUCTURE", cmdBuf);
                        if(DMI.IsXbox(cmdBuf))
                        {
                            dskType = MediaType.XGD;
                            DicConsole.WriteLine("Xbox DMI:\n{0}", DMI.PrettifyXbox(cmdBuf));
                        }
                        else if(DMI.IsXbox360(cmdBuf))
                        {
                            dskType = MediaType.XGD2;
                            DicConsole.WriteLine("Xbox 360 DMI:\n{0}", DMI.PrettifyXbox360(cmdBuf));

                            // All XGD3 all have the same number of blocks
                            if(blocks == 25063   || // Locked (or non compatible drive)
                               blocks == 4229664 || // Xtreme unlock
                               blocks == 4246304)   // Wxripper unlock
                                dskType = MediaType.XGD3;
                        }
                    }
                }
                #endregion All DVD and HD DVD types

                #region DVD-ROM
                if(dskType == MediaType.DVDDownload || dskType == MediaType.DVDROM)
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.CopyrightInformation, 0, dev.Timeout, out _);
                    if(sense)
                        DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: CMI\n{0}",
                                                  Sense.PrettifySense(senseBuf));
                    else
                    {
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_cmi.bin",
                                         "SCSI READ DISC STRUCTURE", cmdBuf);
                        DicConsole.WriteLine("Lead-In CMI:\n{0}", CSS_CPRM.PrettifyLeadInCopyright(cmdBuf));
                    }
                }
                #endregion DVD-ROM

                #region DVD-ROM and HD DVD-ROM
                switch(dskType)
                {
                    case MediaType.DVDDownload:
                    case MediaType.DVDROM:
                    case MediaType.HDDVDROM:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.BurstCuttingArea, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: BCA\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_bca.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.DvdAacs, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: DVD AACS\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_aacs.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        break;
                    case MediaType.DVDRAM:
                    case MediaType.HDDVDRAM:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.DvdramDds, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: DDS\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdram_dds.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                            DicConsole.WriteLine("Disc Definition Structure:\n{0}", DDS.Prettify(cmdBuf));
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.DvdramMediumStatus, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Medium Status\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdram_status.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                            DicConsole.WriteLine("Medium Status:\n{0}", Cartridge.Prettify(cmdBuf));
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.DvdramSpareAreaInformation, 0, dev.Timeout,
                                                      out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: SAI\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdram_spare.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                            DicConsole.WriteLine("Spare Area Information:\n{0}", Spare.Prettify(cmdBuf));
                        }

                        break;
                    case MediaType.DVDR:
                    case MediaType.HDDVDR:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.LastBorderOutRmd, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command",
                                                      "READ DISC STRUCTURE: Last-Out Border RMD\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_lastrmd.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        break;
                }
                #endregion DVD-ROM and HD DVD-ROM

                #region Require drive authentication, won't work
                /*
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DiscKey, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Disc Key\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_disckey.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.SectorCopyrightInformation, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Sector CMI\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_sectorcmi.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.MediaIdentifier, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Media ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_mediaid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.MediaKeyBlock, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: MKB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_mkb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSVolId, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: AACS Volume ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_aacsvolid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMediaSerial, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: AACS Media Serial Number\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_aacssn.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMediaId, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: AACS Media ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_aacsmediaid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMKB, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: AACS MKB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_aacsmkb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSLBAExtents, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: AACS LBA Extents\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_aacslbaextents.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMKBCPRM, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: AACS CPRM MKB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_aacscprmmkb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSDataKeys, 0, dev.Timeout, out _);
                if(sense)
                    DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: AACS Data Keys\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_aacsdatakeys.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                */
                #endregion Require drive authentication, won't work

                #region DVD-RAM and HD DVD-RAM
                #endregion DVD-RAM and HD DVD-RAM

                #region DVD-R and HD DVD-R
                #endregion DVD-R and HD DVD-R

                #region DVD-R and DVD-RW
                if(dskType == MediaType.DVDR || dskType == MediaType.DVDRW)
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PreRecordedInfo, 0, dev.Timeout, out _);
                    if(sense)
                        DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Pre-Recorded Info\n{0}",
                                                  Sense.PrettifySense(senseBuf));
                    else
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_pri.bin",
                                         "SCSI READ DISC STRUCTURE", cmdBuf);
                }
                #endregion DVD-R and DVD-RW

                switch(dskType)
                {
                    #region DVD-R, DVD-RW and HD DVD-R
                    case MediaType.DVDR:
                    case MediaType.DVDRW:
                    case MediaType.HDDVDR:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.DvdrMediaIdentifier, 0, dev.Timeout,
                                                      out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: DVD-R Media ID\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdr_mediaid.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.DvdrPhysicalInformation, 0, dev.Timeout,
                                                      out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: DVD-R PFI\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdr_pfi.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        break;
                    #endregion DVD-R, DVD-RW and HD DVD-R

                    #region All DVD+
                    case MediaType.DVDPR:
                    case MediaType.DVDPRDL:
                    case MediaType.DVDPRW:
                    case MediaType.DVDPRWDL:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.Adip, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: ADIP\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd+_adip.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.Dcb, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: DCB\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd+_dcb.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        break;
                    #endregion All DVD+

                    #region HD DVD-ROM
                    case MediaType.HDDVDROM:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.HddvdCopyrightInformation, 0, dev.Timeout,
                                                      out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: HDDVD CMI\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_hddvd_cmi.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        break;
                    #endregion HD DVD-ROM
                }

                #region HD DVD-R
                if(dskType == MediaType.HDDVDR)
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.HddvdrMediumStatus, 0, dev.Timeout, out _);
                    if(sense)
                        DicConsole.DebugWriteLine("Media-Info command",
                                                  "READ DISC STRUCTURE: HDDVD-R Medium Status\n{0}",
                                                  Sense.PrettifySense(senseBuf));
                    else
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_hddvdr_status.bin",
                                         "SCSI READ DISC STRUCTURE", cmdBuf);
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.HddvdrLastRmd, 0, dev.Timeout, out _);
                    if(sense)
                        DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Last RMD\n{0}",
                                                  Sense.PrettifySense(senseBuf));
                    else
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_hddvdr_lastrmd.bin",
                                         "SCSI READ DISC STRUCTURE", cmdBuf);
                }
                #endregion HD DVD-R

                #region DVD-R DL, DVD-RW DL, DVD+R DL, DVD+RW DL
                if(dskType == MediaType.DVDPRDL || dskType == MediaType.DVDRDL || dskType == MediaType.DVDRWDL ||
                   dskType == MediaType.DVDPRWDL)
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdrLayerCapacity, 0, dev.Timeout, out _);
                    if(sense)
                        DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Layer Capacity\n{0}",
                                                  Sense.PrettifySense(senseBuf));
                    else
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvdr_layercap.bin",
                                         "SCSI READ DISC STRUCTURE", cmdBuf);
                }
                #endregion DVD-R DL, DVD-RW DL, DVD+R DL, DVD+RW DL

                switch(dskType)
                {
                    #region DVD-R DL
                    case MediaType.DVDRDL:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.MiddleZoneStart, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command",
                                                      "READ DISC STRUCTURE: Middle Zone Start\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_mzs.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.JumpIntervalSize, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command",
                                                      "READ DISC STRUCTURE: Jump Interval Size\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_jis.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.ManualLayerJumpStartLba, 0, dev.Timeout,
                                                      out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command",
                                                      "READ DISC STRUCTURE: Manual Layer Jump Start LBA\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_manuallj.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.RemapAnchorPoint, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command",
                                                      "READ DISC STRUCTURE: Remap Anchor Point\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix,
                                             "_readdiscstructure_dvd_remapanchor.bin", "SCSI READ DISC STRUCTURE",
                                             cmdBuf);
                        break;
                    #endregion DVD-R DL

                    #region All Blu-ray
                    case MediaType.BDR:
                    case MediaType.BDRE:
                    case MediaType.BDROM:
                    case MediaType.BDRXL:
                    case MediaType.BDREXL:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                      MmcDiscStructureFormat.DiscInformation, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: DI\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_di.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                            DicConsole.WriteLine("Blu-ray Disc Information:\n{0}", DI.Prettify(cmdBuf));
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                      MmcDiscStructureFormat.Pac, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: PAC\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_pac.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        break;
                    #endregion All Blu-ray
                }

                switch(dskType)
                {
                    #region BD-ROM only
                    case MediaType.BDROM:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                      MmcDiscStructureFormat.BdBurstCuttingArea, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: BCA\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_bca.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                            DicConsole.WriteLine("Blu-ray Burst Cutting Area:\n{0}", BCA.Prettify(cmdBuf));
                        }

                        break;
                    #endregion BD-ROM only

                    #region Writable Blu-ray only
                    case MediaType.BDR:
                    case MediaType.BDRE:
                    case MediaType.BDRXL:
                    case MediaType.BDREXL:
                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                      MmcDiscStructureFormat.BdDds, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: DDS\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_dds.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                            DicConsole.WriteLine("Blu-ray Disc Definition Structure:\n{0}",
                                                 Decoders.Bluray.DDS.Prettify(cmdBuf));
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                      MmcDiscStructureFormat.CartridgeStatus, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command",
                                                      "READ DISC STRUCTURE: Cartridge Status\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_cartstatus.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                            DicConsole.WriteLine("Blu-ray Cartridge Status:\n{0}",
                                                 Decoders.Bluray.Cartridge.Prettify(cmdBuf));
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                      MmcDiscStructureFormat.BdSpareAreaInformation, 0, dev.Timeout,
                                                      out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command",
                                                      "READ DISC STRUCTURE: Spare Area Information\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_spare.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                            DicConsole.WriteLine("Blu-ray Spare Area Information:\n{0}",
                                                 Decoders.Bluray.Spare.Prettify(cmdBuf));
                        }

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                      MmcDiscStructureFormat.RawDfl, 0, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: Raw DFL\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_bd_dfl.bin",
                                             "SCSI READ DISC STRUCTURE", cmdBuf);
                        sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf,
                                                        MmcDiscInformationDataTypes.TrackResources, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC INFORMATION 001b\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DicConsole.WriteLine("Track Resources Information:\n{0}", DiscInformation.Prettify(cmdBuf));
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscinformation_001b.bin",
                                             "SCSI READ DISC INFORMATION", cmdBuf);
                        }

                        sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf,
                                                        MmcDiscInformationDataTypes.PowResources, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ DISC INFORMATION 010b\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DicConsole.WriteLine("POW Resources Information:\n{0}", DiscInformation.Prettify(cmdBuf));
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscinformation_010b.bin",
                                             "SCSI READ DISC INFORMATION", cmdBuf);
                        }

                        break;
                    #endregion Writable Blu-ray only

                    #region CDs
                    case MediaType.CD:
                    case MediaType.CDR:
                    case MediaType.CDROM:
                    case MediaType.CDRW:
                    case MediaType.Unknown:
                        TOC.CDTOC? toc = null;

                        // We discarded all discs that falsify a TOC before requesting a real TOC
                        // No TOC, no CD (or an empty one)
                        bool tocSense = dev.ReadTocPmaAtip(out cmdBuf, out senseBuf, false, 0, 0, dev.Timeout, out _);
                        if(tocSense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ TOC/PMA/ATIP: TOC\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            toc = TOC.Decode(cmdBuf);
                            DicConsole.WriteLine("TOC:\n{0}", TOC.Prettify(toc));
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_toc.bin", "SCSI READ TOC/PMA/ATIP",
                                             cmdBuf);

                            // As we have a TOC we know it is a CD
                            if(dskType == MediaType.Unknown) dskType = MediaType.CD;
                        }

                        // ATIP exists on blank CDs
                        sense = dev.ReadAtip(out cmdBuf, out senseBuf, dev.Timeout, out _);
                        if(sense)
                            DicConsole.DebugWriteLine("Media-Info command", "READ TOC/PMA/ATIP: ATIP\n{0}",
                                                      Sense.PrettifySense(senseBuf));
                        else
                        {
                            DataFile.WriteTo("Media-Info command", outputPrefix, "_atip.bin", "SCSI READ TOC/PMA/ATIP",
                                             cmdBuf);
                            ATIP.CDATIP? atip = ATIP.Decode(cmdBuf);
                            if(atip.HasValue)
                            {
                                DicConsole.WriteLine("ATIP:\n{0}", ATIP.Prettify(atip));
                                // Only CD-R and CD-RW have ATIP
                                dskType = atip.Value.DiscType ? MediaType.CDRW : MediaType.CDR;
                            }
                        }

                        // We got a TOC, get information about a recorded/mastered CD
                        if(!tocSense)
                        {
                            sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf,
                                                            MmcDiscInformationDataTypes.DiscInformation, dev.Timeout,
                                                            out _);
                            if(sense)
                                DicConsole.DebugWriteLine("Media-Info command", "READ DISC INFORMATION 000b\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                            else
                            {
                                DiscInformation.StandardDiscInformation? discInfo = DiscInformation.Decode000b(cmdBuf);
                                if(discInfo.HasValue)
                                {
                                    DicConsole.WriteLine("Standard Disc Information:\n{0}",
                                                         DiscInformation.Prettify000b(discInfo));
                                    DataFile.WriteTo("Media-Info command", outputPrefix,
                                                     "_readdiscinformation_000b.bin", "SCSI READ DISC INFORMATION",
                                                     cmdBuf);

                                    // If it is a read-only CD, check CD type if available
                                    if(dskType == MediaType.CD)
                                        switch(discInfo.Value.DiscType)
                                        {
                                            case 0x10:
                                                dskType = MediaType.CDI;
                                                break;
                                            case 0x20:
                                                dskType = MediaType.CDROMXA;
                                                break;
                                        }
                                }
                            }

                            int sessions              = 1;
                            int firstTrackLastSession = 0;

                            sense = dev.ReadSessionInfo(out cmdBuf, out senseBuf, dev.Timeout, out _);
                            if(sense)
                                DicConsole.DebugWriteLine("Media-Info command", "READ TOC/PMA/ATIP: Session info\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                            else
                            {
                                DataFile.WriteTo("Media-Info command", outputPrefix, "_session.bin",
                                                 "SCSI READ TOC/PMA/ATIP", cmdBuf);
                                Session.CDSessionInfo? session = Session.Decode(cmdBuf);
                                DicConsole.WriteLine("Session information:\n{0}", Session.Prettify(session));
                                if(session.HasValue)
                                {
                                    sessions              = session.Value.LastCompleteSession;
                                    firstTrackLastSession = session.Value.TrackDescriptors[0].TrackNumber;
                                }
                            }

                            if(dskType == MediaType.CD)
                            {
                                bool hasDataTrack                  = false;
                                bool hasAudioTrack                 = false;
                                bool allFirstSessionTracksAreAudio = true;
                                bool hasVideoTrack                 = false;

                                if(toc.HasValue)
                                    foreach(TOC.CDTOCTrackDataDescriptor track in toc.Value.TrackDescriptors)
                                    {
                                        if(track.TrackNumber == 1 &&
                                           ((TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrack ||
                                            (TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrackIncremental))
                                            allFirstSessionTracksAreAudio &= firstTrackLastSession != 1;

                                        if((TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrack ||
                                           (TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrackIncremental)
                                        {
                                            hasDataTrack                  =  true;
                                            allFirstSessionTracksAreAudio &= track.TrackNumber >= firstTrackLastSession;
                                        }
                                        else hasAudioTrack = true;

                                        hasVideoTrack |= track.ADR == 4;
                                    }

                                if(hasDataTrack && hasAudioTrack && allFirstSessionTracksAreAudio && sessions == 2)
                                    dskType = MediaType.CDPLUS;
                                if(!hasDataTrack && hasAudioTrack && sessions == 1) dskType = MediaType.CDDA;
                                if(hasDataTrack && !hasAudioTrack && sessions == 1) dskType = MediaType.CDROM;
                                if(hasVideoTrack && !hasDataTrack && sessions == 1) dskType = MediaType.CDV;
                            }

                            sense = dev.ReadRawToc(out cmdBuf, out senseBuf, 1, dev.Timeout, out _);
                            if(sense)
                                DicConsole.DebugWriteLine("Media-Info command", "READ TOC/PMA/ATIP: Raw TOC\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                            else
                            {
                                DataFile.WriteTo("Media-Info command", outputPrefix, "_rawtoc.bin",
                                                 "SCSI READ TOC/PMA/ATIP", cmdBuf);
                                DicConsole.WriteLine("Raw TOC:\n{0}", FullTOC.Prettify(cmdBuf));

                                FullTOC.CDFullTOC? fullToc = FullTOC.Decode(cmdBuf);
                                if(fullToc.HasValue)
                                {
                                    FullTOC.TrackDataDescriptor a0Track =
                                        fullToc.Value.TrackDescriptors
                                               .FirstOrDefault(t => t.POINT == 0xA0 && t.ADR == 1);
                                    if(a0Track.POINT == 0xA0)
                                        switch(a0Track.PSEC)
                                        {
                                            case 0x10:
                                                dskType = MediaType.CDI;
                                                break;
                                            case 0x20:
                                                dskType = MediaType.CDROMXA;
                                                break;
                                        }
                                }
                            }

                            sense = dev.ReadPma(out cmdBuf, out senseBuf, dev.Timeout, out _);
                            if(sense)
                                DicConsole.DebugWriteLine("Media-Info command", "READ TOC/PMA/ATIP: PMA\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                            else
                            {
                                DataFile.WriteTo("Media-Info command", outputPrefix, "_pma.bin",
                                                 "SCSI READ TOC/PMA/ATIP", cmdBuf);
                                DicConsole.WriteLine("PMA:\n{0}", PMA.Prettify(cmdBuf));
                            }

                            sense = dev.ReadCdText(out cmdBuf, out senseBuf, dev.Timeout, out _);
                            if(sense)
                                DicConsole.DebugWriteLine("Media-Info command", "READ TOC/PMA/ATIP: CD-TEXT\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                            else
                            {
                                DataFile.WriteTo("Media-Info command", outputPrefix, "_cdtext.bin",
                                                 "SCSI READ TOC/PMA/ATIP", cmdBuf);
                                if(CDTextOnLeadIn.Decode(cmdBuf).HasValue)
                                    DicConsole.WriteLine("CD-TEXT on Lead-In:\n{0}", CDTextOnLeadIn.Prettify(cmdBuf));
                            }

                            sense = dev.ReadMcn(out string mcn, out _, out _, dev.Timeout, out _);
                            if(!sense && mcn != null && mcn != "0000000000000") DicConsole.WriteLine("MCN: {0}", mcn);

                            for(byte i = toc.Value.FirstTrack; i <= toc.Value.LastTrack; i++)
                            {
                                sense = dev.ReadIsrc(i, out string isrc, out _, out _, dev.Timeout, out _);
                                if(!sense && isrc != null && isrc != "000000000000")
                                    DicConsole.WriteLine("Track's {0} ISRC: {1}", i, isrc);
                            }
                        }

                        break;
                    #endregion CDs
                }

                #region Nintendo
                if(dskType == MediaType.Unknown && blocks > 0)
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out _);
                    if(sense)
                        DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: PFI\n{0}",
                                                  Sense.PrettifySense(senseBuf));
                    else
                    {
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_pfi.bin",
                                         "SCSI READ DISC STRUCTURE", cmdBuf);
                        PFI.PhysicalFormatInformation? nintendoPfi = PFI.Decode(cmdBuf);
                        if(nintendoPfi != null)
                        {
                            DicConsole.WriteLine("PFI:\n{0}", PFI.Prettify(cmdBuf));
                            if(nintendoPfi.Value.DiskCategory == DiskCategory.Nintendo &&
                               nintendoPfi.Value.PartVersion  == 15)
                                switch(nintendoPfi.Value.DiscSize)
                                {
                                    case DVDSize.Eighty:
                                        dskType = MediaType.GOD;
                                        break;
                                    case DVDSize.OneTwenty:
                                        dskType = MediaType.WOD;
                                        break;
                                }
                        }
                    }

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout,
                                                  out _);
                    if(sense)
                        DicConsole.DebugWriteLine("Media-Info command", "READ DISC STRUCTURE: DMI\n{0}",
                                                  Sense.PrettifySense(senseBuf));
                    else
                        DataFile.WriteTo("Media-Info command", outputPrefix, "_readdiscstructure_dvd_dmi.bin",
                                         "SCSI READ DISC STRUCTURE", cmdBuf);
                }
                #endregion Nintendo
            }

            #region Xbox
            switch(dskType)
            {
                case MediaType.XGD:
                case MediaType.XGD2:
                case MediaType.XGD3:
                    // We need to get INQUIRY to know if it is a Kreon drive
                    Inquiry.SCSIInquiry? inq;

                    sense = dev.ScsiInquiry(out byte[] inqBuffer, out senseBuf);
                    if(!sense)
                    {
                        inq = Inquiry.Decode(inqBuffer);
                        if(inq.HasValue && inq.Value.KreonPresent)
                        {
                            sense = dev.KreonExtractSs(out cmdBuf, out senseBuf, dev.Timeout, out _);
                            if(sense)
                                DicConsole.DebugWriteLine("Media-Info command", "KREON EXTRACT SS:\n{0}",
                                                          Sense.PrettifySense(senseBuf));
                            else
                                DataFile.WriteTo("Media-Info command", outputPrefix, "_xbox_ss.bin", "KREON EXTRACT SS",
                                                 cmdBuf);

                            if(SS.Decode(cmdBuf).HasValue)
                                DicConsole.WriteLine("Xbox Security Sector:\n{0}", SS.Prettify(cmdBuf));

                            ulong l0Video, l1Video, middleZone, gameSize, totalSize, layerBreak;

                            // Get video partition size
                            DicConsole.DebugWriteLine("Dump-media command", "Getting video partition size");
                            sense = dev.KreonLock(out senseBuf, dev.Timeout, out _);
                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Cannot lock drive, not continuing.");
                                return;
                            }

                            sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out _);
                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                                return;
                            }

                            totalSize = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]);
                            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                          MmcDiscStructureFormat.PhysicalInformation, 0, 0, out _);
                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Cannot get PFI.");
                                return;
                            }

                            DicConsole.DebugWriteLine("Dump-media command", "Video partition total size: {0} sectors",
                                                      totalSize);
                            l0Video = PFI.Decode(cmdBuf).Value.Layer0EndPSN -
                                      PFI.Decode(cmdBuf).Value.DataAreaStartPSN + 1;
                            l1Video = totalSize - l0Video + 1;

                            // Get game partition size
                            DicConsole.DebugWriteLine("Dump-media command", "Getting game partition size");
                            sense = dev.KreonUnlockXtreme(out senseBuf, dev.Timeout, out _);
                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                                return;
                            }

                            sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out _);
                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                                return;
                            }

                            gameSize = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]) +
                                       1;
                            DicConsole.DebugWriteLine("Dump-media command", "Game partition total size: {0} sectors",
                                                      gameSize);

                            // Get middle zone size
                            DicConsole.DebugWriteLine("Dump-media command", "Getting middle zone size");
                            sense = dev.KreonUnlockWxripper(out senseBuf, dev.Timeout, out _);
                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Cannot unlock drive, not continuing.");
                                return;
                            }

                            sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out _);
                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Cannot get disc capacity.");
                                return;
                            }

                            totalSize = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]);
                            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                          MmcDiscStructureFormat.PhysicalInformation, 0, 0, out _);
                            if(sense)
                            {
                                DicConsole.ErrorWriteLine("Cannot get PFI.");
                                return;
                            }

                            DicConsole.DebugWriteLine("Dump-media command", "Unlocked total size: {0} sectors",
                                                      totalSize);
                            middleZone =
                                totalSize -
                                (PFI.Decode(cmdBuf).Value.Layer0EndPSN -
                                 PFI.Decode(cmdBuf).Value.DataAreaStartPSN + 1) - gameSize + 1;

                            totalSize  = l0Video + l1Video + middleZone * 2 + gameSize;
                            layerBreak = l0Video + middleZone               + gameSize / 2;

                            DicConsole.WriteLine("Video layer 0 size: {0} sectors", l0Video);
                            DicConsole.WriteLine("Video layer 1 size: {0} sectors", l1Video);
                            DicConsole.WriteLine("Middle zone size: {0} sectors",   middleZone);
                            DicConsole.WriteLine("Game data size: {0} sectors",     gameSize);
                            DicConsole.WriteLine("Total size: {0} sectors",         totalSize);
                            DicConsole.WriteLine("Real layer break: {0}",           layerBreak);
                            DicConsole.WriteLine();
                        }
                    }

                    break;
                case MediaType.Unknown:
                    dskType = MediaTypeFromScsi.Get((byte)dev.ScsiType, dev.Manufacturer, dev.Model, scsiMediumType,
                                                    scsiDensityCode, blocks, blockSize);
                    break;
            }
            #endregion Xbox

            if(dskType == MediaType.Unknown && dev.IsUsb && containsFloppyPage) dskType = MediaType.FlashDrive;

            DicConsole.WriteLine("Media identified as {0}", dskType);
            Core.Statistics.AddMedia(dskType, true);

            sense = dev.ReadMediaSerialNumber(out cmdBuf, out senseBuf, dev.Timeout, out _);
            if(sense)
                DicConsole.DebugWriteLine("Media-Info command", "READ MEDIA SERIAL NUMBER\n{0}",
                                          Sense.PrettifySense(senseBuf));
            else
            {
                DataFile.WriteTo("Media-Info command", outputPrefix, "_mediaserialnumber.bin",
                                 "SCSI READ MEDIA SERIAL NUMBER", cmdBuf);
                if(cmdBuf.Length < 4) return;

                DicConsole.Write("Media Serial Number: ");
                for(int i = 4; i < cmdBuf.Length; i++) DicConsole.Write("{0:X2}", cmdBuf[i]);

                DicConsole.WriteLine();
            }
        }
    }
}