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
using System.IO;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Core.Devices.Report;
using DiscImageChef.Core.Devices.Report.SCSI;
using DiscImageChef.Devices;
using Newtonsoft.Json;
using Ata = DiscImageChef.Core.Devices.Report.Ata;

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

            if(dev.IsUsb)
            {
                ConsoleKeyInfo pressedKey = new ConsoleKeyInfo();
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

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    Ata.Report(dev, ref report, options.Debug, ref removable);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    SecureDigital.Report(dev, ref report);
                    break;
                case DeviceType.NVMe:
                    Nvme.Report(dev, ref report, options.Debug, ref removable);
                    break;
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