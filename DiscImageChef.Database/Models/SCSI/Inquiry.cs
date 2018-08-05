// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Inquiry.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SCSI INQUIRY.
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

using System.Collections.Generic;
using System.Linq;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Decoders.SCSI;

namespace DiscImageChef.Database.Models.SCSI
{
    public class Inquiry : BaseEntity
    {
        public bool                  AccessControlCoordinator { get; set; }
        public bool                  ACKRequests              { get; set; }
        public bool                  AERCSupported            { get; set; }
        public bool                  Address16                { get; set; }
        public bool                  Address32                { get; set; }
        public byte?                 ANSIVersion              { get; set; }
        public TGPSValues            AsymmetricalLUNAccess    { get; set; }
        public bool                  BasicQueueing            { get; set; }
        public byte?                 DeviceTypeModifier       { get; set; }
        public byte?                 ECMAVersion              { get; set; }
        public bool                  EnclosureServices        { get; set; }
        public bool                  HierarchicalLUN          { get; set; }
        public bool                  IUS                      { get; set; }
        public byte?                 ISOVersion               { get; set; }
        public bool                  LinkedCommands           { get; set; }
        public bool                  MediumChanger            { get; set; }
        public bool                  MultiPortDevice          { get; set; }
        public bool                  NormalACA                { get; set; }
        public PeripheralDeviceTypes PeripheralDeviceType     { get; set; }
        public PeripheralQualifiers  PeripheralQualifier      { get; set; }
        public string                ProductIdentification    { get; set; }
        public string                ProductRevisionLevel     { get; set; }
        public bool                  Protection               { get; set; }
        public bool                  QAS                      { get; set; }
        public bool                  RelativeAddressing       { get; set; }
        public bool                  Removable                { get; set; }
        public byte?                 ResponseDataFormat       { get; set; }
        public bool                  TaggedCommandQueue       { get; set; }
        public bool                  TerminateTaskSupported   { get; set; }
        public bool                  ThirdPartyCopy           { get; set; }
        public bool                  TranferDisable           { get; set; }
        public bool                  SoftReset                { get; set; }
        public SPIClocking           SPIClocking              { get; set; }
        public bool                  StorageArrayController   { get; set; }
        public bool                  SyncTransfer             { get; set; }
        public string                VendorIdentification     { get; set; }
        public List<UshortClass>     VersionDescriptors       { get; set; }
        public bool                  WideBus16                { get; set; }
        public bool                  WideBus32                { get; set; }
        public byte[]                Data                     { get; set; }

        public static Inquiry MapInquiry(scsiInquiryType oldInquiry)
        {
            if(oldInquiry == null) return null;

            Inquiry newInquiry = new Inquiry
            {
                AccessControlCoordinator = oldInquiry.AccessControlCoordinator,
                ACKRequests              = oldInquiry.ACKRequests,
                AERCSupported            = oldInquiry.AERCSupported,
                Address16                = oldInquiry.Address16,
                Address32                = oldInquiry.Address32,
                AsymmetricalLUNAccess    = oldInquiry.AsymmetricalLUNAccess,
                BasicQueueing            = oldInquiry.BasicQueueing,
                EnclosureServices        = oldInquiry.EnclosureServices,
                HierarchicalLUN          = oldInquiry.HierarchicalLUN,
                IUS                      = oldInquiry.IUS,
                LinkedCommands           = oldInquiry.LinkedCommands,
                MediumChanger            = oldInquiry.MediumChanger,
                MultiPortDevice          = oldInquiry.MultiPortDevice,
                NormalACA                = oldInquiry.NormalACA,
                PeripheralDeviceType     = oldInquiry.PeripheralDeviceType,
                PeripheralQualifier      = oldInquiry.PeripheralQualifier,
                Protection               = oldInquiry.Protection,
                QAS                      = oldInquiry.QAS,
                RelativeAddressing       = oldInquiry.RelativeAddressing,
                Removable                = oldInquiry.Removable,
                TaggedCommandQueue       = oldInquiry.TaggedCommandQueue,
                TerminateTaskSupported   = oldInquiry.TerminateTaskSupported,
                ThirdPartyCopy           = oldInquiry.ThirdPartyCopy,
                TranferDisable           = oldInquiry.TranferDisable,
                SoftReset                = oldInquiry.SoftReset,
                SPIClocking              = oldInquiry.SPIClocking,
                StorageArrayController   = oldInquiry.StorageArrayController,
                SyncTransfer             = oldInquiry.SyncTransfer,
                WideBus16                = oldInquiry.WideBus16,
                WideBus32                = oldInquiry.WideBus32,
                Data                     = oldInquiry.Data
            };

            if(oldInquiry.ANSIVersionSpecified) newInquiry.ANSIVersion               = oldInquiry.ANSIVersion;
            if(oldInquiry.DeviceTypeModifierSpecified) newInquiry.DeviceTypeModifier = oldInquiry.DeviceTypeModifier;
            if(oldInquiry.ECMAVersionSpecified) newInquiry.ECMAVersion               = oldInquiry.ECMAVersion;
            if(oldInquiry.ISOVersionSpecified) newInquiry.ISOVersion                 = oldInquiry.ISOVersion;
            if(oldInquiry.ProductIdentificationSpecified)
                newInquiry.ProductIdentification = oldInquiry.ProductIdentification;
            if(oldInquiry.ProductRevisionLevelSpecified)
                newInquiry.ProductRevisionLevel = oldInquiry.ProductRevisionLevel;
            if(oldInquiry.ResponseDataFormatSpecified) newInquiry.ResponseDataFormat = oldInquiry.ResponseDataFormat;
            if(oldInquiry.VendorIdentificationSpecified)
                newInquiry.VendorIdentification = oldInquiry.VendorIdentification;
            if(oldInquiry.VersionDescriptors == null) return newInquiry;

            newInquiry.VersionDescriptors =
                new List<UshortClass>(oldInquiry.VersionDescriptors.Select(t => new UshortClass {Value = t}));

            return newInquiry;
        }
    }
}