// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UsbVendor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Model for storing USB vendor identifiers in database.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aaru.Database.Models
{
    /// <summary>USB vendor</summary>
    public class UsbVendor
    {
        /// <summary>Builds an empty USB vendor</summary>
        public UsbVendor() {}

        /// <summary>Builds a USB vendor with the specified parameters</summary>
        /// <param name="id">Vendor ID</param>
        /// <param name="vendor">Vendor name</param>
        public UsbVendor(ushort id, string vendor)
        {
            Id        = id;
            Vendor    = vendor;
            AddedWhen = ModifiedWhen = DateTime.UtcNow;
        }

        /// <summary>Database ID</summary>
        [Key]
        public ushort Id { get; set; }
        /// <summary>Vendor name</summary>
        public string Vendor { get; set; }
        /// <summary>Date when model has been added to the database</summary>
        public DateTime AddedWhen { get; set; }
        /// <summary>Date when model was last modified</summary>
        public DateTime ModifiedWhen { get; set; }

        /// <summary>List of products from this vendor</summary>
        public virtual ICollection<UsbProduct> Products { get; set; }
    }
}