// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Info
{
    public partial class DeviceInfo
    {
        public byte[]                                 AtaIdentify            { get; }
        public byte[]                                 AtapiIdentify          { get; }
        public byte[]                                 ScsiInquiryData        { get; }
        public Inquiry.SCSIInquiry?                   ScsiInquiry            { get; }
        public AtaErrorRegistersChs?                  AtaMcptError           { get; }
        public Dictionary<byte, byte[]>               ScsiEvpdPages          { get; }
        public Modes.DecodedMode?                     ScsiMode               { get; }
        public byte[]                                 ScsiModeSense6         { get; }
        public byte[]                                 ScsiModeSense10        { get; }
        public byte[]                                 MmcConfiguration       { get; }
        public Plextor                                PlextorFeatures        { get; }
        public KreonFeatures                          KreonFeatures          { get; }
        public byte[]                                 BlockLimits            { get; }
        public byte[]                                 DensitySupport         { get; }
        public DensitySupport.DensitySupportHeader?   DensitySupportHeader   { get; }
        public byte[]                                 MediumDensitySupport   { get; }
        public DensitySupport.MediaTypeSupportHeader? MediaTypeSupportHeader { get; }
        public byte[]                                 CID                    { get; }
        public byte[]                                 CSD                    { get; }
        public byte[]                                 ExtendedCSD            { get; }
        public byte[]                                 SCR                    { get; }
        public byte[]                                 OCR                    { get; }
        public DeviceType                             Type                   { get; }
        public string                                 Manufacturer           { get; }
        public string                                 Model                  { get; }
        public string                                 FirmwareRevision       { get; }
        public string                                 Serial                 { get; }
        public PeripheralDeviceTypes                  ScsiType               { get; }
        public bool                                   IsRemovable            { get; }
        public bool                                   IsUsb                  { get; }
        public ushort                                 UsbVendorId            { get; }
        public ushort                                 UsbProductId           { get; }
        public byte[]                                 UsbDescriptors         { get; }
        public string                                 UsbManufacturerString  { get; }
        public string                                 UsbProductString       { get; }
        public string                                 UsbSerialString        { get; }
        public bool                                   IsFireWire             { get; }
        public ulong                                  FireWireGuid           { get; }
        public uint                                   FireWireModel          { get; }
        public string                                 FireWireModelName      { get; }
        public uint                                   FireWireVendor         { get; }
        public string                                 FireWireVendorName     { get; }
        public bool                                   IsCompactFlash         { get; }
        public bool                                   IsPcmcia               { get; }
        public byte[]                                 Cis                    { get; }
    }
}