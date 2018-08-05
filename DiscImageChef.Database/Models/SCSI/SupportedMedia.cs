// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SupportedMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SCSI supported media information.
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

using System.Collections.Generic;
using System.Linq;

namespace DiscImageChef.Database.Models.SCSI
{
    public class SupportedMedia : BaseEntity
    {
        public byte           MediumType   { get; set; }
        public List<IntClass> DensityCodes { get; set; }
        public ushort         Width        { get; set; }
        public ushort         Length       { get; set; }
        public string         Organization { get; set; }
        public string         Name         { get; set; }
        public string         Description  { get; set; }

        public static SupportedMedia MapSupportedMedia(CommonTypes.Metadata.SupportedMedia oldSupportedMedia)
        {
            SupportedMedia newSupportedMedia = new SupportedMedia
            {
                MediumType   = oldSupportedMedia.MediumType,
                Width        = oldSupportedMedia.Width,
                Length       = oldSupportedMedia.Length,
                Organization = oldSupportedMedia.Organization,
                Name         = oldSupportedMedia.Name,
                Description  = oldSupportedMedia.Description
            };

            if(oldSupportedMedia.DensityCodes == null) return newSupportedMedia;

            newSupportedMedia.DensityCodes =
                new List<IntClass>(oldSupportedMedia.DensityCodes.Select(t => new IntClass {Value = t}));

            return newSupportedMedia;
        }
    }
}