// /***************************************************************************
// The Disc Image Chef
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
using System.Runtime.InteropServices;

// Copyright "Fort Hood TX", public domain, 2007
namespace DiscImageChef.Devices.Windows
{

    // 
    // A place for "higher level" related functions 
    // You might not want to keep these in the USB class... your choice 
    // 
    public partial class Usb
    {

        // 
        // Get a list of all connected devices 
        // 
        static public List<USBDevice> GetConnectedDevices()
        {
            List<USBDevice> DevList = new List<USBDevice>();

            foreach(USBController Controller in GetHostControllers())
            {
                ListHub(Controller.GetRootHub(), DevList);
            }
            return DevList;
        }

        // private routine for enumerating a hub 
        static void ListHub(USBHub Hub, List<USBDevice> DevList)
        {
            foreach(USBPort Port in Hub.GetPorts())
            {
                if(Port.IsHub)
                {
                    // recursive 
                    ListHub(Port.GetHub(), DevList);
                }
                else
                {
                    if(Port.IsDeviceConnected)
                    {
                        DevList.Add(Port.GetDevice());
                    }
                }
            }
        }

        // 
        // Find a device based upon it's DriverKeyName 
        // 
        static public USBDevice FindDeviceByDriverKeyName(string DriverKeyName)
        {
            USBDevice FoundDevice = null;

            foreach(USBController Controller in GetHostControllers())
            {
                SearchHubDriverKeyName(Controller.GetRootHub(), ref FoundDevice, DriverKeyName);
                if(FoundDevice != null)
                    break;
            }
            return FoundDevice;
        }

        // private routine for enumerating a hub 
        static void SearchHubDriverKeyName(USBHub Hub, ref USBDevice FoundDevice, string DriverKeyName)
        {
            foreach(USBPort Port in Hub.GetPorts())
            {
                if(Port.IsHub)
                {
                    // recursive 
                    SearchHubDriverKeyName(Port.GetHub(), ref FoundDevice, DriverKeyName);
                }
                else
                {
                    if(Port.IsDeviceConnected)
                    {
                        USBDevice Device = Port.GetDevice();
                        if(Device.DeviceDriverKey == DriverKeyName)
                        {
                            FoundDevice = Device;
                            break;
                        }
                    }
                }
            }
        }

        // 
        // Find a device based upon it's Instance ID 
        // 
        static public USBDevice FindDeviceByInstanceID(string InstanceID)
        {
            USBDevice FoundDevice = null;

            foreach(USBController Controller in GetHostControllers())
            {
                SearchHubInstanceID(Controller.GetRootHub(), ref FoundDevice, InstanceID);
                if(FoundDevice != null)
                    break;
            }
            return FoundDevice;
        }

        // private routine for enumerating a hub 
        static void SearchHubInstanceID(USBHub Hub, ref USBDevice FoundDevice, string InstanceID)
        {
            foreach(USBPort Port in Hub.GetPorts())
            {
                if(Port.IsHub)
                {
                    // recursive 
                    SearchHubInstanceID(Port.GetHub(), ref FoundDevice, InstanceID);
                }
                else
                {
                    if(Port.IsDeviceConnected)
                    {
                        USBDevice Device = Port.GetDevice();
                        if(Device.InstanceID == InstanceID)
                        {
                            FoundDevice = Device;
                            break;
                        }
                    }
                }
            }
        }

        const int IOCTL_STORAGE_GET_DEVICE_NUMBER = 0x2D1080;
        public const string GUID_DEVINTERFACE_DISK = "53f56307-b6bf-11d0-94f2-00a0c91efb8b";
        public const string GUID_DEVINTERFACE_CDROM = "53f56308-b6bf-11d0-94f2-00a0c91efb8b";
        public const string GUID_DEVINTERFACE_FLOPPY = "53f56311-b6bf-11d0-94f2-00a0c91efb8b";

        //typedef struct _STORAGE_DEVICE_NUMBER { 
        //  DEVICE_TYPE  DeviceType; 
        //  ULONG  DeviceNumber; 
        //  ULONG  PartitionNumber; 
        //} STORAGE_DEVICE_NUMBER, *PSTORAGE_DEVICE_NUMBER; 
        [StructLayout(LayoutKind.Sequential)]
        struct STORAGE_DEVICE_NUMBER
        {
            public int DeviceType;
            public int DeviceNumber;
            public int PartitionNumber;
        }

        //CMAPI CONFIGRET WINAPI  CM_Get_Parent( 
        //   OUT PDEVINST  pdnDevInst, 
        //   IN DEVINST  dnDevInst, 
        //   IN ULONG  ulFlags 
        //); 
        [DllImport("setupapi.dll")]
        static extern int CM_Get_Parent(
            out IntPtr pdnDevInst,
            IntPtr dnDevInst,
            int ulFlags
        );

        //CMAPI CONFIGRET WINAPI  CM_Get_Device_ID( 
        //    IN DEVINST  dnDevInst, 
        //    OUT PTCHAR  Buffer, 
        //    IN ULONG  BufferLen, 
        //    IN ULONG  ulFlags 
        //); 
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern int CM_Get_Device_ID(
            IntPtr dnDevInst,
            IntPtr Buffer,
            int BufferLen,
            int ulFlags
        );

