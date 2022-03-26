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
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Helpers;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;

/// <summary>Implements a device or media containing drive</summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "UnusedMember.Global"),
 SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public partial class Device
{
    /// <summary>Opens the device for sending direct commands</summary>
    /// <param name="devicePath">Device path</param>
    public static Device Create(string devicePath)
    {
        Device dev = null;
        Uri    aaruUri;

        try
        {
            aaruUri = new Uri(devicePath);
        }
        catch(Exception)
        {
            return null;
        }

        if(aaruUri.Scheme is "dic" or "aaru")
            dev = Remote.Device.Create(aaruUri);
        else if(OperatingSystem.IsLinux())
            dev = Linux.Device.Create(devicePath);
        else if(OperatingSystem.IsWindows())
            dev = Windows.Device.Create(devicePath);

        if(dev is null)
            throw new DeviceException("Platform not supported.");

        if(dev.Type == DeviceType.SCSI ||
           dev.Type == DeviceType.ATAPI)
        {
            dev.ScsiInquiry(out byte[] inqBuf, out _);

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

            bool atapiSense = dev.AtapiIdentify(out byte[] ataBuf, out _);

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

        if(dev.Type != DeviceType.SCSI && dev.Type != DeviceType.ATAPI && !(dev.IsUsb || dev.IsFireWire) ||
           dev.Manufacturer == "ATA")
        {
            bool ataSense = dev.AtaIdentify(out byte[] ataBuf, out _);

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

    protected Device() {}
}