// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Usb.cs
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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Aaru.Devices.Windows;

// TODO: Even after cleaning, refactoring and xml-documenting, this code needs some love
/// <summary>Implements functions for getting and accessing information from the USB bus</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
static partial class Usb
{
    /// <summary>Return a list of USB Host Controllers</summary>
    /// <returns>List of USB Host Controllers</returns>
    static IEnumerable<UsbController> GetHostControllers()
    {
        var hostList = new List<UsbController>();
        var hostGuid = new Guid(GUID_DEVINTERFACE_HUBCONTROLLER);

        // We start at the "root" of the device tree and look for all
        // devices that match the interface GUID of a Hub Controller
        IntPtr h = SetupDiGetClassDevs(ref hostGuid, 0, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

        if(h == _invalidHandleValue)
            return new ReadOnlyCollection<UsbController>(hostList);

        IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);
        bool   success;
        var    i = 0;

        do
        {
            var host = new UsbController
            {
                ControllerIndex = i
            };

            // create a Device Interface Data structure
            var dia = new SpDeviceInterfaceData();
            dia.cbSize = Marshal.SizeOf(dia);

            // start the enumeration
            success = SetupDiEnumDeviceInterfaces(h, IntPtr.Zero, ref hostGuid, i, ref dia);

            if(success)
            {
                // build a DevInfo Data structure
                var da = new SpDevinfoData();
                da.cbSize = Marshal.SizeOf(da);

                // build a Device Interface Detail Data structure
                var didd = new SpDeviceInterfaceDetailData
                {
                    cbSize = 4 + Marshal.SystemDefaultCharSize
                };

                // trust me :)

                // now we can get some more detailed information
                var nRequiredSize = 0;

                if(SetupDiGetDeviceInterfaceDetail(h, ref dia, ref didd, BUFFER_SIZE, ref nRequiredSize, ref da))
                {
                    host.ControllerDevicePath = didd.DevicePath;

                    // get the Device Description and DriverKeyName
                    var requiredSize = 0;
                    int regType      = REG_SZ;

                    if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DEVICEDESC, ref regType, ptrBuf, BUFFER_SIZE,
                                                        ref requiredSize))
                        host.ControllerDeviceDesc = Marshal.PtrToStringAuto(ptrBuf);

                    if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref regType, ptrBuf, BUFFER_SIZE,
                                                        ref requiredSize))
                        host.ControllerDriverKeyName = Marshal.PtrToStringAuto(ptrBuf);
                }

                hostList.Add(host);
            }

            i++;
        } while(success);

        Marshal.FreeHGlobal(ptrBuf);
        SetupDiDestroyDeviceInfoList(h);

        // convert it into a Collection
        return new ReadOnlyCollection<UsbController>(hostList);
    }

    /// <summary>private function for finding a USB device's Description</summary>
    /// <param name="driverKeyName">Device driver key name</param>
    /// <returns>USB device description</returns>
    static string GetDescriptionByKeyName(string driverKeyName)
    {
        var ans = "";

        // Use the "enumerator form" of the SetupDiGetClassDevs API
        // to generate a list of all USB devices
        IntPtr h = SetupDiGetClassDevs(0, REGSTR_KEY_USB, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);

        if(h == _invalidHandleValue)
            return ans;

        IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);

        bool success;
        var  i = 0;

        do
        {
            // create a Device Interface Data structure
            var da = new SpDevinfoData();
            da.cbSize = Marshal.SizeOf(da);

            // start the enumeration
            success = SetupDiEnumDeviceInfo(h, i, ref da);

            if(success)
            {
                var requiredSize = 0;
                int regType      = REG_SZ;
                var keyName      = "";

                if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref regType, ptrBuf, BUFFER_SIZE,
                                                    ref requiredSize))
                    keyName = Marshal.PtrToStringAuto(ptrBuf);

                // is it a match?
                if(keyName == driverKeyName)
                {
                    if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DEVICEDESC, ref regType, ptrBuf, BUFFER_SIZE,
                                                        ref requiredSize))
                        ans = Marshal.PtrToStringAuto(ptrBuf);

                    break;
                }
            }

            i++;
        } while(success);

        Marshal.FreeHGlobal(ptrBuf);
        SetupDiDestroyDeviceInfoList(h);

        return ans;
    }

    /// <summary>private function for finding a USB device's Instance ID</summary>
    /// <param name="driverKeyName">Device driver key name</param>
    /// <returns>Device instance ID</returns>
    static string GetInstanceIdByKeyName(string driverKeyName)
    {
        var ans = "";

        // Use the "enumerator form" of the SetupDiGetClassDevs API
        // to generate a list of all USB devices
        IntPtr h = SetupDiGetClassDevs(0, REGSTR_KEY_USB, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);

        if(h == _invalidHandleValue)
            return ans;

        IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);

        bool success;
        var  i = 0;

        do
        {
            // create a Device Interface Data structure
            var da = new SpDevinfoData();
            da.cbSize = Marshal.SizeOf(da);

            // start the enumeration
            success = SetupDiEnumDeviceInfo(h, i, ref da);

            if(success)
            {
                var requiredSize = 0;
                int regType      = REG_SZ;

                var keyName = "";

                if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref regType, ptrBuf, BUFFER_SIZE,
                                                    ref requiredSize))
                    keyName = Marshal.PtrToStringAuto(ptrBuf);

                // is it a match?
                if(keyName == driverKeyName)
                {
                    var sb = new StringBuilder(BUFFER_SIZE);
                    SetupDiGetDeviceInstanceId(h, ref da, sb, BUFFER_SIZE, out requiredSize);
                    ans = sb.ToString();

                    break;
                }
            }

            i++;
        } while(success);

        Marshal.FreeHGlobal(ptrBuf);
        SetupDiDestroyDeviceInfoList(h);

        return ans;
    }

