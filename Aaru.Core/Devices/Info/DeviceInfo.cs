// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core.
//
// --[ Description ] ----------------------------------------------------------
//
//     Retrieves the device info.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.ATA;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decryption;
using Aaru.Devices;
using Aaru.Helpers;
using DVDDecryption = Aaru.Decryption.DVD.Dump;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;

namespace Aaru.Core.Devices.Info;

/// <summary>Obtains and contains information about a device</summary>
public partial class DeviceInfo
{
    const string MODULE_NAME = "Device information";

    /// <summary>Initializes an instance of this class for the specified device</summary>
    /// <param name="dev">Device</param>
    public DeviceInfo(Device dev)
    {
        Type                  = dev.Type;
        Manufacturer          = dev.Manufacturer;
        Model                 = dev.Model;
        FirmwareRevision      = dev.FirmwareRevision;
        Serial                = dev.Serial;
        ScsiType              = dev.ScsiType;
        IsRemovable           = dev.IsRemovable;
        IsUsb                 = dev.IsUsb;
        UsbVendorId           = dev.UsbVendorId;
        UsbProductId          = dev.UsbProductId;
        UsbDescriptors        = dev.UsbDescriptors;
        UsbManufacturerString = dev.UsbManufacturerString;
        UsbProductString      = dev.UsbProductString;
        UsbSerialString       = dev.UsbSerialString;
        IsFireWire            = dev.IsFireWire;
        FireWireGuid          = dev.FireWireGuid;
        FireWireModel         = dev.FireWireModel;
        FireWireModelName     = dev.FireWireModelName;
        FireWireVendor        = dev.FireWireVendor;
        FireWireVendorName    = dev.FireWireVendorName;
        IsCompactFlash        = dev.IsCompactFlash;
        IsPcmcia              = dev.IsPcmcia;
        Cis                   = dev.Cis;

        switch(dev.Type)
        {
            case DeviceType.ATA:
            {
                bool sense = dev.AtaIdentify(out byte[] ataBuf, out AtaErrorRegistersChs errorRegisters);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, "STATUS = 0x{0:X2}", errorRegisters.Status);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ERROR = 0x{0:X2}",  errorRegisters.Error);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "NSECTOR = 0x{0:X2}", errorRegisters.SectorCount);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "SECTOR = 0x{0:X2}", errorRegisters.Sector);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "CYLHIGH = 0x{0:X2}", errorRegisters.CylinderHigh);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "CYLLOW = 0x{0:X2}", errorRegisters.CylinderLow);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "DEVICE = 0x{0:X2}", errorRegisters.DeviceHead);

                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Error_code_equals_0, dev.LastError);

                    break;
                }

                if(dev.Error)
                {
                    AaruConsole.ErrorWriteLine(Localization.Core.Error_0_querying_ATA_IDENTIFY, dev.LastError);

                    break;
                }

                AtaIdentify = ataBuf;

                dev.EnableMediaCardPassThrough(out errorRegisters, dev.Timeout, out _);

                if(errorRegisters is { Sector: 0xAA, SectorCount: 0x55 }) AtaMcptError = errorRegisters;

