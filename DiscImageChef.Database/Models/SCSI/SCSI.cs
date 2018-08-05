// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SCSI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SCSI device information.
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

namespace DiscImageChef.Database.Models.SCSI
{
    public class SCSI : BaseEntity
    {
        public Inquiry           Inquiry              { get; set; }
        public List<Page>        EVPDPages            { get; set; }
        public bool              SupportsModeSense6   { get; set; }
        public bool              SupportsModeSense10  { get; set; }
        public bool              SupportsModeSubpages { get; set; }
        public Mode              ModeSense            { get; set; }
        public MMC.MMC           MultiMediaDevice     { get; set; }
        public TestedMedia       ReadCapabilities     { get; set; }
        public List<TestedMedia> RemovableMedias      { get; set; }
        public SSC.SSC           SequentialDevice     { get; set; }
        public byte[]            ModeSense6Data       { get; set; }
        public byte[]            ModeSense10Data      { get; set; }

        public static SCSI MapScsi(scsiType oldScsi)
        {
            if(oldScsi == null) return null;

            SCSI newScsi = new SCSI
            {
                Inquiry              = Inquiry.MapInquiry(oldScsi.Inquiry),
                SupportsModeSense6   = oldScsi.SupportsModeSense6,
                SupportsModeSense10  = oldScsi.SupportsModeSense10,
                SupportsModeSubpages = oldScsi.SupportsModeSubpages,
                ModeSense            = Mode.MapMode(oldScsi.ModeSense),
                MultiMediaDevice     = MMC.MMC.MapMmc(oldScsi.MultiMediaDevice),
                ReadCapabilities     = TestedMedia.MapTestedMedia(oldScsi.ReadCapabilities),
                SequentialDevice     = SSC.SSC.MapSsc(oldScsi.SequentialDevice),
                ModeSense6Data       = oldScsi.ModeSense6Data,
                ModeSense10Data      = oldScsi.ModeSense10Data
            };

            if(oldScsi.EVPDPages != null) newScsi.EVPDPages = new List<Page>(oldScsi.EVPDPages.Select(Page.MapPage));

            if(oldScsi.RemovableMedias == null) return newScsi;

            {
                newScsi.RemovableMedias =
                    new List<TestedMedia>(oldScsi.RemovableMedias.Select(TestedMedia.MapTestedMedia));
            }

            return newScsi;
        }
    }
}