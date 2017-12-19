// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ListDevices.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Windows direct device access.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace DiscImageChef.Devices.Windows
{
    public static class ListDevices
    {
        public static DeviceInfo[] GetList()
        {
            List<string> DeviceIDs = new List<string>();

            try
            {
                ManagementObjectSearcher mgmtObjSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                ManagementObjectCollection objCol = mgmtObjSearcher.Get();

                foreach (ManagementObject drive in objCol)
                    DeviceIDs.Add((string)drive["DeviceID"]);

                mgmtObjSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_TapeDrive");
                objCol = mgmtObjSearcher.Get();

                foreach (ManagementObject drive in objCol)
                    DeviceIDs.Add((string)drive["DeviceID"]);

                mgmtObjSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_CDROMDrive");
                objCol = mgmtObjSearcher.Get();

                foreach (ManagementObject drive in objCol)
                    DeviceIDs.Add((string)drive["Drive"]);
            }
            catch(Exception ex)
            {
#if DEBUG
                throw ex;
#else
                return null;
#endif
            }

            List<DeviceInfo> dev_list = new List<DeviceInfo>();

            foreach (string devId in DeviceIDs)
            {
                string physId = devId;
                // TODO: This can be done better
                if (devId.Length == 2 && devId[1] == ':')
                    physId = "\\\\?\\" + devId;
                SafeFileHandle fd = Extern.CreateFile(physId, 0, FileShare.Read | FileShare.Write, IntPtr.Zero, FileMode.OpenExisting, 0, IntPtr.Zero);
                if (fd.IsInvalid)
                    continue;

                StoragePropertyQuery query = new StoragePropertyQuery();
                query.PropertyId = StoragePropertyId.Device;
                query.QueryType = StorageQueryType.Standard;
                query.AdditionalParameters = new byte[1];

                //StorageDeviceDescriptor descriptor = new StorageDeviceDescriptor();
                //descriptor.RawDeviceProperties = new byte[16384];

                IntPtr descriptorPtr = Marshal.AllocHGlobal(1000);
                byte[] descriptor_b = new byte[1000];

                uint returned = 0;
                int error = 0;

                bool hasError = !Extern.DeviceIoControlStorageQuery(fd, WindowsIoctl.IOCTL_STORAGE_QUERY_PROPERTY, ref query, (uint)Marshal.SizeOf(query), descriptorPtr, 1000, ref returned, IntPtr.Zero);

                if (hasError)
                    error = Marshal.GetLastWin32Error();

                Marshal.Copy(descriptorPtr, descriptor_b, 0, 1000);

                if (hasError && error != 0)
                    continue;

                StorageDeviceDescriptor descriptor = new StorageDeviceDescriptor();
                descriptor.Version = BitConverter.ToUInt32(descriptor_b, 0);
                descriptor.Size = BitConverter.ToUInt32(descriptor_b, 4);
                descriptor.DeviceType = descriptor_b[8];
                descriptor.DeviceTypeModifier = descriptor_b[9];
                descriptor.RemovableMedia = BitConverter.ToBoolean(descriptor_b, 10);
                descriptor.CommandQueueing= BitConverter.ToBoolean(descriptor_b, 11);
                descriptor.VendorIdOffset = BitConverter.ToUInt32(descriptor_b, 12);
                descriptor.ProductIdOffset = BitConverter.ToUInt32(descriptor_b, 16);
                descriptor.ProductRevisionOffset = BitConverter.ToUInt32(descriptor_b, 20);
                descriptor.SerialNumberOffset = BitConverter.ToUInt32(descriptor_b, 24);
                descriptor.BusType = (StorageBusType)BitConverter.ToUInt32(descriptor_b, 28);
                descriptor.RawPropertiesLength = BitConverter.ToUInt32(descriptor_b, 32);

                DeviceInfo info = new DeviceInfo
                {
                    path = physId,
                    bus = descriptor.BusType.ToString()
                };

                if (descriptor.VendorIdOffset > 0)
                    info.vendor = StringHandlers.CToString(descriptor_b, Encoding.ASCII, start: (int)descriptor.VendorIdOffset);
                if (descriptor.ProductIdOffset > 0)
                    info.model = StringHandlers.CToString(descriptor_b, Encoding.ASCII, start: (int)descriptor.ProductIdOffset);
                // TODO: Get serial number of SCSI and USB devices, probably also FireWire (untested)
                if (descriptor.SerialNumberOffset > 0)
                    info.serial = StringHandlers.CToString(descriptor_b, Encoding.ASCII, start: (int)descriptor.SerialNumberOffset);

                if (string.IsNullOrEmpty(info.vendor) || info.vendor == "ATA")
                {
                    string[] pieces = info.model.Split(' ');
                    if (pieces.Length > 1)
                    {
                        info.vendor = pieces[0];
                        info.model = info.model.Substring(pieces[0].Length + 1);
                    }
                }

                switch (descriptor.BusType)
                {
                    case StorageBusType.SCSI:
                    case StorageBusType.ATAPI:
                    case StorageBusType.ATA:
                    case StorageBusType.FireWire:
                    case StorageBusType.SSA:
                    case StorageBusType.Fibre:
                    case StorageBusType.USB:
                    case StorageBusType.iSCSI:
                    case StorageBusType.SAS:
                    case StorageBusType.SATA:
                        info.supported = true;
                        break;
                }

                Marshal.FreeHGlobal(descriptorPtr);
                dev_list.Add(info);
            }

            DeviceInfo[] devices = dev_list.ToArray();

            return devices;
        }
    }
}
