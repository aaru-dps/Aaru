// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'device-info' verb.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Devices;
using System.IO;
using DiscImageChef.Console;
using System.Text;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Commands
{
    public static class DeviceInfo
    {
        public static void doDeviceInfo(DeviceInfoOptions options)
        {
            DicConsole.DebugWriteLine("Device-Info command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Device-Info command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Device-Info command", "--device={0}", options.DevicePath);
            DicConsole.DebugWriteLine("Device-Info command", "--output-prefix={0}", options.OutputPrefix);

            if(!File.Exists(options.DevicePath))
            {
                DicConsole.ErrorWriteLine("Specified device does not exist.");
                return;
            }

            if(options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + char.ToUpper(options.DevicePath[0]) + ':';
            }

            Device dev = new Device(options.DevicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Core.Statistics.AddDevice(dev);

            if(dev.IsUSB)
            {
                DicConsole.WriteLine("USB device");
                if(dev.USBDescriptors != null)
                    DicConsole.WriteLine("USB descriptor is {0} bytes", dev.USBDescriptors.Length);
                DicConsole.WriteLine("USB Vendor ID: {0:X4}", dev.USBVendorID);
                DicConsole.WriteLine("USB Product ID: {0:X4}", dev.USBProductID);
                DicConsole.WriteLine("USB Manufacturer: {0}", dev.USBManufacturerString);
                DicConsole.WriteLine("USB Product: {0}", dev.USBProductString);
                DicConsole.WriteLine("USB Serial number: {0}", dev.USBSerialString);
                DicConsole.WriteLine();
            }

            if(dev.IsFireWire)
            {
                DicConsole.WriteLine("FireWire device");
                DicConsole.WriteLine("FireWire Vendor ID: {0:X6}", dev.FireWireVendor);
                DicConsole.WriteLine("FireWire Model ID: {0:X6}", dev.FireWireModel);
                DicConsole.WriteLine("FireWire Manufacturer: {0}", dev.FireWireVendorName);
                DicConsole.WriteLine("FireWire Model: {0}", dev.FireWireModelName);
                DicConsole.WriteLine("FireWire GUID: {0:X16}", dev.FireWireGUID);
                DicConsole.WriteLine();
            }

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    {
                        AtaErrorRegistersCHS errorRegisters;

                        byte[] ataBuf;
                        bool sense = dev.AtaIdentify(out ataBuf, out errorRegisters);

                        if(sense)
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

                        doWriteFile(options.OutputPrefix, "_ata_identify.bin", "ATA IDENTIFY", ataBuf);

                        DicConsole.WriteLine(Identify.Prettify(ataBuf));

                        double duration;
                        dev.EnableMediaCardPassThrough(out errorRegisters, dev.Timeout, out duration);

                        if(errorRegisters.sector == 0xAA && errorRegisters.sectorCount == 0x55)
                        {
                            DicConsole.WriteLine("Device supports the Media Card Pass Through Command Set");
                            switch(errorRegisters.deviceHead & 0x7)
                            {
                                case 0:
                                    DicConsole.WriteLine("Device reports incorrect media card type");
                                    break;
                                case 1:
                                    DicConsole.WriteLine("Device contains a Secure Digital card");
                                    break;
                                case 2:
                                    DicConsole.WriteLine("Device contains a MultiMediaCard ");
                                    break;
                                case 3:
                                    DicConsole.WriteLine("Device contains a Secure Digital I/O card");
                                    break;
                                case 4:
                                    DicConsole.WriteLine("Device contains a Smart Media card");
                                    break;
                                default:
                                    DicConsole.WriteLine("Device contains unknown media card type {0}", errorRegisters.deviceHead & 0x07);
                                    break;
                            }

                            if((errorRegisters.deviceHead & 0x08) == 0x08)
                                DicConsole.WriteLine("Media card is write protected");

                            ushort specificData = (ushort)((errorRegisters.cylinderHigh * 0x100) + errorRegisters.cylinderLow);
                            if(specificData != 0)
                                DicConsole.WriteLine("Card specific data: 0x{0:X4}", specificData);
                        }

                        break;
                    }
                case DeviceType.ATAPI:
                    {
                        AtaErrorRegistersCHS errorRegisters;

                        byte[] ataBuf;
                        bool sense = dev.AtapiIdentify(out ataBuf, out errorRegisters);

                        if(sense)
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

                        doWriteFile(options.OutputPrefix, "_atapi_identify.bin", "ATAPI IDENTIFY", ataBuf);

                        DicConsole.WriteLine(Identify.Prettify(ataBuf));

                        // ATAPI devices are also SCSI devices
                        goto case DeviceType.SCSI;
                    }
                case DeviceType.SCSI:
                    {
                        byte[] senseBuf;
                        byte[] inqBuf;

                        bool sense = dev.ScsiInquiry(out inqBuf, out senseBuf);

                        if(sense)
                        {
                            DicConsole.ErrorWriteLine("SCSI error:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));

                            break;
                        }

                        if(dev.Type != DeviceType.ATAPI)
                            DicConsole.WriteLine("SCSI device");

                        doWriteFile(options.OutputPrefix, "_scsi_inquiry.bin", "SCSI INQUIRY", inqBuf);

                        Decoders.SCSI.Inquiry.SCSIInquiry? inq = Decoders.SCSI.Inquiry.Decode(inqBuf);
                        DicConsole.WriteLine(Decoders.SCSI.Inquiry.Prettify(inq));

                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, 0x00);

                        if(!sense)
                        {
                            byte[] pages = Decoders.SCSI.EVPD.DecodePage00(inqBuf);

                            if(pages != null)
                            {
                                foreach(byte page in pages)
                                {
                                    if(page >= 0x01 && page <= 0x7F)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("ASCII Page {0:X2}h: {1}", page, Decoders.SCSI.EVPD.DecodeASCIIPage(inqBuf));

                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0x80)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("Unit Serial Number: {0}", Decoders.SCSI.EVPD.DecodePage80(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0x81)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_81(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0x82)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("ASCII implemented operating definitions: {0}", Decoders.SCSI.EVPD.DecodePage82(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0x83)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_83(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0x84)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_84(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0x85)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_85(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0x86)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_86(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0x89)
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_89(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0xC0 && StringHandlers.CToString(inq.Value.VendorIdentification).ToLowerInvariant().Trim() == "quantum")
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_C0_Quantum(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if((page == 0xC0 || page == 0xC1) && StringHandlers.CToString(inq.Value.VendorIdentification).ToLowerInvariant().Trim() == "certance")
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_C0_C1_Certance(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if((page == 0xC2 || page == 0xC3 || page == 0xC4 || page == 0xC5 || page == 0xC6) &&
                                            StringHandlers.CToString(inq.Value.VendorIdentification).ToLowerInvariant().Trim() == "certance")
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_C2_C3_C4_C5_C6_Certance(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else if(page == 0xDF && StringHandlers.CToString(inq.Value.VendorIdentification).ToLowerInvariant().Trim() == "certance")
                                    {
                                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                        if(!sense)
                                        {
                                            DicConsole.WriteLine("{0}", Decoders.SCSI.EVPD.PrettifyPage_DF_Certance(inqBuf));
                                            doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                        }
                                    }
                                    else
                                    {
                                        if(page != 0x00)
                                        {
                                            DicConsole.DebugWriteLine("Device-Info command", "Found undecoded SCSI VPD page 0x{0:X2}", page);

                                            sense = dev.ScsiInquiry(out inqBuf, out senseBuf, page);
                                            if(!sense)
                                            {
                                                doWriteFile(options.OutputPrefix, string.Format("_scsi_evpd_{0:X2}h.bin", page), string.Format("SCSI INQUIRY EVPD {0:X2}h", page), inqBuf);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        byte[] modeBuf;
                        double duration;
                        Decoders.SCSI.Modes.DecodedMode? decMode = null;
                        Decoders.SCSI.PeripheralDeviceTypes devType = (Decoders.SCSI.PeripheralDeviceTypes)inq.Value.PeripheralDeviceType;

                        sense = dev.ModeSense10(out modeBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0xFF, 5, out duration);
                        if(sense || dev.Error)
                            sense = dev.ModeSense10(out modeBuf, out senseBuf, false, true, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out duration);

                        if(!sense && !dev.Error)
                            decMode = Decoders.SCSI.Modes.DecodeMode10(modeBuf, devType);

                        if(sense || dev.Error || !decMode.HasValue)
                        {
                            sense = dev.ModeSense6(out modeBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0xFF, 5, out duration);
                            if(sense || dev.Error)
                                sense = dev.ModeSense6(out modeBuf, out senseBuf, false, ScsiModeSensePageControl.Current, 0x3F, 0x00, 5, out duration);
                            if(sense || dev.Error)
                                sense = dev.ModeSense(out modeBuf, out senseBuf, 5, out duration);

                            if(!sense && !dev.Error)
                                decMode = Decoders.SCSI.Modes.DecodeMode6(modeBuf, devType);
                        }

                        if(!sense)
                            doWriteFile(options.OutputPrefix, "_scsi_modesense.bin", "SCSI MODE SENSE", modeBuf);

                        if(decMode.HasValue)
                        {
                            DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModeHeader(decMode.Value.Header, devType));

                            if(decMode.Value.Pages != null)
                            {
                                foreach(Decoders.SCSI.Modes.ModePage page in decMode.Value.Pages)
                                {
                                    //DicConsole.WriteLine("Page {0:X2}h subpage {1:X2}h is {2} bytes long", page.Page, page.Subpage, page.PageResponse.Length);
                                    switch(page.Page)
                                    {
                                        case 0x00:
                                            {
                                                if(devType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice && page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_00_SFF(page.PageResponse));
                                                else
                                                {
                                                    if(page.Subpage != 0)
                                                        DicConsole.WriteLine("Found unknown vendor mode page {0:X2}h subpage {1:X2}h", page.Page, page.Subpage);
                                                    else
                                                        DicConsole.WriteLine("Found unknown vendor mode page {0:X2}h", page.Page);
                                                }
                                                break;
                                            }
                                        case 0x01:
                                            {
                                                if(page.Subpage == 0)
                                                {
                                                    if(devType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
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
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_02(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x03:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_03(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x04:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_04(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x05:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_05(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x06:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_06(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x07:
                                            {
                                                if(page.Subpage == 0)
                                                {
                                                    if(devType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
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
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_08(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0A:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0A(page.PageResponse));
                                                else if(page.Subpage == 1)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0A_S01(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0B:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0B(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0D:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0D(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0E:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0E(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x0F:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_0F(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x10:
                                            {
                                                if(page.Subpage == 0)
                                                {
                                                    if(devType == Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_10_SSC(page.PageResponse));
                                                    else
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_10(page.PageResponse));
                                                }
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x11:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_11(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x12:
                                        case 0x13:
                                        case 0x14:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_12_13_14(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x1A:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1A(page.PageResponse));
                                                else if(page.Subpage == 1)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1A_S01(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x1B:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1B(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x1C:
                                            {
                                                if(page.Subpage == 0)
                                                {
                                                    if(devType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1C_SFF(page.PageResponse));
                                                    else
                                                        DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1C(page.PageResponse));
                                                }
                                                else if(page.Subpage == 1)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1C_S01(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x1D:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_1D(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x21:
                                            {
                                                if(StringHandlers.CToString(inq.Value.VendorIdentification).Trim() == "CERTANCE")
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyCertanceModePage_21(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x22:
                                            {
                                                if(StringHandlers.CToString(inq.Value.VendorIdentification).Trim() == "CERTANCE")
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyCertanceModePage_22(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x2A:
                                            {
                                                if(page.Subpage == 0)
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyModePage_2A(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        case 0x3E:
                                            {
                                                if(StringHandlers.CToString(inq.Value.VendorIdentification).Trim() == "FUJITSU")
                                                    DicConsole.WriteLine(Decoders.SCSI.Modes.PrettifyFujitsuModePage_3E(page.PageResponse));
                                                else
                                                    goto default;

                                                break;
                                            }
                                        default:
                                            {
                                                if(page.Subpage != 0)
                                                    DicConsole.WriteLine("Found unknown mode page {0:X2}h subpage {1:X2}h", page.Page, page.Subpage);
                                                else
                                                    DicConsole.WriteLine("Found unknown mode page {0:X2}h", page.Page);
                                                break;
                                            }
                                    }
                                }
                            }
                        }

                        if(devType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                        {

                            byte[] confBuf;
                            sense = dev.GetConfiguration(out confBuf, out senseBuf, dev.Timeout, out duration);

                            if(!sense)
                            {
                                doWriteFile(options.OutputPrefix, "_mmc_getconfiguration.bin", "MMC GET CONFIGURATION", confBuf);

                                Decoders.SCSI.MMC.Features.SeparatedFeatures ftr = Decoders.SCSI.MMC.Features.Separate(confBuf);

                                DicConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION length is {0} bytes", ftr.DataLength);
                                DicConsole.DebugWriteLine("Device-Info command", "GET CONFIGURATION current profile is {0:X4}h", ftr.CurrentProfile);
                                if(ftr.Descriptors != null)
                                {
                                    DicConsole.WriteLine("SCSI MMC GET CONFIGURATION Features:");
                                    foreach(Decoders.SCSI.MMC.Features.FeatureDescriptor desc in ftr.Descriptors)
                                    {
                                        DicConsole.DebugWriteLine("Device-Info command", "Feature {0:X4}h", desc.Code);

                                        switch(desc.Code)
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

                            // TODO: DVD drives respond correctly to BD status.
                            // While specification says if no medium is present
                            // it should inform all possible capabilities,
                            // testing drives show only supported media capabilities.
                            /*
                            byte[] strBuf;
                            sense = dev.ReadDiscStructure(out strBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.CapabilityList, 0, dev.Timeout, out duration);

                            if (!sense)
                            {
                                Decoders.SCSI.DiscStructureCapabilities.Capability[] caps = Decoders.SCSI.DiscStructureCapabilities.Decode(strBuf);
                                if (caps != null)
                                {
                                    foreach (Decoders.SCSI.DiscStructureCapabilities.Capability cap in caps)
                                    {
                                        if (cap.SDS && cap.RDS)
                                            DicConsole.WriteLine("Drive can READ/SEND DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                        else if (cap.SDS)
                                            DicConsole.WriteLine("Drive can SEND DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                        else if (cap.RDS)
                                            DicConsole.WriteLine("Drive can READ DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                    }
                                }
                            }

                            sense = dev.ReadDiscStructure(out strBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.CapabilityList, 0, dev.Timeout, out duration);

                            if (!sense)
                            {
                                Decoders.SCSI.DiscStructureCapabilities.Capability[] caps = Decoders.SCSI.DiscStructureCapabilities.Decode(strBuf);
                                if (caps != null)
                                {
                                    foreach (Decoders.SCSI.DiscStructureCapabilities.Capability cap in caps)
                                    {
                                        if (cap.SDS && cap.RDS)
                                            DicConsole.WriteLine("Drive can READ/SEND DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                        else if (cap.SDS)
                                            DicConsole.WriteLine("Drive can SEND DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                        else if (cap.RDS)
                                            DicConsole.WriteLine("Drive can READ DISC STRUCTURE format {0:X2}h", cap.FormatCode);
                                    }
                                }
                            }
                            */

                            #region Plextor
                            if(dev.Manufacturer == "PLEXTOR")
                            {
                                bool plxtSense = true;
                                bool plxtDvd = false;
                                byte[] plxtBuf = null;

                                switch(dev.Model)
                                {
                                    case "DVDR   PX-708A":
                                    case "DVDR   PX-708A2":
                                    case "DVDR   PX-712A":
                                        plxtDvd = true;
                                        plxtSense = dev.PlextorReadEeprom(out plxtBuf, out senseBuf, dev.Timeout, out duration);
                                        break;
                                    case "DVDR   PX-714A":
                                    case "DVDR   PX-716A":
                                    case "DVDR   PX-716AL":
                                    case "DVDR   PX-755A":
                                    case "DVDR   PX-760A":
                                        {
                                            byte[] plxtBufSmall;
                                            plxtBuf = new byte[256 * 4];
                                            for(byte i = 0; i < 4; i++)
                                            {
                                                plxtSense = dev.PlextorReadEepromBlock(out plxtBufSmall, out senseBuf, i, 256, dev.Timeout, out duration);
                                                if(plxtSense)
                                                    break;
                                                Array.Copy(plxtBufSmall, 0, plxtBuf, i * 256, 256);
                                            }
                                            plxtDvd = true;
                                            break;
                                        }
                                    default:
                                        {
                                            if(dev.Model.StartsWith("CD-R   ", StringComparison.Ordinal))
                                            {
                                                plxtSense = dev.PlextorReadEepromCDR(out plxtBuf, out senseBuf, dev.Timeout, out duration);
                                            }
                                            break;
                                        }
                                }

                                if(!plxtSense)
                                {
                                    doWriteFile(options.OutputPrefix, "_plextor_eeprom.bin", "PLEXTOR READ EEPROM", plxtBuf);

                                    ushort discs;
                                    uint cdReadTime, cdWriteTime, dvdReadTime = 0, dvdWriteTime = 0;

                                    BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
                                    if(plxtDvd)
                                    {
                                        discs = BigEndianBitConverter.ToUInt16(plxtBuf, 0x0120);
                                        cdReadTime = BigEndianBitConverter.ToUInt32(plxtBuf, 0x0122);
                                        cdWriteTime = BigEndianBitConverter.ToUInt32(plxtBuf, 0x0126);
                                        dvdReadTime = BigEndianBitConverter.ToUInt32(plxtBuf, 0x012A);
                                        dvdWriteTime = BigEndianBitConverter.ToUInt32(plxtBuf, 0x012E);
                                    }
                                    else
                                    {
                                        discs = BigEndianBitConverter.ToUInt16(plxtBuf, 0x0078);
                                        cdReadTime = BigEndianBitConverter.ToUInt32(plxtBuf, 0x006C);
                                        cdWriteTime = BigEndianBitConverter.ToUInt32(plxtBuf, 0x007A);
                                    }

                                    DicConsole.WriteLine("Drive has loaded a total of {0} discs", discs);
                                    DicConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds reading CDs",
                                        cdReadTime / 3600, (cdReadTime / 60) % 60, cdReadTime % 60);
                                    DicConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds writing CDs",
                                        cdWriteTime / 3600, (cdWriteTime / 60) % 60, cdWriteTime % 60);
                                    if(plxtDvd)
                                    {
                                        DicConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds reading DVDs",
                                            dvdReadTime / 3600, (dvdReadTime / 60) % 60, dvdReadTime % 60);
                                        DicConsole.WriteLine("Drive has spent {0} hours, {1} minutes and {2} seconds writing DVDs",
                                            dvdWriteTime / 3600, (dvdWriteTime / 60) % 60, dvdWriteTime % 60);
                                    }
                                }

                                bool plxtPwrRecEnabled;
                                ushort plxtPwrRecSpeed;
                                plxtSense = dev.PlextorGetPoweRec(out senseBuf, out plxtPwrRecEnabled, out plxtPwrRecSpeed, dev.Timeout, out duration);
                                if(!plxtSense)
                                {
                                    DicConsole.Write("Drive supports PoweRec");
                                    if(plxtPwrRecEnabled)
                                    {
                                        DicConsole.Write(", has it enabled");

                                        if(plxtPwrRecSpeed > 0)
                                            DicConsole.WriteLine(" and recommends {0} Kb/sec.", plxtPwrRecSpeed);
                                        else
                                            DicConsole.WriteLine(".");

                                        ushort plxtPwrRecSelected, plxtPwrRecMax, plxtPwrRecLast;
                                        plxtSense = dev.PlextorGetSpeeds(out senseBuf, out plxtPwrRecSelected, out plxtPwrRecMax, out plxtPwrRecLast, dev.Timeout, out duration);

                                        if(!plxtSense)
                                        {
                                            if(plxtPwrRecSelected > 0)
                                                DicConsole.WriteLine("Selected PoweRec speed for currently inserted media is {0} Kb/sec ({1}x)", plxtPwrRecSelected, plxtPwrRecSelected / 177);
                                            if(plxtPwrRecMax > 0)
                                                DicConsole.WriteLine("Maximum PoweRec speed for currently inserted media is {0} Kb/sec ({1}x)", plxtPwrRecMax, plxtPwrRecMax / 177);
                                            if(plxtPwrRecLast > 0)
                                                DicConsole.WriteLine("Last used PoweRec was {0} Kb/sec ({1}x)", plxtPwrRecLast, plxtPwrRecLast / 177);
                                        }
                                    }
                                    else
                                        DicConsole.WriteLine("PoweRec is disabled");
                                }

                                // TODO: Check it with a drive
                                plxtSense = dev.PlextorGetSilentMode(out plxtBuf, out senseBuf, dev.Timeout, out duration);
                                if(!plxtSense)
                                {
                                    DicConsole.WriteLine("Drive supports Plextor SilentMode");
                                    if(plxtBuf[0] == 1)
                                    {
                                        DicConsole.WriteLine("Plextor SilentMode is enabled:");
                                        if(plxtBuf[1] == 2)
                                            DicConsole.WriteLine("\tAccess time is slow");
                                        else
                                            DicConsole.WriteLine("\tAccess time is fast");

                                        if(plxtBuf[2] > 0)
                                            DicConsole.WriteLine("\tCD read speed limited to {0}x", plxtBuf[2]);
                                        if(plxtBuf[3] > 0 && plxtDvd)
                                            DicConsole.WriteLine("\tDVD read speed limited to {0}x", plxtBuf[3]);
                                        if(plxtBuf[4] > 0)
                                            DicConsole.WriteLine("\tCD write speed limited to {0}x", plxtBuf[4]);
                                        if(plxtBuf[6] > 0)
                                            DicConsole.WriteLine("\tTray eject speed limited to {0}", -(plxtBuf[6] + 48));
                                        if(plxtBuf[7] > 0)
                                            DicConsole.WriteLine("\tTray eject speed limited to {0}", plxtBuf[7] - 47);
                                    }
                                }

                                plxtSense = dev.PlextorGetGigaRec(out plxtBuf, out senseBuf, dev.Timeout, out duration);
                                if(!plxtSense)
                                {
                                    DicConsole.WriteLine("Drive supports Plextor GigaRec");
                                    // TODO: Pretty print it
                                }

                                plxtSense = dev.PlextorGetSecuRec(out plxtBuf, out senseBuf, dev.Timeout, out duration);
                                if(!plxtSense)
                                {
                                    DicConsole.WriteLine("Drive supports Plextor SecuRec");
                                    // TODO: Pretty print it
                                }

                                plxtSense = dev.PlextorGetSpeedRead(out plxtBuf, out senseBuf, dev.Timeout, out duration);
                                if(!plxtSense)
                                {
                                    DicConsole.Write("Drive supports Plextor SpeedRead");
                                    if((plxtBuf[2] & 0x01) == 0x01)
                                        DicConsole.WriteLine("and has it enabled");
                                    else
                                        DicConsole.WriteLine();
                                }

                                plxtSense = dev.PlextorGetHiding(out plxtBuf, out senseBuf, dev.Timeout, out duration);
                                if(!plxtSense)
                                {
                                    DicConsole.WriteLine("Drive supports hiding CD-Rs and forcing single session");

                                    if((plxtBuf[2] & 0x02) == 0x02)
                                        DicConsole.WriteLine("Drive currently hides CD-Rs");
                                    if((plxtBuf[2] & 0x01) == 0x01)
                                        DicConsole.WriteLine("Drive currently forces single session");
                                }

                                plxtSense = dev.PlextorGetVariRec(out plxtBuf, out senseBuf, false, dev.Timeout, out duration);
                                if(!plxtSense)
                                {
                                    DicConsole.WriteLine("Drive supports Plextor VariRec");
                                    // TODO: Pretty print it
                                }

                                if(plxtDvd)
                                {
                                    plxtSense = dev.PlextorGetVariRec(out plxtBuf, out senseBuf, true, dev.Timeout, out duration);
                                    if(!plxtSense)
                                    {
                                        DicConsole.WriteLine("Drive supports Plextor VariRec for DVDs");
                                        // TODO: Pretty print it
                                    }

                                    plxtSense = dev.PlextorGetBitsetting(out plxtBuf, out senseBuf, false, dev.Timeout, out duration);
                                    if(!plxtSense)
                                        DicConsole.WriteLine("Drive supports bitsetting DVD+R book type");
                                    plxtSense = dev.PlextorGetBitsetting(out plxtBuf, out senseBuf, true, dev.Timeout, out duration);
                                    if(!plxtSense)
                                        DicConsole.WriteLine("Drive supports bitsetting DVD+R DL book type");
                                    plxtSense = dev.PlextorGetTestWriteDvdPlus(out plxtBuf, out senseBuf, dev.Timeout, out duration);
                                    if(!plxtSense)
                                        DicConsole.WriteLine("Drive supports test writing DVD+");
                                }
                            }
                            #endregion Plextor
                        }

                        if(devType == Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
                        {
                            byte[] seqBuf;

                            sense = dev.ReadBlockLimits(out seqBuf, out senseBuf, dev.Timeout, out duration);
                            if(sense)
                                DicConsole.ErrorWriteLine("READ BLOCK LIMITS:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                            else
                            {
                                doWriteFile(options.OutputPrefix, "_ssc_readblocklimits.bin", "SSC READ BLOCK LIMITS", seqBuf);
                                DicConsole.WriteLine("Block limits for device:");
                                DicConsole.WriteLine(Decoders.SCSI.SSC.BlockLimits.Prettify(seqBuf));
                            }

                            sense = dev.ReportDensitySupport(out seqBuf, out senseBuf, dev.Timeout, out duration);
                            if(sense)
                                DicConsole.ErrorWriteLine("REPORT DENSITY SUPPORT:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                            else
                            {
                                doWriteFile(options.OutputPrefix, "_ssc_reportdensitysupport.bin", "SSC REPORT DENSITY SUPPORT", seqBuf);
                                Decoders.SCSI.SSC.DensitySupport.DensitySupportHeader? dens = Decoders.SCSI.SSC.DensitySupport.DecodeDensity(seqBuf);
                                if(dens.HasValue)
                                {
                                    DicConsole.WriteLine("Densities supported by device:");
                                    DicConsole.WriteLine(Decoders.SCSI.SSC.DensitySupport.PrettifyDensity(dens));
                                }
                            }

                            sense = dev.ReportDensitySupport(out seqBuf, out senseBuf, true, false, dev.Timeout, out duration);
                            if(sense)
                                DicConsole.ErrorWriteLine("REPORT DENSITY SUPPORT (MEDIUM):\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                            else
                            {
                                doWriteFile(options.OutputPrefix, "_ssc_reportdensitysupport_medium.bin", "SSC REPORT DENSITY SUPPORT (MEDIUM)", seqBuf);
                                Decoders.SCSI.SSC.DensitySupport.MediaTypeSupportHeader? meds = Decoders.SCSI.SSC.DensitySupport.DecodeMediumType(seqBuf);
                                if(meds.HasValue)
                                {
                                    DicConsole.WriteLine("Medium types supported by device:");
                                    DicConsole.WriteLine(Decoders.SCSI.SSC.DensitySupport.PrettifyMediumType(meds));
                                }
                                DicConsole.WriteLine(Decoders.SCSI.SSC.DensitySupport.PrettifyMediumType(seqBuf));
                            }
                        }

                        break;
                    }
                default:
                    DicConsole.ErrorWriteLine("Unknown device type {0}, cannot get information.", dev.Type);
                    break;
            }

            Core.Statistics.AddCommand("device-info");
        }

        static void doWriteFile(string outputPrefix, string outputSuffix, string whatWriting, byte[] data)
        {
            if(!string.IsNullOrEmpty(outputPrefix))
            {
                if(!File.Exists(outputPrefix + outputSuffix))
                {
                    try
                    {
                        DicConsole.DebugWriteLine("Device-Info command", "Writing " + whatWriting + " to {0}{1}", outputPrefix, outputSuffix);
                        FileStream outputFs = new FileStream(outputPrefix + outputSuffix, FileMode.CreateNew);
                        outputFs.Write(data, 0, data.Length);
                        outputFs.Close();
                    }
                    catch
                    {
                        DicConsole.ErrorWriteLine("Unable to write file {0}{1}", outputPrefix, outputSuffix);
                    }
                }
                else
                    DicConsole.ErrorWriteLine("Not overwriting file {0}{1}", outputPrefix, outputSuffix);
            }
        }
    }
}