#region Nested type: UsbController

    /// <summary>Represents a USB Host Controller</summary>
    sealed class UsbController
    {
        internal string ControllerDriverKeyName, ControllerDevicePath, ControllerDeviceDesc;
        internal int    ControllerIndex;

        /// <summary>A simple default constructor</summary>
        internal UsbController()
        {
            ControllerIndex         = 0;
            ControllerDevicePath    = "";
            ControllerDeviceDesc    = "";
            ControllerDriverKeyName = "";
        }

        /// <summary>Return the index of the instance</summary>
        internal int Index => ControllerIndex;

        /// <summary>
        ///     Return the Device Path, such as "\\?\pci#ven_10de&amp;dev_005a&amp;subsys_815a1043&amp;rev_a2#3&267a616a&0&
        ///     58#{3abf6f2d-71c4-462a-8a92-1e6861e6af27}"
        /// </summary>
        internal string DevicePath => ControllerDevicePath;

        /// <summary>The DriverKeyName may be useful as a search key</summary>
        internal string DriverKeyName => ControllerDriverKeyName;

        /// <summary>Return the Friendly Name, such as "VIA USB Enhanced Host Controller"</summary>
        internal string Name => ControllerDeviceDesc;

        /// <summary>Return Root Hub for this Controller</summary>
        internal UsbHub GetRootHub()
        {
            var root = new UsbHub
            {
                HubIsRootHub  = true,
                HubDeviceDesc = "Root Hub"
            };

            // Open a handle to the Host Controller
            IntPtr h = CreateFile(ControllerDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                  IntPtr.Zero);

            if(h == _invalidHandleValue)
                return root;

            var    hubName    = new UsbRootHubName();
            int    nBytes     = Marshal.SizeOf(hubName);
            IntPtr ptrHubName = Marshal.AllocHGlobal(nBytes);

            // get the Hub Name
            if(DeviceIoControl(h, IOCTL_USB_GET_ROOT_HUB_NAME, ptrHubName, nBytes, ptrHubName, nBytes, out _,
                               IntPtr.Zero))
            {
                hubName = (UsbRootHubName)(Marshal.PtrToStructure(ptrHubName, typeof(UsbRootHubName)) ??
                                           default(UsbRootHubName));

                root.HubDevicePath = @"\\.\" + hubName.RootHubName;
            }

            // TODO: Get DriverKeyName for Root Hub
            // Now let's open the Hub (based upon the HubName we got above)
            IntPtr h2 = CreateFile(root.HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                   IntPtr.Zero);

            if(h2 != _invalidHandleValue)
            {
                var nodeInfo = new UsbNodeInformation
                {
                    NodeType = (int)UsbHubNode.UsbHub
                };

                nBytes = Marshal.SizeOf(nodeInfo);
                IntPtr ptrNodeInfo = Marshal.AllocHGlobal(nBytes);
                Marshal.StructureToPtr(nodeInfo, ptrNodeInfo, true);

                // get the Hub Information
                if(DeviceIoControl(h2, IOCTL_USB_GET_NODE_INFORMATION, ptrNodeInfo, nBytes, ptrNodeInfo, nBytes, out _,
                                   IntPtr.Zero))
                {
                    nodeInfo = (UsbNodeInformation)(Marshal.PtrToStructure(ptrNodeInfo, typeof(UsbNodeInformation)) ??
                                                    default(UsbNodeInformation));

                    root.HubIsBusPowered = Convert.ToBoolean(nodeInfo.HubInformation.HubIsBusPowered);
                    root.HubPortCount    = nodeInfo.HubInformation.HubDescriptor.bNumberOfPorts;
                }

                Marshal.FreeHGlobal(ptrNodeInfo);
                CloseHandle(h2);
            }

            Marshal.FreeHGlobal(ptrHubName);
            CloseHandle(h);

            return root;
        }
    }

