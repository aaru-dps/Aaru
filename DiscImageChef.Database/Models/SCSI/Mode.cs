// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Mode.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for SCSI MODE.
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
    public class Mode : BaseEntity
    {
        public byte?                 MediumType        { get; set; }
        public bool                  WriteProtected    { get; set; }
        public List<BlockDescriptor> BlockDescriptors  { get; set; }
        public byte?                 Speed             { get; set; }
        public byte?                 BufferedMode      { get; set; }
        public bool                  BlankCheckEnabled { get; set; }
        public bool                  DPOandFUA         { get; set; }
        public List<ModePage>        ModePages         { get; set; }

        public static Mode MapMode(modeType oldMode)
        {
            if(oldMode == null) return null;

            Mode newMode = new Mode
            {
                WriteProtected    = oldMode.WriteProtected,
                BlankCheckEnabled = oldMode.BlankCheckEnabled,
                DPOandFUA         = oldMode.DPOandFUA
            };

            if(oldMode.BufferedModeSpecified) newMode.BufferedMode = oldMode.BufferedMode;
            if(oldMode.MediumTypeSpecified) newMode.MediumType     = oldMode.MediumType;
            if(oldMode.SpeedSpecified) newMode.Speed               = oldMode.Speed;

            if(oldMode.BlockDescriptors != null)
                newMode.BlockDescriptors =
                    new List<BlockDescriptor>(oldMode.BlockDescriptors.Select(BlockDescriptor.MapBlockDescriptor));

            if(oldMode.ModePages == null) return newMode;

            {
                newMode.ModePages = new List<ModePage>(oldMode.ModePages.Select(ModePage.MapModePage));
            }

            return newMode;
        }
    }
}