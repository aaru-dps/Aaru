// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Device.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Remote device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prepares a remote device for direct access.
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

namespace Aaru.Devices.Remote;

using System;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SecureDigital;

/// <inheritdoc />
public sealed class Device : Devices.Device
{
    Device() {}

    /// <summary>Opens the device for sending direct commands</summary>
    /// <param name="aaruUri">AaruRemote URI</param>
    /// <returns>Device</returns>
    public static Device Create(Uri aaruUri)
    {
        var dev = new Device
        {
            PlatformId  = DetectOS.GetRealPlatformID(),
            Timeout     = 15,
            Error       = false,
            IsRemovable = false
        };

        if(aaruUri.Scheme is not ("dic" or "aaru"))
            return null;

        string devicePath = aaruUri.AbsolutePath;

        if(devicePath.StartsWith('/'))
            devicePath = devicePath[1..];

        if(devicePath.StartsWith("dev", StringComparison.Ordinal))
            devicePath = $"/{devicePath}";

        dev._devicePath = devicePath;

        dev._remote = new Remote(aaruUri);

        dev.Error     = !dev._remote.Open(devicePath, out int errno);
        dev.LastError = errno;

        if(dev.Error)
            throw new DeviceException(dev.LastError);

        if(dev._remote.ServerOperatingSystem == "Linux")
            _readMultipleBlockCannotSetBlockCount = true;

        dev.Type     = DeviceType.Unknown;
        dev.ScsiType = PeripheralDeviceTypes.UnknownDevice;

        byte[] ataBuf;
        byte[] inqBuf = null;

        if(dev.Error)
            throw new DeviceException(dev.LastError);

        dev.Type = dev._remote.GetDeviceType();

        switch(dev.Type)
        {
            case DeviceType.ATAPI:
            case DeviceType.SCSI:
                bool scsiSense = dev.ScsiInquiry(out inqBuf, out _);

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
        #endregion USB

        #region FireWire
        if(dev._remote.GetFireWireData(out dev._firewireVendor, out dev._firewireModel, out dev._firewireGuid,
                                       out string remoteFireWireVendorName, out string remoteFireWireModelName))
        {
            dev.IsFireWire         = true;
            dev.FireWireVendorName = remoteFireWireVendorName;
            dev.FireWireModelName  = remoteFireWireModelName;
        }
        #endregion FireWire
        #region PCMCIA
        if(dev._remote.GetPcmciaData(out byte[] cisBuf))
        {
            dev.IsPcmcia = true;
            dev.Cis      = cisBuf;
        }
        #endregion PCMCIA

        return dev;
    }

    /// <inheritdoc />
    public override void Close()
    {
        if(_remote == null)
            return;

        _remote.Close();
        _remote.Disconnect();
    }
}