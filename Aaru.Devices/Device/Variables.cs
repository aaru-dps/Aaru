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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs.Devices.SCSI;

namespace Aaru.Devices;

public partial class Device
{
    const string ATA_MODULE_NAME  = "ATA Device";
    const string SCSI_MODULE_NAME = "SCSI Device";
    const string SD_MODULE_NAME   = "SecureDigital Device";
    const string MMC_MODULE_NAME  = "MultiMediaCard Device";

    /// <summary>Gets the Platform ID for this device</summary>
    /// <value>The Platform ID</value>
    public PlatformID PlatformId { get; private protected set; }

    /// <summary>Gets or sets the standard timeout for commands sent to this device</summary>
    /// <value>The timeout in seconds</value>
    public uint Timeout { get; set; }

    /// <summary>Gets a value indicating whether this <see cref="Device" /> is in error.</summary>
    /// <value><c>true</c> if error; otherwise, <c>false</c>.</value>
    public bool Error { get; private protected set; }

    /// <summary>Gets the last error number.</summary>
    /// <value>The last error.</value>
    public int LastError { get; private protected set; }

    /// <summary>Gets the device type.</summary>
    /// <value>The device type.</value>
    public DeviceType Type { get; private protected set; }

    /// <summary>Gets the device's manufacturer</summary>
    /// <value>The manufacturer.</value>
    public string Manufacturer { get; private protected set; }

    /// <summary>Gets the device model</summary>
    /// <value>The model.</value>
    public string Model { get; private protected set; }

    /// <summary>Gets the device's firmware version.</summary>
    /// <value>The firmware version.</value>
    public string FirmwareRevision { get; private protected set; }

    /// <summary>Gets the device's serial number.</summary>
    /// <value>The serial number.</value>
    public string Serial { get; private protected set; }

    /// <summary>Gets the device's SCSI peripheral device type</summary>
    /// <value>The SCSI peripheral device type.</value>
    public PeripheralDeviceTypes ScsiType { get; private protected set; }

    /// <summary>Gets a value indicating whether this device's media is removable.</summary>
    /// <value><c>true</c> if this device's media is removable; otherwise, <c>false</c>.</value>
    public bool IsRemovable { get; private protected set; }

    /// <summary>Gets a value indicating whether this device is attached via USB.</summary>
    /// <value><c>true</c> if this device is attached via USB; otherwise, <c>false</c>.</value>
    public bool IsUsb { get; private protected set; }

    /// <summary>Gets the USB vendor ID.</summary>
    /// <value>The USB vendor ID.</value>
    public ushort UsbVendorId => UsbVendor;

    /// <summary>Gets the USB product ID.</summary>
    /// <value>The USB product ID.</value>
    public ushort UsbProductId => UsbProduct;

    /// <summary>Gets the USB descriptors.</summary>
    /// <value>The USB descriptors.</value>
    public byte[] UsbDescriptors { get; private protected set; }

    /// <summary>Gets the USB manufacturer string.</summary>
    /// <value>The USB manufacturer string.</value>
    public string UsbManufacturerString { get; private protected set; }

    /// <summary>Gets the USB product string.</summary>
    /// <value>The USB product string.</value>
    public string UsbProductString { get; private protected set; }

    /// <summary>Gets the USB serial string.</summary>
    /// <value>The USB serial string.</value>
    public string UsbSerialString { get; private protected set; }

    /// <summary>Gets a value indicating whether this device is attached via FireWire.</summary>
    /// <value><c>true</c> if this device is attached via FireWire; otherwise, <c>false</c>.</value>
    public bool IsFireWire { get; private protected set; }

    /// <summary>Gets the FireWire GUID</summary>
    /// <value>The FireWire GUID.</value>
    public ulong FireWireGuid => FirewireGuid;

    /// <summary>Gets the FireWire model number</summary>
    /// <value>The FireWire model.</value>
    public uint FireWireModel => FirewireModel;

    /// <summary>Gets the FireWire model name.</summary>
    /// <value>The FireWire model name.</value>
    public string FireWireModelName { get; private protected set; }

    /// <summary>Gets the FireWire vendor number.</summary>
    /// <value>The FireWire vendor number.</value>
    public uint FireWireVendor => FirewireVendor;

    /// <summary>Gets the FireWire vendor name.</summary>
    /// <value>The FireWire vendor name.</value>
    public string FireWireVendorName { get; private protected set; }

    /// <summary>Gets a value indicating whether this device is a CompactFlash device.</summary>
    /// <value><c>true</c> if this device is a CompactFlash device; otherwise, <c>false</c>.</value>
    public bool IsCompactFlash { get; private set; }

    /// <summary>Gets a value indicating whether this device is a PCMCIA device.</summary>
    /// <value><c>true</c> if this device is a PCMCIA device; otherwise, <c>false</c>.</value>
    public bool IsPcmcia { get; private protected set; }

    /// <summary>Contains the PCMCIA CIS if applicable</summary>
    public byte[] Cis { get; private protected set; }

    // MMC and SecureDigital, values that need to be get with card idle, something that may
    // not be possible to do but usually is already done by the SDHCI driver.
#pragma warning disable PH2070 // Risks are known. TODO: Maybe protected property?
    private protected byte[] CachedCid;
    private protected byte[] CachedCsd;
    private protected byte[] CachedOcr;
    private protected byte[] CachedScr;

    private protected string DevicePath;
    private protected ulong  FirewireGuid;
    private protected uint   FirewireModel;
    private protected uint   FirewireVendor;
    private protected ushort UsbProduct;
    private protected ushort UsbVendor;
#pragma warning restore PH2070
}