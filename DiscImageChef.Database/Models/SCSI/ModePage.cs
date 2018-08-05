// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ModePage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SCSI MODE page.
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

namespace DiscImageChef.Database.Models.SCSI
{
    public class ModePage : BaseEntity
    {
        public byte   page    { get; set; }
        public byte   subpage { get; set; }
        public byte[] value   { get; set; }

        public static ModePage MapModePage(modePageType oldModePage)
        {
            return oldModePage == null
                       ? null
                       : new ModePage
                       {
                           page    = oldModePage.page,
                           subpage = oldModePage.subpage,
                           value   = oldModePage.value
                       };
        }
    }
}