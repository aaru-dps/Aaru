// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

// Copyright "Fort Hood TX", internal domain, 2007
namespace DiscImageChef.Devices.Windows
{
    partial class Usb
    {
        #region "API Region" 
        // ********************** Constants ************************ 

        const int GENERIC_WRITE = 0x40000000;
        const int FILE_SHARE_READ = 0x1;
        const int FILE_SHARE_WRITE = 0x2;
        const int OPEN_EXISTING = 0x3;
        const int INVALID_HANDLE_VALUE = -1;

        const int IOCTL_GET_HCD_DRIVERKEY_NAME = 0x220424;
        const int IOCTL_USB_GET_ROOT_HUB_NAME = 0x220408;
        const int IOCTL_USB_GET_NODE_INFORMATION = 0x220408; // same as above... strange, eh? 
        const int IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX = 0x220448;
        const int IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION = 0x220410;
        const int IOCTL_USB_GET_NODE_CONNECTION_NAME = 0x220414;
        const int IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME = 0x220420;

        const int USB_DEVICE_DESCRIPTOR_TYPE = 0x1;
        const int USB_CONFIGURATION_DESCRIPTOR_TYPE = 0x2;
        const int USB_STRING_DESCRIPTOR_TYPE = 0x3;

        const int BUFFER_SIZE = 2048;
        const int MAXIMUM_USB_STRING_LENGTH = 255;

        const string GUID_DEVINTERFACE_HUBCONTROLLER = "3abf6f2d-71c4-462a-8a92-1e6861e6af27";
        const string REGSTR_KEY_USB = "USB";
        const int DIGCF_PRESENT = 0x2;
        const int DIGCF_ALLCLASSES = 0x4;
        const int DIGCF_DEVICEINTERFACE = 0x10;
        const int SPDRP_DRIVER = 0x9;
        const int SPDRP_DEVICEDESC = 0x0;
        const int REG_SZ = 1;

        // ********************** Enumerations ************************ 

        //typedef enum _USB_HUB_NODE { 
        //    UsbHub, 
        //    UsbMIParent 
        //} USB_HUB_NODE; 
        enum UsbHubNode
        {
            UsbHub,
            UsbMiParent
        }

        //typedef enum _USB_CONNECTION_STATUS { 
        //    NoDeviceConnected, 
        //    DeviceConnected, 
        //    DeviceFailedEnumeration, 
        //    DeviceGeneralFailure, 
        //    DeviceCausedOvercurrent, 
        //    DeviceNotEnoughPower, 
        //    DeviceNotEnoughBandwidth, 
        //    DeviceHubNestedTooDeeply, 
        //    DeviceInLegacyHub 
        //} USB_CONNECTION_STATUS, *PUSB_CONNECTION_STATUS; 
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

        //typedef enum _USB_DEVICE_SPEED { 
        //    UsbLowSpeed = 0, 
        //    UsbFullSpeed, 
        //    UsbHighSpeed 
        //} USB_DEVICE_SPEED; 
        enum UsbDeviceSpeed : byte
        {
            UsbLowSpeed,
            UsbFullSpeed,
            UsbHighSpeed
        }

        // ********************** Stuctures ************************ 

        //typedef struct _SP_DEVINFO_DATA { 
        //  DWORD cbSize; 
        //  GUID ClassGuid; 
        //  DWORD DevInst; 
        //  ULONG_PTR Reserved; 
        //} SP_DEVINFO_DATA,  *PSP_DEVINFO_DATA; 
        [StructLayout(LayoutKind.Sequential)]
        struct SpDevinfoData
        {
            internal int cbSize;
            internal Guid ClassGuid;
            internal IntPtr DevInst;
            internal IntPtr Reserved;
        }

        //typedef struct _SP_DEVICE_INTERFACE_DATA { 
        //  DWORD cbSize; 
        //  GUID InterfaceClassGuid; 
        //  DWORD Flags; 
        //  ULONG_PTR Reserved; 
        //} SP_DEVICE_INTERFACE_DATA,  *PSP_DEVICE_INTERFACE_DATA; 
        [StructLayout(LayoutKind.Sequential)]
        struct SpDeviceInterfaceData
        {
            internal int cbSize;
            internal Guid InterfaceClassGuid;
            internal int Flags;
            internal IntPtr Reserved;
        }

        //typedef struct _SP_DEVICE_INTERFACE_DETAIL_DATA { 
        //  DWORD cbSize; 
        //  TCHAR DevicePath[ANYSIZE_ARRAY]; 
        //} SP_DEVICE_INTERFACE_DETAIL_DATA,  *PSP_DEVICE_INTERFACE_DETAIL_DATA; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SpDeviceInterfaceDetailData
        {
            internal int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] internal string DevicePath;
        }

