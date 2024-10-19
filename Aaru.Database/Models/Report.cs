// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Metadata;

namespace Aaru.Database.Models;

/// <summary>Device report</summary>
public class Report : DeviceReport
{
    /// <summary>Builds an empty device report</summary>
    public Report()
    {
        Created  = DateTime.UtcNow;
        Uploaded = false;
    }

    /// <summary>Builds a device report model from a device report</summary>
    /// <param name="report">Device report</param>
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    public Report(DeviceReport report)
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

    /// <summary>Date when the device report was created</summary>
    public DateTime Created { get; set; }

    /// <summary>If this model has already been upload</summary>
    public bool Uploaded { get; set; }
}