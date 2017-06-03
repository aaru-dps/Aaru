// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SscTestedMedia.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Collections.Generic;
using DiscImageChef.Metadata;

namespace DiscImageChef.Server.App_Start
{
    public static class SscTestedMedia
    {
        public static void Report(SequentialMedia[] testedMedia, ref List<string> mediaOneValue)
        {
            foreach(SequentialMedia media in testedMedia)
            {
                if(!string.IsNullOrWhiteSpace(media.MediumTypeName))
                {
                    mediaOneValue.Add(string.Format("<i>Information for medium named \"{0}\"</i>", media.MediumTypeName));
                    if(media.MediumTypeSpecified)
                        mediaOneValue.Add(string.Format("Medium type code: {0:X2}h", media.MediumType));
                }
                else if(media.MediumTypeSpecified)
                    mediaOneValue.Add(string.Format("<i>Information for medium type {0:X2}h</i>", media.MediumType));
                else
                    mediaOneValue.Add("<i>Information for unknown medium type</i>");

                if(!string.IsNullOrWhiteSpace(media.Manufacturer))
                    mediaOneValue.Add(string.Format("Medium manufactured by: {0}", media.Manufacturer));
                if(!string.IsNullOrWhiteSpace(media.Model))
                    mediaOneValue.Add(string.Format("Medium model: {0}", media.Model));

                if(media.DensitySpecified)
                    mediaOneValue.Add(string.Format("Medium has density code {0:X2}h", media.Density));
                if(media.CanReadMediaSerial)
                    mediaOneValue.Add("Drive can read medium serial number.");
                if(media.MediaIsRecognized)
                    mediaOneValue.Add("DiscImageChef recognizes this medium.");

                mediaOneValue.Add("");
            }
        }
    }
}
