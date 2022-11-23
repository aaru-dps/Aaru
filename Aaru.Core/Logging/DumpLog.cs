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

namespace Aaru.Core.Logging;

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

        _logSw.WriteLine(Localization.Core.Start_logging_at_0, DateTime.Now);

        PlatformID platId  = DetectOS.GetRealPlatformID();
        string     platVer = DetectOS.GetVersion();

        var assemblyVersion =
            Attribute.GetCustomAttribute(typeof(DumpLog).Assembly, typeof(AssemblyInformationalVersionAttribute)) as
                AssemblyInformationalVersionAttribute;

        _logSw.WriteLine(Localization.Core.System_information);

        _logSw.WriteLine("{0} {1} ({2}-bit)", DetectOS.GetPlatformName(platId, platVer), platVer,
                         Environment.Is64BitOperatingSystem ? 64 : 32);

        if(DetectOS.IsMono)
            _logSw.WriteLine("Mono {0}", Version.GetMonoVersion());
        else if(DetectOS.IsNetCore)
            _logSw.WriteLine(".NET Core {0}", Version.GetNetCoreVersion());
        else
            _logSw.WriteLine(RuntimeInformation.FrameworkDescription);

        _logSw.WriteLine();

        _logSw.WriteLine(Localization.Core.Program_information);
        _logSw.WriteLine("Aaru {0}", assemblyVersion?.InformationalVersion);
        _logSw.WriteLine(Localization.Core.Running_in_0_bit, Environment.Is64BitProcess ? 64 : 32);

        _logSw.WriteLine(DetectOS.IsAdmin ? Localization.Core.Running_as_superuser_Yes
                             : Localization.Core.Running_as_superuser_No);
    #if DEBUG
        _logSw.WriteLine(Localization.Core.DEBUG_version);
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

            _logSw.WriteLine(Localization.Core.Command_line_0, string.Join(" ", args));
        }
        else
            _logSw.WriteLine(Localization.Core.Command_line_0, Environment.CommandLine);

        _logSw.WriteLine();

        if(dev is Aaru.Devices.Remote.Device remoteDev)
        {
            _logSw.WriteLine(Localization.Core.Remote_information);
            _logSw.WriteLine(Localization.Core.Server_0, remoteDev.RemoteApplication);
            _logSw.WriteLine(Localization.Core.Version_0, remoteDev.RemoteVersion);

            _logSw.WriteLine(Localization.Core.Operating_system_0_1, remoteDev.RemoteOperatingSystem,
                             remoteDev.RemoteOperatingSystemVersion);

            _logSw.WriteLine(Localization.Core.Architecture_0, remoteDev.RemoteArchitecture);
            _logSw.WriteLine(Localization.Core.Protocol_version_0, remoteDev.RemoteProtocolVersion);

            _logSw.WriteLine(DetectOS.IsAdmin ? Localization.Core.Running_as_superuser_Yes
                                 : Localization.Core.Running_as_superuser_No);

            _logSw.WriteLine(Localization.Core.Log_section_separator);
        }

        _logSw.WriteLine(Localization.Core.Device_information);
        _logSw.WriteLine(Localization.Core.Manufacturer_0, dev.Manufacturer);
        _logSw.WriteLine(Localization.Core.Model_0, dev.Model);
        _logSw.WriteLine(Localization.Core.Firmware_revision_0, dev.FirmwareRevision);

        if(!@private)
            _logSw.WriteLine(Localization.Core.Serial_number_0, dev.Serial);

        _logSw.WriteLine(Localization.Core.Removable_device_0, dev.IsRemovable);
        _logSw.WriteLine(Localization.Core.Device_type_0, dev.Type);
        _logSw.WriteLine(Localization.Core.CompactFlash_device_0, dev.IsCompactFlash);
        _logSw.WriteLine(Localization.Core.PCMCIA_device_0, dev.IsPcmcia);
        _logSw.WriteLine(Localization.Core.USB_device_0, dev.IsUsb);

        if(dev.IsUsb)
        {
            _logSw.WriteLine(Localization.Core.USB_manufacturer_0, dev.UsbManufacturerString);
            _logSw.WriteLine(Localization.Core.USB_product_0, dev.UsbProductString);

            if(!@private)
                _logSw.WriteLine(Localization.Core.USB_serial_0, dev.UsbSerialString);

            _logSw.WriteLine(Localization.Core.USB_vendor_ID_0, dev.UsbVendorId);
            _logSw.WriteLine(Localization.Core.USB_product_ID_0, dev.UsbProductId);
        }

        _logSw.WriteLine(Localization.Core.FireWire_device_0, dev.IsFireWire);

        if(dev.IsFireWire)
        {
            _logSw.WriteLine(Localization.Core.FireWire_vendor_0, dev.FireWireVendorName);
            _logSw.WriteLine(Localization.Core.FireWire_model_0, dev.FireWireModelName);

            if(!@private)
                _logSw.WriteLine(Localization.Core.FireWire_GUID_0, dev.FireWireGuid);

            _logSw.WriteLine(Localization.Core.FireWire_vendor_ID_0, dev.FireWireVendor);
            _logSw.WriteLine(Localization.Core.FireWire_product_ID_0, dev.FireWireModel);
        }

        _logSw.WriteLine(Localization.Core.Log_section_separator);

        _logSw.WriteLine();
        _logSw.WriteLine(Localization.Core.Dumping_progress_log);
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
        _logSw?.WriteLine(Localization.Core.Log_section_separator);
        _logSw?.WriteLine(Localization.Core.End_logging_at_0, DateTime.Now);
        _logSw?.Close();
    }
}