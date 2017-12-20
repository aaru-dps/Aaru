// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : General.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from SCSI devices.
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
using System.Collections.Generic;
using System.IO;
using DiscImageChef.Console;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;

namespace DiscImageChef.Core.Devices.Report.SCSI
{
    public static class General
    {
        public static void Report(Device dev, ref DeviceReport report, bool debug, ref bool removable)
        {
            if(report == null) return;

            byte[] senseBuffer;
            byte[] buffer;
            double duration;
            bool sense;
            uint timeout = 5;
            ConsoleKeyInfo pressedKey;

            if(dev.IsUsb) Usb.Report(dev, ref report, debug, ref removable);

            if(dev.IsFireWire) FireWire.Report(dev, ref report, debug, ref removable);

            if(dev.IsPcmcia) Pcmcia.Report(dev, ref report, debug, ref removable);

            if(!dev.IsUsb && !dev.IsFireWire && dev.IsRemovable)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the media removable from the reading/writing elements (flash memories ARE NOT removable)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                removable = pressedKey.Key == ConsoleKey.Y;
            }

            if(dev.Type == DeviceType.ATAPI) Atapi.Report(dev, ref report, debug, ref removable);

            DicConsole.WriteLine("Querying SCSI INQUIRY...");
            sense = dev.ScsiInquiry(out buffer, out senseBuffer);

            report.SCSI = new scsiType();

