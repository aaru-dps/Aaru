// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReport.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'device-report' verb.
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
using System.Threading;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;
using Newtonsoft.Json;
using Mmc = DiscImageChef.Core.Devices.Report.SCSI.Mmc;
using Ssc = DiscImageChef.Core.Devices.Report.SCSI.Ssc;

namespace DiscImageChef.Commands
{
    static class DeviceReport
    {
        internal static void DoDeviceReport(DeviceReportOptions options)
        {
            DicConsole.DebugWriteLine("Device-Report command", "--debug={0}",   options.Debug);
            DicConsole.DebugWriteLine("Device-Report command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Device-Report command", "--device={0}",  options.DevicePath);

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

            DeviceReportV2 report    = new DeviceReportV2();
            bool           removable = false;
            string         jsonFile;

            if(!string.IsNullOrWhiteSpace(dev.Manufacturer) && !string.IsNullOrWhiteSpace(dev.Revision))
                jsonFile = dev.Manufacturer + "_" + dev.Model + "_" + dev.Revision + ".json";
            else if(!string.IsNullOrWhiteSpace(dev.Manufacturer))
                jsonFile                                               = dev.Manufacturer + "_" + dev.Model + ".json";
            else if(!string.IsNullOrWhiteSpace(dev.Revision)) jsonFile = dev.Model + "_" + dev.Revision     + ".json";
            else jsonFile                                              = dev.Model                          + ".json";

            jsonFile = jsonFile.Replace('\\', '_').Replace('/', '_').Replace('?', '_');

            Core.Devices.Report.DeviceReport reporter = new Core.Devices.Report.DeviceReport(dev, options.Debug);

            ConsoleKeyInfo pressedKey;

            if(dev.IsUsb)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the device natively USB (in case of doubt, press Y)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key == ConsoleKey.Y)
                {
                    report.USB = reporter.UsbReport();

                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    report.USB.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
                    removable                 = report.USB.RemovableMedia;
                }
            }

