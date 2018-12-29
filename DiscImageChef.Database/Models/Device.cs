// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Device.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Model for storing processed device reports in database.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/
using System;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models
{
    public class Device : DeviceReportV2
    {
        public Device()
        {
            LastSynchronized = DateTime.UtcNow;
        }

        public Device(DeviceReportV2 report)
        {
            ATA              = report.ATA;
            ATAPI            = report.ATAPI;
            CompactFlash     = report.CompactFlash;
            FireWire         = report.FireWire;
            LastSynchronized = DateTime.UtcNow;
            MultiMediaCard   = report.MultiMediaCard;
            PCMCIA           = report.PCMCIA;
            SCSI             = report.SCSI;
            SecureDigital    = report.SecureDigital;
            USB              = report.USB;
            Manufacturer     = report.Manufacturer;
            Model            = report.Model;
            Revision         = report.Revision;
            Type             = report.Type;
        }

        public DateTime LastSynchronized { get; set; }
    }
}