            if(!sense && Decoders.SCSI.Inquiry.Decode(buffer).HasValue)
            {
                Decoders.SCSI.Inquiry.SCSIInquiry inq = Decoders.SCSI.Inquiry.Decode(buffer).Value;

                List<ushort> versionDescriptors = new List<ushort>();
                report.SCSI.Inquiry = new scsiInquiryType();

                if(inq.DeviceTypeModifier != 0)
                {
                    report.SCSI.Inquiry.DeviceTypeModifier = inq.DeviceTypeModifier;
                    report.SCSI.Inquiry.DeviceTypeModifierSpecified = true;
                }
                if(inq.ISOVersion != 0)
                {
                    report.SCSI.Inquiry.ISOVersion = inq.ISOVersion;
                    report.SCSI.Inquiry.ISOVersionSpecified = true;
                }
                if(inq.ECMAVersion != 0)
                {
                    report.SCSI.Inquiry.ECMAVersion = inq.ECMAVersion;
                    report.SCSI.Inquiry.ECMAVersionSpecified = true;
                }
                if(inq.ANSIVersion != 0)
                {
                    report.SCSI.Inquiry.ANSIVersion = inq.ANSIVersion;
                    report.SCSI.Inquiry.ANSIVersionSpecified = true;
                }
                if(inq.ResponseDataFormat != 0)
                {
                    report.SCSI.Inquiry.ResponseDataFormat = inq.ResponseDataFormat;
                    report.SCSI.Inquiry.ResponseDataFormatSpecified = true;
                }
                if(!string.IsNullOrWhiteSpace(StringHandlers.CToString(inq.VendorIdentification)))
                {
                    report.SCSI.Inquiry.VendorIdentification =
                        StringHandlers.CToString(inq.VendorIdentification).Trim();
                    if(!string.IsNullOrWhiteSpace(report.SCSI.Inquiry.VendorIdentification))
                        report.SCSI.Inquiry.VendorIdentificationSpecified = true;
                }
                if(!string.IsNullOrWhiteSpace(StringHandlers.CToString(inq.ProductIdentification)))
                {
                    report.SCSI.Inquiry.ProductIdentification =
                        StringHandlers.CToString(inq.ProductIdentification).Trim();
                    if(!string.IsNullOrWhiteSpace(report.SCSI.Inquiry.ProductIdentification))
                        report.SCSI.Inquiry.ProductIdentificationSpecified = true;
                }
                if(!string.IsNullOrWhiteSpace(StringHandlers.CToString(inq.ProductRevisionLevel)))
                {
                    report.SCSI.Inquiry.ProductRevisionLevel =
                        StringHandlers.CToString(inq.ProductRevisionLevel).Trim();
                    if(!string.IsNullOrWhiteSpace(report.SCSI.Inquiry.ProductRevisionLevel))
                        report.SCSI.Inquiry.ProductRevisionLevelSpecified = true;
                }
                if(inq.VersionDescriptors != null)
                {
                    foreach(ushort descriptor in inq.VersionDescriptors)
                    {
                        if(descriptor != 0) versionDescriptors.Add(descriptor);
                    }

                    if(versionDescriptors.Count > 0)
                        report.SCSI.Inquiry.VersionDescriptors = versionDescriptors.ToArray();
                }

                report.SCSI.Inquiry.PeripheralQualifier = (Decoders.SCSI.PeripheralQualifiers)inq.PeripheralQualifier;
                report.SCSI.Inquiry.PeripheralDeviceType =
                    (Decoders.SCSI.PeripheralDeviceTypes)inq.PeripheralDeviceType;
                report.SCSI.Inquiry.AsymmetricalLUNAccess = (Decoders.SCSI.TGPSValues)inq.TPGS;
                report.SCSI.Inquiry.SPIClocking = (Decoders.SCSI.SPIClocking)inq.Clocking;

                report.SCSI.Inquiry.AccessControlCoordinator = inq.ACC;
                report.SCSI.Inquiry.ACKRequests = inq.ACKREQQ;
                report.SCSI.Inquiry.AERCSupported = inq.AERC;
                report.SCSI.Inquiry.Address16 = inq.Addr16;
                report.SCSI.Inquiry.Address32 = inq.Addr32;
                report.SCSI.Inquiry.BasicQueueing = inq.BQue;
                report.SCSI.Inquiry.EnclosureServices = inq.EncServ;
                report.SCSI.Inquiry.HierarchicalLUN = inq.HiSup;
                report.SCSI.Inquiry.IUS = inq.IUS;
                report.SCSI.Inquiry.LinkedCommands = inq.Linked;
                report.SCSI.Inquiry.MediumChanger = inq.MChngr;
                report.SCSI.Inquiry.MultiPortDevice = inq.MultiP;
                report.SCSI.Inquiry.NormalACA = inq.NormACA;
                report.SCSI.Inquiry.Protection = inq.Protect;
                report.SCSI.Inquiry.QAS = inq.QAS;
                report.SCSI.Inquiry.RelativeAddressing = inq.RelAddr;
                report.SCSI.Inquiry.Removable = inq.RMB;
                report.SCSI.Inquiry.TaggedCommandQueue = inq.CmdQue;
                report.SCSI.Inquiry.TerminateTaskSupported = inq.TrmTsk;
                report.SCSI.Inquiry.ThirdPartyCopy = inq.ThreePC;
                report.SCSI.Inquiry.TranferDisable = inq.TranDis;
                report.SCSI.Inquiry.SoftReset = inq.SftRe;
                report.SCSI.Inquiry.StorageArrayController = inq.SCCS;
                report.SCSI.Inquiry.SyncTransfer = inq.Sync;
                report.SCSI.Inquiry.WideBus16 = inq.WBus16;
                report.SCSI.Inquiry.WideBus32 = inq.WBus32;

                if(debug) report.SCSI.Inquiry.Data = buffer;
            }

            DicConsole.WriteLine("Querying list of SCSI EVPDs...");
            sense = dev.ScsiInquiry(out buffer, out senseBuffer, 0x00);

            if(!sense)
            {
                byte[] evpdPages = Decoders.SCSI.EVPD.DecodePage00(buffer);
                if(evpdPages != null && evpdPages.Length > 0)
                {
                    List<pageType> evpds = new List<pageType>();
                    foreach(byte page in evpdPages)
                    {
                        if(page != 0x80)
                        {
                            DicConsole.WriteLine("Querying SCSI EVPD {0:X2}h...", page);
                            sense = dev.ScsiInquiry(out buffer, out senseBuffer, page);
                            if(!sense)
                            {
                                pageType evpd = new pageType();
                                evpd.page = page;
                                evpd.value = buffer;
                                evpds.Add(evpd);
                            }
                        }
                    }

                    if(evpds.Count > 0) report.SCSI.EVPDPages = evpds.ToArray();
                }
            }

