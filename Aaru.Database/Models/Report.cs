// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Report.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Model for storing uploaded device reports in database.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Metadata;

namespace Aaru.Database.Models
{
    public class Report : DeviceReportV2
    {
        public Report()
        {
            Created  = DateTime.UtcNow;
            Uploaded = false;
        }

        public Report(DeviceReportV2 report)
        {
            ATA            = report.ATA;
            ATAPI          = report.ATAPI;
            CompactFlash   = report.CompactFlash;
            FireWire       = report.FireWire;
            Created        = DateTime.UtcNow;
            MultiMediaCard = report.MultiMediaCard;
            PCMCIA         = report.PCMCIA;
            SCSI           = report.SCSI;
            SecureDigital  = report.SecureDigital;
            USB            = report.USB;
            Uploaded       = false;
            Manufacturer   = report.Manufacturer;
            Model          = report.Model;
            Revision       = report.Revision;
            Type           = report.Type;
        }

        public DateTime Created  { get; set; }
        public bool     Uploaded { get; set; }
    }
}