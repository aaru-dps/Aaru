// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SCSI MultiMedia Command devices.
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

namespace DiscImageChef.Database.Models.SCSI.MMC
{
    public class MMC : BaseEntity
    {
        public Mode2A            ModeSense2A { get; set; }
        public Features          Features    { get; set; }
        public List<TestedMedia> TestedMedia { get; set; }

        public static MMC MapMmc(mmcType oldMmc)
        {
            if(oldMmc == null) return null;

            MMC newMmc = new MMC
            {
                Features    = Features.MapFeatures(oldMmc.Features),
                ModeSense2A = Mode2A.MapMode2A(oldMmc.ModeSense2A)
            };
            if(oldMmc.TestedMedia == null) return newMmc;

            newMmc.TestedMedia = new List<TestedMedia>(oldMmc.TestedMedia.Select(Models.TestedMedia.MapTestedMedia));

            return newMmc;
        }
    }
}