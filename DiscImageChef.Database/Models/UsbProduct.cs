// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscImageChef.Database.Models
{
    public class UsbProduct
    {
        public UsbProduct() { }

        public UsbProduct(ushort vendorId, ushort id, string product)
        {
            ProductId = id;
            Product   = product;
            AddedWhen = ModifiedWhen = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        [Index]
        public ushort ProductId { get;   set; }
        public string   Product   { get; set; }
        public DateTime AddedWhen { get; set; }
        [Index]
        public DateTime ModifiedWhen { get; set; }
        [Index]
        public ushort VendorId { get;          set; }
        public virtual UsbVendor Vendor { get; set; }
    }
}