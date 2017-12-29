// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DumpLog.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains glue logic for writing a dump log.
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
using System.Reflection;
using DiscImageChef.Devices;
using DiscImageChef.Interop;
using PlatformID = DiscImageChef.Interop.PlatformID;
using Version = DiscImageChef.Interop.Version;

namespace DiscImageChef.Core.Logging
{
    /// <summary>
    ///     Creates a dump log
    /// </summary>
    public class DumpLog
    {
        readonly StreamWriter logSw;

        /// <summary>
        ///     Initializes the dump log
        /// </summary>
        /// <param name="outputFile">Output log file</param>
        /// <param name="dev">Device</param>
        public DumpLog(string outputFile, Device dev)
        {
            if(string.IsNullOrEmpty(outputFile)) return;

            logSw = new StreamWriter(outputFile, true);

            logSw.WriteLine("Start logging at {0}", DateTime.Now);

            PlatformID platId      = DetectOS.GetRealPlatformID();
            string     platVer     = DetectOS.GetVersion();
            Type       monoRunType = Type.GetType("Mono.Runtime");

            logSw.WriteLine("################# System information #################");
            logSw.WriteLine("{0} {1} ({2}-bit)", DetectOS.GetPlatformName(platId, platVer), platVer,
                            Environment.Is64BitOperatingSystem ? 64 : 32);
            if(monoRunType != null)
            {
                string     monoVer         = "unknown version";
                MethodInfo monoDisplayName =
                    monoRunType.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if(monoDisplayName != null) monoVer = (string)monoDisplayName.Invoke(null, null);
                logSw.WriteLine("Mono {0}", monoVer);
            }
            else logSw.WriteLine(".NET Framework {0}", Environment.Version);

            logSw.WriteLine();

            logSw.WriteLine("################# Program information ################");
            logSw.WriteLine("DiscImageChef {0} running in {1}-bit", Version.GetVersion(),
                            Environment.Is64BitProcess ? 64 : 32);
            #if DEBUG
            logSw.WriteLine("DEBUG version");
            #endif
            logSw.WriteLine("Command line: {0}", Environment.CommandLine);
            logSw.WriteLine();

            logSw.WriteLine("################# Device information #################");
            logSw.WriteLine("Manufacturer: {0}",        dev.Manufacturer);
            logSw.WriteLine("Model: {0}",               dev.Model);
            logSw.WriteLine("Firmware revision: {0}",   dev.Revision);
            logSw.WriteLine("Serial number: {0}",       dev.Serial);
            logSw.WriteLine("Removable device: {0}",    dev.IsRemovable);
            logSw.WriteLine("Device type: {0}",         dev.Type);
            logSw.WriteLine("CompactFlash device: {0}", dev.IsCompactFlash);
            logSw.WriteLine("PCMCIA device: {0}",       dev.IsPcmcia);
            logSw.WriteLine("USB device: {0}",          dev.IsUsb);
            if(dev.IsUsb)
            {
                logSw.WriteLine("USB manufacturer: {0}",   dev.UsbManufacturerString);
                logSw.WriteLine("USB product: {0}",        dev.UsbProductString);
                logSw.WriteLine("USB serial: {0}",         dev.UsbSerialString);
                logSw.WriteLine("USB vendor ID: {0:X4}h",  dev.UsbVendorId);
                logSw.WriteLine("USB product ID: {0:X4}h", dev.UsbProductId);
            }

            logSw.WriteLine("FireWire device: {0}", dev.IsFireWire);
            if(dev.IsFireWire)
            {
                logSw.WriteLine("FireWire vendor: {0}",          dev.FireWireVendorName);
                logSw.WriteLine("FireWire model: {0}",           dev.FireWireModelName);
                logSw.WriteLine("FireWire GUID: 0x{0:X16}",      dev.FireWireGuid);
                logSw.WriteLine("FireWire vendor ID: 0x{0:X8}",  dev.FireWireVendor);
                logSw.WriteLine("FireWire product ID: 0x{0:X8}", dev.FireWireModel);
            }

            logSw.WriteLine();
            logSw.WriteLine("######################################################");

            logSw.WriteLine();
            logSw.WriteLine("################ Dumping progress log ################");
            logSw.Flush();
        }

        /// <summary>
        ///     Adds a new line to the dump log
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments</param>
        public void WriteLine(string format, params object[] args)
        {
            if(logSw == null) return;

            string text = string.Format(format, args);
            logSw.WriteLine("{0:s} {1}", DateTime.Now, text);
            logSw.Flush();
        }

        /// <summary>
        ///     Finishes and closes the dump log
        /// </summary>
        public void Close()
        {
            logSw?.WriteLine("######################################################");
            logSw?.WriteLine("End logging at {0}", DateTime.Now);
            logSw?.Close();
        }
    }
}