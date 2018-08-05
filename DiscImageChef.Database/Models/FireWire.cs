// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FireWire.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for FireWire device information.
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

using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models
{
    public class FireWire : BaseEntity
    {
        public uint   VendorID       { get; set; }
        public uint   ProductID      { get; set; }
        public string Manufacturer   { get; set; }
        public string Product        { get; set; }
        public bool   RemovableMedia { get; set; }

        public static FireWire MapFirewire(firewireType oldFirewire)
        {
            if(oldFirewire == null) return null;

            return new FireWire
            {
                Manufacturer   = oldFirewire.Manufacturer,
                Product        = oldFirewire.Product,
                ProductID      = oldFirewire.ProductID,
                RemovableMedia = oldFirewire.RemovableMedia,
                VendorID       = oldFirewire.VendorID
            };
        }
    }
}