                break;
            }

            case DeviceType.ATAPI:
            {
                bool sense = dev.AtapiIdentify(out byte[] ataBuf, out AtaErrorRegistersChs errorRegisters);

                if(sense)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, "STATUS = 0x{0:X2}", errorRegisters.Status);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "ERROR = 0x{0:X2}",  errorRegisters.Error);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "NSECTOR = 0x{0:X2}", errorRegisters.SectorCount);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "SECTOR = 0x{0:X2}", errorRegisters.Sector);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "CYLHIGH = 0x{0:X2}", errorRegisters.CylinderHigh);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "CYLLOW = 0x{0:X2}", errorRegisters.CylinderLow);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "DEVICE = 0x{0:X2}", errorRegisters.DeviceHead);

                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Error_code_equals_0, dev.LastError);

                    break;
                }

                if(!dev.Error)
                    AtapiIdentify = ataBuf;
                else
                    AaruConsole.ErrorWriteLine(Localization.Core.Error_0_querying_ATA_PACKET_IDENTIFY, dev.LastError);

                // ATAPI devices are also SCSI devices
                goto case DeviceType.SCSI;
            }

            case DeviceType.SCSI:
            {
                bool sense = dev.ScsiInquiry(out byte[] inqBuf, out byte[] senseBuf);

                if(sense)
                {
                    AaruConsole.ErrorWriteLine(Localization.Core.SCSI_error_0, Sense.PrettifySense(senseBuf));

                    break;
                }

                ScsiInquiryData = inqBuf;
                ScsiInquiry     = Inquiry.Decode(inqBuf);

                sense = dev.ScsiInquiry(out inqBuf, out senseBuf, 0x00);

                if(!sense)
                {
                    ScsiEvpdPages = new Dictionary<byte, byte[]>();

                    byte[] pages = EVPD.DecodePage00(inqBuf);

                    if(pages != null)
                    {
                        foreach(byte page in pages)
                        {
                            sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);

                            if(sense) continue;

                            ScsiEvpdPages.Add(page, inqBuf);
                        }
                    }
                }

                var devType = (PeripheralDeviceTypes)ScsiInquiry.Value.PeripheralDeviceType;

                sense = dev.ModeSense10(out byte[] modeBuf,
                                        out senseBuf,
                                        false,
                                        true,
                                        ScsiModeSensePageControl.Current,
                                        0x3F,
                                        0xFF,
                                        5,
                                        out _);

                if(!sense && !dev.Error) ScsiModeSense10 = modeBuf;

                if(sense || dev.Error)
                {
                    sense = dev.ModeSense10(out modeBuf,
                                            out senseBuf,
                                            false,
                                            true,
                                            ScsiModeSensePageControl.Current,
                                            0x3F,
                                            0x00,
                                            5,
                                            out _);

                    if(!sense && !dev.Error) ScsiModeSense10 = modeBuf;
                }

                if(!sense && !dev.Error) ScsiMode = Modes.DecodeMode10(modeBuf, devType);

                bool useMode10 = !(sense || dev.Error || !ScsiMode.HasValue);

                sense = dev.ModeSense6(out modeBuf,
                                       out senseBuf,
                                       false,
                                       ScsiModeSensePageControl.Current,
                                       0x3F,
                                       0xFF,
                                       5,
                                       out _);

                if(!sense && !dev.Error) ScsiModeSense6 = modeBuf;

                if(sense || dev.Error)
                {
                    sense = dev.ModeSense6(out modeBuf,
                                           out senseBuf,
                                           false,
                                           ScsiModeSensePageControl.Current,
                                           0x3F,
                                           0x00,
                                           5,
                                           out _);

                    if(!sense && !dev.Error) ScsiModeSense6 = modeBuf;
                }

                if(sense || dev.Error)
                {
                    sense = dev.ModeSense(out modeBuf, out senseBuf, 5, out _);

                    if(!sense && !dev.Error) ScsiModeSense6 = modeBuf;
                }

                if(!sense && !dev.Error && !useMode10) ScsiMode = Modes.DecodeMode6(modeBuf, devType);

                switch(devType)
                {
                    case PeripheralDeviceTypes.MultiMediaDevice:
                    {
                        sense = dev.GetConfiguration(out byte[] confBuf, out senseBuf, dev.Timeout, out _);

                        if(!sense) MmcConfiguration = confBuf;

                        var dvdDecrypt = new DVDDecryption(dev);

                        sense = dvdDecrypt.ReadRpc(out byte[] cmdBuf,
                                                   out _,
                                                   DvdCssKeyClass.DvdCssCppmOrCprm,
                                                   dev.Timeout,
                                                   out _);

                        if(!sense)
                        {
                            CSS_CPRM.RegionalPlaybackControlState? rpc =
                                CSS_CPRM.DecodeRegionalPlaybackControlState(cmdBuf);

                            if(rpc.HasValue) RPC = rpc;
                        }

                        // TODO: DVD drives respond correctly to BD status.
                        // While specification says if no medium is present
                        // it should inform all possible capabilities,
                        // testing drives show only supported media capabilities.
                        /*
                        byte[] strBuf;
                        sense = dev.ReadDiscStructure(out strBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.CapabilityList, 0, dev.Timeout, out _);

                        if (!sense)
                        {
                            Decoders.SCSI.DiscStructureCapabilities.Capability[] caps = Decoders.SCSI.DiscStructureCapabilities.Decode(strBuf);
                            if (caps != null)
                            {
                                foreach (Decoders.SCSI.DiscStructureCapabilities.Capability cap in caps)
                                {
                                    if (cap.SDS && cap.RDS)
                                        AaruConsole.WriteLine("Drive can READ/SEND DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                    else if (cap.SDS)
                                        AaruConsole.WriteLine("Drive can SEND DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                    else if (cap.RDS)
                                        AaruConsole.WriteLine("Drive can READ DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                }
                            }
                        }

                        sense = dev.ReadDiscStructure(out strBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.CapabilityList, 0, dev.Timeout, out _);

                        if (!sense)
                        {
                            Decoders.SCSI.DiscStructureCapabilities.Capability[] caps = Decoders.SCSI.DiscStructureCapabilities.Decode(strBuf);
                            if (caps != null)
                            {
                                foreach (Decoders.SCSI.DiscStructureCapabilities.Capability cap in caps)
                                {
                                    if (cap.SDS && cap.RDS)
                                        AaruConsole.WriteLine("Drive can READ/SEND DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                    else if (cap.SDS)
                                        AaruConsole.WriteLine("Drive can SEND DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                    else if (cap.RDS)
                                        AaruConsole.WriteLine("Drive can READ DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                }
                            }
                        }
                        */

#region Plextor

                        if(dev.Manufacturer == "PLEXTOR")
                        {
                            var    plxtSense = true;
                            var    plxtDvd   = false;
                            byte[] plxtBuf   = null;

                            switch(dev.Model)
                            {
                                case "DVDR   PX-708A":
                                case "DVDR   PX-708A2":
                                case "DVDR   PX-712A":
                                    plxtDvd = true;

                                    plxtSense = dev.PlextorReadEeprom(out plxtBuf, out senseBuf, dev.Timeout, out _);

                                    break;
                                case "DVDR   PX-714A":
                                case "DVDR   PX-716A":
                                case "DVDR   PX-716AL":
                                case "DVDR   PX-755A":
                                case "DVDR   PX-760A":
                                {
                                    plxtBuf = new byte[256 * 4];

                                    for(byte i = 0; i < 4; i++)
                                    {
                                        plxtSense = dev.PlextorReadEepromBlock(out byte[] plxtBufSmall,
                                                                               out senseBuf,
                                                                               i,
                                                                               256,
                                                                               dev.Timeout,
                                                                               out _);

                                        if(plxtSense) break;

                                        Array.Copy(plxtBufSmall, 0, plxtBuf, i * 256, 256);
                                    }

                                    plxtDvd = true;

                                    break;
                                }

                                default:
                                {
                                    if(dev.Model.StartsWith("CD-R   ", StringComparison.Ordinal))
                                    {
                                        plxtSense = dev.PlextorReadEepromCdr(out plxtBuf,
                                                                             out senseBuf,
                                                                             dev.Timeout,
                                                                             out _);
                                    }

                                    break;
                                }
                            }

                            PlextorFeatures = new Plextor
                            {
                                IsDvd = plxtDvd
                            };

                            if(!plxtSense)
                            {
                                PlextorFeatures.Eeprom = plxtBuf;

                                if(plxtDvd)
                                {
                                    PlextorFeatures.Discs        = BigEndianBitConverter.ToUInt16(plxtBuf, 0x0120);
                                    PlextorFeatures.CdReadTime   = BigEndianBitConverter.ToUInt32(plxtBuf, 0x0122);
                                    PlextorFeatures.CdWriteTime  = BigEndianBitConverter.ToUInt32(plxtBuf, 0x0126);
                                    PlextorFeatures.DvdReadTime  = BigEndianBitConverter.ToUInt32(plxtBuf, 0x012A);
                                    PlextorFeatures.DvdWriteTime = BigEndianBitConverter.ToUInt32(plxtBuf, 0x012E);
                                }
                                else
                                {
                                    PlextorFeatures.Discs       = BigEndianBitConverter.ToUInt16(plxtBuf, 0x0078);
                                    PlextorFeatures.CdReadTime  = BigEndianBitConverter.ToUInt32(plxtBuf, 0x006C);
                                    PlextorFeatures.CdWriteTime = BigEndianBitConverter.ToUInt32(plxtBuf, 0x007A);
                                }
                            }

                            plxtSense = dev.PlextorGetPoweRec(out senseBuf,
                                                              out bool plxtPwrRecEnabled,
                                                              out ushort plxtPwrRecSpeed,
                                                              dev.Timeout,
                                                              out _);

                            if(!plxtSense)
                            {
                                PlextorFeatures.PoweRec = true;

                                if(plxtPwrRecEnabled)
                                {
                                    PlextorFeatures.PoweRecEnabled          = true;
                                    PlextorFeatures.PoweRecRecommendedSpeed = plxtPwrRecSpeed;

                                    plxtSense = dev.PlextorGetSpeeds(out senseBuf,
                                                                     out ushort plxtPwrRecSelected,
                                                                     out ushort plxtPwrRecMax,
                                                                     out ushort plxtPwrRecLast,
                                                                     dev.Timeout,
                                                                     out _);

                                    if(!plxtSense)
                                    {
                                        PlextorFeatures.PoweRecSelected = plxtPwrRecSelected;
                                        PlextorFeatures.PoweRecMax      = plxtPwrRecMax;
                                        PlextorFeatures.PoweRecLast     = plxtPwrRecLast;
                                    }
                                }
                            }

                            // TODO: Check it with a drive
                            plxtSense = dev.PlextorGetSilentMode(out plxtBuf, out senseBuf, dev.Timeout, out _);

                            if(!plxtSense)
                            {
                                if(plxtBuf[0] == 1)
                                {
                                    PlextorFeatures.SilentModeEnabled = true;
                                    PlextorFeatures.AccessTimeLimit   = plxtBuf[1];

                                    PlextorFeatures.CdReadSpeedLimit  = plxtBuf[2];
                                    PlextorFeatures.DvdReadSpeedLimit = plxtBuf[3];
                                    PlextorFeatures.CdWriteSpeedLimit = plxtBuf[4];

                                    // TODO: Check which one is each one
                                    /*
                                        if(plxtBuf[6] > 0)
                                            AaruConsole.WriteLine("\tTray eject speed limited to {0}",
                                                                 -(plxtBuf[6] + 48));
                                        if(plxtBuf[7] > 0)
                                            AaruConsole.WriteLine("\tTray eject speed limited to {0}",
                                                                 plxtBuf[7] - 47);
                                    */
                                }
                            }

                            plxtSense = dev.PlextorGetGigaRec(out plxtBuf, out senseBuf, dev.Timeout, out _);

                            if(!plxtSense) PlextorFeatures.GigaRec = true;

                            plxtSense = dev.PlextorGetSecuRec(out plxtBuf, out senseBuf, dev.Timeout, out _);

                            if(!plxtSense) PlextorFeatures.SecuRec = true;

                            plxtSense = dev.PlextorGetSpeedRead(out plxtBuf, out senseBuf, dev.Timeout, out _);

                            if(!plxtSense)
                            {
                                PlextorFeatures.SpeedRead = true;

                                if((plxtBuf[2] & 0x01) == 0x01) PlextorFeatures.SpeedReadEnabled = true;
                            }

                            plxtSense = dev.PlextorGetHiding(out plxtBuf, out senseBuf, dev.Timeout, out _);

                            if(!plxtSense)
                            {
                                PlextorFeatures.Hiding = true;

                                if((plxtBuf[2] & 0x02) == 0x02) PlextorFeatures.HidesRecordables = true;

                                if((plxtBuf[2] & 0x01) == 0x01) PlextorFeatures.HidesSessions = true;
                            }

                            plxtSense = dev.PlextorGetVariRec(out plxtBuf, out senseBuf, false, dev.Timeout, out _);

                            if(!plxtSense) PlextorFeatures.VariRec = true;

                            if(plxtDvd)
                            {
                                plxtSense = dev.PlextorGetVariRec(out plxtBuf, out senseBuf, true, dev.Timeout, out _);

                                if(!plxtSense) PlextorFeatures.VariRecDvd = true;

                                plxtSense = dev.PlextorGetBitsetting(out plxtBuf,
                                                                     out senseBuf,
                                                                     false,
                                                                     dev.Timeout,
                                                                     out _);

                                if(!plxtSense) PlextorFeatures.BitSetting = true;

                                plxtSense = dev.PlextorGetBitsetting(out plxtBuf,
                                                                     out senseBuf,
                                                                     true,
                                                                     dev.Timeout,
                                                                     out _);

                                if(!plxtSense) PlextorFeatures.BitSettingDl = true;

                                plxtSense = dev.PlextorGetTestWriteDvdPlus(out plxtBuf,
                                                                           out senseBuf,
                                                                           dev.Timeout,
                                                                           out _);

                                if(!plxtSense) PlextorFeatures.DvdPlusWriteTest = true;
                            }
                        }

#endregion Plextor

                        if(ScsiInquiry.Value.KreonPresent)
                        {
                            if(!dev.KreonGetFeatureList(out senseBuf, out KreonFeatures krFeatures, dev.Timeout, out _))
                                KreonFeatures = krFeatures;
                        }

                        break;
                    }

                    case PeripheralDeviceTypes.SequentialAccess:
                    {
                        sense = dev.ReadBlockLimits(out byte[] seqBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                            AaruConsole.ErrorWriteLine("READ BLOCK LIMITS:\n{0}", Sense.PrettifySense(senseBuf));
                        else
                            BlockLimits = seqBuf;

                        sense = dev.ReportDensitySupport(out seqBuf, out senseBuf, dev.Timeout, out _);

                        if(sense)
                            AaruConsole.ErrorWriteLine("REPORT DENSITY SUPPORT:\n{0}", Sense.PrettifySense(senseBuf));
                        else
                        {
                            DensitySupport       = seqBuf;
                            DensitySupportHeader = Decoders.SCSI.SSC.DensitySupport.DecodeDensity(seqBuf);
                        }

                        sense = dev.ReportDensitySupport(out seqBuf, out senseBuf, true, false, dev.Timeout, out _);

                        if(sense)
                        {
                            AaruConsole.ErrorWriteLine("REPORT DENSITY SUPPORT (MEDIUM):\n{0}",
                                                       Sense.PrettifySense(senseBuf));
                        }
                        else
                        {
                            MediumDensitySupport   = seqBuf;
                            MediaTypeSupportHeader = Decoders.SCSI.SSC.DensitySupport.DecodeMediumType(seqBuf);
                        }

                        break;
                    }
                }

                break;
            }

            case DeviceType.MMC:
            {
                bool sense = dev.ReadCid(out byte[] mmcBuf, out _, dev.Timeout, out _);

                if(!sense) CID = mmcBuf;

                sense = dev.ReadCsd(out mmcBuf, out _, dev.Timeout, out _);

                if(!sense) CSD = mmcBuf;

                sense = dev.ReadOcr(out mmcBuf, out _, dev.Timeout, out _);

                if(!sense) OCR = mmcBuf;

                sense = dev.ReadExtendedCsd(out mmcBuf, out _, dev.Timeout, out _);

                if(!sense && !ArrayHelpers.ArrayIsNullOrEmpty(mmcBuf)) ExtendedCSD = mmcBuf;
            }

                break;
            case DeviceType.SecureDigital:
            {
                bool sense = dev.ReadCid(out byte[] sdBuf, out _, dev.Timeout, out _);

                if(!sense) CID = sdBuf;

                sense = dev.ReadCsd(out sdBuf, out _, dev.Timeout, out _);

                if(!sense) CSD = sdBuf;

                sense = dev.ReadSdocr(out sdBuf, out _, dev.Timeout, out _);

                if(!sense) OCR = sdBuf;

                sense = dev.ReadScr(out sdBuf, out _, dev.Timeout, out _);

                if(!sense) SCR = sdBuf;
            }

                break;
            default:
                AaruConsole.ErrorWriteLine(Localization.Core.Unknown_device_type_0_cannot_get_information, dev.Type);

                break;
        }
    }
}