// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aaru.Database.Models
{
    public class UsbVendor
    {
        public UsbVendor() { }

        public UsbVendor(ushort id, string vendor)
        {
            Id        = id;
            Vendor    = vendor;
            AddedWhen = ModifiedWhen = DateTime.UtcNow;
        }

        [Key]
        public ushort Id { get;          set; }
        public string   Vendor    { get; set; }
        public DateTime AddedWhen { get; set; }
        [Index]
        public DateTime ModifiedWhen { get; set; }

        public virtual ICollection<UsbProduct> Products { get; set; }
    }
}