#endregion

#region Nested type: UsbDevice

    /// <summary>Represents an USB device</summary>
    internal class UsbDevice
    {
        internal byte[]              BinaryDeviceDescriptors;
        internal UsbDeviceDescriptor DeviceDescriptor;
        internal string              DeviceDriverKey,    DeviceHubDevicePath, DeviceInstanceId, DeviceName;
        internal string              DeviceManufacturer, DeviceProduct,       DeviceSerialNumber;
        internal int                 DevicePortNumber;

        /// <summary>a simple default constructor</summary>
        internal UsbDevice()
        {
            DevicePortNumber        = 0;
            DeviceHubDevicePath     = "";
            DeviceDriverKey         = "";
            DeviceManufacturer      = "";
            DeviceProduct           = "Unknown USB Device";
            DeviceSerialNumber      = "";
            DeviceName              = "";
            DeviceInstanceId        = "";
            BinaryDeviceDescriptors = null;
        }

        /// <summary>return Port Index of the Hub</summary>
        internal int PortNumber => DevicePortNumber;

        /// <summary>return the Device Path of the Hub (the parent device)</summary>
        internal string HubDevicePath => DeviceHubDevicePath;

        /// <summary>useful as a search key</summary>
        internal string DriverKey => DeviceDriverKey;

        /// <summary>the device path of this device</summary>
        internal string InstanceId => DeviceInstanceId;

        /// <summary>the friendly name</summary>
        internal string Name => DeviceName;

        internal string Manufacturer => DeviceManufacturer;

        internal string Product => DeviceProduct;

        internal string SerialNumber => DeviceSerialNumber;

        internal byte[] BinaryDescriptors => BinaryDeviceDescriptors;
    }

#endregion

#region Nested type: UsbHub

    /// <summary>The Hub class</summary>
    internal class UsbHub
    {
        internal string HubDriverKey,    HubDevicePath, HubDeviceDesc;
        internal bool   HubIsBusPowered, HubIsRootHub;
        internal string HubManufacturer, HubProduct, HubSerialNumber, HubInstanceId;
        internal int    HubPortCount;

        /// <summary>a simple default constructor</summary>
        internal UsbHub()
        {
            HubPortCount    = 0;
            HubDevicePath   = "";
            HubDeviceDesc   = "";
            HubDriverKey    = "";
            HubIsBusPowered = false;
            HubIsRootHub    = false;
            HubManufacturer = "";
            HubProduct      = "";
            HubSerialNumber = "";
            HubInstanceId   = "";
        }

        /// <summary>return Port Count</summary>
        internal int PortCount => HubPortCount;

        /// <summary>
        ///     return the Device Path, such as "\\?\pci#ven_10de&amp;dev_005a&amp;subsys_815a1043&amp;rev_a2#3&267a616a&0&
        ///     58#{3abf6f2d-71c4-462a-8a92-1e6861e6af27}"
        /// </summary>
        internal string DevicePath => HubDevicePath;

        /// <summary>The DriverKey may be useful as a search key</summary>
        internal string DriverKey => HubDriverKey;

        /// <summary>return the Friendly Name, such as "VIA USB Enhanced Host Controller"</summary>
        internal string Name => HubDeviceDesc;

        /// <summary>the device path of this device</summary>
        internal string InstanceId => HubInstanceId;

        /// <summary>is is this a self-powered hub?</summary>
        internal bool IsBusPowered => HubIsBusPowered;

        /// <summary>is this a root hub?</summary>
        internal bool IsRootHub => HubIsRootHub;

        internal string Manufacturer => HubManufacturer;

        internal string Product => HubProduct;

        internal string SerialNumber => HubSerialNumber;

        /// <summary>return a list of the down stream ports</summary>
        /// <returns>List of downstream ports</returns>
        internal IEnumerable<UsbPort> GetPorts()
        {
            var portList = new List<UsbPort>();

            // Open a handle to the Hub device
            IntPtr h = CreateFile(HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                  IntPtr.Zero);

            if(h == _invalidHandleValue)
                return new ReadOnlyCollection<UsbPort>(portList);

            int    nBytes            = Marshal.SizeOf(typeof(UsbNodeConnectionInformationEx));
            IntPtr ptrNodeConnection = Marshal.AllocHGlobal(nBytes);

            // loop thru all of the ports on the hub
            // BTW: Ports are numbered starting at 1
            for(var i = 1; i <= HubPortCount; i++)
            {
                var nodeConnection = new UsbNodeConnectionInformationEx
                {
                    ConnectionIndex = i
                };

                Marshal.StructureToPtr(nodeConnection, ptrNodeConnection, true);

                if(!DeviceIoControl(h, IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX, ptrNodeConnection, nBytes,
                                    ptrNodeConnection, nBytes, out _, IntPtr.Zero))
                    continue;

                nodeConnection =
                    (UsbNodeConnectionInformationEx)(Marshal.PtrToStructure(ptrNodeConnection,
                                                                            typeof(UsbNodeConnectionInformationEx)) ??
                                                     default(UsbNodeConnectionInformationEx));

                // load up the USBPort class
                var port = new UsbPort
                {
                    PortPortNumber        = i,
                    PortHubDevicePath     = HubDevicePath,
                    PortStatus            = ((UsbConnectionStatus)nodeConnection.ConnectionStatus).ToString(),
                    PortSpeed             = ((UsbDeviceSpeed)nodeConnection.Speed).ToString(),
                    PortIsDeviceConnected = nodeConnection.ConnectionStatus == (int)UsbConnectionStatus.DeviceConnected,
                    PortIsHub             = Convert.ToBoolean(nodeConnection.DeviceIsHub),
                    PortDeviceDescriptor  = nodeConnection.DeviceDescriptor
                };

                // add it to the list
                portList.Add(port);
            }

            Marshal.FreeHGlobal(ptrNodeConnection);
            CloseHandle(h);

            // convert it into a Collection
            return new ReadOnlyCollection<UsbPort>(portList);
        }
    }

