// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : this.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for device information.
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
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models
{
    public class Device : DatedEntity
    {
        public Device()
        {
            WhenAdded = DateTime.UtcNow;
        }

        public Device(DeviceReport report)
        {
            WhenAdded = DateTime.UtcNow;
            if(report.SecureDigital       != null) Type = DeviceType.SecureDigital;
            else if(report.MultiMediaCard != null) Type = DeviceType.MMC;
            else if(report.FireWire       != null) Type = DeviceType.FireWire;
            else if(report.USB            != null) Type = DeviceType.USB;
            else if(report.PCMCIA         != null) Type = DeviceType.PCMCIA;
            else if(report.ATAPI          != null) Type = DeviceType.ATAPI;
            else if(report.ATA            != null) Type = DeviceType.ATA;
            else if(report.SCSI?.Inquiry  != null) Type = DeviceType.SCSI;

            if(report.CompactFlashSpecified && report.CompactFlash) Type = DeviceType.CompactFlash;

            if(!string.IsNullOrWhiteSpace(report.FireWire?.Manufacturer)) Manufacturer = report.FireWire.Manufacturer;
            else if(!string.IsNullOrWhiteSpace(report.USB?.Manufacturer)) Manufacturer = report.USB.Manufacturer;
            else if(!string.IsNullOrWhiteSpace(report.SCSI?.Inquiry?.VendorIdentification))
                Manufacturer =
                    report.SCSI.Inquiry.VendorIdentification;
            else if(!string.IsNullOrWhiteSpace(report.PCMCIA?.Manufacturer)) Manufacturer = report.PCMCIA.Manufacturer;
            else if(!string.IsNullOrWhiteSpace(report.ATAPI?.Model))
            {
                string[] atapiSplit = report.ATAPI.Model.Split(' ');
                Manufacturer = atapiSplit.Length > 1 ? atapiSplit[0] : report.ATAPI.Model;
            }
            else if(!string.IsNullOrWhiteSpace(report.ATA?.Model))
            {
                string[] ataSplit = report.ATA.Model.Split(' ');
                Manufacturer = ataSplit.Length > 1 ? ataSplit[0] : report.ATA.Model;
            }

            if(!string.IsNullOrWhiteSpace(report.FireWire?.Product)) Model = report.FireWire.Product;
            else if(!string.IsNullOrWhiteSpace(report.USB?.Product)) Model = report.USB.Product;
            else if(!string.IsNullOrWhiteSpace(report.SCSI?.Inquiry?.ProductIdentification))
                Model =
                    report.SCSI.Inquiry.ProductIdentification;
            else if(!string.IsNullOrWhiteSpace(report.PCMCIA?.ProductName)) Model = report.PCMCIA.ProductName;
            else if(!string.IsNullOrWhiteSpace(report.ATAPI?.Model))
            {
                string[] atapiSplit = report.ATAPI.Model.Split(' ');
                Model = atapiSplit.Length > 1 ? report.ATAPI.Model.Substring(atapiSplit[0].Length + 1) : null;
            }
            else if(!string.IsNullOrWhiteSpace(report.ATA?.Model))
            {
                string[] ataSplit = report.ATA.Model.Split(' ');
                Model = ataSplit.Length > 1 ? report.ATA.Model.Substring(ataSplit[0].Length + 1) : null;
            }

            if(!string.IsNullOrWhiteSpace(report.SCSI?.Inquiry?.ProductRevisionLevel))
                Revision = report.SCSI.Inquiry.ProductRevisionLevel;
            else if(!string.IsNullOrWhiteSpace(report.ATAPI?.FirmwareRevision))
                Revision                                                               = report.ATAPI.FirmwareRevision;
            else if(!string.IsNullOrWhiteSpace(report.ATA?.FirmwareRevision)) Revision = report.ATA.FirmwareRevision;

            USB            = USB.MapUsb(report.USB);
            FireWire       = FireWire.MapFirewire(report.FireWire);
            PCMCIA         = PCMCIA.MapPcmcia(report.PCMCIA);
            ATA            = ATA.MapAta(report.ATA);
            ATAPI          = ATA.MapAta(report.ATAPI);
            SCSI           = Models.SCSI.SCSI.MapScsi(report.SCSI);
            MultiMediaCard = SecureDigital.MapSd(report.MultiMediaCard);
            SecureDigital  = SecureDigital.MapSd(report.SecureDigital);

            WhenAdded = DateTime.UtcNow;
            IsValid   = true;
        }

        public Device(string manufacturer, string model, string revision, DeviceType type, DeviceReport report)
        {
            WhenAdded    = DateTime.UtcNow;
            Manufacturer = manufacturer;
            Model        = model;
            Revision     = revision;
            Type         = type;

            USB            = USB.MapUsb(report.USB);
            FireWire       = FireWire.MapFirewire(report.FireWire);
            PCMCIA         = PCMCIA.MapPcmcia(report.PCMCIA);
            ATA            = ATA.MapAta(report.ATA);
            ATAPI          = ATA.MapAta(report.ATAPI);
            SCSI           = Models.SCSI.SCSI.MapScsi(report.SCSI);
            MultiMediaCard = SecureDigital.MapSd(report.MultiMediaCard);
            SecureDigital  = SecureDigital.MapSd(report.SecureDigital);

            WhenAdded = DateTime.UtcNow;
            IsValid   = true;
        }

        public string        Manufacturer   { get; set; }
        public string        Model          { get; set; }
        public string        Revision       { get; set; }
        public DeviceType    Type           { get; set; }
        public USB           USB            { get; set; }
        public FireWire      FireWire       { get; set; }
        public PCMCIA        PCMCIA         { get; set; }
        public ATA           ATA            { get; set; }
        public ATA           ATAPI          { get; set; }
        public SCSI.SCSI     SCSI           { get; set; }
        public SecureDigital MultiMediaCard { get; set; }
        public SecureDigital SecureDigital  { get; set; }
        public bool          IsValid        { get; set; }
        public ulong         TimesSeen      { get; set; }
    }

    public enum DeviceType
    {
        Unknown,
        ATA,
        ATAPI,
        SCSI,
        SecureDigital,
        MMC,
        NVMe,
        PCMCIA,
        CompactFlash,
        FireWire,
        USB
    }
}