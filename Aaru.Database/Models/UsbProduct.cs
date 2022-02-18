// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UsbProduct.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Model for storing USB product identifiers in database.
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
using System.ComponentModel.DataAnnotations;

namespace Aaru.Database.Models
{
    /// <summary>USB product</summary>
    public class UsbProduct
    {
        /// <summary>Builds an empty USB product</summary>
        public UsbProduct() {}

        /// <summary>Builds a USB product with the specified parameters</summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="id">Product ID</param>
        /// <param name="product">Product name</param>
        public UsbProduct(ushort vendorId, ushort id, string product)
        {
            VendorId  = vendorId;
            ProductId = id;
            Product   = product;
            AddedWhen = ModifiedWhen = DateTime.UtcNow;
        }

        /// <summary>Database ID</summary>
        [Key]
        public int Id { get; set; }
        /// <summary>Product ID</summary>
        public ushort ProductId { get; set; }
        /// <summary>Product name</summary>
        public string Product { get; set; }
        /// <summary>Date when model has been added to the database</summary>
        public DateTime AddedWhen { get; set; }
        /// <summary>Date when model was last modified</summary>
        public DateTime ModifiedWhen { get; set; }
        /// <summary>USB vendor ID</summary>
        public ushort VendorId { get; set; }
        /// <summary>Database link to USB vendor</summary>
        public virtual UsbVendor Vendor { get; set; }
    }
}