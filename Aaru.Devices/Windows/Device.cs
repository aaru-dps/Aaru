// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Device.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Windows direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prepares a Windows device for direct access.
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

namespace Aaru.Devices.Windows;

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SecureDigital;
using Microsoft.Win32.SafeHandles;

/// <inheritdoc />
[SupportedOSPlatform("windows")]
partial class Device : Devices.Device
{
    /// <summary>Gets the file handle representing this device</summary>
    /// <value>The file handle</value>
    SafeFileHandle _fileHandle;

    Device() {}

    internal new static Device Create(string devicePath)
    {
        var dev = new Device
        {
            PlatformId = DetectOS.GetRealPlatformID(),
            Timeout = 15,
            Error = false,
            IsRemovable = false,
            _fileHandle = Extern.CreateFile(devicePath, FileAccess.GenericRead | FileAccess.GenericWrite,
                                            FileShare.Read | FileShare.Write, IntPtr.Zero, FileMode.OpenExisting,
                                            FileAttributes.Normal, IntPtr.Zero)
        };

        if(dev._fileHandle.IsInvalid)
        {
            dev.Error     = true;
            dev.LastError = Marshal.GetLastWin32Error();
        }

        if(dev.Error)
            throw new DeviceException(dev.LastError);

        dev.Type     = DeviceType.Unknown;
        dev.ScsiType = PeripheralDeviceTypes.UnknownDevice;

        if(dev.Error)
            throw new DeviceException(dev.LastError);

        // Windows is answering SCSI INQUIRY for all device types so it needs to be detected first
        var query = new StoragePropertyQuery();
        query.PropertyId           = StoragePropertyId.Device;
        query.QueryType            = StorageQueryType.Standard;
        query.AdditionalParameters = new byte[1];

        IntPtr descriptorPtr = Marshal.AllocHGlobal(1000);
        var    descriptorB   = new byte[1000];

        uint returned = 0;
        var  error    = 0;

        bool hasError = !Extern.DeviceIoControlStorageQuery(dev._fileHandle, WindowsIoctl.IoctlStorageQueryProperty,
                                                            ref query, (uint)Marshal.SizeOf(query), descriptorPtr, 1000,
                                                            ref returned, IntPtr.Zero);

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
                case DeviceType.ATA:
                    bool atapiSense = dev.AtapiIdentify(out byte[] _, out _);

                    if(!atapiSense)
                        dev.Type = DeviceType.ATAPI;
                    else
                        dev.Manufacturer = "ATA";

                    break;
            }
        }

        Marshal.FreeHGlobal(descriptorPtr);

        if(IsSdhci(dev._fileHandle))
        {
            var sdBuffer = new byte[16];

            dev.LastError = dev.SendMmcCommand(MmcCommands.SendCsd, false, false,
                                               MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 | MmcFlags.CommandAc, 0, 16,
                                               1, ref sdBuffer, out _, out _, out bool sense);

            if(!sense)
            {
                dev._cachedCsd = new byte[16];
                Array.Copy(sdBuffer, 0, dev._cachedCsd, 0, 16);
            }

            sdBuffer = new byte[16];

            dev.LastError = dev.SendMmcCommand(MmcCommands.SendCid, false, false,
                                               MmcFlags.ResponseSpiR2 | MmcFlags.ResponseR2 | MmcFlags.CommandAc, 0, 16,
                                               1, ref sdBuffer, out _, out _, out sense);

            if(!sense)
            {
                dev._cachedCid = new byte[16];
                Array.Copy(sdBuffer, 0, dev._cachedCid, 0, 16);
            }

            sdBuffer = new byte[8];

            dev.LastError = dev.SendMmcCommand((MmcCommands)SecureDigitalCommands.SendScr, false, true,
                                               MmcFlags.ResponseSpiR1 | MmcFlags.ResponseR1 | MmcFlags.CommandAdtc, 0,
                                               8, 1, ref sdBuffer, out _, out _, out sense);

            if(!sense)
            {
                dev._cachedScr = new byte[8];
                Array.Copy(sdBuffer, 0, dev._cachedScr, 0, 8);
            }

            sdBuffer = new byte[4];

            dev.LastError =
                dev.SendMmcCommand(dev._cachedScr != null ? (MmcCommands)SecureDigitalCommands.SendOperatingCondition : MmcCommands.SendOpCond,
                                   false, true, MmcFlags.ResponseSpiR3 | MmcFlags.ResponseR3 | MmcFlags.CommandBcr, 0,
                                   4, 1, ref sdBuffer, out _, out _, out sense);

            if(!sense)
            {
                dev._cachedScr = new byte[4];
                Array.Copy(sdBuffer, 0, dev._cachedScr, 0, 4);
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
        #endregion USB

        #region FireWire
        // TODO: Implement

        dev.IsFireWire = false;
        #endregion FireWire

        #region PCMCIA
        // TODO: Implement
        #endregion PCMCIA

        return dev;
    }

    /// <inheritdoc />
    public override void Close()
    {
        if(_fileHandle == null)
            return;

        _fileHandle?.Close();

        _fileHandle = null;
    }
}