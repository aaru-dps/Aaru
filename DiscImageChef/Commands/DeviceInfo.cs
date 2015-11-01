// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceInfo.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using DiscImageChef.Devices;
using System.IO;
using DiscImageChef.Console;
using System.Text;

namespace DiscImageChef.Commands
{
    public static class DeviceInfo
    {
        public static void doDeviceInfo(DeviceInfoSubOptions options)
        {
            DicConsole.DebugWriteLine("Device-Info command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Device-Info command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Device-Info command", "--device={0}", options.DevicePath);

            if (options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && Char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + Char.ToUpper(options.DevicePath[0]) + ':';
            }

            Device dev = new Device(options.DevicePath);

            if (dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            switch (dev.Type)
            {
                case DeviceType.ATA:
                    {
                        AtaErrorRegistersCHS errorRegisters;

                        byte[] ataBuf;
                        bool sense = dev.AtaIdentify(out ataBuf, out errorRegisters);

                        if (sense)
                        {
                            DicConsole.DebugWriteLine("Device-Info command", "STATUS = 0x{0:X2}", errorRegisters.status);
                            DicConsole.DebugWriteLine("Device-Info command", "ERROR = 0x{0:X2}", errorRegisters.error);
                            DicConsole.DebugWriteLine("Device-Info command", "NSECTOR = 0x{0:X2}", errorRegisters.sectorCount);
                            DicConsole.DebugWriteLine("Device-Info command", "SECTOR = 0x{0:X2}", errorRegisters.sector);
                            DicConsole.DebugWriteLine("Device-Info command", "CYLHIGH = 0x{0:X2}", errorRegisters.cylinderHigh);
                            DicConsole.DebugWriteLine("Device-Info command", "CYLLOW = 0x{0:X2}", errorRegisters.cylinderLow);
                            DicConsole.DebugWriteLine("Device-Info command", "DEVICE = 0x{0:X2}", errorRegisters.deviceHead);
                            DicConsole.DebugWriteLine("Device-Info command", "COMMAND = 0x{0:X2}", errorRegisters.command);
                            DicConsole.DebugWriteLine("Device-Info command", "Error code = {0}", dev.LastError);
                            break;
                        }

                        DicConsole.WriteLine(Decoders.ATA.Identify.Prettify(ataBuf));
                        break;
                    }
                case DeviceType.ATAPI:
                    {
                        AtaErrorRegistersCHS errorRegisters;

                        byte[] ataBuf;
                        bool sense = dev.AtapiIdentify(out ataBuf, out errorRegisters);

                        if (sense)
                        {
                            DicConsole.DebugWriteLine("Device-Info command", "STATUS = 0x{0:X2}", errorRegisters.status);
                            DicConsole.DebugWriteLine("Device-Info command", "ERROR = 0x{0:X2}", errorRegisters.error);
                            DicConsole.DebugWriteLine("Device-Info command", "NSECTOR = 0x{0:X2}", errorRegisters.sectorCount);
                            DicConsole.DebugWriteLine("Device-Info command", "SECTOR = 0x{0:X2}", errorRegisters.sector);
                            DicConsole.DebugWriteLine("Device-Info command", "CYLHIGH = 0x{0:X2}", errorRegisters.cylinderHigh);
                            DicConsole.DebugWriteLine("Device-Info command", "CYLLOW = 0x{0:X2}", errorRegisters.cylinderLow);
                            DicConsole.DebugWriteLine("Device-Info command", "DEVICE = 0x{0:X2}", errorRegisters.deviceHead);
                            DicConsole.DebugWriteLine("Device-Info command", "COMMAND = 0x{0:X2}", errorRegisters.command);
                            DicConsole.DebugWriteLine("Device-Info command", "Error code = {0}", dev.LastError);
                            break;
                        }

                        DicConsole.WriteLine(Decoders.ATA.Identify.Prettify(ataBuf));

                        // ATAPI devices are also SCSI devices
                        goto case DeviceType.SCSI;
                    }
                case DeviceType.SCSI:
                    {
                        byte[] senseBuf;
                        byte[] inqBuf;

                        bool sense = dev.ScsiInquiry(out inqBuf, out senseBuf);

                        if (sense)
                        {
                            DicConsole.ErrorWriteLine("SCSI error. Sense decoding not yet implemented.");

                            #if DEBUG
                            FileStream senseFs = File.Open("sense.bin", FileMode.OpenOrCreate);
                            senseFs.Write(senseBuf, 0, senseBuf.Length);
                            #endif

                            break;
                        }

                        if (dev.Type != DeviceType.ATAPI)
                            DicConsole.WriteLine("SCSI device");

                        Decoders.SCSI.Inquiry.SCSIInquiry? inq = Decoders.SCSI.Inquiry.Decode(inqBuf);
                        DicConsole.WriteLine(Decoders.SCSI.Inquiry.Prettify(inq));

                        bool scsi83 = false;
                        string scsiSerial = null;
                        StringBuilder sb = null;

                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, 0x00);

                        if (!sense)
                        {
                            byte[] pages = Decoders.SCSI.EVPD.DecodePage00(inqBuf);

                            if (pages != null)
                            {
                                foreach (byte page in pages)
                                {
                                    if (page >= 0x01 && page <= 0x7F)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if (!sense)
                                        {
                                            if (sb == null)
                                                sb = new StringBuilder();
                                            sb.AppendFormat("Page 0x{0:X2}: ", Decoders.SCSI.EVPD.DecodeASCIIPage(inqBuf)).AppendLine();
                                        }
                                    }
                                    else if (page == 0x80)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if (!sense)
                                        {
                                            scsi83 = true;
                                            scsiSerial = Decoders.SCSI.EVPD.DecodePage80(inqBuf);
                                        }
                                    }
                                    else
                                    {
                                        if (page != 0x00)
                                            DicConsole.DebugWriteLine("Device-Info command", "Found undecoded SCSI VPD page 0x{0:X2}", page);
                                    }
                                }
                            }
                        }

                        if (scsi83)
                            DicConsole.WriteLine("Unit Serial Number: {0}", scsiSerial);

                        if (sb != null)
                        {
                            DicConsole.WriteLine("ASCII VPDs:");
                            DicConsole.WriteLine(sb.ToString());
                        }

                        byte[] modeBuf;
                        double duration;
                        Decoders.SCSI.Modes.DecodedMode? decMode = null;
                        Decoders.SCSI.PeripheralDeviceTypes devType = (DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes)inq.Value.PeripheralDeviceType;

                        sense = dev.ModeSense10(out modeBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0xFF, dev.Timeout, out duration);
                        if (sense || dev.Error)
                        {
                            sense = dev.ModeSense10(out modeBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0x00, dev.Timeout, out duration);
                        }

                        if (!sense && !dev.Error)
                        {
                            decMode = Decoders.SCSI.Modes.DecodeMode10(modeBuf, devType);
                        }

                        if(sense || dev.Error || !decMode.HasValue)
                        {
                            sense = dev.ModeSense6(out modeBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, dev.Timeout, out duration);
                            if (sense || dev.Error)
                                sense = dev.ModeSense6(out modeBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, dev.Timeout, out duration);
                            if (sense || dev.Error)
                                sense = dev.ModeSense(out modeBuf, out senseBuf, 15000, out duration);

                            if (!sense && !dev.Error)
                                decMode = Decoders.SCSI.Modes.DecodeMode6(modeBuf, devType);
                        }

                        if (decMode.HasValue)
                        {
                            DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModeHeader(decMode.Value.Header, devType));

                            if (decMode.Value.Pages != null)
                            {
                                foreach (Decoders.SCSI.Modes.ModePage page in decMode.Value.Pages)
                                {
                                    //DicConsole.WriteLine("Page {0:X2}h subpage {1:X2}h is {2} bytes long", page.Page, page.Subpage, page.PageResponse.Length);
                                    switch (page.Page)
                                    {
                                        case 0x00:
                                            {
                                                if (devType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice && page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_00_SFF(page.PageResponse));
                                                else
                                                {
                                                    if (page.Subpage != 0)
                                                        DicConsole.WriteLine("Found unknown vendor mode page {0:X2}h subpage {1:X2}h", page.Page, page.Subpage);
                                                    else
                                                        DicConsole.WriteLine("Found unknown vendor mode page {0:X2}h", page.Page);
                                                }
                                                break;
                                            }
                                        case 0x01:
                                            {
                                                if (page.Subpage == 0)
                                                {
                                                    if (devType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_01_MMC(page.PageResponse));
                                                    else
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_01(page.PageResponse));
                                                }
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x02:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_02(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x03:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_03(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x04:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_04(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x05:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_05(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x06:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_06(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x07:
                                            {
                                                if (page.Subpage == 0)
                                                {
                                                    if (devType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_07_MMC(page.PageResponse));
                                                    else
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_07(page.PageResponse));
                                                }
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x08:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_08(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0A:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0A(page.PageResponse));
                                                else if (page.Subpage == 1)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0A_S01(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0B:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0B(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0D:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0D(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0E:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0E(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0F:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0F(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x10:
                                            {
                                                if (page.Subpage == 0)
                                                {
                                                    if (devType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_10_SSC(page.PageResponse));
                                                    else
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_10(page.PageResponse));
                                                }
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x1A:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1A(page.PageResponse));
                                                else if (page.Subpage == 1)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1A_S01(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x1B:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1B(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x1C:
                                            {
                                                if (page.Subpage == 0)
                                                {
                                                    if (devType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1C_SFF(page.PageResponse));
                                                    else
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1C(page.PageResponse));
                                                }
                                                else if (page.Subpage == 1)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1C_S01(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x2A:
                                            {
                                                if (page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_2A(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        default:
                                            {
                                                if (page.Subpage != 0)
                                                    DicConsole.WriteLine("Found unknown mode page {0:X2}h subpage {1:X2}h", page.Page, page.Subpage);
                                                else
                                                    DicConsole.WriteLine("Found unknown mode page {0:X2}h", page.Page);
                                                break;
                                            }
                                    }
                                }
                            }
                        }

                        if (devType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                        {
                            
                        }

                        byte[] confBuf;
                        sense = dev.GetConfiguration(out confBuf, out senseBuf, dev.Timeout, out duration);

                        if (!sense)
                        {
                            Decoders.SCSI.MMC.Features.SeparatedFeatures ftr = Decoders.SCSI.MMC.Features.Separate(confBuf);

                            DicConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION length is {0} bytes", ftr.DataLength);
                            DicConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION current profile is {0:X4}h", ftr.CurrentProfile);
                            if (ftr.Descriptors != null)
                            {
                                DicConsole.WriteLine("SCSI MMC GET CONFIGURATION Features:");
                                foreach (Decoders.SCSI.MMC.Features.FeatureDescriptor desc in ftr.Descriptors)
                                {
                                    DicConsole.DebugWriteLine("Device-Info command", "Feature {0:X4}h", desc.Code);

                                    switch (desc.Code)
                                    {
                                        case 0x0000:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0000(desc.Data));
                                            break;
                                        case 0x0001:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0001(desc.Data));
                                            break;
                                        case 0x0002:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0002(desc.Data));
                                            break;
                                        case 0x0003:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0003(desc.Data));
                                            break;
                                        case 0x0004:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0004(desc.Data));
                                            break;
                                        case 0x0010:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0010(desc.Data));
                                            break;
                                        case 0x001D:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_001D(desc.Data));
                                            break;
                                        case 0x001E:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_001E(desc.Data));
                                            break;
                                        case 0x001F:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_001F(desc.Data));
                                            break;
                                        case 0x0020:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0020(desc.Data));
                                            break;
                                        case 0x0021:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0021(desc.Data));
                                            break;
                                        case 0x0022:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0022(desc.Data));
                                            break;
                                        case 0x0023:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0023(desc.Data));
                                            break;
                                        case 0x0024:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0024(desc.Data));
                                            break;
                                        case 0x0025:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0025(desc.Data));
                                            break;
                                        case 0x0026:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0026(desc.Data));
                                            break;
                                        case 0x0027:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0027(desc.Data));
                                            break;
                                        case 0x0028:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0028(desc.Data));
                                            break;
                                        case 0x0029:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0029(desc.Data));
                                            break;
                                        case 0x002A:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_002A(desc.Data));
                                            break;
                                        case 0x002B:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_002B(desc.Data));
                                            break;
                                        case 0x002C:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_002C(desc.Data));
                                            break;
                                        case 0x002D:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_002D(desc.Data));
                                            break;
                                        case 0x002E:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_002E(desc.Data));
                                            break;
                                        case 0x002F:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_002F(desc.Data));
                                            break;
                                        case 0x0030:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0030(desc.Data));
                                            break;
                                        case 0x0031:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0031(desc.Data));
                                            break;
                                        case 0x0032:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0032(desc.Data));
                                            break;
                                        case 0x0033:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0033(desc.Data));
                                            break;
                                        case 0x0035:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0035(desc.Data));
                                            break;
                                        case 0x0037:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0037(desc.Data));
                                            break;
                                        case 0x0038:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0038(desc.Data));
                                            break;
                                        case 0x003A:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_003A(desc.Data));
                                            break;
                                        case 0x003B:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_003B(desc.Data));
                                            break;
                                        case 0x0040:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0040(desc.Data));
                                            break;
                                        case 0x0041:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0041(desc.Data));
                                            break;
                                        case 0x0042:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0042(desc.Data));
                                            break;
                                        case 0x0050:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0050(desc.Data));
                                            break;
                                        case 0x0051:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0051(desc.Data));
                                            break;
                                        case 0x0080:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0080(desc.Data));
                                            break;
                                        case 0x0100:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0100(desc.Data));
                                            break;
                                        case 0x0101:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0101(desc.Data));
                                            break;
                                        case 0x0102:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0102(desc.Data));
                                            break;
                                        case 0x0103:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0103(desc.Data));
                                            break;
                                        case 0x0104:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0104(desc.Data));
                                            break;
                                        case 0x0105:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0105(desc.Data));
                                            break;
                                        case 0x0106:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0106(desc.Data));
                                            break;
                                        case 0x0107:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0107(desc.Data));
                                            break;
                                        case 0x0108:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0108(desc.Data));
                                            break;
                                        case 0x0109:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0109(desc.Data));
                                            break;
                                        case 0x010A:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_010A(desc.Data));
                                            break;
                                        case 0x010B:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_010B(desc.Data));
                                            break;
                                        case 0x010C:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_010C(desc.Data));
                                            break;
                                        case 0x010D:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_010D(desc.Data));
                                            break;
                                        case 0x010E:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_010E(desc.Data));
                                            break;
                                        case 0x0110:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0110(desc.Data));
                                            break;
                                        case 0x0113:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0113(desc.Data));
                                            break;
                                        case 0x0142:
                                            DicConsole.WriteLine(Decoders.SCSI.MMC.Features.Prettify_0142(desc.Data));
                                            break;
                                        default:
                                            DicConsole.WriteLine("Found unknown feature code {0:X4}h", desc.Code);
                                            break;
                                    }
                                }
                            }
                            else
                                DicConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION returned no feature descriptors");
                        }

                        break;
                    }
                default:
                    DicConsole.ErrorWriteLine("Unknown device type {0}, cannot get information.", dev.Type);
                    break;
            }
        }
    }
}

