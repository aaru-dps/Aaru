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

                        break;
                    }
                default:
                    DicConsole.ErrorWriteLine("Unknown device type {0}, cannot get information.", dev.Type);
                    break;
            }
        }
    }
}

