// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ListDevices.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Gets a list of known physical devices.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Devices.Linux;

using System;
using System.IO;
using System.Text;

[System.Runtime.Versioning.SupportedOSPlatform("linux")]
static class ListDevices
{
    const string PATH_SYS_DEVBLOCK = "/sys/block/";

    /// <summary>Gets a list of all known storage devices on Linux</summary>
    /// <returns>List of devices</returns>
    internal static DeviceInfo[] GetList()
    {
        string[] sysdevs = Directory.GetFileSystemEntries(PATH_SYS_DEVBLOCK, "*", SearchOption.TopDirectoryOnly);

        var  devices = new DeviceInfo[sysdevs.Length];
        bool hasUdev;

        IntPtr udev = IntPtr.Zero;

        try
        {
            udev    = Extern.udev_new();
            hasUdev = udev != IntPtr.Zero;
        }
        catch
        {
            hasUdev = false;
        }

        for(var i = 0; i < sysdevs.Length; i++)
        {
            devices[i] = new DeviceInfo
            {
                Path = "/dev/" + Path.GetFileName(sysdevs[i])
            };

            if(hasUdev)
            {
                IntPtr udevDev =
                    Extern.udev_device_new_from_subsystem_sysname(udev, "block", Path.GetFileName(sysdevs[i]));

                devices[i].Vendor = Extern.udev_device_get_property_value(udevDev, "ID_VENDOR");
                devices[i].Model  = Extern.udev_device_get_property_value(udevDev, "ID_MODEL");

                if(!string.IsNullOrEmpty(devices[i].Model))
                    devices[i].Model = devices[i].Model.Replace('_', ' ');

                devices[i].Serial = Extern.udev_device_get_property_value(udevDev, "ID_SCSI_SERIAL");

                if(string.IsNullOrEmpty(devices[i].Serial))
                    devices[i].Serial = Extern.udev_device_get_property_value(udevDev, "ID_SERIAL_SHORT");

                devices[i].Bus = Extern.udev_device_get_property_value(udevDev, "ID_BUS");
            }

            StreamReader sr;

            if(File.Exists(Path.Combine(sysdevs[i], "device/vendor")) &&
               string.IsNullOrEmpty(devices[i].Vendor))
            {
                sr                = new StreamReader(Path.Combine(sysdevs[i], "device/vendor"), Encoding.ASCII);
                devices[i].Vendor = sr.ReadLine()?.Trim();
            }
            else if(devices[i].Path.StartsWith("/dev/loop", StringComparison.CurrentCulture))
                devices[i].Vendor = "Linux";

            if(File.Exists(Path.Combine(sysdevs[i], "device/model")) &&
               (string.IsNullOrEmpty(devices[i].Model) || devices[i].Bus == "ata"))
            {
                sr               = new StreamReader(Path.Combine(sysdevs[i], "device/model"), Encoding.ASCII);
                devices[i].Model = sr.ReadLine()?.Trim();
            }
            else if(devices[i].Path.StartsWith("/dev/loop", StringComparison.CurrentCulture))
                devices[i].Model = "Linux";

            if(File.Exists(Path.Combine(sysdevs[i], "device/serial")) &&
               string.IsNullOrEmpty(devices[i].Serial))
            {
                sr                = new StreamReader(Path.Combine(sysdevs[i], "device/serial"), Encoding.ASCII);
                devices[i].Serial = sr.ReadLine()?.Trim();
            }

            if(string.IsNullOrEmpty(devices[i].Vendor) ||
               devices[i].Vendor == "ATA")
                if(devices[i].Model != null)
                {
                    string[] pieces = devices[i].Model.Split(' ');

                    if(pieces.Length > 1)
                    {
                        devices[i].Vendor = pieces[0];
                        devices[i].Model  = devices[i].Model.Substring(pieces[0].Length + 1);
                    }
                }

            // TODO: Get better device type from sysfs paths
            if(string.IsNullOrEmpty(devices[i].Bus))
            {
                if(devices[i].Path.StartsWith("/dev/loop", StringComparison.CurrentCulture))
                    devices[i].Bus = "loop";
                else if(devices[i].Path.StartsWith("/dev/nvme", StringComparison.CurrentCulture))
                    devices[i].Bus = "NVMe";
                else if(devices[i].Path.StartsWith("/dev/mmc", StringComparison.CurrentCulture))
                    devices[i].Bus = "MMC/SD";
            }
            else
                devices[i].Bus = devices[i].Bus.ToUpper();

            switch(devices[i].Bus)
            {
                case "ATA":
                case "ATAPI":
                case "SCSI":
                case "USB":
                case "PCMCIA":
                case "FireWire":
                case "MMC/SD":
                    devices[i].Supported = true;

                    break;
            }
        }

        return devices;
    }
}