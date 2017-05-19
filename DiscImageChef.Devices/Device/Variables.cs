// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Variables.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains various device variables.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        Interop.PlatformID platformID;
        object fd;
        bool error;
        int lastError;
        readonly DeviceType type;
        readonly string manufacturer;
        readonly string model;
        readonly string revision;
        readonly string serial;
        readonly Decoders.SCSI.PeripheralDeviceTypes scsiType;
        readonly bool removable;
        readonly bool usb;
        readonly ushort usbVendor;
        readonly ushort usbProduct;
        readonly byte[] usbDescriptors;
        readonly string usbManufacturerString;
        readonly string usbProductString;
        readonly string usbSerialString;
        readonly bool firewire;
        readonly ulong firewireGuid;
        readonly uint firewireModel;
        readonly string firewireModelName;
        readonly uint firewireVendor;
        readonly string firewireVendorName;
        readonly bool compactFlash;
        readonly bool pcmcia;
        readonly byte[] cis;

        /// <summary>
        /// Gets the Platform ID for this device
        /// </summary>
        /// <value>The Platform ID</value>
        public Interop.PlatformID PlatformID
        {
            get
            {
                return platformID;
            }
        }

        /// <summary>
        /// Gets the file handle representing this device
        /// </summary>
        /// <value>The file handle</value>
        public object FileHandle
        {
            get
            {
                return fd;
            }
        }

        /// <summary>
        /// Gets or sets the standard timeout for commands sent to this device
        /// </summary>
        /// <value>The timeout in seconds</value>
        public uint Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Device"/> is in error.
        /// </summary>
        /// <value><c>true</c> if error; otherwise, <c>false</c>.</value>
        public bool Error
        {
            get
            {
                return error;
            }
        }

        /// <summary>
        /// Gets the last error number.
        /// </summary>
        /// <value>The last error.</value>
        public int LastError
        {
            get
            {
                return lastError;
            }
        }

        /// <summary>
        /// Gets the device type.
        /// </summary>
        /// <value>The device type.</value>
        public DeviceType Type
        {
            get
            {
                return type;
            }
        }

        /// <summary>
        /// Gets the device's manufacturer
        /// </summary>
        /// <value>The manufacturer.</value>
        public string Manufacturer
        {
            get
            {
                return manufacturer;
            }
        }

        /// <summary>
        /// Gets the device model
        /// </summary>
        /// <value>The model.</value>
        public string Model
        {
            get
            {
                return model;
            }
        }

        /// <summary>
        /// Gets the device's revision.
        /// </summary>
        /// <value>The revision.</value>
        public string Revision
        {
            get
            {
                return revision;
            }
        }

        /// <summary>
        /// Gets the device's serial number.
        /// </summary>
        /// <value>The serial number.</value>
        public string Serial
        {
            get
            {
                return serial;
            }
        }

        /// <summary>
        /// Gets the device's SCSI peripheral device type
        /// </summary>
        /// <value>The SCSI peripheral device type.</value>
        public Decoders.SCSI.PeripheralDeviceTypes SCSIType
        {
            get
            {
                return scsiType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this device's media is removable.
        /// </summary>
        /// <value><c>true</c> if this device's media is removable; otherwise, <c>false</c>.</value>
        public bool IsRemovable
        {
            get
            {
                return removable;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this device is attached via USB.
        /// </summary>
        /// <value><c>true</c> if this device is attached via USB; otherwise, <c>false</c>.</value>
        public bool IsUSB
        {
            get
            {
                return usb;
            }
        }

        /// <summary>
        /// Gets the USB vendor ID.
        /// </summary>
        /// <value>The USB vendor ID.</value>
        public ushort USBVendorID
        {
            get
            {
                return usbVendor;
            }
        }

        /// <summary>
        /// Gets the USB product ID.
        /// </summary>
        /// <value>The USB product ID.</value>
        public ushort USBProductID
        {
            get
            {
                return usbProduct;
            }
        }

        /// <summary>
        /// Gets the USB descriptors.
        /// </summary>
        /// <value>The USB descriptors.</value>
        public byte[] USBDescriptors
        {
            get
            {
                return usbDescriptors;
            }
        }

        /// <summary>
        /// Gets the USB manufacturer string.
        /// </summary>
        /// <value>The USB manufacturer string.</value>
        public string USBManufacturerString
        {
            get
            {
                return usbManufacturerString;
            }
        }

        /// <summary>
        /// Gets the USB product string.
        /// </summary>
        /// <value>The USB product string.</value>
        public string USBProductString
        {
            get
            {
                return usbProductString;
            }
        }

        /// <summary>
        /// Gets the USB serial string.
        /// </summary>
        /// <value>The USB serial string.</value>
        public string USBSerialString
        {
            get
            {
                return usbSerialString;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this device is attached via FireWire.
        /// </summary>
        /// <value><c>true</c> if this device is attached via FireWire; otherwise, <c>false</c>.</value>
        public bool IsFireWire { get { return firewire; } }

        /// <summary>
        /// Gets the FireWire GUID
        /// </summary>
        /// <value>The FireWire GUID.</value>
        public ulong FireWireGUID { get { return firewireGuid; } }

        /// <summary>
        /// Gets the FireWire model number
        /// </summary>
        /// <value>The FireWire model.</value>
        public uint FireWireModel { get { return firewireModel; } }

        /// <summary>
        /// Gets the FireWire model name.
        /// </summary>
        /// <value>The FireWire model name.</value>
        public string FireWireModelName { get { return firewireModelName; } }

        /// <summary>
        /// Gets the FireWire vendor number.
        /// </summary>
        /// <value>The FireWire vendor number.</value>
        public uint FireWireVendor { get { return firewireVendor; } }

        /// <summary>
        /// Gets the FireWire vendor name.
        /// </summary>
        /// <value>The FireWire vendor name.</value>
        public string FireWireVendorName { get { return firewireVendorName; } }

        /// <summary>
        /// Gets a value indicating whether this device is a CompactFlash device.
        /// </summary>
        /// <value><c>true</c> if this device is a CompactFlash device; otherwise, <c>false</c>.</value>
        public bool IsCompactFlash
        {
            get
            {
                return compactFlash;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this device is a PCMCIA device.
        /// </summary>
        /// <value><c>true</c> if this device is a PCMCIA device; otherwise, <c>false</c>.</value>
        public bool IsPCMCIA
        {
            get
            {
                return pcmcia;
            }
        }

        /// <summary>
        /// Contains the PCMCIA CIS if applicable
        /// </summary>
        public byte[] CIS
        {
            get
            {
                return cis;
            }
        }
    }
}

