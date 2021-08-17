// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the device info properties.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.ATA;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Devices;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;

namespace Aaru.Core.Devices.Info
{
    public partial class DeviceInfo
    {
        /// <summary>Raw IDENTIFY DEVICE response</summary>
        public byte[] AtaIdentify { get; }
        /// <summary>Raw IDENTIFY PACKET DEVICE response</summary>
        public byte[] AtapiIdentify { get; }
        /// <summary>Raw INQUIRY response</summary>
        public byte[] ScsiInquiryData { get; }
        /// <summary>Decoded INQUIRY response</summary>
        public Inquiry? ScsiInquiry { get; }
        /// <summary>Response for ATA Memory Card Pass Through</summary>
        public AtaErrorRegistersChs? AtaMcptError { get; }
        /// <summary>List of raw EVPD page indexed by page number</summary>
        public Dictionary<byte, byte[]> ScsiEvpdPages { get; }
        /// <summary>Decoded MODE SENSE response</summary>
        public Modes.DecodedMode? ScsiMode { get; }
        /// <summary>Raw MODE SENSE(6) response</summary>
        public byte[] ScsiModeSense6 { get; }
        /// <summary>Raw MODE SENSE(10) response</summary>
        public byte[] ScsiModeSense10 { get; }
        /// <summary>Raw GET CONFIGURATION response</summary>
        public byte[] MmcConfiguration { get; }
        /// <summary>Decoded Plextor features</summary>
        public Plextor PlextorFeatures { get; }
        /// <summary>Decoded Kreon features</summary>
        public KreonFeatures KreonFeatures { get; }
        /// <summary>Raw GET BLOCK LIMITS support</summary>
        public byte[] BlockLimits { get; }
        /// <summary>Raw density support</summary>
        public byte[] DensitySupport { get; }
        /// <summary>Decoded density support</summary>
        public DensitySupport.DensitySupportHeader? DensitySupportHeader { get; }
        /// <summary>Raw medium density support</summary>
        public byte[] MediumDensitySupport { get; }
        /// <summary>Decoded medium density support</summary>
        public DensitySupport.MediaTypeSupportHeader? MediaTypeSupportHeader { get; }
        /// <summary>Raw CID registers</summary>
        public byte[] CID { get; }
        /// <summary>Raw CSD</summary>
        public byte[] CSD { get; }
        /// <summary>Raw extended CSD</summary>
        public byte[] ExtendedCSD { get; }
        /// <summary>Raw SCR registers</summary>
        public byte[] SCR { get; }
        /// <summary>Raw OCR registers</summary>
        public byte[] OCR { get; }
        /// <summary>Aaru's device type</summary>
        public DeviceType Type { get; }
        /// <summary>Device manufacturer</summary>
        public string Manufacturer { get; }
        /// <summary>Device model</summary>
        public string Model { get; }
        /// <summary>Device firmware version or revision</summary>
        public string FirmwareRevision { get; }
        /// <summary>Device serial number</summary>
        public string Serial { get; }
        /// <summary>SCSI Peripheral Device Type</summary>
        public PeripheralDeviceTypes ScsiType { get; }
        /// <summary>Is media removable from device?</summary>
        public bool IsRemovable { get; }
        /// <summary>Is device attached via USB?</summary>
        public bool IsUsb { get; }
        /// <summary>USB vendor ID</summary>
        public ushort UsbVendorId { get; }
        /// <summary>USB product ID</summary>
        public ushort UsbProductId { get; }
        /// <summary>Raw USB descriptors</summary>
        public byte[] UsbDescriptors { get; }
        /// <summary>USB manufacturer string</summary>
        public string UsbManufacturerString { get; }
        /// <summary>USB product string</summary>
        public string UsbProductString { get; }
        /// <summary>USB serial number string</summary>
        public string UsbSerialString { get; }
        /// <summary>Is device attached via FireWire?</summary>
        public bool IsFireWire { get; }
        /// <summary>FireWire's device GUID</summary>
        public ulong FireWireGuid { get; }
        /// <summary>FireWire's device model ID</summary>
        public uint FireWireModel { get; }
        /// <summary>FireWire's device model name</summary>
        public string FireWireModelName { get; }
        /// <summary>FireWire's device vendor ID</summary>
        public uint FireWireVendor { get; }
        /// <summary>FireWire's device vendor name</summary>
        public string FireWireVendorName { get; }
        /// <summary>Is device a CompactFlash device?</summary>
        public bool IsCompactFlash { get; }
        /// <summary>Is device a PCMCIA or CardBus device?</summary>
        public bool IsPcmcia { get; }
        /// <summary>PCMCIA/CardBus CIS</summary>
        public byte[] Cis { get; }
        /// <summary>MMC device CSS/CPRM Region Protection Code</summary>
        public CSS_CPRM.RegionalPlaybackControlState? RPC { get; }
    }
}