            if(removable)
            {
                if(dev.ScsiType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                {
                    dev.AllowMediumRemoval(out senseBuffer, timeout, out duration);
                    dev.EjectTray(out senseBuffer, timeout, out duration);
                }
                else if(dev.ScsiType == Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
                {
                    dev.SpcAllowMediumRemoval(out senseBuffer, timeout, out duration);
                    DicConsole.WriteLine("Asking drive to unload tape (can take a few minutes)...");
                    dev.Unload(out senseBuffer, timeout, out duration);
                }
                DicConsole.WriteLine("Please remove any media from the device and press any key when it is out.");
                System.Console.ReadKey(true);
            }

            Decoders.SCSI.Modes.DecodedMode? decMode = null;
            Decoders.SCSI.PeripheralDeviceTypes devType = dev.ScsiType;

            DicConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (10)...");
            sense = dev.ModeSense10(out byte[] mode10Buffer, out senseBuffer, false, true,
                                    ScsiModeSensePageControl.Default, 0x3F, 0xFF, timeout, out duration);
            if(sense || dev.Error)
            {
                DicConsole.WriteLine("Querying all mode pages using SCSI MODE SENSE (10)...");
                sense = dev.ModeSense10(out mode10Buffer, out senseBuffer, false, true,
                                        ScsiModeSensePageControl.Default, 0x3F, 0x00, timeout, out duration);
                if(!sense && !dev.Error)
                {
                    report.SCSI.SupportsModeSense10 = true;
                    report.SCSI.SupportsModeSubpages = false;
                    decMode = Decoders.SCSI.Modes.DecodeMode10(mode10Buffer, devType);
                }
            }
            else
            {
                report.SCSI.SupportsModeSense10 = true;
                report.SCSI.SupportsModeSubpages = true;
                decMode = Decoders.SCSI.Modes.DecodeMode10(mode10Buffer, devType);
            }

            DicConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (6)...");
            sense = dev.ModeSense6(out byte[] mode6Buffer, out senseBuffer, false, ScsiModeSensePageControl.Default,
                                   0x3F, 0xFF, timeout, out duration);
            if(sense || dev.Error)
            {
                DicConsole.WriteLine("Querying all mode pages using SCSI MODE SENSE (6)...");
                sense = dev.ModeSense6(out mode6Buffer, out senseBuffer, false, ScsiModeSensePageControl.Default, 0x3F,
                                       0x00, timeout, out duration);
                if(sense || dev.Error)
                {
                    DicConsole.WriteLine("Querying SCSI MODE SENSE (6)...");
                    sense = dev.ModeSense(out mode6Buffer, out senseBuffer, timeout, out duration);
                }
            }
            else report.SCSI.SupportsModeSubpages = true;

            if(!sense && !dev.Error && !decMode.HasValue)
                decMode = Decoders.SCSI.Modes.DecodeMode6(mode6Buffer, devType);

            report.SCSI.SupportsModeSense6 |= !sense && !dev.Error;

            Decoders.SCSI.Modes.ModePage_2A? cdromMode = null;

            if(debug && report.SCSI.SupportsModeSense6) report.SCSI.ModeSense6Data = mode6Buffer;
            if(debug && report.SCSI.SupportsModeSense10) report.SCSI.ModeSense10Data = mode10Buffer;

            if(decMode.HasValue)
            {
                report.SCSI.ModeSense = new modeType();
                report.SCSI.ModeSense.BlankCheckEnabled = decMode.Value.Header.EBC;
                report.SCSI.ModeSense.DPOandFUA = decMode.Value.Header.DPOFUA;
                report.SCSI.ModeSense.WriteProtected = decMode.Value.Header.WriteProtected;

                if(decMode.Value.Header.BufferedMode > 0)
                {
                    report.SCSI.ModeSense.BufferedMode = decMode.Value.Header.BufferedMode;
                    report.SCSI.ModeSense.BufferedModeSpecified = true;
                }

                if(decMode.Value.Header.Speed > 0)
                {
                    report.SCSI.ModeSense.Speed = decMode.Value.Header.Speed;
                    report.SCSI.ModeSense.SpeedSpecified = true;
                }

                if(decMode.Value.Pages != null)
                {
                    List<modePageType> modePages = new List<modePageType>();
                    foreach(Decoders.SCSI.Modes.ModePage page in decMode.Value.Pages)
                    {
                        modePageType modePage = new modePageType();
                        modePage.page = page.Page;
                        modePage.subpage = page.Subpage;
                        modePage.value = page.PageResponse;
                        modePages.Add(modePage);

                        if(modePage.page == 0x2A && modePage.subpage == 0x00)
                        {
                            cdromMode = Decoders.SCSI.Modes.DecodeModePage_2A(page.PageResponse);
                        }
                    }

                    if(modePages.Count > 0) report.SCSI.ModeSense.ModePages = modePages.ToArray();
                }
            }

            List<string> mediaTypes = new List<string>();

            if(dev.ScsiType == Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                Mmc.Report(dev, ref report, debug, ref cdromMode, ref mediaTypes);
            else if(dev.ScsiType == Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
                Ssc.Report(dev, ref report, debug);
            else
            {
                if(removable)
                {
                    List<testedMediaType> mediaTests = new List<testedMediaType>();

                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.N)
                    {
                        pressedKey = new ConsoleKeyInfo();
                        while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                        {
                            DicConsole.Write("Do you have media that you can insert in the drive? (Y/N): ");
                            pressedKey = System.Console.ReadKey();
                            DicConsole.WriteLine();
                        }

                        if(pressedKey.Key == ConsoleKey.Y)
                        {
                            DicConsole.WriteLine("Please insert it in the drive and press any key when it is ready.");
                            System.Console.ReadKey(true);

                            testedMediaType mediaTest = new testedMediaType();
                            DicConsole.Write("Please write a description of the media type and press enter: ");
                            mediaTest.MediumTypeName = System.Console.ReadLine();
                            DicConsole.Write("Please write the media manufacturer and press enter: ");
                            mediaTest.Manufacturer = System.Console.ReadLine();
                            DicConsole.Write("Please write the media model and press enter: ");
                            mediaTest.Model = System.Console.ReadLine();

                            mediaTest.ManufacturerSpecified = true;
                            mediaTest.ModelSpecified = true;
                            mediaTest.MediaIsRecognized = true;

                            sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                            if(sense)
                            {
                                Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuffer);
                                if(decSense.HasValue)
                                {
                                    if(decSense.Value.ASC == 0x3A)
                                    {
                                        int leftRetries = 20;
                                        while(leftRetries > 0)
                                        {
                                            DicConsole.Write("\rWaiting for drive to become ready");
                                            System.Threading.Thread.Sleep(2000);
                                            sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                                            if(!sense) break;

                                            leftRetries--;
                                        }

                                        mediaTest.MediaIsRecognized &= !sense;
                                    }
                                    else if(decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                                    {
                                        int leftRetries = 20;
                                        while(leftRetries > 0)
                                        {
                                            DicConsole.Write("\rWaiting for drive to become ready");
                                            System.Threading.Thread.Sleep(2000);
                                            sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                                            if(!sense) break;

                                            leftRetries--;
                                        }

                                        mediaTest.MediaIsRecognized &= !sense;
                                    }
                                    else mediaTest.MediaIsRecognized = false;
                                }
                                else mediaTest.MediaIsRecognized = false;
                            }

                            if(mediaTest.MediaIsRecognized)
                            {
                                mediaTest.SupportsReadCapacitySpecified = true;
                                mediaTest.SupportsReadCapacity16Specified = true;

                                DicConsole.WriteLine("Querying SCSI READ CAPACITY...");
                                sense = dev.ReadCapacity(out buffer, out senseBuffer, timeout, out duration);
                                if(!sense && !dev.Error)
                                {
                                    mediaTest.SupportsReadCapacity = true;
                                    mediaTest.Blocks =
                                        (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) +
                                                buffer[3]) + 1;
                                    mediaTest.BlockSize =
                                        (uint)((buffer[4] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
                                    mediaTest.BlocksSpecified = true;
                                    mediaTest.BlockSizeSpecified = true;
                                }

                                DicConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
                                sense = dev.ReadCapacity16(out buffer, out buffer, timeout, out duration);
                                if(!sense && !dev.Error)
                                {
                                    mediaTest.SupportsReadCapacity16 = true;
                                    byte[] temp = new byte[8];
                                    Array.Copy(buffer, 0, temp, 0, 8);
                                    Array.Reverse(temp);
                                    mediaTest.Blocks = BitConverter.ToUInt64(temp, 0) + 1;
                                    mediaTest.BlockSize =
                                        (uint)((buffer[8] << 24) + (buffer[9] << 16) + (buffer[10] << 8) +
                                               buffer[11]);
                                    mediaTest.BlocksSpecified = true;
                                    mediaTest.BlockSizeSpecified = true;
                                }

                                decMode = null;

                                DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
                                sense = dev.ModeSense10(out buffer, out senseBuffer, false, true,
                                                        ScsiModeSensePageControl.Current, 0x3F, 0x00, timeout,
                                                        out duration);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.SupportsModeSense10 = true;
                                    decMode = Decoders.SCSI.Modes.DecodeMode10(buffer, dev.ScsiType);
                                    if(debug) mediaTest.ModeSense10Data = buffer;
                                }

                                DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                                sense = dev.ModeSense(out buffer, out senseBuffer, timeout, out duration);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.SupportsModeSense6 = true;
                                    if(!decMode.HasValue)
                                        decMode = Decoders.SCSI.Modes.DecodeMode6(buffer, dev.ScsiType);
                                    if(debug) mediaTest.ModeSense6Data = buffer;
                                }

                                if(decMode.HasValue)
                                {
                                    mediaTest.MediumType = (byte)decMode.Value.Header.MediumType;
                                    mediaTest.MediumTypeSpecified = true;
                                    if(decMode.Value.Header.BlockDescriptors != null &&
                                       decMode.Value.Header.BlockDescriptors.Length > 0)
                                    {
                                        mediaTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                                        mediaTest.DensitySpecified = true;
                                    }
                                }

                                mediaTest.SupportsReadSpecified = true;
                                mediaTest.SupportsRead10Specified = true;
                                mediaTest.SupportsRead12Specified = true;
                                mediaTest.SupportsRead16Specified = true;
                                mediaTest.SupportsReadLongSpecified = true;

                                DicConsole.WriteLine("Trying SCSI READ (6)...");
                                mediaTest.SupportsRead = !dev.Read6(out buffer, out senseBuffer, 0, mediaTest.BlockSize,
                                                                    timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "read6",
                                                     "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                     buffer);

                                DicConsole.WriteLine("Trying SCSI READ (10)...");
                                mediaTest.SupportsRead10 =
                                    !dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                                mediaTest.BlockSize, 0, 1, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead10);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "read10",
                                                     "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                     buffer);

                                DicConsole.WriteLine("Trying SCSI READ (12)...");
                                mediaTest.SupportsRead12 =
                                    !dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                                mediaTest.BlockSize, 0, 1, false, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead12);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "read12",
                                                     "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                     buffer);

                                DicConsole.WriteLine("Trying SCSI READ (16)...");
                                mediaTest.SupportsRead16 =
                                    !dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0,
                                                mediaTest.BlockSize, 0, 1, false, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead16);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "read16",
                                                     "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                     buffer);

                                mediaTest.LongBlockSize = mediaTest.BlockSize;
                                DicConsole.WriteLine("Trying SCSI READ LONG (10)...");
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, timeout,
                                                       out duration);
                                if(sense && !dev.Error)
                                {
                                    Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuffer);
                                    if(decSense.HasValue)
                                    {
                                        if(decSense.Value.SenseKey == Decoders.SCSI.SenseKeys.IllegalRequest &&
                                           decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            if(decSense.Value.InformationValid && decSense.Value.ILI)
                                            {
                                                mediaTest.LongBlockSize =
                                                    0xFFFF - (decSense.Value.Information & 0xFFFF);
                                                mediaTest.LongBlockSizeSpecified = true;
                                            }
                                        }
                                    }
                                }

                                if(mediaTest.SupportsReadLong && mediaTest.LongBlockSize == mediaTest.BlockSize)
                                {
                                    if(mediaTest.BlockSize == 512)
                                    {
                                        foreach(ushort testSize in new[]
                                        {
                                            // Long sector sizes for floppies
                                            514,
                                            // Long sector sizes for SuperDisk
                                            536, 558,
                                            // Long sector sizes for 512-byte magneto-opticals
                                            600, 610, 630
                                        })
                                        {
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                   testSize, timeout, out duration);
                                            if(!sense && !dev.Error)
                                            {
                                                mediaTest.SupportsReadLong = true;
                                                mediaTest.LongBlockSize = testSize;
                                                mediaTest.LongBlockSizeSpecified = true;
                                                break;
                                            }
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 1024)
                                    {
                                        foreach(ushort testSize in new[]
                                        {
                                            // Long sector sizes for floppies
                                            1026,
                                            // Long sector sizes for 1024-byte magneto-opticals
                                            1200
                                        })
                                        {
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                   testSize, timeout, out duration);
                                            if(!sense && !dev.Error)
                                            {
                                                mediaTest.SupportsReadLong = true;
                                                mediaTest.LongBlockSize = testSize;
                                                mediaTest.LongBlockSizeSpecified = true;
                                                break;
                                            }
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 2048)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380,
                                                               timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = 2380;
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 4096)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760,
                                                               timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = 4760;
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 8192)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424,
                                                               timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = 9424;
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                }

                                if(mediaTest.SupportsReadLong && mediaTest.LongBlockSize == mediaTest.BlockSize)
                                {
                                    pressedKey = new ConsoleKeyInfo();
                                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole
                                            .Write("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");
                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    if(pressedKey.Key == ConsoleKey.Y)
                                    {
                                        for(ushort i = (ushort)mediaTest.BlockSize; i <= ushort.MaxValue; i++)
                                        {
                                            DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, i,
                                                                   timeout, out duration);
                                            if(!sense)
                                            {
                                                mediaTest.LongBlockSize = i;
                                                mediaTest.LongBlockSizeSpecified = true;
                                                break;
                                            }

                                            if(i == ushort.MaxValue) break;
                                        }

                                        DicConsole.WriteLine();
                                    }
                                }

