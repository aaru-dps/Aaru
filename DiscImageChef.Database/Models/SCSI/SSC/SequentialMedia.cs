// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SequentialMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SSC tested media.
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

namespace DiscImageChef.Database.Models.SCSI.SSC
{
    public class SequentialMedia : BaseEntity
    {
        public bool?                  CanReadMediaSerial  { get; set; }
        public byte?                  Density             { get; set; }
        public string                 Manufacturer        { get; set; }
        public bool                   MediaIsRecognized   { get; set; }
        public byte?                  MediumType          { get; set; }
        public string                 MediumTypeName      { get; set; }
        public string                 Model               { get; set; }
        public List<SupportedDensity> SupportedDensities  { get; set; }
        public List<SupportedMedia>   SupportedMediaTypes { get; set; }
        public byte[]                 ModeSense6Data      { get; set; }
        public byte[]                 ModeSense10Data     { get; set; }

        public static SequentialMedia MapSequentialMedia(CommonTypes.Metadata.SequentialMedia oldSequentialMedia)
        {
            SequentialMedia newSequentialMedia = new SequentialMedia
            {
                Manufacturer      = oldSequentialMedia.Manufacturer,
                MediaIsRecognized = oldSequentialMedia.MediaIsRecognized,
                MediumTypeName    = oldSequentialMedia.MediumTypeName,
                Model             = oldSequentialMedia.Model,
                ModeSense6Data    = oldSequentialMedia.ModeSense6Data,
                ModeSense10Data   = oldSequentialMedia.ModeSense10Data
            };

            if(oldSequentialMedia.CanReadMediaSerialSpecified)
                newSequentialMedia.CanReadMediaSerial = oldSequentialMedia.CanReadMediaSerial;
            if(oldSequentialMedia.DensitySpecified) newSequentialMedia.Density       = oldSequentialMedia.Density;
            if(oldSequentialMedia.MediumTypeSpecified) newSequentialMedia.MediumType = oldSequentialMedia.MediumType;
            if(newSequentialMedia.SupportedDensities != null)
                newSequentialMedia.SupportedDensities =
                    new List<SupportedDensity>(oldSequentialMedia.SupportedDensities.Select(SupportedDensity
                                                                                               .MapSupportedDensity));

            if(newSequentialMedia.SupportedMediaTypes == null) return newSequentialMedia;

            newSequentialMedia.SupportedMediaTypes =
                new List<SupportedMedia>(oldSequentialMedia
                                        .SupportedMediaTypes.Select(SupportedMedia.MapSupportedMedia));

            return newSequentialMedia;
        }
    }
}