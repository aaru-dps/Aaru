// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interop;
using Aaru.Devices;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Core.Logging
{
    /// <summary>Creates a dump log</summary>
    public sealed class DumpLog
    {
        readonly StreamWriter _logSw;

        /// <summary>Initializes the dump log</summary>
        /// <param name="outputFile">Output log file</param>
        /// <param name="dev">Device</param>
        /// <param name="private">Disable saving paths or serial numbers in log</param>
        public DumpLog(string outputFile, Device dev, bool @private)
        {
            if(string.IsNullOrEmpty(outputFile))
                return;

            _logSw = new StreamWriter(outputFile, true);

            _logSw.WriteLine("Start logging at {0}", DateTime.Now);

            PlatformID platId  = DetectOS.GetRealPlatformID();
            string     platVer = DetectOS.GetVersion();

            var assemblyVersion =
                Attribute.GetCustomAttribute(typeof(DumpLog).Assembly, typeof(AssemblyInformationalVersionAttribute)) as
                    AssemblyInformationalVersionAttribute;

            _logSw.WriteLine("################# System information #################");

            _logSw.WriteLine("{0} {1} ({2}-bit)", DetectOS.GetPlatformName(platId, platVer), platVer,
                             Environment.Is64BitOperatingSystem ? 64 : 32);

            if(DetectOS.IsMono)
                _logSw.WriteLine("Mono {0}", Version.GetMonoVersion());
            else if(DetectOS.IsNetCore)
                _logSw.WriteLine(".NET Core {0}", Version.GetNetCoreVersion());
            else
                _logSw.WriteLine(RuntimeInformation.FrameworkDescription);

            _logSw.WriteLine();

            _logSw.WriteLine("################# Program information ################");
            _logSw.WriteLine("Aaru {0}", assemblyVersion?.InformationalVersion);
            _logSw.WriteLine("Running in {0}-bit", Environment.Is64BitProcess ? 64 : 32);
            _logSw.WriteLine("Running as superuser: {0}", DetectOS.IsAdmin ? "Yes" : "No");
        #if DEBUG
            _logSw.WriteLine("DEBUG version");
        #endif
            if(@private)
            {
                string[] args = Environment.GetCommandLineArgs();

                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i].StartsWith("/dev", StringComparison.OrdinalIgnoreCase) ||
                       args[i].StartsWith("aaru://", StringComparison.OrdinalIgnoreCase))
                        continue;

                    try
                    {
                        args[i] = Path.GetFileName(args[i]);
                    }
                    catch
                    {
                        // Do nothing
                    }
                }

                _logSw.WriteLine("Command line: {0}", string.Join(" ", args));
            }
            else
                _logSw.WriteLine("Command line: {0}", Environment.CommandLine);

            _logSw.WriteLine();

            if(dev.IsRemote)
            {
                _logSw.WriteLine("################# Remote information #################");
                _logSw.WriteLine("Server: {0}", dev.RemoteApplication);
                _logSw.WriteLine("Version: {0}", dev.RemoteVersion);

                _logSw.WriteLine("Operating system: {0} {1}", dev.RemoteOperatingSystem,
                                 dev.RemoteOperatingSystemVersion);

                _logSw.WriteLine("Architecture: {0}", dev.RemoteArchitecture);
                _logSw.WriteLine("Protocol version: {0}", dev.RemoteProtocolVersion);
                _logSw.WriteLine("Running as superuser: {0}", dev.IsRemoteAdmin ? "Yes" : "No");
                _logSw.WriteLine("######################################################");
            }

            _logSw.WriteLine("################# Device information #################");
            _logSw.WriteLine("Manufacturer: {0}", dev.Manufacturer);
            _logSw.WriteLine("Model: {0}", dev.Model);
            _logSw.WriteLine("Firmware revision: {0}", dev.FirmwareRevision);

            if(!@private)
                _logSw.WriteLine("Serial number: {0}", dev.Serial);

            _logSw.WriteLine("Removable device: {0}", dev.IsRemovable);
            _logSw.WriteLine("Device type: {0}", dev.Type);
            _logSw.WriteLine("CompactFlash device: {0}", dev.IsCompactFlash);
            _logSw.WriteLine("PCMCIA device: {0}", dev.IsPcmcia);
            _logSw.WriteLine("USB device: {0}", dev.IsUsb);

            if(dev.IsUsb)
            {
                _logSw.WriteLine("USB manufacturer: {0}", dev.UsbManufacturerString);
                _logSw.WriteLine("USB product: {0}", dev.UsbProductString);

                if(!@private)
                    _logSw.WriteLine("USB serial: {0}", dev.UsbSerialString);

                _logSw.WriteLine("USB vendor ID: {0:X4}h", dev.UsbVendorId);
                _logSw.WriteLine("USB product ID: {0:X4}h", dev.UsbProductId);
            }

            _logSw.WriteLine("FireWire device: {0}", dev.IsFireWire);

            if(dev.IsFireWire)
            {
                _logSw.WriteLine("FireWire vendor: {0}", dev.FireWireVendorName);
                _logSw.WriteLine("FireWire model: {0}", dev.FireWireModelName);

                if(!@private)
                    _logSw.WriteLine("FireWire GUID: 0x{0:X16}", dev.FireWireGuid);

                _logSw.WriteLine("FireWire vendor ID: 0x{0:X8}", dev.FireWireVendor);
                _logSw.WriteLine("FireWire product ID: 0x{0:X8}", dev.FireWireModel);
            }

            _logSw.WriteLine("######################################################");

            _logSw.WriteLine();
            _logSw.WriteLine("################ Dumping progress log ################");
            _logSw.Flush();
        }

        /// <summary>Adds a new line to the dump log</summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments</param>
        public void WriteLine(string format, params object[] args)
        {
            if(_logSw == null)
                return;

            string text = string.Format(format, args);
            _logSw.WriteLine("{0:s} {1}", DateTime.Now, text);
            _logSw.Flush();
        }

        /// <summary>Finishes and closes the dump log</summary>
        public void Close()
        {
            _logSw?.WriteLine("######################################################");
            _logSw?.WriteLine("End logging at {0}", DateTime.Now);
            _logSw?.Close();
        }
    }
}