        // 
        // Find a device based upon a Drive Letter 
        // 
        static public USBDevice FindDriveLetter(string DriveLetter, string deviceGuid)
        {
            USBDevice FoundDevice = null;
            string InstanceID = "";

            // We start by getting the unique DeviceNumber of the given 
            // DriveLetter.  We'll use this later to find a matching 
            // DevicePath "symbolic name" 
            int DevNum = GetDeviceNumber(@"\\.\" + DriveLetter.TrimEnd('\\'));
            if(DevNum < 0)
            {
                return null;
            }

            return FindDeviceNumber(DevNum, deviceGuid);
        }

        static public USBDevice FindDrivePath(string DrivePath, string deviceGuid)
        {
            USBDevice FoundDevice = null;
            string InstanceID = "";

            // We start by getting the unique DeviceNumber of the given 
            // DriveLetter.  We'll use this later to find a matching 
            // DevicePath "symbolic name" 
            int DevNum = GetDeviceNumber(DrivePath);
            if(DevNum < 0)
            {
                return null;
            }

            return FindDeviceNumber(DevNum, deviceGuid);
        }

        // 
        // Find a device based upon a Drive Letter 
        // 
        static public USBDevice FindDeviceNumber(int DevNum, string deviceGuid)
        {
            USBDevice FoundDevice = null;
            string InstanceID = "";

            Guid DiskGUID = new Guid(deviceGuid);

            // We start at the "root" of the device tree and look for all 
            // devices that match the interface GUID of a disk 
            IntPtr h = SetupDiGetClassDevs(ref DiskGUID, 0, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if(h.ToInt32() != INVALID_HANDLE_VALUE)
            {
                bool Success = true;
                int i = 0;
                do
                {
                    // create a Device Interface Data structure 
                    SP_DEVICE_INTERFACE_DATA dia = new SP_DEVICE_INTERFACE_DATA();
                    dia.cbSize = Marshal.SizeOf(dia);

                    // start the enumeration  
                    Success = SetupDiEnumDeviceInterfaces(h, IntPtr.Zero, ref DiskGUID, i, ref dia);
                    if(Success)
                    {
                        // build a DevInfo Data structure 
                        SP_DEVINFO_DATA da = new SP_DEVINFO_DATA();
                        da.cbSize = Marshal.SizeOf(da);

                        // build a Device Interface Detail Data structure 
                        SP_DEVICE_INTERFACE_DETAIL_DATA didd = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                        didd.cbSize = 4 + Marshal.SystemDefaultCharSize; // trust me :) 

                        // now we can get some more detailed information 
                        int nRequiredSize = 0;
                        int nBytes = BUFFER_SIZE;
                        if(SetupDiGetDeviceInterfaceDetail(h, ref dia, ref didd, nBytes, ref nRequiredSize, ref da))
                        {
                            // Now that we have a DevicePath... we can use it to 
                            // generate another DeviceNumber to see if it matches 
                            // the one we're looking for. 
                            if(GetDeviceNumber(didd.DevicePath) == DevNum)
                            {
                                // current InstanceID is at the "USBSTOR" level, so we 
                                // need up "move up" one level to get to the "USB" level 
                                IntPtr ptrPrevious;
                                CM_Get_Parent(out ptrPrevious, da.DevInst, 0);

                                // Now we get the InstanceID of the USB level device 
                                IntPtr ptrInstanceBuf = Marshal.AllocHGlobal(nBytes);
                                CM_Get_Device_ID(ptrPrevious, ptrInstanceBuf, nBytes, 0);
                                InstanceID = Marshal.PtrToStringAuto(ptrInstanceBuf);

                                Marshal.FreeHGlobal(ptrInstanceBuf);
                                System.Console.WriteLine("InstanceId: {0}", InstanceID);
                                //break;
                            }
                        }
                    }
                    i++;
                } while(Success);
                SetupDiDestroyDeviceInfoList(h);
            }

            // Did we find an InterfaceID of a USB device? 
            if(InstanceID.StartsWith("USB\\"))
            {
                FoundDevice = FindDeviceByInstanceID(InstanceID);
            }
            return FoundDevice;
        }

        // return a unique device number for the given device path 
        private static int GetDeviceNumber(string DevicePath)
        {
            int ans = -1;

            IntPtr h = CreateFile(DevicePath.TrimEnd('\\'), 0, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if(h.ToInt32() != INVALID_HANDLE_VALUE)
            {
                int requiredSize;
                STORAGE_DEVICE_NUMBER Sdn = new STORAGE_DEVICE_NUMBER();
                int nBytes = Marshal.SizeOf(Sdn);
                IntPtr ptrSdn = Marshal.AllocHGlobal(nBytes);

                if(DeviceIoControl(h, IOCTL_STORAGE_GET_DEVICE_NUMBER, IntPtr.Zero, 0, ptrSdn, nBytes, out requiredSize,
                    IntPtr.Zero))
                {
                    Sdn = (STORAGE_DEVICE_NUMBER)Marshal.PtrToStructure(ptrSdn, typeof(STORAGE_DEVICE_NUMBER));
                    // just my way of combining the relevant parts of the 
                    // STORAGE_DEVICE_NUMBER into a single number 
                    ans = (Sdn.DeviceType << 8) + Sdn.DeviceNumber;
                }
                Marshal.FreeHGlobal(ptrSdn);
                CloseHandle(h);
            }
            return ans;
        }
    }
} 
