﻿// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ListDevices.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains code to list available devices on Linux.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.IO;
using System.Text;

namespace DiscImageChef.Devices.Linux
{
    public static class ListDevices
    {
        const string PATH_SYS_DEVBLOCK = "/sys/block/";

        public static DeviceInfo[] GetList()
        {
            string[] sysdevs = Directory.GetFileSystemEntries(PATH_SYS_DEVBLOCK, "*", SearchOption.TopDirectoryOnly);
            DeviceInfo[] devices = new DeviceInfo[sysdevs.Length];
            bool hasUdev;

            StreamReader sr;
            IntPtr udev = IntPtr.Zero;
            IntPtr udevDev = IntPtr.Zero;

            try
            {
                udev = Extern.udev_new();
                hasUdev = udev != IntPtr.Zero;
            }
            catch
            {
                hasUdev = false;
            }

            for(int i = 0; i < sysdevs.Length; i++)
            {
                devices[i] = new DeviceInfo();
                devices[i].path = "/dev/" + Path.GetFileName(sysdevs[i]);

                if(hasUdev)
                {
                    udevDev = Extern.udev_device_new_from_subsystem_sysname(udev, "block", Path.GetFileName(sysdevs[i]));
                    devices[i].vendor = Extern.udev_device_get_property_value(udevDev, "ID_VENDOR");
                    devices[i].model = Extern.udev_device_get_property_value(udevDev, "ID_MODEL");
                    if(!string.IsNullOrEmpty(devices[i].model))
                        devices[i].model = devices[i].model.Replace('_', ' ');
                    devices[i].serial = Extern.udev_device_get_property_value(udevDev, "ID_SCSI_SERIAL");
                    if(string.IsNullOrEmpty(devices[i].serial))
                        devices[i].serial = Extern.udev_device_get_property_value(udevDev, "ID_SERIAL_SHORT");
                    devices[i].bus = Extern.udev_device_get_property_value(udevDev, "ID_BUS");
                }

                if(File.Exists(Path.Combine(sysdevs[i], "device/vendor")) && string.IsNullOrEmpty(devices[i].vendor))
                {
                    sr = new StreamReader(Path.Combine(sysdevs[i], "device/vendor"), Encoding.ASCII);
                    devices[i].vendor = sr.ReadLine().Trim();
                }
                else if(devices[i].path.StartsWith("/dev/loop", StringComparison.CurrentCulture))
                    devices[i].vendor = "Linux";

                if(File.Exists(Path.Combine(sysdevs[i], "device/model")) && (string.IsNullOrEmpty(devices[i].model) || devices[i].bus == "ata"))
                {
                    sr = new StreamReader(Path.Combine(sysdevs[i], "device/model"), Encoding.ASCII);
                    devices[i].model = sr.ReadLine().Trim();
                }
                else if(devices[i].path.StartsWith("/dev/loop", StringComparison.CurrentCulture))
                    devices[i].model = "Linux";

                if(File.Exists(Path.Combine(sysdevs[i], "device/serial")) && string.IsNullOrEmpty(devices[i].serial))
                {
                    sr = new StreamReader(Path.Combine(sysdevs[i], "device/serial"), Encoding.ASCII);
                    devices[i].serial = sr.ReadLine().Trim();
                }

                if(string.IsNullOrEmpty(devices[i].vendor) || devices[i].vendor == "ATA")
                {
                    if(devices[i].model != null)
                    {
                        string[] pieces = devices[i].model.Split(' ');
                        if(pieces.Length > 1)
                        {
                            devices[i].vendor = pieces[0];
                            devices[i].model = devices[i].model.Substring(pieces[0].Length + 1);
                        }
                    }
                }

                // TODO: Get better device type from sysfs paths
                if(string.IsNullOrEmpty(devices[i].bus))
                {
                    if(devices[i].path.StartsWith("/dev/loop", StringComparison.CurrentCulture))
                        devices[i].bus = "loop";
                    else if(devices[i].path.StartsWith("/dev/nvme", StringComparison.CurrentCulture))
                        devices[i].bus = "NVMe";
                    else if(devices[i].path.StartsWith("/dev/mmc", StringComparison.CurrentCulture))
                        devices[i].bus = "MMC/SD";
                }
                else
                    devices[i].bus = devices[i].bus.ToUpper();

                switch(devices[i].bus)
                {
                    case "ATA":
                    case "ATAPI":
                    case "SCSI":
                    case "USB":
                    case "PCMCIA":
                    case "FireWire":
                    case "MMC/SD":
                        devices[i].supported = true;
                        break;
                }
            }

            return devices;
        }
    }
}
