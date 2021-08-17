// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs.Devices.SCSI;

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        readonly ushort _usbVendor;
        readonly ushort _usbProduct;
        readonly ulong  _firewireGuid;
        readonly uint   _firewireModel;
        readonly uint   _firewireVendor;

        // MMC and SecureDigital, values that need to be get with card idle, something that may
        // not be possible to do but usually is already done by the SDHCI driver.
        readonly byte[] _cachedCsd;
        readonly byte[] _cachedCid;
        readonly byte[] _cachedScr;
        readonly byte[] _cachedOcr;

        /// <summary>Gets the Platform ID for this device</summary>
        /// <value>The Platform ID</value>
        public PlatformID PlatformId { get; }

        /// <summary>Gets the file handle representing this device</summary>
        /// <value>The file handle</value>
        public object FileHandle { get; private set; }

        /// <summary>Gets or sets the standard timeout for commands sent to this device</summary>
        /// <value>The timeout in seconds</value>
        public uint Timeout { get; }

        /// <summary>Gets a value indicating whether this <see cref="Device" /> is in error.</summary>
        /// <value><c>true</c> if error; otherwise, <c>false</c>.</value>
        public bool Error { get; private set; }

        /// <summary>Gets the last error number.</summary>
        /// <value>The last error.</value>
        public int LastError { get; private set; }

        /// <summary>Gets the device type.</summary>
        /// <value>The device type.</value>
        public DeviceType Type { get; }

        /// <summary>Gets the device's manufacturer</summary>
        /// <value>The manufacturer.</value>
        public string Manufacturer { get; }

        /// <summary>Gets the device model</summary>
        /// <value>The model.</value>
        public string Model { get; }

        /// <summary>Gets the device's firmware version.</summary>
        /// <value>The firmware version.</value>
        public string FirmwareRevision { get; }

        /// <summary>Gets the device's serial number.</summary>
        /// <value>The serial number.</value>
        public string Serial { get; }

        /// <summary>Gets the device's SCSI peripheral device type</summary>
        /// <value>The SCSI peripheral device type.</value>
        public PeripheralDeviceTypes ScsiType { get; }

        /// <summary>Gets a value indicating whether this device's media is removable.</summary>
        /// <value><c>true</c> if this device's media is removable; otherwise, <c>false</c>.</value>
        public bool IsRemovable { get; }

        /// <summary>Gets a value indicating whether this device is attached via USB.</summary>
        /// <value><c>true</c> if this device is attached via USB; otherwise, <c>false</c>.</value>
        public bool IsUsb { get; }

        /// <summary>Gets the USB vendor ID.</summary>
        /// <value>The USB vendor ID.</value>
        public ushort UsbVendorId => _usbVendor;

        /// <summary>Gets the USB product ID.</summary>
        /// <value>The USB product ID.</value>
        public ushort UsbProductId => _usbProduct;

        /// <summary>Gets the USB descriptors.</summary>
        /// <value>The USB descriptors.</value>
        public byte[] UsbDescriptors { get; }

        /// <summary>Gets the USB manufacturer string.</summary>
        /// <value>The USB manufacturer string.</value>
        public string UsbManufacturerString { get; }

        /// <summary>Gets the USB product string.</summary>
        /// <value>The USB product string.</value>
        public string UsbProductString { get; }

        /// <summary>Gets the USB serial string.</summary>
        /// <value>The USB serial string.</value>
        public string UsbSerialString { get; }

        /// <summary>Gets a value indicating whether this device is attached via FireWire.</summary>
        /// <value><c>true</c> if this device is attached via FireWire; otherwise, <c>false</c>.</value>
        public bool IsFireWire { get; }

        /// <summary>Gets the FireWire GUID</summary>
        /// <value>The FireWire GUID.</value>
        public ulong FireWireGuid => _firewireGuid;

        /// <summary>Gets the FireWire model number</summary>
        /// <value>The FireWire model.</value>
        public uint FireWireModel => _firewireModel;

        /// <summary>Gets the FireWire model name.</summary>
        /// <value>The FireWire model name.</value>
        public string FireWireModelName { get; }

        /// <summary>Gets the FireWire vendor number.</summary>
        /// <value>The FireWire vendor number.</value>
        public uint FireWireVendor => _firewireVendor;

        /// <summary>Gets the FireWire vendor name.</summary>
        /// <value>The FireWire vendor name.</value>
        public string FireWireVendorName { get; }

        /// <summary>Gets a value indicating whether this device is a CompactFlash device.</summary>
        /// <value><c>true</c> if this device is a CompactFlash device; otherwise, <c>false</c>.</value>
        public bool IsCompactFlash { get; }

        /// <summary>Gets a value indicating whether this device is a PCMCIA device.</summary>
        /// <value><c>true</c> if this device is a PCMCIA device; otherwise, <c>false</c>.</value>
        public bool IsPcmcia { get; }

        /// <summary>Contains the PCMCIA CIS if applicable</summary>
        public byte[] Cis { get; }

        readonly Remote.Remote _remote;
        bool?                  _isRemoteAdmin;
        readonly string        _devicePath;

        /// <summary>Returns if remote is running under administrative (aka root) privileges</summary>
        public bool IsRemoteAdmin
        {
            get
            {
                _isRemoteAdmin ??= _remote.IsRoot;

                return _isRemoteAdmin == true;
            }
        }

        /// <summary>Current device is remote</summary>
        public bool IsRemote => _remote != null;
        /// <summary>Remote application</summary>
        public string RemoteApplication => _remote?.ServerApplication;
        /// <summary>Remote application server</summary>
        public string RemoteVersion => _remote?.ServerVersion;
        /// <summary>Remote operating system name</summary>
        public string RemoteOperatingSystem => _remote?.ServerOperatingSystem;
        /// <summary>Remote operating system version</summary>
        public string RemoteOperatingSystemVersion => _remote?.ServerOperatingSystemVersion;
        /// <summary>Remote architecture</summary>
        public string RemoteArchitecture => _remote?.ServerArchitecture;
        /// <summary>Remote protocol version</summary>
        public int RemoteProtocolVersion => _remote?.ServerProtocolVersion ?? 0;
    }
}