            if(dev.IsFireWire)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the device natively FireWire (in case of doubt, press Y)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key != ConsoleKey.Y)
                {
                    report.FireWire = reporter.FireWireReport();

                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    report.FireWire.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
                    removable                      = report.FireWire.RemovableMedia;
                }
            }

            if(dev.IsPcmcia) report.PCMCIA = reporter.PcmciaReport();

            byte[] buffer;

            switch(dev.Type)
            {
                case DeviceType.ATA:
                {
                    DicConsole.WriteLine("Querying ATA IDENTIFY...");

                    dev.AtaIdentify(out buffer, out _, dev.Timeout, out _);

                    if(!Identify.Decode(buffer).HasValue) break;

                    report.ATA = new Ata {IdentifyDevice = Identify.Decode(buffer)};

                    if(report.ATA.IdentifyDevice == null) break;

                    if((ushort)report.ATA.IdentifyDevice?.GeneralConfiguration == 0x848A)
                    {
                        report.CompactFlash = true;
                        removable           = false;
                    }
                    else if(!removable &&
                            report.ATA.IdentifyDevice?.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit
                                                                                            .Removable) == true)
                    {
                        pressedKey = new ConsoleKeyInfo();
                        while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                        {
                            DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                            pressedKey = System.Console.ReadKey();
                            DicConsole.WriteLine();
                        }

                        removable = pressedKey.Key == ConsoleKey.Y;
                    }

                    if(removable)
                    {
                        DicConsole
                           .WriteLine("Please remove any media from the device and press any key when it is out.");
                        System.Console.ReadKey(true);
                        DicConsole.WriteLine("Querying ATA IDENTIFY...");
                        dev.AtaIdentify(out buffer, out _, dev.Timeout, out _);
                        report.ATA.IdentifyDevice = Identify.Decode(buffer);
                        if(options.Debug) report.ATA.Identify = buffer;
                        List<TestedMedia> mediaTests          = new List<TestedMedia>();

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

                            DicConsole.Write("Please write a description of the media type and press enter: ");
                            string mediumTypeName = System.Console.ReadLine();
                            DicConsole.Write("Please write the media model and press enter: ");
                            string mediumModel = System.Console.ReadLine();

                            TestedMedia mediaTest = reporter.ReportAtaMedia();
                            mediaTest.MediumTypeName = mediumTypeName;
                            mediaTest.Model          = mediumModel;

                            mediaTests.Add(mediaTest);
                        }

                        report.ATA.RemovableMedias = mediaTests.ToArray();
                    }
                    else report.ATA.ReadCapabilities = reporter.ReportAta(report.ATA.IdentifyDevice.Value);

                    break;
                }
                case DeviceType.MMC:
                    report.MultiMediaCard = reporter.MmcSdReport();
                    break;
                case DeviceType.SecureDigital:
                    report.SecureDigital = reporter.MmcSdReport();
                    break;
                case DeviceType.NVMe: throw new NotImplementedException("NVMe devices not yet supported.");
                case DeviceType.ATAPI:
                    DicConsole.WriteLine("Querying ATAPI IDENTIFY...");

                    dev.AtapiIdentify(out buffer, out _, dev.Timeout, out _);

                    if(!Identify.Decode(buffer).HasValue) return;

                    Identify.IdentifyDevice? atapiIdNullable = Identify.Decode(buffer);
                    if(atapiIdNullable != null) report.ATAPI = new Ata {IdentifyDevice = atapiIdNullable};

                    if(options.Debug) report.ATAPI.Identify = buffer;

                    goto case DeviceType.SCSI;
                case DeviceType.SCSI:
                    if(!dev.IsUsb && !dev.IsFireWire && dev.IsRemovable)
                    {
                        pressedKey = new ConsoleKeyInfo();
                        while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                        {
                            DicConsole
                               .Write("Is the media removable from the reading/writing elements (flash memories ARE NOT removable)? (Y/N): ");
                            pressedKey = System.Console.ReadKey();
                            DicConsole.WriteLine();
                        }

                        removable = pressedKey.Key == ConsoleKey.Y;
                    }

                    if(removable)
                    {
                        switch(dev.ScsiType)
                        {
                            case PeripheralDeviceTypes.MultiMediaDevice:
                                dev.AllowMediumRemoval(out buffer, dev.Timeout, out _);
                                dev.EjectTray(out buffer, dev.Timeout, out _);
                                break;
                            case PeripheralDeviceTypes.SequentialAccess:
                                dev.SpcAllowMediumRemoval(out buffer, dev.Timeout, out _);
                                DicConsole.WriteLine("Asking drive to unload tape (can take a few minutes)...");
                                dev.Unload(out buffer, dev.Timeout, out _);
                                break;
                        }

                        DicConsole
                           .WriteLine("Please remove any media from the device and press any key when it is out.");
                        System.Console.ReadKey(true);
                    }

                    report.SCSI = reporter.ReportScsiInquiry();
                    if(report.SCSI == null) break;

                    report.SCSI.EVPDPages = reporter.ReportEvpdPages();

                    Modes.ModePage_2A? cdromMode = null;

                    reporter.ReportScsiModes(ref report, ref cdromMode);

                    string productIdentification = null;
                    if(!string.IsNullOrWhiteSpace(StringHandlers.CToString(report.SCSI.Inquiry?.ProductIdentification)))
                        productIdentification =
                            StringHandlers.CToString(report.SCSI.Inquiry?.ProductIdentification).Trim();

                    switch(dev.ScsiType)
                    {
                        case PeripheralDeviceTypes.MultiMediaDevice:
                            Mmc.Report(dev, ref report, options.Debug, cdromMode, productIdentification);
                            break;
                        case PeripheralDeviceTypes.SequentialAccess:
                            Ssc.Report(dev, ref report, options.Debug);
                            break;
                        default:
                        {
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

                                    DicConsole
                                       .WriteLine("Please insert it in the drive and press any key when it is ready.");
                                    System.Console.ReadKey(true);

                                    DicConsole.Write("Please write a description of the media type and press enter: ");
                                    string mediumTypeName = System.Console.ReadLine();
                                    DicConsole.Write("Please write the media manufacturer and press enter: ");
                                    string manufacturer = System.Console.ReadLine();
                                    DicConsole.Write("Please write the media model and press enter: ");
                                    string model = System.Console.ReadLine();

                                    bool mediaIsRecognized = true;

                                    bool sense = dev.ScsiTestUnitReady(out byte[] senseBuffer, dev.Timeout, out _);
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
                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);
                                                    if(!sense) break;

                                                    leftRetries--;
                                                }

                                                mediaIsRecognized &= !sense;
                                            }
                                            else if(decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                                            {
                                                int leftRetries = 20;
                                                while(leftRetries > 0)
                                                {
                                                    DicConsole.Write("\rWaiting for drive to become ready");
                                                    Thread.Sleep(2000);
                                                    sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);
                                                    if(!sense) break;

                                                    leftRetries--;
                                                }

                                                mediaIsRecognized &= !sense;
                                            }
                                            else mediaIsRecognized = false;
                                        else mediaIsRecognized = false;
                                    }

                                    TestedMedia mediaTest = new TestedMedia();

                                    if(mediaIsRecognized)
                                    {
                                        mediaTest = reporter.ReportScsiMedia();

                                        if(mediaTest.SupportsReadLong == true &&
                                           mediaTest.LongBlockSize    == mediaTest.BlockSize)
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
                                                    DicConsole
                                                       .Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                           i, dev.Timeout, out _);
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

                                        if(options.Debug && mediaTest.SupportsReadLong == true &&
                                           mediaTest.LongBlockSize                     != mediaTest.BlockSize)
                                        {
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                                   (ushort)mediaTest.LongBlockSize, dev.Timeout, out _);
                                            if(!sense)
                                                DataFile.WriteTo("SCSI Report", "readlong10",
                                                                 "_debug_" + mediaTest.MediumTypeName + ".bin",
                                                                 "read results", buffer);
                                        }
                                    }

                                    mediaTest.MediumTypeName = mediumTypeName;
                                    mediaTest.Manufacturer   = manufacturer;
                                    mediaTest.Model          = model;

                                    mediaTests.Add(mediaTest);
                                }

                                report.SCSI.RemovableMedias = mediaTests.ToArray();
                            }
                            else
                            {
                                report.SCSI.ReadCapabilities = reporter.ReportScsi();

                                if(report.SCSI.ReadCapabilities.SupportsReadLong == true &&
                                   report.SCSI.ReadCapabilities.LongBlockSize ==
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
                                        for(ushort i = (ushort)report.SCSI.ReadCapabilities.BlockSize;; i++)
                                        {
                                            DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                            bool sense = dev.ReadLong10(out buffer, out byte[] senseBuffer, false,
                                                                        false, 0, i, dev.Timeout, out _);
                                            if(!sense)
                                            {
                                                if(options.Debug)
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

                                if(options.Debug && report.SCSI.ReadCapabilities.SupportsReadLong == true &&
                                   report.SCSI.ReadCapabilities.LongBlockSize !=
                                   report.SCSI.ReadCapabilities.BlockSize)
                                {
                                    bool sense = dev.ReadLong10(out buffer, out byte[] senseBuffer, false, false, 0,
                                                                (ushort)report.SCSI.ReadCapabilities.LongBlockSize,
                                                                dev.Timeout, out _);
                                    if(!sense)
                                        DataFile.WriteTo("SCSI Report", "readlong10", "_debug_" + dev.Model + ".bin",
                                                         "read results", buffer);
                                }
                            }

                            break;
                        }
                    }

                    break;
                default: throw new NotSupportedException("Unknown device type.");
            }

            FileStream   jsonFs = new FileStream(jsonFile, FileMode.Create);
            StreamWriter jsonSw = new StreamWriter(jsonFs);
            jsonSw.Write(JsonConvert.SerializeObject(report, Formatting.Indented,
                                                     new JsonSerializerSettings
                                                     {
                                                         NullValueHandling = NullValueHandling.Ignore
                                                     }));
            jsonSw.Close();
            jsonFs.Close();

            Core.Statistics.AddCommand("device-report");

            // TODO:
            //if(Settings.Settings.Current.ShareReports) Remote.SubmitReport(report);
        }
    }
}