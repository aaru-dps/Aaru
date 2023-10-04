// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ScsiInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core.
//
// --[ Description ] ----------------------------------------------------------
//
//     Retrieves the media info for a SCSI device.
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
using System.Linq;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core.Media.Detection;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Decoders.Xbox;
using Aaru.Devices;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;
using DMI = Aaru.Decoders.Xbox.DMI;
using DVDDecryption = Aaru.Decryption.DVD.Dump;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;

namespace Aaru.Core.Media.Info;

/// <summary>Retrieves information from a SCSI device</summary>
public sealed class ScsiInfo
{
    const string MODULE_NAME = "Media information";

    /// <summary>Initializes this class with the specific device, and fills in the information</summary>
    /// <param name="dev">Device</param>
    public ScsiInfo(Device dev)
    {
        if(dev.Type != DeviceType.SCSI && dev.Type != DeviceType.ATAPI)
            return;

        MediaType     = MediaType.Unknown;
        MediaInserted = false;
        var    resets = 0;
        bool   sense;
        byte[] cmdBuf;
        byte[] senseBuf;
        var    containsFloppyPage    = false;
        var    sessions              = 1;
        var    firstTrackLastSession = 1;

        if(dev.IsRemovable)
        {
        deviceGotReset:
            sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);

            if(sense)
            {
                DecodedSense? decSense = Sense.Decode(senseBuf);

                if(decSense.HasValue)
                {
                    // Just retry, for 5 times
                    if(decSense?.ASC == 0x29)
                    {
                        resets++;

                        if(resets < 5)
                            goto deviceGotReset;
                    }

                    switch(decSense?.ASC)
                    {
                        case 0x3A:
                        {
                            var leftRetries = 5;

                            while(leftRetries > 0)
                            {
                                //AaruConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);

                                if(!sense)
                                    break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                AaruConsole.ErrorWriteLine(Localization.Core.Please_insert_media_in_drive);

                                return;
                            }

                            break;
                        }
                        case 0x04 when decSense?.ASCQ == 0x01:
                        {
                            var leftRetries = 10;

                            while(leftRetries > 0)
                            {
                                //AaruConsole.WriteLine("\rWaiting for drive to become ready");
                                Thread.Sleep(2000);
                                sense = dev.ScsiTestUnitReady(out senseBuf, dev.Timeout, out _);

                                if(!sense)
                                    break;

                                leftRetries--;
                            }

                            if(sense)
                            {
                                AaruConsole.ErrorWriteLine(Localization.Core.Error_testing_unit_was_ready_0,
                                                           Sense.PrettifySense(senseBuf));

                                return;
                            }

                            break;
                        }
                        default:
                            AaruConsole.ErrorWriteLine(Localization.Core.Error_testing_unit_was_ready_0,
                                                       Sense.PrettifySense(senseBuf));

                            return;
                    }
                }
                else
                {
                    AaruConsole.ErrorWriteLine(Localization.Core.Unknown_sense_testing_unit_was_ready);

                    return;
                }
            }
        }

        MediaInserted = true;

        DeviceInfo = new DeviceInfo(dev);

        byte scsiMediumType  = 0;
        byte scsiDensityCode = 0;

        if(DeviceInfo.ScsiMode.HasValue)
        {
            scsiMediumType = (byte)DeviceInfo.ScsiMode.Value.Header.MediumType;

            if(DeviceInfo.ScsiMode?.Header.BlockDescriptors?.Length > 0)
                scsiDensityCode = (byte)DeviceInfo.ScsiMode.Value.Header.BlockDescriptors[0].Density;

            if(DeviceInfo.ScsiMode?.Pages != null)
                containsFloppyPage = DeviceInfo.ScsiMode.Value.Pages.Any(p => p.Page == 0x05);
        }

        Blocks    = 0;
        BlockSize = 0;

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
                    ReadCapacity = cmdBuf;

