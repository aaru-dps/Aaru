// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes.Metadata;

namespace Aaru.Database.Models
{
    /// <summary>Known device</summary>
    public class Device : DeviceReportV2
    {
        /// <summary>Builds an empty device</summary>
        public Device() => LastSynchronized = DateTime.UtcNow;

        /// <summary>Builds a device from a device report</summary>
        /// <param name="report">Device report</param>
        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public Device(DeviceReportV2 report)
        {
            ATA                       = report.ATA;
            ATAPI                     = report.ATAPI;
            CompactFlash              = report.CompactFlash;
            FireWire                  = report.FireWire;
            LastSynchronized          = DateTime.UtcNow;
            MultiMediaCard            = report.MultiMediaCard;
            PCMCIA                    = report.PCMCIA;
            SCSI                      = report.SCSI;
            SecureDigital             = report.SecureDigital;
            USB                       = report.USB;
            Manufacturer              = report.Manufacturer;
            Model                     = report.Model;
            Revision                  = report.Revision;
            Type                      = report.Type;
            GdRomSwapDiscCapabilities = report.GdRomSwapDiscCapabilities;
        }

        /// <summary>When this known device was last synchronized with the server</summary>
        public DateTime LastSynchronized { get; set; }

        /// <summary>Optimal number of blocks to read at once</summary>
        [DefaultValue(0)]
        public int OptimalMultipleSectorsRead { get; set; }

        /// <summary>Can read GD-ROM using swap trick?</summary>
        [DefaultValue(null)]
        public bool? CanReadGdRomUsingSwapDisc { get; set; }
    }
}