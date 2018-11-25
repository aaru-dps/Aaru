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
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Core.Devices.Report.SCSI;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Devices;
using Newtonsoft.Json;

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

            switch(dev.Type)
            {
                case DeviceType.ATA:
                {
                    DicConsole.WriteLine("Querying ATA IDENTIFY...");

                    dev.AtaIdentify(out byte[] buffer, out _, dev.Timeout, out _);

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
                case DeviceType.NVMe:
                    throw new NotImplementedException("NVMe devices not yet supported.");
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    General.Report(dev, ref report, options.Debug, ref removable);
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