                    Blocks = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]) & 0xFFFFFFFF;

                    BlockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
                }

                sense = dev.ReadCapacity16(out cmdBuf, out senseBuf, dev.Timeout, out _);

                if(!sense)
                    ReadCapacity16 = cmdBuf;

                if(ReadCapacity == null || Blocks is 0xFFFFFFFF or 0)
                {
                    if(ReadCapacity16 == null && Blocks == 0)
                    {
                        if(dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
                        {
                            AaruConsole.ErrorWriteLine(Localization.Core.Unable_to_get_media_capacity);
                            AaruConsole.ErrorWriteLine("{0}", Sense.PrettifySense(senseBuf));
                        }
                    }

                    if(ReadCapacity16 != null)
                    {
                        var temp = new byte[8];

                        Array.Copy(cmdBuf, 0, temp, 0, 8);
                        Array.Reverse(temp);
                        Blocks    = BitConverter.ToUInt64(temp, 0);
                        BlockSize = (uint)((cmdBuf[8] << 24) + (cmdBuf[9] << 16) + (cmdBuf[10] << 8) + cmdBuf[11]);
                    }
                }

                if(Blocks != 0 && BlockSize != 0)
                    Blocks++;

                break;
            case PeripheralDeviceTypes.SequentialAccess:
                byte[] medBuf;

                sense = dev.ReportDensitySupport(out byte[] seqBuf, out senseBuf, false, dev.Timeout, out _);

                if(!sense)
                {
                    sense = dev.ReportDensitySupport(out medBuf, out senseBuf, true, dev.Timeout, out _);

                    if(!sense && !seqBuf.SequenceEqual(medBuf))
                    {
                        DensitySupport       = seqBuf;
                        DensitySupportHeader = Decoders.SCSI.SSC.DensitySupport.DecodeDensity(seqBuf);
                    }
                }

                sense = dev.ReportDensitySupport(out seqBuf, out senseBuf, true, false, dev.Timeout, out _);

                if(!sense)
                {
                    sense = dev.ReportDensitySupport(out medBuf, out senseBuf, true, true, dev.Timeout, out _);

                    if(!sense && !seqBuf.SequenceEqual(medBuf))
                    {
                        MediaTypeSupport       = medBuf;
                        MediaTypeSupportHeader = Decoders.SCSI.SSC.DensitySupport.DecodeMediumType(seqBuf);
                    }
                }

                // TODO: Get a machine where 16-byte CDBs don't get DID_ABORT
                /*
            sense = dev.ReadAttribute(out seqBuf, out senseBuf, ScsiAttributeAction.List, 0, dev.Timeout, out _);
            if (sense)
                AaruConsole.ErrorWriteLine("SCSI READ ATTRIBUTE:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
            {
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_scsi_readattribute.bin", "SCSI READ ATTRIBUTE", seqBuf);
            }
            */
                break;
            case PeripheralDeviceTypes.BridgingExpander when dev.Model.StartsWith("MDM", StringComparison.Ordinal) ||
                                                             dev.Model.StartsWith("MDH", StringComparison.Ordinal):
                sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out _);

                if(!sense)
                {
                    ReadCapacity = cmdBuf;

                    Blocks = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]) & 0xFFFFFFFF;

                    BlockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
                }

                break;
        }

        if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
        {
            sense = dev.GetConfiguration(out cmdBuf, out senseBuf, 0, MmcGetConfigurationRt.Current, dev.Timeout,
                                         out _);

            if(sense)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ GET CONFIGURATION:\n{0}", Sense.PrettifySense(senseBuf));

                if(dev.IsUsb && scsiMediumType is 0x40 or 0x41 or 0x42)
                    MediaType = MediaType.FlashDrive;
            }
            else
            {
                MmcConfiguration = cmdBuf;
                Features.SeparatedFeatures ftr = Features.Separate(cmdBuf);

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.GET_CONFIGURATION_current_profile_is_0,
                                           ftr.CurrentProfile);

                switch(ftr.CurrentProfile)
                {
                    case 0x0001:
                        MediaType = MediaType.GENERIC_HDD;

                        break;
                    case 0x0002:
                        MediaType = scsiMediumType switch
                                    {
                                        0x01 => MediaType.PD650,
                                        0x41 => Blocks switch
                                                {
                                                    58620544 => MediaType.REV120,
                                                    17090880 => MediaType.REV35,
                                                    34185728 => MediaType.REV70,
                                                    _        => MediaType
                                                },
                                        _ => MediaType.Unknown
                                    };

                        break;
                    case 0x0005:
                        MediaType = MediaType.CDMO;

                        break;
                    case 0x0008:
                        MediaType = MediaType.CD;

                        break;
                    case 0x0009:
                        MediaType = MediaType.CDR;

                        break;
                    case 0x000A:
                        MediaType = MediaType.CDRW;

                        break;
                    case 0x0010:
                        MediaType = MediaType.DVDROM;

                        break;
                    case 0x0011:
                        MediaType = MediaType.DVDR;

                        break;
                    case 0x0012:
                        MediaType = MediaType.DVDRAM;

                        break;
                    case 0x0013:
                    case 0x0014:
                        MediaType = MediaType.DVDRW;

                        break;
                    case 0x0015:
                    case 0x0016:
                        MediaType = MediaType.DVDRDL;

                        break;
                    case 0x0017:
                        MediaType = MediaType.DVDRWDL;

                        break;
                    case 0x0018:
                        MediaType = MediaType.DVDDownload;

                        break;
                    case 0x001A:
                        MediaType = MediaType.DVDPRW;

                        break;
                    case 0x001B:
                        MediaType = MediaType.DVDPR;

                        break;
                    case 0x0020:
                        MediaType = MediaType.DDCD;

                        break;
                    case 0x0021:
                        MediaType = MediaType.DDCDR;

                        break;
                    case 0x0022:
                        MediaType = MediaType.DDCDRW;

                        break;
                    case 0x002A:
                        MediaType = MediaType.DVDPRWDL;

                        break;
                    case 0x002B:
                        MediaType = MediaType.DVDPRDL;

                        break;
                    case 0x0040:
                        MediaType = MediaType.BDROM;

                        break;
                    case 0x0041:
                    case 0x0042:
                        MediaType = MediaType.BDR;

                        break;
                    case 0x0043:
                        MediaType = MediaType.BDRE;

                        break;
                    case 0x0050:
                        MediaType = MediaType.HDDVDROM;

                        break;
                    case 0x0051:
                        MediaType = MediaType.HDDVDR;

                        break;
                    case 0x0052:
                        MediaType = MediaType.HDDVDRAM;

                        break;
                    case 0x0053:
                        MediaType = MediaType.HDDVDRW;

                        break;
                    case 0x0058:
                        MediaType = MediaType.HDDVDRDL;

                        break;
                    case 0x005A:
                        MediaType = MediaType.HDDVDRWDL;

                        break;
                }
            }

            if(MediaType == MediaType.PD650 && Blocks == 1281856)
                MediaType = MediaType.PD650_WORM;

            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.RecognizedFormatLayers, 0, dev.Timeout, out _);

            if(sense)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Core.READ_DISC_STRUCTURE_Recognized_Format_Layers_0,
                                           Sense.PrettifySense(senseBuf));
            }
            else
                RecognizedFormatLayers = cmdBuf;

            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                          MmcDiscStructureFormat.WriteProtectionStatus, 0, dev.Timeout, out _);

            if(sense)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_Write_Protection_Status_0,
                                           Sense.PrettifySense(senseBuf));
            }
            else
                WriteProtectionStatus = cmdBuf;

            // More like a drive information
            /*
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.CapabilityList, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: Capability List\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_capabilitylist.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            */

        #region All DVD and HD DVD types

            if(MediaType is MediaType.DVDDownload or MediaType.DVDPR or MediaType.DVDPRDL or MediaType.DVDPRW
                         or MediaType.DVDPRWDL or MediaType.DVDR or MediaType.DVDRAM or MediaType.DVDRDL
                         or MediaType.DVDROM or MediaType.DVDRW or MediaType.DVDRWDL or MediaType.HDDVDR
                         or MediaType.HDDVDRAM or MediaType.HDDVDRDL or MediaType.HDDVDROM or MediaType.HDDVDRW
                         or MediaType.HDDVDRWDL)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out _);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_PFI_0,
                                               Sense.PrettifySense(senseBuf));
                }
                else
                {
                    DvdPfi     = cmdBuf;
                    DecodedPfi = PFI.Decode(cmdBuf, MediaType);

                    if(DecodedPfi.HasValue)
                    {
                        if(MediaType == MediaType.DVDROM)
                        {
                            MediaType = DecodedPfi.Value.DiskCategory switch
                                        {
                                            DiskCategory.DVDPR    => MediaType.DVDPR,
                                            DiskCategory.DVDPRDL  => MediaType.DVDPRDL,
                                            DiskCategory.DVDPRW   => MediaType.DVDPRW,
                                            DiskCategory.DVDPRWDL => MediaType.DVDPRWDL,
                                            DiskCategory.DVDR => DecodedPfi.Value.PartVersion >= 6
                                                                     ? MediaType.DVDRDL
                                                                     : MediaType.DVDR,
                                            DiskCategory.DVDRAM => MediaType.DVDRAM,
                                            DiskCategory.DVDRW => DecodedPfi.Value.PartVersion >= 15
                                                                      ? MediaType.DVDRWDL
                                                                      : MediaType.DVDRW,
                                            DiskCategory.HDDVDR   => MediaType.HDDVDR,
                                            DiskCategory.HDDVDRAM => MediaType.HDDVDRAM,
                                            DiskCategory.HDDVDROM => MediaType.HDDVDROM,
                                            DiskCategory.HDDVDRW  => MediaType.HDDVDRW,
                                            DiskCategory.Nintendo => DecodedPfi.Value.DiscSize == DVDSize.Eighty
                                                                         ? MediaType.GOD
                                                                         : MediaType.WOD,
                                            DiskCategory.UMD => MediaType.UMD,
                                            _                => MediaType.DVDROM
                                        };
                        }
                    }
                }

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout,
                                              out _);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_DMI_0,
                                               Sense.PrettifySense(senseBuf));
                }
                else
                {
                    DvdDmi = cmdBuf;

                    if(DMI.IsXbox(cmdBuf))
                        MediaType = MediaType.XGD;
                    else if(DMI.IsXbox360(cmdBuf))
                    {
                        MediaType = MediaType.XGD2;

                        // All XGD3 all have the same number of blocks
                        if(Blocks is 25063 or 4229664 or 4246304) // Wxripper unlock
                            MediaType = MediaType.XGD3;
                    }
                }
            }

        #endregion All DVD and HD DVD types

        #region DVD-ROM

            if(MediaType is MediaType.DVDDownload or MediaType.DVDROM)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.CopyrightInformation, 0, dev.Timeout, out _);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_CMI_0,
                                               Sense.PrettifySense(senseBuf));
                }
                else
                    DvdCmi = cmdBuf;
            }

        #endregion DVD-ROM

            switch(MediaType)
            {
            #region DVD-ROM and HD DVD-ROM

                case MediaType.DVDDownload:
                case MediaType.DVDROM:
                case MediaType.HDDVDROM:
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.BurstCuttingArea, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_BCA_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdBca = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdAacs, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_DVD_AACS_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdAacs = cmdBuf;

                    break;

            #endregion DVD-ROM and HD DVD-ROM

            #region DVD-RAM and HD DVD-RAM

                case MediaType.DVDRAM:
                case MediaType.HDDVDRAM:
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdramDds, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_DDS_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdRamDds = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdramMediumStatus, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_Medium_Status_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdRamCartridgeStatus = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdramSpareAreaInformation, 0, dev.Timeout,
                                                  out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_SAI_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdRamSpareArea = cmdBuf;

                    break;

            #endregion DVD-RAM and HD DVD-RAM

            #region DVD-R and HD DVD-R

                case MediaType.DVDR:
                case MediaType.HDDVDR:
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.LastBorderOutRmd, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Core.READ_DISC_STRUCTURE_Last_Out_Border_RMD_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        LastBorderOutRmd = cmdBuf;

                    break;

            #endregion DVD-R and HD DVD-R
            }

            var dvdDecrypt = new DVDDecryption(dev);
            sense = dvdDecrypt.ReadBusKey(out cmdBuf, out senseBuf, CopyrightType.CSS, dev.Timeout, out _);

            if(!sense)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.DiscKey, dvdDecrypt.Agid, dev.Timeout, out _);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_Disc_Key_0,
                                               Sense.PrettifySense(senseBuf));
                }
                else
                    DvdDiscKey = cmdBuf;

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.SectorCopyrightInformation, dvdDecrypt.Agid,
                                              dev.Timeout, out _);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_Sector_CMI_0,
                                               Sense.PrettifySense(senseBuf));
                }
                else
                    DvdSectorCmi = cmdBuf;
            }

        #region Require drive authentication, won't work

            /*
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.MediaIdentifier, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: Media ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_dvd_mediaid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.MediaKeyBlock, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: MKB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_dvd_mkb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSVolId, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: AACS Volume ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_aacsvolid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMediaSerial, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: AACS Media Serial Number\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_aacssn.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMediaId, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: AACS Media ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_aacsmediaid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMKB, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: AACS MKB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_aacsmkb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSLBAExtents, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: AACS LBA Extents\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_aacslbaextents.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMKBCPRM, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: AACS CPRM MKB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_aacscprmmkb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSDataKeys, 0, dev.Timeout, out _);
            if(sense)
                AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: AACS Data Keys\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
            else
                DataFile.WriteTo(MODULE_NAME, outputPrefix, "_readdiscstructure_aacsdatakeys.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
            */

        #endregion Require drive authentication, won't work

        #region DVD-R and DVD-RW

            if(MediaType is MediaType.DVDR or MediaType.DVDRW or MediaType.DVDRDL or MediaType.DVDRWDL)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.PreRecordedInfo, 0, dev.Timeout, out _);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_Pre_Recorded_Info_0,
                                               Sense.PrettifySense(senseBuf));
                }
                else
                {
                    DvdPreRecordedInfo = cmdBuf;

                    DecodedDvdPrePitInformation = PRI.Decode(cmdBuf);
                }
            }

        #endregion DVD-R and DVD-RW

            switch(MediaType)
            {
            #region DVD-R, DVD-RW and HD DVD-R

                case MediaType.DVDR:
                case MediaType.DVDRW:
                case MediaType.DVDRDL:
                case MediaType.DVDRWDL:
                case MediaType.HDDVDR:
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdrMediaIdentifier, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_DVD_R_Media_ID_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdrMediaIdentifier = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdrPhysicalInformation, 0, dev.Timeout,
                                                  out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_DVD_R_PFI_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                    {
                        DvdrPhysicalInformation = cmdBuf;

                        DecodedDvdrPfi = PFI.Decode(cmdBuf, MediaType);
                    }

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
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_ADIP_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdPlusAdip = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.Dcb, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_DCB_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdPlusDcb = cmdBuf;

                    break;

            #endregion All DVD+

            #region HD DVD-ROM

                case MediaType.HDDVDROM:
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.HddvdCopyrightInformation, 0, dev.Timeout,
                                                  out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_HD_DVD_CMI_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        HddvdCopyrightInformation = cmdBuf;

                    break;

            #endregion HD DVD-ROM
            }

            switch(MediaType)
            {
            #region HD DVD-R

                case MediaType.HDDVDR:
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.HddvdrMediumStatus, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Core.READ_DISC_STRUCTURE_HD_DVD_R_Medium_Status,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        HddvdrMediumStatus = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.HddvdrLastRmd, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_Last_RMD_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        HddvdrLastRmd = cmdBuf;

                    break;
                }

            #endregion HD DVD-R

            #region DVD-R DL, DVD-RW DL, DVD+R DL, DVD+RW DL

                case MediaType.DVDPRDL or MediaType.DVDRDL or MediaType.DVDRWDL or MediaType.DVDPRWDL:
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdrLayerCapacity, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_Layer_Capacity_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdrLayerCapacity = cmdBuf;

                    break;
                }

            #endregion DVD-R DL, DVD-RW DL, DVD+R DL, DVD+RW DL
            }

            switch(MediaType)
            {
            #region DVD-R DL

                case MediaType.DVDRDL:
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.MiddleZoneStart, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Core.READ_DISC_STRUCTURE_Middle_Zone_Start_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdrDlMiddleZoneStart = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.JumpIntervalSize, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Core.READ_DISC_STRUCTURE_Jump_Interval_Size_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdrDlJumpIntervalSize = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.ManualLayerJumpStartLba, 0, dev.Timeout,
                                                  out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Core.READ_DISC_STRUCTURE_Manual_Layer_Jump_Start_LBA_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdrDlManualLayerJumpStartLba = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.RemapAnchorPoint, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Core.READ_DISC_STRUCTURE_Remap_Anchor_Point_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        DvdrDlRemapAnchorPoint = cmdBuf;

                    break;

            #endregion DVD-R DL

            #region All Blu-ray

                case MediaType.BDR:
                case MediaType.BDRE:
                case MediaType.BDROM:
                case MediaType.UHDBD:
                case MediaType.BDRXL:
                case MediaType.BDREXL:
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.DiscInformation, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_DI_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        BlurayDiscInformation = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.Pac, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_PAC_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        BlurayPac = cmdBuf;

                    break;

            #endregion All Blu-ray
            }

            switch(MediaType)
            {
            #region BD-ROM only

                case MediaType.BDROM:
                case MediaType.UHDBD:
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.BdBurstCuttingArea, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_BCA_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        BlurayBurstCuttingArea = cmdBuf;

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
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_DDS_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        BlurayDds = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.CartridgeStatus, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Core.READ_DISC_STRUCTURE_Cartridge_Status_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        BlurayCartridgeStatus = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.BdSpareAreaInformation, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, "READ DISC STRUCTURE: Spare Area Information\n{0}",
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        BluraySpareAreaInformation = cmdBuf;

                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.RawDfl, 0, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_Raw_DFL_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        BlurayRawDfl = cmdBuf;

                    sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf,
                                                    MmcDiscInformationDataTypes.TrackResources, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_001b_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        BlurayTrackResources = cmdBuf;

                    sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf, MmcDiscInformationDataTypes.PowResources,
                                                    dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_010b_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                        BlurayPowResources = cmdBuf;

                    break;

            #endregion Writable Blu-ray only

            #region CDs

                case MediaType.CD:
                case MediaType.CDR:
                case MediaType.CDROM:
                case MediaType.CDRW:
                case MediaType.Unknown:
                    // We discarded all discs that falsify a TOC before requesting a real TOC
                    // No TOC, no CD (or an empty one)
                    bool tocSense = dev.ReadTocPmaAtip(out cmdBuf, out senseBuf, false, 0, 0, dev.Timeout, out _);

                    if(tocSense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_TOC_PMA_ATIP_TOC_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                    {
                        Toc        = cmdBuf;
                        DecodedToc = TOC.Decode(cmdBuf);

                        // As we have a TOC we know it is a CD
                        if(MediaType == MediaType.Unknown)
                            MediaType = MediaType.CD;
                    }

                    // ATIP exists on blank CDs
                    sense = dev.ReadAtip(out cmdBuf, out senseBuf, dev.Timeout, out _);

                    if(sense)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_TOC_PMA_ATIP_ATIP_0,
                                                   Sense.PrettifySense(senseBuf));
                    }
                    else
                    {
                        Atip        = cmdBuf;
                        DecodedAtip = ATIP.Decode(cmdBuf);

                        if(DecodedAtip != null)

                            // Only CD-R and CD-RW have ATIP
                            MediaType = DecodedAtip.DiscType ? MediaType.CDRW : MediaType.CDR;
                        else
                            Atip = null;
                    }

                    // We got a TOC, get information about a recorded/mastered CD
                    if(!tocSense)
                    {
                        sense = dev.ReadSessionInfo(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_TOC_PMA_ATIP_Session_Info_0,
                                                       Sense.PrettifySense(senseBuf));
                        }
                        else if(cmdBuf.Length > 4)
                        {
                            Session        = cmdBuf;
                            DecodedSession = Decoders.CD.Session.Decode(cmdBuf);

                            if(DecodedSession.HasValue)
                            {
                                sessions              = DecodedSession.Value.LastCompleteSession;
                                firstTrackLastSession = DecodedSession.Value.TrackDescriptors[0].TrackNumber;
                            }
                        }

                        sense = dev.ReadRawToc(out cmdBuf, out senseBuf, 1, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_TOC_PMA_ATIP_Raw_TOC_0,
                                                       Sense.PrettifySense(senseBuf));
                        }
                        else if(cmdBuf.Length > 4)
                        {
                            RawToc = cmdBuf;

                            FullToc = FullTOC.Decode(cmdBuf);
                        }

                        sense = dev.ReadPma(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_TOC_PMA_ATIP_PMA_0,
                                                       Sense.PrettifySense(senseBuf));
                        }
                        else if(cmdBuf.Length > 4)
                            Pma = cmdBuf;

                        sense = dev.ReadCdText(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_TOC_PMA_ATIP_CD_TEXT_0,
                                                       Sense.PrettifySense(senseBuf));
                        }
                        else if(cmdBuf.Length > 4)
                        {
                            CdTextLeadIn        = cmdBuf;
                            DecodedCdTextLeadIn = CDTextOnLeadIn.Decode(cmdBuf);
                        }

                        sense = dev.ReadMcn(out string mcn, out _, out _, dev.Timeout, out _);

                        if(!sense && mcn != null && mcn != "0000000000000")
                            Mcn = mcn;

                        Isrcs = new Dictionary<byte, string>();

                        for(byte i = DecodedToc.Value.FirstTrack; i <= DecodedToc.Value.LastTrack; i++)
                        {
                            sense = dev.ReadIsrc(i, out string isrc, out _, out _, dev.Timeout, out _);

                            if(!sense && isrc != null && isrc != "000000000000")
                                Isrcs.Add(i, isrc);
                        }

                        if(Isrcs.Count == 0)
                            Isrcs = null;
                    }

                    break;

            #endregion CDs
            }

        #region Nintendo

            if(MediaType == MediaType.Unknown && Blocks > 0)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out _);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_PFI_0,
                                               Sense.PrettifySense(senseBuf));
                }
                else
                {
                    DvdPfi = cmdBuf;
                    PFI.PhysicalFormatInformation? nintendoPfi = PFI.Decode(cmdBuf, MediaType);

                    if(nintendoPfi != null)
                    {
                        AaruConsole.WriteLine("PFI:\n{0}", PFI.Prettify(cmdBuf, MediaType));

                        if(nintendoPfi is { DiskCategory: DiskCategory.Nintendo, PartVersion: 15 })
                        {
                            MediaType = nintendoPfi.Value.DiscSize switch
                                        {
                                            DVDSize.Eighty    => MediaType.GOD,
                                            DVDSize.OneTwenty => MediaType.WOD,
                                            _                 => MediaType
                                        };
                        }
                    }
                }

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout,
                                              out _);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_STRUCTURE_DMI_0,
                                               Sense.PrettifySense(senseBuf));
                }
                else
                    DvdDmi = cmdBuf;
            }

        #endregion Nintendo
        }

        sense = dev.ReadMediaSerialNumber(out cmdBuf, out senseBuf, dev.Timeout, out _);

        if(sense)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_MEDIA_SERIAL_NUMBER_0,
                                       Sense.PrettifySense(senseBuf));
        }
        else
        {
            if(cmdBuf.Length >= 4)
                MediaSerialNumber = cmdBuf;
        }

        switch(MediaType)
        {
        #region Xbox

            case MediaType.XGD:
            case MediaType.XGD2:
            case MediaType.XGD3:
                // We need to get INQUIRY to know if it is a Kreon drive
                sense = dev.ScsiInquiry(out byte[] inqBuffer, out senseBuf);

                if(!sense)
                {
                    Inquiry? inq = Inquiry.Decode(inqBuffer);

                    if(inq?.KreonPresent == true)
                    {
                        sense = dev.KreonExtractSs(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.KREON_EXTRACT_SS_0,
                                                       Sense.PrettifySense(senseBuf));
                        }
                        else
                            XboxSecuritySector = cmdBuf;

                        DecodedXboxSecuritySector = SS.Decode(cmdBuf);

                        // Get video partition size
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Getting_video_partition_size);

                        sense = dev.KreonLock(out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.ErrorWriteLine(Localization.Core.Cannot_lock_drive_not_continuing);

                            break;
                        }

                        sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.ErrorWriteLine(Localization.Core.Cannot_get_disc_capacity);

                            break;
                        }

                        ulong totalSize =
                            (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]) & 0xFFFFFFFF;

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.PhysicalInformation, 0, 0, out _);

                        if(sense)
                        {
                            AaruConsole.ErrorWriteLine(Localization.Core.Cannot_get_PFI);

                            break;
                        }

                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Video_partition_total_size_0_sectors,
                                                   totalSize);

                        ulong l0Video = PFI.Decode(cmdBuf, MediaType).Value.Layer0EndPSN -
                                        PFI.Decode(cmdBuf, MediaType).Value.DataAreaStartPSN +
                                        1;

                        ulong l1Video = totalSize - l0Video + 1;

                        // Get game partition size
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Getting_game_partition_size);
                        sense = dev.KreonUnlockXtreme(out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.ErrorWriteLine(Localization.Core.Cannot_unlock_drive_not_continuing);

                            break;
                        }

                        sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.ErrorWriteLine(Localization.Core.Cannot_get_disc_capacity);

                            return;
                        }

                        ulong gameSize =
                            ((ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]) &
                             0xFFFFFFFF) +
                            1;

                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Game_partition_total_size_0_sectors,
                                                   gameSize);

                        // Get middle zone size
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Getting_middle_zone_size);
                        sense = dev.KreonUnlockWxripper(out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.ErrorWriteLine(Localization.Core.Cannot_unlock_drive_not_continuing);

                            break;
                        }

                        sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.ErrorWriteLine(Localization.Core.Cannot_get_disc_capacity);

                            break;
                        }

                        totalSize = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]) &
                                    0xFFFFFFFF;

                        sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                      MmcDiscStructureFormat.PhysicalInformation, 0, 0, out _);

                        if(sense)
                        {
                            AaruConsole.ErrorWriteLine(Localization.Core.Cannot_get_PFI);

                            break;
                        }

                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Unlocked_total_size_0_sectors,
                                                   totalSize);

                        ulong middleZone = totalSize -
                                           (PFI.Decode(cmdBuf, MediaType).Value.Layer0EndPSN -
                                            PFI.Decode(cmdBuf, MediaType).Value.DataAreaStartPSN +
                                            1) -
                                           gameSize +
                                           1;

                        totalSize = l0Video + l1Video + middleZone * 2 + gameSize;
                        ulong layerBreak = l0Video + middleZone + gameSize / 2;

                        XgdInfo = new XgdInfo
                        {
                            L0Video    = l0Video,
                            L1Video    = l1Video,
                            MiddleZone = middleZone,
                            GameSize   = gameSize,
                            TotalSize  = totalSize,
                            LayerBreak = layerBreak
                        };
                    }
                }

                break;

        #endregion Xbox

            case MediaType.Unknown:
                MediaType = MediaTypeFromDevice.GetFromScsi((byte)dev.ScsiType, dev.Manufacturer, dev.Model,
                                                            scsiMediumType, scsiDensityCode, Blocks, BlockSize,
                                                            dev.IsUsb, true);

                break;
        }

        if(MediaType == MediaType.Unknown && dev.IsUsb && containsFloppyPage)
            MediaType = MediaType.FlashDrive;

        if(MediaType == MediaType.Unknown && !dev.IsRemovable)
            MediaType = MediaType.GENERIC_HDD;

        if(DeviceInfo.ScsiType != PeripheralDeviceTypes.MultiMediaDevice ||
           dev.IsUsb && scsiMediumType is 0x40 or 0x41 or 0x42)
            return;

        sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf, MmcDiscInformationDataTypes.DiscInformation,
                                        dev.Timeout, out _);

        if(sense)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.READ_DISC_INFORMATION_000b_0,
                                       Sense.PrettifySense(senseBuf));
        }
        else
        {
            DiscInformation        = cmdBuf;
            DecodedDiscInformation = Decoders.SCSI.MMC.DiscInformation.Decode000b(cmdBuf);

            if(DecodedDiscInformation.HasValue)
            {
                if(MediaType == MediaType.CD)
                {
                    MediaType = DecodedDiscInformation.Value.DiscType switch
                                {
                                    0x10 => MediaType.CDI,
                                    0x20 => MediaType.CDROMXA,
                                    _    => MediaType
                                };
                }
            }
        }

        MediaType tmpType = MediaType;
        MMC.DetectDiscType(ref tmpType, sessions, FullToc, dev, out _, out _, firstTrackLastSession, Blocks);

        MediaType = tmpType;
    }

    /// <summary>Decoded DVD Pre-Recorded Information</summary>
    public PRI.PreRecordedInformation? DecodedDvdPrePitInformation { get; }

    /// <summary>Decoded recordable DVD Physical Format Information</summary>
    public PFI.PhysicalFormatInformation? DecodedDvdrPfi { get; }

    /// <summary>Raw media serial number</summary>
    public byte[] MediaSerialNumber { get; }

    /// <summary>Raw Xbox security sectors</summary>
    public byte[] XboxSecuritySector { get; }

    /// <summary>Decoded Xbox security sectors</summary>
    public SS.SecuritySector? DecodedXboxSecuritySector { get; }

    /// <summary>Information about an XGD, XGD2 or XGD3 media</summary>
    public XgdInfo XgdInfo { get; }

    /// <summary>MMC drive raw GET CONFIGURATION</summary>
    public byte[] MmcConfiguration { get; }

    /// <summary>Raw recognized format layers</summary>
    public byte[] RecognizedFormatLayers { get; }

    /// <summary>Raw write protection status</summary>
    public byte[] WriteProtectionStatus { get; }

    /// <summary>Raw DVD Physical Format Information</summary>
    public byte[] DvdPfi { get; }

    /// <summary>Decoded DVD Physical Format Information</summary>
    public PFI.PhysicalFormatInformation? DecodedPfi { get; }

    /// <summary>Raw DVD Disc Manufacturing Information</summary>
    public byte[] DvdDmi { get; }

    /// <summary>Raw DVD Copyright Management Information</summary>
    public byte[] DvdCmi { get; }

    /// <summary>Raw DVD Burst Cutting Area</summary>
    public byte[] DvdBca { get; }

    /// <summary>Raw DVD AACS information</summary>
    public byte[] DvdAacs { get; }

    /// <summary>Raw DVD-RAM Disc Definition Structure</summary>
    public byte[] DvdRamDds { get; }

    /// <summary>Raw DVD-RAM Cartridge Status</summary>
    public byte[] DvdRamCartridgeStatus { get; }

    /// <summary>Raw DVD-RAM Spare Area Information</summary>
    public byte[] DvdRamSpareArea { get; }

    /// <summary>Raw DVD-R(W) Last Border-Out RMD</summary>
    public byte[] LastBorderOutRmd { get; }

    /// <summary>Raw DVD-R(W) Pre-Recorded Information</summary>
    public byte[] DvdPreRecordedInfo { get; }

    /// <summary>Raw DVD-R Media ID</summary>
    public byte[] DvdrMediaIdentifier { get; }

    /// <summary>Raw recordable DVD Physical Format Information</summary>
    public byte[] DvdrPhysicalInformation { get; }

    /// <summary>Raw DVD+R(W) ADIP</summary>
    public byte[] DvdPlusAdip { get; }

    /// <summary>Raw DVD+R(W) Disc Control Blocks</summary>
    public byte[] DvdPlusDcb { get; }

    /// <summary>Raw HD DVD Copyright Management Information</summary>
    public byte[] HddvdCopyrightInformation { get; }

    /// <summary>Raw HD DVD-R Medium Status</summary>
    public byte[] HddvdrMediumStatus { get; }

    /// <summary>Raw HD DVD-R(W) Last Border-Out RMD</summary>
    public byte[] HddvdrLastRmd { get; }

    /// <summary>Raw DVD-R(W) Layer Capacity</summary>
    public byte[] DvdrLayerCapacity { get; }

    /// <summary>Raw DVD-R DL Middle Zone start</summary>
    public byte[] DvdrDlMiddleZoneStart { get; }

    /// <summary>Raw DVD-R DL Jump Interval size</summary>
    public byte[] DvdrDlJumpIntervalSize { get; }

    /// <summary>Raw DVD-R DL Manual Layer Jump Start LBA</summary>
    public byte[] DvdrDlManualLayerJumpStartLba { get; }

    /// <summary>Raw DVD-R DL Remap Anchor Point</summary>
    public byte[] DvdrDlRemapAnchorPoint { get; }

    /// <summary>Raw Blu-ray Disc Information</summary>
    public byte[] BlurayDiscInformation { get; }

    /// <summary>Raw Blu-ray PAC</summary>
    public byte[] BlurayPac { get; }

    /// <summary>Raw Blu-ray Burst Cutting Area</summary>
    public byte[] BlurayBurstCuttingArea { get; }

    /// <summary>Raw Blu-ray Disc Definition Structure</summary>
    public byte[] BlurayDds { get; }

    /// <summary>Raw Blu-ray Cartridge Status</summary>
    public byte[] BlurayCartridgeStatus { get; }

    /// <summary>Raw Blu-ray Spare Area Information</summary>
    public byte[] BluraySpareAreaInformation { get; }

    /// <summary>Raw Blu-ray DFL</summary>
    public byte[] BlurayRawDfl { get; }

    /// <summary>Raw Blu-ray Pseudo OverWrite Resources</summary>
    public byte[] BlurayPowResources { get; }

    /// <summary>Raw READ TOC response</summary>
    public byte[] Toc { get; }

    /// <summary>Raw READ ATIP response</summary>
    public byte[] Atip { get; }

    /// <summary>Raw READ DISC INFORMATION response</summary>
    public byte[] DiscInformation { get; }

    /// <summary>Raw READ SESSION response</summary>
    public byte[] Session { get; }

    /// <summary>Raw READ FULL TOC response</summary>
    public byte[] RawToc { get; }

    /// <summary>Raw READ PMA response</summary>
    public byte[] Pma { get; }

    /// <summary>Raw Lead-In's CD-TEXT response</summary>
    public byte[] CdTextLeadIn { get; }

    /// <summary>Decoded READ TOC response</summary>
    public TOC.CDTOC? DecodedToc { get; }

    /// <summary>Decoded READ ATIP response</summary>
    public ATIP.CDATIP DecodedAtip { get; }

    /// <summary>Decoded READ SESSION response</summary>
    public Session.CDSessionInfo? DecodedSession { get; }

    /// <summary>Decoded READ FULL TOC response</summary>
    public FullTOC.CDFullTOC? FullToc { get; }

    /// <summary>Decoded Lead-In CD-TEXT response</summary>
    public CDTextOnLeadIn.CDText? DecodedCdTextLeadIn { get; }

    /// <summary>Raw Blu-ray track resources</summary>
    public byte[] BlurayTrackResources { get; }

    /// <summary>Decoded Blu-ray Disc Information</summary>
    public DiscInformation.StandardDiscInformation? DecodedDiscInformation { get; }

    /// <summary>Decoded Media Catalogue Number</summary>
    public string Mcn { get; }

    /// <summary>List of decoded track ISRCs</summary>
    public Dictionary<byte, string> Isrcs { get; }

    /// <summary>Set if media is inserted in drive</summary>
    public bool MediaInserted { get; }

    /// <summary>Detected media type</summary>
    public MediaType MediaType { get; }

    /// <summary>Device information</summary>
    public DeviceInfo DeviceInfo { get; }

    /// <summary>Raw READ CAPACITY(10) response</summary>
    public byte[] ReadCapacity { get; }

    /// <summary>Number of blocks in media</summary>
    public ulong Blocks { get; }

    /// <summary>Logical block size</summary>
    public uint BlockSize { get; }

    /// <summary>Raw READ CAPACITY(16) response</summary>
    public byte[] ReadCapacity16 { get; }

    /// <summary>Raw SSC Density support</summary>
    public byte[] DensitySupport { get; }

    /// <summary>Decoded SSC Density support</summary>
    public DensitySupport.DensitySupportHeader? DensitySupportHeader { get; }

    /// <summary>Raw SSC media support</summary>
    public byte[] MediaTypeSupport { get; }

    /// <summary>Decoded SSC media support</summary>
    public DensitySupport.MediaTypeSupportHeader? MediaTypeSupportHeader { get; }

    /// <summary>Raw data from DVD sector Copyright Management Information</summary>
    public byte[] DvdSectorCmi { get; }

    /// <summary>Raw DVD Disc Key</summary>
    public byte[] DvdDiscKey { get; }
}