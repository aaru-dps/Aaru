// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SSC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SCSI Streaming Command devices.
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
using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Database.Models.SCSI.SSC
{
    public class SSC : BaseEntity
    {
        public byte?                  BlockSizeGranularity { get; set; }
        public uint?                  MaxBlockLength       { get; set; }
        public uint?                  MinBlockLength       { get; set; }
        public List<SupportedDensity> SupportedDensities   { get; set; }
        public List<SupportedMedia>   SupportedMediaTypes  { get; set; }
        public List<SequentialMedia>  TestedMedia          { get; set; }

        public static SSC MapSsc(sscType oldSsc)
        {
            if(oldSsc == null) return null;

            SSC newSsc = new SSC();

            if(oldSsc.BlockSizeGranularitySpecified) newSsc.BlockSizeGranularity = oldSsc.BlockSizeGranularity;
            if(oldSsc.MaxBlockLengthSpecified) newSsc.MaxBlockLength             = oldSsc.MaxBlockLength;
            if(oldSsc.MinBlockLengthSpecified) newSsc.MinBlockLength             = oldSsc.MinBlockLength;

            if(oldSsc.SupportedDensities != null)
                newSsc.SupportedDensities =
                    new List<SupportedDensity>(oldSsc.SupportedDensities.Select(SupportedDensity.MapSupportedDensity));

            if(oldSsc.SupportedMediaTypes != null)
                newSsc.SupportedMediaTypes =
                    new List<SupportedMedia>(oldSsc.SupportedMediaTypes.Select(SupportedMedia.MapSupportedMedia));

            if(oldSsc.TestedMedia == null) return newSsc;

            newSsc.TestedMedia =
                new List<SequentialMedia>(oldSsc.TestedMedia.Select(SequentialMedia.MapSequentialMedia));

            return newSsc;
        }
    }
}