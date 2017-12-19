// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SscTestedMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI Streaming media tests from reports.
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
                    mediaOneValue.Add(string.Format("<i>Information for medium named \"{0}\"</i>",
                                                    media.MediumTypeName));
                    if(media.MediumTypeSpecified)
                        mediaOneValue.Add(string.Format("Medium type code: {0:X2}h", media.MediumType));
                }
                else if(media.MediumTypeSpecified)
                    mediaOneValue.Add(string.Format("<i>Information for medium type {0:X2}h</i>", media.MediumType));
                else mediaOneValue.Add("<i>Information for unknown medium type</i>");

                if(!string.IsNullOrWhiteSpace(media.Manufacturer))
                    mediaOneValue.Add(string.Format("Medium manufactured by: {0}", media.Manufacturer));
                if(!string.IsNullOrWhiteSpace(media.Model))
                    mediaOneValue.Add(string.Format("Medium model: {0}", media.Model));

                if(media.DensitySpecified)
                    mediaOneValue.Add(string.Format("Medium has density code {0:X2}h", media.Density));
                if(media.CanReadMediaSerial) mediaOneValue.Add("Drive can read medium serial number.");
                if(media.MediaIsRecognized) mediaOneValue.Add("DiscImageChef recognizes this medium.");

                mediaOneValue.Add("");
            }
        }
    }
}