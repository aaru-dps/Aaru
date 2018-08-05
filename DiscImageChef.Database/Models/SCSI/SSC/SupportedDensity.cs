// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SupportedDensity.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SSC supported density.
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

namespace DiscImageChef.Database.Models.SCSI.SSC
{
    public class SupportedDensity : BaseEntity
    {
        public byte   PrimaryCode    { get; set; }
        public byte   SecondaryCode  { get; set; }
        public bool   Writable       { get; set; }
        public bool   Duplicate      { get; set; }
        public bool   DefaultDensity { get; set; }
        public uint   BitsPerMm      { get; set; }
        public ushort Width          { get; set; }
        public ushort Tracks         { get; set; }
        public uint   Capacity       { get; set; }
        public string Organization   { get; set; }
        public string Name           { get; set; }
        public string Description    { get; set; }

        public static SupportedDensity MapSupportedDensity(CommonTypes.Metadata.SupportedDensity oldDensity)
        {
            return new SupportedDensity
            {
                PrimaryCode    = oldDensity.PrimaryCode,
                SecondaryCode  = oldDensity.SecondaryCode,
                Writable       = oldDensity.Writable,
                Duplicate      = oldDensity.Duplicate,
                DefaultDensity = oldDensity.DefaultDensity,
                BitsPerMm      = oldDensity.BitsPerMm,
                Width          = oldDensity.Width,
                Tracks         = oldDensity.Tracks,
                Capacity       = oldDensity.Capacity,
                Organization   = oldDensity.Organization,
                Name           = oldDensity.Name,
                Description    = oldDensity.Description
            };
        }
    }
}