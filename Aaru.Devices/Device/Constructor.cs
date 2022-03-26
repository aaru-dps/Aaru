// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constructor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prepares a device for direct access.
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

namespace Aaru.Devices;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decoders.SecureDigital;
using Aaru.Devices.Linux;
using Aaru.Devices.Windows;
using Aaru.Helpers;
using Microsoft.Win32.SafeHandles;
using Extern = Aaru.Devices.Windows.Extern;
using FileAccess = Aaru.Devices.Windows.FileAccess;
using FileAttributes = Aaru.Devices.Windows.FileAttributes;
using FileMode = Aaru.Devices.Windows.FileMode;
using FileShare = Aaru.Devices.Windows.FileShare;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;
using Marshal = System.Runtime.InteropServices.Marshal;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using VendorString = Aaru.Decoders.SecureDigital.VendorString;

/// <summary>Implements a device or media containing drive</summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "UnusedMember.Global"),
 SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public sealed partial class Device
{
    /// <summary>Opens the device for sending direct commands</summary>
    /// <param name="devicePath">Device path</param>
    public static Device Create(string devicePath)
    {
        var dev = new Device();

        dev.PlatformId  = DetectOS.GetRealPlatformID();
        dev.Timeout     = 15;
        dev.Error       = false;
        dev.IsRemovable = false;
        dev._devicePath = devicePath;

        Uri aaruUri;

        try
        {
            aaruUri = new Uri(devicePath);
        }
        catch(Exception)
        {
            // Ignore, treat as local path below
            aaruUri = null;
        }

        if(aaruUri?.Scheme is "dic" or "aaru")
        {
            devicePath = aaruUri.AbsolutePath;

            if(devicePath.StartsWith('/'))
                devicePath = devicePath.Substring(1);

            if(devicePath.StartsWith("dev", StringComparison.Ordinal))
                devicePath = $"/{devicePath}";

            dev._remote = new Remote.Remote(aaruUri);

            dev.Error     = !dev._remote.Open(devicePath, out int errno);
            dev.LastError = errno;
        }
        else
            switch(dev.PlatformId)
            {
                case PlatformID.Win32NT:
                {
                    dev.FileHandle = Extern.CreateFile(devicePath, FileAccess.GenericRead | FileAccess.GenericWrite,
                                                       FileShare.Read | FileShare.Write, IntPtr.Zero,
                                                       FileMode.OpenExisting, FileAttributes.Normal, IntPtr.Zero);

                    if(((SafeFileHandle)dev.FileHandle).IsInvalid)
                    {
                        dev.Error     = true;
                        dev.LastError = Marshal.GetLastWin32Error();
                    }

                    break;
                }
                case PlatformID.Linux:
                {
                    dev.FileHandle =
                        Linux.Extern.open(devicePath,
                                          FileFlags.ReadWrite | FileFlags.NonBlocking | FileFlags.CreateNew);

                    if((int)dev.FileHandle < 0)
                    {
                        dev.LastError = Marshal.GetLastWin32Error();

                        if(dev.LastError is 13 or 30) // EACCES or EROFS
                        {
                            dev.FileHandle = Linux.Extern.open(devicePath, FileFlags.Readonly | FileFlags.NonBlocking);

                            if((int)dev.FileHandle < 0)
                            {
                                dev.Error     = true;
                                dev.LastError = Marshal.GetLastWin32Error();
                            }
                        }
                        else
                            dev.Error = true;

                        dev.LastError = Marshal.GetLastWin32Error();
                    }

                    break;
                }
                default: throw new DeviceException($"Platform {dev.PlatformId} not supported.");
            }

        if(dev.Error)
            throw new DeviceException(dev.LastError);

        // Seems ioctl(2) does not allow the atomicity needed
        if(dev._remote is null)
        {
            if(dev.PlatformId == PlatformID.Linux)
                _readMultipleBlockCannotSetBlockCount = true;
        }
        else if(dev._remote.ServerOperatingSystem == "Linux")
            _readMultipleBlockCannotSetBlockCount = true;

        dev.Type     = DeviceType.Unknown;
        dev.ScsiType = PeripheralDeviceTypes.UnknownDevice;

        byte[] ataBuf;
        byte[] inqBuf = null;

        if(dev.Error)
            throw new DeviceException(dev.LastError);

        var scsiSense = true;

        if(dev._remote is null)

            // Windows is answering SCSI INQUIRY for all device types so it needs to be detected first
            switch(dev.PlatformId)
            {
                case PlatformID.Win32NT:
                    var query = new StoragePropertyQuery();
                    query.PropertyId           = StoragePropertyId.Device;
                    query.QueryType            = StorageQueryType.Standard;
                    query.AdditionalParameters = new byte[1];

                    IntPtr descriptorPtr = Marshal.AllocHGlobal(1000);
                    var    descriptorB   = new byte[1000];

                    uint returned = 0;
                    var  error    = 0;

                    bool hasError = !Extern.DeviceIoControlStorageQuery((SafeFileHandle)dev.FileHandle,
                                                                        WindowsIoctl.IoctlStorageQueryProperty,
                                                                        ref query, (uint)Marshal.SizeOf(query),
                                                                        descriptorPtr, 1000, ref returned, IntPtr.Zero);

                    if(hasError)
                        error = Marshal.GetLastWin32Error();

                    Marshal.Copy(descriptorPtr, descriptorB, 0, 1000);

                    if(!hasError &&
                       error == 0)
                    {
                        var descriptor = new StorageDeviceDescriptor
                        {
                            Version               = BitConverter.ToUInt32(descriptorB, 0),
                            Size                  = BitConverter.ToUInt32(descriptorB, 4),
                            DeviceType            = descriptorB[8],
                            DeviceTypeModifier    = descriptorB[9],
                            RemovableMedia        = descriptorB[10] > 0,
                            CommandQueueing       = descriptorB[11] > 0,
                            VendorIdOffset        = BitConverter.ToInt32(descriptorB, 12),
                            ProductIdOffset       = BitConverter.ToInt32(descriptorB, 16),
                            ProductRevisionOffset = BitConverter.ToInt32(descriptorB, 20),
                            SerialNumberOffset    = BitConverter.ToInt32(descriptorB, 24),
                            BusType               = (StorageBusType)BitConverter.ToUInt32(descriptorB, 28),
                            RawPropertiesLength   = BitConverter.ToUInt32(descriptorB, 32)
                        };

                        descriptor.RawDeviceProperties = new byte[descriptor.RawPropertiesLength];

                        Array.Copy(descriptorB, 36, descriptor.RawDeviceProperties, 0, descriptor.RawPropertiesLength);

                        switch(descriptor.BusType)
                        {
                            case StorageBusType.SCSI:
                            case StorageBusType.SSA:
                            case StorageBusType.Fibre:
                            case StorageBusType.iSCSI:
                            case StorageBusType.SAS:
                                dev.Type = DeviceType.SCSI;

                                break;
                            case StorageBusType.FireWire:
                                dev.IsFireWire = true;
                                dev.Type       = DeviceType.SCSI;

                                break;
                            case StorageBusType.USB:
                                dev.IsUsb = true;
                                dev.Type  = DeviceType.SCSI;

                                break;
                            case StorageBusType.ATAPI:
                                dev.Type = DeviceType.ATAPI;

                                break;
                            case StorageBusType.ATA:
                            case StorageBusType.SATA:
                                dev.Type = DeviceType.ATA;

                                break;
                            case StorageBusType.MultiMediaCard:
                                dev.Type = DeviceType.MMC;

                                break;
                            case StorageBusType.SecureDigital:
                                dev.Type = DeviceType.SecureDigital;

                                break;
                            case StorageBusType.NVMe:
                                dev.Type = DeviceType.NVMe;

                                break;
                        }

                        switch(dev.Type)
                        {
                            case DeviceType.SCSI:
                            case DeviceType.ATAPI:
                                scsiSense = dev.ScsiInquiry(out inqBuf, out _);

                                break;
                            case DeviceType.ATA:
                                bool atapiSense = dev.AtapiIdentify(out ataBuf, out _);

                                if(!atapiSense)
                                {
                                    dev.Type = DeviceType.ATAPI;
                                    Identify.IdentifyDevice? ataid = Identify.Decode(ataBuf);

                                    if(ataid.HasValue)
                                        scsiSense = dev.ScsiInquiry(out inqBuf, out _);
                                }
                                else
                                    dev.Manufacturer = "ATA";

                                break;
                        }
                    }

                    Marshal.FreeHGlobal(descriptorPtr);

                    if(Windows.Command.IsSdhci((SafeFileHandle)dev.FileHandle))
                    {
                        var sdBuffer = new byte[16];

                        dev.LastError = Windows.Command.SendMmcCommand((SafeFileHandle)dev.FileHandle,
                                                                       MmcCommands.SendCsd, false, false,
                                                                       MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 |
                                                                       MmcFlags.CommandAc, 0, 16, 1, ref sdBuffer,
                                                                       out _, out _, out bool sense);

                        if(!sense)
                        {
                            dev._cachedCsd = new byte[16];
                            Array.Copy(sdBuffer, 0, dev._cachedCsd, 0, 16);
                        }

                        sdBuffer = new byte[16];

                        dev.LastError = Windows.Command.SendMmcCommand((SafeFileHandle)dev.FileHandle,
                                                                       MmcCommands.SendCid, false, false,
                                                                       MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 |
                                                                       MmcFlags.CommandAc, 0, 16, 1, ref sdBuffer,
                                                                       out _, out _, out sense);

                        if(!sense)
                        {
                            dev._cachedCid = new byte[16];
                            Array.Copy(sdBuffer, 0, dev._cachedCid, 0, 16);
                        }

                        sdBuffer = new byte[8];

                        dev.LastError = Windows.Command.SendMmcCommand((SafeFileHandle)dev.FileHandle,
                                                                       (MmcCommands)SecureDigitalCommands.SendScr,
                                                                       false, true,
                                                                       MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 |
                                                                       MmcFlags.CommandAdtc, 0, 8, 1, ref sdBuffer,
                                                                       out _, out _, out sense);

                        if(!sense)
                        {
                            dev._cachedScr = new byte[8];
                            Array.Copy(sdBuffer, 0, dev._cachedScr, 0, 8);
                        }

                        sdBuffer = new byte[4];

                        dev.LastError = Windows.Command.SendMmcCommand((SafeFileHandle)dev.FileHandle,
                                                                       dev._cachedScr != null
                                                                           ? (MmcCommands)SecureDigitalCommands.
                                                                               SendOperatingCondition
                                                                           : MmcCommands.SendOpCond, false, true,
                                                                       MmcFlags.ResponseSpiR3 | MmcFlags.ResponseR3 |
                                                                       MmcFlags.CommandBcr, 0, 4, 1, ref sdBuffer,
                                                                       out _, out _, out sense);

                        if(!sense)
                        {
                            dev._cachedScr = new byte[4];
                            Array.Copy(sdBuffer, 0, dev._cachedScr, 0, 4);
                        }
                    }

                    break;
                case PlatformID.Linux:
                    if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
                       devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
                       devicePath.StartsWith("/dev/st", StringComparison.Ordinal) ||
                       devicePath.StartsWith("/dev/sg", StringComparison.Ordinal))
                        scsiSense = dev.ScsiInquiry(out inqBuf, out _);

                    // MultiMediaCard and SecureDigital go here
                    else if(devicePath.StartsWith("/dev/mmcblk", StringComparison.Ordinal))
                    {
                        string devPath = devicePath.Substring(5);

                        if(File.Exists("/sys/block/" + devPath + "/device/csd"))
                        {
                            int len = ConvertFromFileHexAscii("/sys/block/" + devPath + "/device/csd",
                                                              out dev._cachedCsd);

                            if(len == 0)
                                dev._cachedCsd = null;
                        }

                        if(File.Exists("/sys/block/" + devPath + "/device/cid"))
                        {
                            int len = ConvertFromFileHexAscii("/sys/block/" + devPath + "/device/cid",
                                                              out dev._cachedCid);

                            if(len == 0)
                                dev._cachedCid = null;
                        }

                        if(File.Exists("/sys/block/" + devPath + "/device/scr"))
                        {
                            int len = ConvertFromFileHexAscii("/sys/block/" + devPath + "/device/scr",
                                                              out dev._cachedScr);

                            if(len == 0)
                                dev._cachedScr = null;
                        }

                        if(File.Exists("/sys/block/" + devPath + "/device/ocr"))
                        {
                            int len = ConvertFromFileHexAscii("/sys/block/" + devPath + "/device/ocr",
                                                              out dev._cachedOcr);

                            if(len == 0)
                                dev._cachedOcr = null;
                        }
                    }

                    break;
                default:
                    scsiSense = dev.ScsiInquiry(out inqBuf, out _);

                    break;
            }
        else
        {
            dev.Type = dev._remote.GetDeviceType();

            switch(dev.Type)
            {
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    scsiSense = dev.ScsiInquiry(out inqBuf, out _);

                    break;
                case DeviceType.SecureDigital:
                case DeviceType.MMC:
                    if(!dev._remote.GetSdhciRegisters(out dev._cachedCsd, out dev._cachedCid, out dev._cachedOcr,
                                                      out dev._cachedScr))
                    {
                        dev.Type     = DeviceType.SCSI;
                        dev.ScsiType = PeripheralDeviceTypes.DirectAccess;
                    }

                    break;
            }
        }

        #region SecureDigital / MultiMediaCard
        if(dev._cachedCid != null)
        {
            dev.ScsiType    = PeripheralDeviceTypes.DirectAccess;
            dev.IsRemovable = false;

            if(dev._cachedScr != null)
            {
                dev.Type = DeviceType.SecureDigital;
                CID decoded = Decoders.DecodeCID(dev._cachedCid);
                dev.Manufacturer = VendorString.Prettify(decoded.Manufacturer);
                dev.Model        = decoded.ProductName;

                dev.FirmwareRevision =
                    $"{(decoded.ProductRevision & 0xF0) >> 4:X2}.{decoded.ProductRevision & 0x0F:X2}";

                dev.Serial = $"{decoded.ProductSerialNumber}";
            }
            else
            {
                dev.Type = DeviceType.MMC;
                Aaru.Decoders.MMC.CID decoded = Aaru.Decoders.MMC.Decoders.DecodeCID(dev._cachedCid);
                dev.Manufacturer = Aaru.Decoders.MMC.VendorString.Prettify(decoded.Manufacturer);
                dev.Model        = decoded.ProductName;

                dev.FirmwareRevision =
                    $"{(decoded.ProductRevision & 0xF0) >> 4:X2}.{decoded.ProductRevision & 0x0F:X2}";

                dev.Serial = $"{decoded.ProductSerialNumber}";
            }

            return dev;
        }
        #endregion SecureDigital / MultiMediaCard

        #region USB
        if(dev._remote is null)
            switch(dev.PlatformId)
            {
                case PlatformID.Linux:
                    if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
                       devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
                       devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                    {
                        string devPath = devicePath.Substring(5);

                        if(Directory.Exists("/sys/block/" + devPath))
                        {
                            string resolvedLink = Linux.Command.ReadLink("/sys/block/" + devPath);

                            if(!string.IsNullOrEmpty(resolvedLink))
                            {
                                resolvedLink = "/sys" + resolvedLink.Substring(2);

                                while(resolvedLink.Contains("usb"))
                                {
                                    resolvedLink = Path.GetDirectoryName(resolvedLink);

                                    if(!File.Exists(resolvedLink + "/descriptors") ||
                                       !File.Exists(resolvedLink + "/idProduct")   ||
                                       !File.Exists(resolvedLink + "/idVendor"))
                                        continue;

                                    var usbFs = new FileStream(resolvedLink + "/descriptors", System.IO.FileMode.Open,
                                                               System.IO.FileAccess.Read);

                                    var usbBuf   = new byte[65536];
                                    int usbCount = usbFs.Read(usbBuf, 0, 65536);
                                    dev.UsbDescriptors = new byte[usbCount];
                                    Array.Copy(usbBuf, 0, dev.UsbDescriptors, 0, usbCount);
                                    usbFs.Close();

                                    var    usbSr   = new StreamReader(resolvedLink + "/idProduct");
                                    string usbTemp = usbSr.ReadToEnd();

                                    ushort.TryParse(usbTemp, NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                                                    out dev._usbProduct);

                                    usbSr.Close();

                                    usbSr   = new StreamReader(resolvedLink + "/idVendor");
                                    usbTemp = usbSr.ReadToEnd();

                                    ushort.TryParse(usbTemp, NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                                                    out dev._usbVendor);

                                    usbSr.Close();

                                    if(File.Exists(resolvedLink + "/manufacturer"))
                                    {
                                        usbSr                     = new StreamReader(resolvedLink + "/manufacturer");
                                        dev.UsbManufacturerString = usbSr.ReadToEnd().Trim();
                                        usbSr.Close();
                                    }

                                    if(File.Exists(resolvedLink + "/product"))
                                    {
                                        usbSr                = new StreamReader(resolvedLink + "/product");
                                        dev.UsbProductString = usbSr.ReadToEnd().Trim();
                                        usbSr.Close();
                                    }

                                    if(File.Exists(resolvedLink + "/serial"))
                                    {
                                        usbSr               = new StreamReader(resolvedLink + "/serial");
                                        dev.UsbSerialString = usbSr.ReadToEnd().Trim();
                                        usbSr.Close();
                                    }

                                    dev.IsUsb = true;

                                    break;
                                }
                            }
                        }
                    }

                    break;
                case PlatformID.Win32NT:
                    Usb.UsbDevice usbDevice = null;

                    // I have to search for USB disks, floppies and CD-ROMs as separate device types
                    foreach(string devGuid in new[]
                            {
                                Usb.GUID_DEVINTERFACE_FLOPPY, Usb.GUID_DEVINTERFACE_CDROM, Usb.GUID_DEVINTERFACE_DISK,
                                Usb.GUID_DEVINTERFACE_TAPE
                            })
                    {
                        usbDevice = Usb.FindDrivePath(devicePath, devGuid);

                        if(usbDevice != null)
                            break;
                    }

                    if(usbDevice != null)
                    {
                        dev.UsbDescriptors        = usbDevice.BinaryDescriptors;
                        dev._usbVendor            = (ushort)usbDevice._deviceDescriptor.idVendor;
                        dev._usbProduct           = (ushort)usbDevice._deviceDescriptor.idProduct;
                        dev.UsbManufacturerString = usbDevice.Manufacturer;
                        dev.UsbProductString      = usbDevice.Product;

                        dev.UsbSerialString =
                            usbDevice.SerialNumber; // This is incorrect filled by Windows with SCSI/ATA serial number
                    }

                    break;
                default:
                    dev.IsUsb = false;

                    break;
            }
        else
        {
            if(dev._remote.GetUsbData(out byte[] remoteUsbDescriptors, out ushort remoteUsbVendor,
                                      out ushort remoteUsbProduct, out string remoteUsbManufacturer,
                                      out string remoteUsbProductString, out string remoteUsbSerial))
            {
                dev.IsUsb                 = true;
                dev.UsbDescriptors        = remoteUsbDescriptors;
                dev._usbVendor            = remoteUsbVendor;
                dev._usbProduct           = remoteUsbProduct;
                dev.UsbManufacturerString = remoteUsbManufacturer;
                dev.UsbProductString      = remoteUsbProductString;
                dev.UsbSerialString       = remoteUsbSerial;
            }
        }
        #endregion USB

        #region FireWire
        if(!(dev._remote is null))
        {
            if(dev._remote.GetFireWireData(out dev._firewireVendor, out dev._firewireModel, out dev._firewireGuid,
                                           out string remoteFireWireVendorName, out string remoteFireWireModelName))
            {
                dev.IsFireWire         = true;
                dev.FireWireVendorName = remoteFireWireVendorName;
                dev.FireWireModelName  = remoteFireWireModelName;
            }
        }
        else
        {
            if(dev.PlatformId == PlatformID.Linux)
            {
                if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
                   devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
                   devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                {
                    string devPath = devicePath.Substring(5);

                    if(Directory.Exists("/sys/block/" + devPath))
                    {
                        string resolvedLink = Linux.Command.ReadLink("/sys/block/" + devPath);
                        resolvedLink = "/sys" + resolvedLink.Substring(2);

                        if(!string.IsNullOrEmpty(resolvedLink))
                            while(resolvedLink.Contains("firewire"))
                            {
                                resolvedLink = Path.GetDirectoryName(resolvedLink);

                                if(!File.Exists(resolvedLink + "/model")  ||
                                   !File.Exists(resolvedLink + "/vendor") ||
                                   !File.Exists(resolvedLink + "/guid"))
                                    continue;

                                var    fwSr   = new StreamReader(resolvedLink + "/model");
                                string fwTemp = fwSr.ReadToEnd();

                                uint.TryParse(fwTemp, NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                                              out dev._firewireModel);

                                fwSr.Close();

                                fwSr   = new StreamReader(resolvedLink + "/vendor");
                                fwTemp = fwSr.ReadToEnd();

                                uint.TryParse(fwTemp, NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                                              out dev._firewireVendor);

                                fwSr.Close();

                                fwSr   = new StreamReader(resolvedLink + "/guid");
                                fwTemp = fwSr.ReadToEnd();

                                ulong.TryParse(fwTemp, NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                                               out dev._firewireGuid);

                                fwSr.Close();

                                if(File.Exists(resolvedLink + "/model_name"))
                                {
                                    fwSr                  = new StreamReader(resolvedLink + "/model_name");
                                    dev.FireWireModelName = fwSr.ReadToEnd().Trim();
                                    fwSr.Close();
                                }

                                if(File.Exists(resolvedLink + "/vendor_name"))
                                {
                                    fwSr                   = new StreamReader(resolvedLink + "/vendor_name");
                                    dev.FireWireVendorName = fwSr.ReadToEnd().Trim();
                                    fwSr.Close();
                                }

                                dev.IsFireWire = true;

                                break;
                            }
                    }
                }
            }

            // TODO: Implement for other operating systems
            else
                dev.IsFireWire = false;
        }
        #endregion FireWire

        #region PCMCIA
        if(dev._remote is null)
        {
            if(dev.PlatformId == PlatformID.Linux)
            {
                if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
                   devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
                   devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                {
                    string devPath = devicePath.Substring(5);

                    if(Directory.Exists("/sys/block/" + devPath))
                    {
                        string resolvedLink = Linux.Command.ReadLink("/sys/block/" + devPath);
                        resolvedLink = "/sys" + resolvedLink.Substring(2);

                        if(!string.IsNullOrEmpty(resolvedLink))
                            while(resolvedLink.Contains("/sys/devices"))
                            {
                                resolvedLink = Path.GetDirectoryName(resolvedLink);

                                if(!Directory.Exists(resolvedLink + "/pcmcia_socket"))
                                    continue;

                                string[] subdirs = Directory.GetDirectories(resolvedLink + "/pcmcia_socket",
                                                                            "pcmcia_socket*",
                                                                            SearchOption.TopDirectoryOnly);

                                if(subdirs.Length <= 0)
                                    continue;

                                string possibleDir = Path.Combine(resolvedLink, "pcmcia_socket", subdirs[0]);

                                if(!File.Exists(possibleDir + "/card_type") ||
                                   !File.Exists(possibleDir + "/cis"))
                                    continue;

                                var cisFs = new FileStream(possibleDir + "/cis", System.IO.FileMode.Open,
                                                           System.IO.FileAccess.Read);

                                var cisBuf   = new byte[65536];
                                int cisCount = cisFs.Read(cisBuf, 0, 65536);
                                dev.Cis = new byte[cisCount];
                                Array.Copy(cisBuf, 0, dev.Cis, 0, cisCount);
                                cisFs.Close();

                                dev.IsPcmcia = true;

                                break;
                            }
                    }
                }
            }

            // TODO: Implement for other operating systems
            else
                dev.IsPcmcia = false;
        }
        else
        {
            if(dev._remote.GetPcmciaData(out byte[] cisBuf))
            {
                dev.IsPcmcia = true;
                dev.Cis      = cisBuf;
            }
        }
        #endregion PCMCIA

        if(!scsiSense)
        {
            Inquiry? inquiry = Inquiry.Decode(inqBuf);

            dev.Type = DeviceType.SCSI;
            bool serialSense = dev.ScsiInquiry(out inqBuf, out _, 0x80);

            if(!serialSense)
                dev.Serial = EVPD.DecodePage80(inqBuf);

            if(inquiry.HasValue)
            {
                string tmp = StringHandlers.CToString(inquiry.Value.ProductRevisionLevel);

                if(tmp != null)
                    dev.FirmwareRevision = tmp.Trim();

                tmp = StringHandlers.CToString(inquiry.Value.ProductIdentification);

                if(tmp != null)
                    dev.Model = tmp.Trim();

                tmp = StringHandlers.CToString(inquiry.Value.VendorIdentification);

                if(tmp != null)
                    dev.Manufacturer = tmp.Trim();

                dev.IsRemovable = inquiry.Value.RMB;

                dev.ScsiType = (PeripheralDeviceTypes)inquiry.Value.PeripheralDeviceType;
            }

            bool atapiSense = dev.AtapiIdentify(out ataBuf, out _);

            if(!atapiSense)
            {
                dev.Type = DeviceType.ATAPI;
                Identify.IdentifyDevice? ataId = Identify.Decode(ataBuf);

                if(ataId.HasValue)
                    dev.Serial = ataId.Value.SerialNumber;
            }

            dev.LastError = 0;
            dev.Error     = false;
        }

        if(scsiSense && !(dev.IsUsb || dev.IsFireWire) ||
           dev.Manufacturer == "ATA")
        {
            bool ataSense = dev.AtaIdentify(out ataBuf, out _);

            if(!ataSense)
            {
                dev.Type = DeviceType.ATA;
                Identify.IdentifyDevice? ataid = Identify.Decode(ataBuf);

                if(ataid.HasValue)
                {
                    string[] separated = ataid.Value.Model.Split(' ');

                    if(separated.Length == 1)
                        dev.Model = separated[0];
                    else
                    {
                        dev.Manufacturer = separated[0];
                        dev.Model        = separated[^1];
                    }

                    dev.FirmwareRevision = ataid.Value.FirmwareRevision;
                    dev.Serial           = ataid.Value.SerialNumber;

                    dev.ScsiType = PeripheralDeviceTypes.DirectAccess;

                    if((ushort)ataid.Value.GeneralConfiguration != 0x848A)
                        dev.IsRemovable |=
                            (ataid.Value.GeneralConfiguration & Identify.GeneralConfigurationBit.Removable) ==
                            Identify.GeneralConfigurationBit.Removable;
                    else
                        dev.IsCompactFlash = true;
                }
            }
        }

        if(dev.Type == DeviceType.Unknown)
        {
            dev.Manufacturer     = null;
            dev.Model            = null;
            dev.FirmwareRevision = null;
            dev.Serial           = null;
        }

        if(dev.IsUsb)
        {
            if(string.IsNullOrEmpty(dev.Manufacturer))
                dev.Manufacturer = dev.UsbManufacturerString;

            if(string.IsNullOrEmpty(dev.Model))
                dev.Model = dev.UsbProductString;

            if(string.IsNullOrEmpty(dev.Serial))
                dev.Serial = dev.UsbSerialString;
            else
                foreach(char c in dev.Serial.Where(c => !char.IsControl(c)))
                    dev.Serial = $"{dev.Serial}{c:X2}";
        }

        if(dev.IsFireWire)
        {
            if(string.IsNullOrEmpty(dev.Manufacturer))
                dev.Manufacturer = dev.FireWireVendorName;

            if(string.IsNullOrEmpty(dev.Model))
                dev.Model = dev.FireWireModelName;

            if(string.IsNullOrEmpty(dev.Serial))
                dev.Serial = $"{dev._firewireGuid:X16}";
            else
                foreach(char c in dev.Serial.Where(c => !char.IsControl(c)))
                    dev.Serial = $"{dev.Serial}{c:X2}";
        }

        // Some optical drives are not getting the correct serial, and IDENTIFY PACKET DEVICE is blocked without
        // administrator privileges
        if(dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
            return dev;

        bool featureSense = dev.GetConfiguration(out byte[] featureBuffer, out _, 0x0108, MmcGetConfigurationRt.Single,
                                                 dev.Timeout, out _);

        if(featureSense)
            return dev;

        Features.SeparatedFeatures features = Features.Separate(featureBuffer);

        if(features.Descriptors?.Length != 1 ||
           features.Descriptors[0].Code != 0x0108)
            return dev;

        Feature_0108? serialFeature = Features.Decode_0108(features.Descriptors[0].Data);

        if(serialFeature is null)
            return dev;

        dev.Serial = serialFeature.Value.Serial;

        return dev;
    }

    Device() {}

    static int ConvertFromFileHexAscii(string file, out byte[] outBuf)
    {
        var    sr  = new StreamReader(file);
        string ins = sr.ReadToEnd().Trim();

        int count = Helpers.Marshal.ConvertFromHexAscii(ins, out outBuf);

        sr.Close();

        return count;
    }
}