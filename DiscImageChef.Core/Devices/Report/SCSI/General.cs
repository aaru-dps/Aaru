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
using System.Linq;
using System.Threading;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Report.SCSI
{
    /// <summary>
    ///     Implements creating a report of SCSI and ATAPI devices
    /// </summary>
    public static class General
    {
        /// <summary>
        ///     Creates a report of SCSI and ATAPI devices, and if appropiate calls the report creators for MultiMedia and
        ///     Streaming devices
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="report">Device report</param>
        /// <param name="debug">If debug is enabled</param>
        /// <param name="removable">If device is removable</param>
        public static void Report(Device dev, ref DeviceReportV2 report, bool debug, ref bool removable)
        {
            if(report == null) return;

            bool           sense;
            const uint     TIMEOUT = 5;
            ConsoleKeyInfo pressedKey;

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

            DicConsole.WriteLine("Querying SCSI INQUIRY...");
            sense = dev.ScsiInquiry(out byte[] buffer, out byte[] senseBuffer);

            report.SCSI = new Scsi();

            if(!sense && Inquiry.Decode(buffer).HasValue)
            {
                report.SCSI.Inquiry = Inquiry.Decode(buffer);

                if(debug) report.SCSI.InquiryData = buffer;
            }

            DicConsole.WriteLine("Querying list of SCSI EVPDs...");
            sense = dev.ScsiInquiry(out buffer, out senseBuffer, 0x00);

            if(!sense)
            {
                byte[] evpdPages = EVPD.DecodePage00(buffer);
                if(evpdPages != null && evpdPages.Length > 0)
                {
                    List<ScsiPage> evpds = new List<ScsiPage>();
                    foreach(byte page in evpdPages.Where(page => page != 0x80))
                    {
                        DicConsole.WriteLine("Querying SCSI EVPD {0:X2}h...", page);
                        sense = dev.ScsiInquiry(out buffer, out senseBuffer, page);
                        if(sense) continue;

                        ScsiPage evpd = new ScsiPage {page = page, value = buffer};
                        evpds.Add(evpd);
                    }

                    if(evpds.Count > 0) report.SCSI.EVPDPages = evpds.ToArray();
                }
            }

            if(removable)
            {
                switch(dev.ScsiType)
                {
                    case PeripheralDeviceTypes.MultiMediaDevice:
                        dev.AllowMediumRemoval(out senseBuffer, TIMEOUT, out _);
                        dev.EjectTray(out senseBuffer, TIMEOUT, out _);
                        break;
                    case PeripheralDeviceTypes.SequentialAccess:
                        dev.SpcAllowMediumRemoval(out senseBuffer, TIMEOUT, out _);
                        DicConsole.WriteLine("Asking drive to unload tape (can take a few minutes)...");
                        dev.Unload(out senseBuffer, TIMEOUT, out _);
                        break;
                }

                DicConsole.WriteLine("Please remove any media from the device and press any key when it is out.");
                System.Console.ReadKey(true);
            }

            Modes.DecodedMode?    decMode = null;
            PeripheralDeviceTypes devType = dev.ScsiType;

            DicConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (10)...");
            sense = dev.ModeSense10(out byte[] mode10Buffer, out senseBuffer, false, true,
                                    ScsiModeSensePageControl.Default, 0x3F, 0xFF, TIMEOUT, out _);
            if(sense || dev.Error)
            {
                DicConsole.WriteLine("Querying all mode pages using SCSI MODE SENSE (10)...");
                sense = dev.ModeSense10(out mode10Buffer, out senseBuffer, false, true,
                                        ScsiModeSensePageControl.Default, 0x3F, 0x00, TIMEOUT, out _);
                if(!sense && !dev.Error)
                {
                    report.SCSI.SupportsModeSense10  = true;
                    report.SCSI.SupportsModeSubpages = false;
                    decMode                          = Modes.DecodeMode10(mode10Buffer, devType);
                }
            }
            else
            {
                report.SCSI.SupportsModeSense10  = true;
                report.SCSI.SupportsModeSubpages = true;
                decMode                          = Modes.DecodeMode10(mode10Buffer, devType);
            }

            DicConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (6)...");
            sense = dev.ModeSense6(out byte[] mode6Buffer, out senseBuffer, false, ScsiModeSensePageControl.Default,
                                   0x3F, 0xFF, TIMEOUT, out _);
            if(sense || dev.Error)
            {
                DicConsole.WriteLine("Querying all mode pages using SCSI MODE SENSE (6)...");
                sense = dev.ModeSense6(out mode6Buffer, out senseBuffer, false, ScsiModeSensePageControl.Default, 0x3F,
                                       0x00, TIMEOUT, out _);
                if(sense || dev.Error)
                {
                    DicConsole.WriteLine("Querying SCSI MODE SENSE (6)...");
                    sense = dev.ModeSense(out mode6Buffer, out senseBuffer, TIMEOUT, out _);
                }
            }
            else report.SCSI.SupportsModeSubpages = true;

            if(!sense && !dev.Error && !decMode.HasValue) decMode = Modes.DecodeMode6(mode6Buffer, devType);

            report.SCSI.SupportsModeSense6 |= !sense && !dev.Error;

            Modes.ModePage_2A? cdromMode = null;

            if(debug && report.SCSI.SupportsModeSense6) report.SCSI.ModeSense6Data   = mode6Buffer;
            if(debug && report.SCSI.SupportsModeSense10) report.SCSI.ModeSense10Data = mode10Buffer;

            if(decMode.HasValue)
            {
                report.SCSI.ModeSense = new ScsiMode
                {
                    BlankCheckEnabled = decMode.Value.Header.EBC,
                    DPOandFUA         = decMode.Value.Header.DPOFUA,
                    WriteProtected    = decMode.Value.Header.WriteProtected
                };

                if(decMode.Value.Header.BufferedMode > 0)
                    report.SCSI.ModeSense.BufferedMode = decMode.Value.Header.BufferedMode;

                if(decMode.Value.Header.Speed > 0) report.SCSI.ModeSense.Speed = decMode.Value.Header.Speed;

                if(decMode.Value.Pages != null)
                {
                    List<ScsiPage> modePages = new List<ScsiPage>();
                    foreach(Modes.ModePage page in decMode.Value.Pages)
                    {
                        ScsiPage modePage = new ScsiPage
                        {
                            page = page.Page, subpage = page.Subpage, value = page.PageResponse
                        };
                        modePages.Add(modePage);

                        if(modePage.page == 0x2A && modePage.subpage == 0x00)
                            cdromMode = Modes.DecodeModePage_2A(page.PageResponse);
                    }

                    if(modePages.Count > 0) report.SCSI.ModeSense.ModePages = modePages.ToArray();
                }
            }

            string productIdentification = null;
            if(!string.IsNullOrWhiteSpace(StringHandlers.CToString(report.SCSI.Inquiry?.ProductIdentification)))
                productIdentification = StringHandlers.CToString(report.SCSI.Inquiry?.ProductIdentification).Trim();

            switch(dev.ScsiType)
            {
                case PeripheralDeviceTypes.MultiMediaDevice:
                    Mmc.Report(dev, ref report, debug, ref cdromMode, productIdentification);
                    break;
                case PeripheralDeviceTypes.SequentialAccess:
                    Ssc.Report(dev, ref report, debug);
                    break;
                default:
                    if(removable)
                    {
                        List<TestedMedia> mediaTests = new List<TestedMedia>();

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

                            if(pressedKey.Key != ConsoleKey.Y) continue;

                            DicConsole.WriteLine("Please insert it in the drive and press any key when it is ready.");
                            System.Console.ReadKey(true);

                            TestedMedia mediaTest = new TestedMedia();
                            DicConsole.Write("Please write a description of the media type and press enter: ");
                            mediaTest.MediumTypeName = System.Console.ReadLine();
                            DicConsole.Write("Please write the media manufacturer and press enter: ");
                            mediaTest.Manufacturer = System.Console.ReadLine();
                            DicConsole.Write("Please write the media model and press enter: ");
                            mediaTest.Model = System.Console.ReadLine();

                            mediaTest.MediaIsRecognized = true;

                            sense = dev.ScsiTestUnitReady(out senseBuffer, TIMEOUT, out _);
                            if(sense)
                            {
                                FixedSense? decSense = Sense.DecodeFixed(senseBuffer);
                                if(decSense.HasValue)
                                    if(decSense.Value.ASC == 0x3A)
                                    {
                                        int leftRetries = 20;
                                        while(leftRetries > 0)
                                        {
                                            DicConsole.Write("\rWaiting for drive to become ready");
                                            Thread.Sleep(2000);
                                            sense = dev.ScsiTestUnitReady(out senseBuffer, TIMEOUT, out _);
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
                                            Thread.Sleep(2000);
                                            sense = dev.ScsiTestUnitReady(out senseBuffer, TIMEOUT, out _);
                                            if(!sense) break;

                                            leftRetries--;
                                        }

                                        mediaTest.MediaIsRecognized &= !sense;
                                    }
                                    else mediaTest.MediaIsRecognized = false;
                                else mediaTest.MediaIsRecognized = false;
                            }

                            if(mediaTest.MediaIsRecognized)
                            {
                                DicConsole.WriteLine("Querying SCSI READ CAPACITY...");
                                sense = dev.ReadCapacity(out buffer, out senseBuffer, TIMEOUT, out _);
                                if(!sense && !dev.Error)
                                {
                                    mediaTest.SupportsReadCapacity = true;
                                    mediaTest.Blocks =
                                        (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) +
                                        1;
                                    mediaTest.BlockSize =
                                        (uint)((buffer[4] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
                                }

                                DicConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
                                sense = dev.ReadCapacity16(out buffer, out buffer, TIMEOUT, out _);
                                if(!sense && !dev.Error)
                                {
                                    mediaTest.SupportsReadCapacity16 = true;
                                    byte[] temp = new byte[8];
                                    Array.Copy(buffer, 0, temp, 0, 8);
                                    Array.Reverse(temp);
                                    mediaTest.Blocks = BitConverter.ToUInt64(temp, 0) + 1;
                                    mediaTest.BlockSize =
                                        (uint)((buffer[8] << 24) + (buffer[9] << 16) + (buffer[10] << 8) + buffer[11]);
                                }

                                decMode = null;

                                DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
                                sense = dev.ModeSense10(out buffer, out senseBuffer, false, true,
                                                        ScsiModeSensePageControl.Current, 0x3F, 0x00, TIMEOUT, out _);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.SupportsModeSense10 = true;
                                    decMode                         = Modes.DecodeMode10(buffer, dev.ScsiType);
                                    if(debug) mediaTest.ModeSense10Data = buffer;
                                }

                                DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                                sense = dev.ModeSense(out buffer, out senseBuffer, TIMEOUT, out _);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.SupportsModeSense6 = true;
                                    if(!decMode.HasValue) decMode      = Modes.DecodeMode6(buffer, dev.ScsiType);
                                    if(debug) mediaTest.ModeSense6Data = buffer;
                                }

                                if(decMode.HasValue)
                                {
                                    mediaTest.MediumType = (byte)decMode.Value.Header.MediumType;
                                    if(decMode.Value.Header.BlockDescriptors        != null &&
                                       decMode.Value.Header.BlockDescriptors.Length > 0)
                                        mediaTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                                }

                                DicConsole.WriteLine("Trying SCSI READ (6)...");
                                mediaTest.SupportsRead6 = !dev.Read6(out buffer, out senseBuffer, 0,
                                                                     mediaTest.BlockSize ?? 512, TIMEOUT, out _);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead6);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "read6",
                                                     "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                     buffer);

                                DicConsole.WriteLine("Trying SCSI READ (10)...");
                                mediaTest.SupportsRead10 =
                                    !dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                                mediaTest.BlockSize ?? 512, 0, 1, TIMEOUT, out _);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead10);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "read10",
                                                     "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                     buffer);

                                DicConsole.WriteLine("Trying SCSI READ (12)...");
                                mediaTest.SupportsRead12 =
                                    !dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                                mediaTest.BlockSize ?? 512, 0, 1, false, TIMEOUT, out _);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead12);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "read12",
                                                     "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                     buffer);

                                DicConsole.WriteLine("Trying SCSI READ (16)...");
                                mediaTest.SupportsRead16 =
                                    !dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0,
                                                mediaTest.BlockSize ?? 512, 0, 1, false, TIMEOUT, out _);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead16);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "read16",
                                                     "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                     buffer);

                                mediaTest.LongBlockSize = mediaTest.BlockSize;
                                DicConsole.WriteLine("Trying SCSI READ LONG (10)...");
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, TIMEOUT,
                                                       out _);
                                if(sense && !dev.Error)
                                {
                                    FixedSense? decSense = Sense.DecodeFixed(senseBuffer);
                                    if(decSense.HasValue)
                                        if(decSense.Value.SenseKey == SenseKeys.IllegalRequest &&
                                           decSense.Value.ASC      == 0x24                     &&
                                           decSense.Value.ASCQ     == 0x00)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            if(decSense.Value.InformationValid && decSense.Value.ILI)
                                                mediaTest.LongBlockSize =
                                                    0xFFFF - (decSense.Value.Information & 0xFFFF);
                                        }
                                }

                                if(mediaTest.SupportsReadLong == true && mediaTest.LongBlockSize == mediaTest.BlockSize)
                                    if(mediaTest.BlockSize == 512)
                                        foreach(int i in new[]
                                        {
                                            // Long sector sizes for floppies
                                            514,
                                            // Long sector sizes for SuperDisk
                                            536, 558,
                                            // Long sector sizes for 512-byte magneto-opticals
                                            600, 610, 630
                                        })
                                        {
                                            ushort testSize = (ushort)i;
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                   testSize, TIMEOUT, out _);
                                            if(sense || dev.Error) continue;

                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize    = testSize;
                                            break;
                                        }
                                    else if(mediaTest.BlockSize == 1024)
                                        foreach(int i in new[]
                                        {
                                            // Long sector sizes for floppies
                                            1026,
                                            // Long sector sizes for 1024-byte magneto-opticals
                                            1200
                                        })
                                        {
                                            ushort testSize = (ushort)i;
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                   (ushort)i, TIMEOUT, out _);
                                            if(sense || dev.Error) continue;

                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize    = testSize;
                                            break;
                                        }
                                    else if(mediaTest.BlockSize == 2048)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380,
                                                               TIMEOUT, out _);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize    = 2380;
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 4096)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760,
                                                               TIMEOUT, out _);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize    = 4760;
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 8192)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424,
                                                               TIMEOUT, out _);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize    = 9424;
                                        }
                                    }

                                if(mediaTest.SupportsReadLong == true && mediaTest.LongBlockSize == mediaTest.BlockSize)
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
                                        for(ushort i = (ushort)mediaTest.BlockSize;; i++)
                                        {
                                            DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, i,
                                                                   TIMEOUT, out _);
                                            if(!sense)
                                            {
                                                mediaTest.LongBlockSize = i;
                                                break;
                                            }

                                            if(i == ushort.MaxValue) break;
                                        }

                                        DicConsole.WriteLine();
                                    }
                                }

                                if(debug && mediaTest.SupportsReadLong == true &&
                                   mediaTest.LongBlockSize             != mediaTest.BlockSize)
                                {
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                           (ushort)mediaTest.LongBlockSize, TIMEOUT, out _);
                                    if(!sense)
                                        DataFile.WriteTo("SCSI Report", "readlong10",
                                                         "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                                         buffer);
                                }

                                DicConsole.WriteLine("Trying SCSI READ MEDIA SERIAL NUMBER...");
                                mediaTest.CanReadMediaSerial =
                                    !dev.ReadMediaSerialNumber(out buffer, out senseBuffer, TIMEOUT, out _);
                            }

                            mediaTests.Add(mediaTest);
                        }

                        report.SCSI.RemovableMedias = mediaTests.ToArray();
                    }
                    else
                    {
                        report.SCSI.ReadCapabilities                   = new TestedMedia();
                        report.SCSI.ReadCapabilities.MediaIsRecognized = true;

                        DicConsole.WriteLine("Querying SCSI READ CAPACITY...");
                        sense = dev.ReadCapacity(out buffer, out senseBuffer, TIMEOUT, out _);
                        if(!sense && !dev.Error)
                        {
                            report.SCSI.ReadCapabilities.SupportsReadCapacity = true;
                            report.SCSI.ReadCapabilities.Blocks =
                                (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) + 1;
                            report.SCSI.ReadCapabilities.BlockSize =
                                (uint)((buffer[4] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
                        }

                        DicConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
                        sense = dev.ReadCapacity16(out buffer, out buffer, TIMEOUT, out _);
                        if(!sense && !dev.Error)
                        {
                            report.SCSI.ReadCapabilities.SupportsReadCapacity16 = true;
                            byte[] temp = new byte[8];
                            Array.Copy(buffer, 0, temp, 0, 8);
                            Array.Reverse(temp);
                            report.SCSI.ReadCapabilities.Blocks = BitConverter.ToUInt64(temp, 0) + 1;
                            report.SCSI.ReadCapabilities.BlockSize =
                                (uint)((buffer[8] << 24) + (buffer[9] << 16) + (buffer[10] << 8) + buffer[11]);
                        }

                        decMode = null;

                        DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
                        sense = dev.ModeSense10(out buffer, out senseBuffer, false, true,
                                                ScsiModeSensePageControl.Current, 0x3F, 0x00, TIMEOUT, out _);
                        if(!sense && !dev.Error)
                        {
                            report.SCSI.SupportsModeSense10 = true;
                            decMode                         = Modes.DecodeMode10(buffer, dev.ScsiType);
                            if(debug) report.SCSI.ReadCapabilities.ModeSense10Data = buffer;
                        }

                        DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                        sense = dev.ModeSense(out buffer, out senseBuffer, TIMEOUT, out _);
                        if(!sense && !dev.Error)
                        {
                            report.SCSI.SupportsModeSense6 = true;
                            if(!decMode.HasValue)
                                decMode = Modes.DecodeMode6(buffer, dev.ScsiType);
                            if(debug) report.SCSI.ReadCapabilities.ModeSense6Data = buffer;
                        }

                        if(decMode.HasValue)
                        {
                            report.SCSI.ReadCapabilities.MediumType = (byte)decMode.Value.Header.MediumType;
                            if(decMode.Value.Header.BlockDescriptors        != null &&
                               decMode.Value.Header.BlockDescriptors.Length > 0)
                                report.SCSI.ReadCapabilities.Density =
                                    (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                        }

                        DicConsole.WriteLine("Trying SCSI READ (6)...");
                        report.SCSI.ReadCapabilities.SupportsRead6 =
                            !dev.Read6(out buffer, out senseBuffer, 0, report.SCSI.ReadCapabilities.BlockSize ?? 512,
                                       TIMEOUT, out _);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                  !report.SCSI.ReadCapabilities.SupportsRead6);
                        if(debug)
                            DataFile.WriteTo("SCSI Report", "read6", "_debug_" + productIdentification + ".bin",
                                             "read results", buffer);

                        DicConsole.WriteLine("Trying SCSI READ (10)...");
                        report.SCSI.ReadCapabilities.SupportsRead10 =
                            !dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                        report.SCSI.ReadCapabilities.BlockSize ?? 512, 0, 1, TIMEOUT, out _);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                  !report.SCSI.ReadCapabilities.SupportsRead10);
                        if(debug)
                            DataFile.WriteTo("SCSI Report", "read10", "_debug_" + productIdentification + ".bin",
                                             "read results", buffer);

                        DicConsole.WriteLine("Trying SCSI READ (12)...");
                        report.SCSI.ReadCapabilities.SupportsRead12 =
                            !dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                        report.SCSI.ReadCapabilities.BlockSize ?? 512, 0, 1, false, TIMEOUT, out _);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                  !report.SCSI.ReadCapabilities.SupportsRead12);
                        if(debug)
                            DataFile.WriteTo("SCSI Report", "read12", "_debug_" + productIdentification + ".bin",
                                             "read results", buffer);

                        DicConsole.WriteLine("Trying SCSI READ (16)...");
                        report.SCSI.ReadCapabilities.SupportsRead16 =
                            !dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0,
                                        report.SCSI.ReadCapabilities.BlockSize ?? 512, 0, 1, false, TIMEOUT, out _);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                  !report.SCSI.ReadCapabilities.SupportsRead16);
                        if(debug)
                            DataFile.WriteTo("SCSI Report", "read16", "_debug_" + productIdentification + ".bin",
                                             "read results", buffer);

                        report.SCSI.ReadCapabilities.LongBlockSize = report.SCSI.ReadCapabilities.BlockSize;
                        DicConsole.WriteLine("Trying SCSI READ LONG (10)...");
                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, TIMEOUT, out _);
                        if(sense && !dev.Error)
                        {
                            FixedSense? decSense = Sense.DecodeFixed(senseBuffer);
                            if(decSense.HasValue)
                                if(decSense.Value.SenseKey == SenseKeys.IllegalRequest && decSense.Value.ASC == 0x24 &&
                                   decSense.Value.ASCQ     == 0x00)
                                {
                                    report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                    if(decSense.Value.InformationValid && decSense.Value.ILI)
                                        report.SCSI.ReadCapabilities.LongBlockSize =
                                            0xFFFF - (decSense.Value.Information & 0xFFFF);
                                }
                        }

                        if(report.SCSI.ReadCapabilities.SupportsReadLong == true &&
                           report.SCSI.ReadCapabilities.LongBlockSize    == report.SCSI.ReadCapabilities.BlockSize)
                            if(report.SCSI.ReadCapabilities.BlockSize == 512)
                                foreach(int i in new[]
                                {
                                    // Long sector sizes for floppies
                                    514,
                                    // Long sector sizes for SuperDisk
                                    536, 558,
                                    // Long sector sizes for 512-byte magneto-opticals
                                    600, 610, 630
                                })
                                {
                                    ushort testSize = (ushort)i;
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, (ushort)i,
                                                           TIMEOUT, out _);
                                    if(sense || dev.Error) continue;

                                    report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                    report.SCSI.ReadCapabilities.LongBlockSize    = testSize;
                                    break;
                                }
                            else if(report.SCSI.ReadCapabilities.BlockSize == 1024)
                                foreach(int i in new[]
                                {
                                    // Long sector sizes for floppies
                                    1026,
                                    // Long sector sizes for 1024-byte magneto-opticals
                                    1200
                                })
                                {
                                    ushort testSize = (ushort)i;
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, (ushort)i,
                                                           TIMEOUT, out _);
                                    if(sense || dev.Error) continue;

                                    report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                    report.SCSI.ReadCapabilities.LongBlockSize    = testSize;
                                    break;
                                }
                            else if(report.SCSI.ReadCapabilities.BlockSize == 2048)
                            {
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380, TIMEOUT,
                                                       out _);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                    report.SCSI.ReadCapabilities.LongBlockSize    = 2380;
                                }
                            }
                            else if(report.SCSI.ReadCapabilities.BlockSize == 4096)
                            {
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760, TIMEOUT,
                                                       out _);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                    report.SCSI.ReadCapabilities.LongBlockSize    = 4760;
                                }
                            }
                            else if(report.SCSI.ReadCapabilities.BlockSize == 8192)
                            {
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424, TIMEOUT,
                                                       out _);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                    report.SCSI.ReadCapabilities.LongBlockSize    = 9424;
                                }
                            }

                        if(report.SCSI.ReadCapabilities.SupportsReadLong == true &&
                           report.SCSI.ReadCapabilities.LongBlockSize    == report.SCSI.ReadCapabilities.BlockSize)
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
                                for(ushort i = (ushort)report.SCSI.ReadCapabilities.BlockSize;; i++)
                                {
                                    DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, i, TIMEOUT,
                                                           out _);
                                    if(!sense)
                                    {
                                        if(debug)
                                        {
                                            FileStream bingo =
                                                new FileStream($"{dev.Model}_readlong.bin", FileMode.Create);
                                            bingo.Write(buffer, 0, buffer.Length);
                                            bingo.Close();
                                        }

                                        report.SCSI.ReadCapabilities.LongBlockSize = i;
                                        break;
                                    }

                                    if(i == ushort.MaxValue) break;
                                }

                                DicConsole.WriteLine();
                            }
                        }

                        if(debug && report.SCSI.ReadCapabilities.SupportsReadLong == true &&
                           report.SCSI.ReadCapabilities.LongBlockSize !=
                           report.SCSI.ReadCapabilities.BlockSize)
                        {
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                   (ushort)report.SCSI.ReadCapabilities.LongBlockSize, TIMEOUT, out _);
                            if(!sense)
                                DataFile.WriteTo("SCSI Report", "readlong10",
                                                 "_debug_" + productIdentification + ".bin", "read results", buffer);
                        }
                    }

                    break;
            }
        }
    }
}