#endregion

#region Nested type: UsbPort

    /// <summary>Represents an USB port</summary>
    internal class UsbPort
    {
        internal UsbDeviceDescriptor PortDeviceDescriptor;
        internal bool                PortIsHub, PortIsDeviceConnected;
        internal int                 PortPortNumber;
        internal string              PortStatus, PortHubDevicePath, PortSpeed;

        /// <summary>a simple default constructor</summary>
        internal UsbPort()
        {
            PortPortNumber        = 0;
            PortStatus            = "";
            PortHubDevicePath     = "";
            PortSpeed             = "";
            PortIsHub             = false;
            PortIsDeviceConnected = false;
        }

        /// <summary>return Port Index of the Hub</summary>
        internal int PortNumber => PortPortNumber;

        /// <summary>return the Device Path of the Hub</summary>
        internal string HubDevicePath => PortHubDevicePath;

        /// <summary>the status (see USB_CONNECTION_STATUS above)</summary>
        internal string Status => PortStatus;

        /// <summary>the speed of the connection (see USB_DEVICE_SPEED above)</summary>
        internal string Speed => PortSpeed;

        /// <summary>is this a downstream external hub?</summary>
        internal bool IsHub => PortIsHub;

        /// <summary>is anybody home?</summary>
        internal bool IsDeviceConnected => PortIsDeviceConnected;

        /// <summary>return a down stream external hub</summary>
        /// <returns>Downstream external hub</returns>
        internal UsbDevice GetDevice()
        {
            if(!PortIsDeviceConnected)
                return null;

            // Copy over some values from the Port class
            // Ya know, I've given some thought about making Device a derived class...
            var device = new UsbDevice
            {
                DevicePortNumber    = PortPortNumber,
                DeviceHubDevicePath = PortHubDevicePath,
                DeviceDescriptor    = PortDeviceDescriptor
            };

            // Open a handle to the Hub device
            IntPtr h = CreateFile(PortHubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                  IntPtr.Zero);

            if(h == _invalidHandleValue)
                return device;

            int nBytesReturned;
            int nBytes = BUFFER_SIZE;

            // We use this to zero fill a buffer
            var nullString = new string((char)0, BUFFER_SIZE / Marshal.SystemDefaultCharSize);

            // The iManufacturer, iProduct and iSerialNumber entries in the
            // Device Descriptor are really just indexes.  So, we have to
            // request a String Descriptor to get the values for those strings.

            if(PortDeviceDescriptor.iManufacturer > 0)
            {
                // build a request for string descriptor
                var request = new UsbDescriptorRequest
                {
                    ConnectionIndex = PortPortNumber,
                    SetupPacket =
                    {
                        // Language Code
                        wIndex = 0x409,
                        wValue = (short)((USB_STRING_DESCRIPTOR_TYPE << 8) + PortDeviceDescriptor.iManufacturer)
                    }
                };

                request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(request));

                // Geez, I wish C# had a Marshal.MemSet() method
                IntPtr ptrRequest = Marshal.StringToHGlobalAuto(nullString);
                Marshal.StructureToPtr(request, ptrRequest, true);

                // Use an IOCTL call to request the String Descriptor
                if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes, ptrRequest,
                                   nBytes, out nBytesReturned, IntPtr.Zero))
                {
                    // The location of the string descriptor is immediately after
                    // the Request structure.  Because this location is not "covered"
                    // by the structure allocation, we're forced to zero out this
                    // chunk of memory by using the StringToHGlobalAuto() hack above
                    var ptrStringDesc = IntPtr.Add(ptrRequest, Marshal.SizeOf(request));

                    var stringDesc =
                        (UsbStringDescriptor)(Marshal.PtrToStructure(ptrStringDesc, typeof(UsbStringDescriptor)) ??
                                              default(UsbStringDescriptor));

                    device.DeviceManufacturer = stringDesc.bString;
                }

                Marshal.FreeHGlobal(ptrRequest);
            }

            if(PortDeviceDescriptor.iProduct > 0)
            {
                // build a request for string descriptor
                var request = new UsbDescriptorRequest
                {
                    ConnectionIndex = PortPortNumber,
                    SetupPacket =
                    {
                        // Language Code
                        wIndex = 0x409,
                        wValue = (short)((USB_STRING_DESCRIPTOR_TYPE << 8) + PortDeviceDescriptor.iProduct)
                    }
                };

                request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(request));

                // Geez, I wish C# had a Marshal.MemSet() method
                IntPtr ptrRequest = Marshal.StringToHGlobalAuto(nullString);
                Marshal.StructureToPtr(request, ptrRequest, true);

                // Use an IOCTL call to request the String Descriptor
                if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes, ptrRequest,
                                   nBytes, out nBytesReturned, IntPtr.Zero))
                {
                    // the location of the string descriptor is immediately after the Request structure
                    var ptrStringDesc = IntPtr.Add(ptrRequest, Marshal.SizeOf(request));

                    var stringDesc =
                        (UsbStringDescriptor)(Marshal.PtrToStructure(ptrStringDesc, typeof(UsbStringDescriptor)) ??
                                              default(UsbStringDescriptor));

                    device.DeviceProduct = stringDesc.bString;
                }

                Marshal.FreeHGlobal(ptrRequest);
            }

            if(PortDeviceDescriptor.iSerialNumber > 0)
            {
                // build a request for string descriptor
                var request = new UsbDescriptorRequest
                {
                    ConnectionIndex = PortPortNumber,
                    SetupPacket =
                    {
                        // Language Code
                        wIndex = 0x409,
                        wValue = (short)((USB_STRING_DESCRIPTOR_TYPE << 8) + PortDeviceDescriptor.iSerialNumber)
                    }
                };

                request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(request));

                // Geez, I wish C# had a Marshal.MemSet() method
                IntPtr ptrRequest = Marshal.StringToHGlobalAuto(nullString);
                Marshal.StructureToPtr(request, ptrRequest, true);

                // Use an IOCTL call to request the String Descriptor
                if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes, ptrRequest,
                                   nBytes, out nBytesReturned, IntPtr.Zero))
                {
                    // the location of the string descriptor is immediately after the Request structure
                    var ptrStringDesc = IntPtr.Add(ptrRequest, Marshal.SizeOf(request));

                    var stringDesc =
                        (UsbStringDescriptor)(Marshal.PtrToStructure(ptrStringDesc, typeof(UsbStringDescriptor)) ??
                                              default(UsbStringDescriptor));

                    device.DeviceSerialNumber = stringDesc.bString;
                }

                Marshal.FreeHGlobal(ptrRequest);
            }

            // build a request for configuration descriptor
            var dcrRequest = new UsbDescriptorRequest
            {
                ConnectionIndex = PortPortNumber,
                SetupPacket =
                {
                    wIndex = 0,
                    wValue = USB_CONFIGURATION_DESCRIPTOR_TYPE << 8
                }
            };

            dcrRequest.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(dcrRequest));

            // Geez, I wish C# had a Marshal.MemSet() method
            IntPtr dcrPtrRequest = Marshal.StringToHGlobalAuto(nullString);
            Marshal.StructureToPtr(dcrRequest, dcrPtrRequest, true);

            // Use an IOCTL call to request the String Descriptor
            if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, dcrPtrRequest, nBytes, dcrPtrRequest,
                               nBytes, out nBytesReturned, IntPtr.Zero))
            {
                var ptrStringDesc = IntPtr.Add(dcrPtrRequest, Marshal.SizeOf(dcrRequest));
                device.BinaryDeviceDescriptors = new byte[nBytesReturned];
                Marshal.Copy(ptrStringDesc, device.BinaryDeviceDescriptors, 0, nBytesReturned);
            }

            Marshal.FreeHGlobal(dcrPtrRequest);

            // Get the Driver Key Name (usefull in locating a device)
            var driverKey = new UsbNodeConnectionDriverkeyName
            {
                ConnectionIndex = PortPortNumber
            };

            nBytes = Marshal.SizeOf(driverKey);
            IntPtr ptrDriverKey = Marshal.AllocHGlobal(nBytes);
            Marshal.StructureToPtr(driverKey, ptrDriverKey, true);

            // Use an IOCTL call to request the Driver Key Name
            if(DeviceIoControl(h,      IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME, ptrDriverKey, nBytes, ptrDriverKey,
                               nBytes, out nBytesReturned,                           IntPtr.Zero))
            {
                driverKey = (UsbNodeConnectionDriverkeyName)(Marshal.PtrToStructure(ptrDriverKey,
                                                                 typeof(UsbNodeConnectionDriverkeyName)) ??
                                                             default(UsbNodeConnectionDriverkeyName));

                device.DeviceDriverKey = driverKey.DriverKeyName;

                // use the DriverKeyName to get the Device Description and Instance ID
                device.DeviceName       = GetDescriptionByKeyName(device.DeviceDriverKey);
                device.DeviceInstanceId = GetInstanceIdByKeyName(device.DeviceDriverKey);
            }

            Marshal.FreeHGlobal(ptrDriverKey);
            CloseHandle(h);

            return device;
        }

        /// <summary>return a down stream external hub</summary>
        /// <returns>Downstream external hub</returns>
        internal UsbHub GetHub()
        {
            if(!PortIsHub)
                return null;

            var hub = new UsbHub
            {
                HubIsRootHub  = false,
                HubDeviceDesc = "External Hub"
            };

            // Open a handle to the Host Controller
            IntPtr h = CreateFile(PortHubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                  IntPtr.Zero);

            if(h == _invalidHandleValue)
                return hub;

            // Get the DevicePath for downstream hub
            var nodeName = new UsbNodeConnectionName
            {
                ConnectionIndex = PortPortNumber
            };

            int    nBytes      = Marshal.SizeOf(nodeName);
            IntPtr ptrNodeName = Marshal.AllocHGlobal(nBytes);
            Marshal.StructureToPtr(nodeName, ptrNodeName, true);

            // Use an IOCTL call to request the Node Name
            if(DeviceIoControl(h, IOCTL_USB_GET_NODE_CONNECTION_NAME, ptrNodeName, nBytes, ptrNodeName, nBytes, out _,
                               IntPtr.Zero))
            {
                nodeName = (UsbNodeConnectionName)(Marshal.PtrToStructure(ptrNodeName, typeof(UsbNodeConnectionName)) ??
                                                   default(UsbNodeConnectionName));

                hub.HubDevicePath = @"\\.\" + nodeName.NodeName;
            }

            // Now let's open the Hub (based upon the HubName we got above)
            IntPtr h2 = CreateFile(hub.HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                   IntPtr.Zero);

            if(h2 != _invalidHandleValue)
            {
                var nodeInfo = new UsbNodeInformation
                {
                    NodeType = (int)UsbHubNode.UsbHub
                };

                nBytes = Marshal.SizeOf(nodeInfo);
                IntPtr ptrNodeInfo = Marshal.AllocHGlobal(nBytes);
                Marshal.StructureToPtr(nodeInfo, ptrNodeInfo, true);

                // get the Hub Information
                if(DeviceIoControl(h2, IOCTL_USB_GET_NODE_INFORMATION, ptrNodeInfo, nBytes, ptrNodeInfo, nBytes, out _,
                                   IntPtr.Zero))
                {
                    nodeInfo = (UsbNodeInformation)(Marshal.PtrToStructure(ptrNodeInfo, typeof(UsbNodeInformation)) ??
                                                    default(UsbNodeInformation));

                    hub.HubIsBusPowered = Convert.ToBoolean(nodeInfo.HubInformation.HubIsBusPowered);
                    hub.HubPortCount    = nodeInfo.HubInformation.HubDescriptor.bNumberOfPorts;
                }

                Marshal.FreeHGlobal(ptrNodeInfo);
                CloseHandle(h2);
            }

            // Fill in the missing Manufacture, Product, and SerialNumber values
            // values by just creating a Device instance and copying the values
            UsbDevice device = GetDevice();
            hub.HubInstanceId   = device.DeviceInstanceId;
            hub.HubManufacturer = device.Manufacturer;
            hub.HubProduct      = device.Product;
            hub.HubSerialNumber = device.SerialNumber;
            hub.HubDriverKey    = device.DriverKey;

            Marshal.FreeHGlobal(ptrNodeName);
            CloseHandle(h);

            return hub;
        }
    }

