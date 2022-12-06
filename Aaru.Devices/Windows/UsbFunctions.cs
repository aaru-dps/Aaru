// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UsbFunctions.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Windows direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains code to access USB device information under Windows.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General internal License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General internal License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// Copyright © 2007 Fort Hood TX, herethen, Public Domain
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Aaru.Devices.Windows
{
    //
    // A place for "higher level" related functions
    // You might not want to keep these in the USB class... your choice
    //
    // TODO: Even after cleaning, refactoring and xml-documenting, this code needs some love
    /// <summary>Implements functions for getting and accessing information from the USB bus</summary>
    internal static partial class Usb
    {
        const          int    IOCTL_STORAGE_GET_DEVICE_NUMBER = 0x2D1080;
        internal const string GuidDevinterfaceDisk            = "53f56307-b6bf-11d0-94f2-00a0c91efb8b";
        internal const string GuidDevinterfaceCdrom           = "53f56308-b6bf-11d0-94f2-00a0c91efb8b";
        internal const string GuidDevinterfaceFloppy          = "53f56311-b6bf-11d0-94f2-00a0c91efb8b";
        internal const string GuidDevinterfaceTape            = "53f5630b-b6bf-11d0-94f2-00a0c91efb8b";

        /// <summary>Get a list of all connected devices</summary>
        /// <returns>List of usb devices</returns>
        internal static List<UsbDevice> GetConnectedDevices()
        {
            List<UsbDevice> devList = new List<UsbDevice>();

            foreach(UsbController controller in GetHostControllers())
                ListHub(controller.GetRootHub(), devList);

            return devList;
        }

        /// <summary>private routine for enumerating a hub</summary>
        /// <param name="hub">Hub</param>
        /// <param name="devList">Device list</param>
        static void ListHub(UsbHub hub, ICollection<UsbDevice> devList)
        {
            foreach(UsbPort port in hub.GetPorts())
                if(port.IsHub)
                {
                    ListHub(port.GetHub(), devList);
                }
                else
                {
                    if(port.IsDeviceConnected)
                        devList.Add(port.GetDevice());
                }
        }

        /// <summary>Find a device based upon it's DriverKeyName</summary>
        /// <param name="driverKeyName">DriverKeyName</param>
        /// <returns>USB device</returns>
        internal static UsbDevice FindDeviceByDriverKeyName(string driverKeyName)
        {
            UsbDevice foundDevice = null;

            foreach(UsbController controller in GetHostControllers())
            {
                SearchHubDriverKeyName(controller.GetRootHub(), ref foundDevice, driverKeyName);

                if(foundDevice != null)
                    break;
            }

            return foundDevice;
        }

        /// <summary>Finds a device connected to a specified hub by it's DriverKeyName</summary>
        /// <param name="hub">Hub</param>
        /// <param name="foundDevice">UsbDevice</param>
        /// <param name="driverKeyName">DriverKeyName</param>
        static void SearchHubDriverKeyName(UsbHub hub, ref UsbDevice foundDevice, string driverKeyName)
        {
            foreach(UsbPort port in hub.GetPorts())
                if(port.IsHub)
                {
                    SearchHubDriverKeyName(port.GetHub(), ref foundDevice, driverKeyName);
                }
                else
                {
                    if(!port.IsDeviceConnected)
                        continue;

                    UsbDevice device = port.GetDevice();

                    if(device.DeviceDriverKey != driverKeyName)
                        continue;

                    foundDevice = device;

                    break;
                }
        }

        /// <summary>Find a device based upon it's Instance ID</summary>
        /// <param name="instanceId">Device instance ID</param>
        /// <returns>USB device</returns>
        static UsbDevice FindDeviceByInstanceId(string instanceId)
        {
            UsbDevice foundDevice = null;

            foreach(UsbController controller in GetHostControllers())
            {
                SearchHubInstanceId(controller.GetRootHub(), ref foundDevice, instanceId);

                if(foundDevice != null)
                    break;
            }

            return foundDevice;
        }

        /// <summary>private routine for enumerating a hub</summary>
        /// <param name="hub">Hub</param>
        /// <param name="foundDevice">USB device</param>
        /// <param name="instanceId">Device instance ID</param>
        static void SearchHubInstanceId(UsbHub hub, ref UsbDevice foundDevice, string instanceId)
        {
            foreach(UsbPort port in hub.GetPorts())
                if(port.IsHub)
                {
                    SearchHubInstanceId(port.GetHub(), ref foundDevice, instanceId);
                }
                else
                {
                    if(!port.IsDeviceConnected)
                        continue;

                    UsbDevice device = port.GetDevice();

                    if(device.InstanceId != instanceId)
                        continue;

                    foundDevice = device;

                    break;
                }
        }

        [DllImport("setupapi.dll")]
        static extern int CM_Get_Parent(out IntPtr pdnDevInst, IntPtr dnDevInst, int ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern int CM_Get_Device_ID(IntPtr dnDevInst, IntPtr buffer, int bufferLen, int ulFlags);

        /// <summary>Find a device based upon a Drive Letter</summary>
        /// <param name="driveLetter">Drive letter</param>
        /// <param name="deviceGuid">Device GUID</param>
        /// <returns>USB device</returns>
        internal static UsbDevice FindDriveLetter(string driveLetter, string deviceGuid)
        {
            // We start by getting the unique DeviceNumber of the given
            // DriveLetter.  We'll use this later to find a matching
            // DevicePath "symbolic name"
            int devNum = GetDeviceNumber(@"\\.\" + driveLetter.TrimEnd('\\'));

            return devNum < 0 ? null : FindDeviceNumber(devNum, deviceGuid);
        }

        /// <summary>Find a device based upon a Drive Path</summary>
        /// <param name="drivePath">Drive path</param>
        /// <param name="deviceGuid">Device GUID</param>
        /// <returns>USB device</returns>
        internal static UsbDevice FindDrivePath(string drivePath, string deviceGuid)
        {
            // We start by getting the unique DeviceNumber of the given
            // DriveLetter.  We'll use this later to find a matching
            // DevicePath "symbolic name"
            int devNum = GetDeviceNumber(drivePath);

            return devNum < 0 ? null : FindDeviceNumber(devNum, deviceGuid);
        }

        /// <summary>Find a device based upon a Device Number</summary>
        /// <param name="devNum">Device Number</param>
        /// <param name="deviceGuid">Device GUID</param>
        /// <returns>USB device</returns>
        static UsbDevice FindDeviceNumber(int devNum, string deviceGuid)
        {
            UsbDevice foundDevice = null;
            string    instanceId  = "";

            var diskGuid = new Guid(deviceGuid);

            // We start at the "root" of the device tree and look for all
            // devices that match the interface GUID of a disk
            IntPtr h = SetupDiGetClassDevs(ref diskGuid, 0, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

            if(h != INVALID_HANDLE_VALUE)
            {
                bool success;
                int  i = 0;

                do
                {
                    // create a Device Interface Data structure
                    var dia = new SpDeviceInterfaceData();
                    dia.cbSize = Marshal.SizeOf(dia);

                    // start the enumeration
                    success = SetupDiEnumDeviceInterfaces(h, IntPtr.Zero, ref diskGuid, i, ref dia);

                    if(success)
                    {
                        // build a DevInfo Data structure
                        var da = new SpDevinfoData();
                        da.cbSize = Marshal.SizeOf(da);

                        // build a Device Interface Detail Data structure
                        var didd = new SpDeviceInterfaceDetailData
                        {
                            cbSize = 4 + Marshal.SystemDefaultCharSize
                        }; // trust me :)

                        // now we can get some more detailed information
                        int       nRequiredSize = 0;
                        const int N_BYTES       = BUFFER_SIZE;

                        if(SetupDiGetDeviceInterfaceDetail(h, ref dia, ref didd, N_BYTES, ref nRequiredSize, ref da))
                            if(GetDeviceNumber(didd.DevicePath) == devNum)
                            {
                                // current InstanceID is at the "USBSTOR" level, so we
                                // need up "move up" one level to get to the "USB" level
                                CM_Get_Parent(out IntPtr ptrPrevious, da.DevInst, 0);

                                // Now we get the InstanceID of the USB level device
                                IntPtr ptrInstanceBuf = Marshal.AllocHGlobal(N_BYTES);
                                CM_Get_Device_ID(ptrPrevious, ptrInstanceBuf, N_BYTES, 0);
                                instanceId = Marshal.PtrToStringAuto(ptrInstanceBuf);

                                Marshal.FreeHGlobal(ptrInstanceBuf);

                                //System.Console.WriteLine("InstanceId: {0}", instanceId);
                                //break;
                            }
                    }

                    i++;
                } while(success);

                SetupDiDestroyDeviceInfoList(h);
            }

            // Did we find an InterfaceID of a USB device?
            if(instanceId?.StartsWith("USB\\", StringComparison.Ordinal) == true)
                foundDevice = FindDeviceByInstanceId(instanceId);

            return foundDevice;
        }

        /// <summary>return a unique device number for the given device path</summary>
        /// <param name="devicePath">Device path</param>
        /// <returns>Device number</returns>
        static int GetDeviceNumber(string devicePath)
        {
            int ans = -1;

            IntPtr h = CreateFile(devicePath.TrimEnd('\\'), 0, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            if(h == INVALID_HANDLE_VALUE)
                return ans;

            var    sdn    = new StorageDeviceNumber();
            int    nBytes = Marshal.SizeOf(sdn);
            IntPtr ptrSdn = Marshal.AllocHGlobal(nBytes);

            if(DeviceIoControl(h, IOCTL_STORAGE_GET_DEVICE_NUMBER, IntPtr.Zero, 0, ptrSdn, nBytes, out _, IntPtr.Zero))
            {
                sdn = (StorageDeviceNumber)Marshal.PtrToStructure(ptrSdn, typeof(StorageDeviceNumber));

                // just my way of combining the relevant parts of the
                // STORAGE_DEVICE_NUMBER into a single number
                ans = (sdn.DeviceType << 8) + sdn.DeviceNumber;
            }

            Marshal.FreeHGlobal(ptrSdn);
            CloseHandle(h);

            return ans;
        }

        [StructLayout(LayoutKind.Sequential)]
        readonly struct StorageDeviceNumber
        {
            internal readonly int DeviceType;
            internal readonly int DeviceNumber;
            internal readonly int PartitionNumber;
        }
    }
}