// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CdOffset.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Model for storing Compact Disc read offsets in database.
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Server.Models
{
    public class CompactDiscOffset : CdOffset
    {
        public CompactDiscOffset() { }

        public CompactDiscOffset(string manufacturer, string model, short offset, int submissions, float agreement)
        {
            Manufacturer = manufacturer;
            Model        = model;
            Offset       = offset;
            Submissions  = submissions;
            Agreement    = agreement;
            AddedWhen    = ModifiedWhen = DateTime.UtcNow;
        }

        public CompactDiscOffset(CdOffset offset)
        {
            Manufacturer = offset.Manufacturer;
            Model        = offset.Model;
            Offset       = offset.Offset;
            Submissions  = offset.Submissions;
            Agreement    = offset.Agreement;
            AddedWhen    = ModifiedWhen = DateTime.UtcNow;
        }

        public int      Id        { get; set; }
        public DateTime AddedWhen { get; set; }
        [Index]
        public DateTime ModifiedWhen { get;               set; }
        public virtual ICollection<Device> Devices { get; set; }
    }
}