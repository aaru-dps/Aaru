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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Devices;

namespace DiscImageChef.Commands
{
    public static class DeviceReport
    {
        public static void doDeviceReport(DeviceReportOptions options)
        {
            DicConsole.DebugWriteLine("Device-Report command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Device-Report command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Device-Report command", "--device={0}", options.DevicePath);

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

            Metadata.DeviceReport report = new Metadata.DeviceReport();
            bool removable = false;
            string xmlFile;
            if(!string.IsNullOrWhiteSpace(dev.Manufacturer) && !string.IsNullOrWhiteSpace(dev.Revision))
                xmlFile = dev.Manufacturer + "_" + dev.Model + "_" + dev.Revision + ".xml";
            else if(!string.IsNullOrWhiteSpace(dev.Manufacturer))
                xmlFile = dev.Manufacturer + "_" + dev.Model + ".xml";
            else if(!string.IsNullOrWhiteSpace(dev.Revision))
                xmlFile = dev.Model + "_" + dev.Revision + ".xml";
            else
                xmlFile = dev.Model + ".xml";

            xmlFile = xmlFile.Replace('\\', '_').Replace('/', '_').Replace('?', '_');

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    Core.Devices.Report.ATA.Report(dev, ref report, options.Debug, ref removable);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    Core.Devices.Report.SecureDigital.Report(dev, ref report, options.Debug, ref removable);
                    break;
                case DeviceType.NVMe:
                    Core.Devices.Report.NVMe.Report(dev, ref report, options.Debug, ref removable);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    Core.Devices.Report.SCSI.General.Report(dev, ref report, options.Debug, ref removable);
                    break;
                default:
                    throw new NotSupportedException("Unknown device type.");
            }

            FileStream xmlFs = new FileStream(xmlFile, FileMode.Create);

            System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(Metadata.DeviceReport));
            xmlSer.Serialize(xmlFs, report);
            xmlFs.Close();
            Core.Statistics.AddCommand("device-report");

            if(Settings.Settings.Current.ShareReports)
            {
                Remote.SubmitReport(report);
            }
        }
    }
}

