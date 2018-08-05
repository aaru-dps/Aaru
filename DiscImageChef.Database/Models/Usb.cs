// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Usb.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for USB device information.
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
    public class USB : BaseEntity
    {
        public ushort VendorID       { get; set; }
        public ushort ProductID      { get; set; }
        public string Manufacturer   { get; set; }
        public string Product        { get; set; }
        public bool   RemovableMedia { get; set; }
        public byte[] Descriptors    { get; set; }

        public static USB MapUsb(usbType oldUsb)
        {
            if(oldUsb == null) return null;

            return new USB
            {
                Descriptors    = oldUsb.Descriptors,
                Manufacturer   = oldUsb.Manufacturer,
                Product        = oldUsb.Product,
                ProductID      = oldUsb.ProductID,
                RemovableMedia = oldUsb.RemovableMedia,
                VendorID       = oldUsb.VendorID
            };
        }
    }
}