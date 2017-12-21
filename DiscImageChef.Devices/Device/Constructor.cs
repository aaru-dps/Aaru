// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using Microsoft.Win32.SafeHandles;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        /// <summary>
        /// Opens the device for sending direct commands
        /// </summary>
        /// <param name="devicePath">Device path</param>
        public Device(string devicePath)
        {
            platformId = Interop.DetectOS.GetRealPlatformID();
            Timeout = 15;
            error = false;
            removable = false;

            switch(platformId)
            {
                case Interop.PlatformID.Win32NT:
                {
                    fd = Windows.Extern.CreateFile(devicePath,
                                                   Windows.FileAccess.GenericRead | Windows.FileAccess.GenericWrite,
                                                   Windows.FileShare.Read | Windows.FileShare.Write, IntPtr.Zero,
                                                   Windows.FileMode.OpenExisting, Windows.FileAttributes.Normal,
                                                   IntPtr.Zero);

                    if(((SafeFileHandle)fd).IsInvalid)
                    {
                        error = true;
                        lastError = Marshal.GetLastWin32Error();
                    }

                    break;
                }
                case Interop.PlatformID.Linux:
                {
                    fd = Linux.Extern.open(devicePath, Linux.FileFlags.Readonly | Linux.FileFlags.NonBlocking);

                    if((int)fd < 0)
                    {
                        error = true;
                        lastError = Marshal.GetLastWin32Error();
                    }

                    break;
                }
                case Interop.PlatformID.FreeBSD:
                {
                    fd = FreeBSD.Extern.cam_open_device(devicePath, FreeBSD.FileFlags.ReadWrite);

                    if(((IntPtr)fd).ToInt64() == 0)
                    {
                        error = true;
                        lastError = Marshal.GetLastWin32Error();
                    }

                    FreeBSD.CamDevice camDevice =
                        (FreeBSD.CamDevice)Marshal.PtrToStructure((IntPtr)fd, typeof(FreeBSD.CamDevice));

                    if(StringHandlers.CToString(camDevice.SimName) == "ata")
                        throw new
                            InvalidOperationException("Parallel ATA devices are not supported on FreeBSD due to upstream bug #224250.");

                    break;
                }
                default:
                    throw new InvalidOperationException(string.Format("Platform {0} not yet supported.", platformId));
            }

            if(error) throw new SystemException(string.Format("Error {0} opening device.", lastError));

            type = DeviceType.Unknown;
            scsiType = Decoders.SCSI.PeripheralDeviceTypes.UnknownDevice;

            AtaErrorRegistersCHS errorRegisters;

            byte[] ataBuf;
            byte[] senseBuf;
            byte[] inqBuf = null;

            if(error) throw new SystemException(string.Format("Error {0} trying device.", lastError));

            bool scsiSense = true;
            string ntDevicePath = null;

            // Windows is answering SCSI INQUIRY for all device types so it needs to be detected first
            switch(platformId) {
                case Interop.PlatformID.Win32NT:
                    Windows.StoragePropertyQuery query = new Windows.StoragePropertyQuery();
                    query.PropertyId = Windows.StoragePropertyId.Device;
                    query.QueryType = Windows.StorageQueryType.Standard;
                    query.AdditionalParameters = new byte[1];

                    IntPtr descriptorPtr = Marshal.AllocHGlobal(1000);
                    byte[] descriptorB = new byte[1000];

                    uint returned = 0;
                    int error = 0;

                    bool hasError = !Windows.Extern.DeviceIoControlStorageQuery((SafeFileHandle)fd,
                                                                                Windows.WindowsIoctl
                                                                                       .IoctlStorageQueryProperty,
                                                                                ref query, (uint)Marshal.SizeOf(query),
                                                                                descriptorPtr, 1000, ref returned,
                                                                                IntPtr.Zero);

                    if(hasError) error = Marshal.GetLastWin32Error();

                    Marshal.Copy(descriptorPtr, descriptorB, 0, 1000);

                    if(!hasError && error == 0)
                    {
                        Windows.StorageDeviceDescriptor descriptor = new Windows.StorageDeviceDescriptor();
                        descriptor.Version = BitConverter.ToUInt32(descriptorB, 0);
                        descriptor.Size = BitConverter.ToUInt32(descriptorB, 4);
                        descriptor.DeviceType = descriptorB[8];
                        descriptor.DeviceTypeModifier = descriptorB[9];
                        descriptor.RemovableMedia = descriptorB[10] > 0;
                        descriptor.CommandQueueing = descriptorB[11] > 0;
                        descriptor.VendorIdOffset = BitConverter.ToInt32(descriptorB, 12);
                        descriptor.ProductIdOffset = BitConverter.ToInt32(descriptorB, 16);
                        descriptor.ProductRevisionOffset = BitConverter.ToInt32(descriptorB, 20);
                        descriptor.SerialNumberOffset = BitConverter.ToInt32(descriptorB, 24);
                        descriptor.BusType = (Windows.StorageBusType)BitConverter.ToUInt32(descriptorB, 28);
                        descriptor.RawPropertiesLength = BitConverter.ToUInt32(descriptorB, 32);
                        descriptor.RawDeviceProperties = new byte[descriptor.RawPropertiesLength];
                        Array.Copy(descriptorB, 36, descriptor.RawDeviceProperties, 0, descriptor.RawPropertiesLength);

                        switch(descriptor.BusType)
                        {
                            case Windows.StorageBusType.SCSI:
                            case Windows.StorageBusType.SSA:
                            case Windows.StorageBusType.Fibre:
                            case Windows.StorageBusType.iSCSI:
                            case Windows.StorageBusType.SAS:
                                type = DeviceType.SCSI;
                                break;
                            case Windows.StorageBusType.FireWire:
                                firewire = true;
                                type = DeviceType.SCSI;
                                break;
                            case Windows.StorageBusType.USB:
                                usb = true;
                                type = DeviceType.SCSI;
                                break;
                            case Windows.StorageBusType.ATAPI:
                                type = DeviceType.ATAPI;
                                break;
                            case Windows.StorageBusType.ATA:
                            case Windows.StorageBusType.SATA:
                                type = DeviceType.ATA;
                                break;
                            case Windows.StorageBusType.MultiMediaCard:
                                type = DeviceType.MMC;
                                break;
                            case Windows.StorageBusType.SecureDigital:
                                type = DeviceType.SecureDigital;
                                break;
                            case Windows.StorageBusType.NVMe:
                                type = DeviceType.NVMe;
                                break;
                        }

                        switch(type) {
                            case DeviceType.SCSI:
                            case DeviceType.ATAPI: scsiSense = ScsiInquiry(out inqBuf, out senseBuf);
                                break;
                            case DeviceType.ATA:
                                bool atapiSense = AtapiIdentify(out ataBuf, out errorRegisters);

                                if(!atapiSense)
                                {
                                    type = DeviceType.ATAPI;
                                    Identify.IdentifyDevice? ataid = Identify.Decode(ataBuf);

                                    if(ataid.HasValue) scsiSense = ScsiInquiry(out inqBuf, out senseBuf);
                                }
                                else manufacturer = "ATA";
                                break;
                        }
                    }

                    ntDevicePath = Windows.Command.GetDevicePath((SafeFileHandle)fd);
                    DicConsole.DebugWriteLine("Windows devices", "NT device path: {0}", ntDevicePath);
                    Marshal.FreeHGlobal(descriptorPtr);

                    if(Windows.Command.IsSdhci((SafeFileHandle)fd))
                    {
                        byte[] sdBuffer = new byte[16];
                        bool sense = false;

                        lastError = Windows.Command.SendMmcCommand((SafeFileHandle)fd, MmcCommands.SendCsd, false, false,
                                                                   MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 |
                                                                   MmcFlags.CommandAc, 0, 16, 1, ref sdBuffer,
                                                                   out uint[] response, out double duration, out sense, 0);

                        if(!sense)
                        {
                            cachedCsd = new byte[16];
                            Array.Copy(sdBuffer, 0, cachedCsd, 0, 16);
                        }

                        sdBuffer = new byte[16];
                        sense = false;

                        lastError = Windows.Command.SendMmcCommand((SafeFileHandle)fd, MmcCommands.SendCid, false, false,
                                                                   MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 |
                                                                   MmcFlags.CommandAc, 0, 16, 1, ref sdBuffer, out response,
                                                                   out duration, out sense, 0);

                        if(!sense)
                        {
                            cachedCid = new byte[16];
                            Array.Copy(sdBuffer, 0, cachedCid, 0, 16);
                        }

                        sdBuffer = new byte[8];
                        sense = false;

                        lastError = Windows.Command.SendMmcCommand((SafeFileHandle)fd,
                                                                   (MmcCommands)SecureDigitalCommands.SendScr, false, true,
                                                                   MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 |
                                                                   MmcFlags.CommandAdtc, 0, 8, 1, ref sdBuffer,
                                                                   out response, out duration, out sense, 0);

                        if(!sense)
                        {
                            cachedScr = new byte[8];
                            Array.Copy(sdBuffer, 0, cachedScr, 0, 8);
                        }

                        if(cachedScr != null)
                        {
                            sdBuffer = new byte[4];
                            sense = false;

                            lastError = Windows.Command.SendMmcCommand((SafeFileHandle)fd,
                                                                       (MmcCommands)SecureDigitalCommands
                                                                           .SendOperatingCondition, false, true,
                                                                       MmcFlags.ResponseSpiR3 | MmcFlags.ResponseR3 |
                                                                       MmcFlags.CommandBcr, 0, 4, 1, ref sdBuffer,
                                                                       out response, out duration, out sense, 0);

                            if(!sense)
                            {
                                cachedScr = new byte[4];
                                Array.Copy(sdBuffer, 0, cachedScr, 0, 4);
                            }
                        }
                        else
                        {
                            sdBuffer = new byte[4];
                            sense = false;

                            lastError = Windows.Command.SendMmcCommand((SafeFileHandle)fd, MmcCommands.SendOpCond, false,
                                                                       true,
                                                                       MmcFlags.ResponseSpiR3 | MmcFlags.ResponseR3 |
                                                                       MmcFlags.CommandBcr, 0, 4, 1, ref sdBuffer,
                                                                       out response, out duration, out sense, 0);

                            if(!sense)
                            {
                                cachedScr = new byte[4];
                                Array.Copy(sdBuffer, 0, cachedScr, 0, 4);
                            }
                        }
                    }
                    break;
                case Interop.PlatformID.Linux:
                    if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
                       devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
                       devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                        scsiSense = ScsiInquiry(out inqBuf, out senseBuf);
                    // MultiMediaCard and SecureDigital go here
                    else if(devicePath.StartsWith("/dev/mmcblk", StringComparison.Ordinal))
                    {
                        string devPath = devicePath.Substring(5);
                        if(System.IO.File.Exists("/sys/block/" + devPath + "/device/csd"))
                        {
                            int len = ConvertFromHexAscii("/sys/block/" + devPath + "/device/csd", out cachedCsd);
                            if(len == 0) cachedCsd = null;
                        }
                        if(System.IO.File.Exists("/sys/block/" + devPath + "/device/cid"))
                        {
                            int len = ConvertFromHexAscii("/sys/block/" + devPath + "/device/cid", out cachedCid);
                            if(len == 0) cachedCid = null;
                        }
                        if(System.IO.File.Exists("/sys/block/" + devPath + "/device/scr"))
                        {
                            int len = ConvertFromHexAscii("/sys/block/" + devPath + "/device/scr", out cachedScr);
                            if(len == 0) cachedScr = null;
                        }
                        if(System.IO.File.Exists("/sys/block/" + devPath + "/device/ocr"))
                        {
                            int len = ConvertFromHexAscii("/sys/block/" + devPath + "/device/ocr", out cachedOcr);
                            if(len == 0) cachedOcr = null;
                        }
                    }
                    break;
                default: scsiSense = ScsiInquiry(out inqBuf, out senseBuf);
                    break;
            }

            #region SecureDigital / MultiMediaCard
            if(cachedCid != null)
            {
                scsiType = Decoders.SCSI.PeripheralDeviceTypes.DirectAccess;
                removable = false;

                if(cachedScr != null)
                {
                    type = DeviceType.SecureDigital;
                    Decoders.SecureDigital.CID decoded = Decoders.SecureDigital.Decoders.DecodeCID(cachedCid);
                    manufacturer = Decoders.SecureDigital.VendorString.Prettify(decoded.Manufacturer);
                    model = decoded.ProductName;
                    revision = string.Format("{0:X2}.{1:X2}", (decoded.ProductRevision & 0xF0) >> 4,
                                             decoded.ProductRevision & 0x0F);
                    serial = string.Format("{0}", decoded.ProductSerialNumber);
                }
                else
                {
                    type = DeviceType.MMC;
                    Decoders.MMC.CID decoded = Decoders.MMC.Decoders.DecodeCID(cachedCid);
                    manufacturer = Decoders.MMC.VendorString.Prettify(decoded.Manufacturer);
                    model = decoded.ProductName;
                    revision = string.Format("{0:X2}.{1:X2}", (decoded.ProductRevision & 0xF0) >> 4,
                                             decoded.ProductRevision & 0x0F);
                    serial = string.Format("{0}", decoded.ProductSerialNumber);
                }
            }
            #endregion SecureDigital / MultiMediaCard

            #region USB
            switch(platformId) {
                case Interop.PlatformID.Linux:
                    if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
                       devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
                       devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                    {
                        string devPath = devicePath.Substring(5);
                        if(System.IO.Directory.Exists("/sys/block/" + devPath))
                        {
                            string resolvedLink = Linux.Command.ReadLink("/sys/block/" + devPath);
                            resolvedLink = "/sys" + resolvedLink.Substring(2);
                            if(!string.IsNullOrEmpty(resolvedLink))
                                while(resolvedLink.Contains("usb"))
                                {
                                    resolvedLink = System.IO.Path.GetDirectoryName(resolvedLink);
                                    if(!System.IO.File.Exists(resolvedLink + "/descriptors") ||
                                       !System.IO.File.Exists(resolvedLink + "/idProduct") ||
                                       !System.IO.File.Exists(resolvedLink + "/idVendor")) continue;

                                    System.IO.FileStream usbFs;
                                    System.IO.StreamReader usbSr;
                                    string usbTemp;

                                    usbFs = new System.IO.FileStream(resolvedLink + "/descriptors",
                                                                     System.IO.FileMode.Open,
                                                                     System.IO.FileAccess.Read);
                                    byte[] usbBuf = new byte[65536];
                                    int usbCount = usbFs.Read(usbBuf, 0, 65536);
                                    usbDescriptors = new byte[usbCount];
                                    Array.Copy(usbBuf, 0, usbDescriptors, 0, usbCount);
                                    usbFs.Close();

                                    usbSr = new System.IO.StreamReader(resolvedLink + "/idProduct");
                                    usbTemp = usbSr.ReadToEnd();
                                    ushort.TryParse(usbTemp, System.Globalization.NumberStyles.HexNumber,
                                                    System.Globalization.CultureInfo.InvariantCulture, out usbProduct);
                                    usbSr.Close();

                                    usbSr = new System.IO.StreamReader(resolvedLink + "/idVendor");
                                    usbTemp = usbSr.ReadToEnd();
                                    ushort.TryParse(usbTemp, System.Globalization.NumberStyles.HexNumber,
                                                    System.Globalization.CultureInfo.InvariantCulture, out usbVendor);
                                    usbSr.Close();

                                    if(System.IO.File.Exists(resolvedLink + "/manufacturer"))
                                    {
                                        usbSr = new System.IO.StreamReader(resolvedLink + "/manufacturer");
                                        usbManufacturerString = usbSr.ReadToEnd().Trim();
                                        usbSr.Close();
                                    }

                                    if(System.IO.File.Exists(resolvedLink + "/product"))
                                    {
                                        usbSr = new System.IO.StreamReader(resolvedLink + "/product");
                                        usbProductString = usbSr.ReadToEnd().Trim();
                                        usbSr.Close();
                                    }

                                    if(System.IO.File.Exists(resolvedLink + "/serial"))
                                    {
                                        usbSr = new System.IO.StreamReader(resolvedLink + "/serial");
                                        usbSerialString = usbSr.ReadToEnd().Trim();
                                        usbSr.Close();
                                    }

                                    usb = true;
                                    break;
                                }
                        }
                    }

                    break;
                case Interop.PlatformID.Win32NT:
                    Windows.Usb.UsbDevice usbDevice = null;

                    // I have to search for USB disks, floppies and CD-ROMs as separate device types
                    foreach(string devGuid in new[]
                    {
                        Windows.Usb.GuidDevinterfaceFloppy, Windows.Usb.GuidDevinterfaceCdrom,
                        Windows.Usb.GuidDevinterfaceDisk
                    })
                    {
                        usbDevice = Windows.Usb.FindDrivePath(devicePath, devGuid);
                        if(usbDevice != null) break;
                    }

                    if(usbDevice != null)
                    {
                        usbDescriptors = usbDevice.BinaryDescriptors;
                        usbVendor = (ushort)usbDevice.DeviceDescriptor.idVendor;
                        usbProduct = (ushort)usbDevice.DeviceDescriptor.idProduct;
                        usbManufacturerString = usbDevice.Manufacturer;
                        usbProductString = usbDevice.Product;
                        usbSerialString =
                            usbDevice.SerialNumber; // This is incorrect filled by Windows with SCSI/ATA serial number
                    }
                    break;
                default: usb = false;
                    break;
            }
            #endregion USB

            #region FireWire
            if(platformId == Interop.PlatformID.Linux)
            {
                if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
                   devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
                   devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                {
                    string devPath = devicePath.Substring(5);
                    if(System.IO.Directory.Exists("/sys/block/" + devPath))
                    {
                        string resolvedLink = Linux.Command.ReadLink("/sys/block/" + devPath);
                        resolvedLink = "/sys" + resolvedLink.Substring(2);
                        if(!string.IsNullOrEmpty(resolvedLink))
                            while(resolvedLink.Contains("firewire"))
                            {
                                resolvedLink = System.IO.Path.GetDirectoryName(resolvedLink);
                                if(!System.IO.File.Exists(resolvedLink + "/model") ||
                                   !System.IO.File.Exists(resolvedLink + "/vendor") ||
                                   !System.IO.File.Exists(resolvedLink + "/guid")) continue;

                                System.IO.StreamReader fwSr;
                                string fwTemp;

                                fwSr = new System.IO.StreamReader(resolvedLink + "/model");
                                fwTemp = fwSr.ReadToEnd();
                                uint.TryParse(fwTemp, System.Globalization.NumberStyles.HexNumber,
                                              System.Globalization.CultureInfo.InvariantCulture, out firewireModel);
                                fwSr.Close();

                                fwSr = new System.IO.StreamReader(resolvedLink + "/vendor");
                                fwTemp = fwSr.ReadToEnd();
                                uint.TryParse(fwTemp, System.Globalization.NumberStyles.HexNumber,
                                              System.Globalization.CultureInfo.InvariantCulture,
                                              out firewireVendor);
                                fwSr.Close();

                                fwSr = new System.IO.StreamReader(resolvedLink + "/guid");
                                fwTemp = fwSr.ReadToEnd();
                                ulong.TryParse(fwTemp, System.Globalization.NumberStyles.HexNumber,
                                               System.Globalization.CultureInfo.InvariantCulture, out firewireGuid);
                                fwSr.Close();

                                if(System.IO.File.Exists(resolvedLink + "/model_name"))
                                {
                                    fwSr = new System.IO.StreamReader(resolvedLink + "/model_name");
                                    firewireModelName = fwSr.ReadToEnd().Trim();
                                    fwSr.Close();
                                }

                                if(System.IO.File.Exists(resolvedLink + "/vendor_name"))
                                {
                                    fwSr = new System.IO.StreamReader(resolvedLink + "/vendor_name");
                                    firewireVendorName = fwSr.ReadToEnd().Trim();
                                    fwSr.Close();
                                }

                                firewire = true;
                                break;
                            }
                    }
                }
            }
            // TODO: Implement for other operating systems
            else firewire = false;
            #endregion FireWire

            #region PCMCIA
            if(platformId == Interop.PlatformID.Linux)
            {
                if(devicePath.StartsWith("/dev/sd", StringComparison.Ordinal) ||
                   devicePath.StartsWith("/dev/sr", StringComparison.Ordinal) ||
                   devicePath.StartsWith("/dev/st", StringComparison.Ordinal))
                {
                    string devPath = devicePath.Substring(5);
                    if(System.IO.Directory.Exists("/sys/block/" + devPath))
                    {
                        string resolvedLink = Linux.Command.ReadLink("/sys/block/" + devPath);
                        resolvedLink = "/sys" + resolvedLink.Substring(2);
                        if(!string.IsNullOrEmpty(resolvedLink))
                            while(resolvedLink.Contains("/sys/devices"))
                            {
                                resolvedLink = System.IO.Path.GetDirectoryName(resolvedLink);
                                if(!System.IO.Directory.Exists(resolvedLink + "/pcmcia_socket")) continue;

                                string[] subdirs =
                                    System.IO.Directory.GetDirectories(resolvedLink + "/pcmcia_socket",
                                                                       "pcmcia_socket*",
                                                                       System.IO.SearchOption.TopDirectoryOnly);

                                if(subdirs.Length <= 0) continue;

                                string possibleDir =
                                    System.IO.Path.Combine(resolvedLink, "pcmcia_socket", subdirs[0]);
                                if(!System.IO.File.Exists(possibleDir + "/card_type") ||
                                   !System.IO.File.Exists(possibleDir + "/cis")) continue;

                                System.IO.FileStream cisFs;

                                cisFs = new System.IO.FileStream(possibleDir + "/cis",
                                                                 System.IO.FileMode.Open,
                                                                 System.IO.FileAccess.Read);
                                byte[] cisBuf = new byte[65536];
                                int cisCount = cisFs.Read(cisBuf, 0, 65536);
                                cis = new byte[cisCount];
                                Array.Copy(cisBuf, 0, cis, 0, cisCount);
                                cisFs.Close();

                                pcmcia = true;
                                break;
                            }
                    }
                }
            }
            // TODO: Implement for other operating systems
            else pcmcia = false;
            #endregion PCMCIA

            if(!scsiSense)
            {
                Decoders.SCSI.Inquiry.SCSIInquiry? inquiry = Decoders.SCSI.Inquiry.Decode(inqBuf);

                type = DeviceType.SCSI;
                bool serialSense = ScsiInquiry(out inqBuf, out senseBuf, 0x80);
                if(!serialSense) serial = Decoders.SCSI.EVPD.DecodePage80(inqBuf);

                if(inquiry.HasValue)
                {
                    string tmp = StringHandlers.CToString(inquiry.Value.ProductRevisionLevel);
                    if(tmp != null) revision = tmp.Trim();
                    tmp = StringHandlers.CToString(inquiry.Value.ProductIdentification);
                    if(tmp != null) model = tmp.Trim();
                    tmp = StringHandlers.CToString(inquiry.Value.VendorIdentification);
                    if(tmp != null) manufacturer = tmp.Trim();
                    removable = inquiry.Value.RMB;

                    scsiType = (Decoders.SCSI.PeripheralDeviceTypes)inquiry.Value.PeripheralDeviceType;
                }

                bool atapiSense = AtapiIdentify(out ataBuf, out errorRegisters);

                if(!atapiSense)
                {
                    type = DeviceType.ATAPI;
                    Identify.IdentifyDevice? ataId = Identify.Decode(ataBuf);

                    if(ataId.HasValue) serial = ataId.Value.SerialNumber;
                }
                else
                {
                    lastError = 0;
                    error = false;
                }
            }

            if(scsiSense && (usb || firewire) || manufacturer == "ATA")
            {
                bool ataSense = AtaIdentify(out ataBuf, out errorRegisters);
                if(!ataSense)
                {
                    type = DeviceType.ATA;
                    Identify.IdentifyDevice? ataid = Identify.Decode(ataBuf);

                    if(ataid.HasValue)
                    {
                        string[] separated = ataid.Value.Model.Split(' ');

                        if(separated.Length == 1) model = separated[0];
                        else
                        {
                            manufacturer = separated[0];
                            model = separated[separated.Length - 1];
                        }

                        revision = ataid.Value.FirmwareRevision;
                        serial = ataid.Value.SerialNumber;

                        scsiType = Decoders.SCSI.PeripheralDeviceTypes.DirectAccess;

                        if((ushort)ataid.Value.GeneralConfiguration != 0x848A)
                            removable |=
                                (ataid.Value.GeneralConfiguration & Identify.GeneralConfigurationBit.Removable) ==
                                Identify.GeneralConfigurationBit.Removable;
                        else compactFlash = true;
                    }
                }
            }

            if(type == DeviceType.Unknown)
            {
                manufacturer = null;
                model = null;
                revision = null;
                serial = null;
            }

            if(usb)
            {
                if(string.IsNullOrEmpty(manufacturer)) manufacturer = usbManufacturerString;
                if(string.IsNullOrEmpty(model)) model = usbProductString;
                if(string.IsNullOrEmpty(serial)) serial = usbSerialString;
                else foreach(char c in serial.Where(c => char.IsControl(c))) serial = usbSerialString;
            }

            if(!firewire) return;

            if(string.IsNullOrEmpty(manufacturer)) manufacturer = firewireVendorName;
            if(string.IsNullOrEmpty(model)) model = firewireModelName;
            if(string.IsNullOrEmpty(serial)) serial = string.Format("{0:X16}", firewireGuid);
            else foreach(char c in serial.Where(c => char.IsControl(c))) serial = string.Format("{0:X16}", firewireGuid);
        }

        static int ConvertFromHexAscii(string file, out byte[] outBuf)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(file);
            string ins = sr.ReadToEnd().Trim();
            outBuf = new byte[ins.Length / 2];
            int count = 0;

            try
            {
                for(int i = 0; i < ins.Length; i += 2)
                {
                    outBuf[i / 2] = Convert.ToByte(ins.Substring(i, 2), 16);
                    count++;
                }
            }
            catch { count = 0; }

            sr.Close();
            return count;
        }
    }
}