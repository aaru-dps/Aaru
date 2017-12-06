using System; 
using System.Collections.Generic; 
using System.Text; 
using System.Runtime.InteropServices; 
using System.ComponentModel; 
using System.Diagnostics;

// Copyright "Fort Hood TX", public domain, 2007
namespace DiscImageChef.Devices.Windows
{
    public partial class Usb
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
        enum USB_HUB_NODE
        {
            UsbHub,
            UsbMIParent
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
        enum USB_CONNECTION_STATUS
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
        enum USB_DEVICE_SPEED : byte
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
        struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public IntPtr DevInst;
            public IntPtr Reserved;
        }

        //typedef struct _SP_DEVICE_INTERFACE_DATA { 
        //  DWORD cbSize; 
        //  GUID InterfaceClassGuid; 
        //  DWORD Flags; 
        //  ULONG_PTR Reserved; 
        //} SP_DEVICE_INTERFACE_DATA,  *PSP_DEVICE_INTERFACE_DATA; 
        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        //typedef struct _SP_DEVICE_INTERFACE_DETAIL_DATA { 
        //  DWORD cbSize; 
        //  TCHAR DevicePath[ANYSIZE_ARRAY]; 
        //} SP_DEVICE_INTERFACE_DETAIL_DATA,  *PSP_DEVICE_INTERFACE_DETAIL_DATA; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] public string DevicePath;
        }

        //typedef struct _USB_HCD_DRIVERKEY_NAME { 
        //    ULONG ActualLength; 
        //    WCHAR DriverKeyName[1]; 
        //} USB_HCD_DRIVERKEY_NAME, *PUSB_HCD_DRIVERKEY_NAME; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct USB_HCD_DRIVERKEY_NAME
        {
            public int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] public string DriverKeyName;
        }

        //typedef struct _USB_ROOT_HUB_NAME { 
        //    ULONG  ActualLength; 
        //    WCHAR  RootHubName[1]; 
        //} USB_ROOT_HUB_NAME, *PUSB_ROOT_HUB_NAME; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct USB_ROOT_HUB_NAME
        {
            public int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] public string RootHubName;
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
        struct USB_HUB_DESCRIPTOR
        {
            public byte bDescriptorLength;
            public byte bDescriptorType;
            public byte bNumberOfPorts;
            public short wHubCharacteristics;
            public byte bPowerOnToPowerGood;
            public byte bHubControlCurrent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] bRemoveAndPowerMask;
        }

        //typedef struct _USB_HUB_INFORMATION { 
        //    USB_HUB_DESCRIPTOR HubDescriptor; 
        //    BOOLEAN HubIsBusPowered; 
        //} USB_HUB_INFORMATION, *PUSB_HUB_INFORMATION; 
        [StructLayout(LayoutKind.Sequential)]
        struct USB_HUB_INFORMATION
        {
            public USB_HUB_DESCRIPTOR HubDescriptor;
            public byte HubIsBusPowered;
        }

        //typedef struct _USB_NODE_INFORMATION { 
        //    USB_HUB_NODE  NodeType; 
        //    union { 
        //        USB_HUB_INFORMATION  HubInformation; 
        //        USB_MI_PARENT_INFORMATION  MiParentInformation; 
        //    } u; 
        //} USB_NODE_INFORMATION, *PUSB_NODE_INFORMATION; 
        [StructLayout(LayoutKind.Sequential)]
        struct USB_NODE_INFORMATION
        {
            public int NodeType;
            public USB_HUB_INFORMATION HubInformation; // Yeah, I'm assuming we'll just use the first form 
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
        struct USB_NODE_CONNECTION_INFORMATION_EX
        {
            public int ConnectionIndex;
            public USB_DEVICE_DESCRIPTOR DeviceDescriptor;
            public byte CurrentConfigurationValue;
            public byte Speed;
            public byte DeviceIsHub;
            public short DeviceAddress;
            public int NumberOfOpenPipes;

            public int ConnectionStatus;
            //public IntPtr PipeList; 
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
        internal struct USB_DEVICE_DESCRIPTOR
        {
            public byte bLength;
            public byte bDescriptorType;
            public short bcdUSB;
            public byte bDeviceClass;
            public byte bDeviceSubClass;
            public byte bDeviceProtocol;
            public byte bMaxPacketSize0;
            public short idVendor;
            public short idProduct;
            public short bcdDevice;
            public byte iManufacturer;
            public byte iProduct;
            public byte iSerialNumber;
            public byte bNumConfigurations;
        }

        //typedef struct _USB_STRING_DESCRIPTOR { 
        //    UCHAR bLength; 
        //    UCHAR bDescriptorType; 
        //    WCHAR bString[1]; 
        //} USB_STRING_DESCRIPTOR, *PUSB_STRING_DESCRIPTOR; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct USB_STRING_DESCRIPTOR
        {
            public byte bLength;
            public byte bDescriptorType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXIMUM_USB_STRING_LENGTH)] public string bString;
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
        struct USB_SETUP_PACKET
        {
            public byte bmRequest;
            public byte bRequest;
            public short wValue;
            public short wIndex;
            public short wLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct USB_DESCRIPTOR_REQUEST
        {
            public int ConnectionIndex;

            public USB_SETUP_PACKET SetupPacket;
            //public byte[] Data; 
        }

        //typedef struct _USB_NODE_CONNECTION_NAME { 
        //    ULONG  ConnectionIndex; 
        //    ULONG  ActualLength; 
        //    WCHAR  NodeName[1]; 
        //} USB_NODE_CONNECTION_NAME, *PUSB_NODE_CONNECTION_NAME; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct USB_NODE_CONNECTION_NAME
        {
            public int ConnectionIndex;
            public int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] public string NodeName;
        }

        //typedef struct _USB_NODE_CONNECTION_DRIVERKEY_NAME { 
        //    ULONG  ConnectionIndex; 
        //    ULONG  ActualLength; 
        //    WCHAR  DriverKeyName[1]; 
        //} USB_NODE_CONNECTION_DRIVERKEY_NAME, *PUSB_NODE_CONNECTION_DRIVERKEY_NAME; 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct USB_NODE_CONNECTION_DRIVERKEY_NAME // Yes, this is the same as the structure above... 
        {
            public int ConnectionIndex;
            public int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)] public string DriverKeyName;
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
            ref Guid ClassGuid,
            int Enumerator,
            IntPtr hwndParent,
            int Flags
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)] // 2nd form uses an Enumerator 
        static extern IntPtr SetupDiGetClassDevs(
            int ClassGuid,
            string Enumerator,
            IntPtr hwndParent,
            int Flags
        );

        //BOOL SetupDiEnumDeviceInterfaces( 
        //  HDEVINFO DeviceInfoSet, 
        //  PSP_DEVINFO_DATA DeviceInfoData, 
        //  const GUID* InterfaceClassGuid, 
        //  DWORD MemberIndex, 
        //  PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData 
        //); 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr DeviceInfoSet,
            IntPtr DeviceInfoData,
            ref Guid InterfaceClassGuid,
            int MemberIndex,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData
        );

        //BOOL SetupDiGetDeviceInterfaceDetail( 
        //  HDEVINFO DeviceInfoSet, 
        //  PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData, 
        //  PSP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, 
        //  DWORD DeviceInterfaceDetailDataSize, 
        //  PDWORD RequiredSize, 
        //  PSP_DEVINFO_DATA DeviceInfoData 
        //); 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
            ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData,
            int DeviceInterfaceDetailDataSize,
            ref int RequiredSize,
            ref SP_DEVINFO_DATA DeviceInfoData
        );

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
        static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            int iProperty,
            ref int PropertyRegDataType,
            IntPtr PropertyBuffer,
            int PropertyBufferSize,
            ref int RequiredSize
        );

        //BOOL SetupDiEnumDeviceInfo( 
        //  HDEVINFO DeviceInfoSet, 
        //  DWORD MemberIndex, 
        //  PSP_DEVINFO_DATA DeviceInfoData 
        //); 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            int MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData
        );

        //BOOL SetupDiDestroyDeviceInfoList( 
        //  HDEVINFO DeviceInfoSet 
        //); 
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr DeviceInfoSet
        );

        //WINSETUPAPI BOOL WINAPI  SetupDiGetDeviceInstanceId( 
        //    IN HDEVINFO  DeviceInfoSet, 
        //    IN PSP_DEVINFO_DATA  DeviceInfoData, 
        //    OUT PTSTR  DeviceInstanceId, 
        //    IN DWORD  DeviceInstanceIdSize, 
        //    OUT PDWORD  RequiredSize  OPTIONAL 
        //); 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiGetDeviceInstanceId(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            StringBuilder DeviceInstanceId,
            int DeviceInstanceIdSize,
            out int RequiredSize
        );

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
        static extern bool DeviceIoControl(
            IntPtr hDevice,
            int dwIoControlCode,
            IntPtr lpInBuffer,
            int nInBufferSize,
            IntPtr lpOutBuffer,
            int nOutBufferSize,
            out int lpBytesReturned,
            IntPtr lpOverlapped
        );

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
        static extern IntPtr CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        //BOOL CloseHandle( 
        //  HANDLE hObject 
        //); 
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CloseHandle(
            IntPtr hObject
        );

        #endregion

        // 
        // Return a list of USB Host Controllers 
        // 
        static public System.Collections.ObjectModel.ReadOnlyCollection<USBController> GetHostControllers()
        {
            List<USBController> HostList = new List<USBController>();
            Guid HostGUID = new Guid(GUID_DEVINTERFACE_HUBCONTROLLER);

            // We start at the "root" of the device tree and look for all 
            // devices that match the interface GUID of a Hub Controller 
            IntPtr h = SetupDiGetClassDevs(ref HostGUID, 0, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if(h.ToInt32() != INVALID_HANDLE_VALUE)
            {
                IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);
                bool Success;
                int i = 0;
                do
                {
                    USBController host = new USBController();
                    host.ControllerIndex = i;

                    // create a Device Interface Data structure 
                    SP_DEVICE_INTERFACE_DATA dia = new SP_DEVICE_INTERFACE_DATA();
                    dia.cbSize = Marshal.SizeOf(dia);

                    // start the enumeration  
                    Success = SetupDiEnumDeviceInterfaces(h, IntPtr.Zero, ref HostGUID, i, ref dia);
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
                            host.ControllerDevicePath = didd.DevicePath;

                            // get the Device Description and DriverKeyName 
                            int RequiredSize = 0;
                            int RegType = REG_SZ;

                            if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DEVICEDESC, ref RegType, ptrBuf,
                                BUFFER_SIZE, ref RequiredSize))
                            {
                                host.ControllerDeviceDesc = Marshal.PtrToStringAuto(ptrBuf);
                            }
                            if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref RegType, ptrBuf,
                                BUFFER_SIZE, ref RequiredSize))
                            {
                                host.ControllerDriverKeyName = Marshal.PtrToStringAuto(ptrBuf);
                            }
                        }
                        HostList.Add(host);
                    }
                    i++;
                } while(Success);

                Marshal.FreeHGlobal(ptrBuf);
                SetupDiDestroyDeviceInfoList(h);
            }

            // convert it into a Collection 
            return new System.Collections.ObjectModel.ReadOnlyCollection<USBController>(HostList);
        }

        // 
        // The USB Host Controller Class 
        // 
        public class USBController
        {
            internal int ControllerIndex;
            internal string ControllerDriverKeyName, ControllerDevicePath, ControllerDeviceDesc;

            // A simple default constructor 
            public USBController()
            {
                ControllerIndex = 0;
                ControllerDevicePath = "";
                ControllerDeviceDesc = "";
                ControllerDriverKeyName = "";
            }

            // Return the index of the instance 
            public int Index
            {
                get { return ControllerIndex; }
            }

            // Return the Device Path, such as "\\?\pci#ven_10de&dev_005a&subsys_815a1043&rev_a2#3&267a616a&0&58#{3abf6f2d-71c4-462a-8a92-1e6861e6af27}" 
            public string DevicePath
            {
                get { return ControllerDevicePath; }
            }

            // The DriverKeyName may be useful as a search key 
            public string DriverKeyName
            {
                get { return ControllerDriverKeyName; }
            }

            // Return the Friendly Name, such as "VIA USB Enhanced Host Controller" 
            public string Name
            {
                get { return ControllerDeviceDesc; }
            }

            // Return Root Hub for this Controller 
            public USBHub GetRootHub()
            {
                IntPtr h, h2;
                USBHub Root = new USBHub();
                Root.HubIsRootHub = true;
                Root.HubDeviceDesc = "Root Hub";

                // Open a handle to the Host Controller 
                h = CreateFile(ControllerDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                    IntPtr.Zero);
                if(h.ToInt32() != INVALID_HANDLE_VALUE)
                {
                    int nBytesReturned;
                    USB_ROOT_HUB_NAME HubName = new USB_ROOT_HUB_NAME();
                    int nBytes = Marshal.SizeOf(HubName);
                    IntPtr ptrHubName = Marshal.AllocHGlobal(nBytes);

                    // get the Hub Name 
                    if(DeviceIoControl(h, IOCTL_USB_GET_ROOT_HUB_NAME, ptrHubName, nBytes, ptrHubName, nBytes,
                        out nBytesReturned, IntPtr.Zero))
                    {
                        HubName = (USB_ROOT_HUB_NAME)Marshal.PtrToStructure(ptrHubName, typeof(USB_ROOT_HUB_NAME));
                        Root.HubDevicePath = @"\\.\" + HubName.RootHubName;
                    }

                    // TODO: Get DriverKeyName for Root Hub 

                    // Now let's open the Hub (based upon the HubName we got above) 
                    h2 = CreateFile(Root.HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                        IntPtr.Zero);
                    if(h2.ToInt32() != INVALID_HANDLE_VALUE)
                    {
                        USB_NODE_INFORMATION NodeInfo = new USB_NODE_INFORMATION();
                        NodeInfo.NodeType = (int)USB_HUB_NODE.UsbHub;
                        nBytes = Marshal.SizeOf(NodeInfo);
                        IntPtr ptrNodeInfo = Marshal.AllocHGlobal(nBytes);
                        Marshal.StructureToPtr(NodeInfo, ptrNodeInfo, true);

                        // get the Hub Information 
                        if(DeviceIoControl(h2, IOCTL_USB_GET_NODE_INFORMATION, ptrNodeInfo, nBytes, ptrNodeInfo, nBytes,
                            out nBytesReturned, IntPtr.Zero))
                        {
                            NodeInfo = (USB_NODE_INFORMATION)Marshal.PtrToStructure(ptrNodeInfo,
                                typeof(USB_NODE_INFORMATION));
                            Root.HubIsBusPowered = Convert.ToBoolean(NodeInfo.HubInformation.HubIsBusPowered);
                            Root.HubPortCount = NodeInfo.HubInformation.HubDescriptor.bNumberOfPorts;
                        }
                        Marshal.FreeHGlobal(ptrNodeInfo);
                        CloseHandle(h2);
                    }

                    Marshal.FreeHGlobal(ptrHubName);
                    CloseHandle(h);
                }
                return Root;
            }
        }

        // 
        // The Hub class 
        // 
        public class USBHub
        {
            internal int HubPortCount;
            internal string HubDriverKey, HubDevicePath, HubDeviceDesc;
            internal string HubManufacturer, HubProduct, HubSerialNumber, HubInstanceID;
            internal bool HubIsBusPowered, HubIsRootHub;

            // a simple default constructor 
            public USBHub()
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
                HubInstanceID = "";
            }

            // return Port Count 
            public int PortCount
            {
                get { return HubPortCount; }
            }

            // return the Device Path, such as "\\?\pci#ven_10de&dev_005a&subsys_815a1043&rev_a2#3&267a616a&0&58#{3abf6f2d-71c4-462a-8a92-1e6861e6af27}" 
            public string DevicePath
            {
                get { return HubDevicePath; }
            }

            // The DriverKey may be useful as a search key 
            public string DriverKey
            {
                get { return HubDriverKey; }
            }

            // return the Friendly Name, such as "VIA USB Enhanced Host Controller" 
            public string Name
            {
                get { return HubDeviceDesc; }
            }

            // the device path of this device 
            public string InstanceID
            {
                get { return HubInstanceID; }
            }

            // is is this a self-powered hub? 
            public bool IsBusPowered
            {
                get { return HubIsBusPowered; }
            }

            // is this a root hub? 
            public bool IsRootHub
            {
                get { return HubIsRootHub; }
            }

            public string Manufacturer
            {
                get { return HubManufacturer; }
            }

            public string Product
            {
                get { return HubProduct; }
            }

            public string SerialNumber
            {
                get { return HubSerialNumber; }
            }

            // return a list of the down stream ports 
            public System.Collections.ObjectModel.ReadOnlyCollection<USBPort> GetPorts()
            {
                List<USBPort> PortList = new List<USBPort>();

                // Open a handle to the Hub device 
                IntPtr h = CreateFile(HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                    IntPtr.Zero);
                if(h.ToInt32() != INVALID_HANDLE_VALUE)
                {
                    int nBytes = Marshal.SizeOf(typeof(USB_NODE_CONNECTION_INFORMATION_EX));
                    IntPtr ptrNodeConnection = Marshal.AllocHGlobal(nBytes);

                    // loop thru all of the ports on the hub 
                    // BTW: Ports are numbered starting at 1 
                    for(int i = 1; i <= HubPortCount; i++)
                    {
                        int nBytesReturned;
                        USB_NODE_CONNECTION_INFORMATION_EX NodeConnection = new USB_NODE_CONNECTION_INFORMATION_EX();
                        NodeConnection.ConnectionIndex = i;
                        Marshal.StructureToPtr(NodeConnection, ptrNodeConnection, true);

                        if(DeviceIoControl(h, IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX, ptrNodeConnection, nBytes,
                            ptrNodeConnection, nBytes, out nBytesReturned, IntPtr.Zero))
                        {
                            NodeConnection =
                                (USB_NODE_CONNECTION_INFORMATION_EX)Marshal.PtrToStructure(ptrNodeConnection,
                                    typeof(USB_NODE_CONNECTION_INFORMATION_EX));

                            // load up the USBPort class 
                            USBPort port = new USBPort();
                            port.PortPortNumber = i;
                            port.PortHubDevicePath = HubDevicePath;
                            USB_CONNECTION_STATUS Status = (USB_CONNECTION_STATUS)NodeConnection.ConnectionStatus;
                            port.PortStatus = Status.ToString();
                            USB_DEVICE_SPEED Speed = (USB_DEVICE_SPEED)NodeConnection.Speed;
                            port.PortSpeed = Speed.ToString();
                            port.PortIsDeviceConnected =
                                (NodeConnection.ConnectionStatus == (int)USB_CONNECTION_STATUS.DeviceConnected);
                            port.PortIsHub = Convert.ToBoolean(NodeConnection.DeviceIsHub);
                            port.PortDeviceDescriptor = NodeConnection.DeviceDescriptor;

                            // add it to the list 
                            PortList.Add(port);
                        }
                    }
                    Marshal.FreeHGlobal(ptrNodeConnection);
                    CloseHandle(h);
                }
                // convert it into a Collection 
                return new System.Collections.ObjectModel.ReadOnlyCollection<USBPort>(PortList);
            }
        }

        // 
        // The Port Class 
        // 
        public class USBPort
        {
            internal int PortPortNumber;
            internal string PortStatus, PortHubDevicePath, PortSpeed;
            internal bool PortIsHub, PortIsDeviceConnected;
            internal USB_DEVICE_DESCRIPTOR PortDeviceDescriptor;

            // a simple default constructor 
            public USBPort()
            {
                PortPortNumber = 0;
                PortStatus = "";
                PortHubDevicePath = "";
                PortSpeed = "";
                PortIsHub = false;
                PortIsDeviceConnected = false;
            }

            // return Port Index of the Hub 
            public int PortNumber
            {
                get { return PortPortNumber; }
            }

            // return the Device Path of the Hub 
            public string HubDevicePath
            {
                get { return PortHubDevicePath; }
            }

            // the status (see USB_CONNECTION_STATUS above) 
            public string Status
            {
                get { return PortStatus; }
            }

            // the speed of the connection (see USB_DEVICE_SPEED above) 
            public string Speed
            {
                get { return PortSpeed; }
            }

            // is this a downstream external hub? 
            public bool IsHub
            {
                get { return PortIsHub; }
            }

            // is anybody home? 
            public bool IsDeviceConnected
            {
                get { return PortIsDeviceConnected; }
            }

            // return a down stream external hub 
            public USBDevice GetDevice()
            {
                if(!PortIsDeviceConnected)
                {
                    return null;
                }
                USBDevice Device = new USBDevice();

                // Copy over some values from the Port class 
                // Ya know, I've given some thought about making Device a derived class... 
                Device.DevicePortNumber = PortPortNumber;
                Device.DeviceHubDevicePath = PortHubDevicePath;
                Device.DeviceDescriptor = PortDeviceDescriptor;

                // Open a handle to the Hub device 
                IntPtr h = CreateFile(PortHubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                    IntPtr.Zero);
                if(h.ToInt32() != INVALID_HANDLE_VALUE)
                {
                    int nBytesReturned;
                    int nBytes = BUFFER_SIZE;
                    // We use this to zero fill a buffer 
                    string NullString = new string((char)0, BUFFER_SIZE / Marshal.SystemDefaultCharSize);

                    // The iManufacturer, iProduct and iSerialNumber entries in the 
                    // Device Descriptor are really just indexes.  So, we have to  
                    // request a String Descriptor to get the values for those strings. 

                    if(PortDeviceDescriptor.iManufacturer > 0)
                    {
                        // build a request for string descriptor 
                        USB_DESCRIPTOR_REQUEST Request = new USB_DESCRIPTOR_REQUEST();
                        Request.ConnectionIndex = PortPortNumber;
                        Request.SetupPacket.wValue =
                            (short)((USB_STRING_DESCRIPTOR_TYPE << 8) + PortDeviceDescriptor.iManufacturer);
                        Request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(Request));
                        Request.SetupPacket.wIndex = 0x409; // Language Code 
                        // Geez, I wish C# had a Marshal.MemSet() method 
                        IntPtr ptrRequest = Marshal.StringToHGlobalAuto(NullString);
                        Marshal.StructureToPtr(Request, ptrRequest, true);

                        // Use an IOCTL call to request the String Descriptor 
                        if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes,
                            ptrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                        {
                            // The location of the string descriptor is immediately after 
                            // the Request structure.  Because this location is not "covered" 
                            // by the structure allocation, we're forced to zero out this 
                            // chunk of memory by using the StringToHGlobalAuto() hack above 
                            IntPtr ptrStringDesc = new IntPtr(ptrRequest.ToInt32() + Marshal.SizeOf(Request));
                            USB_STRING_DESCRIPTOR StringDesc =
                                (USB_STRING_DESCRIPTOR)Marshal.PtrToStructure(ptrStringDesc,
                                    typeof(USB_STRING_DESCRIPTOR));
                            Device.DeviceManufacturer = StringDesc.bString;
                        }
                        Marshal.FreeHGlobal(ptrRequest);
                    }
                    if(PortDeviceDescriptor.iProduct > 0)
                    {
                        // build a request for string descriptor 
                        USB_DESCRIPTOR_REQUEST Request = new USB_DESCRIPTOR_REQUEST();
                        Request.ConnectionIndex = PortPortNumber;
                        Request.SetupPacket.wValue =
                            (short)((USB_STRING_DESCRIPTOR_TYPE << 8) + PortDeviceDescriptor.iProduct);
                        Request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(Request));
                        Request.SetupPacket.wIndex = 0x409; // Language Code 
                        // Geez, I wish C# had a Marshal.MemSet() method 
                        IntPtr ptrRequest = Marshal.StringToHGlobalAuto(NullString);
                        Marshal.StructureToPtr(Request, ptrRequest, true);

                        // Use an IOCTL call to request the String Descriptor 
                        if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes,
                            ptrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                        {
                            // the location of the string descriptor is immediately after the Request structure 
                            IntPtr ptrStringDesc = new IntPtr(ptrRequest.ToInt32() + Marshal.SizeOf(Request));
                            USB_STRING_DESCRIPTOR StringDesc =
                                (USB_STRING_DESCRIPTOR)Marshal.PtrToStructure(ptrStringDesc,
                                    typeof(USB_STRING_DESCRIPTOR));
                            Device.DeviceProduct = StringDesc.bString;
                        }
                        Marshal.FreeHGlobal(ptrRequest);
                    }
                    if(PortDeviceDescriptor.iSerialNumber > 0)
                    {
                        // build a request for string descriptor 
                        USB_DESCRIPTOR_REQUEST Request = new USB_DESCRIPTOR_REQUEST();
                        Request.ConnectionIndex = PortPortNumber;
                        Request.SetupPacket.wValue =
                            (short)((USB_STRING_DESCRIPTOR_TYPE << 8) + PortDeviceDescriptor.iSerialNumber);
                        Request.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(Request));
                        Request.SetupPacket.wIndex = 0x409; // Language Code 
                        // Geez, I wish C# had a Marshal.MemSet() method 
                        IntPtr ptrRequest = Marshal.StringToHGlobalAuto(NullString);
                        Marshal.StructureToPtr(Request, ptrRequest, true);

                        // Use an IOCTL call to request the String Descriptor 
                        if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, ptrRequest, nBytes,
                            ptrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                        {
                            // the location of the string descriptor is immediately after the Request structure 
                            IntPtr ptrStringDesc = new IntPtr(ptrRequest.ToInt32() + Marshal.SizeOf(Request));
                            USB_STRING_DESCRIPTOR StringDesc =
                                (USB_STRING_DESCRIPTOR)Marshal.PtrToStructure(ptrStringDesc,
                                    typeof(USB_STRING_DESCRIPTOR));
                            Device.DeviceSerialNumber = StringDesc.bString;
                        }
                        Marshal.FreeHGlobal(ptrRequest);
                    }

                    // build a request for configuration descriptor 
                    USB_DESCRIPTOR_REQUEST dcrRequest = new USB_DESCRIPTOR_REQUEST();
                    dcrRequest.ConnectionIndex = PortPortNumber;
                    dcrRequest.SetupPacket.wValue = (short)((USB_CONFIGURATION_DESCRIPTOR_TYPE << 8));
                    dcrRequest.SetupPacket.wLength = (short)(nBytes - Marshal.SizeOf(dcrRequest));
                    dcrRequest.SetupPacket.wIndex = 0; 
                    // Geez, I wish C# had a Marshal.MemSet() method 
                    IntPtr dcrPtrRequest = Marshal.StringToHGlobalAuto(NullString);
                    Marshal.StructureToPtr(dcrRequest, dcrPtrRequest, true);

                    // Use an IOCTL call to request the String Descriptor 
                    if(DeviceIoControl(h, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, dcrPtrRequest, nBytes,
                        dcrPtrRequest, nBytes, out nBytesReturned, IntPtr.Zero))
                    {
                        IntPtr ptrStringDesc = new IntPtr(dcrPtrRequest.ToInt32() + Marshal.SizeOf(dcrRequest));
                        Device.BinaryDeviceDescriptors = new byte[nBytesReturned];
                        Marshal.Copy(ptrStringDesc, Device.BinaryDeviceDescriptors, 0, nBytesReturned);
                    }
                    Marshal.FreeHGlobal(dcrPtrRequest);

                    // Get the Driver Key Name (usefull in locating a device) 
                    USB_NODE_CONNECTION_DRIVERKEY_NAME DriverKey = new USB_NODE_CONNECTION_DRIVERKEY_NAME();
                    DriverKey.ConnectionIndex = PortPortNumber;
                    nBytes = Marshal.SizeOf(DriverKey);
                    IntPtr ptrDriverKey = Marshal.AllocHGlobal(nBytes);
                    Marshal.StructureToPtr(DriverKey, ptrDriverKey, true);

                    // Use an IOCTL call to request the Driver Key Name 
                    if(DeviceIoControl(h, IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME, ptrDriverKey, nBytes,
                        ptrDriverKey, nBytes, out nBytesReturned, IntPtr.Zero))
                    {
                        DriverKey = (USB_NODE_CONNECTION_DRIVERKEY_NAME)Marshal.PtrToStructure(ptrDriverKey,
                            typeof(USB_NODE_CONNECTION_DRIVERKEY_NAME));
                        Device.DeviceDriverKey = DriverKey.DriverKeyName;

                        // use the DriverKeyName to get the Device Description and Instance ID 
                        Device.DeviceName = GetDescriptionByKeyName(Device.DeviceDriverKey);
                        Device.DeviceInstanceID = GetInstanceIDByKeyName(Device.DeviceDriverKey);
                    }
                    Marshal.FreeHGlobal(ptrDriverKey);
                    CloseHandle(h);
                }
                return Device;
            }

            // return a down stream external hub 
            public USBHub GetHub()
            {
                if(!PortIsHub)
                {
                    return null;
                }
                USBHub Hub = new USBHub();
                IntPtr h, h2;
                Hub.HubIsRootHub = false;
                Hub.HubDeviceDesc = "External Hub";

                // Open a handle to the Host Controller 
                h = CreateFile(PortHubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                    IntPtr.Zero);
                if(h.ToInt32() != INVALID_HANDLE_VALUE)
                {
                    // Get the DevicePath for downstream hub 
                    int nBytesReturned;
                    USB_NODE_CONNECTION_NAME NodeName = new USB_NODE_CONNECTION_NAME();
                    NodeName.ConnectionIndex = PortPortNumber;
                    int nBytes = Marshal.SizeOf(NodeName);
                    IntPtr ptrNodeName = Marshal.AllocHGlobal(nBytes);
                    Marshal.StructureToPtr(NodeName, ptrNodeName, true);

                    // Use an IOCTL call to request the Node Name 
                    if(DeviceIoControl(h, IOCTL_USB_GET_NODE_CONNECTION_NAME, ptrNodeName, nBytes, ptrNodeName, nBytes,
                        out nBytesReturned, IntPtr.Zero))
                    {
                        NodeName = (USB_NODE_CONNECTION_NAME)Marshal.PtrToStructure(ptrNodeName,
                            typeof(USB_NODE_CONNECTION_NAME));
                        Hub.HubDevicePath = @"\\.\" + NodeName.NodeName;
                    }

                    // Now let's open the Hub (based upon the HubName we got above) 
                    h2 = CreateFile(Hub.HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0,
                        IntPtr.Zero);
                    if(h2.ToInt32() != INVALID_HANDLE_VALUE)
                    {
                        USB_NODE_INFORMATION NodeInfo = new USB_NODE_INFORMATION();
                        NodeInfo.NodeType = (int)USB_HUB_NODE.UsbHub;
                        nBytes = Marshal.SizeOf(NodeInfo);
                        IntPtr ptrNodeInfo = Marshal.AllocHGlobal(nBytes);
                        Marshal.StructureToPtr(NodeInfo, ptrNodeInfo, true);

                        // get the Hub Information 
                        if(DeviceIoControl(h2, IOCTL_USB_GET_NODE_INFORMATION, ptrNodeInfo, nBytes, ptrNodeInfo, nBytes,
                            out nBytesReturned, IntPtr.Zero))
                        {
                            NodeInfo = (USB_NODE_INFORMATION)Marshal.PtrToStructure(ptrNodeInfo,
                                typeof(USB_NODE_INFORMATION));
                            Hub.HubIsBusPowered = Convert.ToBoolean(NodeInfo.HubInformation.HubIsBusPowered);
                            Hub.HubPortCount = NodeInfo.HubInformation.HubDescriptor.bNumberOfPorts;
                        }
                        Marshal.FreeHGlobal(ptrNodeInfo);
                        CloseHandle(h2);
                    }

                    // Fill in the missing Manufacture, Product, and SerialNumber values 
                    // values by just creating a Device instance and copying the values 
                    USBDevice Device = GetDevice();
                    Hub.HubInstanceID = Device.DeviceInstanceID;
                    Hub.HubManufacturer = Device.Manufacturer;
                    Hub.HubProduct = Device.Product;
                    Hub.HubSerialNumber = Device.SerialNumber;
                    Hub.HubDriverKey = Device.DriverKey;

                    Marshal.FreeHGlobal(ptrNodeName);
                    CloseHandle(h);
                }
                return Hub;
            }
        }

        // 
        // The USB Device Class 
        // 
        public class USBDevice
        {
            internal int DevicePortNumber;
            internal string DeviceDriverKey, DeviceHubDevicePath, DeviceInstanceID, DeviceName;
            internal string DeviceManufacturer, DeviceProduct, DeviceSerialNumber;
            internal USB_DEVICE_DESCRIPTOR DeviceDescriptor;
            internal byte[] BinaryDeviceDescriptors;

            // a simple default constructor 
            public USBDevice()
            {
                DevicePortNumber = 0;
                DeviceHubDevicePath = "";
                DeviceDriverKey = "";
                DeviceManufacturer = "";
                DeviceProduct = "Unknown USB Device";
                DeviceSerialNumber = "";
                DeviceName = "";
                DeviceInstanceID = "";
                BinaryDeviceDescriptors = null;
            }

            // return Port Index of the Hub 
            public int PortNumber
            {
                get { return DevicePortNumber; }
            }

            // return the Device Path of the Hub (the parent device) 
            public string HubDevicePath
            {
                get { return DeviceHubDevicePath; }
            }

            // useful as a search key 
            public string DriverKey
            {
                get { return DeviceDriverKey; }
            }

            // the device path of this device 
            public string InstanceID
            {
                get { return DeviceInstanceID; }
            }

            // the friendly name 
            public string Name
            {
                get { return DeviceName; }
            }

            public string Manufacturer
            {
                get { return DeviceManufacturer; }
            }

            public string Product
            {
                get { return DeviceProduct; }
            }

            public string SerialNumber
            {
                get { return DeviceSerialNumber; }
            }
            
            public byte[] BinaryDescriptors
            {
                get { return BinaryDeviceDescriptors; }
            }
        }

        // 
        // private function for finding a USB device's Description 
        // 
        static string GetDescriptionByKeyName(string DriverKeyName)
        {
            string ans = "";
            string DevEnum = REGSTR_KEY_USB;

            // Use the "enumerator form" of the SetupDiGetClassDevs API  
            // to generate a list of all USB devices 
            IntPtr h = SetupDiGetClassDevs(0, DevEnum, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
            if(h.ToInt32() != INVALID_HANDLE_VALUE)
            {
                IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);
                string KeyName;

                bool Success;
                int i = 0;
                do
                {
                    // create a Device Interface Data structure 
                    SP_DEVINFO_DATA da = new SP_DEVINFO_DATA();
                    da.cbSize = Marshal.SizeOf(da);

                    // start the enumeration  
                    Success = SetupDiEnumDeviceInfo(h, i, ref da);
                    if(Success)
                    {
                        int RequiredSize = 0;
                        int RegType = REG_SZ;
                        KeyName = "";

                        if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref RegType, ptrBuf, BUFFER_SIZE,
                            ref RequiredSize))
                        {
                            KeyName = Marshal.PtrToStringAuto(ptrBuf);
                        }

                        // is it a match? 
                        if(KeyName == DriverKeyName)
                        {
                            if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DEVICEDESC, ref RegType, ptrBuf,
                                BUFFER_SIZE, ref RequiredSize))
                            {
                                ans = Marshal.PtrToStringAuto(ptrBuf);
                            }
                            break;
                        }
                    }
                    i++;
                } while(Success);

                Marshal.FreeHGlobal(ptrBuf);
                SetupDiDestroyDeviceInfoList(h);
            }
            return ans;
        }

        // 
        // private function for finding a USB device's Instance ID 
        // 
        static string GetInstanceIDByKeyName(string DriverKeyName)
        {
            string ans = "";
            string DevEnum = REGSTR_KEY_USB;

            // Use the "enumerator form" of the SetupDiGetClassDevs API  
            // to generate a list of all USB devices 
            IntPtr h = SetupDiGetClassDevs(0, DevEnum, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
            if(h.ToInt32() != INVALID_HANDLE_VALUE)
            {
                IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);
                string KeyName;

                bool Success;
                int i = 0;
                do
                {
                    // create a Device Interface Data structure 
                    SP_DEVINFO_DATA da = new SP_DEVINFO_DATA();
                    da.cbSize = Marshal.SizeOf(da);

                    // start the enumeration  
                    Success = SetupDiEnumDeviceInfo(h, i, ref da);
                    if(Success)
                    {
                        int RequiredSize = 0;
                        int RegType = REG_SZ;

                        KeyName = "";
                        if(SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref RegType, ptrBuf, BUFFER_SIZE,
                            ref RequiredSize))
                        {
                            KeyName = Marshal.PtrToStringAuto(ptrBuf);
                        }

                        // is it a match? 
                        if(KeyName == DriverKeyName)
                        {
                            int nBytes = BUFFER_SIZE;
                            StringBuilder sb = new StringBuilder(nBytes);
                            SetupDiGetDeviceInstanceId(h, ref da, sb, nBytes, out RequiredSize);
                            ans = sb.ToString();
                            break;
                        }
                    }
                    i++;
                } while(Success);

                Marshal.FreeHGlobal(ptrBuf);
                SetupDiDestroyDeviceInfoList(h);
            }
            return ans;
        }
    }
} 