#endregion

#region "API Region"

    // ********************** Constants ************************

    const           int    GENERIC_WRITE       = 0x40000000;
    const           int    FILE_SHARE_READ     = 0x1;
    const           int    FILE_SHARE_WRITE    = 0x2;
    const           int    OPEN_EXISTING       = 0x3;
    static readonly IntPtr _invalidHandleValue = new(-1);

    const int IOCTL_GET_HCD_DRIVERKEY_NAME                  = 0x220424;
    const int IOCTL_USB_GET_ROOT_HUB_NAME                   = 0x220408;
    const int IOCTL_USB_GET_NODE_INFORMATION                = 0x220408; // same as above... strange, eh?
    const int IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX  = 0x220448;
    const int IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION = 0x220410;
    const int IOCTL_USB_GET_NODE_CONNECTION_NAME            = 0x220414;
    const int IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME  = 0x220420;

    const int USB_DEVICE_DESCRIPTOR_TYPE        = 0x1;
    const int USB_CONFIGURATION_DESCRIPTOR_TYPE = 0x2;
    const int USB_STRING_DESCRIPTOR_TYPE        = 0x3;

    const int BUFFER_SIZE               = 2048;
    const int MAXIMUM_USB_STRING_LENGTH = 255;

    const string GUID_DEVINTERFACE_HUBCONTROLLER = "3abf6f2d-71c4-462a-8a92-1e6861e6af27";
    const string REGSTR_KEY_USB                  = "USB";
    const int    DIGCF_PRESENT                   = 0x2;
    const int    DIGCF_ALLCLASSES                = 0x4;
    const int    DIGCF_DEVICEINTERFACE           = 0x10;
    const int    SPDRP_DRIVER                    = 0x9;
    const int    SPDRP_DEVICEDESC                = 0x0;
    const int    REG_SZ                          = 1;

    // ********************** Enumerations ************************

    enum UsbHubNode
    {
        UsbHub,
        UsbMiParent
    }

    enum UsbConnectionStatus
    {
        NoDeviceConnected,
        DeviceConnected,
        DeviceFailedEnumeration,
        DeviceGeneralFailure,
        DeviceCausedOvercurrent,
        DeviceNotEnoughPower,
        DeviceNotEnoughBandwidth,
        DeviceHubNestedTooDeeply,
        DeviceInLegacyHub
    }

    enum UsbDeviceSpeed : byte
    {
        UsbLowSpeed,
        UsbFullSpeed,
        UsbHighSpeed
    }

    // ********************** Stuctures ************************

    [StructLayout(LayoutKind.Sequential)]
    struct SpDevinfoData
    {
        internal          int    cbSize;
        internal readonly Guid   ClassGuid;
        internal readonly IntPtr DevInst;
        internal readonly IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SpDeviceInterfaceData
    {
        internal          int    cbSize;
        internal readonly Guid   InterfaceClassGuid;
        internal readonly int    Flags;
        internal readonly IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct SpDeviceInterfaceDetailData
    {
        internal int cbSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
        internal readonly string DevicePath;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct UsbHcdDriverkeyName
    {
        internal readonly int ActualLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
        internal readonly string DriverKeyName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct UsbRootHubName
    {
        internal readonly int ActualLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
        internal readonly string RootHubName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct UsbHubDescriptor
    {
        internal readonly byte  bDescriptorLength;
        internal readonly byte  bDescriptorType;
        internal readonly byte  bNumberOfPorts;
        internal readonly short wHubCharacteristics;
        internal readonly byte  bPowerOnToPowerGood;
        internal readonly byte  bHubControlCurrent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        internal readonly byte[] bRemoveAndPowerMask;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct UsbHubInformation
    {
        internal readonly UsbHubDescriptor HubDescriptor;
        internal readonly byte             HubIsBusPowered;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct UsbNodeInformation
    {
        internal          int               NodeType;
        internal readonly UsbHubInformation HubInformation; // Yeah, I'm assuming we'll just use the first form
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct UsbNodeConnectionInformationEx
    {
        internal          int                 ConnectionIndex;
        internal readonly UsbDeviceDescriptor DeviceDescriptor;
        internal readonly byte                CurrentConfigurationValue;
        internal readonly byte                Speed;
        internal readonly byte                DeviceIsHub;
        internal readonly short               DeviceAddress;
        internal readonly int                 NumberOfOpenPipes;

        internal readonly int ConnectionStatus;

        //internal IntPtr PipeList;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct UsbDeviceDescriptor
    {
        internal byte  bLength;
        internal byte  bDescriptorType;
        internal short bcdUSB;
        internal byte  bDeviceClass;
        internal byte  bDeviceSubClass;
        internal byte  bDeviceProtocol;
        internal byte  bMaxPacketSize0;
        internal short idVendor;
        internal short idProduct;
        internal short bcdDevice;
        internal byte  iManufacturer;
        internal byte  iProduct;
        internal byte  iSerialNumber;
        internal byte  bNumConfigurations;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct UsbStringDescriptor
    {
        internal readonly byte bLength;
        internal readonly byte bDescriptorType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXIMUM_USB_STRING_LENGTH)]
        internal readonly string bString;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct UsbSetupPacket
    {
        internal readonly byte  bmRequest;
        internal readonly byte  bRequest;
        internal          short wValue;
        internal          short wIndex;
        internal          short wLength;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct UsbDescriptorRequest
    {
        internal int ConnectionIndex;

        internal UsbSetupPacket SetupPacket;

        //internal byte[] Data;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct UsbNodeConnectionName
    {
        internal          int ConnectionIndex;
        internal readonly int ActualLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
        internal readonly string NodeName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct UsbNodeConnectionDriverkeyName // Yes, this is the same as the structure above...
    {
        internal          int ConnectionIndex;
        internal readonly int ActualLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
        internal readonly string DriverKeyName;
    }

    // ********************** API Definitions ************************

    [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
    static extern IntPtr SetupDiGetClassDevs( // 1st form using a ClassGUID
        ref Guid classGuid, int enumerator, IntPtr hwndParent, int flags);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto)] // 2nd form uses an Enumerator
    static extern IntPtr SetupDiGetClassDevs(int classGuid, string enumerator, IntPtr hwndParent, int flags);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool SetupDiEnumDeviceInterfaces(IntPtr                    deviceInfoSet,      IntPtr deviceInfoData,
                                                   ref Guid                  interfaceClassGuid, int    memberIndex,
                                                   ref SpDeviceInterfaceData deviceInterfaceData);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet,
                                                       ref SpDeviceInterfaceData deviceInterfaceData,
                                                       ref SpDeviceInterfaceDetailData deviceInterfaceDetailData,
                                                       int deviceInterfaceDetailDataSize, ref int requiredSize,
                                                       ref SpDevinfoData deviceInfoData);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool SetupDiGetDeviceRegistryProperty(IntPtr  deviceInfoSet,  ref SpDevinfoData deviceInfoData,
                                                        int     iProperty,      ref int           propertyRegDataType,
                                                        IntPtr  propertyBuffer, int               propertyBufferSize,
                                                        ref int requiredSize);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, int memberIndex, ref SpDevinfoData deviceInfoData);

    [DllImport("setupapi.dll", SetLastError = true)]
    static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool SetupDiGetDeviceInstanceId(IntPtr        deviceInfoSet,    ref SpDevinfoData deviceInfoData,
                                                  StringBuilder deviceInstanceId, int deviceInstanceIdSize,
                                                  out int       requiredSize);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool DeviceIoControl(IntPtr hDevice,     int dwIoControlCode, IntPtr  lpInBuffer, int nInBufferSize,
                                       IntPtr lpOutBuffer, int nOutBufferSize,  out int lpBytesReturned,
                                       IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr CreateFile(string lpFileName,           int dwDesiredAccess,       int dwShareMode,
                                    IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes,
                                    IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool CloseHandle(IntPtr hObject);

#endregion
}