                                if(debug && mediaTest.SupportsReadLong && mediaTest.LongBlockSizeSpecified &&
                                   mediaTest.LongBlockSize != mediaTest.BlockSize)
                                {
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                           (ushort)mediaTest.LongBlockSize, timeout, out duration);
                                    if(!sense)
                                        DataFile.WriteTo("SCSI Report", "readlong10",
                                                         "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                         buffer);
                                }

                                mediaTest.CanReadMediaSerialSpecified = true;
                                DicConsole.WriteLine("Trying SCSI READ MEDIA SERIAL NUMBER...");
                                mediaTest.CanReadMediaSerial =
                                    !dev.ReadMediaSerialNumber(out buffer, out senseBuffer, timeout, out duration);
                            }

                            mediaTests.Add(mediaTest);
                        }
                    }

                    report.SCSI.RemovableMedias = mediaTests.ToArray();
                }
                else
                {
                    report.SCSI.ReadCapabilities = new testedMediaType();
                    report.SCSI.ReadCapabilitiesSpecified = true;
                    report.SCSI.ReadCapabilities.MediaIsRecognized = true;

                    report.SCSI.ReadCapabilities.SupportsReadCapacitySpecified = true;
                    report.SCSI.ReadCapabilities.SupportsReadCapacity16Specified = true;

                    DicConsole.WriteLine("Querying SCSI READ CAPACITY...");
                    sense = dev.ReadCapacity(out buffer, out senseBuffer, timeout, out duration);
                    if(!sense && !dev.Error)
                    {
                        report.SCSI.ReadCapabilities.SupportsReadCapacity = true;
                        report.SCSI.ReadCapabilities.Blocks =
                            (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) + 1;
                        report.SCSI.ReadCapabilities.BlockSize =
                            (uint)((buffer[4] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
                        report.SCSI.ReadCapabilities.BlocksSpecified = true;
                        report.SCSI.ReadCapabilities.BlockSizeSpecified = true;
                    }

                    DicConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
                    sense = dev.ReadCapacity16(out buffer, out buffer, timeout, out duration);
                    if(!sense && !dev.Error)
                    {
                        report.SCSI.ReadCapabilities.SupportsReadCapacity16 = true;
                        byte[] temp = new byte[8];
                        Array.Copy(buffer, 0, temp, 0, 8);
                        Array.Reverse(temp);
                        report.SCSI.ReadCapabilities.Blocks = BitConverter.ToUInt64(temp, 0) + 1;
                        report.SCSI.ReadCapabilities.BlockSize =
                            (uint)((buffer[8] << 24) + (buffer[9] << 16) + (buffer[10] << 8) + buffer[11]);
                        report.SCSI.ReadCapabilities.BlocksSpecified = true;
                        report.SCSI.ReadCapabilities.BlockSizeSpecified = true;
                    }

                    decMode = null;

                    DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
                    sense = dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Current,
                                            0x3F, 0x00, timeout, out duration);
                    if(!sense && !dev.Error)
                    {
                        report.SCSI.SupportsModeSense10 = true;
                        decMode = Decoders.SCSI.Modes.DecodeMode10(buffer, dev.ScsiType);
                        if(debug) report.SCSI.ReadCapabilities.ModeSense10Data = buffer;
                    }

                    DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                    sense = dev.ModeSense(out buffer, out senseBuffer, timeout, out duration);
                    if(!sense && !dev.Error)
                    {
                        report.SCSI.SupportsModeSense6 = true;
                        if(!decMode.HasValue) decMode = Decoders.SCSI.Modes.DecodeMode6(buffer, dev.ScsiType);
                        if(debug) report.SCSI.ReadCapabilities.ModeSense6Data = buffer;
                    }

                    if(decMode.HasValue)
                    {
                        report.SCSI.ReadCapabilities.MediumType = (byte)decMode.Value.Header.MediumType;
                        report.SCSI.ReadCapabilities.MediumTypeSpecified = true;
                        if(decMode.Value.Header.BlockDescriptors != null &&
                           decMode.Value.Header.BlockDescriptors.Length > 0)
                        {
                            report.SCSI.ReadCapabilities.Density =
                                (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                            report.SCSI.ReadCapabilities.DensitySpecified = true;
                        }
                    }

                    report.SCSI.ReadCapabilities.SupportsReadSpecified = true;
                    report.SCSI.ReadCapabilities.SupportsRead10Specified = true;
                    report.SCSI.ReadCapabilities.SupportsRead12Specified = true;
                    report.SCSI.ReadCapabilities.SupportsRead16Specified = true;
                    report.SCSI.ReadCapabilities.SupportsReadLongSpecified = true;

                    DicConsole.WriteLine("Trying SCSI READ (6)...");
                    report.SCSI.ReadCapabilities.SupportsRead = !dev.Read6(out buffer, out senseBuffer, 0,
                                                                           report.SCSI.ReadCapabilities.BlockSize,
                                                                           timeout, out duration);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !report.SCSI.ReadCapabilities.SupportsRead);
                    if(debug)
                        DataFile.WriteTo("SCSI Report", "read6",
                                         "_debug_" + report.SCSI.Inquiry.ProductIdentification + ".bin", "read results",
                                         buffer);

                    DicConsole.WriteLine("Trying SCSI READ (10)...");
                    report.SCSI.ReadCapabilities.SupportsRead10 =
                        !dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                    report.SCSI.ReadCapabilities.BlockSize, 0, 1, timeout, out duration);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                              !report.SCSI.ReadCapabilities.SupportsRead10);
                    if(debug)
                        DataFile.WriteTo("SCSI Report", "read10",
                                         "_debug_" + report.SCSI.Inquiry.ProductIdentification + ".bin", "read results",
                                         buffer);

                    DicConsole.WriteLine("Trying SCSI READ (12)...");
                    report.SCSI.ReadCapabilities.SupportsRead12 =
                        !dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                    report.SCSI.ReadCapabilities.BlockSize, 0, 1, false, timeout, out duration);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                              !report.SCSI.ReadCapabilities.SupportsRead12);
                    if(debug)
                        DataFile.WriteTo("SCSI Report", "read12",
                                         "_debug_" + report.SCSI.Inquiry.ProductIdentification + ".bin", "read results",
                                         buffer);

                    DicConsole.WriteLine("Trying SCSI READ (16)...");
                    report.SCSI.ReadCapabilities.SupportsRead16 =
                        !dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0,
                                    report.SCSI.ReadCapabilities.BlockSize, 0, 1, false, timeout, out duration);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                              !report.SCSI.ReadCapabilities.SupportsRead16);
                    if(debug)
                        DataFile.WriteTo("SCSI Report", "read16",
                                         "_debug_" + report.SCSI.Inquiry.ProductIdentification + ".bin", "read results",
                                         buffer);

                    report.SCSI.ReadCapabilities.LongBlockSize = report.SCSI.ReadCapabilities.BlockSize;
                    DicConsole.WriteLine("Trying SCSI READ LONG (10)...");
                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, timeout, out duration);
                    if(sense && !dev.Error)
                    {
                        Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuffer);
                        if(decSense.HasValue)
                        {
                            if(decSense.Value.SenseKey == Decoders.SCSI.SenseKeys.IllegalRequest &&
                               decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                            {
                                report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                if(decSense.Value.InformationValid && decSense.Value.ILI)
                                {
                                    report.SCSI.ReadCapabilities.LongBlockSize =
                                        0xFFFF - (decSense.Value.Information & 0xFFFF);
                                    report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                                }
                            }
                        }
                    }

                    if(report.SCSI.ReadCapabilities.SupportsReadLong && report.SCSI.ReadCapabilities.LongBlockSize ==
                       report.SCSI.ReadCapabilities.BlockSize)
                    {
                        if(report.SCSI.ReadCapabilities.BlockSize == 512)
                        {
                            foreach(ushort testSize in new[]
                            {
                                // Long sector sizes for floppies
                                514,
                                // Long sector sizes for SuperDisk
                                536, 558,
                                // Long sector sizes for 512-byte magneto-opticals
                                600, 610, 630
                            })
                            {
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, testSize, timeout,
                                                       out duration);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                    report.SCSI.ReadCapabilities.LongBlockSize = testSize;
                                    report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                                    break;
                                }
                            }
                        }
                        else if(report.SCSI.ReadCapabilities.BlockSize == 1024)
                        {
                            foreach(ushort testSize in new[]
                            {
                                // Long sector sizes for floppies
                                1026,
                                // Long sector sizes for 1024-byte magneto-opticals
                                1200
                            })
                            {
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, testSize, timeout,
                                                       out duration);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                    report.SCSI.ReadCapabilities.LongBlockSize = testSize;
                                    report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                                    break;
                                }
                            }
                        }
                        else if(report.SCSI.ReadCapabilities.BlockSize == 2048)
                        {
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380, timeout,
                                                   out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                report.SCSI.ReadCapabilities.LongBlockSize = 2380;
                                report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                            }
                        }
                        else if(report.SCSI.ReadCapabilities.BlockSize == 4096)
                        {
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760, timeout,
                                                   out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                report.SCSI.ReadCapabilities.LongBlockSize = 4760;
                                report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                            }
                        }
                        else if(report.SCSI.ReadCapabilities.BlockSize == 8192)
                        {
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424, timeout,
                                                   out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                report.SCSI.ReadCapabilities.LongBlockSize = 9424;
                                report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                            }
                        }
                    }

                    if(report.SCSI.ReadCapabilities.SupportsReadLong && report.SCSI.ReadCapabilities.LongBlockSize ==
                       report.SCSI.ReadCapabilities.BlockSize)
                    {
                        pressedKey = new ConsoleKeyInfo();
                        while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                        {
                            DicConsole
                                .Write("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");
                            pressedKey = System.Console.ReadKey();
                            DicConsole.WriteLine();
                        }

                        if(pressedKey.Key == ConsoleKey.Y)
                        {
                            for(ushort i = (ushort)report.SCSI.ReadCapabilities.BlockSize; i <= ushort.MaxValue; i++)
                            {
                                DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, i, timeout,
                                                       out duration);
                                if(!sense)
                                {
                                    if(debug)
                                    {
                                        FileStream bingo =
                                            new FileStream(string.Format("{0}_readlong.bin", dev.Model),
                                                           FileMode.Create);
                                        bingo.Write(buffer, 0, buffer.Length);
                                        bingo.Close();
                                    }
                                    report.SCSI.ReadCapabilities.LongBlockSize = i;
                                    report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                                    break;
                                }

                                if(i == ushort.MaxValue) break;
                            }

                            DicConsole.WriteLine();
                        }
                    }

                    if(debug && report.SCSI.ReadCapabilities.SupportsReadLong &&
                       report.SCSI.ReadCapabilities.LongBlockSizeSpecified &&
                       report.SCSI.ReadCapabilities.LongBlockSize != report.SCSI.ReadCapabilities.BlockSize)
                    {
                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                               (ushort)report.SCSI.ReadCapabilities.LongBlockSize, timeout,
                                               out duration);
                        if(!sense)
                            DataFile.WriteTo("SCSI Report", "readlong10",
                                             "_debug_" + report.SCSI.Inquiry.ProductIdentification + ".bin",
                                             "read results", buffer);
                    }
                }
            }
        }
    }
}