        //typedef struct _USB_HCD_DRIVERKEY_NAME { 
        //    ULONG ActualLength; 
        //    WCHAR DriverKeyName[1]; 
        //} USB_HCD_DRIVERKEY_NAME, *PUSB_HCD_DRIVERKEY_NAME; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct UsbHcdDriverkeyName
        {
            internal int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] internal string DriverKeyName;
        }

        //typedef struct _USB_ROOT_HUB_NAME { 
        //    ULONG  ActualLength; 
        //    WCHAR  RootHubName[1]; 
        //} USB_ROOT_HUB_NAME, *PUSB_ROOT_HUB_NAME; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct UsbRootHubName
        {
            internal int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] internal string RootHubName;
        }

        //typedef struct _USB_HUB_DESCRIPTOR { 
        //    UCHAR  bDescriptorLength; 
        //    UCHAR  bDescriptorType; 
        //    UCHAR  bNumberOfPorts; 
        //    USHORT  wHubCharacteristics; 
        //    UCHAR  bPowerOnToPowerGood; 
        //    UCHAR  bHubControlCurrent; 
        //    UCHAR  bRemoveAndPowerMask[64]; 
        //} USB_HUB_DESCRIPTOR, *PUSB_HUB_DESCRIPTOR; 
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct UsbHubDescriptor
        {
            internal byte bDescriptorLength;
            internal byte bDescriptorType;
            internal byte bNumberOfPorts;
            internal short wHubCharacteristics;
            internal byte bPowerOnToPowerGood;
            internal byte bHubControlCurrent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] internal byte[] bRemoveAndPowerMask;
        }

        //typedef struct _USB_HUB_INFORMATION { 
        //    USB_HUB_DESCRIPTOR HubDescriptor; 
        //    BOOLEAN HubIsBusPowered; 
        //} USB_HUB_INFORMATION, *PUSB_HUB_INFORMATION; 
        [StructLayout(LayoutKind.Sequential)]
        struct UsbHubInformation
        {
            internal UsbHubDescriptor HubDescriptor;
            internal byte HubIsBusPowered;
        }

        //typedef struct _USB_NODE_INFORMATION { 
        //    USB_HUB_NODE  NodeType; 
        //    union { 
        //        USB_HUB_INFORMATION  HubInformation; 
        //        USB_MI_PARENT_INFORMATION  MiParentInformation; 
        //    } u; 
        //} USB_NODE_INFORMATION, *PUSB_NODE_INFORMATION; 
        [StructLayout(LayoutKind.Sequential)]
        struct UsbNodeInformation
        {
            internal int NodeType;
            internal UsbHubInformation HubInformation; // Yeah, I'm assuming we'll just use the first form 
        }

        //typedef struct _USB_NODE_CONNECTION_INFORMATION_EX { 
        //    ULONG  ConnectionIndex; 
        //    USB_DEVICE_DESCRIPTOR  DeviceDescriptor; 
        //    UCHAR  CurrentConfigurationValue; 
        //    UCHAR  Speed; 
        //    BOOLEAN  DeviceIsHub; 
        //    USHORT  DeviceAddress; 
        //    ULONG  NumberOfOpenPipes; 
        //    USB_CONNECTION_STATUS  ConnectionStatus; 
        //    USB_PIPE_INFO  PipeList[0]; 
        //} USB_NODE_CONNECTION_INFORMATION_EX, *PUSB_NODE_CONNECTION_INFORMATION_EX; 
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct UsbNodeConnectionInformationEx
        {
            internal int ConnectionIndex;
            internal UsbDeviceDescriptor DeviceDescriptor;
            internal byte CurrentConfigurationValue;
            internal byte Speed;
            internal byte DeviceIsHub;
            internal short DeviceAddress;
            internal int NumberOfOpenPipes;

            internal int ConnectionStatus;
            //internal IntPtr PipeList; 
        }

        //typedef struct _USB_DEVICE_DESCRIPTOR { 
        //    UCHAR  bLength; 
        //    UCHAR  bDescriptorType; 
        //    USHORT  bcdUSB; 
        //    UCHAR  bDeviceClass; 
        //    UCHAR  bDeviceSubClass; 
        //    UCHAR  bDeviceProtocol; 
        //    UCHAR  bMaxPacketSize0; 
        //    USHORT  idVendor; 
        //    USHORT  idProduct; 
        //    USHORT  bcdDevice; 
        //    UCHAR  iManufacturer; 
        //    UCHAR  iProduct; 
        //    UCHAR  iSerialNumber; 
        //    UCHAR  bNumConfigurations; 
        //} USB_DEVICE_DESCRIPTOR, *PUSB_DEVICE_DESCRIPTOR ; 
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct UsbDeviceDescriptor
        {
            internal byte bLength;
            internal byte bDescriptorType;
            internal short bcdUSB;
            internal byte bDeviceClass;
            internal byte bDeviceSubClass;
            internal byte bDeviceProtocol;
            internal byte bMaxPacketSize0;
            internal short idVendor;
            internal short idProduct;
            internal short bcdDevice;
            internal byte iManufacturer;
            internal byte iProduct;
            internal byte iSerialNumber;
            internal byte bNumConfigurations;
        }

        //typedef struct _USB_STRING_DESCRIPTOR { 
        //    UCHAR bLength; 
        //    UCHAR bDescriptorType; 
        //    WCHAR bString[1]; 
        //} USB_STRING_DESCRIPTOR, *PUSB_STRING_DESCRIPTOR; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct UsbStringDescriptor
        {
            internal byte bLength;
            internal byte bDescriptorType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXIMUM_USB_STRING_LENGTH)] internal string bString;
        }

        //typedef struct _USB_DESCRIPTOR_REQUEST { 
        //  ULONG ConnectionIndex; 
        //  struct { 
        //    UCHAR  bmRequest; 
        //    UCHAR  bRequest; 
        //    USHORT  wValue; 
        //    USHORT  wIndex; 
        //    USHORT  wLength; 
        //  } SetupPacket; 
        //  UCHAR  Data[0]; 
        //} USB_DESCRIPTOR_REQUEST, *PUSB_DESCRIPTOR_REQUEST 
        [StructLayout(LayoutKind.Sequential)]
        struct UsbSetupPacket
        {
            internal byte bmRequest;
            internal byte bRequest;
            internal short wValue;
            internal short wIndex;
            internal short wLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct UsbDescriptorRequest
        {
            internal int ConnectionIndex;

            internal UsbSetupPacket SetupPacket;
            //internal byte[] Data; 
        }

        //typedef struct _USB_NODE_CONNECTION_NAME { 
        //    ULONG  ConnectionIndex; 
        //    ULONG  ActualLength; 
        //    WCHAR  NodeName[1]; 
        //} USB_NODE_CONNECTION_NAME, *PUSB_NODE_CONNECTION_NAME; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct UsbNodeConnectionName
        {
            internal int ConnectionIndex;
            internal int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] internal string NodeName;
        }

        //typedef struct _USB_NODE_CONNECTION_DRIVERKEY_NAME { 
        //    ULONG  ConnectionIndex; 
        //    ULONG  ActualLength; 
        //    WCHAR  DriverKeyName[1]; 
        //} USB_NODE_CONNECTION_DRIVERKEY_NAME, *PUSB_NODE_CONNECTION_DRIVERKEY_NAME; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct UsbNodeConnectionDriverkeyName // Yes, this is the same as the structure above... 
        {
            internal int ConnectionIndex;
            internal int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] internal string DriverKeyName;
        }

        // ********************** API Definitions ************************ 

        //HDEVINFO SetupDiGetClassDevs( 
        //  const GUID* ClassGuid, 
        //  PCTSTR Enumerator, 
        //  HWND hwndParent, 
        //  DWORD Flags 
        //); 
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetupDiGetClassDevs( // 1st form using a ClassGUID 
            ref Guid classGuid, int enumerator, IntPtr hwndParent, int flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)] // 2nd form uses an Enumerator 
        static extern IntPtr SetupDiGetClassDevs(int classGuid, string enumerator, IntPtr hwndParent, int flags);

        //BOOL SetupDiEnumDeviceInterfaces( 
        //  HDEVINFO DeviceInfoSet, 
        //  PSP_DEVINFO_DATA DeviceInfoData, 
        //  const GUID* InterfaceClassGuid, 
        //  DWORD MemberIndex, 
        //  PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData 
        //); 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData,
                                                       ref Guid interfaceClassGuid, int memberIndex,
                                                       ref SpDeviceInterfaceData deviceInterfaceData);

        //BOOL SetupDiGetDeviceInterfaceDetail( 
        //  HDEVINFO DeviceInfoSet, 
        //  PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData, 
        //  PSP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, 
        //  DWORD DeviceInterfaceDetailDataSize, 
        //  PDWORD RequiredSize, 
        //  PSP_DEVINFO_DATA DeviceInfoData 
        //); 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet,
                                                           ref SpDeviceInterfaceData deviceInterfaceData,
                                                           ref SpDeviceInterfaceDetailData
                                                               deviceInterfaceDetailData,
                                                           int deviceInterfaceDetailDataSize, ref int requiredSize,
                                                           ref SpDevinfoData deviceInfoData);

        //BOOL SetupDiGetDeviceRegistryProperty( 
        //  HDEVINFO DeviceInfoSet, 
        //  PSP_DEVINFO_DATA DeviceInfoData, 
        //  DWORD Property, 
        //  PDWORD PropertyRegDataType, 
        //  PBYTE PropertyBuffer, 
        //  DWORD PropertyBufferSize, 
        //  PDWORD RequiredSize 
        //); 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref SpDevinfoData deviceInfoData,
                                                            int iProperty, ref int propertyRegDataType,
                                                            IntPtr propertyBuffer, int propertyBufferSize,
                                                            ref int requiredSize);

        //BOOL SetupDiEnumDeviceInfo( 
        //  HDEVINFO DeviceInfoSet, 
        //  DWORD MemberIndex, 
        //  PSP_DEVINFO_DATA DeviceInfoData 
        //); 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, int memberIndex,
                                                 ref SpDevinfoData deviceInfoData);

        //BOOL SetupDiDestroyDeviceInfoList( 
        //  HDEVINFO DeviceInfoSet 
        //); 
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        //WINSETUPAPI BOOL WINAPI  SetupDiGetDeviceInstanceId( 
        //    IN HDEVINFO  DeviceInfoSet, 
        //    IN PSP_DEVINFO_DATA  DeviceInfoData, 
        //    OUT PTSTR  DeviceInstanceId, 
        //    IN DWORD  DeviceInstanceIdSize, 
        //    OUT PDWORD  RequiredSize  OPTIONAL 
        //); 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiGetDeviceInstanceId(IntPtr deviceInfoSet, ref SpDevinfoData deviceInfoData,
                                                      StringBuilder deviceInstanceId, int deviceInstanceIdSize,
                                                      out int requiredSize);

        //BOOL DeviceIoControl( 
        //  HANDLE hDevice, 
        //  DWORD dwIoControlCode, 
        //  LPVOID lpInBuffer, 
        //  DWORD nInBufferSize, 
        //  LPVOID lpOutBuffer, 
        //  DWORD nOutBufferSize, 
        //  LPDWORD lpBytesReturned, 
        //  LPOVERLAPPED lpOverlapped 
        //); 
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool DeviceIoControl(IntPtr hDevice, int dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize,
                                           IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned,
                                           IntPtr lpOverlapped);

        //HANDLE CreateFile( 
        //  LPCTSTR lpFileName, 
        //  DWORD dwDesiredAccess, 
        //  DWORD dwShareMode, 
        //  LPSECURITY_ATTRIBUTES lpSecurityAttributes, 
        //  DWORD dwCreationDisposition, 
        //  DWORD dwFlagsAndAttributes, 
        //  HANDLE hTemplateFile 
        //); 
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode,
                                        IntPtr lpSecurityAttributes, int dwCreationDisposition,
                                        int dwFlagsAndAttributes, IntPtr hTemplateFile);

        //BOOL CloseHandle( 
        //  HANDLE hObject 
        //); 
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CloseHandle(IntPtr hObject);
        #endregion

        // 
        // Return a list of USB Host Controllers 
        // 
        static internal System.Collections.ObjectModel.ReadOnlyCollection<UsbController> GetHostControllers()
        {
            List<UsbController> hostList = new List<UsbController>();
            Guid hostGuid = new Guid(GUID_DEVINTERFACE_HUBCONTROLLER);

            // We start at the "root" of the device tree and look for all 
            // devices that match the interface GUID of a Hub Controller 
            IntPtr h = SetupDiGetClassDevs(ref hostGuid, 0, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if(h.ToInt32() != INVALID_HANDLE_VALUE)
            {
                IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);
                bool success;
                int i = 0;
                do
                {
                    UsbController host = new UsbController();
                    host.ControllerIndex = i;

                    // create a Device Interface Data structure 
                    SpDeviceInterfaceData dia = new SpDeviceInterfaceData();
                    dia.cbSize = Marshal.SizeOf(dia);

                    // start the enumeration  
                    success = SetupDiEnumDeviceInterfaces(h, IntPtr.Zero, ref hostGuid, i, ref dia);
                    if(success)
                    {
                        // build a DevInfo Data structure 
                        SpDevinfoData da = new SpDevinfoData();
                        da.cbSize = Marshal.SizeOf(da);

                        // build a Device Interface Detail Data structure 
                        SpDeviceInterfaceDetailData didd = new SpDeviceInterfaceDetailData();
                        didd.cbSize = 4 + Marshal.SystemDefaultCharSize; // trust me :) 

                        // now we can get some more detailed information 
                        int nRequiredSize = 0;
                        int nBytes = BUFFER_SIZE;
                        if(SetupDiGetDeviceInterfaceDetail(h, ref dia, ref didd, nBytes, ref nRequiredSize, ref da))
                        {
                            host.ControllerDevicePath = didd.DevicePath;

                            // get the Device Description and DriverKeyName 
                            int requiredSize = 0;
                            int regType = REG_SZ;

                            if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DEVICEDESC, ref regType, ptrBuf,
                                                                BUFFER_SIZE, ref requiredSize))
                            {
                                host.ControllerDeviceDesc = Marshal.PtrToStringAuto(ptrBuf);
                            }
                            if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref regType, ptrBuf,
                                                                BUFFER_SIZE, ref requiredSize))
                            {
                                host.ControllerDriverKeyName = Marshal.PtrToStringAuto(ptrBuf);
                            }
                        }
                        hostList.Add(host);
                    }
                    i++;
                }
                while(success);

                Marshal.FreeHGlobal(ptrBuf);
                SetupDiDestroyDeviceInfoList(h);
            }

            // convert it into a Collection 
            return new System.Collections.ObjectModel.ReadOnlyCollection<UsbController>(hostList);
        }

        // 
        // The USB Host Controller Class 
        // 
        internal class UsbController
        {
            internal int ControllerIndex;
            internal string ControllerDriverKeyName, ControllerDevicePath, ControllerDeviceDesc;

            // A simple default constructor 
            internal UsbController()
            {
                ControllerIndex = 0;
                ControllerDevicePath = "";
                ControllerDeviceDesc = "";
                ControllerDriverKeyName = "";
            }

            // Return the index of the instance 
            internal int Index
            {
                get { return ControllerIndex; }
            }

            // Return the Device Path, such as "\\?\pci#ven_10de&dev_005a&subsys_815a1043&rev_a2#3&267a616a&0&58#{3abf6f2d-71c4-462a-8a92-1e6861e6af27}" 
            internal string DevicePath
            {
                get { return ControllerDevicePath; }
            }

            // The DriverKeyName may be useful as a search key 
            internal string DriverKeyName
            {
                get { return ControllerDriverKeyName; }
            }

            // Return the Friendly Name, such as "VIA USB Enhanced Host Controller" 
            internal string Name
            {
                get { return ControllerDeviceDesc; }
            }

            // Return Root Hub for this Controller 
            internal UsbHub GetRootHub()
            {
                IntPtr h, h2;
                UsbHub root = new UsbHub();
                root.HubIsRootHub = true;
                root.HubDeviceDesc = "Root Hub";

                // Open a handle to the Host Controller 
                h = CreateFile(ControllerDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                               IntPtr.Zero);
                if(h.ToInt32() != INVALID_HANDLE_VALUE)
                {
                    int nBytesReturned;
                    UsbRootHubName hubName = new UsbRootHubName();
                    int nBytes = Marshal.SizeOf(hubName);
                    IntPtr ptrHubName = Marshal.AllocHGlobal(nBytes);

                    // get the Hub Name 
                    if(DeviceIoControl(h, IOCTL_USB_GET_ROOT_HUB_NAME, ptrHubName, nBytes, ptrHubName, nBytes,
                                       out nBytesReturned, IntPtr.Zero))
                    {
                        hubName = (UsbRootHubName)Marshal.PtrToStructure(ptrHubName, typeof(UsbRootHubName));
                        root.HubDevicePath = @"\\.\" + hubName.RootHubName;
                    }

                    // TODO: Get DriverKeyName for Root Hub 

                    // Now let's open the Hub (based upon the HubName we got above) 
                    h2 = CreateFile(root.HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                    IntPtr.Zero);
                    if(h2.ToInt32() != INVALID_HANDLE_VALUE)
                    {
                        UsbNodeInformation nodeInfo = new UsbNodeInformation();
                        nodeInfo.NodeType = (int)UsbHubNode.UsbHub;
                        nBytes = Marshal.SizeOf(nodeInfo);
                        IntPtr ptrNodeInfo = Marshal.AllocHGlobal(nBytes);
                        Marshal.StructureToPtr(nodeInfo, ptrNodeInfo, true);

                        // get the Hub Information 
                        if(DeviceIoControl(h2, IOCTL_USB_GET_NODE_INFORMATION, ptrNodeInfo, nBytes, ptrNodeInfo, nBytes,
                                           out nBytesReturned, IntPtr.Zero))
                        {
                            nodeInfo = (UsbNodeInformation)Marshal.PtrToStructure(ptrNodeInfo,
                                                                                    typeof(UsbNodeInformation));
                            root.HubIsBusPowered = Convert.ToBoolean(nodeInfo.HubInformation.HubIsBusPowered);
                            root.HubPortCount = nodeInfo.HubInformation.HubDescriptor.bNumberOfPorts;
                        }
                        Marshal.FreeHGlobal(ptrNodeInfo);
                        CloseHandle(h2);
                    }

                    Marshal.FreeHGlobal(ptrHubName);
                    CloseHandle(h);
                }
                return root;
            }
        }

        // 
        // The Hub class 
        // 
        internal class UsbHub
        {
            internal int HubPortCount;
            internal string HubDriverKey, HubDevicePath, HubDeviceDesc;
            internal string HubManufacturer, HubProduct, HubSerialNumber, HubInstanceId;
            internal bool HubIsBusPowered, HubIsRootHub;

            // a simple default constructor 
            internal UsbHub()
            {
                HubPortCount = 0;
                HubDevicePath = "";
                HubDeviceDesc = "";
                HubDriverKey = "";
                HubIsBusPowered = false;
                HubIsRootHub = false;
                HubManufacturer = "";
                HubProduct = "";
                HubSerialNumber = "";
                HubInstanceId = "";
            }

            // return Port Count 
            internal int PortCount
            {
                get { return HubPortCount; }
            }

            // return the Device Path, such as "\\?\pci#ven_10de&dev_005a&subsys_815a1043&rev_a2#3&267a616a&0&58#{3abf6f2d-71c4-462a-8a92-1e6861e6af27}" 
            internal string DevicePath
            {
                get { return HubDevicePath; }
            }

            // The DriverKey may be useful as a search key 
            internal string DriverKey
            {
                get { return HubDriverKey; }
            }

            // return the Friendly Name, such as "VIA USB Enhanced Host Controller" 
            internal string Name
            {
                get { return HubDeviceDesc; }
            }

            // the device path of this device 
            internal string InstanceId
            {
                get { return HubInstanceId; }
            }

            // is is this a self-powered hub? 
            internal bool IsBusPowered
            {
                get { return HubIsBusPowered; }
            }

            // is this a root hub? 
            internal bool IsRootHub
            {
                get { return HubIsRootHub; }
            }

            internal string Manufacturer
            {
                get { return HubManufacturer; }
            }

            internal string Product
            {
                get { return HubProduct; }
            }

            internal string SerialNumber
            {
                get { return HubSerialNumber; }
            }

            // return a list of the down stream ports 
            internal System.Collections.ObjectModel.ReadOnlyCollection<UsbPort> GetPorts()
            {
                List<UsbPort> portList = new List<UsbPort>();

                // Open a handle to the Hub device 
                IntPtr h = CreateFile(HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                      IntPtr.Zero);
                if(h.ToInt32() != INVALID_HANDLE_VALUE)
                {
                    int nBytes = Marshal.SizeOf(typeof(UsbNodeConnectionInformationEx));
                    IntPtr ptrNodeConnection = Marshal.AllocHGlobal(nBytes);

                    // loop thru all of the ports on the hub 
                    // BTW: Ports are numbered starting at 1 
                    for(int i = 1; i <= HubPortCount; i++)
                    {
                        int nBytesReturned;
                        UsbNodeConnectionInformationEx nodeConnection = new UsbNodeConnectionInformationEx();
                        nodeConnection.ConnectionIndex = i;
                        Marshal.StructureToPtr(nodeConnection, ptrNodeConnection, true);

                        if(DeviceIoControl(h, IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX, ptrNodeConnection, nBytes,
                                           ptrNodeConnection, nBytes, out nBytesReturned, IntPtr.Zero))
                        {
                            nodeConnection =
                                (UsbNodeConnectionInformationEx)Marshal.PtrToStructure(ptrNodeConnection,
                                                                                           typeof(
                                                                                               UsbNodeConnectionInformationEx
                                                                                           ));

                            // load up the USBPort class 
                            UsbPort port = new UsbPort();
                            port.PortPortNumber = i;
                            port.PortHubDevicePath = HubDevicePath;
                            UsbConnectionStatus status = (UsbConnectionStatus)nodeConnection.ConnectionStatus;
                            port.PortStatus = status.ToString();
                            UsbDeviceSpeed speed = (UsbDeviceSpeed)nodeConnection.Speed;
                            port.PortSpeed = speed.ToString();
                            port.PortIsDeviceConnected =
                                nodeConnection.ConnectionStatus == (int)UsbConnectionStatus.DeviceConnected;
                            port.PortIsHub = Convert.ToBoolean(nodeConnection.DeviceIsHub);
                            port.PortDeviceDescriptor = nodeConnection.DeviceDescriptor;

                            // add it to the list 
                            portList.Add(port);
                        }
                    }

                    Marshal.FreeHGlobal(ptrNodeConnection);
                    CloseHandle(h);
                }
                // convert it into a Collection 
                return new System.Collections.ObjectModel.ReadOnlyCollection<UsbPort>(portList);
            }
        }

        // 
        // The Port Class 
        // 
        internal class UsbPort
        {
            internal int PortPortNumber;
            internal string PortStatus, PortHubDevicePath, PortSpeed;
            internal bool PortIsHub, PortIsDeviceConnected;
            internal UsbDeviceDescriptor PortDeviceDescriptor;

            // a simple default constructor 
            internal UsbPort()
            {
                PortPortNumber = 0;
                PortStatus = "";
                PortHubDevicePath = "";
                PortSpeed = "";
                PortIsHub = false;
                PortIsDeviceConnected = false;
            }

            // return Port Index of the Hub 
            internal int PortNumber
            {
                get { return PortPortNumber; }
            }

            // return the Device Path of the Hub 
            internal string HubDevicePath
            {
                get { return PortHubDevicePath; }
            }

            // the status (see USB_CONNECTION_STATUS above) 
            internal string Status
            {
                get { return PortStatus; }
            }

            // the speed of the connection (see USB_DEVICE_SPEED above) 
            internal string Speed
            {
                get { return PortSpeed; }
            }

            // is this a downstream external hub? 
            internal bool IsHub
            {
                get { return PortIsHub; }
            }

            // is anybody home? 
            internal bool IsDeviceConnected
            {
                get { return PortIsDeviceConnected; }
            }

            // return a down stream external hub 
            internal UsbDevice GetDevice()
            {
                if(!PortIsDeviceConnected) { return null; }

                UsbDevice device = new UsbDevice();

                // Copy over some values from the Port class 
                // Ya know, I've given some thought about making Device a derived class... 
                device.DevicePortNumber = PortPortNumber;
                device.DeviceHubDevicePath = PortHubDevicePath;
                device.DeviceDescriptor = PortDeviceDescriptor;

                // Open a handle to the Hub device 
                IntPtr h = CreateFile(PortHubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                      IntPtr.Zero);
                if(h.ToInt32() != INVALID_HANDLE_VALUE)
                {
                    int nBytesReturned;
                    int nBytes = BUFFER_SIZE;
                    // We use this to zero fill a buffer 
                    string nullString = new string((char)0, BUFFER_SIZE / Marshal.SystemDefaultCharSize);

                    // The iManufacturer, iProduct and iSerialNumber entries in the 
                    // Device Descriptor are really just indexes.  So, we have to  
                    // request a String Descriptor to get the values for those strings. 

                    if(PortDeviceDescriptor.iManufacturer > 0)
                    {
                        // build a request for string descriptor 
                        UsbDescriptorRequest request = new UsbDescriptorRequest();
                        request.ConnectionIndex = PortPortNumber;
                        request.SetupPacket.wValue =
                            (short)((USB_STRING_DESCRIPTOR_TYPE << 8) + PortDeviceDescriptor.iManufacturer);
                        request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(request));
                        request.SetupPacket.wIndex = 0x409; // Language Code 
                        // Geez, I wish C# had a Marshal.MemSet() method 
                        IntPtr ptrRequest = Marshal.StringToHGlobalAuto(nullString);
                        Marshal.StructureToPtr(request, ptrRequest, true);

                        // Use an IOCTL call to request the String Descriptor 
                        if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes,
                                           ptrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                        {
                            // The location of the string descriptor is immediately after 
                            // the Request structure.  Because this location is not "covered" 
                            // by the structure allocation, we're forced to zero out this 
                            // chunk of memory by using the StringToHGlobalAuto() hack above 
                            IntPtr ptrStringDesc = new IntPtr(ptrRequest.ToInt32() + Marshal.SizeOf(request));
                            UsbStringDescriptor stringDesc =
                                (UsbStringDescriptor)Marshal.PtrToStructure(ptrStringDesc,
                                                                              typeof(UsbStringDescriptor));
                            device.DeviceManufacturer = stringDesc.bString;
                        }
                        Marshal.FreeHGlobal(ptrRequest);
                    }
                    if(PortDeviceDescriptor.iProduct > 0)
                    {
                        // build a request for string descriptor 
                        UsbDescriptorRequest request = new UsbDescriptorRequest();
                        request.ConnectionIndex = PortPortNumber;
                        request.SetupPacket.wValue =
                            (short)((USB_STRING_DESCRIPTOR_TYPE << 8) + PortDeviceDescriptor.iProduct);
                        request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(request));
                        request.SetupPacket.wIndex = 0x409; // Language Code 
                        // Geez, I wish C# had a Marshal.MemSet() method 
                        IntPtr ptrRequest = Marshal.StringToHGlobalAuto(nullString);
                        Marshal.StructureToPtr(request, ptrRequest, true);

                        // Use an IOCTL call to request the String Descriptor 
                        if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes,
                                           ptrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                        {
                            // the location of the string descriptor is immediately after the Request structure 
                            IntPtr ptrStringDesc = new IntPtr(ptrRequest.ToInt32() + Marshal.SizeOf(request));
                            UsbStringDescriptor stringDesc =
                                (UsbStringDescriptor)Marshal.PtrToStructure(ptrStringDesc,
                                                                              typeof(UsbStringDescriptor));
                            device.DeviceProduct = stringDesc.bString;
                        }
                        Marshal.FreeHGlobal(ptrRequest);
                    }
                    if(PortDeviceDescriptor.iSerialNumber > 0)
                    {
                        // build a request for string descriptor 
                        UsbDescriptorRequest request = new UsbDescriptorRequest();
                        request.ConnectionIndex = PortPortNumber;
                        request.SetupPacket.wValue =
                            (short)((USB_STRING_DESCRIPTOR_TYPE << 8) + PortDeviceDescriptor.iSerialNumber);
                        request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(request));
                        request.SetupPacket.wIndex = 0x409; // Language Code 
                        // Geez, I wish C# had a Marshal.MemSet() method 
                        IntPtr ptrRequest = Marshal.StringToHGlobalAuto(nullString);
                        Marshal.StructureToPtr(request, ptrRequest, true);

                        // Use an IOCTL call to request the String Descriptor 
                        if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes,
                                           ptrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                        {
                            // the location of the string descriptor is immediately after the Request structure 
                            IntPtr ptrStringDesc = new IntPtr(ptrRequest.ToInt32() + Marshal.SizeOf(request));
                            UsbStringDescriptor stringDesc =
                                (UsbStringDescriptor)Marshal.PtrToStructure(ptrStringDesc,
                                                                              typeof(UsbStringDescriptor));
                            device.DeviceSerialNumber = stringDesc.bString;
                        }
                        Marshal.FreeHGlobal(ptrRequest);
                    }

                    // build a request for configuration descriptor 
                    UsbDescriptorRequest dcrRequest = new UsbDescriptorRequest();
                    dcrRequest.ConnectionIndex = PortPortNumber;
                    dcrRequest.SetupPacket.wValue = (short)(USB_CONFIGURATION_DESCRIPTOR_TYPE << 8);
                    dcrRequest.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(dcrRequest));
                    dcrRequest.SetupPacket.wIndex = 0;
                    // Geez, I wish C# had a Marshal.MemSet() method 
                    IntPtr dcrPtrRequest = Marshal.StringToHGlobalAuto(nullString);
                    Marshal.StructureToPtr(dcrRequest, dcrPtrRequest, true);

                    // Use an IOCTL call to request the String Descriptor 
                    if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, dcrPtrRequest, nBytes,
                                       dcrPtrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                    {
                        IntPtr ptrStringDesc = new IntPtr(dcrPtrRequest.ToInt32() + Marshal.SizeOf(dcrRequest));
                        device.BinaryDeviceDescriptors = new byte[nBytesReturned];
                        Marshal.Copy(ptrStringDesc, device.BinaryDeviceDescriptors, 0, nBytesReturned);
                    }
                    Marshal.FreeHGlobal(dcrPtrRequest);

                    // Get the Driver Key Name (usefull in locating a device) 
                    UsbNodeConnectionDriverkeyName driverKey = new UsbNodeConnectionDriverkeyName();
                    driverKey.ConnectionIndex = PortPortNumber;
                    nBytes = Marshal.SizeOf(driverKey);
                    IntPtr ptrDriverKey = Marshal.AllocHGlobal(nBytes);
                    Marshal.StructureToPtr(driverKey, ptrDriverKey, true);

                    // Use an IOCTL call to request the Driver Key Name 
                    if(DeviceIoControl(h, IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME, ptrDriverKey, nBytes,
                                       ptrDriverKey, nBytes, out nBytesReturned, IntPtr.Zero))
                    {
                        driverKey = (UsbNodeConnectionDriverkeyName)Marshal.PtrToStructure(ptrDriverKey,
                                                                                               typeof(
                                                                                                   UsbNodeConnectionDriverkeyName
                                                                                               ));
                        device.DeviceDriverKey = driverKey.DriverKeyName;

                        // use the DriverKeyName to get the Device Description and Instance ID 
                        device.DeviceName = GetDescriptionByKeyName(device.DeviceDriverKey);
                        device.DeviceInstanceId = GetInstanceIdByKeyName(device.DeviceDriverKey);
                    }
                    Marshal.FreeHGlobal(ptrDriverKey);
                    CloseHandle(h);
                }
                return device;
            }

            // return a down stream external hub 
            internal UsbHub GetHub()
            {
                if(!PortIsHub) { return null; }

                UsbHub hub = new UsbHub();
                IntPtr h, h2;
                hub.HubIsRootHub = false;
                hub.HubDeviceDesc = "External Hub";

                // Open a handle to the Host Controller 
                h = CreateFile(PortHubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                               IntPtr.Zero);
                if(h.ToInt32() != INVALID_HANDLE_VALUE)
                {
                    // Get the DevicePath for downstream hub 
                    int nBytesReturned;
                    UsbNodeConnectionName nodeName = new UsbNodeConnectionName();
                    nodeName.ConnectionIndex = PortPortNumber;
                    int nBytes = Marshal.SizeOf(nodeName);
                    IntPtr ptrNodeName = Marshal.AllocHGlobal(nBytes);
                    Marshal.StructureToPtr(nodeName, ptrNodeName, true);

                    // Use an IOCTL call to request the Node Name 
                    if(DeviceIoControl(h, IOCTL_USB_GET_NODE_CONNECTION_NAME, ptrNodeName, nBytes, ptrNodeName, nBytes,
                                       out nBytesReturned, IntPtr.Zero))
                    {
                        nodeName = (UsbNodeConnectionName)Marshal.PtrToStructure(ptrNodeName,
                                                                                    typeof(UsbNodeConnectionName));
                        hub.HubDevicePath = @"\\.\" + nodeName.NodeName;
                    }

                    // Now let's open the Hub (based upon the HubName we got above) 
                    h2 = CreateFile(hub.HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                                    IntPtr.Zero);
                    if(h2.ToInt32() != INVALID_HANDLE_VALUE)
                    {
                        UsbNodeInformation nodeInfo = new UsbNodeInformation();
                        nodeInfo.NodeType = (int)UsbHubNode.UsbHub;
                        nBytes = Marshal.SizeOf(nodeInfo);
                        IntPtr ptrNodeInfo = Marshal.AllocHGlobal(nBytes);
                        Marshal.StructureToPtr(nodeInfo, ptrNodeInfo, true);

                        // get the Hub Information 
                        if(DeviceIoControl(h2, IOCTL_USB_GET_NODE_INFORMATION, ptrNodeInfo, nBytes, ptrNodeInfo, nBytes,
                                           out nBytesReturned, IntPtr.Zero))
                        {
                            nodeInfo = (UsbNodeInformation)Marshal.PtrToStructure(ptrNodeInfo,
                                                                                    typeof(UsbNodeInformation));
                            hub.HubIsBusPowered = Convert.ToBoolean(nodeInfo.HubInformation.HubIsBusPowered);
                            hub.HubPortCount = nodeInfo.HubInformation.HubDescriptor.bNumberOfPorts;
                        }
                        Marshal.FreeHGlobal(ptrNodeInfo);
                        CloseHandle(h2);
                    }

                    // Fill in the missing Manufacture, Product, and SerialNumber values 
                    // values by just creating a Device instance and copying the values 
                    UsbDevice device = GetDevice();
                    hub.HubInstanceId = device.DeviceInstanceId;
                    hub.HubManufacturer = device.Manufacturer;
                    hub.HubProduct = device.Product;
                    hub.HubSerialNumber = device.SerialNumber;
                    hub.HubDriverKey = device.DriverKey;

                    Marshal.FreeHGlobal(ptrNodeName);
                    CloseHandle(h);
                }
                return hub;
            }
        }

        // 
        // The USB Device Class 
        // 
        internal class UsbDevice
        {
            internal int DevicePortNumber;
            internal string DeviceDriverKey, DeviceHubDevicePath, DeviceInstanceId, DeviceName;
            internal string DeviceManufacturer, DeviceProduct, DeviceSerialNumber;
            internal UsbDeviceDescriptor DeviceDescriptor;
            internal byte[] BinaryDeviceDescriptors;

            // a simple default constructor 
            internal UsbDevice()
            {
                DevicePortNumber = 0;
                DeviceHubDevicePath = "";
                DeviceDriverKey = "";
                DeviceManufacturer = "";
                DeviceProduct = "Unknown USB Device";
                DeviceSerialNumber = "";
                DeviceName = "";
                DeviceInstanceId = "";
                BinaryDeviceDescriptors = null;
            }

            // return Port Index of the Hub 
            internal int PortNumber
            {
                get { return DevicePortNumber; }
            }

            // return the Device Path of the Hub (the parent device) 
            internal string HubDevicePath
            {
                get { return DeviceHubDevicePath; }
            }

            // useful as a search key 
            internal string DriverKey
            {
                get { return DeviceDriverKey; }
            }

            // the device path of this device 
            internal string InstanceId
            {
                get { return DeviceInstanceId; }
            }

            // the friendly name 
            internal string Name
            {
                get { return DeviceName; }
            }

            internal string Manufacturer
            {
                get { return DeviceManufacturer; }
            }

            internal string Product
            {
                get { return DeviceProduct; }
            }

            internal string SerialNumber
            {
                get { return DeviceSerialNumber; }
            }

            internal byte[] BinaryDescriptors
            {
                get { return BinaryDeviceDescriptors; }
            }
        }

        // 
        // private function for finding a USB device's Description 
        // 
        static string GetDescriptionByKeyName(string driverKeyName)
        {
            string ans = "";
            string devEnum = REGSTR_KEY_USB;

            // Use the "enumerator form" of the SetupDiGetClassDevs API  
            // to generate a list of all USB devices 
            IntPtr h = SetupDiGetClassDevs(0, devEnum, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
            if(h.ToInt32() != INVALID_HANDLE_VALUE)
            {
                IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);
                string keyName;

                bool success;
                int i = 0;
                do
                {
                    // create a Device Interface Data structure 
                    SpDevinfoData da = new SpDevinfoData();
                    da.cbSize = Marshal.SizeOf(da);

                    // start the enumeration  
                    success = SetupDiEnumDeviceInfo(h, i, ref da);
                    if(success)
                    {
                        int requiredSize = 0;
                        int regType = REG_SZ;
                        keyName = "";

                        if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref regType, ptrBuf, BUFFER_SIZE,
                                                            ref requiredSize))
                        {
                            keyName = Marshal.PtrToStringAuto(ptrBuf);
                        }

                        // is it a match? 
                        if(keyName == driverKeyName)
                        {
                            if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DEVICEDESC, ref regType, ptrBuf,
                                                                BUFFER_SIZE, ref requiredSize))
                            {
                                ans = Marshal.PtrToStringAuto(ptrBuf);
                            }
                            break;
                        }
                    }

                    i++;
                }
                while(success);

                Marshal.FreeHGlobal(ptrBuf);
                SetupDiDestroyDeviceInfoList(h);
            }

            return ans;
        }

        // 
        // private function for finding a USB device's Instance ID 
        // 
        static string GetInstanceIdByKeyName(string driverKeyName)
        {
            string ans = "";
            string devEnum = REGSTR_KEY_USB;

            // Use the "enumerator form" of the SetupDiGetClassDevs API  
            // to generate a list of all USB devices 
            IntPtr h = SetupDiGetClassDevs(0, devEnum, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
            if(h.ToInt32() != INVALID_HANDLE_VALUE)
            {
                IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);
                string keyName;

                bool success;
                int i = 0;
                do
                {
                    // create a Device Interface Data structure 
                    SpDevinfoData da = new SpDevinfoData();
                    da.cbSize = Marshal.SizeOf(da);

                    // start the enumeration  
                    success = SetupDiEnumDeviceInfo(h, i, ref da);
                    if(success)
                    {
                        int requiredSize = 0;
                        int regType = REG_SZ;

                        keyName = "";
                        if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref regType, ptrBuf, BUFFER_SIZE,
                                                            ref requiredSize))
                        {
                            keyName = Marshal.PtrToStringAuto(ptrBuf);
                        }

                        // is it a match? 
                        if(keyName == driverKeyName)
                        {
                            int nBytes = BUFFER_SIZE;
                            StringBuilder sb = new StringBuilder(nBytes);
                            SetupDiGetDeviceInstanceId(h, ref da, sb, nBytes, out requiredSize);
                            ans = sb.ToString();
                            break;
                        }
                    }

                    i++;
                }
                while(success);

                Marshal.FreeHGlobal(ptrBuf);
                SetupDiDestroyDeviceInfoList(h);
            }

            return ans;
        }
    }
}